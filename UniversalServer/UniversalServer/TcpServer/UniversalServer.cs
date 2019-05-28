using GlobalSettingsFramework;
using Moonbyte.IO.Log;
using Moonbyte.Security.Encryption;
using Moonbyte.UniversalServer.PluginFramework;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Reflection;
using System.Text;
using UniversalServer.IUser;
using static Moonbyte.UniversalServer.PluginLoader;

namespace Moonbyte.Net.TcpServer
{
    public class UniversalServerObject
    {
        #region Vars

        TcpListener listener;
        public GFS settingsFramework;
        public int ClientCount = 0;

        List<UniversalPluginFramework> LoadedPlugins;

        public string ServerName;
        public string PluginDirectory;
        public string PluginEditDirectory;
        public string Status = "Initializing";

        #endregion Vars

        #region Initialization

        public UniversalServerObject(string Name, string ServerDirectory)
        {
            ServerDirectory = ServerDirectory + @"\" + Name;
            PluginDirectory = ServerDirectory + @"\Plugins";
            PluginEditDirectory = ServerDirectory + @"\Plugin Settings";
            if (!Directory.Exists(ServerDirectory)) Directory.CreateDirectory(ServerDirectory);
            ServerDirectory = ServerDirectory + @"\" + "settings.dll";

            if (!Directory.Exists(PluginDirectory)) { Directory.CreateDirectory(PluginDirectory); }
            if (!Directory.Exists(PluginEditDirectory)) { Directory.CreateDirectory(PluginEditDirectory); }

            ServerName = Name;
            settingsFramework = new GFS(ServerDirectory);
            string port = null; if (settingsFramework.CheckSetting("port")) { port = settingsFramework.ReadSetting("port"); }
            else { settingsFramework.EditSetting("port", "7777"); port = "7777"; }
        }

        #endregion Initialization

        #region Start

        public void StartListener()
        {
            string port = null; if (settingsFramework.CheckSetting("port")) { port = settingsFramework.ReadSetting("port"); }
            else { settingsFramework.EditSetting("port", "7777"); port = "7777"; }

            listener = new TcpListener(new IPEndPoint(IPAddress.Any, int.Parse(port)));
            listener.Start();

            ILogger.AddToLog("INFO", this.ServerName + " is listening for clients on port " + port);

            LoadPlugins();

            listener.BeginAcceptTcpClient(OnClientAccepted, listener);
        }

        #endregion Start

        #region Initialize Plugins

