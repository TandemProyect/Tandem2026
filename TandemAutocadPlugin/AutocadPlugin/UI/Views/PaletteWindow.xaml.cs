using System;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Threading;
using Microsoft.Web.WebView2.Core;
using Microsoft.Web.WebView2.Wpf;

namespace AutocadPlugin.UI.Views
{
    public partial class PaletteWindow : Window
    {
        private readonly string _url;
        private readonly bool _authSplash;
        private readonly bool _allowResize;
        private WebView2 Web;
        private TandemMiniPopup _splashPopup;
        private string _splashTitle = "Comprobando autorización en TDesing…";
        private DispatcherTimer _etaTimer;
        private DateTime _etaStartedAt;
        private int _etaPredictedMs;
        private bool _etaCold;
        public event Action<string> MessageReceived;
        public event Action<Uri> Navigated;
        public string LayoutId { get; set; }
        private bool _placing;
        private bool _pageShown;

        public PaletteWindow(string url, double width, double height, bool allowResize = false, bool authSplash = false)
        {
            InitializeComponent();
            Title = string.Empty;
            Width = width;
            Height = height;
            _url = url;
            _authSplash = authSplash;
            _allowResize = allowResize;
            if (_allowResize)
                ResizeMode = ResizeMode.CanResizeWithGrip;

            Background = Brushes.White;
            if (!_authSplash)
                ShowPageLoader();

            if (_authSplash)
            {
                HideHostDuringSplash();
                ShowStatus("Comprobando autorización en TDesing…");
                SourceInitialized += (_, __) =>
                {
                    if (IsSplashVisible())
                        HideHostDuringSplash();
                };
            }

            Loaded += OnLoaded;
            LocationChanged += (_, __) => RememberLocation();
            Closed += (_, __) =>
            {
                RememberLocation();
                StopEtaTicker(record: false);
                CloseSplashPopup();
                DisposeWebView();
            };
        }

        public void PlaceOrRestore(Action fallback)
        {
            _placing = true;
            try
            {
                double left;
                double top;
                if (!string.IsNullOrWhiteSpace(LayoutId)
                    && PaletteLayoutStore.TryGet(LayoutId, out left, out top))
                {
                    Left = left;
                    Top = top;
                    return;
                }
                fallback?.Invoke();
            }
            finally
            {
                _placing = false;
            }
        }

        private void RememberLocation()
        {
            if (_placing || string.IsNullOrWhiteSpace(LayoutId) || !IsVisible)
                return;
            try
            {
                if (Opacity < 0.2 || ActualWidth < 8 || ActualHeight < 8)
                    return;
                PaletteLayoutStore.Save(LayoutId, Left, Top);
            }
            catch
            {
            }
        }

        public void SetOwnerHandle(IntPtr ownerHandle)
        {
            var helper = new WindowInteropHelper(this)
            {
                Owner = ownerHandle
            };
        }

        public void SetSize(double width, double height)
        {
            if (IsSplashVisible())
            {
                HideHostDuringSplash();
                return;
            }
            SizeToContent = SizeToContent.Manual;
            var w = Math.Max(16, width);
            var h = Math.Max(16, height);
            MinWidth = 16;
            MaxWidth = double.PositiveInfinity;
            MinHeight = 16;
            MaxHeight = double.PositiveInfinity;
            Width = w;
            Height = h;
            MinWidth = w;
            MaxWidth = w;
            MinHeight = h;
            MaxHeight = h;
        }

        public void SetCollapsedChrome(bool collapsed)
        {
            try
            {
                if (RootChrome != null)
                {
                    RootChrome.CornerRadius = new CornerRadius(collapsed ? 16 : 10);
                    RootChrome.BorderThickness = new Thickness(1);
                    Brush fill = Brushes.White;
                    if (!collapsed)
                    {
                        try { fill = (Brush)FindResource("SplashBg"); }
                        catch { fill = (Brush)new BrushConverter().ConvertFrom("#F5F5F9"); }
                    }
                    RootChrome.Background = fill;
                }
                if (WebHost != null)
                    WebHost.Margin = collapsed ? new Thickness(2, 10, 2, 2) : new Thickness(7, 10, 7, 6);
                Background = collapsed
                    ? Brushes.White
                    : (Brush)new BrushConverter().ConvertFrom("#F5F5F9");
                ResizeMode = collapsed
                    ? ResizeMode.NoResize
                    : (_allowResize ? ResizeMode.CanResizeWithGrip : ResizeMode.NoResize);
            }
            catch
            {
            }
        }

