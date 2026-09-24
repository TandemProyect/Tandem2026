using System;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;
using CadApp = Bricscad.ApplicationServices.Application;

namespace BricscadPlugin.UI.Views
{
    /// <summary>
    /// Popup de espera de Desing_2 (tarjeta + spinner "Generando modelo de muros…").
    /// </summary>
    public partial class Wall3dProgressWindow : Window
    {
        public Wall3dProgressWindow()
        {
            InitializeComponent();
        }

        public static Wall3dProgressWindow ShowOverCad()
        {
            var win = new Wall3dProgressWindow();
            try
            {
                win.SetOwnerHandle(CadApp.MainWindow.Handle);
            }
            catch
            {
            }

            CenterOverCad(win);

            try
            {
                CadApp.ShowModelessWindow(win);
            }
            catch
            {
                win.Show();
            }

            Pump(win);
            return win;
        }

        public T Wait<T>(Func<T> backgroundWork)
        {
            var task = Task.Run(backgroundWork);
            while (!task.IsCompleted)
            {
                Pump(this);
                Thread.Sleep(20);
            }
            return task.GetAwaiter().GetResult();
        }

        public void SetOwnerHandle(IntPtr ownerHandle)
        {
            var helper = new System.Windows.Interop.WindowInteropHelper(this)
            {
                Owner = ownerHandle
            };
        }

        private static void CenterOverCad(Window window)
        {
            try
            {
                var main = CadApp.MainWindow;
                var origin = main.DeviceIndependentLocation;
                var size = main.DeviceIndependentSize;
                window.Left = origin.X + Math.Max(40, (size.Width - window.Width) / 2);
                window.Top = origin.Y + Math.Max(40, (size.Height - window.Height) / 2);
            }
            catch
            {
                window.WindowStartupLocation = WindowStartupLocation.CenterScreen;
            }
        }

        private static void Pump(Window window)
        {
            try
            {
                window.Dispatcher.Invoke(() => { }, DispatcherPriority.Render);
            }
            catch
            {
            }
        }
    }
}
