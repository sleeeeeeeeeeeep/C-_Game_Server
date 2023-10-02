using ServerCore;
using System;
using System.Net;
using System.Text;
using static DummyClient.PlayerInfoReq.Skill;

namespace DummyClient
{
    public enum PacketID
    {
        PlayerInfoReq = 1,
        Test = 2,

    }


    class PlayerInfoReq
    {

        public byte testByte;
        public long playerId;
        public string name;

        public class Skill
        {

            public int id;
            public short level;
            public float duration;

            public class Attribute
            {

                public int att;


                public void Read(ReadOnlySpan<byte> s, ref ushort count)
                {


                    this.att = BitConverter.ToInt32(s.Slice(count, s.Length - count));
                    count += sizeof(int);

                }

                public bool Write(Span<byte> s, ref ushort count)
                {
                    bool isSuccess = true;



                    isSuccess &= BitConverter.TryWriteBytes(s.Slice(count, s.Length - count), this.att);
                    count += sizeof(int); // att 바이트 크기


                    return isSuccess;
                }
            }

            public List<Attribute> attributes = new List<Attribute>();


            public void Read(ReadOnlySpan<byte> s, ref ushort count)
            {


                this.id = BitConverter.ToInt32(s.Slice(count, s.Length - count));
                count += sizeof(int);

                this.level = BitConverter.ToInt16(s.Slice(count, s.Length - count));
                count += sizeof(short);

                this.duration = BitConverter.ToSingle(s.Slice(count, s.Length - count));
                count += sizeof(float);

                this.attributes.Clear();

                ushort attributeLength = BitConverter.ToUInt16(s.Slice(count, s.Length - count));
                count += sizeof(ushort);

                for (int i = 0; i < attributeLength; i++)
                {
                    Attribute attribute = new Attribute();
                    attribute.Read(s, ref count);

                    attributes.Add(attribute);
                }

            }

            public bool Write(Span<byte> s, ref ushort count)
            {
                bool isSuccess = true;



                isSuccess &= BitConverter.TryWriteBytes(s.Slice(count, s.Length - count), this.id);
                count += sizeof(int); // id 바이트 크기

                isSuccess &= BitConverter.TryWriteBytes(s.Slice(count, s.Length - count), this.level);
                count += sizeof(short); // level 바이트 크기

                isSuccess &= BitConverter.TryWriteBytes(s.Slice(count, s.Length - count), this.duration);
                count += sizeof(float); // duration 바이트 크기

                // list 보낼 때
                isSuccess &= BitConverter.TryWriteBytes(s.Slice(count, s.Length - count), (ushort)this.attributes.Count);
                count += sizeof(ushort); // packet.attributes에서 리스트 크기 알려주는 부분

                foreach (Attribute attribute in this.attributes)
                {
                    isSuccess &= attribute.Write(s, ref count);
                }


                return isSuccess;
            }
        }

        public List<Skill> skills = new List<Skill>();


        public void Read(ArraySegment<byte> arraySegement)
        {
            ushort count = 0;

            ReadOnlySpan<byte> s = new ReadOnlySpan<byte>(arraySegement.Array, arraySegement.Offset, arraySegement.Count);

            count += sizeof(ushort);
            count += sizeof(ushort);



            this.testByte = (byte)arraySegement.Array[arraySegement.Offset + count];
            count += sizeof(byte);

            this.playerId = BitConverter.ToInt64(s.Slice(count, s.Length - count));
            count += sizeof(long);

            ushort nameLength = BitConverter.ToUInt16(s.Slice(count, s.Length - count));
            count += sizeof(ushort);

            this.name = Encoding.Unicode.GetString(s.Slice(count, nameLength));
            count += nameLength;

            this.skills.Clear();

            ushort skillLength = BitConverter.ToUInt16(s.Slice(count, s.Length - count));
            count += sizeof(ushort);

            for (int i = 0; i < skillLength; i++)
            {
                Skill skill = new Skill();
                skill.Read(s, ref count);

                skills.Add(skill);
            }


        }

        public ArraySegment<byte> Write()
        {
            ArraySegment<byte> openSegement = SendBufferHelper.Open(4096);

            ushort count = 0;
            bool isSuccess = true;

            Span<byte> s = new Span<byte>(openSegement.Array, openSegement.Offset, openSegement.Count);

            count += sizeof(ushort); // 전체 패킷 사이즈 정보

            isSuccess &= BitConverter.TryWriteBytes(s.Slice(count, s.Length - count), (ushort)PacketID.PlayerInfoReq);
            count += sizeof(ushort); // 패킷 아이디(패킷 구분) 바이트 크기



            openSegement.Array[openSegement.Offset + count] = (byte)this.testByte;
            count += sizeof(byte);

            isSuccess &= BitConverter.TryWriteBytes(s.Slice(count, s.Length - count), this.playerId);
            count += sizeof(long); // playerId 바이트 크기

            // 스트링 보낼 때: 스트링 크기 먼저 보내고 -> 스트링 내용 보냄
            ushort nameLength = (ushort)Encoding.Unicode.GetBytes(
                this.name,
                0,
                this.name.Length,
                openSegement.Array,
                openSegement.Offset + count + sizeof(ushort)
            );

            isSuccess &= BitConverter.TryWriteBytes(s.Slice(count, s.Length - count), nameLength);
            count += sizeof(ushort); // name에 해당하는 스트링 크기 알려주는 부분
            count += nameLength; // name 바이트 크기

            // list 보낼 때
            isSuccess &= BitConverter.TryWriteBytes(s.Slice(count, s.Length - count), (ushort)this.skills.Count);
            count += sizeof(ushort); // packet.skills에서 리스트 크기 알려주는 부분

            foreach (Skill skill in this.skills)
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

    class ServerSession : Session
    {
        public override void OnConnected(EndPoint endPoint)
        {
            Console.WriteLine($"Connected : {endPoint}");

            PlayerInfoReq packet = new PlayerInfoReq() { playerId = 1001, name = "이르음" };

            var skill = new PlayerInfoReq.Skill()
            {
                id = 101,
                level = 3,
                duration = 2.5f
            };
            skill.attributes.Add(new PlayerInfoReq.Skill.Attribute() { att = 1 });
            packet.skills.Add(skill);

            packet.skills.Add(new PlayerInfoReq.Skill()
            {
                id = 102,
                level = 4,
                duration = 3.5f
            });

            packet.skills.Add(new PlayerInfoReq.Skill()
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
