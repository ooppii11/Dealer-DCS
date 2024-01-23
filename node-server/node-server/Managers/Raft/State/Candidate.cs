using Grpc.Core;
using GrpcServerToServer;
using System.Runtime.CompilerServices;

namespace NodeServer.Managers.RaftNameSpace.States
{
    public class Candidate : State
    {
        public Candidate(RaftSettings settings, Log logger) :
            base(settings, logger)
        {
        }

        public async override Task<Raft.StatesCode> Start()
        { 
            return (await StartElection()) ? Raft.StatesCode.Follower : Raft.StatesCode.Leader;
        }

        private async Task<bool> StartElection()
        {
            int count = 1;
            this._settings.CurrentTerm++;
            this._settings.VotedFor = this._settings.ServerId;
            foreach (string address in this._settings.ServersAddresses)
            {
                ServerToServerClient s2s = new ServerToServerClient(address, 50052);
                RequestVoteResponse response = await s2s.sendNomination(this.RequestVote());
                if (response.Vote)
                {
                    count++;
                }
            }
            return this._settings.ServersAddresses.Count / 2 < count;
        }
        public RequestVoteRequest RequestVote()
        {
            LogEntry lastEntry = this._logger.GetLastLogEntry();
            RequestVoteRequest request = new RequestVoteRequest()
            {
                LastLogIndex = lastEntry.Index,
                LastLogTerm = lastEntry.Term,
                CandidateId = this._settings.ServerId,
                Term = this._settings.CurrentTerm,
            };
            return request;
        }

        public override bool OnReceiveVoteRequest(RequestVoteRequest request)
        {
            if (this._settings.CurrentTerm == request.Term && this._settings.VotedFor == 0)
            {
                return true;
            }
            else if (this._logger.GetLastLogEntry().Index <= request.LastLogIndex && this._settings.CurrentTerm < request.Term)
            {
                this._settings.CurrentTerm = request.Term;
                this._settings.VotedFor = request.CandidateId;
                return true;
            }
            return false;
        }
    }
}