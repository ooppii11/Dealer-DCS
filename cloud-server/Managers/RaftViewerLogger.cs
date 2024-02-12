using GrpcCloud;
using cloud_server.Utilities;
using Microsoft.Extensions.Logging.Abstractions;
using System;
using static Grpc.Core.Metadata;

namespace cloud_server.Managers
{
    public class RaftViewerLogger
    {
        private readonly string _filePath;


        public RaftViewerLogger(string filePath = "LeaderLog.log")
        {
            this._filePath = filePath;
            if (!File.Exists(this._filePath))
            {
                File.Create(this._filePath).Close();
            }
        }

        public void insertEntry(LeaderToViewerHeartBeatRequest entry)
        {
            List<string> fileContent = new List<string>();
            if (File.Exists(this._filePath))
            {
                fileContent = File.ReadAllLines(this._filePath).ToList();
            }
            if (fileContent.Last() == "")
            {
                fileContent.RemoveAt(fileContent.Count - 1);
            }
            fileContent.Add(entryObjToLogLine(entry));
            File.WriteAllLines(this._filePath, fileContent);
        }

        public void insertInvalidLeader()
        {
            List<string> fileContent = new List<string>();
            if (File.Exists(this._filePath))
            {
                fileContent = File.ReadAllLines(this._filePath).ToList();
            }
            fileContent.Add("");
            File.WriteAllLines(this._filePath, fileContent);
        }
        private string entryObjToLogLine(LeaderToViewerHeartBeatRequest entry)
        { 
            return (new DateTimeOffset(DateTime.Now).ToUnixTimeSeconds()).ToString() + "\t" + entry.LeaderIP + "\t" + (entry.Term).ToString() + "\t" + (entry.SystemLastIndex).ToString();
        }

        private LeaderToViewerHeartBeatRequest logLineToEntryObj(string logLine)
        {
            string[] parts = logLine.Split('\t');

            if (parts.Length == 4)
            {
                try
                {
                    string leaderIp = parts[1];
                    int term = int.Parse(parts[2]);
                    int systemLastIndex = int.Parse(parts[3]);

                    // Create LeaderHeartBeatRequest object
                    LeaderToViewerHeartBeatRequest entry = new LeaderToViewerHeartBeatRequest
                    {
                        LeaderIP = leaderIp,
                        Term = term,
                        SystemLastIndex = systemLastIndex
                    };

                    return entry;
                }
                catch (Exception ex)
                {
                    // Handle parsing errors or invalid log lines
                    Console.WriteLine($"Error parsing log line: {ex.Message}");
                    return null;
                }
            }
            else
            {
                // Handle incorrect number of fields in the log line
                Console.WriteLine("Invalid log line format");
                return null;
            }
        }


        public LeaderToViewerHeartBeatRequest getLastEntry()
        {
            LeaderToViewerHeartBeatRequest leaderHeartBeatRequest;
            string logLine = "";

            var logLines = File.ReadLines(this._filePath);
            if (logLines.Count() == 0)
            {
                throw new NoEntryException("No entry found. The system doesn't have any leader.");
            }

            logLine = logLines.Last();
            if (logLine == "")
            {
                throw new EmptyEntryException("The last line is empty. The previous leader is no longer the leader, and there is no new leader.");
            }

            leaderHeartBeatRequest = logLineToEntryObj(logLine);
            return leaderHeartBeatRequest;
        }


        public string getCurrLeaderIP()
        {
            return this.getLastEntry().LeaderIP;
        }
    }
}
