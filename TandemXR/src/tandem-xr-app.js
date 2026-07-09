import * as THREE from 'three';
import { OrbitControls } from 'three/examples/jsm/controls/OrbitControls.js';
import { STLLoader } from 'three/examples/jsm/loaders/STLLoader.js';
import { VRButton } from 'three/examples/jsm/webxr/VRButton.js';
import { ARButton } from 'three/examples/jsm/webxr/ARButton.js';
import { XRControllerModelFactory } from 'three/examples/jsm/webxr/XRControllerModelFactory.js';

const DEFAULT_STL = '/desing-stl/Content/DesignTools/Stl/ATK60/3120270090P.stl';
/** STL de Desing suelen venir en metros; escena Three.js en metros → ×1000 como Desing_2. */
const DEFAULT_STL_SCALE = 1000;

/**
 * Resuelve URL del STL desde query string (integración con Desing).
 * Ej.: ?stl=/Content/DesignTools/Stl/ATK60/foo.stl&scale=1000
 */
function resolveStlUrl() {
  const params = new URLSearchParams(window.location.search);
  const raw = params.get('stl') || params.get('stlUrl');
  if (!raw) return DEFAULT_STL;

  const trimmed = raw.trim().replace(/\\/g, '/');
  if (trimmed.startsWith('http://') || trimmed.startsWith('https://')) {
    return trimmed;
  }

  const path = trimmed.startsWith('/') ? trimmed : `/${trimmed}`;
  if (path.toLowerCase().includes('..')) {
    console.warn('[TandemXR] Ruta STL rechazada (traversal).');
    return DEFAULT_STL;
  }

  return `/desing-stl${path}`;
}

function resolveStlScale() {
  const params = new URLSearchParams(window.location.search);
  const raw = params.get('scale');
  if (raw == null || raw === '') return DEFAULT_STL_SCALE;
  const n = Number(raw);
  return Number.isFinite(n) && n > 0 ? n : DEFAULT_STL_SCALE;
}

function fitGroupToGround(group) {
  const box = new THREE.Box3().setFromObject(group);
  if (box.isEmpty()) return;

  const size = box.getSize(new THREE.Vector3());
  const center = box.getCenter(new THREE.Vector3());

  group.position.x -= center.x;
  group.position.z -= center.z;
  group.position.y -= box.min.y;

  const maxDim = Math.max(size.x, size.y, size.z, 1);
  return { size, maxDim };
}

export class TandemXrApp {
  constructor(canvas, hud) {
    this.canvas = canvas;
    this.hud = hud;
    this.stlUrl = resolveStlUrl();
    this.stlScale = resolveStlScale();

    this.scene = new THREE.Scene();
    this.scene.background = new THREE.Color(0xb8d4f0);

    this.camera = new THREE.PerspectiveCamera(55, 1, 0.01, 500);
    this.camera.position.set(2.2, 1.6, 2.2);

    this.renderer = new THREE.WebGLRenderer({
      canvas,
      antialias: true,
      alpha: true
    });
    this.renderer.setPixelRatio(Math.min(window.devicePixelRatio, 2));
    this.renderer.outputColorSpace = THREE.SRGBColorSpace;
    this.renderer.xr.enabled = true;

    this.controls = new OrbitControls(this.camera, canvas);
    this.controls.enableDamping = true;
    this.controls.target.set(0, 0.8, 0);

    this.modelRoot = new THREE.Group();
    this.modelRoot.name = 'TandemXRModelRoot';
    this.scene.add(this.modelRoot);

    this.arPlaced = false;
    this._buildEnvironment();
    this._buildArReticle();
    this._buildLights();
    this._bindXrUi();
    this._bindResize();
    this._loadModel();
    this._animate();
  }

  _buildEnvironment() {
    const grid = new THREE.GridHelper(12, 48, 0x0e7490, 0x67e8f9);
    grid.position.y = 0;
    grid.name = 'GroundGrid';
    this.scene.add(grid);

    const floor = new THREE.Mesh(
      new THREE.PlaneGeometry(40, 40),
      new THREE.MeshStandardMaterial({
        color: 0xdbeafe,
        roughness: 0.95,
        metalness: 0
      })
    );
    floor.rotation.x = -Math.PI / 2;
    floor.receiveShadow = true;
    floor.name = 'GroundPlane';
    this.scene.add(floor);
  }

  _buildArReticle() {
    const ring = new THREE.RingGeometry(0.12, 0.16, 32).rotateX(-Math.PI / 2);
    const dot = new THREE.CircleGeometry(0.035, 24).rotateX(-Math.PI / 2);
    const mat = new THREE.MeshBasicMaterial({ color: 0x22d3ee, transparent: true, opacity: 0.9 });
    const mesh = new THREE.Mesh(ring, mat);
    mesh.add(new THREE.Mesh(dot, mat.clone()));
    mesh.matrixAutoUpdate = false;
    mesh.visible = false;
    mesh.name = 'ArReticle';
    this.reticle = mesh;
    this.scene.add(mesh);
  }

  _buildLights() {
    this.scene.add(new THREE.HemisphereLight(0xffffff, 0x444444, 1.1));
    const sun = new THREE.DirectionalLight(0xffffff, 1.2);
    sun.position.set(4, 8, 3);
    sun.castShadow = true;
    this.scene.add(sun);
  }

