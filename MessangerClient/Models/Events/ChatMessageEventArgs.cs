using MessangerClient.Models.DTO;
using MessangerClient.Models.Reports;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MessangerClient.Models.Events
{
    internal class ChatMessageEventArgs : EventArgs
    {
        public ChatMessage ChatMessage { get; }
        public ChatMessageEventArgs(ChatMessage report) => ChatMessage = report;

        public override string ToString()
        {
            return $"{ChatMessage.Name}: {ChatMessage.Message}";
        }
    }
}
