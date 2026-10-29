/* Exporta e importa muros Desing_2 en un archivo cifrado (.tnwall).
   La clave está en este fichero y en IA/tools/decrypt-wall-export.mjs
   para poder leer el archivo cuando haya que revisar un diseño. */
(function () {
    'use strict';

    var PASSPHRASE = 'Tandem2026.WallExchange.v1';
    var HEADER = 'TNWALL1\n';

    function toast(message, isError) {
        if (typeof window.maStlDesing2ShowSaveViewToast === 'function') {
            window.maStlDesing2ShowSaveViewToast(message, !!isError);
            return;
        }
        if (isError) window.alert(message);
    }

    function bytesToBase64(bytes) {
        var bin = '';
        var chunk = 0x8000;
        for (var i = 0; i < bytes.length; i += chunk) {
            bin += String.fromCharCode.apply(null, bytes.subarray(i, i + chunk));
        }
        return btoa(bin);
    }

    function base64ToBytes(text) {
        var bin = atob(String(text || '').replace(/\s+/g, ''));
        var out = new Uint8Array(bin.length);
        for (var i = 0; i < bin.length; i++) out[i] = bin.charCodeAt(i);
        return out;
    }

    function aesKey() {
        return crypto.subtle.digest('SHA-256', new TextEncoder().encode(PASSPHRASE)).then(function (digest) {
            return crypto.subtle.importKey('raw', digest, { name: 'AES-GCM' }, false, ['encrypt', 'decrypt']);
        });
    }

    function encryptJson(obj) {
        var plain = new TextEncoder().encode(JSON.stringify(obj));
        var iv = crypto.getRandomValues(new Uint8Array(12));
        return aesKey().then(function (key) {
            return crypto.subtle.encrypt({ name: 'AES-GCM', iv: iv }, key, plain).then(function (cipher) {
                var body = new Uint8Array(cipher);
                var packed = new Uint8Array(iv.length + body.length);
                packed.set(iv, 0);
                packed.set(body, iv.length);
                return HEADER + bytesToBase64(packed);
            });
        });
    }

    function decryptText(fileText) {
        var raw = String(fileText || '').replace(/^\uFEFF/, '');
        if (raw.indexOf('TNWALL1') !== 0) {
            return Promise.reject(new Error('format'));
        }
        var b64 = raw.replace(/^TNWALL1\s*/, '');
        var packed = base64ToBytes(b64);
        if (packed.length < 12 + 16) return Promise.reject(new Error('format'));
        var iv = packed.subarray(0, 12);
        var cipher = packed.subarray(12);
        return aesKey().then(function (key) {
            return crypto.subtle.decrypt({ name: 'AES-GCM', iv: iv }, key, cipher);
        }).then(function (plain) {
            return JSON.parse(new TextDecoder().decode(plain));
        });
    }

    function download(text, designId) {
        var blob = new Blob([text], { type: 'text/plain;charset=utf-8' });
        var url = URL.createObjectURL(blob);
        var a = document.createElement('a');
        var stamp = new Date();
        function two(n) { return n < 10 ? '0' + n : String(n); }
        var name = 'muros-' + (designId || 'diseno') + '-'
            + stamp.getFullYear()
            + two(stamp.getMonth() + 1)
            + two(stamp.getDate())
            + '-'
            + two(stamp.getHours())
            + two(stamp.getMinutes())
            + '.tnwall';
        a.href = url;
        a.download = name;
        document.body.appendChild(a);
        a.click();
        a.remove();
        setTimeout(function () { URL.revokeObjectURL(url); }, 1000);
    }

    function currentDesignId() {
        var shell = document.getElementById('ma-stl-viewer-shell');
        var raw = shell ? shell.getAttribute('data-ma-stl-design-id') : '';
        var n = Number(raw);
        if (Number.isFinite(n) && n > 0) return n;
        try {
            var q = new URLSearchParams(window.location.search);
            var fromQuery = Number(q.get('designId') || '');
            if (Number.isFinite(fromQuery) && fromQuery > 0) return fromQuery;
        } catch (_e) { /* ignore */ }
        return 0;
    }

    function onExport(btn) {
        if (typeof window.maStlDesing2BuildWallExchangePayload !== 'function') {
            toast(btn.getAttribute('data-ma-export-fail') || 'No se pudo leer el archivo de muros', true);
            return;
        }
        var payload;
        try {
            payload = window.maStlDesing2BuildWallExchangePayload();
        } catch (err) {
            toast(btn.getAttribute('data-ma-export-fail') || 'No se pudo leer el archivo de muros', true);
            return;
        }
        encryptJson(payload).then(function (text) {
            download(text, payload.designId || currentDesignId());
            toast(btn.getAttribute('data-ma-export-ok') || 'Muros exportados', false);
        }).catch(function () {
            toast(btn.getAttribute('data-ma-export-fail') || 'No se pudo leer el archivo de muros', true);
        });
    }

    function applyImported(payload, btn) {
        var designId = currentDesignId();
        if (!designId) {
            toast(btn.getAttribute('data-ma-import-fail') || 'No se pudo leer el archivo de muros', true);
            return;
        }
        var url = btn.getAttribute('data-ma-import-url') || '';
        if (!url) {
            toast(btn.getAttribute('data-ma-import-fail') || 'No se pudo leer el archivo de muros', true);
            return;
        }
        var body = {
            DesignId: designId,
            SourceDesignId: Number(payload.designId) || 0,
            Walls: payload.walls || [],
            SpecialArticles: payload.specialArticles || []
        };
        fetch(url, {
            method: 'POST',
            headers: { 'Content-Type': 'application/json', Accept: 'application/json' },
            credentials: 'same-origin',
            body: JSON.stringify(body)
        }).then(function (res) {
            if (!res.ok) throw new Error('HTTP ' + res.status);
            return res.json();
        }).then(function (resp) {
            if (!resp || resp.Exito !== true) {
                throw new Error((resp && resp.Mensaje) || 'import');
            }
            var reload = typeof window.maStlDesing2LoadDesignWallsFromServer === 'function'
                ? window.maStlDesing2LoadDesignWallsFromServer()
                : Promise.resolve();
            return Promise.resolve(reload).then(function () {
                if (payload.view && typeof window.maStlApplyDesing2ViewerStateFromCookie === 'function') {
                    window.maStlApplyDesing2ViewerStateFromCookie(payload.view);
                }
                toast(btn.getAttribute('data-ma-import-ok') || 'Muros importados', false);
            });
        }).catch(function (err) {
            var message = err && err.message && err.message !== 'import'
                ? err.message
                : (btn.getAttribute('data-ma-import-fail') || 'No se pudo leer el archivo de muros');
            toast(message, true);
        });
    }

    function onImportFile(file, btn) {
        if (!file) return;
        var reader = new FileReader();
        reader.onload = function () {
            decryptText(reader.result).then(function (payload) {
                if (!payload || payload.kind !== 'tandem-walls' || !Array.isArray(payload.walls)) {
                    throw new Error('format');
                }
                applyImported(payload, btn);
            }).catch(function () {
                toast(btn.getAttribute('data-ma-import-fail') || 'No se pudo leer el archivo de muros', true);
            });
        };
        reader.onerror = function () {
            toast(btn.getAttribute('data-ma-import-fail') || 'No se pudo leer el archivo de muros', true);
        };
        reader.readAsText(file);
    }

    function wire() {
        var exportBtn = document.getElementById('ma-stl-tool-wall-export');
        var importBtn = document.getElementById('ma-stl-tool-wall-import');
        var fileInput = document.getElementById('ma-stl-tool-wall-import-file');
        if (!exportBtn || !importBtn || !fileInput) return;
        exportBtn.addEventListener('click', function (ev) {
            ev.preventDefault();
            ev.stopPropagation();
            onExport(exportBtn);
        });
        importBtn.addEventListener('click', function (ev) {
            ev.preventDefault();
            ev.stopPropagation();
            var confirmText = importBtn.getAttribute('data-ma-import-confirm') || '';
            if (confirmText && !window.confirm(confirmText)) return;
            fileInput.value = '';
            fileInput.click();
        });
        fileInput.addEventListener('change', function () {
            var file = fileInput.files && fileInput.files[0];
            onImportFile(file, importBtn);
        });
    }

    if (document.readyState === 'loading') {
        document.addEventListener('DOMContentLoaded', wire);
    } else {
        wire();
    }
})();
