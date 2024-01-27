﻿using Grpc.Core;
using GrpcServerToServer;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using NodeServer.Managers.RaftNameSpace.States;
namespace NodeServer.Managers.RaftNameSpace
{
    public class Raft
    {
        //private readonly object _lockObject = new object();

        public enum StatesCode
        {
            Follower,
            Candidate,
            Leader
        }
        private StatesCode _currentStateCode;
        private State _state;
        private RaftSettings _settings;
        private Log _logger;

        public StatesCode RaftStateCode
        {
            get { return _currentStateCode; }
        }
        public State State { get { return _state; } }
        public RaftSettings Settings { get { return this._settings; } }

        public Raft(RaftSettings settings)
        {
            this._currentStateCode = StatesCode.Follower;
            this._settings = settings;
            this._logger = new Log(this._settings.LogFilePath);
            this.Start();
        }

        public void Start()
        {
            Thread thread = new Thread(new ThreadStart(Run));
            thread.Start();
        }
        public async Task<AppendEntriesResponse> OnReceiveAppendEntriesRequest(IAsyncStreamReader<AppendEntriesRequest> request)
        {
            
            int lastLogIndex = this._logger.GetLastLogEntry().Index;
            bool success = false;
            if (request.Current.CommitIndex == lastLogIndex)
            {
                this._logger.CommitEntry(lastLogIndex);
                this._settings.CommitIndex = lastLogIndex;
                success = true;
            }
            if (request.Current.LogEntry != null && request.Current.LogEntry.LogIndex > lastLogIndex)
            {

                this._logger.AppendEntry(
                    new LogEntry(
                        request.Current.LogEntry.LogIndex,
                        DateTime.Parse(request.Current.LogEntry.Timestamp.ToString()),
                        "ip",
                        request.Current.LogEntry.Operation,
                        request.Current.LogEntry.OperationData,
                        request.Current.CommitIndex == request.Current.LogEntry.LogIndex
                    ));
                lastLogIndex = request.Current.LogEntry.LogIndex;
                success = true;
            }

            this._currentStateCode = StatesCode.Follower;
            this._state = null;
            /*
            if the 2 lines above doesn't work, can try:
            
            this._currentStateCode = StatesCode.Follower;
            
            and if still doesn't work, we can add abort function in state (and follower, leader and candidate) that stop the start function
             */
            return new AppendEntriesResponse() { MatchIndex = lastLogIndex, Success = success, Term = this._settings.CurrentTerm};
        }

        public bool OnReceiveVoteRequest(RequestVoteRequest request)
        {
            if (this._logger.GetLastLogEntry().Index <= request.LastLogIndex && this._settings.CurrentTerm < request.Term)
            {
                this._settings.CurrentTerm = request.Term;
                this._settings.VotedFor = request.CandidateId;
                return true;
            }
            this._currentStateCode = StatesCode.Follower;
            this._state = null;
            return false;
        }

        public Task<InstallSnapshotResponse> OnReceiveInstallSnapshotRequest(IAsyncStreamReader<InstallSnapshotRequest> request)
        {
            return Task.FromResult(new InstallSnapshotResponse());
        }

        private async void Run()
        {
            while (true)
            {
                if (this._state == null)
                {
                    if (this._currentStateCode == StatesCode.Follower)
                    {
                        Console.WriteLine("Follower");
                        this._state = new Follower(this._settings, this._logger);
                        this._currentStateCode = await this._state.Start();
                        this._state = null;
                    }
                    else if (this._currentStateCode == StatesCode.Candidate)
                    {
                        Console.WriteLine("Candidate");

                        this._state = new Candidate(this._settings, this._logger);
                        this._currentStateCode = await this._state.Start();
                        this._state = null;
                    }
                    else if (this._currentStateCode == StatesCode.Leader)
                    {
                        Console.WriteLine("leader");
                        this._state = new Leader(this._settings, this._logger);
                        this._currentStateCode = await this._state.Start();
                        this._state = null;
                    }
                }
            }    
        }
    }
}
