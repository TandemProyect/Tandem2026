/* Reparte los muros en fases y los deja pintados por color.
   El juego que se trae a obra es el máximo de cada pieza, no la suma.
   En una fase, lo que solo hace falta en otra se queda en el suelo.
   La esquina que se toma de nuevas entra entera. La esquina siguiente
   puede ir en otra fase: si no, la primera se queda el plano entero
   y las demás no se pintan. */

var PHASE_COLORS = ['#3b82f6', '#16a34a', '#d97706', '#9333ea', '#dc2626', '#0891b2'];

var STRATEGIES = [
    { id: 'corners-long', title: 'Esquinas primero, tramos largos', complete: true, length: -1, roundRobin: false },
    { id: 'corners-short', title: 'Esquinas primero, tramos cortos', complete: true, length: 1, roundRobin: false },
    { id: 'long', title: 'Tramos largos', complete: false, length: -1, roundRobin: false },
    { id: 'short', title: 'Tramos cortos', complete: false, length: 1, roundRobin: false },
    { id: 'two-starts', title: 'Reparte los arranques', complete: true, length: -1, roundRobin: true }
];

function jointOf(raw) {
    var text = String(raw || '').trim();
    var cut = text.indexOf(':');
    if (cut < 1) return null;
    var kind = text.slice(0, cut);
    if (kind !== 'L' && kind !== 'T' && kind !== 'Cross') return null;
    var members = text.slice(cut + 1).split('-').map(function (part) { return part.trim(); }).filter(Boolean);
    if (members.length < 2) return null;
    return { key: text, kind: kind, members: members };
}

function indexWalls(walls) {
    var byId = {};
    var joints = {};
    var neighbors = {};
    for (var i = 0; i < walls.length; i++) {
        var wall = walls[i];
        byId[wall.id] = wall;
        neighbors[wall.id] = [];
    }
    for (var w = 0; w < walls.length; w++) {
        var item = walls[w];
        var ends = item.conn || [];
        for (var e = 0; e < ends.length; e++) {
            var joint = jointOf(ends[e]);
            if (!joint || joints[joint.key]) continue;
            joints[joint.key] = joint;
            for (var a = 0; a < joint.members.length; a++) {
                for (var b = a + 1; b < joint.members.length; b++) {
                    var left = joint.members[a];
                    var right = joint.members[b];
                    if (!byId[left] || !byId[right]) continue;
                    neighbors[left].push({ id: right, joint: joint.key });
                    neighbors[right].push({ id: left, joint: joint.key });
                }
            }
        }
    }
    return { byId: byId, joints: joints, neighbors: neighbors };
}

function areaOf(wall) {
    var height = Number(wall.height) > 0 ? Number(wall.height) : 2.7;
    return (Number(wall.length) || 0) * height;
}

function phaseAreas(byWall, walls, phaseCount) {
    var areas = [];
    for (var p = 0; p < phaseCount; p++) areas.push(0);
    for (var i = 0; i < walls.length; i++) {
        var phase = byWall[walls[i].id];
        if (phase >= 1 && phase <= phaseCount) areas[phase - 1] += areaOf(walls[i]);
    }
    return areas;
}

function unassignedTouching(graph, byWall, phase) {
    var seen = {};
    var out = [];
    var ids = Object.keys(byWall);
    for (var i = 0; i < ids.length; i++) {
        if (byWall[ids[i]] !== phase) continue;
        var near = graph.neighbors[ids[i]] || [];
        for (var n = 0; n < near.length; n++) {
            var other = near[n].id;
            if (byWall[other] || seen[other]) continue;
            seen[other] = true;
            out.push(other);
        }
    }
    return out;
}

