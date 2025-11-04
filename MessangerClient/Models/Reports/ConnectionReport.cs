using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MessangerClient.Models.Reports
{

    // Заменить потом Bool на ConnectionState
    internal class ConnectionReport : NetworkReport
    {
        public readonly bool IsConnected;
        public ConnectionReport(bool isConnected)
        {
            IsConnected = isConnected;
        }
    }
}
