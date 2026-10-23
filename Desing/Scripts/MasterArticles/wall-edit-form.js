/**
 * Formulario único «Modificar muro».
 * Lo usa el visor de Desing y la página del plugin de AutoCAD.
 */
export function mountWallEditForm(parent, options) {
    options = options || {};
    const floating = options.floating !== false;
    const iconBase = options.iconBase || '/Content/DesignTools/MenuIcon/';

    const root = document.createElement('div');
    root.style.position = floating ? 'fixed' : 'relative';
    root.style.zIndex = '2147483200';
    root.style.width = floating ? '680px' : '100%';
    root.style.maxWidth = floating ? 'calc(100vw - 24px)' : 'none';
    root.style.background = '#ffffff';
    root.style.border = floating ? '1px solid #cbd5e1' : 'none';
    root.style.borderRadius = floating ? '10px' : '0';
    root.style.boxShadow = floating ? '0 16px 36px rgba(2,6,23,0.22)' : 'none';
    root.style.display = floating ? 'none' : 'block';
    root.style.fontFamily = "'Segoe UI', Tahoma, sans-serif";
    root.style.userSelect = 'none';
    root.style.boxSizing = 'border-box';

    const head = document.createElement('div');
    head.style.padding = '10px 12px';
    head.style.background = '#0f172a';
    head.style.color = '#ffffff';
    head.style.borderTopLeftRadius = floating ? '10px' : '0';
    head.style.borderTopRightRadius = floating ? '10px' : '0';
    head.style.display = 'flex';
    head.style.alignItems = 'center';
    head.style.justifyContent = 'space-between';
    head.style.cursor = floating ? 'move' : 'default';

    const title = document.createElement('div');
    title.textContent = 'Modificar muro';
    title.style.fontSize = '13px';
    title.style.fontWeight = '600';
    head.appendChild(title);

    const closeBtn = document.createElement('button');
    closeBtn.type = 'button';
    closeBtn.textContent = 'x';
    closeBtn.style.border = 'none';
    closeBtn.style.background = 'transparent';
    closeBtn.style.color = '#ffffff';
    closeBtn.style.cursor = 'pointer';
    closeBtn.style.fontSize = '15px';
    closeBtn.style.padding = '0 4px';
    head.appendChild(closeBtn);

    const body = document.createElement('div');
    body.style.padding = '12px';
    body.style.userSelect = 'text';

    const messageEl = document.createElement('div');
    messageEl.style.display = 'none';
    messageEl.style.margin = '0 0 8px';
    messageEl.style.color = '#b91c1c';
    messageEl.style.fontSize = '12px';

    const tabRow = document.createElement('div');
    tabRow.style.display = 'flex';
    tabRow.style.gap = '6px';
    tabRow.style.marginBottom = '12px';

    function makeTab(label) {
        const btn = document.createElement('button');
        btn.type = 'button';
        btn.textContent = label;
        btn.style.flex = '1';
        btn.style.padding = '7px 6px';
        btn.style.border = 'none';
        btn.style.borderRadius = '4px';
        btn.style.cursor = 'pointer';
        btn.style.fontSize = '12px';
        return btn;
    }

    const tabMuro = makeTab('Muro');
    const tabTapes = makeTab('Configurar Tapes');
    const tabProps = makeTab('Configurar Puntales');
    tabRow.appendChild(tabMuro);
    tabRow.appendChild(tabTapes);
    tabRow.appendChild(tabProps);
    body.appendChild(messageEl);
    body.appendChild(tabRow);

    const muroPane = document.createElement('div');
    const tapesPane = document.createElement('div');
    tapesPane.style.display = 'none';
    tapesPane.style.fontSize = '13px';
    tapesPane.style.color = '#0f172a';
    tapesPane.style.padding = '4px 2px 8px';
    tapesPane.style.maxHeight = floating ? '62vh' : 'none';
    tapesPane.style.overflow = 'auto';

    const tapeMessageEl = document.createElement('div');
    tapeMessageEl.style.fontSize = '13px';
    tapeMessageEl.style.fontWeight = '600';
    tapeMessageEl.style.margin = '0 0 8px';
    tapeMessageEl.style.lineHeight = '1.35';
    tapesPane.appendChild(tapeMessageEl);

    const tapeHeadingEl = document.createElement('div');
    tapeHeadingEl.style.fontSize = '12px';
    tapeHeadingEl.style.color = '#334155';
    tapeHeadingEl.style.margin = '0 0 10px';
    tapeHeadingEl.style.lineHeight = '1.35';
    tapesPane.appendChild(tapeHeadingEl);

    const tapePairRow = document.createElement('div');
    tapePairRow.style.display = 'grid';
    tapePairRow.style.gridTemplateColumns = '1fr 1fr';
    tapePairRow.style.columnGap = '16px';
    tapesPane.appendChild(tapePairRow);

    let tapeIniSelectedId = '';
    let tapeEndSelectedId = '';

    function notifyTape() {
        if (options.onTapeSelection)
            options.onTapeSelection(tapeIniSelectedId, tapeEndSelectedId);
    }

    function makeTapeSection(solapeLabel) {
        const section = document.createElement('div');
        const solapeWrap = document.createElement('label');
        solapeWrap.style.display = 'flex';
        solapeWrap.style.alignItems = 'center';
        solapeWrap.style.gap = '8px';
        solapeWrap.style.marginBottom = '10px';
        const solapeCheckbox = document.createElement('input');
        solapeCheckbox.type = 'checkbox';
        solapeCheckbox.style.margin = '0';
        const solapeText = document.createElement('span');
        solapeText.textContent = solapeLabel;
        solapeWrap.appendChild(solapeCheckbox);
        solapeWrap.appendChild(solapeText);
        section.appendChild(solapeWrap);
        const optionsEl = document.createElement('div');
        section.appendChild(optionsEl);
        solapeCheckbox.addEventListener('change', function () {
            if (options.onRepaint) options.onRepaint();
            else api.paint(options.getUd ? options.getUd() : null);
        });
        tapePairRow.appendChild(section);
        return { solapeCheckbox: solapeCheckbox, optionsEl: optionsEl };
    }

    const tapeIniSection = makeTapeSection('Solape');
    const tapeEndSection = makeTapeSection('Solape');

    const tapeLegendRow = document.createElement('div');
    tapeLegendRow.style.display = 'grid';
    tapeLegendRow.style.gridTemplateColumns = '1fr 1fr';
    tapeLegendRow.style.columnGap = '16px';
    tapeLegendRow.style.marginTop = '4px';
    function makeLegend(text) {
        const cell = document.createElement('div');
        cell.textContent = text;
        cell.style.textAlign = 'center';
        cell.style.fontSize = '12px';
        cell.style.fontWeight = '700';
        cell.style.color = '#0f172a';
        cell.style.paddingTop = '6px';
        cell.style.borderTop = '1px solid #e2e8f0';
        tapeLegendRow.appendChild(cell);
    }
    makeLegend('Solape inicial');
    makeLegend('Solape final');
    tapesPane.appendChild(tapeLegendRow);

    const propsPane = document.createElement('div');
    propsPane.style.display = 'none';

    function makeCheck(text, checked) {
        const wrap = document.createElement('label');
        wrap.style.display = 'flex';
        wrap.style.alignItems = 'flex-start';
        wrap.style.gap = '8px';
        wrap.style.fontSize = '13px';
        wrap.style.color = '#0f172a';
        wrap.style.marginBottom = '14px';
        const input = document.createElement('input');
        input.type = 'checkbox';
        input.checked = checked !== false;
        input.style.marginTop = '2px';
        const span = document.createElement('span');
        span.textContent = text;
        wrap.appendChild(input);
        wrap.appendChild(span);
        return { wrap: wrap, input: input };
    }

    const propGrid = document.createElement('div');
    propGrid.style.display = 'grid';
    propGrid.style.gridTemplateColumns = '1fr 1fr';
    propGrid.style.columnGap = '16px';
    const propIn = makeCheck('Puntales superiores en cara interior');
    const propOut = makeCheck('Puntales superiores en cara exterior');
    const propInInf = makeCheck('Puntales Inferiores en cara interior');
    const propOutInf = makeCheck('Puntales Inferiores en cara exterior');
    propGrid.appendChild(propIn.wrap);
    propGrid.appendChild(propOut.wrap);
    propGrid.appendChild(propInInf.wrap);
    propGrid.appendChild(propOutInf.wrap);
    propsPane.appendChild(propGrid);

    body.appendChild(muroPane);
    body.appendChild(tapesPane);
    body.appendChild(propsPane);

    function paintTab(btn, active) {
        btn.style.background = active ? '#2563eb' : '#e8eef5';
        btn.style.color = active ? '#ffffff' : '#334155';
    }

    function showTab(which) {
        muroPane.style.display = which === 'muro' ? 'block' : 'none';
        tapesPane.style.display = which === 'tapes' ? 'block' : 'none';
        propsPane.style.display = which === 'props' ? 'block' : 'none';
        paintTab(tabMuro, which === 'muro');
        paintTab(tabTapes, which === 'tapes');
        paintTab(tabProps, which === 'props');
    }

    tabMuro.addEventListener('click', function () { showTab('muro'); });
    tabTapes.addEventListener('click', function () { showTab('tapes'); });
    tabProps.addEventListener('click', function () { showTab('props'); });
    showTab('muro');

    function makeLabel(text) {
        const label = document.createElement('label');
        label.textContent = text;
        label.style.display = 'block';
        label.style.fontSize = '12px';
        label.style.fontWeight = '600';
        label.style.color = '#334155';
        label.style.marginBottom = '6px';
        return label;
    }

    function makeMeasureInput() {
        const input = document.createElement('input');
        input.type = 'text';
        input.inputMode = 'decimal';
        input.autocomplete = 'off';
        input.style.width = '100%';
        input.style.boxSizing = 'border-box';
        input.style.padding = '8px 10px';
        input.style.border = '1px solid #cbd5e1';
        input.style.borderRadius = '8px';
        input.style.marginBottom = '10px';
        return input;
    }

    muroPane.appendChild(makeLabel('Grupo'));
    const groupInput = document.createElement('select');
    groupInput.style.width = '100%';
    groupInput.style.boxSizing = 'border-box';
    groupInput.style.padding = '8px 10px';
    groupInput.style.border = '1px solid #cbd5e1';
    groupInput.style.borderRadius = '8px';
    groupInput.style.marginBottom = '10px';
    groupInput.style.background = '#fff';
    muroPane.appendChild(groupInput);
    let groupCatalog = [];

    function groupPlaceholder() {
        const el = document.getElementById('desing2GroupModal');
        return (el && el.getAttribute('data-select')) || 'Seleccionar grupo';
    }

    function fillGroupSelect(selected) {
        const chosen = selected != null && selected !== '' && Number.isFinite(Number(selected))
            ? String(Math.round(Number(selected)))
            : '';
        groupInput.innerHTML = '';
        const blank = document.createElement('option');
        blank.value = '';
        blank.textContent = groupPlaceholder();
        groupInput.appendChild(blank);
        const seen = {};
        for (let i = 0; i < groupCatalog.length; i++) {
            const item = groupCatalog[i];
            if (!item || !Number.isFinite(Number(item.number))) continue;
            const n = String(Math.round(Number(item.number)));
            if (seen[n]) continue;
            seen[n] = true;
            const opt = document.createElement('option');
            opt.value = n;
            const name = item.name ? String(item.name) : '';
            opt.textContent = name ? (n + ' — ' + name) : n;
            groupInput.appendChild(opt);
        }
        if (chosen && !seen[chosen]) {
            const extra = document.createElement('option');
            extra.value = chosen;
            extra.textContent = chosen;
            groupInput.appendChild(extra);
        }
        groupInput.value = chosen;
    }

    fillGroupSelect('');

    muroPane.appendChild(makeLabel('Longitud del muro (m)'));
    const lengthInput = makeMeasureInput();
    muroPane.appendChild(lengthInput);

    muroPane.appendChild(makeLabel('Espesor del muro (m)'));
    const widthInput = makeMeasureInput();
    widthInput.addEventListener('input', function () {
        if (options.onRepaint) options.onRepaint();
        else api.paint(options.getUd ? options.getUd() : null);
    });
    muroPane.appendChild(widthInput);

    muroPane.appendChild(makeLabel('Altura del muro (m)'));
    const heightInput = document.createElement('input');
    heightInput.type = 'number';
    heightInput.min = '0.30';
    heightInput.max = '10.00';
    heightInput.step = '0.15';
    heightInput.value = '2.70';
    heightInput.style.width = '100%';
    heightInput.style.boxSizing = 'border-box';
    heightInput.style.padding = '8px 10px';
    heightInput.style.border = '1px solid #cbd5e1';
    heightInput.style.borderRadius = '8px';
    heightInput.style.marginBottom = '10px';
    muroPane.appendChild(heightInput);

    const applyRow = document.createElement('div');
    applyRow.style.display = 'grid';
    applyRow.style.gridTemplateColumns = '1fr 1fr';
    applyRow.style.columnGap = '12px';
    applyRow.style.marginBottom = '12px';

    const applyAll = makeCheck('Aplicar altura a todos los muros', true);
    applyAll.wrap.style.fontSize = '12px';
    applyAll.wrap.style.color = '#334155';
    const applyConfig = makeCheck('Aplicar la configuración a todos los muros y esquinas', false);
    applyConfig.wrap.style.fontSize = '12px';
    applyConfig.wrap.style.color = '#334155';
    applyRow.appendChild(applyAll.wrap);
    applyRow.appendChild(applyConfig.wrap);
    muroPane.appendChild(applyRow);

    const muroChecksTitle = document.createElement('div');
    muroChecksTitle.textContent = 'En este muro';
    muroChecksTitle.style.fontSize = '12px';
    muroChecksTitle.style.fontWeight = '600';
    muroChecksTitle.style.color = '#334155';
    muroChecksTitle.style.margin = '4px 0 8px';
    muroPane.appendChild(muroChecksTitle);

    const muroGrid = document.createElement('div');
    muroGrid.style.display = 'grid';
    muroGrid.style.gridTemplateColumns = '1fr 1fr';
    muroGrid.style.columnGap = '16px';
    muroGrid.style.marginTop = '4px';
    const bracketIn = makeCheck('Ménsulas en cara interior');
    const bracketOut = makeCheck('Ménsulas en cara exterior');
    const stiffIn = makeCheck('Rigidizadores en cara interior');
    const stiffOut = makeCheck('Rigidizadores en cara exterior');
    muroGrid.appendChild(bracketIn.wrap);
    muroGrid.appendChild(bracketOut.wrap);
    muroGrid.appendChild(stiffIn.wrap);
    muroGrid.appendChild(stiffOut.wrap);
    muroPane.appendChild(muroGrid);

    const panel750 = makeCheck('Utilizar paneles 0,75 regulables');
    const verticalPanels = makeCheck('Paneles verticales');
    muroPane.appendChild(panel750.wrap);
    muroPane.appendChild(verticalPanels.wrap);

    const actions = document.createElement('div');
    actions.style.display = 'flex';
    actions.style.justifyContent = 'flex-end';
    actions.style.gap = '8px';

    const cancelBtn = document.createElement('button');
    cancelBtn.type = 'button';
    cancelBtn.textContent = 'Cancelar';
    cancelBtn.style.padding = '7px 10px';
    cancelBtn.style.border = '1px solid #cbd5e1';
    cancelBtn.style.borderRadius = '8px';
    cancelBtn.style.background = '#ffffff';
    cancelBtn.style.cursor = 'pointer';

    const acceptBtn = document.createElement('button');
    acceptBtn.type = 'button';
    acceptBtn.textContent = 'Aceptar';
    acceptBtn.style.padding = '7px 12px';
    acceptBtn.style.border = 'none';
    acceptBtn.style.borderRadius = '8px';
    acceptBtn.style.background = '#2563eb';
    acceptBtn.style.color = '#ffffff';
    acceptBtn.style.cursor = 'pointer';

    actions.appendChild(cancelBtn);
    actions.appendChild(acceptBtn);
    body.appendChild(actions);
    root.appendChild(head);
    root.appendChild(body);
    (parent || document.body).appendChild(root);

    if (floating) {
        let dragState = null;
        head.addEventListener('mousedown', function (ev) {
            if (ev.button !== 0 || ev.target === closeBtn) return;
            const rect = root.getBoundingClientRect();
            dragState = { dx: ev.clientX - rect.left, dy: ev.clientY - rect.top };
            ev.preventDefault();
        });
        window.addEventListener('mousemove', function (ev) {
            if (!dragState || root.style.display === 'none') return;
            const vw = window.innerWidth || document.documentElement.clientWidth || 1280;
            const vh = window.innerHeight || document.documentElement.clientHeight || 720;
            const w = Math.max(root.offsetWidth, 320);
            const h = Math.max(root.offsetHeight, 160);
            const left = clamp(ev.clientX - dragState.dx, 8, Math.max(8, vw - w - 8));
            const top = clamp(ev.clientY - dragState.dy, 8, Math.max(8, vh - h - 8));
            root.style.left = Math.round(left) + 'px';
            root.style.top = Math.round(top) + 'px';
        });
        window.addEventListener('mouseup', function () { dragState = null; });
    }

    function cancel() {
        if (options.onCancel) options.onCancel();
    }
    closeBtn.addEventListener('click', cancel);
    cancelBtn.addEventListener('click', cancel);
    acceptBtn.addEventListener('click', function () {
        if (options.onAccept) options.onAccept(api.read());
    });

    function icon(fileName) {
        return iconBase + fileName;
    }

    function opt(id, label, fileName) {
        return { id: id, label: label, image: icon(fileName) };
    }

    function blocks() {
        return {
            end01: {
                text: 'El ancho de muro tiene solape',
                defaultId: 'TapeS1',
                options: [opt('TapeS1', 'Solución 1', 'TapeS1.png'), opt('TapeS2', 'Solución 2', 'TapeS2.png')],
            },
            end02: {
                text: 'El ancho de muro es menor de 0,30',
                thickness: 'El ancho de muro es menor de 0,30',
                defaultId: 'TapeS3',
                options: [opt('TapeS3', 'Solución 1', 'TapeS3.png')],
            },
            end03: {
                text: 'El ancho de muro no tiene solape y es 0,30, 0,45, 0,60 o 0,90',
                thickness: 'El ancho de muro es 0,30, 0,45, 0,60 o 0,90',
                defaultId: 'TapeS4',
                options: [opt('TapeS4', 'Solución 1', 'TapeS4.png'), opt('TapeS5', 'Solución 2', 'TapeS5.png')],
            },
            end04: {
                text: 'El ancho de muro no tiene solape, es mayor de 0,30, no es exacto, es múltiplo de 0,05 y menor de 0,60',
                thickness: 'El ancho de muro es mayor de 0,30, no es exacto, es múltiplo de 0,05 y menor de 0,60',
                defaultId: 'TapeS6',
                options: [opt('TapeS6', 'Solución 1', 'TapeS6.png'), opt('TapeS7', 'Solución 2', 'TapeS7.png')],
            },
            end05: {
                text: 'El ancho de muro no tiene solape, es mayor de 0,30, no es exacto, no es múltiplo de 0,05 y menor de 0,60',
                thickness: 'El ancho de muro es mayor de 0,30, no es exacto, no es múltiplo de 0,05 y menor de 0,60',
                defaultId: 'TapeS8',
                options: [opt('TapeS8', 'Solución 1', 'TapeS8.png')],
            },
            end06: {
                text: 'El ancho de muro no tiene solape, es mayor de 0,60, no es exacto y es múltiplo de 0,05',
                thickness: 'El ancho de muro es mayor de 0,60, no es exacto y es múltiplo de 0,05',
                defaultId: 'TapeS9',
                options: [
                    opt('TapeS9', 'Solución 1', 'TapeS9.png'),
                    opt('TapeS10', 'Solución 2', 'TapeS10.png'),
                    opt('TapeS11', 'Solución 3', 'TapeS11.png'),
                ],
            },
            end07: {
                text: 'El ancho de muro no tiene solape, es mayor de 0,60, no es exacto y no es múltiplo de 0,05',
                thickness: 'El ancho de muro es mayor de 0,60, no es exacto y no es múltiplo de 0,05',
                defaultId: 'TapeS12',
                options: [
                    opt('TapeS13', 'Solución 1', 'TapeS12.png'),
                    opt('TapeS12', 'Solución 2', 'TapeS13.png'),
                ],
            },
        };
    }

    function widthMeters(ud) {
        if (options.parseMeters) {
            const parsed = options.parseMeters(widthInput.value);
            if (parsed != null && parsed > 0) return parsed;
        } else {
            const raw = parseMeters(widthInput.value);
            if (raw != null && raw > 0) return raw;
        }
        const fromAttr = Number(ud && ud._DataWith);
        if (Number.isFinite(fromAttr) && fromAttr > 0) return fromAttr;
        return 0.30;
    }

    function blockKey(widthM, solape) {
        if (solape) return 'end01';
        const mm = Math.round((Number(widthM) || 0) * 1000);
        if (mm < 300) return 'end02';
        if (mm === 300 || mm === 450 || mm === 600 || mm === 900) return 'end03';
        const multiple = mm > 0 && mm % 50 === 0;
        if (mm < 600) return multiple ? 'end04' : 'end05';
        return multiple ? 'end06' : 'end07';
    }

    function knownId(value) {
        return typeof value === 'string' && (value.indexOf('TapeS') === 0 || value === 'Solucion_2');
    }

    function paintSide(side) {
        const catalog = blocks();
        const block = catalog[blockKey(widthMeters(side.ud), !!side.checkbox.checked)] || catalog.end03;
        const optionIds = {};
        for (let i = 0; i < block.options.length; i++) optionIds[block.options[i].id] = true;
        let selected = optionIds[side.selectedId] ? side.selectedId : side.storedId;
        if (!optionIds[selected]) selected = block.defaultId;
        side.optionsEl.textContent = '';
        for (let n = 0; n < block.options.length; n++) {
            const item = block.options[n];
            const row = document.createElement('label');
            row.style.display = 'flex';
            row.style.alignItems = 'center';
            row.style.gap = '10px';
            row.style.margin = '0 0 10px';
            row.style.cursor = 'pointer';
            row.style.color = '#0f172a';
            const input = document.createElement('input');
            input.type = 'checkbox';
            input.checked = item.id === selected;
            input.style.margin = '0';
            input.addEventListener('change', function () {
                side.onSelect(item.id);
                const boxes = side.optionsEl.querySelectorAll('input[type="checkbox"]');
                for (let b = 0; b < boxes.length; b++) boxes[b].checked = boxes[b] === input;
                input.checked = true;
            });
            const caption = document.createElement('span');
            caption.textContent = item.label;
            caption.style.minWidth = '0';
            caption.style.fontSize = '13px';
            const image = document.createElement('img');
            image.src = item.image;
            image.alt = item.label;
            image.style.height = '64px';
            image.style.width = 'auto';
            image.style.maxWidth = '120px';
            image.style.objectFit = 'contain';
            image.style.background = '#ffffff';
            if (side.mirror) image.style.transform = 'scaleX(-1)';
            row.appendChild(input);
            row.appendChild(caption);
            row.appendChild(image);
            side.optionsEl.appendChild(row);
        }
        return selected;
    }

    const api = {
        root: root,
        refs: {
            root: root,
            titleEl: title,
            lengthInput: lengthInput,
            widthInput: widthInput,
            heightInput: heightInput,
            groupInput: groupInput,
            applyAllCheckbox: applyAll.input,
            applyConfigAllCheckbox: applyConfig.input,
            bracketInCheckbox: bracketIn.input,
            bracketOutCheckbox: bracketOut.input,
            stiffInCheckbox: stiffIn.input,
            stiffOutCheckbox: stiffOut.input,
            panel750Checkbox: panel750.input,
            verticalPanelsCheckbox: verticalPanels.input,
            propInCheckbox: propIn.input,
            propOutCheckbox: propOut.input,
            propInInfCheckbox: propInInf.input,
            propOutInfCheckbox: propOutInf.input,
            tapeIniSolapeCheckbox: tapeIniSection.solapeCheckbox,
            tapeSolapeCheckbox: tapeEndSection.solapeCheckbox,
            tapeHeadingEl: tapeHeadingEl,
            tapeMessageEl: tapeMessageEl,
            tapeIniOptionsEl: tapeIniSection.optionsEl,
            tapeOptionsEl: tapeEndSection.optionsEl,
            muroPane: muroPane,
            tapesPane: tapesPane,
            propsPane: propsPane,
            acceptBtn: acceptBtn,
            cancelBtn: cancelBtn,
            closeBtn: closeBtn,
        },
        setGroups: function (list, selected) {
            groupCatalog = Array.isArray(list) ? list : [];
            fillGroupSelect(selected != null ? selected : groupInput.value);
        },
        setSelection: function (ini, end) {
            tapeIniSelectedId = ini || '';
            tapeEndSelectedId = end || '';
        },
        paint: function (ud) {
            const catalog = blocks();
            const thick = catalog[blockKey(widthMeters(ud), false)] || catalog.end03;
            const iniSolape = !!tapeIniSection.solapeCheckbox.checked;
            const endSolape = !!tapeEndSection.solapeCheckbox.checked;
            if (iniSolape && endSolape) tapeMessageEl.textContent = 'El ancho de muro tiene solape';
            else if (!iniSolape && !endSolape) tapeMessageEl.textContent = thick.text;
            else tapeMessageEl.textContent = thick.thickness || thick.text;
            if (ud && options.headingFromUd) tapeHeadingEl.textContent = options.headingFromUd(ud) || '';
            const storedIni = knownId(ud && ud._Tape_1) ? ud._Tape_1 : '';
            const storedEnd = knownId(ud && ud._Tape_2) ? ud._Tape_2 : (knownId(ud && ud._Tape_0) ? ud._Tape_0 : '');
            tapeIniSelectedId = paintSide({
                ud: ud,
                checkbox: tapeIniSection.solapeCheckbox,
                optionsEl: tapeIniSection.optionsEl,
                selectedId: tapeIniSelectedId,
                storedId: storedIni,
                mirror: false,
                onSelect: function (id) {
                    tapeIniSelectedId = id;
                    notifyTape();
                },
            });
            tapeEndSelectedId = paintSide({
                ud: ud,
                checkbox: tapeEndSection.solapeCheckbox,
                optionsEl: tapeEndSection.optionsEl,
                selectedId: tapeEndSelectedId,
                storedId: storedEnd,
                mirror: true,
                onSelect: function (id) {
                    tapeEndSelectedId = id;
                    notifyTape();
                },
            });
            notifyTape();
        },
        showMessage: function (text) {
            messageEl.textContent = text || '';
            messageEl.style.display = text ? 'block' : 'none';
        },
        read: function () {
            const lengthM = options.parseMeters ? options.parseMeters(lengthInput.value) : parseMeters(lengthInput.value);
            const widthM = options.parseMeters ? options.parseMeters(widthInput.value) : parseMeters(widthInput.value);
            const heightM = options.parseMeters ? options.parseMeters(heightInput.value) : parseMeters(heightInput.value);
            if (lengthM == null || widthM == null || heightM == null || heightM <= 0) return null;
            return {
                lengthM: lengthM,
                widthM: widthM,
                heightM: heightM,
                group: parseGroupNumber(groupInput.value),
                heightAll: !!applyAll.input.checked,
                configAll: !!applyConfig.input.checked,
                bracketIn: !!bracketIn.input.checked,
                bracketOut: !!bracketOut.input.checked,
                stiffIn: !!stiffIn.input.checked,
                stiffOut: !!stiffOut.input.checked,
                panel750: !!panel750.input.checked,
                verticalPanels: !!verticalPanels.input.checked,
                propIn: !!propIn.input.checked,
                propOut: !!propOut.input.checked,
                propInInf: !!propInInf.input.checked,
                propOutInf: !!propOutInf.input.checked,
                tapeIni: tapeIniSelectedId || '',
                tapeEnd: tapeEndSelectedId || '',
                solapeIni: !!tapeIniSection.solapeCheckbox.checked,
                solapeEnd: !!tapeEndSection.solapeCheckbox.checked,
            };
        },
        applyModel: function (model) {
            model = model || {};
            lengthInput.value = formatMeters(model.lengthM);
            widthInput.value = formatMeters(model.widthM);
            heightInput.value = formatMeters(model.heightM != null ? model.heightM : 2.70);
            if (Array.isArray(model.groups)) groupCatalog = model.groups;
            fillGroupSelect(model.group);
            applyAll.input.checked = model.heightAll !== false;
            applyConfig.input.checked = !!model.configAll;
            bracketIn.input.checked = model.bracketIn !== false;
            bracketOut.input.checked = model.bracketOut !== false;
            stiffIn.input.checked = model.stiffIn !== false;
            stiffOut.input.checked = model.stiffOut !== false;
            panel750.input.checked = model.panel750 !== false;
            verticalPanels.input.checked = model.verticalPanels !== false;
            propIn.input.checked = model.propIn !== false;
            propOut.input.checked = model.propOut !== false;
            propInInf.input.checked = model.propInInf !== false;
            propOutInf.input.checked = model.propOutInf !== false;
            tapeIniSection.solapeCheckbox.checked = !!model.solapeIni || model.tapeIni === 'TapeS1' || model.tapeIni === 'TapeS2';
            tapeEndSection.solapeCheckbox.checked = !!model.solapeEnd || model.tapeEnd === 'TapeS1' || model.tapeEnd === 'TapeS2';
            tapeIniSelectedId = model.tapeIni || '';
            tapeEndSelectedId = model.tapeEnd || '';
            tapeHeadingEl.textContent = model.heading || '';
            api.showMessage('');
            api.paint({
                _Tape_1: model.tapeIni || '',
                _Tape_2: model.tapeEnd || '',
                _DataWith: widthMeters(null),
            });
        },
    };

    return api;
}

