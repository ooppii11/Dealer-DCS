using Grpc.Core;
using GrpcServerToServer;
using System.Timers;

namespace NodeServer.Managers.Raft.States
{
    public class Follower: State
    {
        //private bool _changeState;
        private ManualResetEvent _stateChangeEvent;
        private System.Timers.Timer _timer;

        public Follower(RaftSettings settings, Log logger) :
            base(settings, logger)
        {
            //this._changeState = false;
            this._stateChangeEvent = new ManualResetEvent(false);
        }

        ~Follower()
        {
            this._timer.Stop();
            this._timer.Dispose();
        }

        public async override Task<Raft.StatesCode> Start()
        {
            this._timer = new System.Timers.Timer();
            this._timer.Interval = this._settings.ElectionTimeout;
            this._timer.Elapsed += new ElapsedEventHandler(OnHeartBeatTimerElapsed);
            this._timer.AutoReset = true;
            this._timer.Start();
            this._stateChangeEvent.WaitOne();
            return Raft.StatesCode.Candidate;
        }

        private void resetTimer()
        {
            this._timer.Stop();
            this._timer.Start();
        }   

        private void OnHeartBeatTimerElapsed(object sender, ElapsedEventArgs e)
        {
            this._stateChangeEvent.Set();
        }

        public override bool OnReceiveVoteRequest(RequestVoteRequest request)
        {
            return true;
        }
        public override AppendEntriesResponse OnReceiveAppendEntriesRequest(IAsyncStreamReader<AppendEntriesRequest> request)
        {
            //restart timer
           //if unvalid leader:  
            return new AppendEntriesResponse();
        }
        public override InstallSnapshotResponse OnReceiveInstallSnapshotRequest(IAsyncStreamReader<InstallSnapshotRequest> request)
        {
            //restart timer
            //if unvalid leader:  this._stateChangeEvent.Set();
            return new InstallSnapshotResponse();
        }

    }
}
