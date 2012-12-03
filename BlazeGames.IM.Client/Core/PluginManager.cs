using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Controls;
using System.Windows;
using System.Reflection;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;

namespace BlazeGames.IM.Plugins
{
    internal class PluginsManager
    {
        #region Singleton
        private static PluginsManager _Instance;

        private PluginsManager() { }

        public static PluginsManager Instance
        {
            get
            {
                if (_Instance == null)
                    _Instance = new PluginsManager();

                return _Instance;
            }
        }
        #endregion

        public List<object[]> Plugins = new List<object[]>();

        public void LoadPluginsFromFolder()
        {
            DirectoryInfo di = new DirectoryInfo(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "BlazeGamesIM", "Plugins"));
            if(!di.Exists)
                di.Create();

            foreach (FileInfo fi in di.GetFiles("*.dll"))
            {
                LoadPlugin(fi.FullName, fi.Name);
            }
        }

        public void LoadPlugin(string FilePath, string AssemblyName)
        {
            Assembly asm = Assembly.LoadFile(FilePath);
            Type type = asm.GetType("BlazeGames.IM.TestPlugin.IMPlugin");

            PluginActions actions = new PluginActions();

            object IMPlugin = Activator.CreateInstance(type, actions);
            Plugins.Add(new object[] { IMPlugin, actions });

            MethodInfo InitalizeMethod = IMPlugin.GetType().GetMethod("PluginLoaded");
            InitalizeMethod.Invoke(IMPlugin, null);
        }
    }

    public class PluginActions
    {
        List<Control> Pages = new List<Control>();

        public bool AddPage(Control page)
        {
            if (page != null)
            {
                page.HorizontalAlignment = HorizontalAlignment.Left;
                page.VerticalAlignment = VerticalAlignment.Top;
                page.Visibility = Visibility.Hidden;
                Grid.SetColumn(page, 1);
                Grid.SetRow(page, 2);

                BlazeGames.IM.Client.MainWindow.Instance.grid.Children.Add(page);

                Pages.Add(page);
                return true;
            }
            else
                return false;
        }

        public bool AddControl(Control control)
        {
            return false;
        }

        public bool RequestHomePage()
        {
            return false;
        }
    }

    public interface IPlugin
    {
        /*App Information*/
        string Name { get; }
        double Version { get; }
        string Author { get; }
        string Company { get; }
        string Description { get; }
        bool ShowIconOnHomescreen { get; }

        void PluginLoaded();
        void PluginUnloaded();
    }
}
