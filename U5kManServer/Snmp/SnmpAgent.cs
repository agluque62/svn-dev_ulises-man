using System;
using System.Net;
using System.Linq;
using System.Text;
using System.Globalization;
using System.Collections.Generic;
using System.Threading;

using Lextm.SharpSnmpLib;
using Lextm.SharpSnmpLib.Pipeline;
using Lextm.SharpSnmpLib.Messaging;
using Lextm.SharpSnmpLib.Security;
using Lextm.SharpSnmpLib.Objects;

using U5kManMibRevC;

using NucleoGeneric;
namespace U5kManServer
{
    /// <summary>
    /// 
    /// </summary>
    public class MibObject : ScalarObject, IComparable
    {
        string _oid = "";
        public override ISnmpData Data { get; set; }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public int CompareTo(object obj)
        {
            MibObject orderToCompare = obj as MibObject;
            return string.Compare(_oid, orderToCompare._oid);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="oid"></param>
        public MibObject(string oid)
            : base(oid)
        {
            _oid = oid;
            // LogManager.GetCurrentClassLogger().Info("OID: {0}", oid);
        }

        public String Oid { get { return _oid; } }
        
    }
    /// <summary>
    /// 
    /// </summary>
    sealed class SnmpIntObject : MibObject      // ScalarObject
    {
        /// <summary>
        /// 
        /// </summary>
        public int Value
        {
            get { return _data.ToInt32(); }
            set
            {
                if (Value != value)
                {
                    _data = new Integer32(value);

                    if ((_trapsEps != null) && (_trapsEps.Length > 0))
                    {
                        SnmpAgent.Trap(Variable.Id.ToString(), Variable.Id.ToString(), _data, _trapsEps);
                    }
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public override ISnmpData Data
        {
            get { return _data; }
            set
            {
                if (value == null)
                {
                    throw new ArgumentNullException("value");
                }

                _data = (Integer32)value;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="oid"></param>
        /// <param name="value"></param>
        /// <param name="trapsEps"></param>
        public SnmpIntObject(string oid, int value, params IPEndPoint[] trapsEps)
            : base(oid)
        {
            _data = new Integer32(value);
            _trapsEps = trapsEps;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="oid"></param>
        /// <returns></returns>
        public static SnmpIntObject Get(string oid)
        {
            return SnmpAgent.Store.GetObject(new ObjectIdentifier(oid)) as SnmpIntObject;
        }

        #region Private Members
        /// <summary>
        /// 
        /// </summary>
        private Integer32 _data;
        private IPEndPoint[] _trapsEps;

        #endregion
    }
    /// <summary>
    /// 
    /// </summary>
    sealed class SnmpStringObject : MibObject   // ScalarObject
    {
        /// <summary>
        /// 
        /// </summary>
        public string Value
        {
            get 
            {
                //NLog.LogManager.GetLogger("SnmpStringObject").Debug("Desde Value Get {0}", _data.ToString());
                return _data.ToString(); 
            }
            set
            {
                if (Value != value)
                {
                    _data = new OctetString(value);
                    if ((_trapsEps != null) && (_trapsEps.Length > 0))
                    {
                        SnmpAgent.Trap(Variable.Id.ToString(), Variable.Id.ToString(), _data, _trapsEps);
                    }
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="value"></param>
        public void SendTrap(string value)
        {
            OctetString data = new OctetString(value);

            if ((_trapsEps != null) && (_trapsEps.Length > 0))
                SnmpAgent.Trap(Variable.Id.ToString(), Variable.Id.ToString(), data, _trapsEps);
        }

        /// <summary>
        /// 
        /// </summary>
        public override ISnmpData Data
        {
            get { return _data; }
            set
            {
                if (value == null)
                {
                    throw new ArgumentNullException("value");
                }

                _data = (OctetString)value;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="oid"></param>
        /// <param name="value"></param>
        /// <param name="trapsEps"></param>
        public SnmpStringObject(string oid, string value, params IPEndPoint[] trapsEps)
            : base(oid)
        {
            _data = new OctetString(value);
            _trapsEps = trapsEps;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="oid"></param>
        /// <returns></returns>
        public static SnmpStringObject Get(string oid)
        {
            return SnmpAgent.Store.GetObject(new ObjectIdentifier(oid)) as SnmpStringObject;
        }

        #region Private Members

        /// <summary>
        /// 
        /// </summary>
        private Lextm.SharpSnmpLib.OctetString _data;
        private IPEndPoint[] _trapsEps;

        #endregion
    }

    /// <summary>
    /// 
    /// </summary>
    class SnmpLogger : BaseCode, Lextm.SharpSnmpLib.Pipeline.ILogger
    {
        private const string Empty = "-";

        /// <summary>
        /// 
        /// </summary>
        public SnmpLogger()
        {
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="context"></param>
        public void Log(ISnmpContext context)
        {
            SnmpAgent.Statictics.ActualizeFromAgent(context.Request, context.Response as ResponseMessage);
            LogTrace<SnmpLogger>( GetLogEntry(context));
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        private /*static*/ string GetLogEntry(ISnmpContext context)
        {
            return string.Format(
                 CultureInfo.InvariantCulture,
                 "{0}-{1}:{2} {3} {4}", // {5} {6} {7} {8}", // {9}",
                 context.Request.TypeCode() == SnmpType.Unknown ? Empty : context.Request.TypeCode().ToString(),
                 context.Sender.Address,
                 context.Binding.Endpoint.Port,
                 context.Request.Parameters.UserName,
                 GetStem(context.Request.Pdu().Variables),
                 // DateTime.UtcNow,
                 context.Binding.Endpoint.Address,
                 (context.Response == null) ? Empty : context.Response.Pdu().ErrorStatus.ToErrorCode().ToString(),
                 context.Request.Version,
                 DateTime.Now.Subtract(context.CreatedTime).TotalMilliseconds);

            /*
            return string.Format(
                 CultureInfo.InvariantCulture,
                 "{0} {1} {2} {3} {4} {5} {6} {7} {8}", // {9}",
                 // DateTime.UtcNow,
                 context.Binding.Endpoint.Address,
                 context.Request.TypeCode() == SnmpType.Unknown ? Empty : context.Request.TypeCode().ToString(),
                 GetStem(context.Request.Pdu().Variables),
                 context.Binding.Endpoint.Port,
                 context.Request.Parameters.UserName,
                 context.Sender.Address,
                 (context.Response == null) ? Empty : context.Response.Pdu().ErrorStatus.ToErrorCode().ToString(),
                 context.Request.Version,
                 DateTime.Now.Subtract(context.CreatedTime).TotalMilliseconds);
             * */
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="variables"></param>
        /// <returns></returns>
        private /*static*/ string GetStem(ICollection<Variable> variables)
        {
            if (variables.Count == 0)
            {
                return Empty;
            }

            StringBuilder result = new StringBuilder();
            foreach (Variable v in variables)
            {
                result.AppendFormat("{0};", v.Id);
            }

            if (result.Length > 0)
            {
                result.Length--;
            }

            return result.ToString();
        }
    }
    /// <summary>
    /// 
    /// </summary>
	/*static*/ class SnmpAgent : BaseCode
    {
        /// <summary>
        /// 
        /// </summary>
        // public /*static*/ event Action<string, ISnmpData, IPEndPoint, IPEndPoint> TrapReceived = delegate { };
        public /*static*/ event Action<string, string, ISnmpData, IPEndPoint, IPEndPoint> TrapReceived = delegate { };
        /// <summary>
        /// 
        /// </summary>
		public /*static*/ SynchronizationContext Context
		{
			get { return _context; }
			set { _context = value; }
		}
        /// <summary>
        /// 
        /// </summary>
        private static ObjectStore _store;
        public  static ObjectStore Store { get => _store; }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="ip"></param>
		public /*static*/ void Init(string ip)
		{
			// Init(ip, null, 161, 162);
            CreateAgent(ip);
		}
        /// <summary>
        /// 
        /// </summary>
        /// <param name="ip"></param>
        /// <param name="trapMcastIp"></param>
        /// <param name="port"></param>
        /// <param name="trap"></param>
        /*static*/ TrapV1MessageHandler trapv1 = new TrapV1MessageHandler();
        /*static*/ TrapV2MessageHandler trapv2 = new TrapV2MessageHandler();
        /*static*/ InformRequestMessageHandler inform = new InformRequestMessageHandler();
        /// <summary>
        /// 
        /// </summary>
        /// <param name="ip"></param>
        /// <param name="trapMcastIp"></param>
        /// <param name="port"></param>
        /// <param name="trap"></param>
        public /*static*/ void Init(string ip1, string trapMcastIp, int port, int trap)
		{
            string v2v3 = "v2,v3"; // "v2,v3";
			SnmpLogger logger = new SnmpLogger();
			ObjectStore objectStore = new ObjectStore();

			OctetString getCommunityPublic = new OctetString("public");
			OctetString setCommunityPublic = new OctetString("public");
            OctetString getCommunityPrivate = new OctetString("private");
            OctetString setCommunityPrivate = new OctetString("private");
            /***/
            trapv1 = new TrapV1MessageHandler();
            trapv2 = new TrapV2MessageHandler();
            inform = new InformRequestMessageHandler();
            /**/
			IMembershipProvider[] membershipProviders = new IMembershipProvider[]
			{
				// new Version1MembershipProvider(getCommunity, setCommunity),
				new Version2MembershipProvider(getCommunityPublic, setCommunityPublic),
				new Version2MembershipProvider(getCommunityPrivate,setCommunityPublic),
				new Version3MembershipProvider()
			};
			IMembershipProvider composedMembershipProvider = new ComposedMembershipProvider(membershipProviders);
            /**/
			HandlerMapping[] handlerMappings = new HandlerMapping[]
			{
				new HandlerMapping("v1", "GET", new GetV1MessageHandler())
                ,new HandlerMapping(v2v3, "GET", new GetMessageHandler())
                ,new HandlerMapping("v1", "SET", new SetV1MessageHandler())
                ,new HandlerMapping(v2v3, "SET", new SetMessageHandler())
                ,new HandlerMapping("v1", "GETNEXT", new GetNextV1MessageHandler())
                ,new HandlerMapping(v2v3, "GETNEXT", new GetNextMessageHandler())
                ,new HandlerMapping(v2v3, "GETBULK", new GetBulkMessageHandler())
                ,new HandlerMapping("v1", "TRAPV1", trapv1)
                ,new HandlerMapping(v2v3, "TRAPV2", trapv2)
                ,new HandlerMapping(v2v3, "INFORM", inform)
                //,new HandlerMapping("*", "*", NullMessageHandler)
			};
			MessageHandlerFactory messageHandlerFactory = new MessageHandlerFactory(handlerMappings);

			User[] users = new User[]
			{
				new User(new OctetString("nucleodf"), DefaultPrivacyProvider.DefaultPair),
				new User(new OctetString("nucleodfa"), new DefaultPrivacyProvider(new MD5AuthenticationProvider(new OctetString("ulisesv5000")))),
				new User(new OctetString("nucleodfap"), new DESPrivacyProvider(new OctetString("privacyphrase"), new MD5AuthenticationProvider(new OctetString("ulisesv5000"))))
			};
			UserRegistry userRegistry = new UserRegistry(users);

			EngineGroup engineGroup = new EngineGroup();
			Listener listener = new Listener() { Users = userRegistry };
			SnmpApplicationFactory factory = new SnmpApplicationFactory(logger, objectStore, composedMembershipProvider, messageHandlerFactory);

			_engine = new SnmpEngine(factory, listener, engineGroup);

            //_engine.Listener.AddBinding(new IPEndPoint(IPAddress.Parse(ip), port));
            //_engine.Listener.AddBinding(new IPEndPoint(IPAddress.Parse(ip), trap));
            _engine.Listener.AddBinding(new IPEndPoint(IPAddress.Any, port));
            _engine.Listener.AddBinding(new IPEndPoint(IPAddress.Any, trap));

            // TRAP's de los Recursos...
            IPEndPoint[] RecEP = new IPEndPoint[]
            {
                new IPEndPoint(IPAddress.Any, 16211),new IPEndPoint(IPAddress.Any, 16212),
                new IPEndPoint(IPAddress.Any, 16213),new IPEndPoint(IPAddress.Any, 16214),
                new IPEndPoint(IPAddress.Any, 16221),new IPEndPoint(IPAddress.Any, 16222),
                new IPEndPoint(IPAddress.Any, 16223),new IPEndPoint(IPAddress.Any, 16224),
                new IPEndPoint(IPAddress.Any, 16231),new IPEndPoint(IPAddress.Any, 16232),
                new IPEndPoint(IPAddress.Any, 16233),new IPEndPoint(IPAddress.Any, 16234),
                new IPEndPoint(IPAddress.Any, 16241),new IPEndPoint(IPAddress.Any, 16242),
                new IPEndPoint(IPAddress.Any, 16243),new IPEndPoint(IPAddress.Any, 16244)
            };
            foreach (IPEndPoint ep in RecEP)
                _engine.Listener.AddBinding(ep);
            //--------------------------------------------------------------------------------
            // _engine.Listener.AddBinding(new IPEndPoint(IPAddress.Any, trap));
            _engine.ExceptionRaised += (sender, e) => LogException<SnmpAgent>("", e.Exception);
			_closed = false;
			_store = objectStore;
			_context = SynchronizationContext.Current ?? new SynchronizationContext();

            /** */
            trapv1.MessageReceived += TrapV1Received;
			trapv2.MessageReceived += TrapV2Received;
			inform.MessageReceived += InformRequestReceived;

			// OJO.. (new IPEndPoint(IPAddress.Parse(ip), 0)).SetAsDefault();
		}
        /// <summary>
        /// 
        /// </summary>
        /// <param name="ip"></param>
        public void CreateAgent(string ip)
        {
            Properties.u5kManServer cfg = Properties.u5kManServer.Default;
            string snmpVersion = cfg.Snmp_AgentVersion.ToLower()=="v3" ? "v3" : "v2";    // Por defecto será v2
            SnmpLogger logger = new SnmpLogger();
            ObjectStore objectStore = new ObjectStore();
            /** */
            HandlerMapping[] handlerMappings;
            IMembershipProvider[] membershipProviders;
            Listener listener;

            trapv1 = new TrapV1MessageHandler();
            trapv2 = new TrapV2MessageHandler();
            inform = new InformRequestMessageHandler();
            /** */
            trapv1.MessageReceived += TrapV1Received;
            trapv2.MessageReceived += TrapV2Received;
            inform.MessageReceived += InformRequestReceived;

            if (cfg.Snmp_AgentVersion.ToLower() == "v2") // Configuracion V2
            {
                membershipProviders = new IMembershipProvider[]
			    {
                    new Version1MembershipProvider(new OctetString(cfg.Snmp_V2AgentGetComm), new OctetString(cfg.Snmp_V2AgentSetComm)),
				    new Version2MembershipProvider(new OctetString(cfg.Snmp_V2AgentGetComm), new OctetString(cfg.Snmp_V2AgentSetComm)),
                    new Version2MembershipProvider(new OctetString("private"), new OctetString("private"))
			    };
                listener = new Listener();
                handlerMappings = new HandlerMapping[]
			    {
                     new HandlerMapping(snmpVersion, "GET",     new GetMessageHandler())
                    ,new HandlerMapping(snmpVersion, "SET",     new SetMessageHandler())
                    ,new HandlerMapping(snmpVersion, "GETNEXT", new GetNextMessageHandler())
                    ,new HandlerMapping(snmpVersion, "GETBULK", new GetBulkMessageHandler())
                    ,new HandlerMapping(snmpVersion, "TRAPV2",  trapv2)
                    ,new HandlerMapping(snmpVersion, "INFORM",  inform)
                    ,new HandlerMapping("v1", "GET", new GetV1MessageHandler())
                    ,new HandlerMapping("v1", "GETNEXT", new GetNextV1MessageHandler())
			    };
            }
            else // Configuracion V3.
            {
                membershipProviders = new IMembershipProvider[]
			    {
    				new Version3MembershipProvider()
			    };
                List<User> users = new List<User>();
                foreach (string user_cif in cfg.Snmp_V3Users)
                {
                    string user_str = Utilities.EncryptionHelper.CAE_descifrar(user_cif);
                    string[] user_param = user_str.Split(new string[]{"##"}, StringSplitOptions.None);
                    if (user_param.Length == 4)
                    {
                        OctetString username = new OctetString(user_param[0]);
                        IPrivacyProvider profile = user_param[1] == "1" ? new DefaultPrivacyProvider(new MD5AuthenticationProvider(new OctetString(user_param[2]))) :
                            user_param[1] == "2" ? new DESPrivacyProvider(new OctetString(user_param[3]), new MD5AuthenticationProvider(new OctetString(user_param[2]))) :
                            DefaultPrivacyProvider.DefaultPair;
                        users.Add(new User(username, profile));
                    }
                }
                if (users.Count == 0)
                    users.Add(new User(new OctetString("dfnucleo"), new DefaultPrivacyProvider(new MD5AuthenticationProvider(new OctetString("ulisesv5000")))));
                //User[] users1 = new User[]
                //{
                //    new User(new OctetString("nucleodf"), DefaultPrivacyProvider.DefaultPair),
                //    new User(new OctetString("nucleodfa"), new DefaultPrivacyProvider(new MD5AuthenticationProvider(new OctetString("ulisesv5000")))),
                //    new User(new OctetString("nucleodfap"), new DESPrivacyProvider(new OctetString("privacyphrase"), new MD5AuthenticationProvider(new OctetString("ulisesv5000"))))
                //};
                UserRegistry userRegistry = new UserRegistry(users.ToArray());
                listener = new Listener() { Users = userRegistry };
                handlerMappings = new HandlerMapping[]
			        {
                         new HandlerMapping(snmpVersion, "GET",     new GetMessageHandler(){})
                        ,new HandlerMapping(snmpVersion, "SET",     new SetMessageHandler())
                        ,new HandlerMapping(snmpVersion, "GETNEXT", new GetNextMessageHandler())
                        ,new HandlerMapping(snmpVersion, "GETBULK", new GetBulkMessageHandler())
                        ,new HandlerMapping(snmpVersion, "TRAPV2",  trapv2)
                        ,new HandlerMapping(snmpVersion, "INFORM",  inform)
			        };
            }
            /** */
            IMembershipProvider composedMembershipProvider = new ComposedMembershipProvider(membershipProviders);
            /** */
            MessageHandlerFactory messageHandlerFactory = new MessageHandlerFactory(handlerMappings);
            /** */
            EngineGroup engineGroup = new EngineGroup();
            SnmpApplicationFactory factory = new SnmpApplicationFactory(logger, objectStore, composedMembershipProvider, messageHandlerFactory);
            _engine = new SnmpEngine(factory, listener, engineGroup);
            _engine.Listener.AddBinding(new IPEndPoint(IPAddress.Any, cfg.Snmp_AgentPort));
            _engine.Listener.AddBinding(new IPEndPoint(IPAddress.Any, cfg.Snmp_AgentListenTrapPort));
            _engine.ExceptionRaised += (sender, e) => LogException<SnmpAgent>("", e.Exception);
            _closed = false;
            _store = objectStore;
            _context = SynchronizationContext.Current ?? new SynchronizationContext();
            ///** */
            //trapv1.MessageReceived += TrapV1Received;
            //trapv2.MessageReceived += TrapV2Received;
            //inform.MessageReceived += InformRequestReceived;
        }
        /// <summary>
        /// 
        /// </summary>
		public /*static*/ void Start()		{
			_engine.Start();
		}
        /// <summary>
        /// 
        /// </summary>
		public /*static*/ void Close()
		{
            try
            {
                TrapReceived = delegate { };

                if (_engine != null && _engine.Active)
                {
                    _engine.Stop();
                    _engine.Dispose();
                }
            }
            catch(Exception x)
            {
                LogException<SnmpAgent>("", x);
            }
            finally
            {
                _engine = null;
                _closed = true;
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="ep"></param>
        /// <param name="oid"></param>
        /// <param name="handler"></param>
		public /*static*/ void GetValueAsync(IPEndPoint ep, string oid, Action<ISnmpData> handler)
		{
			GetValueAsync(ep,oid,handler,2000);
		}
        /// <summary>
        /// 
        /// </summary>
        /// <param name="ep"></param>
        /// <param name="oid"></param>
        /// <param name="handler"></param>
        /// <param name="timeout"></param>
		public /*static*/ void GetValueAsync(IPEndPoint ep, string oid, Action<ISnmpData> handler, int timeout)
		{
			ThreadPool.QueueUserWorkItem(delegate 
			{
                U5kGenericos.TraceCurrentThread(this.GetType().Name + " GetValueAsync");
                
                List<Variable> vList = new List<Variable> { new Variable(new ObjectIdentifier(oid)) };

				try
				{
					IList<Variable> value = Messenger.Get(VersionCode.V2, ep, new OctetString("public"), vList, timeout);
					if ((value.Count == 1) && (value[0].Data.TypeCode != SnmpType.NoSuchInstance))
					{
						_context.Post(delegate 
						{
							if (!_closed)
							{
								handler(value[0].Data);
							}
						}, "SnmpAgent.ValueGetted");
					}
				}
				catch (Exception x) 
                {
                    LogException<SnmpAgent>( "", x);
                }
			});
		}
        /// <summary>
        /// 
        /// </summary>
        /// <param name="ep"></param>
        /// <param name="oid"></param>
        /// <param name="data"></param>
        /// <param name="handler"></param>
		public /*static*/ void SetValueAsync(IPEndPoint ep, string oid, ISnmpData data, Action<ISnmpData> handler)
		{
			SetValueAsync(ep, oid, data, handler, 4000);
		}
        /// <summary>
        /// 
        /// </summary>
        /// <param name="ep"></param>
        /// <param name="oid"></param>
        /// <param name="data"></param>
        /// <param name="handler"></param>
        /// <param name="timeout"></param>
		public /*static*/ void SetValueAsync(IPEndPoint ep, string oid, ISnmpData data, Action<ISnmpData> handler, int timeout)
		{
			ThreadPool.QueueUserWorkItem(delegate
			{
                U5kGenericos.TraceCurrentThread(this.GetType().Name + " SetValueAsync");

                List<Variable> vList = new List<Variable> { new Variable(new ObjectIdentifier(oid), data) };

				try
				{
					IList<Variable> value = Messenger.Set(VersionCode.V2, ep, new OctetString("private"), vList, timeout);
					if ((value.Count == 1) && (value[0].Data.TypeCode != SnmpType.NoSuchInstance))
					{
						_context.Post(delegate 
						{
							if (!_closed)
							{
								handler(value[0].Data);
							}
						}, "SnmpAgent.ValueSetted");
					}
				}
				catch (Exception x) 
                {
                    LogException<SnmpAgent>( "", x);
                }
			});
		}
        /// <summary>
        /// 
        /// </summary>
        /// <param name="ep"></param>
        /// <param name="vList"></param>
        /// <param name="handler"></param>
        public /*static*/ void GetAsync(IPEndPoint ep, IList<Variable> vList, Action<IPEndPoint, IList<Variable>> handler)
		{
			GetAsync(ep, vList, handler, 4000);
		}
        /// <summary>
        /// 
        /// </summary>
        /// <param name="ep"></param>
        /// <param name="vList"></param>
        /// <param name="handler"></param>
        /// <param name="timeout"></param>
		public /*static*/ void GetAsync(IPEndPoint ep, IList<Variable> vList, Action<IPEndPoint, IList<Variable>> handler, int timeout)
		{
			ThreadPool.QueueUserWorkItem(delegate
			{
                U5kGenericos.TraceCurrentThread(this.GetType().Name + " GetAsync");
                try
				{
					IList<Variable> results = Messenger.Get(VersionCode.V2, ep, new OctetString("public"), vList, timeout);
					_context.Post(delegate
					{
						if (!_closed)
						{
							handler(ep, results);
						}
					}, "SnmpAgent.GetResult");
				}
				catch (Exception x) 
                {
                    LogException<SnmpAgent>( "", x);               
                }
			});
		}
        /// <summary>
        /// 
        /// </summary>
        /// <param name="oid"></param>
        /// <param name="data"></param>
        /// <param name="eps"></param>
		public static void Trap(string oidtrap, string oidvar, ISnmpData data, params IPEndPoint[] eps)
		{
            var vList = new List<Variable> { new Variable(new ObjectIdentifier(oidvar), data) };
            Trap(new ObjectIdentifier(oidtrap), vList, eps);
		}
        /// <summary>
        /// 
        /// </summary>
        /// <param name="oid"></param>
        /// <param name="data"></param>
        /// <param name="eps"></param>
		public static void Trap(ObjectIdentifier oidtrap, IList<Variable> vList, params IPEndPoint[] eps)
		{
			ThreadPool.QueueUserWorkItem(delegate
			{
                U5kGenericos.TraceCurrentThread("SnmpAgent Trap");
                try
				{
					foreach (IPEndPoint ep in eps)
					{
						Messenger.SendTrapV2(0, VersionCode.V2, ep, new OctetString("private"), oidtrap, 0, vList);
					}
				}
				catch (Exception x)
				{
                    LogException<SnmpAgent>( "", x);
                }
			});
		}

		#region Private Members
        /// <summary>
        /// 
        /// </summary>
        //private /*static*/ Logger _logger = LogManager.GetCurrentClassLogger();
        /// <summary>
        /// 
        /// </summary>
		private /*static*/ SnmpEngine _engine = null;
		private /*static*/ SynchronizationContext _context;
		private /*static*/ bool _closed;
        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
		private /*static*/ void TrapV1Received(object sender, TrapV1MessageReceivedEventArgs e)
		{
            U5kGenericos.TraceCurrentThread(this.GetType().Name + " trapv1 received");
            _context.Post(delegate
			{
				if (!_closed)
				{
					var pdu = e.TrapV1Message.Pdu();
					// if (pdu.ErrorStatus.ToInt32() == 0)
					{
						foreach (var v in pdu.Variables)
						{
							TrapReceived(e.TrapV1Message.Enterprise.ToString(), v.Id.ToString(), v.Data, e.Sender, e.Binding.Endpoint);
						}
					}
				}
			}, "SnmpAgent.TrapV1Received");
		}
        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
		private /*static*/ void TrapV2Received(object sender, TrapV2MessageReceivedEventArgs e)
		{
            U5kGenericos.TraceCurrentThread(this.GetType().Name + " trapv2 received");
            _context.Post(delegate
			{
				if (!_closed)
				{
					var pdu = e.TrapV2Message.Pdu();
                    //if (pdu.ErrorStatus.ToInt32() == 0)
					{
						foreach (var v in pdu.Variables)
						{
							TrapReceived(e.TrapV2Message.Enterprise.ToString(), v.Id.ToString(), v.Data, e.Sender, e.Binding.Endpoint);
						}
					}
				}
			}, "SnmpAgent.TrapV2Received");
		}
        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
		private /*static*/ void InformRequestReceived(object sender, InformRequestMessageReceivedEventArgs e)
		{
            U5kGenericos.TraceCurrentThread(this.GetType().Name + " Inform request received ");
            _context.Post(delegate
			{
				if (!_closed)
				{
					var pdu = e.InformRequestMessage.Pdu();
					// if (pdu.ErrorStatus.ToInt32() == 0)
					{
						foreach (var v in pdu.Variables)
						{
							TrapReceived(e.InformRequestMessage.Enterprise.ToString(), v.Id.ToString(), v.Data, e.Sender, e.Binding.Endpoint);
						}
					}
				}
			}, "SnmpAgent.InformReceived");
		}

        #endregion

        #region Estadisticas.
        public class SnmpStatictics
        {
            public Int32 SnmpInPkts { get; set; }                   // Contadores de Agente y Cliente...
            public Int32 SnmpOutPkts { get; set; }

            public Int32 SnmpInBadVersions { get; set; }            // Contadores de Cliente Pendientes....
            public Int32 SnmpInBadCommunityNames { get; set; }
            public Int32 SnmpInBadCommunityUses { get; set; }
            public Int32 SnmpInASNParseErrs { get; set; }

            public Int32 SnmpInTooBigs { get; set; }                // Contadores de Cliente
            public Int32 SnmpInNoSuchNames { get; set; }
            public Int32 SnmpInBadValues { get; set; }
            public Int32 SnmpInReadOnlys { get; set; }
            public Int32 SnmpInGenErrs { get; set; }

            public Int32 SnmpInTotalReqVars { get; set; }           // Contadores de Agente
            public Int32 SnmpInTotalSetVars { get; set; }
            public Int32 SnmpInGetRequests { get; set; }
            public Int32 SnmpInGetNexts { get; set; }
            public Int32 SnmpInSetRequests { get; set; }

            public Int32 SnmpInGetResponses { get; set; }           // Contador de Cliente

            public Int32 SnmpInTraps { get; set; }                  // Contadores de Agente.
            public Int32 SnmpOutTooBigs { get; set; }
            public Int32 SnmpOutNoSuchNames { get; set; }
            public Int32 SnmpOutBadValues { get; set; }
            public Int32 SnmpOutGenErrs { get; set; }

            public Int32 SnmpOutGetRequests { get; set; }           // Contadores de Cliente
            public Int32 SnmpOutGetNexts { get; set; }
            public Int32 SnmpOutSetRequests { get; set; }

            public Int32 SnmpOutGetResponses { get; set; }          // Contador de Agente

            public Int32 SnmpOutTraps { get; set; }                 // Contadore de Cliente

            public Int32 SnmpEnableAuthenTraps { get; set; }

            public SnmpStatictics()
            {
                SnmpInPkts = SnmpOutPkts = SnmpInBadVersions = SnmpInBadCommunityNames = SnmpInBadCommunityUses = 0;
                SnmpInASNParseErrs = SnmpInTooBigs = SnmpInNoSuchNames = SnmpInBadValues = SnmpInReadOnlys = 0;
                SnmpInGenErrs = SnmpInTotalReqVars = SnmpInTotalSetVars = SnmpInGetRequests = SnmpInGetNexts = 0;
                SnmpInSetRequests = SnmpInGetResponses = SnmpInTraps = SnmpOutTooBigs = SnmpOutNoSuchNames = 0;
                SnmpOutBadValues = SnmpOutGenErrs = SnmpOutGetRequests = SnmpOutGetNexts = SnmpOutSetRequests = 0;
                SnmpOutGetResponses = SnmpOutTraps = 0;
                SnmpEnableAuthenTraps = 2;
            }
            public void ActualizeFromAgent(ISnmpMessage request, ResponseMessage response = null)
            {
                if (request != null)
                {
                    if (response != null)
                    {
                        // Actualizo Estadisticas de Respuestas... (Out)
                        SnmpOutPkts++;
                        SnmpOutGetResponses++;
                        
                        // Actualizo Estadisticas de Errores... (In)
                        switch (response.ErrorStatus)
                        {
                            case ErrorCode.TooBig:
                                SnmpOutTooBigs++;
                                break;
                            case ErrorCode.NoSuchName:      // Solo en SNMP V1
                                SnmpOutNoSuchNames++;
                                break;
                            case ErrorCode.BadValue:        // Solo en SNMP V1
                                SnmpOutBadValues++;
                                break;
                            case ErrorCode.NoAccess:        // Captura estos errores en V2-V3
                            case ErrorCode.GenError:
                                SnmpOutGenErrs++;
                                break;
                            case ErrorCode.NoError:         // En V2-V3 NoSuchName Es una respuesta normal.
                                if ((ISnmpData)(response.Scope.Pdu.Variables[0].Data) is NoSuchInstance)
                                    SnmpOutNoSuchNames++;
                                break;
                        }
                    }

                    // Actualizo Estadisticas de Requests..(In)
                    SnmpInPkts++;
                    switch (request.TypeCode())
                    {
                        case SnmpType.GetRequestPdu:
                            SnmpInGetRequests++;
                            SnmpInTotalReqVars += request.Scope.Pdu.Variables.Count;
                            break;
                        case SnmpType.GetNextRequestPdu:
                            SnmpInGetNexts++;
                            SnmpInTotalReqVars += request.Scope.Pdu.Variables.Count;
                            break;
                        case SnmpType.SetRequestPdu:
                            SnmpInSetRequests++;
                            SnmpInTotalSetVars += request.Scope.Pdu.Variables.Count;
                            break;
                        case SnmpType.TrapV1Pdu:
                        case SnmpType.TrapV2Pdu:
                        case SnmpType.InformRequestPdu:
                            SnmpInTraps++;
                            break;

                        case SnmpType.GetBulkRequestPdu:
                            break;
                    }
                }
            }
            public void ActualizeFromClient(ISnmpMessage request, ResponseMessage response=null)
            {
                if (response != null)
                {
                    SnmpInPkts++;
                    SnmpInGetResponses++;
                    switch (response.ErrorStatus)
                    {
                        case ErrorCode.TooBig:
                            SnmpInTooBigs++;
                            break;
                        case ErrorCode.NoSuchName:
                            SnmpInNoSuchNames++;
                            break;
                        case ErrorCode.BadValue:
                            SnmpInBadValues++;
                            break;
                        case ErrorCode.ReadOnly:
                            SnmpInReadOnlys++;
                            break;
                        case ErrorCode.GenError:
                            SnmpInGenErrs++;
                            break;
                    }
                }
                SnmpOutPkts++;
                switch (request.TypeCode())
                {
                    case SnmpType.GetRequestPdu:
                        SnmpOutGetRequests++;
                        break;
                    case SnmpType.GetNextRequestPdu:
                        SnmpOutGetNexts++;
                        break;
                    case SnmpType.SetRequestPdu:
                        SnmpOutSetRequests++;
                        break;
                    case SnmpType.TrapV1Pdu:
                    case SnmpType.TrapV2Pdu:
                    case SnmpType.InformRequestPdu:
                        SnmpOutTraps++;
                        break;

                    case SnmpType.GetBulkRequestPdu:
                        break;
                }
            }
        };
        static private readonly SnmpStatictics snmpStatictics = new SnmpStatictics();
        static public SnmpStatictics Statictics { get => snmpStatictics; }
        #endregion Estadisticas

        #region Client Interface

        /** Para la Creacion de contadores de respuesta */
        public static bool UseFullRange { get; set; } = true;
        private static readonly Lazy<NumberGenerator> RequestCounterFullRange = new Lazy<NumberGenerator>(() => new NumberGenerator(int.MinValue, int.MaxValue));
        private static readonly Lazy<NumberGenerator> RequestCounterPositive = new Lazy<NumberGenerator>(() => new NumberGenerator(0, int.MaxValue));
        private static NumberGenerator RequestCounter
        {
            get { return UseFullRange ? RequestCounterFullRange.Value : RequestCounterPositive.Value; }
        }
        public static IList<Variable> ClientRequest(
            SnmpType request,
            VersionCode version, 
            IPEndPoint endpoint, 
            OctetString community, 
            IList<Variable> variables, int timeout,
            ObjectIdentifier oidTrap=null)
        {
            if (endpoint == null)
            {
                throw new ArgumentNullException(nameof(endpoint));
            }
            if (community == null)
            {
                throw new ArgumentNullException(nameof(community));
            }
            if (variables == null)
            {
                throw new ArgumentNullException(nameof(variables));
            }
            if (version == VersionCode.V3)
            {
                throw new NotSupportedException("SNMP v3 is not supported");
            }
            ISnmpMessage message;
            switch (request)
            {
                case SnmpType.GetRequestPdu:
                    message = new GetRequestMessage(RequestCounter.NextId, version, community, variables);
                    break;
                case SnmpType.SetRequestPdu:
                    message = new SetRequestMessage(RequestCounter.NextId, version, community, variables);
                    break;

                case SnmpType.TrapV1Pdu:
                case SnmpType.TrapV2Pdu:
                    message = new TrapV2Message(0, version, community, oidTrap, 0, variables);
                    message.Send(endpoint);
                    Statictics.ActualizeFromClient(message);
                    return null;
                default:
                    throw new NotSupportedException("SNMP v3 is not supported");
            }

            ResponseMessage response = null;
            Exception exception = null;
            try
            {
                response = message.GetResponse(timeout, endpoint) as ResponseMessage;
            }
            catch (SnmpException x)
            {
                exception = x;
            }
            Statictics.ActualizeFromClient(message, response);
            if (exception == null )
            {
                if (response.ErrorStatus == ErrorCode.NoError)
                    return response.Pdu().Variables;
                exception = ErrorException.Create("Error in response", endpoint.Address, response);
            }
            throw exception;
        }

        #endregion Client Interface.
    }
}
