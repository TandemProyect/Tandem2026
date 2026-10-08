"""Minimal FreeCAD document tools for the Tandem 2026 workbench."""

import json
import math
import os
import tempfile

import FreeCAD
import Part

import tandem_host


DEFAULT_THICKNESS = 120.0
DEFAULT_HEIGHT = 2700.0
DEFAULT_LENGTH = 3000.0
DEFAULT_ARTICLE = "27904209"


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
    doc = _document()
    index = len(_wall_objects(doc)) + 1
    offset = (index - 1) * (DEFAULT_THICKNESS * 3.0)
    start = FreeCAD.Vector(0.0, offset, 0.0)
    end = FreeCAD.Vector(DEFAULT_LENGTH, offset, 0.0)

    obj = doc.addObject("Part::Feature", "TandemWall2D_{0}".format(index))
    obj.Shape = Part.makeLine(start, end)
    obj.Label = "Tandem Muro 2D {0}".format(index)
    _set_string(obj, "TandemKind", "Wall2D")
    _set_string(obj, "TandemStart", _format_vector(start))
    _set_string(obj, "TandemEnd", _format_vector(end))
    _set_string(obj, "TandemThickness", DEFAULT_THICKNESS)
    _set_string(obj, "TandemHeight", DEFAULT_HEIGHT)
    _set_string(obj, "TandemSource", "FreeCAD")
    if getattr(obj, "ViewObject", None) is not None:
        obj.ViewObject.LineColor = (0.0, 0.25, 1.0)
        obj.ViewObject.LineWidth = 4.0
    doc.recompute()
    _message("Muro 2D creado: longitud {0} mm, espesor {1} mm, altura {2} mm.".format(
        int(DEFAULT_LENGTH), int(DEFAULT_THICKNESS), int(DEFAULT_HEIGHT)))
    return obj


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