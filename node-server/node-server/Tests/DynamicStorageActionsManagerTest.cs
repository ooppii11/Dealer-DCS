using NodeServer.Managers;
/*
namespace NodeServer.Tests
{
    public class DynamicStorageActionsManagerTest
    {
        public static void Main(string[] args)
        {
            AsyncTest();
        }

        public async static void AsyncTest()
        {
            DynamicStorageActionsManager test = new DynamicStorageActionsManager(new FileSaving("127.0.0.1", 50051), new FileVersionManager("FileManager.db"));

            //await test.NameToAction(new NodeServer.Managers.Action("UploadFileBeforeCommit", "[1,test.txt,text,1]", new byte[0]));
            //await test.NameToAction(new NodeServer.Managers.Action("UpdateFileBeforeCommit", "[1,test.txt,2]", new byte[0]));
            //await test.NameToAction(new NodeServer.Managers.Action("DownloadFileBeforeCommit", "[1,test.txt]"));
            //await test.NameToAction(new NodeServer.Managers.Action("DeleteFileBeforeCommit", "[1,test.txt]"));

            await test.NameToAction(new NodeServer.Managers.Action("UploadFileAfterCommit", "[1,test.txt,text,1]"));
            await test.NameToAction(new NodeServer.Managers.Action("UpdateFileAfterCommit", "[1,test.txt,2]"));
            await test.NameToAction(new NodeServer.Managers.Action("DownloadFileAfterCommit", "[1,test.txt]"));
            await test.NameToAction(new NodeServer.Managers.Action("DeleteFileAfterCommit", "[1,test.txt]"));
        }


    }
}
*/