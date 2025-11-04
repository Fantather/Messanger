using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MessangerClient.Models
{
    internal class UserListUpdate : NetworkMessage
    {
        private readonly List<User> _users;
        public UserListUpdate(List<User> users)
        {
            _users = users;
        }
    }
}
