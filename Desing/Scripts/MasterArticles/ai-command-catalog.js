/**
 * Catálogo de comandos de la IA desde la BD (pt107u, 14/11/2026).
 * Carga `Desing_2/AiCommands` (comandos activos, palabras efectivas del usuario y plantillas de respuesta de su
 * entorno de conversación), lo guarda en localStorage por contexto + idioma con su versión (el servidor devuelve
 * `notModified` si la versión no ha cambiado) y avisa a los intérpretes (IA del diseño y del muro) para que lo usen.
 * Si no hay URL, el servidor falla o faltan las tablas, no pasa nada: los intérpretes siguen con su vocabulario JS.
 * Sin dependencias (no importa wall-ai-assistant.js para no crear ciclos).
 */

export const MA_STL_AI_CATALOG_STORAGE = 'ma-stl-ai-catalog:v1:';

/** Valores por defecto (pruebas u otras páginas sin #ma-stl-viewer-shell): { commandsUrl, settingsUrl, fetch, storage }. */
const config = {};

export function maStlAiCatalogConfigure(cfg) {
    Object.keys(cfg || {}).forEach(function (k) { config[k] = cfg[k]; });
    return Object.assign({}, config);
}

const state = {
    byCtx: {},          // context → payload del servidor
    replies: {},        // 'es' | 'en' → { 'design.drawn': '…' }
    environment: null,  // { code, nameEs, nameEn, description }
    environments: [],
    listeners: [],
    pending: {},        // context → Promise
    reqLang: {},        // context → idioma pedido (para recargar igual)
};

function storageOf(opts) {
    if (opts && opts.storage !== undefined) return opts.storage;
    if (config.storage !== undefined) return config.storage;
    try { return typeof window !== 'undefined' && window.localStorage ? window.localStorage : null; } catch (_e) { return null; }
}

function fetchOf(opts) {
    if (opts && opts.fetch) return opts.fetch;
    if (config.fetch) return config.fetch;
    return typeof fetch === 'function' ? fetch : null;
}

/** URLs del servidor: atributos `data-ma-stl-ai-commands-url` / `data-ma-stl-ai-settings-url` de #ma-stl-viewer-shell. */
export function maStlAiCatalogUrls(doc) {
    const d = doc || (typeof document !== 'undefined' ? document : null);
    const el = d && d.getElementById ? d.getElementById('ma-stl-viewer-shell') : null;
    return {
        commands: (el ? el.getAttribute('data-ma-stl-ai-commands-url') : '') || config.commandsUrl || '',
        settings: (el ? el.getAttribute('data-ma-stl-ai-settings-url') : '') || config.settingsUrl || '',
    };
}

/**
 * URL de `Desing_2/SaveAiUnknownPhrase` (pt119u): config.unknownUrl o, si no, la de `AiCommands` del mismo controlador
 * («…/Desing_2/AiCommands» → «…/Desing_2/SaveAiUnknownPhrase»). '' si no hay servidor (paleta de plugins, pruebas).
 */
export function maStlAiUnknownUrl(doc) {
    if (config.unknownUrl !== undefined) return config.unknownUrl || '';
    const c = maStlAiCatalogUrls(doc).commands;
    return c && /\/AiCommands(\?.*)?$/i.test(c) ? c.replace(/\/AiCommands(\?.*)?$/i, '/SaveAiUnknownPhrase') : '';
}

/**
 * Frase que la IA no ha comprendido (pt119u): la manda al servidor sin esperar respuesta, que la guarda en
 * App_Data/IA/frases-no-comprendidas.json para revisarla en el chat («abre el .json»). Nunca lanza ni avisa al usuario.
 * @param {{ frase: string, asistente: string, idioma?: string, contexto?: object }} entry
 * @param {{ url?: string, fetch?: Function, doc?: Document }} [opts]
 * @returns {Promise<boolean>} true si el servidor la guardó
 */
