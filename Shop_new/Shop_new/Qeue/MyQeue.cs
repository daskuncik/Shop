using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Concurrent;

namespace Shop_new.Qeue
{
    public class MyQeue
    {
        private static ConcurrentQueue<Func<Task<bool>>> allTasks = new ConcurrentQueue<Func<Task<bool>>>();
        private static Timer timer = new Timer(TimerCallback, null, 0, 50);

        public static void Retry(Func<Task<bool>> func) => allTasks.Enqueue(func);

        private static void TimerCallback(object o)
        {
            Func<Task<bool>> task = null;
            while (allTasks.Count > 0)
                if (allTasks.TryDequeue(out task) && !task.Invoke().Result)
                    allTasks.Enqueue(task);
        }
    }
}
