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
using System.IO;

namespace BlazeGames.IM.Client
{
    /// <summary>
    /// Interaction logic for ConsoleWindow.xaml
    /// </summary>
    internal partial class ConsoleWindow : Window
    {
        public static ConsoleWindow Instance = null;

        public ConsoleWindow()
        {
            Instance = this;
            InitializeComponent();
        }

        private void Window_Closing_1(object sender, System.ComponentModel.CancelEventArgs e)
        {
            try
            {
                MainWindow.Instance.Close();
            }
            catch { }
        }
    }

    internal class ConsoleLog : TextWriter
    {
        private TextWriter _writer;

        public ConsoleLog(TextWriter writer)
        {
            _writer = writer;
        }

        public override void Write(String s)
        {
            _writer.Write(s);
            ConsoleWindow.Instance.Dispatcher.Invoke((App.MethodInvoker)delegate
            {
                ConsoleWindow.Instance.txt_console.Text += s;
            }, null);
        }

        public override void WriteLine(string value)
        {
            _writer.Write(value);
            ConsoleWindow.Instance.Dispatcher.Invoke((App.MethodInvoker)delegate
            {
                ConsoleWindow.Instance.txt_console.Text += value + Environment.NewLine;
            }, null);
        }

        public override Encoding Encoding
        {
            get
            {
                return
                    Encoding.Default;
            }
        }

    }
}
