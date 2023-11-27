using Grpc.Core;
using Grpc.Net.Client;
using GrpcFileCloudAccessClient;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;


public class FileSaving
{
    private Grpc.Core.Channel channel;
    private FileCloudAccess.FileCloudAccessClient client;
    public FileSaving()
    {
        try
        {
            channel = new Grpc.Core.Channel("[::]:50051", ChannelCredentials.Insecure);
            client = new FileCloudAccess.FileCloudAccessClient(channel);

        }
        catch (Exception ex)
        {
            throw new Exception("Cannot connect to the servise");
        }
    }

    public async Task<byte[]> downloadFile(string filename)
    {
        DownloadFileRequest request = new DownloadFileRequest { FileName = filename };
        try
        {
            using (var call = client.DownloadFile(request))
            {
                using (var memoryStream = new MemoryStream())
                {
                    while (await call.ResponseStream.MoveNext())
                    {
                        var chunk = call.ResponseStream.Current.FileData;
                        await memoryStream.WriteAsync(chunk.ToByteArray(), 0, chunk.Length);
                    }
                    return memoryStream.ToArray();
                }
            }
        }
        catch (Exception ex)
        {
            throw new Exception("Error download this file");
        }
    }

    public async Task<UploadFileResponse> uploadFile(string filename, byte[] fileData, string type)
    {
        try
        {
            IEnumerable<UploadFileRequest> requests = new[] { new UploadFileRequest() { FileName = filename, FileData = Google.Protobuf.ByteString.CopyFrom(fileData), Type = type } };
            var call = client.UploadFile();

            foreach (var request in requests)
            {
                await call.RequestStream.WriteAsync(request);
            }

            await call.RequestStream.CompleteAsync();

            var response = await call.ResponseAsync;
            return response;
        }
        catch
        {
            throw new Exception("Error upload file");
        }
        return new UploadFileResponse();
    }

    public void deleteFile(string filename)
    {
        DeleteFileRequest request = new DeleteFileRequest { FileName = filename };
        try
        {
            client.DeleteFile(request);
        }
        catch(Exception ex)
        {
            throw new Exception("Error deleting file");
        }
    }

}