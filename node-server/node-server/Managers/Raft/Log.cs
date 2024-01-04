using System;
using static System.Net.Mime.MediaTypeNames;

namespace node_server.Managers.Raft
{
    public class Log
    {
        private string _logFilePath;

        public Log(string logFilePath)
        {
            this._logFilePath = logFilePath;
        }

        public void AppendEntry(LogEntry entry)
        {
            System.IO.File.AppendAllText(this._logFilePath, entry.ToString());
        }

        public void CommitEntry(int index)
        {
            string logLine = "";
            string[] fileContent;
            LogEntry entry;

            fileContent = File.ReadAllLines(this._logFilePath);

            logLine = fileContent[index];
            entry = new LogEntry(logLine);
            entry.SetCommit(true);

            File.WriteAllLines(this._logFilePath, fileContent);
        }

        public LogEntry GetLastLogEntry() 
        {
            /*
             *open log file
             *read last line to log entry
             *return log entry
             */
            string logLine = "";
            LogEntry entry;

            logLine = File.ReadLines(this._logFilePath).ElementAtOrDefault(-1);
            entry = new LogEntry(logLine);

            return entry;
        }

    }
}
