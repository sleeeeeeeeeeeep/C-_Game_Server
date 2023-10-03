using ServerCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


internal class PacketHandler
{
    public static void C_PlayerInfoReqHandler(PacketSession session, IPacket packet)
    {
        C_PlayerInfoReq p = packet as C_PlayerInfoReq;

        Console.WriteLine($"플레이어 id: {p.playerId}");
        Console.WriteLine($"플레이어 이름: {p.name}");

        foreach (C_PlayerInfoReq.Skill skill in p.skills)
        {
            Console.WriteLine($"스킬 id: {skill.id}");
            Console.WriteLine($"스킬 레벨: {skill.level}");
            Console.WriteLine($"스킬 시간: {skill.duration}");
        }
    }

    
}

