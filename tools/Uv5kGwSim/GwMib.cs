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

namespace Uv5kGwSim
{
    /// <summary>
    /// 
    /// </summary>
    class GwOids
    {
            // General de la Pasarela.
        public static string EstadoGw = ".1.1.100.2.0";
        public static string TipoSlot0=".1.1.100.31.1.1.0";    
        public static string TipoSlot1=".1.1.100.31.1.1.1";
        public static string TipoSlot2=".1.1.100.31.1.1.2";
        public static string TipoSlot3=".1.1.100.31.1.1.3";
        public static string EstadoSlot0=".1.1.100.31.1.2.0";
        public static string EstadoSlot1=".1.1.100.31.1.2.1";
        public static string EstadoSlot2=".1.1.100.31.1.2.2";
        public static string EstadoSlot3=".1.1.100.31.1.2.3";
        public static string PrincipalReserva="1.1.100.21.0";
        public static string EstadoLan = "1.1.100.22.0";

            // Para cada Recurso.
        public static string TipoRecurso=".1.1.100.100.0";
            
        public static string TipoRecursoRadio=".1.1.200";
        public static string EstadoRecursoRadio=".1.1.200.2.0";         
            
        public static string TipoRecursoLC=".1.1.300";
        public static string EstadoRecursoLC=".1.1.300.2.0";
            
        public static string TipoRecursoTF=".1.1.400";
        public static string EstadoRecursoTF=".1.1.400.2.0";
            
        public static string TipoRecursoATS=".1.1.500";
        public static string EstadoRecursoATS = ".1.1.500.2.0";

    }

    /// <summary>
    /// 
    /// </summary>
    class GwMib
    {
        /// <summary>
        /// Todos los objectos...
        /// </summary>
        public List<MibObject> smib = new List<MibObject>();
        /// <summary>
        /// 
        /// </summary>
        MibObject[] mib_prv = new MibObject[]
        {
            // Parte Privada.            
            new SnmpIntObject(GwOids.EstadoGw, 1),     // 
            new SnmpIntObject(GwOids.TipoSlot0, 2),     // 
            new SnmpIntObject(GwOids.TipoSlot1, 2),     // 
            new SnmpIntObject(GwOids.TipoSlot2, 2),     // 
            new SnmpIntObject(GwOids.TipoSlot3, 2),     // 
            new SnmpIntObject(GwOids.EstadoSlot0, 0x9),     // El primer bit es el estado de la tarjeta.
            new SnmpIntObject(GwOids.EstadoSlot1, 0x17),     // 
            new SnmpIntObject(GwOids.EstadoSlot2, 0x0F),     // 
            new SnmpIntObject(GwOids.EstadoSlot3, 0x03),     // 
            new SnmpIntObject(GwOids.PrincipalReserva, 0),      // 
            new SnmpIntObject(GwOids.EstadoLan, 7),             // 7
        };
        /// <summary>
        /// 
        /// </summary>
        IPEndPoint trap_manager = null;
        /// <summary>
        /// 
        /// </summary>
        GwSnmpAgent _agent = null;
        /// <summary>
        /// 
        /// </summary>
        public GwMib()
        {
        }
        /// <summary>
        /// 
        /// </summary>
        public void Load(GwSnmpAgent agent, string ipserver, int portserver)
        {
            trap_manager = new IPEndPoint(IPAddress.Parse(ipserver), portserver);
            _agent = agent;

            // Carga los elementos discretos.
            smib.Clear();
            foreach (MibObject obj in mib_prv)
            {
                smib.Add(obj);
            }

            // Ordeno OID y cargo el agente
            smib.Sort();
            foreach (MibObject obj in smib)
            {
                _agent.Store.Add(obj);
            }
        }
        #region Propiedades Generales

