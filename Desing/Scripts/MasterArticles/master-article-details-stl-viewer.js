import * as THREE from 'three';
import { OrbitControls } from '@masterarticles/OrbitControls';
import { STLLoader } from '@masterarticles/STLLoader';
import { InfiniteGridHelper } from '@masterarticles/InfiniteGridHelper';
import { Line2 } from '../Design/jsm/lines/Line2.js';
import { LineGeometry } from '../Design/jsm/lines/LineGeometry.js';
import { LineMaterial } from '../Design/jsm/lines/LineMaterial.js';

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

/**
 * Solo visor Desing_2 (`data-ma-stl-show-rulers-toggle`): después de aplicar escala STL (vértices en unidad de archivo
 * → metro vía {@link maStlVertexUnitsToMetersScale}) se multiplica por este factor para obtener **coordenadas de escena**.
 * **`MA_STL_SCENE_MM_PER_PHYSICAL_METER` mm de escena ≡ 1 m físico** (p. ej. rejilla cada 500 mm = cada 0,5 m físicos).
 * Los `.stl` no declaran SI de forma fiable → `data-ma-stl-source-units` / `data-ma-stl-unit-to-meters`; muchos están en mm.
 *
 * Pantalla maestro (sin esos `data-*`): **sin** este factor (`group.scale`=1 como antes): unidades arbitrarias archivo.
 */
const MA_STL_SCENE_MM_PER_PHYSICAL_METER = 1000;
/** Cotas editables herramienta línea: máximo de decimales en **metros** al formatear y al commit. */
const MA_STL_DESING2_DIM_EDITABLE_METERS_DECIMALS = 3;

/** Reglas (Desing_2): pasos en milímetros de escena. Minores 500 mm (0,5 m); mayores cada 2500 mm (2,5 m). */
const MA_STL_DESING2_RULE_MINOR_MM = 500;
const MA_STL_DESING2_RULE_MAJOR_MM = 2500;

/** Radio (mm escena) de la esfera roja en el cruce de reglas en el anclaje; siempre visible aunque se oculten las reglas. */
const MA_STL_DESING2_RULE_ANCHOR_BALL_RADIUS_MM = 91;

/** Color de líneas de reglas con fondo claro (opacity vía {@link maStlApplyDesing2RulerLineMaterialTheme}). */
const MA_STL_DESING2_RULE_LINE_LIGHT_HEX = 0x808080;

/** Color de relleno de las etiquetas numéricas de reglas (sprites canvas `thinFillOnly`). */
const MA_STL_DESING2_RULE_LABEL_LIGHT_FILL = '#333333';
const MA_STL_DESING2_RULE_LABEL_DARK_FILL = '#ffffff';

/** Extensión fija desde origen (≈25 m físicos convertidos a mm escena). */
const MA_STL_DESING2_RULE_FIXED_EXTENT_MM = 25 * MA_STL_SCENE_MM_PER_PHYSICAL_METER;

/** Rejilla alineada con reglas cuando `maStlRulersGate` (base LOD; puede duplicarse al alejar). */
const MA_STL_DESING2_GRID_MINOR_MM = 500;
const MA_STL_DESING2_GRID_MAJOR_MM = 2500;

/** Suelo workspace Desing_2 (mm escena): sombra, reglas, rejilla `uPlaneY` y apoyo del STL. */
const MA_STL_DESING2_WORKSPACE_FLOOR_Y_MM = 0;

/** Máximo recorrido de reglas por eje cuando no modo fijo (≈50 m físicos → mm escena); hoy sólo aplicable si se mostraran reglas. */
const MA_STL_DESING2_RULE_EXTENT_CAP_MM = 50 * MA_STL_SCENE_MM_PER_PHYSICAL_METER;

/** Encuadre ortográfico inicial sin STL cargado (~12 m físicos en mm escena). */
const MA_STL_DESING2_EMPTY_BASELINE_MM = MA_STL_DESING2_GRID_MAJOR_MM * 12;

/**
 * Rejilla Desing_2: mitad diagonal visible × pad (refit orto o perspectiva en `onBeforeRender`).
 * Con perspectiva (Design-3d) el quad sigue a la cámara; con orto legacy usa zoom mínimo real.
 */
const MA_STL_DESING2_GRID_REFIT_DISTANCE_PAD = 3.5;

/**
 * Altura visible de referencia para alcance de rejilla (mm escena): `maStlDesing2MinZoomFromHalfY` ≈ 2·hy / esto.
 * OrbitControls usa {@link MA_STL_DESING2_MIN_ZOOM_FLOOR} (zoom libre); legibilidad al alejar vía shader.
 */
const MA_STL_DESING2_MAX_VISIBLE_HEIGHT_MM = 14 * MA_STL_SCENE_MM_PER_PHYSICAL_METER;

/** Zoom orto mínimo en OrbitControls (alejar libre); la rejilla usa `maStlDesing2MinZoomFromHalfY` solo para `uDistance`. */
const MA_STL_DESING2_MIN_ZOOM_FLOOR = 0.2;

/**
 * Desing_2 pick-lock (línea / anclaje): si `controls.target` se aleja más de esto vs. el target al cerrar órbita,
 * hubo pan (no sólo zoom) → no forzar {@link maStlApplyRulerAnchorOrbitPivotPreserveView} al desbloquear ni al primer rotate.
 */
const MA_STL_DESING2_PICK_ORBIT_PAN_DETECTION_EPS_MM = 0.5;

/** Consola útil: rejilla / raycast pick (`maStlRaycastClipStlMeshFirst`, `maStlUpdateGridIntersectionPickHover`). */
/** Relleno snap intersección rejilla 500 mm (modo pick reglas Desing_2). */
const MA_STL_GRID_INTERSECTION_PICK_HIGHLIGHT_COLOR = 0x00e676;
const MA_STL_GRID_INTERSECTION_PICK_HIGHLIGHT_OPACITY = 0.65;
/** Contorno idle en cruce más cercano (antes de “conectar”). */
const MA_STL_GRID_INTERSECTION_PICK_IDLE_COLOR = 0x26c6da;
const MA_STL_GRID_INTERSECTION_PICK_IDLE_OPACITY = 0.92;
/** Umbral pantalla (px) para estado connected en cruce rejilla vs. cruce cercano idle. */
const MA_STL_GRID_INTERSECTION_PICK_SCREEN_PX_BASE = 52;
/** Suma opcional al umbral px sólo para herramienta línea (snap más tolerante sin afectar anclaje reglas). */
const MA_STL_LINE_TOOL_GRID_PICK_SCREEN_PX_BOOST = 18;
/** Esfera snap vértice / punto medio de línea usuario existente (herramienta línea). */
const MA_STL_LINE_TOOL_VERTEX_SNAP_BALL_RADIUS_MM = 240;
const MA_STL_LINE_TOOL_VERTEX_SNAP_COLOR = 0x00e5ff;
const MA_STL_LINE_TOOL_VERTEX_SNAP_COLOR_ACTIVE = 0x00e676;
const MA_STL_LINE_TOOL_VERTEX_SNAP_OPACITY_IDLE = 0.5;
const MA_STL_LINE_TOOL_VERTEX_SNAP_OPACITY_ACTIVE = 0.5;
/** Factor × paso rejilla Entorno para proximidad XZ cursor→vértice (complementa umbral px). */
const MA_STL_LINE_TOOL_VERTEX_SNAP_WORLD_MM_FACTOR = 0.52;
/** Longitud mínima en planta (XZ) para tomar vector P1→cursor como dirección de caucho / longitud tecleada. */
const MA_STL_LINE_TOOL_DIR_EPS_MM = MA_STL_DESING2_GRID_MINOR_MM * 0.001;
/** Ortográfico / snap 15° en planta XZ: ángulo 0° según atan2(Z,X) → eje mundo **+X**; cada +90° hacia **+Z**. */
const MA_STL_LINE_TOOL_ORTHO15_STEP_RAD = Math.PI / 12;
/** Celda visual hover (ligeramente menor que minor 500 mm). */
const MA_STL_GRID_INTERSECTION_PICK_CELL_MM = 480;
const MA_STL_DESING2_GRID_INTERSECTION_FLOOR_EPS_MM = 0.35;
/** Hover STL en modo pick: emissive / tint azul (solo temporal; se restaura al salir). */
const MA_STL_PICK_HOVER_EMISSIVE_HEX = 0x1a5fb4;
const MA_STL_PICK_HOVER_COLOR_LERP = 0.42;
const MA_STL_PICK_HOVER_EMISSIVE_INTENSITY = 0.38;

/** Tope `uFwidthFloor` cerca (close-up); alejar usa curva aparte hasta {@link MA_STL_DESING2_GRID_FWIDTH_CAP_FAR}. */
const MA_STL_DESING2_GRID_FWIDTH_CAP_NEAR = 5e-4;

/** Tope `uFwidthFloor` alejado (evita relleno sólido cian al alejar). */
const MA_STL_DESING2_GRID_FWIDTH_CAP_FAR = 0.0045;

/** Por encima de este mm/px se aplica suelo de `fwidth` “far” y LOD de celdas. */
const MA_STL_DESING2_GRID_WPP_FAR_START_MM = 1.5;

/** LOD rejilla Desing_2: umbral mm/px → multiplicador de {@link MA_STL_DESING2_GRID_MINOR_MM} / mayor. */
const MA_STL_DESING2_GRID_LOD_WPP_TIER2 = 3;
const MA_STL_DESING2_GRID_LOD_WPP_TIER3 = 10;
const MA_STL_DESING2_GRID_LOD_MULT_TIER2 = 2;
const MA_STL_DESING2_GRID_LOD_MULT_TIER3 = 4;

/** `InfiniteGridHelper` ctor — Desing_2; se reduce dinámicamente al alejar (líneas más gruesas). */
const MA_STL_DESING2_GRID_LINEWIDTH_BASE = 2.55;

/**
 * Design-3d `createCamara` + `THREE.InfiniteGridHelper(25, 100)` (~0,25 m / 1 m); Desing_2 en mm → 500 / 2500.
 * @see Desing/Scripts/Design/3D/Design-3d-three.js
 */
const MA_STL_DESING2_PERSP_FOV = 40;
const MA_STL_DESING2_PERSP_NEAR = 1;
const MA_STL_DESING2_PERSP_FAR = 10_000_000;
/** `InfiniteGridHelper` 4º arg por defecto en legacy Three = 8000. */
const MA_STL_DESING2_GRID_DISTANCE_DESIGN3D = 8_000_000;
/** Legacy Design-3d fragment: `pow(d, 3.0)`. */
const MA_STL_DESING2_GRID_FADE_EXPONENT_DESIGN3D = 3;

/** uFadeExponent < 0.01 ⇒ sin fade radial (ver infinite-grid-helper). */
const MA_STL_DESING2_GRID_NO_RADIAL_FADE = 0;

/**
 * Iluminación Desing_2 — copia de Design-3d `createLight` (InitDesaint3d).
 * Posiciones y sombra en metros en origen; × {@link MA_STL_SCENE_MM_PER_PHYSICAL_METER} → mm escena.
 * @see Desing/Scripts/Design/3D/Design-3d-three.js
 */
const MA_STL_DESING2_LIGHT_AMBIENT_COLOR = 0xffffff;
const MA_STL_DESING2_LIGHT_AMBIENT_INTENSITY = 0.5;
/** Directional con sombra (única añadida a escena en Design-3d). Color/intensidad por defecto THREE (blanco, 1). */
const MA_STL_DESING2_LIGHT_SHADOW_DIR_POS_M = Object.freeze([2.5, 2, 2]);
const MA_STL_DESING2_LIGHT_SHADOW_MAP_SIZE = 512;
const MA_STL_DESING2_LIGHT_SHADOW_NEAR_M = 0.5;
const MA_STL_DESING2_LIGHT_SHADOW_FAR_M = 100;

/**
 * Zoom de referencia para alcance de rejilla (`uDistance`) a ~14 m visibles; no limita OrbitControls.
 * @param {number} frustumHalfY mitad altura mundo del encuadre refit
 */
function maStlDesing2MinZoomFromHalfY(frustumHalfY) {
    const hy = Math.max(frustumHalfY, 1e-6);
    return Math.max((2 * hy) / MA_STL_DESING2_MAX_VISIBLE_HEIGHT_MM, MA_STL_DESING2_MIN_ZOOM_FLOOR);
}

/**
 * mm de escena por píel de pantalla (ortográfica).
 * @param {THREE.OrthographicCamera} camera
 * @param {THREE.WebGLRenderer} renderer
 */
function maStlOrthoWorldMmPerPixel(camera, renderer) {
    const el = renderer.domElement;
    const pxH = Math.max(el.height, 1);
    const pxW = Math.max(el.width, 1);
    const z = Math.max(camera.zoom, 0.001);
    const worldH = (camera.top - camera.bottom) / z;
    const worldW = (camera.right - camera.left) / z;
    return Math.max(worldH / pxH, worldW / pxW);
}

/**
 * mm de escena por píxel (orto o perspectiva Desing_2).
 * @param {THREE.Camera} camera
 * @param {THREE.WebGLRenderer} renderer
 * @param {THREE.Vector3|null} [orbitTarget] requerido si `camera` es perspectiva
 */
function maStlWorldMmPerPixel(camera, renderer, orbitTarget) {
    if (camera && camera.isOrthographicCamera) {
        return maStlOrthoWorldMmPerPixel(camera, renderer);
    }
    if (camera && camera.isPerspectiveCamera && orbitTarget) {
        const el = renderer.domElement;
        const pxH = Math.max(el.height, 1);
        const dist = Math.max(camera.position.distanceTo(orbitTarget), 1e-3);
        const halfFov = THREE.MathUtils.degToRad(camera.fov * 0.5);
        const worldH = 2 * dist * Math.tan(halfFov);
        return worldH / pxH;
    }
    return 1;
}

/**
 * Tamaños de celda rejilla Desing_2 (mm) según zoom en pantalla.
 * @param {number} wpp mm escena / px
 * @param {number} [baseMinorMm]
 * @param {number} [baseMajorMm]
 * @returns {{ minorMm: number, majorMm: number, lodMult: number }}
 */
function maStlDesing2GridLodCellSizesMm(wpp, baseMinorMm, baseMajorMm) {
    let mult = 1;
    if (wpp >= MA_STL_DESING2_GRID_LOD_WPP_TIER3) {
        mult = MA_STL_DESING2_GRID_LOD_MULT_TIER3;
    } else if (wpp >= MA_STL_DESING2_GRID_LOD_WPP_TIER2) {
        mult = MA_STL_DESING2_GRID_LOD_MULT_TIER2;
    }
    const bm = baseMinorMm != null && baseMinorMm > 0 ? baseMinorMm : MA_STL_DESING2_GRID_MINOR_MM;
    const bM = baseMajorMm != null && baseMajorMm > 0 ? baseMajorMm : MA_STL_DESING2_GRID_MAJOR_MM;
    return {
        minorMm: bm * mult,
        majorMm: bM * mult,
        lodMult: mult
    };
}

function maStlDesing2EmptyBaselineDimMm() {
    return MA_STL_DESING2_EMPTY_BASELINE_MM;
}

function maStlRulerExtentFromMaxDimMm(maxDimMm) {
    const raw = Math.max(maxDimMm, 1e-9) * 1.2;
    return THREE.MathUtils.clamp(raw, MA_STL_DESING2_RULE_MINOR_MM * 2, MA_STL_DESING2_RULE_EXTENT_CAP_MM);
}

/**
 * Longitud flechas UCS desde extensión del modelo.
 * @param {boolean} desingMmScene Desing_2 con reglas: mundo en mm; maestro legacy: mismo criterio que antes (clamps en ~m).
 */
function maStlWorldAxesLength(maxDim, desingMmScene) {
    const d = Math.max(maxDim, 1e-9);
    if (!desingMmScene) {
        return THREE.MathUtils.clamp(d * 0.2, 0.3, 1.0);
    }
    const minArm = THREE.MathUtils.clamp(d * 0.08, 80, 400);
    const maxArm = Math.min(Math.max(d * 0.55, minArm), Math.max(d * 0.92, minArm));
    return THREE.MathUtils.clamp(d * 0.22, minArm, maxArm);
}

/** Etiquetas de reglas: lectura física en metros (`1`, `2`, `2.5` — sin ceros de relleno). `tMeters` = coordenada en **metros** (geometría local Desing_2). */
function maStlRulerLabelMetersFromWorldM(tMeters) {
    const m = Math.round(tMeters * 1000) / 1000;
    if (Math.abs(m - Math.round(m)) < 1e-9) {
        return String(Math.round(m));
    }
    return String(parseFloat(m.toFixed(3)));
}

/**
 * Cotas editables herramienta línea Desing_2: metros físicos legibles tipo CAD (máximo {@link MA_STL_DESING2_DIM_EDITABLE_METERS_DECIMALS} decimales; separadores del navegador).
 * @param {number} lengthMm longitud física mm escena
 * @param {string=} localeTag opcional (`undefined` ⇒ runtime); no envía servidor
 */
function maStlDesing2DimEditableMetersDisplayFromMm(lengthMm, localeTag) {
    const m = Math.max(0, Number(lengthMm) || 0) / MA_STL_SCENE_MM_PER_PHYSICAL_METER;
    try {
        const fmt = new Intl.NumberFormat(localeTag || undefined, {
            minimumFractionDigits: 0,
            maximumFractionDigits: MA_STL_DESING2_DIM_EDITABLE_METERS_DECIMALS,
        });
        return fmt.format(m);
    } catch (_eIntl) {
        return String(parseFloat(m.toFixed(MA_STL_DESING2_DIM_EDITABLE_METERS_DECIMALS)));
    }
}

/** `true` sólo desarrollo típico (parse fallido cotas línea usuario). */
function maStlStlViewerIsLocalDevHost() {
    if (typeof location === 'undefined' || !location.hostname) return false;
    const h = String(location.hostname).toLowerCase();
    return h === 'localhost' || h === '127.0.0.1';
}

function maStlPlanRulerTickIsMajor(minorIndexOneBased, minorsPerMajor) {
    return minorIndexOneBased > 0 && minorIndexOneBased % minorsPerMajor === 0;
}

/** Plano horizontal XZ rejilla maestro (`uPlaneY`): Y negativo bajo origen UCS (preview artículos). */
function maStlPlanWorkspaceBaselineY(extentMm, axisLen, minorStepMm) {
    return -Math.max(extentMm * 0.06, axisLen * 0.12, minorStepMm * 0.6);
}

/**
 * Sincroniza `uPlaneY` de la rejilla infinita.
 * Desing_2: {@link MA_STL_DESING2_WORKSPACE_FLOOR_Y_MM} (mismo suelo que reglas/sombra).
 * Maestro: ligeramente bajo el modelo (refit puede afinar con `box.min.y`).
 */
function maStlSyncGridPlaneY(grid, desingMmScene, maxDim) {
    const u = grid && grid.material && grid.material.uniforms;
    if (!u || !u.uPlaneY) return;
    if (desingMmScene) {
        u.uPlaneY.value = MA_STL_DESING2_WORKSPACE_FLOOR_Y_MM;
        return;
    }
    const d = Math.max(maxDim, 1e-9);
    u.uPlaneY.value = -Math.max(d * 0.008, 1e-6);
}

/**
 * Desing_2: traslada `group` en Y para que `box.min.y` coincida con el suelo workspace.
 * Idempotente: re-encuadres y STL secundario recalculan desde la caja ya apoyada.
 * @param {THREE.Group} group
 * @param {number} floorYMm
 * @returns {boolean} true si aplicó corrección
 */
function masterArticleStlGroundGroupOnWorkspaceFloor(group, floorYMm) {
    group.updateMatrixWorld(true);
    const box = new THREE.Box3().setFromObject(group);
    const dy = floorYMm - box.min.y;
    if (Math.abs(dy) < 1e-6) return false;
    group.position.y += dy;
    group.updateMatrixWorld(true);
    return true;
}

/**
 * Extensión reglas+bloque Desing_2 (mm escena): crece con el modelo; tope habitual ~25 m físicos (configurable vía cookie / Entorno).
 * @param {number} maxDimLocal
 * @param {number} [extentCapMm] tope en mm escena (≤ 50 m conservador si se omite el default fijo legacy)
 * @param {number} [baseMajorMm] paso mayor rejilla/reglas base (LOD no aplica aquí)
 */
function maStlDesing2RulerExtentMm(maxDimLocal, extentCapMm, baseMajorMm) {
    const d = Math.max(maxDimLocal, 1e-9);
    const major = baseMajorMm != null && baseMajorMm > 0 ? baseMajorMm : MA_STL_DESING2_GRID_MAJOR_MM;
    const cap =
        extentCapMm != null && extentCapMm > 0 ? extentCapMm : MA_STL_DESING2_RULE_FIXED_EXTENT_MM;
    return THREE.MathUtils.clamp(Math.max(d * 2.75, major * 10), major * 8, cap);
}

/**
 * Mitad de la diagonal del frustum orto (mm escena) a un zoom dado.
 * @param {number} frustumHalfY mitad altura mundo (antes de `camera.zoom`)
 * @param {number} aspect ancho/alto del viewport
 * @param {number} zoom zoom ortográfico (1 = encuadre refit)
 */
function maStlOrthoReachMm(frustumHalfY, aspect, zoom) {
    const z = Math.max(zoom, 0.001);
    const worldH = frustumHalfY * 2 / z;
    const worldW = worldH * Math.max(aspect, 1e-6);
    return 0.5 * Math.hypot(worldW, worldH);
}

/**
 * Uniforms de rejilla + plano de trabajo (Desing_2 mm o maestro legacy).
 * @param {THREE.Mesh} grid `InfiniteGridHelper`
 * @param {number} maxDim
 * @param {boolean} desingMmScene
 * @param {number} frustumHalfY
 * @param {number} aspect viewport w/h
 * @param {number} [orthoMinZoom] zoom mínimo Desing_2 para `uDistance` (default: derivado de `frustumHalfY`)
 */
function maStlSyncInfiniteGridWorkspace(
    grid,
    maxDim,
    desingMmScene,
    frustumHalfY,
    aspect,
    orthoMinZoom,
    baseMinorMm,
    baseMajorMm
) {
    const gMat = grid && grid.material;
    if (!gMat || !gMat.uniforms) return;
    const u = gMat.uniforms;
    const d = Math.max(maxDim, 1e-9);
    const bm = baseMinorMm != null && baseMinorMm > 0 ? baseMinorMm : MA_STL_DESING2_GRID_MINOR_MM;
    const bM = baseMajorMm != null && baseMajorMm > 0 ? baseMajorMm : MA_STL_DESING2_GRID_MAJOR_MM;
    const camFitDim = desingMmScene ? Math.max(d * 1.22, bM * 4) : Math.max(d * 1.18, 1e-6);
    maStlSyncGridPlaneY(grid, desingMmScene, d);
    if (desingMmScene) {
        u.uSize1.value = bm;
        u.uSize2.value = bM;
        const minZ =
            orthoMinZoom != null && orthoMinZoom > 0
                ? orthoMinZoom
                : MA_STL_DESING2_MIN_ZOOM_FLOOR;
        const reachMinZoom = maStlOrthoReachMm(frustumHalfY, aspect, minZ);
        u.uDistance.value = Math.max(
            MA_STL_DESING2_GRID_DISTANCE_DESIGN3D,
            camFitDim * 90,
            d * 85,
            bM * 3.5,
            reachMinZoom * MA_STL_DESING2_GRID_REFIT_DISTANCE_PAD
        );
        if (u.uFadeExponent) u.uFadeExponent.value = MA_STL_DESING2_GRID_FADE_EXPONENT_DESIGN3D;
    } else {
        u.uSize1.value = Math.max(d / 16, 1e-6);
        u.uSize2.value = Math.max(d / 4, 1e-5);
        u.uDistance.value = Math.max(camFitDim * 90, d * 85, 1500);
        if (u.uFadeExponent) u.uFadeExponent.value = 2;
    }
}

/** Frustum ortográfico (mitad altura mundo) coherente con `refitCamerasToObject`. */
function maStlFrustumHalfYFromMaxDim(maxDim, desingMmScene, gridMajorMm) {
    const d = Math.max(maxDim, 1e-9);
    const major = gridMajorMm != null && gridMajorMm > 0 ? gridMajorMm : MA_STL_DESING2_GRID_MAJOR_MM;
    const camFitDim = desingMmScene ? Math.max(d * 1.22, major * 4) : Math.max(d * 1.18, 1e-6);
    return camFitDim * 0.55;
}

function maStlDisableRaycastOnOverlay(root) {
    if (!root) return;
    root.traverse(function (o) {
        o.raycast = function () {};
    });
}

/** Visibilidad de overlay pick rejilla (grupo + hijos; sin frustum cull). */
function maStlSetOverlaySubtreeVisible(root, visible) {
    if (!root) return;
    root.visible = visible;
    root.traverse(function (o) {
        o.visible = visible;
        if (o.isMesh || o.isLine || o.isLineSegments || o.isLineLoop) {
            o.frustumCulled = false;
        }
    });
}

function maStlMakeOverlayLineMat() {
    return new THREE.LineBasicMaterial({
        color: 0x0891b2,
        transparent: true,
        opacity: 0.95,
        depthTest: false,
        depthWrite: false
    });
}

/**
 * Líneas de la herramienta “línea” en planta (Desing_2). Naranja intenso (#ff6600).
 * Grosor real en pantalla vía `Line2` + `LineMaterial` (px CSS; requiere `resolution` al redimensionar).
 */
const MA_STL_USER_FLOOR_LINE_SCREEN_PX_WIDTH = 3;

function maStlMakeUserFloorLineMaterial() {
    return new LineMaterial({
        color: 0xff6600,
        linewidth: MA_STL_USER_FLOOR_LINE_SCREEN_PX_WIDTH,
        transparent: false,
        opacity: 1,
        depthTest: false,
        depthWrite: false
    });
}

/** Segmento usuario Desing_2 (`Line2` + `userData.maStlUserPlanLine`). */
function maStlIsUserFloorPlanLineObject(o) {
    return !!(o && o.isLine2 && o.userData && o.userData.maStlUserPlanLine);
}

function maStlApplyUserFloorLineMaterialResolution(mat, widthPx, heightPx) {
    if (mat && mat.isLineMaterial && mat.resolution) {
        mat.resolution.set(widthPx, heightPx);
    }
}

function maStlCreateUserFloorLineGeometryMm(a, b) {
    const geo = new LineGeometry();
    geo.setPositions(new Float32Array([a.x, a.y, a.z, b.x, b.y, b.z]));
    return geo;
}

function maStlSetUserFloorLineGeometryMm(line, a, b) {
    if (!line || !line.geometry || !line.isLine2) return;
    line.geometry.setPositions(new Float32Array([a.x, a.y, a.z, b.x, b.y, b.z]));
    line.geometry.computeBoundingBox();
    line.geometry.computeBoundingSphere();
}

/** Screen-space proximity pick for floor user lines (`Line2`). */
const MA_STL_USER_FLOOR_LINE_SCREEN_PICK_PX = 13;

/** Hover color (brighter than {@link maStlMakeUserFloorLineMaterial}). */
const MA_STL_USER_FLOOR_LINE_HOVER_HEX = 0xffcc66;

/** Cotas herramienta línea usuario (planta): líneas paralelas/extensiones CAD + overlay HTML sobre canvas. */
const MA_STL_USER_FLOOR_LINE_DIM_DRAW_Y_EPS_MM = 1.55;
/** Bajo esta magnitud planar (ΔX o ΔZ) se omite línea/arrows Δ (readout textual sigue visible). */
const MA_STL_USER_FLOOR_LINE_DIM_MIN_PLAN_DRAW_MM = 4;
/** Cotas paralelas/arrows ΔX ΔZ desde anclaje reglas hasta **P1** (punto inicial); escala triangular flechas CAD en malla (~1/3). */
const MA_STL_USER_FLOOR_LINE_DIM_ARROW_MESH_SCALE = 1 / 3;
/** `clientX/Y` pueden expandir rect de pick (px) sobre readout midpoint. */
const MA_STL_USER_FLOOR_LINE_DIM_LABEL_HIT_PADDING_PX = 28;
/** Vértices compartidos al fusionar segmentos colineales (mm escena). */
const MA_STL_USER_FLOOR_LINE_MERGE_ENDPOINT_EPS_MM = 0.05;
/** Mismo sentido en XZ: `dot(d1,d2) >` umbral (~8° máx. si unitarios). */
const MA_STL_USER_FLOOR_LINE_MERGE_SAME_SENSE_DOT_MIN = 0.99;
/** Colinealidad XZ: `|d1×d2|` con dirs unitarios (`~sin θ`; 0.02 ≈ 1,1°). */
const MA_STL_USER_FLOOR_LINE_MERGE_COLLINEAR_CROSS_MAX = 0.02;
/** Clic derecho corto en canvas (px) sin pan de órbita → refactor merge colineal global. */
const MA_STL_USER_FLOOR_LINE_REFACTOR_RMB_CLICK_MAX_PX = 6;

/** Squared pixel distance from (px,py) to segment (ax,ay)-(bx,by). */
function maStlSqDistPointToSegment2dPx(px, py, ax, ay, bx, by) {
    const abx = bx - ax;
    const aby = by - ay;
    const apx = px - ax;
    const apy = py - ay;
    const ab2 = abx * abx + aby * aby;
    if (ab2 < 1e-12) {
        return apx * apx + apy * apy;
    }
    let t = (apx * abx + apy * aby) / ab2;
    t = Math.max(0, Math.min(1, t));
    const cx = ax + abx * t;
    const cy = ay + aby * t;
    const dpx = px - cx;
    const dpy = py - cy;
    return dpx * dpx + dpy * dpy;
}

/**
 * Parse editable length (`m`, `mm`, comma/dot decimals). Empty → invalid.
 * @returns {number|null} mm escena físicos ≡ mm (×{@link MA_STL_SCENE_MM_PER_PHYSICAL_METER} convención).
 */
function maStlParseLengthInputValueToMm(text) {
    if (text == null) return null;
    let s = String(text).trim().replace(/[\u00a0\u202f\u2009\u2028\u2029]/g, ' ');
    if (!s) return null;
    let unitMult = MA_STL_SCENE_MM_PER_PHYSICAL_METER;
    const suf = s.match(/\s*(mm|m)\s*$/i);
    if (suf) {
        if (String(suf[1]).toLowerCase() === 'mm') unitMult = 1;
        else unitMult = MA_STL_SCENE_MM_PER_PHYSICAL_METER;
        s = s.slice(0, suf.index).trim();
        if (!s) return null;
    }
    /* Quitar otros símbolo de miles/espacios (p. ej. salida Intl: "12 345,67") */
    s = s.replace(/[^\d,.+\-]/g, '');
    if (!s) return null;
    const comma = s.lastIndexOf(',');
    const dot = s.lastIndexOf('.');
    let numTok = s;
    if (comma >= 0 && dot >= 0) {
        if (dot > comma) {
            numTok = numTok.replace(/,/g, '');
        } else {
            numTok = numTok.replace(/\./g, '').replace(',', '.');
        }
    } else if (comma >= 0) {
        numTok = numTok.replace(/,/g, '.');
    }
    const val = Number(numTok);
    if (!Number.isFinite(val)) return null;
    return val * unitMult;
}

function maStlDesing2LengthMmRoundedEditableFromMm(lengthMm) {
    const m = Math.max(0, Number(lengthMm) || 0) / MA_STL_SCENE_MM_PER_PHYSICAL_METER;
    const f = Math.pow(10, MA_STL_DESING2_DIM_EDITABLE_METERS_DECIMALS);
    const mRound = Math.round(m * f) / f;
    return mRound * MA_STL_SCENE_MM_PER_PHYSICAL_METER;
}

/**
 * Desing_2: desplazamiento plano ΔX / ΔZ (mm escena ↔ m) para cotas herramienta línea: signed 3 dec máximos.
 */
function maStlDesing2SignedDeltaMetersDisplayFromMm(deltaMm, localeTag) {
    const m = (Number(deltaMm) || 0) / MA_STL_SCENE_MM_PER_PHYSICAL_METER;
    try {
        const fmt = new Intl.NumberFormat(localeTag || undefined, {
            minimumFractionDigits: 0,
            maximumFractionDigits: MA_STL_DESING2_DIM_EDITABLE_METERS_DECIMALS,
            signDisplay: 'exceptZero',
        });
        return fmt.format(m);
    } catch (_eIntl) {
        let r = Number.parseFloat(m.toFixed(MA_STL_DESING2_DIM_EDITABLE_METERS_DECIMALS));
        if (!Number.isFinite(r)) r = 0;
        if (Math.abs(r) < Math.pow(10, -MA_STL_DESING2_DIM_EDITABLE_METERS_DECIMALS)) {
            return '0';
        }
        return String(r);
    }
}

/** Redondeo firmado mismo paso que longitud editable (~m → mm escena físicos). */
function maStlDesing2SignedDeltaMmRoundedEditableFromMm(signedMm) {
    const m = (Number(signedMm) || 0) / MA_STL_SCENE_MM_PER_PHYSICAL_METER;
    const f = Math.pow(10, MA_STL_DESING2_DIM_EDITABLE_METERS_DECIMALS);
    const mRound = Math.round(m * f) / f;
    return mRound * MA_STL_SCENE_MM_PER_PHYSICAL_METER;
}

/**
 * Vector dirección planar (longitud 1 salvo modo casi paralelo rechazado) y longitud P1→P2 en planta ({@link MA_STL_LINE_TOOL_DIR_EPS_MM}).
 * @returns {{ x: number, z: number, len: number } | null}
 */
function maStlLineToolFloorDirLenFromDeltaMm(dxMm, dzMm, ortho15Enabled) {
    const h = Math.hypot(dxMm, dzMm);
    if (h < MA_STL_LINE_TOOL_DIR_EPS_MM) return null;
    if (!ortho15Enabled) {
        return { x: dxMm / h, z: dzMm / h, len: h };
    }
    const ang = Math.atan2(dzMm, dxMm);
    const snapped = Math.round(ang / MA_STL_LINE_TOOL_ORTHO15_STEP_RAD) * MA_STL_LINE_TOOL_ORTHO15_STEP_RAD;
    return { x: Math.cos(snapped), z: Math.sin(snapped), len: h };
}

/**
 * Tema líneas {@link maStlBuildPlanRulers} sólo Desing_2: fondo negro → cian/azul heredado; fondo claro → gris ~70 % opacity.
 * @param {THREE.LineBasicMaterial|null|undefined} mat
 * @param {boolean} darkBg
 */
function maStlApplyDesing2RulerLineMaterialTheme(mat, darkBg) {
    if (!mat) return;
    if (darkBg) {
        mat.color.setHex(0x0891b2);
        mat.opacity = 0.95;
    } else {
        mat.color.setHex(MA_STL_DESING2_RULE_LINE_LIGHT_HEX);
        mat.opacity = 0.7;
    }
    mat.needsUpdate = true;
}

/**
 * Color de texto de las etiquetas numéricas de reglas (sprites {@link maStlMakeTextSprite} `thinFillOnly`).
 * @param {boolean} darkBg
 * @returns {string}
 */
function maStlDesing2RulerLabelFillForTheme(darkBg) {
    return darkBg ? MA_STL_DESING2_RULE_LABEL_DARK_FILL : MA_STL_DESING2_RULE_LABEL_LIGHT_FILL;
}

/**
 * @param {boolean} darkBg
 */
function maStlCreateDesing2RulerLineMaterial(darkBg) {
    const m = new THREE.LineBasicMaterial({
        color: 0x0891b2,
        transparent: true,
        opacity: 0.95,
        depthTest: false,
        depthWrite: false
    });
    maStlApplyDesing2RulerLineMaterialTheme(m, darkBg);
    return m;
}

/**
 * Lightweight label Sprite.
 * Default: filled text + outline (UCS X/Y/Z). `thinFillOnly`: single `fillText`, thin sans (reglas Desing_2).
 * @param {string} text
 * @param {number} worldScale On-screen footprint in **scene units** (mm en Desing_2; archivo suelto en maestro legacy).
 * @param {{ minPx?: number, maxPx?: number, fontRatio?: number, worldToPixelMult?: number, spriteExpand?: number, strokeRatio?: number, thinFillOnly?: boolean, fontPx?: number, fontWeight?: string|number, fontFamily?: string, fillColor?: string, canvasPad?: number, thinPillFill?: string|null, thinPillStroke?: string|null, thinPillLineWidth?: number, thinPillRadiusPx?: number }=} opts
 *          `worldToPixelMult`: ~canvas px per scene unit — keep labels within ~⅓ interval between reglas mayores.
 */
function maStlMakeTextSprite(text, worldScale, opts) {
    opts = opts || {};
    const thinFillOnly = !!opts.thinFillOnly;
    const spriteExpand = opts.spriteExpand != null ? opts.spriteExpand : thinFillOnly ? 1.0 : 1.02;
    const canvas = document.createElement('canvas');
    const ctx = canvas.getContext('2d');
    if (!ctx) return null;

    let texW;
    let texH;
    if (thinFillOnly) {
        const fontPx = opts.fontPx != null ? opts.fontPx : 14;
        const pad = opts.canvasPad != null ? opts.canvasPad : 3;
        const weight = opts.fontWeight != null ? opts.fontWeight : 500;
        const family = opts.fontFamily || '"Segoe UI",Arial,sans-serif';
        const font = weight + ' ' + fontPx + 'px ' + family;
        ctx.font = font;
        ctx.textAlign = 'center';
        ctx.textBaseline = 'middle';
        const tw = Math.ceil(ctx.measureText(text).width) + pad * 2;
        texW = Math.max(tw, 8);
        texH = fontPx + pad * 2;
        canvas.width = texW;
        canvas.height = texH;
        ctx.clearRect(0, 0, texW, texH);
        const pillFill = opts.thinPillFill != null ? opts.thinPillFill : null;
        const pillStroke = opts.thinPillStroke != null ? opts.thinPillStroke : null;
        const pr = opts.thinPillRadiusPx != null ? opts.thinPillRadiusPx : Math.min(10, texH * 0.42);
        if (pillFill || pillStroke) {
            ctx.beginPath();
            if (ctx.roundRect) {
                ctx.roundRect(0.5, 0.5, texW - 1, texH - 1, Math.max(pr, 3));
            } else {
                ctx.rect(0.5, 0.5, texW - 1, texH - 1);
            }
            if (pillFill) {
                ctx.fillStyle = pillFill;
                ctx.fill();
            }
            if (pillStroke) {
                ctx.strokeStyle = pillStroke;
                ctx.lineWidth =
                    opts.thinPillLineWidth != null
                        ? opts.thinPillLineWidth
                        : THREE.MathUtils.clamp(fontPx / 11, 2.75, 5.5);
                ctx.stroke();
            }
        }
        ctx.font = font;
        ctx.textAlign = 'center';
        ctx.textBaseline = 'middle';
        ctx.fillStyle = opts.fillColor || '#333333';
        ctx.fillText(text, texW * 0.5, texH * 0.5);
    } else {
        const minPx = opts.minPx != null ? opts.minPx : 40;
        const maxPx = opts.maxPx != null ? opts.maxPx : 256;
        const fontRatio = opts.fontRatio != null ? opts.fontRatio : 0.5;
        const w2p = opts.worldToPixelMult != null ? opts.worldToPixelMult : 0.95;
        const px = THREE.MathUtils.clamp(Math.round(Math.max(worldScale, 1e-9) * w2p), minPx, maxPx);
        texW = px;
        texH = px;
        canvas.width = px;
        canvas.height = px;
        ctx.clearRect(0, 0, px, px);
        ctx.font = 'bold ' + Math.round(px * fontRatio) + 'px system-ui,sans-serif';
        ctx.textAlign = 'center';
        ctx.textBaseline = 'middle';
        const strokeRatio = opts.strokeRatio != null ? opts.strokeRatio : 0.09;
        ctx.lineWidth = Math.max(2, px * strokeRatio);
        ctx.strokeStyle = 'rgba(0,0,0,0.86)';
        ctx.fillStyle = '#ffffff';
        const cx = px * 0.5;
        const cy = px * 0.53;
        ctx.strokeText(text, cx, cy);
        ctx.fillText(text, cx, cy);
    }

    const tex = new THREE.CanvasTexture(canvas);
    tex.colorSpace = THREE.SRGBColorSpace;
    tex.needsUpdate = true;
    const mat = new THREE.SpriteMaterial({ map: tex, transparent: true, depthTest: false });
    const sprite = new THREE.Sprite(mat);
    sprite.renderOrder = 200;
    const ws = spriteExpand;
    if (thinFillOnly && texW > 0 && texH > 0) {
        const aspect = texW / texH;
        const h = worldScale * ws;
        sprite.scale.set(h * aspect, h, 1);
        sprite.userData.maStlThinTextSpriteState = {
            text,
            worldScale,
            thinOpts: {
                fontPx: opts.fontPx != null ? opts.fontPx : 14,
                canvasPad: opts.canvasPad != null ? opts.canvasPad : 3,
                fontWeight: opts.fontWeight != null ? opts.fontWeight : 500,
                fontFamily: opts.fontFamily,
                fillColor: opts.fillColor || '#333333',
                spriteExpand: ws,
                thinPillFill: opts.thinPillFill != null ? opts.thinPillFill : null,
                thinPillStroke: opts.thinPillStroke != null ? opts.thinPillStroke : null,
                thinPillLineWidth: opts.thinPillLineWidth,
                thinPillRadiusPx: opts.thinPillRadiusPx
            }
        };
    } else {
        sprite.scale.set(worldScale * ws, worldScale * ws, 1);
    }
    return sprite;
}

/**
 * Redibuja el canvas de un sprite {@link maStlMakeTextSprite} en modo `thinFillOnly` (p. ej. números de regla tras cambiar tema).
 * @param {THREE.Sprite} sprite
 */
function maStlRedrawThinTextSprite(sprite) {
    const st = sprite && sprite.userData && sprite.userData.maStlThinTextSpriteState;
    if (!st || !sprite.material || !sprite.material.map) return;
    const tex = sprite.material.map;
    const canvas = tex.image;
    const ctx = canvas && canvas.getContext && canvas.getContext('2d');
    if (!ctx) return;
    const text = st.text;
    const o = st.thinOpts;
    const fontPx = o.fontPx != null ? o.fontPx : 14;
    const pad = o.canvasPad != null ? o.canvasPad : 3;
    const weight = o.fontWeight != null ? o.fontWeight : 500;
    const family = o.fontFamily || '"Segoe UI",Arial,sans-serif';
    const font = weight + ' ' + fontPx + 'px ' + family;
    ctx.font = font;
    ctx.textAlign = 'center';
    ctx.textBaseline = 'middle';
    const tw = Math.ceil(ctx.measureText(text).width) + pad * 2;
    const texW = Math.max(tw, 8);
    const texH = fontPx + pad * 2;
    if (canvas.width !== texW || canvas.height !== texH) {
        canvas.width = texW;
        canvas.height = texH;
        ctx.font = font;
        ctx.textAlign = 'center';
        ctx.textBaseline = 'middle';
        const ws = st.worldScale;
        const se = o.spriteExpand != null ? o.spriteExpand : 1;
        const aspect = texW / texH;
        const h = ws * se;
        sprite.scale.set(h * aspect, h, 1);
    }
    ctx.clearRect(0, 0, canvas.width, canvas.height);
    const pillFill = o.thinPillFill != null ? o.thinPillFill : null;
    const pillStroke = o.thinPillStroke != null ? o.thinPillStroke : null;
    const pr = o.thinPillRadiusPx != null ? o.thinPillRadiusPx : Math.min(10, texH * 0.42);
    if (pillFill || pillStroke) {
        ctx.beginPath();
        if (ctx.roundRect) {
            ctx.roundRect(0.5, 0.5, texW - 1, texH - 1, Math.max(pr, 3));
        } else {
            ctx.rect(0.5, 0.5, texW - 1, texH - 1);
        }
        if (pillFill) {
            ctx.fillStyle = pillFill;
            ctx.fill();
        }
        if (pillStroke) {
            ctx.strokeStyle = pillStroke;
            ctx.lineWidth =
                o.thinPillLineWidth != null
                    ? o.thinPillLineWidth
                    : THREE.MathUtils.clamp(fontPx / 11, 2.75, 5.5);
            ctx.stroke();
        }
    }
    ctx.font = font;
    ctx.textAlign = 'center';
    ctx.textBaseline = 'middle';
    ctx.fillStyle = o.fillColor || '#333333';
    ctx.fillText(text, texW * 0.5, texH * 0.5);
    tex.needsUpdate = true;
}

/** UCS en origen: líneas finas, flechas abiertas, caja hueca. Si `includeZAxis`, también eje Z y etiqueta Z (solo Desing_2). */
function maStlBuildUcsFromAxisLen(axisLen, lineMat, includeZAxis) {
    const root = new THREE.Group();
    root.renderOrder = 150;
    const head = axisLen * 0.08;
    const body = Math.max(axisLen - head, axisLen * 0.82);

    function addOpenLine(ax, ay, az, bx, by, bz) {
        const geo = new THREE.BufferGeometry().setFromPoints([
            new THREE.Vector3(ax, ay, az),
            new THREE.Vector3(bx, by, bz)
        ]);
        const ln = new THREE.Line(geo, lineMat);
        ln.renderOrder = 150;
        root.add(ln);
    }

    addOpenLine(0, 0, 0, body, 0, 0);
    addOpenLine(body, 0, 0, axisLen, head * 0.52, 0);
    addOpenLine(body, 0, 0, axisLen, -head * 0.52, 0);
    addOpenLine(0, 0, 0, 0, body, 0);
    addOpenLine(0, body, 0, -head * 0.52, axisLen, 0);
    addOpenLine(0, body, 0, head * 0.52, axisLen, 0);
    if (includeZAxis) {
        addOpenLine(0, 0, 0, 0, 0, body);
        addOpenLine(0, 0, body, head * 0.52, 0, axisLen);
        addOpenLine(0, 0, body, -head * 0.52, 0, axisLen);
    }

    const boxSz = axisLen * 0.055;
    const boxGeo = new THREE.BoxGeometry(boxSz, boxSz, boxSz);
    const boxLn = new THREE.LineSegments(new THREE.EdgesGeometry(boxGeo), lineMat);
    boxLn.renderOrder = 150;
    root.add(boxLn);
    boxGeo.dispose();

    const labelScale = axisLen * 0.11;
    const zLift = axisLen * 0.018;
    const sx = maStlMakeTextSprite('X', labelScale);
    if (sx) {
        sx.position.set(axisLen + labelScale * 0.72, labelScale * 0.05, zLift);
        root.add(sx);
    }
    const sy = maStlMakeTextSprite('Y', labelScale);
    if (sy) {
        sy.position.set(-labelScale * 0.06, axisLen + labelScale * 0.68, zLift);
        root.add(sy);
    }
    if (includeZAxis) {
        const sz = maStlMakeTextSprite('Z', labelScale);
        if (sz) {
            sz.position.set(-labelScale * 0.06, -labelScale * 0.06, axisLen + labelScale * 0.78);
            root.add(sz);
        }
    }

    maStlDisableRaycastOnOverlay(root);
    return root;
}

/** Color ejes XYZ Desing_2 (vectores con punta de flecha, independientes de reglas). */
const MA_STL_XYZ_AXES_COLOR = 0x00aa00;

function maStlApplyOverlayDepthState(mat) {
    if (!mat) return;
    mat.depthTest = false;
    mat.depthWrite = false;
    mat.transparent = true;
    if (mat.opacity === undefined || mat.opacity >= 1) mat.opacity = 0.98;
}

/** Ejes XYZ en origen: verdes, convención CAD (ZWCAD/AutoCAD) — solo Desing_2. */
function maStlBuildXyzAxesFromAxisLen(axisLen, colorHex) {
    const root = new THREE.Group();
    root.renderOrder = 150;
    const headLen = Math.max(axisLen * 0.12, 1e-6);
    const headWidth = headLen * 0.5;
    const origin = new THREE.Vector3(0, 0, 0);
    /** +X, -Z (arriba en TOP), +Y (vertical Three.js) — etiquetas X/Y/Z según CAD. */
    const dirs = [
        new THREE.Vector3(1, 0, 0),
        new THREE.Vector3(0, 0, -1),
        new THREE.Vector3(0, 1, 0)
    ];
    dirs.forEach(function (dir) {
        const ah = new THREE.ArrowHelper(dir, origin, axisLen, colorHex, headLen, headWidth);
        ah.renderOrder = 150;
        maStlApplyOverlayDepthState(ah.line.material);
        maStlApplyOverlayDepthState(ah.cone.material);
        root.add(ah);
    });

    const labelScale = axisLen * 0.11 * 1.5;
    /** Pequeño desfase en Y/Z por eje para que los sprites no coplanen en origen. */
    const zLiftX = axisLen * 0.026;
    const zLiftY = axisLen * 0.017;
    /** Despeje respecto a numeración de reglas en planta (+X en +Z; -Z en +X). */
    const planLabelClear = labelScale * 0.62;
    /** Etiquetas junto al origen (no en punta de flecha) — margen para diseños largos. */
    const labelOriginPad = Math.max(axisLen * 0.1, labelScale * 0.68);
    const sx = maStlMakeTextSprite('X', labelScale);
    if (sx) {
        sx.position.set(
            labelOriginPad * 1.38,
            labelScale * 0.06,
            zLiftX - planLabelClear * 1.4
        );
        root.add(sx);
    }
    const sy = maStlMakeTextSprite('Y', labelScale);
    if (sy) {
        sy.position.set(
            -planLabelClear * 1.48,
            labelScale * 0.04,
            -labelOriginPad * 1.44 + zLiftY
        );
        root.add(sy);
    }
    const sz = maStlMakeTextSprite('Z', labelScale);
    if (sz) {
        /* ~28% del eje vertical (+Y Three.js = CAD Z); ligero +X y -Z para planta y 3/4. */
        const yPos = Math.max(axisLen * 0.28, labelOriginPad * 4.5);
        const xPos = labelOriginPad * 0.45;
        const zPos = -labelOriginPad * 0.35;
        sz.position.set(xPos, yPos, zPos);
        root.add(sz);
    }

    maStlDisableRaycastOnOverlay(root);
    return root;
}

/**
 * Reglas en plano de trabajo: tramo horizontal en +X y tramo en profundidad en +Z.
 * `extentGeom` / pasos están en **unidades locales**; `localToSceneMm` convierte a mm de escena para grosores
 * de marcas y tamaño de etiquetas (p. ej. `localToSceneMm = 1000` cuando las distancias locales son metros).
 *
 * @param {number} axisLenWorld Longitud brazos UCS en **unidades de escena** (mm en Desing_2).
 * @param {number} extentGeom Extensión regla en unidades locales (m si `localToSceneMm=1000`, else mm).
 * @param {number} minorStep Paso menor en unidades locales.
 * @param {number} majorStep Paso para marcas mayores / numeración.
 * @param {(tLocal: number) => string} formatMajor Etiqueta en lectura física; `tLocal` es coordenada en **las mismas unidades locales** que los pasos (p. ej. metros si se construye en m).
 * @param {number} [localToSceneMm=1] mm de escena por una unidad de geometría local (`1000` cuando la geometría local está en **metros** y el mundo del visor en mm).
 * @param {number|null} [floorYMm] Y del plano de reglas en mm escena; default = baseline bajo origen (legacy).
 * @param {number} [anchorXMm=0] Origen reglas en X (mm escena).
 * @param {number} [anchorZMm=0] Origen reglas en Z (mm escena).
 */
function maStlBuildPlanRulers(axisLenWorld, extentGeom, minorStep, majorStep, lineMat, formatMajor, localToSceneMm, floorYMm, anchorXMm, anchorZMm) {
    const su = localToSceneMm != null && localToSceneMm > 0 ? localToSceneMm : 1;
    const extentMm = extentGeom * su;
    const minorMm = minorStep * su;
    const majorMm = majorStep * su;
    const root = new THREE.Group();
    root.renderOrder = 150;
    const yrMm =
        floorYMm != null ? floorYMm : maStlPlanWorkspaceBaselineY(extentMm, axisLenWorld, minorMm);
    const tickMinMm = THREE.MathUtils.clamp(extentMm * 0.028, minorMm * 2, extentMm * 0.06);
    const tickMaxMm = tickMinMm * 2.1;
    const tickMin = tickMinMm / su;
    const tickMax = tickMaxMm / su;
    /** Plano Y en mm escena → unidades locales antes del `localToSceneMm` del contenedor. */
    const yr = yrMm / su;
    const ax = (anchorXMm != null && Number.isFinite(anchorXMm) ? anchorXMm : 0) / su;
    const az = (anchorZMm != null && Number.isFinite(anchorZMm) ? anchorZMm : 0) / su;
    const xr = ax;
    /** Tamaño en escena menor que hueco típico entre marcas mayores → evitar solapes. */
    const majorsBetween = Math.max(1, Math.round(majorStep / minorStep));
    const numWorldScaleMm = THREE.MathUtils.clamp(
        majorMm * 0.14,
        26,
        Math.min(majorMm * 0.17, Math.max(extentMm * 0.0055, minorMm * 2.4))
    );
    const numWorldScale = numWorldScaleMm / su;
    const rulerNumSpriteOpts = {
        thinFillOnly: true,
        fontPx: 14,
        fontWeight: 500,
        fillColor: MA_STL_DESING2_RULE_LABEL_LIGHT_FILL,
        canvasPad: 3,
        spriteExpand: 1.0
    };

    const linePos = [];
    function seg(ax, ay, az, bx, by, bz) {
        linePos.push(ax, ay, az, bx, by, bz);
    }
    seg(ax, yr, az, ax + extentGeom, yr, az);
    /** Brazo vertical en planta: crece en -Z (arriba en TOP); etiquetas en metros negativos. */
    seg(xr, yr, az, xr, yr, az - extentGeom);

    const hTickMajor = tickMax * 0.5;
    const hTickMinor = tickMin * 0.5;
    /** Separación perpendicular al palo: más allá del trazo de marca, sin solapar esquina X/Z. */
    const labelGap = numWorldScale * 0.22;
    const labelOffZ = hTickMajor + labelGap;
    const labelOffX = hTickMajor + labelGap;

    for (let k = 1; k * minorStep <= extentGeom + 1e-6 * minorStep; k++) {
        const t = k * minorStep;
        const isMajor = maStlPlanRulerTickIsMajor(k, majorsBetween);
        const hTick = isMajor ? hTickMajor : hTickMinor;
        /** +X baseline: marcas en plano XZ (+Z), visibles en planta. */
        seg(ax + t, yr, az, ax + t, yr, az + hTick);
        /** -Z baseline: marcas en plano XZ (+X), hacia arriba en TOP. */
        seg(xr, yr, az - t, xr + hTick, yr, az - t);

        if (isMajor && t >= majorStep - 1e-6) {
            const labelPosM = formatMajor ? formatMajor(t) : String(Math.round(t));
            /** +X arm: una etiqueta por marca mayor, desplazada en +Z (fuera del trazo). */
            const lblX = maStlMakeTextSprite(labelPosM, numWorldScale, rulerNumSpriteOpts);
            if (lblX) {
                lblX.position.set(ax + t, yr, az + labelOffZ);
                root.add(lblX);
            }
            /** -Z arm: metros negativos creciendo hacia -Z (arriba en planta). */
            const labelNegM = formatMajor ? formatMajor(-t) : String(-Math.round(t));
            const lblZ = maStlMakeTextSprite(labelNegM, numWorldScale, rulerNumSpriteOpts);
            if (lblZ) {
                lblZ.position.set(xr + labelOffX, yr, az - t);
                root.add(lblZ);
            }
        }
    }

    if (linePos.length > 0) {
        const geo = new THREE.BufferGeometry();
        geo.setAttribute('position', new THREE.Float32BufferAttribute(new Float32Array(linePos), 3));
        const lines = new THREE.LineSegments(geo, lineMat);
        lines.renderOrder = 150;
        root.add(lines);
    }

    maStlDisableRaycastOnOverlay(root);
    return root;
}

/**
 * Punto de inserción `primary` en mundo (mm escena), tras {@link masterArticleStlGroundGroupOnWorkspaceFloor}.
 *
 * **Significado CAD / croquis (p. ej. Tinkercad):** origen en la **esquina inferior izquierda de la huella** del objeto
 * en planta (proyección horizontal del AABB mundo sobre el suelo), **no** el centro del rectángulo en X/Z ni el centro 3D.
 * El STL binario **no** aporta punto de inserción DWG/bloque; se deriva geométricamente del envolvente alineado a ejes mundo.
 *
 * **Convención Desing_2 en planta:** en {@link maStlBuildPlanRulers} el brazo −Z son las cotas «hacia arriba» en papel;
 * la parte inferior del dibujo corresponde a **+Z** mundo; la izquierda a **menor X** (el brazo +X crece hacia la derecha).
 * Por tanto la esquina **inferior-izquierda** de la huella es `(box.min.x, box.max.z)` en X/Z.
 *
 * **Modo reglas objeto (toolbar):** se calcula sobre la **pieza clicada**. Cada entrada de `clipStlMeshes`
 * se trata como un objeto STL distinto (p. ej. primario vs `*2.stl`): el AABB mundo de ese `THREE.Mesh` define el punto.
 *
 * **Modo reglas grupo completo:** el mismo proveedor aplicado sobre `THREE.Group` (raíz cargada).
 *
 * Fórmula (esquina huella inferior-izquierda en mundo XZ, Y suelo workspace):
 *   box = AABB mundo del objeto (mesh o grupo + hijos)
 *   x = box.min.x
 *   y = MA_STL_DESING2_WORKSPACE_FLOOR_Y_MM  (tras {@link masterArticleStlGroundGroupOnWorkspaceFloor}, box.min.y === floorY)
 *   z = box.max.z
 *
 * Ampliar con más proveedores en `maStlInsertionPointProviders` si hace falta (metadata CAD futura, otras esquinas).
 *
 * @param {THREE.Object3D} group Malla STL o grupo con geometrías
 * @returns {THREE.Vector3}
 */
function maStlGetInsertionPointBottomLeftFootprintWorld(group) {
    group.updateMatrixWorld(true);
    const box = new THREE.Box3().setFromObject(group);
    const y = MA_STL_DESING2_WORKSPACE_FLOOR_Y_MM;
    return new THREE.Vector3(box.min.x, y, box.max.z);
}

/**
 * @type {{ id: string, label: string, getWorldPosition: (group: THREE.Group) => THREE.Vector3 }[]}
 */
const maStlInsertionPointProviders = [
    {
        id: 'primary',
        label: 'Punto de inserción (esquina inferior izquierda, huella)',
        getWorldPosition: maStlGetInsertionPointBottomLeftFootprintWorld
    }
];

/**
 * @param {THREE.Group|null} group
 * @returns {{ id: string, label: string, position: THREE.Vector3 }[]}
 */
function maStlCollectInsertionPointsWorld(group) {
    if (!group) return [];
    return maStlInsertionPointProviders.map(function (provider) {
        return {
            id: provider.id,
            label: provider.label,
            position: provider.getWorldPosition(group)
        };
    });
}

/** Plano suelo workspace (normal +Y). */
const _maStlWorkspaceFloorPickPlane = new THREE.Plane(
    new THREE.Vector3(0, 1, 0),
    -MA_STL_DESING2_WORKSPACE_FLOOR_Y_MM
);
/** NDC desde rect del canvas (Vector2 — perspectiva Desing_2 requiere z implícito en setFromCamera). */
const _maStlFloorPickNdc = new THREE.Vector2();
const _maStlInsertionPickScreenNdc = new THREE.Vector3();
/**
 * Umbral mm (XZ) cursor→snap en intersección de rejilla minor (base configurada mm).
 * @param {number} maxDim
 * @param {THREE.Camera} camera
 * @param {THREE.Vector3} snapWorld
 */
function maStlGridIntersectionPickScreenThresholdPx(camera) {
    if (camera.isOrthographicCamera) {
        return THREE.MathUtils.clamp(
            MA_STL_GRID_INTERSECTION_PICK_SCREEN_PX_BASE / Math.max(camera.zoom, 0.1),
            32,
            96
        );
    }
    return MA_STL_GRID_INTERSECTION_PICK_SCREEN_PX_BASE;
}

function maStlGridIntersectionPickProximityThresholdMm(maxDim, camera, snapWorld, gridMinorMm) {
    const minor = gridMinorMm != null && gridMinorMm > 0 ? gridMinorMm : MA_STL_DESING2_GRID_MINOR_MM;
    const d = Math.max(maxDim, minor);
    const base = Math.max(minor * 0.58, d * 0.07);
    const camDist = Math.max(camera.position.distanceTo(snapWorld), 1e-3);
    const distFactor = THREE.MathUtils.clamp(camDist / Math.max(d * 0.75, 1e-3), 0.2, 3.2);
    let zoomFactor = 1;
    if (camera.isOrthographicCamera) {
        zoomFactor = THREE.MathUtils.clamp(1.45 / Math.max(camera.zoom, 0.1), 0.45, 5.5);
    } else if (camera.isPerspectiveCamera) {
        zoomFactor = THREE.MathUtils.clamp(camDist / (d * 0.95), 0.25, 3.5);
    }
    return base * distFactor * zoomFactor;
}

/**
 * Snap X/Z en planta a la intersección menor más cercana (mm escena; default 500 si no se indica; LOD no aplica aquí).
 * @param {number} floorX
 * @param {number} floorZ
 * @param {number} [minorMm]
 */
function maStlSnapFloorToGridIntersectionMm(floorX, floorZ, minorMm) {
    const step = minorMm > 0 ? minorMm : MA_STL_DESING2_GRID_MINOR_MM;
    return {
        snapX: Math.round(floorX / step) * step,
        snapZ: Math.round(floorZ / step) * step
    };
}

const _maStlGridSnapProximityWorld = new THREE.Vector3();
/** Delta target→anclaje al colocar reglas sin mover la vista (pan cámara + target). */
const _maStlOrbitPivotPanDelta = new THREE.Vector3();

/**
 * Snap en planta al cruce rejilla minor; con `proximity` evalúa si el cursor está lo bastante cerca.
 * @param {{ x: number, z: number }} floorHit Punto en suelo (mm escena).
 * @param {{ clientX: number, clientY: number, camera: THREE.Camera, canvas: HTMLElement, maxDim: number }} [proximity]
 * @param {number} [gridSnapMm] paso menor rejilla / snap pick (mm escena)
 * @returns {{ x: number, y: number, z: number, active: boolean }}
 */
function maStlSnapFloorToGridIntersection(floorHit, proximity, gridSnapMm) {
    const snap =
        gridSnapMm != null && gridSnapMm > 0 ? gridSnapMm : MA_STL_DESING2_GRID_MINOR_MM;
    const hit = maStlSnapFloorToGridIntersectionMm(floorHit.x, floorHit.z, snap);
    const y = MA_STL_DESING2_WORKSPACE_FLOOR_Y_MM;
    const result = { x: hit.snapX, y: y, z: hit.snapZ, active: false };
    if (!proximity || !proximity.camera || !proximity.canvas) {
        return result;
    }
    _maStlGridSnapProximityWorld.set(hit.snapX, y, hit.snapZ);
    const distXZ = Math.hypot(floorHit.x - hit.snapX, floorHit.z - hit.snapZ);
    const threshMm = maStlGridIntersectionPickProximityThresholdMm(
        proximity.maxDim,
        proximity.camera,
        _maStlGridSnapProximityWorld,
        snap
    );
    const screenPx = maStlInsertionPointScreenDistancePx(
        _maStlGridSnapProximityWorld,
        proximity.clientX,
        proximity.clientY,
        proximity.camera,
        proximity.canvas
    );
    const screenThreshPx = maStlGridIntersectionPickScreenThresholdPx(proximity.camera);
    const xzActive = distXZ <= Math.max(threshMm, snap * 0.52);
    result.active = xzActive || screenPx <= screenThreshPx;
    return result;
}

/**
 * Snap informativo en planta: cruces del retículo menor, puntos medios de aristas, centros de celda.
 * Misma regla de proximidad/activación que {@link maStlSnapFloorToGridIntersection}.
 * @param {{ x: number, z: number }} floorHit Punto en suelo (mm escena).
 * @param {{ clientX: number, clientY: number, camera: THREE.Camera, canvas: HTMLElement, maxDim: number, pickScreenPxBoost?: number }} [proximity] `pickScreenPxBoost`: px extra al umbral pantalla (p. ej. herramienta línea).
 * @param {number} [gridSnapMm]
 * @returns {{ x: number, y: number, z: number, active: boolean }}
 */
function maStlSnapFloorToGridFeatures(floorHit, proximity, gridSnapMm) {
    const step = gridSnapMm != null && gridSnapMm > 0 ? gridSnapMm : MA_STL_DESING2_GRID_MINOR_MM;
    const fx = floorHit.x;
    const fz = floorHit.z;
    const y = MA_STL_DESING2_WORKSPACE_FLOOR_Y_MM;
    let bestX = 0;
    let bestZ = 0;
    let bestD2 = Infinity;
    const ix0 = Math.floor(fx / step);
    const iz0 = Math.floor(fz / step);
    for (let di = -2; di <= 2; di++) {
        for (let dj = -2; dj <= 2; dj++) {
            const i = ix0 + di;
            const j = iz0 + dj;
            const xi = i * step;
            const zj = j * step;
            const candidates = [
                [xi, zj],
                [(i + 0.5) * step, zj],
                [xi, (j + 0.5) * step],
                [(i + 0.5) * step, (j + 0.5) * step]
            ];
            for (let ci = 0; ci < candidates.length; ci++) {
                const cx = candidates[ci][0];
                const cz = candidates[ci][1];
                const d2 = (fx - cx) * (fx - cx) + (fz - cz) * (fz - cz);
                if (d2 < bestD2) {
                    bestD2 = d2;
                    bestX = cx;
                    bestZ = cz;
                }
            }
        }
    }
    const result = { x: bestX, y: y, z: bestZ, active: false };
    if (!proximity || !proximity.camera || !proximity.canvas) {
        return result;
    }
    _maStlGridSnapProximityWorld.set(bestX, y, bestZ);
    const distXZ = Math.hypot(fx - bestX, fz - bestZ);
    const threshMm = maStlGridIntersectionPickProximityThresholdMm(
        proximity.maxDim,
        proximity.camera,
        _maStlGridSnapProximityWorld,
        step
    );
    const screenPx = maStlInsertionPointScreenDistancePx(
        _maStlGridSnapProximityWorld,
        proximity.clientX,
        proximity.clientY,
        proximity.camera,
        proximity.canvas
    );
    const boostPx =
        proximity.pickScreenPxBoost != null && proximity.pickScreenPxBoost > 0
            ? proximity.pickScreenPxBoost
            : 0;
    const screenThreshPx = maStlGridIntersectionPickScreenThresholdPx(proximity.camera) + boostPx;
    const xzActive = distXZ <= Math.max(threshMm, step * 0.52);
    result.active = xzActive || screenPx <= screenThreshPx;
    return result;
}

/**
 * @param {string} template `{0}` = X m, `{1}` = Z m (2 decimales)
 * @param {number} snapXMm
 * @param {number} snapZMm
 */
function maStlFormatRulerAnchorGridIntersectionToast(template, snapXMm, snapZMm) {
    const xM = (snapXMm / 1000).toFixed(2);
    const zM = (snapZMm / 1000).toFixed(2);
    return (template || '')
        .replace(/\{0\}/g, xM)
        .replace(/\{1\}/g, zM);
}

/**
 * Rayo pantalla (clientX/Y) → intersección exacta con plano suelo Y={@link MA_STL_DESING2_WORKSPACE_FLOOR_Y_MM}.
 * NDC desde `canvas.getBoundingClientRect()` (orto y perspectiva Desing_2). Sin offsets: (x, floorY, z).
 * @returns {boolean}
 */
function maStlClientRayToWorkspaceFloor(clientX, clientY, canvas, camera, ndcVec, raycaster, outPoint) {
    if (!canvas || !camera || !raycaster || !outPoint) return false;
    const rect = canvas.getBoundingClientRect();
    const rw = Math.max(rect.width, 1);
    const rh = Math.max(rect.height, 1);
    const ndcX = ((clientX - rect.left) / rw) * 2 - 1;
    const ndcY = -((clientY - rect.top) / rh) * 2 + 1;
    _maStlFloorPickNdc.set(ndcX, ndcY);
    if (ndcVec) {
        ndcVec.x = ndcX;
        ndcVec.y = ndcY;
    }
    camera.updateMatrixWorld(true);
    raycaster.setFromCamera(_maStlFloorPickNdc, camera);
    if (raycaster.ray.intersectPlane(_maStlWorkspaceFloorPickPlane, outPoint) === null) {
        return false;
    }
    outPoint.y = MA_STL_DESING2_WORKSPACE_FLOOR_Y_MM;
    return true;
}

/** Distancia en px del cursor al punto de inserción proyectado en pantalla. */
function maStlInsertionPointScreenDistancePx(worldPt, clientX, clientY, camera, canvas) {
    const rect = canvas.getBoundingClientRect();
    _maStlInsertionPickScreenNdc.copy(worldPt).project(camera);
    const pxX = rect.left + (_maStlInsertionPickScreenNdc.x * 0.5 + 0.5) * Math.max(rect.width, 1);
    const pxY = rect.top + (-_maStlInsertionPickScreenNdc.y * 0.5 + 0.5) * Math.max(rect.height, 1);
    return Math.hypot(clientX - pxX, clientY - pxY);
}

/** Recuadro en suelo (mm escena), centrado en el punto de inserción (pad = esquina huella inferior-izquierda). */
function maStlBuildInsertionPickHighlightRect(halfSizeMm, activeColor) {
    const root = new THREE.Group();
    root.renderOrder = 165;
    const hs = halfSizeMm > 0 ? halfSizeMm : 250;
    const y = 2.5;
    const mat = new THREE.LineBasicMaterial({
        color: activeColor != null ? activeColor : 0x00e676,
        transparent: true,
        opacity: 0.98,
        depthTest: false,
        depthWrite: false
    });
    const pos = [
        -hs, y, -hs, hs, y, -hs,
        hs, y, -hs, hs, y, hs,
        hs, y, hs, -hs, y, hs,
        -hs, y, hs, -hs, y, -hs
    ];
    const geo = new THREE.BufferGeometry();
    geo.setAttribute('position', new THREE.Float32BufferAttribute(new Float32Array(pos), 3));
    const loop = new THREE.LineLoop(geo, mat);
    loop.renderOrder = 165;
    root.add(loop);
    maStlDisableRaycastOnOverlay(root);
    return root;
}

/** Marca en suelo (cruz cyan) del anclaje de reglas Desing_2; origen local = anclaje (grupo en X/Z suelo). */
function maStlBuildRulerAnchorFloorMarker(armMm) {
    const root = new THREE.Group();
    root.renderOrder = 160;
    const r = armMm != null && armMm > 0 ? armMm : 220;
    const y = 0.8;
    const mat = new THREE.LineBasicMaterial({
        color: 0x00e5c0,
        transparent: true,
        opacity: 0.95,
        depthTest: false,
        depthWrite: false
    });
    const pos = [
        -r, y, 0, r, y, 0,
        0, y, -r, 0, y, r
    ];
    const geo = new THREE.BufferGeometry();
    geo.setAttribute('position', new THREE.Float32BufferAttribute(new Float32Array(pos), 3));
    const lines = new THREE.LineSegments(geo, mat);
    lines.renderOrder = 160;
    root.add(lines);
    maStlDisableRaycastOnOverlay(root);
    return root;
}

/**
 * Esfera roja en el cruce visual de los brazos (+X / −Z) sobre el anclaje; no se oculta con `#ma-stl-ucs-rulers-toggle`.
 */
function maStlBuildRulerAnchorIntersectBallMm(radiusMm) {
    const r = radiusMm > 0 ? radiusMm : MA_STL_DESING2_RULE_ANCHOR_BALL_RADIUS_MM;
    const geo = new THREE.SphereGeometry(r, 22, 16);
    const mat = new THREE.MeshBasicMaterial({
        color: 0xe53935,
        transparent: true,
        opacity: 0.96,
        depthTest: false,
        depthWrite: false,
        toneMapped: false
    });
    const mesh = new THREE.Mesh(geo, mat);
    mesh.name = 'maStlRulerAnchorIntersectBall';
    mesh.position.y = Math.min(r * 0.75, MA_STL_DESING2_GRID_MINOR_MM * 0.11);
    mesh.renderOrder = 162;
    mesh.frustumCulled = false;
    maStlDisableRaycastOnOverlay(mesh);
    return mesh;
}

/** Esfera cian/verde en vértice o punto medio de línea usuario (hover snap herramienta línea). */
function maStlBuildLineToolVertexSnapBallMm(radiusMm, colorHex, opacity) {
    const r = radiusMm > 0 ? radiusMm : MA_STL_LINE_TOOL_VERTEX_SNAP_BALL_RADIUS_MM;
    const geo = new THREE.SphereGeometry(r, 20, 14);
    const mat = new THREE.MeshBasicMaterial({
        color: colorHex != null ? colorHex : MA_STL_LINE_TOOL_VERTEX_SNAP_COLOR,
        transparent: true,
        opacity: opacity != null ? opacity : MA_STL_LINE_TOOL_VERTEX_SNAP_OPACITY_IDLE,
        depthTest: false,
        depthWrite: false,
        toneMapped: false
    });
    const mesh = new THREE.Mesh(geo, mat);
    mesh.name = 'maStlLineToolVertexSnapBall';
    mesh.position.y = Math.min(r * 0.72, MA_STL_DESING2_GRID_MINOR_MM * 0.1);
    mesh.renderOrder = 167;
    mesh.frustumCulled = false;
    maStlDisableRaycastOnOverlay(mesh);
    return mesh;
}

/** Relleno en planta (quad) para hover de cruce rejilla (modo pick reglas). */
function maStlBuildGridIntersectionPickHighlightFilled(halfSizeMm, colorHex, opacity) {
    const root = new THREE.Group();
    root.renderOrder = 166;
    const hs = halfSizeMm > 0 ? halfSizeMm : MA_STL_GRID_INTERSECTION_PICK_CELL_MM * 0.5;
    const geo = new THREE.PlaneGeometry(hs * 2, hs * 2);
    geo.rotateX(-0.5 * Math.PI);
    const mat = new THREE.MeshBasicMaterial({
        color: colorHex != null ? colorHex : MA_STL_GRID_INTERSECTION_PICK_HIGHLIGHT_COLOR,
        transparent: true,
        opacity: opacity != null ? opacity : MA_STL_GRID_INTERSECTION_PICK_HIGHLIGHT_OPACITY,
        depthTest: false,
        depthWrite: false,
        side: THREE.DoubleSide,
        polygonOffset: true,
        polygonOffsetFactor: -2,
        polygonOffsetUnits: -2
    });
    const mesh = new THREE.Mesh(geo, mat);
    mesh.position.y = MA_STL_DESING2_GRID_INTERSECTION_FLOOR_EPS_MM + 0.85;
    mesh.renderOrder = 167;
    mesh.frustumCulled = false;
    root.add(mesh);
    maStlDisableRaycastOnOverlay(root);
    return root;
}

/** Contorno en planta (idle) en el cruce rejilla más cercano. */
function maStlBuildGridIntersectionPickHighlightOutline(halfSizeMm, colorHex, opacity) {
    const root = new THREE.Group();
    root.renderOrder = 165;
    const hs = halfSizeMm > 0 ? halfSizeMm : MA_STL_GRID_INTERSECTION_PICK_CELL_MM * 0.5;
    const y = MA_STL_DESING2_GRID_INTERSECTION_FLOOR_EPS_MM + 0.15;
    const mat = new THREE.LineBasicMaterial({
        color: colorHex != null ? colorHex : MA_STL_GRID_INTERSECTION_PICK_IDLE_COLOR,
        transparent: true,
        opacity: opacity != null ? opacity : MA_STL_GRID_INTERSECTION_PICK_IDLE_OPACITY,
        depthTest: false,
        depthWrite: false
    });
    const pos = [
        -hs, y, -hs, hs, y, -hs,
        hs, y, -hs, hs, y, hs,
        hs, y, hs, -hs, y, hs,
        -hs, y, hs, -hs, y, -hs
    ];
    const geo = new THREE.BufferGeometry();
    geo.setAttribute('position', new THREE.Float32BufferAttribute(new Float32Array(pos), 3));
    const loop = new THREE.LineLoop(geo, mat);
    loop.renderOrder = 165;
    root.add(loop);
    maStlDisableRaycastOnOverlay(root);
    return root;
}

/**
 * Rejilla Desing_2 en perspectiva (Design-3d): `uDistance` según distancia cámara–target y FOV.
 * @param {THREE.Mesh} grid
 * @param {number} frustumHalfY referencia de encuadre (mm escena)
 * @param {number} aspect
 * @param {THREE.PerspectiveCamera} camera
 * @param {THREE.Vector3} orbitTarget
 */
function maStlSyncDesing2GridDistancePerspective(grid, frustumHalfY, aspect, camera, orbitTarget) {
    const gMat = grid && grid.material;
    if (!gMat || !gMat.uniforms || !gMat.uniforms.uDistance || !camera.isPerspectiveCamera) return;
    const dist = Math.max(camera.position.distanceTo(orbitTarget), 1e-3);
    const halfFov = THREE.MathUtils.degToRad(camera.fov * 0.5);
    const reach = dist * Math.tan(halfFov) * Math.max(aspect, 1e-6) * MA_STL_DESING2_GRID_REFIT_DISTANCE_PAD;
    const reachOrthoFloor = maStlOrthoReachMm(frustumHalfY, aspect, MA_STL_DESING2_MIN_ZOOM_FLOOR);
    gMat.uniforms.uDistance.value = Math.max(
        MA_STL_DESING2_GRID_DISTANCE_DESIGN3D,
        reach,
        reachOrthoFloor * MA_STL_DESING2_GRID_REFIT_DISTANCE_PAD
    );
}

/**
 * Rejilla Desing_2: LOD de celdas, suelo `fwidth` y grosor de línea según mm/px (orto o perspectiva).
 * @param {THREE.ShaderMaterial} mat
 * @param {THREE.Camera} camera
 * @param {THREE.WebGLRenderer} renderer
 * @param {number} desing2OrthoMinZoom referencia orto (`maStlDesing2MinZoomFromHalfY`); perspectiva lo ignora
 * @param {THREE.Vector3|null} [orbitTarget]
 * @param {number} [baseMinorMm]
 * @param {number} [baseMajorMm]
 */
function maStlSyncDesing2ScreenSpaceOverlay(
    mat,
    camera,
    renderer,
    desing2OrthoMinZoom,
    orbitTarget,
    baseMinorMm,
    baseMajorMm
) {
    if (!mat || !mat.uniforms) return;
    const u = mat.uniforms;
    const bm = baseMinorMm != null && baseMinorMm > 0 ? baseMinorMm : MA_STL_DESING2_GRID_MINOR_MM;
    const bM = baseMajorMm != null && baseMajorMm > 0 ? baseMajorMm : MA_STL_DESING2_GRID_MAJOR_MM;
    const wpp = maStlWorldMmPerPixel(camera, renderer, orbitTarget);
    const lod = maStlDesing2GridLodCellSizesMm(wpp, bm, bM);
    if (u.uSize1) u.uSize1.value = lod.minorMm;
    if (u.uSize2) u.uSize2.value = lod.majorMm;
    const cell = lod.minorMm;
    const rawNear = (0.055 * wpp) / Math.max(cell, 1e-9);
    let floor;
    if (wpp < MA_STL_DESING2_GRID_WPP_FAR_START_MM) {
        floor = THREE.MathUtils.clamp(rawNear, 1e-10, MA_STL_DESING2_GRID_FWIDTH_CAP_NEAR);
    } else {
        const rawFar = (0.18 * Math.sqrt(wpp)) / Math.max(cell, 1e-9);
        floor = THREE.MathUtils.clamp(rawFar, MA_STL_DESING2_GRID_FWIDTH_CAP_NEAR, MA_STL_DESING2_GRID_FWIDTH_CAP_FAR);
    }
    if (u.uFwidthFloor) u.uFwidthFloor.value = floor;
    if (u.uLineWidthScale) {
        const wppFar = THREE.MathUtils.clamp(wpp / MA_STL_DESING2_GRID_WPP_FAR_START_MM, 1, 2.8);
        const lodThin = 1 + (lod.lodMult - 1) * 0.22;
        let thickFactor = wppFar * lodThin;
        if (camera && camera.isOrthographicCamera) {
            const z = Math.max(camera.zoom, desing2OrthoMinZoom);
            thickFactor *= THREE.MathUtils.clamp(Math.sqrt(desing2OrthoMinZoom / z), 1, 1.5);
        }
        thickFactor = THREE.MathUtils.clamp(thickFactor, 1, 2.85);
        u.uLineWidthScale.value = MA_STL_DESING2_GRID_LINEWIDTH_BASE / thickFactor;
    }
    if (u.uOpacityMax) {
        const opFar = THREE.MathUtils.clamp(1.08 - wpp * 0.06 - lod.lodMult * 0.04, 0.38, 0.56);
        u.uOpacityMax.value = wpp < MA_STL_DESING2_GRID_WPP_FAR_START_MM ? 0.56 : opFar;
    }
    if (u.uFadeExponent && wpp >= MA_STL_DESING2_GRID_LOD_WPP_TIER3) {
        u.uFadeExponent.value = 3.6;
    } else if (u.uFadeExponent) {
        u.uFadeExponent.value = MA_STL_DESING2_GRID_FADE_EXPONENT_DESIGN3D;
    }
}

/**
 * Ángulo de la rosa SVG (grados): **Norte = mundo +Z** (cara FRONT del cubo).
 * Usa la dirección de vista de la **cámara activa** (`ortho` | `iso`) y tolera `camera.up`
 * en plantas/alzados; si la vista es casi vertical, cae a euler Y o vector en planta.
 */
function maStlSvgCompassDialRotationDeg(camera, orbitTarget) {
    camera.updateWorldMatrix(true, false);
    const d = new THREE.Vector3();
    camera.getWorldDirection(d);
    const flat = new THREE.Vector3(d.x, 0, d.z);
    if (flat.lengthSq() > 1e-12) {
        flat.normalize();
        return THREE.MathUtils.radToDeg(Math.atan2(flat.x, flat.z));
    }
    const horiz = new THREE.Vector3().subVectors(orbitTarget, camera.position);
    horiz.y = 0;
    if (horiz.lengthSq() > 1e-12) {
        horiz.normalize();
        return THREE.MathUtils.radToDeg(Math.atan2(horiz.x, horiz.z));
    }
    const e = new THREE.Euler().setFromQuaternion(camera.quaternion, 'YXZ');
    return THREE.MathUtils.radToDeg(-e.y);
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
                if (!m) return;
                if (m.map) m.map.dispose();
                if (m.dispose) m.dispose();
            });
        }
    });
    if (obj.parent) obj.parent.remove(obj);
}

/**
 * Color 1 desde `data-ma-text-color1` (hex #rgb / #rrggbb); inválido → negro.
 * @param {string|null|undefined} hexRaw
 * @returns {THREE.Color}
 */
function masterArticleStlTintColorFromDataHex(hexRaw) {
    const c = new THREE.Color();
    const hex = (hexRaw || '').trim();
    if (/^#[0-9a-fA-F]{3}$/.test(hex) || /^#[0-9a-fA-F]{6}$/.test(hex)) {
        try {
            c.setStyle(hex);
        } catch (e) {
            c.setStyle('#000000');
        }
    } else {
        c.setStyle('#000000');
    }
    return c;
}

/**
 * URL del STL secundario convención `{{stem}}2.stl` (p. ej. `27104219P.stl` → `27104219P2.stl`).
 * Conserva sufijos tras `.stl` (p. ej. query string) si los hubiera.
 * @param {string|null|undefined} primaryUrl
 * @returns {string|null}
 */
function masterArticleStlSecondaryUrlFromPrimary(primaryUrl) {
    const u = (primaryUrl || '').trim();
    if (!u) return null;
    const lower = u.toLowerCase();
    const idx = lower.lastIndexOf('.stl');
    if (idx < 0) return null;
    return u.slice(0, idx) + '2' + u.slice(idx);
}

/**
 * Factor **vértice en unidad de archivo → metros** (intermedio). En Desing_2 se multiplica además por
 * {@link MA_STL_SCENE_MM_PER_PHYSICAL_METER} en `stlVertexToSceneScale` para obtener **mm de escena**.
 * Prioridad: `data-ma-stl-unit-to-meters` (número &gt; 0); si no, `data-ma-stl-source-units`.
 * Sin atributos → **1** (el archivo se interpreta ya en metros ante el factor anterior).
 * @param {HTMLElement | null} shell `#ma-stl-viewer-shell`
 * @returns {number}
 */
function maStlVertexUnitsToMetersScale(shell) {
    if (!shell) return 1;
    const rawExplicit = shell.getAttribute('data-ma-stl-unit-to-meters');
    if (rawExplicit != null && String(rawExplicit).trim() !== '') {
        const v = Number.parseFloat(String(rawExplicit).trim());
        if (Number.isFinite(v) && v > 0) return v;
    }
    const u = String(shell.getAttribute('data-ma-stl-source-units') || 'm')
        .trim()
        .toLowerCase();
    if (u === 'mm' || u === 'millimeter' || u === 'millimeters' || u === 'milimetro' || u === 'milimetros') {
        return 0.001;
    }
    if (u === 'cm' || u === 'centimeter' || u === 'centimeters' || u === 'centimetro' || u === 'centimetros') {
        return 0.01;
    }
    if (u === 'm' || u === 'meter' || u === 'meters' || u === 'metro' || u === 'metros') {
        return 1;
    }
    if (u === 'ft' || u === 'feet' || u === 'foot') return 0.3048;
    if (u === 'in' || u === 'inch' || u === 'inches' || u === 'pulgada' || u === 'pulgadas') return 0.0254;
    return 1;
}

/**
 * Luces de escena: Desing_2 replica Design-3d `createLight`; maestro mantiene ambient + 2× directional.
 * @returns {{ ambientLight: THREE.AmbientLight, mainDirLight: THREE.DirectionalLight, fillDirLight: THREE.DirectionalLight | null }}
 */
function maStlCreateSceneLights(scene, useDesing2Design3dLights) {
    if (useDesing2Design3dLights) {
        /* Desing_2 lights from Design-3d-three.js InitDesaint3d — revert by restoring maStlCreateLights branch */
        const mm = MA_STL_SCENE_MM_PER_PHYSICAL_METER;
        const ambientLight = new THREE.AmbientLight(
            MA_STL_DESING2_LIGHT_AMBIENT_COLOR,
            MA_STL_DESING2_LIGHT_AMBIENT_INTENSITY
        );
        scene.add(ambientLight);
        const mainDirLight = new THREE.DirectionalLight();
        mainDirLight.position.set(
            MA_STL_DESING2_LIGHT_SHADOW_DIR_POS_M[0] * mm,
            MA_STL_DESING2_LIGHT_SHADOW_DIR_POS_M[1] * mm,
            MA_STL_DESING2_LIGHT_SHADOW_DIR_POS_M[2] * mm
        );
        mainDirLight.castShadow = false;
        mainDirLight.shadow.mapSize.width = MA_STL_DESING2_LIGHT_SHADOW_MAP_SIZE;
        mainDirLight.shadow.mapSize.height = MA_STL_DESING2_LIGHT_SHADOW_MAP_SIZE;
        mainDirLight.shadow.camera.near = MA_STL_DESING2_LIGHT_SHADOW_NEAR_M * mm;
        mainDirLight.shadow.camera.far = MA_STL_DESING2_LIGHT_SHADOW_FAR_M * mm;
        scene.add(mainDirLight);
        scene.add(mainDirLight.target);
        mainDirLight.target.position.set(0, 0, 0);
        return { ambientLight, mainDirLight, fillDirLight: null };
    }

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
    scene.add(mainDirLight.target);
    const fillDirLight = new THREE.DirectionalLight(0xe2eaf8, 0.45);
    fillDirLight.position.set(-6, 2.5, -4);
    fillDirLight.castShadow = false;
    scene.add(fillDirLight);
    return { ambientLight, mainDirLight, fillDirLight };
}

/** Cookie global Desing_2: misma vista en cualquier diseño/oferta. */
const MA_STL_DESING2_VIEWER_COOKIE_GLOBAL = 'ma_stl_desing2_viewer_state_global';
const MA_STL_DESING2_VIEWER_COOKIE_LEGACY_BASE = 'ma_stl_desing2_viewer_state';
const MA_STL_DESING2_VIEWER_COOKIE_MAX_AGE_SEC = 30 * 24 * 60 * 60;
const MA_STL_DESING2_VIEWER_COOKIE_MAX_BYTES = 3800;
const MA_STL_DESING2_VIEWER_STATE_VERSION = 1;

/**
 * Nombre cookie legado por oferta/diseño (solo lectura para migrar estado guardado).
 * @returns {string|null}
 */
function maStlDesing2LegacyPerDesignViewerCookieName() {
    const params = new URLSearchParams(window.location.search);
    const offerId = (params.get('offerId') || '').trim();
    const designId = (params.get('designId') || '').trim();
    if (!offerId && !designId) return null;
    return (
        MA_STL_DESING2_VIEWER_COOKIE_LEGACY_BASE +
        '_o' +
        (offerId || '0') +
        '_d' +
        (designId || '0')
    );
}

/**
 * @param {string} name
 * @returns {string|null}
 */
function maStlReadCookie(name) {
    if (!name) return null;
    const prefix = name + '=';
    const parts = document.cookie ? document.cookie.split(';') : [];
    for (let i = 0; i < parts.length; i++) {
        const chunk = parts[i].trim();
        if (!chunk.startsWith(prefix)) continue;
        const raw = chunk.slice(prefix.length);
        if (!raw) return null;
        try {
            return decodeURIComponent(raw.replace(/\+/g, ' '));
        } catch (_e) {
            return raw;
        }
    }
    return null;
}

/**
 * @param {string} name
 * @param {string} value
 * @param {number} maxAgeSec
 * @returns {boolean}
 */
function maStlWriteCookie(name, value, maxAgeSec) {
    if (!name || value == null) return false;
    const encoded = encodeURIComponent(value);
    if (encoded.length > MA_STL_DESING2_VIEWER_COOKIE_MAX_BYTES) return false;
    document.cookie =
        name +
        '=' +
        encoded +
        '; path=/; max-age=' +
        String(Math.max(0, maxAgeSec | 0)) +
        '; SameSite=Lax';
    return maStlReadCookie(name) === value;
}

/**
 * @returns {object|null}
 */
function maStlDesing2ReadViewerStateFromCookie() {
    const legacyPerDesign = maStlDesing2LegacyPerDesignViewerCookieName();
    const names = [MA_STL_DESING2_VIEWER_COOKIE_GLOBAL, legacyPerDesign, MA_STL_DESING2_VIEWER_COOKIE_LEGACY_BASE].filter(
        Boolean
    );
    for (let i = 0; i < names.length; i++) {
        const parsed = maStlDesing2ParseViewerStateCookie(maStlReadCookie(names[i]));
        if (parsed) return parsed;
    }
    return null;
}

/**
 * @param {string} raw
 * @returns {object|null}
 */
function maStlDesing2ParseViewerStateCookie(raw) {
    if (!raw) return null;
    try {
        const data = JSON.parse(raw);
        if (!data || typeof data !== 'object' || data.v !== MA_STL_DESING2_VIEWER_STATE_VERSION) {
            return null;
        }
        return data;
    } catch (_e) {
        return null;
    }
}

/**
 * @param {THREE.Camera} cam
 * @returns {{ position: number[], up: number[], zoom?: number }}
 */
function maStlSerializeCameraState(cam) {
    const snap = {
        position: [cam.position.x, cam.position.y, cam.position.z],
        up: [cam.up.x, cam.up.y, cam.up.z]
    };
    if (cam.isOrthographicCamera && Number.isFinite(cam.zoom)) {
        snap.zoom = cam.zoom;
    }
    return snap;
}

/**
 * @param {THREE.Camera} cam
 * @param {{ position?: number[], up?: number[], zoom?: number } | null | undefined} snap
 * @param {{ skipLookAt?: boolean, lookAt?: THREE.Vector3 }=} opts
 */
function maStlApplyCameraState(cam, snap, opts) {
    if (!snap || !Array.isArray(snap.position) || snap.position.length < 3) return;
    cam.position.set(snap.position[0], snap.position[1], snap.position[2]);
    if (Array.isArray(snap.up) && snap.up.length >= 3) {
        cam.up.set(snap.up[0], snap.up[1], snap.up[2]);
    }
    if (cam.isOrthographicCamera && Number.isFinite(snap.zoom)) {
        cam.zoom = snap.zoom;
    }
    if (!opts || !opts.skipLookAt) {
        const la = opts && opts.lookAt;
        if (la && la.isVector3) {
            cam.lookAt(la);
        } else {
            cam.lookAt(0, 0, 0);
        }
    }
    cam.updateProjectionMatrix();
}

function bootMasterArticleDetailsStlViewer() {
    const canvasHost = document.getElementById('ma-stl-viewer-gl-host');
    const statusEl = document.getElementById('master-article-details-stl-viewer-status');
    if (!canvasHost) return;

    const viewerShell = document.getElementById('ma-stl-viewer-shell');
    const maStlViewerCanvasHudWrapEl =
        viewerShell instanceof Element ? viewerShell.querySelector('#master-article-details-stl-viewer-canvas') : null;
    const stlMeshTintColor = masterArticleStlTintColorFromDataHex(
        viewerShell ? viewerShell.getAttribute('data-ma-text-color1') : null
    );
    const stlMeshTintColor2 = masterArticleStlTintColorFromDataHex(
        viewerShell ? viewerShell.getAttribute('data-ma-text-color2') : null
    );

    /**
     * Shell del visor STL en Desing_2 (reglas, UCS extendido, compás, escala a mm escena desde unidad de archivo).
     */
    const maStlDesingV2Viewer =
        !!(viewerShell && viewerShell.getAttribute('data-ma-stl-show-rulers-toggle') === 'true');

    /** Solo Desing_2: STL mm/cm → m. Maestro: siempre 1. */
    /** Solo Desing_2: `maStlVertexUnitsToMetersScale` × {@link MA_STL_SCENE_MM_PER_PHYSICAL_METER} ⇒ mm escena. Maestro: 1 (archivo=tal cual). */
    const stlVertexToSceneScale = maStlDesingV2Viewer
        ? maStlVertexUnitsToMetersScale(viewerShell) * MA_STL_SCENE_MM_PER_PHYSICAL_METER
        : 1;

    const maStlUcsRulersToggleBtn = document.getElementById('ma-stl-ucs-rulers-toggle');
    const maStlXyzAxesToggleBtn = document.getElementById('ma-stl-xyz-axes-toggle');
    const maStlRulersGate =
        !!(maStlUcsRulersToggleBtn &&
            viewerShell &&
            viewerShell.getAttribute('data-ma-stl-show-rulers-toggle') === 'true');
    const maStlXyzAxesGate = !!(maStlXyzAxesToggleBtn && maStlDesingV2Viewer);

    /** Cookie leída al arranque; se aplica una vez tras el primer refit (o al boot sin STL). */
    let pendingDesing2Restore = maStlDesingV2Viewer ? maStlDesing2ReadViewerStateFromCookie() : null;
    /** Tras restaurar, refits posteriores (p. ej. `*2.stl`) no deben resetear cámara con `placeCamerasForModel`. */
    let maStlDesing2StateRestored = false;
    let maStlDesing2RestoringViewerState = false;

    /** Desing_2: reglas y ejes XYZ con toggles independientes; visibles al cargar. */
    let maStlUcsRulersManualOn = true;
    let maStlXyzAxesManualOn = true;

    /** Desing_2: anclaje de reglas en mm escena (X/Z desde pick; Y = suelo workspace). */
    const maStlRulerAnchorMm = new THREE.Vector3(0, MA_STL_DESING2_WORKSPACE_FLOOR_Y_MM, 0);
    if (pendingDesing2Restore && pendingDesing2Restore.rulerAnchor && typeof pendingDesing2Restore.rulerAnchor === 'object') {
        const ra = pendingDesing2Restore.rulerAnchor;
        if (Number.isFinite(ra.x) && Number.isFinite(ra.z)) {
            maStlRulerAnchorMm.set(
                ra.x,
                Number.isFinite(ra.y) ? ra.y : MA_STL_DESING2_WORKSPACE_FLOOR_Y_MM,
                ra.z
            );
        }
    }
    /** Pick anclaje reglas Desing_2: `null` ninguno | `grid` snap cruces rejilla menor (mm, Entorno) | `object` clic en STL → inserción. */
    let maStlRulerAnchorPickMode = null;
    const maStlRulerAnchorPickToggleBtn = document.getElementById('ma-stl-ruler-anchor-pick-toggle');
    const maStlRulerAnchorObjectPickToggleBtn = document.getElementById(
        'ma-stl-ruler-anchor-object-pick-toggle'
    );
    const maStlRulerAnchorCoordsHud = document.getElementById('ma-stl-ruler-anchor-coords-hud');
    const maStlLineToolHud = document.getElementById('ma-stl-line-tool-hud');
    const maStlLineToolHudInstruction = document.getElementById('ma-stl-line-tool-hud-instruction');
    const maStlLineToolHudCoords = document.getElementById('ma-stl-line-tool-hud-coords');
    const maStlLineToolHudDistanceRow = document.getElementById('ma-stl-line-tool-hud-distance-row');
    const maStlLineToolHudDistanceInput = document.getElementById('ma-stl-line-tool-hud-distance');
    const maStlLineToolHudDistancePreview = document.getElementById('ma-stl-line-tool-hud-distance-preview');
    const maStlLineToolToggleBtn = document.getElementById('ma-stl-tool-line');
    const maStlLineToolOrtho15ToggleBtn = document.getElementById('ma-stl-tool-ortho-15');

    /** Desing_2 herramienta línea: `null` | `picking1` | `picking2` (varios segmentos hasta Escape o clic vacío en `picking1`). */
    let maStlLineToolState = null;
    /** Snap dirección P2 en planta a múltiplos de 15° (0° = +X); predeterminado encendido (estilo CAD). */
    let maStlLineToolOrtho15Enabled = true;
    const maStlLineToolPoint1Mm = new THREE.Vector3();
    const maStlLineToolLastPointerClientXY = new THREE.Vector2(Number.NaN, Number.NaN);
    /** Dirección planar en último hover válido: componente X en `.x`, **Z mundo** en `.y` (no es altura Y). */
    const maStlLineToolLastHoverDirUnitXz = new THREE.Vector2(1, 0);
    const maStlLineToolTypedEndRubberMm = new THREE.Vector3();
    let maStlLineToolDistanceTypeBuffer = '';
    /** @type {THREE.Line|null} */
    let maStlLineToolRubberBandLine = null;
    /** Cotas CAD + overlay DOM en vivo durante `picking2` (P1→cursor/caucho). */
    let maStlLineToolPreviewDimActive = false;
    const maStlLineToolPreviewDimUd = {
        id: -1,
        p1Mm: { x: 0, y: 0, z: 0 },
        p2Mm: { x: 0, y: 0, z: 0 },
    };
    const _maStlLineToolRubberEndMm = { x: 0, y: 0, z: 0 };

    function maStlIsLineToolPlacementActive() {
        return maStlLineToolState === 'picking1' || maStlLineToolState === 'picking2';
    }

    function maStlIsRulerAnchorPickModeActive() {
        return maStlRulerAnchorPickMode !== null;
    }

    let maStlGridIntersectionNearActive = false;
    /** @type {{ enabled: boolean; enableRotate: boolean; enablePan: boolean; enableZoom: boolean }|null} */
    let maStlRulerAnchorPickOrbitLockSnapshot = null;
    /** @type {((ev?: PointerEvent) => void) | null} Handler window pointerup/pointercancel; véase maStlExitRulerAnchorPickAfterPlacement. */
    let maStlDeferredRulerPickUnlockPointerEnded = null;

    /**
     * Desing_2: con pick-lock (`enableRotate` off) el usuario puede panear; entonces omitir indefinidamente
     * {@link maStlApplyRulerAnchorOrbitPivotPreserveView()} hasta nuevo anclaje/cubo/refit/bind (evita salto inicial y en rotaciones posteriores).
     */
    let maStlDesing2OrbitDeferRulerPivotPreserveOnNextSync = false;
    /**
     * Desing_2: sólo TRUE justo después de colocar anclaje (rejilla/objeto). El primer LMB-rotate ejecuta
     * {@link maStlApplyRulerAnchorOrbitPivotPreserveView} para alinear `controls.target` con `maStlRulerAnchorMm`.
     * Tras cualquier navegación normal (paneo orbita derecho/consola), debe quedar FALSE para orbitar sobre el pivote actual
     * (evita saltar al estado previo al pan). Ver docs desing-2-orbit-pivot.md.
     */
    let maStlDesing2OrbitPreserveRulerPivotOnRotatePointerDown = false;
    /** Target al iniciar sesión pick-lock (referencia para detectar traslación del pivote por pan). */
    const _maStlPickLockOrbitTargetBaseline = new THREE.Vector3();
    /** Anclaje de reglas al iniciar la misma sesión pick-lock (saber si colocación grid/objeto lo movió). */
    const _maStlPickLockRulerAnchorStartMm = new THREE.Vector3();
    /** @type {(() => void) | null} */
    let _maStlDesing2OrbitPickLockChangeHandler = null;
    /** @type {((ev: KeyboardEvent) => void) | null} */
    let _maStlDesingV2EscapeKeydownHandler = null;
    /** @type {((ev: KeyboardEvent) => void) | null} */
    let _maStlDesingV2F8OrthoKeydownHandler = null;
    /**
     * `picking2` línea: handler keydown paralelo Escape (no pisar cotas STL / inputs página).
     * @type {((ev: KeyboardEvent) => void) | null}
     */
    let _maStlDesingV2LineToolDistanceKeyHandler = null;
    /** @type {{ clientX: number; clientY: number; orbitTargetX: number; orbitTargetY: number; orbitTargetZ: number } | null} */
    let _maStlUserFloorLineRefactorRmbGesture = null;
    const _maStlUserFloorLineRefactorRmbOrbitBaseline = new THREE.Vector3();
    const _maStlGridIntersectionSnapMm = new THREE.Vector3(
        0,
        MA_STL_DESING2_WORKSPACE_FLOOR_Y_MM,
        0
    );
    /** @type {{ mesh: THREE.Mesh, color: THREE.Color, emissive: THREE.Color, emissiveIntensity: number }[]} */
    let maStlPickHoverMaterialSnapshots = [];
    const _maStlPickHoverColor = new THREE.Color(0x3d8bfd);

    const maStlEntornoGridSnapSelect = document.getElementById('ma-stl-entorno-grid-snap-mm');
    const maStlEntornoRulerExtentSelect = document.getElementById('ma-stl-entorno-ruler-extent-m');

    /** Valores admitidos rejilla/pick snap (mm escena ≡ mm físicos con `MA_STL_SCENE_MM_PER_PHYSICAL_METER`). */
    function maStlClampAllowedDesing2GridSnapMm(mm) {
        const allowed = [50, 100, 250, 500, 1000, 2000];
        const n = Number(mm);
        if (!Number.isFinite(n) || n <= 0) return MA_STL_DESING2_GRID_MINOR_MM;
        let best = allowed[0];
        let bestD = Infinity;
        for (let i = 0; i < allowed.length; i++) {
            const d = Math.abs(allowed[i] - n);
            if (d < bestD) {
                bestD = d;
                best = allowed[i];
            }
        }
        return best;
    }

    /** Tope físico visible de brazos de regla → mm escena. */
    function maStlDesing2RulerExtentCapFromMeters(m) {
        const x = Number(m);
        if (!Number.isFinite(x)) return MA_STL_DESING2_RULE_FIXED_EXTENT_MM;
        return THREE.MathUtils.clamp(x, 5, 80) * MA_STL_SCENE_MM_PER_PHYSICAL_METER;
    }

    let desing2EnvGridSnapMm = MA_STL_DESING2_GRID_MINOR_MM;
    let desing2EnvRulerExtentCapMm = MA_STL_DESING2_RULE_FIXED_EXTENT_MM;
    function desing2EnvGridMajorMm() {
        return desing2EnvGridSnapMm * 5;
    }

    function desing2EnvSyncRulerExtentSelectToCapMm(capMmScene) {
        if (!(maStlEntornoRulerExtentSelect instanceof HTMLSelectElement)) return;
        const tgt = THREE.MathUtils.clamp(
            capMmScene / MA_STL_SCENE_MM_PER_PHYSICAL_METER,
            5,
            80
        );
        let bestVal = '';
        let bestDelta = Infinity;
        const sel = maStlEntornoRulerExtentSelect;
        for (let oi = 0; oi < sel.options.length; oi++) {
            const ov = Number.parseFloat(sel.options[oi].value);
            if (!Number.isFinite(ov)) continue;
            const dd = Math.abs(ov - tgt);
            if (dd < bestDelta) {
                bestDelta = dd;
                bestVal = sel.options[oi].value;
            }
        }
        if (bestVal !== '') sel.value = bestVal;
    }

    const _desingHasPendingEnvRestore =
        !!(
            pendingDesing2Restore &&
            pendingDesing2Restore.environment &&
            typeof pendingDesing2Restore.environment === 'object'
        );

    if (_desingHasPendingEnvRestore) {
        const ei = pendingDesing2Restore.environment;
        if (Number.isFinite(ei.gridSnapMm)) {
            desing2EnvGridSnapMm = maStlClampAllowedDesing2GridSnapMm(ei.gridSnapMm);
        }
        if (Number.isFinite(ei.rulerExtentCapM)) {
            desing2EnvRulerExtentCapMm = maStlDesing2RulerExtentCapFromMeters(ei.rulerExtentCapM);
        }
    } else if (maStlDesingV2Viewer && maStlEntornoGridSnapSelect instanceof HTMLSelectElement) {
        desing2EnvGridSnapMm = maStlClampAllowedDesing2GridSnapMm(maStlEntornoGridSnapSelect.value);
    }
    if (!_desingHasPendingEnvRestore && maStlDesingV2Viewer && maStlEntornoRulerExtentSelect instanceof HTMLSelectElement) {
        desing2EnvRulerExtentCapMm = maStlDesing2RulerExtentCapFromMeters(
            maStlEntornoRulerExtentSelect.value
        );
    }
    if (maStlDesingV2Viewer && maStlEntornoGridSnapSelect instanceof HTMLSelectElement) {
        maStlEntornoGridSnapSelect.value = String(desing2EnvGridSnapMm);
        desing2EnvGridSnapMm = maStlClampAllowedDesing2GridSnapMm(maStlEntornoGridSnapSelect.value);
    }
    if (maStlDesingV2Viewer && maStlEntornoRulerExtentSelect instanceof HTMLSelectElement) {
        desing2EnvSyncRulerExtentSelectToCapMm(desing2EnvRulerExtentCapMm);
        desing2EnvRulerExtentCapMm = maStlDesing2RulerExtentCapFromMeters(
            maStlEntornoRulerExtentSelect.value
        );
    }

    let currentRoot = null;
    let loadToken = 0;
    /** Última extensión del modelo (para distancia de cámara en vistas del dado). Desing_2: baseline ~12 m en mm escena. */
    let lastMaxDim = maStlRulersGate ? maStlDesing2EmptyBaselineDimMm() : 1;
    /** Dimensión de referencia overlays (rejilla/reglas/UCS): no encoge workspace Desing_2 por celdas mayores/menores. */
    function desing2WorkspaceOverlayDim() {
        if (!maStlRulersGate) return lastMaxDim;
        return Math.max(lastMaxDim, desing2EnvGridMajorMm() * 8);
    }
    /** Half-height of ortho frustum in world units (before camera.zoom). */
    let frustumHalfY = maStlFrustumHalfYFromMaxDim(
        desing2WorkspaceOverlayDim(),
        maStlRulersGate,
        desing2EnvGridMajorMm()
    );
    /** Referencia de rejilla al zoom mínimo de encuadre; OrbitControls usa {@link MA_STL_DESING2_MIN_ZOOM_FLOOR}. */
    let desing2OrthoMinZoom = maStlDesing2MinZoomFromHalfY(frustumHalfY);
    let lastAspect = 1;
    let controls = null;
    /** @type {'ortho' | 'iso'} */
    let activeMode = 'ortho';
    /** Recorte local (planos mundo): barra vertical → corte Y; horizontal → X. Valor 0–1000 en UI; fracción f = (1000−v)/1000 ∈ [0,1]: f=0 sin recorte, f=1 máximo. */
    const clipBounds = { min: new THREE.Vector3(), max: new THREE.Vector3() };
    const clipPlaneY = new THREE.Plane();
    const clipPlaneX = new THREE.Plane();
    /** Mallas STL con recorte / sombra (primera = principal, opcional segunda = `*2.stl`). */
    /** @type {THREE.Mesh[]} */
    let clipStlMeshes = [];
    /** @type {HTMLInputElement | null} */
    const clipInputY = document.getElementById('ma-stl-clip-y');
    /** @type {HTMLInputElement | null} */
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

    /*
     * VIEW CUBE 90° — DO NOT REGRESS: see desing-2-orbit-pivot.md
     * Caras = ejes ±X/±Y/±Z; aristas/esquinas = diagonales 45°. camera.up siempre (0,1,0) para OrbitControls.
     */
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

    /* VIEW CUBE 90° — DO NOT REGRESS: see desing-2-orbit-pivot.md */
    /**
     * Orienta cámara mirando al anclaje desde `dir` (vistas 90° en caras, 45° en aristas/esquinas).
     * `camera.up` permanece (0,1,0): OrbitControls deriva phi/theta en espacio Y-up; up en ±Z rompe TOP/BOTTOM.
     * No tocar `controls.target` aquí — el llamador encadena `bindControls` → `maStlFinalizeViewCubePreset`.
     */
    function applyDirectionToOrthoCam(camera, dir) {
        const anchor = maStlRulerAnchorMm;
        const d = viewDistanceFromModel();
        const p = dir.clone();
        if (p.lengthSq() < 1e-12) return;
        p.normalize().multiplyScalar(d);
        camera.up.set(0, 1, 0);
        camera.position.copy(anchor).add(p);
        camera.lookAt(anchor);
        if (camera.isOrthographicCamera) {
            camera.zoom = 1;
        }
        camera.updateProjectionMatrix();
    }

    /* VIEW CUBE 90° — DO NOT REGRESS: see desing-2-orbit-pivot.md */
    /**
     * Tras preset del cubo: anclaje = target, `update()` sincroniza esfericas internas, `saveState()` fija baseline.
     */
    function maStlFinalizeViewCubePreset() {
        if (!controls) return;
        maStlClearDesing2OrbitDeferRulerPivotPreserve();
        maStlClearDesing2OrbitPreserveRulerPivotOnRotatePointerDown();
        maStlResetOrbitTargetToRulerAnchor();
        controls.object.updateMatrixWorld(true);
        controls.update();
        if (typeof controls.saveState === 'function') {
            controls.saveState();
        }
    }

    function applyOrthoDirection(dir) {
        applyDirectionToOrthoCam(cameraOrtho, dir);
    }

    function applyOrthoDataView(viewKey) {
        const dir = ORTHO_VIEW_DIR[viewKey];
        if (!dir) return;
        activeMode = 'ortho';
        syncCameraRadios();
        applyOrthoDirection(dir);
        bindControls(cameraOrtho);
        maStlFinalizeViewCubePreset();
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
        applyDirectionToOrthoCam(cameraIso, dir);
        bindControls(cameraIso);
        maStlFinalizeViewCubePreset();
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

    /* Rejilla: minor/major configurables Entorno (default 500/2500 mm); LOD en `onBeforeRender`; maestro ctor compacto. */
    const infiniteGrid = maStlRulersGate
        ? new InfiniteGridHelper(
              desing2EnvGridSnapMm,
              desing2EnvGridMajorMm(),
              new THREE.Color(0x00b8dc),
              MA_STL_DESING2_GRID_DISTANCE_DESIGN3D,
              2.35,
              0.56
          )
        : new InfiniteGridHelper(
              8,
              32,
              new THREE.Color(0x00b8dc),
              500,
              MA_STL_DESING2_GRID_LINEWIDTH_BASE,
              0.56
          );
    maStlSyncInfiniteGridWorkspace(
        infiniteGrid,
        desing2WorkspaceOverlayDim(),
        maStlRulersGate,
        frustumHalfY,
        lastAspect,
        maStlRulersGate ? MA_STL_DESING2_MIN_ZOOM_FLOOR : desing2OrthoMinZoom,
        desing2EnvGridSnapMm,
        desing2EnvGridMajorMm()
    );
    scene.add(infiniteGrid);

    /** Origen: UCS clásico (maestro) o ejes XYZ verdes (Desing_2) + reglas plano (Desing_2, toggle aparte). */
    const maStlUcsAxesGroup = new THREE.Group();
    maStlUcsAxesGroup.renderOrder = 150;
    const maStlXyzAxesGroup = new THREE.Group();
    maStlXyzAxesGroup.renderOrder = 150;
    const maStlRulersGroup = new THREE.Group();
    maStlRulersGroup.renderOrder = 150;
    /** Marca visual del anclaje de reglas (solo Desing_2). */
    const maStlRulerAnchorMarkerGroup = new THREE.Group();
    maStlRulerAnchorMarkerGroup.renderOrder = 160;
    /** Recuadro de proximidad al punto de inserción (modo pick, solo Desing_2). */
    const maStlInsertionPickHighlightGroup = new THREE.Group();
    maStlInsertionPickHighlightGroup.renderOrder = 165;
    maStlInsertionPickHighlightGroup.visible = false;
    /** Recuadro cyan en intersección rejilla (modo pick reglas; celda ≈ snap Entorno). */
    const maStlGridIntersectionPickHighlightGroup = new THREE.Group();
    maStlGridIntersectionPickHighlightGroup.renderOrder = 166;
    maStlGridIntersectionPickHighlightGroup.visible = false;
    /** @type {{ idle: THREE.Object3D|null, connected: THREE.Object3D|null }} */
    const maStlGridIntersectionPickMeshes = { idle: null, connected: null };
    /** Segmentos dibujados por la herramienta línea (suelo); persisten para borrado futuro. */
    const maStlUserLinesGroup = new THREE.Group();
    maStlUserLinesGroup.renderOrder = 168;
    /** Esfera snap vértice/medio línea existente (herramienta línea, Desing_2). */
    const maStlLineToolVertexSnapHighlightGroup = new THREE.Group();
    maStlLineToolVertexSnapHighlightGroup.renderOrder = 167;
    maStlLineToolVertexSnapHighlightGroup.visible = false;
    let maStlLineToolVertexSnapBallMesh = null;
    const _maStlLineToolVertexSnapMm = new THREE.Vector3();
    const _maStlLineToolVertexSnapWorldScratch = new THREE.Vector3();
    /** Cotas CAD en planta (+ overlay HTML pantalla): fuera del grupo raycast-off de las líneas. */
    const maStlUserFloorLineDimHudGroup = new THREE.Group();
    maStlUserFloorLineDimHudGroup.renderOrder = 172;
    /** @type {THREE.LineSegments|null} */
    let maStlUserFloorDimGuideLinesMesh = null;
    /** Triángulos transparentes cotas CAD (flechas en extremos líneas cotas planta XZ). */
    let maStlUserFloorDimArrowMesh = null;
    /** @type {{ floorY:number, midLen:{x:number,z:number}|null,midDx:{x:number,z:number}|null,midDz:{x:number,z:number}|null, drawDx:boolean, drawDz:boolean, validLen:boolean, validDx:boolean, validDz:boolean, chLenA:{x:number,z:number}|null, chLenB:{x:number,z:number}|null, chLnA:{x:number,z:number}|null, chLnB:{x:number,z:number}|null, chDxA:{x:number,z:number}|null, chDxB:{x:number,z:number}|null, chDzA:{x:number,z:number}|null, chDzB:{x:number,z:number}|null }} */
    let maStlUserFloorDimHudWorldMid = {
        floorY: 0,
        midLen: null,
        midDx: null,
        midDz: null,
        drawDx: false,
        drawDz: false,
        validLen: false,
        validDx: false,
        validDz: false,
        chLenA: null,
        chLenB: null,
        chLnA: null,
        chLnB: null,
        chDxA: null,
        chDxB: null,
        chDzA: null,
        chDzB: null,
    };
    /** @type {'length'|'deltaX'|'deltaZ'|'all'} */
    let maStlUserFloorLineDimEditKind = 'length';
    /** @type {'length'|'deltaX'|'deltaZ'|null} */
    let maStlUserFloorDimReadoutHoveredKind = null;
    /** BBox pantalla cliente (left…bottom, valid). */
    const maStlUserFloorDimScrBoxLen = {
        valid: false,
        left: 0,
        top: 0,
        right: 0,
        bottom: 0,
    };
    const maStlUserFloorDimScrBoxDx = Object.assign({}, maStlUserFloorDimScrBoxLen);
    const maStlUserFloorDimScrBoxDz = Object.assign({}, maStlUserFloorDimScrBoxLen);
    /** Cliente `clientX/Y` proyectados midpoint readouts cotas triples. */
    const maStlFloorDimHudReadoutScrPx = {
        length: { x: 0, y: 0, valid: false },
        deltaX: { x: 0, y: 0, valid: false },
        deltaZ: { x: 0, y: 0, valid: false },
    };
    let maStlUserFloorDimGuideGeomCacheKey = '';
    /** Hover resaltado + cota midpoint (sólo Desing_2, sin modos herramienta activos). */
    let maStlHoveredUserFloorLine = null;
    let maStlUserFloorLineNextSegId = 1;
    let maStlUserFloorLineDimPickScreenPxValid = false;
    const maStlUserFloorLineDimPickScreenPx = { x: 0, y: 0 };
    /** Bbox readout cotas en px pantalla para pick canvas. */
    let maStlUserFloorLineDimPickScreenBBoxValid = false;
    const maStlUserFloorLineDimPickScreenBBox = { left: 0, top: 0, right: 0, bottom: 0 };
    /** ID timeout blur — debe cancelarse al dispose(skip) para que no ejecute commit con refs borradas. */
    let maStlUserFloorLineDimBlurTimerId = null;
    let maStlUserFloorDimDomHudEditing = false;
    /** HUD DOM cotas línea usuario (véase #ma-stl-line-dim-edit-overlay en parcial Vista). */
    let maStlUserFloorDimDomHud = null;
    let maStlUserFloorDimReadoutWire = false;
    let maStlUserFloorLineDimEditOverlay = null;
    let maStlUserFloorLineDimEditLineRef = null;
    /** Sin captura blur: función que quita blur listener del input activo. */
    let maStlUserFloorLineDimEditDispose = null;
    /** Suprime commit blur al pulsar la asa de arrastre (evita cerrar edición). */
    let maStlUserFloorLineDragSuppressBlurCommit = false;
    let maStlUserFloorLineDragActive = false;
    let maStlUserFloorLineDragHandleHovered = false;
    let maStlUserFloorLineDragHandleWire = false;
    /** Snapshot OrbitControls durante arrastre asa midpoint. */
    let maStlUserFloorLineDragOrbitSnapshot = null;
    const _maStlUserFloorLineDragFloorPt = new THREE.Vector3();
    const _maStlUserFloorLineDragStartFloor = new THREE.Vector3();
    const _maStlUserFloorLineDragOrigP1 = { x: 0, y: 0, z: 0 };
    const _maStlUserFloorLineDragOrigP2 = { x: 0, y: 0, z: 0 };
    const _maStlUserFloorLineProjScr = new THREE.Vector3();
    /** Wrap ×1000 (Desing_2): geometría reglas en m → mm escena. */
    let maStlRulersSceneWrap = null;
    /** @type {THREE.LineBasicMaterial|null} */
    let maStlOverlayLineMat = null;
    /** @type {LineMaterial|null} Persistente; no comparte tema cyan/gris de reglas. */
    let maStlUserFloorLineMat = null;

    function maStlEnsureUserFloorLineMat() {
        if (!maStlUserFloorLineMat) {
            maStlUserFloorLineMat = maStlMakeUserFloorLineMaterial();
        }
        return maStlUserFloorLineMat;
    }

    function maStlSyncAllUserFloorLineMaterialResolutions(widthPx, heightPx) {
        maStlApplyUserFloorLineMaterialResolution(maStlUserFloorLineMat, widthPx, heightPx);
        if (maStlUserLinesGroup) {
            for (let uli = 0; uli < maStlUserLinesGroup.children.length; uli++) {
                const o = maStlUserLinesGroup.children[uli];
                if (o.material) {
                    maStlApplyUserFloorLineMaterialResolution(o.material, widthPx, heightPx);
                }
            }
        }
        if (maStlLineToolRubberBandLine && maStlLineToolRubberBandLine.material) {
            maStlApplyUserFloorLineMaterialResolution(
                maStlLineToolRubberBandLine.material,
                widthPx,
                heightPx
            );
        }
    }
    /** Hijo opcional del grupo anclaje: cruz cyan, visible solo cuando reglas están en pantalla. */
    let maStlRulerAnchorCrossObject = null;
    /** Esfera roja del anclaje; permanece cuando se ocultan reglas (#ma-stl-ucs-rulers-toggle). */
    let maStlRulerAnchorBallMesh = null;

    function maStlStripOverlayMeshes(group) {
        if (!group) return;
        while (group.children.length > 0) {
            const ch = group.children[0];
            const disposedMats = new WeakSet();
            ch.traverse(function (c) {
                if (c.geometry) {
                    c.geometry.dispose();
                }
                const mats = Array.isArray(c.material) ? c.material : c.material ? [c.material] : [];
                mats.forEach(function (m) {
                    if (!m || disposedMats.has(m)) return;
                    disposedMats.add(m);
                    if (m.map) m.map.dispose();
                    if (m.dispose) m.dispose();
                });
            });
            group.remove(ch);
        }
    }

    function syncMaStlUcsOverlayVisibility() {
        if (maStlRulersGate) {
            maStlRulersGroup.visible = maStlUcsRulersManualOn;
            /* Grupo padre sigue visible: esfera de anclaje no depende del toggle de reglas. */
            maStlRulerAnchorMarkerGroup.visible = true;
            if (maStlRulerAnchorCrossObject) {
                maStlRulerAnchorCrossObject.visible = maStlUcsRulersManualOn;
            }
            if (maStlRulerAnchorBallMesh) {
                maStlRulerAnchorBallMesh.visible = true;
            }
            maStlXyzAxesGroup.visible = maStlXyzAxesGate ? maStlXyzAxesManualOn : false;
            maStlUcsAxesGroup.visible = false;
        } else {
            maStlUcsAxesGroup.visible = true;
            maStlXyzAxesGroup.visible = false;
            maStlRulersGroup.visible = false;
            maStlRulerAnchorMarkerGroup.visible = false;
        }
    }

    function syncMaStlUcsRulersToggleBtnUi() {
        if (!maStlUcsRulersToggleBtn) return;
        maStlUcsRulersToggleBtn.setAttribute('aria-pressed', maStlUcsRulersManualOn ? 'true' : 'false');
        maStlUcsRulersToggleBtn.classList.toggle('active', maStlUcsRulersManualOn);
    }

    function syncMaStlXyzAxesToggleBtnUi() {
        if (!maStlXyzAxesToggleBtn) return;
        maStlXyzAxesToggleBtn.setAttribute('aria-pressed', maStlXyzAxesManualOn ? 'true' : 'false');
        maStlXyzAxesToggleBtn.classList.toggle('active', maStlXyzAxesManualOn);
    }

    function maStlRebuildRulerAnchorMarker() {
        if (!maStlDesingV2Viewer) return;
        maStlStripOverlayMeshes(maStlRulerAnchorMarkerGroup);
        maStlRulerAnchorCrossObject = null;
        maStlRulerAnchorBallMesh = null;
        maStlRulerAnchorMarkerGroup.position.set(
            maStlRulerAnchorMm.x,
            MA_STL_DESING2_WORKSPACE_FLOOR_Y_MM,
            maStlRulerAnchorMm.z
        );
        maStlRulerAnchorCrossObject = maStlBuildRulerAnchorFloorMarker();
        maStlRulerAnchorMarkerGroup.add(maStlRulerAnchorCrossObject);
        maStlRulerAnchorBallMesh = maStlBuildRulerAnchorIntersectBallMm(MA_STL_DESING2_RULE_ANCHOR_BALL_RADIUS_MM);
        maStlRulerAnchorMarkerGroup.add(maStlRulerAnchorBallMesh);
    }

    function syncMaStlRulerPickToolbarUi() {
        if (maStlRulerAnchorPickToggleBtn) {
            maStlRulerAnchorPickToggleBtn.setAttribute(
                'aria-pressed',
                maStlRulerAnchorPickMode === 'grid' ? 'true' : 'false'
            );
            maStlRulerAnchorPickToggleBtn.classList.toggle('active', maStlRulerAnchorPickMode === 'grid');
        }
        if (maStlRulerAnchorObjectPickToggleBtn) {
            maStlRulerAnchorObjectPickToggleBtn.setAttribute(
                'aria-pressed',
                maStlRulerAnchorPickMode === 'object' ? 'true' : 'false'
            );
            maStlRulerAnchorObjectPickToggleBtn.classList.toggle('active', maStlRulerAnchorPickMode === 'object');
        }
    }

    function syncMaStlRulerAnchorPickCursor() {
        if (!renderer || !renderer.domElement) return;
        renderer.domElement.style.cursor = maStlIsRulerAnchorPickModeActive() ? 'crosshair' : '';
    }

    function maStlTeardownPickHighlightsOnly() {
        maStlClearInsertionPickHighlight();
        maStlClearGridIntersectionPickHighlight();
        maStlClearLineToolVertexSnapHighlight();
        maStlClearStlPickHoverHighlight();
    }

    /** Toolbar u otro disparador: cerrar modo pick sin defer (no hay clic en canvas cerrando gesto). */
    function maStlStopRulerAnchorPickModesToolbar() {
        if (!maStlIsRulerAnchorPickModeActive()) return;
        maStlRulerAnchorPickMode = null;
        maStlUnlockOrbitForRulerAnchorPick();
        maStlTeardownPickHighlightsOnly();
        syncMaStlRulerPickToolbarUi();
        syncMaStlRulerAnchorPickCursor();
    }

    /**
     * Desing_2: cancelar herramienta línea, modo pick rejilla/objeto y restos pick-lock/highlights visuales (p. ej. Escape).
     * @returns {boolean} true si había modo interactivo o pick-lock pendiente y se aplicó teardown
     */
    function maStlCancelAllViewerInteractionModes() {
        maStlDisposeUserFloorLineDimEdit(false);
        const hadLinePlacement = maStlIsLineToolPlacementActive();
        const hadPickMode = maStlIsRulerAnchorPickModeActive();
        let touched = !!(hadLinePlacement || hadPickMode);
        if (hadLinePlacement) {
            maStlStopLineToolModesToolbar(false);
        }
        if (hadPickMode) {
            maStlStopRulerAnchorPickModesToolbar();
        }
        if (maStlRulerAnchorPickOrbitLockSnapshot !== null || maStlDeferredRulerPickUnlockPointerEnded !== null) {
            maStlUnlockOrbitForRulerAnchorPick();
            touched = true;
        }
        if (!touched) {
            return false;
        }
        maStlTeardownPickHighlightsOnly();
        if (maStlLineToolRubberBandLine) {
            maStlLineToolRubberBandLine.visible = false;
        }
        maStlSyncLineToolHud();
        syncMaStlRulerPickToolbarUi();
        maStlSyncLineToolToggleBtnUi();
        maStlLineToolPickCursorSync();
        syncMaStlRulerAnchorPickCursor();
        [maStlLineToolToggleBtn, maStlRulerAnchorPickToggleBtn, maStlRulerAnchorObjectPickToggleBtn].forEach(
            function (el) {
                if (!el) return;
                el.classList.remove('active', 'pressed');
            }
        );
        syncMaStlRulerPickToolbarUi();
        maStlSyncLineToolToggleBtnUi();
        return true;
    }

    function maStlClearStlPickHoverHighlight() {
        maStlPickHoverMaterialSnapshots.forEach(function (entry) {
            const mesh = entry.mesh;
            if (!mesh || !mesh.material) return;
            mesh.material.color.copy(entry.color);
            if (mesh.material.emissive) {
                mesh.material.emissive.copy(entry.emissive);
            }
            mesh.material.emissiveIntensity = entry.emissiveIntensity;
            mesh.material.needsUpdate = true;
        });
        maStlPickHoverMaterialSnapshots = [];
    }

    /**
     * Hover azul temporal sobre malla STL en modo objeto (solo temporal; revierte al mover/salir).
     * @param {THREE.Mesh|null} mesh
     */
    function maStlApplyStlPickHoverHighlight(mesh) {
        if (!mesh || !mesh.isMesh || !mesh.material) return;
        const already = maStlPickHoverMaterialSnapshots.some(function (e) {
            return e.mesh === mesh;
        });
        if (already) return;
        maStlPickHoverMaterialSnapshots.push({
            mesh: mesh,
            color: mesh.material.color.clone(),
            emissive: mesh.material.emissive ? mesh.material.emissive.clone() : new THREE.Color(0x000000),
            emissiveIntensity: mesh.material.emissiveIntensity != null ? mesh.material.emissiveIntensity : 0
        });
        mesh.material.emissive.setHex(MA_STL_PICK_HOVER_EMISSIVE_HEX);
        mesh.material.emissiveIntensity = MA_STL_PICK_HOVER_EMISSIVE_INTENSITY;
        mesh.material.color.lerp(_maStlPickHoverColor, MA_STL_PICK_HOVER_COLOR_LERP);
        mesh.material.needsUpdate = true;
    }

    function maStlClearInsertionPickHighlight() {
        maStlInsertionPickHighlightGroup.visible = false;
        maStlStripOverlayMeshes(maStlInsertionPickHighlightGroup);
    }

    function maStlSyncRulerAnchorCoordsHud() {
        if (!maStlRulerAnchorCoordsHud) return;
        const showLineSnap =
            maStlRulerAnchorPickMode === 'grid' &&
            maStlLineToolVertexSnapHighlightGroup &&
            maStlLineToolVertexSnapHighlightGroup.visible;
        const showGridSnap = maStlRulerAnchorPickMode === 'grid' && maStlGridIntersectionNearActive;
        const show = showLineSnap || showGridSnap;
        if (!show) {
            maStlRulerAnchorCoordsHud.classList.add('d-none');
            maStlRulerAnchorCoordsHud.textContent = '';
            return;
        }
        const tpl =
            maStlRulerAnchorCoordsHud.getAttribute('data-ma-stl-ruler-anchor-coords-template') || '';
        const snapX = showLineSnap ? _maStlLineToolVertexSnapMm.x : _maStlGridIntersectionSnapMm.x;
        const snapZ = showLineSnap ? _maStlLineToolVertexSnapMm.z : _maStlGridIntersectionSnapMm.z;
        maStlRulerAnchorCoordsHud.textContent = maStlFormatRulerAnchorGridIntersectionToast(
            tpl,
            snapX,
            snapZ
        );
        maStlRulerAnchorCoordsHud.classList.remove('d-none');
    }

    function maStlTouchGridPickHighlightMaterials(mode) {
        const root =
            mode === 'connected'
                ? maStlGridIntersectionPickMeshes.connected
                : maStlGridIntersectionPickMeshes.idle;
        if (!root) return;
        root.traverse(function (o) {
            if (o.material) {
                o.material.needsUpdate = true;
            }
        });
    }

    /**
     * Herramientas de pick (rejilla/objeto/regla/línea): zoom + pan (encuadrar punto); rotación desactivada para no interferir con colocación.
     * Capture en pointerdown sólo corta órbita con botón izquierdo (línea: clic tras up; rejilla/objeto: colocación en down).
     * Captura flags sólo la primera vez; en cada llamada re-fuerza rotación-off + pan/zoom-on (evita carrera si algo rehabilitó flags con snapshot vivo).
     */
    function maStlLockOrbitForRulerAnchorPick() {
        if (!controls) return;
        if (!maStlRulerAnchorPickOrbitLockSnapshot) {
            maStlRulerAnchorPickOrbitLockSnapshot = {
                enabled: controls.enabled,
                enableRotate: !!controls.enableRotate,
                enablePan: !!controls.enablePan,
                enableZoom: !!controls.enableZoom
            };
            maStlClearDesing2OrbitDeferRulerPivotPreserve();
            _maStlPickLockOrbitTargetBaseline.copy(controls.target);
            _maStlPickLockRulerAnchorStartMm.copy(maStlRulerAnchorMm);
        }
        controls.enabled = true;
        controls.enableRotate = false;
        controls.enablePan = true;
        controls.enableZoom = true;
    }

    function maStlClearDeferredRulerPickOrbitUnlock() {
        if (!maStlDeferredRulerPickUnlockPointerEnded) return;
        window.removeEventListener('pointerup', maStlDeferredRulerPickUnlockPointerEnded, true);
        window.removeEventListener('pointercancel', maStlDeferredRulerPickUnlockPointerEnded, true);
        maStlDeferredRulerPickUnlockPointerEnded = null;
    }

    function maStlClearDesing2OrbitDeferRulerPivotPreserve() {
        maStlDesing2OrbitDeferRulerPivotPreserveOnNextSync = false;
    }

    function maStlClearDesing2OrbitPreserveRulerPivotOnRotatePointerDown() {
        maStlDesing2OrbitPreserveRulerPivotOnRotatePointerDown = false;
    }

    function maStlMarkDesing2OrbitRulerAnchoredPreserveNeededOnRotate() {
        if (maStlDesingV2Viewer) {
            maStlDesing2OrbitPreserveRulerPivotOnRotatePointerDown = true;
        }
    }

    function maStlDisposeDesing2OrbitPickLockListener() {
        if (_maStlDesing2OrbitPickLockChangeHandler && controls) {
            controls.removeEventListener('change', _maStlDesing2OrbitPickLockChangeHandler);
        }
        _maStlDesing2OrbitPickLockChangeHandler = null;
    }

    /**
     * Registrar en cada instancia de OrbitControls. Desing_2: durante pick-lock, si `controls.target` se mueve
     * respecto al baseline (> {@link MA_STL_DESING2_PICK_ORBIT_PAN_DETECTION_EPS_MM}), hubo pan → no preservar vista
     * re-anclando al ruler en unlock ni en el primer rotate.
     */
    function maStlWireDesing2OrbitPickLockListener() {
        if (!maStlDesingV2Viewer || !controls) return;
        maStlDisposeDesing2OrbitPickLockListener();
        const epsMm = MA_STL_DESING2_PICK_ORBIT_PAN_DETECTION_EPS_MM;
        const epsSq = epsMm * epsMm;
        _maStlDesing2OrbitPickLockChangeHandler = function () {
            if (!maStlRulerAnchorPickOrbitLockSnapshot || !controls) return;
            if (controls.target.distanceToSquared(_maStlPickLockOrbitTargetBaseline) > epsSq) {
                maStlDesing2OrbitDeferRulerPivotPreserveOnNextSync = true;
            }
        };
        controls.addEventListener('change', _maStlDesing2OrbitPickLockChangeHandler);
    }

    /**
     * Rehabilitar órbita tras pick sin carrera con OrbitControls.
     * @param {boolean} deferUntilPointerEnd `true`: clic coloca en `pointerdown` → esperar `pointerup`/`pointercancel` + 2 RAF (reglas/objects).
     *   `false`: colocación tras `pointerup` (p. ej. evento `click` en herramienta línea) → sólo 2 RAF.
     * @param {boolean} [skipRulerAnchorPreserveViewOnUnlock] Desing_2: si es true (herramienta **línea**), no ejecutar
     *   {@link maStlApplyRulerAnchorOrbitPivotPreserveView} tras unlock: la línea no mueve `maStlRulerAnchorMm`;
     *   forzar pivote al ruler aquí reproduce el salto de “pan → snap” (orbit target desalineado a propósito).
     *   Si además `deferUntilPointerEnd` es false, se usa `queueMicrotask` + 2 RAF (el `click` de P2 corre tras `pointerup`).
     */
    function maStlSchedulePickOrbitUnlockAfterPlacement(
        deferUntilPointerEnd,
        skipRulerAnchorPreserveViewOnUnlock
    ) {
        if (!maStlRulerAnchorPickOrbitLockSnapshot) return;
        maStlClearDeferredRulerPickOrbitUnlock();
        const tryFinishDeferredUnlockFromPlacement = function () {
            if (!maStlRulerAnchorPickOrbitLockSnapshot || !controls) return;
            maStlUnlockOrbitForRulerAnchorPickInner();
            maStlSyncDesing2OrbitPivotAfterPickOrbitUnlock(!!skipRulerAnchorPreserveViewOnUnlock);
            if (typeof controls.saveState === 'function') {
                controls.saveState();
            }
        };
        if (deferUntilPointerEnd) {
            const onPointerEnded = function () {
                maStlClearDeferredRulerPickOrbitUnlock();
                requestAnimationFrame(function () {
                    requestAnimationFrame(tryFinishDeferredUnlockFromPlacement);
                });
            };
            maStlDeferredRulerPickUnlockPointerEnded = onPointerEnded;
            window.addEventListener('pointerup', onPointerEnded, true);
            window.addEventListener('pointercancel', onPointerEnded, true);
        } else if (skipRulerAnchorPreserveViewOnUnlock) {
            queueMicrotask(function () {
                requestAnimationFrame(function () {
                    requestAnimationFrame(tryFinishDeferredUnlockFromPlacement);
                });
            });
        } else {
            requestAnimationFrame(function () {
                requestAnimationFrame(tryFinishDeferredUnlockFromPlacement);
            });
        }
    }

    /** Tras modo pick — fin de gesto (`pointerup`/`pointercancel`) + 2 RAF antes de rehabilitar órbita (evita carrera con OrbitControls). */
    function maStlScheduleDeferRulerPickOrbitUnlockAfterPointerEnd() {
        maStlSchedulePickOrbitUnlockAfterPlacement(true);
    }

    function maStlUnlockOrbitForRulerAnchorPickInner() {
        if (!controls || !maStlRulerAnchorPickOrbitLockSnapshot) return;
        const snap = maStlRulerAnchorPickOrbitLockSnapshot;
        controls.enabled = snap.enabled;
        controls.enableRotate = snap.enableRotate;
        controls.enablePan = snap.enablePan;
        controls.enableZoom = snap.enableZoom;
        maStlRulerAnchorPickOrbitLockSnapshot = null;
    }

    /**
     * Desing_2: tras desbloquear órbita al salir pick de anclaje / regla (o línea con `skip`): alinear target con ruler via
     * {@link maStlApplyRulerAnchorOrbitPivotPreserveView()} salvo cuando el usuario **paneó** bajo pick-lock **y**
     * **`maStlRulerAnchorMm` no cambió** desde el inicio de esa sesión lock. Si durante lock sí se **colocó** nuevo anclaje
     * (rejilla/objeto), ejecutar preserveView y limpiar defer. **Herramienta línea** (`skip`): no preserve hacia ruler —
     * la línea no mueve `maStlRulerAnchorMm`.
     *
     * @see Desing/docs/desing-2-orbit-pivot.md ({@link MA_STL_DESING2_PICK_ORBIT_PAN_DETECTION_EPS_MM}, defer flag)
     * @param {boolean} [skipRulerAnchorPreserveViewOnUnlock] Herramienta línea / cierre sin colocación de anclaje:
     *   no alinear `controls.target` a `maStlRulerAnchorMm` aquí (el usuario puede haber paneado con target desfasado).
     */
    function maStlSyncDesing2OrbitPivotAfterPickOrbitUnlock(skipRulerAnchorPreserveViewOnUnlock) {
        if (!controls || !maStlUsesFixedOrbitPivotAtOrigin()) return;
        if (skipRulerAnchorPreserveViewOnUnlock) {
            maStlClearDesing2OrbitDeferRulerPivotPreserve();
            controls.update();
            return;
        }
        const epsMm = MA_STL_DESING2_PICK_ORBIT_PAN_DETECTION_EPS_MM;
        const anchorMoved =
            _maStlPickLockRulerAnchorStartMm.distanceToSquared(maStlRulerAnchorMm) > epsMm * epsMm;
        if (maStlDesing2OrbitDeferRulerPivotPreserveOnNextSync && !anchorMoved) {
            /* Pan bajo pick-lock sin mover anclaje (p. ej. herramienta línea): no re-encajar pivote al ruler. */
            return;
        }
        maStlApplyRulerAnchorOrbitPivotPreserveView();
        maStlClearDesing2OrbitDeferRulerPivotPreserve();
    }

    /** @param {boolean} [skipRulerAnchorPreserveViewOnUnlock] Ver {@link maStlSyncDesing2OrbitPivotAfterPickOrbitUnlock}. */
    function maStlUnlockOrbitForRulerAnchorPick(skipRulerAnchorPreserveViewOnUnlock) {
        maStlClearDeferredRulerPickOrbitUnlock();
        maStlUnlockOrbitForRulerAnchorPickInner();
        maStlSyncDesing2OrbitPivotAfterPickOrbitUnlock(skipRulerAnchorPreserveViewOnUnlock);
    }

    function maStlEnsureGridIntersectionPickHighlightMeshes() {
        if (maStlGridIntersectionPickMeshes.connected) {
            if (maStlDesingV2Viewer && maStlGridIntersectionPickHighlightGroup.parent !== scene) {
                scene.add(maStlGridIntersectionPickHighlightGroup);
            }
            return;
        }
        const half = desing2EnvGridSnapMm * 0.48;
        maStlGridIntersectionPickMeshes.idle = maStlBuildGridIntersectionPickHighlightOutline(
            half,
            MA_STL_GRID_INTERSECTION_PICK_IDLE_COLOR,
            MA_STL_GRID_INTERSECTION_PICK_IDLE_OPACITY
        );
        maStlGridIntersectionPickMeshes.connected = maStlBuildGridIntersectionPickHighlightFilled(
            half,
            MA_STL_GRID_INTERSECTION_PICK_HIGHLIGHT_COLOR,
            MA_STL_GRID_INTERSECTION_PICK_HIGHLIGHT_OPACITY
        );
        maStlGridIntersectionPickMeshes.idle.visible = false;
        maStlGridIntersectionPickMeshes.connected.visible = false;
        maStlGridIntersectionPickHighlightGroup.add(maStlGridIntersectionPickMeshes.idle);
        maStlGridIntersectionPickHighlightGroup.add(maStlGridIntersectionPickMeshes.connected);
    }

    function maStlClearGridIntersectionPickHighlight() {
        maStlGridIntersectionNearActive = false;
        maStlGridIntersectionPickHighlightGroup.visible = false;
        maStlSetOverlaySubtreeVisible(maStlGridIntersectionPickMeshes.idle, false);
        maStlSetOverlaySubtreeVisible(maStlGridIntersectionPickMeshes.connected, false);
        maStlSyncRulerAnchorCoordsHud();
        maStlSyncLineToolHud();
    }

    /**
     * @param {boolean} active
     * @param {THREE.Vector3|null} [worldPos]
     */
    function maStlSetInsertionPickHighlight(active, worldPos) {
        if (!active || !worldPos) {
            maStlClearInsertionPickHighlight();
            return;
        }
        maStlStripOverlayMeshes(maStlInsertionPickHighlightGroup);
        const half = THREE.MathUtils.clamp(lastMaxDim * 0.06, 180, 400);
        maStlInsertionPickHighlightGroup.position.set(
            worldPos.x,
            MA_STL_DESING2_WORKSPACE_FLOOR_Y_MM,
            worldPos.z
        );
        maStlInsertionPickHighlightGroup.add(maStlBuildInsertionPickHighlightRect(half));
        maStlInsertionPickHighlightGroup.visible = true;
    }

    /**
     * @param {'idle'|'connected'} mode
     * @param {{ x: number, y: number, z: number }} snapSnap Punto snap rejilla (mm).
     */
    function maStlSetGridIntersectionPickHighlight(mode, snapSnap) {
        if (!mode || !snapSnap) {
            maStlClearGridIntersectionPickHighlight();
            return;
        }
        maStlEnsureGridIntersectionPickHighlightMeshes();
        maStlGridIntersectionNearActive = mode === 'connected';
        _maStlGridIntersectionSnapMm.set(snapSnap.x, snapSnap.y, snapSnap.z);
        maStlGridIntersectionPickHighlightGroup.position.set(snapSnap.x, snapSnap.y, snapSnap.z);
        maStlSetOverlaySubtreeVisible(maStlGridIntersectionPickMeshes.idle, mode === 'idle');
        maStlSetOverlaySubtreeVisible(maStlGridIntersectionPickMeshes.connected, mode === 'connected');
        maStlTouchGridPickHighlightMaterials('idle');
        maStlTouchGridPickHighlightMaterials('connected');
        maStlGridIntersectionPickHighlightGroup.visible = true;
        maStlSyncRulerAnchorCoordsHud();
        maStlSyncLineToolHud();
    }

    /** Solo click: anclaje + reglas; pivote = target con pan compensado (vista estable). */
    function maStlApplyRulerAnchorOrbitTargetOnly() {
        if (!controls) return;
        maStlApplyRulerAnchorOrbitPivotPreserveView();
    }

    function maStlSetRulerAnchorFromGridSnap(snap) {
        maStlRulerAnchorMm.set(snap.x, snap.y, snap.z);
        maStlInvalidateUserFloorDimGuideGeomCache();
        maStlSyncUserFloorDimHudScreenOnly();
        rebuildMaStlUcsOverlayDecor(lastMaxDim);
        /* Desing_2 Viewer: no tocar cámara/target/controls en el clic de colocación; rejilla/brújula usan maStlRulerAnchorMm. */
        if (!maStlDesingV2Viewer) {
            maStlApplyRulerAnchorOrbitTargetOnly();
        } else {
            maStlMarkDesing2OrbitRulerAnchoredPreserveNeededOnRotate();
        }
    }

    function maStlSetRulerAnchorFromInsertionPoint(worldPos) {
        maStlRulerAnchorMm.set(worldPos.x, MA_STL_DESING2_WORKSPACE_FLOOR_Y_MM, worldPos.z);
        maStlInvalidateUserFloorDimGuideGeomCache();
        maStlSyncUserFloorDimHudScreenOnly();
        rebuildMaStlUcsOverlayDecor(lastMaxDim);
        if (!maStlDesingV2Viewer) {
            maStlApplyRulerAnchorOrbitTargetOnly();
        } else {
            maStlMarkDesing2OrbitRulerAnchoredPreserveNeededOnRotate();
        }
    }

    /** @deprecated usar maStlSetRulerAnchorFromInsertionPoint */
    function maStlSetRulerAnchorFromPickPoint(hitPoint) {
        maStlSetRulerAnchorFromInsertionPoint(hitPoint);
    }

    /** Salir del modo pick tras colocación por clic canvas (defer unlock órbita). */
    function maStlExitRulerAnchorPickAfterPlacement() {
        maStlRulerAnchorPickMode = null;
        /**
         * DESING_2 ruler pick — MUST NOT rehabilitar OrbitControls hasta que termine la gestión `pointer*` del clic.
         *
         * `queueMicrotask` fue insuficiente: el microtask siguen antes que `pointerup` y algunos navegadores/Orbit pueden
         * quedar fuera de fase (`enabled=true` antes de cerrar gesto → primer `controls.update()` “engancha”).
         * Se espera `pointerup`/`pointercancel` en window (captura), luego 2 RAF, y sólo entonces unlock + `saveState`.
         * @see Desing/docs/desing-2-orbit-pivot.md (ruler anchor pick)
         */
        maStlScheduleDeferRulerPickOrbitUnlockAfterPointerEnd();
        maStlClearInsertionPickHighlight();
        maStlClearGridIntersectionPickHighlight();
        maStlClearLineToolVertexSnapHighlight();
        maStlClearStlPickHoverHighlight();
        syncMaStlRulerPickToolbarUi();
        syncMaStlRulerAnchorPickCursor();
    }

    const _maStlInsertionFloorProbe = new THREE.Vector3();

    function maStlSyncLineToolHud() {
        if (!maStlLineToolHud || !maStlDesingV2Viewer) return;
        if (!maStlIsLineToolPlacementActive()) {
            maStlLineToolHud.classList.add('d-none');
            if (maStlLineToolHudInstruction) {
                maStlLineToolHudInstruction.textContent = '';
            }
            if (maStlLineToolHudCoords) {
                maStlLineToolHudCoords.textContent = '';
            }
            if (maStlLineToolHudDistanceRow) {
                maStlLineToolHudDistanceRow.classList.add('d-none');
            }
            if (maStlLineToolHudDistancePreview) {
                maStlLineToolHudDistancePreview.textContent = '';
            }
            return;
        }
        const insFirst =
            (maStlLineToolHud &&
                maStlLineToolHud.getAttribute('data-ma-stl-line-tool-instruction-first')) ||
            '';
        const insSecond =
            (maStlLineToolHud &&
                maStlLineToolHud.getAttribute('data-ma-stl-line-tool-instruction-second')) ||
            '';
        if (maStlLineToolHudInstruction) {
            maStlLineToolHudInstruction.textContent =
                maStlLineToolState === 'picking1' ? insFirst : insSecond;
        }
        const tpl =
            (maStlLineToolHudCoords &&
                maStlLineToolHudCoords.getAttribute('data-ma-stl-line-tool-coords-template')) ||
            '';
        const showCoords = !!(
            (maStlGridIntersectionPickHighlightGroup && maStlGridIntersectionPickHighlightGroup.visible) ||
            (maStlLineToolVertexSnapHighlightGroup && maStlLineToolVertexSnapHighlightGroup.visible)
        );
        if (maStlLineToolHudCoords) {
            if (showCoords && tpl) {
                const snapX = maStlLineToolVertexSnapHighlightGroup.visible
                    ? _maStlLineToolVertexSnapMm.x
                    : _maStlGridIntersectionSnapMm.x;
                const snapZ = maStlLineToolVertexSnapHighlightGroup.visible
                    ? _maStlLineToolVertexSnapMm.z
                    : _maStlGridIntersectionSnapMm.z;
                maStlLineToolHudCoords.textContent = maStlFormatRulerAnchorGridIntersectionToast(
                    tpl,
                    snapX,
                    snapZ
                );
            } else {
                maStlLineToolHudCoords.textContent = '';
            }
        }
        if (maStlLineToolHudDistanceRow) {
            maStlLineToolHudDistanceRow.classList.toggle('d-none', maStlLineToolState !== 'picking2');
        }
        maStlLineToolSyncTypingPreviewUi();
        maStlLineToolHud.classList.remove('d-none');
    }

    function maStlSyncLineToolToggleBtnUi() {
        if (!maStlLineToolToggleBtn) return;
        const on = maStlIsLineToolPlacementActive();
        maStlLineToolToggleBtn.setAttribute('aria-pressed', on ? 'true' : 'false');
        maStlLineToolToggleBtn.classList.toggle('active', on);
    }

    function maStlSyncLineToolOrtho15ToggleUi() {
        if (!maStlLineToolOrtho15ToggleBtn) return;
        maStlLineToolOrtho15ToggleBtn.setAttribute('aria-pressed', maStlLineToolOrtho15Enabled ? 'true' : 'false');
        maStlLineToolOrtho15ToggleBtn.classList.toggle('active', maStlLineToolOrtho15Enabled);
        const onTitle = maStlLineToolOrtho15ToggleBtn.getAttribute('data-ma-stl-ortho15-title-on') || '';
        const offTitle = maStlLineToolOrtho15ToggleBtn.getAttribute('data-ma-stl-ortho15-title-off') || '';
        const t = maStlLineToolOrtho15Enabled ? onTitle : offTitle;
        if (t) {
            maStlLineToolOrtho15ToggleBtn.setAttribute('title', t);
            maStlLineToolOrtho15ToggleBtn.setAttribute('aria-label', t);
        }
    }

    function maStlToggleLineToolOrtho15FromUi() {
        maStlLineToolOrtho15Enabled = !maStlLineToolOrtho15Enabled;
        maStlSyncLineToolOrtho15ToggleUi();
        if (maStlLineToolState === 'picking2') {
            maStlLineToolRefreshPicking2RubberBand();
        }
    }

    function maStlLineToolPickCursorSync() {
        if (!renderer || !renderer.domElement) return;
        if (maStlIsLineToolPlacementActive()) {
            renderer.domElement.style.cursor = 'crosshair';
        } else if (!maStlIsRulerAnchorPickModeActive()) {
            renderer.domElement.style.cursor = '';
        }
    }

    function maStlResetLineToolDistanceTypingState() {
        maStlLineToolDistanceTypeBuffer = '';
        if (maStlLineToolHudDistanceInput) {
            maStlLineToolHudDistanceInput.value = '';
        }
        if (maStlLineToolHudDistancePreview) {
            maStlLineToolHudDistancePreview.textContent = '';
        }
    }

    /** Al iniciar `picking1` o tras cancelación: dirección+tamaño último puntero limpios. */
    function maStlResetLineToolPickingBaselineState() {
        maStlLineToolLastHoverDirUnitXz.set(1, 0);
        maStlLineToolLastPointerClientXY.set(Number.NaN, Number.NaN);
        maStlResetLineToolDistanceTypingState();
    }

    function maStlLineToolMaybeUpdateHoverDirFromP2Candidate(p) {
        if (!p || maStlLineToolState !== 'picking2') return;
        const dx = p.x - maStlLineToolPoint1Mm.x;
        const dz = p.z - maStlLineToolPoint1Mm.z;
        const r = maStlLineToolFloorDirLenFromDeltaMm(dx, dz, maStlLineToolOrtho15Enabled);
        if (r) {
            maStlLineToolLastHoverDirUnitXz.set(r.x, r.z);
        }
    }

    /**
     * Dirección en planta (XZ): P1→cursor en suelo; con orto 15° redondea el acimut a múltiplos de 15° (0° = +X, +90° = +Z vía atan2).
     * Si rayo inválido o longitud ~0, último hover no nulo ({@link maStlLineToolLastHoverDirUnitXz}); si vacío, +X.
     * @returns {{ x: number, z: number }} unitario XZ.
     */
    function maStlLineToolComputeFloorDirUnitXz() {
        const p1x = maStlLineToolPoint1Mm.x;
        const p1z = maStlLineToolPoint1Mm.z;
        if (Number.isFinite(maStlLineToolLastPointerClientXY.x) && renderer) {
            const pCur = maStlResolveLineToolFloorPointMm(
                maStlLineToolLastPointerClientXY.x,
                maStlLineToolLastPointerClientXY.y
            );
            if (pCur) {
                const dx = pCur.x - p1x;
                const dz = pCur.z - p1z;
                const r = maStlLineToolFloorDirLenFromDeltaMm(dx, dz, maStlLineToolOrtho15Enabled);
                if (r) {
                    return { x: r.x, z: r.z };
                }
            }
        }
        const ux = maStlLineToolLastHoverDirUnitXz.x;
        const uz = maStlLineToolLastHoverDirUnitXz.y;
        const hu = Math.hypot(ux, uz);
        if (hu >= 1e-9) {
            return { x: ux / hu, z: uz / hu };
        }
        return { x: 1, z: 0 };
    }

    /** Texto de distancia: HUD y buffer global pueden desalinearse (Input vs keydown ventana); unificar antes de parseo. */
    function maStlLineToolRawDistanceTypingTrimmed() {
        const vi = maStlLineToolHudDistanceInput ? String(maStlLineToolHudDistanceInput.value || '').trim() : '';
        const vb = String(maStlLineToolDistanceTypeBuffer || '').trim();
        return vi || vb;
    }

    function maStlLineToolParsedPreviewLenMm() {
        return maStlParseLengthInputValueToMm(maStlLineToolRawDistanceTypingTrimmed());
    }

    function maStlLineToolSyncTypingPreviewUi() {
        if (
            maStlLineToolHudDistancePreview &&
            maStlLineToolState !== 'picking2'
        ) {
            maStlLineToolHudDistancePreview.textContent = '';
        }
        if (!maStlLineToolHudDistancePreview || maStlLineToolState !== 'picking2') {
            return;
        }
        const lenParsed = maStlLineToolParsedPreviewLenMm();
        if (lenParsed != null && lenParsed > 0) {
            const minMm = maStlUserFloorSegmentMinMm();
            const maxMm = Math.max(minMm, lastMaxDim * 25);
            const lenDisp = maStlDesing2LengthMmRoundedEditableFromMm(
                THREE.MathUtils.clamp(lenParsed, minMm, maxMm)
            );
            const d = maStlDesing2DimEditableMetersDisplayFromMm(lenDisp);
            const pfx =
                (maStlLineToolHud &&
                    maStlLineToolHud.getAttribute('data-ma-stl-line-tool-distance-preview-prefix')) ||
                '\u2248';
            maStlLineToolHudDistancePreview.textContent =
                String(pfx) + '\u00A0' + d + '\u00A0m';
        } else {
            maStlLineToolHudDistancePreview.textContent = '';
        }
    }

    function maStlLineToolTypingBufferedLenOkForRubberMm() {
        const len = maStlLineToolParsedPreviewLenMm();
        const minMm = maStlUserFloorSegmentMinMm();
        return maStlLineToolState === 'picking2' && len != null && Number.isFinite(len) && len >= minMm - 1e-9;
    }

    /**
     * Extremo P2 del caucho en `picking2` (cursor, snap vértice u longitud tecleada + orto 15°).
     * @param {{ x: number, y: number, z: number }} out
     * @returns {{ x: number, y: number, z: number }|null}
     */
    function maStlLineToolComputeRubberBandEndMm(out) {
        if (maStlLineToolState !== 'picking2') return null;
        if (maStlLineToolTypingBufferedLenOkForRubberMm()) {
            out.x = maStlLineToolTypedEndRubberMm.x;
            out.y = maStlLineToolTypedEndRubberMm.y;
            out.z = maStlLineToolTypedEndRubberMm.z;
            return out;
        }
        if (!Number.isFinite(maStlLineToolLastPointerClientXY.x) || !renderer) return null;
        const p = maStlResolveLineToolFloorPointMm(
            maStlLineToolLastPointerClientXY.x,
            maStlLineToolLastPointerClientXY.y
        );
        if (!p) return null;
        const yFl = MA_STL_DESING2_WORKSPACE_FLOOR_Y_MM;
        if (p.maStlLineVertexSnap) {
            out.x = p.x;
            out.y = p.y;
            out.z = p.z;
            return out;
        }
        const dx = p.x - maStlLineToolPoint1Mm.x;
        const dz = p.z - maStlLineToolPoint1Mm.z;
        const r = maStlLineToolFloorDirLenFromDeltaMm(dx, dz, maStlLineToolOrtho15Enabled);
        if (r) {
            out.x = maStlLineToolPoint1Mm.x + r.x * r.len;
            out.y = yFl;
            out.z = maStlLineToolPoint1Mm.z + r.z * r.len;
            return out;
        }
        out.x = p.x;
        out.y = p.y;
        out.z = p.z;
        return out;
    }

    function maStlLineToolRefreshPicking2RubberBand() {
        if (maStlLineToolState !== 'picking2') {
            maStlLineToolHidePreviewDimHud();
            return;
        }
        if (maStlLineToolTypingBufferedLenOkForRubberMm()) {
            let lenMm = maStlLineToolParsedPreviewLenMm();
            const minMm = maStlUserFloorSegmentMinMm();
            const maxMm = Math.max(minMm, lastMaxDim * 25);
            lenMm = THREE.MathUtils.clamp(lenMm, minMm, maxMm);
            lenMm = maStlDesing2LengthMmRoundedEditableFromMm(lenMm);
            const u = maStlLineToolComputeFloorDirUnitXz();
            const y = MA_STL_DESING2_WORKSPACE_FLOOR_Y_MM;
            maStlLineToolTypedEndRubberMm.set(
                maStlLineToolPoint1Mm.x + u.x * lenMm,
                y,
                maStlLineToolPoint1Mm.z + u.z * lenMm
            );
        }
        const end = maStlLineToolComputeRubberBandEndMm(_maStlLineToolRubberEndMm);
        if (!end) {
            if (maStlLineToolRubberBandLine) {
                maStlLineToolRubberBandLine.visible = false;
            }
            maStlLineToolHidePreviewDimHud();
            return;
        }
        maStlEnsureLineToolRubberBand();
        maStlUpdateLineToolRubberBandMm(end.x, end.y, end.z);
        maStlLineToolUpdatePreviewDimHud();
    }

    function maStlLineToolGloballyConsumeDistanceKeys() {
        const el = document.activeElement;
        if (maStlLineToolHudDistanceInput && el === maStlLineToolHudDistanceInput) return false;
        if (maStlIsUserFloorLineDimEditOverlayActive()) return false;
        if (maStlDesingV2AvoidKeyboardShortcutSteal(el)) return false;
        /* picking2: el botón línea suele mantener foco tras el clic — sin esto los dígitos no entran en buffer. */
        if (
            maStlLineToolState === 'picking2' &&
            maStlLineToolToggleBtn &&
            el === maStlLineToolToggleBtn
        ) {
            return true;
        }
        if (el && el.closest && el.closest('button, a[href], [role="button"], label')) return false;
        return true;
    }

    /** @returns {boolean} true si trató esta tecla (puede haber ejecutado {@link ev.preventDefault}). */
    function maStlLineToolApplyWindowKeydownToDistanceBuffer(ev) {
        if (maStlLineToolState !== 'picking2' || !maStlDesingV2Viewer) return false;
        if (!maStlIsDesing2ViewerShellVisibleForKeyboardShortcuts()) return false;
        if (ev.defaultPrevented) return false;
        if (!maStlLineToolGloballyConsumeDistanceKeys(ev)) return false;
        if (ev.ctrlKey || ev.metaKey || ev.altKey) return false;
        if (ev.key === 'Escape' || ev.code === 'Escape') return false;
        if (ev.key === 'Enter' || ev.code === 'Enter' || ev.key === 'NumpadEnter') {
            const ok = maStlLineToolTryTypedCommitDistanceOrbitDefer();
            if (ok) {
                ev.preventDefault();
            }
            return ok;
        }
        let next = maStlLineToolDistanceTypeBuffer;
        const k = ev.key;
        if (k === 'Backspace') {
            if (!next.length) return false;
            ev.preventDefault();
            next = next.slice(0, -1);
        } else if (k === 'Delete') {
            if (!next.length) return false;
            ev.preventDefault();
            next = '';
        } else if (k.length === 1 && /^[0-9.,\sMm]$/.test(k)) {
            ev.preventDefault();
            next += k;
        } else {
            return false;
        }
        maStlLineToolDistanceTypeBuffer = next;
        if (maStlLineToolHudDistanceInput && document.activeElement !== maStlLineToolHudDistanceInput) {
            maStlLineToolHudDistanceInput.value = next;
        }
        maStlLineToolSyncTypingPreviewUi();
        maStlLineToolRefreshPicking2RubberBand();
        return true;
    }

    /** @returns {boolean} true si aplicó segmento completo (`true` igual que segundo clic). */
    function maStlLineToolTryTypedCommitDistanceOrbitDefer() {
        const raw = maStlLineToolRawDistanceTypingTrimmed();
        maStlLineToolDistanceTypeBuffer = raw;
        const lenMmRaw = maStlParseLengthInputValueToMm(raw);
        const minMm = maStlUserFloorSegmentMinMm();
        const maxMm = Math.max(minMm, lastMaxDim * 25);
        if (lenMmRaw == null || !Number.isFinite(lenMmRaw)) {
            const toastTpl =
                maStlLineToolHud &&
                maStlLineToolHud.getAttribute('data-ma-stl-line-tool-distance-invalid-toast');
            if (toastTpl) maStlDesing2ShowSaveViewToast(toastTpl);
            return false;
        }
        let lenMm = THREE.MathUtils.clamp(lenMmRaw, minMm, maxMm);
        lenMm = maStlDesing2LengthMmRoundedEditableFromMm(lenMm);
        const u = maStlLineToolComputeFloorDirUnitXz();
        const y = MA_STL_DESING2_WORKSPACE_FLOOR_Y_MM;
        const end = {
            x: maStlLineToolPoint1Mm.x + u.x * lenMm,
            y: y,
            z: maStlLineToolPoint1Mm.z + u.z * lenMm,
        };
        maStlCommitUserPlanLineSegmentMm(maStlLineToolPoint1Mm, end);
        maStlLineToolResetForNextSegment();
        return true;
    }

    function maStlEnsureLineToolVertexSnapMarker() {
        if (maStlLineToolVertexSnapBallMesh) return;
        maStlLineToolVertexSnapBallMesh = maStlBuildLineToolVertexSnapBallMm(
            MA_STL_LINE_TOOL_VERTEX_SNAP_BALL_RADIUS_MM,
            MA_STL_LINE_TOOL_VERTEX_SNAP_COLOR,
            MA_STL_LINE_TOOL_VERTEX_SNAP_OPACITY_IDLE
        );
        maStlLineToolVertexSnapHighlightGroup.add(maStlLineToolVertexSnapBallMesh);
    }

    function maStlClearLineToolVertexSnapHighlight() {
        maStlLineToolVertexSnapHighlightGroup.visible = false;
        if (maStlLineToolVertexSnapBallMesh) {
            maStlLineToolVertexSnapBallMesh.scale.setScalar(1);
        }
        maStlSyncLineToolHud();
        maStlSyncRulerAnchorCoordsHud();
    }

    /**
     * @param {{ x: number, y: number, z: number }} snapSnap
     * @param {boolean} active connected (verde) vs. idle (cian)
     */
    function maStlSetLineToolVertexSnapHighlight(snapSnap, active) {
        if (!snapSnap) {
            maStlClearLineToolVertexSnapHighlight();
            return;
        }
        maStlEnsureLineToolVertexSnapMarker();
        _maStlLineToolVertexSnapMm.set(snapSnap.x, snapSnap.y, snapSnap.z);
        maStlLineToolVertexSnapHighlightGroup.position.set(snapSnap.x, snapSnap.y, snapSnap.z);
        if (maStlLineToolVertexSnapBallMesh && maStlLineToolVertexSnapBallMesh.material) {
            const mat = maStlLineToolVertexSnapBallMesh.material;
            mat.color.setHex(
                active ? MA_STL_LINE_TOOL_VERTEX_SNAP_COLOR_ACTIVE : MA_STL_LINE_TOOL_VERTEX_SNAP_COLOR
            );
            mat.opacity = active
                ? MA_STL_LINE_TOOL_VERTEX_SNAP_OPACITY_ACTIVE
                : MA_STL_LINE_TOOL_VERTEX_SNAP_OPACITY_IDLE;
            maStlLineToolVertexSnapBallMesh.scale.setScalar(active ? 1.12 : 1);
        }
        maStlLineToolVertexSnapHighlightGroup.visible = true;
        maStlSyncLineToolHud();
        maStlSyncRulerAnchorCoordsHud();
    }

    /**
     * Snap a P1, P2 o punto medio de otra línea usuario (excluye el vértice P1 del trazo en curso).
     * Prioridad: menor distancia en pantalla (px).
     * @param {{ x: number, z: number }} [floorHit] impacto cursor en suelo (proximidad XZ).
     * @param {{ clientX: number, clientY: number, camera: THREE.Camera, canvas: HTMLElement, maxDim: number, pickScreenPxBoost?: number }} proximity
     * @returns {{ x: number, y: number, z: number, kind: 'p1'|'p2'|'mid', active: boolean }|null}
     */
    function maStlFindLineToolVertexSnapCandidate(clientX, clientY, floorHit, proximity) {
        if (!maStlUserLinesGroup || !proximity || !proximity.camera || !proximity.canvas) {
            return null;
        }
        const cam = proximity.camera;
        const canvas = proximity.canvas;
        const maxDim = proximity.maxDim;
        const boostPx =
            proximity.pickScreenPxBoost != null && proximity.pickScreenPxBoost > 0
                ? proximity.pickScreenPxBoost
                : 0;
        const screenThreshPx = maStlGridIntersectionPickScreenThresholdPx(cam) + boostPx;
        const minSegMm = maStlUserFloorSegmentMinMm();
        const excludeNearP1 = maStlLineToolState === 'picking2';
        const yFloor = MA_STL_DESING2_WORKSPACE_FLOOR_Y_MM;
        let best = null;
        let bestScreenPx = Infinity;
        const ch = maStlUserLinesGroup.children;
        for (let i = 0; i < ch.length; i++) {
            const line = ch[i];
            if (!maStlIsUserFloorPlanLineObject(line)) continue;
            const ud = line.userData.maStlUserPlanLine;
            if (!ud || !ud.p1Mm || !ud.p2Mm) continue;
            const p1 = ud.p1Mm;
            const p2 = ud.p2Mm;
            const candidates = [
                { kind: 'p1', x: p1.x, y: p1.y, z: p1.z },
                { kind: 'p2', x: p2.x, y: p2.y, z: p2.z },
                {
                    kind: 'mid',
                    x: (p1.x + p2.x) * 0.5,
                    y: yFloor,
                    z: (p1.z + p2.z) * 0.5
                }
            ];
            for (let ci = 0; ci < candidates.length; ci++) {
                const c = candidates[ci];
                if (excludeNearP1) {
                    const dP1 = Math.hypot(c.x - maStlLineToolPoint1Mm.x, c.z - maStlLineToolPoint1Mm.z);
                    if (dP1 < minSegMm) continue;
                }
                _maStlLineToolVertexSnapWorldScratch.set(c.x, c.y, c.z);
                const screenPx = maStlInsertionPointScreenDistancePx(
                    _maStlLineToolVertexSnapWorldScratch,
                    clientX,
                    clientY,
                    cam,
                    canvas
                );
                let active = screenPx <= screenThreshPx;
                if (!active && floorHit) {
                    const threshMm = maStlGridIntersectionPickProximityThresholdMm(
                        maxDim,
                        cam,
                        _maStlLineToolVertexSnapWorldScratch,
                        desing2EnvGridSnapMm
                    );
                    const distXZ = Math.hypot(floorHit.x - c.x, floorHit.z - c.z);
                    active =
                        distXZ <=
                        Math.max(threshMm, desing2EnvGridSnapMm * MA_STL_LINE_TOOL_VERTEX_SNAP_WORLD_MM_FACTOR);
                }
                if (!active) continue;
                if (screenPx < bestScreenPx) {
                    bestScreenPx = screenPx;
                    best = { x: c.x, y: c.y, z: c.z, kind: c.kind, active: true };
                }
            }
        }
        return best;
    }

    /**
     * Snap P1/P2/mid de línea usuario en planta (herramienta línea y pick anclaje rejilla).
     * @param {number} clientX
     * @param {number} clientY
     * @param {number} [pickScreenPxBoost] p. ej. {@link MA_STL_LINE_TOOL_GRID_PICK_SCREEN_PX_BOOST}
     * @returns {{ x: number, y: number, z: number, kind: 'p1'|'p2'|'mid', active: boolean }|null}
     */
    function maStlFindFloorLineVertexSnapAtPointer(clientX, clientY, pickScreenPxBoost) {
        if (!renderer) return null;
        const canvas = renderer.domElement;
        const cam = activeCamera();
        if (
            !maStlClientRayToWorkspaceFloor(
                clientX,
                clientY,
                canvas,
                cam,
                orbitPivotNdc,
                orbitPivotRaycaster,
                _maStlInsertionFloorProbe
            )
        ) {
            return null;
        }
        const floorHitObj = { x: _maStlInsertionFloorProbe.x, z: _maStlInsertionFloorProbe.z };
        const proximity = {
            clientX: clientX,
            clientY: clientY,
            camera: cam,
            canvas: canvas,
            maxDim: lastMaxDim,
            pickScreenPxBoost:
                pickScreenPxBoost != null && pickScreenPxBoost > 0 ? pickScreenPxBoost : 0
        };
        const lineSnap = maStlFindLineToolVertexSnapCandidate(
            clientX,
            clientY,
            floorHitObj,
            proximity
        );
        return lineSnap && lineSnap.active ? lineSnap : null;
    }

    /**
     * Punto en planta: snap extendido (cruce / arista / centro) si está activo; si no, impacto libre en el suelo.
     */
    function maStlResolveLineToolFloorPointMm(clientX, clientY) {
        if (!renderer) return null;
        const canvas = renderer.domElement;
        const cam = activeCamera();
        if (
            !maStlClientRayToWorkspaceFloor(
                clientX,
                clientY,
                canvas,
                cam,
                orbitPivotNdc,
                orbitPivotRaycaster,
                _maStlInsertionFloorProbe
            )
        ) {
            return null;
        }
        const rawX = _maStlInsertionFloorProbe.x;
        const rawZ = _maStlInsertionFloorProbe.z;
        const lineSnap = maStlFindFloorLineVertexSnapAtPointer(
            clientX,
            clientY,
            MA_STL_LINE_TOOL_GRID_PICK_SCREEN_PX_BOOST
        );
        if (lineSnap) {
            return {
                x: lineSnap.x,
                y: lineSnap.y,
                z: lineSnap.z,
                maStlLineVertexSnap: true
            };
        }
        const proximity = {
            clientX: clientX,
            clientY: clientY,
            camera: cam,
            canvas: canvas,
            maxDim: lastMaxDim,
            pickScreenPxBoost: MA_STL_LINE_TOOL_GRID_PICK_SCREEN_PX_BOOST
        };
        const gridSnap = maStlSnapFloorToGridFeatures({ x: rawX, z: rawZ }, proximity, desing2EnvGridSnapMm);
        if (gridSnap.active) {
            return { x: gridSnap.x, y: gridSnap.y, z: gridSnap.z };
        }
        return {
            x: rawX,
            y: MA_STL_DESING2_WORKSPACE_FLOOR_Y_MM,
            z: rawZ
        };
    }

    function maStlUpdateLineToolFloorHover(clientX, clientY) {
        if (!maStlIsLineToolPlacementActive() || !maStlDesingV2Viewer || !renderer) {
            return false;
        }
        const canvas = renderer.domElement;
        const cam = activeCamera();
        const floorHit = maStlClientRayToWorkspaceFloor(
            clientX,
            clientY,
            canvas,
            cam,
            orbitPivotNdc,
            orbitPivotRaycaster,
            _maStlInsertionFloorProbe
        );
        if (!floorHit) {
            maStlClearGridIntersectionPickHighlight();
            maStlClearLineToolVertexSnapHighlight();
            maStlSyncLineToolHud();
            if (maStlLineToolState === 'picking2') {
                maStlLineToolRefreshPicking2RubberBand();
            }
            return false;
        }
        const floorHitObj = { x: _maStlInsertionFloorProbe.x, z: _maStlInsertionFloorProbe.z };
        const lineSnap = maStlFindFloorLineVertexSnapAtPointer(
            clientX,
            clientY,
            MA_STL_LINE_TOOL_GRID_PICK_SCREEN_PX_BOOST
        );
        if (lineSnap) {
            maStlSetLineToolVertexSnapHighlight(lineSnap, true);
            maStlClearGridIntersectionPickHighlight();
            return true;
        }
        maStlClearLineToolVertexSnapHighlight();
        const proximity = {
            clientX: clientX,
            clientY: clientY,
            camera: cam,
            canvas: canvas,
            maxDim: lastMaxDim,
            pickScreenPxBoost: MA_STL_LINE_TOOL_GRID_PICK_SCREEN_PX_BOOST
        };
        const gridSnap = maStlSnapFloorToGridFeatures(floorHitObj, proximity, desing2EnvGridSnapMm);
        maStlSetGridIntersectionPickHighlight(gridSnap.active ? 'connected' : 'idle', gridSnap);
        return true;
    }

    function maStlEnsureLineToolRubberBand() {
        if (maStlLineToolRubberBandLine) return;
        const geo = new LineGeometry();
        geo.setPositions(new Float32Array(6));
        const mat = maStlEnsureUserFloorLineMat();
        maStlLineToolRubberBandLine = new Line2(geo, mat);
        maStlLineToolRubberBandLine.renderOrder = 169;
        maStlLineToolRubberBandLine.visible = false;
        maStlDisableRaycastOnOverlay(maStlLineToolRubberBandLine);
        scene.add(maStlLineToolRubberBandLine);
    }

    function maStlUpdateLineToolRubberBandMm(p2x, p2y, p2z) {
        if (maStlLineToolState !== 'picking2') {
            if (maStlLineToolRubberBandLine) {
                maStlLineToolRubberBandLine.visible = false;
            }
            return;
        }
        maStlEnsureLineToolRubberBand();
        maStlSetUserFloorLineGeometryMm(maStlLineToolRubberBandLine, maStlLineToolPoint1Mm, {
            x: p2x,
            y: p2y,
            z: p2z
        });
        maStlLineToolRubberBandLine.visible = true;
    }

    function maStlLineToolHidePreviewDimHud() {
        if (!maStlLineToolPreviewDimActive) return;
        maStlLineToolPreviewDimActive = false;
        const h = maStlEnsureUserFloorDimDomHud();
        if (h && h.root) {
            h.root.classList.remove('desing2-stl-floor-dim-overlay--line-tool-preview');
        }
        maStlHideUserFloorLineDimHud(true);
    }

    /** Cotas CAD + readouts DOM en vivo (`picking2`): longitud P1→P2; ΔX/ΔZ ancla reglas→P1 (mismo esquema que hover). */
    function maStlLineToolUpdatePreviewDimHud() {
        if (maStlLineToolState !== 'picking2' || !maStlDesingV2Viewer) {
            maStlLineToolHidePreviewDimHud();
            return;
        }
        const end = maStlLineToolComputeRubberBandEndMm(_maStlLineToolRubberEndMm);
        if (!end) {
            maStlLineToolHidePreviewDimHud();
            return;
        }
        const ud = maStlLineToolPreviewDimUd;
        ud.p1Mm.x = maStlLineToolPoint1Mm.x;
        ud.p1Mm.y = maStlLineToolPoint1Mm.y;
        ud.p1Mm.z = maStlLineToolPoint1Mm.z;
        ud.p2Mm.x = end.x;
        ud.p2Mm.y = end.y;
        ud.p2Mm.z = end.z;
        if (maStlUserFloorLineLengthMm(ud) < maStlUserFloorSegmentMinMm() - 1e-9) {
            maStlLineToolHidePreviewDimHud();
            return;
        }
        const mergedSpan = maStlFindUserFloorCollinearExtensionMergeSpanMm(ud);
        const dimUd = mergedSpan || ud;
        maStlLineToolPreviewDimActive = true;
        maStlRebuildUserFloorDimGuideGeometry(dimUd);
        if (!maStlUserFloorDimProjectHudReadoutScreens()) {
            maStlLineToolHidePreviewDimHud();
            return;
        }
        const hdom = maStlEnsureUserFloorDimDomHud();
        if (
            !hdom ||
            !hdom.root ||
            !hdom.readoutBtn ||
            !hdom.readoutDxBtn ||
            !hdom.readoutDzBtn
        ) {
            return;
        }
        hdom.root.hidden = false;
        hdom.root.removeAttribute('hidden');
        hdom.root.setAttribute('aria-hidden', 'false');
        hdom.root.classList.add('desing2-stl-floor-dim-overlay--line-tool-preview');
        const dxHud = ud.p1Mm.x - maStlRulerAnchorMm.x;
        const dzHud = ud.p1Mm.z - maStlRulerAnchorMm.z;
        hdom.readoutBtn.hidden = false;
        hdom.readoutBtn.removeAttribute('hidden');
        hdom.readoutBtn.setAttribute('aria-hidden', 'false');
        hdom.readoutBtn.textContent = maStlUserFloorLineDimensionLabelMm(dimUd);
        hdom.readoutDxBtn.hidden = false;
        hdom.readoutDxBtn.removeAttribute('hidden');
        hdom.readoutDxBtn.setAttribute('aria-hidden', 'false');
        hdom.readoutDxBtn.textContent = maStlDesing2SignedDeltaMetersDisplayFromMm(dxHud);
        hdom.readoutDzBtn.hidden = false;
        hdom.readoutDzBtn.removeAttribute('hidden');
        hdom.readoutDzBtn.setAttribute('aria-hidden', 'false');
        hdom.readoutDzBtn.textContent = maStlDesing2SignedDeltaMetersDisplayFromMm(dzHud);
        maStlPlaceAllFloorDimHudReadouts();
        maStlApplyFloorDimHudReadoutBrightClass(null);
        maStlApplyUserFloorDimDomHudTheme(hdom.readoutBtn);
        maStlApplyUserFloorDimDomHudTheme(hdom.readoutDxBtn);
        maStlApplyUserFloorDimDomHudTheme(hdom.readoutDzBtn);
    }

    /** Reposiciona cotas preview al orbitar/zoom sin recalcular geometría CAD. */
    function maStlLineToolSyncPreviewDimHudScreenOnly() {
        if (!maStlLineToolPreviewDimActive || maStlLineToolState !== 'picking2') return;
        if (!maStlUserFloorDimProjectHudReadoutScreens()) return;
        const hdom = maStlEnsureUserFloorDimDomHud();
        if (!hdom || !hdom.root) return;
        maStlPlaceAllFloorDimHudReadouts();
    }

    /**
     * Tras crear un segmento: permanece en herramienta línea (`picking1`) para el siguiente trazo.
     * Órbita sigue bloqueada hasta Escape o clic izquierdo en lienzo vacío (sin snap/colocación).
     */
    function maStlLineToolResetForNextSegment() {
        if (!maStlLineToolState) return;
        maStlLineToolHidePreviewDimHud();
        maStlLineToolState = 'picking1';
        maStlResetLineToolPickingBaselineState();
        if (maStlLineToolRubberBandLine) {
            maStlLineToolRubberBandLine.visible = false;
        }
        maStlClearGridIntersectionPickHighlight();
        maStlClearLineToolVertexSnapHighlight();
        maStlSyncLineToolHud();
        maStlSyncLineToolToggleBtnUi();
        maStlLineToolPickCursorSync();
        syncMaStlRulerAnchorPickCursor();
    }

    /**
     * @param {boolean} deferOrbitUnlock reservado; salida completa siempre rehabilita órbita al instante.
     */
    function maStlStopLineToolModesToolbar(deferOrbitUnlock) {
        if (!maStlLineToolState) return;
        maStlRefactorUserFloorLinesMergeCollinear();
        maStlLineToolHidePreviewDimHud();
        maStlLineToolState = null;
        maStlResetLineToolPickingBaselineState();
        if (maStlLineToolRubberBandLine) {
            maStlLineToolRubberBandLine.visible = false;
        }
        maStlClearGridIntersectionPickHighlight();
        maStlClearLineToolVertexSnapHighlight();
        maStlSyncLineToolHud();
        maStlSyncLineToolToggleBtnUi();
        maStlLineToolPickCursorSync();
        syncMaStlRulerAnchorPickCursor();
        /* Línea no mueve anclaje de reglas → nunca preserveView→ruler en unlock (misma filosofía que defer tras pan). */
        maStlUnlockOrbitForRulerAnchorPick(true);
    }

    /**
     * Desing_2: Escape / cancelación unificada — línea en curso, pick anclaje rejilla/objeto, lock temporal de órbita,
     * highlights STL/rejilla e HUD asociados. Idempotente.
     */
    function maStlDesing2CancelTransientToolsEscape() {
        if (!maStlDesingV2Viewer) return;
        maStlDisposeUserFloorLineDimEdit(false);
        maStlStopLineToolModesToolbar(false);
        if (maStlIsRulerAnchorPickModeActive()) {
            maStlStopRulerAnchorPickModesToolbar();
        } else {
            maStlUnlockOrbitForRulerAnchorPick();
        }
        maStlTeardownPickHighlightsOnly();
        syncMaStlRulerPickToolbarUi();
        maStlSyncLineToolToggleBtnUi();
        maStlSyncLineToolHud();
        maStlSyncRulerAnchorCoordsHud();
        syncMaStlRulerAnchorPickCursor();
        maStlLineToolPickCursorSync();
    }

    function maStlCommitUserPlanLineSegmentMm(a, b) {
        const weldEps = maStlUserFloorLineMergeEndpointEpsMm();
        maStlWeldUserFloorPlanPointToExistingEndpointsMm(a, weldEps);
        maStlWeldUserFloorPlanPointToExistingEndpointsMm(b, weldEps);
        const geo = maStlCreateUserFloorLineGeometryMm(a, b);
        const um = maStlEnsureUserFloorLineMat();
        const mat = um.clone ? um.clone() : um;
        const line = new Line2(geo, mat);
        line.renderOrder = 168;
        line.userData.maStlUserPlanLine = {
            id: maStlUserFloorLineNextSegId++,
            /** Primer clic herramienta línea (**fijo** al editar longitud). */
            p1Mm: { x: a.x, y: a.y, z: a.z },
            /** Segundo clic: se mueve manteniendo la dirección 3D desde p1. */
            p2Mm: { x: b.x, y: b.y, z: b.z },
            /** @future Extensible: cotas HUD / restricciones adicionales en la misma estructura. */
        };
        maStlDisableRaycastOnOverlay(line);
        maStlUserLinesGroup.add(line);
        maStlTryMergeUserFloorLineWithConnected(line);
    }

    function maStlUserFloorLineMergeEndpointEpsMm() {
        const snapWorldMm =
            desing2EnvGridSnapMm > 0
                ? desing2EnvGridSnapMm * MA_STL_LINE_TOOL_VERTEX_SNAP_WORLD_MM_FACTOR
                : 0;
        return Math.max(
            MA_STL_USER_FLOOR_LINE_MERGE_ENDPOINT_EPS_MM,
            maStlUserFloorSegmentMinMm() * 0.25,
            MA_STL_LINE_TOOL_DIR_EPS_MM * 2,
            snapWorldMm > 0 ? snapWorldMm * 0.004 : 0
        );
    }

    function maStlUserFloorPlanPointEqualMm(p, q, eps) {
        return (
            Math.abs(p.x - q.x) <= eps &&
            Math.abs(p.y - q.y) <= eps &&
            Math.abs(p.z - q.z) <= eps
        );
    }

    /** Igualdad en planta XZ (tolerancia merge / snap herramienta línea). */
    function maStlUserFloorPlanPointEqualXzMm(p, q, eps) {
        return Math.abs(p.x - q.x) <= eps && Math.abs(p.z - q.z) <= eps;
    }

    /** Ajusta `p` al vértice P1/P2 existente más cercano en XZ (≤ `eps`). */
    function maStlWeldUserFloorPlanPointToExistingEndpointsMm(p, eps) {
        if (!p || !maStlUserLinesGroup) return false;
        const ch = maStlUserLinesGroup.children;
        for (let i = 0; i < ch.length; i++) {
            const line = ch[i];
            if (!maStlIsUserFloorPlanLineObject(line)) continue;
            const ud = line.userData && line.userData.maStlUserPlanLine;
            if (!ud || !ud.p1Mm || !ud.p2Mm) continue;
            const ends = [ud.p1Mm, ud.p2Mm];
            for (let ei = 0; ei < ends.length; ei++) {
                const ep = ends[ei];
                if (maStlUserFloorPlanPointEqualXzMm(p, ep, eps)) {
                    p.x = ep.x;
                    p.y = ep.y;
                    p.z = ep.z;
                    return true;
                }
            }
        }
        return false;
    }

    /** Dirección unitaria y longitud en planta XZ (`p1Mm` → `p2Mm`). */
    function maStlUserFloorPlanLineDirUnitXz(ud) {
        if (!ud || !ud.p1Mm || !ud.p2Mm) return null;
        const dx = ud.p2Mm.x - ud.p1Mm.x;
        const dz = ud.p2Mm.z - ud.p1Mm.z;
        const len = Math.hypot(dx, dz);
        if (!Number.isFinite(len) || len < 1e-9) return null;
        return { ux: dx / len, uz: dz / len, lenMm: len };
    }

    /**
     * Dirección unitaria en XZ **saliente** del vértice `atEnd` (`'p1'`|`'p2'`) — hacia el exterior de la polilínea en la junta.
     * (Reservado; la fusión colineal usa {@link maStlUserFloorPlanLinesCollinearSameSenseXzAtShare} con dirs `p1→p2`.)
     */
    function maStlUserFloorPlanLineOutwardDirUnitXz(ud, atEnd) {
        const d = maStlUserFloorPlanLineDirUnitXz(ud);
        if (!d) return null;
        if (atEnd === 'p2') {
            return { ux: d.ux, uz: d.uz };
        }
        return { ux: -d.ux, uz: -d.uz };
    }

    /**
     * Vértice compartido entre dos segmentos (`onA`/`onB` = `'p1'`|`'p2'`).
     * @returns {{ onA: string, onB: string }|null}
     */
    function maStlUserFloorPlanLinesShareEndpointMm(udA, udB, eps) {
        if (!udA || !udB || !udA.p1Mm || !udA.p2Mm || !udB.p1Mm || !udB.p2Mm) return null;
        const endsA = [
            { onA: 'p1', p: udA.p1Mm },
            { onA: 'p2', p: udA.p2Mm },
        ];
        const endsB = [
            { onB: 'p1', p: udB.p1Mm },
            { onB: 'p2', p: udB.p2Mm },
        ];
        for (let ai = 0; ai < endsA.length; ai++) {
            for (let bi = 0; bi < endsB.length; bi++) {
                if (maStlUserFloorPlanPointEqualXzMm(endsA[ai].p, endsB[bi].p, eps)) {
                    return { onA: endsA[ai].onA, onB: endsB[bi].onB };
                }
            }
        }
        return null;
    }

    /**
     * Colineales y extensión de **cadena** en la junta (`onA`/`onB` = `'p1'`|`'p2'` compartidos).
     * Usa `p1→p2` de cada segmento: en `(p2,p1)`/`(p1,p2)` deben alinearse (`dot>0`); en `(p1,p1)`/`(p2,p2)`
     * deben oponerse (`dot<0`). Las dirs “salientes” fallaban en el caso habitual `(p2,p1)`.
     */
    function maStlUserFloorPlanLinesCollinearSameSenseXzAtShare(udA, udB, onA, onB) {
        const dA = maStlUserFloorPlanLineDirUnitXz(udA);
        const dB = maStlUserFloorPlanLineDirUnitXz(udB);
        if (!dA || !dB) return false;
        const cross = dA.ux * dB.uz - dA.uz * dB.ux;
        if (Math.abs(cross) > MA_STL_USER_FLOOR_LINE_MERGE_COLLINEAR_CROSS_MAX) return false;
        const dot = dA.ux * dB.ux + dA.uz * dB.uz;
        if (onA === onB) {
            return dot < -MA_STL_USER_FLOOR_LINE_MERGE_SAME_SENSE_DOT_MIN;
        }
        return dot > MA_STL_USER_FLOOR_LINE_MERGE_SAME_SENSE_DOT_MIN;
    }

    /** Tramo combinado tras fusionar `baseUd` con `newUd` en la junta indicada. */
    function maStlUserFloorPlanLineMergedSpanMm(baseUd, newUd, shareOnBase, shareOnNew) {
        const newOuter = shareOnNew === 'p1' ? newUd.p2Mm : newUd.p1Mm;
        if (shareOnBase === 'p2') {
            return { p1Mm: baseUd.p1Mm, p2Mm: newOuter };
        }
        return { p1Mm: newOuter, p2Mm: baseUd.p2Mm };
    }

    /**
     * Si `newUd` extiende colinealmente a **un único** segmento existente, devuelve el tramo fusionado para cotas preview.
     * @param {{ p1Mm: *, p2Mm: * }} newUd
     * @returns {{ p1Mm: *, p2Mm: * }|null}
     */
    function maStlFindUserFloorCollinearExtensionMergeSpanMm(newUd) {
        if (!newUd || !newUd.p1Mm || !newUd.p2Mm || !maStlUserLinesGroup) return null;
        const eps = maStlUserFloorLineMergeEndpointEpsMm();
        const partners = [];
        const ch = maStlUserLinesGroup.children;
        for (let i = 0; i < ch.length; i++) {
            const line = ch[i];
            if (!maStlIsUserFloorPlanLineObject(line)) continue;
            const ud = line.userData && line.userData.maStlUserPlanLine;
            if (!ud) continue;
            const share = maStlUserFloorPlanLinesShareEndpointMm(newUd, ud, eps);
            if (!share) continue;
            if (!maStlUserFloorPlanLinesCollinearSameSenseXzAtShare(newUd, ud, share.onA, share.onB)) continue;
            partners.push({
                ud: ud,
                shareOnExisting: share.onB,
                shareOnNew: share.onA,
            });
        }
        if (partners.length !== 1) return null;
        const p = partners[0];
        return maStlUserFloorPlanLineMergedSpanMm(p.ud, newUd, p.shareOnExisting, p.shareOnNew);
    }

    /**
     * Extiende `baseUd` (segmento ya en escena) hasta el vértice libre de `otherUd` en el junta compartido.
     * Conserva `p1Mm` como punto inicial del trazo base salvo extensión por el extremo `p1`.
     */
    function maStlMergeUserFloorPlanLineEndpointsOnBase(baseUd, otherUd, shareOnBase, shareOnOther) {
        const otherOuter =
            shareOnOther === 'p1' ? otherUd.p2Mm : otherUd.p1Mm;
        if (shareOnBase === 'p2') {
            baseUd.p2Mm.x = otherOuter.x;
            baseUd.p2Mm.y = otherOuter.y;
            baseUd.p2Mm.z = otherOuter.z;
            return;
        }
        const baseOuter = baseUd.p2Mm;
        baseUd.p1Mm.x = otherOuter.x;
        baseUd.p1Mm.y = otherOuter.y;
        baseUd.p1Mm.z = otherOuter.z;
        baseUd.p2Mm.x = baseOuter.x;
        baseUd.p2Mm.y = baseOuter.y;
        baseUd.p2Mm.z = baseOuter.z;
    }

    function maStlDisposeUserFloorPlanLineObject(line) {
        if (!line || !maStlUserLinesGroup) return;
        if (maStlHoveredUserFloorLine === line) {
            maStlClearUserFloorLineHover();
            maStlHideUserFloorLineDimHud(true);
        }
        if (maStlUserFloorLineDimEditLineRef === line) {
            maStlDisposeUserFloorLineDimEdit(false);
        }
        maStlUserLinesGroup.remove(line);
        disposeObject3D(line);
    }

    /**
     * Tras confirmar un trazo: fusiona con **un único** segmento existente si comparten vértice,
     * son colineales en XZ y van en el **mismo sentido**. Repite para cadenas colineales.
     * @param {THREE.Line2} lnNew segmento recién añadido
     * @returns {THREE.Line2} línea resultante (fusionada o `lnNew`)
     */
    function maStlTryMergeUserFloorLineWithConnected(lnNew) {
        if (!lnNew || !maStlIsUserFloorPlanLineObject(lnNew)) return lnNew;
        let current = lnNew;
        for (;;) {
            const udNew = current.userData && current.userData.maStlUserPlanLine;
            if (!udNew) break;
            const eps = maStlUserFloorLineMergeEndpointEpsMm();
            const partners = [];
            const ch = maStlUserLinesGroup.children;
            for (let i = 0; i < ch.length; i++) {
                const line = ch[i];
                if (line === current || !maStlIsUserFloorPlanLineObject(line)) continue;
                const ud = line.userData.maStlUserPlanLine;
                if (!ud) continue;
                const share = maStlUserFloorPlanLinesShareEndpointMm(udNew, ud, eps);
                if (!share) continue;
                if (!maStlUserFloorPlanLinesCollinearSameSenseXzAtShare(udNew, ud, share.onA, share.onB)) continue;
                partners.push({
                    line: line,
                    shareOnExisting: share.onB,
                    shareOnNew: share.onA,
                });
            }
            if (partners.length !== 1) break;
            const partner = partners[0];
            const baseLine = partner.line;
            const baseUd = baseLine.userData.maStlUserPlanLine;
            maStlMergeUserFloorPlanLineEndpointsOnBase(
                baseUd,
                udNew,
                partner.shareOnExisting,
                partner.shareOnNew
            );
            maStlApplyUserFloorLineSegmentGeometryFromMm(baseLine);
            maStlDisposeUserFloorPlanLineObject(current);
            current = baseLine;
            if (maStlHoveredUserFloorLine === baseLine && !maStlUserFloorDimDomHudEditing) {
                maStlUpdateUserFloorLineDimHud();
            }
        }
        return current;
    }

    /** Solda iterativamente todos los extremos P1/P2 al mismo vértice XZ (≤ ε merge). */
    function maStlWeldAllUserFloorLineEndpointsMm() {
        if (!maStlUserLinesGroup) return;
        const eps = maStlUserFloorLineMergeEndpointEpsMm();
        let welded = true;
        while (welded) {
            welded = false;
            const ch = maStlUserLinesGroup.children;
            for (let i = 0; i < ch.length; i++) {
                const line = ch[i];
                if (!maStlIsUserFloorPlanLineObject(line)) continue;
                const ud = line.userData && line.userData.maStlUserPlanLine;
                if (!ud || !ud.p1Mm || !ud.p2Mm) continue;
                const p1Before = { x: ud.p1Mm.x, z: ud.p1Mm.z };
                const p2Before = { x: ud.p2Mm.x, z: ud.p2Mm.z };
                maStlWeldUserFloorPlanPointToExistingEndpointsMm(ud.p1Mm, eps);
                maStlWeldUserFloorPlanPointToExistingEndpointsMm(ud.p2Mm, eps);
                if (
                    Math.abs(ud.p1Mm.x - p1Before.x) > 1e-9 ||
                    Math.abs(ud.p1Mm.z - p1Before.z) > 1e-9 ||
                    Math.abs(ud.p2Mm.x - p2Before.x) > 1e-9 ||
                    Math.abs(ud.p2Mm.z - p2Before.z) > 1e-9
                ) {
                    welded = true;
                    maStlApplyUserFloorLineSegmentGeometryFromMm(line);
                }
            }
        }
    }

    /**
     * Refactor global: soldar vértices y fusionar cadenas colineales mismo sentido hasta estabilizar.
     * @returns {number} segmentos eliminados por fusión
     */
    function maStlRefactorUserFloorLinesMergeCollinear() {
        if (!maStlUserLinesGroup) return 0;
        maStlWeldAllUserFloorLineEndpointsMm();
        let removed = 0;
        let passMerged = true;
        while (passMerged) {
            passMerged = false;
            const snapshot = [];
            const ch = maStlUserLinesGroup.children;
            for (let i = 0; i < ch.length; i++) {
                const ln = ch[i];
                if (maStlIsUserFloorPlanLineObject(ln)) snapshot.push(ln);
            }
            for (let si = 0; si < snapshot.length; si++) {
                const ln = snapshot[si];
                if (!ln.parent || ln.parent !== maStlUserLinesGroup) continue;
                const beforeCount = maStlUserLinesGroup.children.length;
                maStlTryMergeUserFloorLineWithConnected(ln);
                const delta = beforeCount - maStlUserLinesGroup.children.length;
                if (delta > 0) {
                    removed += delta;
                    passMerged = true;
                    break;
                }
            }
        }
        if (removed > 0 && maStlHoveredUserFloorLine && !maStlUserFloorDimDomHudEditing) {
            maStlUpdateUserFloorLineDimHud();
        }
        return removed;
    }

    function maStlUserFloorLineLengthMm(ud) {
        if (!ud || !ud.p1Mm || !ud.p2Mm) return 0;
        const dx = ud.p2Mm.x - ud.p1Mm.x;
        const dy = ud.p2Mm.y - ud.p1Mm.y;
        const dz = ud.p2Mm.z - ud.p1Mm.z;
        return Math.sqrt(dx * dx + dy * dy + dz * dz);
    }

    function maStlApplyUserFloorLineSegmentGeometryFromMm(line) {
        const ud = line && line.userData && line.userData.maStlUserPlanLine;
        if (!ud) return;
        maStlSetUserFloorLineGeometryMm(line, ud.p1Mm, ud.p2Mm);
    }

    function maStlUserFloorSegmentMinMm() {
        return MA_STL_DESING2_GRID_MINOR_MM * 0.001;
    }

    /**
     * Ajuste de longitud de segmento herramienta línea usuario (persiste dirección desde `p1Mm`).
     * Nombre alternativo esperado por integradores: `maStlResizeUserPlanLineSegmentMm`.
     */
    function maStlResizeUserFloorLineToLengthMm(line, newLenMm) {
        if (!line || !line.userData || !line.userData.maStlUserPlanLine) return;
        const ud = line.userData.maStlUserPlanLine;
        const a = ud.p1Mm;
        const b = ud.p2Mm;
        if (!a || !b) return;
        const dx = b.x - a.x;
        const dy = b.y - a.y;
        const dz = b.z - a.z;
        const cur = Math.sqrt(dx * dx + dy * dy + dz * dz);
        const minMm = maStlUserFloorSegmentMinMm();
        const target = THREE.MathUtils.clamp(newLenMm, minMm, lastMaxDim * 25);
        if (!(cur >= minMm)) {
            ud.p2Mm.x = a.x + target;
            ud.p2Mm.y = a.y;
            ud.p2Mm.z = a.z;
        } else {
            const scale = target / cur;
            ud.p2Mm.x = a.x + dx * scale;
            ud.p2Mm.y = a.y + dy * scale;
            ud.p2Mm.z = a.z + dz * scale;
        }
        maStlApplyUserFloorLineSegmentGeometryFromMm(line);
    }

    /** Tope habitual ~25 m físicos desde reglas/extent (mm escena). */
    function maStlUserFloorPlanLineCapMm() {
        return lastMaxDim * 25;
    }

    /** Cota ΔX editable desde **anclaje reglas**: traslada el segmento en X para que `P1.x = ref.x + Δ` (P1 y P2 se mueven igual; ref = `{@link maStlRulerAnchorMm}` en planta). */
    function maStlResizeUserFloorLinePlanDeltaXMm(line, signedDeltaXmMmVsRulerAnchor) {
        if (!line || !line.userData || !line.userData.maStlUserPlanLine) return false;
        const ud = line.userData.maStlUserPlanLine;
        const p1 = ud.p1Mm;
        const p2 = ud.p2Mm;
        if (!p1 || !p2) return false;
        const snapP1 = { x: p1.x, y: p1.y, z: p1.z };
        const snapP2 = { x: p2.x, y: p2.y, z: p2.z };
        const refX = maStlRulerAnchorMm.x;
        const cap = maStlUserFloorPlanLineCapMm();
        const rnd = maStlDesing2SignedDeltaMmRoundedEditableFromMm(signedDeltaXmMmVsRulerAnchor);
        const newDx = THREE.MathUtils.clamp(rnd, -cap, cap);
        const shiftX = newDx - (p1.x - refX);
        if (Math.abs(shiftX) < 1e-12) return true;
        p1.x += shiftX;
        p2.x += shiftX;
        const minMm = maStlUserFloorSegmentMinMm();
        if (maStlUserFloorLineLengthMm(ud) < minMm) {
            p1.x = snapP1.x;
            p1.y = snapP1.y;
            p1.z = snapP1.z;
            p2.x = snapP2.x;
            p2.y = snapP2.y;
            p2.z = snapP2.z;
            return false;
        }
        maStlApplyUserFloorLineSegmentGeometryFromMm(line);
        return true;
    }

    /** Cota ΔZ editable desde **anclaje reglas**: traslada el segmento en Z para que `P1.z = ref.z + Δ` (P1 y P2 se mueven igual; ref en planta `{@link maStlRulerAnchorMm}`). */
    function maStlResizeUserFloorLinePlanDeltaZMm(line, signedDeltaZmMmVsRulerAnchor) {
        if (!line || !line.userData || !line.userData.maStlUserPlanLine) return false;
        const ud = line.userData.maStlUserPlanLine;
        const p1 = ud.p1Mm;
        const p2 = ud.p2Mm;
        if (!p1 || !p2) return false;
        const snapP1 = { x: p1.x, y: p1.y, z: p1.z };
        const snapP2 = { x: p2.x, y: p2.y, z: p2.z };
        const refZ = maStlRulerAnchorMm.z;
        const cap = maStlUserFloorPlanLineCapMm();
        const rnd = maStlDesing2SignedDeltaMmRoundedEditableFromMm(signedDeltaZmMmVsRulerAnchor);
        const newDz = THREE.MathUtils.clamp(rnd, -cap, cap);
        const shiftZ = newDz - (p1.z - refZ);
        if (Math.abs(shiftZ) < 1e-12) return true;
        p1.z += shiftZ;
        p2.z += shiftZ;
        const minMm = maStlUserFloorSegmentMinMm();
        if (maStlUserFloorLineLengthMm(ud) < minMm) {
            p1.x = snapP1.x;
            p1.y = snapP1.y;
            p1.z = snapP1.z;
            p2.x = snapP2.x;
            p2.y = snapP2.y;
            p2.z = snapP2.z;
            return false;
        }
        maStlApplyUserFloorLineSegmentGeometryFromMm(line);
        return true;
    }

    /** Traslada P1 y P2 en planta (ΔX/ΔZ iguales; Y suelo fijo). */
    function maStlTranslateUserFloorLineSegmentPlanMm(line, deltaXMm, deltaZMm) {
        if (!line || !line.userData || !line.userData.maStlUserPlanLine) return false;
        const ud = line.userData.maStlUserPlanLine;
        const p1 = ud.p1Mm;
        const p2 = ud.p2Mm;
        if (!p1 || !p2) return false;
        if (Math.abs(deltaXMm) < 1e-12 && Math.abs(deltaZMm) < 1e-12) return true;
        const snapP1 = { x: p1.x, y: p1.y, z: p1.z };
        const snapP2 = { x: p2.x, y: p2.y, z: p2.z };
        p1.x += deltaXMm;
        p2.x += deltaXMm;
        p1.z += deltaZMm;
        p2.z += deltaZMm;
        const minMm = maStlUserFloorSegmentMinMm();
        if (maStlUserFloorLineLengthMm(ud) < minMm) {
            p1.x = snapP1.x;
            p1.y = snapP1.y;
            p1.z = snapP1.z;
            p2.x = snapP2.x;
            p2.y = snapP2.y;
            p2.z = snapP2.z;
            return false;
        }
        maStlApplyUserFloorLineSegmentGeometryFromMm(line);
        return true;
    }

    function maStlLockOrbitForUserFloorLineDrag() {
        if (!controls) return;
        if (!maStlUserFloorLineDragOrbitSnapshot) {
            maStlUserFloorLineDragOrbitSnapshot = {
                enabled: controls.enabled,
                enableRotate: !!controls.enableRotate,
                enablePan: !!controls.enablePan,
                enableZoom: !!controls.enableZoom,
            };
        }
        controls.enabled = false;
    }

    function maStlUnlockOrbitForUserFloorLineDrag() {
        if (!controls || !maStlUserFloorLineDragOrbitSnapshot) return;
        const snap = maStlUserFloorLineDragOrbitSnapshot;
        controls.enabled = snap.enabled;
        controls.enableRotate = snap.enableRotate;
        controls.enablePan = snap.enablePan;
        controls.enableZoom = snap.enableZoom;
        maStlUserFloorLineDragOrbitSnapshot = null;
    }

    function maStlEndUserFloorLineDragHandle(ev) {
        window.removeEventListener('pointermove', maStlOnUserFloorLineDragHandleMove, true);
        window.removeEventListener('pointerup', maStlEndUserFloorLineDragHandle, true);
        window.removeEventListener('pointercancel', maStlEndUserFloorLineDragHandle, true);
        if (!maStlUserFloorLineDragActive) return;
        maStlUserFloorLineDragActive = false;
        maStlUnlockOrbitForUserFloorLineDrag();
        const h = maStlEnsureUserFloorDimDomHud();
        if (h && h.dragHandle) {
            h.dragHandle.classList.remove('desing2-stl-floor-line-drag-handle--dragging');
            const pid = ev && ev.pointerId;
            if (pid != null && pid >= 0) {
                try {
                    if (h.dragHandle.hasPointerCapture && h.dragHandle.hasPointerCapture(pid)) {
                        h.dragHandle.releasePointerCapture(pid);
                    }
                } catch (_eCap) {}
            }
        }
    }

    function maStlOnUserFloorLineDragHandleMove(ev) {
        if (!maStlUserFloorLineDragActive) return;
        const line = maStlUserFloorLineDimEditLineRef;
        const canvas = renderer && renderer.domElement;
        const cam = activeCamera();
        if (
            !line ||
            !line.userData ||
            !line.userData.maStlUserPlanLine ||
            !canvas ||
            !cam
        ) {
            return;
        }
        if (
            !maStlClientRayToWorkspaceFloor(
                ev.clientX,
                ev.clientY,
                canvas,
                cam,
                orbitPivotNdc,
                orbitPivotRaycaster,
                _maStlUserFloorLineDragFloorPt
            )
        ) {
            return;
        }
        const deltaX = _maStlUserFloorLineDragFloorPt.x - _maStlUserFloorLineDragStartFloor.x;
        const deltaZ = _maStlUserFloorLineDragFloorPt.z - _maStlUserFloorLineDragStartFloor.z;
        const ud = line.userData.maStlUserPlanLine;
        ud.p1Mm.x = _maStlUserFloorLineDragOrigP1.x;
        ud.p1Mm.y = _maStlUserFloorLineDragOrigP1.y;
        ud.p1Mm.z = _maStlUserFloorLineDragOrigP1.z;
        ud.p2Mm.x = _maStlUserFloorLineDragOrigP2.x;
        ud.p2Mm.y = _maStlUserFloorLineDragOrigP2.y;
        ud.p2Mm.z = _maStlUserFloorLineDragOrigP2.z;
        maStlInvalidateUserFloorDimGuideGeomCache();
        maStlTranslateUserFloorLineSegmentPlanMm(line, deltaX, deltaZ);
        maStlRefreshUserFloorLineDimEditHudPositions();
    }

    function maStlOnUserFloorLineDragHandleDown(ev) {
        if (ev.button !== 0) return;
        const line = maStlUserFloorLineDimEditLineRef;
        const canvas = renderer && renderer.domElement;
        const cam = activeCamera();
        const h = maStlEnsureUserFloorDimDomHud();
        if (
            !maStlUserFloorDimDomHudEditing ||
            !line ||
            !line.userData ||
            !line.userData.maStlUserPlanLine ||
            !canvas ||
            !cam ||
            !h ||
            !h.dragHandle
        ) {
            return;
        }
        ev.preventDefault();
        ev.stopPropagation();
        maStlUserFloorLineDragSuppressBlurCommit = true;
        maStlClearUserFloorLineDimBlurTimer();
        if (
            !maStlClientRayToWorkspaceFloor(
                ev.clientX,
                ev.clientY,
                canvas,
                cam,
                orbitPivotNdc,
                orbitPivotRaycaster,
                _maStlUserFloorLineDragStartFloor
            )
        ) {
            maStlUserFloorLineDragSuppressBlurCommit = false;
            return;
        }
        const ud = line.userData.maStlUserPlanLine;
        _maStlUserFloorLineDragOrigP1.x = ud.p1Mm.x;
        _maStlUserFloorLineDragOrigP1.y = ud.p1Mm.y;
        _maStlUserFloorLineDragOrigP1.z = ud.p1Mm.z;
        _maStlUserFloorLineDragOrigP2.x = ud.p2Mm.x;
        _maStlUserFloorLineDragOrigP2.y = ud.p2Mm.y;
        _maStlUserFloorLineDragOrigP2.z = ud.p2Mm.z;
        maStlUserFloorLineDragActive = true;
        maStlLockOrbitForUserFloorLineDrag();
        h.dragHandle.classList.add('desing2-stl-floor-line-drag-handle--dragging');
        try {
            h.dragHandle.setPointerCapture(ev.pointerId);
        } catch (_eCap) {}
        window.addEventListener('pointermove', maStlOnUserFloorLineDragHandleMove, true);
        window.addEventListener('pointerup', maStlEndUserFloorLineDragHandle, true);
        window.addEventListener('pointercancel', maStlEndUserFloorLineDragHandle, true);
    }

    /** Texto de cota herramienta línea para UI (metros, Intl; entrada sin sufijo ⇒ metros). */
    function maStlUserFloorLineDimensionLabelMm(ud) {
        const lm = maStlUserFloorLineLengthMm(ud);
        return maStlDesing2DimEditableMetersDisplayFromMm(lm);
    }

    function maStlApplyUserFloorLineBaseMaterial(mat) {
        const ref = maStlEnsureUserFloorLineMat();
        mat.color.copy(ref.color);
        mat.opacity = ref.opacity;
        mat.transparent = ref.transparent;
        if (mat.isLineMaterial) {
            mat.linewidth = ref.linewidth;
        }
        mat.needsUpdate = true;
    }

    function maStlApplyUserFloorLineHoverBrightMaterial(mat) {
        maStlApplyUserFloorLineBaseMaterial(mat);
        mat.color.setHex(MA_STL_USER_FLOOR_LINE_HOVER_HEX);
        mat.needsUpdate = true;
    }

    function maStlEnsureUserFloorDimDomHud() {
        if (maStlUserFloorDimDomHud || !viewerShell) return maStlUserFloorDimDomHud;
        maStlUserFloorDimDomHud = {
            root: viewerShell.querySelector('#ma-stl-line-dim-edit-overlay'),
            readoutBtn: viewerShell.querySelector('#ma-stl-floor-dim-readout'),
            readoutDxBtn: viewerShell.querySelector('#ma-stl-floor-dim-readout-dx'),
            readoutDzBtn: viewerShell.querySelector('#ma-stl-floor-dim-readout-dz'),
            inputEl: viewerShell.querySelector('#ma-stl-floor-dim-input'),
            inputDxEl: viewerShell.querySelector('#ma-stl-floor-dim-input-dx'),
            inputDzEl: viewerShell.querySelector('#ma-stl-floor-dim-input-dz'),
            dragHandle: viewerShell.querySelector('#ma-stl-floor-line-drag-handle'),
            canvasWrap: viewerShell.querySelector('#master-article-details-stl-viewer-canvas'),
        };
        return maStlUserFloorDimDomHud;
    }

    /** @param {'length'|'deltaX'|'deltaZ'|null|undefined} kindMaybe */
    function maStlApplyFloorDimHudReadoutBrightClass(kindMaybe) {
        const h = maStlEnsureUserFloorDimDomHud();
        if (!h) return;
        const k = kindMaybe || null;
        if (h.readoutBtn) {
            h.readoutBtn.classList.toggle(
                'desing2-stl-floor-dim-readout--hot',
                k === 'length'
            );
        }
        if (h.readoutDxBtn) {
            h.readoutDxBtn.classList.toggle(
                'desing2-stl-floor-dim-readout--hot',
                k === 'deltaX'
            );
        }
        if (h.readoutDzBtn) {
            h.readoutDzBtn.classList.toggle(
                'desing2-stl-floor-dim-readout--hot',
                k === 'deltaZ'
            );
        }
    }

    function maStlResetFloorDimHudReadoutScreens() {
        maStlFloorDimHudReadoutScrPx.length.valid =
            maStlFloorDimHudReadoutScrPx.deltaX.valid =
            maStlFloorDimHudReadoutScrPx.deltaZ.valid =
                false;
    }

    /**
     * Proyecta midpoint cotas paralelas tras {@link maStlRebuildUserFloorDimGuideGeometry} (incluye ΔX / ΔZ plano — mm escena).
     */
    function maStlUserFloorDimProjectHudReadoutScreens() {
        maStlResetFloorDimHudReadoutScreens();
        const mwm = maStlUserFloorDimHudWorldMid;
        const fy = mwm.floorY;
        if (!mwm.midLen || !mwm.validLen) return false;
        if (!maStlWorldMmToScreenPx(mwm.midLen.x, fy, mwm.midLen.z, maStlFloorDimHudReadoutScrPx.length)) {
            return false;
        }
        maStlFloorDimHudReadoutScrPx.length.valid = true;
        maStlUserFloorLineDimPickScreenPx.x = maStlFloorDimHudReadoutScrPx.length.x;
        maStlUserFloorLineDimPickScreenPx.y = maStlFloorDimHudReadoutScrPx.length.y;
        maStlUserFloorLineDimPickScreenPxValid = true;
        if (
            mwm.midDx &&
            mwm.validDx &&
            maStlWorldMmToScreenPx(mwm.midDx.x, fy, mwm.midDx.z, maStlFloorDimHudReadoutScrPx.deltaX)
        )
            maStlFloorDimHudReadoutScrPx.deltaX.valid = true;
        if (
            mwm.midDz &&
            mwm.validDz &&
            maStlWorldMmToScreenPx(mwm.midDz.x, fy, mwm.midDz.z, maStlFloorDimHudReadoutScrPx.deltaZ)
        )
            maStlFloorDimHudReadoutScrPx.deltaZ.valid = true;
        return true;
    }

    function maStlUserFloorDimCopyDomRect(rr, box) {
        box.valid = rr.width >= 2 && rr.height >= 2;
        if (!box.valid) return;
        box.left = rr.left;
        box.top = rr.top;
        box.right = rr.right;
        box.bottom = rr.bottom;
    }

    /** Desplazo en px cliente para separar las 3 cajitas TinkerCAD. */
    function maStlUserFloorDimReadoutHudNudgeXY(kind /* 'length' | 'deltaX' | 'deltaZ' */, out2) {
        if (kind === 'length') {
            out2.x = -2;
            out2.y = -26;
            return;
        }
        if (kind === 'deltaX') {
            out2.x = 0;
            out2.y = 30;
            return;
        }
        out2.x = 54;
        out2.y = -10;
    }

    function maStlPlaceHudReadoutButtonAtScr(btn, scrSlot, nx, ny, rWrapLeft, rWrapTop) {
        if (!btn) return;
        if (!scrSlot || !scrSlot.valid) {
            btn.hidden = true;
            btn.setAttribute('aria-hidden', 'true');
            return;
        }
        btn.hidden = false;
        btn.removeAttribute('hidden');
        btn.setAttribute('aria-hidden', 'false');
        btn.style.position = 'absolute';
        btn.style.left = scrSlot.x - rWrapLeft + nx + 'px';
        btn.style.top = scrSlot.y - rWrapTop + ny + 'px';
        btn.style.transform = 'translate(-50%, -50%)';
        btn.style.pointerEvents = 'auto';
        maStlApplyUserFloorDimDomHudTheme(btn);
    }

    /** Posiciona cotas paralelas ΔX ΔZ sobre el canvas overlay (solo Desing_2). */
    function maStlPlaceAllFloorDimHudReadouts(/* evLike clientXY ignored; usa scr proyectados */) {
        const h = maStlEnsureUserFloorDimDomHud();
        if (!h || !h.root || !h.canvasWrap) return;
        const rWrap = h.canvasWrap.getBoundingClientRect();
        h.root.style.pointerEvents = 'none';
        h.root.hidden = false;
        h.root.setAttribute('aria-hidden', 'false');
        const nudge = { x: 0, y: 0 };
        maStlUserFloorDimReadoutHudNudgeXY('length', nudge);
        maStlPlaceHudReadoutButtonAtScr(
            h.readoutBtn,
            maStlFloorDimHudReadoutScrPx.length,
            nudge.x,
            nudge.y,
            rWrap.left,
            rWrap.top
        );
        maStlUserFloorDimReadoutHudNudgeXY('deltaX', nudge);
        maStlPlaceHudReadoutButtonAtScr(
            h.readoutDxBtn,
            maStlFloorDimHudReadoutScrPx.deltaX,
            nudge.x,
            nudge.y,
            rWrap.left,
            rWrap.top
        );
        maStlUserFloorDimReadoutHudNudgeXY('deltaZ', nudge);
        maStlPlaceHudReadoutButtonAtScr(
            h.readoutDzBtn,
            maStlFloorDimHudReadoutScrPx.deltaZ,
            nudge.x,
            nudge.y,
            rWrap.left,
            rWrap.top
        );
    }

    function maStlUserFloorDimPickReadoutKindAtPx(clientX, clientY, padPx) {
        const p = padPx != null ? padPx : MA_STL_USER_FLOOR_LINE_DIM_LABEL_HIT_PADDING_PX;
        const hits = [['deltaX', maStlUserFloorDimScrBoxDx], ['deltaZ', maStlUserFloorDimScrBoxDz], ['length', maStlUserFloorDimScrBoxLen]];
        for (let hi = 0; hi < hits.length; hi++) {
            const b = hits[hi][1];
            const kn = hits[hi][0];
            if (
                !b.valid ||
                clientX < b.left - p ||
                clientX > b.right + p ||
                clientY < b.top - p ||
                clientY > b.bottom + p
            ) {
                continue;
            }
            return /** @type {'length'|'deltaX'|'deltaZ'} */ (kn);
        }
        return null;
    }

    /**
     * Pick cotas CAD en pantalla: tramo usuario + acordes longitud / ΔX / ΔZ (mm escena → px).
     * @param {number} clientX
     * @param {number} clientY
     * @param {number} [maxPx]
     * @returns {'length'|'deltaX'|'deltaZ'|null}
     */
    function maStlUserFloorDimPickGuideKindAtPx(clientX, clientY, maxPx) {
        const mwm = maStlUserFloorDimHudWorldMid;
        if (!mwm.validLen) return null;
        const tolPx = maxPx != null ? maxPx : MA_STL_USER_FLOOR_LINE_SCREEN_PICK_PX + 8;
        const tolSq = tolPx * tolPx;
        const fy = mwm.floorY;
        let bestKind = null;
        let bestDSq = Infinity;
        const sp1 = { x: 0, y: 0 };
        const sp2 = { x: 0, y: 0 };

        function consider(kind, ax, az, bx, bz) {
            if (!maStlWorldMmToScreenPx(ax, fy, az, sp1)) return;
            if (!maStlWorldMmToScreenPx(bx, fy, bz, sp2)) return;
            const dSq = maStlSqDistPointToSegment2dPx(clientX, clientY, sp1.x, sp1.y, sp2.x, sp2.y);
            if (dSq <= tolSq && dSq < bestDSq) {
                bestDSq = dSq;
                bestKind = kind;
            }
        }

        if (mwm.chLnA && mwm.chLnB) {
            consider('length', mwm.chLnA.x, mwm.chLnA.z, mwm.chLnB.x, mwm.chLnB.z);
        }
        if (mwm.chLenA && mwm.chLenB) {
            consider('length', mwm.chLenA.x, mwm.chLenA.z, mwm.chLenB.x, mwm.chLenB.z);
        }
        if (mwm.drawDx && mwm.chDxA && mwm.chDxB) {
            consider('deltaX', mwm.chDxA.x, mwm.chDxA.z, mwm.chDxB.x, mwm.chDxB.z);
        }
        if (mwm.drawDz && mwm.chDzA && mwm.chDzB) {
            consider('deltaZ', mwm.chDzA.x, mwm.chDzA.z, mwm.chDzB.x, mwm.chDzB.z);
        }
        return bestKind;
    }

    function maStlUserFloorDimLabelScreenHitIncludesPx(clientX, clientY, padPx) {
        if (!maStlUserFloorLineDimPickScreenBBoxValid) return false;
        const p = padPx != null ? padPx : MA_STL_USER_FLOOR_LINE_DIM_LABEL_HIT_PADDING_PX;
        const b = maStlUserFloorLineDimPickScreenBBox;
        return (
            clientX >= b.left - p &&
            clientX <= b.right + p &&
            clientY >= b.top - p &&
            clientY <= b.bottom + p
        );
    }

    function maStlSyncUserFloorDimHudScreenOnly() {
        if (
            !maStlDesingV2Viewer ||
            !maStlHoveredUserFloorLine ||
            maStlUserFloorDimDomHudEditing ||
            maStlIsLineToolPlacementActive() ||
            maStlIsRulerAnchorPickModeActive()
        ) {
            return;
        }
        const udd = maStlHoveredUserFloorLine.userData && maStlHoveredUserFloorLine.userData.maStlUserPlanLine;
        if (!udd || !udd.p1Mm || !udd.p2Mm) return;
        maStlRebuildUserFloorDimGuideGeometry(udd);
        if (!maStlUserFloorDimProjectHudReadoutScreens()) return;
        const hdom = maStlEnsureUserFloorDimDomHud();
        if (
            !hdom ||
            !hdom.root ||
            !hdom.readoutBtn ||
            !hdom.readoutDxBtn ||
            !hdom.readoutDzBtn
        ) {
            return;
        }
        maStlPlaceAllFloorDimHudReadouts();
        maStlApplyFloorDimHudReadoutBrightClass(maStlUserFloorDimReadoutHoveredKind);
        maStlRefreshUserFloorDimReadoutHudScreenBBox();
    }

    function maStlHideFloorDimEditInputs(h) {
        if (!h) return;
        const rows = [h.inputEl, h.inputDxEl, h.inputDzEl];
        for (let i = 0; i < rows.length; i++) {
            const el = rows[i];
            if (!el) continue;
            el.hidden = true;
            el.style.visibility = 'hidden';
            el.style.pointerEvents = 'none';
            el.setAttribute('aria-hidden', 'true');
            el.value = '';
        }
    }

    /** Commit longitud + ΔX + ΔZ (orden: ΔX → ΔZ → longitud; traslación en planta, luego resize desde P1). */
    function maStlCommitUserFloorLineDimensionMulti(line, rawLen, rawDx, rawDz) {
        if (!line || !line.userData || !line.userData.maStlUserPlanLine) return false;
        maStlInvalidateUserFloorDimGuideGeomCache();
        let anyOk = false;
        const lenMm = maStlParseLengthInputValueToMm(rawLen);
        const dxMm = maStlParseLengthInputValueToMm(rawDx);
        const dzMm = maStlParseLengthInputValueToMm(rawDz);
        const minAllowed = maStlUserFloorSegmentMinMm();
        if (dxMm != null && maStlResizeUserFloorLinePlanDeltaXMm(line, dxMm)) anyOk = true;
        if (dzMm != null && maStlResizeUserFloorLinePlanDeltaZMm(line, dzMm)) anyOk = true;
        if (lenMm != null && lenMm >= minAllowed - 1e-9) {
            maStlResizeUserFloorLineToLengthMm(
                line,
                maStlDesing2LengthMmRoundedEditableFromMm(lenMm)
            );
            anyOk = true;
        }
        return anyOk;
    }

    function maStlFloorDimEditInputsActive(inputs) {
        const ae = document.activeElement;
        for (let i = 0; i < inputs.length; i++) {
            if (inputs[i] && ae === inputs[i]) return true;
        }
        return false;
    }

    function maStlApplyUserFloorDimDomHudTheme(el) {
        if (!el) return;
        el.classList.toggle('desing2-stl-floor-dim-readout--on-dark', !!darkBgVisible);
    }

    function maStlInvalidateUserFloorDimGuideGeomCache() {
        maStlUserFloorDimGuideGeomCacheKey = '';
    }

    function maStlUserFloorDimGuideGeomKey(ud) {
        if (!ud || !ud.p1Mm || !ud.p2Mm) return '';
        return [
            ud.id,
            ud.p1Mm.x,
            ud.p1Mm.y,
            ud.p1Mm.z,
            ud.p2Mm.x,
            ud.p2Mm.y,
            ud.p2Mm.z,
            maStlRulerAnchorMm.x,
            maStlRulerAnchorMm.z,
            lastMaxDim,
            desing2EnvGridSnapMm,
        ].join('|');
    }

    function maStlEnsureUserFloorDimGuideLinesMesh() {
        if (!maStlDesingV2Viewer) return null;
        if (maStlUserFloorDimGuideLinesMesh) return maStlUserFloorDimGuideLinesMesh;
        const geo = new THREE.BufferGeometry();
        const material = new THREE.LineBasicMaterial({
            color: 0x272727,
            transparent: false,
            depthTest: false,
            depthWrite: false,
        });
        const mesh = new THREE.LineSegments(geo, material);
        mesh.visible = false;
        mesh.renderOrder = 173;
        maStlDisableRaycastOnOverlay(mesh);
        maStlUserFloorLineDimHudGroup.add(mesh);
        maStlApplyUserFloorDimGuideLineColors();
        maStlUserFloorDimGuideLinesMesh = mesh;
        return mesh;
    }

    function maStlApplyUserFloorDimGuideLineColors() {
        const mesh = maStlUserFloorDimGuideLinesMesh;
        let hex = 0x202020;
        if (darkBgVisible) hex = 0xdddddd;
        if (mesh && mesh.material && mesh.material.isLineBasicMaterial) {
            mesh.material.color.setHex(hex);
            mesh.material.needsUpdate = true;
        }
        const arm = maStlUserFloorDimArrowMesh;
        if (arm && arm.material && arm.material.isMeshBasicMaterial) {
            arm.material.color.setHex(hex);
            arm.material.needsUpdate = true;
        }
    }

    /** Triángulo plano en Y=floorYB (DoubleSide Mesh). */
    function maStlUserFloorDimPushTriangle(posTri, floorYB, t1x, t1z, t2x, t2z, t3x, t3z) {
        posTri.push(t1x, floorYB, t1z, t2x, floorYB, t2z, t3x, floorYB, t3z);
    }

    /** Flechas CAD planas en cada extremo de cota (vértices hacia dentro del tramo entre extensiones). */
    function maStlUserFloorDimPushChordArrows(
        posTri,
        floorYB,
        ax,
        az,
        bx,
        bz,
        deep,
        wing
    ) {
        const mx = (ax + bx) * 0.5,
            mz = (az + bz) * 0.5;
        maStlUserFloorDimCadArrowTriangleAtEnd(posTri, floorYB, ax, az, mx, mz, deep, wing);
        maStlUserFloorDimCadArrowTriangleAtEnd(posTri, floorYB, bx, bz, mx, mz, deep, wing);
    }

    function maStlUserFloorDimCadArrowTriangleAtEnd(posTri, floorYB, ex, ez, inwardRefx, inwardRefz, deep, wing) {
        let hx = inwardRefx - ex,
            hz = inwardRefz - ez;
        const hlen = Math.hypot(hx, hz);
        if (hlen < 1e-7) return;
        hx /= hlen;
        hz /= hlen;
        /* Punta en el extremo (ex); base hacia el interior del elemento medido. */
        const tcx = ex + hx * deep;
        const tcz = ez + hz * deep;
        const px = -hz;
        const pz = hx;
        const b1x = tcx + px * wing;
        const b1z = tcz + pz * wing;
        const b2x = tcx - px * wing;
        const b2z = tcz - pz * wing;
        maStlUserFloorDimPushTriangle(posTri, floorYB, ex, ez, b1x, b1z, b2x, b2z);
    }

    function maStlEnsureUserFloorDimArrowMesh() {
        if (!maStlDesingV2Viewer) return null;
        if (maStlUserFloorDimArrowMesh) return maStlUserFloorDimArrowMesh;
        const geo = new THREE.BufferGeometry();
        const mat = new THREE.MeshBasicMaterial({
            color: 0x272727,
            transparent: false,
            depthTest: false,
            depthWrite: false,
            side: THREE.DoubleSide,
        });
        const ar = new THREE.Mesh(geo, mat);
        ar.visible = false;
        ar.renderOrder = 174;
        maStlDisableRaycastOnOverlay(ar);
        maStlUserFloorLineDimHudGroup.add(ar);
        maStlApplyUserFloorDimGuideLineColors();
        maStlUserFloorDimArrowMesh = ar;
        return ar;
    }

    function maStlRebuildUserFloorDimGuideGeometry(ud) {
        const mesh = maStlEnsureUserFloorDimGuideLinesMesh();
        const ah = maStlEnsureUserFloorDimArrowMesh();
        if (!mesh || !ah || !ud || !ud.p1Mm || !ud.p2Mm) return;
        const kNow = maStlUserFloorDimGuideGeomKey(ud);
        if (kNow === maStlUserFloorDimGuideGeomCacheKey) return;
        maStlUserFloorDimGuideGeomCacheKey = kNow;
        const floorYB = MA_STL_DESING2_WORKSPACE_FLOOR_Y_MM + MA_STL_USER_FLOOR_LINE_DIM_DRAW_Y_EPS_MM;
        const p1 = ud.p1Mm;
        const p2 = ud.p2Mm;
        const refX = maStlRulerAnchorMm.x;
        const refZ = maStlRulerAnchorMm.z;
        const dxMm = p2.x - p1.x;
        const dzMm = p2.z - p1.z;
        const lenXZ = Math.hypot(dxMm, dzMm);
        let px = 1,
            pz = 0;
        if (lenXZ > 1e-4) {
            px = -dzMm / lenXZ;
            pz = dxMm / lenXZ;
        }
        const off = THREE.MathUtils.clamp(
            lenXZ * 0.085 + Math.max(desing2EnvGridSnapMm * 0.45, 220),
            220,
            Math.min(4600, lastMaxDim * 0.52)
        );
        const fx1 = p1.x + px * off;
        const fz1 = p1.z + pz * off;
        const fx2 = p2.x + px * off;
        const fz2 = p2.z + pz * off;
        const stagger = THREE.MathUtils.clamp(
            lenXZ * 0.07 + Math.max(desing2EnvGridSnapMm * 0.5, 280),
            320,
            Math.min(5600, lastMaxDim * 0.62)
        );
        const epsZ = 2;
        /* Offset cotas paralelas Δ: ancla reglas vs P1 (posición); incluir P2 evita rozar la oblicua segmento-longitud. */
        const auxZX = Math.min(refZ, p1.z, p2.z) - stagger - off * 0.25;
        const auxXZ = Math.min(refX, p1.x, p2.x) - stagger - Math.min(off * 0.18, stagger * 0.35);

        const dxRefMm = p1.x - refX;
        const dzRefMm = p1.z - refZ;

        maStlUserFloorDimHudWorldMid.midLen = null;
        maStlUserFloorDimHudWorldMid.midDx = null;
        maStlUserFloorDimHudWorldMid.midDz = null;
        maStlUserFloorDimHudWorldMid.drawDx =
            Math.abs(dxRefMm) >= MA_STL_USER_FLOOR_LINE_DIM_MIN_PLAN_DRAW_MM;
        maStlUserFloorDimHudWorldMid.drawDz =
            Math.abs(dzRefMm) >= MA_STL_USER_FLOOR_LINE_DIM_MIN_PLAN_DRAW_MM;

        maStlUserFloorDimHudWorldMid.midLen = { x: (fx1 + fx2) * 0.5, z: (fz1 + fz2) * 0.5 };

        maStlUserFloorDimHudWorldMid.floorY = floorYB;
        maStlUserFloorDimHudWorldMid.validLen = true;
        maStlUserFloorDimHudWorldMid.chLnA = { x: p1.x, z: p1.z };
        maStlUserFloorDimHudWorldMid.chLnB = { x: p2.x, z: p2.z };
        maStlUserFloorDimHudWorldMid.chLenA = { x: fx1, z: fz1 };
        maStlUserFloorDimHudWorldMid.chLenB = { x: fx2, z: fz2 };
        maStlUserFloorDimHudWorldMid.chDxA = null;
        maStlUserFloorDimHudWorldMid.chDxB = null;
        maStlUserFloorDimHudWorldMid.chDzA = null;
        maStlUserFloorDimHudWorldMid.chDzB = null;

        maStlUserFloorDimHudWorldMid.midDx = {
            x: (refX + p1.x) * 0.5,
            z:
                maStlUserFloorDimHudWorldMid.drawDx && Math.abs(refZ - auxZX) > epsZ
                    ? auxZX
                    : refZ,
        };

        maStlUserFloorDimHudWorldMid.midDz = {
            x:
                maStlUserFloorDimHudWorldMid.drawDz && Math.abs(refX - auxXZ) > epsZ ? auxXZ : refX,
            z: (refZ + p1.z) * 0.5,
        };

        maStlUserFloorDimHudWorldMid.validDx = true;
        maStlUserFloorDimHudWorldMid.validDz = true;

        const pts = [
            new THREE.Vector3(p1.x, floorYB, p1.z),
            new THREE.Vector3(fx1, floorYB, fz1),
            new THREE.Vector3(p2.x, floorYB, p2.z),
            new THREE.Vector3(fx2, floorYB, fz2),
            new THREE.Vector3(fx1, floorYB, fz1),
            new THREE.Vector3(fx2, floorYB, fz2),
        ];
        const drawDxGeom = maStlUserFloorDimHudWorldMid.drawDx;
        if (drawDxGeom) {
            const zChord = auxZX;
            const needRefDrop = Math.abs(refZ - zChord) > epsZ;
            const needP1Drop = Math.abs(p1.z - zChord) > epsZ;
            if (needRefDrop) {
                pts.push(new THREE.Vector3(refX, floorYB, refZ));
                pts.push(new THREE.Vector3(refX, floorYB, zChord));
            }
            if (needP1Drop) {
                pts.push(new THREE.Vector3(p1.x, floorYB, p1.z));
                pts.push(new THREE.Vector3(p1.x, floorYB, zChord));
            }
            pts.push(new THREE.Vector3(refX, floorYB, zChord));
            pts.push(new THREE.Vector3(p1.x, floorYB, zChord));
            maStlUserFloorDimHudWorldMid.chDxA = { x: refX, z: zChord };
            maStlUserFloorDimHudWorldMid.chDxB = { x: p1.x, z: zChord };
        }
        const drawDzGeom = maStlUserFloorDimHudWorldMid.drawDz;
        if (drawDzGeom) {
            const xChord = auxXZ;
            const needRefJog = Math.abs(refX - xChord) > epsZ;
            const needP1Jog = Math.abs(p1.x - xChord) > epsZ;
            if (needRefJog) {
                pts.push(new THREE.Vector3(refX, floorYB, refZ));
                pts.push(new THREE.Vector3(xChord, floorYB, refZ));
            }
            if (needP1Jog) {
                pts.push(new THREE.Vector3(p1.x, floorYB, p1.z));
                pts.push(new THREE.Vector3(xChord, floorYB, p1.z));
            }
            pts.push(new THREE.Vector3(xChord, floorYB, refZ));
            pts.push(new THREE.Vector3(xChord, floorYB, p1.z));
            maStlUserFloorDimHudWorldMid.chDzA = { x: xChord, z: refZ };
            maStlUserFloorDimHudWorldMid.chDzB = { x: xChord, z: p1.z };
        }
        mesh.geometry.dispose();
        mesh.geometry = new THREE.BufferGeometry().setFromPoints(pts);
        mesh.geometry.computeBoundingSphere();
        mesh.visible = true;

        let chordLenMm = THREE.MathUtils.clamp(
            lenXZ * 0.022 + Math.max(desing2EnvGridSnapMm * 0.18, 90),
            420,
            1850
        );
        chordLenMm *= MA_STL_USER_FLOOR_LINE_DIM_ARROW_MESH_SCALE;
        const wingMm = chordLenMm * 0.55;
        const posTri = [];
        maStlUserFloorDimPushChordArrows(posTri, floorYB, fx1, fz1, fx2, fz2, chordLenMm, wingMm);
        if (drawDxGeom && Math.abs(dxRefMm) >= MA_STL_USER_FLOOR_LINE_DIM_MIN_PLAN_DRAW_MM) {
            maStlUserFloorDimPushChordArrows(posTri, floorYB, refX, auxZX, p1.x, auxZX, chordLenMm, wingMm);
        }
        if (drawDzGeom && Math.abs(dzRefMm) >= MA_STL_USER_FLOOR_LINE_DIM_MIN_PLAN_DRAW_MM) {
            maStlUserFloorDimPushChordArrows(posTri, floorYB, auxXZ, refZ, auxXZ, p1.z, chordLenMm, wingMm);
        }
        if (posTri.length > 8) {
            ah.geometry.dispose();
            ah.geometry = new THREE.BufferGeometry();
            ah.geometry.setAttribute('position', new THREE.BufferAttribute(new Float32Array(posTri), 3));
            ah.geometry.computeBoundingSphere();
            ah.visible = true;
        } else {
            ah.visible = false;
        }
        maStlApplyUserFloorDimGuideLineColors();
    }

    function maStlWorldMmToScreenPx(wx, wy, wz, outPx) {
        if (!renderer || !renderer.domElement || !activeCamera()) return false;
        const rect = renderer.domElement.getBoundingClientRect();
        const cw = Math.max(rect.width, 1);
        const cam = activeCamera();
        cam.updateProjectionMatrix();
        cam.updateMatrixWorld(true);
        _maStlUserFloorLineProjScr.set(wx, wy, wz);
        _maStlUserFloorLineProjScr.project(cam);
        outPx.x = rect.left + (_maStlUserFloorLineProjScr.x * 0.5 + 0.5) * cw;
        outPx.y = rect.top + (-_maStlUserFloorLineProjScr.y * 0.5 + 0.5) * Math.max(rect.height, 1);
        return true;
    }

    function maStlPlaceFloorDimDomInputForKind(inp, kind) {
        const h = maStlEnsureUserFloorDimDomHud();
        if (!h || !h.root || !h.canvasWrap || !inp) return;
        inp.style.pointerEvents = 'auto';
        const rWrap = h.canvasWrap.getBoundingClientRect();
        const k = kind === 'deltaX' || kind === 'deltaZ' ? kind : 'length';
        const scr =
            k === 'deltaX'
                ? maStlFloorDimHudReadoutScrPx.deltaX
                : k === 'deltaZ'
                  ? maStlFloorDimHudReadoutScrPx.deltaZ
                  : maStlFloorDimHudReadoutScrPx.length;
        const nudge = { x: 0, y: 0 };
        maStlUserFloorDimReadoutHudNudgeXY(k, nudge);
        const lxDefault = maStlUserFloorLineDimPickScreenPxValid
            ? maStlUserFloorLineDimPickScreenPx.x - rWrap.left
            : rWrap.width * 0.5;
        const lyDefault = maStlUserFloorLineDimPickScreenPxValid
            ? maStlUserFloorLineDimPickScreenPx.y - rWrap.top
            : rWrap.height * 0.5;
        const lx =
            scr && scr.valid ? scr.x - rWrap.left + nudge.x : lxDefault + nudge.x;
        const ly =
            scr && scr.valid ? scr.y - rWrap.top + nudge.y : lyDefault + nudge.y;
        inp.style.position = 'absolute';
        inp.style.left = lx + 'px';
        inp.style.top = ly + 'px';
        inp.style.transform = 'translate(-50%, -50%)';
        inp.style.visibility = '';
        inp.removeAttribute('hidden');
        inp.setAttribute('aria-hidden', 'false');
        maStlApplyUserFloorDimDomHudTheme(inp);
    }

    function maStlHideUserFloorLineDragHandle() {
        maStlEndUserFloorLineDragHandle({ pointerId: -1 });
        maStlUserFloorLineDragHandleHovered = false;
        const h = maStlEnsureUserFloorDimDomHud();
        if (!h || !h.dragHandle) return;
        h.dragHandle.hidden = true;
        h.dragHandle.setAttribute('hidden', 'hidden');
        h.dragHandle.setAttribute('aria-hidden', 'true');
        h.dragHandle.classList.remove(
            'desing2-stl-floor-line-drag-handle--hot',
            'desing2-stl-floor-line-drag-handle--dragging'
        );
    }

    function maStlPlaceUserFloorLineDragHandleAtScr(scrSlot, rWrapLeft, rWrapTop) {
        const h = maStlEnsureUserFloorDimDomHud();
        if (!h || !h.dragHandle || !h.root || !h.canvasWrap) return;
        if (!scrSlot || !scrSlot.valid) {
            maStlHideUserFloorLineDragHandle();
            return;
        }
        h.dragHandle.hidden = false;
        h.dragHandle.removeAttribute('hidden');
        h.dragHandle.setAttribute('aria-hidden', 'false');
        h.dragHandle.style.position = 'absolute';
        h.dragHandle.style.left = scrSlot.x - rWrapLeft + 'px';
        h.dragHandle.style.top = scrSlot.y - rWrapTop + 'px';
        h.dragHandle.style.transform = 'translate(-50%, -50%)';
        h.dragHandle.style.pointerEvents = 'auto';
        h.dragHandle.classList.toggle(
            'desing2-stl-floor-line-drag-handle--hot',
            !!maStlUserFloorLineDragHandleHovered || !!maStlUserFloorLineDragActive
        );
    }

    /** Reposiciona inputs + asa midpoint durante sesión edición cotas (cámara / arrastre). */
    function maStlRefreshUserFloorLineDimEditHudPositions() {
        const line = maStlUserFloorLineDimEditLineRef;
        const h = maStlEnsureUserFloorDimDomHud();
        if (!maStlUserFloorDimDomHudEditing || !line || !h || !h.root || !h.canvasWrap) return;
        const ud = line.userData && line.userData.maStlUserPlanLine;
        if (!ud || !ud.p1Mm || !ud.p2Mm) return;
        maStlRebuildUserFloorDimGuideGeometry(ud);
        maStlUserFloorDimProjectHudReadoutScreens();
        const rWrap = h.canvasWrap.getBoundingClientRect();
        h.root.hidden = false;
        h.root.setAttribute('aria-hidden', 'false');
        if (h.inputEl && !h.inputEl.hidden) maStlPlaceFloorDimDomInputForKind(h.inputEl, 'length');
        if (h.inputDxEl && !h.inputDxEl.hidden) maStlPlaceFloorDimDomInputForKind(h.inputDxEl, 'deltaX');
        if (h.inputDzEl && !h.inputDzEl.hidden) maStlPlaceFloorDimDomInputForKind(h.inputDzEl, 'deltaZ');
        const midScr = { x: 0, y: 0, valid: false };
        const midY =
            MA_STL_DESING2_WORKSPACE_FLOOR_Y_MM + MA_STL_USER_FLOOR_LINE_DIM_DRAW_Y_EPS_MM;
        const midX = (ud.p1Mm.x + ud.p2Mm.x) * 0.5;
        const midZ = (ud.p1Mm.z + ud.p2Mm.z) * 0.5;
        if (maStlWorldMmToScreenPx(midX, midY, midZ, midScr)) {
            midScr.valid = true;
            maStlPlaceUserFloorLineDragHandleAtScr(midScr, rWrap.left, rWrap.top);
        } else {
            maStlHideUserFloorLineDragHandle();
        }
        if (maStlUserFloorLineDimEditKind === 'all') {
            const dxHud = ud.p1Mm.x - maStlRulerAnchorMm.x;
            const dzHud = ud.p1Mm.z - maStlRulerAnchorMm.z;
            if (h.inputDxEl && !h.inputDxEl.hidden) {
                h.inputDxEl.value = maStlDesing2SignedDeltaMetersDisplayFromMm(dxHud);
            }
            if (h.inputDzEl && !h.inputDzEl.hidden) {
                h.inputDzEl.value = maStlDesing2SignedDeltaMetersDisplayFromMm(dzHud);
            }
        } else if (maStlUserFloorLineDimEditKind === 'deltaX' && h.inputDxEl && !h.inputDxEl.hidden) {
            h.inputDxEl.value = maStlDesing2SignedDeltaMetersDisplayFromMm(
                ud.p1Mm.x - maStlRulerAnchorMm.x
            );
        } else if (maStlUserFloorLineDimEditKind === 'deltaZ' && h.inputDzEl && !h.inputDzEl.hidden) {
            h.inputDzEl.value = maStlDesing2SignedDeltaMetersDisplayFromMm(
                ud.p1Mm.z - maStlRulerAnchorMm.z
            );
        }
        if (maStlUserFloorDimGuideLinesMesh) maStlUserFloorDimGuideLinesMesh.visible = true;
        if (maStlUserFloorDimArrowMesh) maStlUserFloorDimArrowMesh.visible = true;
    }

    function maStlWireUserFloorLineDragHandleDomOnce() {
        if (!maStlDesingV2Viewer || maStlUserFloorLineDragHandleWire) return;
        const h = maStlEnsureUserFloorDimDomHud();
        if (!h || !h.dragHandle || !viewerShell) return;
        maStlUserFloorLineDragHandleWire = true;
        const tpl = viewerShell.getAttribute('data-ma-stl-user-floor-line-drag-handle-aria');
        if (tpl) h.dragHandle.setAttribute('aria-label', tpl);
        h.dragHandle.addEventListener('pointerdown', maStlOnUserFloorLineDragHandleDown, false);
        h.dragHandle.addEventListener('pointerenter', function () {
            maStlUserFloorLineDragHandleHovered = true;
            if (maStlUserFloorDimDomHudEditing) maStlRefreshUserFloorLineDimEditHudPositions();
        });
        h.dragHandle.addEventListener('pointerleave', function () {
            if (maStlUserFloorLineDragActive) return;
            maStlUserFloorLineDragHandleHovered = false;
            if (maStlUserFloorDimDomHudEditing) maStlRefreshUserFloorLineDimEditHudPositions();
        });
    }

    function maStlRefreshUserFloorDimReadoutHudScreenBBox() {
        maStlUserFloorLineDimPickScreenBBoxValid = false;
        maStlUserFloorDimScrBoxLen.valid = false;
        maStlUserFloorDimScrBoxDx.valid = false;
        maStlUserFloorDimScrBoxDz.valid = false;
        const h = maStlEnsureUserFloorDimDomHud();
        if (!h || maStlUserFloorDimDomHudEditing) return;
        const rows = [
            { btn: h.readoutBtn, box: maStlUserFloorDimScrBoxLen },
            { btn: h.readoutDxBtn, box: maStlUserFloorDimScrBoxDx },
            { btn: h.readoutDzBtn, box: maStlUserFloorDimScrBoxDz },
        ];
        let lu = Infinity,
            tu = Infinity,
            rr = -Infinity,
            bb = -Infinity;
        let any = false;
        for (let i = 0; i < rows.length; i++) {
            const bEl = rows[i].btn,
                bxOut = rows[i].box;
            if (!bEl || bEl.hidden) continue;
            const rrDom = bEl.getBoundingClientRect();
            maStlUserFloorDimCopyDomRect(rrDom, bxOut);
            if (!bxOut.valid) continue;
            any = true;
            lu = Math.min(lu, bxOut.left);
            tu = Math.min(tu, bxOut.top);
            rr = Math.max(rr, bxOut.right);
            bb = Math.max(bb, bxOut.bottom);
        }
        if (!any || !(lu <= rr && tu <= bb)) return;
        maStlUserFloorLineDimPickScreenBBox.left = lu;
        maStlUserFloorLineDimPickScreenBBox.top = tu;
        maStlUserFloorLineDimPickScreenBBox.right = rr;
        maStlUserFloorLineDimPickScreenBBox.bottom = bb;
        maStlUserFloorLineDimPickScreenBBoxValid = true;
    }

    function maStlWireUserFloorDimReadoutDomOnce() {
        if (!maStlDesingV2Viewer || maStlUserFloorDimReadoutWire) return;
        const h = maStlEnsureUserFloorDimDomHud();
        if (
            !h ||
            !h.readoutBtn ||
            !h.readoutDxBtn ||
            !h.readoutDzBtn ||
            !viewerShell
        ) {
            return;
        }
        maStlUserFloorDimReadoutWire = true;
        function wireDbl(btn, kind) {
            btn.addEventListener(
                'dblclick',
                function (ev) {
                    ev.preventDefault();
                    ev.stopPropagation();
                    if (
                        !maStlHoveredUserFloorLine ||
                        maStlIsLineToolPlacementActive() ||
                        maStlIsRulerAnchorPickModeActive()
                    ) {
                        return;
                    }
                    maStlBeginUserFloorLineDimensionEdit(maStlHoveredUserFloorLine, kind);
                },
                false
            );
        }
        wireDbl(h.readoutBtn, 'length');
        wireDbl(h.readoutDxBtn, 'deltaX');
        wireDbl(h.readoutDzBtn, 'deltaZ');
    }

    function maStlPickUserFloorLineNearScreenMm(clientX, clientY, maxPx) {
        if (!renderer || maStlUserLinesGroup.children.length === 0) return null;
        const pickTolSq = maxPx * maxPx;
        let best = null;
        let bestD = Infinity;
        for (let i = 0; i < maStlUserLinesGroup.children.length; i++) {
            const line = maStlUserLinesGroup.children[i];
            const ud = line.userData.maStlUserPlanLine;
            if (!maStlIsUserFloorPlanLineObject(line) || !ud || !ud.p1Mm || !ud.p2Mm) continue;
            const sp1 = {};
            const sp2 = {};
            if (!maStlWorldMmToScreenPx(ud.p1Mm.x, ud.p1Mm.y, ud.p1Mm.z, sp1)) return null;
            if (!maStlWorldMmToScreenPx(ud.p2Mm.x, ud.p2Mm.y, ud.p2Mm.z, sp2)) return null;
            const dSq = maStlSqDistPointToSegment2dPx(clientX, clientY, sp1.x, sp1.y, sp2.x, sp2.y);
            if (dSq < bestD && dSq <= pickTolSq) {
                bestD = dSq;
                best = line;
            }
        }
        return best;
    }

    function maStlHideUserFloorLineDimHud(force) {
        if (maStlUserFloorDimDomHudEditing) return;
        if (!force && maStlLineToolPreviewDimActive) return;
        if (force) {
            maStlLineToolPreviewDimActive = false;
        }
        maStlInvalidateUserFloorDimGuideGeomCache();
        maStlResetFloorDimHudReadoutScreens();
        maStlUserFloorLineDimPickScreenPxValid = false;
        maStlUserFloorLineDimPickScreenBBoxValid = false;
        maStlUserFloorDimScrBoxLen.valid =
            maStlUserFloorDimScrBoxDx.valid =
            maStlUserFloorDimScrBoxDz.valid =
                false;
        maStlUserFloorDimReadoutHoveredKind = null;
        maStlUserFloorDimHudWorldMid.validLen =
            maStlUserFloorDimHudWorldMid.validDx =
            maStlUserFloorDimHudWorldMid.validDz =
                false;
        maStlUserFloorDimHudWorldMid.midLen =
            maStlUserFloorDimHudWorldMid.midDx =
            maStlUserFloorDimHudWorldMid.midDz =
                null;
        maStlUserFloorDimHudWorldMid.chLenA =
            maStlUserFloorDimHudWorldMid.chLenB =
            maStlUserFloorDimHudWorldMid.chLnA =
            maStlUserFloorDimHudWorldMid.chLnB =
            maStlUserFloorDimHudWorldMid.chDxA =
            maStlUserFloorDimHudWorldMid.chDxB =
            maStlUserFloorDimHudWorldMid.chDzA =
            maStlUserFloorDimHudWorldMid.chDzB =
                null;
        if (maStlUserFloorDimGuideLinesMesh) {
            maStlUserFloorDimGuideLinesMesh.visible = false;
        }
        if (maStlUserFloorDimArrowMesh) {
            maStlUserFloorDimArrowMesh.visible = false;
        }
        const h = maStlEnsureUserFloorDimDomHud();
        if (h && h.root) {
            h.root.hidden = true;
            h.root.setAttribute('aria-hidden', 'true');
            if (!maStlUserFloorDimDomHudEditing) {
                if (h.readoutBtn) {
                    h.readoutBtn.hidden = true;
                    h.readoutBtn.textContent = '';
                }
                if (h.readoutDxBtn) {
                    h.readoutDxBtn.hidden = true;
                    h.readoutDxBtn.textContent = '';
                }
                if (h.readoutDzBtn) {
                    h.readoutDzBtn.hidden = true;
                    h.readoutDzBtn.textContent = '';
                }
                maStlHideFloorDimEditInputs(h);
            }
        }
    }

    function maStlUpdateUserFloorLineDimHud() {
        const udd =
            maStlHoveredUserFloorLine && maStlHoveredUserFloorLine.userData
                ? maStlHoveredUserFloorLine.userData.maStlUserPlanLine
                : null;
        if (!udd || !udd.p1Mm || !udd.p2Mm) {
            maStlHideUserFloorLineDimHud();
            return;
        }
        maStlRebuildUserFloorDimGuideGeometry(udd);
        if (!maStlUserFloorDimProjectHudReadoutScreens()) {
            maStlHideUserFloorLineDimHud();
            return;
        }
        maStlUserFloorLineDimPickScreenPxValid = true;
        const hdom = maStlEnsureUserFloorDimDomHud();
        maStlWireUserFloorDimReadoutDomOnce();
        if (
            !hdom ||
            !hdom.root ||
            maStlUserFloorDimDomHudEditing ||
            !hdom.readoutBtn ||
            !hdom.readoutDxBtn ||
            !hdom.readoutDzBtn
        ) {
            return;
        }
        hdom.root.hidden = false;
        hdom.root.setAttribute('aria-hidden', 'false');
        const dxHud = udd.p1Mm.x - maStlRulerAnchorMm.x;
        const dzHud = udd.p1Mm.z - maStlRulerAnchorMm.z;
        hdom.readoutBtn.hidden = false;
        hdom.readoutBtn.removeAttribute('hidden');
        hdom.readoutBtn.textContent = maStlUserFloorLineDimensionLabelMm(udd);
        hdom.readoutDxBtn.textContent = maStlDesing2SignedDeltaMetersDisplayFromMm(dxHud);
        hdom.readoutDzBtn.textContent = maStlDesing2SignedDeltaMetersDisplayFromMm(dzHud);
        maStlPlaceAllFloorDimHudReadouts();
        maStlApplyFloorDimHudReadoutBrightClass(maStlUserFloorDimReadoutHoveredKind);
        maStlApplyUserFloorDimDomHudTheme(hdom.readoutBtn);
        maStlApplyUserFloorDimDomHudTheme(hdom.readoutDxBtn);
        maStlApplyUserFloorDimDomHudTheme(hdom.readoutDzBtn);
        maStlRefreshUserFloorDimReadoutHudScreenBBox();
    }

    function maStlClearUserFloorLineHover() {
        if (maStlHoveredUserFloorLine && maStlHoveredUserFloorLine.material) {
            maStlApplyUserFloorLineBaseMaterial(maStlHoveredUserFloorLine.material);
        }
        maStlHoveredUserFloorLine = null;
        maStlHideUserFloorLineDimHud();
    }

    function maStlClearUserFloorLineDimBlurTimer() {
        if (maStlUserFloorLineDimBlurTimerId != null) {
            window.clearTimeout(maStlUserFloorLineDimBlurTimerId);
            maStlUserFloorLineDimBlurTimerId = null;
        }
    }

    function maStlDisposeUserFloorLineDimDomVisual() {
        maStlHideUserFloorLineDragHandle();
        maStlHideFloorDimEditInputs(maStlEnsureUserFloorDimDomHud());
    }

    /** @param {*} [capt] snapshot blur: `{ detachBlur, line, inputEl }` — commit fiable tras dispose(true). */
    function maStlDisposeUserFloorLineDimEdit(skipCommit, evIsEscape, capt) {
        maStlClearUserFloorLineDimBlurTimer();
        const detachWas = maStlUserFloorLineDimEditDispose;
        let lineWas =
            capt && Object.prototype.hasOwnProperty.call(capt, 'line') ? capt.line : maStlUserFloorLineDimEditLineRef;
        let elWas =
            capt && Object.prototype.hasOwnProperty.call(capt, 'inputEl') ? capt.inputEl : maStlUserFloorLineDimEditOverlay;
        let fnDetach =
            capt && typeof capt.detachBlur === 'function' ? capt.detachBlur : detachWas;
        let snapDimKind =
            capt && Object.prototype.hasOwnProperty.call(capt, 'dimKind')
                ? capt.dimKind
                : maStlUserFloorLineDimEditKind;
        if (
            !(
                snapDimKind === 'deltaX' ||
                snapDimKind === 'deltaZ' ||
                snapDimKind === 'length' ||
                snapDimKind === 'all'
            )
        ) {
            snapDimKind = 'length';
        }
        if (!fnDetach && !lineWas && !elWas) return;
        maStlEndUserFloorLineDragHandle({ pointerId: -1 });
        maStlUserFloorLineDragSuppressBlurCommit = false;
        maStlUserFloorLineDimEditDispose = null;
        maStlUserFloorLineDimEditLineRef = null;
        maStlUserFloorLineDimEditOverlay = null;
        maStlUserFloorDimDomHudEditing = false;
        maStlUserFloorLineDimEditKind = 'length';
        if (typeof fnDetach === 'function') {
            fnDetach();
        }
        if (!skipCommit && elWas && evIsEscape !== true) {
            maStlInvalidateUserFloorDimGuideGeomCache();
            let commitOk = false;
            let logDev = false;
            if (snapDimKind === 'all' && lineWas && lineWas.userData && lineWas.userData.maStlUserPlanLine) {
                const hdomSnap = maStlEnsureUserFloorDimDomHud();
                const inpLen =
                    (capt && capt.inputLenEl) ||
                    elWas ||
                    (hdomSnap && hdomSnap.inputEl);
                const inpDx =
                    (capt && capt.inputDxEl) ||
                    (hdomSnap && hdomSnap.inputDxEl);
                const inpDz =
                    (capt && capt.inputDzEl) ||
                    (hdomSnap && hdomSnap.inputDzEl);
                const rawLen = String(inpLen && inpLen.value != null ? inpLen.value : '').trim();
                const rawDx = String(inpDx && inpDx.value != null ? inpDx.value : '').trim();
                const rawDz = String(inpDz && inpDz.value != null ? inpDz.value : '').trim();
                logDev = maStlStlViewerIsLocalDevHost() && (rawLen.length > 0 || rawDx.length > 0 || rawDz.length > 0);
                commitOk = maStlCommitUserFloorLineDimensionMulti(lineWas, rawLen, rawDx, rawDz);
            } else {
                const raw = String(elWas.value != null ? elWas.value : '').trim();
                logDev = maStlStlViewerIsLocalDevHost() && raw.length > 0;
                if (
                    snapDimKind === 'deltaX' &&
                    lineWas &&
                    lineWas.userData &&
                    lineWas.userData.maStlUserPlanLine
                ) {
                    const pv = maStlParseLengthInputValueToMm(raw);
                    commitOk =
                        pv != null && maStlResizeUserFloorLinePlanDeltaXMm(lineWas, pv);
                } else if (
                    snapDimKind === 'deltaZ' &&
                    lineWas &&
                    lineWas.userData &&
                    lineWas.userData.maStlUserPlanLine
                ) {
                    const pv = maStlParseLengthInputValueToMm(raw);
                    commitOk =
                        pv != null && maStlResizeUserFloorLinePlanDeltaZMm(lineWas, pv);
                } else if (lineWas && lineWas.userData && lineWas.userData.maStlUserPlanLine) {
                    const lenMmParsed = maStlParseLengthInputValueToMm(raw);
                    const minAllowed = maStlUserFloorSegmentMinMm();
                    const planOk = lenMmParsed != null && lenMmParsed >= minAllowed - 1e-9;
                    if (planOk) {
                        maStlResizeUserFloorLineToLengthMm(
                            lineWas,
                            maStlDesing2LengthMmRoundedEditableFromMm(lenMmParsed)
                        );
                        commitOk = true;
                    }
                }
            }
            if (commitOk) {
                if (maStlHoveredUserFloorLine === lineWas && lineWas.material) {
                    maStlApplyUserFloorLineHoverBrightMaterial(lineWas.material);
                }
                maStlUpdateUserFloorLineDimHud();
            } else if (logDev && lineWas && lineWas.userData && lineWas.userData.maStlUserPlanLine) {
                console.error('[maSTL] Cotas línea usuario: entrada no válida tras commit', {
                    snapDimKind: snapDimKind,
                });
            }
        }
        maStlDisposeUserFloorLineDimDomVisual();
        if (maStlHoveredUserFloorLine) {
            maStlUpdateUserFloorLineDimHud();
        }
    }

    function maStlBeginUserFloorLineDimensionEdit(line, editKindOpt) {
        const isAll = editKindOpt === 'all';
        maStlUserFloorLineDimEditKind = isAll
            ? 'all'
            : editKindOpt === 'deltaX' || editKindOpt === 'deltaZ'
              ? editKindOpt
              : 'length';
        const hdom = maStlEnsureUserFloorDimDomHud();
        if (
            !line ||
            !line.userData ||
            !line.userData.maStlUserPlanLine ||
            !hdom ||
            !hdom.root ||
            !hdom.canvasWrap ||
            !hdom.inputEl ||
            !viewerShell
        ) {
            maStlUserFloorLineDimEditKind = 'length';
            return;
        }
        if (isAll && (!hdom.inputDxEl || !hdom.inputDzEl)) {
            maStlUserFloorLineDimEditKind = 'length';
            editKindOpt = 'length';
        }
        const inpExisting = maStlUserFloorLineDimEditOverlay;
        if (
            inpExisting &&
            inpExisting !== hdom.inputEl &&
            inpExisting !== hdom.inputDxEl &&
            inpExisting !== hdom.inputDzEl &&
            document.body &&
            typeof document.body.contains === 'function' &&
            document.body.contains(inpExisting)
        ) {
            try {
                document.body.removeChild(inpExisting);
            } catch (_eRm) {}
        }
        maStlDisposeUserFloorLineDimEdit(true);
        maStlUserFloorLineDimEditLineRef = line;
        maStlUserFloorDimDomHudEditing = true;
        maStlWireUserFloorLineDragHandleDomOnce();
        maStlUserFloorLineDimEditKind = isAll
            ? 'all'
            : editKindOpt === 'deltaX' || editKindOpt === 'deltaZ'
              ? editKindOpt
              : 'length';
        const tplLength = viewerShell.getAttribute('data-ma-stl-user-floor-line-dim-edit-aria');
        const tplDx = viewerShell.getAttribute('data-ma-stl-user-floor-line-dim-edit-x-aria');
        const tplDz = viewerShell.getAttribute('data-ma-stl-user-floor-line-dim-edit-z-aria');
        const ud = line.userData.maStlUserPlanLine;
        const dxHud = ud.p1Mm.x - maStlRulerAnchorMm.x;
        const dzHud = ud.p1Mm.z - maStlRulerAnchorMm.z;
        hdom.root.hidden = false;
        hdom.root.setAttribute('aria-hidden', 'false');
        if (hdom.readoutBtn) hdom.readoutBtn.hidden = true;
        if (hdom.readoutDxBtn) hdom.readoutDxBtn.hidden = true;
        if (hdom.readoutDzBtn) hdom.readoutDzBtn.hidden = true;

        function wireDimInput(inp, kind, capExtra) {
            inp.autocomplete = 'off';
            inp.hidden = false;
            inp.classList.remove('d-none');
            maStlPlaceFloorDimDomInputForKind(inp, kind);
            if (inp.parentElement !== hdom.root) {
                hdom.root.appendChild(inp);
            }
            function onKey(kev) {
                if (kev.key === 'Enter' && !kev.shiftKey) {
                    kev.preventDefault();
                    maStlClearUserFloorLineDimBlurTimer();
                    const capEnter = Object.assign(
                        {
                            line: line,
                            inputEl: inp,
                            dimKind: maStlUserFloorLineDimEditKind,
                        },
                        capExtra || {}
                    );
                    maStlDisposeUserFloorLineDimEdit(false, undefined, capEnter);
                } else if (kev.key === 'Escape' || kev.code === 'Escape') {
                    kev.preventDefault();
                    kev.stopPropagation();
                    maStlClearUserFloorLineDimBlurTimer();
                    const capEsc = Object.assign(
                        {
                            line: line,
                            inputEl: inp,
                            dimKind: maStlUserFloorLineDimEditKind,
                        },
                        capExtra || {}
                    );
                    maStlDisposeUserFloorLineDimEdit(false, undefined, capEsc);
                }
            }
            inp.addEventListener('keydown', onKey);
            const onBlur = function () {
                const cap = Object.assign(
                    {
                        line: line,
                        inputEl: inp,
                        dimKind: maStlUserFloorLineDimEditKind,
                    },
                    capExtra || {}
                );
                if (!capExtra || !capExtra.allInputs) {
                    cap.detachBlur = function detachInner() {
                        inp.removeEventListener('blur', onBlur);
                        inp.removeEventListener('keydown', onKey);
                    };
                }
                maStlUserFloorLineDimBlurTimerId = window.setTimeout(function () {
                    maStlUserFloorLineDimBlurTimerId = null;
                    if (maStlUserFloorLineDragSuppressBlurCommit) {
                        maStlUserFloorLineDragSuppressBlurCommit = false;
                        return;
                    }
                    if (maStlUserFloorLineDragActive) return;
                    if (maStlFloorDimEditInputsActive(cap.allInputs || [inp])) return;
                    maStlDisposeUserFloorLineDimEdit(false, undefined, cap);
                }, 0);
            };
            inp.addEventListener('blur', onBlur);
            return { onKey: onKey, onBlur: onBlur };
        }

        if (maStlUserFloorLineDimEditKind === 'all') {
            const inpLen = hdom.inputEl;
            const inpDx = hdom.inputDxEl;
            const inpDz = hdom.inputDzEl;
            maStlUserFloorLineDimEditOverlay = inpLen;
            if (tplLength) inpLen.setAttribute('aria-label', tplLength);
            if (tplDx) inpDx.setAttribute('aria-label', tplDx);
            if (tplDz) inpDz.setAttribute('aria-label', tplDz);
            inpLen.value = maStlUserFloorLineDimensionLabelMm(ud);
            inpDx.value = maStlDesing2SignedDeltaMetersDisplayFromMm(dxHud);
            inpDz.value = maStlDesing2SignedDeltaMetersDisplayFromMm(dzHud);
            const allInputs = [inpLen, inpDx, inpDz];
            const capExtra = {
                inputLenEl: inpLen,
                inputDxEl: inpDx,
                inputDzEl: inpDz,
                allInputs: allInputs,
            };
            const wLen = wireDimInput(inpLen, 'length', capExtra);
            const wDx = wireDimInput(inpDx, 'deltaX', capExtra);
            const wDz = wireDimInput(inpDz, 'deltaZ', capExtra);
            maStlUserFloorLineDimEditDispose = function () {
                inpLen.removeEventListener('blur', wLen.onBlur);
                inpLen.removeEventListener('keydown', wLen.onKey);
                inpDx.removeEventListener('blur', wDx.onBlur);
                inpDx.removeEventListener('keydown', wDx.onKey);
                inpDz.removeEventListener('blur', wDz.onBlur);
                inpDz.removeEventListener('keydown', wDz.onKey);
                maStlClearUserFloorLineDimBlurTimer();
            };
            inpLen.focus({ preventScroll: true });
            try {
                inpLen.select();
            } catch (_eSel) {}
            maStlRefreshUserFloorLineDimEditHudPositions();
            return;
        }

        maStlHideFloorDimEditInputs(hdom);
        const kindSingle = maStlUserFloorLineDimEditKind;
        const inp =
            kindSingle === 'deltaX' && hdom.inputDxEl
                ? hdom.inputDxEl
                : kindSingle === 'deltaZ' && hdom.inputDzEl
                  ? hdom.inputDzEl
                  : hdom.inputEl;
        if (!inp) {
            maStlUserFloorLineDimEditKind = 'length';
            return;
        }
        maStlUserFloorLineDimEditOverlay = inp;
        if (kindSingle === 'deltaX' && tplDx) inp.setAttribute('aria-label', tplDx);
        else if (kindSingle === 'deltaZ' && tplDz) inp.setAttribute('aria-label', tplDz);
        else if (tplLength) inp.setAttribute('aria-label', tplLength);
        if (kindSingle === 'deltaX') {
            inp.value = maStlDesing2SignedDeltaMetersDisplayFromMm(dxHud);
        } else if (kindSingle === 'deltaZ') {
            inp.value = maStlDesing2SignedDeltaMetersDisplayFromMm(dzHud);
        } else inp.value = maStlUserFloorLineDimensionLabelMm(ud);
        const wired = wireDimInput(inp, kindSingle, null);
        maStlUserFloorLineDimEditDispose = function () {
            inp.removeEventListener('blur', wired.onBlur);
            inp.removeEventListener('keydown', wired.onKey);
            maStlClearUserFloorLineDimBlurTimer();
        };
        inp.focus({ preventScroll: true });
        try {
            inp.select();
        } catch (_eSel) {}
        maStlRefreshUserFloorLineDimEditHudPositions();
    }

    function maStlIsUserFloorLineDimEditOverlayActive() {
        return !!(maStlUserFloorDimDomHudEditing && maStlUserFloorLineDimEditOverlay);
    }

    function onCanvasPointerMoveUserFloorLineHover(ev) {
        if (!maStlDesingV2Viewer || !renderer || maStlIsLineToolPlacementActive()) {
            maStlClearUserFloorLineHover();
            return;
        }
        if (maStlIsRulerAnchorPickModeActive()) {
            maStlClearUserFloorLineHover();
            return;
        }
        if (maStlIsUserFloorLineDimEditOverlayActive()) return;
        let pick = maStlPickUserFloorLineNearScreenMm(ev.clientX, ev.clientY, MA_STL_USER_FLOOR_LINE_SCREEN_PICK_PX);
        if (
            !pick &&
            maStlHoveredUserFloorLine &&
            maStlUserFloorLineDimPickScreenBBoxValid &&
            maStlUserFloorDimLabelScreenHitIncludesPx(
                ev.clientX,
                ev.clientY,
                MA_STL_USER_FLOOR_LINE_DIM_LABEL_HIT_PADDING_PX + 12
            )
        ) {
            pick = maStlHoveredUserFloorLine;
        }
        if (pick !== maStlHoveredUserFloorLine) {
            maStlClearUserFloorLineHover();
            maStlHoveredUserFloorLine = pick;
            if (maStlHoveredUserFloorLine && maStlHoveredUserFloorLine.material) {
                maStlApplyUserFloorLineHoverBrightMaterial(maStlHoveredUserFloorLine.material);
            }
        }
        if (maStlHoveredUserFloorLine) {
            maStlUpdateUserFloorLineDimHud();
            const nk = maStlUserFloorDimPickReadoutKindAtPx(ev.clientX, ev.clientY, 6);
            if (nk !== maStlUserFloorDimReadoutHoveredKind) {
                maStlUserFloorDimReadoutHoveredKind = nk;
                maStlApplyFloorDimHudReadoutBrightClass(maStlUserFloorDimReadoutHoveredKind);
                maStlRefreshUserFloorDimReadoutHudScreenBBox();
            }
        } else {
            maStlHideUserFloorLineDimHud();
        }
    }

    function onCanvasPointerLeaveUserFloorLineHover() {
        if (!maStlDesingV2Viewer) return;
        if (maStlIsUserFloorLineDimEditOverlayActive()) return;
        maStlClearUserFloorLineHover();
    }

    function onCanvasDblClickUserFloorLineDimension(ev) {
        if (!maStlDesingV2Viewer || maStlIsLineToolPlacementActive()) return;
        if (maStlIsRulerAnchorPickModeActive()) return;
        if (maStlIsUserFloorLineDimEditOverlayActive()) return;
        const canvas = renderer && renderer.domElement;
        if (!canvas || ev.currentTarget !== canvas) return;
        if (ev.button !== 0) return;
        const rawTarget = ev.target;
        if (rawTarget && typeof rawTarget.closest === 'function') {
            if (rawTarget.closest('button, input, select, textarea, [role="button"], label')) return;
        }
        let ln = maStlPickUserFloorLineNearScreenMm(ev.clientX, ev.clientY, MA_STL_USER_FLOOR_LINE_SCREEN_PICK_PX + 8);
        if (
            !ln &&
            maStlHoveredUserFloorLine &&
            maStlUserFloorLineDimPickScreenBBoxValid &&
            maStlUserFloorDimLabelScreenHitIncludesPx(
                ev.clientX,
                ev.clientY,
                MA_STL_USER_FLOOR_LINE_DIM_LABEL_HIT_PADDING_PX + 18
            )
        ) {
            ln = maStlHoveredUserFloorLine;
        }
        if (!ln) return;
        ev.preventDefault();
        ev.stopPropagation();
        const prevHover = maStlHoveredUserFloorLine;
        if (prevHover && prevHover !== ln && prevHover.material) {
            maStlApplyUserFloorLineBaseMaterial(prevHover.material);
        }
        maStlHoveredUserFloorLine = ln;
        if (ln.material) {
            maStlApplyUserFloorLineHoverBrightMaterial(ln.material);
        }
        maStlUpdateUserFloorLineDimHud();
        const kindHud = maStlUserFloorDimPickReadoutKindAtPx(ev.clientX, ev.clientY, 10);
        maStlBeginUserFloorLineDimensionEdit(ln, kindHud || 'all');
    }

    function onCanvasPointerMoveLineToolSync(ev) {
        if (!maStlIsLineToolPlacementActive()) return;
        maStlLineToolLastPointerClientXY.set(ev.clientX, ev.clientY);
        maStlUpdateLineToolFloorHover(ev.clientX, ev.clientY);
        const p = maStlResolveLineToolFloorPointMm(ev.clientX, ev.clientY);
        if (p && maStlLineToolState === 'picking2') {
            maStlLineToolMaybeUpdateHoverDirFromP2Candidate(p);
        }
        if (maStlLineToolState === 'picking2') {
            maStlLineToolRefreshPicking2RubberBand();
        }
    }

    /** Bloquea órbita en botón izquierdo mientras hay picking; la colocación es por {@link onCanvasClickLineTool}. */
    function onCanvasPointerDownLineTool(ev) {
        if (!maStlIsLineToolPlacementActive() || !maStlDesingV2Viewer) return;
        if (ev.button !== 0) return;
        const canvas = renderer.domElement;
        if (ev.currentTarget !== canvas) return;
        const rawTarget = ev.target;
        if (rawTarget && typeof rawTarget.closest === 'function') {
            if (rawTarget.closest('button, input, select, textarea, [role="button"], label')) return;
        }
        ev.preventDefault();
        ev.stopPropagation();
        ev.stopImmediatePropagation();
    }

    /**
     * Clic derecho corto en canvas (sin arrastre pan): refactor global de segmentos colineales.
     * Durante herramienta línea el RMB sigue siendo pan; sólo dispara si el gesto no movió órbita.
     */
    function onCanvasPointerDownUserFloorLineRefactorRmb(ev) {
        if (!maStlDesingV2Viewer || ev.button !== 2) return;
        const canvas = renderer.domElement;
        if (ev.currentTarget !== canvas) return;
        const rawTarget = ev.target;
        if (rawTarget && typeof rawTarget.closest === 'function') {
            if (rawTarget.closest('button, input, select, textarea, [role="button"], label')) return;
        }
        if (maStlIsRulerAnchorPickModeActive()) return;
        if (maStlIsUserFloorLineDimEditOverlayActive()) return;
        _maStlUserFloorLineRefactorRmbGesture = {
            clientX: ev.clientX,
            clientY: ev.clientY,
        };
        if (controls) {
            _maStlUserFloorLineRefactorRmbOrbitBaseline.copy(controls.target);
        }
    }

    function onWindowPointerUpUserFloorLineRefactorRmb(ev) {
        if (ev.button !== 2 || !_maStlUserFloorLineRefactorRmbGesture) return;
        const track = _maStlUserFloorLineRefactorRmbGesture;
        _maStlUserFloorLineRefactorRmbGesture = null;
        if (!maStlDesingV2Viewer) return;
        const dx = ev.clientX - track.clientX;
        const dy = ev.clientY - track.clientY;
        const maxPx = MA_STL_USER_FLOOR_LINE_REFACTOR_RMB_CLICK_MAX_PX;
        if (dx * dx + dy * dy > maxPx * maxPx) return;
        if (controls) {
            const epsMm = MA_STL_DESING2_PICK_ORBIT_PAN_DETECTION_EPS_MM;
            if (
                controls.target.distanceToSquared(_maStlUserFloorLineRefactorRmbOrbitBaseline) >
                epsMm * epsMm
            ) {
                return;
            }
        }
        maStlRefactorUserFloorLinesMergeCollinear();
    }

    function onWindowPointerCancelUserFloorLineRefactorRmb(ev) {
        if (ev.button !== 2) return;
        _maStlUserFloorLineRefactorRmbGesture = null;
    }

    /**
     * Dos clics simples: P1 y P2. Se usa `click` (no `pointerdown`) para no colocar si el usuario arrastró el botón
     * izquierdo; el {@link onCanvasPointerDownLineTool} en captura evita que OrbitControls inicie rotación en el down.
     */
    function onCanvasClickLineTool(ev) {
        if (!maStlIsLineToolPlacementActive() || !maStlDesingV2Viewer) return;
        if (ev.button !== 0) return;
        const canvas = renderer.domElement;
        if (ev.currentTarget !== canvas) return;
        const rawTarget = ev.target;
        if (rawTarget && typeof rawTarget.closest === 'function') {
            if (rawTarget.closest('button, input, select, textarea, [role="button"], label')) return;
        }
        ev.preventDefault();
        ev.stopPropagation();
        ev.stopImmediatePropagation();
        const p = maStlResolveLineToolFloorPointMm(ev.clientX, ev.clientY);
        if (!p) {
            if (maStlLineToolState === 'picking1') {
                maStlStopLineToolModesToolbar(false);
            }
            return;
        }
        if (maStlLineToolState === 'picking1') {
            maStlLineToolPoint1Mm.set(p.x, p.y, p.z);
            maStlWeldUserFloorPlanPointToExistingEndpointsMm(
                maStlLineToolPoint1Mm,
                maStlUserFloorLineMergeEndpointEpsMm()
            );
            maStlLineToolState = 'picking2';
            maStlResetLineToolDistanceTypingState();
            maStlLineToolLastPointerClientXY.set(ev.clientX, ev.clientY);
            maStlLineToolMaybeUpdateHoverDirFromP2Candidate(p);
            maStlSyncLineToolHud();
            maStlLineToolRefreshPicking2RubberBand();
            queueMicrotask(function () {
                if (maStlLineToolState !== 'picking2' || !maStlLineToolHudDistanceInput) return;
                try {
                    maStlLineToolHudDistanceInput.focus({ preventScroll: true });
                } catch (_e) {
                    maStlLineToolHudDistanceInput.focus();
                }
            });
            return;
        }
        if (maStlLineToolState === 'picking2') {
            const y = MA_STL_DESING2_WORKSPACE_FLOOR_Y_MM;
            let end;
            if (p.maStlLineVertexSnap) {
                end = { x: p.x, y: p.y, z: p.z };
            } else {
                const dx = p.x - maStlLineToolPoint1Mm.x;
                const dz = p.z - maStlLineToolPoint1Mm.z;
                const r = maStlLineToolFloorDirLenFromDeltaMm(dx, dz, maStlLineToolOrtho15Enabled);
                end = r
                    ? {
                          x: maStlLineToolPoint1Mm.x + r.x * r.len,
                          y: y,
                          z: maStlLineToolPoint1Mm.z + r.z * r.len,
                      }
                    : { x: p.x, y: p.y, z: p.z };
            }
            maStlCommitUserPlanLineSegmentMm(maStlLineToolPoint1Mm, end);
            maStlLineToolResetForNextSegment();
        }
    }

    /**
     * Hover rejilla (pointermove): highlight + HUD únicamente — sin cámara, target, controls ni reglas.
     * @returns {boolean} true si hay snap en planta (idle o connected)
     */
    function maStlUpdateGridIntersectionPickHover(clientX, clientY) {
        if (maStlRulerAnchorPickMode !== 'grid' || !maStlDesingV2Viewer || !renderer) {
            maStlClearGridIntersectionPickHighlight();
            maStlClearLineToolVertexSnapHighlight();
            return false;
        }
        const canvas = renderer.domElement;
        const cam = activeCamera();
        const floorHit = maStlClientRayToWorkspaceFloor(
            clientX,
            clientY,
            canvas,
            cam,
            orbitPivotNdc,
            orbitPivotRaycaster,
            _maStlInsertionFloorProbe
        );
        if (!floorHit) {
            maStlClearGridIntersectionPickHighlight();
            maStlClearLineToolVertexSnapHighlight();
            return false;
        }
        const lineSnap = maStlFindFloorLineVertexSnapAtPointer(
            clientX,
            clientY,
            MA_STL_LINE_TOOL_GRID_PICK_SCREEN_PX_BOOST
        );
        if (lineSnap) {
            maStlSetLineToolVertexSnapHighlight(lineSnap, true);
            maStlClearGridIntersectionPickHighlight();
            return true;
        }
        maStlClearLineToolVertexSnapHighlight();
        const gridSnap = maStlSnapFloorToGridIntersection(
            { x: _maStlInsertionFloorProbe.x, z: _maStlInsertionFloorProbe.z },
            { clientX: clientX, clientY: clientY, camera: cam, canvas: canvas, maxDim: lastMaxDim },
            desing2EnvGridSnapMm
        );
        maStlSetGridIntersectionPickHighlight(gridSnap.active ? 'connected' : 'idle', gridSnap);
        return true;
    }

    function onCanvasPointerMoveRulerAnchorPick(ev) {
        if (!maStlIsRulerAnchorPickModeActive()) return;
        if (maStlRulerAnchorPickMode === 'grid') {
            maStlUpdateGridIntersectionPickHover(ev.clientX, ev.clientY);
            return;
        }
        maStlUpdateObjectInsertionPickHover(ev.clientX, ev.clientY);
    }

    function onCanvasPointerLeaveInsertionPick() {
        onCanvasPointerLeaveUserFloorLineHover();
        if (maStlIsLineToolPlacementActive()) {
            maStlClearGridIntersectionPickHighlight();
            maStlClearLineToolVertexSnapHighlight();
            maStlSyncLineToolHud();
            if (maStlLineToolState === 'picking2') {
                maStlLineToolRefreshPicking2RubberBand();
            }
        }
        if (!maStlIsRulerAnchorPickModeActive()) return;
        maStlClearStlPickHoverHighlight();
        maStlClearInsertionPickHighlight();
        maStlClearGridIntersectionPickHighlight();
        maStlClearLineToolVertexSnapHighlight();
    }

    /**
     * Reconstruye overlay según tamaño modelo; Desing_2: reglas hasta ~25 m físicos (unidades escena = mm).
     * @param {number} maxDimLocal mismo criterio que `refitCamerasToObject`.
     */
    function rebuildMaStlUcsOverlayDecor(maxDimLocal) {
        maStlStripOverlayMeshes(maStlUcsAxesGroup);
        maStlStripOverlayMeshes(maStlXyzAxesGroup);
        maStlStripOverlayMeshes(maStlRulersGroup);
        maStlOverlayLineMat = null;
        const axisLen = maStlWorldAxesLength(maxDimLocal, maStlRulersGate);
        const rulerExtent = maStlRulersGate
            ? maStlDesing2RulerExtentMm(
                  maxDimLocal,
                  desing2EnvRulerExtentCapMm,
                  desing2EnvGridMajorMm()
              )
            : maStlRulerExtentFromMaxDimMm(maxDimLocal);
        maStlOverlayLineMat =
            maStlRulersGate && maStlDesingV2Viewer
                ? maStlCreateDesing2RulerLineMaterial(darkBgVisible)
                : maStlMakeOverlayLineMat();
        if (maStlDesingV2Viewer) {
            maStlXyzAxesGroup.add(maStlBuildXyzAxesFromAxisLen(axisLen, MA_STL_XYZ_AXES_COLOR));
        } else {
            maStlUcsAxesGroup.add(maStlBuildUcsFromAxisLen(axisLen, maStlOverlayLineMat, false));
        }
        if (maStlRulersGate) {
            /** Geometría de reglas en **metros** (÷1000 vs mm escena); grupo escalado ×1000 → alineado con malla/rejilla. */
            const s = MA_STL_SCENE_MM_PER_PHYSICAL_METER;
            const rulerInner = maStlBuildPlanRulers(
                axisLen,
                rulerExtent / s,
                desing2EnvGridSnapMm / s,
                desing2EnvGridMajorMm() / s,
                maStlOverlayLineMat,
                maStlRulerLabelMetersFromWorldM,
                s,
                MA_STL_DESING2_WORKSPACE_FLOOR_Y_MM,
                maStlRulerAnchorMm.x,
                maStlRulerAnchorMm.z
            );
            const rulerSceneWrap = new THREE.Group();
            rulerSceneWrap.scale.setScalar(s);
            rulerSceneWrap.add(rulerInner);
            maStlRulersSceneWrap = rulerSceneWrap;
            maStlRulersGroup.add(rulerSceneWrap);
        } else {
            maStlRulersSceneWrap = null;
        }
        maStlRebuildRulerAnchorMarker();
        syncMaStlUcsOverlayVisibility();
        maStlSyncDesing2RulerLabelSpritesToTheme();
        if (maStlDesingV2Viewer && maStlUserLinesGroup) {
            const um = maStlEnsureUserFloorLineMat();
            for (let uli = 0; uli < maStlUserLinesGroup.children.length; uli++) {
                const o = maStlUserLinesGroup.children[uli];
                if (!maStlIsUserFloorPlanLineObject(o)) continue;
                if (!o.material || !o.material.isLineMaterial) {
                    o.material = um.clone();
                }
                maStlApplyUserFloorLineBaseMaterial(o.material);
                if (o === maStlHoveredUserFloorLine) maStlApplyUserFloorLineHoverBrightMaterial(o.material);
            }
        }
        if (maStlDesingV2Viewer && maStlLineToolRubberBandLine) {
            maStlLineToolRubberBandLine.material = maStlEnsureUserFloorLineMat();
        }
    }

    function maStlSyncDesing2RulerLabelSpritesToTheme() {
        if (!maStlDesingV2Viewer) return;
        const fill = maStlDesing2RulerLabelFillForTheme(darkBgVisible);
        if (maStlRulersGate && maStlRulersSceneWrap) {
            maStlRulersSceneWrap.traverse(function (obj) {
                if (!obj.isSprite) return;
                const st = obj.userData && obj.userData.maStlThinTextSpriteState;
                if (!st) return;
                st.thinOpts.fillColor = fill;
                maStlRedrawThinTextSprite(obj);
            });
        }
        maStlApplyUserFloorDimGuideLineColors();
        if (maStlEnsureUserFloorDimDomHud()) {
            const hHud = maStlUserFloorDimDomHud;
            if (hHud && hHud.readoutBtn) maStlApplyUserFloorDimDomHudTheme(hHud.readoutBtn);
            if (hHud && hHud.inputEl) maStlApplyUserFloorDimDomHudTheme(hHud.inputEl);
        }
        if (maStlHoveredUserFloorLine && !maStlUserFloorDimDomHudEditing) maStlUpdateUserFloorLineDimHud();
    }

    function maStlSyncDesing2RulerLineMaterialToTheme() {
        if (!maStlDesingV2Viewer) return;
        if (maStlRulersGate && maStlOverlayLineMat) {
            maStlApplyDesing2RulerLineMaterialTheme(maStlOverlayLineMat, darkBgVisible);
        }
        maStlSyncDesing2RulerLabelSpritesToTheme();
    }

    scene.add(maStlUcsAxesGroup);
    scene.add(maStlXyzAxesGroup);
    scene.add(maStlRulersGroup);
    if (maStlDesingV2Viewer) {
        scene.add(maStlRulerAnchorMarkerGroup);
        scene.add(maStlInsertionPickHighlightGroup);
        scene.add(maStlGridIntersectionPickHighlightGroup);
        scene.add(maStlLineToolVertexSnapHighlightGroup);
        scene.add(maStlUserLinesGroup);
        scene.add(maStlUserFloorLineDimHudGroup);
        maStlDisableRaycastOnOverlay(maStlUserLinesGroup);
    }
    rebuildMaStlUcsOverlayDecor(lastMaxDim);

    if (maStlDesingV2Viewer && maStlRulerAnchorPickToggleBtn) {
        maStlRulerAnchorPickToggleBtn.addEventListener('click', function () {
            if (maStlRulerAnchorPickMode === 'grid') {
                maStlStopRulerAnchorPickModesToolbar();
                return;
            }
            maStlStopLineToolModesToolbar(false);
            maStlTeardownPickHighlightsOnly();
            maStlRulerAnchorPickMode = 'grid';
            maStlLockOrbitForRulerAnchorPick();
            maStlEnsureGridIntersectionPickHighlightMeshes();
            const modeToast =
                maStlRulerAnchorPickToggleBtn.getAttribute(
                    'data-ma-stl-ruler-anchor-pick-mode-toast'
                ) || '';
            if (modeToast) maStlDesing2ShowSaveViewToast(modeToast);
            syncMaStlRulerPickToolbarUi();
            syncMaStlRulerAnchorPickCursor();
        });
    }
    if (maStlDesingV2Viewer && maStlRulerAnchorObjectPickToggleBtn) {
        maStlRulerAnchorObjectPickToggleBtn.addEventListener('click', function () {
            if (maStlRulerAnchorPickMode === 'object') {
                maStlStopRulerAnchorPickModesToolbar();
                return;
            }
            maStlStopLineToolModesToolbar(false);
            maStlTeardownPickHighlightsOnly();
            maStlRulerAnchorPickMode = 'object';
            maStlLockOrbitForRulerAnchorPick();
            const objToast =
                maStlRulerAnchorObjectPickToggleBtn.getAttribute(
                    'data-ma-stl-ruler-anchor-object-mode-toast'
                ) || '';
            if (objToast) maStlDesing2ShowSaveViewToast(objToast);
            syncMaStlRulerPickToolbarUi();
            syncMaStlRulerAnchorPickCursor();
        });
    }
    if (maStlDesingV2Viewer && maStlLineToolToggleBtn) {
        maStlLineToolToggleBtn.addEventListener(
            'keydown',
            function (ev) {
                if (!maStlIsLineToolPlacementActive()) return;
                /* Enter/Espacio en botón con foco: sin esto Intro “hace clic” y sale del modo sin confirmar segmento. */
                if (ev.key === 'Enter' || ev.code === 'Enter' || ev.key === 'NumpadEnter') {
                    ev.preventDefault();
                    ev.stopPropagation();
                    if (maStlLineToolState === 'picking2') {
                        maStlLineToolTryTypedCommitDistanceOrbitDefer();
                    }
                    return;
                }
                if (ev.key === ' ' || ev.code === 'Space') {
                    ev.preventDefault();
                    ev.stopPropagation();
                }
            },
            true
        );
        maStlLineToolToggleBtn.addEventListener('click', function (ev) {
            ev.stopPropagation();
            if (maStlIsLineToolPlacementActive()) {
                maStlStopLineToolModesToolbar(false);
                return;
            }
            maStlStopRulerAnchorPickModesToolbar();
            maStlTeardownPickHighlightsOnly();
            maStlResetLineToolPickingBaselineState();
            maStlLineToolState = 'picking1';
            maStlLockOrbitForRulerAnchorPick();
            maStlEnsureGridIntersectionPickHighlightMeshes();
            maStlSyncLineToolToggleBtnUi();
            maStlLineToolPickCursorSync();
            maStlSyncLineToolHud();
            if (document.activeElement === maStlLineToolToggleBtn) {
                maStlLineToolToggleBtn.blur();
            }
        });
    }
    if (maStlDesingV2Viewer && maStlLineToolOrtho15ToggleBtn) {
        maStlLineToolOrtho15ToggleBtn.addEventListener('click', function (ev) {
            ev.stopPropagation();
            maStlToggleLineToolOrtho15FromUi();
        });
    }
    maStlSyncLineToolOrtho15ToggleUi();
    (function wireDesing2TopToolbarExclusiveLineTool() {
        const tb = document.getElementById('desing2-stl-hover-top-toolbar');
        if (!maStlDesingV2Viewer || !tb) return;
        tb.addEventListener('click', function (ev) {
            const btn = ev.target && ev.target.closest && ev.target.closest('.ma-stl-viewer-toolbar-btn');
            if (!btn || btn.id === 'ma-stl-tool-line' || btn.id === 'ma-stl-tool-ortho-15') return;
            if (maStlIsLineToolPlacementActive()) {
                maStlStopLineToolModesToolbar(false);
            }
        });
    })();

    if (maStlUcsRulersToggleBtn && maStlRulersGate) {
        maStlUcsRulersToggleBtn.addEventListener('click', function () {
            maStlUcsRulersManualOn = !maStlUcsRulersManualOn;
            syncMaStlUcsOverlayVisibility();
            syncMaStlUcsRulersToggleBtnUi();
        });
    }
    if (maStlXyzAxesToggleBtn && maStlXyzAxesGate) {
        maStlXyzAxesToggleBtn.addEventListener('click', function () {
            maStlXyzAxesManualOn = !maStlXyzAxesManualOn;
            syncMaStlUcsOverlayVisibility();
            syncMaStlXyzAxesToggleBtnUi();
        });
    }
    syncMaStlUcsRulersToggleBtnUi();
    syncMaStlXyzAxesToggleBtnUi();

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
    /** Por encima de InfiniteGridHelper (-10): la rejilla depthTest:false tapaba la sombra. */
    shadowGroundPlane.renderOrder = -5;
    scene.add(shadowGroundPlane);
    (function initShadowGroundExtent() {
        const span = Math.max(lastMaxDim * 140, 2500);
        shadowGroundPlane.scale.set(span, span, 1);
    })();
    let groundShadowVisible = false;

    function maStlSyncShadowGroundMaterialVisual() {
        shadowGroundMaterial.opacity = darkBgVisible ? 0.68 : 0.42;
        shadowGroundMaterial.needsUpdate = true;
    }

    let gridVisible = maStlRulersGate;
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
    /** Suelo: perspectiva Desing_2 como Design-3d; maestro sigue en orto con `uFwidthFloor`. */
    infiniteGrid.onBeforeRender = function (renderer, _scene, camera) {
        if (!infiniteGrid.visible) return;
        const mat = infiniteGrid.material;
        if (!mat || !mat.uniforms || !mat.uniforms.uFwidthFloor) return;
        if (maStlRulersGate) {
            const tgt = maStlDesingV2Viewer ? maStlRulerAnchorMm : controls ? controls.target : new THREE.Vector3();
            maStlSyncGridPlaneY(infiniteGrid, true, lastMaxDim);
            if (camera && camera.isPerspectiveCamera) {
                maStlSyncDesing2GridDistancePerspective(infiniteGrid, frustumHalfY, lastAspect, camera, tgt);
            }
            maStlSyncDesing2ScreenSpaceOverlay(
                mat,
                camera,
                renderer,
                desing2OrthoMinZoom,
                tgt,
                desing2EnvGridSnapMm,
                desing2EnvGridMajorMm()
            );
            return;
        }
        if (camera && camera.isOrthographicCamera) {
            const wpp = maStlOrthoWorldMmPerPixel(camera, renderer);
            const cell = Math.min(mat.uniforms.uSize1.value, mat.uniforms.uSize2.value);
            const raw = (0.06 * wpp) / Math.max(cell, 1e-9);
            mat.uniforms.uFwidthFloor.value = THREE.MathUtils.clamp(raw, 1e-10, 2e-4);
        } else {
            mat.uniforms.uFwidthFloor.value = 1e-5;
        }
    };

    function maStlRefreshDesing2OrthoMinZoom() {
        if (!maStlRulersGate) return;
        desing2OrthoMinZoom = maStlDesing2MinZoomFromHalfY(frustumHalfY);
        if (controls && activeCamera().isOrthographicCamera) {
            controls.minZoom = MA_STL_DESING2_MIN_ZOOM_FLOOR;
        }
    }

    /**
     * Host layout size in CSS px (bounding rect fixes stale clientWidth/Height right after fullscreen / nested flex).
     * updateStyle=false en setSize → el tamaño visual lo marca CSS (#ma-stl-viewer-gl-host canvas 100%).
     */
    function readHostSizeCssPx(fallbackW, fallbackH) {
        const rect = canvasHost.getBoundingClientRect();
        let nw = Math.round(rect.width);
        let nh = Math.round(rect.height);
        if (nw < 2 || nh < 2) {
            nw = Math.max(canvasHost.clientWidth, 0);
            nh = Math.max(canvasHost.clientHeight, 0);
        }
        if (nw < 2) nw = fallbackW;
        if (nh < 2) nh = fallbackH;
        return { nw: Math.max(nw, 200), nh: Math.max(nh, 200) };
    }
    const initialSz = readHostSizeCssPx(400, 380);
    const w0 = initialSz.nw;
    const h0 = initialSz.nh;
    lastAspect = w0 / Math.max(h0, 1);

    function makeOrthoCamera() {
        const aspect = lastAspect;
        const hy = frustumHalfY;
        return new THREE.OrthographicCamera(-hy * aspect, hy * aspect, hy, -hy, 0.01, 500000);
    }

    /** Desing_2: perspectiva como Design-3d (rejilla al alejar). Maestro: orto. Cubo de vistas sigue con la misma API. */
    function makeStlCamera() {
        if (maStlRulersGate) {
            return new THREE.PerspectiveCamera(
                MA_STL_DESING2_PERSP_FOV,
                lastAspect,
                MA_STL_DESING2_PERSP_NEAR,
                MA_STL_DESING2_PERSP_FAR
            );
        }
        return makeOrthoCamera();
    }

    const cameraOrtho = makeStlCamera();
    const cameraIso = makeStlCamera();

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
        if (cam.isPerspectiveCamera) {
            cam.aspect = lastAspect;
            cam.updateProjectionMatrix();
            return;
        }
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

    function maStlRebuildGridIntersectionPickHighlightMeshesForSnapChange() {
        if (!maStlDesingV2Viewer || !maStlGridIntersectionPickHighlightGroup) return;
        maStlStripOverlayMeshes(maStlGridIntersectionPickHighlightGroup);
        maStlGridIntersectionPickMeshes.idle = null;
        maStlGridIntersectionPickMeshes.connected = null;
        maStlGridIntersectionNearActive = false;
        maStlSyncRulerAnchorCoordsHud();
    }

    /** Lectura `#ma-stl-entorno-*` → rejilla LOD base, snap pick y alcance cotas — sin persistir hasta «Guardar vista». */
    function applyDesing2EntornoLive() {
        if (!maStlDesingV2Viewer) return;
        if (maStlEntornoGridSnapSelect instanceof HTMLSelectElement) {
            desing2EnvGridSnapMm = maStlClampAllowedDesing2GridSnapMm(maStlEntornoGridSnapSelect.value);
        }
        if (maStlEntornoRulerExtentSelect instanceof HTMLSelectElement) {
            desing2EnvRulerExtentCapMm = maStlDesing2RulerExtentCapFromMeters(
                maStlEntornoRulerExtentSelect.value
            );
        }
        const od = desing2WorkspaceOverlayDim();
        frustumHalfY = maStlFrustumHalfYFromMaxDim(
            od,
            maStlRulersGate,
            maStlRulersGate ? desing2EnvGridMajorMm() : undefined
        );
        maStlRefreshDesing2OrthoMinZoom();
        maStlSyncInfiniteGridWorkspace(
            infiniteGrid,
            od,
            maStlRulersGate,
            frustumHalfY,
            lastAspect,
            maStlRulersGate ? MA_STL_DESING2_MIN_ZOOM_FLOOR : desing2OrthoMinZoom,
            desing2EnvGridSnapMm,
            desing2EnvGridMajorMm()
        );
        /* LOD + grosor: coincide con `infiniteGrid.onBeforeRender`; fuerza uso del nuevo paso menor/mayor al instante. */
        if (maStlRulersGate && renderer && infiniteGrid && infiniteGrid.material && infiniteGrid.material.uniforms) {
            const cam = activeCamera();
            const mat = infiniteGrid.material;
            const tgt = maStlDesingV2Viewer ? maStlRulerAnchorMm : controls ? controls.target : new THREE.Vector3();
            maStlSyncGridPlaneY(infiniteGrid, true, lastMaxDim);
            if (cam && cam.isPerspectiveCamera) {
                maStlSyncDesing2GridDistancePerspective(infiniteGrid, frustumHalfY, lastAspect, cam, tgt);
            }
            maStlSyncDesing2ScreenSpaceOverlay(
                mat,
                cam,
                renderer,
                desing2OrthoMinZoom,
                tgt,
                desing2EnvGridSnapMm,
                desing2EnvGridMajorMm()
            );
        }
        maStlRebuildGridIntersectionPickHighlightMeshesForSnapChange();
        rebuildMaStlUcsOverlayDecor(od);
        applyFrustumToBoth();
        clampDesing2OrthoZoom(cameraOrtho);
        clampDesing2OrthoZoom(cameraIso);
        if (controls) {
            controls.update();
        }
    }

    /**
     * Ortogonal: alzado / frontal (eje Z).
     * Isométrica: dirección (1,1,1) con proyección ortográfica.
     */
    function placeCamerasForModel(maxDim) {
        const anchor = maStlRulerAnchorMm;
        const d = Math.max(maxDim * 3, 1e-3);
        cameraOrtho.position.set(anchor.x, anchor.y, anchor.z + d);
        cameraOrtho.up.set(0, 1, 0);
        cameraOrtho.lookAt(anchor);
        if (cameraOrtho.isOrthographicCamera) cameraOrtho.zoom = 1;

        const isoDist = Math.max(maxDim * 2.5, 1e-3);
        const dir = new THREE.Vector3(1, 1, 1).normalize().multiplyScalar(isoDist);
        cameraIso.position.copy(anchor).add(dir);
        cameraIso.up.set(0, 1, 0);
        cameraIso.lookAt(anchor);
        if (cameraIso.isOrthographicCamera) cameraIso.zoom = 1;
        maStlClearDesing2OrbitDeferRulerPivotPreserve();
        maStlClearDesing2OrbitPreserveRulerPivotOnRotatePointerDown();
        maStlResetOrbitTargetToRulerAnchor();
    }

    function clampDesing2OrthoZoom(cam) {
        if (!maStlRulersGate || !cam || !cam.isOrthographicCamera) return;
        if (cam.zoom < MA_STL_DESING2_MIN_ZOOM_FLOOR) {
            cam.zoom = MA_STL_DESING2_MIN_ZOOM_FLOOR;
            cam.updateProjectionMatrix();
        }
    }

    function bindControls(camera) {
        /* VIEW CUBE 90° — DO NOT REGRESS: see desing-2-orbit-pivot.md (reuse branch: target + update only) */
        if (controls && controls.object === camera) {
            maStlClearDesing2OrbitDeferRulerPivotPreserve();
            maStlClearDesing2OrbitPreserveRulerPivotOnRotatePointerDown();
            maStlResetOrbitTargetToRulerAnchor();
            if (maStlRulersGate && camera.isOrthographicCamera) {
                maStlRefreshDesing2OrthoMinZoom();
                controls.minZoom = MA_STL_DESING2_MIN_ZOOM_FLOOR;
                clampDesing2OrthoZoom(camera);
            }
            controls.update();
            maStlWireDesing2OrbitPickLockListener();
            return;
        }
        if (controls) {
            maStlDisposeDesing2OrbitPickLockListener();
            controls.dispose();
        }
        controls = new OrbitControls(camera, renderer.domElement);
        controls.enableDamping = maStlRulersGate ? false : true;
        controls.dampingFactor = 0.06;
        maStlClearDesing2OrbitDeferRulerPivotPreserve();
        maStlClearDesing2OrbitPreserveRulerPivotOnRotatePointerDown();
        maStlResetOrbitTargetToRulerAnchor();
        if (maStlRulersGate && camera.isOrthographicCamera) {
            maStlRefreshDesing2OrthoMinZoom();
            controls.minZoom = MA_STL_DESING2_MIN_ZOOM_FLOOR;
            clampDesing2OrthoZoom(camera);
        }
        controls.update();
        maStlWireDesing2OrbitPickLockListener();
    }

    const renderer = new THREE.WebGLRenderer({ antialias: true, alpha: false });
    renderer.setClearColor(MA_STL_SKY_OFF_HEX, 1);
    renderer.setPixelRatio(Math.min(window.devicePixelRatio || 1, 2));
    renderer.setSize(w0, h0, false);
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
     * Ray desde pantalla a la primera malla STL en `clipStlMeshes` (cada entrada = objeto/parte para inserción).
     * @returns {THREE.Mesh|null}
     */
    function maStlRaycastClipStlMeshFirst(clientX, clientY) {
        if (!renderer || clipStlMeshes.length === 0) return null;
        const canvas = renderer.domElement;
        const rect = canvas.getBoundingClientRect();
        const rw = Math.max(rect.width, 1);
        const rh = Math.max(rect.height, 1);
        _maStlFloorPickNdc.set(
            ((clientX - rect.left) / rw) * 2 - 1,
            -((clientY - rect.top) / rh) * 2 + 1
        );
        const cam = activeCamera();
        cam.updateMatrixWorld(true);
        orbitPivotRaycaster.setFromCamera(_maStlFloorPickNdc, cam);
        const hits = orbitPivotRaycaster.intersectObjects(clipStlMeshes, false);
        for (let hi = 0; hi < hits.length; hi++) {
            const obj = hits[hi].object;
            if (obj && obj.isMesh) return obj;
        }
        return null;
    }

    /** Hover + recuadro verde sobre el punto de inserción de la pieza bajo cursor (modo objeto). */
    function maStlUpdateObjectInsertionPickHover(clientX, clientY) {
        if (
            maStlRulerAnchorPickMode !== 'object' ||
            !maStlDesingV2Viewer ||
            !renderer ||
            clipStlMeshes.length === 0
        ) {
            maStlClearStlPickHoverHighlight();
            maStlClearInsertionPickHighlight();
            return;
        }
        const mesh = maStlRaycastClipStlMeshFirst(clientX, clientY);
        maStlClearStlPickHoverHighlight();
        if (!mesh) {
            maStlClearInsertionPickHighlight();
            return;
        }
        maStlApplyStlPickHoverHighlight(mesh);
        const insertion = maStlGetInsertionPointBottomLeftFootprintWorld(mesh);
        maStlSetInsertionPickHighlight(true, insertion);
    }

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

    /*
     * =========================================================================
     * DESING_2 — PIVOTE DE ÓRBITA EN ANCLAJE DE REGLAS — NO REGREDIR (2026-05-20)
     * =========================================================================
     *
     * En Desing_2 los giros LMB **no raycastean** STL: el punto focal por defecto
     * viene del anclaje de reglas sólo después de **`maStlSetRulerAnchor*`** (flag
     * `maStlDesing2OrbitPreserveRulerPivotOnRotatePointerDown`); tras paneo órbita libre el
     * `controls.target` se respeta hasta cubo/bind/refit/cookie.
     *
     * Regresión repetida (~4×): onCanvasPointerDownSetOrbitPivot hacía
     * raycast → intersectObject(currentRoot) → controls.target.copy(hit).
     * Con zoom alejado el impacto cae lejos del anclaje, la órbita “se pierde”
     * y el usuario no recupera la orientación.
     *
     * Detección Desing_2: maStlUsesFixedOrbitPivotAtOrigin() === true cuando
     * #ma-stl-viewer-shell tiene `data-ma-stl-show-rulers-toggle="true"`.
     * NO usar maStlRulersGate (exige botón UCS en DOM); el shell basta.
     *
     * Obligatorio en Desing_2:
     *   • onCanvasPointerDownSetOrbitPivot — preserveView sólo cuando el flag tras colocar anclaje;
     *       defer pick-lock igual que antes (`maStlDesing2OrbitDeferRulerPivotPreserveOnNextSync`);
     *       si no aplican ambos → `update` + `saveState`.
     *   • Colocar anclaje (snap rejilla Entorno vs clic en objeto STL): sólo `maStlRulerAnchorMm` + overlays; marca el snap-en-rotate; sin mover cámara en el pointerdown del pick
     *   • Cubo de vistas — orden: applyDirectionToOrthoCam (up Y-up, lookAt anclaje)
     *       → bindControls (misma cámara: reutilizar; si no: OrbitControls nuevo)
     *       → maStlFinalizeViewCubePreset (target + update + saveState)
     *   • bindControls — reset al crear OrbitControls (tras mover cámara en cubo)
     *   • maStlApplyDesing2ViewerStateFromCookie — restaurar rulerAnchor; NO aplicar state.target
     *       (preset de pivote = anclaje; cookie target legado se ignora tras cubo o pick)
     *
     * Maestro de artículos: mantener raycast CAD bajo el cursor (rama else).
     *
     * Doc: Desing/docs/desing-2-orbit-pivot.md (VIEW CUBE 90° — DO NOT REGRESS)
     * =========================================================================
     */
    function maStlResetOrbitTargetToRulerAnchor() {
        if (controls && maStlDesingV2Viewer) {
            controls.target.copy(maStlRulerAnchorMm);
        } else if (controls) {
            controls.target.set(0, 0, 0);
        }
    }

    /**
     * Coloca el pivote de órbita en el anclaje sin cambiar encuadre (pan cámara + target).
     * No llama placeCamerasForModel / applyDirectionToOrthoCam / cookie restore.
     */
    function maStlApplyRulerAnchorOrbitPivotPreserveView() {
        if (!controls) return;
        if (!maStlDesingV2Viewer) {
            maStlResetOrbitTargetToRulerAnchor();
            controls.update();
            return;
        }
        _maStlOrbitPivotPanDelta.subVectors(maStlRulerAnchorMm, controls.target);
        controls.target.copy(maStlRulerAnchorMm);
        if (_maStlOrbitPivotPanDelta.lengthSq() > 1e-6) {
            controls.object.position.add(_maStlOrbitPivotPanDelta);
        }
        controls.update();
        if (maStlDesingV2Viewer) {
            maStlDesing2OrbitPreserveRulerPivotOnRotatePointerDown = false;
        }
    }

    /** @deprecated alias interno; usar maStlResetOrbitTargetToRulerAnchor. */
    function maStlResetOrbitTargetToSceneCenter() {
        maStlResetOrbitTargetToRulerAnchor();
    }

    /** Desing_2: shell `data-ma-stl-show-rulers-toggle="true"` → pivote fijo, sin raycast. */
    function maStlUsesFixedOrbitPivotAtOrigin() {
        return maStlDesingV2Viewer;
    }

    /** Capture pointerdown antes de OrbitControls; Desing_2 alinea pivote sin raycast + pan opcional si target quedó desfasado tras pick. */
    function onCanvasPointerDownSetOrbitPivot(ev) {
        /* Misma ventana que aplaza unlock tras pick reglas/objeto: evitar pivot/vista si el gesto aún cierra pointerup. */
        if (maStlDeferredRulerPickUnlockPointerEnded != null) return;
        if (maStlIsRulerAnchorPickModeActive()) return;
        if (maStlIsLineToolPlacementActive()) return;
        if (!stlOrbitPointerDownWillRotate(ev)) return;
        const canvas = renderer.domElement;
        if (ev.currentTarget !== canvas) return;
        const rawTarget = ev.target;
        if (rawTarget && typeof rawTarget.closest === 'function') {
            if (rawTarget.closest('button, input, select, textarea, [role="button"], label')) return;
        }
        if (!controls) return;
        /* VIEW CUBE 90° — DO NOT REGRESS: see desing-2-orbit-pivot.md (Desing_2: no STL raycast on rotate) */
        if (maStlUsesFixedOrbitPivotAtOrigin()) {
            if (maStlDesing2OrbitDeferRulerPivotPreserveOnNextSync) {
                maStlClearDesing2OrbitPreserveRulerPivotOnRotatePointerDown();
                controls.update();
                if (typeof controls.saveState === 'function') {
                    controls.saveState();
                }
                return;
            }
            if (maStlDesing2OrbitPreserveRulerPivotOnRotatePointerDown) {
                maStlApplyRulerAnchorOrbitPivotPreserveView();
            } else {
                controls.update();
            }
            if (typeof controls.saveState === 'function') {
                controls.saveState();
            }
            return;
        }
        if (!currentRoot) return;
        const rect = canvas.getBoundingClientRect();
        const rw = Math.max(rect.width, 1);
        const rh = Math.max(rect.height, 1);
        _maStlFloorPickNdc.set(
            ((ev.clientX - rect.left) / rw) * 2 - 1,
            -((ev.clientY - rect.top) / rh) * 2 + 1
        );
        const cam = activeCamera();
        cam.updateMatrixWorld(true);
        orbitPivotRaycaster.setFromCamera(_maStlFloorPickNdc, cam);
        const hits = orbitPivotRaycaster.intersectObject(currentRoot, true);
        if (hits.length > 0) {
            controls.target.copy(hits[0].point);
        }
        controls.update();
    }

    /**
     * Modo rejilla (#ma-stl-ruler-anchor-pick-toggle): sólo cruces al paso menor de rejilla (Entorno).
     * Modo objeto (#ma-stl-ruler-anchor-object-pick-toggle): raycast STL → punto de inserción (`maStlGetInsertionPointBottomLeftFootprintWorld`).
     */
    function onCanvasPointerDownRulerAnchorPick(ev) {
        if (!maStlIsRulerAnchorPickModeActive() || !maStlDesingV2Viewer) return;
        if (ev.button !== 0) return;
        const canvas = renderer.domElement;
        if (ev.currentTarget !== canvas) return;
        const rawTarget = ev.target;
        if (rawTarget && typeof rawTarget.closest === 'function') {
            if (rawTarget.closest('button, input, select, textarea, [role="button"], label')) return;
        }
        ev.preventDefault();
        ev.stopPropagation();
        ev.stopImmediatePropagation();
        const cam = activeCamera();
        if (maStlRulerAnchorPickMode === 'grid') {
            const lineSnapFromClick = maStlFindFloorLineVertexSnapAtPointer(
                ev.clientX,
                ev.clientY,
                MA_STL_LINE_TOOL_GRID_PICK_SCREEN_PX_BOOST
            );
            if (lineSnapFromClick) {
                maStlSetRulerAnchorFromGridSnap(lineSnapFromClick);
                if (maStlRulerAnchorPickToggleBtn) {
                    maStlDesing2ShowSaveViewToast(
                        maStlRulerAnchorPickToggleBtn.getAttribute(
                            'data-ma-stl-ruler-anchor-pick-grid-toast'
                        ) || 'Reglas colocadas'
                    );
                }
                maStlExitRulerAnchorPickAfterPlacement();
                return;
            }
            const gridSnapFromClick = maStlClientRayToWorkspaceFloor(
                ev.clientX,
                ev.clientY,
                canvas,
                cam,
                orbitPivotNdc,
                orbitPivotRaycaster,
                _maStlInsertionFloorProbe
            )
                ? maStlSnapFloorToGridIntersection(
                      { x: _maStlInsertionFloorProbe.x, z: _maStlInsertionFloorProbe.z },
                      {
                          clientX: ev.clientX,
                          clientY: ev.clientY,
                          camera: cam,
                          canvas: canvas,
                          maxDim: lastMaxDim
                      },
                      desing2EnvGridSnapMm
                  )
                : null;
            if (gridSnapFromClick && gridSnapFromClick.active) {
                maStlSetRulerAnchorFromGridSnap(gridSnapFromClick);
                if (maStlRulerAnchorPickToggleBtn) {
                    maStlDesing2ShowSaveViewToast(
                        maStlRulerAnchorPickToggleBtn.getAttribute(
                            'data-ma-stl-ruler-anchor-pick-grid-toast'
                        ) || 'Reglas colocadas'
                    );
                }
                maStlExitRulerAnchorPickAfterPlacement();
                return;
            }
            const needSnapTpl =
                (maStlRulerAnchorPickToggleBtn &&
                    maStlRulerAnchorPickToggleBtn.getAttribute(
                        'data-ma-stl-ruler-anchor-grid-snap-required-toast'
                    )) ||
                '';
            if (needSnapTpl) {
                maStlDesing2ShowSaveViewToast(needSnapTpl);
            }
            return;
        }
        /** `object`: colocación sólo con impacto válido sobre malla STL. */
        const hitMesh =
            clipStlMeshes.length > 0 ? maStlRaycastClipStlMeshFirst(ev.clientX, ev.clientY) : null;
        if (hitMesh) {
            const ins = maStlGetInsertionPointBottomLeftFootprintWorld(hitMesh);
            maStlSetRulerAnchorFromInsertionPoint(ins);
            const toastTpl =
                (maStlRulerAnchorObjectPickToggleBtn &&
                    maStlRulerAnchorObjectPickToggleBtn.getAttribute(
                        'data-ma-stl-ruler-anchor-insertion-toast'
                    )) ||
                '';
            if (toastTpl) maStlDesing2ShowSaveViewToast(toastTpl);
            maStlExitRulerAnchorPickAfterPlacement();
            return;
        }
        const missTpl =
            (maStlRulerAnchorObjectPickToggleBtn &&
                maStlRulerAnchorObjectPickToggleBtn.getAttribute(
                    'data-ma-stl-ruler-anchor-object-miss-toast'
                )) ||
            '';
        if (missTpl) maStlDesing2ShowSaveViewToast(missTpl);
    }

    renderer.domElement.addEventListener('pointerdown', onCanvasPointerDownLineTool, true);
    renderer.domElement.addEventListener('click', onCanvasClickLineTool, true);
    renderer.domElement.addEventListener('pointerdown', onCanvasPointerDownRulerAnchorPick, true);
    renderer.domElement.addEventListener('pointermove', onCanvasPointerMoveLineToolSync);
    renderer.domElement.addEventListener('pointermove', onCanvasPointerMoveRulerAnchorPick);
    const hudPointerMoveHost =
        maStlDesingV2Viewer && maStlViewerCanvasHudWrapEl instanceof Element
            ? maStlViewerCanvasHudWrapEl
            : renderer.domElement;
    hudPointerMoveHost.addEventListener('pointermove', onCanvasPointerMoveUserFloorLineHover);
    renderer.domElement.addEventListener('dblclick', onCanvasDblClickUserFloorLineDimension, true);
    renderer.domElement.addEventListener('pointerleave', onCanvasPointerLeaveInsertionPick);

    renderer.domElement.addEventListener('pointerdown', onCanvasPointerDownSetOrbitPivot, true);

    if (maStlDesingV2Viewer) {
        renderer.domElement.addEventListener('pointerdown', onCanvasPointerDownUserFloorLineRefactorRmb, true);
        window.addEventListener('pointerup', onWindowPointerUpUserFloorLineRefactorRmb, true);
        window.addEventListener('pointercancel', onWindowPointerCancelUserFloorLineRefactorRmb, true);
    }

    if (maStlDesingV2Viewer && maStlLineToolHudDistanceInput) {
        maStlLineToolHudDistanceInput.autocomplete = 'off';
        maStlLineToolHudDistanceInput.addEventListener('input', function () {
            if (maStlLineToolState !== 'picking2') return;
            maStlLineToolDistanceTypeBuffer = String(maStlLineToolHudDistanceInput.value || '');
            maStlLineToolSyncTypingPreviewUi();
            maStlLineToolRefreshPicking2RubberBand();
        });
        maStlLineToolHudDistanceInput.addEventListener('keydown', function (ev) {
            if (!(ev.key === 'Enter' || ev.code === 'Enter' || ev.key === 'NumpadEnter') || ev.shiftKey) return;
            ev.preventDefault();
            maStlLineToolTryTypedCommitDistanceOrbitDefer();
        });
    }

    function maStlIsDesing2ViewerShellVisibleForKeyboardShortcuts() {
        if (!viewerShell || !maStlDesingV2Viewer) return false;
        if (viewerShell.hasAttribute('hidden')) return false;
        const cs = window.getComputedStyle(viewerShell);
        return cs.display !== 'none' && cs.visibility !== 'hidden';
    }

    function maStlDesingV2AvoidKeyboardShortcutSteal(activeEl) {
        if (!activeEl || typeof activeEl.closest !== 'function') return false;
        if (activeEl.closest('textarea, select, [contenteditable="true"]')) return true;
        if (!(activeEl instanceof HTMLInputElement)) return false;
        const t = activeEl.type ? activeEl.type.toLowerCase() : '';
        /** Text-ish / IME: no interferir Escape. */
        return (
            t !== 'button' &&
            t !== 'checkbox' &&
            t !== 'radio' &&
            t !== 'reset' &&
            t !== 'submit' &&
            t !== 'hidden' &&
            t !== 'file' &&
            t !== 'image' &&
            t !== 'range' &&
            t !== 'color'
        );
    }

    /** Quitar Escape global si el bootstrap del visor algún día añade teardown explícito. */
    function maStlDisposeDesingV2EscapeKeyListener() {
        if (!_maStlDesingV2EscapeKeydownHandler) return;
        window.removeEventListener('keydown', _maStlDesingV2EscapeKeydownHandler);
        _maStlDesingV2EscapeKeydownHandler = null;
    }

    function maStlDisposeDesingV2LineToolDistanceKeyListener() {
        if (!_maStlDesingV2LineToolDistanceKeyHandler) return;
        window.removeEventListener('keydown', _maStlDesingV2LineToolDistanceKeyHandler);
        _maStlDesingV2LineToolDistanceKeyHandler = null;
    }

    function maStlDisposeDesingV2F8OrthoKeyListener() {
        if (!_maStlDesingV2F8OrthoKeydownHandler) return;
        window.removeEventListener('keydown', _maStlDesingV2F8OrthoKeydownHandler);
        _maStlDesingV2F8OrthoKeydownHandler = null;
    }

    function maStlWireDesingV2EscapeKeyListener() {
        maStlDisposeDesingV2EscapeKeyListener();
        if (!maStlDesingV2Viewer || !viewerShell) return;
        _maStlDesingV2EscapeKeydownHandler = function (ev) {
            if (ev.code !== 'Escape' && ev.key !== 'Escape') return;
            if (!maStlIsDesing2ViewerShellVisibleForKeyboardShortcuts()) return;
            if (ev.defaultPrevented) return;
            const ae = document.activeElement;
            const lineDistFocused =
                maStlLineToolHudDistanceInput &&
                ae === maStlLineToolHudDistanceInput &&
                maStlLineToolState === 'picking2';
            if (maStlDesingV2AvoidKeyboardShortcutSteal(ae) && !lineDistFocused) return;
            if (maStlCancelAllViewerInteractionModes()) {
                ev.preventDefault();
                ev.stopPropagation();
            }
        };
        window.addEventListener('keydown', _maStlDesingV2EscapeKeydownHandler);
    }

    /** `picking2` línea: dígitos/Enter/punto — no compite cotas STL ni inputs de página. */
    function maStlWireDesingV2LineToolDistanceKeyListener() {
        maStlDisposeDesingV2LineToolDistanceKeyListener();
        if (!maStlDesingV2Viewer || !viewerShell) return;
        _maStlDesingV2LineToolDistanceKeyHandler = function (ev) {
            maStlLineToolApplyWindowKeydownToDistanceBuffer(ev);
        };
        window.addEventListener('keydown', _maStlDesingV2LineToolDistanceKeyHandler);
    }

    /** F8 (Desing_2): alterna orto 15°; no cuando el foco está en campos de texto o edición IME. */
    function maStlWireDesingV2F8OrthoKeyListener() {
        maStlDisposeDesingV2F8OrthoKeyListener();
        if (!maStlDesingV2Viewer || !viewerShell) return;
        _maStlDesingV2F8OrthoKeydownHandler = function (ev) {
            if (ev.code !== 'F8' && ev.key !== 'F8') return;
            if (!maStlIsDesing2ViewerShellVisibleForKeyboardShortcuts()) return;
            if (ev.defaultPrevented) return;
            const ae = document.activeElement;
            if (maStlDesingV2AvoidKeyboardShortcutSteal(ae)) return;
            ev.preventDefault();
            ev.stopPropagation();
            maStlToggleLineToolOrtho15FromUi();
        };
        window.addEventListener('keydown', _maStlDesingV2F8OrthoKeydownHandler);
    }

    maStlWireDesingV2EscapeKeyListener();
    maStlWireDesingV2LineToolDistanceKeyListener();
    maStlWireDesingV2F8OrthoKeyListener();

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
        maStlSyncShadowGroundMaterialVisual();
        maStlSyncDesing2RulerLineMaterialToTheme();
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

    const { mainDirLight } = maStlCreateSceneLights(scene, maStlRulersGate);
    const _maStlShadowTarget = new THREE.Vector3();

    function syncGroundShadowToggleUi() {
        shadowGroundPlane.visible = groundShadowVisible;
        mainDirLight.castShadow = groundShadowVisible;
        renderer.shadowMap.enabled = groundShadowVisible;
        maStlSyncShadowGroundMaterialVisual();
        clipStlMeshes.forEach(function (mesh) {
            mesh.castShadow = groundShadowVisible;
            mesh.receiveShadow = false;
        });
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

    /**
     * Serializa cámara activa + toggles para cookie Desing_2 (sin URL STL ni datos de usuario).
     * @returns {object}
     */
    function maStlDesing2BuildViewerStateSnapshot() {
        const rendererCam = activeCamera();
        const toggles = {
            grid: gridVisible,
            sky: skyVisible,
            groundShadow: groundShadowVisible,
            darkBg: darkBgVisible,
            clipUi: clipUiVisible,
            xyzAxes: maStlXyzAxesManualOn,
            ucsRulers: maStlUcsRulersManualOn
        };
        const snap = {
            v: MA_STL_DESING2_VIEWER_STATE_VERSION,
            activeCamera: rendererCam === cameraIso ? 'iso' : 'ortho',
            cameraOrtho: maStlSerializeCameraState(cameraOrtho),
            cameraIso: maStlSerializeCameraState(cameraIso),
            target: controls ? [controls.target.x, controls.target.y, controls.target.z] : [0, 0, 0],
            rulerAnchor: {
                x: maStlRulerAnchorMm.x,
                y: maStlRulerAnchorMm.y,
                z: maStlRulerAnchorMm.z
            },
            toggles: toggles,
            environment: {
                gridSnapMm: desing2EnvGridSnapMm,
                rulerExtentCapM: desing2EnvRulerExtentCapMm / MA_STL_SCENE_MM_PER_PHYSICAL_METER
            }
        };
        if (clipInputX) snap.clipX = Number.parseFloat(String(clipInputX.value).trim()) || 1000;
        if (clipInputY) snap.clipY = Number.parseFloat(String(clipInputY.value).trim()) || 1000;
        return snap;
    }

    /**
     * Restaura toggles y cámaras desde cookie (tras refit; no sustituye frustum por tamaño de modelo).
     * @param {object|null|undefined} state
     */
    function maStlApplyDesing2ViewerStateFromCookie(state) {
        if (!state || typeof state !== 'object') return;
        maStlDesing2RestoringViewerState = true;
        try {
            const t = state.toggles;
            if (t && typeof t === 'object') {
                if (typeof t.grid === 'boolean') {
                    gridVisible = t.grid;
                    syncGridToggleUi();
                }
                if (typeof t.sky === 'boolean') {
                    skyVisible = t.sky;
                    syncSkyToggleUi();
                }
                if (typeof t.darkBg === 'boolean') {
                    darkBgVisible = t.darkBg;
                    syncDarkBgToggleUi();
                }
                if (typeof t.groundShadow === 'boolean') {
                    groundShadowVisible = t.groundShadow;
                    syncGroundShadowToggleUi();
                }
                if (typeof t.clipUi === 'boolean') {
                    clipUiVisible = t.clipUi;
                    syncClipToggleUi();
                }
                if (typeof t.ucsRulers === 'boolean') {
                    maStlUcsRulersManualOn = t.ucsRulers;
                    syncMaStlUcsOverlayVisibility();
                    syncMaStlUcsRulersToggleBtnUi();
                }
                if (typeof t.xyzAxes === 'boolean') {
                    maStlXyzAxesManualOn = t.xyzAxes;
                    syncMaStlUcsOverlayVisibility();
                    syncMaStlXyzAxesToggleBtnUi();
                }
            }
            if (clipInputX && Number.isFinite(state.clipX)) {
                clipInputX.value = String(THREE.MathUtils.clamp(Math.round(state.clipX), 0, 1000));
            }
            if (clipInputY && Number.isFinite(state.clipY)) {
                clipInputY.value = String(THREE.MathUtils.clamp(Math.round(state.clipY), 0, 1000));
            }
            const envIn = state.environment;
            if (envIn && typeof envIn === 'object') {
                if (Number.isFinite(envIn.gridSnapMm)) {
                    desing2EnvGridSnapMm = maStlClampAllowedDesing2GridSnapMm(envIn.gridSnapMm);
                    if (maStlEntornoGridSnapSelect instanceof HTMLSelectElement) {
                        maStlEntornoGridSnapSelect.value = String(desing2EnvGridSnapMm);
                    }
                }
                if (Number.isFinite(envIn.rulerExtentCapM)) {
                    desing2EnvRulerExtentCapMm = maStlDesing2RulerExtentCapFromMeters(
                        envIn.rulerExtentCapM
                    );
                    if (maStlEntornoRulerExtentSelect instanceof HTMLSelectElement) {
                        let bestVal = '';
                        let bestDelta = Infinity;
                        const tgt = THREE.MathUtils.clamp(envIn.rulerExtentCapM, 5, 80);
                        const sel = maStlEntornoRulerExtentSelect;
                        for (let oi = 0; oi < sel.options.length; oi++) {
                            const ov = Number.parseFloat(sel.options[oi].value);
                            if (!Number.isFinite(ov)) continue;
                            const dd = Math.abs(ov - tgt);
                            if (dd < bestDelta) {
                                bestDelta = dd;
                                bestVal = sel.options[oi].value;
                            }
                        }
                        if (bestVal !== '') sel.value = bestVal;
                    }
                }
            }
            if (state.rulerAnchor && typeof state.rulerAnchor === 'object') {
                const ra = state.rulerAnchor;
                if (Number.isFinite(ra.x) && Number.isFinite(ra.z)) {
                    maStlClearDesing2OrbitDeferRulerPivotPreserve();
                    maStlClearDesing2OrbitPreserveRulerPivotOnRotatePointerDown();
                    maStlRulerAnchorMm.set(
                        ra.x,
                        Number.isFinite(ra.y) ? ra.y : MA_STL_DESING2_WORKSPACE_FLOOR_Y_MM,
                        ra.z
                    );
                    maStlInvalidateUserFloorDimGuideGeomCache();
                    maStlSyncUserFloorDimHudScreenOnly();
                    rebuildMaStlUcsOverlayDecor(lastMaxDim);
                }
            }
            /* Desing_2: state.target en cookie es informativo; pivote = rulerAnchor (no sobrescribir tras cubo). */
            updateClipPlanes();
            if (state.cameraOrtho) maStlApplyCameraState(cameraOrtho, state.cameraOrtho, { skipLookAt: true });
            if (state.cameraIso) maStlApplyCameraState(cameraIso, state.cameraIso, { skipLookAt: true });
            clampDesing2OrthoZoom(cameraOrtho);
            clampDesing2OrthoZoom(cameraIso);
            const mode = state.activeCamera === 'iso' ? 'iso' : 'ortho';
            setCameraMode(mode);
            maStlResetOrbitTargetToRulerAnchor();
            if (state.environment && typeof state.environment === 'object') {
                applyDesing2EntornoLive();
            }
            if (controls) {
                controls.object.updateMatrixWorld(true);
                controls.update();
                if (typeof controls.saveState === 'function') {
                    controls.saveState();
                }
            }
        } finally {
            maStlDesing2RestoringViewerState = false;
        }
    }

    /** @returns {boolean} */
    function maStlDesing2SaveViewerStateToCookie() {
        if (!maStlDesingV2Viewer || maStlDesing2RestoringViewerState) return false;
        try {
            const json = JSON.stringify(maStlDesing2BuildViewerStateSnapshot());
            return maStlWriteCookie(MA_STL_DESING2_VIEWER_COOKIE_GLOBAL, json, MA_STL_DESING2_VIEWER_COOKIE_MAX_AGE_SEC);
        } catch (_e) {
            return false;
        }
    }

    let maStlDesing2SaveViewFeedbackTimer = null;

    /**
     * @param {string} message
     */
    function maStlDesing2ShowSaveViewToast(message) {
        const text = (message || '').trim();
        if (!text) return;
        let container = document.getElementById('ma-stl-desing2-toast-container');
        if (!container) {
            container = document.createElement('div');
            container.id = 'ma-stl-desing2-toast-container';
            container.className = 'toast-container position-fixed top-0 end-0 p-3 ma-stl-desing2-toast-container';
            container.setAttribute('aria-live', 'polite');
            container.setAttribute('aria-atomic', 'true');
            document.body.appendChild(container);
        }
        const toastEl = document.createElement('div');
        toastEl.className = 'toast bs-toast bg-success';
        toastEl.setAttribute('role', 'alert');
        toastEl.setAttribute('aria-live', 'assertive');
        toastEl.setAttribute('aria-atomic', 'true');
        const body = document.createElement('div');
        body.className = 'toast-body d-flex align-items-center gap-2';
        const icon = document.createElement('i');
        icon.className = 'icon-base ri ri-checkbox-circle-line flex-shrink-0';
        icon.setAttribute('aria-hidden', 'true');
        const label = document.createElement('span');
        label.textContent = text;
        body.appendChild(icon);
        body.appendChild(label);
        toastEl.appendChild(body);
        container.appendChild(toastEl);
        const ToastCtor = typeof bootstrap !== 'undefined' && bootstrap && bootstrap.Toast;
        if (ToastCtor) {
            const toast = new ToastCtor(toastEl, { autohide: true, delay: 3000 });
            toastEl.addEventListener('hidden.bs.toast', function () {
                toast.dispose();
                toastEl.remove();
            });
            toast.show();
            return;
        }
        toastEl.classList.add('show');
        window.setTimeout(function () {
            toastEl.classList.remove('show');
            window.setTimeout(function () {
                toastEl.remove();
            }, 300);
        }, 3000);
    }

    /**
     * @param {HTMLButtonElement | null | undefined} btn
     */
    function maStlDesing2FlashSaveViewFeedback(btn) {
        if (!btn) return;
        const defaultTitle = btn.getAttribute('data-ma-stl-save-view-title') || btn.title || '';
        const savedTitle = btn.getAttribute('data-ma-stl-save-view-saved') || 'Guardado';
        if (maStlDesing2SaveViewFeedbackTimer) window.clearTimeout(maStlDesing2SaveViewFeedbackTimer);
        btn.title = savedTitle;
        btn.setAttribute('aria-label', savedTitle);
        btn.classList.add('active');
        maStlDesing2SaveViewFeedbackTimer = window.setTimeout(function () {
            maStlDesing2SaveViewFeedbackTimer = null;
            btn.title = defaultTitle;
            btn.setAttribute('aria-label', defaultTitle);
            btn.classList.remove('active');
        }, 1600);
    }

    const maStlSaveViewerStateBtn = document.getElementById('ma-stl-save-viewer-state');
    if (maStlSaveViewerStateBtn instanceof HTMLButtonElement && maStlDesingV2Viewer) {
        const saveStateBtn = maStlSaveViewerStateBtn;
        saveStateBtn.addEventListener('click', function () {
            if (maStlDesing2SaveViewerStateToCookie()) {
                maStlDesing2FlashSaveViewFeedback(saveStateBtn);
                maStlDesing2ShowSaveViewToast(saveStateBtn.getAttribute('data-ma-stl-save-view-toast') || '');
            }
        });
    }
    if (maStlDesingV2Viewer && viewerShell) {
        /** Captura/select en panel Entorno puede no propagarse como en elementos sueltos; delegación sobre el shell Desing_2. */
        function maStlOnDesing2EntornoSelectInput(ev) {
            const el = ev.target;
            const id = el instanceof Element ? el.id : '';
            if (id !== 'ma-stl-entorno-grid-snap-mm' && id !== 'ma-stl-entorno-ruler-extent-m') return;
            if (!(el instanceof HTMLSelectElement)) return;
            applyDesing2EntornoLive();
        }
        viewerShell.addEventListener('change', maStlOnDesing2EntornoSelectInput);
        viewerShell.addEventListener('input', maStlOnDesing2EntornoSelectInput);
    }

    (function wireDesing2EscapeCancelTransientModes() {
        if (!maStlDesingV2Viewer) return;
        const escapeToastTpl =
            (viewerShell &&
                viewerShell.getAttribute &&
                viewerShell.getAttribute('data-ma-stl-escape-cancel-toast')) ||
            '';

        function escapeTargetTypingOrModalHost(evTarget) {
            const root =
                evTarget && /** @type {Node} */ (evTarget.nodeType === 1 ? evTarget : evTarget.parentElement);
            if (!root || typeof root.closest !== 'function') return false;
            if (root.closest('.modal.show')) return true;
            if (root.closest('[contenteditable="true"]')) return true;
            const formField = root.closest('input, textarea, select');
            if (!formField || !(formField instanceof HTMLElement)) return false;
            if (formField instanceof HTMLSelectElement || formField instanceof HTMLTextAreaElement) {
                return true;
            }
            if (formField instanceof HTMLInputElement) {
                const tp = (formField.type || '').toLowerCase();
                if (
                    tp === 'button' ||
                    tp === 'submit' ||
                    tp === 'reset' ||
                    tp === 'checkbox' ||
                    tp === 'radio' ||
                    tp === 'range' ||
                    tp === 'file' ||
                    tp === 'color' ||
                    tp === 'hidden'
                ) {
                    return false;
                }
                return true;
            }
            return false;
        }

        function maStlDesing2TransientEscapeHandledBeforeCancel() {
            return (
                maStlLineToolState != null ||
                maStlRulerAnchorPickMode !== null ||
                !!maStlRulerAnchorPickOrbitLockSnapshot ||
                !!maStlDeferredRulerPickUnlockPointerEnded ||
                !!(maStlLineToolRubberBandLine && maStlLineToolRubberBandLine.visible === true)
            );
        }

        function onEscapeKeyDown(ev) {
            if (!(ev instanceof KeyboardEvent)) return;
            if (escapeTargetTypingOrModalHost(ev.target)) return;
            if (ev.repeat) return;
            if (ev.key !== 'Escape' && ev.key !== 'Esc') return;
            if (!maStlDesing2TransientEscapeHandledBeforeCancel()) return;
            ev.preventDefault();
            maStlDesing2CancelTransientToolsEscape();
            if ((escapeToastTpl || '').trim()) {
                maStlDesing2ShowSaveViewToast(escapeToastTpl);
            }
        }

        window.addEventListener('keydown', onEscapeKeyDown, true);
        window.addEventListener(
            'pagehide',
            function disposeMaStlDesing2EscapeHandler() {
                window.removeEventListener('keydown', onEscapeKeyDown, true);
            },
            { once: true }
        );
    })();

    function maStlDesing2TryRestoreViewerStateFromCookie() {
        if (!maStlDesingV2Viewer || maStlDesing2StateRestored || !pendingDesing2Restore) {
            return;
        }
        maStlApplyDesing2ViewerStateFromCookie(pendingDesing2Restore);
        maStlDesing2StateRestored = true;
        pendingDesing2Restore = null;
    }

    applyFrustumToBoth();
    placeCamerasForModel(lastMaxDim);
    bindControls(activeCamera());
    syncCameraRadios();
    syncViewCubesVisibility();

    if (maStlDesingV2Viewer && pendingDesing2Restore && !maStlDesing2StateRestored) {
        const bootBtn = document.getElementById('desing2-initial-stl-boot');
        const autoStlUrl = bootBtn ? (bootBtn.getAttribute('data-stl-url') || '').trim() : '';
        if (!autoStlUrl) {
            requestAnimationFrame(maStlDesing2TryRestoreViewerStateFromCookie);
        }
    }

    /** Rosa náutica SVG (solo Desing_2): overlay DOM; sigue `activeCamera()` al cambiar orto/iso. */
    const compassDialEl = maStlDesingV2Viewer ? document.getElementById('ma-stl-compass-dial') : null;

    const orthoCubeWrap = document.getElementById('ma-stl-view-cube-ortho-wrap');
    if (orthoCubeWrap) {
        orthoCubeWrap.addEventListener('click', function (ev) {
            const t = ev.target instanceof Element ? ev.target : null;
            if (!t) return;
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
        if (controls && !maStlIsRulerAnchorPickModeActive()) {
            controls.update();
        }
        setViewCubeCssFromCamera(orthoCubeEl, cameraOrtho);
        setViewCubeCssFromCamera(isoCubeEl, cameraIso);
        const camMain = activeCamera();
        renderer.render(scene, camMain);
        if (compassDialEl && controls) {
            const orbitPivotHud = maStlDesingV2Viewer ? maStlRulerAnchorMm : controls.target;
            const deg = maStlSvgCompassDialRotationDeg(camMain, orbitPivotHud);
            compassDialEl.setAttribute(
                'transform',
                'translate(100 100) rotate(' + deg + ') translate(-100 -100)'
            );
        }
        if (
            maStlDesingV2Viewer &&
            maStlHoveredUserFloorLine &&
            !maStlUserFloorDimDomHudEditing &&
            !maStlIsLineToolPlacementActive() &&
            !maStlIsRulerAnchorPickModeActive()
        ) {
            maStlSyncUserFloorDimHudScreenOnly();
        }
        if (maStlDesingV2Viewer && maStlLineToolPreviewDimActive && maStlLineToolState === 'picking2') {
            maStlLineToolSyncPreviewDimHudScreenOnly();
        }
        if (maStlDesingV2Viewer && maStlUserFloorDimDomHudEditing && maStlUserFloorLineDimEditLineRef) {
            maStlRefreshUserFloorLineDimEditHudPositions();
        }
    }
    function resizeRendererToHost() {
        const sz = readHostSizeCssPx(w0, h0);
        const nw = sz.nw;
        const nh = sz.nh;
        lastAspect = nw / Math.max(nh, 1);
        applyFrustumToBoth();
        maStlRefreshDesing2OrthoMinZoom();
        if (maStlRulersGate) {
            maStlSyncInfiniteGridWorkspace(
                infiniteGrid,
                Math.max(lastMaxDim, desing2EnvGridMajorMm() * 8),
                true,
                frustumHalfY,
                lastAspect,
                MA_STL_DESING2_MIN_ZOOM_FLOOR,
                desing2EnvGridSnapMm,
                desing2EnvGridMajorMm()
            );
        }
        renderer.setPixelRatio(Math.min(window.devicePixelRatio || 1, 2));
        renderer.setSize(nw, nh, false);
        maStlSyncAllUserFloorLineMaterialResolutions(nw, nh);
    }

    /** Dos frames: tras fullscreen el layout del shell aún no ha aplicado en el primer frame. */
    function scheduleResizeToHost() {
        resizeRendererToHost();
        requestAnimationFrame(function () {
            resizeRendererToHost();
            requestAnimationFrame(resizeRendererToHost);
        });
    }

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
            scheduleResizeToHost();
        });
        document.addEventListener('webkitfullscreenchange', function () {
            syncFullscreenToggleUi();
            scheduleResizeToHost();
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
        if (!(inp instanceof HTMLInputElement)) return;
        inp.addEventListener('change', function () {
            if (!inp.checked) return;
            setCameraMode(inp.value);
        });
    });

    /**
     * @param {HTMLInputElement | null | undefined} inputEl
     */
    function clipFractionFromSlider(inputEl) {
        if (!inputEl) return 0;
        const vRaw = Number.parseFloat(String(inputEl.value).trim());
        const v = Number.isFinite(vRaw) ? vRaw : 1000;
        return THREE.MathUtils.clamp((1000 - v) / 1000, 0, 1);
    }

    function updateClipPlanes() {
        if (!clipStlMeshes.length) return;
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
        clipStlMeshes.forEach(function (m) {
            if (m.material) {
                m.material.clippingPlanes = [clipPlaneY, clipPlaneX];
            }
        });
    }

    if (clipInputY) {
        clipInputY.addEventListener('input', function () {
            updateClipPlanes();
        });
    }
    if (clipInputX) {
        clipInputX.addEventListener('input', function () {
            updateClipPlanes();
        });
    }

    /**
     * Característica de tamaño para encuadre (cámara / frustum) sin mover geometría: arista máxima del AABB
     * y extensión máxima desde el origen (inserción CAD en (0,0,0)) por si el modelo no está centrado en bbox.
     */
    function masterArticleStlFitMaxDimFromWorldBox(box) {
        const size = box.getSize(new THREE.Vector3());
        const maxEdge = Math.max(size.x, size.y, size.z, 1e-6);
        const spanX = Math.max(Math.abs(box.min.x), Math.abs(box.max.x));
        const spanY = Math.max(Math.abs(box.min.y), Math.abs(box.max.y));
        const spanZ = Math.max(Math.abs(box.min.z), Math.abs(box.max.z));
        const spanFromOrigin = Math.max(spanX, spanY, spanZ, 1e-6);
        return Math.max(maxEdge, spanFromOrigin);
    }

    function refitCamerasToObject(group) {
        if (maStlRulersGate) {
            masterArticleStlGroundGroupOnWorkspaceFloor(group, MA_STL_DESING2_WORKSPACE_FLOOR_Y_MM);
        }
        group.updateMatrixWorld(true);
        const box = new THREE.Box3().setFromObject(group);
        const modelDim = masterArticleStlFitMaxDimFromWorldBox(box);
        lastMaxDim = modelDim;
        /** Rejilla/reglas/UCS: dimensión de workspace (no encoger por mallas pequeñas en mm escena). */
        const overlayDim = maStlRulersGate
            ? Math.max(modelDim, desing2EnvGridMajorMm() * 8)
            : modelDim;
        frustumHalfY = maStlFrustumHalfYFromMaxDim(
            overlayDim,
            maStlRulersGate,
            maStlRulersGate ? desing2EnvGridMajorMm() : undefined
        );
        maStlRefreshDesing2OrthoMinZoom();
        const hostSz = readHostSizeCssPx(400, 380);
        lastAspect = hostSz.nw / Math.max(hostSz.nh, 1);
        maStlSyncInfiniteGridWorkspace(
            infiniteGrid,
            overlayDim,
            maStlRulersGate,
            frustumHalfY,
            lastAspect,
            maStlRulersGate ? MA_STL_DESING2_MIN_ZOOM_FLOOR : desing2OrthoMinZoom,
            desing2EnvGridSnapMm,
            desing2EnvGridMajorMm()
        );
        if (!maStlRulersGate) {
            const gMat = infiniteGrid.material;
            if (gMat && gMat.uniforms && gMat.uniforms.uPlaneY) {
                gMat.uniforms.uPlaneY.value = box.min.y - Math.max(modelDim * 0.008, 1e-6);
            }
        }
        const camFitDim = maStlRulersGate
            ? Math.max(overlayDim * 1.22, desing2EnvGridMajorMm() * 4)
            : Math.max(modelDim * 1.18, 1e-6);
        if (maStlRulersGate) {
            mainDirLight.target.position.set(0, 0, 0);
            mainDirLight.target.updateMatrixWorld();
            const shadowCam = mainDirLight.shadow.camera;
            const s = Math.max(overlayDim * 0.5, desing2EnvGridMajorMm() * 2);
            shadowCam.left = -s;
            shadowCam.right = s;
            shadowCam.top = s;
            shadowCam.bottom = -s;
            shadowCam.updateProjectionMatrix();
        } else {
            box.getCenter(_maStlShadowTarget);
            mainDirLight.target.position.copy(_maStlShadowTarget);
            mainDirLight.target.updateMatrixWorld();
            const lightSpan = Math.max(modelDim, overlayDim, 1e-3);
            mainDirLight.position.set(
                _maStlShadowTarget.x + lightSpan * 0.45,
                _maStlShadowTarget.y + lightSpan * 1.05,
                _maStlShadowTarget.z + lightSpan * 0.6
            );
            const shadowCam = mainDirLight.shadow.camera;
            const s = lightSpan * 3.2;
            shadowCam.left = -s;
            shadowCam.right = s;
            shadowCam.top = s;
            shadowCam.bottom = -s;
            shadowCam.near = Math.max(lightSpan * 0.002, 0.2);
            shadowCam.far = Math.max(lightSpan * 24, 800);
            shadowCam.updateProjectionMatrix();
        }
        const floorSpan = Math.max(overlayDim * 140, 2500);
        skyFloorPlane.scale.set(floorSpan, floorSpan, 1);
        skyFloorPlane.position.set(0, -Math.max(overlayDim * 0.018, 5e-4), 0);
        shadowGroundPlane.scale.set(floorSpan, floorSpan, 1);
        shadowGroundPlane.position.set(0, 0, 0);
        clipBounds.min.copy(box.min);
        clipBounds.max.copy(box.max);
        updateClipPlanes();
        rebuildMaStlUcsOverlayDecor(overlayDim);
        if (maStlDesingV2Viewer && maStlIsRulerAnchorPickModeActive()) {
            maStlTeardownPickHighlightsOnly();
        }
        applyFrustumToBoth();
        const shouldRestoreDesing2View =
            maStlDesingV2Viewer && !maStlDesing2StateRestored && !!pendingDesing2Restore;
        const skipDefaultCameraPlacement =
            shouldRestoreDesing2View || (maStlDesingV2Viewer && maStlDesing2StateRestored);
        if (!skipDefaultCameraPlacement) {
            placeCamerasForModel(modelDim);
        }
        clampDesing2OrthoZoom(cameraOrtho);
        clampDesing2OrthoZoom(cameraIso);
        bindControls(activeCamera());
        if (shouldRestoreDesing2View) {
            maStlDesing2TryRestoreViewerStateFromCookie();
        }
    }

    function makeStlMeshStandardMaterial(tintColor) {
        return new THREE.MeshStandardMaterial({
            color: tintColor.clone(),
            metalness: 0.14,
            roughness: 0.42,
            side: THREE.DoubleSide,
            clippingPlanes: [clipPlaneY, clipPlaneX],
            clipShadows: true
        });
    }

    /**
     * Opcional: `{{base}}2.stl` junto al primario. Mismo `THREE.Group` y mismas rotaciones (origen escena compartido).
     * 404 u error de red → se ignora sin mensaje. Hereda `group.scale` (mm escena en Desing_2); mismo grupo y rotación que el primario.
     * @param {string} primaryUrl
     * @param {THREE.Group} group
     * @param {number} myToken
     * @param {*} loader
     */
    function tryLoadSecondaryStl(primaryUrl, group, myToken, loader) {
        const url2 = masterArticleStlSecondaryUrlFromPrimary(primaryUrl);
        if (!url2 || url2 === primaryUrl) return;
        fetch(url2, { method: 'GET', credentials: 'same-origin' })
            .then(function (res) {
                if (!res.ok) return null;
                return res.arrayBuffer();
            })
            .then(function (buffer) {
                if (myToken !== loadToken || !buffer) return;
                let geometry;
                try {
                    geometry = loader.parse(buffer);
                } catch (_e) {
                    return;
                }
                geometry.computeVertexNormals();
                const mesh2 = new THREE.Mesh(geometry, makeStlMeshStandardMaterial(stlMeshTintColor2));
                mesh2.castShadow = groundShadowVisible;
                mesh2.receiveShadow = false;
                mesh2.rotation.x = -0.5 * Math.PI;
                group.add(mesh2);
                clipStlMeshes.push(mesh2);
                updateClipPlanes();
                syncGroundShadowToggleUi();
                refitCamerasToObject(group);
            })
            .catch(function () {});
    }

    function loadStl(url, label) {
        const myToken = ++loadToken;
        setStatus('Cargando…');
        clipStlMeshes = [];
        disposeObject3D(currentRoot);
        currentRoot = null;
        if (renderer.renderLists && typeof renderer.renderLists.dispose === 'function') {
            renderer.renderLists.dispose();
        }

        const loader = new STLLoader();
        loader.load(
            url,
            function (geometry) {
                if (myToken !== loadToken) return;
                geometry.computeVertexNormals();
                const mesh = new THREE.Mesh(geometry, makeStlMeshStandardMaterial(stlMeshTintColor));
                mesh.castShadow = groundShadowVisible;
                mesh.receiveShadow = false;
                /* STL/CAD suele tener la planta en XY y Z como eje del edificio; en Three (Y arriba, frente +Z)
                   hay que bascular -90° en X para que FRONT sea alzado y la planta se vea con TOP. */
                mesh.rotation.x = -0.5 * Math.PI;
                const group = new THREE.Group();
                /* Desing_2: unidad archivo → m → mm escena (×1000 con source-units "m"). Maestro: 1. */
                group.scale.setScalar(stlVertexToSceneScale);
                group.add(mesh);
                clipStlMeshes = [mesh];
                if (clipInputY) clipInputY.value = '1000';
                if (clipInputX) clipInputX.value = '1000';
                currentRoot = group;
                scene.add(group);
                refitCamerasToObject(group);
                setStatus(label ? 'Viendo: ' + label : 'Modelo cargado.');
                tryLoadSecondaryStl(url, group, myToken, loader);
            },
            undefined,
            function () {
                if (myToken !== loadToken) return;
                setStatus('No se pudo cargar el STL.');
            }
        );
    }

    document.querySelectorAll('.master-article-stl-load:not([disabled])').forEach(function (btn) {
        if (!btn) return;
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

    /** Desarrollo: auto-carga STL si `#ma-stl-viewer-shell` lleva data-ma-stl-dev-auto-load="true". Producción sin atributos → sin efecto. */
    (function maStlMaybeLoadDevDefaultStl() {
        const shell = document.getElementById('ma-stl-viewer-shell');
        if (!shell || shell.getAttribute('data-ma-stl-dev-auto-load') !== 'true') return;
        const u = (shell.getAttribute('data-ma-stl-dev-default-stl') || '').trim();
        if (!u) return;
        const lbl = (shell.getAttribute('data-ma-stl-dev-default-label') || '').trim();
        window.setTimeout(function () {
            loadStl(u, lbl || 'Dev STL');
        }, 120);
    })();

    if (maStlDesingV2Viewer) {
        document.dispatchEvent(new CustomEvent('ma-stl-desing2-viewer-ready'));
    }
}

bootMasterArticleDetailsStlViewer();
