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
            UniversalClient client = new UniversalClient(false);
            client.ConnectToRemoteServer("localhost", 7777);
            Console.WriteLine("Connected to a local universal server on port 7875");
            Thread.Sleep(3);
            client.SendCommand("dyn", new string[] { "AddProject", "UniversalServer", "4.3.4", "123", "corporate@moonbyte.net" });
            string s = client.WaitForResult();
            Console.WriteLine(s);
        }
    }
}
