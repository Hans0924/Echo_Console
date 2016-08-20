using System;
using System.Collections.Generic;
using System.Linq;

namespace Echo_Console.Protocol
{
    public class ClientResponse : IProtocalSerializable
    {
        protected string Command;
        public string Source { get; set; }
        public string Destination { get; set; }
        public string Parameters { get; set; }

        public ClientResponse()
        {
            Command = "$CR";
        }

        public ClientResponse(string source, string destination, string parameters) : this()
        {
            Source = source;
            Destination = destination;
            Parameters = parameters;
        }

        public ClientResponse(string packet) : this()
        {
            var props = packet.Split(':').ToList();
            if (props.Count < 3)
                throw new ArgumentException();
            Source = props[0].Replace("%", "");
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
