namespace NodeServer.Managers.RaftNameSpace
{
    public class RaftSettings
    {
        public int CurrentTerm { get; set; } = 1;
        public int PreviousTerm { get; set; } = 0;
        public int VotedFor { get; set; } = -1;
        public int CommitIndex { get; set; } = 0;
        public int LastApplied { get; set; } = 0;
        public int ElectionTimeout { get; set; } = (new Random().Next(300, 3001));
        public int HeartbeatTimeout { get;} = 100;
        public int MaxLogEntriesPerRequest { get;} = 5;
        public string LogFilePath { get; set; } = "raftLog.log";
        public int ServersPort { get; set; } = 50052;
        public List<string> ServersAddresses { get; set; } = new List<string>();//(Environment.GetEnvironmentVariable("NODES_IPS"))?.Split(":")?.ToList();
        public string ServerAddress { get; set; } = "";// Environment.GetEnvironmentVariable("NODE_SERVER_IP");
        public int ServerId { get; set; } = -1;//int.Parse(Environment.GetEnvironmentVariable("NODE_SERVER_ID"));
    }
}