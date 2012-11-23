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
using System.Windows.Shapes;
using System.Threading;
using System.Windows.Media.Animation;
using BlazeGames.IM.Client.Networking;
using BlazeGames.Networking;
using System.Net;

namespace BlazeGames.IM.Client
{
    /// <summary>
    /// Interaction logic for NotificationWindow.xaml
    /// </summary>
    internal partial class NotificationWindow : Window
    {
        static List<NotificationWindow> Windows = new List<NotificationWindow>();

        public static void ShowNotification(string Title, string Message)
        {
            NotificationWindow wnd = new NotificationWindow();
            wnd.Left = (SystemParameters.FullPrimaryScreenWidth - wnd.Width) - 10;
            wnd.Top = 10;
            wnd.txt_notificationtitle.Content = Title;
            wnd.txt_notificationcontent.Text = Message;
            wnd.Show();
            Windows.Add(wnd);
            ReDraw();
        }

        public static void ShowCallNotification(Contact contact, string UDPAddress, int Port)
        {
            SoundManager.VoiceRingingSound.PlayLooping();

            NotificationWindow wnd = new NotificationWindow(true);
            wnd.Left = (SystemParameters.FullPrimaryScreenWidth - wnd.Width) - 10;
            wnd.Top = 10;
            wnd.txt_notificationtitle.Content = "Incoming Call From " + contact.NickName;
            wnd.txt_notificationcontent.Text = "";

            wnd.btn_call_accept.Visibility = Visibility.Visible;
            wnd.btn_call_deny.Visibility = Visibility.Visible;
            wnd.profile_image.Visibility = Visibility.Visible;

            wnd.profile_image_source.ImageSource = new System.Windows.Media.Imaging.BitmapImage(new Uri("https://blaze-games.com/api/image/nocompress/?nickname=" + contact.NickName));

            wnd.UDPAddress = UDPAddress;
            wnd.Port = Port;
            wnd.contact = contact;

            wnd.Show();
            Windows.Add(wnd);
            ReDraw();
        }

        public static void RemoveCallNotification(Contact contact)
        {
            foreach(NotificationWindow wnd in Windows)
            {
                if(wnd.contact != null)
                    if(wnd.contact.ID == contact.ID)
                    {
                        wnd.animation_fadeout.Stop();
                        wnd.animation_fadeout_now.Begin();
                    }
            }
        }

        public static void ShowNotification(string Title, string Message, Contact contact)
        {
            NotificationWindow wnd = new NotificationWindow();
            wnd.Left = (SystemParameters.FullPrimaryScreenWidth - wnd.Width) - 10;
            wnd.Top = 10;
            wnd.txt_notificationtitle.Content = Title;
            wnd.txt_notificationcontent.Text = Message;
            wnd.Show();
            Windows.Add(wnd);
            ReDraw();
        }

        static void ReDraw()
        {
            int i = 0;

            foreach (NotificationWindow wnd in Windows)
            {
                wnd.Top = (wnd.Height * i) + (10 * (i+1));
                i++;
            }
        }

        public static void ForceClose()
        {
            foreach (NotificationWindow wnd in Windows)
                wnd.Close();
        }

        public Storyboard animation_fadeout = new Storyboard();
        public Storyboard animation_fadeout_now = new Storyboard();

        private bool Call;
        public string UDPAddress;
        public int Port;
        public Contact contact;
        private bool CallHandled = false;

        public NotificationWindow(bool Call=false)
        {
            InitializeComponent();

            this.Call = Call;
            TimeSpan FadeOutTime = TimeSpan.FromSeconds(5);

            if (Call)
                FadeOutTime = TimeSpan.FromSeconds(60);

            //  Create an animation for the opacity.
            var opacityAnimation = new DoubleAnimation() { From = 1, To = 0, Duration = new Duration(TimeSpan.FromMilliseconds(750)), BeginTime = FadeOutTime };
            var opacityAnimationNow = new DoubleAnimation() { From = 1, To = 0, Duration = new Duration(TimeSpan.FromMilliseconds(750)) };

            //  Set the targets for the animations.
            Storyboard.SetTarget(opacityAnimation, this);
            Storyboard.SetTargetProperty(opacityAnimation, new PropertyPath(UIElement.OpacityProperty));
            Storyboard.SetTarget(opacityAnimationNow, this);
            Storyboard.SetTargetProperty(opacityAnimationNow, new PropertyPath(UIElement.OpacityProperty));

            animation_fadeout.Children.Add(opacityAnimation);
            animation_fadeout_now.Children.Add(opacityAnimationNow);
            animation_fadeout.Completed += new EventHandler(animation_fadeout_Completed);
            animation_fadeout_now.Completed += new EventHandler(animation_fadeout_Completed);

            if(!IsMouseOver)
                animation_fadeout.Begin();
        }

        void animation_fadeout_Completed(object sender, EventArgs e)
        {
            if (Call && !CallHandled)
            {
                App.Instance.CSocket.SendPacket(Packet.New(Packets.PAK_CLI_CALL_DNY, contact.ID));
                SoundManager.VoiceRingingSound.Stop();
            }

            this.Close();
            Windows.Remove(this);
            ReDraw();
        }

        private void Window_MouseEnter(object sender, MouseEventArgs e)
        {
            try
            {
                if (animation_fadeout_now.GetCurrentState() == ClockState.Active)
                    return;
            }
            catch { }

            this.Opacity = 1;
            animation_fadeout.Stop();
        }

        private void Window_MouseLeave(object sender, MouseEventArgs e)
        {
            try
            {
                if (animation_fadeout_now.GetCurrentState() == ClockState.Active)
                    return;
            }
            catch { }

            animation_fadeout.Begin();
        }

        private void btn_close_Click(object sender, RoutedEventArgs e)
        {
            animation_fadeout.Stop();
            animation_fadeout_now.Begin();
        }

        private void btn_call_accept_Click(object sender, RoutedEventArgs e)
        {
            SoundManager.VoiceRingingSound.Stop();
            CallHandled = true;
            App.Instance.CSocket.SendPacket(Packet.New(Packets.PAK_CLI_CALL_ACC, contact.ID, App.Instance.VCallCore.UDPPort, BlazeGames.IM.Client.Core.Utilities.GetLocalAddress()));
            App.Instance.VCallCore.StartCall(contact.ID, new IPEndPoint(IPAddress.Parse(UDPAddress), Port));
            contact.CallActive = true;

            if(!App.Instance.OpenChats.Contains(contact))
                App.Instance.OpenChats.Add(contact);
            contact.LastMessage = DateTime.Now;

            animation_fadeout.Stop();
            animation_fadeout_now.Begin();
        }

        private void btn_call_deny_Click(object sender, RoutedEventArgs e)
        {
            SoundManager.VoiceRingingSound.Stop();
            CallHandled = true;
            App.Instance.CSocket.SendPacket(Packet.New(Packets.PAK_CLI_CALL_DNY, contact.ID));

            animation_fadeout.Stop();
            animation_fadeout_now.Begin();
        }
    }
}
