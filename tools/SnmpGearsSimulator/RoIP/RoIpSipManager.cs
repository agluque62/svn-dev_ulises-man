using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.InteropServices;

using NLog;
using Utilities;

namespace Roip
{
    class RoIpSipManager
    {
        [DllImport("coresip", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi, ExactSpelling = true)]
        static extern int CORESIP_CallSquelch(int call, int OnOff);

        private Logger log = LogManager.GetCurrentClassLogger();
        private EventQueue _EventQueue = new EventQueue();

        /// <summary>
        /// 
        /// </summary>
        class RoIpCallInfo
        {
            public bool habilitado { get; set; }
            public List<int> active_calls { get; set; }
            public int active_call { get; set; }
            public CORESIP_CallState state { get; set; }
            public CORESIP_CallInfo info { get; set; }
            public bool squelch { get; set; }
        }
        /// <summary>
        /// 
        /// </summary>
        private Dictionary<string, RoIpCallInfo> calls = new Dictionary<string, RoIpCallInfo>();
        /// <summary>
        /// 
        /// </summary>
        public void Init()
        {
            LoadFromConfig();

            SipAgent.CallState += OnCallState;
            SipAgent.CallIncoming += OnCallIncoming;
            SipAgent.RdInfo += OnRdInfo;
            SipAgent.KaTimeout += OnKaTimeout;

            SipAgent.OptionsReceive += OnOptionsReceive;
            SipAgent.InfoReceived += OnInfoReceived;

            SipAgent.Init("AGL", RcsHfSimulator.Properties.Settings.Default.IpPropia, 5060);
        }
        /// <summary>
        /// 
        /// </summary>
        public void Start()
        {
            _EventQueue.Start();
            SipAgent.Start();
        }
        /// <summary>
        /// 
        /// </summary>
        public void End()
        {
            SipAgent.End();
            _EventQueue.Stop();
        }

        #region operaciones

