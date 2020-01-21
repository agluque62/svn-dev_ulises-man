using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;

using NLog;
using Lextm.SharpSnmpLib;
using Lextm.SharpSnmpLib.Messaging;
using Lextm.SharpSnmpLib.Objects;
using Lextm.SharpSnmpLib.Pipeline;

namespace Uv5kiTaHfManager.Helpers
{
    public static class SnmpClient
    {

        #region Generic Methods

        private static ISnmpData GetParam(SnmpType paramType, Object value = null)
        {
            switch (paramType)
            {
                case SnmpType.Integer32:
                    return new Lextm.SharpSnmpLib.Integer32(Convert.ToInt32(value));

                case SnmpType.OctetString:
                    if (null == value)
                        return new Lextm.SharpSnmpLib.OctetString(String.Empty);
                    else
                        return new Lextm.SharpSnmpLib.OctetString(value.ToString());

                case SnmpType.Gauge32:
                    return new Lextm.SharpSnmpLib.Gauge32(Convert.ToUInt32(value));

                case SnmpType.Counter32:
                    return new Lextm.SharpSnmpLib.Counter32(Convert.ToUInt32(value));

                case SnmpType.Counter64:
                    return new Lextm.SharpSnmpLib.Counter64(Convert.ToUInt64(value));

                default:
                    throw new NotImplementedException("SnmtType Parse Pending Development. Not valid right now.");
            }
        }

        private static object GetOne(
            string ip, string community, string oid, int timeout,
            Int32 port, VersionCode snmpVersion,
            SnmpType? param1Type, Object param1Value = null)
        {
            List<Variable> lst = new List<Variable>();
            ObjectIdentifier OID = new ObjectIdentifier(oid);

            ISnmpData paramValue = GetParam((SnmpType)param1Type, param1Value);
            lst.Add(
                new Variable(
                    OID,
                    paramValue));

            List<Variable> result = (
                List<Variable>)Messenger.Get(
                    snmpVersion,
                    new IPEndPoint(
                        IPAddress.Parse(ip),
                        port),
                    new OctetString(community),
                    lst,
                    timeout);

            if (result.Count <= 0)
                throw new SnmpException(string.Format("CienteSnmp.GetInt: result.count <= 0: {0}---{1}", ip, oid));

            if (result[0].Id != OID)
                throw new SnmpException(string.Format("CienteSnmp.GetInt: result[0].Id != ido: {0}---{1}", ip, oid));

            /** Obtener de 'result' el estado */
            int _ret = 0;
            if (int.TryParse(result[0].Data.ToString(), out _ret) == false)
                throw new SnmpException(string.Format("CienteSnmp.GetInt: TryParse(result[0].Data.ToString(): {0}---{1}", ip, oid));

            return _ret;
        }
        private static object SetOne(
            string ip, string community, string oid, int timeout,
            Int32 port, VersionCode snmpVersion,
            SnmpType param1Type, Object param1Value)
        {
            List<Variable> lst = new List<Variable>();

            ISnmpData paramValue = GetParam(param1Type, param1Value);

            lst.Add(
                new Variable(
                    new ObjectIdentifier(oid),
                    paramValue));

            return Messenger.Set(
                snmpVersion,
                new IPEndPoint(IPAddress.Parse(ip), port),
                new OctetString(community),
                lst,
                timeout);
        }

        #endregion

        #region Defined Type Methods

        public static int GetInt(string ip, string community, string oid, int timeout, Int32 port = 161, VersionCode snmpVersion = VersionCode.V2)
        {
            return Convert.ToInt32(
                GetOne(ip, community, oid, timeout, port, snmpVersion, SnmpType.Integer32));
        }
        public static void SetInt(string ip, string community, string oid, int valor, int timeout, Int32 port = 161, VersionCode snmpVersion = VersionCode.V2, Boolean tryGetInt = true)
        {
            try
            {
                SetOne(ip, community, oid, timeout, port, snmpVersion, SnmpType.Integer32, valor);
            }
            catch (SnmpException ex)
            {
                if (tryGetInt)
                {
                    int _val = SnmpClient.GetInt(ip, community, oid, timeout);
                    if (_val == valor)
                        return;
                }

                throw ex;
            }
        }

