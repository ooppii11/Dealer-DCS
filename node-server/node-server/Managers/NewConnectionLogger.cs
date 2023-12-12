using Grpc.Core;
using Grpc.Core.Interceptors;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;

namespace NodeServer.Managers
{
    
    public class ConnectionLoggerInterceptor : Interceptor
    {
        private readonly ILogger<ConnectionLoggerInterceptor> _logger;

        public ConnectionLoggerInterceptor(ILogger<ConnectionLoggerInterceptor> logger)
        {
            _logger = logger;
        }

        public override Task<TResponse> UnaryServerHandler<TRequest, TResponse>(
            TRequest request,
            ServerCallContext context,
            UnaryServerMethod<TRequest, TResponse> continuation)
        {
            _logger.LogInformation($"Client connected: {context.Peer}");
            try
            {
                return base.UnaryServerHandler(request, context, continuation);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error while receiving call. Type/Method: {Type} / {Method}",
                                   MethodType.Unary, context.Method);
                throw;
            }
        }
    }
}
