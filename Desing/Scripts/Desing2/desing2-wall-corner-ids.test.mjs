import { assignCorrelativeWallCornerIds, wallCornerIdText } from './desing2-wall-corner-ids.js';

function assertEqual(name, actual, expected) {
    if (actual !== expected) {
        throw new Error(name + ': esperado ' + expected + ', obtenido ' + actual);
    }
}

const walls = [
    { id: 'bottom', x: 2000, z: 150 },
    { id: 'right', x: 3850, z: 1500 },
    { id: 'top', x: 2000, z: 2850 },
    { id: 'left', x: 150, z: 1500 }
];
const corners = [
    { id: 'bl', x: 150, z: 150 },
    { id: 'br', x: 3850, z: 150 },
    { id: 'tr', x: 3850, z: 2850 },
    { id: 'tl', x: 150, z: 2850 }
];

const assigned = assignCorrelativeWallCornerIds(walls, corners);
assertEqual('8 elementos', assigned.list.length, 8);
assertEqual('empieza en S', assigned.list[0].text, 'S-1');
assertEqual('sigue W', assigned.list[1].text, 'W-2');
assertEqual('textos', assigned.list.map(function (x) { return x.text; }).join(' '), 'S-1 W-2 S-3 W-4 S-5 W-6 S-7 W-8');
assertEqual('BL es S-1', assigned.cornerById.get('bl'), 1);
assertEqual('left es W-2', assigned.wallById.get('left'), 2);
assertEqual('sin S-1 y W-1', assigned.wallById.get('bottom') === 1, false);
assertEqual('números únicos', new Set(assigned.list.map(function (x) { return x.n; })).size, 8);

const onlyWalls = assignCorrelativeWallCornerIds(walls, []);
assertEqual('solo muros 4', onlyWalls.list.length, 4);
assertEqual('solo W', onlyWalls.list.every(function (x) { return x.text.indexOf('W-') === 0; }), true);

const empty = assignCorrelativeWallCornerIds([], []);
assertEqual('vacío', empty.list.length, 0);

assertEqual('texto esquina', wallCornerIdText('corner', 4), 'S-4');
assertEqual('texto muro', wallCornerIdText('wall', 2), 'W-2');

console.log('OK desing2-wall-corner-ids');
