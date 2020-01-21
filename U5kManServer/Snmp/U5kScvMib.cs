using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Lextm.SharpSnmpLib;
using Lextm.SharpSnmpLib.Pipeline;
using Lextm.SharpSnmpLib.Messaging;
using Lextm.SharpSnmpLib.Security;
using Lextm.SharpSnmpLib.Objects;

using Newtonsoft.Json;

using NucleoGeneric;

namespace U5kManServer 
{
    /** */
    public enum mib2OperStatus { down = 2, up = 1, testing = 3 }
    public enum mib2AdminStatus { down = 2, up = 1, testing = 3 }
    static class MibHelper
    {
        static public mib2OperStatus std2mib2OperStatus(std estado)
        {
            switch (estado)
            {
                case std.Error:
                case std.NoInfo:
                    return mib2OperStatus.down;
            }
            return mib2OperStatus.up;
        }
        static public int Seleccionado2stdext(sel Seleccionado)
        {
            return Seleccionado == sel.NoInfo ? (int)std.NoInfo :
                Seleccionado == sel.NoSeleccionado ? 8 : 7;
        }
        static public string stdlans2snmp( std[] par)
        {
            byte[] ret = new byte[] { 
                (byte)((int)std.NoExiste+'0'), 
                (byte)((int)std.NoExiste+'0'), 
                (byte)((int)std.NoExiste+'0'), 
                (byte)((int)std.NoExiste+'0'), 
                (byte)((int)std.NoExiste+'0'), 
                (byte)((int)std.NoExiste+'0'), 
                (byte)((int)std.NoExiste+'0'), 
                (byte)((int)std.NoExiste+'0')
            };            
            int iret = 0;
            foreach (std estado in par)
            {
                if (iret < ret.Length)
                {
                    ret[iret++] = (byte )((int)estado + '0');
                }
            }
            return Encoding.ASCII.GetString(ret).ToLower();
        }
        static public string stdcards(stdGw cpua)
        {
            byte[] ret = new byte[6];

            ret[0] = (byte )((int)cpua.gwA.std+'0');
            for (int card = 0; card < 4; card++)
            {
                if (cpua.cpu_activa.slots[card].std_online == std.Ok)
                {
                    ret[card + 1] = (byte)((int)std.Ok + '0');
                }
                else
                {
                    ret[card + 1] = (byte)('0');
                }
            }
            ret[5] = (byte)((int)cpua.gwB.std + '0');

            return Encoding.ASCII.GetString(ret).ToLower();
        }
    }

    /// <summary>
    /// MTTO V1/V2. Definicion de OIDS
    /// </summary>
    class U5kManMibOids
    {

        /// <summary>
        /// 
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        static string GetOid(string id, string _default)
        {
            foreach (string s in Properties.u5kManServer.Default.ScvOids)
            {
                string[] p = s.Split(':');
                if (p.Count() == 2 && p[0] == id)
                    return p[1];
            }

            return _default;
        }

        /** Parte RFC1213 MIB-2 */
        static public string MIB2sysDescr = ".1.3.6.1.2.1.1.1.0";           
        static public string MIB2sysObjectID = ".1.3.6.1.2.1.1.2.0";        
        static public string MIB2sysUpTime = ".1.3.6.1.2.1.1.3.0";
        static public string MIB2sysContact = ".1.3.6.1.2.1.1.4.0";
        static public string MIB2sysName = ".1.3.6.1.2.1.1.5.0";
        static public string MIB2sysLocation = ".1.3.6.1.2.1.1.6.0";
        static public string MIB2sysServices = ".1.3.6.1.2.1.1.7.0";

        static public string MIB2interfacesifNumber = ".1.3.6.1.2.1.2.1.0"; 
        static public string MIB2interfacesifTable = ".1.3.6.1.2.1.2.2.1";  
#if _MTTO_V1_
        /** Parte privada. Version-1*/
        static public string U5KDualLAN = ".1.3.6.1.4.1.7916.8.1.1.1.0";
        static public string U5KServidorDual = ".1.3.6.1.4.1.7916.8.1.1.2.0";
        static public string U5KLan1 =  ".1.3.6.1.4.1.7916.8.1.1.3.0";
        static public string U5KLan2 =  ".1.3.6.1.4.1.7916.8.1.1.4.0";
        static public string U5KServ1 = ".1.3.6.1.4.1.7916.8.1.1.5.0";
        static public string U5KServ2 = ".1.3.6.1.4.1.7916.8.1.1.6.0";
        static public string U5KNtp = ".1.3.6.1.4.1.7916.8.1.1.7.0";
        static public string U5KSacta1 = ".1.3.6.1.4.1.7916.8.1.1.8.0";
        static public string U5KSacta2 = ".1.3.6.1.4.1.7916.8.1.1.9.0";

        static public string U5KNPuestos = ".1.3.6.1.4.1.7916.8.1.2.1.0";
        static public string U5KPuestos =  ".1.3.6.1.4.1.7916.8.1.2.2.1";

        static public string U5KNRadios = ".1.3.6.1.4.1.7916.8.1.3.1.0";
        static public string U5KRadios =  ".1.3.6.1.4.1.7916.8.1.3.2.1";

        static public string U5KNLineas = ".1.3.6.1.4.1.7916.8.1.4.1.0";
        static public string U5KLineas =  ".1.3.6.1.4.1.7916.8.1.4.2.1";
#endif
        /** Parte Privada. Version-2 */
        static public string MttoV2Oid_base = ".1.3.6.1.4.1.7916.8.1.5";
        /** Grupo Config */
        static public string MttoV2Oid_ServidorDual =   MttoV2Oid_base + ".1.1.0";
        static public string MttoV2Oid_HayPbx =         MttoV2Oid_base + ".1.2.0";
        static public string MttoV2Oid_HaySacta =       MttoV2Oid_base + ".1.3.0";
        static public string MttoV2Oid_HayNtp =         MttoV2Oid_base + ".1.4.0";

        /** Grupo StdGral */
        static public string MttoV2Oid_Version =        MttoV2Oid_base + ".2.1.0";
        static public string MttoV2Oid_CfgVersion =     MttoV2Oid_base + ".2.2.0";
        static public string MttoV2Oid_StdgTbNum =      MttoV2Oid_base + ".2.3.1.0";
        static public string MttoV2Oid_StdgTbBase =     MttoV2Oid_base + ".2.3.2.1";

        /** Tabla de Puestos */
        static public string MttoV2Oid_PuestosStdg =    MttoV2Oid_base + ".3.1.0";
        static public string MttoV2Oid_PuestosTbNum =   MttoV2Oid_base + ".3.2.1.0";
        static public string MttoV2Oid_PuestosTbBase =  MttoV2Oid_base + ".3.2.2.1";

        /** Pasarelas y Tablas de Interfaz LEGACY */
        static public string MttoV2Oid_GwsStdg =        MttoV2Oid_base + ".4.1.0";
        static public string MttoV2Oid_GwsTbNum =       MttoV2Oid_base + ".4.2.1.0";
        static public string MttoV2Oid_GwsTbBase =      MttoV2Oid_base + ".4.2.2.1";
        static public string MttoV2Oid_RadioTbNum =     MttoV2Oid_base + ".4.3.1.0";
        static public string MttoV2Oid_RadioTbBase =    MttoV2Oid_base + ".4.3.2.1";
        static public string MttoV2Oid_TelefTbNum =     MttoV2Oid_base + ".4.4.1.0";
        static public string MttoV2Oid_TelefTbBase =    MttoV2Oid_base + ".4.4.2.1";

