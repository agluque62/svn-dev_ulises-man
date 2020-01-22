using System;
using System.Collections.Generic;
using System.Linq;

using System.Net;

using Lextm.SharpSnmpLib;

using U5kBaseDatos;
using Utilities;

#if _ED137_REVB_
#else
using U5kManMibRevC;
#endif

namespace U5kManServer
{
    /// <summary>
    /// 
    /// </summary>
    delegate void Supervisor();
    /// <summary>
    /// 
    /// </summary>
    class U5kSnmpSystemAgent : NucleoGeneric.NGThread
    {
        enum SystemAgentState { NotInit, NotStarted, Operative }
        string ipServ = Properties.u5kManServer.Default.MiDireccionIP;  //  U5kGenericos.MiDireccionIP;
#if _ED137_REVB_
        U5kScvMib _mib;
#else
        Ed137RevCMib _mib;
#endif
#if _PASARELAS_NO_UNIFICADAS_
        /// <summary>
        /// Para gestionar los eventos de las Pasarelas...
        /// </summary>
        EventosRadio _evradio = new EventosRadio();
        EventosLC _evLcen = new EventosLC();
        EventosTLF _evTlf = new EventosTLF();
        EventosATS _evAts = new EventosATS();
#endif
        SnmpAgent snmpAgent = new SnmpAgent();

