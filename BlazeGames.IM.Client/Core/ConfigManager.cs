using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace BlazeGames.IM.Client
{
    internal class ConfigManager
    {
        #region Singleton
        private static ConfigManager _Instance;

        public static ConfigManager Instance
        {
            get
            {
                if (_Instance == null)
                    _Instance = new ConfigManager();

                return _Instance;
            }
        }
        #endregion

        Dictionary<string, string> ConfigData = new Dictionary<string, string>();

        private ConfigManager()
        {
            FileInfo fi = new FileInfo(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "BlazeGamesIM", "config.ini"));
            if (fi.Exists)
            {
                string[] FileData = File.ReadAllLines(fi.FullName);
                if (FileData.Length > 0)
                {
                    if (FileData[0] != "config_ver=2")
                    {
                        File.WriteAllText(fi.FullName, DefaultConfig);
                        FileData = File.ReadAllLines(fi.FullName);
                    }
                }
                else
                {
                    File.WriteAllText(fi.FullName, DefaultConfig);
                    FileData = File.ReadAllLines(fi.FullName);
                }

                foreach (string ConfigLine in FileData)
                {
                    if (ConfigLine.Trim() == "")
                        continue;
                    else if (ConfigLine.Split('=').Length < 2)
                        continue;
                    else
                    {
                        string[] ConfigLineData = ConfigLine.Split('=');
                        ConfigData.Add(ConfigLineData[0], String.Join("=", ConfigLineData, 1, ConfigLineData.Length - 1));
                    }
                }
            }
        }

        public void Save()
        {
            string TmpConfigFile = "";
            foreach (string ConfigDataKey in ConfigData.Keys)
            {
                TmpConfigFile += ConfigDataKey + "=" + ConfigData[ConfigDataKey] + "\r\n";
            }

            File.WriteAllText(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "BlazeGamesIM", "config.ini"), TmpConfigFile);
        }

        public string GetString(string Key, string DefaultValue="")
        {
            if (ConfigData.ContainsKey(Key))
                return ConfigData[Key];
            else
            {
                ConfigData.Add(Key, DefaultValue);
                return DefaultValue;
            }
        }

        public bool GetBool(string Key, bool DefaultValue = false)
        {
            if (ConfigData.ContainsKey(Key))
                return Convert.ToBoolean(ConfigData[Key]);
            else
            {
                ConfigData.Add(Key, Convert.ToString(DefaultValue));
                return DefaultValue;
            }
        }

        public bool GetBool(string Key, bool? DefaultValue = false)
        {
            if (ConfigData.ContainsKey(Key))
                return Convert.ToBoolean(ConfigData[Key]);
            else
            {
                ConfigData.Add(Key, Convert.ToString(DefaultValue));
                return (bool)DefaultValue;
            }
        }

        public void SetValue(string Key, string Value)
        {
            if (ConfigData.ContainsKey(Key))
                ConfigData[Key] = Value;
            else
                ConfigData.Add(Key, Value);
        }

        public void SetValue(string Key, bool Value)
        {
            if (ConfigData.ContainsKey(Key))
                ConfigData[Key] = Convert.ToString(Value);
            else
                ConfigData.Add(Key, Convert.ToString(Value));
        }

        public void SetValue(string Key, bool? Value)
        {
            if (ConfigData.ContainsKey(Key))
                ConfigData[Key] = Convert.ToString(Value);
            else
                ConfigData.Add(Key, Convert.ToString(Value));
        }

        private string DefaultConfig = @"config_ver=2

txt_notifications=true
txt_loginnotification=true
txt_logoutnotification=true
txt_newrequestnotification=true
txt_newmessagenotification=true
txt_appnotifications=true

sound_notifications=true
sound_newmessagenotification=true
sound_appnotification=true

autologin=false
autologin_account=
autologin_password=

font=Segoe WP
font_size=12
font_color=#000000";
    }
}
