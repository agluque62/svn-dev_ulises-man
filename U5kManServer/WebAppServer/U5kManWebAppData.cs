using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Converters;

using U5kBaseDatos;
using NucleoGeneric;

using Utilities;

namespace U5kManServer.WebAppServer
{
    /// <summary>
    /// Clase General...
    /// </summary>
    public class U5kManWebAppData : BaseCode
    {
        /// <summary>
        /// 
        /// </summary>
        // protected static Logger _logger = NLog.LogManager.GetCurrentClassLogger();

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        static public string JSerialize<JObject>(JObject obj)
        {
            return JsonConvert.SerializeObject(obj);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="JObject"></typeparam>
        /// <param name="strData"></param>
        /// <returns></returns>
        static public JObject JDeserialize<JObject>(string strData)
        {
            return JsonConvert.DeserializeObject<JObject>(strData);
        }
    }

    /// <summary>
    /// Resultado Genérico de una operación.
    /// </summary>
    class U5kManWADResultado : U5kManWebAppData
    {
        public string res { get; set; }
    }

    /// <summary>
    /// Lista de Incidencias Pendientes.
    /// </summary>
    public class U5kManWADInci : U5kManWebAppData
    {
        public class InciData
        {
            public string time { get; set; }
            public string inci { get; set; }
            public int id { get; set; }
        }

        public class InciRec
        {
            public string user { get; set; }
            public InciData inci { get; set; }
        }