        SystemAgentState State { get; set; }
        public bool ReloadRequest { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public bool SnmpAgentStartForTestUnit()
        {
            try
            {
                U5kManService._Master = true;

                SnmpAgentInit();
                snmpAgent.Start();
                LogInfo<U5kSnmpSystemAgent>("Agente SNMP. Arrancado");
                return true;
            }
            catch (Exception x)
            {
                LogException<U5kSnmpSystemAgent>("", x);
                return false;
            }
        }
        public void SnmpAgentStopForTestUnit()
        {
            try
            {
                snmpAgent.Close();
                U5kManService._Master = false;
                LogInfo<U5kSnmpSystemAgent>("Agente SNMP. Detenido...");
            }
            catch (Exception x)
            {
                LogException<U5kSnmpSystemAgent>("", x);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public U5kSnmpSystemAgent()
        {
            Name = "U5kSnmpSystemAgent";
            State = SystemAgentState.NotInit;
            ReloadRequest = false;
        }
        /// <summary>
        /// 
        /// </summary>
        protected override void Run()
        {
            U5kGenericos.TraceCurrentThread(this.GetType().Name);
            List<Action> functions = new List<Action>()
            {
                SupervisarPuestos,
                Supervisa_sysUpTime,
                SupervisarLineas,
                SupervisarParametrosGenerales,
                Supervisa_tablas_v2
            };

            Decimal interval = Properties.u5kManServer.Default.SpvInterval;
            //U5kGenericos.SetCurrentCulture();

            LogInfo<U5kSnmpSystemAgent>("U5kSnmpSystemAgent, running...");
            using (timer = new TaskTimer(new TimeSpan(0, 0, 0, 0, Decimal.ToInt32(interval)), this.Cancel))
            {
                while (IsRunning())
                {
                    switch (State)
                    {
                        case SystemAgentState.NotInit:
                            if (SnmpAgentInit())
                            {
                                State = SystemAgentState.NotStarted;
                            }
                            break;

                        case SystemAgentState.NotStarted:
                            if (SnmpAgentStart())
                            {
                                State = SystemAgentState.Operative;
                                ReloadRequest = false;
                            }
                            else
                            {
                                SnmpAgentStop();
                                State = SystemAgentState.NotInit;
                            }
                            break;

                        case SystemAgentState.Operative:

                            if (ReloadRequest == true)
                            {
                                SnmpAgentStop();
                                State = SystemAgentState.NotInit;
                                ReloadRequest = false;
                            }
                            else
                            {
#if _ED137_REVB_
                                functions.ForEach(f =>
                                {
                                    try
                                    {
                                        GlobalServices.GetWriteAccess((data) =>
                                        {
                                            if (U5kManService._Master == true)
                                                f();
                                        });
                                    }
                                    catch (Exception x)
                                    {
                                        LogException<U5kSnmpSystemAgent>("", x);
                                    }
                                });
#else
                                // Por si hay algo que supervisar...
#endif
                            }
                            break;
                    }
                    GoToSleepInTimer();
                }
            }
            SnmpAgentStop();
            Dispose();
            LogInfo<U5kSnmpSystemAgent>("U5kSnmpSystemAgent stopped...");
        }
        /// <summary>
        /// Carga la base de OID's del Agente.
        /// </summary>
        bool SnmpAgentInit()
        {
            try
            {
#if _ED137_REVB_
                _mib = new U5kScvMib();
#endif
                snmpAgent.Init(ipServ);            // Poner la IP del Servidor....
                snmpAgent.TrapReceived += new Action<string, string, ISnmpData, IPEndPoint, IPEndPoint>(RecibidoTrap);

                LogInfo<U5kSnmpSystemAgent>("Agente SNMP. configurado");
                return true;
            }
            catch (Exception x)
            {
                LogException<U5kSnmpSystemAgent>("", x);
                SnmpAgentStop();
                return false;
            }
        }
        /// <summary>
        /// 
        /// </summary>
        bool SnmpAgentStart()
        {
            try
            {
#if _ED137_REVB_
                _mib.Load();
#else
                _mib = new Ed137RevCMib(
                    new AgentDataGet((toma) =>
                    {
                        toma(rMONData);
                    }),
                    new AgentDataGet(
                    (toma) =>
                    {
                        GlobalServices.GetWriteAccess((data) =>
                        {
                            toma(data);
                        });
                    }), AgentData.OidBase);
                _mib.StoreTo(SnmpAgent.Store);
#endif
                snmpAgent.Start();
                LogInfo<U5kSnmpSystemAgent>("Agente SNMP. Arrancado");
                return true;
            }
            catch (Exception x)
            {
                LogException<U5kSnmpSystemAgent>("", x);
                return false;
            }
        }
        /// <summary>
        /// 
        /// </summary>
        void SnmpAgentStop()
        {
            try
            {
                snmpAgent.Close();
                _mib.Dispose();
                _mib = null;
                LogInfo<U5kSnmpSystemAgent>("Agente SNMP. Detenido...");
            }
            catch (Exception x)
            {
                LogException<U5kSnmpSystemAgent>("", x);
            }
        }
        /// <summary>
        /// 
        /// </summary>        
        void SupervisarParametrosGenerales()
        {
            /** Supervisa la Configurcion */
#if _ED137_REVB_
            _mib.SupervisaCfgSettings();
#else
#endif
        }
        /// <summary>
        /// 
        /// </summary>
        void Supervisa_sysUpTime()
        {
#if _ED137_REVB_
            _mib.sysUpTime();
#else
#endif
        }

        /// <summary>
        /// 
        /// </summary>
        void Supervisa_tablas_v2()
        {
#if _ED137_REVB_
            _mib.sysUpTime();
            _mib.MttoV2Tick();
#else
#endif
        }
        /// <summary>
        /// 
        /// </summary>
        void Supervisa_Mib2()
        {
        }
        /// <summary>
        /// 
        /// </summary>
        void SupervisarPuestos()
        {
#if _ED137_REVB_
            List<stdPos> Posiciones = U5kManService._std.STDTOPS.OrderBy(e => e.name).ToList();
            for (int npos = 0; npos < Posiciones.Count; npos++)
            {
                stdPos pos = Posiciones[npos];
                _mib.itfPosicion(npos, pos.stdpos, pos.name);
            }
#else
#endif
        }
        /// <summary>
        /// 
        /// </summary>
        void SupervisarLineas()
        {
            /** */
#if _ED137_REVB_
            mib2OperStatus globalStatusTlf = mib2OperStatus.down;
            mib2OperStatus globalStatusRad = mib2OperStatus.down;
            mib2OperStatus globalStatusRec = mib2OperStatus.down;

            /** Estados de Interfaces LEGACY */
            List<stdGw> stdgws = U5kManService._std.STDGWS;
            foreach (stdGw gw in stdgws)
            {
                foreach (stdSlot slot in gw.gwA.slots)
                {
                    if (slot != null)
                    {
                        foreach (stdRec rc in slot.rec)
                        {
                            if (rc != null)
                            {
                                mib2OperStatus itemStatus = MibHelper.std2mib2OperStatus(rc.std_online);
                                if (rc.tipo == itf.rcRadio) // TODO. repensar los estado..
                                {
                                    globalStatusTlf = itemStatus == mib2OperStatus.up ? itemStatus : globalStatusTlf;
                                }
                                else if (rc.tipo != itf.rcNotipo)
                                {
                                    globalStatusRad = itemStatus == mib2OperStatus.up ? itemStatus : globalStatusRad;
                                }
                            }
                        }
                    }
                }
            }

            /** Estados de Interfaces IP */
            List<U5kManStdEquiposEurocae.EquipoEurocae> stdeqeu = U5kManService._std.STDEQS;
            foreach (U5kManStdEquiposEurocae.EquipoEurocae equipo in stdeqeu)
            {
                mib2OperStatus itemStatus = MibHelper.std2mib2OperStatus(equipo.EstadoGeneral);
                if (equipo.Tipo == 3 )
                    globalStatusTlf = itemStatus == mib2OperStatus.up ? itemStatus : globalStatusTlf;
                else if (equipo.Tipo == 2)
                    globalStatusRad = itemStatus == mib2OperStatus.up ? itemStatus : globalStatusRad;
                else if (equipo.Tipo == 5)
                    globalStatusRec = itemStatus == mib2OperStatus.up ? itemStatus : globalStatusRec;
            }

            _mib.itfTelefonia = globalStatusTlf;
            _mib.itfRadio = globalStatusRad;
            _mib.itfRecorder = globalStatusRec;
#else
#endif
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="variable"></param>
        /// <param name="data"></param>
        /// <param name="ipfrom"></param>
        void RecibidoTrap(string oidtrap, string oidvar, ISnmpData data, IPEndPoint ipfrom, IPEndPoint ipto)
        {
            LogTrace<U5kSnmpSystemAgent>($"Trap from {ipto.Address.ToString()}, Oid: {oidtrap}, Var: {oidvar}");
            /** Ajusto el oidtrap / oidvar para que comienze por <.>*/
            oidvar = oidvar.StartsWith(".") ? oidvar : "." + oidvar;
            oidtrap = oidtrap.StartsWith(".") ? oidtrap : "." + oidtrap;
            GlobalServices.GetWriteAccess((gdata) =>
            {
                try
                {
                    if (U5kManService._Master == true)
                    {
                        // Busco si es una posicion.
                        List<stdPos> stdpos = gdata?.STDTOPS;
                        stdPos pos = stdpos?.Find(r => r.ip == ipfrom.Address.ToString());

                        // Busco si es una Pasarela.
                        List<stdGw> stdgws = gdata?.STDGWS;
                        stdGw gw = stdgws?.Find(r => r.ip == ipfrom.Address.ToString() || r.gwA.ip == ipfrom.Address.ToString() || r.gwB.ip == ipfrom.Address.ToString());
                        
                        if (oidvar.StartsWith(Properties.u5kManServer.Default.HfEventOids) ||
                            oidvar.StartsWith(Properties.u5kManServer.Default.CfgEventOid))
                        {
                            RecibidoEventoExterno(oidvar, ((OctetString)data).ToString());
                        }
                        else
                        {
                            if (gw != null)
                            {
                                stdPhGw pgw = gw.gwA.ip == ipfrom.Address.ToString() ? gw.gwA : gw.gwB;
#if _PASARELAS_NO_UNIFICADAS_
                                var BdtItem = GwExplorer._GwOids.Where(p => p.Value.EndsWith(oidvar)).ToList();
                                if (BdtItem.Count > 0)
                                {
                                    eGwPar par = BdtItem[0].Key;
                                    /** Pasarelas no Unificadas */
                                    if (par != eGwPar.None)
                                    {
                                        LogWarn<U5kSnmpSystemAgent>(String.Format("GW-ANT OID [{0}] No encontrado para {1}", oidvar, ipfrom));
                                    }
                                    else if (EventosRecursos.IsEventRadio(oidvar) == true)
                                    {
                                        if (_evradio.AutomataEventos(pgw, ipto.Port, oidvar, data) == true)
                                        {
                                            RecordEvent<U5kSnmpSystemAgent>(DateTime.Now, (eIncidencias)_evradio.lastInci, eTiposInci.TEH_TIFX, gw.name, _evradio.lastParameters);
                                        }
                                    }
                                    else if (EventosRecursos.IsEventLcen(oidvar) == true)
                                    {
                                        if (_evLcen.AutomataEventos(pgw, ipto.Port, oidvar, data) == true)
                                        {
                                            RecordEvent<U5kSnmpSystemAgent>(DateTime.Now, (eIncidencias)_evLcen.lastInci, eTiposInci.TEH_TIFX, gw.name, _evLcen.lastParameters);
                                        }
                                    }
                                    else if (EventosRecursos.IsEventTlf(oidvar) == true)
                                    {
                                        if (_evTlf.AutomataEventos(pgw, ipto.Port, oidvar, data) == true)
                                        {
                                            RecordEvent<U5kSnmpSystemAgent>(DateTime.Now, (eIncidencias)_evTlf.lastInci, eTiposInci.TEH_TIFX, gw.name, _evTlf.lastParameters);
                                        }
                                    }
                                    else if (EventosRecursos.IsEventAts(oidvar) == true)
                                    {
                                        if (_evAts.AutomataEventos(pgw, ipto.Port, oidvar, data) == true)
                                        {
                                            RecordEvent<U5kSnmpSystemAgent>(DateTime.Now, (eIncidencias)_evAts.lastInci, eTiposInci.TEH_TIFX, gw.name, _evAts.lastParameters);
                                        }
                                    }
                                    else if (oidtrap.Contains(".1.3.6.1.4.1.7916.8.3.2.1") == true)
                                    {
                                        /** Pasarelas Unificadas */
                                        GwExplorer.RecibidoTrapGw_unificada(gw, pgw, oidtrap, oidvar, data);
                                    }
                                    else
                                    {
                                        LogWarn<U5kSnmpSystemAgent>(String.Format("Recibido TrapGW {0}:{1} oid={2}. Comando No esperado", ipfrom.Address.ToString(), ipto.Port, oidvar));
                                    }
                                }
                                else
                                {
                                    LogError<U5kSnmpSystemAgent>(String.Format("GW OID [{0}] No encontrado para {1}", oidvar, ipfrom));
                                }
#else
                                /** Pasarelas Unificadas */
                                if (oidtrap.Contains(".1.3.6.1.4.1.7916.8.3.2.1") == true)
                                {
                                    /** Pasarelas Unificadas */
                                    GwExplorer.RecibidoTrapGw_unificada(gw, pgw, oidtrap, oidvar, data);
                            }
                                else
                                {
                                    LogWarn<U5kSnmpSystemAgent>(String.Format("GW OID [{0}] No encontrado para {1}", oidvar, ipfrom));
                                }
#endif
                            }
                            else if (pos != null /*&& pos.stdpos != std.NoInfo*/)
                            {
                                // Proviene de un Puesto.
                                var BdtItem = TopSnmpExplorer._OidPos.Where(p => p.Value.EndsWith(oidvar)).ToList();
                                if (BdtItem.Count > 0)
                                {
                                    RecibidoTrapPosicion(pos, BdtItem[0].Key, data);
                                }
                                else
                                {
                                    LogWarn<U5kSnmpSystemAgent>(String.Format("TOP OID [{0}] No encontrado para {1}", oidvar, ipfrom));
                                }
                            }
                            else
                            {
#if DEBUG
                                if (oidtrap.Contains(".1.3.6.1.4.1.7916.8.3.2.1") == true)
                                {
                                    /** Pasarelas Unificadas */
                                    GwExplorer.RecibidoTrapGw_unificada(null, null, oidtrap, oidvar, data);
                                }
#endif
                                LogWarn<U5kSnmpSystemAgent>(String.Format("OID [{0}] No encontrado para {1}", oidvar, ipfrom));
                            }
                        }
                    }
                }
                catch (Exception x)
                {
                    LogException<U5kSnmpSystemAgent>("", x);
                }
            });
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pos"></param>
        /// <param name="parametro"></param>
        /// <param name="valor"></param>
        void RecibidoTrapPosicion(stdPos pos, eTopPar parametro, ISnmpData data)
        {
            LogTrace<U5kSnmpSystemAgent>(String.Format("Trap POS {0}, {1}", pos.ip, parametro));
            switch (parametro)
            {
                case eTopPar.Top:
                    TopSnmpExplorer.EstadoPosicionSet(pos.name, pos, ((Integer32)data).ToInt32() == 0 ? std.NoInfo : std.Ok);
                    break;

                case eTopPar.EstadoPanel:
                    TopSnmpExplorer.PosicionPanelSet(pos.name, pos, ((Integer32)data).ToInt32() == 0 ? std.NoInfo : std.Ok);
                    break;

                case eTopPar.EstadoJacksEjecutivo:
                    TopSnmpExplorer.PosicionEjecutivoSet(pos.name, pos, ((Integer32)data).ToInt32() == 0 ? std.NoInfo : std.Ok);
                    break;

                case eTopPar.EstadoJacksAyudante:
                    TopSnmpExplorer.PosicionAyudanteSet(pos.name, pos, ((Integer32)data).ToInt32() == 0 ? std.NoInfo : std.Ok);
                    break;

                case eTopPar.EstadoAltavozRadio:
                    TopSnmpExplorer.PosicionALRSet(pos.name, pos, ((Integer32)data).ToInt32() == 0 ? std.NoInfo : std.Ok);
                    break;

                case eTopPar.EstadoAltavozLC:               // Entero
                    TopSnmpExplorer.PosicionALTSet(pos.name, pos, ((Integer32)data).ToInt32() == 0 ? std.NoInfo : std.Ok);
                    break;

                case eTopPar.EstadoLan1:
                    TopSnmpExplorer.PosicionLan1Set(pos.name, pos, ((Integer32)data).ToInt32() == 0 ? std.NoInfo : ((Integer32)data).ToInt32() == 1 ? std.Ok : std.Error);
                    break;

                case eTopPar.EstadoLan2:
                    TopSnmpExplorer.PosicionLan2Set(pos.name, pos, ((Integer32)data).ToInt32() == 0 ? std.NoInfo : ((Integer32)data).ToInt32() == 1 ? std.Ok : std.Error);
                    break;

                case eTopPar.EstadoSync:
                    TopSnmpExplorer.PosicionSyncStatusSet(pos.name, pos, ((OctetString)data).ToString());
                    break;

                case eTopPar.SeleccionPaginaRadio:          // Entero.
                    RecordEvent<U5kSnmpSystemAgent>(DateTime.Now, eIncidencias.ITO_PAGINA_FRECUENCIAS, 0, pos.name, Params(((Integer32)data).ToInt32(), pos.name));
                    break;

                case eTopPar.EstadoPtt:                     // String... 'SECT_1 PTT-ON en Sector, SECT_0 PTT-OFF en Sector...
                    {
                        string[] _str = ((OctetString)data).ToString().Split('_');
                        if (_str.Length == 2)
                            RecordEvent<U5kSnmpSystemAgent>(DateTime.Now, eIncidencias.ITO_PTT, 0, pos.name, Params(pos.name, _str[0] == "0" ? "OFF" : "ON", _str[1]));
                    }
                    break;

                case eTopPar.LlamadaEntrante:               // STRING
                    RecordEvent<U5kSnmpSystemAgent>(DateTime.Now, eIncidencias.ITO_LLAMADA_ENTRANTE, eTiposInci.TEH_TOP, pos.name, Params(((OctetString)data).ToString(), pos.name));
                    break;
                case eTopPar.LlamadaSaliente:               // STRING
                    RecordEvent<U5kSnmpSystemAgent>(DateTime.Now, eIncidencias.ITO_LLAMADA_SALIENTE, eTiposInci.TEH_TOP, pos.name, Params(((OctetString)data).ToString(), pos.name));
                    break;
                case eTopPar.LlamadaEstablecida:            // STRING
                    RecordEvent<U5kSnmpSystemAgent>(DateTime.Now, eIncidencias.ITO_LLAMADA_ESTABLECIDA, eTiposInci.TEH_TOP, pos.name, Params(((OctetString)data).ToString(), pos.name));
                    break;
                case eTopPar.LlamadaFinaliza:               // STRING
                    RecordEvent<U5kSnmpSystemAgent>(DateTime.Now, eIncidencias.ITO_LLAMADA_FIN, eTiposInci.TEH_TOP, pos.name, Params(((OctetString)data).ToString(), pos.name));
                    break;

                case eTopPar.FacilidadTelefonia:            // String
                    RecordEvent<U5kSnmpSystemAgent>(DateTime.Now, eIncidencias.ITO_FACILIDAD_SELECCIONADA, eTiposInci.TEH_TOP, pos.name, Params(((OctetString)data).ToString(), pos.name));
                    break;

                case eTopPar.Briefing:                      // STRING
                    {
                        string[] _str = ((OctetString)data).ToString().Split('_');
                        if (_str.Length == 2)
                            RecordEvent<U5kSnmpSystemAgent>(DateTime.Now, eIncidencias.ITO_SESION_BREIFING, eTiposInci.TEH_TOP, pos.name, Params(_str[1] == "0" ? "OFF" : "ON", _str[0]));
                    }
                    break;

                case eTopPar.Replay:                        // STRING
                    {
                        string[] _str = ((OctetString)data).ToString().Split('_');
                        if (_str.Length == 2)
                            RecordEvent<U5kSnmpSystemAgent>(DateTime.Now, eIncidencias.ITO_FUNCION_REPLAY, eTiposInci.TEH_TOP, pos.name, Params(_str[1] == "0" ? "OFF" : "ON", _str[0]));
                    }
                    break;

                default:
                    LogWarn<U5kSnmpSystemAgent>(String.Format("Recibido TRAP-TOP OID-No Esperado de {0}.{1}", pos.ip, parametro.ToString()));
                    break;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="oid"></param>
        /// <param name="valor"></param>
        void RecibidoEventoExterno(string oid, string valor)
        {
            int nInci;
            string idhw = oid.StartsWith(Properties.u5kManServer.Default.HfEventOids) ? "NBX" :
                oid.StartsWith(Properties.u5kManServer.Default.CfgEventOid) ? "CFG" : "???";

            string[] _oid_val = oid.Split('.');
            if (int.TryParse(_oid_val[_oid_val.Length - 1], out nInci))
            {
                List<string> _params = valor.Split(',').ToList();

                /** El primer Parámetro es el numero de Incidencia */
                _params.RemoveAt(0);

                /** Si viene del NODEBOX, el segundo parámetro es el ID-Hardware*/
                if (idhw == "NBX")
                {
                    idhw = _params[0];
                    _params.RemoveAt(0);
                }

                /** Completo con un máximo de 8 parámetros.... */
                for (int i = _params.Count; i < 8; i++)
                    _params.Add("");
                RecordEvent<U5kSnmpSystemAgent>(DateTime.Now, (eIncidencias)nInci, eTiposInci.TEH_SISTEMA, idhw, _params.ToArray());
            }
            else
            {
                LogWarn<U5kSnmpSystemAgent>(String.Format("RecibidoEventoExterno. Error de Formato. OID={0}, VAL={1}", oid, valor));
            }

        }

#if DEBUG
        public void TestTrap()
        {
            RecibidoTrap("", ".1.1.100.1.3051",
                new OctetString("3051,[GEAR INIT] Error de configuraci?n en el Canal de la emision. Se asigna el valor por defecto.  Node: RX_UHF_M1 {IP=192.168.2.101} {S=Initial} {F=243.000} {ReB=} {ReT=}"),
                new IPEndPoint(IPAddress.Parse("127.0.0.1"), 162),
                new IPEndPoint(IPAddress.Parse("127.0.0.1"), 621));
        }
#endif
        public class AgentData
        {
            public class RMONEventTableItem
            {
                public int index { get; set; }
                public string description { get; set; }
                public int type { get; set; }
                public string community { get; set; }
                public string owner { get; set; }
                public int last { get; set; }

            }
            public class RMONAlarmTableItem
            {
                public int index { get; set; }
                public int interval { get; set; }
                public string variable { get; set; }
                public int stype { get; set; }
                public int lastv { get; set; }
                public int when { get; set; }
                public int rTh { get; set; }
                public int fTh { get; set; }
                public int reindex { get; set; }
                public int feindex { get; set; }
                public string owner { get; set; }
                public int st { get; set; }
                public DateTime lastp { get; set; }
            }
            public static string OidBase
            {
                get
                {
                    return Properties.u5kManServer.Default.SnmpEnterpriseBaseOidType==0 ? ".1.3.6.1.4.1.7916" : ".1.3.6.1.4.1.2363.6";      // Eurocontrol
                }
            }
            public object SnmpStatistics
            {
                get
                {
                    return SnmpAgent.Statictics;
                }
            }
            private readonly Object _rMonEventTable = new List<Object>()
            {
                new RMONEventTableItem(){index=1, description="Normal",  type=4, community="public", owner="NucleoCC", last=(int)(Environment.TickCount / 10) },
                new RMONEventTableItem(){index=2, description="Warning", type=4, community="public", owner="NucleoCC", last=(int)(Environment.TickCount / 10) },
                new RMONEventTableItem(){index=3, description="Alarm",   type=4, community="public", owner="NucleoCC", last=(int)(Environment.TickCount / 10) },
            };
            public object RMONEventTable
            {
                get
                {
                    return _rMonEventTable;
                }
            }
            private readonly Object _rMonAlarmTable = new List<Object>()
            {
#if !DEBUG1
                /** Calidad de Estado GRAL */
                new RMONAlarmTableItem()
                {
                    index =1, interval=30, variable=OidBase + ".8.1.5.8.1.0",
                    stype =1, lastv=100, when=1, rTh=30, fTh=0, reindex=2, feindex=0,
                    owner ="NucleoCC", st=1, lastp=DateTime.Now
                },
                new RMONAlarmTableItem()
                {
                    index =3, interval=30, variable=OidBase + ".8.1.5.8.1.0",
                    stype =1, lastv=100, when=1, rTh=60, fTh=0, reindex=1, feindex=0,
                    owner ="NucleoCC", st=1, lastp=DateTime.Now
                },
                new RMONAlarmTableItem()
                {
                    index =1, interval=30, variable=OidBase + ".8.1.5.8.1.0",
                    stype =1, lastv=100, when=2, rTh=0, fTh=60, reindex=0, feindex=2,
                    owner ="NucleoCC", st=1, lastp=DateTime.Now
                },
                new RMONAlarmTableItem()
                {
                    index =3, interval=30, variable=OidBase + ".8.1.5.8.1.0",
                    stype =1, lastv=100, when=2, rTh=0, fTh=30, reindex=0, feindex=3,
                    owner ="NucleoCC", st=1, lastp=DateTime.Now
                },
                /** Calidad de Estado TOPS */
                new RMONAlarmTableItem()
                {
                    index =1, interval=30, variable=OidBase + ".8.1.5.8.2.0",
                    stype =1, lastv=100, when=1, rTh=30, fTh=0, reindex=2, feindex=0,
                    owner ="NucleoCC", st=1, lastp=DateTime.Now
                },
                new RMONAlarmTableItem()
                {
                    index =3, interval=30, variable=OidBase + ".8.1.5.8.2.0",
                    stype =1, lastv=100, when=1, rTh=60, fTh=0, reindex=1, feindex=0,
                    owner ="NucleoCC", st=1, lastp=DateTime.Now
                },
                new RMONAlarmTableItem()
                {
                    index =1, interval=30, variable=OidBase + ".8.1.5.8.2.0",
                    stype =1, lastv=100, when=2, rTh=0, fTh=60, reindex=0, feindex=2,
                    owner ="NucleoCC", st=1, lastp=DateTime.Now
                },
                new RMONAlarmTableItem()
                {
                    index =3, interval=30, variable=OidBase + ".8.1.5.8.2.0",
                    stype =1, lastv=100, when=2, rTh=0, fTh=30, reindex=0, feindex=3,
                    owner ="NucleoCC", st=1, lastp=DateTime.Now
                },
                /** Calidad de Estado GWS */
                new RMONAlarmTableItem()
                {
                    index =1, interval=30, variable=OidBase + ".8.1.5.8.3.0",
                    stype =1, lastv=100, when=1, rTh=30, fTh=0, reindex=2, feindex=0,
                    owner ="NucleoCC", st=1, lastp=DateTime.Now
                },
                new RMONAlarmTableItem()
                {
                    index =3, interval=30, variable=OidBase + ".8.1.5.8.3.0",
                    stype =1, lastv=100, when=1, rTh=60, fTh=0, reindex=1, feindex=0,
                    owner ="NucleoCC", st=1, lastp=DateTime.Now
                },
                new RMONAlarmTableItem()
                {
                    index =1, interval=30, variable=OidBase + ".8.1.5.8.3.0",
                    stype =1, lastv=100, when=2, rTh=0, fTh=60, reindex=0, feindex=2,
                    owner ="NucleoCC", st=1, lastp=DateTime.Now
                },
                new RMONAlarmTableItem()
                {
                    index =3, interval=30, variable=OidBase + ".8.1.5.8.3.0",
                    stype =1, lastv=100, when=2, rTh=0, fTh=30, reindex=0, feindex=3,
                    owner ="NucleoCC", st=1, lastp=DateTime.Now
                },
                /** Calidad de Estado EXTS */
                new RMONAlarmTableItem()
                {
                    index =1, interval=30, variable=OidBase + ".8.1.5.8.4.0",
                    stype =1, lastv=100, when=1, rTh=30, fTh=0, reindex=2, feindex=0,
                    owner ="NucleoCC", st=1, lastp=DateTime.Now
                },
                new RMONAlarmTableItem()
                {
                    index =3, interval=30, variable=OidBase + ".8.1.5.8.4.0",
                    stype =1, lastv=100, when=1, rTh=60, fTh=0, reindex=1, feindex=0,
                    owner ="NucleoCC", st=1, lastp=DateTime.Now
                },
                new RMONAlarmTableItem()
                {
                    index =1, interval=30, variable=OidBase + ".8.1.5.8.4.0",
                    stype =1, lastv=100, when=2, rTh=0, fTh=60, reindex=0, feindex=2,
                    owner ="NucleoCC", st=1, lastp=DateTime.Now
                },
                new RMONAlarmTableItem()
                {
                    index =3, interval=30, variable=OidBase + ".8.1.5.8.4.0",
                    stype =1, lastv=100, when=2, rTh=0, fTh=30, reindex=0, feindex=3,
                    owner ="NucleoCC", st=1, lastp=DateTime.Now
                },
                /** Calidad de Estado TELEFONIA */
                new RMONAlarmTableItem()
                {
                    index =1, interval=30, variable=OidBase + ".8.1.5.8.5.0",
                    stype =1, lastv=100, when=1, rTh=30, fTh=0, reindex=2, feindex=0,
                    owner ="NucleoCC", st=1, lastp=DateTime.Now
                },
                new RMONAlarmTableItem()
                {
                    index =3, interval=30, variable=OidBase + ".8.1.5.8.5.0",
                    stype =1, lastv=100, when=1, rTh=60, fTh=0, reindex=1, feindex=0,
                    owner ="NucleoCC", st=1, lastp=DateTime.Now
                },
                new RMONAlarmTableItem()
                {
                    index =1, interval=30, variable=OidBase + ".8.1.5.8.5.0",
                    stype =1, lastv=100, when=2, rTh=0, fTh=60, reindex=0, feindex=2,
                    owner ="NucleoCC", st=1, lastp=DateTime.Now
                },
                new RMONAlarmTableItem()
                {
                    index =3, interval=30, variable=OidBase + ".8.1.5.8.5.0",
                    stype =1, lastv=100, when=2, rTh=0, fTh=30, reindex=0, feindex=3,
                    owner ="NucleoCC", st=1, lastp=DateTime.Now
                },
                /** Calidad de Estado RADIO */
                new RMONAlarmTableItem()
                {
                    index =1, interval=30, variable=OidBase + ".8.1.5.8.6.0",
                    stype =1, lastv=100, when=1, rTh=30, fTh=0, reindex=2, feindex=0,
                    owner ="NucleoCC", st=1, lastp=DateTime.Now
                },
                new RMONAlarmTableItem()
                {
                    index =3, interval=30, variable=OidBase + ".8.1.5.8.6.0",
                    stype =1, lastv=100, when=1, rTh=60, fTh=0, reindex=1, feindex=0,
                    owner ="NucleoCC", st=1, lastp=DateTime.Now
                },
                new RMONAlarmTableItem()
                {
                    index =1, interval=30, variable=OidBase + ".8.1.5.8.6.0",
                    stype =1, lastv=100, when=2, rTh=0, fTh=60, reindex=0, feindex=2,
                    owner ="NucleoCC", st=1, lastp=DateTime.Now
                },
                new RMONAlarmTableItem()
                {
                    index =3, interval=30, variable=OidBase + ".8.1.5.8.6.0",
                    stype =1, lastv=100, when=2, rTh=0, fTh=30, reindex=0, feindex=3,
                    owner ="NucleoCC", st=1, lastp=DateTime.Now
                },
#endif
                new RMONAlarmTableItem()
                {
                    index =1, interval=30, variable=OidBase + ".8.1.5.8.7.0",
                    stype =1, lastv=100, when=1, rTh=30, fTh=0, reindex=2, feindex=0,
                    owner ="NucleoCC", st=1, lastp=DateTime.Now
                },
                new RMONAlarmTableItem()
                {
                    index =3, interval=30, variable=OidBase + ".8.1.5.8.7.0",
                    stype =1, lastv=100, when=1, rTh=60, fTh=0, reindex=1, feindex=0,
                    owner ="NucleoCC", st=1, lastp=DateTime.Now
                },
                new RMONAlarmTableItem()
                {
                    index =1, interval=30, variable=OidBase + ".8.1.5.8.7.0",
                    stype =1, lastv=100, when=2, rTh=0, fTh=60, reindex=0, feindex=2,
                    owner ="NucleoCC", st=1, lastp=DateTime.Now
                },
                new RMONAlarmTableItem()
                {
                    index =3, interval=30, variable=OidBase + ".8.1.5.8.7.0",
                    stype =1, lastv=100, when=2, rTh=0, fTh=30, reindex=0, feindex=3,
                    owner ="NucleoCC", st=1, lastp=DateTime.Now
                },                
            };
            public object RMONAlarmTable
            {
                get
                {
                    return _rMonAlarmTable;
                }
            }
            public Action<ObjectIdentifier, IList<Variable>> RMonEventTrap = new Action<ObjectIdentifier, IList<Variable>>((trap, lvars) =>
            {
                SnmpRmonTrapReceiver((server) =>
                {
                    SnmpAgent.Trap(trap, lvars, server);
                });
            });
            static void SnmpRmonTrapReceiver(Action<IPEndPoint> action)
            {
                string endpstr = Properties.u5kManServer.Default.SnmpRMONTrapReceiver;
                UriBuilder uri = new UriBuilder("snmp://" + endpstr);
                try
                {
                    IPEndPoint endp = new IPEndPoint(IPAddress.Parse(uri.Host), uri.Port==-1 ? 162 : uri.Port);
                    action(endp);
                }
                catch
                {
                    LogWarn<U5kSnmpSystemAgent>($"No puedo obtener el servidor RMON: {endpstr}");
                }
            }
        }
        public static AgentData rMONData = new AgentData();
    }
}
