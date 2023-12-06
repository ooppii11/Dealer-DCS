using cloud_server.DB;
using cloud_server.Managers;

namespace cloud_server.Tests
{
    public class MetadataDBTest
    {
        private static int USER_ID = 1;
        private static string FILENAME = "test3";
        private static int FILE_SIZE = 0;
        private static string FILE_TYPE = "type";
        static void Main(string[] args)
        {
            FileMetadataDB db = new FileMetadataDB("./DB/tables.sql", "localhost", "postgres", "5432", "123456", "postgres");
            FileMetadata file = new FileMetadata(USER_ID, FILENAME, FILE_TYPE, FILE_SIZE);
            try
            {
                db.uploadFileMetadata(file);
                Console.WriteLine("---Upload File---");
                Console.WriteLine(db.getFile(USER_ID, FILENAME));
                Console.WriteLine("---Get File---");
                Console.WriteLine(db.getUserFilesMetadata(USER_ID));
                Console.WriteLine("---Get Files Of  User---");
                db.deleteFileMetadata(USER_ID, FILENAME);
                Console.WriteLine("---Delete File---");
            }
            catch 
            {
                Console.WriteLine("Test Faild");
            }

        }
    }
}
