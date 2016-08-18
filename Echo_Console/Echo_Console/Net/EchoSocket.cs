using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Threading;

namespace Echo_Console.Net
{

    #region Enums

    public enum SocketState
    {
        Closed,
        Closing,
        Connected,
        Connecting,
        Listening
    }

    #endregion

    #region Event Args

    public class EchoSocketConnectedEventArgs : EventArgs
    {
        public IPAddress SourceIp;

        public EchoSocketConnectedEventArgs(IPAddress ip)
        {
            SourceIp = ip;
        }
    }

    public class EchoSocketDisconnectedEventArgs : EventArgs
    {
        public string Reason;

        public EchoSocketDisconnectedEventArgs(string reason)
        {
            Reason = reason;
        }
    }

    public class EchoSocketStateChangedEventArgs : EventArgs
    {
        public SocketState NewState;
        public SocketState PrevState;

        public EchoSocketStateChangedEventArgs(SocketState newState, SocketState prevState)
        {
            NewState = newState;
            PrevState = prevState;
        }
    }

    public class EchoSocketDataReceivedEventArgs : EventArgs
    {
        public byte[] Data;

        public EchoSocketDataReceivedEventArgs(byte[] data)
        {
            Data = data;
        }
    }

    public class EchoSocketExceptionThrownEventArgs : EventArgs
    {
        public Exception Exception;
        public string Function;

        public EchoSocketExceptionThrownEventArgs(string function, Exception ex)
        {
            Function = function;
            Exception = ex;
        }
    }

    public class EchoSocketConnectionRequestEventArgs : EventArgs
    {
        public Socket Client;

        public EchoSocketConnectionRequestEventArgs(Socket client)
        {
            Client = client;
        }
    }

    #endregion

    #region Socket Classes

    public abstract class EchoSocketBase
    {
        #region Constructor

        /// Base constructor sets up buffer and timer
        public EchoSocketBase()
        {
            ConnectionTimer = new Timer(
                ConnectedTimerCallback,
                null, Timeout.Infinite, Timeout.Infinite);
        }

        #endregion

        #region Close

        /// Disconnect the socket
        public void Close(string reason)
        {
            try
            {
                if (state == SocketState.Closing || state == SocketState.Closed)
                    return; // already closing/closed

                OnChangeState(SocketState.Closing);

                if (Socket != null)
                {
                    Socket.Close();
                    Socket = null;
                }
            }
            catch (Exception ex)
            {
                OnErrorReceived("Close", ex);
            }

            try
            {
                if (RxBuffer.Length > 0)
                {
                    if (RxHeaderIndex > -1 && RxBodyLen > -1)
                    {
                        // start of message - length of header
                        var msgbytes = (int) RxBuffer.Length - RxHeaderIndex - BomBytes.Count - sizeof(int);
                        OnErrorReceived("Close Buffer",
                            new Exception("Incomplete Message (" + msgbytes + " of " + RxBodyLen + " bytes received)"));
                    }
                    else
                    {
                        OnErrorReceived("Close Buffer", new Exception("Unprocessed data " + RxBuffer.Length + " bytes"));
                    }
                }
            }
            catch (Exception ex)
            {
                OnErrorReceived("Close Buffer", ex);
            }

            try
            {
                lock (RxBuffer)
                {
                    RxBuffer.SetLength(0);
                }
                lock (SendBuffer)
                {
                    SendBuffer.Clear();
                    IsSending = false;
                }
                OnChangeState(SocketState.Closed);
                Disconnected?.Invoke(this, new EchoSocketDisconnectedEventArgs(reason));
            }
            catch (Exception ex)
            {
                OnErrorReceived("Close Cleanup", ex);
            }
        }

        #endregion

        #region Connection Sanity Check

        private void ConnectedTimerCallback(object sender)
        {
            try
            {
                if (state == SocketState.Connected &&
                    (Socket == null || !Socket.Connected))
                    Close("Connect Timer");
            }
            catch (Exception ex)
            {
                OnErrorReceived("ConnectTimer", ex);
                Close("Connect Timer Exception");
            }
        }

        #endregion

        #region Fields

        /// Current socket state
        protected SocketState state = SocketState.Closed;

