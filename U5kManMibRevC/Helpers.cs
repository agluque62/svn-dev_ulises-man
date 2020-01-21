using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Runtime.InteropServices;
using System.ComponentModel;
using System.Net;
using System.Net.Sockets;
using System.Net.NetworkInformation;
using System.Management;

namespace U5kManMibRevC
{
    public class MibIITableHelpers
    {
        public class IpHlpApiHelper
        {
            public delegate int TableDataGetDel(IntPtr pTable, ref int dwsize, bool bOrder);

            // The insufficient buffer error.
            const int ERROR_INSUFFICIENT_BUFFER = 122;

            /** Datos Grupo IF */
            public const int IF_ROW_COUNT = 16;
            public const int MAX_INTERFACE_NAME_LEN = 256;
            public const int MAXLEN_PHYSADDR = 8;
            public const int MAXLEN_IFDESCR = 256;
            [StructLayout(LayoutKind.Sequential)]
            public struct MIB_IFROW
            {
                [MarshalAs(UnmanagedType.ByValArray, SizeConst = MAX_INTERFACE_NAME_LEN*2)]
                public byte[] wszName;
                public UInt32 dwIndex;
                public UInt32 dwType;
                public UInt32 dwMtu;
                public UInt32 dwSpeed;
                public UInt32 dwPhysAddrLen;
                [MarshalAs(UnmanagedType.ByValArray, SizeConst = MAXLEN_PHYSADDR)]
                public byte[] bPhysAddr;
                public UInt32 dwAdminStatus;
                public UInt32 dwOperStatus;
                public UInt32 dwLastChange;
                public UInt32 dwInOctets;
                public UInt32 dwInUcastPkts;
                public UInt32 dwInNUcastPkts;
                public UInt32 dwInDiscards;
                public UInt32 dwInErrors;
                public UInt32 dwInUnknownProtos;
                public UInt32 dwOutOctets;
                public UInt32 dwOutUcastPkts;
                public UInt32 dwOutNUcastPkts;
                public UInt32 dwOutDiscards;
                public UInt32 dwOutErrors;
                public UInt32 dwOutQLen;
                public UInt32 dwDescrLen;
                [MarshalAs(UnmanagedType.ByValArray, SizeConst = MAXLEN_IFDESCR)]
                public byte[] bDescr;
            }

                /** Datos Grupo IP */
                [StructLayout(LayoutKind.Sequential)]
            public struct MIB_IPSTATS
            {
                public Int32 dwForwarding;
                public Int32 dwDefaultTTL;
                public Int32 dwInReceives;
                public Int32 dwInHdrErrors;
                public Int32 dwInAddrErrors;
                public Int32 dwForwDatagrams;
                public Int32 dwInUnknownProtos;
                public Int32 dwInDiscards;
                public Int32 dwInDelivers;
                public Int32 dwOutRequests;
                public Int32 dwRoutingDiscards;
                public Int32 dwOutDiscards;
                public Int32 dwOutNoRoutes;
                public Int32 dwReasmTimeout;
                public Int32 dwReasmReqds;
                public Int32 dwReasmOks;
                public Int32 dwReasmFails;
                public Int32 dwFragOks;
                public Int32 dwFragFails;
                public Int32 dwFragCreates;
                public Int32 dwNumIf;
                public Int32 dwNumAddr;
                public Int32 dwNumRoutes;
            };
            [StructLayout(LayoutKind.Sequential)]
            public struct MIB_IPADDRROW_W2K
            {
                public UInt32 dwAddr;
                public UInt32 dwIndex;
                public UInt32 dwMask;
                public UInt32 dwBCastAddr;
                public UInt32 dwReasmSize;
                public UInt16 unused1;
                public UInt16 unused2;
            };
            [StructLayout(LayoutKind.Sequential)]
            public struct MIB_IPFORWARDROW
            {
                public UInt32 dwForwardDest;
                public UInt32 dwForwardMask;
                public UInt32 dwForwardPolicy;
                public UInt32 dwForwardNextHop;
                public UInt32 dwForwardIfIndex;
                public UInt32 ForwardType;
                public UInt32 dwForwardProto;
                public UInt32 dwForwardAge;
                public UInt32 dwForwardNextHopAS;
                public UInt32 dwForwardMetric1;
                public UInt32 dwForwardMetric2;
                public UInt32 dwForwardMetric3;
                public UInt32 dwForwardMetric4;
                public UInt32 dwForwardMetric5;
            };
            [StructLayout(LayoutKind.Sequential)]
            public struct MIB_IPNETROW
            {
                [MarshalAs(UnmanagedType.U4)]
                public int dwIndex;
                [MarshalAs(UnmanagedType.U4)]
                public int dwPhysAddrLen;
                [MarshalAs(UnmanagedType.U1)]
                public byte mac0;
                [MarshalAs(UnmanagedType.U1)]
                public byte mac1;
                [MarshalAs(UnmanagedType.U1)]
                public byte mac2;
                [MarshalAs(UnmanagedType.U1)]
                public byte mac3;
                [MarshalAs(UnmanagedType.U1)]
                public byte mac4;
                [MarshalAs(UnmanagedType.U1)]
                public byte mac5;
                [MarshalAs(UnmanagedType.U1)]
                public byte mac6;
                [MarshalAs(UnmanagedType.U1)]
                public byte mac7;
                [MarshalAs(UnmanagedType.U4)]
                public int dwAddr;
                [MarshalAs(UnmanagedType.U4)]
                public int dwType;
            }

