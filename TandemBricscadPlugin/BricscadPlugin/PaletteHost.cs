using System;
using Bricscad.ApplicationServices;
using Bricscad.EditorInput;
using BricscadPlugin.UI.Views;
using CadApp = Bricscad.ApplicationServices.Application;

namespace BricscadPlugin
{
    /// <summary>
    /// Abre las dos paletas MVC (sistema + herramientas) sobre BricsCAD.
    /// </summary>
    public static class PaletteHost
    {
        private static PaletteWindow _mode;
        private static PaletteWindow _tools;

        public static bool AreVisible =>
            (_mode != null && _mode.IsVisible) || (_tools != null && _tools.IsVisible);

        public static void Toggle()
        {
            if (AreVisible)
                CloseAll();
            else
                Show();
        }

        public static void Show()
        {
            PaletteWindow.PrepareNativeLoader();
            var baseUrl = PluginExceptionHelper.ResolveBaseUrlFromEnv();
            var modeUrl = baseUrl + "DesignToolsAutocad/PaletteMode";
            var toolsUrl = baseUrl + "DesignToolsAutocad/PaletteTools";

            EnsureWindow(ref _mode, modeUrl, 348, 62, 80, 90);
            EnsureWindow(ref _tools, toolsUrl, 508, 58, -1, 90);
        }

        public static void CloseAll()
        {
            Close(ref _mode);
            Close(ref _tools);
        }

        private static void EnsureWindow(ref PaletteWindow window, string url, double width, double height, double leftOffset, double topOffset)
        {
            if (window != null)
            {
                if (window.IsVisible)
                {
                    window.Activate();
                    return;
                }
                try { window.Close(); } catch { }
                window = null;
            }

            var created = new PaletteWindow(url, width, height);
            created.MessageReceived += OnPaletteMessage;
            created.Closed += (_, __) =>
            {
                if (ReferenceEquals(_mode, created)) _mode = null;
                if (ReferenceEquals(_tools, created)) _tools = null;
            };

            try
            {
                created.SetOwnerHandle(CadApp.MainWindow.Handle);
            }
            catch
            {
            }

            PositionOverCad(created, leftOffset, topOffset);

            try
            {
                CadApp.ShowModelessWindow(created);
            }
            catch
            {
                created.Show();
            }

            window = created;
        }

        private static void PositionOverCad(PaletteWindow window, double leftOffset, double topOffset)
        {
            try
            {
                var main = CadApp.MainWindow;
                var origin = main.DeviceIndependentLocation;
                var size = main.DeviceIndependentSize;
                window.Top = origin.Y + topOffset;
                if (leftOffset < 0)
                    window.Left = origin.X + Math.Max(40, (size.Width - window.Width) / 2);
                else
                    window.Left = origin.X + leftOffset;
            }
            catch
            {
                window.WindowStartupLocation = System.Windows.WindowStartupLocation.CenterScreen;
            }
        }

        private static void OnPaletteMessage(string json)
        {
            if (string.IsNullOrWhiteSpace(json)) return;

            string command = MapToCadCommand(json);
            if (string.IsNullOrWhiteSpace(command))
            {
                WriteMessage("\n[Tandem paleta] " + json + "\n");
                return;
            }

            Document doc = CadApp.DocumentManager.MdiActiveDocument;
            if (doc == null) return;
            WriteMessage("\n[Tandem] " + command + "\n");
            doc.SendStringToExecute(command + "\n", true, false, false);
        }

        private static string MapToCadCommand(string json)
        {
            try
            {
                var click = Newtonsoft.Json.JsonConvert.DeserializeAnonymousType(
                    json, new { tool = "", mode = "", system = "" });
                if (click == null) return null;

                switch (click.tool)
                {
                    case "polyline": return "._PLINE";
                    case "wall-2d": return Wall2dCommand.CommandName;
                    case "wall-3d": return Wall3dCommand.CommandName;
                    case "insert-enclosure": return "._RECTANG";
                    case "copy": return "._COPY";
                    case "move": return "._MOVE";
                    case "trim": return "._TRIM";
                    case "extend": return "._EXTEND";
                    case "stretch": return "._STRETCH";
                    case "offset": return "._OFFSET";
                    case "delete": return "._ERASE";
                }

                switch (click.mode)
                {
                    case "lines": return "._PLINE";
                    case "wall-2d": return Wall2dCommand.CommandName;
                    case "wall-3d": return Wall3dCommand.CommandName;
                }
            }
            catch
            {
            }

            return null;
        }

        private static void WriteMessage(string text)
        {
            Document doc = CadApp.DocumentManager.MdiActiveDocument;
            Editor ed = doc?.Editor;
            ed?.WriteMessage(text);
        }

        private static void Close(ref PaletteWindow window)
        {
            if (window == null) return;
            try { window.Close(); } catch { }
            window = null;
        }
    }
}
