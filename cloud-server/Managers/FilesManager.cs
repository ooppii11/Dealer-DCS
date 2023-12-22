using cloud_server.DB;
using cloud_server.Services;
using cloud_server.Utilities;

namespace cloud_server.Managers
{
    public class FilesManager
    {
        private FileMetadataDB _db;
        private NodeServerCommunication[] _nodes;

        public FilesManager(FileMetadataDB db, NodeServerCommunication[] nodes)
        {
            this._db = db;
            _nodes = nodes;
        }

        public async Task uploadFile(int userid, string filename, string type, long size, byte[] fileData)
        {
            FileMetadata file = new FileMetadata(userid, filename, type, (int)size);
            Location location = this.getLocation();
            int fileId = 0;

            this._db.uploadFileMetadata(file, location);
            fileId = this._db.getFileId(file.Name, file.CreatorId);

            // save the file
            await this._nodes[0].uploadFile($"{fileId}", fileData, type, location);
        }

        public void deleteFile(int userId, string filename)
        {
            int fileId = 0;

            fileId = this._db.getFileId(filename, userId);
            this._db.deleteFileMetadata(userId, filename);

            // Delete file from locations
            this._nodes[0].deleteFile($"{fileId}");
        }

        public GrpcCloud.FileMetadata getFileMetadata(int userId, string filename)
        {
            return Converter.ConvertToMessage(this._db.getFile(userId, filename));

        }
        public List<GrpcCloud.FileMetadata> getFilesMetadata(int userId)
        {
            return Converter.ConvertToMessage(this._db.getUserFilesMetadata(userId));
        }

        public async Task<byte[]> downloadFile(int userId, string filename)
        {
            int fileId = 0;

            fileId = this._db.getFileId(filename, userId);
            return await this._nodes[0].DownloadFile($"{fileId}");
        }
         

        private Location getLocation()
        {
            // decide where to save the file
            // Not implomented
            // string servers = "172.18.0.4:172.18.0.5:172.18.0.6"; //Environment.GetEnvironmentVariable("NODES_IPS");
            // string[] <> = servers.Split()
            // List<string> <> = servers.Split().ToList()
            return new Location("172.18.0.4", "172.18.0.5", "172.18.0.6");
        }
    }
}
