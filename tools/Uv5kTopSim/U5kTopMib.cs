using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;

using NLog;
using Lextm.SharpSnmpLib;
using Lextm.SharpSnmpLib.Pipeline;
using Lextm.SharpSnmpLib.Messaging;
using Lextm.SharpSnmpLib.Security;
using Lextm.SharpSnmpLib.Objects;

namespace Uv5kTopSim 
{
    class U5kTopOids
    {
#if OIDS_V0
        public static string Oid_iEstadoTop = ".1.1.1000.0";
        public static string Oid_iAltavozRadio = ".1.1.1000.1.2.0";
        public static string Oid_iAltavozLcen = ".1.1.1000.1.2.1";
        public static string Oid_iAltavozHf = ".1.1.1000.1.2.2";
        public static string Oid_iCableGrabacion = ".1.1.1000.1.2.3";
        public static string Oid_iJackEjecutivo = ".1.1.1000.1.3.0";
        public static string Oid_iJackAyudante = ".1.1.1000.1.3.1";
        public static string Oid_iPanel = ".1.1.1000.1.4";
        public static string Oid_iLan1 = ".1.1.1000.3.1";
        public static string Oid_iLan2 = ".1.1.1000.3.2";
        public static string Oid_sPtt = ".1.1.1000.2";
        public static string Oid_sPaginaRadio = ".1.1.1000.6";
        public static string Oid_sLlamadaSaliente = ".1.1.1000.7";
        public static string Oid_sLlamadaEntrante = ".1.1.1000.9";
        public static string Oid_sLlamadaEstablecida = ".1.1.1000.11";
        public static string Oid_sLlamadaFinalizada = ".1.1.1000.10";
        public static string Oid_sFacilidad = ".1.1.1000.8";
        public static string Oid_sBriefing = ".1.1.1000.12";
        public static string Oid_sReplay = ".1.1.1000.13";
#else
        public static string Oid_iEstadoTop = ".1.3.6.1.4.1.7916.8.2.2.1.0";
        public static string Oid_iAltavozRadio = ".1.3.6.1.4.1.7916.8.2.2.2.0";
        public static string Oid_iAltavozLcen = ".1.3.6.1.4.1.7916.8.2.2.3.0";
        public static string Oid_iAltavozHf = ".1.3.6.1.4.1.7916.8.2.2.4.0";
        public static string Oid_iCableGrabacion = ".1.3.6.1.4.1.7916.8.2.2.5.0";
        public static string Oid_iJackEjecutivo = ".1.3.6.1.4.1.7916.8.2.2.6.0";
        public static string Oid_iJackAyudante = ".1.3.6.1.4.1.7916.8.2.2.7.0";
        public static string Oid_iPanel = ".1.3.6.1.4.1.7916.8.2.2.8.0";
        public static string Oid_iLan1 = ".1.3.6.1.4.1.7916.8.2.2.9.0";
        public static string Oid_iLan2 = ".1.3.6.1.4.1.7916.8.2.2.10.0";
        public static string Oid_sPtt = ".1.3.6.1.4.1.7916.8.2.3.1.0";
        public static string Oid_sPaginaRadio = ".1.3.6.1.4.1.7916.8.2.3.2.0";
        public static string Oid_sLlamadaSaliente = ".1.3.6.1.4.1.7916.8.2.3.3.0";
        public static string Oid_sLlamadaEntrante = ".1.3.6.1.4.1.7916.8.2.3.5.0";
        public static string Oid_sLlamadaEstablecida = ".1.3.6.1.4.1.7916.8.2.3.7.0";
        public static string Oid_sLlamadaFinalizada = ".1.3.6.1.4.1.7916.8.2.3.6.0";
        public static string Oid_sFacilidad = ".1.3.6.1.4.1.7916.8.2.3.4.0";
        public static string Oid_sBriefing = ".1.3.6.1.4.1.7916.8.2.3.8.0";
        public static string Oid_sReplay = ".1.3.6.1.4.1.7916.8.2.3.9.0";
#endif
    };

    /// <summary>
    /// 
    /// </summary>
    class U5kTopMib
    {
        /// <summary>
        /// Todos los objetos.
        /// </summary>
        // MibObject[] mib = new MibObject[]{};
        public List<MibObject> smib = new List<MibObject>();


        /// <summary>
        /// 
        /// </summary>
        MibObject[] mib_prv = new MibObject[]
        {
            // Parte Privada.            
            new SnmpIntObject(U5kTopOids.Oid_iEstadoTop, 1),     // 
            new SnmpIntObject(U5kTopOids.Oid_iAltavozRadio, 1),     // 
            new SnmpIntObject(U5kTopOids.Oid_iAltavozLcen, 1),     // 
            new SnmpIntObject(U5kTopOids.Oid_iAltavozHf, 1),     // 
            new SnmpIntObject(U5kTopOids.Oid_iCableGrabacion, 1),     // 
            new SnmpIntObject(U5kTopOids.Oid_iJackEjecutivo, 0),     // 
            new SnmpIntObject(U5kTopOids.Oid_iJackAyudante, 0),     // 
            new SnmpIntObject(U5kTopOids.Oid_iPanel, 1),     // 
            new SnmpIntObject(U5kTopOids.Oid_iLan1, 1),     // 
            new SnmpIntObject(U5kTopOids.Oid_iLan2, 1),

            new SnmpStringObject(U5kTopOids.Oid_sPtt, ""),
            new SnmpStringObject(U5kTopOids.Oid_sPaginaRadio, ""),
            new SnmpStringObject(U5kTopOids.Oid_sLlamadaEntrante, ""),
            new SnmpStringObject(U5kTopOids.Oid_sLlamadaEstablecida, ""),
            new SnmpStringObject(U5kTopOids.Oid_sLlamadaFinalizada, ""),
            new SnmpStringObject(U5kTopOids.Oid_sFacilidad, ""),
            new SnmpStringObject(U5kTopOids.Oid_sBriefing, ""),
            new SnmpStringObject(U5kTopOids.Oid_sReplay, "")
        };

