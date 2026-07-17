import * as THREE from 'three';
import { RoomEnvironment } from '../Design/jsm/environments/RoomEnvironment.js';
import { EffectComposer } from '../Design/jsm/postprocessing/EffectComposer.js';
import { RenderPass } from '../Design/jsm/postprocessing/RenderPass.js';
import { SSAOPass } from '../Design/jsm/postprocessing/SSAOPass.js';

/**
 * Luz tipo sol exterior + sombra en rejilla (sin “modo estudio” que blanquea el suelo).
 * Referencia: Design-3d `createLight` — ambient 0.5 + directional en (2.5,2,2) con sombras.
 */
export const MA_STL_FORMWORK_ENV_DEFAULTS = Object.freeze({
    sunAzimuthDeg: 138,
    sunElevationDeg: 41,
    sunIntensity: 1.85,
    sunColor: 0xffe8c0,
    ambientIntensity: 0.28,
    hemisphereSkyColor: 0xdce8ff,
    hemisphereGroundColor: 0xe4e4e4,
    hemisphereIntensity: 0.14,
    fillIntensity: 0.09,
    fillColor: 0xeef2ff,
    rimIntensity: 0,
    rimColor: 0xffffff,
    iblIntensity: 0,
    exposure: 1.02,
    fogEnabled: false,
    fogStrength: 0,
    shadowsEnabled: true,
    shadowMapSize: 2048,
    shadowOpacity: 0.35,
    ssaoEnabled: false,
});

const _sunPos = new THREE.Vector3();
const _shadowBox = new THREE.Box3();
const _shadowCenter = new THREE.Vector3();
const _shadowSize = new THREE.Vector3();
const _shadowExpand = new THREE.Vector3();
const _sunDir = new THREE.Vector3();

/**
 * @param {number} azimuthDeg 0 = +X, 90 = +Z
 * @param {number} elevationDeg sobre horizonte
 * @param {number} distanceMm
 */
export function maStlFormworkEnvSunOffsetMm(azimuthDeg, elevationDeg, distanceMm) {
    const az = THREE.MathUtils.degToRad(azimuthDeg);
    const el = THREE.MathUtils.degToRad(elevationDeg);
    const dist = Math.max(distanceMm, 1);
    const y = Math.sin(el) * dist;
    const h = Math.cos(el) * dist;
    _sunPos.set(Math.cos(az) * h, y, Math.sin(az) * h);
    return _sunPos;
}

function mergeFormworkEnvSettings(partial) {
    const out = Object.assign({}, MA_STL_FORMWORK_ENV_DEFAULTS);
    if (!partial || typeof partial !== 'object') return out;
    for (const key of Object.keys(MA_STL_FORMWORK_ENV_DEFAULTS)) {
        if (partial[key] != null) out[key] = partial[key];
    }
    out.sunAzimuthDeg = THREE.MathUtils.clamp(Number(out.sunAzimuthDeg) || 0, 0, 360);
    out.sunElevationDeg = THREE.MathUtils.clamp(Number(out.sunElevationDeg) || 0, 5, 85);
    out.sunIntensity = THREE.MathUtils.clamp(Number(out.sunIntensity) || 0, 0.05, 3);
    out.ambientIntensity = THREE.MathUtils.clamp(Number(out.ambientIntensity) || 0, 0, 1);
    out.hemisphereIntensity = THREE.MathUtils.clamp(Number(out.hemisphereIntensity) || 0, 0, 1.5);
    out.fillIntensity = THREE.MathUtils.clamp(Number(out.fillIntensity) || 0, 0, 1.5);
    out.rimIntensity = THREE.MathUtils.clamp(Number(out.rimIntensity) || 0, 0, 1);
    out.iblIntensity = THREE.MathUtils.clamp(Number(out.iblIntensity) || 0, 0, 2);
    out.exposure = THREE.MathUtils.clamp(Number(out.exposure) || 1, 0.5, 2.5);
    out.fogStrength = THREE.MathUtils.clamp(Number(out.fogStrength) || 0, 0, 1);
    out.shadowOpacity = THREE.MathUtils.clamp(Number(out.shadowOpacity) || 0, 0.05, 0.85);
    const sms = Number(out.shadowMapSize);
    out.shadowMapSize = sms === 512 || sms === 1024 || sms === 2048 ? sms : 2048;
    out.shadowsEnabled = out.shadowsEnabled !== false;
    out.fogEnabled = out.fogEnabled === true;
    out.ssaoEnabled = out.ssaoEnabled === true;
    return out;
}

