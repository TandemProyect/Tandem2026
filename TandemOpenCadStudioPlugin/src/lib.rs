use std::env;
use std::path::{Path, PathBuf};
use std::process::Command;
use std::sync::{Mutex, OnceLock};
use std::time::{SystemTime, UNIX_EPOCH};

use ocs_plugin_api::host::{
    codec, BuiltinPlugin, CommandStep, EntityType, HostApi, HostNotification, InteractiveCommand,
    LayerConfig, PreviewWire,
};
use ocs_plugin_api::manifest::{ApiVersion, PluginManifest};
use ocs_plugin_api::ribbon::{CadModule, RibbonGroup};

static MANIFEST: PluginManifest = PluginManifest {
    id: "tandem.opencadstudio",
    name: "Tandem 2026",
    version: env!("CARGO_PKG_VERSION"),
    description: "Ribbon Tandem 2026 para bridge MVC/OpenCADStudio",
    api_version: ApiVersion::CURRENT,
    ribbon_order: 40,
    xdata_apps: &[],
    command_prefixes: &["TANDEM", "TANDEM_"],
};

const CMD_TANDEM: &str = "TANDEM";
const CMD_TANDEM_2026: &str = "TANDEM_2026";
const CMD_TANDEM_MURO2D: &str = "TANDEM_MURO2D";
const CMD_TANDEM_MURO2D_FINALIZE: &str = "TANDEM_MURO2D_FINALIZE";
const BRIDGE_EXE_NAME: &str = "OpenCadStudioPluging.exe";
const WALL_LAYER_AXIS: &str = "TANDEM_MURO_EJE";
const WALL_LAYER_FACE: &str = "TANDEM_MURO_CARA";
const DEFAULT_WALL_THICKNESS: f64 = 0.30;
const EPS: f64 = 1e-8;
const TOUCH_ENDPOINT_GUARD_MIN: f64 = 1e-4;
const TOUCH_PROJECTION_TOLERANCE_MIN: f64 = 0.01;
const TOUCH_SIDE_TOLERANCE_FACTOR: f64 = 1e-6;
const WALL_STEP_PREFIX: &str = "WALL2D|";

struct TandemModule;

#[derive(Clone)]
struct PendingWall2dState {
    points: Vec<[f64; 3]>,
    thickness: f64,
    half: f64,
    group_id: i32,
    existing_axis_segments: Vec<([f64; 3], [f64; 3])>,
    finalize_requested: bool,
}

fn pending_wall2d_slot() -> &'static Mutex<Option<PendingWall2dState>> {
    static SLOT: OnceLock<Mutex<Option<PendingWall2dState>>> = OnceLock::new();
    SLOT.get_or_init(|| Mutex::new(None))
}

impl CadModule for TandemModule {
    fn id(&self) -> &'static str {
        "tandem_2026"
    }

    fn title(&self) -> &'static str {
        "Tandem 2026"
    }

    fn ribbon_groups(&self) -> &[RibbonGroup] {
        static GROUPS: std::sync::OnceLock<Vec<RibbonGroup>> = std::sync::OnceLock::new();
        GROUPS.get_or_init(Vec::new)
    }
}

struct TandemPlugin;

impl BuiltinPlugin for TandemPlugin {
    fn manifest(&self) -> &'static PluginManifest {
        &MANIFEST
    }

    fn ribbon(&self) -> Box<dyn CadModule> {
        Box::new(TandemModule)
    }

    fn dispatch(&self, host: &mut dyn HostApi, cmd: &str) -> bool {
        let normalized = normalize_command(cmd);
        if is_connect_command(&normalized) {
            execute_bridge_command(host, &["connect-ui"]);
            return true;
        }

        if is_wall2d_command(&normalized) {
            start_wall2d_command(host);
            return true;
        }

        if is_wall2d_finalize_command(&normalized) {
            finalize_pending_wall2d(host);
            return true;
        }

        false
    }

    fn on_notification(&mut self, _command_id: Option<u64>, notification: HostNotification) {
        match notification {
            HostNotification::DocumentOpened { .. } => {
                let _ = spawn_connect_ui();
            }
            HostNotification::Cancel => {
                request_finalize_on_cancel();
            }
            HostNotification::CommandStateChanged { command, .. } => {
                if command.is_none() {
                    request_finalize_on_cancel();
                }
            }
            _ => {}
        }
    }
}

fn normalize_command(cmd: &str) -> String {
    cmd.trim().to_ascii_uppercase()
}

fn is_connect_command(normalized: &str) -> bool {
    normalized == CMD_TANDEM || normalized == CMD_TANDEM_2026
}

fn is_wall2d_command(normalized: &str) -> bool {
    normalized == CMD_TANDEM_MURO2D
}

fn is_wall2d_finalize_command(normalized: &str) -> bool {
    normalized == CMD_TANDEM_MURO2D_FINALIZE
}

fn start_wall2d_command(host: &mut dyn HostApi) {
    ensure_wall_layers(host);
    let thickness = resolve_wall_thickness();
    if let Err(err) = probe_wall_step_solver(thickness) {
        host.push_error(&format!(
            "Tandem Muro2D no pudo iniciar solver C# (Desing): {}",
            trim_to_limit(&err, 700)
        ));
        return;
    }
    let group_id = generate_group_id();
    let existing_axis_segments = collect_existing_axis_segments(host);
    init_pending_wall2d_state(
        thickness,
        group_id,
        existing_axis_segments.clone(),
    );
    host.push_info(&format!(
        "Tandem Muro2D: espesor {:.3}. Punto inicial; Enter termina. Cierra tocando punto inicial.",
        thickness
    ));
    host.start_interactive(Box::new(Wall2dInteractive::new(
        thickness,
        group_id,
        existing_axis_segments,
    )));
}

fn execute_bridge_command(host: &mut dyn HostApi, args: &[&str]) {
    let executable = resolve_bridge_executable();

    if !args.is_empty() && args[0] == "connect-ui" {
        let mut command = build_bridge_command(&executable);
        command.args(args);
        match command.spawn() {
            Ok(_) => {
                host.push_info(&format!(
                    "Tandem 2026: abriendo barras MVC ({})",
                    executable.display()
                ));
            }
            Err(ex) => {
                host.push_error(&format!(
                    "No se pudo abrir la UI Tandem con {}. Motivo: {}",
                    executable.display(),
                    ex
                ));
            }
        }
        return;
    }

    let mut command = build_bridge_command(&executable);
    command.args(args);

    match command.output() {
        Ok(output) => {
            let stdout = String::from_utf8_lossy(&output.stdout);
            let stderr = String::from_utf8_lossy(&output.stderr);
            if output.status.success() {
                let message = prefer_message(&stdout, &stderr)
                    .unwrap_or_else(|| "Comando ejecutado.".to_string());
                host.push_info(&format!(
                    "Tandem 2026: {} ({})",
                    trim_to_limit(&message, 700),
                    executable.display()
                ));
                return;
            }

            let detail = prefer_message(&stderr, &stdout)
                .unwrap_or_else(|| "Sin detalle de error.".to_string());
            host.push_error(&format!(
                "Tandem 2026 bridge fallo ({:?}) {}: {}",
                output.status.code(),
                executable.display(),
                trim_to_limit(&detail, 1200)
            ));
        }
        Err(ex) => {
            host.push_error(&format!(
                "No se pudo iniciar {}. Configura TANDEM_OPENCAD_BRIDGE o coloca el bridge en PATH. Motivo: {}",
                executable.display(),
                ex
            ));
        }
    }
}

