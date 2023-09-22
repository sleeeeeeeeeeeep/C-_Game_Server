using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace ServerCore
{
    class GameSession : Session
    {
        public override void OnConnected(EndPoint endPoint)
        {
            Console.WriteLine($"Connected : {endPoint}");

            // 보냄
            byte[] sendBuffer = Encoding.UTF8.GetBytes("send from sever");
            Send(sendBuffer);

            Thread.Sleep(1000);

            Disconnect();
        }

        public override void OnReceived(ArraySegment<byte> buffer)
        {
            string recvData = Encoding.UTF8.GetString(buffer.Array, buffer.Offset, buffer.Count);
            Console.WriteLine($"받은거 : {recvData}");
        }

        public override void OnSend(int numOfBytes)
        {
            Console.WriteLine($"Transfreed bytes: {numOfBytes}");
        }

        public override void OnDisconnected(EndPoint endPoint)
        {
            Console.WriteLine($"disconnected : {endPoint}");
        }
    }

    internal class Program
    {
        static Listener listener = new Listener();

        static void Main(string[] args)
        {
            // DNS
            string host = Dns.GetHostName();
            IPHostEntry ipHost = Dns.GetHostEntry(host);
            IPAddress ipAddr = ipHost.AddressList[0];
            IPEndPoint endPoint = new IPEndPoint(ipAddr, 7777);

            listener.Init(endPoint, () => { return new GameSession(); });
            Console.WriteLine("listen...");

            while (true)
            {
                
            }
            
        }
    }
}