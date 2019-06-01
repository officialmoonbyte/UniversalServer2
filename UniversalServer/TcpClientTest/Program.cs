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
            UniversalClient client = new UniversalClient();
            client.ConnectToRemoteServer("127.0.0.1", 7777);
            Console.WriteLine("Connected to a local universal server on port 7777");
            Thread.Sleep(3);
            client.SendCommand("dyn", new string[] { "AddProject", "UniversalClient", "4.3.4", "123", "corporate@moonbyte.net" });
            string s = client.WaitForResult();
            Console.WriteLine(s);
        }
    }
}