export function maStlAiLogUnknownPhrase(entry, opts) {
    try {
        const frase = String((entry && entry.frase) || '').trim();
        const url = (opts && opts.url) || maStlAiUnknownUrl(opts && opts.doc);
        const f = fetchOf(opts);
        if (!frase || !url || !f) return Promise.resolve(false);
        const body = JSON.stringify({
            frase: frase.slice(0, 500),
            asistente: String((entry && entry.asistente) || 'otro'),
            idioma: (entry && entry.idioma) || null,
            contexto: (entry && entry.contexto) || {},
        });
        return Promise.resolve(f(url, {
            method: 'POST', credentials: 'same-origin', keepalive: true,
            headers: { 'Content-Type': 'application/json; charset=utf-8', 'X-Requested-With': 'XMLHttpRequest' }, body: body,
        })).then(function (r) {
            if (!r || !r.ok || typeof r.json !== 'function') return false;
            return r.json().then(function (j) { return !!(j && j.ok); }, function () { return false; });
        }, function () { return false; });
    } catch (_e) {
        return Promise.resolve(false);
    }
}

/** Idioma BCP-47 en minúsculas ('es', 'es-ar', 'en-us'); null si no vale. */
export function maStlAiCatalogLang(raw) {
    const s = String(raw || '').trim().toLowerCase().replace(/_/g, '-');
    return /^[a-z]{2,3}(-[a-z0-9]{2,8}){0,2}$/.test(s) ? s : null;
}

/** Idioma base de una variante ('es-ar' → 'es'). */
export function maStlAiCatalogBaseLang(lang) {
    const s = maStlAiCatalogLang(lang) || 'es';
    return s.split('-')[0];
}

function apply(context, data) {
    state.byCtx[context] = data;
    if (data && data.replies && typeof data.replies === 'object') {
        Object.keys(data.replies).forEach(function (l) { state.replies[l] = data.replies[l] || {}; });
    }
    if (data && data.environment) state.environment = data.environment;
    if (data && Array.isArray(data.environments)) state.environments = data.environments;
    state.listeners.slice().forEach(function (fn) {
        try { fn(context, data); } catch (_e) { /* un intérprete no rompe al otro */ }
    });
}

/** Escucha catálogos cargados: fn(context, payload | null). Devuelve la función para dejar de escuchar. */
export function maStlAiCatalogOnChange(fn) {
    if (typeof fn === 'function') state.listeners.push(fn);
    return function () { state.listeners = state.listeners.filter(function (f) { return f !== fn; }); };
}

export function maStlAiCatalogGet(context) { return state.byCtx[context] || null; }

/** Plantillas del entorno activo para un idioma ('es-ar' usa 'es' si no hay variante): { clave: plantilla }. */
export function maStlAiCatalogReplies(lang) {
    const l = maStlAiCatalogLang(lang) || 'es';
    return state.replies[l] || state.replies[l.split('-')[0]] || {};
}

/** Plantilla de una clave ('design.drawn') o null (→ texto del JS). */
export function maStlAiCatalogReply(key, lang) {
    const r = maStlAiCatalogReplies(lang);
    return Object.prototype.hasOwnProperty.call(r, key) && r[key] ? r[key] : null;
}

/** Entorno activo { code, nameEs, nameEn } (null hasta cargar). */
export function maStlAiCatalogEnv() { return state.environment; }

export function maStlAiCatalogEnvironments() { return state.environments.slice(); }

/** Nombre del entorno en el idioma de la interfaz. */
export function maStlAiCatalogEnvName(env, lang) {
    if (!env) return '';
    return maStlAiCatalogBaseLang(lang) === 'en' ? (env.nameEn || env.code || '') : (env.nameEs || env.code || '');
}

/** Olvida lo cargado (pruebas y cambio de entorno). `storage: true` borra también la caché local. */
export function maStlAiCatalogClear(opts) {
    state.byCtx = {};
    state.replies = {};
    state.environment = null;
    state.environments = [];
    state.pending = {};
    if (opts && opts.storage) {
        const st = storageOf(opts);
        try {
            if (st) {
                const keys = [];
                for (let i = 0; i < st.length; i++) { const k = st.key(i); if (k && k.indexOf(MA_STL_AI_CATALOG_STORAGE) === 0) keys.push(k); }
                keys.forEach(function (k) { st.removeItem(k); });
            }
        } catch (_e) { /* sin storage */ }
    }
    if (opts && opts.listeners) state.listeners = [];
}

