using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using BlazeGames.Networking;

namespace BlazeGames.IM.Client.Web
{
    internal partial class App : Application
    {
        public delegate void MethodInvoker();

        public static App Instance = null;

        internal ClientSocket CSocket = null;
        public MainPage mainPage;

        public App()
        {
            if (Instance == null)
                Instance = this;

            this.Startup += this.Application_Startup;
            this.Exit += this.Application_Exit;
            this.UnhandledException += this.Application_UnhandledException;

            InitializeComponent();
        }

        private void Application_Startup(object sender, StartupEventArgs e)
        {
            mainPage = new MainPage();

            CSocket = new ClientSocket(IPAddress.Parse("209.141.53.112"), 25050);
            CSocket.ClientSocketConnected_Event += new EventHandler(CSocket_ClientSocketConnected_Event);
            CSocket.ClientSocketDisconnected_Event += new EventHandler(CSocket_ClientSocketDisconnected_Event);
            CSocket.ClientSocketPacketReceived_Event += new ClientSocketPacketReceived_Handler(CSocket_ClientSocketPacketReceived_Event);
            CSocket.Connect();

            this.RootVisual = mainPage;
        }

        void CSocket_ClientSocketConnected_Event(object sender, EventArgs e)
        {
            mainPage.Dispatcher.BeginInvoke((MethodInvoker)delegate
            {
                MessageBox.Show("Connected");
            }, null);
        }

        void CSocket_ClientSocketDisconnected_Event(object sender, EventArgs e)
        {
            mainPage.Dispatcher.BeginInvoke((MethodInvoker)delegate
            {
                MessageBox.Show("Disconnected");
            }, null);
        }

        void CSocket_ClientSocketPacketReceived_Event(object sender, ClientSocket clientSocket, Packet pak)
        {
            mainPage.Dispatcher.BeginInvoke((MethodInvoker)delegate
            {
                MessageBox.Show("Received");
            }, null);   
        }

        private void Application_Exit(object sender, EventArgs e)
        {
            CSocket.Disconnect();
            CSocket.Dispose();
        }

        private void Application_UnhandledException(object sender, ApplicationUnhandledExceptionEventArgs e)
        {
            // If the app is running outside of the debugger then report the exception using
            // the browser's exception mechanism. On IE this will display it a yellow alert 
            // icon in the status bar and Firefox will display a script error.
            if (!System.Diagnostics.Debugger.IsAttached)
            {

                // NOTE: This will allow the application to continue running after an exception has been thrown
                // but not handled. 
                // For production applications this error handling should be replaced with something that will 
                // report the error to the website and stop the application.
                e.Handled = true;
                Deployment.Current.Dispatcher.BeginInvoke(delegate { ReportErrorToDOM(e); });
            }
        }

        private void ReportErrorToDOM(ApplicationUnhandledExceptionEventArgs e)
        {
            try
            {
                string errorMsg = e.ExceptionObject.Message + e.ExceptionObject.StackTrace;
                errorMsg = errorMsg.Replace('"', '\'').Replace("\r\n", @"\n");

                System.Windows.Browser.HtmlPage.Window.Eval("alert(\"" + e.ExceptionObject.ToString() + "\");");
                System.Windows.Browser.HtmlPage.Window.Eval("throw new Error(\"Unhandled Error in Silverlight Application " + errorMsg + "\");");
            }
            catch (Exception)
            {
            }
        }
    }
}
