using System;
using System.Net;
using System.Runtime.CompilerServices;
using System.Timers;
using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using GrpcServerToServer;
namespace NodeServer.Managers.RaftNameSpace.States
{
    class Node
    {
        private readonly string addres;
        private int _matchIndex;
        private int _commitIndex;
        private AppendEntriesRequest _request;

        public Node(string addres)
        {
            this.addres = addres;
            this._matchIndex = 0;
            this._commitIndex = 0;
        }

        public int CommitIndex
        {
            get => _commitIndex;
            set => _commitIndex = value;
        }

        public int MatchIndex
        {
            get => _matchIndex;
            set => _matchIndex = value;
        }

        public AppendEntriesRequest Request
        {
            get => _request;
            set => _request = value;
        }
    }
        public class Leader : State
    {
        private Dictionary<string, Node> _followers;
        private System.Timers.Timer _timer;
        private LogEntry _lastLogEntry;
        private bool _changeState;
        private CancellationToken _cancellationToken;
        private TaskCompletionSource<bool> _completionSource;
        public Leader(RaftSettings raftSettings, Log logger) :
            base(raftSettings, logger)
        {
            this._changeState = false;
            this._lastLogEntry = this._logger.GetLastLogEntry();
            this._followers = new Dictionary<string, Node>();
            this.InitHeartbeatMessages();
            LogEntry entry = new LogEntry(1, DateTime.UtcNow, "null", "null", "null", false);
            this.AppendEntries(entry);
        }

        private void InitHeartbeatMessages()
        {
            for (int i = 0; i < this._settings.ServersAddresses.Count; i++)
            {
                if (this._settings.ServersAddresses[i] != this._settings.ServerAddress)
                {
                    this._followers.Add(this._settings.ServersAddresses[i], new Node(this._settings.ServersAddresses[i]));
                    this._followers[this._settings.ServersAddresses[i]].Request =  new AppendEntriesRequest()
                    {
                        Term = this._settings.CurrentTerm,
                        PrevTerm = this._settings.PreviousTerm,
                        PrevIndex = _lastLogEntry.Index,
                        CommitIndex = (_lastLogEntry.IsCommited()) ? _lastLogEntry.Index : (_lastLogEntry.Index - 1 > 0) ? _lastLogEntry.Index : 0
                    };
                   
                }
            }
        }

        ~Leader()
        {
            if (this._timer != null)
            {
                this._timer.Stop();
                this._timer.Dispose();
            }
        }

        public async override Task<Raft.StatesCode> Start(CancellationToken cancellationToken)
        {
            this._cancellationToken = cancellationToken;
            this._completionSource = new TaskCompletionSource<bool>();


            this._timer = new System.Timers.Timer();
            this._timer.Interval = this._settings.HeartbeatTimeout;
            this._timer.Elapsed += new ElapsedEventHandler(OnHeartBeatTimerElapsed);
            this._timer.AutoReset = true;

            this._cancellationToken.Register(() =>
            {
                this._completionSource.SetResult(true);
            });

            this._timer.Start();

            await this._completionSource.Task;

            return Raft.StatesCode.Follower;
        }

        private void OnHeartBeatTimerElapsed(object sender, ElapsedEventArgs e)
        {
            this.SendHeartbeatRequest();
        }

        private async void SendHeartbeatRequest()
        {
            foreach (string address in this._settings.ServersAddresses)
            {
                if (address != this._settings.ServerAddress)
                {
                    try
                    {
                        ServerToServerClient s2s = new ServerToServerClient(address);
                        AppendEntriesResponse response = await s2s.sendAppendEntriesRequest(this._followers[address].Request);
                        this.OnReceiveAppendEntriesResponse(response, address);
                    }
                    catch (RpcException e)
                    {
                        if (e.StatusCode == StatusCode.Unavailable)
                        {
                            Console.WriteLine($"Server at {address} is Unavailable (down)");
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"error send to {address}");
                        Console.WriteLine(ex.ToString());
                    }
                }
            }
        }

        public async void AppendEntries(LogEntry entry)
        {
            this._logger.AppendEntry(entry);
            this._lastLogEntry = entry;

            Console.WriteLine("leader append entry to the log");
            this._settings.LastLogIndex += 1;

            foreach (string address in _followers.Keys.ToList())
            {
                Console.WriteLine($"{address}");
                this._followers[address].Request = new AppendEntriesRequest()
                {
                    Term = this._settings.CurrentTerm,
                    PrevTerm = this._settings.PreviousTerm,
                    PrevIndex = _lastLogEntry.Index,
                    CommitIndex = (_lastLogEntry.IsCommited()) ? _lastLogEntry.Index : (_lastLogEntry.Index - 1 > 0) ? _lastLogEntry.Index : 0,
                    LogEntry = new GrpcServerToServer.LogEntry()
                    {
                        LogIndex = _lastLogEntry.Index,
                        Operation = _lastLogEntry.Operation,
                        OperationData = _lastLogEntry.OperationArgs,
                        PrevLogIndex = (_lastLogEntry.Index - 1 >= 0) ? _lastLogEntry.Index - 1 : 0,
                        PrevTerm = this._settings.PreviousTerm,
                        Term = this._settings.CurrentTerm,
                        Timestamp = Timestamp.FromDateTime(this._lastLogEntry.Timestamp)
                    },

                    Args = new operationArgs() { Args = this._lastLogEntry.OperationArgs }
                };    
                ServerToServerClient s2s = new ServerToServerClient(address);
                AppendEntriesResponse response = await s2s.sendAppendEntriesRequest(this._followers[address].Request);
                this.OnReceiveAppendEntriesResponse(response, address);
            }
        }

        private bool MajorityAgreeOnMatchIndex(int matchIndexToCheck)
        {
            int nodesCount = _followers.Count + 1;
            int agreeCount = 0;

            if(this._lastLogEntry.Index >= matchIndexToCheck) { agreeCount++; }

            foreach (var follower in _followers.Values)
            {
                if (follower.MatchIndex >= matchIndexToCheck)
                {
                    agreeCount++;
                }
            }

            return agreeCount > nodesCount / 2;
        }

        public async void OnReceiveAppendEntriesResponse(AppendEntriesResponse response, string address)
        {
            ServerToServerClient s2s = new ServerToServerClient(address);

            if (response.Success)
            {
                if (this._followers[address].MatchIndex < response.MatchIndex)
                {
                    this._followers[address].MatchIndex = response.MatchIndex;
                }
                if(this._followers[address].MatchIndex != this._followers[address].CommitIndex && MajorityAgreeOnMatchIndex(response.MatchIndex))
                {
                    this._followers[address].CommitIndex = response.MatchIndex;

                    this._settings.CommitIndex = Math.Max(this._settings.CommitIndex, _lastLogEntry.Index);
                    this._logger.CommitEntry(this._settings.CommitIndex - 1);
                    this._lastLogEntry.SetCommit(true);
                    this._followers[address].Request.LogEntry = null;

                    await s2s.sendAppendEntriesRequest(this._followers[address].Request);
                }
            }
            else if (response.MatchIndex + 1 == _lastLogEntry.Index)
            {
                // send the previus message:
                await s2s.sendAppendEntriesRequest(this._followers[address].Request);
            }
            // else install snapshot

        }
    }
}
