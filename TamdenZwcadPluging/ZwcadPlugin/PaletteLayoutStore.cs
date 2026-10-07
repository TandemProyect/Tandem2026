using System;
using System.IO;
using System.Windows;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace ZwcadPlugin
{
    /// <summary>
    /// Última posición de cada paleta / popup, en coordenadas de pantalla.
    /// </summary>
    internal static class PaletteLayoutStore
    {
        public static bool TryGet(string id, out double left, out double top)
        {
            left = 0;
            top = 0;
            if (string.IsNullOrWhiteSpace(id))
                return false;
            try
            {
                var root = Read();
                var node = root[id] as JObject;
                if (node == null)
                    return false;
                left = (double?)node["left"] ?? double.NaN;
                top = (double?)node["top"] ?? double.NaN;
                return !double.IsNaN(left) && !double.IsNaN(top) && IsOnScreen(left, top);
            }
            catch
            {
                return false;
            }
        }

        public static void Save(string id, double left, double top)
        {
            if (string.IsNullOrWhiteSpace(id) || double.IsNaN(left) || double.IsNaN(top))
                return;
            try
            {
                var root = Read();
                root[id] = new JObject
                {
                    ["left"] = Math.Round(left, 1),
                    ["top"] = Math.Round(top, 1)
                };
                var path = StorePath();
                Directory.CreateDirectory(Path.GetDirectoryName(path));
                File.WriteAllText(path, root.ToString(Formatting.Indented));
            }
            catch
            {
            }
        }

        public static bool IsOnScreen(double left, double top)
        {
            try
            {
                var area = SystemParameters.VirtualScreenLeft;
                var atop = SystemParameters.VirtualScreenTop;
                var aw = SystemParameters.VirtualScreenWidth;
                var ah = SystemParameters.VirtualScreenHeight;
                return left + 40 >= area
                    && top + 20 >= atop
                    && left < area + aw - 20
                    && top < atop + ah - 20;
            }
            catch
            {
                return true;
            }
        }

        private static JObject Read()
        {
            try
            {
                var path = StorePath();
                if (File.Exists(path))
                    return JObject.Parse(File.ReadAllText(path)) ?? new JObject();
            }
            catch
            {
            }
            return new JObject();
        }

        private static string StorePath()
        {
            return Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                Atk60LibrarySync.ProductFolder,
                "Content",
                "Data",
                "db",
                "palette-layout.json");
        }
    }
}

