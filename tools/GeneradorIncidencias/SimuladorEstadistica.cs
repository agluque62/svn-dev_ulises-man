using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using U5kBaseDatos;
using U5kManServer;

namespace GeneradorIncidencias
{
    class SimuladorEstadistica
    {
        public SimuladorEstadistica(Uv5kDbAccess _database)
        {
            Database = _database;
            Tarea = null;
        }

        /// <summary>
        /// 
        /// </summary>
        public void Start()
        {
            if (Tarea == null)
            {
                Cancel = new CancellationTokenSource();
                Tarea = new Task(() => TareaSimuladorEstadistica(Cancel.Token), Cancel.Token);
                Tarea.Start();
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public void Stop()
        {
            if (Tarea != null)
            {
                Cancel.Cancel();
                Tarea.Wait();
                Tarea = null;
            }
        }
        /// <summary>
        /// 
        /// </summary>
        public bool IsRunning
        {
            get
            {
                return Tarea == null ? false : true;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        private Uv5kDbAccess Database { get; set; }
        private Task Tarea { get; set; }
        private CancellationTokenSource Cancel { get; set; }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="ct"></param>
        private void TareaSimuladorEstadistica(CancellationToken ct)
        {
            Tops = Database.Database.GetListaTop("departamento");
            Gws = Database.Database.GetListaGw("departamento");


            U5kEstadisticaProc.Estadisticas.Start();
            ActivateTops();
            ActivateGws();

            while (ct.IsCancellationRequested == false)
            {
                Tick(5, ct);
                switch (rnd_number.Next(0, 2))
                {
                    case 0:
                        SimulateEventTop();
                        break;
                    case 1:
                        SimulateEventGw();
                        break;
                }
                Console.Write(".");
            }

            U5kEstadisticaProc.Estadisticas.Stop();
        }
        /// <summary>
        /// 
        /// </summary>
        private List<U5kBaseDatos.BdtGw> Gws { get; set; }
        private void ActivateGws()
        {
            foreach (BdtGw gw in Gws)
            {
                U5kEstadisticaProc.Estadisticas.AddPasarela(gw.Id);
                U5kEstadisticaProc.Estadisticas.EventoPasarela(gw.Id, true);
                gw.Ip = "activo";
            }
        }
        /// <summary>
        /// 
        /// </summary>
        private List<U5kBaseDatos.BdtTop> Tops { get; set; }
        private void ActivateTops()
        {
            foreach (BdtTop top in Tops)
            {
                U5kEstadisticaProc.Estadisticas.AddOperador(top.Id);
                U5kEstadisticaProc.Estadisticas.EventoOperador(top.Id, true);
                top.Ip = "activo";
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="nsec"></param>
        private void Tick(int nsec, CancellationToken ct)
        {
            for (int sec = 0; sec < nsec; sec++)
            {
                if (ct.IsCancellationRequested == false)
                    Thread.Sleep(1000);
            }
        }
        private Random rnd_number = new Random();
        private void SimulateEventTop()
        {
            int ntop = rnd_number.Next(0, Tops.Count);
            switch (Tops[ntop].Ip)
            {
                case "activo":
                    U5kEstadisticaProc.Estadisticas.EventoOperador(Tops[ntop].Id, false);
                    Tops[ntop].Ip = "inactivo";
                    break;
                case "inactivo":
                    U5kEstadisticaProc.Estadisticas.EventoOperador(Tops[ntop].Id, true);
                    Tops[ntop].Ip = "activo";
                    break;
            }
        }
        private void SimulateEventGw()
        {
            int ngw = rnd_number.Next(0, Gws.Count);
            switch (Gws[ngw].Ip)
            {
                case "activo":
                    U5kEstadisticaProc.Estadisticas.EventoPasarela(Gws[ngw].Id, false);
                    Gws[ngw].Ip = "inactivo";
                    break;
                case "inactivo":
                    U5kEstadisticaProc.Estadisticas.EventoPasarela(Gws[ngw].Id, true);
                    Gws[ngw].Ip = "activo";
                    break;
            }
        }
    }
}
