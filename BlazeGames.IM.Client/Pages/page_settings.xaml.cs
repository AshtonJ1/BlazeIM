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
using System.Collections.ObjectModel;
using System.Drawing.Text;
using System.Threading;
using System.Windows.Media.Animation;

namespace BlazeGames.IM.Client
{
    /// <summary>
    /// Interaction logic for page_home.xaml
    /// </summary>
    internal partial class page_settings : UserControl
    {
        public Color AccentColor
        {
            get { return (Color)this.GetValue(AccentColorProperty); }
            set
            {
                this.Resources["AccentColor"] = value;
                this.SetValue(AccentColorProperty, value);
            }
        }
        public static readonly DependencyProperty AccentColorProperty = DependencyProperty.Register("AccentColor", typeof(Color), typeof(page_settings), new PropertyMetadata(default(Color)));

        public Microsoft.Win32.OpenFileDialog bg_browse_dialog;
        public page_settings()
        {
            if (!System.ComponentModel.DesignerProperties.GetIsInDesignMode(this))
            {
                InitializeComponent();

                bg_browse_dialog = new Microsoft.Win32.OpenFileDialog();
                bg_browse_dialog.Filter = "Image Files|*.png;*.jpg;*.bmp;*.jpeg";
                bg_browse_dialog.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyPictures);
                bg_browse_dialog.FileOk += bg_browse_dialog_FileOk;
            }
        }

        void bg_browse_dialog_FileOk(object sender, System.ComponentModel.CancelEventArgs e)
        {
            try
            {
                ImageBrush imgBrush = new ImageBrush(new BitmapImage(new Uri(bg_browse_dialog.FileName)));
                imgBrush.Stretch = Stretch.UniformToFill;

                MainWindow.Instance.Background = imgBrush;

                ConfigManager.Instance.SetValue("custom_background", bg_browse_dialog.FileName);
                ConfigManager.Instance.Save();
            }
            catch { MessageBox.Show("Corrupt Image"); }
        }

        private void chk_notifications_changed(object sender, RoutedEventArgs e)
        {
            ConfigManager.Instance.SetValue("txt_notifications", chk_txtnotifications.IsChecked);
            ConfigManager.Instance.SetValue("txt_loginnotification", chk_loginnotification.IsChecked);
            ConfigManager.Instance.SetValue("txt_logoutnotification", chk_logoutnotification.IsChecked);
            ConfigManager.Instance.SetValue("txt_newrequestnotification", chk_newrequestnotification.IsChecked);
            ConfigManager.Instance.SetValue("txt_newmessagenotification", chk_newmessagenotification.IsChecked);
            //ConfigManager.Instance.SetValue("txt_appnotifications", chk_appnotifications.IsChecked);

            ConfigManager.Instance.SetValue("sound_notifications", chk_soundnotifications.IsChecked);
            ConfigManager.Instance.SetValue("sound_newmessagenotification", chk_newmessagesound.IsChecked);
            //ConfigManager.Instance.SetValue("sound_appnotification", chk_appsound.IsChecked);

            ConfigManager.Instance.Save();
        }

        private void slider_defaultfontsize_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (e.NewValue == 0)
                return;

            txt_defaultfontsize.Content = String.Format("Default Font Size ({0}):", Math.Round(e.NewValue, 0));
            ConfigManager.Instance.SetValue("font_size", Math.Round(e.NewValue, 0).ToString());

            ConfigManager.Instance.Save();

