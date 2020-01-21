/** Variables Globales */
var Simulate = location.port == 1445;
var pollingTime = 5000;
var maxPreconf = 8;
var InciPoll = 10;
var userLang = navigator.language;
var default_logs_limit = "2000";
var nbx_splitted = true;
var max_number = 65536;

/** */
var Uv5kiman = angular.module('Uv5kiman',
    ['ngRoute', 'ngCookies', 'ngSanitize', 'ui.bootstrap', 'smart-table', 'pascalprecht.translate']);

/** */
Uv5kiman.config(function ($translateProvider) {
    // Our translations will go in here
    $translateProvider.useStaticFilesLoader({
        prefix: '/languages/',
        suffix: '.json'
    });

    // var userLang = navigator.language[0];
    // userLang = navigator.language;

    if (userLang.indexOf("en") == 0)
        $translateProvider.use('en_US');
    else if (userLang.indexOf("fr") == 0)
        $translateProvider.use('fr_FR');
    else
        $translateProvider.use('es_ES');
    
    /** Configuraicion de las fechas a traves de momment */
    /** Español */
    moment.locale('es', {
        months : 'enero_febrero_marzo_abril_mayo_junio_julio_agosto_septiempbre_octubre_noviembre_diciembre'.split('_'),
        monthsShort : 'ene._feb._mar._abr._may._jun._jul._ago._sep._oct._nov._dic.'.split('_'),
        monthsParseExact : true,
        weekdays : 'domingo_lunes_martes_miercoles_jueves_viernes_sabado'.split('_'),
        weekdaysShort : 'dom._lun._mar._mie._jue._vie._sab.'.split('_'),
        weekdaysMin : 'Do_Lu_Ma_Mi_Ju_Vi_Sa'.split('_'),
        weekdaysParseExact : true,
        longDateFormat : {
            LT : 'HH:mm',
            LTS : 'HH:mm:ss',
            L : 'DD/MM/YYYY',
            LL : 'D MMMM YYYY',
            LLL : 'D MMMM YYYY HH:mm',
            LLLL : 'dddd D MMMM YYYY HH:mm'
        },
        calendar : {
            sameDay : '[Hoy a las] LT',
            nextDay : '[Mañana a las] LT',
            nextWeek : 'dddd [a las] LT',
            lastDay : '[Ayer a las] LT',
            lastWeek : 'dddd [anterior a las] LT',
            sameElse : 'L'
        },
        relativeTime : {
            future : 'en %s',
            past : 'hace %s',
            s : 'algunos segundos',
            m : 'un minuto',
            mm : '%d minutos',
            h : 'una hora',
            hh : '%d horas',
            d : 'un dia',
            dd : '%d dias',
            M : 'un mes',
            MM : '%d meses',
            y : 'un año',
            yy : '%d años'
        },
        dayOfMonthOrdinalParse : /\d{1,2}(er|e)/,
        ordinal : function (number) {
            return number + (number === 1 ? 'er' : 'e');
        },
        meridiemParse : /PM|AM/,
        isPM : function (input) {
            return input.charAt(0) === 'M';
        },
        // In case the meridiem units are not separated around 12, then implement
        // this function (look at locale/id.js for an example).
        // meridiemHour : function (hour, meridiem) {
        //     return /* 0-23 hour, given meridiem token and hour 1-12 */ ;
        // },
        meridiem : function (hours, minutes, isLower) {
            return hours < 12 ? 'PM' : 'AM';
        },
        week : {
            dow : 1, // Monday is the first day of the week.
            doy : 4  // The week that contains Jan 4th is the first week of the year.
        }
    });
    /** Francés */
    moment.locale('fr', {
        months : 'janvier_février_mars_avril_mai_juin_juillet_aout_septembre_octobre_novembre_decembre'.split('_'),
        monthsShort : 'janv._fevr._mars_avr._mai_juin_juil._aout_sept._oct._nov._dec.'.split('_'),
        monthsParseExact : true,
        weekdays : 'dimanche_lundi_mardi_mercredi_jeudi_vendredi_samedi'.split('_'),
        weekdaysShort : 'dim._lun._mar._mer._jeu._ven._sam.'.split('_'),
        weekdaysMin : 'Di_Lu_Ma_Me_Je_Ve_Sa'.split('_'),
        weekdaysParseExact : true,
        longDateFormat : {
            LT : 'HH:mm',
            LTS : 'HH:mm:ss',
            L : 'DD/MM/YYYY',
            LL : 'D MMMM YYYY',
            LLL : 'D MMMM YYYY HH:mm',
            LLLL : 'dddd D MMMM YYYY HH:mm'
        },
        calendar : {
            sameDay : '[Aujourd’hui a] LT',
            nextDay : '[Demain a] LT',
            nextWeek : 'dddd [a] LT',
            lastDay : '[Hier a] LT',
            lastWeek : 'dddd [dernier a] LT',
            sameElse : 'L'
        },
        relativeTime : {
            future : 'dans %s',
            past : 'il y a %s',
            s : 'quelques secondes',
            m : 'une minute',
            mm : '%d minutes',
            h : 'une heure',
            hh : '%d heures',
            d : 'un jour',
            dd : '%d jours',
            M : 'un mois',
            MM : '%d mois',
            y : 'un an',
            yy : '%d ans'
        },
        dayOfMonthOrdinalParse : /\d{1,2}(er|e)/,
        ordinal : function (number) {
            return number + (number === 1 ? 'er' : 'e');
        },
        meridiemParse : /PD|MD/,
        isPM : function (input) {
            return input.charAt(0) === 'M';
        },
        // In case the meridiem units are not separated around 12, then implement
        // this function (look at locale/id.js for an example).
        // meridiemHour : function (hour, meridiem) {
        //     return /* 0-23 hour, given meridiem token and hour 1-12 */ ;
        // },
        meridiem : function (hours, minutes, isLower) {
            return hours < 12 ? 'PD' : 'MD';
        },
        week : {
            dow : 1, // Monday is the first day of the week.
            doy : 4  // The week that contains Jan 4th is the first week of the year.
        }
    });
    /** Formato de fecha segun el lenguaje del navegador*/
    moment.locale(userLang.indexOf("en") == 0 ? "en" : userLang.indexOf("fr") == 0 ? "fr" : "es");

});

