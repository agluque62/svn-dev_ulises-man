angular.module("Uv5kiman")
.controller("uv5kiTlfCtrl", function ($scope, $interval, $serv, $lserv) {
    /** Inicializacion */
    var ctrl = this;
    //ctrl.pagina = 0;
    ctrl.pagina = function (pagina) {
        var menu = $lserv.Submenu(pagina);
        return menu ? menu : 0;
    };
    ctrl.info = {};
    ctrl.info.tifx = [];
    ctrl.info.pbx = [];
    ctrl.info.proxies = {};
    ctrl.info.extats = [];
    /** */
    ctrl.dep = [];
    ctrl.selectedDep = "";

    ctrl.txtNRecursos = function (cgw) {
        return $lserv.translate("Recursos") + " (" + cgw.res.length + ")";
    };

    ctrl.txtNAbonados = function (cgw) {
        return $lserv.translate("Abonados") + " (" + cgw.res.length + ")";
    };

    //** */
    ctrl.ProxyTypeText = function (tp) {
        if (tp == 5)
            return $lserv.translate("Local") + "-" + $lserv.translate("Principal");
        else if (tp == 6)
            return $lserv.translate("Local") + "-" + $lserv.translate("Alternativo");
        else if (tp == 7)
            return $lserv.translate("Externo") + "-" + $lserv.translate("Principal");
        else if (tp == 8)
            return $lserv.translate("Externo") + "-" + $lserv.translate("Alternativo");

        return $lserv.translate("Desconocido");
    };

    ctrl.ProxyStatusText = function (tp) {
        if (tp == 3)
            return $lserv.translate("No Disponible");
        else if (tp == 0)
            return $lserv.translate("Disponible");

        return $lserv.translate("Desconocido");
    };

    ctrl.ProxyStatusColor = function (tp) {
        if (tp == 3)
            return "text-danger";
        else if (tp == 0)
            return "text-success";

        return "bg-danger text-primary";
    };

    ctrl.Uri = function (item) {
        return "<sip:" + item.id + "@" + item.dep + ">";
    };

    /** */
    /** Servicios Pagina TIFX */
    /** */
    function tlfTifxDataGet() {
        $serv.tifxinfo_get().then(function (response) {
            console.log(response.data);
            if (tlfTifxDataChanged(response.data) == true) {
                ctrl.info.tifx = response.data;
                extractDep();
            }
        }
        , function (response) {
            console.log(response);
        });

    }

    //** */
    function tlfTifxDataChanged(data) {
        if (data.constructor === Array && ctrl.info.tifx.constructor === Array) {
            /** Deben ser iguales */
            if (data.length != ctrl.info.tifx.length)
                return true;
            /** ... y en el mismo orden */
            for (i = 0; i < data.length; i++) {
                var nuevo = angular.toJson(data[i]);
                var viejo = angular.toJson(ctrl.info.tifx[i]);
                if (nuevo != viejo)
                    return true;
            }
        }
        return false;
    }

    /** */
    function extractDep() {
        ctrl.dep = [];
        $.each(ctrl.info.tifx, function (ind1, grp) {
            if (grp.tp == 3) {
                $.each(grp.res, function (ind2, item) {
                    var depExist = ctrl.dep.indexOf(item.dep) > -1;
                    if (depExist == false)
                        ctrl.dep.push(item.dep);
                });
            }
        });
        ctrl.selectedDep = ctrl.dep.length > 0 ? ctrl.dep[0] : "";
    }

    /** Servicios Pagina PBX */

    /** Funciones Locales */

    /** Funcion Periodica del controlador */
    tlfTifxDataGet();
    var timer = $interval(function () {
        tlfTifxDataGet();
    }, pollingTime);

    /** Salida del Controlador. Borrado de Variables */
    $scope.$on("$destroy", function () {
        $interval.cancel(timer);
    });
});
