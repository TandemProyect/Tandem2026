/**
 * Lista de muros del diseño: medidas de solo lectura y los mismos checks
 * que «Modificar muro», en las cuatro pestañas. Aceptar escribe las variables.
 */
const TAPE_ICON = '/Content/DesignTools/MenuIcon/';

function tapeOpt(id, file) {
    return { id: id, image: TAPE_ICON + file };
}

function tapeBlocks() {
    return {
        end01: { defaultId: 'TapeS1', options: [tapeOpt('TapeS1', 'TapeS1.png'), tapeOpt('TapeS2', 'TapeS2.png')] },
        end02: { defaultId: 'TapeS3', options: [tapeOpt('TapeS3', 'TapeS3.png')] },
        end03: { defaultId: 'TapeS4', options: [tapeOpt('TapeS4', 'TapeS4.png'), tapeOpt('TapeS5', 'TapeS5.png')] },
        end04: { defaultId: 'TapeS6', options: [tapeOpt('TapeS6', 'TapeS6.png'), tapeOpt('TapeS7', 'TapeS7.png')] },
        end05: { defaultId: 'TapeS8', options: [tapeOpt('TapeS8', 'TapeS8.png')] },
        end06: { defaultId: 'TapeS9', options: [tapeOpt('TapeS9', 'TapeS9.png'), tapeOpt('TapeS10', 'TapeS10.png'), tapeOpt('TapeS11', 'TapeS11.png')] },
        end07: { defaultId: 'TapeS12', options: [tapeOpt('TapeS13', 'TapeS12.png'), tapeOpt('TapeS12', 'TapeS13.png')] },
    };
}

function tapeBlockKey(widthM, solape) {
    if (solape) return 'end01';
    const mm = Math.round((Number(widthM) || 0) * 1000);
    if (mm < 300) return 'end02';
    if (mm === 300 || mm === 450 || mm === 600 || mm === 900) return 'end03';
    const multiple = mm > 0 && mm % 50 === 0;
    if (mm < 600) return multiple ? 'end04' : 'end05';
    return multiple ? 'end06' : 'end07';
}

function tapeBlockFor(widthM, solape) {
    const all = tapeBlocks();
    return all[tapeBlockKey(widthM, solape)] || all.end03;
}

function meters(value) {
    const n = Number(value);
    if (!Number.isFinite(n)) return '';
    return n.toLocaleString('es-ES', { minimumFractionDigits: 2, maximumFractionDigits: 2 });
}

function loadScript(src) {
    return new Promise(function (resolve, reject) {
        const el = document.createElement('script');
        el.src = src;
        el.onload = function () { resolve(); };
        el.onerror = reject;
        document.body.appendChild(el);
    });
}

function ensureDataTables() {
    if (window.jQuery && window.jQuery.fn && window.jQuery.fn.DataTable) return Promise.resolve();
    if (!window.jQuery) return Promise.reject(new Error('jquery'));
    if (!document.getElementById('desing2-wall-list-dt-css')) {
        const css = document.createElement('link');
        css.id = 'desing2-wall-list-dt-css';
        css.rel = 'stylesheet';
        css.href = 'https://cdn.datatables.net/1.13.8/css/dataTables.bootstrap5.min.css';
        document.head.appendChild(css);
    }
    return loadScript('https://cdn.datatables.net/1.13.8/js/jquery.dataTables.min.js')
        .then(function () { return loadScript('https://cdn.datatables.net/1.13.8/js/dataTables.bootstrap5.min.js'); });
}

function checkTitle(label, flag) {
    return label + '<br><input type="checkbox" data-wall-flag-all="' + flag + '" title="' + label + '" />';
}

function checkCol(flag) {
    return {
        data: flag,
        orderable: false,
        className: 'text-center desing2-wall-list-check',
        render: function (value, type, row) {
            if (type !== 'display') return value ? 1 : 0;
            return '<input type="checkbox" data-wall-id="' + row.id + '" data-wall-flag="' + flag + '"' + (value ? ' checked' : '') + ' />';
        },
    };
}

