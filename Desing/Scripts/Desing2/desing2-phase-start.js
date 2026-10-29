/* Cuadro de encofrado por fases, con el mismo formato que el recuento.
   Al indicar los equipos salen las filas. El muro de inicio se elige
   en el visor de abajo. No reparte los tramos. */
import * as THREE from 'three';
import { OrbitControls } from '@masterarticles/OrbitControls';
import { solvePhasePlans } from './desing2-phase-solve.js';

var DEFAULT_COLOR = '#c9a0a8';
var DEFAULT_WORKERS = '4';

var walls = [];
var savedRows = [];
var activeTeam = -1;
var phasePlans = [];
var phasePlanIndex = 0;
var viewer = null;
var viewerDirty = true;
var running = false;

function modalEl() {
    return document.getElementById('desing2PhaseModal');
}

function text(name) {
    var el = modalEl();
    return el ? (el.getAttribute(name) || '') : '';
}

function fill(template) {
    var args = Array.prototype.slice.call(arguments, 1);
    return String(template || '').replace(/\{(\d+)\}/g, function (_m, index) {
        var value = args[Number(index)];
        return value == null ? '' : String(value);
    });
}

function toast(message, isError) {
    if (typeof window.maStlDesing2ShowSaveViewToast === 'function') {
        window.maStlDesing2ShowSaveViewToast(message, !!isError);
    }
}

function showMessage(message) {
    var el = document.getElementById('desing2PhaseMessage');
    if (!el) return;
    if (!message) {
        el.hidden = true;
        el.textContent = '';
        return;
    }
    el.hidden = false;
    el.textContent = message;
}

function meters(value) {
    var n = Number(value);
    if (!Number.isFinite(n)) return '';
    return n.toLocaleString('es-ES', { minimumFractionDigits: 2, maximumFractionDigits: 2 });
}

function wholeNumber(input) {
    if (!input) return 0;
    var raw = String(input.value || '').trim();
    if (!/^\d+$/.test(raw)) return 0;
    var n = Number(raw);
    return n >= 1 ? n : 0;
}

function readAxes() {
    if (typeof window.maStlDesing2BuildWallExchangePayload !== 'function') return [];
    var payload;
    try {
        payload = window.maStlDesing2BuildWallExchangePayload();
    } catch (_err) {
        return [];
    }
    var lines = payload && payload.walls ? payload.walls : [];
    var out = [];
    for (var i = 0; i < lines.length; i++) {
        var line = lines[i];
        if (!line || line.wallRole !== 'axis' || !line.p1Mm || !line.p2Mm) continue;
        var id = line.wallId != null ? line.wallId : line.id;
        if (id == null || id === '') continue;
        var dx = Number(line.p2Mm.x) - Number(line.p1Mm.x);
        var dz = Number(line.p2Mm.z) - Number(line.p1Mm.z);
        var length = Number(line._Datalong);
        if (!Number.isFinite(length) || length <= 0) length = Math.hypot(dx, dz) / 1000;
        if (!Number.isFinite(length) || length <= 0) continue;
        out.push({
            id: String(id),
            length: length,
            height: Number(line._DataHeight) > 0 ? Number(line._DataHeight) : 2.7,
            conn: [line._Idconnection_1 || '', line._Idconnection_2 || ''],
            tapeIni: line._Tape_1 || '',
            tapeEnd: line._Tape_2 || '',
            prop: !!(line._CHeckPropInside || line._CHeckPropOutside),
            propInf: !!(line._CHeckPropInsideInf || line._CHeckPropOutsideInf),
            stiff: !!(line._CHeckRijiInside || line._CHeckRijiOutside),
            bracket: !!(line._CHeckBracketInside || line._CHeckBracketOutside),
            p1: { x: Number(line.p1Mm.x) || 0, z: Number(line.p1Mm.z) || 0 },
            p2: { x: Number(line.p2Mm.x) || 0, z: Number(line.p2Mm.z) || 0 },
            thickMm: Math.max(50, (Number(line._DataWith) || 0.3) * 1000),
            heightMm: Math.max(300, (Number(line._DataHeight) || 2.7) * 1000)
        });
    }
    out.sort(function (a, b) { return Number(a.id) - Number(b.id); });
    return out;
}

