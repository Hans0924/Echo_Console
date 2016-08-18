namespace Echo_Console.Protocol
{
    public class DisconnectFromServer : IProtocalSerializable
    {
        protected string Command;
        public string Callsign { get; set; }
        public string Destination { get; set; }

        public DisconnectFromServer()
        {
            Command = "#DA";
        }

        public DisconnectFromServer(string callsign, string destination) : this()
        {
            Callsign = callsign;
            Destination = destination;
        }

        public string Serialize()
        {
            return Command + Callsign + ":" + Destination;
        }
    }
}