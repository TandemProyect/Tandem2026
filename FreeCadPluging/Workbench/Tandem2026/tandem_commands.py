"""Commands exposed by the Tandem 2026 FreeCAD workbench."""

import FreeCAD
import FreeCADGui

import tandem_host
import tandem_freecad_tools
import tandem_command_bridge

_environment_started = False


def _message(text):
    FreeCAD.Console.PrintMessage("[Tandem] {0}\n".format(text))


def _warning(text):
    FreeCAD.Console.PrintWarning("[Tandem] {0}\n".format(text))


def _run(command):
    code, output, error = tandem_host.run_host(command)
    if output:
        _message(output)
    if error:
        _warning(error)
    if code != 0 and not error:
        _warning("FreeCadPluging.exe devolvio codigo {0}.".format(code))


def activate_environment(force=False):
    global _environment_started
    tandem_command_bridge.start()
    if _environment_started and not force:
        return
    ok, error = tandem_host.run_host_detached("connect-ui")
    if not ok:
        _warning(error)
        return
    _environment_started = True
    _message("Entorno Tandem 2026 abierto. Las herramientas se cargan desde MVC.")


class ConnectCommand:
    def GetResources(self):
        return {
            "MenuText": "Conectar / Preparar entorno",
            "ToolTip": "Abrir sesion MVC y mostrar las herramientas Tandem",
        }

    def Activated(self):
        activate_environment(force=True)

    def IsActive(self):
        return True


class SetLocalServerCommand:
    def GetResources(self):
        return {
            "MenuText": "Servidor local",
            "ToolTip": "Usar https://localhost:44384/",
        }

    def Activated(self):
        _run("set-local")

    def IsActive(self):
        return True


class SetProductionServerCommand:
    def GetResources(self):
        return {
            "MenuText": "Servidor produccion",
            "ToolTip": "Usar https://tdesing.net/",
        }

    def Activated(self):
        _run("set-production")

    def IsActive(self):
        return True


class ShowServerCommand:
    def GetResources(self):
        return {
            "MenuText": "Mostrar servidor",
            "ToolTip": "Mostrar el servidor MVC activo",
        }

    def Activated(self):
        _run("show-server")

    def IsActive(self):
        return True


class Wall2DCommand:
    def GetResources(self):
        return {
            "MenuText": "Muro 2D",
            "ToolTip": "Entrada C# para dibujar muro 2D Tandem en FreeCAD",
        }

    def Activated(self):
        _run("wall-2d")
        tandem_freecad_tools.create_wall_2d()

    def IsActive(self):
        return True


class Wall3DCommand:
    def GetResources(self):
        return {
            "MenuText": "Generar 3D",
            "ToolTip": "Entrada C# para llamar a LCornerDetector y crear solidos FreeCAD",
        }

    def Activated(self):
        _run("wall-3d")
        tandem_freecad_tools.generate_wall_3d()

    def IsActive(self):
        return True


class FormworkCommand:
    def GetResources(self):
        return {
            "MenuText": "Encofrar",
            "ToolTip": "Enviar muros al encofrado MVC e insertar piezas ATK60",
        }

    def Activated(self):
        _run("formwork")
        tandem_freecad_tools.insert_formwork()

    def IsActive(self):
        return True


def register_commands():
    commands = {
        "Tandem_Connect": ConnectCommand(),
    }
    for name, command in commands.items():
        FreeCADGui.addCommand(name, command)
    return list(commands.keys())