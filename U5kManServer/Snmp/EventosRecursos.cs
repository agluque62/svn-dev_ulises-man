using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;

using Lextm.SharpSnmpLib;
//using Lextm.SharpSnmpLib.Pipeline;
//using Lextm.SharpSnmpLib.Messaging;
//using Lextm.SharpSnmpLib.Security;
//using Lextm.SharpSnmpLib.Objects;

namespace U5kManServer
{
    /// <summary>
    /// 
    /// </summary>
    public class EventosRecursos
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        protected static string GetOid(StringCollection collection, string id, string _default)
        {
            foreach (string s in collection)
            {
                string[] p = s.Split(':');
                if (p.Count() == 2 && p[0] == id)
                    return p[1];
            }

            return _default;
        }

        public static bool IsEventRadio(string oid)
        {
            return oid.StartsWith(GetOid(Properties.u5kManServer.Default.RadioOids, "S", ".1.1.200.3.1"));
        }

        public static bool IsEventLcen(string oid)
        {
            return oid.StartsWith(GetOid(Properties.u5kManServer.Default.LcenOids, "S", ".1.1.300.3.1"));
        }

        public static bool IsEventTlf(string oid)
        {
            return oid.StartsWith(GetOid(Properties.u5kManServer.Default.TelefOids, "S", ".1.1.400.3.1"));
        }

        public static bool IsEventAts(string oid)
        {
            return oid.StartsWith(GetOid(Properties.u5kManServer.Default.AtsOids, "S", ".1.1.500.3.1"));
        }        
    
        /// <summary>
        /// Automata de Eventos...
        /// </summary>
        public Dictionary<string, int> _oids;
        int _std=0;
        
        /// <summary>
        /// Resultados del automata...
        /// </summary>
        public int lastInci { get; set; }
        public object[] lastParameters { get; set; }
        

       /// <summary>
        /// Contruccion de la Incidencia
       /// </summary>
       /// <param name="gw"></param>
       /// <param name="port"></param>
       /// <param name="oid"></param>
       /// <param name="data"></param>
       /// <returns></returns>
        public bool AutomataEventos(stdPhGw gw, int port, string oid, ISnmpData data)
        {
            if (_oids != null)
            {
                if (_oids.ContainsKey(oid) == true)
                {
                    if (_std == _oids[oid])
                    {
                        if (_std == 1)
                            lastInci = (int)((Integer32)data).ToInt32();                        
                        else if (_std == 2)
                        {
                            object [] decodPar = DecodificaParametros(gw, port, lastInci, (string)((OctetString)data).ToString());

                            _std = 0;
                            return SetLastParameters(gw, lastInci, decodPar);
                        }
                        _std = _std == 2 ? 0 : _std + 1;
                    }
                    else
                    {
                        _std = 0;
                    }
                }
            }
            return false;
        }

