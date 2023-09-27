using ServerCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Server
{
    class Packet
    {
        public ushort size;
        public ushort packetId;
    }

    class PlayerInfoReq : Packet
    {
        public long playerId;
    }

    class PlayerInfoRes : Packet
    {
        public int hp;
        public int attack;
    }

    public enum PacketID
    {
        PlayerInfoReq = 1,
        PlayerInfoRes = 2,
    }

    class ClientSession : PacketSession
    {
        public override void OnConnected(EndPoint endPoint)
        {
            Console.WriteLine($"Connected : {endPoint}");

            Packet packet = new Packet() { size = 50, packetId = 1 };

            ArraySegment<byte> openSegement = SendBufferHelper.Open(4096);
            byte[] buffer1 = BitConverter.GetBytes(packet.size);
            byte[] buffer2 = BitConverter.GetBytes(packet.packetId);
            Array.Copy(buffer1, 0, openSegement.Array, openSegement.Offset, buffer1.Length);
            Array.Copy(buffer2, 0, openSegement.Array, openSegement.Offset + buffer1.Length, buffer2.Length);
            ArraySegment<byte> sendBuffer = SendBufferHelper.Close(buffer1.Length + buffer2.Length);

            // 보냄
            Send(sendBuffer);

            Thread.Sleep(5000);

            Disconnect();
        }

        public override void OnReceivedPacket(ArraySegment<byte> buffer)
        {
            ushort count = 0;

            ushort size = BitConverter.ToUInt16(buffer.Array, buffer.Offset);
            count += 2;
            ushort id = BitConverter.ToUInt16(buffer.Array, buffer.Offset + count);
            count += 2;

            switch((PacketID) id)
            {
                case PacketID.PlayerInfoReq:
                    {
                        long playerId = (long)BitConverter.ToUInt64(buffer.Array, buffer.Offset + count);
                        count += 8;

                        Console.WriteLine($"플레이어 id: {playerId}");

                        break;
                    } 
            }

            Console.WriteLine($"패킷사이즈: {size}, 패킷아이디: {id}");
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
