using Moonbyte.UniversalServer.PluginFramework;
using System;
using System.IO;

namespace UniversalTestPlugin
{
    public class UniversalTestPlugin : UniversalPluginFramework
    {

        #region Vars

        ClientContext Controller;

        string PluginsSettingDirectory;

        #endregion

        #region Plugin Name

        public string Name
        {
            get { return "UniversalTestPlugin"; }
        }

        #endregion Plugin Name

        #region Invoke

        public void Invoke(string[] RawCommand)
        {
            Console.WriteLine("TEST");
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
