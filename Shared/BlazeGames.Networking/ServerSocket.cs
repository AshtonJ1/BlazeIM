using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Sockets;
using System.Net;
using System.Threading;
using MySql.Data.MySqlClient;

namespace BlazeGames.Networking
{
    delegate void SocketConnectionConnected_Handler(object sender, SocketConnection socketConnection);
    delegate void SocketConnectionDisconnected_Handler(object sender, SocketConnection socketConnection);
    delegate void SocketConnection_PacketReceived_Handler(SocketConnection socketConnection, Packet pak);

    class ServerSocket : IDisposable
    {
        public event SocketConnectionConnected_Handler SocketConnectionConnected_Event;
        public event SocketConnectionDisconnected_Handler SocketConnectionDisconnected_Event;
        public event SocketConnection_PacketReceived_Handler SocketConnection_PacketReceived_Event;

        public List<SocketConnection> SocketConnections = new List<SocketConnection>();
        public Dictionary<int, SocketConnection> MemberConnections = new Dictionary<int, SocketConnection>();
        public System.Net.Sockets.Socket RawSocket;

        public long
            PacketsReceived = 0,
            PacketsSent = 0;

        public static ServerSocket Instance;

        public ServerSocket(IPEndPoint EP)
        {
            RawSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            RawSocket.Bind(EP);

            Instance = this;
        }

        public ServerSocket(IPAddress IP, int Port)
        {
            RawSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            RawSocket.Bind(new IPEndPoint(IP, Port));

            Instance = this;
        }

        public void Start()
        {
            RawSocket.Listen(20);
            RawSocket.BeginAccept(new AsyncCallback(AcceptConnection), RawSocket);
        }

        public void Shutdown()
        {
            RawSocket.Close();
        }

        public void SocketReceivedPacket(SocketConnection conn, Packet pak)
        {
            PacketsReceived++;
            Console.Title = string.Format("{0} Active Connection - {1} Packets Received - {2} Packets Sent", SocketConnections.Count, PacketsReceived, PacketsSent);

            SocketConnection_PacketReceived_Event(conn, pak);
        }

        public void SocketSentPacket()
        {
            PacketsSent++;
            Console.Title = string.Format("{0} Active Connection - {1} Packets Received - {2} Packets Sent", SocketConnections.Count, PacketsReceived, PacketsSent);
        }

        public void SocketConnectionDisconnected(SocketConnection socketConnection)
        {
            try
            {
                socketConnection.SqlConnection.Close();
            }
            catch { }

            SocketConnections.Remove(socketConnection);
            socketConnection.Dispose();

            SocketConnectionDisconnected_Event(this, socketConnection);

            Console.Title = string.Format("{0} Active Connection - {1} Packets Received - {2} Packets Sent", SocketConnections.Count, PacketsReceived, PacketsSent);
        }

        private void AcceptConnection(IAsyncResult ar)
        {
            try
            {
                Socket ClientSocket = RawSocket.EndAccept(ar);
                RawSocket.BeginAccept(new AsyncCallback(AcceptConnection), RawSocket);

                SocketConnection socketConnection = new SocketConnection(this, ClientSocket);
                SocketConnections.Add(socketConnection);
                SocketConnectionConnected_Event(this, socketConnection);

                Console.Title = string.Format("{0} Active Connection - {1} Packets Received - {2} Packets Sent", SocketConnections.Count, PacketsReceived, PacketsSent);
            }
            catch { }
        }

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

    class SocketConnection : IDisposable
    {
        public EndPoint IP;
        public Socket clientSocket;
        public ServerSocket serverSocket;
        public MySqlConnection SqlConnection;
        public bool SqlConnectionInUse = false;

        public Dictionary<string, object> ConnectionData = new Dictionary<string, object>();

        private byte[] ClientReceiveBuffer = new byte[Packet.MaxLength];

        public bool Connected { get { return clientSocket.Connected; } }
        

        public SocketConnection(ServerSocket serverSocket, Socket clientSocket)
        {
            this.clientSocket = clientSocket;
            this.serverSocket = serverSocket;
            this.IP = clientSocket.RemoteEndPoint;
            this.SqlConnection = new MySqlConnection("Server=blaze-games.com;Uid=root;Pwd=hl1vlAbR9a3Riu;database=blazegameshome5;");
            this.SqlConnection.Open();

            this.clientSocket.BeginReceive(ClientReceiveBuffer, 0, Packet.MaxLength, SocketFlags.None, new AsyncCallback(ReceivePacket), clientSocket);
        }

        public void SendPacket(Packet pak)
        {
            clientSocket.BeginSend(pak.ToArray(), 0, pak.Length, SocketFlags.None, new AsyncCallback(SendPacketCallback), clientSocket);
        }

        public void SendBuffer(byte[] Buffer)
        {
            clientSocket.BeginSend(Buffer, 0, Buffer.Length, SocketFlags.None, new AsyncCallback(SendPacketCallback), clientSocket);
        }

        private void SendPacketCallback(IAsyncResult ar)
        {
            try
            {
                clientSocket.EndSend(ar);
                serverSocket.SocketSentPacket();
            }
            catch { serverSocket.SocketConnectionDisconnected(this); }
        }

        private void ReceivePacket(IAsyncResult ar)
        {
            try
            {
                int ReceiveSize = clientSocket.EndReceive(ar);
				
                if (ReceiveSize > 0)
                {
                    byte[] TMPClientReceiveBuffer = new byte[Packet.MaxLength];
                    Array.Copy(ClientReceiveBuffer, TMPClientReceiveBuffer, Packet.MaxLength);

                    this.clientSocket.BeginReceive(ClientReceiveBuffer, 0, Packet.MaxLength, SocketFlags.None, ReceivePacket, null);

                    Packet[] ReceivePakets = Packet.SplitPackets(TMPClientReceiveBuffer);
                    foreach (Packet ReceivePak in ReceivePakets)
                        serverSocket.SocketReceivedPacket(this, ReceivePak);
                }
                else { serverSocket.SocketConnectionDisconnected(this); }
            }
            catch { serverSocket.SocketConnectionDisconnected(this); }
        }

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
                clientSocket.Dispose();
                SqlConnection.Dispose();
            }
        }
    }
}
