using System;
using System.Threading.Tasks;
using AutocadPlugin.Models;
using AutocadPlugin.UI.Views;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.EditorInput;
using Newtonsoft.Json.Linq;
using AcadApp = Autodesk.AutoCAD.ApplicationServices.Application;

namespace AutocadPlugin
{
    /// <summary>
    /// Paletas MVC sobre AutoCAD: sesión (login / menú general) + modo + herramientas.
    /// El login es la página de Desing; la cookie Identity vive en WebView2 (una vez por sesión).
    /// Tras el login las paletas de herramientas quedan visibles; el menú general se abre
    /// con el primer botón de la barra de modo. Abrir un diseño dibuja los muros en CAD.
    /// </summary>
    public static class PaletteHost
    {
        private static PaletteWindow _session;
        private static PaletteWindow _mode;
        private static PaletteWindow _tools;
        private static PaletteWindow _blocks;
        private static bool _sessionAuthenticated;
        private static bool _pendingBlocks;
        private static string _readyUrl;
        private static string _sessionStartUrl;
        private static int _splashWaitGen;
        private static double _pendingSessionWidth = 460;
        private static double _pendingSessionHeight = 720;

        public static bool AreVisible =>
            (_session != null && _session.IsVisible)
            || (_mode != null && _mode.IsVisible)
            || (_tools != null && _tools.IsVisible)
            || (_blocks != null && _blocks.IsVisible);

        public static void Toggle()
        {
            if (AreVisible)
                HideUi();
            else
                Show();
        }

        public static void Show()
        {
            PaletteWindow.PrepareNativeLoader();
            var baseUrl = PluginExceptionHelper.ResolveBaseUrlFromEnv();
            _readyUrl = baseUrl + "DesignToolsAutocad/PluginReady";
            _sessionStartUrl = BuildSessionStartUrl(baseUrl);

            if (_sessionAuthenticated && _session != null)
            {
                ShowToolPalettes();
                return;
            }

            WriteMessage("[Tandem] Comprobando autorización en " + MvcServerSettings.CurrentLabel() + "…");
            EnsureSession();
        }

        public static void ShowBlocks()
        {
            if (!_sessionAuthenticated)
            {
                _pendingBlocks = true;
                Show();
                return;
            }

            ShowToolPalettes();
            if (_blocks != null && _blocks.IsVisible)
            {
                if (_blocks.Width + 1 < BlocksWidth)
                {
                    ApplyBlocksSize(expanded: true);
                    return;
                }
                HidePalette(_blocks);
                return;
            }

            EnsureBlocks();
        }

        public static void CloseAll()
        {
            Close(ref _session);
            Close(ref _mode);
            Close(ref _tools);
            Close(ref _blocks);
            _sessionAuthenticated = false;
            _pendingBlocks = false;
        }

        public static void ReconnectToCurrentServer()
        {
            CloseAll();
            Show();
        }

        private static void HideUi()
        {
            HideSessionWindow();
            HidePalette(_mode);
            HidePalette(_tools);
            HidePalette(_blocks);
        }

        private static void HidePalette(PaletteWindow window)
        {
            if (window == null) return;
            try
            {
                Action hide = () =>
                {
                    if (window.IsVisible)
                        window.Hide();
                };
                if (window.Dispatcher.CheckAccess())
                    hide();
                else
                    window.Dispatcher.Invoke(hide);
            }
            catch
            {
            }
        }

        private static void EnsureSession()
        {
            if (_session != null)
            {
                if (!_session.IsVisible)
                {
                    try { _session.Show(); } catch { }
                }
                _session.Activate();
                if (_sessionAuthenticated)
                    ShowToolPalettes();
                return;
            }

            var created = new PaletteWindow(_sessionStartUrl, 320, 292, allowResize: false, authSplash: true);
            created.MessageReceived += OnPaletteMessage;
            created.Navigated += OnSessionNavigated;
            created.Closed += (_, __) =>
            {
                if (ReferenceEquals(_session, created))
                {
                    _session = null;
                    _sessionAuthenticated = false;
                }
            };

            Attach(created, -1, 70);
            created.ShowStatus("Comprobando autorización en TDesing…");
            _session = created;
        }