function readCache(st, key) {
    if (!st) return null;
    try {
        const raw = st.getItem(key);
        const obj = raw ? JSON.parse(raw) : null;
        return obj && obj.version && obj.data ? obj : null;
    } catch (_e) { return null; }
}

function writeCache(st, key, version, data) {
    if (!st) return;
    try { st.setItem(key, JSON.stringify({ version: version, at: Date.now(), data: data })); } catch (_e) { /* lleno o bloqueado: sin caché */ }
}

/**
 * Carga el catálogo de un contexto ('design' | 'wall') para un idioma. Promesa con el payload o null (sin URL / error:
 * los intérpretes siguen con el vocabulario JS). Con caché válida y `notModified` no se vuelve a descargar.
 * @param {string} context
 * @param {string} lang
 * @param {{ url?: string, doc?: Document, storage?: Storage|null, fetch?: Function, force?: boolean }} [opts]
 */
export function maStlAiCatalogLoad(context, lang, opts) {
    opts = opts || {};
    const ctx = context === 'wall' ? 'wall' : 'design';
    const l = maStlAiCatalogLang(lang) || 'es';
    const url = opts.url != null ? opts.url : maStlAiCatalogUrls(opts.doc).commands;
    const f = fetchOf(opts);
    if (!url || !f) return Promise.resolve(null);
    const pkey = ctx + ':' + l;
    state.reqLang[ctx] = l;
    if (state.pending[pkey] && !opts.force) return state.pending[pkey];
    const st = storageOf(opts);
    const key = MA_STL_AI_CATALOG_STORAGE + ctx + ':' + l;
    const cached = opts.force ? null : readCache(st, key);
    const sep = url.indexOf('?') >= 0 ? '&' : '?';
    const full = url + sep + 'context=' + encodeURIComponent(ctx) + '&lang=' + encodeURIComponent(l)
        + '&v=' + encodeURIComponent(cached ? cached.version : '');
    const p = Promise.resolve().then(function () {
        return f(full, { method: 'GET', credentials: 'same-origin', headers: { Accept: 'application/json' } });
    }).then(function (res) {
        if (!res || !res.ok) throw new Error('HTTP ' + (res ? res.status : '?'));
        return res.json();
    }).then(function (json) {
        if (json && json.ok && json.notModified && cached) {
            apply(ctx, cached.data);
            return cached.data;
        }
        if (json && json.ok && Array.isArray(json.commands)) {
            writeCache(st, key, json.version || '', json);
            apply(ctx, json);
            return json;
        }
        throw new Error((json && json.message) || 'catálogo no disponible');
    }).catch(function () {
        /* Sin servidor: la última copia buena (si la hay); si no, vocabulario JS. */
        if (cached) { apply(ctx, cached.data); return cached.data; }
        return null;
    }).then(function (r) {
        delete state.pending[pkey];
        return r;
    });
    state.pending[pkey] = p;
    return p;
}

/**
 * Guarda el entorno del usuario (`Desing_2/SaveAiUserSettings`) y recarga los catálogos ya cargados.
 * Promesa { ok, environment?, message? }.
 */
export function maStlAiCatalogSaveEnv(code, opts) {
    opts = opts || {};
    const url = opts.url != null ? opts.url : maStlAiCatalogUrls(opts.doc).settings;
    const f = fetchOf(opts);
    if (!url || !f) return Promise.resolve({ ok: false, message: 'sin servidor' });
    return Promise.resolve().then(function () {
        return f(url, {
            method: 'POST', credentials: 'same-origin',
            headers: { 'Content-Type': 'application/json; charset=utf-8', Accept: 'application/json' },
            body: JSON.stringify({ env: String(code || '') }),
        });
    }).then(function (res) {
        if (!res || !res.ok) throw new Error('HTTP ' + (res ? res.status : '?'));
        return res.json();
    }).then(function (json) {
        if (!json || !json.ok) return { ok: false, message: (json && json.message) || 'error' };
        const loaded = Object.keys(state.byCtx);
        const langOf = function (c) { return state.reqLang[c] || opts.lang || 'es'; };
        return Promise.all(loaded.map(function (c) {
            return maStlAiCatalogLoad(c, langOf(c), Object.assign({}, opts, { url: opts.commandsUrl, force: true }));
        })).then(function () { return { ok: true, environment: json.environment || state.environment }; });
    }).catch(function (e) { return { ok: false, message: e && e.message ? e.message : String(e) }; });
}