function wallById(id) {
    for (var i = 0; i < walls.length; i++) {
        if (walls[i].id === id) return walls[i];
    }
    return null;
}

function rows() {
    var body = document.getElementById('desing2PhaseTeamsBody');
    return body ? body.querySelectorAll('tr') : [];
}

function rowData(tr) {
    var nameInput = tr.querySelector('[data-field="name"]');
    var workersInput = tr.querySelector('[data-field="workers"]');
    var colorInput = tr.querySelector('[data-field="color"]');
    return {
        team: Number(tr.getAttribute('data-team')) || 0,
        name: nameInput ? String(nameInput.value || '').trim() : '',
        workers: workersInput ? wholeNumber(workersInput) : 0,
        color: colorInput ? (colorInput.value || DEFAULT_COLOR) : DEFAULT_COLOR,
        wallId: tr.getAttribute('data-wall-id') || ''
    };
}

function wallLabel(wallId) {
    var wall = wallById(wallId);
    if (!wall) return '';
    return fill(text('data-wall-option') || 'Muro {0} · {1} m', wall.id, meters(wall.length));
}

function renderTeams(count) {
    var body = document.getElementById('desing2PhaseTeamsBody');
    var wrap = document.getElementById('desing2PhaseTableWrap');
    if (!body || !wrap) return;
    var previous = [];
    var current = rows();
    if (current.length) {
        for (var i = 0; i < current.length; i++) previous.push(rowData(current[i]));
        savedRows = previous.slice();
    } else {
        previous = savedRows.slice();
    }
    body.innerHTML = '';
    if (count < 1) {
        wrap.hidden = true;
        hideView();
        return;
    }
    wrap.hidden = false;
    var pickLabel = text('data-pick-wall') || 'Elegir muro';
    for (var n = 0; n < count; n++) {
        var kept = previous[n];
        var tr = document.createElement('tr');
        tr.setAttribute('data-team', String(n + 1));
        tr.setAttribute('data-wall-id', kept && kept.wallId ? kept.wallId : '');
        var name = kept && kept.name ? kept.name : fill(text('data-team-default') || 'Equipo {0}', n + 1);
        var workers = kept && kept.workers ? String(kept.workers) : DEFAULT_WORKERS;
        var color = kept && kept.color ? kept.color : DEFAULT_COLOR;
        tr.innerHTML =
            '<td>' + (n + 1) + '</td>' +
            '<td><input type="text" class="desing2-phase-name" data-field="name" maxlength="80" /></td>' +
            '<td><input type="number" class="desing2-phase-qty" data-field="workers" min="1" step="1" inputmode="numeric" /></td>' +
            '<td><input type="color" class="desing2-phase-color" data-field="color" /></td>' +
            '<td class="desing2-phase-wall"></td>' +
            '<td><button type="button" class="btn btn-sm btn-outline-secondary desing2-phase-pick">' + pickLabel + '</button></td>';
        tr.querySelector('[data-field="name"]').value = name;
        tr.querySelector('[data-field="workers"]').value = workers;
        tr.querySelector('[data-field="color"]').value = color;
        var label = tr.querySelector('.desing2-phase-wall');
        if (label) label.textContent = wallLabel(tr.getAttribute('data-wall-id'));
        body.appendChild(tr);
    }
    if (activeTeam >= count) hideView();
    else markActiveRow();
    paintWalls();
}

function markActiveRow() {
    var list = rows();
    for (var i = 0; i < list.length; i++) {
        list[i].classList.toggle('is-picking', i === activeTeam);
    }
}

function takenByOther(wallId, teamIndex) {
    var list = rows();
    for (var i = 0; i < list.length; i++) {
        if (i === teamIndex) continue;
        if ((list[i].getAttribute('data-wall-id') || '') === wallId) return true;
    }
    return false;
}

function assignWall(wallId) {
    if (activeTeam < 0) return;
    var list = rows();
    var tr = list[activeTeam];
    if (!tr) return;
    if (takenByOther(wallId, activeTeam)) {
        showMessage(text('data-wall-taken'));
        return;
    }
    showMessage('');
    tr.setAttribute('data-wall-id', wallId);
    var label = tr.querySelector('.desing2-phase-wall');
    if (label) label.textContent = wallLabel(wallId);
    paintWalls();
}