        /// The socket object, obviously
        protected Socket Socket;

        /// Keep track of when data is being sent
        protected bool IsSending;

        /// Queue of objects to be sent out
        protected Queue<byte[]> SendBuffer = new Queue<byte[]>();

        /// Store incoming bytes to be processed
        protected byte[] ByteBuffer = new byte[8192];

        /// Position of the bom header in the rxBuffer
        protected int RxHeaderIndex = -1;

        /// Expected length of the message from the bom header
        protected int RxBodyLen = -1;

        /// Buffer of received data
        protected MemoryStream RxBuffer = new MemoryStream();

        /// Beginning of message indicator
        protected ArraySegment<byte> BomBytes = new ArraySegment<byte>(new byte[] {1, 2, 1, 255});

        /// TCP inactivity before sending keep-alive packet(ms)
        protected uint KeepAliveInactivity = 500;

        /// Interval to send keep-alive packet if acknowledgement was not received(ms)
        protected uint KeepAliveInterval = 100;

        /// Threaded timer checks if socket is busted
        protected Timer ConnectionTimer;

        /// Interval for socket checks(ms)
        protected int ConnectionCheckInterval = 1000;

        #endregion

        #region Public Properties

        /// Current state of the socket
        public SocketState State => state;

        /// Port the socket control is listening on.
        public int LocalPort
        {
            get
            {
                try
                {
                    return ((IPEndPoint) Socket.LocalEndPoint).Port;
                }
                catch
                {
                    return -1;
                }
            }
        }

        /// IP address enumeration for local computer
        public static string[] LocalIp
        {
            get
            {
                var h = Dns.GetHostEntry(Dns.GetHostName());
                var s = new List<string>(h.AddressList.Length);
                s.AddRange(h.AddressList.Select(i => i.ToString()));
                return s.ToArray();
            }
        }

        #endregion

        #region Events

        /// Socket is connected
        public event EventHandler<EchoSocketConnectedEventArgs> Connected;

        /// Socket connection closed
        public event EventHandler<EchoSocketDisconnectedEventArgs> Disconnected;

        /// Socket state has changed
        /// This has the ability to fire very rapidly during connection / disconnection.
        public event EventHandler<EchoSocketStateChangedEventArgs> StateChanged;

        /// Recived a new object
        public event EventHandler<EchoSocketDataReceivedEventArgs> DataReceived;

        /// An error has occurred
        public event EventHandler<EchoSocketExceptionThrownEventArgs> ExceptionThrown;

        #endregion

        #region Send

        /// Send data
        /// Bytes to send
        public void Send(byte[] data)
        {
            try
            {
                if (data == null)
                    throw new NullReferenceException("data cannot be null");
                if (data.Length == 0)
                    throw new NullReferenceException("data cannot be empty");
                lock (SendBuffer)
                {
                    SendBuffer.Enqueue(data);
                }

                if (IsSending) return;
                IsSending = true;
                SendNextQueued();
            }
            catch (Exception ex)
            {
                OnErrorReceived("Send", ex);
            }
        }

        /// Send data for real
        private void SendNextQueued()
        {
            try
            {
                var send = new List<ArraySegment<byte>>(3);
                lock (SendBuffer)
                {
                    if (SendBuffer.Count == 0)
                    {
                        IsSending = false;
                        return; // nothing more to send
                    }

                    var data = SendBuffer.Dequeue();
                    send.Add(BomBytes);
                    send.Add(new ArraySegment<byte>(BitConverter.GetBytes(data.Length)));
                    send.Add(new ArraySegment<byte>(data));
                }
                Socket.BeginSend(send, SocketFlags.None, SendCallback, Socket);
            }
            catch (Exception ex)
            {
                OnErrorReceived("Sending", ex);
            }
        }

        /// Callback for BeginSend
        private void SendCallback(IAsyncResult ar)
        {
            try
            {
                var sock = (Socket) ar.AsyncState;
                sock?.EndSend(ar);

                if (Socket != sock)
                {
                    Close("Async Connect Socket mismatched");
                    return;
                }

                SendNextQueued();
            }
            catch (ObjectDisposedException)
            {
            }
            catch (SocketException ex)
            {
                if (ex.SocketErrorCode == SocketError.ConnectionReset)
                    Close("Remote Socket Closed");
                else
                    throw;
            }
            catch (Exception ex)
            {
                Close("Socket Send Exception");
                OnErrorReceived("Socket Send", ex);
            }
        }

