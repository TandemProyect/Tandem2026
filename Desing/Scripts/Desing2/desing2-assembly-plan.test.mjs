/* Visor de vistas del plano de montaje: node Scripts/Desing2/desing2-assembly-plan.test.mjs (desde Desing/). */
import { register } from 'node:module';

const imports = {
    three: new URL('../Design/build/three.module.js', import.meta.url).href,
    '@masterarticles/STLLoader': new URL('../Design/jsm/loaders/STLLoader.js', import.meta.url).href,
};
register('data:text/javascript,' + encodeURIComponent(
    'const m = ' + JSON.stringify(imports) + ';' +
    'export async function resolve(s, c, n) { return m[s] ? { url: m[s], shortCircuit: true } : n(s, c); }'
));

globalThis.window = globalThis;
globalThis.document = { readyState: 'complete', getElementById: function () { return null; } };

const THREE = await import('three');
const { assemblyFaceMatrix } = await import('./desing2-assembly-view-frame.js');
const { assemblyDimChains, formatMeters } = await import('./desing2-assembly-view-dims.js');
const { buildFaceGroup, applyAssemblyColors } = await import('./desing2-assembly-plan.js');

let failed = 0;
function check(name, ok, info) {
    if (!ok) {
        failed++;
        console.log('FALLA ' + name + (info != null ? ': ' + info : ''));
    }
}
function near(a, b) {
    return Math.abs(a - b) < 1e-6;
}
function at(m, x, y, z) {
    return new THREE.Vector3(x, y, z).applyMatrix4(m);
}
function labels(list) {
    return (list || []).map(function (s) { return s.label; }).join(' ');
}

/* ---------- textos y cadenas de cotas ---------- */
check('0,6m', formatMeters(600) === '0,6m', formatMeters(600));
check('2,11m', formatMeters(2110) === '2,11m', formatMeters(2110));
check('0,43m', formatMeters(430) === '0,43m', formatMeters(430));

/* Como el plano de referencia: 0,6 0,9 0,9 0,9 0,9 0,6 0,6 en dos filas de 2,70 (medidas con 1-2 mm de holgura). */
const widths = [600, 900, 900, 900, 900, 600, 600];
const ref = [];
let x = 0;
for (let r = 0; r < 2; r++) {
    x = 0;
    for (let i = 0; i < widths.length; i++) {
        ref.push({ x0: x + 1, x1: x + widths[i] - 1, y0: r * 2700 + 1, y1: (r + 1) * 2700 - 1, nominalW: widths[i], nominalH: 2700 });
        x += widths[i];
    }
}
const refChains = assemblyDimChains(ref, x);
check('cadena de paneles', labels(refChains.horizontal) === '0,6m 0,9m 0,9m 0,9m 0,9m 0,6m 0,6m', labels(refChains.horizontal));
check('largo total', refChains.total.label === '5,4m' && near(refChains.total.to, 5400), refChains.total.label);
check('alturas 2,7 + 2,7', labels(refChains.vertical) === '2,7m 2,7m', labels(refChains.vertical));
check('altura total 5,4', refChains.verticalTotal && refChains.verticalTotal.label === '5,4m');

const mixed = assemblyDimChains([
    { x0: 0, x1: 602, y0: 0, y1: 2700, nominalW: 600, nominalH: 2700 },
    { x0: 602, x1: 752, y0: 0, y1: 2700, filler: true },
    { x0: 752, x1: 1499, y0: 0, y1: 2700 },
    { x0: 0, x1: 2700, y0: 2700, y1: 3300, nominalW: 600, nominalH: 2700, lying: true },
], 1500);
check('borde: remate entre paneles y medida sin maestro', labels(mixed.horizontal) === '0,6m 0,15m 0,745m', labels(mixed.horizontal));
check('borde: tumbado arriba no entra en la cadena de abajo; su alto es 0,6', labels(mixed.vertical) === '2,7m 0,6m', labels(mixed.vertical));
check('borde: sin paneles solo el largo total', assemblyDimChains([], 2110).horizontal.length === 0 && assemblyDimChains([], 2110).total.label === '2,11m');

