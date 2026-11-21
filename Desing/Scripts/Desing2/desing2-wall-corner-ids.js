/**
 * IDs correlativos de muro (W-n) y esquina (S-n) en una sola serie, horario desde abajo.
 */

function hypot(dx, dz) {
    return Math.sqrt(dx * dx + dz * dz);
}

function centroidOf(items) {
    let sx = 0;
    let sz = 0;
    let n = 0;
    for (let i = 0; i < items.length; i++) {
        const it = items[i];
        if (!it) continue;
        sx += Number(it.x) || 0;
        sz += Number(it.z) || 0;
        n += 1;
    }
    if (!(n > 0)) return { x: 0, z: 0 };
    return { x: sx / n, z: sz / n };
}

export function orderClockwiseFromBottom(items, centroid) {
    const c = centroid || centroidOf(items);
    const ordered = (items || []).slice().sort(function (a, b) {
        const aa = Math.atan2(a.z - c.z, a.x - c.x);
        const ab = Math.atan2(b.z - c.z, b.x - c.x);
        return ab - aa;
    });
    if (ordered.length < 2) return ordered;
    let best = 0;
    for (let i = 1; i < ordered.length; i++) {
        const it = ordered[i];
        const bm = ordered[best];
        if (it.z < bm.z - 1e-6 || (Math.abs(it.z - bm.z) <= 1e-6 && it.x < bm.x)) best = i;
    }
    return ordered.slice(best).concat(ordered.slice(0, best));
}

/**
 * @param {Array<{ id:*, x:number, z:number }>} walls
 * @param {Array<{ id:*, x:number, z:number }>} corners
 * @returns {{ wallById: Map<string, number>, cornerById: Map<string, number>, list: Array<{ type:string, id:string, n:number, text:string }> }}
 */
export function assignCorrelativeWallCornerIds(walls, corners) {
    const items = [];
    const srcW = Array.isArray(walls) ? walls : [];
    const srcC = Array.isArray(corners) ? corners : [];
    for (let i = 0; i < srcW.length; i++) {
        const w = srcW[i];
        if (!w || w.id == null) continue;
        items.push({ type: 'wall', id: String(w.id), x: Number(w.x), z: Number(w.z) });
    }
    for (let i = 0; i < srcC.length; i++) {
        const c = srcC[i];
        if (!c || c.id == null) continue;
        items.push({ type: 'corner', id: String(c.id), x: Number(c.x), z: Number(c.z) });
    }
    const ordered = orderClockwiseFromBottom(items, centroidOf(items));
    const wallById = new Map();
    const cornerById = new Map();
    const list = [];
    let n = 1;
    for (let i = 0; i < ordered.length; i++) {
        const it = ordered[i];
        const prefix = it.type === 'corner' ? 'S' : 'W';
        const text = prefix + '-' + n;
        if (it.type === 'corner') cornerById.set(it.id, n);
        else wallById.set(it.id, n);
        list.push({ type: it.type, id: it.id, n: n, text: text, x: it.x, z: it.z });
        n += 1;
    }
    return { wallById: wallById, cornerById: cornerById, list: list };
}

export function wallCornerIdText(type, n) {
    const prefix = type === 'corner' ? 'S' : 'W';
    return prefix + '-' + n;
}

void hypot;
