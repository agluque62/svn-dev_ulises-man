using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using NLog;
using Lextm.SharpSnmpLib;
using Lextm.SharpSnmpLib.Pipeline;
using Lextm.SharpSnmpLib.Messaging;
using Lextm.SharpSnmpLib.Security;
using Lextm.SharpSnmpLib.Objects;


namespace RcsHfSimulator.HfSnmp
{
    /// <summary>
    /// 
    /// </summary>
    public static class GearMibHelper
    {
        public enum eRadioTipo { Receptor=0, Transmisor=1 }
        public enum eRadioBanda { VHF=0, UHF=1 }
        public enum eRadioModo { Main=0, Rsva=1 }

        public class GearData
        {
            public int index { get; set; }
            public string name { get; set; }
            public eRadioTipo tipo { get; set; }
            public eRadioBanda banda { get; set; }
            public eRadioModo modo { get; set; }

            public string frecuency { get; set; }
            public Int32 channel_spacing { get; set; }
            public Int32 modulation { get; set; }
            public Int32 carrier_offset { get; set; }
            public Int32 power { get; set; }
            public Int32 status { get; set; }
        }
        /// <summary>
        /// Inicializacion....
        /// </summary>
        public static List<GearData> GearConfig
        {
            get
            {
                List<GearData> _list = new List<GearData>();

                foreach (String strEqu in Properties.Settings.Default.GearList)
                {
                    String[] EquParam = strEqu.Split(';');
                    if (EquParam.Length == 9)
                    {
                        _list.Add(new GearData()
                        {
                            name = EquParam[0],
                            tipo = (eRadioTipo)(Enum.Parse(typeof(eRadioTipo), EquParam[1])),
                            banda= (eRadioBanda)(Enum.Parse(typeof(eRadioBanda), EquParam[2])),
                            modo = (eRadioModo)(Enum.Parse(typeof(eRadioModo), EquParam[3])),
                            frecuency = EquParam[4],
                            channel_spacing = Int32.Parse(EquParam[5]),
                            modulation = Int32.Parse(EquParam[6]),
                            carrier_offset = Int32.Parse(EquParam[7]),
                            power = Int32.Parse(EquParam[8]),
                            status = 1
                        });
                    }                   
                }

                return _list;
            }
        }
        /// <summary>
        /// 
        /// </summary>
        public static string GearsConfigAndStatusOid
        {
            get
            {
                return Properties.Settings.Default.OidBase;
            }
        }

    }
    /// <summary>
    /// 
    /// </summary>
    public abstract class GearsSnmpTableBase : TableObject
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="oid_base"></param>
        public GearsSnmpTableBase(string oid_base, int ncol)
        {
            _oid_base = oid_base;
            _ncol = ncol + 1;
        }

        /// <summary>
        /// 
        /// </summary>
        public void Load(List<MibObject> list)
        {
            foreach (MibObject obj in Objects)
            {
                list.Add(obj);
            }
        }
        /// <summary>
        /// 
        /// </summary>
        public int NRows { get { return _nrow; } }
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

        public ScalarObject ElementAt(int row, int col)
        {
            int index = row * _ncol + col + 1;
            if (index < Objects.Count())
            {
                return MibObjects[index];
            }
            throw new Exception("SnmpAgent Exception. Elemento de Tabla fuera de rango..");
        }

        abstract public void Tick();
        /// <summary>
        /// 
        /// </summary>
        /// <param name="row"></param>
        /// <returns></returns>
        public List<ScalarObject> RowContent(int row)
        {
            List<ScalarObject> lista = new List<ScalarObject>();
            for (int col = 1; col <= _ncol; col++)
            {
                string oid_object = string.Format("{0:000}.{1:000}.{2:000}", _oid_base, col, row + 1);
                lista.Add(SnmpAgent.Store.GetObject(new ObjectIdentifier(oid_object)));
            }
            return lista;
        }


