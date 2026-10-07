using System;
using System.IO;
using System.Threading.Tasks;
using ZwcadPlugin.Models;
using ZwcadPlugin.UI.Views;
using ZwSoft.ZwCAD.ApplicationServices;
using ZwSoft.ZwCAD.EditorInput;
using Newtonsoft.Json.Linq;
using AcadApp = ZwSoft.ZwCAD.ApplicationServices.Application;

namespace ZwcadPlugin
{
    /// <summary>
    /// Paletas MVC sobre ZWCAD: sesión (login / menú general) + modo + herramientas.
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
        private static bool _keepUi;
        private static bool _docsBound;
        private static bool _pendingBlocks;
        private static bool _waitingHome;
        private static string _readyUrl;
        private static string _sessionStartUrl;
        private static int _splashWaitGen;
        private static double _pendingSessionWidth = 400;
        private static double _pendingSessionHeight = 620;

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

        public static void BindDocumentLifetime()
        {
            if (_docsBound)
                return;
            _docsBound = true;
            try
            {
                AcadApp.DocumentManager.DocumentActivated += OnDocumentActivated;
                Atk60LibrarySync.AfterSync = PushCatalogToBlocks;
                WallSpecialCad.Bind();
            }
            catch
            {
            }
        }

        public static void Show()
        {
            BindDocumentLifetime();
            PaletteWindow.PrepareNativeLoader();
            var baseUrl = PluginExceptionHelper.ResolveBaseUrlFromEnv();
            _readyUrl = baseUrl + "DesignToolsAutocad/PluginReady";
            _sessionStartUrl = BuildSessionStartUrl(baseUrl);
            _keepUi = true;

            if (_sessionAuthenticated)
            {
                Atk60LibrarySync.LoadIndexIfPresent();
                ShowToolPalettes();
                EnsureSession(splash: false);
                return;
            }

            if (PluginSessionStore.HasRecent())
            {
                PluginSplashBrand.ForceDefaultUntilPlantilla = false;
                WriteMessage("[Tandem] Reanudando sesión en " + MvcServerSettings.CurrentLabel() + "…");
                Atk60LibrarySync.LoadIndexIfPresent();
                EnsureSession(splash: false);
                return;
            }

            PluginSplashBrand.ForceDefaultUntilPlantilla = !PluginPlantillaTheme.HasCachedTheme();
            EnsureSession(splash: true);
            Atk60LibrarySync.LoadIndexIfPresent();
            ReportConnectStep("Conectando con " + MvcServerSettings.CurrentLabel() + "…");
        }

        /// <summary>
        /// Instalación explícita (comando ATDESING): carpetas AppData + copia de bloques.
        /// NETLOAD / TANDEM no hacen esto.
        /// </summary>
        public static void InstallLibrary()
        {
            BindDocumentLifetime();
            WriteMessage("[Tandem] ATDESING: instalando biblioteca en %LocalAppData%\\AtDesing …");
            Atk60LibrarySync.EnsureFolders();
            RunLibraryUpdate(useSessionSplash: false, connectWhenDone: true);
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
            _keepUi = false;
            _pendingBlocks = false;
        }

        public static string ResetDeveloperLocalState()
        {
            CloseAll();
            PluginAutoload.Uninstall();
            Atk60LibrarySync.ClearMemory();
            PluginSessionStore.Clear();
            PluginSplashBrand.Clear();
            PluginPlantillaTheme.Reset();
            try { System.Threading.Thread.Sleep(400); } catch { }
            var report = PluginDevReset.WipeAll();
            if (Directory.Exists(PaletteWindow.UserDataDir())
                || Directory.Exists(Atk60LibrarySync.ProductRoot()))
            {
                try { System.Threading.Thread.Sleep(600); } catch { }
                report += PluginDevReset.WipeAll();
            }
            return report;
        }

        public static void ReconnectToCurrentServer()
        {
            CloseAll();
            Show();
        }

        private static void HideUi()
        {
            _keepUi = false;
            HideSessionWindow();
            HidePalette(_mode);
            HidePalette(_tools);
            HidePalette(_blocks);
        }

