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

        // send할 내역들 큐로 관리
        Queue<byte[]> _sendQueue = new Queue<byte[]>();
        bool _pending = false;
        SocketAsyncEventArgs _sendArgs = new SocketAsyncEventArgs();

        public void Start(Socket socket)
        {
            _socket = socket;
            SocketAsyncEventArgs receiveArgs = new SocketAsyncEventArgs();
            receiveArgs.Completed += new EventHandler<SocketAsyncEventArgs>(OnReceiveCompleted);
            receiveArgs.SetBuffer(new byte[1024], 0, 1024);

            _sendArgs.Completed += new EventHandler<SocketAsyncEventArgs>(OnSendCompleted);

            RegisterReceive(receiveArgs);
        }

        public void Send(byte[] sendBuffer)
        {
            // 락(멀티스레드 동작)
            lock (_lock)
            {
                _sendQueue.Enqueue(sendBuffer);
                if (_pending == false)
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
        private void OnSendCompleted(object sender, SocketAsyncEventArgs args)
        {
            lock(_lock)
            {
                if (args.BytesTransferred > 0 && args.SocketError == SocketError.Success)
                {
                    try
                    {
                        // 큐에 사람있어요
                        if(_sendQueue.Count > 0)
                        {
                            // 처리해
                            RegisterSend();
                        }
                        else
                        {
                            _pending = false;
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

        private void RegisterSend()
        {
            // true면 다른 스레드가 RegisterSend() 동작 못하게
            _pending = true;

            byte[] buffer = _sendQueue.Dequeue();
            _sendArgs.SetBuffer(buffer, 0, buffer.Length);

            bool pending = _socket.SendAsync(_sendArgs);
            if (pending == false)
            {
                OnSendCompleted(null, _sendArgs);
            }
        }

        private void RegisterReceive(SocketAsyncEventArgs args)
        {
            bool pending = _socket.ReceiveAsync(args);
            if (pending == false)
            {
                OnReceiveCompleted(null, args);
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

                    RegisterReceive(args);
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
