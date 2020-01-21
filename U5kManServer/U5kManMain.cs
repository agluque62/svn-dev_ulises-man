#define MASTER_SLAVE
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.ServiceProcess;
using System.Text;

using System.Net;
using System.Net.Sockets;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using ClusterLib;

using System.Threading;
using System.Threading.Tasks;
using System.ServiceModel;
using System.ServiceModel.Description;

using U5kBaseDatos;
using NucleoGeneric;
using Utilities;

namespace U5kManServer
{
    public partial class U5kManMain : ServiceBase
    {
        /// <summary>
        /// 
        /// </summary>
        // Logger _Logger = LogManager.GetCurrentClassLogger();
        U5kServiceMain thMainService;
        /// <summary>
        /// 
        /// </summary>
        public U5kManMain()
        {
            InitializeComponent();
            thMainService = new U5kServiceMain();
        }

        /// <summary>
        /// Punto de Entrada Modo SERVICIO.
        /// </summary>
        /// <param name="args"></param>
        protected override void OnStart(string[] args)
        {
#if MASTER_SLAVE
            // thMainService.Start(this);
            BaseCode.Log<U5kManMain>(NLog.LogLevel.Debug, String.Format("Starting Service"), eIncidencias.IGNORE);
            thMainService.Start();
            BaseCode.Log<U5kManMain>(NLog.LogLevel.Debug, String.Format("Service Active"), eIncidencias.IGNORE);
#else
            _started = ActivateService();
            if (_started == true)
            {
                U5kManService._main.Start();
            }
#endif
        }

        /// <summary>
        /// Punto de Salida Modo SERVICIO.
        /// </summary>
        protected override void OnStop()
        {
#if MASTER_SLAVE
            //bMainServiceThreadExit = true;
            //thMainService.Join(5000);
            BaseCode.Log<U5kManMain>(NLog.LogLevel.Debug, String.Format("Finishing service"), eIncidencias.IGNORE);
            thMainService.Stop(TimeSpan.FromSeconds(20));
            BaseCode.Log<U5kManMain>(NLog.LogLevel.Debug, String.Format("Service Finished"), eIncidencias.IGNORE);
#else
            if (_started)
            {
                U5kManService._main.Stop();
                DeactivateService();
                _Logger.Warn("Fin del Programa");
            }
#endif
        }

        /// <summary>
        /// Punto de Entrada CONSOLE...
        /// </summary>
        public void StartDebug()
        {
#if MASTER_SLAVE
            // thMainService.Start(this);
            thMainService.Start();
#else
            _started = ActivateService();
            if (_started == true)
            {
                U5kManService._main.Start();
            }
#endif
        }

        /// <summary>
        /// Punto de Salida CONSOLE.
        /// </summary>
        public void StopDebug()
        {
#if MASTER_SLAVE
            //bMainServiceThreadExit = true;
            //thMainService.Join(5000);
            thMainService.Stop(TimeSpan.FromSeconds(20));
#else
            if (_started == true)
            {
                U5kManService._main.Stop();
                DeactivateService();
                _Logger.Warn("Fin del Programa");
            }
#endif
        }

    }

    class U5kServiceMain : NGThread
    {
        void GeneraIncidencia(int scv, eIncidencias inci, eTiposInci thw, string idhw, params object[] parametros)
        {
            RecordEvent<U5kServiceMain>(DateTime.Now, inci, thw, idhw, parametros);
        }

