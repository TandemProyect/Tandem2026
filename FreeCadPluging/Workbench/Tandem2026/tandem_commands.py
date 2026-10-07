"""Commands exposed by the Tandem 2026 FreeCAD workbench."""

import FreeCAD
import FreeCADGui

import tandem_host


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


class ConnectCommand:
    def GetResources(self):
        return {
            "MenuText": "Conectar",
            "ToolTip": "Probar conexion con Desing MVC desde el host C#",
        }

    def Activated(self):
        _run("ping")

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

    def IsActive(self):
        return True


def register_commands():
    commands = {
        "Tandem_Connect": ConnectCommand(),
        "Tandem_ServerLocal": SetLocalServerCommand(),
        "Tandem_ServerProduction": SetProductionServerCommand(),
        "Tandem_ShowServer": ShowServerCommand(),
        "Tandem_Wall2D": Wall2DCommand(),
        "Tandem_Wall3D": Wall3DCommand(),
    }
    for name, command in commands.items():
        FreeCADGui.addCommand(name, command)
    return list(commands.keys())