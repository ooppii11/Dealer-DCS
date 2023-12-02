using Grpc.Core;
using GrpcCloud;

namespace cloud_server.Services
{   
    public class CloudGrpsService: Cloud.CloudBase
    {
        private AuthDB _db;
        private readonly ILogger<CloudGrpsService> _logger;

        public CloudGrpsService(ILogger<CloudGrpsService> logger, AuthDB db)
        {
            _logger = logger;
            _db = db;  
        }
      
        public override Task<signupResponse> signup(signupRequest request, ServerCallContext context)
        {
            try
            {
                this._db.signup(request.Username, request.Password, request.Email ,(request.PhoneNumber != "")? request.PhoneNumber : "NULL");
                
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
            try
            {
                if (this._db.login(request.Username, request.Password))
                {
                    return Task.FromResult(new loginResponse { SessionId = "", Status = GrpcCloud.Status.Success });
                }
                else
                {
                    return Task.FromResult(new loginResponse { SessionId = "", Status = GrpcCloud.Status.Failure });
                }
            }
            catch (Exception ex)
            {
                // Send Error response:
                return Task.FromResult(new loginResponse
                {
                    Status = GrpcCloud.Status.Failure,
                    SessionId = ""
                });

            }
        }
        public override Task<logoutResponse> logout(logoutRequest request, ServerCallContext context)
        {
            return Task.FromResult(new logoutResponse());
        }
    }
}