        /// <summary>
        /// 
        /// </summary>
        /// <param name="user"></param>
        public void Squelch(string user)
        {
            RoIpCallInfo callInfo = calls.Where(kvp => kvp.Key == user).ToList()[0].Value;
            if (callInfo != null)
            {                
                if (callInfo.active_call != -1)
                {
                    callInfo.squelch = !callInfo.squelch;
                    CORESIP_CallSquelch(callInfo.active_call, callInfo.squelch ? 1 : 0);
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="user"></param>
        public void Habilita(string user)
        {
            if (calls.Keys.Contains(user))
            {
                RoIpCallInfo callInfo = calls[user];
                callInfo.habilitado = true;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="user"></param>
        public void Inhabilita(string user)
        {
            if (calls.Keys.Contains(user))
            {
                RoIpCallInfo callInfo = calls[user];

                /** Cuelga todas las llamadas. */
                foreach(int call in callInfo.active_calls)
                    SipAgent.HangupCall(callInfo.active_call);

                callInfo.habilitado = false;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="user"></param>
        /// <returns></returns>
        public string strEstado(string user)
        {
            if (calls.Keys.Contains(user))
            {
                RoIpCallInfo callInfo = calls[user];
                return callInfo.habilitado == false ? "-" : callInfo.active_calls.Count.ToString();
                    //callInfo.current_call != -1 ? "C" : "D";
            }
            return "?";
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public string strEstado()
        {
            StringBuilder sb = new StringBuilder();
            foreach (KeyValuePair<string, RoIpCallInfo> kpv in calls)
            {
                sb.AppendFormat("{0}: HAB={1}, EST={2}, SQH={3}\n", kpv.Key, kpv.Value.habilitado, kpv.Value.state, kpv.Value.squelch);
            }
            return sb.ToString();
        }

        #endregion

        #region private
        /// <summary>
        /// 
        /// </summary>
        private void LoadFromConfig()
        {
            RcsHfSimulator.Properties.Settings Settings = RcsHfSimulator.Properties.Settings.Default;

            calls.Clear();
            foreach (String strEqu in Settings.GearList)
            {
                String[] EquParam = strEqu.Split(';');
                calls[EquParam[0]] = new RoIpCallInfo() { active_calls = new List<int>(), state = CORESIP_CallState.CORESIP_CALL_STATE_DISCONNECTED, habilitado = true, info = new CORESIP_CallInfo(), squelch = false, active_call=-1 }; 
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

            _EventQueue.Enqueue("ChannelError", delegate()
            {
                try
                {
                    var contain = calls.Where(kvp => kvp.Value.active_calls.Contains(call)==true);
                    if (contain == null) 
                    {
                        log.Debug("OnCallState: call={0}, info.type={1}, state={2}. Trama no Relacionada...", call, info.Type, stateInfo.State);
                        return;
                    }

                    List<KeyValuePair<string, RoIpCallInfo>> callList = contain.ToList();
                    if (callList != null && callList.Count > 0)
                    {
                        RoIpCallInfo callInfo = callList[0].Value;
                        if (callInfo != null)
                        {
                            //string user = callList[0].Key;
                            //if (user == "TX2_AGL")
                            {
                                switch (stateInfo.State)
                                {
                                    case CORESIP_CallState.CORESIP_CALL_STATE_DISCONNECTED:
                                        callInfo.active_call = callInfo.active_call==call ? -1 : callInfo.active_call;
                                        callInfo.active_calls.Remove(call);
                                        log.Error("Sip sesion cerrada: {0}", callList[0].Key);
                                        break;
                                    case CORESIP_CallState.CORESIP_CALL_STATE_INCOMING:
                                    case CORESIP_CallState.CORESIP_CALL_STATE_CALLING:
                                    case CORESIP_CallState.CORESIP_CALL_STATE_CONNECTING:
                                    case CORESIP_CallState.CORESIP_CALL_STATE_EARLY:
                                    case CORESIP_CallState.CORESIP_CALL_STATE_NULL:
                                        break;
                                    case CORESIP_CallState.CORESIP_CALL_STATE_CONFIRMED:
                                        if (callInfo.active_calls.Count > 1)
                                            log.Error("OnCallState Error: Mas de una llamada entrante para {0}", callList[0].Key);
                                        log.Info("Sip sesion establecida con: {0}", callList[0].Key);
                                        break;
                                }
                            }
                        }
                        callInfo.state = stateInfo.State;
                    }
                }
                catch (Exception x)
                {
                    log.Error("OnCallState Exception: {0}", x.Message);
                }
            });
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
            log.Debug("OnCallIncoming: call={0}, call2replace={1}, info.type={2}, destino={3}", call, call2replace, info.Type, inInfo.DstId);
            _EventQueue.Enqueue("ChannelError", delegate()
            {
                try
                {
                    if (calls.Keys.Contains(inInfo.DstId))
                    {
                        RoIpCallInfo callInfo = calls.Where(kvp => kvp.Key == inInfo.DstId).ToList()[0].Value;
                        if (callInfo != null)
                        {
                            if (callInfo.habilitado)
                            {
                                callInfo.active_call = call;
                                callInfo.active_calls.Add(call);
                                callInfo.info = info;
                                SipAgent.AnswerCall(call, SipAgent.SIP_OK);
                                log.Info("Sip sesion Entrante desde: {0}@{1}", inInfo.SrcId, inInfo.SrcIp);
                            }
                        }
                    }
                }
                catch (Exception x)
                {
                    log.Error("OnCallIncoming Exception: {0}", x.Message);
                }
            });
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="call"></param>
        private void OnKaTimeout(int call)
        {
            log.Debug("OnKaTimeout: call={0}", call);
            _EventQueue.Enqueue("ChannelError", delegate()
            {
                try
                {
                    SipAgent.HangupCall(call);
                    log.Error("Cerrando Llamada (OnKaTimeout)");
                    //List<KeyValuePair<string, RoIpCallInfo>> callList = calls.Where(kvp => kvp.Value.active_calls == call).ToList();
                    //if (callList != null && callList.Count > 0)
                    //{
                    //    RoIpCallInfo callInfo = callList[0].Value;
                    //    {
                    //        if (callInfo != null)
                    //        {
                    //            SipAgent.HangupCall(call);
                    //            log.Error("Cerrando Llamada (OnKaTimeout): {0}", callList[0].Key);
                    //        }
                    //    }
                    //}
                }
                catch (Exception x)
                {
                    log.Error("OnKaTimeout Exception: {0}", x.Message);
                }
            });
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="call"></param>
        /// <param name="info"></param>
        private void OnRdInfo(int call, CORESIP_RdInfo info)
        {
            _EventQueue.Enqueue("ChannelError", delegate()
            {
                try
                {
                    //List<KeyValuePair<string, RoIpCallInfo>> callList = calls.Where(kvp => kvp.Value.call == call).ToList();
                    //if (callList != null && callList.Count > 0)
                    //{
                    //    string user = callList[0].Key;
                    //    if (user == "TX2_AGL")
                    //        return;
                    //}

                    log.Debug("OnRdInfo: call={0}, ptt-id={1}, ptt={2}", call, info.PttId, info.PttType);

                    switch (info.PttType)
                    {
                        case CORESIP_PttType.CORESIP_PTT_NORMAL:
                            SipAgent.PttOn(call, info.PttId, CORESIP_PttType.CORESIP_PTT_NORMAL);
                            break;
                        case CORESIP_PttType.CORESIP_PTT_PRIORITY:
                            SipAgent.PttOn(call, info.PttId, CORESIP_PttType.CORESIP_PTT_PRIORITY);
                            break;
                        case CORESIP_PttType.CORESIP_PTT_EMERGENCY:
                            SipAgent.PttOn(call, info.PttId, CORESIP_PttType.CORESIP_PTT_EMERGENCY);
                            break;
                        case CORESIP_PttType.CORESIP_PTT_COUPLING:
                            SipAgent.PttOn(call, info.PttId, CORESIP_PttType.CORESIP_PTT_COUPLING);
                            break;
                        case CORESIP_PttType.CORESIP_PTT_OFF:
                            SipAgent.PttOff(call);
                            break;
                    }
                }
                catch (Exception x)
                {
                    log.Error("OnRdInfo Exception: {0}", x.Message);
                }
            });
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="fromUri"></param>
        private void OnOptionsReceive(string fromUri)
        {
            log.Debug("OnOptiosReceive. from={0}", fromUri);
            _EventQueue.Enqueue("ChannelError", delegate()
            {
                try
                {
                }
                catch (Exception x)
                {
                    log.Error("OnOptionsReceive Exception: {0}", x.Message);
                }
            });
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
            _EventQueue.Enqueue("ChannelError", delegate()
            {
                try
                {
                }
                catch (Exception x)
                {
                    log.Error("OnInfoReceived Exception: {0}", x.Message);
                }
            });
        }
        #endregion

    }
}
