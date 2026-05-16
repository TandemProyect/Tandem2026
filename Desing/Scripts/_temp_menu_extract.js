const fs = require('fs');
const c = fs.readFileSync('c:/00_Tandem2026/Desing/assets/materio/vendor/js/helpers.js', 'utf8');
const m = c.match(/eval\("((?:\\.|[^"\\])*)"\)/);
const src = JSON.parse('"' + m[1] + '"');
const start = src.indexOf('scrollToActive:');
fs.writeFileSync('c:/00_Tandem2026/Desing/Scripts/_temp_scroll_active.txt', src.substring(start, start + 4000), 'utf8');
