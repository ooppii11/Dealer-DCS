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

namespace cloud_server.Services
{   
    public class CloudGrpcService: Cloud.CloudBase
    {
        private Authentication _authManager;
        private FilesManager _filesManager;
        //private readonly ConcurrentQueue<(Delegate Action, TaskCompletionSource<object> Completion, object[] Parameters)> _requestQueue = new ConcurrentQueue<(Delegate Action, TaskCompletionSource<object> Completion, object[] Parameters)>();
        private readonly Dictionary<string, ConcurrentQueue<(Delegate Action, TaskCompletionSource<object> Completion, object[] Parameters)>> _userRequestQueues = new Dictionary<string, ConcurrentQueue<(Delegate Action, TaskCompletionSource<object> Completion, object[] Parameters)>>();
        private readonly Dictionary<string, object> _userQueueLockObjects = new Dictionary<string, object>();
        private readonly Dictionary<string, CancellationTokenSource> _userCancellationTokens = new Dictionary<string, CancellationTokenSource>();
        //private readonly object _queueLockObject = new object();
        //private readonly ManualResetEventSlim _queueEvent = new ManualResetEventSlim(false);
        private readonly Dictionary<string, ManualResetEventSlim> _usersQueueEvent = new Dictionary<string, ManualResetEventSlim>();
        private static readonly RaftViewerLogger _raftLogger = new RaftViewerLogger("LeaderLog.log");
        private static readonly object _fileLock = new object();


        private readonly ILogger<CloudGrpcService> _logger;

