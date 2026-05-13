import * as THREE from 'three';
import { OrbitControls } from '@masterarticles/OrbitControls';
import { STLLoader } from '@masterarticles/STLLoader';
import { InfiniteGridHelper } from '@masterarticles/InfiniteGridHelper';

/** Zenith → horizon gradient as `scene.background` (same module Three as import map `three.module.js`). */
function createMasterArticleStlSkyBackgroundTexture() {
    const w = 2;
    const h = 256;
    const canvas = document.createElement('canvas');
    canvas.width = w;
    canvas.height = h;
    const ctx = canvas.getContext('2d');
    const grd = ctx.createLinearGradient(0, 0, 0, h);
    /* Narrow blue band at top; ~lower 55–60% reads as near-white horizon / suelo. */
    grd.addColorStop(0, '#5e9fd4');
    grd.addColorStop(0.14, '#7eb8ea');
    grd.addColorStop(0.28, '#a8cef0');
    grd.addColorStop(0.4, '#dceaf7');
    grd.addColorStop(0.52, '#f0f6fb');
    grd.addColorStop(0.62, '#f5f8fc');
    grd.addColorStop(0.78, '#fafcfd');
    grd.addColorStop(1, '#ffffff');
    ctx.fillStyle = grd;
    ctx.fillRect(0, 0, w, h);
    const tex = new THREE.CanvasTexture(canvas);
    tex.colorSpace = THREE.SRGBColorSpace;
    tex.minFilter = THREE.LinearFilter;
    tex.magFilter = THREE.LinearFilter;
    tex.generateMipmaps = false;
    return tex;
}

/** Horizon tone aligned with gradient bottom (clear color / no fog). */
const MA_STL_SKY_HORIZON_HEX = 0xffffff;
const MA_STL_SKY_OFF_HEX = 0xffffff;

/** World XYZ at origin; length refit in `refitCamerasToObject` from `lastMaxDim`. */
const MA_STL_AXES_VISIBLE = true;
const MA_STL_AXES_OPACITY = 0.42;

/**
 * Discrete world axes length (model units): ~20% of span (2× prior 0.1), caps doubled.
 * axisLength = clamp(maxDim * 0.2, 0.3, 1.0)  // was clamp(maxDim * 0.1, 0.15, 0.5)
 */
function masterArticleStlWorldAxesLength(maxDim) {
    const d = Math.max(maxDim, 1e-9);
    return THREE.MathUtils.clamp(d * 0.2, 0.3, 1.0);
}

