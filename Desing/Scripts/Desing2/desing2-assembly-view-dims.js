/**
 * Plano de montaje: cotas de la cara vista de frente (mm; X a lo largo desde P1, Y altura desde la base).
 * Arriba: cadena de paneles de la fila de abajo y, encima, el largo total de la cara.
 * Izquierda: alturas de los paneles (y total si hay más de una fila). Abajo: línea de tierra.
 * Textos como en el plano: metros con coma («0,6m», «2,7m»).
 */
const SNAP_MM = 20;
const ROW_TOL_MM = 60;
const NOMINAL_TOL_MM = 30;
const DIM_HEX = 0x24313d;
const GROUND_HEX = 0x1b1b1b;

export function formatMeters(mm) {
    const n = Math.round(Number(mm) || 0);
    const text = (n / 1000).toFixed(3).replace(/0+$/, '').replace(/\.$/, '');
    return text.replace('.', ',') + 'm';
}

/** Medida a mostrar: la nominal del maestro si encaja; si no, la medida redondeada a 5 mm. */
function shownMm(measured, nominals) {
    for (let i = 0; i < nominals.length; i++) {
        const n = Number(nominals[i]);
        if (n > 0 && Math.abs(n - measured) <= NOMINAL_TOL_MM) return n;
    }
    return Math.round(measured / 5) * 5;
}

function mergeBreaks(values) {
    const sorted = values.filter(function (v) { return Number.isFinite(v); }).sort(function (a, b) { return a - b; });
    const out = [];
    for (let i = 0; i < sorted.length; i++) {
        if (!out.length || sorted[i] - out[out.length - 1] > SNAP_MM) out.push(sorted[i]);
    }
    return out;
}

function chain(breaks, pieces, lo, hi, nominalsOf) {
    const out = [];
    for (let i = 0; i + 1 < breaks.length; i++) {
        const from = breaks[i];
        const to = breaks[i + 1];
        const measured = to - from;
        let nominals = [];
        for (let k = 0; k < pieces.length; k++) {
            const p = pieces[k];
            if (Math.abs(p[lo] - from) <= SNAP_MM && Math.abs(p[hi] - to) <= SNAP_MM) {
                nominals = nominalsOf(p);
                break;
            }
        }
        out.push({ from: from, to: to, label: formatMeters(shownMm(measured, nominals)) });
    }
    return out;
}

/**
 * @param {Array<{x0:number,x1:number,y0:number,y1:number,nominalW?:number,nominalH?:number,lying?:boolean,filler?:boolean}>} pieces paneles y remates
 * @returns {{ horizontal: Array, total: object, vertical: Array, verticalTotal: object|null }}
 */
export function assemblyDimChains(pieces, lengthMm) {
    const length = Math.max(Number(lengthMm) || 0, 0);
    const result = {
        horizontal: [],
        total: { from: 0, to: length, label: formatMeters(length) },
        vertical: [],
        verticalTotal: null,
    };
    const valid = (Array.isArray(pieces) ? pieces : []).filter(function (p) {
        return p && p.x1 - p.x0 > SNAP_MM && p.y1 - p.y0 > SNAP_MM;
    });
    if (!valid.length) return result;

    // Un panel tumbado mide a lo largo su alto nominal.
    function alongNominals(p) { return p.lying ? [p.nominalH, p.nominalW] : [p.nominalW, p.nominalH]; }
    function upNominals(p) { return p.lying ? [p.nominalW, p.nominalH] : [p.nominalH, p.nominalW]; }

    const bottom = Math.min.apply(null, valid.map(function (p) { return p.y0; }));
    const row = valid.filter(function (p) { return p.y0 <= bottom + ROW_TOL_MM; });
    const xs = mergeBreaks([].concat.apply([], row.map(function (p) { return [p.x0, p.x1]; })));
    result.horizontal = chain(xs, row, 'x0', 'x1', alongNominals);

    const panels = valid.filter(function (p) { return !p.filler; });
    const ys = mergeBreaks([].concat.apply([], panels.map(function (p) { return [p.y0, p.y1]; })));
    result.vertical = chain(ys, panels, 'y0', 'y1', upNominals);
    if (result.vertical.length > 1) {
        const from = ys[0];
        const to = ys[ys.length - 1];
        result.verticalTotal = { from: from, to: to, label: formatMeters(Math.round((to - from) / 5) * 5) };
    }
    return result;
}

