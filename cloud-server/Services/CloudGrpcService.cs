using Grpc.Core;
using GrpcCloud;
using cloud_server.Managers;
using Google.Protobuf;
using System.Collections.Concurrent;
using cloud_server.Utilities;
using cloud_server.DB;
using System.Threading.Tasks;

namespace cloud_server.Services
{   
    public class CloudGrpcService: Cloud.CloudBase
    {
        private Authentication _authManager;
        private FilesManager _filesManager;
        private readonly ConcurrentQueue<(Delegate Action, TaskCompletionSource<object> Completion, object[] Parameters)> _requestQueue = new ConcurrentQueue<(Delegate Action, TaskCompletionSource<object> Completion, object[] Parameters)>();
        private static readonly object _queueLockObject = new object();
        private readonly ManualResetEventSlim _queueEvent = new ManualResetEventSlim(false);
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
            Task.Factory.StartNew(ProcessQueue, TaskCreationOptions.LongRunning);
        }

        private Task<object> EnqueueRequestAsync(Delegate action, params object[] parameters)
        {
            var completionSource = new TaskCompletionSource<object>();
            this._requestQueue.Enqueue((action, completionSource, parameters));
            this._queueEvent.Set();
            return completionSource.Task;
        }


        private async Task ProcessQueue()
        {
            while (true)
            {
                this._queueEvent.Wait();

                lock (CloudGrpcService._queueLockObject)
                {
                    try
                    {
                        while (this._requestQueue.TryDequeue(out var requestPair))
                        {
                            lock (CloudGrpcService._fileLock)
                            {
                                CloudGrpcService._raftLogger.getCurrLeaderAddress();
                            }
                            try
                            {
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
                            }
                            catch (Exception ex)
                            {
                                requestPair.Completion.TrySetException(ex);
                            }
                        }
                    }
                    catch (NoLeaderException ex)
                    {
                        Console.WriteLine(ex.Message);
                    }
                    this._queueEvent.Reset();
                }
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
                        this._queueEvent.Set();
                        this._filesManager.LeaderAddress = request.LeaderAddress;
                        return Task.FromResult(new LeaderToViewerHeartBeatResponse { Status = true });
                    }
                }
                catch (NoLeaderException ex)
                {
                    CloudGrpcService._raftLogger.insertEntry(request);
                    this._queueEvent.Set();
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
            return Task.FromResult(new LogoutResponse());
        }

        public override async Task<GetListOfFilesResponse> getListOfFiles(GetListOfFilesRequest request, ServerCallContext context)
        {
            var task = await EnqueueRequestAsync(ProcessGetListOfFiles, request, context);
            return (GetListOfFilesResponse)task;
        }

        public override async Task<GetFileMetadataResponse> getFileMetadata(GetFileMetadataRequest request, ServerCallContext context)
        {
            var task = await EnqueueRequestAsync(ProcessGetFileMetadata, request, context);
            return (GetFileMetadataResponse) task;
        }

        public override async Task<DeleteFileResponse> DeleteFile(DeleteFileRequest request, ServerCallContext context)
        {
            var task = await EnqueueRequestAsync(ProcessDeleteFile, request, context);
            return (DeleteFileResponse)task;
        }

        public override Task DownloadFile(DownloadFileRequest request, IServerStreamWriter<DownloadFileResponse> responseStream, ServerCallContext context)
        {
            var task = EnqueueRequestAsync(ProcessDownloadFile, request, responseStream, context);
            return task;
        }

        public override async Task<UploadFileResponse> UploadFile(IAsyncStreamReader<UploadFileRequest> requestStream, ServerCallContext context)
        {
            var task = await EnqueueRequestAsync(ProcessUploadFile, requestStream, context);
            return  (UploadFileResponse)task;
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