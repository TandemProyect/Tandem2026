import { register } from 'node:module';

register(new URL('./strip-v-query-loader.mjs', import.meta.url).href, {
    parentURL: import.meta.url,
});

const {
    maStlDesignAiParse,
    maStlDesignAiLex,
    maStlDesignAiBestCommand,
    maStlDesignAiHandleMessage,
    maStlDesignAiNewSession,
    maStlDesignAiCommandIsClear,
} = await import('../MasterArticles/design-ai-assistant.js');

function assertEqual(name, actual, expected) {
    if (actual !== expected) {
        throw new Error(name + ': esperado ' + JSON.stringify(expected) + ', obtenido ' + JSON.stringify(actual));
    }
}

function assert(name, cond) {
    if (!cond) throw new Error(name);
}

function assertNear(name, actual, expected, tol) {
    if (Math.abs(Number(actual) - Number(expected)) > (tol || 1)) {
        throw new Error(name + ': esperado ~' + expected + ', obtenido ' + actual);
    }
}

function mockBridge(extra) {
    const calls = { walls: [], pillars: [] };
    const B = {
        isReady: function () { return true; },
        walls: function () { return []; },
        selectedWall: function () { return null; },
        highlight: function () { },
        clearHighlight: function () { },
        ensureLinesMode: function () { return { changed: false }; },
        hasFormwork: function () { return false; },
        defaults: function () {
            return { thicknessMm: 200, heightMm: 2700, minLengthMm: 100, minThicknessMm: 100, maxThicknessMm: 800 };
        },
        origin: function () { return { x: 0, y: 0, z: 0 }; },
        drawWall: function (a, b, opts) {
            calls.walls.push({ a: a, b: b, opts: opts || {} });
            return {
                ok: true,
                walls: [{
                    id: 1, label: '1', lengthMm: 10000, thicknessMm: 300, heightMm: 3120,
                    p1: { x: a.x, z: a.z }, p2: { x: b.x, z: b.z },
                }],
            };
        },
        drawPillar: function (opts) {
            calls.pillars.push(opts);
            return { ok: true };
        },
        calls: calls,
    };
    return Object.assign(B, extra || {});
}

function cmdId(text) {
    const b = maStlDesignAiBestCommand(maStlDesignAiParse(text));
    return b && b.cmd ? b.cmd.id : null;
}

function run(text) {
    const B = mockBridge();
    const s = maStlDesignAiNewSession(B);
    const r = maStlDesignAiHandleMessage(s, text, []);
    return { B: B, r: r, p: maStlDesignAiParse(text) };
}

/* Léxico: 0,0,0 no es 0,30. */
const xyzToks = maStlDesignAiLex('en el punto 0,0,0');
assert('lex xyz', xyzToks.some(function (t) { return t.t === 'xyz' && t.x === 0 && t.y === 0 && t.z === 0; }));
const decToks = maStlDesignAiLex('espesor 0,30');
assert('lex 0,30 decimal', decToks.some(function (t) { return t.t === 'n' && t.v === 0.3; }));
assert('lex 0,30 no xyz', !decToks.some(function (t) { return t.t === 'xyz'; }));

const p1 = maStlDesignAiParse('inserta un muro en el punto 0,0,0 con 10.03 mts de longitud 0,30 de ancho y 3,12 de altura');
assert('parse xyz', p1.xyz && p1.xyz.x === 0 && p1.xyz.z === 0);
assertEqual('cmd muro largo', cmdId('inserta un muro en el punto 0,0,0 con 10.03 mts de longitud 0,30 de ancho y 3,12 de altura'), 'wall.draw');
assert('frase larga clara', maStlDesignAiCommandIsClear(p1, maStlDesignAiBestCommand(p1)));

const pBox = maStlDesignAiParse('Inserta muro 10 x 0,30 x 3,12');
assert('box largo', pBox.box && pBox.box.length.v === 10);
assertNear('box espesor', pBox.box.thickness.v, 0.3, 0.001);
assertNear('box alto', pBox.box.height.v, 3.12, 0.001);
assertEqual('cmd box muro', cmdId('Inserta muro 10 x 0,30 x 3,12'), 'wall.draw');
assertEqual('cmd ángulo 0 por omisión', cmdId('Inserta un muro en 0,0,0 de 10 x 0,30 x 4,0 a 30 grados'), 'wall.draw');
assertEqual('cmd pilar', cmdId('inserta un pilar en 0,0,0 de 0,9 x 0,9 x 5'), 'pillar.insert');

const w1 = run('inserta un muro en el punto 0,0,0 con 10.03 mts de longitud 0,30 de ancho y 3,12 de altura');
assertEqual('muro hecho', w1.r.reply, 'Hecho.');
assertEqual('un muro', w1.B.calls.walls.length, 1);
assertNear('largo 10.03 m', w1.B.calls.walls[0].opts.heightM != null ? Math.hypot(w1.B.calls.walls[0].b.x - w1.B.calls.walls[0].a.x, w1.B.calls.walls[0].b.z - w1.B.calls.walls[0].a.z) : 0, 10030, 2);
assertNear('espesor 0,30', w1.B.calls.walls[0].opts.thicknessMm, 300, 1);
assertNear('alto 3,12', w1.B.calls.walls[0].opts.heightM, 3.12, 0.01);
assertNear('origen x', w1.B.calls.walls[0].a.x, 0, 1);
assertNear('ángulo 0 → +X', w1.B.calls.walls[0].b.z, 0, 1);

const w2 = run('Inserta muro 10 x 0,30 x 3,12');
assertEqual('box hecho', w2.r.reply, 'Hecho.');
assertNear('box largo 10 m', Math.hypot(w2.B.calls.walls[0].b.x - w2.B.calls.walls[0].a.x, w2.B.calls.walls[0].b.z - w2.B.calls.walls[0].a.z), 10000, 2);
assertNear('box espesor', w2.B.calls.walls[0].opts.thicknessMm, 300, 1);
assertNear('box alto', w2.B.calls.walls[0].opts.heightM, 3.12, 0.01);

const w3 = run('Inserta un muro en 0,0,0 de 10 x 0,30 x 4,0 a 30 grados');
assertEqual('ángulo hecho', w3.r.reply, 'Hecho.');
assertNear('30° x', w3.B.calls.walls[0].b.x, 10000 * Math.cos(30 * Math.PI / 180), 3);
assertNear('30° z', w3.B.calls.walls[0].b.z, 10000 * -Math.sin(30 * Math.PI / 180), 3);
assertNear('alto 4', w3.B.calls.walls[0].opts.heightM, 4, 0.01);

const miss = run('inserta un muro');
assertEqual('sin medidas no pregunta', miss.r.reply, 'No te he entendido.');
assertEqual('sin medidas no dibuja', miss.B.calls.walls.length, 0);

const pil = run('inserta un pilar en 0,0,0 de 0,9 x 0,9 x 5');
assertEqual('pilar hecho', pil.r.reply, 'Hecho.');
assertEqual('un pilar', pil.B.calls.pillars.length, 1);
assertNear('pilar clamp largo', pil.B.calls.pillars[0].longMm, 650, 1);
assertNear('pilar clamp ancho', pil.B.calls.pillars[0].widthMm, 650, 1);
assertNear('pilar alto 5', pil.B.calls.pillars[0].heightM, 5, 0.01);
assertNear('pilar origen', pil.B.calls.pillars[0].x, 0, 1);
assertEqual('pilar ángulo 0', pil.B.calls.pillars[0].angleDeg, 0);

console.log('OK design-ai-insert-wall-pillar');
