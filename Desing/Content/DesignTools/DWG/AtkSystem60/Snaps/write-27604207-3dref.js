/**
 * 3DRef 27604207 (2.70 x 0.60): caja de líneas en metros, mismo eje que 27904209R.
 * X=ancho, Y=espesor (negativo), Z=alto. INSBASE 0,0,0 cara Y=0.
 */
const fs = require("fs");
const path = require("path");

const outDir = path.resolve(__dirname, "..", "3DRef");
const w = 0.6;
const h = 2.7;
const t = -0.12;
const insetX = 0.0437138;
const zG = 0.55;
const rMark = 0.04;

function n(v) {
  return Number(v).toFixed(6).replace(/\.?0+$/, "") || "0";
}

function line(x1, y1, z1, x2, y2, z2) {
  return [
    "0", "LINE", "8", "0",
    "10", n(x1), "20", n(y1), "30", n(z1),
    "11", n(x2), "21", n(y2), "31", n(z2)
  ];
}

function circle(x, y, z, r) {
  return [
    "0", "CIRCLE", "8", "0",
    "10", n(x), "20", n(y), "30", n(z),
    "40", n(r)
  ];
}

const lines = [
  "0", "SECTION", "2", "HEADER",
  "9", "$ACADVER", "1", "AC1021",
  "9", "$INSUNITS", "70", "6",
  "9", "$INSBASE", "10", "0.0", "20", "0.0", "30", "0.0",
  "0", "ENDSEC",
  "0", "SECTION", "2", "TABLES",
  "0", "TABLE", "2", "LTYPE", "70", "1",
  "0", "LTYPE", "2", "CONTINUOUS", "70", "0", "3", "Solid line", "72", "65", "73", "0", "40", "0.0",
  "0", "ENDTAB",
  "0", "TABLE", "2", "LAYER", "70", "1",
  "0", "LAYER", "2", "0", "70", "0", "62", "7", "6", "CONTINUOUS",
  "0", "ENDTAB",
  "0", "ENDSEC",
  "0", "SECTION", "2", "ENTITIES"
];

const edges = [
  [0, 0, 0, w, 0, 0],
  [w, 0, 0, w, 0, h],
  [w, 0, h, 0, 0, h],
  [0, 0, h, 0, 0, 0],
  [0, t, 0, w, t, 0],
  [w, t, 0, w, t, h],
  [w, t, h, 0, t, h],
  [0, t, h, 0, t, 0],
  [0, 0, 0, 0, t, 0],
  [w, 0, 0, w, t, 0],
  [0, 0, h, 0, t, h],
  [w, 0, h, w, t, h]
];
for (const e of edges) lines.push(...line(e[0], e[1], e[2], e[3], e[4], e[5]));

for (const x of [insetX, w - insetX]) {
  for (const z of [zG, h - zG]) {
    lines.push(...circle(x, 0, z, rMark));
  }
}
for (const c of [[0, 0, 0], [w, 0, 0], [0, 0, h], [w, 0, h]]) {
  lines.push(...circle(c[0], c[1], c[2], rMark * 1.3));
}

lines.push("0", "ENDSEC", "0", "EOF", "");
fs.mkdirSync(outDir, { recursive: true });
const dxfPath = path.join(outDir, "27604207R.dxf");
fs.writeFileSync(dxfPath, lines.join("\n"));
console.log("Wrote " + dxfPath);
