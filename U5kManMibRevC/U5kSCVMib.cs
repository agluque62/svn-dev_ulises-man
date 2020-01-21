using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Dynamic;
using Lextm.SharpSnmpLib;
using Lextm.SharpSnmpLib.Pipeline;
using Lextm.SharpSnmpLib.Objects;

namespace U5kManMibRevC
{
    /// <summary>
    /// 
    /// </summary>
    internal class U5kSCVMib : U5kiMIb
    {
        public U5kSCVMib(AgentDataGet scvDataGet, string oidBase= ".1.3.6.1.4.1.7916") 
        {
            DataGet = scvDataGet;
            OidBase = oidBase;
            LoadGroups();
        }
        protected override void LoadGroups()
        {
            mibGroups = new List<U5kMibGroup>()
            {
                new U5kSCVMibCfgGroup(),
                new U5kSCVMibStdGroup(),
                new U5kSCVMibPosGroup(),
                new U5kSCVMibGwsGroup(),
                new U5kSCVMibExtGroup(),
                new U5kSCVMibPbxGroup(),
                new U5kSCVMibRadGroup(),
                new U5kSCVMibQtyGroup()
            };
        }
        public static AgentDataGet DataGet;
        public static string OidBase;
    }
    /// <summary>
    /// Grupo de Configuracion
    /// </summary>
    class U5kSCVMibCfgGroup : U5kMibGroup
    {
        /** Clases de los elementos del grupo */
        internal sealed class CfgServerDual : ScalarObject
        {
            /// <summary>
            /// Initializes a new instance of the <see cref="CfgServerDual"/> class.
            /// </summary>
            public CfgServerDual()
                : base(OidBase + ".8.1.5.1.1.0")
            {
            }
            /// <summary>
            /// 
            /// </summary>
            public override ISnmpData Data
            {
                get
                {
                    int data = 0;
                    U5kSCVMib.DataGet((object gdata) =>
                    {
                        dynamic ddata = gdata;
                        data = ddata.STDG.bDualServ == true ? 1 : 0;
                    });
                    return new Integer32(data);
                }
                set { throw new AccessFailureException(); }
            }
        }
        internal sealed class CfgPbx : ScalarObject
        {
            /// <summary>
            /// Initializes a new instance of the <see cref="CfgPbx"/> class.
            /// </summary>
            public CfgPbx()
                : base(OidBase + ".8.1.5.1.2.0")
            {
            }
            /// <summary>
            /// 
            /// </summary>
            public override ISnmpData Data
            {
                get
                {
                    int data = 0;
                    U5kSCVMib.DataGet((object gdata) =>
                    {
                        dynamic ddata = gdata;
                        data = ddata.STDG.HayPbx == true ? 1 : 0;
                    });
                    return new Integer32(data);
                }
                set { throw new AccessFailureException(); }
            }
        }
        internal sealed class CfgSacta : ScalarObject
        {
            /// <summary>
            /// Initializes a new instance of the <see cref="CfgSacta"/> class.
            /// </summary>
            public CfgSacta()
                : base(OidBase + ".8.1.5.1.3.0")
            {
            }
            /// <summary>
            /// 
            /// </summary>
            public override ISnmpData Data
            {
                get
                {
                    int data = 0;
                    U5kSCVMib.DataGet((object gdata) =>
                    {
                        dynamic ddata = gdata;
                        data = ddata.STDG.HaySacta == true ? 1 : 0;
                    });
                    return new Integer32(data);
                }
                set { throw new AccessFailureException(); }
            }
        }
        internal sealed class CfgNtpServer : ScalarObject
        {
            /// <summary>
            /// Initializes a new instance of the <see cref="CfgPbx"/> class.
            /// </summary>
            public CfgNtpServer()
                : base(OidBase + ".8.1.5.1.4.0")
            {
            }
            /// <summary>
            /// 
            /// </summary>
            public override ISnmpData Data
            {
                get
                {
                    int data = 0;
                    U5kSCVMib.DataGet((object gdata) =>
                    {
                        dynamic ddata = gdata;
                        data = ddata.STDG.HayReloj == true ? 1 : 0;
                    });
                    return new Integer32(data);
                }
                set { throw new AccessFailureException(); }
            }
        }
        /// <summary>
        /// 
        /// </summary>
        protected override void LoadElements()
        {
            snmpObjects = new List<SnmpObjectBase>()
            {
                new CfgServerDual(), new CfgPbx(), new CfgSacta(), new CfgNtpServer(),
            };
        }
        /// <summary>
        /// 
        /// </summary>
        public U5kSCVMibCfgGroup() : base(U5kSCVMib.OidBase)
        {
        }
    }
    /// <summary>
    /// Grupo de Estado Global.
    /// </summary>
    class U5kSCVMibStdGroup : U5kMibGroup
    {
        /** Clases de los elementos del grupo */
        internal sealed class StdVersion : ScalarObject
        {
            /// <summary>
            /// Initializes a new instance of the <see cref="StdVersion"/> class.
            /// </summary>
            public StdVersion()
                : base(OidBase + ".8.1.5.2.1.0")
            {
            }
            /// <summary>
            /// 
            /// </summary>
            public override ISnmpData Data
            {
                get
                {
                    string data = string.Empty;
                    U5kSCVMib.DataGet((object gdata) =>
                    {
                        dynamic ddata = gdata;
                        data = ddata.STDG.ProgramVersion;
                    });
                    return new OctetString(data);
                }
                set { throw new AccessFailureException(); }
            }
        }
        internal sealed class StdCfgact : ScalarObject
        {
            /// <summary>
            /// Initializes a new instance of the <see cref="StdCfgact"/> class.
            /// </summary>
            public StdCfgact()
                : base(OidBase + ".8.1.5.2.2.0")
            {
            }
            /// <summary>
            /// 
            /// </summary>
            public override ISnmpData Data
            {
                get
                {
                    string data = string.Empty;
                    U5kSCVMib.DataGet((object gdata) =>
                    {
                        dynamic ddata = gdata;
                        data = ddata.STDG.BdtConfVersion;
                    });
                    return new OctetString(data);
                }
                set { throw new AccessFailureException(); }
            }
        }
        internal sealed class StdgTableLen : ScalarObject
        {
            /// <summary>
            /// Initializes a new instance of the <see cref="StdgTableLen"/> class.
            /// </summary>
            public StdgTableLen()
                : base(OidBase + ".8.1.5.2.3.1.0")
            {
            }
            /// <summary>
            /// 
            /// </summary>
            public override ISnmpData Data
            {
                get { return new Integer32(8); }
                set { throw new AccessFailureException(); }
            }
        }       /** 8 Elementos en la Tabla */

        /** */
        internal sealed class StdgTable : TableObject
        {
            internal sealed class StdgIndice : ScalarObject
            {
                readonly ISnmpData _data;
                /// <summary>
                /// Initializes a new instance of the <see cref="StdgIndice"/> class.
                public StdgIndice(int index, object onlineData)
                    // ReSharper restore UnusedParameter.Local
                    : base(OidBase + ".8.1.5.2.3.2.1.1.{0}", index)
                {
                    _data = new Integer32(index);
                }
                public override ISnmpData Data
                {
                    get { return _data; }
                    set { throw new AccessFailureException(); }
                }
            }
            internal sealed class StdgDisp : ScalarObject
            {
                private readonly int RowIndex = 0;
                /// <summary>
                /// Initializes a new instance of the <see cref="StdgDisp"/> class.
                public StdgDisp(int index, object onlineData)
                    // ReSharper restore UnusedParameter.Local
                    : base(OidBase + ".8.1.5.2.3.2.1.2.{0}", index)
                {
                    RowIndex = index;
                }
                public override ISnmpData Data
                {
                    get
                    {
                        String DevName = "Error";
                        U5kSCVMib.DataGet((object gdata) =>
                        {
                            dynamic ddata = gdata;
                            //data = ddata.STDG.BdtConfVersion;
                            switch (RowIndex)
                            {
                                case 1:     // Servidor 1
                                    DevName = "Servidor 1";
                                    break;
                                case 2:     // Servidor 2
                                    DevName = "Servidor 2";
                                    break;
                                case 3:     // Servicio de Radio.
                                    DevName = "Servicio Radio";
                                    break;
                                case 4:     // Servicio de Telefonia.
                                    DevName = "Servicio Telefonia";
                                    break;
                                case 5:     // Pbx
                                    DevName = "Pbx";
                                    break;
                                case 6:     // NTP Server.
                                    DevName = "Servidor NTP";
                                    break;
                                case 7:     // Sacta 1
                                    DevName = "Sacta #1";
                                    break;
                                case 8:     // Sacta 2
                                    DevName = "Sacta #2";
                                    break;
                            }
                        });

                        return new OctetString(DevName);
                    }
                    set { throw new AccessFailureException(); }
                }
            }
            internal sealed class StdgIp : ScalarObject
            {
                private readonly int RowIndex = 0;
                /// <summary>
                /// Initializes a new instance of the <see cref="StdgIp"/> class.
                public StdgIp(int index, object onlineData)
                    : base(OidBase + ".8.1.5.2.3.2.1.3.{0}", index)
                {
                    RowIndex = index;
                }
                public override ISnmpData Data
                {
                    get
                    {
                        String IpInfo = "Error";
                        U5kSCVMib.DataGet((object gdata) =>
                        {
                            dynamic ddata = gdata;
                            //data = ddata.STDG
                            switch(RowIndex)
                            {
                                case 1:     // Servidor 1
                                    IpInfo = ddata.STDG.stdServ1.name;
                                    break;
                                case 2:     // Servidor 2
                                    IpInfo = ddata.STDG.stdServ1.name;
                                    break;
                                case 3:     // Servicio de Radio.
                                    IpInfo = ddata.STDG.RadioServiceIp;
                                    break;
                                case 4:     // Servicio de Telefonia.
                                    IpInfo = ddata.STDG.PhoneServiceIp;
                                    break;
                                case 5:     // Pbx
                                    IpInfo = ddata.STDG.stdPabx.name;
                                    break;
                                case 6:     // NTP Server.
                                    IpInfo = ddata.STDG.stdClock.name;
                                    break;
                                case 7:     // Sacta 1
                                    IpInfo = "LAN #01";
                                    break;
                                case 8:     // Sacta 2
                                    IpInfo = "LAN #02";
                                    break;
                            }
                        });

                        return new OctetString(IpInfo);
                    }
                    set { throw new AccessFailureException(); }
                }
            }
            internal sealed class StdgStd : ScalarObject
            {
                private readonly int RowIndex = 0;
                /// <summary>
                /// Initializes a new instance of the <see cref="StdgStd"/> class.
                public StdgStd(int index, object onlineData)
                    // ReSharper restore UnusedParameter.Local
                    : base(OidBase + ".8.1.5.2.3.2.1.4.{0}", index)
                {
                    RowIndex = index;
                }
                public override ISnmpData Data
                {
                    get
                    {
                        Int32 StdInfo = 0;
                        U5kSCVMib.DataGet((object gdata) =>
                        {
                            dynamic ddata = gdata;
                            //data = ddata.STDG.BdtConfVersion;
                            switch (RowIndex)
                            {
                                case 1:     // Servidor 1
                                    StdInfo = ddata.STDG.Server1Std;
                                    break;
                                case 2:     // Servidor 2
                                    StdInfo = ddata.STDG.Server2Std;
                                    break;
                                case 3:     // Servicio de Radio.
                                    StdInfo = ddata.STDG.RadioServiceStd;
                                    break;
                                case 4:     // Servicio de Telefonia.
                                    StdInfo = ddata.STDG.PhoneServiceStd;
                                    break;
                                case 5:     // Pbx
                                    StdInfo = (int )ddata.STDG.stdPabx.Estado;
                                    break;
                                case 6:     // NTP Server.
                                    StdInfo = (int )ddata.STDG.stdClock.Estado;
                                    break;
                                case 7:     // Sacta 1
                                    StdInfo = (int )ddata.STDG.stdSacta1;
                                    break;
                                case 8:     // Sacta 2
                                    StdInfo = (int )ddata.STDG.stdSacta2;
                                    break;
                            }
                        });

                        return new Integer32(StdInfo);
                    }
                    set { throw new AccessFailureException(); }
                }
            }
            internal sealed class StdgLans : ScalarObject
            {
                private readonly int RowIndex = 0;
                /// <summary>
                /// Initializes a new instance of the <see cref="StdgLans"/> class.
                public StdgLans(int index, object onlineData)
                    // ReSharper restore UnusedParameter.Local
                    : base(OidBase + ".8.1.5.2.3.2.1.5.{0}", index)
                {
                    RowIndex = index;
                }
                public override ISnmpData Data
                {
                    get
                    {
                        String LansInfo = "Error";
                        U5kSCVMib.DataGet((object gdata) =>
                        {
                            dynamic ddata = gdata;
                            //data = ddata.STDG.BdtConfVersion;
                            switch (RowIndex)
                            {
                                case 1:     // Servidor 1
                                    LansInfo = ddata.STDG.Server1Lans;
                                    break;
                                case 2:     // Servidor 2
                                    LansInfo = ddata.STDG.Server2Lans;
                                    break;
                                case 3:     // Servicio de Radio.
                                    LansInfo = "---";
                                    break;
                                case 4:     // Servicio de Telefonia.
                                    LansInfo = "---";
                                    break;
                                case 5:     // Pbx
                                    LansInfo = "---";
                                    break;
                                case 6:     // NTP Server.
                                    LansInfo = "---";
                                    break;
                                case 7:     // Sacta 1
                                    LansInfo = "---";
                                    break;
                                case 8:     // Sacta 2
                                    LansInfo = "---";
                                    break;
                            }
                        });

                        return new OctetString(LansInfo);
                    }
                    set { throw new AccessFailureException(); }
                }
            }
            internal sealed class StdgSync : ScalarObject
            {
                private readonly int RowIndex = 0;
                /// <summary>
                /// Initializes a new instance of the <see cref="StdgSync"/> class.
                public StdgSync(int index, object onlineData)
                    // ReSharper restore UnusedParameter.Local
                    : base(OidBase + ".8.1.5.2.3.2.1.6.{0}", index)
                {
                    RowIndex = index;
                }
                public override ISnmpData Data
                {
                    get
                    {
                        String SyncInfo = "Error";
                        U5kSCVMib.DataGet((object gdata) =>
                        {
                            dynamic ddata = gdata;
                            //data = ddata.STDG.BdtConfVersion;
                            switch (RowIndex)
                            {
                                case 1:     // Servidor 1
                                    SyncInfo = ddata.STDG.stdServ1.ntp_sync;
                                    break;
                                case 2:     // Servidor 2
                                    SyncInfo = ddata.STDG.stdServ2.ntp_sync;
                                    break;
                                case 3:     // Servicio de Radio.
                                    SyncInfo = "---";
                                    break;
                                case 4:     // Servicio de Telefonia.
                                    SyncInfo = "---";
                                    break;
                                case 5:     // Pbx
                                    SyncInfo = "---";
                                    break;
                                case 6:     // NTP Server.
                                    SyncInfo = "---";
                                    break;
                                case 7:     // Sacta 1
                                    SyncInfo = "---";
                                    break;
                                case 8:     // Sacta 2
                                    SyncInfo = "---";
                                    break;
                            }
                        });

                        return new OctetString(SyncInfo);
                    }
                    set { throw new AccessFailureException(); }
                }
            }

