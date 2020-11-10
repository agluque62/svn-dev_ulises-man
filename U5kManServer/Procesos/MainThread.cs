using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Globalization;

using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Win32;

using System.Media;
using System.Data;
using System.Timers;

using U5kBaseDatos;
using NucleoGeneric;
using Utilities;

namespace U5kManServer
{
    /// <summary>
    /// 
    /// </summary>
    public delegate void RefrescarTop(string ippos, stdPos pos);
    public delegate std ChangeStatusDelegate(std antiguo, std nuevo, int scv, eIncidencias inci, eTiposInci thw, string idhw, params object[] parametros);

    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    public delegate Object BdtServiceAcces();

    /// <summary>
    /// 
    /// </summary>
    public class MainThread : NucleoGeneric.NGThread
    {
        bool OneThreadInit = false;
        public override void DoWork()
        {
            try
            {
                if (OneThreadInit == false)
                {
                    HistThread.hproc = new HistThread();
                    _snmpagent = new U5kSnmpSystemAgent();
                    _gwExplorer = new GwExplorer(CambiaEstado);
                    _topExplorer = new TopSnmpExplorer(CambiaEstado);
                    _ext_sup = new ExtEquSpvSpace.ExtEquSpv();

                    MonitorOfServices = new Services.CentralServicesMonitor(() =>
                    {
                        NucleoGeneric.BaseCode.ConfigCultureSet();
                        return U5kManService._Master;
                    },
                        (alarma, str1, str2, str3) =>
                        {
                            eIncidencias inci = alarma == true ? eIncidencias.IGRL_NBXMNG_ALARM : eIncidencias.IGRL_NBXMNG_EVENT;
                            RecordEvent<Services.CentralServicesMonitor>(DateTime.Now, inci, eTiposInci.TEH_SISTEMA, "SPV",
                                new object[] { str1, " Server en " + str2, str3, "", "", "", "", "" });
                        },
                        (m, x) =>
                        {
                            if (x != null)
                                LogException<Services.CentralServicesMonitor>("CentralServiceMonitor", x);
                            else
                                LogDebug<Services.CentralServicesMonitor>(m);
                        },
                        (l, m) =>
                        {
                            LogTrace<Services.CentralServicesMonitor>(m);
                        },
                            Properties.u5kManServer.Default.nbxSupPort
                    );
                    MonitorOfServices.Start();

                    OneThreadInit = true;
                }
                /** Supervisa la configuracion */
                if (cfg_loaded == false)
                {
                    GlobalServices.GetWriteAccess((data) =>
                    {
                        cfg_loaded = LoadConfig(data);
                    });
                }

                new List<NGThread>()
                    {
                        HistThread.hproc,
                        _snmpagent,
                        _gwExplorer,
                        _topExplorer,
                        _ext_sup,
                    }.ForEach(p =>
                    {
                        try
                        {
                            p.DoWork();
                        }
                        catch (Exception x)
                        {
                            LogException<Action>("", x);
                        }
                    });

                new List<Action<U5kManStdData>>()
                    {
                        SupervisaScv,SupervisaPosiciones,/*SupervisaSacta,*/SupervisaNtp,SpAlarmas
                    }.ForEach(action =>
                    {
                        GlobalServices.GetWriteAccess((gdata) =>
                        {
                            try
                            {
                                action(gdata);
                            }
                            catch (Exception x)
                            {
                                LogException<Action>("", x);
                            }
                        });
                    });

            }
            catch (Exception x)
            {
                LogException<MainThread>("Supervisor Genérico", x);
            }
            LogInfo<MainThread>("MainThread DoWork!");
        }
        public override void LocalDispose()
        {
            MonitorOfServices.Dispose();
        }

        /// <summary>
        /// Los procesos hijos.
        /// </summary>
        GwExplorer _gwExplorer;
        TopSnmpExplorer _topExplorer;
        U5kSnmpSystemAgent _snmpagent;
#if _HAY_NODEBOX__
        NbxSpv _nbx_scan;
#else
        Services.CentralServicesMonitor MonitorOfServices;
#endif
        U5kManServer.ExtEquSpvSpace.ExtEquSpv _ext_sup = null;
        PabxItfService pabxService = null;

