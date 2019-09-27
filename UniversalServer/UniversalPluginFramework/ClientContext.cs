using Moonbyte.Logging;
using Moonbyte.Security.Encryption;
using System.IO;
using System.Net.Sockets;
using System.Text;
using UniversalServer.IUser;

namespace Moonbyte.UniversalServer.PluginFramework
{
    public class ClientContext
    {

        public string ClientIP;
        public TcpClient Client;
        public StringBuilder sb;
        public Stream Stream;
        public ServerRSA Encryption;
        public ClientTracker clientTracker;
        public ClientStorage clientStorage;
        public byte[] Buffer = new byte[65525];
        public MemoryStream Message = new MemoryStream();

        public void SendMessage(string Value)
        {
            Client.Client.Send(Encoding.UTF8.GetBytes(Value));
        }

        public void Log(string Header, string Value) { ILogger.AddToLog(Header, Value); }

    }
}
