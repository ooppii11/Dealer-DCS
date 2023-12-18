using Google.Protobuf;
using Grpc.Core;
using GrpcServerToServer;
using NodeServer.Managers;

namespace NodeServer.Services
{
    public class ServerToServerService : ServerToServer.ServerToServerBase
    {
        private FileSaving _microservice;
        private Dictionary<string, List<string>> _replicatedPlaces;
        private NodeSystemParse _system;
        private readonly string _serverIP = Environment.GetEnvironmentVariable("NODE_SERVER_IP");
        public ServerToServerService(NodeSystemParse sys, FileSaving micro, Dictionary<string, List<string>> places) 
        {
            this._system = sys;
            this._microservice = micro;
            this._replicatedPlaces = places;
        }

        public override async Task<PassFileResponse> PassFile(IAsyncStreamReader<PassFileRequest> requestStream, ServerCallContext context)
        {
            try
            {
                string fileName = "";
                string type = "";
                List<string> placesFromRequest = new List<string>();
                MemoryStream fileData = new MemoryStream();
                List<string> places = ((Environment.GetEnvironmentVariable("NODES_IPS")).Split(':').ToList());
                places.Remove(this._serverIP);


                await foreach (var chunk in requestStream.ReadAllAsync())
                {
                    fileName = chunk.FileId;
                    type = chunk.Type;
                    fileData.Write(chunk.FileContent.ToArray(), 0, chunk.FileContent.Length);
                    foreach (var serverAddress in chunk.ServersAddressesWhereSaved)
                    {
                        placesFromRequest.Add(serverAddress);
                    }
                }
                if (!places.Contains(fileName))
                {

                    this._replicatedPlaces[fileName] = places;
                    await this._microservice.uploadFile(fileName, fileData.ToArray(), type);
                    this._system.addFile();
                    //consensus + S2S
                }
                else
                {
                    return new PassFileResponse { Status = false, Message = "Unable to upload file: The file is already saved on the machine" };
                }
                return new PassFileResponse { Status = true, Message = "File uploaded successfully." };
            }
            catch (Exception ex)
            {
                return new PassFileResponse { Status = false, Message = $"Error uploading file: {ex.Message}" };
            }
        }
    }
}