function cornerScore(graph, byWall, wallId, phase) {
    var wall = graph.byId[wallId];
    var ends = wall && wall.conn ? wall.conn : [];
    var score = 0;
    for (var i = 0; i < ends.length; i++) {
        var joint = graph.joints[String(ends[i] || '').trim()];
        if (!joint) continue;
        var inside = 0;
        var free = 0;
        for (var m = 0; m < joint.members.length; m++) {
            var member = joint.members[m];
            if (!graph.byId[member]) continue;
            if (member === wallId) continue;
            if (byWall[member] === phase) inside += 1;
            else if (!byWall[member]) free += 1;
        }
        if (inside > 0 && free === 0) score += 5;
        else if (inside > 0) score += 2;
    }
    return score;
}

function seedOfCandidate(graph, byWall, wallId, origins) {
    var best = origins[0];
    var bestDist = 1e9;
    for (var s = 0; s < origins.length; s++) {
        var dist = distanceThrough(graph, byWall, origins[s], wallId);
        if (dist < bestDist) {
            bestDist = dist;
            best = origins[s];
        }
    }
    return best;
}

function distanceThrough(graph, byWall, fromId, toId) {
    if (fromId === toId) return 0;
    var queue = [fromId];
    var seen = {};
    seen[fromId] = 0;
    while (queue.length) {
        var current = queue.shift();
        var near = graph.neighbors[current] || [];
        for (var i = 0; i < near.length; i++) {
            var next = near[i].id;
            if (seen[next] != null) continue;
            if (next !== toId && byWall[next] && byWall[next] !== byWall[fromId]) continue;
            seen[next] = seen[current] + 1;
            if (next === toId) return seen[next];
            queue.push(next);
        }
    }
    return 1e9;
}

function areaBySeed(graph, byWall, origins, phase) {
    var totals = {};
    for (var s = 0; s < origins.length; s++) totals[origins[s]] = 0;
    var ids = Object.keys(byWall);
    for (var i = 0; i < ids.length; i++) {
        if (byWall[ids[i]] !== phase) continue;
        var seed = origins.indexOf(ids[i]) >= 0 ? ids[i] : seedOfCandidate(graph, byWall, ids[i], origins);
        if (totals[seed] == null) totals[seed] = 0;
        totals[seed] += areaOf(graph.byId[ids[i]]);
    }
    return totals;
}

function chooseWall(graph, byWall, candidates, phase, origins, strategy) {
    var seedAreas = strategy.roundRobin ? areaBySeed(graph, byWall, origins, phase) : null;
    var best = candidates[0];
    var bestScore = -1e15;
    for (var i = 0; i < candidates.length; i++) {
        var id = candidates[i];
        var wall = graph.byId[id];
        var score = strategy.complete ? cornerScore(graph, byWall, id, phase) * 1000 : 0;
        score += (Number(wall.length) || 0) * (strategy.length < 0 ? 1 : -1);
        if (seedAreas) {
            var seed = seedOfCandidate(graph, byWall, id, origins);
            score -= (seedAreas[seed] || 0) * 10;
        }
        if (score > bestScore) {
            bestScore = score;
            best = id;
        }
    }
    return best;
}

function growPhase(graph, byWall, phase, origins, target, strategy) {
    var guard = 0;
    while (guard < 80) {
        guard += 1;
        var areas = phaseAreas(byWall, Object.keys(graph.byId).map(function (id) { return graph.byId[id]; }), phase);
        if (areas[phase - 1] >= target - 0.4) break;
        var candidates = unassignedTouching(graph, byWall, phase);
        if (!candidates.length) break;
        var next = chooseWall(graph, byWall, candidates, phase, origins, strategy);
        byWall[next] = phase;
        claimJoints(graph, byWall, next, phase);
    }
}

