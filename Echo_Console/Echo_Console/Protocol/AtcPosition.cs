using System;
using System.Linq;

namespace Echo_Console.Protocol
{
    public class AtcPosition : IProtocalSerializable
    {
        protected string Command;

        public string Source { get; set; }
        public string Frequency { get; set; }
        public string Facility { get; set; }
        public string Range { get; set; }
        public string Level { get; set; }
        public string Latitude { get; set; }
        public string Longitude { get; set; }
        public string Altitude { get; set; }

        public AtcPosition()
        {
            Command = "%";
        }

        public AtcPosition(string source, string frequency, string facility, string range, string level,
            string latitude, string longitude, string altitude) : this()
        {
            Source = source;
            Frequency = frequency;
            Facility = facility;
            Range = range;
            Level = level;
            Latitude = latitude;
            Longitude = longitude;
            Altitude = altitude;
        }
        public AtcPosition(string packet) : this()
        {
            var props = packet.Split(':').ToList();
            if (props.Count != 8)
                throw new ArgumentException();
            Source = props[0].Replace("%", "");
            Frequency = props[1];
            Facility = props[2];
            Range = props[3];
            Level = props[4];
            Latitude = props[5];
            Longitude = props[6];
            Altitude = props[7];
        }

        public string Serialize()
        {
            return Command + Source + ":" + Frequency + ":" + Facility + ":" + Range + ":" + Level + ":" + Latitude +
                   ":" + Longitude + ":" + Altitude;
        }
    }
}