            /// <summary>
            /// Initializes a new instance of the <see cref="StdgTable"/> class.
            /// </summary>
            public StdgTable()
            {
                //                /** Ejemplo para programar el evento de actualizacion de la tabla */
                //                NetworkChange.NetworkAddressChanged +=
                //                    (sender, args) => LoadElements();
                //#if NET452
                //            NetworkChange.NetworkAvailabilityChanged +=
                //                (sender, args) => LoadElements();
                //#endif
                LoadElements();
            }

            // ".1.3.6.1.4.1.7916.8.1.5.2.3.2"
            private readonly IList<ScalarObject> _elements = new List<ScalarObject>();
            private void LoadElements()
            {
                _elements.Clear();
                var columnTypes = new[]
                {
                    typeof(StdgIndice),
                    typeof(StdgDisp),
                    typeof(StdgIp),
                    typeof(StdgStd),
                    typeof(StdgLans),
                    typeof(StdgSync),
                };
                foreach (var type in columnTypes)
                {
                    for (int i = 0; i < Ids.Count(); i++)
                    {
                        _elements.Add((ScalarObject)Activator.CreateInstance(type, new object[] { i + 1, Ids[i] }));
                    }
                }
            }
            protected override IEnumerable<ScalarObject> Objects
            {
                get { return _elements; }
            }

            /** Descripcion de cada Fila */
            private readonly String[] Ids = new string[] 
            {
                "Servidor 1", "Servidor 2","Servicio Radio","Servicio Telefonia",
                "Pbx", "Servidor NTP", "Sacta-1","Sacta-2"
            };
        }

        /// <summary>
        /// 
        /// </summary>
        protected override void LoadElements()
        {
            snmpObjects = new List<SnmpObjectBase>()
            {
                new StdVersion(), new StdCfgact(), new StdgTableLen(), new StdgTable(), 
            };
        }
        /// <summary>
        /// 
        /// </summary>
        public U5kSCVMibStdGroup() : base(U5kSCVMib.OidBase)
        {
        }

    }
    /// <summary>
    /// Grupo de Puestos.
    /// </summary>
    class U5kSCVMibPosGroup : U5kMibGroup
    {
        internal sealed class GlobalStd : ScalarObject
        {
            /// <summary>
            /// Initializes a new instance of the <see cref="GlobalStd"/> class.
            /// </summary>
            public GlobalStd()
                : base(OidBase + ".8.1.5.3.1.0")
            {
            }
            /// <summary>
            /// 
            /// </summary>
            public override ISnmpData Data
            {
                get
                {
                    int data = 0;
                    U5kSCVMib.DataGet((object gdata) =>
                    {
                        dynamic ddata = gdata;
                        data = (int)ddata.STDG.stdGlobalPos;
                    });
                    return new Integer32(data);
                }
                set { throw new AccessFailureException(); }
            }
        }
        internal sealed class PosTableLen : ScalarObject
        {
            /// <summary>
            /// Initializes a new instance of the <see cref="PosTableLen"/> class.
            /// </summary>
            public PosTableLen()
                : base(OidBase + ".8.1.5.3.2.1.0")
            {
            }
            /// <summary>
            /// 
            /// </summary>
            public override ISnmpData Data
            {
                get
                {
                    int data = 0;
                    U5kSCVMib.DataGet((object gdata) =>
                    {
                        dynamic ddata = gdata;
                        data = ddata.STDTOPS.Count;
                    });
                    return new Integer32(data);
                }
                set { throw new AccessFailureException(); }
            }
        }
        /** */
        internal sealed class PosTable : TableObject
        {
            internal sealed class PosIndex : ScalarObject
            {
                readonly ISnmpData _data;
                /// <summary>
                /// Initializes a new instance of the <see cref="PosIndex"/> class.
                public PosIndex(int index, object onlineData)
                    : base(OidBase + ".8.1.5.3.2.2.1.1.{0}", index)
                {
                    _data = new Integer32(index);
                }
                public override ISnmpData Data
                {
                    get { return _data; }
                    set { throw new AccessFailureException(); }
                }
            }
            internal sealed class PosDisp : ScalarObject
            {
                private readonly int RowIndex = 0;
                /// <summary>
                /// Initializes a new instance of the <see cref="PosDisp"/> class.
                public PosDisp(int index, object onlineData)
                    // ReSharper restore UnusedParameter.Local
                    : base(OidBase + ".8.1.5.3.2.2.1.2.{0}", index)
                {
                    RowIndex = index;
                }
                public override ISnmpData Data
                {
                    get
                    {
                        String DevName = "Error";
                        U5kSCVMib.DataGet((object gdata) =>
                        {
                            dynamic ddata = gdata;
                            // DevName = ((IList<dynamic>)ddata.STDTOPS).ElementAt(RowIndex-1).name;
                            DevName = ddata.STDTOPS[RowIndex-1].name;
                        });

                        return new OctetString(DevName);
                    }
                    set { throw new AccessFailureException(); }
                }
            }
            internal sealed class PosIp : ScalarObject
            {
                private readonly int RowIndex = 0;
                /// <summary>
                /// Initializes a new instance of the <see cref="PosIp"/> class.
                public PosIp(int index, object onlineData)
                    : base(OidBase + ".8.1.5.3.2.2.1.3.{0}", index)
                {
                    RowIndex = index;
                }
                public override ISnmpData Data
                {
                    get
                    {
                        String IpInfo = "Error";
                        U5kSCVMib.DataGet((object gdata) =>
                        {
                            dynamic ddata = gdata;
                            IpInfo = ddata.STDTOPS[RowIndex - 1].ip;
                        });

                        return new OctetString(IpInfo);
                    }
                    set { throw new AccessFailureException(); }
                }
            }
            internal sealed class PosStd : ScalarObject
            {
                private readonly int RowIndex = 0;
                /// <summary>
                /// Initializes a new instance of the <see cref="PosStd"/> class.
                public PosStd(int index, object onlineData)
                    // ReSharper restore UnusedParameter.Local
                    : base(OidBase + ".8.1.5.3.2.2.1.4.{0}", index)
                {
                    RowIndex = index;
                }
                public override ISnmpData Data
                {
                    get
                    {
                        Int32 StdInfo = 0;
                        U5kSCVMib.DataGet((object gdata) =>
                        {
                            dynamic ddata = gdata;
                            StdInfo = (int )ddata.STDTOPS[RowIndex - 1].stdpos;
                            //data = ddata.STDG.BdtConfVersion;
                        });
                        return new Integer32(StdInfo);
                    }
                    set { throw new AccessFailureException(); }
                }
            }
            internal sealed class PosLans : ScalarObject
            {
                private readonly int RowIndex = 0;
                /// <summary>
                /// Initializes a new instance of the <see cref="PosLans"/> class.
                public PosLans(int index, object onlineData)
                    // ReSharper restore UnusedParameter.Local
                    : base(OidBase + ".8.1.5.3.2.2.1.5.{0}", index)
                {
                    RowIndex = index;
                }
                public override ISnmpData Data
                {
                    get
                    {
                        String LansInfo = "Error";
                        U5kSCVMib.DataGet((object gdata) =>
                        {
                            dynamic ddata = gdata;
                            int lan1 = (int)ddata.STDTOPS[RowIndex - 1].lan1;
                            int lan2 = (int)ddata.STDTOPS[RowIndex - 1].lan2;
                            LansInfo = ddata.STDG.PosLans(lan1, lan2);
                        });

                        return new OctetString(LansInfo);
                    }
                    set { throw new AccessFailureException(); }
                }
            }
            internal sealed class PosSync : ScalarObject
            {
                private readonly int RowIndex = 0;
                /// <summary>
                /// Initializes a new instance of the <see cref="PosSync"/> class.
                public PosSync(int index, object onlineData)
                    // ReSharper restore UnusedParameter.Local
                    : base(OidBase + ".8.1.5.3.2.2.1.6.{0}", index)
                {
                    RowIndex = index;
                }
                public override ISnmpData Data
                {
                    get
                    {
                        String SyncInfo = "Error";
                        U5kSCVMib.DataGet((object gdata) =>
                        {
                            dynamic ddata = gdata;
                            SyncInfo = ddata.STDTOPS[RowIndex - 1].status_sync;
                        });

                        return new OctetString(SyncInfo);
                    }
                    set { throw new AccessFailureException(); }
                }
            }
            internal sealed class PosUris : ScalarObject
            {
                private readonly int RowIndex = 0;
                /// <summary>
                /// Initializes a new instance of the <see cref="PosUris"/> class.
                public PosUris(int index, object onlineData)
                    // ReSharper restore UnusedParameter.Local
                    : base(OidBase + ".8.1.5.3.2.2.1.7.{0}", index)
                {
                    RowIndex = index;
                }
                public override ISnmpData Data
                {
                    get
                    {
                        String UrisInfo = "Error";
                        U5kSCVMib.DataGet((object gdata) =>
                        {
                            dynamic ddata = gdata;
                            List<string> uris = ddata.STDTOPS[RowIndex - 1].uris;
                            UrisInfo = uris == null ? "null" : String.Join("##", uris.ToArray());
                        });

                        return new OctetString(UrisInfo);
                    }
                    set { throw new AccessFailureException(); }
                }
            }

            /// <summary>
            /// Initializes a new instance of the <see cref="PosTable"/> class.
            /// </summary>
            public PosTable()
            {
                //                /** Ejemplo para programar el evento de actualizacion de la tabla */
                //                NetworkChange.NetworkAddressChanged +=
                //                    (sender, args) => LoadElements();
                //#if NET452
                //            NetworkChange.NetworkAvailabilityChanged +=
                //                (sender, args) => LoadElements();
                //#endif
                LoadElements();
            }

