import { formworkArticleColorHex, formworkColorModePaints, formworkKeepsFactoryRole, formworkMeshColorHex, formworkModeClickAction, formworkPanelColorHex, formworkParseColorHex, formworkResolveSystemPanelColor, formworkSystemPanelColorHex, wallDimKindWithoutWall } from './formwork-panel-color.js';

const catalog = {
    900: '#000000',
    600: '#FF0000',
    450: '#0000FF',
    300: '#FF00FF',
    750: '#00FF00',
};

function assertEqual(name, actual, expected) {
    if (actual !== expected) {
        throw new Error(name + ': esperado ' + expected + ', obtenido ' + actual);
    }
}

assertEqual('0,90 negro', formworkPanelColorHex(catalog, 900, 'Panel', true), 0x000000);
assertEqual('0,60 con redondeo', formworkPanelColorHex(catalog, 600.4, 'Panel', true), 0xff0000);
assertEqual('0,45 azul', formworkPanelColorHex(catalog, 450, 'panel', true), 0x0000ff);
assertEqual('ancho sin catálogo', formworkPanelColorHex(catalog, 1200, 'Panel', true), null);
assertEqual('grapa no se pinta', formworkPanelColorHex(catalog, 900, 'Union', true), null);
assertEqual('modo apagado', formworkPanelColorHex(catalog, 900, 'Panel', false), null);
assertEqual('ancho vacío', formworkPanelColorHex(catalog, 0, 'Panel', true), null);
assertEqual('color inválido', formworkParseColorHex('rojo'), null);
assertEqual('artículo 0,60 rojo', formworkArticleColorHex('#FF0000', 'Panel', true), 0xff0000);
assertEqual('cero del maestro no pinta', formworkArticleColorHex('0', 'Panel', true), null);
assertEqual('grapa con color no pinta', formworkArticleColorHex('#00FF00', 'Union', true), null);
assertEqual('modo colores apagado', formworkArticleColorHex('#0000FF', 'Panel', false), null);
assertEqual('modo colores usa el negro del maestro', formworkArticleColorHex('#000000', 'Panel', true, 'Vertical'), 0x000000);
assertEqual('modo colores no pinta el suplemento', formworkArticleColorHex('#000000', 'Panel', true, 'Tumbado'), null);
assertEqual('modo colores sin orientación sigue pintando', formworkArticleColorHex('#000000', 'Panel', true), 0x000000);
assertEqual('modo colores pinta panel vertical', formworkColorModePaints('Panel', 'Vertical'), true);
assertEqual('modo colores no toca el suplemento', formworkColorModePaints('Panel', 'Tumbado'), false);
assertEqual('modo colores no toca la grapa', formworkColorModePaints('Union', 'Vertical'), false);
assertEqual('puntal exterior conserva su color', formworkKeepsFactoryRole('prop'), true);
assertEqual('tubo interior del puntal conserva su color', formworkKeepsFactoryRole('prop-inner'), true);
assertEqual('un panel no es puntal', formworkKeepsFactoryRole('frame'), false);
assertEqual('sistema pinta panel vertical', formworkSystemPanelColorHex('#00FF00', 'Panel', 'Vertical'), 0x00ff00);
assertEqual('suplemento tumbado no pinta', formworkSystemPanelColorHex('#00FF00', 'Panel', 'Tumbado'), null);
assertEqual('panel sin orientación no pinta', formworkSystemPanelColorHex('#00FF00', 'Panel', ''), null);
assertEqual('sistema vacío no pinta', formworkSystemPanelColorHex('', 'Panel', 'Vertical'), null);
assertEqual('sistema inválido no pinta', formworkSystemPanelColorHex('verde', 'Panel', 'Vertical'), null);
assertEqual('sistema no pinta grapa', formworkSystemPanelColorHex('#FF0000', 'Union', 'Vertical'), null);
assertEqual('el color del elemento gana', formworkResolveSystemPanelColor({ SystemPanelColor: '#FF0000' }, '#00FF00'), '#FF0000');
assertEqual('si el elemento no trae color, usa el del sistema', formworkResolveSystemPanelColor({ Orientation: 'Vertical' }, '#00FF00'), '#00FF00');
assertEqual('sin color de sistema no hay fallback', formworkResolveSystemPanelColor(null, ''), '');
assertEqual('el fenólico no coge el color del sistema', formworkMeshColorHex('phenolic', 0x00ff00, 0xefb608, 0x1a1816), 0x1a1816);
assertEqual('la estructura coge el color del sistema', formworkMeshColorHex('frame', 0x00ff00, 0xefb608, 0x1a1816), 0x00ff00);
assertEqual('sin color de sistema la estructura queda de fábrica', formworkMeshColorHex('frame', null, 0xefb608, 0x1a1816), 0xefb608);
assertEqual('grapa y piezas sueltas no se repintan', formworkMeshColorHex('other', 0x00ff00, 0xefb608, 0x1a1816), null);

import fs from 'node:fs';
import vm from 'node:vm';

const wiring = fs.readFileSync(new URL('./desing2-stl-viewer-toolbar-wiring.js', import.meta.url), 'utf8');
const latchStart = wiring.indexOf('function formworkColorLatch');
const latchEnd = wiring.indexOf('\n        const systemToggle', latchStart);
if (latchStart < 0 || latchEnd < 0) throw new Error('no está formworkColorLatch');
const latch = vm.runInNewContext(wiring.slice(latchStart, latchEnd) + '\nformworkColorLatch');

assertEqual('clic Colores enciende', latch(true, false, false), true);
assertEqual('el segundo clic de fase no apaga Colores', latch(false, true, true), true);
assertEqual('clic Encofrar apaga Colores', latch(false, false, true), false);
assertEqual('la fase de Encofrar sigue apagada', latch(false, true, false), false);
assertEqual('sin encofrado hay que calcular', formworkModeClickAction(false, true, false), 'solve');
assertEqual('colores sobre encofrado solo repinta', formworkModeClickAction(true, true, false), 'repaint-color');
assertEqual('volver a Encofrar solo repinta', formworkModeClickAction(true, false, true), 'repaint-system');
assertEqual('Encofrar otra vez sigue calculando', formworkModeClickAction(true, false, false), 'solve');
assertEqual('sin muro el id sigue', wallDimKindWithoutWall('wall-id', { panels: false }), true);
assertEqual('sin muro los paneles marcados se acotan', wallDimKindWithoutWall('panels', { panels: true }), true);
assertEqual('sin muro los paneles apagados no', wallDimKindWithoutWall('panels', { panels: false }), false);
assertEqual('sin muro la longitud no', wallDimKindWithoutWall('walls-total', { 'walls-total': true }), false);

console.log('OK formwork-panel-color 46 casos');
