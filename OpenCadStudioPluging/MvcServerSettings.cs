using System;
using System.IO;
using System.Text.Json;

namespace OpenCadStudioPluging
{
    internal static class MvcServerSettings
    {
        private const string LocalUrl = "https://localhost:44384/";
        private const string ProductionUrl = "https://tdesing.net/";

        public static string CurrentUrl()
        {
            var env = Environment.GetEnvironmentVariable("TANDEM_MVC_BASE_URL");
            if (!string.IsNullOrWhiteSpace(env))
                return NormalizeBaseUrl(env);

            var target = ReadTarget();
            if (string.Equals(target, "production", StringComparison.OrdinalIgnoreCase)
                || string.Equals(target, "produccion", StringComparison.OrdinalIgnoreCase)
                || string.Equals(target, "prod", StringComparison.OrdinalIgnoreCase))
                return ProductionUrl;

            if (!string.IsNullOrWhiteSpace(target) && target.StartsWith("http", StringComparison.OrdinalIgnoreCase))
                return NormalizeBaseUrl(target);

            return LocalUrl;
        }

        public static string CurrentLabel()
        {
            return IsProduction() ? "produccion (tdesing.net)" : "local (localhost:44384)";
        }

        public static bool IsProduction()
        {
            return CurrentUrl().IndexOf("tdesing.net", StringComparison.OrdinalIgnoreCase) >= 0;
        }

        public static void SetTarget(string target)
        {
            OpenCadStudioPluginEnvironment.EnsureFolders();
            var path = OpenCadStudioPluginEnvironment.MvcSettingsPath();
            var json = JsonSerializer.Serialize(new MvcTargetConfig { target = target ?? "local" });
            File.WriteAllText(path, json);
        }

        public static string NormalizeBaseUrl(string url)
        {
            if (string.IsNullOrWhiteSpace(url))
                return LocalUrl;

            var normalized = url.Trim();
            if (normalized.StartsWith("https://www.tdesing.net", StringComparison.OrdinalIgnoreCase))
                normalized = ProductionUrl.TrimEnd('/') + normalized.Substring("https://www.tdesing.net".Length);

            return normalized.EndsWith("/", StringComparison.Ordinal) ? normalized : normalized + "/";
        }

        private static string ReadTarget()
        {
            var path = OpenCadStudioPluginEnvironment.MvcSettingsPath();
            if (!File.Exists(path))
                return "local";

            try
            {
                var json = File.ReadAllText(path);
                using (var doc = JsonDocument.Parse(json))
                {
                    if (doc.RootElement.TryGetProperty("target", out var targetElement))
                        return targetElement.GetString() ?? "local";
                }
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine("No se pudo leer " + path + ". Se usara local. Motivo: " + ex.Message);
            }

            return "local";
        }

        private sealed class MvcTargetConfig
        {
            public string target { get; set; }
        }
    }
}
