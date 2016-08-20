using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Echo_Console.Protocol
{
    public class AddAtc : IProtocalSerializable
    {
        protected string Command;
        public string Source { get; set; }
        public string Destination { get; set; }
        public string RealName { get; set; }
        public string Cid { get; set; }
        public string Password { get; set; }
        public string RequireLevel { get; set; }
        public string ProtocolRevision { get; set; }

        public AddAtc()
        {
            Command = "#AA";
        }

        public AddAtc(string source, string destination, string realName, string cid, string password, string requireLevel, string protocolRevision) : this()
        {
            Source = source;
            Destination = destination;
            RealName = realName;
            Cid = cid;
            Password = password;
            RequireLevel = requireLevel;
            ProtocolRevision = protocolRevision;
        }

        public AddAtc(string packet) : this()
        {
            var props = packet.Split(':').ToList();
            if (props.Count < 6)
                throw new ArgumentException();
            Source = props[0].Replace(Command, "");
            Destination = props[1];
            RealName = props[2];
            Cid = props[3];
            Password = props[4];
            RequireLevel = props[5];
        }

        public string Serialize()
        {
            return Command + Source + ":" + Destination + ":" + RealName + ":" + Cid + ":" + Password + ":" + RequireLevel + ":" + ProtocolRevision;
        }
    }
}