            // ".1.3.6.1.4.1.7916.8.1.5.2.3.2"
            private readonly IList<ScalarObject> _elements = new List<ScalarObject>();
            private void LoadElements()
            {
                _elements.Clear();
                var columnTypes = new[]
                {
                    typeof(PosIndex),
                    typeof(PosDisp),
                    typeof(PosIp),
                    typeof(PosStd),
                    typeof(PosLans),
                    typeof(PosSync),
                    typeof(PosUris),
                };
                int count = 0;
                U5kSCVMib.DataGet((object gdata) =>
                {
                    dynamic ddata = gdata;
                    count = ddata.STDTOPS.Count;
                });

                foreach (var type in columnTypes)
                {
                    for (int i = 0; i < count; i++)
                    {
                        _elements.Add((ScalarObject)Activator.CreateInstance(type, new object[] { i + 1, null }));
                    }
                }
            }
            protected override IEnumerable<ScalarObject> Objects
            {
                get { return _elements; }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        protected override void LoadElements()
        {
            snmpObjects = new List<SnmpObjectBase>()
            {
                new GlobalStd(), new PosTableLen(), new PosTable()
            };
        }
        /// <summary>
        /// 
        /// </summary>
        public U5kSCVMibPosGroup(): base(U5kSCVMib.OidBase)
        {
        }
    }
    /// <summary>
    /// Grupo de Pasarelas
    /// </summary>
    class U5kSCVMibGwsGroup : U5kMibGroup
    {
        internal sealed class GlobalStd : ScalarObject
        {
            /// <summary>
            /// Initializes a new instance of the <see cref="GlobalStd"/> class.
            /// </summary>
            public GlobalStd()
                : base(OidBase + ".8.1.5.4.1.0")
            {
            }
            /// <summary>
            /// 
            /// </summary>
            public override ISnmpData Data
            {
                get
                {
                    int data = 0;
                    U5kSCVMib.DataGet((object gdata) =>
                    {
                        dynamic ddata = gdata;
                        data = (int)ddata.STDG.stdScv1.Estado;
                    });
                    return new Integer32(data);
                }
                set { throw new AccessFailureException(); }
            }
        }
        /** */
        internal sealed class GwsTableLen : ScalarObject
        {
            /// <summary>
            /// Initializes a new instance of the <see cref="GwsTableLen"/> class.
            /// </summary>
            public GwsTableLen()
                : base(OidBase + ".8.1.5.4.2.1.0")
            {
            }
            /// <summary>
            /// 
            /// </summary>
            public override ISnmpData Data
            {
                get
                {
                    int data = 0;
                    U5kSCVMib.DataGet((object gdata) =>
                    {
                        dynamic ddata = gdata;
                        data = ddata.STDGWS.Count;
                    });
                    return new Integer32(data);
                }
                set { throw new AccessFailureException(); }
            }
        }
        internal sealed class GwsTable : TableObject
        {
            internal sealed class GwIndex : ScalarObject
            {
                readonly ISnmpData _data;
                /// <summary>
                /// Initializes a new instance of the <see cref="GwIndex"/> class.
                public GwIndex(int index, object onlineData)
                    : base(OidBase + ".8.1.5.4.2.2.1.1.{0}", index)
                {
                    _data = new Integer32(index);
                }
                public override ISnmpData Data
                {
                    get { return _data; }
                    set { throw new AccessFailureException(); }
                }
            }
            internal sealed class GwDisp : ScalarObject
            {
                private readonly int RowIndex = 0;
                /// <summary>
                /// Initializes a new instance of the <see cref="GwDisp"/> class.
                public GwDisp(int index, object onlineData)
                    // ReSharper restore UnusedParameter.Local
                    : base(OidBase + ".8.1.5.4.2.2.1.2.{0}", index)
                {
                    RowIndex = index;
                }
                public override ISnmpData Data
                {
                    get
                    {
                        String DevName = "Error";
                        U5kSCVMib.DataGet((object gdata) =>
                        {
                            dynamic ddata = gdata;
                            DevName = ddata.STDGWS[RowIndex - 1].name;
                        });

                        return new OctetString(DevName);
                    }
                    set { throw new AccessFailureException(); }
                }
            }
            internal sealed class GwIp : ScalarObject
            {
                private readonly int RowIndex = 0;
                /// <summary>
                /// Initializes a new instance of the <see cref="GwIp"/> class.
                public GwIp(int index, object onlineData)
                    : base(OidBase + ".8.1.5.4.2.2.1.3.{0}", index)
                {
                    RowIndex = index;
                }
                public override ISnmpData Data
                {
                    get
                    {
                        String IpInfo = "Error";
                        U5kSCVMib.DataGet((object gdata) =>
                        {
                            dynamic ddata = gdata;
                            IpInfo = ddata.STDGWS[RowIndex - 1].ip;
                        });

                        return new OctetString(IpInfo);
                    }
                    set { throw new AccessFailureException(); }
                }
            }
            internal sealed class GwStd : ScalarObject
            {
                private readonly int RowIndex = 0;
                /// <summary>
                /// Initializes a new instance of the <see cref="GwStd"/> class.
                public GwStd(int index, object onlineData)
                    // ReSharper restore UnusedParameter.Local
                    : base(OidBase + ".8.1.5.4.2.2.1.4.{0}", index)
                {
                    RowIndex = index;
                }
                public override ISnmpData Data
                {
                    get
                    {
                        Int32 StdInfo = 0;
                        U5kSCVMib.DataGet((object gdata) =>
                        {
                            dynamic ddata = gdata;
                            StdInfo = (int)ddata.STDGWS[RowIndex - 1].std;
                        });
                        return new Integer32(StdInfo);
                    }
                    set { throw new AccessFailureException(); }
                }
            }
            internal sealed class GwLans : ScalarObject
            {
                private readonly int RowIndex = 0;
                /// <summary>
                /// Initializes a new instance of the <see cref="GwLans"/> class.
                public GwLans(int index, object onlineData)
                    // ReSharper restore UnusedParameter.Local
                    : base(OidBase + ".8.1.5.4.2.2.1.5.{0}", index)
                {
                    RowIndex = index;
                }
                public override ISnmpData Data
                {
                    get
                    {
                        String LansInfo = "Error";
                        U5kSCVMib.DataGet((object gdata) =>
                        {
                            dynamic ddata = gdata;
                            int lan1 = (int)ddata.STDGWS[RowIndex - 1].cpu_activa.lan1;
                            int lan2 = (int)ddata.STDGWS[RowIndex - 1].cpu_activa.lan2;
                            LansInfo = ddata.STDG.PosLans(lan1, lan2);
                        });

                        return new OctetString(LansInfo);
                    }
                    set { throw new AccessFailureException(); }
                }
            }
            internal sealed class GwSync : ScalarObject
            {
                private readonly int RowIndex = 0;
                /// <summary>
                /// Initializes a new instance of the <see cref="GwSync"/> class.
                public GwSync(int index, object onlineData)
                    // ReSharper restore UnusedParameter.Local
                    : base(OidBase + ".8.1.5.4.2.2.1.6.{0}", index)
                {
                    RowIndex = index;
                }
                public override ISnmpData Data
                {
                    get
                    {
                        String SyncInfo = "Error";
                        U5kSCVMib.DataGet((object gdata) =>
                        {
                            dynamic ddata = gdata;

                            List<string> ntpl = ddata.STDGWS[RowIndex - 1].ntp_client_status;
                            SyncInfo = ntpl == null ? "null" : String.Join("##", ntpl.ToArray());
                        });

                        return new OctetString(SyncInfo);
                    }
                    set { throw new AccessFailureException(); }
                }
            }
            internal sealed class GwHw : ScalarObject
            {
                private readonly int RowIndex = 0;
                /// <summary>
                /// Initializes a new instance of the <see cref="GwHw"/> class.
                public GwHw(int index, object onlineData)
                    // ReSharper restore UnusedParameter.Local
                    : base(OidBase + ".8.1.5.4.2.2.1.7.{0}", index)
                {
                    RowIndex = index;
                }
                public override ISnmpData Data
                {
                    get
                    {
                        String GwHwInfo = "Error";
                        U5kSCVMib.DataGet((object gdata) =>
                        {
                            dynamic ddata = gdata;
                            GwHwInfo = ddata.STDG.GwHwStd(ddata.STDGWS[RowIndex - 1]);
                        });

                        return new OctetString(GwHwInfo);
                    }
                    set { throw new AccessFailureException(); }
                }
            }

            /// <summary>
            /// Initializes a new instance of the <see cref="GwsTable"/> class.
            /// </summary>
            public GwsTable()
            {
                //                /** Ejemplo para programar el evento de actualizacion de la tabla */
                //                NetworkChange.NetworkAddressChanged +=
                //                    (sender, args) => LoadElements();
                //#if NET452
                //            NetworkChange.NetworkAvailabilityChanged +=
                //                (sender, args) => LoadElements();
                //#endif
                LoadElements();
            }

            // ".1.3.6.1.4.1.7916.8.1.5.2.3.2"
            private readonly IList<ScalarObject> _elements = new List<ScalarObject>();
            private void LoadElements()
            {
                _elements.Clear();
                var columnTypes = new[]
                {
                    typeof(GwIndex),
                    typeof(GwDisp),
                    typeof(GwIp),
                    typeof(GwStd),
                    typeof(GwLans),
                    typeof(GwSync),
                    typeof(GwHw),
                };
                int count = 0;
                U5kSCVMib.DataGet((object gdata) =>
                {
                    dynamic ddata = gdata;
                    count = ddata.STDGWS.Count;
                });

                foreach (var type in columnTypes)
                {
                    for (int i = 0; i < count; i++)
                    {
                        _elements.Add((ScalarObject)Activator.CreateInstance(type, new object[] { i + 1, null }));
                    }
                }
            }
            protected override IEnumerable<ScalarObject> Objects
            {
                get { return _elements; }
            }
        }

        /** */
        internal sealed class RadTableLen : ScalarObject
        {
            /// <summary>
            /// Initializes a new instance of the <see cref="RadTableLen"/> class.
            /// </summary>
            public RadTableLen()
                : base(OidBase + ".8.1.5.4.3.1.0")
            {
            }
            /// <summary>
            /// 
            /// </summary>
            public override ISnmpData Data
            {
                get
                {
                    int data = 0;
                    U5kSCVMib.DataGet((object gdata) =>
                    {
                        dynamic ddata = gdata;
                        data = ddata.STDG.TotalGwRadResources.Count;
                    });
                    return new Integer32(data);
                }
                set { throw new AccessFailureException(); }
            }
        }
        internal sealed class RadTable : TableObject
        {
            internal sealed class RdIndex : ScalarObject
            {
                readonly ISnmpData _data;
                /// <summary>
                /// Initializes a new instance of the <see cref="RdIndex"/> class.
                public RdIndex(int index, object onlineData)
                    : base(OidBase + ".8.1.5.4.3.2.1.1.{0}", index)
                {
                    _data = new Integer32(index);
                }
                public override ISnmpData Data
                {
                    get { return _data; }
                    set { throw new AccessFailureException(); }
                }
            }
            internal sealed class RdDisp : ScalarObject
            {
                private readonly int RowIndex = 0;
                /// <summary>
                /// Initializes a new instance of the <see cref="RdDisp"/> class.
                public RdDisp(int index, object onlineData)
                    // ReSharper restore UnusedParameter.Local
                    : base(OidBase + ".8.1.5.4.3.2.1.2.{0}", index)
                {
                    RowIndex = index;
                }
                public override ISnmpData Data
                {
                    get
                    {
                        String DevName = "Error";
                        U5kSCVMib.DataGet((object gdata) =>
                        {
                            dynamic ddata = gdata;
                            DevName = ddata.STDG.TotalGwRadResources[RowIndex - 1].name;
                        });

                        return new OctetString(DevName);
                    }
                    set { throw new AccessFailureException(); }
                }
            }
            internal sealed class RdStd : ScalarObject
            {
                private readonly int RowIndex = 0;
                /// <summary>
                /// Initializes a new instance of the <see cref="RdStd"/> class.
                public RdStd(int index, object onlineData)
                    // ReSharper restore UnusedParameter.Local
                    : base(OidBase + ".8.1.5.4.3.2.1.3.{0}", index)
                {
                    RowIndex = index;
                }
                public override ISnmpData Data
                {
                    get
                    {
                        Int32 StdInfo = 0;
                        U5kSCVMib.DataGet((object gdata) =>
                        {
                            dynamic ddata = gdata;
                            dynamic rc = ddata.STDG.TotalGwRadResources[RowIndex - 1];                            
                            StdInfo = (int)(rc.presente ? rc.std_online : 0);
                        });
                        return new Integer32(StdInfo);
                    }
                    set { throw new AccessFailureException(); }
                }
            }
            internal sealed class RdUri : ScalarObject
            {
                private readonly int RowIndex = 0;
                /// <summary>
                /// Initializes a new instance of the <see cref="RdUri"/> class.
                public RdUri(int index, object onlineData)
                    // ReSharper restore UnusedParameter.Local
                    : base(OidBase + ".8.1.5.4.3.2.1.4.{0}", index)
                {
                    RowIndex = index;
                }
                public override ISnmpData Data
                {
                    get
                    {
                        String LansInfo = "Error";
                        U5kSCVMib.DataGet((object gdata) =>
                        {
                            dynamic ddata = gdata;
                            dynamic rc = ddata.STDG.TotalGwRadResources[RowIndex - 1];
                            LansInfo = String.Format("<sip:{0}@{1}:{2}>", rc.name, rc.VirtualIp, 5060); ;
                        });

                        return new OctetString(LansInfo);
                    }
                    set { throw new AccessFailureException(); }
                }
            }

