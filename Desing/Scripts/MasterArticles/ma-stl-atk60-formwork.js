/**
 * AT-60 — huellas de encofrado (forma blanca) en planta Desing_2.
 *
 * Reglas de negocio (documentadas en IA/docs/ENCOFRADO-AT60-BASE.md):
 * Implementación visor (handover agentes): IA/docs/ENCOFRADO-AT60-IMPLEMENTACION.md
 * - Esquina fija: 0,30 m (300 mm escena).
 * - Paneles rectos: múltiplos de 0,15 m desde 0,30.
 * - E (espesor muro) es cualquier valor; taco madera = panel − necesidad.
 * - Cada sistema de encofrado tendrá su propio módulo (IEncofradoSystem).
 */

/** Esquina AT-60 en planta — mm escena (= mm físicos). */
export const MA_ATK60_CORNER_PANEL_MM = 300;

/** Paso módulo AT-60 — mm escena. */
export const MA_ATK60_MODULE_STEP_MM = 150;

/** Panel mínimo catálogo — mm escena. */
export const MA_ATK60_MIN_PANEL_MM = 300;

/** Máximo panel generado por catálogo (evita bucles infinitos). */
export const MA_ATK60_MAX_PANEL_MM = 15000;

/**
 * @param {number} needMm necesidad geométrica (E + esquina, 2×E, etc.)
 * @returns {{ panelMm: number, woodTacoMm: number, needMm: number }}
 */
export function maAtk60SelectPanelMm(needMm) {
    const need = Math.max(0, Number(needMm) || 0);
    let panel = MA_ATK60_MIN_PANEL_MM;
    while (panel < need - 1e-6 && panel < MA_ATK60_MAX_PANEL_MM) {
        panel += MA_ATK60_MODULE_STEP_MM;
    }
    if (panel > MA_ATK60_MAX_PANEL_MM) {
        panel = MA_ATK60_MAX_PANEL_MM;
    }
    return {
        needMm: need,
        panelMm: panel,
        woodTacoMm: Math.max(0, panel - need),
    };
}

/**
 * @param {{ x: number, z: number }} a
 * @param {{ x: number, z: number }} b
 */
export function maAtk60UnitXz(a, b) {
    const dx = b.x - a.x;
    const dz = b.z - a.z;
    const len = Math.hypot(dx, dz);
    if (!(len > 1e-9)) return null;
    return { x: dx / len, z: dz / len, len };
}

/**
 * @param {{ x: number, z: number }} u unit
 * @param {number} sign
 */
export function maAtk60NormalXz(u, sign) {
    const s = sign >= 0 ? 1 : -1;
    return { x: -u.z * s, z: u.x * s };
}

/**
 * Punto mundo XZ desde vértice + combinación lineal u/v.
 */
export function maAtk60PointXz(vertex, u, v, alongU, alongV, y) {
    return {
        x: vertex.x + u.x * alongU + v.x * alongV,
        y: y != null ? y : vertex.y,
        z: vertex.z + u.z * alongU + v.z * alongV,
    };
}

/**
 * Esquina L AT-60 — 6 vértices (forma blanca), CCW en planta.
 *
 * @param {{ x: number, y: number, z: number }} vertex nudo
 * @param {{ x: number, z: number }} d1 dirección hacia el cuerpo eje 1
 * @param {{ x: number, z: number }} d2 dirección hacia el cuerpo eje 2
 * @param {number} e1Mm espesor muro eje 1 (completo)
 * @param {number} e2Mm espesor muro eje 2 (completo)
 * @returns {{ points: {x:number,y:number,z:number}[], meta: object }|null}
 */
