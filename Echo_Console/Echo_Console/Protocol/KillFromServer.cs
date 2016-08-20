using System;
using System.Linq;

namespace Echo_Console.Protocol
{
    public class KillFromServer : IProtocalSerializable
    {
        protected string Command;
        public string Source { get; set; }
        public string Target { get; set; }
        public string Reason { get; set; }

        public KillFromServer()
        {
            Command = "$!!";
        }

        public KillFromServer(string source, string target, string reason) : this()
        {
            Source = source;
            Target = target;
            Reason = reason;
        }

        public KillFromServer(string packet) : this()
        {
            var props = packet.Split(':').ToList();
            if (props.Count < 3)
                throw new ArgumentException();
            Source = props[0].Replace(Command, "");
            Target = props[1];
            Reason = props[2];
        }

        public string Serialize()
        {
            return Command + Source + ":" + Target + ":" + Reason;
        }
    }
}