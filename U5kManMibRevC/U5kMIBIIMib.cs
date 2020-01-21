using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net/*.Sockets*/;
using System.Net.NetworkInformation;
using System.Dynamic;

//using System.Management;

using Lextm.SharpSnmpLib;
using Lextm.SharpSnmpLib.Pipeline;
using Lextm.SharpSnmpLib.Objects;


namespace U5kManMibRevC
{
    /// <summary>
    /// 
    /// </summary>
    class U5kMIBIISystemGroup : U5kMibGroup
    {
        public U5kMIBIISystemGroup() : base() { }
        /// <summary>
        /// 
        /// </summary>
        sealed class U5kSysDescr : ScalarObject
        {
#if NET452
        private readonly OctetString _description =
            new OctetString(string.Format(CultureInfo.InvariantCulture, "#SNMP Agent on {0}", Environment.OSVersion));
#else
            private readonly OctetString _description =
                new OctetString("Ulises V 5000. Sistema de Comunicaciones Vocales. Nucleo CC 2014..2019.");
#endif
            /// <summary>
            /// Initializes a new instance of the <see cref="U5kSysDescr"/> class.
            /// </summary>
            public U5kSysDescr()
                : base(new ObjectIdentifier("1.3.6.1.2.1.1.1.0"))
            {
            }

            /// <summary>
            /// Gets or sets the data.
            /// </summary>
            /// <value>The data.</value>
            public override ISnmpData Data
            {
                get { return _description; }
                set { throw new AccessFailureException(); }
            }
        }
        /// <summary>
        /// 
        /// </summary>
        sealed class U5kSysObjectId : ScalarObject
        {
            private readonly ObjectIdentifier _objectId = new ObjectIdentifier(".1.3.6.1.4.1.7916.8.1");

            /// <summary>
            /// Initializes a new instance of the <see cref="U5kSysObjectId"/> class.
            /// </summary>
            public U5kSysObjectId()
                : base(new ObjectIdentifier("1.3.6.1.2.1.1.2.0"))
            {
            }

            /// <summary>
            /// Gets or sets the data.
            /// </summary>
            /// <value>The data.</value>
            public override ISnmpData Data
            {
                get { return _objectId; }
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
            new U5kSysDescr(), new U5kSysObjectId(), new SysUpTime(),
            new SysContact() { Data = new OctetString("NUCLEO-CC DT. MADRID. SPAIN") },
            new SysName() { Data = new OctetString("NU-UV5K-2.5.X") },
            new SysLocation() { Data = new OctetString("NUCLEO-CC LABS") },
            new SysServices(),
            new SysORLastChange(),
            new SysORTable()
            };
        }
    }
    /// <summary>
    /// 
    /// </summary>
    class U5kMIBIIInterfacesGroup : U5kMibGroup
    {
        public U5kMIBIIInterfacesGroup() : base() { }
        protected override void LoadElements()
        {
            snmpObjects = new List<SnmpObjectBase>()
            {
                //new IfNumber(), new IfTable()
                new IfNumberLoc(), new IfTableLoc()
            };
        }

        internal sealed class IfNumberLoc : ScalarObject
        {
            /// <summary>
            /// Initializes a new instance of the <see cref="IfNumberLoc"/> class.
            /// </summary>
            public IfNumberLoc()
                : base(".1.3.6.1.2.1.2.1.0")
            {
            }
            /// <summary>
            /// 
            /// </summary>
            public override ISnmpData Data
            {
                get
                {
                    var number = MibIITableHelpers.IfData.IfTable.Count();
                    return new Integer32(number);
                }
                set
                {
                    throw new AccessFailureException();
                }
            }

        }
        internal sealed class IfTableLoc : TableObject
        {
            internal sealed class IfIndex : ScalarObject
            {
                private int Index { get; set; }
                /// <summary>
                /// Initializes a new instance of the <see cref="IfIndex"/> class.
                public IfIndex(int index, object onlineData)
                    : base(".1.3.6.1.2.1.2.2.1.1.{0}", index)
                {
                    Index = index-1;
                }
                public override ISnmpData Data
                {
                    get { return new Integer32((int)MibIITableHelpers.IfData.IfTable[Index].dwIndex); }
                    set { throw new AccessFailureException(); }
                }
            }
            internal sealed class IfDescr : ScalarObject
            {
                private int Index { get; set; }
                /// <summary>
                /// Initializes a new instance of the <see cref="IfDescr"/> class.
                public IfDescr(int index, object onlineData)
                    : base(".1.3.6.1.2.1.2.2.1.2.{0}", index)
                {
                    Index = index-1;
                }
                public override ISnmpData Data
                {
                    get
                    {
                        var row = MibIITableHelpers.IfData.IfTable[Index];
                        return new OctetString(Encoding.ASCII.GetString(row.bDescr, 0, (int)row.dwDescrLen - 1));
                    }
                    set { throw new AccessFailureException(); }
                }
            }
            internal sealed class IfType : ScalarObject
            {
                private int Index { get; set; }
                /// <summary>
                /// Initializes a new instance of the <see cref="IfType"/> class.
                public IfType(int index, object onlineData)
                    : base(".1.3.6.1.2.1.2.2.1.3.{0}", index)
                {
                    Index = index-1;
                }
                public override ISnmpData Data
                {
                    get
                    {
                        var row = MibIITableHelpers.IfData.IfTable[Index];
                        return new Integer32((int)row.dwType);
                    }
                    set { throw new AccessFailureException(); }
                }
            }
            internal sealed class IfMtu : ScalarObject
            {
                private int Index { get; set; }
                /// <summary>
                /// Initializes a new instance of the <see cref="IfMtu"/> class.
                public IfMtu(int index, object onlineData)
                    : base(".1.3.6.1.2.1.2.2.1.4.{0}", index)
                {
                    Index = index-1;
                }
                public override ISnmpData Data
                {
                    get
                    {
                        var row = MibIITableHelpers.IfData.IfTable[Index];
                        return new Integer32((int)row.dwMtu);
                    }
                    set { throw new AccessFailureException(); }
                }
            }
            internal sealed class IfSpeed : ScalarObject
            {
                private int Index { get; set; }
                /// <summary>
                /// Initializes a new instance of the <see cref="IfSpeed"/> class.
                public IfSpeed(int index, object onlineData)
                    : base(".1.3.6.1.2.1.2.2.1.5.{0}", index)
                {
                    Index = index-1;
                }
                public override ISnmpData Data
                {
                    get
                    {
                        var row = MibIITableHelpers.IfData.IfTable[Index];
                        return new Integer32((int)row.dwSpeed);
                    }
                    set { throw new AccessFailureException(); }
                }
            }
            internal sealed class IfPhysAddress : ScalarObject
            {
                private int Index { get; set; }
                /// <summary>
                /// Initializes a new instance of the <see cref="IfPhysAddress"/> class.
                public IfPhysAddress(int index, object onlineData)
                    : base(".1.3.6.1.2.1.2.2.1.6.{0}", index)
                {
                    Index = index-1;
                }
                public override ISnmpData Data
                {
                    get
                    {
                        var row = MibIITableHelpers.IfData.IfTable[Index];
                        var first = row.bPhysAddr[0];
                        var last = row.bPhysAddr[7];
                        string PhysAddress = "";
                        if (first == 0 && last != 0)
                        {
                            PhysAddress = row.bPhysAddr[0].ToString("X2") + '-' +
                            row.bPhysAddr[1].ToString("X2") + '-' +
                            row.bPhysAddr[2].ToString("X2") + '-' +
                            row.bPhysAddr[3].ToString("X2") + '-' +
                            row.bPhysAddr[4].ToString("X2") + '-' +
                            row.bPhysAddr[5].ToString("X2") + '-' +
                            row.bPhysAddr[6].ToString("X2") + '-' +
                            row.bPhysAddr[7].ToString("X2");
                        }
                        else if (first != 0)
                        {
                            PhysAddress = row.bPhysAddr[0].ToString("X2") + '-' +
                            row.bPhysAddr[1].ToString("X2") + '-' +
                            row.bPhysAddr[2].ToString("X2") + '-' +
                            row.bPhysAddr[3].ToString("X2") + '-' +
                            row.bPhysAddr[4].ToString("X2") + '-' +
                            row.bPhysAddr[5].ToString("X2");
                        }
                        return new OctetString(PhysAddress);
                    }
                    set { throw new AccessFailureException(); }
                }
            }
            internal sealed class IfAdminStatus : ScalarObject
            {
                private int Index { get; set; }
                /// <summary>
                /// Initializes a new instance of the <see cref="IfAdminStatus"/> class.
                public IfAdminStatus(int index, object onlineData)
                    : base(".1.3.6.1.2.1.2.2.1.7.{0}", index)
                {
                    Index = index-1;
                }
                public override ISnmpData Data
                {
                    get
                    {
                        var row = MibIITableHelpers.IfData.IfTable[Index];
                        return new Integer32((int)row.dwAdminStatus);
                    }
                    set { throw new AccessFailureException(); }
                }
            }
            internal sealed class IfOperStatus : ScalarObject
            {
                private int Index { get; set; }
                /// <summary>
                /// Initializes a new instance of the <see cref="IfOperStatus"/> class.
                public IfOperStatus(int index, object onlineData)
                    : base(".1.3.6.1.2.1.2.2.1.8.{0}", index)
                {
                    Index = index - 1;
                }
                public override ISnmpData Data
                {
                    get
                    {
                        var row = MibIITableHelpers.IfData.IfTable[Index];
                        return new Integer32(MibIITableHelpers.IfData.SnmpOperationalStatus((int)row.dwOperStatus, (int)row.dwAdminStatus));
                    }
                    set { throw new AccessFailureException(); }
                }
            }
            internal sealed class IfLastChange : ScalarObject
            {
                private int Index { get; set; }
                /// <summary>
                /// Initializes a new instance of the <see cref="IfLastChange"/> class.
                public IfLastChange(int index, object onlineData)
                    : base(".1.3.6.1.2.1.2.2.1.9.{0}", index)
                {
                    Index = index - 1;
                }
                public override ISnmpData Data
                {
                    get
                    {
                        var row = MibIITableHelpers.IfData.IfTable[Index];
                        return new TimeTicks(row.dwLastChange);
                    }
                    set { throw new AccessFailureException(); }
                }
            }
            internal sealed class IfInOctects : ScalarObject
            {
                private int Index { get; set; }
                /// <summary>
                /// Initializes a new instance of the <see cref="IfInOctects"/> class.
                public IfInOctects(int index, object onlineData)
                    : base(".1.3.6.1.2.1.2.2.1.10.{0}", index)
                {
                    Index = index - 1;
                }
                public override ISnmpData Data
                {
                    get
                    {
                        var row = MibIITableHelpers.IfData.IfTable[Index];
                        return new Integer32((int)row.dwInOctets);
                    }
                    set { throw new AccessFailureException(); }
                }
            }
            internal sealed class IfInUcastPkts : ScalarObject
            {
                private int Index { get; set; }
                /// <summary>
                /// Initializes a new instance of the <see cref="IfInUcastPkts"/> class.
                public IfInUcastPkts(int index, object onlineData)
                    : base(".1.3.6.1.2.1.2.2.1.11.{0}", index)
                {
                    Index = index - 1;
                }
                public override ISnmpData Data
                {
                    get
                    {
                        var row = MibIITableHelpers.IfData.IfTable[Index];
                        return new Integer32((int)row.dwInUcastPkts);
                    }
                    set { throw new AccessFailureException(); }
                }
            }
            internal sealed class IfInNUcastPkts : ScalarObject
            {
                private int Index { get; set; }
                /// <summary>
                /// Initializes a new instance of the <see cref="IfInNUcastPkts"/> class.
                public IfInNUcastPkts(int index, object onlineData)
                    : base(".1.3.6.1.2.1.2.2.1.12.{0}", index)
                {
                    Index = index - 1;
                }
                public override ISnmpData Data
                {
                    get
                    {
                        var row = MibIITableHelpers.IfData.IfTable[Index];
                        return new Integer32((int)row.dwInNUcastPkts);
                    }
                    set { throw new AccessFailureException(); }
                }
            }
            internal sealed class IfInDiscards : ScalarObject
            {
                private int Index { get; set; }
                /// <summary>
                /// Initializes a new instance of the <see cref="IfInDiscards"/> class.
                public IfInDiscards(int index, object onlineData)
                    : base(".1.3.6.1.2.1.2.2.1.13.{0}", index)
                {
                    Index = index - 1;
                }
                public override ISnmpData Data
                {
                    get
                    {
                        var row = MibIITableHelpers.IfData.IfTable[Index];
                        return new Integer32((int)row.dwInDiscards);
                    }
                    set { throw new AccessFailureException(); }
                }
            }
            internal sealed class IfInErrors : ScalarObject
            {
                private int Index { get; set; }
                /// <summary>
                /// Initializes a new instance of the <see cref="IfInErrors"/> class.
                public IfInErrors(int index, object onlineData)
                    : base(".1.3.6.1.2.1.2.2.1.14.{0}", index)
                {
                    Index = index - 1;
                }
                public override ISnmpData Data
                {
                    get
                    {
                        var row = MibIITableHelpers.IfData.IfTable[Index];
                        return new Integer32((int)row.dwInErrors);
                    }
                    set { throw new AccessFailureException(); }
                }
            }
            internal sealed class IfInUnknowProtos : ScalarObject
            {
                private int Index { get; set; }
                /// <summary>
                /// Initializes a new instance of the <see cref="IfInUnknowProtos"/> class.
                public IfInUnknowProtos(int index, object onlineData)
                    : base(".1.3.6.1.2.1.2.2.1.15.{0}", index)
                {
                    Index = index - 1;
                }
                public override ISnmpData Data
                {
                    get
                    {
                        var row = MibIITableHelpers.IfData.IfTable[Index];
                        return new Integer32((int)row.dwInUnknownProtos);
                    }
                    set { throw new AccessFailureException(); }
                }
            }
            internal sealed class IfOutOctets : ScalarObject
            {
                private int Index { get; set; }
                /// <summary>
                /// Initializes a new instance of the <see cref="IfOutOctets"/> class.
                public IfOutOctets(int index, object onlineData)
                    : base(".1.3.6.1.2.1.2.2.1.16.{0}", index)
                {
                    Index = index - 1;
                }
                public override ISnmpData Data
                {
                    get
                    {
                        var row = MibIITableHelpers.IfData.IfTable[Index];
                        return new Integer32((int)row.dwOutOctets);
                    }
                    set { throw new AccessFailureException(); }
                }
            }
            internal sealed class IfOutUcastPkts : ScalarObject
            {
                private int Index { get; set; }
                /// <summary>
                /// Initializes a new instance of the <see cref="IfOutUcastPkts"/> class.
                public IfOutUcastPkts(int index, object onlineData)
                    : base(".1.3.6.1.2.1.2.2.1.17.{0}", index)
                {
                    Index = index - 1;
                }
                public override ISnmpData Data
                {
                    get
                    {
                        var row = MibIITableHelpers.IfData.IfTable[Index];
                        return new Integer32((int)row.dwOutUcastPkts);
                    }
                    set { throw new AccessFailureException(); }
                }
            }
            internal sealed class IfOutNUcastPkts : ScalarObject
            {
                private int Index { get; set; }
                /// <summary>
                /// Initializes a new instance of the <see cref="IfOutNUcastPkts"/> class.
                public IfOutNUcastPkts(int index, object onlineData)
                    : base(".1.3.6.1.2.1.2.2.1.18.{0}", index)
                {
                    Index = index - 1;
                }
                public override ISnmpData Data
                {
                    get
                    {
                        var row = MibIITableHelpers.IfData.IfTable[Index];
                        return new Integer32((int)row.dwOutNUcastPkts);
                    }
                    set { throw new AccessFailureException(); }
                }
            }
            internal sealed class IfOutDiscards : ScalarObject
            {
                private int Index { get; set; }
                /// <summary>
                /// Initializes a new instance of the <see cref="IfOutDiscards"/> class.
                public IfOutDiscards(int index, object onlineData)
                    : base(".1.3.6.1.2.1.2.2.1.19.{0}", index)
                {
                    Index = index - 1;
                }
                public override ISnmpData Data
                {
                    get
                    {
                        var row = MibIITableHelpers.IfData.IfTable[Index];
                        return new Integer32((int)row.dwOutDiscards);
                    }
                    set { throw new AccessFailureException(); }
                }
            }
            internal sealed class IfOutErrors : ScalarObject
            {
                private int Index { get; set; }
                /// <summary>
                /// Initializes a new instance of the <see cref="IfOutErrors"/> class.
                public IfOutErrors(int index, object onlineData)
                    : base(".1.3.6.1.2.1.2.2.1.20.{0}", index)
                {
                    Index = index - 1;
                }
                public override ISnmpData Data
                {
                    get
                    {
                        var row = MibIITableHelpers.IfData.IfTable[Index];
                        return new Integer32((int)row.dwOutErrors);
                    }
                    set { throw new AccessFailureException(); }
                }
            }
            internal sealed class IfOutQLen : ScalarObject
            {
                private int Index { get; set; }
                /// <summary>
                /// Initializes a new instance of the <see cref="IfOutQLen"/> class.
                public IfOutQLen(int index, object onlineData)
                    : base(".1.3.6.1.2.1.2.2.1.21.{0}", index)
                {
                    Index = index - 1;
                }
                public override ISnmpData Data
                {
                    get
                    {
                        var row = MibIITableHelpers.IfData.IfTable[Index];
                        return new Integer32((int)row.dwOutQLen);
                    }
                    set { throw new AccessFailureException(); }
                }
            }
            internal sealed class IfSpecific : ScalarObject
            {
                private int Index { get; set; }
                /// <summary>
                /// Initializes a new instance of the <see cref="IfSpecific"/> class.
                public IfSpecific(int index, object onlineData)
                    : base(".1.3.6.1.2.1.2.2.1.22.{0}", index)
                {
                    Index = index - 1;
                }
                public override ISnmpData Data
                {
                    get { return new OctetString(".0.0"); }
                    set { throw new AccessFailureException(); }
                }
            }

