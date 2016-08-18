using System;

namespace Echo_Console.Protocol
{
    public class PilotConnectRequest : IProtocalSerializable
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

        public PilotConnectRequest()
        {
            Command = "#AP";
        }

        public PilotConnectRequest(string callsign, string destination, string cid, 
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

        public string Serialize()
        {
            return Command + Callsign + ":" + Destination + ":" + Cid + ":" + Password
                + ":" + RequestLevel + ":" + ProtocolRevision + ":" + SimType + ":" + Remarks;
        }
    }
}