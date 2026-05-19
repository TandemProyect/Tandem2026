/**
 * Comportamiento del formulario Crear / Editar Plantilla:
 *  - Sincroniza pickers de color con sus inputs HEX.
 *  - Genera la vista previa de marca (primera letra acento + resto).
 *  - Lee tamano de archivos (logo / favicon) y muestra previa.
 *
 * Las cadenas de UI vienen del objeto global window.plantillaFormDt
 * (serializado desde el .cshtml con Plantilla.* del .resx).
 */
(function ($) {
    'use strict';

    if (!$ || !$.fn) return;

    $(function () {
        var i18n = window.plantillaFormDt || {};
        var fileTooLargeLogo = i18n.fileTooLargeLogo || 'File exceeds 2 MB.';
        var fileTooLargeFavicon = i18n.fileTooLargeFavicon || 'Favicon exceeds 512 KB.';

        var $picker = $('#AttColorPicker');
        var $text = $('#AttColorText');
        var $previewBox = $('#PlantillaPreview');
        var $previewTxt = $('#PlantillaPreviewColorTxt');
        var $previewLogo = $('#PlantillaPreviewLogo');
        var $logo = $('#AttLogo');

        function applyMainColor(c) {
            $previewBox.css('background', c);
            $previewTxt.text(c);
        }

        $picker.on('input change', function () {
            $text.val(this.value);
            applyMainColor(this.value);
        });
        $text.on('input change', function () {
            var v = this.value;
            if (/^#([A-Fa-f0-9]{6}|[A-Fa-f0-9]{3})$/.test(v)) {
                $picker.val(v);
                applyMainColor(v);
            }
        });

        function syncHexPair(pickerSel, inputSel) {
            var $p = $(pickerSel), $i = $(inputSel);
            $p.on('input change', function () { $i.val(this.value); applyBrandPreview(); });
            $i.on('input change', function () {
                var v = this.value;
                if (/^#([A-Fa-f0-9]{6}|[A-Fa-f0-9]{3})$/.test(v)) { $p.val(v); }
                applyBrandPreview();
            });
        }
        syncHexPair('#AttBrandAccentPicker', '#AttBrandAccentColor');
        syncHexPair('#AttBrandTextColorPicker', '#AttBrandTextColor');

        function applyBrandPreview() {
            var full = ($('#AttBrandText').val() || 'T Desing.net').trim();
            if (!full.length) full = 'T Desing.net';
            var first = full.charAt(0);
            var rest = full.length > 1 ? full.substring(1) : '';
            $('#pvFirst').text(first).css('color', $('#AttBrandAccentColor').val() || '#f29100');
            var tc = ($('#AttBrandTextColor').val() || '').trim();
            if (!tc) {
                $('#pvRest').text(rest).attr('style', '');
            } else {
                $('#pvRest').text(rest).attr('style', 'color:' + tc + ';');
            }
        }
        $('#AttBrandText').on('input change', applyBrandPreview);
        applyBrandPreview();

        $logo.on('input change', function () {
            var v = this.value;
            if (v && v.length > 0) {
                var src = v.indexOf('http') === 0 ? v : (v.charAt(0) === '/' ? v : '/' + v);
                $previewLogo.attr('src', src);
            }
        });

        $('#logoFile').on('change', function () {
            var file = this.files && this.files[0];
            if (!file) return;
            if (file.size > 2 * 1024 * 1024) {
                alert(fileTooLargeLogo);
                this.value = '';
                return;
            }
            var reader = new FileReader();
            reader.onload = function (e) { $previewLogo.attr('src', e.target.result); };
            reader.readAsDataURL(file);
        });

        var $previewFavicon = $('#PlantillaFaviconPreview');
        $('#faviconFile').on('change', function () {
            var file = this.files && this.files[0];
            if (!file) return;
            if (file.size > 512 * 1024) {
                alert(fileTooLargeFavicon);
                this.value = '';
                return;
            }
            var reader = new FileReader();
            reader.onload = function (e) { $previewFavicon.attr('src', e.target.result); };
            reader.readAsDataURL(file);
        });

        $('#frmPlantilla').on('submit', function () {
            var tc = ($('#AttBrandTextColor').val() || '').trim();
            if (tc === '#ffffff' || tc === '#fff') {
                $('#AttBrandTextColor').val('');
            }
        });
    });
}(window.jQuery));