export function maAtk60BuildLFootprintMm(vertex, d1, d2, e1Mm, e2Mm) {
    if (!vertex || !d1 || !d2) return null;
    const cross = d1.x * d2.z - d1.z * d2.x;
    if (Math.abs(cross) < 1e-9) return null;

    const leg1 = maAtk60SelectPanelMm(Math.max(0, e1Mm) + MA_ATK60_CORNER_PANEL_MM);
    const leg2 = maAtk60SelectPanelMm(Math.max(0, e2Mm) + MA_ATK60_CORNER_PANEL_MM);

    const n1 = maAtk60NormalXz(d1, cross > 0 ? -1 : 1);
    const n2 = maAtk60NormalXz(d2, cross > 0 ? 1 : -1);

    const exteriorP1 = {
        x: vertex.x + n1.x * e1Mm * 0.5,
        z: vertex.z + n1.z * e1Mm * 0.5,
    };
    const exteriorP2 = {
        x: vertex.x + n2.x * e2Mm * 0.5,
        z: vertex.z + n2.z * e2Mm * 0.5,
    };

    const det = d1.x * d2.z - d1.z * d2.x;
    if (Math.abs(det) < 1e-9) return null;
    const dx = exteriorP2.x - exteriorP1.x;
    const dz = exteriorP2.z - exteriorP1.z;
    const t = (dx * d2.z - dz * d2.x) / det;
    const outer = {
        x: exteriorP1.x + d1.x * t,
        z: exteriorP1.z + d1.z * t,
    };

    const q1Outer = { x: outer.x + d1.x * leg1.panelMm, z: outer.z + d1.z * leg1.panelMm };
    const q1Inner = {
        x: q1Outer.x - n1.x * e1Mm,
        z: q1Outer.z - n1.z * e1Mm,
    };
    const innerCorner = {
        x: outer.x - n1.x * e1Mm - n2.x * e2Mm,
        z: outer.z - n1.z * e1Mm - n2.z * e2Mm,
    };
    const q2Inner = {
        x: outer.x + d2.x * leg2.panelMm - n2.x * e2Mm,
        z: outer.z + d2.z * leg2.panelMm - n2.z * e2Mm,
    };
    const q2Outer = { x: outer.x + d2.x * leg2.panelMm, z: outer.z + d2.z * leg2.panelMm };

    const y = vertex.y;
    const points = [
        { x: outer.x, y: y, z: outer.z },
        { x: q1Outer.x, y: y, z: q1Outer.z },
        { x: q1Inner.x, y: y, z: q1Inner.z },
        { x: innerCorner.x, y: y, z: innerCorner.z },
        { x: q2Inner.x, y: y, z: q2Inner.z },
        { x: q2Outer.x, y: y, z: q2Outer.z },
    ];

    return {
        points: points,
        meta: {
            kind: 'cornerL',
            system: 'Atk-60',
            e1Mm: e1Mm,
            e2Mm: e2Mm,
            leg1PanelMm: leg1.panelMm,
            leg2PanelMm: leg2.panelMm,
            leg1WoodTacoMm: leg1.woodTacoMm,
            leg2WoodTacoMm: leg2.woodTacoMm,
        },
    };
}

/**
 * Unión T AT-60 — huella 8 vértices P-1…P-8 (boceto plano → extruir).
 *
 * Recorrido CCW en planta (u = atravesado, v = rama):
 *   P-1 → P-8 → P-7 → P-6 → P-5 → P-4 → P-3 → P-2 → P-1
 *
 * Caso sencillo (simétrico): hombro a v = E_atravesado; cotas panel AT-60 en u/v.
 */
