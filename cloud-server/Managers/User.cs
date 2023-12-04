using System.Runtime.CompilerServices;
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


        public static bool operator == (User lhs, User rhs)
        {
            return lhs._id == rhs._id && lhs._username == rhs._username && lhs._email == rhs._email && lhs._phoneNumber == rhs._phoneNumber;
        }

        public static bool operator !=(User lhs, User rhs)
        {
            return lhs._id != rhs._id || lhs._username != rhs._username || lhs._email != rhs._email || lhs._phoneNumber != rhs._phoneNumber;

        }
    }
}
