import * as THREE from 'three';
import { OrbitControls } from '@masterarticles/OrbitControls';
import { STLLoader } from '@masterarticles/STLLoader';
import { InfiniteGridHelper } from '@masterarticles/InfiniteGridHelper';

function disposeObject3D(obj) {
    if (!obj) return;
    obj.traverse(function (child) {
        if (child.geometry) {
            child.geometry.dispose();
        }
        if (child.material) {
            const mats = Array.isArray(child.material) ? child.material : [child.material];
            mats.forEach(function (m) {
                if (m && m.dispose) m.dispose();
            });
        }
    });
    if (obj.parent) obj.parent.remove(obj);
}

function bootMasterArticleDetailsStlViewer() {
    const canvasHost = document.getElementById('ma-stl-viewer-gl-host');
    const statusEl = document.getElementById('master-article-details-stl-viewer-status');
    if (!canvasHost) return;

    let currentRoot = null;
    let loadToken = 0;
    /** Half-height of ortho frustum in world units (before camera.zoom). */
    let frustumHalfY = 1;
    let lastAspect = 1;
    let controls = null;
    /** @type {'ortho' | 'iso'} */
    let activeMode = 'ortho';
    /** Última extensión del modelo (para distancia de cámara en vistas del dado). */
    let lastMaxDim = 1;

    function syncCameraRadios() {
        const ortho = document.getElementById('ma-stl-cam-ortho');
        const iso = document.getElementById('ma-stl-cam-iso');
        if (ortho && iso) {
            ortho.checked = activeMode === 'ortho';
            iso.checked = activeMode === 'iso';
        }
    }

    function viewDistanceFromModel() {
        return Math.max(lastMaxDim * 3, 1e-3);
    }

    function syncViewCubesVisibility() {
        const wo = document.getElementById('ma-stl-view-cube-ortho-wrap');
        const wi = document.getElementById('ma-stl-view-cube-iso-wrap');
        if (wo) wo.classList.toggle('d-none', activeMode !== 'ortho');
        if (wi) wi.classList.toggle('d-none', activeMode !== 'iso');
    }

    /** Direcciones de vista ortogonal (+X derecha, +Y arriba, +Z frente), alineadas con `data-view` del cubo DesignTools. */
    const ORTHO_VIEW_DIR = {
        front: new THREE.Vector3(0, 0, 1),
        back: new THREE.Vector3(0, 0, -1),
        top: new THREE.Vector3(0, 1, 0),
        bottom: new THREE.Vector3(0, -1, 0),
        right: new THREE.Vector3(1, 0, 0),
        left: new THREE.Vector3(-1, 0, 0),
        'front-top': new THREE.Vector3(0, 1, 1),
        'front-bottom': new THREE.Vector3(0, -1, 1),
        'front-left': new THREE.Vector3(-1, 0, 1),
        'front-right': new THREE.Vector3(1, 0, 1),
        'top-back': new THREE.Vector3(0, 1, -1),
        'back-bottom': new THREE.Vector3(0, -1, -1),
        'back-right': new THREE.Vector3(1, 0, -1),
        'back-left': new THREE.Vector3(-1, 0, -1),
        'left-top': new THREE.Vector3(-1, 1, 0),
        'right-top': new THREE.Vector3(1, 1, 0),
        'left-bottom': new THREE.Vector3(-1, -1, 0),
        'right-bottom': new THREE.Vector3(1, -1, 0),
        'front-top-left': new THREE.Vector3(-1, 1, 1),
        'front-top-right': new THREE.Vector3(1, 1, 1),
        'front-bottom-left': new THREE.Vector3(-1, -1, 1),
        'front-bottom-right': new THREE.Vector3(1, -1, 1),
        'top-back-left': new THREE.Vector3(-1, 1, -1),
        'top-back-right': new THREE.Vector3(1, 1, -1),
        'bottom-back-left': new THREE.Vector3(-1, -1, -1),
        'bottom-back-right': new THREE.Vector3(1, -1, -1)
    };

    /** Coloca una cámara ortográfica mirando al origen desde `dir` (mundo; misma convención que `ORTHO_VIEW_DIR`). */
    function applyDirectionToOrthoCam(camera, dir) {
        const d = viewDistanceFromModel();
        const p = dir.clone();
        if (p.lengthSq() < 1e-12) return;
        p.normalize().multiplyScalar(d);
        camera.up.set(0, 1, 0);
        if (Math.abs(p.y) > d * 0.999) {
            camera.up.set(0, 0, p.y > 0 ? -1 : 1);
        }
        camera.position.copy(p);
        camera.lookAt(0, 0, 0);
        camera.zoom = 1;
        camera.updateProjectionMatrix();
        controls.target.set(0, 0, 0);
        controls.update();
    }

    function applyOrthoDirection(dir) {
        applyDirectionToOrthoCam(cameraOrtho, dir);
    }

    function applyOrthoDataView(viewKey) {
        const dir = ORTHO_VIEW_DIR[viewKey];
        if (!dir) return;
        activeMode = 'ortho';
        syncCameraRadios();
        bindControls(cameraOrtho);
        applyOrthoDirection(dir);
        syncViewCubesVisibility();
    }

    function applyOrthoFaceToView(face) {
        applyOrthoDataView(face);
    }

    /**
     * Caras del cubo isométrico: mismas vistas ortogonales que TOP / FRONT / … (planta, alzados).
     * Antes se usaban direcciones diagonales por cara, y TOP no era planta sino otra vista 3/4.
     */
    function applyIsoFaceToView(face) {
        const dir = ORTHO_VIEW_DIR[face];
        if (!dir) return;
        activeMode = 'iso';
        syncCameraRadios();
        bindControls(cameraIso);
        applyDirectionToOrthoCam(cameraIso, dir);
        syncViewCubesVisibility();
    }

    const scene = new THREE.Scene();
    scene.background = new THREE.Color(0xffffff);

    const infiniteGrid = new InfiniteGridHelper(8, 32, new THREE.Color(0x9aa3ad), 500);
    scene.add(infiniteGrid);
    let gridVisible = false;
    const gridToggleBtn = document.getElementById('ma-stl-grid-toggle');
    const gridToggleLabel = document.getElementById('ma-stl-grid-toggle-label');
    function syncGridToggleUi() {
        infiniteGrid.visible = gridVisible;
        if (gridToggleBtn) {
            gridToggleBtn.setAttribute('aria-pressed', gridVisible ? 'true' : 'false');
            gridToggleBtn.classList.toggle('active', gridVisible);
        }
        if (gridToggleLabel) {
            gridToggleLabel.textContent = gridVisible ? 'Ocultar rejilla' : 'Mostrar rejilla';
        }
    }
    if (gridToggleBtn) {
        gridToggleBtn.addEventListener('click', function () {
            gridVisible = !gridVisible;
            syncGridToggleUi();
        });
    }
    syncGridToggleUi();
    /** Suelo numérico para fwidth(r): proporcional a mundo/píxel (orto), sin valores ~1e-2 que engordan líneas. */
    infiniteGrid.onBeforeRender = function (renderer, _scene, camera) {
        if (!infiniteGrid.visible) return;
        const mat = infiniteGrid.material;
        if (!mat || !mat.uniforms || !mat.uniforms.uFwidthFloor) return;
        const el = renderer.domElement;
        const pxH = Math.max(el.height, 1);
        const pxW = Math.max(el.width, 1);
        if (camera && camera.isOrthographicCamera) {
            const worldH = (camera.top - camera.bottom) / Math.max(camera.zoom, 0.001);
            const worldW = (camera.right - camera.left) / Math.max(camera.zoom, 0.001);
            const wpp = Math.max(worldH / pxH, worldW / pxW);
            const cell = Math.min(mat.uniforms.uSize1.value, mat.uniforms.uSize2.value);
            const raw = (0.06 * wpp) / Math.max(cell, 1e-9);
            mat.uniforms.uFwidthFloor.value = THREE.MathUtils.clamp(raw, 1e-10, 2e-4);
        } else {
            mat.uniforms.uFwidthFloor.value = 1e-5;
        }
    };

    const h0 = Math.max(canvasHost.clientHeight || 380, 200);
    const w0 = Math.max(canvasHost.clientWidth || 400, 200);
    lastAspect = w0 / Math.max(h0, 1);

    function makeOrthoCamera() {
        const aspect = lastAspect;
        const hy = frustumHalfY;
        return new THREE.OrthographicCamera(-hy * aspect, hy * aspect, hy, -hy, 0.01, 500000);
    }

    const cameraOrtho = makeOrthoCamera();
    const cameraIso = makeOrthoCamera();

    /** Igual que `Design-3d-three.js`: `matrixWorldInverse` + negaciones en columna Y para `matrix3d` CSS. */
    const _vcM = new THREE.Matrix4();
    /** Escala con el cubo CSS (~90px cara vs 112px); mantiene el widget alineado en perspectiva. */
    const VC_CSS_TZ = -384;
    function vcEpsilon(value) {
        return Math.abs(value) < 1e-10 ? 0 : value;
    }
    function getCameraCssMatrix3d(matrix) {
        const el = matrix.elements;
        return (
            'matrix3d(' +
            vcEpsilon(el[0]) +
            ',' +
            vcEpsilon(-el[1]) +
            ',' +
            vcEpsilon(el[2]) +
            ',' +
            vcEpsilon(el[3]) +
            ',' +
            vcEpsilon(el[4]) +
            ',' +
            vcEpsilon(-el[5]) +
            ',' +
            vcEpsilon(el[6]) +
            ',' +
            vcEpsilon(el[7]) +
            ',' +
            vcEpsilon(el[8]) +
            ',' +
            vcEpsilon(-el[9]) +
            ',' +
            vcEpsilon(el[10]) +
            ',' +
            vcEpsilon(el[11]) +
            ',' +
            vcEpsilon(el[12]) +
            ',' +
            vcEpsilon(-el[13]) +
            ',' +
            vcEpsilon(el[14]) +
            ',' +
            vcEpsilon(el[15]) +
            ')'
        );
    }
    function setViewCubeCssFromCamera(cubeEl, camera) {
        if (!cubeEl || !camera) return;
        camera.updateWorldMatrix(true, false);
        _vcM.extractRotation(camera.matrixWorldInverse);
        cubeEl.style.transform = 'translateZ(' + VC_CSS_TZ + 'px) ' + getCameraCssMatrix3d(_vcM);
    }
    const orthoCubeEl = document.querySelector('#ma-stl-view-cube-ortho-wrap .ma-stl-vc-cube');
    const isoCubeEl = document.querySelector('#ma-stl-view-cube-iso-wrap .ma-stl-vc-cube');

    function activeCamera() {
        return activeMode === 'iso' ? cameraIso : cameraOrtho;
    }

    function applyFrustumToCamera(cam) {
        const aspect = lastAspect;
        const hy = frustumHalfY;
        cam.left = -hy * aspect;
        cam.right = hy * aspect;
        cam.top = hy;
        cam.bottom = -hy;
        cam.updateProjectionMatrix();
    }

    function applyFrustumToBoth() {
        applyFrustumToCamera(cameraOrtho);
        applyFrustumToCamera(cameraIso);
    }

    /**
     * Ortogonal: alzado / frontal (eje Z).
     * Isométrica: dirección (1,1,1) con proyección ortográfica.
     */
    function placeCamerasForModel(maxDim) {
        const d = Math.max(maxDim * 3, 1e-3);
        cameraOrtho.position.set(0, 0, d);
        cameraOrtho.up.set(0, 1, 0);
        cameraOrtho.lookAt(0, 0, 0);
        cameraOrtho.zoom = 1;

        const isoDist = Math.max(maxDim * 2.5, 1e-3);
        const dir = new THREE.Vector3(1, 1, 1).normalize().multiplyScalar(isoDist);
        cameraIso.position.copy(dir);
        cameraIso.up.set(0, 1, 0);
        cameraIso.lookAt(0, 0, 0);
        cameraIso.zoom = 1;
    }

    function bindControls(camera) {
        if (controls) {
            controls.dispose();
        }
        controls = new OrbitControls(camera, renderer.domElement);
        controls.enableDamping = true;
        controls.dampingFactor = 0.06;
        controls.target.set(0, 0, 0);
        controls.update();
    }

    const renderer = new THREE.WebGLRenderer({ antialias: true, alpha: false });
    renderer.setPixelRatio(Math.min(window.devicePixelRatio || 1, 2));
    renderer.setSize(w0, h0);
    renderer.shadowMap.enabled = true;
    renderer.shadowMap.type = THREE.PCFSoftShadowMap;
    renderer.toneMapping = THREE.ACESFilmicToneMapping;
    renderer.toneMappingExposure = 1.05;
    canvasHost.innerHTML = '';
    canvasHost.appendChild(renderer.domElement);

    const ambientLight = new THREE.AmbientLight(0xffffff, 0.34);
    scene.add(ambientLight);
    const mainDirLight = new THREE.DirectionalLight(0xffffff, 1.05);
    mainDirLight.position.set(4.5, 9, 6);
    mainDirLight.castShadow = true;
    mainDirLight.shadow.mapSize.set(2048, 2048);
    mainDirLight.shadow.camera.near = 0.2;
    mainDirLight.shadow.camera.far = 8000;
    mainDirLight.shadow.radius = 4;
    mainDirLight.shadow.bias = -0.00012;
    mainDirLight.shadow.normalBias = 0.045;
    scene.add(mainDirLight);
    const fillDirLight = new THREE.DirectionalLight(0xe2eaf8, 0.45);
    fillDirLight.position.set(-6, 2.5, -4);
    fillDirLight.castShadow = false;
    scene.add(fillDirLight);

    applyFrustumToBoth();
    placeCamerasForModel(1);
    bindControls(activeCamera());
    syncCameraRadios();
    syncViewCubesVisibility();

    const orthoCubeWrap = document.getElementById('ma-stl-view-cube-ortho-wrap');
    if (orthoCubeWrap) {
        orthoCubeWrap.addEventListener('click', function (ev) {
            const t = ev.target;
            if (!t || !t.closest) return;
            const corner = t.closest('[data-ortho-view]');
            if (corner) {
                ev.preventDefault();
                const key = corner.getAttribute('data-ortho-view');
                if (key) applyOrthoDataView(key);
                return;
            }
            const face = t.closest('[data-face]');
            if (face) {
                ev.preventDefault();
                const f = face.getAttribute('data-face');
                if (f) applyOrthoFaceToView(f);
            }
        });
    }
    const isoCubeWrap = document.getElementById('ma-stl-view-cube-iso-wrap');
    if (isoCubeWrap) {
        isoCubeWrap.querySelectorAll('[data-face]').forEach(function (el) {
            el.addEventListener('click', function () {
                const f = el.getAttribute('data-face');
                if (f) applyIsoFaceToView(f);
            });
        });
    }

    function setStatus(text) {
        if (statusEl) statusEl.textContent = text || '';
    }

    function tick() {
        requestAnimationFrame(tick);
        if (controls) controls.update();
        setViewCubeCssFromCamera(orthoCubeEl, cameraOrtho);
        setViewCubeCssFromCamera(isoCubeEl, cameraIso);
        renderer.render(scene, activeCamera());
    }
    function resizeRendererToHost() {
        const nw = Math.max(canvasHost.clientWidth || w0, 200);
        const nh = Math.max(canvasHost.clientHeight || h0, 200);
        lastAspect = nw / Math.max(nh, 1);
        applyFrustumToBoth();
        renderer.setSize(nw, nh);
    }

    const viewerShell = document.getElementById('ma-stl-viewer-shell');
    const fullscreenBtn = document.getElementById('ma-stl-fullscreen-toggle');
    function getFullscreenElement() {
        return (
            document.fullscreenElement ||
            document.webkitFullscreenElement ||
            document.mozFullScreenElement ||
            document.msFullscreenElement ||
            null
        );
    }
    function requestFullscreenFor(el) {
        if (!el) return Promise.reject();
        const fn =
            el.requestFullscreen ||
            el.webkitRequestFullscreen ||
            el.mozRequestFullScreen ||
            el.msRequestFullscreen;
        return fn ? fn.call(el) : Promise.reject();
    }
    function exitFullscreenDoc() {
        const fn =
            document.exitFullscreen ||
            document.webkitExitFullscreen ||
            document.mozCancelFullScreen ||
            document.msExitFullscreen;
        return fn ? fn.call(document) : Promise.reject();
    }
    function syncFullscreenToggleUi() {
        if (!fullscreenBtn) return;
        const active = viewerShell && getFullscreenElement() === viewerShell;
        fullscreenBtn.setAttribute('aria-pressed', active ? 'true' : 'false');
        fullscreenBtn.setAttribute('title', active ? 'Salir de pantalla completa' : 'Pantalla completa');
        const icon = fullscreenBtn.querySelector('i');
        if (icon) {
            icon.className = 'icon-base ri ' + (active ? 'ri-fullscreen-exit-line' : 'ri-fullscreen-line');
        }
    }
    if (fullscreenBtn && viewerShell) {
        fullscreenBtn.addEventListener('click', function () {
            if (getFullscreenElement() === viewerShell) {
                exitFullscreenDoc().catch(function () {});
            } else {
                requestFullscreenFor(viewerShell).catch(function () {});
            }
        });
        document.addEventListener('fullscreenchange', function () {
            syncFullscreenToggleUi();
            resizeRendererToHost();
        });
        document.addEventListener('webkitfullscreenchange', function () {
            syncFullscreenToggleUi();
            resizeRendererToHost();
        });
        syncFullscreenToggleUi();
    }

    tick();

    const ro = new ResizeObserver(function () {
        resizeRendererToHost();
    });
    ro.observe(canvasHost);

    function setCameraMode(mode) {
        activeMode = mode === 'iso' ? 'iso' : 'ortho';
        bindControls(activeCamera());
        syncCameraRadios();
        syncViewCubesVisibility();
    }

    const modeInputs = document.querySelectorAll('#master-article-stl-camera-modes input[name="ma-stl-cam-mode"]');
    modeInputs.forEach(function (inp) {
        inp.addEventListener('change', function () {
            if (!inp.checked) return;
            setCameraMode(inp.value);
        });
    });

    function refitCamerasToObject(group) {
        const box = new THREE.Box3().setFromObject(group);
        const size = box.getSize(new THREE.Vector3());
        const maxDim = Math.max(size.x, size.y, size.z, 1e-6);
        lastMaxDim = maxDim;
        frustumHalfY = maxDim * 0.55;
        lastAspect = Math.max(canvasHost.clientWidth || 400, 200) / Math.max(canvasHost.clientHeight || 200, 200);
        const gMat = infiniteGrid.material;
        if (gMat && gMat.uniforms) {
            const u = gMat.uniforms;
            /**
             * uSize = periodo en unidades mundo (línea cada uSize). Valor MAYOR → celdas más grandes → menos tupido.
             * Antes se confundió con “dividir más”: maxDim/150 da uSize pequeño y rejilla más densa.
             */
            u.uSize1.value = Math.max(maxDim / 16, 1e-6);
            u.uSize2.value = Math.max(maxDim / 4, 1e-5);
            u.uDistance.value = Math.max(maxDim * 100, 1500);
        }
        const shadowCam = mainDirLight.shadow.camera;
        const s = maxDim * 3.2;
        shadowCam.left = -s;
        shadowCam.right = s;
        shadowCam.top = s;
        shadowCam.bottom = -s;
        shadowCam.far = Math.max(maxDim * 24, 800);
        shadowCam.updateProjectionMatrix();
        applyFrustumToBoth();
        placeCamerasForModel(maxDim);
        bindControls(activeCamera());
    }

    function loadStl(url, label) {
        const myToken = ++loadToken;
        setStatus('Cargando…');
        disposeObject3D(currentRoot);
        currentRoot = null;

        const loader = new STLLoader();
        loader.load(
            url,
            function (geometry) {
                if (myToken !== loadToken) return;
                geometry.computeVertexNormals();
                geometry.center();
                const mat = new THREE.MeshStandardMaterial({
                    color: 0x5a7aa5,
                    metalness: 0.14,
                    roughness: 0.42,
                    side: THREE.DoubleSide
                });
                const mesh = new THREE.Mesh(geometry, mat);
                mesh.castShadow = true;
                mesh.receiveShadow = true;
                /* STL/CAD suele tener la planta en XY y Z como eje del edificio; en Three (Y arriba, frente +Z)
                   hay que bascular -90° en X para que FRONT sea alzado y la planta se vea con TOP. */
                mesh.rotation.x = -0.5 * Math.PI;
                const group = new THREE.Group();
                group.add(mesh);
                currentRoot = group;
                scene.add(group);
                refitCamerasToObject(group);
                setStatus(label ? 'Viendo: ' + label : 'Modelo cargado.');
            },
            undefined,
            function () {
                if (myToken !== loadToken) return;
                setStatus('No se pudo cargar el STL.');
            }
        );
    }

    document.querySelectorAll('.master-article-stl-load:not([disabled])').forEach(function (btn) {
        btn.addEventListener('click', function () {
            if (btn.disabled) return;
            const url = btn.getAttribute('data-stl-url');
            if (!url) return;
            const label = btn.getAttribute('data-slot-label') || '';
            document.querySelectorAll('.master-article-stl-load.is-active').forEach(function (b) {
                b.classList.remove('is-active');
            });
            btn.classList.add('is-active');
            loadStl(url, label);
        });
    });
}

bootMasterArticleDetailsStlViewer();
