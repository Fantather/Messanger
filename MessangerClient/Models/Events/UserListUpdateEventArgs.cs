using MessangerClient.Models.DTO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MessangerClient.Models.Events
{
    internal class UserListUpdateEventArgs : EventArgs
    {
        public UserListUpdateDataPacket UpdateList { get; }
        public UserListUpdateEventArgs(UserListUpdateDataPacket updateList) => UpdateList = updateList;
    }
}