export function maAtk60BuildTFootprintMm(
    vertex,
    throughInto,
    branchInto,
    eThroughMm,
    eBranchMm,
    throughAxisDir
) {
    if (!vertex || !branchInto) return null;
    const axisSrc = throughAxisDir || throughInto;
    if (!axisSrc) return null;
    const uUnit = maAtk60UnitXz({ x: 0, z: 0 }, { x: axisSrc.x, z: axisSrc.z });
    const vUnit = maAtk60UnitXz({ x: 0, z: 0 }, { x: branchInto.x, z: branchInto.z });
    if (!uUnit || !vUnit) return null;
    const dot = uUnit.x * vUnit.x + uUnit.z * vUnit.z;
    if (Math.abs(dot) > 0.15) return null;

    const eThrough = Math.max(0, eThroughMm);
    const eBranch = Math.max(0, eBranchMm);
    const throughPanel = maAtk60SelectPanelMm(2 * eThrough);
    const stemPanel = maAtk60SelectPanelMm(eThrough + MA_ATK60_CORNER_PANEL_MM);
    const innerSpan = maAtk60SelectPanelMm(2 * MA_ATK60_CORNER_PANEL_MM + eBranch);
    const branchPanel = maAtk60SelectPanelMm(eBranch);
    const trims = maAtk60GetTJunctionTrimMm(eThroughMm, eBranchMm);

    const u = { x: uUnit.x, z: uUnit.z };
    const v = { x: vUnit.x, z: vUnit.z };

    /** Cara atravesado hacia la rama (near) / opuesta (far) — no el eje. */
    const nProbe = maAtk60NormalXz(u, 1);
    const nearSign = nProbe.x * v.x + nProbe.z * v.z >= 0 ? 1 : -1;

    const throughHalf = trims.throughHalfMm;
    const stemLen = trims.branchStemMm;
    const branchHalf = eBranch * 0.5;
    const nearFace = eThrough * 0.5;
    const farFace = -eThrough * 0.5;

    const y = vertex.y;
    /** Punto local en la base T: u = atravesado, v = rama. */
    const pLocal = function (au, av) {
        return {
            x: vertex.x + u.x * au + v.x * av,
            y: y,
            z: vertex.z + u.z * au + v.z * av,
        };
    };

    // Orden boceto: P-1 → P-8 → P-7 → P-6 → P-5 → P-4 → P-3 → P-2
    const points = [
        pLocal(-branchHalf, stemLen), // P-1: cara izquierda rama, conecta con recto rama
        pLocal(-branchHalf, nearFace), // P-8: intersección cara rama + cara near atravesado
        pLocal(-throughHalf, nearFace), // P-7: extremo superior recto atravesado izquierdo
        pLocal(-throughHalf, farFace), // P-6: extremo inferior recto atravesado izquierdo
        pLocal(throughHalf, farFace), // P-5: extremo inferior recto atravesado derecho
        pLocal(throughHalf, nearFace), // P-4: extremo superior recto atravesado derecho
        pLocal(branchHalf, nearFace), // P-3: intersección cara rama + cara near atravesado
        pLocal(branchHalf, stemLen), // P-2: cara derecha rama, conecta con recto rama
    ];

    return {
        points: points,
        meta: {
            kind: 'cornerT',
            system: 'Atk-60',
            eThroughMm: eThroughMm,
            eBranchMm: eBranchMm,
            throughPanelMm: throughPanel.panelMm,
            stemPanelMm: stemPanel.panelMm,
            innerSpanPanelMm: innerSpan.panelMm,
            branchPanelMm: branchPanel.panelMm,
            throughWoodTacoMm: throughPanel.woodTacoMm,
            stemWoodTacoMm: stemPanel.woodTacoMm,
            innerWoodTacoMm: innerSpan.woodTacoMm,
            branchWoodTacoMm: branchPanel.woodTacoMm,
            vShoulderMm: nearFace,
            throughHalfMm: throughHalf,
            stemLenMm: stemLen,
            branchHalfMm: branchHalf,
            innerHalfMm: innerSpan.panelMm * 0.5,
            nearSign: nearSign,
            pointLabels: ['P-1', 'P-8', 'P-7', 'P-6', 'P-5', 'P-4', 'P-3', 'P-2'],
        },
    };
}

/**
 * Recortes de tramo recto en unión T (mm escena) — restar en cada pata del atravesado y en la rama.
 * @returns {{ throughHalfMm: number, branchStemMm: number }}
 */
export function maAtk60GetTJunctionTrimMm(eThroughMm, eBranchMm) {
    const throughPanel = maAtk60SelectPanelMm(2 * Math.max(0, eThroughMm));
    const stemPanel = maAtk60SelectPanelMm(Math.max(0, eThroughMm) + MA_ATK60_CORNER_PANEL_MM);
    return {
        throughHalfMm: throughPanel.panelMm * 0.5,
        branchStemMm: stemPanel.panelMm,
        throughPanelMm: throughPanel.panelMm,
        stemPanelMm: stemPanel.panelMm,
    };
}

/**
 * Recortes de tramo recto en esquina L (mm escena) — panel cateto por eje.
 * @returns {{ leg1Mm: number, leg2Mm: number }}
 */
export function maAtk60GetLJunctionTrimMm(e1Mm, e2Mm) {
    const leg1 = maAtk60SelectPanelMm(Math.max(0, e1Mm) + MA_ATK60_CORNER_PANEL_MM);
    const leg2 = maAtk60SelectPanelMm(Math.max(0, e2Mm) + MA_ATK60_CORNER_PANEL_MM);
    return { leg1Mm: leg1.panelMm, leg2Mm: leg2.panelMm };
}

/**
 * Recorte de tramo recto en esquina L medido sobre cada eje desde el vértice del eje.
 * Incluye el desplazamiento eje->esquina exterior para que el recto conecte exactamente
 * con el extremo de la pieza L en planta.
 *
 * @param {{x:number,z:number}} d1 dirección del eje 1 hacia el cuerpo
 * @param {{x:number,z:number}} d2 dirección del eje 2 hacia el cuerpo
 * @param {number} e1Mm espesor eje 1 (completo)
 * @param {number} e2Mm espesor eje 2 (completo)
 * @returns {{ leg1AxisTrimMm:number, leg2AxisTrimMm:number, leg1PanelMm:number, leg2PanelMm:number }}
 */
