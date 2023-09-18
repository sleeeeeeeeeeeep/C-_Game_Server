using System.Net;
using System.Net.Sockets;
using System.Text;

namespace DummyClient
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

            while (true)
            {
                Socket socket = new Socket(endPoint.AddressFamily, SocketType.Stream, ProtocolType.Tcp);

                try
                {
                    // 소켓 연결
                    socket.Connect(endPoint);
                    Console.WriteLine($"서버 연결 {socket.RemoteEndPoint.ToString()}");

                    // 보냄
                    byte[] sendBuffer = Encoding.UTF8.GetBytes("클라에서 보냄");
                    int sendBytes = socket.Send(sendBuffer);

                    // 받음
                    byte[] recvBuffer = new byte[1024];
                    int recvBytes = socket.Receive(recvBuffer);
                    string recvData = Encoding.UTF8.GetString(recvBuffer, 0, recvBytes);
                    Console.WriteLine(recvData);

                    // 연결 해제
                    socket.Shutdown(SocketShutdown.Both);
                    socket.Close();
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }

                Thread.Sleep(1000);
            }
        }
    }
}