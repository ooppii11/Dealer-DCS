using cloud_server.DB;
using Grpc.Core;
using Grpc.Net.Client;
using GrpcNodeServer;
using System;
using System.ComponentModel.DataAnnotations;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace cloud_server.Services
{
    public class NodeServerCommunication
    {
        private GrpcChannel _channel; //Grpc.Core.Channel _channel;
        private GrpcNodeServer.NodeServices.NodeServicesClient _client;
        private const int MaxFileChunckLength = 3145728;

        public static async Task Main(string[] args)
        {
            string fileName = "test2.txt";
            byte[] data = Encoding.ASCII.GetBytes(fileName);
            NodeServerCommunication node = new NodeServerCommunication("127.0.0.1", 50052);
            var response1 = await node.uploadFile(fileName, data, "", new Location("", "", ""));
            Console.Write(response1.ToString());
            
            var response2 = await node.DownloadFile(fileName);
            node.deleteFile(fileName);

           
            Console.WriteLine(Encoding.ASCII.GetString(response2));    
            
        }
        public NodeServerCommunication(string host, int port)
        {
            try
            {
                // Create Grpc connction:
                this._channel = GrpcChannel.ForAddress("http://localhost:50052");
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
            List<UploadFileRequest> requests = createUploadRequests(fileId, fileData, type, location);
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
                    SecondReplicationPlace = location.SecondBackupServer,
                    ThirdReplicationPlace = location.FirstBackupServer,
                    FileContent = Google.Protobuf.ByteString.CopyFrom(chunk)
                };

                 uploadFileRequests.Add(request);
            }

            return uploadFileRequests;
        }

    }
}
