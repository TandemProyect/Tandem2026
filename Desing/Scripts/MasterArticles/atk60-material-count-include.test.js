'use strict';

function includeInCount(code, importPath) {
    if (!code || /_PHENOLIC$/i.test(code) || /^MARK_/i.test(code)) return false;
    if (importPath && /_F\.stl$/i.test(String(importPath))) return false;
    code = String(code).replace(/_FRAME$/i, '');
    return !!code;
}

function countCode(items, want) {
    var n = 0;
    for (var i = 0; i < items.length; i++) {
        var item = items[i];
        if (!includeInCount(item.code, item.path)) continue;
        var code = String(item.code).replace(/_FRAME$/i, '');
        if (code === want) n++;
    }
    return n;
}

var failed = 0;
function check(name, got, want) {
    if (got !== want) {
        failed++;
        console.error('FAIL ' + name + ': got ' + got + ' want ' + want);
        return;
    }
    console.log('OK   ' + name);
}

// L 2,70: marco + piel → 1 esquina interior
check('L 2700 one band', countCode([
    { code: 'E27004210', path: 'E27004210.stl' },
    { code: 'E27004210_PHENOLIC', path: 'E27004210_F.stl' },
    { code: '27000000', path: 'EEx27000000.stl' }
], 'E27004210'), 1);

// Payload viejo: misma code en _F.stl
check('L old _F path', countCode([
    { code: 'E27004210', path: 'E27004210.stl' },
    { code: 'E27004210', path: 'E27004210_F.stl' }
], 'E27004210'), 1);

// L apilado 5,40: dos bandas
check('L stacked 5400', countCode([
    { code: 'E27004210', path: 'E27004210.stl' },
    { code: 'E27004210_PHENOLIC', path: 'E27004210_F.stl' },
    { code: 'E27004210', path: 'E27004210.stl' },
    { code: 'E27004210_PHENOLIC', path: 'E27004210_F.stl' }
], 'E27004210'), 2);

// T: dos rincones interiores (no cuatro)
check('T two interiors', countCode([
    { code: 'E27004210', path: 'E27004210.stl' },
    { code: 'E27004210_PHENOLIC', path: 'E27004210_F.stl' },
    { code: 'E27004210', path: 'E27004210.stl' },
    { code: 'E27004210_PHENOLIC', path: 'E27004210_F.stl' }
], 'E27004210'), 2);

function nativeCode(code) {
    return String(code || '').replace(/_PHENOLIC$/i, '').replace(/_FRAME$/i, '');
}
function keepNative(code) {
    return /^(E27004210|E24004217|E12004216|27000000|24000000|12000000)$/.test(nativeCode(code));
}
function checkBool(name, got, want) {
    if (!!got !== !!want) {
        failed++;
        console.error('FAIL ' + name + ': got ' + got + ' want ' + want);
        return;
    }
    console.log('OK   ' + name);
}
checkBool('native frame 270', keepNative('E27004210'), true);
checkBool('native phenolic 270', keepNative('E27004210_PHENOLIC'), true);
checkBool('native not panel 900', keepNative('27904209'), false);

function mapUniversal(code) {
    var m = {
        '27754219': '27104219',
        '24754224': '24104224',
        '12754120': '12104120'
    };
    var key = String(code || '');
    if (key.toUpperCase().indexOf('PANEL_') === 0) key = key.substring(6);
    return m[key] || key;
}
check('univ 270x75 glb', mapUniversal('27754219'), '27104219');
check('univ 240x75 glb', mapUniversal('24754224'), '24104224');
check('univ 120x75 glb', mapUniversal('12754120'), '12104120');
check('univ PANEL_ prefix', mapUniversal('PANEL_27754219'), '27104219');
check('panel 900 unchanged', mapUniversal('27904209'), '27904209');

if (failed) {
    process.exit(1);
}
console.log('atk60-material-count-include: ok');
