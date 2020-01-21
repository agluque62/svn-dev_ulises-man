using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

using NLog;

namespace Uv5kGwSim
{
    public enum prModes {Ppal = 1, Rsva = 0 };
    public enum cpuIds { cgwa = 0, cgwb = 1 };

    class GwSimData
    {
        /// <summary>
        /// Estado del Recurso Segun el Tipo.
        /// </summary>
        public static Dictionary<CGWResSim.tipos, string> _oidEstado = new Dictionary<CGWResSim.tipos, string>()
        {
            {CGWResSim.tipos.Radio, GwResMib.GwResOids.EstadoRecursoRadio},
            {CGWResSim.tipos.Lcen, GwResMib.GwResOids.EstadoRecursoLC},
            {CGWResSim.tipos.Telef, GwResMib.GwResOids.EstadoRecursoTF},
            {CGWResSim.tipos.Lcen, GwResMib.GwResOids.EstadoRecursoATS}
        };
    }

    /// <summary>
    /// Simula la Pasarela Simple o Dual...
    /// </summary>
    class GwSim
    {
        public static int StartTime = 5000;

        #region Atributos
        /// <summary>
        /// 
        /// </summary>
        bool _dual = Properties.Settings.Default.dual;
        CGWSim _cgwa = new CGWSim(Properties.Settings.Default.CgwaIp, Properties.Settings.Default.CgwaPort);
        CGWSim _cgwb = new CGWSim(Properties.Settings.Default.CgwbIp, Properties.Settings.Default.CgwbPort);
        #endregion
        /// <summary>
        /// 
        /// </summary>
        public GwSim()
        {
        }

