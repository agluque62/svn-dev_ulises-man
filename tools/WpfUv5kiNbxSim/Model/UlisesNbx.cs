using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Net;
using System.Net.Sockets;

namespace WpfUv5kiNbxSim.Model 
{
    public enum SimServiceState { Slave = 0, Master = 1, Stopped = 2, NoIni = 3};
    public class UlisesNbx : IDisposable
    {
        public static event Action<string> NotifyChange;
        public static String ServerIp { get; set; }
        public static int ServerPort { get; set; }
        
        public IList<SimServiceState> ServiceStates
        {
            get
            {
                // Will result in a list like {"Tester", "Engineer"}
                var ret = Enum.GetValues(typeof(SimServiceState)).Cast<SimServiceState>().ToList<SimServiceState>();
                return ret;
            }
        }

        public String Ip { get; set; }
        public int WebPort { get; set; }

        public SimServiceState CfgService { get; set; }
        public SimServiceState RadioService { get; set; }
        public SimServiceState TifxService { get; set; }
        public SimServiceState PresService { get; set; }
        public bool Active { get; set; }

        public UlisesNbx()
        {
            CfgService = SimServiceState.Stopped;
            RadioService = SimServiceState.Stopped;
            TifxService = SimServiceState.Stopped;
            PresService = SimServiceState.Stopped;
            Active = false;
        }

        public UlisesNbx(String ip, int webPort) 
        {
            CfgService = SimServiceState.Stopped;
            RadioService = SimServiceState.Stopped;
            TifxService = SimServiceState.Stopped;
            PresService = SimServiceState.Stopped;

            Ip = ip;
            WebPort = webPort;
            Task.Run(()=>Job());
        }

        public void Dispose()
        {
            Running = false;
            NbxJobTask.Wait(3000);
            NbxJobTask = null;
        }

        private void Job()
        {
            IPEndPoint local = new IPEndPoint(IPAddress.Parse(Ip), 0);
            IPEndPoint destino = new IPEndPoint(IPAddress.Parse(ServerIp), ServerPort);
            using (UdpClient client = new UdpClient(local))
            {
                Running = true;
                while (Running)
                {
                    Task.Delay(2000).Wait();
                    if (Active)
                    {
                        try
                        {
                            Byte[] msg =                     
                            {
                                (Byte)CfgService,(Byte)RadioService,(Byte)TifxService,(Byte)PresService,
                                (Byte)0xff,(Byte)0xff,(Byte)0xff,(Byte)0xff,
                                (Byte)(WebPort & 0xff),
                                (Byte)(WebPort >> 8)                    
                            };
                            client.Send(msg, msg.Length, destino);
                            NotifyChange(String.Format("{0}: Sending msg C:{1}, R:{2}, T:{3}, P:{4}", Ip,
                                msg[0], msg[1], msg[2], msg[3]));
                        }
                        catch (Exception x)
                        {
                            NLog.LogManager.GetLogger("UlisesNbx").Error("Job Exception", x);
                        }
                    }
                }
            }
        }
        Task NbxJobTask = null;
        private bool Running { get; set; }

    }
}