/**
 * Frases del catálogo de un contexto, ya troceadas con `normalize` (el del intérprete):
 * [{ code, aiId, concept, kind, weight, toks, text, implemented, handler, nameEs, nameEn, lang, env, user }].
 * `kinds` filtra por tipo (por defecto todos).
 */
export function maStlAiCatalogPhrases(data, normalize, kinds) {
    const out = [];
    if (!data || !Array.isArray(data.commands)) return out;
    const norm = typeof normalize === 'function' ? normalize : function (s) { return String(s || '').toLowerCase(); };
    data.commands.forEach(function (c) {
        (c.phrases || []).forEach(function (ph) {
            const kind = ph.k || 'keyword';
            if (kinds && kinds.indexOf(kind) < 0) return;
            const n = norm(ph.p);
            if (!n) return;
            out.push({
                code: c.code, aiId: c.aiId || null, context: c.context, concept: ph.c || null, kind: kind,
                weight: ph.w == null ? null : Number(ph.w), toks: n.split(' '), text: ph.p,
                implemented: !!c.implemented, handler: c.handler || '', nameEs: c.nameEs || c.code, nameEn: c.nameEn || c.code,
                lang: ph.l || '', env: !!ph.e, user: !!ph.u, useDefaults: c.useDefaults !== false,
            });
        });
    });
    return out;
}

/**
 * Lo que el usuario ha quitado o sustituido, por concepto (diseño) o por aiId (muro):
 * { replaced: { clave: true }, excluded: { clave: ['palabra normalizada', …] } }.
 * `replaced` = algún comando de esa clave no usa las palabras por defecto (Is_UseDefaultPhrases = 0).
 */
export function maStlAiCatalogUserRules(data, normalize, keyOf) {
    const out = { replaced: {}, excluded: {} };
    if (!data || !Array.isArray(data.commands)) return out;
    const norm = typeof normalize === 'function' ? normalize : function (s) { return String(s || '').toLowerCase(); };
    data.commands.forEach(function (c) {
        const keys = keyOf(c);
        keys.forEach(function (k) {
            if (!k) return;
            if (c.useDefaults === false) out.replaced[k] = true;
            (c.excluded || []).forEach(function (n) {
                const w = norm(n);
                if (w) (out.excluded[k] || (out.excluded[k] = [])).push(w);
            });
        });
    });
    return out;
}

/**
 * Coincidencia más larga de una frase del catálogo dentro de las palabras del mensaje (ya normalizadas).
 * Regla: gana la frase con más palabras; a igualdad, la de mejor calidad; a igualdad, la primera del catálogo.
 * `match(palabra, palabraFrase)` → 0..1; cada palabra debe llegar a `min` (0,95 por defecto: exacta, sin acentos o con
 * enclítico; una falta de una letra NO vale en frases, «gira el muro» no es «tira el muro»). Calidad = la peor.
 * @returns {{ item: object, start: number, len: number, q: number } | null}
 */
export function maStlAiCatalogLongest(words, phrases, match, min) {
    const floor = typeof min === 'number' ? min : 0.95;
    let best = null;
    if (!Array.isArray(words) || !words.length || !Array.isArray(phrases)) return null;
    for (let i = 0; i < phrases.length; i++) {
        const ph = phrases[i];
        const n = ph.toks.length;
        if (!n || n > words.length || (best && n < best.len)) continue;
        for (let s = 0; s + n <= words.length; s++) {
            let qv = 1;
            for (let j = 0; j < n && qv > 0; j++) {
                const m = match(words[s + j], ph.toks[j]);
                qv = m >= floor ? Math.min(qv, m) : 0;
            }
            if (qv > 0) {
                if (!best || n > best.len || (n === best.len && qv > best.q)) best = { item: ph, start: s, len: n, q: qv };
                break;
            }
        }
    }
    return best;
}

/* ------------------------- Entorno de conversación ------------------------- */