            /** Datos Grupo TCP */
            [StructLayout(LayoutKind.Sequential)]
            public struct MIB_TCPSTATS
            {
                public Int32 dwRtoAlgorithm;
                public Int32 dwRtoMin;
                public Int32 dwRtoMax;
                public Int32 dwMaxConn;
                public Int32 dwActiveOpens;
                public Int32 dwPassiveOpens;
                public Int32 dwAttemptFails;
                public Int32 dwEstabResets;
                public Int32 dwCurrEstab;
                public Int32 dwInSegs;
                public Int32 dwOutSegs;
                public Int32 dwRetransSegs;
                public Int32 dwInErrs;
                public Int32 dwOutRsts;

                public Int32 dwNumConns;
            };
            [StructLayout(LayoutKind.Sequential)]
            public struct MIB_TCPROW
            {
                public Int32 dwState;
                public UInt32 dwLocalAddr;
                public Int32 dwLocalPort;
                public UInt32 dwRemoteAddr;
                public Int32 dwRemotePort;
            };

            /** Datos Grupo UDP */
            [StructLayout(LayoutKind.Sequential)]
            public struct MIB_UDPSTATS
            {
                public Int32 dwInDatagrams;
                public Int32 dwNoPorts;
                public Int32 dwInErrors;
                public Int32 dwOutDatagrams;
                public Int32 dwNumAddrs;
            };
            [StructLayout(LayoutKind.Sequential)]
            public struct MIB_UDPROW
            {
                public Int32 dwLocalAddr;
                public Int32 dwLocalPort;
            }

            [DllImport("Iphlpapi.dll", SetLastError= true)]
            public static extern int GetIfTable(IntPtr pIfTable, ref int pdwSize, bool bOrder);

            [DllImport("iphlpapi.dll", SetLastError = true)]
            public static extern int GetIpStatistics(ref MIB_IPSTATS pStats);
            [DllImport("IpHlpApi.dll")]
            [return: MarshalAs(UnmanagedType.U4)]
            public static extern int GetIpAddrTable(
                IntPtr pIpNetTable,
                [MarshalAs(UnmanagedType.U4)] ref int pdwSize,
                bool bOrder);
            [DllImport("IpHlpApi.dll")]
            [return: MarshalAs(UnmanagedType.U4)]
            public static extern int GetIpNetTable(
                IntPtr pIpNetTable,
                [MarshalAs(UnmanagedType.U4)] ref int pdwSize,
                bool bOrder);
            [DllImport("IpHlpApi.dll")]
            [return: MarshalAs(UnmanagedType.U4)]
            public static extern int GetIpForwardTable(
                IntPtr pIpNetTable,
                [MarshalAs(UnmanagedType.U4)] ref int pdwSize,
                bool bOrder);

            [DllImport("IpHlpApi.dll", SetLastError = true, CharSet = CharSet.Auto)]
            public static extern int FreeMibTable(IntPtr plpNetTable);

            [DllImport("iphlpapi.dll", SetLastError = true)]
            public static extern int GetTcpStatistics(ref MIB_TCPSTATS pStats);
            [DllImport("IpHlpApi.dll")]
            [return: MarshalAs(UnmanagedType.U4)]
            public static extern int GetTcpTable(
                IntPtr pTcpTable,
                [MarshalAs(UnmanagedType.U4)] ref int pdwSize,
                bool bOrder);

            [DllImport("iphlpapi.dll", SetLastError = true)]
            public static extern int GetUdpStatistics(ref MIB_UDPSTATS pStats);
            [DllImport("IpHlpApi.dll")]
            [return: MarshalAs(UnmanagedType.U4)]
            public static extern int GetUdpTable(
                IntPtr pTcpTable,
                [MarshalAs(UnmanagedType.U4)] ref int pdwSize,
                bool bOrder);

