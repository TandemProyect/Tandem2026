"""Commands exposed by the Tandem 2026 FreeCAD workbench."""

import FreeCAD
import FreeCADGui

import tandem_host
import tandem_command_bridge

_environment_started = False


def _message(text):
    FreeCAD.Console.PrintMessage("[Tandem] {0}\n".format(text))


def _warning(text):
    FreeCAD.Console.PrintWarning("[Tandem] {0}\n".format(text))


def activate_environment(force=False):
    global _environment_started
    tandem_command_bridge.start()
    if _environment_started and not force:
        return
    try:
        owner_handle = str(int(FreeCADGui.getMainWindow().winId()))
    except Exception:
        owner_handle = "0"
    ok, error = tandem_host.run_host_detached("connect-ui", owner_handle)
    if not ok:
        _warning(error)
        return
    _environment_started = True
    _message("Entorno Tandem 2026 abierto. Las herramientas se cargan desde MVC.")


class _OpenMvcPalettesCommand:
    def GetResources(self):
        return {
            "MenuText": "Abrir barras MVC",
            "ToolTip": "Reabre la sesion y las barras MVC de Tandem 2026",
        }

    def Activated(self):
        activate_environment(force=True)

    def IsActive(self):
        return True


def register_gui_commands():
    try:
        if not getattr(register_gui_commands, "_registered", False):
            FreeCADGui.addCommand("Tandem_OpenMvcPalettes", _OpenMvcPalettesCommand())
            register_gui_commands._registered = True
    except Exception as ex:
        _warning("No se pudo registrar el comando de barras MVC: {0}".format(ex))