            /// <summary>
            /// Initializes a new instance of the <see cref="GwsTable"/> class.
            /// </summary>
            public RadTable()
            {
                //                /** Ejemplo para programar el evento de actualizacion de la tabla */
                //                NetworkChange.NetworkAddressChanged +=
                //                    (sender, args) => LoadElements();
                //#if NET452
                //            NetworkChange.NetworkAvailabilityChanged +=
                //                (sender, args) => LoadElements();
                //#endif
                LoadElements();
            }

            private readonly IList<ScalarObject> _elements = new List<ScalarObject>();
            private void LoadElements()
            {
                _elements.Clear();
                var columnTypes = new[]
                {
                    typeof(RdIndex),
                    typeof(RdDisp),
                    typeof(RdStd),
                    typeof(RdUri),
                };
                int count = 0;
                U5kSCVMib.DataGet((object gdata) =>
                {
                    dynamic ddata = gdata;
                    count = ddata.STDG.TotalGwRadResources.Count;
                });

                foreach (var type in columnTypes)
                {
                    for (int i = 0; i < count; i++)
                    {
                        _elements.Add((ScalarObject)Activator.CreateInstance(type, new object[] { i + 1, null }));
                    }
                }
            }
            protected override IEnumerable<ScalarObject> Objects
            {
                get { return _elements; }
            }
        }

        /** */
        internal sealed class PhoTableLen : ScalarObject
        {
            /// <summary>
            /// Initializes a new instance of the <see cref="PhoTableLen"/> class.
            /// </summary>
            public PhoTableLen()
                : base(OidBase + ".8.1.5.4.4.1.0")
            {
            }
            /// <summary>
            /// 
            /// </summary>
            public override ISnmpData Data
            {
                get
                {
                    int data = 0;
                    U5kSCVMib.DataGet((object gdata) =>
                    {
                        dynamic ddata = gdata;
                        data = ddata.STDG.TotalGwPhoResources.Count;
                    });
                    return new Integer32(data);
                }
                set { throw new AccessFailureException(); }
            }
        }
        internal sealed class PhoTable : TableObject
        {
            internal sealed class PhIndex : ScalarObject
            {
                readonly ISnmpData _data;
                /// <summary>
                /// Initializes a new instance of the <see cref="PhIndex"/> class.
                public PhIndex(int index, object onlineData)
                    : base(OidBase + ".8.1.5.4.4.2.1.1.{0}", index)
                {
                    _data = new Integer32(index);
                }
                public override ISnmpData Data
                {
                    get { return _data; }
                    set { throw new AccessFailureException(); }
                }
            }
            internal sealed class PhDisp : ScalarObject
            {
                private readonly int RowIndex = 0;
                /// <summary>
                /// Initializes a new instance of the <see cref="PhDisp"/> class.
                public PhDisp(int index, object onlineData)
                    // ReSharper restore UnusedParameter.Local
                    : base(OidBase + ".8.1.5.4.4.2.1.2.{0}", index)
                {
                    RowIndex = index;
                }
                public override ISnmpData Data
                {
                    get
                    {
                        String DevName = "Error";
                        U5kSCVMib.DataGet((object gdata) =>
                        {
                            dynamic ddata = gdata;
                            DevName = ddata.STDG.TotalGwPhoResources[RowIndex - 1].name;
                        });

                        return new OctetString(DevName);
                    }
                    set { throw new AccessFailureException(); }
                }
            }
            internal sealed class PhType : ScalarObject
            {
                private readonly int RowIndex = 0;
                /// <summary>
                /// Initializes a new instance of the <see cref="PhType"/> class.
                public PhType(int index, object onlineData)
                    // ReSharper restore UnusedParameter.Local
                    : base(OidBase + ".8.1.5.4.4.2.1.3.{0}", index)
                {
                    RowIndex = index;
                }
                public override ISnmpData Data
                {
                    get
                    {
                        Int32 DevType = 0;
                        U5kSCVMib.DataGet((object gdata) =>
                        {
                            dynamic ddata = gdata;
                            DevType = (int)ddata.STDG.TotalGwPhoResources[RowIndex - 1].tipo;
                        });

                        return new Integer32(DevType);
                    }
                    set { throw new AccessFailureException(); }
                }
            }
            internal sealed class PhStd : ScalarObject
            {
                private readonly int RowIndex = 0;
                /// <summary>
                /// Initializes a new instance of the <see cref="PhStd"/> class.
                public PhStd(int index, object onlineData)
                    // ReSharper restore UnusedParameter.Local
                    : base(OidBase + ".8.1.5.4.4.2.1.4.{0}", index)
                {
                    RowIndex = index;
                }
                public override ISnmpData Data
                {
                    get
                    {
                        Int32 StdInfo = 0;
                        U5kSCVMib.DataGet((object gdata) =>
                        {
                            dynamic ddata = gdata;
                            dynamic rc = ddata.STDG.TotalGwPhoResources[RowIndex - 1];
                            StdInfo = (int)(rc.presente ? rc.std_online : 0);
                        });
                        return new Integer32(StdInfo);
                    }
                    set { throw new AccessFailureException(); }
                }
            }
            internal sealed class PhUri : ScalarObject
            {
                private readonly int RowIndex = 0;
                /// <summary>
                /// Initializes a new instance of the <see cref="PhUri"/> class.
                public PhUri(int index, object onlineData)
                    // ReSharper restore UnusedParameter.Local
                    : base(OidBase + ".8.1.5.4.4.2.1.5.{0}", index)
                {
                    RowIndex = index;
                }
                public override ISnmpData Data
                {
                    get
                    {
                        String LansInfo = "Error";
                        U5kSCVMib.DataGet((object gdata) =>
                        {
                            dynamic ddata = gdata;
                            dynamic rc = ddata.STDG.TotalGwPhoResources[RowIndex - 1];
                            LansInfo = String.Format("<sip:{0}@{1}:{2}>", rc.name, rc.VirtualIp, 5060); ;
                        });

                        return new OctetString(LansInfo);
                    }
                    set { throw new AccessFailureException(); }
                }
            }

            /// <summary>
            /// Initializes a new instance of the <see cref="GwsTable"/> class.
            /// </summary>
            public PhoTable()
            {
                //                /** Ejemplo para programar el evento de actualizacion de la tabla */
                //                NetworkChange.NetworkAddressChanged +=
                //                    (sender, args) => LoadElements();
                //#if NET452
                //            NetworkChange.NetworkAvailabilityChanged +=
                //                (sender, args) => LoadElements();
                //#endif
                LoadElements();
            }

            private readonly IList<ScalarObject> _elements = new List<ScalarObject>();
            private void LoadElements()
            {
                _elements.Clear();
                var columnTypes = new[]
                {
                    typeof(PhIndex),
                    typeof(PhDisp),
                    typeof(PhType),
                    typeof(PhStd),
                    typeof(PhUri),
                };
                int count = 0;
                U5kSCVMib.DataGet((object gdata) =>
                {
                    dynamic ddata = gdata;
                    count = ddata.STDG.TotalGwPhoResources.Count;
                });

                foreach (var type in columnTypes)
                {
                    for (int i = 0; i < count; i++)
                    {
                        _elements.Add((ScalarObject)Activator.CreateInstance(type, new object[] { i + 1, null }));
                    }
                }
            }
            protected override IEnumerable<ScalarObject> Objects
            {
                get { return _elements; }
            }
        }
        /// <summary>
        /// 
        /// </summary>
        protected override void LoadElements()
        {
            snmpObjects = new List<SnmpObjectBase>()
            {
                new GlobalStd(),
                new GwsTableLen(), new GwsTable(),
                new RadTableLen(), new RadTable(),
                new PhoTableLen(), new PhoTable(),
            };
        }
        public U5kSCVMibGwsGroup() : base(U5kSCVMib.OidBase)
        {
        }
    }
    /// <summary>
    /// Grupo de Equipos Externos
    /// </summary>
    class U5kSCVMibExtGroup : U5kMibGroup
    {
        /** */
        internal sealed class RadTableLen : ScalarObject
        {
            /// <summary>
            /// Initializes a new instance of the <see cref="RadTableLen"/> class.
            /// </summary>
            public RadTableLen()
                : base(OidBase + ".8.1.5.5.1.1.0")
            {
            }
            /// <summary>
            /// 
            /// </summary>
            public override ISnmpData Data
            {
                get
                {
                    int data = 0;
                    U5kSCVMib.DataGet((object gdata) =>
                    {
                        dynamic ddata = gdata;
                        data = ddata.STDG.TotalExRadResources.Count;
                    });
                    return new Integer32(data);
                }
                set { throw new AccessFailureException(); }
            }
        }
        internal sealed class RadTable : TableObject
        {
            internal sealed class RdIndex : ScalarObject
            {
                readonly ISnmpData _data;
                /// <summary>
                /// Initializes a new instance of the <see cref="RdIndex"/> class.
                public RdIndex(int index, object onlineData)
                    : base(OidBase + ".8.1.5.5.1.2.1.1.{0}", index)
                {
                    _data = new Integer32(index);
                }
                public override ISnmpData Data
                {
                    get { return _data; }
                    set { throw new AccessFailureException(); }
                }
            }
            internal sealed class RdDisp : ScalarObject
            {
                private readonly int RowIndex = 0;
                /// <summary>
                /// Initializes a new instance of the <see cref="RdDisp"/> class.
                public RdDisp(int index, object onlineData)
                    // ReSharper restore UnusedParameter.Local
                    : base(OidBase + ".8.1.5.5.1.2.1.2.{0}", index)
                {
                    RowIndex = index;
                }
                public override ISnmpData Data
                {
                    get
                    {
                        String DevName = "Error";
                        U5kSCVMib.DataGet((object gdata) =>
                        {
                            dynamic ddata = gdata;
                            DevName = ddata.STDG.TotalExRadResources[RowIndex - 1].Id;
                        });

                        return new OctetString(DevName);
                    }
                    set { throw new AccessFailureException(); }
                }
            }
            internal sealed class RdIp : ScalarObject
            {
                private readonly int RowIndex = 0;
                /// <summary>
                /// Initializes a new instance of the <see cref="RdIp"/> class.
                public RdIp(int index, object onlineData)
                    // ReSharper restore UnusedParameter.Local
                    : base(OidBase + ".8.1.5.5.1.2.1.3.{0}", index)
                {
                    RowIndex = index;
                }
                public override ISnmpData Data
                {
                    get
                    {
                        String DevName = "Error";
                        U5kSCVMib.DataGet((object gdata) =>
                        {
                            dynamic ddata = gdata;
                            DevName = ddata.STDG.TotalExRadResources[RowIndex - 1].Ip1;
                        });

                        return new OctetString(DevName);
                    }
                    set { throw new AccessFailureException(); }
                }
            }
            internal sealed class RdStd : ScalarObject
            {
                private readonly int RowIndex = 0;
                /// <summary>
                /// Initializes a new instance of the <see cref="RdStd"/> class.
                public RdStd(int index, object onlineData)
                    // ReSharper restore UnusedParameter.Local
                    : base(OidBase + ".8.1.5.5.1.2.1.4.{0}", index)
                {
                    RowIndex = index;
                }
                public override ISnmpData Data
                {
                    get
                    {
                        Int32 StdInfo = 0;
                        U5kSCVMib.DataGet((object gdata) =>
                        {
                            dynamic ddata = gdata;
                            dynamic rc = ddata.STDG.TotalExRadResources[RowIndex - 1];
                            StdInfo = (int)(rc.EstadoGeneral);
                        });
                        return new Integer32(StdInfo);
                    }
                    set { throw new AccessFailureException(); }
                }
            }
            internal sealed class RdUri : ScalarObject
            {
                private readonly int RowIndex = 0;
                /// <summary>
                /// Initializes a new instance of the <see cref="RdUri"/> class.
                public RdUri(int index, object onlineData)
                    // ReSharper restore UnusedParameter.Local
                    : base(OidBase + ".8.1.5.5.1.2.1.5.{0}", index)
                {
                    RowIndex = index;
                }
                public override ISnmpData Data
                {
                    get
                    {
                        String LansInfo = "Error";
                        U5kSCVMib.DataGet((object gdata) =>
                        {
                            dynamic ddata = gdata;
                            dynamic equipo = ddata.STDG.TotalExRadResources[RowIndex - 1];
                            LansInfo = String.Format("<sip:{0}@{1}:{2}>", equipo.sip_user, equipo.Ip1, equipo.sip_port);
                        });

                        return new OctetString(LansInfo);
                    }
                    set { throw new AccessFailureException(); }
                }
            }

            /// <summary>
            /// Initializes a new instance of the <see cref="RecTable"/> class.
            /// </summary>
            public RadTable()
            {
                //                /** Ejemplo para programar el evento de actualizacion de la tabla */
                //                NetworkChange.NetworkAddressChanged +=
                //                    (sender, args) => LoadElements();
                //#if NET452
                //            NetworkChange.NetworkAvailabilityChanged +=
                //                (sender, args) => LoadElements();
                //#endif
                LoadElements();
            }

