using Grpc.Core;
using Grpc;
using GrpcCloud;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.IO;
using Google.Protobuf;
using System;

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

        ~GrpcClient()
        {
            _channel.ShutdownAsync().Wait();
        }
        public async Task ShutdownAsync()
        {
            await _channel.ShutdownAsync();
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
            List<UploadFileRequest> requests = CreateRequests<UploadFileRequest>(fileName, sessionId, fileData, fileType);

            var call = this._client.UploadFile();

            foreach (var request in requests)
            {
                await call.RequestStream.WriteAsync(request);
            }

            await call.RequestStream.CompleteAsync();

            var response = await call.ResponseAsync;
            return response;
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

        public async Task<UpdateFileResponse> UpdateFile(string fileName, string sessionId, byte[] fileData)
        {
            List<UpdateFileRequest> requests = CreateRequests<UpdateFileRequest>(fileName, sessionId, fileData);

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

        private List<T> CreateRequests<T>(string fileName, string sessionId, byte[] fileData, string type = null) where T : IMessage<T>, new()
        {
            var requests = new List<T>();
            int chunkSize = GrpcClient.MaxFileChunckLength;
            int numberOfChunks = fileData.Length / chunkSize + (fileData.Length % chunkSize == 0 ? 0 : 1);

            for (int i = 0; i < numberOfChunks; i++)
            {
                int currentChunkSize = Math.Min(chunkSize, fileData.Length - i * chunkSize);
                byte[] chunk = new byte[currentChunkSize];
                Array.Copy(fileData, i * chunkSize, chunk, 0, currentChunkSize);

                var request = new T();
                if (request is UploadFileRequest uploadRequest)
                {
                    uploadRequest.FileName = fileName;
                    uploadRequest.SessionId = sessionId;
                    uploadRequest.FileData = Google.Protobuf.ByteString.CopyFrom(chunk);
                    uploadRequest.Type = type;
                    requests.Add((T)(IMessage)uploadRequest);
                }
                else if (request is UpdateFileRequest updateRequest)
                {
                    updateRequest.FileName = fileName;
                    updateRequest.SessionId = sessionId;
                    updateRequest.FileData = Google.Protobuf.ByteString.CopyFrom(chunk);
                    requests.Add((T)(IMessage)updateRequest);
                }
            }

            return requests;
        }

        public DeleteFileResponse DeleteFile(string fileName, string sessionId)
        {
            var request = new DeleteFileRequest { FileName = fileName, SessionId = sessionId };
            var response = this._client.DeleteFile(request);
            return response;
        }
    }
}