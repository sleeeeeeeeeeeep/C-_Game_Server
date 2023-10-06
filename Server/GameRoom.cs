using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Server.Session;
using ServerCore;

namespace Server
{
    internal class GameRoom : IJobQueue
    {
        List<ClientSession> _sessions = new List<ClientSession>();
        JobQueue _jobqueue = new JobQueue();

        public void Push(Action job)
        {
            _jobqueue.Push(job);
        }

        public void BroadCast(ClientSession session, string chat)
        {
            S_Chat packet = new S_Chat();
            packet.playerId = session.SessionId; 
            packet.chat = chat + " " + packet.playerId;

            ArraySegment<byte> segment = packet.Write();

            foreach(ClientSession s in _sessions)
            {
                s.Send(segment);
            }
        }

        public void Enter(ClientSession session)
        {
            _sessions.Add(session);
            session.Room = this;
        }

        public void Leave(ClientSession session)
        {
            _sessions.Remove(session);
        }
    }
}
