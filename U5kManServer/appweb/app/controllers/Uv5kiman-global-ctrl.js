/** */
angular.module("Uv5kiman")
    .controller("uv5kiGlobalCtrl", function ($scope, $interval, $location, $translate, $serv, $lserv) {
        /** Inicializacion */
        var timetoinci = 0;
        var ctrl = this;

        ctrl.pagina = function (pagina) {
            var menu = $lserv.Menu(pagina);
            return menu ? menu : 0;
        };

        /** Lista de Incidencias */
        ctrl.listainci = [];
        ctrl.dlistainci = [].concat(ctrl.listainci);
        ctrl.HashCode = 0;
        load_inci();

        //ctrl.user="nouser";
        //ctrl.retorno = "noretorno";

        ctrl.date = (new Date()).toLocaleDateString();
        ctrl.hora = (new Date()).toLocaleTimeString();

        //$lserv.check_access();
        //ctrl.user = $lserv.User();
        //ctrl.retorno = $lserv.Retorno();

        //** */
        ctrl.user = function () {
            var user = $lserv.User();
            var perfil = $lserv.Perfil();
            user += (perfil == 0 ? "(ope)" : perfil == 1 ? "(T1)" : perfil == 2 ? "(T2)" : "(T3)");
            return user;
        };

        //** */
        ctrl.retorno = function () {
            return $lserv.Retorno() == null ? false : true;
        };

        //** */
        ctrl.reconoce = function (incidencia, pregunta) {
            if (pregunta) {
                //if (confirm($lserv.translate('GCT_MSG_00') /*"¿ Desea reconocer la incidencia: " */ + incidencia.inci + "?") == false)
                //    return;
                alertify.confirm($lserv.translate('GCT_MSG_00') /*"¿ Desea reconocer la incidencia: " */ + incidencia.inci + "?",
                    function () {
                        $serv.listinci_rec({ user: ctrl.user(), inci: incidencia });
                        load_inci();
                    },
                    function () {
                        alertify.message($lserv.translate("Operacion Cancelada"));
                    }
                );
            }
            else {
                $serv.listinci_rec({ user: ctrl.user(), inci: incidencia });
            }
        };

        /** */
        ctrl.reconoce_todas = function () {
            //if (ctrl.listainci.length==0 || confirm("¿ Desea Reconocer todas las Incidencias ?") == false)
            //    return;
            ///** inci in ctrl.dlistainci */
            //ctrl.listainci.forEach(function (inci, index) {
            //    ctrl.reconoce(inci, false);
            //});
            //load_inci();

            alertify.confirm($lserv.translate("Desea Reconocer todas las Incidencias ?"),
                function () {
                    ctrl.listainci.forEach(function (inci, index) {
                        ctrl.reconoce(inci, false);
                    });

                    load_inci();

                    // ctrl.listainci = [];
                    // ctrl.HashCode = -1;
                },
                function () {
                    alertify.message($lserv.translate("Operacion Cancelada"));
                }
            );
        };

        /** */
        ctrl.retorna = function () {
            window.location.href = $lserv.Retorno();
        };

        /** */
        ctrl.manual = function () {
            window.open("/doc/Manual_de_Usuario.htm", "_blank");
        };

        //** */
        ctrl.inci_desc = function (desc) {
            return /*$lserv.descifra(desc)*/desc;
        };

        /** */
        ctrl.about = function () {
            var page_from = parseInt(ctrl.pagina());
            // alertify.alert("Acerca de...");
            if (!alertify.About) {
                //define a new dialog
                alertify.dialog('About', function factory() {
                    return {
                        main: function (message) {
                            this.message = message;
                        },
                        setup: function () {
                            return {
                                buttons: [
                                    //{ text: $lserv.translate("Opciones") },
                                    { text: $lserv.translate("Aceptar"), key: 27/*Esc*/ }
                                ],
                                focus: { element: 0 }
                            };
                        },
                        prepare: function () {
                            this.setContent(this.message);
                            this.set('maximizable', false);
                            this.set('closable', false);
                        },
                        build: function () {
                            this.setHeader('<p style="text-align: left; font-size: 90%;color: #4A7729;"><i><b>' + $lserv.translate('Ulises V 5000 I. Acerca de...') + '</b></i></p>');
                            this.set('resizable', true);
                        },
                        // This will be called each time an action button is clicked.
                        callback: function (closeEvent) {
                            //The closeEvent has the following properties
                            //
                            // index: The index of the button triggering the event.
                            // button: The button definition object.
                            // cancel: When set true, prevent the dialog from closing.
                            //if (closeEvent.index === 0) {
                            //    javascript:showOptions();
                            //    closeEvent.cancel = true;
                            //}
                        }
                    };
                });
            }

            $serv.options_get().then(function (response) {
                var url_license = "http://" + window.location.hostname + ':' + window.location.port + '/COPYING.AUTHORIZATION.txt';
                var msg = '<div>' +
                    '<br/>' +
                    '<h4 style="color: #4A7729;">Ulises V 5000 I</h4>' +
                    '<p style="text-align:center; color: #4A7729;">Version ' + response.data.version + '</p>' +
                    '<br/>' +
                    '<p style="text-align:center; color: #4A7729;">' + $lserv.translate('INDEX_PIE') + '</p>' +
                    '<p style="text-align: right"><a href="' + url_license + '" target="_blank">' + $lserv.translate('Acuerdo de Licencia') + '</a></p>' +
                    '</div>';
                alertify.About(msg).resizeTo(500, 300).set({
                    onclose: function () {
                        ctrl.pagina(page_from);
                    }
                });
            },
                function (response) {
                    console.log(response);
                });

        };

        /** 20181009. Configuraciones locales */
        var editor = null;
        window.showOptions = function () {


            if (!alertify.LocalOptions) {

                //define a new dialog
                alertify.dialog('LocalOptions', function factory() {
                    return {
                        main: function (message) {
                            //this.message = message;
                            editor.setValue({ SoundOnClient: $lserv.Sound() });
                        },
                        setup: function () {
                            return {
                                buttons: [
                                    { text: $lserv.translate("Aceptar") },
                                    { text: $lserv.translate("Cancelar"), key: 27/*Esc*/ }
                                ],
                                focus: { element: 0 }
                            };
                        },
                        prepare: function () {
                            this.setContent(this.message);
                            this.set('maximizable', false);
                            this.set('closable', false);
                        },
                        build: function () {
                            this.setHeader('<p style="text-align: left; font-size: 90%;"><i>' + $lserv.translate('Ulises V 5000 I. Opciones Locales') + '</i></p>');
                            this.set('resizable', true);
                            editor = new JSONEditor(
                                this.elements.content,
                                {
                                    schema: {
                                        type: "object",
                                        title: "Opciones Locales",
                                        options: {
                                            disable_properties: true,
                                            disable_collapse: true,
                                            disable_edit_json: true
                                        },
                                        properties: {
                                            SoundOnClient: { title: "Sonido en Alarmas  ", type: "boolean", format: "checkbox", default: false }
                                            //Region: { title: "Region / Zona", type: "string" },
                                            //HistoricsDeep: { title: "Dias de Historico", type: "integer", enum: [30, 90, 180, 365], default: 30 },
                                            //refreshTime: { title: 'Tiempo de Refresco en ms', type: "integer" },
                                            //log2con: {title: 'Tracear a consola',type: "string", enum: 
                                            //    ['none','silly','info','warn'], default: 'none'},

                                        },
                                        required: ['SoundOnClient']
                                    },
                                    theme: 'bootstrap2'
                                }
                            );
                            this.elements.content.id = 'srv_cfg';
                        },
                        // This will be called each time an action button is clicked.
                        callback: function (closeEvent) {
                            //The closeEvent has the following properties
                            //
                            // index: The index of the button triggering the event.
                            // button: The button definition object.
                            // cancel: When set true, prevent the dialog from closing.
                            if (closeEvent.index === 0) {
                                // TODO. Salvar las opciones....
                                if (confirm('Desea Salvar los cambios?')) {
                                    $lserv.Sound(editor.getValue().SoundOnClient);
                                }
                                else {
                                    closeEvent.cancel = true;
                                }
                            }
                        }
                    };
                });
            }
            alertify.LocalOptions("Hola").resizeTo(500, 300);
        };

        ctrl.SoundImg = function () {
            var ret = $lserv.Sound() ? "images/speakeron.png" : "images/speakeroff.png";
            console.log(ret);
            return ret;
        };

        ctrl.ToggleSound = function () {
            $lserv.Sound(!$lserv.Sound());
        };

        ctrl.PhoneGlobalState = 0;
        ctrl.PhoneStdClass = function () {
            return GlobalStdClass("btn btn-xs", ctrl.PhoneGlobalState);
        };

        ctrl.RadioGlobalState = 0;
        ctrl.RadioStdClass = function () {
            return GlobalStdClass("btn btn-xs", ctrl.RadioGlobalState);
        };

        function GlobalStdClass(base, std) {
            var stdClass = std == undefined ? "btn-default" :
                std == 0 ? "btn-success" :
                    std == 1 ? "btn-warning" :
                        std == 2 ? "btn-danger" : "btn-danger";
            return base + " " + stdClass;
        }

        /** Funciones o servicios */
        /** */
        function load_inci() {
            /* Obtener el estado del servidor... */
            $serv.listinci_get().then(function (response) {

                if (response.status == 200 && (typeof response) == 'object') {
                    if (new_inci(response.data) == true) {
                        ctrl.listainci = response.data.lista;
                        ctrl.HashCode = response.data.HashCode;
                    }
                    // console.log(ctrl.listainci);
                }
                else {
                    /** El servidor me devuelve errores... */
                    //if (!Simulate) window.open(routeForDisconnect, "_self");
                }
            }
                , function (response) {
                    // Error. No puedo conectarme al servidor.
                    //if (!Simulate) window.open(routeForDisconnect, "_self");
                });
        }

        /** */
        function get_std_gen() {
            $serv.stdgen_get().then(function (response) {

                $lserv.ConfigServerHf(response.data.hf);
                $lserv.ConfigServerSacta(response.data.sct1.enable);
                $lserv.Perfil(response.data.perfil);

                if (userLang != response.data.lang) {
                    userLang = response.data.lang;
                    if (userLang.indexOf("en") == 0)
                        $translate.use('en_US');
                    else if (userLang.indexOf("fr") == 0)
                        $translate.use('fr_FR');
                    else
                        $translate.use('es_ES');
                }

                ctrl.PhoneGlobalState = response.data.tf_status;
                ctrl.RadioGlobalState = response.data.rd_status;

            }
                , function (response) {
                    console.log(response);
                });
        }

        //** */
        function new_inci(newi) {
            if (newi.HashCode != ctrl.HashCode)
                return true;
            return false;
        }

        /** Funcion Periodica del controlador */
        var timer = $interval(function () {
            // ctrl.date=(new Date()).toLocaleDateString();
            // ctrl.hora = (new Date()).toLocaleTimeString();
            ctrl.date = moment().format('ll');
            ctrl.hora = moment().format('LTS');
            if (++timetoinci == InciPoll) {
                load_inci();
                get_std_gen();
                timetoinci = 0;
            }

            /** Control de Audio  en Local */
            if ($lserv.Sound() === true) {
                var x = document.getElementById("myAudio");
                if (ctrl.listainci.length > 0) {
                    if (x.paused == true)
                        x.play();
                }
                else {
                    if (x.paused == false)
                        x.pause();
                }
            }
            /** Control de audio en local */

        }, 1000);

        $scope.$on('$viewContentLoaded', function () {

            /** Alertify */
            alertify.defaults.transition = 'zoom';
            alertify.defaults.glossary = {
                title: $lserv.translate("ULISES V 5000 I. Aplicacion de Mantenimiento"),
                ok: $lserv.translate("Aceptar"),
                cancel: $lserv.translate("Cancelar")
            };

            //call it here
            get_std_gen();

            //// DEBUG.
            //var cifrado = $lserv.cifra("...Hay Hola que tal...?");
            //var descifrado = $lserv.descifra(cifrado);
            //console.log(descifrado + ": " + cifrado);
        });

        /** Salida del Controlador. Borrado de Variables */
        $scope.$on("$destroy", function () {
            $interval.cancel(timer);
        });

    });


