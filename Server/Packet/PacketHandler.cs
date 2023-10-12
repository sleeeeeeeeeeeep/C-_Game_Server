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
    public static void C_LeaveGameHandler(PacketSession session, IPacket packet)
    {
        ClientSession clientSession = session as ClientSession;
        GameRoom room = clientSession.Room; // 큐에 넣어서 작업 밀리면 널참조로 크래쉬 발생할 수 있어서 이렇게 씀

        if (clientSession == null)
        {
            return;
        }

        room.Push(() => room.Leave(clientSession));
    }

    public static void C_MoveHandler(PacketSession session, IPacket packet)
    {
        C_Move movePacket = packet as C_Move;
        ClientSession clientSession = session as ClientSession;
        GameRoom room = clientSession.Room; // 큐에 넣어서 작업 밀리면 널참조로 크래쉬 발생할 수 있어서 이렇게 씀

        if (clientSession == null)
        {
            return;
        }

        Console.WriteLine($"이동좌표 {movePacket.posX}, {movePacket.posY}, {movePacket.posZ}");

        room.Push(() => room.Move(clientSession, movePacket));
    }
}

