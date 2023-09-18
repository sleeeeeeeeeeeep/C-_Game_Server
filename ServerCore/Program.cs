using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace ServerCore
{
    internal class Program
    {
        static void Main(string[] args)
        {
            // DNS
            string host = Dns.GetHostName();
            IPHostEntry ipHost = Dns.GetHostEntry(host);
            IPAddress ipAddr = ipHost.AddressList[0];
            IPEndPoint endPoint = new IPEndPoint(ipAddr, 7777);

            Socket listenSocket = new Socket(endPoint.AddressFamily, SocketType.Stream, ProtocolType.Tcp);

            try
            {
                listenSocket.Bind(endPoint);

                // backlog: 최대 대기 수
                listenSocket.Listen(10);

                while (true)
                {
                    Console.WriteLine("listen...");

                    Socket clientSocket = listenSocket.Accept();

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
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }
    }
}