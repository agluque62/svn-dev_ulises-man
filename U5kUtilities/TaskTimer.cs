using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Utilities
{
    public class TaskTimer : IDisposable
    {
        public TimeSpan Interval { get; set; }
        public TaskTimer(TimeSpan ts, CancellationTokenSource _cancelToken) 
        { 
            Interval = ts;
            LastWake = DateTime.Now;
            Owner = System.Threading.Thread.CurrentThread.Name;
            InitialOffset = new TimeSpan(0, 0, 0, 0, users * 1000);
            users++;
            cancelToken = _cancelToken;
        }
        public void Wait()
        {
            NextWake = LastWake + Interval + InitialOffset;
            while (DateTime.Now  < NextWake && (cancelToken==null || cancelToken.Token.IsCancellationRequested==false))
            {
                Task.Delay(100).Wait();
            }
            InitialOffset = new TimeSpan();
            LastWake = DateTime.Now;
        }
        public void Dispose()
        {
            users--;
        }
        private DateTime LastWake { get; set; }
        private DateTime NextWake { get; set; }
        private TimeSpan InitialOffset { get; set; }
        private string Owner { get; set; }
        private CancellationTokenSource cancelToken;
        private static int users = 0;
    }
}
