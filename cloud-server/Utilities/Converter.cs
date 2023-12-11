using Google.Protobuf.WellKnownTypes;

namespace cloud_server.Utilities
{
    public class Converter
    {
        public static GrpcCloud.FileMetadata ConvertToMessage(cloud_server.Managers.FileMetadata metadata)
        {
            return new GrpcCloud.FileMetadata()
            {
                Filename = metadata.Name,
                Size = metadata.Size,
                Type = metadata.Type,
                CreationDate = Timestamp.FromDateTime(metadata.CreationDate),
                LastModified = Timestamp.FromDateTime(metadata.LastModified)
            };
        }
        public static List<GrpcCloud.FileMetadata> ConvertToMessage(List<cloud_server.Managers.FileMetadata> metadata)
        {
            List<GrpcCloud.FileMetadata> fileMetadataList = new List<GrpcCloud.FileMetadata>();
            foreach (var metadataItem in metadata)
            {
                fileMetadataList.Add(ConvertToMessage(metadataItem));
            };

            return fileMetadataList;
        }
    }
}
