using Moonbyte.UniversalServer.PluginFramework;
using System;
using System.IO;

namespace UniversalTestPlugin
{
    public class UniversalTestPlugin : UniversalPluginFramework
    {

        #region Vars

        string PluginsSettingDirectory;

        #endregion

        #region Plugin Name

        public string Name
        {
            get { return "UniversalTestPlugin"; }
        }

        #endregion Plugin Name

        #region Invoke

        public void Invoke(ClientContext Client, string[] RawCommand)
        {
            Client.Log("INFO", "LOL");
        }

        #endregion Invoke

        #region OnLoad

        public void OnLoad(string PluginSettingsDirectory)
        {
            PluginsSettingDirectory = PluginSettingsDirectory;

            if (!Directory.Exists(PluginSettingsDirectory)) Directory.CreateDirectory(PluginSettingsDirectory);
            Console.WriteLine("UniversalTestPlugin Loaded!");
        }

        #endregion OnLoad

    }
}
