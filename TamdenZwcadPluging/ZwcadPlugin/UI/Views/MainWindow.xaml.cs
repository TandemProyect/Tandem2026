using System.Windows;
using System.Windows.Interop;

namespace ZwcadPlugin.UI.Views
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Vincula el Owner de la ventana WPF al handle de la ventana principal de ZWCAD,
        /// evitando que la ventana quede detrás del host.
        /// </summary>
        public void SetOwnerHandle(System.IntPtr ownerHandle)
        {
            var helper = new WindowInteropHelper(this);
            helper.Owner = ownerHandle;
        }
    }
}