export function maAtk60GetLJunctionAxisTrimMm(d1, d2, e1Mm, e2Mm) {
    const e1 = Math.max(0, Number(e1Mm) || 0);
    const e2 = Math.max(0, Number(e2Mm) || 0);
    const leg1 = maAtk60SelectPanelMm(e1 + MA_ATK60_CORNER_PANEL_MM);
    const leg2 = maAtk60SelectPanelMm(e2 + MA_ATK60_CORNER_PANEL_MM);
    if (!d1 || !d2) {
        return {
            leg1AxisTrimMm: leg1.panelMm,
            leg2AxisTrimMm: leg2.panelMm,
            leg1PanelMm: leg1.panelMm,
            leg2PanelMm: leg2.panelMm,
        };
    }

    const cross = d1.x * d2.z - d1.z * d2.x;
    if (Math.abs(cross) < 1e-9) {
        return {
            leg1AxisTrimMm: leg1.panelMm,
            leg2AxisTrimMm: leg2.panelMm,
            leg1PanelMm: leg1.panelMm,
            leg2PanelMm: leg2.panelMm,
        };
    }

    const n1 = maAtk60NormalXz(d1, cross > 0 ? -1 : 1);
    const n2 = maAtk60NormalXz(d2, cross > 0 ? 1 : -1);

    // Intersección de caras exteriores de ambas patas, en sistema local con vértice en (0,0).
    const ex1 = n1.x * e1 * 0.5;
    const ez1 = n1.z * e1 * 0.5;
    const ex2 = n2.x * e2 * 0.5;
    const ez2 = n2.z * e2 * 0.5;
    const dx = ex2 - ex1;
    const dz = ez2 - ez1;
    const det = d1.x * d2.z - d1.z * d2.x;

    if (Math.abs(det) < 1e-9) {
        return {
            leg1AxisTrimMm: leg1.panelMm,
            leg2AxisTrimMm: leg2.panelMm,
            leg1PanelMm: leg1.panelMm,
            leg2PanelMm: leg2.panelMm,
        };
    }

    const t1 = (dx * d2.z - dz * d2.x) / det;
    const t2 = (dx * d1.z - dz * d1.x) / det;

    return {
        leg1AxisTrimMm: Math.max(0, t1 + leg1.panelMm),
        leg2AxisTrimMm: Math.max(0, t2 + leg2.panelMm),
        leg1PanelMm: leg1.panelMm,
        leg2PanelMm: leg2.panelMm,
    };
}

/** Mínimo remate (taco madera) visible en acotación — mm escena. */
export const MA_ATK60_DIM_MIN_REMATE_MM = 5;

/** Desplazamiento cota cateto (panel) respecto al borde — mm escena. */
export const MA_ATK60_DIM_PANEL_OFFSET_MM = 280;

/** Desplazamiento extra cota remate respecto al cateto — mm escena. */
export const MA_ATK60_DIM_REMATE_STACK_MM = 320;

/**
 * Normal exterior (lado derecho) de un segmento A→B en planta XZ (polígono CCW).
 */
export function maAtk60SegOutwardNormalXz(pA, pB) {
    const dx = pB.x - pA.x;
    const dz = pB.z - pA.z;
    const len = Math.hypot(dx, dz);
    if (!(len > 1e-9)) return null;
    return { nx: dz / len, nz: -dx / len, ux: dx / len, uz: dz / len, len: len };
}

/**
 * Cota alineada CAD compatible con `maStlWallDimRebuildFromPlacements`.
 */
export function maAtk60BuildAlignedDimPlacement(pA, pB, labelMm, dimOutMm, floorY, kind) {
    if (!pA || !pB || !(labelMm >= 0) || !(dimOutMm > 0)) return null;
    const n = maAtk60SegOutwardNormalXz(pA, pB);
    if (!n) return null;
    const extA = { x: pA.x + n.nx * dimOutMm, z: pA.z + n.nz * dimOutMm };
    const extB = { x: pB.x + n.nx * dimOutMm, z: pB.z + n.nz * dimOutMm };
    return {
        floorY: floorY,
        pA: { x: pA.x, z: pA.z },
        pB: { x: pB.x, z: pB.z },
        extA_end: extA,
        extB_end: extB,
        labelMid: { x: (extA.x + extB.x) * 0.5, z: (extA.z + extB.z) * 0.5 },
        labelMm: labelMm,
        kind: kind || 'atk60-panel',
    };
}

