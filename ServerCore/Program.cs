using System.Threading;

namespace ServerCore
{
    // thread local storage
    internal class Program
    {
        static ThreadLocal<string> ThreadName = new ThreadLocal<string>(() => { 
            return Thread.CurrentThread.ManagedThreadId.ToString();
        });

        static void GetThreadName()
        {
            bool isRepeat = ThreadName.IsValueCreated;

            if(isRepeat)
            {
                Console.WriteLine(ThreadName.Value + "repeat");
            }
            else
            {
                Console.WriteLine(ThreadName.Value);
            }

            Thread.Sleep(1000);

            Console.WriteLine(ThreadName.Value);
        }

        static void Main(string[] args)
        {
            ThreadPool.SetMinThreads(1, 1);
            ThreadPool.SetMaxThreads(3, 3);

            Parallel.Invoke(GetThreadName, GetThreadName, GetThreadName, GetThreadName, GetThreadName, GetThreadName);
        
            ThreadName.Dispose();
        }
        
    }
}