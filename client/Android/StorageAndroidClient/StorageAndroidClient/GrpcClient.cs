using Grpc.Core;
using Grpc;
using GrpcCloud;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.IO;

namespace StorageAndroidClient
{
    public class GrpcClient
    {
        private readonly Channel _channel;
        private Cloud.CloudClient _client;
        private const int MaxFileChunckLength = 31457;

        public GrpcClient(string host, int port)
        {
            this._channel = new Channel(host, port, ChannelCredentials.Insecure);
            this._client = new Cloud.CloudClient(this._channel);
        }

        public GrpcClient(string address)
        {
            this._channel = new Channel(address, ChannelCredentials.Insecure);
            this._client = new Cloud.CloudClient(this._channel);
        }

        public SignupResponse Signup(string username, string email, string password, string phoneNumber)
        {
            var request = new SignupRequest { Username = username, Email = email, Password = password, PhoneNumber = phoneNumber };
            var response = this._client.signup(request);
            return response;
        }

        public async Task<SignupResponse> SignupAsync(string username, string email, string password, string phoneNumber)
        {
            var request = new SignupRequest { Username = username, Email = email, Password = password, PhoneNumber = phoneNumber };
            var response = await this._client.signupAsync(request);
            return response;
        }

        public LoginResponse Login(string username, string password)
        {
            var request = new LoginRequest { Username = username, Password = password };
            var response = this._client.login(request);
            return response;
        }

        public async Task<LoginResponse> loginAsync(string username, string password)
        {
            var request = new LoginRequest { Username = username, Password = password };
            var response = await this._client.loginAsync(request);
            return response;
        }

        public LogoutResponse Logout(string sessionId)
        {
            var request = new LogoutRequest { SessionId = sessionId };
            var response = this._client.logout(request);
            return response;
        }

        public GetListOfFilesResponse GetFiles(string sessionId)
        {
            var request = new GetListOfFilesRequest { SessionId = sessionId };
            var response = this._client.getListOfFiles(request);
            return response;
        }

        public async Task<UploadFileResponse> UploadFile(string fileName, string sessionId, byte[] fileData, string fileType)
        {
            List<UploadFileRequest> requests = createUploadRequests(fileName, sessionId, fileData, fileType);

            var call = this._client.UploadFile();

            foreach (var request in requests)
            {
                await call.RequestStream.WriteAsync(request);
            }

            await call.RequestStream.CompleteAsync();

            var response = await call.ResponseAsync;
            return response;
        }

        private List<UploadFileRequest> createUploadRequests(string name, string sessionId, byte[] fileData, string type)
        {
            List<UploadFileRequest> uploadFileRequests = null;
            byte[] chunk = null;
            UploadFileRequest request = null;
            int numberOfChunks = 0;
            int chunkSize = GrpcClient.MaxFileChunckLength;
            int i, j = 0;

            uploadFileRequests = new List<UploadFileRequest>();
            numberOfChunks = fileData.Length / chunkSize + ((fileData.Length % chunkSize == 0) ? 0 : 1);

            for (i = 0; i < numberOfChunks; i++)
            {
                if ((i + 1) * chunkSize < fileData.Length) { chunk = new byte[chunkSize]; }
                else { chunk = new byte[fileData.Length % ((i + 1) * chunkSize)]; }

                for (j = 0; j < chunkSize && j + i * chunkSize < fileData.Length; j++)
                {
                    chunk[j] = fileData[i * chunkSize + j];
                }

                request = new UploadFileRequest()
                {
                    FileName = name,
                    SessionId = sessionId,
                    Type = type,
                    FileData = Google.Protobuf.ByteString.CopyFrom(chunk)
                };
                uploadFileRequests.Add(request);
            }

            return uploadFileRequests;
        }

        public async Task<byte[]> DownloadFile(string fileName, string sessionId)
        {
            DownloadFileRequest request = new DownloadFileRequest { SessionId = sessionId, FileName = fileName };

            // Download the file:
            using (var call = this._client.DownloadFile(request))
            {
                using (var memoryStream = new MemoryStream())
                {
                    // append chuncks to memoryStream
                    while (await call.ResponseStream.MoveNext())
                    {
                        var chunk = call.ResponseStream.Current.FileData;
                        await memoryStream.WriteAsync(chunk.ToByteArray(), 0, chunk.Length);
                    }
                    return memoryStream.ToArray();
                }
            }
        }

        public async Task<UpdateFileResponse> updateFile(string fileName, string sessionId, byte[] fileData)
        {
            List<UpdateFileRequest> requests = createUpdateRequests(fileName, sessionId, fileData);

            var call = this._client.UpdateFile();

            // For evry chunk of file call to upload 
            foreach (var request in requests)
            {
                await call.RequestStream.WriteAsync(request);
            }

            // Wait until all request are send
            await call.RequestStream.CompleteAsync();

            // Wait for response
            var response = await call.ResponseAsync;
            return response;
        }

        private List<UpdateFileRequest> createUpdateRequests(string fileName, string sessionId, byte[] fileData)
        {
            List<UpdateFileRequest> updateFileRequests = null;
            byte[] chunk = null;
            UpdateFileRequest request = null;
            int numberOfChunks = 0;
            int chunkSize = GrpcClient.MaxFileChunckLength;
            int i, j = 0;

            updateFileRequests = new List<UpdateFileRequest>();
            numberOfChunks = fileData.Length / chunkSize + ((fileData.Length % chunkSize == 0) ? 0 : 1);

            for (i = 0; i < numberOfChunks; i++)
            {
                if ((i + 1) * chunkSize < fileData.Length) { chunk = new byte[chunkSize]; }
                else { chunk = new byte[fileData.Length % ((i + 1) * chunkSize)]; }

                for (j = 0; j < chunkSize && j + i * chunkSize < fileData.Length; j++)
                {
                    chunk[j] = fileData[i * chunkSize + j];
                }


                request = new UpdateFileRequest()
                {
                    FileName = fileName,
                    SessionId = sessionId,
                    FileData = Google.Protobuf.ByteString.CopyFrom(chunk),

                };

                updateFileRequests.Add(request);
            }

            return updateFileRequests;
        }
    }
}