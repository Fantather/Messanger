using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MessangerClient.Models.Interop.Reports
{
    internal class MessageBytesReport : NetworkReport
    {
        public byte[] Message { get; set; }
        public MessageBytesReport(byte[] message) => Message = message;
    }
}