            /// <summary>
            /// Initializes a new instance of the <see cref="IfTableLoc"/> class.
            /// </summary>
            public IfTableLoc()
            {
                /** Ejemplo para programar el evento de actualizacion de la tabla */
                NetworkChange.NetworkAddressChanged +=
                    (sender, args) => LoadElements();
#if NET452
            NetworkChange.NetworkAvailabilityChanged +=
                (sender, args) => LoadElements();
#endif                
                LoadElements();
            }

            // ".1.3.6.1.2.1.4.22"
            private readonly IList<ScalarObject> _elements = new List<ScalarObject>();
            private void LoadElements()
            {
                _elements.Clear();

                //var netToMediaTable = MibIITableHelpers.IpNetTable.Data;
                var ifTable = MibIITableHelpers.IfData.IfTable;
                var columnTypes = new[]
                {
                    typeof(IfIndex),
                    typeof(IfDescr),
                    typeof(IfType),
                    typeof(IfMtu),
                    typeof(IfSpeed),
                    typeof(IfPhysAddress),
                    typeof(IfAdminStatus),
                    typeof(IfOperStatus),
                    typeof(IfLastChange),
                    typeof(IfInOctects),
                    typeof(IfInUcastPkts),
                    typeof(IfInNUcastPkts),
                    typeof(IfInDiscards),
                    typeof(IfInErrors),
                    typeof(IfInUnknowProtos),
                    typeof(IfOutOctets),
                    typeof(IfOutUcastPkts),
                    typeof(IfOutNUcastPkts),
                    typeof(IfOutDiscards),
                    typeof(IfOutErrors),
                    typeof(IfOutQLen),
                    typeof(IfSpecific),
                };
                foreach (var type in columnTypes)
                {
                    for (int i = 0; i < ifTable.Count; i++)
                    {
                        _elements.Add((ScalarObject)Activator.CreateInstance(type, new object[] { i + 1, ifTable[i] }));
                    }
                }
            }
            protected override IEnumerable<ScalarObject> Objects
            {
                get { return _elements; }
            }
        }

    }
    /// <summary>
    /// 
    /// </summary>
    class U5kMIBIIIpObjectGroup : U5kMibGroup
    {
        public U5kMIBIIIpObjectGroup() : base() { }

        /** Elementos del Grupo */
        internal sealed class IpForwarding : ScalarObject
        {
            /// <summary>
            /// Initializes a new instance of the <see cref="IpForwarding"/> class.
            /// </summary>
            public IpForwarding()
                : base(".1.3.6.1.2.1.4.1.0")
            {
            }
            /// <summary>
            /// 
            /// </summary>
            public override ISnmpData Data
            {
                get
                {
                    return new Integer32(MibIITableHelpers.IpData.Statistics.dwForwarding);
                }
                set
                {
                    throw new AccessFailureException();
                }
            }

        }
        internal sealed class IpDefaultTTL : ScalarObject
        {
            /// <summary>
            /// Initializes a new instance of the <see cref="IpDefaultTTL"/> class.
            /// </summary>
            public IpDefaultTTL()
                : base(".1.3.6.1.2.1.4.2.0")
            {
            }
            /// <summary>
            /// 
            /// </summary>
            public override ISnmpData Data
            {
                get
                {
                    return new Integer32(MibIITableHelpers.IpData.Statistics.dwDefaultTTL);
                }
                set
                {
                    throw new AccessFailureException();
                }
            }
        }
        internal sealed class IpInReceives : ScalarObject
        {
            /// <summary>
            /// Initializes a new instance of the <see cref="IpInReceives"/> class.
            /// </summary>
            public IpInReceives()
                : base(".1.3.6.1.2.1.4.3.0")
            {
            }
            /// <summary>
            /// 
            /// </summary>
            public override ISnmpData Data
            {
                get
                {
                    return new Counter32(MibIITableHelpers.IpData.Statistics.dwInReceives);
                }
                set
                {
                    throw new AccessFailureException();
                }
            }
        }
        internal sealed class IpHdrErrors : ScalarObject
        {
            /// <summary>
            /// Initializes a new instance of the <see cref="IpHdrErrors"/> class.
            /// </summary>
            public IpHdrErrors()
                : base(".1.3.6.1.2.1.4.4.0")
            {
            }
            /// <summary>
            /// 
            /// </summary>
            public override ISnmpData Data
            {
                get
                {
                    return new Counter32(MibIITableHelpers.IpData.Statistics.dwInHdrErrors);
                }
                set
                {
                    throw new AccessFailureException();
                }
            }
        }
        internal sealed class IpInAddrErrors : ScalarObject
        {
            /// <summary>
            /// Initializes a new instance of the <see cref="IpInAddrErrors"/> class.
            /// </summary>
            public IpInAddrErrors()
                : base(".1.3.6.1.2.1.4.5.0")
            {
            }
            /// <summary>
            /// 
            /// </summary>
            public override ISnmpData Data
            {
                get
                {
                    return new Counter32(MibIITableHelpers.IpData.Statistics.dwInAddrErrors);
                }
                set
                {
                    throw new AccessFailureException();
                }
            }
        }
        internal sealed class IpForwDatagrams : ScalarObject
        {
            /// <summary>
            /// Initializes a new instance of the <see cref="IpForwDatagrams"/> class.
            /// </summary>
            public IpForwDatagrams()
                : base(".1.3.6.1.2.1.4.6.0")
            {
            }
            /// <summary>
            /// 
            /// </summary>
            public override ISnmpData Data
            {
                get
                {
                    return new Counter32(MibIITableHelpers.IpData.Statistics.dwForwDatagrams);
                }
                set
                {
                    throw new AccessFailureException();
                }
            }
        }
        internal sealed class IpUnknowProtos : ScalarObject
        {
            /// <summary>
            /// Initializes a new instance of the <see cref="IpUnknowProtos"/> class.
            /// </summary>
            public IpUnknowProtos()
                : base(".1.3.6.1.2.1.4.7.0")
            {
            }
            /// <summary>
            /// 
            /// </summary>
            public override ISnmpData Data
            {
                get
                {
                    return new Counter32(MibIITableHelpers.IpData.Statistics.dwInUnknownProtos);
                }
                set
                {
                    throw new AccessFailureException();
                }
            }
        }
        internal sealed class IpInDiscards : ScalarObject
        {
            /// <summary>
            /// Initializes a new instance of the <see cref="IpInDiscards"/> class.
            /// </summary>
            public IpInDiscards()
                : base(".1.3.6.1.2.1.4.8.0")
            {
            }
            /// <summary>
            /// 
            /// </summary>
            public override ISnmpData Data
            {
                get
                {
                    return new Counter32(MibIITableHelpers.IpData.Statistics.dwInDiscards);
                }
                set
                {
                    throw new AccessFailureException();
                }
            }
        }
        internal sealed class IpInDelivers : ScalarObject
        {
            /// <summary>
            /// Initializes a new instance of the <see cref="IpInDelivers"/> class.
            /// </summary>
            public IpInDelivers()
                : base(".1.3.6.1.2.1.4.9.0")
            {
            }
            /// <summary>
            /// 
            /// </summary>
            public override ISnmpData Data
            {
                get
                {
                    return new Counter32(MibIITableHelpers.IpData.Statistics.dwInDelivers);
                }
                set
                {
                    throw new AccessFailureException();
                }
            }
        }
        internal sealed class IpOutRequests : ScalarObject
        {
            /// <summary>
            /// Initializes a new instance of the <see cref="IpOutRequests"/> class.
            /// </summary>
            public IpOutRequests()
                : base(".1.3.6.1.2.1.4.10.0")
            {
            }
            /// <summary>
            /// 
            /// </summary>
            public override ISnmpData Data
            {
                get
                {
                    return new Counter32(MibIITableHelpers.IpData.Statistics.dwOutRequests);
                }
                set
                {
                    throw new AccessFailureException();
                }
            }
        }
        internal sealed class IpOutDiscards : ScalarObject
        {
            /// <summary>
            /// Initializes a new instance of the <see cref="IpOutDiscards"/> class.
            /// </summary>
            public IpOutDiscards()
                : base(".1.3.6.1.2.1.4.11.0")
            {
            }
            /// <summary>
            /// 
            /// </summary>
            public override ISnmpData Data
            {
                get
                {
                    return new Counter32(MibIITableHelpers.IpData.Statistics.dwOutDiscards);
                }
                set
                {
                    throw new AccessFailureException();
                }
            }
        }
        internal sealed class IpOutNoRoutes : ScalarObject
        {
            /// <summary>
            /// Initializes a new instance of the <see cref="IpOutNoRoutes"/> class.
            /// </summary>
            public IpOutNoRoutes()
                : base(".1.3.6.1.2.1.4.12.0")
            {
            }
            /// <summary>
            /// 
            /// </summary>
            public override ISnmpData Data
            {
                get
                {
                    return new Counter32(MibIITableHelpers.IpData.Statistics.dwOutNoRoutes);
                }
                set
                {
                    throw new AccessFailureException();
                }
            }
        }
        internal sealed class IpReasmTimeout : ScalarObject
        {
            /// <summary>
            /// Initializes a new instance of the <see cref="IpReasmTimeout"/> class.
            /// </summary>
            public IpReasmTimeout()
                : base(".1.3.6.1.2.1.4.13.0")
            {
            }
            /// <summary>
            /// 
            /// </summary>
            public override ISnmpData Data
            {
                get
                {
                    return new Integer32(MibIITableHelpers.IpData.Statistics.dwReasmTimeout);
                }
                set
                {
                    throw new AccessFailureException();
                }
            }
        }
        internal sealed class IpReasmReqds : ScalarObject
        {
            /// <summary>
            /// Initializes a new instance of the <see cref="IpReasmReqds"/> class.
            /// </summary>
            public IpReasmReqds()
                : base(".1.3.6.1.2.1.4.14.0")
            {
            }
            /// <summary>
            /// 
            /// </summary>
            public override ISnmpData Data
            {
                get
                {
                    var data = IPGlobalProperties.GetIPGlobalProperties().GetIPv4GlobalStatistics().PacketReassembliesRequired;
                    return new Counter32((int)data);
                }
                set
                {
                    throw new AccessFailureException();
                }
            }
        }
        internal sealed class IpReasmOKs : ScalarObject
        {
            /// <summary>
            /// Initializes a new instance of the <see cref="IpReasmOKs"/> class.
            /// </summary>
            public IpReasmOKs()
                : base(".1.3.6.1.2.1.4.15.0")
            {
            }
            /// <summary>
            /// 
            /// </summary>
            public override ISnmpData Data
            {
                get
                {
                    return new Counter32(MibIITableHelpers.IpData.Statistics.dwReasmOks);
                }
                set
                {
                    throw new AccessFailureException();
                }
            }
        }
        internal sealed class IpReasmFails : ScalarObject
        {
            /// <summary>
            /// Initializes a new instance of the <see cref="IpReasmFails"/> class.
            /// </summary>
            public IpReasmFails()
                : base(".1.3.6.1.2.1.4.16.0")
            {
            }
            /// <summary>
            /// 
            /// </summary>
            public override ISnmpData Data
            {
                get
                {
                    return new Counter32(MibIITableHelpers.IpData.Statistics.dwReasmFails);
                }
                set
                {
                    throw new AccessFailureException();
                }
            }
        }
        internal sealed class IpFragOKs : ScalarObject
        {
            /// <summary>
            /// Initializes a new instance of the <see cref="IpFragOKs"/> class.
            /// </summary>
            public IpFragOKs()
                : base(".1.3.6.1.2.1.4.17.0")
            {
            }
            /// <summary>
            /// 
            /// </summary>
            public override ISnmpData Data
            {
                get
                {
                    return new Counter32(MibIITableHelpers.IpData.Statistics.dwFragOks);
                }
                set
                {
                    throw new AccessFailureException();
                }
            }
        }
        internal sealed class IpFragFails : ScalarObject
        {
            /// <summary>
            /// Initializes a new instance of the <see cref="IpFragFails"/> class.
            /// </summary>
            public IpFragFails()
                : base(".1.3.6.1.2.1.4.18.0")
            {
            }
            /// <summary>
            /// 
            /// </summary>
            public override ISnmpData Data
            {
                get
                {
                    return new Counter32(MibIITableHelpers.IpData.Statistics.dwFragFails);
                }
                set
                {
                    throw new AccessFailureException();
                }
            }
        }
        internal sealed class IpFragCreates : ScalarObject
        {
            /// <summary>
            /// Initializes a new instance of the <see cref="IpFragCreates"/> class.
            /// </summary>
            public IpFragCreates()
                : base(".1.3.6.1.2.1.4.19.0")
            {
            }
            /// <summary>
            /// 
            /// </summary>
            public override ISnmpData Data
            {
                get
                {
                    return new Counter32(MibIITableHelpers.IpData.Statistics.dwFragCreates);
                }
                set
                {
                    throw new AccessFailureException();
                }
            }
        }

