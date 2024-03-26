using Grpc.Core;
using GrpcCloud;
using cloud_server.Managers;
using Google.Protobuf;
using System.Collections.Concurrent;
using cloud_server.Utilities;
using cloud_server.DB;
using System.Threading.Tasks;
using System.Net;
using System.Linq.Expressions;
using GrpcNodeServer;

namespace cloud_server.Services
{   
    public class CloudGrpcService: Cloud.CloudBase
    {
        private Authentication _authManager;
        private FilesManager _filesManager;

        private readonly RaftViewerLogger _raftLogger;
        private static readonly object _fileLock = new object();

        //private readonly ConcurrentQueue<(Delegate Action, TaskCompletionSource<object> Completion, object[] Parameters)> _requestQueue = new ConcurrentQueue<(Delegate Action, TaskCompletionSource<object> Completion, object[] Parameters)>();
        private static readonly Dictionary<string, ConcurrentQueue<(Delegate Action, TaskCompletionSource<object> Completion, object[] Parameters)>> _usersRequestQueues = new Dictionary<string, ConcurrentQueue<(Delegate Action, TaskCompletionSource<object> Completion, object[] Parameters)>>();
        private static readonly Dictionary<string, object> _usersQueueLockObjects = new Dictionary<string, object>();
        private static readonly Dictionary<string, CancellationTokenSource> _usersCancellationTokens = new Dictionary<string, CancellationTokenSource>();
        //private readonly object _queueLockObject = new object();
        //private readonly ManualResetEventSlim _queueEvent = new ManualResetEventSlim(false);
        private static readonly Dictionary<string, ManualResetEventSlim> _usersQueueEvent = new Dictionary<string, ManualResetEventSlim>();
        


        private readonly ILogger<CloudGrpcService> _logger;

        public CloudGrpcService(ILogger<CloudGrpcService> logger, Authentication auth, FileMetadataDB filesManagerDB, RaftViewerLogger raftLogger)
        {
            _raftLogger = raftLogger;
            try
            {
                this._filesManager = new FilesManager(filesManagerDB, this._raftLogger.getCurrLeaderAddress());
            }
            catch (NoLeaderException ex)
            {
                this._filesManager = new FilesManager(filesManagerDB);
            }
            this._logger = logger;
            this._authManager = auth;
        }
        private void StartProcessQueueForUser(string userId)
        {
            if (!CloudGrpcService._usersCancellationTokens.ContainsKey(userId))
            {
                var cancellationTokenSource = new CancellationTokenSource();
                CloudGrpcService._usersCancellationTokens[userId] = cancellationTokenSource;

                var userQueueLockObject = new object();
                CloudGrpcService._usersQueueLockObjects[userId] = userQueueLockObject;

                var userQueueEvent = new ManualResetEventSlim(false);
                CloudGrpcService._usersQueueEvent[userId] = userQueueEvent;

                CloudGrpcService._usersRequestQueues[userId] = new ConcurrentQueue<(Delegate Action, TaskCompletionSource<object> Completion, object[] Parameters)>();


                Task.Factory.StartNew(() => ProcessQueue(userId), TaskCreationOptions.LongRunning);
            }
        }

        private void StopProcessQueueForUser(string userId)
        {
            if (CloudGrpcService._usersCancellationTokens.TryGetValue(userId, out var cancellationTokenSource))
            {
                cancellationTokenSource.Cancel();
                CloudGrpcService._usersCancellationTokens.Remove(userId);
            }
            if (CloudGrpcService._usersQueueLockObjects.TryGetValue(userId, out var userQueueLockObject))
            {
                lock (userQueueLockObject)
                {
                    CloudGrpcService._usersQueueLockObjects.Remove(userId);
                }
            }
            if (CloudGrpcService._usersRequestQueues.TryGetValue(userId, out var userQueue))
            {
                _usersRequestQueues.Remove(userId);
            }
        }


