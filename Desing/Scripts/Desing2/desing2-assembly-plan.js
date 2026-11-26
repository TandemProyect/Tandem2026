/**
 * US «Generar plano de montaje» — icono + visor de vistas.
 * 1. El icono pide al servidor las vistas por cara (Desing_2/GenerateAssemblyViews).
 * 2. Sale la lista de vistas (#desing2AssemblyViewsModal).
 * 3. Al elegir una, se pinta la cara y sus piezas en el visor del recuento (#desing2-article-stl,
 *    TandemCadBlockPreview.loadScene): mismas herramientas, vista frontal.
 * Las piezas se copian de la escena (pose exacta del encofrado); si no están, se pintan con su STL.
 */
import * as THREE from 'three';
import { STLLoader } from '@masterarticles/STLLoader';
import { assemblyFaceMatrix } from './desing2-assembly-view-frame.js?v=20261125a';
import { assemblyDimChains, buildAssemblyDims, buildPlanDims, formatMeters, planFaceLabel } from './desing2-assembly-view-dims.js?v=20261125b';
import { DESIGN_FACE_COLORS } from './desing2-wall-faces.js?v=20261120c';

const FRAME_HEX = 0xefb608;
const PHENOLIC_HEX = 0x1a1816;
const PROP_HEX = 0x908f8e;
const MISSING_HEX = 0xd9534f;
const FACE_HEX = 0xbfc3c7;
const FACE_EDGE_HEX = 0x5c6670;
const POSITION_TOL_MM = 1;

const stlLoader = new STLLoader();
const geometryCache = new Map();

function attr(el, name, fallback) {
    return (el && el.getAttribute(name)) || fallback;
}

function format(template, values) {
    return String(template || '').replace(/\{(\d+)\}/g, function (all, i) {
        const v = values[Number(i)];
        return v == null ? all : String(v);
    });
}

function notify(message) {
    if (typeof window.maStlDesing2ShowSaveViewToast === 'function') {
        window.maStlDesing2ShowSaveViewToast(message);
    } else if (message) {
        window.alert(message);
    }
}

function overlay(visible, message) {
    if (typeof window.maStlDesing2SetPersistOverlay === 'function') {
        window.maStlDesing2SetPersistOverlay(visible, 'save', message);
    }
}

function designId() {
    const shell = document.getElementById('ma-stl-viewer-shell');
    const n = Number(shell ? shell.getAttribute('data-ma-stl-design-id') || '' : '');
    if (Number.isFinite(n) && n > 0) return n;
    try {
        const q = Number(new URLSearchParams(window.location.search).get('designId') || '');
        if (Number.isFinite(q) && q > 0) return q;
    } catch (_e) { /* sin query */ }
    return 0;
}

function formatDeg(value) {
    const n = Number(value);
    if (!Number.isFinite(n)) return '';
    return (Math.round(n * 1000) / 1000).toLocaleString(undefined, { maximumFractionDigits: 3 }) + '°';
}

function normCode(raw) {
    return String(raw || '').trim()
        .replace(/^PANEL_/i, '')
        .replace(/_FRAME$/i, '')
        .replace(/_PHENOLIC$/i, '')
        .toUpperCase();
}

function isGrayCode(code) {
    return /^(?:HTIPO|H0_|H03|10000221|1120100040)/i.test(normCode(code));
}

/* ---------- piezas de la escena ---------- */

function sceneIndex() {
    const groups = typeof window.maStlDesing2AssemblySceneGroups === 'function'
        ? window.maStlDesing2AssemblySceneGroups()
        : null;
    const index = { formworkRoot: null, formwork: [], manualRoot: null, manual: new Map() };
    if (groups && groups.formwork) {
        index.formworkRoot = groups.formwork;
        groups.formwork.traverse(function (obj) {
            const ud = obj && obj.userData;
            if (!ud || (!ud.maStlAtk60Element && !ud.maStlAtk60SamplePlaced)) return;
            const pud = obj.parent && obj.parent.userData;
            if (pud && (pud.maStlAtk60Element || pud.maStlAtk60SamplePlaced)) return;
            index.formwork.push({
                obj: obj,
                code: normCode(ud.maStlAtk60ElementCode),
                phenolic: /_PHENOLIC$/i.test(String(ud.maStlAtk60ElementCode || ''))
                    || /_F\.stl$/i.test(String(ud.maStlAtk60ImportPath || '')),
                used: false,
                x: obj.position.x,
                y: obj.position.y,
                z: obj.position.z,
            });
        });
    }
    if (groups && groups.manual) {
        index.manualRoot = groups.manual;
        const kids = groups.manual.children || [];
        for (let i = 0; i < kids.length; i++) {
            const ud = kids[i] && kids[i].userData;
            if (ud && ud.maStlManualWallArticle && ud.maStlManualArticleId != null) {
                index.manual.set(String(ud.maStlManualArticleId), kids[i]);
            }
        }
    }
    return index;
}

