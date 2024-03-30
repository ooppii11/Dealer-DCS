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
        private IDynamicActions _dynamicActions;

        public StatesCode RaftStateCode
        {
            get { return _currentStateCode; }
        }
        public State State { get { return _state; } }
        public RaftSettings Settings { get { return this._settings; } }

        public Raft(RaftSettings settings, FileSaving micro, FileVersionManager fileVerM)
        {
            this._dynamicActions = new DynamicStorageActionsManager(micro, fileVerM);

            this._currentStateCode = StatesCode.Follower;
            //this._currentStateCode = StatesCode.Leader;

            this._settings = settings;
            this._logger = new Log(this._settings.LogFilePath);
            this._cancellationTokenSource = new CancellationTokenSource();
            LogEntry entry = this._logger.GetLastLogEntry();
            this._settings.CurrentTerm = entry.Term;
            this._settings.LastLogIndex = entry.Index;
            if (entry.IsCommited()) { this._settings.CommitIndex = entry.Index; }
            else 
            {
                for (int i = entry.Index - 1; i > -1; i--)
                {
                    if (this._logger.GetLogAtPlaceN(i).IsCommited())
                    {
                        this._settings.CommitIndex = i;
                        break;
                    }
                }
            }

            this.Start();
        }

        public Raft(RaftSettings settings, FileSaving micro, FileVersionManager fileVerM, string folderName)
        {
            this._dynamicActions = new DynamicStorageActionsManager(micro, fileVerM, folderName);

            this._currentStateCode = StatesCode.Follower;
            //this._currentStateCode = StatesCode.Leader;

            this._settings = settings;
            this._logger = new Log(this._settings.LogFilePath);
            this._cancellationTokenSource = new CancellationTokenSource();
            LogEntry entry = this._logger.GetLastLogEntry();
            this._settings.CurrentTerm = entry.Term;
            this._settings.LastLogIndex = entry.Index;
            if (entry.IsCommited()) { this._settings.CommitIndex = entry.Index; }
            else
            {
                for (int i = entry.Index - 1; i > -1; i--)
                {
                    if (this._logger.GetLogAtPlaceN(i).IsCommited())
                    {
                        this._settings.CommitIndex = i;
                        break;
                    }
                }
            }

            this.Start();
        }

        public bool appendEntry(LogEntry entry, byte[] fileData)
        {
            if (this._currentStateCode == StatesCode.Leader)
            {
                Leader leaderObject = this._state as Leader;
                leaderObject.AppendEntries(entry, fileData);

                return true;
            }
            return false;
        }

        public bool appendEntry(LogEntry entry)
        {
            if (this._currentStateCode == StatesCode.Leader)
            {
                Leader leaderObject = this._state as Leader;
                leaderObject.AppendEntries(entry, new byte[0]);
                return true;
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
        public async Task<AppendEntriesResponse> OnReceiveAppendEntriesRequest(IAsyncStreamReader<AppendEntriesRequest> requests, string address)
        {
            _cancellationTokenSource.Cancel();
            this._settings.IsAppendEnteriesReset = true;
            //Console.WriteLine("resetting timer");
            int totalTerm = 0;
            int totalPrevIndex = 0;
            int totalPrevTerm = 0;
            int totalCommitIndex = 0;
            var totalLogEntries = new List<GrpcServerToServer.LogEntry>();
            MemoryStream fileData = new MemoryStream();


            /***
             add args
             ***/
            try
            {               
                await foreach (var request in requests.ReadAllAsync())
                {
                    totalTerm = request.Term;
                    totalPrevIndex = request.PrevIndex;
                    totalPrevTerm = request.PrevTerm;
                    totalCommitIndex = request.CommitIndex;
                    if (request.LogEntry != null)
                        totalLogEntries.Add(request.LogEntry);
                    fileData.Write(request.FileData.ToArray(), 0, request.FileData.Length);
                }
            }
            catch (Exception ex)
            {
                Console.Write(ex.Message);
                throw new Exception("Stream error");
            }

            if (totalLogEntries.Count > 0)
            {
                Console.WriteLine("address: " + address);
                Console.WriteLine("Total Term: " + totalTerm);
                Console.WriteLine("Total Previous Index: " + totalPrevIndex);
                Console.WriteLine("Total Previous Term: " + totalPrevTerm);
                Console.WriteLine("Total Commit Index: " + totalCommitIndex);

                // Print accumulated log entries
                Console.WriteLine($"Total Log Entries {totalLogEntries.Count()}");
                foreach (var logEntry in totalLogEntries)
                {
                    Console.WriteLine($"- Term: {logEntry.Term}, LogIndex: {logEntry.LogIndex}, Operation: {logEntry.Operation}, OperationArgs: {logEntry.OperationArgs}, Timestamp: {logEntry.Timestamp}");
                }
            }
            try
            {
                // sever was down, one or more logs are missing index
                if (totalCommitIndex > this._settings.CommitIndex + 1 || totalPrevIndex > this._settings.LastLogIndex)
                {
                    Console.WriteLine("totalCommitIndex: " + totalCommitIndex);
                    Console.WriteLine("_settings.totalCommitIndex: " + this._settings.CommitIndex);
                    Console.WriteLine("totalPrevIndex: " + totalPrevIndex);
                    Console.WriteLine("this._settings.LastLogIndex: " + this._settings.LastLogIndex);

                    Console.WriteLine("ERROR to append Entries");
                    return new AppendEntriesResponse() { MatchIndex = this._settings.LastLogIndex, Success = false, Term = this._settings.CurrentTerm };

                }

                // check for append new log line
                if (totalLogEntries.Count() > 0 && (totalLogEntries[0].LogIndex == 1 + this._settings.LastLogIndex))//|| this._settings.LastLogIndex == 0))
                {
                    Console.WriteLine("Append entries");
                    LogEntry entry = new LogEntry(
                                totalLogEntries[0].LogIndex,
                                totalLogEntries[0].Timestamp.ToDateTime(),
                                address,
                                totalLogEntries[0].Operation,
                                totalLogEntries[0].OperationArgs,
                           false
                        );
                    
                    bool result = false;
                    if (fileData.Length > 0)
                    {
                        Action commitAction = new Action(entry.Operation + "BeforeCommit", entry.OperationArgs, fileData.ToArray());
                        result = await this._dynamicActions.NameToAction(commitAction);
                    }
                    else 
                    {
                        Action commitAction = new Action(entry.Operation + "BeforeCommit", entry.OperationArgs);
                        result = await this._dynamicActions.NameToAction(commitAction);
                    }

                    if (result)
                    {
                        this._logger.AppendEntry(entry);
                        this._settings.LastLogIndex += 1;
                    }
                    else 
                    {
                        return new AppendEntriesResponse() { MatchIndex = this._settings.LastLogIndex, Success = false, Term = this._settings.CurrentTerm };
                    }
                    
                }

                // commit
                if (totalCommitIndex > this._settings.CommitIndex)
                {
                    Console.WriteLine("commit");
                    Console.WriteLine(totalCommitIndex);
                    LogEntry entry = this._logger.GetLogAtPlaceN(totalCommitIndex);
                    Action commitAction = new Action(entry.Operation + "AfterCommit", entry.OperationArgs);

                    // preform dynamic action after commit:
                    if (await this._dynamicActions.NameToAction(commitAction))
                    {
                        this._logger.CommitEntry(totalCommitIndex);
                        this._settings.CommitIndex = totalCommitIndex;
                    }
                    else
                    {
                        return new AppendEntriesResponse() { MatchIndex = this._settings.LastLogIndex, Success = false, Term = this._settings.CurrentTerm };
                    }
                }

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }

            return new AppendEntriesResponse()
            {
                MatchIndex = this._settings.LastLogIndex,
                Success = true,
                Term = this._settings.CurrentTerm
            };
            
        }

        public bool OnReceiveVoteRequest(RequestVoteRequest request)
        {
            if (!this._settings.LockLeaderFirstHeartBeat)
            {
                Console.WriteLine($"Voting");
                Console.WriteLine($"My Term: {this._settings.CurrentTerm}, Request Term: {request.Term}");
                _cancellationTokenSource.Cancel();
                //Console.WriteLine("resetting timer");
                if (this._logger.GetLastLogEntry().Index <= request.LastLogIndex && this._settings.CurrentTerm < request.Term)
                {
                    this._settings.PreviousTerm = this._settings.CurrentTerm;
                    this._settings.CurrentTerm = request.Term;
                    this._settings.VotedFor = request.CandidateId;
                    return true;
                }

                
            }

            Console.WriteLine("REJECT");
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
                        //Console.WriteLine("Follower");
                        this._state = new Follower(this._settings, this._logger);
                        this._currentStateCode = await this._state.Start(cancellationToken);
                        this._state = null;
                    }
                    else if (this._currentStateCode == StatesCode.Candidate)
                    {
                        //Console.WriteLine("Candidate");
                        this._state = new Candidate(this._settings, this._logger);
                        this._currentStateCode = await this._state.Start(cancellationToken);
                        this._state = null;
                        
                    }
                    else if (this._currentStateCode == StatesCode.Leader)
                    {
                        //Console.WriteLine("Leader");
                        this._settings.LockLeaderFirstHeartBeat = true;
                        this._state = new Leader(this._settings, this._logger, this._dynamicActions);
                        this._currentStateCode = await this._state.Start(cancellationToken);
                        this._state = null;
                    }
                    this._settings.LockLeaderFirstHeartBeat = false;
                    _cancellationTokenSource.Dispose();
                    _cancellationTokenSource = new CancellationTokenSource();
                }
                
            }    
        }
    }
}
