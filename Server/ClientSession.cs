using Server.Packet;
using ServerCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Server
{
    class ClientSession : PacketSession
    {
        public override void OnConnected(EndPoint endPoint)
        {
            Console.WriteLine($"Connected : {endPoint}");

            PlayerInfoReq packet = new PlayerInfoReq();

            //ArraySegment<byte> openSegement = SendBufferHelper.Open(4096);
            //byte[] buffer1 = BitConverter.GetBytes(packet.size);
            //byte[] buffer2 = BitConverter.GetBytes(packet.packetId);
            //Array.Copy(buffer1, 0, openSegement.Array, openSegement.Offset, buffer1.Length);
            //Array.Copy(buffer2, 0, openSegement.Array, openSegement.Offset + buffer1.Length, buffer2.Length);
            //ArraySegment<byte> sendBuffer = SendBufferHelper.Close(buffer1.Length + buffer2.Length);

            //// 보냄
            //Send(sendBuffer);

            Thread.Sleep(5000);

            Disconnect();
        }

        public override void OnReceivedPacket(ArraySegment<byte> buffer)
        {
            PacketManager.Instatnce.OnRecvPacket(this, buffer);
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
}
