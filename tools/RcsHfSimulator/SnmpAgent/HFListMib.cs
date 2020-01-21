using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
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
    public static class HFMibHelper
    {
        public enum eEstadoEquipo { NoPresente=-1, Disponible=1, EnFalloGlobal=0, EnFalloSintonizar=3, EnError=-2 }
        public enum eCmdTx { CMD_FRECUENCIA = 4, CMD_MODULACION = 8, CMD_POTENCIA = 2, CMD_ACTUALIZAR = 1001, CMD_NOCMD = -1 }
        public enum eModoModulacion { mH3E = 2, mBLS = 0 }
        public enum eOids { OID_FREC_CMD, OID_FREC, OID_ESTADO, OID_CMD, OID_MOD_CMD, OID_MOD }

        static Dictionary<eOids, string> _oids = null;

        //const string OID_FREC_CMD = ".5";               // Escritura Frecuencia
        //const string OID_FREC = ".25";                  // Lectura Frecuencia
        //const string OID_ESTADO = ".36";                // Lectura Estado.
        //const string OID_CMD = ".0";                    // Escritura Escritura Comandos (eCmdTx)
        //const string OID_MOD_CMD = ".9";                // Escritura ModoModulacion
        //const string OID_MOD = ".29";                   // Lectura ModoModulacion

        public static string OidTail(eOids oid)
        {
            if (_oids == null)
            {
                bool usingStandard = Properties.Settings.Default.UsingStandarOids;
                _oids = new Dictionary<eOids, string>()
                {
                    {eOids.OID_CMD, usingStandard ? ".0.0" : ".0"},
                    {eOids.OID_FREC_CMD, usingStandard ? ".5.0" : ".5"},
                    {eOids.OID_MOD_CMD, usingStandard ? ".9.0" : ".9"},
                    {eOids.OID_FREC, usingStandard ? ".25.0" : ".25"},
                    {eOids.OID_MOD, usingStandard ? ".29.0" : ".29"},
                    {eOids.OID_ESTADO, usingStandard ? ".36.0" : ".36"}
                };
            }
            KeyValuePair<eOids, string> val = _oids.Where(i => i.Key == oid).FirstOrDefault();            
            return val.Value;
        }

        public static bool IsWritable(string oid) 
        {
            return true;
        }

        public static eCmdTx Cmd(string oid, ISnmpData data)
        {
            if (oid.EndsWith(HFMibHelper.OidTail(eOids.OID_CMD)) == true)
            {
                if (data.TypeCode == SnmpType.Integer32)
                {
                    int cmd = ((Integer32)data).ToInt32();
                    if (Enum.IsDefined(typeof(eCmdTx), cmd) == true)
                    {
                        return (eCmdTx)cmd;
                    }
                }
            }
            return eCmdTx.CMD_NOCMD;
        }

        public class HFTxData
        {
            // Configuracion...
            public string id { get; set; }
            public string oid { get; set; }
            public int time2respond { get; set; }
            public int time2tunein { get; set; }
            public int time2changemode { get; set; }

            // Estado...
            public Int32 frecuency { get; set; }
            public Int32 mode { get; set; }
            public eEstadoEquipo status { get; set; }
            public Int32 error { get; set; }
            public eModoModulacion Mode
            {
                get
                {
                    return (eModoModulacion)mode;
                }
            }

            public void  SetEstado(int estado)
            {
                if (Enum.IsDefined(typeof(eEstadoEquipo), estado) == true)
                {
                    error = 0;
                    status = (eEstadoEquipo)estado;
                }
                else
                {
                    error = estado;
                    status = eEstadoEquipo.EnError;
                }
            }

            public ConsoleColor ColorEstado 
            { 
                get 
                {
                    return status == eEstadoEquipo.NoPresente ? ConsoleColor.Gray :
                        status == eEstadoEquipo.Disponible ? ConsoleColor.Green :
                        status == eEstadoEquipo.EnFalloGlobal ? ConsoleColor.Magenta :
                        status == eEstadoEquipo.EnFalloSintonizar ? ConsoleColor.DarkMagenta : ConsoleColor.Red;
                } 
            }

            public string TextoEstado
            {
                get
                {
                    return status.ToString();
                }
            }

            public string StrStatus
            {
                get
                {
                    return String.Format("{0}##{1}##{2}##{3}", id, (Int32)status, frecuency, mode);
                }
                set
                {
                    string[] items = value.Split(new string[] { "##" }, StringSplitOptions.None);
                    if (items.Length == 4 && items[0] == id)
                    {
                        SetEstado(Int32.Parse(items[1]));
                        frecuency = Int32.Parse(items[2]);
                        mode = Int32.Parse(items[3]);
                        mode = mode == -1 ? (int)eModoModulacion.mBLS : mode;
                    }
                }
            }

        }
        /// <summary>
        /// Inicializacion....
        /// </summary>
        public static List<HFTxData> InitialConfiguration
        {
            get
            {
                List<HFTxData> _list = new List<HFTxData>();

                foreach (String strEqu in Properties.Settings.Default.Transmisores)
                {
                    String[] EquParam = strEqu.Split(',');
                    if (EquParam.Length == 2)
                    {
                        _list.Add(new HFTxData()
                        {
                            id = EquParam[0],
                            oid = EquParam[1],
                            time2respond = 100,             // TODO Poner tiempos configurables....
                            time2tunein = 3000,
                            time2changemode = 300,
                            frecuency = 0,
                            mode = (int )eModoModulacion.mBLS,
                            status = 0
                        });
                    }                   
                }

                return _list;
            }
        }

    }

    /// <summary>
    /// 
    /// </summary>
    class HfTxMib
    {
        /// <summary>
        /// 
        /// </summary>
        List<HFMibHelper.HFTxData> config = null;
        /// <summary>
        /// 
        /// </summary>
        public HfTxMib()
        {
            config = HFMibHelper.InitialConfiguration;
            smib =  new List<MibObject>();
        }
        /// <summary>
        /// 
        /// </summary>
        public void Load()
        {

            loadLastStatus();

            foreach (HFMibHelper.HFTxData tx in config)
            {
                smib.Add(new SnmpIntObject(tx.oid + HFMibHelper.OidTail(HFMibHelper.eOids.OID_CMD), 0));
                smib.Add(new SnmpIntObject(tx.oid + HFMibHelper.OidTail(HFMibHelper.eOids.OID_FREC_CMD), 0));
                smib.Add(new SnmpIntObject(tx.oid + HFMibHelper.OidTail(HFMibHelper.eOids.OID_MOD_CMD), 0));
                smib.Add(new SnmpIntObject(tx.oid + HFMibHelper.OidTail(HFMibHelper.eOids.OID_FREC), tx.frecuency));
                smib.Add(new SnmpIntObject(tx.oid + HFMibHelper.OidTail(HFMibHelper.eOids.OID_MOD), tx.mode));
                smib.Add(new SnmpIntObject(tx.oid + HFMibHelper.OidTail(HFMibHelper.eOids.OID_ESTADO), (int)tx.status));
            }

            smib.Sort();
            foreach (MibObject obj in smib)
                SnmpAgent.Store.Add(obj);

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
            HFMibHelper.HFTxData tx = config.Where(eq => oid.StartsWith(eq.oid)).FirstOrDefault(); 
            if (tx != null && HFMibHelper.IsWritable(oid))
            {
                switch (HFMibHelper.Cmd(oid, data))
                {
                    case HFMibHelper.eCmdTx.CMD_FRECUENCIA:
                        SetFrecuency(tx);
                        break;

                    case HFMibHelper.eCmdTx.CMD_MODULACION:
                        SetTxMode(tx);
                        break;
                }
            }
            else
            {
                // Solo puedo dar un aviso ya que el gestor va a dejar por debajo....
            }

            //if (oid == Properties.Settings.Default.OidPregunta)
            //{
            //    ///** Tengo que escribir en Properties.Settings.Default.OidRespuesta la Oid Base (fila) de la tabla correspondiente al equipo */
            //    string oid_equipo = gears.OidEquipo(data.ToString());
            //    SnmpStringObject.Get(Properties.Settings.Default.OidRespuesta).Value = oid_equipo;
            //    // LogManager.GetCurrentClassLogger().Info("OidEquipo {0} => {1}", data.ToString(), oid_equipo);
            //}
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="oid"></param>
        /// <returns></returns>
        public bool PduGetReceived_handler(string oid)
        {
            HFMibHelper.HFTxData tx = config.Where(eq => oid.StartsWith(eq.oid)).FirstOrDefault();
            if (tx != null && tx.status == HFMibHelper.eEstadoEquipo.NoPresente)
                return false;
            return true;
        }

        /// <summary>
        /// 
        /// </summary>
        public List<HFMibHelper.HFTxData> Transmisores
        {
            get
            {
                foreach (HFMibHelper.HFTxData tx in config)
                {
                    tx.mode = GetScalarObjectValue(tx.oid + HFMibHelper.OidTail(HFMibHelper.eOids.OID_MOD));
                    tx.frecuency = GetScalarObjectValue(tx.oid + HFMibHelper.OidTail(HFMibHelper.eOids.OID_FREC));
                    tx.SetEstado(GetScalarObjectValue(tx.oid + HFMibHelper.OidTail(HFMibHelper.eOids.OID_ESTADO)));
                }
                return config;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="GearIndex"></param>
        /// <param name="status"></param>
        /// <returns></returns>
        public bool SetTxStatus(HFMibHelper.HFTxData tx, int status)
        {
            if (tx != null)
            {
                SetScalarObjectValue(tx.oid + HFMibHelper.OidTail(HFMibHelper.eOids.OID_ESTADO), status);
            }            
            return false;
        }

        protected void SetFrecuency(HFMibHelper.HFTxData tx)
        {
            if (tx.status == HFMibHelper.eEstadoEquipo.EnFalloSintonizar)
                return;
            Task.Factory.StartNew(() =>
            {
                int frec = GetScalarObjectValue(tx.oid + HFMibHelper.OidTail(HFMibHelper.eOids.OID_FREC_CMD));
                SetScalarObjectValue(tx.oid + HFMibHelper.OidTail(HFMibHelper.eOids.OID_FREC), 0);
                Thread.Sleep(tx.time2tunein);
                SetScalarObjectValue(tx.oid + HFMibHelper.OidTail(HFMibHelper.eOids.OID_FREC), frec);
            });
        }

        protected void SetTxMode(HFMibHelper.HFTxData tx)
        {
            if (tx.status == HFMibHelper.eEstadoEquipo.EnFalloSintonizar)
                return;
            Task.Factory.StartNew(() =>
            {
                int modo = GetScalarObjectValue(tx.oid + HFMibHelper.OidTail(HFMibHelper.eOids.OID_MOD_CMD));
                Thread.Sleep(tx.time2tunein);
                SetScalarObjectValue(tx.oid + HFMibHelper.OidTail(HFMibHelper.eOids.OID_MOD), modo);
            });
        }

        /// <summary>
        /// 
        /// </summary>
        protected void saveLastStatus()
        {
            Properties.Settings.Default.lastStatus = new System.Collections.Specialized.StringCollection();
            foreach (HFMibHelper.HFTxData tx in config)
            {
                Properties.Settings.Default.lastStatus.Add(tx.StrStatus);
            }
            Properties.Settings.Default.Save();
        }

        /// <summary>
        /// 
        /// </summary>
        protected void loadLastStatus()
        {
            if (Properties.Settings.Default.lastStatus != null)
            {
                foreach (string str_status in Properties.Settings.Default.lastStatus)
                {
                    HFMibHelper.HFTxData tx = config.First(t => str_status.StartsWith(t.id)==true);// .FirstOrDefault();
                    if (tx != null)
                    {
                        tx.StrStatus = str_status;
                    }
                }
            }
        }

        protected void SetScalarObjectValue(string oid, int value)
        {
            ScalarObject scalar = smib.Where(obj => obj.Variable.Id.Equals(new ObjectIdentifier(oid))).FirstOrDefault();
            if (scalar != null)
            {
                (scalar as SnmpIntObject).Value = value;
            }
        }

        protected int GetScalarObjectValue(string oid)
        {
            ScalarObject scalar = smib.Where(obj => obj.Variable.Id.Equals(new ObjectIdentifier(oid))).FirstOrDefault();
            if (scalar != null)
            {
                return (scalar as SnmpIntObject).Value;
            }
            return -1;
        }

    }
}
