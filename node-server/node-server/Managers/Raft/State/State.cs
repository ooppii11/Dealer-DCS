using Grpc.Core;
using GrpcServerToServer;
using System.Threading;

namespace NodeServer.Managers.RaftNameSpace.States
{
    public abstract class State
    {
        protected RaftSettings _settings;
        protected Log _logger;
      
        public State(RaftSettings settings, Log logger) 
        {
            this._settings = settings;
            this._logger = logger;
        }

        public abstract Task<Raft.StatesCode> Start(CancellationToken cancellationToken);
    }
}
