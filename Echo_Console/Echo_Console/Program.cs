using System;
using System.Diagnostics;
using System.Net;
using System.Timers;
using Echo_Console.Net;
using Echo_Console.Protocol;

namespace Echo_Console
{
    internal class Program
    {
        private const string Callsign = "900";
        private const string Cid = "900";
        private const string Password = "a786013819+";
        private const string Server = "SERVER";
        private readonly Timer _positionReportTimer;

        public Program()
        {
            Client = new EchoTcpClient();
            Client.Connected += Connected;
            Client.DataReceived += DataReceived;
            Client.Disconnected += Disconnected;
            Client.ExceptionThrown += ExceptionThrown;
            Client.StateChanged += StateChanged;
            _positionReportTimer = new Timer();
            _positionReportTimer.Elapsed += PositionReportTimer_Tick;
            _positionReportTimer.Interval = 5000;
            _positionReportTimer.Enabled = false;
        }

        public EchoTcpClient Client { get; set; }
        public SocketState State { get; } = SocketState.Closed;

        public void PositionReportTimer_Tick(object sender, ElapsedEventArgs e)
        {
            if (Client == null || Client.State != ClientState.Connected) return;
            var pilotPositionReport = new PilotPositionReport("S", Callsign, "1200", "1", "49.60581", "126.23750",
                "100.24880", "0", "8391656", "4");
            SendString(pilotPositionReport.Serialize());
        }

        public void SendString(string content)
        {
            Client.Send(content);
            Debug.WriteLine($"Send: {content}");
        }

        public void Connected(object sender, EchoConnectedEventArgs e)
        {
            Console.WriteLine($"We have connected to {e.SourceIp}");
            var pilotConnectRequest = new PilotConnectRequest(Callsign, Server, Cid, Password, "1", "9", "2",
                "Connection From ECHO Console Alpha");
            SendString(pilotConnectRequest.Serialize());
            var pilotPositionReport = new PilotPositionReport("S", Callsign, "1200", "1", "45.60581", "-126.23750",
                "462.24880", "0", "8391656", "4");
            SendString("$CQ" + Callsign + ":" + Server + ":IP\r\n" + pilotPositionReport.Serialize());
            _positionReportTimer.Enabled = true;
        }

        public void DataReceived(object sender, EchoDataReceivedEventArgs e)
        {
            Debug.WriteLine($"Receive: {e.Data}");
            if (e.Data.Contains("#TM"))
                Console.WriteLine($"Receive: {e.Data}");
        }

        public void Disconnected(object sender, EchoDisconnectedEventArgs e)
        {
            Console.WriteLine($"We have disconnected from server, reason: {e.Reason}");
            _positionReportTimer.Enabled = false;
        }

        public void ExceptionThrown(object sender, EchoExceptionThrownEventArgs e)
        {
            Console.WriteLine($"There was an exception has been thrown: {e.Function}, {e.Exception.Message}");
            _positionReportTimer.Enabled = false;
        }

        public void StateChanged(object sender, EchoStateChangedEventArgs e)
        {
            Console.WriteLine($"Connection State has changed from {e.PrevState} to {e.NewState}");
        }

        public static void Main(string[] args)
        {
            var program = new Program();
            var ipep = new IPEndPoint(IPAddress.Parse("121.40.103.105"), 6809);
            program.Client.Connect(ipep);
            while (true)
            {
                Console.Write(">");
                var cmd = Console.ReadLine();
                if (cmd == null) return;
                if (cmd.ToLower().Equals("on"))
                {
                    program.Client.Connect(ipep);
                }
                else if (cmd.ToLower().Equals("off"))
                {
                    var disconnenctedFromServer = new DisconnectFromServer(Callsign, Server);
                    program.SendString(disconnenctedFromServer.Serialize());
                    program.Client.Close("commanded");
                }
                else if (cmd.ToLower().Equals("weather"))
                {
                    var weather = new RequestWeather(Callsign, Server, "ZSSS");
                    program.SendString(weather.Serialize());
                }
                else if (cmd.ToLower().Equals("msg"))
                {
                    Console.Write("Target: ");
                    var target = Console.ReadLine();
                    if (target == null) continue;
                    Console.Write("Message: ");
                    var message = Console.ReadLine();
                    if (message == null) continue;
                    var textMessage = new SendTextMessage(Callsign, target, message);
                    program.SendString(textMessage.Serialize());
                }
                else if (cmd.ToLower().Equals("exit"))
                {
                    return;
                }
            }
        }
    }
}