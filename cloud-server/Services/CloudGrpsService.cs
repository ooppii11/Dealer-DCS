using Grpc.Core;
using GrpcCloud;

namespace cloud_server.Services
{   
    public class CloudGrpsService: Cloud.CloudBase
    {
        private readonly ILogger<CloudGrpsService> _logger;

        public CloudGrpsService(ILogger<CloudGrpsService> logger)
        {
            _logger = logger;
        }
      
        public override Task<signupResponse> signup(signupRequest request, ServerCallContext context)
        {
            // imploment
            return Task.FromResult(new signupResponse { SessionId = "1" });
        }

        public override Task<loginResponse> login(loginRequest request, ServerCallContext context)
        {
            // imploment

            return Task.FromResult(new loginResponse { SessionId = "1" });
        }
        public override Task<logoutResponse> logout(logoutRequest request, ServerCallContext context)
        {
            // imploment

            return Task.FromResult(new logoutResponse());

        }
    }
}
