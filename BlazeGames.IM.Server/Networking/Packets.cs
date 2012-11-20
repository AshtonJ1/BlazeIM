using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BlazeGames.Networking;
using BlazeGames.IM.Server.Core;
using System.Threading;

namespace BlazeGames.IM.Server.Networking
{
    class Packets
    {
        public const uint
            // PAK_CLI
            PAK_CLI_LGNRQST         = 0x01,     //  LoginRequest(Account, Passwordhash)
            PAK_CLI_FRNDLSTRQST     = 0x02,     //  FriendListRequest()
            PAK_CLI_SNDMSG          = 0x03,     //  MessageSend(Message) 
            PAK_CLI_CHNGSTSRQST     = 0x05,     //  ChangeStatusRequest(NewStatus)     
            PAK_CLI_CHNGUPDTRQST    = 0x06,     //  ChangeUpdateRequest(NewUpdate)
            PAK_CLI_CHNGIMGRQST     = 0x07,     //  ChangeProfileImageRequest()        
            PAK_CLI_MEMINFORQST     = 0x08,     //  MemberInfoRequest(ID)
            PAK_CLI_FRNDADDRQST     = 0x09,     //  FriendAddRequest(ID)
            PAK_CLI_FRNDDNYRQST     = 0x11,     //  FriendDenyRequest(ID)              
            PAK_CLI_FRNDBLKRQST     = 0x12,     //  FriendBlockRequest(ID)             
            PAK_CLI_FRNDRMVRQST     = 0x13,     //  FriendRemoveRequest(ID)            
            PAK_CLI_OFFLNMSGRQST    = 0x14,     //  OfflineMessageRequest()

            PAK_CLI_GRP_INV         = 0x15,     //  GroupInviteRequest(GroupID, MemID)              *
            PAK_CLI_GRP_LEAVE       = 0x16,     //  GroupLeave(GroupID)                             *
            PAK_CLI_GRP_SNDMSG      = 0x17,     //  GroupSendMessage(GroupID, Message)              *

            // PAK_SRV
            PAK_SRV_LGNRESP         = 0x51,     //  LoginResponse(ResponseCode, ID, Nickname, Status)
            PAK_SRV_FRNDLSTRESP     = 0x52,     //  FriendListResponse(FriendID's)
            PAK_SRV_MSGSNDRESP      = 0x53,     //  MessageSendResponse(Status)
            PAK_SRV_MEMINFORESP     = 0x54,     //  MemberInfoResponse(MemberData)
            PAK_SRV_MSGDLVR         = 0x55,     //  MessageDeliver(FromID, Message)
            PAK_SRV_FRNDRQSTDLVR    = 0x56,     //  FriendRequestDeliver(FromID)
            PAK_SRV_NEWSTSDLVR      = 0x58,     //  NewStatusDeliver(ID, NewStatus)
            PAK_SRV_NEWUPDTDLVR     = 0x59,     //  NewUpdateDeliver(ID, NewUpdate)
            PAK_SRV_FRNDRMVDLVR     = 0x61,     //  FriendRemoveDeliver(ID)

            PAK_SRV_GRP_MSGDLVR     = 0x62,     //  GroupMessageDelver(GroupID, FromID, Message)    *
            PAK_SRV_GRP_JOINDLVR    = 0x63,     //  GroupJoinDeliver(GroupID, MemID)                *
            PAK_SRV_GRP_LEAVDLVR    = 0x64;     //  GroupLeaveDeliver(GroupID, MemID)               *
    }

