/*
 * DataTables state per user + cookie + ColReorder + colResizable
 * -----------------------------------------------------------------
 *  - ColReorder activo por defecto; la ultima columna (p. ej. Acciones) queda fija
 *    con iFixedColumnsRight: 1. Si hay ID oculto despues de Acciones, en la vista
 *    pon colReorder: { iFixedColumnsRight: 2 }.
 *  - Parche preInit: el plugin oficial hace $.extend({}, init, defaults) y el
 *    default true machaca iFixedColumnsRight del init; aqui mergeamos bien.
 *  - Persiste estado (orden, visibilidad, longitud, busqueda, anchos)
 *    en una cookie por usuario y tabla:  dt_<usuario>_<idTabla>
 *  - El usuario se obtiene del <meta name="dt-user"> del layout.
 *  - Activa colResizable (si esta cargado) y guarda los anchos en el
 *    mismo objeto de estado.
 */
(function ($) {
    if (!$ || !$.fn || !$.fn.dataTable) { return; }
    var DT = $.fn.dataTable;

    /* ColReorder: merge correcto (init gana sobre defaults) y ultima columna fija por defecto */
    (function patchColReorderPreInit() {
        var CR = DT.ColReorder;
        if (!CR) { return; }
        $(document).off('preInit.dt.colReorder');
        $(document).on('preInit.dt.colReorder', function (e, settings) {
            if (e.namespace !== 'dt') { return; }
            var init = settings.oInit.colReorder;
            if (init === false) { return; }
            var def = DT.defaults.colReorder;
            if (def === false && (init === undefined || init === null)) { return; }
            if (!init && (def === false || def === null || def === undefined)) { return; }

            var base = { bEnable: true, iFixedColumnsRight: 1, iFixedColumnsLeft: 0 };
            if (def && typeof def === 'object') {
                $.extend(base, def);
            } else if (def === true) {
                /* ya esta base */
            } else if (def === false) {
                return;
            }

            var opts;
            if (init === true || init === undefined || init === null) {
                opts = $.extend({}, base);
            } else if (typeof init === 'object') {
                opts = $.extend({}, base, init);
            } else {
                opts = $.extend({}, base);
            }

            if (settings._colReorder) { return; }
            new CR(settings, opts);
        });
    })();

    function getUser() {
        var meta = $('meta[name="dt-user"]').attr('content');
        return (meta && meta.length) ? meta : 'anon';
    }
    function cookieKey(tableId) {
        return 'dt_' + getUser() + '_' + (tableId || 'unknown');
    }
    function setCookie(name, value) {
        var days = 365;
        var d = new Date();
        d.setTime(d.getTime() + days * 86400000);
        try {
            document.cookie = name + '=' + encodeURIComponent(value) +
                ';expires=' + d.toUTCString() + ';path=/;SameSite=Lax';
        } catch (e) { /* noop */ }
    }
    function getCookie(name) {
        var re = new RegExp('(?:^|; )' + name.replace(/([.$?*|{}()\[\]\\\/\+^])/g, '\\$1') + '=([^;]*)');
        var m = document.cookie.match(re);
        return m ? decodeURIComponent(m[1]) : null;
    }
    function deleteCookie(name) {
        document.cookie = name + '=;expires=Thu, 01 Jan 1970 00:00:01 GMT;path=/';
    }

    /* === Defaults globales para todas las tablas ========================= */
    $.extend(true, DT.defaults, {
        colReorder: { bEnable: true, iFixedColumnsRight: 1 },
        stateSave: true,
        stateDuration: 60 * 60 * 24 * 365, /* 1 ano (en segundos) */

        /* Textos en español: primera letra mayúscula, resto minúsculas (estilo oración) */
        language: {
            emptyTable: "No hay información",
            info: "Mostrando _START_ a _END_ de _TOTAL_ entradas",
            infoEmpty: "Mostrando 0 a 0 de 0 entradas",
            infoFiltered: "(filtrado de _MAX_ entradas totales)",
            lengthMenu: "Mostrar _MENU_ entradas",
            loadingRecords: "Cargando...",
            processing: "Procesando...",
            search: "Buscar:",
            zeroRecords: "No se encontraron resultados",
            thousands: ".",
            decimal: ",",
            paginate: {
                first: "Primero",
                last: "Último",
                next: "Siguiente",
                previous: "Anterior"
            }
        },

        stateSaveCallback: function (settings, data) {
            try {
                var id = settings.sTableId || (settings.nTable && settings.nTable.id) || 'unknown';
                var cached = $.data(settings.nTable, 'dtColWidths');
                if (cached) { data.colWidths = cached; }
                setCookie(cookieKey(id), JSON.stringify(data));
            } catch (e) { /* noop */ }
        },

        stateLoadCallback: function (settings) {
            try {
                var id = settings.sTableId || (settings.nTable && settings.nTable.id) || 'unknown';
                var raw = getCookie(cookieKey(id));
                if (!raw) { return null; }
                var parsed = JSON.parse(raw);
                /* Cookie demasiado antigua o corrupta: limpiar */
                if (!parsed || typeof parsed !== 'object') {
                    deleteCookie(cookieKey(id));
                    return null;
                }
                return parsed;
            } catch (e) {
                return null;
            }
        }
    });

    /* === Resize de columnas por arrastre (con o sin scrollX) ============ */
    /*
     * Para cada cabecera (`thead th`) inserta un manejador a la derecha.
     * Funciona tanto si la tabla esta dentro de un scroller (scrollX: true,
     * en cuyo caso hay tabla de cabecera + tabla de cuerpo) como si no.
     * Sincroniza el ancho entre la cabecera y el cuerpo via <colgroup><col>.
     */

    function ensureColgroup($table, colCount) {
        var $cg = $table.children('colgroup');
        if (!$cg.length) {
            $cg = $('<colgroup/>').prependTo($table);
        }
        var $cols = $cg.children('col');
        while ($cols.length < colCount) {
            $cg.append('<col/>');
            $cols = $cg.children('col');
        }
        return $cg.children('col');
    }

    function getRelatedTables(api) {
        var settings = api.settings()[0];
        var $body = $(api.table().node());
        var $head = $body;
        var $scrollWrap = $(settings.nScrollHead || null);
        if ($scrollWrap.length) {
            $head = $scrollWrap.find('table').first();
        }
        return { $head: $head, $body: $body };
    }

    function setColumnWidth(api, colIdx, width) {
        var t = getRelatedTables(api);
        var nCols = api.columns().count();
        if (t.$head.length) {
            var $colsH = ensureColgroup(t.$head, nCols);
            $($colsH[colIdx]).css('width', width + 'px');
        }
        if (t.$body[0] !== t.$head[0]) {
            var $colsB = ensureColgroup(t.$body, nCols);
            $($colsB[colIdx]).css('width', width + 'px');
        }
        /* Tambien sobre el th visible (sin scroller) para que cuente en layout. */
        var $ths = t.$head.find('thead tr').first().children('th');
        if ($ths[colIdx]) { $($ths[colIdx]).css({ width: width + 'px', minWidth: width + 'px' }); }
    }

    function captureWidths(api) {
        var widths = [];
        var t = getRelatedTables(api);
        t.$head.find('thead tr').first().children('th').each(function () {
            widths.push($(this).outerWidth());
        });
        return widths;
    }

    function applyResizers(api) {
        var t = getRelatedTables(api);
        var $ths = t.$head.find('thead tr').first().children('th');
        $ths.each(function (idx) {
            var $th = $(this);
            $th.find('.dt-col-resizer').remove();
            var $grip = $('<span class="dt-col-resizer" aria-hidden="true"></span>');
            $th.append($grip);

            $grip.on('mousedown.dtresize', function (ev) {
                ev.preventDefault();
                ev.stopPropagation();
                var startX = ev.pageX;
                var startW = $th.outerWidth();
                $grip.addClass('dt-col-resizing');
                $('body').addClass('dt-col-resizing');

                function onMove(e) {
                    var delta = e.pageX - startX;
                    var w = Math.max(30, startW + delta);
                    setColumnWidth(api, idx, w);
                }
                function onUp() {
                    $(document).off('mousemove.dtresize mouseup.dtresize');
                    $grip.removeClass('dt-col-resizing');
                    $('body').removeClass('dt-col-resizing');
                    /* Guardar estado completo (incluye colWidths via stateSaveCallback). */
                    $.data(api.table().node(), 'dtColWidths', captureWidths(api));
                    try { api.state.save(); } catch (e2) { /* noop */ }
                    try { api.columns.adjust(); } catch (e2) { /* noop */ }
                }
                $(document).on('mousemove.dtresize', onMove).on('mouseup.dtresize', onUp);
            });
        });
    }

    function restoreWidths(api) {
        var st = api.state.loaded();
        if (!st || !st.colWidths) { return; }
        st.colWidths.forEach(function (w, i) {
            if (w) { setColumnWidth(api, i, w); }
        });
        $.data(api.table().node(), 'dtColWidths', st.colWidths);
        try { api.columns.adjust(); } catch (e) { /* noop */ }
    }

    $(document).on('init.dt', function (e, settings) {
        if (e.namespace !== 'dt') { return; }
        var api = new DT.Api(settings);
        setTimeout(function () {
            restoreWidths(api);
            applyResizers(api);
        }, 50);
    });

    /* En tablas serverSide DataTables redibuja la cabecera con cada draw:
       hay que volver a inyectar los handles. */
    $(document).on('draw.dt', function (e, settings) {
        if (e.namespace !== 'dt') { return; }
        var api = new DT.Api(settings);
        applyResizers(api);
    });

    /* Helper publico para que el usuario pueda resetear su tabla. */
    window.dtResetState = function (tableId) {
        deleteCookie(cookieKey(tableId));
    };
})(window.jQuery);