//** */
 Uv5kiman.directive('fileModel', ['$parse', function ($parse) {
     return {
         restrict: 'A',
         link: function (scope, element, attrs) {
             var model = $parse(attrs.fileModel);
             var modelSetter = model.assign;
             element.bind('change', function () {
                 scope.$apply(function () {
                     modelSetter(scope, element[0].files[0]);
                 });
             });
         }
     };
 }]);

/** */
 Uv5kiman.directive('bsPopover', function () {
     return function (scope, element, attrs) {
         element.popover({ placement: 'top', html: 'true', trigger: 'manual' });
     };
 });

/** Directivas de Validacion INPUT */
/** Numero entre max y  min */
 Uv5kiman.directive('nuNumber', function () {
     return {
         require: 'ngModel',
         link: function (scope, element, attr, mCtrl) {
             function myValidation(value) {
                 attr.min = !attr.min ? 1 : parseInt(attr.min);
                 attr.max = !attr.max ? max_number : parseInt(attr.max);
                 if (!(new RegExp("^[0-9]*$")).test(value)) {
                     if (mCtrl.$valid) {
                         mCtrl.$setValidity('maxminNumber', false);
                         alertify.error(/*$lserv.translate(*/"El valor introducido debe ser un numero entre " + attr.min + " y " + attr.max/*)*/);
                     }
                 }
                 else {
                     var ivalue = parseInt(value);
                     if (ivalue < attr.min || ivalue > attr.max) {
                         if (mCtrl.$valid) {
                             mCtrl.$setValidity('maxminNumber', false);
                             alertify.error(/*$lserv.translate(*/"El valor introducido debe ser un numero entre " + attr.min + " y " + attr.max/*)*/);
                         }
                     }
                     else {
                         mCtrl.$setValidity('maxminNumber', true);
                     }
                 }
                 return value;
             }
             mCtrl.$parsers.push(myValidation);
         }
     };
 });

 Uv5kiman.directive('nuIpadd', function () {
     return {
         require: 'ngModel',
         link: function (scope, element, attr, mCtrl) {
             function myValidation(value) {
                 if (value.match(regx_ipval) == null) {
                     if (mCtrl.$valid) {
                         mCtrl.$setValidity('ipval', false);
                         alertify.error(/*$lserv.translate(*/"El valor introducido no tiene formato IP-V4"/*)*/);
                     }
                 }
                 else {
                     mCtrl.$setValidity('ipval', true);
                 }
                 return value;
             }
             mCtrl.$parsers.push(myValidation);
         }
     };
 });

 Uv5kiman.directive('nuNumberlist', function () {
     return {
         require: 'ngModel',
         link: function (scope, element, attr, mCtrl) {
             function myValidation(value) {
                 var elements = value.split(",");
                 var min = !attr.min ? 1 : parseInt(attr.min);
                 var max = !attr.max ? max_number : parseInt(attr.max);

                 var isValid = true;
                 $.each(elements, function (index, item) {
                     if (!(new RegExp("^[0-9]*$")).test(item)) {
                         isValid = false;
                     }
                     else {
                         var ivalue = parseInt(item);
                         if (ivalue < min || ivalue > max) {
                             isValid = false;
                         }
                     }
                 });
                 if (!isValid) {
                     if (mCtrl.$valid) {
                         mCtrl.$setValidity('nlist', false);
                         alertify.error(/*$lserv.translate(*/"El valor introducido debe ser una lista de numeros mayores de " + min + " y menores de " + max + " separados por comas" /*)*/);
                     }
                 }
                 else {
                     mCtrl.$setValidity('nlist', true);
                 }
                 return value;
             }
             mCtrl.$parsers.push(myValidation);
         }
     };
 });

