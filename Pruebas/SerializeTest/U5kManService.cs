using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using System.Threading;
using System.Threading.Tasks;

using System.Runtime.Serialization;
#if !_TESTING_
using System.ServiceModel;
#endif

using System.Net;
using System.Net.Sockets;
using System.Net.NetworkInformation;
using System.Runtime.InteropServices;
using System.Media;

namespace U5kManServer
{
    /// <summary>
    /// 
    /// </summary>
    public enum std 
    { 
        NoInfo = 0, 
        Ok = 1, 
        AvisoReconocido = 2, 
        AlarmaReconocida = 3, 
        Aviso = 4, 
        Alarma = 5, 
        Error = 6,
        Principal = 7,
        Reserva = 8,
        NoExiste = 9,
        Inicio = -1
    }

    /// <summary>
    /// 
    /// </summary>
    public enum sel
    {
        NoInfo = 0,
        Seleccionado = 1,
        NoSeleccionado = 2
    }

    /// <summary>
    /// 
    /// </summary>
    public enum itf 
    {
        rcRadio = 0, 
        rcLCE = 1, 
        rcPpBL = 3, 
        rcPpBC = 2, 
        rcPpAB = 4, 
        rcPpEM = 13, 
        rcPpEMM = 51, 
        rcAtsR2 = 5, 
        rcAtsN5 = 6, 
        rcNotipo = -1 
    }

    /// <summary>
    /// 
    /// </summary>
    public enum trc { rcRadio = 2, rcLCE = 3, rcTLF = 4, rcATS = 5, rcNotipo=-1 }

    [DataContract]
    public class StdServ
    {
        [DataMember]
        public sel Seleccionado { get; set; }
        [DataMember]
        public std Estado { get; set; }
        [DataMember]
        public string name { get; set; }
#if _LISTANBX_
        public int timer { get; set; }
#endif
        [DataMember]
        public string ntp_sync { get; set; }
        [DataMember]
        public Dictionary<string, std> lanes { get; set; }

