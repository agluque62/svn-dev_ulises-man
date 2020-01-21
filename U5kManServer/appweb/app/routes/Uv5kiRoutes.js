/** */
angular
    .module('Uv5kiman')
    .config(config);

function config($routeProvider) {
    $routeProvider
        .when(routeDefault, {
            templateUrl: 'app/views/Uv5kiman-superv.html',
            controller: 'uv5kiSupervCtrl',
            controllerAs: 'ctrl',
            resolve: {
                access: function ($lserv) {
                    $lserv.check_access();
                    $lserv.Menu(0);
                    $lserv.Submenu(0);
                }
            }
        })
        .when(routeOpeSp, {
            templateUrl: 'app/views/Uv5kiman-ope.html',
            controller: 'uv5kiOpeSpCtrl',
            controllerAs: 'ctrl',
            resolve: {
                access: function ($lserv) {
                    $lserv.check_access();
                    $lserv.Menu(1);
                    $lserv.Submenu(0);
                }
            }
        })
        .when(routeTifxSp, {
            templateUrl: 'app/views/Uv5kiman-tlf.html',
            controller: 'uv5kiTlfCtrl',
            controllerAs: 'ctrl',
            resolve: {
                access: function ($lserv) {
                    $lserv.check_access();
                    $lserv.Menu(4);
                    $lserv.Submenu(0);
                }
            }
        })
        .when(routeConfig, {
            templateUrl: 'app/views/Uv5kiman-config.html',
            controller: 'uv5kiConfigCtrl',
            controllerAs: 'ctrl',
            resolve: {
                access: function ($lserv) {
                    $lserv.check_access();
                    $lserv.Menu(3);
                    $lserv.Submenu(0);
                }
            }
        })
        .when(routeHist, {
            templateUrl: 'app/views/Uv5kiman-hist.html',
            controller: 'uv5kiHistCtrl',
            controllerAs: 'ctrl',
            resolve: {
                access: function ($lserv) {
                    $lserv.check_access();
                    $lserv.Menu(2);
                    $lserv.Submenu(0);
                }
            }
        })
        .when(routeManual, {
            templateUrl: 'doc/Manual_de_Usuario.htm',
            resolve: {
                access: function ($lserv) {
                    $lserv.check_access();
                }
            }
        })
        .when(routeNoaccess, {
            templateUrl: './error.html'
            // templateUrl: 'session-expired.html'
        }
        );
}