/** Objetos de la escena de esta pieza: uno de bastidor y, si hay, uno de fenólico. Cada objeto se usa una vez (piezas repetidas). */
function formworkMatches(index, el) {
    const code = normCode(el.Code);
    const x = Number(el.X);
    const y = Number(el.Y);
    const z = Number(el.Z);
    let main = null;
    let phenolic = null;
    for (let i = 0; i < index.formwork.length; i++) {
        const f = index.formwork[i];
        if (f.used || f.code !== code) continue;
        if (Math.abs(f.x - x) > POSITION_TOL_MM || Math.abs(f.y - y) > POSITION_TOL_MM || Math.abs(f.z - z) > POSITION_TOL_MM) continue;
        if (f.phenolic && !phenolic) phenolic = f;
        else if (!f.phenolic && !main) main = f;
        if (main && phenolic) break;
    }
    const out = [];
    if (main) {
        main.used = true;
        out.push({ obj: main.obj, phenolic: false });
    }
    if (phenolic) {
        phenolic.used = true;
        out.push({ obj: phenolic.obj, phenolic: true });
    }
    return out;
}

function ownMaterial(material) {
    if (!material) return material;
    const copy = material.clone();
    copy.clippingPlanes = [];
    copy.transparent = false;
    copy.opacity = 1;
    copy.depthWrite = true;
    copy.needsUpdate = true;
    return copy;
}

/** Fenólico = casi negro (sRGB). Sirve para piezas copiadas que traen bastidor y fenólico juntos. */
function looksPhenolic(material) {
    const m = Array.isArray(material) ? material[0] : material;
    if (!m || !m.color || typeof m.color.getHex !== 'function') return false;
    const hex = m.color.getHex();
    const lum = (((hex >> 16) & 255) * 0.299 + ((hex >> 8) & 255) * 0.587 + (hex & 255) * 0.114) / 255;
    return lum < 0.16;
}

function tagPart(mesh, part) {
    const m = Array.isArray(mesh.material) ? mesh.material[0] : mesh.material;
    mesh.userData.assemblyPart = part;
    mesh.userData.assemblyBaseHex = m && m.color && typeof m.color.getHex === 'function' ? m.color.getHex() : null;
}

/** Copia las mallas de `obj` (sin userData) con su pose relativa a `root` (coordenadas guardadas). */
function addSceneMeshes(target, obj, root, phenolicObject) {
    if (root) root.updateWorldMatrix(true, false);
    obj.updateWorldMatrix(true, true);
    const toRoot = new THREE.Matrix4();
    if (root) toRoot.copy(root.matrixWorld).invert();
    let added = 0;
    obj.traverse(function (child) {
        if (!child || !child.isMesh || !child.geometry) return;
        const material = Array.isArray(child.material)
            ? child.material.map(ownMaterial)
            : ownMaterial(child.material);
        const mesh = new THREE.Mesh(child.geometry, material);
        new THREE.Matrix4().multiplyMatrices(toRoot, child.matrixWorld)
            .decompose(mesh.position, mesh.quaternion, mesh.scale);
        tagPart(mesh, phenolicObject || looksPhenolic(material) ? 'phenolic' : 'frame');
        target.add(mesh);
        added++;
    });
    return added;
}

/* ---------- piezas desde STL (si no están en la escena) ---------- */

function loadGeometry(url) {
    if (!url) return Promise.resolve(null);
    if (!geometryCache.has(url)) {
        geometryCache.set(url, new Promise(function (resolve) {
            stlLoader.load(url, function (geometry) {
                geometry.computeVertexNormals();
                resolve(geometry);
            }, undefined, function () { resolve(null); });
        }).catch(function () { return null; }));
    }
    return geometryCache.get(url);
}

function yawDegOf(el) {
    const rawZ = Number(el.RotationZ) || 0;
    if (Math.abs(rawZ) < 3000) return rawZ;
    // Pose de AutoCAD codificada (mismo criterio que los artículos manuales del visor).
    const sign = rawZ < 0 ? -1 : 1;
    const spun = Math.abs(rawZ) >= 4500;
    let yaw = rawZ - sign * (spun ? 5000 : 4000);
    if (spun) yaw -= 180;
    return yaw;
}

function addStlPiece(target, el) {
    const urls = [el.StlUrl, el.StlPhenolicUrl].filter(function (u) { return !!u; });
    return Promise.all(urls.map(loadGeometry)).then(function (geoms) {
        const root = new THREE.Group();
        const gray = isGrayCode(el.Code);
        for (let i = 0; i < geoms.length; i++) {
            if (!geoms[i]) continue;
            const mesh = new THREE.Mesh(geoms[i], new THREE.MeshLambertMaterial({
                color: gray ? PROP_HEX : (i === 0 ? FRAME_HEX : PHENOLIC_HEX),
                side: THREE.DoubleSide,
            }));
            mesh.rotation.x = -0.5 * Math.PI;
            tagPart(mesh, i === 0 ? 'frame' : 'phenolic');
            root.add(mesh);
        }
        const found = root.children.length > 0;
        if (!found) {
            const cube = new THREE.Mesh(
                new THREE.BoxGeometry(80, 80, 80),
                new THREE.MeshLambertMaterial({ color: MISSING_HEX })
            );
            tagPart(cube, 'other');
            root.add(cube);
            target.userData.assemblyPlaceholder = true;
        } else {
            root.updateMatrixWorld(true);
            const size = new THREE.Box3().setFromObject(root).getSize(new THREE.Vector3());
            const maxDim = Math.max(size.x, size.y, size.z);
            if (Number.isFinite(maxDim) && maxDim > 1e-6 && maxDim < 50) root.scale.multiplyScalar(1000);
        }
        root.rotation.order = 'YZX';
        root.rotation.set(0, yawDegOf(el) * Math.PI / 180, 0);
        if (Math.abs((Number(el.RotationX) || 0) - 90) < 1) root.rotateZ(Math.PI / 2);
        root.position.set(Number(el.X) || 0, Number(el.Y) || 0, Number(el.Z) || 0);
        target.add(root);
        return found;
    });
}

