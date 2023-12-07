using Grpc.Core;
using GrpcNodeServer;

namespace NodeServer.Services
{
    public class NodeServer : NodeServices.NodeServicesBase
    {
        private FileSaving _microservice;
        private Dictionary<string, (string, string)> _replicatedPlaces;
        //loginfo 
        public NodeServer(string host= "127.0.0.1", int port=50051) {
            this._microservice = new FileSaving(host, port);
            this._replicatedPlaces = new Dictionary<string, (string, string)>();

            //parse log and get all the replicated places
        }

        public override async Task<UploadFileResponse> UploadFile(IAsyncStreamReader<UploadFileRequest> requestStream, ServerCallContext context)
        {
            try
            {
                string fileName = null;
                string type = null;
                string SecondReplicationPlace = null;
                string ThirdReplicationPlace = null;
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
                    if (SecondReplicationPlace == null)
                    {
                        SecondReplicationPlace = chunk.SecondReplicationPlace;
                    }
                    if (ThirdReplicationPlace == null)
                    {
                        ThirdReplicationPlace = chunk.ThirdReplicationPlace;
                    }
                    fileData.Write(chunk.FileContent.ToArray(), 0, chunk.FileContent.Length);
                }
                this._replicatedPlaces[fileName] = (SecondReplicationPlace, ThirdReplicationPlace);
                this._microservice.uploadFile(fileName, fileData.ToArray(), type);
                //consensus + S2S

                return new UploadFileResponse { Status = true, Message = "File uploaded successfully." };
            }
            catch (Exception ex)
            {
                return new UploadFileResponse { Status = false, Message = $"Error uploading file: {ex.Message}" };
            }
        }


        public override async Task<UpdateFileResponse> UpdateFile(IAsyncStreamReader<UpdateFileRequest> requestStream, ServerCallContext context)
        {
            try
            {
                string fileName = null;

                MemoryStream fileData = new MemoryStream();

                await foreach(var chunk in requestStream.ReadAllAsync())
                {
                    if (fileName == null)
                    {
                        fileName = chunk.FileId;
                    }
                    fileData.Write(chunk.NewContent.ToArray(), 0, chunk.NewContent.Length);
                }
                if (this._replicatedPlaces.ContainsKey(fileName))
                {
                    //get type from microservice
                    this._microservice.deleteFile(fileName);
                    this._microservice.uploadFile(fileName, fileData.ToArray(), "");
                    //consensus + S2S
                }
                else
                {
                    return new UpdateFileResponse { Status = false, Message = "Unable to update file: The file isn't saved on the machine" };
                }
                

                return new UpdateFileResponse { Status = true, Message = "File updated successfully." };
            }
            catch (Exception ex)
            {
                return new UpdateFileResponse { Status = false, Message = $"Error updating file: {ex.Message}" };
            }
        }
    }
}