﻿using Moonbyte.Security.Encryption;
using System;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Text;

namespace Moonbyte.UniversalClient
{
    public class UniversalClient
    {

        #region Vars

        private TcpClient Client;
        public static bool LogEvents;
        RSA Encryption = new RSA();
        public bool IsConnected
        {
            get
            { try { if (Client.Connected) { return true; } else { return false; } } catch { return false; } }
        }

        #endregion Vars

        #region Initialization

        public UniversalClient(bool logEvents)
        {
            LogEvents = logEvents;
            Client = new TcpClient();
        }

        #endregion Initialization

        #region ConnectToRemoteServer

        public void ConnectToRemoteServer(IPAddress InternetProtocolAddress, int ServerPort)
        { ConnectToRemoteServer(InternetProtocolAddress.ToString(), ServerPort); }
        public void ConnectToRemoteServer(string ServerIP, int ServerPort)
        {
            if (Client == null) return;

            Client.Connect(ServerIP, ServerPort);

            if (Client.Connected)
            {
                SendMessage("Key_ServerPublic|", false, null);
                this.Encryption.SetServerPublicKey(WaitForResult()); 
                SendMessage("Key_ClientPublic|" + this.Encryption.Encrypt(Encryption.GetClientPublicKey(), Encryption.GetServerPublicKey()), false);
                SendMessage("Key_ClientPrivate|" + this.Encryption.Encrypt(Encryption.GetClientPrivateKey(), Encryption.GetServerPublicKey()), false);

                var macAddr = ( from nic in NetworkInterface.GetAllNetworkInterfaces()
                        where nic.OperationalStatus == OperationalStatus.Up
                        select nic.GetPhysicalAddress().ToString()).FirstOrDefault();
                Console.WriteLine("test1");
                SendMessage("USER|SETID " + macAddr);
                WaitForResult(); Console.WriteLine("test2");
                SendMessage("USER|LOGIN");
                WaitForResult(); Console.WriteLine("test3");
            }
        }

        #endregion ConnectToRemoteServer

        #region SaveLogFile

        public void SendLogFileToServer(string ApplicationName, string LogContent)
        { SendMessage("USER|LOGFILE " + ApplicationName + " " + LogContent); WaitForResult(); }

        #endregion SaveLogFile

        #region WaitForResult

        public string WaitForResult()
        {
            byte[] data = new byte[Client.Client.ReceiveBufferSize];
            int receivedDataLength = Client.Client.Receive(data);
            string stringData = Encoding.ASCII.GetString(data, 0, receivedDataLength);
            if (LogEvents)Console.WriteLine("Server response: " + stringData);
            string Final = stringData.Replace("%20%", " ");
            //if (UseEncryption)
           // {
               // Final = encryption.Decrypt(Final, encryption.GetClientPrivateKey());
           // }
            return Final;
        }

        #endregion WaitForResult

        #region Communication

        #region SendCommand

        public string SendCommand(string Command, string[] args)
        {
            for (int i = 0; i < args.Length; i++)
            {
                args[i] = args[i].Replace(" ", "%40%");
            }
            string ArgsSend = string.Join(" ", args);
            if (LogEvents) Console.WriteLine("Args Send : " + ArgsSend);
            string valueToSend = "CLNT|" + Command + " " + ArgsSend;
            SendMessage(valueToSend); Console.WriteLine("test1");
            return WaitForResult();
        }

        #endregion SendCommand

        #region SendMessage

        public void SendMessage(string Value, bool UseEncryption = true, string Key = null)
        {
            //Sends the message to the client
            string stringToSend = Value.Replace(" ", "%20%");
            if (UseEncryption)
            {
                if (Key == null) { stringToSend = Encryption.Encrypt(stringToSend, Encryption.GetClientPrivateKey()); }
                else { stringToSend = Encryption.Encrypt(stringToSend, Key); }
            }
            if (LogEvents) Console.WriteLine("Sending " + stringToSend);
            byte[] BytesToSend = Encoding.UTF8.GetBytes(stringToSend);
            Client.Client.BeginSend(BytesToSend, 0, BytesToSend.Length, 0, new AsyncCallback(SendCallBack), Client);
        }

        #endregion SendMessage

        #region SendCallBack

        private void SendCallBack(IAsyncResult ar)
        {
            if (ar.IsCompleted)
            {
                if (LogEvents) Console.WriteLine("Data sent sucessfully!");
            }
            else
            {
                if (LogEvents) Console.WriteLine("Data was not sucessfully!");
            }
        }

        #endregion SendCallBack

        #endregion

        #region Disconnect

        public void Disconnect()
        {
            if (Client.Connected) Client.Close();
        }

        #endregion Disconnect

    }
}
