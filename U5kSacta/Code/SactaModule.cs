#define __SACTA2017__
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Timers;
using System.Diagnostics;

//using System.Web.Configuration;
//using System.Xml;

using Newtonsoft.Json;

using U5kBaseDatos;
using Utilities;

namespace SactaSectionHandler
{

    public class CfgSacta
    {
#if !__LOCAL_TESTING_PARAMETERS__
#if __SACTA2017__
        static public System.Configuration.Configuration cfg = System.Web.Configuration.WebConfigurationManager.OpenWebConfiguration("~/InterfazSacta");
        static public SactaMulticastConfigurationHandler CfgSactaUdp = cfg.GetSection("SactaUdpSection/PuertosMulticast") as SactaMulticastConfigurationHandler;
        static public SactaUsuarioListaPsiConfigurationHandler CfgSactaUsuarioListaPsi = cfg.GetSection("SactaUsuarioSection/listaPSI") as SactaUsuarioListaPsiConfigurationHandler;
        static public SactaUsuarioSettingsConfigurationHandler CfgSactaUsuarioSettings = cfg.GetSection("SactaUsuarioSection/settings") as SactaUsuarioSettingsConfigurationHandler;
        static public SactaUsuarioSectoresConfigurationHandler CfgSactaUsuarioSectores = cfg.GetSection("SactaUsuarioSection/sectores") as SactaUsuarioSectoresConfigurationHandler;
        static public SactaCentroConfigurationHandler CfgSactaCentro = cfg.GetSection("SactaCentroSection/settings") as SactaCentroConfigurationHandler;
        static public SactaDominioConfigurationHandler CfgSactaDominio = cfg.GetSection("SactaDominioSection/settings") as SactaDominioConfigurationHandler;
        static public SactaIpAddressConfigurationHandler CfgIpAddress = cfg.GetSection("SactaUdpSection/IpAddress") as SactaIpAddressConfigurationHandler;
        static public SactaTimeoutsConfigurationHandler CfgTimeouts = cfg.GetSection("SactaTimeOuts/Tiempos") as SactaTimeoutsConfigurationHandler;
        static public SactaIpMulticastConfigurationHandler CfgMulticast = cfg.GetSection("SactaUdpSection/IpMulticast") as SactaIpMulticastConfigurationHandler;
#else
        static public SactaMulticastConfigurationHandler CfgSactaUdp = System.Web.Configuration.WebConfigurationManager.GetSection("SactaUdpSection/PuertosMulticast")
                                                                as SactaMulticastConfigurationHandler;
        static public SactaUsuarioListaPsiConfigurationHandler CfgSactaUsuarioListaPsi = System.Web.Configuration.WebConfigurationManager.GetSection("SactaUsuarioSection/listaPSI")
                                                                as SactaUsuarioListaPsiConfigurationHandler;
        static public SactaUsuarioSettingsConfigurationHandler CfgSactaUsuarioSettings = System.Web.Configuration.WebConfigurationManager.GetSection("SactaUsuarioSection/settings")
                                                                as SactaUsuarioSettingsConfigurationHandler;
        static public SactaUsuarioSectoresConfigurationHandler CfgSactaUsuarioSectores = System.Web.Configuration.WebConfigurationManager.GetSection("SactaUsuarioSection/sectores")
                                                                as SactaUsuarioSectoresConfigurationHandler;
        static public SactaCentroConfigurationHandler CfgSactaCentro = System.Web.Configuration.WebConfigurationManager.GetSection("SactaCentroSection/settings")
                                                                as SactaCentroConfigurationHandler;
        static public SactaDominioConfigurationHandler CfgSactaDominio = System.Web.Configuration.WebConfigurationManager.GetSection("SactaDominioSection/settings")
                                                                as SactaDominioConfigurationHandler;
        static public SactaIpAddressConfigurationHandler CfgIpAddress = System.Web.Configuration.WebConfigurationManager.GetSection("SactaUdpSection/IpAddress")
                                                                as SactaIpAddressConfigurationHandler;
        static public SactaTimeoutsConfigurationHandler CfgTimeouts = System.Web.Configuration.WebConfigurationManager.GetSection("SactaTimeOuts/Tiempos")
                                                                as SactaTimeoutsConfigurationHandler;
        static public SactaIpMulticastConfigurationHandler CfgMulticast = System.Web.Configuration.WebConfigurationManager.GetSection("SactaUdpSection/IpMulticast")
                                                                as SactaIpMulticastConfigurationHandler;
#endif
#else
        static public SactaMulticastConfigurationHandler CfgSactaUdp = new SactaMulticastConfigurationHandler()
        {
            PuertoDestino = 15100,
            PuertoOrigen = 19204
        };
        static public SactaUsuarioListaPsiConfigurationHandler CfgSactaUsuarioListaPsi = new SactaUsuarioListaPsiConfigurationHandler()
        {
            idSpsi = "111,112,113,114,7286,7287,7288,7289",
            idSpv = "86,87,88,89,7266,7267,7268,7269"              
        };
        static public SactaUsuarioSettingsConfigurationHandler CfgSactaUsuarioSettings = new SactaUsuarioSettingsConfigurationHandler()
        {
            Grupo = 110,
            Origen = 10
        };
        static public SactaUsuarioSectoresConfigurationHandler CfgSactaUsuarioSectores = new SactaUsuarioSectoresConfigurationHandler()
        {
            IdSectores = "25"
        };
        static public SactaCentroConfigurationHandler CfgSactaCentro = new SactaCentroConfigurationHandler()
        {
            Destino = 107,
             Origen = 107
        };
        static public SactaDominioConfigurationHandler CfgSactaDominio = new SactaDominioConfigurationHandler()
        {
            Origen = 1,
            Destino = 1
        };
        static public SactaIpAddressConfigurationHandler CfgIpAddress = new SactaIpAddressConfigurationHandler()
        {
            IpRedA = "192.168.0.71",
            IpRedB = "192.168.1.71"
        };
        static public SactaTimeoutsConfigurationHandler CfgTimeouts = new SactaTimeoutsConfigurationHandler()
        {
            Presencia = 5000,
            TimeOutActividad = 30000
        };
        static public SactaIpMulticastConfigurationHandler CfgMulticast = new SactaIpMulticastConfigurationHandler()
        {
            RedA = "225.12.101.1",
            RedB = "225.212.101.1",
            Interfaz = "192.168.0.71"
        };
#endif
    }

