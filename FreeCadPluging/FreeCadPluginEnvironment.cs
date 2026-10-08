using System;
using System.IO;

namespace FreeCadPluging
{
    internal static class FreeCadPluginEnvironment
    {
        public static string ProductRoot()
        {
            return Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                "Tandem",
                "FreecadPlugin");
        }

        public static string RequestDir()
        {
            return Path.Combine(ProductRoot(), "requests");
        }

        public static string WebView2Dir()
        {
            return Path.Combine(ProductRoot(), "WebView2");
        }

        public static void EnsureFolders()
        {
            Directory.CreateDirectory(ProductRoot());
            Directory.CreateDirectory(RequestDir());
            Directory.CreateDirectory(WebView2Dir());
        }
    }
}