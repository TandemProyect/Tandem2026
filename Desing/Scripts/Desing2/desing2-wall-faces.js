/**
 * Caras de muro (interior / exterior). Cada eje tiene dos caras.
 * En L la cara cubre esquina izq. + tramo + esquina der. (extiende el exterior, recorta el interior).
 * IDs horarios: primero exteriores (arranca en la inferior), luego interiores.
 */
export const DESIGN_FACE_COLORS = {
    horizontalExterior: 0xff0000,
    horizontalInterior: 0x00c853,
    verticalExterior: 0xff00ff,
    verticalInterior: 0x2962ff,
};

function hypot(dx, dz) {
    return Math.sqrt(dx * dx + dz * dz);
}

function nearPt(a, b, eps) {
    return !!(a && b && hypot(a.x - b.x, a.z - b.z) <= eps);
}

function dirOf(axis) {
    if (!axis || !axis.p1 || !axis.p2) return null;
    const dx = axis.p2.x - axis.p1.x;
    const dz = axis.p2.z - axis.p1.z;
    const len = hypot(dx, dz);
    if (!(len > 1e-6)) return null;
    return { ux: dx / len, uz: dz / len, lenMm: len };
}

function midOf(axis) {
    return {
        x: (axis.p1.x + axis.p2.x) * 0.5,
        z: (axis.p1.z + axis.p2.z) * 0.5,
    };
}

function thicknessMmOf(axis) {
    const t = Number(axis && axis.thicknessMm);
    return t > 1 ? t : 300;
}

function faceColor(orientation, side) {
    if (orientation === 'vertical') {
        return side === 'interior'
            ? DESIGN_FACE_COLORS.verticalInterior
            : DESIGN_FACE_COLORS.verticalExterior;
    }
    return side === 'interior'
        ? DESIGN_FACE_COLORS.horizontalInterior
        : DESIGN_FACE_COLORS.horizontalExterior;
}

function pointOnSegment(pt, a, b, eps) {
    const dx = b.x - a.x;
    const dz = b.z - a.z;
    const len = hypot(dx, dz);
    if (!(len > 1e-9)) return nearPt(pt, a, eps);
    const t = ((pt.x - a.x) * dx + (pt.z - a.z) * dz) / (len * len);
    if (t < -1e-6 || t > 1 + 1e-6) return false;
    const px = a.x + dx * t;
    const pz = a.z + dz * t;
    return hypot(pt.x - px, pt.z - pz) <= eps;
}

function joinedHalfMm(self, endPt, alongUx, alongUz, axes, eps) {
    let best = 0;
    for (let i = 0; i < axes.length; i++) {
        const other = axes[i];
        if (!other || other === self) continue;
        if (self && other.id != null && self.id != null && String(other.id) === String(self.id)) continue;
        const dir = dirOf(other);
        if (!dir) continue;
        if (Math.abs(dir.ux * alongUx + dir.uz * alongUz) > 0.35) continue;
        const touches =
            nearPt(endPt, other.p1, eps) ||
            nearPt(endPt, other.p2, eps) ||
            pointOnSegment(endPt, other.p1, other.p2, eps);
        if (!touches) continue;
        const half = thicknessMmOf(other) * 0.5;
        if (half > best) best = half;
    }
    return best;
}

function parentOf(parent, i) {
    let x = i;
    while (parent[x] !== x) {
        parent[x] = parent[parent[x]];
        x = parent[x];
    }
    return x;
}

function connectedComponents(axes, eps) {
    const n = axes.length;
    const parent = [];
    for (let i = 0; i < n; i++) parent[i] = i;
    function unite(a, b) {
        const pa = parentOf(parent, a);
        const pb = parentOf(parent, b);
        if (pa !== pb) parent[pa] = pb;
    }
    for (let i = 0; i < n; i++) {
        for (let j = i + 1; j < n; j++) {
            const a = axes[i];
            const b = axes[j];
            if (
                nearPt(a.p1, b.p1, eps) ||
                nearPt(a.p1, b.p2, eps) ||
                nearPt(a.p2, b.p1, eps) ||
                nearPt(a.p2, b.p2, eps)
            ) {
                unite(i, j);
            }
        }
    }
    const groups = new Map();
    for (let i = 0; i < n; i++) {
        const p = parentOf(parent, i);
        const list = groups.get(p) || [];
        list.push(axes[i]);
        groups.set(p, list);
    }
    return Array.from(groups.values());
}

function componentCentroid(axes) {
    let sx = 0;
    let sz = 0;
    let n = 0;
    for (let i = 0; i < axes.length; i++) {
        const m = midOf(axes[i]);
        sx += m.x;
        sz += m.z;
        n += 1;
    }
    if (!(n > 0)) return { x: 0, z: 0 };
    return { x: sx / n, z: sz / n };
}

