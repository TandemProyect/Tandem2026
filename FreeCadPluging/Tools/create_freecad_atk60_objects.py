"""Create FreeCAD ATK60 object documents from Tandem source assets.

DWG files define the object catalog. On this machine FreeCAD has no external DWG
converter configured, so geometry is loaded from the matching STL files already
used by Desing.
"""

from __future__ import annotations

import json
from pathlib import Path

import FreeCAD
import Mesh


REPO_ROOT = Path(__file__).resolve().parents[2]
DESIGN_TOOLS = REPO_ROOT / "Desing" / "Content" / "DesignTools"
DWG_ROOT = DESIGN_TOOLS / "DWG" / "AtkSystem60"
STL_ROOT = DESIGN_TOOLS / "Stl" / "ATK60"
FREECAD_ROOT = DESIGN_TOOLS / "FreeCAD" / "AtkSystem60"
MAP_PATH = FREECAD_ROOT / "SourceMap" / "atk-system60-freecad-map.json"


def _relative(path: Path) -> str:
    return path.relative_to(REPO_ROOT).as_posix()


def _stl_for(source_group: str, dwg: Path) -> Path:
    stem = dwg.stem
    if source_group == "3DRef" and stem.endswith("R"):
        return STL_ROOT / f"{stem[:-1]}_F.stl"
    return STL_ROOT / f"{stem}.stl"


def _create_document(source_group: str, dwg: Path, stl: Path, output: Path) -> dict:
    doc = FreeCAD.newDocument(dwg.stem)
    try:
        mesh = Mesh.Mesh(str(stl))
        obj = doc.addObject("Mesh::Feature", dwg.stem)
        obj.Mesh = mesh
        obj.Label = dwg.stem
        obj.addProperty("App::PropertyString", "TandemArticleCode", "Tandem", "Tandem article code")
        obj.addProperty("App::PropertyString", "TandemSourceDwg", "Tandem", "Source DWG")
        obj.addProperty("App::PropertyString", "TandemSourceStl", "Tandem", "Geometry STL")
        obj.addProperty("App::PropertyString", "TandemSourceGroup", "Tandem", "Source group")
        obj.TandemArticleCode = dwg.stem[:-1] if source_group == "3DRef" and dwg.stem.endswith("R") else dwg.stem
        obj.TandemSourceDwg = _relative(dwg)
        obj.TandemSourceStl = _relative(stl)
        obj.TandemSourceGroup = source_group
        doc.recompute()
        output.parent.mkdir(parents=True, exist_ok=True)
        doc.saveAs(str(output))
    finally:
        FreeCAD.closeDocument(doc.Name)

    return {
        "articleCode": dwg.stem[:-1] if source_group == "3DRef" and dwg.stem.endswith("R") else dwg.stem,
        "sourceGroup": source_group,
        "sourceDwg": _relative(dwg),
        "geometrySource": "stl-existing-asset",
        "sourceStl": _relative(stl),
        "freeCadObject": _relative(output),
        "freeCadObjectType": "Mesh::Feature",
    }


def _convert_group(source_group: str) -> list[dict]:
    source_dir = DWG_ROOT / source_group
    target_dir = FREECAD_ROOT / source_group
    entries: list[dict] = []

    for dwg in sorted(source_dir.glob("*.dwg")):
        stl = _stl_for(source_group, dwg)
        output = target_dir / f"{dwg.stem}.FCStd"
        if not stl.exists():
            entries.append({
                "articleCode": dwg.stem,
                "sourceGroup": source_group,
                "sourceDwg": _relative(dwg),
                "status": "missing-stl",
                "expectedStl": _relative(stl),
            })
            continue
        entries.append(_create_document(source_group, dwg, stl, output))

    return entries


def main() -> int:
    (FREECAD_ROOT / "3D").mkdir(parents=True, exist_ok=True)
    (FREECAD_ROOT / "3DRef").mkdir(parents=True, exist_ok=True)
    (FREECAD_ROOT / "Step").mkdir(parents=True, exist_ok=True)
    MAP_PATH.parent.mkdir(parents=True, exist_ok=True)

    entries = _convert_group("3D") + _convert_group("3DRef")
    payload = {
        "system": "AtkSystem60",
        "targetFormat": "FCStd",
        "dwgConverter": "not-configured",
        "geometrySource": "Matching STL files from Desing/Content/DesignTools/Stl/ATK60",
        "entries": entries,
    }
    MAP_PATH.write_text(json.dumps(payload, indent=2, ensure_ascii=False) + "\n", encoding="utf-8")

    created = sum(1 for entry in entries if entry.get("freeCadObject"))
    missing = sum(1 for entry in entries if entry.get("status") == "missing-stl")
    print(f"Created {created} FCStd objects. Missing STL: {missing}. Map: {_relative(MAP_PATH)}")
    return 1 if missing else 0


if __name__ == "__main__":
    raise SystemExit(main())