    // Class that creates the configuration handler
    public class SactaMulticastConfigurationHandler : ConfigurationSection
    {
        public SactaMulticastConfigurationHandler() { }

        [ConfigurationProperty("PuertoOrigen", DefaultValue = 15100, IsRequired = true)]
        //[IntegerValidator(MinValue = 4000, MaxValue = Int16.MaxValue)]
        public Int32 PuertoOrigen
        {
            get
            {
                return (Int32)this["PuertoOrigen"];
            }
            set
            {
                this["PuertoOrigen"] = value;
            }
        }

        [ConfigurationProperty("PuertoDestino", DefaultValue = 19204, IsRequired = true)]
        [IntegerValidator(MinValue = 4000, MaxValue = Int32.MaxValue)]
        public Int32 PuertoDestino
        {
            get
            {
                return (Int32)this["PuertoDestino"];
            }
            set
            {
                this["PuertoDestino"] = value;
            }
        }
    }

    public class SactaUsuarioSettingsConfigurationHandler : System.Configuration.ConfigurationSection
    {
        public SactaUsuarioSettingsConfigurationHandler() { }

        [ConfigurationProperty("Origen", DefaultValue = 1)]
        [IntegerValidator(MinValue = 1, MaxValue = 99999)]
        public Int32 Origen
        {
            get
            {
                return (Int32)this["Origen"];
            }
            set
            {
                this["Origen"] = value;
            }
        }

        [ConfigurationProperty("Grupo", DefaultValue = 110)]
        [IntegerValidator(MinValue = 1, MaxValue = 99999)]
        public Int32 Grupo
        {
            get
            {
                return (Int32)this["Grupo"];
            }
            set
            {
                this["Grupo"] = value;
            }
        }
    }

    public class SactaUsuarioListaPsiConfigurationHandler : ConfigurationSection
    {
        public SactaUsuarioListaPsiConfigurationHandler() { }

        [ConfigurationProperty("idSpsi", DefaultValue = "")]
        public string idSpsi
        {
            get
            {
                return (string)this["idSpsi"];
            }
            set
            {
                this["idSpsi"] = value;
            }
        }

        [ConfigurationProperty("idSpv", DefaultValue = "")]
        public string idSpv
        {
            get
            {
                return (string)this["idSpv"];
            }
            set
            {
                this["idSpv"] = value;
            }
        }
    }

    public class SactaUsuarioSectoresConfigurationHandler : ConfigurationSection
    {
        public SactaUsuarioSectoresConfigurationHandler() { }

        [ConfigurationProperty("idSectores", DefaultValue = "")]
        public string IdSectores
        {
            get
            {
                return (string)this["idSectores"];
            }
            set
            {
                this["idSectores"] = value;
            }
        }
    }

    public class SactaCentroConfigurationHandler : ConfigurationSection
    {
        // Empty Construct
        public SactaCentroConfigurationHandler() { }

        [ConfigurationProperty("Origen", DefaultValue = 1, IsRequired = true)]
        [IntegerValidator(MinValue = 0, MaxValue = Int32.MaxValue)]
        public Int32 Origen
        {
            get
            {
                return (Int32)this["Origen"];
            }
            set
            {
                this["Origen"] = value;
            }
        }
        [ConfigurationProperty("Destino", DefaultValue = 1, IsRequired = true)]
        //[IntegerValidator(MinValue = 0, MaxValue = byte.MaxValue)]
        public Int32 Destino
        {
            get
            {
                return (Int32)this["Destino"];
            }
            set
            {
                this["Destino"] = value;
            }
        }
    }

    public class SactaDominioConfigurationHandler : ConfigurationSection
    {
        // Empty Construct
        public SactaDominioConfigurationHandler() { }

        [ConfigurationProperty("Origen", DefaultValue = 1, IsRequired = true)]
        //[IntegerValidator(MinValue = 0, MaxValue = byte.MaxValue)]
        public Int32 Origen
        {
            get
            {
                return (Int32)this["Origen"];
            }
            set
            {
                this["Origen"] = value;
            }
        }
        [ConfigurationProperty("Destino", DefaultValue = 1, IsRequired = true)]
        //[IntegerValidator(MinValue = 0, MaxValue = byte.MaxValue)]
        public Int32 Destino
        {
            get
            {
                return (Int32)this["Destino"];
            }
            set
            {
                this["Destino"] = value;
            }
        }
    }

    public class SactaIpAddressConfigurationHandler : ConfigurationSection
    {
        public SactaIpAddressConfigurationHandler() { }

        [ConfigurationProperty("IpRedA", DefaultValue = "127.0.0.1", IsRequired = true)]
        public string IpRedA
        {
            get
            {
                return (string)this["IpRedA"];
            }
            set
            {
                this["IpRedA"] = value;
            }
        }

        [ConfigurationProperty("IpRedB", DefaultValue = "127.0.0.1", IsRequired = true)]
        public string IpRedB
        {
            get
            {
                return (string)this["IpRedB"];
            }
            set
            {
                this["IpRedB"] = value;
            }
        }
    }

    public class SactaIpMulticastConfigurationHandler : ConfigurationSection
    {
        public SactaIpMulticastConfigurationHandler() { }

        [ConfigurationProperty("RedA", DefaultValue = "127.0.0.1", IsRequired = true)]
        public string RedA
        {
            get
            {
                return (string)this["RedA"];
            }
            set
            {
                this["RedA"] = value;
            }
        }

        [ConfigurationProperty("RedB", DefaultValue = "127.0.0.1", IsRequired = true)]
        public string RedB
        {
            get
            {
                return (string)this["RedB"];
            }
            set
            {
                this["RedB"] = value;
            }
        }

#if __SACTA2017__
        [ConfigurationProperty("Interfaz", DefaultValue = "127.0.0.1", IsRequired = true)]
        public string Interfaz
        {
            get
            {
                return (string)this["Interfaz"];
            }
            set
            {
                this["Interfaz"] = value;
            }
        }
#endif

    }

