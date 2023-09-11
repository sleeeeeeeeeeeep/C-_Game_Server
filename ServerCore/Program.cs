namespace ServerCore
{
    // 메모리 배리어
    // * 코드 재배치 못하게
    // * 가시성: 공유 변수 값 동기화

    // 1. full memory barrier: store/load 둘 다 막음 -> Thread.MemoryBarrier()
    // 2. store memory barrier: store 막음
    // 3. load memory barrier: load 막음

    internal class Program
    {
        static int x = 0;
        static int y = 0;
        static int result1 = 0;
        static int result2 = 0;


        static void Thread_1()
        {
            y = 1; // store y

            Thread.MemoryBarrier(); // 최적화 단계에서 순서 변경 못하도록

            result1 = x;  // load z
        }

        static void Thread_2()
        {
            x = 1; // store x

            Thread.MemoryBarrier(); // 최적화 단계에서 순서 변경 못하도록

            result2 = y;  // load y 
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

                // 메모리 배리어 쓰면 아래 조건 성립 안함
                if(result1 == 0 && result2 == 0)
                {
                    break;
                }
            }

            Console.WriteLine($"{count}");
        }
    }
}