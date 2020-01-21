using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.NetworkInformation;

using System.Diagnostics;
using System.Diagnostics.Eventing.Reader;

using Utilities;

namespace NICEventMonitorProgram
{
    class Program
    {
        static NICEventMonitor monitor = null;
        static NICList.Properties.Settings config = NICList.Properties.Settings.Default;
        static void Main(string[] args)
        {
#if DEBUG1
            static string LOGNAME = "d:\\Datos\\Empresa\\_SharedPrj\\UlisesV5000i-MN\\ulises-man\\tools\\NICList\\Fallos_Red_ULISES_PO_WINCOM_MARVELL.evtx";
            static bool FileInsteadLog = true;
#else
#endif

#if DEBUG1
            string LOGNAME = NICList.Properties.Settings.Default.LogName;
            bool FileInsteadLog = false;
            monitor = new NICEventMonitor(config.NicServiceName, FileInsteadLog, LOGNAME, config.Lan1Device, config.Lan2Device)
            {
                UpEventId = config.UpEventId,
                DownEventId = config.DownEventId
            };
#elif _ON1__
            monitor = new NICEventMonitor(
                config.TeamType,
                "System",
                "e1rexpress", 
                27, 32);
#else
            monitor = new NICEventMonitor(
                "Intel",
                "System",
                "iANSMiniport",
                11, 15);
#endif

            monitor.StatusChanged += MonitorStatusChanged;
            monitor.MessageError += MonitorError;
            monitor.Start();

            ConsoleKeyInfo result;
            do
            {
                PrintMenu();
                result = Console.ReadKey(true);
                switch (result.Key)
                {
                    case ConsoleKey.D1:
#if OLD
                        if (config.Simulador == true)
                        {
                            new Task(() =>
                            {
                                if (monitor.Lan1Status != NICEventMonitor.LanStatus.Unknown)
                                {
                                    monitor.EventSimulate(0, monitor.Lan1Status == NICEventMonitor.LanStatus.Down ? true : false);
                                }
                            }).Start();
                        }
#endif
                        break;
                    case ConsoleKey.D2:
#if OLD
                        if (config.Simulador == true)
                        {
                            new Task(() =>
                            {
                                if (monitor.Lan2Status != NICEventMonitor.LanStatus.Unknown)
                                {
                                    monitor.EventSimulate(1, monitor.Lan2Status == NICEventMonitor.LanStatus.Down ? true : false);
                                }
                            }).Start();
                        }
#endif
                        break;
                    case ConsoleKey.D3:
                        monitor.ScanForDevices();
                        break;
                    case ConsoleKey.D4:
                        Console.WriteLine(monitor.Info());
                        Console.ReadKey(true);
                        break;
                    case ConsoleKey.D5:
                        Utilities.NICS.ShowInterfaceSummary();
                        Console.ReadKey(true);
                        break;
                    case ConsoleKey.D6:
                        LocalIps();
                        Console.ReadKey(true);
                        break;
                    case ConsoleKey.D7:
                        break;
                    case ConsoleKey.D8:
                        break;
                    case ConsoleKey.D9:
                        break;
                    case ConsoleKey.D0:
                        break;
                }

            } while (result.Key != ConsoleKey.Escape);

            monitor.Dispose();
        }

        /// <summary>
        /// 
        /// </summary>
        static void PrintMenu()
        {
            Console.Clear();
            Console.WriteLine("Prueba de Estado Dispositivos TEAM ({0}). DFNucleo 2016", config.TeamType);
            Console.WriteLine();
#if OLD
            Console.WriteLine("LAN1 Device: {0}", monitor.Lan1Device);
            Console.WriteLine("LAN2 Device: {0}", monitor.Lan2Device);
            Console.WriteLine("Simulador:   {0}", NICList.Properties.Settings.Default.Simulador);
#else
            Console.WriteLine("Detected LANS {0}", monitor.NICList.Count);
            for (int lan = 0; lan < monitor.NICList.Count; lan++)
            {
                Console.WriteLine("    LAN{0} Device: {1}. Status: {2}", lan + 1, monitor.NICList[lan].DeviceId, monitor.NICList[lan].Status);
            }
#endif
            Console.WriteLine();
#if OLD
            Console.WriteLine("\t1.- LAN-1 ({0})", monitor.Lan1Status);
            Console.WriteLine("\t2.- LAN-2 ({0})", monitor.Lan2Status);
            Console.WriteLine();
#else
#endif
            Console.WriteLine("\t3.- Eth. Devices");
            Console.WriteLine("\t4.- INFO");
            Console.WriteLine("\t5.- Eth. Interfaces");
            Console.WriteLine("\t6.- IPs");
            Console.WriteLine();
            Console.WriteLine("\tESC. Salir");
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="lan"></param>
        /// <param name="status"></param>
        static void MonitorStatusChanged(int lan)
        {
            PrintMenu();
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="mensaje"></param>
        static void MonitorError(string mensaje)
        {
            Console.WriteLine(mensaje);
        }

        static void LocalIps()
        {
            IPAddress[] localIPs = Dns.GetHostAddresses(Dns.GetHostName());
            foreach (IPAddress localip in localIPs)
            {
                Console.WriteLine(localip.ToString());
            }
        }
    }
}
