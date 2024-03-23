using Grpc.Core;
using GrpcFileCloudAccessClient;

namespace NodeServer.Managers
{
    public class FileSaving : ActionMaker
    {
        private Grpc.Core.Channel channel;
        private FileCloudAccess.FileCloudAccessClient client;

        public FileSaving(string host, int port)
        {
            try
            {
                // Create Grpc connction:
                channel = new Channel($"{host}:{port}", ChannelCredentials.Insecure);
                client = new FileCloudAccess.FileCloudAccessClient(channel);
            }
            catch (Exception ex)
            {
                throw new Exception("Cannot connect to the service");
            }
        }

        ~FileSaving()
        {
            this.channel.ShutdownAsync().Wait();
        }

        public void Dispose() 
        {
            this.channel.ShutdownAsync().Wait();
        }


        public async Task<byte[]> downloadFile(string filename)
        {
            DownloadFileRequest request = new DownloadFileRequest { FileName = filename };
            try
            {
                // Download chunks of file
                using (var call = client.DownloadFile(request))
                {
                    using (var memoryStream = new MemoryStream())
                    {
                        while (await call.ResponseStream.MoveNext())
                        {
                            var chunk = call.ResponseStream.Current.FileData;
                            await memoryStream.WriteAsync(chunk.ToByteArray(), 0, chunk.Length);
                        }
                        return memoryStream.ToArray();
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Error occurred while downloading this file");
            }
        }

        public async Task<UploadFileResponse> uploadFile(string filename, byte[] fileData, string type)
        {
            try
            {
                IEnumerable<UploadFileRequest> requests = new[] { new UploadFileRequest() { FileName = filename, FileData = Google.Protobuf.ByteString.CopyFrom(fileData), Type = type } };
                var call = client.UploadFile();
  
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

                throw new Exception("Error occurred while uploading this file");
            }
            
        }

        public void deleteFile(string filename)
        {
            DeleteFileRequest request = new DeleteFileRequest { FileName = filename };
            try
            {
                client.DeleteFile(request);
            }
            catch (Exception ex)
            {
                throw new Exception("Error deleting this file");
            }
        }
    }
}
