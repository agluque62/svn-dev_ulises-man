using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

using Uv5kiTaHfManager.Modelos;
using Uv5kiTaHfManager.Comandos;
using Uv5kiTaHfManager.Helpers;

namespace Uv5kiTaHfManager.ModelosDeVistas
{
    class MainViewModel : ViewModelBase, IDisposable
    {
        #region Publicos
        
        public ObservableCollection<EquipoRadio> Equipos { get; set; }
        public ObservableCollection<Frecuencia> Frecuencias { get; set; }

        public ICommand ProximoEquipo { get; set; }
        public ICommand ProximaFrecuencia { get; set; }
        public ICommand Salir { get; set; }
        public ICommand Sintonizar { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public string Mensaje
        {
            get { return _mensaje; }
            set
            {
                if (_mensaje != value)
                {
                    _mensaje = value;
                    OnPropertyChanged("Mensaje");
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public EquipoRadio EquipoSeleccionado
        {
            get { return _EquipoSeleccionado; }
            set
            {
                if (_EquipoSeleccionado != value)
                {
                    _EquipoSeleccionado = value;
                    
                    Frecuencia fsel = GetFrecuenciaEquipo(value);
                    if (fsel != null)
                        FrecuenciaSeleccionada = fsel;

                    OnEquStatusChanged(value);
                    OnPropertyChanged("EquipoSeleccionado");
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public System.Windows.Media.Brush ForegroundColorEquipo
        {
            get { return _ForegroundColorEquipo; }
            set
            {
                if (_ForegroundColorEquipo != value)
                {
                    _ForegroundColorEquipo = value;
                    OnPropertyChanged("ForegroundColorEquipo");
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public Frecuencia FrecuenciaSeleccionada
        {
            get { return _FrecuenciaSeleccionada; }
            set
            {
                if (_FrecuenciaSeleccionada != value)
                {
                    _FrecuenciaSeleccionada = value;
                    IndiceFrecuencias = GetIndiceFrecuencia(value);

                    OnPropertyChanged("FrecuenciaSeleccionada");
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public MainViewModel()
        {
            ProximoEquipo = new DelegateCommandBase(new Action<object>(OnProximoEquipo));
            ProximaFrecuencia = new DelegateCommandBase(new Action<object>(OnProximaFrecuencia));
            Salir = new DelegateCommandBase(new Action<object>(OnExit));
            Sintonizar = new DelegateCommandBase(new Action<object>(OnSintonizar));
            LoadFrecuencias();  // Primero Cargar las Frecuencias....
            LoadEquipos();      // ... y luego los equipos.

            DelayedMensaje = "";
        }

        /// <summary>
        /// 
        /// </summary>
        public void Dispose()
        {
            foreach (EquipoRadio equipo in Equipos)
                equipo.Dispose();
        }

        #endregion

        #region Privados

        /// <summary>
        /// 
        /// </summary>
        void LoadEquipos()
        {
            Equipos = new ObservableCollection<EquipoRadio>();
            foreach (string strEqu in Properties.Settings.Default.RadioEqSet)
            {
                //Equipos.Add(new EquipoRadio(OnEquStatusChanged)
                //{
                //    Id = strEqu,
                //    Estado = EstadoEquipo.Desconectado,
                //    FrecuenciaSeleccionada = LocalConfig.GetLastFrecuencyOnEqu(strEqu)
                //});
                Equipos.Add(new EquipoRadioHfInvelco(OnEquStatusChanged)
                {
                    Id = LocalConfig.IdOfEquipment(strEqu),
                    Estado = EstadoEquipo.Desconectado,
                    FrecuenciaSeleccionada = LocalConfig.GetLastFrecuencyOnEqu(strEqu),
                    OidBase=LocalConfig.OidBaseOfEquipment(strEqu)
                });
            }
            IndiceEquipos = Equipos.Count > 0 ? 0 : -1;
            EquipoSeleccionado = IndiceEquipos >= 0 ? Equipos[IndiceEquipos] : null;
        }

        /// <summary>
        /// 
        /// </summary>
        void LoadFrecuencias()
        {
#if DEBUG01
            Frecuencias = new ObservableCollection<Frecuencia>()
            {
                new Frecuencia(){Id="5.210", MegaHerzios=5.21},
                new Frecuencia(){Id="10.230", MegaHerzios=10.23},
                new Frecuencia(){Id="22.560", MegaHerzios=22.56}
            };
            IndiceFrecuencias = 0;
            FrecuenciaSeleccionada = Frecuencias[IndiceFrecuencias];
#else
            Frecuencias = new ObservableCollection<Frecuencia>();
            foreach (string strFrec in Properties.Settings.Default.FrecuencySet)
            {
                Frecuencias.Add(new Frecuencia()
                {
                    Id = LocalConfig.IdOfFrecuency(strFrec),
                    MegaHerzios = LocalConfig.FrecOfFrecuency(strFrec)
                });
            }
            IndiceFrecuencias = Frecuencias.Count > 0 ? 0 : -1;
            FrecuenciaSeleccionada = IndiceFrecuencias >= 0 ? Frecuencias[IndiceFrecuencias] : null;
#endif
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="parameter"></param>
        void OnProximoEquipo(object parameter)
        {
            IndiceEquipos++;
            if (IndiceEquipos >= Equipos.Count)
                IndiceEquipos = 0;
            if (IndiceEquipos < Equipos.Count)
                EquipoSeleccionado = Equipos[IndiceEquipos];
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="parameter"></param>
        void OnProximaFrecuencia(object parameter)
        {
            IndiceFrecuencias++;
            if (IndiceFrecuencias >= Frecuencias.Count)
                IndiceFrecuencias = 0;
            if (IndiceFrecuencias < Frecuencias.Count)
            {
                FrecuenciaSeleccionada = Frecuencias[IndiceFrecuencias];
                OnEquStatusChanged(_EquipoSeleccionado);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="parameter"></param>
        void OnSintonizar(object parameter)
        {
            if (EquipoSeleccionado != null && FrecuenciaSeleccionada != null)
            {
                if (FrecuenciaSeleccionada.MegaHerzios != EquipoSeleccionado.FrecuenciaSeleccionada)
                {
                    if (EquipoSeleccionado.Estado != EstadoEquipo.Desconectado)
                        EquipoSeleccionado.Sintoniza(FrecuenciaSeleccionada.MegaHerzios, OnEquSintonizado, OnEquErrorAlSintonizar);
                    else
                        DelayedMensaje = Properties.Resources.Equipo_no_activo;
                }
                else
                    DelayedMensaje = Properties.Resources.Nada_que_sintonizar;
            }
            else
                DelayedMensaje = Properties.Resources.No_hay_equipos_o_frecuencias_a_sintonizar;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="parameter"></param>
        void OnExit(object parameter)
        {
            Dispose();
            System.Windows.Application.Current.Shutdown();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="equ"></param>
        void OnEquStatusChanged(object equ)
        {
            EquipoRadio equipo = (EquipoRadio)equ;
            if (EquipoSeleccionado == equipo)
            {
                switch (equipo.Estado)
                {
                    case EstadoEquipo.Desconectado:
                        ForegroundColorEquipo = System.Windows.Media.Brushes.Gray;
                        break;
                    case EstadoEquipo.Conectado:
                        if (equipo.FrecuenciaSeleccionada == FrecuenciaSeleccionada.MegaHerzios)
                        {
                            ForegroundColorEquipo = System.Windows.Media.Brushes.Blue;
                        }
                        else
                        {
                            ForegroundColorEquipo = System.Windows.Media.Brushes.Orange;
                        }
                        break;
                    case EstadoEquipo.Error:
                        ForegroundColorEquipo = System.Windows.Media.Brushes.Red;
                        break;
                }
                OnPropertyChanged("EquipoSeleccionado");
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="equ"></param>
        void OnEquSintonizado(object equ)
        {
            LocalConfig.SetLastFrecuencyOnEqu(((EquipoRadio)equ).Id, ((EquipoRadio)equ).FrecuenciaSeleccionada);
            DelayedMensaje = ((EquipoRadio)equ).Id + ". " + Properties.Resources.Equipo_Sintonizado;
            OnEquStatusChanged((EquipoRadio)equ);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="equ"></param>
        void OnEquErrorAlSintonizar(object msg)
        {
            DelayedMensaje = (string)msg;
        }

        /// <summary>
        /// 
        /// </summary>
        private string DelayedMensaje
        {
            set
            {
                (new Task(() => 
                { 
                    Mensaje = value;
                    System.Threading.Thread.Sleep(2000);
                    Mensaje = "";
                })).Start();
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="equipo"></param>
        /// <returns></returns>
        private Frecuencia GetFrecuenciaEquipo(EquipoRadio equipo)
        {
            foreach (Frecuencia fr in Frecuencias)
            {
                if (equipo.FrecuenciaSeleccionada == fr.MegaHerzios)
                    return fr;
            }
            return null;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="frec"></param>
        /// <returns></returns>
        private Int32 GetIndiceFrecuencia(Frecuencia frec)
        {
            Int32 index = 0;
            foreach (Frecuencia ifr in Frecuencias)
            {
                if (ifr == frec)
                    return index;
                index++;
            }
            return -1;
        }

        /// <summary>
        /// 
        /// </summary>
        private EquipoRadio _EquipoSeleccionado;
        private Int32 IndiceEquipos = 0;
        private Frecuencia _FrecuenciaSeleccionada;
        private Int32 IndiceFrecuencias = 0;
        private System.Windows.Media.Brush _ForegroundColorEquipo;
        private string _mensaje;

        #endregion

    }
}
