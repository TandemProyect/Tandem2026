// @ts-nocheck
(function () {
    'use strict';

    function wireHoverLockPanel(rootSel, collapseId, hoverZoneCls) {
        const root = document.querySelector(rootSel);
        const btn = document.getElementById(collapseId);
        const zone = root ? root.querySelector(hoverZoneCls) : null;
        if (!root || !btn || !zone) return;
        var leftSinceLock = false;
        btn.addEventListener('click', (clickEv) => {
            clickEv.preventDefault();
            root.classList.add('is-hover-locked');
            leftSinceLock = false;
            try {
                btn.blur();
            } catch (_ex) {
                /* ignore */
            }
        });
        zone.addEventListener('mouseleave', () => {
            if (root.classList.contains('is-hover-locked')) leftSinceLock = true;
        });
        zone.addEventListener('mouseenter', () => {
            if (root.classList.contains('is-hover-locked') && leftSinceLock) {
                root.classList.remove('is-hover-locked');
                leftSinceLock = false;
            }
        });
    }

    function wireToolbarPin(panelSel, btnId, iconCls) {
        const panel = document.querySelector(panelSel);
        const btn = document.getElementById(btnId);
        if (!panel || !btn) return;
        const icon = panel.querySelector(iconCls);
        if (!icon) return;
        const pinTitle = btn.getAttribute('data-ma-pin-title') || '';
        const unpinTitle = btn.getAttribute('data-ma-unpin-title') || '';
        function syncPinUi() {
            const pinned = panel.classList.contains('is-pinned');
            btn.setAttribute('aria-pressed', pinned ? 'true' : 'false');
            btn.title = pinned ? unpinTitle : pinTitle;
            btn.setAttribute('aria-label', pinned ? unpinTitle : pinTitle);
            icon.classList.toggle('ri-pushpin-line', !pinned);
            icon.classList.toggle('ri-pushpin-fill', pinned);
        }
        btn.addEventListener('click', (clickEv) => {
            clickEv.preventDefault();
            panel.classList.toggle('is-pinned');
            syncPinUi();
            try {
                btn.blur();
            } catch (_ex) {
                /* ignore */
            }
        });
        syncPinUi();
    }

    function wireInitialStlBoot() {
        var booted = false;
        function tryBoot() {
            if (booted) return;
            var bootBtn = document.getElementById('desing2-initial-stl-boot');
            if (!bootBtn) return;
            var stlUrl = bootBtn.getAttribute('data-stl-url');
            if (!stlUrl) return;
            booted = true;
            bootBtn.click();
        }
        document.addEventListener('ma-stl-desing2-viewer-ready', tryBoot, { once: true });
        setTimeout(tryBoot, 400);
    }

    function wireRulersFlyout() {
        const flyout = document.getElementById('desing2-stl-rulers-flyout');
        const toggleBtn = document.getElementById('desing2-stl-rulers-flyout-toggle');
        const panel = document.getElementById('desing2-stl-rulers-flyout-panel');
        if (!flyout || !toggleBtn || !panel) return;

        const childToggleIds = [
            'ma-stl-grid-toggle',
            'ma-stl-ucs-rulers-toggle',
            'ma-stl-edge-rulers-toggle',
            'ma-stl-ruler-anchor-pick-toggle',
            'ma-stl-ruler-anchor-object-pick-toggle'
        ];

        function childButtons() {
            return childToggleIds
                .map(function (id) {
                    return document.getElementById(id);
                })
                .filter(Boolean);
        }

        function syncParentActiveState() {
            const anyActive = childButtons().some(function (btn) {
                return btn.classList.contains('active') || btn.getAttribute('aria-pressed') === 'true';
            });
            toggleBtn.classList.toggle('has-active-children', anyActive);
        }

        function setOpen(open) {
            const isOpen = !!open;
            toggleBtn.classList.toggle('is-open', isOpen);
            toggleBtn.setAttribute('aria-expanded', isOpen ? 'true' : 'false');
            if (isOpen) {
                panel.removeAttribute('hidden');
            } else {
                panel.setAttribute('hidden', 'hidden');
            }
        }

        toggleBtn.addEventListener('click', function (clickEv) {
            clickEv.preventDefault();
            clickEv.stopPropagation();
            setOpen(!toggleBtn.classList.contains('is-open'));
            try {
                toggleBtn.blur();
            } catch (_ex) {
                /* ignore */
            }
        });

        document.addEventListener('click', function (docEv) {
            if (!toggleBtn.classList.contains('is-open')) return;
            if (flyout.contains(docEv.target)) return;
            setOpen(false);
        });

        document.addEventListener('keydown', function (keyEv) {
            if (keyEv.key === 'Escape' && toggleBtn.classList.contains('is-open')) {
                setOpen(false);
            }
        });

        childButtons().forEach(function (btn) {
            btn.addEventListener('click', function () {
                window.setTimeout(syncParentActiveState, 0);
            });
        });

        childButtons().forEach(function (btn) {
            if (typeof MutationObserver === 'undefined') return;
            const observer = new MutationObserver(syncParentActiveState);
            observer.observe(btn, {
                attributes: true,
                attributeFilter: ['class', 'aria-pressed']
            });
        });

        syncParentActiveState();
    }

    function wireModeSystemDropdown() {
        const toggle = document.getElementById('ma-stl-mode-system-dropdown');
        if (!toggle) return;
        const menu = document.querySelector('.desing2-stl-mode-toolbar__systems-menu');
        if (!menu) return;

        const items = Array.prototype.slice.call(menu.querySelectorAll('.dropdown-item'));
        if (!items.length) return;

        function setActiveItem(item) {
            if (!item || item.classList.contains('disabled') || item.disabled) return;
            items.forEach(function (it) {
                it.classList.remove('active');
                it.removeAttribute('aria-current');
            });
            item.classList.add('active');
            item.setAttribute('aria-current', 'true');
            const label = (item.getAttribute('data-ma-system') || item.textContent || '').trim();
            if (label) {
                toggle.textContent = label;
            }
        }

        items.forEach(function (item) {
            item.addEventListener('click', function (ev) {
                if (item.classList.contains('disabled') || item.disabled) {
                    ev.preventDefault();
                    return;
                }
                setActiveItem(item);
            });
        });
    }

    function wireFormworkButton() {
        const btn = document.getElementById('ma-stl-mode-formwork');
        if (!btn) return;

        const systemToggle = document.getElementById('ma-stl-mode-system-dropdown');

        btn.addEventListener('click', function () {
            const selectedSystem = ((systemToggle && systemToggle.textContent) || '').trim();

            if (selectedSystem !== 'Atk-60') {
                console.warn('Sistema no soportado por ahora:', selectedSystem);
                return;
            }

            const atk60Url = btn.getAttribute('data-ma-formwork-url-atk60');
            if (!atk60Url) {
                console.error('No existe URL configurada para Encofrar ATK-60.');
                return;
            }

            function normalizeWall(raw) {
                if (!raw) return null;
                const lineId = raw.LineId != null ? raw.LineId : raw.id;
                const wallGroupId = raw.WallGroupId != null ? raw.WallGroupId : raw.wallGroupId;
                const wallId = lineId != null
                    ? lineId
                    : (raw.Id != null ? raw.Id : (raw.wallId != null ? raw.wallId : wallGroupId));
                const attrs = raw.Attributes && typeof raw.Attributes === 'object'
                    ? raw.Attributes
                    : Object.assign({}, raw);

                return Object.assign({}, raw, {
                    Id: wallId,
                    LineId: lineId,
                    WallGroupId: wallGroupId != null ? wallGroupId : null,
                    Attributes: attrs,
                });
            }

            function mergeWallsUnique(sources) {
                const out = [];
                const seen = Object.create(null);
                for (let si = 0; si < sources.length; si++) {
                    const src = sources[si];
                    if (!Array.isArray(src)) continue;
                    for (let i = 0; i < src.length; i++) {
                        const w = normalizeWall(src[i]);
                        if (!w) continue;
                        const key = String(w.LineId != null ? w.LineId : w.Id);
                        if (!key || seen[key]) continue;
                        seen[key] = true;
                        out.push(w);
                    }
                }
                return out;
            }

            const getWallsFromScene = window.maStlDesing2GetStraightWallsFromScene;
            const wallsFromScene = typeof getWallsFromScene === 'function' ? getWallsFromScene() : [];

            const getWallsForFormwork = window.maStlDesing2GetStraightWallsForFormwork;
            const wallsFromFormwork = typeof getWallsForFormwork === 'function' ? getWallsForFormwork() : [];

            const getWallsFromWallModelSource = window.maStlDesing2GetStraightWallsFromWallModelSource;
            const wallsFromWallModelSource = typeof getWallsFromWallModelSource === 'function'
                ? getWallsFromWallModelSource()
                : [];

            const buildConnections = window.maStlDesing2BuildWallConnectionsPayload;
            const builtConnections = typeof buildConnections === 'function' ? buildConnections() : null;
            const wallConnections = builtConnections && builtConnections.payload ? builtConnections.payload : null;

            // Fuente estricta: solo sólidos 3D dibujados.
            // Si no hay sólidos, no enviamos líneas 2D al backend.
            let walls = mergeWallsUnique([
                wallsFromWallModelSource,
            ]);

            if (!Array.isArray(walls)) walls = [];
            if (!walls.length) {
                // Fallback 3D seguro: ejes de muro en escena (misma fuente del inspector de atributos).
                walls = mergeWallsUnique([wallsFromScene]);
                if (!walls.length) {
                    window.alert('ATK60: no hay muros 3D disponibles (sólidos ni ejes).');
                    console.error('ATK60 abortado: sin muros 3D.');
                    return;
                }
                console.warn('ATK60 fallback: usando ejes 3D de escena por wallModelSource vacío.');
            }

            // Flujo legacy (ayer): inserta panel GLB + marcas.
            // Se deja comentado para referencia, pero desactivado en esta fase
            // porque debemos pintar unicamente el punto inicial desde C#.
            // function insertAtk60SampleOnWalls(rawWalls) {
            //     const insertFn = window.maStlDesing2InsertAtk60SampleOnWalls;
            //     if (typeof insertFn !== 'function') {
            //         return Promise.reject(new Error('No existe API maStlDesing2InsertAtk60SampleOnWalls.'));
            //     }
            //     return insertFn(rawWalls, { clearPrevious: true });
            // }
            //
            // insertAtk60SampleOnWalls(walls)
            //     .then(function (result) {
            //         const inserted = result && result.inserted != null ? result.inserted : 0;
            //         const requested = result && result.requested != null ? result.requested : 0;
            //         console.info('GLB ATK-60 insertado:', inserted + '/' + requested);
            //     })
            //     .catch(function (err) {
            //         const msg = err && err.message ? err.message : String(err || 'Error al insertar GLB');
            //         console.error('No se pudo insertar GLB ATK-60:', msg);
            //     });

            function toNum(v) {
                const n = Number(v);
                return Number.isFinite(n) ? n : null;
            }

            function pointXmm(p) {
                if (!p || typeof p !== 'object') return null;
                return toNum(p.xMm != null ? p.xMm : p.x);
            }

            function pointYmm(p) {
                if (!p || typeof p !== 'object') return null;
                return toNum(p.yMm != null ? p.yMm : p.y);
            }

            function pointZmm(p) {
                if (!p || typeof p !== 'object') return null;
                return toNum(p.zMm != null ? p.zMm : p.z);
            }

            function buildWallGeom(w) {
                const attrs = (w && w.Attributes && typeof w.Attributes === 'object') ? w.Attributes : {};
                const p1 = w && w.P1 ? w.P1 : attrs.p1;
                const p2 = w && w.P2 ? w.P2 : attrs.p2;

                const startXmm = pointXmm(p1) != null ? pointXmm(p1) : toNum(attrs.InicioX);
                const startYmm = pointYmm(p1) != null ? pointYmm(p1) : toNum(attrs.InicioZ);
                const startZmm = pointZmm(p1) != null ? pointZmm(p1) : toNum(attrs.InicioY);

                const endXmm = pointXmm(p2) != null ? pointXmm(p2) : toNum(attrs.FinX);
                const endYmm = pointYmm(p2) != null ? pointYmm(p2) : toNum(attrs.FinZ);
                const endZmm = pointZmm(p2) != null ? pointZmm(p2) : toNum(attrs.FinY);

                let lengthMm = null;
                if (startXmm != null && startZmm != null && endXmm != null && endZmm != null) {
                    const dx = endXmm - startXmm;
                    const dz = endZmm - startZmm;
                    lengthMm = Math.sqrt(dx * dx + dz * dz);
                }

                let widthMm = toNum(attrs._DataWith);
                if (widthMm != null) widthMm = widthMm * 1000;
                if (widthMm == null) widthMm = toNum(attrs.ThicknessMm);

                let heightMm = toNum(attrs._DataHeight);
                if (heightMm != null) heightMm = heightMm * 1000;
                if (heightMm == null) heightMm = toNum(attrs.HeightMm);

                return {
                    Id: w && w.Id != null ? String(w.Id) : null,
                    LineId: w && w.LineId != null ? String(w.LineId) : null,
                    WallId: w && w.WallId != null ? String(w.WallId) : null,
                    StartXmm: startXmm,
                    StartYmm: startYmm,
                    StartZmm: startZmm,
                    EndXmm: endXmm,
                    EndYmm: endYmm,
                    EndZmm: endZmm,
                    LengthMm: lengthMm,
                    WidthMm: widthMm,
                    HeightMm: heightMm,
                };
            }

            const wallGeom = walls
                .map(buildWallGeom)
                .filter(function (g) {
                    return g
                        && (g.Id || g.LineId || g.WallId)
                        && g.StartXmm != null
                        && g.StartZmm != null
                        && g.EndXmm != null
                        && g.EndZmm != null;
                });

            function cornerKey(x, z) {
                return String(Math.round(x)) + '|' + String(Math.round(z));
            }

            const nodeDegree = Object.create(null);
            for (let i = 0; i < wallGeom.length; i++) {
                const g = wallGeom[i];
                const ks = cornerKey(g.StartXmm, g.StartZmm);
                const ke = cornerKey(g.EndXmm, g.EndZmm);
                nodeDegree[ks] = (nodeDegree[ks] || 0) + 1;
                nodeDegree[ke] = (nodeDegree[ke] || 0) + 1;
            }

            const CONNECTED_END_TRIM_MM = 450;
            for (let i = 0; i < wallGeom.length; i++) {
                const g = wallGeom[i];
                const dx = g.EndXmm - g.StartXmm;
                const dz = g.EndZmm - g.StartZmm;
                const len = Math.sqrt(dx * dx + dz * dz);
                if (!(len > 1e-6)) continue;

                const ux = dx / len;
                const uz = dz / len;
                const ks = cornerKey(g.StartXmm, g.StartZmm);
                const ke = cornerKey(g.EndXmm, g.EndZmm);
                const trimStart = (nodeDegree[ks] || 0) >= 2 ? CONNECTED_END_TRIM_MM : 0;
                const trimEnd = (nodeDegree[ke] || 0) >= 2 ? CONNECTED_END_TRIM_MM : 0;

                const trimmedLen = Math.max(0, len - trimStart - trimEnd);
                const newStartX = g.StartXmm + ux * trimStart;
                const newStartZ = g.StartZmm + uz * trimStart;
                const newEndX = newStartX + ux * trimmedLen;
                const newEndZ = newStartZ + uz * trimmedLen;

                g.RawStartXmm = g.StartXmm;
                g.RawStartZmm = g.StartZmm;
                g.RawEndXmm = g.EndXmm;
                g.RawEndZmm = g.EndZmm;
                g.RawLengthMm = len;
                g.TrimStartMm = trimStart;
                g.TrimEndMm = trimEnd;

                g.StartXmm = newStartX;
                g.StartZmm = newStartZ;
                g.EndXmm = newEndX;
                g.EndZmm = newEndZ;
                g.LengthMm = trimmedLen;
            }

            const wallGeomById = Object.create(null);
            function indexGeomKey(key, geom) {
                if (key == null) return;
                const k = String(key).trim();
                if (!k) return;
                wallGeomById[k] = geom;
            }
            for (let i = 0; i < wallGeom.length; i++) {
                const g = wallGeom[i];
                indexGeomKey(g.Id, g);
                indexGeomKey(g.LineId, g);
                indexGeomKey(g.WallId, g);
            }

            // Lista de envío al controlador con atributos normalizados desde geometría 3D real.
            const idsDetailed = walls
                .map(function (w) {
                    const id = w && w.LineId != null
                        ? String(w.LineId)
                        : (w && w.Id != null ? String(w.Id) : '');
                    if (!id) return null;

                    const lineId = w && w.LineId != null ? String(w.LineId) : null;
                    const wallId = w && w.WallId != null ? String(w.WallId) : null;
                    const geom = wallGeomById[id] || wallGeomById[lineId] || wallGeomById[wallId] || null;
                    const baseAttrs = (w && w.Attributes && typeof w.Attributes === 'object')
                        ? Object.assign({}, w.Attributes)
                        : Object.assign({}, w || {});

                    if (geom) {
                        // Convención del backend: InicioY/FinY corresponden a Z en planta, InicioZ/FinZ a Y vertical.
                        baseAttrs.InicioX = geom.StartXmm;
                        baseAttrs.InicioY = geom.StartZmm;
                        baseAttrs.InicioZ = geom.StartYmm != null ? geom.StartYmm : 0;
                        baseAttrs.FinX = geom.EndXmm;
                        baseAttrs.FinY = geom.EndZmm;
                        baseAttrs.FinZ = geom.EndYmm != null ? geom.EndYmm : 0;

                        // Mantener unidades legacy esperadas por C#.
                        if (geom.LengthMm != null) baseAttrs._Datalong = geom.LengthMm / 1000;
                        if (geom.WidthMm != null) baseAttrs._DataWith = geom.WidthMm / 1000;
                        if (geom.HeightMm != null) baseAttrs._DataHeight = geom.HeightMm / 1000;

                        const cx = (geom.StartXmm + geom.EndXmm) * 0.5;
                        const cy = ((geom.StartYmm != null ? geom.StartYmm : 0) + (geom.EndYmm != null ? geom.EndYmm : 0)) * 0.5;
                        const cz = (geom.StartZmm + geom.EndZmm) * 0.5;
                        baseAttrs._XCoordinate = cx;
                        baseAttrs._YCoordinate = cy;
                        baseAttrs._ZCoordinate = cz;

                        baseAttrs.__Source3D = true;
                        baseAttrs.__Geom3D = {
                            id: geom.Id,
                            lineId: geom.LineId,
                            wallId: geom.WallId,
                            startXmm: geom.StartXmm,
                            startYmm: geom.StartYmm,
                            startZmm: geom.StartZmm,
                            endXmm: geom.EndXmm,
                            endYmm: geom.EndYmm,
                            endZmm: geom.EndZmm,
                            lengthMm: geom.LengthMm,
                            rawStartXmm: geom.RawStartXmm,
                            rawStartZmm: geom.RawStartZmm,
                            rawEndXmm: geom.RawEndXmm,
                            rawEndZmm: geom.RawEndZmm,
                            rawLengthMm: geom.RawLengthMm,
                            trimStartMm: geom.TrimStartMm,
                            trimEndMm: geom.TrimEndMm,
                        };
                    }

                    return {
                        Id: id,
                        LineId: lineId,
                        WallGroupId: w && w.WallGroupId != null ? String(w.WallGroupId) : null,
                        Attributes: baseAttrs,
                    };
                })
                .filter(Boolean);

            const geomMatchedCount = idsDetailed.reduce(function (acc, item) {
                const attrs = item && item.Attributes ? item.Attributes : null;
                return acc + (attrs && attrs.__Source3D === true ? 1 : 0);
            }, 0);
            console.info('ATK60 source3D matched walls:', geomMatchedCount + '/' + idsDetailed.length);
            const lineLikeCount = idsDetailed.reduce(function (acc, item) {
                const attrs = item && item.Attributes ? item.Attributes : null;
                const tipo = attrs && attrs.Tipo != null ? String(attrs.Tipo) : '';
                return acc + (tipo.toLowerCase() === 'line' ? 1 : 0);
            }, 0);
            console.info('ATK60 line-like payload walls:', lineLikeCount);
            console.info('ATK60 wall sources:', {
                wallModelSource: Array.isArray(wallsFromWallModelSource) ? wallsFromWallModelSource.length : 0,
                scene: Array.isArray(wallsFromScene) ? wallsFromScene.length : 0,
                formwork: Array.isArray(wallsFromFormwork) ? wallsFromFormwork.length : 0,
                merged: walls.length,
            });

            walls = idsDetailed;

            const payload = {
                id: 0,
                list: walls,
                system: selectedSystem,
                walls: walls,
                wallGeom: wallGeom,
                wallConnections: wallConnections,
                meta: {
                    generatedAtUtc: new Date().toISOString(),
                    pageUrl: window.location.href,
                    counts: {
                        scene: Array.isArray(wallsFromScene) ? wallsFromScene.length : 0,
                        formwork: Array.isArray(wallsFromFormwork) ? wallsFromFormwork.length : 0,
                        wallModelSource: Array.isArray(wallsFromWallModelSource) ? wallsFromWallModelSource.length : 0,
                        connections: wallConnections && Array.isArray(wallConnections.Nodes) ? wallConnections.Nodes.length : 0,
                        merged: walls.length,
                        geom: wallGeom.length,
                    },
                },
            };

            if (typeof window.jQuery === 'undefined' || !window.jQuery.ajax) {
                console.error('No existe jQuery.ajax disponible para encofrar.');
                return;
            }

            window.jQuery.ajax({
                type: 'POST',
                url: atk60Url,
                dataType: 'json',
                data: {
                    IdsJson: JSON.stringify(idsDetailed),
                },
                success: function (resp) {
                    if (!resp || resp.Exito !== true) {
                        console.error((resp && resp.Mensaje) || 'No se pudo iniciar Encofrar ATK-60.');
                        return;
                    }

                    const renderAnchors = window.maStlDesing2RenderAtk60AnchorPoints;
                    const renderElements = window.maStlDesing2RenderAtk60Elements;
                    const anchorWalls = resp
                        && resp.ElementsForThreeJs
                        && Array.isArray(resp.ElementsForThreeJs.Walls)
                        ? resp.ElementsForThreeJs.Walls
                        : [];
                    const elementItems = resp
                        && resp.ElementsForThreeJs
                        && Array.isArray(resp.ElementsForThreeJs.Elements)
                        ? resp.ElementsForThreeJs.Elements
                        : [];

                    if (typeof renderAnchors === 'function') {
                        const result = renderAnchors(anchorWalls, { clearPrevious: true });
                        const inserted = result && result.inserted != null ? result.inserted : 0;
                        const requested = result && result.requested != null ? result.requested : 0;
                        console.info('ATK-60 puntos ancla pintados:', inserted + '/' + requested);
                    } else {
                        console.error('No existe API maStlDesing2RenderAtk60AnchorPoints.');
                    }

                    if (typeof renderElements === 'function') {
                        renderElements(elementItems, { clearPrevious: false })
                            .then(function (result) {
                                const inserted = result && result.inserted != null ? result.inserted : 0;
                                const requested = result && result.requested != null ? result.requested : 0;
                                console.info('ATK-60 paneles pintados:', inserted + '/' + requested);
                            })
                            .catch(function (err) {
                                const msg = err && err.message ? err.message : String(err || 'Error al pintar paneles');
                                console.error('ATK-60 error al pintar paneles:', msg);
                            });
                    } else {
                        console.error('No existe API maStlDesing2RenderAtk60Elements.');
                    }

                    const wallsReturned = Array.isArray(resp.Walls) ? resp.Walls : [];
                    console.info('Muros recibidos (' + wallsReturned.length + '):', wallsReturned);
                },
                error: function (xhr, _status, err) {
                    const serverMsg = xhr && xhr.responseJSON && xhr.responseJSON.Mensaje;
                    console.error('Error al encofrar:', (serverMsg || (err && err.message) || 'Error HTTP'));
                },
            });
        });
    }

    function wireFormworkVisibilityByMode() {
        const formworkBtn = document.getElementById('ma-stl-mode-formwork');
        const wall3dBtn = document.getElementById('ma-stl-mode-wall-3d');
        const modeBar = document.getElementById('desing2-stl-mode-toolbar');
        if (!formworkBtn || !wall3dBtn || !modeBar) return;

        function isWall3dActive() {
            return wall3dBtn.classList.contains('active') || wall3dBtn.getAttribute('aria-pressed') === 'true';
        }

        function syncVisibility() {
            const visible = isWall3dActive();
            formworkBtn.classList.toggle('d-none', !visible);
            formworkBtn.setAttribute('aria-hidden', visible ? 'false' : 'true');
            if (visible) {
                formworkBtn.removeAttribute('tabindex');
            } else {
                formworkBtn.setAttribute('tabindex', '-1');
            }
        }

        modeBar.addEventListener('click', function () {
            window.setTimeout(syncVisibility, 0);
        });

        if (typeof MutationObserver !== 'undefined') {
            const observer = new MutationObserver(syncVisibility);
            observer.observe(wall3dBtn, {
                attributes: true,
                attributeFilter: ['class', 'aria-pressed'],
            });
        }

        syncVisibility();
    }

    wireHoverLockPanel('#desing2-stl-hover-side-panel', 'desing2-stl-side-panel-collapse', '.desing2-stl-hover-side-panel__hover-zone');
    wireHoverLockPanel('#desing2-stl-hover-right-panel', 'desing2-stl-right-panel-collapse', '.desing2-stl-hover-right-panel__hover-zone');
    wireRulersFlyout();
    wireModeSystemDropdown();
    wireFormworkButton();
    wireFormworkVisibilityByMode();
    wireInitialStlBoot();
})();
