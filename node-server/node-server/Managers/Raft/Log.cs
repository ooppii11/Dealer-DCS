namespace node_server.Managers.Raft
{
    public class Log
    {
        private string _logFilePath;

        public Log(string logFilePath)
        {
            this._logFilePath = logFilePath;
        }

        public void AppendEntry(/*LogEntry entry*/)
        {
            /*
             *open file
               write last line entry.ToString();
             */
        }

        public void CommitEntry(int index)
        {
            /*
             open log file
            add commit sign
             */
        }

        public LogEntry GetLastLogEntry() 
        {
            /*
             *open log file
             *read last line to log entry
             *return log entry
             */
            return new LogEntry();
        }

    }
}
