using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SactaServiceTest
{
    class Program
    {
        static Sacta.SactaModule smodule = new Sacta.SactaModule("departamento");

        static void Main(string[] args)
        {
            ConsoleKeyInfo result;
            do
            {
                PrintInfo();
                PrintMenu();
                result = Console.ReadKey(true);
                switch (result.Key)
                {
                    case ConsoleKey.S:
                        break;
                }

            } while (result.Key != ConsoleKey.Escape);
        }

        /// <summary>
        /// 
        /// </summary>
        static void PrintInfo()
        {
            Console.Clear();
            ConsoleColor last = Console.ForegroundColor;
            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine("Test de Servicio SACTA. DFNucleo 2017");
            Console.WriteLine("Escuchando en {0},{1}", SactaSectionHandler.CfgSacta.CfgIpAddress.IpRedA + ":" + SactaSectionHandler.CfgSacta.CfgSactaUdp.PuertoOrigen.ToString(),
                SactaSectionHandler.CfgSacta.CfgIpAddress.IpRedB + ":" + SactaSectionHandler.CfgSacta.CfgSactaUdp.PuertoOrigen.ToString());
            Console.WriteLine();
            Console.WriteLine("Estado: {0}", smodule.State);
            Console.WriteLine();
            Console.ForegroundColor = last;
        }

        static void PrintMenu()
        {
            ConsoleColor last = Console.ForegroundColor;
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("Opciones:\n");
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("      [1] Start / Stop");
            /*
            Console.WriteLine("      [1] Equipo => SNMP-Fallo  ");
            Console.WriteLine("      [2] Equipo => SNMP-Timeout");
            Console.WriteLine("      [3] Equipo => SNMP-Local  ");
            Console.WriteLine("      [4] Todos a SNMP ...  ");
            Console.WriteLine();
            Console.WriteLine("      [5] Equipo => SIP-OK  ");
            Console.WriteLine("      [6] Equipo => SIP-Error  ");
            Console.WriteLine("      [7] Todos a SIP ... ");
            Console.WriteLine();
            Console.WriteLine("      [S] Secuencias Programadas...  ");
            */
            Console.WriteLine();
            Console.WriteLine("  [ESC] Salir");
            Console.WriteLine();

            Console.ForegroundColor = last;
        }
    }
}
