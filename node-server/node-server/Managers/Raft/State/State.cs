using Grpc.Core;
using GrpcRaft;

namespace node_server.Managers.Raft.States
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

        public abstract Raft.StatesCode Start();
        public abstract bool OnReceiveVoteRequest(RequestVoteRequest request);
        public abstract AppendEntriesResponse OnReceiveAppendEntriesRequest(IAsyncStreamReader<AppendEntriesRequest> request);
        public abstract InstallSnapshotResponse OnReceiveInstallSnapshotRequestRequest(IAsyncStreamReader<InstallSnapshotRequest> request);
    }
}
