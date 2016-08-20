using System;
using System.Collections.Generic;
using System.Linq;

namespace Echo_Console.Protocol
{
    public class ClientQuery : IProtocalSerializable
    {
        protected string Command;

        public ClientQuery()
        {
            Command = "$CQ";
        }

        public ClientQuery(string source, string destination, string parameters) : this()
        {
            Source = source;
            Destination = destination;
            Parameters = parameters;
        }

        public ClientQuery(string packet) : this()
        {
            var props = packet.Split(':').ToList();
            if (props.Count < 3)
                throw new ArgumentException();
            Source = props[0].Replace("%", "");
            Destination = props[1];
            Parameters = packet.Substring(Command.Length + Source.Length + Destination.Length + 2);
        }

        public string Source { get; set; }
        public string Destination { get; set; }
        public string Parameters { get; set; }

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