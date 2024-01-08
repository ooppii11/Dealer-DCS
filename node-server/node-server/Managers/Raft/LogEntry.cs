namespace NodeServer.Managers.Raft
{
    public class LogEntry
    {
        private bool _commit;
        public readonly int _index;

        public readonly int _term;

        public readonly DateTime _timestamp;

        public readonly string _leaderIp;

        public readonly string _operation;

        public readonly string _operationArgs;
        public int Index => _index;
        
        public int Term => _term;
        
        public DateTime Timestamp => _timestamp;

        public string LeaderIp => _leaderIp;

        public string Operation => _operation;

        public string OperationArgs => _operationArgs;


        public LogEntry(string logLine)
        {
            List<string> logParameters = logLine.Split("\t").ToList();

            this._index = int.Parse(logParameters[0]);
            this._timestamp = DateTime.Parse(logParameters[1]);
            this._term = int.Parse(logParameters[2]);
            this._leaderIp = logParameters[3];
            this._operation = logParameters[4];
            this._operationArgs = logParameters[5];
            this._commit = bool.Parse(logParameters[6]);
        }

        public bool IsCommited()
        {
            return this._commit;
        }

        public void SetCommit(bool isCommited)
        {
            this._commit = isCommited;
        }

        public override string ToString()
        {
            return $"{this._index}\t{this._timestamp.ToString("s") + "Z"}\t{this._term}\t{this._leaderIp}\t{this._operation}\t{this._operationArgs}\t{this._commit}";
        }
    }
}