function applyMasterArticleStlAxesStyle(axesRoot) {
    if (!axesRoot) return;
    axesRoot.traverse(function (obj) {
        obj.raycast = function () {};
        if (obj.material) {
            const mats = Array.isArray(obj.material) ? obj.material : [obj.material];
            mats.forEach(function (m) {
                if (m) {
                    m.transparent = true;
                    m.opacity = MA_STL_AXES_OPACITY;
                }
            });
        }
    });
}

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
    /** Recorte local (planos mundo): barra vertical → corte Y; horizontal → X. Valor 0–1000 en UI; fracción f = (1000−v)/1000 ∈ [0,1]: f=0 sin recorte, f=1 máximo. */
    const clipBounds = { min: new THREE.Vector3(), max: new THREE.Vector3() };
    const clipPlaneY = new THREE.Plane();
    const clipPlaneX = new THREE.Plane();
    /** @type {THREE.Mesh | null} */
    let clipStlMesh = null;
    const clipInputY = document.getElementById('ma-stl-clip-y');
    const clipInputX = document.getElementById('ma-stl-clip-x');
    const clipControlsEl = document.getElementById('ma-stl-clip-controls');
    const clipToggleBtn = document.getElementById('ma-stl-clip-toggle');
    const clipCanvasEl = document.getElementById('master-article-details-stl-viewer-canvas');
    let clipUiVisible = false;

    function syncClipToggleUi() {
        const visible = clipUiVisible;
        if (clipControlsEl) {
            clipControlsEl.classList.toggle('d-none', !visible);
            clipControlsEl.setAttribute('aria-hidden', visible ? 'false' : 'true');
        }
        if (clipCanvasEl) {
            clipCanvasEl.classList.toggle('ma-stl-canvas--clips-ui-visible', visible);
        }
        if (clipToggleBtn) {
            clipToggleBtn.setAttribute('aria-pressed', visible ? 'true' : 'false');
            clipToggleBtn.classList.toggle('active', visible);
            clipToggleBtn.setAttribute('title', visible ? 'Ocultar controles de corte' : 'Mostrar u ocultar cortes');
        }
    }
    if (clipToggleBtn && clipControlsEl) {
        clipToggleBtn.addEventListener('click', function () {
            clipUiVisible = !clipUiVisible;
            syncClipToggleUi();
        });
    }
    syncClipToggleUi();

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
    const skyBackgroundTexture = createMasterArticleStlSkyBackgroundTexture();
    const skyOffBackground = new THREE.Color(MA_STL_SKY_OFF_HEX);
    const skyToggleBtn = document.getElementById('ma-stl-sky-toggle');
    const groundShadowToggleBtn = document.getElementById('ma-stl-ground-shadow-toggle');
    const darkBgToggleBtn = document.getElementById('ma-stl-dark-bg-toggle');
    let skyVisible = false;
    let darkBgVisible = false;
    scene.background = skyOffBackground;

    /* Rejilla: cian ligeramente más legible sobre blanco / suelo (ver infinite-grid-helper uOpacityMax). */
    const infiniteGrid = new InfiniteGridHelper(8, 32, new THREE.Color(0x00b8dc), 500, 2.55, 0.56);
    scene.add(infiniteGrid);

    /** Origen mundo (0,0,0): ejes discretos; una sola vez en `scene`, escala en `refitCamerasToObject`. */
    const worldAxesHelper = new THREE.AxesHelper(1);
    worldAxesHelper.position.set(0, 0, 0);
    worldAxesHelper.visible = MA_STL_AXES_VISIBLE;
    applyMasterArticleStlAxesStyle(worldAxesHelper);
    scene.add(worldAxesHelper);
    worldAxesHelper.scale.setScalar(masterArticleStlWorldAxesLength(lastMaxDim));

    /** Plano “suelo” bajo la rejilla solo con cielo: refuerza lectura de suelo sin afectar cielo apagado. */
    const skyFloorGeometry = new THREE.PlaneGeometry(1, 1);
    const skyFloorMaterial = new THREE.MeshBasicMaterial({
        color: 0xfafafa,
        transparent: true,
        opacity: 0.85,
        depthWrite: false,
        toneMapped: false
    });
    const skyFloorPlane = new THREE.Mesh(skyFloorGeometry, skyFloorMaterial);
    skyFloorPlane.rotation.x = -0.5 * Math.PI;
    skyFloorPlane.renderOrder = -20;
    skyFloorPlane.visible = false;
    scene.add(skyFloorPlane);
    (function initSkyFloorExtent() {
        const span = Math.max(lastMaxDim * 140, 2500);
        skyFloorPlane.scale.set(span, span, 1);
        skyFloorPlane.position.set(0, -Math.max(lastMaxDim * 0.018, 5e-4), 0);
    })();

    /** Plano receptor de sombras en Y=0 (solo con “sombra en suelo” activa). */
    const shadowGroundGeometry = new THREE.PlaneGeometry(1, 1);
    const shadowGroundMaterial = new THREE.ShadowMaterial({ opacity: 0.42 });
    const shadowGroundPlane = new THREE.Mesh(shadowGroundGeometry, shadowGroundMaterial);
    shadowGroundPlane.rotation.x = -0.5 * Math.PI;
    shadowGroundPlane.position.set(0, 0, 0);
    shadowGroundPlane.receiveShadow = true;
    shadowGroundPlane.visible = false;
    shadowGroundPlane.renderOrder = -15;
    scene.add(shadowGroundPlane);
    (function initShadowGroundExtent() {
        const span = Math.max(lastMaxDim * 140, 2500);
        shadowGroundPlane.scale.set(span, span, 1);
    })();
    let groundShadowVisible = false;

    let gridVisible = false;
    const gridToggleBtn = document.getElementById('ma-stl-grid-toggle');
    function syncGridToggleUi() {
        infiniteGrid.visible = gridVisible;
        if (gridToggleBtn) {
            gridToggleBtn.setAttribute('aria-pressed', gridVisible ? 'true' : 'false');
            gridToggleBtn.classList.toggle('active', gridVisible);
            gridToggleBtn.setAttribute('title', gridVisible ? 'Ocultar rejilla de fondo' : 'Mostrar rejilla de fondo');
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
    renderer.setClearColor(MA_STL_SKY_OFF_HEX, 1);
    renderer.setPixelRatio(Math.min(window.devicePixelRatio || 1, 2));
    renderer.setSize(w0, h0);
    renderer.shadowMap.enabled = false;
    renderer.shadowMap.type = THREE.PCFSoftShadowMap;
    renderer.toneMapping = THREE.ACESFilmicToneMapping;
    renderer.toneMappingExposure = 1.05;
    renderer.localClippingEnabled = true;
    canvasHost.innerHTML = '';
    canvasHost.appendChild(renderer.domElement);

    /** Raycast desde la cámara activa para fijar el pivote de órbita bajo el cursor (comportamiento CAD). */
    const orbitPivotRaycaster = new THREE.Raycaster();
    const orbitPivotNdc = new THREE.Vector2();

    /**
     * Misma lógica que OrbitControls `onMouseDown`: solo cuando el gesto va a ROTAR (no pan/dolly con el mismo botón).
     * Ratón/pen; touch queda con el comportamiento por defecto de OrbitControls.
     */
    function stlOrbitPointerDownWillRotate(ev) {
        if (!controls || controls.enabled === false || !controls.enableRotate) return false;
        if ((ev.pointerType !== 'mouse' && ev.pointerType !== 'pen') || ev.button !== 0) return false;
        const MOUSE = THREE.MOUSE;
        const leftAction = controls.mouseButtons.LEFT;
        const mod = ev.ctrlKey || ev.metaKey || ev.shiftKey;
        if (leftAction === MOUSE.ROTATE) {
            return !mod;
        }
        if (leftAction === MOUSE.PAN) {
            return mod;
        }
        return false;
    }

    /**
     * Antes de que OrbitControls procese el pointerdown (fase capture): pivote = impacto en el grupo del STL.
     * Sin impacto: no se modifica `controls.target` (mantiene último pivote o el fijado por el cubo de vistas en origen).
     * Perspectiva: minDistance/maxDistance de OrbitControls siguen aplicando; aquí solo hay cámaras ortográficas (minZoom/maxZoom por defecto).
     */
    function onCanvasPointerDownSetOrbitPivot(ev) {
        if (!stlOrbitPointerDownWillRotate(ev)) return;
        const canvas = renderer.domElement;
        if (ev.currentTarget !== canvas) return;
        const rawTarget = ev.target;
        if (rawTarget && typeof rawTarget.closest === 'function') {
            if (rawTarget.closest('button, input, select, textarea, [role="button"], label')) return;
        }
        if (!currentRoot || !controls) return;
        const rect = canvas.getBoundingClientRect();
        const rw = Math.max(rect.width, 1);
        const rh = Math.max(rect.height, 1);
        orbitPivotNdc.x = ((ev.clientX - rect.left) / rw) * 2 - 1;
        orbitPivotNdc.y = -((ev.clientY - rect.top) / rh) * 2 + 1;
        orbitPivotRaycaster.setFromCamera(orbitPivotNdc, activeCamera());
        const hits = orbitPivotRaycaster.intersectObject(currentRoot, true);
        if (hits.length > 0) {
            controls.target.copy(hits[0].point);
        }
        controls.update();
    }

    renderer.domElement.addEventListener('pointerdown', onCanvasPointerDownSetOrbitPivot, true);

    function applySceneBackgroundAndClearColor() {
        if (darkBgVisible) {
            scene.background = new THREE.Color(0x000000);
            renderer.setClearColor(0x000000, 1);
        } else if (skyVisible) {
            scene.background = skyBackgroundTexture;
            renderer.setClearColor(MA_STL_SKY_HORIZON_HEX, 1);
        } else {
            scene.background = skyOffBackground;
            renderer.setClearColor(MA_STL_SKY_OFF_HEX, 1);
        }
        skyFloorPlane.visible = skyVisible && !darkBgVisible;
    }

    function syncSkyToggleUi() {
        applySceneBackgroundAndClearColor();
        if (skyToggleBtn) {
            skyToggleBtn.setAttribute('aria-pressed', skyVisible ? 'true' : 'false');
            skyToggleBtn.classList.toggle('active', skyVisible);
            skyToggleBtn.setAttribute('title', skyVisible ? 'Ocultar cielo' : 'Mostrar u ocultar cielo');
        }
    }
    if (skyToggleBtn) {
        skyToggleBtn.addEventListener('click', function () {
            skyVisible = !skyVisible;
            syncSkyToggleUi();
        });
    }
    syncSkyToggleUi();

    function syncDarkBgToggleUi() {
        applySceneBackgroundAndClearColor();
        if (darkBgToggleBtn) {
            darkBgToggleBtn.setAttribute('aria-pressed', darkBgVisible ? 'true' : 'false');
            darkBgToggleBtn.classList.toggle('active', darkBgVisible);
            darkBgToggleBtn.setAttribute('title', darkBgVisible ? 'Desactivar fondo negro' : 'Activar fondo negro');
        }
    }
    if (darkBgToggleBtn) {
        darkBgToggleBtn.addEventListener('click', function () {
            darkBgVisible = !darkBgVisible;
            syncDarkBgToggleUi();
        });
    }
    syncDarkBgToggleUi();

    const ambientLight = new THREE.AmbientLight(0xffffff, 0.34);
    scene.add(ambientLight);
    const mainDirLight = new THREE.DirectionalLight(0xffffff, 1.05);
    mainDirLight.position.set(4.5, 9, 6);
    mainDirLight.castShadow = false;
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

    function syncGroundShadowToggleUi() {
        shadowGroundPlane.visible = groundShadowVisible;
        mainDirLight.castShadow = groundShadowVisible;
        renderer.shadowMap.enabled = groundShadowVisible;
        if (clipStlMesh) {
            clipStlMesh.castShadow = groundShadowVisible;
            clipStlMesh.receiveShadow = groundShadowVisible;
        }
        if (groundShadowToggleBtn) {
            groundShadowToggleBtn.setAttribute('aria-pressed', groundShadowVisible ? 'true' : 'false');
            groundShadowToggleBtn.classList.toggle('active', groundShadowVisible);
            groundShadowToggleBtn.setAttribute(
                'title',
                groundShadowVisible ? 'Ocultar sombra en el suelo' : 'Mostrar sombra en el suelo'
            );
        }
    }
    if (groundShadowToggleBtn) {
        groundShadowToggleBtn.addEventListener('click', function () {
            groundShadowVisible = !groundShadowVisible;
            syncGroundShadowToggleUi();
        });
    }
    syncGroundShadowToggleUi();

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

    function clipFractionFromSlider(inputEl) {
        if (!inputEl) return 0;
        const vRaw = Number.parseFloat(String(inputEl.value).trim());
        const v = Number.isFinite(vRaw) ? vRaw : 1000;
        return THREE.MathUtils.clamp((1000 - v) / 1000, 0, 1);
    }

    function updateClipPlanes() {
        if (!clipStlMesh || !clipStlMesh.material) return;
        const min = clipBounds.min;
        const max = clipBounds.max;
        const pad = Math.max(lastMaxDim * 0.02, 1e-6);
        const h = max.y - min.y + 2 * pad;
        const w = max.x - min.x + 2 * pad;
        const fY = clipFractionFromSlider(clipInputY);
        const fX = clipFractionFromSlider(clipInputX);
        const cutY = max.y + pad - fY * h;
        const cutX = max.x + pad - fX * w;
        /* Three.js descarta si n·p + d < 0: (0,-1,0,cutY) descarta y > cutY → recorte desde arriba al subir fY. */
        clipPlaneY.setComponents(0, -1, 0, cutY);
        /* (-1,0,0,cutX) descarta x > cutX → recorte desde +X (derecha) al subir fX. */
        clipPlaneX.setComponents(-1, 0, 0, cutX);
        clipStlMesh.material.clippingPlanes = [clipPlaneY, clipPlaneX];
    }

    if (clipInputY) clipInputY.addEventListener('input', updateClipPlanes);
    if (clipInputX) clipInputX.addEventListener('input', updateClipPlanes);

    function refitCamerasToObject(group) {
        const groundY = 0;
        group.updateMatrixWorld(true);
        let box = new THREE.Box3().setFromObject(group);
        const sizePre = box.getSize(new THREE.Vector3());
        const maxDimPre = Math.max(sizePre.x, sizePre.y, sizePre.z, 1e-6);
        /* Rejilla en Y=0 (InfiniteGridHelper): sin esto, geometry.center() + rotación deja el AABB simétrico en Y y la mitad queda bajo el plano. */
        const epsilon = Math.max(maxDimPre * 1e-6, 1e-9);
        const dy = groundY + epsilon - box.min.y;
        if (Math.abs(dy) > 1e-12) {
            group.position.y += dy;
            group.updateMatrixWorld(true);
            box = new THREE.Box3().setFromObject(group);
        }
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
        const floorSpan = Math.max(maxDim * 140, 2500);
        skyFloorPlane.scale.set(floorSpan, floorSpan, 1);
        skyFloorPlane.position.set(0, -Math.max(maxDim * 0.018, 5e-4), 0);
        shadowGroundPlane.scale.set(floorSpan, floorSpan, 1);
        shadowGroundPlane.position.set(0, 0, 0);
        clipBounds.min.copy(box.min);
        clipBounds.max.copy(box.max);
        updateClipPlanes();
        const axisLen = masterArticleStlWorldAxesLength(maxDim);
        worldAxesHelper.scale.setScalar(axisLen);
        applyFrustumToBoth();
        placeCamerasForModel(maxDim);
        bindControls(activeCamera());
    }

    function loadStl(url, label) {
        const myToken = ++loadToken;
        setStatus('Cargando…');
        clipStlMesh = null;
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
                    side: THREE.DoubleSide,
                    clippingPlanes: [clipPlaneY, clipPlaneX],
                    clipShadows: true
                });
                const mesh = new THREE.Mesh(geometry, mat);
                mesh.castShadow = groundShadowVisible;
                mesh.receiveShadow = groundShadowVisible;
                /* STL/CAD suele tener la planta en XY y Z como eje del edificio; en Three (Y arriba, frente +Z)
                   hay que bascular -90° en X para que FRONT sea alzado y la planta se vea con TOP. */
                mesh.rotation.x = -0.5 * Math.PI;
                const group = new THREE.Group();
                group.add(mesh);
                clipStlMesh = mesh;
                if (clipInputY) clipInputY.value = '1000';
                if (clipInputX) clipInputX.value = '1000';
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