            private readonly IList<ScalarObject> _elements = new List<ScalarObject>();
            private void LoadElements()
            {
                _elements.Clear();
                var columnTypes = new[]
                {
                    typeof(RdIndex),
                    typeof(RdDisp),
                    typeof(RdIp),
                    typeof(RdStd),
                    typeof(RdUri),
                };
                int count = 0;
                U5kSCVMib.DataGet((object gdata) =>
                {
                    dynamic ddata = gdata;
                    count = ddata.STDG.TotalExRadResources.Count;
                });

                foreach (var type in columnTypes)
                {
                    for (int i = 0; i < count; i++)
                    {
                        _elements.Add((ScalarObject)Activator.CreateInstance(type, new object[] { i + 1, null }));
                    }
                }
            }
            protected override IEnumerable<ScalarObject> Objects
            {
                get { return _elements; }
            }
        }

        /** */
        internal sealed class PhoTableLen : ScalarObject
        {
            /// <summary>
            /// Initializes a new instance of the <see cref="PhoTableLen"/> class.
            /// </summary>
            public PhoTableLen()
                : base(OidBase + ".8.1.5.5.2.1.0")
            {
            }
            /// <summary>
            /// 
            /// </summary>
            public override ISnmpData Data
            {
                get
                {
                    int data = 0;
                    U5kSCVMib.DataGet((object gdata) =>
                    {
                        dynamic ddata = gdata;
                        data = ddata.STDG.TotalExPhoResources.Count;
                    });
                    return new Integer32(data);
                }
                set { throw new AccessFailureException(); }
            }
        }
        internal sealed class PhoTable : TableObject
        {
            internal sealed class PhIndex : ScalarObject
            {
                readonly ISnmpData _data;
                /// <summary>
                /// Initializes a new instance of the <see cref="PhIndex"/> class.
                public PhIndex(int index, object onlineData)
                    : base(OidBase + ".8.1.5.5.2.2.1.1.{0}", index)
                {
                    _data = new Integer32(index);
                }
                public override ISnmpData Data
                {
                    get { return _data; }
                    set { throw new AccessFailureException(); }
                }
            }
            internal sealed class PhDisp : ScalarObject
            {
                private readonly int RowIndex = 0;
                /// <summary>
                /// Initializes a new instance of the <see cref="PhDisp"/> class.
                public PhDisp(int index, object onlineData)
                    // ReSharper restore UnusedParameter.Local
                    : base(OidBase + ".8.1.5.5.2.2.1.2.{0}", index)
                {
                    RowIndex = index;
                }
                public override ISnmpData Data
                {
                    get
                    {
                        String DevName = "Error";
                        U5kSCVMib.DataGet((object gdata) =>
                        {
                            dynamic ddata = gdata;
                            DevName = ddata.STDG.TotalExPhoResources[RowIndex - 1].Id;
                        });

                        return new OctetString(DevName);
                    }
                    set { throw new AccessFailureException(); }
                }
            }
            internal sealed class PhIp : ScalarObject
            {
                private readonly int RowIndex = 0;
                /// <summary>
                /// Initializes a new instance of the <see cref="PhIp"/> class.
                public PhIp(int index, object onlineData)
                    // ReSharper restore UnusedParameter.Local
                    : base(OidBase + ".8.1.5.5.2.2.1.3.{0}", index)
                {
                    RowIndex = index;
                }
                public override ISnmpData Data
                {
                    get
                    {
                        String DevName = "Error";
                        U5kSCVMib.DataGet((object gdata) =>
                        {
                            dynamic ddata = gdata;
                            DevName = ddata.STDG.TotalExPhoResources[RowIndex - 1].Ip1;
                        });

                        return new OctetString(DevName);
                    }
                    set { throw new AccessFailureException(); }
                }
            }
            internal sealed class PhStd : ScalarObject
            {
                private readonly int RowIndex = 0;
                /// <summary>
                /// Initializes a new instance of the <see cref="PhStd"/> class.
                public PhStd(int index, object onlineData)
                    // ReSharper restore UnusedParameter.Local
                    : base(OidBase + ".8.1.5.5.2.2.1.4.{0}", index)
                {
                    RowIndex = index;
                }
                public override ISnmpData Data
                {
                    get
                    {
                        Int32 StdInfo = 0;
                        U5kSCVMib.DataGet((object gdata) =>
                        {
                            dynamic ddata = gdata;
                            dynamic rc = ddata.STDG.TotalExPhoResources[RowIndex - 1];
                            StdInfo = (int)(rc.EstadoGeneral);
                        });
                        return new Integer32(StdInfo);
                    }
                    set { throw new AccessFailureException(); }
                }
            }
            internal sealed class PhUri : ScalarObject
            {
                private readonly int RowIndex = 0;
                /// <summary>
                /// Initializes a new instance of the <see cref="PhUri"/> class.
                public PhUri(int index, object onlineData)
                    // ReSharper restore UnusedParameter.Local
                    : base(OidBase + ".8.1.5.5.2.2.1.5.{0}", index)
                {
                    RowIndex = index;
                }
                public override ISnmpData Data
                {
                    get
                    {
                        String LansInfo = "Error";
                        U5kSCVMib.DataGet((object gdata) =>
                        {
                            dynamic ddata = gdata;
                            dynamic equipo = ddata.STDG.TotalExPhoResources[RowIndex - 1];
                            LansInfo = String.Format("<sip:{0}@{1}:{2}>", equipo.sip_user, equipo.Ip1, equipo.sip_port);
                        });

                        return new OctetString(LansInfo);
                    }
                    set { throw new AccessFailureException(); }
                }
            }

            /// <summary>
            /// Initializes a new instance of the <see cref="PhoTable"/> class.
            /// </summary>
            public PhoTable()
            {
                //                /** Ejemplo para programar el evento de actualizacion de la tabla */
                //                NetworkChange.NetworkAddressChanged +=
                //                    (sender, args) => LoadElements();
                //#if NET452
                //            NetworkChange.NetworkAvailabilityChanged +=
                //                (sender, args) => LoadElements();
                //#endif
                LoadElements();
            }

            private readonly IList<ScalarObject> _elements = new List<ScalarObject>();
            private void LoadElements()
            {
                _elements.Clear();
                var columnTypes = new[]
                {
                    typeof(PhIndex),
                    typeof(PhDisp),
                    typeof(PhIp),
                    typeof(PhStd),
                    typeof(PhUri),
                };
                int count = 0;
                U5kSCVMib.DataGet((object gdata) =>
                {
                    dynamic ddata = gdata;
                    count = ddata.STDG.TotalExPhoResources.Count;
                });

                foreach (var type in columnTypes)
                {
                    for (int i = 0; i < count; i++)
                    {
                        _elements.Add((ScalarObject)Activator.CreateInstance(type, new object[] { i + 1, null }));
                    }
                }
            }
            protected override IEnumerable<ScalarObject> Objects
            {
                get { return _elements; }
            }
        }

        /** */
        internal sealed class RecTableLen : ScalarObject
        {
            /// <summary>
            /// Initializes a new instance of the <see cref="RecTableLen"/> class.
            /// </summary>
            public RecTableLen()
                : base(OidBase + ".8.1.5.5.3.1.0")
            {
            }
            /// <summary>
            /// 
            /// </summary>
            public override ISnmpData Data
            {
                get
                {
                    int data = 0;
                    U5kSCVMib.DataGet((object gdata) =>
                    {
                        dynamic ddata = gdata;
                        data = ddata.STDG.TotalExRecResources.Count;
                    });
                    return new Integer32(data);
                }
                set { throw new AccessFailureException(); }
            }
        }
        internal sealed class RecTable : TableObject
        {
            internal sealed class ReIndex : ScalarObject
            {
                readonly ISnmpData _data;
                /// <summary>
                /// Initializes a new instance of the <see cref="ReIndex"/> class.
                public ReIndex(int index, object onlineData)
                    : base(OidBase + ".8.1.5.5.3.2.1.1.{0}", index)
                {
                    _data = new Integer32(index);
                }
                public override ISnmpData Data
                {
                    get { return _data; }
                    set { throw new AccessFailureException(); }
                }
            }
            internal sealed class ReDisp : ScalarObject
            {
                private readonly int RowIndex = 0;
                /// <summary>
                /// Initializes a new instance of the <see cref="ReDisp"/> class.
                public ReDisp(int index, object onlineData)
                    // ReSharper restore UnusedParameter.Local
                    : base(OidBase + ".8.1.5.5.3.2.1.2.{0}", index)
                {
                    RowIndex = index;
                }
                public override ISnmpData Data
                {
                    get
                    {
                        String DevName = "Error";
                        U5kSCVMib.DataGet((object gdata) =>
                        {
                            dynamic ddata = gdata;
                            DevName = ddata.STDG.TotalExRecResources[RowIndex - 1].Id;
                        });

                        return new OctetString(DevName);
                    }
                    set { throw new AccessFailureException(); }
                }
            }
            internal sealed class ReIp : ScalarObject
            {
                private readonly int RowIndex = 0;
                /// <summary>
                /// Initializes a new instance of the <see cref="ReIp"/> class.
                public ReIp(int index, object onlineData)
                    // ReSharper restore UnusedParameter.Local
                    : base(OidBase + ".8.1.5.5.3.2.1.3.{0}", index)
                {
                    RowIndex = index;
                }
                public override ISnmpData Data
                {
                    get
                    {
                        String DevName = "Error";
                        U5kSCVMib.DataGet((object gdata) =>
                        {
                            dynamic ddata = gdata;
                            DevName = ddata.STDG.TotalExRecResources[RowIndex - 1].Ip1;
                        });

                        return new OctetString(DevName);
                    }
                    set { throw new AccessFailureException(); }
                }
            }
            internal sealed class ReStd : ScalarObject
            {
                private readonly int RowIndex = 0;
                /// <summary>
                /// Initializes a new instance of the <see cref="ReStd"/> class.
                public ReStd(int index, object onlineData)
                    // ReSharper restore UnusedParameter.Local
                    : base(OidBase + ".8.1.5.5.3.2.1.4.{0}", index)
                {
                    RowIndex = index;
                }
                public override ISnmpData Data
                {
                    get
                    {
                        Int32 StdInfo = 0;
                        U5kSCVMib.DataGet((object gdata) =>
                        {
                            dynamic ddata = gdata;
                            dynamic rc = ddata.STDG.TotalExRecResources[RowIndex - 1];
                            StdInfo = (int)(rc.EstadoGeneral);
                        });
                        return new Integer32(StdInfo);
                    }
                    set { throw new AccessFailureException(); }
                }
            }

            /// <summary>
            /// Initializes a new instance of the <see cref="PhoTable"/> class.
            /// </summary>
            public RecTable()
            {
                //                /** Ejemplo para programar el evento de actualizacion de la tabla */
                //                NetworkChange.NetworkAddressChanged +=
                //                    (sender, args) => LoadElements();
                //#if NET452
                //            NetworkChange.NetworkAvailabilityChanged +=
                //                (sender, args) => LoadElements();
                //#endif
                LoadElements();
            }

