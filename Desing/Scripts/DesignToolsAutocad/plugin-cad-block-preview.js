import * as THREE from 'three';
import { OrbitControls } from '@masterarticles/OrbitControls';
import { STLLoader } from '@masterarticles/STLLoader';

const FRAME_HEX = 0xefb608;
const PHENOLIC_HEX = 0x1a1816;
const GENERIC_HEX = 0xc5d86d;
const VC_CSS_TZ = -230;
const FACE_DIR = {
    front: new THREE.Vector3(0, 0, 1),
    back: new THREE.Vector3(0, 0, -1),
    top: new THREE.Vector3(0, 1, 0),
    bottom: new THREE.Vector3(0, -1, 0),
    right: new THREE.Vector3(1, 0, 0),
    left: new THREE.Vector3(-1, 0, 0)
};

const prefs = {
    frameHex: FRAME_HEX,
    phenolicHex: PHENOLIC_HEX,
    dark: false,
    clipOn: false,
    axesOn: true,
    expanded: false
};

let state = null;
let cameraMode = 'ortho';
const _vcM = new THREE.Matrix4();
const clipPlaneY = new THREE.Plane();
const clipPlaneX = new THREE.Plane();

function parseCssHex(css, fallback) {
    const n = parseInt(String(css || '').replace('#', ''), 16);
    return Number.isFinite(n) ? n : fallback;
}

function toCssHex(n) {
    return '#' + ((n >>> 0) & 0xffffff).toString(16).padStart(6, '0');
}

function dispose() {
    if (!state) return;
    try {
        if (state.ro) state.ro.disconnect();
    } catch (e) { }
    try {
        if (state.stop) state.stop();
    } catch (e0) { }
    try {
        if (state.renderer) {
            state.renderer.dispose();
            if (state.renderer.domElement && state.renderer.domElement.parentNode)
                state.renderer.domElement.parentNode.removeChild(state.renderer.domElement);
        }
    } catch (e2) { }
    state = null;
}

function vcEpsilon(value) {
    return Math.abs(value) < 1e-10 ? 0 : value;
}

function getCameraCssMatrix3d(matrix) {
    const el = matrix.elements;
    return (
        'matrix3d(' +
        vcEpsilon(el[0]) + ',' + vcEpsilon(-el[1]) + ',' + vcEpsilon(el[2]) + ',' + vcEpsilon(el[3]) + ',' +
        vcEpsilon(el[4]) + ',' + vcEpsilon(-el[5]) + ',' + vcEpsilon(el[6]) + ',' + vcEpsilon(el[7]) + ',' +
        vcEpsilon(el[8]) + ',' + vcEpsilon(-el[9]) + ',' + vcEpsilon(el[10]) + ',' + vcEpsilon(el[11]) + ',' +
        vcEpsilon(el[12]) + ',' + vcEpsilon(-el[13]) + ',' + vcEpsilon(el[14]) + ',' + vcEpsilon(el[15]) + ')'
    );
}

function setViewCubeCssFromCamera(cubeEl, camera) {
    if (!cubeEl || !camera) return;
    camera.updateWorldMatrix(true, false);
    _vcM.extractRotation(camera.matrixWorldInverse);
    cubeEl.style.transform = 'translateZ(' + VC_CSS_TZ + 'px) ' + getCameraCssMatrix3d(_vcM);
}

function syncCamButtons() {
    const orthoBtn = document.getElementById('plugin-blocks-cam-ortho');
    const isoBtn = document.getElementById('plugin-blocks-cam-iso');
    if (orthoBtn) orthoBtn.classList.toggle('is-on', cameraMode === 'ortho');
    if (isoBtn) isoBtn.classList.toggle('is-on', cameraMode === 'iso');
}

