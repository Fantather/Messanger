using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MessangerClient.Models.Interop.Reports
{
    internal class ExceptionReport : NetworkReport
    {
        public Exception Exception { get; }
        public ExceptionReport(Exception ex) => Exception = ex;
    }
}
