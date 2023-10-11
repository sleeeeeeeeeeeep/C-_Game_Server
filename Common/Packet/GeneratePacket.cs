using ServerCore;
using System;
using System.Net;
using System.Text;

public enum PacketID
{
    C_Chat = 1,
	S_Chat = 2,
	
}

public interface IPacket
{
    ushort Protocol { get;}
    void Read(ArraySegment<byte> segement);
    ArraySegment<byte> Write();
}


class C_Chat : IPacket
{
    
	public string chat;
	

    public ushort Protocol
	{
		get { return (ushort)PacketID.C_Chat; }
	}

    public void Read(ArraySegment<byte> arraySegement)
    {
        ushort count = 0;

        ReadOnlySpan<byte> s = new ReadOnlySpan<byte>(arraySegement.Array, arraySegement.Offset, arraySegement.Count);

        count += sizeof(ushort);
        count += sizeof(ushort);

        
		
		ushort chatLength = BitConverter.ToUInt16(s.Slice(count, s.Length - count));
		count += sizeof(ushort);
		
		this.chat = Encoding.Unicode.GetString(s.Slice(count, chatLength));
		count += chatLength;
		

    }

    public ArraySegment<byte> Write()
    {
        ArraySegment<byte> openSegement = SendBufferHelper.Open(4096);

        ushort count = 0;
        bool isSuccess = true;

        Span<byte> s = new Span<byte>(openSegement.Array, openSegement.Offset, openSegement.Count);

        count += sizeof(ushort); // 전체 패킷 사이즈 정보

        isSuccess &= BitConverter.TryWriteBytes(s.Slice(count, s.Length - count), (ushort)PacketID.C_Chat);
        count += sizeof(ushort); // 패킷 아이디(패킷 구분) 바이트 크기

        
		
		// 스트링 보낼 때: 스트링 크기 먼저 보내고 -> 스트링 내용 보냄
		ushort chatLength = (ushort)Encoding.Unicode.GetBytes(
		    this.chat,
		    0,
		    this.chat.Length,
		    openSegement.Array,
		    openSegement.Offset + count + sizeof(ushort)
		);
		
		isSuccess &= BitConverter.TryWriteBytes(s.Slice(count, s.Length - count), chatLength);
		count += sizeof(ushort); // chat에 해당하는 스트링 크기 알려주는 부분
		count += chatLength; // chat 바이트 크기
		

        // 전체 패킷 사이즈 정보는 마지막에
        isSuccess &= BitConverter.TryWriteBytes(s, count);

        if (!isSuccess)
        {
            return null;
        }

        return SendBufferHelper.Close(count);
    }
}

class S_Chat : IPacket
{
    
	public int playerId;
	public string chat;
	

    public ushort Protocol
	{
		get { return (ushort)PacketID.S_Chat; }
	}

    public void Read(ArraySegment<byte> arraySegement)
    {
        ushort count = 0;

        ReadOnlySpan<byte> s = new ReadOnlySpan<byte>(arraySegement.Array, arraySegement.Offset, arraySegement.Count);

        count += sizeof(ushort);
        count += sizeof(ushort);

        
		
		this.playerId = BitConverter.ToInt32(s.Slice(count, s.Length - count));
		count += sizeof(int);
		
		ushort chatLength = BitConverter.ToUInt16(s.Slice(count, s.Length - count));
		count += sizeof(ushort);
		
		this.chat = Encoding.Unicode.GetString(s.Slice(count, chatLength));
		count += chatLength;
		

    }

    public ArraySegment<byte> Write()
    {
        ArraySegment<byte> openSegement = SendBufferHelper.Open(4096);

        ushort count = 0;
        bool isSuccess = true;

        Span<byte> s = new Span<byte>(openSegement.Array, openSegement.Offset, openSegement.Count);

        count += sizeof(ushort); // 전체 패킷 사이즈 정보

        isSuccess &= BitConverter.TryWriteBytes(s.Slice(count, s.Length - count), (ushort)PacketID.S_Chat);
        count += sizeof(ushort); // 패킷 아이디(패킷 구분) 바이트 크기

        
		
		isSuccess &= BitConverter.TryWriteBytes(s.Slice(count, s.Length - count), this.playerId);
		count += sizeof(int); // playerId 바이트 크기
		
		// 스트링 보낼 때: 스트링 크기 먼저 보내고 -> 스트링 내용 보냄
		ushort chatLength = (ushort)Encoding.Unicode.GetBytes(
		    this.chat,
		    0,
		    this.chat.Length,
		    openSegement.Array,
		    openSegement.Offset + count + sizeof(ushort)
		);
		
		isSuccess &= BitConverter.TryWriteBytes(s.Slice(count, s.Length - count), chatLength);
		count += sizeof(ushort); // chat에 해당하는 스트링 크기 알려주는 부분
		count += chatLength; // chat 바이트 크기
		

        // 전체 패킷 사이즈 정보는 마지막에
        isSuccess &= BitConverter.TryWriteBytes(s, count);

        if (!isSuccess)
        {
            return null;
        }

        return SendBufferHelper.Close(count);
    }
}

