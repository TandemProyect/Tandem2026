/**
 * Desing_2 — modal mapa OSM: buscar dirección, cargar huellas y devolver edificio seleccionado.
 * Depende de Leaflet (window.L). El visor STL orquesta inserción en planta.
 */
(function (global) {
  'use strict';

  var DEFAULT_CENTER = [40.4168, -3.7038];
  var DEFAULT_ZOOM = 17;

  var state = {
    map: null,
    buildingsLayer: null,
    selectedLayer: null,
    selected: null,
    busy: false,
    modalEl: null,
    acceptBtn: null,
    selectedLabelEl: null,
    searchInput: null,
    urls: { search: '', buildings: '', importUrl: '' },
    onImported: null,
    onToast: null,
    suppressCloseCallback: false,
  };

  function attr(el, name) {
    return el ? el.getAttribute(name) || '' : '';
  }

  function formatTpl(tpl, value) {
    if (!tpl) return value == null ? '' : String(value);
    return String(tpl).replace(/\{0\}/g, value == null ? '' : String(value));
  }

  function toast(msg) {
    if (typeof state.onToast === 'function' && msg) state.onToast(msg);
  }

  function syncAcceptUi() {
    if (!state.acceptBtn) return;
    state.acceptBtn.disabled = !state.selected || state.busy;
  }

  function setSelectedLabel(text) {
    if (state.selectedLabelEl) state.selectedLabelEl.textContent = text || '';
  }

  function clearSelection() {
    state.selected = null;
    if (state.selectedLayer && state.buildingsLayer) {
      try {
        state.buildingsLayer.resetStyle(state.selectedLayer);
      } catch (e) {
        /* ignore */
      }
    }
    state.selectedLayer = null;
    setSelectedLabel('');
    syncAcceptUi();
  }

  function styleBuilding(feature) {
    return {
      color: '#0d6efd',
      weight: 1.5,
      opacity: 0.9,
      fillColor: '#0d6efd',
      fillOpacity: 0.22,
    };
  }

  function styleSelected() {
    return {
      color: '#dc3545',
      weight: 2.5,
      opacity: 1,
      fillColor: '#dc3545',
      fillOpacity: 0.35,
    };
  }

  function ringToLatLngs(ring) {
    var out = [];
    if (!ring || !ring.length) return out;
    for (var i = 0; i < ring.length; i++) {
      var p = ring[i];
      if (!p) continue;
      var lat = Number(p.Lat != null ? p.Lat : p.lat);
      var lng = Number(p.Lng != null ? p.Lng : p.lng);
      if (!isFinite(lat) || !isFinite(lng)) continue;
      out.push([lat, lng]);
    }
    return out;
  }

  function ensureMap() {
    if (state.map || typeof global.L === 'undefined') return state.map;
    var mapEl = document.getElementById('ma-stl-map-building-map');
    if (!mapEl) return null;

    state.map = global.L.map(mapEl, {
      zoomControl: true,
      attributionControl: true,
    }).setView(DEFAULT_CENTER, DEFAULT_ZOOM);

    global.L
      .tileLayer('https://{s}.tile.openstreetmap.org/{z}/{x}/{y}.png', {
        maxZoom: 19,
        attribution: '&copy; OpenStreetMap',
      })
      .addTo(state.map);

    state.buildingsLayer = global.L.featureGroup().addTo(state.map);
    return state.map;
  }

  function invalidateMapSize() {
    if (!state.map) return;
    setTimeout(function () {
      try {
        state.map.invalidateSize();
      } catch (e) {
        /* ignore */
      }
    }, 180);
  }

  function clearBuildings() {
    clearSelection();
    if (state.buildingsLayer) state.buildingsLayer.clearLayers();
  }

  function selectBuilding(bldg, layer) {
    if (state.selectedLayer && state.buildingsLayer) {
      try {
        state.buildingsLayer.resetStyle(state.selectedLayer);
      } catch (e) {
        /* ignore */
      }
    }
    state.selected = bldg;
    state.selectedLayer = layer || null;
    if (layer && layer.setStyle) layer.setStyle(styleSelected());
    var tpl = attr(state.acceptBtn, 'data-ma-stl-map-building-selected-tpl');
    var label = bldg.TextLabel || bldg.textLabel || ('OSM ' + (bldg.OsmId || bldg.osmId || ''));
    setSelectedLabel(formatTpl(tpl, label));
    syncAcceptUi();
  }

  function renderBuildings(list) {
    clearBuildings();
    if (!state.map || !state.buildingsLayer || !list || !list.length) {
      var noneTpl = attr(state.acceptBtn, 'data-ma-stl-map-building-none-loaded');
      if (noneTpl) toast(noneTpl);
      return;
    }

    for (var i = 0; i < list.length; i++) {
      (function (bldg) {
        var latlngs = ringToLatLngs(bldg.Ring || bldg.ring);
        if (latlngs.length < 3) return;
        var poly = global.L.polygon(latlngs, styleBuilding(bldg));
        poly.on('click', function (ev) {
          if (ev && ev.originalEvent) {
            global.L.DomEvent.stopPropagation(ev.originalEvent);
          }
          selectBuilding(bldg, poly);
        });
        poly.bindTooltip(bldg.TextLabel || bldg.textLabel || '', { sticky: true });
        state.buildingsLayer.addLayer(poly);
      })(list[i]);
    }

    try {
      var bounds = state.buildingsLayer.getBounds();
      if (bounds && bounds.isValid && bounds.isValid()) {
        state.map.fitBounds(bounds.pad(0.08));
      }
    } catch (e) {
      /* ignore */
    }
  }

  function getUrlsFromShell(shell) {
    return {
      search: shell ? shell.getAttribute('data-ma-stl-map-building-search-url') || '' : '',
      buildings: shell ? shell.getAttribute('data-ma-stl-map-building-buildings-url') || '' : '',
      importUrl: shell ? shell.getAttribute('data-ma-stl-map-building-import-url') || '' : '',
    };
  }

  function searchAddress() {
    if (state.busy || !state.urls.search) return;
    var q = state.searchInput ? String(state.searchInput.value || '').trim() : '';
    if (!q) return;
    var searchingTpl = attr(state.acceptBtn, 'data-ma-stl-map-building-searching');
    if (searchingTpl) toast(searchingTpl);
    state.busy = true;
    syncAcceptUi();
    var url = state.urls.search + (state.urls.search.indexOf('?') >= 0 ? '&' : '?') + 'q=' + encodeURIComponent(q);
    fetch(url, { method: 'GET', credentials: 'same-origin' })
      .then(function (res) {
        if (!res.ok) throw new Error('HTTP ' + res.status);
        return res.json();
      })
      .then(function (resp) {
        state.busy = false;
        syncAcceptUi();
        if (!resp || !resp.Exito || !resp.Datos || !resp.Datos.length) {
          var errTpl = attr(state.acceptBtn, 'data-ma-stl-map-building-error');
          toast(formatTpl(errTpl, (resp && resp.Mensaje) || 'Sin resultados'));
          return;
        }
        var first = resp.Datos[0];
        var lat = Number(first.Lat != null ? first.Lat : first.lat);
        var lng = Number(first.Lng != null ? first.Lng : first.lng);
        ensureMap();
        if (state.map && isFinite(lat) && isFinite(lng)) {
          var south = first.South != null ? Number(first.South) : Number(first.south);
          var west = first.West != null ? Number(first.West) : Number(first.west);
          var north = first.North != null ? Number(first.North) : Number(first.north);
          var east = first.East != null ? Number(first.East) : Number(first.east);
          if (
            isFinite(south) &&
            isFinite(west) &&
            isFinite(north) &&
            isFinite(east) &&
            south < north &&
            west < east
          ) {
            state.map.fitBounds([
              [south, west],
              [north, east],
            ]);
          } else {
            state.map.setView([lat, lng], Math.max(state.map.getZoom(), 18));
          }
          loadBuildings();
        }
      })
      .catch(function (err) {
        state.busy = false;
        syncAcceptUi();
        var errTpl = attr(state.acceptBtn, 'data-ma-stl-map-building-error');
        toast(formatTpl(errTpl, err && err.message ? err.message : err));
      });
  }

  function loadBuildings() {
    if (state.busy || !state.urls.buildings) return;
    ensureMap();
    if (!state.map) return;
    var b = state.map.getBounds();
    if (!b) return;
    var loadingTpl = attr(state.acceptBtn, 'data-ma-stl-map-building-loading');
    if (loadingTpl) toast(loadingTpl);
    state.busy = true;
    syncAcceptUi();
    var body = {
      South: b.getSouth(),
      West: b.getWest(),
      North: b.getNorth(),
      East: b.getEast(),
    };
    fetch(state.urls.buildings, {
      method: 'POST',
      credentials: 'same-origin',
      headers: { 'Content-Type': 'application/json', Accept: 'application/json' },
      body: JSON.stringify(body),
    })
      .then(function (res) {
        if (!res.ok) throw new Error('HTTP ' + res.status);
        return res.json();
      })
      .then(function (resp) {
        state.busy = false;
        syncAcceptUi();
        if (!resp || !resp.Exito) {
          var errTpl = attr(state.acceptBtn, 'data-ma-stl-map-building-error');
          toast(formatTpl(errTpl, (resp && resp.Mensaje) || 'Error'));
          return;
        }
        renderBuildings(resp.Datos || []);
      })
      .catch(function (err) {
        state.busy = false;
        syncAcceptUi();
        var errTpl = attr(state.acceptBtn, 'data-ma-stl-map-building-error');
        toast(formatTpl(errTpl, err && err.message ? err.message : err));
      });
  }

  function importSelected() {
    if (state.busy) return;
    if (!state.selected) {
      var noSel = attr(state.acceptBtn, 'data-ma-stl-map-building-no-selection');
      if (noSel) toast(noSel);
      return;
    }
    if (!state.urls.importUrl) return;
    var processingTpl = attr(state.acceptBtn, 'data-ma-stl-map-building-processing');
    if (processingTpl) toast(processingTpl);
    state.busy = true;
    syncAcceptUi();
    var payload = {
      OsmId: state.selected.OsmId != null ? state.selected.OsmId : state.selected.osmId,
      TextLabel: state.selected.TextLabel || state.selected.textLabel || null,
      Ring: state.selected.Ring || state.selected.ring || [],
    };
    fetch(state.urls.importUrl, {
      method: 'POST',
      credentials: 'same-origin',
      headers: { 'Content-Type': 'application/json', Accept: 'application/json' },
      body: JSON.stringify(payload),
    })
      .then(function (res) {
        if (!res.ok) throw new Error('HTTP ' + res.status);
        return res.json();
      })
      .then(function (resp) {
        state.busy = false;
        syncAcceptUi();
        if (!resp || !resp.Exito || !resp.Datos) {
          var errTpl = attr(state.acceptBtn, 'data-ma-stl-map-building-error');
          toast(formatTpl(errTpl, (resp && resp.Mensaje) || 'Error'));
          return;
        }
        state.suppressCloseCallback = true;
        hideModal();
        if (typeof state.onImported === 'function') {
          state.onImported(resp.Datos, resp.Mensaje || '');
        }
      })
      .catch(function (err) {
        state.busy = false;
        syncAcceptUi();
        var errTpl = attr(state.acceptBtn, 'data-ma-stl-map-building-error');
        toast(formatTpl(errTpl, err && err.message ? err.message : err));
      });
  }

  function getModalInstance() {
    if (!state.modalEl || typeof global.bootstrap === 'undefined' || !global.bootstrap.Modal) {
      return null;
    }
    return global.bootstrap.Modal.getOrCreateInstance(state.modalEl);
  }

  function showModal() {
    var inst = getModalInstance();
    if (inst) inst.show();
  }

  function hideModal() {
    var inst = getModalInstance();
    if (inst) inst.hide();
  }

  function bindOnce() {
    if (state.modalEl && state.modalEl.getAttribute('data-desing2-map-bound') === '1') return;
    state.modalEl = document.getElementById('ma-stl-map-building-modal');
    if (!state.modalEl) return;
    state.modalEl.setAttribute('data-desing2-map-bound', '1');
    state.acceptBtn = document.getElementById('ma-stl-map-building-accept');
    state.selectedLabelEl = document.getElementById('ma-stl-map-building-selected-label');
    state.searchInput = document.getElementById('ma-stl-map-building-search-input');
    var searchBtn = document.getElementById('ma-stl-map-building-search-btn');
    var loadBtn = document.getElementById('ma-stl-map-building-load-btn');
    var cancelBtn = document.getElementById('ma-stl-map-building-cancel');

    if (searchBtn) {
      searchBtn.addEventListener('click', function (ev) {
        ev.preventDefault();
        searchAddress();
      });
    }
    if (state.searchInput) {
      state.searchInput.addEventListener('keydown', function (ev) {
        if (ev.key === 'Enter') {
          ev.preventDefault();
          searchAddress();
        }
      });
    }
    if (loadBtn) {
      loadBtn.addEventListener('click', function (ev) {
        ev.preventDefault();
        loadBuildings();
      });
    }
    if (state.acceptBtn) {
      state.acceptBtn.addEventListener('click', function (ev) {
        ev.preventDefault();
        importSelected();
      });
    }
    if (cancelBtn) {
      cancelBtn.addEventListener('click', function () {
        /* modal hide handled by data-bs-dismiss; viewer listens hidden */
      });
    }
    state.modalEl.addEventListener('shown.bs.modal', function () {
      ensureMap();
      invalidateMapSize();
    });
  }

  function open(options) {
    options = options || {};
    bindOnce();
    var shell = options.shell || document.getElementById('ma-stl-viewer-shell');
    state.urls = getUrlsFromShell(shell);
    state.onImported = options.onImported || null;
    state.onToast = options.onToast || null;
    state.suppressCloseCallback = false;
    state.busy = false;
    clearSelection();
    showModal();
    ensureMap();
    invalidateMapSize();
    syncAcceptUi();
  }

  function close() {
    state.suppressCloseCallback = true;
    hideModal();
  }

  function consumeSuppressClose() {
    if (!state.suppressCloseCallback) return false;
    state.suppressCloseCallback = false;
    return true;
  }

  function isBusy() {
    return state.busy === true;
  }

  global.Desing2MapBuildingImport = {
    open: open,
    close: close,
    consumeSuppressClose: consumeSuppressClose,
    isBusy: isBusy,
  };
})(typeof window !== 'undefined' ? window : this);
