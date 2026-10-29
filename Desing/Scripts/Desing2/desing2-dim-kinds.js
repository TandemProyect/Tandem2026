/* Tipos de cota. Doble clic en Acotar abre el cuadro. Las casillas se guardan en una cookie del diseño. */
(function () {
    var COOKIE_BASE = "ma_stl_desing2_dim_kinds";
    var COOKIE_MAX_AGE = 365 * 24 * 60 * 60;
    var KINDS = [
        "walls-total",
        "walls-partial",
        "walls-axes",
        "panels",
        "remates",
        "adjustable-panel",
        "wall-id"
    ];

    function safeId(value) {
        var clean = String(value || "0").replace(/[^a-zA-Z0-9_-]/g, "");
        return clean ? clean.slice(0, 40) : "0";
    }

    function cookieName() {
        var params = new URLSearchParams(window.location.search);
        var offerId = (params.get("offerId") || "").trim();
        var designId = (params.get("designId") || "").trim();
        if (!offerId && !designId) return COOKIE_BASE;
        return COOKIE_BASE + "_o" + safeId(offerId) + "_d" + safeId(designId);
    }

    function readCookie(name) {
        var prefix = name + "=";
        var parts = document.cookie ? document.cookie.split(";") : [];
        for (var i = 0; i < parts.length; i++) {
            var chunk = parts[i].replace(/^\s+/, "");
            if (chunk.indexOf(prefix) !== 0) continue;
            try {
                return decodeURIComponent(chunk.slice(prefix.length));
            } catch (e) {
                return chunk.slice(prefix.length);
            }
        }
        return null;
    }

    function writeCookie(name, value) {
        document.cookie = name + "=" + encodeURIComponent(value) +
            "; path=/; max-age=" + COOKIE_MAX_AGE + "; SameSite=Lax";
    }

    function readStored() {
        try {
            var raw = readCookie(cookieName());
            if (!raw) return null;
            var parsed = JSON.parse(raw);
            return parsed && typeof parsed === "object" ? parsed : null;
        } catch (e) {
            return null;
        }
    }

    function publish(panel) {
        var map = {};
        var inputs = panel.querySelectorAll("input[data-dim-kind]");
        for (var i = 0; i < inputs.length; i++) {
            map[inputs[i].getAttribute("data-dim-kind")] = inputs[i].checked;
        }
        try {
            writeCookie(cookieName(), JSON.stringify(map));
        } catch (e) { }
        var on = [];
        for (var k = 0; k < KINDS.length; k++) {
            if (map[KINDS[k]]) on.push(KINDS[k]);
        }
        var shell = document.getElementById("ma-stl-viewer-shell");
        if (shell) shell.setAttribute("data-ma-stl-dim-kinds", on.join(","));
        window.desing2DimKinds = map;
        if (typeof window.desing2RefreshWallDims === 'function') window.desing2RefreshWallDims();
        if (typeof window.desing2SyncCornerGuides === 'function') window.desing2SyncCornerGuides();
    }

    function boot() {
        var pop = document.getElementById("desing2-dim-kinds-pop");
        var panel = document.getElementById("desing2-dim-kinds");
        if (!pop || !panel || pop.getAttribute("data-dim-kinds-ready") === "1") return;
        pop.setAttribute("data-dim-kinds-ready", "1");
        if (pop.parentNode !== document.body) document.body.appendChild(pop);

        var stored = readStored();
        var inputs = panel.querySelectorAll("input[data-dim-kind]");
        for (var i = 0; i < inputs.length; i++) {
            var id = inputs[i].getAttribute("data-dim-kind");
            if (stored && Object.prototype.hasOwnProperty.call(stored, id)) {
                inputs[i].checked = !!stored[id];
            }
            inputs[i].addEventListener("change", function () {
                publish(panel);
            });
        }
        publish(panel);

        function setOpen(open) {
            pop.hidden = !open;
            if (open) pop.removeAttribute("hidden");
            else pop.setAttribute("hidden", "hidden");
        }

        window.desing2OpenDimKindsPopup = function () {
            setOpen(true);
        };
        window.desing2CloseDimKindsPopup = function () {
            setOpen(false);
        };

        var closeBtn = document.getElementById("desing2-dim-kinds-close");
        if (closeBtn) {
            closeBtn.addEventListener("click", function (ev) {
                ev.preventDefault();
                ev.stopPropagation();
                setOpen(false);
            });
        }

        pop.addEventListener("click", function (ev) {
            if (ev.target === pop) setOpen(false);
        });

        document.addEventListener("keydown", function (ev) {
            if (ev.key === "Escape" && !pop.hasAttribute("hidden")) setOpen(false);
        });

        var head = panel.querySelector(".desing2-dim-kinds__head");
        if (head) {
            var dragging = false;
            var grabX = 0;
            var grabY = 0;
            var place = function (left, top) {
                var w = panel.offsetWidth;
                var h = panel.offsetHeight;
                var maxL = Math.max(8, window.innerWidth - w - 8);
                var maxT = Math.max(8, window.innerHeight - h - 8);
                left = Math.min(Math.max(8, left), maxL);
                top = Math.min(Math.max(8, top), maxT);
                panel.style.position = "fixed";
                panel.style.left = left + "px";
                panel.style.top = top + "px";
                panel.style.margin = "0";
            };
            head.addEventListener("pointerdown", function (ev) {
                if (ev.button !== 0) return;
                if (ev.target.closest && ev.target.closest("#desing2-dim-kinds-close")) return;
                var rect = panel.getBoundingClientRect();
                dragging = true;
                grabX = ev.clientX - rect.left;
                grabY = ev.clientY - rect.top;
                place(rect.left, rect.top);
                if (head.setPointerCapture) head.setPointerCapture(ev.pointerId);
                ev.preventDefault();
            });
            head.addEventListener("pointermove", function (ev) {
                if (!dragging) return;
                place(ev.clientX - grabX, ev.clientY - grabY);
            });
            var endDrag = function () { dragging = false; };
            head.addEventListener("pointerup", endDrag);
            head.addEventListener("pointercancel", endDrag);
        }
    }

    if (document.readyState === "loading") document.addEventListener("DOMContentLoaded", boot);
    else boot();
})();
