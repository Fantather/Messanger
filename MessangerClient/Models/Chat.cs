using MessangerClient.Models.DTO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace MessangerClient.Models
{
    internal class Chat
    {
        public List<ChatMessage> Messages { get; }
        public User Companion { get; set; }


        // Конструктор без параметров для JSON десериализации
        public Chat()
        {
            Messages = new List<ChatMessage>();
            Companion = new User("");
        }

        public Chat(User companion)
        {
            Companion = companion;
            Messages = new List<ChatMessage>();
        }

        public Chat(User companion, List<ChatMessage> messages)
        {
            Companion = companion;
            Messages = messages;
        }


        public void AddMessage(ChatMessage message)
        {
            Messages.Add(message);
        }
    }
}
