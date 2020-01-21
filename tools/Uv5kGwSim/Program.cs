using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace Uv5kGwSim
{
    class Program
    {
        static string Name = "UV5KGWSIM";

        /// <summary>
        /// 
        /// </summary>
        static GwSim gw = new GwSim();

        /// <summary>
        /// 
        /// </summary>
        /// <param name="args"></param>
        static void Main(string[] args)
        {
            PrintMenu();
            (new Thread(new ParameterizedThreadStart(printEstadoTh)) { IsBackground = true, Name = "PrintEstado" }).Start(null);
            ConsoleKeyInfo result;
            do
            {
                result = Console.ReadKey(true);
                switch (result.Key)
                {
                    case ConsoleKey.D1:
                        gw.GlobalStart();
                        break;
                    case ConsoleKey.D2:
                        gw.GlobalStop();
                        break;
                    case ConsoleKey.D3:
                        gw.StartStop(cpuIds.cgwa);
                        break;
                    case ConsoleKey.D4:
                        gw.StartStop(cpuIds.cgwb);
                        break;
                    case ConsoleKey.D5:
                        gw.CGWReset(cpuIds.cgwa);
                        break;
                    case ConsoleKey.D6:
                        gw.CGWReset(cpuIds.cgwb);
                        break;
                    case ConsoleKey.D7:
                        gw.ConmutaPpalRsva();
                        break;
                    case ConsoleKey.D8:
                        break;
                    case ConsoleKey.D9:
                        break;
                    case ConsoleKey.D0:
                        break;
                }
            } while (result.Key != ConsoleKey.Escape);

            gw.GlobalStop();
        }

        /// <summary>
        /// 
        /// </summary>
        static void PrintMenu()
        {
            Console.Clear();
            Console.WriteLine("{0}. Simulador GW SNMP. DFNucleo 2015. ", Name);
            Console.WriteLine();
            Console.WriteLine("\t1.- Start");
            Console.WriteLine("\t2.- Stop");
            Console.WriteLine();
            Console.WriteLine("\t3.- Start/Stop CGW-A");
            Console.WriteLine("\t4.- Start/Stop CGW-B");
            Console.WriteLine();
            Console.WriteLine("\t5.- Reset CGW-A");
            Console.WriteLine("\t6.- Reset CGW-B");
            Console.WriteLine();
            Console.WriteLine("\t7.- Conmuta P-R");
            Console.WriteLine();
            Console.WriteLine("\t8.- Trap Test-R2");
            Console.WriteLine();
            Console.WriteLine("\tESC. Salir");
            Console.WriteLine();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="obj"></param>
        static void printEstadoTh(object obj)
        {
            while (true)
            {
                Console.Write("\rGW {0}", gw.GlobalStatus);
                Thread.Sleep(1000);
            }
        }

    }
}
