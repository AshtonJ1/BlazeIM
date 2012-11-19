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

namespace BlazeGames.IM.Client
{
    /// <summary>
    /// Interaction logic for Notification.xaml
    /// </summary>
    internal partial class Notification : Window
    {
        static Notification Instance;

        private List<NotificationMessage> Notifications = new List<NotificationMessage>();

        public void Show(string Title, string Message, Image Img=null, string Act = null)
        {

        }

        Storyboard animation_fadeout = new Storyboard();
        Storyboard animation_fadeout_now = new Storyboard();

        public Notification()
        {
            InitializeComponent();

            //  Create an animation for the opacity.
            var opacityAnimation = new DoubleAnimation() { From = 1, To = 0, Duration = new Duration(TimeSpan.FromMilliseconds(750)), BeginTime = TimeSpan.FromSeconds(5) };
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
            this.Close();
            
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
    }

    internal class NotificationMessage
    {

    }
}
