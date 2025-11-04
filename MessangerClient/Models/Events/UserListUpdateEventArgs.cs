using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MessangerClient.Models.Events
{
    internal class UserListUpdateEventArgs : EventArgs
    {
        public UserListUpdate UpdateList { get; }
        public UserListUpdateEventArgs(UserListUpdate updateList) => UpdateList = updateList;
    }
}