function escHtml(value) {
    return String(value == null ? '' : value)
        .replace(/&/g, '&amp;')
        .replace(/</g, '&lt;')
        .replace(/"/g, '&quot;');
}

function groupOptionsMarkup(row, groups, placeholder) {
    const current = row.group != null && Number.isFinite(Number(row.group)) && Number(row.group) >= 0
        ? String(Math.round(Number(row.group)))
        : '';
    let html = '<option value="">' + escHtml(placeholder) + '</option>';
    const seen = {};
    const list = groups || [];
    for (let i = 0; i < list.length; i++) {
        const item = list[i];
        if (!item || !Number.isFinite(Number(item.number))) continue;
        const n = String(Math.round(Number(item.number)));
        if (seen[n]) continue;
        seen[n] = true;
        const name = item.name ? String(item.name) : '';
        const label = name ? (n + ' — ' + name) : n;
        html += '<option value="' + escHtml(n) + '"' + (n === current ? ' selected' : '') + '>' + escHtml(label) + '</option>';
    }
    if (current && !seen[current]) {
        html += '<option value="' + escHtml(current) + '" selected>' + escHtml(current) + '</option>';
    }
    return '<select class="desing2-wall-list-group" data-wall-id="' + row.id + '">' + html + '</select>';
}

function infoCols(getGroups, placeholder) {
    return [
        { data: 'id', title: 'Id', className: 'text-end' },
        {
            data: 'group',
            title: 'Grupo',
            orderable: false,
            render: function (value, type, row) {
                if (type !== 'display') return value == null ? '' : value;
                const groups = getGroups ? getGroups() : [];
                return groupOptionsMarkup(row, groups, placeholder);
            },
        },
        { data: 'lengthM', title: 'Largo', className: 'text-end', render: function (v, type) { return type === 'display' || type === 'filter' ? meters(v) : v; } },
        { data: 'widthM', title: 'Ancho', className: 'text-end', render: function (v, type) { return type === 'display' || type === 'filter' ? meters(v) : v; } },
        { data: 'heightM', title: 'Alto', className: 'text-end', render: function (v, type) { return type === 'display' || type === 'filter' ? meters(v) : v; } },
    ];
}

function tapeCol(flag, solapeFlag, mirror) {
    return {
        data: flag,
        orderable: false,
        className: 'desing2-wall-list-tape',
        render: function (value, type, row) {
            if (type !== 'display') return value || '';
            const block = tapeBlockFor(row.widthM, !!row[solapeFlag]);
            let html = '';
            for (let i = 0; i < block.options.length; i++) {
                const opt = block.options[i];
                const on = opt.id === value;
                html += '<label><input type="checkbox" data-wall-id="' + row.id + '" data-wall-flag="' + flag + '" value="' + opt.id + '"' + (on ? ' checked' : '') + ' />'
                    + '<img src="' + opt.image + '" alt=""' + (mirror ? ' class="is-mirror"' : '') + ' /></label>';
            }
            return html;
        },
    };
}

export function mountWallBulkList(deps) {
    const btn = document.getElementById('ma-stl-wall-list');
    const modalEl = document.getElementById('desing2WallListModal');
    if (!btn || !modalEl || !window.bootstrap || !deps) return;

    const modal = window.bootstrap.Modal.getOrCreateInstance(modalEl);
    const emptyText = modalEl.getAttribute('data-empty') || 'No hay muros';
    const searchText = modalEl.getAttribute('data-search') || 'Buscar:';
    let rows = [];
    let byId = {};
    let tables = null;
    let opening = false;

    function syncHeaderChecks(table) {
        const root = table.table().container();
        const boxes = root.querySelectorAll('input[data-wall-flag-all]');
        for (let i = 0; i < boxes.length; i++) {
            const flag = boxes[i].getAttribute('data-wall-flag-all');
            let on = rows.length > 0;
            for (let r = 0; r < rows.length; r++) {
                if (!rows[r][flag]) { on = false; break; }
            }
            boxes[i].checked = on;
        }
    }

    function drawAll() {
        if (!tables) return;
        tables.muro.clear().rows.add(rows).draw(false);
        tables.tapes.clear().rows.add(rows).draw(false);
        tables.props.clear().rows.add(rows).draw(false);
        tables.dim.clear().rows.add(rows).draw(false);
        syncHeaderChecks(tables.muro);
        syncHeaderChecks(tables.tapes);
        syncHeaderChecks(tables.props);
        syncHeaderChecks(tables.dim);
    }

    function makeTable(id, columns) {
        return window.jQuery('#' + id).DataTable({
            data: [],
            paging: false,
            searching: true,
            info: false,
            ordering: true,
            order: [[0, 'asc']],
            scrollX: true,
            autoWidth: false,
            dom: 'frt',
            language: {
                search: searchText,
                emptyTable: emptyText,
                zeroRecords: emptyText,
            },
            columns: columns,
        });
    }

    function ensureTables() {
        if (tables) return;
        const placeholder = modalEl.getAttribute('data-group-select') || 'Seleccionar grupo';
        const groupCols = function () { return infoCols(deps.groups, placeholder); };
        tables = {
            muro: makeTable('desing2WallListMuro', groupCols().concat([
                Object.assign({ title: checkTitle('Ménsulas en cara interior', 'bracketIn') }, checkCol('bracketIn')),
                Object.assign({ title: checkTitle('Ménsulas en cara exterior', 'bracketOut') }, checkCol('bracketOut')),
                Object.assign({ title: checkTitle('Rigidizadores en cara interior', 'stiffIn') }, checkCol('stiffIn')),
                Object.assign({ title: checkTitle('Rigidizadores en cara exterior', 'stiffOut') }, checkCol('stiffOut')),
                Object.assign({ title: checkTitle('Utilizar paneles 0,75 regulables', 'panel750') }, checkCol('panel750')),
                Object.assign({ title: checkTitle('Paneles verticales', 'verticalPanels') }, checkCol('verticalPanels')),
            ])),
            tapes: makeTable('desing2WallListTapes', groupCols().concat([
                Object.assign({ title: checkTitle('Solape inicial', 'solapeIni') }, checkCol('solapeIni')),
                Object.assign({ title: 'Solape inicial' }, tapeCol('tapeIni', 'solapeIni', false)),
                Object.assign({ title: checkTitle('Solape final', 'solapeEnd') }, checkCol('solapeEnd')),
                Object.assign({ title: 'Solape final' }, tapeCol('tapeEnd', 'solapeEnd', true)),
            ])),
            props: makeTable('desing2WallListProps', groupCols().concat([
                Object.assign({ title: checkTitle('Puntales superiores en cara interior', 'propIn') }, checkCol('propIn')),
                Object.assign({ title: checkTitle('Puntales superiores en cara exterior', 'propOut') }, checkCol('propOut')),
                Object.assign({ title: checkTitle('Puntales inferiores en cara interior', 'propInInf') }, checkCol('propInInf')),
                Object.assign({ title: checkTitle('Puntales inferiores en cara exterior', 'propOutInf') }, checkCol('propOutInf')),
            ])),
            dim: makeTable('desing2WallListDim', groupCols().concat([
                Object.assign({ title: checkTitle(modalEl.getAttribute('data-dim-label') || 'Acotar este muro', 'dimWall') }, checkCol('dimWall')),
            ])),
        };
    }

    function normalizeTape(row) {
        const ini = tapeBlockFor(row.widthM, row.solapeIni);
        const end = tapeBlockFor(row.widthM, row.solapeEnd);
        const iniIds = {};
        const endIds = {};
        for (let i = 0; i < ini.options.length; i++) iniIds[ini.options[i].id] = true;
        for (let e = 0; e < end.options.length; e++) endIds[end.options[e].id] = true;
        if (!iniIds[row.tapeIni]) row.tapeIni = ini.defaultId;
        if (!endIds[row.tapeEnd]) row.tapeEnd = end.defaultId;
    }

    function openList() {
        if (opening) return;
        opening = true;
        const host = document.fullscreenElement || document.body;
        if (modalEl.parentNode !== host) host.appendChild(modalEl);
        ensureDataTables().then(function () {
            ensureTables();
            rows = deps.collect() || [];
            byId = {};
            for (let i = 0; i < rows.length; i++) {
                normalizeTape(rows[i]);
                byId[String(rows[i].id)] = rows[i];
            }
            drawAll();
            modal.show();
            window.setTimeout(function () {
                if (!tables) return;
                tables.muro.columns.adjust();
                tables.tapes.columns.adjust();
                tables.props.columns.adjust();
                tables.dim.columns.adjust();
            }, 50);
        }).catch(function () {
            if (deps.fail) deps.fail('No se ha podido abrir la lista de muros.');
        }).then(function () { opening = false; });
    }

    modalEl.addEventListener('mousedown', function (ev) {
        const t = ev.target;
        if (t && t.getAttribute && (t.getAttribute('data-wall-flag-all') || (t.classList && t.classList.contains('desing2-wall-list-group')))) ev.stopPropagation();
    }, true);
    modalEl.addEventListener('click', function (ev) {
        const t = ev.target;
        if (t && t.getAttribute && (t.getAttribute('data-wall-flag-all') || (t.classList && t.classList.contains('desing2-wall-list-group')))) ev.stopPropagation();
    }, true);

    modalEl.addEventListener('change', function (ev) {
        const input = ev.target;
        if (!input || !input.getAttribute) return;
        const allFlag = input.getAttribute('data-wall-flag-all');
        if (allFlag) {
            for (let i = 0; i < rows.length; i++) {
                rows[i][allFlag] = input.checked;
                if (allFlag === 'solapeIni' || allFlag === 'solapeEnd') normalizeTape(rows[i]);
            }
            drawAll();
            return;
        }
        const id = input.getAttribute('data-wall-id');
        const flag = input.getAttribute('data-wall-flag');
        const row = id != null ? byId[String(id)] : null;
        if (!row) return;
        if (input.classList && input.classList.contains('desing2-wall-list-group')) {
            const n = Number(String(input.value || '').trim().replace(',', '.'));
            if (Number.isFinite(n) && n >= 0) row.group = Math.round(n);
            return;
        }
        if (!flag) return;
        if (flag === 'tapeIni' || flag === 'tapeEnd') {
            row[flag] = input.value;
            const cell = input.closest('td');
            if (cell) {
                const boxes = cell.querySelectorAll('input[type="checkbox"]');
                for (let b = 0; b < boxes.length; b++) boxes[b].checked = boxes[b] === input;
            }
            input.checked = true;
            return;
        }
        row[flag] = !!input.checked;
        if (flag === 'solapeIni' || flag === 'solapeEnd') {
            normalizeTape(row);
            if (tables) tables.tapes.rows().invalidate().draw(false);
        }
        if (tables) {
            syncHeaderChecks(tables.muro);
            syncHeaderChecks(tables.tapes);
            syncHeaderChecks(tables.props);
            syncHeaderChecks(tables.dim);
        }
    });

    function adjustTables() {
        if (!tables) return;
        tables.muro.columns.adjust();
        tables.tapes.columns.adjust();
        tables.props.columns.adjust();
        tables.dim.columns.adjust();
    }

    modalEl.addEventListener('shown.bs.tab', adjustTables);
    modalEl.addEventListener('shown.bs.modal', adjustTables);

    const accept = document.getElementById('desing2WallListAccept');
    const cancel = document.getElementById('desing2WallListCancel');
    if (accept) {
        accept.addEventListener('click', function () {
            if (deps.apply) deps.apply(rows);
            modal.hide();
        });
    }
    if (cancel) cancel.addEventListener('click', function () { modal.hide(); });
    btn.addEventListener('click', function (ev) {
        ev.preventDefault();
        openList();
    });
}
