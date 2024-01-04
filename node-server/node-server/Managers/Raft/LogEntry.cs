namespace node_server.Managers.Raft
{
    public class LogEntry
    {
        private bool _commit;
        public int Index { get; }
        public int Term { get; }


        public LogEntry(string logLine)
        { 
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
            return "";
        }

    }
}
