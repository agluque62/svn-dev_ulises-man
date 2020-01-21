using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Net;

using Utilities;
using NucleoGeneric;

namespace U5kManServer.WebAppServer
{
    enum cmdSync
    {
        Incidencias,
        Opciones,
        InfoLanes,
        InfoNtpClient,
        OpcionesSnmp,
        OpcionesSacta,
        VersionsDetails,
        GlobalStatus
    }
    class MainStandbySyncServer : BaseCode
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="srcIp"></param>
        /// <param name="grpIp"></param>
        /// <param name="Port"></param>
        public MainStandbySyncServer()
        {
        }

        /// <summary>
        /// 
        /// </summary>
        public string Name
        {
            get { return "MainStandbySyncServer"; }
        }
        /// <summary>
        /// 
        /// </summary>
        public ServiceStatus Status
        {
            get { return _status; }
        }
        /// <summary>
        /// 
        /// </summary>
        public void Start(String srcIp, String grpIp, int Port)
        {
            try
            {
                _ip_propia = IPAddress.Parse(srcIp);
                _ip_grp = new IPEndPoint(IPAddress.Parse(grpIp), Port);

                _listener = new UdpSocket(srcIp, Port/* Port*/);
                _listener.MaxReceiveThreads = 1;
                _listener.NewDataEvent += OnNewData;
                _listener.Base.JoinMulticastGroup(IPAddress.Parse(grpIp), _ip_propia);
                _listener.BeginReceive();
            }
            catch (Exception x)
            {
                LogException<MainStandbySyncServer>("", x);
            }
        }
        /// <summary>
        /// 
        /// </summary>
        public void Stop()
        {
            try
            {
                if (_listener != null)
                {
                    _listener.Dispose();
                    _listener = null;
                }
            }
            catch (Exception x)
            {
                LogException<MainStandbySyncServer>("", x);
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="data"></param>
        public void Sync(cmdSync cmd, String data)
        {
            try
            {
                byte[] bData = (new byte[] { (byte)cmd }).Concat(Encoding.ASCII.GetBytes(data)).ToArray();
                _listener.Send(_ip_grp, bData);
            }
            catch (Exception x)
            {
                LogException<MainStandbySyncServer>("", x);
            }

            /** Para que salga la trama */
            System.Threading.Thread.Sleep(50);
        }

        public void QueryVersionData()
        {
            versync.Reset();
            Sync(cmdSync.VersionsDetails, "");
            versync.WaitOne(2500);
        }

        public void SignalVersionDataReceived()
        {
            versync.Set();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="dg"></param>
        private void OnNewData(object sender, DataGram dg)
        {
            U5kGenericos.TraceCurrentThread(this.GetType().Name + " OnNewData");
            try
            {
#if !DEBUG1
                if (_ip_propia.ToString() != dg.Client.Address.ToString())
#endif
                {
                    cmdSync cmd = (cmdSync)dg.Data[0];
                    string strData = System.Text.Encoding.Default.GetString(dg.Data, 1, dg.Data.Count() - 1);

                    switch (cmd)
                    {
                        case cmdSync.Incidencias:
                            LogDebug<MainStandbySyncServer>(
                                "Recibido cmdSync.Incidencias");
                            U5kManWADDbInci incis = U5kManWebAppData.JDeserialize<U5kManWADDbInci>(strData);
                            incis.Save();
                            break;
                        case cmdSync.Opciones:
                            LogDebug<MainStandbySyncServer>(
                                "Recibido cmdSync.Opciones");
                            U5kManWADOptions opts = U5kManWebAppData.JDeserialize<U5kManWADOptions>(strData);
                            opts.Save();
                            break;
                        case cmdSync.OpcionesSnmp:
                            LogDebug<MainStandbySyncServer>("Recibido cmdSync.Opciones-SNMP");
                            U5kManWADSnmpOptions snmpopts = U5kManWebAppData.JDeserialize<U5kManWADSnmpOptions>(strData);
                            snmpopts.Save();
                            break;

                        case cmdSync.OpcionesSacta:
                            LogDebug<MainStandbySyncServer>("Recibido cmdSync.Opciones-SNMP");
                            ServicioInterfazSacta sacta_srv = new ServicioInterfazSacta(U5kManServer.Properties.u5kManServer.Default.MiDireccionIP);
                            sacta_srv.SactaConfSet(strData);
                            break;

                        case cmdSync.InfoLanes:
                            {
                                GlobalServices.GetWriteAccess((data) =>
                                {
                                    StdServ srv = data.STDG.RemoteServer;
                                    if (srv != null)
                                    {
                                        srv.string2lanes = strData;
                                    }
                                });
                            }
                            break;

                        case cmdSync.InfoNtpClient:
                            {
                                GlobalServices.GetWriteAccess((data) =>
                                {
                                    StdServ srv = data.STDG.RemoteServer;
                                    if (srv != null)
                                    {
                                        srv.ntp_sync = strData;
                                    }
                                });
                                break;
                            }

                        case cmdSync.VersionsDetails:
                            GlobalServices.GetWriteAccess((data) =>
                            {
                                if (U5kManService._Master)
                                {
                                        // Si le llega al MASTER debe obtener de los datos la version de SLAVE.
                                        StdServ srv = data.STDG.RemoteServer;
                                    if (srv != null)
                                    {
                                        srv.jversion = strData;
                                        SignalVersionDataReceived();
                                        LogTrace<MainStandbySyncServer>("Informacion de Versiones Recibida.");
                                    }
                                }
                                else
                                {
                                        // Si le llega al SLAVE debe generar los datos para el MASTER.
                                        Sync(cmdSync.VersionsDetails, (new VersionDetails("versiones.json")).ToString());
                                    LogTrace<MainStandbySyncServer>("Informacion de Versiones Enviada.");
                                }
                            });

                            break;

                        case cmdSync.GlobalStatus:
                            //#if DEBUG
                            //                            U5kManServer.DebugHelper.checkEquals = true;
                            //                            Console.WriteLine("Recibido GlobalStatus {0} bytes. Igual Anterior: {1}",
                            //                                strData.Count(), U5kManService._std.Equals(U5kGenericos.JDeserialize<U5kManServer.U5kManStdData>(strData)));
                            //                            U5kManServer.DebugHelper.checkEquals = false;
                            //#endif
                            //                            /** El Esclavo actualiza su estado global con lo que le dice el MASTER */
                            //                            if (U5kManService._Master == false)
                            //                            {
                            //                                //U5kGenericos.DeserializaContractObject<U5kManServer.U5kManStdData>(strData, ref U5kManService._std);
                            //                                U5kManService._std = U5kGenericos.JDeserialize<U5kManServer.U5kManStdData>(strData);
                            //                            }
                            break;
                    }
                }
#if DEBUG
                    else
                    {
                        LogDebug<MainStandbySyncServer>(
                            String.Format("Recibido Mensaje PROPIO: {0}", (cmdSync)dg.Data[0]));
                    }
#endif
            }
            catch (Exception x)
            {
                LogException<MainStandbySyncServer>("", x);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        private ServiceStatus _status = ServiceStatus.Disabled;
        private UdpSocket _listener = null;
        private IPAddress _ip_propia = null;
        private IPEndPoint _ip_grp = null;

        /** */
        private static ManualResetEvent versync = new ManualResetEvent(false);

    }
}
