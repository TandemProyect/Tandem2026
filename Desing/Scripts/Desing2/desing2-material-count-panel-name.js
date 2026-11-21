import { formworkPanelColorHex } from './formwork-panel-color.js';

const VOWELS = 'aeiouáéíóúüAEIOUÁÉÍÓÚÜ';
const INSEP = /^(?:ll|rr|ch|pl|pr|bl|br|fl|fr|gl|gr|cl|cr|dr|tr|tl|qu|gu)$/i;

export function isPanelArticle(name, code) {
    if (/^\s*panel\b/i.test(String(name || ''))) return true;
    const c = String(code || '').replace(/^PANEL_/i, '');
    return /^3120\d{6}$/.test(c) || /^(?:12|24|27)\d{5,}/.test(c);
}

export function widthMmFromPanelName(name) {
    const text = String(name || '');
    const re = /(\d+)[,.](\d+)/g;
    let match;
    let widthMm = null;
    while ((match = re.exec(text))) {
        const meters = Number(match[1] + '.' + match[2]);
        if (!Number.isFinite(meters) || meters < 0.2 || meters > 1.2) continue;
        widthMm = Math.round(meters * 1000);
    }
    return widthMm;
}

function isVowel(ch) {
    return VOWELS.indexOf(ch) >= 0 || ch === 'y' || ch === 'Y';
}

function nextOnsetIndex(cons) {
    const len = cons.length;
    if (len <= 1) return 0;
    if (INSEP.test(cons.slice(-2))) return len - 2;
    return len - 1;
}

export function spanishSyllables(word) {
    const w = String(word || '');
    if (!w) return [];
    const nuclei = [];
    let i = 0;
    while (i < w.length) {
        if (!isVowel(w[i])) {
            i += 1;
            continue;
        }
        let j = i + 1;
        while (j < w.length && isVowel(w[j])) j += 1;
        nuclei.push({ start: i, end: j });
        i = j;
    }
    if (!nuclei.length) return [w];
    const out = [];
    for (let n = 0; n < nuclei.length; n++) {
        const prevEnd = n === 0 ? 0 : nuclei[n - 1].end;
        const consStart = n === 0 ? 0 : nuclei[n - 1].end;
        const cons = w.slice(consStart, nuclei[n].start);
        const onsetAt = n === 0 ? 0 : consStart + nextOnsetIndex(cons);
        const from = n === 0 ? 0 : onsetAt;
        let to;
        if (n === nuclei.length - 1) {
            to = w.length;
        } else {
            const between = w.slice(nuclei[n].end, nuclei[n + 1].start);
            to = nuclei[n].end + nextOnsetIndex(between);
        }
        if (n === 0) {
            out.push(w.slice(0, to));
        } else {
            out.push(w.slice(from, to));
        }
        void prevEnd;
    }
    return out.filter(Boolean);
}

export function firstSyllablesEndIndex(text, count) {
    const s = String(text || '');
    const n = Number(count);
    if (!s || !(n > 0)) return 0;
    let taken = 0;
    let i = 0;
    const letter = /[A-Za-zÁÉÍÓÚÜáéíóúüñÑ]/;
    while (i < s.length && taken < n) {
        if (!letter.test(s[i])) {
            i += 1;
            continue;
        }
        let j = i;
        while (j < s.length && letter.test(s[j])) j += 1;
        const parts = spanishSyllables(s.slice(i, j));
        let k = 0;
        for (let p = 0; p < parts.length; p++) {
            k += parts[p].length;
            taken += 1;
            if (taken >= n) return i + k;
        }
        i = j;
    }
    return s.length;
}

function escapeHtml(value) {
    return String(value == null ? '' : value)
        .replace(/&/g, '&amp;')
        .replace(/</g, '&lt;')
        .replace(/>/g, '&gt;')
        .replace(/"/g, '&quot;');
}

function toCssHex(n) {
    if (n == null || !Number.isFinite(n)) return '';
    return '#' + n.toString(16).padStart(6, '0');
}

export function catalogFromServerPayload(data) {
    const rows = (data && (data.Colors || data.colors)) || [];
    const map = {};
    for (let i = 0; i < rows.length; i++) {
        const row = rows[i] || {};
        const w = Number(row.WidthMm != null ? row.WidthMm : row.widthMm);
        const c = row.Color || row.color;
        if (!Number.isFinite(w) || w <= 0 || !c) continue;
        map[w] = c;
    }
    return map;
}

export function materialCountColorPanelNameHtml(name, code, catalog) {
    const text = name == null ? '' : String(name);
    if (!isPanelArticle(text, code)) return escapeHtml(text);
    const widthMm = widthMmFromPanelName(text);
    const hexNum = formworkPanelColorHex(catalog, widthMm, 'Panel', true);
    const css = toCssHex(hexNum);
    if (!css) return escapeHtml(text);
    const end = firstSyllablesEndIndex(text, 5);
    const head = text.slice(0, end);
    const tail = text.slice(end);
    return '<span class="desing2-count-panel-syllables" style="color:' + css + ';font-weight:700">'
        + escapeHtml(head)
        + '</span>'
        + escapeHtml(tail);
}

if (typeof window !== 'undefined') {
    window.TandemMaterialCountPanelName = {
        isPanelArticle,
        widthMmFromPanelName,
        spanishSyllables,
        firstSyllablesEndIndex,
        catalogFromServerPayload,
        materialCountColorPanelNameHtml
    };
}