        #region operaciones
        /// <summary>
        /// 
        /// </summary>
        public void GlobalStart()
        {
            (new Thread(new ParameterizedThreadStart(CGWDelayedStartPpal)) { IsBackground = true, Name = "StartCGWA" }).Start(_cgwa);
            if (_dual)
                (new Thread(new ParameterizedThreadStart(CGWDelayedStartRsva)) { IsBackground = true, Name = "StartCGWB" }).Start(_cgwb);
        }
        /// <summary>
        /// 
        /// </summary>
        public void GlobalStop()
        {
            _cgwa.Desconecta();
            if (_dual)
                _cgwb.Desconecta();
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="cgw"></param>
        public void CGWStart(cpuIds cgw)
        {
            if (cgw == cpuIds.cgwa)
            {
                if (_dual && _cgwb.Principal)
                {
                    (new Thread(new ParameterizedThreadStart(CGWDelayedStartRsva)) { IsBackground = true, Name = "StartCGWA" }).Start(_cgwa);
                }
                else
                {
                    (new Thread(new ParameterizedThreadStart(CGWDelayedStartPpal)) { IsBackground = true, Name = "StartCGWA" }).Start(_cgwa);
                }
            }
            else
            {
                if (_dual)
                {
                    if (_cgwa.Principal)
                    {
                        (new Thread(new ParameterizedThreadStart(CGWDelayedStartRsva)) { IsBackground = true, Name = "StartCGWB" }).Start(_cgwb);
                    }
                    else
                    {
                        (new Thread(new ParameterizedThreadStart(CGWDelayedStartPpal)) { IsBackground = true, Name = "StartCGWB" }).Start(_cgwb);
                    }
                }
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="cgw"></param>
        public void CGWStop(cpuIds cgw)
        {
            if (cgw == cpuIds.cgwa)
            {
                _cgwa.Desconecta();
                _cgwb.Principal = true;
            }
            else
            {
                _cgwb.Desconecta();
                _cgwa.Principal = true;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="cgw"></param>
        public void StartStop(cpuIds cgw)
        {
            CGWSim _cgw = cgw == cpuIds.cgwa ? _cgwa : _cgwb;
            if (_cgw.Conectada)
                CGWStop(cgw);
            else
                CGWStart(cgw);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="cgw"></param>
        public void CGWReset(cpuIds cgw)
        {
            CGWStop(cgw);
            CGWStart(cgw);
        }

        /// <summary>
        /// 
        /// </summary>
        public void ConmutaPpalRsva()
        {
            if (_dual)
            {
                if (_cgwa.Principal && _cgwb.Conectada)
                {
                    _cgwb.Principal = true;
                    // _cgwb.StartResources();
                    CGWReset(cpuIds.cgwa);
                }
                else if (_cgwb.Principal && _cgwa.Conectada)
                {
                    _cgwa.Principal = true;
                    // _cgwa.StartResources();
                    CGWReset(cpuIds.cgwb);
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public string GlobalStatus
        {
            get
            {
                return string.Format("{0}. CGW-A({5}:{6}): {1},{2}. CGW-B({7}:{8}): {3},{4}",
                    _dual ? "DUAL" : "SIMPLE",
                    _cgwa.Conectada ? "A" : "C",
                    _cgwa.Principal ? "P" : "R",
                    _cgwb.Conectada ? "A" : "C",
                    _cgwb.Principal ? "P" : "R",
                    Properties.Settings.Default.CgwaIp, Properties.Settings.Default.CgwaPort,
                    Properties.Settings.Default.CgwbIp, Properties.Settings.Default.CgwbPort);
            }
        }

        #endregion

        #region Delayed Thread

        /// <summary>
        /// 
        /// </summary>
        /// <param name="obj"></param>
        protected void CGWDelayedStartPpal(object obj)
        {
            Thread.Sleep(GwSim.StartTime);
            ((CGWSim)obj).Conecta(prModes.Ppal);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="obj"></param>
        protected void CGWDelayedStartRsva(object obj)
        {
            Thread.Sleep(GwSim.StartTime);
            ((CGWSim)obj).Conecta(prModes.Rsva);
        }

        #endregion
    }

    /// <summary>
    /// Simula los recursos...
    /// </summary>
    class CGWResSim
    {
        #region public
        /// <summary>
        /// 
        /// </summary>
        public enum tipos { None=0, Radio = 2, Lcen = 3, Telef = 4, Ats = 5 }
        public enum estado { Error = 0, NoError = 1 }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="slot"></param>
        /// <param name="pos"></param>
        public CGWResSim(int slot=0, int pos = 0, tipos tipo=tipos.None)
        {
            Tipo = tipo;
            _port = 16100 + (10 * (slot + 1)) + (pos + 1);            
        }
        /// <summary>
        /// 
        /// </summary>
        public void Start(string ipgw)
        {
            _agent.Init(ipgw, null, _port, 162);
            _mib.Load(_agent, Properties.Settings.Default.IpServidor, Properties.Settings.Default.PortSnmpServidor);
            _agent.Start();

            _Logger.Debug("Arrancando Recurso en {0}:{1}", ipgw, _port);

            if (Tipo != tipos.None)
            {
                _mib.IntOidSet(GwResMib.GwResOids.TipoRecurso, (int)Tipo);
                _mib.IntOidSet(_oidEstado[Tipo], (int)estado.NoError);
            }
        }
        /// <summary>
        /// 
        /// </summary>
        public void Stop()
        {
            _agent.Close();
        }
        #endregion

        #region Propiedades
        public tipos Tipo { get; set; }
        #endregion

        #region Atributos

        int _port = 0;
        GwSnmpAgent _agent = new GwSnmpAgent();
        GwResMib _mib = new GwResMib();
        Logger _Logger = LogManager.GetCurrentClassLogger();

        /// <summary>
        /// Estado del Recurso Segun el Tipo.
        /// </summary>
        public Dictionary<CGWResSim.tipos, string> _oidEstado = new Dictionary<CGWResSim.tipos, string>()
        {
            {CGWResSim.tipos.Radio, GwResMib.GwResOids.EstadoRecursoRadio},
            {CGWResSim.tipos.Lcen, GwResMib.GwResOids.EstadoRecursoLC},
            {CGWResSim.tipos.Telef, GwResMib.GwResOids.EstadoRecursoTF},
            {CGWResSim.tipos.Ats, GwResMib.GwResOids.EstadoRecursoATS}
        };
        #endregion
    }

    /// <summary>
    /// Simulador de la CPU.
    /// </summary>
    class CGWSim
    {       
        #region Atributos

        GwMib _mib = new GwMib();
        GwSnmpAgent _agent = new GwSnmpAgent();
        string SnmpIp = "";
        int SnmpPort = 0;

        /// <summary>
        /// 
        /// </summary>
        CGWResSim[] recursos = new CGWResSim[] 
        {
            new CGWResSim(0,0),new CGWResSim(0,1),new CGWResSim(0,2,CGWResSim.tipos.Radio),new CGWResSim(0,3,CGWResSim.tipos.None),
            new CGWResSim(1,0,CGWResSim.tipos.Radio),new CGWResSim(1,1,CGWResSim.tipos.Radio),new CGWResSim(1,2),new CGWResSim(1,3,CGWResSim.tipos.Radio),
            new CGWResSim(2,0,CGWResSim.tipos.Radio),new CGWResSim(2,1,CGWResSim.tipos.Radio),new CGWResSim(2,2,CGWResSim.tipos.Radio),new CGWResSim(2,3),
            new CGWResSim(3,0,CGWResSim.tipos.Lcen),new CGWResSim(3,1),new CGWResSim(3,2),new CGWResSim(3,3)
        };

        /// <summary>
        /// Estado Interno.
        /// </summary>
        bool Conectado = false;

        #endregion

        #region Privados
        /// <summary>
        /// 
        /// </summary>
        void Start()
        {
            _agent.Init(SnmpIp, null, SnmpPort, 162);
            _mib.Load(_agent, Properties.Settings.Default.IpServidor, Properties.Settings.Default.PortSnmpServidor);
            _agent.Start();
        }

        /// <summary>
        /// 
        /// </summary>
        public void StartResources()
        {
            if (Principal == true)
            {
                foreach (CGWResSim rec in recursos)
                {
                    if (rec.Tipo != CGWResSim.tipos.None)
                        rec.Start(SnmpIp);
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        void Stop()
        {
            _agent.Close();
        }

        /// <summary>
        /// 
        /// </summary>
        void StopResources()
        {
            if (Principal == true)
            {
                foreach (CGWResSim rec in recursos)
                {
                    if (rec.Tipo != CGWResSim.tipos.None)
                        rec.Stop();
                }
            }
        }

        #endregion

        /// <summary>
        /// 
        /// </summary>
        /// <param name="ip"></param>
        /// <param name="port"></param>
        public CGWSim(string ip, int port)
        {
            SnmpIp = ip;
            SnmpPort = port;
        }

        #region Operaciones

        /// <summary>
        /// 
        /// </summary>
        /// <param name="modo"></param>
        public void Conecta(prModes modo)
        {
            if (Conectado == false)
            {
                Start();
                Conectado = true;
                Principal = modo == prModes.Ppal ? true : false;
                // StartResources();
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public void Desconecta()
        {
            if (Conectado == true)
            {
                // StopResources();
                Principal = false;
                Conectado = false;
                Stop();
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="eth1"></param>
        /// <param name="eth2"></param>
        /// <param name="eth3"></param>
        public void SetEstadoLan(bool eth0, bool eth1, bool bond)
        {
            int lan = eth0 ? 1 : 0;
            lan |= (eth1 ? 2 : 0);
            lan |= (bond ? 4 : 0);
            _mib.EstadoLan = lan;
        }

        /// <summary>
        /// 
        /// </summary>
        public bool Principal
        {
            get
            {
                if (Conectado)
                {
                    int mode = _mib.PrincipalReserva;
                    return _mib.PrincipalReserva == (int)prModes.Ppal;
                }
                return false;
            }
            set
            {
                if (Conectado)
                {
                    _mib.PrincipalReserva = (value ? (int)prModes.Ppal : (int)prModes.Rsva);

                    _mib.TipoSlot0 = (value ? 2 : 0);
                    _mib.TipoSlot1 = (value ? 2 : 0);
                    _mib.TipoSlot2 = (value ? 2 : 0);
                    _mib.TipoSlot3 = (value ? 2 : 0);

                    if (value == true)
                        StartResources();
                    else
                        StopResources();
                }
            }
        }
        /// <summary>
        /// 
        /// </summary>
        public int EstadoLan
        {
            get
            {
                return _mib.EstadoLan;
            }
        }
        /// <summary>
        /// 
        /// </summary>
        public bool Conectada
        {
            get
            {
                return Conectado;
            }
        }

        #endregion
    }
}