        IPEndPoint trap_manager = null;
        /// <summary>
        /// 
        /// </summary>
        public U5kTopMib()
        {
        }

        /// <summary>
        /// 
        /// </summary>
        public void Load(string ipserver, int portserver)
        {
            trap_manager = new IPEndPoint(IPAddress.Parse(ipserver), portserver);

            // Carga los elementos discretos.
            foreach (MibObject obj in mib_prv)
            {
                smib.Add(obj);
            }

            // Ordeno OID y cargo el agente
            smib.Sort();
            foreach (MibObject obj in smib)
                TopSnmpAgent.Store.Add(obj);
        }

        /// <summary>
        /// 
        /// </summary>
        public int EstadoTop
        {
            get
            {
                return SnmpIntObject.Get(U5kTopOids.Oid_iEstadoTop).Value;
            }
            set
            {
                SnmpIntObject.Get(U5kTopOids.Oid_iEstadoTop).Value = value;
                TopSnmpAgent.Trap(U5kTopOids.Oid_iEstadoTop, SnmpIntObject.Get(U5kTopOids.Oid_iEstadoTop).Data, trap_manager);
            }
        }


        /// <summary>
        /// 
        /// </summary>
        public int AltavozRadio
        {
            get
            {
                return SnmpIntObject.Get(U5kTopOids.Oid_iAltavozRadio).Value;
            }
            set
            {
                SnmpIntObject.Get(U5kTopOids.Oid_iAltavozRadio).Value = value;
                TopSnmpAgent.Trap(U5kTopOids.Oid_iAltavozRadio, SnmpIntObject.Get(U5kTopOids.Oid_iAltavozRadio).Data, trap_manager);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public int AltavozLcen
        {
            get
            {
                return SnmpIntObject.Get(U5kTopOids.Oid_iAltavozLcen).Value;
            }
            set
            {
                SnmpIntObject.Get(U5kTopOids.Oid_iAltavozLcen).Value = value;
                TopSnmpAgent.Trap(U5kTopOids.Oid_iAltavozLcen, SnmpIntObject.Get(U5kTopOids.Oid_iAltavozLcen).Data, trap_manager);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public int AltavozHf
        {
            get
            {
                return SnmpIntObject.Get(U5kTopOids.Oid_iAltavozHf).Value;
            }
            set
            {
                SnmpIntObject.Get(U5kTopOids.Oid_iAltavozHf).Value = value;
                TopSnmpAgent.Trap(U5kTopOids.Oid_iAltavozHf, SnmpIntObject.Get(U5kTopOids.Oid_iAltavozHf).Data, trap_manager);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public int CableGrabacion
        {
            get
            {
                return SnmpIntObject.Get(U5kTopOids.Oid_iCableGrabacion).Value;
            }
            set
            {
                SnmpIntObject.Get(U5kTopOids.Oid_iCableGrabacion).Value = value;
                TopSnmpAgent.Trap(U5kTopOids.Oid_iCableGrabacion, SnmpIntObject.Get(U5kTopOids.Oid_iCableGrabacion).Data, trap_manager);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public int JackEjecutivo
        {
            get
            {
                return SnmpIntObject.Get(U5kTopOids.Oid_iJackEjecutivo).Value;
            }
            set
            {
                SnmpIntObject.Get(U5kTopOids.Oid_iJackEjecutivo).Value = value;
                TopSnmpAgent.Trap(U5kTopOids.Oid_iJackEjecutivo, SnmpIntObject.Get(U5kTopOids.Oid_iJackEjecutivo).Data, trap_manager);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public int JackAyudante
        {
            get
            {
                return SnmpIntObject.Get(U5kTopOids.Oid_iJackAyudante).Value;
            }
            set
            {
                SnmpIntObject.Get(U5kTopOids.Oid_iJackAyudante).Value = value;
                TopSnmpAgent.Trap(U5kTopOids.Oid_iJackAyudante, SnmpIntObject.Get(U5kTopOids.Oid_iJackAyudante).Data, trap_manager);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public int Panel
        {
            get
            {
                return SnmpIntObject.Get(U5kTopOids.Oid_iPanel).Value;
            }
            set
            {
                SnmpIntObject.Get(U5kTopOids.Oid_iPanel).Value = value;
                TopSnmpAgent.Trap(U5kTopOids.Oid_iPanel, SnmpIntObject.Get(U5kTopOids.Oid_iPanel).Data, trap_manager);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public int Lan1
        {
            get
            {
                return SnmpIntObject.Get(U5kTopOids.Oid_iLan1).Value;
            }
            set
            {
                SnmpIntObject.Get(U5kTopOids.Oid_iLan1).Value = value;
                TopSnmpAgent.Trap(U5kTopOids.Oid_iLan1, SnmpIntObject.Get(U5kTopOids.Oid_iLan1).Data, trap_manager);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public int Lan2
        {
            get
            {
                return SnmpIntObject.Get(U5kTopOids.Oid_iLan2).Value;
            }
            set
            {
                SnmpIntObject.Get(U5kTopOids.Oid_iLan2).Value = value;
                TopSnmpAgent.Trap(U5kTopOids.Oid_iLan2, SnmpIntObject.Get(U5kTopOids.Oid_iLan2).Data, trap_manager);
            }
        }

    }
}
