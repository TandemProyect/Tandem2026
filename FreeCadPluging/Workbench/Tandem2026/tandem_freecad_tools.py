"""Minimal FreeCAD document tools for the Tandem 2026 workbench."""

import json
import math
import os
import tempfile

import FreeCAD
import Part
from pivy import coin

import tandem_host


DEFAULT_THICKNESS = 300.0
DEFAULT_HEIGHT = 2700.0
DEFAULT_LENGTH = 3000.0
DEFAULT_ARTICLE = "27904209"
MIN_WALL_SEGMENT = 1.0
_wall_drawing = None


def _message(text):
    FreeCAD.Console.PrintMessage("[Tandem] {0}\n".format(text))


def _warning(text):
    FreeCAD.Console.PrintWarning("[Tandem] {0}\n".format(text))


def _document():
    doc = FreeCAD.ActiveDocument
    if doc is None:
        doc = FreeCAD.newDocument("Tandem2026")
    return doc


def _set_string(obj, name, value):
    if not hasattr(obj, name):
        obj.addProperty("App::PropertyString", name, "Tandem", name)
    setattr(obj, name, str(value))


def _parse_vector(value):
    parts = [float(part) for part in str(value).split(",")]
    return FreeCAD.Vector(parts[0], parts[1], parts[2])


def _format_vector(vector):
    return "{0},{1},{2}".format(vector.x, vector.y, vector.z)


def _wall_objects(doc):
    return [obj for obj in doc.Objects if getattr(obj, "TandemKind", "") == "Wall2D"]


def _wall_solid_objects(doc):
    return [obj for obj in doc.Objects if getattr(obj, "TandemKind", "") == "Wall3D"]


def _formwork_objects(doc):
    return [obj for obj in doc.Objects if getattr(obj, "TandemKind", "") == "FormworkPiece"]


def create_wall_2d():
    """Start interactive wall-axis capture in the XY plan view."""
    global _wall_drawing
    import FreeCADGui

    if _wall_drawing is not None:
        _warning("Ya hay un dibujo de muro activo. Termina con clic derecho/Enter o cancela con Escape.")
        return _wall_drawing

    gui_doc = FreeCADGui.activeDocument()
    if gui_doc is None:
        _warning("Abre un documento FreeCAD antes de dibujar el muro.")
        return None

    view = gui_doc.activeView()
    view.viewTop()
    _wall_drawing = _WallDrawing(view)
    _wall_drawing.start()
    return _wall_drawing


def _create_wall_segments(points, thickness=DEFAULT_THICKNESS, height=DEFAULT_HEIGHT):
    doc = _document()
    valid_points = []
    for point in points:
        point = FreeCAD.Vector(point.x, point.y, 0.0)
        if not valid_points or point.sub(valid_points[-1]).Length >= MIN_WALL_SEGMENT:
            valid_points.append(point)
    if len(valid_points) < 2:
        raise ValueError("El muro necesita al menos dos puntos separados 1 mm o mas.")

    index = len(_wall_objects(doc)) + 1
    created = []
    for segment_index in range(len(valid_points) - 1):
        start = valid_points[segment_index]
        end = valid_points[segment_index + 1]
        if end.sub(start).Length < MIN_WALL_SEGMENT:
            continue
        edges = _wall_outline_edges(start, end, thickness)
        if len(edges) < 2:
            continue
        obj = doc.addObject("Part::Feature", "TandemWall2D_{0}".format(index + segment_index))
        obj.Shape = Part.makeCompound(edges)
        obj.Label = "Tandem Muro 2D {0}".format(index + segment_index)
        _set_string(obj, "TandemKind", "Wall2D")
        _set_string(obj, "TandemStart", _format_vector(start))
        _set_string(obj, "TandemEnd", _format_vector(end))
        _set_string(obj, "TandemThickness", thickness)
        _set_string(obj, "TandemHeight", height)
        _set_string(obj, "TandemSource", "FreeCAD")
        if getattr(obj, "ViewObject", None) is not None:
            obj.ViewObject.LineColor = (0.0, 0.25, 1.0)
            obj.ViewObject.LineWidth = 2.0
        created.append(obj)

    doc.recompute()
    _message("Muro 2D creado: {0} tramo(s), espesor {1} mm, altura {2} mm.".format(
        len(created), int(thickness), int(height)))
    return created


