/** */
angular.module("Uv5kiman")
    .controller("uv5kiOpeSpCtrl", function ($scope, $interval, $serv, $lserv) {
        var session_stdcodes = { Desconectado: 0, Conectado: 1, Deshabilitado: 2 };
        var session_types = { RX: "Rx", TX: "Tx", RXTX: "RxTx" };
        var equ_types = { Main: 0, Reserva: 1 };
        var led_std = { Off: 0, On1: 1, On2: 2 };
        var frec_stdcodes = { NoDisponible: 0, Disponible: 1, Degradada: 2 };
        var frec_tipos = { Normal: 0, UnoMasUno: 1, FD: 2, EM: 3 };
        var frec_prio = { Normal: 0, Emergencia: 1 };
        var frec_cclimax = { Realtivo: 0, Absoluto: 1 };

        /** Inicializacion */
        var ctrl = this;

        /** Para borrar */
        /** Flags de Presentacion */
        ctrl.info_telefonia = false;
        ctrl.control_mnm = false;

        /** Tablas de Gestion de Radio */
        // Sesiones
        ctrl.sessions = [];
        // M+N
        ctrl.gestormn = [];
        /** HF */
        ctrl.gestorhf = [];

        ctrl.frecs = [];
        ctrl.mnman = [];
        ctrl.site_select = null;
        ctrl.vhf_mode_select = -1;
        ctrl.uhf_mode_select = -1;

        ctrl.pagina = function (pagina) {
            var menu = $lserv.Submenu(pagina);
            return menu ? menu : 0;
        };


        /** Servicios Pagina de Sesiones*/
        //** Version 1 */ 
        ctrl.colorEstadoFrecuencia = function (std) {
            return (std == frec_stdcodes.NoDisponible ? "bg-warning text-danger" :
                std == frec_stdcodes.Disponible ? "text-info" :
                    std == frec_stdcodes.Degradada ? "bg-warning text-info" : "text-danger bg-danger");
        };
        ctrl.textTipoPrio = function (tp, pr) {
            var txtTipo = tp == frec_tipos.Normal ? $lserv.translate("Simple") :
                tp == frec_tipos.UnoMasUno ? $lserv.translate("Dual") :
                    tp == frec_tipos.FD ? $lserv.translate("FD") :
                        tp == frec_tipos.EM ? $lserv.translate("ME") : "ERR";
            var txtPrio = pr == frec_prio.Normal ? $lserv.translate("Normal") :
                pr == frec_prio.Emergencia ? $lserv.translate("Emergencia") : $lserv.translate("Error");
            return txtTipo + "/" + txtPrio;
        };
        ctrl.colorEstadoSesion = function (std) {
            return std == session_stdcodes.Desconectado ? "bg-warning text-danger" :
                std == session_stdcodes.Conectado ? "text-info" :
                    std == session_stdcodes.Deshabilitado ? "text-muted" : "text-danger bg-danger";
        };
        ctrl.txtCClimax = function (md) {
            return md == frec_cclimax.Absoluto ? "A" :
                md == frec_cclimax.Realtivo ? "R" : "?";
        };
        ctrl.enableOnFD = function (tp) {
            return tp == frec_tipos.FD ? true : false;
        };
        ctrl.showOnTx = function (tp) {
            return (tp == session_types.TX || tp == session_types.RXTX);
        };
        ctrl.showOnRx = function (tp) {
            return (tp == session_types.RX || tp == session_types.RXTX);
        };
        ctrl.txtPestana = function () {
            return $lserv.translate("Frecuencias");
        };
        ctrl.txtFrecAndType = function (fr, tp) {
            var txtTipo = tp == frec_tipos.Normal ? $lserv.translate("Normal") :
                tp == frec_tipos.UnoMasUno ? $lserv.translate("1+1") :
                    tp == frec_tipos.FD ? $lserv.translate("FD") :
                        tp == frec_tipos.EM ? $lserv.translate("ME") : "ERR";
            if (fr.length == 0)
                return fr;
            return (fr + " (" + txtTipo + ")");
        };
        ctrl.RxSelected = function (item) {
            switch (item.ftipo) {
                case frec_tipos.UnoMasUno:
                case frec_tipos.EM:
                    return "????-" + item.ftipo.toString();
                case frec_tipos.Normal:
                case frec_tipos.FD:
                    break;
                default:
                    return "????";
            }
            if (item.selected_site == "")
                return "";
            return item.selected_site + "/" + item.selected_site_qidx.toString();
        };
        ctrl.TxSelected = function (item) {
            switch (item.ftipo) {
                case frec_tipos.UnoMasUno:
                case frec_tipos.EM:
                    return "????-" + item.ftipo.toString();
                case frec_tipos.Normal:
                    return "";
                case frec_tipos.FD:
                    break;
                default:
                    return "????";
            }
            return item.selected_tx.toString();
        };
        /** */
        var SesParCollapseStatus = [];
        ctrl.saveSesParCollapseStatus = function (ses, id, ifrec, ises) {
            var iddiv = '#' + id + "-" + ifrec + "-" + ises;
            var isExpanded = $(iddiv).hasClass('in') ? false : true;

            console.log(ses + ": " + iddiv + ": " + isExpanded);

            for (i = 0; i < SesParCollapseStatus.length; i++) {
                if (SesParCollapseStatus[i].id == ses) {
                    SesParCollapseStatus[i].isExpanded = isExpanded;
                    return;
                }
            }
            SesParCollapseStatus.push({ id: ses, isExpanded: isExpanded });
        };
        ctrl.setSesParCollapseStatus = function (ses) {

            // console.log("setSesParCollapseStatus: " + ses);
            for (i = 0; i < SesParCollapseStatus.length; i++) {
                if (SesParCollapseStatus[i].id == ses) {
                    return SesParCollapseStatus[i].isExpanded ? "collapse in" : "collapse";
                }
            }
            return "collapse";
        };

        /** Version 0 */
        //** */
        ctrl.txtEstado = function (std) {
            return std == session_stdcodes.Desconectado ? $lserv.translate("Desconectado") :
                std == session_stdcodes.Conectado ? $lserv.translate("Conectado") : $lserv.translate("Estado Erroneo");
        };
        //** */
        ctrl.colorEstado = function (std) {
            return std == session_stdcodes.Desconectado ? "text-danger" :
                std == session_stdcodes.Conectado ? "text-info" :
                    std == session_stdcodes.Deshabilitado ? "text-warning" : "text-danger";
        };
        //** */
        ctrl.txtTipo = function (type) {
            return type == session_types.RX ? "Rx" :
                type == session_types.TX ? "Tx" :
                    type == session_types.RXTX ? "TxRx" : "??";
        };

        //** Servicios Pagina del Gestor */
        ctrl.txtTipoEquipo = function (eq) {
            var type = eq.tip == equ_types.Main ? "M" :
                eq.tip == equ_types.Reserva ? "S" : "ERROR";
            type += eq.tip == equ_types.Main ? ("-" + eq.prio) : "";
            return type;
        };
        //** */
        ctrl.gearColorEstado = function (std, sip) {
            return std == 0 ? "text-warning" :                              // Estado Inicial
                std == 1 ? "text-info" :                                 // Ok. sin Asignar.
                    std == 2 ? (sip == "3" ? "text-success" : "text-danger") :  // OK. Asignado. SIP Conectado...
                        std == 3 ? "text-danger" :                                  // En Fallo....
                            std == 4 ? "text-muted" : "text-danger";                    // Ok. Deshabilitado....
        };
        //** */
        ctrl.gearEnableDisableShow = function (std) {
            return (std != 0);
        };
        //** */
        ctrl.gearTextoEstadoEquipo = function (std) {
            return std == 0 ? $lserv.translate("No Inicializado") :                           // Estado Inicial
                std == 1 ? $lserv.translate("Disponible") :                                // Ok. sin Asignar.
                    std == 2 ? $lserv.translate("Asignado") :                                  // OK. Asignado....
                        std == 3 ? $lserv.translate("Fallo") :                                     // En Fallo....
                            std == 4 ? $lserv.translate("No Habilitado") : "???_" + std;               // Ok. Deshabilitado....
        };
        /* */
        ctrl.txtHabilitar = function (equ) {
            return (equ.std == 1 || equ.std == 2 || equ.std == 3) ? $lserv.translate("Disable") : equ.std == 4 ? $lserv.translate("Enable") : "???_" + equ.std;
        };
        //** */
        ctrl.led = function () {
            return ctrl.leds == led_std.Off ? "" : ctrl.leds == led_std.On1 ? "led-green" : ctrl.leds == led_std.On2 ? "led-yellow" : "led-red";
        };
        //** */
        ctrl.EnableDisable = function (item) {
            var bDisable = (item.std == 4);

            var strQuestion = bDisable ? sprintf($lserv.translate("Desea Habilitar el Equipo %1$s ?"), item.equ) :
                sprintf($lserv.translate("Desea Deshabilitar el Equipo %1$s ?"), item.equ);
            if (confirm(strQuestion) == true) {
                $serv.radio_gestormn_enable(item).then(
                    function () {
                        alertify.success($lserv.translate("Operacion Efectuada"));
                        //rdGestormnGet();
                        rdDataGet();
                    },
                    function (response) {
                        console.log(response);
                    });
            }
        };

        //** */
        ctrl.txtOnVHF = function () {
            return sprintf($lserv.translate("VHF En %1$s"), ctrl.site_select /* txtMdSelect(ctrl.vhf_mode_select)*/);
        };
        //** */
        ctrl.txtOnUHF = function () {
            return sprintf($lserv.translate("UHF En %1$s"), ctrl.site_select /*txtMdSelect(ctrl.uhf_mode_select)*/);
        };
        //** */
        ctrl.txtMdSelect = function (md) {
            return md == 0 ? $lserv.translate("Transmisores") : md == 1 ? $lserv.translate("Receptores") : $lserv.translate("Transmisores y Receptores");
        };

        //** */
        ctrl.gearAsignarShow = function (eq) {
            return (eq.tip == 1) && (eq.std == 1 /*|| eq.std == 2*/);
        };
        //** */
        ctrl.gearAsignarText = function (eq) {
            return (eq.std == 1) ? $lserv.translate("Asignar") : (eq.std == 2) ? $lserv.translate("Desasignar") : "???_" + eq.std;
        };
        //** */
        ctrl.Asignar = function (item) {
            switch (item.std) {
                case 0:
                    alertify.error($lserv.translate("El equipo aun no esta inicializado. No puede ser asignado..."));
                    break;
                case 1:
                    alertify.prompt($lserv.translate("Introduzca la frecuencia"), "",
                        function (evt, frec) {
                            var modo_val = item.grp == 0 ? 3 : 4;               // Rango de VHF (4) o de UHF (3)
                            if ($lserv.validate(modo_val, frec) == true) {
                                alertify.confirm(sprintf($lserv.translate("Desea Asignar el equipo %1$s a la frecuencia %2$s?"), item.equ, frec),
                                    function () {
                                        var cmd = { equ: item.equ, cmd: 1, frec: frec };
                                        $serv.radio_gestormn_asigna(cmd).then(function (response) {
                                            alertify.success($lserv.translate("Operacion Efectuada"));
                                        }, function (response) {
                                            console.log(response);
                                        });
                                    },
                                    function () {
                                        alertify.message($lserv.translate("Operacion Cancelada"));
                                    }
                                );

                            }
                            else
                                alertify.error($lserv.translate("Error en formato de frecuencia introducida. El formato de la frecuencia debe ser 'XXX.XXX'"));
                        },
                        function () {
                            alertify.message($lserv.translate("Operacion Cancelada"));
                        }
                    );
                    break;
                case 2:
                    alertify.confirm(sprintf($lserv.translate("Desea Desasignar el equipo %1$d ?"), item.equ),
                        function () {
                            var cmd = { equ: item.eq, cmd: 0, frec: "---.--" };
                            $serv.radio_gestormn_asigna(cmd).then(function (response) {
                                alertify.success($lserv.translate("Operacion Efectuada"));
                            }, function (response) {
                                console.log(response);
                            });
                        },
                        function () {
                            alertify.message($lserv.translate("Operacion Cancelada"));
                        }
                    );

                    break;
                case 3:
                    alertify.error($lserv.translate("El equipo esta en FALLO. No puede ser asignado..."));
                    break;
                case 4:
                    alertify.error($lserv.translate("El equipo esta deshabilitado. Habilitalo antes para poder asignarlo"));
                    break;
                default:
                    break;
            }
        };
        /** */
        ctrl.ResetServicio = function () {
            alertify.confirm($lserv.translate("Desea reiniciar el servicio de gestión radio?"),
                function () {
                    $serv.radio_gestormn_reset().then(function () {
                        alertify.success($lserv.translate("Operacion Efectuada"));
                    }, function (response) {
                        console.log(response);
                        alertify.error($lserv.translate("Error al Ejecutar la operacion"));
                    });
                },
                function () {
                    alertify.message($lserv.translate("Operacion Cancelada"));
                }
            );
        };

        //** */
        ctrl.siteSelect = function (site) {
            ctrl.site_select = site;
        };

        /** Para los Destinos ATS Externos */
        ctrl.eds = [];      // Lista de Destinos...
        ctrl.std_class = function (item) {
            if (item == undefined)
                return stdc_class[stdc.NoInfo];
            if (item.std == stdc.Ok && item.sel == 2)
                return stdc_class[stdc.Reserva];
            if (item.std <= stdc.Error)
                return stdc_class[item.std];
            return stdc_class[stdc.NoInfo];
        };
        ctrl.ed_detail = function (item) {
            var detail = "<table>";
            detail += "<tr class=\"small\"><td>" + "<strong>IP1</strong>" + "</td><td class=\"hyp-none\">" + item.ip1 + "</td><td>" + (item.lan1 == 1 ? $lserv.translate('SCT_MSG_00')/*"Ok"*/ : ("<i>" + $lserv.translate('SCT_MSG_04')/*"Desconectada"*/ + "</i>")) + "</td></tr>";
            if (item.tipo != 5) {
                /** Los grabadores no tienen agente SIP */
                detail += "<tr class=\"small\"><td>" + "<strong>SIP</strong>" + "</td><td class=\"hyp-none\">" + item.uri + "</td><td>" +
                    (item.std_sip == 1 ? $lserv.translate('SCT_MSG_00')/*"Ok"*/ :
                        item.std_sip == 4 ? $lserv.translate('NI')/*"Ok"*/ : ("<i>" + $lserv.translate('Error')/*"Desconectada"*/ + "</i>")) + "</td></tr>";
            }
            detail += "</table>";
            return detail;
        };

        /** HF */
        //** */
        ctrl.hfColorEstado = function (std) {
            return std == 0 ? "text-muted" :                                // No Info.               
                std == 2 ? "text-primary" :                                 // Disponible.. 
                    std == 3 ? "text-success" :                                 // Asignado
                        "text-danger";                                   // Error
        };
        ctrl.hfTextoEstadoEquipo = function (std) {
            return std == 0 ? $lserv.translate("No Inicializado") :         // No Info.               
                std == 2 ? $lserv.translate("Disponible") :                 // Disponible.. 
                    std == 3 ? $lserv.translate("Asignado") :                   // Asignado
                        $lserv.translate("Fallo");                      // Error
        };
        //** */
        ctrl.hfEnableDisableShow = function (std) {
            return (std == 3);
        };
        //** */
        ctrl.hfLiberar = function (item) {
            var strQuestion = item.id + ". " + $lserv.translate("¿Desea Liberar este Transmisor?");
            alertify.confirm(strQuestion,
                function () {
                    $serv.radio_hf_release(item).then(
                        function () {
                            alertify.success($lserv.translate("Operacion Ejecutada."));
                            //rdHfGet();
                            rdDataGet()
                        },
                        function (response) {
                            console.log(response);
                            alertify.error($lserv.translate("No se ha podido ejecutar la operacion."));
                        });
                },
                function () {
                    alertify.message($lserv.translate("Operacion Cancelada"));
                });
        };

        /** Rutinas Generales */
        /** Datos desde el Servidor */
        function rdSessionsGet() {
            $serv.radio_sessions_get().then(function (response) {
                console.log(response.data);
                if (rdSessionsChanged(response.data) == true) {
                    console.log("Cambio en tabla de sesiones");
                    ctrl.sessions = response.data;
                    rdSessionsSort();
                }
            }
                , function (response) {
                    console.log(response);
                });
        }
        /** */
        function rdGestormnGet() {
            $serv.radio_gestormn_get().then(function (response) {
                console.log(response.data);
                if (rdMNManagerChanged(response.data) == true) {
                    console.log("Cambio en tabla de M+N");
                    ctrl.gestormn = response.data;
                    rdMNManagerSort();
                }
                ctrl.leds = ctrl.leds == led_std.On2 ? led_std.On1 : led_std.On2;
            }
                , function (response) {
                    console.log(response);
                    ctrl.leds = led_std.Off;
                });
        }

        //** */
        function rdSessionsChanged(ses) {
            if (ses.constructor === Array && ctrl.sessions.constructor === Array) {
                /** Deben ser iguales */
                if (ses.length != ctrl.sessions.length)
                    return true;
                /** ... y en el mismo orden */
                for (i = 0; i < ses.length; i++) {
                    var nuevo = angular.toJson(ses[i]);
                    var viejo = angular.toJson(ctrl.sessions[i]);
                    if (nuevo != viejo)
                        return true;
                }
            }
            return false;
        }

        /** */
        function rdSessionsSort() {
            var sorted = new Object();
            for (i = 0; i < ctrl.sessions.length; i++) {
                var session = ctrl.sessions[i];
                var frec = session.frec.toString();
                if (sorted[frec] == undefined) {
                    sorted[frec] = {
                        frec: session.frec,
                        ftipo: session.ftipo,
                        prio: session.prio,
                        fstd: session.fstd,
                        fpar: session.fpar,
                        fp_climax_mc: session.fp_climax_mc,
                        fp_bss_win: session.fp_bss_win,
                        selected_site: session.selected_site,
                        selected_site_qidx: session.selected_site_qidx,
                        selected_tx: session.selected_tx,
                        ses: new Object()
                    };
                }
                // sorted[frec].ses.push({
                //     uri: session.uri,
                //     tipo: session.tipo,
                //     std: session.std,
                //     tx_rtp: session.tx_rtp,
                //     tx_cld: session.tx_cld,
                //     tx_owd: session.tx_owd,
                //     rx_rtp: session.rx_rtp,
                //     rx_qidx: session.rx_qidx
                // });
                var sindex = session.uri.toString();
                sorted[frec].ses[sindex] = {
                    uri: session.uri,
                    site: session.site,
                    tipo: session.tipo,
                    std: session.std,
                    tx_rtp: session.tx_rtp,
                    tx_cld: session.tx_cld,
                    tx_owd: session.tx_owd,
                    rx_rtp: session.rx_rtp,
                    rx_qidx: session.rx_qidx
                };
            }
            /** Ordeno por frecuencia */
            //sorted.sort();
            ctrl.frecs = sorted;
        }

        //** */
        function rdMNManagerChanged(mn) {
            if (mn.constructor === Array && ctrl.gestormn.constructor === Array) {
                /** Deben ser iguales */
                if (mn.length != ctrl.gestormn.length)
                    return true;
                /** ... y en el mismo orden */
                for (i = 0; i < mn.length; i++) {
                    if (angular.toJson(mn[i]) != angular.toJson(ctrl.gestormn[i]))
                        return true;
                }
            }
            return false;
        }

        /** */
        function rdMNManagerSort() {
            var PrimerNivel = new Object();
            for (i = 0; i < ctrl.gestormn.length; i++) {

                if (!ctrl.gestormn[i].emp)
                    ctrl.gestormn[i].emp = $lserv.translate("Emplazamiento");

                var empl = ctrl.gestormn[i].emp.toString();
                if (PrimerNivel[empl] == undefined) {
                    PrimerNivel[empl] = { name: empl, equipos: [] };
                }
                PrimerNivel[empl].equipos.push(ctrl.gestormn[i]);
            }

            /** Ordenar en cada Emplazamiento. Criterio 1: BANDA (vhf,uhf), Criterio 2: TIPO (Rx,Tx), Criterio 3: Modo (M,N)*/
            jQuery.each(PrimerNivel, function (index, item) {
                item.equipos.sort(function (a, b) {
                    if (a.grp == b.grp) {
                        if (a.mod == b.mod) {
                            return a.tip < b.tip ? -1 : a.tip > b.tip ? 1 : 0;
                        }
                        return a.mod < b.mod ? -1 : 1;
                    }
                    return a.grp < b.grp ? -1 : 1;
                });
            });

            ctrl.mnman = PrimerNivel;
            /** Selecciono el emplazamiento del primer equipo */
            if (!ctrl.site_select)
                ctrl.site_select = ctrl.gestormn.length == 0 ? "" : ctrl.gestormn[0].emp;
        }

        /** HF */
        /** */
        function rdHfGet() {
            $serv.radio_hf_get().then(function (response) {
                console.log(response.data);
                if (rdHfChanged(response.data) == true) {
                    console.log("Cambio en tabla HF");
                    ctrl.gestorhf = response.data;
                }
            }
                , function (response) {
                    console.log(response);
                });
        }

        //** */
        function rdHfChanged(newdata) {
            return angular.toJson(newdata) != angular.toJson(ctrl.gestorhf);
        }

        /** */
        function extDestGet() {
            $serv.extatsdest_get().then(function (response) {
                console.log(response.data);
                ctrl.eds = response.data.lista;
            }
                , function (response) {
                    console.log(response);
                });
        }

        //** Version Integrada */
        ctrl.LinkEnable = function (strData) {
            if (strData === "MN") {
                return ctrl.gestormn.length > 0 ? "" : "disabled";
            }
            return "";
        };

        //
        function rdDataGet() {
            $serv.radio_data().then(response_on_success => {
                console.log("RdData => ", response_on_success.data);
                rdDataProccess(response_on_success.data);
            }, response_on_fail => {
                console.erro("RdData Error => ", response_on_fail);
            });
        }
        //
        function rdDataProccess(data) {
            rdDataSessionProccess(data.sessions);
            rdDataMNProcess(data.mnenable, data.mndata);
            rdHfDataProcess(data.hfenable, data.hfdata);
            rdUnoMasUnoProcess(data.msenable, data.msdata);
        }
        ///
        function rdDataSessionProccess(sessions) {
            if (rdSessionsChanged(sessions) == true) {
                console.log("Cambio en tabla de sesiones");
                ctrl.sessions = sessions;
                rdSessionsSort();
            }
        }
        ///
        function rdDataMNProcess(enable, data) {
            if (enable === 1) {
                if (rdMNManagerChanged(data) == true) {
                    console.log("Cambio en tabla de M+N");
                    ctrl.gestormn = data;
                    rdMNManagerSort();
                }
            }
            else {
                // TODO Marcar que está InHabiltado...
                ctrl.gestormn = [];
                ctrl.mnman = [];
                ctrl.site_select = null;
            }
        }
        //
        function rdHfDataProcess(enable, data) {
            if (enable === 1) {
                if (rdHfChanged(data) == true) {
                    console.log("Cambio en tabla HF");
                    ctrl.gestorhf = data;
                }
            }
            else {
                ctrl.gestorhf = [];
            }
        }

        // Gestor 1+1
        ctrl.dtUnoMasUno = [];
        ctrl.dtUnoMasUnoSelectedFrec = "";
        function rdUnoMasUnoProcess(enable, data) {
            if (enable === 1) {
                var normalizedData = rdUnoMasUnoNormalize(data);
                if (rdUnoMasUnoChanged(normalizedData) == true) {
                    console.log("Cambio en tabla de 1+1");
                    ctrl.dtUnoMasUno = normalizedData;
                    ctrl.dtUnoMasUnoSelectedFrec = ctrl.dtUnoMasUno.length > 0 ? ctrl.dtUnoMasUno[0].fr : "";
                }
            }
            else {
                ctrl.dtUnoMasUno = [];
                ctrl.dtUnoMasUnoSelectedFrec = "";
            }
        }
        function rdUnoMasUnoChanged(newdata) {
            return angular.toJson(newdata) != angular.toJson(ctrl.dtUnoMasUno);
        }
        function rdUnoMasUnoNormalize(datain) {

            //var data = shuffle(datain);
            //console.log("1+1 Data shuffled => ", data);
            var data = datain;
            // Utiliza linq.js
            // Los agrupa por frecuencia y despues por emplazamiento...
            var res = Enumerable.from(data)
                .orderBy('$.fr')
                .groupBy(
                    '$.fr', '$',
                    function (fr, grp) {
                        //console.log("grp =>", grp);
                        var res1 = Enumerable.from(grp.getSource())
                            .orderBy('$.site')
                            .groupBy('$.site', '$', function (site, grp1) {
                                var en = Enumerable.from(grp1.getSource());
                                var txs = en.where(eq => eq.tx == 1).toArray();
                                var rxs = en.where(eq => eq.tx == 0).toArray();
                                return { site: site, txs: txs, rxs: rxs };
                            })
                            .toArray();
                        return { fr: fr, sites: res1 };
                    }
                )
                .toArray();
            console.log("Res =>", res);
            return res;
        }
        ctrl.rdUnoMasUnoId = function (res) {
            var retorno = "(" + (res.ab == 1 ? "A" : "B") + ") " + res.id;
            return StringCut(retorno, 24);
        };
        ctrl.rdUnoMasUnoIdClass = function (res) {
            var retorno = res.ses == 0 ? "bg-danger" : "";
            return retorno;
        };
        ctrl.rdUnoMasUnoTxDisabled = function (res) {
            return res.ses == 0;
        };
        ctrl.rdUnoMasUnoSelectMain = function (equ) {
                var strQuestion = equ.id + $lserv.translate(" ¿Desea Seleccionar el equipo como Principal?");
                alertify.confirm(strQuestion,
                    function () {
                        $serv.radio_11_select(equ).then(function (response) {
                            console.log("RD1+1 Post Response => ", response.data);
                            alertify.success($lserv.translate("Operacion Ejecutada."));
                        }, function (response) {
                            console.log("RD1+1 Post Error => ", response);
                            alertify.error($lserv.translate("Error: " + response.status + ", " + response.statusText));
                        });
                    },
                    function () {
                        alertify.message($lserv.translate("Operacion Cancelada"));
                    });
        };

    /** Al Iniciarse */
        $scope.$on('$viewContentLoaded', function () {
            //call it here
            //rdSessionsGet();
            //rdGestormnGet();
            //extDestGet();
            //rdHfGet();
            rdDataGet();
        });

        /** Funcion Periodica del controlador */
        var timer = $interval(function () {
            //if (ctrl.pagina() == 0)
            //    rdSessionsGet();
            //else if (ctrl.pagina() == 1) {
            //    rdGestormnGet();
            //}
            //else if (ctrl.pagina() == 3) {
            //    rdHfGet();
            //}
            //else if (ctrl.pagina() == 4) {
            //    extDestGet();
            //}
            rdDataGet();
        }, pollingTime);

        /** Salida del Controlador. Borrado de Variables */
        $scope.$on("$destroy", function () {
            $interval.cancel(timer);
        });

    });


