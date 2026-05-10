using System;
using System.Windows;
using System.Windows.Interop;

namespace ZwcadPlugin.UI.Views
{
    public partial class CreateDesignWindow : Window
    {
        public CreateDesignWindow(string url)
        {
            InitializeComponent();
            TxtHeader.Text = $"Formulario: {url}";
            Loaded += (s, e) => BrowserHost.Navigate(url);
        }

        public void SetOwnerHandle(IntPtr ownerHandle)
        {
            var helper = new WindowInteropHelper(this);
            helper.Owner = ownerHandle;
        }
    }
}
