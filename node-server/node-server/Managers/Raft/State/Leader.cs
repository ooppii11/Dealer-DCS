using System;
using System.Timers;
using Grpc.Core;
using GrpcServerToServer;
namespace NodeServer.Managers.Raft.States
{
    public class Leader: State
    {
        private AppendEntriesRequest lastHeartbeatMessage;
        private System.Timers.Timer _timer;
        private LogEntry lastLogEntry;
        private bool _changeState;
        private ManualResetEvent _stateChangeEvent;

        public Leader(RaftSettings raftSettings, Log logger):
            base(raftSettings, logger)
        {
            this._changeState = false;
            this._stateChangeEvent = new ManualResetEvent(false);
            this.lastHeartbeatMessage = new AppendEntriesRequest();
            // set lastHeartbeatMessage with defult values:
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
            this._timer.Interval = 150;
            this._timer.Elapsed += OnHeartBeatTimerElapsed;
            this._timer.AutoReset = true;
            // Start the timer
            this._timer.Start();

            this._stateChangeEvent.WaitOne();
            return Raft.StatesCode.Candidate;
        }
        private void OnHeartBeatTimerElapsed(object sender, ElapsedEventArgs e)
        {
            this.SendHeartbeatResponse();
        }

        private void SendHeartbeatResponse()
        {
            /*for (follwer:this._settings.follwers)
            {
                follwer.sendAppenEntries(this.lastHeartbeatMessage);
            }*/
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
            if (this._settings.VotedFor == 0)
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
        public override InstallSnapshotResponse OnReceiveInstallSnapshotRequestRequest(IAsyncStreamReader<InstallSnapshotRequest> request)
        { 
            return new InstallSnapshotResponse();
        }

    }
}
