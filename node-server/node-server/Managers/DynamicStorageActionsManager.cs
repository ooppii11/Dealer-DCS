using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace NodeServer.Managers
{
    public class DynamicStorageActionsManager : IDynamicActions
    {
        private readonly FileVersionManager _fileVersionManager;
        private readonly FileSaving _microservice;
        private readonly string _baseFolderName = "TempFiles";

        public DynamicStorageActionsManager(FileSaving micro, FileVersionManager fileVerM)
        {
            this._microservice = micro;
            this._fileVersionManager = fileVerM;
        }

        public override async Task<bool> NameToAction(Action ac)
        {
            Dictionary<string, Delegate> functionsWrapper = new Dictionary<string, Delegate>
        {
            { "UploadFileAfterCommit", new Func<int, string, string, int, Task<bool>>(UploadFileAfterCommit) },
            { "UpdateFileAfterCommit", new Func<int, string, string, int, Task<bool>>(UpdateFileAfterCommit) },
            { "DownloadFileAfterCommit", new Func<string, int, bool>(DownloadFileAfterCommit) },
            { "DeleteFileAfterCommit", new Func<string, int, bool>(DeleteFileAfterCommit) }
        };

            if (functionsWrapper.TryGetValue(ac.ActionName, out Delegate func))
            {
                if (func is Func<int, string, string, int, Task<bool>> asyncFunc)
                {
                    var task = func.DynamicInvoke(ac.Args) as Task<bool>;
                    return await task;
                }
                else if (func is Func<string, int, bool> syncFunc)
                {
                    return (bool)syncFunc.DynamicInvoke(ac.Args);
                }
                else
                {
                    throw new ArgumentException("Unsupported action delegate type");
                }
            }
            else
            {
                throw new ArgumentException("Action not found");
            }
        }

        private byte[] GetFile(string fileId, int version)
        {
            string dirPath = Path.Combine(Directory.GetCurrentDirectory(), this._baseFolderName, fileId);
            string filePath = Path.Combine(dirPath, $"{fileId}_{version}");
            if (Directory.Exists(dirPath) && File.Exists(filePath))
            {
                return File.ReadAllBytes(filePath);
            }
            return null;
        }

        private void RemovePreviseVersions(int userId, string fileId, int version)
        {
            this._fileVersionManager.RemovePreviousVersions(fileId, userId, version);

            string directoryPath = Path.Combine(Directory.GetCurrentDirectory(), this._baseFolderName, fileId);
            string[] previousVersionFiles = Directory.GetFiles(directoryPath, $"{fileId}_*");

            foreach (string file in previousVersionFiles)
            {
                int fileVersion;
                if (int.TryParse(Path.GetFileNameWithoutExtension(file).Split('_').Last(), out fileVersion))
                {
                    if (fileVersion < version)
                    {
                        File.Delete(file);
                    }
                }
            }
        }

        private void RemoveCurrentVersion(int userId, string fileId, int version)
        {
            this._fileVersionManager.RemoveVersion(fileId, userId, version);
            string filePath = Path.Combine(Directory.GetCurrentDirectory(), this._baseFolderName, fileId, $"{fileId}_{version}");
            File.Delete(filePath);
        }

        private async Task<bool> UploadFileAfterCommit(int userId, string fileId, string type, int version)
        {
            try
            {
                byte[] data = GetFile(fileId, version);
                if (data == null)
                {
                    return true;
                }
                await this._microservice.uploadFile(fileId, data, type);
                RemovePreviseVersions(userId, fileId, version);
                RemoveCurrentVersion(userId, fileId, version);
                return true;
            }
            catch
            {
                return false;
            }
        }

        private async Task<bool> UpdateFileAfterCommit(int userId, string fileId, string type, int version)
        {
            try
            {
                byte[] data = GetFile(fileId, version);
                if (data == null)
                {
                    return true;
                }
                this._microservice.deleteFile(fileId);
                await this._microservice.uploadFile(fileId, data, type);
                RemovePreviseVersions(userId, fileId, version);
                RemoveCurrentVersion(userId, fileId, version);
                return true;
            }
            catch
            {
                return false;
            }
        }

        private bool DownloadFileAfterCommit(string fileId, int userId)
        {
            return true;
        }

        private bool DeleteFileAfterCommit(string fileId, int userId)
        {
            try
            {
                this._microservice.deleteFile(fileId);
                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        
    }
}

