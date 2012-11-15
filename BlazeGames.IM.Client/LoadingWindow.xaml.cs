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
using System.Windows.Shapes;

namespace BlazeGames.IM.Client
{
    /// <summary>
    /// Interaction logic for LoadingWindow.xaml
    /// </summary>
    internal partial class LoadingWindow : Window
    {
        public static LoadingWindow Instance { get; private set; }

        static LoadingWindow()
        {
            try
            {
                Instance = new LoadingWindow();
            }
            catch (Exception ex) { MessageBox.Show(ex.ToString()); }
        }

        public LoadingWindow()
        {
            InitializeComponent();
        }
    }
}