    public class SactaTimeoutsConfigurationHandler : ConfigurationSection
    {
        public SactaTimeoutsConfigurationHandler() { }

        [ConfigurationProperty("Presencia", DefaultValue = 5000, IsRequired = true)]
        [IntegerValidator(MinValue = 0, MaxValue = 60000)]
        public Int32 Presencia
        {
            get
            {
                return (Int32)this["Presencia"];
            }
            set
            {
                this["Presencia"] = value;
            }
        }

        [ConfigurationProperty("TimeOutActividad", DefaultValue = 30000, IsRequired = true)]
        [IntegerValidator(MinValue = 0, MaxValue = 120000)]
        public Int32 TimeOutActividad
        {
            get
            {
                return (Int32)this["TimeOutActividad"];
            }
            set
            {
                this["TimeOutActividad"] = value;
            }
        }

    }
}

namespace U5kSacta
{
    public class SactaModule
    {
#if __SACTA2017__
        /** 20170619. AGL. Obtener los Id's de sectores y ucs's de la base de datos */
        public class CfgBaseDatos
        {
            List<UInt16> idSectores = new List<UInt16>();
            List<UInt16> idUcs = new List<UInt16>();
            List<UInt16> idSectoresIgnorados = new List<ushort>();

            public bool UcsInBdt(UInt16 ucs) { return idUcs.Contains(ucs); }
            public bool SectInBdt(UInt16 sec) { return idSectores.Contains(sec); }
            public bool HayQueIgnorar(UInt16 sec) { return idSectoresIgnorados.Contains(sec); }
            public void init(string idSistema, MySql.Data.MySqlClient.MySqlConnection bdtconn)
            {
                try
                {
                    bdtconn.Open();
                    string sec_qry = String.Format("SELECT NumSacta FROM sectores WHERE IdSistema='{0}' AND sectorsimple=1 AND (tipo='R' OR tipo='V')", idSistema);
                    using (MySql.Data.MySqlClient.MySqlCommand command = new MySql.Data.MySqlClient.MySqlCommand(sec_qry, bdtconn))
                    {
                        using (MySql.Data.MySqlClient.MySqlDataReader reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                idSectores.Add(reader.GetUInt16(0));
                            }
                        }
                    }
                    /** 20180716. Proteccion contra STRING vacio */
                    if (SactaSectionHandler.CfgSacta.CfgSactaUsuarioSectores.IdSectores != string.Empty)
                    {
                        string[] otrossectores = SactaSectionHandler.CfgSacta.CfgSactaUsuarioSectores.IdSectores.Split(new char[] { ',' }).ToArray();
                        foreach (string sect in otrossectores)
                        {
                            /** 20180716. Proteccion errores de formato */
                                try
                                {
                                /** 20180731 Considero todo el filtro. */
                                //if (!idSectores.Contains(UInt16.Parse(sect)))
                                idSectoresIgnorados.Add(UInt16.Parse(sect));
                                }
                                finally { }
                            }
                        }
                    /**************************/

                    string top_qry = String.Format("SELECT PosicionSacta FROM top WHERE IdSistema='{0}'", idSistema);
                    using (MySql.Data.MySqlClient.MySqlCommand command = new MySql.Data.MySqlClient.MySqlCommand(top_qry, bdtconn))
                    {
                        using (MySql.Data.MySqlClient.MySqlDataReader reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                idUcs.Add(reader.GetUInt16(0));
                            }
                        }
                    }
                }
                catch (Exception x)
                {
                    throw x;
                }
                finally
                {
                    bdtconn.Close();
                }
            }
            public string IdSectores
            {
                get
                {
                    string strSectores = "";
                    foreach (Int32 sec in idSectores)
                        strSectores += (sec.ToString() + " ");
                    return strSectores;
                }
            }
            public string IdUcs
            {
                get
                {
                    string strUcs = "";
                    foreach (Int32 ucs in idUcs)
                        strUcs += (ucs.ToString() + " ");
                    return strUcs;
                }
            }
        }
        CfgBaseDatos cfgBasedatos = new CfgBaseDatos();
#endif
        /**************************************************/

        public event GenericEventHandler<Dictionary<string, object>> SactaActivityChanged;

        #region Declaración de atributos
        enum SactaState { WaitingSactaActivity, /*WaitingIOLActivity,*/ WaitingSectorization, WaitingSectFinish, SendingPresences, Stopped }

        const uint _WaitForSectTimeOut = 60000;
        const uint _PeriodicTasksInterval = 1000;

        object _Sync;

#if __SACTA2017__
        UdpSocket _socket = null;
#else
        UdpSocket[] _Comm;
#endif

        IPEndPoint[] _EndPoint;
        int _ActivityState;
        uint _ActivityTimeOut = (uint)SactaSectionHandler.CfgSacta.CfgTimeouts.Presencia; // = Settings.Default.ActivityTimeOut; 
        uint _PresenceInterval = (uint)SactaSectionHandler.CfgSacta.CfgTimeouts.TimeOutActividad; // = Settings.Default.PresenceInterval;
        SactaState _State;
        DateTime[] _LastSactaReceived;
        DateTime _BeginOfWaitForSect;
        DateTime _LastPresenceSended;
#if !__SACTA2017__
        ushort _SeqNum;
        byte[] _InitMsg;
        byte[] _SectAskMsg;
        byte[] _PresenceMsg;
        byte[] _SectAnswerMsg;
#else
        int _SeqNum = 0;
#endif
        Timer _PeriodicTasks;
        uint _TryingSectVersion;
        Dictionary<ushort, PSIInfo> _SactaSPSIUsers;
        Dictionary<ushort, PSIInfo> _SactaSPVUsers;
        bool _Enabled = true; // Settings.Default.Enabled;
        bool _Disposed;
        string IdSistema;
#if !__LOCAL_TESTING__
        MySql.Data.MySqlClient.MySqlConnection ConexionCD40;
