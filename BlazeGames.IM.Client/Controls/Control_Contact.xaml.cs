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

namespace BlazeGames.IM.Client
{
    /// <summary>
    /// Interaction logic for Control_Contact.xaml
    /// </summary>
    internal partial class Control_Contact : UserControl
    {
        Contact contact;

        public Control_Contact(Contact contact)
        {
            InitializeComponent();

            this.contact = contact;

            profile_image_source.ImageSource = new System.Windows.Media.Imaging.BitmapImage(new Uri(contact.ProfileImage));
            profile_image.BorderBrush = (SolidColorBrush)new BrushConverter().ConvertFromString(contact.status.GetColor());
            txt_Name.Text = contact.NickName;
            txt_Status.Text = contact.StatusUpdate;

            if (contact.Pending)
            {
                btn_acceptfriend.Visibility = System.Windows.Visibility.Visible;
                btn_denyfriend.Visibility = System.Windows.Visibility.Visible;
                btn_removefriend.Visibility = System.Windows.Visibility.Collapsed;
                txt_Status.Text = "Pending Request";
                txt_Status.Foreground = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(125, 125, 125));
            }

            btn_removefriend.Header = String.Format(btn_removefriend.Header.ToString(), contact.NickName);
            btn_blockfriend.Header = String.Format(btn_blockfriend.Header.ToString(), contact.NickName);
        }

        private void UserControl_MouseEnter_1(object sender, MouseEventArgs e)
        {
            if (MainWindow.Instance.Color == null)
                MainWindow.Instance.Color = new SolidColorBrush((Color)ColorConverter.ConvertFromString(ConfigManager.Instance.GetString("design_color", "#FF25A0DA")));

            this.BorderBrush = MainWindow.Instance.Color;
        }

        private void UserControl_MouseLeave_1(object sender, MouseEventArgs e)
        {
            this.BorderBrush = null;
        }

        private void btn_acceptfriend_Click(object sender, RoutedEventArgs e)
        {
            if (contact.Pending)
                App.Instance.CSocket.SendPacket(Packet.New(Packets.PAK_CLI_FRNDADDRQST, contact.NickName));
        }

        private void btn_denyfriend_Click(object sender, RoutedEventArgs e)
        {
            if (contact.Pending)
                App.Instance.CSocket.SendPacket(Packet.New(Packets.PAK_CLI_FRNDDNYRQST, contact.ID));
        }

        private void btn_removefriend_Click(object sender, RoutedEventArgs e)
        {
            if (!contact.Pending)
                App.Instance.CSocket.SendPacket(Packet.New(Packets.PAK_CLI_FRNDRMVRQST, contact.ID));
        }

        private void btn_blockfriend_Click(object sender, RoutedEventArgs e)
        {
            App.Instance.CSocket.SendPacket(Packet.New(Packets.PAK_CLI_FRNDBLKRQST, contact.ID));
        }

        private void UserControl_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (this.IsMouseOver)
            {
                if (!contact.Pending)
                {
                    if (MainWindow.Instance.page_Chat.ChattingWith != contact || MainWindow.Instance.CurrentPage != "chat")
                    {
                        MainWindow.Instance.btn_home.BorderThickness = new Thickness(0);
                        MainWindow.Instance.btn_contacts.BorderThickness = new Thickness(0);
                        MainWindow.Instance.btn_chat.BorderThickness = new Thickness(0, 0, 0, 2);
                        MainWindow.Instance.btn_settings.BorderThickness = new Thickness(0);

                        SlideFade.StartAnimationIn(MainWindow.Instance.page_Chat);

                        MainWindow.Instance.page_Home.Visibility = System.Windows.Visibility.Hidden;
                        MainWindow.Instance.page_Contacts.Visibility = System.Windows.Visibility.Hidden;
                        MainWindow.Instance.page_Chat.Visibility = System.Windows.Visibility.Visible;
                        MainWindow.Instance.page_Settings.Visibility = System.Windows.Visibility.Hidden;
                    }

                    MainWindow.Instance.page_Chat.StartChattingWith(contact);
                    if (!App.Instance.OpenChats.Contains(contact))
                        App.Instance.OpenChats.Add(contact);
                    contact.MarkAllMessagesRead();

                    MainWindow.Instance.CurrentPage = "chat";
                }
            }
        }
    }
}