        /// <summary>
        /// 
        /// </summary>
        public List<InciData> lista = new List<InciData>();
        public int HashCode { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public U5kManWADInci(bool bGenerate = false)
        {
            if (bGenerate)
            {
                lock (U5kManService._last_inci)
                {
                    foreach (U5kManLastInciList.eListaInci inci in U5kManService._last_inci._lista)
                    {
                        lista.Add(new InciData() 
                        { 
                            time = inci._fecha.ToString(),
                            inci = /*EncryptionHelper.CAE_cifrar(inci._desc)*/inci._desc, 
                            id = (int)inci._id 
                        });
                    }
                    // La Lista ya debe estar ordenada...
                    // HashCode = lista.GetHashCode();
                    HashCode = HashCodeGet();
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        private int HashCodeGet()
        {
            int hash = 0;
            foreach (InciData item in lista)
            {
                hash += item.time.GetHashCode();
                hash += item.inci.GetHashCode();
            }
            return hash;
        }

        /// <summary>
        /// 
        /// </summary>
        static public void Reconoce(string jStrInci)
        {
            InciRec inci = JDeserialize<InciRec>(jStrInci);
            U5kManService.stReconoceAlarma(inci.user, DateTime.Parse(inci.inci.time), inci.inci.inci);
        }
    }

    /// <summary>
    /// Estado General.
    /// </summary>
    class U5kManWADStd : U5kManWebAppData
    {
        public class itemData
        {
            public string name { get; set; }
            public int enable { get; set; }
            public int std { get; set; }
            public int sel { get; set; }
            public string url { get; set; }
        }

        /// <summary>
        /// 
        /// </summary>
        public string version { get; set; }
        public string cfg { get; set; }
        public int hf { get; set; }
        public int recw { get; set; }
        public itemData sv1 { get; set; }
        public itemData sv2 { get; set; }
        public itemData cwp { get; set; }
        public itemData gws { get; set; }
#if _HAY_NODEBOX__
        public itemData nbx { get; set; }
#endif
        public itemData pbx { get; set; }
        public itemData ntp { get; set; }
        public bool sactaservicerunning { get; set; }
        public bool sactaserviceenabled { get; set; }
        public itemData sct1 { get; set;}
        public itemData sct2 { get; set; }
        public itemData ext { get; set; }
#if _HAY_NODEBOX__
#if _LISTANBX_V0
        public class itemNbx
        {
            public string name { get; set; }
            public string modo { get; set; }
        }
#elif _LISTANBX_
        public class itemNbx
        {
            public string name { get; set; }                // Para compatibilidad con el anterior.
            public string modo { get; set; }

            public string url { get; set; }
            public int CfgService { get; set; }
            public int RadioService { get; set; }
            public int TifxService { get; set; }
            public int PresenceService { get; set; }
            public bool Running { get; set; }
        }
        public List<itemNbx> nbxs = new List<itemNbx>();
#endif
#else
        public dynamic csi { get; set; }
#endif
        public int perfil { get; set; }
        public string lang { get; set; }

        public int rd_status { get; set; }
        public int tf_status { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public U5kManWADStd(U5kManStdData gdt, string user ="false", bool bGenerate = false)
        {
            if (bGenerate)
            {
#if STD_ACCESS_V0
                U5KStdGeneral stdg = U5kManService._std._gen;
                lock (U5kManService._std._gen)
#else
                U5KStdGeneral stdg = gdt.STDG;
#endif
                {
                    version = U5kGenericos.Version;
                    cfg = stdg.CfgId;                // .cfgVersion;
                    hf = U5kManService.cfgSettings/*Properties.u5kManServer.Default*/.HayAltavozHF ? 1 : 0;
                    recw = U5kManService.cfgSettings/*Properties.u5kManServer.Default*/.OpcOpeCableGrabacion ? 1 : 0;
                    sv1 = new itemData()
                    {
                        name = stdg.stdServ1.name,
                        enable = 1,
                        std = (int)stdg.stdServ1.Estado,
                        sel = (int)stdg.stdServ1.Seleccionado,
                        url = U5kManService.cfgSettings/*Properties.u5kManServer.Default*/.ServidorDual ?
                                String.Format("http://{0}/NucleoDF/U5kCfg/Cluster/Default.aspx", U5kManServer.Properties.u5kManServer.Default.MySqlServer) : ""
                    };
                    sv2 = new itemData()
                    {
                        name = stdg.stdServ2.name,
                        enable = stdg.bDualServ ? 1 : 0,
                        std = (int)stdg.stdServ2.Estado,
                        sel = (int)stdg.stdServ2.Seleccionado,
                        url = U5kManService.cfgSettings/*Properties.u5kManServer.Default*/.ServidorDual ?
                                String.Format("http://{0}/NucleoDF/U5kCfg/Cluster/Default.aspx", U5kManServer.Properties.u5kManServer.Default.MySqlServer) : ""
                    };
                    cwp = new itemData()
                    {
                        name = idiomas.strings.WAP_MSG_003 /* "Puestos de Operador"*/,
                        enable = 1,
                        std = (int)stdg.stdGlobalPos,
                        sel = 0,
                        url = ""
                    };
                    gws = new itemData()
                    {
                        name = idiomas.strings.WAP_MSG_004 /* "Pasarelas"*/,
                        enable = 1,
                        std = (int)stdg.stdScv1.Estado,
                        sel = 0,
                        url = ""
                    };

#if _HAY_NODEBOX__

#if _LISTANBX_V0
                    /** Busco el Master en la lista */
                    int nbx_count = U5kManService._std._gen.lstNbx.Count;
                    StdServ masternbx = U5kManService._std._gen.lstNbx.Find(x => x.Seleccionado == sel.Seleccionado);
                    List<StdServ> nbx_master_list = masternbx != null ? U5kManService._std._gen.lstNbx.Where(x => x.Seleccionado == sel.Seleccionado).ToList() : null;
#elif _LISTANBX_
                    /** Busco el Master en la lista */
                    int nbx_count = stdg.lstNbx.Count;
                    U5KStdGeneral.StdNbx masternbx = stdg.lstNbx.Find(x => x.CfgService==U5KStdGeneral.NbxServiceState.Master);
                    List<U5KStdGeneral.StdNbx> nbx_master_list = masternbx != null ? stdg.lstNbx.Where(x => x.CfgService == U5KStdGeneral.NbxServiceState.Master).ToList() : null;
#endif
                    nbx = new itemData()
                    {
#if _LISTANBX_V0
                        name = masternbx != null ? masternbx.name : "???",
                        enable = 1,
                        std = (int)(masternbx != null ? (nbx_master_list.Count > 1 ? std.Error :
                                                         nbx_count > 1 ? masternbx.Estado : std.Aviso) : std.NoInfo),
                        sel = 0,
                        url = masternbx != null ? U5kGenericos.NodeboxUrl(masternbx.name) : "???"
#elif _LISTANBX_
                        name = masternbx != null ? masternbx.ip : "Unknown",
                        enable = 1,
                        std = (int)(masternbx != null ? (nbx_master_list.Count > 1 ? std.Error :
                                                         nbx_count > 1 ? std.Ok : std.Aviso) : std.NoInfo),
                        sel = 0,
                        url = masternbx != null ? "http://" + masternbx.ip + ":" + masternbx.webport.ToString() + "/" : "???"
#else
                    name = U5kManService._std._gen.stdNbx.name,
                    enable = 1,
                    std = (int)U5kManService._std._gen.stdNbx.Estado,
                    sel = 0,
                    url = U5kGenericos.NodeboxUrl(U5kManService._std._gen.stdNbx.name)
#endif
                    };
#else
                    Services.CentralServicesMonitor.Monitor.DataGetForWebServer((csid) =>
                    {
                        csi = csid;
                    });
#endif
                    pbx = new itemData()
                    {
                        name = stdg.stdPabx.name,
                        enable = stdg.HayPbx ? 1 : 0,
                        std = (int)stdg.stdPabx.Estado,
                        sel = 0,
                        url = U5kGenericos.PabxUrl(stdg.stdPabx.name)
                    };
                    ntp = new itemData()
                    {
                        name = stdg.stdClock.name,
                        enable = stdg.HayReloj ? 1 : 0,
                        std = (int)stdg.stdClock.Estado,
                        sel = 0,
                        url = ""
                    };
                    sactaservicerunning = stdg.SactaService == std.Ok;
                    sactaserviceenabled = stdg.SactaServiceEnabled;
                    sct1 = new itemData()
                    {
                        name = "SACTA-1",
                        enable = stdg.HaySacta ? 1 : 0,
                        std = (int)stdg.stdSacta1,
                        sel = 0,
                        url = ""
                    };
                    sct2 = new itemData()
                    {
                        name = "SACTA-2",
                        enable = stdg.HaySacta ? 1 : 0,
                        std = (int)stdg.stdSacta2,
                        sel = 0,
                        url = ""
                    };
#if _HAY_NODEBOX__

#if _LISTANBX_V0
                    nbxs.Clear();
                    foreach (StdServ nodebox in U5kManService._std._gen.lstNbx)
                    {
                        nbxs.Add(new itemNbx() { name = nodebox.name, modo = nodebox.Seleccionado == sel.Seleccionado ? "Master" : "Slave" });
                    }
#elif _LISTANBX_
                    nbxs.Clear();
                    foreach (U5KStdGeneral.StdNbx nodebox in stdg.lstNbx)
                    {
                        nbxs.Add(new itemNbx() 
                        {
                            name = nodebox.ip,
                            modo = nodebox.CfgService == U5KStdGeneral.NbxServiceState.Master ? "Master" : "Slave",
                            CfgService = (int )nodebox.CfgService,
                            RadioService = (int )nodebox.RadioService,
                            TifxService = (int)nodebox.TifxService,
                            PresenceService = (int)nodebox.PresenceService,
                            url = "http://" + nodebox.ip + ":" + nodebox.webport.ToString() + "/",
                            Running = nodebox.Running
                        });
                    }
#endif
#else
                    // TODO...
#endif
                    ext = new itemData()
                    {
                        name = idiomas.strings.EquiposExternos,
                        enable = 1,
                        std = (int)stdg.stdGlobalExt,
                        sel = 0,
                        url = ""
                    };
                    var operador = gdt.usuarios.Find(x => ((U5kBdtService.SystemUserInfo)x).id == user);
                    perfil = operador == null ? 0 : ((U5kBdtService.SystemUserInfo)operador).prf;
                    lang = U5kManService.cfgSettings/*Properties.u5kManServer.Default*/.Idioma;

                    /** Estado Global Radio */
#if _HAY_NODEBOX__
                    var fr = U5kManService._sessions_data.Count();                              // Frecuencias Configuradas.
                    var fa = U5kManService._sessions_data.Where(f => f.fstd == 0).Count();      // Frecuencias No disponibles
                    var fd = U5kManService._sessions_data.Where(f => f.fstd == 2).Count();      // Frecuencias Degradadas.
                    rd_status = fr==0 ? -1 /** No INFO */ : fa > 0 ? 2 /** Alarma */ : fd > 0 ? 1 /** Warning */: 0; /** OK */

                    /** Estado Global Telefonia: 0=>OK, 1=>Con PROXY ALT, 2=>Emergencia */
                    tf_status = U5kManService.tlf_mode;

#else
                    //var sessions_data = JsonConvert.DeserializeObject<List<U5kManService.radioSessionData>>(Services.CentralServicesMonitor.Monitor.RadioSessionsString);
                    //var fr = sessions_data.Count();                              // Frecuencias Configuradas.
                    //var fa = sessions_data.Where(f => f.fstd == 0).Count();      // Frecuencias No disponibles
                    //var fd = sessions_data.Where(f => f.fstd == 2).Count();      // Frecuencias Degradadas.
                    //rd_status = fr == 0 ? -1 /** No INFO */ : fa > 0 ? 2 /** Alarma */ : fd > 0 ? 1 /** Warning */: 0; /** OK */
                    var rs = Services.CentralServicesMonitor.Monitor.GlobalRadioStatus;
                    rd_status = rs == std.NoInfo ? -1 : rs == std.Alarma ? 2 : rs == std.Aviso ? 1 : 0;

                    ///** 20181010. De los datos obtenemos el estado de emergencia */
                    //JArray grps = JsonHelper.SafeJArrayParse(Services.CentralServicesMonitor.Monitor.PresenceDataString);
                    //JObject prx_grp = grps == null ? null :
                    //    grps.Where(u => u.Value<int>("tp") == 4).FirstOrDefault() as JObject;
                    //JProperty prx_prop = prx_grp == null ? null : prx_grp.Property("res");
                    //JArray proxies = prx_prop == null ? null : prx_prop.Value as JArray;
                    //int ppal = proxies == null ? 0 : proxies.Where(u => u.Value<int>("tp") == 5 && u.Value<int>("std") == 0).Count();
                    //int alt = proxies == null ? 0 : proxies.Where(u => u.Value<int>("tp") == 6 && u.Value<int>("std") == 0).Count();
                    //tf_status = ppal > 0 ? 0 /** OK */ : alt > 0 ? 1 /** DEG */ : 2 /** EMG */;

                    var tfs = Services.CentralServicesMonitor.Monitor.GlobalPhoneStatus;
                    tf_status = tfs == std.Ok ? 0 /** OK */ : tfs==std.Aviso ? 1 /** DEG */ : 2 /** EMG */;

#endif
                }
            }
        }
    }

    /// <summary>
    /// Estado de Operadores.
    /// </summary>
    class U5kManWADCwps : U5kManWebAppData
    {
        public class CWPData
        {
            public string name { get; set; }
            public string ip { get; set; }
            public int std { get; set; }
            public int panel { get; set; }
            public int jack_exe { get; set; }
            public int jack_ayu { get; set; }
            public int alt_r { get; set; }
            public int alt_t { get; set; }
            public int lan1 { get; set; }
            public int lan2 { get; set; }
            public int alt_hf { get; set; }
            public int rec_w { get; set; }
        }

        /// <summary>
        /// 
        /// </summary>
        public List<CWPData> lista = new List<CWPData>();

        /// <summary>
        /// 
        /// </summary>
        public U5kManWADCwps(U5kManStdData gdt, bool bGenerate)
        {
            if (bGenerate)
            {
#if STD_ACCESS_V0
                lock (U5kManService._std.stdpos)
                {
                    foreach (stdPos pos in U5kManService._std.stdpos)
                    {
                        lista.Add(new CWPData()
                        {
                            name = pos.name,
                            ip = pos.ip,
                            std = (int)pos.stdg,
                            panel = pos.panel == std.Ok ? 1 : 0,
                            jack_exe = pos.jack_exe == std.Ok ? 1 : 0,
                            jack_ayu = pos.jack_ayu == std.Ok ? 1 : 0,
                            alt_r = pos.alt_r == std.Ok ? 1 : 0,
                            alt_t = pos.alt_t == std.Ok ? 1 : 0,
                            lan1 = pos.lan1 == std.Ok ? 1 : pos.lan1 == std.Error ? 2 : 0,
                            lan2 = pos.lan2 == std.Ok ? 1 : pos.lan2 == std.Error ? 2 : 0,
                            alt_hf = Properties.u5kManServer.Default.HayAltavozHF ? (pos.alt_hf == std.Ok ? 1 : 0) : -1,
                            rec_w = pos.rec_w == std.Ok ? 1 : 0
                        });
                    }
                }
#else
                List<stdPos> stdpos = gdt.STDTOPS;
                foreach (stdPos pos in stdpos)
                {
                    lista.Add(new CWPData()
                    {
                        name = pos.name,
                        ip = pos.ip,
                        std = (int)pos.stdg,
                        panel = pos.panel == std.Ok ? 1 : 0,
                        jack_exe = pos.jack_exe == std.Ok ? 1 : 0,
                        jack_ayu = pos.jack_ayu == std.Ok ? 1 : 0,
                        alt_r = pos.alt_r == std.Ok ? 1 : 0,
                        alt_t = pos.alt_t == std.Ok ? 1 : 0,
                        lan1 = pos.lan1 == std.Ok ? 1 : pos.lan1 == std.Error ? 2 : 0,
                        lan2 = pos.lan2 == std.Ok ? 1 : pos.lan2 == std.Error ? 2 : 0,
                        alt_hf = U5kManService.cfgSettings/*Properties.u5kManServer.Default*/.HayAltavozHF ? (pos.alt_hf == std.Ok ? 1 : 0) : -1,
                        rec_w = U5kManService.cfgSettings/*Properties.u5kManServer.Default*/.OpcOpeCableGrabacion ? (pos.rec_w == std.Ok ? 1 : 0) : -1
                    });
                }
#endif
            }
        }
    }

    /// <summary>
    /// Estado de Pasarelas
    /// </summary>
    class U5kManWADGws : U5kManWebAppData
    {
        /// <summary>
        /// 
        /// </summary>
        public class GWData
        {
            public string name { get; set; }
            public string ip { get; set; }
            public int tipo { get; set; }
            public int std { get; set; }
            public int main { get; set; }
            public int lan1 { get; set; }
            public int lan2 { get; set; }
            /** */
            public int cpu0 { get; set; }   // 0 NP, 1: Main, 2 Standby
            public int cpu1 { get; set; }   // 0 NP, 1: Main, 2 Standby
        };

        /// <summary>
        /// 
        /// </summary>
        public List<GWData> lista = new List<GWData>();

        public int gdt { get; set; }

        public U5kManWADGws(U5kManStdData gdata, bool bGenerate = false)
        {
            if (bGenerate)
            {
#if STD_ACCESS_V0
                lock (U5kManService._std.stdgws)
                {
                    foreach (stdGw gw in U5kManService._std.stdgws)
                    {
                        lista.Add(new GWData()
                        {
                            name = gw.name,
                            ip = gw.ip,
                            tipo = gw.Dual ? 1 : 0,
                            std = (int)gw.std,
                            main = gw.Dual == false ? 0 : gw.gwA.Seleccionada ? 0 : gw.gwB.Seleccionada ? 1 : -1,
                            lan1 = (gw.Dual == false || gw.gwA.Seleccionada) ? (gw.gwA.lan1 == std.Ok ? 1 : 0) : (gw.gwB.lan1 == std.Ok ? 1 : 0),
                            lan2 = (gw.Dual == false || gw.gwA.Seleccionada) ? (gw.gwA.lan2 == std.Ok ? 1 : 0) : (gw.gwB.lan2 == std.Ok ? 1 : 0)
                        });
                    }
                }
#else
                List<stdGw> stdgws = gdata.STDGWS;
                lista = stdgws.Select(gw => new GWData()
                {
                    name = gw.name,
                    ip = gw.ip,
                    tipo = gw.Dual ? 1 : 0,
                    std = (int)gw.std,
                    main = gw.Dual == false ? 0 : gw.gwA.Seleccionada ? 0 : gw.gwB.Seleccionada ? 1 : -1,
                    lan1 = (gw.Dual == false || gw.gwA.Seleccionada) ? (gw.gwA.lan1 == std.Ok ? 1 : 0) : (gw.gwB.lan1 == std.Ok ? 1 : 0),
                    lan2 = (gw.Dual == false || gw.gwA.Seleccionada) ? (gw.gwA.lan2 == std.Ok ? 1 : 0) : (gw.gwB.lan2 == std.Ok ? 1 : 0),
                    cpu0 = gw.Dual == false ? (gw.gwA.presente ? 1 : 0) : (gw.gwA.presente ? (gw.gwA.Seleccionada ? 1 : 2) : (0)),
                    cpu1 = gw.Dual == false ? (0) : (gw.gwB.presente ? (gw.gwB.Seleccionada ? 1 : 2) : (0))
                }).ToList();

                gdt = Properties.u5kManServer.Default.GatewaysDualityType;
#endif
            }
        }
    }

    /// <summary>
    /// Detalle de Pasarela
    /// </summary>
    class U5kManWADGwData : U5kManWebAppData
    {
        /// <summary>
        /// 
        /// </summary>
        public class itemVersion
        {
            public string line { get; set; }
        }
        /// <summary>
        /// 
        /// </summary>
        public class itemTar
        {
            public int cfg { get; set; }
            public int not { get; set; }
        }
        /// <summary>
        /// 
        /// </summary>
        public class itemRec
        {
            public string name { get; set; }
            public int cfg { get; set; }
            public int not { get; set; }
            public int std { get; set; }
        }
        /// <summary>
        /// 
        /// </summary>
        public class itemCpu
        {
            public string ip { get; set; }
            public int ntp { get; set; }
            public int lan1 { get; set; }
            public int lan2 { get; set; }
            public List<itemTar> tars { get; set; }
            public List<itemRec> recs = new List<itemRec>();
#if GW_STD_V1
            public int sipMod { get; set; }
            public int snmpMod { get; set; }
            public int cfgMod { get; set; }
            public int fa { get; set; }
#endif
        }

        public string name { get; set; }
        public string ip { get; set; }
        public int tipo { get; set; }
        public int std { get; set; }
        public int main { get; set; }
        public int fa { get; set; }
#if _GETVER_UNIFI_V0
        public List<itemVersion> versiones = new List<itemVersion>();
#else
        public string versiones { get; set; }
#endif
        public List<itemCpu> cpus = new List<itemCpu>();

        /// <summary>
        /// 
        /// </summary>
        /// <param name="name"></param>
        public U5kManWADGwData(U5kManStdData gdata, string Name, bool bGenerate = false)
        {
            if (bGenerate)
            {
#if STD_ACCESS_V0
                lock (U5kManService._std.stdgws)
                {
                    foreach (stdGw gw in U5kManService._std.stdgws)
                    {
                        if (gw.name == Name)
                        {
                            /** Parametros Generales */
                            name = gw.name;
                            ip = gw.ip;
                            tipo = gw.Dual ? 1 : 0;
                            std = (int)gw.std;
                            main = gw.Dual == false ? 0 : gw.gwA.Seleccionada ? 0 : gw.gwB.Seleccionada ? 1 : -1;
                            fa = std == 0 ? 0 : 1;

                            /** Versiones */
                            versiones = FormatVersiones((gw.Dual == false || gw.gwA.Seleccionada) ? gw.gwA.version : gw.gwB.version);

                            /** Datos de Interfaces CPU 0/1 */
                            for (int cpu = 0; cpu < 2; cpu++)
                            {
                                stdPhGw pgw = cpu == 0 ? gw.gwA : gw.gwB;
                                cpus.Add(new itemCpu()
                                {
                                    ip = pgw.ip,
                                    ntp = 0,
                                    lan1 = pgw.lan1 == U5kManServer.std.Ok ? 1 : 0,
                                    lan2 = pgw.lan2 == U5kManServer.std.Ok ? 1 : 0,
                                    tars = PhysicalGwTars(pgw),
                                    recs = PhysicalGwResources(pgw)
                                });
                            }
                            return;
                        }
                    }
                }
#else
                List<stdGw> stdgws = gdata.STDGWS;
                stdGw gw = stdgws.Where(i => i.name == Name).FirstOrDefault();
                if (gw != null)
                {
                    /** Parametros Generales */
                    name = gw.name;
                    ip = gw.ip;
                    tipo = gw.Dual ? 1 : 0;
                    std = (int)gw.std;
                    main = gw.Dual == false ? 0 : gw.gwA.Seleccionada ? 0 : gw.gwB.Seleccionada ? 1 : -1;

                    /** Versiones */
                    versiones = FormatVersiones((gw.Dual == false || gw.gwA.Seleccionada) ? gw.gwA.version : gw.gwB.version);

                    /** Datos de Interfaces CPU 0/1 */
                    for (int cpu = 0; cpu < 2; cpu++)
                    {
                        stdPhGw pgw = cpu == 0 ? gw.gwA : gw.gwB;
                        cpus.Add(new itemCpu()
                        {
                            ip = pgw.ip,
                            ntp = 0,
                            lan1 = pgw.lan1 == U5kManServer.std.Ok ? 1 : 0,
                            lan2 = pgw.lan2 == U5kManServer.std.Ok ? 1 : 0,
                            tars = PhysicalGwTars(pgw),
                            recs = PhysicalGwResources(pgw)
#if GW_STD_V1
                            , cfgMod = pgw.CfgMod.Std == U5kManServer.std.Ok ? 1 : 0
                            , sipMod = pgw.SipMod.Std == U5kManServer.std.Ok ? 1 : 0
                            , snmpMod = pgw.SnmpMod.Std == U5kManServer.std.Ok ? 1 : 0
                            , fa = pgw.stdFA == U5kManServer.std.Ok ? 1 : 0
#endif
                        });
                    }
                    fa = gw.gwA.stdFA == U5kManServer.std.Ok || gw.gwB.stdFA == U5kManServer.std.Ok ? 1 : 0;
                }
#endif
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public static void MainStandByChange(string name)
        {
            U5kManService.stMainStandbyChange(name, 0);
        }

#if _GETVER_UNIFI_V0
        /// <summary>
        /// 
        /// </summary>
        /// <param name="strVer"></param>
        /// <returns></returns>
        protected List<itemVersion> FormatVersiones(string strVer)
        {
            List<itemVersion> version = new List<itemVersion>();
            // strVer = U5kGenericos.CleanInput(strVer);
            using (StringReader reader = new StringReader(strVer))
            {
                string rline;
                while ((rline = reader.ReadLine()) != null)
                {
                    // rline = U5kGenericos.CleanInput(rline);
                    version.Add(new itemVersion() { line = rline });
                }
            }

            return version;
        }
#else
        protected string FormatVersiones(string strVer)
        {
            return strVer;
        }
#endif

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pgw"></param>
        /// <returns></returns>
        protected List<itemRec> PhysicalGwResources(stdPhGw pgw)
        {
            List<itemRec> recursos = new List<itemRec>();
            foreach (stdSlot tar in pgw.slots)
            {
                foreach (stdRec rec in tar.rec)
                {
                    recursos.Add(new itemRec()
                    {
                        name = rec.name,
                        cfg = (int)rec.tipo,
                        not = (int)(rec.presente ? rec.tipo_online : trc.rcNotipo),
                        /** Los subtipos 3 (TXHF) si estan presentes estan OK */
                        std = (int)(rec.presente ? (rec.Stpo==3 ? U5kManServer.std.Ok : rec.std_online) : U5kManServer.std.NoInfo)
                    });
                }
            }

            return recursos;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pgw"></param>
        /// <returns></returns>
        protected List<itemTar> PhysicalGwTars(stdPhGw pgw)
        {
            List<itemTar> tars = new List<itemTar>();
            foreach (stdSlot tar in pgw.slots)
            {
                tars.Add(new itemTar() { cfg = (tar.std_cfg == U5kManServer.std.Ok) ? 1 : 0, not = (tar.std_online == U5kManServer.std.Ok) ? 1 : 0 });
            }
            return tars;
        }
    }

    /// <summary>
    /// Estado Equipos Externos.
    /// </summary>
    class U5kManWADExtEqu : U5kManWebAppData
    {
        /// <summary>
        /// 
        /// </summary>
        public class itemEqu
        {
            public string name { get; set; }
            public string ip1 { get; set; }
            public string ip2 { get; set; }
            public int std { get; set; }
            public int tipo { get; set; }
            public int lan1 { get; set; }
            public int lan2 { get; set; }
            public int modelo { get; set; }
            public int txrx { get; set; }
            public int std_sip { get; set; }
            public string uri { get; set; }
            public string lor { get; set; }
        }
        /// <summary>
        /// 
        /// </summary>
        public List<itemEqu> lista = new List<itemEqu>();
        /// <summary>
        /// 
        /// </summary>
        public U5kManWADExtEqu(U5kManStdData gdata, bool bGenerate = false)
        {
            if (bGenerate)
            {
#if STD_ACCESS_V0
                foreach (U5kManStdEquiposEurocae.EquipoEurocae equipo in U5kManService._std.stdeqeu.Equipos)
                {
                    lista.Add(new itemEqu()
                    {
                        name = equipo.sip_user ?? equipo.Id,
                        ip1 = equipo.Ip1,
                        ip2 = equipo.Ip2,
                        std = (int )equipo.EstadoGeneral,
                        tipo = equipo.Tipo,
                        modelo = equipo.Modelo,
                        txrx = equipo.RxTx,
                        lan1 = (int)equipo.EstadoRed1,
                        lan2 = (int)equipo.EstadoRed2,
                        std_sip = (int)equipo.EstadoSip,
                        uri = String.Format("sip:{0}@{1}:{2}",equipo.sip_user, equipo.Ip1, equipo.sip_port)
                    });
                }
#else
                lista = gdata.STDEQS.Select(equipo => new itemEqu()
                    {
                        name = equipo.sip_user ?? equipo.Id,
                        ip1 = equipo.Ip1,
                        ip2 = equipo.Ip2,
                        std = (int)equipo.EstadoGeneral,
                        tipo = equipo.Tipo,
                        modelo = equipo.Modelo,
                        txrx = equipo.RxTx,
                        lan1 = (int)equipo.EstadoRed1,
                        lan2 = (int)equipo.EstadoRed2,
                        std_sip = (int)equipo.EstadoSip,
                        uri = String.Format("sip:{0}@{1}:{2}", equipo.sip_user, equipo.Ip1, equipo.sip_port),
                        lor = equipo.LastOptionsResponse
                    }).ToList();
#endif
            }
        }

        //private int std_global(std lan1, std lan2, std sip)
        //{
        //    if (lan1 == std.Ok || lan2 == std.Ok)
        //    {
        //        if (sip != std.Ok)
        //            return (int)std.Error;
        //        return (int)std.Ok;
        //    }
        //    else if (lan1 == std.NoInfo && lan2 == std.NoInfo)
        //    {
        //        return (int )std.NoInfo;
        //    }
        //    return (int )std.Aviso;
        //}
    }


    class U5kManWADExtAtsDst : U5kManWebAppData
    {
        public class itemDst
        {
            public string name { get; set; }
            public string centro { get; set; }
            public string ip1 { get; set; }
            public string ip2 { get; set; }
            public string ats { get; set; }
            public string uri { get; set; }

            public int std { get; set; }
            public int lan1 { get; set; }
            public int lan2 { get; set; }
            public int std_sip { get; set; }
        }
        public List<itemDst> lista = new List<itemDst>();
        public U5kManWADExtAtsDst(U5kManStdData gdata, bool bGenerate = false)
        {
            if (bGenerate)
            {
                foreach (EquipoEurocae equipo in gdata.STDEQS)
                {
                    lista.Add(new itemDst()
                    {
                        name = equipo.Id,
                        centro = equipo.fid,
                        ip1 = equipo.Ip1,
                        ip2 = equipo.Ip2,
                        ats = equipo.sip_user,
                        uri = String.Format("sip:{0}@{1}:{2}", equipo.sip_user, equipo.Ip1, equipo.sip_port),
                        std = (int)equipo.EstadoGeneral,
                        lan1 = (int)equipo.EstadoRed1,
                        lan2 = (int)equipo.EstadoRed2,
                        std_sip = (int)equipo.EstadoSip
                    });
                }
            }
        }
    }

    /// <summary>
    /// Estado abonados PBX
    /// </summary>
    class U5kManWADPbx : U5kManWebAppData
    {
        /// <summary>
        /// 
        /// </summary>
        public class itemPabx
        {
            public string name { get; set; }
            public int std { get; set; }
        }
        /// <summary>
        /// 
        /// </summary>
        public List<itemPabx> lista = new List<itemPabx>();
        /// <summary>
        /// 
        /// </summary>
        public U5kManWADPbx(U5kManStdData gdata, bool bGenerate = false)
        {
            if (bGenerate)
            {
#if STD_ACCESS_V0
                foreach (Uv5kManDestinosPabx.DestinoPabx destino in U5kManService._std.pabxdest.Destinos)
                {
                    lista.Add(new itemPabx()
                    {
                        name = destino.Id,
                        std = (int)destino.Estado
                    });
                }
#else
                lista = gdata.STDPBXS.Select(d => new itemPabx() { name = d.Id, std = (int)d.Estado }).ToList();
#endif
            }
        }
    }

    /// <summary>
    /// Lista de Operadores.
    /// </summary>
    class U5kManWADDbCwps : U5kManWebAppData
    {
        /// <summary>
        /// 
        /// </summary>
        public class itemDbCWP
        {
            public string id { get; set; }
        }
        /// <summary>
        /// 
        /// </summary>
        public List<itemDbCWP> lista = new List<itemDbCWP>();
        /// <summary>
        /// 
        /// </summary>
        public U5kManWADDbCwps(U5kManStdData gdata, bool bGenerate = false)
        {
            if (bGenerate)
            {
#if STD_ACCESS_V0
                lock (U5kManService._std.stdpos)
                {
                    foreach (stdPos pos in U5kManService._std.stdpos)
                    {
                        lista.Add(new itemDbCWP()
                        {
                            id = pos.name
                        });
                    }
                }
#else
                List<stdPos> stdpos = gdata.STDTOPS;
                foreach (stdPos pos in stdpos)
                {
                    lista.Add(new itemDbCWP()
                    {
                        id = pos.name
                    });
                }
#endif
            }
        }
    }

    /// <summary>
    /// Lista de Pasarelas.
    /// </summary>
    class U5kManWADDbGws : U5kManWebAppData
    {
        /// <summary>
        /// 
        /// </summary>
        public class itemDbGW
        {
            public string id { get; set; }
        }
        /// <summary>
        /// 
        /// </summary>
        public List<itemDbGW> lista = new List<itemDbGW>();
        /// <summary>
        /// 
        /// </summary>
        public U5kManWADDbGws(U5kManStdData gdata, bool bGenerate)
        {
            if (bGenerate)
            {
#if STD_ACCESS_V0
                lock (U5kManService._std.stdgws)
                {
                    foreach (stdGw gw in U5kManService._std.stdgws)
                    {
                        lista.Add(new itemDbGW()
                        {
                            id = gw.name
                        });
                    }
                }
#else
                List<stdGw> stdgws = gdata.STDGWS;
                lista = stdgws.Select(gw => new itemDbGW() { id = gw.name }).ToList();
#endif
            }
        }
    }

    class U5kManAllhard : U5kManWebAppData
    {
        public class itemHard
        {
            public string Id { get; set; }
            public int tipo { get; set; }
        }
        public List<itemHard> items { get; set; }
        public U5kManAllhard(U5kManStdData gdata )
        {
            items = new List<itemHard>();
#if STD_ACCESS_V0
            /** Añado los Operadores */
            lock (U5kManService._std.stdpos)
            {
                foreach (stdPos pos in U5kManService._std.stdpos)
                {
                    items.Add(new itemHard()
                    {
                        Id = pos.name, tipo = 0
                    });
                }
            }
            /** Añado las Pasarelas */
            lock (U5kManService._std.stdgws)
            {
                foreach (stdGw gw in U5kManService._std.stdgws)
                {
                    items.Add(new itemHard()
                    {
                        Id = gw.name, tipo = 1
                    });
                }
            }
            /** Añado los equipos*/
            lock (U5kManService._std.stdeqeu.Equipos)
            {
                foreach (U5kManStdEquiposEurocae.EquipoEurocae equipo in U5kManService._std.stdeqeu.Equipos)
                {
                    items.Add(new itemHard()
                    {
                        Id = equipo.sip_user ?? equipo.Id,
                        tipo = equipo.Tipo
                    });
                }
            }
#else
            /** Añado los Operadores */
            items.AddRange(gdata.STDTOPS.Select(item => new itemHard() {Id = item.name, tipo = 0 }).ToList()); 

            /** Añado las Pasarelas */
            items.AddRange(gdata.STDGWS.Select(gw => new itemHard() { Id = gw.name, tipo = 1 }).ToList());

            /** Añado los equipos*/
            items.AddRange(gdata.STDEQS.Select(eq => new itemHard() { Id = eq.sip_user ?? eq.Id, tipo = eq.Tipo }).ToList());

#endif
        }
    }

    /// <summary>
    /// Lista de ITEMS MN
    /// </summary>
    class U5kManWADDbMNItems : U5kManWebAppData
    {
        /// <summary>
        /// 
        /// </summary>
        public class itemDb
        {
            public string id { get; set; }
        }
        /// <summary>
        /// 
        /// </summary>
        public List<itemDb> lista = new List<itemDb>();
        /// <summary>
        /// 
        /// </summary>
        public U5kManWADDbMNItems(bool bGenerate)
        {
            if (bGenerate)
            {
                List<string> _lista = U5kManService.bdtListaItemsMN();
                foreach (String str in _lista)
                {
                    lista.Add(new itemDb() { id = str });
                }
            }
        }
    }

    /// <summary>
    /// Configuracion de Tipo de Incidencias.
    /// </summary>
    class U5kManWADDbInci : U5kManWebAppData
    {
        /// <summary>
        /// 
        /// </summary>
        public List<U5kIncidenciaDescr> lista = new List<U5kIncidenciaDescr>();
        /// <summary>
        /// 
        /// </summary>
        public U5kManWADDbInci(bool bGenerate = false)
        {
            if (bGenerate)
            {
                try
                {
                    lista = U5kManService.stListaIncidencias((idioma)=> { LogDebug<U5kManWADDbInci>("Lista de Incidencias en " + idioma); });
                }
                catch (Exception x)
                {
                    LogException<U5kManWADDbInci>( "", x);
                }
            }
        }
        /// <summary>
        /// 
        /// </summary>
        public void Save()
        {
            foreach (U5kIncidenciaDescr inci in lista)
            {
                try
                {
                    U5kManService.bdtUpdateIncidencia(inci);
                }
                catch (Exception x)
                {
                    LogException<U5kManWADDbInci>( "", x);
                }
            }

            // Hay que Reiniciar el Servicio Temporizadamente...
            U5kGenericos.ResetService = true;
        }
    }

    /// <summary>
    /// Consulta de Historico
    /// </summary>
    class U5kManWADDbHist : U5kManWebAppData
    {
        /// <summary>
        /// 
        /// </summary>
        public class Filtro
        {
            public DateTime dtDesde { get; set; }
            public DateTime dtHasta { get; set; }
            public int tpMat { get; set; }
            public string Mat { get; set; }
            public string txt { get; set; }
            public string limit { get; set; }
            public List<string> Inci { get; set; }

            /// <summary>
            /// 
            /// </summary>
            /// <returns></returns>
            public string SqlQuery
            {
                get
                {
                    string strConsulta = string.Format("SELECT * FROM HISTORICOINCIDENCIAS WHERE ({0}{1}{2}{3}{4}) ORDER BY FECHAHORA DESC LIMIT " + limit /*1000"*/,
                        /** Fecha Hora */
                        FiltroFechas,
                        FiltroTipoHardware,
                        FiltroIdHardware,
                        FiltroIncidencias(),
                        FiltroRegexpTextoExt
                        );
                    return strConsulta;
                }
            }

            /// <summary>
            /// 
            /// </summary>
            /// <param name="desde"></param>
            /// <param name="hasta"></param>
            /// <returns></returns>
            protected string FiltroFechas
            {
                get
                {
                    //dtHasta += new TimeSpan(1, 0, 0, 0);
                    //string filtro2 = string.Format("(  FECHAHORA BETWEEN '{0:yyyy-MM-dd}' AND '{1:yyyy-MM-dd}')",
                    //                dtDesde, dtHasta);
                    DateTime dtDesdeF = dtDesde.ToLocalTime();    // new DateTime(dtDesde.Year, dtDesde.Month, dtDesde.Day);
                    DateTime dtHastaF = dtHasta.ToLocalTime();    // new DateTime(dtHasta.Year, dtHasta.Month, dtHasta.Day, 23, 59, 59);
                    string filtro2 = string.Format("(  FECHAHORA BETWEEN '{0:yyyy-MM-dd HH:mm}' AND '{1:yyyy-MM-dd HH:mm}')",
                                    dtDesdeF, dtHastaF);
                    return filtro2;
                }
            }

            /// <summary>
            /// 
            /// </summary>
            protected string FiltroTipoHardware
            {
                get
                {
                    string strFiltro = "";
                    switch (tpMat)
                    {
                        case 1:           // Operadores.
                            strFiltro = " AND (TIPOHW = 0)";
                            break;
                        case 2:           // Pasarelas..
                            strFiltro = " AND (TIPOHW = 1)";
                            break;
                        default:            // Resto...
                            strFiltro = ""; // " AND (TIPOHW = 4)";
                            break;
                    }
                    return strFiltro;
                }
            }

            /// <summary>
            /// 
            /// </summary>
            protected string FiltroIdHardware
            {
                get
                {
                    if (/*tpMat == 0 || */
                        /*tpMat == 3 || */
                        /*tpMat == 4 || */
                        /*tpMat == 5 || */
                        Mat == "" ||
                        Mat == idiomas.strings.WAP_MSG_005 /* Todas */ ||
                        Mat == idiomas.strings.WAP_MSG_014 /* Todos */)
                        return "";
                    return string.Format(" AND (IDHW LIKE '{0}%')", Mat);
                }
            }

            /// <summary>
            /// 
            /// </summary>
            protected string FiltroGrupoIncidencias
            {
                get
                {
                    string strFiltro = "";
                    switch (tpMat)
                    {
                        case 0:          // Generales... (i.id >= 50 && i.id < 1000)
                            strFiltro = " AND ( (IDINCIDENCIA >= 50 AND IDINCIDENCIA < 1000) )";
                            break;
                        case 1:           // Operadores. (i.id >= 1000 && i.id < 2000)
                            strFiltro = " AND ( (IDINCIDENCIA >= 1000 AND IDINCIDENCIA < 2000) )";
                            break;
                        case 2:           // Pasarelas.. (i.id >= 2000 && i.id < 3000)
                            strFiltro = " AND ( (IDINCIDENCIA >= 2000 AND IDINCIDENCIA < 3000) )";
                            break;
                        case 3:           // HF. (i.id < 50) 
                            strFiltro = " AND ( (IDINCIDENCIA < 50) )";
                            break;
                        case 4:           // M+N.. (i.id >= 3050 && i.id < 3100)
                            strFiltro = " AND ( (IDINCIDENCIA >= 3050 AND IDINCIDENCIA < 3200) )";
                            break;
                        case 5:           // Equipo Externos
                            strFiltro = " AND ( (IDINCIDENCIA >= 3000 AND IDINCIDENCIA < 3050) )";
                            break;
                        default:
                            strFiltro = " AND ( (IDINCIDENCIA < 5000) )";
                            break;
                    }
                    return strFiltro;
                }
            }

            /// <summary>
            /// 
            /// </summary>
            /// <returns></returns>
            protected string FiltroIncidencias()
            {
                if (Inci.Count > 0)
                {
                    string filtro = " AND (";
                    foreach (string valor in Inci)
                    {
                        if (valor == "-1")
                            return FiltroGrupoIncidencias;

                        filtro += string.Format("IDINCIDENCIA = {0} OR ", valor);
                    }
                    filtro = filtro.Substring(0, filtro.Length - 3) + ")";
                    return filtro;
                }
                // Si no hay incidencias Seleccionadas o Se ha seleccionado 'todas'... Retorna Grupo de Incidencias..
                return FiltroGrupoIncidencias;
            }
            /// <summary>
            /// 
            /// </summary>
            /// <returns></returns>
            protected string FiltroTexto_1
            {
                get
                {
                    string strFiltro = "";                    
                    if (txt != null && txt != string.Empty)
                    {
                        strFiltro = " AND (Descripcion LIKE '%" + txt + "%')";
                    }
                    return strFiltro;
                }
            }
            /// <summary>
            /// 
            /// </summary>
            /// <returns></returns>
            protected string FiltroTexto
            {
                get
                {
                    string strFiltro = "";
                    if (txt != null && txt != string.Empty)
                    {
                        string[] campos = txt.Split(';');
                        strFiltro = " AND (";
                        foreach (string campo in campos)
                        {
                            strFiltro += String.Format("Descripcion LIKE '%{0}%' AND ", campo.Trim());
                        }
                        strFiltro = strFiltro.Substring(0, strFiltro.Length - 4) + ")";
                    }
                    return strFiltro;
                }
            }
            /// <summary>
            /// 
            /// </summary>
            protected string FiltroRegexpTexto
            {
                get
                {
                    string strFiltro = "";
                    if (txt != null && txt != string.Empty)
                    {
                        strFiltro = " AND (Descripcion REGEXP '" + txt + "')";
                    }
                    return strFiltro;
                }
            }

            /// <summary>
            /// 
            /// </summary>
            /// <returns></returns>
            protected string FiltroRegexpTextoExt
            {
                get
                {
                    string strFiltro = "";
                    if (txt != null && txt != string.Empty)
                    {
                        string[] campos = txt.Split('&');
                        strFiltro = " AND (";
                        foreach (string campo in campos)
                        {
                            strFiltro += String.Format("Descripcion REGEXP '{0}' AND ", campo.Trim());
                        }
                        strFiltro = strFiltro.Substring(0, strFiltro.Length - 4) + ")";
                    }
                    return strFiltro;
                }
            }
        }
        /// <summary>
        /// 
        /// </summary>
        public List<U5kHistLine> lista = null;
        /// <summary>
        /// 
        /// </summary>
        /// <param name="jFiltro"></param>
        public U5kManWADDbHist(string jFiltro)
        {
            Filtro f = JDeserialize<Filtro>(jFiltro);
            string strQuery = f.SqlQuery;

            lista = U5kManService.bdtConsultaHistorico(strQuery);
            LogDebug<U5kManWADDbHist>("HistQuery: " + strQuery);
        }
    }

    /// <summary>
    /// Consulta de Estadistica
    /// </summary>
    class U5kManWADDbEstadistica : U5kManWebAppData
    {
        class Filtro
        {
            public DateTime desde { get; set; }
            public DateTime hasta { get; set; }
            public U5kEstadisticaTiposElementos tipo { get; set; }
            public List<string> elementos { get; set; }

            public void Normalize()
            {
                if (hasta < desde)
                    hasta = desde;
            }
        }
        /// <summary>
        /// 
        /// </summary>
        public U5kEstadisticaResultado res = null;
        /// <summary>
        /// 
        /// </summary>
        /// <param name="jfiltro"></param>
        public U5kManWADDbEstadistica(string jfiltro)
        {
            Filtro f = JDeserialize<Filtro>(jfiltro);
            f.Normalize();
            res = U5kEstadisticaProc.Estadisticas.Calcula(f.desde, f.hasta, f.tipo, f.elementos);
        }
    }

    /// <summary>
    /// 
    /// </summary>
    class U5kManWADSnmpOptions : U5kManWebAppData
    {
        /// <summary>
        /// 
        /// </summary>
        public class snmpOption
        {
            public string id { get; set; }
            public int tp { get; set; }
            public List<string> opt { get; set; }
            public object val { get; set; }
            public string key { get; set; }
            public int show { get; set; }
        }
        /// <summary>
        /// 
        /// </summary>
        public List<snmpOption> snmpOptions = new List<snmpOption>()
        {
            new snmpOption()
                {
                    id=idiomas.strings.VERSION,
                    tp=1, 
                    opt = new List<string>()
                    {
                        idiomas.strings.V2,
                        idiomas.strings.V3
                    }, 
                    show=0,
                    key="Snmp_AgentVersion",
                    val=""
                },
            new snmpOption()
                {
                    id=idiomas.strings.SNMPPORT,
                    tp=0, 
                    opt = null, 
                    show=0,
                    key="Snmp_AgentPort", 
                    val=""
                },
            new snmpOption()
                {
                    id=idiomas.strings.SNMPTRAPPORT,
                    tp=0, 
                    opt = null, 
                    show=0,
                    key="Snmp_AgentListenTrapPort", 
                    val=""
                },
            new snmpOption()
                {
                    id=idiomas.strings.IDGETCOMM,
                    tp=0, 
                    opt = null, 
                    show=1,
                    key="Snmp_V2AgentGetComm", 
                    val=""
                },
            new snmpOption()
                {
                    id=idiomas.strings.IDSETCOMM,
                    tp=0, 
                    opt = null, 
                    show=1,
                    key="Snmp_V2AgentSetComm", 
                    val=""
                },
            new snmpOption()
                {
                    id=idiomas.strings.USERS,
                    tp=2, 
                    opt = new List<string>(), 
                    show=2,
                    key="Snmp_V3Users", 
                    val=""
                }
        };
        public U5kManWADSnmpOptions(bool bGenerate = false)
        {
            if (bGenerate)
            {
                foreach (snmpOption item in snmpOptions)
                {
                    item.val = PropertyGet(item.key);
                }
            }
        }
        /// <summary>
        /// 
        /// </summary>
        public void Save()
        {
            foreach (snmpOption opt in snmpOptions)
            {
                PropertySet(opt.key, opt.val);
            }
            U5kManServer.Properties.u5kManServer.Default.Save();
            // Hay que Reiniciar el Servicio Temporizadamente...
            U5kGenericos.ResetService = true;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        protected object PropertyGet(string key)
        {
            U5kManServer.Properties.u5kManServer Prop = U5kManServer.Properties.u5kManServer.Default;
            switch (key)
            {
                case "Snmp_AgentVersion":
                    return Prop.Snmp_AgentVersion=="v3" ? "1" : "0";
                case "Snmp_AgentPort":
                    return Prop.Snmp_AgentPort.ToString();
                case "Snmp_AgentListenTrapPort":
                    return Prop.Snmp_AgentListenTrapPort.ToString();
                case "Snmp_V2AgentGetComm":
                    return Prop.Snmp_V2AgentGetComm;
                case "Snmp_V2AgentSetComm":
                    return Prop.Snmp_V2AgentSetComm;
                case "Snmp_V3Users":
                    return Prop.Snmp_V3Users;
            }
            return "";
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="key"></param>
        /// <param name="val"></param>
        protected void PropertySet(string key, object val)
        {
            U5kManServer.Properties.u5kManServer Prop = U5kManServer.Properties.u5kManServer.Default;
            switch (key)
            {
                case "Snmp_AgentVersion":
                    Prop.Snmp_AgentVersion = val.ToString() == "0" ? "v2" : "v3";
                    break;
                case "Snmp_AgentPort":
                    Prop.Snmp_AgentPort = Int32.Parse(val as String);
                    break;
                case "Snmp_AgentListenTrapPort":
                    Prop.Snmp_AgentListenTrapPort = Int32.Parse(val as String);
                    break;
                case "Snmp_V2AgentGetComm":
                    Prop.Snmp_V2AgentGetComm = val as String;
                    break;
                case "Snmp_V2AgentSetComm":
                    Prop.Snmp_V2AgentSetComm = val as String;
                    break;
                case "Snmp_V3Users":
                    Prop.Snmp_V3Users.Clear();
                    List<string> users = (val as JArray).ToObject<List<string>>();
                    Prop.Snmp_V3Users.AddRange(users.ToArray());
                    break;
            }
        }
    }

    /// <summary>
    /// Configuracion de Opciones.
    /// </summary>
    class U5kManWADOptions : U5kManWebAppData
    {
        /// <summary>
        /// 
        /// </summary>
        public class itemOptions
        {
            public string id { get; set; }
            public string val { get; set; }
            public int tp { get; set; }
            public List<string> opt { get; set; }
            public string key { get; set; }
        }
        /// <summary>
        /// 
        /// </summary>
        public string version { get; set; }
        public string bdt { get; set; }
        public List<itemOptions> lconf = new List<itemOptions>();

        /// <summary>
        /// 
        /// </summary>
        class itemProperty
        {
            public string id { get; set; }
            public int tp { get; set; }
            public List<string> opt { get; set; }
        }
        /// <summary>
        /// 
        /// </summary>
        Dictionary<string, itemProperty> prop = new Dictionary<string, itemProperty>()
        {
            {
                "Idioma",
                new itemProperty()
                {                        
                    id=idiomas.strings.Idioma/*"Idioma"*/,                     
                    tp=1,                     
                    opt=new List<string>()                    
                    {                    
                        "es",                        
                        "fr",                        
                        "en"                         
                    }                    
                }
            },
            {
                "ServidorDual",
                new itemProperty()                
                {
                    id=idiomas.strings.ServidorDual /*"Servidor Dual"*/, 
                    tp=1, 
                    opt=new List<string>()
                    {
                        "False",
                        "True"
                    }
                }
            },
            {
                "HayReloj",
                new itemProperty()
                {
                    id=idiomas.strings.WAP_MSG_008 /*"Patron de Reloj"*/, 
                    tp=1, 
                    opt=new List<string>()
                    {
                        "False",
                        "True"
                    }
                }
            },
            //{
            //    "HayPabx",
            //    new itemProperty()
            //    {
            //        id=idiomas.strings.WAP_MSG_009 /*"PABX Interna"*/, 
            //        tp=1, 
            //        opt=new List<string>()
            //        {
            //            "False",
            //            "True"
            //        }
            //    }
            //},
            //{
            //    "GwsUnificadas",
            //    new itemProperty()
            //    {
            //        id=idiomas.strings.WAP_MSG_016 /*"PABX Interna"*/, 
            //        tp=1, 
            //        opt=new List<string>()
            //        {
            //            idiomas.strings.WAP_OPT_007,
            //            idiomas.strings.WAP_OPT_008
            //        }
            //    }
            //},
            {
                "HaySacta",
                new itemProperty()
                {
                    id="SACTA", 
                    tp=1, 
                    opt=new List<string>()
                    {
                        "False",
                        "True"
                    }
                }
            },
            {
                "HayAltavozHF",
                new itemProperty()
                {
                    id=idiomas.strings.WAP_MSG_015,      // "AltavozHF", 
                    tp=1, 
                    opt=new List<string>()
                    {
                        "False",
                        "True"
                    }
                }
            },
            {
                "OpcOpCableGrabacion",
                new itemProperty()
                {
                    id="Cables de Grabacion" /*idiomas.strings.WAP_MSG_015*/,      // "AltavozHF", TODO
                    tp=1,
                    opt=new List<string>()
                    {
                        "False",
                        "True"
                    }
                }
            },
            {
                "SonidoAlarmas",
                new itemProperty()
                {
                    id=idiomas.strings.WAP_MSG_010 /*"Sonido"*/, 
                    tp=1, 
                    opt=new List<string>()
                    {
                        "False",
                        "True"
                    }
                }
            },
            {
                "GenerarHistorico",
                new itemProperty()
                {
                    id=idiomas.strings.WAP_MSG_011 /*"Generar Historico"*/, 
                    tp=1, 
                    opt=new List<string>()
                    {
                        "False",
                        "True" 
                    }
                }
            },
            {
                "PttAndSqhOnBdt",
                new itemProperty()
                {
                    id= idiomas.strings.EVENTOSPTT, // "Almacenar Eventos PTT / SQH",
                    tp=1, 
                    opt=new List<string>()
                    {
                        "False",
                        "True" 
                    }
                }
            },
            {
                "DiasEnHistorico",
                new itemProperty()
                {
                    id=idiomas.strings.WAP_MSG_012 /*"Dias en Historico"*/, 
                    tp=1, 
                    opt=new List<string>()
                    {
                        idiomas.strings.WAP_OPT_001 /*"1 Semana"*/,
                        idiomas.strings.WAP_OPT_002 /*"2 Semanas"*/, 
                        idiomas.strings.WAP_OPT_003 /*"1 Mes"*/, 
                        idiomas.strings.WAP_OPT_004 /*"3 Meses"*/, 
                        idiomas.strings.WAP_OPT_005 /*"6 Meses"*/, 
                        idiomas.strings.WAP_OPT_006 /*"1 Año"*/ 
                    }
                }
            },
            {
                "LineasIncidencias",
                new itemProperty()
                {
                    id=idiomas.strings.WAP_MSG_013 /*"Incidencias sin Reconocer"*/, 
                    tp=1, 
                    opt=new List<string>()
                    {
                        "8", 
                        "16", 
                        "32", 
                        "64" 
                    }
                }
            },
        };

        /// <summary>
        /// 
        /// </summary>
        public U5kManWADOptions(U5kManStdData gdata, bool bGenerate = false)
        {
            if (bGenerate)
            {
#if STD_ACCESS_V0
                lock (U5kManService._std._gen)
                {
                    version = U5kGenericos.Version;
                    bdt = U5kManService._std._gen.CfgId;        // .cfgVersion;
                }
#else
                version = U5kGenericos.Version;
                bdt = gdata.STDG.CfgId;        // .cfgVersion;
#endif
                foreach (KeyValuePair<string, itemProperty> p in prop)
                {
                    lconf.Add(new itemOptions()
                    {
                        key = p.Key,
                        id = p.Value.id,
                        tp = p.Value.tp,
                        opt = p.Value.opt,
                        val = PropertyGet(p.Key)    // U5kManServer.Properties.u5kManServer.Default.
                    });
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public void Save()
        {
            foreach (itemOptions opt in lconf)
            {
                PropertySet(opt.key, opt.val);
            }
            //U5kManServer.Properties.u5kManServer.Default.Save();
            U5kManService.cfgSettings.Save();
            // Hay que Reiniciar el Servicio Temporizadamente...
            U5kGenericos.ResetService = true;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        protected string PropertyGet(string key)
        {
            // U5kManServer.Properties.u5kManServer Prop = U5kManServer.Properties.u5kManServer.Default;
            var Prop = U5kManService.cfgSettings;
            switch (key)
            {
                case "Idioma":
                    return Prop.Idioma == "es" ? "0" : Prop.Idioma == "fr" ? "1" : "2";

                case "ServidorDual":
                    return Prop.ServidorDual ? "1" : "0";

                case "HayReloj":
                    return Prop.HayReloj ? "1" : "0";

                //case "HayPabx":
                //    return Prop.HayPabx ? "1" : "0";

                case "HaySacta":
                    return Prop.HaySacta ? "1" : "0";

                case "HayAltavozHF":
                    return Prop.HayAltavozHF ? "1" : "0";

                case "OpcOpCableGrabacion":
                    return Prop.OpcOpeCableGrabacion ? "1" : "0";

                case "SonidoAlarmas":
                    return Prop.SonidoAlarmas ? "1" : "0";

                case "GenerarHistorico":
                    return Prop.GenerarHistoricos ? "1" : "0";

                case "DiasEnHistorico":
                    return Prop.DiasEnHistorico <= 7 ? "0" :
                        Prop.DiasEnHistorico <= 14 ? "1" :
                        Prop.DiasEnHistorico <= 30 ? "2" :
                        Prop.DiasEnHistorico <= 92 ? "3" :
                        Prop.DiasEnHistorico <= 184 ? "4" : "5";

                case "LineasIncidencias":
                    return Prop.LineasIncidencias <= 8 ? "0" :
                        Prop.LineasIncidencias <= 16 ? "1" :
                        Prop.LineasIncidencias <= 32 ? "2" : "3";

                //case "GwsUnificadas":
                //    return Prop.GwsUnificadas == false ? "0" : "1";

                case "PttAndSqhOnBdt":
                    return Prop.Historico_PttSqhOnBdt == false ? "0" : "1";

                default:
                    return "0";
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="key"></param>
        /// <param name="val"></param>
        protected void PropertySet(string key, string val)
        {
            //U5kManServer.Properties.u5kManServer Prop = U5kManServer.Properties.u5kManServer.Default;
            var Prop = U5kManService.cfgSettings;
            switch (key)
            {
                case "Idioma":
                    Prop.Idioma = val == "0" ? "es" : val == "1" ? "fr" : "en";
                    break;

                case "ServidorDual":
                    Prop.ServidorDual = (val == "1");
                    break;

                case "HayReloj":
                    Prop.HayReloj = (val == "1");
                    break;

                //case "HayPabx":
                //    Prop.HayPabx = (val == "1");
                //    break;

                case "HaySacta":
                    Prop.HaySacta = (val == "1");
                    break;

                case "HayAltavozHF":
                    Prop.HayAltavozHF = (val == "1");
                    break;

                case "OpcOpCableGrabacion":
                    Prop.OpcOpeCableGrabacion = (val == "1");
                    break;

                case "SonidoAlarmas":
                    Prop.SonidoAlarmas = (val == "1");
                    break;

                case "GenerarHistorico":
                    Prop.GenerarHistoricos = (val == "1");
                    break;

                case "DiasEnHistorico":
                    Prop.DiasEnHistorico = val == "0" ? 7 :
                        val == "1" ? 14 :
                        val == "2" ? 30 :
                        val == "3" ? 92 :
                        val == "4" ? 184 : 365;
                    break;

                case "LineasIncidencias":
                    Prop.LineasIncidencias = val == "0" ? 8 :
                        val == "1" ? 16 :
                        val == "2" ? 32 : 64;
                    break;

                case "PttAndSqhOnBdt":
                    Prop.Historico_PttSqhOnBdt = val == "1";
                    break;

            }
        }
    }

    /// <summary>
    /// 
    /// </summary>
    class UG5kVersion : U5kManWebAppData
    {
        public class Linea
        {
            public string res { get; set; }
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();

            sb.Append("Version  : " + version + "\n");
            sb.Append("Config   : " + cfgsver + "\n");
            sb.Append("Snmp     : " + snmpver + "\n");
            sb.Append("Grabador : " + recsver + "\n");
            sb.Append("VOIP: " + "\n");
            foreach (Linea line in lines)
            {
                sb.Append("    " + line.res + "\n");
            }

            return sb.ToString();
        }

        /// <summary>
        /// 
        /// </summary>
        public string version { get; set; }
        public string cfgsver { get; set; }
        public string snmpver { get; set; }
        public string recsver { get; set; }
        public List<Linea> lines { get; set; }
    }

    /// <summary>
    /// 
    /// </summary>
    class UG5KExtVersion : U5kManWebAppData
    {
        public class Component
        {
            public class FileDesc
            {
                public string Path { get; set; }
                public string Date { get; set; }
                public string Size { get; set; }
                public int Modo { get; set; }
            }

            public string Name { get; set; }
            public string Id { get; set; }
            public List<FileDesc> Files { get; set; }
        }

        public string Version { get; set; }
        public string Fecha { get; set; }
        public List<Component> Components { get; set; }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            StringWriter strWriter = new StringWriter();

            strWriter.WriteLine("Version: {0}. Fecha: {1}", Version, Fecha);
            foreach (Component comp in Components)
            {
                strWriter.WriteLine();
                strWriter.WriteLine("{0,8}", comp.Id);
                foreach (Component.FileDesc file in comp.Files)
                {
                    strWriter.WriteLine("{0,30}: ({1,10}-{2,8})", Path.GetFileName(file.Path), file.Date, file.Size);
                }
            }
            return strWriter.ToString();
        }
    }
}