#endif
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
        public SactaModule(string idSistema
#if !__LOCAL_TESTING__
            , MySql.Data.MySqlClient.MySqlConnection conexionCD40
#endif
            )
        {
#if !__LOCAL_TESTING__
            ConexionCD40 = conexionCD40;
#endif
            IdSistema = idSistema;

            _Sync = new object();
            _State = SactaState.Stopped;
            _LastSactaReceived = new DateTime[2];

            _SactaSPSIUsers = new Dictionary<ushort, PSIInfo>();
            foreach (string user in SactaSectionHandler.CfgSacta.CfgSactaUsuarioListaPsi.idSpsi.Split(new char[] { ',', ';' }, StringSplitOptions.RemoveEmptyEntries))
            {
                _SactaSPSIUsers[UInt16.Parse(user)] = new PSIInfo();
            }
            _SactaSPVUsers = new Dictionary<ushort, PSIInfo>();
            foreach (string user in SactaSectionHandler.CfgSacta.CfgSactaUsuarioListaPsi.idSpv.Split(new char[] { ',', ';' }, StringSplitOptions.RemoveEmptyEntries))
            {
                _SactaSPVUsers[UInt16.Parse(user)] = new PSIInfo();
            }

            _EndPoint = new IPEndPoint[2];
            _EndPoint[0] = new IPEndPoint(IPAddress.Parse(SactaSectionHandler.CfgSacta.CfgMulticast.RedA), SactaSectionHandler.CfgSacta.CfgSactaUdp.PuertoDestino);
            _EndPoint[1] = new IPEndPoint(IPAddress.Parse(SactaSectionHandler.CfgSacta.CfgMulticast.RedB), SactaSectionHandler.CfgSacta.CfgSactaUdp.PuertoDestino);

#if !__SACTA2017__
            CustomBinaryFormatter bf = new CustomBinaryFormatter();

            MemoryStream ms = new MemoryStream();
            SactaMsg msg = new SactaMsg(SactaMsg.MsgType.Init, SactaMsg.InitId);
            bf.Serialize(ms, msg);
            _InitMsg = ms.ToArray();

            ms = new MemoryStream();
            msg = new SactaMsg(SactaMsg.MsgType.SectAsk, 0);
            bf.Serialize(ms, msg);
            _SectAskMsg = ms.ToArray();

            ms = new MemoryStream();
            msg = new SactaMsg(SactaMsg.MsgType.SectAnwer, 0);
            bf.Serialize(ms, msg);
            _SectAnswerMsg = ms.ToArray();

            ms = new MemoryStream();
            msg = new SactaMsg(SactaMsg.MsgType.Presence, 0);
            bf.Serialize(ms, msg);
            _PresenceMsg = ms.ToArray();
#endif

#if __SACTA2017__
            cfgBasedatos.init(IdSistema, ConexionCD40);

            /** 20180716. Para seleccionar la IP Source.. */
            _socket = new UdpSocket(SactaSectionHandler.CfgSacta.CfgMulticast.Interfaz, SactaSectionHandler.CfgSacta.CfgSactaUdp.PuertoOrigen);

            /** Para seleccionar correctamente la Interfaz de salida de las tramas MCAST */
            _socket.Base.MulticastLoopback = false;
            _socket.Base.JoinMulticastGroup(IPAddress.Parse(SactaSectionHandler.CfgSacta.CfgMulticast.RedA),
                IPAddress.Parse(SactaSectionHandler.CfgSacta.CfgMulticast.Interfaz));
            _socket.Base.JoinMulticastGroup(IPAddress.Parse(SactaSectionHandler.CfgSacta.CfgMulticast.RedB),
                IPAddress.Parse(SactaSectionHandler.CfgSacta.CfgMulticast.Interfaz));

            /** 20180731. Para poder pasar por una red de ROUTERS */
            _socket.Base.Client.SetSocketOption(SocketOptionLevel.IP,SocketOptionName.MulticastTimeToLive, 16);

            _socket.NewDataEvent += OnNewData;
#else
            _Comm = new UdpSocket[] {	new UdpSocket(SactaSectionHandler.CfgSacta.CfgIpAddress.IpRedA, SactaSectionHandler.CfgSacta.CfgSactaUdp.PuertoOrigen), 
										new UdpSocket(SactaSectionHandler.CfgSacta.CfgIpAddress.IpRedB,SactaSectionHandler.CfgSacta.CfgSactaUdp.PuertoOrigen) };
            _Comm[0].NewDataEvent += OnNewData;
            _Comm[1].NewDataEvent += OnNewData;
#endif

            _PeriodicTasks = new Timer(_PeriodicTasksInterval);
            _PeriodicTasks.AutoReset = false;
            _PeriodicTasks.Elapsed += PeriodicTasks;

#if __SACTA2017__
            Log(false, System.Reflection.MethodBase.GetCurrentMethod().Name, "SactaModule inicializado. Sacta1 en {0}:{1}, Sacta2 en {2}:{3}",
                _EndPoint[0].Address.ToString(), _EndPoint[0].Port, _EndPoint[1].Address.ToString(), _EndPoint[1].Port);
            Log(false, System.Reflection.MethodBase.GetCurrentMethod().Name, "SactaModule. Gestionado los Sectores: {0} en las posicones {1} ",
               cfgBasedatos.IdSectores, cfgBasedatos.IdUcs);
#endif
        }

        /// <summary>
        /// 
        /// </summary>
        public void Start()
        {
            if (_Enabled)
            {
                _State = SactaState.WaitingSactaActivity;
#if __SACTA2017__
                _socket.BeginReceive();
                Log(false, System.Reflection.MethodBase.GetCurrentMethod().Name, "Modulo arrancado en puerto {0}...", SactaSectionHandler.CfgSacta.CfgSactaUdp.PuertoOrigen);
#else
                _Comm[0].BeginReceive();
                _Comm[1].BeginReceive();
#endif
                _PeriodicTasks.Enabled = true;
            }
        }
        /// <summary>
        /// 
        /// </summary>
        public void Stop()
        {
            if (!_Disposed)
            {
                _Disposed = true;
                _State = SactaState.Stopped;

                if (_PeriodicTasks != null)
                {
                    _PeriodicTasks.Enabled = false;
                    _PeriodicTasks.Close();
                    _PeriodicTasks = null;
                }

#if __SACTA2017__
                if (_socket != null)
                {
                    _socket.Dispose();
                    _socket = null;
                }
                Log(false, System.Reflection.MethodBase.GetCurrentMethod().Name, "Modulo detenido...");
#else
                if (_Comm != null)
                {
                    _Comm[0].Dispose();
                    _Comm[1].Dispose();
                    _Comm = null;
                }
#endif
            }
        }

