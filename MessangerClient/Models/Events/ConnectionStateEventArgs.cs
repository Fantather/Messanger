using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MessangerClient.Models.Events
{
    internal class ConnectionStateEventArgs : EventArgs
    {
        public bool State { get; }
        public ConnectionStateEventArgs(bool state) => State = state;
    }
}
