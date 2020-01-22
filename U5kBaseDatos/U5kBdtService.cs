using MySql.Data.MySqlClient;
using NLog;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SQLite;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Security.Principal;
using System.Text;

namespace U5kBaseDatos
{
    public enum eTiposInci { TEH_TOP = 0, TEH_TIFX = 1, TEH_EXTERNO_RADIO, TEH_EXTERNO_TELEFONIA, TEH_SISTEMA, TEH_RADIOHF, TEH_RADIOMN, TEH_RECORDER }
    /// <summary>
    /// 
    /// </summary>
    public enum eIncidencias
    {
        IGNORE = -1,

        // HF...
        IGRL_HF_CONN = 1,
        IGRL_HF_ERR = 2,
        IGRL_HF_UNCONN = 3,
        IGRL_HF_ASIGN = 4,
        IGRL_HF_UNASIGN = 5,
        IGRL_HF_GEN_ERROR = 6,
        IGRL_HF_ASIGN_ERROR = 7,
        IGRL_HF_UNASIGN_ERROR = 8,
        IGRL_HF_MULT_ASIGN_ERROR = 9,
        IGRL_HF_SELCAL_ERROR = 10,

        IGRL_U5KI_SERVICE_INFO = 50,
        IGRL_U5KI_SERVICE_ERROR = 51,

        IGRL_NBXMNG_EVENT = 300,
        IGRL_NBXMNG_ALARM = 301,

        // GENERALES...
        IGRL_CAMBIO_DE_DIA = 96,

        IGRL_SCV_SELECTED = 101,
        IGRL_SECT_LOADED = 105,
        IGRL_SECT_LOAD_ERROR = 106,
        IGRL_SECT_REJECTED = 108,
        IGRL_SECT_AUT_LOADED = 109,
        IGRL_SECT_AUT_REJECTED = 110,
        IGRL_SECT_SECTOR_ASIGNED = 111,
        IGRL_SECT_SECTOR_UNASIGNED = 112,
        IGRL_SEC_1MAS1_REJECTED = 113,

        IGRL_SRV_ACTIVE_MAIN = 201,
        IGRL_SRV_ACTIVE_STBY = 202,
        IGRL_SRV_INACTIVE = 203,

        // TOP
        ITO_ENTRADA = 1001,                         //
        ITO_CAIDA = 1002,                           //
        ITO_CONEXION_JACK_EJECUTIVO = 1003,         //
        ITO_DESCONEXION_JACK_EJECUTIVO = 1004,      //
        ITO_CONEXION_JACK_AYUDANTE = 1005,          //
        ITO_DESCONEXION_JACK_AYUDANTE = 1006,       //
        ITO_CONEXION_ALTAVOZ = 1007,                //
        ITO_DESCONEXION_ALTAVOZ = 1008,             //
        ITO_PANEL_OPERACION = 1009,                 //
        ITO_PANEL_SBY = 1010,                       //
        ITO_CONEXION_CABLE_GRABACION = 1023,        //
        ITO_DESCONEXION_CABLE_GRABACION = 1024,     //
        ITO_ESTADO_LAN = 1025,                      // 

        ITO_PAGINA_FRECUENCIAS = 1011,
        ITO_PTT = 1014,
        ITO_FACILIDAD_SELECCIONADA = 1015,
        ITO_LLAMADA_SALIENTE = 1017,
        ITO_LLAMADA_ENTRANTE = 1016,
        ITO_LLAMADA_ESTABLECIDA = 1020,
        ITO_LLAMADA_FIN = 1019,
        ITO_SESION_BREIFING = 1021,
        ITO_FUNCION_REPLAY = 1022,

        // GW
        IGW_ENTRADA = 2001,                         //
        IGW_CAIDA = 2002,                           //
        IGW_CONEXION_IA4 = 2007,                    // 
        IGW_DESCONEXION_IA4 = 2008,                 //
        IGW_CONEXION_RECURSO_RADIO = 2003,          //
        IGW_DESCONEXION_RECURSO_RADIO = 2004,       //
        IGW_CONEXION_RECURSO_TLF = 2005,            //
        IGW_DESCONEXION_RECURSO_TLF = 2006,         //
        IGW_CONEXION_RECURSO_R2 = 2009,             //
        IGW_DESCONEXION_RECURSO_R2 = 2010,          //

        IGW_ERROR_PROTOCOLO_LCN = 2012,
        IGW_ACTIVACION_LCN = 2013,
        IGW_LCN_FUERA_SERVICIO = 2014,

        IGW_PTT_ON = 2050,
        IGW_PTT_OFF = 2051,
        IGW_SQ_ON = 2052,
        IGW_SQ_OFF = 2053,

        IGW_LLAMADA_ENTRANTE_TF = 2040,
        IGW_FIN_LLAMADA_ENTRANTE_TF = 2041,
        IGW_LLAMADA_SALIENTE_TF = 2042,
        IGW_FIN_LLAMADA_SALIENTE_TF = 2043,

        IGW_LLAMADA_ENTRANTE_LC = 2030,
        IGW_FIN_LLAMADA_ENTRANTE_LC = 2031,
        IGW_LLAMADA_SALIENTE_LC = 2032,
        IGW_FIN_LLAMADA_SALIENTE_LC = 2033,

        IGW_LLAMADA_ENTRANTE_R2 = 2020,
        IGW_FIN_LLAMADA_ENTRANTE_R2 = 2021,
        IGW_LLAMADA_SALIENTE_R2 = 2022,
        IGW_FIN_LLAMADA_SALIENTE_R2 = 2023,
        IGW_LLAMADA_PRUEBA_R2 = 2024,
        IGW_ERROR_PROTOCOLO_R2 = 2025,

        IGW_PRINCIPAL_RESERVA = 2100,
        IGW_EVENTO = 2200,
        IGW_ACCION = 2300,

        // Equipos Externos
        IEE_ENTRADA = 3001,
        IEE_CAIDA = 3002,
        // Abonados de PBX
        IPBX_SUBSC_ACTIVE=3003,
        IPBX_SUBSC_INACTIVE=3004,

        // M+N...
        U5KI_NBX_NM_GEAR_DISP = 3050,
        U5KI_NBX_NM_GEAR_FAIL = 3051,
        U5KI_NBX_NM_GEAR_ITF_ERROR = 3052,

        U5KI_NBX_NM_FRECUENCY_TX_ONMAIN = 3060,
        U5KI_NBX_NM_FRECUENCY_TX_ONRSVA = 3061,
        U5KI_NBX_NM_FRECUENCY_TX_ONERROR = 3062,
        U5KI_NBX_NM_FRECUENCY_TX_NONPRIORITY_ONERROR = 3063,
        U5KI_NBX_NM_FRECUENCY_RX_ONMAIN = 3064,
        U5KI_NBX_NM_FRECUENCY_RX_ONRSVA = 3065,
        U5KI_NBX_NM_FRECUENCY_RX_ONERROR = 3066,
        U5KI_NBX_NM_FRECUENCY_RX_NONPRIORITY_ONERROR = 3067,

        U5KI_NBX_NM_COMMAND_COMMAND = 3070,
        U5KI_NBX_NM_COMMAND_ERROR = 3071,

        U5KI_NBX_NM_GENERIC_EVENT = 3080,
        U5KI_NBX_NM_GENERIC_ERROR = 3081,
        U5KI_NBX_NM_CONFIGURATION_ERROR = 3082,

        // Para las Estadisticas...
        EST_INCI_TOPE = 5000,
        EST_INCI_EOPE = 5001,
        EST_INCI_SOPE = 5002,
        EST_INCI_TFAL = 5003,
        EST_INCI_EFAL = 5004,
        EST_INCI_SFAL = 5005,

    }

    // Base de Datos Soportadas.
    public enum eBdt { bdtMySql, bdtSqlite };

    /// <summary>
    /// 
    /// </summary>
    public class U5kIncidencia
    {
        public DateTime fecha { get; set; }
        public DateTime reconocida { get; set; }
        public int id { get; set; }
        public string sistema { get; set; }
        public int scv { get; set; }
        public string idhw { get; set; }
        public int tipo { get; set; }
        public string desc { get; set; }
    }

    /// <summary>
    /// 
    /// </summary>
    public class BdtObject
    {
        public string Id { get; set; }
    }

    /// <summary>
    /// 
    /// </summary>
    public class BdtTop : BdtObject
    {
        public string Ip { get; set; }
    }

    /// <summary>
    /// 
    /// </summary>
    public class BdtGwRes : BdtObject
    {
        public int Slot { get; set; }
        public int Pos { get; set; }
        public uint Tpo { get; set; }   // 0: Radio, 1: Telefonia, 2: Linea Caliente
        public uint Itf { get; set; }   // 2: BC, 3: BL, 4: AB, 5: R2, 6: N5 
        public uint Stpo { get; set; }  // 0: AudioRx, 1: AudioTx, 2: AudioRxTx, 3: AudioTxHF, 4: MNRx, 5: MNTx
    }

    /// <summary>
    /// 
    /// </summary>
    public class BdtGw : BdtTop
    {
        public bool Dual { get; set; }
        public string Ip1 { get; set; }
        public string Ip2 { get; set; }
        public int SnmpPortA { get; set; }
        public int SnmpPortB { get; set; }
        public List<BdtGwRes> Res { get; set; }
    }

    /// <summary>
    /// 
    /// </summary>
    public class BdtMNRd : BdtObject
    {
        public string frec { get; set; }
        public int RxTx { get; set; }
    }