function textMesh(THREE, label, sizeMm, bgCss, fgCss) {
    let material;
    let aspect = Math.max(2.2, String(label).length * 0.62);
    const doc = typeof document !== 'undefined' ? document : null;
    if (doc && typeof doc.createElement === 'function') {
        const canvas = doc.createElement('canvas');
        const ctx = canvas.getContext && canvas.getContext('2d');
        if (ctx) {
            const font = 'bold 56px Arial, sans-serif';
            ctx.font = font;
            const w = Math.ceil(ctx.measureText(label).width) + 24;
            canvas.width = w;
            canvas.height = 72;
            ctx.font = font;
            ctx.fillStyle = bgCss || 'rgba(255,255,255,0.88)';
            ctx.fillRect(0, 0, canvas.width, canvas.height);
            ctx.fillStyle = fgCss || '#1f2933';
            ctx.textAlign = 'center';
            ctx.textBaseline = 'middle';
            ctx.fillText(label, canvas.width / 2, canvas.height / 2 + 2);
            const texture = new THREE.CanvasTexture(canvas);
            texture.anisotropy = 4;
            aspect = canvas.width / canvas.height;
            material = new THREE.MeshBasicMaterial({ map: texture, transparent: true, depthTest: false, side: THREE.DoubleSide });
        }
    }
    if (!material) material = new THREE.MeshBasicMaterial({ color: DIM_HEX, transparent: true, depthTest: false, side: THREE.DoubleSide });
    const mesh = new THREE.Mesh(new THREE.PlaneGeometry(sizeMm * aspect, sizeMm), material);
    mesh.renderOrder = 21;
    mesh.userData.assemblyDimLabel = label;
    return mesh;
}

/**
 * Líneas, textos y línea de tierra.
 * @param {{ lengthMm:number, topY:number, leftX:number, rightX:number, zFront:number, zBack:number }} box
 */
export function buildAssemblyDims(THREE, chains, box) {
    const group = new THREE.Group();
    group.name = 'assemblyDims';
    const length = Math.max(Number(box.lengthMm) || 0, 1);
    const t = Math.min(200, Math.max(70, length * 0.03));
    const z = 0;
    const pts = [];
    function seg(x1, y1, x2, y2) { pts.push(x1, y1, z, x2, y2, z); }
    function label(text, x, y, vertical) {
        const mesh = textMesh(THREE, text, t);
        mesh.position.set(x, y, z);
        if (vertical) mesh.rotation.z = Math.PI / 2;
        group.add(mesh);
    }
    function horizontalRow(items, y, objectTop) {
        if (!items.length) return;
        const xs = [items[0].from].concat(items.map(function (s) { return s.to; }));
        seg(xs[0], y, xs[xs.length - 1], y);
        for (let i = 0; i < xs.length; i++) {
            seg(xs[i], objectTop + 0.4 * t, xs[i], y + 0.4 * t);
            seg(xs[i] - 0.35 * t, y - 0.35 * t, xs[i] + 0.35 * t, y + 0.35 * t);
        }
        for (let i = 0; i < items.length; i++) label(items[i].label, (items[i].from + items[i].to) / 2, y + 0.75 * t, false);
    }
    function verticalRow(items, x, objectLeft) {
        if (!items.length) return;
        const ys = [items[0].from].concat(items.map(function (s) { return s.to; }));
        seg(x, ys[0], x, ys[ys.length - 1]);
        for (let i = 0; i < ys.length; i++) {
            seg(objectLeft - 0.4 * t, ys[i], x - 0.4 * t, ys[i]);
            seg(x - 0.35 * t, ys[i] - 0.35 * t, x + 0.35 * t, ys[i] + 0.35 * t);
        }
        for (let i = 0; i < items.length; i++) label(items[i].label, x - 0.75 * t, (items[i].from + items[i].to) / 2, true);
    }

    const topY = Number(box.topY) || 0;
    const leftX = Math.min(0, Number(box.leftX) || 0);
    const rightX = Math.max(length, Number(box.rightX) || 0);
    const chainY = topY + 2.2 * t;
    const totalY = chainY + 2.2 * t;
    horizontalRow(chains.horizontal || [], chainY, topY);
    horizontalRow(chains.total ? [chains.total] : [], chains.horizontal && chains.horizontal.length ? totalY : chainY, topY);
    const vChainX = leftX - 2.2 * t;
    verticalRow(chains.vertical || [], vChainX, leftX);
    if (chains.verticalTotal) verticalRow([chains.verticalTotal], vChainX - 2.2 * t, leftX);

    if (pts.length) {
        const geometry = new THREE.BufferGeometry();
        geometry.setAttribute('position', new THREE.Float32BufferAttribute(pts, 3));
        const lines = new THREE.LineSegments(geometry, new THREE.LineBasicMaterial({ color: DIM_HEX, transparent: true, depthTest: false }));
        lines.renderOrder = 20;
        group.add(lines);
    }

    // Línea de tierra: franja oscura bajo la cara que cubre las piezas en profundidad.
    const zFront = Number.isFinite(box.zFront) ? box.zFront : 300;
    const zBack = Number.isFinite(box.zBack) ? box.zBack : -300;
    const groundFrom = leftX - 6 * t;
    const groundTo = rightX + 6 * t;
    const depth = Math.max(zFront - zBack, 0) + 400;
    const ground = new THREE.Mesh(
        new THREE.BoxGeometry(groundTo - groundFrom, 0.18 * t, depth),
        new THREE.MeshLambertMaterial({ color: GROUND_HEX })
    );
    ground.position.set((groundFrom + groundTo) / 2, -0.09 * t, (zFront + zBack) / 2);
    ground.userData.assemblyGround = true;
    group.add(ground);
    return group;
}

