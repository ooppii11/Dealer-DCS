using GrpcRaft;

namespace node_server.Managers.Raft.State
{
    public class Follower: State
    {
        public Follower(RaftSettings settings) :
            base(settings)
        { 
        }

        public AppendEntriesResponse AppendEntries(AppendEntriesRequest request)
        {
           // save log if need
           // commit if need
           // return response
            return new AppendEntriesResponse();
        }
        // public OnReceiveVoteRequest()

        // public OnReceiveAppendEntries()
    }
}