function currentPhasePlan() {
    return phasePlans.length ? phasePlans[phasePlanIndex] : null;
}

function colorForWall(wallId) {
    var plan = currentPhasePlan();
    if (plan && plan.byWall && plan.byWall[wallId]) {
        var phase = plan.phases[plan.byWall[wallId] - 1];
        if (phase && phase.color) return phase.color;
    }
    var list = rows();
    for (var i = 0; i < list.length; i++) {
        if ((list[i].getAttribute('data-wall-id') || '') === wallId) {
            var input = list[i].querySelector('[data-field="color"]');
            return input && input.value ? input.value : DEFAULT_COLOR;
        }
    }
    return '';
}

function paintWalls() {
    if (!viewer) return;
    var plan = currentPhasePlan();
    viewer.meshes.forEach(function (mesh, wallId) {
        var chosen = colorForWall(wallId);
        if (chosen) {
            mesh.material.color.set(chosen);
            mesh.material.opacity = plan ? 0.94 : 0.96;
        } else {
            mesh.material.color.set('#d5dde6');
            mesh.material.opacity = 0.72;
        }
    });
}

function meters1(value) {
    var n = Number(value);
    if (!Number.isFinite(n)) return '';
    return n.toLocaleString('es-ES', { minimumFractionDigits: 1, maximumFractionDigits: 1 });
}

function groundBlock(phase) {
    var ground = phase && phase.ground ? phase.ground : {};
    var keys = Object.keys(ground);
    var label = String(text('data-ground') || 'En el suelo: {0}').replace('{0}', '').replace(/:\s*$/, '');
    if (!keys.length) {
        return '<p class="desing2-phase-legend__idle">' + (text('data-ground-none') || 'En el suelo: nada') + '</p>';
    }
    var html = '<p class="desing2-phase-legend__idle">' + label + '</p><ul class="desing2-phase-legend__list">';
    for (var i = 0; i < keys.length; i++) {
        html += '<li><b>' + ground[keys[i]] + '</b> ' + keys[i] + '</li>';
    }
    return html + '</ul>';
}

function renderLegend(plan) {
    var host = document.getElementById('desing2PhaseLegend');
    if (!host) return;
    if (!plan) {
        host.hidden = true;
        host.innerHTML = '';
        return;
    }
    host.hidden = false;
    var tpl = text('data-legend') || 'Fase {0} · {1} m² · {2} esquinas · {3} tapes';
    var html = '<p class="desing2-phase-legend__title">' + plan.title + '</p>';
    for (var i = 0; i < plan.phases.length; i++) {
        var phase = plan.phases[i];
        html += '<article class="desing2-phase-legend__chip"><p class="desing2-phase-legend__phase"><i style="background:' +
            phase.color + '"></i><span>' +
            fill(tpl, phase.phase, meters1(phase.m2), phase.corners, phase.tapes) +
            '</span></p>' + groundBlock(phase) + '</article>';
    }
    host.innerHTML = html;
}

function applyPlanToDesign(plan) {
    if (!plan || typeof window.maStlDesing2PaintWallPhaseColors !== 'function') return;
    var colors = {};
    var ids = Object.keys(plan.byWall);
    for (var i = 0; i < ids.length; i++) {
        var phase = plan.phases[plan.byWall[ids[i]] - 1];
        if (phase) colors[ids[i]] = phase.color;
    }
    window.maStlDesing2PaintWallPhaseColors(colors);
}