        private Task<object> EnqueueRequestAsync(string userId, Delegate action, params object[] parameters)
        {
            var completionSource = new TaskCompletionSource<object>();
            CloudGrpcService._usersRequestQueues[userId].Enqueue((action, completionSource, parameters));
            CloudGrpcService._usersQueueEvent[userId].Set();
            return completionSource.Task;
        }



        private async Task ProcessQueue(string userId)
        {
            while (!CloudGrpcService._usersCancellationTokens[userId].IsCancellationRequested)
            {
                CloudGrpcService._usersQueueEvent[userId].Wait();

                lock (CloudGrpcService._usersQueueLockObjects[userId])
                {
                    bool stopProcessingQueueNow = false;
                    while (CloudGrpcService._usersRequestQueues[userId].TryPeek(out var requestPair) && !stopProcessingQueueNow)
                    {
                        try
                        {
                            lock (CloudGrpcService._fileLock)
                            {
                                this._raftLogger.getCurrLeaderAddress();
                            }
                            var result = requestPair.Action.DynamicInvoke(requestPair.Parameters);
                            if (result is Task taskResult)
                            {
                                if (taskResult.IsCompleted)
                                {
                                    requestPair.Completion.TrySetResult(taskResult.GetType().GetProperty("Result")?.GetValue(taskResult));
                                }
                                
                                else
                                {
                                    taskResult.ContinueWith(t =>
                                    {
                                        if (t.IsFaulted && t.Exception.InnerException is RpcException)
                                        {
                                            stopProcessingQueueNow = true;
                                            lock (CloudGrpcService._fileLock)
                                            {
                                                this._raftLogger.insertInvalidLeader();
                                            }
                                        }
                                        else
                                        {
                                            requestPair.Completion.TrySetResult(t.GetType().GetProperty("Result")?.GetValue(t));
                                        }   
                                    }, TaskScheduler.Default);
                                }
                            }
                            else
                            {
                                requestPair.Completion.TrySetResult(result);
                            }
                            if (!stopProcessingQueueNow)
                            {
                                CloudGrpcService._usersRequestQueues[userId].TryDequeue(out requestPair);
                            }
                        }
                        catch (NoLeaderException ex)
                        {
                            Console.WriteLine(ex.Message);
                            stopProcessingQueueNow = true;
                        }
                        catch (Exception ex)
                        {
                            requestPair.Completion.TrySetException(ex);
                        }
                            
                    }
                    CloudGrpcService._usersQueueEvent[userId].Reset();
                }
            }
        }

        private void setAllUsersEvents()
        {
            foreach (var pair in CloudGrpcService._usersQueueEvent)
            {
                if (!pair.Value.IsSet)
                {
                    pair.Value.Set();
                }
            }
        }

        public override Task<LeaderToViewerHeartBeatResponse> GetOrUpdateSystemLeader(LeaderToViewerHeartBeatRequest request, ServerCallContext context)
        {
            lock (CloudGrpcService._fileLock)
            {
                try
                {
                    bool isNewLeader = request.LeaderAddress != this._raftLogger.getCurrLeaderAddress();
                    //bool isValidTerm = CloudGrpcService._raftLogger.getLastEntry().Term <= request.Term;
                    bool isValidIndex = this._raftLogger.getLastEntry().SystemLastIndex <= request.SystemLastIndex;
                    if (isNewLeader
                        //&& isValidTerm
                        && isValidIndex)
                    {
                        this._raftLogger.insertEntry(request);
                        this.setAllUsersEvents();
                        this._filesManager.LeaderAddress = request.LeaderAddress;
                        return Task.FromResult(new LeaderToViewerHeartBeatResponse { Status = true });
                    }
                }
                catch (NoLeaderException ex)
                {
                    this._raftLogger.insertEntry(request);
                    this.setAllUsersEvents();
                    this._filesManager.LeaderAddress = request.LeaderAddress;
                    return Task.FromResult(new LeaderToViewerHeartBeatResponse { Status = true });
                }

                return Task.FromResult(new LeaderToViewerHeartBeatResponse { Status = false });
            }
        }
        public override Task<SignupResponse> signup(SignupRequest request, ServerCallContext context)
        {
            try
            {
                this._authManager.Signup(request.Username, request.Password, request.Email ,(request.PhoneNumber != "")? request.PhoneNumber : "NULL");
                // Send Response:
                return Task.FromResult(new SignupResponse
                {
                    Status = GrpcCloud.Status.Success,
                    Message = ""
                });

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                // Send Error response:
                context.Status = new Grpc.Core.Status(StatusCode.Internal, "Internal Error.");
                return Task.FromResult(new SignupResponse
                {
                    Status = GrpcCloud.Status.Failure,
                    Message = "Internal Error"
                });

            }
        }

