using GrpcCloud;

namespace cloud_server.Managers
{
    public class Authentication
    {
        private AuthDB _db;
        private Dictionary<string, User> _users;

        public Authentication(AuthDB db)
        {
            this._db = db; 
        }

        public bool Signup(string username, string password, string email, string phoneNumber="NULL")
        {
            this._db.signup(username, password, email, phoneNumber);
            return true;
        }

        public bool Login(string username, string password)
        {
            string sessionId = "";
            if (this._db.login(username, password))
            {
                User user = this._db.GetUser(username, password);
                // check if user already login:
                if (!this._users.ContainsValue(user))
                {
                    do
                    {
                        sessionId = new Guid().ToString();
                    } while (!this._users.ContainsKey(sessionId));

                    // Add user 
                    this._users.Add(sessionId, user);
                    return true;
                }
            }
            return false;
        }

        public void Logout(string sessionId)
        { 
            this._users.Remove(sessionId);
        }

        public User GetUser(string sessionId)
        {
            try
            {
                return this._users[sessionId];
            }
            catch
            {
                throw new Exception("Incorrect session id");
            }
        }
    }
}
