angular.module("Uv5kiman")
.controller("uv5kiConfigCtrl", function ($scope, $interval, $serv, $lserv) {
    /** Inicializacion */
    var ctrl = this;
    ctrl.grupo = 0;

    /** */
    ctrl.linci = [];    // Lista de Incidencias.
    ctrl.ginci = [];    // Grupo de Incidencias.

    /** */
    ctrl.options = {};

    /** */
    ctrl.snmpoptions = [];
    ctrl.snmpv3users = [];
    ctrl.snmpv3user_onedit = {};

    /** */
    ctrl.show_hf = $lserv.ConfigServerHf();

    /** */
    // ctrl.pagina = 0;
    //ctrl.Pagina = function(pg) {
    //    ctrl.pagina = pg;
    //    if (ctrl.pagina==1)
    //        ctrl.Grupo(0);
    //}
    ctrl.pagina = function (pagina) {
        if (pagina == 1) ctrl.Grupo(0);
        var menu = $lserv.Submenu(pagina);
        return menu ? menu : 0;
    }

    /** */
    ctrl.Grupo = function(grp) {
        switch (grp) {
            case 0:
                ctrl.ginci = ctrl.linci.filter($lserv.inci_filter_genonly);
                break;
            case 1:
                ctrl.ginci = ctrl.linci.filter($lserv.inci_filter_pict);
                break;
            case 2:
                ctrl.ginci = ctrl.linci.filter($lserv.inci_filter_pasa);
                break;
            case 3:
                ctrl.ginci = ctrl.linci.filter($lserv.inci_filter_ext);
                break;
            case 4:
                ctrl.ginci = ctrl.linci.filter($lserv.inci_filter_radio_hf);
                break;
            case 5:
                ctrl.ginci = ctrl.linci.filter($lserv.inci_filter_radio_mn);
                break;
            default:
                ctrl.ginci = [];
                break;
        }
        ctrl.grupo = grp;
        console.log(ctrl.grupo);
    }

    /** */
    ctrl.salvar = function() {
        //if (confirm($lserv.translate('CCT_MSG_00')/*"Desea salvar los cambios efectuados?.\nEl servicio de mantenimiento se Reinciara."*/) == true) {
        //    var obj = { lista: ctrl.linci };
        //    $serv.db_inci_set(obj).then(function(response) {
        //        // alert($lserv.translate('CCT_MSG_01')/*"Cambios salvados correctamente"*/);
        //        alertify.success($lserv.translate('CCT_MSG_01')/*"Cambios salvados correctamente"*/);
        //    }
        //    , function(response) {
        //        console.log(response);
        //    });
        //}
        alertify.confirm($lserv.translate('CCT_MSG_00'),
            function () {
                var obj = { lista: ctrl.linci };
                $serv.db_inci_set(obj).then(function (response) {
                    // alert($lserv.translate('CCT_MSG_01')/*"Cambios salvados correctamente"*/);
                    alertify.success($lserv.translate('CCT_MSG_01')/*"Cambios salvados correctamente"*/);
                }
                , function (response) {
                    console.log(response);
                });
            },
            function () {
                alertify.message($lserv.translate("Operacion Cancelada"));
            }
        );
    }

    /** */
    ctrl.reset = function() {
        //if (confirm($lserv.translate('CCT_MSG_02')/*"Desea Reiniciar el servicio de mantenimiento?"*/)==true) {
        //    $serv.options_set(ctrl.options).then(function(response){
        //        // alert($lserv.translate('CCT_MSG_03')/*"Reinicio de Servicio en Curso"*/);
        //        alertify.success($lserv.translate('CCT_MSG_03')/*"Reinicio de Servicio en Curso"*/);
        //    }
        //    , function(response) {
        //        console.log(response);
        //    });
        //}
        alertify.confirm($lserv.translate('CCT_MSG_02'),
            function () {
                $serv.module_reset().then(function (response) {
                    // alert($lserv.translate('CCT_MSG_03')/*"Reinicio de Servicio en Curso"*/);
                    alertify.success($lserv.translate('CCT_MSG_03')/*"Reinicio de Servicio en Curso"*/);
                }
                , function (response) {
                    console.log(response);
                });
            },
            function () {
                alertify.message($lserv.translate("Operacion Cancelada"));
            }
        );
    }

    /** */
    ctrl.logs = function () {
        var win = window.open('/logs/logfile.txt', '_blank');
        win.focus();
    }

    /** */
    ctrl.options_save = function() {
        //if (confirm($lserv.translate('CCT_MSG_04')/*"Desea Salvar la Configuracion del Servicio ?.\nEl servicio de mantenimiento se Reinciara."*/)==true) {
        //    $serv.options_set(ctrl.options).then(function(response){
        //        // alert($lserv.translate('CCT_MSG_05')/*"Configuración Salvada. Reinicio de Servicio en Curso"*/);
        //        alertify.success($lserv.translate('CCT_MSG_05')/*"Configuración Salvada. Reinicio de Servicio en Curso"*/);
        //    }
        //    , function(response) {
        //        console.log(response);
        //    });
        //}
        alertify.confirm($lserv.translate('CCT_MSG_04'),
            function () {
                $serv.options_set(ctrl.options).then(function (response) {
                    // alert($lserv.translate('CCT_MSG_05')/*"Configuración Salvada. Reinicio de Servicio en Curso"*/);
                    alertify.success($lserv.translate('CCT_MSG_05')/*"Configuración Salvada. Reinicio de Servicio en Curso"*/);
                }
                , function (response) {
                    console.log(response);
                });
            },
            function () {
                alertify.message($lserv.translate("Operacion Cancelada"));
            }
        );
    }

    /** */
    ctrl.snmp_options_save = function () {
        alertify.confirm($lserv.translate('CCT_MSG_04'),
            function () {
                snmp_options_set();
            },
            function () {
                alertify.message($lserv.translate("Operacion Cancelada"));
            }
        );
    }

    /** */
    ctrl.snmp_option_show = function (opt) {
        if (opt.show == 0)
            return true;
        else if (ctrl.snmpoptions && ctrl.snmpoptions.length > 0) {
            if (opt.show == 1)
                return ctrl.snmpoptions[0].val == "0";
            if (opt.show == 2)
                return ctrl.snmpoptions[0].val == "1";
        }
        return true;
    }

    /** */
    ctrl.snmp_user_tp = function (user) {
        return user.tipo == 0 ? "No AUTH, No PRIV" : user.tipo == 1 ? "AUTH, No PRIV" : "AUTH, PRIV";
    }

    /** */
    ctrl.snmp_user_delete = function (user) {
        alertify.confirm($lserv.translate('Desea Eliminar al usuario ' + user.user),
            function () {
                var index = ctrl.snmpv3users.indexOf(user);
                if (index >= 0) {
                    ctrl.snmpv3users.splice(index, 1);
                    alertify.message($lserv.translate("Usuario Eliminado."));
                }
                else {
                    alertify.Error($lserv.translate("Error localizando usuario"));
                }
            },
            function () {
                alertify.message($lserv.translate("Operacion Cancelada"));
            }
        );
    }

    /** */
    ctrl.snmp_user_newormod = function (user) {
        if (!user) {
            ctrl.snmpv3user_onedit = {
                idx: -1,
                user: {
                    user: "",
                    tipo: "0",
                    pwd1: "",
                    pwd2: ""
                }
            };
        }
        else {
            ctrl.snmpv3user_onedit = {
                idx: ctrl.snmpv3users.indexOf(user),
                user: JSON.parse(JSON.stringify(user))
            };
        }
        $("#UserDetail").modal("show");
    }

    /** */
    ctrl.on_snmp_user_edit_ok = function () {
        /** */
        if (!ctrl.snmpv3user_onedit.user.user) {
            alertify.error($lserv.translate("No puede dejar el campo Usuario vacio"));
            return;
        }
        if (ctrl.snmpv3user_onedit.user.tipo != 0) {
            if (!ctrl.snmpv3user_onedit.user.pwd1) {
                alertify.error($lserv.translate("No puede dejar el campo Pwd AUTH vacio"));
                return;
            }
        }
        if (ctrl.snmpv3user_onedit.user.tipo == 2) {
            if (!ctrl.snmpv3user_onedit.user.pwd2) {
                alertify.error($lserv.translate("No puede dejar el campo Pwd PRIV vacio"));
                return;
            }
        }
        /**              */
        alertify.confirm($lserv.translate('Desea Salvar las modificaciones ?'),
            function () {
                if (ctrl.snmpv3user_onedit.idx == -1) {
                    ctrl.snmpv3users.push(ctrl.snmpv3user_onedit.user);
                }
                else {
                    ctrl.snmpv3users[ctrl.snmpv3user_onedit.idx] = JSON.parse(JSON.stringify(ctrl.snmpv3user_onedit.user));
                }
                $("#UserDetail").modal("hide");
                alertify.success(ctrl.snmpv3user_onedit.idx == -1 ? $lserv.translate("Usuario Agregado") : $lserv.translate("Usuario Modificado"));
            },
            function () {
                alertify.message($lserv.translate("Operacion Cancelada"));
            }
        );
    }

    /** Zona de configuracion SACTA */
    /** */
    ctrl.sacta_enable = function () {
        return $lserv.ConfigServerSacta();
    }
    /** */
    ctrl.spsi_users = "";
    ctrl.spv_users = "";
    ctrl.sacta_cfg = null;

    ctrl.sacta_save = function () {
        alertify.confirm($lserv.translate('Desea salvar los cambios efectuados '),
        function () {
            ctrl.sacta_cfg.sacta.SpiUsers = ctrl.spsi_users;
            ctrl.sacta_cfg.sacta.SpvUsers = ctrl.spv_users;
            $serv.sacta_set(ctrl.sacta_cfg).then(function (response) {
                alertify.success($lserv.translate("Operacion Efectuada"));
            },
            function (response) {
                alertify.error($lserv.translate("Error al ejecutar la operacion"));
            }
         );
        },
        function () {
            alertify.message($lserv.translate("Operacion Cancelada"));
        }
    );
    }

    /** Arrancar la visualizacion de versiones. */
    ctrl.versiones = [null, null];
    ctrl.versiones_index = 0;
    ctrl.VersionDetailShow = function () {
        $serv.versiones_get().then(function (response) {
            ctrl.versiones = response.data;
            ctrl.versiones_index = ctrl.versiones[0] ? 0 : 1;
            $("#VersionDetail").modal("show");
        }
        , function (response) {
            alertify.error($lserv.translate("Error al ejecutar la operacion"));
        });
    }

    /** */
    ctrl.versiones_export = function () {
        var csvData = "Servidor;" +
                      "Version;" +
                      "Componente;" +
                      "Fichero;" +
                      "Tamano;" +
                      "Fecha;" +
                      "Hash\r\n";
        $.each(ctrl.versiones, function (iserv, serv) {
            if (serv) {
                $.each(serv.Components, function (index, comp) {
                    $.each(comp.Files, function (index1, file) {
                        var item = (serv.Server + ";") +
                                   (serv.Version + ";") +
                                   (comp.Name + ";") +
                                   (file.Path + ";") + (file.Size + ";") + (file.Date + ";") + (file.MD5 + "\r\n");
                        csvData += item;
                    });
                });
            }
        });

        var myLink = document.createElement('a');
        myLink.download = 'serv_versiones.csv';
        myLink.href = "data:application/csv," + escape(csvData);
        myLink.click();
    }

    /** Funciones Internas.. */
    /** */
    function linci_get() {
        $serv.db_inci_get().then(function (response) {
            ctrl.linci = response.data.lista;
            ctrl.ginci = ctrl.linci.filter($lserv.inci_filter_genonly);
        }
        , function (response) {
            console.log(response);
        });        
    }

    /** */
    function options_get() {
        $serv.options_get().then(function(response){
            ctrl.options = response.data;
        },
        function(response){
            console.log(response);
        });
    }

    /** */
    function snmpv3users_decode(users_in) {
        if (!users_in || users_in.length == 0)
            return [];
        var encoded_users_array = users_in[0].val;
        var decoded_users_array = [];
        $.each(encoded_users_array, function (index, encoded_user) {
            var decoded_user_string = $lserv.descifra(encoded_user);
            var decoded_user_fields = decoded_user_string.split("##");
            if (decoded_user_fields.length == 4) {
                decoded_users_array.push({
                    user: decoded_user_fields[0],
                    tipo: decoded_user_fields[1],
                    pwd1: decoded_user_fields[2],
                    pwd2: decoded_user_fields[3]
                });
            }
        });
        return decoded_users_array;
    }

    /** */
    function snmpv3users_encode(users_in) {
        if (!users_in || users_in.length == 0)
            return [];
        var encoded_users_array = [];
        $.each(users_in, function (index, user) {
            var user_string = $lserv.cifra(user.user + "##" + user.tipo + "##" + user.pwd1 + "##" + user.pwd2);
            encoded_users_array.push(user_string);
        });
        return encoded_users_array;
    }

    /** */
    function snmp_options_get() {
        $serv.snmpoptions_get().then(function (response) {
            console.log(response.data);
            ctrl.snmpoptions = response.data.snmpOptions.filter(function (opt) {
                return opt.tp != 2;
            });
            ctrl.snmpv3users = snmpv3users_decode(response.data.snmpOptions.filter(function (opt) {
                return opt.tp == 2;
            }));
        },
        function (response) {
            console.log(response);
        });
    }

    /** */
    function snmp_options_set() {
        var snmp_options = JSON.parse(JSON.stringify(ctrl.snmpoptions));
        snmp_options.push({
            id: "",
            tp: 2, 
            opt: null, 
            show: 2,
            key: "Snmp_V3Users", 
            val: snmpv3users_encode(ctrl.snmpv3users)
        });
        var data = {
            snmpOptions: snmp_options
        };
        $serv.snmpoptions_set(data).then(function (response) {
            alertify.success($lserv.translate('CCT_MSG_05')/*"Configuración Salvada. Reinicio de Servicio en Curso"*/);
        },
        function (response) {
            alertify.error($lserv.translate("Error en el envio de datos."));
        });
    }

    /** */
    function sacta_cfg_get() {
        $serv.sacta_get().then(function (response) {
            ctrl.sacta_cfg = response.data;
            ctrl.spsi_users = ctrl.sacta_cfg.sacta.SpiUsers;
            ctrl.spv_users = ctrl.sacta_cfg.sacta.SpvUsers;
        }
        , function (response) {
            console.log(response);
        });
    }

    /** Al cargar la Pagina */
    $scope.$on('$viewContentLoaded', function() {
        linci_get();
        options_get();
        snmp_options_get();
        sacta_cfg_get();
    });

    /** Salida del Controlador. Borrado de Variables */
    $scope.$on("$destroy", function () {
    });

    
});