fn init_pending_wall2d_state(
    thickness: f64,
    group_id: i32,
    existing_axis_segments: Vec<([f64; 3], [f64; 3])>,
) {
    if let Ok(mut slot) = pending_wall2d_slot().lock() {
        *slot = Some(PendingWall2dState {
            points: Vec::new(),
            thickness,
            half: thickness * 0.5,
            group_id,
            existing_axis_segments,
            finalize_requested: false,
        });
    }
}

fn sync_pending_wall2d_points(points: &[[f64; 3]]) {
    if let Ok(mut slot) = pending_wall2d_slot().lock() {
        if let Some(state) = slot.as_mut() {
            state.points = points.to_vec();
        }
    }
}

fn clear_pending_wall2d_state() {
    if let Ok(mut slot) = pending_wall2d_slot().lock() {
        *slot = None;
    }
}

fn request_finalize_on_cancel() {
    let should_spawn = if let Ok(mut slot) = pending_wall2d_slot().lock() {
        if let Some(state) = slot.as_mut() {
            if state.points.len() < 2 {
                *slot = None;
                false
            } else if state.finalize_requested {
                false
            } else {
                state.finalize_requested = true;
                true
            }
        } else {
            false
        }
    } else {
        false
    };

    if !should_spawn {
        return;
    }

    if spawn_finalize_wall2d_command().is_err() {
        if let Ok(mut slot) = pending_wall2d_slot().lock() {
            if let Some(state) = slot.as_mut() {
                state.finalize_requested = false;
            }
        }
    }
}

fn take_pending_wall2d_for_finalize() -> Option<PendingWall2dState> {
    let mut slot = pending_wall2d_slot().lock().ok()?;
    let should_take = match slot.as_ref() {
        Some(state) if state.finalize_requested && state.points.len() >= 2 => true,
        _ => false,
    };
    if !should_take {
        return None;
    }
    slot.take()
}

fn spawn_finalize_wall2d_command() -> std::io::Result<()> {
    let executable = resolve_bridge_executable();
    let mut command = build_bridge_command(&executable);
    command.args(["ui-run", CMD_TANDEM_MURO2D_FINALIZE]);
    command.spawn().map(|_| ())
}

fn finalize_pending_wall2d(host: &mut dyn HostApi) {
    let Some(state) = take_pending_wall2d_for_finalize() else {
        return;
    };

    let entities = match rebuild_final_wall_entities(&state) {
        Ok(items) => items,
        Err(err) => {
            host.push_error(&format!(
                "Tandem Muro2D: no se pudo finalizar tras ESC: {}",
                trim_to_limit(&err, 700)
            ));
            return;
        }
    };

    ensure_wall_layers(host);
    host.push_undo("Tandem Muro2D finalize ESC");
    replace_group_entities(host, state.group_id, entities);
    apply_touch_face_cuts(host, &state);
    host.bump_geometry();
    host.set_dirty();
    host.push_info("Tandem Muro2D: revisión/finalización aplicada tras ESC.");
}

fn rebuild_final_wall_entities(state: &PendingWall2dState) -> Result<Vec<EntityType>, String> {
    if state.points.len() < 2 {
        return Ok(Vec::new());
    }

    let mut all = Vec::new();
    for idx in 1..state.points.len() {
        let base = &state.points[..idx];
        let next = state.points[idx];
        let reserve_end = idx < state.points.len() - 1;
        let mut step = resolve_wall_step_entities(
            base,
            next,
            state.half,
            state.thickness,
            reserve_end,
            state.group_id,
            &state.existing_axis_segments,
        )?;
        all.append(&mut step);
    }
    Ok(all)
}

fn replace_group_entities(host: &mut dyn HostApi, group_id: i32, entities: Vec<EntityType>) {
    let handles = collect_group_handles(host, group_id);
    for handle in handles {
        let _ = host.remove_entity(handle);
    }
    if !entities.is_empty() {
        let _ = host.add_entities(entities);
    }
}

fn collect_group_handles(host: &mut dyn HostApi, group_id: i32) -> Vec<codec::Handle> {
    let mut handles = Vec::new();
    for entity in host.document().entities() {
        let EntityType::Line(line) = entity else {
            continue;
        };
        if tandem_group_id(line) != Some(group_id) {
            continue;
        }
        handles.push(line.common.handle);
    }
    handles
}

fn tandem_group_id(line: &codec::Line) -> Option<i32> {
    let record = line.common.extended_data.get_record("TANDEM")?;
    for value in &record.values {
        if let codec::xdata::XDataValue::Integer32(group_id) = value {
            return Some(*group_id);
        }
    }
    None
}

fn apply_touch_face_cuts(host: &mut dyn HostApi, state: &PendingWall2dState) {
    if state.points.len() < 2 || state.existing_axis_segments.is_empty() {
        return;
    }

    for idx in 0..(state.points.len() - 1) {
        let start = state.points[idx];
        let end = state.points[idx + 1];
        if idx == 0 {
            cut_touch_face_at_point(
                host,
                start,
                end,
                state.half,
                state.group_id,
                &state.existing_axis_segments,
            );
        }
        cut_touch_face_at_point(
            host,
            end,
            start,
            state.half,
            state.group_id,
            &state.existing_axis_segments,
        );
    }
}

fn cut_touch_face_at_point(
    host: &mut dyn HostApi,
    touch_point: [f64; 3],
    probe_point: [f64; 3],
    half: f64,
    active_group_id: i32,
    existing_axis_segments: &[([f64; 3], [f64; 3])],
) {
    let Some((projected_touch, axis_a, axis_b)) =
        find_axis_touch_candidate(touch_point, existing_axis_segments, half)
    else {
        return;
    };
    let Some((face_a, face_b)) =
        choose_touch_face_line(axis_a, axis_b, half, projected_touch, probe_point)
    else {
        return;
    };
    let Some((cut_a, cut_b)) =
        compute_cut_interval(projected_touch, probe_point, half, face_a, face_b)
    else {
        return;
    };

    let _ = split_host_face_line(
        host,
        face_a,
        face_b,
        cut_a,
        cut_b,
        projected_touch,
        active_group_id,
    );
}

