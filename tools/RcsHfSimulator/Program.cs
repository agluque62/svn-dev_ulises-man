using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using RcsHfSimulator.HfSnmp;

namespace RcsHfSimulator
{
    class Program
    {
        static string SnmpIp = Properties.Settings.Default.IpPropia;
        static int SnmpPort = Properties.Settings.Default.PortSnmp;
        static HfTxMib _mib = new HfTxMib();
        static int NumeroDeEquipos = 0;

        static int origWidth;
        static int origHeight;

        static Random rnd = new Random(DateTime.Now.Millisecond);

        static void Main(string[] args)
        {
            SnmpAgent.Init(SnmpIp, null, SnmpPort, 1162);

            _mib.Load();

            SnmpAgent.PduSetReceived += _mib.PduSetReceived_handler;
            SnmpAgent.RespondToGet += _mib.PduGetReceived_handler;

            SnmpAgent.Start();

            origWidth = Console.WindowWidth;
            origHeight = Console.WindowHeight;
            Console.SetWindowSize(120, 25);

            bool exit = false;
            Task refresh = Task.Factory.StartNew(() =>
            {
                while (!exit)
                {
                    System.Threading.Thread.Sleep(2000);
                    Refresh();
                }
                Console.WriteLine("Saliendo Proceso....");
            });

            ConsoleKeyInfo result;
            do
            {
                Refresh();
                result = Console.ReadKey(true);
                lock (lockRefresh)
                {
                    switch (result.Key)
                    {
                        case ConsoleKey.M:
                            break;
                        case ConsoleKey.B:
                            break;
                        case ConsoleKey.T:
                            break;

                        case ConsoleKey.D0:     // 
                            SetEstadoEquipo((int)HFMibHelper.eEstadoEquipo.NoPresente);
                            break;
                        case ConsoleKey.D1:     // 
                            SetEstadoEquipo((int)HFMibHelper.eEstadoEquipo.Disponible);
                            break;
                        case ConsoleKey.D2:     // 
                            SetEstadoEquipo((int)HFMibHelper.eEstadoEquipo.EnFalloGlobal);
                            break;
                        case ConsoleKey.D3:
                            SetEstadoEquipo((int)HFMibHelper.eEstadoEquipo.EnFalloSintonizar);
                            break;

                        case ConsoleKey.D4:     // SNMP Todos a ...
                            AllStatusSet();
                            break;

                        case ConsoleKey.D5:
                            break;
                        case ConsoleKey.D6:
                            break;
                        case ConsoleKey.D7:
                            break;

                        case ConsoleKey.S:      // Secuencia Programada.
                            SecuenciasProgramadas();
                            break;
                    }
                }
            } while (result.Key != ConsoleKey.Escape);
            
            
            exit = true;
            Task.WaitAll(refresh);

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
            Console.WriteLine("Simulador Transmisores HF Telemandados. DFNucleo 2017");
            Console.WriteLine("Escuchando en {0}:{1}", SnmpIp, SnmpPort);
            Console.WriteLine();
            Console.ForegroundColor = last;
        }

        static void PrintMenu()
        {
            ConsoleColor last = Console.ForegroundColor;
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine("Opciones:\n");
            Console.ForegroundColor = ConsoleColor.Yellow;
            
            Console.WriteLine("      [0] Equipo => SNMP-NoPresente     ");
            Console.WriteLine("      [1] Equipo => SNMP-Ok  ");
            Console.WriteLine("      [2] Equipo => SNMP-Fallo Global");
            Console.WriteLine("      [3] Equipo => SNMP-Fallo Sintonizar  ");

            Console.WriteLine("      [4] Todos a SNMP ...  ");
            Console.WriteLine();
            //Console.WriteLine("      [5] Equipo => SIP-OK  ");
            //Console.WriteLine("      [6] Equipo => SIP-Error  ");
            //Console.WriteLine("      [7] Todos a SIP ... ");
            //Console.WriteLine();
            //Console.WriteLine("      [S] Secuencias Programadas...  ");

            Console.WriteLine();
            Console.WriteLine("  [ESC] Salir");
            Console.WriteLine();

            Console.ForegroundColor = last;
        }

