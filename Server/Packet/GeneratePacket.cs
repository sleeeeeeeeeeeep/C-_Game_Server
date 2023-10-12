using ServerCore;
using System;
using System.Net;
using System.Text;

public enum PacketID
{
    S_BroadcastEnterGame = 1,
	C_LeaveGame = 2,
	S_BroadcastLeaveGame = 3,
	S_PlayerList = 4,
	C_Move = 5,
	S_BroadcastMove = 6,
	
}

public interface IPacket
{
    ushort Protocol { get;}
    void Read(ArraySegment<byte> segement);
    ArraySegment<byte> Write();
}


public class S_BroadcastEnterGame : IPacket
{
    
	public int playerId;
	public float posX;
	public float posY;
	public float posZ;
	

    public ushort Protocol
	{
		get { return (ushort)PacketID.S_BroadcastEnterGame; }
	}

    public void Read(ArraySegment<byte> arraySegement)
    {
        ushort count = 0;

        ReadOnlySpan<byte> s = new ReadOnlySpan<byte>(arraySegement.Array, arraySegement.Offset, arraySegement.Count);

        count += sizeof(ushort);
        count += sizeof(ushort);

        
		
		this.playerId = BitConverter.ToInt32(s.Slice(count, s.Length - count));
		count += sizeof(int);
		
		this.posX = BitConverter.ToSingle(s.Slice(count, s.Length - count));
		count += sizeof(float);
		
		this.posY = BitConverter.ToSingle(s.Slice(count, s.Length - count));
		count += sizeof(float);
		
		this.posZ = BitConverter.ToSingle(s.Slice(count, s.Length - count));
		count += sizeof(float);
		

    }

    public ArraySegment<byte> Write()
    {
        ArraySegment<byte> openSegement = SendBufferHelper.Open(4096);

        ushort count = 0;
        bool isSuccess = true;

        Span<byte> s = new Span<byte>(openSegement.Array, openSegement.Offset, openSegement.Count);

        count += sizeof(ushort); // 전체 패킷 사이즈 정보

        isSuccess &= BitConverter.TryWriteBytes(s.Slice(count, s.Length - count), (ushort)PacketID.S_BroadcastEnterGame);
        count += sizeof(ushort); // 패킷 아이디(패킷 구분) 바이트 크기

        
		
		isSuccess &= BitConverter.TryWriteBytes(s.Slice(count, s.Length - count), this.playerId);
		count += sizeof(int); // playerId 바이트 크기
		
		isSuccess &= BitConverter.TryWriteBytes(s.Slice(count, s.Length - count), this.posX);
		count += sizeof(float); // posX 바이트 크기
		
		isSuccess &= BitConverter.TryWriteBytes(s.Slice(count, s.Length - count), this.posY);
		count += sizeof(float); // posY 바이트 크기
		
		isSuccess &= BitConverter.TryWriteBytes(s.Slice(count, s.Length - count), this.posZ);
		count += sizeof(float); // posZ 바이트 크기
		

        // 전체 패킷 사이즈 정보는 마지막에
        isSuccess &= BitConverter.TryWriteBytes(s, count);

        if (!isSuccess)
        {
            return null;
        }

        return SendBufferHelper.Close(count);
    }
}

public class C_LeaveGame : IPacket
{
    

    public ushort Protocol
	{
		get { return (ushort)PacketID.C_LeaveGame; }
	}

    public void Read(ArraySegment<byte> arraySegement)
    {
        ushort count = 0;

        ReadOnlySpan<byte> s = new ReadOnlySpan<byte>(arraySegement.Array, arraySegement.Offset, arraySegement.Count);

        count += sizeof(ushort);
        count += sizeof(ushort);

        

    }

    public ArraySegment<byte> Write()
    {
        ArraySegment<byte> openSegement = SendBufferHelper.Open(4096);

        ushort count = 0;
        bool isSuccess = true;

        Span<byte> s = new Span<byte>(openSegement.Array, openSegement.Offset, openSegement.Count);

        count += sizeof(ushort); // 전체 패킷 사이즈 정보

        isSuccess &= BitConverter.TryWriteBytes(s.Slice(count, s.Length - count), (ushort)PacketID.C_LeaveGame);
        count += sizeof(ushort); // 패킷 아이디(패킷 구분) 바이트 크기

        

        // 전체 패킷 사이즈 정보는 마지막에
        isSuccess &= BitConverter.TryWriteBytes(s, count);

        if (!isSuccess)
        {
            return null;
        }

        return SendBufferHelper.Close(count);
    }
}

public class S_BroadcastLeaveGame : IPacket
{
    
	public int playerId;
	

    public ushort Protocol
	{
		get { return (ushort)PacketID.S_BroadcastLeaveGame; }
	}

