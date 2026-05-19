/**
 * DataTables list toolbar estándar (Empleados / Intranet).
 * Requiere jQuery DataTables + Buttons (layout Materio).
 *
 * i18n:
 *   - Lee window.tandemCommonDt (inyectado en _LayoutMaterio.cshtml desde el
 *     modulo Common) para textos del menu colectivo y del lengthMenu por
 *     defecto. Si no existe, cae a textos en español (compatibilidad).
 *   - Una vista que defina su propio objeto i18n a nivel de modulo (p. ej.
 *     companyDt) y lo pase a applyListDefaults sigue ganando: su language /
 *     lengthMenu / buttons sustituye al default global.
 */
(function (window, $) {
    'use strict';

    if (!$ || !$.fn || !$.fn.dataTable) {
        return;
    }

    /* Defaults i18n (window.tandemCommonDt -> _LayoutMaterio.cshtml). */
    var i18n = window.tandemCommonDt || {};
    function t(key, fallback) {
        var v = i18n[key];
        return (typeof v === 'string' && v.length) ? v : fallback;
    }

    var dom = '<"dataTables-length-position">Bfrt<"dataTables-length-position"i>p';

    var rowsLabel = t('rowsN', 'filas');
    var allLabel = t('rowsAll', 'Todas');
    var lengthMenu = [
        [10, 25, 50, -1],
        ['10 ' + rowsLabel, '25 ' + rowsLabel, '50 ' + rowsLabel, allLabel]
    ];

    /**
     * Texto plano para Excel/CSV: evita fallos de excelHtml5 con HTML complejo
     * (p. ej. img + onerror con JS en columnas tipo bandera / acciones).
     */
    function stripHtmlForExport(data /*, row, column, node */) {
        if (data == null || data === '') {
            return '';
        }
        var s = typeof data === 'string' ? data : String(data);
        var tmp = document.createElement('div');
        tmp.innerHTML = s;
        var text = tmp.textContent || tmp.innerText || '';
        return text.replace(/\s+/g, ' ').trim();
    }

    var exportOptsPlainVisible = {
        /* Excluye columna de acciones (HTML/botones) y celdas ocultas. */
        columns: ':visible:not(.tandem-col-actions)',
        orthogonal: 'export',
        format: {
            body: stripHtmlForExport
        }
    };

    function buildCollectionButtons(options) {
        options = options || {};
        /* Un solo indicador de desplegable: Bootstrap añade caret vía .dropdown-toggle (::after). */
        var icon = options.icon === 'materio'
            ? '<i class="icon-base ri ri-menu-line" aria-hidden="true"></i>'
            : "<i class='fas fa-bars'></i>";

        var sectionRecords = t('sectionRecords', 'Registros');
        var sectionExport = t('sectionExport', 'Exportar');
        var sectionColvis = t('sectionColumnsVisible', 'Columnas visibles');

        return [{
            extend: 'collection',
            text: icon,
            className: 'custom-html-collection',
            buttons: [
                '<h5>' + sectionRecords + '</h5>',
                'pageLength',
                '<h5>' + sectionExport + '</h5>',
                'print',
                'copy',
                'pdf',
                {
                    extend: 'csv',
                    exportOptions: exportOptsPlainVisible
                },
                {
                    extend: 'excel',
                    exportOptions: exportOptsPlainVisible
                },
                '<h5 class="not-top-heading">' + sectionColvis + '</h5>',
                'colvis'
            ]
        }];
    }

    /**
     * Fusiona opciones del módulo con dom, lengthMenu, buttons y stateSave por defecto.
     * Pasar buttons en options para sustituir el menú estándar.
     */
    function applyListDefaults(options) {
        options = options || {};
        var customButtons = options.buttons;
        var buttonsOpts = options.buttonsOptions;
        delete options.buttons;
        delete options.buttonsOptions;

        var defaults = {
            dom: dom,
            lengthMenu: lengthMenu,
            stateSave: true,
            buttons: buildCollectionButtons(buttonsOpts),
            /* Normaliza conteos si el JSON no trae camelCase o viene incompleto (evita NaN en info / paginación). */
            ajax: {
                dataFilter: function (data, type) {
                    if (typeof data !== 'string' || data === '') {
                        return data;
                    }
                    if (type && type !== 'json' && type !== 'application/json' && type !== 'text json') {
                        return data;
                    }
                    try {
                        var j = JSON.parse(data);
                        if (!j || typeof j !== 'object') {
                            return data;
                        }
                        var rt = parseInt(j.recordsTotal, 10);
                        if (isNaN(rt)) {
                            rt = parseInt(j.RecordsTotal, 10);
                        }
                        j.recordsTotal = isNaN(rt) ? 0 : rt;
                        var rf = parseInt(j.recordsFiltered, 10);
                        if (isNaN(rf)) {
                            rf = parseInt(j.RecordsFiltered, 10);
                        }
                        j.recordsFiltered = isNaN(rf) ? 0 : rf;
                        var dr = parseInt(j.draw, 10);
                        j.draw = isNaN(dr) ? 0 : dr;
                        return JSON.stringify(j);
                    } catch (e) {
                        return data;
                    }
                }
            }
        };

        if (customButtons) {
            defaults.buttons = customButtons;
        }

        return $.extend(true, {}, defaults, options);
    }

    window.TandemDataTablesList = {
        dom: dom,
        lengthMenu: lengthMenu,
        exportOptsPlainVisible: exportOptsPlainVisible,
        stripHtmlForExport: stripHtmlForExport,
        buildCollectionButtons: buildCollectionButtons,
        applyListDefaults: applyListDefaults
    };
}(window, window.jQuery));
