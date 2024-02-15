using Grpc.Core;
using GrpcCloud;
using GrpcServerToServer;

namespace NodeServer.Managers
{
    public class RaftViewerClient : IDisposable
    {
        private Grpc.Core.Channel channel;
        private Cloud.CloudClient client;
        public RaftViewerClient(string host, int port)
        {
            try
            {
                // Create Grpc connction:
                channel = new Channel($"{host}:{port}", ChannelCredentials.Insecure);
                client = new Cloud.CloudClient(channel);
            }
            catch (Exception ex)
            {
                throw new Exception("Cannot connect to the service");
            }
        }
        public RaftViewerClient(string address)
        {
            try
            {
                // Create Grpc connction:
                //channel = new Channel("127.0.0.1:1111", ChannelCredentials.Insecure);
                channel = new Channel($"{address}", ChannelCredentials.Insecure);
                client = new Cloud.CloudClient(channel);
            }
            catch (Exception ex)
            {
                throw new Exception("Cannot connect to the service");
            }
        }

        ~RaftViewerClient()
        {
            this.channel.ShutdownAsync().Wait();
        }

        public void Dispose()
        {
            this.channel.ShutdownAsync().Wait();
        }

        public async Task<LeaderToViewerHeartBeatResponse> ViewerUpdate(LeaderToViewerHeartBeatRequest request)
        {
            var response = await this.client.GetOrUpdateSystemLeaderAsync(request);
            return response;
        }
    }
}