    /// <summary>
    /// 
    /// </summary>
    public class BdtEuEq : BdtTop
    {
        public string Ip2 { get; set; }
        public int Tipo { get; set; }
        public int Modelo { get; set; }
        public int RxOrTx { get; set; }
        public string IdDestino { get; set; }
        public string IdRecurso { get; set; }
        public int SipPort { get; set; }
    }

    /// <summary>
    /// 
    /// </summary>
    public class BdtPabxDest : BdtObject
    {
    }
    /// <summary>
    /// 
    /// </summary>
    public class BdtExtDest : BdtObject
    {
    }

    /// <summary>
    /// 
    /// </summary>
    public class U5kBdtService
    {
        /// <summary>
        /// 
        /// </summary>
        protected Logger _logger = LogManager.GetCurrentClassLogger();
        object _DB = null;
        string _strConn = String.Empty;
        System.Globalization.CultureInfo _idioma = new System.Globalization.CultureInfo("es");
        public string BdtLanguage { get { return _idioma.ToString(); } }

        #region PUBLIC

        /// <summary>
        /// 
        /// </summary>
        public eBdt Tipo { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public U5kBdtService(System.Globalization.CultureInfo idioma, eBdt tipo, string pathorserver, string user = "CD40", string pwd = "cd40")
        {
            Tipo = tipo;
            _idioma = idioma;
            _strConn = U5kBdtService.BdtStrConn(tipo, pathorserver, user, pwd);
            Open();
        }

        public void dbClose()
        {
            if (Tipo == eBdt.bdtMySql)
            {
                ((MySqlConnection)_DB).Close();
            }
            else if (Tipo == eBdt.bdtSqlite)
            {
                ((SQLiteConnection)_DB).Close();
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public List<BdtTop> GetListaTop(string sistema)
        {
            List<BdtTop> lista = new List<BdtTop>();
            try
            {
                DataSet data = GetDataSet(String.Format("SELECT * FROM Top WHERE IdSistema=\"{0}\"", sistema));
                foreach (System.Data.DataRow top in data.Tables[0].Rows)
                {
                    try
                    {
                        lista.Add(new BdtTop() { Id = top.Field<string>("IdTop"), Ip = top.Field<string>("IpRed1") });
                    }
                    catch (Exception x)
                    {
                        _logger.Error(x, "U5kBdtService.GetListaTop Exception.");
                    }
                }
            }
            catch (Exception x)
            {
                _logger.Error(x, "U5kBdtService.GetListaTop Exception.");
            }
            return lista;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sistema"></param>
        /// <returns></returns>
        public UInt32 GetNumeroTop(string sistema)
        {
            UInt32 elementos = GetScalar(String.Format("SELECT COUNT(*) FROM Top WHERE IdSistema=\"{0}\"", sistema));
#if DEBUG1 
            elementos = 4;
#endif
            return elementos;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="scv"></param>
        /// <returns></returns>
        public List<BdtGw> GetListaGw(string sistema)
        {
            List<BdtGw> gws = new List<BdtGw>();
            try
            {
                DataSet data = GetDataSet(String.Format("SELECT tifx.*, gwactivas.ipred FROM tifx, gwactivas WHERE tifx.idtifx=gwactivas.idtifx and tifx.idSistema=\"{0}\" order by idTIFX", sistema));
                foreach (System.Data.DataRow gw in data.Tables[0].Rows)
                {
                    try
                    {
                        BdtGw bgw = new BdtGw()
                        {
                            Id = gw.Field<string>("IdTIFX"),
                            Ip = gw.Field<string>("IpRed"),
                            Ip1 = gw.Field<string>("IpRed1"),
                            Ip2 = gw.Field<string>("IpRed2"),
                            Dual = gw.Field<string>("IpRed1") == gw.Field<string>("IpRed2") ? false : true,
                            SnmpPortA = data.Tables[0].Columns["SNMPPortLocal"].DataType.Name == "UInt32" ? (int)gw.Field<uint>("SNMPPortLocal") : gw.Field<int>("SNMPPortLocal"),
                            SnmpPortB = data.Tables[0].Columns["SNMPPortRemoto"].DataType.Name == "UInt32" ? (int)gw.Field<uint>("SNMPPortRemoto") : gw.Field<int>("SNMPPortRemoto")
                        };

                        bgw.Res = new List<BdtGwRes>();
                        DataSet lres = GetDataSet(string.Format("select * from recursos where idTIFX=\"{0}\" order by SlotPasarela,NumDispositivoSlot", bgw.Id));
                        foreach (DataRow bdtres in lres.Tables[0].Rows)
                        {
                            try
                            {
                                BdtGwRes res = new BdtGwRes()
                                {
                                    Id = bdtres.Field<string>("IdRecurso"),
                                    Slot = lres.Tables[0].Columns["SlotPasarela"].DataType.Name == "UInt32" ? (int)bdtres.Field<uint>("SlotPasarela") : bdtres.Field<int>("SlotPasarela"),
                                    Pos = lres.Tables[0].Columns["NumDispositivoSlot"].DataType.Name == "UInt32" ? (int)bdtres.Field<uint>("NumDispositivoSlot") : bdtres.Field<int>("NumDispositivoSlot"),
                                    Tpo = lres.Tables[0].Columns["TipoRecurso"].DataType.Name == "UInt32" ? bdtres.Field<uint>("TipoRecurso") : (uint)bdtres.Field<int>("TipoRecurso"),
                                    Itf = lres.Tables[0].Columns["Interface"].DataType.Name == "UInt32" ? bdtres.Field<uint>("Interface") : (uint)bdtres.Field<int>("Interface"),
                                    Stpo = lres.Tables[0].Columns["Tipo"].DataType.Name == "UInt32" ? bdtres.Field<uint>("Tipo") : (uint)bdtres.Field<int>("Tipo")
                                };
                                bgw.Res.Add(res);
                            }
                            catch (Exception x)
                            {
                                _logger.Error(x, "U5kBdtService.GetListaGw (Recursos) Exception.");
                            }
                        }

                        gws.Add(bgw);
                    }
                    catch (Exception x)
                    {
                        _logger.Error(x, "U5kBdtService.GetListaGw Exception.");
                    }
                }
            }
            catch (Exception x)
            {
                _logger.Error(x, "U5kBdtService.GetListaGw Exception.");
            }
            return gws;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sistema"></param>
        /// <returns></returns>
        public UInt32 GetNumeroGw(string sistema)
        {
            return GetScalar(String.Format("SELECT COUNT(*) FROM tifx, gwactivas WHERE tifx.idtifx=gwactivas.idtifx and tifx.idSistema=\"{0}\"", sistema));
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sistema"></param>
        /// <returns></returns>
        public List<BdtMNRd> BdtListaEquiposMN(string sistema)
        {
            List<BdtMNRd> _lista = new List<BdtMNRd>();
            try
            {
                DataSet data = GetDataSet(String.Format("SELECT IdRecurso, tipo FROM recursos where (IdSistema=\"{0}\" and TipoRecurso=0 and (tipo=4 or tipo=5))", sistema));
                foreach (System.Data.DataRow top in data.Tables[0].Rows)
                {
                    try
                    {
                        _lista.Add(new BdtMNRd() 
                        { 
                            Id = top.Field<string>("IdRecurso") 
                            //RxTx = top.Field<int>("tipo") 
                        });
                    }
                    catch (Exception x)
                    {
                        _logger.Error(x, "U5kBdtService.BdtListaEquiposMN Exception.");
                    }
                }
            }
            catch (Exception x)
            {
                _logger.Error(x, "U5kBdtService.BdtListaEquiposMN Exception.");
            }

            return _lista;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sistema"></param>
        /// <returns></returns>
        public List<BdtMNRd> BdtListaDestinosRadio(string sistema)
        {
            List<BdtMNRd> _lista = new List<BdtMNRd>();
            try
            {
                DataSet data = GetDataSet(String.Format("SELECT IdRecurso, IdDestino FROM recursosradio where IdSistema=\"{0}\"", sistema));
                foreach (System.Data.DataRow top in data.Tables[0].Rows)
                {
                    try
                    {
                        _lista.Add(new BdtMNRd() 
                        { 
                            Id = top.Field<string>("IdRecurso"), 
                            frec = top.Field<string>("IdDestino") 
                        });
                    }
                    catch (Exception x)
                    {
                        _logger.Error(x, "U5kBdtService.BdtListaDestinosRadio Exception.");
                    }
                }
            }
            catch (Exception x)
            {
                _logger.Error(x, "U5kBdtService.BdtListaDestinosRadio Exception.");
            }

            return _lista;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sistema"></param>
        /// <returns></returns>
        public List<BdtEuEq> ListaEquiposEurocae(string sistema)
        {
            List<BdtEuEq> equipos = new List<BdtEuEq>();
            try
            {
                DataSet data;
                int ver;

                if (TableOrViewExist("destinosenequipoexternoparaoptions") == true)
                {
                    data = GetDataSet(String.Format("select * from destinosenequipoexternoparaoptions order by idEquipos", sistema));
                    ver = 2;
                }
                else if (TableOrViewExist("viewequiposexternos") == true)
                {
                    data = GetDataSet(String.Format("select * from viewequiposexternos order by idEquipos", sistema));
                    ver = 1;
                }
                else
                {
                    data = GetDataSet(String.Format("select * from equiposeu where idSistema=\"{0}\" order by idEquipos", sistema));
                    ver = 0;
                }

                DataTable tb = data.Tables[0];
                foreach (DataRow row in tb.Rows)
                {
                    try
                    {
                        string id = row.Field<string>("IDEQUIPOS");
                        string ip = row.Field<string>("IPRED1");
                        string ip2 = row.Field<string>("IPRED2");
                        uint tipo = row.Field<uint>("TIPOEQUIPO");
                        long? modelo =  ver < 1 ? 0 : row.Field<long?>("MODELOEQUIPO");
                        ulong? rxortx = ver < 1 ? 0 : row.Field<ulong?>("TIPORADIO");
                        string iddestino = ver < 2 ? "" : row.Field<string>("IDDESTINO");
                        string idrecurso = ver < 2 ? "" : row.Field<string>("IDRECURSO");
                        int sip_port = ver < 2 ? 5060 : row.Field<int>("SIPPORT");
                        equipos.Add(new BdtEuEq()
                        {
                            Id = id,
                            Ip = ip,
                            Ip2 = ip2,
                            Tipo = (int )tipo,
                            Modelo = modelo==null ? 0 : (int )modelo,
                            RxOrTx = rxortx==null ? 0 : (int )rxortx,
                            IdDestino = iddestino,
                            IdRecurso = idrecurso,
                            SipPort = sip_port
                        });
                    }
                    catch (Exception x)
                    {
                        _logger.Error<Exception>("U5kBdtService.ListaEquiposEurocae Exception.", x);
                    }
                }
            }
            catch (Exception x)
            {
                _logger.Error(x, "U5kBdtService.ListaEquiposEurocae Exception.");
            }
            return equipos;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sistema"></param>
        /// <returns></returns>
        public UInt32 ExternalRadioResourcesCount()
        {
            return GetScalar("SELECT COUNT(*) FROM destinosenequipoexternoparaoptions WHERE idrecurso IS NOT NULL AND TipoEquipo=2;");
        }

        /// <summary>
        /// TODO....Ojo con los puertos....
        /// </summary>
        /// <returns></returns>
        public UInt32 ExternalPhoneResourcesCount(string ipPbx)
        {
            return GetScalar(
                String.Format("SELECT COUNT(*) FROM destinosenequipoexternoparaoptions " + 
                    "WHERE idrecurso IS NOT NULL AND TipoEquipo=3 AND (ipRed1 NOT LIKE \"{0}%\" AND ipRed2 NOT LIKE \"{0}%\")", ipPbx)
                    );
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public UInt32 ExternalRecordersCount()
        {
            return GetScalar("SELECT COUNT(*) FROM destinosenequipoexternoparaoptions WHERE TipoEquipo=5;");
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="ip"></param>
        /// <returns></returns>
        public List<BdtPabxDest> ListaDestinosPABX(string ip)
        {
            List<BdtPabxDest> ldest = new List<BdtPabxDest>();

            try
            {                
#if __PBAX_DEST__
                DataSet data = GetDataSet(String.Format("SELECT * FROM destinosenequipoexterno WHERE IpRed1='{0}' or IpRed2='{0}' order by IdDestino;", ip));

                foreach (DataRow row in data.Tables[0].Rows)
                {
                    try
                    {
                        ldest.Add(new BdtPabxDest() { Id = row.Field<string>("IdDestino") });
                    }
                    catch (Exception x)
                    {
                        _logger.Error(x, "U5kBdtService.ListaDestinosPABX Exception.");
                    }
                }
#else
                String sql = String.Format(
                    "SELECT `rt`.`IdDestino` AS `IdDestino`, `rt`.`IdRecurso` AS `IdRecurso`, `e`.`IpRed1` AS `IpRed1`, `e`.`IpRed2` AS `IpRed2`, `e`.`idEquipos`  AS `idEquipos` " +
                    "FROM ((`recursostf` `rt` LEFT JOIN `recursos` `r` ON ((`r`.`IdRecurso` = `rt`.`IdRecurso`))) JOIN `equiposeu` `e` ON ((`r`.`idEquipos` = `e`.`idEquipos`))) " +
                    "WHERE (`rt`.`IdDestino` IS NOT NULL) AND (IpRed1 LIKE \"{0}%\" or IpRed2 LIKE \"{0}%\") order by IdRecurso;",ip);
                DataSet data1 = GetDataSet(sql);
                if (data1.Tables.Count > 0)
                {
                    DataTable tb = data1.Tables[0];
                    foreach (DataRow r in tb.Rows)
                    {
                        try
                        {
                            ldest.Add(new BdtPabxDest() { Id = r.Field<string>("IdRecurso") });
                        }
                        catch (Exception x)
                        {
                            _logger.Error(x, "U5kBdtService.ListaDestinosPABX Exception.");
                        }
                    }
                }
#endif
            }
            catch (Exception x)
            {
                _logger.Error(x, "U5kBdtService.ListaDestinosPABX Exception.");
            }
            return ldest;
        }

        /// <summary>
        /// 
        /// </summary>
        //public List<U5kIncidenciaDescr> ListaDeIncidencias(System.Collections.Specialized.StringCollection filtro)
        //{
        //    List<U5kIncidenciaDescr> _lst = new List<U5kIncidenciaDescr>();

        //    string strsql = _idioma.Name.StartsWith("en") ? "select * from incidencias_ingles" :
        //        _idioma.Name.StartsWith("fr") ? "select * from incidencias_frances" : "select * from incidencias";

        //    DataSet dsInci = GetDataSet(strsql);
        //    DataTable incis = dsInci.Tables[0];

        //    int nInci = -1;
        //    int alarma = 0;
        //    foreach (System.Data.DataRow inci in incis.Rows)
        //    {
        //        try
        //        {
        //            if (inci.IsNull("IdIncidencia"))
        //            {
        //                _logger.Error("Lista de Incidencias. Error. Registro de Incidencia NULO");
        //                continue;
        //            }

        //            nInci = inci.IsNull("IdIncidencia") ? -1 :
        //                incis.Columns["IdIncidencia"].DataType.Name == "UInt32" ? (int)inci.Field<uint>("IdIncidencia") :
        //                inci.Field<int>("IdIncidencia");

        //            var fInci = inci.IsNull("Incidencia") ? "ERROR Incidencia " + nInci.ToString() : inci.Field<string>("Incidencia");
        //            var strf = inci.IsNull("Descripcion") ? "ERROR Descripcion " + nInci.ToString() : inci.Field<string>("Descripcion");

        //            alarma = inci.IsNull("generaerror") ? 0 :
        //                incis.Columns["generaerror"].DataType.Name == "Boolean" ? (inci.Field<bool>("generaerror") ? 1 : 0) :
        //                inci.Field<int>("generaerror");

        //            if (filtro == null || filtro.Count == 0 || filtro.Contains(nInci.ToString()) == false)
        //            {
        //                _lst.Add(new U5kIncidenciaDescr
        //                {
        //                    id = nInci,
        //                    desc = fInci,
        //                    alarm = alarma == 0 ? false : true,
        //                    strformat = strf
        //                });
        //            }
        //        }
        //        catch (Exception x)
        //        {
        //            _logger.Error(x, String.Format("ListaDeIncidencias. Excepcion en Incidencia {0}", nInci));
        //        }
        //    }
        //    return _lst;
        //}

        public List<U5kIncidenciaDescr> ListaDeIncidencias(System.Collections.Specialized.StringCollection filtro)
        {
            List<U5kIncidenciaDescr> _lst = new List<U5kIncidenciaDescr>();

            string strsql = _idioma.Name.StartsWith("en") ? "select * from incidencias_ingles" :
                _idioma.Name.StartsWith("fr") ? "select * from incidencias_frances" : "select * from incidencias";

            DataSet dsInci = GetDataSet(strsql);
            DataTable incis = dsInci.Tables[0];

            int nInci = -1;
            //int alarma = 0;
            foreach (System.Data.DataRow inci in incis.Rows)
            {
                try
                {
                    if (inci.IsNull("IdIncidencia"))
                    {
                        _logger.Error("Lista de Incidencias. Error. Registro de Incidencia NULO");
                        continue;
                    }

                    nInci = inci.IsNull("IdIncidencia") ? -1 :
                        incis.Columns["IdIncidencia"].DataType.Name == "UInt32" ? (int)inci.Field<uint>("IdIncidencia") :
                        inci.Field<int>("IdIncidencia");

                    var fInci = inci.IsNull("Incidencia") ? "ERROR Incidencia " + nInci.ToString() : inci.Field<string>("Incidencia");
                    var strf = inci.IsNull("Descripcion") ? /*"ERROR Descripcion " + nInci.ToString()*/fInci : inci.Field<string>("Descripcion");

                    var alarma = inci.IsNull("generaerror") ? 0 :
                        incis.Columns["generaerror"].DataType.Name == "Boolean" ? (inci.Field<bool>("generaerror") ? 1 : 0) :
                        inci.Field<int>("generaerror");

                    var nTrep = inci.IsNull("trep") ? 0 :
                        incis.Columns["trep"].DataType.Name == "UInt32" ? (int)inci.Field<uint>("trep") :
                        inci.Field<int>("trep");

                    if (filtro == null || filtro.Count == 0 || filtro.Contains(nInci.ToString()) == false)
                    {
                        _lst.Add(new U5kIncidenciaDescr
                        {
                            id = nInci,
                            desc = fInci,
                            alarm = alarma == 0 ? false : true,
                            strformat = strf,
                            Trep = nTrep
                        });
                    }
                }
                catch (Exception x)
                {
                    _logger.Error(x, String.Format("ListaDeIncidencias. Excepcion en Incidencia {0}", nInci));
                }
            }
            return _lst;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="inci_descr"></param>
        /// <param name="generaerror"></param>
        public int UpdateGeneraErrorIncidencia(string inci_descr, int generaerror)
        {
            string str_table_inci = _idioma.Name.StartsWith("en") ? "incidencias_ingles" : _idioma.Name.StartsWith("fr") ? "incidencias_frances" : "incidencias";
            string sql = string.Format("UPDATE {0} SET GENERAERROR={1} WHERE INCIDENCIA=\"{2}\";", str_table_inci, generaerror, inci_descr);
            return SqlExecute(sql);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="inci_descr"></param>
        /// <param name="generaerror"></param>
        public int UpdateGeneraErrorIncidencia(int inci, int generaerror)
        {
            string str_table_inci = _idioma.Name.StartsWith("en") ? "incidencias_ingles" : _idioma.Name.StartsWith("fr") ? "incidencias_frances" : "incidencias";
            string sql = string.Format("UPDATE {0} SET GENERAERROR={1} WHERE IDINCIDENCIA=\"{2}\";", str_table_inci, generaerror, inci);
            return SqlExecute(sql);
        }

        /// <summary>
        ///
        /// </summary>
        /// <returns></returns>
        public bool InsertaIncidencia(bool alarma, U5kIncidencia inci)
        {
            try
            {
                lock (_lock)
                {
                    string sqlInsert = string.Format("INSERT INTO historicoincidencias (IdSistema, Scv, IdIncidencia, IdHw, TipoHw, FechaHora, Descripcion) " +
                                                     "VALUES (\"{0}\",{1},{2},\"{3}\",{4},\"{5}\",\"{6}\")",
                                                     inci.sistema, inci.scv, inci.id, inci.idhw, inci.tipo, String.Format("{0:yyyy-MM-dd HH:mm:ss}", inci.fecha), inci.desc);
                    SqlExecute(sqlInsert);
                }
            }
            catch (Exception /*x*/)
            {
                throw;
            }
            return true;
        }

        /// <summary>
        /// Borra de la tabla de historico las incidencias con una antiguedad MAYOR de 'dias'
        /// 
        /// </summary>
        /// <param name="dias"></param>
        public long SupervisaTablaIncidencia(int dias)
        {
            long afectados = 0;
            try
            {
                lock (_lock)
                {
                    string fecha = String.Format("{0:yyyy-MM-dd}", DateTime.Now - new TimeSpan(dias, 0, 0, 0));
                    //string sqlDel = String.Format("DELETE FROM historicoincidencias WHERE FechaHora < \"{0}\"", fecha);
                    string sqlDel = String.Format("DELETE FROM historicoincidencias WHERE FechaHora < \"{0}\" ORDER BY FechaHora ASC LIMIT 10000;", fecha);

                    long borrados = 0;
                    while ((borrados=SqlExecute(sqlDel)) > 0)
                    {
                        afectados += borrados;
                        System.Threading.Tasks.Task.Delay(100).Wait();
                    }
                }
            }
            catch (Exception /*x*/)
            {
                throw /*x*/;
            }
            return afectados;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public List<string> ListaTops(string sistema, string todos)
        {
            List<string> _tops = new List<string>();
            List<BdtTop> tmp = GetListaTop(sistema);
            _tops.Add(todos);
            foreach (BdtTop top in tmp)
            {
                _tops.Add(top.Id);
            }

            return _tops;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public List<string> ListaGw(string sistema, string todos)
        {
            List<string> _gws = new List<string>();
            List<BdtGw> tmp = GetListaGw(sistema);
            _gws.Add(todos);
            foreach (BdtGw gw in tmp)
            {
                _gws.Add(gw.Id);
            }

            return _gws;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sistema"></param>
        /// <param name="todos"></param>
        /// <returns></returns>
        public List<String> ListaMN(string sistema="departamento")
        {
            List<String> _mneq = new List<string>();

            /** Obtener los Equipos y Añadirlos */
            List<BdtMNRd> _lequipos = BdtListaEquiposMN(sistema);
            foreach (BdtMNRd equipo in _lequipos)
                _mneq.Add(equipo.Id);

            /** Obtener las Frecuencias y Añadirlas */
            var varDest = BdtListaDestinosRadio(sistema).Where(e => _mneq.Contains(e.Id));
            if (varDest != null)
            {
                List<BdtMNRd> _ldest = varDest.ToList();
                foreach (BdtMNRd equipo in _ldest)
                {
                    if (equipo.frec != null && _mneq.Contains(equipo.frec)==false)
                        _mneq.Add(equipo.frec);
                }
            }
            return _mneq;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sql"></param>
        /// <returns></returns>
        public DataSet ConsultaHistorico(string sql)
        {
            string sqlLimit = sql + " LIMIT 1000";
            DataSet data = GetDataSet(sqlLimit);
            return data;
        }

        /** **/
        public List<U5kHistLine> ConsultaHistoricoList(string query)
        {
            List<U5kHistLine> lista = new List<U5kHistLine>();
            DataSet ds = GetDataSet(query);
            foreach (System.Data.DataRow row in ds.Tables[0].Rows)
            {
                try
                {
                    lista.Add(new U5kHistLine()
                    {
                        date = row.Field<DateTime>("FechaHora").ToString(),
                        idhw = row.Field<string>("IdHw"),
                        desc = row.Field<string>("Descripcion"),
                        acknw = row.IsNull("Reconocida") ? "" : row.Field<DateTime>("Reconocida").ToString(),
                        user = row.Field<string>("Usuario")
                    });

                    //new BdtTop() { Id = top.Field<string>("IdTop"), Ip = top.Field<string>("IpRed1") });
                }
                catch (Exception x)
                {
                    _logger.Error("U5kBdtService.ConsultaHistoricoList Exception: {0}", x.Message);
                    _logger.Trace(x, "U5kBdtService.ConsultaHistoricoList Exception");
                }
            }

            return lista;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="fechaalarma"></param>
        /// <param name="descripcion"></param>
        public void ReconoceAlarma(string user, DateTime fechaalarma, string idhw, string descripcion)
        {
            string sqlUpdate = string.Format("UPDATE historicoincidencias SET Reconocida=\"{3:yyyy-MM-dd HH:mm:ss}\", usuario=\"{4}\" " +
                                             "WHERE FechaHora=\"{0:yyyy-MM-dd HH:mm:ss}\" AND idhw=\"{1}\" AND Descripcion=\"{2}\";",
                        fechaalarma, idhw, descripcion, DateTime.Now, user);
            SqlExecute(sqlUpdate);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="query"></param>
        /// <returns></returns>
        public UInt32 GetScalar(string query)
        {
            return Convert.ToUInt32( SqlExecuteScalar(query) );
        }

        public UInt64 GetLongScalar(string query)
        {
            return SqlExecuteScalar(query);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="tableorview_name"></param>
        /// <returns></returns>
        public bool TableOrViewExist(string tableorview_name)
        {
            try
            {
                SqlExecuteScalar(String.Format("SELECT COUNT(*) FROM {0};", tableorview_name));
                return true;
            }
            catch (Exception )
            {                
            }
            return false;
        }

        /// <summary>
        /// 20170309. AGL. Para Obtener las URIS de cada TOP.
        /// </summary>
        /// <param name="sistema"></param>
        /// <param name="cfg_id"></param>
        /// <returns></returns>
        public List<Tuple<String, String>> GetUsersOnTop(string sistema)
        {
            List<Tuple<String, String>> users = new List<Tuple<string, string>>();

            string cfg_id="", cfg_date="";
            string mcast_ip = "";
            long mcast_port = 0;
            GetCfgActiva(sistema, ref cfg_id, ref cfg_date, ref mcast_ip, ref mcast_port);
            string prefix = GetAtsPrefix(sistema);

            string strsql = String.Format("SELECT s.IdTop, s.IdSector, a.IdAbonado " + 
                 "FROM sectoressectorizacion s, usuariosabonados a " + 
                 "WHERE s.Idsectorizacion=\"{0}\" AND a.IdAbonado LIKE \"{1}%\" AND a.IdSector = s.IdSector;", 
                  cfg_id, prefix);
            try
            {
                DataTable tb_users = GetDataSet(strsql).Tables[0];
                foreach (System.Data.DataRow tb_user in tb_users.Rows)
                {
                    try
                    {
                        string idTop = tb_user.Field<string>("IDTOP");
                        string idAbn = tb_user.Field<string>("IDABONADO");
                        users.Add(new Tuple<string, string>(idTop, idAbn));
                    }
                    catch (Exception x)
                    {
                        _logger.Error(x, String.Format("GetUsersOnTop. Excepcion en Fila {0}", tb_user));
                    }
                }
            }
            catch (Exception x)
            {
                throw x;
            }

            return users;
        }

        /// <summary>
        /// 20170309. AGL. Para Obtener el ID de configuracion activa.
        /// </summary>
        /// 20180220. AGL. Tambien retorna el grupo y puerto mcast de configuracion...
        /// <param name="sistema"></param>
        /// <returns></returns>
        public string GetCfgActiva(string sistema, ref string id, ref string date, ref string mcastip, ref long mcastport)
        {            
            //string strsql = String.Format("SELECT * " +
            //     "FROM sectorizaciones " +
            //     "WHERE IdSistema=\"{0}\" AND Activa=1;",
            //     sistema);
            string strsql = String.Format(
                "select idsectorizacion as c1, 0 as c2, fechaactivacion as c3 "+
                "from sectorizaciones where idsistema='{0}' and activa=1 " +
                "union select grupomulticastconfiguracion, puertomulticastconfiguracion, 0 from sistema;", sistema);
            try
            {
                DataTable tb_cfg = GetDataSet(strsql).Tables[0];
                // if (tb_cfg.Rows.Count > 0)
                if (tb_cfg.Rows.Count > 1)
                {
                    //id   = tb_cfg.Rows[0].Field<string>("IDSECTORIZACION");
                    //date = (tb_cfg.Rows[0].Field<DateTime>("FECHAACTIVACION")).ToString();
                    id = tb_cfg.Rows[0].Field<string>("c1");
                    date = tb_cfg.Rows[0].Field<string>("c3");
                    mcastip = tb_cfg.Rows[1].Field<string>("c1");
                    mcastport = tb_cfg.Rows[1].Field<long>("c2");
                }
                else
                {
                    id = "NONE";
                    date = DateTime.MinValue.ToString();
                    mcastip = "";
                    mcastport = -1;
                }
            }
            catch (Exception x)
            {
                id = "NONE";
                _logger.Error(x, String.Format("GetCfgActiva. Excepcion"));
            }

            return id;
        }

        /// <summary>
        /// Para obtener el Prefijo de Red ATS.
        /// </summary>
        /// <param name="sistema"></param>
        /// <returns></returns>
        public string GetAtsPrefix(string sistema)
        {
            string prefix;
            string strsql = String.Format("SELECT IdPrefijo " +
                 "FROM redes " +
                 "WHERE IdSistema=\"{0}\" AND IdRed=\"{1}\";",
                 sistema, "ATS");
            try
            {
                DataTable tb_cfg = GetDataSet(strsql).Tables[0];
                if (tb_cfg.Rows.Count > 0)
                {
                    uint uPrefix = tb_cfg.Rows[0].Field<uint>("IDPREFIJO");
                    prefix = uPrefix.ToString();
                }
                else
                {
                    prefix = "?";
                }
            }
            catch (Exception x)
            {
                prefix = "?";
                _logger.Error(x, String.Format("GetAtsPrefix. Excepcion"));
            }

            return prefix;
        }

        public string PbxEndpoint
        {
            get
            {
                string strsql = String.Format("select IpRed1 as endpoint from equiposeu where interno=1;");
                try 
                {
                    DataTable tb_cfg = GetDataSet(strsql).Tables[0];
                    if (tb_cfg != null && tb_cfg.Rows.Count > 0)
                    {
                        string endpoint = tb_cfg.Rows[0].Field<string>("endpoint");
                        return endpoint;
                    }
                }
                catch(Exception x)
                {
                    _logger.Error(x, String.Format("PbxEndpoint. Excepcion"));
                }
                return "";
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public class BdtStdServ
        {
            public string Name { get; set; }
            public int Estado { get; set; }
            public bool Presencia { get; set; }
        }
        public List<BdtStdServ> GetClusterInfo(string sistema)
        {
            List<BdtStdServ> cluster = new List<BdtStdServ>()
            {
                new BdtStdServ(){Name=System.Environment.MachineName,   Estado=2,   Presencia=true},
                new BdtStdServ(){Name="",   Estado=0,   Presencia=false}
            };

            string strsql = String.Format("SELECT * FROM estadocluster;");
            try
            {
                DataTable tb_cluster = GetDataSet(strsql).Tables[0];
                if (tb_cluster.Rows.Count > 1)
                {
                    cluster[0].Name = tb_cluster.Rows[0].Field<string>("NAME");
                    cluster[0].Estado = tb_cluster.Rows[0].Field<int>("ESTADO");
                    cluster[0].Presencia = tb_cluster.Rows[0].Field<bool>("PRESENCIA");

                    cluster[1].Name = tb_cluster.Rows[1].Field<string>("NAME");
                    cluster[1].Estado = tb_cluster.Rows[1].Field<int>("ESTADO");
                    cluster[1].Presencia = tb_cluster.Rows[1].Field<bool>("PRESENCIA");
                }
            }
            catch (Exception x)
            {
                _logger.Error(x, String.Format("GetClusterInfo. Excepcion"));
            }

            return cluster;
        }

        /// <summary>
        /// TODO. Ojo con los puertos...
        /// </summary>
        /// <param name="sistema"></param>
        /// <returns></returns>
        public List<BdtEuEq> ListaDestinosAtsExternos(string sistema)
        {
            List<BdtEuEq> atsDest = new List<BdtEuEq>();
            try
            {
                String strQuery = "SELECT a.IdDestino, a.IdAbonado, b.Central, b.Inicial, b.Final, c.IpRed1, c.IpRed2, c.SipPort  FROM destinosexternos AS a " +
                    "INNER JOIN rangos AS b  ON a.IdAbonado BETWEEN b.Inicial AND b.Final " +
                    "INNER JOIN equiposeu AS c ON b.Central=c.IdEquipos;";

                DataSet data = GetDataSet(strQuery);
                DataTable tb = data.Tables[0];
                foreach (DataRow row in tb.Rows)
                {
                    try
                    {
                        string id = row.Field<string>("IDDESTINO");
                        string ip = row.Field<string>("IPRED1");
                        string ip2 = row.Field<string>("IPRED2");
                        // uint tipo = row.Field<uint>("TIPOEQUIPO");
                        // long? modelo = ver < 1 ? 0 : row.Field<long?>("MODELOEQUIPO");
                        // ulong? rxortx = ver < 1 ? 0 : row.Field<ulong?>("TIPORADIO");
                        string iddestino = row.Field<string>("CENTRAL");
                        string idrecurso = row.Field<string>("IDABONADO");
                        int sip_port = row.Field<int>("SIPPORT");
                        atsDest.Add(new BdtEuEq()
                        {
                            Id = id,
                            Ip = ip,
                            Ip2 = ip2,
                            Tipo = 0, Modelo = 0, RxOrTx = 0,
                            IdDestino = iddestino,
                            IdRecurso = idrecurso,
                            SipPort = sip_port
                        });
                    }
                    catch (Exception x)
                    {
                        _logger.Error<Exception>("U5kBdtService.ListaDestinosAtsExternos Exception.", x);
                    }
                }
            }
            catch (Exception x)
            {
                _logger.Error(x, "U5kBdtService.ListaDestinosAtsExternos Exception.");
            }

            return atsDest;
        }

        /// <summary>
        /// Obtiene la lista de operadores/perfiles del sistema,
        /// </summary>
        /// <returns></returns>
        public class SystemUserInfo
        {
            public string id { get; set; }
            public string pwd { get; set; }
            public int prf { get; set; }
        }

        public List<object> SystemUsers()
        {
            var operadores = new List<object>();
            try
            {
                String strQuery = "SELECT IdOperador, Clave, NivelAcceso FROM operadores;";
                DataSet data = GetDataSet(strQuery);
                DataTable tb = data.Tables[0];
                foreach (DataRow row in tb.Rows)
                {
                    try
                    {
                        operadores.Add(new SystemUserInfo()
                        {
                            id = row.Field<string>("IdOperador"),
                            pwd = row.Field<string>("Clave"),
                            prf = (int)row.Field<uint>("NivelAcceso")
                        });
                    }
                    catch (Exception x)
                    {
                        _logger.Error<Exception>("U5kBdtService.ListaDestinosAtsExternos Exception.", x);
                    }
                }
            }
            catch (Exception x)
            {
                _logger.Error(x, "U5kBdtService.ListaOperadoresSistema Exception.");
            }
            return operadores;
        }

        public int Execute(string sql)
        {
            return SqlExecute(sql);
        }

        public DataSet ReadTableOrView(string sql)
        {
            return GetDataSet(sql);
        }

        #endregion

        #region PROTECTED

        /// <summary>
        /// 
        /// </summary>
        protected void Open()
        {
            if (Tipo == eBdt.bdtMySql)
            {
                try
                {
                    _DB = new MySqlConnection(_strConn);
                    ((MySqlConnection)_DB).Open();
                }
                catch (MySqlException e)
                {
                    _logger.Error(e, "BdtOpen Exception: ");
                    throw new Exception(e.Message);
                }
            }
            else if (Tipo == eBdt.bdtSqlite)
            {
                try
                {
                    _DB = new SQLiteConnection(_strConn);
                    ((SQLiteConnection)_DB).Open();
                }
                catch (SQLiteException e)
                {
                    _logger.Error(e, "BdtOpen Exception: ");
                    throw new Exception(e.Message);
                }
            }
            else
            {
                throw new Exception("U5kBdtService.Open: Tipo de Base de Datos no Soportado.");
            }
        }

        /// <summary>
        /// 
        /// </summary>
        protected void ReOpen()
        {
            if (_DB == null)
                Open();

            ConnectionState bdtState = Tipo == eBdt.bdtMySql ? ((MySqlConnection)_DB).State : Tipo == eBdt.bdtSqlite ? ((SQLiteConnection)_DB).State : ConnectionState.Closed;
            if (bdtState != ConnectionState.Open)
            {
                _logger.Error("BDT_NO_OPEN: {0}", bdtState);
                Open();
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="consulta"></param>
        /// <param name="tran"></param>
        /// <returns></returns>
        protected DataSet GetDataSet(string consulta/* , MySqlTransaction tran*/)
        {
            DataSet DataSetResultado = new DataSet();

            ReOpen();
            if (Tipo == eBdt.bdtMySql)
            {
                try
                {
                    lock (_lock)
                    {
                        MySqlDataAdapter myDataAdapter = new MySqlDataAdapter(consulta, ((MySqlConnection)_DB));
                        using (myDataAdapter)
                        {
                            myDataAdapter.Fill(DataSetResultado, "TablaCliente");
                        }
                    }
                }
                catch (MySqlException e)
                {
                    _logger.Error(e, "GetDataSet Exception: " + consulta);
                    throw new Exception(e.Message);
                }
                return DataSetResultado;
            }
            else if (Tipo == eBdt.bdtSqlite)
            {
                try
                {
                    lock (_lock)
                    {
                        SQLiteDataAdapter myDataAdapter = new SQLiteDataAdapter(consulta, ((SQLiteConnection)_DB));
                        using (myDataAdapter)
                        {
                            myDataAdapter.Fill(DataSetResultado, "TablaCliente");
                        }
                    }
                }
                catch (SQLiteException e)
                {
                    _logger.Error(e, "GetDataSet Exception: " + consulta);
                    throw new Exception(e.Message);
                }
                return DataSetResultado;
            }

            throw new Exception("U5kBdtService.GetDataSet: Tipo de Base de Datos no Soportado");
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sql"></param>
        /// <returns></returns>
        protected int SqlExecute(string sql)
        {
            int _ret = 0;
            ReOpen();

            if (Tipo == eBdt.bdtMySql)
            {
                try
                {
                    lock (_lock)
                    {
                        MySqlCommand cmd = new MySqlCommand(sql, ((MySqlConnection)_DB));
                        _ret = cmd.ExecuteNonQuery();
                    }
                }
                catch (MySqlException e)
                {
                    _logger.Error(e, "SqlExecute Exception: " + sql);
                    throw new Exception(e.Message);
                }
                return _ret;
            }
            else if (Tipo == eBdt.bdtSqlite)
            {
                try
                {
                    lock (_lock)
                    {
                        SQLiteCommand cmd = new SQLiteCommand(sql, ((SQLiteConnection)_DB));
                        _ret = cmd.ExecuteNonQuery();
                    }
                }
                catch (SQLiteException e)
                {
                    _logger.Error(e, "SqlExecute Exception: " + sql);
                    throw new Exception(e.Message);
                }
                return _ret;
            }

            throw new Exception("U5kBdtService.SqlExecute: Tipo de Base de Datos no Soportado ");
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sql"></param>
        /// <returns></returns>
        protected UInt64 SqlExecuteScalar(string sql)
        {
            Int64 _ret = 0;
            ReOpen();

            if (Tipo == eBdt.bdtMySql)
            {
                try
                {
                    lock (_lock)
                    {
                        MySqlCommand cmd = new MySqlCommand(sql, ((MySqlConnection)_DB));
                        object res = cmd.ExecuteScalar();
                        _ret = Convert.ToInt64(res);
                    }
                }
                catch (MySqlException e)
                {
                    _logger.Error(e, "SqlExecute Exception: " + sql);
                    throw new Exception(e.Message);
                }
                return (UInt64)_ret;
            }
            else if (Tipo == eBdt.bdtSqlite)
            {
                try
                {
                    lock (_lock)
                    {
                        SQLiteCommand cmd = new SQLiteCommand(sql, ((SQLiteConnection)_DB));
                        object res = cmd.ExecuteScalar();
                        _ret = Convert.ToInt64(res);
                    }
                }
                catch (SQLiteException e)
                {
                    _logger.Error(e, "SqlExecuteScalar Exception: " + sql);
                    throw new Exception(e.Message);
                }
                return (UInt64)_ret;
            }

            throw new Exception("U5kBdtService.ExecuteScalar: Tipo de Base de Datos no Soportado ");
        }


        #endregion

        #region Atributos y Metodos Estaticos
        /// <summary>
        /// 
        /// </summary>
        /// <param name="tipo"></param>
        /// <param name="pathorserver"></param>
        /// <returns></returns>
        static public bool CheckConnection(eBdt tipo, string pathorserver, string user = "CD40", string pwd = "cd40")
        {
            bool isConn = false;
            string strConn = U5kBdtService.BdtStrConn(tipo, pathorserver, user, pwd);
            if (tipo == eBdt.bdtMySql)
            {
                try
                {
                    using (MySqlConnection conn = new MySqlConnection(strConn))
                    {
                        conn.Open();
                        isConn = true;
                    }
                }
                catch (Exception x)
                {
                    LogManager.GetCurrentClassLogger().Error(x, String.Format("CheckMySqlBdtConnection <{0}>", strConn));
                }
            }
            else if (tipo == eBdt.bdtSqlite)
            {
                try
                {
                    using (SQLiteConnection conn = new SQLiteConnection(String.Format("Data Source={0};Version=3;", pathorserver)))
                    {
                        conn.Open();
                        isConn = true;
                    }
                }
                catch (Exception x)
                {
                    LogManager.GetCurrentClassLogger().Error(x, String.Format("CheckSqLiteBdtConnection <{0}>", strConn));
                }
            }

            return isConn;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pathorserver"></param>
        /// <returns></returns>
        static protected string BdtStrConn(eBdt Tipo, string pathorserver, string user = "CD40", string pwd = "cd40")
        {
            if (Tipo == eBdt.bdtMySql)
                return String.Format("Server={0};Database={3};Uid={1};Pwd={2};", pathorserver, user, pwd, BdtSchema);
            else if (Tipo == eBdt.bdtSqlite)
                return String.Format("Data Source={0};Version=3;", pathorserver);

            throw new Exception("U5kBdtService.BdtStrConn: Tipo de Base de Datos no Soportado ");
        }
        /// <summary>
        /// Para sincronizar los accesos de los diferentes thread's.
        /// </summary>
        // private static Mutex _mutex = new Mutex(false, "FF8CFE8C-3DBD-4BD7-BDC7-04C9DBBD770A");
        private static System.Object _lock = new System.Object();
        /// <summary>
        /// 
        /// </summary>
        static public string BdtSchema = "new_cd40";
        #endregion
    }

    /// <summary>
    /// Descripcion de las Incidencias.
    /// </summary>
    [Serializable()]
    public class U5kIncidenciaDescr
    {
        public int id { get; set; }
        public string desc { get; set; }
        public string strformat { get; set; }
        public bool alarm { get; set; }
        /** 20190305. */
        public int Trep { get; set; }
    }

    [Serializable()]
    public class U5kHistLine
    {
        public string date { get; set; }
        public string idhw { get; set; }
        public string desc { get; set; }
        public string acknw { get; set; }
        public string user { get; set; }
    }

    /** Integracion REDAN / ULISES */
    /// <summary>
    /// Convierte los Eventos Historicos de REDAN en Eventos Historicos de ULISES...
    /// </summary>
    public class Redan2UlisesHist
    {
        enum enHistClass {Normal, Evento, Operacion, Llamada, Ignorar}

        class HistClass
        {
            public int ulInci { get; set; }
            public enHistClass clase { get; set; }
            public string texto { get; set; }
            public List<string> call_data = new List<string>();
            /// <summary>
            /// 
            /// </summary>
            /// <param name="_inci"></param>
            /// <param name="_clase"></param>
            /// <param name="_texto"></param>
            public HistClass(int _inci, enHistClass _clase, string _texto)
            {
                ulInci = _inci;
                clase = _clase;
                texto = _texto;
                read_call_data();
            }
            /// <summary>
            /// 
            /// </summary>
            protected void read_call_data()
            {
                call_data.Clear();
                if (clase == enHistClass.Llamada)
                {   // texto: KEY1;KEY2;...
                    call_data.Clear();
                    string[] keys = texto.Split(';');
                    foreach (string key in keys)
                        call_data.Add(key);
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        static Dictionary<int, HistClass> tbConvInci = new Dictionary<int, HistClass>();

        /// <summary>
        /// 
        /// </summary>
        /// 2005: 1970-01-01 19.58.20: EMPLAZ: CGW1: 872:- :192.168.0.41: Principal: Caida>>>
        /// <param name="strRedanInci"></param>
        public Redan2UlisesHist(string strRedanInci)
        {
            string[] campos = strRedanInci.Split(':');
            if (campos.Count() < 6)
                return;

            redanCode = int.Parse(campos[0]);
            redanDate = DateTime.ParseExact(campos[1], "yyyy-MM-dd HH.mm.ss", System.Globalization.CultureInfo.InvariantCulture);
            redanSite = campos[2];
            redanIdhw = campos[3];
            redanUser = campos[5];

            for (int ipar = 6; ipar < campos.Count(); ipar++)
                redanParams.Add(campos[ipar]);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="inci"></param>
        /// <param name="parametros"></param>
        /// <returns></returns>
        public bool UlisesInci(out U5kIncidencia inci, out List<object> parametros)
        {
            inci = new U5kIncidencia() { id = redanCode, fecha = redanDate, idhw = redanIdhw, tipo=1 };
            parametros = new List<object>();

            if (tbConvInci.Count == 0)
                loadInci();

            if (tbConvInci.ContainsKey(redanCode))
            {
                HistClass histclass = tbConvInci[redanCode];

                inci.id = histclass.ulInci;
                if (histclass.clase == enHistClass.Evento ||
                    histclass.clase == enHistClass.Operacion)
                {
                    parametros.Add(histclass.texto);
                    string parametro_global = "";
                    foreach (string parametro in redanParams)
                        parametro_global += (parametro + ",");
                    parametros.Add(parametro_global);
                    return true;
                }
                else if (histclass.clase == enHistClass.Llamada)
                {
                    if (redanParams.Count < 2)
                        return false;

                    parametros.Add(redanParams[0]);                 // Id Recurso.
                    Dictionary<string, string> callData = CallDataParse(redanParams[1].ToString());

                    foreach (string key in histclass.call_data)
                    {
                        if (callData.ContainsKey(key))
                            parametros.Add(callData[key]);
                        else
                            parametros.Add("???");
                    }
                    return true;
                }
                else if (histclass.clase == enHistClass.Normal)
                {
                    parametros = redanParams;
                    return true;
                }                
            }
            return false;
        }

        /// <summary>
        /// IF=5|TRO=T_BCN|NO=311101|ND=316197|PR=4|EST=O|TRA=0|CH=0:25/11/2016 12:10:05
        /// </summary>
        /// <param name="strData"></param>
        /// <returns></returns>
        protected Dictionary<string, string> CallDataParse(string strData)
        {
            Dictionary<string, string> callData = new Dictionary<string, string>();
            string[] pairs = strData.Split('|');
            foreach (string pair in pairs)
            {
                string[] keyvalue = pair.Split('=');
                if (keyvalue.Length == 2)
                {
                    if (callData.ContainsKey(keyvalue[0]) == false)
                        callData[keyvalue[0]] = keyvalue[1];
                }
            }

            return callData;
        }

        /// <summary>
        /// 
        /// </summary>
        protected void loadInci()
        {
            string[] lines = System.IO.File.ReadAllLines(@"tbConvInci.txt");
            foreach (string line in lines)
            {
                string[] parts = line.Split(',');
                if (parts.Count() == 4)
                {
                    int redanInci = Int32.Parse(parts[0]);
                    tbConvInci.Add(redanInci, new HistClass(
                        Int32.Parse(parts[1]),
                        parts[2]=="Normal" ? enHistClass.Normal :
                            parts[2] == "Llamada" ? enHistClass.Llamada :
                            parts[2]=="Evento" ? enHistClass.Evento : 
                            parts[2]=="Operacion" ? enHistClass.Operacion : enHistClass.Ignorar,
                        parts[3]));
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        protected int redanCode;
        protected DateTime redanDate;
        protected string redanSite;
        protected string redanIdhw;
        protected string redanUser;
        protected List<Object> redanParams = new List<object>();        
    }

    /** 20171018. Backup / Restore de la Base de Datos.. */
    public class U5kiDbHelper
    {
        public static bool IsUserAdministrator()
        {
            bool isAdmin;
            try
            {
                WindowsIdentity user = WindowsIdentity.GetCurrent();
                WindowsPrincipal principal = new WindowsPrincipal(user);
                isAdmin = principal.IsInRole(WindowsBuiltInRole.Administrator);
            }
            catch (UnauthorizedAccessException )
            {
                isAdmin = false;
            }
            catch (Exception )
            {
                isAdmin = false;
            }
            return isAdmin;
        }

        static int nfiles = 0;
        static IEnumerable<string> GetFiles(string path)
        {
            Queue<string> queue = new Queue<string>();
            queue.Enqueue(path);
            while (queue.Count > 0)
            {
                path = queue.Dequeue();
                try
                {
                    foreach (string subDir in Directory.GetDirectories(path))
                    {
                        queue.Enqueue(subDir);
                    }
                }
                catch (Exception x)
                {
                    LogManager.GetCurrentClassLogger().Error(x, "U5kiDbHelper.GetFiles Exception");
                }
                string[] files = null;
                try
                {
                    files = Directory.GetFiles(path);
                }
                catch (Exception x)
                {
                    LogManager.GetCurrentClassLogger().Error(x, "U5kiDbHelper.GetFiles Exception");
                }
                if (files != null)
                {
                    for (int i = 0; i < files.Length; i++)
                    {
                        yield return files[i];
                    }
                }
            }
        }
        static string FindFile(string[] paths, string nameoffile, string fileVersion = "")
        {
            nfiles = 0;
            foreach (string path in paths)
            {
                foreach (string file in GetFiles(path))
                {
                    nfiles++;
                    if (file.EndsWith(nameoffile))
                    {
                        if (fileVersion != "")
                        {
                            var versionInfo = FileVersionInfo.GetVersionInfo(file);
                            string version = versionInfo.ProductVersion;
                            if (version.StartsWith(fileVersion))
                            {
                                return file;
                            }
                        }
                        else
                        {
                            return file;
                        }
                    }
                }
            }
            return string.Empty;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public class BackupInciItem
        {
            public DateTime When { get; set; }
            public String What { get; set; }
            public bool IsError { get; set; }
        }
        public static List<BackupInciItem> Backup(string host, string bdt, string user, string pwd, string fileVersion="", int days2con=7)
        {
            var InciItems = new List<BackupInciItem>();
            try
            {
                // LogManager.GetCurrentClassLogger().Warn("Iniciando Backup de Base de Datos.");
                InciItems.Add(new BackupInciItem() { When = DateTime.Now, What = "Iniciando Backup de Base de Datos", IsError = false });
                /** Busco el fichero de backup.... */
                string sqldumpfile = FindFile( new string [] {
                    Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles),
                    Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86) }, "mysqldump.exe", fileVersion);
                if (sqldumpfile != String.Empty)
                {
                    ProcessStartInfo psi = new ProcessStartInfo();
                    psi.FileName = sqldumpfile;
                    psi.RedirectStandardInput = false;
                    psi.RedirectStandardOutput = true;
                    psi.RedirectStandardError = true;
                    psi.StandardOutputEncoding = Encoding.UTF8;
                    /** 20181001. Para que el backup salve todas las Bases de datos... */
                    psi.Arguments = string.Format(@"-u{0} -p{1} -h{2} --add-drop-database --add-drop-table --add-locks --comments --create-options --dump-date --lock-tables --all-databases --routines",
                        user, pwd, host, bdt);
                    psi.UseShellExecute = false;
                    psi.CreateNoWindow = true;

                    Process process = Process.Start(psi);

                    string output = process.StandardOutput.ReadToEnd();
                    string strerror = process.StandardError.ReadToEnd();

                    bool error = true;
                    if (!strerror.Contains("Got error:") && !strerror.Contains("no-beep"))
                    {
                        error = false;
                        File.WriteAllText(String.Format("{0:yyyyMMddHHmm}-{1}-bkp.sql", DateTime.Now, bdt), output);

                        /** Limpio los mas antiguos */
#if DEBUG
                        DateTime lastTime = DateTime.Now.AddMinutes(-days2con);
#else
                        DateTime lastTime = DateTime.Today.AddDays(-days2con);        // TODO Poner Configurable...
#endif
                        string[] files2delete = Directory.GetFiles(".", "*-bkp.sql").Where(f => (new FileInfo(f)).LastWriteTime < lastTime).ToArray();
                        foreach (string file in files2delete)
                        {
                            InciItems.Add(new BackupInciItem() { When = DateTime.Now, What = "Eliminado backup antiguo: " + file, IsError = false });
                            File.Delete(file);
                        }
                    }
                    if (error)
                    {
                        InciItems.Add(new BackupInciItem() { When = DateTime.Now, What = "Error en Ejecucion de Backup: " + strerror, IsError = true });
                    }
                    else
                    {
                        InciItems.Add(new BackupInciItem() { When = DateTime.Now, What = "Backup de Base de Datos completado.", IsError = false });
                    }
//                    LogManager.GetCurrentClassLogger().Warn("Backup de Base de Datos. {0} {1}", error ? "Error en Ejecucion" : "Ejecutado", error ? strerror : "");
                    File.WriteAllText("mysql-err.txt", strerror);
                }
                else
                {
                    // LogManager.GetCurrentClassLogger().Error("Error en Backup de Base de Datos. No se encuentra MYSQLDUMP.EXE");
                    InciItems.Add(new BackupInciItem() { When = DateTime.Now, What = "Error en Backup de Base de Datos. No se encuentra MYSQLDUMP.EXE", IsError = true });
                }
            }
            catch (Exception x)
            {
                // LogManager.GetCurrentClassLogger().Error(x, "U5kiDbHelper.Backup Exception");
                InciItems.Add(new BackupInciItem() { When = DateTime.Now, What = "U5kiDbHelper.Backup Exception: " + x.Message, IsError = true });
            }
            return InciItems;
        }
        public static List<BackupInciItem> NewBackup(string host, string bdt, string user, string pwd, string fileVersion = "", int days2con = 7)
        {
            var InciItems = new List<BackupInciItem>();
            try
            {
                InciItems.Add(new BackupInciItem() { When = DateTime.Now, What = "Iniciando Backup de Base de Datos", IsError = false });
                /** Busco el fichero de backup.... */
                string sqldumpfile = FindFile(new string[] {
                    Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles),
                    Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86) }, "mysqldump.exe", fileVersion);
                if (sqldumpfile != String.Empty)
                {
                    string dumpCmd = string.Format("\"{3}\" -u{0} -p{1} -h{2} --database new_cd40 new_cd40_trans",
                        user, pwd, host, sqldumpfile);
                    string cmd = "cmd.exe";
                    string fileout = String.Format("{0:yyyyMMddHHmm}-{1}-bkp.sql", DateTime.Now, bdt);
                    string Arguments = "/c " + dumpCmd + " > " + fileout;

                    ProcessStartInfo psi = new ProcessStartInfo()
                    {
                        FileName = cmd,
                        RedirectStandardInput = false,
                        RedirectStandardOutput = true,
                        RedirectStandardError = true,
                        StandardOutputEncoding = Encoding.UTF8,
                        Arguments = Arguments,
                        UseShellExecute = false,
                        CreateNoWindow = true
                    };
                    Process process = Process.Start(psi);

                    string output = process.StandardOutput.ReadToEnd();
                    string strerror = process.StandardError.ReadToEnd();

                    bool error = true;
                    if (!strerror.Contains("Got error:") && !strerror.Contains("no-beep"))
                    {
                        error = false;

                        /** Limpio los mas antiguos */
#if DEBUG
                        DateTime lastTime = DateTime.Now.AddMinutes(-days2con);
#else
                        DateTime lastTime = DateTime.Today.AddDays(-days2con);        // TODO Poner Configurable...
#endif
                        string[] files2delete = Directory.GetFiles(".", "*-bkp.sql").Where(f => (new FileInfo(f)).LastWriteTime < lastTime).ToArray();
                        foreach (string file in files2delete)
                        {
                            InciItems.Add(new BackupInciItem() { When = DateTime.Now, What = "Eliminado backup antiguo: " + file, IsError = false });
                            File.Delete(file);
                        }
                    }
                    if (error)
                    {
                        InciItems.Add(new BackupInciItem() { When = DateTime.Now, What = "Error en Ejecucion de Backup: " + strerror, IsError = true });
                    }
                    else
                    {
                        InciItems.Add(new BackupInciItem() { When = DateTime.Now, What = "Backup de Base de Datos completado.", IsError = false });
                    }
                    //                    LogManager.GetCurrentClassLogger().Warn("Backup de Base de Datos. {0} {1}", error ? "Error en Ejecucion" : "Ejecutado", error ? strerror : "");
                    File.WriteAllText("mysql-err.txt", strerror);
                }
                else
                {
                    // LogManager.GetCurrentClassLogger().Error("Error en Backup de Base de Datos. No se encuentra MYSQLDUMP.EXE");
                    InciItems.Add(new BackupInciItem() { When = DateTime.Now, What = "Error en Backup de Base de Datos. No se encuentra MYSQLDUMP.EXE", IsError = true });
                }
            }
            catch (Exception x)
            {
                // LogManager.GetCurrentClassLogger().Error(x, "U5kiDbHelper.Backup Exception");
                InciItems.Add(new BackupInciItem() { When = DateTime.Now, What = "U5kiDbHelper.Backup Exception: " + x.Message, IsError = true });
            }
            return InciItems;
        }
    }

    /** 20180710. Para la configuracion del servicio en la base de datos */
    public class U5kiLocalConfigInDb : IDisposable
    {
        #region Esquemas y Consultas
        /** Esquema de la Tabla y Nombre de la misma */
        const String TableName = "MttoLocalConfig";
        string createTableQuery = string.Format(
            @"CREATE TABLE `{0}` (
            `sid` smallint(5) unsigned NOT NULL AUTO_INCREMENT,
            `pkey` varchar(120) NOT NULL DEFAULT '',
            `val` varchar(120) NOT NULL DEFAULT '',
            PRIMARY KEY (`sid`) ) ENGINE = MyISAM AUTO_INCREMENT = 1 DEFAULT CHARSET = utf8;", TableName);
        string readTableQuery = string.Format("SELECT * FROM {0} ORDER BY sid;", TableName);
        string updateTableQuery(string key, string newval)
        {
            return string.Format("UPDATE {0} SET val='{1}' WHERE pkey='{2}';", TableName, newval, key);
        }
        string insertTableQuery(string key, string val)
        {
            return string.Format("INSERT INTO {0} (pkey, val) VALUES ('{1}', '{2}');", TableName, key, val);
        }
        string dropTableQuery = string.Format("DROP TABLE {0};", TableName);

        /** Esquema del Dato on-line */
        public class U5kiLocalConfigItem
        {
            public string Key { get; set; }
            public string Val { get; set; }
        }
        #endregion

        #region Constructores
        public U5kiLocalConfigInDb(U5kBdtService bdt, bool init=false)
        {
            Bdt = bdt;
            if (init)
            {
                DeleteTable();
            }
            Load();
        }

        public void Dispose()
        {
            UpdateBdt();
        }
        #endregion

        #region Properties 
        public U5kBdtService Bdt { get; set; }

        public string Idioma
        {
            get { return GetProperty<string>("Idioma", "es"); }
            set { SetProperty<string>("Idioma", value); }
        }
        
        public bool ServidorDual
        {
            get { return GetProperty<bool>("ServidorDual", true); }
            set { SetProperty<bool>("ServidorDual", value); }
        }

        public bool HayReloj
        {
            get { return GetProperty<bool>("HayReloj", true); }
            set { SetProperty<bool>("HayReloj", value); }
        }

        public bool HaySacta
        {
            get { return GetProperty<bool>("HaySacta", true); }
            set { SetProperty<bool>("HaySacta", value); }
        }

        public bool HayAltavozHF
        {
            get { return GetProperty<bool>("HayAltavozHF", true); }
            set { SetProperty<bool>("HayAltavozHF", value); }
        }

        public bool SonidoAlarmas
        {
            get { return GetProperty<bool>("SonidoAlarmas", true); }
            set { SetProperty<bool>("SonidoAlarmas", value); }
        }

        public bool GenerarHistoricos
        {
            get { return GetProperty<bool>("GenerarHistoricos", true); }
            set { SetProperty<bool>("GenerarHistoricos", value); }
        }

        public bool Historico_PttSqhOnBdt
        {
            get { return GetProperty<bool>("PttAndSqhOnBdt", true); }
            set { SetProperty<bool>("PttAndSqhOnBdt", value); }
        }

        public int DiasEnHistorico
        {
            get { return GetProperty<int>("DiasEnHistorico", 180); }
            set { SetProperty<int>("DiasEnHistorico", value); }
        }

        public int LineasIncidencias
        {
            get { return GetProperty<int>("LineasIncidencias", 32); }
            set { SetProperty<int>("LineasIncidencias", value); }
        }

        #endregion

        #region Publics
        public void Consolidate()
        {
            UpdateBdt();
        }

        #endregion

        #region Internals
        void Load()
        {
            if (Bdt==null)
            {
                LoadDefaults();
            }
            else if (Bdt.TableOrViewExist(TableName)==false)
            {
                CreateTable();
                LoadDefaults();
                InitDataOnBdt();
            }
            else
            {
                ReadBdt();
            }
        }
        void LoadDefaults()
        {
            OnLineData.AddRange(Defaults);
        }
        void ReadBdt()
        {
            if (Bdt != null)
            {
                OnLineData.Clear();
                var dbdata = Bdt.ReadTableOrView(readTableQuery);
                if (dbdata.Tables.Count > 0)
                {
                    foreach(System.Data.DataRow item in dbdata.Tables[0].Rows)
                    {
                        OnLineData.Add(new U5kiLocalConfigItem()
                        {
                            Key = item.Field<string>("pkey"),
                            Val = item.Field<string>("val"),
                        });
                    }
                }
                else
                {
                    // Todo.
                }
            }
            else
            {
                    // Todo
            }
        }
        void UpdateBdt()
        {
            if (Bdt != null)
            {
                OnLineData.ForEach(item =>
                {
                    Bdt.Execute(updateTableQuery(item.Key, item.Val));
                });
            }
            else
            {
                // todo
            }
        }
        void InitDataOnBdt()
        {
            if (Bdt != null)
            {
                OnLineData.ForEach(item =>
                {
                    Bdt.Execute(insertTableQuery(item.Key, item.Val));
                });
            }
            else
            {
                // todo
            }
        }
        void CreateTable()
        {
            if (Bdt != null)
            {
                Bdt.Execute(createTableQuery);
            }
        }
        void DeleteTable()
        {
            if (Bdt != null)
            {
                Bdt.Execute(dropTableQuery);
            }
        }

        T GetProperty<T>(string name, T def)
        {
            var item = OnLineData.Where(i => i.Key == name).FirstOrDefault();
            if (item != null)
            {
                return (T)Convert.ChangeType(item.Val, typeof(T));
            }
            return (T)Convert.ChangeType(def, typeof(T));
        }

        void SetProperty<T>(string name, T val)
        {
            var item = OnLineData.Where(i => i.Key == name).FirstOrDefault();
            if (item != null)
                item.Val = val.ToString();
        }

        #endregion

        #region Private
        private U5kBdtService _bdt = null;
        private List<U5kiLocalConfigItem> OnLineData = new List<U5kiLocalConfigItem>();
        private List<U5kiLocalConfigItem> Defaults = new List<U5kiLocalConfigItem>()
        {
            new U5kiLocalConfigItem(){Key="Idioma", Val="es" },
            new U5kiLocalConfigItem(){Key="ServidorDual", Val="true" },
            new U5kiLocalConfigItem(){Key="HayReloj", Val="true" },
            new U5kiLocalConfigItem(){Key="HaySacta", Val="false"},
            new U5kiLocalConfigItem(){Key="HayAltavozHF", Val="false"},
            new U5kiLocalConfigItem(){Key="SonidoAlarmas", Val="false"},
            new U5kiLocalConfigItem(){Key="GenerarHistoricos", Val="true"},
            new U5kiLocalConfigItem(){Key="PttAndSqhOnBdt", Val="false"},
            new U5kiLocalConfigItem(){Key="DiasEnHistorico", Val="180"},
            new U5kiLocalConfigItem(){Key="LineasIncidencias", Val="32"},
        };
        #endregion Private
    }
}
