using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;

namespace TandemRevit
{
    [Transaction(TransactionMode.Manual)]
    public class CmdTogglePalettes : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            PaletteHost.Toggle();
            return Result.Succeeded;
        }
    }

    [Transaction(TransactionMode.Manual)]
    public class CmdWall2d : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            return Wall2dCommand.Run(commandData.Application.ActiveUIDocument);
        }
    }

    [Transaction(TransactionMode.Manual)]
    public class CmdWall3d : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            return Wall3dCommand.Run(commandData.Application.ActiveUIDocument);
        }
    }
}
