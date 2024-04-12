using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServerCore.CustomLock
{
    // ------------ Lock Policy ------------
    // Allow recursively lock (YES) WriteLock -> WriteLock OK, WriteLock -> ReadLock OK, ReadLock -> WriteLock NO
    // Spin lock policy (5000 count -> yield)
    // -------------------------------------
    /// <summary>
    /// 상호배제를 위한 락
    /// </summary>
    class ReaderWriterLock
    {
        const int EMPTY_FLAG = 0x00000000;
        const int WRITE_MASK = 0x7FFF0000;
        const int READ_MASK = 0x0000FFFF;
        const int MAX_SPIN_COUNT = 5000;

        // flag is divided into bits with specific goals, total 32bit (4byte)
        // [unused(1)] [WriteThreadId(15)] [ReadCount(16)]
        int _flag = EMPTY_FLAG;
        int _writeCount = 0;

        public void WriteLock()
        {
            // check another thread already has WriteLock
            int lockThreadId = (_flag & WRITE_MASK) >> 16;
            if (Thread.CurrentThread.ManagedThreadId == lockThreadId)
            {
                _writeCount++;
                return;
            }

            // When no one had WriteLock or ReadLock, get lock after competing
            int desired = Thread.CurrentThread.ManagedThreadId << 16 & WRITE_MASK;
            while (true)
            {
                for (int i = 0; i < MAX_SPIN_COUNT; i++)
                {
                    // try and return if succeed
                    //if (_flag == EMPTY_FLAG)
                    //    _flag = desired;
                    if (Interlocked.CompareExchange(ref _flag, desired, EMPTY_FLAG) == EMPTY_FLAG)
                    {
                        _writeCount = 1;
                        return;
                    }
                }
                Thread.Yield();
            }
        }

        public void WriteUnLock()
        {
            // reset flag to empty
            int lockCount = --_writeCount;
            if (lockCount == 0)
                Interlocked.Exchange(ref _flag, EMPTY_FLAG);
        }

        public void ReadLock()
        {
            // check thread already has WriteLock
            int lockThreadId = (_flag & WRITE_MASK) >> 16;
            if (Thread.CurrentThread.ManagedThreadId == lockThreadId)
            {
                Interlocked.Increment(ref _flag);
                return;
            }

            // if no one had WriteLock, add 1 to read count
            while (true)
            {
                for (int i = 0; i < MAX_SPIN_COUNT; i++)
                {
                    //if ((_flag & WRITE_MASK) == 0)
                    //{
                    //    _flag = _flag + 1;
                    //    return;
                    //}
                    int expected = _flag & READ_MASK;
                    if (Interlocked.CompareExchange(ref _flag, expected + 1, expected) == expected)
                        return;
                }
            }
        }

        public void ReadUnlock()
        {
            Interlocked.Decrement(ref _flag);
        }
    }

    internal class RWLockTest
    {
        static volatile int count = 0;
        static ReaderWriterLock _lock = new ReaderWriterLock();
        public static void Run()
        {
            Task t1 = new Task(delegate ()
            {
                for (int i = 0; i < 10000; i++)
                {
                    _lock.WriteLock();
                    count++;
                    _lock.WriteUnLock();
                }
            });

            Task t2 = new Task(delegate ()
            {
                for (int i = 0; i < 10000; i++)
                {
                    _lock.WriteLock();
                    count--;
                    _lock.WriteUnLock();
                }
            });

            t1.Start();
            t2.Start();
            Task.WaitAll(t1, t2);
            Console.WriteLine(count);
        }
    }
}