        /// <summary>
        /// 
        /// </summary>
        public int EstadoGw
        {
            get
            {
                return SnmpIntObject.Get(_agent, GwOids.EstadoGw).Value;
            }
            set
            {
                SnmpIntObject.Get(_agent, GwOids.EstadoGw).Value = value;
                GwSnmpAgent.Trap(GwOids.EstadoGw, SnmpIntObject.Get(_agent, GwOids.EstadoGw).Data, trap_manager);
            }
        }
        /// <summary>
        /// 
        /// </summary>
        public int TipoSlot0
        {
            get
            {
                return SnmpIntObject.Get(_agent, GwOids.TipoSlot0).Value;
            }
            set
            {
                SnmpIntObject.Get(_agent, GwOids.TipoSlot0).Value = value;
                GwSnmpAgent.Trap(GwOids.TipoSlot0, SnmpIntObject.Get(_agent, GwOids.TipoSlot0).Data, trap_manager);
            }
        }
        /// <summary>
        /// 
        /// </summary>
        public int TipoSlot1
        {
            get
            {
                return SnmpIntObject.Get(_agent, GwOids.TipoSlot1).Value;
            }
            set
            {
                SnmpIntObject.Get(_agent, GwOids.TipoSlot1).Value = value;
                GwSnmpAgent.Trap(GwOids.TipoSlot1, SnmpIntObject.Get(_agent, GwOids.TipoSlot1).Data, trap_manager);
            }
        }
        /// <summary>
        /// 
        /// </summary>
        public int TipoSlot2
        {
            get
            {
                return SnmpIntObject.Get(_agent, GwOids.TipoSlot2).Value;
            }
            set
            {
                SnmpIntObject.Get(_agent, GwOids.TipoSlot2).Value = value;
                GwSnmpAgent.Trap(GwOids.TipoSlot2, SnmpIntObject.Get(_agent, GwOids.TipoSlot2).Data, trap_manager);
            }
        }
        /// <summary>
        /// 
        /// </summary>
        public int TipoSlot3
        {
            get
            {
                return SnmpIntObject.Get(_agent, GwOids.TipoSlot3).Value;
            }
            set
            {
                SnmpIntObject.Get(_agent, GwOids.TipoSlot3).Value = value;
                GwSnmpAgent.Trap(GwOids.TipoSlot3, SnmpIntObject.Get(_agent, GwOids.TipoSlot3).Data, trap_manager);
            }
        }
        /// <summary>
        /// 
        /// </summary>
        public int EstadoSlot0
        {
            get
            {
                return SnmpIntObject.Get(_agent, GwOids.EstadoSlot0).Value;
            }
            set
            {
                SnmpIntObject.Get(_agent, GwOids.EstadoSlot0).Value = value;
                GwSnmpAgent.Trap(GwOids.EstadoSlot0, SnmpIntObject.Get(_agent, GwOids.EstadoSlot0).Data, trap_manager);
            }
        }
        /// <summary>
        /// 
        /// </summary>
        public int EstadoSlot1
        {
            get
            {
                return SnmpIntObject.Get(_agent, GwOids.EstadoSlot1).Value;
            }
            set
            {
                SnmpIntObject.Get(_agent, GwOids.EstadoSlot1).Value = value;
                GwSnmpAgent.Trap(GwOids.EstadoSlot1, SnmpIntObject.Get(_agent, GwOids.EstadoSlot1).Data, trap_manager);
            }
        }
        /// <summary>
        /// 
        /// </summary>
        public int EstadoSlot2
        {
            get
            {
                return SnmpIntObject.Get(_agent, GwOids.EstadoSlot2).Value;
            }
            set
            {
                SnmpIntObject.Get(_agent, GwOids.EstadoSlot2).Value = value;
                GwSnmpAgent.Trap(GwOids.EstadoSlot2, SnmpIntObject.Get(_agent, GwOids.EstadoSlot2).Data, trap_manager);
            }
        }
        /// <summary>
        /// 
        /// </summary>
        public int EstadoSlot3
        {
            get
            {
                return SnmpIntObject.Get(_agent, GwOids.EstadoSlot3).Value;
            }
            set
            {
                SnmpIntObject.Get(_agent, GwOids.EstadoSlot3).Value = value;
                GwSnmpAgent.Trap(GwOids.EstadoSlot3, SnmpIntObject.Get(_agent, GwOids.EstadoSlot3).Data, trap_manager);
            }
        }
        /// <summary>
        /// 
        /// </summary>
        public int PrincipalReserva
        {
            get
            {
                return SnmpIntObject.Get(_agent, GwOids.PrincipalReserva).Value;
            }
            set
            {
                SnmpIntObject.Get(_agent, GwOids.PrincipalReserva).Value = value;
                GwSnmpAgent.Trap(GwOids.PrincipalReserva, SnmpIntObject.Get(_agent, GwOids.PrincipalReserva).Data, trap_manager);
            }
        }
        /// <summary>
        /// 
        /// </summary>
        public int EstadoLan
        {
            get
            {
                return SnmpIntObject.Get(_agent, GwOids.EstadoLan).Value;
            }
            set
            {
                SnmpIntObject.Get(_agent, GwOids.EstadoLan).Value = value;
                GwSnmpAgent.Trap(GwOids.EstadoLan, SnmpIntObject.Get(_agent, GwOids.EstadoLan).Data, trap_manager);
            }
        }

