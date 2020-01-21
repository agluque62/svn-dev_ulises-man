using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace GeneradorIncidencias
{
    class SimuladorHistoricos
    {
        public SimuladorHistoricos(Uv5kDbAccess _database)
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
                Tarea = new Task(() => TareaSimuladorHistoricos(Cancel.Token), Cancel.Token);
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
        private void TareaSimuladorHistoricos(CancellationToken ct)
        {
            while (ct.IsCancellationRequested == false)
            {
                Database.RdnGenIncidencia();

                for (int nsec = 0; nsec < 20; nsec++)
                {
                    if (ct.IsCancellationRequested == false)
                        Thread.Sleep(1000);
                }                
                Console.WriteLine("Tarea Simulador Historicos....");
            }
        }

    }
}
