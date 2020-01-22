#define _INTEGRATED_SNMP_CLIENT_
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;

using Lextm.SharpSnmpLib;
using Lextm.SharpSnmpLib.Messaging;
using Lextm.SharpSnmpLib.Objects;
using Lextm.SharpSnmpLib.Pipeline;

using Utilities;
using NucleoGeneric;

namespace U5kManServer
{
    /// <summary>
    /// 
    /// </summary>
    public class SnmpClient : BaseCode
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="message"></param>
        /// <param name="lineNumber"></param>
        /// <param name="caller"></param>
        void LogError(string message, 
            [System.Runtime.CompilerServices.CallerLineNumber] int lineNumber = 0, [System.Runtime.CompilerServices.CallerMemberName] string caller = null)
        {
            LogWarn<SnmpClient>(message, lineNumber, caller);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="x"></param>
        /// <param name="lineNumber"></param>
        /// <param name="caller"></param>
        void LogException(Exception x, 
            [System.Runtime.CompilerServices.CallerLineNumber] int lineNumber = 0, [System.Runtime.CompilerServices.CallerMemberName] string caller = null)
        {
            LogException<SnmpClient>("", x, false, false, lineNumber, caller);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="ip"></param>
        /// <param name="port"></param>
        /// <param name="community"></param>
        /// <param name="_data"></param>
        /// <param name="handler"></param>
        /// <param name="timeout"></param>
        /// <returns></returns>
        public bool GetSet(string ip, int port, string community, IList<Variable> _data, Action<IPEndPoint, IList<Variable>> handler, int timeout)
        {
            /** 20171017. Ignoro el PAR timeout y los sustituyo por Config timeout/reint por defecto 1000/2*/
            int reint = Properties.u5kManServer.Default.SnmpClientRequestReint;
            timeout = Properties.u5kManServer.Default.SnmpClientRequestTimeout;
            do
            {
                try
                {
#if !_INTEGRATED_SNMP_CLIENT_
                    IList<Variable> results = Messenger.Get(VersionCode.V2, new IPEndPoint(IPAddress.Parse(ip), port),
                                                new OctetString("public"), _data, timeout);
#else
                    IList<Variable> results = SnmpAgent.ClientRequest(SnmpType.GetRequestPdu,
                        VersionCode.V2, new IPEndPoint(IPAddress.Parse(ip), port), new OctetString("public"), _data, timeout);
#endif
                    if (results.Count != _data.Count)
                        throw new SnmpException(String.Format("SnmpClient.GetSet[{0}]: Invalid result.count", ip));

                    handler(new IPEndPoint(IPAddress.Parse(ip), port), results);
                    return true;
                }

                catch (Lextm.SharpSnmpLib.Messaging.TimeoutException)
                {
                    // Si es timeout continuo reintentos...
                }

                catch (Exception x)
                {
                    LogException(x);
                    throw x;
                }

            } while (--reint > 0);

            throw new SnmpException(String.Format("SnmpClient.GetSet[{0}]: No responde...", ip));
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="version"></param>
        /// <param name="endpoint"></param>
        /// <param name="community"></param>
        /// <param name="variables"></param>
        /// <param name="timeout"></param>
        /// <returns></returns>
        public IList<Variable> Get(VersionCode version, IPEndPoint endpoint, OctetString community, IList<Variable> variables, int timeout, int reint)
        {
            /** 20171017. Ignoro el PAR timeout y los sustituyo por Config timeout/reint por defecto 1000/2*/
            //int reint = Properties.u5kManServer.Default.SnmpClientRequestReint;
            //timeout = Properties.u5kManServer.Default.SnmpClientRequestTimeout;

            do
            {
                try
                {
#if !_INTEGRATED_SNMP_CLIENT_
                    IList<Variable> results = Messenger.Get(version, endpoint, community, variables, timeout);
#else
                    IList<Variable> results = SnmpAgent.ClientRequest(SnmpType.GetRequestPdu, 
                        version, endpoint, community, variables, timeout);
#endif
                    if (results.Count != variables.Count)
                        throw new SnmpException(string.Format("CienteSnmp.Get: result.count incorrecto [{0}]", endpoint));

                    return results;
                }

                catch (Lextm.SharpSnmpLib.Messaging.TimeoutException)
                {
                    // Si es timeout continuo reintentos...
                }

                catch (Exception x)
                {
                    LogException(x);
                    throw x;
                }

            } while (--reint > 0);

            throw new SnmpException(string.Format("CienteSnmp.Get: Elemento [{0}], No responde...", endpoint));
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="ip"></param>
        /// <param name="port"></param>
        /// <param name="community"></param>
        /// <param name="oid"></param>
        /// <param name="timeout"></param>
        /// <param name="_ret"></param>
        /// <returns></returns>
        public bool GetInt(string ip, int port, string community, string oid, int timeout, out int _ret)
        {
            /** 20171017. Ignoro el PAR timeout y los sustituyo por Config timeout/reint por defecto 1000/3*/
            int reint = Properties.u5kManServer.Default.SnmpClientRequestReint;
            timeout = Properties.u5kManServer.Default.SnmpClientRequestTimeout;
            do
            {
                List<Variable> result;
                List<Variable> lst;
                ObjectIdentifier ido;
                _ret = 0;
                try
                {
                    lst = new List<Variable>();
                    ido = new ObjectIdentifier(oid);

                    lst.Add(new Variable(ido));
#if !_INTEGRATED_SNMP_CLIENT_
                    result = (List<Variable>)Messenger.Get(
                                VersionCode.V2,
                                new IPEndPoint(IPAddress.Parse(ip), port),  // 161),
                                new OctetString(community),
                                lst, timeout);
#else
                    result = (List<Variable>)SnmpAgent.ClientRequest(SnmpType.GetRequestPdu,
                                VersionCode.V2,
                                new IPEndPoint(IPAddress.Parse(ip), port), 
                                new OctetString(community),
                                lst, timeout);
#endif

                    if (result.Count <= 0)
                    {
                        throw new SnmpException(string.Format("CienteSnmp.GetInt: result.count <= 0: {0}---{1}", ip, oid));
                    }

                    if (result[0].Id != ido)
                        throw new SnmpException(string.Format("CienteSnmp.GetInt: result[0].Id != ido: {0}---{1}", ip, oid));

                    /** Obtener de 'result' el estado */
                    if (int.TryParse(result[0].Data.ToString(), out _ret) == false)
                        throw new SnmpException(string.Format("CienteSnmp.GetInt: TryParse(result[0].Data.ToString(): {0}---{1}", ip, oid));

                    return true;
                }

                catch (Lextm.SharpSnmpLib.Messaging.TimeoutException)
                {
                    // Si es timeout continuo reintentos...
                }

                catch (Exception x)
                {
                    LogException(x);
                    return false;
                }

            } while (--reint > 0);

            return false;

        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="ip"></param>
        /// <param name="port"></param>
        /// <param name="community"></param>
        /// <param name="oid"></param>
        /// <param name="valor"></param>
        /// <param name="timeout"></param>
        public void SetInt(string ip, int port, string community, string oid, int valor, int timeout)
        {
            /** 20171016. Ignoro el PAR timeout y los sustituyo por Config timeout/reint por defecto 1000/3*/
            int reint = Properties.u5kManServer.Default.SnmpClientRequestReint;
            timeout = Properties.u5kManServer.Default.SnmpClientRequestTimeout;
            do
            {
                try
                {
                    List<Variable> lst = new List<Variable>();

                    lst.Add(new Variable(new ObjectIdentifier(oid), new Integer32(valor)));
#if !_INTEGRATED_SNMP_CLIENT_
                    var result = Messenger.Set(
                            VersionCode.V2,
                            new IPEndPoint(IPAddress.Parse(ip), port),
                            new OctetString(community),
                            lst, timeout);
#else
                    var result = SnmpAgent.ClientRequest(SnmpType.SetRequestPdu,
                            VersionCode.V2,
                            new IPEndPoint(IPAddress.Parse(ip), port),
                            new OctetString(community),
                            lst, timeout);
#endif
                    return;
                }

                catch (Lextm.SharpSnmpLib.Messaging.TimeoutException)
                {
                    // Si es timeout continuo reintentos...
                    int _val;
                    GetInt(ip, port, community, oid, timeout, out _val);
                    if (_val == valor)
                        return;
                }
                catch (Exception x)
                {
                    LogException(x);
                    throw x;
                }
            } while (--reint > 0);
            throw new SnmpException(string.Format("CienteSnmp.SetInt: Elemento {0}, No responde...", ip, oid));
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="ip"></param>
        /// <param name="oid"></param>
        /// <returns></returns>
        public string GetString(string ip, int port, string community, string oid, int timeout)
        {
            /** 20171017. Ignoro el PAR timeout y los sustituyo por Config timeout/reint por defecto 1000/3*/
            int reint = Properties.u5kManServer.Default.SnmpClientRequestReint;
            timeout = Properties.u5kManServer.Default.SnmpClientRequestTimeout;
            do
            {
                List<Variable> result;
                List<Variable> lst;
                ObjectIdentifier ido;

                try
                {
                    lst = new List<Variable>();
                    ido = new ObjectIdentifier(oid);

                    lst.Add(new Variable(ido));
                    /** Cambia la Frecuencia en el agente */
#if !_INTEGRATED_SNMP_CLIENT_
                    result = (List<Variable>)Messenger.Get(
                                VersionCode.V2,
                                new IPEndPoint(IPAddress.Parse(ip), port),
                                new OctetString(community),
                                lst, timeout);
#else
                    result = (List<Variable>)SnmpAgent.ClientRequest(SnmpType.GetRequestPdu,
                                VersionCode.V2,
                                new IPEndPoint(IPAddress.Parse(ip), port),
                                new OctetString(community),
                                lst, timeout);
#endif
                    if (result.Count <= 0)
                        throw new SnmpException(string.Format("CienteSnmp.GetString: result.count <= 0: {0}---{1}", ip, oid));

                    if (result[0].Id != ido)
                        throw new SnmpException(string.Format("CienteSnmp.GetString: result[0].Id != ido: {0}---{1}", ip, oid));

                    return result[0].Data.ToString();
                }

                catch (Lextm.SharpSnmpLib.Messaging.TimeoutException)
                {
                    // Si es timeout continuo reintentos...
                }

                catch (Exception x)
                {
                    LogException(x);
                    throw x;
                }

            } while (--reint > 0);

            throw new SnmpException(string.Format("SnmpClient.GetString: Elemento {0}, No responde...", ip, oid));
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="ip"></param>
        /// <param name="oid"></param>
        /// <param name="valor"></param>
        public void SetString(string ip, int port, string community, string oid, string valor, int timeout)
        {
            /** 20171017. Ignoro el PAR timeout y los sustituyo por Config timeout/reint por defecto 1000/3*/
            int reint = Properties.u5kManServer.Default.SnmpClientRequestReint;
            timeout = Properties.u5kManServer.Default.SnmpClientRequestTimeout;
            do
            {
                try
                {
                    List<Variable> lst = new List<Variable>();

                    lst.Add(new Variable(new ObjectIdentifier(oid), new OctetString(valor)));
#if !_INTEGRATED_SNMP_CLIENT_
                    var result = Messenger.Set(
                            VersionCode.V2,
                            new IPEndPoint(IPAddress.Parse(ip), port),
                            new OctetString(community),
                            lst, timeout);
#else
                    var result = SnmpAgent.ClientRequest(SnmpType.SetRequestPdu,
                            VersionCode.V2,
                            new IPEndPoint(IPAddress.Parse(ip), port),
                            new OctetString(community),
                            lst, timeout);
#endif
                    return;
                }

                catch (Lextm.SharpSnmpLib.Messaging.TimeoutException)
                {
                    // Si es timeout continuo reintentos...
                }

                catch (Exception x)
                {
                    LogException(x);
                    throw x;
                }

            } while (--reint > 0);

            throw new SnmpException(string.Format("SnmpClient.SetString: Elemento {0}, No responde...", ip, oid));
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="ipTo"></param>
        /// <param name="community"></param>
        /// <param name="oid"></param>
        public void TrapTo(string ipTo, int port, string community, string oid, string val)
        {
            List<Variable> lst = new List<Variable>();
            Variable var = new Variable(new ObjectIdentifier(oid), new OctetString(val));
            lst.Add(var);
#if !_INTEGRATED_SNMP_CLIENT_
            Messenger.SendTrapV2(0, VersionCode.V2,
                new IPEndPoint(IPAddress.Parse(ipTo), port),
                new OctetString(community),
                new ObjectIdentifier(oid),
                0, lst);
#else
            SnmpAgent.ClientRequest(SnmpType.TrapV2Pdu,
                VersionCode.V2,
                new IPEndPoint(IPAddress.Parse(ipTo), port),
                new OctetString(community),
                lst, 0, new ObjectIdentifier(oid));
#endif
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public int Integer(ISnmpData data)
        {
            return (data is Integer32) ? ((Integer32)data).ToInt32() : -1;
        }
    }
}
