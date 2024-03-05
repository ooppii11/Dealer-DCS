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
            Console.WriteLine(logFilePath);
            this._logFilePath = logFilePath;
            if (!File.Exists(this._logFilePath))
            {
                File.Create(this._logFilePath).Close();
            }
        }

        public void AppendEntry(LogEntry entry)
        {
            List<string> fileContent = new List<string>();
            if (File.Exists(this._logFilePath))
            {
                fileContent = File.ReadAllLines(this._logFilePath).ToList();
            }
            entry.Index = fileContent.Count();
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

        public LogEntry GetLogAtPlaceN(uint n)
        {
            string logLine = "";
            LogEntry entry;

            var logLines = File.ReadLines(this._logFilePath);
            if (logLines.Count() < n)
            {
                logLine = "";
                entry = new LogEntry(-1, DateTime.MinValue, "null", "null", "null", false);
            }
            else
            {
                logLine = logLines.ToArray()[n];
                entry = new LogEntry(logLine);
            }

            return entry;
        }
        public LogEntry GetLastLogEntry()
        {
            string logLine = "";
            LogEntry entry;

            var logLines = File.ReadLines(this._logFilePath);
            if (logLines.Count() == 0)
            {
                logLine = "";
                entry = new LogEntry(-1, DateTime.MinValue, "null", "null", "null", false);
            }
            else
            {
                logLine = logLines.Last();
                entry = new LogEntry(logLine);
            }

            return entry;
        }

    }
}
