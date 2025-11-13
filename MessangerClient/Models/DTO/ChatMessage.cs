using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MessangerClient.Models.DTO
{
    /// <summary>
    /// Представляет собой объект сообщения в чате, Передаётся по сети
    /// </summary>
    internal class ChatMessage : DataPacket
    {
        public string Name { get; set; }
        public string Message { get; set; }

        public ChatMessage(string name, string message)
        {
            Name = name;
            Message = message;
        }

        public override string ToString()
        {
            return Name + ": " + Message;
        }
    }
}
