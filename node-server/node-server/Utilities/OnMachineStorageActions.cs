using NodeServer.Managers;
using NodeServer.Managers.RaftNameSpace;

namespace NodeServer.Utilities
{
    public class OnMachineStorageActions
    {
        private static readonly string _baseFolderName = "TempFiles";
        private static readonly int _fixedUserStorageSpace = 100000000;//in bytes = 100mb
        private static readonly int _fixedUserTempStorageSpace = 100000000;//in bytes = 100mb
        public static void SaveMemoryStreamToFile(MemoryStream memoryStream, string filePath)
        {
            memoryStream.Position = 0;
            using (FileStream fileStream = new FileStream(filePath, FileMode.Create))
            {
                memoryStream.CopyTo(fileStream);
            }
        }

        public static long GetDirectorySize(string directoryPath)
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

        public static bool IsFolderEmpty(string path)
        {
            if (!Directory.Exists(path))
            {
                throw new DirectoryNotFoundException($"Directory not found: {path}");
            }

            string[] files = Directory.GetFiles(path);

            return (files.Length == 0);
        }

        public static bool SaveFile(string fileId, int userId, string type, MemoryStream fileData, FileVersionManager fileVersionManager)
        {
            if (fileData.Length + fileVersionManager.GetUserUsedSpace(userId, fileId) > OnMachineStorageActions._fixedUserStorageSpace || //memory
                OnMachineStorageActions.GetDirectorySize(Path.Combine(Directory.GetCurrentDirectory(), OnMachineStorageActions._baseFolderName, userId.ToString(), fileId)) + fileData.Length > OnMachineStorageActions._fixedUserTempStorageSpace) //temp memory
            {
                return false;
            }

            string currentDirectory = Directory.GetCurrentDirectory();
            string folderPath = Path.Combine(currentDirectory, OnMachineStorageActions._baseFolderName, userId.ToString(), fileId);
            if (!Directory.Exists(folderPath))
            {
                Directory.CreateDirectory(folderPath);
            }

            string filePath = Path.Combine(folderPath, $"{fileId}_{fileVersionManager.GetLatestFileVersion(fileId, userId) + 1}");

            OnMachineStorageActions.SaveMemoryStreamToFile(fileData, filePath);
            fileVersionManager.SaveFileVersion(userId, fileId, type, fileData.Length, filePath);
            return true;
        }

        public static bool DoesFileExist(int userId, string fileId)
        {
            string folderPath = Path.Combine(Directory.GetCurrentDirectory(), OnMachineStorageActions._baseFolderName, userId.ToString(), fileId);
            if (Directory.Exists(folderPath))
            {
                return true;
            }
            return false;
        }

        public static async Task<byte[]> GetFile(string opName, string opArgs, bool beforeCommit, FileSaving micro)
        {
            string[] argsList = OnMachineStorageActions.ParseLogEntryArgs(opArgs);
            if (opName == "UploadFile" || opName == "UpdateFile")
            {
                string folderPath = Path.Combine(Directory.GetCurrentDirectory(), OnMachineStorageActions._baseFolderName, argsList[0], argsList[1]);
                if (beforeCommit)
                {
                    return File.ReadAllBytes(Path.Combine(folderPath, $"{argsList[1]}_{argsList[2]}"));
                }
                return await micro.downloadFile(argsList[1]);
            }
            return new byte[0];
        }


        private static string[] ParseLogEntryArgs(string args)
        {
            return args.Trim('[', ']').Split(',');

        }
    }
}
