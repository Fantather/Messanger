using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MessangerClient.Models.DTO
{
    /// <summary>
    /// Представляет собой объект сообщения в чате, Передаётся по сети(Хотя не должен, но переписывать уже не хочу)
    /// </summary>
    internal class ChatMessage : DataPacket
    {
        public User Sender { get; set; } = new User();
        public User Recipient { get; set; } = new User();

        public string Message { get; set; } = string.Empty;

        // Для десериализации
        public ChatMessage()
        {
        }

        public ChatMessage(User sender, User recipient, string message)
        {
            Sender = sender;
            Recipient = recipient;
            Message = message;
        }

        public override string ToString()
        {
            return Sender.Name + ": " + Message;
        }
    }
}
