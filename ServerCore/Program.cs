using System.Threading;

namespace ServerCore
{
    internal class Program
    {
        static int number = 0;

        static void Thread_1()
        {
            for(int i = 0; i < 100000; i++) 
            {
                // number++ 원자성 보장하기 위해 사용, 속도 느려짐
                Interlocked.Increment(ref number);
            }
        }

        static void Thread_2()
        {
            for (int i = 0; i < 100000; i++)
            {
                // number-- 원자성 보장하기 위해 사용, 속도 느려짐
                Interlocked.Decrement(ref number);
            }
        }

        static void Main(string[] args)
        {
            Task t1 = new Task(Thread_1);
            Task t2 = new Task(Thread_2);

            t1.Start();
            t2.Start();

            Task.WaitAll(t1, t2);

            Console.WriteLine(number);
        }
    }
}