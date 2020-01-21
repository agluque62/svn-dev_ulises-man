using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.InteropServices;

using NLog;

using CoreSipNet;

namespace RadioVoipSim
{
    /// <summary>
    /// 
    /// </summary>
    public class ControlledSipAgent
    {
        public static void Init(CallIncomingCb onCallIncoming, 
                CallStateCb onCallState, RdInfoCb onRdInfo, KaTimeoutCb onKaTimeout, 
                OptionsReceiveCb onOptionsReceive, InfoReceivedCb onInfoReceived)
        {
            try
            {
                SipAgentNet.CallState += onCallState;
                SipAgentNet.CallIncoming += onCallIncoming;
                SipAgentNet.RdInfo += onRdInfo;
                SipAgentNet.KaTimeout += onKaTimeout;
                SipAgentNet.OptionsReceive += onOptionsReceive;
                SipAgentNet.InfoReceived += onInfoReceived;

                SipAgentNet.Log += (p1, p2, p3) =>
                {
                    if (p1 <= Properties.Settings.Default.CoresipLogLevel)
                    {
                        LogManager.GetLogger("ControlledSipAgent").
                            Debug("CoreSipNet Log Level {0}: {1}", p1, p2);
                    }
                };

                SipAgentNet.Init(settings, "ROIPSIM",
                    Properties.Settings.Default.IpBase,
                    Properties.Settings.Default.SipPort);
            }
            catch (Exception x)
            {
                LogException("Init Exception", x);
            }
        }
        public static void Start()
        {
            try
            {
                SipAgentNet.Start();
            }
            catch (Exception x)
            {
                LogException("Start Exception", x);
            }
        }
        public static void End()
        {
            try
            {
                SipAgentNet.End();
            }
            catch (Exception x)
            {
                LogException("End Exception", x);
            }
        }
        public static void AnswerCall(int callid, int code)
        {
            try
            {
                SipAgentNet.AnswerCall(callid, code);
            }
            catch (Exception x)
            {
                LogException("AnswerCall Exception", x);
            }
        }
        public static void HangupCall(int callid, int code)
        {
            try
            {
                SipAgentNet.HangupCall(callid, code);
            }
            catch (Exception x)
            {
                LogException("AnswerCall Exception", x);
            }
        }
        public static void PttSet(int callid, CORESIP_PttType tipo, ushort pttId = 0, int PttMute = 0)
        {
            try
            {
                if (tipo == CORESIP_PttType.CORESIP_PTT_OFF)
                    SipAgentNet.PttOff(callid);
                else
                    SipAgentNet.PttOn(callid, pttId, tipo, PttMute);
            }
            catch (Exception x)
            {
                LogException("PttSet Exception", x);
            }
        }
        public static void SquelchSet(int callid, bool valor, 
            CORESIP_PttType tipoptt = CORESIP_PttType.CORESIP_PTT_OFF, ushort pttId = 0, uint PttMute = 0)
        {
            try
            {
                uint RssiQidx = Properties.Settings.Default.RssiQidx;
                SipAgentNet.SqhOnOffSet(callid, valor, RssiQidx, tipoptt, pttId, PttMute);
            }
            catch (Exception x)
            {
                LogException("SquelchSet Exception", x);
            }
        }
        public static int CreateWavPlayer(string file, bool loop)
        {
            int wavplayer = -1;
            try
            {
                wavplayer = SipAgentNet.CreateWavPlayer(file, loop);
            }
            catch (Exception x)
            {
                LogException("CreateWavPlayer Exception", x);
                wavplayer = -1;
            }
            return wavplayer;
        }
        public static void DestroyWavPlayer(int wavplayer)
        {
            try
            {
                SipAgentNet.DestroyWavPlayer(wavplayer);
            }
            catch (Exception x)
            {
                LogException("DestroyWavPlayer Exception", x);
            }
        }
        public static void MixerLink(int srcId, int dstId)
        {
            try
            {
                SipAgentNet.MixerLink(srcId, dstId);
            }
            catch (Exception x)
            {
                LogException("MixerLink Exception", x);
            }
        }
        public static void MixerUnlink(int srcId, int dstId)
        {
            try
            {
                SipAgentNet.MixerUnlink(srcId, dstId);
            }
            catch (Exception x)
            {
                LogException("MixerUnlink Exception", x);
            }
        }
        public static string SendOptionsMsg(string OptionsUri)
        {
            try
            {
                string callid = "";
                SipAgentNet.SendOptionsMsg(OptionsUri, out callid);
                return callid;
            }
            catch (Exception x)
            {
                LogException("SendOptionsMsg Exception", x);
            }
            return "";
        }