/** Nombres por defecto (si el catálogo aún no ha llegado) de los entornos sembrados en TSql_AiEnvironment. */
export const MA_STL_AI_ENV_NAMES = {
    EXECUTIVE: { es: 'Ejecutivo', en: 'Executive' },
    COLLOQUIAL: { es: 'Coloquial', en: 'Colloquial' },
    STREET: { es: 'Colega barriobajero', en: 'Street buddy' },
    TECHNICAL: { es: 'Técnico', en: 'Technical' },
};

/**
 * Frases JS para cambiar de entorno cuando no hay catálogo (texto ya normalizado: minúsculas, sin acentos).
 * Las de la BD (comandos ENV_*) mandan: se miran antes.
 */
export const MA_STL_AI_ENV_FALLBACK = {
    EXECUTIVE: /\b(modo|entorno) (ejecutivo|formal)\b|\bponte (serio|seria|ejecutivo|formal)\b|\bhabla formal\b|\btratame de usted\b|\b(executive|formal) (mode|environment)\b|\bbe formal\b/,
    COLLOQUIAL: /\b(modo|entorno) (coloquial|normal)\b|\bhabla normal\b|\bhablame de tu\b|\b(casual|colloquial) (mode|environment)\b|\btalk normally\b/,
    STREET: /\b(modo|entorno) (colega|barrio|barriobajero)\b|\bhabla(me)? como un colega\b|\b(buddy|street|mate) mode\b|\btalk like a mate\b/,
    TECHNICAL: /\b(modo|entorno) tecnico\b|\bhabla tecnico\b|\bse tecnico\b|\btechnical (mode|environment)\b|\btalk technical\b/,
};

/** Nombre de un entorno por su código (catálogo → nombres por defecto → código). */
export function maStlAiCatalogEnvLabel(code, lang) {
    const c = String(code || '').toUpperCase();
    const e = state.environments.find(function (x) { return x && x.code === c; })
        || (state.environment && state.environment.code === c ? state.environment : null);
    if (e) return maStlAiCatalogEnvName(e, lang);
    const d = MA_STL_AI_ENV_NAMES[c];
    return d ? d[maStlAiCatalogBaseLang(lang) === 'en' ? 'en' : 'es'] : c;
}

/**
 * Entorno pedido en el mensaje ('EXECUTIVE' | 'COLLOQUIAL' | 'STREET' | 'TECHNICAL' | otro código de la BD) o null.
 * Mensajes cortos (≤ 7 palabras); la frase ENV_* más larga del catálogo debe ocupar casi todo el mensaje.
 * @param {string} normText texto normalizado (maStlWallAiNormalize)
 * @param {Array} [phrases] frases del catálogo (maStlAiCatalogPhrases)
 * @param {Function} [match] comparación de palabras (0..1)
 */
export function maStlAiCatalogEnvTarget(normText, phrases, match) {
    const t = String(normText || '').trim();
    const words = t ? t.split(/\s+/) : [];
    if (!words.length || words.length > 7) return null;
    if (Array.isArray(phrases) && phrases.length) {
        const envPh = phrases.filter(function (ph) { return ph && /^ENV_/.test(ph.code); });
        const m = maStlAiCatalogLongest(words, envPh, match || function (a, b) { return a === b ? 1 : 0; });
        if (m && m.len >= Math.max(1, words.length - 3)) return m.item.code.slice(4);
    }
    const codes = Object.keys(MA_STL_AI_ENV_FALLBACK);
    for (let i = 0; i < codes.length; i++) if (MA_STL_AI_ENV_FALLBACK[codes[i]].test(t)) return codes[i];
    return null;
}

/** Rellena una plantilla con huecos {0}, {1}… */
export function maStlAiCatalogFmt(tpl) {
    const args = Array.prototype.slice.call(arguments, 1);
    return String(tpl == null ? '' : tpl).replace(/\{(\d+)\}/g, function (m, i) { return args[i] != null ? String(args[i]) : m; });
}

if (typeof window !== 'undefined') {
    window.maStlAiCatalog = {
        load: maStlAiCatalogLoad, get: maStlAiCatalogGet, env: maStlAiCatalogEnv, saveEnv: maStlAiCatalogSaveEnv,
        replies: maStlAiCatalogReplies, clear: maStlAiCatalogClear,
    };
}
