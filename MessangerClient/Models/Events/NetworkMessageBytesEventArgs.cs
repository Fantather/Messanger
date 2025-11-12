using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MessangerClient.Models.Events
{
    internal class NetworkMessageBytesEventArgs : EventArgs
    {
        public byte[] Message { get; set; }
        public NetworkMessageBytesEventArgs (byte[] networkMessage) => Message = networkMessage;
    }
}
