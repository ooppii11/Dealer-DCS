using cloud_server.DB;
using cloud_server.Utilities;
using GrpcCloud;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion.Internal;

namespace cloud_server.Managers
{
    public class FilesManager
    {
        private FileMetadataDB _db;

        public FilesManager(FileMetadataDB db)
        {
            this._db = db;
        }

        public void uploadFile(int userid, string filename, string type, long size, byte[] fileData)
        {
            FileMetadata file = new FileMetadata(userid, filename, type, (int)size);
            Location location = this.getLocation();

            this._db.uploadFileMetadata(file, location);

            // save the file
        }

        public void deleteFile(int userId, string filename)
        {
            this._db.deleteFileMetadata(userId, filename);

            // Delete file from locations
        }

        public GrpcCloud.FileMetadata getFile(int userId, string filename)
        {
            return Converter.ConvertToMessage(this._db.getFile(userId, filename));


        }
        public List<GrpcCloud.FileMetadata> getFiles(int userId)
        {
            return Converter.ConvertToMessage(this._db.getUserFilesMetadata(userId));
        }

        public async Task<byte[]> downloadFile(int userId, string filename)
        {
            return new byte[1];
        }
         

        private Location getLocation()
        {
            // decide where to save the file
            // Not implomented yet
            return new Location("", "", "");
        }
    }
}
