using NodeServer.Managers;
/*
namespace NodeServer.Tests
{
    public class FileVersionManagerTest
    {
        public static void Main(string[] args)
        {
            FileVersionManager test = new FileVersionManager("FileManager.db");

            Console.WriteLine($"first user, first file");
            string fileName = "test.txt";
            string type = "text";
            int userId = 1;
            long size = 500;
            string filePath = @"test1\test";
            test.SaveFileVersion(userId, fileName, type, size, filePath);

            Console.WriteLine($"inserted first version");


            fileName = "test.txt";
            type = "text";
            userId = 1;
            size = 1000;
            filePath = @"test1\test";
            test.SaveFileVersion(userId, fileName, type, size, filePath);

            Console.WriteLine($"inserted second version");

            Console.WriteLine($"file type: {test.GetFileType(fileName, userId)}");
            Console.WriteLine($"last version: {test.GetLatestFileVersion(fileName, userId)}");
            Console.WriteLine($"used space: {test.GetUserUsedSpace(userId)}");
            Console.WriteLine($"used space beside the first file: {test.GetUserUsedSpace(userId, fileName)}");
            Console.WriteLine($"Number of files for user {userId}: {test.GetUserNumOfFiles(userId)}");

            Console.WriteLine($"first user, second file");

            fileName = "test2.txt";
            type = "text";
            userId = 1;
            size = 1000;
            filePath = @"test1\test2";
            test.SaveFileVersion(userId, fileName, type, size, filePath);

            Console.WriteLine($"inserted first version");

            Console.WriteLine($"used space {test.GetUserUsedSpace(userId)}");
            Console.WriteLine($"used space beside the first file {test.GetUserUsedSpace(userId, fileName)}");
            Console.WriteLine($"Number of files for user {userId}: {test.GetUserNumOfFiles(userId)}");

            fileName = "test.txt";
            type = "text";
            userId = 1;
            size = 1000;
            filePath = @"test1\test2";

            test.RemovePreviousVersions(fileName, userId, test.GetLatestFileVersion(fileName, userId));
            test.RemoveVersion(fileName, userId, test.GetLatestFileVersion(fileName, userId));

            Console.WriteLine($"second user, first file");

            fileName = "test.txt";
            type = "text";
            userId = 2;
            size = 500;
            filePath = @"test2\test";
            test.SaveFileVersion(userId, fileName, type, size, filePath);


            Console.WriteLine($"used space {test.GetUserUsedSpace(userId)}");
            Console.WriteLine($"used space beside the first file {test.GetUserUsedSpace(userId, fileName)}");
            Console.WriteLine($"Number of files for user {userId}: {test.GetUserNumOfFiles(userId)}");

            test.RemovePreviousVersions(fileName, userId, test.GetLatestFileVersion(fileName, userId));
            test.RemoveVersion(fileName, userId, test.GetLatestFileVersion(fileName, userId));

            fileName = "test2.txt";
            type = "text";
            userId = 1;
            size = 1000;
            filePath = @"test1\test2";

            test.RemovePreviousVersions(fileName, userId, test.GetLatestFileVersion(fileName, userId));
            test.RemoveVersion(fileName, userId, test.GetLatestFileVersion(fileName, userId));

        }
    }
}
*/