class _WallDrawing:
    def __init__(self, view):
        self.view = view
        self.points = []
        self.preview = None
        self.mouse_callback = self._on_mouse
        self.move_callback = self._on_move
        self.keyboard_callback = self._on_keyboard
        self._callback_handles = []
        self._preview_error_reported = False

    def start(self):
        self._add_callback(coin.SoMouseButtonEvent.getClassTypeId(), self.mouse_callback, "SoMouseButtonEvent")
        self._add_callback(coin.SoLocation2Event.getClassTypeId(), self.move_callback, "SoLocation2Event")
        self._add_callback(coin.SoKeyboardEvent.getClassTypeId(), self.keyboard_callback, "SoKeyboardEvent")
        _message("Dibuja el eje del muro: primer clic inicio, segundo clic fin. Escape cancela.")

    def _add_callback(self, pivy_type, callback, legacy_name):
        try:
            handle = self.view.addEventCallbackPivy(pivy_type, callback)
            self._callback_handles.append(("pivy", pivy_type, handle if handle is not None else callback, callback))
            return
        except Exception:
            pass
        try:
            handle = self.view.addEventCallback(legacy_name, callback)
            self._callback_handles.append(("legacy", legacy_name, handle, callback))
            return
        except Exception as ex:
            _warning("No se pudo registrar callback {0}: {1}".format(legacy_name, ex))

    def _point_from_event(self, event):
        position = event.getPosition()
        x, y = self._xy_from_position(position)
        if x is None or y is None:
            return None
        point = self.view.getPoint(x, y)
        return FreeCAD.Vector(point.x, point.y, 0.0)

    def _xy_from_position(self, position):
        try:
            return int(position[0]), int(position[1])
        except Exception:
            pass
        try:
            values = position.getValue()
            return int(values[0]), int(values[1])
        except Exception:
            pass
        return None, None

    def _on_mouse(self, event_callback):
        event = event_callback.getEvent()
        if event.getState() != coin.SoMouseButtonEvent.DOWN:
            return
        if event.getButton() == coin.SoMouseButtonEvent.BUTTON3:
            self.finish()
            return
        if event.getButton() != coin.SoMouseButtonEvent.BUTTON1:
            return

        point = self._point_from_event(event)
        if point is None:
            return
        if self.points and point.sub(self.points[-1]).Length < MIN_WALL_SEGMENT:
            _warning("Tramo ignorado: debe medir al menos 1 mm.")
            return
        self.points.append(point)
        self._update_preview(point)
        _message("Punto {0}: ({1:.1f}, {2:.1f}) mm.".format(len(self.points), point.x, point.y))

        # Flujo robusto en FreeCAD 1.1.4: con dos clics cerramos el tramo.
        if len(self.points) >= 2:
            self.finish()

    def _on_move(self, event_callback):
        if not self.points:
            return
        event = event_callback.getEvent()
        point = self._point_from_event(event)
        if point is not None:
            self._update_preview(point)

    def _on_keyboard(self, event_callback):
        event = event_callback.getEvent()
        if event.getState() != coin.SoKeyboardEvent.DOWN:
            return
        if event.getKey() in (coin.SoKeyboardEvent.RETURN, coin.SoKeyboardEvent.ENTER):
            self.finish()
        elif event.getKey() == coin.SoKeyboardEvent.ESCAPE:
            self.cancel()

    def _update_preview(self, cursor):
        doc = _document()
        if self.preview is None:
            self.preview = doc.addObject("Part::Feature", "TandemWallPreview")
            self.preview.Label = "Vista previa muro"
            if getattr(self.preview, "ViewObject", None) is not None:
                self.preview.ViewObject.LineColor = (1.0, 0.45, 0.0)
                self.preview.ViewObject.LineWidth = 2.0
        edges = []
        for index in range(len(self.points) - 1):
            edges.extend(_wall_outline_edges(self.points[index], self.points[index + 1], DEFAULT_THICKNESS))
        if self.points and cursor.sub(self.points[-1]).Length >= MIN_WALL_SEGMENT:
            edges.extend(_wall_outline_edges(self.points[-1], cursor, DEFAULT_THICKNESS))
        if edges:
            try:
                self.preview.Shape = Part.makeCompound(edges)
                doc.recompute()
                self._preview_error_reported = False
            except Exception as ex:
                if not self._preview_error_reported:
                    _warning("Vista previa de muro omitida por geometria invalida: {0}".format(ex))
                    self._preview_error_reported = True

    def finish(self):
        if len(self.points) < 2:
            _warning("Dibuja al menos dos puntos antes de terminar.")
            return
        points = list(self.points)
        self._detach()
        _create_wall_segments(points)

    def cancel(self):
        self._detach()
        _message("Dibujo de muro cancelado.")

    def _detach(self):
        global _wall_drawing
        for kind, event_key, handle, callback in self._callback_handles:
            try:
                if kind == "pivy":
                    try:
                        self.view.removeEventCallbackPivy(event_key, handle)
                    except Exception:
                        self.view.removeEventCallbackPivy(event_key, callback)
                else:
                    self.view.removeEventCallback(event_key, handle)
            except Exception:
                pass
        self._callback_handles = []
        doc = FreeCAD.ActiveDocument
        if doc is not None and self.preview is not None:
            doc.removeObject(self.preview.Name)
            doc.recompute()
        _wall_drawing = None


