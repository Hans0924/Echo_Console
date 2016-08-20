using System;
using System.Linq;

namespace Echo_Console.Protocol
{
    public class IdentifyVatsim : IProtocalSerializable
    {
        protected string Command;

        public IdentifyVatsim()
        {
            Command = "$ID";
        }

        public IdentifyVatsim(string callsign, string destination, string unknown1, string client,
            string version, string cid, string unknown2) : this()
        {
            Callsign = callsign;
            Destination = destination;
            Unknown1 = unknown1;
            Client = client;
            Version = version;
            Cid = cid;
            Unknown2 = unknown2;
        }

        public IdentifyVatsim(string packet) : this()
        {
            var props = packet.Split(':').ToList();
            if (props.Count != 7)
                throw new ArgumentException();
            Callsign = props[0].Replace(Command, "");
            Destination = props[1];
            Unknown1 = props[2];
            Client = props[3];
            Version = props[4];
            Cid = props[5];
            Unknown2 = props[6];
        }

        public string Callsign { get; set; }
        public string Destination { get; set; }
        public string Unknown1 { get; set; }
        public string Client { get; set; }
        public string Version { get; set; }
        public string Cid { get; set; }
        public string Unknown2 { get; set; }

        public string Serialize()
        {
            return Command + Callsign + ":" + Destination + ":" + Unknown1 + ":" + Client + ":" + Version + ":" + Cid +
                   ":" + Unknown2;
        }
    }
}