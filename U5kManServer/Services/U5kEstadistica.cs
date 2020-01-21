/**-------------------------------------------------------------------------------------------------------
 Calculo de Estadisticas. ULISES V 5000 I.Version 2.5.x
Datos de Entrada:
	Intervalo de Calculo
		Fecha / Hora Inicial + Fecha / Hora Final => Th (Numero de Horas).
	
	Tipo Hardware.
		Operadores o,
		Pasarelas o,
        Equipos Externos o, 
		Todos.
		
	Unidad.
		Todas (Las pasarelas, operadores, equipos ext)          => Nu (Numero de Unidades observadas) o,
		Un Operedor determinado                                 => Nu=1
		Una Pasarela Determinada                                => Nu=1
        Un equipo Externo                                       => Nu=1
		
	Eventos Considerados como 'Fallos'.
		Fijos para todos los cálculos.
		Dependen del 'tipo de harware'.
		Se consideran los Eventos de 'Fallos' y sus correspondientes de 'Recuperacion'.
		
Calculos.
	Intervalo de Consulta (Horas)						Th
	Numero de Unidades.									Nu
	Número de Fallos.									Nf (se obtiene de la consulta en BD)
	Número de Eventos de Activacion						Na (se obtiene de la consulta en BD)	
	Tiempo en Operacion									To (se obtiene en la consulta en BD)

	Resultados:
		Tasa de Fallos por Unidad (cuando Nu > 1) (%)	Nf/Nu		
		Tasa de Fallos por Año (%).						(Nf*(365*24))/(Th)
		MTBF (horas).									(Th*Nu)/Nf.	(en horas)
		MUT (Tiempo Promedio en Operacion) (horas)		(To)/Na
		Disponibilidad	(%)								(To)/(Th*Nu)
 *------------------------------------------------------------------------------------------------------ */
using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using U5kBaseDatos;

namespace U5kManServer
{
    /// <summary>
    /// 
    /// </summary>
    public enum U5kEstadisticaTiposElementos { Cwp, Gateway, ExtRadio, ExtPhone, Recorder, All }
    public enum U5kEstadisticaEstadoContador { Inactivo, Activo }
    /// <summary>
    /// 
    /// </summary>
    /// <param name="contador"></param>
    public delegate void ProcesaRegistroContador(U5kEstadisticaContador contador);

    /// <summary>
    /// 
    /// </summary>
    public class U5kEstadisticaContador : NucleoGeneric.BaseCode
    {
        /** */
        public event ProcesaRegistroContador ProcesaRegistro;

        /** */
        public U5kEstadisticaTiposElementos TipoElemento { get; set; }
        public string Elemento { get; set; }

        /** */
        public U5kEstadisticaEstadoContador Estado { get; set; }
        public TimeSpan Valor { get; set; }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="proc"></param>
        public U5kEstadisticaContador(ProcesaRegistroContador proc)
        {
            ProcesaRegistro = proc;
        }

        /// <summary>
        /// 
        /// </summary>
        public void Activar()
        {
            Valor = new TimeSpan();
            Estado = U5kEstadisticaEstadoContador.Activo;
        }
        /// <summary>
        /// 
        /// </summary>
        public void Desactivar()
        {
            Registrar();
            Estado = U5kEstadisticaEstadoContador.Inactivo;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public U5kEstadisticaContador Clone()
        {
            return new U5kEstadisticaContador(null)
            {
                TipoElemento = this.TipoElemento,
                Elemento = this.Elemento,
                Estado = this.Estado,
                Valor = this.Valor
            };
        }

        /** */
        protected void Registrar()
        {
            ProcesaRegistro(this);
        }
    }

    /// <summary>
    /// 
    /// </summary>
    public class U5kEstadisticaResultado
    {
        public UInt32 NumeroElementos { get; set; }
        public UInt32 HorasTotales { get; set; }
        public UInt32 HorasOperativas { get; set; }
        public UInt32 NumeroDeFallos { get; set; }
        public UInt32 NumeroDeActivaciones { get; set; }


