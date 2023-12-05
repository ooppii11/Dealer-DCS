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
            this._users = new Dictionary<string, User>();
        }

        public bool Signup(string username, string password, string email, string phoneNumber="NULL")
        {
            this._db.signup(username, password, email, phoneNumber);
            return true;
        }

        public string Login(string username, string password)
        {
            string sessionId = "";
            if (this._db.login(username, password))
            {
                User user = this._db.GetUser(username, password);

                // check if user already login:
                if (!_users.Any(pair => pair.Value == user))
                {
                    do
                    {
                        sessionId = Guid.NewGuid().ToString();
                    } while (this._users.ContainsKey(sessionId));

                    // Add user 
                    this._users.Add(sessionId, user);
                    return sessionId;
                }
                throw new Exception("User already logged in");
            }
            throw new Exception("User not found");
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
