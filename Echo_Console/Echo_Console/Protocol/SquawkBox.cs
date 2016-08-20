using System;
using System.Collections.Generic;
using System.Linq;

namespace Echo_Console.Protocol
{
    public class SquawkBox : IProtocalSerializable
    {
        protected string Command;
        public string Source { get; set; }
        public string Destination { get; set; }
        public string Parameters { get; set; }

        public SquawkBox()
        {
            Command = "#SB";
        }

        public SquawkBox(string source, string destination, string parameters) : this()
        {
            Source = source;
            Destination = destination;
            Parameters = parameters;
        }

        public SquawkBox(string packet) : this()
        {
            var props = packet.Split(':').ToList();
            if (props.Count < 3)
                throw new ArgumentException();
            Source = props[0].Replace(Command, "");
            Destination = props[1];
            Parameters = packet.Substring(Command.Length + Source.Length + Destination.Length + 2);
        }

        public string Serialize()
        {
            return Command + Source + ":" + Destination + ":" + Parameters;
        }

        public List<string> SplitParam()
        {
            return Parameters.Split(':').ToList();
        }
    }
}
