(function () {
    'use strict';

    function getI18n() {
        var root = getRoot();
        if (!root) return {};
        var raw = root.getAttribute('data-i18n');
        if (!raw) return {};
        try {
            return JSON.parse(raw);
        } catch (e) {
            return {};
        }
    }

    function msg(key, fallback) {
        var i18n = getI18n();
        return i18n[key] || fallback || '';
    }

    function getAntiForgeryToken() {
        var el = document.querySelector('#companyForm input[name="__RequestVerificationToken"]');
        return el ? el.value : '';
    }

    function getRoot() {
        return document.getElementById('companyBranchesPanelRoot');
    }

    function refreshBranchModalAddressUi() {
        if (!window.jQuery || !window.TandemAddressPlaces || !window.TandemAddressPlaces.initBlocks) {
            return Promise.resolve();
        }
        var $b = window.jQuery('#companyBranchModal fieldset.tandem-address-block[data-prefix="BrLoc"]');
        if ($b.length) {
            return window.TandemAddressPlaces.initBlocks($b);
        }
        return Promise.resolve();
    }

    function syncBranchPacSearch(text) {
        var host = document.getElementById('BrLoc_PlacesSearch');
        if (!host) return;
        var inner = host.firstElementChild;
        if (inner && 'value' in inner) {
            try {
                inner.value = text != null ? String(text) : '';
            } catch (e) { /* ignore */ }
        }
    }

    function clearBranchModalLocFields() {
        ['Place_Id', 'Formatted_Address', 'Lat', 'Lng', 'Street_Number', 'Route', 'Subpremise', 'Locality', 'Admin_Area_1', 'Admin_Area_2', 'Postal_Code', 'Country_Code', 'Country_Name', 'Address_Components_Json'].forEach(function (s) {
            var el = document.getElementById('BrLoc_' + s);
            if (el) el.value = '';
        });
        syncBranchPacSearch('');
    }

    function fillBranchModalLocFromJson(rawAttr, runRefresh) {
        clearBranchModalLocFields();
        var doRefresh = runRefresh !== false;
        if (!rawAttr || rawAttr === '{}' || rawAttr === '') {
            return doRefresh ? refreshBranchModalAddressUi() : Promise.resolve();
        }
        var o = JSON.parse(rawAttr);
        function set(suf, v) {
            var el = document.getElementById('BrLoc_' + suf);
            if (!el) return;
            el.value = v !== undefined && v !== null ? String(v) : '';
        }
        set('Place_Id', o.pi);
        set('Formatted_Address', o.fa);
        set('Lat', o.lat);
        set('Lng', o.lng);
        set('Street_Number', o.sn);
        set('Route', o.rt);
        set('Subpremise', o.sp);
        set('Locality', o.loc);
        set('Admin_Area_1', o.a1);
        set('Admin_Area_2', o.a2);
        set('Postal_Code', o.pc);
        set('Country_Code', o.cc);
        set('Country_Name', o.cn);
        set('Address_Components_Json', o.cj);
        if (doRefresh) {
            return refreshBranchModalAddressUi().then(function () {
                if (o.fa) syncBranchPacSearch(o.fa);
            });
        }
        if (o.fa) syncBranchPacSearch(o.fa);
        return Promise.resolve();
    }

    function appendBranchLocToPayload(payload) {
        [['Loc_Place_Id', 'Place_Id'], ['Loc_Formatted_Address', 'Formatted_Address'], ['Loc_Lat', 'Lat'], ['Loc_Lng', 'Lng'], ['Loc_Street_Number', 'Street_Number'], ['Loc_Route', 'Route'], ['Loc_Subpremise', 'Subpremise'], ['Loc_Locality', 'Locality'], ['Loc_Admin_Area_1', 'Admin_Area_1'], ['Loc_Admin_Area_2', 'Admin_Area_2'], ['Loc_Postal_Code', 'Postal_Code'], ['Loc_Country_Code', 'Country_Code'], ['Loc_Country_Name', 'Country_Name'], ['Loc_Address_Components_Json', 'Address_Components_Json']].forEach(function (p) {
            var el = document.getElementById('BrLoc_' + p[1]);
            payload[p[0]] = el ? el.value : '';
        });
    }

    function wireBranchColorPair(pickerId, textId) {
        var p = document.getElementById(pickerId);
        var t = document.getElementById(textId);
        if (!p || !t) return;
        function applyPickerFromText() {
            var v = (t.value || '').trim();
            if (/^#[0-9A-Fa-f]{6}$/.test(v)) {
                p.value = v;
            }
        }
        p.addEventListener('input', function () { t.value = p.value; });
        p.addEventListener('change', function () { t.value = p.value; });
        t.addEventListener('input', applyPickerFromText);
        applyPickerFromText();
    }

    function setBranchColorUi(pickerId, textId, rawHex) {
        var p = document.getElementById(pickerId);
        var t = document.getElementById(textId);
        if (!p || !t) return;
        var v = (rawHex || '').trim();
        if (/^#[0-9A-Fa-f]{6}$/.test(v)) {
            t.value = v;
            p.value = v;
        } else {
            t.value = '';
            p.value = '#808080';
        }
    }

    function refreshList() {
        var root = getRoot();
        if (!root) return;
        var companyId = root.getAttribute('data-company-id');
        var listUrl = root.getAttribute('data-list-url');
        var mount = document.getElementById('companyBranchListMount');
        if (!companyId || !listUrl || !mount) return;

        var url = listUrl + (listUrl.indexOf('?') >= 0 ? '&' : '?') + 'companyId=' + encodeURIComponent(companyId);
        fetch(url, { credentials: 'same-origin' })
            .then(function (r) { return r.text(); })
            .then(function (html) {
                mount.innerHTML = html;
                bindRows();
            })
            .catch(function () {
                window.alert(msg('refreshFail', 'No se pudo actualizar la lista de sedes.'));
            });
    }

    function bindRows() {
        var root = getRoot();
        if (!root) return;
        root.querySelectorAll('.btn-edit-branch').forEach(function (btn) {
            btn.addEventListener('click', onEditClick);
        });
        root.querySelectorAll('.btn-delete-branch').forEach(function (btn) {
            btn.addEventListener('click', onDeleteClick);
        });
    }

    function clearModalForCreate() {
        document.getElementById('companyBranchModalSysObjectID').value = '';
        document.getElementById('companyBranchModalAttLabel').value = '';
        document.getElementById('companyBranchModalAttDescription').value = '';
        document.getElementById('companyBranchModalAddLetter').value = '';
        setBranchColorUi('companyBranchModalAttcolorPicker', 'companyBranchModalAttcolorText', '');
        clearBranchModalLocFields();
        document.getElementById('companyBranchModalTitle').textContent = msg('modalTitleCreate', 'Nueva sede');
        refreshBranchModalAddressUi();
    }

    function onEditClick(e) {
        var li = e.currentTarget.closest('.js-branch-row');
        if (!li) return;
        document.getElementById('companyBranchModalSysObjectID').value = li.getAttribute('data-branch-id') || '';
        document.getElementById('companyBranchModalAttLabel').value = li.getAttribute('data-label') || '';
        document.getElementById('companyBranchModalAttDescription').value = li.getAttribute('data-description') || '';
        document.getElementById('companyBranchModalAddLetter').value = li.getAttribute('data-letter') || '';
        setBranchColorUi('companyBranchModalAttcolorPicker', 'companyBranchModalAttcolorText', li.getAttribute('data-attcolor') || '');
        document.getElementById('companyBranchModalTitle').textContent = msg('modalTitleEdit', 'Editar sede');
        var locJson = li.getAttribute('data-loc-json') || '';
        fillBranchModalLocFromJson(locJson, false);
        showModal();
        window.setTimeout(function () {
            refreshBranchModalAddressUi().then(function () {
                try {
                    var parsed = JSON.parse(locJson || '{}');
                    if (parsed && parsed.fa) syncBranchPacSearch(parsed.fa);
                } catch (err) { /* ignore */ }
            });
        }, 350);
    }

    function onNewClick() {
        var root = getRoot();
        if (!root) return;
        var companyId = root.getAttribute('data-company-id');
        if (!companyId) {
            window.alert(msg('newNeedsBusiness', 'Primero guarde la empresa para poder añadir sedes.'));
            return;
        }
        clearModalForCreate();
        showModal();
        setTimeout(function () {
            var first = document.getElementById('companyBranchModalAttLabel');
            if (first) first.focus();
        }, 400);
    }

    function showModal() {
        var el = document.getElementById('companyBranchModal');
        if (!el || typeof window.bootstrap === 'undefined' || !window.bootstrap.Modal) return;
        var M = window.bootstrap.Modal;
        var inst = typeof M.getOrCreateInstance === 'function'
            ? M.getOrCreateInstance(el)
            : new M(el);
        inst.show();
    }

    function hideModal() {
        var el = document.getElementById('companyBranchModal');
        if (!el || typeof window.bootstrap === 'undefined' || !window.bootstrap.Modal) return;
        var inst = window.bootstrap.Modal.getInstance(el);
        if (inst) inst.hide();
    }

    function postForm(url, payload) {
        var body = payload || {};
        body.__RequestVerificationToken = getAntiForgeryToken();
        return fetch(url, {
            method: 'POST',
            headers: { 'Content-Type': 'application/x-www-form-urlencoded; charset=UTF-8' },
            credentials: 'same-origin',
            body: new URLSearchParams(body)
        }).then(function (r) { return r.json(); });
    }

    function onSaveClick() {
        var root = getRoot();
        if (!root) return;
        var companyId = root.getAttribute('data-company-id');
        var idStr = document.getElementById('companyBranchModalSysObjectID').value;
        var mt = document.getElementById('companyBranchModalAttcolorText');
        var payload = {
            companyId: companyId,
            attLabel: document.getElementById('companyBranchModalAttLabel').value,
            attDescription: document.getElementById('companyBranchModalAttDescription').value,
            addLetter: document.getElementById('companyBranchModalAddLetter').value,
            attcolor: mt ? (mt.value || '').trim() : ''
        };

        var url = idStr ? root.getAttribute('data-update-url') : root.getAttribute('data-create-url');
        if (idStr) {
            payload.sysObjectId = idStr;
        }

        appendBranchLocToPayload(payload);

        postForm(url, payload)
            .then(function (res) {
                if (!res || !res.IsOk) {
                    window.alert(res && res.Message ? res.Message : msg(idStr ? 'saveFail' : 'createFail', 'Error.'));
                    return;
                }
                hideModal();
                refreshList();
            })
            .catch(function () {
                window.alert(msg('networkError', 'Error de red.'));
            });
    }

    function onDeleteClick(e) {
        var li = e.currentTarget.closest('.js-branch-row');
        if (!li || !window.confirm(msg('deleteConfirm', '¿Eliminar esta sede?'))) return;
        var root = getRoot();
        postForm(root.getAttribute('data-delete-url'), {
            companyId: root.getAttribute('data-company-id'),
            sysObjectId: li.getAttribute('data-branch-id')
        })
            .then(function (res) {
                if (!res || !res.IsOk) {
                    window.alert(res && res.Message ? res.Message : msg('deleteFail', 'No se pudo eliminar.'));
                    return;
                }
                refreshList();
            })
            .catch(function () {
                window.alert(msg('networkError', 'Error de red.'));
            });
    }

    document.addEventListener('DOMContentLoaded', function () {
        var root = getRoot();
        if (!root) return;
        wireBranchColorPair('companyBranchModalAttcolorPicker', 'companyBranchModalAttcolorText');
        bindRows();
        var cid = root.getAttribute('data-company-id');

        var btnNew = document.getElementById('btnNewCompanyBranch');
        if (btnNew && cid) btnNew.addEventListener('click', onNewClick);

        var btnSave = document.getElementById('companyBranchModalSave');
        if (btnSave && cid) btnSave.addEventListener('click', onSaveClick);
    });
})();
