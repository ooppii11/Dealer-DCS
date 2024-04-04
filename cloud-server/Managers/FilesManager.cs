using cloud_server.DB;
using cloud_server.Services;
using cloud_server.Utilities;
using Grpc.Core;
using System.Data;
using static Google.Protobuf.Reflection.SourceCodeInfo.Types;

namespace cloud_server.Managers
{
    public class FilesManager
    {
        private FileMetadataDB _db;
        public string _leaderAddress;

        
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
            try 
            {
                FileMetadata file = new FileMetadata(userid, filename, type, (int)size); // create metadata for the file
                cloud_server.DB.Location location = this.getLocation(); // Find loactions for save this file.
                int fileId = 0;

                this._db.uploadFileMetadata(file, location);
                fileId = this._db.getFileId(file.Name, file.CreatorId);


                // save the file
                NodeServerCommunication client = new NodeServerCommunication(this._leaderAddress);
                await client.uploadFile(userid, $"{fileId}", fileData, type);
            }
            catch (RpcException ex)
            {
                this._db.deleteFileMetadata(userid, filename);
                //Console.WriteLine(ex);
                throw ex;
            }
            
        }

        public async Task updateFile(int userid, string filename, long size, byte[] fileData)
        {
            int fileId = 0;
            fileId = this._db.getFileId(filename, userid);
            NodeServerCommunication client = new NodeServerCommunication(this._leaderAddress);
            await client.updateFile(userid, $"{fileId}", fileData);
            this._db.updateFileMetadata(userid, filename, size);
        }

        public void deleteFile(int userId, string filename)
        {
            int fileId = 0;

            fileId = this._db.getFileId(filename, userId);
            this._db.deleteFileMetadata(userId, filename);

            // Delete file from locations
            (new NodeServerCommunication(this._leaderAddress)).deleteFile(userId, $"{fileId}");
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
            return await (new NodeServerCommunication(this._leaderAddress)).DownloadFile(userId, $"{fileId}");
        }
                 

        private cloud_server.DB.Location getLocation()
        {
            // decide where to save the file
            // Not implomented
            //return new Location("172.18.0.4", "172.18.0.5", "172.18.0.6");
            var addresses = (Environment.GetEnvironmentVariable("NODES_IPS"))?.Split(",")?.ToList();
            while (addresses.Count < 3)
            {
                addresses.Add("");
            }
            return new cloud_server.DB.Location(addresses[0], addresses[1], addresses[2]);
        }
    }
}
