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
using System.Runtime.CompilerServices;
//using GrpcNodeServer;

namespace cloud_server.Services
{   
    public class CloudGrpcService: Cloud.CloudBase
    {
        private Authentication _authManager;
        static private FilesManager _filesManager;

        private readonly RaftViewerLogger _raftLogger;
        private static readonly object _fileLock = new object();

        //private readonly ConcurrentQueue<(Delegate Action, TaskCompletionSource<object> Completion, object[] Parameters)> _requestQueue = new ConcurrentQueue<(Delegate Action, TaskCompletionSource<object> Completion, object[] Parameters)>();
        private static readonly Dictionary<string, ConcurrentQueue<(Delegate Action, TaskCompletionSource<object> Completion, object[] Parameters)>> _usersRequestQueues = new Dictionary<string, ConcurrentQueue<(Delegate Action, TaskCompletionSource<object> Completion, object[] Parameters)>>();
        private static readonly Dictionary<string, object> _usersQueueLockObjects = new Dictionary<string, object>();
        private static readonly Dictionary<string, CancellationTokenSource> _usersCancellationTokens = new Dictionary<string, CancellationTokenSource>();
        private static readonly Dictionary<string, bool> _usersLoggedInStatus = new Dictionary<string, bool>();
        //private readonly object _queueLockObject = new object();
        //private readonly ManualResetEventSlim _queueEvent = new ManualResetEventSlim(false);
        private static readonly Dictionary<string, ManualResetEventSlim> _usersQueueEvent = new Dictionary<string, ManualResetEventSlim>();
        


        private readonly ILogger<CloudGrpcService> _logger;

        public CloudGrpcService(ILogger<CloudGrpcService> logger, Authentication auth, FileMetadataDB filesManagerDB, RaftViewerLogger raftLogger)
        {
            _raftLogger = raftLogger;
            try
            {
                CloudGrpcService._filesManager = new FilesManager(filesManagerDB, this._raftLogger.getCurrLeaderAddress());
            }
            catch (NoLeaderException ex)
            {
                CloudGrpcService._filesManager = new FilesManager(filesManagerDB);
            }
            this._logger = logger;
            this._authManager = auth;
        }
        private void StartProcessQueueForUser(string userSessionId)
        {
            if (!CloudGrpcService._usersCancellationTokens.ContainsKey(userSessionId))
            {
                var cancellationTokenSource = new CancellationTokenSource();
                CloudGrpcService._usersCancellationTokens[userSessionId] = cancellationTokenSource;

                var userQueueLockObject = new object();
                CloudGrpcService._usersQueueLockObjects[userSessionId] = userQueueLockObject;

                var userQueueEvent = new ManualResetEventSlim(false);
                CloudGrpcService._usersQueueEvent[userSessionId] = userQueueEvent;

                CloudGrpcService._usersRequestQueues[userSessionId] = new ConcurrentQueue<(Delegate Action, TaskCompletionSource<object> Completion, object[] Parameters)>();


                Task.Factory.StartNew(() => ProcessQueue(userSessionId), TaskCreationOptions.LongRunning);
            }
        }

        private void StopProcessQueueForUser(string userSessionId)
        {
            
            if (CloudGrpcService._usersCancellationTokens.TryGetValue(userSessionId, out var cancellationTokenSource))
            {
                cancellationTokenSource.Cancel();
                CloudGrpcService._usersCancellationTokens.Remove(userSessionId);
            }
            if (CloudGrpcService._usersQueueLockObjects.TryGetValue(userSessionId, out var userQueueLockObject))
            {
                lock (userQueueLockObject)
                {
                    CloudGrpcService._usersQueueLockObjects.Remove(userSessionId);
                }
            }
            if (CloudGrpcService._usersRequestQueues.TryGetValue(userSessionId, out var userQueue))
            {
                _usersRequestQueues.Remove(userSessionId);
            }
        }


        private Task<object> EnqueueRequestAsync(string userSessionId, Delegate action, params object[] parameters)
        {
            if (!_usersLoggedInStatus.ContainsKey(userSessionId) || !_usersLoggedInStatus[userSessionId])
            {
                throw new UserIsNotLoggedIn("User is not logged in.");
            }
            var completionSource = new TaskCompletionSource<object>();
            lock (CloudGrpcService._usersQueueLockObjects[userSessionId])
            {
                CloudGrpcService._usersRequestQueues[userSessionId].Enqueue((action, completionSource, parameters));
                CloudGrpcService._usersQueueEvent[userSessionId].Set();
            }
            return completionSource.Task;
        }



