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
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
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
        private const double SplashWidth = 320;
        private const double SplashHeight = 292;
        private DispatcherTimer _etaTimer;
        private DateTime _etaStartedAt;
        private int _etaPredictedMs;
        private bool _etaCold;
        public event Action<string> MessageReceived;
        public event Action<Uri> Navigated;

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

            if (_authSplash)
            {
                ApplySplashWindowSize();
                ApplySplashLogo(PluginSplashBrand.CachedFileIfExists());
                ShowStatus("Comprobando autorización en TDesing…");
                SourceInitialized += (_, __) => ApplySplashWindowSize();
            }

            Loaded += OnLoaded;
            Closed += (_, __) => StopEtaTicker(record: false);
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
                ApplySplashWindowSize();
                return;
            }
            SizeToContent = SizeToContent.Manual;
            MaxWidth = double.PositiveInfinity;
            MaxHeight = double.PositiveInfinity;
            MinWidth = Math.Max(16, Math.Min(width, 48));
            MinHeight = Math.Max(16, Math.Min(height, 36));
            Width = width;
            Height = height;
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
                    WebHost.Margin = collapsed ? new Thickness(0) : new Thickness(7, 6, 7, 6);
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
            return _authSplash && SplashPanel != null && SplashPanel.Visibility == Visibility.Visible;
        }

        private void ApplySplashWindowSize()
        {
            SizeToContent = SizeToContent.Height;
            MinWidth = SplashWidth;
            MaxWidth = SplashWidth;
            Width = SplashWidth;
            MinHeight = 0;
            Height = SplashHeight;
        }

        public void ShowStatus(string text)
        {
            if (_authSplash)
                ApplySplashWindowSize();
            if (!string.IsNullOrWhiteSpace(text) && StatusText != null)
                StatusText.Text = text;
            if (SplashPanel != null)
                SplashPanel.Visibility = Visibility.Visible;
            ParkWeb();
            ApplySplashLogo(PluginSplashBrand.CachedFileIfExists());
            StartSpinner();
            StartEtaTicker();
        }

        public void ApplySplashLogo(string filePath)
        {
            if (SplashLogo == null) return;
            try
            {
                if (!string.IsNullOrWhiteSpace(filePath) && File.Exists(filePath))
                {
                    using (var fs = File.OpenRead(filePath))
                    {
                        var bmp = new BitmapImage();
                        bmp.BeginInit();
                        bmp.CacheOption = BitmapCacheOption.OnLoad;
                        bmp.StreamSource = fs;
                        bmp.EndInit();
                        bmp.Freeze();
                        SplashLogo.Source = bmp;
                    }
                    return;
                }
            }
            catch
            {
            }

            SplashLogo.Source = new BitmapImage(PluginSplashBrand.DefaultPackUri);
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
                    ApplySplashLogo(null);
                    return;
                }

                await PluginSplashBrand.SaveFromUrlAsync(logoUrl);
                ApplySplashLogo(PluginSplashBrand.CachedFileIfExists());
            }
            catch
            {
            }
        }

        public void HideStatus()
        {
            RevealWeb();
        }

        public void Navigate(string url)
        {
            if (Web?.CoreWebView2 != null && !string.IsNullOrWhiteSpace(url))
                Web.CoreWebView2.Navigate(url);
        }

        public void ClearCookiesForSite(string siteUrl)
        {
            var web = Web?.CoreWebView2;
            if (web == null || string.IsNullOrWhiteSpace(siteUrl)) return;
            _ = ClearCookiesForSiteAsync(web, siteUrl);
        }

        private void EnsureWebControl()
        {
            if (Web != null) return;
            Web = new WebView2
            {
                DefaultBackgroundColor = System.Drawing.Color.FromArgb(255, 245, 245, 249),
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
            StopSpinner();
            if (SplashPanel != null)
                SplashPanel.Visibility = Visibility.Collapsed;
            SizeToContent = SizeToContent.Manual;
            MaxWidth = double.PositiveInfinity;
            MinWidth = 120;
            MinHeight = 58;
            AttachWeb();
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

        private void StartSpinner()
        {
            StartForever(SpinArc, 0.85);
            StartForever(SpinHands, 1.35);
        }

        private static void StartForever(RotateTransform spin, double seconds)
        {
            if (spin == null) return;
            var anim = new DoubleAnimation(0, 360, TimeSpan.FromSeconds(seconds))
            {
                RepeatBehavior = RepeatBehavior.Forever
            };
            spin.BeginAnimation(RotateTransform.AngleProperty, anim);
        }

        private void StopSpinner()
        {
            SpinArc?.BeginAnimation(RotateTransform.AngleProperty, null);
            SpinHands?.BeginAnimation(RotateTransform.AngleProperty, null);
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
            if (EtaText == null)
                return;
            var elapsed = Math.Max(0, (int)(DateTime.UtcNow - _etaStartedAt).TotalMilliseconds);
            if (!_etaCold && elapsed > _etaPredictedMs + 1500)
            {
                _etaCold = true;
                _etaPredictedMs = ConnectEta.ColdMs();
            }

            var remaining = Math.Max(1100, (int)(_etaPredictedMs - elapsed * 0.72));
            var coldHang = _etaCold && elapsed > _etaPredictedMs;
            EtaText.Text = ConnectEta.Format(remaining, coldHang);

            double pct;
            if (coldHang)
                pct = Math.Min(96, 78 + (elapsed - _etaPredictedMs) / 4000.0);
            else
                pct = Math.Max(6, Math.Min(92, elapsed / (double)Math.Max(1, _etaPredictedMs) * 88));

            if (EtaBarFill != null && EtaBarTrack != null)
            {
                var track = EtaBarTrack.ActualWidth;
                if (track > 1)
                    EtaBarFill.Width = Math.Max(8, track * pct / 100.0);
            }
        }

        private static async Task ClearCookiesForSiteAsync(CoreWebView2 web, string siteUrl)
        {
            try
            {
                var cookies = await web.CookieManager.GetCookiesAsync(siteUrl);
                foreach (var cookie in cookies)
                {
                    var name = cookie.Name ?? "";
                    if (name.IndexOf("AspNet", StringComparison.OrdinalIgnoreCase) >= 0
                        || name.IndexOf("ApplicationCookie", StringComparison.OrdinalIgnoreCase) >= 0)
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
                    ApplySplashWindowSize();
                    ShowStatus("Comprobando autorización en TDesing…");
                    await Dispatcher.InvokeAsync(() => { }, System.Windows.Threading.DispatcherPriority.Render);
                    await Task.Delay(80);
                }

                EnsureWebControl();
                if (_authSplash)
                    ParkWeb();
                else
                    AttachWeb();

                PrepareNativeLoader();
                var userData = Path.Combine(Path.GetTempPath(), "TandemAutocadWebView2");
                var env = await CoreWebView2Environment.CreateAsync(null, userData);
                await Web.EnsureCoreWebView2Async(env);
                try
                {
                    Web.DefaultBackgroundColor = System.Drawing.Color.FromArgb(255, 245, 245, 249);
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
                    args.Action = CoreWebView2ServerCertificateErrorAction.AlwaysAllow;
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
                Web.CoreWebView2.NavigationCompleted += (_, __) =>
                {
                    try { Navigated?.Invoke(Web.Source); } catch { }
                };
                if (_authSplash)
                    await SyncSplashLogoFromCookiesAsync();
                if (!string.IsNullOrWhiteSpace(_url))
                    Web.CoreWebView2.Navigate(_url);
            }
            catch (Exception ex)
            {
                StopEtaTicker(record: false);
                if (StatusText != null)
                    StatusText.Text = "No se pudo conectar con TDesing.";
                if (EtaText != null)
                    EtaText.Text = "Comprueba que Desing esté en ejecución.";
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
            if (e.ChangedButton == MouseButton.Left)
                DragMove();
        }
    }
}
