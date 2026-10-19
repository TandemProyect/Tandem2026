import * as THREE from 'three';

const LONG_MM = 300;
const WIDTH_MM = 300;
const HEIGHT_MM = 2700;

/**
 * Fantasma del pilar en planta: cuadrado de 0,30 × 0,30 m.
 * El doble clic lo deja. El ancho y el largo se editan después; la altura llega al pasar a 3D.
 */
export function createDesing2PillarTool(deps) {
    const scene = deps.scene;
    const group = new THREE.Group();
    group.name = 'desing2-pillar-ghost';
    scene.add(group);

    const material = new THREE.LineBasicMaterial({ color: 0xc47a12, transparent: true, opacity: 0.9 });
    const ghost = new THREE.LineSegments(new THREE.BufferGeometry(), material);
    ghost.frustumCulled = false;
    ghost.visible = false;
    group.add(ghost);

    let active = false;
    let last = null;

    function writeBox(target, cx, cz, y) {
        const hx = LONG_MM * 0.5;
        const hz = WIDTH_MM * 0.5;
        const y0 = (y || 0) + 2;
        const corners = [
            [cx - hx, y0, cz - hz],
            [cx + hx, y0, cz - hz],
            [cx + hx, y0, cz + hz],
            [cx - hx, y0, cz + hz],
        ];
        const pos = [];
        for (let i = 0; i < 4; i++) {
            const a = corners[i];
            const b = corners[(i + 1) % 4];
            pos.push(a[0], a[1], a[2], b[0], b[1], b[2]);
        }
        target.geometry.setAttribute('position', new THREE.Float32BufferAttribute(pos, 3));
        target.geometry.computeBoundingSphere();
    }

    function showAt(point) {
        if (!point) {
            ghost.visible = false;
            return;
        }
        writeBox(ghost, point.x, point.z, point.y || 0);
        ghost.visible = true;
    }

    return {
        isActive: function () {
            return active;
        },
        start: function () {
            active = true;
            showAt(last);
        },
        stop: function () {
            active = false;
            ghost.visible = false;
        },
        moveTo: function (clientX, clientY) {
            if (!active || typeof deps.pickFloor !== 'function') return;
            const point = deps.pickFloor(clientX, clientY);
            if (!point) return;
            last = point;
            showAt(point);
        },
        placeAtPointer: function (clientX, clientY) {
            if (!active || typeof deps.pickFloor !== 'function') return null;
            const point = deps.pickFloor(clientX, clientY);
            if (!point) return null;
            last = point;
            showAt(point);
            return {
                x: point.x,
                y: point.y || 0,
                z: point.z,
                longMm: LONG_MM,
                widthMm: WIDTH_MM,
                heightMm: HEIGHT_MM,
            };
        },
    };
}
