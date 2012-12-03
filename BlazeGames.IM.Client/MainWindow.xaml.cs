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
using System.Windows.Markup;
using System.Windows.Media.Animation;
using System.ComponentModel;
using MS.Win32;
using BlazeGames.IM.Client.Networking;
using BlazeGames.Networking;
using System.IO;
using System.Runtime.InteropServices;
using System.Windows.Interop;
using System.Net;

namespace BlazeGames.IM.Client
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    internal partial class MainWindow : Window
    {
        public static MainWindow Instance { get; private set; }
        public ListCollectionView view;

        public Microsoft.Win32.OpenFileDialog dialog;

        public bool Minimum = false;
        public bool Desktop = false;

        public SolidColorBrush Color;
        private SolidColorBrush _Color2;
        public SolidColorBrush Color2
        {
            get { return _Color2; }
            set
            {
                _Color2 = value;
                foreach (Contact contact in App.Instance.Contacts.Values)
                {
                    contact.control.txt_Name.Foreground = value;
                    contact.control.txt_Status.Foreground = value;
                }
            }
        }

        static MainWindow()
        {
            Instance = new MainWindow();
        }

        public MainWindow()
        {
            InitializeComponent();

            this.SourceInitialized += new EventHandler(MainWindow_SourceInitialized);

            SlideFade.StartAnimationIn(this);

            view = new ListCollectionView(App.Instance.OpenChats);
            view.SortDescriptions.Add(new System.ComponentModel.SortDescription("LastMessage", System.ComponentModel.ListSortDirection.Descending));
            view.SortDescriptions.Add(new System.ComponentModel.SortDescription("NewMessages", System.ComponentModel.ListSortDirection.Descending));
            view.SortDescriptions.Add(new System.ComponentModel.SortDescription("NickName", System.ComponentModel.ListSortDirection.Ascending));
            view.Refresh();

            this.listbox1.ItemsSource = view;
            //listbox1.Items.SortDescriptions.Add(new System.ComponentModel.SortDescription("NewMessages", System.ComponentModel.ListSortDirection.Descending));
            //listbox1.Items.SortDescriptions.Add(new System.ComponentModel.SortDescription("NickName", System.ComponentModel.ListSortDirection.Ascending));

            dialog = new Microsoft.Win32.OpenFileDialog();
            dialog.Filter = "Image Files|*.png;*.jpg;*.bmp;*.jpeg";
            dialog.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyPictures);
            dialog.FileOk += new CancelEventHandler(dialog_FileOk);

            try
            {
                int Width = Convert.ToInt32(ConfigManager.Instance.GetString("WndWidth", this.Width.ToString()));
                int Height = Convert.ToInt32(ConfigManager.Instance.GetString("WndHeight", this.Height.ToString()));

                if (Width >= SystemParameters.WorkArea.Width || Height >= SystemParameters.WorkArea.Height)
                    return;

                this.Width = Width;
                this.Height = Height;
            }
            catch { }
        }

        private const int WM_SYSCOMMAND = 0x112;
        private HwndSource hwndSource;

        void MainWindow_SourceInitialized(object sender, EventArgs e)
        {
            hwndSource = PresentationSource.FromVisual((Visual)sender) as HwndSource;
        }

        public enum ResizeDirection
        {
            Left = 1,
            Right = 2,
            Top = 3,
            TopLeft = 4,
            TopRight = 5,
            Bottom = 6,
            BottomLeft = 7,
            BottomRight = 8,
        }

        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        private static extern IntPtr SendMessage(IntPtr hWnd, uint Msg, IntPtr wParam, IntPtr lParam);

        private void ResizeWindow(ResizeDirection direction)
        {
            SendMessage(hwndSource.Handle, WM_SYSCOMMAND, (IntPtr)(61440 + direction), IntPtr.Zero);
        }

        #region AnimationHeight
        [StructLayout(LayoutKind.Sequential)]
        public struct RECT
        {
            public int X;
            public int Y;
            public int Width;
            public int Height;
        }

        public enum SpecialWindowHandles
        {
            HWND_TOP = 0,
            HWND_BOTTOM = 1,
            HWND_TOPMOST = -1,
            HWND_NOTOPMOST = -2
        }

        [DllImport("user32.dll", SetLastError = true)]
        static extern bool GetWindowRect(IntPtr hWnd, ref RECT Rect);

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int X, int Y, int cx, int cy, uint uFlags);

        public static readonly DependencyProperty WindowHeightAnimationProperty = DependencyProperty.Register("WindowHeightAnimation", typeof(double),
                                                                                                    typeof(MainWindow), new PropertyMetadata(OnWindowHeightAnimationChanged));

        private static void OnWindowHeightAnimationChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var window = d as Window;

            if (window != null)
            {
                IntPtr handle = new WindowInteropHelper(window).Handle;
                var rect = new RECT();
                if (GetWindowRect(handle, ref rect))
                {
                    rect.X = (int)window.Left;
                    rect.Y = (int)window.Top;

                    rect.Width = (int)window.ActualWidth;
                    rect.Height = (int)(double)e.NewValue;  // double casting from object to double to int

                    SetWindowPos(handle, new IntPtr((int)SpecialWindowHandles.HWND_TOP), rect.X, rect.Y, rect.Width, rect.Height, (uint)SWP.SHOWWINDOW);
                }
            }
        }

        public double WindowHeightAnimation
        {
            get { return (double)GetValue(WindowHeightAnimationProperty); }
            set { SetValue(WindowHeightAnimationProperty, value); }
        }

        public static readonly DependencyProperty WindowWidthAnimationProperty = DependencyProperty.Register("WindowWidthAnimation", typeof(double),
                                                                                                    typeof(MainWindow), new PropertyMetadata(OnWindowWidthAnimationChanged));

        private static void OnWindowWidthAnimationChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var window = d as Window;

            if (window != null)
            {
                IntPtr handle = new WindowInteropHelper(window).Handle;
                var rect = new RECT();
                if (GetWindowRect(handle, ref rect))
                {
                    rect.X = (int)window.Left;
                    rect.Y = (int)window.Top;
                    var width = (int)(double)e.NewValue;
                    rect.Width = width;
                    rect.Height = (int)window.ActualHeight;

                    SetWindowPos(handle, new IntPtr((int)SpecialWindowHandles.HWND_TOP), rect.X, rect.Y, rect.Width, rect.Height, (uint)SWP.SHOWWINDOW);
                }
            }
        }

        public double WindowWidthAnimation
        {
            get { return (double)GetValue(WindowWidthAnimationProperty); }
            set { SetValue(WindowWidthAnimationProperty, value); }
        }

        /// <summary>
        /// SetWindowPos Flags
        /// </summary>
        public static class SWP
        {
            public static readonly int
            NOSIZE = 0x0001,
            NOMOVE = 0x0002,
            NOZORDER = 0x0004,
            NOREDRAW = 0x0008,
            NOACTIVATE = 0x0010,
            DRAWFRAME = 0x0020,
            FRAMECHANGED = 0x0020,
            SHOWWINDOW = 0x0040,
            HIDEWINDOW = 0x0080,
            NOCOPYBITS = 0x0100,
            NOOWNERZORDER = 0x0200,
            NOREPOSITION = 0x0200,
            NOSENDCHANGING = 0x0400,
            DEFERERASE = 0x2000,
            ASYNCWINDOWPOS = 0x4000;
        }
        #endregion

        private void wnd_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (txt_status.IsKeyboardFocused)
                Keyboard.ClearFocus();

            if (AddFriendWindow.Visibility == System.Windows.Visibility.Visible)
                AddFriendWindow.Visibility = System.Windows.Visibility.Collapsed;

            if (e.LeftButton == MouseButtonState.Pressed && !Minimum && !Desktop)
            {
                DragMove();
            }
        }

        private void btn_close_Click(object sender, RoutedEventArgs e)
        {
            //wnd.WindowStyle = System.Windows.WindowStyle.SingleBorderWindow;
            wnd.WindowState = System.Windows.WindowState.Minimized;
            wnd.WindowStyle = System.Windows.WindowStyle.None;
        }

        public string CurrentPage = "login";

        private Contact[] TmpOpenChats = null;
        private void btn_minimize_Click(object sender, RoutedEventArgs e)
        {
            if (!Desktop)
            {
                TmpOpenChats = App.Instance.OpenChats.ToArray();
                App.Instance.OpenChats.Clear();
                view.Refresh();
                SlideFade.CreateAnimationInMinimum(listbox1);
                App.Instance.OpenChats.AddRange(App.Instance.Contacts.Values.ToArray());
                view.Refresh();

                Topmost = true;

                /*LastLeft = Left;
                Width = 70;
                Left = SystemParameters.PrimaryScreenWidth - 70;
                Minimum = true;*/
                SlideFade.CreateAnimationToMinimum();
                btn_maximum1.Visibility = System.Windows.Visibility.Visible;
                btn_maximum2.Visibility = System.Windows.Visibility.Visible;
            }
        }

        private void btn_settings_Click(object sender, RoutedEventArgs e)
        {
            if (!App.Instance.LoginComplete || CurrentPage == "settings")
            {
                return;
            }

            btn_home.BorderThickness = new Thickness(0);
            btn_contacts.BorderThickness = new Thickness(0);
            btn_chat.BorderThickness = new Thickness(0);
            btn_settings.BorderThickness = new Thickness(0, 0, 0, 2);

            SlideFade.StartAnimationIn(page_Settings);

            page_Home.Visibility = System.Windows.Visibility.Hidden;
            page_Contacts.Visibility = System.Windows.Visibility.Hidden;
            page_Chat.Visibility = System.Windows.Visibility.Hidden;
            page_Settings.Visibility = System.Windows.Visibility.Visible;

            CurrentPage = "settings";
        }

        private void btn_chat_Click(object sender, RoutedEventArgs e)
        {
            if (!App.Instance.LoginComplete || CurrentPage == "chat")
            {
                return;
            }

            if (page_Chat.ChattingWith != null)
            {
                btn_home.BorderThickness = new Thickness(0);
                btn_contacts.BorderThickness = new Thickness(0);
                btn_chat.BorderThickness = new Thickness(0, 0, 0, 2);
                btn_settings.BorderThickness = new Thickness(0);

                SlideFade.StartAnimationIn(page_Chat);

                page_Home.Visibility = System.Windows.Visibility.Hidden;
                page_Contacts.Visibility = System.Windows.Visibility.Hidden;
                page_Chat.Visibility = System.Windows.Visibility.Visible;
                page_Settings.Visibility = System.Windows.Visibility.Hidden;

                CurrentPage = "chat";
            }
            else
            {
                if (CurrentPage == "contacts")
                {
                    return;
                }

                btn_home.BorderThickness = new Thickness(0);
                btn_contacts.BorderThickness = new Thickness(0, 0, 0, 2);
                btn_chat.BorderThickness = new Thickness(0);
                btn_settings.BorderThickness = new Thickness(0);


                SlideFade.StartAnimationIn(page_Contacts);

                page_Home.Visibility = System.Windows.Visibility.Hidden;
                page_Contacts.Visibility = System.Windows.Visibility.Visible;
                page_Chat.Visibility = System.Windows.Visibility.Hidden;
                page_Settings.Visibility = System.Windows.Visibility.Hidden;

                CurrentPage = "contacts";
            }
        }

        private void btn_contacts_Click(object sender, RoutedEventArgs e)
        {
            if (!App.Instance.LoginComplete || CurrentPage == "contacts")
            {
                return;
            }

            btn_home.BorderThickness = new Thickness(0);
            btn_contacts.BorderThickness = new Thickness(0, 0, 0, 2);
            btn_chat.BorderThickness = new Thickness(0);
            btn_settings.BorderThickness = new Thickness(0);


            SlideFade.StartAnimationIn(page_Contacts);

            page_Home.Visibility = System.Windows.Visibility.Hidden;
            page_Contacts.Visibility = System.Windows.Visibility.Visible;
            page_Chat.Visibility = System.Windows.Visibility.Hidden;
            page_Settings.Visibility = System.Windows.Visibility.Hidden;

            CurrentPage = "contacts";
        }

        private void btn_home_Click(object sender, RoutedEventArgs e)
        {
            if (!App.Instance.LoginComplete || CurrentPage == "home")
            {
                return;
            }

            btn_home.BorderThickness = new Thickness(0, 0, 0, 2);
            btn_contacts.BorderThickness = new Thickness(0);
            btn_chat.BorderThickness = new Thickness(0);
            btn_settings.BorderThickness = new Thickness(0);

            SlideFade.StartAnimationIn(page_Home);

            page_Home.Visibility = System.Windows.Visibility.Visible;
            page_Contacts.Visibility = System.Windows.Visibility.Hidden;
            page_Chat.Visibility = System.Windows.Visibility.Hidden;
            page_Settings.Visibility = System.Windows.Visibility.Hidden;

            CurrentPage = "home";
        }

        private void wnd_StateChanged(object sender, EventArgs e)
        {
            if(wnd.WindowState == System.Windows.WindowState.Normal)
                SlideFade.StartAnimationIn(wnd); 
        }

        private void listbox1_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (!App.Instance.LoginComplete)
            {
                if (listbox1.SelectedIndex != -1)
                    listbox1.SelectedIndex = -1;

                return;
            }

            if (listbox1.SelectedIndex != -1)
            {
                Contact contact = listbox1.SelectedItem as Contact;

                if (Minimum)
                {
                    /*Width = 850;
                    Left = LastLeft;*/
                    SlideFade.CreateAnimationToMaximum();
                    Minimum = false;

                    App.Instance.OpenChats.Clear();
                    view.Refresh();
                    SlideFade.CreateAnimationInMinimum(listbox1);
                    App.Instance.OpenChats.AddRange(TmpOpenChats);
                    view.Refresh();

                    Topmost = false;
                }

                if (!App.Instance.OpenChats.Contains(contact))
                    App.Instance.OpenChats.Add(contact);

                if (page_Chat.ChattingWith != contact || CurrentPage != "chat")
                {
                    btn_home.BorderThickness = new Thickness(0);
                    btn_contacts.BorderThickness = new Thickness(0);
                    btn_chat.BorderThickness = new Thickness(0, 0, 0, 2);
                    btn_settings.BorderThickness = new Thickness(0);

                    SlideFade.StartAnimationIn(page_Chat);

                    page_Home.Visibility = System.Windows.Visibility.Hidden;
                    page_Contacts.Visibility = System.Windows.Visibility.Hidden;
                    page_Chat.Visibility = System.Windows.Visibility.Visible;
                    page_Settings.Visibility = System.Windows.Visibility.Hidden;
                }
                

                page_Chat.StartChattingWith(contact);
                contact.MarkAllMessagesRead();
                Console.WriteLine("ChatWith(\"{0}\");", contact.NickName);
                listbox1.SelectedIndex = -1;

                CurrentPage = "chat";
            }
        }

        private void wnd_Loaded(object sender, RoutedEventArgs e)
        {
            BlazeGames.IM.Client.MainWindow.Instance.txt_nickname.Visibility = Visibility.Hidden;
            BlazeGames.IM.Client.MainWindow.Instance.txt_status.Visibility = Visibility.Hidden;
            BlazeGames.IM.Client.MainWindow.Instance.profile_image.Visibility = Visibility.Hidden;
            BlazeGames.IM.Client.MainWindow.Instance.nav_bar.Visibility = Visibility.Hidden;
            BlazeGames.IM.Client.MainWindow.Instance.listbox1.Visibility = Visibility.Hidden;

            BlazeGames.IM.Client.MainWindow.Instance.btn_ProfileSettings.Visibility = Visibility.Hidden;
            BlazeGames.IM.Client.MainWindow.Instance.btn_AddFriend.Visibility = Visibility.Hidden;

            BlazeGames.IM.Client.MainWindow.Instance.btn_chat.Visibility = Visibility.Hidden;
            BlazeGames.IM.Client.MainWindow.Instance.btn_close.Visibility = Visibility.Hidden;
            BlazeGames.IM.Client.MainWindow.Instance.btn_contacts.Visibility = Visibility.Hidden;
            BlazeGames.IM.Client.MainWindow.Instance.btn_home.Visibility = Visibility.Hidden;
            BlazeGames.IM.Client.MainWindow.Instance.btn_minimize.Visibility = Visibility.Hidden;
            BlazeGames.IM.Client.MainWindow.Instance.btn_settings.Visibility = Visibility.Hidden;
            BlazeGames.IM.Client.MainWindow.Instance.txt_search.Visibility = Visibility.Hidden;

            btn_maximum1.Visibility = System.Windows.Visibility.Collapsed;
            btn_maximum2.Visibility = System.Windows.Visibility.Collapsed;
        }

        private void txt_search_TextChanged(object sender, TextChangedEventArgs e)
        {
            this.page_Contacts.Draw();

            /*if (txt_search.Text == "%RAINBOW%")
            {
                SolidColorBrush RainbowBrush = new SolidColorBrush(Colors.Red);

                ColorAnimation[] RainbowAnimations = new ColorAnimation[]
                {
                    new ColorAnimation(Colors.Red, TimeSpan.FromSeconds(1)) { BeginTime = TimeSpan.FromSeconds(0), RepeatBehavior = System.Windows.Media.Animation.RepeatBehavior.Forever },
                    new ColorAnimation(Colors.Orange, TimeSpan.FromSeconds(1)) { BeginTime = TimeSpan.FromSeconds(1), RepeatBehavior = System.Windows.Media.Animation.RepeatBehavior.Forever },
                    new ColorAnimation(Colors.Yellow, Colors.Orange, TimeSpan.FromSeconds(1)) { BeginTime = TimeSpan.FromSeconds(2), RepeatBehavior = System.Windows.Media.Animation.RepeatBehavior.Forever },
                    new ColorAnimation(Colors.Green, Colors.Orange, TimeSpan.FromSeconds(1)) { BeginTime = TimeSpan.FromSeconds(3), RepeatBehavior = System.Windows.Media.Animation.RepeatBehavior.Forever },
                    new ColorAnimation(Colors.Blue, Colors.Orange, TimeSpan.FromSeconds(1)) { BeginTime = TimeSpan.FromSeconds(4), RepeatBehavior = System.Windows.Media.Animation.RepeatBehavior.Forever },
                    new ColorAnimation(Colors.Indigo, Colors.Orange, TimeSpan.FromSeconds(1)) { BeginTime = TimeSpan.FromSeconds(5), RepeatBehavior = System.Windows.Media.Animation.RepeatBehavior.Forever },
                    new ColorAnimation(Colors.Violet, Colors.Orange, TimeSpan.FromSeconds(1)) { BeginTime = TimeSpan.FromSeconds(6), RepeatBehavior = System.Windows.Media.Animation.RepeatBehavior.Forever }
                };

                Storyboard RainbowAnimationStoryboard = new Storyboard();
                Storyboard.SetTarget(RainbowAnimationStoryboard, RainbowBrush);

                foreach (ColorAnimation Animation in RainbowAnimations)
                {
                    Storyboard.SetTarget(Animation, RainbowAnimationStoryboard);
                    Storyboard.SetTargetProperty(Animation, new PropertyPath(SolidColorBrush.ColorProperty));
                }

                RainbowAnimationStoryboard.Children.Add(RainbowAnimations[0]);
                RainbowAnimationStoryboard.Children.Add(RainbowAnimations[1]);
                RainbowAnimationStoryboard.Children.Add(RainbowAnimations[2]);
                RainbowAnimationStoryboard.Children.Add(RainbowAnimations[3]);
                RainbowAnimationStoryboard.Children.Add(RainbowAnimations[4]);
                RainbowAnimationStoryboard.Children.Add(RainbowAnimations[5]);
                RainbowAnimationStoryboard.Children.Add(RainbowAnimations[6]);

                RainbowAnimationStoryboard.SpeedRatio = 3;
                RainbowAnimationStoryboard.Begin();
                
                txt_nickname.Foreground = RainbowBrush;

                txt_search.Text = "";
            }*/
        }

        private void txt_search_GotFocus(object sender, RoutedEventArgs e)
        {
            if (!App.Instance.LoginComplete)
                return;

            btn_home.BorderThickness = new Thickness(0);
            btn_contacts.BorderThickness = new Thickness(0, 0, 0, 2);
            btn_chat.BorderThickness = new Thickness(0);
            btn_settings.BorderThickness = new Thickness(0);


            SlideFade.StartAnimationIn(page_Contacts);

            page_Home.Visibility = System.Windows.Visibility.Hidden;
            page_Contacts.Visibility = System.Windows.Visibility.Visible;
            page_Chat.Visibility = System.Windows.Visibility.Hidden;
            page_Settings.Visibility = System.Windows.Visibility.Hidden;

            CurrentPage = "contacts";
        }

        private void setstatus_online_Click(object sender, RoutedEventArgs e)
        {
            MenuItem item = sender as MenuItem;
            MenuItem parent = item.Parent as MenuItem;

            foreach (object obj in parent.Items)
            {
                MenuItem mitem = obj as MenuItem;
                if (mitem.Header != item.Header)
                    mitem.IsChecked = false;
            }

            item.IsChecked = true;

            App.CurrentStatus = Status.Online;
        }

        private void setstatus_away_Click(object sender, RoutedEventArgs e)
        {
            MenuItem item = sender as MenuItem;
            MenuItem parent = item.Parent as MenuItem;

            foreach (object obj in parent.Items)
            {
                MenuItem mitem = obj as MenuItem;
                if (mitem.Header != item.Header)
                    mitem.IsChecked = false;
            }

            item.IsChecked = true;

            App.CurrentStatus = Status.Away;
        }

        private void setstatus_busy_Click(object sender, RoutedEventArgs e)
        {
            MenuItem item = sender as MenuItem;
            MenuItem parent = item.Parent as MenuItem;

            foreach (object obj in parent.Items)
            {
                MenuItem mitem = obj as MenuItem;
                if (mitem.Header != item.Header)
                    mitem.IsChecked = false;
            }

            item.IsChecked = true;

            App.CurrentStatus = Status.Busy;
        }

        private void setstatus_offline_Click(object sender, RoutedEventArgs e)
        {
            MenuItem item = sender as MenuItem;
            MenuItem parent = item.Parent as MenuItem;

            foreach (object obj in parent.Items)
            {
                MenuItem mitem = obj as MenuItem;
                if (mitem.Header != item.Header)
                    mitem.IsChecked = false;
            }

            item.IsChecked = true;

            App.CurrentStatus = Status.Offline;
        }

        private void MenuItem_Click(object sender, RoutedEventArgs e)
        {
            
        }

        private void MenuItem_Click_1(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void MenuItem_Click_2(object sender, RoutedEventArgs e)
        {
            if (Minimum)
            {
                SlideFade.CreateAnimationToMaximum();
                Minimum = false;

                App.Instance.OpenChats.Clear();
                view.Refresh();
                SlideFade.CreateAnimationInMinimum(listbox1);
                App.Instance.OpenChats.AddRange(TmpOpenChats);
                view.Refresh();

                Topmost = false;

                btn_maximum1.Visibility = System.Windows.Visibility.Collapsed;
                btn_maximum2.Visibility = System.Windows.Visibility.Collapsed;
            }
        }

        private void wnd_GotFocus(object sender, RoutedEventArgs e)
        {
            WindowExtensions.StopFlashingWindow(this);
        }

        private void wnd_MouseEnter(object sender, MouseEventArgs e)
        {
            if (Minimum)
            {
                SlideFade.CreateAnimationToMinimum_max();
            }
        }

        private void wnd_MouseLeave(object sender, MouseEventArgs e)
        {
            if (Minimum)
            {
                SlideFade.CreateAnimationToMinimum_min();
            }
        }

        private void txt_status_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter || e.Key == Key.Return)
            {
                this.txt_nickname.Focus();
                Keyboard.ClearFocus();
            }
        }

        string LastStatus = "";
        private void txt_status_LostKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
        {
            if (LastStatus != txt_status.Text)
            {
                App.Instance.CSocket.SendPacket(Packet.New(Packets.PAK_CLI_CHNGUPDTRQST, txt_status.Text));
                LastStatus = txt_status.Text;
            }
        }

        private void txt_addfriend_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Return || e.Key == Key.Enter)
            {
                TextBox tb = sender as TextBox;
                string ID = tb.Text;
                tb.Text = "";

                App.Instance.CSocket.SendPacket(Packet.New(Packets.PAK_CLI_FRNDADDRQST, ID));
            }
        }

        private void wnd_Closing(object sender, CancelEventArgs e)
        {
            if (!Minimum)
            {
                ConfigManager.Instance.SetValue("WndWidth", this.Width.ToString());
                ConfigManager.Instance.SetValue("WndHeight", this.Height.ToString());
                ConfigManager.Instance.Save();
            }

            NotificationWindow.ForceClose();

            try
            {
                ConsoleWindow.Instance.Close();
            }
            catch { }
        }

        private void bar_Loaded(object sender, RoutedEventArgs e)
        {
            
        }

        private void btn_changeprofileimg_Click(object sender, RoutedEventArgs e)
        {
            dialog.ShowDialog();
        }

        System.Drawing.Image ProfileImage = null;

        void dialog_FileOk(object sender, CancelEventArgs e)
        {
            try
            {
                using (Stream stream = dialog.OpenFile())
                {
                    MemoryStream ms = new MemoryStream();

                    System.Drawing.Image img = System.Drawing.Image.FromStream(stream);
                    ProfileImage = img;
                    img.Save(ms, System.Drawing.Imaging.ImageFormat.Png);

                    WebClient wc_uploader = new WebClient();
                    wc_uploader.UploadDataCompleted += new UploadDataCompletedEventHandler(wc_uploader_UploadDataCompleted);
                    wc_uploader.UploadDataAsync(new Uri(String.Format("https://blaze-games.com/api/uploadimage/?account={0}&password={1}", App.Account, App.Password)), ms.ToArray());
                }
            }
            catch { NotificationWindow.ShowNotification("Upload Failed", "Unable to upload your profile image since it was not a valid image file."); }
        }

        void wc_uploader_UploadDataCompleted(object sender, UploadDataCompletedEventArgs e)
        {
            //TODO: Update the profile image client sided
            BlazeGames.IM.Client.MainWindow.Instance.profile_image_source.ImageSource = new System.Windows.Media.Imaging.BitmapImage(new Uri("https://blaze-games.com/api/image/nocompress/?nickname=" + App.NickName), new System.Net.Cache.RequestCachePolicy(System.Net.Cache.RequestCacheLevel.NoCacheNoStore));
        }

        private void btn_AddFriend_Click(object sender, RoutedEventArgs e)
        {
            if (AddFriendWindow.Visibility == System.Windows.Visibility.Collapsed)
                AddFriendWindow.Visibility = System.Windows.Visibility.Visible;
            else
                AddFriendWindow.Visibility = System.Windows.Visibility.Collapsed;
        }

        private void btn_ProfileSettings_Click(object sender, RoutedEventArgs e)
        {
            if (AddFriendWindow.Visibility != System.Windows.Visibility.Collapsed)
                AddFriendWindow.Visibility = System.Windows.Visibility.Collapsed;

            this.ProfileSettingsMenu.PlacementTarget = btn_settings;
            this.ProfileSettingsMenu.IsOpen = true;
        }

        private void resize_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (!Minimum && !Desktop)
            {
                Rectangle resizecontrol = sender as Rectangle;
                switch(resizecontrol.Name)
                {
                    case "resize_Top": ResizeWindow(ResizeDirection.Top); break;
                    case "resize_Bottom": ResizeWindow(ResizeDirection.Bottom); break;
                    case "resize_Left": ResizeWindow(ResizeDirection.Left); break;
                    case "resize_Right": ResizeWindow(ResizeDirection.Right); break;

                    case "resize_TopLeft": ResizeWindow(ResizeDirection.TopLeft); break;
                    case "resize_TopRight": ResizeWindow(ResizeDirection.TopRight); break;
                    case "resize_BottomLeft": ResizeWindow(ResizeDirection.BottomLeft); break;
                    case "resize_BottomRight": ResizeWindow(ResizeDirection.BottomRight); break;
                    default: break;
                }
            }
        }

        private void btn_call_end_Click(object sender, RoutedEventArgs e)
        {
            Button clicked = sender as Button;
            int MemberID = Convert.ToInt32(clicked.Tag);
            Contact contact = App.Instance.Contacts[MemberID];

            App.Instance.VCallCore.EndCall(contact.ID);
            App.Instance.CSocket.SendPacket(Packet.New(Packets.PAK_CLI_CALL_DNY, contact.ID));
            contact.CallActive = false;
            SoundManager.VoiceCallingSound.Stop();
        }

        private void btn_call_start_Click(object sender, RoutedEventArgs e)
        {
            Button clicked = sender as Button;
            int MemberID = Convert.ToInt32(clicked.Tag);
            Contact contact = App.Instance.Contacts[MemberID];

            if (contact.status != Status.Offline)
            {
                App.Instance.CSocket.SendPacket(Packet.New(Packets.PAK_CLI_CALL_RQST, contact.ID, App.Instance.VCallCore.UDPPort, BlazeGames.IM.Client.Core.Utilities.GetLocalAddress()));
                SoundManager.VoiceCallingSound.PlayLooping();
                contact.CallActive = true;
            }
        }

        private void btn_desktop_Click(object sender, RoutedEventArgs e)
        {
            if (!Minimum)
            {
                if (Desktop)
                {
                    try
                    {
                        int Width = Convert.ToInt32(ConfigManager.Instance.GetString("WndWidth", this.Width.ToString()));
                        int Height = Convert.ToInt32(ConfigManager.Instance.GetString("WndHeight", this.Height.ToString()));

                        if (Width >= SystemParameters.WorkArea.Width || Height >= SystemParameters.WorkArea.Height)
                        {
                            this.Width = 850;
                            this.Height = 575;
                        }
                        else
                        {
                            this.Width = Width;
                            this.Height = Height;
                        }
                    }
                    catch
                    {
                        this.Width = 850;
                        this.Height = 575;
                    }

                    btn_desktop.IsChecked = false;
                    btn_desktop1.IsChecked = false;
                    Desktop = false;
                }
                else
                {
                    this.Left = 0;
                    this.Top = 0;
                    this.Width = SystemParameters.WorkArea.Width;
                    this.Height = SystemParameters.WorkArea.Height;

                    btn_desktop.IsChecked = true;
                    btn_desktop1.IsChecked = true;
                    Desktop = true;
                }
            }
        }

        private void btn_disconnect_Click(object sender, RoutedEventArgs e)
        {
            App.Instance.CSocket.Disconnect();
        }
    }

    internal class ConcatStringExtension : MarkupExtension
    {
        //Converter to generate the string
        class ConcatString : IValueConverter
        {
            public string InitString { get; set; }

            #region IValueConverter Members
            public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
            {
                //append the string
                return InitString + value.ToString();
            }
            public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
            {
                throw new NotImplementedException();
            }
            #endregion
        }
        //the value to bind to
        public Binding BindTo { get; set; }
        //the string to attach in front of the value
        public string AttachString { get; set; }

        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            //modify the binding by setting the converter
            BindTo.Converter = new ConcatString { InitString = AttachString };
            return BindTo.ProvideValue(serviceProvider);
        }
    }
}
