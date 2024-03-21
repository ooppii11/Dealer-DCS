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
            //check if file that the folder and file are not deleted, take the correct version of the file, remove all the previse versions from db, delete the file and the previse versions on the machine, save the file to google cloud using the microservice
            //if folder is deleted it's ok
            //if file is deleted it's ok
            //if can't upload using microservice it's not ok
            return true;
        }

        private bool UpdateFile(int userId, string fileId, string type, int version)
        {
            //check if file that the folder and file are not deleted, take the correct version of the file, remove all the previse versions from db, delete the file and the previse versions on the machine, delete the privies version using the microservice, save the new version of the file to google cloud using the microservice
            //if folder is deleted it's ok
            //if file is deleted it's ok
            //if can't upload using microservice it's not ok
            return true;
        }

        private bool DownloadFile(string fileId)
        {
            return true;
        }

        private bool DeleteFile(string fileId) 
        {
            //delete using the microservice
            return true;
        }
    }
}
