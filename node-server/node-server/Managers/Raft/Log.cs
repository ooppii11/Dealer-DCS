using System;
using static System.Net.Mime.MediaTypeNames;
using System.Collections.Generic;

namespace NodeServer.Managers.RaftNameSpace
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
            List<string> fileContent = new List<string>();
            if (File.Exists(this._logFilePath))
            {
                fileContent = File.ReadAllLines(this._logFilePath).ToList();
            }
            fileContent.Add(entry.ToString());
            File.WriteAllLines(this._logFilePath, fileContent);
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
            fileContent[index] = entry.ToString();

            File.WriteAllLines(this._logFilePath, fileContent);
        }

        public LogEntry GetLastLogEntry() 
        {
            string logLine = "";
            LogEntry entry;

            logLine = File.ReadLines(this._logFilePath).Last();
            entry = new LogEntry(logLine);

            return entry;
        }

    }
}
