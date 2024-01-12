using System;
using System.Net;
using System.Runtime.CompilerServices;
using System.Timers;
using Grpc.Core;
using GrpcServerToServer;
namespace NodeServer.Managers.Raft.States
{
    public class Leader: State
    {
        private Dictionary<string, AppendEntriesRequest> _heartbeatMessages;
        private System.Timers.Timer _timer;
        private LogEntry _lastLogEntry;
        private bool _changeState;
        private ManualResetEvent _stateChangeEvent;
        private readonly string _serverIP = Environment.GetEnvironmentVariable("NODE_SERVER_IP");

        public Leader(RaftSettings raftSettings, Log logger):
            base(raftSettings, logger)
        {
            this._settings.VotedFor = 0;
            this._changeState = false;
            this._stateChangeEvent = new ManualResetEvent(false);
            this._lastLogEntry = this._logger.GetLastLogEntry();
            foreach (string address in this._settings.ServersAddresses)
            {
                if (address != this._serverIP)
                {
                    this._heartbeatMessages[address] = new AppendEntriesRequest()
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

        public async override Task<Raft.StatesCode> Start()
        {
            this._timer = new System.Timers.Timer();
            this._timer.Interval = this._settings.HeartbeatTimeout;
            this._timer.Elapsed += new ElapsedEventHandler(OnHeartBeatTimerElapsed);
            this._timer.AutoReset = true;
            // Start the timer
            this._timer.Start();

            this._stateChangeEvent.WaitOne();
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
                if(address != this._serverIP)
                {
                    ServerToServerClient s2s = new ServerToServerClient(address, 50052);
                    AppendEntriesResponse response = await s2s.sendAppendEntriesRequest(this._heartbeatMessages[address]);
                }
            }
            
        }

        public void AppendEntries()
        {
            // change last log entry:
            // ...

            // create apeend entry request:
            //...

            /*for (follwer:this._settings.follwers)
            {
                follwer.sendAppenEntries(appenEntryMessege);
            }*/
        }

        public override bool OnReceiveVoteRequest(RequestVoteRequest request)
        {
            bool vote = false;
            if (this._settings.VotedFor != 0)
            {
                vote = false;
            }
            else if (this._logger.GetLastLogEntry().Index <= request.LastLogIndex && this._settings.CurrentTerm < request.Term)
            {
                this._settings.CurrentTerm = request.Term;
                this._settings.VotedFor = request.CandidateId;
                vote = true;
                //log action - vote for candidate - write current raft settings to log
            }
            else
            {
                this._stateChangeEvent.Set();
                throw new Exception("change from leader to follower");
            }
            return vote;
        }

        
        public void OnReceiveAppendEntriesResponse(AppendEntriesResponse response)
        {
            if(response.Success)
            {
                /*
           1. commit in leader log
           2. send to folower commit this log
           3. updare heart beat msg
               */
                this._logger.CommitEntry(response.MatchIndex);
                // 2.send to folower commit this log:
                // 3.updare heart beat msg:
            }        
            else
            {
             // re send append entry request
            }
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
