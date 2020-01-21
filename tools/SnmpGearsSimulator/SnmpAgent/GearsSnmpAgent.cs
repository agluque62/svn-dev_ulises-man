using System;
using System.Net;
using System.Linq;
using System.Text;
using System.Globalization;
using System.Collections.Generic;
using System.Threading;

using NLog;
using Lextm.SharpSnmpLib;
using Lextm.SharpSnmpLib.Pipeline;
using Lextm.SharpSnmpLib.Messaging;
using Lextm.SharpSnmpLib.Security;
using Lextm.SharpSnmpLib.Objects;


namespace RcsHfSimulator.HfSnmp
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
                        SnmpAgent.Trap(Variable.Id, _data, _trapsEps);
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
            get { return _data.ToString(); }
            set
            {
                if (Value != value)
                {
                    _data = new OctetString(value);
                    if ((_trapsEps != null) && (_trapsEps.Length > 0))
                    {
                        SnmpAgent.Trap(Variable.Id, _data, _trapsEps);
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
                SnmpAgent.Trap(Variable.Id, data, _trapsEps);
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
    class SnmpLogger : Lextm.SharpSnmpLib.Pipeline.ILogger
    {
        /// <summary>
        /// 
        /// </summary>
        private static readonly Logger _logger = LogManager.GetCurrentClassLogger();
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
            if (_logger.IsTraceEnabled) 
            {                
                _logger.Trace(GetLogEntry(context));
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        private static string GetLogEntry(ISnmpContext context)
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
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="variables"></param>
        /// <returns></returns>
        private static string GetStem(ICollection<Variable> variables)
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
    class SnmpSetMessageHandler : IMessageHandler
    {
        public event Action<ISnmpContext> PduSetReceived = delegate { };

        public void Handle(ISnmpContext context, ObjectStore store)
        {
            try
            {
                PduSetReceived(context);                
            }
            catch (Exception) { }
            finally
            {
                setHandler.Handle(context, store);
            }
        }

        SetMessageHandler setHandler = new SetMessageHandler();
    }

    /// <summary>
    /// 
    /// </summary>
	static class SnmpAgent
	{
        /// <summary>
        /// 
        /// </summary>
        public static event Action<string, ISnmpData, IPEndPoint, IPEndPoint> TrapReceived = delegate { };
        public static event Action<string, ISnmpData> PduSetReceived = delegate { };
        /// <summary>
        /// 
        /// </summary>
		public static SynchronizationContext Context
		{
			get { return _context; }
			set { _context = value; }
		}

        /// <summary>
        /// 
        /// </summary>
		public static ObjectStore Store
		{
			get { return SnmpAgent._store; }
		}

        /// <summary>
        /// 
        /// </summary>
        /// <param name="ip"></param>
		public static void Init(string ip)
		{
			Init(ip, null, 161, 162);
		}

        /// <summary>
        /// 
        /// </summary>
        /// <param name="ip"></param>
        /// <param name="trapMcastIp"></param>
        /// <param name="port"></param>
        /// <param name="trap"></param>
        static TrapV1MessageHandler trapv1 = new TrapV1MessageHandler();
        static TrapV2MessageHandler trapv2 = new TrapV2MessageHandler();
        static InformRequestMessageHandler inform = new InformRequestMessageHandler();
        /// <summary>
        /// 
        /// </summary>
        static GetMessageHandler getHandler = new GetMessageHandler();
        static SnmpSetMessageHandler setHandler = new SnmpSetMessageHandler();
        /// <summary>
        /// 
        /// </summary>
        /// <param name="ip"></param>
        /// <param name="trapMcastIp"></param>
        /// <param name="port"></param>
        /// <param name="trap"></param>
        public static void Init(string ip, string trapMcastIp, int port, int trap)
		{
			SnmpLogger logger = new SnmpLogger();
			ObjectStore objectStore = new ObjectStore();

			OctetString getCommunityPublic = new OctetString("public");                         // 
			OctetString setCommunityPublic = new OctetString("public");
            OctetString getCommunityPrivate = new OctetString("private");                       
            OctetString setCommunityPrivate = new OctetString("private");

            /** */
            trapv1 = new TrapV1MessageHandler();
            trapv2 = new TrapV2MessageHandler();
            inform = new InformRequestMessageHandler();

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
                ,new HandlerMapping("v2,v3", "GET", /*new GetMessageHandler()*/ getHandler)
                ,new HandlerMapping("v1", "SET", new SetV1MessageHandler())
                ,new HandlerMapping("v2,v3", "SET", /*new SetMessageHandler()*/ setHandler)
                ,new HandlerMapping("v1", "GETNEXT", new GetNextV1MessageHandler())
                ,new HandlerMapping("v2,v3", "GETNEXT", new GetNextMessageHandler())
                ,new HandlerMapping("v2,v3", "GETBULK", new GetBulkMessageHandler())
                ,new HandlerMapping("v1", "TRAPV1", trapv1)
                ,new HandlerMapping("v2,v3", "TRAPV2", trapv2)
                ,new HandlerMapping("v2,v3", "INFORM", inform)
                //,new HandlerMapping("*", "*", NullMessageHandler)
			};
			MessageHandlerFactory messageHandlerFactory = new MessageHandlerFactory(handlerMappings);

			User[] users = new User[]
			{
				new User(new OctetString("neither"), DefaultPrivacyProvider.DefaultPair),
				new User(new OctetString("authen"), new DefaultPrivacyProvider(new MD5AuthenticationProvider(new OctetString("authentication")))),
				new User(new OctetString("privacy"), new DESPrivacyProvider(new OctetString("privacyphrase"), new MD5AuthenticationProvider(new OctetString("authentication"))))
			};
			UserRegistry userRegistry = new UserRegistry(users);

			EngineGroup engineGroup = new EngineGroup();
			Listener listener = new Listener() { Users = userRegistry };
			SnmpApplicationFactory factory = new SnmpApplicationFactory(logger, objectStore, composedMembershipProvider, messageHandlerFactory);

			_engine = new SnmpEngine(factory, listener, engineGroup);
			_engine.Listener.AddBinding(new IPEndPoint(IPAddress.Parse(ip), port));
			_engine.Listener.AddBinding(new IPEndPoint(IPAddress.Parse(ip), trap));

            //// TRAP's de los Recursos...
            //IPEndPoint[] RecEP = new IPEndPoint[]
            //{
            //    new IPEndPoint(IPAddress.Parse(ip), 16211),new IPEndPoint(IPAddress.Parse(ip), 16212),
            //    new IPEndPoint(IPAddress.Parse(ip), 16213),new IPEndPoint(IPAddress.Parse(ip), 16214),
            //    new IPEndPoint(IPAddress.Parse(ip), 16221),new IPEndPoint(IPAddress.Parse(ip), 16222),
            //    new IPEndPoint(IPAddress.Parse(ip), 16223),new IPEndPoint(IPAddress.Parse(ip), 16224),
            //    new IPEndPoint(IPAddress.Parse(ip), 16231),new IPEndPoint(IPAddress.Parse(ip), 16232),
            //    new IPEndPoint(IPAddress.Parse(ip), 16233),new IPEndPoint(IPAddress.Parse(ip), 16234),
            //    new IPEndPoint(IPAddress.Parse(ip), 16241),new IPEndPoint(IPAddress.Parse(ip), 16242),
            //    new IPEndPoint(IPAddress.Parse(ip), 16243),new IPEndPoint(IPAddress.Parse(ip), 16244)
            //};
            //foreach (IPEndPoint ep in RecEP)
            //    _engine.Listener.AddBinding(ep);
            //--------------------------------------------------------------------------------
            // _engine.Listener.AddBinding(new IPEndPoint(IPAddress.Any, trap));

            _engine.ExceptionRaised += (sender, e) => _logger.Error(e.Exception, "ERROR Snmp");
			_closed = false;
			_store = objectStore;
			_context = SynchronizationContext.Current ?? new SynchronizationContext();

            trapv1.MessageReceived += TrapV1Received;
			trapv2.MessageReceived += TrapV2Received;
			inform.MessageReceived += InformRequestReceived;

            setHandler.PduSetReceived += setHandler_PduSetReceived;

		}

        /// <summary>
        /// 
        /// </summary>
		public static void Start()
		{
			_engine.Start();
		}

        /// <summary>
        /// 
        /// </summary>
		public static void Close()
		{
            TrapReceived = delegate { };
            PduSetReceived = delegate { };

            _closed = true;
			_engine.Stop();
			_engine.Dispose();
		}

        /// <summary>
        /// 
        /// </summary>
        /// <param name="ep"></param>
        /// <param name="oid"></param>
        /// <param name="handler"></param>
		public static void GetValueAsync(IPEndPoint ep, string oid, Action<ISnmpData> handler)
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
		public static void GetValueAsync(IPEndPoint ep, string oid, Action<ISnmpData> handler, int timeout)
		{
			ThreadPool.QueueUserWorkItem(delegate 
			{
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
                    _logger.Error("SnmpAgent::GetValueAsync", x.Message);
                    _logger.Trace(x,"SnmpAgent::GetValueAsync");
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
		public static void SetValueAsync(IPEndPoint ep, string oid, ISnmpData data, Action<ISnmpData> handler)
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
		public static void SetValueAsync(IPEndPoint ep, string oid, ISnmpData data, Action<ISnmpData> handler, int timeout)
		{
			ThreadPool.QueueUserWorkItem(delegate
			{
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
                    _logger.Error("SnmpAgent::SetValueAsync", x.Message);
                    _logger.Trace(x,"SnmpAgent::SetValueAsync");
                }
			});
		}

        /// <summary>
        /// 
        /// </summary>
        /// <param name="ep"></param>
        /// <param name="vList"></param>
        /// <param name="handler"></param>
        public static void GetAsync(IPEndPoint ep, IList<Variable> vList, Action<IPEndPoint, IList<Variable>> handler)
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
		public static void GetAsync(IPEndPoint ep, IList<Variable> vList, Action<IPEndPoint, IList<Variable>> handler, int timeout)
		{
			ThreadPool.QueueUserWorkItem(delegate
			{
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
                    _logger.Error("SnmpAgent::GetAsync", x.Message);
                    _logger.Trace(x,"SnmpAgent::GetAsync");               
                }
			});
		}

        /// <summary>
        /// 
        /// </summary>
        /// <param name="oid"></param>
        /// <param name="data"></param>
        /// <param name="eps"></param>
		public static void Trap(string oid, ISnmpData data, params IPEndPoint[] eps)
		{
			Trap(new ObjectIdentifier(oid), data, eps);
		}

        /// <summary>
        /// 
        /// </summary>
        /// <param name="oid"></param>
        /// <param name="data"></param>
        /// <param name="eps"></param>
		public static void Trap(ObjectIdentifier oid, ISnmpData data, params IPEndPoint[] eps)
		{
			ThreadPool.QueueUserWorkItem(delegate
			{
				try
				{
					List<Variable> vList = new List<Variable> { new Variable(oid, data) };

					foreach (IPEndPoint ep in eps)
					{
						Messenger.SendTrapV2(0, VersionCode.V2, ep, 
                            new OctetString("private"),
                            oid, // ???
                            0, vList);
                        //Messenger.SendTrapV2(0, VersionCode.V2, ep, new OctetString("private"), new ObjectIdentifier("TODO"), 0, vList);
                    }
				}
				catch (Exception x)
				{
                    _logger.Error("SnmpAgent::Trap", x.Message);
                    _logger.Trace(x,"SnmpAgent::Trap");
                }
			});
		}

		#region Private Members
        /// <summary>
        /// 
        /// </summary>
		private static Logger _logger = LogManager.GetCurrentClassLogger();
        /// <summary>
        /// 
        /// </summary>
		private static SnmpEngine _engine;
		private static ObjectStore _store;
		private static SynchronizationContext _context;
		private static bool _closed;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
		private static void TrapV1Received(object sender, TrapV1MessageReceivedEventArgs e)
		{
			_context.Post(delegate
			{
				if (!_closed)
				{
					var pdu = e.TrapV1Message.Pdu();
					// if (pdu.ErrorStatus.ToInt32() == 0)
					{
						foreach (var v in pdu.Variables)
						{
							TrapReceived(v.Id.ToString(), v.Data, e.Sender, e.Binding.Endpoint);
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
		private static void TrapV2Received(object sender, TrapV2MessageReceivedEventArgs e)
		{            
			_context.Post(delegate
			{
				if (!_closed)
				{
					var pdu = e.TrapV2Message.Pdu();
                    //if (pdu.ErrorStatus.ToInt32() == 0)
					{
						foreach (var v in pdu.Variables)
						{
							TrapReceived(v.Id.ToString(), v.Data, e.Sender, e.Binding.Endpoint);
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
		private static void InformRequestReceived(object sender, InformRequestMessageReceivedEventArgs e)
		{
			_context.Post(delegate
			{
				if (!_closed)
				{
					var pdu = e.InformRequestMessage.Pdu();
					// if (pdu.ErrorStatus.ToInt32() == 0)
					{
						foreach (var v in pdu.Variables)
						{
							TrapReceived(v.Id.ToString(), v.Data, e.Sender, e.Binding.Endpoint);
						}
					}
				}
			}, "SnmpAgent.InformReceived");
		}

        /// <summary>
        /// 
        /// </summary>
        /// <param name="context"></param>
        private static void setHandler_PduSetReceived(ISnmpContext context)
        {
            _context.Post(delegate
            {
                if (!_closed)
                {
                    var pdu = context.Request.Scope.Pdu;
                    {
                        foreach (var v in pdu.Variables)
                        {
                            // TrapReceived(v.Id.ToString(), v.Data, e.Sender, e.Binding.Endpoint);
                            PduSetReceived(v.Id.ToString(), v.Data);
                        }
                    }
                }
            }, "SnmpAgent.setHandler_PduSetReceived");
        }

		#endregion
	}
}
