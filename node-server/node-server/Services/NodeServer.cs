using Grpc.Core;
using GrpcNodeServer;

namespace NodeServer.Services
{
    public class NodeServer : NodeServices.NodeServicesBase
    {
        private FileSaving microservice;
        private string[] replicationPlaces;
        public NodeServer(string host= "localhost", int port=50051) {
            microservice = new FileSaving(host, port);
            replicationPlaces = new string[3];
        }

        public override async Task<UploadFileResponse> UploadFile(IAsyncStreamReader<UploadFileRequest> requestStream, ServerCallContext context)
        {
            try
            {
                string fileName = null;
                string type = null;
                MemoryStream fileData = new MemoryStream();

                await foreach (var chunk in requestStream.ReadAllAsync())
                {
                    if (fileName == null)
                    {
                        fileName = chunk.FileId;
                    }
                    if (type == null)
                    {
                        type = chunk.Type;
                    }
                    fileData.Write(chunk.FileContent.ToArray(), 0, chunk.FileContent.Length);
                }
                microservice.uploadFile(fileName, fileData.ToArray(), type);
                //consensus + S2S

                return new UploadFileResponse { Status = true, Message = "File uploaded successfully." };
            }
            catch (Exception ex)
            {
                return new UploadFileResponse { Status = false, Message = $"Error uploading file: {ex.Message}" };
            }
        }
    }
}