        /// <summary>
        /// Version de la Configuracion...
        /// </summary>
        string _strVersion = string.Empty;
        /// <summary>
        /// 
        /// </summary>
        public MainThread()
        {
            Name = "MainThread";
        }
        public override bool Start()
        {
            return base.Start();
        }
        public override void Stop(TimeSpan timeout)
        {
            /** Proteger el stop de incidencias */
            try
            {
                base.Stop(timeout);
            }
            catch (Exception x)
            {
                LogException<MainThread>("Stopping Mainthread Exception.", x);
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="inci"></param>
        public void GeneraIncidencia(int scv, eIncidencias inci, eTiposInci thw, string idhw, params object[] parametros)
        {
            RecordEvent<MainThread>(DateTime.Now, inci, thw, idhw, parametros);
        }
        /// 
        /// </summary>
        /// <param name="antiguo"></param>
        /// <param name="nuevo"></param>
        /// <param name="inci"></param>
        /// <param name="parametros"></param>
        /// <returns></returns>
        public std CambiaEstado(std antiguo, std nuevo, int scv, eIncidencias inci, eTiposInci thw, string idhw, params object[] parametros)
        {
            if (antiguo != nuevo)
            {
                /** 20170802. Cambiar el acceso al Historico */
                if (HistThread.hproc != null)
                {
                    RecordEvent<MainThread>(DateTime.Now, inci, thw, idhw, parametros);
                }
            }
            return nuevo;
        }
        /// <summary>
        /// Para borrar.
        /// </summary>
        protected void Init()
        {
            try
            {
                /** 20170802. Cambiar el acceso al Historico */
                HistThread.hproc = new HistThread();
                _snmpagent = new U5kSnmpSystemAgent();

                _gwExplorer = new GwExplorer(CambiaEstado);

                _topExplorer = new TopSnmpExplorer(CambiaEstado);

#if _HAY_NODEBOX__
                _nbx_scan = new NbxSpv();
#else
                MonitorOfServices = new Services.CentralServicesMonitor(() =>                
                    {
                        NucleoGeneric.BaseCode.ConfigCultureSet();
                        return U5kManService._Master;
                    },
                    (alarma, str1, str2, str3) =>                
                    {
                        eIncidencias inci = alarma == true ? eIncidencias.IGRL_NBXMNG_ALARM : eIncidencias.IGRL_NBXMNG_EVENT;
                        RecordEvent<Services.CentralServicesMonitor>(DateTime.Now, inci, eTiposInci.TEH_SISTEMA, "SPV",
                            new object[] { str1, " Server en " + str2, str3, "", "", "", "", "" });
                    },
                    (m, x) =>
                    {
                        if (x != null)
                            LogException<Services.CentralServicesMonitor>("CentralServiceMonitor", x);
                        else
                            LogDebug<Services.CentralServicesMonitor>(m);
                    },
                    (l, m) =>
                    {
                        LogTrace<Services.CentralServicesMonitor>(m);
                    },
                        Properties.u5kManServer.Default.nbxSupPort
                );
                MonitorOfServices.Start();
#endif
                _ext_sup = new ExtEquSpvSpace.ExtEquSpv();

                pabxService = new PabxItfService();
            }
            catch (Exception x)
            {
                LogException<MainThread>("En Procesos", x);
            }
        }
        /// <summary>
        /// 
        /// </summary>
        protected bool LoadConfig(U5kManStdData gdata)
        {
            bool retorno = true;
            try
            {

                if (U5kManService.Database != null)
                {
                    //U5kGenericos.SetCurrentCulture();

                    _strVersion = string.Empty;
                    U5KStdGeneral stdg = gdata.STDG;

                 
                    stdg.bDualScv = false;
                    stdg.bDualServ = U5kManService.cfgSettings/* Properties.u5kManServer.Default*/.ServidorDual;
                    stdg.HayReloj = U5kManService.cfgSettings/* Properties.u5kManServer.Default*/.HayReloj;
                    stdg.HaySacta = U5kManService.cfgSettings/* Properties.u5kManServer.Default*/.HaySacta;

                    stdg.HayPbx = U5kManService.PbxEndpoint != null;
                    stdg.stdPabx.name = String.Format("{0}:{1}",
                        U5kManService.PbxEndpoint == null ? "none" : U5kManService.PbxEndpoint.Address.ToString(),
                        Properties.u5kManServer.Default.PabxWsPort);

                    Dictionary<string, Action<U5kManStdData>> LoadTables = new Dictionary<string, Action<U5kManStdData>>()
                    {
                        {"LoadTops", LoadTops },
                        {"LoadGw", LoadGw },
                        {"LoadEquiposEurocae", LoadEquiposEurocae },
                        {"LoadDestinosPbx", LoadDestinosPabx },
                    };
                    foreach (var action in LoadTables)
                    {
                        try
                        {
                            action.Value(gdata);
                        }
                        catch (Exception x)
                        {
                            LogException<MainThread>("Cargando Tabla " + action.Key, x);
                            retorno = false;
                        }
                    }
                    if (retorno == true)
                    {
                        // 20170802. Carga de Usuarios....
                        gdata.usuarios = U5kManService.Database.SystemUsers();

                        /** 20171130. Supervisa el gestor Agente SNMP que debe resetearse con la configuracion */
                        //if (_snmpagent.IsRunning())
                        //{
                        //    _snmpagent.Stop();
                        //    //_snmpagent.Join(5000);
                        //}
                        //_snmpagent.Start();
                        /** 20180705. Las paradas y arranques de procesos daban problemas.... */
                        _snmpagent.ReloadRequest = true;
                        /*******************************************/
                    }
                }
                else
                {
                    retorno = false;        // No hay todavia base de datos...
                }
            }
            catch (Exception x)
            {
                LogException<MainThread>("En BDT", x);
                retorno = false;
            }
            return retorno;

        }
        /// <summary>
        /// 
        /// </summary>
        void LoadTops(U5kManStdData gdata)
        {
            string[] permitidos = null;

            if (Properties.u5kManServer.Default.FiltroPOS.Equals("None") == false)
                permitidos = Properties.u5kManServer.Default.FiltroPOS.Split(';');

            // Lista de TOPS
            List<BdtTop> tops = U5kManService.Database.GetListaTop(Properties.u5kManServer.Default.stringSistema);
            // Lista de USES en TOP
            List<Tuple<String, String>> usersOnTop = U5kManService.Database.GetUsersOnTop(Properties.u5kManServer.Default.stringSistema);

            List<stdPos> stdpos = new List<stdPos>();
            foreach (BdtTop top in tops)
            {
                if (permitidos == null || Array.Exists(permitidos, element => element == top.Id) == true)
                {
                    var uris = usersOnTop.Where(e => e.Item1 == top.Id)
                        .Select(e => String.Format("<sip:{0}@{1}:{2}>", e.Item2, top.Ip, 5060)).ToList();
                    stdpos.Add(new stdPos()
                    {
                        name = top.Id,
                        ip = top.Ip,
                        snmpport = Properties.u5kManServer.Default.TopSnmpPort,
                        uris = uris
                    });

                    /** */
                    U5kEstadisticaProc.Estadisticas.AddOperador(top.Id);
                }
            }

            gdata.CFGTOPS = stdpos;
        }
        /// <summary>
        /// 
        /// </summary>
        void LoadGw(U5kManStdData gdata)
        {
            string[] permitidos = null;
            if (Properties.u5kManServer.Default.FiltroGW.Equals("None") == false)
                permitidos = Properties.u5kManServer.Default.FiltroGW.Split(';');
            List<BdtGw> lgw = U5kManService.Database.GetListaGw(Properties.u5kManServer.Default.stringSistema);
            List<stdGw> stdgws = new List<stdGw>();
            foreach (BdtGw bgw in lgw)
            {
                if (permitidos == null || Array.Exists(permitidos, element => element == bgw.Id) == true)
                {
                    stdGw gw = new stdGw(null)
                    {
                        name = bgw.Id,
                        ip = bgw.Ip,
                        Dual = bgw.Dual
                    };
                    gw.gwA = new stdPhGw()
                    {
                        ip = bgw.Ip1,
                        snmpport = bgw.SnmpPortA,
                        name = bgw.Id + "-A",
                    };
                    gw.gwB = new stdPhGw()
                    {
                        ip = bgw.Ip2,
                        snmpport = bgw.SnmpPortB,
                        name = bgw.Id + "-B",
                    };

                    // Mapea los Recursos en las Pasarelas...
                    gw.gwA.RecInit();
                    gw.gwB.RecInit();

                    foreach (BdtGwRes res in bgw.Res)
                    {
                        if (res.Slot < 4 && res.Pos < 4)
                        {
                            /** Configuracion de Recursos */
                            gw.gwA.slots[res.Slot].std_cfg = gw.gwB.slots[res.Slot].std_cfg = std.Ok;
                            gw.gwA.slots[res.Slot].rec[res.Pos].name = gw.gwB.slots[res.Slot].rec[res.Pos].name = res.Id;
                            gw.gwA.slots[res.Slot].rec[res.Pos].bdt_name = gw.gwB.slots[res.Slot].rec[res.Pos].bdt_name = res.Id;
                            gw.gwA.slots[res.Slot].rec[res.Pos].tipo = gw.gwB.slots[res.Slot].rec[res.Pos].tipo = U5kManService.Bdt2Rct(res.Tpo, res.Itf);
                            gw.gwA.slots[res.Slot].rec[res.Pos].bdt = gw.gwB.slots[res.Slot].rec[res.Pos].bdt = true;
                            gw.gwA.slots[res.Slot].rec[res.Pos].Stpo = gw.gwB.slots[res.Slot].rec[res.Pos].Stpo = res.Stpo;
                            // 
                            gw.gwA.slots[res.Slot].rec[res.Pos].snmp_port = gw.gwB.slots[res.Slot].rec[res.Pos].snmp_port = 161 * 100 + (((res.Slot + 1) * 10) + res.Pos + 1); ;
                            gw.gwA.slots[res.Slot].rec[res.Pos].snmp_trap_port = gw.gwA.slots[res.Slot].rec[res.Pos].snmp_trap_port = 162 * 100 + (((res.Slot + 1) * 10) + res.Pos + 1); ;

                            /** Estado de Recursos */
                            gw.gwA.slots[res.Slot].rec[res.Pos].presente = false;
                            gw.gwB.slots[res.Slot].rec[res.Pos].presente = false;
                            gw.gwA.slots[res.Slot].rec[res.Pos].std_online = std.NoInfo;
                            gw.gwB.slots[res.Slot].rec[res.Pos].std_online = std.NoInfo;
                            gw.gwA.slots[res.Slot].rec[res.Pos].tipo_online = trc.rcNotipo;
                            gw.gwB.slots[res.Slot].rec[res.Pos].tipo_online = trc.rcNotipo;

                            /** Referencia a su IP Virtual */
                            gw.gwA.slots[res.Slot].rec[res.Pos].VirtualIp = gw.ip;
                            gw.gwB.slots[res.Slot].rec[res.Pos].VirtualIp = gw.ip;
                        }
                    }

                    stdgws.Add(gw);
                    /** */
                    U5kEstadisticaProc.Estadisticas.AddPasarela(bgw.Id);
                }
            }
            gdata.CFGGWS = stdgws;
        }
        /// <summary>
        /// 
        /// </summary>
        void LoadEquiposEurocae(U5kManStdData gdata)
        {
#if DEBUG1
            List<BdtEuEq> equipos = new List<BdtEuEq>()
            {
                new BdtEuEq(){ Id="RD-01",Ip="192.168.0.51", Ip2="192.168.1.51",  Tipo=2, IdDestino="", IdRecurso="RD-01", RxOrTx=4, Modelo=1000, SipPort=5060},
                new BdtEuEq(){ Id="RD-02",Ip="192.168.0.129", Ip2="192.168.0.129",  Tipo=2, IdDestino="", IdRecurso="RD-02", RxOrTx=5, Modelo=1001, SipPort=5060},
                new BdtEuEq(){ Id="RD-03",Ip="192.168.0.155", Ip2="192.168.0.155",  Tipo=2, IdDestino="", IdRecurso="RD-02", RxOrTx=5, Modelo=1001, SipPort=5060},
                new BdtEuEq(){ Id="BC_4", Ip="192.168.0.155",Ip2="192.168.0.155", Tipo=3, IdDestino="", IdRecurso="BC_4",  RxOrTx=0, Modelo=1000, SipPort=5060},
                new BdtEuEq(){ Id="BC-1", Ip="192.168.0.51",Ip2="192.168.0.51", Tipo=3, IdDestino="", IdRecurso="BC-1",  RxOrTx=0, Modelo=1000, SipPort=5060},
                new BdtEuEq(){ Id="LC_22",Ip="192.168.0.129",Ip2="192.168.0.129", Tipo=3, IdDestino="", IdRecurso="LC_22", RxOrTx=0, Modelo=1000, SipPort=5060},
                new BdtEuEq(){ Id="REC_A",Ip="192.168.0.129",Ip2="192.168.0.129", Tipo=5, IdDestino="", IdRecurso="REC_A", RxOrTx=0, Modelo=1000, SipPort=5060},
                new BdtEuEq(){ Id="REC_B",Ip="192.168.0.155",Ip2="192.168.0.155", Tipo=5, IdDestino="", IdRecurso="REC_B", RxOrTx=0, Modelo=1000, SipPort=5060},
            };
#else
            List<BdtEuEq> equipos = U5kManService.Database.ListaEquiposEurocae(Properties.u5kManServer.Default.stringSistema);
#endif

            /** Filtro de equipo valido */
            Func<BdtEuEq, bool> filtro = delegate(BdtEuEq equipo)
            {
                string PbxIp = U5kManService.PbxEndpoint == null ? "none" : U5kManService.PbxEndpoint.Address.ToString();

                /** Filtro el Equipo Correspondiente a la PABX */
                if (equipo.Ip.Contains(PbxIp) || equipo.Ip2.Contains(PbxIp))
                    return false;

                /** Filtro Equipos No-Asignados (Sin SIP)*/
                if (Properties.u5kManServer.Default.ExternalEquipmentUnassigned == false)
                {
                    if (equipo.Tipo != 5 && equipo.IdRecurso == null)
                        return false;
                }
                return true;
            };

            /** Configuro la lista de equipos */
            var lequipos = equipos.Where(equipo => filtro(equipo)).
                Select(equipo => new EquipoEurocae(null)
                    {
                        Id = equipo.Id,
                        Ip1 = equipo.Ip,
                        Ip2 = equipo.Ip2,
                        Tipo = equipo.Tipo,
                        Modelo = equipo.Modelo,
                        RxTx = equipo.RxOrTx,
                        fid = equipo.IdDestino,
                        sip_user = equipo.IdRecurso,
                        sip_port = equipo.SipPort
                    }).ToList();

            gdata.CFGEQS = lequipos;
            /** Activo la lista de equipos en la Estadisticas*/
            gdata.STDEQS.ForEach(equipo => U5kEstadisticaProc.Estadisticas.AddExternal(equipo.sip_user ?? equipo.Id, equipo.Tipo));
        }
        /// <summary>
        /// 
        /// </summary>
        void LoadDestinosPabx(U5kManStdData gdata)
        {
#if DEBUG1
            List<BdtPabxDest> destinos = new List<BdtPabxDest>()
            {
                new BdtPabxDest(){ Id="344000"},
                new BdtPabxDest(){ Id="344001"},
                new BdtPabxDest(){ Id="344002"},
                new BdtPabxDest(){ Id="344003"},
                new BdtPabxDest(){ Id="344004"},
                new BdtPabxDest(){ Id="344005"},
                new BdtPabxDest(){ Id="344006"},
                new BdtPabxDest(){ Id="344007"}
            };
#else
            string PbxIp = U5kManService.PbxEndpoint == null ? "none" : U5kManService.PbxEndpoint.Address.ToString();
            List<BdtPabxDest> destinos = U5kManService.Database.ListaDestinosPABX(PbxIp);
#endif

            gdata.CFGPBXS = destinos.Select(d => new Uv5kManDestinosPabx.DestinoPabx() { Id = d.Id }).ToList();
        }
        /// <summary>
        /// 
        /// </summary>
        void LoadDestinosAtsExternos(U5kManStdData gdata)
        {
            List<BdtEuEq> atsDestinations = U5kManService.Database.ListaDestinosAtsExternos("departamento");
            foreach (BdtEuEq atsDest in atsDestinations)
            {
                EquipoEurocae last = null;
                gdata.atsDestStd.Add(
                    new EquipoEurocae(last)
                    {
                        Id = atsDest.Id,
                        Ip1 = atsDest.Ip,
                        Ip2 = atsDest.Ip2,
                        Tipo = atsDest.Tipo,
                        Modelo = atsDest.Modelo,
                        RxTx = atsDest.RxOrTx,
                        fid = atsDest.IdDestino,
                        sip_user = atsDest.IdRecurso,
                        sip_port = atsDest.SipPort
                    }
                );
            }
        }
        /// <summary>
        /// 
        /// </summary>
        int spv_ntp_reint = 0;
        void SupervisaNtp(U5kManStdData gdata)
        {
            // Es una Action. en Version 1, viene con el Semaforo de Escritura cogido...
            if (U5kManService.cfgSettings/*vProperties.u5kManServer.Default*/.HayReloj == true)
            {
                U5KStdGeneral stdg = gdata.STDG;
                LogDebug<MainThread>("SupervisaNTP");

                try
                {
                    if (stdg.LocalServer != null)
                    {
                        String client_response = stdg.LocalServer.ntp_sync.Replace("##", "\r\n");
                        stdg.stdClock.name = (new Utilities.NtpClientStatus(Properties.u5kManServer.Default.NtpClient)).UrlServer(client_response);
                        if (U5kGenericos.ValidateIp(stdg.stdClock.name))
                        {
                            // Hago PING....
                            bool ntpPres = U5kGenericos.Ping(stdg.stdClock.name, stdg.stdClock.Estado == std.Ok);
                            switch (stdg.stdClock.Estado)
                            {
                                case std.Inicio:
                                case std.NoInfo:
                                    if (ntpPres == true)
                                    {
                                        GeneraIncidencia(0, eIncidencias.IGRL_NBXMNG_EVENT, eTiposInci.TEH_SISTEMA, "SPV",
                                            new object[] { idiomas.strings.NTP_ServerConnected/*"Servidor NTP Conectado"*/, stdg.stdClock.name, "", "", "", "", "", "" });
                                    }
                                    break;
                                case std.Ok:
                                    if (ntpPres == false)
                                    {
                                        GeneraIncidencia(0, eIncidencias.IGRL_NBXMNG_ALARM, eTiposInci.TEH_SISTEMA, "SPV",
                                            new object[] { idiomas.strings.NTP_NoServer/*"No Hay Servidor NTP en el Sistema"*/, stdg.stdClock.name, "", "", "", "", "", "" });
                                    }
                                    break;
                            }
                            spv_ntp_reint = 0;
                            stdg.stdClock.Estado = ntpPres == true ? std.Ok : std.NoInfo;
                        }
                        else
                        {
                            if (spv_ntp_reint++ > 3)
                            {
                                LogWarn<MainThread>(String.Format("NTP => NoInfo. IP/Name no Valido: {0}", stdg.stdClock.name));
                                spv_ntp_reint = 0;
                                stdg.stdClock.Estado = std.NoInfo;
                            }
                        }
                    }
                    else
                    {
                        if (spv_ntp_reint++ > 3)
                        {
                            LogDebug<MainThread>(String.Format("NTP => NoInfo. LocalServer == NULL"));
                            spv_ntp_reint = 0;
                            stdg.stdClock.name = "Unknown";
                            stdg.stdClock.Estado = std.NoInfo;
                        }
                    }
                }

                catch (Exception x)
                {
                    LogException<MainThread>("", x);
                    if (spv_ntp_reint++ > 3)
                    {
                        stdg.stdClock.Estado = std.NoInfo;
                        spv_ntp_reint = 0;
                    }
                }
            }
        }
        /// <summary>
        /// 
        /// </summary>
        void SupervisaScv(U5kManStdData gdata)
        {
            // Es una Action. en Version 1, viene con el Semaforo de Escritura cogido...

            U5KStdGeneral stdg = gdata.STDG;
            List<stdGw> stdgws = gdata.STDGWS;
            if (gdata != null && stdgws.Count > 0)
            {
                // Marcadores.
                int no_presentes = 0, en_alarma = 0;
                int ngw = 0;
                ngw = stdgws.Count;
                foreach (stdGw gw in stdgws)
                {
                    switch (gw.std)
                    {
                        case std.NoInfo:
                            no_presentes++;
                            break;
                        case std.Error:
                            en_alarma++;
                            break;
                    }
                }
                if (no_presentes == ngw)
                    stdg.stdScv1.Estado = std.NoInfo;
                else if (no_presentes > 0 && en_alarma == 0)
                    stdg.stdScv1.Estado = std.Aviso;
                else if (en_alarma > 0)
                    stdg.stdScv1.Estado = std.Alarma;
                else
                    stdg.stdScv1.Estado = std.Ok;

                stdg.stdScv1.Seleccionado = sel.Seleccionado;
            }
            else
            {
                stdg.stdScv1.Estado = std.NoInfo;
                stdg.stdScv1.Seleccionado = sel.NoSeleccionado;
            }

            stdg.stdScv2.Estado = std.NoInfo;
            stdg.stdScv2.Seleccionado = sel.NoSeleccionado;
            LogDebug<MainThread>("SupervisaSCV");
        }
        /// <summary>
        /// 
        /// </summary>
        void SupervisaPosiciones(U5kManStdData gdata)
        {
            // Es una Action. en Version 1, viene con el Semaforo de Escritura cogido...
            U5KStdGeneral stdg = gdata.STDG;
            List<stdPos> stdpos = gdata.STDTOPS;
            if (stdpos.Count > 0)
            {
                // Marcadores.
                int no_presentes = 0, en_alarma = 0;
                int npos = 0;
                npos = stdpos.Count;
                foreach (stdPos pos in stdpos)
                {
                    switch (pos/*.stdGlobal()*/.StdGlobal)
                    {
                        case std.NoInfo:
                            no_presentes++;
                            break;
                        case std.Error:
                            en_alarma++;
                            break;
                    }
                }

                if (no_presentes == npos)
                    stdg.stdGlobalPos = std.NoInfo;
                else if (no_presentes > 0 && en_alarma == 0)
                    stdg.stdGlobalPos = std.Aviso;
                else if (en_alarma > 0)
                    stdg.stdGlobalPos = std.Alarma;
                else
                    stdg.stdGlobalPos = std.Ok;
            }
            else
            {
                stdg.stdGlobalPos = std.NoInfo;
            }
            LogDebug<MainThread>("SupervisaPosiciones");
        }
        /// <summary>
        /// 20170704. Propiedad para la Supervision SACTA,
        /// </summary>
        public void EstadoSacta(int valor, U5KStdGeneral stdg)
        {
            // Debe ser llamada con el Semaforo de Escritura cogido...

            std stdSacta1 = (valor & 1) == 1 ? std.Ok : std.NoInfo;
            std stdSacta2 = (valor & 2) == 2 ? std.Ok : std.NoInfo;

            /** 20171023. Solo se genera la incidencia si SACTA está habilitado... */
            if (stdSacta1 != stdg.stdSacta1 && U5kManService.cfgSettings/*Properties.u5kManServer.Default*/.HaySacta == true)
            {
                GeneraIncidencia(0, stdSacta1 == std.Ok ? eIncidencias.IGRL_NBXMNG_EVENT : eIncidencias.IGRL_NBXMNG_ALARM,
                    eTiposInci.TEH_SISTEMA, "SPV",
                    new object[] { stdSacta1 == std.Ok ? idiomas.strings.SACTA_LAN1_ON/*"Red SACTA1 Conectada"*/ : idiomas.strings.SACTA_LAN1_OFF/*"Red SACTA1 Desconectada"*/, 
                                "", "", "", "", "", "", "" });
            }

            /** 20171023. Solo se genera la incidencia si SACTA está habilitado... */
            if (stdSacta2 != stdg.stdSacta2 && U5kManService.cfgSettings/*Properties.u5kManServer.Default*/.HaySacta == true)
            {
                GeneraIncidencia(0, stdSacta2 == std.Ok ? eIncidencias.IGRL_NBXMNG_EVENT : eIncidencias.IGRL_NBXMNG_ALARM,
                    eTiposInci.TEH_SISTEMA, "SPV",
                    new object[] { stdSacta2 == std.Ok ? idiomas.strings.SACTA_LAN2_ON/*"Red SACTA2 Conectada"*/ : idiomas.strings.SACTA_LAN2_OFF/*"Red SACTA2 Desconectada"*/, 
                                "", "", "", "", "", "", "" });
            }

            /** 20180726. Estado del Servicio */
            stdg.SactaService = (valor & 16) == 16 ? std.Ok : std.NoInfo;

            stdg.stdSacta1 = stdSacta1;
            stdg.stdSacta2 = stdSacta2;
        }
        /// <summary>
        /// 
        /// </summary>
        protected override void Run()
        {
            U5kGenericos.TraceCurrentThread(this.GetType().Name);

            //U5kGenericos.SetCurrentCulture();
            /** Lista de Tareas */
            List<Action<U5kManStdData>> actions = new List<Action<U5kManStdData>>()
            {
                SupervisaScv,SupervisaPosiciones,/*SupervisaSacta,*/SupervisaNtp,SpAlarmas
            };
            /** Crea los Procesos */
            Init();

            /** Lista de Procesos */
            List<NucleoGeneric.NGThread> process = new List<NGThread>()
            {
                HistThread.hproc,
                _snmpagent,
                _gwExplorer,
                _topExplorer,
#if _HAY_NODEBOX__
                _nbx_scan,
#endif
                _ext_sup,
                pabxService
            };

            LogInfo<MainThread>("Arrancado...");
            Decimal interval = Properties.u5kManServer.Default.SpvInterval;
            using (timer = new TaskTimer(new TimeSpan(0, 0, 0, 0, Decimal.ToInt32(interval)), this.Cancel))
            {
                while (IsRunning())
                {
                    try
                    {
                        /** Supervisa la configuracion */
                        if (cfg_loaded == false)
                        {
                            GlobalServices.GetWriteAccess((data) =>
                            {
                                cfg_loaded = LoadConfig(data);
                            });
                        }
                        else
                        {
                            /** Supervisa que los procesos esten arrancados */
                            process.ForEach(proc =>
                            {
                                try
                                {
                                    if (!proc.Running)
                                        proc.Start();
                                }
                                catch (Exception x)
                                {
                                    LogException<MainThread>(proc.Name, x);
                                }
                            });

                            /** Ejecutar las Acciones */
                            if (U5kManService._Master == true)
                            {
                                actions.ForEach(action =>
                                {
                                    Task.Factory.StartNew(() =>
                                    {
                                        GlobalServices.GetWriteAccess((gdata) =>
                                        {
                                            try
                                            {
                                                action(gdata);
                                            }
                                            catch (Exception x)
                                            {
                                                LogException<Action>("", x);
                                            }
                                        });

                                    }, TaskCreationOptions.LongRunning);
                                });
                            }

                        }
                    }
                    catch (Exception x)
                    {
                        LogException<MainThread>("Supervisor Genérico", x);
                        if (x is ThreadAbortException)
                        {
                            Thread.ResetAbort();
                            break;
                        }
                    }
                    GoToSleepInTimer();
                }
            }

            /** Detener los procesos... */
            //process.ForEach(proc =>
            //{
            //    try
            //    {
            //        if (proc.Running)
            //        {
            //            proc.Stop(TimeSpan.FromSeconds(5));
            //        }
            //    }
            //    catch (Exception x)
            //    {
            //        LogException<MainThread>(proc.Name, x);
            //    }
            //});

            List<Task> task2sync = new List<Task>();
            process.ForEach(p => {
                p.StopAsync((t) =>
                {
                    task2sync.Add(t);
                });
            });
            Task.WaitAll(task2sync.ToArray(), TimeSpan.FromSeconds(15));

#if !_HAY_NODEBOX__
            MonitorOfServices.Dispose();
#endif
            Dispose();
            LogInfo<MainThread>("Finalizado...");
        }

        bool cfg_loaded = false;
        public void InvalidateConfig()
        {
            cfg_loaded = false;
            /** 20180827. Para que se pueda volver a recargar las incidencias... */
            if (HistThread.hproc != null )
                HistThread.hproc.Reset();
        }

        /// <summary>
        /// 
        /// </summary>
        bool _playing = false;
        //        SoundPlayer _alarmas = null;
        //        void SpAlarmas_old()
        //        {
        //            try
        //            {
        //                if (Properties.u5kManServer.Default.SonidoAlarmas == true)
        //                {

        //                    if (_alarmas == null)
        //                        _alarmas = new SoundPlayer("ALARM.WAV");

        //                    if (U5kManService._last_inci._lista.Count != 0)
        //                    {
        //                        if (_playing == false)
        //                        {
        //                            _alarmas.PlayLooping();
        //                            _playing = true;
        //                        }
        //                    }
        //                    else
        //                    {
        //                        if (_playing == true)
        //                        {
        //                            _alarmas.Stop();
        //                            _playing = false;
        //                        }
        //                    }

        //                }
        //                else if (_playing == true)
        //                {
        //                    _alarmas.Stop();
        //                    _playing = false;
        //                }
        //                LogTrace<MainThread>(
        //                    String.Format("WavPlayer: Enable {0}, Incidencias {3}, Player {1}, Playing {2}.",
        //                    Properties.u5kManServer.Default.SonidoAlarmas, _alarmas, _playing, U5kManService._last_inci._lista.Count));
        //            }

        //            catch (Exception x)
        //            {
        //                _playing = false;
        //                LogException<MainThread>("", x);
        //            }
        //        }
        //        void SpAlarmas_new_v0()
        //        {
        //            try
        //            {
        //#if !DEBUG
        //                if (Properties.u5kManServer.Default.SonidoAlarmas == true)
        //#endif
        //                {
        //                    if (U5kManService._last_inci._lista.Count != 0)
        //                    {
        //                        if (_playing == false)
        //                        {
        //                            Task.Factory.StartNew(() =>
        //                            {
        //                                try
        //                                {
        //                                    _playing = true;
        //                                    WASAPI Player = new WASAPI("ALARM.WAV");
        //                                }
        //                                catch (Exception x)
        //                                {
        //                                    LogError<MainThread>(x.Message);
        //                                }
        //                                Thread.Sleep(8000);
        //                                _playing = false;
        //                            });
        //                        }
        //                    }
        //                }
        //                LogTrace<MainThread>(
        //                    String.Format("WavPlayer: Enable {0}, Incidencias {3}, Player {1}, Playing {2}.",
        //                    Properties.u5kManServer.Default.SonidoAlarmas, _alarmas, _playing, U5kManService._last_inci._lista.Count));
        //            }

        //            catch (Exception x)
        //            {
        //                _playing = false;
        //                LogException<MainThread>("", x);
        //            }
        //        }
        static bool spAlarmas = false;
        void SpAlarmas(U5kManStdData gdata)
        {
            if (spAlarmas == false)
            {
                spAlarmas = true;
                Task.Factory.StartNew(() =>
                {
                    using (Uvki5WavPlayer player = new Uvki5WavPlayer("ALARM.WAV"))
                    {
                        while (true) // TODO. Condicion de Salida.
                        {
                            try
                            {
                                if (U5kManService.cfgSettings/*Properties.u5kManServer.Default*/.SonidoAlarmas == true)
                                {
                                    if (U5kManService._last_inci._lista.Count != 0)
                                    {
                                        if (_playing == false)
                                        {
                                            player.Play();
                                            _playing = true;
                                        }
                                    }
                                    else
                                    {
                                        if (_playing == true)
                                        {
                                            player.Stop();
                                            _playing = false;
                                        }
                                    }
                                }
                                else if (_playing == true)
                                {
                                    player.Stop();
                                    _playing = false;
                                }
                                LogTrace<MainThread>(
                                    String.Format("WavPlayer: Enable: {0}, Incidencias: {3}, Player: {1}, Playing: {2}.",
                                    U5kManService.cfgSettings/*Properties.u5kManServer.Default*/.SonidoAlarmas, player, _playing, U5kManService._last_inci._lista.Count));
                            }

                            catch (Exception x)
                            {
                                _playing = false;
                                LogException<MainThread>("", x);
                            }
                            Thread.Sleep(5000);
                        }
                    }
                });
            }
        }
    }
}
