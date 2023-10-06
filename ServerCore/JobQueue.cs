﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServerCore
{
    public interface IJobQueue
    {
        void Push(Action job);
    }

    public class JobQueue : IJobQueue
    {
        Queue<Action> _jobQueue = new Queue<Action>();
        object _lock = new object();
        bool _flush = false;

        public void Push(Action job)
        {
            bool flush = false;

            lock (_lock)
            {
                _jobQueue.Enqueue(job);

                if (_flush == false)
                {
                    flush = _flush = true;
                }
            }

            if (flush)
            {
                Flush();
            }
        }

        private void Flush()
        {
            while(true)
            {
                Action action = Pop();
                if(action == null) 
                {
                    return;
                }

                action.Invoke();
            }
        }

        private Action Pop() 
        { 
            if(_jobQueue.Count == 0) 
            {
                _flush = false;
                return null;
            }

            return _jobQueue.Dequeue(); 
        }
    }
}
