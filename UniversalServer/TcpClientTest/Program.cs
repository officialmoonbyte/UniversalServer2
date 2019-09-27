using Moonbyte.Security.Encryption;
using Moonbyte.UniversalClient;
using System;
using System.Threading;

namespace TcpClientTest
{
    class Program
    {
        static void Main(string[] args)
        {
            UniversalClient client = new UniversalClient(true);
            client.ConnectToRemoteServer("127.0.0.1", 7777);
            Console.WriteLine("Connected to a local universal server on port 7777");
            Thread.Sleep(3);
            string s = client.SendCommand("EmailServer", new string[] { "SetServerSettings", "moonbyte", "moonbyte123", "moonbyte.net", "465", "corporate@moonbyte.net", "MoonByte(ra3810)" });
            string s1 = client.SendCommand("EmailServer", new string[] { "SendMail", "moonbyte", "moonbyte123", "braydelritter@gmail.com", "support@moonbyte.net", "test", "test", false.ToString() });
            Console.WriteLine(s);
            Console.WriteLine(s1);
            Console.Read();
        }
    }
}
