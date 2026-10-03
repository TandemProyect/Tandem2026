// @ts-nocheck
(function () {
    'use strict';

    function pick(obj, camel, pascal) {
        if (!obj) return undefined;
        if (obj[camel] != null) return obj[camel];
        if (pascal && obj[pascal] != null) return obj[pascal];
        return undefined;
    }

    function text(el, value) {
        if (el) el.textContent = value == null || value === '' ? '—' : String(value);
    }

    function attr(el, name, fallback) {
        if (!el) return fallback || '';
        return el.getAttribute(name) || fallback || '';
    }

    function qualityDots(box, n) {
        if (!box) return;
        var dots = box.querySelectorAll('.desing2-conn-test__dots i');
        for (var i = 0; i < dots.length; i++)
            dots[i].className = i < n ? 'is-on' : '';
    }

    function qualityMap(panel) {
        return {
            good: { dots: 4, cls: 'is-good', text: attr(panel, 'data-lbl-good', 'Buena') },
            ok: { dots: 3, cls: 'is-ok', text: attr(panel, 'data-lbl-ok', 'Aceptable') },
            poor: { dots: 2, cls: 'is-poor', text: attr(panel, 'data-lbl-poor', 'Regular') },
            bad: { dots: 1, cls: 'is-bad', text: attr(panel, 'data-lbl-bad', 'No es buena') }
        };
    }

    function paintQuality(panel, boxKey, labelKey, quality) {
        var box = panel.querySelector('[data-conn="' + boxKey + '"]');
        var map = qualityMap(panel);
        var q = map[quality] || { dots: 0, cls: 'is-fail', text: attr(panel, 'data-msg-fail') };
        if (box) box.className = 'desing2-conn-test__quality ' + q.cls;
        qualityDots(box, q.dots);
        text(panel.querySelector('[data-conn="' + labelKey + '"]'), q.text);
    }

    function friendlyTime(ms) {
        if (ms == null || ms < 0 || isNaN(ms)) return '';
        if (ms < 50) return (ms / 1000).toFixed(2).replace('.', ',') + ' s';
        return (ms / 1000).toFixed(1).replace('.', ',') + ' s';
    }

    function friendlyHost(host, appUrl) {
        var h = ((host || appUrl || '') + '').toLowerCase();
        if (!h) return '—';
        if (h.indexOf('localhost') >= 0 || h.indexOf('127.0.0.1') >= 0)
            return 'Este ordenador';
        if (h.indexOf('tdesing.net') >= 0)
            return 'Internet (TDesing)';
        try {
            if (appUrl) return new URL(appUrl).host;
        } catch (e1) { }
        return host || '—';
    }

    function qualityFromMs(ms) {
        if (ms == null || ms < 0 || isNaN(ms)) return '';
        if (ms < 80) return 'good';
        if (ms < 200) return 'ok';
        if (ms < 500) return 'poor';
        return 'bad';
    }

    function qualityFromMbps(mbps) {
        if (mbps >= 100) return 'good';
        if (mbps >= 50) return 'ok';
        if (mbps >= 20) return 'poor';
        if (mbps > 0) return 'bad';
        return '';
    }

    function browserNet() {
        try {
            var c = navigator.connection || navigator.mozConnection || navigator.webkitConnection;
            if (!c) return { kind: '', mbps: 0, rtt: -1, online: navigator.onLine !== false };
            var t = (c.type || c.effectiveType || '') + '';
            var kind = t.indexOf('wifi') >= 0 ? 'wifi' : (t.indexOf('ethernet') >= 0 ? 'cable' : 'net');
            var mbps = c.downlink > 0 ? Math.max(1, Math.round(c.downlink)) : 0;
            var rtt = c.rtt > 0 ? Math.round(c.rtt) : -1;
            return { kind: kind, mbps: mbps, rtt: rtt, online: navigator.onLine !== false };
        } catch (e1) {
            return { kind: '', mbps: 0, rtt: -1, online: navigator.onLine !== false };
        }
    }

    function kindTitle(panel, kind) {
        if (kind === 'cable') return attr(panel, 'data-lbl-cable', 'Cable');
        if (kind === 'net') return attr(panel, 'data-lbl-net', 'Red');
        return attr(panel, 'data-lbl-wifi', 'Wi‑Fi');
    }

    function whereLabel(panel, isLocal) {
        return isLocal
            ? attr(panel, 'data-lbl-local', 'En este ordenador')
            : attr(panel, 'data-lbl-remote', 'En internet');
    }

    function paintWifi(panel, serverNet) {
        var browser = browserNet();
        var nicMbps = pick(serverNet, 'mbps', 'Mbps') || 0;
        var nicKind = pick(serverNet, 'kind', 'Kind') || '';
        var kind = nicKind || browser.kind || 'wifi';
        var mbps = nicMbps > 0 ? nicMbps : browser.mbps;
        paintQuality(panel, 'wifi-quality', 'wifi-label', qualityFromMbps(mbps));
        text(panel.querySelector('[data-conn="wifi-type"]'), kindTitle(panel, kind));
        text(panel.querySelector('[data-conn="wifi-speed"]'), mbps > 0 ? mbps + ' Mb/s' : attr(panel, 'data-msg-fail'));
    }

    function showPanel() {
        var root = document.getElementById('desing2-stl-hover-right-panel');
        var sheet = root ? root.querySelector('.desing2-stl-hover-right-panel__sheet') : null;
        var placeholder = document.getElementById('desing2-stl-right-panel-placeholder');
        var panel = document.getElementById('desing2-connection-test-panel');
        var btn = document.getElementById('desing2-stl-right-panel-connection-test');
        if (placeholder) placeholder.classList.add('d-none');
        if (panel) {
            panel.classList.remove('d-none');
            panel.removeAttribute('hidden');
        }
        if (btn) btn.setAttribute('aria-pressed', 'true');
        if (sheet) {
            if (!sheet.hasAttribute('tabindex')) sheet.setAttribute('tabindex', '-1');
            try { sheet.focus(); } catch (e1) { }
        }
    }

    function runTest() {
        var btn = document.getElementById('desing2-stl-right-panel-connection-test');
        var panel = document.getElementById('desing2-connection-test-panel');
        if (!btn || !panel) return;
        var url = btn.getAttribute('data-conn-url');
        if (!url) return;
        showPanel();
        paintWifi(panel, null);
        paintQuality(panel, 'quality', 'quality-label', '');
        text(panel.querySelector('[data-conn="quality-label"]'), attr(panel, 'data-msg-measuring'));

        var t0 = Date.now();
        fetch(url + (url.indexOf('?') >= 0 ? '&' : '?') + 't=' + t0, {
            credentials: 'same-origin',
            headers: { 'Accept': 'application/json' },
            cache: 'no-store'
        }).then(function (r) {
            if (!r.ok) throw new Error('http');
            return r.json();
        }).then(function (data) {
            var httpMs = Date.now() - t0;
            var d = data || {};
            var sql = pick(d, 'data', 'Data') || {};
            var idn = pick(d, 'identity', 'Identity') || {};
            var list = pick(d, 'list', 'List') || {};
            var net = pick(d, 'net', 'Net') || {};
            var sqlMs = pick(sql, 'ms', 'Ms');
            var quality = pick(d, 'quality', 'Quality') || qualityFromMs(sqlMs);
            paintWifi(panel, net);
            paintQuality(panel, 'quality', 'quality-label', quality);
            text(panel.querySelector('[data-conn="host"]'), friendlyHost(pick(d, 'host', 'Host'), pick(d, 'appUrl', 'AppUrl')));
            text(panel.querySelector('[data-conn="machine"]'), pick(d, 'machineName', 'MachineName'));
            text(panel.querySelector('[data-conn="user"]'), pick(d, 'userName', 'UserName'));
            text(panel.querySelector('[data-conn="data-where"]'), whereLabel(panel, pick(sql, 'isLocal', 'IsLocal')));
            text(panel.querySelector('[data-conn="data-ms"]'), pick(sql, 'ok', 'Ok') ? friendlyTime(sqlMs) : attr(panel, 'data-msg-fail'));
            var idMs = pick(idn, 'ms', 'Ms');
            text(panel.querySelector('[data-conn="id-ms"]'), pick(idn, 'ok', 'Ok') ? friendlyTime(idMs) : attr(panel, 'data-msg-fail'));
            var listMs = pick(list, 'ms', 'Ms');
            text(panel.querySelector('[data-conn="list-ms"]'), pick(list, 'ok', 'Ok') ? friendlyTime(listMs) : attr(panel, 'data-msg-fail'));
            text(panel.querySelector('[data-conn="http-ms"]'), friendlyTime(httpMs));
        }).catch(function () {
            paintWifi(panel, null);
            paintQuality(panel, 'quality', 'quality-label', 'fail');
        });
    }

    function wire() {
        var btn = document.getElementById('desing2-stl-right-panel-connection-test');
        var retry = document.getElementById('desing2-connection-test-retry');
        if (!btn) return;
        btn.addEventListener('click', function (ev) {
            ev.preventDefault();
            runTest();
        });
        if (retry) {
            retry.addEventListener('click', function (ev) {
                ev.preventDefault();
                runTest();
            });
        }
    }

    if (document.readyState === 'loading')
        document.addEventListener('DOMContentLoaded', wire);
    else
        wire();
})();
