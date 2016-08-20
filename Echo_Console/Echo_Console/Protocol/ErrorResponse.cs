using System;
using System.Linq;

namespace Echo_Console.Protocol
{
    public class ErrorResponse : IProtocalSerializable
    {
        protected string Command;

        public ErrorResponse()
        {
            Command = "$ER";
        }

        public ErrorResponse(string source, string destination, string errorCode, string unknown, string content)
            : this()
        {
            Source = source;
            Destination = destination;
            ErrorCode = errorCode;
            Unknown = unknown;
            Content = content;
        }

        public ErrorResponse(string packet) : this()
        {
            var props = packet.Split(':').ToList();
            if (props.Count < 4)
                throw new ArgumentException();
            Source = props[0].Replace(Command, "");
            Destination = props[1];
            ErrorCode = props[2];
            Unknown = props[3];
            Content = props[4];
        }

        public string Source { get; set; }

        public string Destination { get; set; }

        public string ErrorCode { get; set; }

        public string Unknown { get; set; }

        public string Content { get; set; }

        public string Serialize()
        {
            return Command + Source + ":" + Destination + ":" + ErrorCode + ":" + Unknown + ":" + Content;
        }
    }
}