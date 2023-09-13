using System.Threading;

namespace ServerCore
{
    // spinlock 예제
    class SpinLock
    {
        // lock state
        volatile int _locked = 0;

        // lock
        public void Acquire()
        {
            while(true)
            {
                //// original: _locked가 가지고 있던 원래 값
                //// Interlocked.Exchange(ref _locked, 1); -> _locked에 1 삽입
                //int original = Interlocked.Exchange(ref _locked, 1);

                //// 다른 스레드에서 락을 걸지 않았을 때(1이면 락이 걸린 것)
                //if (original == 0)
                //{
                //    break;
                //}

                int expected = 0;
                int desired = 1;
                // Interlocked.Exchange(ref _locked, 1, 0); -> _locked와 0(두 번째 인자)이 같으면 _locked에 1 삽입
                int original = Interlocked.CompareExchange(ref _locked, desired, expected);
                if (original == expected)
                {
                    break;
                }
            }
            
        }

        // unlock
        public void Relase()
        {
            _locked = 0;
        }
    }

    internal class Program
    {
        static int _num = 0;
        static SpinLock _lock = new SpinLock();

        static void Thread_1()
        {
            for (int i = 0; i < 100000; i++)
            {
                _lock.Acquire();
                _num++;
                _lock.Relase();
            }
        }

        static void Thread_2()
        {
            for (int i = 0; i < 100000; i++)
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