function showPhaseResult() {
    var plan = currentPhasePlan();
    if (!plan) return;
    var host = document.getElementById('desing2PhaseView');
    var hint = document.getElementById('desing2PhaseViewHint');
    var another = document.getElementById('desing2PhaseAnother');
    activeTeam = -1;
    markActiveRow();
    if (host) {
        host.hidden = false;
        if (host.scrollIntoView) host.scrollIntoView({ block: 'nearest' });
    }
    if (hint) hint.textContent = plan.title || '';
    if (another) another.hidden = phasePlans.length < 2;
    renderLegend(plan);
    if (viewerDirty || !viewer) {
        rebuildMeshes();
        viewerDirty = false;
        if (plan.camera) applyPhaseCamera(plan.camera);
    } else {
        paintWalls();
        resizeView();
    }
    if (!running) {
        running = true;
        frame();
    }
    window.requestAnimationFrame(resizeView);
    applyPlanToDesign(plan);
    var starts = [];
    var list = rows();
    for (var i = 0; i < list.length; i++) starts.push(rowData(list[i]));
    window.desing2PhaseStartPlan = {
        phases: wholeNumber(document.getElementById('desing2PhaseCount')) || (plan.phases && plan.phases.length) || 0,
        teams: starts.length || (plan.starts ? plan.starts.length : 0),
        starts: starts.length ? starts : (plan.starts || []),
        solution: phasePlanIndex,
        solutionTitle: plan.title,
        byWall: plan.byWall,
        phaseSummary: plan.phases,
        kit: plan.kit || null,
        camera: readPhaseCamera() || plan.camera || null
    };
}

function readPhaseCamera() {
    if (!viewer || !viewer.camera || !viewer.controls) return null;
    var position = viewer.camera.position;
    var target = viewer.controls.target;
    return {
        position: [position.x, position.y, position.z],
        target: [target.x, target.y, target.z]
    };
}

function applyPhaseCamera(cam) {
    if (!viewer || !cam || !cam.position || !cam.target) return;
    if (cam.position.length < 3 || cam.target.length < 3) return;
    viewer.camera.position.set(Number(cam.position[0]) || 0, Number(cam.position[1]) || 0, Number(cam.position[2]) || 0);
    viewer.controls.target.set(Number(cam.target[0]) || 0, Number(cam.target[1]) || 0, Number(cam.target[2]) || 0);
    viewer.controls.update();
}

function designId() {
    var shell = document.getElementById('ma-stl-viewer-shell');
    var n = shell ? Number(shell.getAttribute('data-ma-stl-design-id')) : 0;
    return n > 0 ? n : 0;
}

function frame() {
    if (!running || !viewer) return;
    viewer.controls.update();
    viewer.renderer.render(viewer.scene, viewer.camera);
    window.requestAnimationFrame(frame);
}

function resizeView() {
    if (!viewer) return;
    var box = viewer.canvas.parentElement;
    if (!box) return;
    var width = Math.max(1, box.clientWidth);
    var height = Math.max(1, box.clientHeight);
    viewer.camera.aspect = width / height;
    viewer.camera.updateProjectionMatrix();
    viewer.renderer.setSize(width, height, false);
}

function fitCamera() {
    if (!viewer || !viewer.group.children.length) return;
    var bounds = new THREE.Box3().setFromObject(viewer.group);
    var size = bounds.getSize(new THREE.Vector3());
    var center = bounds.getCenter(new THREE.Vector3());
    var span = Math.max(size.x, size.y, size.z, 1000);
    viewer.camera.position.set(center.x + span * 0.85, center.y + span * 0.72, center.z + span * 0.95);
    viewer.controls.target.copy(center);
    viewer.controls.update();
    var gridSpan = Math.max(span * 2.4, 8000);
    if (viewer.grid) viewer.scene.remove(viewer.grid);
    viewer.grid = new THREE.GridHelper(gridSpan, 20, 0xb7c3d0, 0xd3dde8);
    viewer.grid.position.y = 0;
    viewer.scene.add(viewer.grid);
}

function ensureViewer() {
    if (viewer) return viewer;
    var canvas = document.getElementById('desing2PhaseCanvas');
    if (!canvas) return null;
    var renderer = new THREE.WebGLRenderer({ canvas: canvas, antialias: true });
    renderer.setPixelRatio(Math.min(window.devicePixelRatio || 1, 2));
    renderer.setClearColor(0xe7eef5, 1);
    var scene = new THREE.Scene();
    var camera = new THREE.PerspectiveCamera(42, 1, 10, 500000);
    var controls = new OrbitControls(camera, canvas);
    controls.enableDamping = true;
    controls.target.set(0, 1000, 0);
    scene.add(new THREE.AmbientLight(0xffffff, 0.85));
    var sun = new THREE.DirectionalLight(0xffffff, 0.85);
    sun.position.set(4000, 8000, 3000);
    scene.add(sun);
    var group = new THREE.Group();
    scene.add(group);
    viewer = {
        canvas: canvas,
        renderer: renderer,
        scene: scene,
        camera: camera,
        controls: controls,
        group: group,
        grid: null,
        meshes: new Map()
    };
    var downAt = null;
    canvas.addEventListener('pointerdown', function (ev) {
        downAt = { x: ev.clientX, y: ev.clientY, pointerId: ev.pointerId };
    });
    canvas.addEventListener('pointerup', function (ev) {
        if (!downAt) return;
        var dx = ev.clientX - downAt.x;
        var dy = ev.clientY - downAt.y;
        downAt = null;
        if (dx * dx + dy * dy > 16) return;
        pickAt(ev);
    });
    return viewer;
}

