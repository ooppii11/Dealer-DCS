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
            if (await StartElection())
            {
                return Raft.StatesCode.Leader;
            }
            return Raft.StatesCode.Follower;
        }

        private async Task<bool> StartElection()
        {
            int count = 1;
            this._settings.CurrentTerm++;
            this._settings.VotedFor = this._settings.ServerId;
            foreach (string address in this._settings.ServersAddresses)
            {
                try
                {
                    if (address != this._settings.ServerAddress)
                    {
                        //ServerToServerClient s2s = new ServerToServerClient($"{address}:{this._settings.ServersPort}");
                        ServerToServerClient s2s = new ServerToServerClient(address);
                        RequestVoteResponse response = await s2s.sendNomination(this.RequestVote());

                        if (response.Vote)
                        {
                            count++;
                        }
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.ToString());    
                }
            }
            Console.WriteLine($"my count {count}, num of servers = {this._settings.ServersAddresses.Count()}");
            Console.WriteLine(this._settings.ServersAddresses.Count() /2 < count);

            return this._settings.ServersAddresses.Count() / 2 < count;
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
           if (this._logger.GetLastLogEntry().Index <= request.LastLogIndex && this._settings.CurrentTerm < request.Term)
            {
                this._settings.CurrentTerm = request.Term;
                this._settings.VotedFor = request.CandidateId;
                return true;
            }
            return false;
        }
    }
}