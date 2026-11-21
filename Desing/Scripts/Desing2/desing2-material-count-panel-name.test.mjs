import {
    catalogFromServerPayload,
    firstSyllablesEndIndex,
    isPanelArticle,
    materialCountColorPanelNameHtml,
    spanishSyllables,
    widthMmFromPanelName
} from './desing2-material-count-panel-name.js';

const catalog = {
    900: '#000000',
    600: '#FF0000',
    450: '#0000FF',
    300: '#FF00FF',
    750: '#00FF00'
};

function assertEqual(name, actual, expected) {
    if (actual !== expected) {
        throw new Error(name + ': esperado ' + expected + ', obtenido ' + actual);
    }
}

assertEqual('panel por nombre', isPanelArticle('Panel Universal 2,70 × 0,75', ''), true);
assertEqual('grapa no es panel', isPanelArticle('Grapa rápida', '4110010045'), false);
assertEqual('panel por código Atenko', isPanelArticle('', '3120270075'), true);

assertEqual('ancho 0,75', widthMmFromPanelName('Panel Universal 2,70 × 0,75'), 750);
assertEqual('alto 2,70 no es ancho', widthMmFromPanelName('Panel 2,70'), null);

assertEqual('sílabas Panel', spanishSyllables('Panel').join('-'), 'Pa-nel');
assertEqual('sílabas Universal', spanishSyllables('Universal').join('-'), 'U-ni-ver-sal');

const name = 'Panel Universal 2,70 × 0,75';
const end = firstSyllablesEndIndex(name, 5);
assertEqual('cinco sílabas', name.slice(0, end), 'Panel Univer');

const html = materialCountColorPanelNameHtml(name, '3120270075', catalog);
if (html.indexOf('font-weight:700') < 0 || html.indexOf('#00ff00') < 0 && html.indexOf('#00FF00') < 0) {
    throw new Error('html color: ' + html);
}
if (html.indexOf('Panel Univer') < 0) throw new Error('html sílabas: ' + html);

assertEqual('no panel sin span', materialCountColorPanelNameHtml('Grapa', '4110010045', catalog), 'Grapa');

const mapped = catalogFromServerPayload({ Colors: [{ WidthMm: 750, Color: '#00FF00' }] });
assertEqual('payload 750', mapped[750], '#00FF00');

console.log('OK desing2-material-count-panel-name');
