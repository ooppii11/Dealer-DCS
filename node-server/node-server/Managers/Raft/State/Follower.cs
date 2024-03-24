using Grpc.Core;
using GrpcServerToServer;
using System;
using System.Timers;

namespace NodeServer.Managers.RaftNameSpace.States
{
    public class Follower: State
    {
        private System.Timers.Timer _timer;
        private CancellationToken _cancellationToken;
        private TaskCompletionSource<bool> _completionSource;
        private readonly object _lockObject = new object();

        public Follower(RaftSettings settings, Log logger) :
            base(settings, logger)
        {
            
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
            this._timer.Interval = this._settings.ElectionTimeout + (new Random().Next(100, 1000));
            this._timer.Elapsed += new ElapsedEventHandler(OnHeartBeatTimerElapsed);
            this._timer.Start();
        }

        public async override Task<Raft.StatesCode> Start(CancellationToken cancellationToken)
        {
            this._cancellationToken = cancellationToken;
            this._completionSource = new TaskCompletionSource<bool>();

            this._cancellationToken.Register(() =>
            {
                lock (_lockObject)
                {
                    if (!_completionSource.Task.IsCompleted) 
                    {
                        this._completionSource.SetResult(true);
                    }
                }
            });
            StartTimer();

            await this._completionSource.Task;
            if (cancellationToken.IsCancellationRequested)
            {
                return Raft.StatesCode.Follower;
            }
            return Raft.StatesCode.Candidate;
        }

        private void OnHeartBeatTimerElapsed(object sender, ElapsedEventArgs e)
        {
            lock (_lockObject)
            {
                if (!_completionSource.Task.IsCompleted)
                {
                    this._completionSource.SetResult(true);
                }
            }
        }
    }
}
