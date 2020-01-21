using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Diagnostics;

using System.Net;
using System.Net.Sockets;
using System.Net.NetworkInformation;
using System.Management;
using System.Dynamic;

using U5kManMibRevC;

namespace UnitTesting
{
    [TestClass]
    public class NetworkInformationTests
    {
        [TestMethod]
        public void GettingTcpInfo()
        {
            var tcpStats = MibIITableHelpers.TcpData.Statistics;
            var tcpTable = MibIITableHelpers.TcpData.TableData;
        }

        [TestMethod]
        public void GettingUdpInfo()
        {
            var udpStats = MibIITableHelpers.UdpData.Statistics;
            var udpTable = MibIITableHelpers.UdpData.TableData;
        }

        [TestMethod]
        public void GettingIpInfo()
        {
            var ipStats = MibIITableHelpers.IpData.Statistics;
            var ipAddTb = MibIITableHelpers.IpData.AddrTable;
            var ipRouTb = MibIITableHelpers.IpData.RouteTable;
            var ipNetTb = MibIITableHelpers.IpData.NetTable;
        }

        [TestMethod]
        public void InitialitingMib()
        {
            //Ed137RevCMib _mib = new Ed137RevCMib(
            //    new Func<object>(() => { return SnmpStatatics; }),
            //    new AgentDataGet(
            //        (toma) =>
            //        {
            //            toma(ServiceData);
            //        }), "");
        }       

        object SnmpStatatics
        {
            get
            {
                return new 
                {
                    SnmpInPkts = 0,
                    SnmpOutPkts = 0,
                    SnmpInBadVersions = 0,
                    SnmpInBadCommunityNames = 0,
                    SnmpInBadCommunityUses = 0,
                    SnmpInASNParseErrs = 0,
                    SnmpInTooBigs = 0,
                    SnmpInNoSuchNames = 0,
                    SnmpInBadValues = 0,
                    SnmpInReadOnlys = 0,
                    SnmpInGenErrs = 0,
                    SnmpInTotalReqVars = 0,
                    SnmpInTotalSetVars = 0,
                    SnmpInGetRequest = 0,
                    SnmpInGetNexts = 0,
                    SnmpInSetRequest = 0,
                    SnmpInGetResponses = 0,
                    SnmpInTraps = 0,
                    SnmpOutTooBigs = 0,
                    SnmpOutNoSuchNames = 0,
                    SnmpOutBadValues = 0,
                    SnmpOutGenErrs = 0,
                    SnmpOutGetRequests = 0,
                    SnmpOutGetNexts = 0,
                    SnmpOutSetRequests = 0,
                    SnmpOutGetResponses = 0,
                    SnmpOutTraps = 0,
                    SnmpEnableAuthenTraps = 2
                };
            }
        }

        object ServiceData
        {
            get
            {
                return new
                {
                    STDTOPS = new List<int>() { 0, 1, 2, 3, 4 }
                };
            }
        }

        [TestMethod]
        public void InterfacesGroup()
        {
            var number = NetworkInterface.GetAllNetworkInterfaces().Count();

            var nicNames = new List<object>();
            //var mc = new ManagementClass("Win32_NetworkAdapterConfiguration");
            var mc = new ManagementClass("Win32_NetworkAdapter");
            var moc = mc.GetInstances();

            foreach (var mo in moc)
            {
                var nicdata = new
                {
                    ifIndex = 0,
                    ifDescr = mo["Description"].ToString(),
                    ifTypeDesc = mo["AdapterType"],
                    ifType = mo["AdapterTypeID"],
                    ifMtu = 0,
                    ifSpeed = 0,
                    ifPhysAddress="",
                    ifAdminStatus=0,
                    ifOperStatus=0,
                    ifLastChange=0,
                    ifInOctets=0,
                    ifInUcastPkts=0,
                    ifInNUcastPkts=0,
                    ifInDiscards=0,
                    ifInErrors=0,
                    IfInUnknowProtos=0,
                    ifOutOctets = 0,
                    ifOutUcastPkts = 0,
                    ifOutNUcastPkts = 0,
                    ifOutDiscards = 0,
                    ifOutErrors = 0,
                    ifOutQLen=0,
                    ifSpecific=0
                };
                nicNames.Add(nicdata);
            }

        }

        [TestMethod]
        public void InterfacesGroup2()
        {
            ManagementObjectSearcher networkAdapterSearcher = new ManagementObjectSearcher("root\\cimv2", "select * from Win32_NetworkAdapter");
            ManagementObjectCollection objectCollection = networkAdapterSearcher.Get();

            var number = NetworkInterface.GetAllNetworkInterfaces().Count();
            var ifNumber = objectCollection.Count;

            var ifList = new List<object>();

            foreach (ManagementObject networkAdapter in objectCollection)
            {
                PropertyDataCollection networkAdapterProperties = networkAdapter.Properties;
                var nicdata = new
                {
                    ifIndex = 0,
                    ifDescr = networkAdapterProperties["Caption"].ToString(),
                    ifType =0,
                    ifTypeStr = networkAdapterProperties["Caption"].ToString(),
                    ifMtu = 0,
                    ifSpeed = 0,
                    ifPhysAddress = "",
                    ifAdminStatus = 0,
                    ifOperStatus = 0,
                    ifLastChange = 0,
                    ifInOctets = 0,
                    ifInUcastPkts = 0,
                    ifInNUcastPkts = 0,
                    ifInDiscards = 0,
                    ifInErrors = 0,
                    IfInUnknowProtos = 0,
                    ifOutOctets = 0,
                    ifOutUcastPkts = 0,
                    ifOutNUcastPkts = 0,
                    ifOutDiscards = 0,
                    ifOutErrors = 0,
                    ifOutQLen = 0,
                    ifSpecific = 0
                };
                ifList.Add(nicdata);
                //foreach (PropertyData networkAdapterProperty in networkAdapterProperties)
                //{
                //    Debug.WriteLine("Network adapter property name: {0}, ", networkAdapterProperty.Name);
                //    if (networkAdapterProperty.Value != null)
                //    {
                //        //Console.WriteLine("Network adapter property name: {0}", networkAdapterProperty.Name);
                //        //Console.WriteLine("Network adapter property value: {0}", networkAdapterProperty.Value);
                //        Debug.WriteLine("value: {0}", networkAdapterProperty.Value);
                //    }
                //}
                //Debug.WriteLine("---------------------------------------");
            }
        }

        [TestMethod]
        public void GetIfTable()
        {
            var iftable = MibIITableHelpers.IfData.IfTable;
            iftable.ForEach(row =>
            {
                var name = Encoding.Unicode.GetString(row.wszName);
                var desc = Encoding.ASCII.GetString(row.bDescr, 0, (int)row.dwDescrLen);
                var physAdd = row.bPhysAddr[0].ToString("X2") + '-' +
                        row.bPhysAddr[1].ToString("X2") + '-' +
                        row.bPhysAddr[2].ToString("X2") + '-' +
                        row.bPhysAddr[3].ToString("X2") + '-' +
                        row.bPhysAddr[4].ToString("X2") + '-' +
                        row.bPhysAddr[5].ToString("X2") + '-';
                Debug.WriteLine(desc ?? "null" + "\n");
            });
        }
    }
}
