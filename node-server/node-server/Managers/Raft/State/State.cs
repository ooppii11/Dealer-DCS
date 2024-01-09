using Grpc.Core;
using GrpcServerToServer;

namespace NodeServer.Managers.Raft.States
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

        public abstract Task<Raft.StatesCode> Start();
        public abstract bool OnReceiveVoteRequest(RequestVoteRequest request);
        public abstract AppendEntriesResponse OnReceiveAppendEntriesRequest(IAsyncStreamReader<AppendEntriesRequest> request);
        public abstract InstallSnapshotResponse OnReceiveInstallSnapshotRequestRequest(IAsyncStreamReader<InstallSnapshotRequest> request);
    }
}
