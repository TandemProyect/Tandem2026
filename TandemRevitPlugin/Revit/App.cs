using Autodesk.Revit.UI;

namespace TandemRevit
{
    /// <summary>
    /// Entrada del add-in. Pestaña Tandem 2026 + paletas MVC (mismo flujo que AutoCAD/BricsCAD).
    /// </summary>
    public class App : IExternalApplication
    {
        internal static ExternalEvent Bus;
        internal static RevitRequestHandler Handler;

        public Result OnStartup(UIControlledApplication application)
        {
            Handler = new RevitRequestHandler();
            Bus = ExternalEvent.Create(Handler);
            CreateRibbon(application);
            return Result.Succeeded;
        }

        public Result OnShutdown(UIControlledApplication application)
        {
            try { PaletteHost.CloseAll(); } catch { }
            return Result.Succeeded;
        }

        internal static void Raise(TandemRequest request, PostableCommand? post = null)
        {
            if (Handler == null || Bus == null) return;
            Handler.Request = request;
            Handler.Post = post;
            Bus.Raise();
        }

        private static void CreateRibbon(UIControlledApplication application)
        {
            const string tab = "Tandem 2026";
            try { application.CreateRibbonTab(tab); } catch { }

            RibbonPanel panel = application.CreateRibbonPanel(tab, "Tandem");
            panel.AddItem(new PushButtonData(
                "TandemMenus",
                "Menús",
                typeof(App).Assembly.Location,
                "TandemRevit.CmdTogglePalettes"));
            panel.AddItem(new PushButtonData(
                "TandemWall2d",
                "Muro 2D",
                typeof(App).Assembly.Location,
                "TandemRevit.CmdWall2d"));
            panel.AddItem(new PushButtonData(
                "TandemWall3d",
                "Generar 3D",
                typeof(App).Assembly.Location,
                "TandemRevit.CmdWall3d"));
        }
    }
}