function disposeObject(obj) {
    if (!obj) return;
    var kids = obj.children ? obj.children.slice() : [];
    for (var i = 0; i < kids.length; i++) disposeObject(kids[i]);
    if (obj.geometry) obj.geometry.dispose();
    if (obj.material) {
        if (Array.isArray(obj.material)) {
            for (var m = 0; m < obj.material.length; m++) obj.material[m].dispose();
        } else {
            obj.material.dispose();
        }
    }
}

function rebuildMeshes() {
    var view = ensureViewer();
    if (!view) return;
    while (view.group.children.length) {
        disposeObject(view.group.children.pop());
    }
    view.meshes = new Map();
    for (var i = 0; i < walls.length; i++) {
        var wall = walls[i];
        var dx = wall.p2.x - wall.p1.x;
        var dz = wall.p2.z - wall.p1.z;
        var len = Math.hypot(dx, dz);
        if (len < 1) continue;
        var geo = new THREE.BoxGeometry(len, wall.heightMm, wall.thickMm);
        var mat = new THREE.MeshStandardMaterial({
            color: 0xd5dde6,
            transparent: true,
            opacity: 0.72,
            roughness: 0.72,
            metalness: 0
        });
        var mesh = new THREE.Mesh(geo, mat);
        mesh.position.set((wall.p1.x + wall.p2.x) * 0.5, wall.heightMm * 0.5, (wall.p1.z + wall.p2.z) * 0.5);
        mesh.rotation.y = -Math.atan2(dz, dx);
        mesh.userData.wallId = wall.id;
        var edges = new THREE.LineSegments(
            new THREE.EdgesGeometry(geo),
            new THREE.LineBasicMaterial({ color: 0x8aa0b8 })
        );
        mesh.add(edges);
        view.group.add(mesh);
        view.meshes.set(wall.id, mesh);
    }
    paintWalls();
    fitCamera();
    resizeView();
}

function pickAt(ev) {
    if (!viewer || activeTeam < 0) return;
    var rect = viewer.canvas.getBoundingClientRect();
    if (!rect.width || !rect.height) return;
    var ndc = new THREE.Vector2(
        ((ev.clientX - rect.left) / rect.width) * 2 - 1,
        -((ev.clientY - rect.top) / rect.height) * 2 + 1
    );
    var ray = new THREE.Raycaster();
    ray.setFromCamera(ndc, viewer.camera);
    var hits = ray.intersectObjects(viewer.group.children, false);
    if (!hits.length) return;
    var wallId = hits[0].object.userData && hits[0].object.userData.wallId;
    if (wallId) assignWall(String(wallId));
}

function showView(teamIndex) {
    var host = document.getElementById('desing2PhaseView');
    var hint = document.getElementById('desing2PhaseViewHint');
    var list = rows();
    var tr = list[teamIndex];
    if (!host || !tr) return;
    activeTeam = teamIndex;
    markActiveRow();
    host.hidden = false;
    if (host.scrollIntoView) host.scrollIntoView({ block: 'nearest' });
    var data = rowData(tr);
    if (hint) hint.textContent = fill(text('data-pick-hint'), data.name || fill(text('data-team-default'), teamIndex + 1));
    if (viewerDirty || !viewer) {
        rebuildMeshes();
        viewerDirty = false;
    } else {
        paintWalls();
        resizeView();
    }
    if (!running) {
        running = true;
        frame();
    }
    window.requestAnimationFrame(resizeView);
}

function hideView() {
    var host = document.getElementById('desing2PhaseView');
    if (host) host.hidden = true;
    activeTeam = -1;
    running = false;
    markActiveRow();
}

