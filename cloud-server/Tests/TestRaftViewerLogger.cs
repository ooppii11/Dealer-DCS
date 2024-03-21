using cloud_server.Managers;
using cloud_server.Utilities;
using GrpcCloud;
using Grpc;
/*
public class TestRaftViewerLogger
{
    public static void Main(string[] args)
    {
        RaftViewerLogger _raftLogger = new RaftViewerLogger("LeaderLog.log");
        _raftLogger.insertEntry(new LeaderToViewerHeartBeatRequest
        {
            LeaderIP = "127.0.0.1::5555",
            Term = 1,
            SystemLastIndex = 0,
        });
        Console.WriteLine(_raftLogger.getCurrLeaderIP());
    }
}
*/