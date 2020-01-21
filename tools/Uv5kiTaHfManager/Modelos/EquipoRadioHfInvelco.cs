using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Uv5kiTaHfManager.Helpers;

namespace Uv5kiTaHfManager.Modelos
{
    enum eCmdTx { CMD_FRECUENCIA = 4, CMD_MODULACION = 8, CMD_POTENCIA = 2, CMD_ACTUALIZAR = 1001 }

    class EquipoRadioHfInvelco : EquipoRadio
    {
        const string OID_CMD = ".0";
        const string OID_ESTADO = ".36";
        const string OID_FREC_WR = ".5";
        const string OID_FREC_RD = ".25";

        public string OidBase { get; set; }
        public override event Action<object> StatusChanged;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="_status_changed"></param>
        public EquipoRadioHfInvelco(Action<object> _status_changed)
        {
            StatusChanged += _status_changed;

            RcsSnmpIp = Properties.Settings.Default.RcsSnmpIp;
            RcsSnmpPort = Properties.Settings.Default.RcsSnmpPort;
            RcsSnmpReadCommunity = Properties.Settings.Default.RcsSnmpReadCommunity;
            RcsSnmpWriteCommunity = Properties.Settings.Default.RcsSnmpWriteCommunity;

            SnmpReadTimeout = Properties.Settings.Default.SnmpReadTimeout;
            SnmpWriteTimeout = Properties.Settings.Default.SnmpWriteTimeout;
        }

        /// <summary>
        /// 
        /// </summary>
        public override void GetStatusProc()
        {
            try
            {
                Estado = (EstadoEquipo)SnmpClient.GetInt(
                    RcsSnmpIp,
                    RcsSnmpReadCommunity,
                    OidBase + OID_ESTADO,
                    SnmpReadTimeout,
                    RcsSnmpPort);
                FrecuenciaSeleccionada = ((double)SnmpClient.GetInt(
                    RcsSnmpIp,
                    RcsSnmpReadCommunity,
                    OidBase + OID_FREC_RD,
                    SnmpReadTimeout,
                    RcsSnmpPort))/1000000;
                StatusChanged(this);
            }
            catch (Exception x)
            {
                NLog.LogManager.GetCurrentClassLogger().Error(x, String.Format("Excepcion Obteniendo el Estado del Equipo {0}", Id));
                Estado = EstadoEquipo.Desconectado;
                StatusChanged(this);
            }
            finally
            {
                base.GetStatusProc();
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="mhz"></param>
        /// <param name="_frecuency_change"></param>
        /// <param name="_error"></param>
        public override void SintonizaProc(double mhz, Action<object> _frecuency_change, Action<object> _error)
        {
            System.Threading.Thread.CurrentThread.CurrentUICulture = new System.Globalization.CultureInfo(Uv5kiTaHfManager.Properties.Settings.Default.Idioma); 
            try
            {
                SnmpClient.SetInt(
                    RcsSnmpIp,
                    RcsSnmpWriteCommunity,
                    OidBase + OID_FREC_WR,
                    (int)(mhz*1000000),
                    SnmpWriteTimeout,
                    RcsSnmpPort);
                SnmpClient.SetInt(
                    RcsSnmpIp,
                    RcsSnmpWriteCommunity,
                    OidBase + OID_CMD,
                    (int)eCmdTx.CMD_FRECUENCIA,
                    SnmpWriteTimeout,
                    RcsSnmpPort);

                FrecuenciaSeleccionada = mhz;
                _frecuency_change(this);
            }
            catch (Exception x)
            {
                NLog.LogManager.GetCurrentClassLogger().Error(x, String.Format("Excepcion Sintonizando el Equipo {0}", Id));
                _error(Id + ". " + Properties.Resources.Error_al_Sintonizar);
            }
            finally
            {
                base.SintonizaProc(mhz, _frecuency_change, _error);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        string RcsSnmpIp { get; set; }
        int RcsSnmpPort { get; set; }
        string RcsSnmpReadCommunity { get; set; }
        string RcsSnmpWriteCommunity { get; set; }

        int SnmpReadTimeout { get; set; }
        int SnmpWriteTimeout { get; set; }
    }
}
