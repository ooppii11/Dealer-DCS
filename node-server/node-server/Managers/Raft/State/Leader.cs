﻿using System;
using System.Collections;
using System.Net;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Timers;
using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using GrpcServerToServer;
using static Grpc.Core.Metadata;
using NodeServer.Utilities;

namespace NodeServer.Managers.RaftNameSpace.States
{
    class Node
    {
        private readonly string address;
        private int _matchIndex;
        private int _commitIndex;
        private AppendEntriesRequest _request;

        public Node(string address)
        {
            this.address = address;
            this._matchIndex = -1;
            this._commitIndex = -1;
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
        private readonly string _cloudAddress = "127.0.0.1:50053";
        private IDynamicActions _dynamicActions;
        public Leader(RaftSettings raftSettings, Log logger, IDynamicActions dynamicActions) :
            base(raftSettings, logger)
        {
            this._dynamicActions = dynamicActions;
            Console.WriteLine("leader");
            this._changeState = false;
            this._lastLogEntry = this._logger.GetLastLogEntry();
            this._followers = new Dictionary<string, Node>();
            this.InitHeartbeatMessages();
        }

        private void InitHeartbeatMessages()
        {
            foreach (string serverAddress in this._settings.ServersAddresses)
            {
                if (serverAddress != this._settings.ServerAddress)
                {
                    this._followers.Add(serverAddress, new Node(serverAddress));
                    this._followers[serverAddress].Request = new AppendEntriesRequest()
                    {
                        Term = this._settings.CurrentTerm,
                        PrevTerm = this._settings.PreviousTerm,
                        PrevIndex = _lastLogEntry.Index,
                        CommitIndex = this._settings.CommitIndex
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
                if (!this._completionSource.Task.IsCompleted)
                {
                    this._completionSource.SetResult(true);
                    this._timer.Stop();
                }
                
            });

            this._timer.Start();

            await this._completionSource.Task;

            return Raft.StatesCode.Follower;
        }

        private async void OnHeartBeatTimerElapsed(object sender, ElapsedEventArgs e)
        {
            if (!this._completionSource.Task.IsCompleted && this._cancellationToken.IsCancellationRequested)
            {
                this._completionSource.SetResult(true);
                this._timer.Stop();
            }
            await this.SendHeartbeatRequest();
        }

        private async Task SendHeartbeatRequest()
        {
            foreach (string address in _followers.Keys.ToList())
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
                }
            }
            await sendLeaderToViewerHeartBeat();
            this._settings.LockLeaderFirstHeartBeat = false;
        }

        private async Task sendLeaderToViewerHeartBeat()
        {
            try 
            {
                RaftViewerClient client = new RaftViewerClient(this._cloudAddress);
                await client.ViewerUpdate(
                    new GrpcCloud.LeaderToViewerHeartBeatRequest
                    {
                        LeaderAddress = this._settings.ServerAddress,
                        Term = this._settings.CurrentTerm,
                        SystemLastIndex = this._lastLogEntry.Index
                    });
            }
            catch (RpcException e)
            {
                if (e.StatusCode == StatusCode.Unavailable)
                {
                    Console.WriteLine($"Cloud Server at {this._cloudAddress} is Unavailable (down)");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"error when sending msg to cloud at: {this._cloudAddress}");
                Console.WriteLine(ex.ToString());
            }
        }

        public async Task AppendEntries(LogEntry entry, byte[] fileData)
        {
            this._timer.Stop();
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
                    PrevIndex = this._settings.LastLogIndex - 1,
                    CommitIndex = this._settings.CommitIndex,
                    LogEntry = new GrpcServerToServer.LogEntry()
                    {
                        PrevTerm = this._settings.PreviousTerm,
                        Term = this._settings.CurrentTerm,
                        PrevLogIndex = this._settings.LastLogIndex, //(_lastLogEntry.Index - 1 >= -1) ? _lastLogEntry.Index - 1 : -1,
                        LogIndex = _lastLogEntry.Index,
                        Timestamp = Timestamp.FromDateTime(this._lastLogEntry.Timestamp),
                        Operation = _lastLogEntry.Operation,
                        OperationArgs = _lastLogEntry.OperationArgs,
                        
                    },
                    FileData = Google.Protobuf.ByteString.CopyFrom(fileData)
                };

                Console.WriteLine(this._followers[address].Request.ToString());
                try
                {
                    ServerToServerClient s2s = new ServerToServerClient(address);
                    AppendEntriesResponse response = await s2s.sendAppendEntriesRequest(this._followers[address].Request);
                    Console.WriteLine($"sent new append entries to {address}");
                    this.OnReceiveAppendEntriesResponse(response, address);
                }
                catch (RpcException e)
                {
                    if (e.StatusCode == StatusCode.Unavailable)
                    {
                        Console.WriteLine($"Server at {address} is Unavailable (down)");
                    }
                }
                catch (Exception e) 
                {
                    Console.WriteLine($"error send append entries to {address}");
                }
            }
            this._timer.Start();
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

