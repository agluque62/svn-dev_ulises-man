using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;

using RcsHfSimulator.HfSnmp;
using Roip;

namespace RcsHfSimulator
{
    class FiltroMostrar
    {
        static public int FiltroBanda { get; set; }
        static public int FiltroTipo { get; set; }
        static public bool FiltroEquipo(GearMibHelper.GearData eq)
        {
            if (FiltroBanda != -1 && FiltroBanda != (int)eq.banda)
                return false;
            if (FiltroTipo != -1 && FiltroTipo != (int)eq.tipo)
                return false;

            return true;
        }        
    }

    class Program
    {
        static string SnmpIp = Properties.Settings.Default.IpPropia;
        static int SnmpPort = Properties.Settings.Default.PortSnmp;
        static GearsListMib _mib = new GearsListMib();
        static int NumeroDeEquipos = 0;

        static int origWidth;
        static int origHeight;

        static Timer timer = new System.Timers.Timer(1000);
        static int modo = 0;
        static Random rnd = new Random(DateTime.Now.Millisecond);
        /// <summary>
        /// 
        /// </summary>
        static RoIpSipManager roip = new RoIpSipManager();

        static void Main(string[] args)
        {
            SnmpAgent.Init(SnmpIp, null, SnmpPort, 162);

            _mib.Load();
            SnmpAgent.PduSetReceived += _mib.PduSetReceived_handler;
            SnmpAgent.Start();

            FiltroMostrar.FiltroBanda = FiltroMostrar.FiltroTipo = -1;
            origWidth = Console.WindowWidth;
            origHeight = Console.WindowHeight;
            Console.SetWindowSize(120, 50);

            timer.Elapsed += ElapseTimerHandler;
            timer.Enabled = true;
                        
            roip.Init();
            roip.Start();

            ConsoleKeyInfo result;
            do
            {
                PrintTitulo();
                PrintListaEquipos();
                PrintMenu();
                result = Console.ReadKey(true);
                switch (result.Key)
                {
                    case ConsoleKey.M:     // Modo Manual / Random...
                        modo = modo == 0 ? 1 : 0;
                        break;
                    case ConsoleKey.B:     // Seleccion de Banda
                        FiltroMostrar.FiltroBanda = FiltroMostrar.FiltroBanda == 0 ? 1 : FiltroMostrar.FiltroBanda == 1 ? -1 : 0;
                        break;
                    case ConsoleKey.T:     // Seleccion TX / RX
                        FiltroMostrar.FiltroTipo = FiltroMostrar.FiltroTipo == 0 ? 1 : FiltroMostrar.FiltroTipo == 1 ? -1 : 0;
                        break;

                    case ConsoleKey.D0:     // SNMP Equipo Ok (1).
                        GearStatusSet(1);
                        break;
                    case ConsoleKey.D1:     // SNMP Equipo Fallo (2)
                        GearStatusSet(2);
                        break;
                    case ConsoleKey.D2:     // SNMP Equipo Timeout (3)
                        GearStatusSet(3);
                        break;
                    case ConsoleKey.D3:     // SNMP Equipo Local (0)
                        GearStatusSet(0);
                        break;
                    case ConsoleKey.D4:     // SNMP Todos a ...
                        AllStatusSet();
                        break;

                    case ConsoleKey.D5:     // SIP Equipo Ok
                        GearStatusSipSet(1);
                        break;
                    case ConsoleKey.D6:     // SIP Equipo Fallo
                        GearStatusSipSet(2);
                        break;
                    case ConsoleKey.D7:     // SIP todos a ...
                        AllSipStatusSet();
                        break;

                    case ConsoleKey.S:      // Secuencia Programada.
                        SecuenciasProgramadas();
                        break;
                }

            } while (result.Key != ConsoleKey.Escape);

            roip.End();

            _mib.Unload();
            SnmpAgent.Close();
            Console.SetWindowSize(origWidth, origHeight);
        }

        /// <summary>
        /// 
        /// </summary>
        static void PrintTitulo()
        {
            Console.Clear();
            ConsoleColor last = Console.ForegroundColor;
            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine("Simulador Equipos Radio Telemandados. DFNucleo 2016");
            Console.WriteLine("Escuchando en {0}:{1}", SnmpIp, SnmpPort);
            Console.WriteLine();
            Console.WriteLine("[M] Modo {0}", modo == 0 ? "Manual" : "Automatico");
            Console.WriteLine();
            Console.ForegroundColor = last;
        }

