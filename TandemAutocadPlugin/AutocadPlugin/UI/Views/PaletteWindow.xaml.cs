using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Input;
using Microsoft.Web.WebView2.Core;

namespace AutocadPlugin.UI.Views
{
    public partial class PaletteWindow : Window
    {
        private readonly string _url;
        public event Action<string> MessageReceived;
        public event Action<Uri> Navigated;

        public PaletteWindow(string url, double width, double height, bool allowResize = false)
        {
            InitializeComponent();
            Title = string.Empty;
            Width = width;
            Height = height;
            _url = url;
            if (allowResize)
                ResizeMode = ResizeMode.CanResizeWithGrip;
            Loaded += OnLoaded;
        }

        public void SetOwnerHandle(IntPtr ownerHandle)
        {
            var helper = new System.Windows.Interop.WindowInteropHelper(this)
            {
                Owner = ownerHandle
            };
        }

        public void SetSize(double width, double height)
        {
            Width = width;
            Height = height;
        }

        public void Navigate(string url)
        {
            if (Web?.CoreWebView2 != null && !string.IsNullOrWhiteSpace(url))
                Web.CoreWebView2.Navigate(url);
        }

        private async void OnLoaded(object sender, RoutedEventArgs e)
        {
            try
            {
                PrepareNativeLoader();
                var userData = Path.Combine(Path.GetTempPath(), "TandemAutocadWebView2");
                var env = await CoreWebView2Environment.CreateAsync(null, userData);
                await Web.EnsureCoreWebView2Async(env);
                Web.CoreWebView2.Settings.AreDefaultContextMenusEnabled = true;
                Web.CoreWebView2.Settings.AreDevToolsEnabled = false;
                Web.CoreWebView2.ServerCertificateErrorDetected += (_, args) =>
                {
                    args.Action = CoreWebView2ServerCertificateErrorAction.AlwaysAllow;
                };
                Web.CoreWebView2.WebMessageReceived += (_, args) =>
                {
                    MessageReceived?.Invoke(args.TryGetWebMessageAsString());
                };
                Web.CoreWebView2.NavigationCompleted += (_, __) =>
                {
                    try { Navigated?.Invoke(Web.Source); } catch { }
                };
                Web.CoreWebView2.Navigate(_url);
            }
            catch (Exception ex)
            {
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

        [DllImport("kernel32", SetLastError = true, CharSet = CharSet.Unicode)]
        private static extern bool SetDllDirectory(string lpPathName);

        private void OnDrag(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
                DragMove();
        }
    }
}