        private bool IsSplashVisible()
        {
            return _authSplash && _splashPopup != null && _splashPopup.IsVisible;
        }

        private void HideHostDuringSplash()
        {
            try
            {
                Opacity = 0;
                IsHitTestVisible = false;
                ShowInTaskbar = false;
                SizeToContent = SizeToContent.Manual;
                MinWidth = 1;
                MaxWidth = 1;
                Width = 1;
                MinHeight = 1;
                MaxHeight = 1;
                Height = 1;
            }
            catch
            {
            }
        }

        private void RestoreHostAfterSplash()
        {
            try
            {
                Opacity = 1;
                IsHitTestVisible = true;
                MaxWidth = double.PositiveInfinity;
                MaxHeight = double.PositiveInfinity;
            }
            catch
            {
            }
        }

        public void ShowStatus(string text)
        {
            if (!string.IsNullOrWhiteSpace(text))
                _splashTitle = text;
            HideHostDuringSplash();
            ParkWeb();
            EnsureSplashPopup();
            if (_splashPopup != null)
            {
                _splashPopup.ShowProgress(
                    PluginSplashBrand.ForceDefaultUntilPlantilla ? "Instalación TDesing" : "Conectando TDesing",
                    text ?? _splashTitle,
                    showBrand: true);
                if (!string.IsNullOrWhiteSpace(text))
                    _splashPopup.AddInstallStep(text);
            }
            StartEtaTicker();
        }

        public void SetStatusTitle(string text)
        {
            AddInstallStep(text);
        }

        public void AddInstallStep(string text)
        {
            if (string.IsNullOrWhiteSpace(text) || _splashPopup == null)
                return;
            try { _splashPopup.AddInstallStep(text); }
            catch { }
        }

        public void RefreshSplashTheme()
        {
            try { _splashPopup?.RefreshTheme(); }
            catch { }
        }

        private void EnsureSplashPopup()
        {
            if (_splashPopup != null)
            {
                try
                {
                    if (_splashPopup.IsVisible)
                    {
                        PrefetchPlantillaLogo();
                        return;
                    }
                }
                catch
                {
                    _splashPopup = null;
                }
            }

            var pop = TandemMiniPopup.ShowProgressTop(
                _splashTitle,
                ConnectEta.Format(ConnectEta.PredictedStartMs(), false),
                showBrand: true);
            _splashPopup = pop;
            pop.Closed += (s, __) =>
            {
                if (ReferenceEquals(_splashPopup, s))
                    _splashPopup = null;
            };
            PrefetchPlantillaLogo();
        }

        private void PrefetchPlantillaLogo()
        {
            if (PluginSplashBrand.ForceDefaultUntilPlantilla)
                return;
            var url = PluginPlantillaTheme.LogoUrl;
            if (string.IsNullOrWhiteSpace(url))
                return;
            _ = PluginSplashBrand.SaveFromUrlAsync(url).ContinueWith(_ =>
            {
                try { Dispatcher.BeginInvoke(new Action(RefreshSplashTheme)); }
                catch { }
            });
        }

        private void CloseSplashPopup()
        {
            var pop = _splashPopup;
            _splashPopup = null;
            if (pop == null)
                return;
            try { pop.CloseSafe(); }
            catch { }
        }

        public async Task SyncSplashLogoFromCookiesAsync()
        {
            var web = Web?.CoreWebView2;
            if (web == null || string.IsNullOrWhiteSpace(_url))
                return;

            try
            {
                var origin = new Uri(_url).GetLeftPart(UriPartial.Authority) + "/";
                var cookies = await web.CookieManager.GetCookiesAsync(origin);
                var logoCookie = cookies?.FirstOrDefault(c =>
                    string.Equals(c.Name, PluginSplashBrand.LogoCookieName, StringComparison.OrdinalIgnoreCase));
                var logoUrl = logoCookie != null ? logoCookie.Value : null;
                if (string.IsNullOrWhiteSpace(logoUrl))
                {
                    PluginSplashBrand.Clear();
                    return;
                }

                await PluginSplashBrand.SaveFromUrlAsync(logoUrl);
            }
            catch
            {
            }
        }