        private static void OnDocumentActivated(object sender, DocumentCollectionEventArgs e)
        {
            if (!_sessionAuthenticated || !_keepUi)
                return;
            try { ShowToolPalettes(); }
            catch { }
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

        private static void EnsureSession(bool splash)
        {
            if (_session != null)
            {
                if (_sessionAuthenticated)
                {
                    HideSessionWindow();
                    ShowToolPalettes();
                    return;
                }
                if (!_session.IsVisible)
                {
                    try { _session.Show(); } catch { }
                }
                _session.Activate();
                return;
            }

            var created = new PaletteWindow(_sessionStartUrl, 400, 620, allowResize: false, authSplash: splash);
            created.LayoutId = "session";
            created.MessageReceived += OnPaletteMessage;
            created.Navigated += OnSessionNavigated;
            created.Closed += (_, __) =>
            {
                if (ReferenceEquals(_session, created))
                    _session = null;
            };

            Attach(created, "session", -1, 70);
            if (splash)
                created.ShowStatus("Conectando con " + MvcServerSettings.CurrentLabel() + "…");
            else
                HideSessionWindow();
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
                PluginSessionStore.Clear();
                Close(ref _mode);
                Close(ref _tools);
                Close(ref _blocks);
                _pendingBlocks = false;
                _pendingSessionWidth = 400;
                _pendingSessionHeight = 620;
                if (_session != null)
                {
                    ShowSessionWindow();
                    PlacePalette(_session, "session", -1, 70);
                    _session.SetLoginChrome(true);
                }
                ReportConnectStep("Esperando que inicies sesión…");
                RevealSessionPage();
                PushLinkSpeed();
                return;
            }

            if (path.IndexOf("/DesignToolsAutocad/PluginSession", StringComparison.OrdinalIgnoreCase) >= 0)
            {
                ReportConnectStep("Comprobando si ya hay sesión…");
                return;
            }

            if (path.IndexOf("/DesignToolsAutocad/PluginCadAuth", StringComparison.OrdinalIgnoreCase) >= 0)
            {
                ReportConnectStep("Validando usuario y equipo…");
                ArmAuthReadyFallback();
                return;
            }

            if (path.IndexOf("/DesignToolsAutocad/PluginReady", StringComparison.OrdinalIgnoreCase) >= 0)
            {
                _pendingSessionWidth = 540;
                _pendingSessionHeight = 700;
                if (_waitingHome)
                {
                    ReportConnectStep("Cargando obras, ofertas y diseños…");
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

        private static void ArmAuthReadyFallback()
        {
            var gen = ++_splashWaitGen;
            Task.Delay(2500).ContinueWith(_ =>
            {
                if (gen != _splashWaitGen || _sessionAuthenticated)
                    return;
                try
                {
                    var win = _session;
                    if (win == null) return;
                    win.Dispatcher.Invoke(() =>
                    {
                        ReportConnectStep("Sesión lista. Abriendo herramientas…");
                        MarkAuthenticated();
                    });
                }
                catch
                {
                }
            });
        }

        private static void RevealSessionPage()
        {
            _splashWaitGen++;
            ShowSessionWindow();
            try
            {
                if (_session != null && _session.IsOnPluginReady())
                    _session.SetLoginChrome(false);
                else
                    _session?.SetLoginChrome(true);
            }
            catch { }
            _session?.HideStatus();
            ApplySessionSize(_pendingSessionWidth, _pendingSessionHeight);
            PlacePalette(_session, "session", -1, 70);
        }

        private static void MarkAuthenticated()
        {
            _splashWaitGen++;
            if (!_sessionAuthenticated)
            {
                _sessionAuthenticated = true;
                _keepUi = true;
                PluginSessionStore.Remember();
            }

            FinishConnectUi();
        }

        private static void RunLibraryUpdate(bool useSessionSplash, bool connectWhenDone = false)
        {
            TandemMiniPopup pop = null;
            if (!useSessionSplash)
                pop = TandemMiniPopup.ShowProgressTop(
                    connectWhenDone ? "ATDESING" : "Actualizar biblioteca",
                    connectWhenDone ? "Instalando biblioteca local…" : "Comprobando cambios…",
                    showBrand: true);

            Atk60LibrarySync.StartInBackground(
                step =>
                {
                    if (useSessionSplash)
                        ReportConnectStep(step);
                    else
                        pop?.AddInstallStep(step);
                },
                () =>
                {
                    Action finish = () =>
                    {
                        Atk60LibrarySync.ClearStep();
                        if (useSessionSplash)
                            FinishConnectUi();
                        else
                        {
                            try { pop?.CloseSafe(); } catch { }
                            PushCatalogToBlocks();
                            if (connectWhenDone && !_sessionAuthenticated)
                            {
                                WriteMessage("[Tandem] Biblioteca lista. Conectando…");
                                Show();
                            }
                            else if (Atk60LibrarySync.HasLocalLibrary())
                                WriteMessage("[Tandem] Biblioteca local lista.");
                            else
                                WriteMessage("[Tandem] ATDESING no pudo completar la biblioteca. Revisa IIS / TANDEM_LOCAL.");
                        }
                    };
                    try
                    {
                        if (_session != null)
                            _session.Dispatcher.BeginInvoke(finish);
                        else
                            finish();
                    }
                    catch
                    {
                        finish();
                    }
                });
        }

        private static void FinishConnectUi()
        {
            Atk60LibrarySync.ClearStep();
            PluginSplashBrand.ForceDefaultUntilPlantilla = false;
            try { _session?.SetLoginChrome(false); } catch { }
            try { _session?.HideStatus(); } catch { }
            HideSessionWindow();
            ShowToolPalettes();
            WriteMessage("[Tandem] Sesión conectada a " + MvcServerSettings.CurrentLabel() + ". El primer botón abre el menú general.");
            PluginAutoload.Disable();
            Atk60LibrarySync.LoadIndexIfPresent();
            PushCatalogToBlocks();
            if (!Atk60LibrarySync.HasLocalLibrary())
                WriteMessage("[Tandem] No hay biblioteca local. Escribe ATDESING para instalar los bloques (una vez).");
            if (_pendingBlocks)
            {
                _pendingBlocks = false;
                ShowBlocks();
            }
        }

        private static void ShowHome()
        {
            EnsureSession(splash: !_sessionAuthenticated);
            if (_session == null) return;

            _waitingHome = true;
            _pendingSessionWidth = 540;
            _pendingSessionHeight = 700;
            ShowSessionWindow();
            _session.ShowStatus(_sessionAuthenticated
                ? "Cargando obras, ofertas y diseños…"
                : "Cargando TDesing…");
            ArmSplashFallback();
            if (!string.IsNullOrWhiteSpace(_readyUrl))
                _session.Navigate(CacheBust(_readyUrl));
        }

        private static string CacheBust(string url)
        {
            if (string.IsNullOrWhiteSpace(url)) return url;
            var sep = url.IndexOf('?') >= 0 ? "&" : "?";
            return url + sep + "t=" + DateTime.UtcNow.Ticks.ToString();
        }

        private static void HideHome()
        {
            _waitingHome = false;
            HideSessionWindow();
        }

        internal static void HideHomeForm()
        {
            HideHome();
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
            EnsureWindow(ref _mode, "mode", modeUrl, 492, 70, 80, 90);
            EnsureWindow(ref _tools, "tools", toolsUrl, 508, 66, -1, 90);
        }

        private const double BlocksWidth = 320;
        private const double BlocksHeight = 680;
        private const double BlocksCollapsedWidth = 36;
        private const double BlocksCollapsedHeight = 46;
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
                    PlacePalette(_blocks, "blocks", BlocksLeft, BlocksTop);
                }
                catch
                {
                }
                return;
            }

            var created = new PaletteWindow(url, BlocksWidth, BlocksHeight, allowResize: false);
            created.LayoutId = "blocks";
            created.MessageReceived += OnPaletteMessage;
            created.Closed += (_, __) =>
            {
                if (ReferenceEquals(_blocks, created))
                    _blocks = null;
            };
            Attach(created, "blocks", BlocksLeft, BlocksTop);
            _blocks = created;
        }

