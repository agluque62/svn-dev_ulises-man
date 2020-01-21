angular.module("Uv5kiman")
.controller("uv5kiHistCtrl", function ($scope, $interval, $serv, $lserv) {
    /** Inicializacion */
    var ctrl = this;

    ctrl.portrait = false;

    /** Para que accedan el HTML a los valores del servicio */
    ctrl.ls = $lserv;

    /** */
    ctrl.itemsPage=20;
    ctrl.lhis = [];     // Lista de Historicos.
    ctrl.dlhis = [].concat(ctrl.lhis); 
    
    ctrl.linci = [];    // Lista de Incidencias.
    ctrl.pict = [];     // Lista de Operadores.
    ctrl.pasa = [];     // Lista de Pasarelas.
    ctrl.mni = [];      // Lista Items MN

    /** */
    ctrl.show_hf = $lserv.ConfigServerHf();

    // ctrl.pagina = 0;
    /** Control de la Pagina */
    //ctrl.setPagina = function (pag) {
    //    ctrl.pagina = pag;
    //    filtro_reset(pag);
    //}
    ctrl.pagina = function (pagina) {
        //if (pagina != undefined)
        //    filtro_reset(pagina);
        var menu = $lserv.Submenu(pagina);
        return menu ? menu : 0;
    };

    /** Opciones DPicker */
    ctrl.dpOptions = {
        showWeeks: false,
        startingDay: 1,
        maxDate: new Date()
    };

    /** Control del Filtro de Historico*/
    // filtro_reset(ctrl.pagina());
    //ctrl.fil = {
    //    dtDesde: moment().startOf('day').toDate(),
    //    dtHasta: moment().startOf('minute').toDate(),
    //    tpMat: "0",
    //    Mat: "",
    //    txt: "",
    //    Inci: []
    //};

    /** Control de las Fechas */
    ctrl.dtDesdeOpen = false;
    ctrl.dtHastaOpen = false;
    ctrl.openCalendarDesde = function(dtp) {
        if (dtp==='dpDesde')
            ctrl.dtDesdeOpen = true;
        else if (dtp==='dpHasta')
            ctrl.dtHastaOpen = true;
    };

    /** Control del Material segun el tipo */
    ctrl.mat=[];
    ctrl.inci=[];
    ctrl.OnTpMatChange = function () {
        fhis_prepare(true);
    };

    //** Obtiene el Historico del Servidor */
    function LogFilterNormalize(filter) {
        var now = moment().endOf('day').toDate();
        if (filter.dtDesde > now ||
            filter.dtHasta > now) {
            alertify.alert($lserv.translate('Error en la Seleccion'), $lserv.translate('La fecha inicial o la fecha final está en el futuro!'));
            return false;
        }
        else if (filter.dtDesde > filter.dtHasta) {
            alertify.alert($lserv.translate('Error en la Seleccion'), $lserv.translate('La fecha inicial es mayor que la fecha Final!'));
            return false;
        }
        return true;

    }
  ctrl.getHistorico = function () {
      console.log(ctrl.ls.LogFilter());
      ctrl.clearhis();
      if (LogFilterNormalize(ctrl.ls.LogFilter()) == true) {
          $("body").css("cursor", "progress");
          $serv.db_hist_get(ctrl.ls.LogFilter()).then(
              function (response) {
                  ctrl.lhis = response.data.lista;
                  $("body").css("cursor", "default");
                  if (ctrl.lhis.length == 0) {
                      alertify.warning($lserv.translate("No se han encontrado registros"));
                  }
              },
              function (response) {
                  $("body").css("cursor", "default");
                  console.log(response);
                  ctrl.lhis = [];
                  alertify.success($lserv.translate("Error en Consulta de Base de Datos."));
              });
      }
    };

    //** */
    ctrl.hist_reg_txt = function () {
        var nreg = ctrl.lhis.length;
        var npag = nreg == 0 ? 0 : Math.ceil(nreg / ctrl.itemsPage);
        return sprintf($lserv.translate("Registros: %1$d, Paginas: %2$d"), nreg, npag);
    };

    /** Limpia la Lista de Incidencias */
    ctrl.linci_clear = function () {
        filtro_reset(ctrl.pagina());
        document.getElementById("list_inci").selectedIndex = "-1";
        //ctrl.fil.Inci = [];
        //ctrl.fil.txt = "";
    };

    /** Accede a Configurar el Filtro de Historicos */
    ctrl.setfiltro = function () {
        /** */
        dateRangeUpdate();
        $("#hfiltroModal").modal("show");
    };

    //** */
    ctrl.textoInciSel = function () {
        if (ctrl.linci.length == 0) return "";

        var txt = (ctrl.ls.LogFilter().Inci.length == 0 ? "" :
            ctrl.ls.LogFilter().Inci.length == 1 ? ctrl.linci.filter($lserv.inci_filter_one, ctrl.ls.LogFilter().Inci[0])[0].desc :
                ctrl.linci.filter($lserv.inci_filter_one, ctrl.ls.LogFilter().Inci[0])[0].desc + ", ...");
        return txt;
    };

    //** */
    ctrl.textoTexto = function () {
        return ctrl.ls.LogFilter().txt == undefined ? "" : ctrl.ls.LogFilter().txt;
    };

    //** */
    ctrl.textoGrpSel = function () {
        var txtGrupos = [
            $lserv.translate("Generales"),
            $lserv.translate("Operadores"),
            $lserv.translate("Pasarelas"),
            $lserv.translate("Radio HF"),
            $lserv.translate("Radio M+N"),
            $lserv.translate("Equipos Externos"),
            $lserv.translate("Todos")
        ];
        return ctrl.ls.LogFilter().tpMat < 7 ? txtGrupos[ctrl.ls.LogFilter().tpMat] : sprintf($lserv.translate("Error %1$d"), ctrl.ls.LogFilter().tpMat);
    };

    /** Generacion de Informe PDF de Historicos */
    ctrl.toPdf = function () {
        var now = new Date();
        var lpag = ctrl.portrait == true ? 47 : 30;
        var lsep = 5;
        var doc = new jsPDF(ctrl.portrait == true ? 'portrait' : 'landscape');
        var nPages = Math.ceil(ctrl.lhis.length / lpag);
        doc.setFont("verdana");

        for (pg = 0; pg < nPages; pg++) {
            /** La Cabecera ... */
            doc.addImage(imgData, 'JPEG', 10, 10, 30, 15);
            doc.setFontSize(14);
            doc.setTextColor(0x4A, 0x77, 0x29);
            doc.text(45, 22, $lserv.translate('HCT_MSG_02')/*"ULISES-5000 I. INFORME DE HISTORICOS"*/);

            doc.setFontSize(10);
            doc.setTextColor(0, 0, 0);
            doc.text(20, 32, $lserv.translate('HCT_MSG_03')/*"Fecha"*/);
            doc.text(75, 32, $lserv.translate('HCT_MSG_04')/*"Incidencia"*/);
            doc.line(20, 34, ctrl.portrait == true ? 190 : 290, 34);

            /** Filtro Cabecera */
            doc.setFontSize(8);
            doc.setTextColor(0, 0, 0);
            doc.text(160, 20, $lserv.translate("Desde:")); doc.text(210, 20, $lserv.translate("Hasta:"));
            doc.text(160, 24, $lserv.translate("Grupo:")); doc.text(210, 24, $lserv.translate("Elemento:"));
            doc.text(160, 28, $lserv.translate("Incidencias:"));
            doc.text(160, 32, $lserv.translate("Contiene:"));

            /** Filtro Datos */
            doc.setFontSize(8);
            doc.setTextColor(0, 0, 192);
            doc.text(180, 20, ctrl.ls.LogFilter().dtDesde.toLocaleString()); doc.text(230, 20, ctrl.ls.LogFilter().dtHasta.toLocaleString());
            doc.text(180, 24, ctrl.textoGrpSel()); doc.text(230, 24, ctrl.ls.LogFilter().Mat);
            doc.text(180, 28, ctrl.textoInciSel());
            doc.text(180, 32, ctrl.textoTexto());

            /** El Cuerpo */
            var iInicial = pg * lpag;
            var iFinal = iInicial + lpag;
            iFinal = iFinal <= ctrl.lhis.length ? iFinal : ctrl.lhis.length;
            var cLine = 0;

            doc.setFontSize(9);
            for (iInci = iInicial; iInci < iFinal; iInci++ , cLine++) {
                var left = 20;
                var top = 40 + lsep * cLine;
                doc.setTextColor(0, 0, 0);
                doc.text(left, top, ctrl.lhis[iInci].date);         // La Fecha

                doc.setTextColor(0x4A, 0x77, 0x29);
                doc.text(left + 32, top, ctrl.lhis[iInci].idhw);      // El Item

                doc.setTextColor(0x4A, 0x77, 0x29);
                doc.text(left + 55, top, ctrl.lhis[iInci].desc);      // La Incidencias
            }

            /** El Pie ... */
            var topPie = ctrl.portrait == true ? 290 : 200;				// 290 portrait;
            doc.setTextColor(0, 0, 0);      // Negro
            doc.text(10, topPie, now.toLocaleString());
            doc.text(80, topPie, $lserv.translate('HCT_MSG_05')/*'DF-Nucleo. 2016. All rights reserved. '*/);
            doc.text(190, topPie, $lserv.translate('HCT_MSG_06')/*'Pag '*/ + (pg + 1));

            /** Pagina Siguiente */
            if (pg < (nPages - 1))
                doc.addPage();
        }

        // Salvar el Documento...
        doc.save('uv5ki-man-hist-' + now.toLocaleString() + '.pdf');
    };

    /** Salva el Informe de Historicos a EXCEL (csv) */
    ctrl.toExcel = function () {
    /** Filtro Aplicado */
        var strData = "Applied Filter\n";
        strData += ("From;" + ctrl.ls.LogFilter().dtDesde.toLocaleString() + ";To;" + ctrl.ls.LogFilter().dtHasta.toLocaleString() + "\n");
        strData += ("Group;" + ctrl.textoGrpSel() + ";Item;" + ctrl.ls.LogFilter().Mat + "\n");
        strData += ("Codes;" + ctrl.textoInciSel() + "\n");
        strData += ("Content;" + ctrl.textoTexto() + "\n");
        strData += "\n";
    /** Tabla */
        strData += "Date;Item;Description;Ack\n";
        ctrl.lhis.forEach(function (inci, index) {
            strData += (inci.date + ";" + inci.idhw + ";" + inci.desc + ";" + inci.acknw + "-" + inci.user + "\n");
        });

        var now = new Date();
        var myLink = document.createElement('a');
        myLink.download = 'uv5ki-man-hist-' + now.toLocaleString() + '.csv';
        myLink.href = "data:application/csv," + escape(strData);
        myLink.click();
    };

    /** Borrado del Historico */
    ctrl.clearhis = function () {
        // TODO. Leer la Fecha / Hora introducida.
        ctrl.lhis = [];
    };

    ctrl.filmat = function (id) {
        ctrl.ls.LogFilter().Mat = id;
    };

    /** */
    // OLD
    ctrl.lest = [];     // Listado Estadistico.

    ctrl.est = {
        lest: [],
        fil: {
            dtDesde: moment().startOf('year').millisecond(0).toDate(),
            dtHasta: moment().endOf('day').millisecond(0).toDate(),
            tpMat: -1,
            Mat: ""
        },
        mat: []
    };

    function StatsFilterNormalize(filter) {
        var now = moment().add(1, 'days').toDate();

        filter.desde = moment(filter.desde).startOf('day').toDate();
        filter.hasta = moment(filter.hasta).endOf('day').toDate();
        if (filter.desde > now ||
            filter.hasta > now) {
            alertify.alert($lserv.translate('Error en la Seleccion'), $lserv.translate('La fecha inicial o la fecha final está en el futuro!'));
            return false;
        }
        else if (filter.desde > filter.hasta) {
            alertify.alert($lserv.translate('Error en la Seleccion'), $lserv.translate('La fecha inicial es mayor que la fecha Final!'));
            return false;
        }
        return true;
    }
    /** Obtiene el Historico del Servidor */
    ctrl.getEstadistica = function () {
        /** Preparamos el filtro para Estadistica */
        var todos = $lserv.translate('HCT_MSG_00');
        var todas = $lserv.translate('HCT_MSG_01');
        var elemento = ctrl.ls.StsFilter().Mat == todos || ctrl.ls.StsFilter().Mat == todas ? undefined : ctrl.ls.StsFilter().Mat;
        var filtro = {
            desde: ctrl.ls.StsFilter().dtDesde,
            hasta: ctrl.ls.StsFilter().dtHasta,
            tipo: (ctrl.ls.StsFilter().tpMat).toString(),
            elementos: elemento == undefined ? [] : [elemento]
        };
        console.log(filtro);
        /** Normalizar el filtro. */
        ctrl.clearest();
        if (StatsFilterNormalize(filtro) == true) {
            $serv.db_esta_get(filtro).then(function (response) {
                //ctrl.lhis = response.data.lista;
                console.log(response.data);
                prepare_lest(response.data);
                alertify.success($lserv.translate("Operacion realizada..."));
            }, function (response) {
                console.log(response);
                alertify.success($lserv.translate("Error en la peticion"));
            });
        }
    };
    /** */
    ctrl.OnEstTpMatChange = function () {
        fest_prepare(true);
    };

    /** */
    ctrl.clearest = function () {
        ctrl.est.lest = [];
    };

    /** */
    ctrl.toPDFest = function () {
        var now = new Date();
        var lpag = 47;
        var lsep = 5;
        var doc = new jsPDF();
        doc.setFont("verdana");

        /** La Cabecera ... */
        doc.addImage(imgData, 'JPEG', 10, 10, 15, 15);
        doc.setFontSize(14);
        doc.setTextColor(0xC1, 0x02, 0x2C);
        doc.text(45, 15, $lserv.translate("ULISES-5000 I. INFORME ESTADISTICO"));

        /** El Filtro */
        doc.setFontSize(11);
        doc.text(30, 40, $lserv.translate("Periodo Considerado"));
        doc.text(30, 50, $lserv.translate("Elementos Considerados"));
        doc.setTextColor(0, 0, 0);      // Negro
        doc.text(30, 70, $lserv.translate("Valores Calculados"));
        doc.text(90, 40, sprintf($lserv.translate("Desde %1$s hasta %2$s"),
            ctrl.ls.StsFilter().dtDesde.toLocaleDateString(),
            ctrl.ls.StsFilter().dtHasta.toLocaleDateString()));
        doc.text(90, 50, sprintf("%1$s: %2$s",
            // (ctrl.ls.StsFilter().tpMat == "1" ? $lserv.translate("Operadores") : $lserv.translate("Pasarelas")),
            getTipoMat(ctrl.ls.StsFilter().tpMat),
            ctrl.ls.StsFilter().Mat));

        /** Los Calculos */
        doc.setFontSize(9);
        for (ival = 0; ival < ctrl.est.lest.length; ival++) {
            doc.setTextColor(0xC1, 0x02, 0x2C);
            doc.text(40, 80 + 10 * ival, ctrl.est.lest[ival].texto);
            doc.setTextColor(0, 0, 0);      // Negro
            doc.text(110, 80 + 10 * ival, ctrl.est.lest[ival].valor.toString());
        }

        /** El Pie ... */
        var topPie = 290;
        doc.setFontSize(9);
        doc.setTextColor(0, 0, 0);      // Negro
        doc.text(10, topPie, now.toLocaleString());
        doc.text(80, topPie, $lserv.translate('HCT_MSG_05')/*'DF-Nucleo. 2016. All rights reserved. '*/);
        doc.text(190, topPie, $lserv.translate('HCT_MSG_06')/*'Pag '*/ + (1));

        // Salvar el Documento...
        doc.save('uv5ki-man-est-' + now.toLocaleString() + '.pdf');
    };


    /** Funciones Internas.. */
    //** Prepara el los valroes para el filtro de historico */
    function fhis_prepare(reset) {

        if (reset === true) ctrl.ls.LogFilter().txt = "";

        if (ctrl.ls.LogFilter().tpMat == 0) {
            ctrl.mat = [];
            if (reset === true) ctrl.ls.LogFilter().Mat = "";
            if (reset === true) ctrl.ls.LogFilter().Inci = [];
            ctrl.inci = ctrl.linci.filter($lserv.inci_filter_genonly);
        }
        else if (ctrl.ls.LogFilter().tpMat == 1) {
            ctrl.mat = [{ id: $lserv.translate('HCT_MSG_00')/*"Todos"*/ }].concat(ctrl.pict);
            if (reset === true) ctrl.ls.LogFilter().Mat = $lserv.translate('HCT_MSG_00')/*"Todos"*/;
            if (reset === true) ctrl.ls.LogFilter().Inci = [];
            ctrl.inci = ctrl.linci.filter($lserv.inci_filter_pict);
        }
        else if (ctrl.ls.LogFilter().tpMat == 2) {
            ctrl.mat = [{ id: $lserv.translate('HCT_MSG_01')/*"Todas"*/ }].concat(ctrl.pasa);
            if (reset === true) ctrl.ls.LogFilter().Mat = $lserv.translate('HCT_MSG_01')/*"Todas"*/;
            if (reset === true) ctrl.ls.LogFilter().Inci = [];
            ctrl.inci = ctrl.linci.filter($lserv.inci_filter_pasa);
        }
        else if (ctrl.ls.LogFilter().tpMat == 3) {
            ctrl.mat = [];
            if (reset === true) ctrl.ls.LogFilter().Mat = "";
            if (reset === true) ctrl.ls.LogFilter().Inci = [];
            ctrl.inci = ctrl.linci.filter($lserv.inci_filter_radio_hf);
        }
        else if (ctrl.ls.LogFilter().tpMat == 4) {
            ctrl.mat = [{ id: $lserv.translate('HCT_MSG_00')/*"Todos"*/ }].concat(ctrl.mni);        // [];
            if (reset === true) ctrl.ls.LogFilter().Mat = $lserv.translate('HCT_MSG_00')/*"Todos"*/;                               // "";
            if (reset === true) ctrl.ls.LogFilter().Inci = [];
            ctrl.inci = ctrl.linci.filter($lserv.inci_filter_radio_mn);
        }
        else if (ctrl.ls.LogFilter().tpMat == 5) {
            ctrl.mat = [];
            if (reset === true) ctrl.ls.LogFilter().Mat = "";
            if (reset === true) ctrl.ls.LogFilter().Inci = [];
            ctrl.inci = ctrl.linci.filter($lserv.inci_filter_ext);
        }
        else if (ctrl.ls.LogFilter().tpMat == 6) {
            ctrl.mat = [];
            if (reset === true) ctrl.ls.LogFilter().Mat = "";
            if (reset === true) ctrl.ls.LogFilter().Inci = [];
            ctrl.inci = ctrl.linci.filter($lserv.inci_filter_all);
        }
        else {
            ctrl.mat = [];
            ctrl.ls.LogFilter().Mat = "";
            ctrl.ls.LogFilter().Inci = [];
        }
    }

    //** */
    function fest_prepare(reset) {
        var todos = $lserv.translate('HCT_MSG_00');
        var todas = $lserv.translate('HCT_MSG_01');
        switch (ctrl.ls.StsFilter().tpMat) {
            case "0":                     // Operadores.
                ctrl.est.mat = [todos].concat(getHardOfType(0));
                if (reset === true || ctrl.ls.StsFilter().Mat==="") ctrl.ls.StsFilter().Mat = todos;
                break;
            case "1":                     // Pasarelas
                ctrl.est.mat = [todas].concat(getHardOfType(1));
                if (reset === true || ctrl.ls.StsFilter().Mat === "") ctrl.ls.StsFilter().Mat = todas;
                break;
            case "2":                     // Radios IP
                ctrl.est.mat = [todos].concat(getHardOfType(2));
                if (reset === true || ctrl.ls.StsFilter().Mat === "") ctrl.ls.StsFilter().Mat = todos;
                break;
            case "3":                     // Telefonos IP
                ctrl.est.mat = [todos].concat(getHardOfType(3));
                if (reset === true || ctrl.ls.StsFilter().Mat === "") ctrl.ls.StsFilter().Mat = todos;
                break;
            case "4":                     // Grabadores.
                ctrl.est.mat = [todos].concat(getHardOfType(5));
                if (reset === true || ctrl.ls.StsFilter().Mat === "") ctrl.ls.StsFilter().Mat = todos;
                break;
        }
    }

    //** */
    function filtro_reset(pag) {
        if (pag == 0) {
            ctrl.fil = {
                dtDesde: moment().startOf('day').millisecond(0).toDate(),
                dtHasta: moment().endOf('day').millisecond(0).toDate(),
                tpMat: "0",
                Mat: "",
                txt: "",
                limit: default_logs_limit,
                Inci: []
            };

            ctrl.ls.LogFilter().dtDesde = moment().startOf('day').millisecond(0).toDate();
            ctrl.ls.LogFilter().dtHasta = moment().endOf('day').millisecond(0).toDate();
            ctrl.ls.LogFilter().tpMat = "0";
            ctrl.ls.LogFilter().Mat = "";
            ctrl.ls.LogFilter().txt = "";
            ctrl.ls.LogFilter().limit = default_logs_limit;
            ctrl.ls.LogFilter().Inci = [];

            //ctrl.fil.dtDesde.setMilliseconds(0);
            //ctrl.fil.dtHasta.setMilliseconds(0);
            ctrl.inci = ctrl.linci.filter($lserv.inci_filter_genonly);
        }
        else if (pag == 1) {
            ctrl.fil = {
                dtDesde: moment().startOf('year').millisecond(0).toDate(),
                dtHasta: moment().endOf('day').millisecond(0).toDate(),
                tpMat: "1",
                Mat: "",
                txt: "",
                limit: default_logs_limit,
                Inci: []
            };

            ctrl.mat = [{ id: $lserv.translate('HCT_MSG_00')/*"Todos"*/ }].concat(ctrl.pict);
            ctrl.ls.LogFilter().Mat = $lserv.translate('HCT_MSG_00')/*"Todos"*/;
            ctrl.ls.LogFilter().Inci = [];
            //ctrl.fil.dtDesde.setMilliseconds(0);
            //ctrl.fil.dtHasta.setMilliseconds(0);
        }
    }

    //** */
    function prepare_lest(data) {
        ctrl.est.lest = [
            {
                texto: $lserv.translate("Elementos Considerados"),
                valor: data.res.NumeroElementos
            },
            {
                texto: $lserv.translate("Total Horas Consideradas"),
                valor: data.res.HorasTotales
            },
            {
                texto: $lserv.translate("Total Horas en Operacion"),
                valor: data.res.HorasOperativas
            },
            {
                texto: $lserv.translate("Numero de Fallos"),
                valor: data.res.NumeroDeFallos
            },
            {
                texto: $lserv.translate("Numero de Activaciones"),
                valor: data.res.NumeroDeActivaciones
            },
            {
                texto: $lserv.translate("Fallos por Unidad Considerada"),
                valor: data.res.TasaFallosUnidades.toFixed(2)
            },
            {
                texto: $lserv.translate("Fallos por Año"),
                valor: data.res.TasaFallosAnno.toFixed(2)
            },
            {
                texto: $lserv.translate("MTBF (Horas)"),
                valor: data.res.MTBF.toFixed(2)
            },
            {
                texto: $lserv.translate("MUT (Horas)"),
                valor: data.res.MUT.toFixed(2)
            },
            {
                texto: $lserv.translate("Disponibilidad (%)"),
                valor: data.res.Disponibilidad.toFixed(2)
            }
        ];
    }

    /** */
    function linci_get() {
        $serv.db_inci_get().then(function (response) {
            console.log(response.data);
            ctrl.linci = response.data.lista;
            fhis_prepare(false);
            // ctrl.inci = ctrl.linci.filter($lserv.inci_filter_genonly);
        }
        , function (response) {
            console.log(response);
        });        
    }
    /** */
    function pict_get() {
        $serv.db_ope_get().then(function (response) {
            console.log(response.data);
            ctrl.pict = response.data.lista;
        },
            function (response) {
            console.log(response);
        });        
    }
    /** */
    function pasa_get() {
        $serv.db_gw_get().then(function (response) {
            console.log(response.data);
            ctrl.pasa = response.data.lista;
        }
        , function (response) {
            console.log(response);
        });        
    }
    /** */
    function mni_get() {
        $serv.db_mni_get().then(function (response) {
            console.log(response.data);
            ctrl.mni = response.data.lista;
        }
        , function (response) {
            console.log(response);
        });
    }

    /** */
    ctrl.allhard = [];
    function cwp_gws_ext_get() {
        $serv.allhard_get().then(function (response) {
            console.log(response.data);
            ctrl.allhard = response.data.items;
            fest_prepare(false);
        },
        function (response) {
        });
    }

    function getHardOfType(tipo) {
        var names = [];
        $.each(ctrl.allhard, function (index, val) {
            if (val.tipo == tipo)
                names.push(val.Id);
        });
        return names;
    }

    function getTipoMat(tipo) {
        var tipos = [$lserv.translate("Operadores"), $lserv.translate("Pasarelas"), $lserv.translate("Radios IP"),
            $lserv.translate("Telef Ip"), $lserv.translate("Grabadores")];
        return tipos[parseInt(tipo)];
    }

    //** */
    function getDateRanges() {
        var ranges = {};
        ranges[$lserv.translate("Ultima Hora")] = [moment().subtract(1, "hours"), moment().endOf('day')];
        ranges[$lserv.translate("Hoy")] = [moment().startOf('day'), moment().endOf('day')];
        ranges[$lserv.translate("Ultimos 7 Dias")] = [moment().subtract(7, "days"), moment().endOf('day')];
        ranges[$lserv.translate("Ultimos 30 Dias")] = [moment().subtract(30, "days"), moment().endOf('day')];
        return ranges;
    }
    //** */
    function getDaysOfWeek() {
        return [
            $lserv.translate("Do"),
            $lserv.translate("Lu"),
            $lserv.translate("Ma"),
            $lserv.translate("Mi"),
            $lserv.translate("Ju"),
            $lserv.translate("Vi"),
            $lserv.translate("Sa")
        ];
    }

    //** */
    function getMonthNames() {
        return [
            $lserv.translate("Enero"),
            $lserv.translate("Febrero"),
            $lserv.translate("Marzo"),
            $lserv.translate("Abril"),
            $lserv.translate("Mayo"),
            $lserv.translate("Junio"),
            $lserv.translate("Julio"),
            $lserv.translate("Agosto"),
            $lserv.translate("Septiembre"),
            $lserv.translate("Octubre"),
            $lserv.translate("Noviembre"),
            $lserv.translate("Diciembre")
        ];
    }

    /** */
    function dateRangeUpdate() {
        var drp = $('#daterange').data('daterangepicker');

        drp.ranges = getDateRanges();
        drp.startDate = moment(ctrl.ls.LogFilter().dtDesde);
        drp.endDate = moment(ctrl.ls.LogFilter().dtHasta);
    }

    /** */
    function dateRangeInit() {
        $('#daterange').daterangepicker({
            "timePicker": true,
            "timePicker24Hour": true,
            "ranges": getDateRanges(),
            "locale": {
                "format": $lserv.translate("DD/MM/YYYY HH:mm"),
                "separator": " - ",
                "applyLabel": $lserv.translate("Aplicar"),
                "cancelLabel": $lserv.translate("Cancelar"),
                "fromLabel": $lserv.translate("Desde: "),
                "toLabel": $lserv.translate("Hasta: "),
                "customRangeLabel": $lserv.translate("Otro"),
                "weekLabel": $lserv.translate("Sem"),
                "daysOfWeek": getDaysOfWeek(),
                "monthNames": getMonthNames(),
                "firstDay": 1
            },
            "startDate": moment(ctrl.ls.LogFilter().dtDesde),
            "endDate": moment(ctrl.ls.LogFilter().dtHasta)
        }, function (start, end, label) {
            ctrl.ls.LogFilter().dtDesde = start.startOf('minute').toDate();
            ctrl.ls.LogFilter().dtHasta = end.endOf('minute').toDate();
        });
    }

    /** Al cargar la Pagina */
    $scope.$on('$viewContentLoaded', function () {
        linci_get();
        pict_get();
        pasa_get();
        mni_get();
        cwp_gws_ext_get();
        /** */
        dateRangeInit();
    });

    /** Funcion Periodica del controlador */
    var timer = $interval(function () {
        if (ctrl.linci.length == 0)
            linci_get();
        if (ctrl.pict.length == 0)
            pict_get();
        if (ctrl.pasa.length == 0)
            pasa_get();
        if (ctrl.mni.length == 0)
            mni_get();
        if (ctrl.allhard.length == 0)
            cwp_gws_ext_get();
    }, pollingTime);

    /** Salida del Controlador. Borrado de Variables */
    $scope.$on("$destroy", function () {
        $interval.cancel(timer);
    });

});
