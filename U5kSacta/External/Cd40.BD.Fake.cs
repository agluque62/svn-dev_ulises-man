using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CD40.BD
{
    public delegate void SectorizacionEventHandler<T>(T msg);
    public class SactaInfo
    {
        public object this[string index]
        {
            get
            {
                return Data.ContainsKey(index) ? Data[index] : null;
            }
            set
            {
                Data[index] = value;
            }
        }
        public bool ContainsKey(string key) => Data.ContainsKey(key);

        private Dictionary<string, object> Data = new Dictionary<string, object>();
    }

    public class Utilidades
    {
        public Utilidades(object conexion) { }
        public event SectorizacionEventHandler<SactaInfo> EventResultSectorizacion;
        public object GeneraSectorizacion(object info, DateTime Date)
        {
            return null;
        }

        public void CreaEventoConfiguracion(string id_sistema, uint idIncidencia, string[] parametros, string ipserverMantto = null)
        {

        }

    }

}
