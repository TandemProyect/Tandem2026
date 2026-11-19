import fs from 'node:fs';
import vm from 'node:vm';

const file = new URL('./desing2-stl-viewer-toolbar-wiring.js', import.meta.url);
const source = fs.readFileSync(file, 'utf8');

function braceDepthAt(text, index) {
    let depth = 0;
    let quote = '';
    let escape = false;
    let lineComment = false;
    let blockComment = false;
    for (let i = 0; i < index; i++) {
        const ch = text[i];
        const next = text[i + 1];
        if (lineComment) {
            if (ch === '\n') lineComment = false;
            continue;
        }
        if (blockComment) {
            if (ch === '*' && next === '/') {
                blockComment = false;
                i++;
            }
            continue;
        }
        if (quote) {
            if (escape) {
                escape = false;
                continue;
            }
            if (ch === '\\') {
                escape = true;
                continue;
            }
            if (ch === quote) quote = '';
            continue;
        }
        if (ch === '/' && next === '/') {
            lineComment = true;
            i++;
            continue;
        }
        if (ch === '/' && next === '*') {
            blockComment = true;
            i++;
            continue;
        }
        if (ch === '"' || ch === "'" || ch === '`') {
            quote = ch;
            continue;
        }
        if (ch === '{') depth++;
        else if (ch === '}') depth--;
    }
    return depth;
}

function assert(name, condition) {
    if (!condition) throw new Error(name);
}

const offerDecl = source.indexOf('function wireClientOfferOpenFromDesign');
const dropdownDecl = source.indexOf('function wireModeSystemDropdown');
const setActiveDecl = source.indexOf('function setActiveItem');
assert('existe wireClientOfferOpenFromDesign', offerDecl > 0);
assert('la oferta no queda dentro de setActiveItem', offerDecl > setActiveDecl);
assert(
    'la oferta está al mismo nivel que el desplegable de sistema',
    braceDepthAt(source, offerDecl) === braceDepthAt(source, dropdownDecl)
);

const fnSource = source.slice(offerDecl, source.indexOf('\n    function ', offerDecl));
const opened = [];
const clicks = [];

function fakeDocument(button) {
    return {
        getElementById: function (id) {
            if (id === 'ma-stl-open-client-offer') return button;
            if (id === 'ma-stl-mode-system-dropdown') return { textContent: ' ATK-60 ' };
            return null;
        },
    };
}

function run(button) {
    const context = {
        document: fakeDocument(button),
        window: { open: function (url) { opened.push(url); } },
    };
    vm.runInNewContext(fnSource + '\nwireClientOfferOpenFromDesign();', context);
    if (button) button.click();
}

run(null);
assert('sin botón no abre nada', opened.length === 0);

run({
    getAttribute: function () { return ''; },
    addEventListener: function (_name, handler) { clicks.push(handler); },
    click: function () { clicks[clicks.length - 1](); },
});
assert('sin url ni diseño no abre', opened.length === 0);

run({
    getAttribute: function (name) {
        if (name === 'data-ma-client-offer-url') return '/OfertaCliente/Crear';
        return '';
    },
    addEventListener: function (_name, handler) { clicks.push(handler); },
    click: function () { clicks[clicks.length - 1](); },
});
assert('sin designId no abre', opened.length === 0);

run({
    getAttribute: function (name) {
        if (name === 'data-ma-client-offer-url') return '/OfertaCliente/Crear?plantilla=1';
        if (name === 'data-ma-client-offer-design-id') return '42';
        return '';
    },
    addEventListener: function (_name, handler) { clicks.push(handler); },
    click: function () { clicks[clicks.length - 1](); },
});
assert('con datos abre la oferta', opened.length === 1);
assert('conserva la query y añade diseño y sistema', opened[0] === '/OfertaCliente/Crear?plantilla=1&designId=42&systemLabel=ATK-60');

console.log('OK client-offer-wiring 4 casos');
