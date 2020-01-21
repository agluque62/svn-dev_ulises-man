using System;
using System.IO;
using System.Net;
using System.Timers;
using System.Diagnostics;
using System.Collections.Generic;
using System.Text;
using Utilities;
using System.Configuration;

namespace SactaSectionHandler
{
#if !_SACTASERVICE_TEST_

    public class CfgSacta
	{
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
	}

	//private static struct Default
	//{
	//    static public Int16 PuertoOrigen=
	//}

	//private static Settings defaultInstance = ((Settings)(global::System.Configuration.ApplicationSettingsBase.Synchronized(new Settings())));

	//public static Settings Default {
	//    get {
	//        return defaultInstance;
	//    }
	//}


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
#else
    public class CfgSacta   
    {
        public class CfgSactaUsuarioListaPsi
        {
            public static string idSpsi = "111,112,113,114,7286,7287,7288,7289";
            public static string idSpv = "86,87,88,89,7266,7267,7268,7269";
        }

        public class CfgMulticast
        {
            public static string RedA = "192.168.0.71";
            public static string RedB = "192.168.0.72";
        }

        public class CfgSactaUdp
        {
            public static int PuertoDestino = 19204;
            public static int PuertoOrigen = 15100;
        }

        public class CfgIpAddress
        {
            public static string IpRedA = "192.168.0.71";
            public static string IpRedB = "192.168.0.72";
        }

        public class CfgSactaDominio
        {
            public static int Origen = 1;
            public static int Destino = 1;
        }

        public class CfgSactaCentro
        {
            public static int Origen = 107;
            public static int Destino = 107;
        }

        public class CfgSactaUsuarioSettings
        {
            public static int Origen = 0;
            public static int Grupo = 0;
        }

        public class CfgSactaUsuarioSectores
        {
            public static string IdSectores = "1,2,3,4,5,6,7,8";
        }

        public class CfgTimeouts
        {
            public static int Presencia = 5000;
            public static int TimeOutActividad = 30000;
        }
    }
#endif
}

namespace Sacta
{
	public class SactaModule
	{
		//static Ref_InterfazSacta.ServicioInterfazSacta servicioInterfazSacta = null;

		public event GenericEventHandler<Dictionary<string, object>> SactaActivityChanged;

		#region Declaración de atributos
		enum SactaState { WaitingSactaActivity, /*WaitingIOLActivity,*/ WaitingSectorization, WaitingSectFinish, SendingPresences }

		const uint _WaitForSectTimeOut = 60000;
		const uint _PeriodicTasksInterval = 1000;

		//static Logger // _Logger = LogManager.GetCurrentClassLogger();

