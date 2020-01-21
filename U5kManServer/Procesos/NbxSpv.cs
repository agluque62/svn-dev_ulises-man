using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;
using System.Net.Http;
using System.Timers;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Converters;

using U5kBaseDatos;
using Utilities;

namespace U5kManServer
{
    class NbxSpv : NucleoGeneric.NGThread/*, IDisposable*/
    {
        /// <summary>
        /// 
        /// </summary>
        enum eNbxMngEvents { CAIDO, ACTIVO, MASTER, SLAVE, NONBX, ERROR }
        /// <summary>
        /// 
        /// </summary>
        enum eGlobalStatus { INICIO, NORMAL, ALARMA }
        /// <summary>
        /// 
        /// </summary>
        static eGlobalStatus globalStatus = eGlobalStatus.INICIO;
        eGlobalStatus GlobalStatus
        {
            get
            {
                return globalStatus;
            }
            set
            {
                LogWarn<NbxSpv>(String.Format("ScanNbxThread GlobalStatus {0}=>{1}", globalStatus, value));
                globalStatus = value;
                timer_largo = 0;
            }
        }
        // TODO. Falta Incluir PabxService solo cuando está activado...
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        bool FaltaAlgunMaster()
        {
            U5KStdGeneral stdg = U5kManService._std.STDG;
            return (
            (stdg.lstNbx.Where(n => n.CfgService == U5KStdGeneral.NbxServiceState.Master).Count() == 0) ||
            (stdg.lstNbx.Where(n => n.RadioService == U5KStdGeneral.NbxServiceState.Master).Count() == 0) ||
            (stdg.lstNbx.Where(n => n.TifxService == U5KStdGeneral.NbxServiceState.Master).Count() == 0)
            );
        }
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        bool TodosMasterOk()
        {
            U5KStdGeneral stdg = U5kManService._std.STDG;
            return (
            (stdg.lstNbx.Where(n => n.CfgService == U5KStdGeneral.NbxServiceState.Master).Count() == 1) &&
            (stdg.lstNbx.Where(n => n.RadioService == U5KStdGeneral.NbxServiceState.Master).Count() == 1) &&
            (stdg.lstNbx.Where(n => n.TifxService == U5KStdGeneral.NbxServiceState.Master).Count() == 1)
            );
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="viejo"></param>
        /// <param name="nuevo"></param>
        /// <param name="eventid"></param>
        /// <returns></returns>
        U5KStdGeneral.NbxServiceState ServiceStatusSet(U5KStdGeneral.NbxServiceState viejo, U5KStdGeneral.NbxServiceState nuevo, string eventid)
        {
            if (nuevo != U5KStdGeneral.NbxServiceState.NoIni && nuevo != viejo)
            {
                RaiseNbxMngEvent(eventid, nuevo== U5KStdGeneral.NbxServiceState.Master ? eNbxMngEvents.MASTER : 
                    nuevo== U5KStdGeneral.NbxServiceState.Slave ? eNbxMngEvents.SLAVE :
                    eNbxMngEvents.ERROR);
            }
            return nuevo;
        }        
        /// <summary>
        /// 
        /// </summary>
        System.Timers.Timer _timer = new System.Timers.Timer(1000);
        int timer_largo = 0;
        void _timer_Elapsed(object sender, ElapsedEventArgs eventArgs)
        {
            if (U5kManService._std.wrAccAcquire())
            {
                try
                {
                    // Evento....
                    if (U5kManService._Master == true)
                    {
                        //U5kGenericos.SetCurrentCulture();
                        ConfigCultureSet();
                        U5KStdGeneral stdg = U5kManService._std.STDG;

                        LogTrace<NbxSpv>("NbxTimerElapsed: " + eventArgs.SignalTime.ToLocalTime());

                        /** Superviso a los que no responden */
                        List<U5KStdGeneral.StdNbx> aBorrar = new List<U5KStdGeneral.StdNbx>();
                        foreach (U5KStdGeneral.StdNbx nbx in stdg.lstNbx)
                        {
                            if (nbx.timer-- <= 0)
                                aBorrar.Add(nbx);
                        }
                        foreach (U5KStdGeneral.StdNbx nbx in aBorrar)
                        {
                            stdg.lstNbx.Remove(nbx);
                            // Evento NBX-CAIDO...
                            RaiseNbxMngEvent(nbx.ip, eNbxMngEvents.CAIDO);
                        }

                        /** Supervision TIMER-LARGO */
                        if (++timer_largo == 30)
                        {
                            /** Supervisa que tras el timer largo de inicio se activa algun NODEBOX */
                            {
                                if (GlobalStatus == eGlobalStatus.INICIO && FaltaAlgunMaster())
                                {
                                    RaiseNbxMngEvent(null, eNbxMngEvents.NONBX);
                                    GlobalStatus = eGlobalStatus.ALARMA;
                                }
                            }

                            timer_largo = 0;
                        }
                        /** Supervision TIMER-CORTO */
                        switch (GlobalStatus)
                        {
                            case eGlobalStatus.INICIO:
                            case eGlobalStatus.ALARMA:
                                if (TodosMasterOk())
                                    GlobalStatus = eGlobalStatus.NORMAL;
                                break;
                            case eGlobalStatus.NORMAL:
                                if (FaltaAlgunMaster() || !TodosMasterOk())
                                    GlobalStatus = eGlobalStatus.INICIO;
                                break;
                        }

                        U5kManService._std.STDG = stdg;
                    }
                    else
                    {
                        /** Si no soy MASTER borro la lista */
                        U5kManService._std.STDG.lstNbx.Clear();
                    }
                }
                catch (Exception x)
                {
                    LogException<NbxSpv>("", x);
                }
                finally
                {
                    U5kManService._std.wrAccRelease();
                }
            }
            _timer.Enabled = true;            
        }

        /// <summary>
        /// 
        /// </summary>
        public NbxSpv()
        {
            Name = "ScanNbxThread";
        }
        /// <summary>
        /// 
        /// </summary>
        public override bool Start()
        {
            _timer.Elapsed += new ElapsedEventHandler(_timer_Elapsed);
            _timer.AutoReset = false;
            _timer.Enabled = true;

            _http_timer.Elapsed += new ElapsedEventHandler(_http_timer_Elapsed);
            _http_timer.AutoReset = false;
            _http_timer.Enabled = true;

            {
                U5KStdGeneral stdg = U5kManService._std.STDG;
                foreach (U5KStdGeneral.StdNbx nbx in stdg.lstNbx)
                {
                    stdg.lstNbx.Add(new U5KStdGeneral.StdNbx()
                    {
                        ip = nbx.ip,
                        webport = nbx.webport,
                        CfgService = nbx.CfgService,
                        RadioService = nbx.RadioService,
                        TifxService = nbx.TifxService,
                        PresenceService = nbx.PresenceService,
                        timer = nbx.timer,
                        Running = nbx.Running
                    });
                }
                // AGL. Como no es un evento evito el acceso por semaforo...
                U5kManService._std.STDG = stdg;
            }
            base.Start();
            return true;
        }
        /// <summary>
        /// 
        /// </summary>
        protected void Dispose()
        {
            _timer.Enabled = false;
            _timer.Elapsed -= new ElapsedEventHandler(_timer_Elapsed);

            _http_timer.Enabled = false;
            _http_timer.Elapsed -= new ElapsedEventHandler(_http_timer_Elapsed);

            //_listener.Client.Close();
            //_listener.Close();
            
            /** 20180309. Limpio la lista para que no persistan posibles errores. */
            U5kManService._std.STDG.lstNbx.Clear();

            LogDebug<NbxSpv>("NbxScanner Dispose...");
        }

        /// <summary>
        /// 
        /// </summary>
        UdpClient _listener = null;// new UdpClient(new IPEndPoint(IPAddress.Any, Properties.u5kManServer.Default.nbxSupPort));        
        /// <summary>
        /// 
        /// </summary>
        protected override void Run()
        {
            U5kGenericos.TraceCurrentThread(this.GetType().Name);
            //U5kGenericos.SetCurrentCulture();

            LogDebug<NbxSpv>("NbxScanner Starting...");
            
            //Init();

            using (_listener = new UdpClient(new IPEndPoint(IPAddress.Any, Properties.u5kManServer.Default.nbxSupPort)))
            {
                _listener.Client.ReceiveTimeout = 1000;

                while (IsRunning())
                {
                    try
                    {
                        // Evento....
                        IPEndPoint from = new IPEndPoint(IPAddress.Any, Properties.u5kManServer.Default.nbxSupPort);
                        Byte[] recibido = _listener.Receive(ref from);
                        if (U5kManService._std.wrAccAcquire())
                        {
                            if (U5kManService._Master == true)
                            {
                                U5KStdGeneral stdg = U5kManService._std.STDG;
                                /** Si es la primera vez que aparece lo añado a la lista */
                                U5KStdGeneral.StdNbx itemNbx = stdg.lstNbx.Find(x => x.ip == from.Address.ToString());
                                if (itemNbx == null)
                                {
                                    itemNbx = new U5KStdGeneral.StdNbx()
                                    {
                                        ip = from.Address.ToString(),
                                        webport = recibido.Length > 9 ? (recibido[9] << 8 | recibido[8]) : 1023,
                                        CfgService = U5KStdGeneral.NbxServiceState.NoIni,
                                        RadioService = U5KStdGeneral.NbxServiceState.NoIni,
                                        TifxService = U5KStdGeneral.NbxServiceState.NoIni,
                                        PresenceService = U5KStdGeneral.NbxServiceState.NoIni,
                                        Running = true
                                    };

                                    stdg.lstNbx.Add(itemNbx);
                                    // Evento NBX-ACTIVO
                                    RaiseNbxMngEvent(itemNbx.ip, eNbxMngEvents.ACTIVO);
                                }

                                // 20171010. Por si se ha perdido el attributo en una copia....
                                itemNbx.Running = true;

                                // Si hay cambio en seleccionado EVENTO MASTER / SLAVE en Cada Servicio.
                                itemNbx.CfgService = ServiceStatusSet(itemNbx.CfgService, (U5KStdGeneral.NbxServiceState)recibido[0], itemNbx.ip + "_CfgService");
                                if (recibido.Length > 4)
                                {
                                    itemNbx.RadioService = ServiceStatusSet(itemNbx.RadioService, (U5KStdGeneral.NbxServiceState)recibido[1], itemNbx.ip + "_RdService");
                                    itemNbx.TifxService = ServiceStatusSet(itemNbx.TifxService, (U5KStdGeneral.NbxServiceState)recibido[2], itemNbx.ip + "_TifxService");
                                    itemNbx.PresenceService = ServiceStatusSet(itemNbx.PresenceService, (U5KStdGeneral.NbxServiceState)recibido[3], itemNbx.ip + "_PresenceService");
                                }

                                itemNbx.timer = 10;
                                U5kManService._std.STDG = stdg;
                            }
                            U5kManService._std.wrAccRelease();
                        }
                    }
                    catch (Exception x)
                    {
                        if (x is ThreadAbortException)
                        {
                            Thread.ResetAbort();
                            LogDebug<NbxSpv>("NbxScanner. Abortando...");
                            break;
                        }
                        else if (x is SocketException &&
                            ((SocketException)x).ErrorCode == 10060)
                        {
#if DEBUG1
                            LogDebug<NbxSpv>("NbxScanner TIMEOUT Exception");
#endif
                        }
                        else
                        {
                            LogException<MainThread>("", x);
                        }
                    }
                }
            }
            Dispose();
            LogInfo<NbxSpv>("Finalizado...");
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="evento"></param>        
        private void RaiseNbxMngEvent(string id, eNbxMngEvents evento)
        {
            switch (evento)
            {
                case eNbxMngEvents.ACTIVO:
                    RecordEvent<NbxSpv>(DateTime.Now, eIncidencias.IGRL_NBXMNG_EVENT, eTiposInci.TEH_SISTEMA, "SPV",
                        new object[] { idiomas.strings.NBX_Activo/*"NBX Activado en"*/, id, "", "", "", "", "", "" });
                    break;
                case eNbxMngEvents.CAIDO:
                    RecordEvent<NbxSpv>(DateTime.Now, eIncidencias.IGRL_NBXMNG_EVENT, eTiposInci.TEH_SISTEMA, "SPV",
                        new object[] { idiomas.strings.NBX_Caido/*"NBX Caido en"*/, id, "", "", "", "", "", "" });
                    break;
                case eNbxMngEvents.MASTER:
                    RecordEvent<NbxSpv>(DateTime.Now, eIncidencias.IGRL_NBXMNG_EVENT, eTiposInci.TEH_SISTEMA, "SPV",
                        new object[] { "NBX", id, "MASTER", "", "", "", "", "" });
                    break;
                case eNbxMngEvents.SLAVE:
                    RecordEvent<NbxSpv>(DateTime.Now, eIncidencias.IGRL_NBXMNG_EVENT, eTiposInci.TEH_SISTEMA, "SPV",
                        new object[] { "NBX", id, "SLAVE", "", "", "", "", "" });
                    break;
                case eNbxMngEvents.NONBX:
                    RecordEvent<NbxSpv>(DateTime.Now, eIncidencias.IGRL_NBXMNG_ALARM, eTiposInci.TEH_SISTEMA, "SPV",
                        new object[] { idiomas.strings.NBX_NoService/*"No Hay Servicio NBX MASTER en el Sistema"*/, "", "", "", "", "", "", "" });
                    break;
            }
            LogWarn<NbxSpv>(String.Format("ScanNbxThread ({2}) RaiseNbxMngEvent: {0}, {1}", id ?? "GLOBAL", evento, GlobalStatus));
        }
        /// <summary>
        /// 
        /// </summary>
        private void Init()
        {
            while (IsRunning())
            {
                try
                {
                    _listener = new UdpClient(new IPEndPoint(IPAddress.Any, Properties.u5kManServer.Default.nbxSupPort));
                    _listener.Client.ReceiveTimeout = 1000;
                    return;
                }
                catch (Exception x)
                {
                    LogException<NbxSpv>("", x);
                }
                Sleep(5000);
            }
        }

        #region Servicio Radio

        /** */
        System.Timers.Timer _http_timer = new System.Timers.Timer(1000);
        void _http_timer_Elapsed(object sender, ElapsedEventArgs eventArgs)
        {
            if (U5kManService._Master == true)
            {
                //U5kGenericos.SetCurrentCulture();
                ConfigCultureSet();
                _http_timer.Enabled = false;
                try
                {
                    GetOperationalData();
                }
                catch (Exception x)
                {
                    LogException<NbxSpv>("", x);
                }
            }
            else
            {
                _http_timer.Enabled = true;
            }
        }

#if DEBUG
        int _count_in = 0;
        int _count_out = 0;
#endif
        /// <summary>
        /// 
        /// </summary>
        private async void GetOperationalData()
        {
            // Busca el NBX Activo.
            U5KStdGeneral.StdNbx curNbx = null;
            curNbx = U5kManService._std.STDG.lstNbx.Find(x => x.Running == true && x.RadioService == U5KStdGeneral.NbxServiceState.Master);
#if DEBUG
            if (true)
            {
                string sesPg = "http://192.168.0.212:1023/rdsessions";
                string mnmPg = "http://192.168.0.212:1023/gestormn";
                string thfPg = "http://192.168.0.212:1023/rdhf";
                string prePg = "http://192.168.0.212:1023/tifxinfo";
                _count_in++;
#else
            if (curNbx != null)
            {
                string sesPg = "http://" + curNbx.ip + ":" + curNbx.webport.ToString() + "/rdsessions";
                string mnmPg = "http://" + curNbx.ip + ":" + curNbx.webport.ToString() + "/gestormn";
                string thfPg = "http://" + curNbx.ip + ":" + curNbx.webport.ToString() + "/rdhf";
                string prePg = "http://" + curNbx.ip + ":" + curNbx.webport.ToString() + "/tifxinfo";

#endif
                // ... Use HttpClient.
                try
                {
                    using (HttpClient client = new HttpClient())
                    using (HttpResponseMessage 
                        SesRsp = await client.GetAsync(sesPg), 
                        MNRsp = await client.GetAsync(mnmPg), 
                        HFRsp = await client.GetAsync(thfPg),
                        PSRsp = await client.GetAsync(prePg))
                    using (HttpContent 
                        SesContent = SesRsp.Content, 
                        MNContent = MNRsp.Content, 
                        tHFContent = HFRsp.Content,
                        preContent = PSRsp.Content)
                    {
                        // ... Read the string.
                        try
                        {
                            string result = await SesContent.ReadAsStringAsync();
                            U5kManService._sessions_data = JsonConvert.DeserializeObject<List<U5kManService.radioSessionData>>(result);
                            string result1 = await MNContent.ReadAsStringAsync();
                            U5kManService._MNMan_data = JsonConvert.DeserializeObject<List<U5kManService.equipoMNData>>(result1);
                            /** 20171010. Datos Equipos HF */
                            string result2 = await tHFContent.ReadAsStringAsync();
                            U5kManService._txhf_data = JsonConvert.DeserializeObject<List<U5kManService.txHF>>(result2);
                            /** 20180214. Datos del servicio de Presencia */
                            U5kManService._ps_data = await preContent.ReadAsStringAsync();

                            /** 20181010. De los datos obtenemos el estado de emergencia */
                            JArray grps = JsonHelper.SafeJArrayParse(U5kManService._ps_data);
                            JObject prx_grp = grps == null ? null :
                                grps.Where(u => u.Value<int>("tp") == 4).FirstOrDefault() as JObject;
                            JProperty prx_prop = prx_grp == null ? null : prx_grp.Property("res");
                            JArray proxies = prx_prop == null ? null : prx_prop.Value as JArray;
                            int ppal = proxies == null ? 0 : proxies.Where(u => u.Value<int>("tp") == 5 && u.Value<int>("std") == 0).Count();
                            int alt = proxies == null ? 0 : proxies.Where(u => u.Value<int>("tp") == 6 && u.Value<int>("std") == 0).Count();

                            U5kManService.tlf_mode = ppal > 0 ? 0 /** OK */ : alt > 0 ? 1 /** DEG */ : 2 /** EMG */;
                            /** */
                        }
                        catch (Exception x)
                        {
                            LogException<NbxSpv>("getRdData-01", x);
                        }
                    }
                }
                catch (Exception x)
                {
                    LogException<NbxSpv>("getRdData-02", x);
                }
            }
            else
            {
                LogDebug<NbxSpv>("No encuentro NODEBOX ACTIVO");
            }
#if DEBUG
            _count_out++;
#endif
            _http_timer.Interval = 5000;
            _http_timer.Enabled = true;
        }

        #endregion
    }

    #region NUEVA-VERSION

    #endregion
}
