import { register } from 'node:module';

register(new URL('./strip-v-query-loader.mjs', import.meta.url).href, {
    parentURL: import.meta.url,
});

const { maStlDesignAiParse, maStlDesignAiModeTarget } = await import('../MasterArticles/design-ai-assistant.js');

function assertEqual(name, actual, expected) {
    if (actual !== expected) {
        throw new Error(name + ': esperado ' + expected + ', obtenido ' + actual);
    }
}

assertEqual('encofra sigue siendo el encofrado normal', maStlDesignAiModeTarget(maStlDesignAiParse('encofra')), 'formwork');
assertEqual('modo encofrar no es colores', maStlDesignAiModeTarget(maStlDesignAiParse('modo encofrar')), 'formwork');
assertEqual('encofrado colores', maStlDesignAiModeTarget(maStlDesignAiParse('encofrado colores')), 'formworkColor');
assertEqual('cambia a encofrado colores', maStlDesignAiModeTarget(maStlDesignAiParse('cambia a encofrado colores')), 'formworkColor');
assertEqual('cambiar de modelo a encofrado colores', maStlDesignAiModeTarget(maStlDesignAiParse('cambiar de modelo a encofrado colores')), 'formworkColor');
assertEqual('color formwork', maStlDesignAiModeTarget(maStlDesignAiParse('color formwork')), 'formworkColor');
assertEqual('genera 3D no es colores', maStlDesignAiModeTarget(maStlDesignAiParse('genera 3D')), 'wall3d');
assertEqual('colores suelto', maStlDesignAiModeTarget(maStlDesignAiParse('colores')), 'formworkColor');

console.log('OK formwork-color-ai-mode 8 casos');
