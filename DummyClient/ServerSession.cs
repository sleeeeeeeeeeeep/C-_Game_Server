using ServerCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace DummyClient
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

    class ServerSession : Session
    {
        public override void OnConnected(EndPoint endPoint)
        {
            Console.WriteLine($"Connected : {endPoint}");

            PlayerInfoReq packet = new PlayerInfoReq() { packetId = (ushort)PacketID.PlayerInfoReq, playerId = 1001 };

            // 보냄
            ArraySegment<byte> openSegement = SendBufferHelper.Open(4096);

            ushort count = 0;
            bool isSuccess = true;

            count += 2; // packet.size 바이트 크기

            isSuccess &= BitConverter.TryWriteBytes(
                new Span<byte>(
                    openSegement.Array,
                    openSegement.Offset + count,
                    openSegement.Count - count
                    ), packet.packetId);
            count += 2; // packet.packetId 바이트 크기

            isSuccess &= BitConverter.TryWriteBytes(
                new Span<byte>(
                    openSegement.Array,
                    openSegement.Offset + count,
                    openSegement.Count - count
                    ), packet.playerId);
            count += 8; // packet.packetId 바이트 크기

            isSuccess &= BitConverter.TryWriteBytes(
                new Span<byte>(
                    openSegement.Array,
                    openSegement.Offset,
                    openSegement.Count
                    ), count);


            ArraySegment<byte> sendBuffer = SendBufferHelper.Close(count);

            // 보냄
            if(isSuccess)
            {
                Send(sendBuffer);
            }
        }

        public override int OnReceived(ArraySegment<byte> buffer)
        {
            string recvData = Encoding.UTF8.GetString(buffer.Array, buffer.Offset, buffer.Count);
            Console.WriteLine($"서버에서 받은거 : {recvData}");

            return buffer.Count;
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