def _wall_outline_edges(start, end, thickness):
    delta = end.sub(start)
    length = delta.Length
    if length < MIN_WALL_SEGMENT:
        return []
    normal = FreeCAD.Vector(-delta.y / length, delta.x / length, 0.0)
    half = float(thickness) / 2.0
    left_start = start.add(normal.multiply(half))
    left_end = end.add(normal.multiply(half))
    right_start = start.add(normal.multiply(-half))
    right_end = end.add(normal.multiply(-half))
    edges = []
    left = _safe_make_line(left_start, left_end)
    right = _safe_make_line(right_start, right_end)
    if left is not None:
        edges.append(left)
    if right is not None:
        edges.append(right)
    return edges


def _safe_make_line(start, end):
    if start is None or end is None:
        return None
    if end.sub(start).Length < MIN_WALL_SEGMENT:
        return None
    try:
        return Part.makeLine(start, end)
    except Exception:
        return None


def _make_wall_shape(start, end, thickness, height):
    direction = end.sub(start)
    length = direction.Length
    if length <= 0.001:
        raise ValueError("El muro no tiene longitud suficiente.")
    angle = math.degrees(math.atan2(direction.y, direction.x))
    normal = FreeCAD.Vector(-direction.y / length, direction.x / length, 0.0)
    base = start.add(normal.multiply(-thickness / 2.0))
    shape = Part.makeBox(length, thickness, height)
    shape.Placement = FreeCAD.Placement(base, FreeCAD.Rotation(FreeCAD.Vector(0, 0, 1), angle))
    return shape


def generate_wall_3d():
    doc = _document()
    walls = _wall_objects(doc)
    if not walls:
        _warning("No hay muros 2D Tandem. Ejecuta primero 'Muro 2D'.")
        return []

    created = []
    existing_sources = set(getattr(obj, "TandemWallSource", "") for obj in _wall_solid_objects(doc))
    for wall in walls:
        if wall.Name in existing_sources:
            continue
        start = _parse_vector(wall.TandemStart)
        end = _parse_vector(wall.TandemEnd)
        thickness = float(wall.TandemThickness)
        height = float(wall.TandemHeight)
        solid = doc.addObject("Part::Feature", "TandemWall3D_{0}".format(len(_wall_solid_objects(doc)) + 1))
        solid.Shape = _make_wall_shape(start, end, thickness, height)
        solid.Label = "Tandem Muro 3D"
        _set_string(solid, "TandemKind", "Wall3D")
        _set_string(solid, "TandemWallSource", wall.Name)
        _set_string(solid, "TandemStart", wall.TandemStart)
        _set_string(solid, "TandemEnd", wall.TandemEnd)
        _set_string(solid, "TandemThickness", thickness)
        _set_string(solid, "TandemHeight", height)
        if getattr(solid, "ViewObject", None) is not None:
            solid.ViewObject.ShapeColor = (0.75, 0.75, 0.75)
            solid.ViewObject.Transparency = 55
        created.append(solid)

    doc.recompute()
    _message("Muros 3D creados: {0}.".format(len(created)))
    return created


def _repo_root():
    configured = os.environ.get("TANDEM_REPO_ROOT")
    if configured and os.path.isdir(configured):
        return configured
    default_root = r"C:\00_Tandem2026"
    if os.path.isdir(default_root):
        return default_root
    return None


def _article_path(article_code):
    root = _repo_root()
    if root is None:
        return None
    return os.path.join(
        root,
        "Desing",
        "Content",
        "DesignTools",
        "FreeCAD",
        "AtkSystem60",
        "3D",
        article_code + ".FCStd")


def _code_name(element):
    code = str(element.get("ElementCode") or element.get("elementCode") or "").strip()
    if code.upper().startswith("PANEL_"):
        code = code[6:]
    if _looks_like_code(code):
        return code

    import_path = str(element.get("ImportPath") or element.get("importPath") or "").strip()
    if import_path:
        file_name = os.path.splitext(os.path.basename(import_path))[0]
        if file_name.upper().endswith("_F"):
            file_name = file_name[:-2]
        if _looks_like_code(file_name):
            return file_name
    return None