function syncToolButtons() {
    const darkBtn = document.getElementById('plugin-blocks-tool-dark');
    const clipBtn = document.getElementById('plugin-blocks-tool-clip');
    const axesBtn = document.getElementById('plugin-blocks-tool-axes');
    const expandBtn = document.getElementById('plugin-blocks-tool-expand');
    const clipBox = document.getElementById('plugin-blocks-clip');
    const root = document.querySelector('.plugin-blocks');
    if (darkBtn) darkBtn.classList.toggle('is-on', prefs.dark);
    if (clipBtn) clipBtn.classList.toggle('is-on', prefs.clipOn);
    if (axesBtn) axesBtn.classList.toggle('is-on', prefs.axesOn);
    if (expandBtn) expandBtn.classList.toggle('is-on', prefs.expanded);
    if (clipBox) clipBox.classList.toggle('is-on', prefs.clipOn);
    if (root) root.classList.toggle('is-viewer-max', prefs.expanded);
}

function activeCamera() {
    if (!state) return null;
    return cameraMode === 'iso' ? state.cameraIso : state.cameraOrtho;
}

function bindControls(camera) {
    if (!state || !state.controls || !camera) return;
    state.controls.object = camera;
    state.controls.update();
}

function applyFrustum(cam, w, h, dist) {
    if (!cam) return;
    const aspect = Math.max(w, 1) / Math.max(h, 1);
    if (cam.isPerspectiveCamera) {
        cam.aspect = aspect;
        cam.updateProjectionMatrix();
        return;
    }
    const half = Math.max(dist, 1) * 0.42;
    cam.left = -half * aspect;
    cam.right = half * aspect;
    cam.top = half;
    cam.bottom = -half;
    cam.updateProjectionMatrix();
}

function lookFace(face) {
    if (!state) return;
    const dir = FACE_DIR[face];
    if (!dir) return;
    const cam = activeCamera();
    const target = state.controls ? state.controls.target : new THREE.Vector3();
    const dist = state.dist || cam.position.distanceTo(target) || 1000;
    const p = dir.clone().normalize().multiplyScalar(dist);
    cam.up.set(0, 1, 0);
    cam.position.copy(target).add(p);
    cam.lookAt(target);
    applyFrustum(cam, state.width, state.height, dist);
    bindControls(cam);
    if (typeof state.controls.saveState === 'function') state.controls.saveState();
}

function setCameraMode(mode) {
    cameraMode = mode === 'iso' ? 'iso' : 'ortho';
    syncCamButtons();
    if (!state) return;
    const from = cameraMode === 'iso' ? state.cameraOrtho : state.cameraIso;
    const to = activeCamera();
    if (from && to) {
        to.position.copy(from.position);
        to.up.copy(from.up);
        to.lookAt(state.controls.target);
    }
    applyFrustum(to, state.width, state.height, state.dist);
    bindControls(to);
}

function frameObject(camera, controls, object) {
    const box = new THREE.Box3().setFromObject(object);
    const size = box.getSize(new THREE.Vector3());
    const center = box.getCenter(new THREE.Vector3());
    object.position.sub(center);
    const maxDim = Math.max(size.x, size.y, size.z, 1e-6);
    const dist = maxDim * 2.2;
    camera.position.set(dist * 0.85, dist * 0.55, dist * 0.85);
    controls.target.set(0, 0, 0);
    camera.lookAt(controls.target);
    controls.update();
    if (state) {
        state.dist = dist;
        if (state.cameraOrtho && state.cameraOrtho !== camera) {
            state.cameraOrtho.position.copy(camera.position);
            state.cameraOrtho.lookAt(controls.target);
        }
        if (state.cameraIso && state.cameraIso !== camera) {
            state.cameraIso.position.copy(camera.position);
            state.cameraIso.lookAt(controls.target);
        }
        applyFrustum(state.cameraOrtho, state.width, state.height, dist);
        applyFrustum(state.cameraIso, state.width, state.height, dist);
        object.updateMatrixWorld(true);
        state.clipBounds = new THREE.Box3().setFromObject(object);
        updateClipPlanes();
        if (state.axes) {
            state.axes.scale.setScalar(Math.max(maxDim * 0.35, 1));
        }
    }
    return dist;
}

