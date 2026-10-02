using System;
using System.IO;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace AutocadPlugin
{
    /// <summary>
    /// Recuerda que este PC ya inició sesión CAD. Sirve para reanudar
    /// en otro dibujo o al volver a abrir AutoCAD sin pedir login otra vez.
    /// La cookie Identity sigue en el perfil WebView2.
    /// </summary>
    internal static class PluginSessionStore
    {
        public static bool HasRecent()
        {
            var data = Read();
            if (data == null)
                return false;
            if (!SameServer(data.Server, MvcServerSettings.CurrentUrl()))
                return false;
            return data.Utc.AddDays(14) >= DateTime.UtcNow;
        }

        public static void Remember()
        {
            try
            {
                var path = StorePath();
                Directory.CreateDirectory(Path.GetDirectoryName(path));
                var json = new JObject
                {
                    ["server"] = MvcServerSettings.CurrentUrl() ?? "",
                    ["utc"] = DateTime.UtcNow.ToString("o")
                };
                File.WriteAllText(path, json.ToString(Formatting.Indented));
            }
            catch
            {
            }
        }

        public static void Clear()
        {
            try
            {
                var path = StorePath();
                if (File.Exists(path))
                    File.Delete(path);
            }
            catch
            {
            }
        }

        private static Record Read()
        {
            try
            {
                var path = StorePath();
                if (!File.Exists(path))
                    return null;
                var root = JObject.Parse(File.ReadAllText(path));
                DateTime utc;
                if (!DateTime.TryParse((string)root["utc"], null,
                    System.Globalization.DateTimeStyles.RoundtripKind, out utc))
                    return null;
                return new Record
                {
                    Server = (string)root["server"] ?? "",
                    Utc = utc.ToUniversalTime()
                };
            }
            catch
            {
                return null;
            }
        }

        private static bool SameServer(string a, string b)
        {
            return string.Equals(Norm(a), Norm(b), StringComparison.OrdinalIgnoreCase);
        }

        private static string Norm(string url)
        {
            return (url ?? "").Trim().TrimEnd('/');
        }

        private static string StorePath()
        {
            return Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                Atk60LibrarySync.ProductFolder,
                "Content",
                "Data",
                "db",
                "cad-session.json");
        }

        private sealed class Record
        {
            public string Server;
            public DateTime Utc;
        }
    }
}
