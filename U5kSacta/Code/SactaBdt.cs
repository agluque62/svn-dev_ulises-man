using System;
using System.Data;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using U5kBaseDatos;
using CD40.BD;

namespace U5kSacta
{
    class SactaBdt
    {
        static public void Init(Func<object> dbService)
        {
            DbService = dbService;
        }
        static public void Start()
        {
            if (U5kBdt != null)
            {
                var qrySectors = $"SELECT NumSacta FROM sectores WHERE IdSistema='departamento' AND sectorsimple=1 AND (tipo='R' OR tipo='V')";
                var qryTops = $"SELECT PosicionSacta FROM top WHERE IdSistema='departamento'";
                idSectores = U5kBdt.ReadTableOrView(qrySectors)?.Tables[0]?.AsEnumerable()
                    .Select(s => s.Field<UInt16>("NumSacta")).ToList();
                idUcs = U5kBdt.ReadTableOrView(qryTops)?.Tables[0]?.AsEnumerable()
                    .Select(s => s.Field<UInt16>("PosicionSacta")).ToList();
                SactaConfig.GetConfig((cfg, error) =>
                {
                    idSectoresIgnorados = cfg.scv.Ignore.Split(new char[] { ',' }).ToList()
                    .Select(item =>
                    {
                        UInt16 ui = 0;
                        return (UInt16.TryParse(item, out ui) ? ui : default);
                    }).ToList();
                });
            }
        }

        static public void Stop()
        {
            idSectores.Clear();
            idUcs.Clear();
            idSectoresIgnorados.Clear();
        }

        static public string MttoSectors()
        {
            // TODO
            return default;
        }
        static public void GeneraSectorizacionSacta(uint version, string dataSect, Action<bool, DateTime, SectorizationResult> Result)
        {
            var ConexionCD40 = new Object();
            var info = new SactaInfo();
            var fechaActivacion = DateTime.Now;
            var util = new Utilidades(ConexionCD40);

            util.EventResultSectorizacion += new CD40.BD.SectorizacionEventHandler<CD40.BD.SactaInfo>((resinfo) =>
            {
                Result(true, fechaActivacion, new SectorizationResult()
                {
                    SectName = "SACTA",
                    SectData = dataSect,
                    Version = (uint)info["SectVersion"],
                    Resultado = (int)info["Resultado"],
                    ErrorCause = info.ContainsKey("ErrorCause") ? (string)info["ErrorCause"] : null
                });
                // TODO. Meter en el generador de Historicos propio del Servicio.
                util.CreaEventoConfiguracion("departamento",
                    (uint)((int)info["Resultado"] == 0 ? 109 : 110),
                    new string[] { info.ContainsKey("ErrorCause") ? (string)info["ErrorCause"] : null }, "127.0.0.1");
            });

            info["Version"] = version;
            info["SectName"] = "SACTA";
            info["SectData"] = dataSect;
            try
            {
                var sectorizacion = util.GeneraSectorizacion(info, fechaActivacion);
            }
            catch (Exception x)
            {
                Result(false, fechaActivacion, new SectorizationResult()
                {
                    SectName = "SACTA",
                    SectData = dataSect,
                    Version = version,
                    Resultado = 1,
                    ErrorCause = $"Exception {x.Message}"
                });

            }

            //new CD40.BD.Utilidades(ConexionCD40).CreaEventoConfiguracion("departamento", (uint)(result == 0 ? 109 : 110), new string[] { cause }, "127.0.0.1");
        }
        static public bool UcsInBdt(UInt16 ucs) { return idUcs.Contains(ucs); }
        static public bool SectInBdt(UInt16 sec) { return idSectores.Contains(sec); }
        static public bool HayQueIgnorar(UInt16 sec) { return idSectoresIgnorados.Contains(sec); }
        static public string IdSectores
        {
            get
            {
                string strSectores = "";
                foreach (Int32 sec in idSectores)
                    strSectores += (sec.ToString() + " ");
                return strSectores;
            }
        }
        static public string IdUcs
        {
            get
            {
                string strUcs = "";
                foreach (Int32 ucs in idUcs)
                    strUcs += (ucs.ToString() + " ");
                return strUcs;
            }
        }


        static List<UInt16> idSectores = new List<UInt16>();
        static List<UInt16> idUcs = new List<UInt16>();
        static List<UInt16> idSectoresIgnorados = new List<ushort>();
        static Func<object> DbService = null;
        static U5kBdtService U5kBdt => (U5kBdtService)DbService?.Invoke();
    }
}