        private void LoadPlugins()
        {
            string[] dllFileNames = null;
            if (Directory.Exists(PluginDirectory)) { dllFileNames = Directory.GetFiles(PluginDirectory, "*.dll"); }

            ILogger.AddToLog("INFO", "Loading " + dllFileNames.Length + " plugins... Please wait.");

            GC.Collect();
            GC.WaitForPendingFinalizers();

            LoadedPlugins = GenericPluginLoader<UniversalPluginFramework>.LoadPlugins(PluginDirectory);

            foreach(UniversalPluginFramework plugin in LoadedPlugins)
            { plugin.OnLoad(PluginEditDirectory + @"\" + plugin.Name); }

            ILogger.AddWhitespace();
            Log("Loaded a total of " + LoadedPlugins.Count + " plugins.");

        }

        #endregion

        #region Server.Log

        private void Log(string Value)
        {
            ILogger.AddToLog("INFO", "(" + ServerName + ") " + Value);
        }

        #endregion Server.Log

        #region OnClientReadBack

        private void OnClientRead(IAsyncResult ar)
        {
            ClientContext context = ar.AsyncState as ClientContext;
            if (context == null) return;

            try
            {
                string req = ReadClient(context, ar);

                string[] commandArgs = req.Split('|');
                if (commandArgs[0] == "Key_ClientPublic")
                { context.Encryption.SetClientPublicKey(context.Encryption.Decrypt(commandArgs[1], context.Encryption.GetServerPrivateKey()));
                    ILogger.AddToLog("INFO", "Received " + context.ClientIP + " public key.");
                }
                else if (commandArgs[0] == "Key_ClientPrivate")
                { context.Encryption.SetClientPrivateKey(context.Encryption.Decrypt(commandArgs[1], context.Encryption.GetServerPrivateKey()));
                    ILogger.AddToLog("INFO", "Received " + context.ClientIP + " private key.");
                }
                else if (commandArgs[0] == "Key_ServerPublic")
                { context.Client.Client.Send(Encoding.UTF8.GetBytes(context.Encryption.GetServerPublicKey()));
                    ILogger.AddToLog("INFO", "Client " + context.ClientIP + " has requested the server public key");
                }
                else
                {
                    string unEncryptedReq = context.Encryption.Decrypt(req, context.Encryption.GetClientPrivateKey());
                    string[] ClientArgs = unEncryptedReq.Split('|');

                    if (ClientArgs[0] == "CLNT")
                    {
                        context.clientTracker.LogClientCommand(unEncryptedReq, context.ClientIP);

                        string[] rCommandArgs = ClientArgs[1].Split(new string[] { "%20%" }, StringSplitOptions.None);

                        bool SentCommand = false;

                        foreach(UniversalPluginFramework plugin in LoadedPlugins)
                        {
                            if (plugin.Name == rCommandArgs[0]) { plugin.Invoke(rCommandArgs); SentCommand = true; }
                        }
                        if (SentCommand == false) { context.SendMessage("Unknown Command."); }

                    }
                    else if (ClientArgs[0] == "USER")
                    { ProcessUserRequest(context, ClientArgs[0], ClientArgs[1]); }
                }
            }
            catch (NullReferenceException e)
            {

            }
            catch (Exception e)
            {
                ILogger.LogExceptions(e);
                ILogger.AddToLog("INFO", "Disconnect client from OnClientRead");
                DisconnectClient(context);
            }
            finally
            {
                try { if (context != null && context.Stream != null) context.Stream.BeginRead(context.Buffer, 0, context.Buffer.Length, OnClientRead, context); } catch { }
            }
        }

        private string ReadClient(ClientContext context, IAsyncResult ar)
        {
            string Content = String.Empty;

            context.Stream.Flush();
            context.sb = new StringBuilder();

            try
            {
                if (context == null) return null;
                Socket handler = context.Client.Client;

                int read = handler.EndReceive(ar);

                // Data was read from the client socket.
                if (read > 0)
                {
                    string b = context.sb.Append(Encoding.UTF8.GetString(context.Buffer, 0, read)).ToString();
                    string[] Result = b.Split(new string[] { "<EOF>" }, StringSplitOptions.RemoveEmptyEntries);
                    return Result[0];
                }

                return null;
            }
            catch (Exception e)
            {
                ILogger.LogExceptions(e);
                ILogger.AddToLog("INFO", "Disconnect client from ReadClient");
                DisconnectClient(context);
                return null;
            }
        }

        #endregion OnClientReadBack

        #region OnClientAccepted

        private void OnClientAccepted(IAsyncResult ar)
        {
            bool CountClient = false;
            TcpListener listener = ar.AsyncState as TcpListener;
            if (listener == null)
                return;

            try
            {
                ClientContext context = new ClientContext();
                context.Encryption = new RSA(true);
                context.clientTracker = new ClientTracker(this.ServerName);
                context.Client = listener.EndAcceptTcpClient(ar);
                context.Stream = context.Client.GetStream();
                context.Stream.BeginRead(context.Buffer, 0, context.Buffer.Length, OnClientRead, context);
                IPEndPoint endPoint = (IPEndPoint)context.Client.Client.RemoteEndPoint;
                context.ClientIP = endPoint.ToString();
                this.ClientCount += 1; CountClient = true; ILogger.AddToLog("INFO", this.ServerName + " has established a connection with " + context.ClientIP);

            }
            catch (Exception e)
            {
                ILogger.LogExceptions(e);
                if (CountClient) { this.ClientCount -= 1; }
            }
            finally
            {
                listener.BeginAcceptTcpClient(OnClientAccepted, listener);
            }
        }

        #endregion OnClientAccepted

        #region Process Command

        #region User

        private void ProcessUserRequest(ClientContext context, string Header, string Command)
        {
            Command = Command.Replace("%20%", " ");
            string[] CommandArgs = Command.Split(' ');
            if (CommandArgs[0].ToUpper() == "SETID") { context.clientTracker.ClientID = CommandArgs[1]; context.SendMessage("SetID Complete"); }
            else if (CommandArgs[0].ToUpper() == "LOGIN") { context.clientTracker.AddLoginEvent(context.ClientIP); context.SendMessage("Login Complete"); }
        }

        #endregion User

        #endregion Process Command

        #region DisconnectUser

        private void DisconnectClient(ClientContext context)
        {
            ILogger.AddToLog("WARN", "Client receive error. Client disconnected?");
            ILogger.AddToLog("WARN", "Disconnecting client with receive error.");

            context.clientTracker.LogClientDisconnect(context.ClientIP);

            context.Buffer = null;
            context.Client = null;
            context.Stream.Dispose();
            context = null;

            ClientCount--;
        }

        #endregion DisconnectUser

        #region Dispose

        public void Dispose()
        {
            if (listener != null)
            {
                try { listener.Stop(); } catch (Exception e) { ILogger.LogExceptions(e); }
            }
        }

        #endregion
    }
}
