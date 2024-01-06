namespace node_server.Managers.Raft
{
    public class RaftSettings
    {
        private int _currentTerm;

        public int CurrentTerm
        {
            get { return this._currentTerm; }
        }
    }
}
