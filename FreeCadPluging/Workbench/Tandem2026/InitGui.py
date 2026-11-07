"""FreeCAD GUI entry point for the Tandem 2026 workbench."""

import FreeCADGui as Gui


class TandemWorkbench(Workbench):
    MenuText = "Tandem 2026"
    ToolTip = "Tandem 2026 para FreeCAD"

    def Initialize(self):
        import tandem_commands

        tandem_commands.register_gui_commands()
        self.appendMenu("Tandem 2026", ["Tandem_OpenMvcPalettes"])

    def Activated(self):
        import FreeCAD
        import tandem_commands

        FreeCAD.Console.PrintMessage("Tandem 2026 cargado. Abriendo entorno MVC...\n")
        tandem_commands.activate_environment()

    def Deactivated(self):
        pass

    def GetClassName(self):
        return "Gui::PythonWorkbench"


Gui.addWorkbench(TandemWorkbench())