using Google.Protobuf;
using Grpc.Core;
using GrpcNodeServer;
using NodeServer.Managers;
using NodeServer.Managers.RaftNameSpace;
using LogEntry = NodeServer.Managers.RaftNameSpace.LogEntry;
using NodeServer.Utilities;
using System.Diagnostics.CodeAnalysis;
using static System.Net.Mime.MediaTypeNames;

namespace NodeServer.Services
{
    public class NodeServerService : NodeServices.NodeServicesBase
    {
        private FileSaving _microservice;
        private Raft _raft;
        private FileVersionManager _fileVersionManager;
        

        public NodeServerService(FileSaving micro, Raft raft, FileVersionManager fileVerM)
        {
            this._microservice = micro;
            this._raft = raft;
            this._fileVersionManager = fileVerM;

            string currentDirectory = Directory.GetCurrentDirectory();
            string folderPath = Path.Combine(currentDirectory, OnMachineStorageActions._baseFolderName);
            if (!Directory.Exists(folderPath))
            {
                Directory.CreateDirectory(folderPath);
            }
        }

        
        static private int GetLastIndex()
        {
            RaftSettings tempRaftSettings = new RaftSettings();
            return (new Log(tempRaftSettings.LogFilePath)).GetLastLogEntry().Index;
        }

        private static string GetServerIP()
        {
            RaftSettings tempRaftSettings = new RaftSettings();
            return tempRaftSettings.ServerAddress;
        }

        private async Task<Tuple<Status, string, MemoryStream>> ParseUploadRequest(IAsyncStreamReader<UploadFileRequest> requestStream)
        {
            string fileId = "";
            int userId = 0;
            string type = "";
            MemoryStream fileData = new MemoryStream();

            await foreach (var chunk in requestStream.ReadAllAsync())
            {
                fileId = chunk.FileId;
                type = chunk.Type;
                userId = chunk.UserId;
                fileData.Write(chunk.FileContent.ToArray(), 0, chunk.FileContent.Length);
            }


            if (OnMachineStorageActions.DoesFileExist(userId, fileId))
            {
                return new Tuple<Status, string, MemoryStream>(new Status(StatusCode.AlreadyExists, "Can't upload the file. The file already exist in the system"), null, null);
            }

            if (!OnMachineStorageActions.SaveFile(fileId, userId, type, fileData, this._fileVersionManager))
            {
                return new Tuple<Status, string, MemoryStream>(new Status(StatusCode.ResourceExhausted, "Can't upload the file. Either the file is too big or the user has used up all their memory."), null, null);
            }

            return new Tuple<Status, string, MemoryStream>(new Status(), $"[{userId},{fileId},{this._fileVersionManager.GetLatestFileVersion(fileId, userId)},{type}]", fileData);
        }
        

        public override async Task<UploadFileResponse> UploadFile(IAsyncStreamReader<UploadFileRequest> requestStream, ServerCallContext context)
        {
            try
            {
                const string operationName = "UploadFile";
                Tuple<Status, string, MemoryStream> StatusArgsAndFileData = await ParseUploadRequest(requestStream);
                if (StatusArgsAndFileData.Item2 == null) 
                {
                    context.Status = StatusArgsAndFileData.Item1;
                    return new UploadFileResponse { Status = false, Message = StatusArgsAndFileData.Item1.Detail };
                }

                

                LogEntry entry = new LogEntry(GetLastIndex() + 1, GetServerIP(), operationName, StatusArgsAndFileData.Item2);
                if (this._raft.appendEntry(entry, StatusArgsAndFileData.Item3.ToArray()))
                {
                    return new UploadFileResponse { Status = true, Message = "File uploaded successfully." };
                }

                context.Status = new Status(StatusCode.PermissionDenied, "Can't get requests from cloud, this server is not the leader at the moment.");
                return new UploadFileResponse { Status = false, Message = "Can't get requests from cloud, this server is not the leader at the moment." };

            }
            catch (Exception ex)
            {
                context.Status = new Status(StatusCode.Internal, $"Error uploading file: {ex.Message}");
                return new UploadFileResponse { Status = false, Message = $"Error updating the file: {ex.Message}"}; ;
            }
        }

        private async Task<Tuple<Status, string, MemoryStream>> ParseUpdateRequest(IAsyncStreamReader<UpdateFileRequest> requestStream)
        {
            string fileId = "";
            int userId = 0;
            
            MemoryStream fileData = new MemoryStream();

            await foreach (var chunk in requestStream.ReadAllAsync())
            {
                fileId = chunk.FileId;
                userId = chunk.UserId;
                fileData.Write(chunk.NewContent.ToArray(), 0, chunk.NewContent.Length);
            }

           
            string type = this._fileVersionManager.GetFileType(fileId, userId); 


            if (!OnMachineStorageActions.DoesFileExist(userId, fileId))
            {
                return new Tuple<Status, string, MemoryStream>(new Status(StatusCode.AlreadyExists, "Can't update the file. The file doesn't exist int the system"), null, null);
            }

            if (!OnMachineStorageActions.SaveFile(fileId, userId, type, fileData, this._fileVersionManager))
            {
                return new Tuple<Status, string, MemoryStream>(new Status(StatusCode.ResourceExhausted, "Can't upload the file. Either the file is too big or the user has used up all their memory."), null, null);
            }

            return new Tuple<Status, string, MemoryStream>(new Status(), $"[{userId},{fileId},{this._fileVersionManager.GetLatestFileVersion(fileId, userId)}]", fileData);
        }
        