        private static void OnSessionNavigated(Uri uri)
        {
            if (uri == null) return;
            var path = uri.AbsolutePath ?? "";

            if (path.IndexOf("/Account/Login", StringComparison.OrdinalIgnoreCase) >= 0
                || string.Equals(path, "/", StringComparison.Ordinal))
            {
                _sessionAuthenticated = false;
                Close(ref _mode);
                Close(ref _tools);
                Close(ref _blocks);
                _pendingBlocks = false;
                _pendingSessionWidth = 460;
                _pendingSessionHeight = 720;
                if (_session != null)
                {
                    ShowSessionWindow();
                    PositionOverAcad(_session, -1, 70);
                    try
                    {
                        var origin = uri.GetLeftPart(UriPartial.Authority);
                        if (!string.IsNullOrWhiteSpace(origin))
                            _session.ClearCookiesForSite(origin + "/");
                    }
                    catch
                    {
                    }
                }
                WriteMessage("[Tandem] Inicia sesion en Desing (una vez por sesion).");
                RevealSessionPage();
                return;
            }

            if (path.IndexOf("/DesignToolsAutocad/PluginSession", StringComparison.OrdinalIgnoreCase) >= 0)
            {
                _session?.ShowStatus("Comprobando autorización en TDesing…");
                if (_session != null)
                    PositionOverAcad(_session, -1, 70);
                return;
            }

            if (path.IndexOf("/DesignToolsAutocad/PluginCadAuth", StringComparison.OrdinalIgnoreCase) >= 0)
            {
                _session?.ShowStatus("Comprobando autorización en TDesing…");
                return;
            }

            if (path.IndexOf("/DesignToolsAutocad/PluginReady", StringComparison.OrdinalIgnoreCase) >= 0)
            {
                _pendingSessionWidth = 540;
                _pendingSessionHeight = 700;
                if (_sessionAuthenticated)
                    RevealSessionPage();
                else
                {
                    _session?.ShowStatus("Cargando TDesing…");
                    ArmSplashFallback();
                }
                return;
            }

            // Si el login ignoró ReturnUrl y cayó en Home, repetir el arranque de sesión.
            if (!string.IsNullOrWhiteSpace(_sessionStartUrl)
                && path.IndexOf("/Home", StringComparison.OrdinalIgnoreCase) >= 0)
            {
                if (_sessionAuthenticated && !string.IsNullOrWhiteSpace(_readyUrl))
                    _session?.Navigate(_readyUrl);
                else
                    _session?.Navigate(_sessionStartUrl);
            }
        }

        private static string BuildSessionStartUrl(string baseUrl)
        {
            var qs =
                "deviceId=" + Uri.EscapeDataString(PluginDeviceId.Current()) +
                "&machineName=" + Uri.EscapeDataString(Environment.MachineName ?? "") +
                "&usuarioWindows=" + Uri.EscapeDataString(PluginDeviceId.WindowsUser()) +
                "&pluginVersion=" + Uri.EscapeDataString(PluginDeviceId.PluginVersion());
            return baseUrl + "DesignToolsAutocad/PluginSession?" + qs;
        }

        private static void ArmSplashFallback()
        {
            var gen = ++_splashWaitGen;
            Task.Delay(ConnectEta.FallbackMs()).ContinueWith(_ =>
            {
                if (gen != _splashWaitGen) return;
                try
                {
                    var win = _session;
                    if (win == null) return;
                    win.Dispatcher.Invoke(RevealSessionPage);
                }
                catch
                {
                }
            });
        }

        private static void RevealSessionPage()
        {
            _splashWaitGen++;
            _session?.HideStatus();
            ApplySessionSize(_pendingSessionWidth, _pendingSessionHeight);
            PositionOverAcad(_session, -1, 70);
        }

        private static void MarkAuthenticated()
        {
            if (_sessionAuthenticated)
            {
                ShowToolPalettes();
                return;
            }

            _sessionAuthenticated = true;
            HideSessionWindow();
            ShowToolPalettes();
            WriteMessage("[Tandem] Sesion conectada a " + MvcServerSettings.CurrentLabel() + ". El primer boton abre el menu general.");
            if (_pendingBlocks)
            {
                _pendingBlocks = false;
                ShowBlocks();
            }
        }

        private static void ShowHome()
        {
            EnsureSession();
            if (_session == null) return;

            ApplySessionSize(540, 700);
            PositionOverAcad(_session, -1, 70);
            ShowSessionWindow();
            _session.Activate();
            RevealSessionPage();

            if (!_sessionAuthenticated && !string.IsNullOrWhiteSpace(_readyUrl))
            {
                _session.ShowStatus("Cargando TDesing…");
                ArmSplashFallback();
                _session.Navigate(_readyUrl);
            }
        }

        private static void HideHome()
        {
            HideSessionWindow();
        }

