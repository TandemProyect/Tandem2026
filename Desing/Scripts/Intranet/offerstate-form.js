/**
 * Estados de oferta: sincroniza el selector nativo de color con el campo HEX (#RGB / #RRGGBB).
 * Mismo patrón que plantilla-form.js (color principal).
 */
(function ($) {
    'use strict';

    if (!$ || !$.fn) return;

    function hexOk(v) {
        return /^#([A-Fa-f0-9]{6}|[A-Fa-f0-9]{3})$/.test((v || '').trim());
    }

    $(function () {
        var $picker = $('#OfferStateColorPicker');
        var $text = $('#OfferStateColorText');
        if (!$picker.length || !$text.length) return;

        $picker.on('input change', function () {
            $text.val(this.value);
        });
        $text.on('input change', function () {
            var v = (this.value || '').trim();
            if (hexOk(v)) {
                $picker.val(v);
            }
        });
    });
}(window.jQuery));