fn find_axis_touch_candidate(
    point: [f64; 3],
    segments: &[([f64; 3], [f64; 3])],
    half: f64,
) -> Option<([f64; 3], [f64; 3], [f64; 3])> {
    let endpoint_guard = (half * 0.2).max(TOUCH_ENDPOINT_GUARD_MIN);
    let snap_tolerance = (half * 0.35).max(TOUCH_PROJECTION_TOLERANCE_MIN);
    let mut best: Option<([f64; 3], [f64; 3], [f64; 3], f64)> = None;

    for (axis_a, axis_b) in segments {
        let dx = axis_b[0] - axis_a[0];
        let dy = axis_b[1] - axis_a[1];
        let len2 = dx * dx + dy * dy;
        if len2 < EPS {
            continue;
        }

        let t = ((point[0] - axis_a[0]) * dx + (point[1] - axis_a[1]) * dy) / len2;
        let projected = [axis_a[0] + dx * t, axis_a[1] + dy * t, point[2]];
        let dist = distance2d(point, projected);
        if dist > snap_tolerance {
            continue;
        }
        if !point_on_segment_interior_2d(projected, *axis_a, *axis_b, endpoint_guard) {
            continue;
        }

        match best {
            Some((_, _, _, best_dist)) if best_dist <= dist => {}
            _ => best = Some((projected, *axis_a, *axis_b, dist)),
        }
    }

    best.map(|(projected, axis_a, axis_b, _)| (projected, axis_a, axis_b))
}

fn point_on_segment_interior_2d(
    point: [f64; 3],
    a: [f64; 3],
    b: [f64; 3],
    endpoint_guard: f64,
) -> bool {
    let len = distance2d(a, b);
    if len < EPS {
        return false;
    }

    let mut guard = endpoint_guard.max(1e-6);
    guard = guard.min(len * 0.49);
    if distance2d(point, a) < guard || distance2d(point, b) < guard {
        return false;
    }

    let dx = b[0] - a[0];
    let dy = b[1] - a[1];
    let cross = (point[0] - a[0]) * dy - (point[1] - a[1]) * dx;
    if cross.abs() > 1e-5 * len {
        return false;
    }

    let dot = (point[0] - a[0]) * dx + (point[1] - a[1]) * dy;
    let along = dot / len;
    along > guard && along < len - guard
}

fn choose_touch_face_line(
    touch_a: [f64; 3],
    touch_b: [f64; 3],
    half: f64,
    touch_point: [f64; 3],
    probe_point: [f64; 3],
) -> Option<([f64; 3], [f64; 3])> {
    let dx = touch_b[0] - touch_a[0];
    let dy = touch_b[1] - touch_a[1];
    let len = (dx * dx + dy * dy).sqrt();
    if len < EPS {
        return None;
    }

    let side = dx * (probe_point[1] - touch_point[1]) - dy * (probe_point[0] - touch_point[0]);
    let tol = TOUCH_SIDE_TOLERANCE_FACTOR * len;
    if side.abs() <= tol {
        return None;
    }

    let n = perp2d(touch_a, touch_b, half);
    if side > 0.0 {
        Some((add(touch_a, n), add(touch_b, n)))
    } else {
        Some((sub(touch_a, n), sub(touch_b, n)))
    }
}

fn compute_cut_interval(
    touch_point: [f64; 3],
    probe_point: [f64; 3],
    half: f64,
    face_a: [f64; 3],
    face_b: [f64; 3],
) -> Option<([f64; 3], [f64; 3])> {
    if distance2d(touch_point, probe_point) < EPS {
        return None;
    }

    let n = perp2d(touch_point, probe_point, half);
    let plus_touch = add(touch_point, n);
    let plus_probe = add(probe_point, n);
    let minus_touch = sub(touch_point, n);
    let minus_probe = sub(probe_point, n);

    let hit_plus = try_intersect2d(plus_touch, plus_probe, face_a, face_b)?;
    let hit_minus = try_intersect2d(minus_touch, minus_probe, face_a, face_b)?;
    if distance2d(hit_plus, hit_minus) < EPS {
        return None;
    }

    let Some(t_plus) = segment_param(face_a, face_b, hit_plus, 1e-4) else {
        return None;
    };
    let Some(t_minus) = segment_param(face_a, face_b, hit_minus, 1e-4) else {
        return None;
    };

    if t_plus <= t_minus {
        Some((hit_plus, hit_minus))
    } else {
        Some((hit_minus, hit_plus))
    }
}

fn split_host_face_line(
    host: &mut dyn HostApi,
    face_a: [f64; 3],
    face_b: [f64; 3],
    cut_a: [f64; 3],
    cut_b: [f64; 3],
    touch_point: [f64; 3],
    active_group_id: i32,
) -> bool {
    let mut best: Option<(codec::Handle, codec::Line, f64, f64, f64)> = None;

    for entity in host.document().entities() {
        let EntityType::Line(line) = entity else {
            continue;
        };
        if !line.common.layer.eq_ignore_ascii_case(WALL_LAYER_FACE) {
            continue;
        }
        if tandem_group_id(line) == Some(active_group_id) {
            continue;
        }

        let start = [line.start.x, line.start.y, line.start.z];
        let end = [line.end.x, line.end.y, line.end.z];
        if !is_same_support_line(face_a, face_b, start, end) {
            continue;
        }

        let Some(mut t1) = segment_param(start, end, cut_a, 1e-3) else {
            continue;
        };
        let Some(mut t2) = segment_param(start, end, cut_b, 1e-3) else {
            continue;
        };
        if t1 > t2 {
            std::mem::swap(&mut t1, &mut t2);
        }
        if t2 - t1 < 1e-6 {
            continue;
        }

        let score = distance_point_to_segment_2d(touch_point, start, end);
        match &best {
            Some((_, _, _, _, best_score)) if *best_score <= score => {}
            _ => best = Some((line.common.handle, line.clone(), t1, t2, score)),
        }
    }

    let Some((handle, template, t1, t2, _)) = best else {
        return false;
    };
    let start = [template.start.x, template.start.y, template.start.z];
    let end = [template.end.x, template.end.y, template.end.z];
    let cut_start = lerp(start, end, t1);
    let cut_end = lerp(start, end, t2);

    let _ = host.remove_entity(handle);
    if t1 > 1e-6 {
        let left = clone_line_with_points(&template, start, cut_start);
        let _ = host.add_entity(EntityType::Line(left));
    }
    if t2 < 1.0 - 1e-6 {
        let right = clone_line_with_points(&template, cut_end, end);
        let _ = host.add_entity(EntityType::Line(right));
    }

    true
}

fn clone_line_with_points(template: &codec::Line, start: [f64; 3], end: [f64; 3]) -> codec::Line {
    let mut line = template.clone();
    line.common.handle = codec::Handle::NULL;
    line.start.x = start[0];
    line.start.y = start[1];
    line.start.z = start[2];
    line.end.x = end[0];
    line.end.y = end[1];
    line.end.z = end[2];
    line
}

