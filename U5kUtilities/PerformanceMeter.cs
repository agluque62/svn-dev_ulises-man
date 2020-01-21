using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;
using System.Diagnostics;

namespace Utilities
{
    public class PerformanceMeter
    {
    }
    public class TimeMeasurement : PerformanceMeter
    {
        Stopwatch watch;
        String Id { get; set; }
        long TickAverageTime { get; set; }
        long RunningAverageTime { get; set; }
        long RunningMaxTime { get; set; }
        public TimeMeasurement(String name = "Generico")
        {
            Id = name;
            watch = new Stopwatch();
            watch.Start();
            TickAverageTime = -1;
            RunningAverageTime = -1;
            RunningMaxTime = -1;
        }
        public void StopAndPrint(Action<string>action, string etiqueta = "", [System.Runtime.CompilerServices.CallerLineNumber] int lineNumber = 0)
        {
            watch.Stop();
            if (action != null)
                action(String.Format("[{0,-16}<{1,-8}>]: Tiempo Medido: {2,6} ms.",
                    Id,                
                    etiqueta,                
                    watch.ElapsedMilliseconds));
        }
        public void Tick(Action<long, long, long> cb)
        {
            watch.Stop();
            TickAverageTime = TickAverageTime==-1 ? watch.ElapsedMilliseconds : (TickAverageTime + watch.ElapsedMilliseconds)/ 2;
            watch.Restart();
            if (cb!=null) cb.Invoke(TickAverageTime, RunningAverageTime, RunningMaxTime);
        }
        public long TickToSleep()
        {
            long measured = watch.ElapsedMilliseconds;
            RunningAverageTime = RunningAverageTime == -1 ? measured : (RunningAverageTime + measured) / 2;
            RunningMaxTime = RunningMaxTime >= measured ? RunningMaxTime : measured;
            return RunningAverageTime;
        }

        public void Dispose()
        {
            if (watch != null && watch.IsRunning)
            {
                watch.Stop();
            }
        }
    }
}
