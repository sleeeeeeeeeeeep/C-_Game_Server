using ServerCore;
using System;
using System.Net;
using System.Text;

namespace DummyClient
{
    

    class ServerSession : PacketSession
    {
        public override void OnConnected(EndPoint endPoint)
        {
            Console.WriteLine($"Connected : {endPoint}");
        }

        public override void OnReceivedPacket(ArraySegment<byte> buffer)
        {
            PacketManager.Instatnce.OnRecvPacket(this, buffer);
        }

        public override void OnSend(int numOfBytes)
        {
            // Console.WriteLine($"Transfreed bytes: {numOfBytes}");
        }

        public override void OnDisconnected(EndPoint endPoint)
        {
            Console.WriteLine($"disconnected : {endPoint}");
        }
    }
}
