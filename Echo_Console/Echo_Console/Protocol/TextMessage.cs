using System;
using System.Linq;

namespace Echo_Console.Protocol
{
    public class TextMessage : IProtocalSerializable
    {
        protected string Command;
        public string Source { get; set; }
        public string Destination { get; set; }
        public string Message { get; set; }

        public TextMessage()
        {
            Command = "#TM";
        }

        public TextMessage(string source, string destination, string message) : this()
        {
            Source = source;
            Destination = destination;
            Message = message;
        }

        public TextMessage(string packet) : this()
        {
            var props = packet.Split(':').ToList();
            if (props.Count != 3)
                throw new ArgumentException();
            Source = props[0].Replace(Command, "");
            Destination = props[1];
            Message = props[2];
        }

        public string Serialize()
        {
            return Command + Source + ":" + Destination + ":" + Message;
        }
    }
}