        private static void EnsureWindow(ref PaletteWindow window, string layoutId, string url, double width, double height, double leftOffset, double topOffset)
        {
            if (window != null)
            {
                try
                {
                    if (!window.IsVisible)
                        window.Show();
                    window.Activate();
                    PlacePalette(window, layoutId, leftOffset, topOffset);
                }
                catch
                {
                }
                return;
            }

            var created = new PaletteWindow(url, width, height);
            created.LayoutId = layoutId;
            created.MessageReceived += OnPaletteMessage;
            created.Closed += (_, __) =>
            {
                if (ReferenceEquals(_mode, created)) _mode = null;
                if (ReferenceEquals(_tools, created)) _tools = null;
            };

            Attach(created, layoutId, leftOffset, topOffset);
            window = created;
        }

        private static void Attach(PaletteWindow created, string layoutId, double leftOffset, double topOffset)
        {
            try
            {
                created.SetOwnerHandle(AcadApp.MainWindow.Handle);
            }
            catch
            {
            }

            created.LayoutId = layoutId;
            PlacePalette(created, layoutId, leftOffset, topOffset);

            try
            {
                AcadApp.ShowModelessWindow(created);
            }
            catch
            {
                created.Show();
            }

            PlacePalette(created, layoutId, leftOffset, topOffset);
        }

