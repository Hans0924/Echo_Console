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
        public string Callsign = "900";
        private const string Cid = "900";
        private const string Password = "";
        private const string Server = "SERVER";
        private const string Lat = "27.837727";
        private const string Lon = "112.993988";
        private readonly Timer _positionReportTimer;
        private readonly Timer _pingTimer;

        public string[] Commands = {"", "#AP", "#AA", "@", "%", "$FP", "!!", "#TM", "$PI", "$PO", "$CQ", "$CR", "#SB", "$AX", "$AR", "#WX", "#DP", "#DA", "#ER"};

        public enum EnumCommand
        {
            Unknown,
            AddPilot,
            AddAtc,
            PilotPosition,
            AtcPosition,
            FlightPlan,
            Kill,
            TextMessage,
            Ping,
            Pong,
            ClientQuery,
            ClientResponse,
            SquawkBox,
            AcarsQuery,
            AcarsResponse,
            WeatherRequest,
            DeletePilot,
            DeleteAtc,
            ErrorResponse
        }
        
        public Program()
        {
            Client = new EchoTcpClient();
            Client.Connected += Connected;
            Client.DataReceived += DataReceived;
            Client.Disconnected += Disconnected;
            //Client.ExceptionThrown += ExceptionThrown;
            Client.StateChanged += StateChanged;
            _positionReportTimer = new Timer();
            _positionReportTimer.Elapsed += PositionReportTimer_Tick;
            _positionReportTimer.Interval = 5000;
            _positionReportTimer.Enabled = false;
            _pingTimer = new Timer();
            _pingTimer.Elapsed += PingTimer_Tick;
            _pingTimer.Interval = 5000;
            _pingTimer.Enabled = false;
        }

        public EchoTcpClient Client { get; set; }
        public ClientState State { get; } = ClientState.Closed;

        public void PositionReportTimer_Tick(object sender, ElapsedEventArgs e)
        {
            if (Client == null || Client.State != ClientState.Connected) return;
            var pilotPositionReport = new PilotPosition("S", Callsign, "1200", "1", Lat, Lon,
                "0.24880", "0", "8391656", "4");
            SendPacket(pilotPositionReport.Serialize());
        }

        public void PingTimer_Tick(object sender, ElapsedEventArgs e)
        {
            if (Client == null || Client.State != ClientState.Connected) return;
            var dateTime = DateTime.UtcNow;
            var start = new DateTime(1970, 1, 1, 0, 0, 0, dateTime.Kind);
            var timestamp = Convert.ToInt64((dateTime - start).TotalSeconds);
            var ping = new Ping(Callsign, Server, timestamp.ToString());
            SendPacket(ping.Serialize());
        }

        public void SendPacket(string content)
        {
            Client.Send(content);
            Debug.WriteLine($"Send: {content}");
        }

        public void Print(string str)
        {
            Console.WriteLine("\r" + str);
        }

        public void Connected(object sender, EchoConnectedEventArgs e)
        {
            Print($"We have connected to {e.SourceIp}");
            var pilotConnectRequest = new AddPilot(Callsign, Server, Cid, Password, "1", "9", "2",
                "Connection From ECHO Console Alpha");
            SendPacket(pilotConnectRequest.Serialize());
            var clientQuery = new ClientQuery(Callsign, Server, "IP");
            var pilotPositionReport = new PilotPosition("S", Callsign, "1200", "1", Lat, Lon,
                "0.24880", "0", "8391656", "4");
            SendPacket(clientQuery.Serialize());
            SendPacket(pilotPositionReport.Serialize());
            _positionReportTimer.Enabled = true;
            _pingTimer.Enabled = true;
        }

        public void DataReceived(object sender, EchoDataReceivedEventArgs e)
        {
            var command = e.Data.Split(':')[0];
            var index = EnumCommand.Unknown;
            for (var i = 1; i < Commands.Length; ++i)
            {
                if (command.IndexOf(Commands[i], StringComparison.Ordinal) == -1) continue;
                index = (EnumCommand) Enum.ToObject(typeof(EnumCommand), i);
                break;
            }
            switch (index)
            {
                case EnumCommand.AddPilot:
                    var ap = new AddPilot(e.Data);
                    Print($"Server: We have a new pilot {ap.Callsign}.");
                    break;
                case EnumCommand.AddAtc:
                    var aa = new AddAtc(e.Data);
                    Print($"Server: We have a new controller {aa.Source}.");
                    break;
                case EnumCommand.PilotPosition:
                    var pp = new PilotPosition(e.Data);
                    //Print($"Server: {pp.Source} has reported its position.");
                    break;
                case EnumCommand.AtcPosition:
                    var atcPosition = new AtcPosition(e.Data);
                    Print($"Server: {atcPosition.Source} has reported its position.");
                    break;
                case EnumCommand.FlightPlan:
                    var fp = new FlightPlan(e.Data);
                    Print($"Server: {fp.Source} has submitted its flight plan({fp.DepartureAirport}-{fp.ArrivalAirport}).");
                    break;
                case EnumCommand.Kill:
                    var kill = new KillFromServer(e.Data);
                    Print($"Server: {kill.Target} has been killed.");
                    break;
                case EnumCommand.TextMessage:
                    var tm = new TextMessage(e.Data);
                    Callsign = tm.Destination;
                    Print($"{tm.Source}=>{tm.Destination}: {tm.Message}");
                    break;
                case EnumCommand.Pong:
                    var po = new Pong(e.Data);
                    //Print($"Server: Pong {po.Timestamp}");
                    break;
                case EnumCommand.ClientQuery:
                    var cq = new ClientQuery(e.Data);
                    var param = cq.SplitParam();
                    ClientResponse cr = null;
                    if (param[0].Equals("RN"))
                    {
                        Print($"Server: {cq.Source} request your RealName.");
                        cr = new ClientResponse(Callsign, cq.Source, "RN:Hans Zeng");
                    }
                    else if (param[0].Equals("CAPS"))
                    {
                        Print($"Server: {cq.Source} request your CAPS.");
                        cr = new ClientResponse(Callsign, cq.Source, "CAPS::MODELDESC=1:ATCINFO=1");
                    }
                    if (cr != null)
                        SendPacket(cr.Serialize());
                    break;
                case EnumCommand.ClientResponse:
                    cr = new ClientResponse(e.Data);
                    param = cr.SplitParam();
                    if (param[0].Equals("RN"))
                    {
                        Print($"Server: {cr.Source} response your request of RealName: {param[1]}.");
                    }
                    else if (param[0].Equals("CAPS"))
                    {
                        Print($"Server: {cr.Source} response your request of CAPS: {cr.Parameters.Substring(5)}.");
                    }
                    break;
                case EnumCommand.SquawkBox:
                    var sb = new SquawkBox(e.Data);
                    param = sb.SplitParam();
                    if (sb.Destination == Callsign)
                    {
                        /*Print(param.Count == 1
                            ? $"Server: {sb.Source} request your {sb.Parameters}"
                            : $"Server: {sb.Source} response you {sb.Parameters}");*/
                    }
                    else
                    {
                        /*Print(param.Count == 1
                            ? $"Server: {sb.Source} request {sb.Destination}'s {sb.Parameters}"
                            : $"Server: {sb.Source} response {sb.Destination} {sb.Parameters}");*/
                    }
                    break;
                case EnumCommand.AcarsResponse:
                    var ax = new AcarsResponse(e.Data);
                    param = ax.SplitParam();
                    if (param[0] == "METAR")
                    {
                        Print($"Server: {param[1]}");
                    }
                    break;
                case EnumCommand.DeletePilot:
                    var dp = new DeletePilot(e.Data);
                    Print($"Server: {dp.Source} has disconnected.");
                    break;
                case EnumCommand.DeleteAtc:
                    var da = new DeleteAtc();
                    Print($"Server: {da.Source} has disconnected.");
                    break;
                case EnumCommand.ErrorResponse:
                    var er = new ErrorResponse(e.Data);
                    Print($"Error: {er.ErrorCode} - {er.Content}.");
                    break;
                default:
                    Debug.WriteLine($"Receive: {e.Data}");
                    break;
            }
        }

        public void Disconnected(object sender, EchoDisconnectedEventArgs e)
        {
            Print($"We have disconnected from server, reason: {e.Reason}");
            _positionReportTimer.Enabled = false;
        }

        public void ExceptionThrown(object sender, EchoExceptionThrownEventArgs e)
        {
            Print($"There was an exception has been thrown: {e.Function}, {e.Exception.Message}");
            _positionReportTimer.Enabled = false;
        }

        public void StateChanged(object sender, EchoStateChangedEventArgs e)
        {
            Print($"Connection State has changed from {e.PrevState} to {e.NewState}");
        }

        public static void Main(string[] args)
        {
            var program = new Program();
            var ipep = new IPEndPoint(IPAddress.Parse(""), 6809);
            program.Client.Connect(ipep);
            while (true)
            {
                Console.Write("> ");
                var cmd = Console.ReadLine();
                if (cmd == null) return;
                if (cmd.ToLower().Equals("on"))
                {
                    program.Client.Connect(ipep);
                }
                else if (cmd.ToLower().Equals("off"))
                {
                    var disconnenctedFromServer = new DeletePilot(program.Callsign, Server);
                    program.SendPacket(disconnenctedFromServer.Serialize());
                    program.Client.Close("commanded");
                }
                else if (cmd.ToLower().Equals("weather"))
                {
                    var weather = new WeatherRequest(program.Callsign, Server, "ZSSS");
                    program.SendPacket(weather.Serialize());
                }
                else if (cmd.ToLower().Equals("msg"))
                {
                    Console.Write("Target: ");
                    var target = Console.ReadLine();
                    if (target == null) continue;
                    Console.Write("Message: ");
                    var message = Console.ReadLine();
                    if (message == null) continue;
                    var textMessage = new TextMessage(program.Callsign, target, message);
                    program.SendPacket(textMessage.Serialize());
                }
                else if (cmd.ToLower().Equals("fp"))
                {
                    var fp = new FlightPlan(program.Callsign, Server, "IFR", "C172", "120", "ZGSZ", "", "", 
                        "3900", "ZGSZ", "", "", "", "", "", "地面测试", "");
                    program.SendPacket(fp.Serialize());
                }
                else if (cmd.ToLower().Equals("exit"))
                {
                    return;
                }
            }
        }
    }
}