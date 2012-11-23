using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BlazeGames.Networking;
namespace BlazeGames.IM.Client.Networking
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

            //PAK_CLI_GRP_INV         = 0x15,     //  GroupInviteRequest(GroupID, MemID)              *
            //PAK_CLI_GRP_LEAVE       = 0x16,     //  GroupLeave(GroupID)                             *
            //PAK_CLI_GRP_SNDMSG      = 0x17,     //  GroupSendMessage(GroupID, Message)              *

            PAK_CLI_CALL_RQST       = 0x18,     //  CallRequest(MemberID)                           *
            PAK_CLI_CALL_CNCL       = 0x19,     //  CallCancel(MemberID)                            *
            PAK_CLI_CALL_ACC        = 0x20,     //  CallAccept(MemberID)                            *
            PAK_CLI_CALL_DNY        = 0x21,     //  CallDeny(MemberID)                              *
            PAK_CLI_CALL_END        = 0x22,     //  CallEnd(MemberID)                               *

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

            //PAK_SRV_GRP_MSGDLVR     = 0x62,     //  GroupMessageDelver(GroupID, FromID, Message)    *
            //PAK_SRV_GRP_JOINDLVR    = 0x63,     //  GroupJoinDeliver(GroupID, MemID)                *
            //PAK_SRV_GRP_LEAVDLVR    = 0x64,     //  GroupLeaveDeliver(GroupID, MemID)               *

            PAK_SRV_CALL_DLVR       = 0x65,     //  CallDeliver(MemberID, UDPAddress)               *
            PAK_SRV_CALL_CNCL_DLVR  = 0x66,     //  CallCancelDeliver(MemberID)                     *
            PAK_SRV_CALL_ACC_DLVR   = 0x67,     //  CallAcceptDeliver(MemberID, UDPAddress)         *
            PAK_SRV_CALL_DNY_DLVR   = 0x68,     //  CallDenyDeliver(MemberID)                       *
            PAK_SRV_CALL_END_DLVR   = 0x69;     //  CallEndDeliver(MemberID)                        *
    }
}