        private static void PlacePalette(PaletteWindow window, string layoutId, double leftOffset, double topOffset)
        {
            if (window == null) return;
            if (!string.IsNullOrWhiteSpace(layoutId))
                window.LayoutId = layoutId;
            window.PlaceOrRestore(() => PositionOverAcad(window, leftOffset, topOffset));
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
        /// Lanza un comando en ZWCAD desde la paleta (ventana modeless).
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
                    _blocks.Opacity = 1;
                    _blocks.IsHitTestVisible = true;
                    _blocks.SetCollapsedChrome(!expanded);
                    _blocks.SetSize(
                        expanded ? BlocksWidth : BlocksCollapsedWidth,
                        expanded ? BlocksHeight : BlocksCollapsedHeight);
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
                    if (PluginSplashBrand.ForceDefaultUntilPlantilla)
                        return true;
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
                    if (PluginSplashBrand.ForceDefaultUntilPlantilla)
                        return true;
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

                if (string.Equals(action, "login-ready", StringComparison.OrdinalIgnoreCase)
                    || string.Equals(action, "login-submit", StringComparison.OrdinalIgnoreCase))
                {
                    if (string.Equals(action, "login-ready", StringComparison.OrdinalIgnoreCase))
                    {
                        var w = obj["width"] != null ? (double)obj["width"] : 0;
                        var h = obj["height"] != null ? (double)obj["height"] : 0;
                        if (w >= 320 && w <= 520)
                            _pendingSessionWidth = w;
                        if (h >= 420 && h <= 760)
                            _pendingSessionHeight = h;
                        _session?.SetLoginChrome(true);
                        RevealSessionPage();
                        _session?.HidePageLoader();
                        PushLinkSpeed();
                    }
                    else
                    {
                        ReportConnectStep("Conectando…");
                    }
                    return true;
                }

                if (string.Equals(action, "home-ready", StringComparison.OrdinalIgnoreCase)
                    || string.Equals(action, "page-ready", StringComparison.OrdinalIgnoreCase)
                    || string.Equals(action, "request-link-speed", StringComparison.OrdinalIgnoreCase))
                {
                    if (string.Equals(action, "home-ready", StringComparison.OrdinalIgnoreCase)
                        || string.Equals(action, "page-ready", StringComparison.OrdinalIgnoreCase))
                    {
                        if (_waitingHome)
                        {
                            ShowSessionWindow();
                            RevealSessionPage();
                        }
                    }
                    PushLinkSpeed();
                    return true;
                }

                if (string.Equals(action, "session-ready", StringComparison.OrdinalIgnoreCase))
                {
                    if (_sessionAuthenticated)
                        return true;
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

                if (string.Equals(action, "request-catalog", StringComparison.OrdinalIgnoreCase))
                {
                    PushCatalogToBlocks();
                    return true;
                }

                if (string.Equals(action, "update-library", StringComparison.OrdinalIgnoreCase))
                {
                    RunLibraryUpdate(useSessionSplash: false);
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

                if (string.Equals(action, "save-walls", StringComparison.OrdinalIgnoreCase))
                {
                    RunAcadCommandWhenIdle(WallSaveCommand.CommandName);
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
                    case "polyline": return "TANDEM_LINEA";
                    case "wall-2d": return Wall2dCommand.CommandName;
                    case "wall-3d": return Wall3dCommand.CommandName;
                    case "formwork": return FormworkCommand.CommandName;
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
                    case "lines": return "TANDEM_VER2D";
                    case "wall-2d": return "TANDEM_VER2D";
                    case "wall-3d": return Wall3dCommand.CommandName;
                    case "formwork": return FormworkCommand.CommandName;
                }
            }
            catch
            {
            }

            return null;
        }

        private static void ReportConnectStep(string title)
        {
            WriteMessage("[Tandem] " + title);
            _session?.AddInstallStep(title);
        }

        private static void PushLinkSpeed()
        {
            try
            {
                Task.Run(() =>
                {
                    NetworkLinkInfo.RefreshServerPing();
                    string kind;
                    int mbps;
                    NetworkLinkInfo.Read(out kind, out mbps);
                    var json = new JObject
                    {
                        ["action"] = "link-quality",
                        ["kind"] = kind ?? "",
                        ["mbps"] = mbps,
                        ["wifiLine"] = NetworkLinkInfo.FormatWifiLine(),
                        ["serverLine"] = NetworkLinkInfo.FormatServerLine(),
                        ["serverMs"] = NetworkLinkInfo.LastServerMs,
                        ["local"] = MvcServerSettings.IsLocal()
                    }.ToString(Newtonsoft.Json.Formatting.None);
                    _session?.PostToPage(json);
                });
            }
            catch
            {
            }
        }

        private static void PushCatalogToBlocks()
        {
            if (_blocks == null)
                return;
            try
            {
                var rows = Atk60LibrarySync.ReadCatalogJson();
                _blocks.PostToPage("{\"type\":\"catalog\",\"rows\":" + rows + "}");
            }
            catch
            {
            }
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
            try { window.DisposeWebView(); } catch { }
            try { window.Close(); } catch { }
            window = null;
        }
    }
}

