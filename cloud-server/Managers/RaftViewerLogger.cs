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
        

        public RaftViewerLogger(string filePath)
        {
            _filePath = filePath;
            // Create the log file if it doesn't exist
            if (!File.Exists(_filePath))
            {
                using (File.Create(_filePath)) { }
            }
        }

        public void insertEntry(LeaderToViewerHeartBeatRequest entry)
        {
            List<string> fileContent = File.ReadAllLines(_filePath).ToList();
            if (fileContent.Any() && string.IsNullOrWhiteSpace(fileContent.Last()))
            {
                fileContent.RemoveAt(fileContent.Count - 1);
            }
            fileContent.Add(EntryObjToLogLine(entry));
            File.WriteAllLines(_filePath, fileContent);
        }

        public void insertInvalidLeader()
        {
            List<string> fileContent = File.Exists(_filePath) ? File.ReadAllLines(_filePath).ToList() : new List<string>();
            fileContent.Add("");
            File.WriteAllLines(_filePath, fileContent);
        }

        private string EntryObjToLogLine(LeaderToViewerHeartBeatRequest entry)
        {
            return $"{DateTimeOffset.Now.ToUnixTimeSeconds()}\t{entry.LeaderIP}\t{entry.Term}\t{entry.SystemLastIndex}";
        }

        private LeaderToViewerHeartBeatRequest LogLineToEntryObj(string logLine)
        {
            string[] parts = logLine.Split('\t');

            if (parts.Length == 4 && int.TryParse(parts[2], out int term) && int.TryParse(parts[3], out int systemLastIndex))
            {
                return new LeaderToViewerHeartBeatRequest
                {
                    LeaderIP = parts[1],
                    Term = term,
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
            string logLine = File.ReadLines(_filePath).LastOrDefault();
            
            if (logLine == null)
                throw new NoEntryException("No entry found. The system doesn't have any leader.");

            if (string.IsNullOrWhiteSpace(logLine))
                throw new EmptyEntryException("The last line is empty. The previous leader is no longer the leader, and there is no new leader.");

            return LogLineToEntryObj(logLine);
        }

        public string getCurrLeaderIP()
        {
            return getLastEntry().LeaderIP;
        }
    }
}
