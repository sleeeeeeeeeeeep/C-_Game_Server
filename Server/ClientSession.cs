﻿using ServerCore;
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
        public string name;

        public struct SkillInfo
        {
            public int id;
            public short level;
            public float duration;

            public bool Write(Span<byte> s, ref ushort count)
            {
                bool isSuccess = true;

                isSuccess &= BitConverter.TryWriteBytes(s.Slice(count, s.Length - count), id);
                count += sizeof(int);

                isSuccess &= BitConverter.TryWriteBytes(s.Slice(count, s.Length - count), level);
                count += sizeof(short);

                isSuccess &= BitConverter.TryWriteBytes(s.Slice(count, s.Length - count), duration);
                count += sizeof(float);

                return isSuccess;
            }

            public void Read(ReadOnlySpan<byte> s, ref ushort count)
            {
                id = BitConverter.ToInt32(s.Slice(count, s.Length - count));
                count += sizeof(int);

                level = BitConverter.ToInt16(s.Slice(count, s.Length - count));
                count += sizeof(short);

                duration = BitConverter.ToSingle(s.Slice(count, s.Length - count));
                count += sizeof(float);

            }
        }

        public List<SkillInfo> skills = new List<SkillInfo>();

        public PlayerInfoReq()
        {
            this.packetId = (ushort)PacketID.PlayerInfoReq;
        }

        public override void Read(ArraySegment<byte> arraySegement)
        {
            ushort count = 0;

            ReadOnlySpan<byte> s = new ReadOnlySpan<byte>(arraySegement.Array, arraySegement.Offset, arraySegement.Count);

            count += sizeof(ushort);
            count += sizeof(ushort);

            this.playerId = BitConverter.ToInt64(s.Slice(count, s.Length - count));
            count += sizeof(long);

            // string
            ushort nameLength = BitConverter.ToUInt16(s.Slice(count, s.Length - count));
            count += sizeof(ushort);

            this.name = Encoding.Unicode.GetString(s.Slice(count, nameLength));
            count += nameLength;

            // list
            skills.Clear();

            ushort skillLength = BitConverter.ToUInt16(s.Slice(count, s.Length - count));
            count += sizeof(ushort);

            for (int i = 0; i < skillLength; i++)
            {
                SkillInfo skill = new SkillInfo();
                skill.Read(s, ref count);

                skills.Add(skill);
            }

        }

        public override ArraySegment<byte> Write()
        {
            ArraySegment<byte> openSegement = SendBufferHelper.Open(4096);

            ushort count = 0;
            bool isSuccess = true;

            Span<byte> s = new Span<byte>(openSegement.Array, openSegement.Offset, openSegement.Count);

            count += sizeof(ushort); // packet.size 바이트 크기(전체 패킷 사이즈 정보)

            isSuccess &= BitConverter.TryWriteBytes(s.Slice(count, s.Length - count), this.packetId);
            count += sizeof(ushort); // packet.packetId 바이트 크기

            isSuccess &= BitConverter.TryWriteBytes(s.Slice(count, s.Length - count), this.playerId);
            count += sizeof(long); // packet.playerId 바이트 크기


            // 스트링 보낼 때: 스트링 크기 먼저 보내고 -> 스트링 내용 보냄
            ushort nameLength = (ushort)Encoding.Unicode.GetBytes(
                this.name,
                0,
                this.name.Length,
                openSegement.Array,
                openSegement.Offset + count + sizeof(ushort)
            );

            isSuccess &= BitConverter.TryWriteBytes(s.Slice(count, s.Length - count), nameLength);
            count += sizeof(ushort); // packet.name에서 스트링 크기 알려주는 부분
            count += nameLength; // packet.name 바이트 크기

            // list 보낼 때
            isSuccess &= BitConverter.TryWriteBytes(s.Slice(count, s.Length - count), (ushort)skills.Count);
            count += sizeof(ushort); // packet.skills에서 리스트 크기 알려주는 부분

            foreach (SkillInfo skill in skills)
            {
                isSuccess &= skill.Write(s, ref count);
            }

            // 전체 패킷 사이즈 정보는 마지막에
            isSuccess &= BitConverter.TryWriteBytes(s, count);

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
                        Console.WriteLine($"플레이어 이름: {playerInfo.name}");

                        foreach(PlayerInfoReq.SkillInfo skill in playerInfo.skills)
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