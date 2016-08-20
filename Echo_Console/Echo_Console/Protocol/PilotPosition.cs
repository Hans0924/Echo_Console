using System;
using System.Linq;

namespace Echo_Console.Protocol
{
    public class PilotPosition : IProtocalSerializable
    {
        protected string Command;

        // S - Standby, N - Contact, Y - Identify
        public string IdentFlag { get; set; }

        public string Source { get; set; }

        // 0000 - 7777
        public string Squawk { get; set; }

        public string Rating { get; set; }

        public string Latitude { get; set; }

        public string Longitue { get; set; }

        public string Altitude { get; set; }

        public string GroundSpeed { get; set; }

        public string PitchBankHeading { get; set; }

        public string Flags { get; set; }

        public PilotPosition()
        {
            Command = "@";
        }

        public PilotPosition(string identFlag, string source, string squawk, 
            string rating, string latitude, string longitue, string altitude, 
            string groundSpeed, string pitchBankHeading) : this()
        {
            IdentFlag = identFlag;
            Source = source;
            Squawk = squawk;
            Rating = rating;
            Latitude = latitude;
            Longitue = longitue;
            Altitude = altitude;
            GroundSpeed = groundSpeed;
            PitchBankHeading = pitchBankHeading;
        }

        public PilotPosition(string identFlag, string source, string squawk, string rating, string latitude, string longitue, string altitude, string groundSpeed, string pitchBankHeading, string flags) : this()
        {
            IdentFlag = identFlag;
            Source = source;
            Squawk = squawk;
            Rating = rating;
            Latitude = latitude;
            Longitue = longitue;
            Altitude = altitude;
            GroundSpeed = groundSpeed;
            PitchBankHeading = pitchBankHeading;
            Flags = flags;
        }

        public PilotPosition(string packet) : this()
        {
            var props = packet.Split(':').ToList();
            if (props.Count < 10)
                throw new ArgumentException();
            IdentFlag = props[0].Replace(Command, "");
            Source = props[1];
            Squawk = props[2];
            Rating = props[3];
            Latitude = props[4];
            Longitue = props[5];
            Altitude = props[6];
            GroundSpeed = props[7];
            PitchBankHeading = props[8];
            Flags = props[9];
        }

        public string Serialize()
        {
            return Command + IdentFlag + ":" + Source + ":" + Squawk + ":" + Rating + ":" + Latitude + ":" +
                Longitue + ":" + Altitude + ":" + GroundSpeed + ":" + PitchBankHeading + ":" + Flags;
        }
    }
}