        /** Equipos Externos Radio / Telefonia */
        static public string MttoV2Oid_ExtRadioTbNum =  MttoV2Oid_base + ".5.1.1.0";
        static public string MttoV2Oid_ExtRadioTbBase = MttoV2Oid_base + ".5.1.2.1";
        static public string MttoV2Oid_ExtTelefTbNum =  MttoV2Oid_base + ".5.2.1.0";
        static public string MttoV2Oid_ExtTelefTbBase = MttoV2Oid_base + ".5.2.2.1";
        static public string MttoV2Oid_ExtRecsTbNum   = MttoV2Oid_base + ".5.3.1.0";
        static public string MttoV2Oid_ExtRecsTbBase  = MttoV2Oid_base + ".5.3.2.1";

        /** PBX y Abonados */
        static public string MttoV2Oid_PbxStd =         MttoV2Oid_base + ".6.1.0";
        static public string MttoV2Oid_PbxAbTbNum =     MttoV2Oid_base + ".6.2.1.0";
        static public string MttoV2Oid_PbxAbTbBase =    MttoV2Oid_base + ".6.2.2.1";

        /** 20170220. */
        /** Operativa Radio y Gestor M+N */
        static public string MttoV2Oid_RadioSesNum =    MttoV2Oid_base + ".7.1.1.0";
        static public string MttoV2Oid_RadioSesBase =   MttoV2Oid_base + ".7.1.2.1";
        static public string MttoV2Oid_RadioMnNum =     MttoV2Oid_base + ".7.2.1.0";
        static public string MttoV2Oid_RadioMnBase =    MttoV2Oid_base + ".7.2.2.1";

    };

    /// <summary>
    /// MTTO-V1/V2. Clase Base para Tablas...
    /// </summary>
    public class U5kSnmpTable : TableObject
    {
        IList<MibObject> _elements = new List<MibObject>();
        protected string _oid_base = "";
        protected int _nrow = 0;

        /// <summary>
        /// 
        /// </summary>
        public int Rows { get { return _nrow; } }

        /// <summary>
        /// 
        /// </summary>
        protected override IEnumerable<ScalarObject> Objects
        {
            get { return _elements; }
        }