/** Icono para los Informes de historicos.*/
var imgData = "data:image/png;base64,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";

/** Rutas de Aplicacion */
var routeDefault = "/";
var routeConfig = "/config";
var routeOpeSp = "/opesp";
var routeTifxSp = "/tifxsp";
var routeHist = "/hist";
var routeNoaccess = "/noaccess";
var routeManual = "/manual";

/** Peticiones REST */
var rest_url_listinci = "/listinci";        // GET & POST
var rest_url_std = "/std";                  // GET
var rest_url_cwps = "/cwp";                 // GET
var rest_url_exteq = "/exteq";              // GET
var rest_url_pbxab = "/pbxab";              // GET
var rest_url_gws = "/gws";                  // GET /gws, GET /gws/name, POST /gws/name {cmd: (getVersion, chgPR)}
var rest_url_db_ops = "/db/operadores";     // GET 
var rest_url_db_gws = "/db/pasarelas";      // GET
var rest_url_db_mni = "/db/mnitems";        // GET
var rest_url_db_inc = "/db/incidencias";    // GET & POST.
var rest_url_db_his = "/db/historicos";     // POST para enviar el Filtro...
var rest_url_options = "/options";          // GET & POST
var rest_url_snmpoptions = "/snmpopts";     // GET & POST
var rest_url_db_est = "/db/estadistica";    // POST para poder enviar el Filtro.

var rest_url_preconf = "/preconf";
var rest_url_local_config = "/lconfig";

var rest_url_radio_sessions = "/rdsessions";
var rest_url_radio_gestormn = "/gestormn";
var rest_url_radio_gestormn_habilita = "/gestormn/enable";
var rest_url_radio_gestormn_asigna = "/gestormn/assign";
var rest_url_radio_hf = "/rdhf";
var rest_url_tifx_info = "/tifxinfo";

