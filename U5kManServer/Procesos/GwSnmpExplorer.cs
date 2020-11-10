// #define _EXPLORE_THREADS_
using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.Threading;
using System.Net;

using System.Threading.Tasks;
using System.Net.Http;

using Lextm.SharpSnmpLib;
//using Lextm.SharpSnmpLib.Messaging;
//using Lextm.SharpSnmpLib.Objects;
//using Lextm.SharpSnmpLib.Pipeline;

using NucleoGeneric;
using U5kBaseDatos;
using U5kManServer.WebAppServer;

using Utilities;
namespace U5kManServer
{
    /// <summary>
    /// 
    /// </summary>
    enum eGwPar
    {
        None = 0,
        GwStatus,
        Slot0Type, Slot1Type, Slot2Type, Slot3Type,
        Slot0Status, Slot1Status, Slot2Status, Slot3Status, LanStatus, MainOrStandby,
        ResourceType,
        RadioResourceType, RadioResourceStatus,
        IntercommResourceType, IntercommResourceStatus,
        LegacyPhoneResourceType, LegacyPhoneResourceStatus,
        ATSPhoneResourceType, ATSPhoneResourceStatus,
        Lan2Status
    };

    class GwHelper
    {
        public static string SlotStd2String(Int32 nslot, Int32 tipo, Int32 slotstd)
        {
            // BIT | 7 | 6 | 5 | 4 | 3 | 2 | 1 | 0 |
            // CAN | - | - | - | 3 | 2 | 1 | 0 | - |
            return tipo == 2 ? String.Format("{4}: [{0} {1} {2} {3}]",
                                (slotstd & Convert.ToInt32("00010", 2)) != 0 ? "0" : "-",
                                (slotstd & Convert.ToInt32("00100", 2)) != 0 ? "1" : "-",
                                (slotstd & Convert.ToInt32("01000", 2)) != 0 ? "2" : "-",
                                (slotstd & Convert.ToInt32("10000", 2)) != 0 ? "3" : "-",
                                nslot) : String.Format("{0}", nslot);
        }
        public static void SetToOutOfOrder(stdPhGw phgw)
        {
            phgw.SnmpDataReset();
            phgw.version = string.Empty;
        }
    }
    /// <summary>
    /// 
    /// </summary>
    class GwExplorer : NucleoGeneric.NGThread
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        static string OidGet(string id, string _default)
        {
            foreach (string s in Properties.u5kManServer.Default.GwOids)
            {
                string[] p = s.Split(':');
                if (p.Count() == 2 && p[0] == id)
                    return p[1];
            }

            return _default;
        }
        /// <summary>
        /// 
        /// </summary>
        static public Dictionary<eGwPar, string> _GwOids = new Dictionary<eGwPar, string>()
        {
            // General de la Pasarela.
            {eGwPar.GwStatus,OidGet("EstadoGw",".1.1.100.2.0")},
            {eGwPar.Slot0Type,OidGet("TipoSlot0",".1.1.100.31.1.1.0")},
            {eGwPar.Slot1Type,OidGet("TipoSlot1",".1.1.100.31.1.1.1")},
            {eGwPar.Slot2Type,OidGet("TipoSlot2",".1.1.100.31.1.1.2")},
            {eGwPar.Slot3Type,OidGet("TipoSlot3",".1.1.100.31.1.1.3")},
            {eGwPar.Slot0Status,OidGet("EstadoSlot0",".1.1.100.31.1.2.0")},
            {eGwPar.Slot1Status,OidGet("EstadoSlot1",".1.1.100.31.1.2.1")},
            {eGwPar.Slot2Status,OidGet("EstadoSlot2",".1.1.100.31.1.2.2")},
            {eGwPar.Slot3Status,OidGet("EstadoSlot3",".1.1.100.31.1.2.3")},
            {eGwPar.MainOrStandby,OidGet("PrincipalReserva","1.1.100.21.0")},
            {eGwPar.LanStatus,OidGet("EstadoLan","1.1.100.22.0")},
        
            // Para cada Recurso.
            {eGwPar.ResourceType,OidGet("TipoRecurso",".1.1.100.100.0")},

            {eGwPar.RadioResourceType,OidGet("TipoRecursoRadio",".1.1.200")},
            {eGwPar.RadioResourceStatus,OidGet("EstadoRecursoRadio",".1.1.200.2.0")},

            {eGwPar.IntercommResourceType,OidGet("TipoRecursoLC",".1.1.300")},
            {eGwPar.IntercommResourceStatus,OidGet("EstadoRecursoLC",".1.1.300.2.0")},

            {eGwPar.LegacyPhoneResourceType,OidGet("TipoRecursoTF",".1.1.400")},
            {eGwPar.LegacyPhoneResourceStatus,OidGet("EstadoRecursoTF",".1.1.400.2.0")},

            {eGwPar.ATSPhoneResourceType,OidGet("TipoRecursoATS",".1.1.500")},
            {eGwPar.ATSPhoneResourceStatus,OidGet("EstadoRecursoATS",".1.1.500.2.0")},

        };

        /// <summary>
        /// Polling a la Pasarela.
        /// </summary>
        static List<Variable> _GwVarList = new List<Variable>()
        {
            new Variable(new ObjectIdentifier(_GwOids[eGwPar.GwStatus])),
            new Variable(new ObjectIdentifier(_GwOids[eGwPar.MainOrStandby])),
            new Variable(new ObjectIdentifier(_GwOids[eGwPar.LanStatus])),
            new Variable(new ObjectIdentifier(_GwOids[eGwPar.Slot0Type])),
            new Variable(new ObjectIdentifier(_GwOids[eGwPar.Slot1Type])),
            new Variable(new ObjectIdentifier(_GwOids[eGwPar.Slot2Type])),
            new Variable(new ObjectIdentifier(_GwOids[eGwPar.Slot3Type])),
            new Variable(new ObjectIdentifier(_GwOids[eGwPar.Slot0Status])),
            new Variable(new ObjectIdentifier(_GwOids[eGwPar.Slot1Status])),
            new Variable(new ObjectIdentifier(_GwOids[eGwPar.Slot2Status])),
            new Variable(new ObjectIdentifier(_GwOids[eGwPar.Slot3Status]))
        };

        /// <summary>
        /// Tipos Notificados...
        /// </summary>
        const int RadioResource_AgentType = 2;
        const int IntercommResource_AgentType = 3;
        const int LegacyPhoneResource_AgentType = 4;
        const int ATSPhoneResource_AgentType = 5;

        /// <summary>
        /// Oid de los Tipos Reales segun los tipos notificados.
        /// </summary>
        Dictionary<int, string> _TypesOids = new Dictionary<int, string>()
        {
            { RadioResource_AgentType, _GwOids[eGwPar.RadioResourceType] },
            { IntercommResource_AgentType, _GwOids[eGwPar.IntercommResourceType] },
            { LegacyPhoneResource_AgentType, _GwOids[eGwPar.LegacyPhoneResourceType]},
            { ATSPhoneResource_AgentType, _GwOids[eGwPar.ATSPhoneResourceType] },
        };

        /// <summary>
        /// Oid de los Estados segun los tipos notificados.
        /// </summary>
        Dictionary<int, string> _StatusOids = new Dictionary<int, string>()
        {
            { RadioResource_AgentType, _GwOids[eGwPar.RadioResourceStatus] },
            { IntercommResource_AgentType, _GwOids[eGwPar.IntercommResourceStatus] },
            { LegacyPhoneResource_AgentType, _GwOids[eGwPar.LegacyPhoneResourceStatus] },
            { ATSPhoneResource_AgentType, _GwOids[eGwPar.ATSPhoneResourceStatus] },
        };

        /// <summary>
        /// Incidencias de Activacion Asociadas a los tipos de Recurso notificados.
        /// </summary>
        static protected Dictionary<trc, eIncidencias> ResourceActivationEventsCodes = new Dictionary<trc, eIncidencias>()
        {
            {trc.rcRadio, eIncidencias.IGW_CONEXION_RECURSO_RADIO},
            {trc.rcTLF, eIncidencias.IGW_CONEXION_RECURSO_TLF},
            {trc.rcLCE, eIncidencias.IGW_CONEXION_RECURSO_TLF},
            {trc.rcATS, eIncidencias.IGW_CONEXION_RECURSO_R2},
            {trc.rcNotipo, eIncidencias.IGNORE}
        };

