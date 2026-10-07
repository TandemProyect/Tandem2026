"""Convert STEP files to native FreeCAD FCStd documents.

Run with FreeCADCmd, not with system Python:

    FreeCADCmd.exe convert_step_to_fcstd.py input.stp output.FCStd
"""

from __future__ import annotations

import sys
import os
from pathlib import Path

import FreeCAD
import Import


def convert_step_to_fcstd(source: Path, target: Path) -> int:
    if not source.is_file():
        print(f"Missing STEP file: {source}", file=sys.stderr)
        return 2

    target.parent.mkdir(parents=True, exist_ok=True)
    document_name = source.stem.replace("-", "_")
    doc = FreeCAD.newDocument(document_name)
    try:
        Import.insert(str(source), doc.Name)
        doc.recompute()
        if not doc.Objects:
            print(f"No objects imported from: {source}", file=sys.stderr)
            return 3
        doc.saveAs(str(target))
        print(f"Converted {source} -> {target} ({len(doc.Objects)} objects)")
        return 0
    finally:
        FreeCAD.closeDocument(doc.Name)


def main(argv: list[str]) -> int:
    if len(argv) == 3:
        source = Path(argv[1]).resolve()
        target = Path(argv[2]).resolve()
        return convert_step_to_fcstd(source, target)

    source_env = os.environ.get("TANDEM_STEP_SOURCE")
    target_env = os.environ.get("TANDEM_FCSTD_TARGET")
    if not source_env or not target_env:
        print(
            "Usage: FreeCADCmd.exe convert_step_to_fcstd.py input.stp output.FCStd\n"
            "   or: set TANDEM_STEP_SOURCE and TANDEM_FCSTD_TARGET, then run the script.",
            file=sys.stderr)
        return 1

    source = Path(source_env).resolve()
    target = Path(target_env).resolve()
    return convert_step_to_fcstd(source, target)


if __name__ == "__main__":
    raise SystemExit(main(sys.argv))