        private static void ShowSessionWindow()
        {
            if (_session == null) return;
            try
            {
                if (!_session.IsVisible)
                    _session.Show();
            }
            catch { }
        }

        private static void HideSessionWindow()
        {
            if (_session == null) return;
            try
            {
                if (_session.IsVisible)
                    _session.Hide();
            }
            catch { }
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
            EnsureWindow(ref _mode, modeUrl, 492, 62, 80, 90);
            EnsureWindow(ref _tools, toolsUrl, 508, 58, -1, 90);
        }

        private const double BlocksWidth = 320;
        private const double BlocksHeight = 680;
        private const double BlocksCollapsedWidth = 32;
        private const double BlocksLeft = 80;
        private const double BlocksTop = 158;

        private static void EnsureBlocks()
        {
            var baseUrl = PluginExceptionHelper.ResolveBaseUrlFromEnv();
            var url = baseUrl + "DesignToolsAutocad/PluginBlocks";

            if (_blocks != null)
            {
                try
                {
                    if (!_blocks.IsVisible)
                        _blocks.Show();
                    ApplyBlocksSize(expanded: true);
                    _blocks.Activate();
                    PositionOverAcad(_blocks, BlocksLeft, BlocksTop);
                }
                catch
                {
                }
                return;
            }

            var created = new PaletteWindow(url, BlocksWidth, BlocksHeight, allowResize: false);
            created.MessageReceived += OnPaletteMessage;
            created.Closed += (_, __) =>
            {
                if (ReferenceEquals(_blocks, created))
                    _blocks = null;
            };
            Attach(created, BlocksLeft, BlocksTop);
            _blocks = created;
        }