fn is_same_support_line(
    ref_a: [f64; 3],
    ref_b: [f64; 3],
    cand_a: [f64; 3],
    cand_b: [f64; 3],
) -> bool {
    let ref_dx = ref_b[0] - ref_a[0];
    let ref_dy = ref_b[1] - ref_a[1];
    let cand_dx = cand_b[0] - cand_a[0];
    let cand_dy = cand_b[1] - cand_a[1];
    let ref_len = (ref_dx * ref_dx + ref_dy * ref_dy).sqrt();
    let cand_len = (cand_dx * cand_dx + cand_dy * cand_dy).sqrt();
    if ref_len < EPS || cand_len < EPS {
        return false;
    }

    let cross_dir = (ref_dx * cand_dy - ref_dy * cand_dx).abs();
    if cross_dir > 1e-6 * ref_len * cand_len {
        return false;
    }

    let dist_a = distance_point_to_line_2d(cand_a, ref_a, ref_b);
    let dist_b = distance_point_to_line_2d(cand_b, ref_a, ref_b);
    dist_a <= 1e-3 && dist_b <= 1e-3
}

fn segment_param(a: [f64; 3], b: [f64; 3], p: [f64; 3], tolerance: f64) -> Option<f64> {
    let dx = b[0] - a[0];
    let dy = b[1] - a[1];
    let len2 = dx * dx + dy * dy;
    if len2 < EPS {
        return None;
    }

    let t = ((p[0] - a[0]) * dx + (p[1] - a[1]) * dy) / len2;
    if t < -tolerance || t > 1.0 + tolerance {
        return None;
    }

    let proj = [a[0] + dx * t, a[1] + dy * t, p[2]];
    if distance2d(proj, p) > tolerance {
        return None;
    }

    Some(t.clamp(0.0, 1.0))
}

fn distance_point_to_line_2d(p: [f64; 3], a: [f64; 3], b: [f64; 3]) -> f64 {
    let dx = b[0] - a[0];
    let dy = b[1] - a[1];
    let len = (dx * dx + dy * dy).sqrt();
    if len < EPS {
        return distance2d(p, a);
    }
    ((dx * (p[1] - a[1])) - (dy * (p[0] - a[0]))).abs() / len
}

fn distance_point_to_segment_2d(p: [f64; 3], a: [f64; 3], b: [f64; 3]) -> f64 {
    let dx = b[0] - a[0];
    let dy = b[1] - a[1];
    let len2 = dx * dx + dy * dy;
    if len2 < EPS {
        return distance2d(p, a);
    }
    let t = (((p[0] - a[0]) * dx + (p[1] - a[1]) * dy) / len2).clamp(0.0, 1.0);
    let proj = [a[0] + dx * t, a[1] + dy * t, p[2]];
    distance2d(p, proj)
}

fn lerp(a: [f64; 3], b: [f64; 3], t: f64) -> [f64; 3] {
    [
        a[0] + (b[0] - a[0]) * t,
        a[1] + (b[1] - a[1]) * t,
        a[2] + (b[2] - a[2]) * t,
    ]
}

#[cfg(test)]
fn split_finalize_chain(points: &[[f64; 3]]) -> Option<(Vec<[f64; 3]>, [f64; 3])> {
    if points.len() < 2 {
        return None;
    }
    let next = *points.last()?;
    let base = points[..points.len() - 1].to_vec();
    if base.is_empty() {
        return None;
    }
    Some((base, next))
}

fn ensure_wall_layers(host: &mut dyn HostApi) {
    let _ = host.add_layer(LayerConfig {
        name: WALL_LAYER_AXIS.to_string(),
        color: Some(codec::types::Color::Index(8)),
        ..LayerConfig::default()
    });
    let _ = host.add_layer(LayerConfig {
        name: WALL_LAYER_FACE.to_string(),
        color: Some(codec::types::Color::Index(7)),
        ..LayerConfig::default()
    });
}

fn resolve_wall_thickness() -> f64 {
    DEFAULT_WALL_THICKNESS
}

fn probe_wall_step_solver(thickness: f64) -> Result<(), String> {
    let seed = [[0.0, 0.0, 0.0]];
    request_wall_step_entities(&seed, [1.0, 0.0, 0.0], thickness, false, 0, &[]).map(|_| ())
}

fn collect_existing_axis_segments(host: &mut dyn HostApi) -> Vec<([f64; 3], [f64; 3])> {
    let mut segments = Vec::new();
    for entity in host.document().entities() {
        let EntityType::Line(line) = entity else {
            continue;
        };
        if !line.common.layer.eq_ignore_ascii_case(WALL_LAYER_AXIS) {
            continue;
        }
        let a = [line.start.x, line.start.y, line.start.z];
        let b = [line.end.x, line.end.y, line.end.z];
        if distance2d(a, b) < EPS {
            continue;
        }
        segments.push((a, b));
    }
    segments
}

fn generate_group_id() -> i32 {
    let millis = SystemTime::now()
        .duration_since(UNIX_EPOCH)
        .map(|it| it.as_millis() as u64)
        .unwrap_or(1);
    (millis & 0x7fff_ffff) as i32
}

fn spawn_connect_ui() -> std::io::Result<()> {
    let executable = resolve_bridge_executable();
    let mut command = build_bridge_command(&executable);
    command.arg("connect-ui");
    command.spawn().map(|_| ())
}

fn resolve_bridge_executable() -> PathBuf {
    for key in ["TANDEM_OPENCAD_BRIDGE", "TANDEM_OPENCADSTUDIO_BRIDGE"] {
        if let Some(value) = non_empty_env(key) {
            return PathBuf::from(value);
        }
    }

    if let Some(path) = resolve_from_current_exe() {
        return path;
    }

    if let Some(path) = resolve_from_appdata() {
        return path;
    }

    PathBuf::from(BRIDGE_EXE_NAME)
}

fn resolve_from_current_exe() -> Option<PathBuf> {
    let current_exe = env::current_exe().ok()?;
    let current_dir = current_exe.parent()?;

    let same_dir = current_dir.join(BRIDGE_EXE_NAME);
    if same_dir.is_file() {
        return Some(same_dir);
    }

    let sibling_host = current_dir.join("Host").join(BRIDGE_EXE_NAME);
    if sibling_host.is_file() {
        return Some(sibling_host);
    }

    None
}

fn resolve_from_appdata() -> Option<PathBuf> {
    let appdata = env::var_os("APPDATA")?;
    let candidate = Path::new(&appdata)
        .join("Tandem")
        .join("OpenCadStudioPluging")
        .join("Host")
        .join(BRIDGE_EXE_NAME);
    if candidate.is_file() {
        return Some(candidate);
    }
    None
}

fn non_empty_env(key: &str) -> Option<String> {
    let value = env::var(key).ok()?;
    let trimmed = value.trim();
    if trimmed.is_empty() {
        return None;
    }
    Some(trimmed.to_string())
}

fn prefer_message(primary: &str, fallback: &str) -> Option<String> {
    sanitize_message(primary).or_else(|| sanitize_message(fallback))
}

fn sanitize_message(raw: &str) -> Option<String> {
    let compact = raw
        .lines()
        .map(str::trim)
        .filter(|line| !line.is_empty())
        .collect::<Vec<_>>()
        .join(" | ");
    if compact.is_empty() {
        return None;
    }
    Some(compact)
}

