namespace NodeServer.Managers.Raft
{
    public class RaftSettings
    {
        public int CurrentTerm { get; set; } = 1;
        public int VotedFor { get; set; } = 0;
        public int CommitIndex { get; set; } = 0;
        public int LastApplied { get; set; } = 0;
        public TimeSpan ElectionTimeout { get; set; } = TimeSpan.FromMilliseconds(new Random().Next(150, 301));
        public TimeSpan HeartbeatTimeout { get;} = TimeSpan.FromMilliseconds(100);
        public int MaxLogEntriesPerRequest { get;} = 5;
        public List<string> ServersAddresses { get; set; } = (Environment.GetEnvironmentVariable("NODES_IPS"))?.Split(":")?.ToList();
        public int ServerId { get; set; } = Int32.Parse(Environment.GetEnvironmentVariable("NODE_SERVER_ID") ?? "0");
    }
}