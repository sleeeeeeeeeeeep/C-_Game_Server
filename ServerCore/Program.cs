using System.Threading;

namespace ServerCore
{
    // AutoResetEvent
    class Lock
    {
        // 입장 가능한지(true), 하나가 들어오면 false로 바뀜
        AutoResetEvent _available = new AutoResetEvent(true);

        // lock
        public void Acquire()
        {
            _available.WaitOne(); // 입장 시도
            // _available.Reset(); // flag -> false로 변경, WaitOne() 에서 실행됨
        }

        // unlock
        public void Relase()
        {
           _available.Set(); // flag -> true로 변경
        }
    }

    internal class Program
    {
        static int _num = 0;
        static Lock _lock = new Lock();

        static void Thread_1()
        {
            for (int i = 0; i < 10000; i++)
            {
                _lock.Acquire();
                _num++;
                _lock.Relase();
            }
        }

        static void Thread_2()
        {
            for (int i = 0; i < 10000; i++)
            {
                _lock.Acquire();
                _num--;
                _lock.Relase();
            }
        }

        static void Main(string[] args)
        {
            Task t1 = new Task(Thread_1);
            Task t2 = new Task(Thread_2);

            t1.Start();
            t2.Start();

            Task.WaitAll(t1, t2);

            Console.WriteLine(_num);
        }
    }
}