        /// <summary>
        /// Decodificacion de Formato de incidencias
        /// </summary>
        /// <param name="gw"></param>
        /// <param name="port"></param>
        /// <param name="inci"></param>
        /// <param name="_trama"></param>
        /// <returns></returns>
        protected virtual object [] DecodificaParametros(stdPhGw gw, int port, int inci, string _trama)
        {
            string [] items = _trama.Split('|');
            return items.ToArray();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="decodPar"></param>
        protected virtual bool SetLastParameters(stdPhGw gw, int inci, object[] decodPar)
        {
            lastParameters = decodPar;
            return true;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="arr"></param>
        /// <param name="key"></param>
        /// <returns></returns>
        protected string FindParameter(object[] arr, string key)
        {
            object p1 = Array.Find(arr, e => e.ToString().StartsWith(key));
            return p1 == null ? key+"???" : ((string)p1).Substring(key.Length) ;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="str_if"></param>
        /// <returns></returns>
        protected Dictionary<itf, string> _str_if = new Dictionary<itf, string>()
        {
            {itf.rcRadio,"RD"},
            {itf.rcLCE,"LCE"},
            {itf.rcPpBC,"BC"},
            {itf.rcPpBL,"BL"},
            {itf.rcPpAB,"AB"},
            {itf.rcAtsN5,"N5"},
            {itf.rcAtsR2,"R2"},
            {itf.rcPpEM,"EM"},
            {itf.rcPpEMM,"EMM"},
        };
        protected string if2str(string str_if)
        {
            int result;
            if (int.TryParse(str_if, out result) == true)
            {
                itf itf = (itf)result;
                if (_str_if.ContainsKey(itf))
                    return _str_if[itf];
            }
            return str_if;
        }
    }

    /// <summary>
    /// 
    /// </summary>
    public class EventosRadio : EventosRecursos     // Todo. Eventos. Parámetros para Estadística....
    {
        public EventosRadio()
        {
            _oids = new Dictionary<string, int>()
            {
                {GetOid(Properties.u5kManServer.Default.RadioOids, "0", ".1.1.200.3.1.13.1"), 0},
                {GetOid(Properties.u5kManServer.Default.RadioOids, "1", ".1.1.200.3.1.17.1"), 1},
                {GetOid(Properties.u5kManServer.Default.RadioOids, "2", ".1.1.200.3.1.18.1"), 2}
            };
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="inci"></param>
        /// <param name="decodPar"></param>
        protected override bool SetLastParameters(stdPhGw gw, int inci, object[] decodPar)
        {
            List<string> lst = new List<string>();
            switch (inci)
            {
                case 2050:                      // SALIDA: PASARELA,RECURSO,DESTINO.
                case 2051:
                case 2052:
                case 2053:
                    lst.Add(gw.name);
                    lst.Add(FindParameter(decodPar, "RC="));
                    lst.Add(FindParameter(decodPar, "DS="));
                    break;

                case 2003:
                case 2004:
                default:
                    lst.Add(FindParameter(decodPar, "RC="));
                    return false;
            }

            lastParameters = lst.ToArray();
            return true;
        }

    }

    /// <summary>
    /// 
    /// </summary>
    public class EventosLC : EventosRecursos        // Todo. Eventos. Parámetros para Estadística....
    {
        /// <summary>
        /// 
        /// </summary>
        public EventosLC()
        {
            _oids = new Dictionary<string, int>()
            {
                {GetOid(Properties.u5kManServer.Default.LcenOids, "0", ".1.1.300.3.1.9.1"), 0},
                {GetOid(Properties.u5kManServer.Default.LcenOids, "1", ".1.1.300.3.1.13.1"), 1},
                {GetOid(Properties.u5kManServer.Default.LcenOids, "2", ".1.1.300.3.1.14.1"), 2}
            };
        }
                                                    // No Hay Eventos con parametros adicionales a los extraidos.
        /// <summary>
        /// 
        /// </summary>
        /// <param name="inci"></param>
        /// <param name="decodPar"></param>
        protected override bool SetLastParameters(stdPhGw gw, int inci, object[] decodPar)
        {
            List<string> lst = new List<string>();
            switch (inci)
            {
                case 2030:                      // SALIDA: RECURSO.
                case 2031:
                case 2032:
                case 2033:
                case 2012:
                    lst.Add(FindParameter(decodPar, "RC="));
                    break;
                case 2013:
                case 2014:
                default:
                    lst.Add(FindParameter(decodPar, "RC="));
                    return false;
            }

            lastParameters = lst.ToArray();
            return true;
        }

    }

    /// <summary>
    /// 
    /// </summary>
    public class EventosTLF : EventosRecursos
    {
        /// <summary>
        /// 
        /// </summary>
        public EventosTLF()
        {
            _oids = new Dictionary<string, int>()
            {
                {GetOid(Properties.u5kManServer.Default.TelefOids, "0", ".1.1.400.3.1.10.1"), 0},            
                {GetOid(Properties.u5kManServer.Default.TelefOids, "1", ".1.1.400.3.1.14.1"), 1},
                {GetOid(Properties.u5kManServer.Default.TelefOids, "2", ".1.1.400.3.1.15.1"), 2}
            };
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="inci"></param>
        /// <param name="decodPar"></param>
        protected override bool SetLastParameters(stdPhGw gw, int inci, object[] decodPar)
        {
            List<string> lst = new List<string>();
            switch (inci)
            {
                case 2040:                      // SALIDA: RECURSO, LINEA, RED, TIPO_INTERFACE, ACCESO
                case 2041:
                case 2042:
                case 2043:
                    lst.Add(FindParameter(decodPar, "RC="));                // RECURSO
                    lst.Add("---");                                         // LINEA
                    lst.Add(FindParameter(decodPar, "DS="));                // RED/DESTINO
                    lst.Add(if2str( FindParameter(decodPar, "IF=") ));      // TIPO de INTERFAZ
                    lst.Add("---");                                         // ACCESO
                    break;

                case 2005:
                case 2006:
                default:
                    lst.Add(FindParameter(decodPar, "RC="));
                    return false;
            }

            lastParameters = lst.ToArray();
            return true;
        }

    }

    /// <summary>
    /// 
    /// </summary>
    public class EventosATS : EventosRecursos
    {
        /// <summary>
        /// 
        /// </summary>
        public EventosATS()
        {
            _oids = new Dictionary<string, int>()
            {
                {GetOid(Properties.u5kManServer.Default.AtsOids, "0", ".1.1.500.3.1.13.1"), 0},            
                {GetOid(Properties.u5kManServer.Default.AtsOids, "1", ".1.1.500.3.1.17.1"), 1},
                {GetOid(Properties.u5kManServer.Default.AtsOids, "2", ".1.1.500.3.1.18.1"), 2}
            };
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="inci"></param>
        /// <param name="decodPar"></param>
        protected override bool SetLastParameters(stdPhGw gw, int inci, object[] decodPar)
        {
            List<string> lst = new List<string>();
            switch (inci)
            {
                case 2020:                      // SALIDA: RECURSO,TRONCAL,ORIGEN,PRIORIDAD, DESTINO
                case 2022:
                    lst.Add(FindParameter(decodPar, "RC="));
                    lst.Add(FindParameter(decodPar, "TRO="));
                    lst.Add(FindParameter(decodPar, "NO="));
                    lst.Add(FindParameter(decodPar, "PR="));
                    lst.Add(FindParameter(decodPar, "ND="));
                    break;

                case 2021:                      // SALIDA: RECURSO, TRONCAL.
                case 2023:
                    lst.Add(FindParameter(decodPar, "RC="));
                    lst.Add(FindParameter(decodPar, "TRO="));
                    break;

                case 2024:                      // SALIDA: PASARELA, RECURSO...
                case 2025:
                    lst.Add(gw.name);
                    lst.Add(FindParameter(decodPar, "RC="));
                    break;

                case 2009:
                case 2010:
                default:
                    return false;
            }

            lastParameters = lst.ToArray();
            return true;
        }

    }

}