            try
            {
                MainWindow.Instance.page_Chat.rtf_input.FontSize = Math.Round(e.NewValue, 0);
            }
            catch { }
        }

        private void FontsList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ConfigManager.Instance.SetValue("font", FontsList.SelectedItem.ToString());
            ConfigManager.Instance.Save();

            try
            {
                MainWindow.Instance.page_Chat.rtf_input.FontFamily = FontsList.SelectedItem as FontFamily;
            }
            catch { }
        }

        private void color_defaultfontcolor_SelectedColorChanged(object sender, RoutedPropertyChangedEventArgs<Color> e)
        {
            ConfigManager.Instance.SetValue("font_color", ToHexColor(e.NewValue));
            ConfigManager.Instance.Save();

            try
            {
                MainWindow.Instance.page_Chat.rtf_input.Foreground = new SolidColorBrush(e.NewValue);
            }
            catch { }
        }

        private void color_designcolor_SelectedColorChanged(object sender, RoutedPropertyChangedEventArgs<Color> e)
        {
            ConfigManager.Instance.SetValue("design_color", ToHexColor(e.NewValue));
            ConfigManager.Instance.Save();

            SolidColorBrush brush = MainWindow.Instance.Color;
            if (brush == null)
                brush = new SolidColorBrush(e.NewValue);

            try
            {
                ColorAnimation colorAnimation = new ColorAnimation(e.NewValue, TimeSpan.FromMilliseconds(500));
                brush.BeginAnimation(SolidColorBrush.ColorProperty, colorAnimation);

                MainWindow.Instance.Color = brush;
                MainWindow.Instance.Resources["AccentColor"] = brush;
                MainWindow.Instance.page_Login.Resources["AccentColor"] = brush;
                MainWindow.Instance.page_Contacts.Resources["AccentColor"] = brush;
                MainWindow.Instance.AddFriendWindow.Resources["AccentColor"] = brush;
                this.Resources["AccentColor"] = e.NewValue;
            }
            catch (Exception ex) { Console.WriteLine(ex.ToString()); }
        }

        private void color_designcolor2_SelectedColorChanged(object sender, RoutedPropertyChangedEventArgs<Color> e)
        {
            ConfigManager.Instance.SetValue("design_color2", ToHexColor(e.NewValue));
            ConfigManager.Instance.Save();

            SolidColorBrush brush = MainWindow.Instance.Color2;
            if (brush == null)
                brush = new SolidColorBrush(e.NewValue);

            try
            {
                ColorAnimation colorAnimation = new ColorAnimation(e.NewValue, TimeSpan.FromMilliseconds(500));
                brush.BeginAnimation(SolidColorBrush.ColorProperty, colorAnimation);

                MainWindow.Instance.Color2 = brush;
                MainWindow.Instance.Resources["AccentColor2"] = brush;
                MainWindow.Instance.page_Login.Resources["AccentColor2"] = brush;
                MainWindow.Instance.page_Contacts.Resources["AccentColor2"] = brush;
                MainWindow.Instance.AddFriendWindow.Resources["AccentColor2"] = brush;
                this.Resources["AccentColor2"] = brush;
            }
            catch (Exception ex) { Console.WriteLine(ex.ToString()); }
        }

        private string ToHexColor(Color color)
        {
            return String.Format("#{0}{1}{2}{3}",
                                 color.A.ToString("X2"),
                                 color.R.ToString("X2"),
                                 color.G.ToString("X2"),
                                 color.B.ToString("X2"));
        }

        bool FirstSet = true;

        private void FontsList_Loaded(object sender, RoutedEventArgs e)
        {
            chk_txtnotifications.IsChecked = ConfigManager.Instance.GetBool("txt_notifications", true);
            chk_loginnotification.IsChecked = ConfigManager.Instance.GetBool("txt_loginnotification", true);
            chk_logoutnotification.IsChecked = ConfigManager.Instance.GetBool("txt_logoutnotification", true);
            chk_newrequestnotification.IsChecked = ConfigManager.Instance.GetBool("txt_newrequestnotification", true);
            chk_newmessagenotification.IsChecked = ConfigManager.Instance.GetBool("txt_newmessagenotification", true);
            //chk_appnotifications.IsChecked = ConfigManager.Instance.GetBool("txt_appnotifications", true);

            chk_soundnotifications.IsChecked = ConfigManager.Instance.GetBool("sound_notifications", true);
            chk_newmessagesound.IsChecked = ConfigManager.Instance.GetBool("sound_newmessagenotification", true);
            //chk_appsound.IsChecked = ConfigManager.Instance.GetBool("sound_appnotification", true);

            int BG =  Convert.ToInt32(ConfigManager.Instance.GetString("background", "0"));
            if (BG == 4)
            {
                try
                {
                    string CustomBG = ConfigManager.Instance.GetString("custom_background");
                    ImageBrush imgBrush = new ImageBrush(new BitmapImage(new Uri(CustomBG)));
                    imgBrush.Stretch = Stretch.UniformToFill;

                    MainWindow.Instance.Background = imgBrush;
                    BackgroundsList.SelectedIndex = 4;
                }
                catch { BackgroundsList.SelectedIndex = 0; }
            }
            else
                BackgroundsList.SelectedIndex = BG;

            new Thread(new ThreadStart(delegate
                {
                    this.Dispatcher.Invoke((App.MethodInvoker)delegate
                    {
                        FontsList.ItemsSource = Fonts.SystemFontFamilies.OrderBy(i => i.ToString());
                        FontsList.SelectedItem = new FontFamily(ConfigManager.Instance.GetString("font", "Segoe WP"));
                    }, null);
                }));

            slider_defaultfontsize.ValueChanged += new RoutedPropertyChangedEventHandler<double>(slider_defaultfontsize_ValueChanged);
            slider_defaultfontsize.Value = Convert.ToDouble(ConfigManager.Instance.GetString("font_size", "12"));

            color_defaultfontcolor.SelectedColor = (Color)ColorConverter.ConvertFromString(ConfigManager.Instance.GetString("font_color", "#FF000000"));
            color_designcolor.SelectedColorChanged += new RoutedPropertyChangedEventHandler<Color>(color_designcolor_SelectedColorChanged);
            color_designcolor2.SelectedColorChanged += new RoutedPropertyChangedEventHandler<Color>(color_designcolor2_SelectedColorChanged);
            color_designcolor.SelectedColor = (Color)ColorConverter.ConvertFromString(ConfigManager.Instance.GetString("design_color", "#FF25A0DA"));
            color_designcolor2.SelectedColor = (Color)ColorConverter.ConvertFromString(ConfigManager.Instance.GetString("design_color2", "#FFDADADA"));

            color_designcolor.StandardColors.Clear();
            color_designcolor.StandardColors.Add(new Xceed.Wpf.Toolkit.ColorItem(Color.FromRgb(37, 160, 218), "Blue"));
            color_designcolor.StandardColors.Add(new Xceed.Wpf.Toolkit.ColorItem(Colors.LimeGreen, "Green"));
            color_designcolor.StandardColors.Add(new Xceed.Wpf.Toolkit.ColorItem(Colors.SlateBlue, "Purple"));
            color_designcolor.StandardColors.Add(new Xceed.Wpf.Toolkit.ColorItem(Colors.Orchid, "Pink"));
            color_designcolor.StandardColors.Add(new Xceed.Wpf.Toolkit.ColorItem(Colors.Crimson, "Red"));
            color_designcolor.StandardColors.Add(new Xceed.Wpf.Toolkit.ColorItem(Colors.LightSlateGray, "Gray"));
            color_designcolor.StandardColors.Add(new Xceed.Wpf.Toolkit.ColorItem(Color.FromRgb(218, 218, 218), "Light"));
            color_designcolor.StandardColors.Add(new Xceed.Wpf.Toolkit.ColorItem(Color.FromRgb(85, 85, 85), "Dark"));

            if (ConfigManager.Instance.GetBool("indev", false))
            {
                color_designcolor.ShowAvailableColors = true;
                color_designcolor.ShowAdvancedButton = true;
            }

            FirstSet = false;
        }

        private void BackgroundsList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ConfigManager.Instance.SetValue("background", BackgroundsList.SelectedIndex.ToString());
            ConfigManager.Instance.Save();

            string BackgroundUri = "pack://application:,,,/BlazeGames.IM.Client;component/Resources/bg_black.png";

            if (BackgroundsList.SelectedIndex == 1)
            {
                BackgroundUri = "pack://application:,,,/BlazeGames.IM.Client;component/Resources/bg_hills.jpg";
                if(!FirstSet)
                    color_designcolor2.SelectedColor = Color.FromRgb(85, 85, 85);
            }
            else if (BackgroundsList.SelectedIndex == 2)
            {
                BackgroundUri = "pack://application:,,,/BlazeGames.IM.Client;component/Resources/bg_space.jpg";
                if (!FirstSet)
                    color_designcolor2.SelectedColor = Color.FromRgb(85, 85, 85);
            }
            else if (BackgroundsList.SelectedIndex == 3)
            {
                BackgroundUri = "pack://application:,,,/BlazeGames.IM.Client;component/Resources/bg_christmas.jpg";
                if (!FirstSet)
                {
                    color_designcolor2.SelectedColor = Colors.Green;
                    color_designcolor.SelectedColor = Colors.Red;
                }
            }
            else if (BackgroundsList.SelectedIndex == 4)
            {
                if (FirstSet)
                    return;

                bg_browse_dialog.ShowDialog();
                return;
            }
            else if (BackgroundsList.SelectedIndex == 5)
            {
                BackgroundUri = "pack://application:,,,/BlazeGames.IM.Client;component/Resources/bg_weather_sunny.jpg";
            }

            ImageBrush imgBrush = new ImageBrush(new BitmapImage(new Uri(BackgroundUri)));
            imgBrush.Stretch = Stretch.UniformToFill;

            MainWindow.Instance.Background = imgBrush;
        }
    }
}