        #endregion
    }

    /// <summary>
    /// MIB de un Recurso...
    /// </summary>
    class GwResMib
    {
        /// <summary>
        /// 
        /// </summary>
        public class GwResOids
        {
            public static string TipoRecurso = ".1.1.100.100.0";

            public static string TipoRecursoRadio = ".1.1.200";
            public static string EstadoRecursoRadio = ".1.1.200.2.0";

            public static string TipoRecursoLC = ".1.1.300";
            public static string EstadoRecursoLC = ".1.1.300.2.0";

            public static string TipoRecursoTF = ".1.1.400";
            public static string EstadoRecursoTF = ".1.1.400.2.0";

            public static string TipoRecursoATS = ".1.1.500";
            public static string EstadoRecursoATS = ".1.1.500.2.0";
        }
        /// <summary>
        /// 
        /// </summary>
        public List<MibObject> smib = new List<MibObject>();
        /// <summary>
        /// 
        /// </summary>
        MibObject[] mib_prv = new MibObject[]
        {
            // Parte Privada.            
            new SnmpIntObject(GwResOids.TipoRecurso, 0),
            new SnmpIntObject(GwResOids.TipoRecursoRadio, 0),
            new SnmpIntObject(GwResOids.EstadoRecursoRadio, 0),
            new SnmpIntObject(GwResOids.TipoRecursoTF, 0),
            new SnmpIntObject(GwResOids.EstadoRecursoTF, 0),
            new SnmpIntObject(GwResOids.TipoRecursoLC, 0),
            new SnmpIntObject(GwResOids.EstadoRecursoLC, 0),
            new SnmpIntObject(GwResOids.TipoRecursoATS, 0),
            new SnmpIntObject(GwResOids.EstadoRecursoATS, 0) 
        };
        /// <summary>
        /// 
        /// </summary>
        IPEndPoint trap_manager = null;
        /// <summary>
        /// 
        /// </summary>
        GwSnmpAgent _agent = null;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="agent"></param>
        /// <param name="ipserver"></param>
        /// <param name="portserver"></param>
        public void Load(GwSnmpAgent agent, string ipserver, int portserver)
        {
            trap_manager = new IPEndPoint(IPAddress.Parse(ipserver), portserver);
            _agent = agent;

            // Carga los elementos discretos.
            smib.Clear();
            foreach (MibObject obj in mib_prv)
            {
                smib.Add(obj);
            }

            // Ordeno OID y cargo el agente
            smib.Sort();
            foreach (MibObject obj in smib)
            {
                _agent.Store.Add(obj);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="oid"></param>
        /// <returns></returns>
        public int IntOidGet(string oid)
        {
            return SnmpIntObject.Get(_agent, oid).Value;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="oid"></param>
        /// <param name="valor"></param>
        public void IntOidSet(string oid, int valor)
        {
            SnmpIntObject.Get(_agent, oid).Value = valor;
        }
    }
}