            private readonly IList<ScalarObject> _elements = new List<ScalarObject>();
            private void LoadElements()
            {
                _elements.Clear();
                var columnTypes = new[]
                {
                    typeof(ReIndex),
                    typeof(ReDisp),
                    typeof(ReIp),
                    typeof(ReStd),
                };
                int count = 0;
                U5kSCVMib.DataGet((object gdata) =>
                {
                    dynamic ddata = gdata;
                    count = ddata.STDG.TotalExRecResources.Count;
                });

                foreach (var type in columnTypes)
                {
                    for (int i = 0; i < count; i++)
                    {
                        _elements.Add((ScalarObject)Activator.CreateInstance(type, new object[] { i + 1, null }));
                    }
                }
            }
            protected override IEnumerable<ScalarObject> Objects
            {
                get { return _elements; }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        protected override void LoadElements()
        {
            snmpObjects = new List<SnmpObjectBase>()
            {
                new RadTableLen(), new RadTable(),
                new PhoTableLen(), new PhoTable(),
                new RecTableLen(), new RecTable(),
            };
        }
        public U5kSCVMibExtGroup() : base(U5kSCVMib.OidBase)
        {
        }
    }
    /// <summary>
    /// Grupo de Abonados Pbx
    /// </summary>
    class U5kSCVMibPbxGroup : U5kMibGroup
    {
        internal sealed class SubsStd
            : ScalarObject
        {
            /// <summary>
            /// Initializes a new instance of the <see cref="SubsTableLen"/> class.
            /// </summary>
            public SubsStd()
                : base(OidBase + ".8.1.5.6.1.0")
            {
            }
            /// <summary>
            /// 
            /// </summary>
            public override ISnmpData Data
            {
                get
                {
                    int data = 0;
                    U5kSCVMib.DataGet((object gdata) =>
                    {
                        dynamic ddata = gdata;
                        data = (int)ddata.STDG.stdPabx.Estado;
                    });
                    return new Integer32(data);
                }
                set { throw new AccessFailureException(); }
            }
        }
        /** */
        internal sealed class SubsTableLen : ScalarObject
        {
            /// <summary>
            /// Initializes a new instance of the <see cref="SubsTableLen"/> class.
            /// </summary>
            public SubsTableLen()
                : base(OidBase + ".8.1.5.6.2.1.0")
            {
            }
            /// <summary>
            /// 
            /// </summary>
            public override ISnmpData Data
            {
                get
                {
                    int data = 0;
                    U5kSCVMib.DataGet((object gdata) =>
                    {
                        dynamic ddata = gdata;
                        data = ddata.STDPBXS.Count;
                    });
                    return new Integer32(data);
                }
                set { throw new AccessFailureException(); }
            }
        }
        internal sealed class SubsTable : TableObject
        {
            internal sealed class SubIndex : ScalarObject
            {
                readonly ISnmpData _data;
                /// <summary>
                /// Initializes a new instance of the <see cref="SubIndex"/> class.
                public SubIndex(int index, object onlineData)
                    : base(OidBase + ".8.1.5.6.2.2.1.1.{0}", index)
                {
                    _data = new Integer32(index);
                }
                public override ISnmpData Data
                {
                    get { return _data; }
                    set { throw new AccessFailureException(); }
                }
            }
            internal sealed class SubDisp : ScalarObject
            {
                private readonly int RowIndex = 0;
                /// <summary>
                /// Initializes a new instance of the <see cref="SubDisp"/> class.
                public SubDisp(int index, object onlineData)
                    // ReSharper restore UnusedParameter.Local
                    : base(OidBase + ".8.1.5.6.2.2.1.2.{0}", index)
                {
                    RowIndex = index;
                }
                public override ISnmpData Data
                {
                    get
                    {
                        String DevName = "Error";
                        U5kSCVMib.DataGet((object gdata) =>
                        {
                            dynamic ddata = gdata;
                            DevName = ddata.STDPBXS[RowIndex - 1].Id;
                        });

                        return new OctetString(DevName);
                    }
                    set { throw new AccessFailureException(); }
                }
            }
            internal sealed class SubIp : ScalarObject
            {
                private readonly int RowIndex = 0;
                /// <summary>
                /// Initializes a new instance of the <see cref="SubIp"/> class.
                public SubIp(int index, object onlineData)
                    // ReSharper restore UnusedParameter.Local
                    : base(OidBase + ".8.1.5.6.2.2.1.3.{0}", index)
                {
                    RowIndex = index;
                }
                public override ISnmpData Data
                {
                    get
                    {
                        String DevName = "----";
                        //U5kSCVMib.DataGet((object gdata) =>
                        //{
                        //    dynamic ddata = gdata;
                        //    DevName = ddata.STDG.TotalExRadResources[RowIndex - 1].Ip1;
                        //});
                        return new OctetString(DevName);
                    }
                    set { throw new AccessFailureException(); }
                }
            }
            internal sealed class SubStd : ScalarObject
            {
                private readonly int RowIndex = 0;
                /// <summary>
                /// Initializes a new instance of the <see cref="SubStd"/> class.
                public SubStd(int index, object onlineData)
                    // ReSharper restore UnusedParameter.Local
                    : base(OidBase + ".8.1.5.6.2.2.1.4.{0}", index)
                {
                    RowIndex = index;
                }
                public override ISnmpData Data
                {
                    get
                    {
                        Int32 StdInfo = 0;
                        U5kSCVMib.DataGet((object gdata) =>
                        {
                            dynamic ddata = gdata;
                            dynamic rc = ddata.STDPBXS[RowIndex - 1];
                            StdInfo = (int)(rc.Estado);
                        });
                        return new Integer32(StdInfo);
                    }
                    set { throw new AccessFailureException(); }
                }
            }

            /// <summary>
            /// Initializes a new instance of the <see cref="SubsTable"/> class.
            /// </summary>
            public SubsTable()
            {
                //                /** Ejemplo para programar el evento de actualizacion de la tabla */
                //                NetworkChange.NetworkAddressChanged +=
                //                    (sender, args) => LoadElements();
                //#if NET452
                //            NetworkChange.NetworkAvailabilityChanged +=
                //                (sender, args) => LoadElements();
                //#endif
                LoadElements();
            }

            private readonly IList<ScalarObject> _elements = new List<ScalarObject>();
            private void LoadElements()
            {
                _elements.Clear();
                var columnTypes = new[]
                {
                    typeof(SubIndex),
                    typeof(SubDisp),
                    typeof(SubIp),
                    typeof(SubStd),
                };
                int count = 0;
                U5kSCVMib.DataGet((object gdata) =>
                {
                    dynamic ddata = gdata;
                    count = ddata.STDPBXS.Count;
                });

                foreach (var type in columnTypes)
                {
                    for (int i = 0; i < count; i++)
                    {
                        _elements.Add((ScalarObject)Activator.CreateInstance(type, new object[] { i + 1, null }));
                    }
                }
            }
            protected override IEnumerable<ScalarObject> Objects
            {
                get { return _elements; }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        protected override void LoadElements()
        {
            snmpObjects = new List<SnmpObjectBase>()
            {
                new SubsStd(),
                new SubsTableLen(), new SubsTable(),
            };
        }
        public U5kSCVMibPbxGroup() : base(U5kSCVMib.OidBase)
        {
        }
    }
    /// <summary>
    /// Grupo de Radio
    /// </summary>
    class U5kSCVMibRadGroup : U5kMibGroup
    {
        /** */
        internal sealed class RdSesTableLen : ScalarObject
        {
            /// <summary>
            /// Initializes a new instance of the <see cref="RdSesTableLen"/> class.
            /// </summary>
            public RdSesTableLen()
                : base(OidBase + ".8.1.5.7.1.1.0")
            {
            }
            /// <summary>
            /// 
            /// </summary>
            public override ISnmpData Data
            {
                get
                {
                    int data = 0;
                    U5kSCVMib.DataGet((object gdata) =>
                    {
                        dynamic ddata = gdata;
                        data = ddata.STDG.RdSessions.Count;
                    });
                    return new Integer32(data);
                }
                set { throw new AccessFailureException(); }
            }
        }
        internal sealed class RdSesTable : TableObject, IDisposable
        {
            internal sealed class ISesIndex : ScalarObject
            {
                readonly ISnmpData _data;
                /// <summary>
                /// Initializes a new instance of the <see cref="ISesIndex"/> class.
                public ISesIndex(int index, object onlineData)
                    : base(OidBase + ".8.1.5.7.1.2.1.1.{0}", index)
                {
                    _data = new Integer32(index);
                }
                public override ISnmpData Data
                {
                    get { return _data; }
                    set { throw new AccessFailureException(); }
                }
            }
            internal sealed class SSesFreq : ScalarObject
            {
                private readonly int RowIndex = 0;
                /// <summary>
                /// Initializes a new instance of the <see cref="SSesFreq"/> class.
                public SSesFreq(int index, object onlineData)
                    // ReSharper restore UnusedParameter.Local
                    : base(OidBase + ".8.1.5.7.1.2.1.2.{0}", index)
                {
                    RowIndex = index;
                }
                public override ISnmpData Data
                {
                    get
                    {
                        String DevName = "Error";
                        U5kSCVMib.DataGet((object gdata) =>
                        {
                            dynamic ddata = gdata;
                            DevName = ddata.STDG.RdSessions[RowIndex - 1].frec;
                        });

                        return new OctetString(DevName);
                    }
                    set { throw new AccessFailureException(); }
                }
            }
            internal sealed class ISesFType : ScalarObject
            {
                private readonly int RowIndex = 0;
                /// <summary>
                /// Initializes a new instance of the <see cref="ISesFType"/> class.
                public ISesFType(int index, object onlineData)
                    // ReSharper restore UnusedParameter.Local
                    : base(OidBase + ".8.1.5.7.1.2.1.3.{0}", index)
                {
                    RowIndex = index;
                }
                public override ISnmpData Data
                {
                    get
                    {
                        Int32 StdInfo = 0;
                        U5kSCVMib.DataGet((object gdata) =>
                        {
                            dynamic ddata = gdata;
                            StdInfo = (int)(ddata.STDG.RdSessions[RowIndex - 1].ftipo);
                        });
                        return new Integer32(StdInfo);
                    }
                    set { throw new AccessFailureException(); }
                }
            }
            internal sealed class ISesPrio : ScalarObject
            {
                private readonly int RowIndex = 0;
                /// <summary>
                /// Initializes a new instance of the <see cref="ISesPrio"/> class.
                public ISesPrio(int index, object onlineData)
                    // ReSharper restore UnusedParameter.Local
                    : base(OidBase + ".8.1.5.7.1.2.1.4.{0}", index)
                {
                    RowIndex = index;
                }
                public override ISnmpData Data
                {
                    get
                    {
                        Int32 StdInfo = 0;
                        U5kSCVMib.DataGet((object gdata) =>
                        {
                            dynamic ddata = gdata;
                            StdInfo = (int)(ddata.STDG.RdSessions[RowIndex - 1].prio);
                        });
                        return new Integer32(StdInfo);
                    }
                    set { throw new AccessFailureException(); }
                }
            }
            internal sealed class ISesFStd : ScalarObject
            {
                private readonly int RowIndex = 0;
                /// <summary>
                /// Initializes a new instance of the <see cref="ISesFStd"/> class.
                public ISesFStd(int index, object onlineData)
                    // ReSharper restore UnusedParameter.Local
                    : base(OidBase + ".8.1.5.7.1.2.1.5.{0}", index)
                {
                    RowIndex = index;
                }
                public override ISnmpData Data
                {
                    get
                    {
                        Int32 StdInfo = 0;
                        U5kSCVMib.DataGet((object gdata) =>
                        {
                            dynamic ddata = gdata;
                            StdInfo = (int)(ddata.STDG.RdSessions[RowIndex - 1].fstd);
                        });
                        return new Integer32(StdInfo);
                    }
                    set { throw new AccessFailureException(); }
                }
            }
            internal sealed class ISesCldt : ScalarObject
            {
                private readonly int RowIndex = 0;
                /// <summary>
                /// Initializes a new instance of the <see cref="ISesCldt"/> class.
                public ISesCldt(int index, object onlineData)
                    // ReSharper restore UnusedParameter.Local
                    : base(OidBase + ".8.1.5.7.1.2.1.6.{0}", index)
                {
                    RowIndex = index;
                }
                public override ISnmpData Data
                {
                    get
                    {
                        Int32 StdInfo = 0;
                        U5kSCVMib.DataGet((object gdata) =>
                        {
                            dynamic ddata = gdata;
                            StdInfo = (int)(ddata.STDG.RdSessions[RowIndex - 1].fp_climax_mc);
                        });
                        return new Integer32(StdInfo);
                    }
                    set { throw new AccessFailureException(); }
                }
            }
            internal sealed class ISesBssw : ScalarObject
            {
                private readonly int RowIndex = 0;
                /// <summary>
                /// Initializes a new instance of the <see cref="ISesBssw"/> class.
                public ISesBssw(int index, object onlineData)
                    // ReSharper restore UnusedParameter.Local
                    : base(OidBase + ".8.1.5.7.1.2.1.7.{0}", index)
                {
                    RowIndex = index;
                }
                public override ISnmpData Data
                {
                    get
                    {
                        Int32 StdInfo = 0;
                        U5kSCVMib.DataGet((object gdata) =>
                        {
                            dynamic ddata = gdata;
                            StdInfo = (int)(ddata.STDG.RdSessions[RowIndex - 1].fp_bss_win);
                        });
                        return new Integer32(StdInfo);
                    }
                    set { throw new AccessFailureException(); }
                }
            }
            internal sealed class SSesUri : ScalarObject
            {
                private readonly int RowIndex = 0;
                /// <summary>
                /// Initializes a new instance of the <see cref="SSesUri"/> class.
                public SSesUri(int index, object onlineData)
                    // ReSharper restore UnusedParameter.Local
                    : base(OidBase + ".8.1.5.7.1.2.1.8.{0}", index)
                {
                    RowIndex = index;
                }
                public override ISnmpData Data
                {
                    get
                    {
                        String DevName = "Error";
                        U5kSCVMib.DataGet((object gdata) =>
                        {
                            dynamic ddata = gdata;
                            DevName = ddata.STDG.RdSessions[RowIndex - 1].uri;
                        });

                        return new OctetString(DevName);
                    }
                    set { throw new AccessFailureException(); }
                }
            }
            internal sealed class SSesType : ScalarObject
            {
                private readonly int RowIndex = 0;
                /// <summary>
                /// Initializes a new instance of the <see cref="SSesType"/> class.
                public SSesType(int index, object onlineData)
                    // ReSharper restore UnusedParameter.Local
                    : base(OidBase + ".8.1.5.7.1.2.1.9.{0}", index)
                {
                    RowIndex = index;
                }
                public override ISnmpData Data
                {
                    get
                    {
                        String DevName = "Error";
                        U5kSCVMib.DataGet((object gdata) =>
                        {
                            dynamic ddata = gdata;
                            DevName = ddata.STDG.RdSessions[RowIndex - 1].tipo;
                        });

                        return new OctetString(DevName);
                    }
                    set { throw new AccessFailureException(); }
                }
            }
            internal sealed class ISesStd : ScalarObject
            {
                private readonly int RowIndex = 0;
                /// <summary>
                /// Initializes a new instance of the <see cref="ISesStd"/> class.
                public ISesStd(int index, object onlineData)
                    // ReSharper restore UnusedParameter.Local
                    : base(OidBase + ".8.1.5.7.1.2.1.10.{0}", index)
                {
                    RowIndex = index;
                }
                public override ISnmpData Data
                {
                    get
                    {
                        Int32 StdInfo = 0;
                        U5kSCVMib.DataGet((object gdata) =>
                        {
                            dynamic ddata = gdata;
                            StdInfo = (int)(ddata.STDG.RdSessions[RowIndex - 1].std);
                        });
                        return new Integer32(StdInfo);
                    }
                    set { throw new AccessFailureException(); }
                }
            }
            internal sealed class ISesTxRtp : ScalarObject
            {
                private readonly int RowIndex = 0;
                /// <summary>
                /// Initializes a new instance of the <see cref="ISesTxRtp"/> class.
                public ISesTxRtp(int index, object onlineData)
                    // ReSharper restore UnusedParameter.Local
                    : base(OidBase + ".8.1.5.7.1.2.1.11.{0}", index)
                {
                    RowIndex = index;
                }
                public override ISnmpData Data
                {
                    get
                    {
                        Int32 StdInfo = 0;
                        U5kSCVMib.DataGet((object gdata) =>
                        {
                            dynamic ddata = gdata;
                            StdInfo = (int)(ddata.STDG.RdSessions[RowIndex - 1].tx_rtp);
                        });
                        return new Integer32(StdInfo);
                    }
                    set { throw new AccessFailureException(); }
                }
            }
            internal sealed class ISesTxCld : ScalarObject
            {
                private readonly int RowIndex = 0;
                /// <summary>
                /// Initializes a new instance of the <see cref="ISesTxCld"/> class.
                public ISesTxCld(int index, object onlineData)
                    // ReSharper restore UnusedParameter.Local
                    : base(OidBase + ".8.1.5.7.1.2.1.12.{0}", index)
                {
                    RowIndex = index;
                }
                public override ISnmpData Data
                {
                    get
                    {
                        Int32 StdInfo = 0;
                        U5kSCVMib.DataGet((object gdata) =>
                        {
                            dynamic ddata = gdata;
                            StdInfo = (int)(ddata.STDG.RdSessions[RowIndex - 1].tx_cld);
                        });
                        return new Integer32(StdInfo);
                    }
                    set { throw new AccessFailureException(); }
                }
            }
            internal sealed class ISesTxOwd : ScalarObject
            {
                private readonly int RowIndex = 0;
                /// <summary>
                /// Initializes a new instance of the <see cref="ISesTxOwd"/> class.
                public ISesTxOwd(int index, object onlineData)
                    // ReSharper restore UnusedParameter.Local
                    : base(OidBase + ".8.1.5.7.1.2.1.13.{0}", index)
                {
                    RowIndex = index;
                }
                public override ISnmpData Data
                {
                    get
                    {
                        Int32 StdInfo = 0;
                        U5kSCVMib.DataGet((object gdata) =>
                        {
                            dynamic ddata = gdata;
                            StdInfo = (int)(ddata.STDG.RdSessions[RowIndex - 1].tx_owd);
                        });
                        return new Integer32(StdInfo);
                    }
                    set { throw new AccessFailureException(); }
                }
            }
            internal sealed class ISesRxRtp : ScalarObject
            {
                private readonly int RowIndex = 0;
                /// <summary>
                /// Initializes a new instance of the <see cref="ISesRxRtp"/> class.
                public ISesRxRtp(int index, object onlineData)
                    // ReSharper restore UnusedParameter.Local
                    : base(OidBase + ".8.1.5.7.1.2.1.14.{0}", index)
                {
                    RowIndex = index;
                }
                public override ISnmpData Data
                {
                    get
                    {
                        Int32 StdInfo = 0;
                        U5kSCVMib.DataGet((object gdata) =>
                        {
                            dynamic ddata = gdata;
                            StdInfo = (int)(ddata.STDG.RdSessions[RowIndex - 1].rx_rtp);
                        });
                        return new Integer32(StdInfo);
                    }
                    set { throw new AccessFailureException(); }
                }
            }
            internal sealed class ISesRxQidx : ScalarObject
            {
                private readonly int RowIndex = 0;
                /// <summary>
                /// Initializes a new instance of the <see cref="ISesRxQidx"/> class.
                public ISesRxQidx(int index, object onlineData)
                    // ReSharper restore UnusedParameter.Local
                    : base(OidBase + ".8.1.5.7.1.2.1.15.{0}", index)
                {
                    RowIndex = index;
                }
                public override ISnmpData Data
                {
                    get
                    {
                        Int32 StdInfo = 0;
                        U5kSCVMib.DataGet((object gdata) =>
                        {
                            dynamic ddata = gdata;
                            StdInfo = (int)(ddata.STDG.RdSessions[RowIndex - 1].rx_qidx);
                        });
                        return new Integer32(StdInfo);
                    }
                    set { throw new AccessFailureException(); }
                }
            }