  _bindXrUi() {
    const container = document.getElementById('txr-xr-buttons');
    if (!container) return;

    const vrBtn = VRButton.createButton(this.renderer);
    vrBtn.classList.add('txr-vr-btn');
    container.appendChild(vrBtn);

    const arBtn = ARButton.createButton(this.renderer, {
      requiredFeatures: ['hit-test'],
      optionalFeatures: ['dom-overlay', 'local-floor', 'light-estimation']
    });
    arBtn.classList.add('txr-ar-btn');
    container.appendChild(arBtn);

    this.renderer.xr.addEventListener('sessionstart', () => this._onXrSessionStart());
    this.renderer.xr.addEventListener('sessionend', () => this._onXrSessionEnd());

    const factory = new XRControllerModelFactory();
    this.controller0 = this.renderer.xr.getController(0);
    this.controller1 = this.renderer.xr.getController(1);
    this.controller0.add(factory.createControllerModel(this.controller0));
    this.controller1.add(factory.createControllerModel(this.controller1));
    this.scene.add(this.controller0, this.controller1);
  }

  _onXrSessionStart() {
    const session = this.renderer.xr.getSession();
    const mode = session?.mode || 'inline';
    this.controls.enabled = false;

    if (mode === 'immersive-ar') {
      this._setHud('Modo AR — apunta al suelo y toca para colocar', 'El modelo se ancla en el plano detectado.');
      this._setupHitTest(session);
      this.modelRoot.visible = this.arPlaced;
      this.scene.getObjectByName('GroundGrid').visible = false;
      this.scene.getObjectByName('GroundPlane').visible = false;
    } else if (mode === 'immersive-vr') {
      this._setHud('Modo VR — escala real', 'Camina alrededor del montaje para validar encajes e interferencias.');
      this.modelRoot.visible = true;
      this.reticle.visible = false;
    } else {
      this._setHud('Sesión XR activa', '');
    }
  }

  _onXrSessionEnd() {
    this.controls.enabled = true;
    this.hitTestSource = null;
    this.hitTestSourceRequested = false;
    this.reticle.visible = false;
    this.arPlaced = false;
    this.modelRoot.visible = true;
    this.modelRoot.position.set(0, 0, 0);
    this.scene.getObjectByName('GroundGrid').visible = true;
    this.scene.getObjectByName('GroundPlane').visible = true;
    this._setHud('Modo escritorio', 'Orbita con ratón. En dispositivo compatible usa AR o VR.');
  }

  async _setupHitTest(session) {
    if (!session || this.hitTestSourceRequested) return;
    this.hitTestSourceRequested = true;

    const viewerSpace = await session.requestReferenceSpace('viewer');
    const hitTestSource = await session.requestHitTestSource({ space: viewerSpace });
    this.hitTestSource = hitTestSource;
    this.viewerSpace = viewerSpace;

    const onSelect = () => {
      if (!this.reticle.visible || this.arPlaced) return;
      this.modelRoot.position.setFromMatrixPosition(this.reticle.matrix);
      this.modelRoot.visible = true;
      this.arPlaced = true;
      this._setHud('Modelo colocado en obra', 'Mueve el dispositivo para inspeccionar. Sal de AR para reiniciar.');
    };

    session.addEventListener('select', onSelect);
    this._arSelectHandler = onSelect;
  }

  _setHud(mode, hint) {
    const modeEl = document.getElementById('txr-mode-label');
    const hintEl = document.getElementById('txr-hint');
    if (modeEl) modeEl.textContent = mode;
    if (hintEl) hintEl.textContent = hint;
  }

  async _loadModel() {
    const label = document.getElementById('txr-model-label');
    if (label) label.textContent = `STL: ${this.stlUrl}`;

    const loader = new STLLoader();
    try {
      const geometry = await loader.loadAsync(this.stlUrl);
      geometry.computeVertexNormals();
      geometry.center();

      const material = new THREE.MeshStandardMaterial({
        color: 0xfacc15,
        metalness: 0.15,
        roughness: 0.55,
        side: THREE.DoubleSide
      });

      const mesh = new THREE.Mesh(geometry, material);
      mesh.castShadow = true;
      mesh.receiveShadow = true;

      this.modelRoot.clear();
      this.modelRoot.add(mesh);
      this.modelRoot.scale.setScalar(this.stlScale);

      const fit = fitGroupToGround(this.modelRoot);
      if (fit) {
        const dist = Math.max(fit.maxDim * 1.8, 1.5);
        this.camera.position.set(dist, dist * 0.65, dist);
        this.controls.target.set(0, fit.size.y * 0.35, 0);
        this.controls.update();
      }
    } catch (err) {
      console.error('[TandemXR] Error cargando STL', err);
      if (label) {
        label.textContent = `Error STL (¿Desing en marcha?): ${this.stlUrl}`;
      }
      this._setHud('Sin modelo', 'Arranca Desing (IIS Express) o pasa ?stl= con ruta bajo /Content/DesignTools/Stl/.');
    }
  }

  _bindResize() {
    const resize = () => {
      const parent = this.canvas.parentElement;
      const w = parent?.clientWidth || window.innerWidth;
      const h = parent?.clientHeight || window.innerHeight - 120;
      this.camera.aspect = w / Math.max(h, 1);
      this.camera.updateProjectionMatrix();
      this.renderer.setSize(w, h, false);
    };
    resize();
    window.addEventListener('resize', resize);
  }

  _updateArHitTest(frame) {
    if (!this.hitTestSource || !this.viewerSpace) return;

    const hits = frame.getHitTestResults(this.hitTestSource);
    if (hits.length === 0) {
      this.reticle.visible = false;
      return;
    }

    const pose = hits[0].getPose(this.viewerSpace);
    this.reticle.visible = true;
    this.reticle.matrix.fromArray(pose.transform.matrix);
  }

  _animate() {
    this.renderer.setAnimationLoop((time, frame) => {
      if (frame) {
        this._updateArHitTest(frame);
      } else {
        this.controls.update();
      }
      this.renderer.render(this.scene, this.camera);
    });
  }
}