        #endregion

        #region Receive

        /// Receive data asynchronously
        protected void Receive()
        {
            try
            {
                Socket.BeginReceive(ByteBuffer, 0, ByteBuffer.Length, SocketFlags.None, ReceiveCallback, Socket);
            }
            catch (Exception ex)
            {
                OnErrorReceived("Receive", ex);
            }
        }

        /// Callback for BeginReceive
        private void ReceiveCallback(IAsyncResult ar)
        {
            try
            {
                var sock = (Socket) ar.AsyncState;
                var size = sock.EndReceive(ar);

                if (Socket != sock)
                {
                    Close("Async Receive Socket mismatched");
                    return;
                }

                if (size < 1)
                {
                    Close("No Bytes Received");
                    return;
                }

                lock (RxBuffer)
                {
                    // put at the end for safe writing
                    RxBuffer.Position = RxBuffer.Length;
                    RxBuffer.Write(ByteBuffer, 0, size);

                    var more = false;
                    do
                    {
                        // search for header if not found yet
                        if (RxHeaderIndex < 0)
                        {
                            RxBuffer.Position = 0; // rewind to search
                            RxHeaderIndex = IndexOfBytesInStream(RxBuffer, BomBytes.Array);
                        }

                        // have the header
                        if (RxHeaderIndex <= -1) continue;
                        // read the body length from header
                        if (RxBodyLen < 0 && RxBuffer.Length - RxHeaderIndex - BomBytes.Count >= 4)
                        {
                            RxBuffer.Position = RxHeaderIndex + BomBytes.Count; // start reading after bomBytes
                            RxBuffer.Read(ByteBuffer, 0, 4); // read message length
                            RxBodyLen = BitConverter.ToInt32(ByteBuffer, 0);
                        }

                        // we have the message
                        if (RxBodyLen > -1 && RxBuffer.Length - RxHeaderIndex - BomBytes.Count - 4 >= RxBodyLen)
                        {
                            try
                            {
                                RxBuffer.Position = RxHeaderIndex + BomBytes.Count + sizeof(int);
                                var data = new byte[RxBodyLen];
                                RxBuffer.Read(data, 0, data.Length);
                                DataReceived?.Invoke(this, new EchoSocketDataReceivedEventArgs(data));
                            }
                            catch (Exception ex)
                            {
                                OnErrorReceived("Receiving", ex);
                            }

                            if (RxBuffer.Position == RxBuffer.Length)
                            {
                                // no bytes left
                                // just resize buffer
                                RxBuffer.SetLength(0);
                                RxBuffer.Capacity = ByteBuffer.Length;
                                more = false;
                            }
                            else
                            {
                                // leftover bytes after current message
                                // copy these bytes to the beginning of the rxBuffer
                                CopyBack();
                                more = true;
                            }

                            // reset header info
                            RxHeaderIndex = -1;
                            RxBodyLen = -1;
                        }
                        else if (RxHeaderIndex > 0)
                        {
                            // remove bytes from before the header
                            RxBuffer.Position = RxHeaderIndex;
                            CopyBack();
                            RxHeaderIndex = 0;
                            more = false;
                        }
                        else
                            more = false;
                    } while (more);
                }
                Socket.BeginReceive(ByteBuffer, 0, ByteBuffer.Length, SocketFlags.None, ReceiveCallback, Socket);
            }
            catch (ObjectDisposedException)
            {
            }
            catch (SocketException ex)
            {
                if (ex.SocketErrorCode == SocketError.ConnectionReset)
                    Close("Remote Socket Closed");
                else
                    throw;
            }
            catch (Exception ex)
            {
                Close("Socket Receive Exception");
                OnErrorReceived("Socket Receive", ex);
            }
        }