fn build_bridge_command(executable: &Path) -> Command {
    let mut command = Command::new(executable);
    #[cfg(target_os = "windows")]
    {
        use std::os::windows::process::CommandExt;
        const CREATE_NO_WINDOW: u32 = 0x08000000;
        command.creation_flags(CREATE_NO_WINDOW);
    }
    command
}

fn trim_to_limit(message: &str, limit: usize) -> String {
    if message.chars().count() <= limit {
        return message.to_string();
    }
    let mut trimmed = String::new();
    for (index, ch) in message.chars().enumerate() {
        if index >= limit.saturating_sub(1) {
            break;
        }
        trimmed.push(ch);
    }
    trimmed.push('…');
    trimmed
}

#[derive(Clone)]
#[cfg(test)]
struct WallSegment {
    axis_a: [f64; 3],
    axis_b: [f64; 3],
    plus_a: [f64; 3],
    plus_b: [f64; 3],
    minus_a: [f64; 3],
    minus_b: [f64; 3],
}

struct Wall2dInteractive {
    points: Vec<[f64; 3]>,
    thickness: f64,
    half: f64,
    group_id: i32,
    existing_axis_segments: Vec<([f64; 3], [f64; 3])>,
}

impl Wall2dInteractive {
    fn new(
        thickness: f64,
        group_id: i32,
        existing_axis_segments: Vec<([f64; 3], [f64; 3])>,
    ) -> Self {
        Self {
            points: Vec::new(),
            thickness,
            half: thickness * 0.5,
            group_id,
            existing_axis_segments,
        }
    }
}

impl InteractiveCommand for Wall2dInteractive {
    fn prompt(&self) -> String {
        if self.points.is_empty() {
            "Punto inicial del muro:".to_string()
        } else if self.points.len() >= 3 {
            "Punto siguiente del muro [toca punto inicial para cerrar, Enter para terminar]:"
                .to_string()
        } else {
            "Punto siguiente del muro:".to_string()
        }
    }

    fn on_point(&mut self, pt: [f64; 3]) -> CommandStep {
        if self.points.is_empty() {
            self.points.push(pt);
            sync_pending_wall2d_points(&self.points);
            return CommandStep::NeedPoint;
        }

        let current = *self.points.last().unwrap_or(&pt);
        if distance2d(current, pt) < EPS {
            return CommandStep::NeedPoint;
        }

        let start = self.points[0];
        if self.points.len() >= 2 && distance2d(start, pt) < EPS {
            let entities = match resolve_wall_step_entities(
                &self.points,
                start,
                self.half,
                self.thickness,
                false,
                self.group_id,
                &self.existing_axis_segments,
            ) {
                Ok(items) => items,
                Err(_) => {
                    clear_pending_wall2d_state();
                    return CommandStep::Cancel;
                }
            };

            if entities.is_empty() {
                clear_pending_wall2d_state();
                return CommandStep::Done;
            }
            self.points.push(start);
            clear_pending_wall2d_state();
            return CommandStep::CommitManyAndEnd(entities);
        }

        let entities = match resolve_wall_step_entities(
            &self.points,
            pt,
            self.half,
            self.thickness,
            true,
            self.group_id,
            &self.existing_axis_segments,
        ) {
            Ok(items) => items,
            Err(_) => {
                clear_pending_wall2d_state();
                return CommandStep::Cancel;
            }
        };

        self.points.push(pt);
        sync_pending_wall2d_points(&self.points);
        if entities.is_empty() {
            CommandStep::NeedPoint
        } else {
            CommandStep::CommitMany(entities)
        }
    }

    fn on_enter(&mut self) -> CommandStep {
        if self.points.len() < 2 {
            clear_pending_wall2d_state();
            return CommandStep::Done;
        }
        request_finalize_on_cancel();
        CommandStep::Done
    }

    fn on_cursor_move(&mut self, pt: [f64; 3]) -> Vec<PreviewWire> {
        let Some(&last_fixed) = self.points.last() else {
            return Vec::new();
        };

        if distance2d(last_fixed, pt) < EPS {
            return Vec::new();
        }
        build_wall_preview_segment(last_fixed, pt, self.half)
    }
}

fn build_wall_preview_segment(a: [f64; 3], b: [f64; 3], half: f64) -> Vec<PreviewWire> {
    let n = perp2d(a, b, half);
    let plus_a = add(a, n);
    let plus_b = add(b, n);
    let minus_a = sub(a, n);
    let minus_b = sub(b, n);
    vec![
        PreviewWire::line(a, b, None),
        PreviewWire::line(plus_a, plus_b, Some([0.2, 0.9, 1.0, 1.0])),
        PreviewWire::line(minus_a, minus_b, Some([0.2, 0.9, 1.0, 1.0])),
    ]
}

#[cfg(test)]
fn build_wall_step_entities_local(
    points: &[[f64; 3]],
    next: [f64; 3],
    half: f64,
    thickness: f64,
    group_id: i32,
    reserve_end: bool,
) -> Vec<EntityType> {
    let Some(&current) = points.last() else {
        return Vec::new();
    };

    let mut entities = build_wall_segment_entities(
        current,
        next,
        half,
        thickness,
        group_id,
        reserve_end,
    );
    if let Some(&prev_start) = points.get(points.len().saturating_sub(2)) {
        entities.extend(build_corner_join_entities(
            prev_start, current, current, next, half, thickness, group_id,
        ));
    }
    entities
}

#[cfg(test)]
fn resolve_wall_step_entities(
    points: &[[f64; 3]],
    next: [f64; 3],
    half: f64,
    thickness: f64,
    _reserve_end_for_next_step: bool,
    group_id: i32,
    _existing_axis_segments: &[([f64; 3], [f64; 3])],
) -> Result<Vec<EntityType>, String> {
    Ok(build_wall_step_entities_local(
        points, next, half, thickness, group_id, false,
    ))
}

#[cfg(not(test))]
fn resolve_wall_step_entities(
    points: &[[f64; 3]],
    next: [f64; 3],
    _half: f64,
    thickness: f64,
    reserve_end_for_next_step: bool,
    group_id: i32,
    existing_axis_segments: &[([f64; 3], [f64; 3])],
) -> Result<Vec<EntityType>, String> {
    request_wall_step_entities(
        points,
        next,
        thickness,
        reserve_end_for_next_step,
        group_id,
        existing_axis_segments,
    )
}

