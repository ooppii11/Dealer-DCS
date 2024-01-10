using Grpc.Core;
using GrpcServerToServer;
using System.Runtime.CompilerServices;

namespace NodeServer.Managers.Raft.States
{
    public class Candidate : State
    {
        public Candidate(RaftSettings settings, Log logger) :
            base(settings, logger)
        {
            this._settings.VotedFor = 0;
        }

        public async override Task<Raft.StatesCode> Start()
        {
            ;
            return (await StartElection()) ? Raft.StatesCode.Candidate : Raft.StatesCode.Leader;
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
            bool vote = false;
            if (this._settings.VotedFor != 0)
            {
                vote = false;
            }
            else if (this._logger.GetLastLogEntry()._index <= request.LastLogIndex && this._settings.CurrentTerm < request.Term)
            {
                this._settings.CurrentTerm = request.Term;
                this._settings.VotedFor = request.CandidateId;
                vote = true;
                //log action - vote for candidate - write current raft settings to log
            }

            return vote;
        }


        public override AppendEntriesResponse OnReceiveAppendEntriesRequest(IAsyncStreamReader<AppendEntriesRequest> request)
        {
            return new AppendEntriesResponse();
        }
        public override InstallSnapshotResponse OnReceiveInstallSnapshotRequest(IAsyncStreamReader<InstallSnapshotRequest> request)
        {
            return new InstallSnapshotResponse();
        }

    }
}