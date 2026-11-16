/*
 * Funciones (roles) en cliente.
 *
 * window.TandemDeniedFunctions (lo pinta _TandemFunctions.cshtml) trae los comandos protegidos
 * que el usuario NO tiene. Cualquier elemento cuyo comando esté en esa lista se oculta y su
 * clic se bloquea. Un comando sin función creada es libre para todos.
 *
 * Claves de un elemento:
 *   - data-function="Clave1,Clave2"   (explícitas)
 *   - data-ma-stl-tool="polyline"     → Tool.polyline
 *   - data-ma-stl-mode="formwork"     → Mode.formwork
 *   - data-ma-system="Atk-60"         → System.Atk-60
 *   - data-plugin-menu="save-walls"   → Plugin.save-walls
 *
 * En código JS: if (!TandemFunctions.can('Tool.polyline')) return;
 * El servidor comprueba siempre controlador / acción (FunctionAuthorizeFilter).
 */
(function (w, d) {
    'use strict';

    var AUTO = [
        ['data-ma-stl-tool', 'Tool.'],
        ['data-ma-stl-mode', 'Mode.'],
        ['data-ma-system', 'System.'],
        ['data-plugin-menu', 'Plugin.']
    ];
    var SELECTOR = '[data-function],' + AUTO.map(function (a) { return '[' + a[0] + ']'; }).join(',');

    var denied = {};
    (w.TandemDeniedFunctions || []).forEach(function (c) {
        if (c) { denied[String(c).trim().toLowerCase()] = true; }
    });
    var hasDenied = Object.keys(denied).length > 0;

    function can(code) {
        return !code || !denied[String(code).trim().toLowerCase()];
    }

    function codesOf(el) {
        var codes = [];
        if (!el || el.nodeType !== 1) { return codes; }
        var explicit = el.getAttribute('data-function');
        if (explicit) {
            explicit.split(',').forEach(function (c) { if (c.trim()) { codes.push(c.trim()); } });
        }
        AUTO.forEach(function (a) {
            var v = el.getAttribute(a[0]);
            if (v) { codes.push(a[1] + v); }
        });
        return codes;
    }

    function allowed(el) {
        return codesOf(el).every(can);
    }

    function hide(el) {
        if (el.getAttribute('data-tandem-function-denied') === '1') { return; }
        el.setAttribute('data-tandem-function-denied', '1');
        el.setAttribute('aria-hidden', 'true');
        el.style.setProperty('display', 'none', 'important');
        if ('disabled' in el) { el.disabled = true; }
    }

    function apply(root) {
        if (!hasDenied || !root) { return; }
        if (root.nodeType === 1 && root.matches && root.matches(SELECTOR) && !allowed(root)) {
            hide(root);
        }
        if (root.querySelectorAll) {
            Array.prototype.forEach.call(root.querySelectorAll(SELECTOR), function (el) {
                if (!allowed(el)) { hide(el); }
            });
        }
    }

    function guard(e) {
        var el = e.target;
        while (el && el !== d) {
            if (el.nodeType === 1 && codesOf(el).length && !allowed(el)) {
                e.preventDefault();
                e.stopImmediatePropagation();
                return;
            }
            el = el.parentNode;
        }
    }

    if (hasDenied) {
        ['click', 'dblclick', 'pointerdown', 'mousedown', 'submit'].forEach(function (type) {
            d.addEventListener(type, guard, true);
        });

        var start = function () {
            apply(d.body || d.documentElement);
            if (w.MutationObserver) {
                new MutationObserver(function (mutations) {
                    mutations.forEach(function (m) {
                        Array.prototype.forEach.call(m.addedNodes || [], apply);
                    });
                }).observe(d.documentElement, { childList: true, subtree: true });
            }
        };
        if (d.readyState === 'loading') {
            d.addEventListener('DOMContentLoaded', start);
        } else {
            start();
        }
    }

    w.TandemFunctions = {
        can: can,
        canAll: function (codes) { return (codes || []).every(can); },
        denied: Object.keys(denied),
        apply: apply
    };
})(window, document);
