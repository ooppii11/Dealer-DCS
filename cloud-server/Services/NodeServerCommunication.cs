using Grpc.Core;
using GrpcNodeServer;


namespace cloud_server.Services
{
    public class NodeServerCommunication
    {
        private Grpc.Core.Channel _channel;
        private GrpcNodeServer.NodeServices.NodeServicesClient _client;

        public NodeServerCommunication(string host, int port)
        {
            try
            {
                // Create Grpc connction:
                this._channel = new Channel($"{host}:{port}", ChannelCredentials.Insecure);
                this._client = new NodeServices.NodeServicesClient(this._channel);
            }
            catch (Exception ex)
            {
                throw new Exception("Cannot connect to the servise");
            }
        }

        ~NodeServerCommunication()
        {
            this._channel.ShutdownAsync().Wait();
        }

    }
}
