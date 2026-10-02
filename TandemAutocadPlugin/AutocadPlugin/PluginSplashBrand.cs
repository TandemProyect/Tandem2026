using System;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;

namespace AutocadPlugin
{
    /// <summary>
    /// Logo del splash CAD: TDesing por defecto; tras login se cachea el de la empresa
    /// mientras exista la cookie tandem_plugin_company_logo.
    /// </summary>
    internal static class PluginSplashBrand
    {
        public const string LogoCookieName = "tandem_plugin_company_logo";

        public static readonly Uri DefaultPackUri =
            new Uri("pack://application:,,,/AutocadPlugin;component/UI/Assets/tdesing-logo.png");

        /// <summary>
        /// Primera instalación: siempre TDesing, sin cookie ni caché de empresa.
        /// Se apaga al llegar el logo de la plantilla del usuario.
        /// </summary>
        public static bool ForceDefaultUntilPlantilla { get; set; }

        public static string CacheDir => Path.Combine(Path.GetTempPath(), "TandemAutocadWebView2");
        public static string CacheFile => Path.Combine(CacheDir, "splash-company-logo.bin");
        public static string CacheUrlFile => Path.Combine(CacheDir, "splash-company-logo.url");

        public static string CachedFileIfExists()
        {
            return File.Exists(CacheFile) ? CacheFile : null;
        }

        public static void Clear()
        {
            try { if (File.Exists(CacheFile)) File.Delete(CacheFile); } catch { }
            try { if (File.Exists(CacheUrlFile)) File.Delete(CacheUrlFile); } catch { }
        }

        public static async Task SaveFromUrlAsync(string url)
        {
            if (string.IsNullOrWhiteSpace(url))
            {
                Clear();
                return;
            }

            url = url.Trim();
            try
            {
                Directory.CreateDirectory(CacheDir);
                var previous = File.Exists(CacheUrlFile) ? File.ReadAllText(CacheUrlFile) : "";
                if (File.Exists(CacheFile)
                    && string.Equals(previous, url, StringComparison.OrdinalIgnoreCase))
                    return;

                using (var handler = new HttpClientHandler
                {
                    ServerCertificateCustomValidationCallback = (_, __, ___, ____) => true
                })
                using (var http = new HttpClient(handler))
                {
                    http.Timeout = TimeSpan.FromSeconds(8);
                    var bytes = await http.GetByteArrayAsync(url);
                    if (bytes == null || bytes.Length < 32)
                        return;
                    File.WriteAllBytes(CacheFile, bytes);
                    File.WriteAllText(CacheUrlFile, url);
                }
            }
            catch
            {
            }
        }
    }
}
