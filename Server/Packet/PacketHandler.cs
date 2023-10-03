using ServerCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Server.Packet
{
    internal class PacketHandler
    {
        public static void PlayerInfoReqHandler(PacketSession session, IPacket packet)
        {
            PlayerInfoReq p = packet as PlayerInfoReq;

            Console.WriteLine($"플레이어 id: {p.playerId}");
            Console.WriteLine($"플레이어 이름: {p.name}");

            foreach (PlayerInfoReq.Skill skill in p.skills)
            {
                Console.WriteLine($"스킬 id: {skill.id}");
                Console.WriteLine($"스킬 레벨: {skill.level}");
                Console.WriteLine($"스킬 시간: {skill.duration}");
            }
        }
    }
}
