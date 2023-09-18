﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace ServerCore
{
    internal class Listener
    {
        Socket _listenSocket;
        Action<Socket> _onAcceptHandler;

        public void Init(IPEndPoint endPoint, Action<Socket> onAcceptHandler)
        {
            _listenSocket = new Socket(endPoint.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
            _onAcceptHandler += onAcceptHandler;

            _listenSocket.Bind(endPoint);
            _listenSocket.Listen(10); // backlog: 최대 대기 수

            SocketAsyncEventArgs args = new SocketAsyncEventArgs();
            args.Completed += new EventHandler<SocketAsyncEventArgs>(OnAcceptCompleted); //SocketAsyncEventArgs 얘 실행되면 콜백으로 OnAcceptCompleted 실행
            RegisterAccept(args);
        }

        private void RegisterAccept(SocketAsyncEventArgs args)
        {
            // 이벤트 초기화
            args.AcceptSocket = null;

            bool pending = _listenSocket.AcceptAsync(args);

            // 바로 접속 됐으면
            if (pending == false)
            {
                OnAcceptCompleted(null, args);
            }
        }

        private void OnAcceptCompleted(object sender, SocketAsyncEventArgs args)
        {
            if(args.SocketError == SocketError.Success) 
            {
                _onAcceptHandler.Invoke(args.AcceptSocket);
            }
            else
            {
                Console.WriteLine(args.SocketError.ToString());
            }

            // 다음 친구 받기 위해 실행
            RegisterAccept(args);
        }

        public Socket Accept()
        {
            // 비동기 처리 -> send, rececive 계열 논블로킹 함수 사용
            _listenSocket.AcceptAsync();
            return _listenSocket.Accept();
        }
    }
}
