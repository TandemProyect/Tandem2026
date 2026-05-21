/**
 * Cliente (TSql_Client_V2): inicializa bloque de dirección Google Places (prefijo Loc).
 */
(function ($) {
  'use strict';

  $(function () {
    if (!window.TandemAddressPlaces || !TandemAddressPlaces.init) {
      return;
    }
    var r = TandemAddressPlaces.init();
    if (r && typeof r.then === 'function') {
      r.catch(function () {});
    }
  });
})(jQuery);