#if __SACTA2017__
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

#endif

#region Private Members

/// <summary>
/// 
/// </summary>
/// <param name="sender"></param>
/// <param name="dg"></param>
#if __SACTA2017_V00
        private Task sectProcessTask = null;
#endif
        void OnNewData(object sender, DataGram dg)
        {
            MemoryStream ms = new MemoryStream(dg.Data);
            CustomBinaryFormatter bf = new CustomBinaryFormatter();
            SactaMsg msg = bf.Deserialize<SactaMsg>(ms);

#if __SACTA2017_V00
            bool MustSectorize = false;
#endif
            try
            {
#if __SACTA2017__
                lock (_Sync)
                {
#endif
#if __SACTA2017__
                    // Se comparta el BYTE 2 para determinar SACTA1 o SACTA2
                    byte client = dg.Client.Address.GetAddressBytes()[2];
                    byte sacta1 = IPAddress.Parse(SactaSectionHandler.CfgSacta.CfgIpAddress.IpRedA).GetAddressBytes()[2];
                    byte sacta2 = IPAddress.Parse(SactaSectionHandler.CfgSacta.CfgIpAddress.IpRedB).GetAddressBytes()[2];

                    int net = client == sacta1 ? 0 : client == sacta2 ? 1 : -1;
                    if (net == -1)
                    {
                        Log(true, System.Reflection.MethodBase.GetCurrentMethod().Name, "Recibida Trama no identificada de {0}", dg.Client.Address.ToString());
                        return;
                    }
#else
                int net = (sender == _Comm[0] ? 0 : 1);
#endif
                    Log(false, System.Reflection.MethodBase.GetCurrentMethod().Name, "Mensaje de red {0} recibido: {1}", net == 0 ? "SACTA1" : "SACTA2", msg.Type);
                    if (IsValid(msg))
                    {
                        _LastSactaReceived[net] = DateTime.Now;

                        switch (msg.Type)
                        {
                            case SactaMsg.MsgType.Presence:
                                _ActivityTimeOut = (uint)(((SactaMsg.PresenceInfo)(msg.Info)).ActivityTimeOutSg * 1000);
                                _PresenceInterval = (uint)(((SactaMsg.PresenceInfo)(msg.Info)).PresencePerioditySg * 1000);
                                break;
#if __SACTA2017_V00
                            case SactaMsg.MsgType.Sectorization:
                                bool skip = true;
                                uint version = ((SactaMsg.SectInfo)(msg.Info)).Version;
                                Log(false, System.Reflection.MethodBase.GetCurrentMethod().Name, "Recibida petición de sectorización (Red = {0}, Versión = {1}", net, version);



#if !__SACTA2017__
                            lock (_Sync)
                            {
#endif
                                if (!IsSecondSectMsg(msg))
                                {
                                    if ((_State == SactaState.SendingPresences) || (_State == SactaState.WaitingSectorization))
                                    {
                                        _State = SactaState.WaitingSectFinish;
                                        _TryingSectVersion = version;
                                        skip = false;
                                    }
                                    else if ((_State == SactaState.WaitingSectFinish) && (version != _TryingSectVersion))
                                    {
                                        /** ??? */
                                        //_TryingSectVersion = version;
                                        //skip = false;
                                        Log(true, System.Reflection.MethodBase.GetCurrentMethod().Name, "Sectorizacion en cursos. Peticion Rechazada...");
                                        SendSectAnswer(((SactaMsg.SectInfo)(msg.Info)).Version, 1);
                                    }
                                }
#if !__SACTA2017__
                            }
#endif
                                if (!skip)
                                {
#if !__SACTA2017__
                                    ProcessSectorization(msg);
#endif
                                    MustSectorize = true;
                                }
                                else
                                {
                                    Log(false, System.Reflection.MethodBase.GetCurrentMethod().Name, "Petición de sectorización (Red = {0}, Versión = {1}, DESCARTADA...", net, version);
                                }
                                break;
#else
                            case SactaMsg.MsgType.Sectorization:
                                ProcessMsgSect(net, msg);
                                break;
#endif
                        }

                    }
#if __SACTA2017__
                }

#if __SACTA2017_V00
                if (MustSectorize==true)
                {
#if DEBUG1
#else
                    if (sectProcessTask == null)
                    {
                        sectProcessTask = Task.Factory.StartNew(() =>
                        {
                            Log(true, System.Reflection.MethodBase.GetCurrentMethod().Name, "Procesando Sectorizacion...");
                            ProcessSectorization(msg);
                            Log(true, System.Reflection.MethodBase.GetCurrentMethod().Name, "Sectorizacion procesada...");
                            sectProcessTask = null;
                        });
                    }
                    else
                    {
                        Log(true, System.Reflection.MethodBase.GetCurrentMethod().Name, "Sectorizacion en cursos. Peticion Rechazada...");
                        SendSectAnswer(((SactaMsg.SectInfo)(msg.Info)).Version, 1);
                    }
#endif
                }
#endif
#endif
            }
            catch (Exception x)
            {
                if (!_Disposed)
                {
#if !__LOCAL_TESTING__
                    // _Logger.ErrorException(Resources.SactaDataError, ex);
                    const int Error = 1;
                    uint version = ((SactaMsg.SectInfo)(msg.Info)).Version;
                    //Settings stts = Settings.Default;
                    //			ModuleInfo info = new ModuleInfo();
                    CD40.BD.SactaInfo info = new CD40.BD.SactaInfo();

                    info["SectVersion"] = version;
                    info["Resultado"] = Error;
                    info["ErrorCause"] = x.Message;

                    OnResultSectorizacion(info);
#endif
                }
                Log(true, System.Reflection.MethodBase.GetCurrentMethod().Name, "Excepcion: {0}", x.Message);
            }
        }