fn request_wall_step_entities(
    points: &[[f64; 3]],
    next: [f64; 3],
    thickness: f64,
    reserve_end_for_next_step: bool,
    group_id: i32,
    existing_axis_segments: &[([f64; 3], [f64; 3])],
) -> Result<Vec<EntityType>, String> {
    if points.is_empty() {
        return Ok(Vec::new());
    }

    let mut chain: Vec<[f64; 3]> = points.to_vec();
    chain.push(next);
    let points_arg = encode_points_arg(&chain);
    let segments_arg = encode_axis_segments_arg(existing_axis_segments);
    let thickness_arg = format_float(thickness);
    let reserve_arg = if reserve_end_for_next_step { "1" } else { "0" };

    let executable = resolve_bridge_executable();
    let mut command = build_bridge_command(&executable);
    if segments_arg.is_empty() {
        command.args(["wall-step", thickness_arg.as_str(), reserve_arg, points_arg.as_str()]);
    } else {
        command.args([
            "wall-step",
            thickness_arg.as_str(),
            reserve_arg,
            points_arg.as_str(),
            segments_arg.as_str(),
        ]);
    }

    let output = command
        .output()
        .map_err(|e| format!("no se pudo ejecutar wall-step: {e}"))?;

    if !output.status.success() {
        let stderr = String::from_utf8_lossy(&output.stderr);
        let stdout = String::from_utf8_lossy(&output.stdout);
        let detail =
            prefer_message(&stderr, &stdout).unwrap_or_else(|| "sin detalle de error".to_string());
        return Err(format!("wall-step fallo: {detail}"));
    }

    let stdout = String::from_utf8_lossy(&output.stdout);
    let mut entities = Vec::new();
    for raw in stdout.lines() {
        let line = raw.trim();
        if line.is_empty() {
            continue;
        }
        if !line.starts_with(WALL_STEP_PREFIX) {
            continue;
        }
        if let Some(entity) = parse_wall_step_line(line, thickness, group_id)? {
            entities.push(entity);
        }
    }

    Ok(entities)
}

fn parse_wall_step_line(line: &str, thickness: f64, group_id: i32) -> Result<Option<EntityType>, String> {
    let parts: Vec<&str> = line.split('|').collect();
    if parts.len() != 8 {
        return Err(format!("formato wall-step inválido: {line}"));
    }

    let role = if parts[1].trim().eq_ignore_ascii_case("axis") {
        "axis"
    } else {
        "face"
    };
    let layer = if role == "axis" {
        WALL_LAYER_AXIS
    } else {
        WALL_LAYER_FACE
    };

    let x1 = parse_float(parts[2])?;
    let y1 = parse_float(parts[3])?;
    let z1 = parse_float(parts[4])?;
    let x2 = parse_float(parts[5])?;
    let y2 = parse_float(parts[6])?;
    let z2 = parse_float(parts[7])?;

    let start = [x1, y1, z1];
    let end = [x2, y2, z2];
    if distance2d(start, end) < EPS {
        return Ok(None);
    }

    Ok(Some(build_line_entity(
        start, end, layer, role, group_id, thickness,
    )))
}

fn parse_float(raw: &str) -> Result<f64, String> {
    raw.trim()
        .parse::<f64>()
        .map_err(|_| format!("número inválido en wall-step: {raw}"))
}

fn encode_points_arg(points: &[[f64; 3]]) -> String {
    points
        .iter()
        .map(|pt| {
            format!(
                "{},{},{}",
                format_float(pt[0]),
                format_float(pt[1]),
                format_float(pt[2])
            )
        })
        .collect::<Vec<_>>()
        .join(";")
}

fn encode_axis_segments_arg(segments: &[([f64; 3], [f64; 3])]) -> String {
    segments
        .iter()
        .map(|(a, b)| {
            format!(
                "{},{},{},{},{},{}",
                format_float(a[0]),
                format_float(a[1]),
                format_float(a[2]),
                format_float(b[0]),
                format_float(b[1]),
                format_float(b[2])
            )
        })
        .collect::<Vec<_>>()
        .join(";")
}

fn format_float(value: f64) -> String {
    format!("{value:.15}")
}

#[cfg(test)]
fn build_wall_segment_entities(
    a: [f64; 3],
    b: [f64; 3],
    half: f64,
    thickness: f64,
    group_id: i32,
    reserve_end: bool,
) -> Vec<EntityType> {
    if distance2d(a, b) < EPS {
        return Vec::new();
    }

    let n = perp2d(a, b, half);
    let plus_a = add(a, n);
    let mut plus_b = add(b, n);
    let minus_a = sub(a, n);
    let mut minus_b = sub(b, n);

    if reserve_end {
        let len = distance2d(a, b);
        if len > EPS {
            let reserve = half.min(len * 0.5);
            let ux = (b[0] - a[0]) / len;
            let uy = (b[1] - a[1]) / len;
            plus_b = [plus_b[0] - ux * reserve, plus_b[1] - uy * reserve, plus_b[2]];
            minus_b = [minus_b[0] - ux * reserve, minus_b[1] - uy * reserve, minus_b[2]];
        }
    }

    vec![
        build_line_entity(a, b, WALL_LAYER_AXIS, "axis", group_id, thickness),
        build_line_entity(plus_a, plus_b, WALL_LAYER_FACE, "face", group_id, thickness),
        build_line_entity(
            minus_a,
            minus_b,
            WALL_LAYER_FACE,
            "face",
            group_id,
            thickness,
        ),
    ]
}

#[cfg(test)]
fn build_corner_join_entities(
    prev_a: [f64; 3],
    prev_b: [f64; 3],
    curr_a: [f64; 3],
    curr_b: [f64; 3],
    half: f64,
    thickness: f64,
    group_id: i32,
) -> Vec<EntityType> {
    if distance2d(prev_a, prev_b) < EPS || distance2d(curr_a, curr_b) < EPS {
        return Vec::new();
    }

    let prev_n = perp2d(prev_a, prev_b, half);
    let curr_n = perp2d(curr_a, curr_b, half);

    let prev_plus_a = add(prev_a, prev_n);
    let prev_plus_b = add(prev_b, prev_n);
    let curr_plus_a = add(curr_a, curr_n);
    let curr_plus_b = add(curr_b, curr_n);

    let prev_minus_a = sub(prev_a, prev_n);
    let prev_minus_b = sub(prev_b, prev_n);
    let curr_minus_a = sub(curr_a, curr_n);
    let curr_minus_b = sub(curr_b, curr_n);

    let mut entities = Vec::new();

    if let Some(miter_plus) = try_intersect(prev_plus_a, prev_plus_b, curr_plus_a, curr_plus_b) {
        append_face_if_non_zero(&mut entities, prev_plus_b, miter_plus, thickness, group_id);
        append_face_if_non_zero(&mut entities, curr_plus_a, miter_plus, thickness, group_id);
    }

    if let Some(miter_minus) = try_intersect(prev_minus_a, prev_minus_b, curr_minus_a, curr_minus_b)
    {
        append_face_if_non_zero(
            &mut entities,
            prev_minus_b,
            miter_minus,
            thickness,
            group_id,
        );
        append_face_if_non_zero(
            &mut entities,
            curr_minus_a,
            miter_minus,
            thickness,
            group_id,
        );
    }

    entities
}

