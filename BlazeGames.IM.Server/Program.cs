using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using BlazeGames.Networking;
using BlazeGames.IM.Server.Networking;
using MySql.Data.MySqlClient;
using System.Threading;
using BlazeGames.IM.Server.Core;

namespace BlazeGames.IM.Server
{
    class Program
    {
        static void Main(string[] args)
        {
            ServerSocket Socket = new ServerSocket(IPAddress.Any, 25050);
            Socket.SocketConnectionConnected_Event += new SocketConnectionConnected_Handler(Socket_SocketConnectionConnected_Event);
            Socket.SocketConnectionDisconnected_Event += new SocketConnectionDisconnected_Handler(Socket_SocketConnectionDisconnected_Event);
            Socket.SocketConnection_PacketReceived_Event += new SocketConnection_PacketReceived_Handler(Socket_SocketConnection_PacketReceived_Event);
            Socket.Start();

            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("Listening...");
            Console.ReadLine();
        }

        static void Socket_SocketConnection_PacketReceived_Event(SocketConnection socketConnection, Packet pak)
        {
            
            if (pak.IsValid())
            {
                uint Header = pak.Readuint();
                switch (Header)
                {
                    case Packets.PAK_CLI_LGNRQST: PacketHandlers.HandleLoginRequest(socketConnection, pak); break;
                    case Packets.PAK_CLI_FRNDLSTRQST: PacketHandlers.HandleFriendListRequest(socketConnection, pak); break;
                    case Packets.PAK_CLI_MEMINFORQST: PacketHandlers.HandleMemberInfoRequest(socketConnection, pak); break;
                    case Packets.PAK_CLI_SNDMSG: PacketHandlers.HandleMessageSend(socketConnection, pak); break;
                    case Packets.PAK_CLI_CHNGSTSRQST: PacketHandlers.HandleStatusChangeRequest(socketConnection, pak); break;
                    case Packets.PAK_CLI_OFFLNMSGRQST: PacketHandlers.HandleOfflineMessagesRequest(socketConnection, pak); break;
                    case Packets.PAK_CLI_FRNDADDRQST: PacketHandlers.HandleFriendAddRequest(socketConnection, pak); break;
                    case Packets.PAK_CLI_FRNDDNYRQST: PacketHandlers.HandleFriendDenyRequest(socketConnection, pak); break;
                    case Packets.PAK_CLI_CHNGUPDTRQST: PacketHandlers.HandleChangeUpdateRequest(socketConnection, pak); break;
                    case Packets.PAK_CLI_FRNDRMVRQST: PacketHandlers.HandleFriendRemoveRequest(socketConnection, pak); break;
                    default:
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.WriteLine("Invalid packet from {0}, 0x{1:X}", socketConnection.IP, Header);
                        break;
                }
            }
            else
            {
                //Console.ForegroundColor = ConsoleColor.Red;
                //Console.WriteLine("Invalid packet from {0}", socketConnection.IP);
                //socketConnection.clientSocket.Close();
            }
        }

        static void Socket_SocketConnectionDisconnected_Event(object sender, SocketConnection socketConnection)
        {
            Console.ForegroundColor = ConsoleColor.Blue;
            Console.WriteLine("Connection Closed: {0}", socketConnection.IP);

            if (socketConnection.ConnectionData.ContainsKey("Member"))
            {
                Member mem = socketConnection.ConnectionData["Member"] as Member;
                mem.StatusCode = 0x0;

                foreach (string FriendIDStr in mem.Friends)
                    try
                    {
                        int FriendID = int.Parse(FriendIDStr);
                        if (ServerSocket.Instance.MemberConnections.ContainsKey(FriendID))
                            ServerSocket.Instance.MemberConnections[FriendID].SendPacket(Packet.New(Packets.PAK_SRV_NEWSTSDLVR, mem.ID, mem.StatusCode));
                    }
                    catch { }

                ServerSocket.Instance.MemberConnections.Remove(mem.ID);
            }
        }

        static void Socket_SocketConnectionConnected_Event(object sender, SocketConnection socketConnection)
        {
            Console.ForegroundColor = ConsoleColor.Blue;
            Console.WriteLine("New Connection: {0}", socketConnection.IP);
        }
    }
}
