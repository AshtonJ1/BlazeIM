using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BlazeGames.IM.Plugins;
using System.Windows;

namespace BlazeGames.IM.TestPlugin
{
    public class IMPlugin : IPlugin
    {
        private PluginActions actions;

        public IMPlugin(PluginActions pluginActions)
        {
            MessageBox.Show("Plugin Created");
            actions = pluginActions;
            actions.AddPage(new page_test());
        }

        public string Name { get { return "Test Plugin"; } }
        public double Version { get { return 0.01; } }
        public string Author { get { return "Ashton Storks"; } }
        public string Company { get { return "Blaze Games"; } }
        public string Description { get { return "Blaze Games IM Test Plugin"; } }
        public string ImageUrl { get { return "http://blaze-games.com/api/image/nocompress/?nickname=Ashton Storks"; } }

        public void PluginLoaded()
        {
            if(actions.RequestHomePage())
                MessageBox.Show("Plugin Loaded");  

            
        }

        public void PluginUnloaded()
        {
            MessageBox.Show("Plugin Unloaded");
        }
    }
}
