namespace Echo_Console.Protocol
{
    public class AtcPositionReport : IProtocalSerializable
    {
        protected string Command;

        public AtcPositionReport()
        {
            Command = "%";
        }

        public AtcPositionReport(string callsign, string frequency, string facility, string range, string level,
            string latitude, string longitude, string altitude) : this()
        {
            Callsign = callsign;
            Frequency = frequency;
            Facility = facility;
            Range = range;
            Level = level;
            Latitude = latitude;
            Longitude = longitude;
            Altitude = altitude;
        }

        public string Callsign { get; set; }
        public string Frequency { get; set; }
        public string Facility { get; set; }
        public string Range { get; set; }
        public string Level { get; set; }
        public string Latitude { get; set; }
        public string Longitude { get; set; }
        public string Altitude { get; set; }

        public string Serialize()
        {
            return Command + Callsign + ":" + Frequency + ":" + Facility + ":" + Range + ":" + Level + ":" + Latitude +
                   ":" + Longitude + ":" + Altitude;
        }
    }
}