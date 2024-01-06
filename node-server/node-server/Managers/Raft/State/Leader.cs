using System;
using System.Timers;
using GrpcRaft;
namespace node_server.Managers.Raft.State
{
    public class Leader: State
    {
        private AppendEntriesRequest lastHeartbeatMessage;
        private System.Timers.Timer _timer;
        private LogEntry lastLogEntry; 
        public Leader(RaftSettings raftSettings, Log logger):
            base(raftSettings, logger)
        {
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
       
        public void StartLeader()
        {
            this._timer = new System.Timers.Timer();
            this._timer.Interval = 150; 
            this._timer.Elapsed += OnHeartBeatTimerElapsed; 
            this._timer.AutoReset = true; // Timer will automatically restart

            // Start the timer
            this._timer.Start();            
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

        public void OnReceiveVoteRequest(RequestVoteRequest request)
        {
            if(request.LastLogIndex > this._logger.GetLastLogEntry().Index)
            {
                //send to canidte good vote;
                //leader die
            }
            else
            {
                //denie canidiat request and return bad vote;
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
    }
}
