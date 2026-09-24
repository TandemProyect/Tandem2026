using Autodesk.Revit.UI;

namespace TandemRevit
{
    public enum TandemRequest
    {
        None,
        TogglePalettes,
        Wall2d,
        Wall3d,
        PostCommand
    }

    public class RevitRequestHandler : IExternalEventHandler
    {
        public TandemRequest Request { get; set; }
        public PostableCommand? Post { get; set; }

        public void Execute(UIApplication app)
        {
            switch (Request)
            {
                case TandemRequest.TogglePalettes:
                    PaletteHost.Toggle();
                    break;
                case TandemRequest.Wall2d:
                    Wall2dCommand.Run(app.ActiveUIDocument);
                    break;
                case TandemRequest.Wall3d:
                    Wall3dCommand.Run(app.ActiveUIDocument);
                    break;
                case TandemRequest.PostCommand:
                    if (Post.HasValue)
                    {
                        RevitCommandId id = RevitCommandId.LookupPostableCommandId(Post.Value);
                        if (id != null)
                            app.PostCommand(id);
                    }
                    break;
            }
        }

        public string GetName()
        {
            return "Tandem 2026";
        }
    }
}