#[cfg(test)]
fn append_face_if_non_zero(
    entities: &mut Vec<EntityType>,
    a: [f64; 3],
    b: [f64; 3],
    thickness: f64,
    group_id: i32,
) {
    if distance2d(a, b) < EPS {
        return;
    }
    entities.push(build_line_entity(
        a,
        b,
        WALL_LAYER_FACE,
        "face",
        group_id,
        thickness,
    ));
}

#[cfg(test)]
fn build_wall_entities(
    points: &[[f64; 3]],
    half: f64,
    thickness: f64,
    group_id: i32,
) -> Vec<EntityType> {
    let segments = build_wall_segments(points, half);
    if segments.is_empty() {
        return Vec::new();
    }

    let mut entities = Vec::with_capacity(segments.len() * 3);
    for segment in segments {
        entities.push(build_line_entity(
            segment.axis_a,
            segment.axis_b,
            WALL_LAYER_AXIS,
            "axis",
            group_id,
            thickness,
        ));
        entities.push(build_line_entity(
            segment.plus_a,
            segment.plus_b,
            WALL_LAYER_FACE,
            "face",
            group_id,
            thickness,
        ));
        entities.push(build_line_entity(
            segment.minus_a,
            segment.minus_b,
            WALL_LAYER_FACE,
            "face",
            group_id,
            thickness,
        ));
    }
    entities
}

#[cfg(test)]
fn build_wall_segments(points: &[[f64; 3]], half: f64) -> Vec<WallSegment> {
    if points.len() < 2 {
        return Vec::new();
    }

    let mut segments: Vec<WallSegment> = Vec::new();

    for pair in points.windows(2) {
        let a = pair[0];
        let b = pair[1];
        if distance2d(a, b) < EPS {
            continue;
        }

        let n = perp2d(a, b, half);
        let mut plus_a = add(a, n);
        let plus_b = add(b, n);
        let mut minus_a = sub(a, n);
        let minus_b = sub(b, n);

        if let Some(prev) = segments.last_mut() {
            if let Some(hit) = try_intersect(prev.plus_a, prev.plus_b, plus_a, plus_b) {
                prev.plus_b = hit;
                plus_a = hit;
            }
            if let Some(hit) = try_intersect(prev.minus_a, prev.minus_b, minus_a, minus_b) {
                prev.minus_b = hit;
                minus_a = hit;
            }
        }

        segments.push(WallSegment {
            axis_a: a,
            axis_b: b,
            plus_a,
            plus_b,
            minus_a,
            minus_b,
        });
    }

    segments
}

fn build_line_entity(
    start: [f64; 3],
    end: [f64; 3],
    layer: &str,
    role: &str,
    group_id: i32,
    thickness: f64,
) -> EntityType {
    let mut line = codec::Line::from_coords(start[0], start[1], start[2], end[0], end[1], end[2]);
    line.common.layer = layer.to_string();
    let mut record = codec::xdata::ExtendedDataRecord::new("TANDEM");
    record.add_value(codec::xdata::XDataValue::String(role.to_string()));
    record.add_value(codec::xdata::XDataValue::Integer32(group_id));
    record.add_value(codec::xdata::XDataValue::Real(thickness));
    line.common.extended_data.add_record(record);
    EntityType::Line(line)
}

fn perp2d(a: [f64; 3], b: [f64; 3], half: f64) -> [f64; 3] {
    let dx = b[0] - a[0];
    let dy = b[1] - a[1];
    let len = (dx * dx + dy * dy).sqrt();
    if len < 1e-12 {
        return [0.0, half, 0.0];
    }
    let ux = dx / len;
    let uy = dy / len;
    [-uy * half, ux * half, 0.0]
}

fn try_intersect2d(a1: [f64; 3], a2: [f64; 3], b1: [f64; 3], b2: [f64; 3]) -> Option<[f64; 3]> {
    let x1 = a1[0];
    let y1 = a1[1];
    let x2 = a2[0];
    let y2 = a2[1];
    let x3 = b1[0];
    let y3 = b1[1];
    let x4 = b2[0];
    let y4 = b2[1];
    let den = (x1 - x2) * (y3 - y4) - (y1 - y2) * (x3 - x4);
    if den.abs() < 1e-12 {
        return None;
    }
    let t = ((x1 - x3) * (y3 - y4) - (y1 - y3) * (x3 - x4)) / den;
    Some([x1 + t * (x2 - x1), y1 + t * (y2 - y1), a1[2]])
}

#[cfg(test)]
fn try_intersect(a1: [f64; 3], a2: [f64; 3], b1: [f64; 3], b2: [f64; 3]) -> Option<[f64; 3]> {
    try_intersect2d(a1, a2, b1, b2)
}

fn add(a: [f64; 3], b: [f64; 3]) -> [f64; 3] {
    [a[0] + b[0], a[1] + b[1], a[2] + b[2]]
}

fn sub(a: [f64; 3], b: [f64; 3]) -> [f64; 3] {
    [a[0] - b[0], a[1] - b[1], a[2] - b[2]]
}

fn distance2d(a: [f64; 3], b: [f64; 3]) -> f64 {
    let dx = a[0] - b[0];
    let dy = a[1] - b[1];
    (dx * dx + dy * dy).sqrt()
}

ocs_plugin_api::export_plugin!(TandemPlugin);

#[cfg(test)]
mod tests {
    use super::*;

    #[test]
    fn connect_command_maps_only_expected_variants() {
        assert!(is_connect_command(&normalize_command(CMD_TANDEM)));
        assert!(is_connect_command(&normalize_command(CMD_TANDEM_2026)));
        assert!(!is_connect_command(&normalize_command(CMD_TANDEM_MURO2D)));
    }

    #[test]
    fn wall2d_command_is_explicit() {
        assert!(is_wall2d_command(&normalize_command(CMD_TANDEM_MURO2D)));
        assert!(!is_wall2d_command(&normalize_command("TANDEM_MURO3D")));
    }

    #[test]
    fn wall_segments_apply_miter_on_corner() {
        let points = vec![[0.0, 0.0, 0.0], [10.0, 0.0, 0.0], [10.0, 10.0, 0.0]];
        let segments = build_wall_segments(&points, 1.0);
        assert_eq!(segments.len(), 2);

        let first = &segments[0];
        let second = &segments[1];

        assert!((first.plus_b[0] - 9.0).abs() < 1e-9);
        assert!((first.plus_b[1] - 1.0).abs() < 1e-9);
        assert!((second.plus_a[0] - 9.0).abs() < 1e-9);
        assert!((second.plus_a[1] - 1.0).abs() < 1e-9);

        assert!((first.minus_b[0] - 11.0).abs() < 1e-9);
        assert!((first.minus_b[1] + 1.0).abs() < 1e-9);
        assert!((second.minus_a[0] - 11.0).abs() < 1e-9);
        assert!((second.minus_a[1] + 1.0).abs() < 1e-9);
    }

    #[test]
    fn wall_entities_skip_zero_length_segments() {
        let points = vec![[0.0, 0.0, 0.0], [0.0, 0.0, 0.0], [5.0, 0.0, 0.0]];
        let entities = build_wall_entities(&points, 0.15, 0.30, 123);
        assert_eq!(entities.len(), 3);
    }