function configureKeyLightShadow(keyLight, settings, mm) {
    keyLight.castShadow = settings.shadowsEnabled;
    keyLight.shadow.mapSize.set(settings.shadowMapSize, settings.shadowMapSize);
    /* Desing_2 en mm (×1000): escalar bias como en Design pero en unidades mm. */
    const unitScale = Math.max(mm, 1);
    keyLight.shadow.bias = -0.00015 * unitScale;
    keyLight.shadow.normalBias = 0.008 * unitScale;
    keyLight.shadow.radius = 3;
    keyLight.shadow.camera.near = 0.25 * mm;
    keyLight.shadow.camera.far = 200 * mm;
    const half = 40 * mm;
    const cam = keyLight.shadow.camera;
    cam.left = -half;
    cam.right = half;
    cam.top = half;
    cam.bottom = -half;
    cam.updateProjectionMatrix();
}

function createShadowCatcherMaterial(opacity) {
    const mat = new THREE.ShadowMaterial({
        color: 0x000000,
        opacity: opacity,
    });
    mat.fog = false;
    mat.transparent = true;
    mat.depthWrite = false;
    return mat;
}

function maStlFormworkEnvSyncIbl(scene, renderer, rig, settings) {
    if (!scene || !renderer || !rig) return;
    const wantIbl = settings.iblIntensity > 0.04;
    if (!wantIbl) {
        scene.environment = null;
        return;
    }
    if (!rig.envRT) {
        const pmrem = new THREE.PMREMGenerator(renderer);
        pmrem.compileEquirectangularShader();
        const roomEnv = new RoomEnvironment();
        rig.envRT = pmrem.fromScene(roomEnv, 0.04);
        rig.pmrem = pmrem;
        roomEnv.dispose();
    }
    scene.environment = rig.envRT.texture;
    maStlFormworkEnvApplyIblToScene(scene, settings.iblIntensity);
}

/**
 * @param {THREE.Scene} scene
 * @param {THREE.WebGLRenderer} renderer
 * @param {{ mmPerMeter?: number, settings?: object }} [opts]
 */