#if !__SACTA2017_V00
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
                                    if (!_Disposed)
                                    {
                                        CD40.BD.SactaInfo info = new CD40.BD.SactaInfo();

                                        info["SectVersion"] = ((SactaMsg.SectInfo)(currentSect.Info)).Version;
                                        info["Resultado"] = 1;
                                        info["ErrorCause"] = x.Message;

                                        OnResultSectorizacion(info);
                                    }
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
#endif

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
                if (_ActivityState != _LastActivityState || _State != _LastState)
                {
                    Log(false, System.Reflection.MethodBase.GetCurrentMethod().Name, "Tick {0},{1}", _ActivityState, _State);
                    _LastActivityState = _ActivityState;
                    _LastState = _State;
                }
                lock (_Sync)
                {
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
                            ((uint)((DateTime.Now - _BeginOfWaitForSect).TotalMilliseconds) > _WaitForSectTimeOut)))
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
            //Settings stts = Settings.Default;

            return ((msg.DomainOrg == SactaSectionHandler.CfgSacta.CfgSactaDominio.Destino) && (msg.DomainDst == SactaSectionHandler.CfgSacta.CfgSactaDominio.Origen) &&
                    (msg.CenterOrg == SactaSectionHandler.CfgSacta.CfgSactaCentro.Destino) && (msg.CenterDst == SactaSectionHandler.CfgSacta.CfgSactaCentro.Origen) &&
                    (msg.UserDst == SactaSectionHandler.CfgSacta.CfgSactaUsuarioSettings.Origen) && validUsers.ContainsKey(msg.UserOrg));
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
#if __LOCAL_TESTING__
            Log(System.Reflection.MethodBase.GetCurrentMethod().Name, "Procesando Sectorizacion Recibida....");
            OnResultSectorizacion(0, _TryingSectVersion);
#else
            const int Error = 1;

            StringBuilder str = new StringBuilder();
            SactaMsg.SectInfo sactaSect = (SactaMsg.SectInfo)(msg.Info);
            CD40.BD.SactaInfo info = new CD40.BD.SactaInfo();

            info["SectVersion"] = sactaSect.Version;
            List<SactaMsg.SectInfo.SectorInfo> listaSectores = new List<SactaMsg.SectInfo.SectorInfo>();

            foreach (SactaMsg.SectInfo.SectorInfo sector in sactaSect.Sectors)
            {
                /** TODO. Discriminar Virtuales de Ignorados */
                /** Ignoro los sectores virtuales */
                if (cfgBasedatos.HayQueIgnorar(UInt16.Parse(sector.SectorCode)))
                {
                    Log(false, System.Reflection.MethodBase.GetCurrentMethod().Name, "Ignorando Asignacion Virtual {0} => {1}", sector.SectorCode, sector.Ucs);
                    continue;
                }

                if (!cfgBasedatos.UcsInBdt((UInt16)sector.Ucs))
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
#if !__SACTA2017__
                if (!SactaSectionHandler.CfgSacta.CfgSactaUsuarioSectores.IdSectores.Contains(sector.SectorCode))
#else
                if (!cfgBasedatos.SectInBdt(UInt16.Parse(sector.SectorCode)))
#endif
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
            //CD40.BD.Procedimientos p = new CD40.BD.Procedimientos();
            str.Append(CD40.BD.Procedimientos.SectoresManttoEnActiva(ConexionCD40, IdSistema));

            info["SectName"] = "SACTA";
            info["SectData"] = str.ToString();
            info["IdSistema"] = IdSistema;

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

#endif
        }

#if !__LOCAL_TESTING__
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
#else
        void OnResultSectorizacion(int result, uint version)
        {
            try
            {
                lock (_Sync)
                {
                    if ((_State == SactaState.WaitingSectFinish) && (version == _TryingSectVersion))
                    {
                        _State = SactaState.SendingPresences;
                        SendSectAnswer(version, result);

                        //if (result == 0)
                        //{
                        //    Ref_ServicioInterfazSacta.ServicioInterfazSacta servicioInterfazSacta = new Sacta.Ref_ServicioInterfazSacta.ServicioInterfazSacta();
                        //    servicioInterfazSacta.ComunicaSectorizacion();
                        //}
                    }
                }
            }
            catch (Exception)
            {
                if (!_Disposed)
                {
                    // _Logger.ErrorException(Resources.OnSectResultError, ex);
                }
            }
        }
