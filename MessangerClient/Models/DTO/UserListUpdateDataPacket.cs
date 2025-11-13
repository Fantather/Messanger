using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MessangerClient.Models.DTO
{
    /// <summary>
    /// Хранит в себе обновлённый список пользователей, с которыми у клиента существуют чаты.
    /// Передаётся по сети.
    /// </summary>
    internal class UserListUpdateDataPacket : DataPacket
    {
        private readonly List<User> _users;
        public UserListUpdateDataPacket(List<User> users)
        {
            _users = users;
        }
    }
}
