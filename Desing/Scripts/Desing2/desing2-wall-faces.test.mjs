import { buildDesignWallFaces, findDesignWallFaceById, DESIGN_FACE_COLORS } from './desing2-wall-faces.js';

function assertEqual(name, actual, expected) {
    if (actual !== expected) {
        throw new Error(name + ': esperado ' + expected + ', obtenido ' + actual);
    }
}

function assertClose(name, actual, expected, tol) {
    const t = tol == null ? 1.5 : tol;
    if (!(Math.abs(actual - expected) <= t)) {
        throw new Error(name + ': esperado ~' + expected + ', obtenido ' + actual);
    }
}

const T = 300;
const half = T / 2;

/* Recinto 4 muros + 4 L. Ejes a mitad de espesor. Exterior 0..4000 x 0..3000. */
const rect = [
    { id: 1, wallId: 1, p1: { x: 150, z: 150 }, p2: { x: 3850, z: 150 }, thicknessMm: T },
    { id: 2, wallId: 2, p1: { x: 3850, z: 150 }, p2: { x: 3850, z: 2850 }, thicknessMm: T },
    { id: 3, wallId: 3, p1: { x: 3850, z: 2850 }, p2: { x: 150, z: 2850 }, thicknessMm: T },
    { id: 4, wallId: 4, p1: { x: 150, z: 2850 }, p2: { x: 150, z: 150 }, thicknessMm: T },
];

const faces = buildDesignWallFaces(rect, { epsMm: 50 });
assertEqual('recinto: 8 caras', faces.length, 8);
assertEqual('ids 1..8 únicos', faces.map(function (f) { return f.id; }).join(','), '1,2,3,4,5,6,7,8');

const f1 = faces[0];
assertEqual('1 es exterior', f1.side, 'exterior');
assertEqual('1 horizontal', f1.orientation, 'horizontal');
assertEqual('1 rojo', f1.color, DESIGN_FACE_COLORS.horizontalExterior);
assertClose('1 en z exterior', f1.p1.z, 0);
assertClose('1 largo exterior (eje+esquinas)', f1.lengthMm, 4000);

const interiors = faces.filter(function (f) { return f.side === 'interior'; });
assertEqual('4 interiores', interiors.length, 4);
assertEqual('interior horizontal verde', interiors[0].color, DESIGN_FACE_COLORS.horizontalInterior);
assertClose('interior inferior z', interiors[0].p1.z, 300);
assertClose('interior inferior más corto', interiors[0].lengthMm, 3400);

const verticalExt = faces.filter(function (f) {
    return f.side === 'exterior' && f.orientation === 'vertical';
});
assertEqual('2 verticales ext', verticalExt.length, 2);
assertEqual('vertical ext magenta', verticalExt[0].color, DESIGN_FACE_COLORS.verticalExterior);

const verticalInt = faces.filter(function (f) {
    return f.side === 'interior' && f.orientation === 'vertical';
});
assertEqual('vertical int azul', verticalInt[0].color, DESIGN_FACE_COLORS.verticalInterior);

assertEqual('etiqueta F-1', faces[0].idText, 'F-1');
assertEqual('find F1', findDesignWallFaceById(faces, 'F1').id, 1);
assertEqual('find F-1', findDesignWallFaceById(faces, 'F-1').id, 1);
assertEqual('find ID F3', findDesignWallFaceById(faces, 'ID F3').id, 3);

/* Muro suelto: 2 caras, sin extender. */
const lone = buildDesignWallFaces([
    { id: 10, wallId: 10, p1: { x: 0, z: 0 }, p2: { x: 2000, z: 0 }, thicknessMm: T },
]);
assertEqual('suelto: 2 caras', lone.length, 2);
assertEqual('suelto ext', lone[0].side, 'exterior');
assertEqual('suelto int', lone[1].side, 'interior');
assertClose('suelto no extiende', lone[0].lengthMm, 2000);
assertClose('suelto int misma longitud', lone[1].lengthMm, 2000);
assertClose('suelto ext offset', Math.abs(lone[0].p1.z), half);

/* L: el exterior del tramo horizontal gana la esquina; el interior la pierde. */
const ell = buildDesignWallFaces([
    { id: 21, wallId: 21, p1: { x: 0, z: 0 }, p2: { x: 2000, z: 0 }, thicknessMm: T },
    { id: 22, wallId: 22, p1: { x: 2000, z: 0 }, p2: { x: 2000, z: 2000 }, thicknessMm: T },
], { epsMm: 50 });
assertEqual('L: 4 caras', ell.length, 4);
const ellExtH = ell.find(function (f) {
    return f.axisId === 21 && f.side === 'exterior';
});
const ellIntH = ell.find(function (f) {
    return f.axisId === 21 && f.side === 'interior';
});
assertClose('L ext incluye esquina', ellExtH.lengthMm, 2000 + half);
assertClose('L int recorta esquina', ellIntH.lengthMm, 2000 - half);

console.log('OK desing2-wall-faces 3 casos');
