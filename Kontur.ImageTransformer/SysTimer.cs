using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Kontur.ImageTransformer
{
    class SysTimer
    {
        protected static int invokeCount;
        protected static int maxCount;
        protected static Task th;
        protected static int handleTaskId;

        

        public static void Start(Task thread, int count)
        {
            invokeCount = 0;
            maxCount = count;
            th = thread;
            handleTaskId = th.Id;

            var allProcess = System.Diagnostics.Process.GetProcesses();
            // Create an AutoResetEvent to signal the timeout threshold in the
            // timer callback has been reached.
            var autoEvent = new AutoResetEvent(false);

            //var statusChecker = new StatusChecker(1000,ref thread);

            // Create a timer that invokes CheckStatus after one second, 
            // and every 1/4 second thereafter.
            //Console.WriteLine("{0:h:mm:ss.fff} Creating timer.\n",
            //                  DateTime.Now);
            var stateTimer = new Timer(CheckStatus,
                                       autoEvent, 0, 1);

            // When autoEvent signals, change the period to every half second.
            autoEvent.WaitOne(); // был пуск таймера
            //stateTimer.Change(0, 500);
            //Console.WriteLine("\nChanging period to .5 seconds.\n");

            // When autoEvent signals the second time, dispose of the timer.
            //autoEvent.WaitOne();
            stateTimer.Dispose();
            //Console.WriteLine("\nDestroying timer.");
            //Console.ReadLine();
        }


        // This method is called by the timer delegate.
        private static void CheckStatus(Object stateInfo)
        {
            AutoResetEvent autoEvent = (AutoResetEvent)stateInfo;
            //Console.WriteLine("{0} Checking status {1,2}.",
            //    DateTime.Now.ToString("h:mm:ss.fff"),
            //    (++invokeCount).ToString());
            invokeCount++;
            if (invokeCount == maxCount)
            {
                // Reset the counter and signal the waiting thread.

                th.Dispose();

                string response = "callback later";
                invokeCount = 0;
                autoEvent.Set();
            }
        }
    }
}