        public void HideStatus()
        {
            CloseSplashPopup();
            RestoreHostAfterSplash();
            RevealWeb();
        }

        public bool IsOnPluginReady()
        {
            try
            {
                var path = Web?.Source?.AbsolutePath ?? "";
                return path.IndexOf("/DesignToolsAutocad/PluginReady", StringComparison.OrdinalIgnoreCase) >= 0;
            }
            catch
            {
                return false;
            }
        }

        public void Navigate(string url)
        {
            if (Web?.CoreWebView2 != null && !string.IsNullOrWhiteSpace(url))
                Web.CoreWebView2.Navigate(url);
        }

        public void PostToPage(string json)
        {
            try
            {
                var web = Web?.CoreWebView2;
                if (web == null || string.IsNullOrWhiteSpace(json))
                    return;
                Action send = () =>
                {
                    try { web.PostWebMessageAsString(json); }
                    catch { }
                };
                if (Dispatcher.CheckAccess())
                    send();
                else
                    Dispatcher.BeginInvoke(send);
            }
            catch
            {
            }
        }

        public void ClearCookiesForSite(string siteUrl)
        {
            var web = Web?.CoreWebView2;
            if (web == null || string.IsNullOrWhiteSpace(siteUrl)) return;
            _ = ClearCookiesForSiteAsync(web, siteUrl, all: false);
        }

        public void ClearAllCookiesForSite(string siteUrl)
        {
            var web = Web?.CoreWebView2;
            if (web == null || string.IsNullOrWhiteSpace(siteUrl)) return;
            _ = ClearCookiesForSiteAsync(web, siteUrl, all: true);
        }

        private void ShowPageLoader()
        {
            try
            {
                Background = Brushes.White;
                if (RootChrome != null)
                    RootChrome.Background = Brushes.White;
                if (PageLoader != null)
                {
                    PageLoader.Visibility = Visibility.Visible;
                    if (PageLoaderText != null)
                        PageLoaderText.Text = "Cargando…";
                }
            }
            catch
            {
            }
        }

        public void HidePageLoader()
        {
            if (_pageShown)
            {
                try { AttachWeb(); } catch { }
                return;
            }
            _pageShown = true;
            try
            {
                AttachWeb();
                if (PageLoader != null)
                    PageLoader.Visibility = Visibility.Collapsed;
            }
            catch
            {
            }
        }

        private void EnsureWebControl()
        {
            if (Web != null) return;
            Web = new WebView2
            {
                DefaultBackgroundColor = System.Drawing.Color.White,
                HorizontalAlignment = HorizontalAlignment.Stretch,
                VerticalAlignment = VerticalAlignment.Stretch
            };
        }

        private void DetachWeb()
        {
            if (Web == null) return;
            var parent = Web.Parent as Panel;
            parent?.Children.Remove(Web);
        }

        private void ParkWeb()
        {
            EnsureWebControl();
            DetachWeb();
            Web.Width = 8;
            Web.Height = 8;
            Canvas.SetLeft(Web, -8000);
            Canvas.SetTop(Web, -8000);
            if (WebPark != null)
                WebPark.Children.Add(Web);
            HideWebHwnd();
        }

        private void AttachWeb()
        {
            EnsureWebControl();
            DetachWeb();
            Web.Width = double.NaN;
            Web.Height = double.NaN;
            Web.ClearValue(Canvas.LeftProperty);
            Web.ClearValue(Canvas.TopProperty);
            if (WebHost != null)
                WebHost.Children.Add(Web);
            Web.Visibility = Visibility.Visible;
            ShowWebHwnd();
        }

        private void RevealWeb()
        {
            StopEtaTicker(record: true);
            SizeToContent = SizeToContent.Manual;
            MaxWidth = double.PositiveInfinity;
            MinWidth = 120;
            MinHeight = 58;
            HidePageLoader();
        }