    #[test]
    fn wall_preview_keeps_previous_segments_when_adding_next_point() {
        let mut cmd = Wall2dInteractive::new(0.30, 123, Vec::new());
        let _ = cmd.on_point([0.0, 0.0, 0.0]);
        let step = cmd.on_point([10.0, 0.0, 0.0]);
        match step {
            CommandStep::CommitMany(ref entities) => assert_eq!(entities.len(), 3),
            _ => panic!("expected CommitMany after second point"),
        }

        let wires = cmd.on_cursor_move([10.0, 10.0, 0.0]);
        assert_eq!(wires.len(), 3);

        let paused_wires = cmd.on_cursor_move([10.0, 0.0, 0.0]);
        assert!(paused_wires.is_empty());
    }

    #[test]
    fn wall_enter_finishes_without_losing_committed_segments() {
        let mut cmd = Wall2dInteractive::new(0.30, 123, Vec::new());
        let _ = cmd.on_point([0.0, 0.0, 0.0]);
        let step = cmd.on_point([5.0, 0.0, 0.0]);
        match step {
            CommandStep::CommitMany(ref entities) => assert_eq!(entities.len(), 3),
            _ => panic!("expected CommitMany for first segment"),
        }

        match cmd.on_enter() {
            CommandStep::CommitManyAndEnd(ref entities) => assert_eq!(entities.len(), 3),
            CommandStep::Done => {}
            _ => panic!("expected CommitManyAndEnd or Done"),
        }
    }

    #[test]
    fn wall_corner_adds_face_joiners_like_autocad() {
        let mut cmd = Wall2dInteractive::new(0.30, 7, Vec::new());
        let _ = cmd.on_point([0.0, 0.0, 0.0]);
        let _ = cmd.on_point([10.0, 0.0, 0.0]);
        let step = cmd.on_point([10.0, 10.0, 0.0]);

        let CommandStep::CommitMany(entities) = step else {
            panic!("expected CommitMany at corner point");
        };
        assert_eq!(entities.len(), 7);
    }

    #[test]
    fn prefer_message_uses_fallback_when_primary_is_empty() {
        let message = prefer_message(" \n  ", "linea1\n\nlinea2").expect("fallback should be used");
        assert_eq!(message, "linea1 | linea2");
    }

    #[test]
    fn wall_step_parser_maps_axis_to_axis_layer() {
        let entity = parse_wall_step_line("WALL2D|axis|0|0|0|10|0|0", 0.30, 42)
            .expect("valid wall-step line")
            .expect("non-zero segment");

        match entity {
            EntityType::Line(line) => assert_eq!(line.common.layer, WALL_LAYER_AXIS),
            _ => panic!("expected line entity"),
        }
    }

    #[test]
    fn wall_step_parser_rejects_invalid_format() {
        let err = parse_wall_step_line("WALL2D|axis|0|0|0", 0.30, 42);
        assert!(err.is_err());
    }

    #[test]
    fn wall_step_parser_skips_zero_length_segments() {
        let entity = parse_wall_step_line("WALL2D|face|1|1|0|1|1|0", 0.30, 42)
            .expect("line parsed");
        assert!(entity.is_none());
    }

    #[test]
    fn find_axis_touch_candidate_projects_near_axis_only() {
        let segments = vec![([0.0, 0.0, 0.0], [10.0, 0.0, 0.0])];
        let near = find_axis_touch_candidate([5.0, 0.02, 0.0], &segments, 0.15)
            .expect("near interior touch expected");
        assert!((near.0[0] - 5.0).abs() < 1e-9);
        assert!(near.0[1].abs() < 1e-9);

        let endpoint = find_axis_touch_candidate([0.01, 0.0, 0.0], &segments, 0.15);
        assert!(endpoint.is_none());

        let far = find_axis_touch_candidate([5.0, 0.30, 0.0], &segments, 0.15);
        assert!(far.is_none());
    }

    #[test]
    fn point_on_segment_interior_rejects_endpoints() {
        let a = [0.0, 0.0, 0.0];
        let b = [10.0, 0.0, 0.0];
        assert!(point_on_segment_interior_2d([5.0, 0.0, 0.0], a, b, 0.03));
        assert!(!point_on_segment_interior_2d([0.01, 0.0, 0.0], a, b, 0.03));
        assert!(!point_on_segment_interior_2d([9.99, 0.0, 0.0], a, b, 0.03));
    }

    #[test]
    fn clone_line_with_points_resets_handle() {
        let mut source = codec::Line::from_coords(0.0, 0.0, 0.0, 10.0, 0.0, 0.0);
        source.common.handle = codec::Handle::new(1234);
        let cloned = clone_line_with_points(&source, [1.0, 0.0, 0.0], [2.0, 0.0, 0.0]);
        assert!(cloned.common.handle.is_null());
        assert!((cloned.start.x - 1.0).abs() < 1e-9);
        assert!((cloned.end.x - 2.0).abs() < 1e-9);
    }

    #[test]
    fn choose_touch_face_line_picks_near_face_by_probe_side() {
        let axis_a = [0.0, 0.0, 0.0];
        let axis_b = [10.0, 0.0, 0.0];
        let touch = [5.0, 0.0, 0.0];
        let probe = [5.0, -5.0, 0.0];
        let (face_a, face_b) = choose_touch_face_line(axis_a, axis_b, 0.15, touch, probe)
            .expect("expected face line");
        assert!((face_a[1] + 0.15).abs() < 1e-9);
        assert!((face_b[1] + 0.15).abs() < 1e-9);
    }

    #[test]
    fn compute_cut_interval_returns_ordered_face_hits() {
        let touch = [5.0, 0.0, 0.0];
        let probe = [5.0, -5.0, 0.0];
        let face_a = [0.0, -0.15, 0.0];
        let face_b = [10.0, -0.15, 0.0];
        let (cut_a, cut_b) =
            compute_cut_interval(touch, probe, 0.15, face_a, face_b).expect("expected cuts");
        assert!((cut_a[1] + 0.15).abs() < 1e-9);
        assert!((cut_b[1] + 0.15).abs() < 1e-9);
        assert!(cut_a[0] < cut_b[0]);
    }

    #[test]
    fn split_finalize_chain_rejects_short_sequences() {
        assert!(split_finalize_chain(&[]).is_none());
        assert!(split_finalize_chain(&[[0.0, 0.0, 0.0]]).is_none());
    }

    #[test]
    fn split_finalize_chain_returns_base_and_last_point() {
        let points = vec![[0.0, 0.0, 0.0], [10.0, 0.0, 0.0], [10.0, 5.0, 0.0]];
        let (base, next) = split_finalize_chain(&points).expect("expected split");
        assert_eq!(base, vec![[0.0, 0.0, 0.0], [10.0, 0.0, 0.0]]);
        assert_eq!(next, [10.0, 5.0, 0.0]);
    }
}