/* ---------- matriz: planta → cara vista de frente ---------- */
const view = {
    FaceId: 1, FaceName: 'F-1 Exterior', WallId: '4',
    P1X: 2400, P1Z: -15140, P2X: 2400, P2Z: -17250, BaseY: 0,
    LengthMm: 2110, HeightMm: 2700, UX: 0, UZ: -1, NX: 1, NZ: 0,
    Elements: [
        { ArticleId: 1, Code: '27604207', X: 2400, Y: 0, Z: -15740, RotationZ: 90, LocalX: 600, LocalY: 0, Depth: 0, IsPanel: true, NominalWidthMm: 600, NominalHeightMm: 2700, Color: '#FF0000' },
        { ArticleId: 2, Code: '10443020', X: 2520, Y: 550, Z: -15783.714, RotationZ: 0, LocalX: 643.714, LocalY: 550, Depth: 120 },
        { ArticleId: 3, Code: '10443020', X: 2520, Y: 550, Z: -15783.714, RotationZ: 0, LocalX: 643.714, LocalY: 550, Depth: 120 },
        { ArticleId: 4, Code: '10004220', X: 2360, Y: 2250, Z: -17370, RotationZ: 0, LocalX: 2230, LocalY: 2250, Depth: -40 },
        { ArticleId: 5, Code: 'SIN_STL', X: 2400, Y: 0, Z: -16000, RotationZ: 0, LocalX: 860, LocalY: 0, Depth: 0 },
    ],
};
const m = assemblyFaceMatrix(THREE, view);
const p2 = at(m, 2400, 0, -17250);
check('P1 en el origen', near(at(m, 2400, 0, -15140).length(), 0));
check('P2 a la derecha', near(p2.x, 2110) && near(p2.z, 0));
check('delante de la cara = +Z', near(at(m, 2520, 0, -15140).z, 120));
const scale = new THREE.Vector3();
m.decompose(new THREE.Vector3(), new THREE.Quaternion(), scale);
check('sin espejo ni escala', near(scale.x, 1) && near(scale.y, 1) && near(scale.z, 1));

/* ---------- montaje de la vista ---------- */
function scenePiece(code, el, extra, size, hex) {
    const g = new THREE.Group();
    g.position.set(el.X, el.Y, el.Z);
    g.rotation.y = el.RotationZ * Math.PI / 180;
    g.userData = Object.assign({ maStlAtk60Element: true, maStlAtk60ElementCode: code }, extra || {});
    const s = size || [10, 10, 10];
    const mesh = new THREE.Mesh(new THREE.BoxGeometry(s[0], s[1], s[2]), new THREE.MeshLambertMaterial({ color: hex != null ? hex : 0xefb608 }));
    mesh.position.set(0, s[1] / 2, 0);
    g.add(mesh);
    return g;
}
const formwork = new THREE.Group();
const manual = new THREE.Group();
const els = view.Elements;
// Panel de 0,60 × 2,70 girado 90° (como en la escena): su ancho queda a lo largo de la cara (Z de escena).
formwork.add(scenePiece('PANEL_27604207_FRAME', els[0], null, [600, 2700, 10]));
formwork.add(scenePiece('PANEL_27604207', els[0], { maStlAtk60ImportPath: '/x/27604207_F.stl' }, [600, 2700, 10], 0x1a1816));
formwork.add(scenePiece('10443020', els[1]));
formwork.add(scenePiece('10443020', els[2]));
formwork.add(scenePiece('27604207', Object.assign({}, els[4], { X: els[4].X + 5 })));
const grapa = new THREE.Group();
grapa.position.set(els[3].X, els[3].Y, els[3].Z);
grapa.userData = { maStlManualWallArticle: true, maStlManualArticleId: 4 };
grapa.add(new THREE.Mesh(new THREE.BoxGeometry(10, 10, 10), new THREE.MeshLambertMaterial()));
manual.add(grapa);
window.maStlDesing2AssemblySceneGroups = function () { return { formwork: formwork, manual: manual }; };

const built = await buildFaceGroup(view);
const r = built.result;
check('3 exactas desde el encofrado (panel y dos repetidas)', r.exact === 3, JSON.stringify(r));
check('1 aproximada desde el artículo manual', r.approx === 1, JSON.stringify(r));
check('borde: sin STL ni escena → cubo rojo', r.missing === 1, JSON.stringify(r));
built.group.updateMatrixWorld(true);
const meshes = [];
built.group.children[0].traverse(function (o) { if (o.isMesh) meshes.push(o); });
check('mallas: bastidor + fenólico + 2 repetidas + grapa + cubo', meshes.length === 6, meshes.length);
check('un grupo por pieza', built.holders.length === els.length, built.holders.length);
const origins = built.holders.map(function (h) { return h.children[0]; });
for (let i = 1; i < 4; i++) {
    const e = els[i];
    const p = origins[i].getWorldPosition(new THREE.Vector3());
    const lift = i < 3 ? 5 : 0; // las piezas falsas del encofrado llevan la malla 5 mm por encima de su origen
    check('pieza ' + e.ArticleId + ' en su LocalX / LocalY / Depth', near(p.x, e.LocalX) && near(p.y, e.LocalY + lift) && near(p.z, e.Depth));
}
check('material propio: no toca la escena', meshes[0].material !== formwork.children[0].children[0].material);
check('cotas: el panel de 0,60 entra en la cadena', labels(built.chains.horizontal) === '0,6m', labels(built.chains.horizontal));
check('cotas: largo total de la cara', built.chains.total.label === '2,11m', built.chains.total.label);
check('cotas: altura del panel', labels(built.chains.vertical) === '2,7m', labels(built.chains.vertical));
const face = built.group.children[1];
face.geometry.computeBoundingBox();
const bb = face.geometry.boundingBox;
check('cara de largo × alto, 2 mm detrás', near(bb.min.x, 0) && near(bb.max.x, 2110) && near(bb.max.y, 2700) && near(bb.min.z, -2));
let ground = null;
built.group.traverse(function (o) { if (o.userData && o.userData.assemblyGround) ground = o; });
const gb = ground ? new THREE.Box3().setFromObject(ground) : null;
check('línea de tierra bajo la cara y más larga que ella', gb && near(gb.max.y, 0) && gb.min.x < 0 && gb.max.x > 2110);

