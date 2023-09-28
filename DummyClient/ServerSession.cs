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
                ), 
                this.packetId
            );
            count += 2; // packet.packetId 바이트 크기

            isSuccess &= BitConverter.TryWriteBytes(
                new Span<byte>(
                    openSegement.Array,
                    openSegement.Offset + count,
                    openSegement.Count - count
                ), 
                this.playerId
            );
            count += 8; // packet.packetId 바이트 크기

            isSuccess &= BitConverter.TryWriteBytes(
                new Span<byte>(
                    openSegement.Array,
                    openSegement.Offset,
                    openSegement.Count
                ), 
                count
            );

            if (!isSuccess)
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

    class ServerSession : Session
    {
        public override void OnConnected(EndPoint endPoint)
        {
            Console.WriteLine($"Connected : {endPoint}");

            PlayerInfoReq packet = new PlayerInfoReq() { playerId = 1001 };

            ArraySegment<byte> sendBuffer = packet.Write();
            
            // 보냄
            if (sendBuffer != null)
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
