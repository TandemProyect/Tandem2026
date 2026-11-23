/* Develop › US y Bugs: pop-up movible de alta (_DevelopIssuePopup.cshtml).
   Capturas: adjuntar, pegar (Ctrl+V) o recortar la pestaña con dos clics (getDisplayMedia). */
(function (window, document) {
    'use strict';

    var pop = document.getElementById('developIssuePopup');
    if (!pop) return;

    var form = pop.querySelector('form');
    var thumbs = pop.querySelector('.js-dev-issue-thumbs');
    var count = pop.querySelector('.js-dev-issue-count');
    var msg = pop.querySelector('.js-dev-issue-msg');
    var fileInput = pop.querySelector('.js-dev-issue-file');
    var BOX_COOKIE = 'developIssuePopupBox';
    var files = [];
    var stream = null;
    var video = null;

    function text(key) { return pop.getAttribute('data-msg-' + key) || ''; }
    function isOpen() { return pop.style.display === 'flex'; }

    function say(message, kind, url) {
        msg.textContent = message || '';
        msg.className = 'full dev-issue-pop__msg js-dev-issue-msg ' + (kind === 'ok' ? 'text-success' : 'text-danger');
        if (url) {
            var a = document.createElement('a');
            a.href = url;
            a.textContent = text('open');
            a.className = 'ms-1';
            msg.appendChild(a);
        }
    }

    function place(left, top) {
        var w = pop.offsetWidth || 280, h = pop.offsetHeight || 200;
        left = Math.max(0, Math.min(left, window.innerWidth - w));
        top = Math.max(0, Math.min(top, window.innerHeight - Math.min(h, 40)));
        pop.style.left = left + 'px';
        pop.style.top = top + 'px';
        pop.style.right = 'auto';
    }

    /* Tamaño y posición: se leen de la cookie al abrir y se guardan al pulsar Guardar. */
    function restoreBox() {
        var r = pop.getBoundingClientRect(), left = r.left, top = r.top;
        try {
            var m = document.cookie.match(new RegExp('(?:^|; )' + BOX_COOKIE + '=([^;]*)'));
            var p = m ? JSON.parse(decodeURIComponent(m[1])) : null;
            if (p && p.width > 0 && p.height > 0) {
                pop.style.width = p.width + 'px';
                pop.style.height = p.height + 'px';
            }
            if (p && isFinite(p.left) && isFinite(p.top)) { left = p.left; top = p.top; }
        } catch (_e) { /* cookie ausente o ilegible */ }
        place(left, top);
    }

    function saveBox() {
        var r = pop.getBoundingClientRect();
        var box = { left: Math.round(r.left), top: Math.round(r.top) };
        if (pop.style.height) { box.width = parseInt(pop.style.width, 10) || Math.round(r.width); box.height = parseInt(pop.style.height, 10); }
        document.cookie = BOX_COOKIE + '=' + encodeURIComponent(JSON.stringify(box)) + '; path=/; max-age=31536000; SameSite=Lax';
    }

    /* Cabecera roja si es Bug; color según urgencia y estado. */
    function paintCodes() {
        pop.classList.toggle('dev-issue-pop--bug', form.elements.TextType.value === 'BUG');
        ['TextUrgency', 'TextState'].forEach(function (n) { form.elements[n].setAttribute('data-v', form.elements[n].value); });
    }

    function stopStream() {
        if (stream) stream.getTracks().forEach(function (t) { t.stop(); });
        stream = null;
        video = null;
    }

    function open(type) {
        if (type) form.elements.TextType.value = type;
        paintCodes();
        if (!isOpen()) {
            pop.style.display = 'flex';
            restoreBox();
        }
        form.elements.TextLabel.focus();
    }

    function close() {
        if (createdId) reset();
        pop.style.display = 'none';
        stopStream();
    }

    function reset() {
        var type = form.elements.TextType.value;
        var date = form.elements.DateIssue.value;
        form.reset();
        form.elements.TextType.value = type;
        form.elements.DateIssue.value = date;
        paintCodes();
        files = [];
        createdId = createdUrl = createdMsg = null;
        paintThumbs();
    }

    function paintThumbs() {
        thumbs.innerHTML = '';
        count.textContent = files.length ? '(' + files.length + ')' : '';
        files.forEach(function (f, i) {
            var item = document.createElement('div');
            item.className = 'dev-issue-pop__thumb';
            var img = document.createElement('img');
            img.src = URL.createObjectURL(f);
            img.alt = f.name;
            img.onload = function () { URL.revokeObjectURL(img.src); };
            var rm = document.createElement('button');
            rm.type = 'button';
            rm.title = text('remove');
            rm.textContent = '\u00d7';
            rm.addEventListener('click', function () { files.splice(i, 1); paintThumbs(); });
            item.appendChild(img);
            item.appendChild(rm);
            thumbs.appendChild(item);
        });
    }

    function addFiles(list) {
        Array.prototype.forEach.call(list || [], function (f) { if (f && /^image\//.test(f.type)) files.push(f); });
        paintThumbs();
    }

    /* ---- Mover por la cabecera ---- */
    var head = pop.querySelector('.js-dev-issue-drag');
    head.addEventListener('pointerdown', function (e) {
        if (e.button !== 0 || e.target.closest('button')) return;
        var r = pop.getBoundingClientRect();
        var dx = e.clientX - r.left, dy = e.clientY - r.top;
        head.setPointerCapture(e.pointerId);
        function move(ev) { place(ev.clientX - dx, ev.clientY - dy); }
        function up() {
            head.removeEventListener('pointermove', move);
            head.removeEventListener('pointerup', up);
            head.removeEventListener('pointercancel', up);
        }
        head.addEventListener('pointermove', move);
        head.addEventListener('pointerup', up);
        head.addEventListener('pointercancel', up);
        e.preventDefault();
    });
    window.addEventListener('resize', function () {
        if (!isOpen()) return;
        var r = pop.getBoundingClientRect();
        place(r.left, r.top);
    });

    /* ---- Captura de pantalla: rectángulo con dos clics ---- */
    function ensureStream() {
        if (stream && stream.active && video) return Promise.resolve();
        if (!navigator.mediaDevices || !navigator.mediaDevices.getDisplayMedia) return Promise.reject(new Error('unsupported'));
        return navigator.mediaDevices.getDisplayMedia({ video: { displaySurface: 'browser' }, audio: false, preferCurrentTab: true, selfBrowserSurface: 'include' })
            .then(function (s) {
                stream = s;
                s.getVideoTracks()[0].addEventListener('ended', stopStream);
                video = document.createElement('video');
                video.muted = true;
                video.srcObject = s;
                return video.play();
            });
    }

    function pickRect() {
        return new Promise(function (resolve) {
            var layer = document.createElement('div');
            layer.className = 'dev-issue-shot';
            var hint = document.createElement('div');
            hint.className = 'dev-issue-shot__hint';
            hint.textContent = text('hint');
            var rect = document.createElement('div');
            rect.className = 'dev-issue-shot__rect';
            layer.appendChild(hint);
            layer.appendChild(rect);
            document.body.appendChild(layer);
            var start = null;

            function box(e) {
                return { left: Math.min(start.x, e.clientX), top: Math.min(start.y, e.clientY), width: Math.abs(e.clientX - start.x), height: Math.abs(e.clientY - start.y) };
            }
            function finish(result) {
                document.removeEventListener('keydown', key, true);
                layer.remove();
                resolve(result);
            }
            function key(e) {
                if (e.key !== 'Escape') return;
                e.preventDefault();
                e.stopPropagation();
                finish(null);
            }
            layer.addEventListener('mousemove', function (e) {
                if (!start) return;
                var b = box(e);
                rect.style.display = 'block';
                rect.style.left = b.left + 'px';
                rect.style.top = b.top + 'px';
                rect.style.width = b.width + 'px';
                rect.style.height = b.height + 'px';
            });
            layer.addEventListener('click', function (e) {
                e.preventDefault();
                if (!start) { start = { x: e.clientX, y: e.clientY }; return; }
                var b = box(e);
                if (b.width < 4 || b.height < 4) return;
                finish(b);
            });
            layer.addEventListener('contextmenu', function (e) { e.preventDefault(); finish(null); });
            document.addEventListener('keydown', key, true);
        });
    }

    function nextPaint() {
        return new Promise(function (resolve) {
            window.requestAnimationFrame(function () { window.setTimeout(resolve, 250); });
        });
    }

    function grab(b) {
        var sx = video.videoWidth / window.innerWidth, sy = video.videoHeight / window.innerHeight;
        var canvas = document.createElement('canvas');
        canvas.width = Math.max(1, Math.round(b.width * sx));
        canvas.height = Math.max(1, Math.round(b.height * sy));
        canvas.getContext('2d').drawImage(video, Math.round(b.left * sx), Math.round(b.top * sy), canvas.width, canvas.height, 0, 0, canvas.width, canvas.height);
        return new Promise(function (resolve) { canvas.toBlob(resolve, 'image/png'); });
    }

    function capture() {
        say('');
        ensureStream().then(function () {
            pop.style.display = 'none';
            return pickRect();
        }).then(function (b) {
            if (!b) return null;
            return nextPaint().then(function () { return grab(b); });
        }).then(function (blob) {
            if (blob) addFiles([new File([blob], 'captura-' + Date.now() + '.png', { type: 'image/png' })]);
        }).catch(function (err) {
            say(err && err.message === 'unsupported' ? text('unsupported') : text('denied'));
        }).then(function () {
            pop.style.display = 'flex';
        });
    }

    /* ---- Eventos ---- */
    form.addEventListener('change', paintCodes);
    document.addEventListener('click', function (e) {
        var link = e.target.closest ? e.target.closest('.js-develop-issue-new') : null;
        if (!link) return;
        e.preventDefault();
        open(link.getAttribute('data-issue-type'));
    });
    Array.prototype.forEach.call(pop.querySelectorAll('.js-dev-issue-close'), function (b) { b.addEventListener('click', close); });
    pop.querySelector('.js-dev-issue-capture').addEventListener('click', capture);
    pop.querySelector('.js-dev-issue-attach').addEventListener('click', function () { fileInput.click(); });
    fileInput.addEventListener('change', function () { addFiles(fileInput.files); fileInput.value = ''; });

    document.addEventListener('paste', function (e) {
        if (!isOpen() || !e.clipboardData) return;
        var pasted = [];
        Array.prototype.forEach.call(e.clipboardData.items || [], function (item) {
            if (item.kind !== 'file' || !/^image\//.test(item.type)) return;
            var ext = (item.type.split('/')[1] || 'png').replace('jpeg', 'jpg');
            pasted.push(new File([item.getAsFile()], 'captura-' + Date.now() + '-' + pasted.length + '.' + ext, { type: item.type }));
        });
        if (!pasted.length) return;
        e.preventDefault();
        e.stopImmediatePropagation();
        addFiles(pasted);
    }, true);

    function post(url, data) {
        return fetch(url, { method: 'POST', body: data, credentials: 'same-origin', headers: { 'X-Requested-With': 'XMLHttpRequest', Accept: 'application/json' } })
            .then(function (res) { if (!res.ok) throw new Error(String(res.status)); return res.json(); })
            .then(function (r) { if (!r || !r.ok) throw new Error((r && r.message) || text('error')); return r; });
    }

    /* Una petición por captura: no hay tope por el número de imágenes (límite de subida del servidor). */
    function sendRest(id, list, index) {
        if (index >= list.length) return Promise.resolve();
        say(text('uploading').replace('{0}', index + 1).replace('{1}', list.length), 'ok');
        var data = new FormData();
        data.append('__RequestVerificationToken', form.elements.__RequestVerificationToken.value);
        data.append('id', id);
        data.append('captures', list[index], list[index].name);
        return post(pop.getAttribute('data-detail-url'), data).then(function () {
            files.splice(files.indexOf(list[index]), 1);
            return sendRest(id, list, index + 1);
        });
    }

    var createdId = null, createdUrl = null, createdMsg = null;
    form.addEventListener('submit', function (e) {
        e.preventDefault();
        var submit = form.querySelector('[type="submit"]');
        var list = files.slice();
        saveBox();
        submit.disabled = true;
        say('');
        var first;
        if (createdId) {
            /* Reintento: la US/Bug ya existe; solo faltan capturas. */
            first = Promise.resolve();
        } else {
            var data = new FormData(form);
            if (list.length) data.append('captures', list[0], list[0].name);
            first = post(pop.getAttribute('data-create-url'), data).then(function (r) {
                createdId = r.id; createdUrl = r.url; createdMsg = r.message;
                if (list.length) files.splice(files.indexOf(list[0]), 1);
                list.shift();
            });
        }
        first.then(function () { return sendRest(createdId, list, 0); })
            .then(function () {
                var m = createdMsg, u = createdUrl;
                createdId = createdUrl = createdMsg = null;
                reset();
                say(m, 'ok', u);
            })
            .catch(function (err) {
                paintThumbs();
                say(err && err.message && !/^\d+$/.test(err.message) ? err.message : text('error'), 'err', createdUrl);
            })
            .then(function () { submit.disabled = false; });
    });
}(window, document));
