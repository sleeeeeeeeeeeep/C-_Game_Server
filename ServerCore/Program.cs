using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace ServerCore
{
    internal class Program
    {
        static Listener listener = new Listener();
        static void  OnAcceptHandler(Socket clientSocket)
        {
            try
            {
                // 받음
                byte[] recvBuffer = new byte[1024];
                int recvBtyes = clientSocket.Receive(recvBuffer);
                string recvData = Encoding.UTF8.GetString(recvBuffer, 0, recvBtyes);
                Console.WriteLine($"client: {recvData}");

                // 보냄
                byte[] sendBuffer = Encoding.UTF8.GetBytes("send from sever");
                clientSocket.Send(sendBuffer);

                // 연결 해제
                clientSocket.Shutdown(SocketShutdown.Both);
                clientSocket.Close();
            }
            catch(Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }

        static void Main(string[] args)
        {
            // DNS
            string host = Dns.GetHostName();
            IPHostEntry ipHost = Dns.GetHostEntry(host);
            IPAddress ipAddr = ipHost.AddressList[0];
            IPEndPoint endPoint = new IPEndPoint(ipAddr, 7777);

            listener.Init(endPoint, OnAcceptHandler);
            Console.WriteLine("listen...");

            while (true)
            {
                
            }
            
        }
    }
}