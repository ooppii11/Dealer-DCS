using NodeServer.Managers.Raft.States;
namespace NodeServer.Managers.Raft
{
    public class Raft
    {
        public enum StatesCode
        {
            Follower,
            Candidate,
            Leader
        }
        private StatesCode _currentStateCode;
        private State _state;
        private RaftSettings _settings;
        private Log _logger;

        public StatesCode RaftStateCode
        {
            get { return _currentStateCode; }
        }
        public State State { get { return _state; } }
        public RaftSettings Settings { get { return this._settings; } }

        public Raft(RaftSettings settings)
        {
            this._currentStateCode = StatesCode.Follower;
            this._settings = settings;
           // this._logger(settings.loggerPath);
        }

        public void Start()
        {
            Thread thread = new Thread(new ThreadStart(Run));
            thread.Start();
        }

        private void Run()
        {
            while (true)
            {
                if (this._state == null)
                {
                    if (this._currentStateCode == StatesCode.Follower)
                    {
                        this._state = new Follower(this._settings, this._logger);
                        this._state.Start();

                        this._state = null;
                        this._currentStateCode = StatesCode.Candidate;
                    }
                    else if (this._currentStateCode == StatesCode.Candidate)
                    {
                        this._state = new Candidate(this._settings, this._logger);
                        this._currentStateCode = this._state.Start();
                        this._state = null;
                    }
                    else if (this._currentStateCode == StatesCode.Leader)
                    {
                        this._state = new Leader(this._settings, this._logger);
                        this._state.Start();
                        this._state = null;
                    }
                }
            }    
        }
    }
}
