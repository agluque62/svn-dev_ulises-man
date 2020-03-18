using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using CD40.BD;

namespace U5kSacta
{
    class SactaBdt
    {
        static public void Init(/*string idSistema, MySql.Data.MySqlClient.MySqlConnection bdtconn*/)
        {
            // TODO.
            //try
            //{
            //    bdtconn.Open();
            //    string sec_qry = String.Format("SELECT NumSacta FROM sectores WHERE IdSistema='{0}' AND sectorsimple=1 AND (tipo='R' OR tipo='V')", idSistema);
            //    using (MySql.Data.MySqlClient.MySqlCommand command = new MySql.Data.MySqlClient.MySqlCommand(sec_qry, bdtconn))
            //    {
            //        using (MySql.Data.MySqlClient.MySqlDataReader reader = command.ExecuteReader())
            //        {
            //            while (reader.Read())
            //            {
            //                idSectores.Add(reader.GetUInt16(0));
            //            }
            //        }
            //    }
            //    /** 20180716. Proteccion contra STRING vacio */
            //    if (SactaSectionHandler.CfgSacta.CfgSactaUsuarioSectores.IdSectores != string.Empty)
            //    {
            //        string[] otrossectores = SactaSectionHandler.CfgSacta.CfgSactaUsuarioSectores.IdSectores.Split(new char[] { ',' }).ToArray();
            //        foreach (string sect in otrossectores)
            //        {
            //            /** 20180716. Proteccion errores de formato */
            //            try
            //            {
            //                /** 20180731 Considero todo el filtro. */
            //                //if (!idSectores.Contains(UInt16.Parse(sect)))
            //                idSectoresIgnorados.Add(UInt16.Parse(sect));
            //            }
            //            finally { }
            //        }
            //    }
            //    /**************************/

            //    string top_qry = String.Format("SELECT PosicionSacta FROM top WHERE IdSistema='{0}'", idSistema);
            //    using (MySql.Data.MySqlClient.MySqlCommand command = new MySql.Data.MySqlClient.MySqlCommand(top_qry, bdtconn))
            //    {
            //        using (MySql.Data.MySqlClient.MySqlDataReader reader = command.ExecuteReader())
            //        {
            //            while (reader.Read())
            //            {
            //                idUcs.Add(reader.GetUInt16(0));
            //            }
            //        }
            //    }
            //}
            //catch (Exception x)
            //{
            //    throw x;
            //}
            //finally
            //{
            //    bdtconn.Close();
            //}
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
    }
}
