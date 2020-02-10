/** */
angular.module("Uv5kiman")
.controller("uv5kiSupervCtrl", function ($scope, $interval, $serv, $lserv) {
    /** Inicializacion */
    var ctrl = this;    
    ctrl.lserv = $lserv;

    ctrl.std = {};

    ctrl.cwps = [];
    ctrl.dcwps = [].concat(ctrl.cwps);

    ctrl.gws = [];
    ctrl.dgws= [].concat(ctrl.gws);
    ctrl.gwdata = {};
    ctrl.tars = [];
    ctrl.itfs = [];
    ctrl.gwgen = [];
    ctrl.version = "";

    /** Equipos Externos */
    ctrl.exteq = []; 
    ctrl.dexteq = [].concat(ctrl.exteq);
    ctrl.exteq_fil = [];
    ctrl.exteq_grp = -1;

    ctrl.pbxab = [];
    ctrl.dpbxab = [].concat(ctrl.pbxab);

    //** Servicios del Controlador */
    ctrl.pagina = function (pagina) {
        var menu = $lserv.Submenu(pagina);
        return menu ? menu : 0;
    };

    //** */
    ctrl.std_class = function (item) {
        // return stdc_class[stdc.Ok];
        if (item == undefined)
            return stdc_class[stdc.NoInfo];
        if (item.std == stdc.Ok && item.sel == 2)
            return stdc_class[stdc.Reserva];
        if (item.std <= stdc.Error)
            return stdc_class[item.std];
        return stdc_class[stdc.NoInfo];
    };

    //** Servicios de Estado especificos para los Servidores */
    ctrl.serv_std_class = function (serv, item) {

        if (item == undefined)
            return "";
        var dual = ctrl.std.sv2.enable == 1;
        if (!dual)
            return ctrl.std_class(item);

        var estado_propio = item == undefined ? stdc.NoInfo : item.std;
        var item_colateral = (serv == 0 ? ctrl.std.sv2 : ctrl.std.sv1);
        var estado_colateral = item_colateral == undefined ? stdc.NoInfo : item_colateral.std;

        if (estado_propio == stdc.Ok && item.sel == 1 && estado_colateral == stdc.NoInfo)
            return stdc_class[stdc.Aviso];

        if (item.std == stdc.Ok && item.sel == 2)
            return stdc_class[stdc.Reserva];
        if (item.std <= stdc.Error)
            return stdc_class[estado_propio];

        return stdc_class[stdc.NoInfo];

    };


    ctrl.serv_url = function () {
        return ctrl.std.perfil == 3 ? ctrl.std.sv1.url : "";
    };

    //** Retorno el Detalle de los Nodebox Presentes */
    ctrl.nbx_detail = function () {
        //ctrl.std.nbxs = [{ name: "192.168.0.10", modo: "Slave" }, { name: "192.168.0.11", modo: "Master" }, { name: "192.168.0.12", modo: "Slave" }];
        //ctrl.std.nbx.url = "http://www.google.es";
        if (ctrl.std.nbxs != undefined &&
            ctrl.std.nbxs.length > 0) {
            var detail = "<table>";
            for (i = 0; i < ctrl.std.nbxs.length; i++) {
                if (ctrl.std.nbxs[i].modo == "Master") {
                    detail += "<tr><td><a href=\"" + ctrl.std.nbx.url + "\">" + ctrl.std.nbxs[i].name + ", (" + ctrl.std.nbxs[i].modo + ")" + "</a></td></tr>";
                }
                else {
                    detail += "<tr><td>" + ctrl.std.nbxs[i].name + ", (" + ctrl.std.nbxs[i].modo + ")" + "</td></tr>";
                }
            }
            detail += "</table>";
            return detail;
        }
        return $lserv.translate("Servidor Radio no Presente");
    };

    //** Establece el modo de presentacion de los NBX */
    ctrl.nmxSplitted = function () {
        //var splitted = ctrl.std && ctrl.std.version ? (ctrl.std.version == "2.5.9" ? false : true) : false;
        //return splitted;
        return false;       // En esta version el modo es no split
    };
    ctrl.nbx_mixed_std_class = function () {
        var clase = ctrl.std === undefined || ctrl.std.csi === undefined || ctrl.std.csi.mixed === undefined || ctrl.std.csi.mixed.std === undefined ? stdc_class[stdc.Error] :
            ctrl.std.csi.mixed.std === "Ok" ? stdc_class[stdc.Ok] :
                ctrl.std.csi.mixed.std === "Warning" ? stdc_class[stdc.Aviso] :
                    ctrl.std.csi.mixed.std === "Alarm" ? stdc_class[stdc.Error] : stdc_class[stdc.Error];
        return clase;
    };
    ctrl.nbx_mixed_name = function () {
        var name = ctrl.std === undefined || ctrl.std.csi === undefined || ctrl.std.csi.mixed === undefined || ctrl.std.csi.mixed.mas === undefined ? "???" :
            ctrl.std.csi.mixed.mas === "" ? "---" : ctrl.std.csi.mixed.mas;
        return name;
    };
    ctrl.nbx_mixed_list_class = function () {
        return stdc_class[stdc.Ok];
    };
    ctrl.nbx_mixed_info_services = function (item) {
        var info = sprintf(
            "<table class='nbxtb'>" +
            "<tr><td>Config</td><td class='res'>%1$s</td><td>Radio</td><td class='res'>%2$s</td></tr>" +
            "<tr><td>Presencia</td><td class='res'>%3$s</td><td>Tifx</td><td class='res'>%4$s</td></tr>" +
            "</table>",
            item.CfgService, item.RadioService, item.PresenceService, item.TifxService);
        return info;
    };

    /** Para el estado especifico de los NBX */
    //ctrl.nbx_std_class = function (nbx) {
    //    return nbx.modo == "Master" ? stdc_class[stdc.Ok] :
    //        nbx.modo == "Slave" ? stdc_class[stdc.Aviso] : stdc_class[stdc.Error];
    //};

    ///** Para la info adicional de un NBX */
    //ctrl.nbx_info = function (nbx) {
    //    var info = sprintf(
    //        "<table class='nbxtb'>" +
    //        "<tr><td>CFG</td><td class='res'>%1$s</td><td>Radio</td><td class='res'>%2$s</td></tr>" +
    //        "<tr><td>TIFX</td><td class='res'>%3$s</td><td>Presencia</td><td class='res'>%4$s</td></tr>" +
    //        "</table>",
    //        nbx_detail_std(nbx.CfgService),
    //        nbx_detail_std(nbx.RadioService),
    //        nbx_detail_std(nbx.TifxService),
    //        nbx_detail_std(nbx.PresenceService));
    //    return info;
    //};

    //function nbx_detail_std(std) {
    //    return std == 0 ? $lserv.translate('Slave') :
    //        std == 1 ? $lserv.translate('Master') :
    //        std == 2 ? $lserv.translate('Stopped') : $lserv.translate('Error');
    //}

    /** Nuevas funciones para el NBX SPLIT */
    ctrl.nbx_splitted_enable = nbx_splitted;
    ctrl.nbx_radio_std_class = function () {
        var clase = ctrl.std === undefined || ctrl.std.csi === undefined || ctrl.std.csi.radio === undefined || ctrl.std.csi.radio.std === undefined ? stdc_class[stdc.Error] :
            ctrl.std.csi.radio.std === "Ok" ? stdc_class[stdc.Ok] :
                ctrl.std.csi.radio.std === "Warning" ? stdc_class[stdc.Aviso] :
                    ctrl.std.csi.radio.std === "Alarm" ? stdc_class[stdc.Error] : stdc_class[stdc.Error];
        return clase;
    };
    ctrl.nbx_radio_name = function () {
        var name = ctrl.std === undefined || ctrl.std.csi === undefined || ctrl.std.csi.radio === undefined || ctrl.std.csi.radio.mas === undefined ? "???" :
            ctrl.std.csi.radio.mas === "" ? "---" : ctrl.std.csi.radio.mas;
        return name;
    };
    ctrl.nbx_radio_count = function () {
        var count = ctrl.std === undefined ||
            ctrl.std.csi === undefined ||
            ctrl.std.csi.radio === undefined ||
            ctrl.std.csi.radio.rdsl === undefined ? 0 : ctrl.std.csi.radio.rdsl.length;
        return count.toString();
    };
    ctrl.nbx_phone_std_class = function () {
        var clase = ctrl.std === undefined || ctrl.std.csi === undefined || ctrl.std.csi.phone === undefined || ctrl.std.csi.phone.std === undefined ? stdc_class[stdc.Error] :
            ctrl.std.csi.phone.std === "Ok" ? stdc_class[stdc.Ok] :
                ctrl.std.csi.phone.std === "Warning" ? stdc_class[stdc.Aviso] :
                    ctrl.std.csi.phone.std === "Alarm" ? stdc_class[stdc.Error] : stdc_class[stdc.Error];
        return clase;
    };
    ctrl.nbx_phone_name = function () {
        var name = ctrl.std === undefined || ctrl.std.csi === undefined || ctrl.std.csi.radio === undefined || ctrl.std.csi.phone.mas === undefined ? "???" :
            ctrl.std.csi.phone.mas === "" ? "---" : ctrl.std.csi.phone.mas;
        return name;
    };
    ctrl.nbx_phone_count = function () {
        var count = ctrl.std === undefined ||
            ctrl.std.csi === undefined ||
            ctrl.std.csi.phone === undefined ||
            ctrl.std.csi.phone.phsl === undefined ? 0 : ctrl.std.csi.phone.phsl.length;
        return count.toString();
    };
    ctrl.nbx_radio_list_class = function () {
        return stdc_class[stdc.Ok];
    };
    ctrl.nbx_radio_info_services = function (item) {
        var info = sprintf(
            "<table class='nbxtb'>" +
            "<tr><td>Config</td><td class='res'>%1$s</td><td>Radio</td><td class='res'>%2$s</td></tr>" +
            "</table>",
            item.CfgService,
            item.RadioService);
        return info;
    };
    ctrl.nbx_phone_list_class = function () {
        return stdc_class[stdc.Ok];
    };
    ctrl.nbx_phone_info_services = function (item) {
        var info = sprintf(
            "<table class='nbxtb'>" +
            "<tr><td>Focus</td><td class='res'>%1$s</td><td>Presencia</td><td class='res'>%2$s</td></tr>" +
            "<tr><td>TIFX</td><td class='res'>%3$s</td></tr>" +
            "</table>",
            item.PhoneService,
            item.PresenceService,
            item.TifxService);
        return info;
    };
    /* ******************************/

    //** Rutinas especificas para SACTA */
    ctrl.SactaServiceStd = function (std) {
        return {
            text: $lserv.translate(std == true ? 'Servicio Arrancado: Detener' : 'Servicio Detenido: Arrancar'),
            clase: std == true ? 'btn btn-default' : 'btn btn-danger'
        };
    };

    ctrl.SactaServiceStartStop = function (std) {
        var pregunta = std == true ? $lserv.translate('Desea Detener el Sevicio?') : $lserv.translate('Desea Arrancar el Servicio?');
        var mensaje = std == true ? $lserv.translate('Deteniendo Servicio') : $lserv.translate('Arrancando Servicio');

        alertify.confirm(pregunta,
            function () {
                var startstop = !std;
                $serv.sacta_startstop(startstop).then(function (response) {
                    alertify.success(mensaje);
                    setTimeout(function () { get_std(); }, 1000);
                }
                    , function (response) {
                        console.log(response);
                        alertify.error($lserv.translate("Error ejecutando la operacion..."));
                    });
            },
            function () {
                alertify.message($lserv.translate("Operacion Cancelada"));
            }
        );
    };

    /** Retorna el Detalle de un operador 
        Texto HTML para POPOVER
    */
    var popoverTimers = {};
    ctrl.popover_click = function ($event) {

        var current = angular.element($event.currentTarget);
        var isVisible = current.data('bs.popover').tip().hasClass('in');

        if (isVisible == false) {
            current.popover('show');

            /** Timeout de 10 segundos */
            popoverTimers[current] =
                setTimeout(function () {
                    current.popover('hide');
                    popoverTimers[current] = null;
                }, 10000);
        }
        else {
            clearTimeout(popoverTimers[current]);
            current.popover('hide');
        }
    };

    //** */
    ctrl.ope_detail = function (item) {
        var detail = "<table>";
        detail += "<tr><td>" + "<strong>IP</strong>" + "</td><td>" + item.ip + "</td></tr>";
        if (item.std != 0) {
            //detail += "<tr><td>" + "<strong>LAN1</strong>" + "</td><td>" + (item.lan1 == 1 ? $lserv.translate('SCT_MSG_00')/*"Ok"*/ : ("<i>" + $lserv.translate('SCT_MSG_01')/*"Fallo"*/+"</i>") ) + "</td></tr>";
            //detail += "<tr><td>" + "<strong>LAN2</strong>" + "</td><td>" + (item.lan2 == 1 ? $lserv.translate('SCT_MSG_00')/*"Ok"*/ : ("<i>" + $lserv.translate('SCT_MSG_01')/*"Fallo"*/ + "</i>")) + "</td></tr>";
            detail += ctrl_ope_detail_lan("LAN1", item.lan1);
            detail += ctrl_ope_detail_lan("LAN2", item.lan2);

            detail += "<tr><td>" + "<strong>" + $lserv.translate('SCT_MSG_02')/*"Salvapantallas:\t\t"*/ + "</strong>" + "</td><td>" + (item.panel == 1 ? $lserv.translate('SCT_MSG_15')/*"No"*/ : ("<i>" + $lserv.translate('SCT_MSG_14')/*"Activo"*/ + "</i>")) + "</td></tr>";
            detail += "<tr><td>" + "<strong>" + $lserv.translate('SCT_MSG_05')/*"Jack EjAl:\t"*/ + "</strong>" + "</td><td>" + (item.jack_exe == 1 ? $lserv.translate('SCT_MSG_03')/*"Conectado"*/ : $lserv.translate('SCT_MSG_04')/*"Desconectado"*/) + "</td></tr>";
            detail += "<tr><td>" + "<strong>" + $lserv.translate('SCT_MSG_06')/*"Jack AyPr:\t"*/ + "</strong>" + "</td><td>" + (item.jack_ayu == 1 ? $lserv.translate('SCT_MSG_03')/*"Conectado"*/ : $lserv.translate('SCT_MSG_04')/*"Desconectado"*/) + "</td></tr>";
            detail += "<tr><td>" + "<strong>" + $lserv.translate('SCT_MSG_07')/*"Altavoz Radio:\t"*/ + "</strong>" + "</td><td>" + (item.alt_r == 1 ? $lserv.translate('SCT_MSG_03')/*"Conectado"*/ : ("<i>" + $lserv.translate('SCT_MSG_04') /*"Desconectado"*/ + "</i>")) + "</td></tr>";
            detail += "<tr><td>" + "<strong>" + $lserv.translate('SCT_MSG_08')/*"Altavoz LC:\t"*/ + "</strong>" + "</td><td>" + (item.alt_t == 1 ? $lserv.translate('SCT_MSG_03')/*"Conectado"*/ : ("<i>" + $lserv.translate('SCT_MSG_04')/*"Desconectado"*/ + "</i>")) + "</td></tr>";
            if (item.alt_hf != -1)
                detail += "<tr><td>" + "<strong>" + $lserv.translate('SCT_MSG_12')/*"Altavoz HF:\t"*/ + "</strong>" + "</td><td>" + (item.alt_hf == 1 ? $lserv.translate('SCT_MSG_03')/*"Conectado"*/ : ("<i>" + $lserv.translate('SCT_MSG_04')/*"Desconectado"*/ + "</i>")) + "</td></tr>";
            if (item.rec_w != -1)
                detail += "<tr><td>" + "<strong>" + $lserv.translate('SCT_MSG_13')/*"Cable Grabacion:\t"*/ + "</strong>" + "</td><td>" + (item.rec_w == 1 ? $lserv.translate('SCT_MSG_03')/*"Conectado"*/ : ("<i>" + $lserv.translate('SCT_MSG_04')/*"Desconectado"*/ + "</i>")) + "</td></tr>";
        }
        detail += "</table>";
        return detail;
    };

    //** */
    function ctrl_ope_detail_lan(lan, std) {
        var detail = "<tr><td>";
        detail += ("<strong>" + lan + "</strong>");
        detail += "</td><td>";
        detail += (std == 1 ? $lserv.translate('SCT_MSG_00')/*"Ok"*/ :
                       std == 2 ? ("<i>" + $lserv.translate('SCT_MSG_01')/*"Fallo"*/ + "</i>") :
                                  ("<i>" + "Unknown" + "</i>"));
        detail += "</td></tr>";
        return detail;
    }

    /** Abre y Gestiona la Ventana de Version del operador */
    ctrl.ope_version = {};
    ctrl.ope_shown = {};
    ctrl.ope_version_show = function (ope) {
        $serv.cwp_version_get(ope).then(function (response) {
            ctrl.ope_version = response.data;
            ctrl.ope_shown = ope;
            $("#opeVersiones").modal('show');
        }
            , function (response) {
                console.log(response);
                alertify.error($lserv.translate('SCT_MSG_09')/*"Error Comunicaciones. Mire Log Consola..."*/);
            });
    };

    ctrl.export_ope_version = function () {
        var csvData = "Terminal;" +
            "Version;" +
            "Componente;" +
            "Fichero;" +
            "Tamano;" +
            "Fecha;" +
            "Hash\r\n";
        $.each(ctrl.ope_version.Components, function (index, comp) {
            $.each(comp.Files, function (index1, file) {
                var item = (ctrl.ope_version.Server + ";") +
                    (ctrl.ope_version.Version + ";") +
                    (comp.Name + ";") +
                    (file.Path + ";") + (file.Size + ";") + (file.Date + ";") + (file.MD5 + "\r\n");
                csvData += item;
            });
        });

        var myLink = document.createElement('a');
        myLink.download = 'pict_' + ctrl.ope_shown.name + '_versiones.csv';
        myLink.href = "data:application/csv," + escape(csvData);
        myLink.click();
    };

    /** Retorna la Imagen de Una pasarela segun el tipo y el estado de cpu0 y cpu1 si es dual */
    var Type1Images = [
        ["./images/gw-tipo1.png",   "./images/gw-tipo1-1.png", "./images/gw-tipo1-2.png"],
        ["./images/gw-tipo1-3.png", "./images/gw-tipo1-8.png", "./images/gw-tipo1-5.png"],
        ["./images/gw-tipo1-4.png", "./images/gw-tipo1-6.png", "./images/gw-tipo1-7.png"]
    ];
    var Type2Images = [
        ["./images/gw-tipo2.png", "./images/gw-tipo2-1.png", "./images/gw-tipo2-2.png"],
        ["./images/gw-tipo2-3.png", "./images/gw-tipo2-8.png", "./images/gw-tipo2-5.png"],
        ["./images/gw-tipo2-4.png", "./images/gw-tipo2-6.png", "./images/gw-tipo2-7.png"]
    ];

    ctrl.gw_img = function (item) {

        //return item.tipo==0 ? "./images/gw-tipo0.png" :
        //    item.tipo == 1 ? "./images/gw-tipo1.png" : 
        //    item.tipo == 2 ? "./images/gw-tipo2.png" : "./images/gwoff.png";

        if (item.tipo == 0) {
            return "./images/gw-tipo0.png";
        }
        else if (item.tipo == 2) {
            return "./images/gw-tipo2.png";
        }
        else if (item.tipo == 1) {
            var row = item.cpu0 >= 0 && item.cpu0 < 3 ? item.cpu0 : 0;
            var col = item.cpu1 >= 0 && item.cpu1 < 3 ? item.cpu1 : 0;
            if ($lserv.GatewayDualityType()==0)
                return Type1Images[row][col];
            else
                return Type2Images[row][col];
        }
    };

    //** Retorna el Detalle de una Pasarela Texto HTML para POPOVER */
    ctrl.gw_detail = function (item) {
        var detail = "<table>";
        detail += "<tr><td>" + "<strong>IP</strong>" + "</td><td>" + item.ip + "</td></tr>";
        if (item.std != 0) {
            detail += "<tr><td>" + "<strong>LAN1</strong>" + "</td><td>" + (item.lan1 == 1 ? $lserv.translate('SCT_MSG_00')/*"Ok"*/ : ("<i>" + $lserv.translate('SCT_MSG_01')/*"Fallo"*/ + "</i>")) + "</td></tr>";
            detail += "<tr><td>" + "<strong>LAN2</strong>" + "</td><td>" + (item.lan2 == 1 ? $lserv.translate('SCT_MSG_00')/*"Ok"*/ : ("<i>" + $lserv.translate('SCT_MSG_01')/*"Fallo"*/ + "</i>")) + "</td></tr>";
            detail += "<tr><td>" + "<strong>" + (item.tipo != 0 ? "MAIN" : "") + "</strong>" + "</td><td>" + (item.tipo != 0 ? (item.main == 0 ? "0" : "1") : "") + "</td></tr>";
        }
        detail += "</table>";
        return detail;
    };

    //** Pone el titulo a la ventana de Detalle de la Pasarela */
    ctrl.gw_detail_title = function () {
        if (ctrl.gwdata.tipo === 0)
            return ctrl.gwdata.name + " (" + ctrl.gwdata.ip + ")";
        else {
            return ctrl.gwdata.name + ",[CPU-" + ctrl.gwdata.main + "]" + " (" + ctrl.gwdata.ip + ")";
        }
    };

    //** Abre y Gestiona la ventana de Detalle de la Pasarela */
    ctrl.gw_open = function (gw) {
        $serv.gw_detail_get(gw).then(function (response) {
            ctrl.gwdata = response.data;
            ctrl.itfs = ctrl.gwdata.tipo == 0 ? ctrl.gwdata.cpus[0].recs :
                ctrl.gwdata.main == 0 ? ctrl.gwdata.cpus[0].recs : ctrl.gwdata.cpus[1].recs;
            ctrl.tars = ctrl.gwdata.tipo == 0 ? ctrl.gwdata.cpus[0].tars :
                ctrl.gwdata.main == 0 ? ctrl.gwdata.cpus[0].tars : ctrl.gwdata.cpus[1].tars;

            ctrl.gwgen = [];
            ctrl.gwgen[0] = {
                tipo: ctrl.gwdata.tipo,
                fa: ctrl.gwdata.cpus[0].fa,            // ctrl.gwdata.fa,
                cpu: 0,
                ip: ctrl.gwdata.cpus[0].ip,
                lan1: ctrl.gwdata.cpus[0].lan1,
                lan2: ctrl.gwdata.cpus[0].lan2,
                sipMod: ctrl.gwdata.cpus[0].sipMod,
                cfgMod: ctrl.gwdata.cpus[0].cfgMod,
                snmpMod: ctrl.gwdata.cpus[0].snmpMod
            };
            if (ctrl.gwdata.tipo != 0) {
                ctrl.gwgen[1] = {
                    tipo: "",
                    fa: ctrl.gwdata.cpus[1].fa,    // "",
                    cpu: 1,
                    ip: ctrl.gwdata.cpus[1].ip,
                    lan1: ctrl.gwdata.cpus[1].lan1,
                    lan2: ctrl.gwdata.cpus[1].lan2,
                    sipMod: ctrl.gwdata.cpus[1].sipMod,
                    cfgMod: ctrl.gwdata.cpus[1].cfgMod,
                    snmpMod: ctrl.gwdata.cpus[1].snmpMod

                };
            }

            // console.log(response.data);
            $("#gwDataModal").modal('show');
        }
            , function (response) {
                console.log(response);
                alertify.error($lserv.translate('SCT_MSG_09')/*"Error Comunicaciones. Mire Log Consola..."*/);
            });
    };

    //** */
    ctrl.gw_ir = function (gw) {
        var win = window.open('http://' + gw.ip + ':8080', '_blank');
        win.focus();
    };

    //** Abre y Gestiona la Ventana de Version de la Pasarela */
    ctrl.gw_version = function (gw) {
        $serv.gw_version_get(gw).then(function (response) {
            console.log(response.data);
            ctrl.gwdata = response.data;
            ctrl.version = JSON.parse(response.data.versiones);
            $("#gwVersionModal").modal('show');
        }
            , function (response) {
                console.log(response);
                alertify.error($lserv.translate('SCT_MSG_09')/*"Error Comunicaciones. Mire Log Consola..."*/);
            });
    };

    ctrl.export_gw_version = function () {
        var csvData = "Pasarela;" +
            "Version;" +
            "Componente;" +
            "Fichero;" +
            "Tamano;" +
            "Fecha;" +
            "Hash\r\n";
        $.each(ctrl.version.Components, function (index, comp) {
            $.each(comp.Files, function (index1, file) {
                var item = (ctrl.gw_detail_title() + ";") +
                    (ctrl.version.Version + ";") +
                    (comp.Name + ";") +
                    (file.Path + ";") + (file.Size + ";") + (file.Date + ";") + (file.Hash + "\r\n");
                csvData += item;
            });
        });

        var myLink = document.createElement('a');
        myLink.download = 'gw_' + ctrl.gwdata.name + '_versiones.csv';
        myLink.href = "data:application/csv," + escape(csvData);
        myLink.click();
    };

    //** */
    ctrl.verfname = function (path) {
        return path.substring(path.lastIndexOf('/') + 1);
    };

    //** Mando de Control P/R de Pasarela */
    ctrl.gw_control_pr = function (gw) {

        //if (confirm($lserv.translate('SCT_MSG_10')/*"¿Desea ejecutar una conmutacion P/R en la Pasarela "*/ + gw.name + "?") == true) {
        //    $serv.gw_pr_change(gw).then(function (response) {
        //        alertify.success($lserv.translate('SCT_MSG_11')/*"Operacion Efectuada Correctamente..."*/)
        //    }
        //    , function (response) {
        //        console.log(response);
        //        alertify.error($lserv.translate('SCT_MSG_09')/*"Error Comunicaciones. Mire Log Consola..."*/);
        //    });
        //}
        var ChangeAvailable = (gw.cpu0 == 1 && gw.cpu1 == 2) || (gw.cpu0 == 2 && gw.cpu1 == 1);
        if (ChangeAvailable == true) {
            alertify.confirm($lserv.translate('SCT_MSG_10')/*"¿Desea ejecutar una conmutacion P/R en la Pasarela "*/ + gw.name + "?",
                function () {
                    $serv.gw_pr_change(gw).then(function (response) {
                        alertify.success($lserv.translate('SCT_MSG_11')/*"Operacion Efectuada Correctamente..."*/);
                    }
                        , function (response) {
                            console.log(response);
                            alertify.error($lserv.translate('SCT_MSG_09')/*"Error Comunicaciones. Mire Log Consola..."*/);
                        });
                },
                function () {
                    alertify.message($lserv.translate('Operacion Cancelada'));
                }
            );
        }
        else {
            alertify.error($lserv.translate('El cambio no es posible...'));
        }
    };

    //** Retorna la Imagen de Equipo Externo */
    ctrl.exteq_img = function (item) {
        if (item.tipo == 3)
            return "./images/voiptel.jpg";
        else if (item.tipo == 5)
            return "./images/audio-recorder.jpg";
        else if (item.tipo == 2) {
            if (item.modelo == 1000) {
                return item.txrx == 4 ? "./images/radiorh_rx.jpg" : item.txrx == 5 ? "./images/radiorh.jpg" : "./images/radiorh.jpg";
            }
            else if (item.modelo == 1001) {
                return item.txrx == 4 ? "./images/radiojt_rx.jpg" : item.txrx == 5 ? "./images/radiojt.jpg" : "./images/radiojt.jpg";
            }
            else {
                return item.txrx == 4 ? "./images/radio_rx.jpg" : item.txrx == 5 ? "./images/radio.jpg" : "./images/radio.jpg";
            }
        }
        return "./images/TifxGris.png";
    };

    /* */
    ctrl.exteq_detail = function (item) {
        var detail = "<table>";
        detail += "<tr class=\"small\"><td>" + "<strong>IP1</strong>" + "</td><td class=\"hyp-none\">" + item.ip1 + "</td><td>" + (item.lan1 == 1 ? $lserv.translate('SCT_MSG_00')/*"Ok"*/ : ("<i>" + $lserv.translate('SCT_MSG_04')/*"Desconectada"*/ + "</i>")) + "</td></tr>";
        if (item.tipo != 5) {
            /** Los grabadores no tienen agente SIP */
            detail += "<tr class=\"small\"><td>" + "<strong>SIP</strong>" + "</td><td class=\"hyp-none\">" + item.uri + "</td><td>" +
                (item.std_sip == 1 ? $lserv.translate('SCT_MSG_00')/*"Ok"*/ :
                    item.std_sip == 4 ? /*$lserv.translate('NI')*//*"Ok"*/ item.lor :
                        ("<i>" + $lserv.translate('Error')/*"Desconectada"*/ + "</i>")) + "</td></tr>";
        }
        detail += "</table>";
        return detail;
    };

    //** */
    ctrl.exteq_std_class = function (item) {
        if (item == undefined || item.std > 6 || item.std_sip > 6)
            return stdc_class[stdc.NoInfo];
        if (item.std == stdc.Ok) {
            if (item.std_sip == stdc.Ok)
                return stdc_class[stdc.Ok];
            else if (item.std_sip == stdc.Aviso)
                return stdc_class[stdc.Aviso];
            else
                return stdc_class[stdc.Error];
        }
        return stdc_class[item.std];
    };

    /* */
    ctrl.exteq_grupo = function (grp) {
        if (grp == 0) {                 // Telefonia
            ctrl.exteq_grps = ctrl.exteq.filter($lserv.exteq_filter_tel);
        }
        else if (grp == 1) {            // Receptores Radio
            ctrl.exteq_grps = ctrl.exteq.filter($lserv.exteq_filter_rx);
        }
        else if (grp == 2) {            // Transmisores Radio
            ctrl.exteq_grps = ctrl.exteq.filter($lserv.exteq_filter_tx);
        }
        else if (grp == 3) {            // Transmisores Radio
            ctrl.exteq_grps = ctrl.exteq.filter($lserv.exteq_filter_rec);
        }
        else {                          // Todos.
            ctrl.exteq_grps = ctrl.exteq.filter($lserv.exteq_filter_all);
        }
        ctrl.exteq_grp = grp;
    };

    /** Servicios Pagina de Estado */
    /** */
    function get_std() {
        /* Obtener el estado del servidor... */
        $serv.stdgen_get().then(function (response) {
            ctrl.std = response.data;
            // console.log(ctrl.std);
        }
        , function (response) {
            console.log(response);
        });
    }

    /** */
    function get_cwps() {
        $serv.cwps_get().then(function (response) {
            var sortedData = Enumerable.from(response.data.lista)
                .orderBy("$.name")
                .toArray();
            if (new_cwps(sortedData)) {                
                ctrl.cwps = sortedData;
            }
            // console.log(ctrl.cwps);
        }
        , function (response) {
            console.log(response);
        });
    }

    /** */
    function get_gws() {
        $serv.gws_get().then(function (response) {
		
            // ctrl.gws = response.data.lista;
            ctrl.gws = response.data.lista.sort(function(a,b){
				return a.name > b.name ? 1 : a.name < b.name ? -1 : 0;
			});
            // console.log(ctrl.gws);
            /** Para el tipo dualidad de pasarelas */
            localStorage.GwDualityType = response.data.gdt;
        }
        , function (response) {
            console.log(response);
        });        
    }

    /** */
    function get_exteq() {
        $serv.exteq_get().then(function (response) {
            ctrl.exteq = Enumerable.from(response.data.lista)
                .orderBy('$.name')
                .toArray();
            ctrl.exteq_grupo(ctrl.exteq_grp);
            // console.log(ctrl.exteq);
        }
        , function (response) {
            console.log(response);
        });        
    }

    /** */
    function get_pbxab() {
        $serv.pbxab_get().then(function (response) {
            ctrl.pbxab = response.data.lista;
            // console.log(ctrl.pbxab);
        }
        , function (response) {
            console.log(response);
        });                
    }

    //** TODO... */
    function new_cwps(data) {
        // return data.length != ctrl.cwps.length;
        return true;
    }

    /** Funcion Periodica del controlador */
    var timer = $interval(function () {
        if (ctrl.pagina()==0 || ctrl.pagina()==3)
            get_std();
        else if (ctrl.pagina()==1)
            get_cwps();
        else if (ctrl.pagina()==2)
            get_gws();
        else if (ctrl.pagina()==4)
            get_pbxab();
        else if (ctrl.pagina()==5)
            get_exteq();
    }, pollingTime);

    /** */
    $scope.$on('$viewContentLoaded', function() {
        //call it here
        get_std();
        get_cwps();
        get_gws();
        get_pbxab();
        get_exteq();
    });

    /** Salida del Controlador. Borrado de Variables */
    $scope.$on("$destroy", function () {
        $interval.cancel(timer);
    });

});


