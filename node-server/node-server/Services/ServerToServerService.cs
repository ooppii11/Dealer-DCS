using Google.Protobuf;
using Grpc.Core;
using GrpcServerToServer;
using NodeServer.Managers;

namespace NodeServer.Services
{
    public class ServerToServerService : ServerToServer.ServerToServerBase
    {
        private FileSaving _microservice;
        private NodeSystemParse _system;
        private readonly string _serverIP = Environment.GetEnvironmentVariable("NODE_SERVER_IP");
        public ServerToServerService(NodeSystemParse sys, FileSaving micro) 
        {
            this._system = sys;
            this._microservice = micro;
        }

        public override async Task<PassFileResponse> PassFile(IAsyncStreamReader<PassFileRequest> requestStream, ServerCallContext context)
        {
            try
            {
                //consensus + S2S
                string fileName = "";
                string type = "";
                List<string> otherNodeServersAddresses = new List<string>();
                MemoryStream fileData = new MemoryStream();


                await foreach (var chunk in requestStream.ReadAllAsync())
                {
                    fileName = chunk.FileId;
                    type = chunk.Type;
                    fileData.Write(chunk.FileContent.ToArray(), 0, chunk.FileContent.Length);
                    foreach (var serverAddress in chunk.ServersAddressesWhereSaved)
                    {
                        otherNodeServersAddresses.Add(serverAddress);
                    }
                }

                if (!this._system.filExists(fileName))
                {

                    await this._microservice.uploadFile(fileName, fileData.ToArray(), type);
                    this._system.addFile(fileName, otherNodeServersAddresses);
                }
                else
                {
                    context.Status = new Status(StatusCode.AlreadyExists, $"File already exists on the machine - {this._serverIP}");
                    return new PassFileResponse { Status = false, Message = $"Unable to update file: File already exists on the machine - {this._serverIP}"};
                }
                return new PassFileResponse { Status = true, Message = "File uploaded successfully." };
            }
            catch (Exception ex)
            {
                context.Status = new Status(StatusCode.Internal, $"Error uploading file: {ex.Message}");
                return new PassFileResponse { Status = false, Message = $"Error uploading file: {ex.Message}" };
            }
        }
    }
}
