using Google.Protobuf;
using Grpc.Core;
using GrpcNodeServer;
using NodeServer.Managers;
using GrpcServerToServer;
using NodeServer.Managers.RaftNameSpace;
using static NodeServer.Managers.RaftNameSpace.Raft;
using LogEntry = NodeServer.Managers.RaftNameSpace.LogEntry;
using System.Diagnostics;
using System.IO;
using static System.Data.Entity.Infrastructure.Design.Executor;

namespace NodeServer.Services
{
    public class NodeServerService : NodeServices.NodeServicesBase
    {
        private FileSaving _microservice;
        private Raft _raft;
        private FileVersionManager _fileVersionManager;
        private readonly string _folderName = "tempFiles";
        private readonly int _fixedUserStorageSpace = 100000000;//bytes = 100mb
        private readonly int _fixedUserTempStorageSpace = 10000000;//bytes = 10mb

        public NodeServerService(FileSaving micro, Raft raft, FileVersionManager fileVerM)
        {
            this._microservice = micro;
            this._raft = raft;
            this._fileVersionManager = fileVerM;

            string currentDirectory = Directory.GetCurrentDirectory();
            string folderPath = Path.Combine(currentDirectory, this._folderName);
            if (!Directory.Exists(folderPath))
            {
                Directory.CreateDirectory(folderPath);
            }
        }

        private static void SaveMemoryStreamToFile(MemoryStream memoryStream, string filePath)
        {
            using (FileStream fileStream = new FileStream(filePath, FileMode.Create))
            {
                memoryStream.CopyTo(fileStream);
            }
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

        private string SaveFile(string fileID, int userId, MemoryStream fileData)
        {
            string currentDirectory = Directory.GetCurrentDirectory();
            string folderPath = Path.Combine(currentDirectory, this._folderName, fileID);
            if (!Directory.Exists(folderPath))
            {
                Directory.CreateDirectory(folderPath);
            }

            string filePath = Path.Combine(folderPath, $"{fileID}_{this._fileVersionManager.GetLatestFileVersion(fileID, userId)}");

            SaveMemoryStreamToFile(fileData, filePath);
            return filePath;
        }
        private static long GetDirectorySize(string directoryPath)
        {
            long directorySize = 0;

            if (Directory.Exists(directoryPath))
            {
                string[] files = Directory.GetFiles(directoryPath);
                foreach (string file in files)
                {
                    FileInfo fileInfo = new FileInfo(file);
                    directorySize += fileInfo.Length;
                }
                string[] subdirectories = Directory.GetDirectories(directoryPath);
                foreach (string subdirectory in subdirectories)
                {
                    directorySize += GetDirectorySize(subdirectory);
                }
            }
            else
            {
                Console.WriteLine($"Directory {directoryPath} does not exist.");
            }

            return directorySize;
        }


        private async Task<string> UploadOperationArgsToString(IAsyncStreamReader<UploadFileRequest> requestStream)
        {
            string fileID = "";
            int userId = 0;
            string type = "";
            MemoryStream fileData = new MemoryStream();
            bool fileAlreadyExist = false;



            await foreach (var chunk in requestStream.ReadAllAsync())
            {
                fileID = chunk.FileId;
                type = chunk.Type;
                userId = chunk.UserId;
                fileData.Write(chunk.FileContent.ToArray(), 0, chunk.FileContent.Length);
            }

            if (fileData.Length + this._fileVersionManager.GetUserUsedSpace(userId) > this._fixedUserStorageSpace || //memory
                GetDirectorySize(Path.Combine(Directory.GetCurrentDirectory(), this._folderName, fileID)) > this._fixedUserTempStorageSpace)//temp memory
            {
                return null;
            }

            string filePath = SaveFile(fileID, userId, fileData);
            this._fileVersionManager.SaveFileVersion(userId, fileID, type, fileData.Length, filePath);
            return $"[{userId},{fileID},{type},{this._fileVersionManager.GetLatestFileVersion(fileID, userId)}]";
        }
    

        public override async Task<UploadFileResponse> UploadFile(IAsyncStreamReader<UploadFileRequest> requestStream, ServerCallContext context)
        {
            try
            {
                

                const string operationName = "UploadFile";
                string args = await UploadOperationArgsToString(requestStream);
                if (args == null) 
                {
                    context.Status = new Status(StatusCode.ResourceExhausted, "Can't upload the file. Either the file is too big or the user has used up all their memory.");
                    return new UploadFileResponse { Status = false, Message = "Can't upload the file. Either the file is too big or the user has used up all their memory." };
                }

                LogEntry entry = new LogEntry(GetLastIndex() + 1, GetServerIP(), operationName, args);
                if (await this._raft.appendEntry(entry))
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

        private static bool IsFolderEmpty(string path)
        { 
            if (!Directory.Exists(path))
            {
                throw new DirectoryNotFoundException($"Directory not found: {path}");
            }

            string[] files = Directory.GetFiles(path);

            return (files.Length == 0);
        }

        private async Task<byte[]> GetFile(string fileID, int userID)
        {
            string folderPath = Path.Combine(Directory.GetCurrentDirectory(), this._folderName, fileID);
            if (IsFolderEmpty(folderPath)) 
            {
                return await this._microservice.downloadFile(fileID);
            }

            return File.ReadAllBytes(Path.Combine(folderPath, $"{fileID}_{this._fileVersionManager.GetLatestFileVersion(fileID, userID)}"));
        }

        public override async Task DownloadFile(DownloadFileRequest request, IServerStreamWriter<DownloadFileResponse> responseStream, ServerCallContext context)
        {
            try
            {
                const string operationName = "DownloadFile";
                string args = $"[{request.FileId}]";
                LogEntry entry = new LogEntry(GetLastIndex() + 1, GetServerIP(), operationName, args);
                if (!await this._raft.appendEntry(entry))
                {
                    context.Status = new Status(StatusCode.PermissionDenied, "Can't get requests from cloud, this server is not the leader at the moment.");
                    return;
                }

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
                context.Status = new Status(StatusCode.DataLoss, "The Requested file doesn't exist");
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