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

namespace BlazeGames.IM.Client
{
    /// <summary>
    /// Interaction logic for page_home.xaml
    /// </summary>
    internal partial class page_contacts : UserControl
    {
        public page_contacts()
        {
            InitializeComponent();
        }

        private int LastControlsPerRow = 0;

        public void Draw(bool Resize=false)
        {
            try
            {
                App.Instance.Dispatcher.Invoke((App.MethodInvoker)delegate
                {
                    List<Control_Contact> controls_to_remove = new List<Control_Contact>();
                    foreach (UIElement element in page_canvas.Children)
                        controls_to_remove.Add((Control_Contact)element);

                    if (App.Instance.Contacts.Count > 0)
                    {
                        int ControlsPerRow = 0;
                        int Tmp_i = 0;
                        while (true)
                        {
                            if(Tmp_i + 257 > this.ActualWidth)
                                break;

                            Tmp_i += 257;
                            ControlsPerRow++;
                        }

                        if (Resize && ControlsPerRow == LastControlsPerRow)
                            return;

                        LastControlsPerRow = ControlsPerRow;

                        int ContactCount = 0;
                        Contact LastContact = null;
                        int i = 0;
                        int j = 0;

                        Dictionary<int, Contact> sorted_friends = (from entry in App.Instance.Contacts orderby entry.Value.NickName ascending select entry).ToDictionary(pair => pair.Key, pair => pair.Value);

                        foreach (Contact contact in sorted_friends.Values)
                        {
                            if (contact.status == Status.Online)
                            {
                                if (MainWindow.Instance.txt_search.Text != "" && !contact.NickName.ToLower().Contains(MainWindow.Instance.txt_search.Text.ToLower()) && !contact.StatusUpdate.ToLower().Contains(MainWindow.Instance.txt_search.Text.ToLower()))
                                {
                                    contact.control.Visibility = System.Windows.Visibility.Collapsed;
                                    controls_to_remove.Remove(contact.control);
                                    continue;
                                }
                                ContactCount++;
                                LastContact = contact;

                                if (i == ControlsPerRow)
                                {
                                    j++;
                                    i = 0;
                                }

                                if (contact.control == null)
                                {
                                    Control_Contact contactControl = new Control_Contact(contact);
                                    contactControl.Height = 70;
                                    contactControl.Width = 257;

                                    contact.control = contactControl;

                                    page_canvas.Children.Add(contactControl);
                                }
                                else if (contact.control.Visibility == System.Windows.Visibility.Collapsed)
                                {
                                    contact.control.Visibility = System.Windows.Visibility.Visible;
                                    controls_to_remove.Remove(contact.control);
                                }
                                else
                                    controls_to_remove.Remove(contact.control);

                                DynamicCanvas.SetLeft(contact.control, 257 * i);
                                DynamicCanvas.SetTop(contact.control, 70 * j + 10);
                                contact.control.profile_image.BorderBrush = (SolidColorBrush)new BrushConverter().ConvertFromString(contact.status.GetColor());

                                i++;
                            }
                        }
                        foreach (Contact contact in sorted_friends.Values)
                        {
                            if (contact.status == Status.Busy)
                            {
                                if (MainWindow.Instance.txt_search.Text != "" && !contact.NickName.ToLower().Contains(MainWindow.Instance.txt_search.Text.ToLower()) && !contact.StatusUpdate.ToLower().Contains(MainWindow.Instance.txt_search.Text.ToLower()))
                                {
                                    contact.control.Visibility = System.Windows.Visibility.Collapsed;
                                    controls_to_remove.Remove(contact.control);
                                    continue;
                                }
                                ContactCount++;
                                LastContact = contact;

                                if (i == ControlsPerRow)
                                {
                                    j++;
                                    i = 0;
                                }

                                if (contact.control == null)
                                {
                                    Control_Contact contactControl = new Control_Contact(contact);
                                    contactControl.Height = 70;
                                    contactControl.Width = 257;

                                    contact.control = contactControl;

                                    page_canvas.Children.Add(contactControl);
                                }
                                else if (contact.control.Visibility == System.Windows.Visibility.Collapsed)
                                {
                                    contact.control.Visibility = System.Windows.Visibility.Visible;
                                    controls_to_remove.Remove(contact.control);
                                }
                                else
                                    controls_to_remove.Remove(contact.control);

                                DynamicCanvas.SetLeft(contact.control, 257 * i);
                                DynamicCanvas.SetTop(contact.control, 70 * j + 10);
                                contact.control.profile_image.BorderBrush = (SolidColorBrush)new BrushConverter().ConvertFromString(contact.status.GetColor());

                                i++;
                            }
                        }
                        foreach (Contact contact in sorted_friends.Values)
                        { 
                            if (contact.status == Status.Away || contact.status == Status.Afk)
                            {
                                if (MainWindow.Instance.txt_search.Text != "" && !contact.NickName.ToLower().Contains(MainWindow.Instance.txt_search.Text.ToLower()) && !contact.StatusUpdate.ToLower().Contains(MainWindow.Instance.txt_search.Text.ToLower()))
                                {
                                    contact.control.Visibility = System.Windows.Visibility.Collapsed;
                                    controls_to_remove.Remove(contact.control);
                                    continue;
                                }
                                ContactCount++;
                                LastContact = contact;

                                if (i == ControlsPerRow)
                                {
                                    j++;
                                    i = 0;
                                }

                                if (contact.control == null)
                                {
                                    Control_Contact contactControl = new Control_Contact(contact);
                                    contactControl.Height = 70;
                                    contactControl.Width = 257;

                                    contact.control = contactControl;

                                    page_canvas.Children.Add(contactControl);
                                }
                                else if (contact.control.Visibility == System.Windows.Visibility.Collapsed)
                                {
                                    contact.control.Visibility = System.Windows.Visibility.Visible;
                                    controls_to_remove.Remove(contact.control);
                                }
                                else
                                    controls_to_remove.Remove(contact.control);

                                DynamicCanvas.SetLeft(contact.control, 257 * i);
                                DynamicCanvas.SetTop(contact.control, 70 * j + 10);
                                contact.control.profile_image.BorderBrush = (SolidColorBrush)new BrushConverter().ConvertFromString(contact.status.GetColor());

                                i++;
                            }
                        }
                        foreach (Contact contact in sorted_friends.Values)
                        {
                            if (contact.status == Status.Offline)
                            {
                                if (MainWindow.Instance.txt_search.Text != "" && !contact.NickName.ToLower().Contains(MainWindow.Instance.txt_search.Text.ToLower()) && !contact.StatusUpdate.ToLower().Contains(MainWindow.Instance.txt_search.Text.ToLower()))
                                {
                                    contact.control.Visibility = System.Windows.Visibility.Collapsed;
                                    controls_to_remove.Remove(contact.control);
                                    continue;
                                }
                                ContactCount++;
                                LastContact = contact;

                                if (i == ControlsPerRow)
                                {
                                    j++;
                                    i = 0;
                                }

                                if (contact.control == null)
                                {
                                    Control_Contact contactControl = new Control_Contact(contact);
                                    contactControl.Height = 70;
                                    contactControl.Width = 257;

                                    contact.control = contactControl;

                                    page_canvas.Children.Add(contactControl);
                                }
                                else if (contact.control.Visibility == System.Windows.Visibility.Collapsed)
                                {
                                    contact.control.Visibility = System.Windows.Visibility.Visible;
                                    controls_to_remove.Remove(contact.control);
                                }
                                else
                                    controls_to_remove.Remove(contact.control);

                                DynamicCanvas.SetLeft(contact.control, 257 * i);
                                DynamicCanvas.SetTop(contact.control, 70 * j + 10);
                                contact.control.profile_image.BorderBrush = (SolidColorBrush)new BrushConverter().ConvertFromString(contact.status.GetColor());

                                i++;
                            }
                        }

                        if (ContactCount == 1 && MainWindow.Instance.txt_search.Text != "" && MainWindow.Instance.txt_search.Text.ToLower() == LastContact.NickName.ToLower())
                        {
                            if (!LastContact.Pending)
                            {
                                MainWindow.Instance.txt_search.Text = "";

                                if (MainWindow.Instance.page_Chat.ChattingWith != LastContact || MainWindow.Instance.CurrentPage != "chat")
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

                                MainWindow.Instance.page_Chat.StartChattingWith(LastContact);
                                if (!App.Instance.OpenChats.Contains(LastContact))
                                    App.Instance.OpenChats.Add(LastContact);
                                LastContact.MarkAllMessagesRead();

                                MainWindow.Instance.CurrentPage = "chat";

                                MainWindow.Instance.page_Chat.rtf_input.Focusable = true;
                                FocusManager.SetFocusedElement(MainWindow.Instance.page_Chat, MainWindow.Instance.page_Chat.rtf_input);
                                Keyboard.Focus(MainWindow.Instance.page_Chat.rtf_input);
                            }
                        }
                    }

                    foreach (Control_Contact control in controls_to_remove)
                        page_canvas.Children.Remove(control);

                    SlideFade.StartAnimationIn(this);

                    
                }, null);
            }
            catch { }
        }

        private void UserControl_Loaded_1(object sender, RoutedEventArgs e)
        {
            Draw();
        }

        private void UserControl_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            Draw(true);
        }
    }
}
