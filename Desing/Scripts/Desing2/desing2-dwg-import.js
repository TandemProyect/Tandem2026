/* Desing_2 — Importar DWG.
   Botón #ma-stl-tool-dwg-import (barra superior, grupo exportar/importar) abre #ma-stl-dwg-import-modal.
   1) Elegir o arrastrar un .dwg / .dxf (se valida extensión, tamaño y cabecera "AC10xx" del DWG).
   2) Se sube a Desing_2/ImportDwg: el servidor lo pasa a DXF (ODA File Converter), lee unidades,
      capas, líneas y polilíneas (también dentro de bloques) y busca muros (pares de líneas paralelas,
      rectángulos, sombreados, sólidos 2D). La geometría vuelve en unidades del dibujo.
   3) Resumen: unidades (se pueden cambiar → se vuelve a analizar), tamaño en m, capas, muros y vista previa.
   4) Importar: pasa todo a mm y llama a window.maStlDesing2ImportDwgGeometry (un solo paso de deshacer). */
(function () {
    'use strict';

    var DEFAULT_MAX_MB = 45;
    var PREVIEW_HEIGHT = 240;

    function toast(message, isError) {
        if (typeof window.maStlDesing2ShowSaveViewToast === 'function') {
            window.maStlDesing2ShowSaveViewToast(message, !!isError);
            return;
        }
        if (isError) window.alert(message);
    }

    /** Sustituye {0}, {1}… por los argumentos. */
    function fmt(template) {
        var args = Array.prototype.slice.call(arguments, 1);
        return String(template || '').replace(/\{(\d+)\}/g, function (m, i) {
            var v = args[Number(i)];
            return v == null ? m : String(v);
        });
    }

    function formatSize(bytes) {
        if (bytes < 1024) return bytes + ' B';
        if (bytes < 1024 * 1024) return (bytes / 1024).toFixed(1) + ' KB';
        return (bytes / (1024 * 1024)).toFixed(2) + ' MB';
    }

    function formatMeters(v) {
        if (!isFinite(v)) return '—';
        var abs = Math.abs(v);
        return v.toFixed(abs >= 100 ? 1 : 2);
    }

    /** Las cabeceras DWG empiezan por "AC10" (AC1014 R14 … AC1032 2018+). */
    function readDwgSignature(file) {
        return new Promise(function (resolve) {
            if (!file || typeof FileReader === 'undefined' || typeof file.slice !== 'function') {
                resolve(true);
                return;
            }
            var reader = new FileReader();
            reader.onload = function () {
                var bytes = new Uint8Array(reader.result || new ArrayBuffer(0));
                var head = '';
                for (var i = 0; i < bytes.length; i++) head += String.fromCharCode(bytes[i]);
                resolve(head.indexOf('AC') === 0);
            };
            reader.onerror = function () {
                resolve(false);
            };
            reader.readAsArrayBuffer(file.slice(0, 6));
        });
    }

    function wire() {
        var btn = document.getElementById('ma-stl-tool-dwg-import');
        var modalEl = document.getElementById('ma-stl-dwg-import-modal');
        if (!btn || !modalEl) return;
        function q(id) {
            return modalEl.querySelector('#' + id);
        }
        var input = q('ma-stl-dwg-import-file');
        var drop = q('ma-stl-dwg-import-drop');
        var info = q('ma-stl-dwg-import-file-info');
        var errorEl = q('ma-stl-dwg-import-error');
        var submit = q('ma-stl-dwg-import-submit');
        var backBtn = q('ma-stl-dwg-import-back');
        var stepSelect = q('ma-stl-dwg-import-step-select');
        var busyEl = q('ma-stl-dwg-import-busy');
        var busyText = q('ma-stl-dwg-import-busy-text');
        var summaryEl = q('ma-stl-dwg-import-summary');
        var summaryFile = q('ma-stl-dwg-import-summary-file');
        var unitsSel = q('ma-stl-dwg-import-units');
        var unitsNote = q('ma-stl-dwg-import-units-note');
        var extentsEl = q('ma-stl-dwg-import-extents');
        var countsEl = q('ma-stl-dwg-import-counts');
        var ignoredEl = q('ma-stl-dwg-import-ignored');
        var canvas = q('ma-stl-dwg-import-preview');
        var layersEl = q('ma-stl-dwg-import-layers');
        var layersAll = q('ma-stl-dwg-import-layers-all');
        var layersNone = q('ma-stl-dwg-import-layers-none');
        var wallsSummary = q('ma-stl-dwg-import-walls-summary');
        var wallsSolids = q('ma-stl-dwg-import-walls-solids');
        var convertWalls = q('ma-stl-dwg-import-convert-walls');
        var onlyWallLayers = q('ma-stl-dwg-import-only-wall-layers');
        var keepWallLines = q('ma-stl-dwg-import-keep-wall-lines');
        var willEl = q('ma-stl-dwg-import-will');
        var warningsEl = q('ma-stl-dwg-import-warnings');
        if (!input || !drop || !info || !errorEl || !submit || !stepSelect || !busyEl || !summaryEl || !unitsSel) return;

        var texts = {};
        try {
            texts = JSON.parse(modalEl.getAttribute('data-ma-dwg-texts') || '{}') || {};
        } catch (e) {
            texts = {};
        }
        function t(key, fallback) {
            return texts[key] || fallback;
        }
        var analyzeUrl = modalEl.getAttribute('data-ma-dwg-analyze-url') || '';
        var maxMb = parseFloat(modalEl.getAttribute('data-ma-dwg-max-mb'));
        if (!(maxMb > 0)) maxMb = DEFAULT_MAX_MB;
        var noFileText = info.textContent;
        var baseUnitOptions = Array.prototype.slice.call(unitsSel.options);

        var state = {
            file: null,
            result: null,
            toMeters: 1,
            manualUnits: false,
            layerOn: [],
            seq: 0,
            busy: false,
        };

        function getModal() {
            if (typeof window.bootstrap === 'undefined' || !window.bootstrap.Modal) return null;
            return window.bootstrap.Modal.getOrCreateInstance(modalEl);
        }

        function setError(text) {
            errorEl.textContent = text || '';
            errorEl.classList.toggle('d-none', !text);
        }

        function showStep(step, busyMessage) {
            stepSelect.classList.toggle('d-none', step !== 'select');
            busyEl.classList.toggle('d-none', step !== 'busy');
            summaryEl.classList.toggle('d-none', step !== 'summary');
            if (backBtn) backBtn.classList.toggle('d-none', step !== 'summary');
            if (busyText) busyText.textContent = busyMessage || '';
            state.busy = step === 'busy';
            if (step !== 'summary') submit.disabled = true;
        }

        function reset() {
            state.seq++;
            state.file = null;
            state.result = null;
            state.manualUnits = false;
            state.layerOn = [];
            input.value = '';
            info.textContent = noFileText;
            info.classList.add('text-muted');
            drop.classList.remove('is-dragover', 'has-file');
            setError('');
            showStep('select');
        }

        function pickFile(file) {
            state.file = null;
            state.seq++;
            setError('');
            drop.classList.remove('has-file');
            if (!file) {
                info.textContent = noFileText;
                info.classList.add('text-muted');
                return;
            }
            info.textContent = file.name + ' — ' + formatSize(file.size);
            info.classList.remove('text-muted');
            var isDwg = /\.dwg$/i.test(file.name || '');
            if (!isDwg && !/\.dxf$/i.test(file.name || '')) {
                setError(modalEl.getAttribute('data-ma-dwg-invalid-ext') || 'Solo .dwg / .dxf');
                return;
            }
            if (!file.size) {
                setError(modalEl.getAttribute('data-ma-dwg-empty') || 'Archivo vacío');
                return;
            }
            if (file.size > maxMb * 1024 * 1024) {
                setError(fmt(modalEl.getAttribute('data-ma-dwg-too-large') || 'Máx. {0} MB', maxMb));
                return;
            }
            var seq = state.seq;
            (isDwg ? readDwgSignature(file) : Promise.resolve(true)).then(function (ok) {
                if (seq !== state.seq) return;
                if (!ok) {
                    setError(modalEl.getAttribute('data-ma-dwg-invalid-content') || 'No es un DWG válido');
                    return;
                }
                state.file = file;
                state.manualUnits = false;
                state.layerOn = [];
                state.result = null;
                drop.classList.add('has-file');
                analyze(null);
            });
        }

        /** Sube el archivo y analiza. `toMeters` (opcional) fuerza la escala de las unidades. */
        function analyze(toMeters) {
            if (!state.file) return;
            if (!analyzeUrl || typeof window.fetch !== 'function' || typeof window.FormData !== 'function') {
                setError(t('serverFail', 'No se pudo analizar el archivo'));
                return;
            }
            var seq = ++state.seq;
            setError('');
            showStep('busy', busyText ? busyText.getAttribute('data-ma-dwg-analyzing') : '');
            var fd = new window.FormData();
            fd.append('file', state.file, state.file.name);
            if (toMeters > 0) fd.append('toMeters', String(toMeters));
            window
                .fetch(analyzeUrl, {
                    method: 'POST',
                    body: fd,
                    credentials: 'same-origin',
                    headers: { 'X-Requested-With': 'XMLHttpRequest' },
                })
                .then(function (resp) {
                    if (!resp.ok) throw new Error('HTTP ' + resp.status);
                    return resp.json();
                })
                .then(function (res) {
                    if (seq !== state.seq) return;
                    if (!res || !res.ok) {
                        var code = res && res.errorCode;
                        var msg =
                            code === 'oda_missing'
                                ? t('odaMissing', 'Falta ODA File Converter en el servidor')
                                : (res && res.message) || t('serverFail', 'No se pudo analizar el archivo');
                        if (window.console && res) window.console.warn('[Desing2 DWG]', code, res.detail || '');
                        showStep(state.result ? 'summary' : 'select');
                        if (state.result) updateSelection();
                        setError(msg);
                        return;
                    }
                    var previousOn = layerStateByName();
                    state.result = res;
                    renderSummary(previousOn);
                })
                .catch(function (err) {
                    if (seq !== state.seq) return;
                    if (window.console) window.console.warn('[Desing2 DWG]', err);
                    showStep(state.result ? 'summary' : 'select');
                    if (state.result) updateSelection();
                    setError(t('serverFail', 'No se pudo analizar el archivo'));
                });
        }

        /** Casillas de capas del análisis anterior (por nombre); null en el primer análisis. */
        function layerStateByName() {
            if (!state.result) return null;
            var map = {};
            var layers = state.result.layers || [];
            for (var i = 0; i < layers.length; i++) {
                if (state.layerOn[i] != null) map[layers[i].name] = state.layerOn[i];
            }
            return map;
        }

        function unitLabel(code) {
            for (var i = 0; i < baseUnitOptions.length; i++) {
                if (baseUnitOptions[i].getAttribute('data-ma-dwg-unit') === code) return baseUnitOptions[i].textContent;
            }
            return code || '?';
        }

        function renderUnits(units) {
            var toM = units && units.toMeters > 0 ? units.toMeters : 1;
            state.toMeters = toM;
            while (unitsSel.options.length > baseUnitOptions.length) unitsSel.remove(unitsSel.options.length - 1);
            var match = null;
            for (var i = 0; i < baseUnitOptions.length; i++) {
                if (Math.abs(parseFloat(baseUnitOptions[i].value) / toM - 1) < 1e-6) match = baseUnitOptions[i];
            }
            if (!match) {
                match = document.createElement('option');
                match.value = String(toM);
                match.textContent = (units && units.code) || toM + ' m';
                unitsSel.appendChild(match);
            }
            unitsSel.value = match.value;
            var note;
            if (state.manualUnits || (units && units.source === 'override')) {
                note = t('unitsManual', 'Unidades elegidas a mano.');
            } else if (units && units.source === 'insunits-fixed') {
                note = fmt(
                    t('unitsCorrected', 'El archivo indica {0}, pero el tamaño no encaja: se usan {1}. Revísalo.'),
                    unitLabel(units.suggestedCode),
                    unitLabel(units.code)
                );
            } else if (units && units.source === 'guess') {
                note = t('unitsGuessed', 'Unidades estimadas, revísalas.');
            } else if (units && units.suggestedCode && units.suggestedCode !== units.code) {
                note = fmt(t('unitsSuspicious', '¿Quizá {0}?'), unitLabel(units.suggestedCode));
            } else {
                note = t('unitsFromFile', 'Según el archivo.');
            }
            if (unitsNote) {
                unitsNote.textContent = note;
                unitsNote.classList.toggle('text-warning', !state.manualUnits && !!units && units.confidence !== 'high');
                unitsNote.classList.toggle('text-muted', state.manualUnits || !units || units.confidence === 'high');
            }
        }

        function renderSummary(previousOn) {
            var res = state.result;
            var stats = res.stats || {};
            showStep('summary');
            if (summaryFile) summaryFile.textContent = res.fileName || (state.file && state.file.name) || '';
            renderUnits(res.units);
            var ext = res.extents;
            if (extentsEl) {
                extentsEl.textContent = ext
                    ? formatMeters((ext.maxX - ext.minX) * state.toMeters) +
                      ' × ' +
                      formatMeters((ext.maxY - ext.minY) * state.toMeters) +
                      ' m'
                    : '—';
            }
            if (countsEl) {
                countsEl.textContent = fmt(
                    t('counts', '{0} líneas · {1} polilíneas · {2} bloques'),
                    (stats.lines || 0) + (stats.linesInBlocks || 0),
                    (stats.polylines || 0) + (stats.polylinesInBlocks || 0),
                    stats.blocksExploded || 0
                );
            }
            var ignored = res.ignored || {};
            var ignoredParts = Object.keys(ignored).map(function (k) {
                return k.toUpperCase() + ' (' + ignored[k] + ')';
            });
            if (ignoredEl) ignoredEl.textContent = ignoredParts.length ? fmt(t('ignored', 'No se importan: {0}'), ignoredParts.join(', ')) : '';

            var layers = res.layers || [];
            state.layerOn = layers.map(function (l) {
                if (previousOn && Object.prototype.hasOwnProperty.call(previousOn, l.name)) return previousOn[l.name];
                return true;
            });
            renderLayers();

            var walls = res.walls || [];
            var bySource = { pairs: 0, other: 0 };
            walls.forEach(function (w) {
                if (w.source === 'pair-lines') bySource.pairs++;
                else bySource.other++;
            });
            if (wallsSummary) {
                wallsSummary.textContent = walls.length
                    ? fmt(t('wallsSummary', '{0} posibles muros'), walls.length, bySource.pairs, bySource.other)
                    : t('wallsNone', 'No se han encontrado muros.');
            }
            var notConv =
                (stats.solids3d || 0) + (stats.regions || 0) + (stats.aecWalls || 0) + (stats.aecObjects || 0) + (stats.proxies || 0);
            if (wallsSolids) wallsSolids.textContent = notConv ? fmt(t('notConvertible', '{0} sólidos no convertibles'), notConv) : '';

            var wallLayerHasWalls = layers.some(function (l) {
                return l.wallLike && l.walls > 0;
            });
            if (!previousOn) {
                convertWalls.checked = walls.length > 0;
                onlyWallLayers.checked = wallLayerHasWalls;
                keepWallLines.checked = false;
            }
            convertWalls.disabled = !walls.length;
            if (!walls.length) convertWalls.checked = false;
            onlyWallLayers.disabled = !wallLayerHasWalls;
            if (!wallLayerHasWalls) onlyWallLayers.checked = false;

            var warn = (res.warnings || []).map(function (w) {
                if (w === 'segments_truncated') return fmt(t('warnTruncated', 'Dibujo truncado ({0})'), stats.segments || '');
                if (w === 'missing_blocks') return t('warnMissingBlocks', 'Faltan bloques');
                if (w === 'no_geometry') return t('warnNoGeometry', 'Sin líneas visibles');
                return '';
            });
            if (warningsEl) warningsEl.textContent = warn.filter(Boolean).join(' ');
            updateSelection();
        }

        function renderLayers() {
            if (!layersEl) return;
            var layers = (state.result && state.result.layers) || [];
            layersEl.textContent = '';
            layers.forEach(function (l, i) {
                if (!(l.segments > 0) && !(l.walls > 0)) return;
                var row = document.createElement('label');
                row.className = 'desing2-dwg-import__layer form-check';
                var cb = document.createElement('input');
                cb.type = 'checkbox';
                cb.className = 'form-check-input';
                cb.checked = state.layerOn[i] !== false;
                cb.addEventListener('change', function () {
                    state.layerOn[i] = cb.checked;
                    updateSelection();
                });
                var name = document.createElement('span');
                name.className = 'form-check-label text-truncate';
                name.textContent = l.name || '0';
                name.title = l.name || '0';
                var count = document.createElement('span');
                count.className = 'text-muted ms-auto ps-2';
                count.textContent = String(l.segments || 0) + (l.walls ? ' · ' + l.walls + ' ▭' : '');
                row.appendChild(cb);
                row.appendChild(name);
                if (l.wallLike) {
                    var badge = document.createElement('span');
                    badge.className = 'badge bg-label-success ms-1';
                    badge.textContent = t('wallLayerBadge', 'muros');
                    row.appendChild(badge);
                }
                row.appendChild(count);
                layersEl.appendChild(row);
            });
        }

        /** Geometría elegida (unidades del dibujo) según capas y opciones de muros. */
        function computeSelection() {
            var res = state.result;
            var out = { segments: [], walls: [] };
            if (!res) return out;
            var layers = res.layers || [];
            var on = state.layerOn;
            var doWalls = convertWalls.checked && !convertWalls.disabled;
            var onlyWl = doWalls && onlyWallLayers.checked && !onlyWallLayers.disabled;
            if (doWalls) {
                (res.walls || []).forEach(function (w) {
                    if (on[w.layer] === false) return;
                    if (onlyWl && !(layers[w.layer] && layers[w.layer].wallLike)) return;
                    out.walls.push(w);
                });
            }
            var dropWallLines = doWalls && !keepWallLines.checked && out.walls.length > 0;
            (res.segments || []).forEach(function (s) {
                var layer = s[4];
                if (on[layer] === false) return;
                /* flags bit 1: la línea forma parte de un muro detectado. */
                if (dropWallLines && (s[6] & 1) && (!onlyWl || (layers[layer] && layers[layer].wallLike))) return;
                out.segments.push(s);
            });
            return out;
        }

        function updateSelection() {
            var sel = computeSelection();
            if (willEl) willEl.textContent = fmt(t('willImport', '{0} líneas y {1} muros'), sel.segments.length, sel.walls.length);
            submit.disabled = state.busy || (!sel.segments.length && !sel.walls.length);
            drawPreview(sel);
        }

        function drawPreview(sel) {
            if (!canvas || !canvas.getContext || !state.result || !state.result.extents) return;
            var dpr = window.devicePixelRatio || 1;
            var cssW = canvas.clientWidth || 600;
            var cssH = PREVIEW_HEIGHT;
            canvas.width = Math.round(cssW * dpr);
            canvas.height = Math.round(cssH * dpr);
            var ctx = canvas.getContext('2d');
            ctx.setTransform(dpr, 0, 0, dpr, 0, 0);
            ctx.clearRect(0, 0, cssW, cssH);
            var ext = state.result.extents;
            var w = Math.max(ext.maxX - ext.minX, 1e-9);
            var h = Math.max(ext.maxY - ext.minY, 1e-9);
            var pad = 8;
            var k = Math.min((cssW - 2 * pad) / w, (cssH - 2 * pad) / h);
            var ox = (cssW - w * k) / 2;
            var oy = (cssH - h * k) / 2;
            function px(x) {
                return ox + (x - ext.minX) * k;
            }
            function py(y) {
                return cssH - oy - (y - ext.minY) * k;
            }
            /* Todas las líneas (tenue) y encima las que se importarán. */
            ctx.lineWidth = 1;
            ctx.strokeStyle = 'rgba(160,160,160,0.35)';
            ctx.beginPath();
            (state.result.segments || []).forEach(function (s) {
                ctx.moveTo(px(s[0]), py(s[1]));
                ctx.lineTo(px(s[2]), py(s[3]));
            });
            ctx.stroke();
            ctx.strokeStyle = 'rgba(52,58,64,0.9)';
            ctx.beginPath();
            sel.segments.forEach(function (s) {
                ctx.moveTo(px(s[0]), py(s[1]));
                ctx.lineTo(px(s[2]), py(s[3]));
            });
            ctx.stroke();
            ctx.strokeStyle = 'rgba(25,170,60,0.85)';
            ctx.lineCap = 'butt';
            sel.walls.forEach(function (wl) {
                ctx.lineWidth = Math.max(1.5, wl.thickness * k);
                ctx.beginPath();
                ctx.moveTo(px(wl.x1), py(wl.y1));
                ctx.lineTo(px(wl.x2), py(wl.y2));
                ctx.stroke();
            });
        }

        function doImport() {
            if (state.busy || !state.result) return;
            var sel = computeSelection();
            if (!sel.segments.length && !sel.walls.length) {
                setError(t('nothing', 'No hay nada que importar'));
                return;
            }
            if (typeof window.maStlDesing2ImportDwgGeometry !== 'function') {
                setError(t('viewerMissing', 'El visor aún no está listo'));
                return;
            }
            var ext = state.result.extents || { minX: 0, minY: 0 };
            var f = state.toMeters * 1000;
            var payload = {
                segments: sel.segments.map(function (s) {
                    return {
                        x1: (s[0] - ext.minX) * f,
                        y1: (s[1] - ext.minY) * f,
                        x2: (s[2] - ext.minX) * f,
                        y2: (s[3] - ext.minY) * f,
                        group: s[5],
                    };
                }),
                walls: sel.walls.map(function (w) {
                    return {
                        x1: (w.x1 - ext.minX) * f,
                        y1: (w.y1 - ext.minY) * f,
                        x2: (w.x2 - ext.minX) * f,
                        y2: (w.y2 - ext.minY) * f,
                        thicknessMm: w.thickness * f,
                    };
                }),
            };
            setError('');
            showStep('busy', busyText ? busyText.getAttribute('data-ma-dwg-importing') : '');
            /* Dejar que se pinte el spinner antes del trabajo pesado. */
            window.setTimeout(function () {
                var res = null;
                try {
                    res = window.maStlDesing2ImportDwgGeometry(payload);
                } catch (err) {
                    if (window.console) window.console.error('[Desing2 DWG] import', err);
                }
                if (!res || !res.ok) {
                    showStep('summary');
                    updateSelection();
                    setError(modalEl.getAttribute('data-ma-dwg-fail') || 'No se pudo importar el DWG');
                    return;
                }
                toast(fmt(t('done', 'DWG importado: {0} líneas y {1} muros'), res.lines, res.walls), false);
                var modal = getModal();
                if (modal) modal.hide();
            }, 40);
        }

        btn.addEventListener('click', function (ev) {
            ev.preventDefault();
            ev.stopPropagation();
            var modal = getModal();
            if (!modal) return;
            reset();
            modal.show();
        });

        input.addEventListener('change', function () {
            pickFile(input.files && input.files[0]);
        });

        drop.addEventListener('click', function () {
            input.click();
        });
        drop.addEventListener('keydown', function (ev) {
            if (ev.key === 'Enter' || ev.key === ' ') {
                ev.preventDefault();
                input.click();
            }
        });
        ['dragenter', 'dragover'].forEach(function (type) {
            drop.addEventListener(type, function (ev) {
                ev.preventDefault();
                if (ev.dataTransfer) ev.dataTransfer.dropEffect = 'copy';
                drop.classList.add('is-dragover');
            });
        });
        drop.addEventListener('dragleave', function () {
            drop.classList.remove('is-dragover');
        });
        drop.addEventListener('drop', function (ev) {
            ev.preventDefault();
            drop.classList.remove('is-dragover');
            var files = ev.dataTransfer && ev.dataTransfer.files;
            pickFile(files && files[0]);
        });
        /* Soltar fuera de la zona dentro del modal: que el navegador no abra el archivo. */
        modalEl.addEventListener('dragover', function (ev) {
            ev.preventDefault();
        });
        modalEl.addEventListener('drop', function (ev) {
            ev.preventDefault();
        });

        unitsSel.addEventListener('change', function () {
            var v = parseFloat(unitsSel.value);
            if (!(v > 0) || !state.file) return;
            state.manualUnits = true;
            analyze(v);
        });
        [convertWalls, onlyWallLayers, keepWallLines].forEach(function (el) {
            if (el) el.addEventListener('change', updateSelection);
        });
        function setAllLayers(on) {
            state.layerOn = state.layerOn.map(function () {
                return on;
            });
            renderLayers();
            updateSelection();
        }
        if (layersAll) {
            layersAll.addEventListener('click', function () {
                setAllLayers(true);
            });
        }
        if (layersNone) {
            layersNone.addEventListener('click', function () {
                setAllLayers(false);
            });
        }
        if (backBtn) {
            backBtn.addEventListener('click', function () {
                reset();
            });
        }

        /* Con el modal abierto, los atajos del visor (Supr, Enter, Ctrl+Z, letras…) no deben saltar.
           Bootstrap escucha Escape en el propio modal, así que cerrar con Esc sigue funcionando. */
        modalEl.addEventListener('keydown', function (ev) {
            if (ev.key === 'Escape') {
                var modal = getModal();
                if (modal) modal.hide();
            }
            if (ev.key !== 'Tab') ev.stopPropagation();
        });

        submit.addEventListener('click', doImport);

        /* El canvas mide 0 hasta que el modal se ve: repintar al terminar de abrir. */
        modalEl.addEventListener('shown.bs.modal', function () {
            if (state.result) updateSelection();
        });
        modalEl.addEventListener('hidden.bs.modal', function () {
            reset();
            if (document.body.contains(btn)) btn.focus();
        });
    }

    if (document.readyState === 'loading') {
        document.addEventListener('DOMContentLoaded', wire);
    } else {
        wire();
    }
})();
