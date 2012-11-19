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
    internal partial class page_home : UserControl
    {
        public ListCollectionView Appview;

        public page_home()
        {
            InitializeComponent();

            Appview = new ListCollectionView(Plugins.PluginsManager.Instance.Plugins);
            InstalledApps.Items.Add("Test");
            InstalledApps.Items.Add("Test2");
            InstalledApps.Items.Add("Test3");
        }
    }
}
