using System.Threading;

namespace ServerCore
{
    internal class Program
    {
        // Monitor 사용
        static object _lock1 = new object();

        // 스핀락 클래스 사용(너무 오래 락 걸려있으면 쉬다 옴)
        static SpinLock _lock2 = new SpinLock();

        // 느림, 다른 프로그램들 간 락 사용 가능(커널에서 동작하기 때문)
        static Mutex _lock3 = new Mutex();
        
        static void Main(string[] args)
        {
            lock (_lock1)
            {
                // 동작
            }

            bool locked = false;
            try
            {
                _lock2.Enter(ref locked);
            }
            finally
            {
                if (locked)
                {
                    _lock2.Exit();
                }
            }
        }
        
    }
}