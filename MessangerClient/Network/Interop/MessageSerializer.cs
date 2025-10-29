using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

using Message = MessangerClient.Models.Interop.Message;

namespace MessangerClient.Network.Interop
{
    internal class MessageSerializer
    {
        public byte[] Serialize(string clientName, string msg)
        {
            return JsonSerializer.SerializeToUtf8Bytes(new Message(clientName, msg));
        }

        public Message? Deserialize(byte[] msg)
        {
            return JsonSerializer.Deserialize<Message>(msg);
        }
    }
}