        /// <summary>
        /// 
        /// </summary>
        public IList<MibObject> MibObjects
        {
            get
            {
                return _elements; ;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="oid_base"></param>
        public U5kSnmpTable(string oid_base)
        {
            _oid_base = oid_base;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="cols"></param>
        protected void AddRow(params object[] cols)
        {
            int index = 1 + _nrow;
            int col = 1;

            _elements.Add(new SnmpIntObject(string.Format("{0:000}.{1:000}.{2:000}",_oid_base,col++,index), index));
            foreach(object obj in cols)
            {
                if (obj is int)
                    _elements.Add(new SnmpIntObject(string.Format("{0:000}.{1:000}.{2:000}", _oid_base, col++, index), (int)obj));
                else if (obj is string)
                    _elements.Add(new SnmpStringObject(string.Format("{0:000}.{1:000}.{2:000}", _oid_base, col++, index), (string)obj));
                else
                    _elements.Add(new SnmpIntObject(string.Format("{0:000}.{1:000}.{2:000}", _oid_base, col++, index), (int)obj));
            }

            _nrow++;
        }

        /// <summary>
        /// 
        /// </summary>
        public void Load(ref List<MibObject> list)
        {
            foreach (MibObject obj in Objects)
            {
                list.Add(obj);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="name"></param>
        /// <param name="colname"></param>
        /// <param name="colstd"></param>
        /// <param name="nItems"></param>
        /// <param name="oids"></param>
        /// <returns></returns>
        protected bool FindItem(string name, int colname, int colstd, ref int que_row)
        {
            int row_libre = -1;
            for (int row = 0; row < _nrow; row++)
            {
                string oid_name = string.Format("{0:000}.{1:000}.{2:000}", _oid_base, colname, row+1);
                string oid_std = string.Format("{0:000}.{1:000}.{2:000}", _oid_base, colstd, row+1);

                string val_name = SnmpStringObject.Get(oid_name).Value;
                int estado = SnmpIntObject.Get(oid_std).Value;

                if (val_name == name)
                {
                    que_row = row+1;
                    return true;
                }
                else if (row_libre == -1 && estado == -1)        // Todo. Estado Inicial debe Ser -1.
                {
                    row_libre = row+1;
                }
            }

            if (row_libre != -1)
            {
                que_row = row_libre;
                return true;
            }
            return false;
        }

    }

    /// <summary>
    /// MTTO-V1/V2. Tabla MIB-2 de Interfaces.
    /// </summary>
    public class MIB2interfacesifTable : U5kSnmpTable
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="nItf"></param>
        public MIB2interfacesifTable(int nPos)
            : base(U5kManMibOids.MIB2interfacesifTable)    
        {
            AddRow("Telephone", 1, (int)mib2AdminStatus.up, (int)mib2OperStatus.down);
            AddRow("Radio", 1, (int)mib2AdminStatus.up, (int)mib2OperStatus.down);
            AddRow("Recorder", 1, (int)mib2AdminStatus.up, (int)mib2OperStatus.down);
            for (int i=0; i<nPos; i++)
                AddRow("Position " + (i + 1).ToString(), 1, (int)mib2AdminStatus.up, (int)mib2OperStatus.down);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="npos"></param>
        /// <param name="std"></param>
        public void SetOperStatusPosicion(int npos, mib2OperStatus std)
        {
            SnmpIntObject.Get(string.Format("{0:000}.{1:000}.{2:000}", _oid_base, 8, 4 + npos)).Value = (int)std;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="npos"></param>
        /// <param name="name"></param>
        public void SetNamePosicion(int npos, string name)
        {
            SnmpStringObject.Get(string.Format("{0:000}.{1:000}.{2:000}", _oid_base, 2, 4 + npos)).Value = String.Format("Position {0} ({1})", npos+1, name);
        }
        /// <summary>
        /// 
        /// </summary>
        public mib2OperStatus OperStatusTelefonia
        {
            set
            {
                SnmpIntObject.Get(string.Format("{0:000}.{1:000}.{2:000}", _oid_base, 8, 1)).Value = (int)value;
            }
        }
        /// <summary>
        /// 
        /// </summary>
        public mib2OperStatus OperStatusRadio
        {
            set
            {
                SnmpIntObject.Get(string.Format("{0:000}.{1:000}.{2:000}", _oid_base, 8, 2)).Value = (int)value;
            }
        }
        /// <summary>
        /// 
        /// </summary>
        public mib2OperStatus OperStatusRecorder
        {
            set
            {
                SnmpIntObject.Get(string.Format("{0:000}.{1:000}.{2:000}", _oid_base, 8, 3)).Value = (int)value;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="desc"></param>
        /// <param name="ads"></param>
        /// <param name="ops"></param>
        protected void AddRow(string desc, int tipo, int ads, int ops)
        {
            object[] row_objects = new object[]
                {
                    desc, tipo, 0, 0, 0, ads, ops
                };
            AddRow(row_objects);
        }
    }

#if _MTTO_V1_
    /// <summary>
    /// MTTO-V1. Tabla de Puestos.
    /// </summary>    
    public class U5kTablaPuestos : U5kSnmpTable
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="npos"></param>
        public U5kTablaPuestos(int npos)
            : base(U5kManMibOids.U5KPuestos)
        {
            for (int pos = 0; pos < npos; pos++)
                AddRow("Puesto " + pos.ToString(), -1);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="puesto"></param>
        /// <param name="oids"></param>
        /// <returns></returns>
        public bool FindPuesto(string puesto, ref string[] oids)
        {
            int row = -1;
            if (FindItem(puesto, 2, 3, ref row))
            {
                oids[0] = string.Format("{0:000}.{1:000}.{2:000}", _oid_base, 2, row);
                oids[1] = string.Format("{0:000}.{1:000}.{2:000}", _oid_base, 3, row);
                return true;
            } 
            return false;
        }

    }

    /// <summary>
    /// MTTO-V1. Table de Interfaces Radio LEGACY
    /// </summary>    
    public class U5kTablaRadio : U5kSnmpTable
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="npos"></param>
        public U5kTablaRadio(int npos)
            : base(U5kManMibOids.U5KRadios)
        {
            for (int pos = 0; pos < npos; pos++)
                AddRow("Radio " + pos.ToString(), -1);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="radio"></param>
        /// <param name="oids"></param>
        /// <returns></returns>
        public bool FindRadio(string radio, ref string[] oids)
        {
            int row = -1;
            if (FindItem(radio, 2, 3, ref row))
            {
                oids[0] = string.Format("{0:000}.{1:000}.{2:000}", _oid_base, 2, row);
                oids[1] = string.Format("{0:000}.{1:000}.{2:000}", _oid_base, 3, row);
                return true;
            }
            return false;
        }

    }

    /// <summary>
    /// MTTO-V1. Tabla de Interfaces Telefonicas LEGACY
    /// </summary>    
    public class U5kTablaTlf : U5kSnmpTable
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="npos"></param>
        public U5kTablaTlf(int npos)
            : base(U5kManMibOids.U5KLineas)
        {
            for (int pos = 0; pos < npos; pos++)
                AddRow("Telefonia " + pos.ToString(), 0, -1);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="linea"></param>
        /// <param name="oids"></param>
        /// <returns></returns>
        public bool FindLinea(string linea, ref string[] oids)
        {
            int row = -1;
            if (FindItem(linea, 2, 4, ref row))
            {
                oids[0] = string.Format("{0:000}.{1:000}.{2:000}", _oid_base, 2, row);
                oids[1] = string.Format("{0:000}.{1:000}.{2:000}", _oid_base, 3, row);
                oids[2] = string.Format("{0:000}.{1:000}.{2:000}", _oid_base, 4, row);
                return true;
            }
            return false;
        }
    }
#endif

    /// <summary>
    /// Clase Raiz para la supervision de Subsistemas de la version 2 de la MIB
    /// </summary>
    public abstract class U5kMttoV2Tb : U5kSnmpTable
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="oid_base"></param>
        public U5kMttoV2Tb(string oid_base, int ncol)
            : base(oid_base)
        {
            _ncol = ncol + 1;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="nrow">Fila en Rango 0...</param>
        /// <param name="ncol">Columna en Rango 0..</param>
        /// <returns></returns>
        public ScalarObject ElementAt(int row, int col)
        {
            int index = row * _ncol + col + 1;
            if (index < Objects.Count())
            {
                return MibObjects[index];
            }
            throw new Exception("SnmpAgent Exception. Elemento de Tabla fuera de rango..");
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="row"></param>
        /// <param name="cols"></param>
        protected void mod_element(int row, params object[] cols)
        {
            for (int col = 0; col < cols.Length; col++)
            {
                ScalarObject obj = ElementAt(row, col);
                Type tipo = obj.GetType();
                if (tipo.Name == "SnmpStringObject")
                {
                    ((SnmpStringObject)obj).Value = (string)cols[col];
                }
                else if (tipo.Name == "SnmpIntObject")
                {
                    ((SnmpIntObject)obj).Value = (int)cols[col];
                }             
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        protected bool Find(string key, ref int que_row)
        {
            for (int row = 0; row < _nrow; row++)
            {
                SnmpStringObject obj = (SnmpStringObject)ElementAt(row, _ncol - 2);       // El ultimo elemento es la KEY
                if (key == obj.Value)
                {
                    que_row = row;
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// 
        /// </summary>
        abstract public void Tick();

        int _ncol;
    }
    /// <summary>
    /// MTTO-V2. Tabla de Parámetros Generales.
    /// </summary>
    public class U5kStdGenTb : U5kMttoV2Tb
    {
        public U5kStdGenTb()
            : base(U5kManMibOids.MttoV2Oid_StdgTbBase, 5)
        {
            AddRow("Servidor 1", "127.0.0.1", std.NoInfo, "00000000", "---");
            AddRow("Servidor 2", "127.0.0.1", std.NoInfo, "00000000", "---");
            AddRow("Servidor Radio", "127.0.0.1", std.NoInfo, "00000000", "---");
            AddRow("PBX", "127.0.0.1", std.NoInfo, "00000000", "---");
            AddRow("NTP", "127.0.0.1", std.NoInfo, "00000000", "---");
            AddRow("SACTA 1", "127.0.0.1", std.NoInfo, "00000000", "---");
            AddRow("SACTA 2", "127.0.0.1", std.NoInfo, "00000000", "---");
#if !_HAY_NODEBOX__
            AddRow("Servidor Telefonia", "127.0.0.1", std.NoInfo, "00000000", "---");
#endif
        }

        /// <summary>
        /// 
        /// </summary>
        public override void Tick()
        {
            try
            {
                if (Rows >= 6)
                {
#if STD_ACCESS_V0
                    lock (U5kManService._std._gen)
                    {
                        U5KStdGeneral sg = U5kManService._std._gen;
#else
                        U5KStdGeneral sg = U5kManService.GlobalData.STDG;
#endif
                        /** Datos de Servidor-1 */
                        ((SnmpStringObject)(ElementAt(0, 1))).Value = sg.stdServ1.name;
                        ((SnmpIntObject)(ElementAt(0, 2))).Value = MibHelper.Seleccionado2stdext(sg.stdServ1.Seleccionado);
                        ((SnmpStringObject)(ElementAt(0, 3))).Value = MibHelper.stdlans2snmp(sg.stdServ1.lanes.Values.ToArray());
                        ((SnmpStringObject)(ElementAt(0, 4))).Value = sg.stdServ1.ntp_sync;

                        /** Datos de Servidor-2 */
                        ((SnmpStringObject)(ElementAt(1, 1))).Value = sg.stdServ2.name;
                        ((SnmpIntObject)(ElementAt(1, 2))).Value = MibHelper.Seleccionado2stdext(sg.stdServ2.Seleccionado);
                        ((SnmpStringObject)(ElementAt(1, 3))).Value = MibHelper.stdlans2snmp(sg.stdServ2.lanes.Values.ToArray());
                        ((SnmpStringObject)(ElementAt(1, 4))).Value = sg.stdServ2.ntp_sync;

                    /** Datos de NBX */
#if _HAY_NODEBOX__
#if _LISTANBX_V0
                        StdServ masternbx = sg.lstNbx.Find(x => x.Seleccionado == sel.Seleccionado);
                        ((SnmpStringObject)(ElementAt(2, 1))).Value = masternbx != null ? masternbx.name : "???";
                        ((SnmpIntObject)(ElementAt(2, 2))).Value = (int)(masternbx != null ? (sg.lstNbx.Count > 1 ? masternbx.Estado : std.Aviso) : std.NoInfo);
#elif _LISTANBX_
                    U5KStdGeneral.StdNbx masternbx = sg.lstNbx.Find(n => n.CfgService == U5KStdGeneral.NbxServiceState.Master);
                        ((SnmpStringObject)(ElementAt(2, 1))).Value = masternbx != null ? masternbx.ip : "???";
                        ((SnmpIntObject)(ElementAt(2, 2))).Value = (int)(masternbx != null ? (sg.lstNbx.Count > 1 ? std.Ok : std.Aviso) : std.NoInfo);
#endif
#else
                    Services.CentralServicesMonitor.Monitor.DataGetForSnmpAgent((idRadio, stdRadio, idPhone, stdPhone) =>
                    {
                        ((SnmpStringObject)(ElementAt(2, 1))).Value = idRadio;
                        ((SnmpIntObject)(ElementAt(2, 2))).Value = (int)(stdRadio == "Ok" ? std.Ok : stdRadio == "Warning" ? std.Aviso : std.Alarma);

                        ((SnmpStringObject)(ElementAt(7, 1))).Value = idPhone;
                        ((SnmpIntObject)(ElementAt(7, 2))).Value = (int)(stdPhone == "Ok" ? std.Ok : stdPhone == "Warning" ? std.Aviso : std.Alarma);
                    });
#endif
                    /** Datos de PABX */
                    ((SnmpStringObject)(ElementAt(3, 1))).Value = sg.stdPabx.name;
                        ((SnmpIntObject)(ElementAt(3, 2))).Value = (int)sg.stdPabx.Estado;

                        /** Datos de NTP */
                        ((SnmpStringObject)(ElementAt(4, 1))).Value = sg.stdClock.name;
                        ((SnmpIntObject)(ElementAt(4, 2))).Value = (int)sg.stdClock.Estado;

                        /** Datos de SACTA-1 */
                        ((SnmpStringObject)(ElementAt(5, 1))).Value = "sacta-1";
                        ((SnmpIntObject)(ElementAt(5, 2))).Value = (int)sg.stdSacta1;

                        /** Datos de SACTA-2 */
                        ((SnmpStringObject)(ElementAt(6, 1))).Value = "sacta-2";
                        ((SnmpIntObject)(ElementAt(6, 2))).Value = (int)sg.stdSacta2;
                        
                        /** */
                        SnmpStringObject.Get(U5kManMibOids.MttoV2Oid_CfgVersion).Value = sg.CfgId;
#if STD_ACCESS_V0
                    }
#else
#endif
                }
            }
            catch (Exception x)
            {
                BaseCode.LogException<U5kStdGenTb>( "", x);
            }
        }
    }
    /// <summary>
    /// MTTO-V2. Puestos.
    /// </summary>
    public class U5kPuestosTb : U5kMttoV2Tb
    {
        public U5kPuestosTb()
            : base(U5kManMibOids.MttoV2Oid_PuestosTbBase, 6)
        {
#if STD_ACCESS_V0
            foreach (stdPos pos in U5kManService._std.stdpos)
#else
            List<stdPos> stdpos = U5kManService.GlobalData.STDTOPS;
            foreach (stdPos pos in stdpos)
#endif
            {
                AddRow(pos.name, pos.ip, pos.stdpos, "--------", "Estado NTP", "Lista de URIS");
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public override void Tick()
        {
            // Estado de cada uno de los puestos...
            int row = 0;
#if STD_ACCESS_V0
            lock (U5kManService._std.stdpos)
            {
                foreach (stdPos pos in U5kManService._std.stdpos)
                {
                    ((SnmpIntObject)(ElementAt(row, 2))).Value = (int)pos.stdpos;
                    ((SnmpStringObject)(ElementAt(row, 3))).Value = MibHelper.stdlans2snmp(new std[] { pos.lan1, pos.lan2 });
                    ((SnmpStringObject)(ElementAt(row, 4))).Value = pos.status_sync;      // Estado Cliente NTP.
                    ((SnmpStringObject)(ElementAt(row, 5))).Value = pos.uris==null ? "null" : String.Join("##", pos.uris.ToArray());
                    row++;
                }
            }
#else
            List<stdPos> stdpos = U5kManService.GlobalData.STDTOPS;
            foreach (stdPos pos in stdpos)
            {
                ((SnmpIntObject)(ElementAt(row, 2))).Value = (int)pos.stdpos;
                ((SnmpStringObject)(ElementAt(row, 3))).Value = MibHelper.stdlans2snmp(new std[] { pos.lan1, pos.lan2 });
                ((SnmpStringObject)(ElementAt(row, 4))).Value = pos.status_sync;      // Estado Cliente NTP.
                ((SnmpStringObject)(ElementAt(row, 5))).Value = pos.uris == null ? "null" : String.Join("##", pos.uris.ToArray());
                row++;
            }
#endif
            // Estado General...
#if STD_ACCESS_V0
            lock (U5kManService._std._gen)
            {
                SnmpIntObject.Get(U5kManMibOids.MttoV2Oid_PuestosStdg).Value = (int)U5kManService._std._gen.stdGlobalPos;
            }
#else
            SnmpIntObject.Get(U5kManMibOids.MttoV2Oid_PuestosStdg).Value = (int)U5kManService.GlobalData.STDG.stdGlobalPos;
#endif
        }
    }
    /// <summary>
    /// MTTO-V2. Pasarelas.
    /// </summary>
    public class U5kGwsTb : U5kMttoV2Tb
    {
        public U5kGwsTb()
            : base(U5kManMibOids.MttoV2Oid_GwsTbBase, 6)
        {
#if STD_ACCESS_V0
            List<stdGw> stdgws = U5kManService._std.stdgws;
#else
            List<stdGw> stdgws = U5kManService.GlobalData.STDGWS;
#endif
            foreach (stdGw gw in stdgws)
            {
                AddRow(gw.name, gw.ip, gw.std, "--------", "---", "******");
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public override void Tick()
        {
            // Estado de las pasarelas.
            int row = 0;
#if STD_ACCESS_V0
            lock (U5kManService._std.stdgws)
            {
                foreach (stdGw gw in U5kManService._std.stdgws)
                {
                    ((SnmpIntObject)(ElementAt(row, 2))).Value = (int)gw.std;
                    ((SnmpStringObject)(ElementAt(row, 3))).Value = MibHelper.stdlans2snmp(new std[] { gw.cpu_activa.lan1, gw.cpu_activa.lan2 });
                    ((SnmpStringObject)(ElementAt(row, 4))).Value = gw.ntp_client_status == null ? "null" : String.Join("##", gw.ntp_client_status.ToArray());
                    ((SnmpStringObject)(ElementAt(row, 5))).Value = MibHelper.stdcards(gw);
                    row++;
                }
            }
#else
            List<stdGw> stdgws = U5kManService.GlobalData.STDGWS;
            foreach (stdGw gw in stdgws)
            {
                ((SnmpIntObject)(ElementAt(row, 2))).Value = (int)gw.std;
                ((SnmpStringObject)(ElementAt(row, 3))).Value = MibHelper.stdlans2snmp(new std[] { gw.cpu_activa.lan1, gw.cpu_activa.lan2 });
                ((SnmpStringObject)(ElementAt(row, 4))).Value = gw.ntp_client_status == null ? "null" : String.Join("##", gw.ntp_client_status.ToArray());
                ((SnmpStringObject)(ElementAt(row, 5))).Value = MibHelper.stdcards(gw);
                row++;
            }
#endif

            // Estado General de las pasarelas.
#if STD_ACCESS_V0
            lock (U5kManService._std._gen)
            {
                SnmpIntObject.Get(U5kManMibOids.MttoV2Oid_GwsStdg).Value = (int)U5kManService._std._gen.stdScv1.Estado;
            }
#else
            SnmpIntObject.Get(U5kManMibOids.MttoV2Oid_GwsStdg).Value = (int)U5kManService.GlobalData.STDG.stdScv1.Estado;
#endif
        }
    }
    /// <summary>
    /// MTTO-V2. Canales Radio y Líneas Telefonicas.
    /// </summary>
    public class U5kLegacyItfTb : U5kMttoV2Tb
    {
        public int Size { get; set; }
        public U5kLegacyItfTb(bool isRadio = true)
            : base(isRadio ? U5kManMibOids.MttoV2Oid_RadioTbBase : U5kManMibOids.MttoV2Oid_TelefTbBase, isRadio ? 3 : 4)
        {
            _isRadio = isRadio;
            Size = 0;
#if STD_ACCESS_V0
            List<stdGw> stdgws = U5kManService._std.stdgws;
#else
            List<stdGw> stdgws = U5kManService.GlobalData.STDGWS;
#endif
            foreach (stdGw gw in stdgws)
            {
                foreach (stdSlot slot in gw.cpu_activa.slots)
                {
                    foreach (stdRec rc in slot.rec)
                    {
                        if (_isRadio)
                        {
                            if (rc.tipo == itf.rcRadio)
                            {
                                AddRow(rc.name, rc.presente ? rc.std_online : U5kManServer.std.NoInfo, "---");
                                Size = Size + 1;
                            }
                        }
                        else
                        {
                            if (rc.tipo != itf.rcNotipo && rc.tipo != itf.rcRadio)
                            {
                                AddRow(rc.name, (int)rc.tipo, rc.presente ? rc.std_online : U5kManServer.std.NoInfo, "---");
                                Size = Size + 1;
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public override void Tick()
        {
            int row=0;
#if STD_ACCESS_V0
            List<stdGw> stdgws = U5kManService._std.stdgws;
            lock (U5kManService._std.stdgws)
#else
            List<stdGw> stdgws = U5kManService.GlobalData.STDGWS;
#endif
            {
                SnmpIntObject.Get(_isRadio ? U5kManMibOids.MttoV2Oid_RadioTbNum : U5kManMibOids.MttoV2Oid_TelefTbNum).Value = Size;
                foreach (stdGw gw in stdgws)
                {
                    foreach (stdSlot slot in gw.cpu_activa.slots)
                    {
                        foreach (stdRec rc in slot.rec)
                        {
                            if (_isRadio)
                            {
                                if (rc.tipo == itf.rcRadio)
                                {
                                    ((SnmpIntObject)(ElementAt(row, 1))).Value = (int)(rc.presente ? rc.std_online : U5kManServer.std.NoInfo);
                                    ((SnmpStringObject)(ElementAt(row, 2))).Value = String.Format("<sip:{0}@{1}:{2}>", rc.name, gw.ip, 5060);
                                    row++;
                                }
                            }
                            else
                            {
                                if (rc.tipo != itf.rcNotipo && rc.tipo != itf.rcRadio)
                                {
                                    ((SnmpIntObject)(ElementAt(row, 2))).Value = (int)(rc.presente ? rc.std_online : U5kManServer.std.NoInfo);
                                    ((SnmpStringObject)(ElementAt(row, 3))).Value = String.Format("<sip:{0}@{1}:{2}>", rc.name, gw.ip, 5060);
                                    row++;
                                }
                            }
                        }
                    }
                }
            }
        }

        private bool _isRadio;
    }
    /// <summary>
    /// MTTO-V2. Equipos Externos Radio, Teléfonos y Grabadores.
    /// </summary>
    public class U5kIpItfTb : U5kMttoV2Tb
    {
        public enum IpItfTypes { Radio = 0, Telefonia = 1, Grabador = 2 };

        public int Size { get; set; }
        public U5kIpItfTb(IpItfTypes tipo)
            : base(tipo == IpItfTypes.Radio ? U5kManMibOids.MttoV2Oid_ExtRadioTbBase : 
                   tipo == IpItfTypes.Telefonia ? U5kManMibOids.MttoV2Oid_ExtTelefTbBase : U5kManMibOids.MttoV2Oid_ExtRecsTbBase, 4)
        {
            _tipo = tipo;
            Size = 0;
#if STD_ACCESS_V0
            List<U5kManStdEquiposEurocae.EquipoEurocae> stdeqeu = U5kManService._std.stdeqeu.Equipos;            
#else
            List<EquipoEurocae> stdeqeu = U5kManService.GlobalData.STDEQS;
#endif
            foreach (EquipoEurocae equipo in stdeqeu)
            {
                if (_tipo == IpItfTypes.Radio)
                {
                    if (equipo.Tipo == 2)
                    {
                        AddRow(equipo.Id, equipo.Ip1, equipo.EstadoGeneral, "---");
                        Size = Size + 1;
                    }
                }
                else if (_tipo == IpItfTypes.Telefonia)
                {
                    if (equipo.Tipo == 3)
                    {
                        AddRow(equipo.Id, equipo.Ip1, equipo.EstadoGeneral, "---");
                        Size = Size + 1;
                    }
                }
                else if (_tipo == IpItfTypes.Grabador)
                {
                    if (equipo.Tipo == 5)
                    {
                        AddRow(equipo.Id, equipo.Ip1, equipo.EstadoGeneral, "---");
                        Size = Size + 1;
                    }
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public override void Tick()
        {
#if STD_ACCESS_V0
            List<U5kManStdEquiposEurocae.EquipoEurocae> stdeqeu = U5kManService._std.stdeqeu.Equipos;            
            lock (U5kManService._std.stdeqeu.Equipos)
#else
            List<EquipoEurocae> stdeqeu = U5kManService.GlobalData.STDEQS;
#endif
            {
                int row = 0;
                
                String OidNum = _tipo == IpItfTypes.Radio ? U5kManMibOids.MttoV2Oid_ExtRadioTbNum :
                    _tipo == IpItfTypes.Telefonia ? U5kManMibOids.MttoV2Oid_ExtTelefTbNum : U5kManMibOids.MttoV2Oid_ExtRecsTbNum;

                SnmpIntObject.Get(OidNum).Value = Size;

                foreach (EquipoEurocae equipo in stdeqeu)
                {
                    if (_tipo == IpItfTypes.Radio)
                    {
                        if (equipo.Tipo == 2)
                        {
                            ((SnmpIntObject)(ElementAt(row, 2))).Value = (int)equipo.EstadoGeneral;
                            ((SnmpStringObject)(ElementAt(row, 3))).Value = String.Format("<sip:{0}@{1}:{2}>", equipo.sip_user, equipo.Ip1, equipo.sip_port);
                            row++;
                        }
                    }
                    else if (_tipo == IpItfTypes.Telefonia)
                    {
                        if (equipo.Tipo == 3)
                        {
                            ((SnmpIntObject)(ElementAt(row, 2))).Value = (int)equipo.EstadoGeneral;
                            ((SnmpStringObject)(ElementAt(row, 3))).Value = String.Format("<sip:{0}@{1}:{2}>", equipo.sip_user, equipo.Ip1, equipo.sip_port);
                            row++;
                        }
                    }
                    else if (_tipo == IpItfTypes.Grabador)
                    {
                        if (equipo.Tipo == 5)
                        {
                            ((SnmpIntObject)(ElementAt(row, 2))).Value = (int)equipo.EstadoGeneral;
                            row++;
                        }
                    }
                }
            }
        }

        private IpItfTypes _tipo;
    }
    /// <summary>
    /// MTTO-V2. Abonados PBX
    /// </summary>
    public class U5kPbxTb : U5kMttoV2Tb
    {
        public int Size { get; set; }
        public U5kPbxTb()
            : base(U5kManMibOids.MttoV2Oid_PbxAbTbBase, 3)
        {
            Size = 0;
#if STD_ACCESS_V0
            foreach (Uv5kManDestinosPabx.DestinoPabx destino in U5kManService._std.pabxdest.Destinos)
            {
                AddRow(destino.Id, "", destino.Estado);
                Size = Size + 1;
            }
#else
            U5kManService.GlobalData.STDPBXS.ForEach(d => 
            { 
                AddRow(d.Id, "", d.Estado); 
                Size++; 
            });
#endif
        }

        /// <summary>
        /// 
        /// </summary>
        public override void Tick()
        {
            int row = 0;
#if STD_ACCESS_V0
            foreach (Uv5kManDestinosPabx.DestinoPabx destino in U5kManService._std.pabxdest.Destinos)
            {
                ((SnmpIntObject)(ElementAt(row, 2))).Value = (int)destino.Estado;
                row++;
            }
            // Estado General...
            lock (U5kManService._std._gen)
            {
                SnmpIntObject.Get(U5kManMibOids.MttoV2Oid_PbxStd).Value = (int)U5kManService._std._gen.stdPabx.Estado;

                SnmpIntObject.Get(U5kManMibOids.MttoV2Oid_PbxAbTbNum).Value = row;
            }
#else
            U5kManService.GlobalData.STDPBXS.ForEach(d =>
            {
                ((SnmpIntObject)(ElementAt(row, 2))).Value = (int)d.Estado;
                row++;
            });

            // Estado General...
            SnmpIntObject.Get(U5kManMibOids.MttoV2Oid_PbxStd).Value = (int)U5kManService.GlobalData.STDG.stdPabx.Estado;
            SnmpIntObject.Get(U5kManMibOids.MttoV2Oid_PbxAbTbNum).Value = row;
#endif
        }
    }
    /** 20170220 */
    /** Tablas de Operativa Radio */
    /// <summary>
    /// 
    /// </summary>
    public class U5kRadioSessions : U5kMttoV2Tb
    {
        public int Size { get; set; }
        public U5kRadioSessions() :
            base(U5kManMibOids.MttoV2Oid_RadioSesBase, 14)
        {
            for (int i = 0; i < 128; i++)
            {
                AddRow("---", 0, 0, 0, 0, 0,
                    "uri", "tp", 0, 0, 0, 0, 0, 0);
            }
        }

        /** */
        public override void Tick()
        {
#if _HAY_NODEBOX__
            lock (U5kManService._sessions_data)
            {
                int row = 0;
                SnmpIntObject.Get(U5kManMibOids.MttoV2Oid_RadioSesNum).Value = (int)U5kManService._sessions_data.Count;

                foreach (U5kManService.radioSessionData ses in U5kManService._sessions_data)
                {
                    mod_element(row,
                        ses.frec, ses.ftipo, ses.prio, ses.fstd, ses.fp_climax_mc, ses.fp_bss_win,
                        ses.uri, ses.tipo, ses.std, ses.tx_rtp, ses.tx_cld, ses.tx_owd, ses.rx_rtp, ses.rx_qidx);
                    row++;
                }
            }
#else
            var sessions_data = JsonConvert.DeserializeObject<List<U5kManService.radioSessionData>>(Services.CentralServicesMonitor.Monitor.RadioSessionsString);
            int row = 0;
            SnmpIntObject.Get(U5kManMibOids.MttoV2Oid_RadioSesNum).Value = (int)sessions_data.Count;

            foreach (U5kManService.radioSessionData ses in sessions_data)
            {
                mod_element(row,
                    ses.frec, ses.ftipo, ses.prio, ses.fstd, ses.fp_climax_mc, ses.fp_bss_win,
                    ses.uri, ses.tipo, ses.std, ses.tx_rtp, ses.tx_cld, ses.tx_owd, ses.rx_rtp, ses.rx_qidx);
                row++;
            }
#endif
        }
    }
    /// <summary>
    /// 
    /// </summary>
    public class U5kRadioMNManager : U5kMttoV2Tb
    {
        public int Size { get; set; }
        public U5kRadioMNManager() :
            base(U5kManMibOids.MttoV2Oid_RadioMnBase, 11)
        {
            for (int i = 0; i < 64; i++)
            {
                AddRow("", 0, 0, 0, 0, "", 0, 0, "", "", 0);
            }
        }
        /** */
        public override void Tick()
        {
#if _HAY_NODEBOX__
            lock (U5kManService._MNMan_data)
            {
                int row = 0;
                SnmpIntObject.Get(U5kManMibOids.MttoV2Oid_RadioMnNum).Value = (int)U5kManService._MNMan_data.Count;

                foreach (U5kManService.equipoMNData eq in U5kManService._MNMan_data)
                {
                    mod_element(row, eq.equ??"null", eq.grp, eq.mod, eq.tip, eq.std,
                        eq.frec??"---", eq.prio ?? -1, eq.sip, eq.ip ?? "---", eq.emp ?? "---", eq.tfrec);
                    row++;
                }
            }
#else
            var MNMan_data = JsonConvert.DeserializeObject<List<U5kManService.equipoMNData>>(Services.CentralServicesMonitor.Monitor.RadioMNDataString);
            int row = 0;
            SnmpIntObject.Get(U5kManMibOids.MttoV2Oid_RadioMnNum).Value = (int)MNMan_data.Count;

            foreach (U5kManService.equipoMNData eq in MNMan_data)
            {
                mod_element(row, eq.equ ?? "null", eq.grp, eq.mod, eq.tip, eq.std,
                    eq.frec ?? "---", eq.prio ?? -1, eq.sip, eq.ip ?? "---", eq.emp ?? "---", eq.tfrec);
                row++;
            }
#endif
        }
    }

    /// <summary>
    /// 
    /// </summary>
    class U5kScvMib : IDisposable
    {
        /// <summary>
        /// Todos los objetos.
        /// </summary>
        // MibObject[] mib = new MibObject[]{};
        public List<MibObject> smib = new List<MibObject>();

        /// <summary>
        /// Elementos Discretos..
        /// </summary>
        MibObject[] mib_pub = new MibObject[]
        {
            // Parte MIB-2                
            new SnmpStringObject(U5kManMibOids.MIB2sysDescr, "Ulises V 5000. Sistema de Comunicaciones Vocales. Nucleo Duro Felguera 2014..2018."),
            
            // new SnmpStringObject(U5kManMibOids.MIB2sysObjectID, ".1.3.6.1.4.1.7916.8.1.5"),
            new SnmpObjectIdObject(new ObjectIdentifier(".1.3.6.1.4.1.7916.8.1")),

            // new SnmpIntObject(U5kManMibOids.MIB2sysUpTime, 0),
            new SnmpSysUpTimeObject(),
            new SnmpStringObject(U5kManMibOids.MIB2sysContact, "NUCLEO-DF DT. MADRID. SPAIN"),
            new SnmpStringObject(U5kManMibOids.MIB2sysName, "NDF-UV5K-2.5.*"),
            new SnmpStringObject(U5kManMibOids.MIB2sysLocation, "NUCLEO-DF LABS"),
            new SnmpIntObject(U5kManMibOids.MIB2sysServices, 64),       // Solo Servicios de Aplicacion.

            new SnmpIntObject(U5kManMibOids.MIB2interfacesifNumber, 3 + U5kManService.Posiciones)
        };

        /// <summary>
        /// 
        /// </summary>
        MibObject[] mib_prv = new MibObject[]
        {
#if _MTTO_V1_
            // Parte Privada.            
            new SnmpIntObject(U5kManMibOids.U5KDualLAN, 0),     // Dual / No Dual.
            new SnmpIntObject(U5kManMibOids.U5KServidorDual, 1), // SERV Dual / No Dual.
            new SnmpIntObject(U5kManMibOids.U5KLan1, 1),         // SCVA. Estado Global.
            new SnmpIntObject(U5kManMibOids.U5KLan2, 0),         // SCVB. Estado Global.
            new SnmpIntObject(U5kManMibOids.U5KServ1, 1),        // Estado SERV-1.
            new SnmpIntObject(U5kManMibOids.U5KServ2, 1),        // Estado SERV-2.
            new SnmpIntObject(U5kManMibOids.U5KNtp, 0),          // Estado NTP.
            new SnmpIntObject(U5kManMibOids.U5KSacta1, 1),       // Estado SACTA-1
            new SnmpIntObject(U5kManMibOids.U5KSacta2, 0),       // Estado SACTA-2.

            new SnmpIntObject(U5kManMibOids.U5KNPuestos,U5kManService.Posiciones),      // Numero de Puestos.
            new SnmpIntObject(U5kManMibOids.U5KNRadios, U5kManService.Radios),          // Numero de Interfaces Radio.
            new SnmpIntObject(U5kManMibOids.U5KNLineas, U5kManService.Lineas),          // Numero de Interfaces Telefonicas.
#endif
            /** Version 2 */
            /** Grupo Config */
            new SnmpIntObject(U5kManMibOids.MttoV2Oid_ServidorDual, /*Properties.u5kManServer.Default.ServidorDual ? 1 : 0*/1),
            new SnmpIntObject(U5kManMibOids.MttoV2Oid_HayPbx, 1),
            new SnmpIntObject(U5kManMibOids.MttoV2Oid_HaySacta, /*Properties.u5kManServer.Default.HaySacta ? 1 : 0*/0),
            new SnmpIntObject(U5kManMibOids.MttoV2Oid_HayNtp, /*Properties.u5kManServer.Default.HayReloj ? 1 : 0*/1),

            /** Grupo StdGral */
            new SnmpStringObject(U5kManMibOids.MttoV2Oid_Version, U5kGenericos.Version),
            //new SnmpStringObject(U5kManMibOids.MttoV2Oid_CfgVersion, U5kManService._std._gen.cfgVersion),
            new SnmpStringObject(U5kManMibOids.MttoV2Oid_CfgVersion, ""),
            new SnmpIntObject(U5kManMibOids.MttoV2Oid_StdgTbNum, 7),     // 7 elementos en la tabla...

            /** Puestos */
            new SnmpIntObject(U5kManMibOids.MttoV2Oid_PuestosStdg, (int )std.NoInfo),
#if STD_ACCESS_V0
            new SnmpIntObject(U5kManMibOids.MttoV2Oid_PuestosTbNum, U5kManService._std.stdpos.Count),
            new SnmpIntObject(U5kManMibOids.MttoV2Oid_GwsTbNum, U5kManService._std.stdgws.Count),
#else
            new SnmpIntObject(U5kManMibOids.MttoV2Oid_PuestosTbNum, U5kManService.GlobalData.STDTOPS.Count),
            new SnmpIntObject(U5kManMibOids.MttoV2Oid_GwsTbNum, U5kManService.GlobalData.STDGWS.Count),
#endif
            /** Pasarelas e Interfaces LEGACY */
            new SnmpIntObject(U5kManMibOids.MttoV2Oid_GwsStdg, (int )std.NoInfo),
            new SnmpIntObject(U5kManMibOids.MttoV2Oid_RadioTbNum, 0),
            new SnmpIntObject(U5kManMibOids.MttoV2Oid_TelefTbNum, 0),

            /** Equipos Externos */
            new SnmpIntObject(U5kManMibOids.MttoV2Oid_ExtRadioTbNum, 0),
            new SnmpIntObject(U5kManMibOids.MttoV2Oid_ExtTelefTbNum, 0),
            new SnmpIntObject(U5kManMibOids.MttoV2Oid_ExtRecsTbNum, 0),

            /** PBX y Abonados */                        
            new SnmpIntObject(U5kManMibOids.MttoV2Oid_PbxStd, (int )std.NoInfo),
            new SnmpIntObject(U5kManMibOids.MttoV2Oid_PbxAbTbNum, 0),

            /** Sesiones y equipos radio */
            new SnmpIntObject(U5kManMibOids.MttoV2Oid_RadioSesNum, 0),
            new SnmpIntObject(U5kManMibOids.MttoV2Oid_RadioMnNum, 0),
        };

        /// <summary>
        /// Tablas.
        /// </summary>
        U5kSnmpTable[] mib_tables_pub = new U5kSnmpTable[]
        {
            new MIB2interfacesifTable(U5kManService.Posiciones)                       // Tabla de Interfaces segun ED137B.
        };

        /// <summary>
        /// 
        /// </summary>
        U5kSnmpTable[] mib_tables_prv = new U5kSnmpTable[]
        {
#if _MTTO_V1_
            /** Tablas V1. */
            new U5kTablaPuestos(U5kManService.Posiciones),
            new U5kTablaRadio(U5kManService.Radios),
            new U5kTablaTlf(U5kManService.Lineas),
#endif
            /** Tablas V2. */
            new U5kStdGenTb(),
            new U5kPuestosTb(),
            new U5kGwsTb(),
            new U5kLegacyItfTb(true),
            new U5kLegacyItfTb(false),
            new U5kIpItfTb(U5kIpItfTb.IpItfTypes.Radio),
            new U5kIpItfTb(U5kIpItfTb.IpItfTypes.Telefonia),
            new U5kIpItfTb(U5kIpItfTb.IpItfTypes.Grabador),
            new U5kPbxTb(),

            /** 20170220. Operativa Radio */
            new U5kRadioSessions(),
            new U5kRadioMNManager()
        };

        /// <summary>
        /// 
        /// </summary>
        public U5kScvMib()
        {
        }

        /// <summary>
        /// 
        /// </summary>
        public void Load()
        {
            // Carga los elementos discretos.
            // Array.Sort<MibObject>(mib_pub); 
            foreach (MibObject obj in mib_pub)
            {
                smib.Add(obj);
            }

            // Carga los elementos discretos.
            foreach (MibObject obj in mib_prv)
            {
                smib.Add(obj);
            }

            // Carga las tablas.
            foreach (U5kSnmpTable table in mib_tables_pub)
            {
                table.Load(ref smib);
            }

            // Carga las tablas.
            foreach (U5kSnmpTable table in mib_tables_prv)
            {
                table.Load(ref smib);
            }

            // Ordeno OID y cargo el agente
            smib.Sort();       
            foreach (MibObject obj in smib)
                SnmpAgent.Store.Add(obj);
        }

        public void Dispose()
        {
            // OJO hay que vaciar el Object STORE para los rearranques...
            // SnmpAgent.Store.Clear();
        }

#if _MTTO_V1_
        /// <summary>
        /// 
        /// </summary>
        public bool DualLAN
        {
            set
            {
                SnmpIntObject.Get(U5kManMibOids.U5KDualLAN).Value = value==true ? 1 : 0;
            }
        }
        /// <summary>
        /// 
        /// </summary>
        public bool DualidadServidor
        {
            set
            {
                SnmpIntObject.Get(U5kManMibOids.U5KServidorDual).Value = value == true ? 1 : 0;
            }
        }
        /// <summary>
        /// 
        /// </summary>
        public int Lan1
        {
            set
            {
                SnmpIntObject.Get(U5kManMibOids.U5KLan1).Value = value;
            }
        }
        /// <summary>
        /// 
        /// </summary>
        public int Lan2
        {
            set
            {
                SnmpIntObject.Get(U5kManMibOids.U5KLan2).Value = value;
            }
        }
        /// <summary>
        /// 
        /// </summary>
        public int Servidor1
        {
            set
            {
                SnmpIntObject.Get(U5kManMibOids.U5KServ1).Value = value;
            }
        }
        /// <summary>
        /// 
        /// </summary>
        public int Servidor2
        {
            set
            {
                SnmpIntObject.Get(U5kManMibOids.U5KServ2).Value = value;
            }
        }
        /// <summary>
        /// 
        /// </summary>
        public int Ntp
        {
            set
            {
                SnmpIntObject.Get(U5kManMibOids.U5KNtp).Value = value;
            }
        }
        /// <summary>
        /// 
        /// </summary>
        public int Sacta1
        {
            set
            {
                SnmpIntObject.Get(U5kManMibOids.U5KSacta1).Value = value;
            }
        }
        /// <summary>
        /// 
        /// </summary>
        public int Sacta2
        {
            set
            {
                SnmpIntObject.Get(U5kManMibOids.U5KSacta2).Value = value;
            }
        }
#endif
        /// <summary>
        /// 
        /// </summary>
        static DateTime _last = DateTime.Now;
        public void sysUpTime()
        {
            //DateTime _now = DateTime.Now;
            //TimeSpan _elapsed = _now - _last;
            //_last = _now;
            //SnmpIntObject.Get(U5kManMibOids.MIB2sysUpTime).Value += (int )(_elapsed.TotalSeconds * 100);
            mib_pub[2].Data = null;
        }

        /// <summary>
        /// 
        /// </summary>
        public mib2OperStatus itfTelefonia
        {
            set
            {
                ((MIB2interfacesifTable)mib_tables_pub[0]).OperStatusTelefonia = value;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public mib2OperStatus itfRadio
        {
            set
            {
                ((MIB2interfacesifTable)mib_tables_pub[0]).OperStatusRadio = value;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public mib2OperStatus itfRecorder
        {
            set
            {
                ((MIB2interfacesifTable)mib_tables_pub[0]).OperStatusRecorder = value;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pos"></param>
        /// <param name="Value"></param>
        public void itfPosicion(int pos, std estado, string name)
        {
            MIB2interfacesifTable table = (MIB2interfacesifTable)mib_tables_pub[0];
            table.SetOperStatusPosicion(pos, MibHelper.std2mib2OperStatus(estado));
            table.SetNamePosicion(pos, name);
        }
#if _MTTO_V1_
        /// <summary>
        /// 
        /// </summary>
        /// <param name="name"></param>
        /// <param name="estado"></param>
        public void SetPuesto(string name, std estado)
        {
            U5kTablaPuestos puestos = (U5kTablaPuestos)mib_tables_prv[0];
            string[] oids = new string[3];
            if (puestos.FindPuesto(name, ref oids) == true)
            {
                SnmpStringObject.Get(oids[0]).Value = name;
                SnmpIntObject.Get(oids[1]).Value = (int)estado;
            }
            else
            {
                throw new Exception(string.Format("U5kScvMib.SetPuesto <{0}>: No hay suficientes posiciones libres en la MIB",name));
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="name"></param>
        /// <param name="estado"></param>
        public void SetRadio(string name, std estado)
        {
            U5kTablaRadio radios = (U5kTablaRadio)mib_tables_prv[1];
            string[] oids = new string[3];
            if (radios.FindRadio(name, ref oids)==true)
            {
                SnmpStringObject.Get(oids[0]).Value = name;
                SnmpIntObject.Get(oids[1]).Value = (int)(estado== std.Ok ? 1 : 0);
            }
            else
            {
                throw new Exception(string.Format("U5kScvMib.SetRadio <{0}>: No hay suficientes posiciones libres en la MIB", name));
            }
            
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="name"></param>
        /// <param name="tipo"></param>
        /// <param name="estado"></param>
        public void SetLinea(string name, int tipo, std estado)
        {
            U5kTablaTlf lineas = (U5kTablaTlf)mib_tables_prv[2];
            string[] oids = new string[3];
            if (lineas.FindLinea(name, ref oids)==true)
            {
                SnmpStringObject.Get(oids[0]).Value = name;
                SnmpIntObject.Get(oids[1]).Value = (int)tipo;
                SnmpIntObject.Get(oids[2]).Value = (int)(estado== std.Ok ? 1 : 0);
            }
            else
            {
                throw new Exception(string.Format("U5kScvMib.SetLinea <{0}>: No hay suficientes posiciones libres en la MIB", name));
            }
        }
#endif
        /// <summary>
        /// 
        /// </summary>
        public void MttoV2Tick()
        {
            //int ntables = mib_tables_prv.Length;
            //for (int iTable = 3; iTable < ntables; iTable++)
            //{
            //    try
            //    {
            //        ((U5kMttoV2Tb)mib_tables_prv[iTable]).Tick();
            //    }
            //    catch (Exception ex)
            //    {
            //        throw new Exception(String.Format("Excepcion en MIB-MttoV2Tick Tabla {0}: {1}", iTable, ex.Message));
            //    }
            //}
            foreach (U5kSnmpTable table in mib_tables_prv)
            {
                var type = table.GetType();
                if (type.GetMethod("Tick") != null)
                {
                    try
                    {
                        ((U5kMttoV2Tb)table).Tick();
                    }
                    catch (Exception ex)
                    {
                        throw new Exception(String.Format("Excepcion en MIB-MttoV2Tick Tabla {0}: {1}", type.Name, ex.Message));
                    }
                }
            }
        }

        public void SupervisaCfgSettings()
        {
            SnmpIntObject.Get(U5kManMibOids.MttoV2Oid_ServidorDual).Value = (U5kManService.cfgSettings/*Properties.u5kManServer.Default*/.ServidorDual ? 1 : 0);
            SnmpIntObject.Get(U5kManMibOids.MttoV2Oid_HayPbx).Value = (U5kManService.PbxEndpoint != null ? 1 : 0);
            SnmpIntObject.Get(U5kManMibOids.MttoV2Oid_HaySacta).Value = (U5kManService.cfgSettings/*Properties.u5kManServer.Default*/.HaySacta ? 1 : 0);
            SnmpIntObject.Get(U5kManMibOids.MttoV2Oid_HayNtp).Value = (U5kManService.cfgSettings/*Properties.u5kManServer.Default*/.HayReloj ? 1 : 0);
        }

    }
}
