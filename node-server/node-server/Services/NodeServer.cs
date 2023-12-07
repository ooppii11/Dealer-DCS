using Grpc.Core;
using GrpcNodeServer;

namespace NodeServer.Services
{
    public class NodeServer : NodeServices.NodeServicesBase
    {
        private FileSaving microservice;
        public NodeServer() {
            microservice = new FileSaving("localhost", 50051);
        }

        public override async Task<UploadFileResponse> UploadFile(UploadFileRequest request, ServerCallContext context)
        {
            try
            {
                string fileName = null;
                MemoryStream fileData = new MemoryStream();

                await foreach (var chunk in request.ReadAllAsync())
                {
                    if (fileName == null)
                    {
                        fileName = chunk.file_id;
                    }

                    fileData.Write(chunk.file_content.ToArray(), 0, chunk.file_content.Length);
                }

                //fileData.ToArray()

                return new UploadFileResponse { Status = true, Message = "File uploaded successfully." };
            }
            catch (Exception ex)
            {
                return new UploadFileResponse { Status = false, Message = $"Error uploading file: {ex.Message}" };
            }
        }
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

/*
 public class FileUploadService : FileUpload.FileUploadBase
{
    public override async Task<FileResponse> UploadFile(IAsyncStreamReader<FileRequest> requestStream, ServerCallContext context)
    {
        
    }
}

 
 */
