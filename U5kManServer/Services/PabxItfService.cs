#define _SUBSCRIBE_CFG_
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Runtime.InteropServices;
using System.Net;
using System.Net.Sockets;
using System.Net.NetworkInformation;
using System.Timers;

using WebSocket4Net;
using Newtonsoft.Json;

using Utilities;
using NucleoGeneric;
using System.Threading.Tasks;

namespace U5kManServer
{
    public enum ServiceStatus { Running, Stopped, Disabled }

    /// <summary>
    /// 
    /// </summary>
    public class PabxItfService : NucleoGeneric.NGThread //BaseCode
    {
        public override void DoWork()
        {
            throw new NotImplementedException();
        }
        public override void LocalDispose()
        {
            throw new NotImplementedException();
        }
        public PabxItfService()
        {
        }

        /// <summary>
        /// 
        /// </summary>
        protected bool IsOperative { get => U5kManService._Master == true && HayPbx; }
        /// <summary>
        /// 
        /// </summary>
        public new string Name { get => "PabxItfService"; }
        /// <summary>
        /// 
        /// </summary>
        public ServiceStatus Status { get => _Status; }
        /// <summary>
        /// 
        /// </summary>
        public override bool Start()
        {
            try
            {
                LogInfo<PabxItfService>(String.Format("{0}: Iniciando Servicio...", Name));

                GlobalServices.GetWriteAccess((data) =>
                {
                    data.STDG.stdPabx.Estado = std.NoInfo;

                    PbxIp = U5kManService.PbxEndpoint == null ? "none" : U5kManService.PbxEndpoint.Address.ToString();
                    HayPbx = U5kManService.PbxEndpoint != null;
                    _Status = ServiceStatus.Running;
                    _WorkingThread.Start();

                    _TimerPbax = new Timer();
                    _TimerPabxSimulada = new Timer(5000);

                    // Inicializar WebSocket. 
                    if (Properties.u5kManServer.Default.PabxSimulada)
                    {
                        _TimerPabxSimulada.AutoReset = false;
                        _TimerPabxSimulada.Elapsed += OnTimePabxSimuladaElapsed;
                        _TimerPabxSimulada.Enabled = true;
                    }
                    else
                    {
                        PabxUrl = "ws://" + PbxIp + ":" + Properties.u5kManServer.Default.PabxWsPort +
                            "/pbx/ws?login_user=sa&login_password=" + Properties.u5kManServer.Default.PabxSaPwd + "&user=*&registered=True&status=True&line=*";
                        _pabxws = new WebSocket(PabxUrl);
                        _pabxws.Opened += new EventHandler(Websocket_Opened);
                        _pabxws.Error += new EventHandler<SuperSocket.ClientEngine.ErrorEventArgs>(Websocket_Error);
                        _pabxws.Closed += new EventHandler(Websocket_Closed);
                        _pabxws.MessageReceived += new EventHandler<MessageReceivedEventArgs>(Websocket_MessageReceived);
                        _pabxStatus = EPabxStatus.epsDesconectado;
                    }

                    _TimerPbax.Interval = 5000;
                    _TimerPbax.AutoReset = false;
                    _TimerPbax.Elapsed += OnTimePabxElapsed;

                    Init();
                    LogInfo<PabxItfService>(Name + ": Servicio Iniciado.");
                    base.Start();   // salir = false; // Compatibilidad con IsRunning
                }, false);
                return true;
            }
            catch (Exception ex)
            {
                Stop(TimeSpan.FromSeconds(5));
                LogException<PabxItfService>("", ex);
            }
            return false;
        }
        /// <summary>
        /// 
        /// </summary>
        public override void Stop(TimeSpan timeout)
        {
            Action stopbase = () => { base.Stop(timeout); };
            List<Tuple<bool, Action>> ConditionalStopActions = new List<Tuple<bool, Action>>()
            {
                new Tuple<bool, Action>(_Status == ServiceStatus.Running, Dispose),
                new Tuple<bool, Action>(true, _WorkingThread.Stop),
                new Tuple<bool, Action>(true, stopbase),
            };

            LogInfo<PabxItfService>("Iniciando parada servicio.");

            ConditionalStopActions.ForEach(action =>
            {
                try
                {
                    if (action.Item1)
                        action.Item2();
                }
                catch (Exception x)
                {
                    LogException<PabxItfService>("Stoping Exception", x);
                }
            });
            _Status = ServiceStatus.Stopped;
            LogInfo<PabxItfService>("Servicio Detenido.");
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="cb"></param>
        public override void StopAsync(Action<Task> cb)
        {
            Stop(TimeSpan.FromSeconds(5));
        }
        /// <summary>
        /// 
        /// </summary>
        public string PabxUrl { get; set; }

        #region Formatos de Tablas..
        /// <summary>
        /// 
        /// </summary>
        class PabxParamInfo
        {
            // Evento Register
            public string Registered { get; set; }
            public string User { get; set; }
            // Evento Status,
            public long Time { get; set; }
            public string Other_number { get; set; }
            public string Status { get; set; }
            // public string user { get; set; }
        };
        class PabxEvent
        {
            public string Jsonrpc { get; set; }
            public string Method { get; set; }
            public PabxParamInfo Parametros { get; set; }
        };
        #endregion

        #region Private Members
        /// <summary>
        /// 
        /// </summary>
        enum EPabxStatus { epsDesconectado, epsConectando, epsConectado };
        /// <summary>
        /// 
        /// </summary>
        //private bool _Master = false;
        private EPabxStatus _pabxStatus = EPabxStatus.epsDesconectado;
        /// <summary>
        /// 
        /// </summary>
        private ServiceStatus _Status = ServiceStatus.Stopped;
        /// <summary>
        /// 
        /// </summary>
        private EventQueue _WorkingThread = new EventQueue();
        /// <summary>
        /// 
        /// </summary>
        private WebSocket _pabxws = null;
        /// <summary>
        /// 
        /// </summary>
        private Timer _TimerPbax = null;
        private Timer _TimerPabxSimulada = null;

        private bool HayPbx { get; set; }
        private string PbxIp { get; set; }
        private string InfoString { get => String.Format("HayPbx={0}, PbxUrl={1}, Estado={2}", HayPbx, PabxUrl, _pabxStatus); }
        #endregion

        #region Callbacks
        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Websocket_Opened(object sender, EventArgs e)
        {
            U5kGenericos.TraceCurrentThread(this.GetType().Name + " websocket_opened");
            if (IsOperative)
            {
            }
            LogTrace<PabxItfService>("WebSocket Abierto en " + PabxUrl);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Websocket_Error(object sender, SuperSocket.ClientEngine.ErrorEventArgs e)
        {
            U5kGenericos.TraceCurrentThread(this.GetType().Name + " websocket_error");
            if (IsOperative)
            {
            }
            LogWarn<PabxItfService>("WebSocketError en " + PabxUrl + ": " + e.Exception.Message);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Websocket_Closed(object sender, EventArgs e)
        {
            U5kGenericos.TraceCurrentThread(this.GetType().Name + " websocket_closed");

            _WorkingThread.Enqueue("websocket_Closed", delegate ()
            {
                U5kGenericos.TraceCurrentThread(this.GetType().Name + " websocket_closed enqueue");

                GlobalServices.GetWriteAccess((data) =>
                {
                    U5KStdGeneral stdg = data.STDG;
                    try
                    {
                        if (IsOperative)
                        {
                            _pabxStatus = EPabxStatus.epsDesconectado;

                            if (stdg.stdPabx.Estado != std.NoInfo)
                                RecordEvent<PabxItfService>(DateTime.Now, U5kBaseDatos.eIncidencias.IGRL_U5KI_SERVICE_ERROR, U5kBaseDatos.eTiposInci.TEH_SISTEMA, "SPV",
                                    Params(idiomas.strings.PBX_Desconectada/*"PBX Desconectada"*/, "", "", ""));

                            stdg.stdPabx.Estado = std.NoInfo;
                            // stdg.stdPabx.name = idiomas.strings.PBX_Desconocida/*"Desconocido"*/;

                            /* Desregistrar. */
                            List<Uv5kManDestinosPabx.DestinoPabx> stdpbxs = data.STDPBXS;
                            stdpbxs.ForEach(d => d.Estado = std.NoInfo);
                        }
                    }
                    catch (Exception x)
                    {
                        LogException<PabxItfService>("", x);
                    }
                });
            });
            LogInfo<PabxItfService>("WebSocket Cerrado en " + PabxUrl);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Websocket_MessageReceived(object sender, MessageReceivedEventArgs e)
        {
            U5kGenericos.TraceCurrentThread(this.GetType().Name + " websocket_msg");

            _WorkingThread.Enqueue("websocket_MessageReceived", delegate ()
            {
                U5kGenericos.TraceCurrentThread(this.GetType().Name + " websocket_msg enqueue");
                // Como este evento puede tocar la tabla de estado, adquiero el acceso.
                GlobalServices.GetWriteAccess((data) =>
                {
                    try
                    {
                        if (IsOperative)
                        {
                            string msg = e.Message.Replace("params", "parametros");

                            if (msg.StartsWith("{"))
                            {
                                PabxEvent _evento = JsonConvert.DeserializeObject<PabxEvent>(msg);
                                ProcessEvent(data, _evento);
                            }
                            else if (msg.StartsWith("["))
                            {
                                PabxEvent[] _eventos = JsonConvert.DeserializeObject<PabxEvent[]>(msg);
                                foreach (PabxEvent _evento in _eventos)
                                {
                                    ProcessEvent(data, _evento);
                                }
                            }
                        }
                    }
                    catch (Exception x)
                    {
                        LogException<PabxItfService>("", x);
                    }
                });
            });

            LogTrace<PabxItfService>(String.Format("WebSocket en {0}: Mensaje Recibido: {1}", PabxUrl, e.Message));
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnTimePabxElapsed(object sender, ElapsedEventArgs e)
        {
            U5kGenericos.TraceCurrentThread(this.GetType().Name + " TimePbxElapsed");

            _WorkingThread.Enqueue("OnTimePabxElapsed", delegate ()
            {
                U5kGenericos.TraceCurrentThread(this.GetType().Name + " TimePbxElapsed enqueue");
                ConfigCultureSet();
                try
                {
                    if (IsOperative)
                    {
                        if (Properties.u5kManServer.Default.PabxSimulada == false)
                        {
                            /** 20181114. Supervisa los cambios de configuracion */
                            ChangeConfigSpv(() =>
                            {
                                GlobalServices.GetWriteAccess((gdata) =>
                                {
                                    gdata.STDG.stdPabx.Estado = std.NoInfo;
                                });

                                if (_pabxStatus != EPabxStatus.epsDesconectado)
                                {
                                    try
                                    {
                                        _pabxws.Close();
                                    }
                                    finally
                                    {
                                    }
                                }

                                PbxIp = U5kManService.PbxEndpoint == null ? "none" : U5kManService.PbxEndpoint.Address.ToString();
                                PabxUrl = "ws://" + PbxIp + ":" + Properties.u5kManServer.Default.PabxWsPort +
                                    "/pbx/ws?login_user=sa&login_password=" + Properties.u5kManServer.Default.PabxSaPwd + "&user=*&registered=True&status=True&line=*";
                                HayPbx = U5kManService.PbxEndpoint != null;
                                _pabxws = new WebSocket(PabxUrl);
                                _pabxws.Opened += new EventHandler(Websocket_Opened);
                                _pabxws.Error += new EventHandler<SuperSocket.ClientEngine.ErrorEventArgs>(Websocket_Error);
                                _pabxws.Closed += new EventHandler(Websocket_Closed);
                                _pabxws.MessageReceived += new EventHandler<MessageReceivedEventArgs>(Websocket_MessageReceived);
                                _pabxStatus = EPabxStatus.epsDesconectado;

                                LogInfo<PabxItfService>(Name + ": Servicio Reinicializado por cambio de configuracion.");
                            });

                            switch (_pabxStatus)
                            {
                                case EPabxStatus.epsDesconectado:
                                    if (Ping(PbxIp, _pabxStatus))
                                    {
                                        _pabxws.Open();
                                        _pabxStatus = EPabxStatus.epsConectando;
                                    }
                                    break;

                                case EPabxStatus.epsConectando:
                                    /** 20181114. Han pasado 5 segundos sin respuesta. Fuerzo otro ping....*/
                                    _pabxStatus = EPabxStatus.epsDesconectado;
                                    _pabxws.Close();
                                    break;

                                case EPabxStatus.epsConectado:
                                    if (!Ping(PbxIp, _pabxStatus))
                                    {
                                        LogWarn<PabxItfService>("Fallo de Ping....Cierro WS");
                                        _pabxStatus = EPabxStatus.epsDesconectado;
                                        _pabxws.Close();

                                        GlobalServices.GetWriteAccess((gdata) =>
                                        {
                                            if (gdata.STDG.stdPabx.Estado != std.NoInfo)
                                                RecordEvent<PabxItfService>(DateTime.Now, U5kBaseDatos.eIncidencias.IGRL_U5KI_SERVICE_ERROR, U5kBaseDatos.eTiposInci.TEH_SISTEMA, "SPV",
                                                    Params(idiomas.strings.PBX_Desconectada/*"PBX Desconectada"*/, "", "", ""));

                                            gdata.STDG.stdPabx.Estado = std.NoInfo;

                                            /* Desregistrar. */
                                            gdata.STDPBXS.ForEach(d => d.Estado = std.NoInfo);
                                        });
                                    }
                                    break;

                                default:
                                    break;
                            }
                        }
                    }
                }
                catch (Exception x)
                {
                    LogException<PabxItfService>("", x);
                }

                _TimerPbax.Enabled = true;
            });

            IamAlive.Tick("PbxItfService-Timer", () =>
            {
                IamAlive.Message(String.Format("PbxItfService-Timer ({0}). Is Alive.", InfoString));
            });
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        PabxParamInfo infoSimul = new PabxParamInfo()
        {
            User = Properties.u5kManServer.Default.PabxSimuladaRegisterSubcriber,
            Registered = "false",
            Status = "0"
        };
        PabxParamInfo infoSimulNoRegistrado = new PabxParamInfo()
        {
            User = Properties.u5kManServer.Default.PabxSimuladaUnregisterSubcriber,
            Registered = "false",
            Status = "0"
        };
        enum EpabxSimulState { eStd1, eStd2, eStd3, eStd4 };
        EpabxSimulState _simulState = EpabxSimulState.eStd1;
        private void OnTimePabxSimuladaElapsed(object sender, ElapsedEventArgs e)
        {
            U5kGenericos.TraceCurrentThread(this.GetType().Name + " TimePbxSim");
            if (IsOperative)
            {
                _WorkingThread.Enqueue("OnTimePabxSimuladaElapsed", delegate ()
                {
                    GlobalServices.GetWriteAccess((gdata) =>
                    {

                        U5kGenericos.TraceCurrentThread(this.GetType().Name + " TimePbxSim enqueue");
                        switch (_simulState)
                        {
                            case EpabxSimulState.eStd1:
                                infoSimulNoRegistrado.Registered = "true";
                                ProcessUserRegistered(gdata, infoSimulNoRegistrado);

                                infoSimul.Registered = "true";
                                ProcessUserRegistered(gdata, infoSimul);
                                _simulState = EpabxSimulState.eStd2;
                                break;

                            case EpabxSimulState.eStd2:
                                infoSimul.Status = "1";
                                ProcessUserStatus(infoSimul);
                                _simulState = EpabxSimulState.eStd3;
                                break;
                            case EpabxSimulState.eStd3:
                                infoSimul.Status = "14";
                                ProcessUserStatus(infoSimul);
                                _simulState = EpabxSimulState.eStd4;
                                break;
                            case EpabxSimulState.eStd4:
                                infoSimul.Status = "-1";
                                ProcessUserStatus(infoSimul);

                                infoSimulNoRegistrado.Registered = "false";
                                ProcessUserRegistered(gdata, infoSimulNoRegistrado);

                                _simulState = EpabxSimulState.eStd1;
                                break;
                        }
                    });

                    _TimerPabxSimulada.Enabled = true;
                });
            }
            else
            {
                _TimerPabxSimulada.Enabled = true;
            }
        }
        #endregion

        #region Private Functions.
        /// <summary>
        /// 
        /// </summary>
        private void Init()
        {
            if (Properties.u5kManServer.Default.PabxSimulada)
                _pabxStatus = EPabxStatus.epsConectado;
            else
            {
                _pabxStatus = EPabxStatus.epsConectando;
                _pabxws.Open();
            }

            _TimerPbax.Enabled = true;

        }
        /// <summary>
        /// 
        /// </summary>
        private new void Dispose()
        {
            _TimerPbax.Enabled = false;

            if (Properties.u5kManServer.Default.PabxSimulada)
            {
                _TimerPabxSimulada.Enabled = false;
                _pabxStatus = EPabxStatus.epsDesconectado;
            }
            else
            {
                if (_pabxStatus == EPabxStatus.epsConectado)
                {
                    _pabxws.Close();
                    _pabxStatus = EPabxStatus.epsDesconectado;
                }
            }
            // AGL. Al ser llamada desde 'fuera', no utilizare el control de acceso para evitar Lazos no deseados...
            GlobalServices.GetWriteAccess((data) =>
            {
                data.STDG.stdPabx.Estado = std.NoInfo;
            }, false);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="_event"></param>
        private void ProcessEvent(U5kManStdData gdata, PabxEvent _event)
        {
            switch (_event.Method)
            {
                case "notify_serverstatus":
                    ProcessServerStatus(gdata, _event.Parametros);
                    break;
                case "notify_status":
                    ProcessUserStatus(_event.Parametros);
                    break;
                case "notify_registered":
                    ProcessUserRegistered(gdata, _event.Parametros);
                    break;
                default:
                    LogWarn<PabxItfService>("Evento No Procesado: " + _event.Method);
                    break;
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="info"></param>
        private void ProcessServerStatus(U5kManStdData gdata, PabxParamInfo info)
        {
            switch (info.Status)
            {
                case "active":
                    _pabxStatus = EPabxStatus.epsConectado;
                    U5KStdGeneral stdg = gdata.STDG;
                    if (stdg.stdPabx.Estado != std.Ok)
                    {
                        RecordEvent<PabxItfService>(DateTime.Now, U5kBaseDatos.eIncidencias.IGRL_U5KI_SERVICE_INFO, U5kBaseDatos.eTiposInci.TEH_SISTEMA, "SPV",
                            Params(idiomas.strings.PBX_Conectada/*"PBX Conectada"*/, "", "", ""));
                    }
                    stdg.stdPabx.Estado = std.Ok;
                    stdg.stdPabx.name = String.Format("{0}:{1}", PbxIp, Properties.u5kManServer.Default.PabxWsPort);
                    break;
                default:
                    _pabxws.Close();
                    _pabxStatus = EPabxStatus.epsDesconectado;
                    break;
            }
            LogDebug<PabxItfService>("Server Status=>" + info.Status);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="info"></param>
        private void ProcessUserRegistered(U5kManStdData gdata, PabxParamInfo info)
        {
            bool registrado = info.Registered == "true";
            List<Uv5kManDestinosPabx.DestinoPabx> pbxdes = gdata.STDPBXS;
            pbxdes.Where(d => d.Id == info.User).ToList().ForEach(d =>
            {
                std EstadoNotificado = registrado ? std.Ok : std.NoInfo;
                if (d.Estado != EstadoNotificado)
                {
                    d.Estado = EstadoNotificado;
                    // Generar Historico de Conexion / Desconexion...
                    RecordEvent<PabxItfService>(DateTime.Now, registrado ? U5kBaseDatos.eIncidencias.IPBX_SUBSC_ACTIVE :
                        U5kBaseDatos.eIncidencias.IPBX_SUBSC_INACTIVE, U5kBaseDatos.eTiposInci.TEH_EXTERNO_TELEFONIA, info.User, Params());
                }
            });
            LogTrace<PabxItfService>(String.Format("Procesado Registro Usuario {0}, {1}", Name, info.User, info.Registered));
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="info"></param>
        private void ProcessUserStatus(PabxParamInfo info)
        {
            LogTrace<PabxItfService>(String.Format("Procesado Estado Usuario {1}, Estado: {2}", Name, info.User, info.Status));
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="host"></param>
        /// <returns></returns>
        private bool Ping(string host, EPabxStatus current)
        {
            int maxReint = current == EPabxStatus.epsConectado ? 3 : 1;
            int reint = 0;
            PingReply reply;

            do
            {
                Ping pingSender = new Ping();
                PingOptions options = new PingOptions();

                // Use the default Ttl value which is 128, 
                // but change the fragmentation behavior.
                options.DontFragment = true;

                // Create a buffer of 32 bytes of data to be transmitted. 
                string data = "Ulises V 5000i. PabxItfService..";
                byte[] buffer = Encoding.ASCII.GetBytes(data);
                int timeout = 120;  // ms
                reply = pingSender.Send(host, timeout, buffer, options);
                reint++;
                System.Threading.Thread.Sleep(10);
            } while (reply.Status != IPStatus.Success && reint < maxReint);

            return reply.Status == IPStatus.Success ? true : false;
        }
        /// <summary>
        /// 
        /// </summary>
        private void DesregistraTodos()
        {
            LogError<PabxItfService>("Invocando Rutina Obsoleta...");
        }
        /// <summary>
        /// 20181114. No se estaba supervisando los cambios de configuracion....
        /// </summary>
        /// <param name="processChange"></param>
        private void ChangeConfigSpv(Action processChange)
        {
            string actualPbxIp = U5kManService.PbxEndpoint == null ? "none" : U5kManService.PbxEndpoint.Address.ToString();
            bool actualHayPbx = U5kManService.PbxEndpoint != null;

            if (actualHayPbx != HayPbx || actualPbxIp != PbxIp)
            {
                processChange();
            }
        }

        #endregion

    }
}