export function maStlFormworkEnvCreateRig(scene, renderer, opts) {
    const options = opts && typeof opts === 'object' ? opts : {};
    const mm = options.mmPerMeter != null && options.mmPerMeter > 0 ? options.mmPerMeter : 1000;
    const settings = mergeFormworkEnvSettings(options.settings);

    const ambientLight = new THREE.AmbientLight(0xffffff, settings.ambientIntensity);
    scene.add(ambientLight);

    const hemiLight = new THREE.HemisphereLight(
        settings.hemisphereSkyColor,
        settings.hemisphereGroundColor,
        settings.hemisphereIntensity
    );
    scene.add(hemiLight);

    const keyLight = new THREE.DirectionalLight(settings.sunColor, settings.sunIntensity);
    keyLight.name = 'maStlFormworkSunLight';
    configureKeyLightShadow(keyLight, settings, mm);
    const sunDist = 80 * mm;
    const sunOff = maStlFormworkEnvSunOffsetMm(settings.sunAzimuthDeg, settings.sunElevationDeg, sunDist);
    keyLight.position.copy(sunOff);
    scene.add(keyLight);
    scene.add(keyLight.target);
    keyLight.target.position.set(0, 0, 0);

    const fillLight = new THREE.DirectionalLight(settings.fillColor, settings.fillIntensity);
    fillLight.position.set(-0.45 * sunDist, 0.35 * sunDist, -0.55 * sunDist);
    fillLight.castShadow = false;
    scene.add(fillLight);
    scene.add(fillLight.target);
    fillLight.target.position.set(0, 0, 0);

    const rimLight = new THREE.DirectionalLight(settings.rimColor, settings.rimIntensity);
    rimLight.castShadow = false;
    rimLight.position.set(0.35 * sunDist, 0.22 * sunDist, 0.62 * sunDist);
    scene.add(rimLight);
    scene.add(rimLight.target);
    rimLight.target.position.set(0, 0, 0);

    const shadowCatcherMat = createShadowCatcherMaterial(settings.shadowOpacity);
    const shadowCatcher = new THREE.Mesh(new THREE.PlaneGeometry(1, 1), shadowCatcherMat);
    shadowCatcher.name = 'maStlFormworkShadowCatcher';
    shadowCatcher.rotation.x = -0.5 * Math.PI;
    shadowCatcher.position.y = 0.5;
    shadowCatcher.receiveShadow = true;
    shadowCatcher.renderOrder = -5;
    shadowCatcher.visible = settings.shadowsEnabled;
    scene.add(shadowCatcher);

    const rig = {
        settings: settings,
        ambientLight: ambientLight,
        hemiLight: hemiLight,
        keyLight: keyLight,
        fillLight: fillLight,
        rimLight: rimLight,
        shadowCatcher: shadowCatcher,
        shadowCatcherMat: shadowCatcherMat,
        pmrem: null,
        envRT: null,
        ssaoComposer: null,
        ssaoPass: null,
        sceneExtentMm: 25000,
        mmPerMeter: mm,
    };

    maStlFormworkEnvSyncIbl(scene, renderer, rig, settings);
    renderer.toneMappingExposure = settings.exposure;
    scene.fog = null;
    return rig;
}

/**
 * Solo activa el captor de sombras sobre la rejilla; no toca fondo ni rejilla.
 * @param {THREE.Scene} scene
 * @param {ReturnType<typeof maStlFormworkEnvCreateRig>|null} rig
 * @param {boolean} shadowFloorActive
 */
export function maStlFormworkEnvApplyCatalogLook(scene, rig, shadowFloorActive) {
    if (!scene) return;
    scene.fog = null;
    if (!rig) return;
    if (rig.shadowCatcherMat) {
        rig.shadowCatcherMat.opacity = rig.settings.shadowOpacity;
        rig.shadowCatcherMat.needsUpdate = true;
    }
    /* Visibilidad del suelo: la controla el visor (shadowGroundPlane en Desing_2). */
    void shadowFloorActive;
}

/**
 * @param {ReturnType<typeof maStlFormworkEnvCreateRig>} rig
 * @param {object} partial
 * @param {THREE.Scene} scene
 * @param {THREE.WebGLRenderer} renderer
 */
