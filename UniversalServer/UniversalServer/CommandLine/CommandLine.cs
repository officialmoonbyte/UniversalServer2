using System.Collections.Generic;
using System;
using GlobalSettingsFramework;
using Moonbyte.Net.TcpServer;
using System.Threading;
using System.Linq;
using System.IO;
using System.Reflection;
using Moonbyte.Logging;
using Moonbyte.UniversalClient;
using System.Net;
using System.Diagnostics;
using Moonbyte.UniversalServer.PluginFramework;
using System.Net.Sockets;

namespace UniversalServer.CommandLine
{
    public class CommandLine
    {

        #region Vars

        List<UniversalServerObject> servers = new List<UniversalServerObject>();
        GFS settingsFramework;

        UniversalClient MoonbyteServerConnection;

        string seperator = "|%40%|";
        string _servers = "SERVERS";

        #endregion Vars

        #region Initialization

        public CommandLine()
        {
            settingsFramework = new GFS(Environment.CurrentDirectory + @"\" + "UniversalSettings.dll");
            ILogger.SetLoggingEvents();

            //CheckForSystemUpdate();

            string oldSettingValue = settingsFramework.ReadSetting(_servers);
            if (oldSettingValue != null)
            {
                List<string> _Servers = oldSettingValue.Split(new string[] { seperator }, StringSplitOptions.RemoveEmptyEntries).ToList();

                foreach (string s in _Servers)
                {
                    UniversalServerObject USO = new UniversalServerObject(s, Environment.CurrentDirectory);
                    servers.Add(USO);
                }
            }

            new Thread(new ThreadStart(() => { commandLine(); })).Start();
        }

        #endregion Initialization

        #region CheckForUpdate

        #region GetLocalIP

        private string GetLocalIPAddress()
        {
            var host = Dns.GetHostEntry(Dns.GetHostName());
            foreach (var ip in host.AddressList)
            {
                if (ip.AddressFamily == AddressFamily.InterNetwork)
                {
                    return ip.ToString();
                }
            }
            throw new Exception("No network adapters with an IPv4 address in the system!");
        }

        #endregion GetLocalIP
        private void CheckForSystemUpdate()
        {
            MoonbyteServerConnection = new UniversalClient(false);

            if (!settingsFramework.CheckSetting("UPDATEP"))
            { settingsFramework.EditSetting("UPDATEP", true.ToString()); }

            if (settingsFramework.ReadSetting("UPDATEP") == true.ToString())
            {
                string IP = "moonbyte.us";
                string externalip = new WebClient().DownloadString("http://icanhazip.com");

                if (externalip.Contains(Dns.GetHostAddresses(new Uri("http://moonbyte.us").Host)[0].ToString())) { IP = "192.168.0.16"; }
                MoonbyteServerConnection.ConnectToRemoteServer(IP, 7777);

                AssemblyFileVersionAttribute UPFVersion = typeof(UniversalPluginFramework).GetTypeInfo().Assembly.GetCustomAttribute<AssemblyFileVersionAttribute>();
                AssemblyFileVersionAttribute UCVersion = typeof(UniversalClient).GetTypeInfo().Assembly.GetCustomAttribute<AssemblyFileVersionAttribute>();

                string CurrentVersion = MoonbyteServerConnection.SendCommand("dyn", new string[] { "GetVersion", "UniversalServer" });
                Assembly assembly = Assembly.GetExecutingAssembly();
                string Version = FileVersionInfo.GetVersionInfo(assembly.Location).ProductVersion;

                if (Version != CurrentVersion)
                {
                    ILogger.AddToLog("WARN", "Universal Server is currently out of date! Current version installed : " + Version);
                    ILogger.AddToLog("WARN", "With UniversalServer not updated, some plugins and features may not work correctly.");
                    ILogger.AddToLog("WARN", "Please install the updated version " + CurrentVersion);
                }
                else
                {
                    ILogger.AddToLog("INFO", "Universal Server - Version " + Version + ", is currently up to date!");
                }
                ILogger.AddWhitespace();
                ILogger.AddToLog("INFO", "UniversalPluginFramework version " + UPFVersion.Version);
                ILogger.AddToLog("INFO", "UniversalClient version " + UCVersion.Version);
                ILogger.AddWhitespace();

            }
        }

        #endregion CheckForUpdate

        #region CommandLine

        private void commandLine()
        { 
            ILogger.AddToLog("INFO", "Type Help for a list of commands..");

            while (true)
            {
                Console.Write(">");
                string Command = Console.ReadLine();
                string[] CommandArgs = Command.Split(' ');

                if (CommandArgs[0].ToUpper() == "CREATESERVER" ||
                    CommandArgs[0].ToUpper() == "CREATE")
                { CreateServer(CommandArgs[1]); }
                if (CommandArgs[0].ToUpper() == "DELETESERVER" || 
                    CommandArgs[0].ToUpper() == "DELETE")
                { DeleteServer(CommandArgs[1]); }
                if (CommandArgs[0].ToUpper() == "LIST" ||
                    CommandArgs[0].ToUpper() == "LISTSERVER" ||
                    CommandArgs[0].ToUpper() == "LISTSERVERS")
                { List(); }
                if (CommandArgs[0].ToUpper() == "EDITSERVERSETTING")
                { EditServerSetting(CommandArgs[1], CommandArgs[2], CommandArgs[3]); }
                if (CommandArgs[0].ToUpper() == "HELP")
                {
                    try { Help(int.Parse(CommandArgs[1])); }
                    catch { Help(); }
                }
                if (CommandArgs[0].ToUpper() == "STARTSERVER" || 
                    CommandArgs[0].ToUpper() == "START")
                {
                    StartServer(CommandArgs[1]);
                }
                if (CommandArgs[0].ToUpper() == "EXIT" ||
                    CommandArgs[0].ToUpper() == "CLOSE")
                {
                    try
                    {
                        if (CommandArgs[1] == null) { Exit(); }
                        else { Exit(CommandArgs[1]); }
                    } catch { Exit(); }
                }
            }
        }

        #endregion CommandLine

        #region Commands

        #region CreateServer

        private void CreateServer(string ServerName)
        {
            string oldSettingValue = settingsFramework.ReadSetting(_servers);
            if (oldSettingValue != null)
            {
                List<string> _Servers = oldSettingValue.Split(new string[] { seperator }, StringSplitOptions.RemoveEmptyEntries).ToList();

                foreach(string s in _Servers)
                {
                    if (s.ToUpper() == ServerName.ToUpper()) { ILogger.AddToLog("WARN", "Server already exist!"); return; }
                }

                _Servers.Add(ServerName);
                string newSettingValue = string.Join(seperator, _Servers);
                settingsFramework.EditSetting(_servers, newSettingValue);
            }
            else
            {
                string newSettingValue = ServerName + seperator;
                settingsFramework.EditSetting(_servers, newSettingValue);
            }

            UniversalServerObject newServer = new UniversalServerObject(ServerName, Environment.CurrentDirectory);
            servers.Add(newServer);

            ILogger.AddToLog("INFO", "Created " + ServerName + " sucessfully! Please start the server.");
        }

        #endregion CreateServer

        #region DeleteServer

        private void DeleteServer(string ServerName)
        {
            foreach(UniversalServerObject USO in servers)
            { if (USO.ServerName == ServerName)
                { servers.Remove(USO); USO.Dispose(); break; } }

            string Dir = Environment.CurrentDirectory + @"\" + ServerName;
            if (Directory.Exists(Dir)) { Directory.Delete(Dir, true); }

            string oldSettingValue = settingsFramework.ReadSetting(_servers);
            if (oldSettingValue != null)
            {
                List<string> _Servers = oldSettingValue.Split(new string[] { seperator }, StringSplitOptions.RemoveEmptyEntries).ToList();
                _Servers.Remove(ServerName);
                string newSettingValue = string.Join(seperator, _Servers);
                settingsFramework.EditSetting(_servers, newSettingValue);
            }

            ILogger.AddToLog("INFO", "Deleted " + ServerName + " sucessfully!");
        }

        #endregion DeleteServer

        #region ListServers

        private void List()
        {
            ILogger.AddToLog("INFO", "Displaying a total of " + servers.Count + " servers.");
            ILogger.AddWhitespace();

            foreach (UniversalServerObject USO in servers)
            {
                Console.Write(USO.ServerName + " : ");
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.Write("Status ");
                Console.ForegroundColor = ConsoleColor.Gray;
                Console.Write(": " + USO.Status + ", ");
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.Write("Clients Connected ");
                Console.ForegroundColor = ConsoleColor.Gray;
                Console.WriteLine(": " + USO.ClientCount);
            }

            ILogger.AddWhitespace();
            ILogger.AddToLog("INFO", "Finish listing all servers.");
        }

        #endregion ListServers

        #region EditServerSetting

        private void EditServerSetting(string ServerName, string SettingName, string SettingValue)
        {
            UniversalServerObject serverObject = GetServerObject(ServerName);
            if (serverObject == null) { ILogger.AddToLog("EROR", "Server Object is null!"); return; }

            serverObject.settingsFramework.EditSetting(SettingName, SettingValue);

            ILogger.AddToLog("INFO", "Wrote " + SettingValue + " in " + SettingName);
        }

        #endregion EditServerSetting

        #region Help

        private void Help(int Page = 0)
        {
            ILogger.AddWhitespace();
            ILogger.AddToLog("INFO", "CreateServer, Create [ServerName] - Creates a new server, that you'll be able to manage");
            ILogger.AddToLog("INFO", "DeleteServer, Delete [ServerName] - Deletes a current server, and related files.");
            ILogger.AddToLog("INFO", "List, ListServer, ListServers - List all currently created servers");
            ILogger.AddToLog("INFO", "EditServerSetting - Use the servers settings framework to write a server setting.");
            ILogger.AddToLog("INFO", "Exit, Close [ServerName] - If ServerName is null, closes the application. Else closes the server.");
            ILogger.AddWhitespace();
        }

        #endregion Help

        #region StartServer

        private void StartServer(string ServerName)
        {
            UniversalServerObject serverObject = GetServerObject(ServerName);
            if (serverObject == null) { ILogger.AddToLog("INFO", "Server doesn't exist!"); return; }
            serverObject.StartListener();
            ILogger.AddToLog("INFO", ServerName + " is now listening for connections!");
        }

        #endregion StartServer

        #region Exit

        private void Exit(string ServerName = null)
        {
            if (ServerName == null)
            {
                foreach(UniversalServerObject serverObject in servers)
                { serverObject.Dispose(); }

                Environment.Exit(1);
            }
            else
            {
                UniversalServerObject serverObject = GetServerObject(ServerName);
                if (serverObject == null) { ILogger.AddToLog("EROR", "Server Object is null!");  return; }

                serverObject.Dispose();
                ILogger.AddToLog("INFO", "Dispose of the server sucessfully.");
            }
        }

        #endregion Exit

        #endregion Commands 

        #region GetServerObject

        private UniversalServerObject GetServerObject(string ServerName)
        {
            foreach (UniversalServerObject USO in servers)
            { if (USO.ServerName == ServerName) { return USO; } } return null;
        }

        #endregion GetServerObject

    }
}
