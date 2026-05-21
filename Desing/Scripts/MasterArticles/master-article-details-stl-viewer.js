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

/**
 * Solo visor Desing_2 (`data-ma-stl-show-rulers-toggle`): después de aplicar escala STL (vértices en unidad de archivo
 * → metro vía {@link maStlVertexUnitsToMetersScale}) se multiplica por este factor para obtener **coordenadas de escena**.
 * **`MA_STL_SCENE_MM_PER_PHYSICAL_METER` mm de escena ≡ 1 m físico** (p. ej. rejilla cada 500 mm = cada 0,5 m físicos).
 * Los `.stl` no declaran SI de forma fiable → `data-ma-stl-source-units` / `data-ma-stl-unit-to-meters`; muchos están en mm.
 *
 * Pantalla maestro (sin esos `data-*`): **sin** este factor (`group.scale`=1 como antes): unidades arbitrarias archivo.
 */
const MA_STL_SCENE_MM_PER_PHYSICAL_METER = 1000;

/** Reglas (Desing_2): pasos en milímetros de escena. Minores 500 mm (0,5 m); mayores cada 2500 mm (2,5 m). */
const MA_STL_DESING2_RULE_MINOR_MM = 500;
const MA_STL_DESING2_RULE_MAJOR_MM = 2500;

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

/** Consola: distancias pick anclaje reglas Desing_2 (`maStlUpdateInsertionPickProximity`). */
const MA_STL_DEBUG_INSERTION_PICK = false;
/** Umbral pantalla (px) para activar recuadro de inserción junto al punto proyectado. */
const MA_STL_INSERTION_PICK_SCREEN_PX_BASE = 34;
/** Factor umbral pantalla cuando el cursor está sobre malla STL (pick → inserción, no bbox 3D). */
const MA_STL_INSERTION_PICK_MESH_SCREEN_BOOST = 1.75;
/** Relleno snap intersección rejilla 500 mm (modo pick reglas Desing_2). */
const MA_STL_GRID_INTERSECTION_PICK_HIGHLIGHT_COLOR = 0x00e676;
const MA_STL_GRID_INTERSECTION_PICK_HIGHLIGHT_OPACITY = 0.65;
/** Contorno idle en cruce más cercano (antes de “conectar”). */
const MA_STL_GRID_INTERSECTION_PICK_IDLE_COLOR = 0x26c6da;
const MA_STL_GRID_INTERSECTION_PICK_IDLE_OPACITY = 0.92;
/** Umbral pantalla (px) para estado connected en cruce rejilla (más generoso que inserción). */
const MA_STL_GRID_INTERSECTION_PICK_SCREEN_PX_BASE = 52;
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
 * @returns {{ minorMm: number, majorMm: number, lodMult: number }}
 */