		object _Sync;
		UdpSocket[] _Comm;
		IPEndPoint[] _EndPoint;
		//int _ScvPrincipal = 0xFF;
		int _ActivityState;
		uint _ActivityTimeOut = (uint)SactaSectionHandler.CfgSacta.CfgTimeouts.Presencia; // = Settings.Default.ActivityTimeOut; 
		uint _PresenceInterval = (uint)SactaSectionHandler.CfgSacta.CfgTimeouts.TimeOutActividad; // = Settings.Default.PresenceInterval;
		SactaState _State;
		DateTime[] _LastSactaReceived;
		DateTime _BeginOfWaitForSect;
		DateTime _LastPresenceSended;
		ushort _SeqNum;
		byte[] _InitMsg;
		byte[] _SectAskMsg;
		byte[] _PresenceMsg;
		byte[] _SectAnswerMsg;
		Timer _PeriodicTasks;
		uint _TryingSectVersion;
		Dictionary<ushort, PSIInfo> _SactaSPSIUsers;
		Dictionary<ushort, PSIInfo> _SactaSPVUsers;
		bool _Enabled = true; // Settings.Default.Enabled;
		bool _Disposed;
		string IdSistema;
#if _SACTASERVICE_TEST_
#else
		MySql.Data.MySqlClient.MySqlConnection ConexionCD40;
#endif
		#endregion

#if _SACTASERVICE_TEST_
        public SactaModule(string idSistema)
#else
		public SactaModule(string idSistema, MySql.Data.MySqlClient.MySqlConnection conexionCD40)
#endif
		{
			//System.Configuration.Configuration config = System.Web.Configuration.WebConfigurationManager.OpenWebConfiguration("~");

			//System.Configuration.ConfigurationSection seccionSacta = config.GetSection("sacta");

			//if (seccionSacta != null)
			//    seccionSacta. = new SnmpGestor(s.Value, this);


			//string[] sectors = data.Split(new char[] { ',', ';' }, StringSplitOptions.RemoveEmptyEntries);

			//SactaSectionHandler.SactaIpAddressConfigurationHandler ipSacta = new SactaSectionHandler.SactaIpAddressConfigurationHandler();
			//SactaSectionHandler.SactaUdpConfigurationHandler multicastSacta = new SactaSectionHandler.SactaUdpConfigurationHandler();

			//if (servicioInterfazSacta==null)
			//    servicioInterfazSacta = new Sacta.Ref_InterfazSacta.ServicioInterfazSacta();
#if !_SACTASERVICE_TEST_
			ConexionCD40 = conexionCD40;
#endif
			IdSistema = idSistema;

			_Sync = new object();
			_State = SactaState.WaitingSactaActivity;
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

			//server.Subscribe(stts.ScvModuleId, "SectResult", OnSectResult);
			//server.Subscribe(stts.ScvModuleId, "ScvActivityChanged", OnScvActivityChanged);
			//server.Subscribe(stts.ScvModuleId, "ScvPrincipalChanged", OnScvPrincipalChanged);

			_Comm = new UdpSocket[] {	new UdpSocket(SactaSectionHandler.CfgSacta.CfgIpAddress.IpRedA, SactaSectionHandler.CfgSacta.CfgSactaUdp.PuertoOrigen), 
										new UdpSocket(SactaSectionHandler.CfgSacta.CfgIpAddress.IpRedB,SactaSectionHandler.CfgSacta.CfgSactaUdp.PuertoOrigen) };
			_Comm[0].NewDataEvent += OnNewData;
			_Comm[1].NewDataEvent += OnNewData;

			_PeriodicTasks = new Timer(_PeriodicTasksInterval);
			_PeriodicTasks.AutoReset = false;
			_PeriodicTasks.Elapsed += PeriodicTasks;
		}

		public void Start()
		{
			if (_Enabled)
			{
				_Comm[0].BeginReceive();
				_Comm[1].BeginReceive();
				_PeriodicTasks.Enabled = true;
			}
		}

		public void Stop()
		{
			if (!_Disposed)
			{
				_Disposed = true;

				if (_PeriodicTasks != null)
				{
					_PeriodicTasks.Enabled = false;
					_PeriodicTasks.Close();
					_PeriodicTasks = null;
				}

				if (_Comm != null)
				{
					_Comm[0].Dispose();
					_Comm[1].Dispose();
					_Comm = null;
				}
			}
		}

#if _SACTASERVICE_TEST_
        public string State
        {
            get
            {
                return _State.ToString();
            }
        }
#endif

		#region Private Members

//		void OnSectResult(object sender, ModuleInfo info)
		//void OnSectResult(object sender, Dictionary<string, object> info)
		//{
		//    try
		//    {
		//        if (info["SectOrg"] == this)
		//        {
		//            int result = (int)info["SectResult"];
		//            uint version = (uint)info["SectVersion"];

		//            lock (_Sync)
		//            {
		//                if ((_State == SactaState.WaitingSectFinish) && (version == _TryingSectVersion))
		//                {
		//                    _State = SactaState.SendingPresences;
		//                    SendSectAnswer(version, result);
		//                }
		//            }
		//        }
		//    }
		//    catch (Exception ex)
		//    {
		//        if (!_Disposed)
		//        {
		//            // _Logger.ErrorException(Resources.OnSectResultError, ex);
		//        }
		//    }
		//}

//		void OnScvActivityChanged(object sender, ModuleInfo info)
		//void OnScvActivityChanged(object sender, Dictionary<string,object> info)
		//{
		//    try
		//    {
		//        _ScvActivityState = (byte)info["ScvActivity"];
		//    }
		//    catch (Exception ex)
		//    {
		//        if (!_Disposed)
		//        {
		//            // _Logger.ErrorException(Resources.OnScvActivityChangedError, ex);
		//        }
		//    }
		//}

//		void OnScvPrincipalChanged(object sender, ModuleInfo info)
		//void OnScvPrincipalChanged(object sender, Dictionary<string,object> info)
		//{
		//    try
		//    {
		//        _ScvPrincipal = (byte)info["ScvPrincipal"];
		//    }
		//    catch (Exception ex)
		//    {
		//        if (!_Disposed)
		//        {
		//            // _Logger.ErrorException(Resources.OnScvPrincipalChangedError, ex);
		//        }
		//    }
		//}

