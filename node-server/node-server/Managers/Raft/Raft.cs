namespace node_server.Managers.Raft
{
    public class Raft
    {
        public enum StatesCode
        {
            Follower,
            Candidate,
            Leader
        }
        private StatesCode _currentState;

        //private RaftSettings _settings;
        //  private Log _logger;

        public StatesCode RaftStateCode 
        {
            get { return _currentState; }
        }
       
        public Raft(/*RaftSettings settings*/)
        {
            this._currentState = StatesCode.Follower;
           // this._settings = settings;
           // this._logger(settings.loggerPath);
        }

        public void start()
        {
            this.run();
        }

        private void run()
        {
            while (true)
            {
                if (this._currentState == StatesCode.Follower)
                {
                    this.followerSatate();
                    this._currentState = StatesCode.Candidate;
                }
                if (this._currentState == StatesCode.Candidate)
                {
                    if (this.caniddate())
                    {
                        this._currentState = StatesCode.Leader;
                        this.leaderState();
                    }
                }
            }    
        }

        private bool caniddate()
        {
            //Caniddate caniddate();
            //return caniddate.StartElction();
            return false;
        }

        private void leaderState()
        {
            //Leader leader();
            //ledaer.Start();
        }

        private void followerSatate()
        {
            //Follower follower();
            //follower.Start();
        }
    }
}
