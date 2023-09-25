using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace ServerCore
{
    // 패킷 구성: [size(2)] [packetId(2)] [...] [size(2)] [packetId(2)] [...] [size(2)] [packetId(2)] [...]
    public abstract class PacketSession: Session
    {
        public static readonly int HeaderSize = 2;

        // sealed: 상속받은 애가 오버라이드 못하게
        public sealed override int OnReceived(ArraySegment<byte> buffer)
        {
            int processLength = 0;

            while (true)
            {
                // 헤더 크기도 못받았으면 break
                if (buffer.Count < HeaderSize) 
                {
                    break;
                }

                ushort dataSize = BitConverter.ToUInt16(buffer.Array, buffer.Offset); // 패킷 사이즈

                // 패킷이 전부 안왔으면 break
                if (buffer.Count < dataSize)
                {
                    break;
                }

                // 패킷 처리 가능하면 얘 실행
                OnReceivedPacket(new ArraySegment<byte> (buffer.Array, buffer.Offset, dataSize));

                processLength += dataSize;
                
                // 처리한 버퍼 부분 날리기(다시 버퍼 구성)
                buffer = new ArraySegment<byte>(buffer.Array, buffer.Offset + dataSize, buffer.Count - dataSize);
            }

            return processLength;
        }

        public abstract void OnReceivedPacket(ArraySegment<byte> buffer);
    }

    public abstract class Session
    {
        Socket _socket;
        int _disconnected = 0;

        ReceiveBuffer _receivebuffer = new ReceiveBuffer(1024);

        object _lock = new object();

        SocketAsyncEventArgs _receiveArgs = new SocketAsyncEventArgs();

        // send할 내역들 큐로 관리
        Queue<ArraySegment<byte>> _sendQueue = new Queue<ArraySegment<byte>>();
        List<ArraySegment<byte>> _pendingList = new List<ArraySegment<byte>>(); // 큐에 있었던 모든 데이터
        SocketAsyncEventArgs _sendArgs = new SocketAsyncEventArgs();

        public abstract void OnConnected(EndPoint endPoint);
        public abstract int OnReceived(ArraySegment<byte> buffer);
        public abstract void OnSend(int numOfBytes);
        public abstract void OnDisconnected(EndPoint endPoint);

        public void Start(Socket socket)
        {
            _socket = socket;
            
            _receiveArgs.Completed += new EventHandler<SocketAsyncEventArgs>(OnReceiveCompleted);
            _sendArgs.Completed += new EventHandler<SocketAsyncEventArgs>(OnSendCompleted);

            RegisterReceive();
        }

        // send 과정
        // send 호출 > 큐에 일감 넣음 > 앞에 예약된 애들 없으면 RegisterSend()
        // -> 큐에 있는 애들 다 빼서 _pendingList에 넣음 -> _pendingList를 _sendArgs.BufferList에 넣음
        // -> sendAsync로 보냄 -> 보냈으면 OnSendCompleted() -> _pendingList, _sendArgs.BufferList 깔끔하게 지움
        // -> 보내는 동안에 다른 애가 큐에 집어넣으면 다시 RegisterSend()...
        public void Send(ArraySegment<byte> sendBuffer)
        {
            // 락(멀티스레드 동작)
            lock (_lock)
            {
                _sendQueue.Enqueue(sendBuffer);
                if (_pendingList.Count == 0)
                {
                    RegisterSend();
                }
            }
        }

        public void Disconnect()
        {
            // 다른 스레드가 연결 해제 시도했는지 확인
            int flag = Interlocked.Exchange(ref _disconnected, 1);
            if (flag == 1)
            {
                return;
            }

            // 연결 해제
            OnDisconnected(_socket.RemoteEndPoint);
            _socket.Shutdown(SocketShutdown.Both);
            _socket.Close();
        }

        #region 네트워크 통신
        private void RegisterSend()
        {
            while(_sendQueue.Count > 0)
            {
                ArraySegment<byte> data = _sendQueue.Dequeue();
                _pendingList.Add(data);
            }
            _sendArgs.BufferList = _pendingList;

            bool pending = _socket.SendAsync(_sendArgs);
            if (pending == false)
            {
                OnSendCompleted(null, _sendArgs);
            }
        }

        private void OnSendCompleted(object sender, SocketAsyncEventArgs args)
        {
            lock (_lock)
            {
                if (args.BytesTransferred > 0 && args.SocketError == SocketError.Success)
                {
                    try
                    {
                        _sendArgs.BufferList = null;
                        _pendingList.Clear();

                        OnSend(_sendArgs.BytesTransferred);

                        // 큐에 사람있어요
                        if (_sendQueue.Count > 0)
                        {
                            // 처리해
                            RegisterSend();
                        }
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine($"send fail: {e}");
                    }
                }
                else
                {
                    Disconnect();
                }
            }
        }

        private void RegisterReceive()
        {
            _receivebuffer.Clean();

            ArraySegment<byte> segment = _receivebuffer.WriteSegment;
            _receiveArgs.SetBuffer(segment.Array, segment.Offset, segment.Count);

            bool pending = _socket.ReceiveAsync(_receiveArgs);
            if (pending == false)
            {
                OnReceiveCompleted(null, _receiveArgs);
            }

        }

        private void OnReceiveCompleted(object sender, SocketAsyncEventArgs args)
        {
            // 데이터 받기 성공
            if(args.BytesTransferred > 0 && args.SocketError == SocketError.Success)
            {
                try
                {
                    if(_receivebuffer.OnWrite(args.BytesTransferred) == false)
                    {
                        Disconnect();
                        return;
                    }

                    int processLength = OnReceived(_receivebuffer.ReadSegment);
                    if(processLength < 0 || processLength> _receivebuffer.DataSize) 
                    {
                        Disconnect();
                        return;
                    }

                    if (_receivebuffer.OnRead(processLength) == false)
                    {
                        Disconnect();
                        return;
                    }

                    RegisterReceive();
                }
                catch (Exception e)
                {
                    Console.WriteLine($"recive fail: {e}");
                }
            }
            else
            {
                Disconnect();
            }
        }
        #endregion
    }
}
