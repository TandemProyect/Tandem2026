/**
 * Post-procesado único tras editar muros (estirar, mover eje, etc.).
 * Evita duplicar ejes/carás y longitudes erróneas por varias pasadas de
 * refactor / split / weld en el mismo commit.
 */

/** @typedef {{ x: number, y: number, z: number }} MaStlPlanPointMm */

/**
 * Geometría de estirar (vector base→segundo proyectado en el eje del segmento).
 */
export class MaStlWallStretchMath {
    /**
     * @param {MaStlPlanPointMm} p1Mm
     * @param {MaStlPlanPointMm} p2Mm
     * @param {number} dx
     * @param {number} dz
     * @returns {{ movedEndpoint: 'p1'|'p2', fixedPt: MaStlPlanPointMm, dirAnchorPt: MaStlPlanPointMm, axisDeltaMm: number }|null}
     */
    static planFromDisplacementMm(p1Mm, p2Mm, dx, dz) {
        if (!p1Mm || !p2Mm) return null;
        const ax = p2Mm.x - p1Mm.x;
        const az = p2Mm.z - p1Mm.z;
        const axisLen = Math.hypot(ax, az);
        if (axisLen < 1e-9) return null;
        const ux = ax / axisLen;
        const uz = az / axisLen;
        const s = dx * ux + dz * uz;
        if (s >= 0) {
            return {
                movedEndpoint: 'p2',
                fixedPt: p1Mm,
                dirAnchorPt: p2Mm,
                axisDeltaMm: s,
            };
        }
        return {
            movedEndpoint: 'p1',
            fixedPt: p2Mm,
            dirAnchorPt: p1Mm,
            axisDeltaMm: s,
        };
    }

    /**
     * @param {MaStlPlanPointMm} p1Mm
     * @param {MaStlPlanPointMm} p2Mm
     * @param {number} dx
     * @param {number} dz
     * @param {MaStlPlanPointMm} outP1
     * @param {MaStlPlanPointMm} outP2
     * @param {number} minLenMm
     * @returns {boolean}
     */
    static segmentEndpointsByDisplacementMm(p1Mm, p2Mm, dx, dz, outP1, outP2, minLenMm) {
        if (!p1Mm || !p2Mm || !outP1 || !outP2) return false;
        const plan = MaStlWallStretchMath.planFromDisplacementMm(p1Mm, p2Mm, dx, dz);
        if (!plan) return false;
        const ax = p2Mm.x - p1Mm.x;
        const az = p2Mm.z - p1Mm.z;
        const axisLen = Math.hypot(ax, az);
        const ux = ax / axisLen;
        const uz = az / axisLen;
        const newLen =
            plan.movedEndpoint === 'p2' ? axisLen + plan.axisDeltaMm : axisLen - plan.axisDeltaMm;
        if (newLen < minLenMm - 1e-6) return false;
        if (plan.movedEndpoint === 'p2') {
            outP1.x = p1Mm.x;
            outP1.y = p1Mm.y;
            outP1.z = p1Mm.z;
            outP2.x = p1Mm.x + ux * newLen;
            outP2.y = p2Mm.y;
            outP2.z = p1Mm.z + uz * newLen;
        } else {
            outP2.x = p2Mm.x;
            outP2.y = p2Mm.y;
            outP2.z = p2Mm.z;
            outP1.x = p2Mm.x - ux * newLen;
            outP1.y = p1Mm.y;
            outP1.z = p2Mm.z - uz * newLen;
        }
        return true;
    }

    /**
     * Posición del extremo móvil tras estirar (sin snap).
     * @param {MaStlPlanPointMm} p1Mm
     * @param {MaStlPlanPointMm} p2Mm
     * @param {number} dx
     * @param {number} dz
     * @param {MaStlPlanPointMm} out
     * @param {number} minLenMm
     * @returns {'p1'|'p2'|null} extremo movido
     */
    static movedEndpointTargetMm(p1Mm, p2Mm, dx, dz, out, minLenMm) {
        const scratchP1 = { x: 0, y: 0, z: 0 };
        const scratchP2 = { x: 0, y: 0, z: 0 };
        if (
            !MaStlWallStretchMath.segmentEndpointsByDisplacementMm(
                p1Mm,
                p2Mm,
                dx,
                dz,
                scratchP1,
                scratchP2,
                minLenMm
            )
        ) {
            return null;
        }
        const plan = MaStlWallStretchMath.planFromDisplacementMm(p1Mm, p2Mm, dx, dz);
        if (!plan) return null;
        const moved = plan.movedEndpoint === 'p2' ? scratchP2 : scratchP1;
        out.x = moved.x;
        out.y = moved.y;
        out.z = moved.z;
        return plan.movedEndpoint;
    }
}

export class MaStlWallEditFinalize {
    /**
     * @param {object} deps funciones del visor (inyectadas desde master-article-details-stl-viewer.js)
     * @param {function(import('three').Line2):void} deps.refreshAxisFaceOffsetsNoJunction
     * @param {function():void} deps.refactorAllWallJunctionsMm
     * @param {function():boolean} deps.splitAllAxisInteriorCrossingsMm
     * @param {function():void} deps.weldAllUserFloorLineEndpointsMm
     * @param {function():void} deps.reapplyAllUserFloorWallAxisLineStyles
     * @param {function({ force?: boolean, reason?: string }):void} deps.saveWallConnections
     * @param {function(import('three').Line2):import('three').Line2|null} deps.editableAxisLineForDimension
     */
    constructor(deps) {
        this.deps = deps;
    }

    /**
     * Una sola pasada de caras + uniones tras editar ejes.
     * @param {import('three').Line2[]} axisLines
     * @param {{ allowWeld?: boolean, saveReason?: string }} [options]
     */
    finalizeWallAxes(axisLines, options) {
        const opts = options || {};
        const unique = [];
        const seen = new Set();
        for (let i = 0; i < axisLines.length; i++) {
            const axis = this.deps.editableAxisLineForDimension(axisLines[i]);
            if (!axis) continue;
            const ud = axis.userData && axis.userData.maStlUserPlanLine;
            if (!ud || ud.id == null || seen.has(ud.id)) continue;
            seen.add(ud.id);
            unique.push(axis);
        }
        for (let fi = 0; fi < unique.length; fi++) {
            this.deps.refreshAxisFaceOffsetsNoJunction(unique[fi]);
        }
        this.deps.refactorAllWallJunctionsMm();
        this.deps.splitAllAxisInteriorCrossingsMm();
        this.deps.refactorAllWallJunctionsMm();
        if (opts.allowWeld !== false) {
            this.deps.weldAllUserFloorLineEndpointsMm();
        }
        this.deps.refactorAllWallJunctionsMm();
        this.deps.reapplyAllUserFloorWallAxisLineStyles();
        if (opts.saveReason) {
            this.deps.saveWallConnections({ force: true, reason: opts.saveReason });
        }
    }
}
