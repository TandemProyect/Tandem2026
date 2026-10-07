"""Convert ATK60 STEP assets to FCStd documents.

Run with FreeCADCmd because it needs the Part workbench modules:

    FreeCADCmd.exe FreeCadPluging\Tools\convert_atk60_steps_to_fcstd.py
"""

from __future__ import annotations

import json
from pathlib import Path

import FreeCAD
import Part


REPO_ROOT = Path(__file__).resolve().parents[2]
FREECAD_ROOT = REPO_ROOT / "Desing" / "Content" / "DesignTools" / "FreeCAD" / "AtkSystem60"
MAP_PATH = FREECAD_ROOT / "SourceMap" / "atk-system60-step-fcstd-map.json"


def _relative(path: Path) -> str:
    return path.relative_to(REPO_ROOT).as_posix()


def _convert_step(step_path: Path) -> dict:
    target_path = step_path.with_suffix(".FCStd")
    article_code = step_path.stem[:-1] if step_path.stem.endswith("R") else step_path.stem
    source_group = step_path.parent.name

    shape = Part.read(str(step_path))
    bbox = shape.BoundBox
    doc = FreeCAD.newDocument(step_path.stem.replace("-", "_"))
    try:
        obj = doc.addObject("Part::Feature", step_path.stem)
        obj.Shape = shape
        obj.Label = step_path.stem
        obj.addProperty("App::PropertyString", "TandemArticleCode", "Tandem", "Tandem article code")
        obj.addProperty("App::PropertyString", "TandemSourceStep", "Tandem", "Source STEP")
        obj.addProperty("App::PropertyString", "TandemSourceGroup", "Tandem", "Source group")
        obj.TandemArticleCode = article_code
        obj.TandemSourceStep = _relative(step_path)
        obj.TandemSourceGroup = source_group
        doc.recompute()
        doc.saveAs(str(target_path))
    finally:
        FreeCAD.closeDocument(doc.Name)

    geometry_kind = "brep-faces" if len(shape.Faces) else "wire-edges-only"
    return {
        "articleCode": article_code,
        "sourceGroup": source_group,
        "sourceStep": _relative(step_path),
        "freeCadObject": _relative(target_path),
        "freeCadObjectType": "Part::Feature",
        "geometryKind": geometry_kind,
        "solids": len(shape.Solids),
        "faces": len(shape.Faces),
        "edges": len(shape.Edges),
        "bbox": {
            "x": bbox.XLength,
            "y": bbox.YLength,
            "z": bbox.ZLength,
        },
    }


def main() -> int:
    step_files = sorted(FREECAD_ROOT.rglob("*.stp")) + sorted(FREECAD_ROOT.rglob("*.step"))
    MAP_PATH.parent.mkdir(parents=True, exist_ok=True)

    entries = []
    failures = []
    for step_path in step_files:
        try:
            entry = _convert_step(step_path)
            entries.append(entry)
            print(
                f"Converted {entry['sourceStep']} -> {entry['freeCadObject']} "
                f"faces={entry['faces']} edges={entry['edges']} kind={entry['geometryKind']}")
        except Exception as exc:
            failures.append({"sourceStep": _relative(step_path), "error": str(exc)})
            print(f"FAILED {_relative(step_path)}: {exc}")

    payload = {
        "system": "AtkSystem60",
        "sourceFormat": "STEP",
        "targetFormat": "FCStd",
        "entries": entries,
        "failures": failures,
    }
    MAP_PATH.write_text(json.dumps(payload, indent=2, ensure_ascii=False) + "\n", encoding="utf-8")

    print(f"TOTAL_STEP={len(step_files)}")
    print(f"CREATED={len(entries)}")
    print(f"FAILURES={len(failures)}")
    print(f"MAP={_relative(MAP_PATH)}")
    return 1 if failures else 0


raise SystemExit(main())