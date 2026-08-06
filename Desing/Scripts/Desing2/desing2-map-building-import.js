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
  var LAYER_HIT = 'desing2-osm-buildings-hit';
  var LAYER_FILL = 'desing2-osm-buildings-extrusion';
  var LAYER_OUTLINE = 'desing2-osm-buildings-outline';
  var STYLE_URL = 'https://tiles.openfreemap.org/styles/liberty';

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
        promoteId: 'osmId',
      });
    }
    // Capa plana bajo la extrusión: ayuda al hit-test en planta (opacidad baja pero queryable).
    if (!state.map.getLayer(LAYER_HIT)) {
      state.map.addLayer({
        id: LAYER_HIT,
        type: 'fill',
        source: SOURCE_ID,
        paint: {
          'fill-color': [
            'case',
            ['==', ['get', 'selected'], 1],
            '#dc3545',
            '#1f6feb',
          ],
          'fill-opacity': 0.12,
        },
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
            '#1f6feb',
          ],
          'fill-extrusion-height': ['get', 'height'],
          'fill-extrusion-base': 0,
          'fill-extrusion-opacity': 0.9,
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
            '#0b3d91',
          ],
          'line-width': [
            'case',
            ['==', ['get', 'selected'], 1],
            3,
            1.5,
          ],
        },
      });
    }
  }

  function buildingDisplayName(bldg) {
    if (!bldg) return '';
    return (
      bldg.CadastralAddress ||
      bldg.cadastralAddress ||
      bldg.TextLabel ||
      bldg.textLabel ||
      ('OSM ' + (bldg.OsmId != null ? bldg.OsmId : bldg.osmId || ''))
    );
  }

  function buildingCadastralRef(bldg) {
    if (!bldg) return '';
    return String(bldg.CadastralRef || bldg.cadastralRef || '').trim();
  }

  function updateSelectedLabel(bldg) {
    var name = buildingDisplayName(bldg);
    var rc = buildingCadastralRef(bldg);
    var catTpl = attr(state.acceptBtn, 'data-ma-stl-map-building-selected-catastro-tpl');
    var tpl = attr(state.acceptBtn, 'data-ma-stl-map-building-selected-tpl');
    if (rc && catTpl) {
      setSelectedLabel(
        String(catTpl)
          .replace(/\{0\}/g, name || rc)
          .replace(/\{1\}/g, rc)
      );
      return;
    }
    setSelectedLabel(formatTpl(tpl, name || rc || '—'));
  }

  function enrichSelectedWithCatastro(bldg) {
    if (!bldg || !state.urls.catastro) return;
    var c = ringCentroid(bldg.Ring || bldg.ring);
    if (!c) return;
    var url =
      state.urls.catastro +
      (state.urls.catastro.indexOf('?') >= 0 ? '&' : '?') +
      'lat=' +
      encodeURIComponent(String(c.lat)) +
      '&lng=' +
      encodeURIComponent(String(c.lng));
    fetch(url, { method: 'GET', credentials: 'same-origin' })
      .then(function (res) {
        if (!res.ok) throw new Error('HTTP ' + res.status);
        return res.json();
      })
      .then(function (resp) {
        if (!resp || !resp.Exito || !resp.Datos) return;
        if (state.selected !== bldg) return;
        var rc = resp.Datos.CadastralRef || resp.Datos.cadastralRef || '';
        var addr = resp.Datos.Address || resp.Datos.address || '';
        if (rc) {
          bldg.CadastralRef = rc;
          bldg.cadastralRef = rc;
        }
        if (addr) {
          bldg.CadastralAddress = addr;
          bldg.cadastralAddress = addr;
          bldg.TextLabel = addr;
        }
        updateSelectedLabel(bldg);
        toast(
          (addr ? addr + ' · ' : '') +
            (rc ? 'RC ' + rc : buildingDisplayName(bldg))
        );
      })
      .catch(function () {
        /* Catastro opcional: no bloquear selección */
      });
  }

  function selectBuilding(bldg) {
    state.selected = bldg;
    updateSelectedLabel(bldg);
    syncAcceptUi();
    refreshBuildingPaint();
    // Si ya viene RC de Catastro WFS, mostrar; si no, consultar por centroide.
    if (buildingCadastralRef(bldg)) {
      toast(
        (buildingDisplayName(bldg) ? buildingDisplayName(bldg) + ' · ' : '') +
          'RC ' +
          buildingCadastralRef(bldg)
      );
      if (!bldg.CadastralAddress && !bldg.cadastralAddress) {
        enrichSelectedWithCatastro(bldg);
      }
    } else {
      enrichSelectedWithCatastro(bldg);
    }
  }

  /** Ray-cast 2D: fiable con pitch/extrusión (queryRenderedFeatures suele fallar en fill-extrusion). */
  function pointInRingLngLat(lng, lat, ring) {
    var coords = ringToLngLat(ring);
    if (coords.length < 4) return false;
    var inside = false;
    for (var i = 0, j = coords.length - 1; i < coords.length; j = i++) {
      var xi = coords[i][0];
      var yi = coords[i][1];
      var xj = coords[j][0];
      var yj = coords[j][1];
      var denom = yj - yi;
      if (denom === 0) continue;
      var intersect =
        yi > lat !== yj > lat && lng < ((xj - xi) * (lat - yi)) / denom + xi;
      if (intersect) inside = !inside;
    }
    return inside;
  }

  function approxRingAreaAbs(ring) {
    var coords = ringToLngLat(ring);
    if (coords.length < 4) return Number.POSITIVE_INFINITY;
    var sum = 0;
    for (var i = 0, j = coords.length - 1; i < coords.length; j = i++) {
      sum += coords[j][0] * coords[i][1] - coords[i][0] * coords[j][1];
    }
    return Math.abs(sum) * 0.5;
  }

  function findBuildingAtLngLat(lng, lat) {
    var keys = Object.keys(state.buildingsById);
    var best = null;
    var bestArea = Number.POSITIVE_INFINITY;
    for (var i = 0; i < keys.length; i++) {
      var b = state.buildingsById[keys[i]];
      var ring = b.Ring || b.ring;
      if (!pointInRingLngLat(lng, lat, ring)) continue;
      var area = approxRingAreaAbs(ring);
      if (area < bestArea) {
        bestArea = area;
        best = b;
      }
    }
    return best;
  }

  function haversineM(lng1, lat1, lng2, lat2) {
    var R = 6371000;
    var toRad = Math.PI / 180;
    var dLat = (lat2 - lat1) * toRad;
    var dLng = (lng2 - lng1) * toRad;
    var a =
      Math.sin(dLat / 2) * Math.sin(dLat / 2) +
      Math.cos(lat1 * toRad) *
        Math.cos(lat2 * toRad) *
        Math.sin(dLng / 2) *
        Math.sin(dLng / 2);
    return 2 * R * Math.asin(Math.min(1, Math.sqrt(a)));
  }

  function ringCentroid(ring) {
    var coords = ringToLngLat(ring);
    if (!coords.length) return null;
    var sx = 0;
    var sy = 0;
    var n = 0;
    for (var i = 0; i < coords.length - 1; i++) {
      sx += coords[i][0];
      sy += coords[i][1];
      n++;
    }
    if (!n) return null;
    return { lng: sx / n, lat: sy / n };
  }

  /** Con pitch alto el clic en fachada cae fuera de la huella: coger el más cercano. */
  function findNearestBuilding(lng, lat, maxM) {
    var keys = Object.keys(state.buildingsById);
    var best = null;
    var bestD = maxM;
    for (var i = 0; i < keys.length; i++) {
      var b = state.buildingsById[keys[i]];
      var ring = b.Ring || b.ring;
      if (pointInRingLngLat(lng, lat, ring)) return b;
      var c = ringCentroid(ring);
      if (!c) continue;
      var d = haversineM(lng, lat, c.lng, c.lat);
      if (d < bestD) {
        bestD = d;
        best = b;
      }
    }
    return best;
  }

  function buildingFromRenderedFeature(feat) {
    if (!feat) return null;
    var osmId = String(
      (feat.properties && (feat.properties.osmId || feat.properties.OsmId)) ||
        feat.id ||
        ''
    );
    if (!osmId) return null;
    return state.buildingsById[osmId] || null;
  }

  function ownLayerIds() {
    return [LAYER_FILL, LAYER_HIT, LAYER_OUTLINE].filter(function (id) {
      return !!state.map.getLayer(id);
    });
  }

  function queryBuildingAtPoint(point) {
    if (!state.map || !point) return null;
    var layers = ownLayerIds();
    if (!layers.length) return null;
    var pad = 14;
    var boxes = [
      point,
      [
        [point.x - pad, point.y - pad],
        [point.x + pad, point.y + pad],
      ],
    ];
    for (var b = 0; b < boxes.length; b++) {
      try {
        var feats = state.map.queryRenderedFeatures(boxes[b], { layers: layers });
        for (var i = 0; i < (feats || []).length; i++) {
          var hit = buildingFromRenderedFeature(feats[i]);
          if (hit) return hit;
        }
      } catch (e) {
        /* ignore */
      }
    }
    return null;
  }

  function onMapClick(ev) {
    if (!state.map || state.busy) return;
    // 1) Hit visual (fachada/tejado extruido) — prioritario con pitch.
    var bldg = queryBuildingAtPoint(ev.point);

    // 2) Huella en planta bajo el cursor.
    var lng = ev.lngLat && ev.lngLat.lng;
    var lat = ev.lngLat && ev.lngLat.lat;
    if (!bldg && isFinite(lng) && isFinite(lat)) {
      bldg = findBuildingAtLngLat(lng, lat);
    }

    // 3) Clic en fachada: el lngLat cae fuera → edificio más cercano (~40 m).
    if (!bldg && isFinite(lng) && isFinite(lat)) {
      bldg = findNearestBuilding(lng, lat, 40);
    }

    if (bldg) {
      selectBuilding(bldg);
      return;
    }

    var keys = Object.keys(state.buildingsById);
    if (!keys.length) {
      var noneTpl = attr(state.acceptBtn, 'data-ma-stl-map-building-none-loaded');
      if (noneTpl) toast(noneTpl);
      return;
    }
    var noSel = attr(state.acceptBtn, 'data-ma-stl-map-building-no-selection');
    if (noSel) toast(noSel);
  }

  function onMapMouseMove(ev) {
    if (!state.map || !ev) return;
    var keys = Object.keys(state.buildingsById);
    if (!keys.length) {
      state.map.getCanvas().style.cursor = '';
      return;
    }
    var hit =
      queryBuildingAtPoint(ev.point) ||
      (ev.lngLat
        ? findBuildingAtLngLat(ev.lngLat.lng, ev.lngLat.lat) ||
          findNearestBuilding(ev.lngLat.lng, ev.lngLat.lat, 20)
        : null);
    state.map.getCanvas().style.cursor = hit ? 'pointer' : '';
  }

  function setMapBusyOverlay(on, message) {
    var el = document.getElementById('ma-stl-map-building-loading');
    var textEl = document.getElementById('ma-stl-map-building-loading-text');
    var searchBtn = document.getElementById('ma-stl-map-building-search-btn');
    var loadBtn = document.getElementById('ma-stl-map-building-load-btn');
    if (textEl && message) textEl.textContent = message;
    if (el) {
      el.classList.toggle('d-none', !on);
      if (on) el.removeAttribute('hidden');
      else el.setAttribute('hidden', 'hidden');
      el.setAttribute('aria-busy', on ? 'true' : 'false');
    }
    if (searchBtn) searchBtn.disabled = !!on;
    if (loadBtn) loadBtn.disabled = !!on;
  }

  /** Muestra/oculta extrusiones del estilo base (edificios grises del mapa). */
  function setBasemapBuildingsVisible(visible) {
    if (!state.map || !state.map.getStyle) return;
    var style = state.map.getStyle();
    var layers = (style && style.layers) || [];
    for (var i = 0; i < layers.length; i++) {
      var layer = layers[i];
      if (!layer || !layer.id) continue;
      if (String(layer.id).indexOf('desing2-') === 0) continue;
      var idLower = String(layer.id).toLowerCase();
      var isBuilding =
        idLower.indexOf('building') >= 0 ||
        layer.type === 'fill-extrusion';
      if (!isBuilding) continue;
      try {
        state.map.setLayoutProperty(layer.id, 'visibility', visible ? 'visible' : 'none');
      } catch (e) {
        /* ignore */
      }
    }
  }

  /** Capas propias (huellas seleccionables Catastro/OSM). */
  function setOwnBuildingLayersVisible(visible) {
    if (!state.map) return;
    var ids = [LAYER_HIT, LAYER_FILL, LAYER_OUTLINE];
    for (var i = 0; i < ids.length; i++) {
      if (!state.map.getLayer(ids[i])) continue;
      try {
        state.map.setLayoutProperty(ids[i], 'visibility', visible ? 'visible' : 'none');
      } catch (e) {
        /* ignore */
      }
    }
  }

  /** Vuelve al modo inicial: solo mapa base 3D, sin huellas seleccionables. */
  function resetToBasemapBuildingsView() {
    clearBuildings();
    setOwnBuildingLayersVisible(false);
    setBasemapBuildingsVisible(true);
    setSelectedLabel('');
  }

  /** Clic izquierdo = seleccionar; arrastrar con botón central (rueda) = desplazar; rueda = zoom. */
  function bindCadStyleNavigation() {
    if (!state.map || state.map.__desing2CadNavBound) return;
    state.map.__desing2CadNavBound = true;
    try {
      state.map.dragPan.disable();
    } catch (e) {
      /* ignore */
    }

    var canvas = state.map.getCanvas();
    var dragging = false;
    var lastX = 0;
    var lastY = 0;

    canvas.addEventListener('mousedown', function (e) {
      if (e.button !== 1) return;
      e.preventDefault();
      dragging = true;
      lastX = e.clientX;
      lastY = e.clientY;
      canvas.style.cursor = 'grabbing';
    });

    window.addEventListener('mousemove', function (e) {
      if (!dragging || !state.map) return;
      var dx = e.clientX - lastX;
      var dy = e.clientY - lastY;
      lastX = e.clientX;
      lastY = e.clientY;
      try {
        state.map.panBy([-dx, -dy], { animate: false });
      } catch (err) {
        /* ignore */
      }
    });

    window.addEventListener('mouseup', function (e) {
      if (e.button !== 1) return;
      dragging = false;
      if (canvas && canvas.style) canvas.style.cursor = '';
    });

    canvas.addEventListener('auxclick', function (e) {
      if (e.button === 1) e.preventDefault();
    });
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
    bindCadStyleNavigation();
    state.map.on('load', function () {
      ensureBuildingLayers();
      refreshBuildingPaint();
      bindCadStyleNavigation();
    });
    state.map.on('click', onMapClick);
    state.map.on('mousemove', onMapMouseMove);
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
      resetToBasemapBuildingsView();
      var noneTpl = attr(state.acceptBtn, 'data-ma-stl-map-building-none-loaded');
      if (noneTpl) toast(noneTpl);
      return;
    }

    var bounds = null;
    var accepted = 0;
    for (var i = 0; i < list.length; i++) {
      var bldg = list[i];
      var id = String(bldg.OsmId != null ? bldg.OsmId : bldg.osmId || i);
      var ring = bldg.Ring || bldg.ring || [];
      var coords = ringToLngLat(ring);
      if (coords.length < 4) continue;
      state.buildingsById[id] = bldg;
      accepted++;
      for (var j = 0; j < coords.length; j++) {
        var lng = coords[j][0];
        var lat = coords[j][1];
        if (!bounds) {
          bounds = new global.maplibregl.LngLatBounds([lng, lat], [lng, lat]);
        } else {
          bounds.extend([lng, lat]);
        }
      }
    }

    if (!accepted) {
      resetToBasemapBuildingsView();
      var noneTpl2 = attr(state.acceptBtn, 'data-ma-stl-map-building-none-loaded');
      if (noneTpl2) toast(noneTpl2);
      return;
    }

    ensureBuildingLayers();
    refreshBuildingPaint();
    setOwnBuildingLayersVisible(true);
    // Huellas azules seleccionables: ocultar grises del estilo base.
    setBasemapBuildingsVisible(false);

    var loadedTpl = attr(state.acceptBtn, 'data-ma-stl-map-building-loaded-tpl');
    if (loadedTpl) toast(formatTpl(loadedTpl, accepted));

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
      catastro: shell ? shell.getAttribute('data-ma-stl-map-building-catastro-url') || '' : '',
    };
  }

  function searchAddress() {
    var errTpl = attr(state.acceptBtn, 'data-ma-stl-map-building-error');
    if (state.busy) {
      toast(formatTpl(errTpl, 'Espere a que termine la operación en curso…'));
      return;
    }
    if (!state.urls.search) return;
    var q = state.searchInput ? String(state.searchInput.value || '').trim() : '';
    if (!q) return;
    var searchingTpl = attr(state.acceptBtn, 'data-ma-stl-map-building-searching');
    setMapBusyOverlay(true, searchingTpl || 'Buscando dirección…');
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
        setMapBusyOverlay(false);
        if (!resp || !resp.Exito || !resp.Datos || !resp.Datos.length) {
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
            // "Buscar" solo centra el mapa y restaura edificios 3D del estilo base.
            resetToBasemapBuildingsView();
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
          };
          if (state.map.isStyleLoaded()) go();
          else state.map.once('load', go);
        }
      })
      .catch(function (err) {
        state.busy = false;
        syncAcceptUi();
        setMapBusyOverlay(false);
        toast(formatTpl(errTpl, err && err.message ? err.message : err));
      });
  }

  function loadBuildings() {
    var errTpl = attr(state.acceptBtn, 'data-ma-stl-map-building-error');
    if (state.busy) {
      toast(formatTpl(errTpl, 'Espere a que termine la operación en curso…'));
      return;
    }
    if (!state.urls.buildings) {
      var shell = document.getElementById('ma-stl-viewer-shell');
      state.urls = getUrlsFromShell(shell);
    }
    if (!state.urls.buildings) {
      toast(formatTpl(errTpl, 'URL de edificios no configurada'));
      return;
    }
    ensureMap();
    if (!state.map) {
      toast(formatTpl(errTpl, 'Mapa no disponible'));
      return;
    }

    var run = function () {
      if (state.busy) return;
      var b = state.map.getBounds();
      if (!b) {
        toast(formatTpl(errTpl, 'No se pudo leer el área del mapa'));
        return;
      }
      var loadingTpl = attr(state.acceptBtn, 'data-ma-stl-map-building-loading');
      setMapBusyOverlay(true, loadingTpl || 'Cargando edificios…');
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
          setMapBusyOverlay(false);
          if (!resp || !resp.Exito) {
            toast(formatTpl(errTpl, (resp && resp.Mensaje) || 'Error'));
            return;
          }
          ensureBuildingLayers();
          renderBuildings(resp.Datos || []);
        })
        .catch(function (err) {
          state.busy = false;
          syncAcceptUi();
          setMapBusyOverlay(false);
          toast(formatTpl(errTpl, err && err.message ? err.message : err));
        });
    };

    if (state.map.isStyleLoaded()) run();
    else state.map.once('load', run);
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
    setMapBusyOverlay(false);
    clearSelection();
    showModal();
    ensureMap();
    if (state.map && state.map.isStyleLoaded()) {
      resetToBasemapBuildingsView();
    } else if (state.map) {
      state.map.once('load', resetToBasemapBuildingsView);
    }
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
