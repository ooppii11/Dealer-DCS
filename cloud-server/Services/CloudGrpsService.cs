using Grpc.Core;
using GrpcCloud;
using cloud_server.Managers;
using System.Linq.Expressions;
using Google.Protobuf;

namespace cloud_server.Services
{   
    public class CloudGrpsService: Cloud.CloudBase
    {
        private Authentication _authManager;
        private FilesManager _filesManager;
        private readonly ILogger<CloudGrpsService> _logger;

        public CloudGrpsService(ILogger<CloudGrpsService> logger, Authentication auth, FilesManager filesManager)
        {
            this._logger = logger;
            this._authManager = auth; 
            this._filesManager = filesManager; 
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
                // Send Error response:
                return Task.FromResult(new SignupResponse
                {
                    Status = GrpcCloud.Status.Failure,
                    Message = ex.Message
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
                // Send Error response:
                return Task.FromResult(new LoginResponse
                {
                    Status = GrpcCloud.Status.Failure,
                    SessionId = ex.Message
                });

            }
        }
      
        public override Task<LogoutResponse> logout(LogoutRequest request, ServerCallContext context)
        {
            this._authManager.Logout(request.SessionId);
            return Task.FromResult(new LogoutResponse());
        }

        public override Task<GetListOfFilesResponse> getListOfFiles(GetListOfFilesRequest request, ServerCallContext context)
        {
            GetListOfFilesResponse response = new GetListOfFilesResponse();
            try
            {
                User user = this._authManager.GetUser(request.SessionId); // Check if the user conncted
                List<GrpcCloud.FileMetadata> fileMetadata = this._filesManager.getFiles(user.Id); // Get the metadata
                
                // Init response:
                response.Message = "";
                response.Status = GrpcCloud.Status.Success;
                response.Files.Add(fileMetadata);

                return Task.FromResult(response);
            }
            catch (Exception ex)
            {
                response.Message = ex.Message;
                response.Status = GrpcCloud.Status.Failure;
                return Task.FromResult(response);
            }
        }
        public override Task<GetFileMetadataResponse> getFileMetadata(GetFileMetadataRequest request, ServerCallContext context)
        {
            GetFileMetadataResponse response = new GetFileMetadataResponse();
            try 
            {
                User user = this._authManager.GetUser(request.SessionId); // Check if the user conncted
                
                response.Message = "";
                response.Status = GrpcCloud.Status.Success;
                response.File = this._filesManager.getFile(user.Id, request.FileName);
                return Task.FromResult(response);
            }
            catch (Exception ex)
            {
                response.Message = ex.Message;
                response.Status = GrpcCloud.Status.Failure;
                return Task.FromResult(response);
            }
        }
        public override Task<DeleteFileResponse> DeleteFile(DeleteFileRequest request, ServerCallContext context)
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
            catch (Exception ex)
            {
                return Task.FromResult(new DeleteFileResponse
                { 
                    Status = GrpcCloud.Status.Failure,
                    Message = $"Error deleting the file: {ex.Message}" 
                });
            }

        }

        public override async Task DownloadFile(DownloadFileRequest request, IServerStreamWriter<DownloadFileResponse> responseStream, ServerCallContext context)
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
                        offset += chunkSize;
                        await responseStream.WriteAsync(response);
                    }
                }
            }
            catch (Exception ex)
            {
                await responseStream.WriteAsync(new DownloadFileResponse { Status = GrpcCloud.Status.Failure, Message = $"Error downloading the file: {ex.Message}", FileData = ByteString.Empty });
                return;
            }

        }

        public override async Task<UploadFileResponse> UploadFile(IAsyncStreamReader<UploadFileRequest> requestStream, ServerCallContext context)
        {
            try
            {
                User user = null;
                bool isFirstIteration = true;

                string fileName = "";
                string type = "";
                string SecondReplicationPlace = "";
                string ThirdReplicationPlace = "";
                MemoryStream fileData = new MemoryStream();

                await foreach (var chunk in requestStream.ReadAllAsync())
                {
                    if (isFirstIteration)
                    {
                        user = this._authManager.GetUser(chunk.SessionId);
                        fileName = chunk.FileName;
                        type = chunk.Type;
                    }

                    fileData.Write(chunk.FileData.ToArray(), 0, chunk.FileData.Length);
                }

                this._filesManager.uploadFile(user.Id, fileName, type, fileData.Length, fileData.ToArray());
                //consensus + S2S

                return new UploadFileResponse()
                { 
                    Status = GrpcCloud.Status.Success,
                    Message = "File uploaded successfully." 
                };
            }
            catch (Exception ex)
            {
                return new UploadFileResponse()
                {
                    Status = GrpcCloud.Status.Failure,
                    Message = $"Error uploading file: {ex.Message}" 
                };
            }
        }

    }
}
