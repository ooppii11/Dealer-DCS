using System.Xml.Linq;

namespace cloud_server.Managers
{
    public class User
    {
        private string _id;
        private string _username;
        private string _email;
        private string _phoneNumber;

        public User(string id, string username, string email, string phoneNumber)
        {
            this._id = id;
            this._username = username;
            this._email = email;
            this._phoneNumber = phoneNumber;
        }

        private string Id => _id;
        private string Username => _username;
        private string Email => _email;
        private string Phone => _phoneNumber;
    }
}
