using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MessangerClient.Models.Events
{
    internal class DataPackageBytesEventArgs : EventArgs
    {
        public byte[] PacketBytes { get; set; }
        public DataPackageBytesEventArgs (byte[] networkMessage) => PacketBytes = networkMessage;
    }
}