function phaseDialog() {
    var el = modalEl();
    return el ? el.querySelector('.desing2-phase-dialog') : null;
}

function clearPhaseResult() {
    phasePlans = [];
    phasePlanIndex = 0;
    renderLegend(null);
    var another = document.getElementById('desing2PhaseAnother');
    if (another) another.hidden = true;
}

function syncForm() {
    clearPhaseResult();
    var phases = wholeNumber(document.getElementById('desing2PhaseCount'));
    var teams = wholeNumber(document.getElementById('desing2PhaseTeams'));
    var dialog = phaseDialog();
    if (teams > walls.length) {
        showMessage(text('data-too-many-teams'));
        if (dialog) dialog.classList.remove('is-expanded');
        renderTeams(0);
        return;
    }
    if (!phases || !teams) {
        showMessage('');
        if (dialog) dialog.classList.remove('is-expanded');
        renderTeams(0);
        return;
    }
    showMessage('');
    if (dialog) dialog.classList.add('is-expanded');
    renderTeams(teams);
}

function onAccept() {
    var phases = wholeNumber(document.getElementById('desing2PhaseCount'));
    var teamCount = wholeNumber(document.getElementById('desing2PhaseTeams'));
    if (!phases || !teamCount) {
        showMessage(text('data-need-numbers'));
        return;
    }
    if (teamCount > walls.length) {
        showMessage(text('data-too-many-teams'));
        return;
    }
    var list = rows();
    if (list.length !== teamCount) renderTeams(teamCount);
    list = rows();
    var starts = [];
    var seen = {};
    for (var i = 0; i < list.length; i++) {
        var item = rowData(list[i]);
        if (!item.name) {
            showMessage(text('data-need-name'));
            return;
        }
        if (!item.workers) {
            showMessage(text('data-need-workers'));
            return;
        }
        if (!item.wallId || !wallById(item.wallId)) {
            showMessage(text('data-need-all-walls'));
            return;
        }
        if (seen[item.wallId]) {
            showMessage(text('data-wall-taken'));
            return;
        }
        seen[item.wallId] = true;
        starts.push(item);
    }
    var origins = [];
    for (var s = 0; s < starts.length; s++) origins.push(starts[s].wallId);
    if (formMatchesPlan(phases, teamCount, starts)) {
        var dialogSame = phaseDialog();
        if (dialogSame) dialogSame.classList.add('is-expanded');
        showPhaseResult();
        persistAccepted();
        return;
    }
    try {
        phasePlans = solvePhasePlans(walls, origins, phases);
    } catch (_err) {
        showMessage(text('data-solve-fail') || 'No se ha podido repartir.');
        return;
    }
    phasePlanIndex = 0;
    if (!phasePlans.length) {
        showMessage(text('data-need-all-walls'));
        return;
    }
    var dialog = phaseDialog();
    if (dialog) dialog.classList.add('is-expanded');
    showPhaseResult();
    persistAccepted();
}

function formMatchesPlan(phases, teamCount, starts) {
    var saved = window.desing2PhaseStartPlan;
    if (!phasePlans.length || !saved) return false;
    if (Number(saved.phases) !== phases || (saved.starts || []).length !== starts.length) return false;
    if (Number(saved.teams) && Number(saved.teams) !== teamCount) return false;
    for (var i = 0; i < starts.length; i++) {
        var prev = saved.starts[i] || {};
        if (String(prev.wallId || '') !== String(starts[i].wallId || '')) return false;
    }
    return true;
}

