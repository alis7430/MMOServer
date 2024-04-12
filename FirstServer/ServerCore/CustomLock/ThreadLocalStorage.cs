using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServerCore.CustomLock
{
    internal class ThreadLocalStorage
    {
        // 전역변수이지만 Thread마다 고유한 값을 지닐 수 있음
        static ThreadLocal<string> ThreadName = new ThreadLocal<string>(() =>
                                                { return Thread.CurrentThread.ManagedThreadId.ToString(); });

        public static void InitThreadName()
        {
            //ThreadName.Value = Thread.CurrentThread.ManagedThreadId.ToString();
            bool repeat = ThreadName.IsValueCreated;
            if (repeat)
                Console.WriteLine(ThreadName.Value + "(repeat)");
            else
                Console.WriteLine(ThreadName.Value);
        }
        public static void Run()
        {
            //ThreadPool.SetMinThreads(1, 1);
            //ThreadPool.SetMaxThreads(3, 3);
            Parallel.Invoke(InitThreadName, InitThreadName, InitThreadName, InitThreadName, InitThreadName, InitThreadName);

            ThreadName.Dispose();
        }
    }
}