        internal sealed class IpAddrTable : TableObject
        {

            internal sealed class IpAdEntAddr : ScalarObject
            {
                private readonly ISnmpData _data;
                /// <summary>
                /// Initializes a new instance of the <see cref="IpAdEntIfIndex"/> class.
                public IpAdEntAddr(int index, object onlineData)
                    : base(".1.3.6.1.2.1.4.20.1.1.{0}", index)
                {
                    var entryData = (MibIITableHelpers.IpHlpApiHelper.MIB_IPADDRROW_W2K)onlineData;
                    _data = new IP(BitConverter.GetBytes(entryData.dwAddr));
                }
                public override ISnmpData Data
                {
                    get { return _data; }
                    set { throw new AccessFailureException(); }
                }
            }
            internal sealed class IpAdEntIfIndex : ScalarObject
            {
                private readonly ISnmpData _data;
                /// <summary>
                /// Initializes a new instance of the <see cref="IpAdEntIfIndex"/> class.
                public IpAdEntIfIndex(int index, object onlineData)
                    : base(".1.3.6.1.2.1.4.20.1.2.{0}", index)
                {
                    var entryData = (MibIITableHelpers.IpHlpApiHelper.MIB_IPADDRROW_W2K)onlineData;
                    _data = new Integer32((Int32)entryData.dwIndex);
                }
                public override ISnmpData Data
                {
                    get { return _data; }
                    set { throw new AccessFailureException(); }
                }
            }
            internal sealed class IpAdEntNetMask : ScalarObject
            {
                private readonly ISnmpData _data;
                /// <summary>
                /// Initializes a new instance of the <see cref="IpAdEntNetMask"/> class.
                public IpAdEntNetMask(int index, object onlineData)
                    : base(".1.3.6.1.2.1.4.20.1.3.{0}", index)
                {
                    var entryData = (MibIITableHelpers.IpHlpApiHelper.MIB_IPADDRROW_W2K)onlineData;
                    _data = new IP(BitConverter.GetBytes(entryData.dwMask));
                }
                public override ISnmpData Data
                {
                    get { return _data; }
                    set { throw new AccessFailureException(); }
                }
            }
            internal sealed class IpAdEntBcastAddr : ScalarObject
            {
                private readonly ISnmpData _data;
                /// <summary>
                /// Initializes a new instance of the <see cref="IpAdEntBcastAddr"/> class.
                public IpAdEntBcastAddr(int index, object onlineData)
                    : base(".1.3.6.1.2.1.4.20.1.4.{0}", index)
                {
                    var entryData = (MibIITableHelpers.IpHlpApiHelper.MIB_IPADDRROW_W2K)onlineData;
                    _data = new Integer32((Int32)entryData.dwBCastAddr);
                }
                public override ISnmpData Data
                {
                    get { return _data; }
                    set { throw new AccessFailureException(); }
                }
            }
            internal sealed class IpAdEntReasmMaxSize : ScalarObject
            {
                private readonly ISnmpData _data;
                /// <summary>
                /// Initializes a new instance of the <see cref="IpAdEntReasmMaxSize"/> class.
                public IpAdEntReasmMaxSize(int index, object onlineData)
                    : base(".1.3.6.1.2.1.4.20.1.5.{0}", index)
                {
                    var entryData = (MibIITableHelpers.IpHlpApiHelper.MIB_IPADDRROW_W2K)onlineData;
                    _data = new Integer32((Int32)entryData.dwReasmSize);
                }
                public override ISnmpData Data
                {
                    get { return _data; }
                    set { throw new AccessFailureException(); }
                }
            }

            /// <summary>
            /// Initializes a new instance of the <see cref="IpAddrTable"/> class.
            /// </summary>
            public IpAddrTable()
            {
                /** Ejemplo para programar el evento de actualizacion de la tabla*/
                NetworkChange.NetworkAddressChanged +=
                    (sender, args) => LoadElements();
#if NET452
            NetworkChange.NetworkAvailabilityChanged +=
                (sender, args) => LoadElements();
#endif                
                LoadElements();
            }

            // ".1.3.6.1.2.1.4.20"
            private readonly IList<ScalarObject> _elements = new List<ScalarObject>();
            private void LoadElements()
            {
                _elements.Clear();
                //var ipTable = MibIITableHelpers.IpAddrTable.Data;

                var ipTable = MibIITableHelpers.IpData.AddrTable;
                var columnTypes = new[]
    {
                    typeof(IpAdEntAddr),
                    typeof(IpAdEntIfIndex),
                    typeof(IpAdEntNetMask),
                    typeof(IpAdEntBcastAddr),
                    typeof(IpAdEntReasmMaxSize),
                };
                foreach (var type in columnTypes)
                {
                    for (int i = 0; i < ipTable.Count; i++)
                    {
                        _elements.Add((ScalarObject)Activator.CreateInstance(type, new object[] { i + 1, ipTable[i] }));
                    }
                }
            }
            protected override IEnumerable<ScalarObject> Objects
            {
                get { return _elements; }
            }
        }

        internal class IpRouteTable : TableObject
        {
            internal sealed class IpRouteDest : ScalarObject
            {
                private ISnmpData _data;
                /// <summary>
                /// Initializes a new instance of the <see cref="IpRouteIfIndex"/> class.
                public IpRouteDest(int index, object onlineData)
                    : base(".1.3.6.1.2.1.4.21.1.1.{0}", index)
                {
                    var entryData = (MibIITableHelpers.IpHlpApiHelper.MIB_IPFORWARDROW)onlineData;
                    _data = new IP(BitConverter.GetBytes(entryData.dwForwardDest));
                }
                public override ISnmpData Data
                {
                    get { return _data; }
                    set { throw new AccessFailureException(); }
                }
            }
            internal sealed class IpRouteIfIndex : ScalarObject
            {
                private readonly ISnmpData _data;
                /// <summary>
                /// Initializes a new instance of the <see cref="IpRouteIfIndex"/> class.
                public IpRouteIfIndex(int index, object onlineData)
                    : base(".1.3.6.1.2.1.4.21.1.2.{0}", index)
                {
                    var entryData = (MibIITableHelpers.IpHlpApiHelper.MIB_IPFORWARDROW)onlineData;
                    _data = new Integer32((Int32)entryData.dwForwardIfIndex);
                }
                public override ISnmpData Data
                {
                    get { return _data; }
                    set { throw new AccessFailureException(); }
                }
            }
            internal sealed class IpRouteMetric1 : ScalarObject
            {
                private ISnmpData _data;
                /// <summary>
                /// Initializes a new instance of the <see cref="IpRouteMetric1"/> class.
                public IpRouteMetric1(int index, object onlineData)
                    // ReSharper restore UnusedParameter.Local
                    : base(".1.3.6.1.2.1.4.21.1.3.{0}", index)
                {
                    var entryData = (MibIITableHelpers.IpHlpApiHelper.MIB_IPFORWARDROW)onlineData;
                    _data = new Integer32((Int32)entryData.dwForwardMetric1);
                }
                public override ISnmpData Data
                {
                    get { return _data; }
                    set { throw new AccessFailureException(); }
                }
            }
            internal sealed class IpRouteMetric2 : ScalarObject
            {
                private ISnmpData _data;
                /// <summary>
                /// Initializes a new instance of the <see cref="IpRouteMetric2"/> class.
                public IpRouteMetric2(int index, object onlineData)
                    : base(".1.3.6.1.2.1.4.21.1.4.{0}", index)
                {
                    var entryData = (MibIITableHelpers.IpHlpApiHelper.MIB_IPFORWARDROW)onlineData;
                    _data = new Integer32((Int32)entryData.dwForwardMetric2);
                }
                public override ISnmpData Data
                {
                    get { return _data; }
                    set { throw new AccessFailureException(); }
                }
            }
            internal sealed class IpRouteMetric3 : ScalarObject
            {
                private ISnmpData _data;
                /// <summary>
                /// Initializes a new instance of the <see cref="IpRouteMetric3"/> class.
                public IpRouteMetric3(int index, object onlineData)
                    : base(".1.3.6.1.2.1.4.21.1.5.{0}", index)
                {
                    var entryData = (MibIITableHelpers.IpHlpApiHelper.MIB_IPFORWARDROW)onlineData;
                    _data = new Integer32((Int32)entryData.dwForwardMetric3);
                }
                public override ISnmpData Data
                {
                    get { return _data; }
                    set { throw new AccessFailureException(); }
                }
            }
            internal sealed class IpRouteMetric4 : ScalarObject
            {
                private ISnmpData _data;
                /// <summary>
                /// Initializes a new instance of the <see cref="IpRouteMetric4"/> class.
                public IpRouteMetric4(int index, object onlineData)
                    : base(".1.3.6.1.2.1.4.21.1.6.{0}", index)
                {
                    var entryData = (MibIITableHelpers.IpHlpApiHelper.MIB_IPFORWARDROW)onlineData;
                    _data = new Integer32((Int32)entryData.dwForwardMetric4);
                }
                public override ISnmpData Data
                {
                    get { return _data; }
                    set { throw new AccessFailureException(); }
                }
            }
            internal sealed class IpRouteNextHop : ScalarObject
            {
                private ISnmpData _data;
                /// <summary>
                /// Initializes a new instance of the <see cref="IpRouteNextHop"/> class.
                public IpRouteNextHop(int index, object onlineData)
                    : base(".1.3.6.1.2.1.4.21.1.7.{0}", index)
                {
                    var entryData = (MibIITableHelpers.IpHlpApiHelper.MIB_IPFORWARDROW)onlineData;
                    _data = new IP(BitConverter.GetBytes(entryData.dwForwardNextHop));
                }
                public override ISnmpData Data
                {
                    get { return _data; }
                    set { throw new AccessFailureException(); }
                }
            }
            internal sealed class IpRouteType : ScalarObject
            {
                private ISnmpData _data;
                /// <summary>
                /// Initializes a new instance of the <see cref="IpRouteType"/> class.
                public IpRouteType(int index, object onlineData)
                    : base(".1.3.6.1.2.1.4.21.1.8.{0}", index)
                {
                    var entryData = (MibIITableHelpers.IpHlpApiHelper.MIB_IPFORWARDROW)onlineData;
                    _data = new Integer32((Int32)entryData.ForwardType);
                }
                public override ISnmpData Data
                {
                    get { return _data; }
                    set { throw new AccessFailureException(); }
                }
            }
            internal sealed class IpRouteProto : ScalarObject
            {
                private readonly ISnmpData _data;
                /// <summary>
                /// Initializes a new instance of the <see cref="IpRouteProto"/> class.
                public IpRouteProto(int index, object onlineData)
                    : base(".1.3.6.1.2.1.4.21.1.9.{0}", index)
                {
                    var entryData = (MibIITableHelpers.IpHlpApiHelper.MIB_IPFORWARDROW)onlineData;
                    _data = new Integer32((Int32)entryData.dwForwardProto);
                }
                public override ISnmpData Data
                {
                    get { return _data; }
                    set { throw new AccessFailureException(); }
                }
            }
            internal sealed class IpRouteAge : ScalarObject
            {
                private ISnmpData _data;
                /// <summary>
                /// Initializes a new instance of the <see cref="IpRouteAge"/> class.
                public IpRouteAge(int index, object onlineData)
                    : base(".1.3.6.1.2.1.4.21.1.10.{0}", index)
                {
                    var entryData = (MibIITableHelpers.IpHlpApiHelper.MIB_IPFORWARDROW)onlineData;
                    _data = new Integer32((Int32)entryData.dwForwardAge);
                }
                public override ISnmpData Data
                {
                    get { return _data; }
                    set { throw new AccessFailureException(); }
                }
            }
            internal sealed class IpRouteMask : ScalarObject
            {
                private ISnmpData _data;
                /// <summary>
                /// Initializes a new instance of the <see cref="IpRouteMask"/> class.
                public IpRouteMask(int index, object onlineData)
                    // ReSharper restore UnusedParameter.Local
                    : base(".1.3.6.1.2.1.4.21.1.11.{0}", index)
                {
                    var entryData = (MibIITableHelpers.IpHlpApiHelper.MIB_IPFORWARDROW)onlineData;
                    _data = new IP(BitConverter.GetBytes(entryData.dwForwardMask));
                }
                public override ISnmpData Data
                {
                    get { return _data; }
                    set { throw new AccessFailureException(); }
                }
            }
            internal sealed class IpRouteMetric5 : ScalarObject
            {
                private ISnmpData _data;
                /// <summary>
                /// Initializes a new instance of the <see cref="IpRouteMetric5"/> class.
                public IpRouteMetric5(int index, object onlineData)
                    : base(".1.3.6.1.2.1.4.21.1.12.{0}", index)
                {
                    var entryData = (MibIITableHelpers.IpHlpApiHelper.MIB_IPFORWARDROW)onlineData;
                    _data = new Integer32((Int32)entryData.dwForwardMetric5);
                }
                public override ISnmpData Data
                {
                    get { return _data; }
                    set { throw new AccessFailureException(); }
                }
            }
            internal sealed class IpRouteInfo : ScalarObject
            {
                private readonly ISnmpData _data;
                /// <summary>
                /// Initializes a new instance of the <see cref="IpRouteInfo"/> class.
                public IpRouteInfo(int index, object onlineData)
                    : base(".1.3.6.1.2.1.4.21.1.13.{0}", index)
                {
                    _data = new ObjectIdentifier("0.0");
                }
                public override ISnmpData Data
                {
                    get { return _data; }
                    set { throw new AccessFailureException(); }
                }
            }

