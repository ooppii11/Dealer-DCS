using System;
using System.Net;
using System.Runtime.CompilerServices;
using System.Timers;
using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using GrpcServerToServer;
namespace NodeServer.Managers.RaftNameSpace.States
{
    public class Leader : State
    {
        private Dictionary<string, AppendEntriesRequest> _heartbeatMessages;
        private System.Timers.Timer _timer;
        private LogEntry _lastLogEntry;
        private bool _changeState;
        private CancellationToken _cancellationToken;
        private TaskCompletionSource<bool> _completionSource;
        private readonly string _cloudAddress = "127.0.0.1:50053";
        public Leader(RaftSettings raftSettings, Log logger) :
            base(raftSettings, logger)
        {
            this._changeState = false;
            this._lastLogEntry = this._logger.GetLastLogEntry();
            this._heartbeatMessages = new Dictionary<string, AppendEntriesRequest>();
            this.InitHeartbeatMessages();
        }

        private void InitHeartbeatMessages()
        {
            for (int i = 0; i < this._settings.ServersAddresses.Count; i++)
            {
                if (this._settings.ServersAddresses[i] != this._settings.ServerAddress)
                {
                    this._heartbeatMessages.Add(this._settings.ServersAddresses[i], new AppendEntriesRequest()
                    {
                        Term = this._settings.CurrentTerm,
                        PrevTerm = this._settings.PreviousTerm,
                        PrevIndex = _lastLogEntry.Index,
                        CommitIndex = (_lastLogEntry.IsCommited()) ? _lastLogEntry.Index : (_lastLogEntry.Index - 1 > 0) ? _lastLogEntry.Index : 0
                    });
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
                    //ServerToServerClient s2s = new ServerToServerClient($"{address}:{this._settings.ServersPort}");
                    //ServerToServerClient s2s = new ServerToServerClient(address, 50052);
                    try
                    {
                        // Console.WriteLine($"send hert beat to {address}");
                        ServerToServerClient s2s = new ServerToServerClient(address);
                        AppendEntriesResponse response = await s2s.sendAppendEntriesRequest(this._heartbeatMessages[address]);
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
            sendLeaderToViewerHeartBeat();
        }

        private async void sendLeaderToViewerHeartBeat()
        {
            try 
            {
                RaftViewerClient client = new RaftViewerClient(this._cloudAddress);
                await client.ViewerUpdate(
                    new GrpcCloud.LeaderToViewerHeartBeatRequest
                    {
                        LeaderIP = this._settings.ServerAddress,
                        Term = this._settings.CurrentTerm,
                        SystemLastIndex = this._lastLogEntry.Index
                    });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"error send to cloud at: {this._cloudAddress}");
                Console.WriteLine(ex.ToString());
            }
        }

        public async void AppendEntries(LogEntry entry)
        {
            this._logger.AppendEntry(entry);
            this._lastLogEntry = entry;

            foreach (string address in _heartbeatMessages.Keys.ToList())
            {
                this._heartbeatMessages[address] = new AppendEntriesRequest()
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
                        Timestamp = this._lastLogEntry.Timestamp.ToTimestamp()
                    },

                    Args = new operationArgs() { Args = this._lastLogEntry.OperationArgs }
                };
                //ServerToServerClient s2s = new ServerToServerClient(address, 50052);
                ServerToServerClient s2s = new ServerToServerClient(address);
                AppendEntriesResponse response = await s2s.sendAppendEntriesRequest(this._heartbeatMessages[address]);
                this.OnReceiveAppendEntriesResponse(response, address);
            }
        }


        public async void OnReceiveAppendEntriesResponse(AppendEntriesResponse response, string address)
        {
            if (response.Success && response.MatchIndex == _lastLogEntry.Index)
            {
                this._logger.CommitEntry(_lastLogEntry.Index);
                this._settings.CommitIndex = _lastLogEntry.Index;
                this._heartbeatMessages[address].CommitIndex = _lastLogEntry.Index;
                this._heartbeatMessages[address].LogEntry = null;
                //ServerToServerClient s2s = new ServerToServerClient($"{address}:{this._settings.ServersPort}");
                //ServerToServerClient s2s = new ServerToServerClient(address, 50052);
                ServerToServerClient s2s = new ServerToServerClient(address);
                await s2s.sendAppendEntriesRequest(this._heartbeatMessages[address]);
            }
            else if (response.MatchIndex != _lastLogEntry.Index)
            {
                // send install sanpshot from response.MatchIndex
            }
            else
            {
                // send the previus message
                //ServerToServerClient s2s = new ServerToServerClient(address, 50052);
                ServerToServerClient s2s = new ServerToServerClient(address);
                await s2s.sendAppendEntriesRequest(this._heartbeatMessages[address]);
            }

        }
    }
}