#endif
        /// <summary>
        /// 
        /// </summary>
        void SendInit()
        {
            Debug.Assert(_ActivityState != 0);
#if __SACTA2017__
            if ((_ActivityState & 0x1) == 0x1)
                _socket.Send(_EndPoint[0], (new SactaMsg(SactaMsg.MsgType.Init, SactaMsg.InitId, 0)).Serialize());
            if ((_ActivityState & 0x2) == 0x2)
                _socket.Send(_EndPoint[1], (new SactaMsg(SactaMsg.MsgType.Init, SactaMsg.InitId, 0)).Serialize());
            Log(false, System.Reflection.MethodBase.GetCurrentMethod().Name, "Mensaje INIT enviado...");
#else
            if ((_ActivityState & 0x1) == 0x1) _Comm[0].Send(_EndPoint[0], _InitMsg);
            if ((_ActivityState & 0x2) == 0x2) _Comm[1].Send(_EndPoint[1], _InitMsg);
#endif
            _SeqNum = 0;
        }
        /// <summary>
        /// 
        /// </summary>
        void SendSectAsk()
        {
            Debug.Assert(_ActivityState != 0);

#if __SACTA2017__
            if ((_ActivityState & 0x1) == 0x1)
                _socket.Send(_EndPoint[0], (new SactaMsg(SactaMsg.MsgType.SectAsk, 0, _SeqNum)).Serialize());
            if ((_ActivityState & 0x2) == 0x2)
                _socket.Send(_EndPoint[1], (new SactaMsg(SactaMsg.MsgType.SectAsk, 0, _SeqNum)).Serialize());

            _SeqNum = _SeqNum >= 287 ? 0 : _SeqNum + 1; // (_SeqNum + 1) & 0x1FFF;

            Log(false, System.Reflection.MethodBase.GetCurrentMethod().Name, "Mensaje SECTASK enviado...");
#else
            if ((_ActivityState & 0x1) == 0x1) _Comm[0].Send(_EndPoint[0], _SectAskMsg);
            if ((_ActivityState & 0x2) == 0x2) _Comm[1].Send(_EndPoint[1], _SectAskMsg);
#endif
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

#if __SACTA2017__
            if ((_ActivityState & 0x1) == 0x1)
                _socket.Send(_EndPoint[0], (new SactaMsg(SactaMsg.MsgType.SectAnwer, 0, _SeqNum, (int)version, (result == 0 ? 1 : 0))).Serialize());
            if ((_ActivityState & 0x2) == 0x2)
                _socket.Send(_EndPoint[1], (new SactaMsg(SactaMsg.MsgType.SectAnwer, 0, _SeqNum, (int)version, (result == 0 ? 1 : 0))).Serialize());

            _SeqNum = _SeqNum >= 287 ? 0 : _SeqNum + 1; // _SeqNum = (_SeqNum + 1) & 0x1FFF;

            Log(false, System.Reflection.MethodBase.GetCurrentMethod().Name, "Mensaje SectAnswer enviado...");
#else
            _SectAnswerMsg[13] = (byte)_SeqNum++;
            _SectAnswerMsg[24] = (byte)(result == 0 ? 1 : 0);
            Array.Copy(BitConverter.GetBytes(version), 0, _SectAnswerMsg, 20, 4);
            Array.Reverse(_SectAnswerMsg, 20, 4);
            if ((_ActivityState & 0x1) == 0x1) _Comm[0].Send(_EndPoint[0], _SectAnswerMsg);
            if ((_ActivityState & 0x2) == 0x2) _Comm[1].Send(_EndPoint[1], _SectAnswerMsg);
#endif
        }
        /// <summary>
        /// 
        /// </summary>
        void SendPresence()
        {
            Debug.Assert(_ActivityState != 0);

#if __SACTA2017__
            if ((_ActivityState & 0x1) == 0x1)
                _socket.Send(_EndPoint[0], (new SactaMsg(SactaMsg.MsgType.Presence, 0, _SeqNum)).Serialize());
            if ((_ActivityState & 0x2) == 0x2)
                _socket.Send(_EndPoint[1], (new SactaMsg(SactaMsg.MsgType.Presence, 0, _SeqNum)).Serialize());

            _SeqNum = _SeqNum >= 287 ? 0 : _SeqNum + 1; // _SeqNum = (_SeqNum + 1) & 0x1FFF;

            Log(false, 
                System.Reflection.MethodBase.GetCurrentMethod().Name, 
                String.Format("{0}, Mensaje Presencia Enviado...", strModuleState));
#else
            _PresenceMsg[13] = (byte)_SeqNum++;
            if ((_ActivityState & 0x1) == 0x1) _Comm[0].Send(_EndPoint[0], _PresenceMsg);
            if ((_ActivityState & 0x2) == 0x2) _Comm[1].Send(_EndPoint[1], _PresenceMsg);
#endif
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
#if __SACTA2017__
            string message = String.Format("[{0}.{1}]: {2}", "SactaModule", from, msg);
            if (isError)
                NLog.LogManager.GetLogger("SactaModule").Error(message, par);
            else
                NLog.LogManager.GetLogger("SactaModule").Info(message, par);
#endif
        }

        /// <summary>
        /// 
        /// </summary>
        class SactaCfgInterk
        {
            public class SactaSide
            {
                public class SactaLan
                {
                    public string ipmask { get; set; }
                    public string mcast { get; set; }
                    public int udpport { get; set; }
                }
                public Int32 Domain { get; set; }
                public Int32 Center { get; set; }
                public Int32 GrpUser { get; set; }
                public String SpiUsers { get; set; }
                public String SpvUsers { get; set; }
                public SactaLan lan1 { get; set; }
                public SactaLan lan2 { get; set; }

                public SactaSide()
                {
                    Domain = SactaSectionHandler.CfgSacta.CfgSactaDominio.Destino;
                    Center = SactaSectionHandler.CfgSacta.CfgSactaCentro.Destino;
                    GrpUser = SactaSectionHandler.CfgSacta.CfgSactaUsuarioSettings.Grupo;
                    SpiUsers = SactaSectionHandler.CfgSacta.CfgSactaUsuarioListaPsi.idSpsi;
                    SpvUsers = SactaSectionHandler.CfgSacta.CfgSactaUsuarioListaPsi.idSpv;

                    lan1 = new SactaLan();
                    lan1.ipmask = SactaSectionHandler.CfgSacta.CfgIpAddress.IpRedA;
                    lan1.mcast = SactaSectionHandler.CfgSacta.CfgMulticast.RedA;
                    lan1.udpport = SactaSectionHandler.CfgSacta.CfgSactaUdp.PuertoDestino;

                    lan2 = new SactaLan();
                    lan2.ipmask = SactaSectionHandler.CfgSacta.CfgIpAddress.IpRedB;
                    lan2.mcast = SactaSectionHandler.CfgSacta.CfgMulticast.RedB;
                    lan2.udpport = SactaSectionHandler.CfgSacta.CfgSactaUdp.PuertoDestino;
                }

                public void Save()
                {
                    SactaSectionHandler.CfgSacta.CfgSactaDominio.Destino = Domain;
                    SactaSectionHandler.CfgSacta.CfgSactaCentro.Destino = Center;
                    SactaSectionHandler.CfgSacta.CfgSactaUsuarioSettings.Grupo = GrpUser;

                    SactaSectionHandler.CfgSacta.CfgSactaUsuarioListaPsi.idSpsi = SpiUsers;
                    SactaSectionHandler.CfgSacta.CfgSactaUsuarioListaPsi.idSpv = SpvUsers;

                    SactaSectionHandler.CfgSacta.CfgIpAddress.IpRedA = lan1.ipmask;
                    SactaSectionHandler.CfgSacta.CfgMulticast.RedA = lan1.mcast;
                    SactaSectionHandler.CfgSacta.CfgSactaUdp.PuertoDestino = lan1.udpport;

                    SactaSectionHandler.CfgSacta.CfgIpAddress.IpRedB = lan2.ipmask;
                    SactaSectionHandler.CfgSacta.CfgMulticast.RedB = lan2.mcast;
                    SactaSectionHandler.CfgSacta.CfgSactaUdp.PuertoDestino = lan2.udpport;
                }
            }

            public class ScvSide
            {
                public Int32 Domain { get; set; }
                public Int32 Center { get; set; }
                public Int32 User { get; set; }
                public string Interfaz { get; set; }
                public int udpport { get; set; }
                public string Ignore { get; set; }

                public ScvSide()
                {
                    Domain = SactaSectionHandler.CfgSacta.CfgSactaDominio.Origen;
                    Center = SactaSectionHandler.CfgSacta.CfgSactaCentro.Origen;
                    Interfaz = SactaSectionHandler.CfgSacta.CfgMulticast.Interfaz;
                    User = SactaSectionHandler.CfgSacta.CfgSactaUsuarioSettings.Origen;
                    udpport = SactaSectionHandler.CfgSacta.CfgSactaUdp.PuertoOrigen;
                    Ignore = SactaSectionHandler.CfgSacta.CfgSactaUsuarioSectores.IdSectores;
                }

                public void Save()
                {
                    SactaSectionHandler.CfgSacta.CfgSactaDominio.Origen = Domain;
                    SactaSectionHandler.CfgSacta.CfgSactaCentro.Origen = Center;
                    SactaSectionHandler.CfgSacta.CfgMulticast.Interfaz = Interfaz;
                    SactaSectionHandler.CfgSacta.CfgSactaUsuarioSettings.Origen = User;
                    SactaSectionHandler.CfgSacta.CfgSactaUdp.PuertoOrigen = udpport;
                    SactaSectionHandler.CfgSacta.CfgSactaUsuarioSectores.IdSectores = Ignore;
                }
            }

            public Int32 TickPresencia { get; set; }
            public Int32 TimeoutPresencia { get; set; }
            public SactaSide sacta { get; set; }
            public ScvSide scv { get; set; }

            public SactaCfgInterk()
            {
                TickPresencia = SactaSectionHandler.CfgSacta.CfgTimeouts.Presencia;
                TimeoutPresencia = SactaSectionHandler.CfgSacta.CfgTimeouts.TimeOutActividad;

                sacta = new SactaSide();
                scv = new ScvSide();
            }

            public void Save()
            {
                // Salvar la configuracion en Memoria.
                sacta.Save();
                scv.Save();
                SactaSectionHandler.CfgSacta.CfgTimeouts.Presencia = TickPresencia;
                SactaSectionHandler.CfgSacta.CfgTimeouts.TimeOutActividad = TimeoutPresencia;

                // Salvar configuracion en fichero...   
                string physicalPath = Path.Combine(System.Web.HttpContext.Current.Server.MapPath("~"), "web.config");
                Log(false, System.Reflection.MethodBase.GetCurrentMethod().Name, "Loading {0}...", physicalPath);

                XmlDocument doc = new XmlDocument();
                doc.Load(physicalPath);
                XmlElement Root = doc.DocumentElement;
                //
                Root.SelectNodes("SactaUsuarioSection/settings")[0].Attributes["Origen"].Value = scv.User.ToString();
                Root.SelectNodes("SactaUsuarioSection/settings")[0].Attributes["Grupo"].Value = sacta.GrpUser.ToString();
                Root.SelectNodes("SactaUsuarioSection/listaPSI")[0].Attributes["idSpsi"].Value = sacta.SpiUsers;
                Root.SelectNodes("SactaUsuarioSection/listaPSI")[0].Attributes["idSpv"].Value = sacta.SpvUsers;
                Root.SelectNodes("SactaUsuarioSection/sectores")[0].Attributes["idSectores"].Value = scv.Ignore;
                //
                Root.SelectNodes("SactaCentroSection/settings")[0].Attributes["Origen"].Value = scv.Center.ToString();
                Root.SelectNodes("SactaCentroSection/settings")[0].Attributes["Destino"].Value = sacta.Center.ToString();
                //
                Root.SelectNodes("SactaDominioSection/settings")[0].Attributes["Origen"].Value = scv.Domain.ToString();
                Root.SelectNodes("SactaDominioSection/settings")[0].Attributes["Destino"].Value = sacta.Domain.ToString();
                //
                Root.SelectNodes("SactaUdpSection/PuertosMulticast")[0].Attributes["PuertoOrigen"].Value = scv.udpport.ToString();
                Root.SelectNodes("SactaUdpSection/PuertosMulticast")[0].Attributes["PuertoDestino"].Value = sacta.lan1.udpport.ToString();
                Root.SelectNodes("SactaUdpSection/IpMulticast")[0].Attributes["RedA"].Value = sacta.lan1.mcast;
                Root.SelectNodes("SactaUdpSection/IpMulticast")[0].Attributes["RedB"].Value = sacta.lan2.mcast;
                Root.SelectNodes("SactaUdpSection/IpMulticast")[0].Attributes["Interfaz"].Value = scv.Interfaz;
                Root.SelectNodes("SactaUdpSection/IpAddress")[0].Attributes["IpRedA"].Value = sacta.lan1.ipmask;
                Root.SelectNodes("SactaUdpSection/IpAddress")[0].Attributes["IpRedB"].Value = sacta.lan2.ipmask;
                //
                Root.SelectNodes("SactaTimeOuts/Tiempos")[0].Attributes["Presencia"].Value = TickPresencia.ToString();
                Root.SelectNodes("SactaTimeOuts/Tiempos")[0].Attributes["TimeOutActividad"].Value = TimeoutPresencia.ToString();

                Log(false, System.Reflection.MethodBase.GetCurrentMethod().Name, "Writting {0}...", physicalPath);
                doc.Save(physicalPath);
            }
        }

        /// <summary>
        /// Para pantallas de Configuracion...
        /// </summary>
        /// <returns></returns>
        public static string SactaCfgGet()
        {
            return JsonConvert.SerializeObject(new SactaCfgInterk());
        }
        /// <summary>
        /// Para pantallas de configuracion.
        /// </summary>
        /// <param name="sactacfg"></param>
        /// <returns></returns>
        public static bool SactaCfgSet(string sactacfg)
        {
            Log(false, System.Reflection.MethodBase.GetCurrentMethod().Name, "Entrando en el procedimiento...");

            SactaCfgInterk cfg = JsonConvert.DeserializeObject<SactaCfgInterk>(sactacfg);
            cfg.Save();
                        
            Log(false, System.Reflection.MethodBase.GetCurrentMethod().Name, "Saliendo del procedimiento...");
            return true;
        }

    }
}