function maStlDesing2GridLodCellSizesMm(wpp) {
    let mult = 1;
    if (wpp >= MA_STL_DESING2_GRID_LOD_WPP_TIER3) {
        mult = MA_STL_DESING2_GRID_LOD_MULT_TIER3;
    } else if (wpp >= MA_STL_DESING2_GRID_LOD_WPP_TIER2) {
        mult = MA_STL_DESING2_GRID_LOD_MULT_TIER2;
    }
    return {
        minorMm: MA_STL_DESING2_GRID_MINOR_MM * mult,
        majorMm: MA_STL_DESING2_GRID_MAJOR_MM * mult,
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
 * Extensión reglas+bloque Desing_2 (mm escena): crece con el modelo; tope ~25 m físicos para listados grandes.
 */
function maStlDesing2RulerExtentMm(maxDimLocal) {
    const d = Math.max(maxDimLocal, 1e-9);
    return THREE.MathUtils.clamp(
        Math.max(d * 2.75, MA_STL_DESING2_GRID_MAJOR_MM * 10),
        MA_STL_DESING2_GRID_MAJOR_MM * 8,
        MA_STL_DESING2_RULE_FIXED_EXTENT_MM
    );
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
function maStlSyncInfiniteGridWorkspace(grid, maxDim, desingMmScene, frustumHalfY, aspect, orthoMinZoom) {
    const gMat = grid && grid.material;
    if (!gMat || !gMat.uniforms) return;
    const u = gMat.uniforms;
    const d = Math.max(maxDim, 1e-9);
    const camFitDim = desingMmScene
        ? Math.max(d * 1.22, MA_STL_DESING2_GRID_MAJOR_MM * 4)
        : Math.max(d * 1.18, 1e-6);
    maStlSyncGridPlaneY(grid, desingMmScene, d);
    if (desingMmScene) {
        u.uSize1.value = MA_STL_DESING2_GRID_MINOR_MM;
        u.uSize2.value = MA_STL_DESING2_GRID_MAJOR_MM;
        const minZ =
            orthoMinZoom != null && orthoMinZoom > 0
                ? orthoMinZoom
                : MA_STL_DESING2_MIN_ZOOM_FLOOR;
        const reachMinZoom = maStlOrthoReachMm(frustumHalfY, aspect, minZ);
        u.uDistance.value = Math.max(
            MA_STL_DESING2_GRID_DISTANCE_DESIGN3D,
            camFitDim * 90,
            d * 85,
            MA_STL_DESING2_GRID_MAJOR_MM * 3.5,
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
function maStlFrustumHalfYFromMaxDim(maxDim, desingMmScene) {
    const d = Math.max(maxDim, 1e-9);
    const camFitDim = desingMmScene
        ? Math.max(d * 1.22, MA_STL_DESING2_GRID_MAJOR_MM * 4)
        : Math.max(d * 1.18, 1e-6);
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
 * Lightweight label Sprite.
 * Default: filled text + outline (UCS X/Y/Z). `thinFillOnly`: single `fillText`, thin sans (reglas Desing_2).
 * @param {string} text
 * @param {number} worldScale On-screen footprint in **scene units** (mm en Desing_2; archivo suelto en maestro legacy).
 * @param {{ minPx?: number, maxPx?: number, fontRatio?: number, worldToPixelMult?: number, spriteExpand?: number, strokeRatio?: number, thinFillOnly?: boolean, fontPx?: number, fontWeight?: string|number, fontFamily?: string, fillColor?: string, canvasPad?: number }=} opts
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
    } else {
        sprite.scale.set(worldScale * ws, worldScale * ws, 1);
    }
    return sprite;
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
        fillColor: '#333333',
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
 * Fórmula (bottom-center, NO el centro 3D del AABB):
 *   box = AABB mundo del grupo (incluye hijos)
 *   x = (box.min.x + box.max.x) / 2
 *   y = MA_STL_DESING2_WORKSPACE_FLOOR_Y_MM  (box.min.y === floorY tras apoyo)
 *   z = (box.min.z + box.max.z) / 2
 *
 * El centro 3D del bbox ((min+max)/3 ejes) difiere en Y; proximidad/highlight/reglas usan este punto.
 * Ampliar con más `id` (p. ej. esquina min: box.min.x, floorY, box.min.z + offset).
 *
 * @param {THREE.Group} group
 * @returns {THREE.Vector3}
 */
function maStlGetInsertionPointBottomCenterWorld(group) {
    group.updateMatrixWorld(true);
    const box = new THREE.Box3().setFromObject(group);
    const y = MA_STL_DESING2_WORKSPACE_FLOOR_Y_MM;
    return new THREE.Vector3((box.min.x + box.max.x) * 0.5, y, (box.min.z + box.max.z) * 0.5);
}

/**
 * @type {{ id: string, label: string, getWorldPosition: (group: THREE.Group) => THREE.Vector3 }[]}
 */
const maStlInsertionPointProviders = [
    {
        id: 'primary',
        label: 'Punto de inserción (centro base)',
        getWorldPosition: maStlGetInsertionPointBottomCenterWorld
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

/**
 * Umbral de proximidad (mm) para activar el recuadro: escala con tamaño del modelo y con distancia/zoom de cámara.
 * @param {number} maxDim
 * @param {THREE.Camera} camera
 * @param {THREE.Vector3} insertionWorld
 */
function maStlInsertionPickProximityThresholdMm(maxDim, camera, insertionWorld) {
    const d = Math.max(maxDim, MA_STL_DESING2_GRID_MINOR_MM);
    const base = Math.max(d * 0.12, MA_STL_DESING2_GRID_MINOR_MM * 0.65);
    const camDist = Math.max(camera.position.distanceTo(insertionWorld), 1e-3);
    const distFactor = THREE.MathUtils.clamp(camDist / Math.max(d * 0.75, 1e-3), 0.2, 3.2);
    let zoomFactor = 1;
    if (camera.isOrthographicCamera) {
        zoomFactor = THREE.MathUtils.clamp(1.45 / Math.max(camera.zoom, 0.1), 0.45, 5.5);
    } else if (camera.isPerspectiveCamera) {
        zoomFactor = THREE.MathUtils.clamp(camDist / (d * 0.95), 0.25, 3.5);
    }
    return base * distFactor * zoomFactor;
}

/** Plano suelo workspace (normal +Y). */
const _maStlWorkspaceFloorPickPlane = new THREE.Plane(
    new THREE.Vector3(0, 1, 0),
    -MA_STL_DESING2_WORKSPACE_FLOOR_Y_MM
);
/** NDC desde rect del canvas (Vector2 — perspectiva Desing_2 requiere z implícito en setFromCamera). */
const _maStlFloorPickNdc = new THREE.Vector2();
const _maStlInsertionPickScreenNdc = new THREE.Vector3();

function maStlInsertionPickScreenThresholdPx(camera) {
    if (camera.isOrthographicCamera) {
        return THREE.MathUtils.clamp(
            MA_STL_INSERTION_PICK_SCREEN_PX_BASE / Math.max(camera.zoom, 0.1),
            24,
            80
        );
    }
    return MA_STL_INSERTION_PICK_SCREEN_PX_BASE;
}

/**
 * Umbral mm (XZ) cursor→snap en intersección de rejilla minor (base 500 mm).
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

function maStlGridIntersectionPickProximityThresholdMm(maxDim, camera, snapWorld) {
    const d = Math.max(maxDim, MA_STL_DESING2_GRID_MINOR_MM);
    const base = Math.max(MA_STL_DESING2_GRID_MINOR_MM * 0.58, d * 0.07);
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
 * Snap X/Z en planta a la intersección minor más cercana (500 mm base; no LOD dinámico).
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
 * @returns {{ x: number, y: number, z: number, active: boolean }}
 */
function maStlSnapFloorToGridIntersection(floorHit, proximity) {
    const snap = maStlSnapFloorToGridIntersectionMm(floorHit.x, floorHit.z, MA_STL_DESING2_GRID_MINOR_MM);
    const y = MA_STL_DESING2_WORKSPACE_FLOOR_Y_MM;
    const result = { x: snap.snapX, y: y, z: snap.snapZ, active: false };
    if (!proximity || !proximity.camera || !proximity.canvas) {
        return result;
    }
    _maStlGridSnapProximityWorld.set(snap.snapX, y, snap.snapZ);
    const distXZ = Math.hypot(floorHit.x - snap.snapX, floorHit.z - snap.snapZ);
    const threshMm = maStlGridIntersectionPickProximityThresholdMm(
        proximity.maxDim,
        proximity.camera,
        _maStlGridSnapProximityWorld
    );
    const screenPx = maStlInsertionPointScreenDistancePx(
        _maStlGridSnapProximityWorld,
        proximity.clientX,
        proximity.clientY,
        proximity.camera,
        proximity.canvas
    );
    const screenThreshPx = maStlGridIntersectionPickScreenThresholdPx(proximity.camera);
    const xzActive = distXZ <= Math.max(threshMm, MA_STL_DESING2_GRID_MINOR_MM * 0.52);
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

/** Recuadro en suelo (mm escena), centrado en el origen local del grupo padre. */
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
 */
function maStlSyncDesing2ScreenSpaceOverlay(mat, camera, renderer, desing2OrthoMinZoom, orbitTarget) {
    if (!mat || !mat.uniforms) return;
    const u = mat.uniforms;
    const wpp = maStlWorldMmPerPixel(camera, renderer, orbitTarget);
    const lod = maStlDesing2GridLodCellSizesMm(wpp);
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
    let maStlRulerAnchorPickActive = false;
    const maStlRulerAnchorPickToggleBtn = document.getElementById('ma-stl-ruler-anchor-pick-toggle');
    const maStlRulerAnchorCoordsHud = document.getElementById('ma-stl-ruler-anchor-coords-hud');
    /** @type {{ id: string, label: string, position: THREE.Vector3 }[]} */
    let maStlInsertionPointsCache = [];
    let maStlInsertionPickNearActive = false;
    /** @type {string|null} */
    let maStlInsertionPickNearId = null;
    let maStlGridIntersectionNearActive = false;
    /** @type {{ enabled: boolean }|null} */
    let maStlRulerAnchorPickOrbitLockSnapshot = null;
    const _maStlGridIntersectionSnapMm = new THREE.Vector3(
        0,
        MA_STL_DESING2_WORKSPACE_FLOOR_Y_MM,
        0
    );
    /** @type {{ mesh: THREE.Mesh, color: THREE.Color, emissive: THREE.Color, emissiveIntensity: number }[]} */
    let maStlPickHoverMaterialSnapshots = [];
    const _maStlPickHoverColor = new THREE.Color(0x3d8bfd);

    let currentRoot = null;
    let loadToken = 0;
    /** Última extensión del modelo (para distancia de cámara en vistas del dado). Desing_2: baseline ~12 m en mm escena. */
    let lastMaxDim = maStlRulersGate ? maStlDesing2EmptyBaselineDimMm() : 1;
    /** Half-height of ortho frustum in world units (before camera.zoom). */
    let frustumHalfY = maStlFrustumHalfYFromMaxDim(lastMaxDim, maStlRulersGate);
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

    /* Rejilla: Desing_2 500/2500 mm (0,5 m / 2,5 m); LOD en `onBeforeRender`; maestro ctor compacto. */
    const infiniteGrid = maStlRulersGate
        ? new InfiniteGridHelper(
              MA_STL_DESING2_GRID_MINOR_MM,
              MA_STL_DESING2_GRID_MAJOR_MM,
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
        lastMaxDim,
        maStlRulersGate,
        frustumHalfY,
        lastAspect,
        maStlRulersGate ? MA_STL_DESING2_MIN_ZOOM_FLOOR : desing2OrthoMinZoom
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
    /** Recuadro cyan en intersección rejilla 500 mm (modo pick reglas). */
    const maStlGridIntersectionPickHighlightGroup = new THREE.Group();
    maStlGridIntersectionPickHighlightGroup.renderOrder = 166;
    maStlGridIntersectionPickHighlightGroup.visible = false;
    /** @type {{ idle: THREE.Object3D|null, connected: THREE.Object3D|null }} */
    const maStlGridIntersectionPickMeshes = { idle: null, connected: null };
    /** Wrap ×1000 (Desing_2): geometría reglas en m → mm escena. */
    let maStlRulersSceneWrap = null;
    /** @type {THREE.LineBasicMaterial|null} */
    let maStlOverlayLineMat = null;

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
            maStlRulerAnchorMarkerGroup.visible = maStlUcsRulersManualOn;
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
        maStlRulerAnchorMarkerGroup.position.set(
            maStlRulerAnchorMm.x,
            MA_STL_DESING2_WORKSPACE_FLOOR_Y_MM,
            maStlRulerAnchorMm.z
        );
        maStlRulerAnchorMarkerGroup.add(maStlBuildRulerAnchorFloorMarker());
        maStlRulerAnchorMarkerGroup.visible = maStlRulersGate && maStlUcsRulersManualOn;
    }

    function syncMaStlRulerAnchorPickBtnUi() {
        if (!maStlRulerAnchorPickToggleBtn) return;
        maStlRulerAnchorPickToggleBtn.setAttribute('aria-pressed', maStlRulerAnchorPickActive ? 'true' : 'false');
        maStlRulerAnchorPickToggleBtn.classList.toggle('active', maStlRulerAnchorPickActive);
    }

    function syncMaStlRulerAnchorPickCursor() {
        if (!renderer || !renderer.domElement) return;
        renderer.domElement.style.cursor = maStlRulerAnchorPickActive ? 'crosshair' : '';
    }

    function maStlRefreshInsertionPointsCache() {
        if (currentRoot) {
            currentRoot.updateMatrixWorld(true);
        }
        maStlInsertionPointsCache = currentRoot ? maStlCollectInsertionPointsWorld(currentRoot) : [];
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
     * Hover azul temporal sobre malla STL (solo modo pick anclaje; no altera materiales permanentes).
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

    function maStlUpdateStlPickHoverFromPointer(clientX, clientY) {
        if (!maStlRulerAnchorPickActive || !maStlDesingV2Viewer || !renderer || !currentRoot) {
            maStlClearStlPickHoverHighlight();
            return;
        }
        const canvas = renderer.domElement;
        const cam = activeCamera();
        const rect = canvas.getBoundingClientRect();
        const rw = Math.max(rect.width, 1);
        const rh = Math.max(rect.height, 1);
        _maStlFloorPickNdc.set(
            ((clientX - rect.left) / rw) * 2 - 1,
            -((clientY - rect.top) / rh) * 2 + 1
        );
        cam.updateMatrixWorld(true);
        orbitPivotRaycaster.setFromCamera(_maStlFloorPickNdc, cam);
        const meshHits = orbitPivotRaycaster.intersectObject(currentRoot, true);
        maStlClearStlPickHoverHighlight();
        for (let i = 0; i < meshHits.length; i++) {
            const obj = meshHits[i].object;
            if (obj && obj.isMesh) {
                maStlApplyStlPickHoverHighlight(obj);
                return;
            }
        }
    }

    function maStlClearInsertionPickHighlight() {
        maStlInsertionPickNearActive = false;
        maStlInsertionPickNearId = null;
        maStlInsertionPickHighlightGroup.visible = false;
        maStlStripOverlayMeshes(maStlInsertionPickHighlightGroup);
    }

    function maStlSyncRulerAnchorCoordsHud() {
        if (!maStlRulerAnchorCoordsHud) return;
        const show = maStlRulerAnchorPickActive && maStlGridIntersectionNearActive;
        if (!show) {
            maStlRulerAnchorCoordsHud.classList.add('d-none');
            maStlRulerAnchorCoordsHud.textContent = '';
            return;
        }
        const tpl =
            maStlRulerAnchorCoordsHud.getAttribute('data-ma-stl-ruler-anchor-coords-template') || '';
        maStlRulerAnchorCoordsHud.textContent = maStlFormatRulerAnchorGridIntersectionToast(
            tpl,
            _maStlGridIntersectionSnapMm.x,
            _maStlGridIntersectionSnapMm.z
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

    function maStlLockOrbitForRulerAnchorPick() {
        if (!controls || maStlRulerAnchorPickOrbitLockSnapshot) return;
        maStlRulerAnchorPickOrbitLockSnapshot = { enabled: controls.enabled };
        controls.enabled = false;
    }

    function maStlUnlockOrbitForRulerAnchorPick() {
        if (!controls || !maStlRulerAnchorPickOrbitLockSnapshot) return;
        controls.enabled = maStlRulerAnchorPickOrbitLockSnapshot.enabled;
        maStlRulerAnchorPickOrbitLockSnapshot = null;
    }

    function maStlEnsureGridIntersectionPickHighlightMeshes() {
        if (maStlGridIntersectionPickMeshes.connected) {
            if (maStlDesingV2Viewer && maStlGridIntersectionPickHighlightGroup.parent !== scene) {
                scene.add(maStlGridIntersectionPickHighlightGroup);
            }
            return;
        }
        const half = MA_STL_GRID_INTERSECTION_PICK_CELL_MM * 0.5;
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
    }

    /** Solo click: anclaje + reglas; pivote = target con pan compensado (vista estable). */
    function maStlApplyRulerAnchorOrbitTargetOnly() {
        if (!controls) return;
        maStlApplyRulerAnchorOrbitPivotPreserveView();
    }

    function maStlSetRulerAnchorFromGridSnap(snap) {
        maStlRulerAnchorMm.set(snap.x, snap.y, snap.z);
        rebuildMaStlUcsOverlayDecor(lastMaxDim);
        maStlApplyRulerAnchorOrbitTargetOnly();
    }

    function maStlSetRulerAnchorFromInsertionPoint(worldPos) {
        maStlRulerAnchorMm.set(worldPos.x, MA_STL_DESING2_WORKSPACE_FLOOR_Y_MM, worldPos.z);
        rebuildMaStlUcsOverlayDecor(lastMaxDim);
        maStlApplyRulerAnchorOrbitTargetOnly();
    }

    /** @deprecated usar maStlSetRulerAnchorFromInsertionPoint */
    function maStlSetRulerAnchorFromPickPoint(hitPoint) {
        maStlSetRulerAnchorFromInsertionPoint(hitPoint);
    }

    function maStlExitRulerAnchorPickMode() {
        maStlRulerAnchorPickActive = false;
        maStlUnlockOrbitForRulerAnchorPick();
        maStlClearInsertionPickHighlight();
        maStlClearGridIntersectionPickHighlight();
        maStlClearStlPickHoverHighlight();
        syncMaStlRulerAnchorPickBtnUi();
        syncMaStlRulerAnchorPickCursor();
    }

    const _maStlInsertionFloorProbe = new THREE.Vector3();

    /**
     * Hover rejilla (pointermove): highlight + HUD únicamente — sin cámara, target, controls ni reglas.
     * @returns {boolean} true si hay snap en planta (idle o connected)
     */
    function maStlUpdateGridIntersectionPickHover(clientX, clientY) {
        if (!maStlRulerAnchorPickActive || !maStlDesingV2Viewer || !renderer) {
            maStlClearGridIntersectionPickHighlight();
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
            return false;
        }
        const gridSnap = maStlSnapFloorToGridIntersection(
            { x: _maStlInsertionFloorProbe.x, z: _maStlInsertionFloorProbe.z },
            { clientX: clientX, clientY: clientY, camera: cam, canvas: canvas, maxDim: lastMaxDim }
        );
        maStlSetGridIntersectionPickHighlight(gridSnap.active ? 'connected' : 'idle', gridSnap);
        return true;
    }

    function maStlUpdateInsertionPickProximity(clientX, clientY) {
        if (!maStlRulerAnchorPickActive || !maStlDesingV2Viewer || !renderer) {
            maStlClearInsertionPickHighlight();
            maStlClearGridIntersectionPickHighlight();
            maStlClearStlPickHoverHighlight();
            return;
        }
        if (maStlUpdateGridIntersectionPickHover(clientX, clientY)) {
            if (maStlGridIntersectionNearActive) {
                maStlClearInsertionPickHighlight();
                maStlClearStlPickHoverHighlight();
                return;
            }
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
        }
        if (!currentRoot) {
            maStlClearInsertionPickHighlight();
            maStlClearStlPickHoverHighlight();
            return;
        }
        maStlUpdateStlPickHoverFromPointer(clientX, clientY);
        const points =
            maStlInsertionPointsCache.length > 0
                ? maStlInsertionPointsCache
                : maStlCollectInsertionPointsWorld(currentRoot);
        if (!points.length) {
            maStlClearInsertionPickHighlight();
            return;
        }
        orbitPivotRaycaster.setFromCamera(_maStlFloorPickNdc, cam);
        const meshHits = orbitPivotRaycaster.intersectObject(currentRoot, true);
        const meshUnderCursor = meshHits.length > 0;
        if (!floorHit && !meshUnderCursor) {
            maStlClearInsertionPickHighlight();
            return;
        }
        const probeX = floorHit ? _maStlInsertionFloorProbe.x : null;
        const probeZ = floorHit ? _maStlInsertionFloorProbe.z : null;
        const screenThreshPx = maStlInsertionPickScreenThresholdPx(cam);
        const meshScreenThreshPx = screenThreshPx * MA_STL_INSERTION_PICK_MESH_SCREEN_BOOST;
        let best = null;
        let bestScore = Infinity;
        points.forEach(function (pt) {
            const screenPx = maStlInsertionPointScreenDistancePx(pt.position, clientX, clientY, cam, canvas);
            const nearScreen = screenPx < screenThreshPx;
            const nearScreenMesh = meshUnderCursor && screenPx < meshScreenThreshPx;
            let distXZ = Infinity;
            let nearMm = false;
            if (floorHit) {
                distXZ = Math.hypot(probeX - pt.position.x, probeZ - pt.position.z);
                const threshMm = maStlInsertionPickProximityThresholdMm(lastMaxDim, cam, pt.position);
                nearMm = distXZ < threshMm;
            }
            if (MA_STL_DEBUG_INSERTION_PICK && (nearMm || nearScreen || nearScreenMesh)) {
                console.debug('[maStl insertion pick]', pt.id, {
                    distXZ: Number.isFinite(distXZ) ? distXZ.toFixed(1) : 'n/a',
                    screenPx: screenPx.toFixed(1),
                    meshUnderCursor: meshUnderCursor,
                    screenThreshPx: screenThreshPx.toFixed(1)
                });
            }
            if (!nearMm && !nearScreen && !nearScreenMesh) return;
            const score = nearMm ? distXZ : screenPx * 12;
            if (score < bestScore) {
                bestScore = score;
                best = pt;
            }
        });
        if (best) {
            maStlClearGridIntersectionPickHighlight();
            maStlInsertionPickNearActive = true;
            maStlInsertionPickNearId = best.id;
            maStlSetInsertionPickHighlight(true, best.position);
            return;
        }
        maStlClearInsertionPickHighlight();
    }

    function onCanvasPointerMoveRulerAnchorPick(ev) {
        if (!maStlRulerAnchorPickActive) return;
        maStlUpdateGridIntersectionPickHover(ev.clientX, ev.clientY);
    }

    function onCanvasPointerLeaveInsertionPick() {
        if (!maStlRulerAnchorPickActive) return;
        maStlClearStlPickHoverHighlight();
        maStlClearInsertionPickHighlight();
        maStlClearGridIntersectionPickHighlight();
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
            ? maStlDesing2RulerExtentMm(maxDimLocal)
            : maStlRulerExtentFromMaxDimMm(maxDimLocal);
        maStlOverlayLineMat = maStlMakeOverlayLineMat();
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
                MA_STL_DESING2_RULE_MINOR_MM / s,
                MA_STL_DESING2_RULE_MAJOR_MM / s,
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
        }
        maStlRebuildRulerAnchorMarker();
        syncMaStlUcsOverlayVisibility();
    }

    scene.add(maStlUcsAxesGroup);
    scene.add(maStlXyzAxesGroup);
    scene.add(maStlRulersGroup);
    if (maStlDesingV2Viewer) {
        scene.add(maStlRulerAnchorMarkerGroup);
        scene.add(maStlInsertionPickHighlightGroup);
        scene.add(maStlGridIntersectionPickHighlightGroup);
    }
    rebuildMaStlUcsOverlayDecor(lastMaxDim);

    if (maStlRulerAnchorPickToggleBtn && maStlDesingV2Viewer) {
        maStlRulerAnchorPickToggleBtn.addEventListener('click', function () {
            maStlRulerAnchorPickActive = !maStlRulerAnchorPickActive;
            if (maStlRulerAnchorPickActive) {
                maStlRefreshInsertionPointsCache();
                maStlLockOrbitForRulerAnchorPick();
                maStlEnsureGridIntersectionPickHighlightMeshes();
                const modeToast =
                    maStlRulerAnchorPickToggleBtn.getAttribute(
                        'data-ma-stl-ruler-anchor-pick-mode-toast'
                    ) || '';
                if (modeToast) {
                    maStlDesing2ShowSaveViewToast(modeToast);
                }
            } else {
                maStlUnlockOrbitForRulerAnchorPick();
                maStlClearInsertionPickHighlight();
                maStlClearGridIntersectionPickHighlight();
                maStlClearStlPickHoverHighlight();
            }
            syncMaStlRulerAnchorPickBtnUi();
            syncMaStlRulerAnchorPickCursor();
        });
    }

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
            const tgt = controls ? controls.target : new THREE.Vector3();
            maStlSyncGridPlaneY(infiniteGrid, true, lastMaxDim);
            if (camera && camera.isPerspectiveCamera) {
                maStlSyncDesing2GridDistancePerspective(infiniteGrid, frustumHalfY, lastAspect, camera, tgt);
            }
            maStlSyncDesing2ScreenSpaceOverlay(mat, camera, renderer, desing2OrthoMinZoom, tgt);
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
            maStlResetOrbitTargetToRulerAnchor();
            if (maStlRulersGate && camera.isOrthographicCamera) {
                maStlRefreshDesing2OrthoMinZoom();
                controls.minZoom = MA_STL_DESING2_MIN_ZOOM_FLOOR;
                clampDesing2OrthoZoom(camera);
            }
            controls.update();
            return;
        }
        if (controls) {
            controls.dispose();
        }
        controls = new OrbitControls(camera, renderer.domElement);
        controls.enableDamping = maStlRulersGate ? false : true;
        controls.dampingFactor = 0.06;
        maStlResetOrbitTargetToRulerAnchor();
        if (maStlRulersGate && camera.isOrthographicCamera) {
            maStlRefreshDesing2OrthoMinZoom();
            controls.minZoom = MA_STL_DESING2_MIN_ZOOM_FLOOR;
            clampDesing2OrthoZoom(camera);
        }
        controls.update();
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
     * En Desing_2 el pivote de OrbitControls DEBE permanecer en el anclaje de
     * reglas (`maStlRulerAnchorMm`), no raycastear el STL al rotar.
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
     *   • onCanvasPointerDownSetOrbitPivot — early return SIN raycast al rotar
     *   • Cubo de vistas — orden: applyDirectionToOrthoCam (up Y-up, lookAt anclaje)
     *       → bindControls (misma cámara: reutilizar; si no: OrbitControls nuevo)
     *       → maStlFinalizeViewCubePreset (target + update + saveState)
     *   • bindControls — reset al crear OrbitControls (tras mover cámara en cubo)
     *   • maStlApplyDesing2ViewerStateFromCookie — restaurar rulerAnchor; NO aplicar state.target
     *       (órbita = anclaje; cookie target legado se ignora tras cubo o pick)
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
    }

    /** @deprecated alias interno; usar maStlResetOrbitTargetToRulerAnchor. */
    function maStlResetOrbitTargetToSceneCenter() {
        maStlResetOrbitTargetToRulerAnchor();
    }

    /** Desing_2: shell `data-ma-stl-show-rulers-toggle="true"` → pivote fijo, sin raycast. */
    function maStlUsesFixedOrbitPivotAtOrigin() {
        return maStlDesingV2Viewer;
    }

    /** Capture pointerdown antes de OrbitControls; Desing_2 resetea target y sale sin raycast. */
    function onCanvasPointerDownSetOrbitPivot(ev) {
        if (maStlRulerAnchorPickActive) return;
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
            maStlResetOrbitTargetToRulerAnchor();
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
     * Modo pick anclaje reglas: intersección rejilla → inserción → planta libre (X/Z clic, Y=suelo).
     */
    function onCanvasPointerDownRulerAnchorPick(ev) {
        if (!maStlRulerAnchorPickActive || !maStlDesingV2Viewer) return;
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
                  }
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
            maStlExitRulerAnchorPickMode();
            return;
        }
        maStlUpdateInsertionPickProximity(ev.clientX, ev.clientY);
        if (maStlInsertionPickNearActive && maStlInsertionPickNearId) {
            const chosen = maStlInsertionPointsCache.find(function (pt) {
                return pt.id === maStlInsertionPickNearId;
            });
            if (chosen) {
                maStlSetRulerAnchorFromInsertionPoint(chosen.position);
                if (maStlRulerAnchorPickToggleBtn) {
                    maStlDesing2ShowSaveViewToast(
                        maStlRulerAnchorPickToggleBtn.getAttribute('data-ma-stl-ruler-anchor-pick-toast') ||
                            ''
                    );
                }
                maStlExitRulerAnchorPickMode();
            }
            return;
        }
        if (!maStlClientRayToWorkspaceFloor(
            ev.clientX,
            ev.clientY,
            canvas,
            cam,
            orbitPivotNdc,
            orbitPivotRaycaster,
            _maStlInsertionFloorProbe
        )) {
            return;
        }
        maStlSetRulerAnchorFromInsertionPoint(_maStlInsertionFloorProbe);
        if (maStlRulerAnchorPickToggleBtn) {
            maStlDesing2ShowSaveViewToast(
                maStlRulerAnchorPickToggleBtn.getAttribute('data-ma-stl-ruler-anchor-pick-floor-toast') ||
                    'Anclaje en planta'
            );
        }
        maStlExitRulerAnchorPickMode();
    }

    renderer.domElement.addEventListener('pointerdown', onCanvasPointerDownRulerAnchorPick, true);
    renderer.domElement.addEventListener('pointermove', onCanvasPointerMoveRulerAnchorPick);
    renderer.domElement.addEventListener('pointerleave', onCanvasPointerLeaveInsertionPick);

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
        maStlSyncShadowGroundMaterialVisual();
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
            toggles: toggles
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
            if (state.rulerAnchor && typeof state.rulerAnchor === 'object') {
                const ra = state.rulerAnchor;
                if (Number.isFinite(ra.x) && Number.isFinite(ra.z)) {
                    maStlRulerAnchorMm.set(
                        ra.x,
                        Number.isFinite(ra.y) ? ra.y : MA_STL_DESING2_WORKSPACE_FLOOR_Y_MM,
                        ra.z
                    );
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
     * @param {HTMLButtonElement} btn
     */
    function maStlDesing2FlashSaveViewFeedback(btn) {
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
    if (maStlSaveViewerStateBtn && maStlDesingV2Viewer && maStlSaveViewerStateBtn instanceof HTMLButtonElement) {
        maStlSaveViewerStateBtn.addEventListener('click', function () {
            if (maStlDesing2SaveViewerStateToCookie()) {
                maStlDesing2FlashSaveViewFeedback(maStlSaveViewerStateBtn);
                maStlDesing2ShowSaveViewToast(
                    maStlSaveViewerStateBtn.getAttribute('data-ma-stl-save-view-toast') || ''
                );
            }
        });
    }

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
        if (controls && !maStlRulerAnchorPickActive) {
            controls.update();
        }
        setViewCubeCssFromCamera(orthoCubeEl, cameraOrtho);
        setViewCubeCssFromCamera(isoCubeEl, cameraIso);
        const camMain = activeCamera();
        renderer.render(scene, camMain);
        if (compassDialEl && controls) {
            const deg = maStlSvgCompassDialRotationDeg(camMain, controls.target);
            compassDialEl.setAttribute(
                'transform',
                'translate(100 100) rotate(' + deg + ') translate(-100 -100)'
            );
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
                Math.max(lastMaxDim, MA_STL_DESING2_GRID_MAJOR_MM * 8),
                true,
                frustumHalfY,
                lastAspect,
                MA_STL_DESING2_MIN_ZOOM_FLOOR
            );
        }
        renderer.setPixelRatio(Math.min(window.devicePixelRatio || 1, 2));
        renderer.setSize(nw, nh, false);
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
            ? Math.max(modelDim, MA_STL_DESING2_GRID_MAJOR_MM * 8)
            : modelDim;
        frustumHalfY = maStlFrustumHalfYFromMaxDim(overlayDim, maStlRulersGate);
        maStlRefreshDesing2OrthoMinZoom();
        const hostSz = readHostSizeCssPx(400, 380);
        lastAspect = hostSz.nw / Math.max(hostSz.nh, 1);
        maStlSyncInfiniteGridWorkspace(
            infiniteGrid,
            overlayDim,
            maStlRulersGate,
            frustumHalfY,
            lastAspect,
            maStlRulersGate ? MA_STL_DESING2_MIN_ZOOM_FLOOR : desing2OrthoMinZoom
        );
        if (!maStlRulersGate) {
            const gMat = infiniteGrid.material;
            if (gMat && gMat.uniforms && gMat.uniforms.uPlaneY) {
                gMat.uniforms.uPlaneY.value = box.min.y - Math.max(modelDim * 0.008, 1e-6);
            }
        }
        const camFitDim = maStlRulersGate
            ? Math.max(overlayDim * 1.22, MA_STL_DESING2_GRID_MAJOR_MM * 4)
            : Math.max(modelDim * 1.18, 1e-6);
        if (maStlRulersGate) {
            mainDirLight.target.position.set(0, 0, 0);
            mainDirLight.target.updateMatrixWorld();
            const shadowCam = mainDirLight.shadow.camera;
            const s = Math.max(overlayDim * 0.5, MA_STL_DESING2_GRID_MAJOR_MM * 2);
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
        if (maStlDesingV2Viewer) {
            maStlRefreshInsertionPointsCache();
            if (maStlRulerAnchorPickActive) {
                maStlClearInsertionPickHighlight();
            }
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
