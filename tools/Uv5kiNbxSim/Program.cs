#define _ALLOW_ERRORS_
#define _VERSION_1_
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Uv5kiNbxSim
{
    class Program
    {
        /// <summary>
        /// 
        /// </summary>
        static string Name = "UV5KNBXSIM";
        /// <summary>
        /// 
        /// </summary>
#if _VERSION_0_
        static List<NodeboxSim> nbxs = new List<NodeboxSim>()
        {
            new NodeboxSim(){ ipFrom="192.168.0.71", webPort=1023, ipTo="192.168.0.129"},
            new NodeboxSim(){ ipFrom="192.168.0.72", webPort=2023, ipTo="192.168.0.129"},
            new NodeboxSim(){ ipFrom="192.168.0.129", webPort = 8090, ipTo="192.168.0.129"}
        };
#else
        static List<Tuple<string,int>> nbx_cfg = new List<Tuple<string,int>>() {
            new Tuple<string,int>("192.168.0.71", 2023),
            new Tuple<string,int>("192.168.0.72", 3023),
            new Tuple<string,int>("192.168.0.129",4023),
        };
        static NbxManager nbxs = new NbxManager("192.168.0.129");
#endif
        /// <summary>
        /// 
        /// </summary>
        /// <param name="args"></param>
        static void Main(string[] args)
        {
#if !_VERSION_0_
            foreach(Tuple<string, int> nbx in nbx_cfg)
                nbxs.AddNodebox(nbx.Item1, nbx.Item2);
            System.Threading.Thread.Sleep(1000);
#endif
            ConsoleKeyInfo result;
            do
            {
                PrintMenu();
                result = Console.ReadKey(true);
                switch (result.Key)
                {
                    case ConsoleKey.D1:
#if _ALLOW_ERRORS_
#if _VERSION_0_
                        Configure(nbxs[0]);
#else
                        Configure(nbxs[nbx_cfg[0].Item1]);
#endif
#else
                        if (nbxs[0].StateSet() == NodeboxSimState.Master)
                        {
                            nbxs[1].State = nbxs[1].State == NodeboxSimState.Master ? NodeboxSimState.Slave : nbxs[1].State;
                            nbxs[2].State = nbxs[2].State == NodeboxSimState.Master ? NodeboxSimState.Slave : nbxs[2].State;
                        }
#endif
                        break;
                    case ConsoleKey.D2:
#if _ALLOW_ERRORS_
#if _VERSION_0_
                        Configure(nbxs[1]);
#else
                        Configure(nbxs[nbx_cfg[1].Item1]);
#endif
#else
                        if (nbxs[1].StateSet() == NodeboxSimState.Master)
                        {
                            nbxs[0].State = nbxs[0].State == NodeboxSimState.Master ? NodeboxSimState.Slave : nbxs[0].State;
                            nbxs[2].State = nbxs[2].State == NodeboxSimState.Master ? NodeboxSimState.Slave : nbxs[2].State;
                        }
#endif
                        break;
                    case ConsoleKey.D3:
#if _ALLOW_ERRORS_
#if _VERSION_0_
                        Configure(nbxs[2]);
#else
                        Configure(nbxs[nbx_cfg[2].Item1]);
#endif
#else
                        if (nbxs[2].StateSet() == NodeboxSimState.Master)
                        {
                            nbxs[1].State = nbxs[1].State == NodeboxSimState.Master ? NodeboxSimState.Slave : nbxs[1].State;
                            nbxs[0].State = nbxs[0].State == NodeboxSimState.Master ? NodeboxSimState.Slave : nbxs[0].State;
                        }
#endif
                        break;
                    case ConsoleKey.D4:
                        break;
                }
            } while (result.Key != ConsoleKey.Escape);
#if _VERSION_0_
            // Cierro los arrancados...
            foreach (NodeboxSim nbx in nbxs)
                nbx.Stop();
#else
            ((IDisposable)nbxs).Dispose();
#endif

        }

        /// <summary>
        /// 
        /// </summary>
        static void PrintMenu()
        {
            Console.Clear();
            Console.WriteLine("{0}. Simulador Grupo Nodebox. DFNucleo 2015-2017. ", Name);
            Console.WriteLine();
#if _VERSION_0_
            Console.WriteLine("\t1.- Nodebox #1({1})\t=> {0}", nbxs[0].Status, String.Format("{0}:{1}", nbxs[0].ipFrom, nbxs[0].webPort));
            Console.WriteLine("\t2.- Nodebox #2({1})\t=> {0}", nbxs[1].Status, String.Format("{0}:{1}", nbxs[1].ipFrom, nbxs[1].webPort));
            Console.WriteLine("\t3.- Nodebox #3({1})\t=> {0}", nbxs[2].Status, String.Format("{0}:{1}", nbxs[2].ipFrom, nbxs[2].webPort));
#else
            Console.WriteLine("\t1.- Nodebox #1({1})\t=> {0}", nbxs[nbx_cfg[0].Item1].Status, String.Format("{0}:{1}", nbx_cfg[0].Item1, nbx_cfg[0].Item2));
            Console.WriteLine("\t2.- Nodebox #2({1})\t=> {0}", nbxs[nbx_cfg[1].Item1].Status, String.Format("{0}:{1}", nbx_cfg[1].Item1, nbx_cfg[0].Item2));
            Console.WriteLine("\t3.- Nodebox #3({1})\t=> {0}", nbxs[nbx_cfg[2].Item1].Status, String.Format("{0}:{1}", nbx_cfg[2].Item1, nbx_cfg[0].Item2));
#endif
            Console.WriteLine();
            Console.WriteLine("\tESC. Salir");
            Console.WriteLine();
        }

#if _VERSION_0_
        static void Configure(NodeboxSim nbx)
#else
        static void Configure(NbxManager.UlisesNbx nbx)
#endif
        {
            ConsoleKeyInfo result;
            do
            {
                PrintSubmenu(nbx);
                result = Console.ReadKey(true);
                switch (result.Key)
                {
                    case ConsoleKey.C:
                        nbx.CfgService = nbx.NewState(nbx.CfgService);
                        break;
                    case ConsoleKey.R:
                        nbx.RadioService = nbx.NewState(nbx.RadioService);
                        break;
                    case ConsoleKey.T:
                        nbx.TifxService = nbx.NewState(nbx.TifxService);
                        break;
                    case ConsoleKey.P:
                        nbx.PabxService = nbx.NewState(nbx.PabxService);
                        break;
                    case ConsoleKey.D1:
                        nbx.Start();
                        System.Threading.Thread.Sleep(100);
                        break;
                    case ConsoleKey.D2:
                        nbx.Stop();
                        break;
                    default:
                        break;
                }
            } while (result.Key != ConsoleKey.Escape);
        }

#if _VERSION_0_
        static void PrintSubmenu(NodeboxSim nbx)
#else
        static void PrintSubmenu(NbxManager.UlisesNbx nbx)
#endif
        {
            Console.Clear();
            Console.WriteLine("{0}. Simulador Grupo Nodebox. DFNucleo 2015-2017. ", Name);
            Console.WriteLine();
#if _VERSION_0_
            Console.WriteLine("Nodebox ({0}): {1}", String.Format("{0}:{1}", nbx.ipFrom, nbx.webPort), nbx.Status);
#else
            Console.WriteLine("Nodebox ({0}): {1}", String.Format("{0}:{1}", nbx.IpFrom, nbx.WebPort), nbx.Status);
#endif
            Console.WriteLine();
            Console.WriteLine("\t1.- Start");
            Console.WriteLine("\t2.- Stop");
            Console.WriteLine();
            Console.WriteLine("\tC.- Servicio Configuracion: {0}", nbx.CfgService);
            Console.WriteLine("\tR.- Servicio Radio        : {0}", nbx.RadioService);
            Console.WriteLine("\tT.- Servicio TIFX         : {0}", nbx.TifxService);
            Console.WriteLine("\tP.- Servicio PBX          : {0}", nbx.PabxService);
            Console.WriteLine();
            Console.WriteLine("\tESC. Salir");
        }
    }
}