        IList<MibObject> _elements = new List<MibObject>();
        protected string _oid_base = "";
        protected int _nrow = 0;
        protected int _ncol = 0;

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
        /// <param name="cols"></param>
        protected void AddRow(params object[] cols)
        {
            int index = 1 + _nrow;
            int col = 1;

            _elements.Add(new SnmpIntObject(string.Format("{0:000}.{1:000}.{2:000}", _oid_base, col++, index), index));
            foreach (object obj in cols)
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
                string oid_name = string.Format("{0:000}.{1:000}.{2:000}", _oid_base, colname, row + 1);
                string oid_std = string.Format("{0:000}.{1:000}.{2:000}", _oid_base, colstd, row + 1);

                string val_name = SnmpStringObject.Get(oid_name).Value;
                int estado = SnmpIntObject.Get(oid_std).Value;

                if (val_name == name)
                {
                    que_row = row + 1;
                    return true;
                }
                else if (row_libre == -1 && estado == -1)        // Todo. Estado Inicial debe Ser -1.
                {
                    row_libre = row + 1;
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
    /// 
    /// </summary>
    public class GearsConfigAndStatus : GearsSnmpTableBase
    {
        public GearsConfigAndStatus()
            : base(GearMibHelper.GearsConfigAndStatusOid, typeof(GearMibHelper.GearData).GetProperties().Count())
        {
            List<GearMibHelper.GearData> gears = GearMibHelper.GearConfig.OrderBy(e => e.tipo).OrderBy(e => e.banda).ToList();
            foreach (GearMibHelper.GearData gear in gears)
            {
                AddRow(
                    gear.name, 
                    (int )gear.tipo, 
                    (int )gear.banda, 
                    (int )gear.modo, 
                    gear.frecuency, 
                    -1, // gear.channel_spacing, 
                    -1, // gear.carrier_offset,
                    -1, // gear.modulation, 
                    -1, // gear.power,
                    gear.status);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public override void Tick()
        {
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public string OidEquipo(string name)
        {
            string oid_base_name = Properties.Settings.Default.OidBase + ".2";
            var var_equipo_name  = this.MibObjects.Where(obj => obj.Variable.Id.ToString().StartsWith(oid_base_name) && 
                obj.Variable.Data.ToString() == name);
            if (var_equipo_name != null && var_equipo_name.Count()>0)
            {
                MibObject obj_equipo_name = var_equipo_name.ToList()[0];
                string oid_equipo_name = obj_equipo_name.Variable.Id.ToString();
                int index = oid_equipo_name.LastIndexOf(".");
                string ret = oid_equipo_name.Substring(index, oid_equipo_name.Length - index);
                return ret;
            }
            return "";
        }
    }

    /// <summary>
    /// 
    /// </summary>
    class GearsListMib
    {
        /// <summary>
        /// 
        /// </summary>
        public GearsListMib()
        {
            smib =  new List<MibObject>();
        }
        /// <summary>
        /// 
        /// </summary>
        public void Load()
        {
            smib.Add(new SnmpStringObject(Properties.Settings.Default.OidPregunta, "preguntar-equipo"));
            smib.Add(new SnmpStringObject(Properties.Settings.Default.OidRespuesta, "oid-equipo"));
            gears.Load(smib);

            smib.Sort();
            foreach (MibObject obj in smib)
                SnmpAgent.Store.Add(obj);

            loadLastStatus();
        }
        /// <summary>
        /// 
        /// </summary>
        public void Unload()
        {
            saveLastStatus();
        }
        /// <summary>
        /// 
        /// </summary>
        public List<MibObject> smib { get; set; }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="oid"></param>
        /// <param name="data"></param>
        public void PduSetReceived_handler(string oid, ISnmpData data)
        {
            if (oid == Properties.Settings.Default.OidPregunta)
            {
                ///** Tengo que escribir en Properties.Settings.Default.OidRespuesta la Oid Base (fila) de la tabla correspondiente al equipo */
                string oid_equipo = gears.OidEquipo(data.ToString());
                SnmpStringObject.Get(Properties.Settings.Default.OidRespuesta).Value = oid_equipo;
                // LogManager.GetCurrentClassLogger().Info("OidEquipo {0} => {1}", data.ToString(), oid_equipo);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public List<GearMibHelper.GearData> Gears
        {
            get
            {
                List<GearMibHelper.GearData> lista = new List<GearMibHelper.GearData>();
                for (int row = 0; row < gears.NRows; row++)
                {
                    List<ScalarObject> data = gears.RowContent(row);

                    lista.Add(new GearMibHelper.GearData()
                    {
                        index = (data[0] as SnmpIntObject).Value,
                        name = (data[1] as SnmpStringObject).Value,
                        tipo = (GearMibHelper.eRadioTipo)(data[2] as SnmpIntObject).Value,
                        banda= (GearMibHelper.eRadioBanda)(data[3] as SnmpIntObject).Value,
                        modo = (GearMibHelper.eRadioModo)(data[4] as SnmpIntObject).Value,
                        frecuency = (data[5] as SnmpStringObject).Value,
                        channel_spacing = (data[6] as SnmpIntObject).Value,
                        modulation = (data[7] as SnmpIntObject).Value,
                        carrier_offset = (data[8] as SnmpIntObject).Value,
                        power = (data[9] as SnmpIntObject).Value,
                        status = (data[10] as SnmpIntObject).Value
                    });
                }

                return lista;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="GearIndex"></param>
        /// <param name="status"></param>
        /// <returns></returns>
        public bool StatusGearSet(int GearIndex, int status)
        {
            GearIndex -= 1;
            if (GearIndex < gears.NRows)
            {
                List<ScalarObject> data = gears.RowContent(GearIndex);
                if (data.Count == 12)
                {
                    (data[10] as SnmpIntObject).Value = status;
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// 
        /// </summary>
        protected void saveLastStatus()
        {
            Properties.Settings.Default.LastStatus = new System.Collections.Specialized.StringCollection();
            foreach (GearMibHelper.GearData gear in Gears)
            {
                Properties.Settings.Default.LastStatus.Add(gear.index.ToString() + ";" + gear.status.ToString());
            }
            Properties.Settings.Default.Save();
        }

        /// <summary>
        /// 
        /// </summary>
        protected void loadLastStatus()
        {
            if (Properties.Settings.Default.LastStatus != null)
            {
                foreach (string str_status in Properties.Settings.Default.LastStatus)
                {
                    string[] str_comp = str_status.Split(';');
                    if (str_comp.Length == 2)
                    {
                        StatusGearSet(Int32.Parse(str_comp[0]), Int32.Parse(str_comp[1]));
                    }
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        GearsConfigAndStatus gears = new GearsConfigAndStatus();
    }
}
