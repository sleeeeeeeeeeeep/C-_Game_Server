using ServerCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Server.Session
{
    class ClientSession : PacketSession
    {
        public int SessionId { get; set; }
        public GameRoom Room { get; set; }


        public override void OnConnected(EndPoint endPoint)
        {
            Console.WriteLine($"Connected : {endPoint}");

            Program.room.Push(() =>  Program.room.Enter(this) );
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
            SessionManager.Instance.Remove(this);

            if(Room != null )
            {
                GameRoom room = Room; // 큐에 넣어서 작업 밀리면 널참조로 크래쉬 발생할 수 있어서 이렇게 씀
                room.Push(() => room.Leave(this));
                Room = null;
            }

            Console.WriteLine($"disconnected : {endPoint}");
        }
    }
}
