using ServerCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Server
{
    public abstract class Packet
    {
        public ushort size;
        public ushort packetId;

        public abstract ArraySegment<byte> Write();
        public abstract void Read(ArraySegment<byte> arraySegement);

    }

    class PlayerInfoReq : Packet
    {
        public long playerId;

        public PlayerInfoReq()
        {
            this.packetId = (ushort)PacketID.PlayerInfoReq;
        }

        public override void Read(ArraySegment<byte> arraySegement)
        {
            ushort count = 0;

            count += 2;
            count += 2;

            BitConverter.ToUInt64(
                new ReadOnlySpan<byte>(
                    arraySegement.Array,
                    arraySegement.Offset + count,
                    arraySegement.Count - count
                )
            );
            count += 8;
        }

        public override ArraySegment<byte> Write()
        {
            ArraySegment<byte> openSegement = SendBufferHelper.Open(4096);

            ushort count = 0;
            bool isSuccess = true;

            count += 2; // packet.size 바이트 크기

            isSuccess &= BitConverter.TryWriteBytes(
                new Span<byte>(
                    openSegement.Array,
                    openSegement.Offset + count,
                    openSegement.Count - count
                    ), this.packetId);
            count += 2; // packet.packetId 바이트 크기

            isSuccess &= BitConverter.TryWriteBytes(
                new Span<byte>(
                    openSegement.Array,
                    openSegement.Offset + count,
                    openSegement.Count - count
                    ), this.playerId);
            count += 8; // packet.packetId 바이트 크기

            isSuccess &= BitConverter.TryWriteBytes(
                new Span<byte>(
                    openSegement.Array,
                    openSegement.Offset,
                    openSegement.Count
                    ), count);

            if (isSuccess)
            {
                return null;
            }

            return SendBufferHelper.Close(count);
        }
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

            PlayerInfoReq packet = new PlayerInfoReq();

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
                        PlayerInfoReq playerInfo = new PlayerInfoReq();
                        playerInfo.Read(buffer);

                        Console.WriteLine($"플레이어 id: {playerInfo.playerId}");

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