function pullToFirst(graph, byWall, phaseCount) {
    var walls = Object.keys(graph.byId).map(function (id) { return graph.byId[id]; });
    var guard = 0;
    while (guard < 40) {
        guard += 1;
        var areas = phaseAreas(byWall, walls, phaseCount);
        var biggest = 1;
        for (var p = 1; p < phaseCount; p++) {
            if (areas[p] > areas[biggest]) biggest = p;
        }
        if (areas[biggest] <= areas[0] + 0.4) break;
        var candidates = unassignedTouching(graph, byWall, 1).filter(function (id) { return byWall[id] === biggest + 1; });
        if (!candidates.length) {
            var ids = Object.keys(byWall);
            candidates = [];
            for (var i = 0; i < ids.length; i++) {
                if (byWall[ids[i]] !== biggest + 1) continue;
                var near = graph.neighbors[ids[i]] || [];
                for (var n = 0; n < near.length; n++) {
                    if (byWall[near[n].id] === 1) candidates.push(ids[i]);
                }
            }
        }
        if (!candidates.length) break;
        var moving = candidates[0];
        var left = 0;
        var all = Object.keys(byWall);
        for (var k = 0; k < all.length; k++) if (byWall[all[k]] === biggest + 1) left += 1;
        if (left <= 1) break;
        byWall[moving] = 1;
    }
}

function describe(graph, byWall, phaseCount) {
    var walls = Object.keys(graph.byId).map(function (id) { return graph.byId[id]; });
    var areas = phaseAreas(byWall, walls, phaseCount);
    var phases = [];
    for (var p = 1; p <= phaseCount; p++) {
        var ids = [];
        for (var i = 0; i < walls.length; i++) {
            if (byWall[walls[i].id] === p) ids.push(walls[i].id);
        }
        ids.sort(function (a, b) { return Number(a) - Number(b); });
        phases.push({
            phase: p,
            color: PHASE_COLORS[(p - 1) % PHASE_COLORS.length],
            wallIds: ids,
            m2: Math.round(areas[p - 1] * 100) / 100,
            corners: 0,
            tapes: 0
        });
    }
    var jointKeys = Object.keys(graph.joints);
    for (var j = 0; j < jointKeys.length; j++) {
        var joint = graph.joints[jointKeys[j]];
        var present = [];
        for (var m = 0; m < joint.members.length; m++) {
            if (graph.byId[joint.members[m]]) present.push(joint.members[m]);
        }
        if (present.length < 2) continue;
        var phaseSet = {};
        for (var q = 0; q < present.length; q++) phaseSet[byWall[present[q]]] = true;
        var used = Object.keys(phaseSet);
        if (used.length === 1) {
            var owner = Number(used[0]);
            if (owner >= 1 && owner <= phaseCount) phases[owner - 1].corners += 1;
        } else {
            for (var t = 0; t < present.length; t++) {
                var at = byWall[present[t]];
                if (at >= 1 && at <= phaseCount) phases[at - 1].tapes += 1;
            }
        }
    }
    return phases;
}

var PANEL_WIDTHS = [900, 750, 600, 450, 300];

function addPiece(bag, key, count) {
    var n = Number(count) || 0;
    if (!key || n <= 0) return;
    bag[key] = (bag[key] || 0) + n;
}

function widthLabel(mm) {
    return (mm / 1000).toFixed(2).replace('.', ',');
}

function panelHeightLabel(heightM) {
    if (heightM <= 1.2) return '1,20';
    if (heightM <= 2.4) return '2,40';
    return '2,70';
}

function panelCourses(heightM) {
    var height = Number(heightM) > 0 ? Number(heightM) : 2.7;
    if (height <= 2.7) return [{ label: panelHeightLabel(height), count: 1 }];
    var full = Math.floor((height + 0.001) / 2.7);
    var rest = Math.round((height - full * 2.7) * 1000) / 1000;
    var courses = [];
    if (full > 0) courses.push({ label: '2,70', count: full });
    if (rest > 0.05) courses.push({ label: panelHeightLabel(rest), count: 1 });
    return courses;
}

