using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Resources;
using System.IO;
using System.Reflection;
using System.ComponentModel;
using System.Xml;
using System.Xml.Linq;
using BlazeGames.Networking;
using System.Net;
using BlazeGames.IM.Client.Networking;
using System.Media;
using System.Threading;
using System.Diagnostics;

namespace BlazeGames.IM.Client
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    internal partial class App : Application
    {
        public static App Instance { get; private set; }

        public Dictionary<int, Contact> Contacts = new Dictionary<int, Contact>();
        public List<Contact> OpenChats = new List<Contact>();
        public bool LoginComplete = false;
        public ClientSocket CSocket;
        int ApplicationStartTick;

        public string MD5Hash = "";

        public static string Account, Password, NickName;
        private static Status _CurrentStatus;
        public static Status CurrentStatus
        {
            get { return _CurrentStatus; }
            set
            {
                _CurrentStatus = value;
                App.Instance.CSocket.SendPacket(Packet.New(Packets.PAK_CLI_CHNGSTSRQST, (byte)value));
                BlazeGames.IM.Client.MainWindow.Instance.profile_image.BorderBrush = (System.Windows.Media.SolidColorBrush)new System.Windows.Media.BrushConverter().ConvertFromString(value.GetColor());
            }
        }

        public App()
        {
            ApplicationStartTick = Environment.TickCount & Int32.MaxValue;

            AppDomain.CurrentDomain.AssemblyResolve += CurrentDomain_AssemblyResolve;
            Instance = this;
            Startup += App_Startup;

            DirectoryInfo chatlogs_di = new DirectoryInfo(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "BlazeGamesIM", "ChatLogs"));
            if (!chatlogs_di.Exists)
                chatlogs_di.Create();
            //Plugins.PluginsManager.Instance.LoadPluginsFromFolder();

            try
            {
                FileInfo fi = new FileInfo(Process.GetCurrentProcess().MainModule.FileName);
                if (!fi.Name.Contains("vshost"))
                {
                    byte[] file_data = File.ReadAllBytes(fi.FullName);
                    MD5Hash = BlazeGames.IM.Client.Core.Utilities.MD5(file_data);
                }
            }
            catch { }
        }

        void UpdateCheckTimer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            UpdateCheck();
        }

        void UpdateCheck()
        {
            WebClient hashfetch_client = new WebClient();
            hashfetch_client.DownloadStringCompleted += new DownloadStringCompletedEventHandler(hashfetch_client_DownloadStringCompleted);
            hashfetch_client.DownloadStringAsync(new Uri("http://blaze-games.com/im/hash/?os=winnt"));
        }

        void hashfetch_client_DownloadStringCompleted(object sender, DownloadStringCompletedEventArgs e)
        {
#if !DEBUG
            try
            {
                if (MD5Hash != "" && e.Result != null && e.Result != MD5Hash)
                {
                    MessageBox.Show("This client build has expired, please update.");
                    BlazeGames.IM.Client.MainWindow.Instance.Close();
                }
            }
            catch { }
#endif
        }

        static Assembly toolkit = Assembly.Load(BlazeGames.IM.Client.Properties.Resources.WPFToolkit_Extended);
        static Assembly mahapps = Assembly.Load(BlazeGames.IM.Client.Properties.Resources.MahApps_Metro);
        static Assembly interactivity = Assembly.Load(BlazeGames.IM.Client.Properties.Resources.System_Windows_Interactivity);

        static Assembly CurrentDomain_AssemblyResolve(object sender, ResolveEventArgs args)
        {
            if (args.Name.Split(',')[0].ToLower().EndsWith(".resources"))
                return null;

            switch(args.Name.Split(',')[0].ToLower())
            {
                case "mahapps.metro": return mahapps;
                case "wpftoolkit.extended": return toolkit;
                case "system.windows.interactivity": return interactivity;
                default:
                    Console.WriteLine("Failed To Load: {0}", args.Name);
                    return null;
            }
        }

        void HandleLoginResponse(ClientSocket clientSocket, Packet pak)
        {
            bool LoginValid = pak.Readbool();

            if (LoginValid)
            {
                string nickname = pak.Readstring();
                string StatusUpdate = pak.Readstring();
                clientSocket.SendPacket(Packet.New(Packets.PAK_CLI_FRNDLSTRQST));
                this.Dispatcher.Invoke((MethodInvoker)delegate
                {
                    Account = BlazeGames.IM.Client.MainWindow.Instance.page_Login.txt_account.Text;
                    Password = BlazeGames.IM.Client.MainWindow.Instance.page_Login.txt_password.Password;
                    NickName = nickname;

                    BlazeGames.IM.Client.MainWindow.Instance.page_Login.txt_loading.Content = "Fetching Your Contacts";
                    BlazeGames.IM.Client.MainWindow.Instance.txt_nickname.Text = nickname;
                    BlazeGames.IM.Client.MainWindow.Instance.txt_status.Text = StatusUpdate;
                    CurrentStatus = Status.Online;

                    BlazeGames.IM.Client.MainWindow.Instance.profile_image_source.ImageSource = new System.Windows.Media.Imaging.BitmapImage(new Uri("https://blaze-games.com/api/image/nocompress/?nickname=" + nickname), new System.Net.Cache.RequestCachePolicy(System.Net.Cache.RequestCacheLevel.NoCacheNoStore));
                }, null);
            }
            else
            {
                this.Dispatcher.Invoke((MethodInvoker)delegate
                {
                    byte ResponseByte = pak.Readbyte();

                    if (ResponseByte == 0x01)
                        NotificationWindow.ShowNotification("Unable to login", "Blaze Games IM was unable to verify this account, please check your username and password and try again.");
                    else if(ResponseByte == 0x02)
                        NotificationWindow.ShowNotification("Unable to login", "This account is already connected, please sign out and try again.");
                    else
                        MessageBox.Show("o.O", "Login Failed");

                
                    BlazeGames.IM.Client.MainWindow.Instance.page_Login.btn_login.Visibility = Visibility.Visible;
                    BlazeGames.IM.Client.MainWindow.Instance.page_Login.Loading.Visibility = Visibility.Hidden;
                    BlazeGames.IM.Client.MainWindow.Instance.page_Login.txt_loading.Visibility = Visibility.Hidden;
                    BlazeGames.IM.Client.MainWindow.Instance.page_Login.txt_account.Visibility = Visibility.Visible;
                    BlazeGames.IM.Client.MainWindow.Instance.page_Login.txt_password.Visibility = Visibility.Visible;
                }, null);
            }
        }

        int FriendCount = 0;

        void HandleFriendListResponse(ClientSocket clientSocket, Packet pak)
        {
            int FriendsCount = pak.Readint();
            FriendCount = FriendsCount;

            for (int i = 0; i < FriendsCount; i++)
            {
                    int FriendID = pak.Readint();
                    try
                    {
                        clientSocket.SendPacket(Packet.New(Packets.PAK_CLI_MEMINFORQST, FriendID));
                    }
                    catch(Exception ex) { Console.WriteLine(ex.ToString()); }
                    
                    
            }

            this.Dispatcher.Invoke((MethodInvoker)delegate
            {
                LoginComplete = true;
                BlazeGames.IM.Client.MainWindow.Instance.txt_nickname.Visibility = Visibility.Visible;
                BlazeGames.IM.Client.MainWindow.Instance.txt_status.Visibility = Visibility.Visible;
                BlazeGames.IM.Client.MainWindow.Instance.txt_search.Visibility = Visibility.Visible;
                BlazeGames.IM.Client.MainWindow.Instance.profile_image.Visibility = Visibility.Visible;
                BlazeGames.IM.Client.MainWindow.Instance.nav_bar.Visibility = Visibility.Visible;
                BlazeGames.IM.Client.MainWindow.Instance.listbox1.Visibility = Visibility.Visible;

                BlazeGames.IM.Client.MainWindow.Instance.btn_ProfileSettings.Visibility = Visibility.Visible;
                BlazeGames.IM.Client.MainWindow.Instance.btn_AddFriend.Visibility = Visibility.Visible;

                BlazeGames.IM.Client.MainWindow.Instance.btn_chat.Visibility = Visibility.Visible;
                BlazeGames.IM.Client.MainWindow.Instance.btn_close.Visibility = Visibility.Visible;
                BlazeGames.IM.Client.MainWindow.Instance.btn_contacts.Visibility = Visibility.Visible;
                BlazeGames.IM.Client.MainWindow.Instance.btn_home.Visibility = Visibility.Visible;
                BlazeGames.IM.Client.MainWindow.Instance.btn_minimize.Visibility = Visibility.Visible;
                BlazeGames.IM.Client.MainWindow.Instance.btn_settings.Visibility = Visibility.Visible;
                SlideFade.StartAnimationIn(BlazeGames.IM.Client.MainWindow.Instance.wnd);

                BlazeGames.IM.Client.MainWindow.Instance.page_Login.Visibility = Visibility.Hidden;
                BlazeGames.IM.Client.MainWindow.Instance.page_Home.Visibility = System.Windows.Visibility.Visible;
                SlideFade.StartAnimationIn(BlazeGames.IM.Client.MainWindow.Instance.page_Home);

                BlazeGames.IM.Client.MainWindow.Instance.page_Login.btn_login.Visibility = Visibility.Visible;
                BlazeGames.IM.Client.MainWindow.Instance.page_Login.Loading.Visibility = Visibility.Hidden;

                BlazeGames.IM.Client.MainWindow.Instance.page_Login.txt_loading.Visibility = Visibility.Hidden;
                BlazeGames.IM.Client.MainWindow.Instance.page_Login.txt_account.Visibility = Visibility.Visible;
                BlazeGames.IM.Client.MainWindow.Instance.page_Login.txt_password.Visibility = Visibility.Visible;
                BlazeGames.IM.Client.MainWindow.Instance.page_Login.txt_loading.Content = "Loading Your Account";
            }, null);
        }

        int ReceivedFriendCount = 0;

        void HandleMemberInfoResponse(ClientSocket clientSocket, Packet pak)
        {
            this.Dispatcher.Invoke((MethodInvoker)delegate
            {
                ReceivedFriendCount++;

                if (pak.Readbool())
                {
                    int MemberID = pak.Readint();
                    string MemberNickname = pak.Readstring();
                    string MemberStatus = pak.Readstring();
                    int MemberAuth = pak.Readint();
                    byte StatusCode = pak.Readbyte();
                    bool PendingRequest = pak.Readbool();

                    if (!Contacts.ContainsKey(MemberID))
                        Contacts.Add(MemberID, new Contact(MemberID, MemberNickname, PendingRequest, (Status)StatusCode, MemberStatus, MemberAuth));
                    else
                    {
                        Contact contact = Contacts[MemberID];

                        contact.NickName = MemberNickname;
                        contact.StatusUpdate = MemberStatus;
                        contact.Authority = MemberAuth;
                        contact.status = (Status)StatusCode;
                        contact.Pending = PendingRequest;

                        
                    }

                    if (PendingRequest && ConfigManager.Instance.GetBool("txt_newrequestnotification", true) && ConfigManager.Instance.GetBool("txt_notifications", true))
                    {
                        NotificationWindow.ShowNotification("Pending Contact", String.Format("{0} has requested you be added to their contact list.", MemberNickname));
                    }

                    if (ReceivedFriendCount >= FriendCount)
                        BlazeGames.IM.Client.MainWindow.Instance.page_Contacts.Draw();
                }

                if (ReceivedFriendCount == FriendCount)
                    clientSocket.SendPacket(Packet.New(Packets.PAK_CLI_OFFLNMSGRQST));
            }, null);
        }

        void HandleMessageDeliver(ClientSocket clientSocket, Packet pak)
        {
            this.Dispatcher.Invoke((MethodInvoker)delegate
            {
                int FromMemberID = pak.Readint();
                string Message = pak.Readstring();

                if(BlazeGames.IM.Client.MainWindow.Instance.page_Chat.ChattingWith != null)
                    if (BlazeGames.IM.Client.MainWindow.Instance.page_Chat.ChattingWith.ID == FromMemberID)
                        BlazeGames.IM.Client.MainWindow.Instance.page_Chat.HandleMessage(Contacts[FromMemberID].NickName, Message);

                Contacts[FromMemberID].ReceiveNewMessage(Message);
            }, null);
        }

        void HandleStatusChangeDeliver(ClientSocket clientSocket, Packet pak)
        {
            this.Dispatcher.Invoke((MethodInvoker)delegate
            {
                int MemberID = pak.Readint();
                Status NewStatus = (Status)pak.Readbyte();
                Contact contact = Contacts[MemberID];

                if (contact.status == Status.Offline && NewStatus != Status.Offline && ConfigManager.Instance.GetBool("txt_loginnotification", true) && ConfigManager.Instance.GetBool("txt_notifications", true))
                    NotificationWindow.ShowNotification(String.Format("{0} Has Signed In", contact.NickName), String.Format("{0} has just signed in, click here to chat.", contact.NickName), contact);
                if (contact.status != Status.Offline && NewStatus == Status.Offline && ConfigManager.Instance.GetBool("txt_logoutnotification", true) && ConfigManager.Instance.GetBool("txt_notifications", true))
                    NotificationWindow.ShowNotification(String.Format("{0} Has Signed Out", contact.NickName), String.Format("{0} has just signed out.", contact.NickName), contact);

                contact.status = NewStatus;
                BlazeGames.IM.Client.MainWindow.Instance.page_Contacts.Draw();
            }, null);
        }

        void HandleUpdateChangeDeliver(ClientSocket clientSocket, Packet pak)
        {
            this.Dispatcher.Invoke((MethodInvoker)delegate
            {
                int MemberID = pak.Readint();
                string NewStatusUpdate = pak.Readstring();

                Contacts[MemberID].control.txt_Status.Text = NewStatusUpdate;
                Contacts[MemberID].StatusUpdate = NewStatusUpdate;
            }, null);
        }

        void HandleFriendRemoveDeliver(ClientSocket clientSocket, Packet pak)
        {
            this.Dispatcher.Invoke((MethodInvoker)delegate
            {
                int MemberID = pak.Readint();

                if (Contacts.ContainsKey(MemberID))
                    Contacts.Remove(MemberID);

                BlazeGames.IM.Client.MainWindow.Instance.page_Contacts.Draw();
            }, null);
        }

        void App_Startup(object sender, StartupEventArgs e)
        {
            Application.Current.DispatcherUnhandledException += new System.Windows.Threading.DispatcherUnhandledExceptionEventHandler(Current_DispatcherUnhandledException);

            int StartupTick = Environment.TickCount & Int32.MaxValue;

            CSocket = new ClientSocket(IPAddress.Parse("209.141.53.112"), 25050);
            CSocket.ClientSocketPacketReceived_Event += new ClientSocketPacketReceived_Handler(CSocket_ClientSocketPacketReceived_Event);
            CSocket.ClientSocketConnected_Event += new EventHandler(CSocket_ClientSocketConnected_Event);
            CSocket.ClientSocketDisconnected_Event += new EventHandler(CSocket_ClientSocketDisconnected_Event);
            CSocket.Connect();

            int InitalizeTick = Environment.TickCount & Int32.MaxValue;

            BlazeGames.IM.Client.MainWindow.Instance.Show();

            int ShowTick = Environment.TickCount & Int32.MaxValue;

            int InitalizeTime = InitalizeTick - ApplicationStartTick;
            int ShowTime = (ShowTick - ApplicationStartTick) - InitalizeTime;
            int StartTime = ShowTick - ApplicationStartTick;
            int NetworkTime = GetPingMS("blaze-games.com");
            DateTime BuildTime = RetrieveLinkerTimestamp();

            if (ConfigManager.Instance.GetBool("indev", false))
            {
                BlazeGames.IM.Client.MainWindow.Instance.txt_debug.Visibility = Visibility.Visible;
                BlazeGames.IM.Client.MainWindow.Instance.txt_debug.Text = String.Format("Ver: PUBLIC_PREVIEW_{4}_{5}    Startup Time: {0}ms    Server Ping Time: {3}ms", StartTime, InitalizeTime, ShowTime, NetworkTime, BuildTime.ToShortDateString(), BuildTime.ToShortTimeString());
                Plugins.PluginsManager.Instance.LoadPluginsFromFolder();
            }

            UpdateCheck();

            System.Timers.Timer UpdateCheckTimer = new System.Timers.Timer(5 * 60 * 1000);
            UpdateCheckTimer.Elapsed += new System.Timers.ElapsedEventHandler(UpdateCheckTimer_Elapsed);
            UpdateCheckTimer.Start();
        }

        void Current_DispatcherUnhandledException(object sender, System.Windows.Threading.DispatcherUnhandledExceptionEventArgs e)
        {
            this.Dispatcher.Invoke((MethodInvoker)delegate
            {
                MessageBoxResult result = MessageBox.Show(string.Format("Blaze IM has encountered an exception but was able to recover. Would you like to report this issue to us so we can fix it?\r\n\r\nError Details:\r\n{0}", e.Exception.Message), "Blaze IM Error", MessageBoxButton.YesNo, MessageBoxImage.Error);

                if (result == MessageBoxResult.Yes)
                {
                    BlazeGames.IM.Client.Core.Utilities.SubmitBug(e.Exception);
                }
            }, null);
        }

        void CSocket_ClientSocketDisconnected_Event(object sender, EventArgs e)
        {
            App.Instance.Dispatcher.Invoke((MethodInvoker)delegate
            {
                BlazeGames.IM.Client.MainWindow.Instance.txt_nickname.Visibility = Visibility.Hidden;
                BlazeGames.IM.Client.MainWindow.Instance.txt_status.Visibility = Visibility.Hidden;
                BlazeGames.IM.Client.MainWindow.Instance.profile_image.Visibility = Visibility.Hidden;
                BlazeGames.IM.Client.MainWindow.Instance.nav_bar.Visibility = Visibility.Hidden;
                BlazeGames.IM.Client.MainWindow.Instance.listbox1.Visibility = Visibility.Hidden;

                BlazeGames.IM.Client.MainWindow.Instance.btn_ProfileSettings.Visibility = Visibility.Hidden;
                BlazeGames.IM.Client.MainWindow.Instance.btn_AddFriend.Visibility = Visibility.Hidden;
                BlazeGames.IM.Client.MainWindow.Instance.AddFriendWindow.Visibility = Visibility.Collapsed;

                BlazeGames.IM.Client.MainWindow.Instance.btn_chat.Visibility = Visibility.Hidden;
                BlazeGames.IM.Client.MainWindow.Instance.btn_close.Visibility = Visibility.Hidden;
                BlazeGames.IM.Client.MainWindow.Instance.btn_contacts.Visibility = Visibility.Hidden;
                BlazeGames.IM.Client.MainWindow.Instance.btn_home.Visibility = Visibility.Hidden;
                BlazeGames.IM.Client.MainWindow.Instance.btn_minimize.Visibility = Visibility.Hidden;
                BlazeGames.IM.Client.MainWindow.Instance.btn_settings.Visibility = Visibility.Hidden;
                BlazeGames.IM.Client.MainWindow.Instance.txt_search.Visibility = Visibility.Hidden;

                BlazeGames.IM.Client.MainWindow.Instance.page_Login.txt_account.Text = "";
                BlazeGames.IM.Client.MainWindow.Instance.page_Login.txt_password.Password = "";
                App.Instance.OpenChats.Clear();
                BlazeGames.IM.Client.MainWindow.Instance.view.Refresh();

                App.Account = "";
                App.Password = "";
                App.NickName = "";
                App.Instance.Contacts.Clear();

                BlazeGames.IM.Client.MainWindow.Instance.page_Login.Visibility = Visibility.Visible;
                BlazeGames.IM.Client.MainWindow.Instance.page_Home.Visibility = System.Windows.Visibility.Hidden;
                BlazeGames.IM.Client.MainWindow.Instance.page_Chat.Visibility = System.Windows.Visibility.Hidden;
                BlazeGames.IM.Client.MainWindow.Instance.page_Contacts.Visibility = System.Windows.Visibility.Hidden;
                BlazeGames.IM.Client.MainWindow.Instance.page_Settings.Visibility = System.Windows.Visibility.Hidden;
                BlazeGames.IM.Client.MainWindow.Instance.CurrentPage = "login";
                SlideFade.StartAnimationIn(BlazeGames.IM.Client.MainWindow.Instance.page_Login);
            }, null);
            
        }

        void CSocket_ClientSocketConnected_Event(object sender, EventArgs e)
        {
            
        }

        public void CSocket_ClientSocketPacketReceived_Event(object sender, ClientSocket clientSocket, Packet pak)
        {
            if (pak.IsValid())
            {
                uint PacketHeader = pak.Readuint();
                switch (PacketHeader)
                {
                    case Packets.PAK_SRV_LGNRESP: this.HandleLoginResponse(clientSocket, pak); break;
                    case Packets.PAK_SRV_FRNDLSTRESP: this.HandleFriendListResponse(clientSocket, pak); break;
                    case Packets.PAK_SRV_MEMINFORESP: this.HandleMemberInfoResponse(clientSocket, pak); break;
                    case Packets.PAK_SRV_MSGDLVR: this.HandleMessageDeliver(clientSocket, pak); break;
                    case Packets.PAK_SRV_NEWSTSDLVR: this.HandleStatusChangeDeliver(clientSocket, pak); break;
                    case Packets.PAK_SRV_NEWUPDTDLVR: this.HandleUpdateChangeDeliver(clientSocket, pak); break;
                    case Packets.PAK_SRV_FRNDRMVDLVR: this.HandleFriendRemoveDeliver(clientSocket, pak); break;
                    default: break;
                }
            }
        }

        int GetPingMS(string hostNameOrAddress)
        {
            System.Net.NetworkInformation.Ping ping = new System.Net.NetworkInformation.Ping();
            return Convert.ToInt32(ping.Send(hostNameOrAddress).RoundtripTime);
        }

        private DateTime RetrieveLinkerTimestamp()
        {
            string filePath = System.Reflection.Assembly.GetCallingAssembly().Location;
            const int c_PeHeaderOffset = 60;
            const int c_LinkerTimestampOffset = 8;
            byte[] b = new byte[2048];
            System.IO.Stream s = null;

            try
            {
                s = new System.IO.FileStream(filePath, System.IO.FileMode.Open, System.IO.FileAccess.Read);
                s.Read(b, 0, 2048);
            }
            finally
            {
                if (s != null)
                {
                    s.Close();
                }
            }

            int i = System.BitConverter.ToInt32(b, c_PeHeaderOffset);
            int secondsSince1970 = System.BitConverter.ToInt32(b, i + c_LinkerTimestampOffset);
            DateTime dt = new DateTime(1970, 1, 1, 0, 0, 0);
            dt = dt.AddSeconds(secondsSince1970);
            dt = dt.AddHours(TimeZone.CurrentTimeZone.GetUtcOffset(dt).Hours);
            return dt;
        }

        public delegate void MethodInvoker();
    }

    internal enum Status
    {
        Offline = 0x00,
        Online = 0x01,
        Away = 0x03,
        Busy = 0x04,
        Afk = 0x05
    }

    internal static class Status_Ext
    {
        public static byte toByte(this Status status)
        {
            return (byte)status;
        }

        public static string GetColor(this Status status)
        {
            switch (status)
            {
                case Status.Online: return "#FF00AE20";
                case Status.Offline: return "#FF686868";
                case Status.Away: return "#FFFFC907";
                case Status.Afk: return "#FFFFC907";
                case Status.Busy: return "#FFFF1511";
                default: return "#FF686868";
            }
        }
    }

    /// <summary>
    /// Contact class used for the contacts page, the recent chats list, and contact chat
    /// </summary>
    internal class Contact : INotifyPropertyChanged
    {
        /// <summary>
        /// PropertyChanged event for the listbox updates
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string address)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null)
            {
                handler(this, new PropertyChangedEventArgs(address));
            }
        }

        private XmlDocument doc = new XmlDocument();

        /// <summary>
        /// Constructs a new Contact
        /// </summary>
        /// <param name="NickName">The contacts nickname</param>
        /// <param name="status">The contacts current status</param>
        /// <param name="StatusUpdate">The contacts status message</param>
        /// <param name="NewMessages">The new message count for the contact</param>
        public Contact(int ID, string NickName, bool Pending = false, Status status = Status.Offline, string StatusUpdate = "", int Authority = 1, int NewMessages = 0)
        {
            this.ID = ID;
            this.Authority = Authority;
            this.NickName = NickName;
            this.ProfileImage = "https://blaze-games.com/api/image/nocompress/?nickname=" + NickName;
            this.status = status;
            this.StatusUpdate = StatusUpdate;
            this.NewMessages = NewMessages;
            this.LastMessage = DateTime.Now;
            this.Pending = Pending;


            if (File.Exists(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "BlazeGamesIM", "ChatLogs", NickName + ".xml")))
                doc.Load(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "BlazeGamesIM", "ChatLogs", NickName + ".xml"));
            else
                doc.LoadXml(@"<?xml version=""1.0""?>
                    <Messages></Messages>");

                XmlNodeList xmlnodeList = doc.GetElementsByTagName("Message");
                int x = xmlnodeList.Count;

                foreach (XmlNode xmlNode in xmlnodeList)
                {
                    XmlElement element = (XmlElement)xmlNode;
                    
                    string From = element.GetAttribute("From");
                    string To = element.GetAttribute("To");
                    bool Read = Convert.ToBoolean(element.GetAttribute("Read"));
                    DateTime Timestamp = Convert.ToDateTime(element.GetAttribute("Timestamp"));
                    if (From == App.NickName || To == App.NickName)
                    {
                        if (!Read)
                        {
                            if (!App.Instance.OpenChats.Contains(this))
                                App.Instance.OpenChats.Add(this);

                            this.NewMessages++;
                            this.LastMessage = Timestamp;
                        }

                        Messages.Add(new Message(From, To, element.InnerText, Read, Timestamp));
                    }
                    
                }
        }

        private string _NickName,
            _StatusUpdate,
            _ProfileImage,
            _BorderColor;

        private int _NewMessages;

        public int Authority,
            ID;

        private bool _Pending;

        private Status _status;

        private DateTime _LastMessage;

        /// <summary>
        /// Gets and sets the contacts nickname
        /// </summary>
        public string NickName { get { return _NickName; } set { _NickName = value; OnPropertyChanged("NickName"); } }
        /// <summary>
        /// Gets and sets the pending state
        /// </summary>
        public bool Pending
        {
            get { return _Pending; }
            set
            {
                _Pending = value; OnPropertyChanged("Pending");
                if (control != null)
                {
                    if (value)
                    {
                        control.btn_acceptfriend.Visibility = System.Windows.Visibility.Visible;
                        control.btn_denyfriend.Visibility = System.Windows.Visibility.Visible;
                        control.btn_removefriend.Visibility = System.Windows.Visibility.Collapsed;
                        control.txt_Status.Text = "Pending Request";
                        control.txt_Status.Foreground = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(125, 125, 125));
                    }
                    else
                    {
                        control.btn_acceptfriend.Visibility = System.Windows.Visibility.Collapsed;
                        control.btn_denyfriend.Visibility = System.Windows.Visibility.Collapsed;
                        control.btn_removefriend.Visibility = System.Windows.Visibility.Visible;
                        control.txt_Status.Text = StatusUpdate;
                        control.txt_Status.Foreground = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(255, 255, 255));
                    }
                }
            }
        }
        /// <summary>
        /// Gets and sets the contacts new message count
        /// </summary>
        public int NewMessages { get { return _NewMessages; } set { _NewMessages = value; OnPropertyChanged("NewMessages"); App.Instance.Dispatcher.Invoke((App.MethodInvoker)delegate { MainWindow.Instance.view.Refresh(); }, null); } }
        /// <summary>
        /// Gets and sets the contacts status message
        /// </summary>
        public string StatusUpdate { get { return _StatusUpdate; } set { _StatusUpdate = value; OnPropertyChanged("StatusUpdate"); } }
        /// <summary>
        /// Gets and sets the contacts profile image url
        /// </summary>
        public string ProfileImage { get { return _ProfileImage; } set { _ProfileImage = value; OnPropertyChanged("ProfileImage"); } }
        /// <summary>
        /// Gets and sets the contacts status
        /// </summary>
        public Status status { get { return _status; } set { _status = value; BorderColor = value.GetColor(); OnPropertyChanged("status"); } }
        /// <summary>
        /// Gets the contacts "status" border color
        /// </summary>
        public string BorderColor
        {
            get { return _BorderColor; }
            private set
            {
                _BorderColor = value;
                OnPropertyChanged("BorderColor");

                if (MainWindow.Instance.page_Chat.ChattingWith != null)
                    if (MainWindow.Instance.page_Chat.ChattingWith.ID == ID)
                        MainWindow.Instance.page_Chat.profile_image.BorderBrush = (System.Windows.Media.SolidColorBrush)new System.Windows.Media.BrushConverter().ConvertFromString(value);
            }
        }
        /// <summary>
        /// Gets and sets the last message date for sorting
        /// </summary>
        public DateTime LastMessage { get { return _LastMessage; } set { _LastMessage = value; OnPropertyChanged("LastMessage"); App.Instance.Dispatcher.Invoke((App.MethodInvoker)delegate { MainWindow.Instance.view.Refresh(); }, null); } }

        public Control_Contact control { get; set; }
        public List<Message> Messages = new List<Message>();

        public void ReceiveNewMessage(string Message)
        {
            XmlNode MembersNode = doc.SelectSingleNode("/Messages");
            XmlElement NewMessageElement = doc.CreateElement("Message");
            NewMessageElement.SetAttribute("Timestamp", DateTime.Now.ToString());
            NewMessageElement.SetAttribute("From", NickName);
            NewMessageElement.SetAttribute("To", App.NickName);
            NewMessageElement.SetAttribute("Read", "false");
            NewMessageElement.InnerText = Message;
            MembersNode.AppendChild(NewMessageElement);
            doc.Save(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "BlazeGamesIM", "ChatLogs", NickName + ".xml"));

            Message msg = new Message(this.NickName, App.NickName, Message);
            Messages.Add(msg);

            if (!App.Instance.OpenChats.Contains(this))
                App.Instance.OpenChats.Add(this);

            if (MainWindow.Instance.WindowState == WindowState.Minimized) { NotifyNewMessage(Message); }
            else if (MainWindow.Instance.Minimum) { NotifyNewMessage(Message); }
            else if (BlazeGames.IM.Client.MainWindow.Instance.CurrentPage != "chat") { NotifyNewMessage(Message); }
            else if (BlazeGames.IM.Client.MainWindow.Instance.page_Chat.ChattingWith == null) { NotifyNewMessage(Message); }
            else if (BlazeGames.IM.Client.MainWindow.Instance.page_Chat.ChattingWith.ID != ID) { NotifyNewMessage(Message); }

            LastMessage = DateTime.Now;
        }

        private void NotifyNewMessage(string Message)
        {
            try
            {
                NewMessages++;
                if (ConfigManager.Instance.GetBool("sound_notifications", true) && ConfigManager.Instance.GetBool("sound_newmessagenotification", true))
                    new SoundPlayer(Properties.Resources.new_message_bells).Play();

                if (ConfigManager.Instance.GetBool("txt_notifications", true) && ConfigManager.Instance.GetBool("txt_newmessagenotification", true))
                {
                    System.Windows.Controls.RichTextBox rtf = new System.Windows.Controls.RichTextBox();
                    rtf.Selection.Load(new MemoryStream(System.Text.Encoding.Default.GetBytes(Message.Replace("xmlns=\"default\"", "xmlns=\"http://schemas.microsoft.com/winfx/2006/xaml/presentation\""))), DataFormats.Xaml);
                    NotificationWindow.ShowNotification(string.Format("New Message From {0}", NickName), rtf.Selection.Text);
                }

                WindowExtensions.FlashWindow(MainWindow.Instance);
            }
            catch { }
        }

        public void SendMessage(string Message)
        {
            NewMessages = 0;
            LastMessage = DateTime.Now;

            XmlNode MembersNode = doc.SelectSingleNode("/Messages");
            XmlElement NewMessageElement = doc.CreateElement("Message");
            NewMessageElement.SetAttribute("Timestamp", DateTime.Now.ToString());
            NewMessageElement.SetAttribute("From", App.NickName);
            NewMessageElement.SetAttribute("To", NickName);
            NewMessageElement.SetAttribute("Read", "true");
            NewMessageElement.InnerText = Message;
            MembersNode.AppendChild(NewMessageElement);
            doc.Save(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "BlazeGamesIM", "ChatLogs", NickName + ".xml"));

            App.Instance.CSocket.SendPacket(BlazeGames.Networking.Packet.New(BlazeGames.IM.Client.Networking.Packets.PAK_CLI_SNDMSG, ID, Message));
            Messages.Add(new Message(App.NickName, NickName, Message, true, DateTime.Now));
        }

        public void MarkAllMessagesRead()
        {
            foreach (Message msg in Messages)
                msg.Read = true;

            XmlNodeList xmlnodeList = doc.GetElementsByTagName("Message");
            foreach (XmlNode xmlNode in xmlnodeList)
            {
                XmlElement element = (XmlElement)xmlNode;
                element.SetAttribute("Read", "true");
            }

            doc.Save(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "BlazeGamesIM", "ChatLogs", NickName + ".xml"));

            NewMessages = 0;
        }
    }

    internal class Message
    {
        public Message(string From, string To, string Message)
        {
            this.From = From;
            this.To = To;
            this.Msg = Message;

            this.Read = false;
            this.SendTime = DateTime.Now;
        }

        public Message(string From, string To, string Message, bool Read, DateTime SendTime)
        {
            this.From = From;
            this.To = To;
            this.Msg = Message;

            this.Read = Read;
            this.SendTime = SendTime;
        }

        public string From,
            To,
            Msg;

        public bool Read;

        public DateTime SendTime;
    }
}
