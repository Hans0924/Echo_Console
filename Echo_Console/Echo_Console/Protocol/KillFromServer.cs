namespace Echo_Console.Protocol
{
    public class KillFromServer : IProtocalSerializable
    {
        protected string Command;
        public string Callsign { get; set; }
        public string Target { get; set; }
        public string Reason { get; set; }

        public KillFromServer()
        {
            Command = "$!!";
        }

        public KillFromServer(string callsign, string target, string reason) : this()
        {
            Callsign = callsign;
            Target = target;
            Reason = reason;
        }

        public string Serialize()
        {
            return Command + Callsign + ":" + Target + ":" + Reason;
        }
    }
}