function persistAccepted() {
    var payload = window.desing2PhaseStartPlan;
    var id = designId();
    if (!payload || !id) {
        toast(text('data-solve-fail') || 'No se ha podido guardar.', true);
        return;
    }
    var saveUrl = text('data-save-plan-url');
    var wallsSave = typeof window.maStlDesing2SaveDesignWallsToServer === 'function'
        ? window.maStlDesing2SaveDesignWallsToServer({ includePhoto: true })
        : Promise.resolve({ skipped: true });
    var cameraSave = typeof window.maStlDesing2SaveCurrentViewerState === 'function'
        ? window.maStlDesing2SaveCurrentViewerState()
        : Promise.resolve({ skipped: true });
    var phaseSave = fetch(saveUrl, {
        method: 'POST',
        headers: { 'Content-Type': 'application/json', Accept: 'application/json' },
        credentials: 'same-origin',
        body: JSON.stringify({ DesignId: id, Plan: payload })
    }).then(function (res) {
        if (!res.ok) throw new Error('HTTP ' + res.status);
        return res.json();
    }).then(function (resp) {
        if (!resp || resp.Exito !== true) {
            throw new Error((resp && resp.Mensaje) || 'No se han podido guardar las fases.');
        }
        return resp;
    });
    Promise.all([wallsSave, cameraSave, phaseSave]).then(function (results) {
        var camera = results[1] || {};
        if (camera.failed) throw new Error(camera.message || 'No se ha podido guardar la cámara.');
        var el = modalEl();
        if (el) el.setAttribute('data-completed', '1');
        if (el && window.bootstrap && window.bootstrap.Modal) {
            window.bootstrap.Modal.getOrCreateInstance(el).hide();
        }
        var now = new Date();
        var hh = now.getHours();
        var mm = now.getMinutes();
        var ss = now.getSeconds();
        var clock = (hh < 10 ? '0' : '') + hh + ':' + (mm < 10 ? '0' : '') + mm + ':' + (ss < 10 ? '0' : '') + ss;
        toast(fill(text('data-noted'), payload.phases, payload.teams || (payload.starts || []).length) + ' · ' + clock, false);
    }).catch(function (err) {
        var message = err && err.message ? err.message : (text('data-solve-fail') || 'No se ha podido guardar.');
        showMessage(message);
        toast(message, true);
    });
}

function rememberLoadedPlan(plan) {
    if (!plan || !plan.byWall || !plan.phaseSummary) return;
    window.desing2PhaseStartPlan = plan;
    phasePlans = [{
        title: plan.solutionTitle || '',
        byWall: plan.byWall,
        phases: plan.phaseSummary,
        kit: plan.kit || null,
        camera: plan.camera || null
    }];
    phasePlanIndex = 0;
    applyPlanToDesign(phasePlans[0]);
}

function loadSavedPlan() {
    var id = designId();
    var url = text('data-load-plan-url');
    if (!id || !url) return;
    var join = url.indexOf('?') >= 0 ? '&' : '?';
    fetch(url + join + 'designId=' + encodeURIComponent(String(id)), {
        method: 'GET',
        headers: { Accept: 'application/json' },
        credentials: 'same-origin'
    }).then(function (res) {
        if (!res.ok) return null;
        return res.json();
    }).then(function (resp) {
        if (!resp || resp.Exito !== true) return;
        var plan = null;
        if (resp.PlanJson) {
            try { plan = JSON.parse(resp.PlanJson); } catch (_err) { plan = null; }
        }
        var rows = resp.Walls || resp.walls || [];
        if (rows.length) {
            plan = plan || {};
            var byWall = {};
            var maxPhase = 0;
            var colors = ['#3b82f6', '#16a34a', '#d97706', '#9333ea', '#dc2626', '#0891b2'];
            for (var i = 0; i < rows.length; i++) {
                var row = rows[i] || {};
                var phaseNo = Number(row.Phase != null ? row.Phase : row.phase);
                var wallId = String(row.WallId != null ? row.WallId : row.wallId || '');
                if (!wallId || !phaseNo) continue;
                byWall[wallId] = phaseNo;
                if (phaseNo > maxPhase) maxPhase = phaseNo;
            }
            plan.byWall = byWall;
            if (!plan.phases) plan.phases = maxPhase;
            if (!plan.phaseSummary || !plan.phaseSummary.length) {
                plan.phaseSummary = [];
                for (var p = 1; p <= maxPhase; p++) {
                    plan.phaseSummary.push({
                        phase: p,
                        color: colors[(p - 1) % colors.length],
                        m2: 0,
                        corners: 0,
                        tapes: 0
                    });
                }
            }
        }
        rememberLoadedPlan(plan);
    }).catch(function () { /* el diseño se abre igual si el plan aún no existe */ });
}

function showAnotherPlan() {
    if (phasePlans.length < 2) {
        toast(text('data-same') || 'Esta solución ya estaba', false);
        return;
    }
    phasePlanIndex = (phasePlanIndex + 1) % phasePlans.length;
    viewerDirty = false;
    showPhaseResult();
}

