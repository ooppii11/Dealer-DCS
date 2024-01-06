namespace node_server.Managers.Raft.State
{
    public abstract class State
    {
        protected RaftSettings _settings;
        protected Log _logger;
        public State(RaftSettings settings, Log logger) 
        {
            this._settings = settings;
            this._logger = logger;
        }
    }
}
