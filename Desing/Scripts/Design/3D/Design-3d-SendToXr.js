// Enviar diseño actual a un dispositivo XR (Quest / tablet).
(function () {
    var designId = null;
    var $modal, $sel, $status, $btnConfirm;

    function setStatus(msg, isError) {
        if (!$status || !$status.length) return;
        $status.css('color', isError ? '#b00020' : '#555').text(msg || '');
    }

    function openModal() {
        if (!$modal || !$modal.length) return;
        $modal.show();
        setStatus('Cargando dispositivos…', false);
        $sel.empty().append($('<option>').val('').text('Cargando…'));
        $btnConfirm.prop('disabled', true);

        $.getJSON('/TandemXrApi/ListDevices')
            .done(function (resp) {
                $sel.empty();
                if (!resp || !resp.exito) {
                    setStatus((resp && resp.mensaje) || 'No se pudieron cargar dispositivos.', true);
                    $sel.append($('<option>').val('').text('—'));
                    return;
                }
                var list = resp.devices || [];
                if (!list.length) {
                    $sel.append($('<option>').val('').text('No hay dispositivos. Crea uno en Configuración → Dispositivos XR'));
                    setStatus('Aún no hay dispositivos XR activos.', true);
                    return;
                }
                $sel.append($('<option>').val('').text('Selecciona un dispositivo…'));
                list.forEach(function (d) {
                    var label = (d.textLabel || ('#' + d.id)) +
                        ' (' + (d.textDeviceType || '?') + ')' +
                        (d.isPaired ? '' : ' — sin emparejar');
                    $sel.append($('<option>').val(d.id).text(label));
                });
                setStatus('', false);
                $btnConfirm.prop('disabled', false);
            })
            .fail(function () {
                $sel.empty().append($('<option>').val('').text('Error de red'));
                setStatus('Error al consultar /TandemXrApi/ListDevices', true);
            });
    }

    function closeModal() {
        if ($modal) $modal.hide();
    }

    function send() {
        var deviceId = parseInt($sel.val(), 10);
        if (!deviceId) {
            setStatus('Selecciona un dispositivo.', true);
            return;
        }
        if (!designId) {
            setStatus('DesignId no disponible en esta pantalla.', true);
            return;
        }

        $btnConfirm.prop('disabled', true);
        setStatus('Enviando…', false);

        $.ajax({
            url: '/TandemXrApi/SendToDevice',
            type: 'POST',
            data: { designId: designId, deviceId: deviceId }
        }).done(function (resp) {
            if (resp && resp.exito) {
                setStatus(resp.mensaje || 'Enviado.', false);
                setTimeout(closeModal, 900);
            } else {
                setStatus((resp && resp.mensaje) || 'No se pudo enviar.', true);
                $btnConfirm.prop('disabled', false);
            }
        }).fail(function () {
            setStatus('Error de red al enviar.', true);
            $btnConfirm.prop('disabled', false);
        });
    }

    $(function () {
        designId = parseInt($('#btnSendToXr').data('design-id') || window.__tandemDesignId || '0', 10);
        // Fallback: atributo inyectado en Design.cshtml via data
        if (!designId && typeof window.TandemDesignId === 'number') {
            designId = window.TandemDesignId;
        }

        $modal = $('#modalSendToXr');
        $sel = $('#selXrDevice');
        $status = $('#lblSendToXrStatus');
        $btnConfirm = $('#btnConfirmSendToXr');

        $('#btnSendToXr').on('click', openModal);
        $('#btnCloseSendToXr, #btnCancelSendToXr').on('click', closeModal);
        $btnConfirm.on('click', send);
        $modal.on('click', function (e) {
            if (e.target === this) closeModal();
        });
    });
})();
