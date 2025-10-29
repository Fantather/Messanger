using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MessangerClient.Models.Reports
{
    internal class MessageReport : NetworkReport
    {
        public NetworkMessage Message { get; }
        public MessageReport(NetworkMessage message) => Message = message;
    }
}
