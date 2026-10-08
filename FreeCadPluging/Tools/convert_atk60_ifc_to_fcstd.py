"""Convert ATK60 IFC assets to FCStd documents.

Run with FreeCADCmd:

    FreeCADCmd.exe FreeCadPluging\Tools\convert_atk60_ifc_to_fcstd.py
"""

from __future__ import annotations

import json
from pathlib import Path

import FreeCAD
import Mesh
import ifcopenshell
import ifcopenshell.geom


REPO_ROOT = Path(__file__).resolve().parents[2]
FREECAD_ROOT = REPO_ROOT / "Desing" / "Content" / "DesignTools" / "FreeCAD" / "AtkSystem60"
MAP_PATH = FREECAD_ROOT / "SourceMap" / "atk-system60-ifc-fcstd-map.json"


def _relative(path: Path) -> str:
    return path.relative_to(REPO_ROOT).as_posix()


def _article_code(path: Path) -> str:
    return path.stem[:-1] if path.stem.endswith("R") else path.stem


def _make_mesh(geometry) -> Mesh.Mesh:
    vertices = list(geometry.verts)
    faces = list(geometry.faces)
    mesh = Mesh.Mesh()
    for index in range(0, len(faces), 3):
        a = faces[index] * 3
        b = faces[index + 1] * 3
        c = faces[index + 2] * 3
        mesh.addFacet(
            vertices[a], vertices[a + 1], vertices[a + 2],
            vertices[b], vertices[b + 1], vertices[b + 2],
            vertices[c], vertices[c + 1], vertices[c + 2],
        )
    return mesh


def _convert_ifc(ifc_path: Path) -> dict:
    model = ifcopenshell.open(str(ifc_path))
    settings = ifcopenshell.geom.settings()
    try:
        settings.set("use-world-coords", True)
    except Exception:
        pass

    target_path = ifc_path.with_suffix(".FCStd")
    doc = FreeCAD.newDocument(ifc_path.stem.replace("-", "_"))
    product_count = 0
    object_count = 0
    facet_count = 0
    vertex_count = 0
    try:
        for product in model.by_type("IfcProduct"):
            if not getattr(product, "Representation", None):
                continue
            product_count += 1
            shape = ifcopenshell.geom.create_shape(settings, product)
            geometry = shape.geometry
            if not getattr(geometry, "faces", None):
                continue
            mesh = _make_mesh(geometry)
            if mesh.CountFacets == 0:
                continue
            obj = doc.addObject("Mesh::Feature", f"{ifc_path.stem}_{object_count + 1}")
            obj.Mesh = mesh
            obj.Label = ifc_path.stem if object_count == 0 else f"{ifc_path.stem}_{object_count + 1}"
            obj.addProperty("App::PropertyString", "TandemArticleCode", "Tandem", "Tandem article code")
            obj.addProperty("App::PropertyString", "TandemSourceIfc", "Tandem", "Source IFC")
            obj.addProperty("App::PropertyString", "TandemIfcType", "Tandem", "IFC product type")
            obj.TandemArticleCode = _article_code(ifc_path)
            obj.TandemSourceIfc = _relative(ifc_path)
            obj.TandemIfcType = product.is_a()
            object_count += 1
            facet_count += mesh.CountFacets
            vertex_count += len(geometry.verts) // 3

        doc.recompute()
        target_path.parent.mkdir(parents=True, exist_ok=True)
        doc.saveAs(str(target_path))
    finally:
        FreeCAD.closeDocument(doc.Name)

    return {
        "articleCode": _article_code(ifc_path),
        "sourceGroup": ifc_path.parent.name,
        "sourceIfc": _relative(ifc_path),
        "freeCadObject": _relative(target_path),
        "freeCadObjectType": "Mesh::Feature",
        "geometryKind": "ifc-triangulated-mesh",
        "ifcProductsWithRepresentation": product_count,
        "freeCadObjects": object_count,
        "vertices": vertex_count,
        "facets": facet_count,
    }


def main() -> int:
    ifc_files = sorted({path.resolve() for path in FREECAD_ROOT.rglob("*.ifc")})
    MAP_PATH.parent.mkdir(parents=True, exist_ok=True)

    entries = []
    failures = []
    for ifc_path in ifc_files:
        try:
            entry = _convert_ifc(ifc_path)
            entries.append(entry)
            print(
                f"Converted {entry['sourceIfc']} -> {entry['freeCadObject']} "
                f"objects={entry['freeCadObjects']} facets={entry['facets']}")
        except Exception as exc:
            failures.append({"sourceIfc": _relative(ifc_path), "error": str(exc)})
            print(f"FAILED {_relative(ifc_path)}: {exc}")

    payload = {
        "system": "AtkSystem60",
        "sourceFormat": "IFC",
        "targetFormat": "FCStd",
        "entries": entries,
        "failures": failures,
    }
    MAP_PATH.write_text(json.dumps(payload, indent=2, ensure_ascii=False) + "\n", encoding="utf-8")

    print(f"TOTAL_IFC={len(ifc_files)}")
    print(f"CREATED={len(entries)}")
    print(f"FAILURES={len(failures)}")
    print(f"MAP={_relative(MAP_PATH)}")
    return 1 if failures else 0


raise SystemExit(main())