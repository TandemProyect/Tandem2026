/**
 * Color de panel por ancho. El catálogo llega del servidor (mm → #RRGGBB).
 * Solo pinta piezas de tipo Panel. Grapas, remates y esquinas se quedan como están.
 * @param {Object<string, string|number>|null} catalog
 * @param {number} pieceWidthMm
 * @param {string} elementType
 * @param {boolean} enabled
 * @returns {number|null} hex THREE, o null si no hay que cambiar el color
 */
/**
 * Color del maestro de artículos. Solo paneles, y solo si el hex es válido.
 * "0" o vacío no pinta: el panel se queda con el material de encofrado.
 * @param {string|number} articleColor
 * @param {string} elementType
 * @param {boolean} enabled
 * @returns {number|null}
 */
/**
 * Color de paneles del sistema. Solo paneles verticales.
 * Los suplementos (Tumbado) y el resto se quedan con su color.
 * @param {string|number} systemColor
 * @param {string} elementType
 * @param {string} orientation
 * @returns {number|null}
 */
/** Color del elemento, o el del sistema si el elemento no trae hex. */
export function formworkResolveSystemPanelColor(item, fallback) {
    const direct = item && item.SystemPanelColor != null ? item.SystemPanelColor : (item && item.systemPanelColor);
    if (direct != null && String(direct).trim() !== '') return String(direct);
    return fallback != null ? String(fallback) : '';
}

export function formworkSystemPanelColorHex(systemColor, elementType, orientation) {
    if (String(elementType || '').toLowerCase() !== 'panel') return null;
    if (String(orientation || '').toLowerCase() !== 'vertical') return null;
    return formworkParseColorHex(systemColor);
}

/**
 * Color de un mesh del panel. El fenólico nunca coge el color del sistema.
 * Grapas y piezas sin clasificar devuelven null: se quedan con su material.
 * @param {'frame'|'phenolic'|'other'} part
 * @param {number|null} frameOverrideHex
 * @param {number} defaultFrameHex
 * @param {number} phenolicHex
 * @returns {number|null}
 */
export function formworkMeshColorHex(part, frameOverrideHex, defaultFrameHex, phenolicHex) {
    if (part === 'phenolic') return phenolicHex;
    if (part === 'frame') return frameOverrideHex != null ? frameOverrideHex : defaultFrameHex;
    return null;
}

/** Puntal exterior e interior: no los pisa el color de panel. */
export function formworkKeepsFactoryRole(paintRole) {
    const role = String(paintRole || '').toLowerCase();
    return role === 'prop' || role === 'prop-inner';
}

/**
 * Encofrado ya pintado: Colores y Encofrar solo cambian el color.
 * No vuelven a pedir la fase ni al servidor. Sin encofrado, hay que calcular.
 * @returns {'solve'|'repaint-color'|'repaint-system'}
 */
export function formworkModeClickAction(alreadyPainted, fromColorButton, colorWasOn) {
    if (alreadyPainted !== true) return 'solve';
    if (fromColorButton === true) return 'repaint-color';
    if (colorWasOn === true) return 'repaint-system';
    return 'solve';
}

/**
 * Sin muro seleccionado, las cotas de longitud siguen apagadas.
 * Paneles, remates e id de artículo salen si la casilla está marcada.
 */
export function wallDimKindWithoutWall(kindId, map) {
    if (kindId === 'wall-id') return true;
    if (kindId !== 'panels' && kindId !== 'remates' && kindId !== 'article-id') return false;
    if (!map || typeof map !== 'object' || !Object.prototype.hasOwnProperty.call(map, kindId)) return false;
    return map[kindId] === true;
}

/** El modo colores solo cambia paneles verticales. Suplementos, grapas y rigidizadores no. */
export function formworkColorModePaints(elementType, orientation) {
    return String(elementType || '').toLowerCase() === 'panel'
        && String(orientation || '').toLowerCase() === 'vertical';
}

export function formworkArticleColorHex(articleColor, elementType, enabled, orientation) {
    if (enabled !== true) return null;
    if (String(elementType || '').toLowerCase() !== 'panel') return null;
    if (orientation != null && String(orientation).toLowerCase() !== 'vertical') return null;
    return formworkParseColorHex(articleColor);
}

export function formworkPanelColorHex(catalog, pieceWidthMm, elementType, enabled) {
    if (enabled !== true || !catalog) return null;
    if (String(elementType || '').toLowerCase() !== 'panel') return null;
    const width = Number(pieceWidthMm);
    if (!Number.isFinite(width) || width <= 0) return null;

    const keys = Object.keys(catalog);
    let bestKey = null;
    let bestDistance = 16;
    for (let i = 0; i < keys.length; i++) {
        const key = Number(keys[i]);
        if (!Number.isFinite(key)) continue;
        const distance = Math.abs(key - width);
        if (distance < bestDistance) {
            bestDistance = distance;
            bestKey = key;
        }
    }
    if (bestKey == null) return null;

    const raw = catalog[bestKey] != null ? catalog[bestKey] : catalog[String(bestKey)];
    return formworkParseColorHex(raw);
}

/** @param {string|number} raw */
export function formworkParseColorHex(raw) {
    if (typeof raw === 'number' && Number.isFinite(raw) && raw >= 0 && raw <= 0xffffff) {
        return raw;
    }
    const text = String(raw || '').trim();
    const match = /^#?([0-9a-fA-F]{6})$/.exec(text);
    if (!match) return null;
    return parseInt(match[1], 16);
}
