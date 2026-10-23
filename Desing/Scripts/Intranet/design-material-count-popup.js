(function () {
  'use strict';

  var modalEl = document.getElementById('designMaterialCountModal');
  if (!modalEl || !window.bootstrap || !window.jQuery || !window.jQuery.fn || !window.jQuery.fn.DataTable) return;

  var modal = window.bootstrap.Modal.getOrCreateInstance(modalEl);
  var titleEl = document.getElementById('designMaterialCountTitle');
  var summaryEl = document.getElementById('designMaterialCountSummary');
  var searchHost = document.getElementById('designMaterialCountSearch');
  var buttonHost = document.getElementById('designMaterialCountButtons');
  var articlesDt = null;
  var detailsDt = null;
  var lastRows = [];

  function num(value) {
    var n = Number(value);
    return isFinite(n) ? n : 0;
  }

  function fmt(value, digits) {
    return num(value).toLocaleString('es-ES', {
      minimumFractionDigits: digits,
      maximumFractionDigits: digits
    });
  }

  function attr(name) {
    return modalEl.getAttribute(name) || '';
  }

  function numberColumn(data, digits) {
    return {
      data: data,
      className: 'text-end',
      defaultContent: '',
      render: function (value, type) {
        var n = num(value);
        if (type === 'sort' || type === 'type' || type === 'filter') return n;
        return fmt(n, digits);
      }
    };
  }

  function language() {
    return {
      search: attr('data-search') || 'Buscar:',
      emptyTable: attr('data-empty'),
      zeroRecords: attr('data-empty')
    };
  }

  function exportButtons(columns) {
    var excel = attr('data-excel');
    var pdf = attr('data-pdf');
    return [
      {
        extend: 'excel',
        text: '<i class="icon-base ri ri-file-excel-2-line" aria-hidden="true"></i>',
        className: 'btn btn-sm btn-icon btn-text-secondary rounded-pill',
        titleAttr: excel,
        title: excel,
        exportOptions: { columns: columns }
      },
      {
        extend: 'pdf',
        text: '<i class="icon-base ri ri-file-pdf-2-line" aria-hidden="true"></i>',
        className: 'btn btn-sm btn-icon btn-text-secondary rounded-pill',
        titleAttr: pdf,
        title: pdf,
        exportOptions: { columns: columns },
        orientation: 'landscape',
        pageSize: 'A4'
      }
    ];
  }

  function parkChrome(table, moveButtons) {
    var root = table.table().container();
    var filter = root.querySelector('.dataTables_filter');
    if (filter && searchHost && filter.parentNode !== searchHost) searchHost.appendChild(filter);
    if (moveButtons && buttonHost) {
      var box = root.querySelector('.dt-buttons');
      if (box && box.parentNode !== buttonHost) buttonHost.appendChild(box);
    }
  }

  function showActiveSearch() {
    if (!searchHost) return;
    var detailsOn = document.getElementById('designMaterialCountPaneDetails').classList.contains('active');
    var filters = searchHost.querySelectorAll('.dataTables_filter');
    for (var i = 0; i < filters.length; i++) {
      var forDetails = filters[i].id.indexOf('designMaterialCountDetails') >= 0;
      filters[i].hidden = detailsOn ? !forDetails : forDetails;
    }
  }

  function ensureTables() {
    if (articlesDt && detailsDt) return;
    articlesDt = window.jQuery('#designMaterialCountArticles').DataTable({
      data: [],
      paging: false,
      searching: true,
      info: false,
      ordering: true,
      order: [[0, 'asc']],
      lengthChange: false,
      autoWidth: false,
      dom: 'frt',
      language: language(),
      columns: [
        { data: 'code', defaultContent: '' },
        { data: 'name', defaultContent: '' },
        numberColumn('qty', 0)
      ],
      initComplete: function () { parkChrome(this.api(), false); showActiveSearch(); }
    });
    detailsDt = window.jQuery('#designMaterialCountDetails').DataTable({
      data: [],
      paging: false,
      searching: true,
      info: false,
      ordering: true,
      order: [[1, 'asc']],
      lengthChange: false,
      autoWidth: false,
      dom: 'Bfrt',
      language: language(),
      buttons: exportButtons([0, 1, 2, 3, 4, 5, 6, 7]),
      columns: [
        { data: 'n', className: 'text-end', defaultContent: '' },
        { data: 'code', defaultContent: '' },
        { data: 'name', defaultContent: '' },
        numberColumn('qty', 0),
        numberColumn('unitWeight', 2),
        numberColumn('totalWeight', 2),
        numberColumn('unitMts2', 2),
        numberColumn('totalMts2', 2)
      ],
      initComplete: function () { parkChrome(this.api(), true); showActiveSearch(); }
    });
  }

  function paintSummary(rows) {
    var articleQty = 0;
    var totalWeight = 0;
    var totalMts2 = 0;
    for (var i = 0; i < rows.length; i++) {
      var row = rows[i];
      articleQty += num(row.qty);
      totalWeight += num(row.totalWeight);
      totalMts2 += num(row.totalMts2);
    }
    summaryEl.innerHTML = ''
      + '<div class="desing2-material-count__summary-item"><span>' + attr('data-sum-articles') + '</span><strong>' + fmt(articleQty, 0) + '</strong></div>'
      + '<div class="desing2-material-count__summary-item"><span>' + attr('data-sum-weight') + '</span><strong>' + fmt(totalWeight, 2) + ' kg</strong></div>'
      + '<div class="desing2-material-count__summary-item"><span>' + attr('data-sum-m2') + '</span><strong>' + fmt(totalMts2, 2) + ' m²</strong></div>';
  }

  function mapRows(rows) {
    var list = [];
    for (var i = 0; i < rows.length; i++) {
      var row = rows[i] || {};
      var qty = num(row.qty);
      var unitWeight = num(row.unitWeight);
      var unitMts2 = num(row.unitMts2);
      list.push({
        n: i + 1,
        code: row.code == null ? '' : String(row.code),
        name: row.name == null ? '' : String(row.name),
        qty: qty,
        unitWeight: unitWeight,
        totalWeight: num(row.totalWeight || unitWeight * qty),
        unitMts2: unitMts2,
        totalMts2: num(row.totalMts2 || unitMts2 * qty)
      });
    }
    return list;
  }

  function paint(rows) {
    ensureTables();
    lastRows = mapRows(rows || []);
    articlesDt.clear().rows.add(lastRows).draw();
    detailsDt.clear().rows.add(lastRows).draw();
    paintSummary(lastRows);
  }

  function fail() {
    paint([]);
    summaryEl.innerHTML = '<div class="desing2-material-count__summary-item"><span class="text-danger">' + attr('data-error') + '</span></div>';
  }

  modalEl.addEventListener('shown.bs.tab', function () {
    showActiveSearch();
    if (articlesDt) articlesDt.columns.adjust();
    if (detailsDt) detailsDt.columns.adjust();
  });

  modalEl.addEventListener('shown.bs.modal', function () {
    if (articlesDt) articlesDt.columns.adjust();
    if (detailsDt) detailsDt.columns.adjust();
  });

  document.addEventListener('click', function (ev) {
    var btn = ev.target.closest ? ev.target.closest('.js-design-material-count') : null;
    if (!btn) return;
    ev.preventDefault();
    ev.stopPropagation();
    var id = btn.getAttribute('data-design-id');
    var name = btn.getAttribute('data-design-name') || '';
    titleEl.textContent = name || attr('data-title');
    paint([]);
    var url = attr('data-url') + '?designId=' + encodeURIComponent(id || '');
    modal.show();
    fetch(url, { credentials: 'same-origin', headers: { Accept: 'application/json' } })
      .then(function (res) { return res.json(); })
      .then(function (data) {
        if (!data || data.ok === false || data.Ok === false) {
          fail();
          return;
        }
        paint(data.rows || data.Rows || []);
      })
      .catch(fail);
  });
})();
