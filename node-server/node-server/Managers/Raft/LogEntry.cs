﻿namespace NodeServer.Managers.RaftNameSpace
{
    public class LogEntry
    {
        private bool _commit;
        private int _index;

        private readonly int _term;

        private readonly DateTime _timestamp;

        private readonly string _leaderIp;

        private readonly string _operation;

        private readonly string _operationArgs;
       public int Index
        {
            get { return _index; }

            set { _index = value; }
        }
        
        public int Term => _term;
        
        public DateTime Timestamp => _timestamp;

        public string LeaderIp => _leaderIp;

        public string Operation => _operation;

        public string OperationArgs => _operationArgs;

        public LogEntry(int index, DateTime Timestamp, string leaderIp, string operation, string operationArgs, bool commit)
        {
            this._index = index;
            this._timestamp = Timestamp;
            this._leaderIp = leaderIp;
            this._operation = operation;
            this._operationArgs = operationArgs;
            this._commit = commit;
        }

        public LogEntry(int index, string leaderIp, string operation, string operationArgs)
        {
            this._index = index;
            this._timestamp = DateTime.UtcNow;
            this._leaderIp = leaderIp;
            this._operation = operation;
            this._operationArgs = operationArgs;
            this._commit = false;
        }

        public LogEntry(string logLine)
        {
            List<string> logParameters = logLine.Split("\t").ToList();

            this._index = Int32.Parse(logParameters[0]);
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