function packWidths(longMm) {
    var steps = [2550, 2400, 2250, 2100, 1950, 1800, 1650, 1500, 1350, 1200, 1050, 900, 750, 600, 450, 300];
    var n270 = Math.floor(longMm / 2700);
    var rem = longMm - n270 * 2700;
    var counts = {};
    function largest(value) {
        if (value <= 150) return 0;
        for (var i = 0; i < steps.length; i++) {
            if (steps[i] <= value && steps[i] >= 300) return steps[i];
        }
        return 0;
    }
    var finalMm = largest(rem);
    if (finalMm >= 300) {
        counts[finalMm] = 1;
        rem -= finalMm;
    }
    var guard = 0;
    while (rem > 150 && guard < 40) {
        guard += 1;
        var raised = false;
        for (var s = 0; s < steps.length; s++) {
            var size = steps[s];
            if (!counts[size]) continue;
            var next = size + 150;
            if (steps.indexOf(next) < 0) continue;
            counts[size] -= 1;
            if (!counts[size]) delete counts[size];
            counts[next] = (counts[next] || 0) + 1;
            rem -= 150;
            raised = true;
            break;
        }
        if (raised) continue;
        if (n270 <= 0) break;
        n270 -= 1;
        counts[2550] = (counts[2550] || 0) + 1;
        counts[300] = (counts[300] || 0) + 1;
        rem -= 150;
    }
    if (rem > 150) rem = 150;
    var widths = {};
    function take(mm) {
        var left = mm;
        while (left >= 300) {
            var placed = false;
            for (var w = 0; w < PANEL_WIDTHS.length; w++) {
                if (PANEL_WIDTHS[w] <= left) {
                    widths[PANEL_WIDTHS[w]] = (widths[PANEL_WIDTHS[w]] || 0) + 1;
                    left -= PANEL_WIDTHS[w];
                    placed = true;
                    break;
                }
            }
            if (!placed) break;
        }
    }
    for (var n = 0; n < n270; n++) take(2700);
    var moduleKeys = Object.keys(counts);
    for (var m = 0; m < moduleKeys.length; m++) {
        var mod = Number(moduleKeys[m]);
        for (var c = 0; c < counts[mod]; c++) take(mod);
    }
    return { widths: widths, plankMm: rem > 1 ? rem : 0 };
}

function endSides(graph, byWall, wallId, raw) {
    var joint = graph.joints[String(raw || '').trim()];
    var phase = Number(byWall[wallId]);
    var earlier = false;
    var later = false;
    if (!joint) return { earlier: false, later: false };
    for (var i = 0; i < joint.members.length; i++) {
        var member = joint.members[i];
        if (member === wallId || !graph.byId[member]) continue;
        var other = Number(byWall[member]);
        if (other > phase) later = true;
        else if (other >= 1 && other < phase) earlier = true;
    }
    return { earlier: earlier, later: later };
}

function endIsOpen(graph, byWall, wallId, raw) {
    if (!raw) return true;
    var joint = graph.joints[String(raw).trim()];
    if (!joint) return true;
    var sides = endSides(graph, byWall, wallId, raw);
    if (sides.earlier && !sides.later) return false;
    return sides.later;
}

function addPacked(used, wall, lengthMm) {
    if (!(lengthMm > 40)) return;
    var packed = packWidths(Math.round(lengthMm));
    var courses = panelCourses(wall.height);
    var widthKeys = Object.keys(packed.widths);
    for (var c = 0; c < courses.length; c++) {
        for (var w = 0; w < widthKeys.length; w++) {
            var width = Number(widthKeys[w]);
            addPiece(used, 'Panel ' + courses[c].label + '×' + widthLabel(width), packed.widths[width] * courses[c].count * 2);
        }
        if (packed.plankMm > 1) addPiece(used, 'Tablón', courses[c].count * 2);
    }
}

