using System;
using System.IO;
using System.Threading.Tasks;
using Grpc.Core;
using GrpcServerToServer;
using NodeServer.Managers;


namespace NodeServer.Managers
{
    public class ServerToServerClient : IDisposable
    {
        private Grpc.Core.Channel channel;
        private ServerToServer.ServerToServerClient client;

        public ServerToServerClient(string host, int port)
        {
            try
            {
                // Create Grpc connction:
                channel = new Channel($"{host}:{port}", ChannelCredentials.Insecure);
                client = new ServerToServer.ServerToServerClient(channel);
            }
            catch (Exception ex)
            {
                throw new Exception("Cannot connect to the service");
            }
        }

        ~ServerToServerClient()
        {
            this.channel.ShutdownAsync().Wait();
        }

        public void Dispose()
        {
            this.channel.ShutdownAsync().Wait();
        }

        public async Task<PassFileResponse> passFile(string filename, string type, List<string> places, MemoryStream fileData)
        {
            try
            {
                // Upload chunks of file
                using (var call = client.PassFile())
                {
                    var buffer = new byte[1024 * 1024];
                    while (fileData.Position < fileData.Length)
                    {
                        var readCount = fileData.Read(buffer, 0, buffer.Length);
                        if (readCount <= 0)
                        {
                            break;
                        }
                        await call.RequestStream.WriteAsync(new PassFileRequest
                        {
                            FileId = filename,
                            Type = type,
                            FileContent = Google.Protobuf.ByteString.CopyFrom(buffer, 0, readCount),
                            ServersAddressesWhereSaved = { places }
                        });
                    }
                    await call.RequestStream.CompleteAsync();
                    var response = await call.ResponseAsync;
                    return response;
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Error occurred while uploading this file");
            }
        }
    }               
}
