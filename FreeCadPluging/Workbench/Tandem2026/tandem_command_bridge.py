"""Consumes MVC palette requests written by the C# host."""

import json
import os

import FreeCAD

import tandem_freecad_tools

try:
    from PySide6 import QtCore
except Exception:
    from PySide2 import QtCore


_timer = None


def _request_dir():
    return os.path.join(
        os.environ.get("APPDATA", ""),
        "Tandem",
        "FreecadPlugin",
        "requests")


def _message(text):
    FreeCAD.Console.PrintMessage("[Tandem] {0}\n".format(text))


def _warning(text):
    FreeCAD.Console.PrintWarning("[Tandem] {0}\n".format(text))


def start():
    global _timer
    if _timer is not None:
        return
    os.makedirs(_request_dir(), exist_ok=True)
    _timer = QtCore.QTimer()
    _timer.setInterval(500)
    _timer.timeout.connect(process_pending)
    _timer.start()
    _message("Puente MVC -> FreeCAD activo.")


def process_pending():
    root = _request_dir()
    if not os.path.isdir(root):
        return
    for name in sorted(os.listdir(root))[:20]:
        if not name.lower().endswith(".json"):
            continue
        path = os.path.join(root, name)
        try:
            with open(path, "r", encoding="utf-8") as handle:
                payload = json.load(handle)
        except Exception:
            payload = {}
        try:
            action = _action_from(name, payload)
            _dispatch(action, payload)
        except Exception as ex:
            _warning("No se pudo ejecutar peticion MVC {0}: {1}".format(name, ex))
        finally:
            try:
                os.remove(path)
            except Exception:
                pass


def _action_from(name, payload):
    action = str(payload.get("action") or payload.get("command") or "").strip()
    if action:
        return _normalize(action)
    stem = os.path.splitext(name)[0]
    if "-" in stem:
        return _normalize(stem.split("-", 1)[1])
    return _normalize(stem)


def _normalize(action):
    token = action.strip().lower().replace("_", "-")
    if token in ("wall-2d", "muro-2d", "muro2d", "tandem-muro2d"):
        return "wall-2d"
    if token in ("wall-3d", "muro-3d", "muro3d", "generar3d", "regenerar3d", "tandem-muro3d"):
        return "wall-3d"
    if token in ("formwork", "encofrar", "tandem-encofrar"):
        return "formwork"
    return token


def _dispatch(action, payload):
    if action == "wall-2d":
        tandem_freecad_tools.create_wall_2d()
        return
    if action == "wall-3d":
        tandem_freecad_tools.generate_wall_3d()
        return
    if action == "formwork":
        tandem_freecad_tools.insert_formwork()
        return
    _message("Mensaje MVC recibido sin adaptador FreeCAD: {0}".format(action))