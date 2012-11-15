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
            PAK_CLI_CHNGNICKRQST    = 0x04,     //  ChangeNicknameRequest(NewNickname)
            PAK_CLI_CHNGSTSRQST     = 0x05,     //  ChangeStatusRequest(NewStatus)
            PAK_CLI_CHNGUPDTRQST    = 0x06,     //  ChangeUpdateRequest(NewUpdate)
            PAK_CLI_CHNGIMGRQST     = 0x07,     //  ChangeProfileImageRequest()
            PAK_CLI_MEMINFORQST     = 0x08,     //  MemberInfoRequest(Nickname/Account/Email/ID)
            PAK_CLI_FRNDADDRQST     = 0x09,     //  FriendAddRequest(ID)
            PAK_CLI_FRNDACCRQST     = 0x10,     //  FriendAcceptRequest(ID)
            PAK_CLI_FRNDDNYRQST     = 0x11,     //  FriendDenyRequest(ID)
            PAK_CLI_FRNDBLKRQST     = 0x12,     //  FriendBlockRequest(ID)
            PAK_CLI_FRNDRMVRQST     = 0x13,     //  FriendRemoveRequest(ID)
            PAK_CLI_OFFLNMSGRQST    = 0x14,     //  OfflineMessageRequest()

            // PAK_SRV
            PAK_SRV_LGNRESP         = 0x51,     //  LoginResponse(ResponseCode, ID, Nickname, Status)
            PAK_SRV_FRNDLSTRESP     = 0x52,     //  FriendListResponse(FriendID's)
            PAK_SRV_MSGSNDRESP      = 0x53,     //  MessageSendResponse(Status)
            PAK_SRV_MEMINFORESP     = 0x54,     //  MemberInfoResponse(MemberData)
            PAK_SRV_MSGDLVR         = 0x55,     //  MessageDeliver(FromID, Message)
            PAK_SRV_FRNDRQSTDLVR    = 0x56,     //  FriendRequestDeliver(FromID)
            PAK_SRV_NEWNICKDLVR     = 0x57,     //  NewNicknameDeliver(ID, NewNickname)
            PAK_SRV_NEWSTSDLVR      = 0x58,     //  NewStatusDeliver(ID, NewStatus)
            PAK_SRV_NEWUPDTDLVR     = 0x59,     //  NewUpdateDeliver(ID, NewUpdate)
            PAK_SRV_FRNDACCDLVR     = 0x60,     //  FriendAcceptDeliver(ID)
            PAK_SRV_FRNDRMVDLVR     = 0x61;     //  FriendRemoveDeliver(ID)
    }
}
