using cloud_server.DB;
using cloud_server.Services;
using cloud_server.Utilities;
using Grpc.Core;
using static Google.Protobuf.Reflection.SourceCodeInfo.Types;

namespace cloud_server.Managers
{
    public class FilesManager
    {
        private FileMetadataDB _db;
        private string _leaderAddress;

        public string LeaderAddress
        {
            get => _leaderAddress;
            set => _leaderAddress = value;
        }
        public FilesManager(FileMetadataDB db)
        {
            this._db = db;
            this._leaderAddress = "";
        }

        public FilesManager(FileMetadataDB db, string leaderIP)
        {
            this._db = db;
            this._leaderAddress = leaderIP;
        }

        public async Task uploadFile(int userid, string filename, string type, long size, byte[] fileData)
        {
            FileMetadata file = new FileMetadata(userid, filename, type, (int)size); // create metadata for the file
            cloud_server.DB.Location location = this.getLocation(); // Find loactions for save this file.
            int fileId = 0;

            this._db.uploadFileMetadata(file, location);
            fileId = this._db.getFileId(file.Name, file.CreatorId);


            // save the file
            NodeServerCommunication client = new NodeServerCommunication(this._leaderAddress);
            await client.uploadFile($"{fileId}", fileData, type, location);
        }

        public async Task updateFile(int userid, string filename, string type, long size, byte[] fileData)
        {
            int fileId = 0;
            fileId = this._db.getFileId(filename, userid);

            this._db.updateFileMetadata(userid, filename, size);

            NodeServerCommunication client = new NodeServerCommunication(this._leaderAddress);
            await client.updateFile($"{fileId}", fileData);
        }

        public void deleteFile(int userId, string filename)
        {
            int fileId = 0;

            fileId = this._db.getFileId(filename, userId);
            this._db.deleteFileMetadata(userId, filename);

            // Delete file from locations
            (new NodeServerCommunication(this._leaderAddress)).deleteFile($"{fileId}");
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
            return await (new NodeServerCommunication(this._leaderAddress)).DownloadFile($"{fileId}");
        }
                 

        private cloud_server.DB.Location getLocation()
        {
            // decide where to save the file
            // Not implomented
            //return new Location("172.18.0.4", "172.18.0.5", "172.18.0.6");
            return new cloud_server.DB.Location("127.0.0.1::1111", "127.0.0.1::2222", "127.0.0.1::3333");
        }
    }
}
