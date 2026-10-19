import * as THREE from 'three';
import { OBJLoader } from '../Design/jsm/loaders/OBJLoader.js';

/** Alto real del trabajador en la escena (mm). El OBJ mide 28 mm. */
const MA_STL_WORKER_TARGET_HEIGHT_MM = 1750;
const MA_STL_WORKER_OBJ_URL = '/Content/DesignTools/Stl/Worker/Worker.obj';

const MA_STL_WORKER_COLOR = {
    helmet: new THREE.Color(0xf2b43a),
    skin: new THREE.Color(0xe8c09a),
    shirt: new THREE.Color(0xc8ccc8),
    vest: new THREE.Color(0xf4f04a),
    stripe: new THREE.Color(0x2c3128),
    pants: new THREE.Color(0x1a568f),
    boots: new THREE.Color(0x161616),
    tool: new THREE.Color(0x3a3a3a),
};

/**
 * Colores de la foto del peón (casco amarillo, camiseta gris, chaleco
 * con rayas, brazos de piel, pantalón azul, botas y barra oscuras).
 * El rastrillo queda por +X, separado del cuerpo por un hueco.
 * @param {THREE.BufferGeometry} geometry
 */
function maStlPaintWorkerGeometry(geometry) {
    const geo = geometry;
    geo.computeBoundingBox();
    const box = geo.boundingBox;
    const size = new THREE.Vector3();
    box.getSize(size);
    const center = new THREE.Vector3();
    box.getCenter(center);
    geo.translate(-center.x, -box.min.y, -center.z);
    geo.computeBoundingBox();
    const height = size.y > 1e-6 ? size.y : 1;
    const pos = geo.attributes.position;
    const minX = geo.boundingBox.min.x;
    const spanX = Math.max(geo.boundingBox.max.x - minX, 1e-6);
    const toolMinX = maStlWorkerRakeSplitX(pos, minX, spanX);
    let bodyMinX = Infinity;
    let bodyMaxX = -Infinity;
    let bodyMinZ = Infinity;
    let bodyMaxZ = -Infinity;
    let bodySumX = 0;
    let bodySumZ = 0;
    let bodyCount = 0;
    for (let i = 0; i < pos.count; i++) {
        const x = pos.getX(i);
        if (x >= toolMinX) continue;
        const z = pos.getZ(i);
        if (x < bodyMinX) bodyMinX = x;
        if (x > bodyMaxX) bodyMaxX = x;
        if (z < bodyMinZ) bodyMinZ = z;
        if (z > bodyMaxZ) bodyMaxZ = z;
        bodySumX += x;
        bodySumZ += z;
        bodyCount++;
    }
    const bodyCenterX = bodyCount ? bodySumX / bodyCount : 0;
    const bodyCenterZ = bodyCount ? bodySumZ / bodyCount : 0;
    geo.translate(-bodyCenterX, 0, -bodyCenterZ);
    const toolAt = toolMinX - bodyCenterX;
    const colors = new Float32Array(pos.count * 3);
    for (let i = 0; i < pos.count; i++) {
        const x = pos.getX(i);
        const y = pos.getY(i);
        const z = pos.getZ(i);
        const ny = y / height;
        let col = MA_STL_WORKER_COLOR.pants;
        if (x >= toolAt) {
            col = MA_STL_WORKER_COLOR.tool;
        } else if (ny < 0.075) {
            col = MA_STL_WORKER_COLOR.boots;
        } else if (ny > 0.9 || (ny > 0.84 && x > 1 && z > 2)) {
            col = MA_STL_WORKER_COLOR.helmet;
        } else if (ny > 0.78 && x > -0.2 && z > 0.6) {
            col = MA_STL_WORKER_COLOR.skin;
        } else if (x > 4.2 && ny > 0.32 && ny < 0.78) {
            col = MA_STL_WORKER_COLOR.skin;
        } else if (ny > 0.5 && ny < 0.76 && x > -1.2 && x < 2.6 && Math.abs(z) < 1.7) {
            col = MA_STL_WORKER_COLOR.shirt;
        } else if (ny > 0.42 && ny < 0.8) {
            const band = Math.abs(ny - 0.57) < 0.02 || Math.abs(ny - 0.67) < 0.02;
            col = band ? MA_STL_WORKER_COLOR.stripe : MA_STL_WORKER_COLOR.vest;
        }
        colors[i * 3] = col.r;
        colors[i * 3 + 1] = col.g;
        colors[i * 3 + 2] = col.b;
    }
    geo.setAttribute('color', new THREE.BufferAttribute(colors, 3));
    if (!geo.attributes.normal) geo.computeVertexNormals();
    geo.computeBoundingSphere();
    return {
        geometry: geo,
        height: height,
        bodyMinX: bodyMinX - bodyCenterX,
        bodyMaxX: bodyMaxX - bodyCenterX,
        bodyMinZ: bodyMinZ - bodyCenterZ,
        bodyMaxZ: bodyMaxZ - bodyCenterZ,
    };
}

