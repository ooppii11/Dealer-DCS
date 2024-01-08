using Grpc.Core;
using GrpcRaft;

namespace NodeServer.Managers.Raft.States
{
    public class Follower: State
    {
        private bool _changeState;
        private ManualResetEvent _stateChangeEvent;

        public Follower(RaftSettings settings, Log logger) :
            base(settings, logger)
        {
            this._changeState = false;
            this._stateChangeEvent = new ManualResetEvent(false);
        }
        public override Raft.StatesCode Start()
        {
            this._stateChangeEvent.WaitOne();
            return Raft.StatesCode.Candidate;
        }

        public override bool OnReceiveVoteRequest(RequestVoteRequest request)
        {
            return true;
        }
        public override AppendEntriesResponse OnReceiveAppendEntriesRequest(IAsyncStreamReader<AppendEntriesRequest> request)
        {
           //if unvalid leader:  this._stateChangeEvent.Set();
            return new AppendEntriesResponse();
        }
        public override InstallSnapshotResponse OnReceiveInstallSnapshotRequestRequest(IAsyncStreamReader<InstallSnapshotRequest> request)
        {
            //if unvalid leader:  this._stateChangeEvent.Set();
            return new InstallSnapshotResponse();
        }

    }
}
