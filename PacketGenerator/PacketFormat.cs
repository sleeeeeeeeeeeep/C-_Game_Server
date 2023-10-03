using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PacketGenerator
{
    internal class PacketFormat
    {
        // {0} 패킷 종류(enum 값)
        // {1} 패킷 포맷
        public static string fileFormat =
@"using ServerCore;
using System.Net;
using System.Text;

public enum PacketID
{{
    {0}
}}

interface IPacket
{{
    ushort Protocol {{ get;}}
    void Read(ArraySegment<byte> segement);
    ArraySegment<byte> Write();
}}

{1}
";


        // ----------------------------------------------------------------------------------------------------------
        // {0} 패킷 이름
        // {1} 패킷 번호
        public static string packetEnumFormat =
@"{0} = {1},";


        // ----------------------------------------------------------------------------------------------------------
        // {0} 패킷 이름
        // {1} 멤버 변수
        // {2} 멤버 변수 read
        // {3} 멤버 변수 write
        public static string packetFormat =
@"
class {0} : IPacket
{{
    {1}

    public ushort Protocol
	{{
		get {{ return (ushort)PacketID.{0}; }}
	}}

    public void Read(ArraySegment<byte> arraySegement)
    {{
        ushort count = 0;

        ReadOnlySpan<byte> s = new ReadOnlySpan<byte>(arraySegement.Array, arraySegement.Offset, arraySegement.Count);

        count += sizeof(ushort);
        count += sizeof(ushort);

        {2}

    }}

    public ArraySegment<byte> Write()
    {{
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
";

        // ----------------------------------------------------------------------------------------------------------
        // {0} 변수 자료형
        // {1} 변수 이름
        public static string memberFormat = 
@"public {0} {1};
";


        // ----------------------------------------------------------------------------------------------------------
        // {0} 리스트 정의(class)
        // {1} 리스트 이름(변수)
        // {2} 리스트 내 멤버 변수
        // {3} 리스트 내 멤버 변수 read
        // {4} 리스트 내 멤버 변수 write
        public static string memberListFormat =
@"
public class {0}
{{
    {2}

    public void Read(ReadOnlySpan<byte> s, ref ushort count)
    {{
        {3}
    }}

    public bool Write(Span<byte> s, ref ushort count)
    {{
        bool isSuccess = true;

        {4}

        return isSuccess;
    }}
}}

public List<{0}> {1}s = new List<{0}>();
";


        // ----------------------------------------------------------------------------------------------------------
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
        // {1} 변수 자료형(바이트 크기)
        public static string readByteFormat =
@"
this.{0} = ({1})arraySegement.Array[arraySegement.Offset + count];
count += sizeof({1});
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
        // {0} 리스트 정의(struct)
        // {1} 리스트 이름(변수)
        public static string readListFormat =
@"
this.{1}s.Clear();

ushort {1}Length = BitConverter.ToUInt16(s.Slice(count, s.Length - count));
count += sizeof(ushort);

for (int i = 0; i < {1}Length; i++)
{{
    {0} {1} = new {0}();
    {1}.Read(s, ref count);

    {1}s.Add({1});
}}
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
        // {1} 변수 자료형(바이트 크기)
        public static string writeByteFormat =
@"
openSegement.Array[openSegement.Offset + count] = (byte)this.{0};
count += sizeof({1});
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


        // ----------------------------------------------------------------------------------------------------------
        // {0} 리스트 정의(struct)
        // {1} 리스트 이름(변수)
        public static string writeListFormat =
@"
// list 보낼 때
isSuccess &= BitConverter.TryWriteBytes(s.Slice(count, s.Length - count), (ushort)this.{1}s.Count);
count += sizeof(ushort); // packet.{1}s에서 리스트 크기 알려주는 부분

foreach ({0} {1} in this.{1}s)
{{
    isSuccess &= {1}.Write(s, ref count);
}}
";
    }
}