/* ---------- modo colores ---------- */
const panelFrame = built.holders[0].children.find(function (c) { return c.userData.assemblyPart === 'frame'; });
const panelPhenolic = built.holders[0].children.find(function (c) { return c.userData.assemblyPart === 'phenolic'; });
check('colores: pinta el bastidor de 1 panel', applyAssemblyColors(built.holders, true) === 1);
check('colores: bastidor con el color del maestro', panelFrame.material.color.getHex() === 0xff0000, panelFrame.material.color.getHexString());
check('colores: el fenólico no cambia', panelPhenolic.material.color.getHex() === 0x1a1816, panelPhenolic.material.color.getHexString());
applyAssemblyColors(built.holders, false);
check('colores: al quitarlo vuelve al color de origen', panelFrame.material.color.getHex() === 0xefb608, panelFrame.material.color.getHexString());

const emptyView = await buildFaceGroup(Object.assign({}, view, { Elements: [] }));
check('borde: vista sin piezas = solo la cara', emptyView.holders.length === 0 && emptyView.chains.horizontal.length === 0);

/* ---------- plantas ---------- */
const { buildPlanGroup } = await import('./desing2-assembly-plan.js');
// Muro recto de 3 m (eje z = 0, grosor 300). Exterior mira a +Z de escena; interior a −Z, P1 enfrente del P2 exterior.
const panel = { ArticleId: 10, Code: '27604207', X: 300, Y: 0, Z: 150, RotationZ: 0, IsPanel: true, NominalWidthMm: 600, NominalHeightMm: 2700, Color: '#FF0000' };
const tie = { ArticleId: 11, Code: '230080', X: 1500, Y: 600, Z: 0, RotationZ: 0, Through: true };
const ext = { FaceId: 1, FaceName: 'F-1 Exterior', FaceSide: 'Exterior', WallDbId: 7, P1X: 0, P1Z: 150, P2X: 3000, P2Z: 150, BaseY: 0, LengthMm: 3000, HeightMm: 2700, UX: 1, UZ: 0, NX: 0, NZ: 1, Elements: [panel, tie] };
const int = { FaceId: 2, FaceName: 'F-2 Interior', FaceSide: 'Interior', WallDbId: 7, P1X: 3000, P1Z: -150, P2X: 0, P2Z: -150, BaseY: 0, LengthMm: 3000, HeightMm: 2700, UX: -1, UZ: 0, NX: 0, NZ: -1, Elements: [tie] };
const planFormwork = new THREE.Group();
planFormwork.add(scenePiece('PANEL_27604207_FRAME', panel, null, [600, 2700, 10]));
window.maStlDesing2AssemblySceneGroups = function () { return { formwork: planFormwork, manual: new THREE.Group() }; };
function planLabels(group) {
    const out = [];
    group.traverse(function (o) { if (o.userData && o.userData.assemblyDimLabel) out.push(o.userData.assemblyDimLabel); });
    return out.sort().join(' ');
}
const wallsPlan = await buildPlanGroup([ext, int], 'walls');
check('planta muros: sin piezas', wallsPlan.holders.length === 0);
check('planta muros: largo de cada cara', planLabels(wallsPlan.group) === '3m 3m', planLabels(wallsPlan.group));
let wallFill = null;
wallsPlan.group.traverse(function (o) { if (o.isMesh && o.geometry && o.geometry.type === 'ShapeGeometry') wallFill = o; });
const wb = wallFill ? new THREE.Box3().setFromObject(wallFill) : null;
check('planta muros: muro de 3 m × 0,30 (Y de CAD = −Z)', wb && near(wb.min.x, 0) && near(wb.max.x, 3000) && near(wb.min.y, -150) && near(wb.max.y, 150));
const formPlan = await buildPlanGroup([ext, int], 'formwork');
check('planta: el tirante de las dos caras sale una vez', formPlan.holders.length === 2, formPlan.holders.length);
check('planta: cotas de panel, leyendas F-n y largo de la cara sin paneles', planLabels(formPlan.group) === '0,6m 3m F-1 F-2', planLabels(formPlan.group));
const pp = formPlan.holders[0].children[0].getWorldPosition(new THREE.Vector3());
check('planta: el panel en X 300, Y de CAD −150, altura hacia arriba', near(pp.x, 300) && near(pp.y, -150) && pp.z > 0, [pp.x, pp.y, pp.z].join(','));
check('borde: planta sin vistas', (await buildPlanGroup([], 'walls')).holders.length === 0);

console.log(failed === 0 ? 'OK desing2-assembly-plan' : 'FALLOS: ' + failed);
process.exit(failed === 0 ? 0 : 1);