        #region Protegidas.
        protected static void LogException(string msg, Exception x, params object[] par)
        {
            LogManager.GetCurrentClassLogger().Error(msg, x);
        }
        protected static SipAgentNetSettings settings = new SipAgentNetSettings()
        {
            Default = new SipAgentNetSettings.DefaultSettings()
            {
                DefaultCodec = "PCMA",
                DefaultDelayBufPframes = 3,
                DefaultJBufPframes = 4,
                SndSamplingRate = 8000,
                RxLevel = 1,
                TxLevel = 1,
                SipLogLevel = Properties.Settings.Default.CoresipLogLevel,
                TsxTout = 400,
                InvProceedingIaTout = 1000,
                InvProceedingMonitoringTout = 30000,
                InvProceedingDiaTout = 30000,
                InvProceedingRdTout = 1000,
                KAPeriod = 200,
                KAMultiplier = 10
            }
        };
        #endregion

    }
    /// <summary>
    /// 
    /// </summary>
    public class RoIpSipManager
    {
        /// <summary>
        /// 
        /// </summary>
        public class RoIpCallInfo
        {
            public string name { get; set; }
            public bool isTx { get; set; }
            public bool isRx { get; set; }

            public bool habilitado { get; set; }
            public int call { get; set; }
            public CORESIP_CallState state { get; set; }
            public CORESIP_CallInfo info { get; set; }
            public bool LocalSquelch { get; set; }
            public bool Squelch { get; set; }
            public bool ptt { get; set; }
            public CORESIP_PttType PttType { get; set; }
            public ushort ptt_id { get; set; }
            public uint ptt_mute { get; set; }
            public bool error {get;set;}
            public string HalfDuplexRx { get; set; }
            public int SquWavPlayer { get; set; }

            public ConsoleColor ColorEstado
            {
                get
                {
                    return habilitado == false ? ConsoleColor.Gray :
                        state != CORESIP_CallState.CORESIP_CALL_STATE_CONFIRMED ? ConsoleColor.White :
                        error ? ConsoleColor.Red : ConsoleColor.Green;
                }
            }
        }

