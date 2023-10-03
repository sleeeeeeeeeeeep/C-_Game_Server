using ServerCore;
using System;
using System.Net;
using System.Text;

namespace DummyClient
{
    

    class ServerSession : Session
    {
        public override void OnConnected(EndPoint endPoint)
        {
            Console.WriteLine($"Connected : {endPoint}");

            C_PlayerInfoReq packet = new C_PlayerInfoReq() { playerId = 1001, name = "이르음" };

            var skill = new C_PlayerInfoReq.Skill()
            {
                id = 101,
                level = 3,
                duration = 2.5f
            };
            skill.attributes.Add(new C_PlayerInfoReq.Skill.Attribute() { att = 1 });
            packet.skills.Add(skill);

            packet.skills.Add(new C_PlayerInfoReq.Skill()
            {
                id = 102,
                level = 4,
                duration = 3.5f
            });

            packet.skills.Add(new C_PlayerInfoReq.Skill()
            {
                id = 201,
                level = 1,
                duration = 1.0f
            });

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
