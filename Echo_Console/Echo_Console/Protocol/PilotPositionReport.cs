namespace Echo_Console.Protocol
{
    public class PilotPositionReport
    {
        protected string Command;

        // S - Standby, N - Contact, Y - Identify
        public string IdentFlag { get; set; }

        public string Callsign { get; set; }

        // 0000 - 7777
        public string Squawk { get; set; }

        public string Rating { get; set; }

        public string Latitude { get; set; }

        public string Longitue { get; set; }

        public string Altitude { get; set; }

        public string GroundSpeed { get; set; }

        public string PitchBankHeading { get; set; }

        public string Flags { get; set; }

        public PilotPositionReport()
        {
            Command = "@";
        }

        public PilotPositionReport(string identFlag, string callsign, string squawk, 
            string rating, string latitude, string longitue, string altitude, 
            string groundSpeed, string pitchBankHeading) : this()
        {
            IdentFlag = identFlag;
            Callsign = callsign;
            Squawk = squawk;
            Rating = rating;
            Latitude = latitude;
            Longitue = longitue;
            Altitude = altitude;
            GroundSpeed = groundSpeed;
            PitchBankHeading = pitchBankHeading;
        }

        public PilotPositionReport(string identFlag, string callsign, string squawk, 
            string rating, string latitude, string longitue, string altitude, 
            string groundSpeed, string pitchBankHeading, string flags) : this()
        {
            IdentFlag = identFlag;
            Callsign = callsign;
            Squawk = squawk;
            Rating = rating;
            Latitude = latitude;
            Longitue = longitue;
            Altitude = altitude;
            GroundSpeed = groundSpeed;
            PitchBankHeading = pitchBankHeading;
            Flags = flags;
        }

        public string Serialize()
        {
            return Command + IdentFlag + ":" + Callsign + ":" + Squawk + ":" + Rating + ":" + Latitude + ":" +
                Longitue + ":" + Altitude + ":" + GroundSpeed + ":" + PitchBankHeading + ":" + Flags;
        }
    }
}