    class PacketHandlers
    {
        public static void HandleLoginRequest(SocketConnection conn, Packet pak)
        {
            string Account = pak.Readstring();
            string PasswordHash = pak.Readstring();

            string Nickname = Member.TryLoginWithPassword(Account, PasswordHash, conn.SqlConnection);

            if (Nickname == null)
                conn.SendPacket(Packet.New(Packets.PAK_SRV_LGNRESP, false, (byte)0x01));
            else
            {
                Member mem = new Member(Account, conn.SqlConnection);

                if (ServerSocket.Instance.MemberConnections.ContainsKey(mem.ID))
                {
                    ServerSocket.Instance.MemberConnections[mem.ID].SendPacket(Packet.New(0x0));
                    conn.SendPacket(Packet.New(Packets.PAK_SRV_LGNRESP, false, (byte)0x02));
                }
                else
                {
                    mem.StatusCode = 0x01;
                    conn.ConnectionData.Add("MemberConnected", true);
                    conn.ConnectionData.Add("Member", mem);

                    foreach (string FriendIDStr in mem.Friends)
                        try
                        {
                            int FriendID = int.Parse(FriendIDStr);
                            if (ServerSocket.Instance.MemberConnections.ContainsKey(FriendID))
                                ServerSocket.Instance.MemberConnections[FriendID].SendPacket(Packet.New(Packets.PAK_SRV_NEWSTSDLVR, mem.ID, mem.StatusCode));
                        }
                        catch { }

                    ServerSocket.Instance.MemberConnections.Add(mem.ID, conn);

                    conn.SendPacket(Packet.New(Packets.PAK_SRV_LGNRESP, true, Nickname, mem.MemberData));
                }
            }

            Console.WriteLine("HandleLoginRequest({0}, {1});", Account, PasswordHash);
        }

        public static void HandleFriendListRequest(SocketConnection conn, Packet pak)
        {
            if (conn.ConnectionData.ContainsKey("Member"))
            {
                Packet pak2 = new Packet(null);
                Member mem = (Member)conn.ConnectionData["Member"];

                pak2.Write(Packets.PAK_SRV_FRNDLSTRESP);
                int FriendCount = 0;
                foreach (string FriendID in mem.Friends)
                    try { Convert.ToInt32(FriendID); FriendCount++; }
                    catch { }
                foreach (string FriendID in mem.PendingFriends)
                    try { Convert.ToInt32(FriendID); FriendCount++; }
                    catch { }

                pak2.Write(FriendCount);

                foreach (string FriendID in mem.Friends)
                    try { pak2.Write(Convert.ToInt32(FriendID)); }
                    catch { }
                foreach (string FriendID in mem.PendingFriends)
                    try { pak2.Write(Convert.ToInt32(FriendID)); }
                    catch { }

                conn.SendPacket(pak2);

                Console.WriteLine("HandleFriendListRequest({0});", mem.Friends.Count);
            }
        }

        public static void HandleMemberInfoRequest(SocketConnection conn, Packet pak)
        {
            int MemberID = 0;

            if (conn.ConnectionData.ContainsKey("Member"))
            {
                try
                {
                    Packet pak2 = new Packet(null);
                    pak2.Write(Packets.PAK_SRV_MEMINFORESP);

                    MemberID = pak.Readint();
                    Member mem = conn.ConnectionData["Member"] as Member;
                    Member member;
                    if (ServerSocket.Instance.MemberConnections.ContainsKey(MemberID))
                        member = ServerSocket.Instance.MemberConnections[MemberID].ConnectionData["Member"] as Member;
                    else
                        member = new Member(MemberID, conn.SqlConnection);
                    if (member.IsValid)
                    {
                        pak2.Write(true);
                        pak2.Write(MemberID);
                        pak2.Write(member.Nickname);
                        pak2.Write(member.MemberData);
                        pak2.Write(member.Authority);
                        pak2.Write(member.StatusCode);
                        pak2.Write(mem.PendingFriends.Contains(MemberID.ToString()));
                    }
                    else
                        pak2.Write(false);

                    conn.SendPacket(pak2);
                }
                catch (Exception ex) { Console.WriteLine(ex.ToString()); }
            }

            Console.WriteLine("HandleMemberInfoRequest({0}); SqlState: {1}", MemberID, conn.SqlConnection.State);
        }