/** Ángulo de texto legible (nunca boca abajo) para una dirección (ux, uy). */
export function readableAngle(ux, uy) {
    let a = Math.atan2(uy, ux);
    if (a > Math.PI / 2 + 1e-9) a -= Math.PI;
    if (a <= -Math.PI / 2 + 1e-9) a += Math.PI;
    return a;
}

/**
 * Cotas alineadas en planta (plano XY, Z hacia quien mira).
 * @param {Array<{ax:number, ay:number, ux:number, uy:number, nx:number, ny:number, offset:number, stops:number[], labels:string[]}>} rows
 *   Cada fila: origen a, dirección u y normal n (unitarias), distancia de la línea de cota y puntos a lo largo de u.
 * @param {number} t tamaño del texto (mm). @param {number} z altura del dibujo.
 */
export function buildPlanDims(THREE, rows, t, z) {
    const group = new THREE.Group();
    group.name = 'assemblyPlanDims';
    const pts = [];
    function seg(x1, y1, x2, y2) { pts.push(x1, y1, z, x2, y2, z); }
    for (let r = 0; r < (rows || []).length; r++) {
        const row = rows[r];
        const stops = row.stops || [];
        if (stops.length < 2) continue;
        const off = row.offset;
        function at(s, d) { return { x: row.ax + row.ux * s + row.nx * d, y: row.ay + row.uy * s + row.ny * d }; }
        const first = at(stops[0], off);
        const last = at(stops[stops.length - 1], off);
        seg(first.x, first.y, last.x, last.y);
        for (let i = 0; i < stops.length; i++) {
            const from = at(stops[i], 0.3 * t);
            const to = at(stops[i], off + 0.4 * t);
            seg(from.x, from.y, to.x, to.y);
            const c = at(stops[i], off);
            const tx = (row.ux + row.nx) * 0.35 * t;
            const ty = (row.uy + row.ny) * 0.35 * t;
            seg(c.x - tx, c.y - ty, c.x + tx, c.y + ty);
        }
        const angle = readableAngle(row.ux, row.uy);
        for (let i = 0; i + 1 < stops.length; i++) {
            const label = (row.labels || [])[i];
            if (!label) continue;
            const mesh = textMesh(THREE, label, t);
            const p = at((stops[i] + stops[i + 1]) / 2, off + 0.75 * t);
            mesh.position.set(p.x, p.y, z);
            mesh.rotation.z = angle;
            group.add(mesh);
        }
    }
    if (pts.length) {
        const geometry = new THREE.BufferGeometry();
        geometry.setAttribute('position', new THREE.Float32BufferAttribute(pts, 3));
        const lines = new THREE.LineSegments(geometry, new THREE.LineBasicMaterial({ color: DIM_HEX, transparent: true, depthTest: false }));
        lines.renderOrder = 20;
        group.add(lines);
    }
    return group;
}

/** Etiqueta F-n en planta, del color de la cara. */
export function planFaceLabel(THREE, text, colorHex, t, x, y, z) {
    const css = '#' + ('000000' + ((Number(colorHex) >>> 0) & 0xffffff).toString(16)).slice(-6);
    const mesh = textMesh(THREE, text, t * 1.2, css, '#ffffff');
    mesh.position.set(x, y, z);
    return mesh;
}