        /// <summary>
        /// Incidencias de Desactivacion Asociadas a los tipos de Recurso notificados...
        /// </summary>
        static protected Dictionary<trc, eIncidencias> ResourceDeactivationEventsCodes = new Dictionary<trc, eIncidencias>()
        {
            {trc.rcRadio, eIncidencias.IGW_DESCONEXION_RECURSO_RADIO},
            {trc.rcTLF, eIncidencias.IGW_DESCONEXION_RECURSO_TLF},
            {trc.rcLCE, eIncidencias.IGW_DESCONEXION_RECURSO_TLF},
            {trc.rcATS, eIncidencias.IGW_DESCONEXION_RECURSO_R2},
            {trc.rcNotipo, eIncidencias.IGNORE}
        };

        /// <summary>
        /// 
        /// </summary>
        public static event ChangeStatusDelegate ChangeStatus;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pdata"></param>
        public GwExplorer(ChangeStatusDelegate ChangeStatusRoutine)
        {
            Name = "GwSnmpExplorer";
            ChangeStatus = ChangeStatusRoutine;
        }
        /// <summary>
        /// 
        /// </summary>
        protected override void Run()
        {
            U5kGenericos.TraceCurrentThread(this.GetType().Name);

            //U5kGenericos.SetCurrentCulture();
            LogInfo<GwExplorer>("Arrancado...");

            Decimal interval = Properties.u5kManServer.Default.SpvInterval;     // Tiempo de Polling,
            Decimal threadTimeout = 2 * interval / 3;                           // Tiempo de proceso individual.
            Decimal poolTimeout = 3 * interval / 4;                             // Tiempo máximo del Pool de Procesos.            
            
            // 20200805. Control del Polling a pasarelas.
            var taskControl = new PollingHelper();
            using (timer = new TaskTimer(TimeSpan.FromMilliseconds((double)interval), this.Cancel))
            {
                while (IsRunning())
                {
                    if (U5kManService._Master == true)
                    {
#if POOL_METHOD_0
                        List<stdGw> localgws = null;        // new List<stdGw>();
                        try
                        {
                            Utilities.TimeMeasurement tm = new Utilities.TimeMeasurement("GW Explorer");

                            // Relleno los datos...
                            GlobalServices.GetWriteAccess((gdata) => localgws = gdata.STDGWS.Select(gw => new stdGw(gw)).ToList());

                            // Arranco los procesos de exploracion...
                            List<Task> task = new List<Task>();
                            localgws?.ForEach((gw) =>
                            {
                                task.Add(
                                    BackgroundTaskFactory.StartNew(gw.name, () =>
                                    {
                                        U5kGenericos.TraceCurrentThread(this.GetType().Name + " " + gw.name);
                                        ExploraGw(gw);
                                    },
                                    (id, excep) =>
                                    {
                                        LogException<GwExplorer>("Supervisando Pasarela " + gw.name, excep);
                                    }, TimeSpan.FromMilliseconds((double)threadTimeout))
                                    );
                            });

                            // Espero que acaben todos los procesos.
                            Task.WaitAll(task.ToArray(), TimeSpan.FromMilliseconds((double)poolTimeout));
                            tm.StopAndPrint((msg) => LogTrace<GwExplorer>(msg));
                        }
                        catch (Exception x)
                        {
                            if (x is ThreadAbortException)
                            {
                                Thread.ResetAbort();
                                break;
                            }
                            else if (x is AggregateException)
                            {
                                foreach (var excep in (x as AggregateException).InnerExceptions)
                                {
                                    LogTrace<GwExplorer>($"Excepcion {excep.Message}");
                                }
                            }
                            else
                            {
                                LogException<GwExplorer>("Supervisando Pasarelas ", x);
                            }
                        }

                        /// Copio los datos obtenidos a la tabla...
                        if (localgws != null)
                        {
                            GlobalServices.GetWriteAccess((gdata) => gdata.GWSDIC = localgws.Select(gw => gw).ToDictionary(gw => gw.name, gw => gw));
                        }
#else
                        GlobalServices.GetWriteAccess((gdata) =>
                        {
                            // limpiar pollingControl con las Pasarelas que puedan desaparecer de la configuracion.
                            taskControl.DeleteNotPresent(gdata.STDGWS.Select(g => g.name).ToList());

                            // Relleno los datos...
                            gdata.STDGWS.ForEach(gw =>
                            {
                                if (taskControl.IsTaskActive(gw.name) == false)
                                {
                                    var newGw = new stdGw(gw);
                                    var task = Task.Factory.StartNew(() =>
                                    {
                                        try
                                        {
                                            LogTrace<GwExplorer>($"Exploracion {newGw.name} iniciada.");
                                            ExploraGw(newGw);
#if DEBUG1
                                            // Para asegurar un tiempo de ejecucion.
                                            Task.Delay(TimeSpan.FromMilliseconds(500)).Wait();
#endif
                                            /// Copio los datos obtenidos a la tabla...
                                            GlobalServices.GetWriteAccess((gdata1) =>
                                            {
                                                if (gdata1.GWSDIC.ContainsKey(newGw.name))
                                                {
                                                    /** 20200813. Solo actualiza el estado si no se ha cambiado en medio la configuracion */
                                                    if (gdata1.GWSDIC[newGw.name].Equals(newGw))
                                                    {
                                                        gdata1.GWSDIC[newGw.name].CopyFrom(newGw);
                                                    }
                                                    else
                                                    {
                                                        LogWarn<GwExplorer>($"Exploracion {newGw.name}. Resultado Exploracion ignorado. Cambio de configuracion.");
                                                    }
                                                }
                                                else
                                                {
                                                    LogWarn<GwExplorer>($"Exploracion {newGw.name}. Resultado Exploracion ignorado.  Pasarela Eliminada");
                                                }
                                            });
                                        }
                                        catch (Exception x)
                                        {
                                            LogException<GwExplorer>("Supervisando Pasarela " + newGw.name, x);
                                        }
                                        finally
                                        {
                                            LogTrace<GwExplorer>($"Exploracion de {newGw.name} finalizada.");
                                        }
                                    }, TaskCreationOptions.LongRunning);
                                    taskControl.SetTask(gw.name, task);
                                }
                                else
                                {
                                    // todo. Algun tipo de supervision si nunca vuelve...
                                    LogWarn<GwExplorer>($"Exploracion de Pasarela {gw.name} no finalizada en Tiempo ...");
                                }
                            });
                        });
#if DEBUG1
                        /** Para simular Sectorizaciones con cambios 'problematicos' */
                        tm.FromCreation(TimeSpan.FromMinutes(1), () =>
                        {
                            GlobalServices.GetWriteAccess((gdata) =>
                            {
                                //gdata.GWSDIC.Remove("CGW3");
                                gdata.GWSDIC["CGW1"] = new stdGw(null)
                                {
                                    name = "CGW1",
                                    ip = "192.168.0.51",
                                    Dual = true,
                                    gwA = new stdPhGw()
                                    {
                                        name = "CGW01-A",
                                        ip = "192.168.0.50"
                                    },
                                    gwB = new stdPhGw()
                                    {
                                        name = "CGW01-B",
                                        ip = "192.168.0.59"
                                    }
                                };
                            });
                        });
#endif

#endif
                    }
                    GoToSleepInTimer();
                }
            }
            Dispose();
            LogInfo<GwExplorer>("Finalizado...");
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="name"></param>
        /// <param name="nslot"></param>
        /// <param name="slot"></param>
        /// <param name="tipo"></param>
        static public void SlotTypeSet(/*stdGw gw, */stdPhGw pgw, int nslot, stdSlot slot, int tipo, int estado)
        {
            std current = tipo == 2 ? std.Ok : std.NoInfo;
            estado = tipo == 2 ? (estado | 0x01) : (estado & 0xFFFE);

            //slot.std_online = ChangeStatus(slot.std_online,
            //                               current, 0,
            //                               current != std.Ok ? eIncidencias.IGW_DESCONEXION_IA4 : eIncidencias.IGW_CONEXION_IA4,
            //                               eTiposInci.TEH_TIFX, pgw.name, /*nslot, */GwHelper.SlotStd2String(nslot, tipo, estado));

            if (slot.lastResMsc != estado || current != slot.std_online)
            {
                if (current == std.Ok)
                {
                    RecordEvent<GwExplorer>(DateTime.Now, eIncidencias.IGW_CONEXION_IA4, eTiposInci.TEH_TIFX, pgw.name, /*nslot, */
                        Params(GwHelper.SlotStd2String(nslot, 2, estado)));
                }
                else
                {
                    RecordEvent<GwExplorer>(DateTime.Now, eIncidencias.IGW_DESCONEXION_IA4, eTiposInci.TEH_TIFX, pgw.name, /*nslot, */
                        Params(GwHelper.SlotStd2String(nslot, 2, estado)));
                    slot.Reset();
                }
                slot.lastResMsc = estado;
                slot.std_online = current;
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="name"></param>
        /// <param name="nslot"></param>
        /// <param name="slot"></param>
        /// <param name="estado"></param>
        static public void SlotStateSet(/*stdGw gw, */stdPhGw pgw, int nslot, stdSlot slot, int estado)
        {
            /** El primer bit es el estado de la tarjeta */
            estado = (estado >> 1);

            for (int irec = 0; irec < 4; irec++)
            {
                stdRec rec = slot.rec[irec];
                bool presente = ((estado >> irec) & 1) == 1 ? true : false;

                // 20180111. Quitamos este historico....
                // ChangeStatus(rec.presente ? std.Ok : std.NoInfo,
                //    presente ? std.Ok : std.NoInfo,
                //    0,
                //    eIncidencias.IGW_EVENTO,
                //    eTiposInci.TEH_TIFX,
                //    pgw.name,
                //    presente ? idiomas.strings.GWS_ResInterfazSi : idiomas.strings.GWS_ResInterfazNo,   // "Interfaz de Recurso Disponible" : "Interfaz de Recurso no Disponible",
                //    nslot, irec);

                // 20180111. Quitamos el historico y solo actualizamos las variables locales...
                //eIncidencias inci = rec.tipo == itf.rcRadio ? eIncidencias.IGW_DESCONEXION_RECURSO_RADIO :
                //    rec.tipo == itf.rcAtsN5 || rec.tipo == itf.rcAtsR2 ? eIncidencias.IGW_DESCONEXION_RECURSO_R2 :
                //    eIncidencias.IGW_DESCONEXION_RECURSO_TLF;

                //rec.std_online = ChangeStatus(rec.std_online,
                //                              (presente == false ? std.NoInfo : rec.std_online),
                //                              0,
                //                              inci,
                //                              eTiposInci.TEH_TIFX, pgw.name, rec.name);
                rec.presente = presente;
                rec.std_online = (presente == false ? std.NoInfo : rec.std_online);
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="ip"></param>
        /// <param name="slot"></param>
        protected void SlotRecsAct(/*stdGw gw, */stdPhGw pgw, string ip, stdSlot slot)
        {
            if (slot.std_online != std.Ok)
                return;

            SnmpClient snmpc = new SnmpClient();
            for (int rec = 0; rec < 4; rec++)
            {
                //if (slot.rec[rec].bdt == false || slot.rec[rec].presente == false)
                //    continue;
                if (/*slot.rec[rec].bdt == false || */slot.rec[rec].presente == false)
                    continue;

                int tipo = -1;

                // Tipo Notificado de Recurso.
                if (!snmpc.GetInt(ip, slot.rec[rec].snmp_port, "public", _GwOids[eGwPar.ResourceType], 1000, out tipo))
                    continue;

                if (!TestTipoNotificado(tipo))
                    continue;

                SlotRecursoTipoAgenteSet(/*gw, */pgw, slot.rec[rec], tipo);

                // Tipo de Interfaz
                /**
                int titf = (int)itf.rcNotipo;
                SnmpClient.GetInt(ip, slot.rec[rec].snmp_port, "public", OidsTipos[tipo], 1000, out titf);
                SlotRecursoTipoInterfazSet(gw, pgw, slot.rec[rec], titf);
                */

                // Estado  de Recurso.
                int estado = (int)std.NoInfo;
                if (!snmpc.GetInt(ip, slot.rec[rec].snmp_port, "public", _StatusOids[tipo], 1000, out estado))
                    continue;

                SlotRecursoEstadoSet(/*gw, */pgw, slot.rec[rec], estado, (trc)tipo);
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="gwname"></param>
        /// <param name="rec"></param>
        /// <param name="tipo"></param>
        public static void SlotRecursoTipoAgenteSet(/*stdGw gw, */stdPhGw pgw, stdRec rec, int tipo)
        {
            rec.tipo_online = (trc)tipo;              // Todo. Generar Incidencia si procede. Esta Incidencia no Existe.
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="gwname"></param>
        public static void SlotRecursoTipoInterfazSet(/*stdGw gw, */stdPhGw pgw, stdRec rec, int tipo)
        {
            rec.tipo_itf = (itf)tipo;               // Todo. Generar Incidencia si procede. Esta Incidencia no Existe.
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="gwname"></param>
        /// <param name="rec"></param>
        /// <param name="estado">0: NP, 1: OK, 2: Fallo, 3: Degradado.</param>
        public static void SlotRecursoEstadoSet(/*stdGw gw, */stdPhGw pgw, stdRec rec, int estado, trc tipo)
        {
            if (rec.presente)
            {
                if (tipo == trc.rcRadio)
                {
                    /** Para evitar los Fuera de Servicio por falta de sesion radio... */
                    std current = estado == 1 ? std.Ok : std.Error;
                    if (rec.std_online != current)
                    {
                        rec.std_online = current;
                        RecordEvent<GwExplorer>(DateTime.Now, estado != 0 ? ResourceActivationEventsCodes[tipo] : ResourceDeactivationEventsCodes[tipo],
                                                  eTiposInci.TEH_TIFX, pgw.name, Params(rec.name));
                    }
                }
                else
                {
                    rec.std_online = ChangeStatus(rec.std_online,
                                                  estado == 1 ? std.Ok : std.Error, 0,
                                                  estado == 1 ? ResourceActivationEventsCodes[tipo] : ResourceDeactivationEventsCodes[tipo],
                                                  eTiposInci.TEH_TIFX, pgw.name, rec.name);
                }
            }
            else
            {
                rec.std_online = std.NoInfo;
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="tipo"></param>
        /// <returns></returns>
        bool TestTipoNotificado(int tipo)
        {
            return tipo ==
                RadioResource_AgentType ||
                tipo == IntercommResource_AgentType ||
                tipo == LegacyPhoneResource_AgentType ||
                tipo == ATSPhoneResource_AgentType ? true : false;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="gw"></param>
        public static void GwActualizaEstado(stdGw gw)
        {
            //std std_old = gw.std;

            gw.presente = gw.Dual == false ? gw.gwA.presente : (gw.gwA.presente == true || gw.gwB.presente == true) ? true : false;
            if (gw.presente == false)
                gw.Reset();
            gw.std = gw.presente == false ? std.NoInfo : gw.Errores == true ? std.Error : std.Ok;

            //if (std_old != gw.std)
            //{
            //    switch (gw.std)           // Ha habido un cambio de estado global...
            //    {
            //        case std.NoInfo:        // Desconectado o No Operativo.
            //            U5kEstadisticaProc.Estadisticas.EventoPasarela(gw.name, false);
            //            break;

            //        case std.Error:         // Conectado pero en Fallo.
            //        case std.Ok:            // Plenamente operativo.
            //            U5kEstadisticaProc.Estadisticas.EventoPasarela(gw.name, true);
            //            break;

            //        default:
            //            break;
            //    }
            //}
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="gwp"></param>
        /// <param name="pgw"></param>
        /// <param name="estado"></param>
        public static void PhGwCambioEstado(/*stdGw gw, */stdPhGw pgw, int estado)
        {
            /* Actualizo el estado de la pasarela fisica **/
            bool nuevo_estado = estado == 1 ? true : false;
            if (nuevo_estado != pgw.presente)
            {
                pgw.presente = nuevo_estado;
                std std_new = pgw.presente == false ? std.NoInfo : pgw.Errores == true ? std.Error : std.Ok;
                eIncidencias inci = std_new == std.NoInfo ? eIncidencias.IGW_CAIDA : eIncidencias.IGW_ENTRADA;

                pgw.std = ChangeStatus(pgw.std, std_new, 0, inci, eTiposInci.TEH_TIFX, pgw.name);

                if (pgw.presente == false)
                {
                    /** Reset Estado GW fisica */
                    pgw.Reset();
                }
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pgw"></param>
        /// <param name="estado"></param>
        public static void PhGwPrincipalReservaSet(/*stdGw gw, */stdPhGw pgw, int estado)
        {
            pgw.Seleccionada = ChangeStatus(pgw.Seleccionada == true ? std.Ok : std.NoInfo,
                estado == 0 ? std.NoInfo : std.Ok, 0, eIncidencias.IGW_PRINCIPAL_RESERVA, eTiposInci.TEH_TIFX, pgw.name,
                pgw.name,
                estado == 0 ? idiomas.strings.GWS_Reserva : idiomas.strings.GWS_Principal/* "Reserva" : "Principal"*/) == std.Ok ? true : false;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pwg"></param>
        /// <param name="status"></param>
        public static void PhGwLanStatusSet(/*stdGw gw, */stdPhGw pgw, int status)
        {
            int bond = (status & 0x4) >> 2;
            int eth1 = (status & 0x2) >> 1;
            int eth0 = (status & 0x1);

            pgw.lan1 = eth0 == 1 ? std.Ok : std.Error;
            pgw.lan2 = bond == 0 ? std.NoInfo : eth1 == 1 ? std.Ok : std.Error;
        }

#region Threads de Exploracion en Paralelo.

        /// <summary>
        /// Explora una pasarela logica.
        /// </summary>
        /// <param name="obj"></param>
        protected void ExploraGw(object obj)
        {
            stdGw gw = (stdGw)obj;

            if (gw.gwA.IpConn.IsPollingTime() == true)
            {
                LogTrace<GwExplorer>($"POLL Executed: {gw.gwA.name}");
                ExplorePhGw(gw.gwA);
            }
            else
            {
                LogTrace<GwExplorer>($"POLL Skipped : {gw.gwA.name}");
            }

            if (gw.Dual)
            {
                if (gw.gwB.IpConn.IsPollingTime() == true)
                {
                    LogTrace<GwExplorer>($"POLL Executed: {gw.gwB.name}");
                    ExplorePhGw(gw.gwB);
                }
                else
                {
                    LogTrace<GwExplorer>($"POLL Skipped : {gw.gwB.name}");
                }
            }

            /** Actualiza los Parametros Globales de la Pasarela */
            GwActualizaEstado(gw);

            /** */
            if (gw.presente)
            {
                GetNtpStatus(gw);
            }
        }

#region Exploracion de GW Unificada

        /// <summary>
        /// 
        /// </summary>
        /// <param name="ipgw"></param>
        /// <param name="port"></param>
        protected void ExploraGwStdGen_unificada(stdPhGw pgw)
        {
            IPEndPoint gwep = new IPEndPoint(IPAddress.Parse(pgw.ip), pgw.snmpport);
            OctetString community = new OctetString("public");
            List<Variable> vIn = new List<Variable>()
            {
                new Variable(new ObjectIdentifier(".1.3.6.1.4.1.7916.8.3.1.1.2.0")),   // Estado Hw.
                new Variable(new ObjectIdentifier(".1.3.6.1.4.1.7916.8.3.1.1.6.0")),   // Estado LAN1
                new Variable(new ObjectIdentifier(".1.3.6.1.4.1.7916.8.3.1.1.7.0")),   // Estado LAN2
                new Variable(new ObjectIdentifier(".1.3.6.1.4.1.7916.8.3.1.1.8.0")),   // Estado P/R,
                new Variable(new ObjectIdentifier(".1.3.6.1.4.1.7916.8.3.1.1.4.0")),   // Estado FA,
                new Variable(new ObjectIdentifier(".1.3.6.1.4.1.7916.8.3.1.1.1.0")),   // Identificador. Habilita el envio de TRAPS
            };
            SnmpClient snmpc = new SnmpClient();

            IList<Variable> vOut = snmpc.Get(VersionCode.V2, gwep, community, vIn, pgw.SnmpTimeout, pgw.SnmpReintentos);
            // estadoGeneral. 0: No Inicializado, 1: Ok, 2: Fallo, 3: Aviso.
            int stdGeneral = snmpc.Integer(vOut[0].Data);
            // stdLAN1. 0: No Presente, 1: Ok, 2: Error.
            int stdLan1 = snmpc.Integer(vOut[1].Data);
            // stdLAN2. 0: No Presente, 1: Ok, 2: Error.
            int stdLan2 = snmpc.Integer(vOut[2].Data);
            // stdCpuLocal. 0: No Presente. 1: Principal, 2: Reserva, 3: Arrancando
            int stdPR = snmpc.Integer(vOut[3].Data);
            // stdFA. 0: No Presente. 1: Ok, 2: Error
            int stdFA = snmpc.Integer(vOut[4].Data);
            pgw.std = stdGeneral == 0 ? std.NoInfo : stdGeneral == 1 ? std.Ok : std.Error;

            int stdLan = (stdLan1 == 1 ? 0x01 : 0x00) | (stdLan2 == 1 ? 0x02 : 0x00);
            PhGwLanStatusSet(pgw, (0x04 | stdLan));                 // En este tipo de Pasarelas BOND configurado...

            PhGwPrincipalReservaSet(pgw, stdPR == 1 ? 1 : 0);       // Solo se marca PPAL si está en PPAL en cualquier otro caso se marca RSVA

            pgw.stdFA = stdFA == 0 ? std.NoInfo : stdFA == 1 ? std.Ok : stdFA == 2 ? std.Error : std.NoExiste;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="obj"></param>
        protected void ExploraSlot_unificada(object obj)
        {
            KeyValuePair<stdPhGw, int> objIn = (KeyValuePair<stdPhGw, int>)obj;
            stdPhGw gw = objIn.Key;
            int nslot = objIn.Value;
            stdSlot slot = gw.slots[nslot];
            IPEndPoint gwep = new IPEndPoint(IPAddress.Parse(gw.ip), gw.snmpport);
            OctetString community = new OctetString("public");

            try
            {
                string oidbase = ".1.3.6.1.4.1.7916.8.3.1.3.2.1.";
                List<Variable> vIn = new List<Variable>()
                {
                    new Variable(new ObjectIdentifier(oidbase+"2."+(nslot+1).ToString())),   // Tipo. 0: Error, 1: IA4, 2: IQ1
                    new Variable(new ObjectIdentifier(oidbase+"3."+(nslot+1).ToString())),   // Status,
                    new Variable(new ObjectIdentifier(oidbase+"4."+(nslot+1).ToString())),   // Canal-0
                    new Variable(new ObjectIdentifier(oidbase+"5."+(nslot+1).ToString())),   // Canal-1
                    new Variable(new ObjectIdentifier(oidbase+"6."+(nslot+1).ToString())),   // Canal-2
                    new Variable(new ObjectIdentifier(oidbase+"7."+(nslot+1).ToString()))    // Canal-3
                };
                SnmpClient snmpc = new SnmpClient();

                IList<Variable> vOut = snmpc.Get(VersionCode.V2, gwep, community, vIn, gw.SnmpTimeout, gw.SnmpReintentos);
                int stipo = snmpc.Integer(vOut[0].Data);                            // 0: Error, 1: IA4, 2: IQ1
                int status = snmpc.Integer(vOut[1].Data);                           // 0: No presente, 1: Presente

                stipo = status == 0 ? 0 : (stipo == 1 ? 2 : 0);

                int can0 = snmpc.Integer(vOut[2].Data);                             // 0: Desconectada. 1: Conectada
                int can1 = snmpc.Integer(vOut[3].Data);                             // 0: Desconectada. 1: Conectada
                int can2 = snmpc.Integer(vOut[4].Data);                             // 0: Desconectada. 1: Conectada
                int can3 = snmpc.Integer(vOut[5].Data);                             // 0: Desconectada. 1: Conectada

                int std = (can0 << 1) | (can1 << 2) | (can2 << 3) | (can3 << 4);

                SlotTypeSet(gw, nslot, gw.slots[nslot], stipo, std);
                SlotStateSet(gw, nslot, gw.slots[nslot], std);

                for (int rec = 0; rec < 4; rec++)
                {
                    if (slot.rec[rec].presente == true)
                    {
                        ExploraRecurso_unificada(new KeyValuePair<stdPhGw, int>(gw, nslot * 4 + rec));
                    }
                    else
                    {
                        Reset_ExploraRecurso(gw, nslot, rec);
                    }
                }
            }

            catch (Exception x)
            {
                LogException<GwExplorer>(String.Format(" Explorando Slot. CGW {0}.{1}",
                    obj == null ? "null" : ((KeyValuePair<stdPhGw, int>)obj).Key.ip,
                    obj == null ? "null" : ((KeyValuePair<stdPhGw, int>)obj).Value.ToString()), x);
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="obj"></param>
        protected void ExploraRecurso_unificada(object obj)
        {
            KeyValuePair<stdPhGw, int> objIn = (KeyValuePair<stdPhGw, int>)obj;
            stdPhGw gw = objIn.Key;
            int nres = objIn.Value;
            int nslot = nres / 4;
            int ires = nres % 4;
            stdRec rec = gw.slots[nslot].rec[ires];
            IPEndPoint gwep = new IPEndPoint(IPAddress.Parse(gw.ip), gw.snmpport);
            OctetString community = new OctetString("public");

            if (gw.name == "" && nslot == 0 && ires == 1)
                LogTrace<GwExplorer>(String.Format("Presencia (1) Slot 0, Recurso 1: {0}", gw.slots[0].rec[1].presente));

            try
            {
                string oidbase = ".1.3.6.1.4.1.7916.8.3.1.4.2.1.";
                List<Variable> vIn = new List<Variable>()
                {
                    new Variable(new ObjectIdentifier(oidbase+"3."+(nres+1).ToString())),   // Tipo
                    new Variable(new ObjectIdentifier(oidbase+"6."+(nres+1).ToString())),   // Status Hardware,
                    new Variable(new ObjectIdentifier(oidbase+"15."+(nres+1).ToString())),  // Status Interfaz.
                };
                SnmpClient snmpc = new SnmpClient();

                IList<Variable> vOut = snmpc.Get(VersionCode.V2, gwep, community, vIn, gw.SnmpTimeout, gw.SnmpReintentos);

                int ntipo = snmpc.Integer(vOut[0].Data);   // 0: RD, 1: LC, 2: BC, 3: BL, 4: AB, 5: R2, 6: N5, 7: QS, 9: NP, 13: PPEM 
                if (ntipo == 9)
                {
                    // 20170630. El código 9 no es no presente sino NO CONFIGURADO
                    // Reset_ExploraRecurso(gw, nslot, ires);
                    // rec.presente = false;
                    rec.tipo_itf = itf.rcNotipo;
                    rec.tipo_online = trc.rcNotipo;
                    rec.std_online = std.NoInfo;
                }
                else if ((ntipo >= 0 && ntipo < 9) || ntipo == 13)
                {
                    int TipoNotificado = ntipo == 0 ? RadioResource_AgentType :
                        ntipo == 1 ? IntercommResource_AgentType :
                        (ntipo < 5 || ntipo == 13) ? LegacyPhoneResource_AgentType : ATSPhoneResource_AgentType;

                    SlotRecursoTipoAgenteSet(gw, rec, TipoNotificado);
                    /*
                            rcRadio = 0, 
                            rcLCE = 1, 
                            rcPpBC = 2, 
                            rcPpBL = 3, 
                            rcPpAB = 4, 
                            rcAtsR2 = 5, 
                            rcAtsN5 = 6, 
                            rcPpEM = 13, 
                            rcPpEMM = 51, 
                            rcNotipo = -1 
                     * */
                    SlotRecursoTipoInterfazSet(gw, rec, ntipo);

                    int estado = snmpc.Integer(vOut[2].Data);   // 0: NP, 1: OK, 2: Fallo, 3: Degradado
                    SlotRecursoEstadoSet(gw, rec, estado, (trc)TipoNotificado);
                }
                else if (ntipo != 9 && ntipo != -1)
                {
                    LogWarn<GwExplorer>(String.Format("Error Explorando Recurso {0}:{1}: Tipo Notificado <{2}> Erroneo.",
                                gw.ip, nres, ntipo));
                }
            }
            catch (Exception x)
            {
                LogException<GwExplorer>(String.Format(" Explorando recurso en {0}: Rec:{1}-{2}", gw.ip, nres, rec.name), x);
            }
        }

        /** 20200813. Version para solo generar un GET */
        List<Variable> vInAll = new List<Variable>()
        {
            new Variable(new ObjectIdentifier(".1.3.6.1.4.1.7916.8.3.1.1.2.0")),    // 0 => Estado Hw.
            new Variable(new ObjectIdentifier(".1.3.6.1.4.1.7916.8.3.1.1.6.0")),    // 1 => Estado LAN1
            new Variable(new ObjectIdentifier(".1.3.6.1.4.1.7916.8.3.1.1.7.0")),    // 2 => Estado LAN2
            new Variable(new ObjectIdentifier(".1.3.6.1.4.1.7916.8.3.1.1.8.0")),    // 3 => Estado P/R,
            new Variable(new ObjectIdentifier(".1.3.6.1.4.1.7916.8.3.1.1.4.0")),    // 4 => Estado FA,
            new Variable(new ObjectIdentifier(".1.3.6.1.4.1.7916.8.3.1.1.1.0")),    // 5 => Identificador. Habilita el envio de TRAPS
                                                                                    // 6 => Slot 0
            new Variable(new ObjectIdentifier(".1.3.6.1.4.1.7916.8.3.1.3.2.1.2.1")),   // Tipo. 0: Error, 1: IA4, 2: IQ1
            new Variable(new ObjectIdentifier(".1.3.6.1.4.1.7916.8.3.1.3.2.1.3.1")),   // Status,
            new Variable(new ObjectIdentifier(".1.3.6.1.4.1.7916.8.3.1.3.2.1.4.1")),   // Canal-0
            new Variable(new ObjectIdentifier(".1.3.6.1.4.1.7916.8.3.1.3.2.1.5.1")),   // Canal-1
            new Variable(new ObjectIdentifier(".1.3.6.1.4.1.7916.8.3.1.3.2.1.6.1")),   // Canal-2
            new Variable(new ObjectIdentifier(".1.3.6.1.4.1.7916.8.3.1.3.2.1.7.1")),   // Canal-3
                                                                                    // 12 => Slot 1
            new Variable(new ObjectIdentifier(".1.3.6.1.4.1.7916.8.3.1.3.2.1.2.2")),   // Tipo. 0: Error, 1: IA4, 2: IQ1
            new Variable(new ObjectIdentifier(".1.3.6.1.4.1.7916.8.3.1.3.2.1.3.2")),   // Status,
            new Variable(new ObjectIdentifier(".1.3.6.1.4.1.7916.8.3.1.3.2.1.4.2")),   // Canal-0
            new Variable(new ObjectIdentifier(".1.3.6.1.4.1.7916.8.3.1.3.2.1.5.2")),   // Canal-1
            new Variable(new ObjectIdentifier(".1.3.6.1.4.1.7916.8.3.1.3.2.1.6.2")),   // Canal-2
            new Variable(new ObjectIdentifier(".1.3.6.1.4.1.7916.8.3.1.3.2.1.7.2")),   // Canal-3
                                                                                    // 18 => Slot 2
            new Variable(new ObjectIdentifier(".1.3.6.1.4.1.7916.8.3.1.3.2.1.2.3")),   // Tipo. 0: Error, 1: IA4, 2: IQ1
            new Variable(new ObjectIdentifier(".1.3.6.1.4.1.7916.8.3.1.3.2.1.3.3")),   // Status,
            new Variable(new ObjectIdentifier(".1.3.6.1.4.1.7916.8.3.1.3.2.1.4.3")),   // Canal-0
            new Variable(new ObjectIdentifier(".1.3.6.1.4.1.7916.8.3.1.3.2.1.5.3")),   // Canal-1
            new Variable(new ObjectIdentifier(".1.3.6.1.4.1.7916.8.3.1.3.2.1.6.3")),   // Canal-2
            new Variable(new ObjectIdentifier(".1.3.6.1.4.1.7916.8.3.1.3.2.1.7.3")),   // Canal-3
                                                                                    // 24 => Slot 3
            new Variable(new ObjectIdentifier(".1.3.6.1.4.1.7916.8.3.1.3.2.1.2.4")),   // Tipo. 0: Error, 1: IA4, 2: IQ1
            new Variable(new ObjectIdentifier(".1.3.6.1.4.1.7916.8.3.1.3.2.1.3.4")),   // Status,
            new Variable(new ObjectIdentifier(".1.3.6.1.4.1.7916.8.3.1.3.2.1.4.4")),   // Canal-0
            new Variable(new ObjectIdentifier(".1.3.6.1.4.1.7916.8.3.1.3.2.1.5.4")),   // Canal-1
            new Variable(new ObjectIdentifier(".1.3.6.1.4.1.7916.8.3.1.3.2.1.6.4")),   // Canal-2
            new Variable(new ObjectIdentifier(".1.3.6.1.4.1.7916.8.3.1.3.2.1.7.4")),   // Canal-3
                                                                                    // 30 => Recurso 0
            new Variable(new ObjectIdentifier(".1.3.6.1.4.1.7916.8.3.1.4.2.1.3.1")),   // Tipo
            new Variable(new ObjectIdentifier(".1.3.6.1.4.1.7916.8.3.1.4.2.1.15.1")),  // Status Interfaz.
                                                                                    // 32 => Recurso 1
            new Variable(new ObjectIdentifier(".1.3.6.1.4.1.7916.8.3.1.4.2.1.3.2")),   // Tipo
            new Variable(new ObjectIdentifier(".1.3.6.1.4.1.7916.8.3.1.4.2.1.15.2")),  // Status Interfaz.
                                                                                    // 34 => Recurso 2
            new Variable(new ObjectIdentifier(".1.3.6.1.4.1.7916.8.3.1.4.2.1.3.3")),   // Tipo
            new Variable(new ObjectIdentifier(".1.3.6.1.4.1.7916.8.3.1.4.2.1.15.3")),  // Status Interfaz.
                                                                                    // 36 => Recurso 3
            new Variable(new ObjectIdentifier(".1.3.6.1.4.1.7916.8.3.1.4.2.1.3.4")),   // Tipo
            new Variable(new ObjectIdentifier(".1.3.6.1.4.1.7916.8.3.1.4.2.1.15.4")),  // Status Interfaz.
                                                                                    // 38 => Recurso 4
            new Variable(new ObjectIdentifier(".1.3.6.1.4.1.7916.8.3.1.4.2.1.3.5")),   // Tipo
            new Variable(new ObjectIdentifier(".1.3.6.1.4.1.7916.8.3.1.4.2.1.15.5")),  // Status Interfaz.
                                                                                    // 40 => Recurso 5
            new Variable(new ObjectIdentifier(".1.3.6.1.4.1.7916.8.3.1.4.2.1.3.6")),   // Tipo
            new Variable(new ObjectIdentifier(".1.3.6.1.4.1.7916.8.3.1.4.2.1.15.6")),  // Status Interfaz.
                                                                                    // 42 => Recurso 6
            new Variable(new ObjectIdentifier(".1.3.6.1.4.1.7916.8.3.1.4.2.1.3.7")),   // Tipo
            new Variable(new ObjectIdentifier(".1.3.6.1.4.1.7916.8.3.1.4.2.1.15.7")),  // Status Interfaz.
                                                                                    // 44 => Recurso 7
            new Variable(new ObjectIdentifier(".1.3.6.1.4.1.7916.8.3.1.4.2.1.3.8")),   // Tipo
            new Variable(new ObjectIdentifier(".1.3.6.1.4.1.7916.8.3.1.4.2.1.15.8")),  // Status Interfaz.
                                                                                    // 46 => Recurso 8
            new Variable(new ObjectIdentifier(".1.3.6.1.4.1.7916.8.3.1.4.2.1.3.9")),   // Tipo
            new Variable(new ObjectIdentifier(".1.3.6.1.4.1.7916.8.3.1.4.2.1.15.9")),  // Status Interfaz.
                                                                                    // 48 => Recurso 9
            new Variable(new ObjectIdentifier(".1.3.6.1.4.1.7916.8.3.1.4.2.1.3.10")),   // Tipo
            new Variable(new ObjectIdentifier(".1.3.6.1.4.1.7916.8.3.1.4.2.1.15.10")),  // Status Interfaz.
                                                                                    // 50 => Recurso 10
            new Variable(new ObjectIdentifier(".1.3.6.1.4.1.7916.8.3.1.4.2.1.3.11")),   // Tipo
            new Variable(new ObjectIdentifier(".1.3.6.1.4.1.7916.8.3.1.4.2.1.15.11")),  // Status Interfaz.
                                                                                    // 52 => Recurso 11
            new Variable(new ObjectIdentifier(".1.3.6.1.4.1.7916.8.3.1.4.2.1.3.12")),   // Tipo
            new Variable(new ObjectIdentifier(".1.3.6.1.4.1.7916.8.3.1.4.2.1.15.12")),  // Status Interfaz.
                                                                                    // 54 => Recurso 12
            new Variable(new ObjectIdentifier(".1.3.6.1.4.1.7916.8.3.1.4.2.1.3.13")),   // Tipo
            new Variable(new ObjectIdentifier(".1.3.6.1.4.1.7916.8.3.1.4.2.1.15.13")),  // Status Interfaz.
                                                                                    // 56 => Recurso 13
            new Variable(new ObjectIdentifier(".1.3.6.1.4.1.7916.8.3.1.4.2.1.3.14")),   // Tipo
            new Variable(new ObjectIdentifier(".1.3.6.1.4.1.7916.8.3.1.4.2.1.15.14")),  // Status Interfaz.
                                                                                    // 58 => Recurso 14
            new Variable(new ObjectIdentifier(".1.3.6.1.4.1.7916.8.3.1.4.2.1.3.15")),   // Tipo
            new Variable(new ObjectIdentifier(".1.3.6.1.4.1.7916.8.3.1.4.2.1.15.15")),  // Status Interfaz.
                                                                                    // 60 => Recurso 15
            new Variable(new ObjectIdentifier(".1.3.6.1.4.1.7916.8.3.1.4.2.1.3.16")),   // Tipo
            new Variable(new ObjectIdentifier(".1.3.6.1.4.1.7916.8.3.1.4.2.1.15.16")),  // Status Interfaz.
        };
        protected void ExploreEverythingAtOnce(stdPhGw pgw)
        {
            IPEndPoint gwep = new IPEndPoint(IPAddress.Parse(pgw.ip), pgw.snmpport);
            OctetString community = new OctetString("public");
            SnmpClient snmpc = new SnmpClient();

            IList<Variable> vOut = snmpc.Get(VersionCode.V2, gwep, community, vInAll, pgw.SnmpTimeout, pgw.SnmpReintentos);

            // Análisis de Parámetros Generales.
            // estadoGeneral. 0: No Inicializado, 1: Ok, 2: Fallo, 3: Aviso.
            int stdGeneral = snmpc.Integer(vOut[0].Data);
            // stdLAN1. 0: No Presente, 1: Ok, 2: Error.
            int stdLan1 = snmpc.Integer(vOut[1].Data);
            // stdLAN2. 0: No Presente, 1: Ok, 2: Error.
            int stdLan2 = snmpc.Integer(vOut[2].Data);
            // stdCpuLocal. 0: No Presente. 1: Principal, 2: Reserva, 3: Arrancando
            int stdPR = snmpc.Integer(vOut[3].Data);
            // stdFA. 0: No Presente. 1: Ok, 2: Error
            int stdFA = snmpc.Integer(vOut[4].Data);
            pgw.std = stdGeneral == 0 ? std.NoInfo : stdGeneral == 1 ? std.Ok : std.Error;

            int stdLan = (stdLan1 == 1 ? 0x01 : 0x00) | (stdLan2 == 1 ? 0x02 : 0x00);
            PhGwLanStatusSet(pgw, (0x04 | stdLan));                 // En este tipo de Pasarelas BOND configurado...

            PhGwPrincipalReservaSet(pgw, stdPR == 1 ? 1 : 0);       // Solo se marca PPAL si está en PPAL en cualquier otro caso se marca RSVA

            pgw.stdFA = stdFA == 0 ? std.NoInfo : stdFA == 1 ? std.Ok : stdFA == 2 ? std.Error : std.NoExiste;

            // Análisis de Slots
            for (int slot = 0; slot<4; slot++)
            {
                int ibase = 6 + slot * 6;
                int stipo = snmpc.Integer(vOut[ibase+0].Data);                            // 0: Error, 1: IA4, 2: IQ1
                int status = snmpc.Integer(vOut[ibase+1].Data);                           // 0: No presente, 1: Presente

                stipo = status == 0 ? 0 : (stipo == 1 ? 2 : 0);

                int can0 = snmpc.Integer(vOut[ibase + 2].Data);                             // 0: Desconectada. 1: Conectada
                int can1 = snmpc.Integer(vOut[ibase + 3].Data);                             // 0: Desconectada. 1: Conectada
                int can2 = snmpc.Integer(vOut[ibase + 4].Data);                             // 0: Desconectada. 1: Conectada
                int can3 = snmpc.Integer(vOut[ibase + 5].Data);                             // 0: Desconectada. 1: Conectada

                int std = (can0 << 1) | (can1 << 2) | (can2 << 3) | (can3 << 4);

                SlotTypeSet(pgw, slot, pgw.slots[slot], stipo, std);
                SlotStateSet(pgw, slot, pgw.slots[slot], std);
            }

            // Análisis de Recursos.
            for (int nres=0; nres<16; nres++)
            {
                int ibase = 30 + nres * 2;
                int nslot = nres / 4;
                int ires = nres % 4;
                stdRec rec = pgw.slots[nslot].rec[ires];
                int ntipo = snmpc.Integer(vOut[ibase+0].Data);   // 0: RD, 1: LC, 2: BC, 3: BL, 4: AB, 5: R2, 6: N5, 7: QS, 9: NP, 13: PPEM 
                if (ntipo == 9)
                {
                    // 20170630. El código 9 no es no presente sino NO CONFIGURADO
                    // Reset_ExploraRecurso(gw, nslot, ires);
                    // rec.presente = false;
                    rec.tipo_itf = itf.rcNotipo;
                    rec.tipo_online = trc.rcNotipo;
                    rec.std_online = std.NoInfo;
                }
                else if ((ntipo >= 0 && ntipo < 9) || ntipo == 13)
                {
                    int TipoNotificado = ntipo == 0 ? RadioResource_AgentType :
                        ntipo == 1 ? IntercommResource_AgentType :
                        (ntipo < 5 || ntipo == 13) ? LegacyPhoneResource_AgentType : ATSPhoneResource_AgentType;

                    SlotRecursoTipoAgenteSet(pgw, rec, TipoNotificado);
                    /*
                            rcRadio = 0, 
                            rcLCE = 1, 
                            rcPpBC = 2, 
                            rcPpBL = 3, 
                            rcPpAB = 4, 
                            rcAtsR2 = 5, 
                            rcAtsN5 = 6, 
                            rcPpEM = 13, 
                            rcPpEMM = 51, 
                            rcNotipo = -1 
                     * */
                    SlotRecursoTipoInterfazSet(pgw, rec, ntipo);

                    int estado = snmpc.Integer(vOut[ibase+1].Data);   // 0: NP, 1: OK, 2: Fallo, 3: Degradado
                    SlotRecursoEstadoSet(pgw, rec, estado, (trc)TipoNotificado);
                }
                else if (ntipo != 9 && ntipo != -1)
                {
                    LogWarn<GwExplorer>(String.Format("Error Explorando Recurso {0}:{1}: Tipo Notificado <{2}> Erroneo.",
                                pgw.ip, nres, ntipo));
                }
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="gw"></param>
        /// <param name="nslot"></param>
        /// <param name="rec"></param>
        protected void Reset_ExploraRecurso(stdPhGw gw, int nslot, int rec)
        {
            SlotRecursoTipoAgenteSet(gw, gw.slots[nslot].rec[rec], (int)trc.rcNotipo);
            SlotRecursoTipoInterfazSet(gw, gw.slots[nslot].rec[rec], (int)itf.rcNotipo);
            SlotRecursoEstadoSet(gw, gw.slots[nslot].rec[rec], (int)std.NoInfo, trc.rcNotipo);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pgw"></param>
        protected async void GetVersion_unificada(stdPhGw pgw)
        {
            if (pgw.version == string.Empty || pgw.version == idiomas.strings.GWS_VersionError/*"Error en GetVersion_unificada"*/)
            {
                try
                {
                    string page = "http://" + pgw.ip + ":8080/mant/lver";

                    // ... Use HttpClient.
                    using (HttpClient client = new HttpClient())
                    using (HttpResponseMessage response = await client.GetAsync(page))
                    using (HttpContent content = response.Content)
                    {
                        // ... Read the string.
                        string result = await content.ReadAsStringAsync();
                        pgw.version = result;
                    }
                }
                catch (Exception x)
                {
                    if (pgw.version != idiomas.strings.GWS_VersionError/*"Error en GetVersion_unificada"*/)
                        LogException<GwExplorer>(String.Format(" GW:{0},{1}", pgw.name, pgw.ip), x);
                    pgw.version = idiomas.strings.GWS_VersionError/*"Error en GetVersion_unificada"*/;
                }
            }
        }
        protected async void GetNtpStatus(stdGw gw)
        {
            try
            {
                string page = "http://" + gw.ip + ":8080/ntpstatus";
                // ... Use HttpClient.
                using (HttpClient client = new HttpClient())
                using (HttpResponseMessage response = await client.GetAsync(page))
                using (HttpContent content = response.Content)
                {
                    // ... Read the string.
                    string result = await content.ReadAsStringAsync();
                    gw.ntp_client_status = (U5kManWebAppData.JDeserialize<stdGw.RemoteNtpClientStatus>(result)).lines;
                }
            }
            catch (Exception x)
            {
                LogException<GwExplorer>(String.Format(" GW:{0},{1}", gw.name, gw.ip), x);
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="gw"></param>
        /// <param name="pgw"></param>
        /// <param name="oidEnt"></param>
        /// <param name="oidvar"></param>
        static public void RecibidoTrapGw_unificada(stdGw gw, stdPhGw pgw, string oidEnt, string oidvar, ISnmpData data)
        {
            switch (oidEnt)
            {
                case ".1.3.6.1.4.1.7916.8.3.2.1.1":         // Cambio de Configuracion.
                    break;

                case ".1.3.6.1.4.1.7916.8.3.2.1.2":         // Cambio de Estado.
                    break;

                case ".1.3.6.1.4.1.7916.8.3.2.1.3":         // Se genera cuando cambia un parametro del grupo tarjeta
                    break;

                case ".1.3.6.1.4.1.7916.8.3.2.1.4":         // Se genera cuando cambia parametro del grupo interfaz
                    break;

                case ".1.3.6.1.4.1.7916.8.3.2.1.5":         // Evento de Historicos.
                    if (oidvar == ".1.3.6.1.4.1.7916.8.3.2.1.7.0")
                    {
                        LogTrace<GwExplorer>(String.Format("GWU-HISTORICO: <<<{0}>>>", data.ToString()));

                        Redan2UlisesHist conv = new Redan2UlisesHist(data.ToString());
                        U5kIncidencia inci;
                        List<Object> parametros;
                        if (conv.UlisesInci(out inci, out parametros))
                        {
                            RecordEvent<GwExplorer>(DateTime.Now, (eIncidencias)inci.id, (eTiposInci)inci.tipo, inci.idhw, parametros.ToArray());
                        }
                        else
                        {
                            LogWarn<GwExplorer>(String.Format("GWU-HISTORICO NO CONVERTIDO: <<<{0}>>>", data.ToString()));
                        }
                    }
                    break;

                default:
                    LogWarn<GwExplorer>(String.Format("Recibido TRAP-GW OID-Desconocida de {0}, OID={1}", gw?.ip, oidEnt));
                    break;
            }
        }

#endregion //

#region GW_STD_V1
        /// <summary>
        /// 
        /// </summary>
        /// <param name="obj"></param>
        protected void ExplorePhGw(object obj)
        {
            // Obtengo una copia del estado de la pasarela.
            stdPhGw phgw = new stdPhGw((stdPhGw)obj);

            try
            {
                // Ping para la conectividad....
                var resPing = U5kGenericos.Ping(phgw.ip, phgw.presente);
                if (phgw.IpConn.ProcessResult(resPing))
                {
                    phgw.IpConn.Std = resPing ? std.Ok : std.NoInfo;
                    LogTrace<GwExplorer>($"GwPing {(resPing ? "Ok  " : "Fail")} executed: {phgw.name}.");
                    if (phgw.IpConn.Std == std.Ok)
                    {
                        // Supervision del Modulo SIP...
                        SipModuleTest(phgw, (res, newStd) =>
                         {
                             if (phgw.SipMod.ProcessResult(res) == true)
                             {
                                 phgw.SipMod.Std = newStd;
                                 LogTrace<GwExplorer>($"GwSip_ {(res ? "Ok  " : "Fail")} executed: {phgw.name}.");
                             }
                             else
                             {
                                 LogInfo<GwExplorer>($"GwSip_ Fail ignored : {phgw.name}.");
                             }
                         });

                        if (phgw.SipMod.Std == std.Ok)
                        {
                            // Supervision del Modulo de Configuracion...
                            CfgModuleTest(phgw, (res, newStd, mensaje) =>
                            {
                                if (phgw.CfgMod.ProcessResult(res) == true)
                                {
                                    phgw.CfgMod.Std = newStd;
                                    if (res)
                                        LogTrace<GwExplorer>($"GwCfg_ Ok  executed: {phgw.name}.");
                                    else
                                    {
                                        LogTrace<GwExplorer>($"GwCfg_ Fail EXECUTED: {phgw.name}\n   <{mensaje}>.");
                                    }
                                }
                                else
                                {
                                    LogTrace<GwExplorer>($"GwCfg_ Fail IGNORED: {phgw.name}\n   <{mensaje}>.");
                                }
                            });

                            // Supervision del Modulo SNMP....
                            SnmpModuleExplore(phgw, (res) =>
                            {
                                if (phgw.SnmpMod.ProcessResult(res) == true)
                                {
                                    if (res == true)
                                    {
                                        phgw.SnmpMod.Std = std.Ok;
                                    }
                                    else
                                    {
                                        phgw.SnmpMod.Std = std.NoInfo;
                                        phgw.SnmpDataReset();
                                    }
                                    LogTrace<GwExplorer>($"GwSnmp {(res ? "Ok  " : "Fail")} executed: {phgw.name}.");
                                }
                                else
                                {
                                    LogInfo<GwExplorer>($"GwSnmp Fail ignored : {phgw.name}.");
                                }
                            });
                        }
                        else
                        {
                            // No se ha Respondido al SIP...
                        }
                    }
                    else
                    {
                        // No se ha respondido al PING...
                    }
                }
                else
                {
                    LogInfo<GwExplorer>($"GwPing Fail ignored : {phgw.name}.");
                }
            }
            catch (Exception x)
            {
                LogException<GwExplorer>(phgw.name, x);
                if (phgw.IpConn.ProcessResult(false) == true)
                {
                    phgw.IpConn.Std = std.NoInfo;
                }
            }
            finally
            {
                ConsolidateData((stdPhGw)obj, phgw);
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="phgw"></param>
        protected void SipModuleTest(stdPhGw phgw, Action<bool, std> response)
        {
            try
            {
                int timeout = Properties.u5kManServer.Default.SipOptionsTimeout;
                SipUA locale_ua = new SipUA() { user = "MTTO", ip = Properties.u5kManServer.Default.MiDireccionIP, port = 0 };
                SipUA remote_ua = new SipUA() { user = phgw.name, ip = phgw.ip, port = 5060, radio = true };
                SipSupervisor sips = new SipSupervisor(locale_ua, timeout);
                if (sips.SipPing(remote_ua))
                {
                    if (remote_ua.last_response == null || remote_ua.last_response.Result == "Error")
                    {
                        response(false, std.NoInfo);
                    }
                    else
                    {
                        response(true, (remote_ua.last_response.Result == "200" || remote_ua.last_response.Result == "503") ? std.Ok : std.Error);
                    }
                }
                else
                {
                    /** No Responde */
                    response(false, std.NoInfo);
                }
            }
            catch (Exception x)
            {
                // Error en los OPTIONS...
                LogException<GwExplorer>(phgw.name, x);
                response(false, std.NoInfo);
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="phgw"></param>
        protected void CfgModuleTest(stdPhGw phgw, Action<bool, std, string> response)
        {
            std stdRes = std.NoInfo;
            var mensaje = "";
            try
            {
                string page = "http://" + phgw.ip + ":8080/test";
                var timeout = TimeSpan.FromMilliseconds(Properties.u5kManServer.Default.HttpGetTimeout);
                HttpHelper.GetSync(page, timeout, (success, message) =>
                 {
                     if (success)
                     {
                         stdRes  = message.Contains("Handler por Defecto") ? std.Ok : std.Error;
                     }
                     else
                     {
                         stdRes = std.NoInfo;
                         mensaje = message;
                     }
                 });


                //var resp = HttpHelper.Get(page, timeout, null);
                //stdRes = resp == null ? std.NoInfo : resp.Contains("Handler por Defecto") ? std.Ok : std.Error;

                /** Obtiene la version unificada */
                if (stdRes == std.Ok && (phgw.version == string.Empty || phgw.version == idiomas.strings.GWS_VersionError))
                {
                    var resp = HttpHelper.GetSync(phgw.ip, "8080", "/mant/lver", timeout);
                    phgw.version = resp ?? idiomas.strings.GWS_VersionError;
                }
            }
            catch (Exception x)
            {
                // Error en Modulo de Configuracion Local...
                stdRes = std.NoInfo;
                LogException<GwExplorer>(phgw.name, x);
            }
            finally
            {
                //GetVersion_unificada(phgw);
            }
            response(stdRes != std.NoInfo, stdRes, mensaje);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="phgw"></param>
        protected void SnmpModuleExplore(stdPhGw phgw, Action<bool> response)
        {
            try
            {
#if !_EXPLORE_ALL_AT_ONCE_
                ExploraGwStdGen_unificada(phgw);
                for (int slot = 0; slot < 4; slot++)
                {
                    ExploraSlot_unificada(new KeyValuePair<stdPhGw, int>(phgw, slot));
                }
#else
#if DEBUG
                var itm = new TimeMeasurement();
#endif
                ExploreEverythingAtOnce(phgw);
#if DEBUG
                itm.StopAndPrint((msg) => LogTrace<GwExplorer>(msg));
#endif
#endif
                response(true);
            }
            catch (Exception x)
            {
                // Error en la Exploracion SNMP....
                response(false);
                LogException<GwExplorer>(phgw.name, x);
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="last"></param>
        /// <param name="current"></param>
        protected void ConsolidateData(stdPhGw last, stdPhGw current)
        {
            try
            {
                // Calcular Presente....
                current.presente = (current.IpConn.Std == std.Ok && current.SipMod.Std != std.NoInfo);
                // Calcular Estado General.... En current.std se encuentra el estado leido. last.std debe tener tambien los errores de recurso...
                current.std = current.presente == false ? std.NoInfo : current.Errores == true ? std.Error : std.Ok;

                // Historicos de Activacion / Desactivacion de Modulos...
                if (current.CfgMod.Std != last.CfgMod.Std)
                {
                    eIncidencias inci = current.CfgMod.Std == std.NoInfo ? eIncidencias.IGW_CAIDA : eIncidencias.IGW_ENTRADA;
                    RecordEvent<GwExplorer>(DateTime.Now, inci, eTiposInci.TEH_TIFX, current.name, Params(idiomas.strings.GW_CFGL_MODULE));
                }
                if (current.SipMod.Std != last.SipMod.Std)
                {
                    eIncidencias inci = current.SipMod.Std == std.NoInfo ? eIncidencias.IGW_CAIDA : eIncidencias.IGW_ENTRADA;
                    RecordEvent<GwExplorer>(DateTime.Now, inci, eTiposInci.TEH_TIFX, current.name, Params(idiomas.strings.GW_SIP_MODULE));
                }
                if (current.SnmpMod.Std != last.SnmpMod.Std)
                {
                    eIncidencias inci = current.SnmpMod.Std == std.NoInfo ? eIncidencias.IGW_CAIDA : eIncidencias.IGW_ENTRADA;
                    RecordEvent<GwExplorer>(DateTime.Now, inci, eTiposInci.TEH_TIFX, current.name, Params(idiomas.strings.GW_SNMP_MODULE));
                }

                // Genera historicos de activacion / desactivacion de la pasarela...
                if (current.presente != last.presente)
                {
                    eIncidencias inci = current.presente == false ? eIncidencias.IGW_CAIDA : eIncidencias.IGW_ENTRADA;
                    RecordEvent<GwExplorer>(DateTime.Now, inci, eTiposInci.TEH_TIFX, current.name, Params(idiomas.strings.GW_GLOBAL_MODULE));

                    if (current.presente == false)
                    {
                        /** 20200811 Reset de los estados de Módulos */
                        current.SipMod.Std = std.NoInfo;
                        current.CfgMod.Std = std.NoInfo;
                        current.SipMod.Std = std.NoInfo;

                        /** Reset Estado GW fisica */
                        GwHelper.SetToOutOfOrder(current);
                    }
                }
            }
            catch (Exception x)
            {
                // Error en la consolidacion.
                LogException<GwExplorer>(last.name, x);
            }
            finally
            {
                last.CopyFrom(current);
            }
        }


#endregion

        /// <summary>
        /// 
        /// </summary>
        /// <param name="gw"></param>
        static protected void GwTrace(stdGw gw)
        {
            NLog.LogLevel level = NLog.LogLevel.Info;

            Log<GwExplorer>(level, String.Format("Name={0}, IP={1}, Presente={2}, Estado={3}", gw.name, gw.ip, gw.presente, gw.std), eIncidencias.IGNORE);
            // String slots = String.Format("{0},[{1}{2}{3}{4}] 
        }

#endregion
    }   // clase

    /** */


} // namespace.