        /// <summary>
        /// 20170705. Almacena los detalles de Version SW de los Servidores.
        /// </summary>
        [DataMember]
        public string jversion { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public StdServ()
        {
            Seleccionado = sel.NoInfo;
            Estado = std.NoInfo;
            name = "name?";
            ntp_sync = "???";
            lanes = new Dictionary<string, std>();
            jversion = "";
        }
        /// <summary>
        /// 
        /// </summary>
        public string lanes2string
        {
            get
            {
                StringBuilder builder = new StringBuilder(); 
                foreach(KeyValuePair<string, std> pair in lanes)
                {
                    builder.Append(pair.Key).Append(":").Append(((int)pair.Value).ToString()).Append(';');
                }
                return builder.ToString().TrimEnd(';');
            }
        }
        /// <summary>
        /// 
        /// </summary>
        public string string2lanes
        {
            set
            {
                lanes.Clear();
                string[] tokens = value.Split(new char[] { ':', ';' }, StringSplitOptions.RemoveEmptyEntries);
                for (int i = 0; i < tokens.Length; i += 2)
                {
                    string name = tokens[i];
                    int Estado = int.Parse(tokens[i + 1]);
                    lanes[name] = (std )Estado;
                }
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="from"></param>
        public void CopyFrom(StdServ from)
        {
            Seleccionado = from.Seleccionado;
            Estado = from.Estado;
            name = from.name;
            ntp_sync = from.ntp_sync;
            lanes = from.lanes.ToDictionary(x => x.Key, x => x.Value);
            jversion = from.jversion;
            timer = from.timer;
        }
        public bool Equals(StdServ other)
        {
            bool retorno = (
                Seleccionado == other.Seleccionado &&
                Estado == other.Estado &&
                name == other.name &&
                ntp_sync == other.ntp_sync &&
                // lanes == other.lanes &&
                jversion == other.jversion &&
                timer == other.timer
                );
            return retorno;
        }
    }

    [DataContract]
#if _TESTING_
    public class U5KStdGeneral
#else
    public class U5KStdGeneral : BaseCode
#endif
    {
        public enum ClusterErrors 
        { 
            NoError = 0, 
            NoLocalServerNameDetected = 1, 
            RepeatedServerNamesDetected = 2, 
            NoMainNodeDetected=3, 
            NoActiveNodesDetected=4, 
            AllNodesAreMain=5 
        }
        /// <summary>
        /// Parametros de configuracion
        /// </summary>
        [DataMember]
        public bool bDualServ = false;
        [DataMember]
        public bool bDualScv = false;
        [DataMember]
        public bool HaySacta = false;
        [DataMember]
        public bool HayReloj = false;
        [DataMember]
        public bool HayPabx = true;

        /// <summary>
        /// Parámetros de Estado...
        /// </summary>
        [DataMember]
        public ClusterErrors ClusterError { get; set; }
        [DataMember]
        public StdServ stdServ1 = new StdServ() { Estado = std.NoInfo, Seleccionado = sel.NoInfo, name="" };
        [DataMember]
        public StdServ stdServ2 = new StdServ() { Estado = std.NoInfo, Seleccionado = sel.NoInfo, name="" };
        [DataMember]
        public StdServ stdScv1 = new StdServ() { Estado = std.NoInfo, Seleccionado = sel.NoInfo, name="" };
        [DataMember]
        public StdServ stdScv2 = new StdServ() { Estado = std.NoInfo, Seleccionado = sel.NoInfo, name="" };
        [DataMember]
        public StdServ stdClock = new StdServ() { Estado = std.Inicio, Seleccionado = sel.NoInfo, name = "" };
        [DataMember]
        public std stdSacta1 = std.NoInfo;
        [DataMember]
        public std stdSacta2 = std.NoInfo;
        [DataMember]
        public std stdGlobalPos = std.NoInfo;
        [DataMember]
        public std stdGlobalGw1 = std.NoInfo;
        [DataMember]
        public std stdGlobalGw2 = std.NoInfo;
#if _LISTANBX_V0
        [DataMember]
        public List<StdServ> lstNbx = new List<StdServ>();
#elif _LISTANBX_
        public enum NbxServiceState { Parado = 2, Slave = 0, Master = 1, NoIni = 3 };
        [DataContract]
        public class StdNbx
        {
            public string ip { get; set; }
            public int webport { get; set; }
            public bool Running { get; set; }
            public NbxServiceState CfgService { get; set; }
            public NbxServiceState RadioService { get; set; }
            public NbxServiceState TifxService { get; set; }
            public NbxServiceState PabxService { get; set; }
            public int timer { get; set; }
        };
        [DataMember]
        public List<StdNbx> lstNbx = new List<StdNbx>();
#else
        public StdServ stdNbx = new StdServ() { Estado = std.NoInfo, Seleccionado = sel.NoInfo, name = "???" };
#endif
        [DataMember]
        public StdServ stdPabx = new StdServ() { Estado = std.Inicio, name = "???", Seleccionado = sel.NoInfo };

        /// <summary>
        /// 
        /// </summary>
        [DataMember]
        public string cfgVersion = string.Empty;
        [DataMember]
        public string cfgName = string.Empty;
        public string CfgId
        {
            get
            {
                return cfgName + " (" + cfgVersion + ")";
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public StdServ LocalServer
        {
            get
            {
                string MyName = System.Environment.MachineName;
                if (MyName == stdServ1.name)
                    return stdServ1;
                else if (MyName == stdServ2.name)
                    return stdServ2;
                else
                {
#if !_TESTING_
                    LogError<U5KStdGeneral>(System.Reflection.MethodBase.GetCurrentMethod().Name,
                        String.Format("No se determina si soy Servidor-1 o Servidor-2: {0} ?? ({1})-({2}). Se asume SERV1",
                        System.Environment.MachineName, stdServ1.name, stdServ2.name));
#endif
                    return stdServ1;
                }
            }
        }
        public StdServ RemoteServer
        {
            get
            {
                string MyName = System.Environment.MachineName;
                if (MyName == stdServ1.name)
                    return stdServ2;
                else if (MyName == stdServ2.name)
                    return stdServ1;
                else
                    return null;
            }
        }
        public void Init()
        {
            stdServ1 = new StdServ() { Estado = std.NoInfo, Seleccionado = sel.NoInfo };
            stdServ2 = new StdServ() { Estado = std.NoInfo, Seleccionado = sel.NoInfo };
        }

        /// <summary>
        /// 
        /// </summary>
        public U5KStdGeneral()
        {
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="from"></param>
        public U5KStdGeneral(U5KStdGeneral from)
        {
            CopyFrom(from);
        }

        public void CopyFrom(U5KStdGeneral from)
        {
            /** Parte de Configuracion */
            bDualServ = from.bDualServ;
            bDualScv = from.bDualScv;
            HaySacta = from.HaySacta;
            HayReloj = from.HayReloj;
            HayPabx = from.HayPabx;

            /** */
            cfgVersion = from.cfgVersion;
            cfgName = from.cfgName;

            /** */
            stdServ1.CopyFrom(from.stdServ1);
            stdServ2.CopyFrom(from.stdServ2);
            stdScv1.CopyFrom(from.stdScv1);
            stdScv2.CopyFrom(from.stdScv2);
            stdClock.CopyFrom(from.stdClock);
            stdPabx.CopyFrom(from.stdPabx);

            stdSacta1 = from.stdSacta1;
            stdSacta2 = from.stdSacta2;
            stdGlobalPos = from.stdGlobalPos;
            stdGlobalGw1 = from.stdGlobalGw1;
            stdGlobalGw2 = from.stdGlobalGw2;

            /** */
            lstNbx = from.lstNbx.Select(nbx => new StdNbx() 
            {
                ip = nbx.ip,
                webport = nbx.webport,
                Running = nbx.Running,
                CfgService = nbx.CfgService,
                RadioService = nbx.RadioService,
                TifxService = nbx.TifxService,
                PabxService = nbx.PabxService,
                timer = nbx.timer
            }).ToList();
        }

        public bool Equals(U5KStdGeneral other)
        {
            bool retorno = (
                bDualServ == other.bDualServ &&
                bDualScv == other.bDualScv &&
                HaySacta == other.HaySacta &&
                HayReloj == other.HayReloj &&
                HayPabx == other.HayPabx &&
                cfgVersion == other.cfgVersion &&
                cfgName == other.cfgName &&

                stdServ1.Equals(other.stdServ1) &&
                stdServ2.Equals(other.stdServ2) &&
                stdScv1.Equals(other.stdScv1) &&
                stdScv2.Equals(other.stdScv2) &&
                stdClock.Equals(other.stdClock) &&
                stdPabx.Equals(other.stdPabx) &&

                stdSacta1 == other.stdSacta1 &&
                stdSacta2 == other.stdSacta2 &&
                stdGlobalPos == other.stdGlobalPos &&
                stdGlobalGw1 == other.stdGlobalGw1 &&
                stdGlobalGw2 == other.stdGlobalGw2
                );
            return retorno;
        }

    }

    [DataContract]
    public class stdPos 
    {
        [DataMember]
        public string name { get; set; }    // = "";
        [DataMember]
        public string ip { get; set; }      // = "192.168.1.1";
        public int snmpport { get; set; }   // = 261;
        [DataMember]
        public std stdg { get; set; }       // 
        [DataMember]
        public std stdpos { get; set; }     // = std.NoInfo;
        [DataMember]
        public std panel { get; set; }      // = std.NoInfo;
        [DataMember]
        public std jack_exe { get; set; }   // = std.NoInfo;
        [DataMember]
        public std jack_ayu { get; set; }   // = std.NoInfo;
        [DataMember]
        public std alt_r { get; set; }      // = std.NoInfo;
        [DataMember]
        public std alt_t { get; set; }      // = std.NoInfo;
        [DataMember]
        public std lan1{ get; set; }        // = std.NoInfo;
        [DataMember]
        public std lan2{ get; set; }        // = std.NoInfo;
        [DataMember]
        public std alt_hf { get; set; }     // = std.NoInfo
        [DataMember]
        public std rec_w { get; set; }    // = std.NoInfo,

        /// <summary>
        /// 20170309. AGL. Nuevas peticiones para la MIB
        /// </summary>
        [DataMember]
        public string status_sync { get; set; }
        [DataMember]
        public List<string> uris { get; set; }

        /// <summary>
        /// 20170309. AGL. Obtención del Detalle de la Version sofware.
        /// </summary>
        [DataMember]
        public string sw_version { get; set; }

        // public int _polling = 0;            // Para Hacer polling mas rápido a las activas...
        // public Thread thExplore = null;

        /// <summary>
        /// 20171026. Para ajustar los reintentos a si está o no está presente...
        /// </summary>
        public int SnmpTimeout
        {
            get
            {
#if !_TESTING_
                return Properties.u5kManServer.Default.SnmpClientRequestTimeout;
#else
                return 1000;
#endif
            }
        }
        public int SnmpReintentos
        {
            get
            {
#if !_TESTING_
                return stdg == std.NoInfo ? 1 : Properties.u5kManServer.Default.SnmpClientRequestReint;
#else
                return 1;
#endif
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public stdPos()
        {
            name = "";
            ip = "192.168.1.1";
            snmpport = 261;

            lan1 = lan2 = panel = jack_exe = jack_ayu = alt_r = alt_t = alt_hf = rec_w = std.NoInfo;
            stdpos = std.NoInfo; 
            stdg = stdGlobal();
            status_sync = "???";
            uris = new List<string>();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="last"></param>
        public stdPos(stdPos last) : this()
        {
            if (last != null)
            {
                lan1 = last.lan1;
                lan2 = last.lan2;
                panel = last.panel;
                jack_exe = last.jack_exe;
                jack_ayu = last.jack_ayu;
                alt_r = last.alt_r;
                alt_t = last.alt_t;
                alt_hf = last.alt_hf;
                rec_w = last.rec_w;
                stdpos = last.stdpos;
                status_sync = last.status_sync;
                uris = last.uris;
                sw_version = last.sw_version;

                stdg = stdGlobal();
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public void Reset()
        {
            lan1 = lan2 = panel = jack_exe = jack_ayu = alt_r = alt_t = alt_hf = rec_w = std.NoInfo;
        }

        /** */
        public std stdGlobal()
        {
            if (stdpos == std.NoInfo)
                return std.NoInfo;
#if !_TESTING_
            else if (U5kManServer.Properties.u5kManServer.Default.HayAltavozHF)
#else
            else if (false)
#endif
            {
                if (alt_r == std.NoInfo || 
                    alt_t == std.NoInfo || 
//                  panel == std.NoInfo || 
                    lan1 != std.Ok || 
                    lan2 != std.Ok || 
                    rec_w == std.NoInfo || 
                    alt_hf == std.NoInfo)
                    return std.Error;
            }
            else
            {
                if (alt_r == std.NoInfo || 
                    alt_t == std.NoInfo || 
//                  panel == std.NoInfo || 
                    lan1 != std.Ok || 
                    lan2 != std.Ok || 
                    rec_w == std.NoInfo )
                    return std.Error;
            }
            return std.Ok;
        }

        /** 20171023. Copy */
        public void CopyFrom(stdPos from)
        {
            name = from.name;
            ip = from.ip;
            snmpport = from.snmpport;

            lan1 = from.lan1;
            lan2 = from.lan2;
            panel = from.panel;
            jack_exe = from.jack_exe;
            jack_ayu = from.jack_ayu;
            alt_r = from.alt_r;
            alt_t = from.alt_t;
            alt_hf = from.alt_hf;
            rec_w = from.rec_w;
            stdpos = from.stdpos;
            status_sync = from.status_sync;
            uris = from.uris;
            sw_version = from.sw_version;

            stdg = stdGlobal();
        }

        public bool Equals(stdPos other)
        {
            return other == null ? false : (name == other.name && ip == other.ip);
        }
    }

    /// <summary>
    /// 
    /// </summary>
    [DataContract]
    public class stdRec
    {
        [DataMember]
        public string name = "";
        [DataMember]
        public itf tipo = itf.rcNotipo;             // Tipo en Base de Datos.
        [DataMember]
        public trc tipo_online = trc.rcNotipo;      // Tipo Notificado.
        [DataMember]
        public itf tipo_itf = itf.rcNotipo;         // Tipo de Interfaz o subtipo.
        [DataMember]
        public bool bdt = false;                    // Estado en la Base de Datos.
        [DataMember]
        public bool presente = false;               // Hardware insertado.
        [DataMember]
        public std std_online = std.NoInfo;         // Estado Notificado.
        [DataMember]
        public string bdt_name = "";
        public int snmp_port = 161;
        public int snmp_trap_port = 162;

        /// <summary>
        /// 
        /// </summary>
        public bool Errores
        {
            get
            {                
                if (!presente)
                {
                    return bdt == false ? false : true;
                }
                else
                {
                    if ( (bdt == false && tipo_online != trc.rcNotipo) ||
                         (bdt == true && TestInterface()==false) )
//                    if (bdt == false || TestInterface()==false)
                    {
                        /** Conflicto el Base de datos.. */
                        // name = string.Format("Conflicto: {0}", tipo_online);
                        std_online = std.Error;
                        return true;
                    }

                    return bdt==false ? false : std_online == std.Ok ? false : true;
                }                
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        Dictionary<itf, trc> _testItfDic = new Dictionary<itf, trc>
        {
            {itf.rcRadio, trc.rcRadio}, {itf.rcLCE, trc.rcLCE},
            {itf.rcPpAB, trc.rcTLF}, {itf.rcPpBC, trc.rcTLF}, {itf.rcPpBL, trc.rcTLF}, {itf.rcPpEM, trc.rcTLF}, {itf.rcPpEMM, trc.rcTLF},
            {itf.rcAtsR2, trc.rcATS}, {itf.rcAtsN5, trc.rcATS}
        };
        bool TestInterface()
        {
            if (_testItfDic.ContainsKey(tipo) == true)
            {
                if (_testItfDic[tipo] == tipo_online)
                    return true;
            }
            return false;
        }

        /// <summary>
        /// 
        /// </summary>
        public void Reset()
        {
            tipo_online = trc.rcNotipo;
            presente = false;
            std_online = std.NoInfo;
            name = bdt_name;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="from"></param>
        public void CopyFrom(stdRec from)
        {
            name = from.name;
            tipo = from.tipo;
            tipo_online = from.tipo_online;
            tipo_itf = from.tipo_itf;
            bdt = from.bdt;
            presente = from.presente;
            std_online = from.std_online;
            bdt_name = from.bdt_name;
            snmp_port = from.snmp_port;
            snmp_trap_port = from.snmp_trap_port;
        }

        public bool Equals(stdRec other)
        {
            return tipo == other.tipo && bdt == other.bdt;
        }
    }

    /// <summary>
    /// 
    /// </summary>
    [DataContract]
    public class stdSlot
    {
        [DataMember]
        public std std_cfg = std.NoInfo;
        [DataMember]
        public std std_online = std.NoInfo;
        [DataMember]
        public stdRec[] rec = new stdRec[4] { new stdRec(), new stdRec(), new stdRec(), new stdRec() };

        /// <summary>
        /// 
        /// </summary>
        public bool Errores
        {
            get
            {
                if (std_cfg != std_online)
                    return true;

                foreach (stdRec rc in rec)
                {
                    if (rc.Errores == true)
                        return true;
                }
                return false;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public void Reset()
        {
            std_online = std.NoInfo;
            foreach (stdRec r in rec)
            {
                r.Reset();
            }
        }

        public void CopyFrom(stdSlot from)
        {
            std_cfg = from.std_cfg;
            std_online = from.std_online;
            for (int nr = 0; nr < 4; nr++)
            {
                rec[nr].CopyFrom(from.rec[nr]);
            }
        }

        public bool Equals(stdSlot other)
        {
            return std_cfg == other.std_cfg && rec[0].Equals(other.rec[0]) && rec[1].Equals(other.rec[1]) && rec[2].Equals(other.rec[2]) && rec[3].Equals(other.rec[3]);
        }
    }

    /// <summary>
    /// 
    /// </summary>
    [DataContract]
    public class stdPhGw
    {
        public stdPhGw()
        {
            lan1 = std.NoInfo;
            lan2 = std.NoInfo;
            version = string.Empty;
            std = std.NoInfo;
            presente = false;
            Seleccionada = false;
        }
        public stdPhGw(stdPhGw last)
        {
            if (last == null)
            {
                lan1 = std.NoInfo;
                lan2 = std.NoInfo;
                version = string.Empty;
                std = std.NoInfo;
                presente = false;
                Seleccionada = false;
            }
            else
            {
                lan1 = last.lan1;
                lan2 = last.lan2;
                version = last.version;
                std = last.std;
                presente = last.presente;
                Seleccionada = last.Seleccionada;
                for (int islot = 0; islot < 4; islot++)
                {
                    slots[islot].std_online = last.slots[islot].std_online;
                }
            }
        }

        [DataMember]
        public int snmpport { get; set; }
        /// <summary>
        /// 20171026. Para ajustar los reintentos a si está o no está presente...
        /// </summary>
        public int SnmpTimeout
        {
            get
            {
#if !_TESTING_
                return Properties.u5kManServer.Default.SnmpClientRequestTimeout;
#else
                return 1000;
#endif
            }
        }
        public int SnmpReintentos
        {
            get
            {
#if !_TESTING_
                return presente ? Properties.u5kManServer.Default.SnmpClientRequestReint : 1;
#else
                return 1;
#endif
            }
        }

        //public int _polling { get; set; }

        [DataMember]
        public string name { get; set; }
        [DataMember]
        public string ip { get; set; }
        [DataMember]
        public std std { get; set; }
        [DataMember]
        public bool presente { get; set; }

        [DataMember]
        public stdSlot[] slots = new stdSlot[4] { new stdSlot(), new stdSlot(), new stdSlot(), new stdSlot() };

        [DataMember]
        public std lan1 { get; set; }
        [DataMember]
        public std lan2 { get; set; }

        [DataMember]
        public bool Seleccionada { get; set; }  // 
        [DataMember]
        public string version { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public bool Errores
        {
            get
            {
                foreach (stdSlot slot in slots)
                {
                    if (slot.Errores == true)
                        return true;
                }
                return false;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public void Reset()
        {
            lan1 = std.NoInfo;
            lan2 = std.NoInfo;
            Seleccionada = false;
            presente = false;

            foreach (stdSlot slot in slots)
            {
                slot.Reset();
            }

            version = string.Empty;
        }

        /// <summary>
        /// 
        /// </summary>
        public void RecInit()
        {
            for (int iSlot = 0; iSlot < 4; iSlot++)
            {
                for (int iRec = 0; iRec < 4; iRec++)
                {
                    slots[iSlot].rec[iRec].snmp_port = 16100 + (10 * (iSlot + 1)) + (iRec + 1);
                }
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="from"></param>
        public void CopyFrom(stdPhGw from)
        {
            snmpport = from.snmpport;
            name = from.name;
            ip = from.ip;
            std = from.std;
            presente = from.presente;
            lan1 = from.lan1;
            lan2 = from.lan2;
            Seleccionada = from.Seleccionada;
            version = from.version;
            for (int ns = 0; ns < 4; ns++)
            {
                slots[ns].CopyFrom(from.slots[ns]);
            }
        }

        public bool Equals(stdPhGw other)
        {
            return name == other.name && ip == other.ip && slots[0].Equals(other.slots[0]) && slots[1].Equals(other.slots[1]) && slots[2].Equals(other.slots[2]) && slots[3].Equals(other.slots[3]);
        }
    }

    /// <summary>
    /// 
    /// </summary>
    [DataContract]
    public class stdGw
    {
        public stdGw(stdGw last)
        {
            //std = last==null ? std.NoInfo : last.std;
            //presente = last==null ? false : last.presente;
            if (last != null)
                CopyFrom(last);
        }

        [DataMember]
        public string name { get; set; }
        [DataMember]
        public string ip { get; set; }
        [DataMember]
        public std std { get; set; }
        [DataMember]
        public bool presente { get; set; }
        [DataMember]
        public bool Dual { get; set; }         // Para la Dualidad de Pasarela...
        [DataMember]
        public stdPhGw gwA = new stdPhGw();
        [DataMember]
        public stdPhGw gwB = new stdPhGw();

        /****/
        [DataContract]
        public class RemoteNtpClientStatus
        {
            [DataMember]
            public List<string> lines { get; set; }
        }
        [DataMember]
        public List<string> _ntp_client_status = new List<string>();
        public List<string> ntp_client_status
        {
            get
            {
                return _ntp_client_status;
            }
            set
            {
#if !_TESTING_
                List<string> Value = NormalizeNtpStatusList(value);
                _ntp_client_status.Clear();

                if (Value.Count < 2)
                    _ntp_client_status.Add("Error...");
                else if (Value.Count < 3)
                    _ntp_client_status.Add("No NTP Server");
                else
                {
                    Value.RemoveAt(0);
                    Value.RemoveAt(0);
                    foreach (String line in Value)
                    {
                        string[] array = line.Split(" ".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
                        if (array.Length == 10)
                        {
                            _ntp_client_status.Add(String.Format("{0}: {1}", array[0].Trim('*'), array[0].StartsWith("*") ? idiomas.strings.Sincronizado/*"Sincronizado"*/ : 
                                idiomas.strings.NoSincronizado/*"No Sincronizado"*/));
                        }
                        else
                        {
#if DEBUG
                            _ntp_client_status.Add("Line Error");
#endif
                        }
                    }
                }
#endif
            }
        }
        protected List<string> NormalizeNtpStatusList(List<string> input)
        {
            List<string> output = new List<string>();
            int lenline = 78;

            if (input.Count == 1 && input[0].Length > lenline)
            {
                output = Enumerable.Range(0, input[0].Length / lenline).Select(i => input[0].Substring(i * lenline, lenline)).ToList();
            }
            else
                output = input;

            return output;
        }

        /// <summary>
        /// 
        /// </summary>
        public bool Errores
        {
            get
            {
                //if (gwA.Errores == true)
                //    return true;
                //else if (Dual && gwB.Errores == true)
                //    return true;
                //return false;
                return gwA.Seleccionada ? gwA.Errores : gwB.Seleccionada ? gwB.Errores : true;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public void Reset()
        {
            std = std.NoInfo;
            presente = false;
       }

        // public int _polling { get; set; }
        // public System.Threading.Thread _explorer = null;
        /// <summary>
        /// 
        /// </summary>
        public stdPhGw cpu_activa
        {
            get
            {
                if (Dual == false)
                    return gwA;
                if (gwA.Seleccionada)
                    return gwA;
                if (gwB.Seleccionada)
                    return gwB;
                return gwA;
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="from"></param>
        public void CopyFrom(stdGw from)
        {
            name = from.name;
            ip = from.ip;
            std = from.std;
            presente = from.presente;
            Dual = from.Dual;
            _ntp_client_status = from.ntp_client_status.ToList();
            gwA.CopyFrom(from.gwA);
            gwB.CopyFrom(from.gwB);
        }

        public bool Equals(stdGw other)
        {
            return other==null ? false : (name == other.name && ip == other.ip && gwA.Equals(other.gwA) && gwB.Equals(other.gwB));
        }
    
    }

    /// <summary>
    /// 
    /// </summary>
    [DataContract]
#if !_TESTING_
    public class U5kManStdData : BaseCode
#else
    public class U5kManStdData
#endif
    {
#if STD_ACCESS_V0
        [DataMember]
        public U5KStdGeneral _gen = new U5KStdGeneral();
        [DataMember]
        public List<stdPos> stdpos = new List<stdPos>();
        [DataMember]
        public List<stdGw> stdgws = new List<stdGw>();
        [DataMember]
        public U5kManStdEquiposEurocae stdeqeu = new U5kManStdEquiposEurocae();
        [DataMember]
        public U5kManStdEquiposEurocae atsDestStd = new U5kManStdEquiposEurocae();
        [DataMember]
        public Uv5kManDestinosPabx pabxdest = new Uv5kManDestinosPabx();
        [DataMember]
        public List<KeyValuePair<string, int>> usuarios = new List<KeyValuePair<string,int>>();
#else
        [DataMember]
        private U5KStdGeneral _gen = new U5KStdGeneral();
        [DataMember]
        private List<stdPos> stdpos = new List<stdPos>();
        [DataMember]
        private List<stdGw> stdgws = new List<stdGw>();
        [DataMember]
        private U5kManStdEquiposEurocae stdeqeu = new U5kManStdEquiposEurocae();
        [DataMember]
        public U5kManStdEquiposEurocae atsDestStd = new U5kManStdEquiposEurocae();
        [DataMember]
        private Uv5kManDestinosPabx pabxdest = new Uv5kManDestinosPabx();
        [DataMember]
        public List<KeyValuePair<string, int>> usuarios = new List<KeyValuePair<string, int>>();
#endif

        /// <summary>
        /// 
        /// </summary>
        public void Init_()
        {
            _gen = new U5KStdGeneral();
            stdpos = new List<stdPos>();
            stdgws = new List<stdGw>();
            stdeqeu = new U5kManStdEquiposEurocae();
            pabxdest = new Uv5kManDestinosPabx();
        }

#if STD_ACCESS_V0
        /** */
        public stdPos OpeGet(string name)
        {
            lock (stdpos)
            {
                foreach (stdPos pos in stdpos)
                {
                    if (pos.name == name)
                        return pos;
                }
                return null;
            }
        }

        /** */
        public stdGw GwGet(string name)
        {
            foreach (stdGw gw in stdgws)
            {
                if (gw.name == name)
                    return gw;
            }
            return null;
        }
#endif

#if STD_ACCESS_V0
#else
        /** 20171023. Rutinas los Clones... */
        Object lockConcurrent = new Object();
        Semaphore writeAccess = new Semaphore(1, 1);
        
        String ocupadopor = "";
        System.Diagnostics.Stopwatch tm = new System.Diagnostics.Stopwatch();

        public void wrAccAcquire([System.Runtime.CompilerServices.CallerLineNumber] int lineNumber = 0, 
            [System.Runtime.CompilerServices.CallerMemberName] string caller = null,
            [System.Runtime.CompilerServices.CallerFilePath] string file = null)
        {
            if (writeAccess.WaitOne(60000) == false)          // TODO
            {
#if !_TESTING_ && DEBUG 
                LogFatal<U5kManStdData>(String.Format("From [{0}:{1}] ", caller, lineNumber),
                    "Timeout en Acceso al Semaforo de Escritura de Estado. Ocupador por " + ocupadopor);
#endif
                throw new Exception("Timeout en Acceso al Semaforo de Escritura de Estado. Ocupador por " + ocupadopor);
            }
            ocupadopor = String.Format("[{0}.{1},{2}]", System.IO.Path.GetFileNameWithoutExtension(file), caller, lineNumber);
            tm.Restart();
        }
        public void wrAccRelease([System.Runtime.CompilerServices.CallerLineNumber] int lineNumber = 0, [System.Runtime.CompilerServices.CallerMemberName] string caller = null) 
        {
            try
            {
                tm.Stop();
#if !_TESTING_
                NLog.LogManager.GetLogger("sem-hist").Info("Semaforo ocupado por {0} durante {1} ms", ocupadopor, tm.ElapsedMilliseconds);
#endif
                ocupadopor = "";
                writeAccess.Release();
            }
#if !_TESTING_
            catch (Exception x)
            {
#if DEBUG
                LogFatal<U5kManStdData>(String.Format("From [{0}:{1}] ", caller, lineNumber), String.Format("{0}. Semaforo ocupado por {1}", x.Message, ocupadopor)/*, x*/);
#endif
                LogException<U5kManStdData>(System.Reflection.MethodBase.GetCurrentMethod().Name, "", x);
            }
#endif
            catch (Exception)
            {
               
            }
        }
        /// <summary>
        /// 
        /// </summary>
        public U5KStdGeneral STDG
        {
            get
            {
                lock (lockConcurrent)
                {
                    return new U5KStdGeneral(_gen);
                }
            }
            set
            {
                lock (lockConcurrent)
                {
                    _gen.CopyFrom(value);
                }
            }
        }
        /// <summary>
        /// 
        /// </summary>
        public List<stdPos> STDTOPS
        {
            get
            {
                lock (lockConcurrent)
                {
                    List<stdPos> newpos = stdpos.Select(pos => new stdPos(pos) { name = pos.name, ip = pos.ip, snmpport = pos.snmpport }).ToList();
                    return newpos;
                }
            }
            set
            {
                lock (lockConcurrent)
                {
                    foreach (stdPos pos in value)
                    {
                        stdPos local = stdpos.Where(p => p.Equals(pos)).FirstOrDefault();
                        if (local != null)
                        {
                            local.CopyFrom(pos);
                        }
                        else
                        {
                            stdpos.Add(pos);
                        }
                    }
                }
            }
        }
        public List<stdPos> CFGTOPS
        {
            set
            {
                List<stdPos> paraBorrar = STDTOPS;
                lock (lockConcurrent)
                {
                    foreach (stdPos pos in value)
                    {
                        stdPos local = paraBorrar.Where(p => p.Equals(pos)).FirstOrDefault();
                        if (local != null)
                        {
                            paraBorrar.Remove(local);
                        }
                        else
                        {
                            stdpos.Add(pos);
                        }
                    }
                    // Borrar los restos...
                    foreach (stdPos pos in paraBorrar)
                    {
                        stdpos.Remove(stdpos.Find(x => x.Equals(pos)));
                    }

                }
            }
        }
        /// <summary>
        /// 
        /// </summary>
        public List<stdGw> STDGWS
        {
            get
            {
                lock (lockConcurrent)
                {
                    List<stdGw> newgw = stdgws.Select(g => new stdGw(g)).ToList();                    
                    return newgw;
                }
            }
            set
            {
                lock (lockConcurrent)
                {
                    foreach (stdGw gw in value)
                    {
                        stdGw local = stdgws.Where(p => p.Equals(gw)).FirstOrDefault();
                        if (local != null)
                        {
                            local.CopyFrom(gw);
                        }
                        else
                        {
                            stdgws.Add(gw);
                        }
                    }
                }
            }
        }
        public List<stdGw> CFGGWS
        {
            set
            {
                List<stdGw> paraBorrar = STDGWS;
                lock (lockConcurrent)
                {
                    foreach (stdGw gw in value)
                    {
                        stdGw local = paraBorrar.Where(p => p.Equals(gw)).FirstOrDefault();
                        if (local != null)
                        {
                            paraBorrar.Remove(local);
                        }
                        else
                        {
                            stdgws.Add(gw);
                        }
                    }
                    // Borrar los restos...
                    foreach (stdGw gw in paraBorrar)
                    {
                        stdgws.Remove(stdgws.Find(x => x.Equals(gw)));
                    }
                }
            }
        }
        /// <summary>
        /// 
        /// </summary>
        public List<U5kManStdEquiposEurocae.EquipoEurocae> STDEQS
        {
            get
            {
                lock (lockConcurrent)
                {
                    return stdeqeu.Equipos.Select(e => new U5kManStdEquiposEurocae.EquipoEurocae(e) { }).ToList();
                }
            }
            set
            {
                lock (lockConcurrent)
                {
                    foreach (U5kManStdEquiposEurocae.EquipoEurocae equ in value)
                    {
                        U5kManStdEquiposEurocae.EquipoEurocae local = stdeqeu.Equipos.Where(p => p.Equals(equ)).FirstOrDefault();
                        if (local != null)
                        {
                            local.CopyFrom(equ);
                        }
                        else
                        {
                            stdeqeu.Equipos.Add(equ);
                        }
                    }
                }
            }
        }
        public List<U5kManStdEquiposEurocae.EquipoEurocae> CFGEQS
        {
            set
            {
                List<U5kManStdEquiposEurocae.EquipoEurocae> paraBorrar = STDEQS;
                lock (lockConcurrent)
                {
                    foreach (U5kManStdEquiposEurocae.EquipoEurocae equ in value)
                    {
                        U5kManStdEquiposEurocae.EquipoEurocae local = paraBorrar.Where(p => p.Equals(equ)).FirstOrDefault();
                        if (local != null)
                        {
                            paraBorrar.Remove(local);
                        }
                        else
                        {
                            stdeqeu.Equipos.Add(equ);
                        }
                    }
                    // Borrar los restos...
                    foreach (U5kManStdEquiposEurocae.EquipoEurocae equ in paraBorrar)
                    {
                        stdeqeu.Equipos.Remove(stdeqeu.Equipos.Find(x => x.Equals(equ)));
                    }

                }
            }
        }
        /// <summary>
        /// 
        /// </summary>
        public List<Uv5kManDestinosPabx.DestinoPabx> STDPBXS
        {
            get
            {
                lock (lockConcurrent)
                {
                    return pabxdest.Destinos.Select(e => new Uv5kManDestinosPabx.DestinoPabx(e) { Id=e.Id }).ToList();
                }
            }
            set
            {
                lock (lockConcurrent)
                {
                    foreach (Uv5kManDestinosPabx.DestinoPabx dst in value)
                    {
                        Uv5kManDestinosPabx.DestinoPabx local = pabxdest.Destinos.Where(p => p.Equals(dst)).FirstOrDefault();
                        if (local != null)
                        {
                            local.CopyFrom(dst);
                        }
                        else
                        {
                            pabxdest.Destinos.Add(dst);
                        }
                    }
                }
            }
        }
        public List<Uv5kManDestinosPabx.DestinoPabx> CFGPBXS
        {
            set
            {
                List<Uv5kManDestinosPabx.DestinoPabx> paraBorrar = STDPBXS;
                lock (lockConcurrent)
                {
                    foreach (Uv5kManDestinosPabx.DestinoPabx dst in value)
                    {
                        Uv5kManDestinosPabx.DestinoPabx local = paraBorrar.Where(p => p.Equals(dst)).FirstOrDefault();
                        if (local != null)
                        {
                            paraBorrar.Remove(local);
                        }
                        else
                        {
                            pabxdest.Destinos.Add(dst);
                        }
                    }
                    // Borrar los restos...
                    foreach (Uv5kManDestinosPabx.DestinoPabx dst in paraBorrar)
                    {
                        pabxdest.Destinos.Remove(pabxdest.Destinos.Find(x => x.Equals(dst)));
                    }

                }
            }
        }

        public bool Equals(U5kManStdData other)
        {
            bool retorno = _gen.Equals(other._gen);

            stdpos.ForEach(pos => retorno = retorno && pos.Equals(other.stdpos.Where(pos1 => pos1.name == pos.name).FirstOrDefault()));
            stdgws.ForEach(gw1 => retorno = retorno && gw1.Equals(other.stdgws.Where(gw2 => gw2.name == gw1.name).FirstOrDefault()));
            stdeqeu.Equipos.ForEach(eq1 => retorno = retorno && eq1.Equals(other.stdeqeu.Equipos.Where(eq2 => eq2.Id == eq1.Id).FirstOrDefault()));
            pabxdest.Destinos.ForEach(ds1 => retorno = retorno && ds1.Equals(other.pabxdest.Destinos.Where(ds2 => ds1.Id == ds2.Id).FirstOrDefault()));

            return retorno;
        }

#endif
    }

    [DataContract]
    public class U5kManLastInciList
    {
        [DataContractAttribute]
        public class eListaInci
        {
            [DataMember]
            public long _id;
            [DataMember]
            public DateTime _fecha;
            [DataMember]
            public string _desc = "";
        }

        [DataMember]
        public List<eListaInci> _lista = new List<eListaInci>();
    }

    [DataContract]
    public class U5kManStdEquiposEurocae
    {
        [DataContract]
        public class EquipoEurocae : IEquatable<EquipoEurocae>
        {
            [DataMember]
            public string Id { get; set; }
            [DataMember]
            public string Ip1 { get; set; }
            [DataMember]
            public string Ip2 { get; set; }
            [DataMember]
            public int Tipo { get; set; }
            [DataMember]
            public int Modelo { get; set; }
            [DataMember]
            public int RxTx { get; set; }
            [DataMember]
            public string fid { get; set; }
            [DataMember]
            public string sip_user { get; set; }
            [DataMember]
            public int sip_port { get; set; }
            [DataMember]
            public std EstadoRed1 { get; set; }
            [DataMember]
            public std EstadoRed2 { get; set; }
            [DataMember]
            public std EstadoSip { get; set; }

            public EquipoEurocae(EquipoEurocae last)
            {
                if (last == null)
                {
                    EstadoRed1 = EstadoRed2 = EstadoSip = std.NoInfo;
                }
                else
                {
                    //EstadoRed1 = last.EstadoRed1;
                    //EstadoRed2 = last.EstadoRed2;
                    //EstadoSip = last.EstadoSip;
                    CopyFrom(last);
                }
            }

            public std EstadoGeneral
            {
                get
                {
                    if (EstadoRed1 == std.Ok || EstadoRed2 == std.Ok)
                    {
                        if (Tipo != 5) 
                        {                            
                            return EstadoSip;
                        }
                        return std.Ok;
                    }
                    else if (EstadoRed1 == std.NoInfo && EstadoRed2 == std.NoInfo)
                    {
                        return (int)std.NoInfo;
                    }
                    return std.Aviso;
                }
            }

            public bool Equals(EquipoEurocae other)
            {
                return other==null ? false : (Id == other.Id && Ip1 == other.Ip1 && Tipo == other.Tipo && Modelo == other.Modelo && RxTx == other.RxTx && fid == other.fid && sip_port == other.sip_port && sip_user == other.sip_user);
            }

            public void CopyFrom(EquipoEurocae from)
            {
                Id = from.Id;
                Ip1 = from.Ip1;
                Ip2 = from.Ip2;
                Tipo = from.Tipo;
                Modelo = from.Modelo;
                RxTx = from.RxTx;
                fid = from.fid;
                sip_port = from.sip_port;
                sip_user = from.sip_user;

                EstadoRed1 = from.EstadoRed1;
                EstadoRed2 = from.EstadoRed2;
                EstadoSip = from.EstadoSip;
            }
        }

        [DataMember]
        public List<EquipoEurocae> Equipos = new List<EquipoEurocae>();

#if STD_ACCESS_V0
        /// <summary>
        /// 
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public EquipoEurocae EEGet(string name)
        {
            foreach (EquipoEurocae eq in Equipos)
            {
                if (eq.Id == name)
                    return eq;
            }
            return null;
        }
#endif
    }

    [DataContract]
    public class Uv5kManDestinosPabx
    {
        [DataContract]
        public class DestinoPabx
        {
            [DataMember]
            public string Id { get; set; }
            [DataMember]
            public std Estado { get; set; }

            public DestinoPabx(DestinoPabx last=null)
            {
                Estado = last == null ? std.NoInfo : last.Estado;
            }
            public void CopyFrom(DestinoPabx from)
            {
                Id = from.Id;
                Estado = from.Estado;
            }
            public bool Equals(DestinoPabx otro)
            {
                return otro==null ? false : Id == otro.Id;
            }
        }

        [DataMember]
        public List<DestinoPabx> Destinos = new List<DestinoPabx>();

#if STD_ACCESS_V0
        /// <summary>
        /// 
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public DestinoPabx DPGet(string name)
        {
            foreach (DestinoPabx dst in Destinos)
            {
                if (dst.Id == name)
                    return dst;
            }
            return null;
        }
#endif
    }

#if !_TESTING_
    [ServiceContract]
    public interface IU5kManService
    {
        [OperationContract]
        U5KStdGeneral ObtenerEstadoGeneral();
        [OperationContract]
        List<stdPos> ObtenerEstadoPosiciones();
        [OperationContract]
        List<stdGw> ObtenerEstadoPasarelas();

        [OperationContract]
        U5kManLastInciList UltimasIncidencias();
        [OperationContract]
        U5kManLastInciList ReconocerAlarmas(string user, U5kManLastInciList lista);
        [OperationContract]
        List<U5kManStdEquiposEurocae.EquipoEurocae> ObtenerEstadoEquiposEurocae();        
        [OperationContract]
        List<Uv5kManDestinosPabx.DestinoPabx> ObtenerDestinosPabx();
        [OperationContract]
        U5kManLastInciList ReconoceAlarma(string user, DateTime hora, string desc);


        [OperationContract]
        void MainStandbyChange(string gwname, int ab);
        [OperationContract]
        void Reload();
        [OperationContract]
        string Version();
    }

    /// <summary>
    /// 
    /// </summary>
    [ServiceBehavior (IncludeExceptionDetailInFaults =true)]
    class U5kManService : BaseCode, IU5kManService
    {
        #region Atributos Estaticos
        /// <summary>
        /// 20170309. AGL. Para sincronizar los THREAD que tocan la configuracion....
        /// </summary>
        // static public EventQueue _EventQueue = new EventQueue();
        /// <summary>
        /// 
        /// </summary>
        static public U5kBdtService Database { get; set; }
        /// <summary>
        /// 
        /// </summary>
#if STD_ACCESS_V0
        static public U5kManStdData last_std = null;
#endif
        /// <summary>
        /// 
        /// </summary>
        static public U5kManStdData _std;   // = new U5kManStdData();
        /// <summary>
        /// 
        /// </summary>
        static public MainThread _main = new MainThread();
        /// <summary>
        /// 
        /// </summary>
        // static public string cfgVersion { get; set; }
        /// <summary>
        /// 
        /// </summary>
        static public U5kManLastInciList _last_inci = new U5kManLastInciList();
        static public U5kBdtService _bdt = null;
        /// <summary>
        /// 
        /// </summary>
        static public bool _Master = true;
        /// <summary>
        /// 
        /// </summary>
        static int _Radios = -1;
        static int _Lineas = -1;

        #region Servicio Radio
        /// <summary>
        /// 
        /// </summary>
        public class radioSessionData
        {
            /* datos de frecuencia */
            public string frec { get; set; }
            public int ftipo { get; set; }
            public int prio { get; set; }
            public int fstd { get; set; }
            public int fp_climax_mc { get; set; }
            public int fp_bss_win { get; set; }
            public string selected_site { get; set; }
            public int selected_site_qidx { get; set; }

            /* datos de sesion */
            public string uri { get; set; }
            public string tipo { get; set; }       // 0: TX, 1: RX, 2: RXTX
            public int std { get; set; }           // 0: Desconectado, 1: Conectado.
            public int tx_rtp { get; set; }
            public int tx_cld { get; set; }
            public int tx_owd { get; set; }
            public int rx_rtp { get; set; }
            public int rx_qidx { get; set; }

            public radioSessionData() { }
        }
        /// <summary>
        /// 
        /// </summary>
        public class tifxInfo
        {
            public class tifxResInfo
            {
                public string id { get; set; }
                public int ver { get; set; }
                public int std { get; set; }
                public int prio { get; set; }
                public int tp { get; set; }
            }
            public string id { get; set; }
            public int tp { get; set; }
            public string ip { get; set; }
            public int ver { get; set; }
            public List<tifxResInfo> res { get; set; }
        }
        /// <summary>
        /// 
        /// </summary>
        public class equipoMNData
        {
            public string equ { get; set; }
            public int grp { get; set; }    // 0: VHF, 1: UHF
            public int mod { get; set; }    // 0: TX, 1: RX
            public int tip { get; set; }    // 0: MAIN, 1: RSVA
            public int std { get; set; }    // 0: Desconectado, 1: Conectado, 2: Desahabilitado
            public string frec { get; set; }
            public int? prio { get; set; }
            public int sip { get; set; }
            public string ip { get; set; }
            public string emp { get; set; }
            public int tfrec { get; set; }   // Tipo de Frecuencia. 0: Normal, 1: 1+1, 2: FD, 3: EM

            public equipoMNData() 
            {
            }
        }

        /** 20171009. Incluimos las pantallas de Gestion HF*/
        public class txHF
        {
            public string id { get; set; }
            public string gestor { get; set; }
            public string oid { get; set; }
            public int std { get; set; }
            public string fre { get; set; }
            public string uri { get; set; }
            public string user { get; set; }

            public txHF() { }
        }


        /** */
        static public List<radioSessionData> _sessions_data = new List<radioSessionData>();
        static public List<equipoMNData> _MNMan_data = new List<equipoMNData>();
        static public List<txHF> _txhf_data = new List<txHF>();

        static public List<tifxInfo> tifxs = new List<tifxInfo>();

        #endregion

        #endregion

        #region Procedimientos Estaticos

        /// <summary>
        /// 
        /// </summary>
        static protected void OpenBdt()
        {
            if (_bdt == null)
            {
                Properties.u5kManServer cfg = Properties.u5kManServer.Default;
                if (Properties.u5kManServer.Default.TipoBdt == 1)
                    _bdt = new U5kBdtService(System.Threading.Thread.CurrentThread.CurrentUICulture, eBdt.bdtSqlite, cfg.SQLitePath);
                else
                    _bdt = new U5kBdtService(System.Threading.Thread.CurrentThread.CurrentUICulture, eBdt.bdtMySql, 
                        cfg.MySqlServer, cfg.MySqlUser, cfg.MySqlPwd);
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="f1"></param>
        /// <param name="f2"></param>
        /// <returns></returns>
        static protected bool DateCompare(DateTime f1, DateTime f2)
        {
            return f1.DayOfYear == f2.DayOfYear && f1.Hour == f2.Hour && f1.Minute == f2.Minute && f1.Second == f2.Second;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="TipoRecurso"></param>
        /// <param name="interfaz"></param>
        /// <returns></returns>
        static public itf Bdt2Rct(uint TipoRecurso, uint interfaz)
        {
            if (TipoRecurso == 0)
                return itf.rcRadio;
            else if (TipoRecurso == 1)
            {
                return interfaz == 2 ? itf.rcPpBC :
                    interfaz == 3 ? itf.rcPpBL :
                    interfaz == 4 ? itf.rcPpAB :
                    interfaz == 5 ? itf.rcAtsR2 :
                    interfaz == 6 ? itf.rcAtsN5 :
                    interfaz == 50 ? itf.rcPpEM :
                    interfaz == 51 ? itf.rcPpEMM :
                        itf.rcNotipo;
            }
            else if (TipoRecurso == 2)
                return itf.rcLCE;
            else
                return itf.rcNotipo;
        }

        /// <summary>
        /// 
        /// </summary>
        static public int Posiciones
        {
#if STD_ACCESS_V0
            get { return _std.stdpos.Count; }
#else
            get { return _std.STDTOPS.Count; }
#endif
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="dt"></param>
        /// <param name="str"></param>
        static public void AddInci(DateTime dt, string str)
        {
            lock (_last_inci)
            {
                U5kManLastInciList.eListaInci inci = new U5kManLastInciList.eListaInci();
                inci._fecha = dt;
                inci._desc = str;

                _last_inci._lista.Add(inci);

                /** Ordeno Descendente */
                var result = _last_inci._lista.OrderByDescending(x => x._fecha).ToList();
                _last_inci._lista = result;

                /** Elimino la ultima si procede */
                if (_last_inci._lista.Count > Properties.u5kManServer.Default.LineasIncidencias)
                    _last_inci._lista.Remove(_last_inci._lista[Properties.u5kManServer.Default.LineasIncidencias - 1]);

            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="user"></param>
        /// <param name="fecha"></param>
        /// <param name="desc"></param>
        /// <returns></returns>
        static public U5kManLastInciList stReconoceAlarma(string user, DateTime fecha, string desc)
        {
            lock (_last_inci)
            {
                U5kManLastInciList.eListaInci inci = _last_inci._lista.Find(e => DateCompare(e._fecha, fecha) && e._desc == desc);
                if (inci != null)
                {
                    /** Las borro de _last_inci */
                    _last_inci._lista.Remove(inci);

                    /** Las Actualizo en BDT */
                    //OpenBdt();
                    //if (_bdt != null)
                    //{
                    //    string[] par = HistThread.strInci2Descr(inci._desc);
                    //    if (par.Length == 2)
                    //        _bdt.ReconoceAlarma(user, inci._fecha, par[0], par[1]);
                    //}
                    Task.Run(() => UpdateInciTask(user, inci));
                }
                return _last_inci;
            }
        }
        /// <summary>
        /// Acceso a Base de Datos en Threahs aparte...
        /// </summary>
        static Object reconoce_lock = new Object();
        static void UpdateInciTask(string user, U5kManLastInciList.eListaInci inci)
        {
            try
            {
                lock (reconoce_lock)
                {
                    OpenBdt();
                    if (_bdt != null)
                    {
                        string[] par = HistThread.strInci2Descr(inci._desc);
                        if (par.Length == 2)
                            _bdt.ReconoceAlarma(user, inci._fecha, par[0], par[1]);
                    }
                }
            }
            catch (Exception x)
            {
                LogException<uv5kServiceMain>(System.Reflection.MethodBase.GetCurrentMethod().Name, "", x);
            }
            finally
            {
            }
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="gwname"></param>
        /// <param name="ab"></param>
        static public void stMainStandbyChange(string gwname, int ab)
        {
            try
            {
                Socket s = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
                IPAddress ip = IPAddress.Parse(Properties.u5kManServer.Default.MainStanbyMcastAdd);
                IPEndPoint ipep = new IPEndPoint(ip, Properties.u5kManServer.Default.MainStandByMcastPort);

                s.SetSocketOption(SocketOptionLevel.IP, SocketOptionName.AddMembership, new MulticastOption(ip));
                s.SetSocketOption(SocketOptionLevel.IP, SocketOptionName.MulticastTimeToLive, 2);

                s.SetSocketOption(SocketOptionLevel.IP, SocketOptionName.MulticastInterface,
                    (int)IPAddress.HostToNetworkOrder(U5kManService.NetworkInterfaceIndex(Properties.u5kManServer.Default.MiDireccionIP)));

                s.Connect(ipep);

                byte[] men = new byte[35];

                men[0] = 0x32;
                men[33] = (byte)(ab == 0 ? 'P' : 'R');
                men[34] = (byte)(ab == 0 ? 'R' : 'P');

                System.Buffer.BlockCopy(ASCIIEncoding.ASCII.GetBytes(gwname), 0, men, 1, gwname.Length);
                s.Send(men, 0, 35, SocketFlags.None);

                s.Close();
            }
            catch (Exception x)
            {
                LogException<uv5kServiceMain>(System.Reflection.MethodBase.GetCurrentMethod().Name, "", x);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        static void CuentaRadiosYLineas()
        {
            _Radios = 0; _Lineas = 0;
#if STD_ACCESS_V0
            List<stdGw> stdgws = U5kManService._std.stdgws;
#else
            List<stdGw> stdgws = U5kManService._std.STDGWS;
#endif
            foreach (stdGw gw in stdgws)
            {
                foreach (stdSlot slot in gw.gwA.slots)
                {
                    if (slot != null)
                    {
                        foreach (stdRec rc in slot.rec)
                        {
                            if (rc != null)
                            {
                                if (rc.tipo == itf.rcRadio)
                                    _Radios++;
                                else if (rc.tipo != itf.rcNotipo)
                                    _Lineas++;
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        static public int Radios
        {
            get
            {
                if (_Radios == -1)
                {
                    CuentaRadiosYLineas();
                }
                return _Radios;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        static public int Lineas
        {
            get
            {
                if (_Lineas == -1)
                    CuentaRadiosYLineas();
                return _Lineas;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="gwname"></param>
        static public int NetworkInterfaceIndex(string ip)
        {
            NetworkInterface[] nics = NetworkInterface.GetAllNetworkInterfaces();
            foreach (NetworkInterface adapter in nics)
            {
                IPInterfaceProperties ip_properties = adapter.GetIPProperties();

                if (!adapter.GetIPProperties().MulticastAddresses.Any())
                    continue; // most of VPN adapters will be skipped

                if (!adapter.SupportsMulticast)
                    continue; // multicast is meaningless for this type of connection

                if (OperationalStatus.Up != adapter.OperationalStatus)
                    continue; // this adapter is off or not connected

                foreach (UnicastIPAddressInformation inf in ip_properties.UnicastAddresses)
                {
                    if (inf.Address.Equals(IPAddress.Parse(ip)) == true)
                    {

                        IPv4InterfaceProperties p = adapter.GetIPProperties().GetIPv4Properties();
                        if (p != null)
                            return p.Index;

                    }
                }

            }
            return 0;
        }

        /// <summary>
        /// 
        /// </summary>
        static public void Reset()
        {
            try
            {
                Properties.u5kManServer.Default.Reload();

                // System.Configuration.ConfigurationManager.RefreshSection("applicationSettings");
                _main.Stop();
                _main = new MainThread();
                _main.Start();
            }
            catch (Exception x)
            {
                LogException<uv5kServiceMain>(System.Reflection.MethodBase.GetCurrentMethod().Name, "", x);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        static public List<U5kIncidenciaDescr> bdtListaIncidencias()
        {
            OpenBdt();
            return _bdt.ListaDeIncidencias(Properties.u5kManServer.Default.FiltroIncidencias);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="inci"></param>
        static public void bdtUpdateIncidencia(U5kIncidenciaDescr inci)
        {
            OpenBdt();
            _bdt.UpdateGeneraErrorIncidencia(inci.id, inci.alarm ? 1 : 0);
        }

        static public List<U5kHistLine> bdtConsultaHistorico(string query)
        {
            OpenBdt();
            return _bdt.ConsultaHistoricoList(query);
        }

        static public List<string> bdtListaItemsMN()
        {
            OpenBdt();
            return _bdt.ListaMN();
        }

        #endregion

        #region Procedimientos No Estaticos (para el servicio web)
        /// <summary>
        /// 
        /// </summary>
        public U5kManService()
        {
            try
            {
                OpenBdt();
            }
            catch (Exception x)
            {
                LogException<uv5kServiceMain>(System.Reflection.MethodBase.GetCurrentMethod().Name, "", x);
                _bdt = null;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public U5KStdGeneral ObtenerEstadoGeneral()
        {
#if STD_ACCESS_V0
            return _std._gen;
#else
            return _std.STDG;
#endif
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public List<stdPos> ObtenerEstadoPosiciones()
        {
#if STD_ACCESS_V0
            return _std.stdpos;
#else
            return _std.STDTOPS;
#endif
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public List<stdGw> ObtenerEstadoPasarelas()
        {
#if STD_ACCESS_V0
            return _std.stdgws;
#else
            return _std.STDGWS;
#endif
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public List<U5kManStdEquiposEurocae.EquipoEurocae> ObtenerEstadoEquiposEurocae()
        {
#if STD_ACCESS_V0
            return _std.stdeqeu.Equipos;
#else
            return _std.STDEQS;
#endif
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public List<Uv5kManDestinosPabx.DestinoPabx> ObtenerDestinosPabx()
        {
#if STD_ACCESS_V0
            return _std.pabxdest.Destinos;
#else
            return _std.STDPBXS;
#endif
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public U5kManLastInciList UltimasIncidencias()
        {
            return _last_inci;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="lista"></param>
        public U5kManLastInciList ReconocerAlarmas(string user, U5kManLastInciList lista)
        {
            lock (_last_inci)
            {
                foreach (U5kManLastInciList.eListaInci inci in lista._lista)
                {
                    U5kManLastInciList.eListaInci inci1 = _last_inci._lista.Find(e => e._fecha == inci._fecha &&  e._desc == inci._desc);
                    if (inci1 != null && inci1._desc != "")
                    {
                        /** Las borro de _last_inci */
                        _last_inci._lista.Remove(inci1);
                        /** Las Actualizo en BDT */
                        if (_bdt != null)
                        {
                            string[] par = HistThread.strInci2Descr(inci1._desc);
                            if (par.Length == 2)
                                _bdt.ReconoceAlarma(user, inci1._fecha, par[0], par[1]);
                        }
                    }
                }
            }

            return _last_inci;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="user"></param>
        /// <param name="fecha"></param>
        /// <param name="desc"></param>
        /// <returns></returns>
        public U5kManLastInciList ReconoceAlarma(string user, DateTime fecha, string desc)
        {
            //lock (_last_inci)
            //{
            //    U5kManLastInciList.eListaInci inci = _last_inci._lista.Find(e => DateCompare(e._fecha, fecha) && e._desc == desc);
            //    if (inci != null)
            //    {
            //        /** Las borro de _last_inci */
            //        _last_inci._lista.Remove(inci);
            //        /** Las Actualizo en BDT */
            //        if (_bdt != null)
            //        {
            //            string[] par = HistThread.strInci2Descr(inci._desc);
            //            if (par.Length == 2)
            //                _bdt.ReconoceAlarma(user, inci._fecha, par[0], par[1]);
            //        }
            //    }
            //    return _last_inci;
            //}
            return stReconoceAlarma(user, fecha, desc);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="gwname"></param>
        /// <param name="ab"></param>
        public void MainStandbyChange(string gwname, int ab)
        {
            //try
            //{
            //    Socket s = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            //    IPAddress ip = IPAddress.Parse(Properties.u5kManServer.Default.MainStanbyMcastAdd);
            //    IPEndPoint ipep = new IPEndPoint(ip, Properties.u5kManServer.Default.MainStandByMcastPort);
                
            //    s.SetSocketOption(SocketOptionLevel.IP, SocketOptionName.AddMembership, new MulticastOption(ip));
            //    s.SetSocketOption(SocketOptionLevel.IP, SocketOptionName.MulticastTimeToLive, 2);

            //    s.SetSocketOption(SocketOptionLevel.IP, SocketOptionName.MulticastInterface,
            //        (int)IPAddress.HostToNetworkOrder(U5kManService.NetworkInterfaceIndex(Properties.u5kManServer.Default.MiDireccionIP)));

            //    s.Connect(ipep);

            //    byte[] men = new byte[35];

            //    men[0] = 0x32;
            //    men[33] = (byte)(ab == 0 ? 'P' : 'R');
            //    men[34] = (byte)(ab == 0 ? 'R' : 'P');

            //    System.Buffer.BlockCopy(ASCIIEncoding.ASCII.GetBytes(gwname), 0, men, 1, gwname.Length);
            //    s.Send(men, 0, 35, SocketFlags.None);

            //    s.Close();
            //}
            //catch (Exception x)
            //{
            //    _logger.Error("Excepcion en MainStandbyChange:{0}", x.Message);
            //    _logger.Trace(x,"Excepcion en MainStandbyChange: ");
            //}
            stMainStandbyChange(gwname, ab);
        }

        /// <summary>
        /// 
        /// </summary>
        public void Reload()
        {
            U5kManService.Reset();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public string Version()
        {
            return U5kGenericos.Version;
        }

        #endregion

    }
#endif
}
