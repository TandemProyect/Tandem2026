using Microsoft.Web.WebView2.Core;
using Microsoft.Web.WebView2.Wpf;
using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Runtime.Loader;
using System.Windows;

namespace FreeCadPluging
{
    internal sealed class FreeCadPaletteWindow : Window
    {
        private readonly string _url;
        private WebView2 _web;

        public event Action<string> MessageReceived;
        public event Action<Uri> Navigated;

        public FreeCadPaletteWindow(string title, string url, double width, double height)
        {
            Title = title;
            Width = width;
            Height = height;
            MinWidth = 120;
            MinHeight = 64;
            ResizeMode = ResizeMode.NoResize;
            WindowStartupLocation = WindowStartupLocation.CenterScreen;
            _url = url;
            Loaded += OnLoaded;
        }

        public void Navigate(string url)
        {
            if (_web != null && _web.CoreWebView2 != null && !string.IsNullOrWhiteSpace(url))
                _web.CoreWebView2.Navigate(url);
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
                var env = await CoreWebView2Environment.CreateAsync(null, FreeCadPluginEnvironment.WebView2Dir());
                await _web.EnsureCoreWebView2Async(env);
                _web.CoreWebView2.Settings.AreDefaultContextMenusEnabled = true;
                _web.CoreWebView2.Settings.AreDevToolsEnabled = false;
                _web.CoreWebView2.ServerCertificateErrorDetected += (_, args) =>
                {
                    args.Action = Program.IsProduction()
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
                    "No se pudo cargar el formulario MVC. Arranca Desing en IIS Express o usa produccion.\n\n" + ex.Message,
                    "Tandem 2026 FreeCAD",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);
            }
        }

        private static void PrepareNativeLoader()
        {
            var dir = Path.GetDirectoryName(typeof(FreeCadPaletteWindow).Assembly.Location);
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

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern bool SetDllDirectory(string lpPathName);
    }
}