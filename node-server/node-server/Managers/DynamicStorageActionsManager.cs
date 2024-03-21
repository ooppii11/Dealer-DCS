using System.Collections.Generic;
using System.Data.Entity.Core.Common.CommandTrees.ExpressionBuilder;
using System.Net;

namespace NodeServer.Managers
{
    public class DynamicStorageActionsManager
    {

        private FileVersionManager _fileVersionManager;
        private FileSaving _microservice;
        private readonly string _baseFolderName = "TempFiles";

        DynamicStorageActionsManager(FileSaving micro, FileVersionManager fileVerM)
        {
            this._microservice = micro;
            this._fileVersionManager = fileVerM;
        }

        public bool NameToAction(Action ac)
        {
            Dictionary<string, Delegate> functionsWrapper = new Dictionary<string, Delegate> 
            {
                { "UploadFile", UploadFile },
                { "UpdateFile", UpdateFile },
                { "DownloadFile", DownloadFile },
                { "DeleteFile", DeleteFile }
            };
            //new Func<int, string, string, int, bool>(UpdateFile)
            if (functionsWrapper.TryGetValue(ac.ActionName, out Delegate func))
            {
                return (bool)func.DynamicInvoke(ac.Args);
            }
            else
            {
                throw new ArgumentException("Action not found");
            }
        }

        private bool UploadFile(int userId, string fileId, string type, int version)
        {
            return true;
        }

        private bool UpdateFile(int userId, string fileId, string type, int version)
        {
            return true;
        }

        private bool DownloadFile(string fileId)
        {
            return true;
        }

        private bool DeleteFile(string fileId) 
        {
            return true;
        }
    }
}
