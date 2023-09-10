namespace ServerCore
{
    internal class Program
    {
        static void MainThread(Object state)
        {   
            for(int i = 0; i < 10;  i++)
            {
                Console.WriteLine("thread, " + i);
            }
        }

        static void Main(string[] args)
        {         
            // 짧은 작업은 부담이 적은 스레드 풀 사용
            ThreadPool.SetMinThreads(1, 1);
            ThreadPool.SetMaxThreads(5, 5);

            // 모든 스레드 풀 사용
            for (int i = 0; i < 5; i++)
            {
                // 얘도 스레드 풀에서 관리
                Task t = new Task(() => { while (true) { } }, TaskCreationOptions.LongRunning); // 긴 작업이라고 지정
                t.Start();
            }
            // 긴 작업이라고 지정하면 동작함(지정 안하면 X)
            ThreadPool.QueueUserWorkItem(MainThread);

            //// 모든 스레드 풀 사용
            //for (int i = 0;i < 5;i++)
            //{
            //    ThreadPool.QueueUserWorkItem((obj) => { while (true) { } });
            //}
            //// 동작 안함
            //ThreadPool.QueueUserWorkItem(MainThread);

            //Thread t = new Thread(MainThread);
            //t.Name = "Test thread";
            //t.IsBackground = true; // 기본: false
            //t.Start();
            
            //Console.WriteLine("wait thread");

            //t.Join(); // 스레드 종료까지 대기

            Console.WriteLine("Hello, World!");
            while (true) { }
        }
    }
}