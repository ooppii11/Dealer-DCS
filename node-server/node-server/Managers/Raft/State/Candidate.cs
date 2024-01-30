using Grpc.Core;
using GrpcServerToServer;
using System.Runtime.CompilerServices;
using System.Threading;

namespace NodeServer.Managers.RaftNameSpace.States
{
    public class Candidate : State
    {
        public Candidate(RaftSettings settings, Log logger) :
            base(settings, logger)
        {
        }

        public async override Task<Raft.StatesCode> Start(CancellationToken cancellationToken)
        {
            if (await StartElection())
            {
                if (cancellationToken.IsCancellationRequested)
                {
                    return Raft.StatesCode.Follower;
                }
                return Raft.StatesCode.Leader;
            }
            return Raft.StatesCode.Follower;
        }

        private async Task<bool> StartElection()
        {
            Console.WriteLine("Start Election");
            int count = 1;
            int numOfDownServers = 0;
            this._settings.CurrentTerm++;
            this._settings.PreviousTerm++;
            Console.WriteLine($"My term (in leader election): {this._settings.CurrentTerm}");
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
                catch (RpcException e)
                {
                    if (e.StatusCode == StatusCode.Unavailable)
                    {
                        numOfDownServers++;
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.ToString());
                }
            }
            Console.WriteLine($"my count {count}, num of servers = {this._settings.ServersAddresses.Count()}");
            Console.WriteLine(numOfDownServers > (this._settings.ServersAddresses.Count() / 2) ? "Most of the servers in my group/cluster are down so election can't work" : "Election has run smoothly"); //Is it possible to change the algorithm so that it works according to the live servers? Then a leader is chosen for the whole system by the servers that are online...
            Task.Delay(1000).Wait();

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
    }
}