using System.Collections.Generic;
using System;
using System.IO;
using System.Linq;

namespace UniversalServer.IUser
{
    public class ClientTracker
    {

        #region Vars

        string clientID;

        #region Directories

        string ServerName = null;

        private string UserDirectories;

        private string Seperator = "(*50*)";

        private string ThisUserDirectory;
        private string LoginLogDirectory;
        private string DisconnectLogDirectory;
        private string LogIPDirectory;
        private string LogCommandDirectory;

        #endregion Directories

        #endregion

        #region Properties

        public string ClientID
        {
            get { return clientID; }
            set { clientID = value; this.UpdateClient(); }
        }

        #endregion

        #region Initialization

        public ClientTracker(string serverName)
        {
            ServerName = serverName;
            UserDirectories = Environment.CurrentDirectory + @"\" + ServerName + @"\Clients\";
            if (!Directory.Exists(UserDirectories)) Directory.CreateDirectory(UserDirectories);
        }

        #endregion Initialization

        #region Login

        public void AddLoginEvent(string IP)
        {
            LogIP(IP);
            EditValue(DateTime.Now.ToString() + Seperator + IP, LoginLogDirectory);
        }

        #endregion Login

        #region LogCommand

        public void LogClientCommand(string FullCommand, string IP)
        {
            EditValue(DateTime.Now.ToString() + Seperator + IP + Seperator + FullCommand, LogCommandDirectory);
        }

        #endregion LogCommand

        #region Disconnect

        public void LogClientDisconnect(string IP)
        {
            EditValue(DateTime.Now.ToString() + Seperator + IP, DisconnectLogDirectory);
        }

        #endregion Disconnect

        #region LogIP

        public void LogIP(string IP)
        {
            if (File.Exists(LogIPDirectory))
            {
                List<string> IPs = File.ReadAllLines(LogIPDirectory).ToList();
                bool addToList = true;
                foreach(string s in IPs)
                { if (s == IP) { addToList = false; } }
                if (addToList) IPs.Add(IP);
                File.WriteAllLines(LogIPDirectory, IPs);
            }
            else
            {
                File.Create(LogIPDirectory).Close();
                File.WriteAllText(LogIPDirectory, IP);
            }
        }

        #endregion LogIP

        #region EditValue

        private void EditValue(string Value, string FileDirectory)
        {
            if (File.Exists(FileDirectory))
            {
                File.WriteAllText(FileDirectory, Value + "\n" + File.ReadAllText(FileDirectory));
            } else { File.Create(FileDirectory).Close(); File.WriteAllText(FileDirectory, Value); }
        }

        #endregion EditValue

        #region UpdateClient

        private void UpdateClient()
        {
            ThisUserDirectory = UserDirectories + @"\" + clientID;
            LoginLogDirectory = ThisUserDirectory + @"\Logins";
            DisconnectLogDirectory = ThisUserDirectory + @"\Disconnects\";
            LogCommandDirectory = ThisUserDirectory + @"\Commands";
            LogIPDirectory = ThisUserDirectory + @"\Known ID";

            if (!Directory.Exists(ThisUserDirectory)) Directory.CreateDirectory(ThisUserDirectory);
            if (!Directory.Exists(LoginLogDirectory)) Directory.CreateDirectory(LoginLogDirectory);
            if (!Directory.Exists(DisconnectLogDirectory)) Directory.CreateDirectory(DisconnectLogDirectory);
            if (!Directory.Exists(LogCommandDirectory)) Directory.CreateDirectory(LogCommandDirectory);
            if (!Directory.Exists(LogIPDirectory)) Directory.CreateDirectory(LogIPDirectory);

            LoginLogDirectory += @"\Logins.log";
            DisconnectLogDirectory += @"\Disconnects.log";
            LogCommandDirectory += @"\Commands.log";
            LogIPDirectory += @"\IP.log";
        }

        #endregion
    }
}
