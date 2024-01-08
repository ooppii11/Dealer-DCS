using System;
using System.Timers;
using Grpc.Core;
using GrpcRaft;
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

        public override Raft.StatesCode Start()
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
            if (request.LastLogIndex > this._logger.GetLastLogEntry().Index)
            {
                return true;
            }
            else
            {
                this._stateChangeEvent.Set();
                throw new Exception("change from leader to follower");
            }
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
