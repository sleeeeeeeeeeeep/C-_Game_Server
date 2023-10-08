using ServerCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Server
{
    struct JobTimerElement : IComparable<JobTimerElement>
    {
        public int execTick; // 실행해야 하는 시간
        public Action action; // 실행할 액션

        public int CompareTo(JobTimerElement other)
        {
            return other.execTick - execTick;
        }
    }

    internal class JobTimer
    {
        PriorityQueue<JobTimerElement> _pq = new PriorityQueue<JobTimerElement>();
        object _lock = new object();

        public static JobTimer Instance { get; } = new JobTimer();

        public void Push(Action action, int acterTick = 0) 
        {
            JobTimerElement job;
            job.execTick = System.Environment.TickCount + acterTick;
            job.action = action;

            lock (_lock)
            {
                _pq.Push(job);
            }
        }

        public void Flush()
        {
            while (true)
            {
                int now = System.Environment.TickCount;
                JobTimerElement job;

                lock (_lock)
                {
                    if(_pq.Count == 0) 
                    {
                        break;
                    }

                    job = _pq.Peek();

                    // 아직 실행할 때가 아님
                    if(job.execTick > now)
                    {
                        break;
                    }

                    _pq.Pop();
                }

                job.action.Invoke();
            }
        }
    }
}
