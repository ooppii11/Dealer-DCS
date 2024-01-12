﻿using Grpc.Core;
using GrpcServerToServer;
using System.Timers;

namespace NodeServer.Managers.Raft.States
{
    public class Follower: State
    {
        private ManualResetEvent _stateChangeEvent;
        private System.Timers.Timer _timer;

        public Follower(RaftSettings settings, Log logger) :
            base(settings, logger)
        {
            this._stateChangeEvent = new ManualResetEvent(false);
            this._settings.VotedFor = 0;
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

        private void resetTimer()
        {
            if (this._timer != null)
            {
                this._timer.Stop();
                this._timer.Start();
            }
        }   

        private void OnHeartBeatTimerElapsed(object sender, ElapsedEventArgs e)
        {
            this._stateChangeEvent.Set();
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

            return vote;
        }
        public override AppendEntriesResponse OnReceiveAppendEntriesRequest(IAsyncStreamReader<AppendEntriesRequest> request)
        {
            //restart timer:
            this.resetTimer();

            // check if unvalid leader:
                // if unvlid send to sender error + this._stateChangeEvent.Set();


            // procss changes:

            // return response to leader
            return new AppendEntriesResponse();
        }
        public override InstallSnapshotResponse OnReceiveInstallSnapshotRequest(IAsyncStreamReader<InstallSnapshotRequest> request)
        {
            //restart timer
            this.resetTimer();

            //if unvalid leader:  this._stateChangeEvent.Set();

            // procss changes:

            return new InstallSnapshotResponse();
        }

    }
}
