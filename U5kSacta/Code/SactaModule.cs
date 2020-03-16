using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Timers;
using System.Diagnostics;
using System.Collections.Generic;
using System.Text;
using System.Xml;
using System.Threading.Tasks;

using Newtonsoft.Json;

using Utilities;

namespace U5kSacta
{
    public class SactaModule
    {
        public event GenericEventHandler<Dictionary<string, object>> SactaActivityChanged;

        #region Declaración de atributos
        enum SactaState { WaitingSactaActivity, /*WaitingIOLActivity,*/ WaitingSectorization, WaitingSectFinish, SendingPresences, Stopped }

        SactaConfig Config = null;
        object _Sync = null;
        UdpSocket _socket = null;

        IPEndPoint[] _EndPoint;
        int _ActivityState;
        int _ActivityTimeOut;
        int _PresenceInterval;
        SactaState _State;
        DateTime[] _LastSactaReceived;
        DateTime _BeginOfWaitForSect;
        DateTime _LastPresenceSended;
        int _SeqNum = 0;
        Timer _PeriodicTasks;
        uint _TryingSectVersion;
        Dictionary<ushort, PSIInfo> _SactaSPSIUsers;
        Dictionary<ushort, PSIInfo> _SactaSPVUsers;
        bool _Disposed;
        string strModuleState
        {
            get
            {
                return string.Format("Estado Global: [Activity: {0}, State: {1}]", _ActivityState, _State);
            }
        }
        #endregion
        /// <summary>
        /// 
        /// </summary>
        public byte State
        {
            get { return Convert.ToByte(_State != SactaState.Stopped); }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="idSistema"></param>
        public SactaModule()
        {
            SactaConfig.GetConfig((cfg, error) =>
            {
                Config = cfg;
                try
                {
                    _ActivityTimeOut = Config.TimeoutPresencia;
                    _PresenceInterval = Config.TickPresencia;

                    _Sync = new object();
                    _State = SactaState.Stopped;
                    _LastSactaReceived = new DateTime[2];

                    _SactaSPSIUsers = Config.sacta.SpiUsers.Split(new char[] { ',', ';' }, StringSplitOptions.RemoveEmptyEntries)
                        .Select(u => u).ToDictionary(k => UInt16.Parse(k), k => new PSIInfo());

                    _SactaSPVUsers = Config.sacta.SpvUsers.Split(new char[] { ',', ';' }, StringSplitOptions.RemoveEmptyEntries)
                        .Select(u => u).ToDictionary(k => UInt16.Parse(k), k => new PSIInfo());

                    _EndPoint = new IPEndPoint[2]
                    {
                    new IPEndPoint(IPAddress.Parse(Config.sacta.lan1.mcast), Config.sacta.lan1.udpport),
                    new IPEndPoint(IPAddress.Parse(Config.sacta.lan2.mcast), Config.sacta.lan2.udpport)
                    };
                    /** 20180716. Para seleccionar la IP Source.. */
                    _socket = new UdpSocket(Config.scv.Interfaz, Config.scv.udpport);
                    /** Para seleccionar correctamente la Interfaz de salida de las tramas MCAST */
                    _socket.Base.MulticastLoopback = false;
                    _socket.Base.JoinMulticastGroup(IPAddress.Parse(Config.sacta.lan1.mcast), IPAddress.Parse(Config.scv.Interfaz));
                    _socket.Base.JoinMulticastGroup(IPAddress.Parse(Config.sacta.lan2.mcast), IPAddress.Parse(Config.scv.Interfaz));
                    /** 20180731. Para poder pasar por una red de ROUTERS */
                    _socket.Base.Client.SetSocketOption(SocketOptionLevel.IP, SocketOptionName.MulticastTimeToLive, 16);
                    _socket.NewDataEvent += OnNewData;

                    _PeriodicTasks = new Timer(Config.Tick);
                    _PeriodicTasks.AutoReset = false;
                    _PeriodicTasks.Elapsed += PeriodicTasks;

                    Log(false, System.Reflection.MethodBase.GetCurrentMethod().Name, "SactaModule inicializado. Sacta1 en {0}:{1}, Sacta2 en {2}:{3}",
                        _EndPoint[0].Address.ToString(), _EndPoint[0].Port, _EndPoint[1].Address.ToString(), _EndPoint[1].Port);
                    Log(false, System.Reflection.MethodBase.GetCurrentMethod().Name, "SactaModule. Gestionado los Sectores: {0} en las posicones {1} ",
                       SactaBdt.IdSectores, SactaBdt.IdUcs);
                }
                catch(Exception)
                {
                    // TODO...
                }
            });
        }
        /// <summary>
        /// 
        /// </summary>
        public void Start()
        {
            try
            {
                _State = SactaState.WaitingSactaActivity;
                _PeriodicTasks.Enabled = true;
                _socket.BeginReceive();
                Log(false, System.Reflection.MethodBase.GetCurrentMethod().Name, "Modulo arrancado en puerto {0}...", Config.scv.udpport);
            }
            catch(Exception x)
            {
                // TODO.
            }
        }
        /// <summary>
        /// 
        /// </summary>
        public void Stop()
        {
            try
            {
                if (!_Disposed)
                {
                    _Disposed = true;
                    _State = SactaState.Stopped;

                    if (_PeriodicTasks != null)
                    {
                        _PeriodicTasks.Enabled = false;
                        _PeriodicTasks?.Close();
                        _PeriodicTasks = null;
                    }
                    if (_socket != null)
                    {
                        _socket.Dispose();
                        _socket = null;
                    }
                    Log(false, System.Reflection.MethodBase.GetCurrentMethod().Name, "Modulo detenido...");
                }
            }
            catch (Exception x)
            {
                // TODO.
            }
        }

        public class Sacta2017State
        {
            public bool started = false;
            public bool neta_activity = false;
            public bool netb_activity = false;
        }
        public Sacta2017State Status
        {
            get
            {
                return new Sacta2017State()
                {
                    started = _State != SactaState.Stopped,
                    neta_activity = _State == SactaState.Stopped ? false : ((_ActivityState & 0x1) == 0x1) ? true : false,
                    netb_activity = _State == SactaState.Stopped ? false : ((_ActivityState & 0x2) == 0x2) ? true : false
                };
            }
        }

#region Private Members

/// <summary>
/// 
/// </summary>
/// <param name="sender"></param>
/// <param name="dg"></param>
        void OnNewData(object sender, DataGram dg)
        {
            try
            {
                MemoryStream ms = new MemoryStream(dg.Data);
                CustomBinaryFormatter bf = new CustomBinaryFormatter();
                SactaMsg msg = bf.Deserialize<SactaMsg>(ms);
                byte client = dg.Client.Address.GetAddressBytes()[2];

                lock (_Sync)
                {
                    // Se comparta el BYTE 2 para determinar SACTA1 o SACTA2
                    byte sacta1 = IPAddress.Parse(Config.sacta.lan1.ipmask).GetAddressBytes()[2];
                    byte sacta2 = IPAddress.Parse(Config.sacta.lan2.ipmask).GetAddressBytes()[2];

                    int net = client == sacta1 ? 0 : client == sacta2 ? 1 : -1;
                    if (net == -1)
                    {
                        Log(true, System.Reflection.MethodBase.GetCurrentMethod().Name, "Recibida Trama no identificada de {0}", dg.Client.Address.ToString());
                        return;
                    }
                    Log(false, System.Reflection.MethodBase.GetCurrentMethod().Name, "Mensaje de red {0} recibido: {1}", net == 0 ? "SACTA1" : "SACTA2", msg.Type);
                    if (IsValid(msg))
                    {
                        _LastSactaReceived[net] = DateTime.Now;

                        switch (msg.Type)
                        {
                            case SactaMsg.MsgType.Presence:
                                _ActivityTimeOut = (((SactaMsg.PresenceInfo)(msg.Info)).ActivityTimeOutSg * 1000);
                                _PresenceInterval = (((SactaMsg.PresenceInfo)(msg.Info)).PresencePerioditySg * 1000);
                                break;

                            case SactaMsg.MsgType.Sectorization:
                                ProcessMsgSect(net, msg);
                                break;
                        }
                    }
                }
            }
            catch (Exception x)
            {
                //if (!_Disposed)
                //{
                //    // _Logger.ErrorException(Resources.SactaDataError, ex);
                //    const int Error = 1;
                //    uint version = ((SactaMsg.SectInfo)(msg.Info)).Version;
                //    //Settings stts = Settings.Default;
                //    //			ModuleInfo info = new ModuleInfo();
                //    CD40.BD.SactaInfo info = new CD40.BD.SactaInfo();

                //    info["SectVersion"] = version;
                //    info["Resultado"] = Error;
                //    info["ErrorCause"] = x.Message;

                //    OnResultSectorizacion(info);
                //}
                Log(true, System.Reflection.MethodBase.GetCurrentMethod().Name, "Excepcion: {0}", x.Message);
            }
        }

        Queue<SactaMsg> pendindSect = new Queue<SactaMsg>();
        Task pendingSectProc = null;
        void ProcessMsgSect(int net, SactaMsg sect)
        {
            if (!IsSecondSectMsg(sect))
            {
                lock (pendindSect)
                {
                    pendindSect.Enqueue(sect);
                }

                if (pendingSectProc == null)
                {
                    _State = SactaState.WaitingSectFinish;
                    pendingSectProc = Task.Factory.StartNew(() =>
                    {
                        SactaMsg currentSect = null;
                        do
                        {
                            lock (pendindSect)
                            {
                                if (pendindSect.Count > 0)
                                    currentSect = pendindSect.Dequeue();
                                else
                                    currentSect = null;
                            }
                            if (currentSect != null)
                            {
                                Log(true, System.Reflection.MethodBase.GetCurrentMethod().Name,
                                    String.Format("Procesando Sectorizacion {0}", ((SactaMsg.SectInfo)(currentSect.Info)).Version));

                                DateTime startingTime = DateTime.Now;

                                _State = SactaState.WaitingSectFinish;
                                _TryingSectVersion = ((SactaMsg.SectInfo)(currentSect.Info)).Version;
                                try
                                {
                                    ProcessSectorization(currentSect);
                                }
                                catch(Exception x)
                                {
                                    // TODO
                                    //if (!_Disposed)
                                    //{
                                    //    CD40.BD.SactaInfo info = new CD40.BD.SactaInfo();

                                    //    info["SectVersion"] = ((SactaMsg.SectInfo)(currentSect.Info)).Version;
                                    //    info["Resultado"] = 1;
                                    //    info["ErrorCause"] = x.Message;

                                    //    OnResultSectorizacion(info);
                                    //}
                                    Log(true, System.Reflection.MethodBase.GetCurrentMethod().Name, "Excepcion: {0}", x.Message);
                                }
                                Log(true, System.Reflection.MethodBase.GetCurrentMethod().Name,
                                    String.Format("Sectorizacion {0} procesada en {1} segundos.", 
                                    ((SactaMsg.SectInfo)(currentSect.Info)).Version,
                                    (DateTime.Now-startingTime).TotalSeconds));
                            }
                        } while (currentSect != null);
                        pendingSectProc = null;
                    });
                }
            }
            else
            {
                Log(false, System.Reflection.MethodBase.GetCurrentMethod().Name, 
                    "Petición de sectorización (Red = {0}, Versión = {1}, DESCARTADA. Ya se esta procesado...", 
                    net, ((SactaMsg.SectInfo)(sect.Info)).Version);
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        static int _LastActivityState = -1;
        static SactaState _LastState = SactaState.Stopped;
        void PeriodicTasks(object sender, ElapsedEventArgs e)
        {
            try
            {
                lock (_Sync)
                {
                    if (_ActivityState != _LastActivityState || _State != _LastState)
                    {
                        Log(false, System.Reflection.MethodBase.GetCurrentMethod().Name, "Tick {0},{1}", _ActivityState, _State);
                        _LastActivityState = _ActivityState;
                        _LastState = _State;
                    }
                    int activityState = ((uint)((DateTime.Now - _LastSactaReceived[0]).TotalMilliseconds) < _ActivityTimeOut ? 1 : 0);
                    activityState |= ((uint)((DateTime.Now - _LastSactaReceived[1]).TotalMilliseconds) < _ActivityTimeOut ? 2 : 0);

                    if (activityState != _ActivityState)
                    {
                        _ActivityState = activityState;

                        // ModuleInfo info = new ModuleInfo();
                        Dictionary<string, object> info = new Dictionary<string, object>();

                        info["SactaActivity"] = (byte)_ActivityState;
                        info["SactaAEP"] = _EndPoint[0];
                        info["SactaBEP"] = _EndPoint[1];
                        // TODO. Sirve de algo esta notificación...
                        General.AsyncSafeLaunchEvent(SactaActivityChanged, this, info);
                        Log(false, System.Reflection.MethodBase.GetCurrentMethod().Name, "SactaActivityChangedEvent => {0}", _ActivityState);
                    }

                    if (_ActivityState == 0)
                    {
                        _State = SactaState.WaitingSactaActivity;
                        foreach (var item in _SactaSPSIUsers)
                            item.Value.LastSectMsgId = -1;
                    }
                    else
                    {
                        if ((_State == SactaState.WaitingSactaActivity) ||
                            ((_State == SactaState.WaitingSectorization) &&
                            ((uint)((DateTime.Now - _BeginOfWaitForSect).TotalMilliseconds) > Config.TimeoutActividadSacta)))
                        {
                            /** */
                            foreach (var item in _SactaSPSIUsers)
                                item.Value.LastSectMsgId = -1;

                            SendInit();
                            SendSectAsk();
                            SendPresence();

                            _State = SactaState.WaitingSectorization;
                        }
                        else if ((uint)((DateTime.Now - _LastPresenceSended).TotalMilliseconds) > _PresenceInterval)
                        {
                            SendPresence();
                        }

                    }
                }
            }
            catch (Exception x)
            {
                if (!_Disposed)
                {
                }
                Log(true, System.Reflection.MethodBase.GetCurrentMethod().Name, "Excepcion: {0}", x.Message);
            }
            finally
            {
                if (!_Disposed)
                {
                    _PeriodicTasks.Enabled = true;
                }
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="msg"></param>
        /// <returns></returns>
        bool IsValid(SactaMsg msg)
        {
            Dictionary<ushort, PSIInfo> validUsers = msg.Type == SactaMsg.MsgType.Sectorization ? _SactaSPSIUsers : _SactaSPVUsers;
            return ((msg.DomainOrg == Config.sacta.Domain) && (msg.DomainDst == Config.scv.Domain) &&
                    (msg.CenterOrg == Config.sacta.Center) && (msg.CenterDst == Config.scv.Center) &&
                    (msg.UserDst == Config.scv.User) && validUsers.ContainsKey(msg.UserOrg));
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="msg"></param>
        /// <returns></returns>
        bool IsSecondSectMsg(SactaMsg msg)
        {
            PSIInfo psi = _SactaSPSIUsers[msg.UserOrg];
            if ((psi.LastSectMsgId == msg.Id) && (psi.LastSectVersion == ((SactaMsg.SectInfo)(msg.Info)).Version))
            {
                Log(false, System.Reflection.MethodBase.GetCurrentMethod().Name, "Segundo MSG Sectorizacion UserOrg={4}, {0}:{1}, {2}:{3}", psi.LastSectMsgId, msg.Id, psi.LastSectVersion, ((SactaMsg.SectInfo)(msg.Info)).Version, msg.UserOrg);
                return true;
            }
            Log(false, System.Reflection.MethodBase.GetCurrentMethod().Name, "Primer MSG Sectorizacion UserOrg={4}, {0}:{1}, {2}:{3}", psi.LastSectMsgId, msg.Id, psi.LastSectVersion, ((SactaMsg.SectInfo)(msg.Info)).Version, msg.UserOrg);
            psi.LastSectMsgId = msg.Id;
            psi.LastSectVersion = ((SactaMsg.SectInfo)(msg.Info)).Version;
            return false;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="msg"></param>
        void ProcessSectorization(SactaMsg msg)
        {
            const int Error = 1;
            StringBuilder str = new StringBuilder();
            SactaMsg.SectInfo sactaSect = (SactaMsg.SectInfo)(msg.Info);
            // CD40.BD.SactaInfo info = new CD40.BD.SactaInfo();
            var info = new SectorizationResult();

            info["SectVersion"] = sactaSect.Version;
            List<SactaMsg.SectInfo.SectorInfo> listaSectores = new List<SactaMsg.SectInfo.SectorInfo>();

            foreach (SactaMsg.SectInfo.SectorInfo sector in sactaSect.Sectors)
            {
                /** Ignoro los sectores virtuales */
                if (SactaBdt.HayQueIgnorar(UInt16.Parse(sector.SectorCode)))
                {
                    Log(false, System.Reflection.MethodBase.GetCurrentMethod().Name, "Ignorando Asignacion Virtual {0} => {1}", sector.SectorCode, sector.Ucs);
                    continue;
                }
                if (!SactaBdt.UcsInBdt((UInt16)sector.Ucs))
                {
                    info["Resultado"] = Error;
                    info["ErrorCause"] = String.Format("ERROR: TOP {0} desconocido.", sector.Ucs);
                    OnResultSectorizacion(info);
                    Log(true, System.Reflection.MethodBase.GetCurrentMethod().Name, "ERROR: Top desconocido {0}", sector.Ucs);
                    return;
                }
                listaSectores.Add(sector);
            }

            listaSectores.Sort(delegate(SactaMsg.SectInfo.SectorInfo X, SactaMsg.SectInfo.SectorInfo Y)
            {
                if (Convert.ToInt32(X.SectorCode) < Convert.ToInt32(Y.SectorCode))
                    return -1;
                if (Convert.ToInt32(X.SectorCode) > Convert.ToInt32(Y.SectorCode))
                    return 1;
                return 0;
            });

            List<int> controlSectoresRepetidos = new List<int>();
            foreach (SactaMsg.SectInfo.SectorInfo sector in listaSectores)
            {
                if (!SactaBdt.SectInBdt(UInt16.Parse(sector.SectorCode)))
                {
                    info["Resultado"] = Error;
                    info["ErrorCause"] = String.Format("ERROR: Sector {0} desconocido.", sector.SectorCode);
                    OnResultSectorizacion(info);
                    Log(true, System.Reflection.MethodBase.GetCurrentMethod().Name, "ERROR: Sector desconocido {0}", sector.SectorCode);
                    return;
                }
                if (controlSectoresRepetidos.Exists(n => n == Convert.ToInt32(sector.SectorCode)))
                {
                    info["Resultado"] = Error;
                    info["ErrorCause"] = String.Format("ERROR: Sector {0} repetido.", sector.SectorCode);
                    OnResultSectorizacion(info);
                    Log(true, System.Reflection.MethodBase.GetCurrentMethod().Name, "ERROR: Sector repetido {0}", sector.SectorCode);
                    return;
                }

                controlSectoresRepetidos.Add(Convert.ToInt32(sector.SectorCode));
                str.Append(string.Format("{0},{1};", sector.SectorCode, sector.Ucs));
            }

            // Añadir sectores de mantenimiento
            str.Append(SactaBdt.MttoSectors());

            info["SectName"] = "SACTA";
            info["SectData"] = str.ToString();

            //GeneraSectorizacionDll.Sectorization s=new GeneraSectorizacionDll.Sectorization(
            DateTime fechaActivacion = new DateTime();
            fechaActivacion = DateTime.Now;

            CD40.BD.Utilidades util = new CD40.BD.Utilidades(ConexionCD40);
            util.EventResultSectorizacion += new CD40.BD.SectorizacionEventHandler<CD40.BD.SactaInfo>(OnResultSectorizacion);
            GeneraSectorizacionDll.Sectorization sectorizacion = util.GeneraSectorizacion(info, fechaActivacion);

            try
            {
                if (sectorizacion != null)
                {
                    Ref_Service.ServiciosCD40 s = new Ref_Service.ServiciosCD40();

                    System.Configuration.Configuration webConfiguracion = System.Web.Configuration.WebConfigurationManager.OpenWebConfiguration("~");
                    string listenIp = webConfiguracion.AppSettings.Settings["OrigenIp"].Value;
                    Log(false, System.Reflection.MethodBase.GetCurrentMethod().Name, "Comunica Sectorizacion Activa idsec=SACTA, fechaActivacion: {0}", fechaActivacion.ToLocalTime());
                    if (s.ComunicaSectorizacionActiva(listenIp, IdSistema, "SACTA", ref fechaActivacion) == true)
                    {
                        Log(false, System.Reflection.MethodBase.GetCurrentMethod().Name, "Sectorizacion Implantada {0}...", fechaActivacion.ToLocalTime());
                    }
                    else
                    {
                        Log(true, System.Reflection.MethodBase.GetCurrentMethod().Name, "Error Comunica Sectorizacion Activa {0}", fechaActivacion.ToLocalTime());
                    }
                }
            }
            catch (Exception e)
            {
                System.Diagnostics.Debug.Assert(false, e.Message);
                Log(true, System.Reflection.MethodBase.GetCurrentMethod().Name, "Excepcion: {0}", e.Message);
            }

//#endif
        }

//#if !__LOCAL_TESTING__
        /// <summary>
        /// 
        /// </summary>
        /// <param name="info"></param>
        void OnResultSectorizacion(CD40.BD.SactaInfo info)
        {
            try
            {
                int result = (int)info["Resultado"];
                uint version = (uint)info["SectVersion"];
                string cause = info.ContainsKey("ErrorCause") ? (string)info["ErrorCause"] : null;

                Log(result == 1, System.Reflection.MethodBase.GetCurrentMethod().Name, "Resultado Sectorizacion: {0}", cause ?? "OK");
                lock (_Sync)
                {
                    if ((_State == SactaState.WaitingSectFinish) && (version == _TryingSectVersion))
                    {
                        _State = SactaState.SendingPresences;
                        SendSectAnswer(version, result);

                        new CD40.BD.Utilidades(ConexionCD40).CreaEventoConfiguracion("departamento", (uint)(result == 0 ? 109 : 110), new string[] { cause }, "127.0.0.1");
                    }
                    else
                    {
                        Log(true, System.Reflection.MethodBase.GetCurrentMethod().Name, "Resultado de Sectorizacion DESCARTADO State: {0}, Version: {1}, TryingVersion  {2}",
                            _State, version, _TryingSectVersion);
                    }
                }
            }
            catch (Exception x)
            {
                if (!_Disposed)
                {
                    // _Logger.ErrorException(Resources.OnSectResultError, ex);
                }
                Log(true, System.Reflection.MethodBase.GetCurrentMethod().Name, "Excepcion: {0}", x.Message);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        void SendInit()
        {
            Debug.Assert(_ActivityState != 0);
            if ((_ActivityState & 0x1) == 0x1)
                _socket.Send(_EndPoint[0], (new SactaMsg(SactaMsg.MsgType.Init, SactaMsg.InitId, 0)).Serialize());
            if ((_ActivityState & 0x2) == 0x2)
                _socket.Send(_EndPoint[1], (new SactaMsg(SactaMsg.MsgType.Init, SactaMsg.InitId, 0)).Serialize());
            Log(false, System.Reflection.MethodBase.GetCurrentMethod().Name, "Mensaje INIT enviado...");
            _SeqNum = 0;
        }
        /// <summary>
        /// 
        /// </summary>
        void SendSectAsk()
        {
            Debug.Assert(_ActivityState != 0);

            if ((_ActivityState & 0x1) == 0x1)
                _socket.Send(_EndPoint[0], (new SactaMsg(SactaMsg.MsgType.SectAsk, 0, _SeqNum)).Serialize());
            if ((_ActivityState & 0x2) == 0x2)
                _socket.Send(_EndPoint[1], (new SactaMsg(SactaMsg.MsgType.SectAsk, 0, _SeqNum)).Serialize());

            _SeqNum = _SeqNum >= 287 ? 0 : _SeqNum + 1; // (_SeqNum + 1) & 0x1FFF;

            Log(false, System.Reflection.MethodBase.GetCurrentMethod().Name, "Mensaje SECTASK enviado...");
            _BeginOfWaitForSect = DateTime.Now;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="version"></param>
        /// <param name="result"></param>
        void SendSectAnswer(uint version, int result)
        {
            Debug.Assert(_ActivityState != 0);
            if ((_ActivityState & 0x1) == 0x1)
                _socket.Send(_EndPoint[0], (new SactaMsg(SactaMsg.MsgType.SectAnwer, 0, _SeqNum, (int)version, (result == 0 ? 1 : 0))).Serialize());
            if ((_ActivityState & 0x2) == 0x2)
                _socket.Send(_EndPoint[1], (new SactaMsg(SactaMsg.MsgType.SectAnwer, 0, _SeqNum, (int)version, (result == 0 ? 1 : 0))).Serialize());

            _SeqNum = _SeqNum >= 287 ? 0 : _SeqNum + 1; // _SeqNum = (_SeqNum + 1) & 0x1FFF;

            Log(false, System.Reflection.MethodBase.GetCurrentMethod().Name, "Mensaje SectAnswer enviado...");
        }
        /// <summary>
        /// 
        /// </summary>
        void SendPresence()
        {
            Debug.Assert(_ActivityState != 0);
            if ((_ActivityState & 0x1) == 0x1)
                _socket.Send(_EndPoint[0], (new SactaMsg(SactaMsg.MsgType.Presence, 0, _SeqNum)).Serialize());
            if ((_ActivityState & 0x2) == 0x2)
                _socket.Send(_EndPoint[1], (new SactaMsg(SactaMsg.MsgType.Presence, 0, _SeqNum)).Serialize());

            _SeqNum = _SeqNum >= 287 ? 0 : _SeqNum + 1; // _SeqNum = (_SeqNum + 1) & 0x1FFF;

            Log(false, 
                System.Reflection.MethodBase.GetCurrentMethod().Name, 
                String.Format("{0}, Mensaje Presencia Enviado...", strModuleState));
            _LastPresenceSended = DateTime.Now;
        }

#endregion
        /// <summary>
        /// 
        /// </summary>
        /// <param name="from"></param>
        /// <param name="msg"></param>
        /// <param name="par"></param>        
        static void Log(bool isError, string from, string msg, params object[] par)
        {
//#if __SACTA2017__
            string message = String.Format("[{0}.{1}]: {2}", "SactaModule", from, msg);
            if (isError)
                NLog.LogManager.GetLogger("SactaModule").Error(message, par);
            else
                NLog.LogManager.GetLogger("SactaModule").Info(message, par);
//#endif
        }
    }
}