        public CloudGrpcService(ILogger<CloudGrpcService> logger, Authentication auth, FileMetadataDB filesManagerDB)
        {
            try
            {
                this._filesManager = new FilesManager(filesManagerDB, CloudGrpcService._raftLogger.getCurrLeaderAddress());
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
            if (!this._userCancellationTokens.ContainsKey(userId))
            {
                var cancellationTokenSource = new CancellationTokenSource();
                this._userCancellationTokens[userId] = cancellationTokenSource;

                var userQueueLockObject = new object();
                this._userQueueLockObjects[userId] = userQueueLockObject;

                var userQueueEvent = new ManualResetEventSlim(false);
                this._usersQueueEvent[userId] = userQueueEvent;

                if (!this._userRequestQueues.TryGetValue(userId, out var userQueue))
                {
                    userQueue = new ConcurrentQueue<(Delegate Action, TaskCompletionSource<object> Completion, object[] Parameters)>();
                    this._userRequestQueues[userId] = userQueue;
                }

                Task.Factory.StartNew(() => ProcessQueue(userId), TaskCreationOptions.LongRunning);
            }
        }

        private void StopProcessQueueForUser(string userId)
        {
            if (this._userCancellationTokens.TryGetValue(userId, out var cancellationTokenSource))
            {
                cancellationTokenSource.Cancel();
                this._userCancellationTokens.Remove(userId);

                if (this._userQueueLockObjects.TryGetValue(userId, out var userQueueLockObject))
                {
                    lock (userQueueLockObject)
                    {
                        this._userQueueLockObjects.Remove(userId);
                    }
                }
            }
        }


        private Task<object> EnqueueRequestAsync(string userId, Delegate action, params object[] parameters)
        {
            var completionSource = new TaskCompletionSource<object>();
            this._userRequestQueues[userId].Enqueue((action, completionSource, parameters));
            this._usersQueueEvent[userId].Set();
            return completionSource.Task;
        }



        private async Task ProcessQueue(string userId)
        {
            while (!this._userCancellationTokens[userId].IsCancellationRequested)
            {
                this._usersQueueEvent[userId].Wait();

                lock (this._userQueueLockObjects[userId])
                {
                    
                    while (this._userRequestQueues[userId].TryPeek(out var requestPair))
                    {
                        try
                        {
                            lock (CloudGrpcService._fileLock)
                            {
                                CloudGrpcService._raftLogger.getCurrLeaderAddress();
                            }
                            var result = requestPair.Action.DynamicInvoke(requestPair.Parameters);
                            if (result is Task taskResult)
                            {
                                if (taskResult.IsCompleted)
                                {
                                    // If the task is already completed, set the result directly
                                    requestPair.Completion.TrySetResult(taskResult.GetType().GetProperty("Result")?.GetValue(taskResult));
                                }
                                else
                                {
                                    // If the task is not completed, continue with setting the result
                                    taskResult.ContinueWith(t =>
                                    {
                                        requestPair.Completion.TrySetResult(t.GetType().GetProperty("Result")?.GetValue(t));
                                    }, TaskScheduler.Default);
                                }
                            }
                            else
                            {
                                requestPair.Completion.TrySetResult(result);
                            }
                            this._userRequestQueues[userId].TryDequeue(out requestPair);
                        }
                        catch (NoLeaderException ex)
                        {
                            Console.WriteLine(ex.Message);
                            break;
                        }
                        catch (Exception ex)
                        {
                            requestPair.Completion.TrySetException(ex);
                        }
                            
                    }
                    this._usersQueueEvent[userId].Reset();
                }
            }
        }

        private void setAllUsersEvents()
        {
            foreach (var pair in this._usersQueueEvent)
            {
                pair.Value.Set();
            }
        }

        public override Task<LeaderToViewerHeartBeatResponse> GetOrUpdateSystemLeader(LeaderToViewerHeartBeatRequest request, ServerCallContext context)
        {
            lock (CloudGrpcService._fileLock)
            {
                try
                {
                    bool isNewLeader = request.LeaderAddress != CloudGrpcService._raftLogger.getCurrLeaderAddress();
                    //bool isValidTerm = CloudGrpcService._raftLogger.getLastEntry().Term <= request.Term;
                    bool isValidIndex = CloudGrpcService._raftLogger.getLastEntry().SystemLastIndex <= request.SystemLastIndex;
                    if (isNewLeader
                        //&& isValidTerm
                        && isValidIndex)
                    {
                        CloudGrpcService._raftLogger.insertEntry(request);
                        this.setAllUsersEvents();
                        this._filesManager.LeaderAddress = request.LeaderAddress;
                        return Task.FromResult(new LeaderToViewerHeartBeatResponse { Status = true });
                    }
                }
                catch (NoLeaderException ex)
                {
                    CloudGrpcService._raftLogger.insertEntry(request);
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
                context.Status = new Grpc.Core.Status(StatusCode.Internal, ex.Message);
                // Send Error response:
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
                sessionId = this._authManager.Login(request.Username, request.Password);
                StartProcessQueueForUser(sessionId);
                return Task.FromResult(new LoginResponse { SessionId = sessionId, Status = GrpcCloud.Status.Success });


            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                context.Status = new Grpc.Core.Status(StatusCode.Internal, ex.Message);
                // Send Error response:
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
                this._authManager.GetUser(request.SessionId);
                var task = await EnqueueRequestAsync(request.SessionId, ProcessGetListOfFiles, request, context);
                return (GetListOfFilesResponse)task;
            }
            catch (IncorrectSessionIdException ex)
            {
                return new GetListOfFilesResponse { Message = $"Error: {ex.Message}", Status = GrpcCloud.Status.Failure };
            }
            
        }

        public override async Task<GetFileMetadataResponse> getFileMetadata(GetFileMetadataRequest request, ServerCallContext context)
        {
            try 
            {
                this._authManager.GetUser(request.SessionId);
                var task = await EnqueueRequestAsync(request.SessionId, ProcessGetFileMetadata, request, context);
                return (GetFileMetadataResponse)task;
            }
            catch (IncorrectSessionIdException ex)
            {
                return new GetFileMetadataResponse { Message = $"Error: {ex.Message}" , Status = GrpcCloud.Status.Failure};
            }
        }

        public override async Task<DeleteFileResponse> DeleteFile(DeleteFileRequest request, ServerCallContext context)
        {
            try
            {
                this._authManager.GetUser(request.SessionId);
                var task = await EnqueueRequestAsync(request.SessionId, ProcessDeleteFile, request, context);
                return (DeleteFileResponse)task;
            }
            catch (IncorrectSessionIdException ex)
            {
                return new DeleteFileResponse { Message = $"Error: {ex.Message}", Status = GrpcCloud.Status.Failure };
            }
            
        }

        public override async Task DownloadFile(DownloadFileRequest request, IServerStreamWriter<DownloadFileResponse> responseStream, ServerCallContext context)
        {
            try
            {
                this._authManager.GetUser(request.SessionId);
                var task = EnqueueRequestAsync(request.SessionId, ProcessDownloadFile, request, responseStream, context);
                return;
            }
            catch (IncorrectSessionIdException ex)
            {
                await responseStream.WriteAsync(new DownloadFileResponse { Status = GrpcCloud.Status.Failure, Message = $"Error {ex.Message}", FileData = ByteString.Empty });
                return;
            }
            
        }

        public override async Task<UploadFileResponse> UploadFile(IAsyncStreamReader<UploadFileRequest> requestStream, ServerCallContext context)
        {
            try
            {
                this._authManager.GetUser(requestStream.Current.SessionId);
                var task = await EnqueueRequestAsync(requestStream.Current.SessionId, ProcessUploadFile, requestStream, context);
                return (UploadFileResponse)task;
            }
            catch (IncorrectSessionIdException ex)
            {
                return new UploadFileResponse { Message = $"Error: {ex.Message}", Status = GrpcCloud.Status.Failure };
            }
            
        }

        private Task<GetListOfFilesResponse> ProcessGetListOfFiles(GetListOfFilesRequest request, ServerCallContext context)
        {
            GetListOfFilesResponse response = new GetListOfFilesResponse();
            try
            {
                User user = this._authManager.GetUser(request.SessionId); // Check if the user conncted
                List<GrpcCloud.FileMetadata> fileMetadata = this._filesManager.getFilesMetadata(user.Id); // Get the metadata

                // Init response:
                response.Message = "";
                response.Status = GrpcCloud.Status.Success;
                response.Files.Add(fileMetadata);

                return Task.FromResult(response);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                context.Status = new Grpc.Core.Status(StatusCode.Internal, ex.Message);
                response.Message = ex.Message;
                response.Status = GrpcCloud.Status.Failure;
                return Task.FromResult(response);
            }
        }
        private Task<GetFileMetadataResponse> ProcessGetFileMetadata(GetFileMetadataRequest request, ServerCallContext context)
        {
            GetFileMetadataResponse response = new GetFileMetadataResponse();
            try 
            {
                User user = this._authManager.GetUser(request.SessionId); // Check if the user conncted
                
                response.Message = "";
                response.Status = GrpcCloud.Status.Success;
                response.File = this._filesManager.getFileMetadata(user.Id, request.FileName);
                return Task.FromResult(response);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                context.Status = new Grpc.Core.Status(StatusCode.Internal, ex.Message);
                response.Message = ex.Message;
                response.Status = GrpcCloud.Status.Failure;
                return Task.FromResult(response);
            }
        }
        private Task<DeleteFileResponse> ProcessDeleteFile(DeleteFileRequest request, ServerCallContext context)
        {
            try
            {
                User user = this._authManager.GetUser(request.SessionId); // Check if the user conncted

                this._filesManager.deleteFile(user.Id, request.FileName);
                return Task.FromResult(new DeleteFileResponse 
                {
                    Status = GrpcCloud.Status.Success,
                    Message = "" }
                );
            }
            catch (RpcException ex)
            {
                Console.WriteLine(ex.Message);
                lock (CloudGrpcService._fileLock)
                {
                    CloudGrpcService._raftLogger.insertInvalidLeader();
                }   
                context.Status = new Grpc.Core.Status(StatusCode.Unavailable, ex.Message);
                return Task.FromResult(new DeleteFileResponse
                {
                    Status = GrpcCloud.Status.Failure,
                    Message = $"Error deleting the file: {ex.Message}"
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                context.Status = new Grpc.Core.Status(StatusCode.Internal, ex.Message);
                return Task.FromResult(new DeleteFileResponse
                { 
                    Status = GrpcCloud.Status.Failure,
                    Message = $"Internal Error deleting the file." 
                });
            }

        }
        private async Task ProcessDownloadFile(DownloadFileRequest request, IServerStreamWriter<DownloadFileResponse> responseStream, ServerCallContext context)
        {
            try
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
            catch (RpcException ex)
            {
                Console.WriteLine(ex.Message);
                lock (CloudGrpcService._fileLock)
                {
                    CloudGrpcService._raftLogger.insertInvalidLeader();
                }
                context.Status = new Grpc.Core.Status(StatusCode.Unavailable, ex.Message);
                await responseStream.WriteAsync(new DownloadFileResponse { Status = GrpcCloud.Status.Failure, Message = $"Error downloading the file: {ex.Message}", FileData = ByteString.Empty });
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                context.Status = new Grpc.Core.Status(StatusCode.Internal, ex.Message);
                await responseStream.WriteAsync(new DownloadFileResponse { Status = GrpcCloud.Status.Failure, Message = $"Internal Error downloading the file.", FileData = ByteString.Empty });
                return;
            }

        }

        private async Task<UploadFileResponse> ProcessUploadFile(IAsyncStreamReader<UploadFileRequest> requestStream, ServerCallContext context)
        {
            try
            {
                User user = null;
                bool isFirstIteration = true;

                string fileName = "";
                string type = "";
                
                MemoryStream fileData = new MemoryStream();

                await foreach (var chunk in requestStream.ReadAllAsync())
                {
                    if (isFirstIteration)
                    {
                        user = this._authManager.GetUser(chunk.SessionId);
                        fileName = chunk.FileName;
                        type = chunk.Type;
                    }
                    //
                    isFirstIteration = false;

                    fileData.Write(chunk.FileData.ToArray(), 0, chunk.FileData.Length);
                }

                await this._filesManager.uploadFile(user.Id, fileName, type, fileData.Length, fileData.ToArray());

                return new UploadFileResponse()
                { 
                    Status = GrpcCloud.Status.Success,
                    Message = "File uploaded successfully." 
                };
            }
            catch (RpcException ex)
            {
                Console.WriteLine(ex.Message);
                lock (CloudGrpcService._fileLock)
                {
                    CloudGrpcService._raftLogger.insertInvalidLeader();
                }
                context.Status = new Grpc.Core.Status(StatusCode.Unavailable, ex.Message);
                return new UploadFileResponse
                {
                    Status = GrpcCloud.Status.Failure,
                    Message = $"Error uploading the file: {ex.Message}"
                };
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                context.Status = new Grpc.Core.Status(StatusCode.Internal, ex.Message);
                return new UploadFileResponse()
                {
                    Status = GrpcCloud.Status.Failure,
                    Message = $"Internal Error uploading file." 
                };
            }
        }

    }
}