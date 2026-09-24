using System;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;

namespace TandemRevit.UI.Views
{
    public partial class Wall3dProgressWindow : Window
    {
        public Wall3dProgressWindow()
        {
            InitializeComponent();
        }

        public static Wall3dProgressWindow ShowCentered()
        {
            var win = new Wall3dProgressWindow
            {
                WindowStartupLocation = WindowStartupLocation.CenterScreen
            };
            win.Show();
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
