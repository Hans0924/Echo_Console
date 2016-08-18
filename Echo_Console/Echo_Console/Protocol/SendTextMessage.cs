namespace Echo_Console.Protocol
{
    public class SendTextMessage : IProtocalSerializable
    {
        protected string Command;
        public string Callsign { get; set; }
        public string Destination { get; set; }
        public string Message { get; set; }

        public SendTextMessage()
        {
            Command = "#TM";
        }

        public SendTextMessage(string callsign, string destination, string message) : this()
        {
            Callsign = callsign;
            Destination = destination;
            Message = message;
        }

        public string Serialize()
        {
            return Command + Callsign + ":" + Destination + ":" + Message;
        }
    }
}