using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Echo_Console.Protocol
{
    public class SendPing
    {
        protected string Command;
        public string Callsign { get; set; }
        public string Target { get; set; }
        public string Timestamp { get; set; }

    }
}
