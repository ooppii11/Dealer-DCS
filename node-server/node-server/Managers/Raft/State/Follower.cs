using Grpc.Core;
using GrpcServerToServer;
using System.Timers;

namespace NodeServer.Managers.RaftNameSpace.States
{
    public class Follower: State
    {
        private ManualResetEvent _stateChangeEvent;
        private System.Timers.Timer _timer;

        public Follower(RaftSettings settings, Log logger) :
            base(settings, logger)
        {
            this._stateChangeEvent = new ManualResetEvent(false);
            
        }

        ~Follower()
        {
            if (this._timer != null)
            {
                this._timer.Stop();
                this._timer.Dispose();
            }
        }
        private void StartTimer()
        {
            this._timer = new System.Timers.Timer();
            this._timer.Interval = this._settings.ElectionTimeout;
            this._timer.Elapsed += new ElapsedEventHandler(OnHeartBeatTimerElapsed);
            this._timer.Start();
        }

        public async override Task<Raft.StatesCode> Start()
        {
            this.StartTimer();
            this._stateChangeEvent.WaitOne();
            return Raft.StatesCode.Candidate;
        }

        private void OnHeartBeatTimerElapsed(object sender, ElapsedEventArgs e)
        {
            this._stateChangeEvent.Set();
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