function zoomBy(factor) {
    if (!state) return;
    const cam = activeCamera();
    const target = state.controls.target;
    cam.position.sub(target).multiplyScalar(factor).add(target);
    state.dist = Math.max(cam.position.distanceTo(target), 1);
    applyFrustum(cam, state.width, state.height, state.dist);
    bindControls(cam);
}

function fitView() {
    if (!state || !state.group) return;
    frameObject(activeCamera(), state.controls, state.group);
    bindControls(activeCamera());
}

function setExpanded(on) {
    prefs.expanded = !!on;
    syncToolButtons();
    if (state && state.ro && state.host) {
        requestAnimationFrame(function () {
            const nw = Math.max(state.host.clientWidth || state.width, 120);
            const nh = Math.max(state.host.clientHeight || state.height, 120);
            state.width = nw;
            state.height = nh;
            applyFrustum(state.cameraIso, nw, nh, state.dist);
            applyFrustum(state.cameraOrtho, nw, nh, state.dist);
            state.renderer.setSize(nw, nh);
        });
    }
}

function clipFractionFromSlider(inputEl) {
    if (!inputEl) return 0;
    const vRaw = Number.parseFloat(String(inputEl.value).trim());
    const v = Number.isFinite(vRaw) ? vRaw : 1000;
    return THREE.MathUtils.clamp((1000 - v) / 1000, 0, 1);
}

function updateClipPlanes() {
    if (!state || !state.meshes || !state.meshes.length || !state.clipBounds) return;
    const min = state.clipBounds.min;
    const max = state.clipBounds.max;
    const size = max.clone().sub(min);
    const pad = Math.max(Math.max(size.x, size.y, size.z) * 0.02, 1e-6);
    const h = size.y + 2 * pad;
    const w = size.x + 2 * pad;
    const yEl = document.getElementById('plugin-blocks-clip-y');
    const xEl = document.getElementById('plugin-blocks-clip-x');
    const fY = prefs.clipOn ? clipFractionFromSlider(yEl) : 0;
    const fX = prefs.clipOn ? clipFractionFromSlider(xEl) : 0;
    const cutY = max.y + pad - fY * h;
    const cutX = max.x + pad - fX * w;
    clipPlaneY.setComponents(0, -1, 0, cutY);
    clipPlaneX.setComponents(-1, 0, 0, cutX);
    const planes = prefs.clipOn ? [clipPlaneY, clipPlaneX] : [];
    state.meshes.forEach(function (m) {
        if (m.material) {
            m.material.clippingPlanes = planes;
            m.material.needsUpdate = true;
        }
    });
}

function applyColors() {
    if (!state || !state.meshes) return;
    if (state.frameMesh && state.frameMesh.material)
        state.frameMesh.material.color.setHex(prefs.frameHex);
    if (state.phenolicMesh && state.phenolicMesh.material)
        state.phenolicMesh.material.color.setHex(prefs.phenolicHex);
}

function applyDark() {
    if (!state || !state.scene) return;
    state.scene.background = new THREE.Color(prefs.dark ? 0x1a1a1a : 0xf7f7f7);
}

function applyAxes() {
    if (!state || !state.axes) return;
    state.axes.visible = prefs.axesOn;
}

function loadGeometry(loader, url) {
    return new Promise(function (resolve, reject) {
        loader.load(url, resolve, undefined, function (err) {
            reject(err || new Error('STL'));
        });
    });
}

function makeMaterial(hex) {
    return new THREE.MeshLambertMaterial({
        color: hex,
        side: THREE.DoubleSide,
        clippingPlanes: prefs.clipOn ? [clipPlaneY, clipPlaneX] : []
    });
}

