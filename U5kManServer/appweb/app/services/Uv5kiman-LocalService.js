/** */
angular
    .module('Uv5kiman')
    .factory('$lserv', function ($q, $http, $location, $filter, $cookies) {

        /** Para las Traducciones */
        var $translate = $filter('translate');

        var textos_estado = [];
        //    $translate('LSV_STD_00'), // "Sin Informacion",
        //    $translate('LSV_STD_01'), // "Correcto",
        //    $translate('LSV_STD_02'), // "Aviso Reconocido",
        //    $translate('LSV_STD_03'), // "Alarma Reconocida",
        //    $translate('LSV_STD_04'), // "Aviso",
        //    $translate('LSV_STD_05'), // "Alarma",
        //    $translate('LSV_STD_06'), // "Error",
        //    $translate('LSV_STD_07')  // "Reserva"
        //];
        var textos_itfcfg = [];
        //    $translate('LSV_ITF_00'), // "Radio",
        //    $translate('LSV_ITF_01'), // "Linea Caliente",
        //    $translate('LSV_ITF_02'), // "Bateria Central",
        //    $translate('LSV_ITF_03'), // "Bateria Local",
        //    $translate('LSV_ITF_04'), // "Abonado",
        //    $translate('LSV_ITF_05'), // "Linea R2",
        //    $translate('LSV_ITF_06')  // "Linea N5"
        //];
        var textos_recnot = [];
        //    $translate('LSV_NOT_00'), // "",
        //    $translate('LSV_NOT_01'), // "",
        //    $translate('LSV_NOT_02'), // "Radio",
        //    $translate('LSV_NOT_03'), // "Linea Caliente",
        //    $translate('LSV_NOT_04'), // "Telefonia",
        //    $translate('LSV_NOT_05')  // "Telefonia ATS"
        //];

        var user = null;
        var retorno = null;
        var perfil = null;

        var ConfigServer = { hf: false, sacta: false };

        var LocalConfigOnServer = { sound: false };

        var abc = "?$0VxC |KhGcyFS4d-QuoelDn(Iv52#/i:E&BmOL9r{=UfMbtPq7Nw,%1]Y3AsZT)}g_kJ86+aXjH.pz![WR;";

        /** 20171218. Filtros para Historicos y Estadisticas */
        /** Filtro de Estadistica */
        var StsF = {
            dtDesde: moment().startOf('year').millisecond(0).toDate(),
            dtHasta: moment().endOf('day').millisecond(0).toDate(),
            tpMat: "0",
            Mat: ''
        };
        /** Filtro de Historico */
        var LogF = {
            dtDesde: moment().startOf('day').millisecond(0).toDate(),
            dtHasta: moment().endOf('day').millisecond(0).toDate(),
            tpMat: "0",
            Mat: "",
            txt: "",
            limit: default_logs_limit,
            Inci: []
        };

        return {
            StsFilter: function () { return StsF; }
            , LogFilter: function () { return LogF; }
            , translate: function (key) { return $translate(key); }
            , validate: function (tipo, data, max, min) {
                switch (tipo) {
                    case 0:
                        return true;
                    case 1:                     // IP
                        return ip_val(data);
                    case 2:                     // Numerico entre margenes min <= val <= max
                        return (data >= min && data <= max);
                    case 3:                     // Identificador de Frecuencia VHF
                        return vfrec_val(data);
                    case 4:                     // Identificador de Frecuencia UHF
                        return ufrec_val(data);
                    default:
                        return true;
                }
            }
            , date_add_days: function (date, days) {
                var result = new Date(date);
                result.setDate(result.getDate() + days);
                return result;
            }
            , slot_itf: function (itf) {
                return Math.floor(itf / 4).toString() + ":" + (itf % 4).toString();
            }
            , class_estado: function (cfg, not, std) {
                /** Coherencia de los estados notificados y la configuracion */
                switch (not) {
                    case -1:    // Sin Informacion
                        if (cfg != -1)
                            return "std_fallo";
                        return "";
                    case 2:     // Radio
                        if (cfg != 0)
                            return "std_fallo";
                        break;
                    case 3:     // LC
                        if (cfg != 1)
                            return "std_fallo";
                        break;
                    case 4:     // Telefonia PP
                        if (cfg != 2 && cfg != 3 && cfg != 4)
                            return "std_fallo";
                        break;
                    case 5:     // Telefonia ATS (R2/N5)
                        if (cfg != 5 && cfg != 6)
                            return "std_fallo";
                        break;
                    default:
                        return "std_fallo";
                }
                /** Estado del Recurso */
                if (std != 1)
                    return "std_fallo";
            }
            , texto_estado: function (std) {

                if (std > 7)
                    return $translate('LSV_STD_08')/*"Desconocido*/ + ": " + std;
                else {
                    if (textos_estado.length == 0) {
                        textos_estado = [
                            $translate('LSV_STD_00'), // "Sin Informacion",
                            $translate('LSV_STD_01'), // "Correcto",
                            $translate('LSV_STD_02'), // "Aviso Reconocido",
                            $translate('LSV_STD_03'), // "Alarma Reconocida",
                            $translate('LSV_STD_04'), // "Aviso",
                            $translate('LSV_STD_05'), // "Alarma",
                            $translate('LSV_STD_06'), // "Error",
                            $translate('LSV_STD_07')  // "Reserva"
                        ];
                    }
                    return textos_estado[std];
                }
            }
            , texto_recurso_cfg: function (rec) {
                if (rec == 50)
                    return $translate('LSV_ITF_07')/*"EyM tipo 0"*/;
                else if (rec == 51)
                    return $translate('LSV_ITF_08')/*"EyM tipo 1"*/;
                else if (rec == -1)
                    return $translate('LSV_ITF_09')/*"No Configurado"*/;
                else if (rec <= 6) {
                    if (textos_itfcfg.length == 0) {
                        textos_itfcfg = [
                            $translate('LSV_ITF_00'), // "Radio",
                            $translate('LSV_ITF_01'), // "Linea Caliente",
                            $translate('LSV_ITF_02'), // "Bateria Central",
                            $translate('LSV_ITF_03'), // "Bateria Local",
                            $translate('LSV_ITF_04'), // "Abonado",
                            $translate('LSV_ITF_05'), // "Linea R2",
                            $translate('LSV_ITF_06')  // "Linea N5"
                        ];
                    }
                    return textos_itfcfg[rec];
                }
                else if (rec == 13) {
                    return $translate('EyM');
                }
                else
                    return sprintf($translate("Desconocido: %1$d"), rec);
            }
            , texto_recurso_notificado: function (rec) {
                if (rec == -1)
                    return $translate('LSV_STD_00')/*"Sin Informacion"*/;
                else if (rec >= 2 && rec <= 5) {
                    if (textos_recnot.length == 0) {
                        textos_recnot = [
                            $translate('LSV_NOT_00'), // "",
                            $translate('LSV_NOT_01'), // "",
                            $translate('LSV_NOT_02'), // "Radio",
                            $translate('LSV_NOT_03'), // "Linea Caliente",
                            $translate('LSV_NOT_04'), // "Telefonia",
                            $translate('LSV_NOT_05')  // "Telefonia ATS"
                        ];
                    }
                    return textos_recnot[rec];
                }
                else
                    return $translate('LSV_STD_08')/*"Desconocido*/ + ": " + rec;
            }
            , texto_tipogw: function (tp) {
                if (tp === 0)
                    return $translate('LVS_TYP_00')/*"Simple"*/;
                else if (tp === 1) {
                    return gwDualityType() == 0 ? $translate('LVS_TYP_01')/*"CPU Dual"*/ : $translate('LVS_TYP_02')/*"ITF Dual"*/;
                }
                else if (tp === 2)
                    return $translate('LVS_TYP_02')/*"ITF Dual"*/;
                else if (tp == "")
                    return "";
                else
                    return $translate('LSV_STD_08')/*"Desconocido*/ + ": " + tp;
            }
            , class_okfallo: function (std) {
                if (std === false)
                    return "std_fallo";
                return "";
            }
            , texto_okfallo: function (std) {
                if (std === 1)
                    return $translate('SCT_MSG_00')/*"Ok"*/;
                else if (std === 0)
                    return $translate('SCT_MSG_01')/*"Fallo"*/;
                else if (std == "")
                    return "";
                else
                    return $translate('LSV_STD_08')/*"Desconocido*/ + ": " + std;
            }
            , texto_inservice_outservice: function (std) {
                if (std === 1)
                    return $translate('Ok');
                else if (std === 0)
                    return $translate('--');
                return '??';
            }
            , inci_filter_pict: function (inci) {
                return (inci.id >= 1000 && inci.id < 2000);
            }
            , inci_filter_pasa: function (inci) {
                return (inci.id >= 2000 && inci.id < 3000);
            }
            , inci_filter_radio_mn: function (inci) {
                return (inci.id >= 3050 && inci.id < 3200);
            }
            , inci_filter_radio_hf: function (inci) {
                return (inci.id < 50);
            }
            , inci_filter_gen: function (inci) {
                return (inci.id >= 50 && inci.id < 1000) ||
                    (inci.id >= 3000 && inci.id < 3050);
            }
            , inci_filter_genonly: function (inci) {
                return (inci.id >= 50 && inci.id < 1000);
            }
            , inci_filter_ext: function (inci) {
                return (inci.id >= 3000 && inci.id < 3050);
            }
            , inci_filter_all: function (inci) {
                return (false);
            }
            , inci_filter_one: function (inci) {
                return this == inci.id;
            }
            , exteq_filter_all: function (exteq) {
                return true;
            }
            , exteq_filter_tel: function (exteq) {
                return exteq.tipo == 3;
            }
            , exteq_filter_tx: function (exteq) {
                return exteq.tipo == 2 && (exteq.txrx == 5 || exteq.txrx == 1 || exteq.txrx == 2 || exteq.txrx == 3 || exteq.txrx == 6);
            }
            , exteq_filter_rx: function (exteq) {
                return exteq.tipo == 2 && (exteq.txrx == 4 || exteq.txrx == 0 || exteq.txrx == 2 || exteq.txrx == 6);
            }
            , exteq_filter_rec: function (exteq) {
                return exteq.tipo == 5;
            }
            , html_decode: function (str) {
                return html(str).text();
            }
            //, url_param_value_get: function (name) {
            //    if (name = (new RegExp('[?&]' + encodeURIComponent(name) + '=([^&]*)')).exec(location.search))
            //        return decodeURIComponent(name[1]);
            //    return "???";
            //}
            , check_access_old: function () {

                //if (user != null && retorno != null)
                //    return;
                var path = $location.path();
                var params = $location.search();
                console.log(params);

                if (user == null) {
                    if (!params.hasOwnProperty('user')) {
                        //location.href = "/error.html";
                        //return;
                        user = "Init";
                    }
                    else
                        user = params.user;
                }

                console.log("user= " + user);

                if (retorno == null &&
                    params.hasOwnProperty('RetUrl'))
                    retorno = params.RetUrl;

                console.log("retorno= " + retorno);
            }
            , check_access_v1: function () {
                var path = $location.path();
                var params = $location.search();

                console.log(params);
                if (params.hasOwnProperty('user')) {
                    $cookies.put("my-user", params.user);
                }
                else {
                    if ($cookies.get("my-user") == null) {
                        $cookies.put('my-user', "Init");
                    }
                    //else {
                    //}
                }
                console.log("user= " + $cookies.get("my-user"));

                if (params.hasOwnProperty('RetUrl')) {
                    $cookies.put('my-ret', params.RetUrl);
                }
                console.log("retorno= " + $cookies.get('my-ret'));
            }
            , check_access: function () {
                var path = $location.path();
                var params = $location.search();

                console.log(params);
                if (params.hasOwnProperty('user')) {
                    ndfls("user", params.user);
                }
                else {
                    if (ndfls("user") == null) {
                        ndfls('user', "Init");
                    }
                    //else {
                    //}
                }
                console.log("user= " + ndfls("user"));

                if (params.hasOwnProperty('RetUrl')) {
                    ndfls('ret', params.RetUrl);
                }
                console.log("retorno= " + ndfls('ret'));
            }
            , User: function () {
                return /*user$*//*cookies.get("my-user")*/ndfls("user");
            }
            , Retorno: function () {
                return /*retorno*//*$cookies.get("my-ret")*/ndfls("ret");
            }
            , Perfil: function (perf) {
                if (perf != undefined) perfil = perf;
                return perfil;
            }
            , ConfigServerHf: function (hf) {
                if (hf)
                    ConfigServer.hf = hf;
                return ConfigServer.hf == 1;
            }
            , ConfigServerSacta: function (sacta) {
                if (sacta)
                    ConfigServer.sacta = sacta;
                return ConfigServer.sacta == 1;
            }
            , cifra: function (mensaje) {
                return CAE_cifrar(mensaje, 47);
            }
            , descifra: function (mensaje) {
                return CAE_descifrar(mensaje, 47);
            }
            , Menu: function (menu) {
                return ndfls('menu', menu);
            }
            , Submenu: function (menu) {
                return ndfls('submenu', menu);
            }
            /** Control de Audio  en Local */
            , Sound: function (sound) {
                if (sound != undefined) {
                    localStorage.setItem("Uv5kSound", sound);
                }
                var SoundOnClient = localStorage.getItem("Uv5kSound");
                LocalConfigOnServer.sound = SoundOnClient === null ? false : SoundOnClient === 'true';
                return LocalConfigOnServer.sound;
            }
            , GatewayDualityType: function (newtype) {
                return gwDualityType(newtype);
            }
        };

        //** */
        function CAE_cifrar(mensaje, desplazamiento) {
            var cifrado = "";
            desplazamiento = (!desplazamiento || desplazamiento < 0 || desplazamiento >= abc.length) ? 47 : desplazamiento;
            $.each(mensaje.split(''), function (i, m) {
                var index = abc.indexOf(m);
                index = index >= 0 ? index + desplazamiento : index;
                index = index >= abc.length ? index - abc.length : index;
                cifrado += index >= 0 ? abc[index] : m;
            });
            return btoa(cifrado);
        }
        function CAE_descifrar(cifrado, desplazamiento) {
            cifrado = atob(cifrado);
            var descifrado = "";
            desplazamiento = (!desplazamiento || desplazamiento < 0 || desplazamiento >= abc.length) ? 47 : desplazamiento;
            $.each(cifrado.split(''), function (i, m) {
                var index = abc.indexOf(m);
                if (index >= 0) {
                    index -= desplazamiento;
                    index = index < 0 ? index + abc.length : index;
                    descifrado += abc[index];
                }
                else
                    descifrado += m;
            });
            return descifrado;
        }

        //** */
        function ip_val(value) {
            if (value != "" && value.match(regx_ipval) == null)
                return false;
            return true;
        }

        //** XXX.YZ */
        function vfrec_val(value) {
            return value.match(regx_fid_vhf) != null;
        }

        //** XXX.YZ */
        function ufrec_val(value) {
            return value.match(regx_fid_uhf) != null;
        }

        /** Gestion de la cookie ndfls (almacenamiento local)*/
        //** Formato es p1=v1##p2=v2## ... */
        function ndfls(key, value) {
            var found = false;
            var lsg = $cookies.get("ndfls");
            if (!lsg) lsg = "";
            var keyvalues = lsg.split("##");

            $.each(keyvalues, function (i, val) {
                var pair = val.split("=");
                if (pair.length == 2 && pair[0] == key) {
                    /** Cambio el valor */
                    if (value != undefined)
                        keyvalues[i] = pair[0] + "=" + value;
                    found = value ? value : pair[1];
                    return;
                }
            });

            if (!found && value) {
                keyvalues.push(key + "=" + value);
                found = value;
            }

            if (value != undefined) {
                var new_lsg = "";
                $.each(keyvalues, function (i, val) {
                    new_lsg += (val + "##");
                });

                $cookies.put("ndfls", new_lsg.length < 2 ? new_lsg : new_lsg.substring(0, new_lsg.length - 2));
            }

            return found;
        }


        function gwDualityType(newtype) {

            if (newtype != undefined) {
                if (newtype === 0) {
                    localStorage.GwDualityType = 0;
                }
                else {
                    localStorage.GwDualityType = 1;
                }
            }
            return localStorage.GwDualityType == undefined ? 0 : localStorage.GwDualityType;
        }

    });

