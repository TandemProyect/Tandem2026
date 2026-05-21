/*
 * DataTables state por usuario: localStorage + ColReorder + colResizable
 * -----------------------------------------------------------------
 *  - ColReorder activo por defecto; ultima columna fija con iFixedColumnsRight (ver defaults).
 *  - Estado (orden, visibilidad, pageLength, busqueda, anchos): **localStorage**
 *    claves tandem_dt_ls1_* / tandem_dtq_ls1_* — no usa cookies: evita HTTP 400 por cabecera Cookie demasiado grande.
 *  - Si la cabecera Cookie ya es enorme, el servidor puede responder 400 ANTES de enviar HTML:
 *    entonces no hay JS; hay que subir MaxFieldLength/http.sys o borrar cookies del sitio una vez.
 *  - slimStatePayload recorta JSON pesado (~3.8KB) como antes.
 *  - Usuario: meta dt-user del layout.
 */
(function ($) {
    if (!$ || !$.fn || (!$.fn.dataTable && !$.fn.DataTable)) { return; }
    var DT = $.fn.dataTable || $.fn.DataTable;

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
    /** Nombre legacy de cookie de busqueda (solo para borrar en dtResetState). */
    function searchCookieKey(tableId) {
        return 'dtq_' + getUser() + '_' + (tableId || 'unknown');
    }
    function deleteCookie(name) {
        document.cookie = name + '=;expires=Thu, 01 Jan 1970 00:00:01 GMT;path=/;SameSite=Lax';
    }

    /** Prefijos localStorage (v1); no usar cookies para estado de tablas. */
    var LS_PREFIX_STATE = 'tandem_dt_ls1_';
    var LS_PREFIX_SEARCH = 'tandem_dtq_ls1_';

    function lsStateKey(tableId) {
        return LS_PREFIX_STATE + getUser() + '_' + (tableId || 'unknown');
    }
    function lsSearchKey(tableId) {
        return LS_PREFIX_SEARCH + getUser() + '_' + (tableId || 'unknown');
    }
    function lsSet(key, value) {
        try {
            if (window.localStorage) {
                window.localStorage.setItem(key, value);
                return true;
            }
        } catch (e) { /* cuota / modo privado */ }
        try {
            if (window.sessionStorage) {
                window.sessionStorage.setItem(key, value);
                return true;
            }
        } catch (e2) { /* noop */ }
        return false;
    }
    function lsGet(key) {
        try {
            if (window.localStorage) {
                var loc = window.localStorage.getItem(key);
                if (loc !== null && loc !== undefined) {
                    return loc;
                }
            }
        } catch (e) { /* noop */ }
        try {
            if (window.sessionStorage) {
                return window.sessionStorage.getItem(key);
            }
        } catch (e2) { /* noop */ }
        return null;
    }
    function lsRemove(key) {
        try {
            if (window.localStorage) window.localStorage.removeItem(key);
        } catch (e) { /* noop */ }
        try {
            if (window.sessionStorage) window.sessionStorage.removeItem(key);
        } catch (e2) { /* noop */ }
    }

    /** Migra dt_* / dtq_* a storage y SIEMPRE borra la cookie (incluso valor vacío / corrupto). */
    function migrateLegacyDtCookiesToLocalStorage() {
        try {
            var parts = document.cookie.split(';');
            for (var i = 0; i < parts.length; i++) {
                var seg = parts[i];
                var eq = seg.indexOf('=');
                var name = (eq >= 0 ? seg.substring(0, eq) : seg).trim();
                var isDtq = name.indexOf('dtq_') === 0;
                var isDt = !isDtq && name.indexOf('dt_') === 0;
                if (!isDtq && !isDt) {
                    continue;
                }
                var rawVal = eq >= 0 ? seg.substring(eq + 1).trim() : '';
                var val = '';
                if (rawVal.length) {
                    try {
                        val = decodeURIComponent(rawVal.replace(/\+/g, ' '));
                    } catch (de) {
                        val = rawVal;
                    }
                }
                if (val.length) {
                    if (isDtq) {
                        var sfxQ = name.substring(4);
                        var kq = LS_PREFIX_SEARCH + sfxQ;
                        if (!lsGet(kq)) {
                            lsSet(kq, val);
                        }
                    } else {
                        var sfxState = name.substring(3);
                        var ks = LS_PREFIX_STATE + sfxState;
                        if (!lsGet(ks)) {
                            lsSet(ks, val);
                        }
                    }
                }
                deleteCookie(name);
            }
        } catch (e0) { /* noop */ }
    }
    migrateLegacyDtCookiesToLocalStorage();

    function persistSearchPersist(tableId, data) {
        var q = '';
        try {
            if (data && data.search && data.search.search !== undefined && data.search.search !== null) {
                q = String(data.search.search);
            }
        } catch (e0) {
            q = '';
        }
        try {
            lsSet(lsSearchKey(tableId), JSON.stringify({ q: q, t: data && data.time ? data.time : Date.now() }));
        } catch (e1) { /* noop */ }
    }

    function slimStatePayload(tableId, data) {
        /* Limite conservador: cabe en localStorage y evita cuota; antes era ~4KB por cookie. */
        var maxJson = 3800;
        function tryStringify(d) {
            try {
                var s = JSON.stringify(d);
                if (s.length <= maxJson) {
                    return s;
                }
            } catch (e) { /* noop */ }
            return null;
        }
        var d = $.extend(true, {}, data);
        var s = tryStringify(d);
        if (s) {
            return s;
        }
        delete d.colWidths;
        s = tryStringify(d);
        if (s) {
            return s;
        }
        if (d.columns && $.isArray(d.columns)) {
            d.columns = $.map(d.columns, function (c) {
                if (!c) {
                    return { visible: true };
                }
                var vis = c.visible;
                return { visible: vis === true || vis === false ? vis : true };
            });
        }
        s = tryStringify(d);
        if (s) {
            return s;
        }
        delete d.ColReorder;
        s = tryStringify(d);
        if (s) {
            return s;
        }
        try {
            return JSON.stringify(d);
        } catch (e2) {
            return null;
        }
    }

    var dtDefaultsMerged = {
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
                if (cached) {
                    data.colWidths = cached;
                }
                persistSearchPersist(id, data);
                var payload = slimStatePayload(id, data);
                if (payload) {
                    lsSet(lsStateKey(id), payload);
                }
            } catch (e) { /* noop */ }
        },

        stateLoadCallback: function (settings) {
            try {
                var id = settings.sTableId || (settings.nTable && settings.nTable.id) || 'unknown';
                var raw = lsGet(lsStateKey(id));
                var parsed = null;
                if (raw) {
                    try {
                        parsed = JSON.parse(raw);
                    } catch (pe) {
                        lsRemove(lsStateKey(id));
                        parsed = null;
                    }
                    if (!parsed || typeof parsed !== 'object') {
                        lsRemove(lsStateKey(id));
                        parsed = null;
                    }
                }
                var qTxt = '';
                var qRaw = lsGet(lsSearchKey(id));
                if (qRaw) {
                    try {
                        var qo = JSON.parse(qRaw);
                        if (qo && qo.q != null) {
                            qTxt = String(qo.q);
                        }
                    } catch (qe) {
                        lsRemove(lsSearchKey(id));
                    }
                }
                if (qTxt.length && parsed) {
                    parsed.search = parsed.search || {};
                    parsed.search.search = qTxt;
                    if (parsed.search.smart === undefined) {
                        parsed.search.smart = true;
                    }
                    if (parsed.search.regex === undefined) {
                        parsed.search.regex = false;
                    }
                    if (parsed.search.caseInsensitive === undefined) {
                        parsed.search.caseInsensitive = true;
                    }
                }
                if (!parsed && qTxt.length && settings.nTable) {
                    $.data(settings.nTable, 'dtDeferredSearch', qTxt);
                    return null;
                }
                return parsed;
            } catch (e) {
                return null;
            }
        }
    };
    $.extend(true, DT.defaults, dtDefaultsMerged);

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

    function isSingleScrollTable(settings) {
        if (!settings || !settings.nTable) { return false; }
        var $t = $(settings.nTable);
        if ($t.hasClass('dt-no-single-scroll') || $t.closest('.dt-no-single-scroll').length) {
            return false;
        }
        return true;
    }

    // Para listas tipo "pantalla completa": activar scroll interno de DataTables si la vista no lo define.
    $(document).off('preInit.dt.singleScroll');
    $(document).on('preInit.dt.singleScroll', function (e, settings) {
        if (e.namespace !== 'dt') { return; }
        if (!isSingleScrollTable(settings)) { return; }
        var init = settings.oInit || {};
        if (init.scrollY === undefined || init.scrollY === null || init.scrollY === '') {
            settings.oInit.scrollY = 1;
            settings.oInit.scrollCollapse = true;
        }
    });

    function applySingleScrollLayout(api) {
        var settings = api.settings()[0];
        if (!isSingleScrollTable(settings)) { return; }

        document.documentElement.classList.add('master-articles-one-scroll');
        document.body.classList.add('master-articles-one-scroll');

        var $host = $(settings.nTable).closest('.card-body');
        if (!$host.length) {
            $host = $(settings.nTable).closest('.slimScrollDiv');
        }
        if (!$host.length) {
            $host = $(settings.nTable).closest('.card');
        }
        if (!$host.length) {
            $host = $(settings.nTable).parent();
        }
        if (!$host.length) { return; }
        $host.addClass('master-articles-partial-scroll');

        var host = $host[0];
        var hostRect = host.getBoundingClientRect();
        var footer = document.querySelector('.content-footer.footer');
        var bottomLimit = footer ? footer.getBoundingClientRect().top : window.innerHeight;
        var hostHeight = Math.floor(bottomLimit - hostRect.top - 34);
        if (hostHeight < 260) { hostHeight = 260; }
        host.style.setProperty('--ma-partial-height', hostHeight + 'px');

        var body = $host.find('.dataTables_scrollBody')[0];
        if (!body) { return; }
        var bodyTop = body.getBoundingClientRect().top;
        var bodyHeight = Math.floor(bottomLimit - bodyTop - 12);
        if (bodyHeight < 140) { bodyHeight = 140; }
        body.style.height = bodyHeight + 'px';
        body.style.maxHeight = bodyHeight + 'px';
        body.style.overflowY = 'auto';
    }

    $(document).on('init.dt', function (e, settings) {
        if (e.namespace !== 'dt') { return; }
        var api = new DT.Api(settings);
        var id = settings.sTableId || (settings.nTable && settings.nTable.id) || 'unknown';
        var deferred = settings.nTable ? $.data(settings.nTable, 'dtDeferredSearch') : null;
        if (deferred) {
            $.removeData(settings.nTable, 'dtDeferredSearch');
            setTimeout(function () {
                try { api.search(deferred).draw(false); } catch (ex0) { /* noop */ }
            }, 0);
        }

        var $wrap = $(api.table().container());
        var $filt = $wrap.find('.dataTables_filter input');
        if ($filt.length) {
            var tmr;
            $filt.off('input.dtSearchPersist').on('input.dtSearchPersist', function () {
                clearTimeout(tmr);
                tmr = setTimeout(function () {
                    try {
                        persistSearchPersist(id, {
                            time: Date.now(),
                            search: { search: $filt.val(), smart: true, regex: false, caseInsensitive: true }
                        });
                        api.state.save();
                    } catch (ex1) { /* noop */ }
                }, 250);
            });
        }

        setTimeout(function () {
            restoreWidths(api);
            applyResizers(api);
            applySingleScrollLayout(api);
        }, 50);

        setTimeout(function () {
            try {
                var qRaw2 = lsGet(lsSearchKey(id));
                if (!qRaw2) { return; }
                var qo2 = JSON.parse(qRaw2);
                var qv = (qo2 && qo2.q != null) ? String(qo2.q) : '';
                if (!qv) { return; }
                var ps = api.settings()[0].oPreviousSearch || {};
                var cur = ps.sSearch != null ? String(ps.sSearch) : '';
                if (cur !== qv) { api.search(qv).draw(false); }
            } catch (ex2) { /* noop */ }
        }, 120);
    });

    /* En tablas serverSide DataTables redibuja la cabecera con cada draw:
       hay que volver a inyectar los handles. */
    $(document).on('draw.dt', function (e, settings) {
        if (e.namespace !== 'dt') { return; }
        var api = new DT.Api(settings);
        applyResizers(api);
        applySingleScrollLayout(api);
    });

    $(window).off('resize.dtSingleScroll').on('resize.dtSingleScroll', function () {
        try {
            /* DataTables 1.11: `tables({api:true})` no expone `.every()` en el Api raíz. */
            var nodes = DT.tables({ visible: true });
            if (!nodes || !nodes.length) {
                return;
            }
            for (var i = 0; i < nodes.length; i++) {
                try {
                    var $n = $(nodes[i]);
                    if (!$n.length || typeof $n.DataTable !== 'function') {
                        continue;
                    }
                    applySingleScrollLayout($n.DataTable());
                } catch (e) { /* noop */ }
            }
        } catch (ex) { /* noop */ }
    });

    /* Helper publico para que el usuario pueda resetear su tabla. */
    window.dtResetState = function (tableId) {
        lsRemove(lsStateKey(tableId));
        lsRemove(lsSearchKey(tableId));
        deleteCookie(cookieKey(tableId));
        deleteCookie(searchCookieKey(tableId));
    };
})(window.jQuery);
