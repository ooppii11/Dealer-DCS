using Grpc.Core;
using GrpcServerToServer;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using NodeServer.Managers.RaftNameSpace.States;
using System;
using System.Threading;
using Google.Protobuf.WellKnownTypes;

using static Grpc.Core.Metadata;

namespace NodeServer.Managers.RaftNameSpace
{
    public class Raft
    {
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
        public bool appendEntry(LogEntry entry)
        {
            if (this._currentStateCode == StatesCode.Leader)
            {
                Leader leaderObject = this._state as Leader;
                leaderObject.AppendEntries(entry);

            }
            return false;
        }

        ~Raft()
        {
            this._cancellationTokenSource?.Cancel();
            this._cancellationTokenSource?.Dispose();
        }

        public void Start()
        {
            Run();

        }
        public async Task<AppendEntriesResponse> OnReceiveAppendEntriesRequest(IAsyncStreamReader<AppendEntriesRequest> requests)
        {
            bool success = false;
            _cancellationTokenSource.Cancel();

            try
            {
                int totalTerm = 0;
                int totalPrevIndex = 0;
                int totalPrevTerm = 0;
                int totalCommitIndex = 0;
                var totalLogEntries = new List<GrpcServerToServer.LogEntry>();
                var totalArgs = new List<GrpcServerToServer.operationArgs>();

                await foreach (var request in requests.ReadAllAsync())
                {
                    totalTerm = request.Term;
                    totalPrevIndex = request.PrevIndex;
                    totalPrevTerm = request.PrevTerm;
                    totalCommitIndex = request.CommitIndex;
                    if (request.LogEntry != null)
                        totalLogEntries.Add(request.LogEntry);
                    totalArgs.Add(request.Args);
                }
                Console.WriteLine("Total Term: " + totalTerm);
                Console.WriteLine("Total Previous Index: " + totalPrevIndex);
                Console.WriteLine("Total Previous Term: " + totalPrevTerm);
                Console.WriteLine("Total Commit Index: " + totalCommitIndex);

                // Print accumulated log entries
                Console.WriteLine("Total Log Entries" + totalLogEntries.Count());
                foreach (var logEntry in totalLogEntries)
                {
                    Console.WriteLine($"- Term: {logEntry.Term}, LogIndex: {logEntry.LogIndex}, Operation: {logEntry.Operation}, OperationData: {logEntry.OperationData}, Timestamp: {logEntry.Timestamp}");
                }
                
                if(totalCommitIndex  == this._settings.LastLogIndex)
                {
                    if (this._settings.LastLogIndex != 0)
                    {
                        this._logger.CommitEntry(this._settings.LastLogIndex - 1);
                        this._settings.CommitIndex = this._settings.LastLogIndex;
                    }
                    success = true;
                }

                if (totalLogEntries.Count() > 0 && totalLogEntries[0].LogIndex > this._settings.LastLogIndex || this._settings.LastLogIndex == 0)
                {
                    Console.WriteLine("Append entries");

                    this._logger.AppendEntry(
                        new LogEntry(
                                totalLogEntries[0].LogIndex,
                                totalLogEntries[0].Timestamp.ToDateTime(),
                            "ip",
                                totalLogEntries[0].Operation,
                                totalLogEntries[0].OperationData,
                            totalCommitIndex == totalLogEntries[0].LogIndex
                        ));
                    this._settings.LastLogIndex += 1;
                    success = true;
                }
                

                return new AppendEntriesResponse() { MatchIndex = this._settings.LastLogIndex, Success = success, Term = this._settings.CurrentTerm };
            }
            catch (Exception ex)
            {
                Console.Write(ex.Message);
                throw new Exception("Stream error");
            }
        }

        /*public async Task<AppendEntriesResponse> OnReceiveAppendEntriesRequest(IAsyncStreamReader<AppendEntriesRequest> requests)
        {
           // int lastLogIndex = this._logger.GetLastLogEntry().Index;
            bool success = false;
            _cancellationTokenSource.Cancel();
            try
            {
                await foreach (var request in requests.ReadAllAsync())
                {
                    if (requests.Current.CommitIndex == this._settings.LastLogIndex)
                    {
                        if (this._settings.LastLogIndex != 0)
                        {
                            //Console.WriteLine("commit entreis");
                            //Console.WriteLine(this._settings.LastLogIndex - 1);
                            this._logger.CommitEntry(this._settings.LastLogIndex-1);
                            this._settings.CommitIndex = this._settings.LastLogIndex;
                        }
                        success = true;
                    }
                    Console.WriteLine(requests.Current.ToString());
                    Console.WriteLine(requests.MoveNext().ToString());

                    if (requests.Current.LogEntry != null && requests.Current.LogEntry.LogIndex > this._settings.LastLogIndex)
                    {
                        Console.WriteLine("append entreis");
                      
                        this._logger.AppendEntry(
                            new LogEntry(
                                requests.Current.LogEntry.LogIndex,
                                requests.Current.LogEntry.Timestamp.ToDateTime(),
                                "ip",
                                requests.Current.LogEntry.Operation,
                                requests.Current.LogEntry.OperationData,
                                requests.Current.CommitIndex == requests.Current.LogEntry.LogIndex
                            ));
                        this._settings.LastLogIndex += 1;
                        success = true;
                    }
                    return new AppendEntriesResponse() { MatchIndex = this._settings.LastLogIndex, Success = success, Term = this._settings.CurrentTerm };
                }
                Console.Write("stream error");
                throw new Exception("EMPTY STREAM");


            }
            catch (Exception ex)
            {
                Console.Write(ex.Message);

                throw new Exception("stream error");
            }
        }*/

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

                if (this._state == null)
                {
                    if (this._currentStateCode == StatesCode.Follower)
                    {
                        this._state = new Follower(this._settings, this._logger);
                        this._currentStateCode = await this._state.Start(cancellationToken);
                        this._state = null;
                    }
                    else if (this._currentStateCode == StatesCode.Candidate)
                    {
                        this._state = new Candidate(this._settings, this._logger);
                        this._currentStateCode = await this._state.Start(cancellationToken);
                        this._state = null;
                    }
                    else if (this._currentStateCode == StatesCode.Leader)
                    {
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