        private static void EnsureWindow(ref PaletteWindow window, string url, double width, double height, double leftOffset, double topOffset)
        {
            if (window != null)
            {
                try
                {
                    if (!window.IsVisible)
                        window.Show();
                    window.Activate();
                    PositionOverAcad(window, leftOffset, topOffset);
                }
                catch
                {
                }
                return;
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

            PositionOverAcad(created, leftOffset, topOffset);
        }

        private static void PositionOverAcad(PaletteWindow window, double leftOffset, double topOffset)
        {
            if (window == null) return;
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
            var raw = json;
            json = NormalizePaletteJson(json);

            if (TryHandleBlocksChrome(raw) || TryHandleBlocksChrome(json))
                return;

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

        private static bool HasToken(string text, string token)
        {
            return !string.IsNullOrEmpty(text)
                && text.IndexOf(token, StringComparison.OrdinalIgnoreCase) >= 0;
        }

        private static bool TryHandleBlocksChrome(string text)
        {
            if (HasToken(text, "hide-blocks"))
            {
                HidePalette(_blocks);
                return true;
            }

            if (HasToken(text, "collapse-blocks"))
            {
                ApplyBlocksSize(expanded: false);
                return true;
            }

            if (HasToken(text, "expand-blocks"))
            {
                ApplyBlocksSize(expanded: true);
                return true;
            }

            return false;
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
            BlockInsertCommand.FocusDrawing();
            doc.SendStringToExecute("\x03\x03" + command.Trim() + " ", true, false, false);
        }

        private static void RunAcadCommandWhenIdle(string command)
        {
            EventHandler idle = null;
            idle = (s, e) =>
            {
                AcadApp.Idle -= idle;
                BlockInsertCommand.FocusDrawing();
                RunAcadCommand(command);
            };
            AcadApp.Idle += idle;
        }

        private static string NormalizePaletteJson(string raw)
        {
            if (string.IsNullOrWhiteSpace(raw)) return raw;
            var t = raw.Trim();
            if (t.Length >= 2 && t[0] == '"')
            {
                try
                {
                    var unquoted = Newtonsoft.Json.JsonConvert.DeserializeObject<string>(t);
                    if (!string.IsNullOrWhiteSpace(unquoted))
                        t = unquoted.Trim();
                }
                catch
                {
                }
            }

            var start = t.IndexOf('{');
            var end = t.LastIndexOf('}');
            if (start >= 0 && end > start)
                return t.Substring(start, end - start + 1);
            return t;
        }

        private static void RunOnBlocks(Action action)
        {
            if (_blocks == null || action == null) return;
            try
            {
                if (_blocks.Dispatcher.CheckAccess())
                    action();
                else
                    _blocks.Dispatcher.Invoke(action);
            }
            catch
            {
            }
        }

        private static void ApplyBlocksSize(bool expanded)
        {
            RunOnBlocks(() =>
            {
                try
                {
                    if (!_blocks.IsVisible)
                        _blocks.Show();
                    _blocks.SetCollapsedChrome(!expanded);
                    _blocks.SetSize(
                        expanded ? BlocksWidth : BlocksCollapsedWidth,
                        BlocksHeight);
                    PositionOverAcad(_blocks, BlocksLeft, BlocksTop);
                }
                catch
                {
                }
            });
        }

        private static bool TryHandleSessionMessage(string json)
        {
            try
            {
                JObject obj;
                try { obj = JObject.Parse(json); }
                catch { return false; }

                var action = (string)obj["action"];
                if (string.IsNullOrWhiteSpace(action))
                    return false;

                if (string.Equals(action, "company-logo", StringComparison.OrdinalIgnoreCase))
                {
                    var url = (string)obj["url"];
                    if (string.IsNullOrWhiteSpace(url))
                        PluginSplashBrand.Clear();
                    else
                        _ = PluginSplashBrand.SaveFromUrlAsync(url);
                    return true;
                }

                if (string.Equals(action, "plantilla-theme", StringComparison.OrdinalIgnoreCase))
                {
                    PluginPlantillaTheme.Apply(
                        (string)obj["color"],
                        (string)obj["textColor"],
                        (string)obj["logo"]);
                    _session?.RefreshSplashTheme();
                    var logoUrl = (string)obj["logo"];
                    if (string.IsNullOrWhiteSpace(logoUrl))
                        logoUrl = PluginPlantillaTheme.LogoUrl;
                    if (!string.IsNullOrWhiteSpace(logoUrl))
                    {
                        _ = PluginSplashBrand.SaveFromUrlAsync(logoUrl).ContinueWith(__ =>
                        {
                            try
                            {
                                var win = _session;
                                if (win == null) return;
                                win.Dispatcher.BeginInvoke(new Action(() => win.RefreshSplashTheme()));
                            }
                            catch
                            {
                            }
                        });
                    }
                    return true;
                }

                if (string.Equals(action, "session-ready", StringComparison.OrdinalIgnoreCase)
                    || string.Equals(action, "page-ready", StringComparison.OrdinalIgnoreCase))
                {
                    RevealSessionPage();
                    if (string.Equals(action, "session-ready", StringComparison.OrdinalIgnoreCase))
                        MarkAuthenticated();
                    return true;
                }

                if (string.Equals(action, "show-home", StringComparison.OrdinalIgnoreCase))
                {
                    if (_session != null && _session.IsVisible)
                        HideHome();
                    else
                        ShowHome();
                    return true;
                }

                if (string.Equals(action, "hide-home", StringComparison.OrdinalIgnoreCase))
                {
                    HideHome();
                    return true;
                }

                if (string.Equals(action, "show-blocks", StringComparison.OrdinalIgnoreCase))
                {
                    ShowBlocks();
                    return true;
                }

                if (string.Equals(action, "hide-blocks", StringComparison.OrdinalIgnoreCase))
                {
                    HidePalette(_blocks);
                    return true;
                }

                if (string.Equals(action, "collapse-blocks", StringComparison.OrdinalIgnoreCase))
                {
                    ApplyBlocksSize(expanded: false);
                    return true;
                }

                if (string.Equals(action, "expand-blocks", StringComparison.OrdinalIgnoreCase))
                {
                    ApplyBlocksSize(expanded: true);
                    return true;
                }

                if (string.Equals(action, "insert-block", StringComparison.OrdinalIgnoreCase))
                {
                    BlockInsertCommand.QueueFromPalette(obj);
                    ApplyBlocksSize(expanded: false);
                    RunAcadCommandWhenIdle(BlockInsertCommand.CommandName);
                    return true;
                }

                if (string.Equals(action, "convert-blocks", StringComparison.OrdinalIgnoreCase))
                {
                    BlockConvertCommand.QueueFromPalette(obj);
                    RunAcadCommandWhenIdle(BlockConvertCommand.CommandName);
                    return true;
                }

                if (string.Equals(action, "open-design", StringComparison.OrdinalIgnoreCase))
                {
                    var designId = obj["designId"] != null ? (long)obj["designId"] : 0L;
                    var snapshot = obj["snapshot"]?.ToObject<WallSnapshotDto>();
                    WallImportCommand.OpenFromPalette(designId, snapshot);
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
            ed?.WriteMessage("\n" + text);
        }

        private static void Close(ref PaletteWindow window)
        {
            if (window == null) return;
            try { window.Close(); } catch { }
            window = null;
        }
    }
}
