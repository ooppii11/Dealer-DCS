using Grpc.Core;
using GrpcServerToServer;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using NodeServer.Managers.RaftNameSpace.States;
using System;
using System.Threading;
using static Grpc.Core.Metadata;

namespace NodeServer.Managers.RaftNameSpace
{
    public class Raft
    {
        //private readonly object _lockObject = new object();
        private CancellationTokenSource _cancellationTokenSource;

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
            this._cancellationTokenSource = new CancellationTokenSource();
            this.Start();
        }

        ~Raft()
        {
            this._cancellationTokenSource?.Cancel();
            this._cancellationTokenSource?.Dispose();
        }

        public void Start()
        {
            //Thread thread = new Thread(() => Run());
            //thread.Start();
            Run();

        }
        public async Task<AppendEntriesResponse> OnReceiveAppendEntriesRequest(IAsyncStreamReader<AppendEntriesRequest> requests)
        {
            int lastLogIndex = this._logger.GetLastLogEntry().Index;
            bool success = false;
            _cancellationTokenSource.Cancel();
            try
            {
                await foreach (var request in requests.ReadAllAsync())
                {
                    if (requests.Current.CommitIndex == lastLogIndex)
                    {
                        if (lastLogIndex != 0)
                        {
                            this._logger.CommitEntry(lastLogIndex);
                            this._settings.CommitIndex = lastLogIndex;
                        }
                        success = true;
                    }

                    if (requests.Current.LogEntry != null && requests.Current.LogEntry.LogIndex > lastLogIndex)
                    {
                        this._logger.AppendEntry(
                            new LogEntry(
                                requests.Current.LogEntry.LogIndex,
                                DateTime.Parse(requests.Current.LogEntry.Timestamp.ToString()),
                                "ip",
                                requests.Current.LogEntry.Operation,
                                requests.Current.LogEntry.OperationData,
                                requests.Current.CommitIndex == requests.Current.LogEntry.LogIndex
                            ));
                        lastLogIndex = requests.Current.LogEntry.LogIndex;
                        success = true;
                    }
                    return new AppendEntriesResponse() { MatchIndex = lastLogIndex, Success = success, Term = this._settings.CurrentTerm };
                }
                throw new Exception("EMPTY STREAM");


            }
            catch (Exception ex)
            {

                throw new Exception("A");
            }
            
        }

        public bool OnReceiveVoteRequest(RequestVoteRequest request)
        { 
            Console.WriteLine($"Voting");
            Console.WriteLine($"My Term: {this._settings.CurrentTerm}, Request Term: {request.Term}");
            _cancellationTokenSource.Cancel();
            if (this._logger.GetLastLogEntry().Index <= request.LastLogIndex && this._settings.CurrentTerm < request.Term)
            {
                this._settings.PreviousTerm = this._settings.CurrentTerm;
                this._settings.CurrentTerm = request.Term;
                this._settings.VotedFor = request.CandidateId;
                return true;
            }
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
                CancellationToken cancellationToken = _cancellationTokenSource.Token;
                Console.WriteLine($"is Cancelled requested: { _cancellationTokenSource.Token.IsCancellationRequested.ToString()}");

                if (this._state == null)
                {
                    if (this._currentStateCode == StatesCode.Follower)
                    {
                        Console.WriteLine("Follower");
                        this._state = new Follower(this._settings, this._logger);
                        this._currentStateCode = await this._state.Start(cancellationToken);
                        this._state = null;
                    }
                    else if (this._currentStateCode == StatesCode.Candidate)
                    {
                        Console.WriteLine("Candidate");

                        this._state = new Candidate(this._settings, this._logger);
                        this._currentStateCode = await this._state.Start(cancellationToken);
                        this._state = null;
                    }
                    else if (this._currentStateCode == StatesCode.Leader)
                    {
                        Console.WriteLine("leader");
                        this._state = new Leader(this._settings, this._logger);
                        this._currentStateCode = await this._state.Start(cancellationToken);
                        this._state = null;
                    }
                    _cancellationTokenSource.Dispose();
                    _cancellationTokenSource = new CancellationTokenSource();
                }
                
            }    
        }
    }
}
