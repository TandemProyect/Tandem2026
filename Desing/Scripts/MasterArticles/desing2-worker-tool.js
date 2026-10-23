import * as THREE from 'three';
import { OBJLoader } from '../Design/jsm/loaders/OBJLoader.js';

/** Alto real del trabajador en la escena (mm). Worker.obj mide 28 mm. */
const MA_STL_WORKER_TARGET_HEIGHT_MM = 1750;
const MA_STL_WORKER_MODELS = {
    Worker: '/Content/DesignTools/Stl/Worker/Worker.obj',
    Worker2: '/Content/DesignTools/Stl/Worker/Worker2.obj?v=20261022c',
};

function maStlWorkerKind(value) {
    const text = value != null ? String(value) : '';
    return text === 'Worker2' ? 'Worker2' : 'Worker';
}

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

/**
 * Worker2 echa el hormigón hacia +X, que es el muro.
 * La pintura va por partes: casco, cara, chaleco, pantalón, botas y la manguera.
 * @param {THREE.BufferGeometry} geometry
 */
function maStlPaintWorker2Geometry(geometry) {
    const srcPos = geometry.attributes.position;
    let minY = Infinity;
    for (let i = 0; i < srcPos.count; i++) {
        const x = srcPos.getX(i);
        const y = srcPos.getY(i);
        const z = srcPos.getZ(i);
        srcPos.setXYZ(i, x, z, y);
        if (z < minY) minY = z;
    }
    const floor = minY + 2.2;
    const kept = [];
    for (let i = 0; i + 2 < srcPos.count; i += 3) {
        const y0 = srcPos.getY(i);
        const y1 = srcPos.getY(i + 1);
        const y2 = srcPos.getY(i + 2);
        if (y0 < floor && y1 < floor && y2 < floor) continue;
        kept.push(i, i + 1, i + 2);
    }
    const next = new Float32Array(kept.length * 3);
    for (let k = 0; k < kept.length; k++) {
        next[k * 3] = srcPos.getX(kept[k]);
        next[k * 3 + 1] = srcPos.getY(kept[k]);
        next[k * 3 + 2] = srcPos.getZ(kept[k]);
    }
    geometry.setAttribute('position', new THREE.BufferAttribute(next, 3));
    geometry.deleteAttribute('normal');
    const pos = geometry.attributes.position;
    minY = Infinity;
    let maxY = -Infinity;
    for (let i = 0; i < pos.count; i++) {
        const y = pos.getY(i);
        if (y < minY) minY = y;
        if (y > maxY) maxY = y;
    }
    const height = maxY - minY > 1e-6 ? maxY - minY : 1;
    let sumX = 0;
    let sumZ = 0;
    let torso = 0;
    let headX = 0;
    let headY = 0;
    let headZ = 0;
    let headN = 0;
    for (let i = 0; i < pos.count; i++) {
        const y = pos.getY(i) - minY;
        const ny = y / height;
        if (ny > 0.48 && ny < 0.78) {
            sumX += pos.getX(i);
            sumZ += pos.getZ(i);
            torso++;
        }
        if (ny > 0.9) {
            headX += pos.getX(i);
            headY += y;
            headZ += pos.getZ(i);
            headN++;
        }
    }
    const cx = torso ? sumX / torso : 0;
    const cz = torso ? sumZ / torso : 0;
    const headCx = (headN ? headX / headN : cx) - cx;
    const headCy = headN ? headY / headN : height * 0.94;
    const headCz = (headN ? headZ / headN : cz) - cz;
    for (let i = 0; i < pos.count; i++) {
        pos.setXYZ(i, pos.getX(i) - cx, pos.getY(i) - minY, pos.getZ(i) - cz);
    }
    const colors = new Float32Array(pos.count * 3);
    let bodyMinX = Infinity;
    let bodyMaxX = -Infinity;
    let bodyMinZ = Infinity;
    let bodyMaxZ = -Infinity;
    for (let i = 0; i < pos.count; i++) {
        const x = pos.getX(i);
        const y = pos.getY(i);
        const z = pos.getZ(i);
        const ny = y / height;
        const dx = x - headCx;
        const dy = y - headCy;
        const dz = z - headCz;
        const hd = Math.sqrt(dx * dx + dy * dy + dz * dz);
        let col = MA_STL_WORKER_COLOR.pants;
        if (hd < 16) {
            if (dy > -4) col = MA_STL_WORKER_COLOR.helmet;
            else if (dx > -4 && dy > -13) col = MA_STL_WORKER_COLOR.skin;
            else if (dy > -9) col = MA_STL_WORKER_COLOR.helmet;
        } else if (x < -16 && ny > 0.28 && ny < 0.88) {
            col = MA_STL_WORKER_COLOR.tool;
        } else if (ny > 0.4 && ny < 0.78 && Math.abs(z) > 14 && Math.abs(z) < 26 && x > -12) {
            col = MA_STL_WORKER_COLOR.skin;
        } else if (ny > 0.58 && ny < 0.84 && x > -10 && x < 18 && Math.abs(z) < 15) {
            if (Math.abs(ny - 0.65) < 0.018 || Math.abs(ny - 0.74) < 0.018) col = MA_STL_WORKER_COLOR.stripe;
            else if (x > 0 && Math.abs(z) < 6 && ny > 0.6 && ny < 0.76) col = MA_STL_WORKER_COLOR.shirt;
            else col = MA_STL_WORKER_COLOR.vest;
        } else if (ny < 0.055 && x > -8) {
            col = MA_STL_WORKER_COLOR.boots;
        }
        if (col !== MA_STL_WORKER_COLOR.tool && ny > 0.08 && ny < 0.9 && x > -14 && x < 22) {
            if (x < bodyMinX) bodyMinX = x;
            if (x > bodyMaxX) bodyMaxX = x;
            if (z < bodyMinZ) bodyMinZ = z;
            if (z > bodyMaxZ) bodyMaxZ = z;
        }
        colors[i * 3] = col.r;
        colors[i * 3 + 1] = col.g;
        colors[i * 3 + 2] = col.b;
    }
    pos.needsUpdate = true;
    geometry.setAttribute('color', new THREE.BufferAttribute(colors, 3));
    geometry.computeVertexNormals();
    geometry.computeBoundingSphere();
    if (!Number.isFinite(bodyMinX)) {
        bodyMinX = -8;
        bodyMaxX = 12;
        bodyMinZ = -8;
        bodyMaxZ = 8;
    }
    return {
        geometry: geometry,
        height: height,
        bodyMinX: bodyMinX,
        bodyMaxX: bodyMaxX,
        bodyMinZ: bodyMinZ,
        bodyMaxZ: bodyMaxZ,
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

    const paintedByKind = {};
    const loadByKind = {};
    let ghost = null;
    let active = false;
    let activeKind = 'Worker';
    let lastPose = null;

    function modelOf(kind) {
        return paintedByKind[maStlWorkerKind(kind || activeKind)] || null;
    }

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

    function baseScale(kind) {
        const model = modelOf(kind);
        return model ? MA_STL_WORKER_TARGET_HEIGHT_MM / model.height : 1;
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
        const model = modelOf(activeKind);
        const heightScale = baseScale(activeKind);
        pose.scale = heightScale;
        if (!model || !carrier) return pose;
        const depth = model.bodyMaxX - model.bodyMinX;
        if (!(depth > 1)) return pose;
        const fx = Math.cos(pose.rotY);
        const fz = -Math.sin(pose.rotY);
        const span = platformSpan(carrier, fx, fz);
        if (!span) return pose;
        const pad = 40;
        const usable = span.max - span.min - pad * 2;
        if (usable > 200 && depth * heightScale > usable) pose.scale = usable / depth;
        let along = pose.x * fx + pose.z * fz;
        const back = along + model.bodyMinX * pose.scale;
        if (back < span.min + pad) along += span.min + pad - back;
        const front = along + model.bodyMaxX * pose.scale;
        if (front > span.max - pad) along -= front - (span.max - pad);
        const side = pose.x * (-fz) + pose.z * fx;
        pose.x = along * fx + side * (-fz);
        pose.z = along * fz + side * fx;
        return pose;
    }

    function applyBodyScale(mesh, scaleX) {
        const heightScale = baseScale(mesh.userData.workerKind || activeKind);
        const widthScale = scaleX > 0.05 ? scaleX : heightScale;
        mesh.scale.set(widthScale, heightScale, heightScale);
    }

    function ensureGeometry(kind) {
        const key = maStlWorkerKind(kind);
        if (paintedByKind[key]) return Promise.resolve(paintedByKind[key]);
        if (loadByKind[key]) return loadByKind[key];
        loadByKind[key] = new Promise(function (resolve, reject) {
            const loader = new OBJLoader();
            loader.load(
                MA_STL_WORKER_MODELS[key],
                function (obj) {
                    let source = null;
                    obj.traverse(function (child) {
                        if (!source && child.isMesh && child.geometry) source = child.geometry;
                    });
                    if (!source) {
                        loadByKind[key] = null;
                        reject(new Error('El OBJ del trabajador no tiene malla'));
                        return;
                    }
                    const painted = key === 'Worker2'
                        ? maStlPaintWorker2Geometry(source)
                        : maStlPaintWorkerGeometry(source);
                    paintedByKind[key] = painted;
                    resolve(painted);
                },
                undefined,
                function (err) {
                    loadByKind[key] = null;
                    reject(err || new Error('No se pudo cargar el trabajador'));
                }
            );
        });
        return loadByKind[key];
    }

    function makeMesh(opacity, kind) {
        const key = maStlWorkerKind(kind || activeKind);
        const model = modelOf(key);
        const scale = MA_STL_WORKER_TARGET_HEIGHT_MM / model.height;
        const mesh = new THREE.Mesh(model.geometry, maStlWorkerMaterial(opacity));
        mesh.scale.setScalar(scale);
        mesh.castShadow = opacity >= 0.999;
        mesh.receiveShadow = true;
        mesh.name = 'desing2-worker';
        mesh.userData.workerKind = key;
        return mesh;
    }

    function showGhostAt(point) {
        const model = modelOf(activeKind);
        if (!model || !point) return;
        if (ghost && ghost.userData.workerKind !== activeKind) {
            group.remove(ghost);
            if (ghost.material && ghost.material.dispose) ghost.material.dispose();
            ghost = null;
        }
        if (!ghost) {
            ghost = makeMesh(0.55, activeKind);
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
        if (!modelOf(activeKind) || !point || !active) return;
        const mesh = makeMesh(1, activeKind);
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
            TextCode: mesh.userData.workerKind || 'Worker',
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
        const kind = maStlWorkerKind(item.TextCode != null ? item.TextCode : item.textCode);
        if (!Number.isFinite(x) || !Number.isFinite(z) || !modelOf(kind)) return;
        const mesh = makeMesh(1, kind);
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
        isReady: function (kind) {
            return !!modelOf(kind || activeKind);
        },
        start: function (kind) {
            activeKind = maStlWorkerKind(kind);
            active = true;
            return ensureGeometry(activeKind).then(function () {
                if (!active) return;
                if (lastPose) lastPose.scale = 0;
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
                    TextCode: mesh.userData.workerKind || 'Worker',
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
            if (!list.length) return;
            for (let i = 0; i < list.length; i++) addRow(list[i]);
        },
        restore: function (rows) {
            const list = Array.isArray(rows) ? rows : [];
            if (!list.length) {
                clearPlaced();
                return Promise.resolve();
            }
            const kinds = [];
            for (let i = 0; i < list.length; i++) {
                const row = list[i] || {};
                const key = maStlWorkerKind(row.TextCode != null ? row.TextCode : row.textCode);
                if (kinds.indexOf(key) < 0) kinds.push(key);
            }
            return Promise.all(kinds.map(function (key) { return ensureGeometry(key); })).then(function () {
                clearPlaced();
                for (let i = 0; i < list.length; i++) addRow(list[i]);
            });
        },
    };
}
