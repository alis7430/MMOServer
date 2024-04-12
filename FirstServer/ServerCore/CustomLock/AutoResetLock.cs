using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServerCore.CustomLock
{
    /// <summary>
    /// AutoResetEvent는 커널에게 제어 획득을 알리도록 요청함
    /// </summary>
    internal class AutoResetLock
    {
        // 첫 인자는 열린상태로 시작할 것인지 닫힌 상태로 시작할 것인지
        AutoResetEvent _available = new AutoResetEvent(true);

        public void Acquire()
        {
            _available.WaitOne(); // 입장 시도

            // p.s) flag = false로 만들어주는 함수가 있지만 WaitOne에서 자동으로 처리
            // _available.Reset();
        }

        public void Release()
        {
            _available.Set(); // flag = true
        }
    }

    /// <summary>
    /// ManualResetEvent는 Lock의 입장과 문을 닫는 시나리오가 2단계로 이루어져있어 원자성이 없음
    /// 대기전용 스레드를 만들 때 쓸 수 있음
    /// </summary>
    internal class ManualResetLock
    {
        ManualResetEvent _available = new ManualResetEvent(true);

        public void Acquire()
        {
            _available.WaitOne();   // 입장 시도
            _available.Reset();     // 문을 닫는다
        }

        public void Release()
        {
            _available.Set();       // 문을 열어준다
        }
    }

    internal class TestAutoResetEvent
    {
        static int _num = 0;
        //static AutoResetLock _lock = new AutoResetLock();
        static ManualResetLock _lock = new ManualResetLock();
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

    /// <summary>
    /// Mutex는 boolean으로 이루어진게 아니라 여러 정보를 가지고있다.
    /// ThreadId도 가지고 있고, 몇번 lock을 했는지에 관한 정보도 가지고 있다.
    /// 단, 다양한 정보를 가지고 있는 대신 AutoResetEvent에 비해 느리다.
    /// </summary>
    internal class MutexTest
    {
        static int _num = 0;
        static Mutex _lock = new Mutex();

        public static void Run()
        {
            Task t1 = new Task(() =>
            {
                for (int i = 0; i < 10000; i++)
                {
                    _lock.WaitOne(); // 문을 닫는다
                    _num++;
                    _lock.ReleaseMutex(); // 문을 열어준다
                }
            });

            Task t2 = new Task(() =>
            {
                for (int i = 0; i < 10000; i++)
                {
                    _lock.WaitOne();
                    _num--;
                    _lock.ReleaseMutex();
                }
            });

            t1.Start();
            t2.Start();
            Task.WaitAll(t1, t2);

            Console.WriteLine(_num);
        }
    }
}
