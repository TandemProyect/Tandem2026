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
        const overlayRoot = document.getElementById('ma-stl-formwork-render-overlay');
        const overlayTitle = document.getElementById('ma-stl-formwork-render-title');
        const overlayStatus = document.getElementById('ma-stl-formwork-render-status');
        const overlayEta = document.getElementById('ma-stl-formwork-render-eta');
        const overlayBar = document.getElementById('ma-stl-formwork-render-bar');
        const overlayProgress = overlayRoot
            ? overlayRoot.querySelector('.desing2-stl-render-overlay__progress')
            : null;
        let formworkBusy = false;
        let formworkTickTimer = null;
        let formworkState = null;

        function overlayAttr(name, fallback) {
            if (!overlayRoot) return fallback;
            const v = overlayRoot.getAttribute(name);
            return v != null && String(v).trim() !== '' ? String(v) : fallback;
        }

        function overlayFill(tpl, value) {
            return String(tpl || '').replace('{0}', String(value));
        }

        function formatFormworkEta(ms) {
            const almost = overlayAttr(
                'data-ma-stl-formwork-eta-almost',
                'Casi listo…'
            );
            if (!(ms > 0) || ms < 1400) return almost;
            const sec = Math.max(1, Math.ceil(ms / 1000));
            if (sec < 55) {
                return overlayFill(
                    overlayAttr('data-ma-stl-formwork-eta-seconds', 'Quedan unos {0} s'),
                    sec
                );
            }
            const min = Math.max(1, Math.round(sec / 60));
            if (min <= 1) {
                return overlayAttr('data-ma-stl-formwork-eta-minute', 'Queda 1 min');
            }
            return overlayFill(
                overlayAttr('data-ma-stl-formwork-eta-minutes', 'Quedan unos {0} min'),
                min
            );
        }

        function estimateServerMs(wallCount) {
            const n = Math.max(1, wallCount || 1);
            return 1600 + n * 420;
        }

        function stopFormworkTicker() {
            if (formworkTickTimer) {
                window.clearInterval(formworkTickTimer);
                formworkTickTimer = null;
            }
        }

        function refreshFormworkOverlay() {
            if (!overlayRoot || !formworkState) return;
            const now = Date.now();
            let remainingMs;
            let pct;
            if (formworkState.phase === 'paint' && formworkState.total > 0) {
                const done = Math.max(0, formworkState.done || 0);
                const total = formworkState.total;
                const elapsedPaint = Math.max(1, now - (formworkState.paintStartedAt || now));
                const avgMs = done > 0 ? elapsedPaint / done : 70;
                remainingMs = Math.max(500, (total - done) * avgMs);
                pct = Math.max(8, Math.min(99, Math.round((done / total) * 100)));
                if (overlayStatus) {
                    overlayStatus.textContent = overlayAttr(
                        'data-ma-stl-formwork-phase-paint',
                        'Pintando paneles…'
                    );
                }
            } else {
                const predicted = formworkState.predictedMs || estimateServerMs(formworkState.wallCount);
                const elapsed = now - formworkState.startedAt;
                remainingMs = Math.max(1100, predicted - elapsed * 0.72);
                pct = Math.max(6, Math.min(42, Math.round((elapsed / predicted) * 38)));
                if (overlayStatus) {
                    overlayStatus.textContent = overlayAttr(
                        'data-ma-stl-formwork-phase-server',
                        'Calculando módulos…'
                    );
                }
            }
            if (overlayEta) overlayEta.textContent = formatFormworkEta(remainingMs);
            if (overlayBar) overlayBar.style.width = pct + '%';
            if (overlayProgress) overlayProgress.setAttribute('aria-valuenow', String(pct));
        }

        function showFormworkOverlay(systemName, wallCount) {
            if (!overlayRoot) return;
            formworkState = {
                phase: 'server',
                startedAt: Date.now(),
                wallCount: wallCount,
                predictedMs: estimateServerMs(wallCount),
                done: 0,
                total: 0,
                paintStartedAt: 0,
            };
            const titleTpl = overlayAttr(
                'data-ma-stl-formwork-title',
                'Renderizando sistema {0}'
            );
            if (overlayTitle) overlayTitle.textContent = overlayFill(titleTpl, systemName || 'ATK-60');
            const spinner = overlayRoot.querySelector('.spinner-border');
            if (spinner) spinner.classList.remove('d-none');
            overlayRoot.classList.remove('d-none');
            overlayRoot.setAttribute('aria-hidden', 'false');
            overlayRoot.setAttribute('aria-busy', 'true');
            refreshFormworkOverlay();
            stopFormworkTicker();
            formworkTickTimer = window.setInterval(refreshFormworkOverlay, 280);
        }

        function beginFormworkPaintPhase(total) {
            if (!formworkState) return;
            formworkState.phase = 'paint';
            formworkState.total = Math.max(0, total || 0);
            formworkState.done = 0;
            formworkState.paintStartedAt = Date.now();
            refreshFormworkOverlay();
        }

        function updateFormworkPaintProgress(done, total) {
            if (!formworkState) return;
            formworkState.phase = 'paint';
            if (Number.isFinite(total)) formworkState.total = Math.max(0, total);
            if (Number.isFinite(done)) formworkState.done = Math.max(0, done);
            refreshFormworkOverlay();
        }

        function clockNow() {
            const now = new Date();
            const pad = function (n) { return n < 10 ? '0' + n : String(n); };
            return pad(now.getHours()) + ':' + pad(now.getMinutes()) + ':' + pad(now.getSeconds());
        }

        function finishHideFormworkOverlay(ok, message) {
            if (overlayRoot) {
                overlayRoot.classList.add('d-none');
                overlayRoot.setAttribute('aria-hidden', 'true');
                overlayRoot.setAttribute('aria-busy', 'false');
            }
            formworkBusy = false;
            btn.disabled = false;
            btn.classList.remove('disabled');
            document.dispatchEvent(new CustomEvent('desing2-formwork-settled', {
                detail: { ok: ok !== false, message: message || '' }
            }));
        }

        function hideFormworkOverlay(ok, message) {
            stopFormworkTicker();
            formworkState = null;
            const showing = overlayRoot && !overlayRoot.classList.contains('d-none');
            if (ok !== false && showing) {
                if (overlayBar) overlayBar.style.width = '100%';
                if (overlayProgress) overlayProgress.setAttribute('aria-valuenow', '100');
                const clock = clockNow();
                if (overlayEta) overlayEta.textContent = clock;
                if (overlayStatus) overlayStatus.textContent = clock;
                const spinner = overlayRoot.querySelector('.spinner-border');
                if (spinner) spinner.classList.add('d-none');
                overlayRoot.setAttribute('aria-busy', 'false');
                window.setTimeout(function () { finishHideFormworkOverlay(true, message); }, 1600);
                return;
            }
            if (overlayBar) overlayBar.style.width = '100%';
            finishHideFormworkOverlay(ok, message);
        }

        function settleFormworkEarly(message) {
            document.dispatchEvent(new CustomEvent('desing2-formwork-settled', {
                detail: { ok: false, message: message || '' }
            }));
        }

        let skipPhaseAsk = false;
        let selectedPhase = 0;
        let askingPhase = false;

        function phasesFromByWall(byWall) {
            const seen = {};
            Object.keys(byWall || {}).forEach(function (id) {
                const n = Number(byWall[id]);
                if (n >= 1) seen[n] = true;
            });
            return Object.keys(seen).map(Number).sort(function (a, b) { return a - b; });
        }

        function loadPhaseNumbers() {
            const plan = window.desing2PhaseStartPlan;
            const local = phasesFromByWall(plan && plan.byWall);
            if (local.length >= 2) return Promise.resolve(local);
            if (local.length === 1 && plan && plan.byWall) return Promise.resolve(local);
            const shell = document.getElementById('ma-stl-viewer-shell');
            const designId = shell ? Number(shell.getAttribute('data-ma-stl-design-id')) : 0;
            const url = btn.getAttribute('data-phase-load-url') || '';
            if (!designId || !url) return Promise.resolve(local);
            const join = url.indexOf('?') >= 0 ? '&' : '?';
            return fetch(url + join + 'designId=' + encodeURIComponent(String(designId)), {
                method: 'GET',
                headers: { Accept: 'application/json' },
                credentials: 'same-origin'
            }).then(function (res) {
                return res.ok ? res.json() : null;
            }).then(function (resp) {
                const rows = resp && (resp.Walls || resp.walls) ? (resp.Walls || resp.walls) : [];
                if (!rows.length) return local;
                const byWall = {};
                rows.forEach(function (row) {
                    const phase = Number(row.Phase != null ? row.Phase : row.phase);
                    const wallId = String(row.WallId != null ? row.WallId : (row.wallId || ''));
                    if (wallId && phase >= 1) byWall[wallId] = phase;
                });
                if (!window.desing2PhaseStartPlan) window.desing2PhaseStartPlan = {};
                window.desing2PhaseStartPlan.byWall = byWall;
                return phasesFromByWall(byWall);
            }).catch(function () { return local; });
        }

        function phaseSegments(phaseNo) {
            const plan = window.desing2PhaseStartPlan || {};
            const byWall = plan.byWall || {};
            const wanted = Object.create(null);
            Object.keys(byWall).forEach(function (wallId) {
                if (Number(byWall[wallId]) === Number(phaseNo)) wanted[String(wallId)] = true;
            });
            const segs = [];
            if (typeof window.maStlDesing2BuildWallExchangePayload !== 'function') return segs;
            let lines = [];
            try {
                const payload = window.maStlDesing2BuildWallExchangePayload();
                lines = payload && payload.walls ? payload.walls : [];
            } catch (_err) {
                return segs;
            }
            lines.forEach(function (line) {
                if (!line || line.wallRole !== 'axis' || !line.p1Mm || !line.p2Mm) return;
                const wid = line.wallId != null ? String(line.wallId) : String(line.id);
                const lid = line.id != null ? String(line.id) : '';
                if (!wanted[wid] && !wanted[lid]) return;
                segs.push({
                    id: lid,
                    wallId: wid,
                    x1: Number(line.p1Mm.x),
                    z1: Number(line.p1Mm.z),
                    x2: Number(line.p2Mm.x),
                    z2: Number(line.p2Mm.z)
                });
            });
            return segs;
        }

        function wallOnPhase(wall, segs) {
            if (!wall || !segs.length) return false;
            const attrs = wall.Attributes || {};
            const lineKeys = [wall.Id, wall.LineId, attrs._idObject, attrs.id];
            const wallKeys = [wall.WallId, attrs.wallId, attrs.WallId];
            for (let s = 0; s < segs.length; s++) {
                const seg = segs[s];
                for (let i = 0; i < lineKeys.length; i++) {
                    if (lineKeys[i] != null && lineKeys[i] !== '' && String(lineKeys[i]) === seg.id) return true;
                }
                for (let j = 0; j < wallKeys.length; j++) {
                    if (wallKeys[j] != null && wallKeys[j] !== '' && String(wallKeys[j]) === seg.wallId) return true;
                }
            }
            const p1 = wall.P1 || {};
            const p2 = wall.P2 || {};
            const x1 = Number(p1.xMm != null ? p1.xMm : attrs.InicioX);
            const z1 = Number(p1.zMm != null ? p1.zMm : attrs.InicioY);
            const x2 = Number(p2.xMm != null ? p2.xMm : attrs.FinX);
            const z2 = Number(p2.zMm != null ? p2.zMm : attrs.FinY);
            if (![x1, z1, x2, z2].every(function (n) { return Number.isFinite(n); })) return false;
            for (let s = 0; s < segs.length; s++) {
                const seg = segs[s];
                const same = ((x1 - seg.x1) * (x1 - seg.x1)) + ((z1 - seg.z1) * (z1 - seg.z1))
                    + ((x2 - seg.x2) * (x2 - seg.x2)) + ((z2 - seg.z2) * (z2 - seg.z2));
                const flip = ((x1 - seg.x2) * (x1 - seg.x2)) + ((z1 - seg.z2) * (z1 - seg.z2))
                    + ((x2 - seg.x1) * (x2 - seg.x1)) + ((z2 - seg.z1) * (z2 - seg.z1));
                if (Math.min(same, flip) < 500 * 500) return true;
            }
            return false;
        }

        function phaseColor(phaseNo) {
            const summary = window.desing2PhaseStartPlan && window.desing2PhaseStartPlan.phaseSummary;
            if (Array.isArray(summary)) {
                for (let i = 0; i < summary.length; i++) {
                    if (Number(summary[i].phase) === Number(phaseNo) && summary[i].color) return summary[i].color;
                }
            }
            const colors = ['#3b82f6', '#16a34a', '#d97706', '#9333ea', '#dc2626', '#0891b2'];
            return colors[(Number(phaseNo) - 1) % colors.length];
        }

        function fillPhaseLabel(phaseNo) {
            const tpl = btn.getAttribute('data-phase-label') || 'Fase {0}';
            return String(tpl).replace('{0}', String(phaseNo));
        }

        function openPhasePicker(phases, onChoose) {
            let modal = document.getElementById('desing2FormworkPhaseModal');
            if (!modal) {
                modal = document.createElement('div');
                modal.id = 'desing2FormworkPhaseModal';
                modal.className = 'modal fade';
                modal.tabIndex = -1;
                modal.innerHTML =
                    '<div class="modal-dialog modal-dialog-centered modal-sm">' +
                    '<div class="modal-content">' +
                    '<div class="modal-header py-2"><h6 class="modal-title mb-0"></h6>' +
                    '<button type="button" class="btn-close" data-bs-dismiss="modal" aria-label="Close"></button></div>' +
                    '<div class="modal-body d-grid gap-2" data-choices></div>' +
                    '<div class="modal-footer py-2"><button type="button" class="btn btn-sm btn-outline-secondary" data-bs-dismiss="modal" data-cancel></button></div>' +
                    '</div></div>';
                document.body.appendChild(modal);
            }
            const title = modal.querySelector('.modal-title');
            const choices = modal.querySelector('[data-choices]');
            const cancel = modal.querySelector('[data-cancel]');
            if (title) title.textContent = btn.getAttribute('data-phase-ask') || '¿Qué fase quieres ver?';
            if (cancel) cancel.textContent = btn.getAttribute('data-phase-cancel') || 'Cancelar';
            choices.innerHTML = '';
            phases.forEach(function (phaseNo) {
                const choice = document.createElement('button');
                choice.type = 'button';
                choice.className = 'btn btn-outline-primary text-start';
                choice.innerHTML = '<i class="d-inline-block rounded-circle me-2" style="width:.7rem;height:.7rem;background:' +
                    phaseColor(phaseNo) + '"></i>' + fillPhaseLabel(phaseNo);
                choice.addEventListener('click', function () {
                    if (window.bootstrap && window.bootstrap.Modal) {
                        window.bootstrap.Modal.getOrCreateInstance(modal).hide();
                    }
                    onChoose(phaseNo);
                });
                choices.appendChild(choice);
            });
            if (!window.bootstrap || !window.bootstrap.Modal) {
                onChoose(phases[0]);
                return;
            }
            modal.addEventListener('hidden.bs.modal', function () {
                if (askingPhase) askingPhase = false;
            }, { once: true });
            window.bootstrap.Modal.getOrCreateInstance(modal).show();
        }

        btn.addEventListener('click', function () {
            if (formworkBusy || btn.disabled || askingPhase) return;
            const selectedSystem = ((systemToggle && systemToggle.textContent) || '').trim();

            if (selectedSystem !== 'Atk-60') {
                console.warn('Sistema no soportado por ahora:', selectedSystem);
                settleFormworkEarly('Sistema no soportado.');
                return;
            }

            const atk60Url = btn.getAttribute('data-ma-formwork-url-atk60');
            if (!atk60Url) {
                console.error('No existe URL configurada para Encofrar ATK-60.');
                settleFormworkEarly('No existe URL configurada para Encofrar ATK-60.');
                return;
            }

            if (!skipPhaseAsk) {
                askingPhase = true;
                loadPhaseNumbers().then(function (phases) {
                    if (!phases || phases.length < 2) {
                        askingPhase = false;
                        skipPhaseAsk = true;
                        selectedPhase = 0;
                        btn.click();
                        return;
                    }
                    openPhasePicker(phases, function (phaseNo) {
                        askingPhase = false;
                        skipPhaseAsk = true;
                        selectedPhase = phaseNo;
                        btn.click();
                    });
                }).catch(function () {
                    askingPhase = false;
                    skipPhaseAsk = true;
                    selectedPhase = 0;
                    btn.click();
                });
                return;
            }
            skipPhaseAsk = false;
            const formPhase = selectedPhase;
            selectedPhase = 0;

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

            if (typeof window.maStlDesing2MergeCollinearAxisRuns === 'function') {
                const mergedAxes = window.maStlDesing2MergeCollinearAxisRuns();
                if (mergedAxes > 0 && typeof window.maStlDesing2SaveDesignWallsToServer === 'function') {
                    window.maStlDesing2SaveDesignWallsToServer({ includePhoto: false, quiet: true }).catch(function () {});
                }
            }
            if (typeof window.maStlDesing2ReleaseEmptySpecialWalls === 'function') {
                window.maStlDesing2ReleaseEmptySpecialWalls();
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
            const wallIsSpecial = typeof window.maStlDesing2WallIsSpecial === 'function'
                ? window.maStlDesing2WallIsSpecial
                : function (w) {
                    const attrs = w && w.Attributes ? w.Attributes : w;
                    return !!(attrs && (attrs.isSpecial === true || attrs.IsSpecial === true
                        || attrs._IsFormwork === false || attrs._IsFormwork === 0));
                };
            walls = walls.filter(function (w) { return !wallIsSpecial(w); });
            if (!walls.length) {
                // Fallback 3D seguro: ejes de muro en escena (misma fuente del inspector de atributos).
                walls = mergeWallsUnique([wallsFromScene]).filter(function (w) { return !wallIsSpecial(w); });
                if (!walls.length) {
                    window.alert('ATK60: no hay muros 3D para encofrar (los especiales se omiten).');
                    console.error('ATK60 abortado: sin muros 3D automáticos.');
                    settleFormworkEarly('No hay muros 3D para encofrar.');
                    return;
                }
                console.warn('ATK60 fallback: usando ejes 3D de escena por wallModelSource vacío.');
            }

            if (formPhase > 0) {
                const segs = phaseSegments(formPhase);
                walls = walls.filter(function (w) { return wallOnPhase(w, segs); });
                if (!walls.length) {
                    const none = btn.getAttribute('data-phase-none') || 'Esa fase no tiene muros para encofrar.';
                    window.alert(none);
                    settleFormworkEarly(none);
                    return;
                }
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

            // Recorte 450 mm en nudos: Atk60WallsRepository.SolveFromIdsJson
            // (mismo código para Desing y AutoCAD). No recortar aquí.
            for (let i = 0; i < wallGeom.length; i++) {
                const g = wallGeom[i];
                g.RawStartXmm = g.StartXmm;
                g.RawStartZmm = g.StartZmm;
                g.RawEndXmm = g.EndXmm;
                g.RawEndZmm = g.EndZmm;
                g.RawLengthMm = g.LengthMm;
                g.TrimStartMm = 0;
                g.TrimEndMm = 0;
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
            function cornerFacesForAxis(connections, axisId) {
                const lines = connections && Array.isArray(connections.lines) ? connections.lines : [];
                const id = axisId == null ? '' : String(axisId);
                const out = [];
                for (let i = 0; i < lines.length; i++) {
                    const line = lines[i];
                    if (!line || String(line.wallRole || '') !== 'face') continue;
                    if (String(line.linkOffsetFromLineId) !== id) continue;
                    const p1 = line.p1 || {};
                    const p2 = line.p2 || {};
                    const x1 = toNum(p1.xMm != null ? p1.xMm : p1.x);
                    const z1 = toNum(p1.zMm != null ? p1.zMm : p1.z);
                    const x2 = toNum(p2.xMm != null ? p2.xMm : p2.x);
                    const z2 = toNum(p2.zMm != null ? p2.zMm : p2.z);
                    if (x1 == null || z1 == null || x2 == null || z2 == null) continue;
                    let lengthMm = toNum(line.lengthMm);
                    if (lengthMm == null) {
                        const dx = x2 - x1;
                        const dz = z2 - z1;
                        lengthMm = Math.sqrt((dx * dx) + (dz * dz));
                    }
                    out.push({
                        side: Number(line.numberWallFaceSideSign) < 0 ? -1 : 1,
                        lengthMm: lengthMm,
                        x1: x1,
                        z1: z1,
                        x2: x2,
                        z2: z2,
                    });
                }
                return out;
            }

            function planAxisIdNearWall(connections, startX, startZ, endX, endZ, fallbackId) {
                const lines = connections && Array.isArray(connections.lines) ? connections.lines : [];
                let bestId = null;
                let best = 150 * 150;
                for (let i = 0; i < lines.length; i++) {
                    const line = lines[i];
                    if (!line || String(line.wallRole || '') !== 'axis') continue;
                    const p1 = line.p1 || {};
                    const p2 = line.p2 || {};
                    const x1 = toNum(p1.xMm != null ? p1.xMm : p1.x);
                    const z1 = toNum(p1.zMm != null ? p1.zMm : p1.z);
                    const x2 = toNum(p2.xMm != null ? p2.xMm : p2.x);
                    const z2 = toNum(p2.zMm != null ? p2.zMm : p2.z);
                    if (x1 == null || z1 == null || x2 == null || z2 == null) continue;
                    const same = ((startX - x1) * (startX - x1)) + ((startZ - z1) * (startZ - z1))
                        + ((endX - x2) * (endX - x2)) + ((endZ - z2) * (endZ - z2));
                    const flip = ((startX - x2) * (startX - x2)) + ((startZ - z2) * (startZ - z2))
                        + ((endX - x1) * (endX - x1)) + ((endZ - z1) * (endZ - z1));
                    const dist = same < flip ? same : flip;
                    if (dist < best) {
                        best = dist;
                        bestId = line.id;
                    }
                }
                return bestId != null ? bestId : fallbackId;
            }

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

                    if (typeof window.maStlDesing2PropSpacingMm === 'function') {
                        const propSpacingMm = Number(window.maStlDesing2PropSpacingMm());
                        if (Number.isFinite(propSpacingMm) && propSpacingMm > 0) {
                            baseAttrs._PropSpacingMm = propSpacingMm;
                        }
                    }

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

                    if (geom) {
                        const axisId = planAxisIdNearWall(
                            wallConnections,
                            geom.StartXmm,
                            geom.StartZmm,
                            geom.EndXmm,
                            geom.EndZmm,
                            lineId || id
                        );
                        const cornerFaces = cornerFacesForAxis(wallConnections, axisId);
                        if (cornerFaces.length) {
                            baseAttrs.CornerFaces = cornerFaces;
                        }
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
                settleFormworkEarly('No existe jQuery.ajax disponible para encofrar.');
                return;
            }

            formworkBusy = true;
            btn.disabled = true;
            btn.classList.add('disabled');
            showFormworkOverlay(selectedSystem === 'Atk-60' ? 'ATK-60' : (selectedSystem || 'ATK-60'), walls.length);

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
                        hideFormworkOverlay(false, (resp && resp.Mensaje) || 'No se pudo iniciar Encofrar ATK-60.');
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

                    let paintDone = Promise.resolve();
                    if (typeof renderElements === 'function') {
                        beginFormworkPaintPhase(elementItems.length);
                        paintDone = renderElements(elementItems, {
                            clearPrevious: false,
                            onProgress: function (info) {
                                if (!info) return;
                                updateFormworkPaintProgress(info.done, info.total);
                            },
                        })
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
                    paintDone.then(function () { hideFormworkOverlay(true); }, function () { hideFormworkOverlay(false, 'Error al pintar paneles'); });
                },
                error: function (xhr, _status, err) {
                    const serverMsg = xhr && xhr.responseJSON && xhr.responseJSON.Mensaje;
                    console.error('Error al encofrar:', (serverMsg || (err && err.message) || 'Error HTTP'));
                    hideFormworkOverlay(false, serverMsg || (err && err.message) || 'Error HTTP');
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
            const formworkOn = formworkBtn.classList.contains('active')
                || formworkBtn.getAttribute('aria-pressed') === 'true';
            const visible = isWall3dActive() || formworkOn;
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
