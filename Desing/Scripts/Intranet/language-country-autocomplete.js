/**
 * País: autocompletar (≥3 caracteres, jQuery UI) + campo oculto #LinkCountry (long?).
 * Lista desplegable (Bootstrap 5): carga única desde catalogUrl opcional.
 *
 * Textos i18n: si el bloque del input País tiene atributo `data-i18n` (JSON
 * serializado en el partial), se usan esas cadenas para los placeholders del
 * desplegable ("Cargando…", "Sin países en catálogo.", error de carga).
 * Si no hay `data-i18n`, se usa el fallback español duro.
 */
(function ($) {
  'use strict';

  window.TandemLanguageCountryAutocomplete = window.TandemLanguageCountryAutocomplete || {};

  /**
   * Resuelve el bloque del input País (col-md-6 con data-i18n) leyendo el
   * elemento ascendente del input autocomplete. Devuelve un objeto con
   * { loading, empty, loadError } o cadenas vacías si no hay nada.
   */
  function readI18n($txt) {
    var fallback = {
      loading: 'Cargando…',
      empty: 'Sin países en catálogo.',
      loadError: 'No se pudo cargar el catálogo.'
    };

    if (!$txt || !$txt.length) return fallback;
    var $owner = $txt.closest('[data-i18n]');
    if (!$owner.length) return fallback;

    var raw = $owner.attr('data-i18n');
    if (!raw) return fallback;
    try {
      var parsed = JSON.parse(raw);
      return {
        loading: parsed.loading || fallback.loading,
        empty: parsed.empty || fallback.empty,
        loadError: parsed.loadError || fallback.loadError
      };
    } catch (e) {
      return fallback;
    }
  }

  window.TandemLanguageCountryAutocomplete.init = function (cfg) {
    if (!cfg || !cfg.searchUrl) return;

    var ns = '.tandemCountryPicker';
    var $hid = $('#LinkCountry');
    var $txt = $('#LanguageCountryAutocomplete');
    var $toggle = $('#languageCountryCatalogToggle');
    var $menu = $('#languageCountryCatalogMenu');
    var img = document.getElementById('languageCountryFlagPreview');
    var isoEl = document.getElementById('languageCountryIsoPreview');
    var catalogLoaded = false;
    var catalogLoading = false;
    var i18n = readI18n($txt);

    function applyPreview(flagUrl, iso2, iso3) {
      if (!img || !isoEl) return;
      if (flagUrl) {
        img.src = flagUrl;
        img.classList.remove('d-none');
      } else {
        img.removeAttribute('src');
        img.classList.add('d-none');
      }
      var parts = [];
      if (iso2) parts.push(iso2);
      if (iso3) parts.push(iso3);
      isoEl.textContent = parts.length ? parts.join(' · ') : '';
    }

    function applyBootstrap(b) {
      var id = b && (b.Id != null ? b.Id : undefined);
      if (id !== undefined && id !== null && id !== '') {
        $hid.val(String(id));
        $txt.val(b.Label || '');
        applyPreview(b.FlagUrl || null, b.Iso2 || '', b.Iso3 || '');
      } else {
        $hid.val('');
        $txt.val('');
        applyPreview(null, '', '');
      }
    }

    function applySelection(it) {
      if (!it) return;
      var id = it.id != null ? it.id : it.IdObject;
      if (id === undefined || id === null) return;
      $hid.val(String(id));
      var label = it.label != null ? it.label : it.TextLabel || '';
      $txt.val(label);
      applyPreview(it.flagUrl || null, it.iso2 || '', it.iso3 || '');
    }

    function closeCatalogDropdown() {
      if (typeof bootstrap === 'undefined' || !$toggle.length || !$toggle[0]) return;
      if (!bootstrap.Dropdown) return;
      var inst = bootstrap.Dropdown.getInstance($toggle[0]);
      if (inst) inst.hide();
    }

    applyBootstrap(cfg.bootstrap || {});

    $txt.autocomplete({
      minLength: 3,
      delay: 200,
      appendTo: $('body'),
      source: function (request, response) {
        $.ajax({
          url: cfg.searchUrl,
          dataType: 'json',
          cache: false,
          data: { q: $.trim(request.term) },
          success: function (data) {
            response($.isArray(data) ? data : []);
          },
          error: function () {
            response([]);
          }
        });
      },
      select: function (event, ui) {
        event.preventDefault();
        if (!ui.item) return;
        applySelection(ui.item);
      },
      focus: function (event) {
        event.preventDefault();
      }
    });

    $txt.on('blur' + ns, function () {
      window.setTimeout(function () {
        if (!$.trim($txt.val())) {
          $hid.val('');
          applyPreview(null, '', '');
        }
      }, 250);
    });

    /* --- Catálogo (primera apertura) --- */
    if (cfg.catalogUrl && $toggle.length && $menu.length) {
      $toggle.on('show.bs.dropdown', function () {
        if (catalogLoaded || catalogLoading) return;
        catalogLoading = true;
        $menu.empty().append(
          $('<li/>').append(
            $('<span class="dropdown-item disabled small py-3"/>').text(i18n.loading)
          )
        );
        $.ajax({
          url: cfg.catalogUrl,
          dataType: 'json',
          cache: true,
          data: { take: cfg.catalogTake != null ? cfg.catalogTake : 500 }
        })
          .done(function (data) {
            catalogLoaded = true;
            var arr = $.isArray(data) ? data : [];
            $menu.empty();
            if (!arr.length) {
              $menu.append(
                $('<li/>').append(
                  $('<span class="dropdown-item disabled small py-3"/>').text(i18n.empty)
                )
              );
              return;
            }
            for (var i = 0; i < arr.length; i++) {
              (function (it) {
                var $btn = $(
                  '<button type="button" class="dropdown-item d-flex align-items-center gap-2 tandem-country-catalog-item py-2" role="menuitem"/>'
                );
                var flagSrc = it.flagUrl;
                if (flagSrc) {
                  $('<img>', {
                    src: flagSrc,
                    alt: '',
                    width: 28,
                    height: 20,
                    class: 'rounded border flex-shrink-0'
                  })
                    .css('object-fit', 'cover')
                    .on('error', function () {
                      $(this).remove();
                    })
                    .appendTo($btn);
                }
                $('<span />')
                  .addClass('text-truncate flex-grow-1 text-start')
                  .text(it.label || '')
                  .appendTo($btn);
                $btn.on('mousedown', function (ev) { ev.preventDefault(); });
                $btn.on('click', function (ev) {
                  ev.preventDefault();
                  applySelection(it);
                  closeCatalogDropdown();
                });
                $menu.append($('<li class="py-0" />').append($btn));
              })(arr[i]);
            }
          })
          .fail(function () {
            $menu.empty().append(
              $('<li/>').append(
                $('<span class="dropdown-item disabled small text-danger py-3"/>').text(i18n.loadError)
              )
            );
          })
          .always(function () {
            catalogLoading = false;
          });
      });

      $(document).on('keydown' + ns, function (e) {
        if (e.key !== 'Escape') return;
        if (!$menu.hasClass('show')) return;
        closeCatalogDropdown();
      });
    }
  };
})(jQuery);
