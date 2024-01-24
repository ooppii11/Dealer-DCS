namespace NodeServer.Managers.RaftNameSpace
{
    public class RaftSettings
    {
        public int CurrentTerm { get; set; } = 1;
        public int PreviousTerm { get; set; } = 0;
        public int VotedFor { get; set; } = -1;
        public int CommitIndex { get; set; } = 0;
        public int LastApplied { get; set; } = 0;
        public int ElectionTimeout { get; } = (new Random().Next(150, 301));
        public int HeartbeatTimeout { get;} = 100;
        public int MaxLogEntriesPerRequest { get;} = 5;
        public string LogFilePath { get; set; } = "raftLog.log";
        public int ServersPort { get; set; } = 50052;
        public List<string> ServersAddresses { get; set; } = new List<string> { "127.0.0.1:1111" , "127.0.0.1:2222", "127.0.0.1:3333"};/*(Environment.GetEnvironmentVariable("NODES_IPS"))?.Split(":")?.ToList();*/
        public int ServerId { get; set; } = int.Parse(Environment.GetEnvironmentVariable("NODE_SERVER_ID") ?? "0");
        public string ServerAddres = "";
        public RaftSettings( string serverAddres)
        {
            ServerAddres = serverAddres;
        }
    }
}