function maStlWorkerRakeSplitX(pos, minX, spanX) {
    const bins = 48;
    const counts = new Array(bins).fill(0);
    for (let i = 0; i < pos.count; i++) {
        const b = Math.max(0, Math.min(bins - 1, Math.floor(((pos.getX(i) - minX) / spanX) * bins)));
        counts[b]++;
    }
    let seenBody = false;
    let gapStart = -1;
    for (let i = 0; i < bins; i++) {
        const dense = counts[i] > 40;
        if (!seenBody) {
            if (dense) seenBody = true;
            continue;
        }
        if (!dense) {
            if (gapStart < 0) gapStart = i;
            continue;
        }
        if (gapStart >= 0 && i - gapStart >= 2) {
            return minX + (gapStart / bins) * spanX;
        }
        gapStart = -1;
    }
    return minX + spanX * 0.62;
}

function maStlWorkerMaterial(opacity) {
    const ghost = opacity < 0.999;
    return new THREE.MeshLambertMaterial({
        vertexColors: true,
        transparent: ghost,
        opacity: opacity,
        depthWrite: !ghost,
        side: THREE.FrontSide,
    });
}

/**
 * Trabajadores del visor Desing_2: un OBJ coloreado, varios en la escena.
 * @param {{ scene: THREE.Scene, getCamera: () => THREE.Camera, getCanvas: () => HTMLCanvasElement }} deps
 */
