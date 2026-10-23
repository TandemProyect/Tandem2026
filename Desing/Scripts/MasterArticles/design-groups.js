/**
 * Alta de grupos del diseño: filas de los que ya existen y filas nuevas.
 * Aceptar guarda altas, cambios de nombre o número y borrados lógicos.
 */
export function mountDesignGroups(deps) {
    const btn = document.getElementById('ma-stl-design-groups');
    const modalEl = document.getElementById('desing2GroupModal');
    if (!btn || !modalEl || !window.bootstrap || !deps) return;

    const modal = window.bootstrap.Modal.getOrCreateInstance(modalEl);
    const existingBody = document.getElementById('desing2GroupExisting');
    const newBody = document.getElementById('desing2GroupNew');
    const messageEl = document.getElementById('desing2GroupMessage');
    const acceptBtn = document.getElementById('desing2GroupAccept');
    const cancelBtn = document.getElementById('desing2GroupCancel');
    const addBtn = document.getElementById('desing2GroupAddRow');
    let groups = [];
    let deleted = [];
    let nextFloor = 1;
    let saving = false;

    function text(name, fallback) {
        return modalEl.getAttribute(name) || fallback;
    }

    function notify(message) {
        if (deps.notify) deps.notify(message);
    }

    function showMessage(message) {
        if (!messageEl) return;
        messageEl.textContent = message || '';
        messageEl.hidden = !message;
    }

    function publish(list, nextNumber) {
        groups = list || [];
        const next = Number(nextNumber);
        if (Number.isFinite(next) && next >= 1) nextFloor = Math.round(next);
        if (deps.onGroups) deps.onGroups(groups);
    }

    function load() {
        const designId = deps.getDesignId ? deps.getDesignId() : 0;
        if (!designId) {
            publish([]);
            return Promise.resolve([]);
        }
        const url = text('data-list-url', '');
        if (!url) return Promise.resolve(groups);
        const join = url.indexOf('?') >= 0 ? '&' : '?';
        return fetch(url + join + 'designId=' + encodeURIComponent(String(designId)), { credentials: 'same-origin' })
            .then(function (response) { return response.json(); })
            .then(function (data) {
                if (!data || data.ok === false) throw new Error((data && data.message) || text('data-error', ''));
                publish(data.groups || [], data.nextNumber);
                return groups;
            });
    }

    function cellInput(kind, value, lockNumber) {
        const td = document.createElement('td');
        const input = document.createElement('input');
        input.type = kind === 'number' ? 'number' : 'text';
        input.className = kind === 'number' ? 'desing2-group-number' : 'desing2-group-name';
        if (kind === 'number') {
            input.min = '0';
            input.step = '1';
            input.value = value == null ? '' : String(value);
            if (lockNumber) input.readOnly = true;
        } else {
            input.maxLength = 500;
            input.value = value || '';
        }
        td.appendChild(input);
        return td;
    }

    function nextAutoNumber() {
        let max = nextFloor - 1;
        const inputs = modalEl.querySelectorAll('.desing2-group-number');
        for (let i = 0; i < inputs.length; i++) {
            const n = Number(String(inputs[i].value || '').trim().replace(',', '.'));
            if (Number.isFinite(n) && n >= 0 && n > max) max = Math.round(n);
        }
        return max + 1;
    }

    function assignNext(tr) {
        if (!tr) return null;
        const input = tr.querySelector('.desing2-group-number');
        if (!input || String(input.value || '').trim()) return input;
        input.value = String(nextAutoNumber());
        return input;
    }

    function firstEmptyNewRow() {
        const rows = newBody.querySelectorAll('tr');
        for (let i = 0; i < rows.length; i++) {
            const input = rows[i].querySelector('.desing2-group-number');
            if (input && !String(input.value || '').trim()) return rows[i];
        }
        return null;
    }

    function appendExisting(row) {
        const tr = document.createElement('tr');
        tr.setAttribute('data-id', String(row.id));
        tr.appendChild(cellInput('number', row.number));
        tr.appendChild(cellInput('name', row.name));
        const action = document.createElement('td');
        action.className = 'desing2-material-count__row-actions';
        const del = document.createElement('button');
        del.type = 'button';
        del.className = 'btn btn-sm btn-icon';
        del.title = text('data-delete', 'Borrar');
        del.setAttribute('aria-label', text('data-delete', 'Borrar'));
        del.innerHTML = '<i class="icon-base ri ri-delete-bin-line" aria-hidden="true"></i>';
        del.addEventListener('click', function () {
            const id = Number(tr.getAttribute('data-id'));
            if (Number.isFinite(id) && id > 0) deleted.push(id);
            tr.remove();
            if (!existingBody.children.length) showEmptyExisting();
        });
        action.appendChild(del);
        tr.appendChild(action);
        existingBody.appendChild(tr);
    }

    function showEmptyExisting() {
        const tr = document.createElement('tr');
        tr.className = 'desing2-group-sheet__empty';
        const td = document.createElement('td');
        td.colSpan = 3;
        td.textContent = text('data-empty', 'No hay grupos creados');
        tr.appendChild(td);
        existingBody.appendChild(tr);
    }

    function appendNew() {
        const tr = document.createElement('tr');
        tr.appendChild(cellInput('number', '', true));
        tr.appendChild(cellInput('name', ''));
        const action = document.createElement('td');
        action.className = 'desing2-material-count__row-actions';
        tr.appendChild(action);
        newBody.appendChild(tr);
    }

    function paint() {
        deleted = [];
        existingBody.innerHTML = '';
        newBody.innerHTML = '';
        if (!groups.length) showEmptyExisting();
        for (let i = 0; i < groups.length; i++) appendExisting(groups[i]);
        for (let n = 0; n < 6; n++) appendNew();
        showMessage('');
    }

    function readRow(tr) {
        const numberInput = tr.querySelector('.desing2-group-number');
        const nameInput = tr.querySelector('.desing2-group-name');
        if (!numberInput || !nameInput) return null;
        const numberText = String(numberInput.value || '').trim();
        const name = String(nameInput.value || '').trim();
        return { numberText: numberText, name: name, id: Number(tr.getAttribute('data-id')) || 0 };
    }

    function collect() {
        const rows = [];
        const seen = {};
        const bodies = [existingBody, newBody];
        for (let b = 0; b < bodies.length; b++) {
            const list = bodies[b].querySelectorAll('tr');
            for (let i = 0; i < list.length; i++) {
                const raw = readRow(list[i]);
                if (!raw) continue;
                if (!raw.name) {
                    if (raw.id > 0) return { error: text('data-incomplete', 'Cada grupo necesita número y nombre') };
                    continue;
                }
                if (!raw.numberText) {
                    return { error: text('data-incomplete', 'Cada grupo necesita número y nombre') };
                }
                const number = Number(raw.numberText.replace(',', '.'));
                if (!Number.isFinite(number) || number < 0) {
                    return { error: text('data-incomplete', 'Cada grupo necesita número y nombre') };
                }
                const rounded = Math.round(number);
                if (seen[rounded]) {
                    return { error: text('data-duplicate', 'El número de grupo está repetido') };
                }
                if (!numberStillAllowed(rounded, raw.id)) {
                    return { error: text('data-used', 'Ese número de grupo ya se ha usado') };
                }
                seen[rounded] = true;
                rows.push({ id: raw.id, number: rounded, name: raw.name });
            }
        }
        return { rows: rows, deleted: deleted.slice() };
    }

    function numberStillAllowed(number, rowId) {
        if (number >= nextFloor) return true;
        for (let i = 0; i < groups.length; i++) {
            if (Number(groups[i].number) === number && Number(groups[i].id) === rowId) return true;
        }
        return false;
    }

    function openSheet() {
        const designId = deps.getDesignId ? deps.getDesignId() : 0;
        if (!designId) {
            notify(text('data-need-design', 'Abre un diseño para crear grupos'));
            return;
        }
        const host = document.fullscreenElement || document.body;
        if (modalEl.parentNode !== host) host.appendChild(modalEl);
        showMessage('');
        load().then(function () {
            const search = document.getElementById('desing2GroupSearch');
            if (search) search.value = '';
            const tab = document.getElementById('desing2GroupTabExisting');
            if (tab && window.bootstrap.Tab) window.bootstrap.Tab.getOrCreateInstance(tab).show();
            paint();
            modal.show();
        }).catch(function (err) {
            notify((err && err.message) || text('data-error', 'No se han podido guardar los grupos'));
        });
    }

    function groupOutcome(before, packed) {
        const byId = {};
        const previous = before || [];
        for (let i = 0; i < previous.length; i++) byId[String(previous[i].id)] = previous[i];
        let created = 0;
        let modified = (packed.deleted && packed.deleted.length) || 0;
        const rows = packed.rows || [];
        for (let i = 0; i < rows.length; i++) {
            const row = rows[i];
            const prev = row.id ? byId[String(row.id)] : null;
            if (!prev) {
                created++;
                continue;
            }
            const sameNumber = Number(prev.number) === Number(row.number);
            const sameName = String(prev.name || '') === String(row.name || '');
            if (!sameNumber || !sameName) modified++;
        }
        return { created: created, modified: modified };
    }

    function save() {
        if (saving) return;
        const designId = deps.getDesignId ? deps.getDesignId() : 0;
        if (!designId) {
            notify(text('data-need-design', 'Abre un diseño para crear grupos'));
            return;
        }
        const packed = collect();
        if (packed.error) {
            showMessage(packed.error);
            return;
        }
        const url = text('data-save-url', '');
        if (!url) return;
        saving = true;
        if (acceptBtn) acceptBtn.disabled = true;
        showMessage('');
        fetch(url, {
            method: 'POST',
            credentials: 'same-origin',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify({ designId: designId, rows: packed.rows, deleted: packed.deleted }),
        }).then(function (response) { return response.json(); }).then(function (data) {
            if (!data || data.ok === false) throw new Error((data && data.message) || text('data-error', ''));
            const before = groups;
            const outcome = groupOutcome(before, packed);
            publish(data.groups || [], data.nextNumber);
            modal.hide();
            if (outcome.created) notify(text('data-created', 'Grupos creados'));
            if (outcome.modified) notify(text('data-modified', 'Grupos modificados'));
        }).catch(function (err) {
            const message = (err && err.message) || text('data-error', 'No se han podido guardar los grupos');
            showMessage(message);
            notify(message);
        }).then(function () {
            saving = false;
            if (acceptBtn) acceptBtn.disabled = false;
        });
    }

    load().catch(function () { publish([]); });
    btn.addEventListener('click', function (ev) {
        ev.preventDefault();
        openSheet();
    });
    const searchInput = document.getElementById('desing2GroupSearch');
    function applySearch() {
        const q = String(searchInput && searchInput.value || '').trim().toLowerCase();
        const rows = modalEl.querySelectorAll('#desing2GroupExisting tr, #desing2GroupNew tr');
        for (let i = 0; i < rows.length; i++) {
            const tr = rows[i];
            if (tr.classList.contains('desing2-group-sheet__empty')) {
                tr.hidden = !!q;
                continue;
            }
            const numberInput = tr.querySelector('.desing2-group-number');
            const nameInput = tr.querySelector('.desing2-group-name');
            const hay = ((numberInput && numberInput.value) || '') + ' ' + ((nameInput && nameInput.value) || '');
            tr.hidden = !!q && hay.toLowerCase().indexOf(q) < 0;
        }
    }
    if (searchInput) searchInput.addEventListener('input', applySearch);
    const newTab = document.getElementById('desing2GroupTabNew');
    if (newTab) {
        newTab.addEventListener('shown.bs.tab', function () {
            assignNext(newBody.querySelector('tr'));
        });
    }
    newBody.addEventListener('focusin', function (ev) {
        const tr = ev.target && ev.target.closest ? ev.target.closest('tr') : null;
        if (!tr || !newBody.contains(tr)) return;
        assignNext(tr);
    });
    if (addBtn) addBtn.addEventListener('click', function () {
        let tr = firstEmptyNewRow();
        if (!tr) {
            appendNew();
            tr = newBody.lastElementChild;
        }
        assignNext(tr);
        if (newTab && window.bootstrap.Tab) window.bootstrap.Tab.getOrCreateInstance(newTab).show();
        const name = tr && tr.querySelector('.desing2-group-name');
        if (name) name.focus();
    });
    if (acceptBtn) acceptBtn.addEventListener('click', save);
    if (cancelBtn) cancelBtn.addEventListener('click', function () { modal.hide(); });
}
