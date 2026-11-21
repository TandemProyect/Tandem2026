import { register } from 'node:module';

register(new URL('./strip-v-query-loader.mjs', import.meta.url).href, {
    parentURL: import.meta.url,
});

const {
    maStlWallAiCanShowSimultaneousTranslation,
    maStlWallAiDetectLanguage,
    maStlWallAiHandleMessage,
    maStlWallAiOfflineTranslateToEn,
    maStlWallAiTranslateToEn,
    maStlWallAiTranslatePref,
    maStlWallAiSetTranslatePref,
    MA_STL_WALL_AI_TRANSLATE_VOLUME,
    MA_STL_WALL_AI_SPEECH_VOLUME,
    MA_STL_WALL_AI_TRANSLATE_FUNCTION,
    MA_STL_WALL_AI_ES_RATE_WHEN_TRANSLATE,
    maStlWallAiSpeechRate,
    maStlWallAiSpeechVolume,
    maStlWallAiTexts,
    maStlWallAiIsSpeechStopCommand,
    maStlWallAiTranslationSpeechText,
    maStlWallAiUserTranslationSpeechText,
} = await import('../MasterArticles/wall-ai-assistant.js');

function assertEqual(name, actual, expected) {
    if (actual !== expected) {
        throw new Error(name + ': esperado ' + JSON.stringify(expected) + ', obtenido ' + JSON.stringify(actual));
    }
}

function assert(name, cond) {
    if (!cond) throw new Error(name);
}

assertEqual('función de derechos', MA_STL_WALL_AI_TRANSLATE_FUNCTION, 'IA.TraduccionSimultanea');
assertEqual('volumen inglés al máximo', MA_STL_WALL_AI_TRANSLATE_VOLUME, 1);
assertEqual('voz original más baja que traducción', MA_STL_WALL_AI_SPEECH_VOLUME, 0.7);

assert('sin TandemFunctions se muestra', maStlWallAiCanShowSimultaneousTranslation(null) === true);
assert('can true se muestra', maStlWallAiCanShowSimultaneousTranslation({ can: function () { return true; } }) === true);
assert('can false se oculta', maStlWallAiCanShowSimultaneousTranslation({ can: function (c) { return c !== 'IA.TraduccionSimultanea'; } }) === false);

assertEqual('vacío', maStlWallAiOfflineTranslateToEn(''), '');
assertEqual('ya inglés', maStlWallAiOfflineTranslateToEn("I don't have panels"), "I don't have panels");
const order = maStlWallAiOfflineTranslateToEn('dame el material de este muro');
assert('orden azul contiene wall', /wall/i.test(order));
assert('orden azul contiene give me', /give me/i.test(order));

const mem = {
    data: {},
    getItem: function (k) { return Object.prototype.hasOwnProperty.call(this.data, k) ? this.data[k] : null; },
    setItem: function (k, v) { this.data[k] = String(v); },
};
assertEqual('pref por defecto off', maStlWallAiTranslatePref(mem), false);
maStlWallAiSetTranslatePref(true, mem);
assertEqual('pref on', maStlWallAiTranslatePref(mem), true);

const hooked = await maStlWallAiTranslateToEn('hola muro', {
    window: { maStlWallAiTranslateToEnImpl: function () { return 'hello wall'; } },
});
assertEqual('gancho de prueba', hooked, 'hello wall');

assertEqual('el detector reconoce una palabra funcional inglesa', maStlWallAiDetectLanguage('the', 'es').lang, 'en');
let forcedTranslatorOptions = null;
const forcedVoiceTranslation = await maStlWallAiTranslateToEn('the', {
    sourceLanguage: 'es',
    window: {
        Translator: {
            create: function (options) {
                forcedTranslatorOptions = options;
                return Promise.resolve({ translate: function () { return Promise.resolve('el'); } });
            },
        },
    },
});
assertEqual('la voz ES fuerza la traducción aunque el texto parezca inglés', forcedVoiceTranslation, 'el');
assertEqual('el traductor recibe español como origen', forcedTranslatorOptions.sourceLanguage, 'es');
assertEqual('dictado inglés no se retraduce', await maStlWallAiTranslateToEn('the', { sourceLanguage: 'en' }), 'the');

assertEqual('rate ES+translate', maStlWallAiSpeechRate('es', true), MA_STL_WALL_AI_ES_RATE_WHEN_TRANSLATE);
assertEqual('rate ES sin translate', maStlWallAiSpeechRate('es', false), 1);
assertEqual('rate EN+translate', maStlWallAiSpeechRate('en', true), 1);
assertEqual('rate constante 1.15', MA_STL_WALL_AI_ES_RATE_WHEN_TRANSLATE, 1.15);
assertEqual('voz ES atenuada con traducción', maStlWallAiSpeechVolume('es', true, 1), 0.7);
assertEqual('voz ES normal sin traducción', maStlWallAiSpeechVolume('es', false, 1), 1);
assertEqual('voz inglesa al máximo', maStlWallAiSpeechVolume('en', true, 1), 1);
assertEqual('volumen inglés respeta el máximo actual', maStlWallAiSpeechVolume('en', true, 0.6), 0.6);
assertEqual('Hecho. → Done.', maStlWallAiOfflineTranslateToEn('Hecho.'), 'Done.');
assertEqual('texto éxito ES', maStlWallAiTexts('es').done, 'Hecho.');
assertEqual('texto éxito EN', maStlWallAiTexts('en').done, 'Done.');
assertEqual('lee respuesta corta según idioma de origen', maStlWallAiTranslationSpeechText('Hecho.', 'Done.', 'es'), 'Done.');
assertEqual('no duplica respuesta inglesa', maStlWallAiTranslationSpeechText('Done.', 'Done.', 'en'), '');
assertEqual('no lee traducción vacía', maStlWallAiTranslationSpeechText('Hecho.', '', 'es'), '');
assertEqual('lee la orden azul dictada en español', maStlWallAiUserTranslationSpeechText('a cota muros', 'dimension walls', 'es'), 'dimension walls');
assertEqual('no lee la orden si ya estaba en inglés', maStlWallAiUserTranslationSpeechText('dimension walls', 'dimension walls', 'en'), '');
assertEqual('no lee orden sin traducción', maStlWallAiUserTranslationSpeechText('a cota muros', '', 'es'), '');

assert('stop interrumpe', maStlWallAiIsSpeechStopCommand('Stop'));
assert('para interrumpe', maStlWallAiIsSpeechStopCommand('para'));
assert('vale interrumpe', maStlWallAiIsSpeechStopCommand('vale'));
assert('déjalo interrumpe sin tilde', maStlWallAiIsSpeechStopCommand('déjalo'));
assert('admite por favor', maStlWallAiIsSpeechStopCommand('déjalo, por favor'));
assert('no toma órdenes como stop', !maStlWallAiIsSpeechStopCommand('para el muro'));
assert('no toma frases largas como stop', !maStlWallAiIsSpeechStopCommand('vale, haz otra cosa'));

const unknown = await maStlWallAiHandleMessage({}, 'xyzqwerty', []);
assertEqual('desconocido del muro sin explicación larga', unknown.reply, 'No te he entendido.');
assertEqual('desconocido del muro voz breve', unknown.speech, 'No te he entendido.');

console.log('OK wall-ai-simultaneous-translation 39 casos');
