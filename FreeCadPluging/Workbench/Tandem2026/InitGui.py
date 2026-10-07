"""FreeCAD GUI entry point for the Tandem 2026 workbench."""

import FreeCADGui as Gui


class TandemWorkbench(Workbench):
    MenuText = "Tandem 2026"
    ToolTip = "Tandem 2026 para FreeCAD"

    def Initialize(self):
        import tandem_commands

        commands = tandem_commands.register_commands()
        server_commands = [
            "Tandem_ServerLocal",
            "Tandem_ServerProduction",
            "Tandem_ShowServer",
        ]
        geometry_commands = [
            "Tandem_Wall2D",
            "Tandem_Wall3D",
        ]

        self.appendToolbar("Tandem 2026", commands)
        self.appendMenu("Tandem 2026", commands[:1])
        self.appendMenu("Tandem 2026", server_commands)
        self.appendMenu("Tandem 2026", geometry_commands)

    def Activated(self):
        import FreeCAD

        FreeCAD.Console.PrintMessage("Tandem 2026 cargado desde FreeCadPluging.\n")

    def Deactivated(self):
        pass

    def GetClassName(self):
        return "Gui::PythonWorkbench"


Gui.addWorkbench(TandemWorkbench())