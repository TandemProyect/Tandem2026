/**
 * Desing_2 — modal mapa 3D OSM: buscar dirección, ver edificios extruidos,
 * seleccionar uno e importar huella. Depende de MapLibre GL (window.maplibregl).
 */
(function (global) {
  'use strict';

  var DEFAULT_CENTER = [-3.7038, 40.4168]; // lng, lat (MapLibre)
  var DEFAULT_ZOOM = 17;
  var DEFAULT_PITCH = 58;
  var DEFAULT_BEARING = -20;
  var DEFAULT_HEIGHT_M = 9;
  var SOURCE_ID = 'desing2-osm-buildings';
  var LAYER_FILL = 'desing2-osm-buildings-extrusion';
  var LAYER_OUTLINE = 'desing2-osm-buildings-outline';
  var STYLE_URL = 'https://tiles.openfreemap.org/styles/liberty';
  var MAPLIBRE_CSS = [
    'https://unpkg.com/maplibre-gl@4.7.1/dist/maplibre-gl.css',
    'https://cdn.jsdelivr.net/npm/maplibre-gl@4.7.1/dist/maplibre-gl.css',
  ];
  var MAPLIBRE_JS = [
    'https://unpkg.com/maplibre-gl@4.7.1/dist/maplibre-gl.js',
    'https://cdn.jsdelivr.net/npm/maplibre-gl@4.7.1/dist/maplibre-gl.js',
  ];
  var MAPLIBRE_LOAD_TIMEOUT_MS = 12000;

  var state = {
    map: null,
    buildingsById: {},
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
    mapLibrePromise: null,
  };

  function loadStylesheet(href) {
    return new Promise(function (resolve, reject) {
      var existing = document.querySelector('link[data-desing2-maplibre-css="1"]');
      if (existing) {
        resolve();
        return;
      }
      var link = document.createElement('link');
      link.rel = 'stylesheet';
      link.href = href;
      link.setAttribute('data-desing2-maplibre-css', '1');
      link.onload = function () {
        resolve();
      };
      link.onerror = function () {
        if (link.parentNode) link.parentNode.removeChild(link);
        reject(new Error('CSS MapLibre: ' + href));
      };
      document.head.appendChild(link);
    });
  }

  function loadScript(src) {
    return new Promise(function (resolve, reject) {
      if (global.maplibregl) {
        resolve(global.maplibregl);
        return;
      }
      var existing = document.querySelector('script[data-desing2-maplibre-js="1"]');
      if (existing) {
        existing.addEventListener('load', function () {
          if (global.maplibregl) resolve(global.maplibregl);
          else reject(new Error('MapLibre no disponible tras cargar script'));
        });
        existing.addEventListener('error', function () {
          reject(new Error('JS MapLibre: ' + src));
        });
        return;
      }
      var script = document.createElement('script');
      script.src = src;
      script.async = true;
      script.setAttribute('data-desing2-maplibre-js', '1');
      script.onload = function () {
        if (global.maplibregl) resolve(global.maplibregl);
        else reject(new Error('MapLibre no expuso window.maplibregl'));
      };
      script.onerror = function () {
        if (script.parentNode) script.parentNode.removeChild(script);
        reject(new Error('JS MapLibre: ' + src));
      };
      document.head.appendChild(script);
    });
  }

  function withTimeout(promise, ms, label) {
    return new Promise(function (resolve, reject) {
      var done = false;
      var timer = setTimeout(function () {
        if (done) return;
        done = true;
        reject(new Error((label || 'Timeout') + ' (' + ms + ' ms)'));
      }, ms);
      promise.then(
        function (v) {
          if (done) return;
          done = true;
          clearTimeout(timer);
          resolve(v);
        },
        function (err) {
          if (done) return;
          done = true;
          clearTimeout(timer);
          reject(err);
        }
      );
    });
  }

  function tryLoadFromList(list, loader) {
    var chain = Promise.reject(new Error('empty'));
    for (var i = 0; i < list.length; i++) {
      (function (url) {
        chain = chain.catch(function () {
          return withTimeout(loader(url), MAPLIBRE_LOAD_TIMEOUT_MS, 'Carga MapLibre');
        });
      })(list[i]);
    }
    return chain;
  }

  /** Carga MapLibre solo al abrir el modal (no bloquea el arranque de Desing_2). */
  function ensureMapLibre() {
    if (global.maplibregl) return Promise.resolve(global.maplibregl);
    if (state.mapLibrePromise) return state.mapLibrePromise;
    state.mapLibrePromise = tryLoadFromList(MAPLIBRE_CSS, loadStylesheet)
      .catch(function () {
        /* CSS opcional: seguir con JS aunque falle un CDN */
        return null;
      })
      .then(function () {
        return tryLoadFromList(MAPLIBRE_JS, loadScript);
      })
      .catch(function (err) {
        state.mapLibrePromise = null;
        throw err;
      });
    return state.mapLibrePromise;
  }

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
    setSelectedLabel('');
    syncAcceptUi();
    refreshBuildingPaint();
  }

  function ringToLngLat(ring) {
    var out = [];
    if (!ring || !ring.length) return out;
    for (var i = 0; i < ring.length; i++) {
      var p = ring[i];
      if (!p) continue;
      var lat = Number(p.Lat != null ? p.Lat : p.lat);
      var lng = Number(p.Lng != null ? p.Lng : p.lng);
      if (!isFinite(lat) || !isFinite(lng)) continue;
      out.push([lng, lat]);
    }
    if (out.length >= 3) {
      var a = out[0];
      var b = out[out.length - 1];
      if (a[0] !== b[0] || a[1] !== b[1]) out.push([a[0], a[1]]);
    }
    return out;
  }

  function buildingHeightM(bldg) {
    var h = Number(bldg.HeightM != null ? bldg.HeightM : bldg.heightM);
    if (isFinite(h) && h > 1) return h;
    var levels = Number(bldg.Levels != null ? bldg.Levels : bldg.levels);
    if (isFinite(levels) && levels > 0) return Math.max(levels * 3, 4);
    return DEFAULT_HEIGHT_M;
  }

  function refreshBuildingPaint() {
    if (!state.map || !state.map.getSource(SOURCE_ID)) return;
    var selId = state.selected
      ? String(state.selected.OsmId != null ? state.selected.OsmId : state.selected.osmId)
      : '';
    var data = { type: 'FeatureCollection', features: [] };
    var keys = Object.keys(state.buildingsById);
    for (var i = 0; i < keys.length; i++) {
      var b = state.buildingsById[keys[i]];
      var coords = ringToLngLat(b.Ring || b.ring);
      if (coords.length < 4) continue;
      var id = String(b.OsmId != null ? b.OsmId : b.osmId);
      data.features.push({
        type: 'Feature',
        id: id,
        properties: {
          osmId: id,
          name: b.TextLabel || b.textLabel || ('OSM ' + id),
          height: buildingHeightM(b),
          selected: selId && selId === id ? 1 : 0,
        },
        geometry: { type: 'Polygon', coordinates: [coords] },
      });
    }
    state.map.getSource(SOURCE_ID).setData(data);
  }

  function ensureBuildingLayers() {
    if (!state.map) return;
    if (!state.map.getSource(SOURCE_ID)) {
      state.map.addSource(SOURCE_ID, {
        type: 'geojson',
        data: { type: 'FeatureCollection', features: [] },
      });
    }
    if (!state.map.getLayer(LAYER_FILL)) {
      state.map.addLayer({
        id: LAYER_FILL,
        type: 'fill-extrusion',
        source: SOURCE_ID,
        paint: {
          'fill-extrusion-color': [
            'case',
            ['==', ['get', 'selected'], 1],
            '#dc3545',
            '#6c9bd1',
          ],
          'fill-extrusion-height': ['get', 'height'],
          'fill-extrusion-base': 0,
          'fill-extrusion-opacity': 0.88,
        },
      });
    }
    if (!state.map.getLayer(LAYER_OUTLINE)) {
      state.map.addLayer({
        id: LAYER_OUTLINE,
        type: 'line',
        source: SOURCE_ID,
        paint: {
          'line-color': [
            'case',
            ['==', ['get', 'selected'], 1],
            '#9b1c2e',
            '#2f5f8f',
          ],
          'line-width': [
            'case',
            ['==', ['get', 'selected'], 1],
            3,
            1.2,
          ],
        },
      });
    }
  }

  function selectBuilding(bldg) {
    state.selected = bldg;
    var tpl = attr(state.acceptBtn, 'data-ma-stl-map-building-selected-tpl');
    var label = bldg.TextLabel || bldg.textLabel || ('OSM ' + (bldg.OsmId || bldg.osmId || ''));
    setSelectedLabel(formatTpl(tpl, label));
    syncAcceptUi();
    refreshBuildingPaint();
  }

  function onMapClick(ev) {
    if (!state.map) return;
    var feats = state.map.queryRenderedFeatures(ev.point, { layers: [LAYER_FILL, LAYER_OUTLINE] });
    if (!feats || !feats.length) return;
    var osmId = String(
      (feats[0].properties && (feats[0].properties.osmId || feats[0].properties.OsmId)) ||
        feats[0].id ||
        ''
    );
    var bldg = state.buildingsById[osmId];
    if (bldg) selectBuilding(bldg);
  }

  function ensureMap() {
    if (state.map) return state.map;
    if (typeof global.maplibregl === 'undefined') return null;
    var mapEl = document.getElementById('ma-stl-map-building-map');
    if (!mapEl) return null;

    state.map = new global.maplibregl.Map({
      container: mapEl,
      style: STYLE_URL,
      center: DEFAULT_CENTER,
      zoom: DEFAULT_ZOOM,
      pitch: DEFAULT_PITCH,
      bearing: DEFAULT_BEARING,
      antialias: true,
      attributionControl: true,
    });
    state.map.addControl(new global.maplibregl.NavigationControl({ visualizePitch: true }), 'top-right');
    state.map.on('load', function () {
      ensureBuildingLayers();
      refreshBuildingPaint();
    });
    state.map.on('click', onMapClick);
    state.map.on('mouseenter', LAYER_FILL, function () {
      state.map.getCanvas().style.cursor = 'pointer';
    });
    state.map.on('mouseleave', LAYER_FILL, function () {
      state.map.getCanvas().style.cursor = '';
    });
    return state.map;
  }

  function invalidateMapSize() {
    if (!state.map) return;
    setTimeout(function () {
      try {
        state.map.resize();
      } catch (e) {
        /* ignore */
      }
    }, 180);
  }

  function clearBuildings() {
    clearSelection();
    state.buildingsById = {};
    refreshBuildingPaint();
  }

  function renderBuildings(list) {
    clearBuildings();
    if (!state.map || !list || !list.length) {
      var noneTpl = attr(state.acceptBtn, 'data-ma-stl-map-building-none-loaded');
      if (noneTpl) toast(noneTpl);
      return;
    }

    var bounds = null;
    for (var i = 0; i < list.length; i++) {
      var bldg = list[i];
      var id = String(bldg.OsmId != null ? bldg.OsmId : bldg.osmId || i);
      state.buildingsById[id] = bldg;
      var ring = bldg.Ring || bldg.ring || [];
      for (var j = 0; j < ring.length; j++) {
        var p = ring[j];
        var lat = Number(p.Lat != null ? p.Lat : p.lat);
        var lng = Number(p.Lng != null ? p.Lng : p.lng);
        if (!isFinite(lat) || !isFinite(lng)) continue;
        if (!bounds) {
          bounds = new global.maplibregl.LngLatBounds([lng, lat], [lng, lat]);
        } else {
          bounds.extend([lng, lat]);
        }
      }
    }

    ensureBuildingLayers();
    refreshBuildingPaint();

    if (bounds) {
      try {
        state.map.fitBounds(bounds, {
          padding: 48,
          maxZoom: 19,
          pitch: DEFAULT_PITCH,
          bearing: state.map.getBearing(),
          duration: 800,
        });
      } catch (e) {
        /* ignore */
      }
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
          var go = function () {
            if (
              isFinite(south) &&
              isFinite(west) &&
              isFinite(north) &&
              isFinite(east) &&
              south < north &&
              west < east
            ) {
              state.map.fitBounds(
                [
                  [west, south],
                  [east, north],
                ],
                {
                  padding: 40,
                  maxZoom: 18.5,
                  pitch: DEFAULT_PITCH,
                  bearing: DEFAULT_BEARING,
                  duration: 900,
                }
              );
            } else {
              state.map.easeTo({
                center: [lng, lat],
                zoom: Math.max(state.map.getZoom(), 18),
                pitch: DEFAULT_PITCH,
                bearing: DEFAULT_BEARING,
                duration: 900,
              });
            }
            setTimeout(loadBuildings, 500);
          };
          if (state.map.isStyleLoaded()) go();
          else state.map.once('load', go);
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
    var sw = b.getSouthWest();
    var ne = b.getNorthEast();
    var body = {
      South: sw.lat,
      West: sw.lng,
      North: ne.lat,
      East: ne.lng,
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
        var apply = function () {
          ensureBuildingLayers();
          renderBuildings(resp.Datos || []);
        };
        if (state.map.isStyleLoaded()) apply();
        else state.map.once('load', apply);
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
      HeightM: buildingHeightM(state.selected),
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
