using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Uv5kiTaHfManager.Modelos
{
    enum EstadoEquipo { Desconectado=0, Conectado=1, Error=2 }
    abstract class EquipoRadio : IDisposable
    {
        public string Id { get; set; }

        public EstadoEquipo Estado { get; set; }
        public double FrecuenciaSeleccionada { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public virtual event Action<object> StatusChanged;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="_status_changed"></param>
        public EquipoRadio(Action<object> _status_changed=null)
        {
            StatusChanged = _status_changed;

            GetStatusTask = SintonizaTask = null;
            _timer_status.Interval = 1000 * 5;
            _timer_status.Elapsed += new System.Timers.ElapsedEventHandler(Timer_Status_Elapsed);
            _timer_status.Start();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="mhz"></param>
        /// <param name="_frecuency_change"></param>
        /// <param name="_error"></param>
        public void Sintoniza(double mhz, Action<object> _frecuency_change, Action<object> _error)
        {
            if (SintonizaTask == null)
            {
                SintonizaTask = new Task(() => SintonizaProc(mhz, _frecuency_change, _error));
                SintonizaTask.Start();
            }
            else
                _error(Properties.Resources.Operacion_en_Curso);
        }

        /// <summary>
        /// 
        /// </summary>
        public void Dispose()
        {
            if (GetStatusTask != null)
                GetStatusTask.Wait();
            if (SintonizaTask != null)
                SintonizaTask.Wait();
            _timer_status.Stop();
        }

        /// <summary>
        /// 
        /// </summary>
        private System.Timers.Timer _timer_status = new System.Timers.Timer();
        private void Timer_Status_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            if (GetStatusTask == null)
            {
                GetStatusTask = new Task(() => GetStatusProc());
                GetStatusTask.Start();
            }
        }

        /// <summary>
        /// 
        /// </summary>
        private Task GetStatusTask { get; set; }
#if DEBUG0
        private Random rnd_std = new Random();
#endif
        public virtual void GetStatusProc()
        {
#if DEBUG0
            System.Threading.Thread.Sleep(1500);
            Estado = EstadoEquipo.Conectado;        // (EstadoEquipo)rnd_std.Next(0, 3);
            StatusChanged(this);
#endif
            GetStatusTask = null;
        }

        private Task SintonizaTask { get; set; }
        public virtual void SintonizaProc(double mhz, Action<object> _frecuency_change, Action<object> _error)
        {
            System.Threading.Thread.CurrentThread.CurrentUICulture = new System.Globalization.CultureInfo(Uv5kiTaHfManager.Properties.Settings.Default.Idioma);
#if DEBUG0
            System.Threading.Thread.Sleep(1500);
            if (rnd_std.Next(0, 4) == 0)        // Uno de cada cuatro dará error.
            {
                _error(Id + ". " + Properties.Resources.Error_al_Sintonizar);
            }
            else
            {
                FrecuenciaSeleccionada = mhz;
                _frecuency_change(this);
            }
#else
#endif
            SintonizaTask = null;
        }
    }
}
