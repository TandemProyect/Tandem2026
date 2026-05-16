/**
 * DataTables list toolbar estándar (Empleados / Intranet).
 * Requiere jQuery DataTables + Buttons (layout Materio).
 */
(function (window, $) {
    'use strict';

    if (!$ || !$.fn || !$.fn.dataTable) {
        return;
    }

    var dom = '<"dataTables-length-position">Bfrt<"dataTables-length-position"i>p';

    var lengthMenu = [
        [10, 25, 50, -1],
        ['10 filas', '25 filas', '50 filas', 'Todas']
    ];

    function buildCollectionButtons(options) {
        options = options || {};
        /* Un solo indicador de desplegable: Bootstrap añade caret vía .dropdown-toggle (::after). */
        var icon = options.icon === 'materio'
            ? '<i class="icon-base ri ri-menu-line" aria-hidden="true"></i>'
            : "<i class='fas fa-bars'></i>";

        return [{
            extend: 'collection',
            text: icon,
            className: 'custom-html-collection',
            buttons: [
                '<h5>Registros</h5>',
                'pageLength',
                '<h5>Exportar</h5>',
                'print',
                'copy',
                'pdf',
                'csv',
                'excel',
                '<h5 class="not-top-heading">Columnas visibles</h5>',
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
        buildCollectionButtons: buildCollectionButtons,
        applyListDefaults: applyListDefaults
    };
}(window, window.jQuery));
