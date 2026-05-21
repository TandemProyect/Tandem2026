/**

 * Autocompletado Google Places + vista previa de mapa (reutilizable en formularios).

 * Config: window.TandemAddressPlacesConfig { apiKey, language, region }

 *

 * Predicciones: PlaceAutocompleteElement (Places API New). El Autocomplete legacy

 * no está disponible para proyectos/clientes nuevos en Google Cloud.

 *

 * Consola: Maps JavaScript API + Places API (New).

 */


(function (window) {

  'use strict';



  var ns = window.TandemAddressPlaces = window.TandemAddressPlaces || {};

  var CREDENTIALS_URL = 'https://console.cloud.google.com/apis/credentials';

  var MAPS_JS_LOAD_TIMEOUT_MS = 20000;



  var MSG = {

    noKey: 'Google Maps no está configurado. Añada <code>GoogleMaps:ApiKey</code> en Web.config. Puede rellenar la dirección manualmente.',

    loadFailed: 'No se pudo cargar el script de Google Maps. Compruebe la conexión y la clave API en Web.config.',

    authFailed: 'Google Maps rechazó la clave API (autenticación fallida). Revise la clave en Web.config, facturación, APIs habilitadas y referentes HTTP. Puede rellenar la dirección manualmente.',

    mapUnavailable: 'Mapa no disponible (error de Google Maps).'

  };



  var mapsLoadPromise = null;

  var mapState = {};

  var mapsAuthFailed = false;

  var lastAuthFailureDetail = null;



  function getConfig() {

    return window.TandemAddressPlacesConfig || {};

  }



  function hasApiKey() {

    var key = getConfig().apiKey;

    return key && String(key).trim().length > 0;

  }



  function mapsUsable() {

    return hasApiKey() && !mapsAuthFailed &&

      window.google && window.google.maps && window.google.maps.places;

  }



  function getPageOrigin() {

    try {

      return window.location.origin || (window.location.protocol + '//' + window.location.host);

    } catch (e) {

      return '';

    }

  }



  function getReferrerPatterns(origin) {

    if (!origin) {

      return [];

    }

    return [

      origin + '/*',

      origin.replace(/^https:/i, 'http:') + '/*'

    ];

  }



  function logMapsAuthFailure(reason, extra) {

    var origin = getPageOrigin();

    var keyLen = hasApiKey() ? String(getConfig().apiKey).trim().length : 0;

    var detail = {

      reason: reason || 'gm_authFailure',

      origin: origin,

      referrerToAdd: getReferrerPatterns(origin)[0] || null,

      apiKeyLength: keyLen,

      href: window.location.href,

      extra: extra || null

    };

    lastAuthFailureDetail = detail;

    console.error(

      '[TandemAddressPlaces] Google Maps authentication failed.',

      detail.reason + '.',

      'Add HTTP referrer:', detail.referrerToAdd + '.',

      'Enable Maps JavaScript API + Places API (New).',

      'Credentials:', CREDENTIALS_URL,

      detail

    );

  }



  function installAuthFailureHandlerEarly() {

    if (window._tandemGmAuthHandlerInstalled) {

      return;

    }

    window._tandemGmAuthHandlerInstalled = true;

    window.gm_authFailure = function () {

      logMapsAuthFailure('gm_authFailure (Google invocó gm_authFailure: clave, referente, facturación o APIs)');

      if (window.TandemAddressPlaces && window.TandemAddressPlaces._onAuthFailure) {

        window.TandemAddressPlaces._onAuthFailure();

      }

    };

  }

  installAuthFailureHandlerEarly();



  function markAuthFailed(reason, extra) {

    if (!mapsAuthFailed) {

      logMapsAuthFailure(reason || 'auth-failed', extra);

    }

    mapsAuthFailed = true;

    mapsLoadPromise = null;

    if (document.body) {

      document.body.classList.add('tandem-maps-auth-failed');

    }

  }



  function installAuthFailureHandler() {

    installAuthFailureHandlerEarly();

  }



  function showApiWarning(htmlMessage, asError) {

    var $w = $('#tandemAddressApiWarning');

    if (!$w.length) {

      return;

    }



    if (htmlMessage) {

      var $text = $w.find('.js-tandem-maps-warning-text');

      if ($text.length) {

        $text.html(htmlMessage);

      } else {

        $w.html(htmlMessage);

      }

    }

    $w.removeClass('d-none').toggleClass('alert-warning', asError !== false).toggleClass('alert-info', asError === false);

    $('.js-tandem-address-maps-error').each(function () {

      $(this).html(htmlMessage || MSG.authFailed).removeClass('d-none');

    });

  }



  function hideBlockMapsErrors() {

    $('.js-tandem-address-maps-error').addClass('d-none').empty();

    $('#tandemAddressApiWarning .js-tandem-maps-runtime-warning').remove();

  }



  ns._onAuthFailure = function () {

    markAuthFailed('gm_authFailure');

    showApiWarning(MSG.authFailed);

  };



  function fieldId(prefix, suffix) {

    return prefix + '_' + suffix;

  }



  function setField(prefix, suffix, value) {

    var el = document.getElementById(fieldId(prefix, suffix));

    if (el) {

      el.value = value == null ? '' : String(value);

    }

  }



  function getField(prefix, suffix) {

    var el = document.getElementById(fieldId(prefix, suffix));

    return el ? el.value : '';

  }



  function findComponent(components, type) {

    if (!components) return null;

    for (var i = 0; i < components.length; i++) {

      if (components[i].types && components[i].types.indexOf(type) >= 0) {

        return components[i];

      }

    }

    return null;

  }



  function componentValue(components, type, useShort) {

    var c = findComponent(components, type);

    if (!c) return '';

    return useShort ? (c.short_name || '') : (c.long_name || '');

  }



  function parseLatLng(prefix) {

    var lat = parseFloat(getField(prefix, 'Lat'));

    var lng = parseFloat(getField(prefix, 'Lng'));

    if (isNaN(lat) || isNaN(lng)) return null;

    return { lat: lat, lng: lng };

  }



  function setMapEmpty(mapId, message) {

    var el = document.getElementById(mapId);

    if (!el) return;

    el.classList.add('is-empty');

    el.innerHTML = '<span>' + (message || 'Seleccione una dirección para ver el mapa') + '</span>';

  }



  function clearMapState(prefix) {

    if (mapState[prefix]) {

      mapState[prefix] = null;

    }

  }



  function updateMap(prefix, mapId, latLng) {

    var el = document.getElementById(mapId);

    if (!el || !latLng) {

      setMapEmpty(mapId);

      return;

    }



    if (!mapsUsable()) {

      setMapEmpty(mapId, mapsAuthFailed ? MSG.mapUnavailable : 'Mapa no disponible');

      return;

    }



    el.classList.remove('is-empty');

    el.innerHTML = '';



    var center = { lat: latLng.lat, lng: latLng.lng };

    var state = mapState[prefix];



    try {

      if (!state || !state.map) {

        state = {

          map: new google.maps.Map(el, {

            center: center,

            zoom: 16,

            /* Evita capturar la rueda del raton sobre el preview: el usuario puede subir/bajar la pagina. */
            gestureHandling: 'cooperative',

            mapTypeControl: false,

            streetViewControl: false,

            fullscreenControl: true

          }),

          marker: null

        };

        mapState[prefix] = state;

      } else {

        state.map.setCenter(center);

      }



      if (!state.marker) {

        state.marker = new google.maps.Marker({

          map: state.map,

          position: center

        });

      } else {

        state.marker.setPosition(center);

        state.marker.setMap(state.map);

      }



      state.map.setZoom(16);

    } catch (e) {

      markAuthFailed('map-constructor', { message: e && e.message ? e.message : String(e) });

      showApiWarning(MSG.authFailed);

      setMapEmpty(mapId, MSG.mapUnavailable);

    }

  }



  function normalizeNewPlaceAddressComponents(arr) {

    if (!arr || !arr.length) return [];

    var out = [];

    for (var i = 0; i < arr.length; i++) {

      var ac = arr[i];

      out.push({

        long_name: ac.longText || ac.long_name || '',

        short_name: ac.shortText || ac.short_name || '',

        types: ac.types || []

      });

    }

    return out;

  }



  function applyParsedPlaceToForm(prefix, mapId, formatted, placeId, lat, lng, componentsForJson) {

    var components = componentsForJson || [];



    setField(prefix, 'Place_Id', placeId || '');

    setField(prefix, 'Formatted_Address', formatted || '');

    setField(prefix, 'Lat', lat != null ? lat : '');

    setField(prefix, 'Lng', lng != null ? lng : '');

    setField(prefix, 'Street_Number', componentValue(components, 'street_number', true));

    setField(prefix, 'Route', componentValue(components, 'route', false));

    setField(prefix, 'Subpremise', componentValue(components, 'subpremise', false));

    setField(prefix, 'Locality', componentValue(components, 'locality', false));

    setField(prefix, 'Admin_Area_1', componentValue(components, 'administrative_area_level_1', false));

    setField(prefix, 'Admin_Area_2', componentValue(components, 'administrative_area_level_2', false));

    setField(prefix, 'Postal_Code', componentValue(components, 'postal_code', true));

    setField(prefix, 'Country_Code', componentValue(components, 'country', true));

    setField(prefix, 'Country_Name', componentValue(components, 'country', false));



    try {

      setField(prefix, 'Address_Components_Json', JSON.stringify(components));

    } catch (e) {

      setField(prefix, 'Address_Components_Json', '');

    }



    var $block = $('.tandem-address-block[data-prefix="' + prefix + '"]');

    var $searchLegacy = $block.find('.js-tandem-places-autocomplete');

    if ($searchLegacy.length && formatted) {

      $searchLegacy.val(formatted);

    }

    var hostEl = $block.find('.js-tandem-place-autocomplete-host')[0];

    if (hostEl && formatted && hostEl.firstElementChild && 'value' in hostEl.firstElementChild) {

      try {

        hostEl.firstElementChild.value = formatted;

      } catch (e1) { /* ignore */ }

    }



    if (lat != null && lng != null) {

      updateMap(prefix, mapId, { lat: lat, lng: lng });

    } else {

      setMapEmpty(mapId);

    }

  }



  function populateFromPlace(prefix, mapId, place) {

    if (!place) return;



    var formatted = place.formatted_address || '';

    var components = place.address_components || [];

    var placeId = place.place_id || '';

    var lat = null;

    var lng = null;



    if (place.geometry && place.geometry.location) {

      lat = typeof place.geometry.location.lat === 'function'

        ? place.geometry.location.lat()

        : place.geometry.location.lat;

      lng = typeof place.geometry.location.lng === 'function'

        ? place.geometry.location.lng()

        : place.geometry.location.lng;

    }



    applyParsedPlaceToForm(prefix, mapId, formatted, placeId, lat, lng, components);

  }



  function populateFromNewPlace(prefix, mapId, place) {

    if (!place) return;



    var formatted = place.formattedAddress || '';

    var placeId = place.id || '';

    var lat = null;

    var lng = null;

    var loc = place.location;



    if (loc) {

      lat = typeof loc.lat === 'function' ? loc.lat() : loc.lat;

      lng = typeof loc.lng === 'function' ? loc.lng() : loc.lng;

    }



    var norm = normalizeNewPlaceAddressComponents(place.addressComponents || []);

    applyParsedPlaceToForm(prefix, mapId, formatted, placeId, lat, lng, norm);

  }



  /** PlaceAutocompleteElement (Places API New) en .js-tandem-place-autocomplete-host */

  function bindPlaceAutocompleteHost($block) {

    var prefix = $block.data('prefix');

    var mapId = $block.data('map-id');

    var $hostEl = $block.find('.js-tandem-place-autocomplete-host');



    if (!$hostEl.length || $hostEl.data('places-bound')) {

      return;

    }



    var placeholder = $hostEl.data('placeholder');

    placeholder = placeholder != null ? String(placeholder) : '';



    function mountFallbackTextInput() {

      $hostEl.data('places-bound', true);

      var manual = document.createElement('input');

      manual.type = 'text';

      manual.className = 'form-control form-control-sm';

      if (placeholder) manual.setAttribute('placeholder', placeholder);

      manual.addEventListener('change', function () {

        setField(prefix, 'Formatted_Address', manual.value);

      });

      manual.addEventListener('blur', function () {

        setField(prefix, 'Formatted_Address', manual.value);

      });

      $hostEl.empty().append(manual);

    }



    function mountLegacyAutocompleteInput() {

      $hostEl.data('places-bound', true);

      var legacy = document.createElement('input');

      legacy.type = 'text';

      legacy.className = 'form-control form-control-sm js-tandem-places-autocomplete';

      if (placeholder) legacy.setAttribute('placeholder', placeholder);

      $hostEl.empty().append(legacy);

      bindLegacyPlacesAutocomplete($block);

    }



    if (!hasApiKey() || mapsAuthFailed) {

      mountFallbackTextInput();

      return;

    }



    if (!window.google || !window.google.maps || typeof google.maps.importLibrary !== 'function') {

      mountLegacyAutocompleteInput();

      return;

    }



    google.maps.importLibrary('places').then(function (placesLib) {

      var PlaceAutocompleteElement = placesLib.PlaceAutocompleteElement;

      if (!PlaceAutocompleteElement) {

        mountLegacyAutocompleteInput();

        return;

      }



      var pacOptions = {};

      var cfgRegion = getConfig().region;

      if (cfgRegion) {

        pacOptions.includedRegionCodes = [String(cfgRegion).toLowerCase()];

      }

      var pac = new PlaceAutocompleteElement(pacOptions);

      if (placeholder) {

        try {

          pac.placeholder = placeholder;

        } catch (ePh) {

          if (pac.setAttribute) pac.setAttribute('placeholder', placeholder);

        }

      }



      $hostEl.empty().append(pac);

      $hostEl.data('places-bound', true);



      pac.addEventListener('gmp-select', function (event) {

        var pp = event.placePrediction;

        if (!pp) return;

        var place = pp.toPlace();

        place.fetchFields({

          fields: ['id', 'formattedAddress', 'location', 'addressComponents', 'viewport']

        }).then(function () {

          populateFromNewPlace(prefix, mapId, place);

        }).catch(function (eSel) {

          console.error('[TandemAddressPlaces] fetchFields after gmp-select failed', eSel);

        });

      });



      pac.addEventListener('gmp-error', function (event) {

        console.error('[TandemAddressPlaces] PlaceAutocompleteElement gmp-error', event);

      });

    }).catch(function (errLib) {

      console.error('[TandemAddressPlaces] importLibrary(places) failed', errLib);

      mountLegacyAutocompleteInput();

    });

  }



  /** Legacy google.maps.places.Autocomplete en input .js-tandem-places-autocomplete */

  function bindLegacyPlacesAutocomplete($block) {

    var prefix = $block.data('prefix');

    var mapId = $block.data('map-id');

    var $input = $block.find('.js-tandem-places-autocomplete');



    if (!$input.length || $input.data('places-bound')) {

      return;

    }



    if ($input.prop('disabled')) {

      return;

    }



    $input.data('places-bound', true);



    if (!mapsUsable()) {

      $input.on('change blur', function () {

        var val = $input.val();

        setField(prefix, 'Formatted_Address', val);

      });

      return;

    }



    var autocomplete;

    try {

      autocomplete = new google.maps.places.Autocomplete($input[0], {

        fields: ['place_id', 'formatted_address', 'address_components', 'geometry']

      });

    } catch (e) {

      markAuthFailed('autocomplete-constructor', { message: e && e.message ? e.message : String(e) });

      showApiWarning(MSG.authFailed);

      $input.on('change blur', function () {

        setField(prefix, 'Formatted_Address', $input.val());

      });

      return;

    }



    autocomplete.addListener('place_changed', function () {

      var place = autocomplete.getPlace();

      if (!place || !place.geometry) {

        return;

      }

      populateFromPlace(prefix, mapId, place);

    });

  }



  function bindAutocomplete($block) {

    if ($block.find('.js-tandem-place-autocomplete-host').length) {

      bindPlaceAutocompleteHost($block);

      return;

    }

    bindLegacyPlacesAutocomplete($block);

  }



  function restoreMapFromFields($block) {

    var prefix = $block.data('prefix');

    var mapId = $block.data('map-id');

    var latLng = parseLatLng(prefix);



    if (latLng && hasApiKey()) {

      updateMap(prefix, mapId, latLng);

    } else {

      setMapEmpty(mapId);

    }

  }



  function waitForMapsPlacesLibrary(timeoutMs) {

    return new Promise(function (resolve, reject) {

      var elapsed = 0;

      var step = 50;

      var timer = window.setInterval(function () {

        if (mapsAuthFailed) {

          window.clearInterval(timer);

          reject(new Error('maps-auth-failed'));

          return;

        }

        if (window.google && window.google.maps && window.google.maps.places) {

          window.clearInterval(timer);

          resolve();

          return;

        }

        elapsed += step;

        if (elapsed >= timeoutMs) {

          window.clearInterval(timer);

          reject(new Error('maps-load-timeout'));

        }

      }, step);

    });

  }



  function findExistingMapsScript() {

    var scripts = document.getElementsByTagName('script');

    for (var i = 0; i < scripts.length; i++) {

      var src = scripts[i].src || '';

      if (src.indexOf('maps.googleapis.com/maps/api/js') >= 0) {

        return scripts[i];

      }

    }

    return null;

  }



  ns.loadMapsApi = function () {

    installAuthFailureHandler();



    if (!hasApiKey()) {

      return Promise.reject(new Error('no-api-key'));

    }



    if (mapsAuthFailed) {

      return Promise.reject(new Error('maps-auth-failed'));

    }



    if (window.google && window.google.maps && window.google.maps.places) {

      return Promise.resolve();

    }



    if (mapsLoadPromise) {

      return mapsLoadPromise;

    }



    var cfg = getConfig();

    var existing = findExistingMapsScript();



    if (existing) {

      mapsLoadPromise = waitForMapsPlacesLibrary(MAPS_JS_LOAD_TIMEOUT_MS).catch(function (err) {

        mapsLoadPromise = null;

        throw err;

      });

      return mapsLoadPromise;

    }



    mapsLoadPromise = new Promise(function (resolve, reject) {

      var callbackName = 'TandemAddressPlaces_mapsCallback_' + String(Date.now());

      var settled = false;

      var loadTimeoutId = null;



      function finish(err) {

        if (settled) {

          return;

        }

        settled = true;

        if (loadTimeoutId) {

          window.clearTimeout(loadTimeoutId);

        }

        try {

          delete window[callbackName];

        } catch (e1) {

          window[callbackName] = undefined;

        }

        mapsLoadPromise = null;

        if (err) {

          reject(err);

        } else if (mapsAuthFailed) {

          reject(new Error('maps-auth-failed'));

        } else {

          waitForMapsPlacesLibrary(3000).then(resolve, function () {

            reject(new Error('maps-places-missing'));

          });

        }

      }



      window[callbackName] = function () {

        window.setTimeout(function () {

          finish(mapsAuthFailed ? new Error('maps-auth-failed') : null);

        }, 0);

      };



      var script = document.createElement('script');

      script.id = 'tandem-google-maps-js';

      script.async = true;

      script.defer = true;

      script.onerror = function () {

        console.error('[TandemAddressPlaces] Script load error for maps.googleapis.com/maps/api/js');

        finish(new Error('maps-load-failed'));

      };

      script.onload = function () {

        if (mapsAuthFailed) {

          finish(new Error('maps-auth-failed'));

          return;

        }

        if (typeof window[callbackName] === 'function' && window.google && window.google.maps) {

          return;

        }

        waitForMapsPlacesLibrary(5000).then(

          function () { finish(null); },

          function (err) { finish(err); }

        );

      };



      var qs = [

        'key=' + encodeURIComponent(cfg.apiKey),

        'loading=async',

        'libraries=places',

        'v=weekly',

        'callback=' + encodeURIComponent(callbackName)

      ];

      if (cfg.language) {

        qs.push('language=' + encodeURIComponent(cfg.language));

      }

      if (cfg.region) {

        qs.push('region=' + encodeURIComponent(cfg.region));

      }



      script.src = 'https://maps.googleapis.com/maps/api/js?' + qs.join('&');

      document.head.appendChild(script);



      loadTimeoutId = window.setTimeout(function () {

        if (!settled && !(window.google && window.google.maps && window.google.maps.places)) {

          console.error('[TandemAddressPlaces] Timed out waiting for Google Maps JS API');

          finish(new Error('maps-load-timeout'));

        }

      }, MAPS_JS_LOAD_TIMEOUT_MS);

    });



    return mapsLoadPromise;

  };



  ns.initBlocks = function (selector) {

    selector = selector || '.tandem-address-block';

    var $blocks = $(selector);

    if (!$blocks.length) {

      return Promise.resolve();

    }



    installAuthFailureHandler();

    if (!hasApiKey()) {

      showApiWarning(MSG.noKey);

      $blocks.each(function () {

        var $b = $(this);

        bindAutocomplete($b);

        restoreMapFromFields($b);

      });

      return Promise.resolve();

    }



    hideBlockMapsErrors();



    return ns.loadMapsApi()

      .then(function () {

        if (mapsAuthFailed) {

          throw new Error('maps-auth-failed');

        }

        $blocks.each(function () {

          var $b = $(this);

          bindAutocomplete($b);

          restoreMapFromFields($b);

        });

      })

      .catch(function (err) {

        var msg = MSG.loadFailed;

        if (mapsAuthFailed || (err && err.message === 'maps-auth-failed')) {

          msg = MSG.authFailed;

        } else if (err && (err.message === 'maps-load-timeout' || err.message === 'maps-places-missing')) {

          msg = MSG.loadFailed + ' (tiempo de espera agotado o biblioteca places no disponible).';

        }

        console.error('[TandemAddressPlaces] initBlocks failed:', err && err.message ? err.message : err, lastAuthFailureDetail);

        showApiWarning(msg);

        $blocks.each(function () {

          bindAutocomplete($(this));

        });

      });

  };



  ns.init = function (options) {

    options = options || {};

    return ns.initBlocks(options.selector);

  };



  ns.populateFromPlace = populateFromPlace;

  ns.populateFromNewPlace = populateFromNewPlace;

  ns.updateMap = updateMap;

  ns.setField = setField;

  ns.getField = getField;

  ns.clearMapState = clearMapState;



})(window);

