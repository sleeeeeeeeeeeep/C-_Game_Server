namespace ServerCore
{
    internal class Program
    {
        static int x = 0;
        static int y = 0;
        static int result1 = 0;
        static int result2 = 0;


        static void Thread_1()
        {
            y = 1;
            result1 = x;
        }

        static void Thread_2()
        {
            x = 1;
            result2 = y;
        }

        static void Main(string[] args)
        {
            int count = 0;
            while (true)
            {
                count++;
                x = y = result1 = result2 = 0;

                Task t1 = new Task(Thread_1);
                Task t2 = new Task(Thread_2);

                t1.Start();
                t2.Start();
                
                Task.WaitAll(t1, t2);

                // 하드웨어 최적화 때문에 아래 조건 성립
                if(result1 == 0 && result2 == 0)
                {
                    break;
                }
            }

            Console.WriteLine($"{count}");
        }
    }
}