        public static void HandleMessageSend(SocketConnection conn, Packet pak)
        {
            if (conn.ConnectionData.ContainsKey("Member"))
            {
                Member memfrom = (Member)conn.ConnectionData["Member"];
                int MemberSendTo = pak.Readint();
                string Message = pak.Readstring();

                if (memfrom.Friends.Contains(Convert.ToString(MemberSendTo)))
                {
                    if (ServerSocket.Instance.MemberConnections.ContainsKey(MemberSendTo))
                        ServerSocket.Instance.MemberConnections[MemberSendTo].SendPacket(Packet.New(Packets.PAK_SRV_MSGDLVR, memfrom.ID, Message));
                    else
                    {
                        OfflineMessageManager.Instance.NewOfflineMessage(memfrom.ID, MemberSendTo, Message);
                    }
                }
                else
                {
                    //TODO: Notify the member that the friend is not in their list
                }
            }
        }

        public static void HandleStatusChangeRequest(SocketConnection conn, Packet pak)
        {
            if (conn.ConnectionData.ContainsKey("Member"))
            {
                Member mem = (Member)conn.ConnectionData["Member"];
                mem.StatusCode = pak.Readbyte();
                mem.Save();

                foreach (string FriendIDStr in mem.Friends)
                    try
                    {
                        int FriendID = int.Parse(FriendIDStr);
                        if (ServerSocket.Instance.MemberConnections.ContainsKey(FriendID))
                            ServerSocket.Instance.MemberConnections[FriendID].SendPacket(Packet.New(Packets.PAK_SRV_NEWSTSDLVR, mem.ID, mem.StatusCode));
                    }
                    catch { }
            }
        }

        public static void HandleOfflineMessagesRequest(SocketConnection conn, Packet pak)
        {
            if (conn.ConnectionData.ContainsKey("Member"))
            {
                Member mem = (Member)conn.ConnectionData["Member"];
                OfflineMessage[] OfflineMessages = OfflineMessageManager.Instance.GetMessages(mem.ID);

                foreach (OfflineMessage msg in OfflineMessages)
                    conn.SendPacket(Packet.New(Packets.PAK_SRV_MSGDLVR, msg.From, msg.Message));
            }
        }

        public static void HandleFriendAddRequest(SocketConnection conn, Packet pak)
        {
            if (conn.ConnectionData.ContainsKey("Member"))
            {
                Member member1 = (Member)conn.ConnectionData["Member"];
                string FriendSearch = pak.Readstring();

                int MemberID = Member.FindMember(FriendSearch, conn.SqlConnection);
                if (MemberID == -1 || MemberID == member1.ID)
                    return;

                Member member2;
                if (ServerSocket.Instance.MemberConnections.ContainsKey(MemberID))
                    member2 = ServerSocket.Instance.MemberConnections[MemberID].ConnectionData["Member"] as Member;
                else
                    member2 = new Member(MemberID, conn.SqlConnection);

                if (!member2.IsValid || member1.Friends.Contains(member2.ID.ToString()))
                    return;

                if (member1.PendingFriends.Contains(member2.ID.ToString()))
                {
                    member1.PendingFriends.Remove(member2.ID.ToString());

                    member1.Friends.Add(member2.ID.ToString());
                    member2.Friends.Add(member1.ID.ToString());

                    member1.Save();
                    member2.Save();

                    Packet pak2 = new Packet(null);
                    pak2.Write(Packets.PAK_SRV_MEMINFORESP);

                    if (member2.IsValid)
                    {
                        pak2.Write(true);
                        pak2.Write(member2.ID);
                        pak2.Write(member2.Nickname);
                        pak2.Write(member2.MemberData);
                        pak2.Write(member2.Authority);
                        pak2.Write(member2.StatusCode);
                        pak2.Write(false);
                    }
                    else
                        pak2.Write(false);

                    conn.SendPacket(pak2);

                    if (ServerSocket.Instance.MemberConnections.ContainsKey(MemberID))
                    {
                        SocketConnection conn2 = ServerSocket.Instance.MemberConnections[MemberID];

                        Packet pak3 = new Packet(null);
                        pak3.Write(Packets.PAK_SRV_MEMINFORESP);

                        if (member1.IsValid)
                        {
                            pak3.Write(true);
                            pak3.Write(member1.ID);
                            pak3.Write(member1.Nickname);
                            pak3.Write(member1.MemberData);
                            pak3.Write(member1.Authority);
                            pak3.Write(member1.StatusCode);
                            pak3.Write(false);
                        }
                        else
                            pak3.Write(false);

                        conn2.SendPacket(pak3);
                    }
                }
                else if (member2.PendingFriends.Contains(member1.ID.ToString())) { }
                else
                {
                    member2.PendingFriends.Add(member1.ID.ToString());
                    member2.Save();

                    if (ServerSocket.Instance.MemberConnections.ContainsKey(MemberID))
                    {
                        SocketConnection conn2 = ServerSocket.Instance.MemberConnections[MemberID];

                        Packet pak2 = new Packet(null);
                        pak2.Write(Packets.PAK_SRV_MEMINFORESP);

                        if (member1.IsValid)
                        {
                            pak2.Write(true);
                            pak2.Write(member1.ID);
                            pak2.Write(member1.Nickname);
                            pak2.Write(member1.MemberData);
                            pak2.Write(member1.Authority);
                            pak2.Write(member1.StatusCode);
                            pak2.Write(true);
                        }
                        else
                            pak2.Write(false);

                        conn2.SendPacket(pak2);
                    }
                }
            }
        }

