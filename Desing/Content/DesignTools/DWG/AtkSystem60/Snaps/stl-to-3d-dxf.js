/**
 * Convierte STL ATK-60 (metros, X=ancho Y=espesor Z=alto) a DXF 3DFACE
 * para insertar como bloque 3D. Marco + fenólico _F, mismos colores que encofrar.
 * Origen común: minX=0, cara PANEL maxY=0, minZ=0.
 * Une triángulos STL coplanares en 3DFACE de 4 lados (el STL trocea quads).
 */
const fs = require("fs");
const path = require("path");

const root = path.resolve(__dirname, "..");
const stlDir = path.resolve(root, "..", "..", "Stl", "ATK60");
const outDir = path.join(root, "3D");

const FRAME_HEX = 0xefb608;
const PHENOLIC_HEX = 0x1a1816;
const FRAME_ACI = 2;
const PHENOLIC_ACI = 8;

const panels = [
  "27904209",
  "27604207",
  "27454206",
  "27304205",
  "24904240",
  "24604242",
  "24454243",
  "24304244",
  "12904215",
  "12604213",
  "12454212",
  "12304211"
];

function isBinaryStl(buf) {
  if (buf.length < 84) return false;
  const n = buf.readUInt32LE(80);
  return 84 + n * 50 === buf.length;
}

function parseAscii(text) {
  const faces = [];
  const re = /vertex\s+([-\d.eE+]+)\s+([-\d.eE+]+)\s+([-\d.eE+]+)/g;
  let m;
  let tri = [];
  while ((m = re.exec(text))) {
    tri.push([Number(m[1]), Number(m[2]), Number(m[3])]);
    if (tri.length === 3) {
      faces.push(tri);
      tri = [];
    }
  }
  return faces;
}

function parseBinary(buf) {
  const n = buf.readUInt32LE(80);
  const faces = [];
  let o = 84;
  for (let i = 0; i < n; i++) {
    o += 12;
    const v = [];
    for (let k = 0; k < 3; k++) {
      v.push([buf.readFloatLE(o), buf.readFloatLE(o + 4), buf.readFloatLE(o + 8)]);
      o += 12;
    }
    o += 2;
    faces.push(v);
  }
  return faces;
}

function parseStl(filePath) {
  const buf = fs.readFileSync(filePath);
  return isBinaryStl(buf) ? parseBinary(buf) : parseAscii(buf.toString("utf8"));
}

function originShift(parts) {
  let minX = Infinity;
  let minZ = Infinity;
  let maxY = -Infinity;
  for (const part of parts) {
    for (const f of part.faces) {
      for (const v of f) {
        if (v[0] < minX) minX = v[0];
        if (v[1] > maxY) maxY = v[1];
        if (v[2] < minZ) minZ = v[2];
      }
    }
  }
  return { dx: minX, dy: maxY, dz: minZ };
}

function applyShift(faces, shift) {
  return faces.map((f) => f.map((v) => [v[0] - shift.dx, v[1] - shift.dy, v[2] - shift.dz]));
}

/** Weld 0.1 mm: empareja triángulos coplanares que comparten arista → 3DFACE de 4 lados. */
const WELD = 1e-4;
const PLANE_TOL = 5e-4;

function weldKey(v) {
  return v.map((n) => Math.round(n / WELD)).join(",");
}

function edgeKey(a, b) {
  const ka = weldKey(a);
  const kb = weldKey(b);
  return ka < kb ? ka + "|" + kb : kb + "|" + ka;
}

function sub3(a, b) {
  return [a[0] - b[0], a[1] - b[1], a[2] - b[2]];
}

function cross3(u, v) {
  return [u[1] * v[2] - u[2] * v[1], u[2] * v[0] - u[0] * v[2], u[0] * v[1] - u[1] * v[0]];
}

function dot3(u, v) {
  return u[0] * v[0] + u[1] * v[1] + u[2] * v[2];
}

function triNormal(tri) {
  const n = cross3(sub3(tri[1], tri[0]), sub3(tri[2], tri[0]));
  const len = Math.hypot(n[0], n[1], n[2]);
  if (len < 1e-12) return null;
  return [n[0] / len, n[1] / len, n[2] / len];
}

function samePlane(t1, t2, n1) {
  const a = t1[0];
  for (const p of t2) {
    if (Math.abs(dot3(n1, sub3(p, a))) > PLANE_TOL) return false;
  }
  const n2 = triNormal(t2);
  if (!n2) return false;
  return dot3(n1, n2) > 0.999;
}

function quadFromTris(t1, t2) {
  const in2 = new Set(t2.map(weldKey));
  let iu = t1.findIndex((v) => !in2.has(weldKey(v)));
  if (iu < 0) return null;
  const a = t1[iu];
  const b = t1[(iu + 1) % 3];
  const c = t1[(iu + 2) % 3];
  const in1 = new Set(t1.map(weldKey));
  const d = t2.find((v) => !in1.has(weldKey(v)));
  if (!d) return null;
  return [a, b, d, c];
}

function convexQuad(q, n) {
  for (let i = 0; i < 4; i++) {
    const e1 = sub3(q[(i + 1) % 4], q[i]);
    const e2 = sub3(q[(i + 2) % 4], q[(i + 1) % 4]);
    if (dot3(cross3(e1, e2), n) < -1e-9) return false;
  }
  return true;
}

