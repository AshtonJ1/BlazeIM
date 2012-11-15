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
using System.IO;
using System.Text.RegularExpressions;
using System.Windows.Media.Animation;
using System.Threading;
using System.Diagnostics;

namespace BlazeGames.IM.Client
{
    /// <summary>
    /// Interaction logic for page_home.xaml
    /// </summary>
    internal partial class page_chat : UserControl
    {
        public Contact ChattingWith = null;

        public void StartChattingWith(Contact contact)
        {
            if(ChattingWith != null)
            if (ChattingWith.ID == contact.ID)
                return;

            new Thread(new ThreadStart(delegate
                {
                    this.Dispatcher.Invoke((App.MethodInvoker)delegate
                    {
                        ChattingWith = contact;

                        LastMessageFrom = "";

                        profile_image_source.ImageSource = new System.Windows.Media.Imaging.BitmapImage(new Uri(contact.ProfileImage));
                        profile_image.BorderBrush = (SolidColorBrush)new BrushConverter().ConvertFromString(contact.status.GetColor());
                        txt_chattingwith_nickname.Text = contact.NickName;
                        txt_chattingwith_statusupdate.Text = contact.StatusUpdate;

                        rtf_output.SelectAll();
                        rtf_output.Selection.Text = "";

                    }, null);

                    this.Dispatcher.Invoke((App.MethodInvoker)delegate
                    {
                        SlideFade.CreateProfileImgAnimationOut(profile_image);
                        App.Instance.Contacts[contact.ID].LastMessage = DateTime.Now;
                    }, null);

                    foreach (Message msg in contact.Messages)
                    {
                        this.Dispatcher.Invoke((App.MethodInvoker)delegate
                        {
                            HandleMessage(msg.From, msg.Msg);
                        }, null);
                    }
                })).Start();
        }

        public page_chat()
        {
            InitializeComponent();

            RoutedCommand Input_SendMessage = new RoutedCommand();
            RoutedCommand Input_Return = new RoutedCommand();

            KeyBinding Input_SendMessage_Keybinding = new KeyBinding(Input_SendMessage, new KeyGesture(Key.Enter));
            CommandBinding Input_SendMessage_Binding = new CommandBinding(Input_SendMessage, Input_SentMessage_Execute, CmdCanExecute);

            KeyBinding Input_Return_Keybinding = new KeyBinding(Input_Return, new KeyGesture(Key.Enter, ModifierKeys.Control));
            CommandBinding Input_Return_Binding = new CommandBinding(Input_Return, Input_Return_Execute, CmdCanExecute);

            this.rtf_input.InputBindings.Add(Input_SendMessage_Keybinding);
            this.rtf_input.CommandBindings.Add(Input_SendMessage_Binding);

            this.rtf_input.InputBindings.Add(Input_Return_Keybinding);
            this.rtf_input.CommandBindings.Add(Input_Return_Binding);

            try
            {
                this.rtf_input.FontSize = Convert.ToDouble(ConfigManager.Instance.GetString("font_size", "12"));
                this.rtf_input.FontFamily = new FontFamily(ConfigManager.Instance.GetString("font", "Segoe WP"));
                this.rtf_input.Foreground = (SolidColorBrush)new BrushConverter().ConvertFromString(ConfigManager.Instance.GetString("font_color", "#000000"));
            }
            catch { }
        }

        string LastMessageFrom = "";
        public void HandleMessage(string From, string Message)
        {
            try
            {
                if (LastMessageFrom != From)
                {
                    string append = "";
                    if (LastMessageFrom != "")
                        append = "\r";

                    rtf_output.Selection.Select(rtf_output.Document.ContentEnd, rtf_output.Document.ContentEnd);
                    rtf_output.Selection.Load(new MemoryStream(Encoding.Default.GetBytes(string.Format("{1}{0} says\r", From, append))), DataFormats.Text);
                    rtf_output.Selection.ApplyPropertyValue(TextElement.ForegroundProperty, new SolidColorBrush(Color.FromRgb(128, 128, 128)));
                    rtf_output.Selection.ApplyPropertyValue(TextElement.FontSizeProperty, 12.00);
                    LastMessageFrom = From;
                }

                rtf_output.Selection.Select(rtf_output.Document.ContentEnd, rtf_output.Document.ContentEnd);
                rtf_output.Selection.Load(new MemoryStream(Encoding.Default.GetBytes(Message.Replace("xmlns=\"default\"", "xmlns=\"http://schemas.microsoft.com/winfx/2006/xaml/presentation\""))), DataFormats.Xaml);
                rtf_output.ScrollToEnd();
                SubscribeToAllHyperlinks(rtf_output.Document);
            }
            catch (Exception ex)
            {
                this.Dispatcher.Invoke((App.MethodInvoker)delegate
                {
                    MessageBoxResult result = MessageBox.Show(string.Format("Blaze IM has encountered an exception but was able to recover. Would you like to report this issue to us so we can fix it?\r\n\r\nError Details:\r\n{0}", ex.Message), "Blaze IM Error", MessageBoxButton.YesNo, MessageBoxImage.Error);

                    if (result == MessageBoxResult.Yes)
                    {
                        BlazeGames.IM.Client.Core.Utilities.SubmitBug(ex);
                    }
                }, null);
            }
        }

