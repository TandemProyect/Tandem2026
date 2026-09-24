using System;
using System.Windows;
using Autodesk.Revit.UI;
using TandemRevit.UI.Views;

namespace TandemRevit
{
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
            EnsureWindow(ref _mode, baseUrl + "DesignToolsAutocad/PaletteMode", 348, 62, 80, 90);
            EnsureWindow(ref _tools, baseUrl + "DesignToolsAutocad/PaletteTools", 508, 58, -1, 90);
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
                created.SetOwnerHandle(System.Diagnostics.Process.GetCurrentProcess().MainWindowHandle);
            }
            catch
            {
            }

            created.WindowStartupLocation = WindowStartupLocation.Manual;
            created.Left = Math.Max(40, leftOffset < 0 ? 360 : leftOffset);
            created.Top = topOffset;
            created.Show();
            window = created;
        }

        private static void OnPaletteMessage(string json)
        {
            if (string.IsNullOrWhiteSpace(json)) return;

            try
            {
                var click = Newtonsoft.Json.JsonConvert.DeserializeAnonymousType(
                    json, new { tool = "", mode = "", system = "" });
                if (click == null) return;

                if (click.tool == "wall-2d" || click.mode == "wall-2d")
                {
                    App.Raise(TandemRequest.Wall2d);
                    return;
                }
                if (click.tool == "wall-3d" || click.mode == "wall-3d")
                {
                    App.Raise(TandemRequest.Wall3d);
                    return;
                }

                PostableCommand? post = null;
                switch (click.tool)
                {
                    case "polyline": post = PostableCommand.ModelLine; break;
                    case "insert-enclosure": post = PostableCommand.ModelLine; break;
                    case "copy": post = PostableCommand.Copy; break;
                    case "move": post = PostableCommand.Move; break;
                    case "offset": post = PostableCommand.Offset; break;
                    case "delete": post = PostableCommand.Delete; break;
                }

                if (click.mode == "lines" && post == null)
                    post = PostableCommand.ModelLine;

                if (post.HasValue)
                    App.Raise(TandemRequest.PostCommand, post);
            }
            catch
            {
            }
        }

        private static void Close(ref PaletteWindow window)
        {
            if (window == null) return;
            try { window.Close(); } catch { }
            window = null;
        }
    }
}
