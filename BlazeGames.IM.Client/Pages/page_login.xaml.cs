using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
using System.Net;

namespace BlazeGames.IM.Client
{
    /// <summary>
    /// Interaction logic for page_login.xaml
    /// </summary>
    internal partial class page_login : UserControl
    {
        public page_login()
        {
            InitializeComponent();
        }

        private void btn_login_Click(object sender, RoutedEventArgs e)
        {
            if (!App.Instance.CSocket.RawSocket.Connected)
            {
                App.Instance.CSocket = new ClientSocket(IPAddress.Parse("209.141.53.112"), 25050);
                App.Instance.CSocket.ClientSocketPacketReceived_Event += new ClientSocketPacketReceived_Handler(App.Instance.CSocket_ClientSocketPacketReceived_Event);
                App.Instance.CSocket.Connect();
            }

            App.Instance.CSocket.SendPacket(Packet.New(Packets.PAK_CLI_LGNRQST, txt_account.Text, Utilities.MD5("BGxSecure" + Utilities.MD5(txt_password.Password))));

            Loading.Visibility = Visibility.Visible;
            txt_account.Visibility = Visibility.Hidden;
            txt_password.Visibility = Visibility.Hidden;
            txt_loading.Visibility = Visibility.Visible;
            btn_login.Visibility = Visibility.Hidden;
        }

        private void UserControl_Loaded_1(object sender, RoutedEventArgs e)
        {
            Loading.Visibility = Visibility.Hidden;
        }

        private void txt_account_TextChanged(object sender, TextChangedEventArgs e)
        {
            txt_password.Password = "";
        }

        private void btn_close_Click(object sender, RoutedEventArgs e)
        {
            MainWindow.Instance.Close();
        }
    }
}
