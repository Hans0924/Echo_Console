using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Echo_Console.Protocol
{
    public class DeleteAtc : IProtocalSerializable
    {
        protected string Command;
        public string Source { get; set; }
        public string Destination { get; set; }

        public DeleteAtc()
        {
            Command = "#DA";
        }

        public DeleteAtc(string source, string destination) : this()
        {
            Source = source;
            Destination = destination;
        }

        public DeleteAtc(string packet) : this()
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
