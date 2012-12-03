using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BlazeGames.IM.Plugins;

namespace BlazeGames.IM.Plugins.WeatherPlugin
{
    public class MainPlugin : IPlugin
    {
        private PluginActions actions;

        public MainPlugin(PluginActions pluginActions)
        {
            actions = pluginActions;
        }

        public string Name { get { return "Blaze Games Weather Plugin"; } }
        public double Version { get { return 0.1; } }
        public string Author { get { return "Blaze Games"; } }
        public string Company { get { return "Blaze Games"; } }
        public string Description { get { return "Adds a weather widget to the home screen."; } }
        public bool ShowIconOnHomescreen { get { return false; } }

        public void PluginLoaded()
        {

        }

        public void PluginUnloaded()
        {

        }
    }
}
