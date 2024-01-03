namespace node_server.Managers.Raft.State
{
    public abstract class State
    {
        protected RaftSettings _settings;
        protected Log _logger;
        public State(RaftSettings settings) 
        {
            _settings = settings;
        }
    }
}