        public U5kServiceMain()
        {
            Name = "U5kServiceMain";
        }
        /// <summary>
        /// 
        /// </summary>
        protected override void Run()
        {
            U5kGenericos.TraceCurrentThread(this.GetType().Name);
            /** */
            SetThreadPoolSize();

            U5kGenericos.ResetService = false;
            //U5kGenericos.SetCurrentCulture();
            U5kManService.GlobalData = new U5kManStdData();

            // Arranca el Servicio WEB
            _started = ActivateService();

            // Arranca los procesos...
            U5kManService._main.Start();

            // 20170802. Arranca el Servicio de Estadisticas.
            U5kEstadisticaProc.Estadisticas = new U5kEstadisticaProc(
                //GeneraIncidencia,
                delegate ()
                {
                    return U5kManService.Database;
                });
            U5kEstadisticaProc.Estadisticas.Start();
            /*****************************************************/
#if DEBUG1
            DateTime now = DateTime.Now.AddMinutes(1);
            SetUpTimerBackup(new TimeSpan(now.Hour, now.Minute, 0));
            //SetupTimerTest(new TimeSpan(0, 0, 20));
#else
            SetUpTimerBackup(new TimeSpan(0, 15, 0));
#endif
            Decimal interval = Properties.u5kManServer.Default.SpvInterval;
            using (timer = new TaskTimer(new TimeSpan(0, 0, 0, 0, Decimal.ToInt32(interval)), this.Cancel))
            {
                while (IsRunning())
                {
                    try
                    {
                        if (CheckBdt())
                        {
#if CheckingAlls
                            /** Bloque 1. Chequear el Master y la Configuracion */
                            bool IamMaster = false;
                            if (U5kManService._std.wrAccAcquire())
                            {
                                try
                                {
                                    U5KStdGeneral stdg = U5kManService._std.STDG;
                                    IamMaster = CheckSiMaster(stdg);
                                    if (IamMaster)
                                    {
                                        CheckCambioConfig(bMaster, stdg);

                                        if (U5kGenericos.ResetService)
                                        {
                                            U5kGenericos.ResetService = false;
                                            if (bMaster)
                                            {
                                                U5kManService.Reset();
                                            }
                                        }
                                    }
                                }
                                catch(Exception x)
                                {
                                    throw x;
                                }
                                finally
                                {
                                    U5KStdGeneral stdg = U5kManService._std.STDG;
                                    U5kManService._std.wrAccRelease();
                                }
                            }
                            /** Bloque 2. Chequear el Sacta */
                            if (U5kManService._std.wrAccAcquire())
                            {
                                try
                                {
                                    U5KStdGeneral stdg = U5kManService._std.STDG;
                                    CheckSactaService(IamMaster, stdg);
                                }
                                catch (Exception x)
                                {
                                    throw x;
                                }
                                finally
                                {
                                    U5KStdGeneral stdg = U5kManService._std.STDG;
                                    U5kManService._std.wrAccRelease();
                                }
                            }
                            /** Bloque 3. WebServer y Lanes. */
                            if (U5kManService._std.wrAccAcquire())
                            {
                                try
                                {
                                    U5KStdGeneral stdg = U5kManService._std.STDG;
                                    CheckWebServer();
                                    CheckLanesAndNtpSync(stdg);
                                }
                                catch (Exception x)
                                {
                                    throw x;
                                }
                                finally
                                {
                                    U5KStdGeneral stdg = U5kManService._std.STDG;
                                    U5kManService._std.wrAccRelease();
                                }
                            }
#else
                            //if (U5kManService._std.wrAccAcquire())
                            {
                                try
                                {
                                    //U5KStdGeneral stdg = U5kManService._std.STDG;
                                    if (CheckSiMaster(/*stdg*/))
                                    {
                                        CheckCambioConfig(bMaster/*, stdg*/);

                                        if (U5kGenericos.ResetService)
                                        {
                                            U5kGenericos.ResetService = false;
                                            if (bMaster)
                                            {
                                                GlobalServices.GetWriteAccess((data) =>
                                                {
                                                    U5kManService.Reset();
                                                });
                                            }
                                        }
                                        CheckSactaService(/*stdg.SactaServiceEnabled, stdg*/);
                                    }
                                    /** 20180730. Si se es SLAVE no se chequea el SACTA... */
                                    //else
                                    //{
                                    //    CheckSactaService(false, stdg);
                                    //}
                                    CheckWebServer();
                                    CheckLanesAndNtpSync(/*stdg*/);
                                    //U5kManService._std.STDG = stdg;
                                }
                                catch (Exception x)
                                {
                                    throw x;
                                }
                                //finally
                                //{
                                //    U5kManService._std.wrAccRelease();
                                //}
                            }
#endif
                        }
                        else
                        {
                            // No hay Conexion a base de Datos.                        
                            webapp.Enable = false;
                            webapp.DisableCause = idiomas.strings.WEB_DIS_CAUSE_01; // "Servicio sin conexion a la base de Datos.";
                        }
                    }
                    catch (Exception x)
                    {
                        if (x is ThreadAbortException)
                        {
                            Thread.ResetAbort();
                            break;
                        }
                        LogException<U5kServiceMain>("", x);
                    }
                    finally
                    {
                    }
                    GoToSleepInTimer();
                }
            }
            // 20170802. Parar el Servicio de Estadisticas.
            U5kEstadisticaProc.Estadisticas.Stop();
            /*****************************************************/

            if (U5kManService._main.Running)
                U5kManService._main.Stop(TimeSpan.FromSeconds(20));

            CheckSactaService(true);
            DeactivateService();
            /** */
            RestoreThreadPoolSize();
            bMaster = false;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        protected bool ActivateService()
        {
            try
            {
#if DEBUG
                // ExceptionSim.CheckExceptionActivateService();
#endif
                if (Properties.u5kManServer.Default.TipoWeb == 0)
                {
                    Uri baseAddress = new Uri("http://localhost:8080/u5kman");

                    // Create the Service Host.
                    host = new ServiceHost(typeof(U5kManService), baseAddress);
                    WSHttpBinding wb = new WSHttpBinding();
                    // Enable metadata publishing.                
                    ServiceMetadataBehavior smb = new ServiceMetadataBehavior();
                    smb.HttpGetEnabled = true;
                    smb.MetadataExporter.PolicyVersion = PolicyVersion.Policy15;
                    host.Description.Behaviors.Add(smb);
                    //            
                    host.AddServiceEndpoint(typeof(IU5kManService), wb, host.BaseAddresses[0]/*baseAddress*/);
                    // Open the ServiceHost to start listening for messages.            
                    host.Open();

                    LogInfo<U5kServiceMain>("Service Running at: " + baseAddress);
                }
                else
                {
                    // Activar el WEB. Service...
                    webapp = new WebAppServer.U5kManWebApp();
                    webapp.Start();
                    LogInfo<U5kServiceMain>("WebAppServer.U5kManWebApp Running");
                }
            }
            catch (Exception x)
            {
                LogException<U5kServiceMain>("", x);
                return false;
            }
            return true;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        protected bool DeactivateService()
        {
            if (_started)
            {
                try
                {
#if DEBUG
                    // ExceptionSim.CheckExceptionDeactivateService();
#endif
                    if (Properties.u5kManServer.Default.TipoWeb == 0)
                        host.Close();
                    else
                    {
                        if (webapp != null)
                            webapp.Stop();
                    }
                }
                catch (Exception x)
                {
                    LogException<U5kServiceMain>("", x);
                    return false;
                }

            }
            return true;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="isMaster"></param>
        protected void CheckWebServer()
        {
            if (!_started)
            {
                _started = ActivateService();
            }
            else if (bMaster == true)
            {
                if (Properties.u5kManServer.Default.TipoWeb != 0)
                {
                    webapp.Enable = true;
                }
            }
            else
            {
                if (Properties.u5kManServer.Default.TipoWeb != 0)
                {
                    webapp.Enable = false;
                    webapp.DisableCause = idiomas.strings.WEB_DIS_CUASE_02/*"Servicio en Modo Slave..."*/;
                }
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        protected bool CheckSiMaster()
        {
            try
            {
                U5KStdGeneral.ClusterErrors ClusterError = U5KStdGeneral.ClusterErrors.NoError;
                bool _Master = false;

#if DEBUG
                bool ServidorDual = false;
#else
                bool ServidorDual = U5kManService.cfgSettings/*Properties.u5kManServer.Default*/.ServidorDual;
#endif
                if (ServidorDual == true)
                {

                    EstadoCluster cluster = null;
#if DEBUG1
                    List<U5kBdtService.BdtStdServ> serv = U5kManService.Database.GetClusterInfo("departamento");
                    cluster = new EstadoCluster()
                    {
                        EstadoNode1 = new EstadoNode() { Name = serv[0].Name, Estado = serv[0].Estado, Presencia = serv[0].Presencia },
                        EstadoNode2 = new EstadoNode() { Name = serv[1].Name, Estado = serv[1].Estado, Presencia = serv[1].Presencia }
                    };
#else
                    if (Properties.u5kManServer.Default.ClusterPollingMethod == (int)ClusterPollingMethods.Soap)
                    {
                        InterfazSOAPConfiguracion client = new InterfazSOAPConfiguracion();
                        cluster = client.GetEstadoCluster();
                    }
                    else if (Properties.u5kManServer.Default.ClusterPollingMethod == (int)ClusterPollingMethods.DataBase)
                    {
                        List<U5kBdtService.BdtStdServ> serv = U5kManService.Database.GetClusterInfo("departamento");
                        cluster = new EstadoCluster()
                        {
                            EstadoNode1 = new EstadoNode() { Name = serv[0].Name, Estado = serv[0].Estado, Presencia = serv[0].Presencia },
                            EstadoNode2 = new EstadoNode() { Name = serv[1].Name, Estado = serv[1].Estado, Presencia = serv[1].Presencia }
                        };
                    }
                    else
                    {
                        cluster = online_cluster.Update();
                    }

#endif
                    LogDebug<U5kServiceMain>(
                        String.Format("CheckSiMaster. Estado Cluster en {4} = ({0}:{1}, {2}:{3})",
                        cluster.EstadoNode1.Name, cluster.EstadoNode1.Estado, cluster.EstadoNode2.Name, cluster.EstadoNode2.Estado, System.Environment.MachineName));

                    /** Chequeo coherencia de nombres y estados... */
                    string MyName = System.Environment.MachineName;
                    if (MyName != cluster.EstadoNode1.Name && MyName != cluster.EstadoNode2.Name)
                    {
                        /** Error: Nombres no coinciden con el de la máquina */
                        ClusterError = U5KStdGeneral.ClusterErrors.NoLocalServerNameDetected;
                        LogError<U5kServiceMain>(
                            String.Format("Error: El nombre del PC no esta en el CLUSTER. Se asume rol de MASTER..."));
                        _Master = true;
                    }
                    else if (cluster.EstadoNode1.Name == cluster.EstadoNode2.Name)
                    {
                        /** Error: Nombres Repetidos */
                        ClusterError = U5KStdGeneral.ClusterErrors.RepeatedServerNamesDetected;
                        LogError<U5kServiceMain>(
                            String.Format("Error: Los dos nodos del CLUSTER tienen el mismo nombre. Se asume rol de MASTER..."));
                        _Master = true;
                    }
                    else
                    {
                        /** Chequeo las incoherencias de estado */
                        if ((cluster.EstadoNode1.Estado == (int)NodeState.NoValid && cluster.EstadoNode2.Estado == (int)NodeState.NoActive) ||
                            (cluster.EstadoNode1.Estado == (int)NodeState.NoActive && cluster.EstadoNode2.Estado == (int)NodeState.NoValid) ||
                            (cluster.EstadoNode1.Estado == (int)NodeState.NoActive && cluster.EstadoNode2.Estado == (int)NodeState.NoActive))
                        {
                            /** Hay Nodos activos y Ningun nodo MASTER */
                            ClusterError = U5KStdGeneral.ClusterErrors.NoMainNodeDetected;
                            LogError<U5kServiceMain>(
                                String.Format("Error: Hay Nodos activos y Ningun nodo MASTER. Se asume rol de MASTER..."));
                            _Master = true;
                        }
                        else if (cluster.EstadoNode1.Estado == (int)NodeState.Active && cluster.EstadoNode2.Estado == (int)NodeState.Active)
                        {
                            /** Los dos Nodos estan en MASTER */
                            ClusterError = U5KStdGeneral.ClusterErrors.AllNodesAreMain;
#if DEBUG
                            /*U5kManService.*/_Master = true;
#else
                            _Master = U5kGenericos.IsLocalIp(Properties.u5kManServer.Default.MySqlServer); 
#endif
                            LogError<U5kServiceMain>(
                                String.Format("Error: Los dos Nodos estan en MASTER. Se asume rol de {0}, por el criterio de localizacion de la IP virtual ({1})...",
                                U5kManService._Master ? "MASTER" : "STANDBY", Properties.u5kManServer.Default.MySqlServer));
                        }
                        else if (cluster.EstadoNode1.Estado == (int)NodeState.NoValid && cluster.EstadoNode2.Estado == (int)NodeState.NoValid)
                        {
                            /** No hay nodos activos */
                            /*stdg.*/ClusterError = U5KStdGeneral.ClusterErrors.NoActiveNodesDetected;
                            LogError<U5kServiceMain>(
                                String.Format("Error: No hay nodos activos. Se asume rol de MASTER..."));
                            /*U5kManService.*/_Master = true;
                        }
                        else
                        {
                            /** Utilizo el criterio de la INFO de CLUSTER para establecer el modo MASTER/STANDBY */
                            /*U5kManService.*/_Master = (MyName == cluster.EstadoNode1.Name && cluster.EstadoNode1.Estado == (int)NodeState.Active) ||
                                (MyName == cluster.EstadoNode2.Name && cluster.EstadoNode2.Estado == (int)NodeState.Active);
                        }
                    }

                    /** Gestion de las conmutaciones de modo */
                    GlobalServices.GetWriteAccess((data) =>
                    {
                        U5KStdGeneral stdg = data.STDG;

                        U5kManService._Master = _Master;
                        stdg.ClusterError = ClusterError;

                        if (!bMaster && U5kManService._Master)
                        {
                            LogInfo<U5kServiceMain>("Conmutando a PRINCIPAL...");
                            stdg.cfgVersion = stdg.cfgName = string.Empty;
                            U5kManService._main.InvalidateConfig();
                            bMaster = true;
                            LogWarn<U5kServiceMain>("Modo PRINCIPAL.");
                        }
                        else if (bMaster && !U5kManService._Master)
                        {
                            LogInfo<U5kServiceMain>("Conmutando a RESERVA...");
                            // Desactivarse.
                            U5kEstadisticaProc.Estadisticas.FromMasterToSlave();
                            stdg.cfgVersion = stdg.cfgName = string.Empty;
                            bMaster = false;
                            LogWarn<U5kServiceMain>("Modo RESERVA...");
                        }
                        else if (U5kManService._Master && U5kManService._main.Running == false)
                        {
                            /** 20180309. Posiblemente viene de una recuperacion de error de BDT */
                            LogInfo<U5kServiceMain>("Recuperando a PRINCIPAL...");
                            stdg.cfgVersion = stdg.cfgName = string.Empty;
                            U5kManService._main.InvalidateConfig();
                            U5kManService._main.Start();
                        }
                    });

                    /** Actualizar Datos de Servidores */
                    GlobalServices.GetWriteAccess((data) =>
                    {
                        U5KStdGeneral stdg = data.STDG;

                        StdServ serv1_anterior = new StdServ() { name = stdg.stdServ1.name, Estado = stdg.stdServ1.Estado, Seleccionado = stdg.stdServ1.Seleccionado };
                        StdServ serv2_anterior = new StdServ() { name = stdg.stdServ2.name, Estado = stdg.stdServ2.Estado, Seleccionado = stdg.stdServ2.Seleccionado };

                        /** Servidor 1*/
                        stdg.stdServ1.name = cluster.EstadoNode1.Name +
                            (stdg.ClusterError == U5KStdGeneral.ClusterErrors.NoError ? "" :
                             String.Format("\nCluster Error: {0}", stdg.ClusterError));
                        stdg.stdServ1.Estado = cluster.EstadoNode1.Presencia == true ? std.Ok : std.NoInfo;
                        stdg.stdServ1.Seleccionado = cluster.EstadoNode1.Estado == 0 || cluster.EstadoNode1.Estado == 1 ? sel.NoInfo :
                                cluster.EstadoNode1.Estado == 2 ? sel.Seleccionado : sel.NoSeleccionado;
                        /** Servidor 2*/
                        stdg.stdServ2.name = cluster.EstadoNode2.Name +
                            (stdg.ClusterError == U5KStdGeneral.ClusterErrors.NoError ? "" :
                             String.Format("\nCluster Error: {0}", stdg.ClusterError)); ;
                        stdg.stdServ2.Estado = cluster.EstadoNode2.Presencia == true ? std.Ok : std.NoInfo;
                        stdg.stdServ2.Seleccionado = cluster.EstadoNode2.Estado == 0 || cluster.EstadoNode2.Estado == 1 ? sel.NoInfo :
                                cluster.EstadoNode2.Estado == 2 ? sel.Seleccionado : sel.NoSeleccionado;

                        /** Generar el Historico de Activacion del Servidores */
                        if (U5kManService._Master)
                        {
                            ActivationDeactivationLog("CLUSTER SERVER 1", serv1_anterior, stdg.stdServ1);
                            ActivationDeactivationLog("CLUSTER SERVER 2", serv2_anterior, stdg.stdServ2);
                        }
                    });
                }
                else
                {
                    // Servidor no dual...                    
                    GlobalServices.GetWriteAccess((data) =>
                    {
                        U5KStdGeneral stdg = data.STDG;

                        stdg.stdServ1.name = System.Environment.MachineName;
                        stdg.stdServ1.Estado = std.Ok;
                        stdg.stdServ1.Seleccionado = sel.Seleccionado;
                        U5kManService._Master = true;
                        if (!bMaster && U5kManService._Master)
                        {
                            bMaster = true;
                            LogInfo<U5kServiceMain>("Modo No-DUAL");
                        }
                        /** Generar el Historico de Activacion del Servidor */
                    });
                }
            }
            catch (Exception x)
            {
                /** Si no puedo conectarme me quedo como estaba. La inicializacion es MASTER. */
                // U5kManService._Master = true;
                LogException<U5kServiceMain>("", x);
            }
            finally
            {
            }
            return U5kManService._Master;
        }
        protected void ActivationDeactivationLog(string Name, StdServ last, StdServ current)
        {
            eIncidencias inci = eIncidencias.IGNORE;
            eTiposInci ti = eTiposInci.TEH_SISTEMA;
            string info = Name;

            /** */
            if (last.Estado != current.Estado)
            {
                if (current.Estado == std.Ok)
                {
                    if (current.Seleccionado == sel.Seleccionado)
                    {
                        /** Activacion como PPAL */
                        inci = eIncidencias.IGRL_SRV_ACTIVE_MAIN;
                        info += " MAIN";
                    }
                    else
                    {
                        /** Activacion como RSVA */
                        inci = eIncidencias.IGRL_SRV_ACTIVE_STBY;
                        info += " STBY";
                    }
                }
                else
                {
                    /** Desactivacion */
                    inci = eIncidencias.IGRL_SRV_INACTIVE;
                    info += " OFF";
                }
            }
            else if (last.Seleccionado != current.Seleccionado)
            {
                if (current.Seleccionado == sel.Seleccionado)
                {
                    /** Conmutado a PPAL */
                    inci = eIncidencias.IGRL_SRV_ACTIVE_MAIN;
                    info += " MAIN";
                }
                else
                {
                    /** Conmutado a RSVA */
                    inci = eIncidencias.IGRL_SRV_ACTIVE_STBY;
                    info += " STBY";
                }
            }
            else
            {
                return;
            }
            GeneraIncidencia(0, inci, ti, "MTTO", new object[] { info, "", "", "", "", "" });
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="soyMaster"></param>
        /// <returns></returns>
        protected bool CheckCambioConfig(bool soyMaster/*, U5KStdGeneral stdg*/)
        {
            if (soyMaster == true)
            {
                try
                {
                    /** Si utilizo base de datos local, no puede haber cambios de configuracion */
                    if (Properties.u5kManServer.Default.TipoBdt == 1)
                    {
                        GlobalServices.GetWriteAccess((data) =>
                        {
                            U5KStdGeneral stdg = data.STDG;
                            stdg.cfgName = "local";
                            stdg.cfgVersion = "bdt-local";
                        });
                    }
                    else
                    {
                        string strVersion = "", strCfgName = "";
                        string mcast_ip = "";
                        long mcast_port = -1;
#if _IF_SOAP_
                        InterfazSOAPConfiguracion client = new InterfazSOAPConfiguracion();
                        strVersion = client.GetVersionConfiguracion("departamento");                        
#else
                        U5kManService.Database.GetCfgActiva("departamento", ref strCfgName, ref strVersion, ref mcast_ip, ref mcast_port);
                        U5kManService.st_config.mcast_conf_grp = mcast_ip;
                        U5kManService.st_config.mcast_conf_port_base = mcast_port;
#endif
                        GlobalServices.GetWriteAccess((data) =>
                        {
                            U5KStdGeneral stdg = data.STDG;

                            LogDebug<U5kServiceMain>("SpConfiguracion. Version: " + strVersion);
                            if (/*U5kManService._std._gen.cfgVersion != string.Empty && */stdg.cfgVersion != strVersion)
                            {
                                if (stdg.cfgVersion != string.Empty)
                                {
                                    LogInfo<U5kServiceMain>(
                                        String.Format("SpConfiguracion. Nueva Config {0} --> {1}", stdg.cfgVersion, strVersion));
                                    U5kManService._main.InvalidateConfig();
                                }
                                stdg.cfgVersion = strVersion;
                                stdg.cfgName = strCfgName;
                            }
                        });
                    }
                }
                catch (Exception x)
                {
                    LogException<U5kServiceMain>("", x);
                }
                finally
                {
                }
            }
            return false;
        }

        /// <summary>
        /// Para el Backup de Base de Datos...
        /// </summary>
        private System.Threading.Timer timerBackup = null;
        private void SetUpTimerBackup(TimeSpan alertTime)
        {
            DateTime current = DateTime.Now;
            TimeSpan timeToGo = alertTime - current.TimeOfDay;
            if (timeToGo < TimeSpan.Zero)
            {   // time already passed
                timeToGo = new TimeSpan(24, 0, 0) + timeToGo;
            }
            LogInfo<U5kServiceMain>(
                String.Format("Programando Backup para {0:MM/dd HH:mm}", current + timeToGo));
            this.timerBackup = new System.Threading.Timer(x =>
            {
                Properties.u5kManServer cfg = Properties.u5kManServer.Default;
                if (cfg.TipoBdt == 0)
                {
                    List<U5kiDbHelper.BackupInciItem> items =
                        U5kiDbHelper.NewBackup(cfg.MySqlServer, cfg.BdtSchema, cfg.MySqlUser, cfg.MySqlPwd, Properties.u5kManServer.Default.MySqlDumpVersion);
                    items.ForEach(item =>
                    {
                        LogInfo<U5kServiceMain>(item.What);
                        RecordEvent<U5kServiceMain>(item.When, item.IsError ? eIncidencias.IGRL_NBXMNG_EVENT : eIncidencias.IGRL_NBXMNG_ALARM,
                            eTiposInci.TEH_SISTEMA, "BKP", Params(item.What));
                    });
                }

                SetUpTimerBackup(alertTime);

            }, null, timeToGo, Timeout.InfiniteTimeSpan);
        }
#if DEBUG
        private System.Threading.Timer timerTest = null;
        private void SetupTimerTest(TimeSpan interval)
        {
            this.timerTest = new System.Threading.Timer(x =>
            {
                LogInfo<U5kServiceMain>(String.Format("TOTAL MEMORY  [{0:n}].....", (double)GC.GetTotalMemory(true)));
                SetupTimerTest(interval);
            }
            , null, interval, Timeout.InfiniteTimeSpan);
        }
#endif
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        bool CheckBdt()
        {
            Properties.u5kManServer cfg = Properties.u5kManServer.Default;

#if DEBUG
            // U5kiDbHelper.Backup(cfg.MySqlServer, cfg.BdtSchema, cfg.MySqlUser, cfg.MySqlPwd);
#endif
            /** */
            U5kBdtService.BdtSchema = cfg.BdtSchema;

            eBdt bdt_tipo = cfg.TipoBdt == 1 ? eBdt.bdtSqlite : eBdt.bdtMySql;
            string bdt_path = cfg.TipoBdt == 1 ? cfg.SQLitePath : cfg.MySqlServer;
            bool bdt_ok = U5kBdtService.CheckConnection(bdt_tipo, bdt_path, cfg.MySqlUser, cfg.MySqlPwd);
            if (bdt_ok)
            {
                if (U5kManService.Database == null)
                {
                    /** Abrir la Base de Datos.*/
                    if (cfg.TipoBdt == 1)
                        U5kManService.Database = new U5kBdtService(Thread.CurrentThread.CurrentUICulture, eBdt.bdtSqlite, cfg.SQLitePath);
                    else
                        U5kManService.Database = new U5kBdtService(Thread.CurrentThread.CurrentUICulture, eBdt.bdtMySql, cfg.MySqlServer, cfg.MySqlUser, cfg.MySqlPwd);
#if DEBUG
                    /** TODO. Activar la Configuracion Centralizada en RELEASE */
                    if (U5kManService.Database != null)
                        U5kManService.cfgSettings = new CfgSettings(U5kManService.Database);
#endif
                    LogInfo<U5kServiceMain>("CONECTADO A BASE DE DATOS");
                }
            }
            else
            {
                LogError<U5kServiceMain>(String.Format("ERROR EN LA SUPERVISION DE BASE DE DATOS [DB {0}, MASTER {1}]", U5kManService.Database, U5kManService._Master));

                if (U5kManService._Master)
                {
                    bool main_running = U5kManService._main.Running;
                    if (main_running)
                    {
                        U5kManService._main.Stop(TimeSpan.FromSeconds(20));
                        GlobalServices.GetWriteAccess((gdata) =>
                        {                        
                            // TODO. Inicializar _std con los valores convenientes.
                            U5KStdGeneral stdg = gdata.STDG;
                            stdg.cfgVersion = string.Empty;
                            stdg.cfgName = string.Empty;
                        });
                    }
                }
                if (U5kManService.Database != null)
                {
                    /** Cerrar la base de datos */
                    U5kManService.Database.dbClose();
                    U5kManService.Database = null;

                    /** Desactivo la configuracion local en BDT*/
                    if (U5kManService.cfgSettings.OnBdt)
                        U5kManService.cfgSettings = new CfgSettings();
                }
            }

            return bdt_ok;
        }
        /// <summary>
        /// 
        /// </summary>
        Task CheckingSactaServiceTask = null;
        void CheckSactaService(bool MustEnd = false/*Start, U5KStdGeneral stdg*/)
        {
#if !CheckingSactaServiceInline
            if (MustEnd)
            {
                ServicioInterfazSacta sacta_srv = new ServicioInterfazSacta(U5kManServer.Properties.u5kManServer.Default.MySqlServer);
                try
                {
                    sacta_srv.EndSacta();
                }
                catch (Exception x)
                {
                    LogException<U5kServiceMain>("Cerrando Servicio SACTA", x);
                }
            }
            else if (CheckingSactaServiceTask == null)
            {
                /** 20180706. Evita parar al sistema si el servicio WEB no funciona bien... */
                bool MustStart = false;
                GlobalServices.GetWriteAccess((data) =>
                {
                    U5KStdGeneral stdg = data.STDG;
                    MustStart = stdg.SactaServiceEnabled;
                });

                CheckingSactaServiceTask = Task.Run(() =>
                {
                    int sacta_status = 0;
                    ServicioInterfazSacta sacta_srv = new ServicioInterfazSacta(U5kManServer.Properties.u5kManServer.Default.MySqlServer);
                    Sleep(100);
                    try
                    {
                        sacta_status = sacta_srv.GetEstadoSacta();
                    }
                    catch (Exception x)
                    {
                        LogException<U5kServiceMain>("Obteniendo estado SACTA", x);
                    }
                    try
                    {
                        if (U5kManService.cfgSettings/*Properties.u5kManServer.Default*/.HaySacta == false || !bMaster)
                        {
                            if ((sacta_status & 0x00f0) != 0)
                                sacta_srv.EndSacta();
                            sacta_status = 0;
                        }
                        else
                        {
                            if ((sacta_status & 0x00f0) == 0 && MustStart)
                                sacta_srv.StartSacta();
                            else if ((sacta_status & 0x00f0) != 0 && !MustStart)
                                sacta_srv.EndSacta();
                        }
                    }
                    catch (Exception x)
                    {
                        LogException<U5kServiceMain>("Actualizando estado SACTA", x);
                    }

                    GlobalServices.GetWriteAccess((data) =>
                    {
                        U5KStdGeneral stdg = data.STDG;
                        U5kManService._main.EstadoSacta(sacta_status, stdg);
                    });

                    CheckingSactaServiceTask = null;
                });
            }
#else
            // 20170704. Chequea si no hay sacta configurado y lo para.
            try
            {
                ServicioInterfazSacta sacta_srv = new ServicioInterfazSacta(U5kManServer.Properties.u5kManServer.Default.MySqlServer);
                int sacta_status = sacta_srv.GetEstadoSacta();
                if (Properties.u5kManServer.Default.HaySacta == false || !bMaster)
                {
                    if ((sacta_status & 0x00f0) != 0)
                        sacta_srv.EndSacta();
                    if (stdg != null)
                        U5kManService._main.EstadoSacta(0, stdg);
                }
                else
                {
                    if ((sacta_status & 0x00f0) == 0 && MustStart)
                        sacta_srv.StartSacta();
                    else if ((sacta_status & 0x00f0) != 0 && !MustStart)
                        sacta_srv.EndSacta();

                    // OJO.. Pedir acceso a estado general.
                    if (stdg != null)
                        U5kManService._main.EstadoSacta(sacta_status, stdg);
                }
            }
            catch (Exception x)
            {
                LogException<U5kServiceMain>("", x);
            }
            finally
            {
            }
            return;
#endif
        }
        /// <summary>
        /// 
        /// </summary>
        void CheckLanesAndNtpSync(/*U5KStdGeneral stdg*/)
        {
            try
            {

                StdServ TestingServer = new StdServ();
                /** Chequear el TEAMING */
#if _NEM_V0_
                NICEventMonitor.NicEventMonitorConfig cfg = new NICEventMonitor.NicEventMonitorConfig(Properties.u5kManServer.Default.TeamingConfig);
                using (NICEventMonitor monitor = new NICEventMonitor(cfg))
                {
                    if (monitor.NICList.Count > 0)
                    {
                        // Hay Eventos de TEAMING. Asumo que la red es dual...
                        foreach (NICEventMonitor.NICItem item in monitor.NICList)
                        {
                            TestingServer.lanes[item.DeviceId] = item.Status == NICEventMonitor.LanStatus.Up ? std.Ok : std.Error;
                        }
                    }
                    else
                    {
                        // No hay eventos de Teaming. Asumo que la red no es dual...
                        /** Chequear el estado de la LAN que da servicio a la IP-FISICA del Servidor */
                        string MyIp = Properties.u5kManServer.Default.MiDireccionIP;
                        string eth_name = "";
                        bool eth_up = false;
                        if (Utilities.NICS.GetEthInterface(MyIp, ref eth_name, ref eth_up) == true)
                        {
                            TestingServer.lanes[eth_name] = eth_up ? std.Ok : std.Error;
                        }
                        else
                        {
                            LogError<U5kServiceMain>(
                                String.Format("No se encuentra Interfaz ETH para la ip {0}", MyIp));
                            return;
                        }
                    }
                }
#else
                using (NicEventMonitor monitor = new NicEventMonitor(Properties.u5kManServer.Default.TeamingConfig,null,null))
                {
                    if (monitor.GetState((id, status) =>
                    {
                        TestingServer.lanes[id] = status == NicEventMonitor.LanStatus.Up ? std.Ok : std.Error;
                    }) == false)
                    {
                        // No hay eventos de Teaming. Asumo que la red no es dual...
                        /** Chequear el estado de la LAN que da servicio a la IP-FISICA del Servidor */
                        string MyIp = Properties.u5kManServer.Default.MiDireccionIP;
                        string eth_name = "";
                        bool eth_up = false;
                        if (Utilities.NICS.GetEthInterface(MyIp, ref eth_name, ref eth_up) == true)
                        {
                            TestingServer.lanes[eth_name] = eth_up ? std.Ok : std.Error;
                        }
                        else
                        {
                            LogError<U5kServiceMain>(
                                String.Format("No se encuentra Interfaz ETH para la ip {0}", MyIp));
                            return;
                        }
                    }
                }
#endif
                /** Chequear el Estado de Sincronismo */
                using (NtpClientStatus ntpc = new NtpClientStatus(Properties.u5kManServer.Default.NtpClient))
                {
                    TestingServer.ntp_sync = String.Join("##", ntpc.Status.ToArray());
                }

                /** Actualizo los datos en la tabla... */
                GlobalServices.GetWriteAccess((data) =>
                {
                    U5KStdGeneral stdg = data.STDG;
                    StdServ MyStdServer = stdg.LocalServer;

                    if (MyStdServer == null)
                    {
                        LogError<U5kServiceMain>(
                            String.Format("No se determina si soy Servidor-1 o Servidor-2: {0} ?? ({1})-({2})",
                            System.Environment.MachineName, stdg.stdServ1.name, stdg.stdServ2.name));
                    }
                    else
                    {
                        /** Copio los datos obtenidos **/
                        MyStdServer.lanes.Clear();
                        foreach(var lan in TestingServer.lanes)
                        {
                            MyStdServer.lanes[lan.Key] = lan.Value;
                        }
                        MyStdServer.ntp_sync = TestingServer.ntp_sync;
                        /** Si soy esclavo, notifico los datos al master */
                        if (!bMaster)
                            WebAppServer.U5kManWebApp._sync_server.Sync(WebAppServer.cmdSync.InfoLanes, MyStdServer.lanes2string);

                        /** Si soy esclavo, notifico los datos al master */
                        if (!bMaster)
                            WebAppServer.U5kManWebApp._sync_server.Sync(WebAppServer.cmdSync.InfoNtpClient, MyStdServer.ntp_sync);
                    }
                });


            }
            catch (Exception x)
            {
                LogException<U5kServiceMain>("", x);
            }
            finally
            {
            }
        }
        /// <summary>
        /// 
        /// </summary>
        void CheckVersionDetails(U5KStdGeneral stdg)
        {
            try
            {
                // Solo se llama si es MASTER..
                if (U5kManService.cfgSettings/*Properties.u5kManServer.Default*/.ServidorDual == true)
                {
                    string MyName = System.Environment.MachineName;
                    if (MyName == stdg.stdServ1.name)
                    {
                        if (stdg.stdServ1.jversion == string.Empty)
                        {
                            // Calcular la Version.
                            stdg.stdServ1.jversion = (new VersionDetails("versiones.json")).ToString();
                        }
                        if (stdg.stdServ2.jversion == string.Empty)
                        {
                            // Pedir al SLAVE que comunique su version.
                            WebAppServer.U5kManWebApp._sync_server.Sync(WebAppServer.cmdSync.VersionsDetails, "");
                        }
                    }
                    else if (MyName == stdg.stdServ2.name)
                    {
                        if (stdg.stdServ2.jversion == string.Empty)
                        {
                            // Calcular la Version.
                            stdg.stdServ2.jversion = (new VersionDetails("versiones.json")).ToString();
                        }
                        if (stdg.stdServ1.jversion == string.Empty)
                        {
                            // Pedir al SLAVE que comunique su version.
                            WebAppServer.U5kManWebApp._sync_server.Sync(WebAppServer.cmdSync.VersionsDetails, "");
                        }
                    }
                    else
                    {
                        // Hay problemas con los nombres. Obtengo la version como si no fuera dual...
                        if (stdg.stdServ1.jversion == string.Empty)
                        {
                            // Calcular la Version...
                            stdg.stdServ1.jversion = (new VersionDetails("versiones.json")).ToString();
                        }
                    }
                }
                else
                {
                    if (stdg.stdServ1.jversion == string.Empty)
                    {
                        // Calcular la Version...
                        stdg.stdServ1.jversion = (new VersionDetails("versiones.json")).ToString();
                    }
                }
            }
            catch (Exception x)
            {
                LogException<U5kServiceMain>("", x);
            }
            finally
            {
            }
        }

        int minWorker, minIOC;
        void SetThreadPoolSize()
        {
            ThreadPool.GetMinThreads(out minWorker, out minIOC);
            if (ThreadPool.SetMinThreads(20, 20) == false)
            {
                //Console.WriteLine("Error en SetMinThreads... Pulse ENTER");
                //Console.ReadLine();
            }
        }
        void RestoreThreadPoolSize()
        {
            if (ThreadPool.SetMinThreads(minWorker, minIOC) == false)
            {
                //Console.WriteLine("Error en SetMinThreads... Pulse ENTER");
                //Console.ReadLine();
            }
        }

        /// <summary>
        /// 
        /// </summary>
        ServiceHost host = null;
        WebAppServer.U5kManWebApp webapp = null;
        static bool _started = false;
        bool bMaster = false;

        /// <summary>
        /// Gestion del Cluster ONLINE...
        /// </summary>
        OnLineCluster online_cluster = new OnLineCluster();
        public class OnLineCluster : BaseCode
        {
            class OnCLusterServerData : BaseCode
            {
                public string name { get; set; }
                public string ip { get; set; }
                public int port { get; set; }
                public int online_status { get; set; }
                public bool is_running
                {
                    get
                    {
                        return online_status == 2 || online_status == 3 ? true : false;
                    }
                }
                public bool IsLocal { get; set; }
                public int LocalUdpPort { get; set; }
                public int FailuresCounter { get; set; }
                public int FailuresLimit { get; set; }

                /// <summary>
                /// 
                /// </summary>
                /// <param name="serv"></param>
                public OnCLusterServerData(int serv)
                {
                    string WebConfigPath = "c:\\inetpub\\wwwroot\\NucleoDF\\u5kcfg\\web.config";

                    System.Configuration.ExeConfigurationFileMap configFile = new System.Configuration.ExeConfigurationFileMap() { ExeConfigFilename = WebConfigPath };
                    System.Configuration.Configuration config = System.Configuration.ConfigurationManager.OpenMappedExeConfiguration(configFile, System.Configuration.ConfigurationUserLevel.None);
                    System.Configuration.KeyValueConfigurationCollection settings = config.AppSettings.Settings;

                    if (settings.AllKeys.Contains("ClusterSrv1Ip") &&
                        settings.AllKeys.Contains("ClusterSrv2Ip") &&
                        settings.AllKeys.Contains("ClusterSrv1Port") &&
                        settings.AllKeys.Contains("ClusterSrv2Port"))
                    {
                        ip = serv == 0 ? settings["ClusterSrv1Ip"].Value : settings["ClusterSrv2Ip"].Value;
                        port = serv == 0 ? Int32.Parse(settings["ClusterSrv1Port"].Value) : Int32.Parse(settings["ClusterSrv2Port"].Value);
                    }
                    else
                    {
                        ip = "127.0.0.1";
                        port = 6666;
                    }
#if DEBUG
                    IsLocal = (serv == 1);
#else
                    IsLocal = U5kGenericos.IsLocalIpExt(ip);
#endif
                    online_status = 0;
                    LocalUdpPort = serv == 0 ? 9998 : 9999;
                    FailuresCounter = 0;
                    FailuresLimit = 3;
                    name = String.Empty;
                }

                /// <summary>
                /// 
                /// </summary>
                public void Ping()
                {
                    try
                    {
                        using (UdpClient udpClient = new UdpClient(LocalUdpPort))
                        {
                            IPEndPoint dst = new IPEndPoint(IPAddress.Parse(ip), port);
                            TimeSpan timeToWait = TimeSpan.FromMilliseconds(Properties.u5kManServer.Default.ClusterMethodUdpRequestTimeout);

                            // Envio el Comando....
                            MsgType msg = MsgType.GetState;
                            using (MemoryStream ms = new MemoryStream())
                            {
                                BinaryFormatter bf = new BinaryFormatter();
                                bf.Serialize(ms, msg);

                                byte[] bin_msg = ms.ToArray();
                                udpClient.SendAsync(bin_msg, bin_msg.Count(), dst);
                            }

                            // Recibir la Respuesta...
                            var asyncResult = udpClient.BeginReceive(null, null);
                            asyncResult.AsyncWaitHandle.WaitOne(timeToWait);
                            if (asyncResult.IsCompleted)
                            {
                                try
                                {
                                    IPEndPoint remoteEP = null;
                                    byte[] receivedData = udpClient.EndReceive(asyncResult, ref remoteEP);
                                    // EndReceive worked and we have received data and remote endpoint
                                    using (MemoryStream ms1 = new MemoryStream(receivedData))
                                    {
                                        ClusterState cluster_status = (new BinaryFormatter()).Deserialize(ms1) as ClusterState;

                                        name = cluster_status.LocalNode.Name;
                                        online_status = (int)cluster_status.LocalNode.State;
                                        FailuresCounter = 0;
                                    }
                                }
                                catch (Exception x)
                                {
                                    // EndReceive failed and we ended up here
                                    LogException<U5kServiceMain>("Recibiendo Data... ", x);
                                    SetError(String.Format(idiomas.strings.CLUSTER_ERROR_01/*"Nodo en ({0}:{1}) Error Obteniendo Info"*/, ip, port));
                                }
                            }
                            else
                            {
                                // The operation wasn't completed before the timeout and we're off the hook
                                SetError(String.Format(idiomas.strings.CLUSTER_ERROR_02/*"Nodo en ({0}:{1}) no Responde"*/, ip, port));
                            }
                        }
                    }
                    catch (Exception x)
                    {
                        LogException<U5kServiceMain>("Enviando data.... ", x);
                        SetError(String.Format(idiomas.strings.CLUSTER_ERROR_03/*"Nodo en ({0}:{1}) Error Solicitando Info"*/, ip, port));
                    }
                }

                private void SetError(string strError)
                {
                    if (name==String.Empty || (online_status != 0 && ++FailuresCounter >= FailuresLimit))
                    {
                        online_status = 0;
                        name = String.Format(idiomas.strings.CLUSTER_ERROR_03/*"Nodo en ({0}:{1}) Error Solicitando Info"*/, ip, port);
                        FailuresCounter = 0;
                    }

                }
            };

            public OnLineCluster()
            {
                servers.Add(new OnCLusterServerData(0) { FailuresLimit = 3 });  // TODO. Poner en configuracion.
                servers.Add(new OnCLusterServerData(1) { FailuresLimit = 3 });
            }

            /// <summary>
            /// 
            /// </summary>
            /// <returns></returns>
            public EstadoCluster Update()
            {
                servers[0].Ping();
                servers[1].Ping();

                EstadoCluster cluster = new EstadoCluster()
                {
                    EstadoNode1 = new EstadoNode() { Name = servers[0].name, Estado = servers[0].online_status, Presencia = servers[0].is_running },
                    EstadoNode2 = new EstadoNode() { Name = servers[1].name, Estado = servers[1].online_status, Presencia = servers[1].is_running }
                };

                return cluster;
            }


            List<OnCLusterServerData> servers = new List<OnCLusterServerData>();
        };
    }



#if DEBUG

    public class ClusterSim : BaseCode
    {

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public static EstadoCluster GetEstadoCluster(int Nodo1Nodo2)
        {
            LogInfo<ClusterSim>("GetEstadoCluster SIMULADO!!!");
            if (_except == true)
            {
                _except = false;
                throw new Exception("Excepcion (DEBUG) ClusterSim.GetEstadoCluster");
            }

            EstadoCluster cluster = new EstadoCluster();

            cluster.EstadoNode1 = new EstadoNode();
            cluster.EstadoNode1.Name = Nodo1Nodo2 == 0 ? System.Environment.MachineName : "OtroNodo";
            cluster.EstadoNode1.Estado = _std1;
            cluster.EstadoNode1.Presencia = _pres1;

            cluster.EstadoNode2 = new EstadoNode();
            cluster.EstadoNode2.Name = Nodo1Nodo2 == 0 ? "OtroNodo" : System.Environment.MachineName;
            cluster.EstadoNode2.Estado = _std2;
            cluster.EstadoNode2.Presencia = _pres2;

            return cluster;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="std1"></param>
        /// <param name="std2"></param>
        /// <param name="pres1"></param>
        /// <param name="pres2"></param>
        public static void SetEstadoCluster_(int std1, int std2, bool pres1, bool pres2)
        {
            _std1 = std1;
            _std2 = std2;
            _pres1 = pres1;
            _pres2 = pres2;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="Nodo"></param>
        /// <param name="std"></param>
        public static void CambiaEstadoNodoCluster(int Nodo)
        {
            if (Nodo == 0)
                _std1 = _std1 == 2 ? 3 : 2;
            else
                _std2 = _std2 == 2 ? 3 : 2;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="Nodo"></param>
        /// <param name="pres"></param>
        public static void CambiaPresenciaNodoCluster(int Nodo)
        {
            if (Nodo == 0)
                _pres1 = !_pres1;
            else
                _pres2 = !_pres2;
        }
        /// <summary>
        /// 
        /// </summary>
        public static void ConmutaConExcepcion(bool bExceptionService = false)
        {
            _std1 = _std1 == 2 ? 3 : 2;
            _std2 = _std2 == 2 ? 3 : 2;
            _except = true;
            if (bExceptionService == true && _std1 == 2)
                ExceptionSim.ActivateServiceException = true;
            if (bExceptionService == true && _std1 == 3)
                ExceptionSim.DeactivateServiceException = true;
        }

        /// <summary>
        /// 
        /// </summary>
        static int _std1 = 2, _std2 = 3;
        static bool _pres1 = true, _pres2 = true;
        static bool _except = false;
    }

    public class ExceptionSim
    {
        public static bool ActivateServiceException = false;
        public static bool DeactivateServiceException = false;

        public static void CheckExceptionActivateService()
        {
            if (ActivateServiceException == true)
            {
                ActivateServiceException = false;
                throw new Exception("ExceptionSim (DEBUG) ActivateServiceException");
            }
        }

        public static void CheckExceptionDeactivateService()
        {
            if (DeactivateServiceException == true)
            {
                DeactivateServiceException = false;
                throw new Exception("ExceptionSim (DEBUG) DeactivateServiceException");
            }
        }

    }

    public class FailingCluster : BaseCode
    {
    }

#endif
}
