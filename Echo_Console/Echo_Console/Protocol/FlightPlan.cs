using System;
using System.Linq;

namespace Echo_Console.Protocol
{
    public class FlightPlan : IProtocalSerializable
    {
        protected string Command;
        public string Source { get; set; }
        public string Destination { get; set; }
        public string FlightRule { get; set; }
        public string AircraftType { get; set; }
        public string TrueAirSpeed { get; set; }
        public string DepartureAirport { get; set; }
        public string ETD { get; set; }
        public string ATD { get; set; }
        public string CruisingAlt { get; set; }
        public string ArrivalAirport { get; set; }
        public string EnrouteTimeEstHour { get; set; }
        public string EnrouteTimeEstMin { get; set; }
        public string FuelHour { get; set; }
        public string FuelMin { get; set; }
        public string AlternateAirport { get; set; }
        public string Remarks { get; set; }
        public string Route { get; set; }

        public FlightPlan()
        {
            Command = "$FP";
        }

        public FlightPlan(string source, string destination, string flightRule, string aircraftType, 
            string trueAirSpeed, string departureAirport, string etd, string atd, string cruisingAlt, 
            string arrivalAirport, string enrouteTimeEstHour, string enrouteTimeEstMin, string fuelHour, 
            string fuelMin, string alternateAirport, string remarks, string route) : this()
        {
            Source = source;
            Destination = destination;
            FlightRule = flightRule;
            AircraftType = aircraftType;
            TrueAirSpeed = trueAirSpeed;
            DepartureAirport = departureAirport;
            ETD = etd;
            ATD = atd;
            CruisingAlt = cruisingAlt;
            ArrivalAirport = arrivalAirport;
            EnrouteTimeEstHour = enrouteTimeEstHour;
            EnrouteTimeEstMin = enrouteTimeEstMin;
            FuelHour = fuelHour;
            FuelMin = fuelMin;
            AlternateAirport = alternateAirport;
            Remarks = remarks;
            Route = route;
        }

        public FlightPlan(string packet) : this()
        {
            var props = packet.Split(':').ToList();
            if (props.Count < 17)
                throw new ArgumentException();
            Source = props[0].Replace(Command, "");
            Destination = props[1];
            FlightRule = props[2];
            AircraftType = props[3];
            TrueAirSpeed = props[4];
            DepartureAirport = props[5];
            ETD = props[6];
            ATD = props[7];
            CruisingAlt = props[8];
            ArrivalAirport = props[9];
            EnrouteTimeEstHour = props[10];
            EnrouteTimeEstMin = props[11];
            FuelHour = props[12];
            FuelMin = props[13];
            AlternateAirport = props[14];
            Remarks = props[15];
            Route = props[16];
        }

        public string Serialize()
        {
            return Command + Source + ":" + Destination + ":" + FlightRule + ":" + AircraftType + ":" + 
                TrueAirSpeed + ":" + DepartureAirport + ":" + ETD + ":" + ATD + ":" + CruisingAlt + ":" +
                ArrivalAirport + ":" + EnrouteTimeEstHour + ":" + EnrouteTimeEstMin + ":" + FuelHour + ":" +
                FuelMin + ":" + AlternateAirport + ":" + Remarks + ":" + Route;
        }
    }
}