using System;
using System.Threading;
using System.Threading.Tasks;

using Utilities;

namespace NucleoGeneric
{
    //public delegate void ThreadWork();
    //public class ThreadControl
    //{
    //    public ThreadWork _work { get; set; }
    //    public Thread _thread { get; set; }
    //    public string _name { get; set; }
    //}

    /// <summary>	
    /// Clase Global de Implementación de Un Proceso.	
    /// Los Procesos deben derivar de esta clase y redefinir con override el metodo Run	
    /// </summary>
#if NGThreadNew
    public class NGThread : BaseCode
    {
        /// <summary>
        /// Objeto Thread
        /// </summary>
        private Thread m_thread;
        public bool salir = true;

        /// <summary>
        /// 
        /// </summary>
        public string Name
        {
            get
            {
                return m_thread.Name;
            }
            set
            {
                m_thread.Name = value;
            }
        }
        /// <summary>
        /// Constructor. Crea el Objeto Thread interno.
        /// </summary>
        public NGThread()
        {
            m_thread = new Thread(new ThreadStart(Run));
        }

        /// <summary>
        /// Lazo del Proceso. 
        /// Las Clases Heredadas deben redefinir con override Este metodo
        /// </summary>
        protected virtual void Run()
        {
            while (m_thread.IsAlive)
            {
            }
        }

        /// <summary>
        /// Método General de Arranque
        /// </summary>
        public virtual bool Start()
        {
            //switch (m_thread.ThreadState)
            //{
            //    case ThreadState.Unstarted:
            //        m_thread.Start();
            //        salir = false;
            //        break;
            //    case ThreadState.Stopped:
            //        m_thread = new Thread(new ThreadStart(Run));
            //        m_thread.Start();
            //        salir = false;
            //        break;
            //    default:
            //        throw new Exception(String.Format("No puedo Arrancar el Thread <{0}>. Estado: {1}", Name, m_thread.ThreadState));
            //}
            m_thread = new Thread(new ThreadStart(Run));
            salir = false;
            m_thread.Start();
            return true;
        }

        /// <summary>
        /// Método General de Parada.
        /// </summary>
        public virtual void Stop()
        {
            salir = true;
            m_thread.Abort();
            m_thread.Join(10000);
        }

        /// <summary>
        /// Determina si el Proceso está Activo o no
        /// </summary>
        public bool IsRunning()
        {
            return !salir;  // m_thread.IsAlive;
        }

        /// <summary>
        /// Método General para Dormir el Proceso
        /// </summary>

        public void Sleep(int iMsec)
        {
            Decimal ticks = (Decimal)(iMsec / 10);

            while (IsRunning() && ticks > 0)
            {
                Thread.Sleep(10);
                ticks--;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public void Join(int maxtime=0)
        {
            if (maxtime == 0)
                m_thread.Join();
            else
                m_thread.Join(maxtime);
        }

        /** Por compatibildad */
        public CancellationTokenSource Cancel
        {
            get
            {
                return null;
            }
        }

    }	// De la Clase

#else
    public abstract class NGThread : BaseCode
    { 
        public string Name { get; set; }
        public CancellationTokenSource Cancel
        {
            get
            {
                return cancel;
            }
        }
        public bool Running
        {
            get
            {
                return cancel == null ? false : cancel.Token.IsCancellationRequested ? false : true;
            }
        }
        public virtual bool Start()
        {
            if (cancel == null)
            {
                ConfigCultureSet();
                tm = new TimeMeasurement(Name);
                cancel = new CancellationTokenSource();
                LogTrace<NGThread>(String.Format("Starting Thread ({0})", Name));
                taskObject = Task.Factory.StartNew(Run);
                return true;
            }
            return false;
        }
        public virtual void Stop(TimeSpan timeout)
        {
            if (cancel != null)
            {
                LogTrace<NGThread>(String.Format("Stopping Thread ({0})", Name));
                cancel.Cancel();
                if (!taskObject.Wait((int)timeout.TotalMilliseconds))
                {
                    LogWarn<NGThread>(String.Format("Timeout Stopping Thread ({0})", Name));
                }
                else
                {
                    LogTrace<NGThread>(String.Format("Thread ({0}) Stopped", Name));
                }
            }
        }
        public virtual void StopAsync(Action<Task> cb)
        {
            if (cancel != null)
            {
                LogTrace<NGThread>(String.Format("Async Stopping Thread ({0})", Name));
                cancel.Cancel();
                cb(taskObject);
            }
        }

        public void Sleep(int iMsec)
        {
            Decimal ticks = (Decimal)(iMsec / 50);

            while (Running && ticks > 0)
            {
                Task.Delay(50).Wait();
                ticks--;
            }
        }

        protected virtual void Run()
        {
            while (Running)
            {
                Sleep(100);
            }
        }
        protected bool IsRunning()
        {
            tm.Tick((ta, rta, rtm) =>
            {
                IamAlive.Tick(Name, () =>
                {
                    String msg = String.Format("{0,20}. Alive. TA={1,8:G}, RTA={2,8:G}, RTM={3,8:G}", Name, ta, rta, rtm);
#if DEBUG
                    LogWarn<NGThread>(msg);
#else
                    LogDebug<NGThread>(msg);
#endif
                });
            });
            ConfigCultureSet();
            return cancel == null ? false : cancel.Token.IsCancellationRequested ? false : true;
        }
        protected void GoToSleepInTimer()
        {
            if (timer != null)
            {
                tm.TickToSleep();
                timer.Wait();
            }
        }

        protected void Dispose()
        {
            if (tm != null)
            {
                tm.Dispose();
                tm = null;
            }
            //taskObject.Dispose();
            taskObject = null;
            if (cancel != null)
            {
                cancel.Dispose();
                cancel = null;
            }
        }

        protected Task taskObject = null;
        protected CancellationTokenSource cancel = null;
        protected TaskTimer timer = null;
        protected TimeMeasurement tm = null;

        public abstract void DoWork();
        public abstract void LocalDispose();
    }

    public class BackgroundTaskFactory
    {
        static public Task StartNew(string Id, Action routine, Action<string, Exception> error, TimeSpan Timeout)
        {
            CancellationTokenSource CancelSource = new CancellationTokenSource(Timeout);
            return Task.Factory.StartNew(() =>
            {
                try
                {
                    var threadId = System.Threading.Thread.CurrentThread;
                    using (CancelSource.Token.Register(() => threadId.Abort() ))
                    {
                            routine();
                    }
                }
                catch (Exception x)
                {
                    error(Id, x);
                }
            }, CancelSource.Token);
        }
    }
#endif

}		// PISoftware