function clamp(value, min, max) {
    return Math.max(min, Math.min(max, value));
}

function parseGroupNumber(text) {
    if (text == null) return null;
    const s = String(text).trim().replace(',', '.');
    if (!s) return null;
    const n = Number(s);
    if (!Number.isFinite(n) || n < 0) return null;
    return Math.round(n);
}

function parseMeters(text) {
    if (text == null) return null;
    let s = String(text).trim().replace(',', '.');
    if (!s) return null;
    const n = Number(s);
    return Number.isFinite(n) ? n : null;
}

function formatMeters(value) {
    const n = Number(value);
    if (!Number.isFinite(n)) return '';
    return String(Math.round(n * 1000) / 1000);
}

function bootWallFormHost() {
    const host = document.getElementById('tandem-wall-form-host');
    if (!host || !window.chrome || !window.chrome.webview) return;
    const form = mountWallEditForm(host, {
        floating: false,
        onAccept: function (values) {
            if (!values) {
                form.showMessage('Longitud, espesor o altura no válidos');
                return;
            }
            form.showMessage('');
            window.chrome.webview.postMessage(JSON.stringify({ action: 'wall-form-accept', values: values }));
        },
        onCancel: function () {
            window.chrome.webview.postMessage(JSON.stringify({ action: 'wall-form-cancel' }));
        },
    });
    window.chrome.webview.addEventListener('message', function (ev) {
        let data = ev.data;
        if (typeof data === 'string') {
            try { data = JSON.parse(data); } catch (e) { return; }
        }
        if (!data || data.action !== 'wall-form-values') return;
        form.applyModel(data);
        const params = new URLSearchParams(window.location.search);
        const designId = Number(params.get('designId') || '');
        if (!Number.isFinite(designId) || designId <= 0) return;
        fetch('/Desing_2/ListDesignGroups?designId=' + encodeURIComponent(String(designId)), { credentials: 'same-origin' })
            .then(function (response) { return response.json(); })
            .then(function (payload) {
                if (!payload || payload.ok === false) return;
                form.setGroups(payload.groups || [], data.group);
            })
            .catch(function () { });
    });
    window.chrome.webview.postMessage(JSON.stringify({ action: 'wall-form-ready' }));
}

bootWallFormHost();