var rest_url_sacta = "/sacta";
var rest_url_extatsdest = "/extatssest";

var rest_url_versiones = "/versiones";

var rest_url_allhard = "/allhard";
var rest_url_reset = "/reset";

var routeForDisconnect = "/noserver.html";


/** Codigo de Estados */
var stdc = {
    NoInfo: 0,
    Ok: 1,
    AvisoReconocido: 2,
    AlarmaReconocida: 3,
    Aviso: 4,
    Alarma: 5,
    Error: 6,
    Reserva: 7,
    Principal: 8
};

/** Tipos de Interfaces en Configuracion */
var itft = {
    rcNotipo: -1,
    rcRadio: 0,
    rcLCE: 1,
    rcPpBC: 2,
    rcPpBL: 3,
    rcPpAB: 4,
    rcAtsR2: 5,
    rcAtsN5: 6,
    rcPpEM: 50,
    rcPpEMM: 51
};

/** Tipos notificados de Recursos */
var rcnt = {
    rcNotipo: -1,
    rcRadio: 2,
    rcLCE: 3,
    rcTLF: 4,
    rcATS: 5
};

/** Clases segun el codigo de estado */
var stdc_class = ["noinfo", "ok", "aviso", "fallo", "aviso", "fallo", "fallo", "rsva", "ppal"];

/** */
var roles = {
	ADMIN_PROFILE: 64,
	ING_PROFILE: 32,
	GEST_POFILE: 16,
	CRTL_PROFILE: 8,
	ALM1_PROFILE: 4,
	ALM2_PROLIFLE: 2,
	VIS_PROFILE: 1
};
/** */
var routeForUnauthorizedAccess = '/noaut';

/** Validadores. */
var regx_ipval = /^(([0-9]|[1-9][0-9]|1[0-9]{2}|2[0-4][0-9]|25[0-5])\.){3}([0-9]|[1-9][0-9]|1[0-9]{2}|2[0-4][0-9]|25[0-5])$/;
var regx_trpval = /^[1-2]+,(([0-9]|[1-9][0-9]|1[0-9]{2}|2[0-4][0-9]|25[0-5])\.){3}([0-9]|[1-9][0-9]|1[0-9]{2}|2[0-4][0-9]|25[0-5])\/[0-9]{2,5}$/;
var regx_atsrango = /^[0-9]{6}-[0-9]{6}$/;
var regx_atsnumber = /^[0-9]{6}$/;
var regx_urlval = /^(http(?:s)?\:\/\/[a-zA-Z0-9]+(?:(?:\.|\-)[a-zA-Z0-9]+)+(?:\:\d+)?(?:\/[\w\-]+)*(?:\/?|\/\w+\.[a-zA-Z]{2,4}(?:\?[\w]+\=[\w\-]+)?)?(?:\&[\w]+\=[\w\-]+)*)$/;
var regx_ipportval = /^(([0-9]|[1-9][0-9]|1[0-9]{2}|2[0-4][0-9]|25[0-5])\.){3}([0-9]|[1-9][0-9]|1[0-9]{2}|2[0-4][0-9]|25[0-5])(:[\d]{1,5})?$/;
var regx_urival = /^sip:(.+)@(([0-9]|[1-9][0-9]|1[0-9]{2}|2[0-4][0-9]|25[0-5])\.){3}([0-9]|[1-9][0-9]|1[0-9]{2}|2[0-4][0-9]|25[0-5])(:[\d]{1,5})?$/;
var regx_fid = /^(1|2|3)[0-9]{2}\.[0-9]{2}(0|5)$/;
var regx_fid_vhf = /^(1)(1|2|3)([0-9]{1})\.([0-9]{2})(0|5)$/;   /** 118.000 137.00 */
var regx_fid_uhf = /^(2|3|4)([0-9]{2})\.([0-9]{2})(0|5)$/;      /** 225.00 400.00 */