    public void Read(ArraySegment<byte> arraySegement)
    {
        ushort count = 0;

        ReadOnlySpan<byte> s = new ReadOnlySpan<byte>(arraySegement.Array, arraySegement.Offset, arraySegement.Count);

        count += sizeof(ushort);
        count += sizeof(ushort);

        
		
		this.playerId = BitConverter.ToInt32(s.Slice(count, s.Length - count));
		count += sizeof(int);
		

    }

    public ArraySegment<byte> Write()
    {
        ArraySegment<byte> openSegement = SendBufferHelper.Open(4096);

        ushort count = 0;
        bool isSuccess = true;

        Span<byte> s = new Span<byte>(openSegement.Array, openSegement.Offset, openSegement.Count);

        count += sizeof(ushort); // 전체 패킷 사이즈 정보

        isSuccess &= BitConverter.TryWriteBytes(s.Slice(count, s.Length - count), (ushort)PacketID.S_BroadcastLeaveGame);
        count += sizeof(ushort); // 패킷 아이디(패킷 구분) 바이트 크기

        
		
		isSuccess &= BitConverter.TryWriteBytes(s.Slice(count, s.Length - count), this.playerId);
		count += sizeof(int); // playerId 바이트 크기
		

        // 전체 패킷 사이즈 정보는 마지막에
        isSuccess &= BitConverter.TryWriteBytes(s, count);

        if (!isSuccess)
        {
            return null;
        }

        return SendBufferHelper.Close(count);
    }
}

public class S_PlayerList : IPacket
{
    
	
	public class Player
	{
	    
		public bool isSelf;
		public int playerId;
		public float posX;
		public float posY;
		public float posZ;
		
	
	    public void Read(ReadOnlySpan<byte> s, ref ushort count)
	    {
	        
			
			this.isSelf = BitConverter.ToBoolean(s.Slice(count, s.Length - count));
			count += sizeof(bool);
			
			this.playerId = BitConverter.ToInt32(s.Slice(count, s.Length - count));
			count += sizeof(int);
			
			this.posX = BitConverter.ToSingle(s.Slice(count, s.Length - count));
			count += sizeof(float);
			
			this.posY = BitConverter.ToSingle(s.Slice(count, s.Length - count));
			count += sizeof(float);
			
			this.posZ = BitConverter.ToSingle(s.Slice(count, s.Length - count));
			count += sizeof(float);
			
	    }
	
	    public bool Write(Span<byte> s, ref ushort count)
	    {
	        bool isSuccess = true;
	
	        
			
			isSuccess &= BitConverter.TryWriteBytes(s.Slice(count, s.Length - count), this.isSelf);
			count += sizeof(bool); // isSelf 바이트 크기
			
			isSuccess &= BitConverter.TryWriteBytes(s.Slice(count, s.Length - count), this.playerId);
			count += sizeof(int); // playerId 바이트 크기
			
			isSuccess &= BitConverter.TryWriteBytes(s.Slice(count, s.Length - count), this.posX);
			count += sizeof(float); // posX 바이트 크기
			
			isSuccess &= BitConverter.TryWriteBytes(s.Slice(count, s.Length - count), this.posY);
			count += sizeof(float); // posY 바이트 크기
			
			isSuccess &= BitConverter.TryWriteBytes(s.Slice(count, s.Length - count), this.posZ);
			count += sizeof(float); // posZ 바이트 크기
			
	
	        return isSuccess;
	    }
	}
	
	public List<Player> players = new List<Player>();
	

    public ushort Protocol
	{
		get { return (ushort)PacketID.S_PlayerList; }
	}

    public void Read(ArraySegment<byte> arraySegement)
    {
        ushort count = 0;

        ReadOnlySpan<byte> s = new ReadOnlySpan<byte>(arraySegement.Array, arraySegement.Offset, arraySegement.Count);

        count += sizeof(ushort);
        count += sizeof(ushort);

        
		
		this.players.Clear();
		
		ushort playerLength = BitConverter.ToUInt16(s.Slice(count, s.Length - count));
		count += sizeof(ushort);
		
		for (int i = 0; i < playerLength; i++)
		{
		    Player player = new Player();
		    player.Read(s, ref count);
		
		    players.Add(player);
		}
		

    }

