/**
 * Jobside: facturación = local y arranque de bloques de dirección.
 */
(function ($) {
  'use strict';

  function toggleBillSection() {
    var $checkbox = $('#BitBillSameAsLoc');
    var $bill = $('#billAddressSection');
    if (!$checkbox.length || !$bill.length) {
      return;
    }

    var same = $checkbox.is(':checked');

    $bill.toggle(!same);

    var $billBlock = $bill.find('.tandem-address-block');
    var $inputs = $bill.find('input, select, textarea');

    if (same) {
      $inputs.prop('disabled', true);
      if (window.TandemAddressPlaces && TandemAddressPlaces.clearMapState) {
        TandemAddressPlaces.clearMapState('Bill');
      }
    } else {
      $inputs.prop('disabled', false);
      $billBlock.find('.js-tandem-places-autocomplete').removeData('places-bound');
      if (window.TandemAddressPlaces && TandemAddressPlaces.initBlocks) {
        TandemAddressPlaces.initBlocks('#billAddressSection .tandem-address-block');
      }
    }
  }

  function runAfterAddressInit(done) {
    if (!window.TandemAddressPlaces || !TandemAddressPlaces.init) {
      done();
      return;
    }
    var result = TandemAddressPlaces.init();
    if (result && typeof result.then === 'function') {
      result.then(done, done);
    } else {
      done();
    }
  }

  $(function () {
    var $checkbox = $('#BitBillSameAsLoc');
    if ($checkbox.length) {
      $checkbox.on('change', toggleBillSection);
    }
    toggleBillSection();
    runAfterAddressInit(toggleBillSection);
  });
})(jQuery);
