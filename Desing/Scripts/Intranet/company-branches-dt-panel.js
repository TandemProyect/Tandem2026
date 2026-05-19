(function (window, $) {
    'use strict';

    if (!$ || !$.fn.dataTable) {
        return;
    }

    function modalMsg(root, key, fallback) {
        var raw = root.getAttribute('data-modal-i18n');
        if (!raw) return fallback || '';
        try {
            var o = JSON.parse(raw);
            return o[key] || fallback || '';
        } catch (e) {
            return fallback || '';
        }
    }

    function dtCfg(root, key, fallback) {
        var raw = root.getAttribute('data-dt-i18n');
        if (!raw) return fallback || '';
        try {
            var o = JSON.parse(raw);
            return o[key] || fallback || '';
        } catch (e) {
            return fallback || '';
        }
    }

    function getAntiForgeryToken() {
        var el = document.querySelector('#companyForm input[name="__RequestVerificationToken"]');
        return el ? el.value : '';
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

    function clearBranchModalLocFields() {
        ['Place_Id', 'Formatted_Address', 'Lat', 'Lng', 'Street_Number', 'Route', 'Subpremise', 'Locality', 'Admin_Area_1', 'Admin_Area_2', 'Postal_Code', 'Country_Code', 'Country_Name', 'Address_Components_Json'].forEach(function (s) {
            var el = document.getElementById('BrLoc_' + s);
            if (el) el.value = '';
        });
        var host = document.getElementById('BrLoc_PlacesSearch');
        if (host && host.firstElementChild && 'value' in host.firstElementChild) {
            try {
                host.firstElementChild.value = '';
            } catch (e) { /* ignore */ }
        }
    }

    function appendBranchLocToPayload(payload) {
        [['Loc_Place_Id', 'Place_Id'], ['Loc_Formatted_Address', 'Formatted_Address'], ['Loc_Lat', 'Lat'], ['Loc_Lng', 'Lng'], ['Loc_Street_Number', 'Street_Number'], ['Loc_Route', 'Route'], ['Loc_Subpremise', 'Subpremise'], ['Loc_Locality', 'Locality'], ['Loc_Admin_Area_1', 'Admin_Area_1'], ['Loc_Admin_Area_2', 'Admin_Area_2'], ['Loc_Postal_Code', 'Postal_Code'], ['Loc_Country_Code', 'Country_Code'], ['Loc_Country_Name', 'Country_Name'], ['Loc_Address_Components_Json', 'Address_Components_Json']].forEach(function (p) {
            var el = document.getElementById('BrLoc_' + p[1]);
            payload[p[0]] = el ? el.value : '';
        });
    }

    function clearModalCreate(root) {
        document.getElementById('companyBranchModalSysObjectID').value = '';
        document.getElementById('companyBranchModalAttLabel').value = '';
        document.getElementById('companyBranchModalAttDescription').value = '';
        document.getElementById('companyBranchModalAddLetter').value = '';
        setBranchColorUi('companyBranchModalAttcolorPicker', 'companyBranchModalAttcolorText', '');
        clearBranchModalLocFields();
        document.getElementById('companyBranchModalTitle').textContent =
            modalMsg(root, 'modalTitleCreate', 'Nueva sede');
        refreshBranchModalAddressUi();
    }

    document.addEventListener('DOMContentLoaded', function () {
        var root = document.getElementById('companyBranchesDtRoot');
        if (!root) return;

        wireBranchColorPair('companyBranchModalAttcolorPicker', 'companyBranchModalAttcolorText');

        var companyId = root.getAttribute('data-company-id');
        var branchListExportOpts = $.extend(true, {}, window.TandemDataTablesList.exportOptsPlainVisible, {
            format: {
                body: function (data, type, row, meta) {
                    return window.TandemDataTablesList.stripHtmlForExport(data, type, row, meta);
                }
            }
        });

        var dtUi = {
            rowsSuffix: dtCfg(root, 'rowsSuffix', 'filas'),
            allRows: dtCfg(root, 'allRows', 'Todas'),
            sectionRecords: dtCfg(root, 'sectionRecords', 'Registros'),
            sectionExport: dtCfg(root, 'sectionExport', 'Exportar'),
            sectionCols: dtCfg(root, 'sectionCols', 'Columnas visibles'),
            ariaMenu: dtCfg(root, 'ariaMenu', ''),
            confirmDeleteBranch: dtCfg(root, 'confirmDeleteBranch', '')
        };

        var listUrl = root.getAttribute('data-list-branches-url');

        var dt = $('#ListCompanyBranchesEdit').DataTable(window.TandemDataTablesList.applyListDefaults({
            lengthMenu: [
                [10, 25, 50, -1],
                ['10 ' + dtUi.rowsSuffix, '25 ' + dtUi.rowsSuffix, '50 ' + dtUi.rowsSuffix, dtUi.allRows]
            ],
            buttons: [{
                extend: 'collection',
                text: "<i class='fas fa-bars'></i>",
                attr: { 'aria-label': dtUi.ariaMenu },
                className: 'custom-html-collection',
                buttons: [
                    '<h5>' + dtUi.sectionRecords + '</h5>',
                    'pageLength',
                    '<h5>' + dtUi.sectionExport + '</h5>',
                    'print',
                    'copy',
                    'pdf',
                    { extend: 'csv', exportOptions: branchListExportOpts },
                    { extend: 'excel', exportOptions: branchListExportOpts },
                    '<h5 class="not-top-heading">' + dtUi.sectionCols + '</h5>',
                    'colvis'
                ]
            }],
            serverSide: true,
            processing: true,
            colReorder: { iFixedColumnsRight: 1 },
            ajax: {
                url: listUrl,
                type: 'POST'
            },
            order: [[0, 'asc']],
            columns: [
                {
                    data: 'AttLabel',
                    name: 'AttLabel',
                    render: function (data, type, row) {
                        if (type === 'export') {
                            return row.AttLabelPlain || '';
                        }
                        return data;
                    }
                },
                {
                    data: 'LetterHtml',
                    name: 'LetterHtml',
                    orderable: true,
                    render: function (data, type) {
                        if (type === 'export') {
                            return window.TandemDataTablesList.stripHtmlForExport(data, type);
                        }
                        return data;
                    }
                },
                {
                    data: 'AttDescription',
                    name: 'AttDescription',
                    render: function (data, type, row) {
                        if (type === 'export' || type === 'sort' || type === 'filter') {
                            return row.AttDescriptionPlain != null ? row.AttDescriptionPlain : '';
                        }
                        return data;
                    }
                },
                {
                    data: 'rowActions',
                    orderable: false,
                    searchable: false,
                    className: 'tandem-col-actions text-end align-middle text-nowrap'
                },
                { data: 'SysObjectID', visible: false }
            ],
            drawCallback: function () {
                $('[data-toggle="tooltip"]').tooltip({
                    delay: { show: 100, hide: 100 },
                    placement: 'auto right'
                });
                $('[title]').tooltip({
                    delay: { show: 100, hide: 100 },
                    placement: 'auto'
                });
            }
        }));

        var token = getAntiForgeryToken();

        $('#ListCompanyBranchesEdit').on('click', '[data-branch-delete]', function () {
            var sid = $(this).attr('data-branch-delete');
            if (!sid || !confirm(dtUi.confirmDeleteBranch)) return;
            postForm(root.getAttribute('data-delete-url'), {
                sysObjectId: sid,
                companyId: companyId
            })
                .then(function (res) {
                    window.alert(res && res.Message ? res.Message : '');
                    if (res && res.IsOk) {
                        dt.ajax.reload(null, false);
                    }
                })
                .catch(function () {
                    window.alert(modalMsg(root, 'networkError', 'Error de red.'));
                });
        });

        var btnNew = document.getElementById('btnNewCompanyBranchDt');
        if (btnNew) {
            btnNew.addEventListener('click', function () {
                clearModalCreate(root);
                showModal();
                setTimeout(function () {
                    document.getElementById('companyBranchModalAttLabel').focus();
                }, 400);
            });
        }

        document.getElementById('companyBranchModalSave').addEventListener('click', function () {
            var idStr = document.getElementById('companyBranchModalSysObjectID').value;
            var mt = document.getElementById('companyBranchModalAttcolorText');
            var payload = {
                companyId: companyId,
                attLabel: document.getElementById('companyBranchModalAttLabel').value,
                attDescription: document.getElementById('companyBranchModalAttDescription').value,
                addLetter: document.getElementById('companyBranchModalAddLetter').value,
                attcolor: mt ? (mt.value || '').trim() : ''
            };

            var url;
            if (!idStr) {
                url = root.getAttribute('data-create-url');
            } else {
                url = root.getAttribute('data-update-url');
                payload.sysObjectId = idStr;
            }

            appendBranchLocToPayload(payload);

            postForm(url, payload)
                .then(function (res) {
                    if (!res || !res.IsOk) {
                        window.alert(res && res.Message ? res.Message :
                            modalMsg(root, idStr ? 'saveFail' : 'createFail', 'Error.'));
                        return;
                    }
                    hideModal();
                    dt.ajax.reload(null, false);
                })
                .catch(function () {
                    window.alert(modalMsg(root, 'networkError', 'Error de red.'));
                });
        });
    });
})(window, jQuery);
