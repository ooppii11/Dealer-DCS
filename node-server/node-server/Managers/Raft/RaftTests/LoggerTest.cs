namespace node_server.Managers.Raft.RaftTests
{
    public class LoggerTest
    {
        static void Main(string[] args)
        {
            Log logger = new Log("x.log");
            LogEntry logEntry = new LogEntry("1\t2024-01-05T15:30:00.000Z\t1\t127.0.0.1\topertion\topertionData\tfalse");

            logger.AppendEntry(logEntry);
            logger.CommitEntry(1);
            Console.WriteLine(logger.GetLastLogEntry().ToString());

        }
    }
}