    public ArraySegment<byte> Write()
    {
        ArraySegment<byte> openSegement = SendBufferHelper.Open(4096);

        ushort count = 0;
        bool isSuccess = true;

        Span<byte> s = new Span<byte>(openSegement.Array, openSegement.Offset, openSegement.Count);

        count += sizeof(ushort); // 전체 패킷 사이즈 정보

        isSuccess &= BitConverter.TryWriteBytes(s.Slice(count, s.Length - count), (ushort)PacketID.S_PlayerList);
        count += sizeof(ushort); // 패킷 아이디(패킷 구분) 바이트 크기

        
		
		// list 보낼 때
		isSuccess &= BitConverter.TryWriteBytes(s.Slice(count, s.Length - count), (ushort)this.players.Count);
		count += sizeof(ushort); // packet.players에서 리스트 크기 알려주는 부분
		
		foreach (Player player in this.players)
		{
		    isSuccess &= player.Write(s, ref count);
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

public class C_Move : IPacket
{
    
	public float posX;
	public float posY;
	public float posZ;
	

    public ushort Protocol
	{
		get { return (ushort)PacketID.C_Move; }
	}

    public void Read(ArraySegment<byte> arraySegement)
    {
        ushort count = 0;

        ReadOnlySpan<byte> s = new ReadOnlySpan<byte>(arraySegement.Array, arraySegement.Offset, arraySegement.Count);

        count += sizeof(ushort);
        count += sizeof(ushort);

        
		
		this.posX = BitConverter.ToSingle(s.Slice(count, s.Length - count));
		count += sizeof(float);
		
		this.posY = BitConverter.ToSingle(s.Slice(count, s.Length - count));
		count += sizeof(float);
		
		this.posZ = BitConverter.ToSingle(s.Slice(count, s.Length - count));
		count += sizeof(float);
		

    }

    public ArraySegment<byte> Write()
    {
        ArraySegment<byte> openSegement = SendBufferHelper.Open(4096);

        ushort count = 0;
        bool isSuccess = true;

        Span<byte> s = new Span<byte>(openSegement.Array, openSegement.Offset, openSegement.Count);

        count += sizeof(ushort); // 전체 패킷 사이즈 정보

        isSuccess &= BitConverter.TryWriteBytes(s.Slice(count, s.Length - count), (ushort)PacketID.C_Move);
        count += sizeof(ushort); // 패킷 아이디(패킷 구분) 바이트 크기

        
		
		isSuccess &= BitConverter.TryWriteBytes(s.Slice(count, s.Length - count), this.posX);
		count += sizeof(float); // posX 바이트 크기
		
		isSuccess &= BitConverter.TryWriteBytes(s.Slice(count, s.Length - count), this.posY);
		count += sizeof(float); // posY 바이트 크기
		
		isSuccess &= BitConverter.TryWriteBytes(s.Slice(count, s.Length - count), this.posZ);
		count += sizeof(float); // posZ 바이트 크기
		

        // 전체 패킷 사이즈 정보는 마지막에
        isSuccess &= BitConverter.TryWriteBytes(s, count);

        if (!isSuccess)
        {
            return null;
        }

        return SendBufferHelper.Close(count);
    }
}

public class S_BroadcastMove : IPacket
{
    
	public int playerId;
	public float posX;
	public float posY;
	public float posZ;
	

    public ushort Protocol
	{
		get { return (ushort)PacketID.S_BroadcastMove; }
	}

    public void Read(ArraySegment<byte> arraySegement)
    {
        ushort count = 0;

        ReadOnlySpan<byte> s = new ReadOnlySpan<byte>(arraySegement.Array, arraySegement.Offset, arraySegement.Count);

        count += sizeof(ushort);
        count += sizeof(ushort);

        
		
		this.playerId = BitConverter.ToInt32(s.Slice(count, s.Length - count));
		count += sizeof(int);
		
		this.posX = BitConverter.ToSingle(s.Slice(count, s.Length - count));
		count += sizeof(float);
		
		this.posY = BitConverter.ToSingle(s.Slice(count, s.Length - count));
		count += sizeof(float);
		
		this.posZ = BitConverter.ToSingle(s.Slice(count, s.Length - count));
		count += sizeof(float);
		

    }

    public ArraySegment<byte> Write()
    {
        ArraySegment<byte> openSegement = SendBufferHelper.Open(4096);

        ushort count = 0;
        bool isSuccess = true;

        Span<byte> s = new Span<byte>(openSegement.Array, openSegement.Offset, openSegement.Count);

        count += sizeof(ushort); // 전체 패킷 사이즈 정보

        isSuccess &= BitConverter.TryWriteBytes(s.Slice(count, s.Length - count), (ushort)PacketID.S_BroadcastMove);
        count += sizeof(ushort); // 패킷 아이디(패킷 구분) 바이트 크기

        
		
		isSuccess &= BitConverter.TryWriteBytes(s.Slice(count, s.Length - count), this.playerId);
		count += sizeof(int); // playerId 바이트 크기
		
		isSuccess &= BitConverter.TryWriteBytes(s.Slice(count, s.Length - count), this.posX);
		count += sizeof(float); // posX 바이트 크기
		
		isSuccess &= BitConverter.TryWriteBytes(s.Slice(count, s.Length - count), this.posY);
		count += sizeof(float); // posY 바이트 크기
		
		isSuccess &= BitConverter.TryWriteBytes(s.Slice(count, s.Length - count), this.posZ);
		count += sizeof(float); // posZ 바이트 크기
		

        // 전체 패킷 사이즈 정보는 마지막에
        isSuccess &= BitConverter.TryWriteBytes(s, count);

        if (!isSuccess)
        {
            return null;
        }

        return SendBufferHelper.Close(count);
    }
}