        /// Copies the stuff after the current position, back to the start of the stream,
        /// resizes the stream to only include the new content, and
        /// limits the capacity to length + another buffer.
        private void CopyBack()
        {
            int count;
            long writePos = 0;
            do
            {
                count = RxBuffer.Read(ByteBuffer, 0, ByteBuffer.Length);
                var readPos = RxBuffer.Position;
                RxBuffer.Position = writePos;
                RxBuffer.Write(ByteBuffer, 0, count);
                writePos = RxBuffer.Position;
                RxBuffer.Position = readPos;
            } while (count > 0);
            RxBuffer.SetLength(writePos);
            RxBuffer.Capacity = (int) RxBuffer.Length + ByteBuffer.Length;
        }

        /// Find first position the specified byte within the stream, or -1 if not found
        protected virtual int IndexOfByteInStream(Stream ms, byte find)
        {
            int b;
            do
            {
                b = ms.ReadByte();
            } while (b > -1 && b != find);

            if (b == -1)
                return -1;
            return (int) ms.Position - 1; // position is +1 byte after the byte we want
        }

        /// Find first position the specified bytes within the stream, or -1 if not found
        private int IndexOfBytesInStream(Stream ms, IReadOnlyList<byte> find)
        {
            int index;
            do
            {
                index = IndexOfByteInStream(ms, find[0]);

                if (index <= -1) continue;
                var found = true;
                for (var i = 1; i < find.Count; i++)
                {
                    if (find[i] == ms.ReadByte()) continue;
                    found = false;
                    ms.Position = index + 1;
                    break;
                }
                if (found)
                    return index;
            } while (index > -1);
            return -1;
        }

        #endregion

        #region OnEvents

        protected void OnErrorReceived(string function, Exception ex)
        {
            ExceptionThrown?.Invoke(this, new EchoSocketExceptionThrownEventArgs(function, ex));
        }

        protected void OnConnected(Socket sock)
        {
            Connected?.Invoke(this, new EchoSocketConnectedEventArgs(((IPEndPoint) sock.RemoteEndPoint).Address));
        }

        protected void OnChangeState(SocketState newState)
        {
            var prev = state;
            state = newState;
            StateChanged?.Invoke(this, new EchoSocketStateChangedEventArgs(state, prev));

            if (state != SocketState.Connected)
            {
                if (state == SocketState.Closed)
                {
                    ConnectionTimer.Change(Timeout.Infinite, Timeout.Infinite);
                }
            }
            else
            {
                ConnectionTimer.Change(0, ConnectionCheckInterval);
            }
        }

        #endregion

        #region Keep-alives

        /*
		 * Note about usage of keep-alives
		 * The TCP protocol does not successfully detect "abnormal" socket disconnects at both
		 * the client and server end. These are disconnects due to a computer crash, cable 
		 * disconnect, or other failure. The keep-alive mechanism built into the TCP socket can
		 * detect these disconnects by essentially sending null data packets (header only) and
		 * waiting for acks.
		 */

        /// Structure for settings keep-alive bytes
        [StructLayout(LayoutKind.Sequential)]
        private struct TcpKeepalive
        {
            /// 1 = on, 0 = off
            public uint onoff;

            /// TCP inactivity before sending keep-alive packet(ms)
            public uint keepalivetime;

            /// Interval to send keep-alive packet if acknowledgement was not received(ms)
            public uint keepaliveinterval;
        }

        /// Set up the socket to use TCP keep alive messages
        protected void SetKeepAlive()
        {
            try
            {
                var sioKeepAliveVals = new TcpKeepalive
                {
                    onoff = 1, // 1 to enable 0 to disable
                    keepalivetime = KeepAliveInactivity,
                    keepaliveinterval = KeepAliveInterval
                };
                var p = Marshal.AllocHGlobal(Marshal.SizeOf(sioKeepAliveVals));
                Marshal.StructureToPtr(sioKeepAliveVals, p, true);
                var inBytes = new byte[Marshal.SizeOf(sioKeepAliveVals)];
                Marshal.Copy(p, inBytes, 0, inBytes.Length);
                Marshal.FreeHGlobal(p);

                var outBytes = BitConverter.GetBytes(0);
                Socket.IOControl(IOControlCode.KeepAliveValues, inBytes, outBytes);
            }
            catch (Exception ex)
            {
                OnErrorReceived("Keep Alive", ex);
            }
        }

        #endregion
    }

    public class EchoSocketServer : EchoSocketBase
    {
        #region Events

