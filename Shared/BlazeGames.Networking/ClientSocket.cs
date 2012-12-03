using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Sockets;
using System.Net;

namespace BlazeGames.Networking
{
    delegate void ClientSocketPacketReceived_Handler(object sender, ClientSocket clientSocket, Packet pak);

    /// <summary>
    /// Client Socket System Version 1.0
    /// Ashton Storks, Blaze Games
    /// Last Updated: 11/10/2012
    /// </summary>
    class ClientSocket : IDisposable
    {
        /// <summary>
        /// The ammount of packets sent to the server
        /// </summary>
        public long SentPackets { get; private set; }
        /// <summary>
        /// The ammount of packets received from the server
        /// </summary>
        public int ReceivedPackets { get; private set; }

        /// <summary>
        /// The event to hook into for the received packets event
        /// </summary>
        public event ClientSocketPacketReceived_Handler ClientSocketPacketReceived_Event;
        /// <summary>
        /// The event to hook into for the connected event
        /// </summary>
        public event EventHandler ClientSocketConnected_Event;
        /// <summary>
        /// The event to hook into for the desconnected event;
        /// </summary>
        public event EventHandler ClientSocketDisconnected_Event;

        /// <summary>
        /// The raw System.Net.Sockets.Socket of the client
        /// </summary>
        public Socket RawSocket;
        private byte[] ReceiveBuffer = new byte[Packet.MaxLength];
        private IPEndPoint EP;

        /// <summary>
        /// Created a new client connection
        /// </summary>
        /// <param name="EP">The IPEndpoint to connect to</param>
        /// <param name="ProxyType">The the SOCKS proxy type to use to connect</param>
        public ClientSocket(IPEndPoint EP)
        {
            RawSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            /*if (ProxyType != ProxyTypes.None)
            {
                RawSocket.ProxyEndPoint = ProxyEndpoint;
                RawSocket.ProxyType = ProxyType;
            }
            else
                RawSocket.ProxyType = ProxyTypes.None;*/
            this.EP = EP;
        }

        /// <summary>
        /// Creates a new client connection
        /// </summary>
        /// <param name="IP">The System.Net.IPAddress to connect to</param>
        /// <param name="Port">The Port to connect to</param>
        public ClientSocket(IPAddress IP, int Port)
        {
            RawSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            /*if (ProxyType != ProxyTypes.None)
            {
                RawSocket.ProxyEndPoint = ProxyEndpoint;
                RawSocket.ProxyType = ProxyType;
            }
            else
                RawSocket.ProxyType = ProxyTypes.None;*/
            this.EP = new IPEndPoint(IP, Port);
        }

        /// <summary>
        /// Connects to the specified server socket
        /// </summary>
        public void Connect()
        {
            try
            {
                #if SILVERLIGHT
                SocketAsyncEventArgs ConnectionEvent = new SocketAsyncEventArgs();
                ConnectionEvent.RemoteEndPoint = EP;
                ConnectionEvent.UserToken = RawSocket;
                ConnectionEvent.Completed += new EventHandler<SocketAsyncEventArgs>(ConnectionEvent_Completed);

                RawSocket.ConnectAsync(ConnectionEvent);
                #else
                    RawSocket.BeginConnect(EP, new AsyncCallback(SocketConnected), RawSocket);
                #endif
            }
            catch { }
        }

        /// <summary>
        /// Disconnects from the server socket
        /// </summary>
        public void Disconnect()
        {
            #if SILVERLIGHT
                RawSocket.Close();
            #else
                RawSocket.BeginDisconnect(false, new AsyncCallback(SocketDisconnected), RawSocket);
            #endif
        }

        public void SendPacket(Packet pak)
        {
            SendBuffer(pak.ToArray());
        }