function loadStl(host, url, emptyText, phenolicUrl) {
    dispose();
    if (!host) return;
    host.innerHTML = '';
    const frameUrl = url ? String(url).trim() : '';
    const phenolic = phenolicUrl ? String(phenolicUrl).trim() : '';
    if (!frameUrl) {
        host.innerHTML = '<p class="plugin-blocks__empty">' + (emptyText || '') + '</p>';
        return;
    }

    const w = Math.max(host.clientWidth || 280, 120);
    const h = Math.max(host.clientHeight || 160, 120);
    const scene = new THREE.Scene();
    scene.background = new THREE.Color(prefs.dark ? 0x1a1a1a : 0xf7f7f7);
    const cameraIso = new THREE.PerspectiveCamera(40, w / h, 0.01, 500000);
    const cameraOrtho = new THREE.OrthographicCamera(-1, 1, 1, -1, 0.01, 500000);
    const camera = cameraMode === 'iso' ? cameraIso : cameraOrtho;
    const renderer = new THREE.WebGLRenderer({ antialias: true });
    renderer.localClippingEnabled = true;
    renderer.setPixelRatio(Math.min(window.devicePixelRatio || 1, 2));
    renderer.setSize(w, h);
    host.appendChild(renderer.domElement);

    const controls = new OrbitControls(camera, renderer.domElement);
    controls.enableDamping = true;
    controls.dampingFactor = 0.08;
    scene.add(new THREE.AmbientLight(0xffffff, 0.55));
    const d1 = new THREE.DirectionalLight(0xffe8c0, 1.15);
    d1.position.set(3, 6, 4);
    scene.add(d1);
    const d2 = new THREE.DirectionalLight(0xeef2ff, 0.35);
    d2.position.set(-4, 2, -3);
    scene.add(d2);

    const cubeEl = document.getElementById('plugin-blocks-vc-cube');
    const loader = new STLLoader();
    const phenolicJob = phenolic
        ? loadGeometry(loader, phenolic).catch(function () { return null; })
        : Promise.resolve(null);

    let alive = true;
    const axes = new THREE.AxesHelper(1);
    axes.visible = prefs.axesOn;
    scene.add(axes);

    state = {
        renderer: renderer,
        ro: null,
        stop: function () { alive = false; },
        cameraIso: cameraIso,
        cameraOrtho: cameraOrtho,
        controls: controls,
        width: w,
        height: h,
        dist: 1000,
        scene: scene,
        host: host,
        axes: axes,
        meshes: [],
        group: null,
        frameMesh: null,
        phenolicMesh: null,
        clipBounds: null
    };
    applyFrustum(cameraOrtho, w, h, state.dist);
    applyFrustum(cameraIso, w, h, state.dist);
    syncCamButtons();
    syncToolButtons();

    Promise.all([loadGeometry(loader, frameUrl), phenolicJob]).then(function (geoms) {
        if (!alive) return;
        const group = new THREE.Group();
        const frameGeom = geoms[0];
        const phenolicGeom = geoms[1];
        const hasPair = !!phenolicGeom;
        frameGeom.computeVertexNormals();
        const frameMesh = new THREE.Mesh(
            frameGeom,
            makeMaterial(hasPair ? prefs.frameHex : GENERIC_HEX)
        );
        frameMesh.rotation.x = -0.5 * Math.PI;
        group.add(frameMesh);

        let phenolicMesh = null;
        if (phenolicGeom) {
            phenolicGeom.computeVertexNormals();
            phenolicMesh = new THREE.Mesh(phenolicGeom, makeMaterial(prefs.phenolicHex));
            phenolicMesh.rotation.x = -0.5 * Math.PI;
            group.add(phenolicMesh);
        }

        scene.add(group);
        state.group = group;
        state.frameMesh = frameMesh;
        state.phenolicMesh = phenolicMesh;
        state.meshes = phenolicMesh ? [frameMesh, phenolicMesh] : [frameMesh];
        frameObject(activeCamera(), controls, group);
        bindControls(activeCamera());
        applyColors();
        applyAxes();
    }).catch(function () {
        host.innerHTML = '<p class="plugin-blocks__empty">' + (emptyText || '') + '</p>';
    });

    function tick() {
        if (!alive) return;
        requestAnimationFrame(tick);
        controls.update();
        const cam = activeCamera();
        setViewCubeCssFromCamera(cubeEl, cam);
        renderer.render(scene, cam);
    }
    tick();

    const ro = new ResizeObserver(() => {
        const nw = Math.max(host.clientWidth || w, 120);
        const nh = Math.max(host.clientHeight || h, 120);
        if (!state) return;
        state.width = nw;
        state.height = nh;
        applyFrustum(state.cameraIso, nw, nh, state.dist);
        applyFrustum(state.cameraOrtho, nw, nh, state.dist);
        renderer.setSize(nw, nh);
    });
    ro.observe(host);
    state.ro = ro;
}