        public double TasaFallosUnidades { get; set; }
        public double TasaFallosAnno { get; set; }
        public double MTBF { get; set; }
        public double MUT { get; set; }
        public double Disponibilidad { get; set; }
    }

    /// <summary>
    /// 
    /// </summary>
    public class U5kEstadisticaProc : NucleoGeneric.BaseCode
    {
        /// <summary>
        /// 
        /// </summary>
        public static U5kEstadisticaProc Estadisticas = null;
        //public event RecordingEventDelegate Incidencia;
        public event BdtServiceAcces dbService;

        /// <summary>
        /// 
        /// </summary>
        public U5kEstadisticaProc(/*RecordingEventDelegate InciProc, */BdtServiceAcces db_service)
        {
            //Incidencia += InciProc;
            dbService += db_service;

#if DEBUG
            _timer.Interval = 1000 * 1;
            _timer_reg.Interval = 50;
#else
            _timer.Interval = 1000 * 60;
            _timer_reg.Interval = 1000;
#endif
            _timer.Elapsed += new System.Timers.ElapsedEventHandler(Timer_Elapsed);
            _last_inc = _last_reg = DateTime.Now;

            _timer_reg.Elapsed += new System.Timers.ElapsedEventHandler(Timer_Reg_Elapsed);

        }
        /// <summary>
        /// Debe venir con el SMP Global Adquirido...
        /// </summary>
        public void FromMasterToSlave()
        {
#if _STATS_INTERNAL_LOCK_
            lock (_locker)
#endif
            {
                DeactivateAll();
            }
        }
        /// <summary>
        /// Debe venir con el SMP Global Adquirido...
        /// </summary>
        /// <param name="name"></param>
        public void AddOperador(string name)
        {
#if _STATS_INTERNAL_LOCK_
            lock (_locker)
#endif
            {
                AddElemento(U5kEstadisticaTiposElementos.Cwp, name);
            }
        }
        /// <summary>
        /// Debe venir con el SMP Global Adquirido...
        /// </summary>
        /// <param name="name"></param>
        public void AddPasarela(string name)
        {
#if _STATS_INTERNAL_LOCK_
            lock (_locker)
#endif
            {
                AddElemento(U5kEstadisticaTiposElementos.Gateway, name);
            }
        }
        /// <summary>
        /// Debe venir con el SMP Global Adquirido...
        /// </summary>
        /// <param name="name"></param>
        /// <param name="tipo"></param>
        public void AddExternal(string name, int tipo)
        {
            if (tipo == 2 || tipo == 3 || tipo == 5)
            {
#if _STATS_INTERNAL_LOCK_
                lock (_locker)
#endif
                {
                    AddElemento(tipo == 2 ? U5kEstadisticaTiposElementos.ExtRadio :
                        tipo == 3 ? U5kEstadisticaTiposElementos.ExtPhone : U5kEstadisticaTiposElementos.Recorder, name);
                }
            }
        }

        /// <summary>
        /// Debe venir con el SMP Global NO Adquirido...
        /// </summary>
        public void Start()
        {
#if _STATS_INTERNAL_LOCK_
            lock (_locker)
#endif
            {
                _timer.Start();
                _timer_reg.Start();
            }
        }

