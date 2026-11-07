using Microsoft.Web.WebView2.Core;
using Microsoft.Web.WebView2.Wpf;
using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Runtime.Loader;
using System.Windows;

namespace OpenCadStudioPluging
{
    internal sealed class OpenCadPaletteWindow : Window
    {
        private readonly string _url;
        private readonly bool _keepOnTop;
        private WebView2 _web;

        public event Action<string> MessageReceived;
        public event Action<Uri> Navigated;

        public OpenCadPaletteWindow(string title, string url, double width, double height, bool keepOnTop = false)
        {
            Title = title;
            Width = width;
            Height = height;
            MinWidth = 120;
            MinHeight = 64;
            ResizeMode = ResizeMode.NoResize;
            WindowStartupLocation = WindowStartupLocation.CenterScreen;
            ShowInTaskbar = false;
            _keepOnTop = keepOnTop;
            if (_keepOnTop)
            {
                Topmost = true;
            }
            _url = url;
            Loaded += OnLoaded;
        }

        public void EnsureVisible()
        {
            if (WindowState == WindowState.Minimized)
                WindowState = WindowState.Normal;

            if (!IsVisible)
                Show();

            if (_keepOnTop && !Topmost)
                Topmost = true;

            Activate();
        }

        public void Navigate(string url)
        {
            if (_web != null && _web.CoreWebView2 != null && !string.IsNullOrWhiteSpace(url))
                _web.CoreWebView2.Navigate(url);
        }

        public static void PrepareNativeLoader()
        {
            var dir = Path.GetDirectoryName(typeof(OpenCadPaletteWindow).Assembly.Location);
            if (string.IsNullOrEmpty(dir))
                return;

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

        private async void OnLoaded(object sender, RoutedEventArgs e)
        {
            try
            {
                PrepareNativeLoader();
                _web = new WebView2
                {
                    DefaultBackgroundColor = System.Drawing.Color.White
                };
                Content = _web;
                var env = await CoreWebView2Environment.CreateAsync(null, OpenCadStudioPluginEnvironment.WebView2Dir());
                await _web.EnsureCoreWebView2Async(env);
                _web.CoreWebView2.Settings.AreDefaultContextMenusEnabled = true;
                _web.CoreWebView2.Settings.AreDevToolsEnabled = false;
                _web.CoreWebView2.ServerCertificateErrorDetected += (_, args) =>
                {
                    args.Action = MvcServerSettings.IsProduction()
                        ? CoreWebView2ServerCertificateErrorAction.Cancel
                        : CoreWebView2ServerCertificateErrorAction.AlwaysAllow;
                };
                _web.CoreWebView2.WebMessageReceived += (_, args) =>
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
                _web.CoreWebView2.NavigationCompleted += (_, __) =>
                {
                    try { Navigated?.Invoke(_web.Source); } catch { }
                };
                _web.CoreWebView2.Navigate(_url);
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "No se pudo cargar el formulario MVC.\n\n" + ex.Message,
                    "Tandem 2026 OpenCADStudio",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);
            }
        }

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern bool SetDllDirectory(string lpPathName);
    }
}