        public override Task<LoginResponse> login(LoginRequest request, ServerCallContext context)
        {
            string sessionId = "";

            try
            {
                sessionId = this._authManager.Login(request.Username, request.Password).Item1;
                StartProcessQueueForUser(sessionId);
                return Task.FromResult(new LoginResponse { SessionId = sessionId, Status = GrpcCloud.Status.Success });

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);

                // Send Error response:
                context.Status = new Grpc.Core.Status(StatusCode.Internal, "Internal Error");
                return Task.FromResult(new LoginResponse
                {
                    Status = GrpcCloud.Status.Failure,
                    SessionId = "Internal Error"
                });

            }
        }
      
        public override Task<LogoutResponse> logout(LogoutRequest request, ServerCallContext context)
        {
            this._authManager.Logout(request.SessionId);
            StopProcessQueueForUser(request.SessionId);
            return Task.FromResult(new LogoutResponse());
        }

        public override async Task<GetListOfFilesResponse> getListOfFiles(GetListOfFilesRequest request, ServerCallContext context)
        {
            try
            {
                this._authManager.CheckSessionId(request.SessionId);
                var task = await EnqueueRequestAsync(request.SessionId, ProcessGetListOfFiles, request, context);
                return (GetListOfFilesResponse)task;
            }
            catch (IncorrectSessionIdException ex)
            {
                context.Status = new Grpc.Core.Status(StatusCode.InvalidArgument, ex.Message);
                return new GetListOfFilesResponse { Message = $"Error: {ex.Message}", Status = GrpcCloud.Status.Failure };
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                context.Status = new Grpc.Core.Status(StatusCode.Internal, "Internal Error while getting file metadata.");
                GetListOfFilesResponse response = new GetListOfFilesResponse();
                response.Message = "Internal Error while getting List Of files metadata.";
                response.Status = GrpcCloud.Status.Failure;
                return response;
            }

        }

        public override async Task<GetFileMetadataResponse> getFileMetadata(GetFileMetadataRequest request, ServerCallContext context)
        {
            try 
            {
                this._authManager.CheckSessionId(request.SessionId);
                var task = await EnqueueRequestAsync(request.SessionId, ProcessGetFileMetadata, request, context);
                return (GetFileMetadataResponse)task;
            }
            catch (IncorrectSessionIdException ex)
            {
                return new GetFileMetadataResponse { Message = $"Error: {ex.Message}" , Status = GrpcCloud.Status.Failure};
            }
            catch (Exception ex)
            {
                GetFileMetadataResponse response = new GetFileMetadataResponse();
                Console.WriteLine(ex.Message);
                context.Status = new Grpc.Core.Status(StatusCode.Internal, "Internal Error while getting file metadata.");
                response.Message = "Internal Error while getting file metadata.";
                response.Status = GrpcCloud.Status.Failure;
                return response;
            }
        }

        public override async Task<DeleteFileResponse> DeleteFile(DeleteFileRequest request, ServerCallContext context)
        {
            try
            {
                this._authManager.CheckSessionId(request.SessionId);
                var task = await EnqueueRequestAsync(request.SessionId, ProcessDeleteFile, request, context);
                return (DeleteFileResponse)task;
            }
            catch (IncorrectSessionIdException ex)
            {
                context.Status = new Grpc.Core.Status(StatusCode.InvalidArgument, ex.Message);
                return new DeleteFileResponse { Message = $"Error: {ex.Message}", Status = GrpcCloud.Status.Failure };
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                context.Status = new Grpc.Core.Status(StatusCode.Internal, "Internal Error deleting the file.");
                return new DeleteFileResponse
                {
                    Status = GrpcCloud.Status.Failure,
                    Message = $"Internal Error deleting the file."
                };
            }

        }