        public override async Task<UpdateFileResponse> UpdateFile(IAsyncStreamReader<UpdateFileRequest> requestStream, ServerCallContext context)
        {
            try
            {
                const string operationName = "UpdateFile";
                Console.WriteLine("UpdateFile node service");
               // Tuple<Status, string, MemoryStream> StatusArgsAndFileData = await UpdateOperationArgsToString(requestStream);
                Console.WriteLine("get args");

                Tuple<Status, string, MemoryStream> StatusArgsAndFileData = await ParseUpdateRequest(requestStream);
                if (StatusArgsAndFileData.Item2 == null)
                {
                    context.Status = StatusArgsAndFileData.Item1;
                    return new UpdateFileResponse { Status = false, Message = StatusArgsAndFileData.Item1.Detail };
                }
                Console.WriteLine("pass first");

                LogEntry entry = new LogEntry(GetLastIndex() + 1, GetServerIP(), operationName, StatusArgsAndFileData.Item2);
                Console.WriteLine("UpdateFile node service create entry");

                if (this._raft.appendEntry(entry, StatusArgsAndFileData.Item3.ToArray()))
                {
                    return new UpdateFileResponse { Status = true, Message = "File uploaded successfully." };
                }

                context.Status = new Status(StatusCode.PermissionDenied, "Can't get requests from cloud, this server is not the leader at the moment.");
                return new UpdateFileResponse { Status = false, Message = "Can't get requests from cloud, this server is not the leader at the moment." };

            }
            catch (Exception ex)
            {
                context.Status = new Status(StatusCode.Internal, $"Error uploading file: {ex.Message}");
                return new UpdateFileResponse { Status = false, Message = $"Error updating the file: {ex.Message}" }; ;
            }
        }

        

        private async Task<byte[]> GetFile(string fileId, int userId)
        {
            string folderPath = Path.Combine(Directory.GetCurrentDirectory(), OnMachineStorageActions._baseFolderName, userId.ToString(), fileId);
            if (OnMachineStorageActions.IsFolderEmpty(folderPath)) 
            {
                return await this._microservice.downloadFile(fileId);
            }

            return File.ReadAllBytes(Path.Combine(folderPath, $"{fileId}_{this._fileVersionManager.GetLatestFileVersion(fileId, userId)}"));
        }

        public override async Task DownloadFile(DownloadFileRequest request, IServerStreamWriter<DownloadFileResponse> responseStream, ServerCallContext context)
        {
            try
            {
                byte[] file = await GetFile(request.FileId, request.UserId);
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
            catch (DirectoryNotFoundException ex)
            {
                context.Status = new Status(StatusCode.NotFound, "The Requested file doesn't exist");
                await responseStream.WriteAsync(new DownloadFileResponse { Status = false, Message = "The Requested file doesn't exist", FileContent = ByteString.Empty });
                return;
            }
            catch (Exception ex)
            {
                context.Status = new Status(StatusCode.Internal, $"Error downloading the file: {ex.Message}");
                await responseStream.WriteAsync(new DownloadFileResponse { Status = false, Message = $"Error downloading the file: {ex.Message}", FileContent = ByteString.Empty });
                return;
            }

        }

        public override async Task<DeleteFileResponse> DeleteFile(DeleteFileRequest request, ServerCallContext context)
        {
            try
            {
                const string operationName = "DeleteFile";                
                string args = $"[{request.UserId},{request.FileId}]";
                LogEntry entry = new LogEntry(GetLastIndex() + 1, GetServerIP(), operationName, args);
                if (!this._raft.appendEntry(entry))
                {
                    context.Status = new Status(StatusCode.PermissionDenied, "Can't get requests from cloud, this server is not the leader at the moment.");
                    return new DeleteFileResponse { Status = false, Message = "Can't get requests from cloud, this server is not the leader at the moment." }; ;
                }
                string folderPath = Path.Combine(Directory.GetCurrentDirectory(), OnMachineStorageActions._baseFolderName, request.UserId.ToString(), request.FileId);
                Console.Write(folderPath);

                if (!Directory.Exists(folderPath))
                {
                     Console.Write("BBBB\n");

                    context.Status = new Status(StatusCode.NotFound, "The Requested file doesn't exist");
                    return new DeleteFileResponse { Status = false, Message = "The Requested file doesn't exist" };
                }
                
                Directory.Delete(folderPath, true);

                this._fileVersionManager.RemoveAllFileVersions(request.FileId, request.UserId);


                return new DeleteFileResponse { Status = true, Message = "File deleted successfully." };
            }
            catch (Exception ex)
            {
                context.Status = new Status(StatusCode.Internal, $"Error deleting the file: {ex.Message}");
                return new DeleteFileResponse { Status = false, Message = $"Error deleting the file: {ex.Message}" };
            }

        }
    }
}