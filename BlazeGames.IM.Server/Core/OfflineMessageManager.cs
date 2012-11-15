using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Xml;

namespace BlazeGames.IM.Server.Core
{


    class OfflineMessageManager
    {
        #region Singleton
        private static OfflineMessageManager _Instance = null;

        public static OfflineMessageManager Instance
        {
            get
            {
                if (_Instance == null)
                    _Instance = new OfflineMessageManager();

                return _Instance;
            }
        }

        private OfflineMessageManager()
        {

        }
        #endregion

        List<OfflineMessage> OfflineMessages = new List<OfflineMessage>();

        public OfflineMessage[] GetMessages(int MemberID)
        {
            List<OfflineMessage> Messages = new List<OfflineMessage>();

            foreach (OfflineMessage msg in OfflineMessages.ToArray())
                if (msg.To == MemberID)
                {
                    Messages.Add(msg);
                    OfflineMessages.Remove(msg);
                }

            return Messages.ToArray();
        }

        public void NewOfflineMessage(int From, int To, string Message)
        {
            OfflineMessages.Add(new OfflineMessage(From, To, Message, DateTime.Now));
        }
    }

    class OfflineMessage
    {
        public int From,
            To;

        public string Message;

        public DateTime Timestamp;

        public OfflineMessage(int From, int To, string Message, DateTime Timestamp)
        {
            this.From = From;
            this.To = To;
            this.Message = Message;
            this.Timestamp = Timestamp;
        }
    }
}