            /// <summary>
            /// 
            /// </summary>
            /// <typeparam name="T"></typeparam>
            /// <param name="TableDataGet"></param>
            /// <returns></returns>
            public static List<T> Table<T>(TableDataGetDel TableDataGet) 
            {
                int bytesNeeded = 0;
                // The result from the API call.
                int result = TableDataGet(IntPtr.Zero, ref bytesNeeded, false);
                // Call the function, expecting an insufficient buffer.
                if (result != IpHlpApiHelper.ERROR_INSUFFICIENT_BUFFER)
                {
                    throw new Win32Exception(result);
                }
                // Allocate the memory, do it in a try/finally block, to ensure
                // that it is released.
                IntPtr buffer = IntPtr.Zero;
                // Try/finally.
                try
                {
                    // Allocate the memory.
                    buffer = Marshal.AllocCoTaskMem(bytesNeeded);
                    // Make the call again. If it did not succeed, then
                    // raise an error.
                    result = TableDataGet(buffer, ref bytesNeeded, true);
                    // If the result is not 0 (no error), then throw an exception.
                    if (result != 0)
                    {
                        throw new Win32Exception(result);
                    }
                    // Now we have the buffer, we have to marshal it. We can read
                    // the first 4 bytes to get the length of the buffer.
                    int entries = Marshal.ReadInt32(buffer);

                    // Increment the memory pointer by the size of the int.
                    IntPtr currentBuffer = new IntPtr(buffer.ToInt64() + Marshal.SizeOf(typeof(int)));

                    // Allocate an array of entries.
                    T[] table = new T[entries];

                    // Cycle through the entries.
                    for (int index = 0; index < entries; index++)
                    {
                        // Call PtrToStructure, getting the structure information.
                        table[index] = (T)Marshal.PtrToStructure(
                            new IntPtr(currentBuffer.ToInt64() + (index *
                           Marshal.SizeOf(typeof(T)))), typeof(T));
                    }
                    return table.OfType<T>().ToList();
                }
                finally
                {
                    // Release the memory.
                    IpHlpApiHelper.FreeMibTable(buffer);
                }
            }
        }

        public class IfData
        {
            static public List<IpHlpApiHelper.MIB_IFROW> IfTable
            {
                get
                {
                    if (LastIfTable == null || DateTime.Now - LastRead > TimeSpan.FromSeconds(10))
                    {
                        LastIfTable = IpHlpApiHelper.Table<IpHlpApiHelper.MIB_IFROW>(IpHlpApiHelper.GetIfTable);
                        LastRead = DateTime.Now;
                    }
                    return LastIfTable;
                }
            }

            static public int SnmpOperationalStatus(int input, int admin)
            {
                if (admin == 2)
                    return 6;
                else if (admin == 1)
                {
                    switch (input)
                    {
                        case 0:         // IF_OPER_STATUS_NON_OPERATIONAL
                            return 2;
                        case 1:         // IF_OPER_STATUS_UNREACHABLE
                            return 6;
                        case 2:         // IF_OPER_STATUS_DISCONNECTED 
                            return 6;
                        case 3:         // IF_OPER_STATUS_CONNECTING
                            return 1;
                        case 4:         // IF_OPER_STATUS_CONNECTED
                            return 1;
                        case 5:         // IF_OPER_STATUS_OPERATIONAL 
                            return 1;
                        default:
                            return 4;
                    }
                }
                else
                {
                    return 4;
                }
            }

            private static List<IpHlpApiHelper.MIB_IFROW> LastIfTable = null;
            private static DateTime LastRead = DateTime.Now;
        }

        public class IpData
        {
            static public IpHlpApiHelper.MIB_IPSTATS Statistics
            {
                get
                {
                    var stats = new IpHlpApiHelper.MIB_IPSTATS();
                    IpHlpApiHelper.GetIpStatistics(ref stats);
                    return stats;
                }
            }
            static public List<IpHlpApiHelper.MIB_IPADDRROW_W2K> AddrTable
            {
                get
                {
                    return IpHlpApiHelper.Table<IpHlpApiHelper.MIB_IPADDRROW_W2K>(IpHlpApiHelper.GetIpAddrTable);
                }
            }
            static public List<IpHlpApiHelper.MIB_IPFORWARDROW> RouteTable
            {
                get
                {
                    return IpHlpApiHelper.Table<IpHlpApiHelper.MIB_IPFORWARDROW>(IpHlpApiHelper.GetIpForwardTable);
                }
            }
            static public List<IpHlpApiHelper.MIB_IPNETROW> NetTable
            {
                get
                {
                    return IpHlpApiHelper.Table<IpHlpApiHelper.MIB_IPNETROW>(IpHlpApiHelper.GetIpNetTable);
                }
            }
        }
        public class TcpData
        {
            static public IpHlpApiHelper.MIB_TCPSTATS Statistics
            {
                get
                {
                    var stats = new IpHlpApiHelper.MIB_TCPSTATS();
                    IpHlpApiHelper.GetTcpStatistics(ref stats);
                    return stats;
                }
            }
            static public List<IpHlpApiHelper.MIB_TCPROW> TableData
            {
                get
                {
                    var data = IpHlpApiHelper.Table<IpHlpApiHelper.MIB_TCPROW>(IpHlpApiHelper.GetTcpTable);
                    return data;
                }
            }
        }
        public class UdpData
        {
            static public IpHlpApiHelper.MIB_UDPSTATS Statistics
            {
                get
                {
                    var stats = new IpHlpApiHelper.MIB_UDPSTATS();
                    IpHlpApiHelper.GetUdpStatistics(ref stats);
                    return stats;
                }
            }
            static public List<IpHlpApiHelper.MIB_UDPROW> TableData
            {
                get
                {
                    var data = IpHlpApiHelper.Table<IpHlpApiHelper.MIB_UDPROW>(IpHlpApiHelper.GetUdpTable);
                    return data;
                }
            }
        }
    }
}
