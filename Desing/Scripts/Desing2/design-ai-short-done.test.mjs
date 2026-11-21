import { register } from 'node:module';

register(new URL('./strip-v-query-loader.mjs', import.meta.url).href, {
    parentURL: import.meta.url,
});

const {
    maStlDesignAiParse,
    maStlDesignAiBestCommand,
    maStlDesignAiHandleMessage,
    maStlDesignAiNewSession,
    maStlDesignAiDoneReply,
    maStlDesignAiTexts,
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

function wall7() {
    return {
        id: 7,
        label: '7',
        lengthMm: 5000,
        thicknessMm: 200,
        heightMm: 2700,
        p1: { x: 0, z: 0 },
        p2: { x: 5000, z: 0 },
    };
}

function mockBridge(extra) {
    const w = wall7();
    const calls = { delete: 0, undo: 0, redo: 0 };
    const B = {
        isReady: function () { return true; },
        walls: function () { return [w]; },
        selectedWall: function () { return w; },
        highlight: function () { },
        clearHighlight: function () { },
        ensureLinesMode: function () { return { changed: false }; },
        hasFormwork: function () { return false; },
        deleteWall: function (id) {
            calls.delete += 1;
            return { ok: String(id) === '7' };
        },
        canUndo: function () { return true; },
        undo: function () { calls.undo += 1; return true; },
        redo: function () { calls.redo += 1; return true; },
        defaults: function () { return { thicknessMm: 200, heightMm: 2700, minLengthMm: 100 }; },
        origin: function () { return { x: 0, z: 0 }; },
        showWallIds: function () { return { ok: true }; },
        showCotas: function () { return { ok: true }; },
        showAllCotas: function () { return { ok: true }; },
        dimensionWall: function () { return { ok: true, wall: w }; },
        calls: calls,
    };
    return Object.assign(B, extra || {});
}

function cmdId(text) {
    const b = maStlDesignAiBestCommand(maStlDesignAiParse(text));
    return b && b.cmd ? b.cmd.id : null;
}

assertEqual('atras → undo', cmdId('atras'), 'undo');
assertEqual('alante → redo', cmdId('alante'), 'redo');
assertEqual('adelante → redo', cmdId('adelante'), 'redo');
assertEqual('mueve el muro atras no es undo', cmdId('mueve el muro atras'), 'wall.move');

assertEqual('Hecho. ES', maStlDesignAiDoneReply('es').reply, 'Hecho.');
assertEqual('Done. EN', maStlDesignAiDoneReply('en').reply, 'Done.');
assertEqual('speech ES', maStlDesignAiDoneReply('es').speech, 'Hecho.');

const B = mockBridge();
const s = maStlDesignAiNewSession(B);
const del = maStlDesignAiHandleMessage(s, 'borra el muro 7', []);
assertEqual('borrar sin Sí/No', del.reply, 'Hecho.');
assertEqual('borrar speech', del.speech, 'Hecho.');
assert('borrar sin botones', !del.buttons);
assertEqual('deleteWall llamado', B.calls.delete, 1);
assert('borrar no pregunta', /¿|Shall I/i.test(String(del.reply)) === false);

const Bfail = mockBridge({
    deleteWall: function () { this.calls.delete += 1; return { ok: false }; },
});
const sFail = maStlDesignAiNewSession(Bfail);
const delFail = maStlDesignAiHandleMessage(sFail, 'borra el muro 7', []);
assert('fallo no es Hecho', delFail.reply !== 'Hecho.');
assert('fallo explica', /no he podido borrar|could not delete/i.test(delFail.reply));

const Bundo = mockBridge();
const sUndo = maStlDesignAiNewSession(Bundo);
const und = maStlDesignAiHandleMessage(sUndo, 'atras', []);
assertEqual('atras ejecuta undo', und.reply, 'Hecho.');
assertEqual('undo llamado', Bundo.calls.undo, 1);

const Bredo = mockBridge();
const sRedo = maStlDesignAiNewSession(Bredo);
const red = maStlDesignAiHandleMessage(sRedo, 'alante', []);
assertEqual('alante ejecuta redo', red.reply, 'Hecho.');
assertEqual('redo llamado', Bredo.calls.redo, 1);

const Bask = mockBridge({ selectedWall: function () { return null; } });
const sAsk = maStlDesignAiNewSession(Bask);
sAsk.lastWallId = null;
const ask = maStlDesignAiHandleMessage(sAsk, 'longitud 4,5', []);
assertEqual('sin muro no explica', ask.reply, 'No te he entendido.');
assertEqual('sin muro voz breve', ask.speech, 'No te he entendido.');
assert('ask no es Hecho', ask.reply !== 'Hecho.');

const dimsAll = maStlDesignAiHandleMessage(maStlDesignAiNewSession(Bask), 'a cota muros', []);
assertEqual('acota sin selección hace y calla', dimsAll.reply, 'Hecho.');

const ramble = maStlDesignAiHandleMessage(maStlDesignAiNewSession(B), 'el cumple de mi mes pasado y antes de que digas que es mi hermano mira como lo', []);
assertEqual('frase larga no entendida', ramble.reply, 'No te he entendido.');
assert('orden larga no se toma como comando débil', maStlDesignAiCommandIsClear(maStlDesignAiParse('el cumple de mi mes pasado y antes de que digas que es mi hermano mira como lo'), maStlDesignAiBestCommand(maStlDesignAiParse('el cumple de mi mes pasado y antes de que digas que es mi hermano mira como lo'))) === false);

const unknown = maStlDesignAiHandleMessage(maStlDesignAiNewSession(B), 'xyzqwerty', []);
assertEqual('desconocido no despliega ayuda extensa', unknown.reply, 'No te he entendido.');
assertEqual('desconocido voz breve', unknown.speech, 'No te he entendido.');
const unknownEnSession = maStlDesignAiNewSession(B);
unknownEnSession.lang = 'en';
const unknownEn = maStlDesignAiHandleMessage(unknownEnSession, 'xyzqwerty', []);
assertEqual('unknown reply concise in English', unknownEn.reply, 'I did not understand.');
assertEqual('ask and success remain distinct', maStlDesignAiDoneReply('es').reply, 'Hecho.');

console.log('OK design-ai-short-done 24 casos');