            /// <summary>
            /// Initializes a new instance of the <see cref="IpRouteTable"/> class.
            /// </summary>
            public IpRouteTable()
            {
                /** Ejemplo para programar el evento de actualizacion de la tabla */
                NetworkChange.NetworkAddressChanged +=
                    (sender, args) => LoadElements();
#if NET452
            NetworkChange.NetworkAvailabilityChanged +=
                (sender, args) => LoadElements();
#endif                
                LoadElements();
            }

            // ".1.3.6.1.2.1.4.21"
            private readonly IList<ScalarObject> _elements = new List<ScalarObject>();
            private void LoadElements()
            {
                _elements.Clear();

                //var routeTable = MibIITableHelpers.IpRouteTable.Data;
                var routeTable = MibIITableHelpers.IpData.RouteTable;

                var columnTypes = new[]
                {
                    typeof(IpRouteDest),
                    typeof(IpRouteIfIndex),
                    typeof(IpRouteMetric1),
                    typeof(IpRouteMetric2),
                    typeof(IpRouteMetric3),
                    typeof(IpRouteMetric4),
                    typeof(IpRouteNextHop),
                    typeof(IpRouteType),
                    typeof(IpRouteProto),
                    typeof(IpRouteAge),
                    typeof(IpRouteMask),
                    typeof(IpRouteMetric5),
                    typeof(IpRouteInfo),
                };
                foreach (var type in columnTypes)
                {
                    for (int i = 0; i < routeTable.Count; i++)
                    {
                        _elements.Add((ScalarObject)Activator.CreateInstance(type, new object[] { i + 1, routeTable[i] }));
                    }
                }
            }
            protected override IEnumerable<ScalarObject> Objects
            {
                get { return _elements; }
            }
        }

        internal sealed class IpNetToMediaTable : TableObject
        {
            internal sealed class IpNetToMediaIfIndex : ScalarObject
            {
                private readonly ISnmpData _data;
                /// <summary>
                /// Initializes a new instance of the <see cref="IpNetToMediaIfIndex"/> class.
                public IpNetToMediaIfIndex(int index, object onlineData)
                    : base(".1.3.6.1.2.1.4.22.1.1.{0}", index)
                {
                    var entryData = (MibIITableHelpers.IpHlpApiHelper.MIB_IPNETROW)onlineData;
                    _data = new Integer32(entryData.dwIndex);
                }
                public override ISnmpData Data
                {
                    get { return _data; }
                    set { throw new AccessFailureException(); }
                }
            }
            internal sealed class IpNetToMediaPhysAddress : ScalarObject
            {
                private ISnmpData _data;
                /// <summary>
                /// Initializes a new instance of the <see cref="IpNetToMediaPhysAddress"/> class.
                public IpNetToMediaPhysAddress(int index, object onlineData)
                    : base(".1.3.6.1.2.1.4.22.1.2.{0}", index)
                {
                    var row = (MibIITableHelpers.IpHlpApiHelper.MIB_IPNETROW)onlineData;
                    var PhysAddress = row.mac0.ToString("X2") + '-' +
                        row.mac1.ToString("X2") + '-' +
                        row.mac2.ToString("X2") + '-' +
                        row.mac3.ToString("X2") + '-' +
                        row.mac4.ToString("X2") + '-' +
                        row.mac5.ToString("X2") + '-';
                    _data = new OctetString(PhysAddress);
                }
                public override ISnmpData Data
                {
                    get { return _data; }
                    set { throw new AccessFailureException(); }
                }
            }
            internal sealed class IpNetToMediaNetAddress : ScalarObject
            {
                private ISnmpData _data;
                /// <summary>
                /// Initializes a new instance of the <see cref="IpNetToMediaNetAddress"/> class.
                public IpNetToMediaNetAddress(int index, object onlineData)
                    : base(".1.3.6.1.2.1.4.22.1.3.{0}", index)
                {
                    var row = (MibIITableHelpers.IpHlpApiHelper.MIB_IPNETROW)onlineData;
                    _data = new IP(BitConverter.GetBytes(row.dwAddr));
                }
                public override ISnmpData Data
                {
                    get { return _data; }
                    set { throw new AccessFailureException(); }
                }
            }
            internal sealed class IpNetToMedidaType : ScalarObject
            {
                private ISnmpData _data;
                /// <summary>
                /// Initializes a new instance of the <see cref="IpNetToMedidaType"/> class.
                public IpNetToMedidaType(int index, object onlineData)
                    : base(".1.3.6.1.2.1.4.22.1.4.{0}", index)
                {
                    var row = (MibIITableHelpers.IpHlpApiHelper.MIB_IPNETROW)onlineData;
                    _data = new Integer32(row.dwType);
                }
                public override ISnmpData Data
                {
                    get { return _data; }
                    set { throw new AccessFailureException(); }
                }
            }

            /// <summary>
            /// Initializes a new instance of the <see cref="IpNetToMediaTable"/> class.
            /// </summary>
            public IpNetToMediaTable()
            {
                /** Ejemplo para programar el evento de actualizacion de la tabla */
                NetworkChange.NetworkAddressChanged +=
                    (sender, args) => LoadElements();
#if NET452
            NetworkChange.NetworkAvailabilityChanged +=
                (sender, args) => LoadElements();
#endif                
                LoadElements();
            }

            // ".1.3.6.1.2.1.4.22"
            private readonly IList<ScalarObject> _elements = new List<ScalarObject>();
            private void LoadElements()
            {
                _elements.Clear();

                //var netToMediaTable = MibIITableHelpers.IpNetTable.Data;
                var netToMediaTable = MibIITableHelpers.IpData.NetTable;
                var columnTypes = new[]
                {
                    typeof(IpNetToMediaIfIndex),
                    typeof(IpNetToMediaPhysAddress),
                    typeof(IpNetToMediaNetAddress),
                    typeof(IpNetToMedidaType),
                };
                foreach (var type in columnTypes)
                {
                    for (int i = 0; i < netToMediaTable.Count; i++)
                    {
                        _elements.Add((ScalarObject)Activator.CreateInstance(type, new object[] { i + 1, netToMediaTable[i] }));
                    }
                }
            }
            protected override IEnumerable<ScalarObject> Objects
            {
                get { return _elements; }
            }
        }