export function maStlFormworkEnvApplySettings(rig, partial, scene, renderer) {
    if (!rig) return mergeFormworkEnvSettings(partial);
    const settings = mergeFormworkEnvSettings(Object.assign({}, rig.settings, partial || {}));
    rig.settings = settings;

    rig.ambientLight.intensity = settings.ambientIntensity;
    rig.hemiLight.color.setHex(settings.hemisphereSkyColor);
    rig.hemiLight.groundColor.setHex(settings.hemisphereGroundColor);
    rig.hemiLight.intensity = settings.hemisphereIntensity;

    rig.keyLight.color.setHex(settings.sunColor);
    rig.keyLight.intensity = settings.sunIntensity;
    configureKeyLightShadow(rig.keyLight, settings, rig.mmPerMeter);
    const sunDist = 80 * rig.mmPerMeter;
    const sunOff = maStlFormworkEnvSunOffsetMm(settings.sunAzimuthDeg, settings.sunElevationDeg, sunDist);
    rig.keyLight.position.copy(sunOff);

    rig.fillLight.color.setHex(settings.fillColor);
    rig.fillLight.intensity = settings.fillIntensity;
    rig.rimLight.color.setHex(settings.rimColor);
    rig.rimLight.intensity = settings.rimIntensity;

    maStlFormworkEnvSyncIbl(scene, renderer, rig, settings);
    renderer.toneMappingExposure = settings.exposure;
    if (rig.shadowCatcherMat) {
        rig.shadowCatcherMat.opacity = settings.shadowOpacity;
        rig.shadowCatcherMat.needsUpdate = true;
    }

    if (settings.shadowsEnabled) {
        scene.fog = null;
    } else {
        maStlFormworkEnvSyncFog(scene, settings, rig.sceneExtentMm);
    }
    return settings;
}

/**
 * @param {THREE.Scene} scene
 * @param {object} settings
 * @param {number} extentMm
 * @param {number} [horizonColorHex]
 */
export function maStlFormworkEnvSyncFog(scene, settings, extentMm, horizonColorHex) {
    if (!scene) return;
    const s = mergeFormworkEnvSettings(settings);
    if (s.shadowsEnabled) {
        scene.fog = null;
        return;
    }
    const ext = Math.max(extentMm || 25000, 5000);
    if (!s.fogEnabled || s.fogStrength <= 0.01) {
        scene.fog = null;
        return;
    }
    const fogColor = horizonColorHex != null ? horizonColorHex : 0xf2f4f8;
    const near = ext * (0.55 + (1 - s.fogStrength) * 0.85);
    const far = ext * (1.8 + (1 - s.fogStrength) * 2.2);
    if (!scene.fog || !(scene.fog instanceof THREE.Fog)) {
        scene.fog = new THREE.Fog(fogColor, near, far);
    } else {
        scene.fog.color.setHex(fogColor);
        scene.fog.near = near;
        scene.fog.far = far;
    }
}

export function maStlFormworkEnvApplyIblToScene(scene, iblIntensity) {
    if (!scene || !scene.environment) return;
    const scale = (Number(iblIntensity) || 0) / 0.35;
    scene.traverse(function (obj) {
        if (!obj || !obj.isMesh || !obj.material) return;
        const mats = Array.isArray(obj.material) ? obj.material : [obj.material];
        for (let i = 0; i < mats.length; i++) {
            const m = mats[i];
            if (!m || m.envMapIntensity == null) continue;
            m.envMapIntensity = 0.35 * scale;
            m.needsUpdate = true;
        }
    });
}

/** @param {ReturnType<typeof maStlFormworkEnvCreateRig>} rig @param {number} spanMm */
export function maStlFormworkEnvResizeSiteFloor(rig, spanMm) {
    if (!rig) return;
    const span = Math.max(spanMm * 1.1, 8000);
    rig.sceneExtentMm = span;
    if (rig.shadowCatcher) {
        rig.shadowCatcher.scale.set(span, span, 1);
    }
}

/**
 * @param {THREE.DirectionalLight} keyLight
 * @param {THREE.Object3D[]} rootGroups
 * @param {THREE.Vector3} [fallbackCenter]
 * @param {ReturnType<typeof maStlFormworkEnvCreateRig>|null} [rig]
 * @param {THREE.Mesh|null} [floorReceiver] Plano ShadowMaterial bajo paneles (Desing_2).
 */