            /// <summary>
            /// Initializes a new instance of the <see cref="RecTable"/> class.
            /// </summary>
            System.Threading.ManualResetEvent supervisor = new System.Threading.ManualResetEvent(false);
            public RdSesTable()
            {
                LoadElements();
                Task.Factory.StartNew(() =>
                {
                    while (supervisor != null && supervisor.WaitOne(TimeSpan.FromSeconds(5))==false)
                    {
                        int count = 0;
                        U5kSCVMib.DataGet((object gdata) =>
                        {
                            dynamic ddata = gdata;
                            count = ddata.STDG.RdSessions.Count;
                        });

                        if (count != LastCount)
                            LoadElements();
                    }
                });
            }

            int LastCount = 0;
            private readonly IList<ScalarObject> _elements = new List<ScalarObject>();
            private void LoadElements()
            {
                _elements.Clear();
                var columnTypes = new[]
                {
                    typeof(ISesIndex),
                    typeof(SSesFreq),
                    typeof(ISesFType),
                    typeof(ISesPrio),
                    typeof(ISesFStd),
                    typeof(ISesCldt),
                    typeof(ISesBssw),
                    typeof(SSesUri),
                    typeof(SSesType),
                    typeof(ISesStd),
                    typeof(ISesTxRtp),
                    typeof(ISesTxCld),
                    typeof(ISesTxOwd),
                    typeof(ISesRxRtp),
                    typeof(ISesRxQidx),
                };
                int count = 0;
                U5kSCVMib.DataGet((object gdata) =>
                {
                    dynamic ddata = gdata;
                    count = ddata.STDG.RdSessions.Count;
                });

                foreach (var type in columnTypes)
                {
                    for (int i = 0; i < count; i++)
                    {
                        _elements.Add((ScalarObject)Activator.CreateInstance(type, new object[] { i + 1, null }));
                    }
                }
                LastCount = count;
            }
            public void Dispose()
            {
                supervisor?.Set();
                supervisor = null;
            }
            protected override IEnumerable<ScalarObject> Objects
            {
                get { return _elements; }
            }
        }

