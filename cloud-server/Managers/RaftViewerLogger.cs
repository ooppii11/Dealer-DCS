using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using GrpcCloud;
using cloud_server.Utilities;

namespace cloud_server.Managers
{
    public class RaftViewerLogger
    {
        private readonly string _filePath;
        private static readonly object _fileLock = new object();

        public RaftViewerLogger(string filePath)
        {
            lock (RaftViewerLogger._fileLock)
            {
                _filePath = filePath;
                // Create the log file if it doesn't exist
                if (!File.Exists(_filePath))
                {
                    using (File.Create(_filePath)) { }
                }
            }  
        }

        public void insertEntry(LeaderToViewerHeartBeatRequest entry)
        {
            lock (RaftViewerLogger._fileLock)
            {
                List<string> fileContent = File.ReadAllLines(_filePath).ToList();
                if (fileContent.Any() && string.IsNullOrWhiteSpace(fileContent.Last()))
                {
                    fileContent.RemoveAt(fileContent.Count - 1);
                }
                fileContent.Add(EntryObjToLogLine(entry));
                File.WriteAllLines(_filePath, fileContent);
            }
        }

        public void insertInvalidLeader()
        {
            lock (RaftViewerLogger._fileLock)
            {
                List<string> fileContent = File.Exists(_filePath) ? File.ReadAllLines(_filePath).ToList() : new List<string>();
                if (!fileContent.Any() || string.IsNullOrWhiteSpace(fileContent.Last()))
                {
                    return;
                }
                fileContent.Add("");
                File.WriteAllLines(_filePath, fileContent);
            }
        }

        private string EntryObjToLogLine(LeaderToViewerHeartBeatRequest entry)
        {
            return $"{DateTimeOffset.Now.ToUnixTimeSeconds()}\t{entry.LeaderAddress}" /*+ $"\t{entry.Term}"*/ + $"\t{entry.SystemLastIndex}";
        }

        private LeaderToViewerHeartBeatRequest LogLineToEntryObj(string logLine)
        {
            string[] parts = logLine.Split('\t');

            if (parts.Length == 3/*4*/ && int.TryParse(parts[2], out int systemLastIndex) /*int.TryParse(parts[2], out int term) && int.TryParse(parts[3], out int systemLastIndex)*/)
            {
                return new LeaderToViewerHeartBeatRequest
                {
                    LeaderAddress = parts[1],
                    //Term = term,
                    SystemLastIndex = systemLastIndex
                };
            }
            else
            {
                Console.WriteLine("Invalid log line format");
                return null;
            }
        }

        public LeaderToViewerHeartBeatRequest getLastEntry()
        {
            string logLine = null;
            lock (RaftViewerLogger._fileLock)
            {
                logLine = File.ReadLines(_filePath).LastOrDefault();
            }

            if (logLine == null)
                throw new NoEntryException("No entry found. The system doesn't have any leader.");

            if (string.IsNullOrWhiteSpace(logLine))
                throw new EmptyEntryException("The last line is empty. The previous leader is no longer the leader, and there is no new leader.");

            return LogLineToEntryObj(logLine);
        }

        public string getCurrLeaderAddress()
        {
            return getLastEntry().LeaderAddress;
        }
    }
}
