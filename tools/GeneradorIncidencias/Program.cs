using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace GeneradorIncidencias
{
    class Program
    {
        /// <summary>
        /// 
        /// </summary>
        static Uv5kDbAccess dbAccess;
        static SimuladorHistoricos SimHis;
        static SimuladorEstadistica SimEst;

        /// <summary>
        /// 
        /// </summary>
        static void PrintMenu()
        {
            Console.Clear();
            Console.WriteLine("Generador de Eventos en Ulises V 5000. DFNucleo 2016.");
            Console.WriteLine();
            Console.WriteLine("\t1.- Simulador de Historicos ({0})", SimHis.IsRunning ? "Detener" : "Arrancar");
            Console.WriteLine("\t2.- Simulador de Estadisticas ({0})", SimEst.IsRunning ? "Detener" : "Arrancar");
            Console.WriteLine();
            Console.WriteLine("\tESC. Salir");
            Console.WriteLine();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="args"></param>
        static void Main(string[] args)
        {
            ConsoleKeyInfo result;

            dbAccess = new Uv5kDbAccess();
            SimHis = new SimuladorHistoricos(dbAccess);
            SimEst = new SimuladorEstadistica(dbAccess);

            dbAccess.Connect();

            PrintMenu();

            do
            {
                result = Console.ReadKey(true);
                switch (result.Key)
                {
                    case ConsoleKey.D1:
                        if (SimHis.IsRunning)
                            SimHis.Stop();
                        else
                            SimHis.Start();
                        break;
                    case ConsoleKey.D2:
                        if (SimEst.IsRunning)
                            SimEst.Stop();
                        else
                            SimEst.Start();
                        break;
                    case ConsoleKey.D3:
                        break;
                    case ConsoleKey.D4:
                        break;
                    case ConsoleKey.D5:
                        break;
                    case ConsoleKey.D6:
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
                PrintMenu();
            } while (result.Key != ConsoleKey.Escape);

            dbAccess.Disconnect();
        }

    }
}