function bindViewerChrome() {
    const vc = document.getElementById('plugin-blocks-vc');
    if (vc && !vc.dataset.bound) {
        vc.dataset.bound = '1';
        vc.addEventListener('click', function (ev) {
            const faceEl = ev.target && ev.target.closest ? ev.target.closest('[data-face]') : null;
            if (faceEl) {
                ev.preventDefault();
                ev.stopPropagation();
                lookFace(faceEl.getAttribute('data-face'));
                return;
            }
            const camBtn = ev.target && ev.target.closest ? ev.target.closest('[data-cam]') : null;
            if (camBtn) {
                ev.preventDefault();
                setCameraMode(camBtn.getAttribute('data-cam'));
            }
        });
        vc.addEventListener('mousedown', function (ev) { ev.stopPropagation(); });
    }

    const tools = document.getElementById('plugin-blocks-tools');
    if (tools && !tools.dataset.bound) {
        tools.dataset.bound = '1';
        tools.addEventListener('click', function (ev) {
            const btn = ev.target && ev.target.closest ? ev.target.closest('[data-tool]') : null;
            if (!btn) return;
            const tool = btn.getAttribute('data-tool');
            if (tool === 'expand') setExpanded(!prefs.expanded);
            else if (tool === 'zoomin') zoomBy(0.72);
            else if (tool === 'zoomout') zoomBy(1.38);
            else if (tool === 'fit') fitView();
            else if (tool === 'dark') {
                prefs.dark = !prefs.dark;
                applyDark();
                syncToolButtons();
            } else if (tool === 'clip') {
                prefs.clipOn = !prefs.clipOn;
                updateClipPlanes();
                syncToolButtons();
            } else if (tool === 'axes') {
                prefs.axesOn = !prefs.axesOn;
                applyAxes();
                syncToolButtons();
            }
        });
    }

    const colorFrame = document.getElementById('plugin-blocks-color-frame');
    const colorPhenolic = document.getElementById('plugin-blocks-color-phenolic');
    if (colorFrame && !colorFrame.dataset.bound) {
        colorFrame.dataset.bound = '1';
        colorFrame.value = toCssHex(prefs.frameHex);
        colorFrame.addEventListener('input', function () {
            prefs.frameHex = parseCssHex(colorFrame.value, FRAME_HEX);
            applyColors();
        });
    }
    if (colorPhenolic && !colorPhenolic.dataset.bound) {
        colorPhenolic.dataset.bound = '1';
        colorPhenolic.value = toCssHex(prefs.phenolicHex);
        colorPhenolic.addEventListener('input', function () {
            prefs.phenolicHex = parseCssHex(colorPhenolic.value, PHENOLIC_HEX);
            applyColors();
        });
    }

    const yEl = document.getElementById('plugin-blocks-clip-y');
    const xEl = document.getElementById('plugin-blocks-clip-x');
    if (yEl && !yEl.dataset.bound) {
        yEl.dataset.bound = '1';
        yEl.addEventListener('input', updateClipPlanes);
    }
    if (xEl && !xEl.dataset.bound) {
        xEl.dataset.bound = '1';
        xEl.addEventListener('input', updateClipPlanes);
    }

    syncCamButtons();
    syncToolButtons();
}

bindViewerChrome();

window.TandemCadBlockPreview = { loadStl, lookFace, setCameraMode };