        /// A socket has requested a connection
        public event EventHandler ConnectionRequested;

        #endregion

        #region Accept

        /// Accept the connection request
        /// Client socket to accept
        public void Accept(Socket client)
        {
            try
            {
                if (state != SocketState.Listening)
                    throw new Exception("Cannot accept socket is " + state);

                if (Socket != null)
                {
                    try
                    {
                        Socket.Close(); // close listening socket
                    }
                    catch
                    {
                        // ignored
                    } // don't care if this fails
                }

                Socket = client;

                Socket.ReceiveBufferSize = ByteBuffer.Length;
                Socket.SendBufferSize = ByteBuffer.Length;

                SetKeepAlive();

                OnChangeState(SocketState.Connected);
                OnConnected(Socket);

                Receive();
            }
            catch (Exception ex)
            {
                OnErrorReceived("Accept", ex);
            }
        }

        #endregion

        #region Listen

        /// Listen for incoming connections
        /// Port to listen on
        public void Listen(int port)
        {
            try
            {
                if (Socket != null)
                {
                    try
                    {
                        Socket.Close();
                    }
                    catch
                    {
                        // ignored
                    }
                    // ignore problems with old socket
                }
                Socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

                var ipLocal = new IPEndPoint(IPAddress.Any, port);
                Socket.Bind(ipLocal);
                Socket.Listen(1);
                Socket.BeginAccept(AcceptCallback, Socket);
                OnChangeState(SocketState.Listening);
            }
            catch (Exception ex)
            {
                OnErrorReceived("Listen", ex);
            }
        }

        /// Callback for BeginAccept
        private void AcceptCallback(IAsyncResult ar)
        {
            try
            {
                var listener = (Socket) ar.AsyncState;
                var sock = listener.EndAccept(ar);

                if (state == SocketState.Listening)
                {
                    if (Socket != listener)
                    {
                        Close("Async Listen Socket mismatched");
                        return;
                    }

                    ConnectionRequested?.Invoke(this, new EchoSocketConnectionRequestEventArgs(sock));
                }

                if (state == SocketState.Listening)
                    Socket.BeginAccept(AcceptCallback, listener);
                else
                {
                    try
                    {
                        listener.Close();
                    }
                    catch (Exception ex)
                    {
                        OnErrorReceived("Close Listen Socket", ex);
                    }
                }
            }
            catch (ObjectDisposedException)
            {
            }
            catch (SocketException ex)
            {
                Close("Listen Socket Exception");
                OnErrorReceived("Listen Socket", ex);
            }
            catch (Exception ex)
            {
                OnErrorReceived("Listen Socket", ex);
            }
        }

        #endregion
    }

    public class EchoSocketClient : EchoSocketBase
    {
        #region Constructor

        #endregion

        #region Connect

        /// Connect to the computer specified by Host and Port
        public void Connect(IPEndPoint endPoint)
        {
            if (state == SocketState.Connected)
                return; // already connecting to something

            try
            {
                if (state != SocketState.Closed)
                    throw new Exception("Cannot connect socket is " + state);

                OnChangeState(SocketState.Connecting);

                if (Socket == null)
                    Socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

                Socket.BeginConnect(endPoint, ConnectCallback, Socket);
            }
            catch (Exception ex)
            {
                OnErrorReceived("Connect", ex);
                Close("Connect Exception");
            }
        }

        /// Callback for BeginConnect
        private void ConnectCallback(IAsyncResult ar)
        {
            try
            {
                var sock = (Socket) ar.AsyncState;
                sock.EndConnect(ar);

                if (Socket != sock)
                {
                    Close("Async Connect Socket mismatched");
                    return;
                }

                if (state != SocketState.Connecting)
                    throw new Exception("Cannot connect socket is " + state);

                Socket.ReceiveBufferSize = ByteBuffer.Length;
                Socket.SendBufferSize = ByteBuffer.Length;

                SetKeepAlive();

                OnChangeState(SocketState.Connected);
                OnConnected(Socket);

                Receive();
            }
            catch (Exception ex)
            {
                Close("Socket Connect Exception");
                OnErrorReceived("Socket Connect", ex);
            }
        }

        #endregion
    }

    #endregion
}