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
                        Console.WriteLine($"플레이어 이름: {playerInfo.name}");

                        foreach(PlayerInfoReq.Skill skill in playerInfo.skills)
                        {
                            Console.WriteLine($"스킬 id: {skill.id}");
                            Console.WriteLine($"스킬 레벨: {skill.level}");
                            Console.WriteLine($"스킬 시간: {skill.duration}");
                        }

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
