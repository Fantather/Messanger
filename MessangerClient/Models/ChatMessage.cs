using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MessangerClient.Models
{
    internal class ChatMessage : NetworkMessage
    {
        public string Name { get; set; }
        public string Message { get; set; }

        public ChatMessage(string name, string message)
        {
            Name = name;
            Message = message;
        }
    }
}
