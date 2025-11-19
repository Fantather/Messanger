using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MessangerClient.Models.Events
{
    /// <summary>
    /// Передаёт статус соединения с сервером и при закрытии, передаёт список чатов
    /// </summary>
    internal class ConnectionStateEventArgs : EventArgs
    {
        public bool State { get; }
        public IEnumerable<Chat>? Chats { get; }

        public ConnectionStateEventArgs(bool state, IEnumerable<Chat>? chats = null)
        {
            State = state;
            Chats = chats;
        }
    }
}
