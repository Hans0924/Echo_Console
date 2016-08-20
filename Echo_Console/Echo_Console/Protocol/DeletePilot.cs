using System;
using System.Linq;

namespace Echo_Console.Protocol
{
    public class DeletePilot : IProtocalSerializable
    {
        protected string Command;
        public string Source { get; set; }
        public string Destination { get; set; }

        public DeletePilot()
        {
            Command = "#DP";
        }

        public DeletePilot(string source, string destination) : this()
        {
            Source = source;
            Destination = destination;
        }

        public DeletePilot(string packet) : this()
        {
            var props = packet.Split(':').ToList();
            if (props.Count != 2)
                throw new ArgumentException();
            Source = props[0].Replace(Command, "");
            Destination = props[1];
        }

        public string Serialize()
        {
            return Command + Source + ":" + Destination;
        }
    }
}