        static void PrintListaEquipos()
        {
            //int val = 0; 

            ConsoleColor last = Console.ForegroundColor;
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine("Lista de Equipos Simulados.\n");

            List<HFMibHelper.HFTxData> equipos = _mib.Transmisores;
            NumeroDeEquipos = equipos.Count;

            int TxRow = Console.CursorTop;
            int RxRow = Console.CursorTop;

            int index = 0;
            foreach (HFMibHelper.HFTxData eq in equipos)
            {

                    Console.ForegroundColor = ConsoleColor.White;
                    Console.Write("  [{0,2}]", index);

                    Console.ForegroundColor = eq.ColorEstado;

                    Console.WriteLine(
                        String.Format("{0,10} ({3}), [{1,8}]-[{4,5}], {2}",
                            eq.id,
                            eq.frecuency,
                            eq.TextoEstado,
                            eq.oid,
                            eq.Mode
                        ));


                index++;                
            }

            Console.WriteLine();
            Console.ForegroundColor = last;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="status"></param>
        static void SetEstadoEquipo(int status)
        {
            if (NumeroDeEquipos > 0)
            {
                int indice = GetIndice("Introduzca Indice de Equipo: ", NumeroDeEquipos, 0);
                
                HFMibHelper.HFTxData equipo = _mib.Transmisores.ElementAt(indice);
                _mib.SetTxStatus(equipo, status);
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
            //if (NumeroDeEquipos > 0)
            //{
            //    int indice = GetIndice("Introduzca Indice de Equipo: ", NumeroDeEquipos + 1, 1);
            //    var equipo = _mib.Gears.Where(eq => eq.index == indice).FirstOrDefault();
            //    if (status == 0)
            //        roip.Inhabilita(equipo.name);
            //    else
            //        roip.Habilita(equipo.name);
            //    return;
            //}
            //else
            //{
            //    ConsoleSameLine("No hay equipos simulados. Pulse una tecla para continuar...");
            //}
            Console.ReadKey(true);
        }

        /// <summary>
        /// 
        /// </summary>
        static void AllStatusSet()
        {
            int status = GetIndice("Introduzca Estado (0: No Presente, 1: Ok, 2: Fallo Global, 3: Fallo Sintonizar): ", 4, 0);
            for (int index = 0; index < NumeroDeEquipos; index++)
            {
                _mib.SetTxStatus(_mib.Transmisores.ElementAt(index), status);

            }
        }

        /// <summary>
        /// 
        /// </summary>
        static void AllSipStatusSet()
        {
            //int status = GetIndice("Introduzca Estado (0:Fallo, 1:Ok): ", 2, 0);
            //foreach (GearMibHelper.GearData equipo in _mib.Gears)
            //{
            //    if (status == 1)
            //        roip.Habilita(equipo.name);
            //    else
            //        roip.Inhabilita(equipo.name);
            //}
        }

        static void SecuenciasProgramadas()
        {
            //int Secuencia = GetIndice("Introduzca Secuencia (0:A/D Tx, 1: A/D Rx, 2: Sec01, 3: Sec02): ", 4, 0);
            //switch (Secuencia)
            //{
            //    case 0:
            //        Task.Run(() =>
            //        {
            //            int veces = 10;
            //            do
            //            {
            //                System.Threading.Thread.Sleep(2000);
            //                _mib.StatusGearSet(8, 2);
            //                System.Threading.Thread.Sleep(2000);
            //                _mib.StatusGearSet(8, 1);
            //            } while (--veces > 0);
            //        });
            //        break;
            //    case 1:
            //        Task.Run(() =>
            //        {
            //            int veces = 10;
            //            do
            //            {
            //                System.Threading.Thread.Sleep(2000);
            //                _mib.StatusGearSet(1, 2);
            //                System.Threading.Thread.Sleep(2000);
            //                _mib.StatusGearSet(1, 1);
            //            } while (--veces > 0);
            //        });
            //        break;
            //}
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
        //static int timer_largo = 0;
        //static void ElapseTimerHandler(object sender, ElapsedEventArgs e)
        //{
        //    ++timer_largo;
        //    if (timer_largo >= 60)
        //    {
        //        if (modo != 0)
        //        {
        //            int equipo = rnd.Next(0, NumeroDeEquipos) + 1;
        //            int status = rnd.Next(0, 4);
        //            _mib.StatusGearSet(equipo, status);

        //            PrintTitulo();
        //            PrintListaEquipos();
        //            PrintMenu();
        //        }
        //        timer_largo = 0;
        //    }
        //    timer.Enabled = true;
        //}
        static object lockRefresh = new object();
        static void Refresh()
        {
            lock (lockRefresh)
            {
                PrintTitulo();
                PrintListaEquipos();
                PrintMenu();
            }
        }
    }
}