        public override async Task DownloadFile(DownloadFileRequest request, IServerStreamWriter<DownloadFileResponse> responseStream, ServerCallContext context)
        {
            try
            {
                this._authManager.CheckSessionId(request.SessionId);
                var task = EnqueueRequestAsync(request.SessionId, ProcessDownloadFile, request, responseStream, context);
                return;
            }
            catch (IncorrectSessionIdException ex)
            {
                context.Status = new Grpc.Core.Status(StatusCode.InvalidArgument, ex.Message);
                await responseStream.WriteAsync(new DownloadFileResponse { Status = GrpcCloud.Status.Failure, Message = $"Error: {ex.Message}", FileData = ByteString.Empty });
                return;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                context.Status = new Grpc.Core.Status(StatusCode.Internal, "Internal Error downloading the file.");
                await responseStream.WriteAsync(new DownloadFileResponse { Status = GrpcCloud.Status.Failure, Message = $"Internal Error downloading the file.", FileData = ByteString.Empty });
                return;
            }
        }

        public override async Task<UploadFileResponse> UpdateFile(IAsyncStreamReader<UploadFileRequest> requestStream, ServerCallContext context)
        {
            try
            {
                List<UploadFileRequest> requestStreamList = await GetStreamInfoAsList<UploadFileRequest>(requestStream);
                this._authManager.GetUser(requestStreamList[0].SessionId);
                var task = await EnqueueRequestAsync(requestStreamList[0].SessionId, ProcessUpdateFile, requestStreamList, context);
                return (UploadFileResponse)task;
            }
            catch (IncorrectSessionIdException ex)
            {
                context.Status = new Grpc.Core.Status(StatusCode.InvalidArgument, ex.Message);
                return new UploadFileResponse { Message = $"Error: {ex.Message}", Status = GrpcCloud.Status.Failure };
            }
            catch (RpcException ex)
            {
                Console.WriteLine(ex.Message);
                context.Status = new Grpc.Core.Status(StatusCode.Aborted, "Error: connection failed");
                return new UploadFileResponse { Message = $"Error: connection failed", Status = GrpcCloud.Status.Failure };
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                context.Status = new Grpc.Core.Status(StatusCode.Internal, "Internal Error uploading file.");
                return new UploadFileResponse()
                {
                    Status = GrpcCloud.Status.Failure,
                    Message = $"Internal Error uploading file."
                };
            }
            }
        public override async Task<UploadFileResponse> UploadFile(IAsyncStreamReader<UploadFileRequest> requestStream, ServerCallContext context)
        {
            try
            {
                List<UploadFileRequest> requestStreamList = await GetStreamInfoAsList<UploadFileRequest>(requestStream);
                this._authManager.GetUser(requestStreamList[0].SessionId);
                var task = await EnqueueRequestAsync(requestStreamList[0].SessionId, ProcessUploadFile, requestStreamList, context);
                return (UploadFileResponse)task;
            }
            catch (IncorrectSessionIdException ex)
            {
                context.Status = new Grpc.Core.Status(StatusCode.InvalidArgument, ex.Message);
                return new UploadFileResponse { Message = $"Error: {ex.Message}", Status = GrpcCloud.Status.Failure };
            }
            catch (RpcException ex)
            {
                Console.WriteLine(ex.Message);
                context.Status = new Grpc.Core.Status(StatusCode.Aborted, "Error: connection failed");
                return new UploadFileResponse { Message = $"Error: connection failed", Status = GrpcCloud.Status.Failure };
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                context.Status = new Grpc.Core.Status(StatusCode.Internal, "Internal Error uploading file.");
                return new UploadFileResponse()
                {
                    Status = GrpcCloud.Status.Failure,
                    Message = $"Internal Error uploading file."
                };
            }
        }

