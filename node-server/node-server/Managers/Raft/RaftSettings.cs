namespace NodeServer.Managers.RaftNameSpace
{
    public class RaftSettings
    {
        public bool IsAppendEnteriesReset {get; set;} = false;
        public bool LockLeaderFirstHeartBeat { get; set; } = false;
        public int CurrentTerm { get; set; } = 1;
        public int PreviousTerm { get; set; } = 0;
        public int VotedFor { get; set; } = -1;
        public int CommitIndex { get; set; } = -1;
        public int LastLogIndex { get; set; } = -1;
        public int LastApplied { get; set; } = 0;
        //public int ElectionTimeout { get; set; } = (new Random(Environment.GetEnvironmentVariable("NODE_SERVER_ADDRESS").GetHashCode()).Next(300, 4001));
        public int ElectionTimeout { get; set; } = (new Random().Next(300, 4001));
        public int HeartbeatTimeout { get;} = 100;
        public int MaxLogEntriesPerRequest { get;} = 5;
        public string LogFilePath { get; set; } = "raftLog.log";
        public int ServersPort { get; set; } = 50052;
        public List<string> ServersAddresses { get; set; } = (Environment.GetEnvironmentVariable("NODES_ADDRESSES"))?.Split(",")?.ToList();
        public string ServerAddress { get; set; } = Environment.GetEnvironmentVariable("NODE_SERVER_ADDRESS");
        public int ServerId { get; set; } = int.Parse(Environment.GetEnvironmentVariable("NODE_SERVER_ID"));

        /*
        public RaftSettings()
        {
            foreach (var address in ServersAddresses)
            {
                ServersAddresses.Remove(address);
                ServersAddresses.Add($"{address}:{ServersPort}");
            }
            ServerAddress = $"{ServerAddress}:{ServersPort}";
        }
        */
    }
}