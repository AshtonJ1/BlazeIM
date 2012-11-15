using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using BlazeGames.Networking;
using BlazeGames.IM.Client.Networking;

namespace BlazeGames.IM.Client
{
    /// <summary>
    /// Interaction logic for Control_AddFriend.xaml
    /// </summary>
    public partial class Control_AddFriend : UserControl
    {
        public Control_AddFriend()
        {
            InitializeComponent();
        }

        private void btn_add_Click(object sender, RoutedEventArgs e)
        {
            string FriendEmail = txt_Friend.Text;

            if (FriendEmail != "")
            {
                App.Instance.CSocket.SendPacket(Packet.New(Packets.PAK_CLI_FRNDADDRQST, FriendEmail));
                NotificationWindow.ShowNotification("Friend Request Sent", string.Format("Your friend request to {0} has been sent", FriendEmail));
            }

            txt_Friend.Text = "";
            this.Visibility = Visibility.Collapsed;

            
        }
    }
}
