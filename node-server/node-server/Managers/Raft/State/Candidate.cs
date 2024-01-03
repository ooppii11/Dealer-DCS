using GrpcRaft;

namespace node_server.Managers.Raft.State
{
    public class Candidate : State
    {
        bool _alradyVote;
        public Candidate(RaftSettings settings) :
            base(settings)
        {
            this._alradyVote = false;
        }

        public bool StartElection()
        {
            // for(ip:this._settings.ips)
            // {
            // ip.sendrequestVote(this.RequestVote())
            // }

            return false;
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

        public bool OnReceiveVoteResponse(RequestVoteRequest request)
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

        //public  OnReceiveAppendEntriesResponse()

    }
}