function orderClockwiseFromBottom(axes, centroid) {
    const ordered = axes.slice().sort(function (a, b) {
        const ma = midOf(a);
        const mb = midOf(b);
        const aa = Math.atan2(ma.z - centroid.z, ma.x - centroid.x);
        const ab = Math.atan2(mb.z - centroid.z, mb.x - centroid.x);
        return ab - aa;
    });
    if (ordered.length < 2) return ordered;
    let best = 0;
    for (let i = 1; i < ordered.length; i++) {
        const m = midOf(ordered[i]);
        const bm = midOf(ordered[best]);
        if (m.z < bm.z - 1e-6 || (Math.abs(m.z - bm.z) <= 1e-6 && m.x < bm.x)) best = i;
    }
    return ordered.slice(best).concat(ordered.slice(0, best));
}

function buildOneFace(axis, side, id, axes, centroid, eps) {
    const dir = dirOf(axis);
    if (!dir) return null;
    const nLeft = { nx: -dir.uz, nz: dir.ux };
    const mid = midOf(axis);
    const toC = { x: centroid.x - mid.x, z: centroid.z - mid.z };
    const towardCenter = nLeft.nx * toC.x + nLeft.nz * toC.z;
    const leftIsInterior = towardCenter > 1e-6;
    const extSign = leftIsInterior ? -1 : 1;
    const sideSign = side === 'interior' ? -extSign : extSign;
    const halfT = thicknessMmOf(axis) * 0.5;
    const ox = nLeft.nx * halfT * sideSign;
    const oz = nLeft.nz * halfT * sideSign;
    const half1 = joinedHalfMm(axis, axis.p1, dir.ux, dir.uz, axes, eps);
    const half2 = joinedHalfMm(axis, axis.p2, -dir.ux, -dir.uz, axes, eps);
    const along = side === 'interior' ? -1 : 1;
    let p1x = axis.p1.x + ox - dir.ux * half1 * along;
    let p1z = axis.p1.z + oz - dir.uz * half1 * along;
    let p2x = axis.p2.x + ox + dir.ux * half2 * along;
    let p2z = axis.p2.z + oz + dir.uz * half2 * along;
    const len = hypot(p2x - p1x, p2z - p1z);
    if (side === 'interior' && len < 20) {
        p1x = axis.p1.x + ox;
        p1z = axis.p1.z + oz;
        p2x = axis.p2.x + ox;
        p2z = axis.p2.z + oz;
    }
    const orientation = Math.abs(dir.ux) >= Math.abs(dir.uz) ? 'horizontal' : 'vertical';
    const wallId = axis.wallId != null && axis.wallId !== '' ? axis.wallId : axis.id;
    return {
        id: id,
        idText: 'F-' + id,
        wallId: wallId,
        axisId: axis.id,
        side: side,
        orientation: orientation,
        color: faceColor(orientation, side),
        p1: { x: p1x, z: p1z },
        p2: { x: p2x, z: p2z },
        lengthMm: hypot(p2x - p1x, p2z - p1z),
    };
}

/**
 * @param {Array<{ id:*, wallId?:*, p1:{x:number,z:number}, p2:{x:number,z:number}, thicknessMm?:number }>} axes
 * @param {{ epsMm?: number }} [options]
 * @returns {Array<object>}
 */
export function buildDesignWallFaces(axes, options) {
    const eps = options && Number(options.epsMm) > 0 ? Number(options.epsMm) : 50;
    const clean = [];
    const src = Array.isArray(axes) ? axes : [];
    for (let i = 0; i < src.length; i++) {
        if (dirOf(src[i])) clean.push(src[i]);
    }
    const faces = [];
    let nextId = 1;
    const groups = connectedComponents(clean, eps);
    for (let g = 0; g < groups.length; g++) {
        const group = groups[g];
        const centroid = componentCentroid(group);
        const ordered = orderClockwiseFromBottom(group, centroid);
        for (let i = 0; i < ordered.length; i++) {
            const face = buildOneFace(ordered[i], 'exterior', nextId, group, centroid, eps);
            if (face) {
                faces.push(face);
                nextId += 1;
            }
        }
        for (let i = 0; i < ordered.length; i++) {
            const face = buildOneFace(ordered[i], 'interior', nextId, group, centroid, eps);
            if (face) {
                faces.push(face);
                nextId += 1;
            }
        }
    }
    return faces;
}

export function findDesignWallFaceById(faces, raw) {
    if (raw == null) return null;
    const m = String(raw).trim().toUpperCase().match(/^I?\s*D?\s*F?\s*(\d+)\s*$/);
    const n = m ? Number(m[1]) : Number(String(raw).replace(/^[^\d]+/, ''));
    if (!(n > 0) || !Array.isArray(faces)) return null;
    for (let i = 0; i < faces.length; i++) {
        if (faces[i] && faces[i].id === n) return faces[i];
    }
    return null;
}
