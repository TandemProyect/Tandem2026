const fs = require('fs');
const path = process.argv[2];
const text = fs.readFileSync(path, 'utf8');
const re = /vertex\s+([eE0-9+\-\.]+)\s+([eE0-9+\-\.]+)\s+([eE0-9+\-\.]+)/g;
let m, n = 0;
let min = [1e9, 1e9, 1e9], max = [-1e9, -1e9, -1e9];
while ((m = re.exec(text))) {
  n++;
  const p = [+m[1], +m[2], +m[3]];
  for (let i = 0; i < 3; i++) {
    if (p[i] < min[i]) min[i] = p[i];
    if (p[i] > max[i]) max[i] = p[i];
  }
}
console.log(path.split(/[\\/]/).pop(), 'verts', n);
console.log('min mm', min.map(v => (v * 1000).toFixed(1)).join(' '));
console.log('max mm', max.map(v => (v * 1000).toFixed(1)).join(' '));
console.log('size mm', [0, 1, 2].map(i => ((max[i] - min[i]) * 1000).toFixed(1)).join(' '));