        void Input_SentMessage_Execute(object target, ExecutedRoutedEventArgs e)
        {
            UploadAllImages();

            rtf_input.SelectAll();
            if (rtf_input.Selection.Text.Trim() == "")
            {
                return;
            }

            MemoryStream ms = new MemoryStream();
            
            rtf_input.Selection.Save(ms, DataFormats.Xaml);
            rtf_input.Selection.Text = "";

            string Message = Encoding.Default.GetString(ms.ToArray()).Replace("Typography.StandardLigatures=\"True\" Typography.ContextualLigatures=\"True\" Typography.DiscretionaryLigatures=\"False\" Typography.HistoricalLigatures=\"False\" Typography.AnnotationAlternates=\"0\" Typography.ContextualAlternates=\"True\" Typography.HistoricalForms=\"False\" Typography.Kerning=\"True\" Typography.CapitalSpacing=\"False\" Typography.CaseSensitiveForms=\"False\" Typography.StylisticSet1=\"False\" Typography.StylisticSet2=\"False\" Typography.StylisticSet3=\"False\" Typography.StylisticSet4=\"False\" Typography.StylisticSet5=\"False\" Typography.StylisticSet6=\"False\" Typography.StylisticSet7=\"False\" Typography.StylisticSet8=\"False\" Typography.StylisticSet9=\"False\" Typography.StylisticSet10=\"False\" Typography.StylisticSet11=\"False\" Typography.StylisticSet12=\"False\" Typography.StylisticSet13=\"False\" Typography.StylisticSet14=\"False\" Typography.StylisticSet15=\"False\" Typography.StylisticSet16=\"False\" Typography.StylisticSet17=\"False\" Typography.StylisticSet18=\"False\" Typography.StylisticSet19=\"False\" Typography.StylisticSet20=\"False\" Typography.Fraction=\"Normal\" Typography.SlashedZero=\"False\" Typography.MathematicalGreek=\"False\" Typography.EastAsianExpertForms=\"False\" Typography.Variants=\"Normal\" Typography.Capitals=\"Normal\" Typography.NumeralStyle=\"Normal\" Typography.NumeralAlignment=\"Normal\" Typography.EastAsianWidths=\"Normal\" Typography.EastAsianLanguage=\"Normal\" Typography.StandardSwashes=\"0\" Typography.ContextualSwashes=\"0\" Typography.StylisticAlternates=\"0\"", "").Replace("xmlns=\"http://schemas.microsoft.com/winfx/2006/xaml/presentation\"", "xmlns=\"default\"");

            Message = Regex.Replace(Message, "<Paragraph>", "", RegexOptions.IgnoreCase);
            Message = Regex.Replace(Message, "</Paragraph>", "\r", RegexOptions.IgnoreCase);
            Message = Regex.Replace(Message, "<Paragraph(.|\n)*?>", "", RegexOptions.IgnoreCase);
            Message = Regex.Replace(Message, "<Section", "<Span", RegexOptions.IgnoreCase);
            Message = Regex.Replace(Message, "</Section>", "</Span>", RegexOptions.IgnoreCase);
            Message = Regex.Replace(Message, "TextAlignment=\"(.|\n)*?\"", "", RegexOptions.IgnoreCase);
            Message = Regex.Replace(Message, "LineHeight=\"(.|\n)*?\"", "", RegexOptions.IgnoreCase);
            Message = Regex.Replace(Message, "IsHyphenationEnabled=\"(.|\n)*?\"", "", RegexOptions.IgnoreCase);
            Message = Message.Replace("<Run", "<Span").Replace("</Run>", "</Span>");

            Regex urlRx = new Regex(@"(?i)\b((?:https?://|www\d{0,3}[.]|[a-z0-9.\-]+[.][a-z]{2,4}/)(?:[^\s()<>]+|\(([^\s()<>]+|(\([^\s()<>]+\)))*\))+(?:\(([^\s()<>]+|(\([^\s()<>]+\)))*\)|[^\s`!()\[\]{};:'"".,<>?«»“”‘’]))", RegexOptions.IgnoreCase);

            MatchCollection matches = urlRx.Matches(Message);

            foreach (Match match in matches)
            {
                var url = match.Groups[0].Value;
                Message = Message.Replace(url, string.Format("<Hyperlink NavigateUri=\"{0}\">{0}</Hyperlink>", url));
            }

            if (ChattingWith != null)
            {
                if (App.Instance.Contacts.ContainsKey(ChattingWith.ID))
                {
                    App.Instance.Contacts[ChattingWith.ID].SendMessage(Message);
                    HandleMessage(App.NickName, Message);
                }
                else
                    NotificationWindow.ShowNotification("Unable To Deliver Message", string.Format("{0} is no longer in your contact list.", ChattingWith.NickName));
            }
        }

