/**
 * APS Viewer 3D (global Autodesk from viewer3D.min.js).
 * Expects window.__masterArticleApsDwg = { tokenUrl: string, urnUrl: string }.
 */
(function () {
    var cfg = window.__masterArticleApsDwg;
    if (!cfg || !cfg.tokenUrl || !cfg.urnUrl) return;

    var Autodesk = window['Autodesk'];
    var hosts = document.querySelectorAll('[data-aps-dwg-viewer]');
    if (!hosts.length || !Autodesk || !Autodesk.Viewing) return;

    var tokenUrl = cfg.tokenUrl;
    var urnUrl = cfg.urnUrl;

    var options = {
        env: 'AutodeskProduction2',
        getAccessToken: function (onToken) {
            fetch(tokenUrl, { credentials: 'same-origin' })
                .then(function (r) { return r.json(); })
                .then(function (data) {
                    if (!data || !data.access_token) {
                        console.error('APS token', data);
                        onToken('', 0);
                        return;
                    }
                    onToken(data.access_token, data.expires_in || 3600);
                })
                .catch(function () { onToken('', 0); });
        }
    };

    function showError(container, msg) {
        container.innerHTML = '<p class="text-danger small p-3 mb-0">' + (msg || 'No se pudo cargar el visor.') + '</p>';
    }

    function loadOne(container) {
        var id = container.getAttribute('data-article-id');
        var slotKey = container.getAttribute('data-slot-key');
        var url = urnUrl + '?id=' + encodeURIComponent(id) + '&slotKey=' + encodeURIComponent(slotKey);
        fetch(url, { credentials: 'same-origin' })
            .then(function (r) { return r.json(); })
            .then(function (payload) {
                if (!payload || !payload.ok || !payload.urn) {
                    showError(container, (payload && payload.error) ? payload.error : 'Respuesta inválida del servidor.');
                    return;
                }
                container.innerHTML = '';
                var viewer = new Autodesk.Viewing.GuiViewer3D(container, {});
                var started = viewer.start();
                if (!started) {
                    showError(container, 'No se pudo iniciar el visor WebGL.');
                    return;
                }
                Autodesk.Viewing.Document.load('urn:' + payload.urn, function (doc) {
                    var viewables = doc.getRoot().getDefaultGeometry();
                    if (!viewables) {
                        showError(container, 'No hay geometría traducida para este DWG.');
                        return;
                    }
                    viewer.loadDocumentNode(doc, viewables).then(function () {
                        try { viewer.fitToView(); } catch (e) { }
                    }).catch(function () {
                        showError(container, 'Error al cargar el nodo de documento.');
                    });
                }, function (code, msg) {
                    showError(container, msg || ('Manifiesto: ' + code));
                });
                var ro = new ResizeObserver(function () {
                    try { viewer.resize(); } catch (e) { }
                });
                ro.observe(container);
            })
            .catch(function () {
                showError(container, 'Error de red al solicitar la traducción del DWG.');
            });
    }

    Autodesk.Viewing.Initializer(options, function () {
        for (var i = 0; i < hosts.length; i++) {
            loadOne(hosts[i]);
        }
    });
})();
