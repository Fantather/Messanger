using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MessangerClient.Models.DTO
{
    internal class UserListUpdateMessage : NetworkMessage
    {
        private readonly List<User> _users;
        public UserListUpdateMessage(List<User> users)
        {
            _users = users;
        }
    }
}