/* ---------- cara ---------- */

function faceMeshes(view) {
    const length = Math.max(Number(view.LengthMm) || 0, 1);
    const height = Math.max(Number(view.HeightMm) || 0, 1);
    const plane = new THREE.PlaneGeometry(length, height);
    plane.translate(length / 2, height / 2, -2);
    const face = new THREE.Mesh(plane, new THREE.MeshLambertMaterial({
        color: FACE_HEX,
        side: THREE.DoubleSide,
        transparent: true,
        opacity: 0.45,
        depthWrite: false,
    }));
    const edges = new THREE.LineSegments(
        new THREE.EdgesGeometry(plane),
        new THREE.LineBasicMaterial({ color: FACE_EDGE_HEX })
    );
    return [face, edges];
}

/**
 * Coloca la pieza `el` (coordenadas de escena) en `holder`: copia del encofrado de la escena, artículo manual
 * o su STL. Devuelve null si ya está o una promesa si hay que cargar el STL. Cuenta en `result`.
 */
function addElement(holder, el, index, result) {
    holder.userData.assemblyElement = el;
    const hits = formworkMatches(index, el);
    if (hits.length) {
        for (let h = 0; h < hits.length; h++) addSceneMeshes(holder, hits[h].obj, index.formworkRoot, hits[h].phenolic);
        result.exact++;
        return null;
    }
    const manual = el.ArticleId != null ? index.manual.get(String(el.ArticleId)) : null;
    if (manual && addSceneMeshes(holder, manual, index.manualRoot, false) > 0) {
        result.approx++;
        return null;
    }
    return addStlPiece(holder, el).then(function (found) {
        if (found) result.approx++;
        else result.missing++;
    });
}

function buildFaceGroup(view) {
    const index = sceneIndex();
    const world = new THREE.Group();
    const stlJobs = [];
    const holders = [];
    const result = { exact: 0, approx: 0, missing: 0 };
    const elements = Array.isArray(view.Elements) ? view.Elements : [];
    for (let i = 0; i < elements.length; i++) {
        // Un grupo por pieza: sirve para medir paneles (cotas) y para pintarlos (colores).
        const holder = new THREE.Group();
        world.add(holder);
        holders.push(holder);
        const job = addElement(holder, elements[i] || {}, index, result);
        if (job) stlJobs.push(job);
    }
    return Promise.all(stlJobs).then(function () {
        world.applyMatrix4(assemblyFaceMatrix(THREE, view));
        const group = new THREE.Group();
        group.add(world);
        const face = faceMeshes(view);
        for (let i = 0; i < face.length; i++) group.add(face[i]);
        group.updateMatrixWorld(true);

        const length = Math.max(Number(view.LengthMm) || 0, 1);
        const height = Math.max(Number(view.HeightMm) || 0, 1);
        const measured = [];
        const box = new THREE.Box3();
        let topY = height;
        let leftX = 0;
        let rightX = length;
        for (let i = 0; i < holders.length; i++) {
            const el = holders[i].userData.assemblyElement;
            if (!(el.IsPanel || el.IsFiller) || holders[i].userData.assemblyPlaceholder) continue;
            box.setFromObject(holders[i]);
            if (box.isEmpty()) continue;
            measured.push({
                x0: box.min.x, x1: box.max.x, y0: box.min.y, y1: box.max.y,
                nominalW: Number(el.NominalWidthMm) || 0,
                nominalH: Number(el.NominalHeightMm) || 0,
                lying: el.Lying === true,
                filler: el.IsFiller === true,
            });
            topY = Math.max(topY, box.max.y);
            leftX = Math.min(leftX, box.min.x);
            rightX = Math.max(rightX, box.max.x);
        }
        const all = new THREE.Box3().setFromObject(world);
        const chains = assemblyDimChains(measured, length);
        group.add(buildAssemblyDims(THREE, chains, {
            lengthMm: length,
            topY: topY,
            leftX: leftX,
            rightX: rightX,
            zFront: all.isEmpty() ? 300 : all.max.z,
            zBack: all.isEmpty() ? -300 : all.min.z,
        }));
        return { group: group, result: result, holders: holders, chains: chains };
    });
}

