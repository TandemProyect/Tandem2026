using System;
using System.Windows;
using System.Windows.Threading;
using Autodesk.AutoCAD.ApplicationServices;
using AcadApp = Autodesk.AutoCAD.ApplicationServices.Application;

namespace AutocadPlugin.UI.Views
{
    /// <summary>
    /// Toast de éxito al estilo Desing (verde, arriba a la derecha, se oculta solo).
    /// </summary>
    public partial class TandemToastWindow : Window
    {
        public TandemToastWindow()
        {
            InitializeComponent();
        }

        public static void ShowSuccess(string message, int hideMs = 3000)
        {
            var win = new TandemToastWindow();
            if (win.MessageText != null)
                win.MessageText.Text = string.IsNullOrWhiteSpace(message) ? "Diseño salvado" : message;

            try
            {
                var helper = new System.Windows.Interop.WindowInteropHelper(win)
                {
                    Owner = AcadApp.MainWindow.Handle
                };
            }
            catch
            {
            }

            PlaceTopRight(win);

            try
            {
                AcadApp.ShowModelessWindow(win);
            }
            catch
            {
                win.Show();
            }

            var timer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(Math.Max(800, hideMs)) };
            timer.Tick += (_, __) =>
            {
                timer.Stop();
                try { win.Close(); } catch { }
            };
            timer.Start();
        }

        private static void PlaceTopRight(Window window)
        {
            try
            {
                var main = AcadApp.MainWindow;
                var origin = main.DeviceIndependentLocation;
                var size = main.DeviceIndependentSize;
                window.Left = origin.X + Math.Max(24, size.Width - 380);
                window.Top = origin.Y + 72;
            }
            catch
            {
                window.WindowStartupLocation = WindowStartupLocation.CenterScreen;
            }
        }
    }
}
