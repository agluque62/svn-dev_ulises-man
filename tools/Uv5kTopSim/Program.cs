using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Uv5kTopSim
{
    class Program
    {
        static string Name = "UV5KTOPSIM";
        static string SnmpIp = Properties.Settings.Default.IpPropia;
        static int SnmpPort = Properties.Settings.Default.PortSnmp;

        static U5kTopMib _mib = new U5kTopMib();

        static void Main(string[] args)
        {
            TopSnmpAgent.Init(SnmpIp, null, SnmpPort, 1162);
            _mib.Load(Properties.Settings.Default.IpServidor, Properties.Settings.Default.PortSnmpServidor);

            TopSnmpAgent.Start();

            ConsoleKeyInfo result;
            do
            {
                PrintMenu();
                result = Console.ReadKey(true);
                switch (result.Key)
                {
                    case ConsoleKey.D1:
                        _mib.EstadoTop = _mib.EstadoTop == 0 ? 1 : 0;
                        break;
                    case ConsoleKey.D2:
                        _mib.AltavozRadio = _mib.AltavozRadio == 0 ? 1 : 0;
                        break;
                    case ConsoleKey.D3:
                        _mib.AltavozLcen = _mib.AltavozLcen == 0 ? 1 : 0;
                        break;
                    case ConsoleKey.D4:
                        _mib.JackEjecutivo = _mib.JackEjecutivo == 0 ? 1 : 0;
                        break;
                    case ConsoleKey.D5:
                        _mib.JackAyudante = _mib.JackAyudante == 0 ? 1 : 0;
                        break;
                    case ConsoleKey.D6:
                        _mib.Panel = _mib.Panel == 0 ? 1 : 0;
                        break;
                    case ConsoleKey.D7:
                        _mib.Lan1 = _mib.Lan1 == 0 ? 1 : _mib.Lan1==1 ? 2 : 0;
                        break;
                    case ConsoleKey.D8:
                        _mib.Lan2 = _mib.Lan2 == 0 ? 1 : _mib.Lan2 == 1 ? 2 : 0;
                        break;
                    case ConsoleKey.D9:
                        _mib.CableGrabacion = _mib.CableGrabacion == 0 ? 1 : 0;
                        break;
                    case ConsoleKey.D0:
                        break;
                }

            } while (result.Key != ConsoleKey.Escape);

            TopSnmpAgent.Close();
        }

        /// <summary>
        /// 
        /// </summary>
        static void PrintMenu()
        {
            Console.Clear();
            Console.WriteLine("{2}. Simulador Top SNMP. DFNucleo 2015. Escuchando: {0}:{1}", SnmpIp, SnmpPort, Name);
            Console.WriteLine();
            Console.WriteLine("\t1.- Estado General ({0})", _mib.EstadoTop);
            Console.WriteLine("\t2.- Altavoz Radio  ({0})", _mib.AltavozRadio);
            Console.WriteLine("\t3.- Altavoz LCEN   ({0})", _mib.AltavozLcen);
            Console.WriteLine("\t4.- JACK Ejecutivo ({0})", _mib.JackEjecutivo);
            Console.WriteLine("\t5.- JACK Ayudante  ({0})", _mib.JackAyudante);
            Console.WriteLine("\t6.- Salvapantallas ({0})", _mib.Panel==0 ? 1 : 0);
            Console.WriteLine();
            Console.WriteLine("\t7.- Estado LAN-1   ({0})", _mib.Lan1);
            Console.WriteLine("\t8.- Estado LAN-2   ({0})", _mib.Lan2);
            Console.WriteLine("\t9.- Cable Grabacion({0})", _mib.CableGrabacion);
            Console.WriteLine();
            Console.WriteLine("\tESC. Salir");
        }

    }
}
