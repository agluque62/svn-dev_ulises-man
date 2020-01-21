using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace WinAudio
{
    class Program
    {
#if DEBUG
        const int tickms = 100;
#else
        const int tickms = 1000;
#endif
        static CMedia_MMAudioDeviceManager manager = new CMedia_MMAudioDeviceManager();
        static System.Timers.Timer Tick = new System.Timers.Timer(tickms) { AutoReset = false };

        static string appGuid = "{70CA8594-3B06-4028-877C-35D4402EDCE6}";

        static void Main(string[] args)
        {
            using (Mutex mutex = new Mutex(false, "Global\\" + appGuid))
            {
                if (!mutex.WaitOne(0, false))
                {
                    NLog.LogManager.GetLogger("WinAudio").Fatal("Instance already running");
                    return;
                }

                //manager.Init();
                //Tick.Elapsed += Tick_Elapsed;
                //Tick.Enabled = true;

                NLog.LogManager.GetLogger("WinAudio").Info("Application start.");
                ManualResetEvent fin = new ManualResetEvent(false);
                Task.Factory.StartNew(() =>
                {
                    manager.Init();
                    do
                    {
                        manager.Tick();
                    } while (fin.WaitOne(TimeSpan.FromSeconds(1)) == false);
                });

                while (Console.ReadKey(true).Key != ConsoleKey.Escape) ;
                fin.Set();
                NLog.LogManager.GetLogger("WinAudio").Info("Application end.");
            }
        }

        //static void Tick_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        //{
        //    try
        //    {
        //        Tick.Enabled = false;
        //        manager.Tick();
        //    }
        //    catch (Exception )
        //    {
        //    }
        //    finally
        //    {
        //        Tick.Enabled = true;
        //    }
        //}
    }
}
