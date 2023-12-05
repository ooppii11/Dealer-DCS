using Grpc.Core;
using GrpcCloud;
using cloud_server.Managers;

namespace cloud_server.Services
{   
    public class CloudGrpsService: Cloud.CloudBase
    {
        private Authentication _auth;
        private readonly ILogger<CloudGrpsService> _logger;

        public CloudGrpsService(ILogger<CloudGrpsService> logger, Authentication auth)
        {
            this._logger = logger;
            this._auth = auth;  
        }
      
        public override Task<signupResponse> signup(signupRequest request, ServerCallContext context)
        {
            try
            {
                this._auth.Signup(request.Username, request.Password, request.Email ,(request.PhoneNumber != "")? request.PhoneNumber : "NULL");
                
                // Send Response:
                return Task.FromResult(new signupResponse
                {
                    Status = GrpcCloud.Status.Success,
                    Message = ""
                });

            }
            catch (Exception ex)
            {
                // Send Error response:
                return Task.FromResult(new signupResponse
                {
                    Status = GrpcCloud.Status.Failure,
                    Message = ex.Message
                });

            }
        }

        public override Task<loginResponse> login(loginRequest request, ServerCallContext context)
        {
            string sessionId = "";

            try
            {
                sessionId = this._auth.Login(request.Username, request.Password);
                return Task.FromResult(new loginResponse { SessionId = sessionId, Status = GrpcCloud.Status.Success });


            }
            catch (Exception ex)
            {
                // Send Error response:
                return Task.FromResult(new loginResponse
                {
                    Status = GrpcCloud.Status.Failure,
                    SessionId = ex.Message
                });

            }
        }
        public override Task<logoutResponse> logout(logoutRequest request, ServerCallContext context)
        {
            this._auth.Logout(request.SessionId);
            return Task.FromResult(new logoutResponse());
        }
    }
}