		void OnNewData(object sender, DataGram dg)
		{
			MemoryStream ms = new MemoryStream(dg.Data);
			CustomBinaryFormatter bf = new CustomBinaryFormatter();
			SactaMsg msg = bf.Deserialize<SactaMsg>(ms);

			try
			{
				int net = (sender == _Comm[0] ? 0 : 1);

				if (IsValid(msg))
				{
					_LastSactaReceived[net] = DateTime.Now;

					switch (msg.Type)
					{
						case SactaMsg.MsgType.Presence:
							// _Logger.Debug("Recibida presencia SACTA de la red {0}", net);

							_ActivityTimeOut = (uint)(((SactaMsg.PresenceInfo)(msg.Info)).ActivityTimeOutSg * 1000);
							_PresenceInterval = (uint)(((SactaMsg.PresenceInfo)(msg.Info)).PresencePerioditySg * 1000);
							break;
						case SactaMsg.MsgType.Sectorization:
							bool skip = true;
							uint version = ((SactaMsg.SectInfo)(msg.Info)).Version;

							// _Logger.Debug("Recibida petición de sectorización desde SACTA (Red = {0}, Versión = {1}", net, version);

							lock (_Sync)
							{
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
										_TryingSectVersion = version;
										skip = false;
									}
								}
							}

							if (!skip)
							{
								ProcessSectorization(msg);
							}
							else
							{
								// _Logger.Debug("Descartando sectorización de SACTA");
							}

							break;
					}
				}
			}
			catch (Exception )
			{
				if (!_Disposed)
				{
					// _Logger.ErrorException(Resources.SactaDataError, ex);
					const int Error = 1;
					uint version = ((SactaMsg.SectInfo)(msg.Info)).Version;
					//Settings stts = Settings.Default;
					//			ModuleInfo info = new ModuleInfo();
#if !_SACTASERVICE_TEST_
					CD40.BD.SactaInfo info = new CD40.BD.SactaInfo();
#else
                    Dictionary<string, object> info = new Dictionary<string, object>();
#endif
					info["SectVersion"] = version;
					info["Resultado"] = Error;

					OnResultSectorizacion(info);
				}
			}
		}

		void PeriodicTasks(object sender, ElapsedEventArgs e)
		{
			try
			{
				lock (_Sync)
				{
					int activityState = ((uint)((DateTime.Now - _LastSactaReceived[0]).TotalMilliseconds) < _ActivityTimeOut ? 1 : 0);
					activityState |= ((uint)((DateTime.Now - _LastSactaReceived[1]).TotalMilliseconds) < _ActivityTimeOut ? 2 : 0);

					if (activityState != _ActivityState)
					{
						//if (activityState == 0)
						//{
						//    // _Logger.Warn(Resources.SactaActivityTimeOut, (_ActivityState & 1) != 0 ? _LastSactaReceived[0] : _LastSactaReceived[1]);
						//}
						//else
						//{
						//    // _Logger.Info(Resources.ChangingActivityState, activityState);
						//}

						_ActivityState = activityState;

						// ModuleInfo info = new ModuleInfo();
						Dictionary<string, object> info = new Dictionary<string, object>();

						info["SactaActivity"] = (byte)_ActivityState;
						info["SactaAEP"] = _EndPoint[0];
						info["SactaBEP"] = _EndPoint[1];
						General.AsyncSafeLaunchEvent(SactaActivityChanged, this, info);
					}

					if (_ActivityState == 0)
					{
						_State = SactaState.WaitingSactaActivity;
					}
					else
					{
						//if (_State == SactaState.WaitingSactaActivity)
						//{
						//    _State = SactaState.WaitingIOLActivity;
						//}

						//int scvPrincipal = _ScvPrincipal;
						//if ((scvPrincipal == 0xFF) || ((_ScvActivityState & (1 << scvPrincipal)) == 0))
						//{
						//    _State = SactaState.WaitingIOLActivity;
						//}
						//else
						{
//							if (/*(_State == SactaState.WaitingIOLActivity) ||*/
							if ((_State == SactaState.WaitingSactaActivity) ||
								((_State == SactaState.WaitingSectorization) &&
								((uint)((DateTime.Now - _BeginOfWaitForSect).TotalMilliseconds) > _WaitForSectTimeOut)))
							{
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
			}
			catch (Exception )
			{
				if (!_Disposed)
				{
					// _Logger.ErrorException(Resources.PeriodicTasksError, ex);
				}
			}
			finally
			{
				if (!_Disposed)
				{
					_PeriodicTasks.Enabled = true;
				}
			}
		}

		bool IsValid(SactaMsg msg)
		{
			Dictionary<ushort, PSIInfo> validUsers = msg.Type == SactaMsg.MsgType.Sectorization ? _SactaSPSIUsers : _SactaSPVUsers;
			//Settings stts = Settings.Default;
			
			return ((msg.DomainOrg == SactaSectionHandler.CfgSacta.CfgSactaDominio.Destino) && (msg.DomainDst == SactaSectionHandler.CfgSacta.CfgSactaDominio.Origen) &&
					(msg.CenterOrg == SactaSectionHandler.CfgSacta.CfgSactaCentro.Destino) && (msg.CenterDst == SactaSectionHandler.CfgSacta.CfgSactaCentro.Origen) &&
					(msg.UserDst == SactaSectionHandler.CfgSacta.CfgSactaUsuarioSettings.Origen) && validUsers.ContainsKey(msg.UserOrg));
		}

		bool IsSecondSectMsg(SactaMsg msg)
		{
			PSIInfo psi = _SactaSPSIUsers[msg.UserOrg];
			if ((psi.LastSectMsgId == msg.Id) && (psi.LastSectVersion == ((SactaMsg.SectInfo)(msg.Info)).Version))
			{
				return true;
			}

			psi.LastSectMsgId = msg.Id;
			psi.LastSectVersion = ((SactaMsg.SectInfo)(msg.Info)).Version;

			return false;
		}

		void ProcessSectorization(SactaMsg msg)
		{
			const int Error = 1;

			StringBuilder str = new StringBuilder();
			SactaMsg.SectInfo sactaSect = (SactaMsg.SectInfo)(msg.Info);
			//Settings stts = Settings.Default;
//			ModuleInfo info = new ModuleInfo();
#if !_SACTASERVICE_TEST_
			CD40.BD.SactaInfo info = new CD40.BD.SactaInfo();
#else
            Dictionary<string, object> info = new Dictionary<string, object>();
#endif

			info["SectVersion"] = sactaSect.Version;
			//info["SectOrg"] = this;
            List<SactaMsg.SectInfo.SectorInfo> listaSectores = new List<SactaMsg.SectInfo.SectorInfo>();

			foreach (SactaMsg.SectInfo.SectorInfo sector in sactaSect.Sectors)
			{
                listaSectores.Add(sector);
            }
             
            listaSectores.Sort(delegate (SactaMsg.SectInfo.SectorInfo X, SactaMsg.SectInfo.SectorInfo Y)
            {
                if (Convert.ToInt32(X.SectorCode) < Convert.ToInt32(Y.SectorCode))
                    return -1;
                if (Convert.ToInt32(X.SectorCode) > Convert.ToInt32(Y.SectorCode))
                    return 1;

                return 0;
            });
//				if (!stts.SactaSectors.Contains(sector.SectorCode))

            List<int> controlSectoresRepetidos = new List<int>();
            foreach (SactaMsg.SectInfo.SectorInfo sector in listaSectores)
            {
                if (! SactaSectionHandler.CfgSacta.CfgSactaUsuarioSectores.IdSectores.Contains(sector.SectorCode))
				    {
					    // _Logger.Warn(Resources.UnknownSectorError, sector.SectorCode);

					    info["Resultado"] = Error;
					    OnResultSectorizacion(info);

					    return;
				    }
                if (controlSectoresRepetidos.Exists(n => n == Convert.ToInt32(sector.SectorCode)))
                {
                    info["Resultado"] = Error;
                    OnResultSectorizacion(info);

                    return;
                }

                controlSectoresRepetidos.Add(Convert.ToInt32(sector.SectorCode));

			    str.Append(string.Format("{0},{1};", sector.SectorCode, sector.Ucs));
			}

			// Añadir sectores de mantenimiento
			//CD40.BD.Procedimientos p = new CD40.BD.Procedimientos();
#if !_SACTASERVICE_TEST_
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
			//System.Diagnostics.Debug.Assert(false);

			try
			{
				Ref_Service.ServiciosCD40 s = new Sacta.Ref_Service.ServiciosCD40();
				s.ComunicaSectorizacionActiva(IdSistema, "SACTA", ref fechaActivacion);
			}
			catch (Exception e)
			{
				System.Diagnostics.Debug.Assert(false, e.Message);
			}
#endif
		}

#if !_SACTASERVICE_TEST_
		void OnResultSectorizacion(CD40.BD.SactaInfo info)
#else
        void OnResultSectorizacion(Dictionary<string, object> info)
#endif
		{
			try
			{
				int result = (int)info["Resultado"];
				uint version = (uint)info["SectVersion"];

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
			catch (Exception )
			{
				if (!_Disposed)
				{
					// _Logger.ErrorException(Resources.OnSectResultError, ex);
				}
			}
		}

		void SendInit()
		{
			Debug.Assert(_ActivityState != 0);

			// _Logger.Debug("Enviando mensaje Init a SACTA");
			if ((_ActivityState & 0x1) == 0x1) _Comm[0].Send(_EndPoint[0], _InitMsg);
			if ((_ActivityState & 0x2) == 0x2) _Comm[1].Send(_EndPoint[1], _InitMsg);

			_SeqNum = 0;
		}

		void SendSectAsk()
		{
			Debug.Assert(_ActivityState != 0);

			// _Logger.Debug("Pidiendo sectorización activa a SACTA");
			if ((_ActivityState & 0x1) == 0x1) _Comm[0].Send(_EndPoint[0], _SectAskMsg);
			if ((_ActivityState & 0x2) == 0x2) _Comm[1].Send(_EndPoint[1], _SectAskMsg);

			_BeginOfWaitForSect = DateTime.Now;
		}

		void SendSectAnswer(uint version, int result)
		{
			Debug.Assert(_ActivityState != 0);

			_SectAnswerMsg[13] = (byte)_SeqNum++;
			_SectAnswerMsg[24] = (byte)(result == 0 ? 1 : 0);
			Array.Copy(BitConverter.GetBytes(version), 0, _SectAnswerMsg, 20, 4);
			Array.Reverse(_SectAnswerMsg, 20, 4);

			// _Logger.Debug("Enviando resultado de la sectorización a SACTA");
			if ((_ActivityState & 0x1) == 0x1) _Comm[0].Send(_EndPoint[0], _SectAnswerMsg);
			if ((_ActivityState & 0x2) == 0x2) _Comm[1].Send(_EndPoint[1], _SectAnswerMsg);
		}

      void SendPresence()
      {
         Debug.Assert(_ActivityState != 0);

         _PresenceMsg[13] = (byte)_SeqNum++;

         // _Logger.Debug("Enviando mensaje de presencia a SACTA");
         if ((_ActivityState & 0x1) == 0x1) _Comm[0].Send(_EndPoint[0], _PresenceMsg);
         if ((_ActivityState & 0x2) == 0x2) _Comm[1].Send(_EndPoint[1], _PresenceMsg);

         _LastPresenceSended = DateTime.Now;
      }

		#endregion
	}
}