        public static UInt32 GetGauge32(string ip, string community, string oid, int timeout, Int32 port = 161, VersionCode snmpVersion = VersionCode.V1)
        {
            return Convert.ToUInt32(
                GetOne(ip, community, oid, timeout, port, snmpVersion, SnmpType.Gauge32));
        }
        public static void SetGauge32(string ip, string community, string oid, UInt32 valor, int timeout, Int32 port = 161, VersionCode snmpVersion = VersionCode.V1)
        {
            SetOne(ip, community, oid, timeout, port, snmpVersion, SnmpType.Gauge32, valor);
        }

        public static UInt32 GetCounter32(string ip, string community, string oid, int timeout, Int32 port = 161, VersionCode snmpVersion = VersionCode.V1)
        {
            return Convert.ToUInt32(
                GetOne(ip, community, oid, timeout, port, snmpVersion, SnmpType.Counter32));
        }
        public static void SetCounter32(string ip, string community, string oid, UInt32 valor, int timeout, Int32 port = 161, VersionCode snmpVersion = VersionCode.V1)
        {
            SetOne(ip, community, oid, timeout, port, snmpVersion, SnmpType.Counter32, valor);
        }

        public static UInt64 GetCounter64(string ip, string community, string oid, int timeout, Int32 port = 161, VersionCode snmpVersion = VersionCode.V1)
        {
            return Convert.ToUInt64(
                GetOne(ip, community, oid, timeout, port, snmpVersion, SnmpType.Counter64));
        }
        public static void SetCounter64(string ip, string community, string oid, UInt64 valor, int timeout, Int32 port = 161, VersionCode snmpVersion = VersionCode.V1)
        {
            SetOne(ip, community, oid, timeout, port, snmpVersion, SnmpType.Counter64, valor);
        }

        public static string GetString(string ip, string community, string oid, int timeout, Int32 port = 161, VersionCode snmpVersion = VersionCode.V1)
        {
            List<Variable> lst = new List<Variable>();
            ObjectIdentifier ido = new ObjectIdentifier(oid);

            lst.Add(new Variable(ido, new OctetString("")));

            /** Cambia la Frecuencia en el agente */
            List<Variable> result = (
                List<Variable>)Messenger.Get(
                    snmpVersion,
                    new IPEndPoint(IPAddress.Parse(ip), port),
                    new OctetString(community),
                    lst, 
                    timeout);

            if (result.Count <= 0)
                throw new SnmpException(string.Format("CienteSnmp.GetString: result.count <= 0: {0}---{1}", ip, oid));

            if (result[0].Id != ido)
                throw new SnmpException(string.Format("CienteSnmp.GetString: result[0].Id != ido: {0}---{1}", ip, oid));

            return result[0].Data.ToString();
        }
        public static void SetString(string ip, string community, string oid, string valor, int timeout, Int32 port = 161, VersionCode snmpVersion = VersionCode.V1)
        {
            SetOne(ip, community, oid, timeout, port, snmpVersion, SnmpType.OctetString, valor);
        }

        public static void TrapTo(string ipTo, string community, string oid, string val, Int32 port = 162, VersionCode snmpVersion = VersionCode.V2)
        {
            List<Variable> lst = new List<Variable>();
            Variable var = new Variable(new ObjectIdentifier(oid), new OctetString(val));
            lst.Add(var);

            Messenger.SendTrapV2(
                0,
                snmpVersion,                     
                new IPEndPoint(
                    IPAddress.Parse(ipTo),
                    port),
                new OctetString(community),                   
                new ObjectIdentifier(oid),                    
                0, 
                lst);
        }
        
        #endregion

    }
}
