namespace NodeServer.Managers.Raft
{
    public class RaftSettings
    {
        private int _currentTerm;
        private int _votedFor;
        private int _commitIndex;
        private int _lastApplied;
        private TimeSpan _electionTimeout;
        private TimeSpan _heartbeatTimeout;
        private int _maxLogEntriesPerRequest;

        public RaftSettings()
        {
            Random random = new Random();
            this._currentTerm = 1;
            this._votedFor = 0;
            this. _commitIndex = 0;
            this._lastApplied = 0;
            this._electionTimeout = TimeSpan.FromMilliseconds(random.Next(150, 301)); // 150 - 300 ms - if after this time no heartbeat received, start election
            this._heartbeatTimeout = TimeSpan.FromMilliseconds(100); // 100 ms - send heartbeat every 100 ms
            this._maxLogEntriesPerRequest = 5;
        }

        public int CurrentTerm => _currentTerm;
        public int VotedFor => _votedFor;
        public int CommitIndex => _commitIndex;
        public int LastApplied => _lastApplied;
        public TimeSpan ElectionTimeout => _electionTimeout;
        public TimeSpan HeartbeatTimeout => _heartbeatTimeout;
        public int MaxLogEntriesPerRequest => _maxLogEntriesPerRequest;

    }
}
