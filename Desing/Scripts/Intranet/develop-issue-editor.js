/* Develop › US y Bugs: editor de capturas del detalle (trazo libre y bocadillos con texto). */
(function (window, $) {
    'use strict';

    var root = document.getElementById('devIssueEditor');
    if (!root || !$) return;

    var canvas = root.querySelector('canvas');
    var ctx = canvas.getContext('2d');
    var input = root.querySelector('textarea');
    var msg = root.querySelector('[data-role="msg"]');
    var image = null, card = null, shapes = [], current = null, pending = null, tool = 'pen';

    function opt(name) {
        var el = root.querySelector('[data-opt="' + name + '"]');
        return el.type === 'checkbox' ? el.checked : el.value;
    }

    /* Píxeles de imagen por píxel de pantalla: trazos y texto se ven igual sea cual sea el tamaño de la captura. */
    function scale() {
        return canvas.width / (canvas.getBoundingClientRect().width || canvas.width);
    }

    function point(e) {
        var r = canvas.getBoundingClientRect(), k = scale();
        return { x: (e.clientX - r.left) * k, y: (e.clientY - r.top) * k };
    }

    function drawStroke(s) {
        ctx.strokeStyle = s.color;
        ctx.lineWidth = s.width;
        ctx.lineCap = ctx.lineJoin = 'round';
        ctx.beginPath();
        s.points.forEach(function (p, i) { if (i) ctx.lineTo(p.x, p.y); else ctx.moveTo(p.x, p.y); });
        if (s.points.length === 1) ctx.lineTo(s.points[0].x + 0.01, s.points[0].y);
        ctx.stroke();
    }

    function drawBubble(b) {
        var u = b.unit, size = 16 * u, pad = 8 * u, lineHeight = size * 1.3;
        var lines = b.text.split('\n');
        ctx.font = (b.italic ? 'italic ' : '') + size + 'px Arial, sans-serif';
        var w = Math.max.apply(null, lines.map(function (l) { return ctx.measureText(l).width; })) + pad * 2;
        var h = lines.length * lineHeight + pad * 2;
        var x = b.x - w / 2, y = b.y - h / 2, radius = 8 * u;
        var dx = b.tipX - b.x, dy = b.tipY - b.y, len = Math.sqrt(dx * dx + dy * dy) || 1;
        var base = Math.min(w, h) / 4, px = -dy / len * base, py = dx / len * base;

        function outline() {
            ctx.beginPath();
            ctx.moveTo(b.tipX, b.tipY);
            ctx.lineTo(b.x + px, b.y + py);
            ctx.lineTo(b.x - px, b.y - py);
            ctx.closePath();
            ctx.moveTo(x + radius, y);
            ctx.arcTo(x + w, y, x + w, y + h, radius);
            ctx.arcTo(x + w, y + h, x, y + h, radius);
            ctx.arcTo(x, y + h, x, y, radius);
            ctx.arcTo(x, y, x + w, y, radius);
            ctx.closePath();
        }

        /* Contorno a doble grosor y relleno encima: queda solo el borde exterior de la unión cola + caja. */
        ctx.lineJoin = 'round';
        ctx.strokeStyle = b.color;
        ctx.lineWidth = 4 * u;
        outline();
        ctx.stroke();
        ctx.fillStyle = '#fff';
        ctx.fill();

        ctx.save();
        if (b.shadow) {
            ctx.shadowColor = 'rgba(0,0,0,.55)';
            ctx.shadowBlur = 3 * u;
            ctx.shadowOffsetX = ctx.shadowOffsetY = 2 * u;
        }
        ctx.fillStyle = b.color;
        ctx.textBaseline = 'middle';
        lines.forEach(function (l, i) { ctx.fillText(l, x + pad, y + pad + lineHeight * (i + 0.5)); });
        ctx.restore();
    }

    function render() {
        ctx.clearRect(0, 0, canvas.width, canvas.height);
        ctx.drawImage(image, 0, 0);
        shapes.concat(current ? [current] : []).forEach(function (s) {
            if (s.type === 'pen') drawStroke(s);
            else if (s.text) drawBubble(s);
            else drawStroke({ color: s.color, width: 2 * s.unit, points: [{ x: s.tipX, y: s.tipY }, { x: s.x, y: s.y }] });
        });
    }

    function setTool(name) {
        tool = name;
        root.setAttribute('data-tool', name);
        Array.prototype.forEach.call(root.querySelectorAll('[data-tool]'), function (b) {
            if (b !== root) b.classList.toggle('active', b.getAttribute('data-tool') === name);
        });
    }

    function commitText() {
        if (!pending) return;
        var b = pending, text = input.value.replace(/\s+$/, '');
        pending = null;
        input.style.display = 'none';
        input.value = '';
        if (text) {
            b.text = text;
            b.color = opt('color');
            b.italic = opt('italic');
            b.shadow = opt('shadow');
            shapes.push(b);
        }
        render();
    }

    function askText(b) {
        var r = canvas.getBoundingClientRect(), k = scale();
        pending = b;
        input.style.left = Math.max(0, Math.min(b.x / k - 95, r.width - 195)) + 'px';
        input.style.top = Math.max(0, b.y / k - 20) + 'px';
        input.style.color = opt('color');
        input.style.fontStyle = opt('italic') ? 'italic' : 'normal';
        input.style.display = 'block';
        input.focus();
    }

    function close() {
        commitText();
        root.classList.remove('is-open');
        image = card = current = null;
        shapes = [];
    }

    function open($card) {
        var src = $card.find('img').first().attr('src');
        var img = new Image();
        msg.textContent = '';
        img.onload = function () {
            image = img;
            card = $card;
            shapes = [];
            canvas.width = img.naturalWidth;
            canvas.height = img.naturalHeight;
            setTool('pen');
            root.classList.add('is-open');
            render();
        };
        img.src = src;
    }

    function save() {
        commitText();
        if (!shapes.length) { close(); return; }
        var buttons = root.querySelectorAll('[data-act]');
        Array.prototype.forEach.call(buttons, function (b) { b.disabled = true; });
        canvas.toBlob(function (blob) {
            var data = new FormData();
            data.append('__RequestVerificationToken', $('input[name="__RequestVerificationToken"]').first().val());
            data.append('id', window.developIssueUrls.issueId);
            data.append('detailId', card.data('detail-id'));
            data.append('image', blob, 'captura-editada.png');
            $.ajax({ url: root.getAttribute('data-save-url'), type: 'POST', data: data, processData: false, contentType: false })
                .done(function (r) {
                    if (r && r.ok) {
                        card.find('img').first().attr('src', r.url);
                        card.find('.js-issue-image-link').attr('href', r.url);
                        close();
                    } else {
                        msg.textContent = (r && r.message) || root.getAttribute('data-save-error');
                    }
                })
                .fail(function () { msg.textContent = root.getAttribute('data-save-error'); })
                .always(function () { Array.prototype.forEach.call(buttons, function (b) { b.disabled = false; }); });
        }, 'image/png');
    }

    canvas.addEventListener('pointerdown', function (e) {
        if (e.button !== 0 || !image) return;
        commitText();
        var p = point(e), u = scale();
        current = tool === 'pen'
            ? { type: 'pen', color: opt('color'), width: 3 * u, points: [p] }
            : { type: 'bubble', color: opt('color'), unit: u, tipX: p.x, tipY: p.y, x: p.x, y: p.y };
        canvas.setPointerCapture(e.pointerId);
        render();
    });

    canvas.addEventListener('pointermove', function (e) {
        if (!current) return;
        var p = point(e);
        if (current.type === 'pen') current.points.push(p);
        else { current.x = p.x; current.y = p.y; }
        render();
    });

    canvas.addEventListener('pointerup', function () {
        var s = current;
        current = null;
        if (!s) return;
        if (s.type === 'pen') shapes.push(s);
        else {
            /* Clic sin arrastrar: el bocadillo se coloca arriba a la derecha del punto señalado. */
            if (Math.abs(s.x - s.tipX) + Math.abs(s.y - s.tipY) < 6 * s.unit) { s.x += 90 * s.unit; s.y -= 60 * s.unit; }
            askText(s);
        }
        render();
    });

    input.addEventListener('keydown', function (e) {
        if (e.key === 'Enter' && !e.shiftKey) { e.preventDefault(); commitText(); }
        else if (e.key === 'Escape') { e.stopPropagation(); input.value = ''; commitText(); }
    });
    /* Cambiar color, itálica o sombreado no cierra el texto que se está escribiendo. */
    input.addEventListener('blur', function (e) {
        if (!(e.relatedTarget && e.relatedTarget.hasAttribute('data-opt'))) commitText();
    });

    root.addEventListener('click', function (e) {
        var b = e.target.closest('[data-tool],[data-act]');
        if (!b || b === root) return;
        if (b.hasAttribute('data-tool')) { commitText(); setTool(b.getAttribute('data-tool')); return; }
        var act = b.getAttribute('data-act');
        if (act === 'undo') { shapes.pop(); render(); }
        else if (act === 'cancel') { input.value = ''; close(); }
        else if (act === 'save') save();
    });

    root.addEventListener('change', function (e) {
        if (!pending || !e.target.hasAttribute('data-opt')) return;
        input.style.color = opt('color');
        input.style.fontStyle = opt('italic') ? 'italic' : 'normal';
        input.focus();
    });

    $(document).on('click', '.js-issue-image-edit', function () {
        open($(this).closest('.js-issue-detail'));
    });
}(window, window.jQuery));
