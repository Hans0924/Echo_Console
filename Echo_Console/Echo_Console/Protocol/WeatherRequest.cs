namespace Echo_Console.Protocol
{
    public class WeatherRequest : IProtocalSerializable
    {
        protected string Command;

        public string Source { get; set; }

        public string Destination { get; set; }

        public string Airport { get; set; }

        public WeatherRequest()
        {
            Command = "#WX";
        }

        public WeatherRequest(string source, string destination, string airport) : this()
        {
            Source = source;
            Destination = destination;
            Airport = airport;
        }

        public string Serialize()
        {
            return Command + Source + ":" + Destination + ":" + Airport;
        }
    }
}