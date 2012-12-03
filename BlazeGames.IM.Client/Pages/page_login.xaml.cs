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
using System.Threading;

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
            string HashPassword = Utilities.MD5("BGxSecure" + Utilities.MD5(txt_password.Password));

            /*if (chk_rememberme.IsChecked == true)
            {
                ConfigManager.Instance.SetValue("", "");
                ConfigManager.Instance.SetValue("", HashPassword);
            }*/

            if (!App.Instance.CSocket.RawSocket.Connected)
            {
                App.Instance.CSocket = new ClientSocket(IPAddress.Parse("209.141.53.112"), 25050);
                App.Instance.CSocket.ClientSocketPacketReceived_Event += new ClientSocketPacketReceived_Handler(App.Instance.CSocket_ClientSocketPacketReceived_Event);
                App.Instance.CSocket.ClientSocketConnected_Event += new EventHandler(App.Instance.CSocket_ClientSocketConnected_Event);
                App.Instance.CSocket.ClientSocketDisconnected_Event += new EventHandler(App.Instance.CSocket_ClientSocketDisconnected_Event);
                App.Instance.CSocket.ClientSocketConnected_Event += (conn_sender, conn_e) =>
                {
                    App.Instance.Dispatcher.BeginInvoke((App.MethodInvoker)delegate
                    {
                        App.Instance.CSocket.SendPacket(Packet.New(Packets.PAK_CLI_LGNRQST, txt_account.Text, HashPassword));
                    }, null);
                };
                App.Instance.CSocket.Connect();
            }
            else
                App.Instance.CSocket.SendPacket(Packet.New(Packets.PAK_CLI_LGNRQST, txt_account.Text, HashPassword));

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
