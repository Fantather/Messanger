using MessangerClient.Models.DTO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MessangerClient.Models.Interop.Reports
{
    internal class MessageReport : NetworkReport
    {
        public DataPacket Message { get; }
        public MessageReport(DataPacket message) => Message = message;
    }
}
