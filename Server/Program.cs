using System;
using static System.Collections.Specialized.BitVector32;
using System.Net;
using System.Text;
using ServerCore;
using Server.Packet;

namespace Server
{
    internal class Program
    {
        static Listener listener = new Listener();

        static void Main(string[] args)
        {
            PacketManager.Instatnce.Register();

            // DNS
            string host = Dns.GetHostName();
            IPHostEntry ipHost = Dns.GetHostEntry(host);
            IPAddress ipAddr = ipHost.AddressList[0];
            IPEndPoint endPoint = new IPEndPoint(ipAddr, 7777);

            listener.Init(endPoint, () => { return new ClientSession(); });
            Console.WriteLine("listen...");

            while (true)
            {

            }

        }
    }
}