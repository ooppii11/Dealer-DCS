namespace cloud_server.DB
{
    public class Location
    {
        private string _primaryServer;
        private string _firstBackupServer;
        private string _secondBackupServer;
        public string PrimaryServer 
        {
            get { return this._primaryServer; }
        }
        
        public string FirstBackupServer
        {
            get { return this._firstBackupServer; }
        }
        
        public string SecondBackupServer
        {
            get { return this._secondBackupServer; }
        }

        public Location(string primaryServer, string firstBackupServer, string secondBackupServer)
        {
            this._primaryServer = primaryServer;
            this._firstBackupServer = firstBackupServer;
            this._secondBackupServer = secondBackupServer;
        }
    }
}