        void UploadAllImages()
        {
            var images = GetVisuals(rtf_input.Document).OfType<Image>();
            foreach (var image in images)
                try
                {
                    //TODO: Upload image and replace it with a link
                }
                catch { }
        }

        void SubscribeToAllHyperlinks(FlowDocument flowDocument)
        {
            var hyperlinks = GetVisuals(flowDocument).OfType<Hyperlink>();
            foreach (var link in hyperlinks)
                try
                {
                    link.RequestNavigate += new System.Windows.Navigation.RequestNavigateEventHandler(link_RequestNavigate);
                }
                catch { }
        }

        void link_RequestNavigate(object sender, System.Windows.Navigation.RequestNavigateEventArgs e)
        {
            System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo("https://blaze-games.com/LinkOut/?Url=" + System.Net.WebUtility.HtmlEncode(e.Uri.AbsoluteUri)));
            e.Handled = true;
        }

        public static IEnumerable<DependencyObject> GetVisuals(DependencyObject root)
        {
            foreach (var child in LogicalTreeHelper.GetChildren(root).OfType<DependencyObject>())
            {
                yield return child;
                foreach (var descendants in GetVisuals(child))
                    yield return descendants;
            }
        }

        void hlink_RequestNavigate(object sender, System.Windows.Navigation.RequestNavigateEventArgs e)
        {
            System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo(e.Uri.AbsoluteUri));
            e.Handled = true;
        }

        void Input_Return_Execute(object target, ExecutedRoutedEventArgs e)
        {
            rtf_input.Selection.Load(new MemoryStream(Encoding.Default.GetBytes("\r")), DataFormats.Text);
            rtf_input.Selection.Select(rtf_input.Selection.Start.GetNextInsertionPosition(LogicalDirection.Forward), rtf_input.Selection.Start.GetNextInsertionPosition(LogicalDirection.Forward));
        }

        void CmdCanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = true;
        }

        private void rtf_output_TextChanged(object sender, TextChangedEventArgs e)
        {
            //MemoryStream ms = new MemoryStream();
            //rtf_output.Selection.Select(rtf_output.Document.ContentStart, rtf_output.Document.ContentEnd);
            //rtf_output.Selection.Save(ms, DataFormats.Rtf, true);
            //rtf_output.Selection.Select(rtf_output.Document.ContentEnd, rtf_output.Document.ContentEnd);
            
        }

        private void rtf_input_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter || e.Key == Key.Return)
            {
                e.Handled = true;
            }
        }

        private void rtf_input_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter || e.Key == Key.Return)
            {
                e.Handled = true;
            }
        }

        private void rtf_output_MouseEnter(object sender, MouseEventArgs e)
        {
            SlideFade.CreateProfileImgAnimationIn(profile_image);
        }

        private void rtf_output_MouseLeave(object sender, MouseEventArgs e)
        {
            SlideFade.CreateProfileImgAnimationOut(profile_image);
        }

        private void profile_image_MouseEnter(object sender, MouseEventArgs e)
        {
            SlideFade.CancelProfileImgAnimation1();
            
        }

        private void profile_image_MouseLeave(object sender, MouseEventArgs e)
        {
            SlideFade.CancelProfileImgAnimation2();
            
        }
    }
}
