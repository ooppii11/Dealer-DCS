using Grpc.Core;
using GrpcNodeServer;

namespace NodeServer.Services
{
    public class NodeServer : NodeServices.NodeServicesBase
    {
        public NodeServer() { }

        public override Task<UploadFileResponse> UploadFile(UploadFileRequest request, ServerCallContext context)
        {
            return base.UploadFile(request, context);
        }

        public override Task<DownloadFileResponse> DownloadFile(DownloadFileRequest request, ServerCallContext context)
        {
            return base.DownloadFile(request, context);
        }
        public override Task<DeleteFileResponse> DeleteFile(DeleteFileRequest request, ServerCallContext context)
        {
            return base.DeleteFile(request, context);
        }

        public override Task<UpdateFileResponse> UpdateFile(UpdateFileRequest request, ServerCallContext context)
        {
            return base.UpdateFile(request, context);
        }

        public override Task<ReplicateFilesResponse> WhereToReplicateFiles(ReplicateFilesRequest request, ServerCallContext context)
        {
            return base.WhereToReplicateFiles(request, context);
        }
    }
}