        /// <summary>
        /// Debe venir con el SMP Global NO Adquirido...
        /// </summary>
        public void Stop()
        {
#if _STATS_INTERNAL_LOCK_
            lock (_locker)
#endif
            {
                _timer_reg.Stop();
                _timer.Stop();

                if (U5kManService._Master == true)
                {
#if _STATS_INTERNAL_LOCK_    
                    DeactivateAll();
#else
                    GlobalServices.GetWriteAccess(data =>
                    {
                        DeactivateAll();
                    });
#endif
                    /** Para grabar los ultimos eventos.. */
                    Timer_Reg_Elapsed(null, null);
                }

                _Contadores.Clear();

#if DEBUG1
                Calcula(new DateTime(2016, 4, 1), DateTime.Today, U5kEstadisticaTiposElementos.Cwp, new List<string>());
#endif
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="nope"></param>
        /// <param name="actividad"></param>
        private void EventoOperador(string nope, bool actividad)
        {
            if (U5kManService._Master == true)
            {
#if _STATS_INTERNAL_LOCK_
                lock (_locker)
#endif
                {
                    U5kEstadisticaContador TOpe = Find(U5kEstadisticaTiposElementos.Cwp, nope);
                    if (TOpe != null )
                        Evento(TOpe, actividad);
                }
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="npas"></param>
        /// <param name="actividad"></param>
        /// <param name="error"></param>
        private void EventoPasarela(string npas, bool actividad)
        {
            if (U5kManService._Master == true)
            {
#if _STATS_INTERNAL_LOCK_
                lock (_locker)
#endif
                {
                    U5kEstadisticaContador TGw = Find(U5kEstadisticaTiposElementos.Gateway, npas);
                    if (TGw != null)
                        Evento(TGw, actividad);
                }
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="name"></param>
        /// <param name="actividad"></param>
        private void EventoExterno(string name, bool actividad)
        {
            if (U5kManService._Master == true)
            {
#if _STATS_INTERNAL_LOCK_
                lock (_locker)
#endif
                {
                    U5kEstadisticaContador contador = _Contadores.Find(
                        (cnt => (cnt.TipoElemento == U5kEstadisticaTiposElementos.ExtRadio ||
                        cnt.TipoElemento == U5kEstadisticaTiposElementos.ExtPhone ||
                        cnt.TipoElemento == U5kEstadisticaTiposElementos.Recorder) && cnt.Elemento == name));
                    if (contador != null)
                    {
                        Evento(contador, actividad);
                    }
                }
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="desde"></param>
        /// <param name="hasta"></param>
        /// <param name="tipo"></param>
        /// <param name="Elementos"></param>
        /// <returns></returns>
        public U5kEstadisticaResultado Calcula(DateTime desde, DateTime hasta, U5kEstadisticaTiposElementos tipo, List<string> Elementos)
        {
            double nu = (double)parNu(tipo, Elementos);
            double th = (double)parTh(desde, hasta);
            double nf = (double)parNf(SqlFiltroFallos(desde, hasta, tipo, Elementos));
            double na = (double)parNa(SqlFiltroActivaciones(desde, hasta, tipo, Elementos));
            double to = (double)parTo(SqlFiltroTiempoOperativo(desde, hasta, tipo, Elementos));

            /** Evita resultados no coherentes en BDT corruptas, normalmente por manipulacion del reloj. */
            to = to >= (th * nu) ? th * nu : to;

            U5kEstadisticaResultado res = new U5kEstadisticaResultado()
            {
                NumeroElementos = (UInt32)nu,
                HorasTotales = (UInt32)(th * nu),
                HorasOperativas = (UInt32)to,
                NumeroDeFallos = (UInt32)nf,
                NumeroDeActivaciones = (UInt32)na,
                TasaFallosUnidades = nu == 0 ? 0 : (nf / nu),               // * 100,
                TasaFallosAnno = th == 0 ? 0 : ((nf * (365 * 24)) / (th)),  // * 100,
                MTBF = nf == 0 ? th * nu : (th * nu) / nf,
                MUT = na == 0 ? to : (to) / na,
                Disponibilidad = th == 0 || nu == 0 ? 0 : (to / (th * nu)) * 100
            };
            //NormalizeRes(res);
            return res;
        }
        /// <summary>
        /// 
        /// </summary>
        private void DeactivateAll()
        {
            foreach (U5kEstadisticaContador contador in _Contadores)
            {
                if (contador.Estado == U5kEstadisticaEstadoContador.Activo)
                    contador.Desactivar();
            }
        }

        /// <summary>
        /// 
        /// </summary>
        private System.Timers.Timer _timer = new System.Timers.Timer();
        private void Timer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            U5kGenericos.TraceCurrentThread(this.GetType().Name + " TimerElapsed");
#if _STATS_INTERNAL_LOCK_
            lock (_locker)
#endif
            {
                if (U5kManService._Master == true)
                {
#if !_STATS_INTERNAL_LOCK_
                    GlobalServices.GetWriteAccess(data => 
                    {
#endif
                    DateTime now = DateTime.Now;
                    TimeSpan time_add = now - _last_inc;
                    TimeSpan time2reg = now - _last_reg;

                    foreach (U5kEstadisticaContador contador in _Contadores)
                    {
                        if (contador.Estado == U5kEstadisticaEstadoContador.Activo)
                        {
                            contador.Valor += time_add;
                            if (time2reg >= _time2reg)
                            {
                                contador.Desactivar();
                                contador.Activar();
                            }
                        }
                    }

                    _last_inc = now;
                    if (time2reg >= _time2reg)
                        _last_reg = now;

                    /** 201808. Se generaban cada 10 min, en vez de cada 10 seg. */
                    //GenerateActivityEvents();
#if !_STATS_INTERNAL_LOCK_
                });
#endif
                }            
                IamAlive1.Tick("Estadisticas-T1", () =>
                {
                    IamAlive1.Message("Estadisticas-T1. Is Alive.");
                });
            }
        }

        /// <summary>
        /// 
        /// </summary>
        private System.Timers.Timer _timer_reg = new System.Timers.Timer();
        private void Timer_Reg_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            U5kGenericos.TraceCurrentThread(this.GetType().Name + " Timer_Reg_Elapsed");

            int nreg = 0;
            int maxreg = sender == null ? 1000 : 10;
#if _STATS_INTERNAL_LOCK_
            lock (_locker_reg)
#endif
            {
#if !_STATS_INTERNAL_LOCK_
                GlobalServices.GetWriteAccess(data =>
                {
#endif
                while (_registros.Count > 0 && nreg < maxreg)
                {
                    U5kEstadisticaContador contador = _registros.Dequeue();
                    LogDebug<U5kEstadisticaProc>(
                         String.Format("{0} Registrada Actividad de {1} segundos.", contador.Elemento, contador.Valor.TotalSeconds));
#if DEBUG_01
                    using (StreamWriter fwriter =
                        new StreamWriter("Estadisticas.dat.txt", true))
                    {
                        fwriter.WriteLine(String.Format("{0}: {1}-{2}: {3} ", DateTime.Now.ToString(), contador.TipoElemento, contador.Elemento, contador.Valor.ToString()));
                    }
#else
                    eIncidencias ninci = eIncidencias.EST_INCI_TOPE;
                    eTiposInci TpInci = TipoIncidencia(contador.TipoElemento);
                    /** 20171218. Evita registros muy largos... */
                    TimeSpan tsValor = contador.Valor > _time2reg ? _time2reg : contador.Valor;
                    string valor = ((UInt32)tsValor.TotalSeconds).ToString();
                    // string valor = ((UInt32)contador.Valor.TotalSeconds).ToString();
                    RecordEvent<U5kEstadisticaProc>(DateTime.Now, ninci, TpInci, contador.Elemento, new List<string>() { valor }.ToArray());
#endif
                    nreg++;
                }

#if _STATS_INTERNAL_LOCK_
                /** 201808. Se generaban cada 10 min, en vez de cada 10 seg. */
                if (U5kManService._Master == true)
                    GenerateActivityEvents();
#endif
                IamAlive2.Tick("Estadisticas-T2", () =>
                {
                    IamAlive2.Message("Estadisticas-T2. Is Alive.");
                });
#if !_STATS_INTERNAL_LOCK_
            });
                if (U5kManService._Master == true)
                {
                    GenerateActivityEvents();
                }
#endif
            }
        }

        /// <summary>
        /// 
        /// </summary>
        private List<U5kEstadisticaContador> _Contadores = new List<U5kEstadisticaContador>();
        private U5kEstadisticaContador Find(U5kEstadisticaTiposElementos tipoelemento, string name)
        {
            U5kEstadisticaContador contador = _Contadores.Find(cnt => (cnt.TipoElemento == tipoelemento && cnt.Elemento == name));
            return contador;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="actividad"></param>
        /// <param name="error"></param>
        private void Evento(U5kEstadisticaContador contador, bool actividad)
        {
            if (actividad == true)
            {
                if (contador.Estado == U5kEstadisticaEstadoContador.Inactivo)
                {
                    RecordEvent<U5kEstadisticaProc>(DateTime.Now, eIncidencias.EST_INCI_EOPE,
                        TipoIncidencia(contador.TipoElemento),
                        contador.Elemento, new List<string>().ToArray());
                    contador.Activar();

                    LogDebug<U5kEstadisticaProc>(
                        String.Format("EventoEstadistica: {0}({1}) => {2}", contador.Elemento, contador.TipoElemento, "Activo" ));
                }
            }
            else
            {
                if (contador.Estado == U5kEstadisticaEstadoContador.Activo)
                {
                    RecordEvent<U5kEstadisticaProc>(DateTime.Now, eIncidencias.EST_INCI_SOPE,
                        TipoIncidencia(contador.TipoElemento),
                        contador.Elemento, new List<string>().ToArray());
                    contador.Desactivar();

                    LogDebug<U5kEstadisticaProc>(
                        String.Format("EventoEstadistica: {0}({1}) => {2}", contador.Elemento, contador.TipoElemento, "Inactivo"));
                }
            }
        }


        private int cntForGenerateActivityEvents = 0;
        private void GenerateActivityEvents()
        {
            if (--cntForGenerateActivityEvents <= 0)
            {
                GlobalServices.GetWriteAccess((data =>
                {
                    /** Los Puestos */
                    data.STDTOPS.ForEach(p =>
                    {
                        EventoOperador(p.name, p.stdg == std.NoInfo ? false : true);
                    });

                    /** Las Pasarelas */
                    data.STDGWS.ForEach(g =>
                    {
                        EventoPasarela(g.name, g.std == std.NoInfo ? false : true);
                    });

                    /** Los Equipos Externos */
                    data.STDEQS.ForEach(e =>
                    {
                        EventoExterno(e.sip_user ?? e.Id, e.EstadoGeneral != std.NoInfo);
                    });
                }));
                cntForGenerateActivityEvents = Properties.u5kManServer.Default.StatisticsActivityMonitoringTime;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="tipoelemento"></param>
        /// <param name="name"></param>
        private void AddElemento(U5kEstadisticaTiposElementos tipoelemento, string name)
        {
            if (Find(tipoelemento, name) == null)
                _Contadores.Add(new U5kEstadisticaContador(RegistraContador)
                {
                    TipoElemento = tipoelemento,
                    Elemento = name,
#if DEBUG1
                    Estado = U5kEstadisticaEstadoContador.Activo,
#else
                    Estado = U5kEstadisticaEstadoContador.Inactivo,
#endif
                    Valor = new TimeSpan()
                });
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="contador"></param>
        private void RegistraContador(U5kEstadisticaContador contador)
        {
#if _STATS_INTERNAL_LOCK_
            lock (_locker_reg)
#endif
            {
                _registros.Enqueue(contador.Clone());
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="TipoElemento"></param>
        /// <returns></returns>
        private eTiposInci TipoIncidencia(U5kEstadisticaTiposElementos TipoElemento)
        {
            return TipoElemento == U5kEstadisticaTiposElementos.Cwp ? eTiposInci.TEH_TOP :
                        TipoElemento == U5kEstadisticaTiposElementos.Gateway ? eTiposInci.TEH_TIFX :
                        TipoElemento == U5kEstadisticaTiposElementos.ExtRadio ? eTiposInci.TEH_EXTERNO_RADIO :
                        TipoElemento == U5kEstadisticaTiposElementos.ExtPhone ? eTiposInci.TEH_EXTERNO_TELEFONIA :
                        TipoElemento == U5kEstadisticaTiposElementos.Recorder ? eTiposInci.TEH_RECORDER : eTiposInci.TEH_SISTEMA;
        }
        /// <summary>
        /// Numero de Unidades Consideradas...
        /// </summary>
        private UInt32 parNu(U5kEstadisticaTiposElementos tipo, List<string> Elementos)
        {
            string PbxIp = U5kManService.PbxEndpoint == null ? "none" : U5kManService.PbxEndpoint.Address.ToString();
            return Elementos.Count != 0 ? (UInt32)Elementos.Count :
                tipo == U5kEstadisticaTiposElementos.Cwp ? ((U5kBdtService)dbService()).GetNumeroTop("departamento") :
                tipo == U5kEstadisticaTiposElementos.Gateway ? ((U5kBdtService)dbService()).GetNumeroGw("departamento") : 
                tipo == U5kEstadisticaTiposElementos.ExtRadio ? ((U5kBdtService)dbService()).ExternalRadioResourcesCount() :
                tipo == U5kEstadisticaTiposElementos.ExtPhone ? ((U5kBdtService)dbService()).ExternalPhoneResourcesCount(PbxIp) :
                tipo == U5kEstadisticaTiposElementos.Recorder ? ((U5kBdtService)dbService()).ExternalRecordersCount() : 0;
        }

        /// <summary>
        /// Tiempo Considerado en Horas.
        /// </summary>
        /// <param name="desde"></param>
        /// <param name="hasta"></param>
        /// <returns></returns>
        private UInt32 parTh(DateTime desde, DateTime hasta)
        {
#if DEBUG1
            return 1050;
#else
            TimeSpan intervalo = hasta - desde;
            return (UInt32)Math.Round(intervalo.TotalHours);
#endif
        }

        /// <summary>
        /// Numero de Fallos en el Intervalo.
        /// </summary>
        /// <param name="sqlFiltroFallos"></param>
        /// <returns></returns>
        private UInt32 parNf(string sqlFiltroFallos)
        {
            try
            {
                return ((U5kBdtService)dbService()).GetScalar(sqlFiltroFallos);
            }
            catch (Exception x)
            {
                LogException<U5kEstadisticaProc>( "", x);
            }
            return 0;
        }

        /// <summary>
        /// Numero de reactivaciones en el Intervalo.
        /// </summary>
        /// <param name="sqlFiltroActivaciones"></param>
        /// <returns></returns>
        private UInt32 parNa(string sqlFiltroActivaciones)
        {
            try
            {
                return ((U5kBdtService)dbService()).GetScalar(sqlFiltroActivaciones);
            }
            catch (Exception x)
            {
                LogException<U5kEstadisticaProc>( "", x);
            }
            return 0;
        }
        /// <summary>
        /// Tiempo total Operativo (de todas las unidades consideradas) en Horas.
        /// </summary>
        /// <param name="sqlFiltroTiempoOperativo"></param>
        /// <returns></returns>
        private UInt32 parTo(string sqlFiltroTiempoOperativo)
        {
            try
            {
#if DEBUG1
                return ((U5kBdtService)dbService()).GetEscalar(sqlFiltroTiempoOperativo);       // Cada segundo se contabiliza como una hora.
#else
                Decimal segundos = (Decimal)((U5kBdtService)dbService()).GetLongScalar(sqlFiltroTiempoOperativo);
                Decimal horas = Decimal.Round(segundos / 3600);
                return (UInt32)horas;
                //return ((U5kBdtService)dbService()).GetEscalar(sqlFiltroTiempoOperativo) / 3600;
#endif
            }
            catch (Exception x)
            {
                LogException<U5kEstadisticaProc>( "", x);
            }
            return 0;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="desde"></param>
        /// <param name="hasta"></param>
        /// <param name="tipo"></param>
        /// <param name="Elementos"></param>
        /// <returns></returns>
        private string SqlFiltroFallos(DateTime desde, DateTime hasta, U5kEstadisticaTiposElementos tipo, List<string> Elementos)
        {
            string strConsulta = string.Format("SELECT COUNT(*) FROM HISTORICOINCIDENCIAS WHERE ({0}{1}{2}{3})",
                /** Fecha Hora */
                SqlFiltroFecha(desde, hasta),
                SqlFiltroTipo(tipo),
                SqlFiltroNames(Elementos),
                " AND ( (IDINCIDENCIA = 5002) )"
                );
            return strConsulta;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="desde"></param>
        /// <param name="hasta"></param>
        /// <param name="tipo"></param>
        /// <param name="Elementos"></param>
        /// <returns></returns>
        private string SqlFiltroActivaciones(DateTime desde, DateTime hasta, U5kEstadisticaTiposElementos tipo, List<string> Elementos)
        {
            string strConsulta = string.Format("SELECT COUNT(*) FROM HISTORICOINCIDENCIAS WHERE ({0}{1}{2}{3})",
                /** Fecha Hora */
                SqlFiltroFecha(desde, hasta),
                SqlFiltroTipo(tipo),
                SqlFiltroNames(Elementos),
                " AND ( (IDINCIDENCIA = 5001) )"
                );
            return strConsulta;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="desde"></param>
        /// <param name="hasta"></param>
        /// <param name="tipo"></param>
        /// <param name="Elementos"></param>
        /// <returns></returns>
        private string SqlFiltroTiempoOperativo(DateTime desde, DateTime hasta, U5kEstadisticaTiposElementos tipo, List<string> Elementos)
        {
            string strConsulta = string.Format("SELECT SUM(DESCRIPCION) FROM HISTORICOINCIDENCIAS WHERE ({0}{1}{2}{3}{4})",
                /** Fecha Hora */
                SqlFiltroFecha(desde, hasta),
                SqlFiltroTipo(tipo),
                SqlFiltroNames(Elementos),
                " AND ( (IDINCIDENCIA = 5000) )",
                String.Format(" AND ( (DESCRIPCION < {0}) )", _time2reg.TotalSeconds + 10)    // 20171218. Hay registros descontrolados que hay que eliminar como no validos...
                );
            return strConsulta;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="desde"></param>
        /// <param name="hasta"></param>
        /// <returns></returns>
        private string SqlFiltroFecha(DateTime desde, DateTime hasta)
        {
            hasta += new TimeSpan(1, 0, 0, 0);
            string filtro2 = string.Format("(  FECHAHORA BETWEEN '{0:yyyy-MM-dd}' AND '{1:yyyy-MM-dd}')", desde, hasta);
            return filtro2;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="tipo"></param>
        /// <returns></returns>
        private string SqlFiltroTipo(U5kEstadisticaTiposElementos tipo)
        {
            string strFiltro = "";
            int tinci = (int)TipoIncidencia(tipo);
            strFiltro = String.Format(" AND (TIPOHW = {0})", tinci);
            //switch (tipo)
            //{
            //    case U5kEstadisticaTiposElementos.Cwp:           // Operadores.
            //        strFiltro = " AND (TIPOHW = 0)";
            //        break;
            //    case U5kEstadisticaTiposElementos.Gateway:           // Pasarelas..
            //        strFiltro = " AND (TIPOHW = 1)";
            //        break;
            //    case U5kEstadisticaTiposElementos.Externo:            // Equipos Externos.
            //        strFiltro = " AND (TIPOHW = 2)";
            //        break;
            //    default:            // Resto...
            //        break;
            //}
            return strFiltro;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="Elementos"></param>
        /// <returns></returns>
        private string SqlFiltroNames(List<string> Elementos)
        {
            if (Elementos.Count > 0)
            {
                string filtro = " AND (";
                foreach (string valor in Elementos)
                {
                    filtro += string.Format("IDHW = '{0}' OR ", valor);
                }
                filtro = filtro.Substring(0, filtro.Length - 3) + ")";
                return filtro;
            }
            return "";
        }

        private void NormalizeRes(U5kEstadisticaResultado res)
        {
            res.Disponibilidad = res.Disponibilidad > 100 ? 100 : res.Disponibilidad;
            res.Disponibilidad = res.Disponibilidad < 0 ? 0 : res.Disponibilidad;
        }
        /// <summary>
        /// 
        /// </summary>            
#if _STATS_INTERNAL_LOCK_
        private Object _locker = new Object();
        private Object _locker_reg = new Object();
#endif
        private DateTime _last_inc, _last_reg;
#if DEBUG1
        private TimeSpan _time2reg = new TimeSpan(0, 1, 0);
#else
        private TimeSpan _time2reg = new TimeSpan(12, 0, 0);
#endif
        private Queue<U5kEstadisticaContador> _registros = new Queue<U5kEstadisticaContador>();
        // private Logger _logger = LogManager.GetCurrentClassLogger();
        ImAliveTick IamAlive1 = new ImAliveTick(60);
        ImAliveTick IamAlive2 = new ImAliveTick(60);
    }
}
