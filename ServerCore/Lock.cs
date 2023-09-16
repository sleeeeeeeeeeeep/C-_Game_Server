using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ServerCore
{
    // recursice lock: o
    // - w -> w : 0
    // - w -> r : 0
    // - r -> w : x
    // spin lick: 5000번 시도 -> yield
    internal class Lock
    {
        const int EMPTY_FLAG = 0x00000000;
        const int WRITE_MASK = 0x7FFF0000;
        const int READ_MASK = 0x0000FFFF;
        const int MAX_SPIN_COUNT = 5000;

        // 32비트 구성: [unsued(1)], [WriteThreadId(15)], [ReadCount(16)]
        int flag = EMPTY_FLAG;
        int writeCount = 0;

        public void WriteLock()
        {
            // 동일 스레드가 WriteLock 획득하고 있는지
            int lockThreadId = (flag & WRITE_MASK) >> 16;
            if(lockThreadId == Thread.CurrentThread.ManagedThreadId)
            {
                writeCount++;
                return;
            }

            int desired = (Thread.CurrentThread.ManagedThreadId << 16) & WRITE_MASK; // id 채움, 다른 곳에 값 있으면 0으로 만듬(& 연산)
            while(true)
            {
                for(int i = 0; i < MAX_SPIN_COUNT; i++)
                {
                    // WriteLock 아무도 안잡고 있을 때
                    // read, write 락 안걸려 있으면(비트 채워지지 않았으면) 지금 스레드 id 넣고 리턴
                    if (Interlocked.CompareExchange(ref flag, desired, EMPTY_FLAG) == EMPTY_FLAG)
                    {
                        writeCount = 1;
                        return;
                    }
                }

                // 5000번 기다리면 양보
                Thread.Yield();
            }
        }

        public void WriteUnlock()
        {
            int lockCount = --writeCount;
            if(lockCount == 0)
            // flag에 비어있는 비트 넣음(락 해제)
            Interlocked.Exchange(ref flag, EMPTY_FLAG);
        }

        public void ReadLock() 
        {
            // 동일 스레드가 WriteLock 획득하고 있는지
            int lockThreadId = (flag & WRITE_MASK) >> 16;
            if (lockThreadId == Thread.CurrentThread.ManagedThreadId)
            {
                Interlocked.Increment(ref flag);
                return;
            }

            while (true)
            {
                for(int i = 0; i < MAX_SPIN_COUNT; i++) 
                {
                    // WriteLock 가진 스레드 없으면, ReadCount 1 증가
                    int expected = (flag & READ_MASK);
                    if(Interlocked.CompareExchange(ref flag, expected + 1, expected) == expected)
                    {
                        return;
                    }
                }

                Thread.Yield();
            }  
        }

        public void ReadUnlock()
        {
            // ReadCount 1 감소
            Interlocked.Decrement(ref flag);
        }
    }
}
