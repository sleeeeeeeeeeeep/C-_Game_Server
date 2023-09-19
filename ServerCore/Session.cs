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

        public void Start(Socket socket)
        {
            _socket = socket;
            SocketAsyncEventArgs receiveArgs = new SocketAsyncEventArgs();
            receiveArgs.Completed += new EventHandler<SocketAsyncEventArgs>(OnReceiveCompleted);

            receiveArgs.SetBuffer(new byte[1024], 0, 1024);

            RegisterReceive(receiveArgs);
        }

        public void Send(byte[] sendBuffer)
        {
            _socket.Send(sendBuffer);
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
        private void RegisterReceive(SocketAsyncEventArgs args)
        {
            bool pending = _socket.ReceiveAsync(args);
            if (pending == false)
            {
                OnReceiveCompleted(null, args);
            }

        }

        private void OnReceiveCompleted(object semder, SocketAsyncEventArgs args)
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
                    Console.WriteLine($"fail: {e}");
                }
            }
            else
            {

            }
        }
        #endregion
    }
}