export function maStlFormworkEnvFitShadowCamera(keyLight, rootGroups, fallbackCenter, rig, floorReceiver) {
    if (!keyLight || !keyLight.shadow || !keyLight.shadow.camera) return;
    _shadowBox.makeEmpty();
    const groups = Array.isArray(rootGroups) ? rootGroups : [];
    for (let i = 0; i < groups.length; i++) {
        const g = groups[i];
        if (g && g.visible) _shadowBox.expandByObject(g);
    }
    let half = 20000;
    if (_shadowBox.isEmpty()) {
        if (fallbackCenter) {
            _shadowCenter.copy(fallbackCenter);
        } else {
            return;
        }
    } else {
        _shadowBox.getCenter(_shadowCenter);
        _shadowBox.getSize(_shadowSize);
        /* Ampliar en planta: la sombra en Y=0 queda fuera del AABB del panel alto. */
        const panelHeight = Math.max(_shadowSize.y, 500);
        _sunDir.copy(keyLight.position);
        if (keyLight.target && keyLight.target.position) {
            _sunDir.sub(keyLight.target.position);
        }
        if (_sunDir.lengthSq() < 1e-12) {
            _sunDir.set(1, 1.2, 0.8);
        }
        _sunDir.normalize();
        const sunElev = Math.max(Math.abs(_sunDir.y), 0.12);
        const groundReach = panelHeight / sunElev;
        _shadowExpand.set(groundReach, 0, groundReach);
        _shadowBox.expandByVector(_shadowExpand);
        _shadowBox.getSize(_shadowSize);
        half = Math.max(_shadowSize.x, _shadowSize.z, 2500, panelHeight * 0.35) * 0.72;
    }
    const cam = keyLight.shadow.camera;
    cam.left = -half;
    cam.right = half;
    cam.top = half;
    cam.bottom = -half;
    cam.near = Math.max(half * 0.0008, 1);
    cam.far = Math.max(half * 8, 45000);
    cam.updateProjectionMatrix();
    keyLight.target.position.copy(_shadowCenter);
    keyLight.target.updateMatrixWorld();

    const span = Math.max(half * 2.4, (rig && rig.sceneExtentMm) || 12000, 12000);
    if (floorReceiver) {
        floorReceiver.position.set(_shadowCenter.x, 0.25, _shadowCenter.z);
        floorReceiver.scale.set(span, span, 1);
    }
    if (rig && rig.shadowCatcher) {
        rig.shadowCatcher.position.set(_shadowCenter.x, 0.5, _shadowCenter.z);
        rig.shadowCatcher.scale.set(span, span, 1);
    }
}

/**
 * Materiales ATK-60: Lambert como Design View — amarillo vivo bajo sol direccional.
 * @param {number} targetHex
 * @param {boolean} isFrame
 * @param {THREE.Plane[]} clipPlanes
 */
export function maStlFormworkEnvCreateAtk60Material(targetHex, isFrame, clipPlanes) {
    return new THREE.MeshLambertMaterial({
        color: targetHex,
        side: THREE.DoubleSide,
        clippingPlanes: clipPlanes || [],
        clipShadows: false,
        fog: false,
    });
}

/**
 * @param {THREE.Object3D} root
 * @param {boolean} cast
 * @param {boolean} receive
 */
export function maStlFormworkEnvSyncShadowFlags(root, cast, receive) {
    if (!root) return;
    root.traverse(function (obj) {
        if (obj && obj.isMesh) {
            obj.castShadow = !!cast;
            obj.receiveShadow = receive != null ? !!receive : !!cast;
        }
    });
}

/**
 * @param {THREE.Object3D[]} roots
 * @param {boolean} enabled
 * @param {THREE.WebGLRenderer} renderer
 * @param {THREE.DirectionalLight} keyLight
 * @param {ReturnType<typeof maStlFormworkEnvCreateRig>|null} [rig]
 */
export function maStlFormworkEnvApplyShadowState(roots, enabled, renderer, keyLight, rig) {
    if (renderer) renderer.shadowMap.enabled = !!enabled;
    if (keyLight) keyLight.castShadow = !!enabled;
    if (rig && rig.shadowCatcher) {
        rig.shadowCatcher.visible = false;
        rig.shadowCatcher.receiveShadow = false;
    }
    const list = Array.isArray(roots) ? roots : [];
    for (let i = 0; i < list.length; i++) {
        maStlFormworkEnvSyncShadowFlags(list[i], enabled, false);
    }
}

