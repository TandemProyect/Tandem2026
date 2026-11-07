using System;
using System.IO;

namespace OpenCadStudioPluging
{
    internal static class OpenCadStudioPluginEnvironment
    {
        public static string ProductRoot()
        {
            return Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                "Tandem",
                "OpenCadStudioPluging");
        }

        public static string MvcSettingsPath()
        {
            return Path.Combine(ProductRoot(), "mvc-target.json");
        }

        public static string SessionStorePath()
        {
            return Path.Combine(ProductRoot(), "cad-session-boot.json");
        }

        public static string WebView2Dir()
        {
            return Path.Combine(ProductRoot(), "WebView2");
        }

        public static void EnsureFolders()
        {
            Directory.CreateDirectory(ProductRoot());
            Directory.CreateDirectory(WebView2Dir());
        }

        public static string ResolveOpenCadStudioExecutable()
        {
            var configured = Environment.GetEnvironmentVariable("TANDEM_OPENCADSTUDIO_PATH");
            if (!string.IsNullOrWhiteSpace(configured))
                return configured.Trim();
            return "OpenCADStudio";
        }
    }
}
