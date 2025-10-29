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
        public ChatMessage Report { get; }
        public ChatMessageEventArgs(ChatMessage report) => Report = report;
    }
}