/**
 * Modo colores: el bastidor de cada panel vertical con el color del maestro (como «Colores» del visor).
 * El fenólico, los tumbados y el resto de piezas no cambian. `on = false` vuelve al color de origen.
 */
function applyAssemblyColors(holders, on) {
    let painted = 0;
    for (let i = 0; i < (holders || []).length; i++) {
        const el = holders[i].userData.assemblyElement || {};
        const match = /^#?([0-9a-fA-F]{6})$/.exec(String(el.Color || '').trim());
        const paintHex = on && el.IsPanel && !el.Lying && match ? parseInt(match[1], 16) : null;
        holders[i].traverse(function (mesh) {
            if (!mesh || !mesh.isMesh || mesh.userData.assemblyPart !== 'frame') return;
            const target = paintHex != null ? paintHex : mesh.userData.assemblyBaseHex;
            if (target == null) return;
            const mats = Array.isArray(mesh.material) ? mesh.material : [mesh.material];
            for (let k = 0; k < mats.length; k++) {
                if (mats[k] && mats[k].color) mats[k].color.setHex(target);
            }
            if (paintHex != null) painted++;
        });
    }
    return painted;
}

export { buildFaceGroup, applyAssemblyColors };

/* ---------- plantas ---------- */

const WALL_HEX = 0xb0b6bd;
const WALL_EDGE_HEX = 0x2f3740;

/** Escena (x, y arriba, z) → planta (x, Y de CAD = −z, altura hacia quien mira). */
function planPoint(x, z) {
    return { x: x, y: -z };
}

function faceColorHex(view) {
    const horizontal = Math.abs(Number(view.UX) || 0) >= Math.abs(Number(view.UZ) || 0);
    const interior = String(view.FaceSide || '').toLowerCase() === 'interior';
    if (horizontal) return interior ? DESIGN_FACE_COLORS.horizontalInterior : DESIGN_FACE_COLORS.horizontalExterior;
    return interior ? DESIGN_FACE_COLORS.verticalInterior : DESIGN_FACE_COLORS.verticalExterior;
}

/** Planta del muro: exterior P1 → P2 y, enfrente, interior P1 → P2 (mismo extremo que el P2 exterior). */
function wallPolygons(views, z) {
    const byWall = new Map();
    for (let i = 0; i < views.length; i++) {
        const v = views[i];
        const key = String(v.WallDbId);
        const pair = byWall.get(key) || {};
        if (String(v.FaceSide || '').toLowerCase() === 'interior') pair.int = v;
        else pair.ext = v;
        byWall.set(key, pair);
    }
    const out = [];
    byWall.forEach(function (pair) {
        if (!pair.ext || !pair.int) return;
        const pts = [
            planPoint(pair.ext.P1X, pair.ext.P1Z),
            planPoint(pair.ext.P2X, pair.ext.P2Z),
            planPoint(pair.int.P1X, pair.int.P1Z),
            planPoint(pair.int.P2X, pair.int.P2Z),
        ];
        const shape = new THREE.Shape(pts.map(function (p) { return new THREE.Vector2(p.x, p.y); }));
        const fill = new THREE.Mesh(new THREE.ShapeGeometry(shape), new THREE.MeshBasicMaterial({ color: WALL_HEX, side: THREE.DoubleSide }));
        fill.position.z = z;
        const outline = new THREE.LineLoop(
            new THREE.BufferGeometry().setFromPoints(pts.map(function (p) { return new THREE.Vector3(p.x, p.y, z + 1); })),
            new THREE.LineBasicMaterial({ color: WALL_EDGE_HEX })
        );
        out.push(fill, outline);
    });
    return out;
}

/**
 * Planta del diseño a partir de las vistas por cara (respuestas de Desing_2/AssemblyView).
 * mode 'formwork': muros, encofrado, leyendas F-n y cotas de paneles por cara.
 * mode 'walls': solo muros y la longitud de cada cara.
 * Vista de frente = desde arriba: X a la derecha, Y de CAD hacia arriba.
 */
