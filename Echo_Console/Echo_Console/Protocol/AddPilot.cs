using System;
using System.Linq;

namespace Echo_Console.Protocol
{
    public class AddPilot : IProtocalSerializable
    {
        protected string Command;

        public string Callsign { get; set; }

        public string Destination { get; set; }

        public string Cid { get; set; }

        public string Password { get; set; }

        public string RequestLevel { get; set; }

        public string ProtocolRevision { get; set; }

        public string SimType { get; set; }

        public string Remarks { get; set; }

        public AddPilot()
        {
            Command = "#AP";
        }

        public AddPilot(string callsign, string destination, string cid, 
            string password, string requestLevel, string protocolRevision, 
            string simType, string remarks) : this()
        {
            Callsign = callsign;
            Destination = destination;
            Cid = cid;
            Password = password;
            RequestLevel = requestLevel;
            ProtocolRevision = protocolRevision;
            SimType = simType;
            Remarks = remarks;
        }

        public AddPilot(string packet) : this()
        {
            var props = packet.Split(':').ToList();
            if (props.Count < 7)
                throw new ArgumentException();
            Callsign = props[0].Replace(Command, "");
            Destination = props[1];
            Cid = props[2];
            Password = props[3];
            RequestLevel = props[4];
            ProtocolRevision = props[5];
            SimType = props[6];
        }

        public string Serialize()
        {
            return Command + Callsign + ":" + Destination + ":" + Cid + ":" + Password
                + ":" + RequestLevel + ":" + ProtocolRevision + ":" + SimType + ":" + Remarks;
        }
    }
}