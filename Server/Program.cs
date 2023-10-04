using System;
using static System.Collections.Specialized.BitVector32;
using System.Net;
using System.Text;
using ServerCore;
using Server.Session;

namespace Server
{
    internal class Program
    {
        static Listener listener = new Listener();
        public static GameRoom room = new GameRoom();

        static void Main(string[] args)
        {
            PacketManager.Instatnce.Register();

            // DNS
            string host = Dns.GetHostName();
            IPHostEntry ipHost = Dns.GetHostEntry(host);
            IPAddress ipAddr = ipHost.AddressList[0];
            IPEndPoint endPoint = new IPEndPoint(ipAddr, 7777);

            listener.Init(endPoint, () => { return SessionManager.Instance.Generate(); });
            Console.WriteLine("listen...");

            while (true)
            {

            }

        }
    }
}