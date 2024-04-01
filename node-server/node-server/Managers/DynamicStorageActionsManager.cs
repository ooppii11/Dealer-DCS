using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using NodeServer.Utilities;

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

        public DynamicStorageActionsManager(FileSaving micro, FileVersionManager fileVerM, string baseFolderName)
        {
            this._microservice = micro;
            this._fileVersionManager = fileVerM;
            this._baseFolderName = baseFolderName;
        }

        public override ActionMaker getActionMaker()
        {
            return this._microservice;
        }

        public override async Task<bool> NameToAction(Action ac)
        {
            Dictionary<string, Delegate> functionsWrapper = new Dictionary<string, Delegate>
        {
            { "UploadFileAfterCommit", new Func<string, string, string, string, Task<bool>>(UploadFileAfterCommit) },
            { "UpdateFileAfterCommit", new Func<string, string, string, Task<bool>>(UpdateFileAfterCommit) },
            { "DeleteFileAfterCommit", new Func<string, string, bool>(DeleteFileAfterCommit) },

            { "UploadFileBeforeCommit", new Func<string, string, string, string, byte[], bool>(UploadFileBeforeCommit) },
            { "UpdateFileBeforeCommit", new Func<string, string, string, byte[], bool>(UpdateFileBeforeCommit) },
            { "DeleteFileBeforeCommit", new Func< string, string, bool >(DeleteFileBeforeCommit) },
        };

            if (functionsWrapper.TryGetValue(ac.ActionName, out Delegate func))
            {
                MethodInfo methodInfoBool = func.GetType().GetMethod("Invoke");
                if (methodInfoBool.ReturnType == typeof(Task<bool>))
                {
                    var task = func.DynamicInvoke(ac.Args) as Task<bool>;
                    return await task;
                }
                else if (methodInfoBool.ReturnType == typeof(bool))
                {
                    return (bool)func.DynamicInvoke(ac.Args);
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

        private byte[] GetFile(string userId, string fileId, int version)
        {
            string dirPath = Path.Combine(Directory.GetCurrentDirectory(), this._baseFolderName, userId, fileId);
            string filePath = Path.Combine(dirPath, $"{fileId}_{version}");
            if (Directory.Exists(dirPath) && File.Exists(filePath))
            {
                return File.ReadAllBytes(filePath);
            }
            return null;
        }

        private void RemovePreviseVersions(int userId, string fileId, int version)
        {
            string directoryPath = Path.Combine(Directory.GetCurrentDirectory(), this._baseFolderName, userId.ToString(), fileId);
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
            string filePath = Path.Combine(Directory.GetCurrentDirectory(), this._baseFolderName, userId.ToString(), fileId, $"{fileId}_{version}");
            File.Delete(filePath);
        }

        private async Task<bool> UploadFileAfterCommit(string strUserId, string fileId, string strVersion, string type)
        {
            try
            {
                int userId = Convert.ToInt32(strUserId);
                int version = Convert.ToInt32(strVersion);
                byte[] data = GetFile(strUserId, fileId, version);
                if (data == null)
                {
                    return true;
                }
                await this._microservice.uploadFile(fileId, data, type);
                RemovePreviseVersions(userId, fileId, version);
                RemoveCurrentVersion(userId, fileId, version);
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return false;
            }
        }

        private async Task<bool> UpdateFileAfterCommit(string strUserId, string fileId, string strVersion)
        {
            try
            {
                int userId = Convert.ToInt32(strUserId);
                int version = Convert.ToInt32(strVersion);
                string type = this._fileVersionManager.GetFileType(fileId, userId);
                byte[] data = GetFile(strUserId, fileId, version);
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
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return false;
            }
        }

        private bool DeleteFileAfterCommit(string userId, string fileId)
        {
            try
            {
                this._microservice.deleteFile(fileId);
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return false;
            }
        }

        private bool UploadFileBeforeCommit(string strUserId, string fileId, string type, string strVersion, byte[] fileData)
        {
            int userId = Convert.ToInt32(strUserId);
            if (!OnMachineStorageActions.SaveFile(fileId, userId, type, new MemoryStream(fileData), this._fileVersionManager))
            {
                return false;
            }
            return true;
        }

        private bool UpdateFileBeforeCommit(string strUserId, string fileId, string strVersion, byte[] fileData)
        {
            int userId = Convert.ToInt32(strUserId);
            string type = this._fileVersionManager.GetFileType(fileId, userId);
            if (type == null)
            {
                return true;
            }
            if (!OnMachineStorageActions.SaveFile(fileId, userId, type, new MemoryStream(fileData), this._fileVersionManager))
            {
                return false;
            }
            return true;
        }

        private bool DeleteFileBeforeCommit(string strUserId, string fileId)
        {
            int userId = Convert.ToInt32(strUserId);
            string folderPath = Path.Combine(Directory.GetCurrentDirectory(), this._baseFolderName, strUserId, fileId);
            if (!Directory.Exists(folderPath))
            {
                return false;
            }

            Directory.Delete(folderPath, true);
            this._fileVersionManager.RemoveAllFileVersions(fileId, userId);
            return true;
        }


    }
}