/**
 * @param {THREE.WebGLRenderer} renderer
 * @param {THREE.Scene} scene
 * @param {THREE.Camera} camera
 * @param {ReturnType<typeof maStlFormworkEnvCreateRig>} rig
 */
export function maStlFormworkEnvEnsureSsaoComposer(renderer, scene, camera, rig) {
    if (!rig) return null;
    if (!rig.settings.ssaoEnabled) {
        if (rig.ssaoComposer) {
            rig.ssaoComposer.dispose();
            rig.ssaoComposer = null;
            rig.ssaoPass = null;
        }
        return null;
    }
    if (rig.ssaoComposer && rig.ssaoComposer.passes && rig.ssaoComposer.passes.length) {
        const rp = rig.ssaoComposer.passes[0];
        if (rp && rp.camera) rp.camera = camera;
        return rig.ssaoComposer;
    }
    const composer = new EffectComposer(renderer);
    composer.addPass(new RenderPass(scene, camera));
    const ssao = new SSAOPass(scene, camera, renderer.domElement.width, renderer.domElement.height);
    ssao.kernelRadius = 12;
    ssao.minDistance = 0.0008;
    ssao.maxDistance = 0.08;
    composer.addPass(ssao);
    rig.ssaoComposer = composer;
    rig.ssaoPass = ssao;
    return composer;
}

/**
 * @param {ReturnType<typeof maStlFormworkEnvCreateRig>} rig
 * @param {number} width
 * @param {number} height
 * @param {THREE.Camera} camera
 */
export function maStlFormworkEnvResizeSsao(rig, width, height, camera) {
    if (!rig || !rig.ssaoComposer) return;
    rig.ssaoComposer.setSize(width, height);
    if (rig.ssaoPass) {
        rig.ssaoPass.setSize(width, height);
        rig.ssaoPass.camera = camera;
    }
    const rp = rig.ssaoComposer.passes[0];
    if (rp && rp.camera) rp.camera = camera;
}

export function maStlFormworkEnvSerializeSettings(settings) {
    const s = mergeFormworkEnvSettings(settings);
    return {
        sunAzimuthDeg: s.sunAzimuthDeg,
        sunElevationDeg: s.sunElevationDeg,
        sunIntensity: s.sunIntensity,
        ambientIntensity: s.ambientIntensity,
        hemisphereIntensity: s.hemisphereIntensity,
        fillIntensity: s.fillIntensity,
        rimIntensity: s.rimIntensity,
        iblIntensity: s.iblIntensity,
        exposure: s.exposure,
        fogEnabled: s.fogEnabled,
        fogStrength: s.fogStrength,
        shadowsEnabled: s.shadowsEnabled,
        shadowMapSize: s.shadowMapSize,
        shadowOpacity: s.shadowOpacity,
        ssaoEnabled: s.ssaoEnabled,
    };
}

/**
 * @param {Record<string, HTMLElement|null|undefined>} els
 * @returns {object}
 */
