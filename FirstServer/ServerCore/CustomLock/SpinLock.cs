using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServerCore.CustomLock
{
    internal class SpinLock
    {
        volatile int _locked = 0;

        public void Acquire()
        {
            // while(_locked) {}
            // _locked = true
            // 을 사용한 구현은 제대로 동작하지 않음
            // 기다림과 획득, 두단계로 나뉘어져 원자성이 없음

            while (true)
            {
                //int original = Interlocked.Exchange(ref _locked, 1);
                //if (original == 0)
                //    break;

                int expected = 0;
                int desired = 1;
                if (Interlocked.CompareExchange(ref _locked, desired, expected) == expected)
                    break;
            }
        }

        public void Release()
        {
            _locked = 0;
        }
    }

    internal class TestSpinLock
    {
        static int _num = 0;
        static SpinLock _lock = new SpinLock();

        public static void Run()
        {
            Task t1 = new Task(() =>
            {
                for (int i = 0; i < 10000; i++)
                {
                    _lock.Acquire();
                    _num++;
                    _lock.Release();
                }
            });

            Task t2 = new Task(() =>
            {
                for (int i = 0; i < 10000; i++)
                {
                    _lock.Acquire();
                    _num--;
                    _lock.Release();
                }
            });

            t1.Start();
            t2.Start();
            Task.WaitAll(t1, t2);

            Console.WriteLine(_num);
        }
    }

    internal class TestSystemSpinLock
    {
        // library spin lock
        // Threading.SpinLock은 일부 대기후 yield로 유휴상태가 된다
        static System.Threading.SpinLock _spinLock = new System.Threading.SpinLock();
        public static void Run()
        {
            bool lockTaken = false;
            Console.WriteLine(lockTaken.ToString());
            try
            {
                _spinLock.Enter(ref lockTaken);
            }
            finally
            {
                if (lockTaken)
                {
                    Console.WriteLine(lockTaken.ToString());
                    _spinLock.Exit();
                }
            }
        }
    }

}