        static void PrintMenu()
        {
            ConsoleColor last = Console.ForegroundColor;
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("Opciones:\n");
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("      [0] Equipo => SNMP-Ok     ");
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

            Console.WriteLine();
            Console.WriteLine("  [ESC] Salir");
            Console.WriteLine();

            Console.ForegroundColor = last;
        }

        static void PrintListaEquipos()
        {
            //int val = 0; 
            
            ConsoleColor last = Console.ForegroundColor;
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("Lista de Equipos Simulados. Banda: {0}, Tipo : {1}\n",
                FiltroMostrar.FiltroBanda == -1 ? "Todas" : FiltroMostrar.FiltroBanda == 0 ? "VHF" : "UHF",
                FiltroMostrar.FiltroTipo == -1 ? "Todos" : FiltroMostrar.FiltroTipo == 0 ? "RX" : "TX");

            List<GearMibHelper.GearData> equipos = _mib.Gears.OrderBy(e=>e.modo).ToList();
            NumeroDeEquipos = equipos.Count;

            int TxRow = Console.CursorTop;
            int RxRow = Console.CursorTop;

            foreach (GearMibHelper.GearData eq in equipos)
            {
                if (FiltroMostrar.FiltroEquipo(eq))
                {
                    Console.SetCursorPosition(eq.tipo == GearMibHelper.eRadioTipo.Transmisor ? 3 : 55,
                        eq.tipo == GearMibHelper.eRadioTipo.Transmisor ? TxRow : RxRow);

                    Console.ForegroundColor = ConsoleColor.White;
                    Console.Write("  [{0,2}]", /*val*/eq.index);

                    Console.ForegroundColor = eq.status == 0 ? ConsoleColor.DarkYellow :
                        eq.status == 1 ? ConsoleColor.DarkGreen :
                        eq.status == 2 ? ConsoleColor.DarkRed : ConsoleColor.DarkCyan;

                    Console.Write(
                        String.Format("{0,10} [{1,3}: {5}], {2,7}: {3}, [{4,11}]",
                            eq.name,
                            String.Format("{0}{1}{2}", eq.tipo == GearMibHelper.eRadioTipo.Receptor ? "R" : "T",
                                            eq.banda == GearMibHelper.eRadioBanda.VHF ? "V" : "U",
                                            eq.modo == GearMibHelper.eRadioModo.Main ? "M" : "S"),
                            eq.frecuency,
                            eq.status == 0 ? "L" : eq.status == 1 ? "R" : eq.status == 2 ? "F" : "T",    // Estado.
                            String.Format("{0,2},{1,2},{2,2},{3,2}", eq.channel_spacing, eq.modulation, eq.carrier_offset, eq.power),
                            roip.strEstado(eq.name)
                        ));

                    TxRow = eq.tipo == GearMibHelper.eRadioTipo.Transmisor ? TxRow + 1 : TxRow;
                    RxRow = eq.tipo == GearMibHelper.eRadioTipo.Receptor ? RxRow + 1 : RxRow;

                    //if ((val % 2) != 0) Console.WriteLine();
                    //val += 1;
                }
            }

            Console.SetCursorPosition(0, TxRow > RxRow ? TxRow+1 : RxRow+1);
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("      [B] Banda. [T] Tipo");

            Console.WriteLine();


            Console.WriteLine();
            Console.ForegroundColor = last;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="status"></param>
        static void GearStatusSet(int status)
        {
            if (NumeroDeEquipos > 0)
            {
                int indice = GetIndice("Introduzca Indice de Equipo: ", NumeroDeEquipos+1, 1);
                _mib.StatusGearSet(indice, status);
                
                //var equipo = _mib.Gears.Where(eq => eq.index == indice).FirstOrDefault();
                //if (status == 1)
                //    roip.Habilita(equipo.name);
                //else
                //    roip.Inhabilita(equipo.name);
                return;
            }
            else
            {
                ConsoleSameLine("No hay equipos simulados. Pulse una tecla para continuar...");
            }
            Console.ReadKey(true);
        }

        static void GearStatusSipSet(int status)
        {
            if (NumeroDeEquipos > 0)
            {
                int indice = GetIndice("Introduzca Indice de Equipo: ", NumeroDeEquipos + 1, 1);
                var equipo = _mib.Gears.Where(eq => eq.index == indice).FirstOrDefault();
                if (status == 0)
                    roip.Inhabilita(equipo.name);
                else
                    roip.Habilita(equipo.name);
                return;
            }
            else
            {
                ConsoleSameLine("No hay equipos simulados. Pulse una tecla para continuar...");
            }
            Console.ReadKey(true);
        }

        /// <summary>
        /// 
        /// </summary>
        static void AllStatusSet()
        {
            int status = GetIndice("Introduzca Estado (0:Local, 1:Ok, 2:Fallo, 3:Timeout): ", 4, 0);
            for (int index = 0; index < NumeroDeEquipos; index++)
            {
                _mib.StatusGearSet(index + 1, status);

                //var equipo = _mib.Gears.Where(eq => eq.index == (index+1)).FirstOrDefault();
                //if (status == 1)
                //    roip.Habilita(equipo.name);
                //else
                //    roip.Inhabilita(equipo.name);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        static void AllSipStatusSet()
        {
            int status = GetIndice("Introduzca Estado (0:Fallo, 1:Ok): ", 2, 0);
            foreach (GearMibHelper.GearData equipo in _mib.Gears)
            {
                if (status == 1)
                    roip.Habilita(equipo.name);
                else
                    roip.Inhabilita(equipo.name);
            }
        }

        static void SecuenciasProgramadas()
        {
            int Secuencia = GetIndice("Introduzca Secuencia (0:A/D Tx, 1: A/D Rx, 2: Sec01, 3: Sec02): ", 4, 0);
            switch (Secuencia)
            {
                case 0:
                        Task.Run(() =>
                        {
                            int veces = 10;
                            do
                            {
                                System.Threading.Thread.Sleep(2000);
                                _mib.StatusGearSet(8, 2);
                                System.Threading.Thread.Sleep(2000);
                                _mib.StatusGearSet(8, 1);
                            } while (--veces > 0);
                        });
                    break;
                case 1:
                        Task.Run(() =>
                        {
                            int veces = 10;
                            do
                            {
                                System.Threading.Thread.Sleep(2000);
                                _mib.StatusGearSet(1, 2);
                                System.Threading.Thread.Sleep(2000);
                                _mib.StatusGearSet(1, 1);
                            } while (--veces > 0);
                        });
                    break;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="msg"></param>
        /// <returns></returns>
        static int GetIndice(string msg, int max, int min = 0)
        {
            int indice = 0;

            while (true)
            {
                ConsoleSameLine(msg);
                string dataread = Console.ReadLine();
                //if (dataread == string.Empty)
                //    return -1;
                if (int.TryParse(dataread, out indice) && (indice >= min && indice < max))
                    return indice;
                Console.Write("Dato Erroneo. Pulse una tecla para Continuar....");
                Console.ReadKey(true);
                ConsoleClearCurrentLine();
                Console.SetCursorPosition(0, Console.CursorTop - 1);    // Se posicione en la linea anterior...
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="msg"></param>
        static void ConsoleSameLine(string msg)
        {
            ConsoleClearCurrentLine();
            Console.Write(msg);
        }

        /// <summary>
        /// 
        /// </summary>
        static void ConsoleClearCurrentLine()
        {
            int currentLineCursor = Console.CursorTop;
            Console.SetCursorPosition(0, Console.CursorTop);
            Console.Write(new string(' ', Console.WindowWidth));
            Console.SetCursorPosition(0, currentLineCursor);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        static int timer_largo = 0;
        static void ElapseTimerHandler(object sender, ElapsedEventArgs e)
        {
            ++timer_largo;
            if (timer_largo >= 60)
            {
                if (modo != 0)
                {
                    int equipo = rnd.Next(0, NumeroDeEquipos) + 1;
                    int status = rnd.Next(0, 4);
                    _mib.StatusGearSet(equipo, status);

                    PrintTitulo();
                    PrintListaEquipos();
                    PrintMenu();
                }
                timer_largo = 0;
            }
            timer.Enabled = true;
        }

    }
}
