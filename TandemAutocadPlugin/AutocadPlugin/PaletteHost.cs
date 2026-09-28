using System;
using AutocadPlugin.Models;
using AutocadPlugin.UI.Views;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.EditorInput;
using AcadApp = Autodesk.AutoCAD.ApplicationServices.Application;

namespace AutocadPlugin
{
    /// <summary>
    /// Paletas MVC sobre AutoCAD: sesión (login / diseños) + modo + herramientas.
    /// El login es la página de Desing; la cookie Identity vive en WebView2 (una vez por sesión).
    /// </summary>
    public static class PaletteHost
    {
        private static PaletteWindow _session;
        private static PaletteWindow _mode;
        private static PaletteWindow _tools;
        private static bool _sessionAuthenticated;
        private static string _readyUrl;

        public static bool AreVisible =>
            (_session != null && _session.IsVisible)
            || (_mode != null && _mode.IsVisible)
            || (_tools != null && _tools.IsVisible);

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
            _readyUrl = baseUrl + "DesignToolsAutocad/PluginReady";
            EnsureSession();
        }

        public static void CloseAll()
        {
            Close(ref _session);
            Close(ref _mode);
            Close(ref _tools);
            _sessionAuthenticated = false;
        }

        private static void EnsureSession()
        {
            if (_session != null)
            {
                if (_session.IsVisible)
                {
                    _session.Activate();
                    if (_sessionAuthenticated)
                        ShowToolPalettes();
                    return;
                }
                try { _session.Close(); } catch { }
                _session = null;
            }

            var created = new PaletteWindow(_readyUrl, 460, 720, allowResize: true);
            created.MessageReceived += OnPaletteMessage;
            created.Navigated += OnSessionNavigated;
            created.Closed += (_, __) =>
            {
                if (ReferenceEquals(_session, created)) _session = null;
            };

            Attach(created, -1, 70);
            _session = created;
        }

        private static void OnSessionNavigated(Uri uri)
        {
            if (uri == null) return;
            var path = uri.AbsolutePath ?? "";

            if (path.IndexOf("/Account/Login", StringComparison.OrdinalIgnoreCase) >= 0)
            {
                _sessionAuthenticated = false;
                Close(ref _mode);
                Close(ref _tools);
                ApplySessionSize(460, 720);
                if (_session != null)
                    PositionOverAcad(_session, -1, 70);
                WriteMessage("[Tandem] Inicia sesión en Desing (una vez por sesión).");
                return;
            }

            if (path.IndexOf("/DesignToolsAutocad/PluginReady", StringComparison.OrdinalIgnoreCase) >= 0)
            {
                MarkAuthenticated();
                return;
            }

            // Si el login ignoró ReturnUrl y cayó en Home, volver a la paleta de diseños.
            if (!string.IsNullOrWhiteSpace(_readyUrl)
                && path.IndexOf("/Home", StringComparison.OrdinalIgnoreCase) >= 0)
            {
                _session?.Navigate(_readyUrl);
            }
        }

        private static void MarkAuthenticated()
        {
            if (_sessionAuthenticated)
            {
                ShowToolPalettes();
                return;
            }

            _sessionAuthenticated = true;
            ApplySessionSize(320, 400);
            if (_session != null)
                PositionOverAcad(_session, 80, 160);
            ShowToolPalettes();
            WriteMessage("[Tandem] Sesión Desing conectada. Elige un diseño para dibujar los muros.");
        }

        private static void ApplySessionSize(double width, double height)
        {
            if (_session == null) return;
            try { _session.SetSize(width, height); } catch { }
        }

        private static void ShowToolPalettes()
        {
            var baseUrl = PluginExceptionHelper.ResolveBaseUrlFromEnv();
            var modeUrl = baseUrl + "DesignToolsAutocad/PaletteMode";
            var toolsUrl = baseUrl + "DesignToolsAutocad/PaletteTools";
            EnsureWindow(ref _mode, modeUrl, 348, 62, 80, 90);
            EnsureWindow(ref _tools, toolsUrl, 508, 58, -1, 90);
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

            Attach(created, leftOffset, topOffset);
            window = created;
        }

        private static void Attach(PaletteWindow created, double leftOffset, double topOffset)
        {
            try
            {
                created.SetOwnerHandle(AcadApp.MainWindow.Handle);
            }
            catch
            {
            }

            PositionOverAcad(created, leftOffset, topOffset);

            try
            {
                AcadApp.ShowModelessWindow(created);
            }
            catch
            {
                created.Show();
            }
        }

        private static void PositionOverAcad(PaletteWindow window, double leftOffset, double topOffset)
        {
            try
            {
                var main = AcadApp.MainWindow;
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

            if (TryHandleSessionMessage(json))
                return;

            string command = MapToAcadCommand(json);
            if (string.IsNullOrWhiteSpace(command))
            {
                WriteMessage("[Tandem paleta] " + json);
                return;
            }

            RunAcadCommand(command);
        }

        /// <summary>
        /// Lanza un comando en AutoCAD desde la paleta (ventana modeless).
        /// No usar "\n": en la línea de comandos se escribe como letra n
        /// y acaba en NTANDEM_MURO2D. Espacio termina el nombre; ESC limpia input previo.
        /// </summary>
        private static void RunAcadCommand(string command)
        {
            Document doc = AcadApp.DocumentManager.MdiActiveDocument;
            if (doc == null || string.IsNullOrWhiteSpace(command)) return;
            doc.SendStringToExecute("\x03\x03" + command.Trim() + " ", true, false, false);
        }

        private static bool TryHandleSessionMessage(string json)
        {
            try
            {
                var msg = Newtonsoft.Json.JsonConvert.DeserializeAnonymousType(
                    json, new { action = "", designId = 0L, snapshot = (WallSnapshotDto)null });
                if (msg == null || string.IsNullOrWhiteSpace(msg.action))
                    return false;

                if (string.Equals(msg.action, "session-ready", StringComparison.OrdinalIgnoreCase))
                {
                    MarkAuthenticated();
                    return true;
                }

                if (string.Equals(msg.action, "open-design", StringComparison.OrdinalIgnoreCase))
                {
                    WallImportCommand.OpenFromPalette(msg.designId, msg.snapshot);
                    return true;
                }
            }
            catch
            {
            }

            return false;
        }

        private static string MapToAcadCommand(string json)
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
            Document doc = AcadApp.DocumentManager.MdiActiveDocument;
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