        private async Task<List<T>> GetStreamInfoAsList<T>(IAsyncStreamReader<T> requestStream)
        {
            List<T> result = new List<T>();
            await foreach (var request in requestStream.ReadAllAsync())
            {
                result.Add(request);
            }
            return result;
        }

        private Task<GetListOfFilesResponse> ProcessGetListOfFiles(GetListOfFilesRequest request, ServerCallContext context)
        {
            GetListOfFilesResponse response = new GetListOfFilesResponse();
            User user = this._authManager.GetUser(request.SessionId); // Check if the user conncted
            List<GrpcCloud.FileMetadata> fileMetadata = this._filesManager.getFilesMetadata(user.Id); // Get the metadata

            // Init response:
            response.Message = "";
            response.Status = GrpcCloud.Status.Success;
            response.Files.Add(fileMetadata);

            return Task.FromResult(response);
        }

        private Task<GetFileMetadataResponse> ProcessGetFileMetadata(GetFileMetadataRequest request, ServerCallContext context)
        {
            GetFileMetadataResponse response = new GetFileMetadataResponse();
            User user = this._authManager.GetUser(request.SessionId); // Check if the user conncted

            response.Message = "";
            response.Status = GrpcCloud.Status.Success;
            response.File = this._filesManager.getFileMetadata(user.Id, request.FileName);
            return Task.FromResult(response);
        }
        private Task<DeleteFileResponse> ProcessDeleteFile(DeleteFileRequest request, ServerCallContext context)
        {
            User user = this._authManager.GetUser(request.SessionId); // Check if the user conncted

            this._filesManager.deleteFile(user.Id, request.FileName);
            return Task.FromResult(new DeleteFileResponse
            {
                Status = GrpcCloud.Status.Success,
                Message = ""
            }
            );
        }
        private async Task ProcessDownloadFile(DownloadFileRequest request, IServerStreamWriter<DownloadFileResponse> responseStream, ServerCallContext context)
        {
            User user = this._authManager.GetUser(request.SessionId); // Check if the user conncted

            byte[] file = await this._filesManager.downloadFile(user.Id, request.FileName);
            int offset = 0;
            int chunkSize = 64000;
            if (file != null)
            {
                while (offset < file.Length)
                {
                    int remaining = file.Length - offset;
                    int writingSize = Math.Min(remaining, chunkSize);

                    DownloadFileResponse response = new DownloadFileResponse { Status = GrpcCloud.Status.Success, FileData = ByteString.CopyFrom(file, offset, writingSize) };
                    //
                    offset += writingSize;
                    await responseStream.WriteAsync(response);
                }
            }
        }

        private async Task<UploadFileResponse> ProcessUploadFile(List<UploadFileRequest> requestStreamList, ServerCallContext context)
        {
            User user = this._authManager.GetUser(requestStreamList[0].SessionId);

            string fileName = requestStreamList[0].FileName;
            string type = requestStreamList[0].Type;

            MemoryStream fileData = new MemoryStream();

            foreach (var chunk in requestStreamList)
            {
                fileData.Write(chunk.FileData.ToArray(), 0, chunk.FileData.Length);
            }

            await this._filesManager.uploadFile(user.Id, fileName, type, fileData.Length, fileData.ToArray());

            return new UploadFileResponse()
            {
                Status = GrpcCloud.Status.Success,
                Message = "File uploaded successfully."
            };
        }

        private async Task<UploadFileResponse> ProcessUpdateFile(List<UploadFileRequest> requestStreamList, ServerCallContext context)
        {
            User user = this._authManager.GetUser(requestStreamList[0].SessionId);

            string fileName = requestStreamList[0].FileName;
            string type = requestStreamList[0].Type;

            MemoryStream fileData = new MemoryStream();

            foreach (var chunk in requestStreamList)
            {
                fileData.Write(chunk.FileData.ToArray(), 0, chunk.FileData.Length);
            }

            await this._filesManager.updateFile(user.Id, fileName, type, fileData.Length, fileData.ToArray());

            return new UploadFileResponse()
            {
                Status = GrpcCloud.Status.Success,
                Message = "File uploaded successfully."
            };
        }

    }
}