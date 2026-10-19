const fs = require('fs');
const zlib = require('zlib');
const pdf = fs.readFileSync('c:/00_Tandem2026/Desing/Content/DesignTools/DWG/AtkSystem60/Snaps/NEVI_GUIA_USUARIO.pdf');
const latin = pdf.toString('latin1');
let pos = 0;
let n = 0;
const pages = [];
while (pos < pdf.length) {
    const i = latin.indexOf('stream', pos);
    if (i < 0) break;
    let start = i + 6;
    if (pdf[start] === 0x0d && pdf[start + 1] === 0x0a) start += 2;
    else if (pdf[start] === 0x0a) start += 1;
    const end = latin.indexOf('endstream', start);
    if (end < 0) break;
    const raw = pdf.subarray(start, pdf[end - 1] === 0x0a ? end - 1 : end);
    pos = end + 9;
    let text = null;
    try { text = zlib.inflateSync(raw).toString('latin1'); }
    catch (e) {
        try { text = zlib.inflateRawSync(raw).toString('latin1'); }
        catch (e2) { continue; }
    }
    n++;
    const parts = [];
    const re = /\((?:\\.|[^\\)]){2,}\)/g;
    let m;
    while ((m = re.exec(text))) {
        const s = m[0].slice(1, -1).replace(/\\([()\\])/g, '$1').replace(/\\n/g, ' ');
        if (s.trim()) parts.push(s);
    }
    const joined = parts.join(' ');
    if (/puntal|cabezal|pilar|perfil|costilla|orificio|2\.9|3\.8/i.test(joined)) {
        pages.push({ n, joined });
    }
}
console.log('pages with words', pages.length);
for (const p of pages) {
    const j = p.joined.replace(/\s+/g, ' ');
    console.log('\n===== stream', p.n, 'chars', j.length);
    console.log(j.slice(0, 2500));
}
