using System;
using System.Linq;

namespace Echo_Console.Protocol
{
    public class Pong : IProtocalSerializable
    {
        protected string Command;

        public Pong()
        {
            Command = "$PO";
        }

        public Pong(string source, string destination, string timestamp) : this()
        {
            Source = source;
            Destination = destination;
            Timestamp = timestamp;
        }

        public Pong(string packet) : this()
        {
            var props = packet.Split(':').ToList();
            if (props.Count != 3)
                throw new ArgumentException();
            Source = props[0].Replace(Command, "");
            Destination = props[1];
            Timestamp = props[2];
        }

        public string Source { get; set; }
        public string Destination { get; set; }
        public string Timestamp { get; set; }

        public string Serialize()
        {
            return Command + Source + ":" + Destination + ":" + Timestamp;
        }
    }
}