using Grpc.Core;
using GrpcRaft;

namespace node_server.Managers.Raft.States
{
    public class Candidate : State
    {
        bool _alradyVote;
        public Candidate(RaftSettings settings, Log logger) :
            base(settings, logger)
        {
            this._alradyVote = false;
        }

        public override Raft.StatesCode Start()
        {            
            return (elcted)? Raft.StatesCode.Candidate: Raft.StatesCode.Leader;
        }

        private bool StartElection()
        {
            bool elcted = false;
            // for(ip:this._settings.ips)
            // {
            // ip.sendrequestVote(this.RequestVote())
            // }
            return elcted;
        }
        public RequestVoteRequest RequestVote()
        {
            LogEntry lastEntry = this._logger.GetLastLogEntry();
            RequestVoteRequest request = new RequestVoteRequest()
            {
                LastLogIndex = lastEntry.Index,
                LastLogTerm = lastEntry.Term,
                // CandidateId = this._settings.myId;
                //Term = this._settings.currentTerm,

            };
            return request;
        }

        public override bool OnReceiveVoteRequest(RequestVoteRequest request)
        {
            if (this._logger.GetLastLogEntry().Index <= request.LastLogIndex)
            {
                // cancel my elction and return true
            }
            if (this._alradyVote)
            {
                return false;
            }
            this._alradyVote = true;
            return true;
        }


        public override AppendEntriesResponse OnReceiveAppendEntriesRequest(IAsyncStreamReader<AppendEntriesRequest> request)
        {
            return new AppendEntriesResponse();
        }
        public override InstallSnapshotResponse OnReceiveInstallSnapshotRequestRequest(IAsyncStreamReader<InstallSnapshotRequest> request)
        {
            return new InstallSnapshotResponse();
        }

    }
}
