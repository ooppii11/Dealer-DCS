using Google.Protobuf;
using Grpc.Core;
using GrpcNodeServer;
using NodeServer.Managers;
using GrpcServerToServer;
using NodeServer.Managers.RaftNameSpace;
using static NodeServer.Managers.RaftNameSpace.Raft;
using LogEntry = NodeServer.Managers.RaftNameSpace.LogEntry;

namespace NodeServer.Services
{
    public class NodeServerService : NodeServices.NodeServicesBase
    {
        private FileSaving _microservice;
        private Raft _raft;
        private FileVersionManager _fileVersionManager;
        private readonly string _folderName = "tempFiles";
        public NodeServerService(FileSaving micro, Raft raft, FileVersionManager fileVerM)
        {
            this._microservice = micro;
            this._raft = raft;
            this._fileVersionManager = fileVerM;
        }

        private int GetLastIndex()
        {
            RaftSettings tempRaftSettings = new RaftSettings();
            return (new Log(tempRaftSettings.LogFilePath)).GetLastLogEntry().Index;
        }

        private string GetServerIP()
        {
            RaftSettings tempRaftSettings = new RaftSettings();
            return tempRaftSettings.ServerAddress;
        }

        private async Task<string> UploadOperationArgsToString(IAsyncStreamReader<UploadFileRequest> requestStream)
        {
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

            return "";
        }

        public override async Task<UploadFileResponse> UploadFile(IAsyncStreamReader<UploadFileRequest> requestStream, ServerCallContext context)
        {
            try
            {
                /*get data - new function*/
                

                /*Raft: append entry*/
                const string operationName = "uploadFile";
                //LogEntry entry = new LogEntry(GetLastIndex() + 1, GetServerIP(), operationName, );
                //if (await this._raft.appendEntry(entry))
                {
                    return new UploadFileResponse { Status = true, Message = "File uploaded successfully." };
                }
                //context.Status = new Status(StatusCode.PermissionDenied, "Can't get requests from cloud, this server is not the leader at the moment.");
                //return new UploadFileResponse { Status = false, Message = "Can't get requests from cloud, this server is not the leader at the moment." };

            }
            catch (Exception ex)
            {
                context.Status = new Status(StatusCode.Internal, $"Error uploading file: {ex.Message}");
                return new UploadFileResponse { Status = false, Message = $"Error updating the file: {ex.Message}"}; ;
            }
        }
        
        public override async Task<UpdateFileResponse> UpdateFile(IAsyncStreamReader<UpdateFileRequest> requestStream, ServerCallContext context)
        {
            try
            {
                /*get data - new function*/
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

                /*Raft: append entry*/

                /*preform action*/
                this._microservice.deleteFile(fileName);
                await this._microservice.uploadFile(fileName, fileData.ToArray(), "");


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
                /*Raft: append entry*/

                /*preform action + get data*/
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
                /*Raft: append entry*/

                /*preform action + get data*/
                this._microservice.deleteFile(request.FileId);

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