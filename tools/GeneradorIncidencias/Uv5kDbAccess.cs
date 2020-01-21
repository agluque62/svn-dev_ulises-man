using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using U5kBaseDatos;
using U5kManServer;

namespace U5kManServer
{
    /// <summary>
    /// 
    /// </summary>
    public enum eTiposInci { TEH_TOP = 0, TEH_TIFX = 1, TEH_EXTERNO_RADIO, TEH_EXTERNO_TELEFONIA, TEH_SISTEMA, TEH_RADIOHF, TEH_RADIOMN }
    /// <summary>
    /// 
    /// </summary>
    /// <param name="scv"></param>
    /// <param name="inci"></param>
    /// <param name="thw"></param>
    /// <param name="idhw"></param>
    /// <param name="parametros"></param>
    public delegate void GenerarHistorico(int scv, eIncidencias inci, eTiposInci thw, string idhw, params object[] parametros);
    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    public delegate Object BdtServiceAcces();
}

namespace GeneradorIncidencias
{    
    class Uv5kDbAccess
    {
        /// <summary>
        /// 
        /// </summary>
        public U5kBdtService Database
        {
            get
            {
                return _bdt;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public void Connect()
        {
            if (_bdt==null) 
            {
                _bdt = new U5kBdtService(Thread.CurrentThread.CurrentUICulture, 
                    eBdt.bdtSqlite, "d:\\datos\\Empresa\\_SharedPrj\\UlisesV5000i-MN\\ulises-man\\uv5ki-01.db3");
                U5kEstadisticaProc.Estadisticas = new U5kEstadisticaProc(GeneraIncidencia, delegate() { return _bdt; });
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public void Disconnect()
        {
            _bdt = null;
        }

        /// <summary>
        /// 
        /// </summary>
        Random rnd = new Random();
        List<Tuple<int, int, eTiposInci>> grpInci = new List<Tuple<int, int, eTiposInci>>()
        {
            new Tuple<int, int, eTiposInci>(1,50,eTiposInci.TEH_RADIOHF),
            new Tuple<int, int, eTiposInci>(51,1000,eTiposInci.TEH_SISTEMA),
            new Tuple<int, int, eTiposInci>(1001,2000,eTiposInci.TEH_TOP),
            new Tuple<int, int, eTiposInci>(2001,3000,eTiposInci.TEH_TIFX),
            new Tuple<int, int, eTiposInci>(3001,3049,eTiposInci.TEH_EXTERNO_RADIO),
            new Tuple<int, int, eTiposInci>(3005,3200,eTiposInci.TEH_RADIOMN)
        };
        public void RdnGenIncidencia(int grpIndex = 0, bool alarmas = false)
        {
            Tuple<int, int, eTiposInci> grp = grpInci[grpIndex];
            List<U5kIncidenciaDescr> lista = _bdt.ListaDeIncidencias(null).Where(i => (i.id >= grp.Item1 && i.id < grp.Item2) && (alarmas == false || (alarmas == true && i.alarm == true))).ToList();
            int iIndex = rnd.Next(0, lista.Count);
            object[] parametros = {
                "TEST " + grp.Item3.ToString() + " PAR-01",
                "TEST " + grp.Item3.ToString() + " PAR-02",
                "TEST " + grp.Item3.ToString() + " PAR-03",
                "TEST " + grp.Item3.ToString() + " PAR-04"
            };
            GeneraIncidencia(0, (U5kBaseDatos.eIncidencias)lista[iIndex].id, grp.Item3, "TEST", parametros);
        }
        /// <summary>
        /// 
        /// </summary>
        private static U5kBdtService _bdt=null;
        private Random rnd_inci = new Random();
        private Object _locker = new Object();

        /// <summary>
        /// 
        /// </summary>
        /// <param name="scv"></param>
        /// <param name="inci"></param>
        /// <param name="thw"></param>
        /// <param name="idhw"></param>
        /// <param name="parametros"></param>
        private void GeneraIncidencia(int _scv, eIncidencias inci, eTiposInci thw, string idHw, params object[] parametros)
        {
            lock (_locker)
            {
                U5kIncidencia st_inci = new U5kIncidencia()
                {
                    fecha = DateTime.Now,
                    id = (int)inci,
                    tipo = (int )thw,
                    idhw = idHw,
                    sistema = "departamento",
                    desc = parametros.Count()>0 ? (string)parametros[0] : "",
                    scv = _scv,
                    reconocida = DateTime.MinValue
                };

                if (_bdt != null)
                    _bdt.InsertaIncidencia(false, st_inci);
            }
        }
    }
}