function fillMaterial(graph, byWall, phases) {
    for (var p = 0; p < phases.length; p++) phases[p].used = {};
    var ids = Object.keys(graph.byId);
    for (var i = 0; i < ids.length; i++) {
        var wall = graph.byId[ids[i]];
        var phaseNo = byWall[wall.id];
        if (!phaseNo || !phases[phaseNo - 1]) continue;
        var used = phases[phaseNo - 1].used;
        var lengthMm = Math.round((Number(wall.length) || 0) * 1000);
        var ends = wall.conn || [];
        for (var e = 0; e < ends.length; e++) {
            var sides = endSides(graph, byWall, wall.id, ends[e]);
            if (sides.earlier && !sides.later) lengthMm -= 1200;
        }
        if (lengthMm < 300) lengthMm = 300;
        addPacked(used, wall, lengthMm);
        if (endIsOpen(graph, byWall, wall.id, ends[0])) addPiece(used, 'Tape ' + (wall.tapeIni || 'extremo'), 1);
        if (endIsOpen(graph, byWall, wall.id, ends[1])) addPiece(used, 'Tape ' + (wall.tapeEnd || 'extremo'), 1);
        if (wall.prop) addPiece(used, 'Puntal', 1);
        if (wall.propInf) addPiece(used, 'Puntal inferior', 1);
        if (wall.stiff) addPiece(used, 'Rigidizador', 1);
        if (wall.bracket) addPiece(used, 'Ménsula', 1);
    }
    var jointKeys = Object.keys(graph.joints);
    for (var j = 0; j < jointKeys.length; j++) {
        var joint = graph.joints[jointKeys[j]];
        var present = [];
        for (var m = 0; m < joint.members.length; m++) {
            if (graph.byId[joint.members[m]]) present.push(joint.members[m]);
        }
        if (present.length < 2) continue;
        var owner = 0;
        var closed = true;
        for (var q = 0; q < present.length; q++) {
            var memberPhase = Number(byWall[present[q]]);
            if (!(memberPhase >= 1)) continue;
            if (!owner || memberPhase < owner) owner = memberPhase;
            if (q > 0 && byWall[present[q]] !== byWall[present[0]]) closed = false;
        }
        if (!owner || !phases[owner - 1]) continue;
        if (closed) {
            var cornerName = joint.kind === 'T' ? 'Esquina T' : (joint.kind === 'Cross' ? 'Cruce' : 'Esquina L');
            addPiece(phases[owner - 1].used, cornerName, 1);
            continue;
        }
        var cornerSplit = joint.kind === 'T' ? 'Esquina T' : (joint.kind === 'Cross' ? 'Cruce' : 'Esquina L');
        addPiece(phases[owner - 1].used, cornerSplit, 1);
        for (var arm = 0; arm < present.length; arm++) {
            var armPhase = Number(byWall[present[arm]]);
            if (!(armPhase > owner)) continue;
            var laterWall = graph.byId[present[arm]];
            var stubMm = Math.min(1350, Math.round((Number(laterWall.length) || 0) * 1000));
            addPacked(phases[owner - 1].used, laterWall, stubMm);
            addPiece(phases[owner - 1].used, 'Tape extremo', 1);
        }
    }
    var kit = {};
    for (var a = 0; a < phases.length; a++) {
        var keys = Object.keys(phases[a].used);
        for (var k = 0; k < keys.length; k++) {
            kit[keys[k]] = Math.max(kit[keys[k]] || 0, phases[a].used[keys[k]]);
        }
    }
    for (var b = 0; b < phases.length; b++) {
        phases[b].ground = {};
        var kitKeys = Object.keys(kit);
        for (var g = 0; g < kitKeys.length; g++) {
            var idle = kit[kitKeys[g]] - (phases[b].used[kitKeys[g]] || 0);
            if (idle > 0) phases[b].ground[kitKeys[g]] = idle;
        }
    }
    return kit;
}