        private async Task ProcessQueue(string userSessionId)
        {
            while (!CloudGrpcService._usersCancellationTokens[userSessionId].IsCancellationRequested)
            {
                CloudGrpcService._usersQueueEvent[userSessionId].Wait();

                lock (CloudGrpcService._usersQueueLockObjects[userSessionId])
                {
                    bool stopProcessingQueueNow = false;
                    while (CloudGrpcService._usersRequestQueues[userSessionId].TryPeek(out var requestPair) && !stopProcessingQueueNow)
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
                                    if (taskResult.IsFaulted && taskResult.Exception.InnerException is RpcException)
                                    {
                                        //Console.WriteLine(taskResult.Exception.InnerException);
                                        stopProcessingQueueNow = true;
                                        lock (CloudGrpcService._fileLock)
                                        {
                                            this._raftLogger.insertInvalidLeader();
                                        }
                                    }
                                    else if (taskResult.IsFaulted)
                                    {
                                        requestPair.Completion.TrySetException(taskResult.Exception.InnerException);
                                    }
                                    else
                                    {
                                        requestPair.Completion.TrySetResult(taskResult.GetType().GetProperty("Result")?.GetValue(taskResult));
                                    }
                                }
                                else
                                {
                                    var continuationFinishedEvent = new ManualResetEvent(false);

                                    taskResult.ContinueWith(t =>
                                    {
                                        try
                                        {
                                            if (t.IsFaulted && t.Exception.InnerException is RpcException)
                                            {
                                                //Console.WriteLine(t.Exception.InnerException);
                                                stopProcessingQueueNow = true;
                                                lock (CloudGrpcService._fileLock)
                                                {
                                                    this._raftLogger.insertInvalidLeader();
                                                }
                                            }
                                            else if (t.IsFaulted)
                                            {
                                                requestPair.Completion.TrySetException(t.Exception.InnerException);
                                            }
                                            else
                                            {
                                                requestPair.Completion.TrySetResult(t.GetType().GetProperty("Result")?.GetValue(t));
                                            }
                                        }
                                        finally
                                        {
                                            continuationFinishedEvent.Set();
                                        }
                                    }, TaskScheduler.Default);

                                    continuationFinishedEvent.WaitOne();
                                }
                            }
                            else
                            {
                                requestPair.Completion.TrySetResult(result);
                            }
                            if (!stopProcessingQueueNow)
                            {
                                CloudGrpcService._usersRequestQueues[userSessionId].TryDequeue(out requestPair);
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
                    CloudGrpcService._usersQueueEvent[userSessionId].Reset();
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
                    if (isNewLeader)
                    {
                        this._raftLogger.insertEntry(request);
                        CloudGrpcService._filesManager._leaderAddress = request.LeaderAddress;
                        this.setAllUsersEvents();
                        return Task.FromResult(new LeaderToViewerHeartBeatResponse { Status = true });
                    }
                }
                catch (NoLeaderException ex)
                {
                    this._raftLogger.insertEntry(request);
                    CloudGrpcService._filesManager._leaderAddress = request.LeaderAddress;
                    this.setAllUsersEvents();
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
        private void UpdateDictionariesOnSessionChange(string oldSessionId, string newSessionId)
        {
            lock (_fileLock)
            {
                if (_usersRequestQueues.TryGetValue(oldSessionId, out var oldQueue))
                {
                    _usersRequestQueues.Remove(oldSessionId);
                    _usersRequestQueues[newSessionId] = oldQueue;
                }

                if (_usersQueueLockObjects.TryGetValue(oldSessionId, out var oldLockObject))
                {
                    _usersQueueLockObjects.Remove(oldSessionId);
                    _usersQueueLockObjects[newSessionId] = oldLockObject;
                }

                if (_usersQueueEvent.TryGetValue(oldSessionId, out var oldQueueEvent))
                {
                    _usersQueueEvent.Remove(oldSessionId);
                    _usersQueueEvent[newSessionId] = oldQueueEvent;
                }

                if (_usersCancellationTokens.TryGetValue(oldSessionId, out var oldCancellationToken))
                {
                    _usersCancellationTokens[oldSessionId].Cancel();
                    _usersCancellationTokens.Remove(oldSessionId);
                    _usersCancellationTokens[newSessionId] = oldCancellationToken;
                }

                if (_usersLoggedInStatus.TryGetValue(oldSessionId, out var isLoggedIn))
                {
                    _usersLoggedInStatus.Remove(oldSessionId);
                    _usersLoggedInStatus[newSessionId] = isLoggedIn;
                }

                Task.Factory.StartNew(() => ProcessQueue(newSessionId), TaskCreationOptions.LongRunning);
            }
        }

        public override Task<LoginResponse> login(LoginRequest request, ServerCallContext context)
        {
            string newSessionId = "";
            string existingSessionId = "";

            try
            {
                var oldSessionIdAndNew = this._authManager.Login(request.Username, request.Password);
                newSessionId = oldSessionIdAndNew.Item1;
                existingSessionId = oldSessionIdAndNew.Item2;
                if (!_usersCancellationTokens.ContainsKey(newSessionId))
                {
                    if (existingSessionId == null || !_usersCancellationTokens.ContainsKey(existingSessionId))
                    {
                        StartProcessQueueForUser(newSessionId);
                    }
                    else 
                    {
                        UpdateDictionariesOnSessionChange(existingSessionId, newSessionId);
                    }
                }

                
                CloudGrpcService._usersLoggedInStatus[newSessionId] = true;
                return Task.FromResult(new LoginResponse { SessionId = newSessionId, Status = GrpcCloud.Status.Success });

            }
            catch (AuthenticationException ex)
            {
                context.Status = new Grpc.Core.Status(StatusCode.PermissionDenied, ex.Message);
                return Task.FromResult(new LoginResponse { SessionId = $"Error: {ex.Message}", Status = GrpcCloud.Status.Failure });
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
        private async Task WaitForQueueEmpty(string userSessionId)
        {
            while (CloudGrpcService._usersRequestQueues[userSessionId].Count > 0)
            {
                // Wait for a short duration before checking again
                await Task.Delay(100); // You can adjust the delay time as needed
            }
        }
        public override async Task<LogoutResponse> logout(LogoutRequest request, ServerCallContext context)
        {
            try
            {
                this._authManager.Logout(request.SessionId);
                CloudGrpcService._usersLoggedInStatus[request.SessionId] = false;
                await WaitForQueueEmpty(request.SessionId);

                StopProcessQueueForUser(request.SessionId);

                return new LogoutResponse();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return new LogoutResponse();
            }
        }

        public override async Task<GetListOfFilesResponse> getListOfFiles(GetListOfFilesRequest request, ServerCallContext context)
        {
            try
            {
                this._authManager.CheckSessionId(request.SessionId);
                var task = await EnqueueRequestAsync(request.SessionId, ProcessGetListOfFiles, request, context);
                return (GetListOfFilesResponse)task;
            }
            catch (AuthenticationException ex)
            {
                context.Status = new Grpc.Core.Status(StatusCode.PermissionDenied, ex.Message);
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
            catch (AuthenticationException ex)
            {
                context.Status = new Grpc.Core.Status(StatusCode.PermissionDenied, ex.Message);
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
            catch (AuthenticationException ex)
            {
                context.Status = new Grpc.Core.Status(StatusCode.PermissionDenied, ex.Message);
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
                var task = await EnqueueRequestAsync(request.SessionId, ProcessDownloadFile, request, responseStream, context);
                return;
            }
            catch (AuthenticationException ex)
            {
                context.Status = new Grpc.Core.Status(StatusCode.PermissionDenied, ex.Message);
                await responseStream.WriteAsync(new DownloadFileResponse { Status = GrpcCloud.Status.Failure, Message = $"Error: {ex.Message}", FileData = ByteString.Empty });
                return;
            }
            //catch
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                context.Status = new Grpc.Core.Status(StatusCode.Internal, "Internal Error downloading the file.");
                await responseStream.WriteAsync(new DownloadFileResponse { Status = GrpcCloud.Status.Failure, Message = $"Internal Error downloading the file.", FileData = ByteString.Empty });
                return;
            }
        }
        public override async Task<UpdateFileResponse> UpdateFile(IAsyncStreamReader<UpdateFileRequest> requestStream, ServerCallContext context)
        {
            try
            {
                //List<UpdateFileRequest> requestStreamList = await GetStreamInfoAsList<UpdateFileRequest>(requestStream);
                if (await requestStream.MoveNext())
                {
                    UpdateFileRequest firstRequest = requestStream.Current;
                    string sessionId = firstRequest.SessionId;

                    this._authManager.GetUser(sessionId);

                    var task = await EnqueueRequestAsync(sessionId, ProcessUpdateFile, requestStream, firstRequest, context);

                    return (UpdateFileResponse)task;
                }
                else
                {
                    context.Status = new Grpc.Core.Status(StatusCode.InvalidArgument, "Empty request stream");
                    return new UpdateFileResponse { Message = "Empty request stream", Status = GrpcCloud.Status.Failure };
                }
            }
            catch (AuthenticationException ex)
            {
                context.Status = new Grpc.Core.Status(StatusCode.PermissionDenied, ex.Message);
                return new UpdateFileResponse { Message = $"Error: {ex.Message}", Status = GrpcCloud.Status.Failure };
            }
            catch (RpcException ex)
            {
                Console.WriteLine(ex.Message);
                context.Status = new Grpc.Core.Status(StatusCode.Aborted, "Error: connection failed");
                return new UpdateFileResponse { Message = $"Error: connection failed", Status = GrpcCloud.Status.Failure };
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                context.Status = new Grpc.Core.Status(StatusCode.Internal, "Internal Error uploading file.");
                return new UpdateFileResponse()
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
                //List<UploadFileRequest> requestStreamList = await GetStreamInfoAsList<UploadFileRequest>(requestStream);
                if (await requestStream.MoveNext())
                {
                    UploadFileRequest firstRequest = requestStream.Current;
                    string sessionId = firstRequest.SessionId;

                    this._authManager.GetUser(sessionId);

                    var task = await EnqueueRequestAsync(sessionId, ProcessUploadFile, requestStream, firstRequest, context);

                    return (UploadFileResponse)task;
                }
                else
                {
                    context.Status = new Grpc.Core.Status(StatusCode.InvalidArgument, "Empty request stream");
                    return new UploadFileResponse { Message = "Empty request stream", Status = GrpcCloud.Status.Failure };
                }

            }
            catch (AuthenticationException ex)
            {
                context.Status = new Grpc.Core.Status(StatusCode.PermissionDenied, ex.Message);
                return new UploadFileResponse { Message = $"Error: {ex.Message}", Status = GrpcCloud.Status.Failure };
            }
            catch (RpcException ex)
            {
                Console.WriteLine(ex.Message);
                context.Status = new Grpc.Core.Status(StatusCode.Aborted, "Error: connection failed");
                return new UploadFileResponse { Message = $"Error: connection failed", Status = GrpcCloud.Status.Failure };
            }
            catch (Npgsql.PostgresException ex)
            {
                context.Status = new Grpc.Core.Status(StatusCode.AlreadyExists, "File with this name already exist for the user.");
                return new UploadFileResponse { Message = "File with this name already exist for the user.", Status = GrpcCloud.Status.Failure };
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
            List<GrpcCloud.FileMetadata> fileMetadata = CloudGrpcService._filesManager.getFilesMetadata(user.Id); // Get the metadata

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
            response.File = CloudGrpcService._filesManager.getFileMetadata(user.Id, request.FileName);
            return Task.FromResult(response);
        }
        private Task<DeleteFileResponse> ProcessDeleteFile(DeleteFileRequest request, ServerCallContext context)
        {
            User user = this._authManager.GetUser(request.SessionId); // Check if the user conncted

            CloudGrpcService._filesManager.deleteFile(user.Id, request.FileName);
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

            byte[] file = await CloudGrpcService._filesManager.downloadFile(user.Id, request.FileName);
            int offset = 0;
            int chunkSize = 64000;
            if (file != null)
            {
                while (offset < file.Length)
                {
                    int remaining = file.Length - offset;
                    int writingSize = Math.Min(remaining, chunkSize);

                    DownloadFileResponse response = new DownloadFileResponse { Status = GrpcCloud.Status.Success, FileData = ByteString.CopyFrom(file, offset, writingSize) };
                    offset += writingSize;
                    await responseStream.WriteAsync(response);
                }
            }
        }

        private async Task<UploadFileResponse> ProcessUploadFile(IAsyncStreamReader<UploadFileRequest> requestStream, UploadFileRequest fileRequest, ServerCallContext context)
        {
            User user = this._authManager.GetUser(fileRequest.SessionId);

            string fileName = fileRequest.FileName;
            string type = fileRequest.Type;

            MemoryStream fileData = new MemoryStream();
            fileData.Write(fileRequest.FileData.ToByteArray(), 0, fileRequest.FileData.Length);

            await foreach (var chunk in requestStream.ReadAllAsync())
            {
                fileData.Write(chunk.FileData.ToArray(), 0, chunk.FileData.Length);
            }
            await CloudGrpcService._filesManager.uploadFile(user.Id, fileName, type, fileData.Length, fileData.ToArray());

            return new UploadFileResponse()
            {
                Status = GrpcCloud.Status.Success,
                Message = "File uploaded successfully."
            };
        }

        private async Task<UpdateFileResponse> ProcessUpdateFile(IAsyncStreamReader<UpdateFileRequest> requestStream, UpdateFileRequest fileRequest, ServerCallContext context)
        {
            User user = this._authManager.GetUser(fileRequest.SessionId);

            string fileName = fileRequest.FileName; 

            MemoryStream fileData = new MemoryStream();
            fileData.Write(fileRequest.FileData.ToByteArray(), 0, fileRequest.FileData.Length);
            await foreach (var chunk in requestStream.ReadAllAsync())
            {
                fileData.Write(chunk.FileData.ToArray(), 0, chunk.FileData.Length);
            }

            await CloudGrpcService._filesManager.updateFile(user.Id, fileName, fileData.Length, fileData.ToArray());

            return new UpdateFileResponse()
            {
                Status = GrpcCloud.Status.Success,
                Message = "File uploaded successfully."
            };
        }

    }
}