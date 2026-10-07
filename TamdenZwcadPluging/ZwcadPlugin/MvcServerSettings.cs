using System;
using System.IO;

namespace ZwcadPlugin
{
    /// <summary>
    /// Servidor MVC: local (IIS Express) o producción (tdesing.net).
    /// La elección se guarda en AppData y sobrevive a NETLOAD de DebugN.
    /// TANDEM_MVC_BASE_URL, si está definida, gana sobre el archivo.
    /// </summary>
    public static class MvcServerSettings
    {
        public const string LocalUrl = "https://localhost:44384/";
        public const string ProductionUrl = "https://tdesing.net/";

        public static string CurrentUrl()
        {
            var env = Environment.GetEnvironmentVariable("TANDEM_MVC_BASE_URL");
            if (!string.IsNullOrWhiteSpace(env))
                return PluginExceptionHelper.NormalizeBaseUrl(env);

            var saved = ReadSaved();
            if (string.Equals(saved, "production", StringComparison.OrdinalIgnoreCase)
                || string.Equals(saved, "produccion", StringComparison.OrdinalIgnoreCase)
                || string.Equals(saved, "prod", StringComparison.OrdinalIgnoreCase))
                return ProductionUrl;

            if (saved.StartsWith("http", StringComparison.OrdinalIgnoreCase))
                return PluginExceptionHelper.NormalizeBaseUrl(saved);

            return LocalUrl;
        }

        public static bool IsProduction()
        {
            var url = CurrentUrl();
            return url.IndexOf("tdesing.net", StringComparison.OrdinalIgnoreCase) >= 0;
        }

        public static bool IsLocal()
        {
            try
            {
                var host = new Uri(CurrentUrl()).Host;
                return PluginHttp.IsLoopback(host);
            }
            catch
            {
                return true;
            }
        }

        public static string CurrentLabel()
        {
            return IsProduction() ? "producción (tdesing.net)" : "local (localhost:44384)";
        }

        public static void SetLocal()
        {
            WriteSaved("local");
        }

        public static void SetProduction()
        {
            WriteSaved("production");
        }

        private static string SettingsPath()
        {
            var dir = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                "Tandem",
                "AutocadPlugin");
            Directory.CreateDirectory(dir);
            return Path.Combine(dir, "mvc-target.txt");
        }

        private static string ReadSaved()
        {
            try
            {
                var path = SettingsPath();
                if (!File.Exists(path))
                    return "local";
                return (File.ReadAllText(path) ?? "").Trim();
            }
            catch
            {
                return "local";
            }
        }

        private static void WriteSaved(string value)
        {
            File.WriteAllText(SettingsPath(), value ?? "local");
        }
    }
}