        private void HideWebHwnd()
        {
            if (Web == null) return;
            Web.Visibility = Visibility.Collapsed;
            try
            {
                var hwnd = (Web as HwndHost)?.Handle ?? IntPtr.Zero;
                if (hwnd != IntPtr.Zero)
                    ShowWindow(hwnd, SW_HIDE);
            }
            catch
            {
            }
        }

        private void ShowWebHwnd()
        {
            if (Web == null) return;
            Web.Visibility = Visibility.Visible;
            try
            {
                var hwnd = (Web as HwndHost)?.Handle ?? IntPtr.Zero;
                if (hwnd != IntPtr.Zero)
                    ShowWindow(hwnd, SW_SHOW);
            }
            catch
            {
            }
        }

        private void StartEtaTicker()
        {
            if (_etaTimer != null)
                return;
            _etaStartedAt = DateTime.UtcNow;
            _etaPredictedMs = ConnectEta.PredictedStartMs();
            _etaCold = false;
            _etaTimer = new DispatcherTimer
            {
                Interval = TimeSpan.FromMilliseconds(280)
            };
            _etaTimer.Tick += (_, __) => RefreshEta();
            RefreshEta();
            _etaTimer.Start();
        }

        private void StopEtaTicker(bool record)
        {
            if (_etaTimer != null)
            {
                _etaTimer.Stop();
                _etaTimer = null;
            }
            if (record && _etaStartedAt != DateTime.MinValue)
            {
                var elapsed = (int)(DateTime.UtcNow - _etaStartedAt).TotalMilliseconds;
                ConnectEta.Record(elapsed);
            }
            _etaStartedAt = DateTime.MinValue;
        }

        private void RefreshEta()
        {
            if (_splashPopup == null)
                return;
            var elapsed = Math.Max(0, (int)(DateTime.UtcNow - _etaStartedAt).TotalMilliseconds);
            if (!_etaCold && elapsed > _etaPredictedMs + 1500)
            {
                _etaCold = true;
                _etaPredictedMs = ConnectEta.ColdMs();
            }

            var remaining = Math.Max(1100, (int)(_etaPredictedMs - elapsed * 0.72));
            var coldHang = _etaCold && elapsed > _etaPredictedMs;
            _splashPopup.SetProgressDetail(FirstRunEta(ConnectEta.Format(remaining, coldHang)));
        }

        private static string FirstRunEta(string eta)
        {
            if (!PluginSplashBrand.ForceDefaultUntilPlantilla)
                return eta;
            return "Primera instalación · " + eta;
        }

        private static async Task ClearCookiesForSiteAsync(CoreWebView2 web, string siteUrl, bool all)
        {
            try
            {
                var cookies = await web.CookieManager.GetCookiesAsync(siteUrl);
                foreach (var cookie in cookies)
                {
                    var name = cookie.Name ?? "";
                    if (all
                        || name.IndexOf("AspNet", StringComparison.OrdinalIgnoreCase) >= 0
                        || name.IndexOf("ApplicationCookie", StringComparison.OrdinalIgnoreCase) >= 0
                        || string.Equals(name, PluginSplashBrand.LogoCookieName, StringComparison.OrdinalIgnoreCase))
                        web.CookieManager.DeleteCookie(cookie);
                }
            }
            catch
            {
            }
        }

