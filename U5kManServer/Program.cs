using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceProcess;
using System.Text;

using System.Threading;
using System.Threading.Tasks;
using System.Globalization;

using NucleoGeneric;

using Utilities;


namespace U5kManServer
{
    static class Program 
    {
        static string appGuid = "{551D51A6-562B-4438-A07F-E837710DCE95}";
        /// <summary>
        /// Punto de entrada principal para la aplicación.
        /// </summary>
        static void Main(string[] args)
        {
            //List<string> users = new List<string>()
            //{
            //    EncryptionHelper.CAE_cifrar("dfnucleo##0##ulisesv5000##0005vsesilu"),
            //    EncryptionHelper.CAE_cifrar("sdfnucleo##1##ulisesv5000##0005vsesilu"),
            //    EncryptionHelper.CAE_cifrar("edfnucleo##2##ulisesv5000##0005vsesilu")
            //};
            //Properties.u5kManServer.Default.Snmp_V3Users.Clear();
            //Properties.u5kManServer.Default.Snmp_V3Users.AddRange(users.ToArray());
            //Properties.u5kManServer.Default.Save();
            using (Mutex mutex = new Mutex(false, "Global\\" + appGuid))
            {
                if (!mutex.WaitOne(0, false))
                {
                    NLog.LogManager.GetLogger("U5kManServer").Fatal("Instance already running");
                    return;
                }
                (new uv5kSgmManProgram()).Run(args);
            }
        }
    }

    /// <summary>
    /// 
    /// </summary>
    class uv5kSgmManProgram : BaseCode
    {
        public void Run(string[] args)
        {
            // CultureInfo culture = new CultureInfo(Properties.u5kManServer.Default.Idioma);
            // Thread.CurrentThread.CurrentCulture = culture;
            // Thread.CurrentThread.CurrentUICulture = culture;

            System.IO.Directory.SetCurrentDirectory(System.AppDomain.CurrentDomain.BaseDirectory);

            AppDomain currentDomain = AppDomain.CurrentDomain;
            currentDomain.UnhandledException += new UnhandledExceptionEventHandler(MyGlobalExceptionHandler);
#if DEBUG1
            if (true)
#else
            if (args.Contains("-console"))
#endif
            {
                var app = new U5kManMain();
                app.StartDebug();

                LogInfo<uv5kSgmManProgram>(String.Format("Arrancando Modo Consola en {0}", System.IO.Directory.GetCurrentDirectory()));
                System.Console.WriteLine("Version {0}. Press 'q' to Quit.", U5kGenericos.Version);
                char key;
                while ((key = Console.ReadKey(true).KeyChar) != 'q')
                {
                    ConfigCultureSet();
#if DEBUG
                    if (key == 'm')
                    {
                        ClusterSim.CambiaEstadoNodoCluster(0);
                    }
                    if (key == 'M')
                    {
                        ClusterSim.CambiaPresenciaNodoCluster(0);
                    }
                    else if (key == 's')
                    {
                        ClusterSim.CambiaEstadoNodoCluster(1);
                    }
                    else if (key == 'S')
                    {
                        ClusterSim.CambiaPresenciaNodoCluster(1);
                    }
                    else if (key == 'c')
                    {
                        ClusterSim.ConmutaConExcepcion();
                    }
                    else if (key == 'x')
                    {
                        ClusterSim.ConmutaConExcepcion(true);
                    }
                    else if (key == 'i')
                    {
                        HistThread.TestAddInci(1, true);
                    }
                    else if (key == 't')
                    {
                        // Provocar la toma del semaforo durante mucho tiempo....
                        LogInfo<uv5kSgmManProgram>("T Key Pressed...");
                        Task.Factory.StartNew(() => GlobalServices.GetWriteAccess((gdata) => Task.Delay(TimeSpan.FromSeconds(60)).Wait()));
                    }
                    else if (key == 'n')
                    {
                        Utilities.NICS.ShowInterfaceSummary();
                    }
                    else if (key == 'r')
                    {
                        U5kGenericos.ResetService = true;
                    }
#endif
                }

                LogInfo<uv5kSgmManProgram>("Saliendo Modo Consola ...");
                app.StopDebug();
                System.Threading.Thread.Sleep(1000);
                LogInfo<uv5kSgmManProgram>("Fin del Programa");
            }
            else
            {
                LogInfo<uv5kSgmManProgram>(String.Format("Arrancando en {0}", System.IO.Directory.GetCurrentDirectory()));
                ServiceBase[] ServicesToRun;
                ServicesToRun = new ServiceBase[] { new U5kManMain() };
                ServiceBase.Run(ServicesToRun);
            }

        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        void MyGlobalExceptionHandler(object sender, UnhandledExceptionEventArgs args)
        {
            Exception x = (Exception)args.ExceptionObject;
            LogException<uv5kSgmManProgram>("MyGlobalExceptionHandler", x, true);
        }

        void Test()
        {
            try
            {
                System.Xml.XmlDocument doc = new System.Xml.XmlDocument();
                doc.Load("c:\\web.config");
                System.Xml.XmlElement Root = doc.DocumentElement;

                Root.SelectNodes("SactaUsuarioSection/sectores")[0].Attributes["idSectores"].Value = "1,1";

                doc.Save("c:\\web.config");
            }
            catch (Exception )
            {
            }
        }
    }
 
}