function claimJoints(graph, byWall, wallId, phase) {
    var wall = graph.byId[wallId];
    var ends = wall && wall.conn ? wall.conn : [];
    for (var i = 0; i < ends.length; i++) {
        var joint = graph.joints[String(ends[i] || '').trim()];
        if (!joint) continue;
        for (var m = 0; m < joint.members.length; m++) {
            var member = joint.members[m];
            if (!graph.byId[member] || byWall[member]) continue;
            byWall[member] = phase;
        }
    }
}

/* No se llama. Si se usa, una esquina compartida arrastra el plano
   entero a la fase 1 y las demás quedan sin color. */
function sealSplitCorners(graph, byWall) {
    var guard = 0;
    var changed = true;
    while (changed && guard < 8) {
        guard += 1;
        changed = false;
        var keys = Object.keys(graph.joints);
        for (var j = 0; j < keys.length; j++) {
            var joint = graph.joints[keys[j]];
            var owner = 0;
            var members = [];
            for (var m = 0; m < joint.members.length; m++) {
                var id = joint.members[m];
                if (!graph.byId[id]) continue;
                members.push(id);
                var phase = Number(byWall[id]);
                if (phase >= 1 && (owner === 0 || phase < owner)) owner = phase;
            }
            if (!owner || members.length < 2) continue;
            for (var k = 0; k < members.length; k++) {
                if (Number(byWall[members[k]]) === owner) continue;
                byWall[members[k]] = owner;
                changed = true;
            }
        }
    }
}

function solveOne(walls, origins, phaseCount, strategy) {
    var graph = indexWalls(walls);
    var byWall = {};
    var usable = [];
    for (var o = 0; o < origins.length; o++) {
        if (graph.byId[origins[o]] && usable.indexOf(origins[o]) < 0) usable.push(origins[o]);
    }
    if (!usable.length && walls.length) usable.push(walls[0].id);
    for (var s = 0; s < usable.length; s++) {
        byWall[usable[s]] = 1;
        claimJoints(graph, byWall, usable[s], 1);
    }
    var total = 0;
    for (var i = 0; i < walls.length; i++) total += areaOf(walls[i]);
    var target = total / Math.max(1, phaseCount);
    growPhase(graph, byWall, 1, usable, target, strategy);
    for (var phase = 2; phase <= phaseCount; phase++) {
        var open = unassignedTouching(graph, byWall, phase - 1);
        if (!open.length) {
            var ids = Object.keys(graph.byId);
            for (var n = 0; n < ids.length; n++) {
                if (!byWall[ids[n]]) { open = [ids[n]]; break; }
            }
        }
        if (!open.length) break;
        var seed = chooseWall(graph, byWall, open, phase, usable, strategy);
        byWall[seed] = phase;
        claimJoints(graph, byWall, seed, phase);
        if (phase < phaseCount) growPhase(graph, byWall, phase, usable, target, strategy);
    }
    var rest = Object.keys(graph.byId);
    for (var r = 0; r < rest.length; r++) {
        if (!byWall[rest[r]]) byWall[rest[r]] = phaseCount;
    }
    var phases = describe(graph, byWall, phaseCount);
    var kit = fillMaterial(graph, byWall, phases);
    return {
        title: strategy.title,
        byWall: byWall,
        phases: phases,
        kit: kit
    };
}

export function solvePhasePlans(walls, origins, phaseCount) {
    var count = Math.max(1, Number(phaseCount) || 1);
    var plans = [];
    var seen = {};
    for (var i = 0; i < STRATEGIES.length; i++) {
        var plan = solveOne(walls, origins || [], count, STRATEGIES[i]);
        var sign = Object.keys(plan.byWall).sort(function (a, b) { return Number(a) - Number(b); })
            .map(function (id) { return id + ':' + plan.byWall[id]; }).join('|');
        if (seen[sign]) continue;
        seen[sign] = true;
        plans.push(plan);
    }
    return plans;
}

export { PHASE_COLORS };