export function maStlFormworkEnvReadUi(els) {
    const readNum = function (el, fallback) {
        if (!el) return fallback;
        const v = Number.parseFloat(String(el.value).replace(',', '.'));
        return Number.isFinite(v) ? v : fallback;
    };
    const readBool = function (el, fallback) {
        if (!el) return fallback;
        return !!el.checked;
    };
    return mergeFormworkEnvSettings({
        sunAzimuthDeg: readNum(els.sunAzimuth, MA_STL_FORMWORK_ENV_DEFAULTS.sunAzimuthDeg),
        sunElevationDeg: readNum(els.sunElevation, MA_STL_FORMWORK_ENV_DEFAULTS.sunElevationDeg),
        sunIntensity: readNum(els.sunIntensity, MA_STL_FORMWORK_ENV_DEFAULTS.sunIntensity),
        ambientIntensity: readNum(els.ambientIntensity, MA_STL_FORMWORK_ENV_DEFAULTS.ambientIntensity),
        iblIntensity: readNum(els.iblIntensity, MA_STL_FORMWORK_ENV_DEFAULTS.iblIntensity),
        exposure: readNum(els.exposure, MA_STL_FORMWORK_ENV_DEFAULTS.exposure),
        fogStrength: readNum(els.fogStrength, MA_STL_FORMWORK_ENV_DEFAULTS.fogStrength),
        shadowMapSize: els.shadowQuality ? Number.parseInt(String(els.shadowQuality.value), 10) : 2048,
        fogEnabled: readBool(els.fogEnabled, MA_STL_FORMWORK_ENV_DEFAULTS.fogEnabled),
        shadowsEnabled: readBool(els.shadowsEnabled, MA_STL_FORMWORK_ENV_DEFAULTS.shadowsEnabled),
        ssaoEnabled: readBool(els.ssaoEnabled, MA_STL_FORMWORK_ENV_DEFAULTS.ssaoEnabled),
    });
}

/**
 * @param {Record<string, HTMLElement|null|undefined>} els
 * @param {object} settings
 */
export function maStlFormworkEnvWriteUi(els, settings) {
    const s = mergeFormworkEnvSettings(settings);
    const write = function (el, val) {
        if (el) el.value = String(val);
    };
    const writeCheck = function (el, val) {
        if (el) el.checked = !!val;
    };
    write(els.sunAzimuth, Math.round(s.sunAzimuthDeg));
    write(els.sunElevation, Math.round(s.sunElevationDeg));
    write(els.sunIntensity, s.sunIntensity.toFixed(2));
    write(els.ambientIntensity, s.ambientIntensity.toFixed(2));
    write(els.iblIntensity, s.iblIntensity.toFixed(2));
    write(els.exposure, s.exposure.toFixed(2));
    write(els.fogStrength, s.fogStrength.toFixed(2));
    if (els.shadowQuality) els.shadowQuality.value = String(s.shadowMapSize);
    writeCheck(els.fogEnabled, s.fogEnabled);
    writeCheck(els.shadowsEnabled, s.shadowsEnabled);
    writeCheck(els.ssaoEnabled, s.ssaoEnabled);
    const syncLabel = function (inputEl, labelEl, fmt) {
        if (!inputEl || !labelEl) return;
        const v = Number.parseFloat(String(inputEl.value).replace(',', '.'));
        labelEl.textContent = fmt(Number.isFinite(v) ? v : 0);
    };
    syncLabel(els.sunAzimuth, els.sunAzimuthLabel, function (v) { return Math.round(v) + '°'; });
    syncLabel(els.sunElevation, els.sunElevationLabel, function (v) { return Math.round(v) + '°'; });
    syncLabel(els.sunIntensity, els.sunIntensityLabel, function (v) { return v.toFixed(2); });
    syncLabel(els.ambientIntensity, els.ambientIntensityLabel, function (v) { return v.toFixed(2); });
    syncLabel(els.iblIntensity, els.iblIntensityLabel, function (v) { return v.toFixed(2); });
    syncLabel(els.exposure, els.exposureLabel, function (v) { return v.toFixed(2); });
    syncLabel(els.fogStrength, els.fogStrengthLabel, function (v) { return Math.round(v * 100) + '%'; });
}

export function maStlFormworkEnvDispose(rig) {
    if (!rig) return;
    if (rig.ssaoComposer) rig.ssaoComposer.dispose();
    if (rig.envRT) rig.envRT.dispose();
    if (rig.pmrem) rig.pmrem.dispose();
    if (rig.shadowCatcherMat) rig.shadowCatcherMat.dispose();
    if (rig.shadowCatcher && rig.shadowCatcher.geometry) rig.shadowCatcher.geometry.dispose();
}
