using Grpc.Core;
using GrpcServerToServer;
using System;
using System.Timers;

namespace NodeServer.Managers.RaftNameSpace.States
{
    public class Follower: State
    {
        private System.Timers.Timer _timer;
        private TaskCompletionSource<bool> _completionSource;
        private bool _isCompleted = false;
        public Follower(RaftSettings settings, Log logger) :
            base(settings, logger)
        {
            
            if (this._settings.IsAppendEnteriesReset)
            {
                this._settings.ElectionTimeout = 2000;
            }
            else 
            {
                int addition = 0;
                if (new Random().Next(0, 2) == 0)
                {
                    addition = 200;
                    if (new Random().Next(0, 2) == 0)
                    {
                        addition = 400;
                        if (new Random().Next(0, 2) == 0)
                        {
                            addition = 600;
                        }
                    }
                }
                this._settings.ElectionTimeout = (new Random().Next(300, 4001)) + addition;
            }
            this._settings.IsAppendEnteriesReset = false;
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
            this._completionSource = new TaskCompletionSource<bool>();

            cancellationToken.Register(() =>
            {
                if (!_completionSource.Task.IsCompleted && !_isCompleted)
                {
                    _isCompleted = true;
                    this._completionSource.SetResult(true);
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
            if (!_completionSource.Task.IsCompleted && !_isCompleted)
            {
                _isCompleted = true;
                this._completionSource.SetResult(true);
                this._timer.Stop();
            }
        }
    }
}
