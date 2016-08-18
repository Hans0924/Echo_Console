namespace Echo_Console.Protocol
{
    public class RequestWeather : IProtocalSerializable
    {
        protected string Command;

        public string Callsign { get; set; }

        public string Destination { get; set; }

        public string Airport { get; set; }

        public RequestWeather()
        {
            Command = "#WX";
        }

        public RequestWeather(string callsign, string destination, string airport) : this()
        {
            Callsign = callsign;
            Destination = destination;
            Airport = airport;
        }

        public string Serialize()
        {
            return Command + Callsign + ":" + Destination + ":" + Airport;
        }
    }
}