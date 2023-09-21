using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace ServerCore
{
    internal class Session
    {
        Socket _socket;
        int _disconnected = 0;

        object _lock = new object();

        SocketAsyncEventArgs _receiveArgs = new SocketAsyncEventArgs();

        // send할 내역들 큐로 관리
        Queue<byte[]> _sendQueue = new Queue<byte[]>();
        List<ArraySegment<byte>> _pendingList = new List<ArraySegment<byte>>(); // 큐에 있었던 모든 데이터
        SocketAsyncEventArgs _sendArgs = new SocketAsyncEventArgs();


        public void Start(Socket socket)
        {
            _socket = socket;
            
            _receiveArgs.Completed += new EventHandler<SocketAsyncEventArgs>(OnReceiveCompleted);
            _receiveArgs.SetBuffer(new byte[1024], 0, 1024);

            _sendArgs.Completed += new EventHandler<SocketAsyncEventArgs>(OnSendCompleted);

            RegisterReceive();
        }

        // send 과정
        // send 호출 > 큐에 일감 넣음 > 앞에 예약된 애들 없으면 RegisterSend()
        // -> 큐에 있는 애들 다 빼서 _pendingList에 넣음 -> _pendingList를 _sendArgs.BufferList에 넣음
        // -> sendAsync로 보냄 -> 보냈으면 OnSendCompleted() -> _pendingList, _sendArgs.BufferList 깔끔하게 지움
        // -> 보내는 동안에 다른 애가 큐에 집어넣으면 다시 RegisterSend()...
        public void Send(byte[] sendBuffer)
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
            _socket.Shutdown(SocketShutdown.Both);
            _socket.Close();
        }

        #region 네트워크 통신
        private void RegisterSend()
        {
            while(_sendQueue.Count > 0)
            {
                byte[] data = _sendQueue.Dequeue();
                _pendingList.Add(new ArraySegment<byte>(data, 0, data.Length));
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

                        Console.WriteLine($"Transfreed bytes: {_sendArgs.BytesTransferred}");

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
                    string recvData = Encoding.UTF8.GetString(args.Buffer, args.Offset, args.BytesTransferred);
                    Console.WriteLine($"client: {recvData}");

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