        /** */
        internal sealed class MNTableLen : ScalarObject
        {
            /// <summary>
            /// Initializes a new instance of the <see cref="MNTableLen"/> class.
            /// </summary>
            public MNTableLen()
                : base(OidBase + ".8.1.5.7.2.1.0")
            {
            }
            /// <summary>
            /// 
            /// </summary>
            public override ISnmpData Data
            {
                get
                {
                    int data = 0;
                    U5kSCVMib.DataGet((object gdata) =>
                    {
                        dynamic ddata = gdata;
                        data = ddata.STDG.RdMNData.Count;
                    });
                    return new Integer32(data);
                }
                set { throw new AccessFailureException(); }
            }
        }
        internal sealed class MNTable : TableObject, IDisposable
        {
            internal sealed class IMNIndex : ScalarObject
            {
                readonly ISnmpData _data;
                /// <summary>
                /// Initializes a new instance of the <see cref="IMNIndex"/> class.
                public IMNIndex(int index, object onlineData)
                    : base(OidBase + ".8.1.5.7.2.2.1.1.{0}", index)
                {
                    _data = new Integer32(index);
                }
                public override ISnmpData Data
                {
                    get { return _data; }
                    set { throw new AccessFailureException(); }
                }
            }
            internal sealed class SMNId : ScalarObject
            {
                private readonly int RowIndex = 0;
                /// <summary>
                /// Initializes a new instance of the <see cref="SMNId"/> class.
                public SMNId(int index, object onlineData)
                    // ReSharper restore UnusedParameter.Local
                    : base(OidBase + ".8.1.5.7.2.2.1.2.{0}", index)
                {
                    RowIndex = index;
                }
                public override ISnmpData Data
                {
                    get
                    {
                        String DevName = "Error";
                        U5kSCVMib.DataGet((object gdata) =>
                        {
                            dynamic ddata = gdata;
                            string valor = ddata.STDG.RdMNData[RowIndex - 1].equ;
                            DevName = valor ?? "---";
                        });

                        return new OctetString(DevName);
                    }
                    set { throw new AccessFailureException(); }
                }
            }
            internal sealed class IMNGrp : ScalarObject
            {
                private readonly int RowIndex = 0;
                /// <summary>
                /// Initializes a new instance of the <see cref="IMNGrp"/> class.
                public IMNGrp(int index, object onlineData)
                    // ReSharper restore UnusedParameter.Local
                    : base(OidBase + ".8.1.5.7.2.2.1.3.{0}", index)
                {
                    RowIndex = index;
                }
                public override ISnmpData Data
                {
                    get
                    {
                        Int32 StdInfo = 0;
                        U5kSCVMib.DataGet((object gdata) =>
                        {
                            dynamic ddata = gdata;
                            StdInfo = (int)(ddata.STDG.RdMNData[RowIndex - 1].grp);
                        });
                        return new Integer32(StdInfo);
                    }
                    set { throw new AccessFailureException(); }
                }
            }
            internal sealed class IMNMod : ScalarObject
            {
                private readonly int RowIndex = 0;
                /// <summary>
                /// Initializes a new instance of the <see cref="IMNMod"/> class.
                public IMNMod(int index, object onlineData)
                    // ReSharper restore UnusedParameter.Local
                    : base(OidBase + ".8.1.5.7.2.2.1.4.{0}", index)
                {
                    RowIndex = index;
                }
                public override ISnmpData Data
                {
                    get
                    {
                        Int32 StdInfo = 0;
                        U5kSCVMib.DataGet((object gdata) =>
                        {
                            dynamic ddata = gdata;
                            StdInfo = (int)(ddata.STDG.RdMNData[RowIndex - 1].mod);
                        });
                        return new Integer32(StdInfo);
                    }
                    set { throw new AccessFailureException(); }
                }
            }
            internal sealed class IMNTyp : ScalarObject
            {
                private readonly int RowIndex = 0;
                /// <summary>
                /// Initializes a new instance of the <see cref="IMNTyp"/> class.
                public IMNTyp(int index, object onlineData)
                    // ReSharper restore UnusedParameter.Local
                    : base(OidBase + ".8.1.5.7.2.2.1.5.{0}", index)
                {
                    RowIndex = index;
                }
                public override ISnmpData Data
                {
                    get
                    {
                        Int32 StdInfo = 0;
                        U5kSCVMib.DataGet((object gdata) =>
                        {
                            dynamic ddata = gdata;
                            StdInfo = (int)(ddata.STDG.RdMNData[RowIndex - 1].tip);
                        });
                        return new Integer32(StdInfo);
                    }
                    set { throw new AccessFailureException(); }
                }
            }
            internal sealed class IMNStd : ScalarObject
            {
                private readonly int RowIndex = 0;
                /// <summary>
                /// Initializes a new instance of the <see cref="IMNStd"/> class.
                public IMNStd(int index, object onlineData)
                    // ReSharper restore UnusedParameter.Local
                    : base(OidBase + ".8.1.5.7.2.2.1.6.{0}", index)
                {
                    RowIndex = index;
                }
                public override ISnmpData Data
                {
                    get
                    {
                        Int32 StdInfo = 0;
                        U5kSCVMib.DataGet((object gdata) =>
                        {
                            dynamic ddata = gdata;
                            StdInfo = (int)(ddata.STDG.RdMNData[RowIndex - 1].std);
                        });
                        return new Integer32(StdInfo);
                    }
                    set { throw new AccessFailureException(); }
                }
            }
            internal sealed class SMNFreq : ScalarObject
            {
                private readonly int RowIndex = 0;
                /// <summary>
                /// Initializes a new instance of the <see cref="SMNFreq"/> class.
                public SMNFreq(int index, object onlineData)
                    // ReSharper restore UnusedParameter.Local
                    : base(OidBase + ".8.1.5.7.2.2.1.7.{0}", index)
                {
                    RowIndex = index;
                }
                public override ISnmpData Data
                {
                    get
                    {
                        String DevName = "Error";
                        U5kSCVMib.DataGet((object gdata) =>
                        {
                            dynamic ddata = gdata;
                            string valor = ddata.STDG.RdMNData[RowIndex - 1].frec;
                            DevName = valor ?? "---";
                        });

                        return new OctetString(DevName);
                    }
                    set { throw new AccessFailureException(); }
                }
            }
            internal sealed class IMNPrio : ScalarObject
            {
                private readonly int RowIndex = 0;
                /// <summary>
                /// Initializes a new instance of the <see cref="IMNPrio"/> class.
                public IMNPrio(int index, object onlineData)
                    // ReSharper restore UnusedParameter.Local
                    : base(OidBase + ".8.1.5.7.2.2.1.8.{0}", index)
                {
                    RowIndex = index;
                }
                public override ISnmpData Data
                {
                    get
                    {
                        Int32 StdInfo = 0;
                        U5kSCVMib.DataGet((object gdata) =>
                        {
                            dynamic ddata = gdata;
                            int? valor = (int)(ddata.STDG.RdMNData[RowIndex - 1].prio);
                            StdInfo = valor ?? -1;
                        });
                        return new Integer32(StdInfo);
                    }
                    set { throw new AccessFailureException(); }
                }
            }
            internal sealed class IMNSip : ScalarObject
            {
                private readonly int RowIndex = 0;
                /// <summary>
                /// Initializes a new instance of the <see cref="IMNSip"/> class.
                public IMNSip(int index, object onlineData)
                    // ReSharper restore UnusedParameter.Local
                    : base(OidBase + ".8.1.5.7.2.2.1.9.{0}", index)
                {
                    RowIndex = index;
                }
                public override ISnmpData Data
                {
                    get
                    {
                        Int32 StdInfo = 0;
                        U5kSCVMib.DataGet((object gdata) =>
                        {
                            dynamic ddata = gdata;
                            int? valor = (int)(ddata.STDG.RdMNData[RowIndex - 1].sip);
                            StdInfo = valor ?? -1;
                        });
                        return new Integer32(StdInfo);
                    }
                    set { throw new AccessFailureException(); }
                }
            }
            internal sealed class SMNIp : ScalarObject
            {
                private readonly int RowIndex = 0;
                /// <summary>
                /// Initializes a new instance of the <see cref="SMNIp"/> class.
                public SMNIp(int index, object onlineData)
                    // ReSharper restore UnusedParameter.Local
                    : base(OidBase + ".8.1.5.7.2.2.1.10.{0}", index)
                {
                    RowIndex = index;
                }
                public override ISnmpData Data
                {
                    get
                    {
                        String DevName = "Error";
                        U5kSCVMib.DataGet((object gdata) =>
                        {
                            dynamic ddata = gdata;
                            string valor = ddata.STDG.RdMNData[RowIndex - 1].ip;
                            DevName = valor ?? "---";
                        });

                        return new OctetString(DevName);
                    }
                    set { throw new AccessFailureException(); }
                }
            }
            internal sealed class SMNSite : ScalarObject
            {
                private readonly int RowIndex = 0;
                /// <summary>
                /// Initializes a new instance of the <see cref="SMNSite"/> class.
                public SMNSite(int index, object onlineData)
                    // ReSharper restore UnusedParameter.Local
                    : base(OidBase + ".8.1.5.7.2.2.1.11.{0}", index)
                {
                    RowIndex = index;
                }
                public override ISnmpData Data
                {
                    get
                    {
                        String DevName = "Error";
                        U5kSCVMib.DataGet((object gdata) =>
                        {
                            dynamic ddata = gdata;
                            string valor = ddata.STDG.RdMNData[RowIndex - 1].emp;
                            DevName = valor ?? "---";
                        });

                        return new OctetString(DevName);
                    }
                    set { throw new AccessFailureException(); }
                }
            }
            internal sealed class IMNFType : ScalarObject
            {
                private readonly int RowIndex = 0;
                /// <summary>
                /// Initializes a new instance of the <see cref="IMNFType"/> class.
                public IMNFType(int index, object onlineData)
                    // ReSharper restore UnusedParameter.Local
                    : base(OidBase + ".8.1.5.7.2.2.1.12.{0}", index)
                {
                    RowIndex = index;
                }
                public override ISnmpData Data
                {
                    get
                    {
                        Int32 StdInfo = 0;
                        U5kSCVMib.DataGet((object gdata) =>
                        {
                            dynamic ddata = gdata;
                            int? valor = (int)(ddata.STDG.RdMNData[RowIndex - 1].tfrec);
                            StdInfo = valor ?? -1;
                        });
                        return new Integer32(StdInfo);
                    }
                    set { throw new AccessFailureException(); }
                }
            }

            /// <summary>
            /// Initializes a new instance of the <see cref="RecTable"/> class.
            /// </summary>
            public MNTable()
            {
                LoadElements();

                /** Refresca la Tabla  */
                Task.Factory.StartNew(() =>
                {
                    while (supervisor != null && supervisor.WaitOne(TimeSpan.FromSeconds(5)) == false)
                    {
                        int count = 0;
                        U5kSCVMib.DataGet((object gdata) =>
                        {
                            dynamic ddata = gdata;
                            count = ddata.STDG.RdMNData.Count;
                        });

                        if (count != LastCount)
                            LoadElements();
                    }
                });
            }

            private int LastCount = 0;
            private readonly IList<ScalarObject> _elements = new List<ScalarObject>();
            private void LoadElements()
            {
                _elements.Clear();
                var columnTypes = new[]
                {
                    typeof(IMNIndex),
                    typeof(SMNId),
                    typeof(IMNGrp),
                    typeof(IMNMod),
                    typeof(IMNTyp),
                    typeof(IMNStd),
                    typeof(SMNFreq),
                    typeof(IMNPrio),
                    typeof(IMNSip),
                    typeof(SMNIp),
                    typeof(SMNSite),
                    typeof(IMNFType),
                };
                int count = 0;
                U5kSCVMib.DataGet((object gdata) =>
                {
                    dynamic ddata = gdata;
                    count = ddata.STDG.RdMNData.Count;
                });

                foreach (var type in columnTypes)
                {
                    for (int i = 0; i < count; i++)
                    {
                        _elements.Add((ScalarObject)Activator.CreateInstance(type, new object[] { i + 1, null }));
                    }
                }
                LastCount = count;
            }

            protected System.Threading.ManualResetEvent supervisor = new System.Threading.ManualResetEvent(false);
            public void Dispose()
            {
                supervisor?.Set();
                supervisor = null;
            }

            protected override IEnumerable<ScalarObject> Objects
            {
                get { return _elements; }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        protected override void LoadElements()
        {
            snmpObjects = new List<SnmpObjectBase>()
            {
                new RdSesTableLen(), new RdSesTable(),
                new MNTableLen(), new MNTable(),
            };
        }
        public U5kSCVMibRadGroup() : base(U5kSCVMib.OidBase)
        {
        }
    }
    /// <summary>
    /// Grupo de Estado Global.
    /// </summary>
    class U5kSCVMibQtyGroup : U5kMibGroup
    {
        /** Clases de los elementos del grupo */
        internal sealed class StdgQuality : ScalarObject
        {
            /// <summary>
            /// Initializes a new instance of the <see cref="StdgQuality"/> class.
            /// </summary>
            public StdgQuality()
                : base(OidBase + ".8.1.5.8.1.0")
            {
            }
            /// <summary>
            /// 
            /// </summary>
            public override ISnmpData Data
            {
                get
                {
                    Int32 data = 0;
                    U5kSCVMib.DataGet((object gdata) =>
                    {
                        dynamic ddata = gdata;
                        data = ddata.STDG.QualityItems.gral;
                    });
                    return new Integer32(data);
                }
                set { throw new AccessFailureException(); }
            }
        }
        internal sealed class TopsQuality : ScalarObject
        {
            /// <summary>
            /// Initializes a new instance of the <see cref="TopsQuality"/> class.
            /// </summary>
            public TopsQuality()
                : base(OidBase + ".8.1.5.8.2.0")
            {
            }
            /// <summary>
            /// 
            /// </summary>
            public override ISnmpData Data
            {
                get
                {
                    Int32 data = 0;
                    U5kSCVMib.DataGet((object gdata) =>
                    {
                        dynamic ddata = gdata;
                        data = ddata.STDG.QualityItems.tops;
                    });
                    return new Integer32(data);
                }
                set { throw new AccessFailureException(); }
            }
        }
        internal sealed class GwsQuality : ScalarObject
        {
            /// <summary>
            /// Initializes a new instance of the <see cref="GwsQuality"/> class.
            /// </summary>
            public GwsQuality()
                : base(OidBase + ".8.1.5.8.3.0")
            {
            }
            /// <summary>
            /// 
            /// </summary>
            public override ISnmpData Data
            {
                get
                {
                    Int32 data = 0;
                    U5kSCVMib.DataGet((object gdata) =>
                    {
                        dynamic ddata = gdata;
                        data = ddata.STDG.QualityItems.gws;
                    });
                    return new Integer32(data);
                }
                set { throw new AccessFailureException(); }
            }
        }
        internal sealed class ExtsQuality : ScalarObject
        {
            /// <summary>
            /// Initializes a new instance of the <see cref="ExtsQuality"/> class.
            /// </summary>
            public ExtsQuality()
                : base(OidBase + ".8.1.5.8.4.0")
            {
            }
            /// <summary>
            /// 
            /// </summary>
            public override ISnmpData Data
            {
                get
                {
                    Int32 data = 0;
                    U5kSCVMib.DataGet((object gdata) =>
                    {
                        dynamic ddata = gdata;
                        data = ddata.STDG.QualityItems.exts;
                    });
                    return new Integer32(data);
                }
                set { throw new AccessFailureException(); }
            }
        }
        internal sealed class PhoneQuality : ScalarObject
        {
            /// <summary>
            /// Initializes a new instance of the <see cref="PhoneQuality"/> class.
            /// </summary>
            public PhoneQuality()
                : base(OidBase + ".8.1.5.8.5.0")
            {
            }
            /// <summary>
            /// 
            /// </summary>
            public override ISnmpData Data
            {
                get
                {
                    Int32 data = 0;
                    U5kSCVMib.DataGet((object gdata) =>
                    {
                        dynamic ddata = gdata;
                        data = ddata.STDG.QualityItems.phone;
                    });
                    return new Integer32(data);
                }
                set { throw new AccessFailureException(); }
            }
        }
        internal sealed class RadioQuality : ScalarObject
        {
            /// <summary>
            /// Initializes a new instance of the <see cref="RadioQuality"/> class.
            /// </summary>
            public RadioQuality()
                : base(OidBase + ".8.1.5.8.6.0")
            {
            }
            /// <summary>
            /// 
            /// </summary>
            public override ISnmpData Data
            {
                get
                {
                    Int32 data = 0;
                    U5kSCVMib.DataGet((object gdata) =>
                    {
                        dynamic ddata = gdata;
                        data = ddata.STDG.QualityItems.radio;
                    });
                    return new Integer32(data);
                }
                set { throw new AccessFailureException(); }
            }
        }
#if DEBUG
        internal sealed class TestQuality : ScalarObject
        {
            int valor = 0;
            /// <summary>
            /// Initializes a new instance of the <see cref="TestQuality"/> class.
            /// </summary>
            public TestQuality()
                : base(OidBase + ".8.1.5.8.7.0")
            {
            }
            /// <summary>
            /// 
            /// </summary>
            public override ISnmpData Data
            {
                get
                {
                    return new Integer32(valor);
                }
                set
                {
                    if (value is Integer32 data)
                    {
                        valor = data.ToInt32();
                    }
                    else
                    {
                        throw new AccessFailureException();
                    }                
                }
            }
        }
#endif

        /// <summary>
        /// 
        /// </summary>
        protected override void LoadElements()
        {
            snmpObjects = new List<SnmpObjectBase>()
            {
                new StdgQuality(), new TopsQuality(), new GwsQuality(), 
                new ExtsQuality(), new PhoneQuality(), new RadioQuality(),
#if DEBUG
                new TestQuality()
#endif
            };
        }
        /// <summary>
        /// 
        /// </summary>
        public U5kSCVMibQtyGroup() : base(U5kSCVMib.OidBase)
        {
        }
    }

}
