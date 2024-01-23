using Google.Protobuf;
using Grpc.Core;
using GrpcNodeServer;
using NodeServer.Managers;
using GrpcServerToServer;
using Managers;

namespace NodeServer.Services
{
    public class NodeServerService : NodeServices.NodeServicesBase
    {
        private FileSaving _microservice;
        private NodeSystemParse _system;
        private readonly string _serverIP = Environment.GetEnvironmentVariable("NODE_SERVER_IP");
        //logFileInfo 
        public NodeServerService(FileSaving micro, NodeSystemParse sys)
        {
            this._microservice = micro;
            this._system = sys;
        }

        public override async Task<UploadFileResponse> UploadFile(IAsyncStreamReader<UploadFileRequest> requestStream, ServerCallContext context)
        {
            List<string> unreachableServers = new List<string>();
            try
            {
                //consensus + S2S
                string fileID = "";
                string type = "";
                List<string> otherNodeServersAddresses = new List<string>();
                MemoryStream fileData = new MemoryStream();
                bool fileAlreadyExist = false;



                await foreach (var chunk in requestStream.ReadAllAsync())
                {
                    fileID = chunk.FileId;
                    type = chunk.Type;
                    fileData.Write(chunk.FileContent.ToArray(), 0, chunk.FileContent.Length);
                    foreach (var serverAddress in chunk.ServersAddressesWhereSaved)
                    {
                        otherNodeServersAddresses.Add(serverAddress);
                    }
                }


               
                await this._microservice.uploadFile(fileID, fileData.ToArray(), type);
                List<string> serverList = otherNodeServersAddresses;
                serverList.Remove(this._serverIP);
                this._system.addFile(fileID, otherNodeServersAddresses);
                foreach (string serverAddress in serverList)
                {
                    try
                    {
                        fileData.Seek(0, SeekOrigin.Begin);
                        ServerToServerClient s2s = new ServerToServerClient(serverAddress, 50052);
                        PassFileResponse response = await s2s.passFile(fileID, type, otherNodeServersAddresses, fileData);
                        Console.WriteLine($"status: {response.Status}, massenge: {response.Message}");
                    }
                    catch (RpcException ex)
                    {
                        if (!(ex.StatusCode == StatusCode.AlreadyExists))
                        {
                            unreachableServers.Add(serverAddress);
                        }
                    }
                }


                Task.Run(() => tryPassingFile(fileID, type, unreachableServers, otherNodeServersAddresses, fileData));

                return new UploadFileResponse { Status = true, Message = "File uploaded successfully.", UnreachableServers = { unreachableServers } };
            }
            catch (Exception ex)
            {
                context.Status = new Status(StatusCode.Internal, $"Error uploading file: {ex.Message}");
                return new UploadFileResponse { Status = false, Message = $"Error updating the file: {ex.Message}", UnreachableServers = { unreachableServers } }; ;
            }
        }

        private async Task tryPassingFile(string fileID, string type, List<string> unreachableServers, List<string> otherNodeServersAddresses, MemoryStream fileData)
        {

            const int delayInSeconds = 120;
            while (unreachableServers.Count > 0)
            {
                string serverAddress = unreachableServers.First();
                unreachableServers.Remove(serverAddress);
                try
                {
                    fileData.Seek(0, SeekOrigin.Begin);
                    ServerToServerClient s2s = new ServerToServerClient(serverAddress, 50052);
                    PassFileResponse response = await s2s.passFile(fileID, type, otherNodeServersAddresses, fileData);
                    Console.WriteLine($"status: {response.Status}, massenge: {response.Message}");
                }
                catch (RpcException ex)
                {
                    unreachableServers.Add(serverAddress);
                }

                if (unreachableServers.Count > 0)
                {
                    Console.WriteLine($"Not all servers received the message. Retrying in {delayInSeconds} seconds...");
                    Thread.Sleep(TimeSpan.FromSeconds(delayInSeconds));
                }
            }
        }   
        
        public override async Task<UpdateFileResponse> UpdateFile(IAsyncStreamReader<UpdateFileRequest> requestStream, ServerCallContext context)
        {
            try
            {
                //consensus + S2S
                string fileName = null;

                MemoryStream fileData = new MemoryStream();

                await foreach (var chunk in requestStream.ReadAllAsync())
                {
                    if (fileName == null)
                    {
                        fileName = chunk.FileId;
                    }
                    fileData.Write(chunk.NewContent.ToArray(), 0, chunk.NewContent.Length);
                }
                if (this._system.filExists(fileName))
                {
                    //get type from microservice
                    this._microservice.deleteFile(fileName);
                    await this._microservice.uploadFile(fileName, fileData.ToArray(), "");
                    //consensus + S2S
                }
                else
                {
                    context.Status = new Status(StatusCode.NotFound, "File not found");
                    return new UpdateFileResponse { Status = false, Message = "Unable to update file: The file isn't saved on the machine" };
                }


                return new UpdateFileResponse { Status = true, Message = "File updated successfully." };
            }
            catch (Exception ex)
            {
                context.Status = new Status(StatusCode.Internal, $"Error updating the file: {ex.Message}");
                return new UpdateFileResponse { Status = false, Message = $"Error updating the file: {ex.Message}" };
            }
        }

        public override async Task DownloadFile(DownloadFileRequest request, IServerStreamWriter<DownloadFileResponse> responseStream, ServerCallContext context)
        {
            try
            {
                //consensus + S2S
                byte[] file = await this._microservice.downloadFile(request.FileId);
                int offset = 0;
                int chunkSize = 64000;
                if (file != null)
                {
                    while (offset < file.Length)
                    {
                        int remaining = file.Length - offset;
                        int writingSize = Math.Min(remaining, chunkSize);

                        DownloadFileResponse response = new DownloadFileResponse { Status = true, FileContent = ByteString.CopyFrom(file, offset, writingSize) };
                        await responseStream.WriteAsync(response);

                        offset += writingSize;
                    }
                }
            }
            catch (Exception ex)
            {
                context.Status = new Status(StatusCode.Internal, $"Error downloading the file: {ex.Message}");
                await responseStream.WriteAsync(new DownloadFileResponse { Status = false, Message = $"Error downloading the file: {ex.Message}", FileContent = ByteString.Empty });
                return;
            }

        }

        public override Task<DeleteFileResponse> DeleteFile(DeleteFileRequest request, ServerCallContext context)
        {
            try
            {
                //consensus + S2S
                this._microservice.deleteFile(request.FileId);
                this._system.removeFile(request.FileId);

                return Task.FromResult(new DeleteFileResponse { Status = true, Message = "File deleted successfully." });
            }
            catch (Exception ex)
            {
                context.Status = new Status(StatusCode.Internal, $"Error deleting the file: {ex.Message}");
                return Task.FromResult(new DeleteFileResponse { Status = false, Message = $"Error deleting the file: {ex.Message}" });
            }

        }
    }
}