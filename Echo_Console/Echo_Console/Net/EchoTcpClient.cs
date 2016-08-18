using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Echo_Console.Net
{
    #region Enums

    public enum ClientState
    {
        Closed,
        Closing,
        Connected,
        Connecting
    }

    #endregion

    #region Event Args
    public class EchoConnectedEventArgs : EventArgs
    {
        public IPAddress SourceIp;

        public EchoConnectedEventArgs(IPAddress ip)
        {
            SourceIp = ip;
        }
    }

    public class EchoDisconnectedEventArgs : EventArgs
    {
        public string Reason;

        public EchoDisconnectedEventArgs(string reason)
        {
            Reason = reason;
        }
    }

    public class EchoStateChangedEventArgs : EventArgs
    {
        public ClientState NewState;
        public ClientState PrevState;

        public EchoStateChangedEventArgs(ClientState newState, ClientState prevState)
        {
            NewState = newState;
            PrevState = prevState;
        }
    }

    public class EchoDataReceivedEventArgs : EventArgs
    {
        public string Data;

        public EchoDataReceivedEventArgs(string data)
        {
            Data = data;
        }
    }

    public class EchoExceptionThrownEventArgs : EventArgs
    {
        public Exception Exception;
        public string Function;

        public EchoExceptionThrownEventArgs(string function, Exception ex)
        {
            Function = function;
            Exception = ex;
        }
    }
    #endregion

    public class EchoTcpClient
    {
        protected ClientState state = ClientState.Closed;

        public TcpClient TcpClient;
        /// Threaded timer checks if socket is busted
        protected Timer ConnectionTimer;

        /// Interval for socket checks(ms)
        protected int ConnectionCheckInterval = 1000;

        public ClientState State => state;

        /// Socket is connected
        public event EventHandler<EchoConnectedEventArgs> Connected;

        /// Socket connection closed
        public event EventHandler<EchoDisconnectedEventArgs> Disconnected;

        /// Socket state has changed
        /// This has the ability to fire very rapidly during connection / disconnection.
        public event EventHandler<EchoStateChangedEventArgs> StateChanged;

        /// Recived a new object
        public event EventHandler<EchoDataReceivedEventArgs> DataReceived;

        /// An error has occurred
        public event EventHandler<EchoExceptionThrownEventArgs> ExceptionThrown;

        public EchoTcpClient()
        {
            ConnectionTimer = new Timer(ConnectionTimerCallback, null, Timeout.Infinite, Timeout.Infinite);
        }

        public void Connect(IPEndPoint ipep)
        {
            if (state == ClientState.Connected || state == ClientState.Connecting)
                return;

            TcpClient = new TcpClient();
            try
            {
                ChangeState(ClientState.Connecting);
                TcpClient.Connect(ipep);
                ThreadPool.QueueUserWorkItem(Receive);
                ChangeState(ClientState.Connected);
                OnConnected(ipep.Address);
            }
            catch (SocketException ex)
            {
                OnExceptionThrown("Connect", ex);
            }
            
        }

        public void Receive(object sender)
        {
            var ns = TcpClient.GetStream();
            var sr = new StreamReader(ns, Encoding.Default);
            while (true)
            {
                try
                {
                    var data = sr.ReadLine();
                    if (data != null && !data.Equals(""))
                    {
                        OnDataReceived(data);
                    }
                }
                catch (IOException)
                {
                    return;
                }
                catch (Exception ex)
                {
                    OnExceptionThrown("Receive", ex);
                    return;
                }
            }
        }

        public void Send(string text)
        {
            var ns = TcpClient.GetStream();
            var sw = new StreamWriter(ns, Encoding.Default);
            sw.WriteLineAsync(text);
            sw.FlushAsync();
        }

        public void Close(string reason)
        {
            try
            {
                if (state == ClientState.Closed || state == ClientState.Closing)
                    return;
                ChangeState(ClientState.Closing);
                if (TcpClient != null)
                {
                    TcpClient.Close();
                    TcpClient = null;
                }
                ChangeState(ClientState.Closed);
                OnDisconnected(reason);
            }
            catch (Exception ex)
            {
                OnExceptionThrown("Close", ex);
            }
        }

        public void OnConnected(IPAddress ip)
        {
            Connected?.Invoke(this, new EchoConnectedEventArgs(ip));
        }

        public void OnDataReceived(string data)
        {
            DataReceived?.Invoke(this, new EchoDataReceivedEventArgs(data));
        }

        public void OnDisconnected(string reason)
        {
            Disconnected?.Invoke(this, new EchoDisconnectedEventArgs(reason));
        }

        public void OnExceptionThrown(string function, Exception ex)
        {
            ExceptionThrown?.Invoke(this, new EchoExceptionThrownEventArgs(function, ex));
        }

        public void ChangeState(ClientState newState)
        {
            var prevState = state;
            state = newState;
            StateChanged?.Invoke(this, new EchoStateChangedEventArgs(newState, prevState));
        }

        public void ConnectionTimerCallback(object sender)
        {
            try
            {
                if (state == ClientState.Connected && (TcpClient == null || !TcpClient.Connected))
                {
                    Close("Connection Timer");
                }
            }
            catch (Exception ex)
            {
                OnExceptionThrown("ConnectionTimer", ex);
                Close("Connect Timer Exception");
            }
        }
    }
}