        #region Public
        /// <summary>
        /// 
        /// </summary>
        public void Init()
        {
            LoadConfig();
            ControlledSipAgent.Init(OnCallIncoming, OnCallState, OnRdInfo, OnKaTimeout, OnOptionsReceive, OnInfoReceived);
        }
        /// <summary>
        /// 
        /// </summary>
        public void Start()
        {
            ControlledSipAgent.Start();
        }
        /// <summary>
        /// 
        /// </summary>
        public void End()
        {
            ControlledSipAgent.End();
        }
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public string strEstado()
        {
            StringBuilder sb = new StringBuilder();
            cfg.ForEach(item =>
            {
                sb.AppendFormat("{0,10}: IDC={4,10}, HAB={1,6}, EST={2,20}, SQH={3,6}, PTT={5,6}\n",
                    item.name, item.habilitado, item.state, item.Squelch, item.call, item.ptt);
            });
            return sb.ToString();
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="user"></param>
        public void SquelchSwitch(string user)
        {
            var item = cfg.Where(i => i.name == user).FirstOrDefault();
            if (item != null)
            {
                if (item.isTx && !item.isRx)
                {
                    log.Debug("SquelchSwitch on {0} Error. It is not Rx.", item.name);
                    return;
                }

                item.LocalSquelch = !item.LocalSquelch;
                item.Squelch = item.LocalSquelch;

                if (item.call != -1)
                {
                    //item.LocalSquelch = !item.LocalSquelch;
                    //item.Squelch = item.LocalSquelch;

                    if (item.Squelch)
                    {
                        item.SquWavPlayer = ControlledSipAgent.CreateWavPlayer(Properties.Settings.Default.FileWavPlayer, true);
                        if (item.SquWavPlayer > 0)
                            ControlledSipAgent.MixerLink(item.SquWavPlayer, item.call);
                    }
                    
                    ControlledSipAgent.SquelchSet(item.call, item.LocalSquelch);

                    if (!item.Squelch)
                    {
                        if (item.SquWavPlayer > 0)
                        {
                            ControlledSipAgent.MixerUnlink(item.SquWavPlayer, item.call);
                            SipAgentNet.DestroyWavPlayer(item.SquWavPlayer);
                        }
                        item.SquWavPlayer = -1;
                    }

                    log.Debug("SQH on {0} => {1}", item.name, item.LocalSquelch ? "ON" : "OFF");
                }
                else
                {
                    log.Debug("SquelchSwitch on {0} Error. Call not established.", item.name);
                }
            }
            else
            {
                log.Debug("SquelchSwitch on {0} Error. Item not found.", item.name);
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="user"></param>
        public void EnableSwitch(string user)
        {
            var item = cfg.Where(i => i.name == user).FirstOrDefault();
            if (item != null)
            {
                if (item.call != -1)
                {
                    ControlledSipAgent.HangupCall(item.call, SipAgentNet.SIP_OK);
                }
                item.habilitado = !item.habilitado;
                log.Debug("EnableSwitch on {0} => {1}", item.name, item.habilitado ? "Enabled" : "Disabled");
            }
            else
            {
                log.Debug("EnableSwitch on {0} Error. Item not found.", item.name);
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="user"></param>
        public void ErrorSwitch(string user)
        {
            var item = cfg.Where(i => i.name == user).FirstOrDefault();
            if (item != null)
            {
                item.error = !item.error;
                log.Debug("ErrorSwitch on {0} => {1}", item.name, item.error ? "On Error" : "On no error");
            }
            else
            {
                log.Debug("ErrorSwitch on {0} Error. Item not found.", item.name);
            }
        }
        /// <summary>
        /// 
        /// </summary>
        public List<RoIpCallInfo> RadioCfg
        {
            get { return cfg; }
        }
        /// <summary>
        /// 
        /// </summary>
        private object locker = new object();
        private bool _EventRefresh = false;
        public bool EventRefresh {
            get
            {
                lock (locker)
                {
                    return _EventRefresh;
                }
            }
            set
            {
                lock (locker)
                {
                    _EventRefresh = value;
                }
            }
        }
        #endregion

        #region Callbacks
        /// <summary>
        /// 
        /// </summary>
        /// <param name="call"></param>
        /// <param name="info"></param>
        /// <param name="stateInfo"></param>
        private void OnCallState(int call, CORESIP_CallInfo info, CORESIP_CallStateInfo stateInfo)
        {
            log.Debug("OnCallState: call={0}, info.type={1}, state={2}", call, info.Type, stateInfo.State);
            var item = cfg.Where(i => i.call == call).FirstOrDefault();
            if (item != null)
            {
                switch (stateInfo.State)
                {
                    case CORESIP_CallState.CORESIP_CALL_STATE_DISCONNECTED:
                        log.Debug("callid={0}, Disconnected.", call);
                        item.call = -1;
                        break;
                    case CORESIP_CallState.CORESIP_CALL_STATE_CONFIRMED:

                        item.Squelch = item.LocalSquelch;

                        if (item.Squelch)
                        {
                            item.SquWavPlayer = ControlledSipAgent.CreateWavPlayer(Properties.Settings.Default.FileWavPlayer, true);
                            if (item.SquWavPlayer > 0)
                                ControlledSipAgent.MixerLink(item.SquWavPlayer, item.call);
                        }
                    
                        ControlledSipAgent.SquelchSet(item.call, item.LocalSquelch);

                        if (!item.Squelch)
                        {
                            if (item.SquWavPlayer > 0)
                            {
                                ControlledSipAgent.MixerUnlink(item.SquWavPlayer, item.call);
                                SipAgentNet.DestroyWavPlayer(item.SquWavPlayer);
                            }
                            item.SquWavPlayer = -1;
                        }
                        
                        log.Debug("callid={0}, Connected.", call);
                        break;
                    case CORESIP_CallState.CORESIP_CALL_STATE_INCOMING:
                    case CORESIP_CallState.CORESIP_CALL_STATE_CALLING:
                    case CORESIP_CallState.CORESIP_CALL_STATE_CONNECTING:
                    case CORESIP_CallState.CORESIP_CALL_STATE_EARLY:
                    case CORESIP_CallState.CORESIP_CALL_STATE_NULL:
                        log.Debug("callid={0}, {1}.", call, stateInfo.State);
                        break;
                }
                item.info = info;
                item.state = stateInfo.State;

                EventRefresh = true;
            }
            else
            {
                log.Debug("OnCallState Error: Item not found, callid={0}", call);
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="call"></param>
        /// <param name="call2replace"></param>
        /// <param name="info"></param>
        /// <param name="inInfo"></param>
        private void OnCallIncoming(int call, int call2replace, CORESIP_CallInfo info, CORESIP_CallInInfo inInfo)
        {
            log.Debug("OnCallIncoming: call={0}, call2replace={1}, info.type={2}, destino={3}", 
                call, call2replace, info.Type, inInfo.DstId);
            var item = cfg.Where(i => i.name == inInfo.DstId).FirstOrDefault();
            if (item != null)
            {
                if (item.habilitado == true)
                {
                    item.call = call;
                    item.info = info;
                    ControlledSipAgent.AnswerCall(call, SipAgentNet.SIP_OK);
                    log.Debug("CallIncoming on {0} accepted.", inInfo.DstId);
                }
                else
                {
                    ControlledSipAgent.HangupCall(call, SipAgentNet.SIP_TEMPORARILY_UNAVAILABLE);
                    log.Debug("CallIncoming on {0} rejected. Temporarily Unavailable.", inInfo.DstId);
                }
            }
            else
            {
                log.Debug("OnCallIncoming Error: {0}. Not found.", inInfo.DstId);
                ControlledSipAgent.HangupCall(call, SipAgentNet.SIP_NOT_FOUND);
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="call"></param>
        private void OnKaTimeout(int call)
        {
            log.Debug("OnKaTimeout: call={0}", call);

            var item = cfg.Where(i => i.call == call).FirstOrDefault();
            if (item != null)
            {
                item.call = -1;
                ControlledSipAgent.HangupCall(call, SipAgentNet.SIP_ERROR);
                log.Debug("OnKaTimeout on {1}. Hanging... callid={0}", call, item.name);
            }
            else
            {
                log.Debug("OnKaTimeout Error: Item not found, callid={0}", call);
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="call"></param>
        /// <param name="info"></param>
        private void OnRdInfo(int call, CORESIP_RdInfo info)
        {
            log.Debug("OnRdInfo: call={0}, ptt-id={1}, ptt={2}, sqh={3} PttMute={4}",
                call, info.PttId, info.PttType, info.Squelch, info.PttMute);

            var item = cfg.Where(i => i.call == call).FirstOrDefault();
            if (item != null)
            {
                switch (info.PttType)
                {
                    case CORESIP_PttType.CORESIP_PTT_NORMAL:
                    case CORESIP_PttType.CORESIP_PTT_PRIORITY:
                    case CORESIP_PttType.CORESIP_PTT_EMERGENCY:
                    case CORESIP_PttType.CORESIP_PTT_COUPLING:
                        if (item.ptt == false)
                        {
                            if (item.isTx == true && item.error == false)
                            {
                                ControlledSipAgent.PttSet(call, info.PttType, info.PttId, info.PttMute);
                                if (/*info.PttType != CORESIP_PttType.CORESIP_PTT_COUPLING && */item.HalfDuplexRx != "")
                                {
                                    if (info.PttMute == 0)
                                        HalfDuplexSquelchSet(item, true); 
                                    else
                                        HalfDuplexSquelchSet(item, false); 
                                }
                            }
                            else if (item.isRx == true && item.error == false)
                            {
                                //ControlledSipAgent.PttSet(call, info.PttType, info.PttId, info.PttMute);
                                uint pm = (info.PttMute == 0) ? (uint) 0:(uint) 1;
                                ControlledSipAgent.SquelchSet(call, item.Squelch, info.PttType, info.PttId, pm); 
                            }
                            item.ptt = true;
                            item.PttType = info.PttType;
                            item.ptt_id = info.PttId;
                            item.ptt_mute = (info.PttMute == 0) ? (uint) 0 : (uint) 1;
                            EventRefresh = true;
                            log.Debug("RdInfo on {0}. PTTType = {1}", item.name, info.PttType);
                        }
                        break;

                    case CORESIP_PttType.CORESIP_PTT_OFF:
                        if (item.ptt == true)
                        {
                            if (item.isTx == true && item.error == false)
                            {
                                ControlledSipAgent.PttSet(call, CORESIP_PttType.CORESIP_PTT_OFF);
                                if (/*info.PttType != CORESIP_PttType.CORESIP_PTT_OFF && */item.HalfDuplexRx != "")
                                {
                                    HalfDuplexSquelchSet(item, false);    
                                }
                            }
                            else if (item.isRx == true && item.error == false)
                            {
                                //ControlledSipAgent.PttSet(call, info.PttType, info.PttId, info.PttMute);
                                uint pm = (info.PttMute == 0) ? (uint)0 : (uint)1;
                                ControlledSipAgent.SquelchSet(call, item.Squelch, info.PttType, info.PttId, pm); 
                            }
                            item.ptt = false;
                            item.PttType = info.PttType;
                            item.ptt_id = info.PttId;
                            item.ptt_mute = (info.PttMute == 0) ? (uint)0 : (uint)1;
                            EventRefresh = true;
                            log.Debug("RdInfo on {0}. PTTType = {1}", item.name, info.PttType);
                        }
                        break;
                }
            }
            else
            {
                log.Debug("OnRdInfo Error: Item not found, callid={0}", call);
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="fromUri"></param>
        private void OnOptionsReceive(string fromUri/*, string callid, int statusCodem, string supported, string allow*/)
        {
            log.Debug("OnOptiosReceive. from={0}", fromUri);
        }        
        /// <summary>
        /// 
        /// </summary>
        /// <param name="call"></param>
        /// <param name="info"></param>
        /// <param name="lenInfo"></param>
        private void OnInfoReceived(int call, string info, uint lenInfo)
        {
            log.Debug("OnInfoReceived. call={0}, info={1}", call, info);
        }        
        #endregion

        #region Private
        /// <summary>
        /// 
        /// </summary>
        /// <param name="equ"></param>
        /// <param name="sqh"></param>
        private void HalfDuplexSquelchSet(RoIpCallInfo equ, bool sqh)
        {
            var item = cfg.Where(i => i.name == equ.HalfDuplexRx).FirstOrDefault();
            if (item != null)
            {
                if (item.call != -1)
                {
                    Task.Run(() =>
                    {
                        System.Threading.Thread.Sleep(sqh==false ? Properties.Settings.Default.SqhQueueOff : 
                            Properties.Settings.Default.SqhQueueOn);
                        item.Squelch = sqh==false ? item.LocalSquelch : sqh;
                        if (sqh) ControlledSipAgent.MixerLink(equ.call, item.call);
                        ControlledSipAgent.SquelchSet(item.call, item.Squelch, item.PttType, item.ptt_id, item.ptt_mute); //Al receptor le activamos el mismo PTT
                        EventRefresh = true;
                    });

                }
            }
        }
        /// <summary>
        /// 
        /// </summary>
        private void LoadConfig()
        {
            cfg.Clear();
            foreach (string scfg in Properties.Settings.Default.config)
            {
                var data = scfg.Split('#');
                if (data.Length > 1)
                {
                    cfg.Add(new RoIpCallInfo()
                    {
                        name = data[0],
                        isTx = data[1]=="1" ? true : false,
                        isRx = (data[1]=="0" || (data[0].Equals(data[2]))) ? true : false,
                        HalfDuplexRx = data.Length>2 ? data[2] : "",
                        habilitado = true,
                        call = -1,
                        state = new CORESIP_CallState(),
                        info = new CORESIP_CallInfo(),
                        ptt = false,
                        LocalSquelch = false,
                        Squelch = false,
                        error = false,
                        SquWavPlayer = -1
                    });
                }
            }
        }
        /// <summary>
        /// 
        /// </summary>
        private Logger log = LogManager.GetCurrentClassLogger();
        private List<RoIpCallInfo> cfg = new List<RoIpCallInfo>();
        #endregion


    }
}