function buildPlanGroup(views, mode) {
    const list = Array.isArray(views) ? views : [];
    const formwork = mode === 'formwork';
    const index = sceneIndex();
    const world = new THREE.Group();
    const holders = [];
    const jobs = [];
    const result = { exact: 0, approx: 0, missing: 0 };
    if (formwork) {
        const seen = new Set();
        for (let v = 0; v < list.length; v++) {
            const els = Array.isArray(list[v].Elements) ? list[v].Elements : [];
            for (let i = 0; i < els.length; i++) {
                const el = els[i] || {};
                // Las piezas pasantes (tirantes) salen en dos caras: en planta, una vez.
                const key = el.ArticleId != null ? 'a' + el.ArticleId : 'v' + v + '-' + i;
                if (seen.has(key)) continue;
                seen.add(key);
                const holder = new THREE.Group();
                world.add(holder);
                holders.push(holder);
                const job = addElement(holder, el, index, result);
                if (job) jobs.push(job);
            }
        }
    }
    const faceJobs = formwork
        ? list.map(function (view) { return buildFaceGroup(view).then(function (b) { return b.chains; }); })
        : [];
    return Promise.all(jobs.concat([Promise.all(faceJobs)])).then(function (all) {
        const chainsByFace = all[all.length - 1] || [];
        world.applyMatrix4(new THREE.Matrix4().makeRotationX(Math.PI / 2));
        const group = new THREE.Group();
        group.add(world);
        world.updateMatrixWorld(true);
        const pieces = new THREE.Box3().setFromObject(world);
        const zTop = pieces.isEmpty() ? 3000 : Math.max(pieces.max.z, 100) + 50;

        const walls = wallPolygons(list, -5);
        for (let i = 0; i < walls.length; i++) group.add(walls[i]);

        let minX = Infinity; let maxX = -Infinity; let minY = Infinity; let maxY = -Infinity;
        for (let i = 0; i < list.length; i++) {
            const a = planPoint(list[i].P1X, list[i].P1Z);
            const b = planPoint(list[i].P2X, list[i].P2Z);
            minX = Math.min(minX, a.x, b.x); maxX = Math.max(maxX, a.x, b.x);
            minY = Math.min(minY, a.y, b.y); maxY = Math.max(maxY, a.y, b.y);
        }
        const span = Number.isFinite(minX) ? Math.max(maxX - minX, maxY - minY, 1000) : 1000;
        const t = Math.min(260, Math.max(70, span * 0.012));

        const rows = [];
        for (let i = 0; i < list.length; i++) {
            const view = list[i];
            const a = planPoint(view.P1X, view.P1Z);
            const u = { x: Number(view.UX) || 0, y: -(Number(view.UZ) || 0) };
            const n = { x: Number(view.NX) || 0, y: -(Number(view.NZ) || 0) };
            const length = Number(view.LengthMm) || 0;
            let stops = [0, length];
            let labels = [formatMeters(length)];
            let offset = 2.2 * t;
            if (formwork) {
                const chain = (chainsByFace[i] && chainsByFace[i].horizontal) || [];
                if (chain.length) {
                    stops = [chain[0].from].concat(chain.map(function (s) { return s.to; }));
                    labels = chain.map(function (s) { return s.label; });
                }
                offset = 2.2 * t + 150;
                const mid = { x: a.x + u.x * length / 2 + n.x * (offset + 2.6 * t), y: a.y + u.y * length / 2 + n.y * (offset + 2.6 * t) };
                group.add(planFaceLabel(THREE, view.FaceName ? String(view.FaceName).split(' ')[0] : 'F-' + view.FaceId, faceColorHex(view), t, mid.x, mid.y, zTop));
            }
            rows.push({ ax: a.x, ay: a.y, ux: u.x, uy: u.y, nx: n.x, ny: n.y, offset: offset, stops: stops, labels: labels });
        }
        group.add(buildPlanDims(THREE, rows, t, zTop));
        return { group: group, result: result, holders: holders };
    });
}

export { buildPlanGroup };

/* ---------- lista y visor ---------- */