function openModal() {
    var el = modalEl();
    if (!el || !window.bootstrap || !window.bootstrap.Modal) return;
    walls = readAxes();
    viewerDirty = true;
    if (!walls.length) {
        toast(text('data-no-walls') || 'Este diseño no tiene muros.', true);
        return;
    }
    el.setAttribute('data-completed', '0');
    var phasesInput = document.getElementById('desing2PhaseCount');
    var teamsInput = document.getElementById('desing2PhaseTeams');
    var saved = window.desing2PhaseStartPlan;
    if (saved && saved.byWall && saved.phases && (saved.starts || []).length) {
        if (phasesInput) phasesInput.value = String(saved.phases);
        if (teamsInput) teamsInput.value = String(saved.teams || saved.starts.length);
        savedRows = saved.starts.slice();
        showMessage('');
        syncForm();
        rememberLoadedPlan(saved);
        showPhaseResult();
    } else {
        if (phasesInput) phasesInput.value = '';
        if (teamsInput) teamsInput.value = '';
        savedRows = [];
        clearPhaseResult();
        var dialog = phaseDialog();
        if (dialog) dialog.classList.remove('is-expanded');
        renderTeams(0);
        showMessage('');
        hideView();
    }
    var host = document.fullscreenElement || document.body;
    if (el.parentNode !== host) host.appendChild(el);
    window.bootstrap.Modal.getOrCreateInstance(el).show();
    if (phasesInput) phasesInput.focus();
}

function wire() {
    var button = document.getElementById('ma-stl-tool-phase');
    var accept = document.getElementById('desing2PhaseAccept');
    var another = document.getElementById('desing2PhaseAnother');
    var phasesInput = document.getElementById('desing2PhaseCount');
    var teamsInput = document.getElementById('desing2PhaseTeams');
    var body = document.getElementById('desing2PhaseTeamsBody');
    var el = modalEl();
    if (!button || !accept || !phasesInput || !teamsInput || !body || !el) return;
    button.addEventListener('click', function (ev) {
        ev.preventDefault();
        ev.stopPropagation();
        openModal();
    });
    accept.addEventListener('click', function (ev) {
        ev.preventDefault();
        onAccept();
    });
    if (another) {
        another.addEventListener('click', function (ev) {
            ev.preventDefault();
            showAnotherPlan();
        });
    }
    phasesInput.addEventListener('input', syncForm);
    teamsInput.addEventListener('input', syncForm);
    body.addEventListener('click', function (ev) {
        var pick = ev.target && ev.target.closest ? ev.target.closest('.desing2-phase-pick') : null;
        if (!pick) return;
        ev.preventDefault();
        var tr = pick.closest('tr');
        if (!tr || !tr.parentNode) return;
        var index = Array.prototype.indexOf.call(tr.parentNode.children, tr);
        clearPhaseResult();
        showView(index);
    });
    body.addEventListener('input', function (ev) {
        var field = ev.target && ev.target.getAttribute ? ev.target.getAttribute('data-field') : '';
        if (field === 'color') paintWalls();
        if (field === 'name' && activeTeam >= 0) {
            var list = rows();
            var tr = list[activeTeam];
            var hint = document.getElementById('desing2PhaseViewHint');
            if (tr && hint) {
                var data = rowData(tr);
                hint.textContent = fill(text('data-pick-hint'), data.name || fill(text('data-team-default'), activeTeam + 1));
            }
        }
    });
    el.addEventListener('hidden.bs.modal', function () {
        hideView();
        if (el.getAttribute('data-completed') !== '1') {
            var phasesInput = document.getElementById('desing2PhaseCount');
            if (phasesInput) phasesInput.value = '';
            teamsInput.value = '';
            savedRows = [];
            var dialog = phaseDialog();
            if (dialog) dialog.classList.remove('is-expanded');
            renderTeams(0);
        }
        showMessage('');
    });
    window.addEventListener('resize', resizeView);
}

if (document.readyState === 'loading') {
    document.addEventListener('DOMContentLoaded', wire);
} else {
    wire();
}
document.addEventListener('desing2-walls-loaded', loadSavedPlan);
