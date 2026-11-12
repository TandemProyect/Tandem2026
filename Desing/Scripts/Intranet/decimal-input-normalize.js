/**
 * Desing — decimales en formularios: coma → punto.
 *
 * La base de datos guarda los decimales con punto. Este script (incluido en todos los layouts)
 * convierte la coma decimal en punto al salir del campo (blur/change), al enviar el formulario
 * (también con Enter sin salir del campo) y al cargar la página (valores pintados por el servidor
 * con cultura es-ES, p. ej. "2,7").
 *
 * Campos afectados (opt-in, nunca todos los inputs de texto):
 *   - inputs con `data-val-number` (propiedades numéricas MVC: decimal/double/float…);
 *   - inputs con `data-decimal-input` (o clase `js-decimal-input`) para campos manuales/dinámicos.
 *   Exclusión explícita: `data-decimal-input="false"`. Se ignoran type=number/date/checkbox/etc.
 *
 * Regla: sin separador de miles. Si hay '.' y ',' el último es el decimal y el otro se quita.
 * Valores que no son un número (letras, varias comas…) se dejan tal cual para que la validación avise.
 *
 * jQuery Validate: `number`, `range`, `min`, `max` aceptan coma y punto (se normaliza antes de validar).
 */
(function (window, document) {
    'use strict';

    var SELECTOR = 'input[data-val-number], input[data-decimal-input], input.js-decimal-input';
    var TEXT_TYPES = { '': true, text: true, search: true, tel: true, hidden: true };

    /**
     * @param {string} value
     * @returns {string} valor con punto decimal (o el original si no es un número)
     */
    function normalizeText(value) {
        if (value == null) return value;
        var raw = String(value);
        var s = raw.trim();
        if (!s || s.indexOf(',') < 0) return raw;
        s = s.replace(/[\s\u00a0\u202f]/g, '');
        if (!/^[+-]?[0-9.,]+$/.test(s)) return raw;
        var lastComma = s.lastIndexOf(',');
        var lastDot = s.lastIndexOf('.');
        var out;
        if (lastDot >= 0) {
            if (lastComma > lastDot) {
                out = s.replace(/\./g, '').replace(',', '.');
            } else {
                out = s.replace(/,/g, '');
            }
        } else {
            if (lastComma !== s.indexOf(',')) return raw;
            out = s.replace(',', '.');
        }
        out = out.replace(/\.$/, '').replace(/^([+-]?)\./, '$10.');
        if (!/^[+-]?\d+(\.\d+)?$/.test(out)) return raw;
        return out;
    }

    /** @param {any} el */
    function isDecimalInput(el) {
        if (!el || el.tagName !== 'INPUT') return false;
        if (el.disabled) return false;
        var type = String(el.getAttribute('type') || '').toLowerCase();
        if (!TEXT_TYPES[type]) return false;
        if (el.getAttribute('data-decimal-input') === 'false') return false;
        var matches = el.matches || el.msMatchesSelector || el.webkitMatchesSelector;
        return !!(matches && matches.call(el, SELECTOR));
    }

    /**
     * @param {any} el
     * @returns {boolean} true si cambió el valor
     */
    function normalizeInput(el) {
        if (!isDecimalInput(el)) return false;
        var before = el.value;
        var after = normalizeText(before);
        if (after === before) return false;
        el.value = after;
        return true;
    }

    /** @param {ParentNode=} root */
    function normalizeWithin(root) {
        var scope = root && root.querySelectorAll ? root : document;
        var list = scope.querySelectorAll(SELECTOR);
        for (var i = 0; i < list.length; i++) normalizeInput(list[i]);
    }

    /** Captura: corre antes que los manejadores de jQuery Validate / de la página. */
    function onLeave(ev) {
        normalizeInput(ev.target);
    }

    function onSubmit(ev) {
        var form = ev.target;
        if (form && form.tagName === 'FORM') normalizeWithin(form);
    }

    function patchValidator() {
        var $ = window.jQuery;
        if (!$ || !$.validator || !$.validator.methods) return false;
        var methods = $.validator.methods;
        if (methods.__desingDecimalPatched) return true;
        ['number', 'range', 'min', 'max'].forEach(function (name) {
            var original = methods[name];
            if (typeof original !== 'function') return;
            methods[name] = function (value, element, param) {
                var v = isDecimalInput(element) ? normalizeText(value) : value;
                return original.call(this, v, element, param);
            };
        });
        methods.__desingDecimalPatched = true;
        return true;
    }

    document.addEventListener('focusout', onLeave, true);
    document.addEventListener('change', onLeave, true);
    document.addEventListener('submit', onSubmit, true);

    function onReady() {
        normalizeWithin(document);
        patchValidator();
    }

    patchValidator();
    if (document.readyState === 'loading') {
        document.addEventListener('DOMContentLoaded', onReady);
    } else {
        onReady();
    }
    /* jquery.validate suele cargarse en @section scripts (después de este archivo). */
    window.addEventListener('load', patchValidator);

    window.DesingDecimalInput = {
        normalizeText: normalizeText,
        normalizeInput: normalizeInput,
        normalizeWithin: normalizeWithin,
        patchValidator: patchValidator,
    };
})(window, document);
