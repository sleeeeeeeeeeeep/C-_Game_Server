using ServerCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

internal class PacketManager
{
    #region Singleton
    static PacketManager _instatce = new PacketManager();
    public static PacketManager Instatnce
    {
        get{ return _instatce; }
    }
    #endregion

    PacketManager() 
    {
        Register();
    }

    Dictionary<ushort, Action<PacketSession, ArraySegment<byte>>> _onRecv
        = new Dictionary<ushort, Action<PacketSession, ArraySegment<byte>>>();

    Dictionary<ushort, Action<PacketSession, IPacket>> _handler
        = new Dictionary<ushort, Action<PacketSession, IPacket>>();

    public void Register()
    {
        _onRecv.Add((ushort)PacketID.S_Chat, MakePacket<S_Chat>);
        _handler.Add((ushort)PacketID.S_Chat, PacketHandler.S_ChatHandler);


    }

    public void OnRecvPacket(PacketSession session, ArraySegment<byte> buffer)
    {
        ushort count = 0;

        ushort size = BitConverter.ToUInt16(buffer.Array, buffer.Offset);
        count += 2;
        ushort id = BitConverter.ToUInt16(buffer.Array, buffer.Offset + count);
        count += 2;

        Action<PacketSession, ArraySegment<byte>> action = null;
        if (_onRecv.TryGetValue(id, out action))
        {
            action.Invoke(session, buffer);
        }

        // Console.WriteLine($"패킷사이즈: {size}, 패킷아이디: {id}");
    }

    private void MakePacket<T>(PacketSession session, ArraySegment<byte> buffer) where T : IPacket, new ()
    {
        T packet = new T();
        packet.Read(buffer);

        Action<PacketSession, IPacket> action = null;
        if(_handler.TryGetValue(packet.Protocol, out action))
        {
            action.Invoke(session, packet);
        }
    }
}