function mergeCoplanarQuads(tris) {
  const n = tris.length;
  const used = new Array(n).fill(false);
  const normals = tris.map(triNormal);
  const edgeToFaces = new Map();
  for (let i = 0; i < n; i++) {
    const t = tris[i];
    if (!t || t.length < 3) continue;
    for (let e = 0; e < 3; e++) {
      const k = edgeKey(t[e], t[(e + 1) % 3]);
      if (!edgeToFaces.has(k)) edgeToFaces.set(k, []);
      edgeToFaces.get(k).push(i);
    }
  }
  const out = [];
  let quads = 0;
  for (const idxs of edgeToFaces.values()) {
    if (idxs.length !== 2) continue;
    const i = idxs[0];
    const j = idxs[1];
    if (used[i] || used[j]) continue;
    if (!normals[i] || !normals[j]) continue;
    if (tris[i].length !== 3 || tris[j].length !== 3) continue;
    if (!samePlane(tris[i], tris[j], normals[i])) continue;
    const q = quadFromTris(tris[i], tris[j]);
    if (!q || !convexQuad(q, normals[i])) continue;
    used[i] = true;
    used[j] = true;
    out.push(q);
    quads++;
  }
  let leftover = 0;
  for (let i = 0; i < n; i++) {
    if (!used[i]) {
      out.push(tris[i]);
      leftover++;
    }
  }
  return { faces: out, quads: quads, tris: leftover };
}

function fmt(n) {
  return Number(n).toFixed(6).replace(/\.?0+$/, "") || "0";
}

function writeLayer(lines, name, aci, hex) {
  lines.push("0", "LAYER", "2", name, "70", "0", "62", String(aci), "420", String(hex));
}

function writeFaces(lines, faces, layer, aci, hex) {
  for (const f of faces) {
    lines.push("0", "3DFACE", "8", layer, "62", String(aci), "420", String(hex));
    const quad = f.length >= 4;
    for (let i = 0; i < 4; i++) {
      const v = quad ? f[i] : f[Math.min(i, 2)];
      lines.push(String(10 + i), fmt(v[0]));
      lines.push(String(20 + i), fmt(v[1]));
      lines.push(String(30 + i), fmt(v[2]));
    }
  }
}

function writeDxf(parts, outPath) {
  const lines = [
    "0", "SECTION", "2", "HEADER",
    "9", "$INSUNITS", "70", "6",
    "9", "$INSBASE", "10", "0.0", "20", "0.0", "30", "0.0",
    "0", "ENDSEC",
    "0", "SECTION", "2", "TABLES",
    "0", "TABLE", "2", "LAYER", "70", String(parts.length + 1),
    "0", "LAYER", "2", "0", "70", "0", "62", "7"
  ];
  for (const part of parts) {
    writeLayer(lines, part.layer, part.aci, part.hex);
  }
  lines.push("0", "ENDTAB", "0", "ENDSEC");
  lines.push("0", "SECTION", "2", "ENTITIES");
  for (const part of parts) {
    writeFaces(lines, part.faces, part.layer, part.aci, part.hex);
  }
  lines.push("0", "ENDSEC", "0", "EOF", "");
  fs.writeFileSync(outPath, lines.join("\n"));
}

function aabb(faces) {
  let min = [Infinity, Infinity, Infinity];
  let max = [-Infinity, -Infinity, -Infinity];
  for (const f of faces) {
    for (const v of f) {
      for (let i = 0; i < 3; i++) {
        if (v[i] < min[i]) min[i] = v[i];
        if (v[i] > max[i]) max[i] = v[i];
      }
    }
  }
  return { min, max, size: [max[0] - min[0], max[1] - min[1], max[2] - min[2]] };
}

fs.mkdirSync(outDir, { recursive: true });
let ok = 0;
for (const code of panels) {
  const framePath = path.join(stlDir, code + ".stl");
  const phenolicPath = path.join(stlDir, code + "_F.stl");
  if (!fs.existsSync(framePath)) {
    console.log("SKIP sin STL " + code);
    continue;
  }
  const parts = [
    { layer: "ATK_FRAME", aci: FRAME_ACI, hex: FRAME_HEX, faces: parseStl(framePath) }
  ];
  if (fs.existsSync(phenolicPath)) {
    parts.push({
      layer: "ATK_PHENOLIC",
      aci: PHENOLIC_ACI,
      hex: PHENOLIC_HEX,
      faces: parseStl(phenolicPath)
    });
  }
  const shift = originShift(parts);
  const stats = [];
  for (const part of parts) {
    part.faces = applyShift(part.faces, shift);
    const before = part.faces.length;
    const merged = mergeCoplanarQuads(part.faces);
    part.faces = merged.faces;
    stats.push(part.layer + " " + before + "tri -> " + merged.quads + "quad+" + merged.tris + "tri");
  }
  const all = parts.reduce((acc, p) => acc.concat(p.faces), []);
  if (!all.length) {
    console.log("SKIP vacio " + code);
    continue;
  }
  const outPath = path.join(outDir, code + ".dxf");
  writeDxf(parts, outPath);
  const box = aabb(all);
  const sizeKb = Math.round(fs.statSync(outPath).size / 1024);
  console.log(
    code +
      " " +
      stats.join(" | ") +
      " size=" +
      box.size.map((n) => n.toFixed(3)).join("x") +
      " dxf=" +
      sizeKb +
      "KB"
  );
  ok++;
}
console.log("OK - STL marco+fenolico a 3D DXF: " + ok);
