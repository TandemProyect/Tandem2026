const fs = require('fs');
const path = 'C:/00_Tandem2026/Desing/assets/materio/vendor/js/menu.js';
const c = fs.readFileSync(path, 'utf8');
const m = c.match(/eval\("((?:\\.|[^"\\])*)"\)/);
if (!m) {
  fs.writeFileSync('C:/00_Tandem2026/Desing/Scripts/_menu_extract_out.txt', 'no eval match');
  process.exit(1);
}
const src = JSON.parse('"' + m[1] + '"');
const patterns = ['_getItem', 'toggle(', '_bindEvents', '_evntElClick', 'addEventListener', 'scrollToActive', 'constructor'];
const lines = src.split('\n');
const out = [];
for (let i = 0; i < lines.length; i++) {
  const l = lines[i];
  if (patterns.some((p) => l.includes(p))) out.push(`${i + 1}: ${l}`);
}
fs.writeFileSync('C:/00_Tandem2026/Desing/Scripts/_menu_extract_out.txt', out.join('\n'), 'utf8');