function maAtk60PushPanelAndRemateDims(out, pA, pB, panelMm, remateMm, floorY, panelOffsetMm) {
    const panelPl = maAtk60BuildAlignedDimPlacement(
        pA,
        pB,
        panelMm,
        panelOffsetMm,
        floorY,
        'atk60-panel'
    );
    if (panelPl) out.push(panelPl);
    if (!(remateMm >= MA_ATK60_DIM_MIN_REMATE_MM)) return;
    const n = maAtk60SegOutwardNormalXz(pA, pB);
    if (!n || !(n.len > 1e-9)) return;
    const remateA = {
        x: pB.x - n.ux * remateMm,
        z: pB.z - n.uz * remateMm,
    };
    const rematePl = maAtk60BuildAlignedDimPlacement(
        remateA,
        pB,
        remateMm,
        panelOffsetMm + MA_ATK60_DIM_REMATE_STACK_MM,
        floorY,
        'atk60-remate'
    );
    if (rematePl) out.push(rematePl);
}

/**
 * Cotas AT-60 esquina L: catetos (panel por pata) + remate/taco si existe.
 * @param {{ points: object[], meta: object }} footprint
 * @param {number} floorY
 * @param {number} [panelOffsetMm]
 */
export function maAtk60BuildLFootprintDimPlacements(footprint, floorY, panelOffsetMm) {
    const pts = footprint && footprint.points;
    const meta = footprint && footprint.meta;
    if (!pts || pts.length < 6 || !meta || meta.kind !== 'cornerL') return [];
    const off = panelOffsetMm != null ? panelOffsetMm : MA_ATK60_DIM_PANEL_OFFSET_MM;
    const out = [];
    maAtk60PushPanelAndRemateDims(
        out,
        pts[0],
        pts[1],
        meta.leg1PanelMm,
        meta.leg1WoodTacoMm,
        floorY,
        off
    );
    maAtk60PushPanelAndRemateDims(
        out,
        pts[0],
        pts[5],
        meta.leg2PanelMm,
        meta.leg2WoodTacoMm,
        floorY,
        off
    );
    return out;
}

/**
 * Cotas AT-60 unión T: atravesado, luz interior, vástago y rama + remates.
 */
export function maAtk60BuildTFootprintDimPlacements(footprint, floorY, panelOffsetMm) {
    const pts = footprint && footprint.points;
    const meta = footprint && footprint.meta;
    if (!pts || pts.length < 8 || !meta || meta.kind !== 'cornerT') return [];
    const off = panelOffsetMm != null ? panelOffsetMm : MA_ATK60_DIM_PANEL_OFFSET_MM;
    const out = [];
    // P-6 → P-5: ancho atravesado
    maAtk60PushPanelAndRemateDims(
        out,
        pts[3],
        pts[4],
        meta.throughPanelMm,
        meta.throughWoodTacoMm,
        floorY,
        off
    );
    // P-8 → P-7: hombro izquierdo (luz interior en u)
    maAtk60PushPanelAndRemateDims(
        out,
        pts[1],
        pts[2],
        meta.innerSpanPanelMm,
        meta.innerWoodTacoMm,
        floorY,
        off + 80
    );
    // P-8 → P-1: vástago
    maAtk60PushPanelAndRemateDims(
        out,
        pts[1],
        pts[0],
        meta.stemPanelMm,
        meta.stemWoodTacoMm,
        floorY,
        off + 160
    );
    const stemTip = pts[0];
    const stemBase = pts[1];
    const nuDx = stemTip.x - stemBase.x;
    const nuDz = stemTip.z - stemBase.z;
    const nuLen = Math.hypot(nuDx, nuDz);
    if (nuLen > 1e-9) {
        const branchEnd = {
            x: stemTip.x + (nuDx / nuLen) * meta.branchPanelMm,
            y: stemTip.y,
            z: stemTip.z + (nuDz / nuLen) * meta.branchPanelMm,
        };
        maAtk60PushPanelAndRemateDims(
            out,
            stemTip,
            branchEnd,
            meta.branchPanelMm,
            meta.branchWoodTacoMm,
            floorY,
            off + 240
        );
    }
    return out;
}

/**
 * Cotas encofrado AT-60 para una huella L o T.
 */
export function maAtk60BuildFootprintDimPlacements(footprint, floorY, panelOffsetMm) {
    if (!footprint || !footprint.meta) return [];
    const y = floorY != null ? floorY : 0;
    if (footprint.meta.kind === 'cornerL') {
        return maAtk60BuildLFootprintDimPlacements(footprint, y, panelOffsetMm);
    }
    if (footprint.meta.kind === 'cornerT') {
        return maAtk60BuildTFootprintDimPlacements(footprint, y, panelOffsetMm);
    }
    return [];
}

/**
 * Clasificación pieza encofrado por número de vértices de la huella.
 * @param {number} vertexCount
 */
export function maAtk60ClassifyFootprintKind(vertexCount) {
    if (vertexCount >= 8) return 'cornerT';
    if (vertexCount >= 6) return 'cornerL';
    return 'wall';
}