        private async void OnLoaded(object sender, RoutedEventArgs e)
        {
            try
            {
                if (_authSplash)
                {
                    HideHostDuringSplash();
                    ShowStatus(PluginSplashBrand.ForceDefaultUntilPlantilla
                        ? "Primera instalación de TDesing"
                        : "Comprobando autorización en TDesing…");
                    await Dispatcher.InvokeAsync(() => { }, DispatcherPriority.Render);
                    await Task.Delay(80);
                }

                EnsureWebControl();
                ParkWeb();
                if (!_authSplash)
                    ShowPageLoader();

                PrepareNativeLoader();
                var userData = UserDataDir();
                var env = await CoreWebView2Environment.CreateAsync(null, userData);
                await Web.EnsureCoreWebView2Async(env);
                try
                {
                    Web.DefaultBackgroundColor = System.Drawing.Color.White;
                }
                catch
                {
                }

                if (_authSplash)
                    ParkWeb();

                Web.CoreWebView2.Settings.AreDefaultContextMenusEnabled = true;
                Web.CoreWebView2.Settings.AreDevToolsEnabled = false;
                Web.CoreWebView2.ServerCertificateErrorDetected += (_, args) =>
                {
                    args.Action = MvcServerSettings.IsProduction()
                        ? CoreWebView2ServerCertificateErrorAction.Cancel
                        : CoreWebView2ServerCertificateErrorAction.AlwaysAllow;
                };
                Web.CoreWebView2.WebMessageReceived += (_, args) =>
                {
                    string text = null;
                    try { text = args.TryGetWebMessageAsString(); } catch { }
                    if (string.IsNullOrWhiteSpace(text))
                    {
                        try { text = args.WebMessageAsJson; } catch { }
                    }
                    if (!string.IsNullOrWhiteSpace(text))
                        MessageReceived?.Invoke(text);
                };
                Web.CoreWebView2.NavigationCompleted += (_, args) =>
                {
                    try { Navigated?.Invoke(Web.Source); } catch { }
                    if (!_authSplash && args != null && args.IsSuccess)
                        HidePageLoader();
                };
                if (_authSplash && PluginSplashBrand.ForceDefaultUntilPlantilla)
                {
                    ShowStatus("Primera instalación: limpiando cookies antiguas…");
                    try
                    {
                        var origin = new Uri(_url).GetLeftPart(UriPartial.Authority) + "/";
                        await ClearCookiesForSiteAsync(Web.CoreWebView2, origin, all: true);
                    }
                    catch
                    {
                    }
                    PluginSplashBrand.Clear();
                    ShowStatus("Conectando con el servidor TDesing…");
                }
                else if (_authSplash)
                    await SyncSplashLogoFromCookiesAsync();
                if (!string.IsNullOrWhiteSpace(_url))
                    Web.CoreWebView2.Navigate(_url);
            }
            catch (Exception ex)
            {
                StopEtaTicker(record: false);
                try { _splashPopup?.SetProgressDetail("No se pudo conectar con TDesing."); }
                catch { }
                MessageBox.Show(
                    "No se pudo cargar el formulario MVC.\n" +
                    "Si falta WebView2Loader.dll, vuelve a NETLOAD esta carpeta del plugin.\n" +
                    "Si es un error de conexión, arranca Desing en IIS Express.\n\n" +
                    ex.Message,
                    "Tandem 2026",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);
            }
        }

        public static string UserDataDir()
        {
            return Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                "AtDesing",
                "WebView2");
        }

        public void DisposeWebView()
        {
            try
            {
                if (Web == null)
                    return;
                DetachWeb();
                Web.Dispose();
                Web = null;
            }
            catch
            {
            }
        }

        internal static void PrepareNativeLoader()
        {
            var dir = Path.GetDirectoryName(typeof(PaletteWindow).Assembly.Location);
            if (string.IsNullOrEmpty(dir)) return;

            var loader = Path.Combine(dir, "WebView2Loader.dll");
            var nested = Path.Combine(dir, "runtimes", "win-x64", "native", "WebView2Loader.dll");
            if (!File.Exists(loader) && File.Exists(nested))
                File.Copy(nested, loader, overwrite: true);

            try { CoreWebView2Environment.SetLoaderDllFolderPath(dir); } catch { }
            try { SetDllDirectory(dir); } catch { }
            try
            {
                if (File.Exists(loader))
                    NativeLibrary.Load(loader);
            }
            catch
            {
            }
        }

        private const int SW_HIDE = 0;
        private const int SW_SHOW = 5;

        [DllImport("user32.dll")]
        private static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

        [DllImport("kernel32", SetLastError = true, CharSet = CharSet.Unicode)]
        private static extern bool SetDllDirectory(string lpPathName);

        private void OnDrag(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton != MouseButton.Left)
                return;
            try { DragMove(); } catch { }
            RememberLocation();
        }
    }
}