        internal sealed class IpRoutingDiscards : ScalarObject
        {
            /// <summary>
            /// Initializes a new instance of the <see cref="IpRoutingDiscards"/> class.
            /// </summary>
            public IpRoutingDiscards()
                : base(".1.3.6.1.2.1.4.23.0")
            {
            }
            /// <summary>
            /// 
            /// </summary>
            public override ISnmpData Data
            {
                get
                {
                    return new Counter32(MibIITableHelpers.IpData.Statistics.dwRoutingDiscards);
                }
                set
                {
                    throw new AccessFailureException();
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        protected override void LoadElements()
        {
            snmpObjects = new List<SnmpObjectBase>()
            {
                new IpForwarding(),
                new IpDefaultTTL(),
                new IpInReceives(),
                new IpHdrErrors(),
                new IpInAddrErrors(),
                new IpForwDatagrams(),
                new IpUnknowProtos(),
                new IpInDiscards(),
                new IpInDelivers(),
                new IpOutRequests(),
                new IpOutDiscards(),
                new IpOutNoRoutes(),
                new IpReasmTimeout(),
                new IpReasmReqds(),
                new IpReasmOKs(),
                new IpReasmFails(),
                new IpFragOKs(),
                new IpFragFails(),
                new IpFragCreates(),
                new IpAddrTable(),
                new IpRouteTable(),
                new IpNetToMediaTable(),

                new IpRoutingDiscards()
            };
        }
    }
    /// <summary>
    /// 
    /// </summary>
    class U5KMIBIITcpObjectGroup : U5kMibGroup
    {
        public U5KMIBIITcpObjectGroup() : base() { }
        protected override void LoadElements()
        {
            snmpObjects = new List<SnmpObjectBase>()
            {
                new TcpRtoAlgoritm(),
                new TcpRtoMin(),
                new TcpRtoMax(),
                new TcpMaxConn(),
                new TcpActiveOpens(),
                new TcpPassiveOpens(),
                new TcpAttemptFails(),
                new TcpEstabResets(),
                new TcpCurrEstab(),
                new TcpInSegs(),
                new TcpOutSegs(),
                new TcpRetransSegs(),
                new TcpConnTable(),
                new TcpInErrs(),
                new TcpOutErrs(),
            };
        }
        /** Elementos del Grupo */
        internal sealed class TcpRtoAlgoritm : ScalarObject
        {
            /// <summary>
            /// Initializes a new instance of the <see cref="TcpRtoAlgoritm"/> class.
            /// </summary>
            public TcpRtoAlgoritm()
                : base(".1.3.6.1.2.1.6.1.0")
            {
            }
            /// <summary>
            /// 
            /// </summary>
            public override ISnmpData Data
            {
                get
                {
                    var tcpstats = MibIITableHelpers.TcpData.Statistics;
                    return new Integer32(tcpstats.dwRtoAlgorithm);
                }
                set { throw new AccessFailureException(); }
            }
        }
        internal sealed class TcpRtoMin : ScalarObject
        {
            /// <summary>
            /// Initializes a new instance of the <see cref="TcpRtoMin"/> class.
            /// </summary>
            public TcpRtoMin()
                : base(".1.3.6.1.2.1.6.2.0")
            {
            }
            /// <summary>
            /// 
            /// </summary>
            public override ISnmpData Data
            {
                get
                {
                    var tcpstats = MibIITableHelpers.TcpData.Statistics;
                    return new Integer32(tcpstats.dwRtoMin);
                }
                set
                {
                    throw new AccessFailureException();
                }
            }
        }
        internal sealed class TcpRtoMax : ScalarObject
        {
            /// <summary>
            /// Initializes a new instance of the <see cref="TcpRtoMax"/> class.
            /// </summary>
            public TcpRtoMax()
                : base(".1.3.6.1.2.1.6.3.0")
            {
            }
            /// <summary>
            /// 
            /// </summary>
            public override ISnmpData Data
            {
                get
                {
                    var tcpstats = MibIITableHelpers.TcpData.Statistics;
                    return new Integer32(tcpstats.dwRtoMax);
                }
                set
                {
                    throw new AccessFailureException();
                }
            }
        }
        internal sealed class TcpMaxConn : ScalarObject
        {
            /// <summary>
            /// Initializes a new instance of the <see cref="TcpMaxConn"/> class.
            /// </summary>
            public TcpMaxConn()
                : base(".1.3.6.1.2.1.6.4.0")
            {
            }
            /// <summary>
            /// 
            /// </summary>
            public override ISnmpData Data
            {
                get
                {
                    var tcpstats = MibIITableHelpers.TcpData.Statistics;
                    return new Integer32(tcpstats.dwMaxConn);
                }
                set
                {
                    throw new AccessFailureException();
                }
            }
        }
        internal sealed class TcpActiveOpens : ScalarObject
        {
            /// <summary>
            /// Initializes a new instance of the <see cref="TcpActiveOpens"/> class.
            /// </summary>
            public TcpActiveOpens()
                : base(".1.3.6.1.2.1.6.5.0")
            {
            }
            /// <summary>
            /// 
            /// </summary>
            public override ISnmpData Data
            {
                get
                {
                    var tcpstats = MibIITableHelpers.TcpData.Statistics;
                    return new Counter32(tcpstats.dwActiveOpens);
                }
                set
                {
                    throw new AccessFailureException();
                }
            }
        }
        internal sealed class TcpPassiveOpens : ScalarObject
        {
            /// <summary>
            /// Initializes a new instance of the <see cref="TcpPassiveOpens"/> class.
            /// </summary>
            public TcpPassiveOpens()
                : base(".1.3.6.1.2.1.6.6.0")
            {
            }
            /// <summary>
            /// 
            /// </summary>
            public override ISnmpData Data
            {
                get
                {
                    var tcpstats = MibIITableHelpers.TcpData.Statistics;
                    return new Counter32(tcpstats.dwPassiveOpens);
                }
                set
                {
                    throw new AccessFailureException();
                }
            }
        }
        internal sealed class TcpAttemptFails : ScalarObject
        {
            /// <summary>
            /// Initializes a new instance of the <see cref="TcpAttemptFails"/> class.
            /// </summary>
            public TcpAttemptFails()
                : base(".1.3.6.1.2.1.6.7.0")
            {
            }
            /// <summary>
            /// 
            /// </summary>
            public override ISnmpData Data
            {
                get
                {
                    var tcpstats = MibIITableHelpers.TcpData.Statistics;
                    return new Counter32(tcpstats.dwAttemptFails);
                }
                set
                {
                    throw new AccessFailureException();
                }
            }
        }
        internal sealed class TcpEstabResets : ScalarObject
        {
            /// <summary>
            /// Initializes a new instance of the <see cref="TcpEstabResets"/> class.
            /// </summary>
            public TcpEstabResets()
                : base(".1.3.6.1.2.1.6.8.0")
            {
            }
            /// <summary>
            /// 
            /// </summary>
            public override ISnmpData Data
            {
                get
                {
                    var tcpstats = MibIITableHelpers.TcpData.Statistics;
                    return new Counter32(tcpstats.dwEstabResets);
                }
                set
                {
                    throw new AccessFailureException();
                }
            }
        }
        internal sealed class TcpCurrEstab : ScalarObject
        {
            /// <summary>
            /// Initializes a new instance of the <see cref="TcpCurrEstab"/> class.
            /// </summary>
            public TcpCurrEstab()
                : base(".1.3.6.1.2.1.6.9.0")
            {
            }
            /// <summary>
            /// 
            /// </summary>
            public override ISnmpData Data
            {
                get
                {
                    var tcpstats = MibIITableHelpers.TcpData.Statistics;
                    return new Gauge32(tcpstats.dwCurrEstab);
                }
                set
                {
                    throw new AccessFailureException();
                }
            }
        }
        internal sealed class TcpInSegs : ScalarObject
        {
            /// <summary>
            /// Initializes a new instance of the <see cref="TcpInSegs"/> class.
            /// </summary>
            public TcpInSegs()
                : base(".1.3.6.1.2.1.6.10.0")
            {
            }
            /// <summary>
            /// 
            /// </summary>
            public override ISnmpData Data
            {
                get
                {
                    var tcpstats = MibIITableHelpers.TcpData.Statistics;
                    return new Counter32(tcpstats.dwInSegs);
                }
                set
                {
                    throw new AccessFailureException();
                }
            }
        }
        internal sealed class TcpOutSegs : ScalarObject
        {
            /// <summary>
            /// Initializes a new instance of the <see cref="TcpOutSegs"/> class.
            /// </summary>
            public TcpOutSegs()
                : base(".1.3.6.1.2.1.6.11.0")
            {
            }
            /// <summary>
            /// 
            /// </summary>
            public override ISnmpData Data
            {
                get
                {
                    var tcpstats = MibIITableHelpers.TcpData.Statistics;
                    return new Counter32(tcpstats.dwOutSegs);
                }
                set
                {
                    throw new AccessFailureException();
                }
            }
        }
        internal sealed class TcpRetransSegs : ScalarObject
        {
            /// <summary>
            /// Initializes a new instance of the <see cref="TcpRetransSegs"/> class.
            /// </summary>
            public TcpRetransSegs()
                : base(".1.3.6.1.2.1.6.12.0")
            {
            }
            /// <summary>
            /// 
            /// </summary>
            public override ISnmpData Data
            {
                get
                {
                    var tcpstats = MibIITableHelpers.TcpData.Statistics;
                    return new Counter32(tcpstats.dwRetransSegs);
                }
                set
                {
                    throw new AccessFailureException();
                }
            }
        }

        internal sealed class TcpConnTable : TableObject
        {
            internal sealed class TcpConnState : ScalarObject
            {
                private ISnmpData _data;
                /// <summary>
                /// Initializes a new instance of the <see cref="TcpConnLocalAddress"/> class.
                public TcpConnState(int index, object onlineData)
                    : base(".1.3.6.1.2.1.6.13.1.1.{0}", index)
                {
                    var row = (MibIITableHelpers.IpHlpApiHelper.MIB_TCPROW)onlineData;
                    _data = new Integer32(row.dwState);
                }
                public override ISnmpData Data
                {
                    get { return _data; }
                    set
                    {
                        if (value != null && value.TypeCode == SnmpType.Integer32)
                            _data = value;
                        else
                            throw new ArgumentException("Invalid data type.", nameof(value));
                    }
                }
            }
            internal sealed class TcpConnLocalAddress : ScalarObject
            {
                private readonly ISnmpData _data;
                /// <summary>
                /// Initializes a new instance of the <see cref="TcpConnLocalAddress"/> class.
                public TcpConnLocalAddress(int index, object onlineData)
                    : base(".1.3.6.1.2.1.6.13.1.2.{0}", index)
                {
                    var row = (MibIITableHelpers.IpHlpApiHelper.MIB_TCPROW)onlineData;
                    _data = new IP(BitConverter.GetBytes(row.dwLocalAddr));
                }
                public override ISnmpData Data
                {
                    get { return _data; }
                    set { throw new AccessFailureException(); }
                }
            }
            internal sealed class TcpConnLocalPort : ScalarObject
            {
                private ISnmpData _data;
                /// <summary>
                /// Initializes a new instance of the <see cref="TcpConnLocalPort"/> class.
                public TcpConnLocalPort(int index, object onlineData)
                    : base(".1.3.6.1.2.1.6.13.1.3.{0}", index)
                {
                    var row = (MibIITableHelpers.IpHlpApiHelper.MIB_TCPROW)onlineData;
                    _data = new Integer32(IPAddress.NetworkToHostOrder((Int16)row.dwLocalPort) & 0xFFFF);
                }
                public override ISnmpData Data
                {
                    get { return _data; }
                    set { throw new AccessFailureException(); }
                }
            }
            internal sealed class TcpConnRemAddress : ScalarObject
            {
                private ISnmpData _data;
                /// <summary>
                /// Initializes a new instance of the <see cref="TcpConnRemAddress"/> class.
                public TcpConnRemAddress(int index, object onlineData)
                    : base(".1.3.6.1.2.1.6.13.1.4.{0}", index)
                {
                    var row = (MibIITableHelpers.IpHlpApiHelper.MIB_TCPROW)onlineData;
                    _data = new IP(BitConverter.GetBytes(row.dwRemoteAddr));
                }
                public override ISnmpData Data
                {
                    get { return _data; }
                    set { throw new AccessFailureException(); }
                }
            }
            internal sealed class TcpConnRemPort : ScalarObject
            {
                private ISnmpData _data;
                /// <summary>
                /// Initializes a new instance of the <see cref="TcpConnRemPort"/> class.
                public TcpConnRemPort(int index, object onlineData)
                    : base(".1.3.6.1.2.1.6.13.1.5.{0}", index)
                {
                    var row = (MibIITableHelpers.IpHlpApiHelper.MIB_TCPROW)onlineData;
                    _data = new Integer32(IPAddress.NetworkToHostOrder((Int16)row.dwRemotePort) & 0xFFFF);
                }
                public override ISnmpData Data
                {
                    get { return _data; }
                    set { throw new AccessFailureException(); }
                }
            }

            /// <summary>
            /// Initializes a new instance of the <see cref="TcpConnTable"/> class.
            /// </summary>
            public TcpConnTable()
            {
                /** Ejemplo para programar el evento de actualizacion de la tabla */
                NetworkChange.NetworkAddressChanged +=
                    (sender, args) => LoadElements();
#if NET452
            NetworkChange.NetworkAvailabilityChanged +=
                (sender, args) => LoadElements();
#endif
                LoadElements();
            }

            // ".1.3.6.1.2.1.6.13"
            private readonly IList<ScalarObject> _elements = new List<ScalarObject>();
            private void LoadElements()
            {
                _elements.Clear();
                //var tcpConnTable = new List<int>() { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14 }; // TODO. Tabla de rutas de la maquina.
                var tcpConnTable = MibIITableHelpers.TcpData.TableData;
                var columnTypes = new[]
                {
                    typeof(TcpConnState),
                    typeof(TcpConnLocalAddress),
                    typeof(TcpConnLocalPort),
                    typeof(TcpConnRemAddress),
                    typeof(TcpConnRemPort),
                };
                foreach (var type in columnTypes)
                {
                    for (int i = 0; i < tcpConnTable.Count; i++)
                    {
                        _elements.Add((ScalarObject)Activator.CreateInstance(type, new object[] { i + 1, tcpConnTable[i] }));
                    }
                }
            }
            protected override IEnumerable<ScalarObject> Objects
            {
                get { return _elements; }
            }
        }

        internal sealed class TcpInErrs : ScalarObject
        {
            /// <summary>
            /// Initializes a new instance of the <see cref="TcpInErrs"/> class.
            /// </summary>
            public TcpInErrs()
                : base(".1.3.6.1.2.1.6.14.0")
            {
            }
            /// <summary>
            /// 
            /// </summary>
            public override ISnmpData Data
            {
                get
                {
                    var tcpstats = MibIITableHelpers.TcpData.Statistics;
                    return new Counter32(tcpstats.dwInErrs);
                }
                set
                {
                    throw new AccessFailureException();
                }
            }
        }
        internal sealed class TcpOutErrs : ScalarObject
        {
            /// <summary>
            /// Initializes a new instance of the <see cref="TcpOutErrs"/> class.
            /// </summary>
            public TcpOutErrs()
                : base(".1.3.6.1.2.1.6.15.0")
            {
            }
            /// <summary>
            /// 
            /// </summary>
            public override ISnmpData Data
            {
                get
                {
                    var tcpstats = MibIITableHelpers.TcpData.Statistics;
                    return new Counter32(tcpstats.dwOutRsts);
                }
                set
                {
                    throw new AccessFailureException();
                }
            }
        }

    }
    /// <summary>
    /// 
    /// </summary>
    class U5KMIBIIUdpObjectGroup : U5kMibGroup
    {
        public U5KMIBIIUdpObjectGroup() : base() { }
        protected override void LoadElements()
        {
            snmpObjects = new List<SnmpObjectBase>()
            {
                new UdpInDatagrams(),
                new UdpNoPorts(),
                new UdpInErros(),
                new UdpOutDatagrams(),
                new UdpTable(),
            };
        }
        /** Elementos del Grupo */
        internal sealed class UdpInDatagrams : ScalarObject
        {
            /// <summary>
            /// Initializes a new instance of the <see cref="UdpInDatagrams"/> class.
            /// </summary>
            public UdpInDatagrams()
                : base(".1.3.6.1.2.1.7.1.0")
            {
            }
            /// <summary>
            /// 
            /// </summary>
            public override ISnmpData Data
            {
                get
                {
                    return new Counter32(MibIITableHelpers.UdpData.Statistics.dwInDatagrams);
                }
                set
                {
                    throw new AccessFailureException();
                }
            }
        }
        internal sealed class UdpNoPorts : ScalarObject
        {
            /// <summary>
            /// Initializes a new instance of the <see cref="UdpNoPorts"/> class.
            /// </summary>
            public UdpNoPorts()
                : base(".1.3.6.1.2.1.7.2.0")
            {
            }
            /// <summary>
            /// 
            /// </summary>
            public override ISnmpData Data
            {
                get
                {
                    return new Counter32(MibIITableHelpers.UdpData.Statistics.dwNoPorts);
                }
                set
                {
                    throw new AccessFailureException();
                }
            }
        }
        internal sealed class UdpInErros : ScalarObject
        {
            /// <summary>
            /// Initializes a new instance of the <see cref="UdpInErros"/> class.
            /// </summary>
            public UdpInErros()
                : base(".1.3.6.1.2.1.7.3.0")
            {
            }
            /// <summary>
            /// 
            /// </summary>
            public override ISnmpData Data
            {
                get
                {
                    return new Counter32(MibIITableHelpers.UdpData.Statistics.dwInErrors);
                }
                set
                {
                    throw new AccessFailureException();
                }
            }
        }
        internal sealed class UdpOutDatagrams : ScalarObject
        {
            /// <summary>
            /// Initializes a new instance of the <see cref="UdpOutDatagrams"/> class.
            /// </summary>
            public UdpOutDatagrams()
                : base(".1.3.6.1.2.1.7.4.0")
            {
            }
            /// <summary>
            /// 
            /// </summary>
            public override ISnmpData Data
            {
                get
                {
                    return new Counter32(MibIITableHelpers.UdpData.Statistics.dwOutDatagrams);
                }
                set
                {
                    throw new AccessFailureException();
                }
            }
        }

        internal sealed class UdpTable : TableObject
        {
            internal sealed class UdpLocalAddress : ScalarObject
            {
                readonly ISnmpData _data;
                /// <summary>
                /// Initializes a new instance of the <see cref="UdpLocalAddress"/> class.
                public UdpLocalAddress(int index, object onlineData)
                    // ReSharper restore UnusedParameter.Local
                    : base(".1.3.6.1.2.1.7.5.1.1.{0}", index)
                {
                    var row = (MibIITableHelpers.IpHlpApiHelper.MIB_UDPROW)onlineData;
                    _data = new IP(BitConverter.GetBytes(row.dwLocalAddr));
                }
                public override ISnmpData Data
                {
                    get { return _data; }
                    set { throw new AccessFailureException(); }
                }
            }
            internal sealed class UdpLocalPort : ScalarObject
            {
                private readonly ISnmpData _data;
                /// <summary>
                /// Initializes a new instance of the <see cref="UdpLocalPort"/> class.
                public UdpLocalPort(int index, object onlineData)
                    // ReSharper restore UnusedParameter.Local
                    : base(".1.3.6.1.2.1.7.5.1.2.{0}", index)
                {
                    var row = (MibIITableHelpers.IpHlpApiHelper.MIB_UDPROW)onlineData;
                    //var datain16 = (Int16)row.dwLocalPort;
                    //var data32 = IPAddress.NetworkToHostOrder(datain16);
                    //var data16 = (Int16)data32;
                    _data = new Integer32(IPAddress.NetworkToHostOrder((Int16)row.dwLocalPort) & 0xFFFF);
                }
                public override ISnmpData Data
                {
                    get { return _data; }
                    set { throw new AccessFailureException(); }
                }
            }

            /// <summary>
            /// Initializes a new instance of the <see cref="UdpTable"/> class.
            /// </summary>
            public UdpTable()
            {
                /** Ejemplo para programar el evento de actualizacion de la tabla */
                NetworkChange.NetworkAddressChanged +=
                    (sender, args) => LoadElements();
#if NET452
            NetworkChange.NetworkAvailabilityChanged +=
                (sender, args) => LoadElements();
#endif                
                LoadElements();
            }

            // ".1.3.6.1.2.1.7.5"
            private readonly IList<ScalarObject> _elements = new List<ScalarObject>();
            private void LoadElements()
            {
                _elements.Clear();
                var udpConnTable = MibIITableHelpers.UdpData.TableData;
                var columnTypes = new[]
                {
                    typeof(UdpLocalAddress),
                    typeof(UdpLocalPort),
                };
                foreach (var type in columnTypes)
                {
                    for (int i = 0; i < udpConnTable.Count; i++)
                    {
                        _elements.Add((ScalarObject)Activator.CreateInstance(type, new object[] { i + 1, udpConnTable[i] }));
                    }
                }
            }
            protected override IEnumerable<ScalarObject> Objects
            {
                get { return _elements; }
            }
        }
    }
    /// <summary>
    /// 
    /// </summary>
    class U5KMIBIISnmpObjectGroup : U5kMibGroup
    {
        protected override void LoadElements()
        {
            snmpObjects = new List<SnmpObjectBase>()
            {
                new SnmpInPkts(),
                new SnmpOutPkts(),
                new SnmpInBadVersions(),
                new SnmpInBadCommunityNames(),
                new SnmpInBadCommunityUses(),
                new SnmpInASNParseErrs(),
                new SnmpInTooBigs(),
                new SnmpInNoSuchNames(),
                new SnmpInBadValues(),
                new SnmpInReadOnlys(),
                new SnmpInGenErrs(),
                new SnmpInTotalReqVars(),
                new SnmpInTotalSetVars(),
                new SnmpInGetRequests(),
                new SnmpInGetNexts(),
                new SnmpInSetRequests(),
                new SnmpInGetResponses(),
                new SnmpInTraps(),
                new SnmpOutTooBigs(),
                new SnmpOutNotSuchNames(),
                new SnmpOutBadValues(),
                new SnmpOutGenErrs(),
                new SnmpOutGetRequests(),
                new SnmpOutGetNexts(),
                new SnmpOutSetRequests(),
                new SnmpOutGetResponses(),
                new SnmpOutTraps(),
                new SnmpEnableAuthenTraps(),
            };
        }
        /** Elementos del Grupo */
        internal sealed class SnmpInPkts : ScalarObject
        {
            /// <summary>
            /// Initializes a new instance of the <see cref="SnmpInPkts"/> class.
            /// </summary>
            public SnmpInPkts()
                : base(".1.3.6.1.2.1.11.1.0")
            {
            }
            /// <summary>
            /// 
            /// </summary>
            public override ISnmpData Data
            {
                get
                {
                    dynamic stats = SnmpStatistics;
                    return new Counter32(stats.SnmpInPkts);
                }
                set
                {
                    throw new AccessFailureException();
                }
            }
        }
        internal sealed class SnmpOutPkts : ScalarObject
        {
            /// <summary>
            /// Initializes a new instance of the <see cref="SnmpOutPkts"/> class.
            /// </summary>
            public SnmpOutPkts()
                : base(".1.3.6.1.2.1.11.2.0")
            {
            }
            /// <summary>
            /// 
            /// </summary>
            public override ISnmpData Data
            {
                get
                {
                    dynamic stats = SnmpStatistics;
                    return new Counter32(stats.SnmpOutPkts);
                }
                set
                {
                    throw new AccessFailureException();
                }
            }
        }
        internal sealed class SnmpInBadVersions : ScalarObject
        {
            /// <summary>
            /// Initializes a new instance of the <see cref="SnmpInBadVersions"/> class.
            /// </summary>
            public SnmpInBadVersions()
                : base(".1.3.6.1.2.1.11.3.0")
            {
            }
            /// <summary>
            /// 
            /// </summary>
            public override ISnmpData Data
            {
                get
                {
                    dynamic stats = SnmpStatistics;
                    return new Counter32(stats.SnmpInBadVersions);
                }
                set
                {
                    throw new AccessFailureException();
                }
            }
        }
        internal sealed class SnmpInBadCommunityNames : ScalarObject
        {
            /// <summary>
            /// Initializes a new instance of the <see cref="SnmpInBadCommunityNames"/> class.
            /// </summary>
            public SnmpInBadCommunityNames()
                : base(".1.3.6.1.2.1.11.4.0")
            {
            }
            /// <summary>
            /// 
            /// </summary>
            public override ISnmpData Data
            {
                get
                {
                    dynamic stats = SnmpStatistics;
                    return new Counter32(stats.SnmpInBadCommunityNames);
                }
                set
                {
                    throw new AccessFailureException();
                }
            }
        }
        internal sealed class SnmpInBadCommunityUses : ScalarObject
        {
            /// <summary>
            /// Initializes a new instance of the <see cref="SnmpInBadCommunityUses"/> class.
            /// </summary>
            public SnmpInBadCommunityUses()
                : base(".1.3.6.1.2.1.11.5.0")
            {
            }
            /// <summary>
            /// 
            /// </summary>
            public override ISnmpData Data
            {
                get
                {
                    dynamic stats = SnmpStatistics;
                    return new Counter32(stats.SnmpInBadCommunityUses);
                }
                set
                {
                    throw new AccessFailureException();
                }
            }
        }
        internal sealed class SnmpInASNParseErrs : ScalarObject
        {
            /// <summary>
            /// Initializes a new instance of the <see cref="SnmpInASNParseErrs"/> class.
            /// </summary>
            public SnmpInASNParseErrs()
                : base(".1.3.6.1.2.1.11.6.0")
            {
            }
            /// <summary>
            /// 
            /// </summary>
            public override ISnmpData Data
            {
                get
                {
                    dynamic stats = SnmpStatistics;
                    return new Counter32(stats.SnmpInASNParseErrs);
                }
                set
                {
                    throw new AccessFailureException();
                }
            }
        }
        internal sealed class SnmpInTooBigs : ScalarObject
        {
            /// <summary>
            /// Initializes a new instance of the <see cref="SnmpInTooBigs"/> class.
            /// </summary>
            public SnmpInTooBigs()
                : base(".1.3.6.1.2.1.11.8.0")
            {
            }
            /// <summary>
            /// 
            /// </summary>
            public override ISnmpData Data
            {
                get
                {
                    dynamic stats = SnmpStatistics;
                    return new Counter32(stats.SnmpInTooBigs);
                }
                set
                {
                    throw new AccessFailureException();
                }
            }
        }
        internal sealed class SnmpInNoSuchNames : ScalarObject
        {
            /// <summary>
            /// Initializes a new instance of the <see cref="SnmpInNoSuchNames"/> class.
            /// </summary>
            public SnmpInNoSuchNames()
                : base(".1.3.6.1.2.1.11.9.0")
            {
            }
            /// <summary>
            /// 
            /// </summary>
            public override ISnmpData Data
            {
                get
                {
                    dynamic stats = SnmpStatistics;
                    return new Counter32(stats.SnmpInNoSuchNames);
                }
                set
                {
                    throw new AccessFailureException();
                }
            }
        }
        internal sealed class SnmpInBadValues : ScalarObject
        {
            /// <summary>
            /// Initializes a new instance of the <see cref="SnmpInBadValues"/> class.
            /// </summary>
            public SnmpInBadValues()
                : base(".1.3.6.1.2.1.11.10.0")
            {
            }
            /// <summary>
            /// 
            /// </summary>
            public override ISnmpData Data
            {
                get
                {
                    dynamic stats = SnmpStatistics;
                    return new Counter32(stats.SnmpInBadValues);
                }
                set
                {
                    throw new AccessFailureException();
                }
            }
        }
        internal sealed class SnmpInReadOnlys : ScalarObject
        {
            /// <summary>
            /// Initializes a new instance of the <see cref="SnmpInReadOnlys"/> class.
            /// </summary>
            public SnmpInReadOnlys()
                : base(".1.3.6.1.2.1.11.11.0")
            {
            }
            /// <summary>
            /// 
            /// </summary>
            public override ISnmpData Data
            {
                get
                {
                    dynamic stats = SnmpStatistics;
                    return new Counter32(stats.SnmpInReadOnlys);
                }
                set
                {
                    throw new AccessFailureException();
                }
            }
        }
        internal sealed class SnmpInGenErrs : ScalarObject
        {
            /// <summary>
            /// Initializes a new instance of the <see cref="SnmpInGenErrs"/> class.
            /// </summary>
            public SnmpInGenErrs()
                : base(".1.3.6.1.2.1.11.12.0")
            {
            }
            /// <summary>
            /// 
            /// </summary>
            public override ISnmpData Data
            {
                get
                {
                    dynamic stats = SnmpStatistics;
                    return new Counter32(stats.SnmpInGenErrs);
                }
                set
                {
                    throw new AccessFailureException();
                }
            }
        }
        internal sealed class SnmpInTotalReqVars : ScalarObject
        {
            /// <summary>
            /// Initializes a new instance of the <see cref="SnmpInTotalReqVars"/> class.
            /// </summary>
            public SnmpInTotalReqVars()
                : base(".1.3.6.1.2.1.11.13.0")
            {
            }
            /// <summary>
            /// 
            /// </summary>
            public override ISnmpData Data
            {
                get
                {
                    dynamic stats = SnmpStatistics;
                    return new Counter32(stats.SnmpInTotalReqVars);
                }
                set
                {
                    throw new AccessFailureException();
                }
            }
        }
        internal sealed class SnmpInTotalSetVars : ScalarObject
        {
            /// <summary>
            /// Initializes a new instance of the <see cref="SnmpInTotalSetVars"/> class.
            /// </summary>
            public SnmpInTotalSetVars()
                : base(".1.3.6.1.2.1.11.14.0")
            {
            }
            /// <summary>
            /// 
            /// </summary>
            public override ISnmpData Data
            {
                get
                {
                    dynamic stats = SnmpStatistics;
                    return new Counter32(stats.SnmpInTotalSetVars);
                }
                set
                {
                    throw new AccessFailureException();
                }
            }
        }
        internal sealed class SnmpInGetRequests : ScalarObject
        {
            /// <summary>
            /// Initializes a new instance of the <see cref="SnmpInGetRequests"/> class.
            /// </summary>
            public SnmpInGetRequests()
                : base(".1.3.6.1.2.1.11.15.0")
            {
            }
            /// <summary>
            /// 
            /// </summary>
            public override ISnmpData Data
            {
                get
                {
                    dynamic stats = SnmpStatistics;
                    return new Counter32(stats.SnmpInGetRequests);
                }
                set
                {
                    throw new AccessFailureException();
                }
            }
        }
        internal sealed class SnmpInGetNexts : ScalarObject
        {
            /// <summary>
            /// Initializes a new instance of the <see cref="SnmpInGetNexts"/> class.
            /// </summary>
            public SnmpInGetNexts()
                : base(".1.3.6.1.2.1.11.16.0")
            {
            }
            /// <summary>
            /// 
            /// </summary>
            public override ISnmpData Data
            {
                get
                {
                    dynamic stats = SnmpStatistics;
                    return new Counter32(stats.SnmpInGetNexts);
                }
                set
                {
                    throw new AccessFailureException();
                }
            }
        }
        internal sealed class SnmpInSetRequests : ScalarObject
        {
            /// <summary>
            /// Initializes a new instance of the <see cref="SnmpInSetRequests"/> class.
            /// </summary>
            public SnmpInSetRequests()
                : base(".1.3.6.1.2.1.11.17.0")
            {
            }
            /// <summary>
            /// 
            /// </summary>
            public override ISnmpData Data
            {
                get
                {
                    dynamic stats = SnmpStatistics;
                    return new Counter32(stats.SnmpInSetRequests);
                }
                set
                {
                    throw new AccessFailureException();
                }
            }
        }
        internal sealed class SnmpInGetResponses : ScalarObject
        {
            /// <summary>
            /// Initializes a new instance of the <see cref="SnmpInGetResponses"/> class.
            /// </summary>
            public SnmpInGetResponses()
                : base(".1.3.6.1.2.1.11.18.0")
            {
            }
            /// <summary>
            /// 
            /// </summary>
            public override ISnmpData Data
            {
                get
                {
                    dynamic stats = SnmpStatistics;
                    return new Counter32(stats.SnmpInGetResponses);
                }
                set
                {
                    throw new AccessFailureException();
                }
            }
        }
        internal sealed class SnmpInTraps : ScalarObject
        {
            /// <summary>
            /// Initializes a new instance of the <see cref="SnmpInTraps"/> class.
            /// </summary>
            public SnmpInTraps()
                : base(".1.3.6.1.2.1.11.19.0")
            {
            }
            /// <summary>
            /// 
            /// </summary>
            public override ISnmpData Data
            {
                get
                {
                    dynamic stats = SnmpStatistics;
                    return new Counter32(stats.SnmpInTraps);
                }
                set
                {
                    throw new AccessFailureException();
                }
            }
        }
        internal sealed class SnmpOutTooBigs : ScalarObject
        {
            /// <summary>
            /// Initializes a new instance of the <see cref="SnmpOutTooBigs"/> class.
            /// </summary>
            public SnmpOutTooBigs()
                : base(".1.3.6.1.2.1.11.20.0")
            {
            }
            /// <summary>
            /// 
            /// </summary>
            public override ISnmpData Data
            {
                get
                {
                    dynamic stats = SnmpStatistics;
                    return new Counter32(stats.SnmpOutTooBigs);
                }
                set
                {
                    throw new AccessFailureException();
                }
            }
        }
        internal sealed class SnmpOutNotSuchNames : ScalarObject
        {
            /// <summary>
            /// Initializes a new instance of the <see cref="SnmpOutNotSuchNames"/> class.
            /// </summary>
            public SnmpOutNotSuchNames()
                : base(".1.3.6.1.2.1.11.21.0")
            {
            }
            /// <summary>
            /// 
            /// </summary>
            public override ISnmpData Data
            {
                get
                {
                    dynamic stats = SnmpStatistics;
                    return new Counter32(stats.SnmpOutNoSuchNames);
                }
                set
                {
                    throw new AccessFailureException();
                }
            }
        }
        internal sealed class SnmpOutBadValues : ScalarObject
        {
            /// <summary>
            /// Initializes a new instance of the <see cref="SnmpOutBadValues"/> class.
            /// </summary>
            public SnmpOutBadValues()
                : base(".1.3.6.1.2.1.11.22.0")
            {
            }
            /// <summary>
            /// 
            /// </summary>
            public override ISnmpData Data
            {
                get
                {
                    dynamic stats = SnmpStatistics;
                    return new Counter32(stats.SnmpOutBadValues);
                }
                set
                {
                    throw new AccessFailureException();
                }
            }
        }
        internal sealed class SnmpOutGenErrs : ScalarObject
        {
            /// <summary>
            /// Initializes a new instance of the <see cref="SnmpOutGenErrs"/> class.
            /// </summary>
            public SnmpOutGenErrs()
                : base(".1.3.6.1.2.1.11.24.0")
            {
            }
            /// <summary>
            /// 
            /// </summary>
            public override ISnmpData Data
            {
                get
                {
                    dynamic stats = SnmpStatistics;
                    return new Counter32(stats.SnmpOutGenErrs);
                }
                set
                {
                    throw new AccessFailureException();
                }
            }
        }
        internal sealed class SnmpOutGetRequests : ScalarObject
        {
            /// <summary>
            /// Initializes a new instance of the <see cref="SnmpOutGetRequests"/> class.
            /// </summary>
            public SnmpOutGetRequests()
                : base(".1.3.6.1.2.1.11.25.0")
            {
            }
            /// <summary>
            /// 
            /// </summary>
            public override ISnmpData Data
            {
                get
                {
                    dynamic stats = SnmpStatistics;
                    return new Counter32(stats.SnmpOutGetRequests);
                }
                set
                {
                    throw new AccessFailureException();
                }
            }
        }
        internal sealed class SnmpOutGetNexts : ScalarObject
        {
            /// <summary>
            /// Initializes a new instance of the <see cref="SnmpOutGetNexts"/> class.
            /// </summary>
            public SnmpOutGetNexts()
                : base(".1.3.6.1.2.1.11.26.0")
            {
            }
            /// <summary>
            /// 
            /// </summary>
            public override ISnmpData Data
            {
                get
                {
                    dynamic stats = SnmpStatistics;
                    return new Counter32(stats.SnmpOutGetNexts);
                }
                set
                {
                    throw new AccessFailureException();
                }
            }
        }
        internal sealed class SnmpOutSetRequests : ScalarObject
        {
            /// <summary>
            /// Initializes a new instance of the <see cref="SnmpOutSetRequests"/> class.
            /// </summary>
            public SnmpOutSetRequests()
                : base(".1.3.6.1.2.1.11.27.0")
            {
            }
            /// <summary>
            /// 
            /// </summary>
            public override ISnmpData Data
            {
                get
                {
                    dynamic stats = SnmpStatistics;
                    return new Counter32(stats.SnmpOutSetRequests);
                }
                set
                {
                    throw new AccessFailureException();
                }
            }
        }
        internal sealed class SnmpOutGetResponses : ScalarObject
        {
            /// <summary>
            /// Initializes a new instance of the <see cref="SnmpOutGetResponses"/> class.
            /// </summary>
            public SnmpOutGetResponses()
                : base(".1.3.6.1.2.1.11.28.0")
            {
            }
            /// <summary>
            /// 
            /// </summary>
            public override ISnmpData Data
            {
                get
                {
                    dynamic stats = SnmpStatistics;
                    return new Counter32(stats.SnmpOutGetResponses);
                }
                set
                {
                    throw new AccessFailureException();
                }
            }
        }
        internal sealed class SnmpOutTraps : ScalarObject
        {
            /// <summary>
            /// Initializes a new instance of the <see cref="SnmpOutTraps"/> class.
            /// </summary>
            public SnmpOutTraps()
                : base(".1.3.6.1.2.1.11.29.0")
            {
            }
            /// <summary>
            /// 
            /// </summary>
            public override ISnmpData Data
            {
                get
                {
                    dynamic stats = SnmpStatistics;
                    return new Counter32(stats.SnmpOutTraps);
                }
                set
                {
                    throw new AccessFailureException();
                }
            }
        }
        internal sealed class SnmpEnableAuthenTraps : ScalarObject
        {
            /// <summary>
            /// Initializes a new instance of the <see cref="SnmpEnableAuthenTraps"/> class.
            /// </summary>
            public SnmpEnableAuthenTraps()
                : base(".1.3.6.1.2.1.11.30.0")
            {
            }
            /// <summary>
            /// 
            /// </summary>
            public override ISnmpData Data
            {
                get
                {
                    dynamic stats = SnmpStatistics;
                    return new Integer32(stats.SnmpEnableAuthenTraps);
                }
                set
                {
                    throw new AccessFailureException();
                }
            }
        }


        public U5KMIBIISnmpObjectGroup() : base()
        {
        }

        // static protected Func<object> SnmpStatistics;
        static protected dynamic SnmpStatistics
        {
            get
            {
                dynamic stats = null ;
                U5kMIBIIMib.GetDataAgent((data) =>
                {
                    stats = ((dynamic)data).SnmpStatistics;
                });
                return stats;
            }
        }

#if DEBUG
        public void TestSnmpObjects()
        {
        }
#endif
    }
    /// <summary>
    /// 
    /// </summary>
    class U5KMIBIIRMonObjectGroup : U5kMibGroup
    {
        protected override void LoadElements()
        {
            snmpObjects = new List<SnmpObjectBase>()
            {
                new AlarmTable(FireEvent),
                new EventTable(),
                new LogTable()
            };
        }
        public U5KMIBIIRMonObjectGroup() : base()
        {
        }

        internal class AlarmTable : TableObject, IDisposable
        {
            internal sealed class AlarmIndex : ScalarObject
            {
                private readonly Int32 Index;
                /// <summary>
                /// Initializes a new instance of the <see cref="AlarmInterval"/> class.
                public AlarmIndex(int index, object onlineData)
                    : base(".1.3.6.1.2.1.16.3.1.1.1.{0}", index)
                {
                    Index = index;
                }
                public override ISnmpData Data
                {
                    get
                    {
                        return new Integer32(Index);
                    }
                    set { throw new AccessFailureException(); }
                }
            }
            internal sealed class AlarmInterval : ScalarObject
            {
                private readonly Int32 Index;
                /// <summary>
                /// Initializes a new instance of the <see cref="AlarmInterval"/> class.
                public AlarmInterval(int index, object onlineData)
                    : base(".1.3.6.1.2.1.16.3.1.1.2.{0}", index)
                {
                    Index = index;
                }
                public override ISnmpData Data
                {
                    get
                    {
                        return new Integer32((int)(AlarmTableData[Index-1].interval));
                    }
                    set { throw new AccessFailureException(); }
                }
            }
            internal sealed class AlarmVariable : ScalarObject
            {
                private readonly Int32 Index;
                /// <summary>
                /// Initializes a new instance of the <see cref="AlarmVariable"/> class.
                public AlarmVariable(int index, object onlineData)
                    // ReSharper restore UnusedParameter.Local
                    : base(".1.3.6.1.2.1.16.3.1.1.3.{0}", index)
                {
                    Index = index;
                }
                public override ISnmpData Data
                {
                    get
                    {
                        return new OctetString(AlarmTableData[Index - 1].variable);
                    }
                    set { throw new AccessFailureException(); }
                }
            }
            internal sealed class AlarmSampleType : ScalarObject
            {
                private readonly Int32 Index;
                /// <summary>
                /// Initializes a new instance of the <see cref="AlarmSampleType"/> class.
                public AlarmSampleType(int index, object onlineData)
                    : base(".1.3.6.1.2.1.16.3.1.1.4.{0}", index)
                {
                    Index = index;
                }
                public override ISnmpData Data
                {
                    get
                    {
                        return new Integer32((int)(AlarmTableData[Index - 1].stype));
                    }
                    set { throw new AccessFailureException(); }
                }
            }
            internal sealed class AlarmValue : ScalarObject
            {
                private readonly Int32 Index;
                /// <summary>
                /// Initializes a new instance of the <see cref="AlarmValue"/> class.
                public AlarmValue(int index, object onlineData)
                    : base(".1.3.6.1.2.1.16.3.1.1.5.{0}", index)
                {
                    Index = index;
                }
                public override ISnmpData Data
                {
                    get
                    {
                        return new Integer32((int)(AlarmTableData[Index - 1].lastv));
                    }
                    set { throw new AccessFailureException(); }
                }
            }
            internal sealed class AlarmStartupAlarm : ScalarObject
            {
                private readonly Int32 Index;
                /// <summary>
                /// Initializes a new instance of the <see cref="AlarmStartupAlarm"/> class.
                public AlarmStartupAlarm(int index, object onlineData)
                    : base(".1.3.6.1.2.1.16.3.1.1.6.{0}", index)
                {
                    Index = index;
                }
                public override ISnmpData Data
                {
                    get
                    {
                        return new Integer32((int)(AlarmTableData[Index - 1].when));
                    }
                    set { throw new AccessFailureException(); }
                }
            }
            internal sealed class AlarmRisingThreshold : ScalarObject
            {
                private readonly Int32 Index;
                /// <summary>
                /// Initializes a new instance of the <see cref="AlarmRisingThreshold"/> class.
                public AlarmRisingThreshold(int index, object onlineData)
                    : base(".1.3.6.1.2.1.16.3.1.1.7.{0}", index)
                {
                    Index = index;
                }
                public override ISnmpData Data
                {
                    get
                    {
                        return new Integer32((int)(AlarmTableData[Index - 1].rTh));
                    }
                    set { throw new AccessFailureException(); }
                }
            }
            internal sealed class AlarmFallingThreshold : ScalarObject
            {
                private readonly Int32 Index;
                /// <summary>
                /// Initializes a new instance of the <see cref="AlarmFallingThreshold"/> class.
                public AlarmFallingThreshold(int index, object onlineData)
                    : base(".1.3.6.1.2.1.16.3.1.1.8.{0}", index)
                {
                    Index = index;
                }
                public override ISnmpData Data
                {
                    get
                    {
                        return new Integer32((int)(AlarmTableData[Index - 1].fTh));
                    }
                    set { throw new AccessFailureException(); }
                }
            }
            internal sealed class AlarmRisingEventIndex : ScalarObject
            {
                private readonly Int32 Index;
                /// <summary>
                /// Initializes a new instance of the <see cref="AlarmRisingEventIndex"/> class.
                public AlarmRisingEventIndex(int index, object onlineData)
                    : base(".1.3.6.1.2.1.16.3.1.1.9.{0}", index)
                {
                    Index = index;
                }
                public override ISnmpData Data
                {
                    get
                    {
                        return new Integer32((int)(AlarmTableData[Index - 1].reindex));
                    }
                    set { throw new AccessFailureException(); }
                }
            }
            internal sealed class AlarmFallingEventIndex : ScalarObject
            {
                private readonly Int32 Index;
                /// <summary>
                /// Initializes a new instance of the <see cref="AlarmFallingEventIndex"/> class.
                public AlarmFallingEventIndex(int index, object onlineData)
                    : base(".1.3.6.1.2.1.16.3.1.1.10.{0}", index)
                {
                    Index = index;
                }
                public override ISnmpData Data
                {
                    get
                    {
                        return new Integer32((int)(AlarmTableData[Index - 1].feindex));
                    }
                    set { throw new AccessFailureException(); }
                }
            }
            internal sealed class AlarmOwner : ScalarObject
            {
                private readonly Int32 Index;
                /// <summary>
                /// Initializes a new instance of the <see cref="AlarmOwner"/> class.
                public AlarmOwner(int index, object onlineData)
                    // ReSharper restore UnusedParameter.Local
                    : base(".1.3.6.1.2.1.16.3.1.1.11.{0}", index)
                {
                    Index = index;
                }
                public override ISnmpData Data
                {
                    get
                    {
                        return new OctetString(AlarmTableData[Index - 1].owner);
                    }
                    set { throw new AccessFailureException(); }
                }
            }
            internal sealed class AlarmStatus : ScalarObject
            {
                private readonly Int32 Index;
                /// <summary>
                /// Initializes a new instance of the <see cref="AlarmStatus"/> class.
                public AlarmStatus(int index, object onlineData)
                    : base(".1.3.6.1.2.1.16.3.1.1.12.{0}", index)
                {
                    Index = index;
                }
                public override ISnmpData Data
                {
                    get
                    {
                        return new Integer32((int)(AlarmTableData[Index - 1].st));
                    }
                    set { throw new AccessFailureException(); }
                }
            }

            /// <summary>
            /// Initializes a new instance of the <see cref="AlarmTable"/> class.
            /// </summary>
            System.Threading.ManualResetEvent supervisor = new System.Threading.ManualResetEvent(false);
            public AlarmTable(Action<int, string, ObjectIdentifier, IList<Variable>> fireEvents)
            {
                FireEvent = fireEvents;
                LoadElements();
                /** Supervisor de la tabla */
                Task.Factory.StartNew(() =>
                {
                    var table = (List<dynamic>)AlarmTableData;
                    while (supervisor != null && supervisor.WaitOne(TimeSpan.FromSeconds(10)) == false)
                    {
                        DateTime timePoll = DateTime.Now;
                        table.ForEach(w =>
                        {
                            TimeSpan elapsed = timePoll - w.lastp;
                            if (elapsed >= TimeSpan.FromSeconds(w.interval))
                            {
                                w.lastp = timePoll;
                                // Valor de la Variable..
                                if (Ed137RevCMib.SnmpObjectGet(w.variable) is ScalarObject objvar)
                                {
                                    var variable = (objvar.Data as Integer32).ToInt32();
                                    if (variable != w.lastv)
                                    {
                                        if (w.when == 1)        // Rising..
                                        {
                                            if (w.lastv < w.rTh && variable >= w.rTh)
                                            {
                                                // Evento Rising....
                                                List<Variable> lvar = new List<Variable>()
                                                {
                                                    new Variable(new ObjectIdentifier(".1.3.6.1.2.1.16.3.1.1.1"), new Integer32(w.index)),
                                                    new Variable(new ObjectIdentifier(".1.3.6.1.2.1.16.3.1.1.3"), new OctetString(w.variable)),
                                                    new Variable(new ObjectIdentifier(".1.3.6.1.2.1.16.3.1.1.4"), new Integer32(w.stype)),
                                                    new Variable(new ObjectIdentifier(".1.3.6.1.2.1.16.3.1.1.5"), new Integer32(variable)),
                                                    new Variable(new ObjectIdentifier(".1.3.6.1.2.1.16.3.1.1.7"), new Integer32(w.rTh)),
                                                };
                                                FireEvent?.Invoke(w.reindex, w.variable, new ObjectIdentifier(".1.3.6.1.2.1.16.0.1"), lvar);
                                            }
                                        }
                                        else if (w.when == 2)   // Falling.
                                        {
                                            if (w.lastv >= w.fTh && variable < w.fTh)
                                            {
                                                // Evento Falling.
                                                List<Variable> lvar = new List<Variable>()
                                                {
                                                    new Variable(new ObjectIdentifier(".1.3.6.1.2.1.16.3.1.1.1"), new Integer32(w.index)),
                                                    new Variable(new ObjectIdentifier(".1.3.6.1.2.1.16.3.1.1.3"), new OctetString(w.variable)),
                                                    new Variable(new ObjectIdentifier(".1.3.6.1.2.1.16.3.1.1.4"), new Integer32(w.stype)),
                                                    new Variable(new ObjectIdentifier(".1.3.6.1.2.1.16.3.1.1.5"), new Integer32(variable)),
                                                    new Variable(new ObjectIdentifier(".1.3.6.1.2.1.16.3.1.1.7"), new Integer32(w.fTh)),
                                                };
                                                FireEvent?.Invoke(w.feindex, w.variable, new ObjectIdentifier(".1.3.6.1.2.1.16.0.2"), lvar);
                                            }
                                        }
                                        w.lastv = variable;
                                    }
                                }
                            }
                        });
                    }
                });
            }

            // ".1.3.6.1.2.1.4.21"
            private readonly IList<ScalarObject> _elements = new List<ScalarObject>();
            private void LoadElements()
            {
                _elements.Clear();
                var columnTypes = new[]
                {
                    typeof(AlarmIndex),
                    typeof(AlarmInterval),
                    typeof(AlarmVariable),
                    typeof(AlarmSampleType),
                    typeof(AlarmValue),
                    typeof(AlarmStartupAlarm),
                    typeof(AlarmRisingThreshold),
                    typeof(AlarmFallingThreshold),
                    typeof(AlarmRisingEventIndex),
                    typeof(AlarmFallingEventIndex),
                    typeof(AlarmOwner),
                    typeof(AlarmStatus),
                };
                foreach (var type in columnTypes)
                {
                    for (int i = 0; i < AlarmTableData.Count; i++)
                    {
                        _elements.Add((ScalarObject)Activator.CreateInstance(type, new object[] { i + 1, AlarmTableData[i] }));
                    }
                }
            }

            public void Dispose()
            {
                supervisor?.Set();
                supervisor = null;
            }

            static protected dynamic AlarmTableData
            {
                get
                {
                    dynamic evtdata = null;
                    U5kMIBIIMib.GetDataAgent((data) =>
                    {
                        evtdata = ((dynamic)data).RMONAlarmTable;
                    });
                    return evtdata;
                }
            }
            protected override IEnumerable<ScalarObject> Objects
            {
                get { return _elements; }
            }
            protected Action<int, string, ObjectIdentifier, IList<Variable>> FireEvent;
        }
        internal class EventTable : TableObject
        {
            internal sealed class EventIndex : ScalarObject
            {
                private readonly Int32 Index;
                /// <summary>
                /// Initializes a new instance of the <see cref="EventDescription"/> class.
                public EventIndex(int index, object onlineData)
                    : base(".1.3.6.1.2.1.16.9.1.1.1.{0}", index)
                {
                    Index = index;
                }
                public override ISnmpData Data
                {
                    get
                    {
                        return new Integer32(Index);
                    }
                    set { throw new AccessFailureException(); }
                }
            }
            internal sealed class EventDescription : ScalarObject
            {
                private readonly Int32 Index;
                /// <summary>
                /// Initializes a new instance of the <see cref="EventDescription"/> class.
                public EventDescription(int index, object onlineData)
                    : base(".1.3.6.1.2.1.16.9.1.1.2.{0}", index)
                {
                    Index = index;
                }
                public override ISnmpData Data
                {
                    get
                    {
                        return new OctetString(EventTableData[Index-1].description);
                    }
                    set { throw new AccessFailureException(); }
                }
            }
            internal sealed class EventType : ScalarObject
            {
                private readonly Int32 Index;
                /// <summary>
                /// Initializes a new instance of the <see cref="EventType"/> class.
                public EventType(int index, object onlineData)
                    // ReSharper restore UnusedParameter.Local
                    : base(".1.3.6.1.2.1.16.9.1.1.3.{0}", index)
                {
                    Index = index;
                }
                public override ISnmpData Data
                {
                    get
                    {
                        return new Integer32(EventTableData[Index - 1].type);
                    }
                    set { throw new AccessFailureException(); }
                }
            }
            internal sealed class EventCommunity : ScalarObject
            {
                private readonly Int32 Index;
                /// <summary>
                /// Initializes a new instance of the <see cref="EventCommunity"/> class.
                public EventCommunity(int index, object onlineData)
                    : base(".1.3.6.1.2.1.16.9.1.1.4.{0}", index)
                {
                    Index = index;
                }
                public override ISnmpData Data
                {
                    get
                    {
                        return new OctetString(EventTableData[Index - 1].community);
                    }
                    set { throw new AccessFailureException(); }
                }
            }
            internal sealed class EventLastTimeSent : ScalarObject
            {
                private readonly Int32 Index;
                /// <summary>
                /// Initializes a new instance of the <see cref="EventLastTimeSent"/> class.
                public EventLastTimeSent(int index, object onlineData)
                    : base(".1.3.6.1.2.1.16.9.1.1.5.{0}", index)
                {
                    Index = index;
                }
                public override ISnmpData Data
                {
                    get
                    {
                        return new Integer32((Int32)(EventTableData[Index - 1].last));
                    }
                    set { throw new AccessFailureException(); }
                }
            }
            internal sealed class EventOwner : ScalarObject
            {
                private readonly Int32 Index;
                /// <summary>
                /// Initializes a new instance of the <see cref="EventOwner"/> class.
                public EventOwner(int index, object onlineData)
                    : base(".1.3.6.1.2.1.16.9.1.1.6.{0}", index)
                {
                    Index = index;
                }
                public override ISnmpData Data
                {
                    get
                    {
                        return new OctetString(EventTableData[Index - 1].owner);
                    }
                    set { throw new AccessFailureException(); }
                }
            }
            internal sealed class EventStatus : ScalarObject
            {
                private readonly Int32 Index;
                /// <summary>
                /// Initializes a new instance of the <see cref="EventStatus"/> class.
                public EventStatus(int index, object onlineData)
                    : base(".1.3.6.1.2.1.16.9.1.1.7.{0}", index)
                {
                    Index = index;
                }
                public override ISnmpData Data
                {
                    get
                    {
                        return new Integer32(1);
                    }
                    set { throw new AccessFailureException(); }
                }
            }

            /// <summary>
            /// Initializes a new instance of the <see cref="EventTable"/> class.
            /// </summary>
            public EventTable()
            {
                LoadElements();
            }

            // ".1.3.6.1.2.1.4.21"
            private readonly IList<ScalarObject> _elements = new List<ScalarObject>();
            private void LoadElements()
            {
                _elements.Clear();
                var columnTypes = new[]
                {
                    typeof(EventIndex),
                    typeof(EventDescription),
                    typeof(EventType),
                    typeof(EventCommunity),
                    typeof(EventLastTimeSent),
                    typeof(EventOwner),
                    typeof(EventStatus),
                };
                foreach (var type in columnTypes)
                {
                    for (int i = 0; i < EventTableData.Count; i++)
                    {
                        _elements.Add((ScalarObject)Activator.CreateInstance(type, new object[] { i + 1, EventTableData[i] }));
                    }
                }
            }
            protected override IEnumerable<ScalarObject> Objects
            {
                get { return _elements; }
            }
            /// <summary>
            /// 
            /// </summary>
            /// <param name="index"></param>
            /// <param name="oidvar"></param>
            /// <returns>Retorna TRUE si hay que alamacenar el evento</returns>
            public void EventDataGet(int index, Action<string, int, string> execute)
            {
                var table = EventTableData as List<dynamic>;
                if (table != null)
                {
                    var row = table.Where(r => r.index == index).FirstOrDefault() as dynamic;
                    if (row != null)
                    {
                        row.last = (int)(Environment.TickCount / 10);
                        execute(row.description, row.type, row.community);
                    }
                }
            }
        }
        internal class LogTable : TableObject
        {
            internal sealed class LogEventIndex : ScalarObject
            {
                private readonly Int32 Index;
                /// <summary>
                /// Initializes a new instance of the <see cref="LogIndex"/> class.
                public LogEventIndex(int index, object onlineData)
                    : base(".1.3.6.1.2.1.16.9.2.1.1.{0}", index)
                {
                    Index = index;
                }
                public override ISnmpData Data
                {
                    get
                    {
                        return new Integer32((int)(logTableData[Index-1].logEventIndex));
                    }
                    set { throw new AccessFailureException(); }
                }
            }
            internal sealed class LogIndex : ScalarObject
            {
                private readonly Int32 Index;
                /// <summary>
                /// Initializes a new instance of the <see cref="LogIndex"/> class.
                public LogIndex(int index, object onlineData)
                    : base(".1.3.6.1.2.1.16.9.2.1.2.{0}", index)
                {
                    Index = index;
                }
                public override ISnmpData Data
                {
                    get
                    {
                        return new Integer32(Index);
                    }
                    set { throw new AccessFailureException(); }
                }
            }
            internal sealed class LogTime : ScalarObject
            {
                private readonly Int32 Index;
                /// <summary>
                /// Initializes a new instance of the <see cref="LogTime"/> class.
                public LogTime(int index, object onlineData)
                    : base(".1.3.6.1.2.1.16.9.2.1.3.{0}", index)
                {
                    Index = index;
                }
                public override ISnmpData Data
                {
                    get
                    {
                        return new Integer32((int)(logTableData[Index - 1].logTime));
                    }
                    set { throw new AccessFailureException(); }
                }
            }
            internal sealed class LogDescription : ScalarObject
            {
                private readonly Int32 Index;
                /// <summary>
                /// Initializes a new instance of the <see cref="LogDescription"/> class.
                public LogDescription(int index, object onlineData)
                    : base(".1.3.6.1.2.1.16.9.2.1.4.{0}", index)
                {
                    Index = index;
                }
                public override ISnmpData Data
                {
                    get
                    {
                        return new OctetString(logTableData[Index - 1].logDescription);
                    }
                    set { throw new AccessFailureException(); }
                }
            }

            /// <summary>
            /// Initializes a new instance of the <see cref="LogTable"/> class.
            /// </summary>
            public LogTable()
            {
                LoadElements();
            }

            private readonly IList<ScalarObject> _elements = new List<ScalarObject>();
            private void LoadElements()
            {
                _elements.Clear();
                var columnTypes = new[]    
                {
                    typeof(LogEventIndex),
                    typeof(LogIndex),
                    typeof(LogTime),
                    typeof(LogDescription),
                };
                foreach (var type in columnTypes)
                {
                    for (int i = 0; i < logTableData.Count; i++)
                    {
                        _elements.Add((ScalarObject)Activator.CreateInstance(type, new object[] { i + 1, logTableData[i] }));
                    }
                }
            }
            protected override IEnumerable<ScalarObject> Objects
            {
                get { return _elements; }
            }

            protected static List<dynamic> logTableData = new List<dynamic>();
            public void AddLog(int evIndex, string desc)
            {
                int index = logTableData.Count + 1;
                logTableData.Add(new
                {
                    logEventIndex = evIndex,
                    index = index,
                    logTime = (int)(Environment.TickCount / 10),
                    logDescription = desc
                });
                LoadElements();
            }
        }

        static protected dynamic EventTableData
        {
            get
            {
                dynamic evtdata = null ;
                U5kMIBIIMib.GetDataAgent((data) =>
                {
                    evtdata = ((dynamic)data).RMONEventTable;
                });
                return evtdata;
            }
        }
        protected void FireEvent(int index, string oidVar, ObjectIdentifier trap, IList<Variable> lvar)
        {
            /** Obtener la tabla de eventos */
            if (snmpObjects.Where(o => o is EventTable).FirstOrDefault() is EventTable eventTable)
            {
                eventTable.EventDataGet(index, (desc, type, com) =>
                {
                    if (type == 3 || type == 4)
                    {
                        /** Generar el TRAP */
                        U5kMIBIIMib.GetDataAgent((data) =>
                        {
                            ((dynamic)data).RMonEventTrap(trap, lvar);
                        });
                    }
                    if (type == 2 || type == 4)
                    {
                        /** Archivar Log. */
                        if (snmpObjects.Where(o => o is LogTable).FirstOrDefault() is LogTable logTable)
                        {
                            logTable.AddLog(index, desc + ": " + oidVar);
                        }
                    }
                });
            }
        }
    }
    /// <summary>
    /// 
    /// </summary>
    internal class U5kMIBIIMib : U5kiMIb
    {
        public U5kMIBIIMib(AgentDataGet getDataAgent)
        {
            GetDataAgent = getDataAgent;
            LoadGroups();
        }
        protected override void LoadGroups()
        {
            mibGroups = new List<U5kMibGroup>()
            {
                new U5kMIBIISystemGroup(),
                new U5kMIBIIInterfacesGroup(),
                new U5kMIBIIIpObjectGroup(),
                new U5KMIBIITcpObjectGroup(),
                new U5KMIBIIUdpObjectGroup(),
                new U5KMIBIISnmpObjectGroup(),
                new U5KMIBIIRMonObjectGroup()
            };
        }
        public static AgentDataGet GetDataAgent { get; set; }
    }


}