function mount() {
    const btn = document.getElementById('ma-stl-assembly-plan');
    if (!btn) return;
    const modalEl = document.getElementById('desing2AssemblyViewsModal');
    const body = document.getElementById('desing2AssemblyViewsBody');
    const searchInput = document.getElementById('desing2AssemblyViewsSearch');
    let busy = false;
    let openToken = 0;

    /* Visible solo con Encofrar o Colores activos (ahí están los materiales en pantalla). */
    function syncPlanButton() {
        const formwork = document.getElementById('ma-stl-mode-formwork');
        const colors = document.getElementById('ma-stl-mode-formwork-color');
        const on = (formwork && formwork.getAttribute('aria-pressed') === 'true')
            || (colors && colors.getAttribute('aria-pressed') === 'true');
        btn.style.display = on ? '' : 'none';
    }
    ['ma-stl-mode-formwork', 'ma-stl-mode-formwork-color'].forEach(function (modeId) {
        const modeBtn = document.getElementById(modeId);
        if (modeBtn && typeof MutationObserver === 'function') {
            new MutationObserver(syncPlanButton).observe(modeBtn, { attributes: true, attributeFilter: ['aria-pressed', 'class'] });
        }
    });
    syncPlanButton();
    let lastDesignId = 0;
    let colorsOn = false;
    let currentHolders = null;
    let viewOpen = false;
    const colorsBtn = document.getElementById('plugin-blocks-tool-colors');

    function syncColorsButton(show) {
        if (!colorsBtn) return;
        colorsBtn.style.display = show ? '' : 'none';
        colorsBtn.classList.toggle('is-on', colorsOn);
        colorsBtn.setAttribute('aria-pressed', colorsOn ? 'true' : 'false');
    }

    function showFaces(on, faceId) {
        if (typeof window.maStlDesing2AssemblyShowFaces === 'function') window.maStlDesing2AssemblyShowFaces(on, faceId);
    }

    const canvasHost = document.getElementById('master-article-details-stl-viewer-canvas');
    const closeListBtn = document.getElementById('desing2AssemblyViewsClose');
    const headEl = document.getElementById('desing2AssemblyViewsHead');
    let selectedFaceId = 0;
    let lastViews = [];
    const viewCache = new Map();
    const PLAN_FORMWORK = -1;
    const PLAN_WALLS = -2;

    function listIsOpen() {
        return !!(modalEl && !modalEl.hidden);
    }

    function closeList() {
        if (!modalEl) return;
        modalEl.hidden = true;
        modalEl.setAttribute('aria-hidden', 'true');
        // Lista cerrada sin ninguna vista abierta: se quitan las etiquetas de la escena.
        if (!viewOpen) showFaces(false, null);
    }

    if (closeListBtn) closeListBtn.addEventListener('click', function (ev) {
        ev.preventDefault();
        closeList();
    });

    /* Se arrastra por la cabecera dentro del visor; no tapa el diseño ni bloquea el resto. */
    if (headEl && modalEl) {
        headEl.addEventListener('pointerdown', function (ev) {
            if (ev.button !== 0 || (ev.target.closest && ev.target.closest('button, input, a'))) return;
            const parent = modalEl.offsetParent || modalEl.parentElement;
            if (!parent) return;
            const start = modalEl.getBoundingClientRect();
            const dx = ev.clientX - start.left;
            const dy = ev.clientY - start.top;
            headEl.classList.add('is-dragging');
            headEl.setPointerCapture(ev.pointerId);
            function move(e) {
                const host = parent.getBoundingClientRect();
                const x = Math.max(0, Math.min(e.clientX - host.left - dx, host.width - start.width));
                const y = Math.max(0, Math.min(e.clientY - host.top - dy, host.height - 40));
                modalEl.style.left = Math.round(x) + 'px';
                modalEl.style.top = Math.round(y) + 'px';
            }
            function up(e) {
                headEl.classList.remove('is-dragging');
                headEl.removeEventListener('pointermove', move);
                headEl.removeEventListener('pointerup', up);
                try { headEl.releasePointerCapture(e.pointerId); } catch (_err) { /* ya suelto */ }
            }
            headEl.addEventListener('pointermove', move);
            headEl.addEventListener('pointerup', up);
            ev.preventDefault();
        });
    }

    function markSelected() {
        if (!body) return;
        const rows = body.querySelectorAll('tr[data-face-id]');
        for (let i = 0; i < rows.length; i++) {
            rows[i].classList.toggle('is-selected', Number(rows[i].getAttribute('data-face-id')) === selectedFaceId);
        }
    }

    if (colorsBtn) {
        colorsBtn.addEventListener('click', function (ev) {
            ev.preventDefault();
            colorsOn = !colorsOn;
            if (currentHolders) applyAssemblyColors(currentHolders, colorsOn);
            syncColorsButton(true);
        });
    }

    function listText(name, fallback) {
        return attr(modalEl, name, fallback);
    }

    function placedPieces(data) {
        return Math.max(0, (Number(data.pieces) || 0) - (Number(data.outside) || 0) - (Number(data.withoutFace) || 0));
    }

    function resultMessage(data) {
        const views = Array.isArray(data.views) ? data.views.length : 0;
        if (views === 0) return attr(btn, 'data-no-walls', 'El diseño no tiene muros guardados');
        const placed = placedPieces(data);
        if (!(placed > 0)) return attr(btn, 'data-no-pieces', 'No hay encofrado: pulsa Encofrar y vuelve a generar el plano de montaje');
        let message = format(attr(btn, 'data-done', 'Plano de montaje: {0} vistas con {1} piezas'), [views, placed]);
        if (Number(data.withoutFace) > 0) {
            message += ' · ' + format(attr(btn, 'data-without-face', '{0} piezas sin cara'), [data.withoutFace]);
        }
        if (Number(data.outside) > 0) {
            message += ' · ' + format(attr(btn, 'data-outside', '{0} piezas de muros que ya no existen: no salen'), [data.outside]);
        }
        return message;
    }

    function cell(textValue, cls) {
        const td = document.createElement('td');
        if (cls) td.className = cls;
        td.textContent = textValue == null ? '' : String(textValue);
        return td;
    }

    function addListRow(id, name, title, icon, onOpen) {
        const openTitle = listText('data-open', 'Ver la vista');
        const tr = document.createElement('tr');
        tr.className = 'desing2-assembly-views__row';
        tr.style.cursor = 'pointer';
        tr.setAttribute('data-face-id', String(id));
        tr.title = title;
        tr.appendChild(cell(name));
        const action = document.createElement('td');
        action.className = 'desing2-material-count__row-actions';
        const view = document.createElement('button');
        view.type = 'button';
        view.className = 'btn btn-sm btn-icon btn-text-secondary rounded-pill';
        view.title = openTitle;
        view.setAttribute('aria-label', openTitle + ' ' + name);
        view.innerHTML = '<i class="icon-base ri ' + icon + '" aria-hidden="true"></i>';
        action.appendChild(view);
        tr.appendChild(action);
        tr.addEventListener('click', onOpen);
        body.appendChild(tr);
    }

    function paintList(views) {
        body.innerHTML = '';
        lastViews = views.slice();
        if (!views.length) {
            const tr = document.createElement('tr');
            tr.className = 'desing2-group-sheet__empty';
            const td = cell(listText('data-empty', 'No hay vistas'));
            td.colSpan = 2;
            tr.appendChild(td);
            body.appendChild(tr);
            return;
        }
        const planName = listText('data-plan-formwork', 'Planta');
        const wallsName = listText('data-plan-walls', 'Planta muros');
        addListRow(PLAN_FORMWORK, planName, planName, 'ri-layout-grid-line', function () { openPlan('formwork', planName); });
        addListRow(PLAN_WALLS, wallsName, wallsName, 'ri-layout-grid-line', function () { openPlan('walls', wallsName); });
        for (let i = 0; i < views.length; i++) {
            const v = views[i] || {};
            const title = (v.FaceName || '') + ' · Muro ' + (v.WallId || '') + ' · ' + formatDeg(v.WallAngleDeg) + ' · ' + (v.ElementCount || 0);
            addListRow(Number(v.FaceId), v.FaceName || '', title, 'ri-box-3-line', function () {
                openView(Number(v.FaceId), v);
            });
        }
        markSelected();
    }

    function fetchView(faceId) {
        const key = lastDesignId + ':' + faceId;
        if (!viewCache.has(key)) {
            const url = listText('data-view-url', '');
            const join = url.indexOf('?') >= 0 ? '&' : '?';
            viewCache.set(key, fetch(url + join + 'designId=' + encodeURIComponent(String(lastDesignId)) + '&faceId=' + encodeURIComponent(String(faceId)), {
                credentials: 'same-origin',
                headers: { Accept: 'application/json' },
            })
                .then(function (response) { return response.json(); })
                .then(function (data) {
                    if (!data || data.ok === false || !data.view) {
                        throw new Error((data && (data.message || data.Message)) || listText('data-error', 'No se pudo cargar la vista'));
                    }
                    return data.view;
                })
                .catch(function (err) {
                    viewCache.delete(key);
                    throw err;
                }));
        }
        return viewCache.get(key);
    }

    /** Planta: todas las caras juntas, desde arriba, en el mismo visor. */
    function openPlan(mode, title) {
        const box = window.Desing2ArticleStlBox;
        const preview = window.TandemCadBlockPreview;
        if (!box || !preview || typeof preview.loadScene !== 'function') {
            notify(listText('data-error', 'No se pudo cargar la vista'));
            return;
        }
        selectedFaceId = mode === 'walls' ? PLAN_WALLS : PLAN_FORMWORK;
        markSelected();
        const token = ++openToken;
        viewOpen = true;
        const host = box.open(title, listText('data-loading', 'Cargando vista…'));
        currentHolders = null;
        Promise.all(lastViews.map(function (v) { return fetchView(Number(v.FaceId)); }))
            .then(function (views) { return buildPlanGroup(views, mode); })
            .then(function (built) {
                if (token !== openToken) return;
                currentHolders = built.holders;
                if (colorsOn) applyAssemblyColors(currentHolders, true);
                preview.loadScene(host, built.group, box.missingText(), 'front');
                syncColorsButton(mode === 'formwork');
                showFaces(true, null);
                let status = title + ' · ' + format(listText('data-plan-status', '{0} vistas'), [lastViews.length]);
                const rough = built.result.approx + built.result.missing;
                if (rough > 0) status += ' · ' + format(listText('data-approx', '{0} aproximadas: encofra para verlas exactas'), [rough]);
                box.status(status);
            })
            .catch(function (err) {
                if (token !== openToken) return;
                const message = (err && err.message) || listText('data-error', 'No se pudo cargar la vista');
                box.status(message);
                notify(message);
            });
    }

    function openList(views) {
        if (!modalEl || !body) return;
        const host = canvasHost || document.body;
        if (modalEl.parentNode !== host) host.appendChild(modalEl);
        if (searchInput) searchInput.value = '';
        paintList(views);
        modalEl.hidden = false;
        modalEl.setAttribute('aria-hidden', 'false');
    }

    function openView(faceId, summary) {
        const box = window.Desing2ArticleStlBox;
        const preview = window.TandemCadBlockPreview;
        if (!box || !preview || typeof preview.loadScene !== 'function') {
            notify(listText('data-error', 'No se pudo cargar la vista'));
            return;
        }
        // La lista sigue abierta: se puede pasar de una cara a otra cuando se quiera.
        selectedFaceId = faceId;
        markSelected();
        const token = ++openToken;
        viewOpen = true;
        const title = (summary && summary.FaceName) || ('F-' + faceId);
        const host = box.open(title, listText('data-loading', 'Cargando vista…'));
        currentHolders = null;
        fetchView(faceId)
            .then(function (view) {
                return buildFaceGroup(view).then(function (built) {
                    return { view: view, built: built };
                });
            })
            .then(function (done) {
                if (token !== openToken) return;
                currentHolders = done.built.holders;
                if (colorsOn) applyAssemblyColors(currentHolders, true);
                preview.loadScene(host, done.built.group, box.missingText(), 'front');
                syncColorsButton(true);
                showFaces(true, faceId);
                const view = done.view;
                const count = Array.isArray(view.Elements) ? view.Elements.length : 0;
                let status = format(listText('data-status', '{0} · Muro {1} · {2} piezas'), [view.FaceName, view.WallId || '', count]);
                const rough = done.built.result.approx + done.built.result.missing;
                if (rough > 0) status += ' · ' + format(listText('data-approx', '{0} aproximadas: encofra para verlas exactas'), [rough]);
                box.status(status);
            })
            .catch(function (err) {
                if (token !== openToken) return;
                const message = (err && err.message) || listText('data-error', 'No se pudo cargar la vista');
                box.status(message);
                notify(message);
            });
    }

    if (searchInput && body) {
        searchInput.addEventListener('input', function () {
            const q = String(searchInput.value || '').trim().toLowerCase();
            const rows = body.querySelectorAll('tr');
            for (let i = 0; i < rows.length; i++) {
                rows[i].hidden = !!q && rows[i].textContent.toLowerCase().indexOf(q) < 0;
            }
        });
    }

    btn.addEventListener('click', function (ev) {
        ev.preventDefault();
        if (busy) return;
        const id = designId();
        if (!id) {
            notify(attr(btn, 'data-need-design', 'Abre un diseño para generar el plano de montaje'));
            return;
        }
        const url = attr(btn, 'data-url', '');
        if (!url) return;
        const failed = attr(btn, 'data-error', 'No se pudo generar el plano de montaje');
        const working = attr(btn, 'data-working', 'Generando plano de montaje…');

        function generate(force) {
            return fetch(url, {
                method: 'POST',
                credentials: 'same-origin',
                headers: { 'Content-Type': 'application/x-www-form-urlencoded', Accept: 'application/json' },
                body: 'designId=' + encodeURIComponent(String(id)) + (force ? '&force=true' : ''),
            })
                .then(function (response) { return response.json(); })
                .then(function (data) {
                    if (!data || data.ok === false) throw new Error((data && (data.message || data.Message)) || failed);
                    return data;
                });
        }

        /* El icono solo está en Encofrar / Colores: lo que se ve es el diseño y su encofrado actuales.
           1) Muros sin guardar → se guardan. 2) Recuento ausente o anterior a los muros → se guarda el encofrado
           de pantalla (como Guardar del recuento). 3) Se genera; si se guardó algo, se fuerza. */
        function saveDesignIfNeeded() {
            const save = window.maStlDesing2SaveDesignWallsToServer;
            if (typeof save !== 'function') return Promise.resolve(false);
            let changed = false;
            let step = Promise.resolve();
            if (typeof window.maStlDesing2WallsHaveUnsavedChanges === 'function' && window.maStlDesing2WallsHaveUnsavedChanges()) {
                step = step.then(function () {
                    overlay(true, attr(btn, 'data-saving-walls', 'Guardando los muros…'));
                    return save({ includePhoto: false, quiet: true });
                }).then(function (walls) {
                    if (!walls || walls.ok !== true) throw new Error((walls && walls.message) || failed);
                    changed = true;
                });
            }
            return step
                .then(function () {
                    const stateUrl = attr(btn, 'data-state-url', '');
                    if (!stateUrl) return { ok: false };
                    const join = stateUrl.indexOf('?') >= 0 ? '&' : '?';
                    return fetch(stateUrl + join + 'designId=' + encodeURIComponent(String(id)), {
                        credentials: 'same-origin',
                        headers: { Accept: 'application/json' },
                    }).then(function (response) { return response.json(); });
                })
                .then(function (state) {
                    const current = state && state.ok !== false && Number(state.pieceCount) > 0 && state.stale !== true;
                    if (current || sceneIndex().formwork.length === 0) return changed;
                    overlay(true, attr(btn, 'data-saving', 'Guardando el recuento…'));
                    return save({ articlesOnly: true, includePhoto: false, quiet: true }).then(function (saved) {
                        if (!saved || saved.ok !== true || !(Number(saved.articleCount) > 0)) {
                            throw new Error((saved && saved.message) || failed);
                        }
                        return true;
                    });
                });
        }

        busy = true;
        btn.disabled = true;
        overlay(true, working);
        saveDesignIfNeeded()
            .then(function (saved) {
                overlay(true, working);
                return generate(saved === true);
            })
            .then(function (data) {
                lastDesignId = id;
                let message = resultMessage(data);
                if (data.reused) message = attr(btn, 'data-unchanged', 'Sin cambios en el diseño: vistas ya generadas') + ' · ' + message;
                notify(message);
                const views = Array.isArray(data.views) ? data.views : [];
                if (views.length && placedPieces(data) > 0) {
                    viewOpen = false;
                    selectedFaceId = 0;
                    if (!data.reused) viewCache.clear();
                    showFaces(true, null);
                    openList(views);
                }
            })
            .catch(function (err) {
                notify((err && err.message) || failed);
            })
            .then(function () {
                overlay(false);
                busy = false;
                btn.disabled = false;
            });
    });
}

if (document.readyState === 'loading') {
    document.addEventListener('DOMContentLoaded', mount);
} else {
    mount();
}
