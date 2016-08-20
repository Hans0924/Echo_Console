using System;
using System.Collections.Generic;
using System.Linq;

namespace Echo_Console.Protocol
{
    public class AcarsResponse : IProtocalSerializable
    {
        protected string Command;

        public AcarsResponse()
        {
            Command = "$AR";
        }

        public AcarsResponse(string source, string destination, string acarsType, string parameters) : this()
        {
            Source = source;
            Destination = destination;
            AcarsType = acarsType;
            Parameters = parameters;
        }

        public AcarsResponse(string packet) : this()
        {
            var props = packet.Split(':').ToList();
            if (props.Count < 4)
                throw new ArgumentException();
            Source = props[0].Replace(Command, "");
            Destination = props[1];
            AcarsType = props[2];
            Parameters = packet.Substring(Command.Length + Source.Length + Destination.Length + AcarsType.Length + 3);
        }

        public string Source { get; set; }
        public string Destination { get; set; }
        public string AcarsType { get; set; }
        public string Parameters { get; set; }

        public string Serialize()
        {
            return Command + Source + ":" + Destination + ":" + AcarsType + ":" + Parameters;
        }

        public List<string> SplitParam()
        {
            return Parameters.Split(':').ToList();
        }
    }
}
