using Server;
using Server.Session;
using ServerCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


internal class PacketHandler
{
    public static void C_ChatHandler(PacketSession session, IPacket packet)
    {
        C_Chat chatPacket = packet as C_Chat;
        ClientSession clientSession = session as ClientSession; 

        if(clientSession == null)
        {
            return;
        }

        GameRoom room = clientSession.Room; // 큐에 넣어서 작업 밀리면 널참조로 크래쉬 발생할 수 있어서 이렇게 씀
        room.Push(() => clientSession.Room.BroadCast(clientSession, chatPacket.chat));
    }
}