        public static void HandleFriendDenyRequest(SocketConnection conn, Packet pak)
        {
            if (conn.ConnectionData.ContainsKey("Member"))
            {
                Member member1 = (Member)conn.ConnectionData["Member"];

                int MemberID = pak.Readint();

                Member member2;
                if (ServerSocket.Instance.MemberConnections.ContainsKey(MemberID))
                    member2 = ServerSocket.Instance.MemberConnections[MemberID].ConnectionData["Member"] as Member;
                else
                    member2 = new Member(MemberID, conn.SqlConnection);

                if (!member2.IsValid || member1.Friends.Contains(member2.ID.ToString()))
                    return;

                if (member1.PendingFriends.Contains(member2.ID.ToString()))
                {
                    member1.PendingFriends.Remove(member2.ID.ToString());
                    member1.Save();
                }
            }
        }

        public static void HandleChangeUpdateRequest(SocketConnection conn, Packet pak)
        {
            if (conn.ConnectionData.ContainsKey("Member"))
            {
                Member mem = (Member)conn.ConnectionData["Member"];
                mem.MemberData = pak.Readstring();
                mem.Save();

                foreach (string FriendIDStr in mem.Friends)
                    try
                    {
                        int FriendID = int.Parse(FriendIDStr);
                        if (ServerSocket.Instance.MemberConnections.ContainsKey(FriendID))
                            ServerSocket.Instance.MemberConnections[FriendID].SendPacket(Packet.New(Packets.PAK_SRV_NEWUPDTDLVR, mem.ID, mem.MemberData));
                    }
                    catch { }
            }
        }

        public static void HandleFriendBlockRequest(SocketConnection conn, Packet pak)
        {
            if (conn.ConnectionData.ContainsKey("Member"))
            {
                Member mem = (Member)conn.ConnectionData["Member"];


            }
        }

        public static void HandleFriendRemoveRequest(SocketConnection conn, Packet pak)
        {
            if (conn.ConnectionData.ContainsKey("Member"))
            {
                Member member1 = (Member)conn.ConnectionData["Member"];
                int MemberID = pak.Readint();

                Member member2;
                if (ServerSocket.Instance.MemberConnections.ContainsKey(MemberID))
                    member2 = ServerSocket.Instance.MemberConnections[MemberID].ConnectionData["Member"] as Member;
                else
                    member2 = new Member(MemberID, conn.SqlConnection);

                if (!member2.IsValid || !member1.Friends.Contains(member2.ID.ToString()))
                    return;

                member1.Friends.Remove(member2.ID.ToString());
                member2.Friends.Remove(member1.ID.ToString());

                member1.Save();
                member2.Save();

                if (ServerSocket.Instance.MemberConnections.ContainsKey(MemberID))
                    ServerSocket.Instance.MemberConnections[MemberID].SendPacket(Packet.New(Packets.PAK_SRV_FRNDRMVDLVR, member1.ID));
                conn.SendPacket(Packet.New(Packets.PAK_SRV_FRNDRMVDLVR, member2.ID));
            }
        }
    }
}
