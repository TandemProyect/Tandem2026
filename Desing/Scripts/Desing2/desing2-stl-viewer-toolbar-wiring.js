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

    wireHoverLockPanel('#desing2-stl-hover-side-panel', 'desing2-stl-side-panel-collapse', '.desing2-stl-hover-side-panel__hover-zone');
    wireHoverLockPanel('#desing2-stl-hover-right-panel', 'desing2-stl-right-panel-collapse', '.desing2-stl-hover-right-panel__hover-zone');
    wireRulersFlyout();
    wireInitialStlBoot();
})();