        public void SendBuffer(byte[] Buffer)
        {
            #if SILVERLIGHT
                SocketAsyncEventArgs SendEvent = new SocketAsyncEventArgs();
                SendEvent.SetBuffer(Buffer, 0, Buffer.Length);

                RawSocket.SendAsync(SendEvent);
            #else
                RawSocket.BeginSend(Buffer, 0, Buffer.Length, SocketFlags.None, new AsyncCallback(PacketSent), RawSocket);
            #endif
        }

#if SILVERLIGHT
        void ConnectionEvent_Completed(object sender, SocketAsyncEventArgs e)
        {
            if (e.SocketError == SocketError.Success)
            {
                try
                {
                    SocketAsyncEventArgs ReceiveEvent = new SocketAsyncEventArgs();
                    ReceiveEvent.Completed += new EventHandler<SocketAsyncEventArgs>(ReceiveEvent_Completed);

                    RawSocket.ReceiveAsync(ReceiveEvent);
                }
                catch { }

                if (ClientSocketConnected_Event != null)
                    ClientSocketConnected_Event(this, null);
            }
            else
                throw new SocketException((int)e.SocketError);
        }

        void ReceiveEvent_Completed(object sender, SocketAsyncEventArgs e)
        {
            try
            {
                int ReceiveSize = e.BytesTransferred;

                if (ReceiveSize > 0)
                {
                    byte[] TMPReceiveBuffer = new byte[Packet.MaxLength];
                    Array.Copy(e.Buffer, TMPReceiveBuffer, Packet.MaxLength);

                    SocketAsyncEventArgs ReceiveEvent = new SocketAsyncEventArgs();
                    ReceiveEvent.Completed += new EventHandler<SocketAsyncEventArgs>(ReceiveEvent_Completed);

                    RawSocket.ReceiveAsync(ReceiveEvent);

                    Packet[] ReceivePakets = Packet.SplitPackets(TMPReceiveBuffer);
                    foreach (Packet ReceivePak in ReceivePakets)
                    {
                        if (ClientSocketPacketReceived_Event != null)
                            ClientSocketPacketReceived_Event(this, this, ReceivePak);

                        ReceivedPackets++;
                    }
                }
                else { if (ClientSocketDisconnected_Event != null) { ClientSocketDisconnected_Event(this, null); } }
            }
            catch { if (ClientSocketDisconnected_Event != null) { ClientSocketDisconnected_Event(this, null); } }
        }
#else
        private void SocketConnected(IAsyncResult ar)
        {
                try
                {
                    RawSocket.EndConnect(ar);
                    RawSocket.BeginReceive(ReceiveBuffer, 0, Packet.MaxLength, SocketFlags.None, new AsyncCallback(PacketReceived), RawSocket);
                }
                catch { }

                if(ClientSocketConnected_Event != null)
                    ClientSocketConnected_Event(this, null);
        }

        private void SocketDisconnected(IAsyncResult ar)
        {
                RawSocket.EndDisconnect(ar);

                if(ClientSocketDisconnected_Event != null)
                    ClientSocketDisconnected_Event(this, null);
        }

        private void PacketSent(IAsyncResult ar)
        {
                RawSocket.EndSend(ar);
                SentPackets++;
        }

        private void PacketReceived(IAsyncResult ar)
        {
                try
                {
                    int ReceiveSize = RawSocket.EndReceive(ar);

                    if (ReceiveSize > 0)
                    {
                        byte[] TMPReceiveBuffer = new byte[Packet.MaxLength];
                        Array.Copy(ReceiveBuffer, TMPReceiveBuffer, Packet.MaxLength);

                        RawSocket.BeginReceive(ReceiveBuffer, 0, Packet.MaxLength, SocketFlags.None, new AsyncCallback(PacketReceived), RawSocket);

                        Packet[] ReceivePakets = Packet.SplitPackets(TMPReceiveBuffer);
                        foreach (Packet ReceivePak in ReceivePakets)
                        {
                            if(ClientSocketPacketReceived_Event != null)
                                ClientSocketPacketReceived_Event(this, this, ReceivePak);

                            ReceivedPackets++;
                        }
                    }
                    else { if (ClientSocketDisconnected_Event != null) { ClientSocketDisconnected_Event(this, null); } }
                }
                catch { if (ClientSocketDisconnected_Event != null) { ClientSocketDisconnected_Event(this, null); } }
        }
#endif
        /* GC */
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                RawSocket.Dispose();
            }
        }
    }
}