export function createDesing2WorkerTool(deps) {
    const group = new THREE.Group();
    group.name = 'desing2-workers';
    group.visible = false;
    deps.scene.add(group);

    const raycaster = new THREE.Raycaster();
    const pointerNdc = new THREE.Vector2();
    const floorPlane = new THREE.Plane(new THREE.Vector3(0, 1, 0), 0);
    const floorHit = new THREE.Vector3();
    const deckBox = new THREE.Box3();
    const outward = new THREE.Vector3();
    const deckQuat = new THREE.Quaternion();

    let painted = null;
    let loadPromise = null;
    let ghost = null;
    let active = false;
    let lastPose = null;

    function setPointer(clientX, clientY) {
        const canvas = deps.getCanvas();
        const camera = deps.getCamera();
        if (!canvas || !camera) return false;
        const rect = canvas.getBoundingClientRect();
        pointerNdc.set(
            ((clientX - rect.left) / Math.max(rect.width, 1)) * 2 - 1,
            -((clientY - rect.top) / Math.max(rect.height, 1)) * 2 + 1
        );
        camera.updateMatrixWorld(true);
        raycaster.setFromCamera(pointerNdc, camera);
        return true;
    }

    function deckCarrier(mesh) {
        let node = mesh;
        while (node) {
            const code = node.userData && node.userData.maStlAtk60ElementCode;
            if (code != null) {
                const text = String(code);
                if (text === 'Tablon24x15x5' || text === 'Tablon24x15x5-2') return node;
            }
            node = node.parent;
        }
        return null;
    }

    function isFloorPlank(carrier) {
        return String(carrier.userData.maStlAtk60ElementCode || '') === 'Tablon24x15x5';
    }

    function floorPlankTopY(carrier, hitPoint) {
        carrier.updateMatrixWorld(true);
        if (isFloorPlank(carrier)) {
            deckBox.setFromObject(carrier);
            return deckBox.max.y;
        }
        const wallId = carrier.userData.maStlAtk60SampleWallId != null
            ? String(carrier.userData.maStlAtk60SampleWallId)
            : '';
        const meshes = typeof deps.getDeckMeshes === 'function' ? deps.getDeckMeshes() : null;
        if (!meshes) return null;
        let bestY = null;
        let bestD = Infinity;
        for (let i = 0; i < meshes.length; i++) {
            const deck = deckCarrier(meshes[i]);
            if (!deck || !isFloorPlank(deck)) continue;
            const id = deck.userData.maStlAtk60SampleWallId != null
                ? String(deck.userData.maStlAtk60SampleWallId)
                : '';
            if (wallId && id !== wallId) continue;
            deckBox.setFromObject(deck);
            const cx = (deckBox.min.x + deckBox.max.x) * 0.5;
            const cz = (deckBox.min.z + deckBox.max.z) * 0.5;
            const d = (cx - hitPoint.x) * (cx - hitPoint.x) + (cz - hitPoint.z) * (cz - hitPoint.z);
            if (d < bestD && Number.isFinite(deckBox.max.y)) {
                bestD = d;
                bestY = deckBox.max.y;
            }
        }
        return bestY;
    }

    function faceWallYaw(carrier) {
        carrier.updateMatrixWorld(true);
        carrier.getWorldQuaternion(deckQuat);
        // Unión vertical (Rx -90): el +Y local queda horizontal y apunta al muro.
        outward.set(0, 1, 0).applyQuaternion(deckQuat);
        outward.y = 0;
        if (outward.lengthSq() < 1e-6) {
            outward.set(0, 0, 1).applyQuaternion(deckQuat);
            outward.y = 0;
        }
        if (outward.lengthSq() < 1e-6) return 0;
        outward.normalize();
        return Math.atan2(-outward.z, outward.x);
    }

    function poseAt(clientX, clientY) {
        if (!setPointer(clientX, clientY)) return null;
        const decks = typeof deps.getDeckMeshes === 'function' ? deps.getDeckMeshes() : null;
        if (decks && decks.length) {
            const hits = raycaster.intersectObjects(decks, false);
            if (hits.length) {
                const carrier = deckCarrier(hits[0].object);
                if (carrier) {
                    const y = floorPlankTopY(carrier, hits[0].point);
                    if (y != null) {
                        const pose = fitWorkerPose({
                            x: hits[0].point.x,
                            y: y,
                            z: hits[0].point.z,
                            rotY: faceWallYaw(carrier),
                        }, carrier);
                        pose.wallId = wallIdOf(carrier);
                        return pose;
                    }
                }
            }
        }
        if (raycaster.ray.intersectPlane(floorPlane, floorHit) === null) return null;
        return fitWorkerPose({ x: floorHit.x, y: 0, z: floorHit.z, rotY: 0 }, null);
    }

    function wallIdOf(node) {
        let current = node;
        while (current) {
            const id = current.userData && current.userData.maStlAtk60SampleWallId;
            if (id != null && id !== '') return String(id);
            current = current.parent;
        }
        return null;
    }

    function baseScale() {
        return painted ? MA_STL_WORKER_TARGET_HEIGHT_MM / painted.height : 1;
    }

    function platformSpan(carrier, fx, fz) {
        const meshes = typeof deps.getDeckMeshes === 'function' ? deps.getDeckMeshes() : null;
        if (!meshes || !carrier) return null;
        const wallId = carrier.userData.maStlAtk60SampleWallId != null
            ? String(carrier.userData.maStlAtk60SampleWallId)
            : '';
        let min = Infinity;
        let max = -Infinity;
        const seen = [];
        for (let i = 0; i < meshes.length; i++) {
            const deck = deckCarrier(meshes[i]);
            if (!deck || !isFloorPlank(deck) || seen.indexOf(deck) >= 0) continue;
            seen.push(deck);
            const id = deck.userData.maStlAtk60SampleWallId != null
                ? String(deck.userData.maStlAtk60SampleWallId)
                : '';
            if (wallId && id !== wallId) continue;
            deck.updateMatrixWorld(true);
            deckBox.setFromObject(deck);
            const xs = [deckBox.min.x, deckBox.max.x];
            const zs = [deckBox.min.z, deckBox.max.z];
            for (let xi = 0; xi < xs.length; xi++) {
                for (let zi = 0; zi < zs.length; zi++) {
                    const p = xs[xi] * fx + zs[zi] * fz;
                    if (p < min) min = p;
                    if (p > max) max = p;
                }
            }
        }
        if (!Number.isFinite(min) || !Number.isFinite(max) || max - min < 200) return null;
        return { min: min, max: max };
    }

    function fitWorkerPose(pose, carrier) {
        const heightScale = baseScale();
        pose.scale = heightScale;
        if (!painted || !carrier) return pose;
        const depth = painted.bodyMaxX - painted.bodyMinX;
        if (!(depth > 1)) return pose;
        const fx = Math.cos(pose.rotY);
        const fz = -Math.sin(pose.rotY);
        const span = platformSpan(carrier, fx, fz);
        if (!span) return pose;
        const pad = 40;
        const usable = span.max - span.min - pad * 2;
        if (usable > 200 && depth * heightScale > usable) pose.scale = usable / depth;
        let along = pose.x * fx + pose.z * fz;
        const back = along + painted.bodyMinX * pose.scale;
        if (back < span.min + pad) along += span.min + pad - back;
        const front = along + painted.bodyMaxX * pose.scale;
        if (front > span.max - pad) along -= front - (span.max - pad);
        const side = pose.x * (-fz) + pose.z * fx;
        pose.x = along * fx + side * (-fz);
        pose.z = along * fz + side * fx;
        return pose;
    }

    function applyBodyScale(mesh, scaleX) {
        const heightScale = baseScale();
        const widthScale = scaleX > 0.05 ? scaleX : heightScale;
        mesh.scale.set(widthScale, heightScale, heightScale);
    }

    function ensureGeometry() {
        if (painted) return Promise.resolve(painted);
        if (loadPromise) return loadPromise;
        loadPromise = new Promise(function (resolve, reject) {
            const loader = new OBJLoader();
            loader.load(
                MA_STL_WORKER_OBJ_URL,
                function (obj) {
                    let source = null;
                    obj.traverse(function (child) {
                        if (!source && child.isMesh && child.geometry) source = child.geometry;
                    });
                    if (!source) {
                        reject(new Error('El OBJ del trabajador no tiene malla'));
                        return;
                    }
                    painted = maStlPaintWorkerGeometry(source);
                    resolve(painted);
                },
                undefined,
                function (err) {
                    loadPromise = null;
                    reject(err || new Error('No se pudo cargar el trabajador'));
                }
            );
        });
        return loadPromise;
    }

    function makeMesh(opacity) {
        const scale = MA_STL_WORKER_TARGET_HEIGHT_MM / painted.height;
        const mesh = new THREE.Mesh(painted.geometry, maStlWorkerMaterial(opacity));
        mesh.scale.setScalar(scale);
        mesh.castShadow = opacity >= 0.999;
        mesh.receiveShadow = true;
        mesh.name = 'desing2-worker';
        return mesh;
    }

    function showGhostAt(point) {
        if (!painted || !point) return;
        if (!ghost) {
            ghost = makeMesh(0.55);
            ghost.userData.ghost = true;
            ghost.raycast = function () {};
            group.add(ghost);
        }
        ghost.visible = active;
        ghost.position.set(point.x, point.y || 0, point.z);
        ghost.rotation.y = point.rotY || 0;
        if (point.scale > 0) applyBodyScale(ghost, point.scale);
    }

    function placedMeshes() {
        const list = [];
        for (let i = 0; i < group.children.length; i++) {
            const child = group.children[i];
            if (child && child.isMesh && !child.userData.ghost) list.push(child);
        }
        return list;
    }

    function clearPlaced() {
        const meshes = placedMeshes();
        for (let i = 0; i < meshes.length; i++) {
            const mesh = meshes[i];
            group.remove(mesh);
            if (mesh.material && mesh.material.dispose) mesh.material.dispose();
        }
    }

    function placeAt(point) {
        if (!painted || !point || !active) return;
        const mesh = makeMesh(1);
        mesh.position.set(point.x, point.y || 0, point.z);
        mesh.rotation.y = point.rotY || 0;
        if (point.scale > 0) applyBodyScale(mesh, point.scale);
        mesh.userData.kind = 'desing2-worker';
        let wallId = point.wallId || null;
        if (!wallId && typeof deps.wallIdAt === 'function') {
            wallId = deps.wallIdAt(point.x, point.z);
        }
        if (wallId) mesh.userData.maStlWorkerWallId = String(wallId);
        group.add(mesh);
    }

    function rememberWall(mesh, wallId) {
        if (wallId) mesh.userData.maStlWorkerWallId = String(wallId);
    }

    function rowFromMesh(mesh) {
        return {
            XMm: mesh.position.x,
            YMm: mesh.position.y,
            ZMm: mesh.position.z,
            RotY: mesh.rotation.y,
            Scale: mesh.scale.x,
            wallId: mesh.userData.maStlWorkerWallId || null,
        };
    }

    function addRow(row) {
        const item = row || {};
        const x = Number(item.XMm != null ? item.XMm : item.xMm);
        const y = Number(item.YMm != null ? item.YMm : item.yMm);
        const z = Number(item.ZMm != null ? item.ZMm : item.zMm);
        const rot = Number(item.RotY != null ? item.RotY : item.rotY);
        const scale = Number(item.Scale != null ? item.Scale : item.scale);
        const wallId = item.wallId != null ? item.wallId : item.WallId;
        if (!Number.isFinite(x) || !Number.isFinite(z)) return;
        const mesh = makeMesh(1);
        mesh.position.set(x, Number.isFinite(y) ? y : 0, z);
        if (Number.isFinite(rot)) mesh.rotation.y = rot;
        applyBodyScale(mesh, scale);
        mesh.userData.kind = 'desing2-worker';
        rememberWall(mesh, wallId);
        group.add(mesh);
    }

    function pickPlaced(clientX, clientY) {
        const canvas = deps.getCanvas();
        const camera = deps.getCamera();
        const meshes = placedMeshes();
        if (!canvas || !camera || !meshes.length) return null;
        const rect = canvas.getBoundingClientRect();
        pointerNdc.set(
            ((clientX - rect.left) / Math.max(rect.width, 1)) * 2 - 1,
            -((clientY - rect.top) / Math.max(rect.height, 1)) * 2 + 1
        );
        camera.updateMatrixWorld(true);
        raycaster.setFromCamera(pointerNdc, camera);
        const hits = raycaster.intersectObjects(meshes, false);
        return hits.length ? hits[0].object : null;
    }

    return {
        isActive: function () {
            return active;
        },
        setVisible: function (on) {
            group.visible = !!on;
        },
        isReady: function () {
            return !!painted;
        },
        start: function () {
            active = true;
            return ensureGeometry().then(function () {
                if (!active) return;
                showGhostAt(lastPose);
            });
        },
        stop: function () {
            active = false;
            if (ghost) ghost.visible = false;
        },
        moveTo: function (clientX, clientY) {
            if (!active) return;
            const pose = poseAt(clientX, clientY);
            if (!pose) return;
            lastPose = pose;
            showGhostAt(pose);
        },
        placeAtPointer: function (clientX, clientY) {
            if (!active) return false;
            const pose = poseAt(clientX, clientY);
            if (!pose) return false;
            lastPose = pose;
            placeAt(pose);
            return true;
        },
        deleteAt: function (clientX, clientY) {
            const hit = pickPlaced(clientX, clientY);
            if (!hit) return false;
            group.remove(hit);
            if (hit.material && hit.material.dispose) hit.material.dispose();
            return true;
        },
        list: function () {
            return placedMeshes().map(function (mesh) {
                return {
                    XMm: mesh.position.x,
                    YMm: mesh.position.y,
                    ZMm: mesh.position.z,
                    RotY: mesh.rotation.y,
                    Scale: mesh.scale.x,
                };
            });
        },
        removeWhere: function (match) {
            const removed = [];
            const meshes = placedMeshes();
            for (let i = 0; i < meshes.length; i++) {
                const mesh = meshes[i];
                if (!match(mesh)) continue;
                removed.push(rowFromMesh(mesh));
                group.remove(mesh);
                if (mesh.material && mesh.material.dispose) mesh.material.dispose();
            }
            return removed;
        },
        appendRows: function (rows) {
            const list = Array.isArray(rows) ? rows : [];
            if (!painted || !list.length) return;
            for (let i = 0; i < list.length; i++) addRow(list[i]);
        },
        restore: function (rows) {
            const list = Array.isArray(rows) ? rows : [];
            if (!list.length) {
                clearPlaced();
                return Promise.resolve();
            }
            return ensureGeometry().then(function () {
                clearPlaced();
                for (let i = 0; i < list.length; i++) addRow(list[i]);
            });
        },
    };
}
