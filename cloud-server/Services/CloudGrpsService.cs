using Grpc.Core;
using GrpcCloud;
using System.Threading;

namespace cloud_server.Services
{
    public class CloudGrpsService: GrpcCloud.Cloud.CloudBase
    {
        private readonly ILogger<CloudGrpsService> _logger;

        public CloudGrpsService(ILogger<CloudGrpsService> logger)
        {
            _logger = logger;
        }

       
    }
}
