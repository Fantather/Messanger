using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MessangerClient.Models.Interop.Reports
{
    internal class ConnectionReport : NetworkReport
    {
        public readonly ConnectionState State;
        public ConnectionReport(ConnectionState state)
        {
            State = state;
        }
    }
}
