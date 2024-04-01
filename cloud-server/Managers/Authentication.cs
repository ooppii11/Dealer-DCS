using cloud_server.Utilities;
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

        public Tuple<string, string> Login(string username, string password)
        {
            string sessionId = "";
            if (this._db.login(username, password))
            {
                // Get user
                User user = this._db.GetUser(username, password);

                // Check if user alrady login:
                string existingSessionId = _users.FirstOrDefault(pair => pair.Value == user).Key;
                if (existingSessionId != null)
                {
                    // Remove the existing user
                    _users.Remove(existingSessionId);
                }

                // Generate new session id:
                do
                {
                    sessionId = Guid.NewGuid().ToString();
                } while (this._users.ContainsKey(sessionId));

                // Add user 
                this._users.Add(sessionId, user);
                return new Tuple<string, string>(sessionId, existingSessionId);
                
            }
            throw new UserDoesNotExistException("User not found");
        }

        public void Logout(string sessionId)
        {
            if (CheckSessionId(sessionId))  // Check if session id connected 
            {
                this._users.Remove(sessionId);
            }
        }

        public User GetUser(string sessionId)
        {
            try
            {
                return this._users[sessionId];
            }
            catch(Exception ex)
            {
                throw new IncorrectSessionIdException("Incorrect session id");
            }
        }

        public bool CheckSessionId(string sessionId)
        {
            if (this._users.ContainsKey(sessionId)) // Check if session id connected
            {
                return true;
            }
            throw new IncorrectSessionIdException("Incorrect session id");
        }
    }
}