def _looks_like_code(code):
    if not code:
        return False
    return sum(1 for char in code if char.isalnum()) >= 4


def _num(element, name, default=0.0):
    value = element.get(name)
    if value is None:
        value = element.get(name[:1].lower() + name[1:])
    try:
        return float(value)
    except Exception:
        return default


def _build_ids_json(doc):
    walls = []
    for index, wall in enumerate(_wall_objects(doc), start=1):
        start = _parse_vector(wall.TandemStart)
        end = _parse_vector(wall.TandemEnd)
        length = end.sub(start).Length
        thickness = float(wall.TandemThickness)
        height = float(wall.TandemHeight)
        wall_id = getattr(wall, "TandemWallId", "axis-{0}".format(index))
        walls.append({
            "Id": wall_id,
            "LineId": wall_id,
            "WallId": wall_id,
            "Attributes": {
                "_Datalong": length / 1000.0,
                "_DataWith": thickness / 1000.0,
                "_DataHeight": height / 1000.0,
                "InicioX": start.x,
                "InicioY": start.y,
                "InicioZ": start.z,
                "FinX": end.x,
                "FinY": end.y,
                "FinZ": end.z,
                "Tipo": "Line",
            },
        })
    return json.dumps(walls)


def _solve_formwork(doc):
    ids_json = _build_ids_json(doc)
    if ids_json == "[]":
        return None
    fd, path = tempfile.mkstemp(prefix="tandem-freecad-ids-", suffix=".json")
    try:
        with os.fdopen(fd, "w", encoding="utf-8") as handle:
            handle.write(ids_json)
        code, output, error = tandem_host.run_host("formwork-solve", path)
        if error:
            _warning(error)
        if code != 0:
            return None
        return json.loads(output)
    finally:
        try:
            os.remove(path)
        except Exception:
            pass


def _response_ok(response):
    return bool(response and (response.get("Exito") is True or response.get("exito") is True))


def _response_elements(response):
    payload = response.get("ElementsForThreeJs") or response.get("elementsForThreeJs") or {}
    return payload.get("Elements") or payload.get("elements") or []


def _clear_formwork(doc):
    for obj in list(_formwork_objects(doc)):
        doc.removeObject(obj.Name)


def _insert_article_objects(doc, article_code, element):
    article = _article_path(article_code)
    if not article or not os.path.isfile(article):
        return 0

    source_doc = FreeCAD.openDocument(article, True)
    inserted = 0
    try:
        for source_obj in source_doc.Objects:
            copied = doc.copyObject(source_obj, True)
            copied.Label = "ATK60 {0}".format(article_code)
            copied.Placement.Base = FreeCAD.Vector(
                _num(element, "X"),
                _num(element, "Z"),
                _num(element, "Y"))
            copied.Placement.Rotation = FreeCAD.Rotation(FreeCAD.Vector(0, 0, 1), -math.degrees(_num(element, "RotY")))
            _set_string(copied, "TandemKind", "FormworkPiece")
            _set_string(copied, "TandemArticleCode", article_code)
            _set_string(copied, "TandemWallId", element.get("IdWall") or element.get("idWall") or "")
            _set_string(copied, "TandemSourceFcstd", article)
            if getattr(copied, "ViewObject", None) is not None and hasattr(copied.ViewObject, "ShapeColor"):
                copied.ViewObject.ShapeColor = (0.95, 0.55, 0.10)
            inserted += 1
    finally:
        FreeCAD.closeDocument(source_doc.Name)
    return inserted


def insert_formwork(article_code=None):
    doc = _document()
    if not _wall_objects(doc):
        create_wall_2d()
    if not _wall_solid_objects(doc):
        generate_wall_3d()

    response = _solve_formwork(doc)
    if not _response_ok(response):
        message = response.get("Mensaje") if response else "Sin respuesta del encofrado MVC."
        _warning(message)
        return []

    _clear_formwork(doc)
    inserted = 0
    skipped = 0
    elements = _response_elements(response)
    for element in elements:
        code = article_code or _code_name(element)
        if not code:
            skipped += 1
            continue
        count = _insert_article_objects(doc, code, element)
        if count == 0:
            skipped += 1
        inserted += count

    doc.recompute()
    _message("Encofrado ATK-60 insertado: objetos {0}, omitidos {1}.".format(inserted, skipped))
    return _formwork_objects(doc)