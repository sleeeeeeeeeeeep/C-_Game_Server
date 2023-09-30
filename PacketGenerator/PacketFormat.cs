using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PacketGenerator
{
    internal class PacketFormat
    {
        // {0} 패킷 이름
        // {1} 멤버 변수
        // {2} 멤버 변수 read
        // {3} 멤버 변수 write
        public static string packetFormat =
@"
class {0}
{{
    {1}

    public void Read(ArraySegment<byte> arraySegement)
    {{
        ushort count = 0;

        ReadOnlySpan<byte> s = new ReadOnlySpan<byte>(arraySegement.Array, arraySegement.Offset, arraySegement.Count);

        count += sizeof(ushort);
        count += sizeof(ushort);

        {2}

    }}

    public ArraySegment<byte> Write()
    {
        ArraySegment<byte> openSegement = SendBufferHelper.Open(4096);

        ushort count = 0;
        bool isSuccess = true;

        Span<byte> s = new Span<byte>(openSegement.Array, openSegement.Offset, openSegement.Count);

        count += sizeof(ushort); // 전체 패킷 사이즈 정보

        isSuccess &= BitConverter.TryWriteBytes(s.Slice(count, s.Length - count), (ushort)PacketID.{0});
        count += sizeof(ushort); // 패킷 아이디(패킷 구분) 바이트 크기

        {3}

        // 전체 패킷 사이즈 정보는 마지막에
        isSuccess &= BitConverter.TryWriteBytes(s, count);

        if (!isSuccess)
        {{
            return null;
        }}

        return SendBufferHelper.Close(count);
    }}
}}

public enum PacketID
{{
    PlayerInfoReq = 1,
    PlayerInfoRes = 2,
}}
";

        // ----------------------------------------------------------------------------------------------------------
        // {0} 변수 자료형
        // {1} 변수 이름
        public static string memberFormat = "public {0} {1}";

        // {0} 변수 이름
        // {1} 바이트 크기로 변하는 BitConverter.To~()
        // {2} 변수 자료형(바이트 크기)
        public static string readFormat =
@"
this.{0} = BitConverter.{1}(s.Slice(count, s.Length - count));
count += sizeof({2});
";
        // ----------------------------------------------------------------------------------------------------------
        // {0} 변수 이름
        public static string readStringFormat =
@"
ushort {0}Length = BitConverter.ToUInt16(s.Slice(count, s.Length - count));
count += sizeof(ushort);

this.{0} = Encoding.Unicode.GetString(s.Slice(count, {0}Length));
count += {0}Length;
";

        // ----------------------------------------------------------------------------------------------------------
        // {0} 변수 이름
        // {1} 변수 자료형
        public static string writeFormat =
@"
isSuccess &= BitConverter.TryWriteBytes(s.Slice(count, s.Length - count), this.{0});
count += sizeof({1}); // {0} 바이트 크기
";

        // ----------------------------------------------------------------------------------------------------------
        // {0} 변수 이름
        public static string writeStringFormat =
@"
// 스트링 보낼 때: 스트링 크기 먼저 보내고 -> 스트링 내용 보냄
ushort {0}Length = (ushort)Encoding.Unicode.GetBytes(
    this.{0},
    0,
    this.{0}.Length,
    openSegement.Array,
    openSegement.Offset + count + sizeof(ushort)
);

isSuccess &= BitConverter.TryWriteBytes(s.Slice(count, s.Length - count), {0}Length);
count += sizeof(ushort); // {0}에 해당하는 스트링 크기 알려주는 부분
count += {0}Length; // {0} 바이트 크기
";

    }
}
