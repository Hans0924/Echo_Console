namespace Echo_Console.Protocol
{
    public class SendFlightPlan : IProtocalSerializable
    {
        protected string Command;
        public string Callsign { get; set; }
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

        public SendFlightPlan()
        {
            Command = "$FP";
        }

        public SendFlightPlan(string callsign, string destination, string flightRule, string aircraftType, 
            string trueAirSpeed, string departureAirport, string etd, string atd, string cruisingAlt, 
            string arrivalAirport, string enrouteTimeEstHour, string enrouteTimeEstMin, string fuelHour, 
            string fuelMin, string alternateAirport, string remarks, string route) : this()
        {
            Callsign = callsign;
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

        public string Serialize()
        {
            return Command + Callsign + ":" + Destination + ":" + FlightRule + ":" + AircraftType + ":" + 
                TrueAirSpeed + ":" + DepartureAirport + ":" + ETD + ":" + ATD + ":" + CruisingAlt + ":" +
                ArrivalAirport + ":" + EnrouteTimeEstHour + ":" + EnrouteTimeEstMin + ":" + FuelHour + ":" +
                FuelMin + ":" + AlternateAirport + ":" + Remarks + ":" + Route;
        }
    }
}