            return agreeCount + 1 > nodesCount / 2;
        }

        public async void OnReceiveAppendEntriesResponse(AppendEntriesResponse response, string address)
        {
            ServerToServerClient s2s = new ServerToServerClient(address);

            if (response.Success)
            {
                this._followers[address].Request.LogEntry = null;
                this._followers[address].MatchIndex = Math.Max(this._followers[address].MatchIndex, response.MatchIndex);

                if (this._followers[address].MatchIndex != this._followers[address].CommitIndex)
                {
                    if (MajorityAgreeOnMatchIndex(response.MatchIndex))
                    {
                        if(this._settings.CommitIndex < response.MatchIndex)
                        {
                            Console.WriteLine(response.MatchIndex);
                            this._settings.CommitIndex = response.MatchIndex;
                            Console.WriteLine($"leader commit index {this._settings.CommitIndex}");
                            LogEntry entry = this._logger.CommitEntry(this._settings.CommitIndex);

                            // preform dynamic action after commit:
                            Action commitAction = new Action(entry.Operation + "BeforeCommit", entry.OperationArgs);
                            await this._dynamicActions.NameToAction(commitAction);
                        }
                        this._followers[address].CommitIndex = response.MatchIndex;
                        this._followers[address].Request.CommitIndex = response.MatchIndex;
                    }
                }
                else if (response.MatchIndex < this._settings.LastLogIndex)
                {
                    LogEntry entry = this._logger.GetLogAtPlaceN(response.MatchIndex + 1);
                    Console.WriteLine(entry.Timestamp);
                    this._followers[address].Request = new AppendEntriesRequest()
                    {
                        Term = this._settings.CurrentTerm,
                        PrevTerm = this._settings.PreviousTerm,
                        PrevIndex = response.MatchIndex,
                        CommitIndex = Math.Min(this._settings.CommitIndex, response.MatchIndex),
                        LogEntry = new GrpcServerToServer.LogEntry()
                        {
                            PrevTerm = this._settings.PreviousTerm,
                            Term = this._settings.CurrentTerm,
                            PrevLogIndex = response.MatchIndex,
                            LogIndex = response.MatchIndex + 1,

                            Timestamp = Timestamp.FromDateTime(entry.Timestamp.ToUniversalTime()),
                            Operation = entry.Operation,
                            OperationArgs = entry.OperationArgs

                        },
                    };
                }
                try
                {
                    await s2s.sendAppendEntriesRequest(this._followers[address].Request);
                }
                catch (RpcException e)
                {
                    if (e.StatusCode == StatusCode.Unavailable)
                    {
                        Console.WriteLine($"Server at {address} is Unavailable (down)");
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine($"error send commit to {address}\n\n\n");
                    Console.WriteLine(e.Message);
                }

            }
            else
            {
                /*if (response.MatchIndex < this._settings.CommitIndex)
                {
                    //install snapshot
                }
                else*/if (response.MatchIndex < this._followers[address].Request.LogEntry.LogIndex)
                {
                    LogEntry entry = this._logger.GetLogAtPlaceN(response.MatchIndex + 1);
                    Console.WriteLine(entry.Timestamp);
                    this._followers[address].Request = new AppendEntriesRequest()
                    {
                        Term = this._settings.CurrentTerm,
                        PrevTerm = this._settings.PreviousTerm,
                        PrevIndex = response.MatchIndex,
                        CommitIndex = Math.Min(this._settings.CommitIndex, response.MatchIndex),
                        LogEntry = new GrpcServerToServer.LogEntry()
                        {
                            PrevTerm = this._settings.PreviousTerm,
                            Term = this._settings.CurrentTerm,
                            PrevLogIndex = response.MatchIndex,
                            LogIndex = response.MatchIndex + 1,

                            Timestamp = Timestamp.FromDateTime(entry.Timestamp.ToUniversalTime()),
                            Operation = entry.Operation,
                            OperationArgs = entry.OperationArgs

                        },
                        FileData = Google.Protobuf.ByteString.CopyFrom(await OnMachineStorageActions.GetFile(entry.Operation, entry.OperationArgs, (response.MatchIndex > this._settings.CommitIndex), this._dynamicActions.getActionMaker() as FileSaving))
                    };
                }
                    Console.WriteLine("not successful"); 
                // send the previous message:
               // await s2s.sendAppendEntriesRequest(this._followers[address].Request);
            }

        }
    }
}
