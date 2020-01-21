using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;

using Quartz;
using Quartz.Impl;

namespace Uv5kiNbxSim
{
    public enum SimServiceState { Stopped = 2, Slave = 0, Master = 1, NoIni = 3 };
    class NodeboxSim
    {
        /// <summary>
        /// 
        /// </summary>
        public string ipFrom { get; set; }
        public int webPort { get; set; }
        public string ipTo { get; set; }
        public SimServiceState CfgService { get; set; }
        public SimServiceState RadioService { get; set; }
        public SimServiceState TifxService { get; set; }
        public SimServiceState PabxService { get; set; }

        public String Status
        {
            get
            {
                return IsRunning ? String.Format("Cfg:{0} Rad:{1} Tfx:{2} Pbx:{3}", CfgService, RadioService, TifxService, PabxService) : "Detenido";
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public NodeboxSim()
        {
            IsRunning = false;
            CfgService = SimServiceState.NoIni;
            RadioService = SimServiceState.NoIni;
            TifxService = SimServiceState.NoIni;
            PabxService = SimServiceState.NoIni;
        }
        public void Start()
        {
            if (!IsRunning && task == null)
            {
                task = new Task(new Action(wkloop));
                task.Start();
            }
        }
        public void Stop()
        {
            if (IsRunning)
                IsRunning = false;
        }

        public SimServiceState NewState(SimServiceState current)
        {
            return current == SimServiceState.NoIni ? SimServiceState.Stopped :
                current == SimServiceState.Stopped ? SimServiceState.Slave :
                current == SimServiceState.Slave ? SimServiceState.Master : SimServiceState.NoIni;
        }

        /// <summary>
        /// 
        /// </summary>
        bool IsRunning = false;
        Task task = null;
        void wkloop()
        {
            IPEndPoint local = new IPEndPoint(IPAddress.Parse(ipFrom), 1024);
            IPEndPoint destino = new IPEndPoint(IPAddress.Parse(ipTo), 1022);
            using (UdpClient client = new UdpClient(local))
            {
                IsRunning = true;
                while (IsRunning)
                {
                    Thread.Sleep(2000);
                    try
                    {
                        Byte[] msg = 
                    {
                        (Byte)CfgService,(Byte)RadioService,(Byte)TifxService,(Byte)PabxService,
                        (Byte)0xff,(Byte)0xff,(Byte)0xff,(Byte)0xff,
                        (Byte)(webPort & 0xff),
                        (Byte)(webPort >> 8)
                    };
                        client.Send(msg, msg.Length, destino);
                    }
                    catch (Exception)
                    {
                    }
                }
                task = null;
            }
        }
    }

    public class NbxManager : IDisposable
    {
        public class UlisesNbx
        {
            public SimServiceState CfgService { get; set; }
            public SimServiceState RadioService { get; set; }
            public SimServiceState TifxService { get; set; }
            public SimServiceState PabxService { get; set; }
            public String Status
            {
                get
                {
                    return IsRunning ? String.Format("Cfg:{0} Rad:{1} Tfx:{2} Pbx:{3}", CfgService, RadioService, TifxService, PabxService) : "Detenido";
                }
            }
            public String IpFrom { get; set; }
            public int WebPort { get; set; }
            public bool IsRunning { get; set; }

            public UlisesNbx()
            {
                IsRunning = false;
                CfgService = SimServiceState.NoIni;
                RadioService = SimServiceState.NoIni;
                TifxService = SimServiceState.NoIni;
                PabxService = SimServiceState.NoIni;
            }
            public void Start()
            {
                IsRunning = true;
            }
            public void Stop()
            {
                IsRunning = false;
            }

            public SimServiceState NewState(SimServiceState current)
            {
                return current == SimServiceState.NoIni ? SimServiceState.Stopped :
                    current == SimServiceState.Stopped ? SimServiceState.Slave :
                    current == SimServiceState.Slave ? SimServiceState.Master : SimServiceState.NoIni;
            }


            ///// <summary>
            ///// 
            ///// </summary>
            ///// <param name="context"></param>
            //void IJob.Execute(IJobExecutionContext context)
            //{
            //    if (!IsRunning)
            //        return;
            //    try
            //    {
            //        JobDataMap dataMap = context.JobDetail.JobDataMap;
            //        string ipFrom = dataMap.GetString("ipfrom");

            //        IPEndPoint local = new IPEndPoint(IPAddress.Parse(dataMap.GetString("ipfrom")), 1024);
            //        IPEndPoint destino = new IPEndPoint(IPAddress.Parse(dataMap.GetString("ipto")), 1022);
            //        int webPort = dataMap.GetIntValue("port");
            //        using (UdpClient client = new UdpClient(local))
            //        {
            //            Byte[] msg = {
            //            (Byte)CfgService,(Byte)RadioService,(Byte)TifxService,(Byte)PabxService,
            //            (Byte)0xff,(Byte)0xff,(Byte)0xff,(Byte)0xff,
            //            (Byte)(webPort & 0xff),
            //            (Byte)(webPort >> 8) };
            //            client.Send(msg, msg.Length, destino);
            //        }
            //    }
            //    catch (Exception)
            //    {
            //    }
            //}
        }
        /// <summary>
        /// 
        /// </summary>
        public class UlisesNbxJob : IJob
        {
            void IJob.Execute(IJobExecutionContext context)
            {
                try
                {
                    UlisesNbx nbx = context.JobDetail.JobDataMap["nodebox"] as UlisesNbx;
                    if (nbx.IsRunning)
                    {
                        String ipTo = context.JobDetail.JobDataMap["ipto"] as String;

                        IPEndPoint local = new IPEndPoint(IPAddress.Parse(nbx.IpFrom), 1024);
                        IPEndPoint destino = new IPEndPoint(IPAddress.Parse(ipTo), 1022);
                        int webPort = nbx.WebPort;

                        using (UdpClient client = new UdpClient(local))
                        {
                            Byte[] msg = {
                        (Byte)nbx.CfgService,(Byte)nbx.RadioService,(Byte)nbx.TifxService,(Byte)nbx.PabxService,
                        (Byte)0xff,(Byte)0xff,(Byte)0xff,(Byte)0xff,
                        (Byte)(webPort & 0xff),
                        (Byte)(webPort >> 8) };
                            client.Send(msg, msg.Length, destino);
                        }
                    }
                }
                catch (Exception)
                {
                }
            }
        }

        public string IpTo { get; set; }
        protected IScheduler sched = (new StdSchedulerFactory()).GetScheduler();
        protected List<UlisesNbx> nodeboxes = new List<UlisesNbx>();
        public NbxManager(string ipto = "")
        {
            IpTo = ipto;
            sched.Start();
        }

        void IDisposable.Dispose()
        {
            sched.Shutdown();
        }

        public void AddNodebox(string ipfrom, int port)
        {
            UlisesNbx nbx = new UlisesNbx() { IpFrom = ipfrom, WebPort = port };
            nodeboxes.Add(nbx);

            IJobDetail job = JobBuilder.Create<UlisesNbxJob>().WithIdentity(ipfrom, "nodebox")
                .UsingJobData("ipto", IpTo)
                .UsingJobData("port",port).Build();
            
            job.JobDataMap["nodebox"] = nbx;

            ITrigger trigger = TriggerBuilder.Create().WithIdentity(ipfrom, "nodebox")
                .WithSimpleSchedule(x => x.WithIntervalInSeconds(2).RepeatForever()).ForJob(job).StartNow()
                .Build();

            sched.ScheduleJob(job, trigger);
        }

        public UlisesNbx this[string key]
        {
            get
            {
                return nodeboxes.Where(nbx => nbx.IpFrom == key).FirstOrDefault();
            }
        }

        //protected UlisesNbx Nodebox(string id)
        //{
        //    var jobs = sched.GetCurrentlyExecutingJobs();
        //    IJobExecutionContext found = jobs.Where(ctx => ctx.JobDetail.Key.Name == id).FirstOrDefault();
        //    if (found != null)
        //    {
        //        return found.JobInstance as UlisesNbx;
        //    }
        //    return null;
        //}
    };
}
