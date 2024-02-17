using cloud_server.DB;
using Grpc.Core;
using Grpc.Net.Client;
using GrpcNodeServer;
using System;
using System.ComponentModel.DataAnnotations;
using System.Runtime.CompilerServices;
using System.Text;
using Grpc;
namespace cloud_server.Services
{
    public class NodeServerCommunication
    {
        //private GrpcChannel _channel;
        private Grpc.Core.Channel _channel; //Grpc.Core.Channel _channel;
        private GrpcNodeServer.NodeServices.NodeServicesClient _client;
        private const int MaxFileChunckLength = 3145728;
        public NodeServerCommunication(string host)
        {
            try
            {
                // Create Grpc connction:
                this._channel = new Channel(host, ChannelCredentials.Insecure);
                //this._channel = new GrpcChannel.ForAddress(host);
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

        public async Task<byte[]> DownloadFile(string fileId)
        {
            DownloadFileRequest request = new DownloadFileRequest { FileId = fileId };
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

        public void deleteFile(string fileId)
        {
            DeleteFileRequest request = new DeleteFileRequest { FileId = fileId };
            this._client.DeleteFile(request);
        }

        public async Task<UploadFileResponse> uploadFile(string fileId, byte[] fileData, string type, Location locations)
        {
            List<UploadFileRequest> requests = createUploadRequests(fileId, fileData, type, locations);
            
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

        private List<UploadFileRequest> createUploadRequests(string fileId, byte[] fileData, string type, Location location)
        {
            List<UploadFileRequest> uploadFileRequests = null;
            byte[] chunk = null;
            UploadFileRequest request = null;
            int numberOfChunks = 0;
            int chunkSize = NodeServerCommunication.MaxFileChunckLength;
            int i = 0;

            uploadFileRequests = new List<UploadFileRequest> ();
            numberOfChunks = fileData.Length / chunkSize + ((fileData.Length % chunkSize == 0) ? 0 : 1);

            for (i = 0; i < numberOfChunks; i++)
            {
                if ((i + 1) * chunkSize < fileData.Length)
                {
                    chunk = new byte[chunkSize];
                }

                else
                {
                    chunk = new byte[fileData.Length % ((i + 1) * chunkSize)];
                }

                for (int j = 0; j < chunkSize && j + i* chunkSize < fileData.Length; j++)
                {
                    chunk[j] = fileData[i * chunkSize + j];
                }

                request = new UploadFileRequest()
                {
                    FileId = fileId,
                    Type = type,
                    FileContent = Google.Protobuf.ByteString.CopyFrom(chunk)
                };
                request.ServersAddressesWhereSaved.Add(location.FirstBackupServer);
                request.ServersAddressesWhereSaved.Add(location.SecondBackupServer);


                 uploadFileRequests.Add(request);
            }

            return uploadFileRequests;
        }

    }
}
