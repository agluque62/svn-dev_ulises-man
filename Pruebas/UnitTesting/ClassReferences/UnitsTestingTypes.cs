using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UnitTesting
{
    class UnitsTestingTypes
    {
    }
}
namespace U5kManServer1
{
    public enum std
    {
        NoInfo = 0,
        Ok = 1,
        AvisoReconocido = 2,
        AlarmaReconocida = 3,
        Aviso = 4,
        Alarma = 5,
        Error = 6,
        Principal = 7,
        Reserva = 8,
        NoExiste = 9,
        Inicio = -1
    }
    public class U5kManService
    {
        public class radioSessionData
        {
            public int fstd { get; set; }
        }
    }
}