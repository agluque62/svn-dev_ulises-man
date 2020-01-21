using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using CoreSipNet;

namespace RadioVoipSim
{
    class Program
    {
        static NLog.Logger log = NLog.LogManager.GetCurrentClassLogger();
        static RoIpSipManager roipManager = new RoIpSipManager();
        static void Main(string[] args)
        {
            SetThreadPoolSize();

            try
            {
                log.Debug("Arrancando....");
                Selected = -1;
                /** */
                roipManager.Init();
                log.Debug("Agente Inicializado.");
                /** */
                roipManager.Start();
                log.Debug("Agente Arrancado.");
                bool salir = false;

                Task.Factory.StartNew(() =>
                {
                    do
                    {
                        PrintTitulo();
                        PrintListaEquipos();
                        PrintMenu();
                        
                        while (roipManager.EventRefresh == false)
                            System.Threading.Thread.Sleep(100);

                        roipManager.EventRefresh = false;
                    } while (!salir);
                });

                do
                {
                    while (Console.KeyAvailable == false)
                        Thread.Sleep(250); // Loop until input is entered.

                    ConsoleKeyInfo key = Console.ReadKey(true);
                    switch (key.Key)
                    {
                        case ConsoleKey.Spacebar:
                            if ((key.Modifiers & ConsoleModifiers.Shift) != 0)
                            {
                                if (Selected == -1)
                                    Selected = -1;
                                else if (Selected == 0)
                                    Selected = roipManager.RadioCfg.Count - 1;
                                else
                                    Selected = Selected - 1;
                            }
                            else 
                            {
                                Selected = Selected == -1 ? -1 : Selected + 1;
                                if (Selected >= roipManager.RadioCfg.Count)
                                    Selected = 0;
                            }
                            break;

                        case ConsoleKey.Escape:
                            salir = true;
                            break;

                        case ConsoleKey.D1:
                            roipManager.EnableSwitch(SelectedName);
                            break;
                        case ConsoleKey.D2:
                            roipManager.ErrorSwitch(SelectedName);
                            break;
                        case ConsoleKey.D3:
                            roipManager.SquelchSwitch(SelectedName);
                            break;
                    }
                    roipManager.EventRefresh = true;

                } while (!salir);
                
                /** */
                roipManager.End();
                log.Debug("Agente Finalizado");
            }
            catch (Exception x)
            {
                log.Error("Excepcion: {0}", x.Message);
            }

            RestoreThreadPoolSize();
        }
        static void PrintTitulo()
        {
            Console.Clear();
            ConsoleColor last = Console.ForegroundColor;
            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine("Simulador Equipos Radio VoIP. DFNucleo 2018");
            Console.WriteLine("Escuchando en {0}:{1}", 
                Properties.Settings.Default.IpBase, Properties.Settings.Default.SipPort);
            Console.WriteLine();
            Console.ForegroundColor = last;
        }
        static void PrintListaEquipos()
        {
            ConsoleColor lastForegroundColor = Console.ForegroundColor;
            ConsoleColor lastBackgroundColor = Console.BackgroundColor;

            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine("Lista de Equipos Simulados.\n");

            var equipos = roipManager.RadioCfg;
            int index = 0;
            int left = 0;
            int top = Console.CursorTop;

            Selected = (Selected == -1 && equipos.Count > 0) ? 0 : Selected;

            foreach (RoIpSipManager.RoIpCallInfo item in equipos)
            {
                left = (index % 2) == 0 ? 0 : 40;
                Console.SetCursorPosition(left, top);

                Console.ForegroundColor = index == Selected ? ConsoleColor.Red : ConsoleColor.Gray;
                Console.BackgroundColor = index == Selected ? ConsoleColor.DarkBlue : lastBackgroundColor;

                Console.Write("  [{0,2}]", index);
                Console.ForegroundColor = item.ColorEstado;

                Console.Write(
                    String.Format(" {0,-10}: "+/*IDC={4,10}, HAB={1,6}, */"SQH: {3,-3}, PTT: {5,-3}",
                        item.name, item.habilitado, item.state, 
                        item.Squelch ? "On" : "Off", 
                        item.call, 
                        item.ptt ? "On" : "Off"
                    ));                
                index++;
                if ((index % 2) == 0)
                {
                    top++;
                    Console.WriteLine();
                }
            }
            Console.WriteLine();            

            Console.ForegroundColor = lastForegroundColor;
            Console.BackgroundColor = lastBackgroundColor;
        }
        static void PrintMenu()
        {
            ConsoleColor last = Console.ForegroundColor;

            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine();
            Console.Write("[Barra/Shift+Barra] Selecciona Equipo. Seleccionado: ");
            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine(SelectedName);
            Console.WriteLine();

            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine("Opciones:");
            Console.ForegroundColor = ConsoleColor.Yellow;

            Console.WriteLine("      [1] Activa / Desactiva  " + SelectedName);
            Console.WriteLine("      [2] Error / No Error en " + SelectedName);
            Console.WriteLine("      [3] Squelch On / Off en " + SelectedName);

            Console.WriteLine();
            Console.WriteLine("  [ESC] Salir");
            Console.WriteLine();

            Console.ForegroundColor = last; 
        }

        static int Selected { get; set; }
        static string SelectedName
        {
            get
            {
                return Selected == -1 ? "Ninguno" : roipManager.RadioCfg[Selected].name;
            }
        }
        static int minWorker, minIOC;
        static void SetThreadPoolSize()
        {
            // Get the current settings.
            ThreadPool.GetMinThreads(out minWorker, out minIOC);
            if (ThreadPool.SetMinThreads(20, 20) == false)
            {
                Console.WriteLine("Error en SetMinThreads... Pulse ENTER");
                Console.ReadLine();
            }
        }
        static void RestoreThreadPoolSize()
        {
            if (ThreadPool.SetMinThreads(minWorker, minIOC) == false)
            {
                Console.WriteLine("Error en SetMinThreads... Pulse ENTER");
                Console.ReadLine();
            }
        }


    }
}
