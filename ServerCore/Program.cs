using System.Threading;

namespace ServerCore
{
    internal class Program
    {
        // Monitor 사용
        static object moniterLock = new object();

        // 스핀락 클래스 사용(너무 오래 락 걸려있으면 쉬다 옴)
        static SpinLock spinLock = new SpinLock();

        // 느림, 다른 프로그램들 간 락 사용 가능(커널에서 동작하기 때문)
        static Mutex mutex = new Mutex();
        
        // RederWriterLock, write 때만 락 걺
        static ReaderWriterLockSlim rwLock = new ReaderWriterLockSlim();

        static int getValue(int id)
        {
            rwLock.EnterReadLock();
            // 동작
            rwLock.ExitReadLock();
            return 1;
        }

        static void writeValue(int id, int value)
        {
            rwLock.EnterWriteLock();
            // 동작
            rwLock.ExitWriteLock();
        }

        static void Main(string[] args)
        {
            lock (moniterLock)
            {
                // 동작
            }

            bool locked = false;
            try
            {
                spinLock.Enter(ref locked);
            }
            finally
            {
                if (locked)
                {
                    spinLock.Exit();
                }
            }
        }
        
    }
}