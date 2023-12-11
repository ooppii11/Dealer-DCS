using cloud_server.DB;
using Grpc.Core;
using GrpcNodeServer;
using System;
using System.ComponentModel.DataAnnotations;

namespace cloud_server.Services
{
    public class NodeServerCommunication
    {
        private Grpc.Core.Channel _channel;
        private GrpcNodeServer.NodeServices.NodeServicesClient _client;
        private const int MaxFileChunckLength = 3145728;

        public NodeServerCommunication(string host, int port)
        {
            try
            {
                // Create Grpc connction:
                this._channel = new Channel($"{host}:{port}", ChannelCredentials.Insecure);
                this._client = new NodeServices.NodeServicesClient(this._channel);
            }
            catch (Exception ex)
            {
                throw new Exception("Cannot connect to the servise");
            }
        }

        ~NodeServerCommunication()
        {
            this._channel.ShutdownAsync().Wait();
        }

        public async Task<byte[]> DownloadFile(string fileId)
        {
            DownloadFileRequest request = new DownloadFileRequest { FileId = fileId };
            
            try
            {
                // Download chunks of file
                using (var call = this._client.DownloadFile(request))
                {
                    using (var memoryStream = new MemoryStream())
                    {
                        while (await call.ResponseStream.MoveNext())
                        {
                            var chunk = call.ResponseStream.Current.FileContent;
                            await memoryStream.WriteAsync(chunk.ToByteArray(), 0, chunk.Length);
                        }
                        return memoryStream.ToArray();
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Error download this file");
            }
        }

        public void deleteFile(string fileId)
        {
            DeleteFileRequest request = new DeleteFileRequest { FileId = fileId };
            
            try
            {
                this._client.DeleteFile(request);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error to delete this file");
            }
        }

        public async Task<UploadFileResponse> uploadFile(string fileId, byte[] fileData, string type, Location location)
        {
            IEnumerable<UploadFileRequest> requests = createUploadRequests(fileId, fileData, type, location);
            try
            {
                var call = this._client.UploadFile();

                // For evry chunk of file call upload 
                foreach (var request in requests)
                {
                    await call.RequestStream.WriteAsync(request);
                }

                await call.RequestStream.CompleteAsync();

                var response = await call.ResponseAsync;
                return response;
            }
            catch (Exception ex)
            {

                Console.WriteLine(ex.ToString());
            }
            return new UploadFileResponse();
        }

        private IEnumerable<UploadFileRequest> createUploadRequests(string fileId, byte[] fileData, string type, Location location)
        {
            IEnumerable<UploadFileRequest> uploadFileRequests = null;
            byte[] chunk = null;
            UploadFileRequest request = null;
            int numberOfChunks = 0;
            int chunkSize = NodeServerCommunication.MaxFileChunckLength;
            int i = 0;

            uploadFileRequests = new List<UploadFileRequest>();
            numberOfChunks = fileData.Length / chunkSize + ((fileData.Length % chunkSize == 0) ? 0 : 1);

            for (i = 0; i < numberOfChunks; i++)
            {
                chunk = new byte[chunkSize];

                for (int j = 0; j < chunkSize; j++)
                {
                    chunk[j] = fileData[i * chunkSize + j];
                }

                request = new UploadFileRequest()
                {
                    FileId = fileId,
                    Type = type,
                    SecondReplicationPlace = location.SecondBackupServer,
                    ThirdReplicationPlace = location.FirstBackupServer,
                    FileContent = Google.Protobuf.ByteString.CopyFrom(chunk)
                };

                uploadFileRequests.Append(request);
            }

            return uploadFileRequests;
        }

    }
}
