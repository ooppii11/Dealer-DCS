using cloud_server.DB;
using Grpc.Core;
using Grpc.Net.Client;
using GrpcNodeServer;
using System;
using System.ComponentModel.DataAnnotations;
using System.Runtime.CompilerServices;
using System.Text;
using Grpc;
using static Google.Protobuf.Reflection.SourceCodeInfo.Types;

namespace cloud_server.Services
{
    public class NodeServerCommunication
    {
        private Grpc.Core.Channel _channel; 
        private GrpcNodeServer.NodeServices.NodeServicesClient _client;
        private const int MaxFileChunckLength = 3145728;
        public NodeServerCommunication(string host)
        {
            try
            {
                // Create Grpc connction:
                this._channel = new Channel(host, ChannelCredentials.Insecure);
                this._client = new NodeServices.NodeServicesClient(this._channel);
            }
            catch (RpcException ex)
            {
                Console.WriteLine(ex.Message);
                throw ex;
            }
        }

        ~NodeServerCommunication()
        {
            this._channel.ShutdownAsync().Wait();
        }

        public async Task<byte[]> DownloadFile(int userId, string fileId)
        {
            DownloadFileRequest request = new DownloadFileRequest { UserId = userId, FileId = fileId };
            
            // Download the file:
            using (var call = this._client.DownloadFile(request))
            {
                using (var memoryStream = new MemoryStream())
                {
                    // append chuncks to memoryStream
                    while (await call.ResponseStream.MoveNext())
                    {
                        var chunk = call.ResponseStream.Current.FileContent;
                        await memoryStream.WriteAsync(chunk.ToByteArray(), 0, chunk.Length);
                    }
                    return memoryStream.ToArray();
                }
            }
        }

        public void deleteFile(int userId, string fileId)
        {
            DeleteFileRequest request = new DeleteFileRequest { UserId = userId, FileId = fileId };
            this._client.DeleteFile(request);
        }

        public async Task<UploadFileResponse> uploadFile(int userId, string fileId, byte[] fileData, string type)
        {
            List<UploadFileRequest> requests = createUploadRequests(userId, fileId, fileData, type);
            
            var call = this._client.UploadFile();

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

        public async Task<UpdateFileResponse> updateFile(int userId, string fileId, byte[] fileData)
        {
            List<UpdateFileRequest> requests = createUpdateRequests(userId, fileId, fileData);

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

        private List<UpdateFileRequest> createUpdateRequests(int userId, string fileId, byte[] fileData)
        {
            List<UpdateFileRequest> updateFileRequests = null;
            byte[] chunk = null;
            UpdateFileRequest request = null;
            int numberOfChunks = 0;
            int chunkSize = NodeServerCommunication.MaxFileChunckLength;
            int i, j = 0;

            updateFileRequests = new List<UpdateFileRequest>();
            numberOfChunks = fileData.Length / chunkSize + ((fileData.Length % chunkSize == 0) ? 0 : 1);

            for (i = 0; i < numberOfChunks; i++)
            {
                // Check for the size of the new chunck of bytes:
                if ((i + 1) * chunkSize < fileData.Length) { chunk = new byte[chunkSize]; }
                else { chunk = new byte[fileData.Length % ((i + 1) * chunkSize)]; }

                // Set data inside the Chunck 
                for (j = 0; j < chunkSize && j + i * chunkSize < fileData.Length; j++)
                {
                    chunk[j] = fileData[i * chunkSize + j];
                }

                // Create new request:
             
                request = new UpdateFileRequest()
                {
                    FileId = fileId,
                    UserId = userId,
                    NewContent = Google.Protobuf.ByteString.CopyFrom(chunk),
                    
                };

                // Append request to the stream
                updateFileRequests.Add(request);
            }

            return updateFileRequests;
        }
        private List<UploadFileRequest> createUploadRequests(int userId, string fileId, byte[] fileData, string type)
        {
            List<UploadFileRequest> uploadFileRequests = null;
            byte[] chunk = null;
            UploadFileRequest request = null;
            int numberOfChunks = 0;
            int chunkSize = NodeServerCommunication.MaxFileChunckLength;
            int i, j = 0;

            uploadFileRequests = new List<UploadFileRequest> ();
            numberOfChunks = fileData.Length / chunkSize + ((fileData.Length % chunkSize == 0) ? 0 : 1);

            for (i = 0; i < numberOfChunks; i++)
            {
                // Check for the size of the new chunck of bytes:
                if ((i + 1) * chunkSize < fileData.Length) { chunk = new byte[chunkSize]; }
                else { chunk = new byte[fileData.Length % ((i + 1) * chunkSize)]; }

                // Set data inside the Chunck 
                for (j = 0; j < chunkSize && j + i* chunkSize < fileData.Length; j++)
                {
                    chunk[j] = fileData[i * chunkSize + j];
                }

                // Create new request:
                request = new UploadFileRequest()
                {
                    FileId = fileId,
                    UserId = userId,
                    Type = type,
                    FileContent = Google.Protobuf.ByteString.CopyFrom(chunk)
                };
                

                // Append request to the stream
                uploadFileRequests.Add(request);
            }

            return uploadFileRequests;
        }

    }
}
