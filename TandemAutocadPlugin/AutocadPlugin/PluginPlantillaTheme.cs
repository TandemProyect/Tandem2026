using System;
using System.Globalization;
using System.IO;
using System.Windows.Media;

namespace AutocadPlugin
{
    /// <summary>
    /// Color de fondo y de letra de la plantilla del usuario (intranet).
    /// Llega por postMessage desde las paletas MVC y se cachea en disco.
    /// </summary>
    internal static class PluginPlantillaTheme
    {
        public const string DefaultColor = "#349d7d";
        public const string DefaultTextColor = "#ffffff";

        private static string _color = DefaultColor;
        private static string _textColor = DefaultTextColor;
        private static string _logoUrl = "";
        private static bool _loaded;

        public static string LogoUrl
        {
            get
            {
                EnsureLoaded();
                return _logoUrl;
            }
        }

        public static string ColorHex
        {
            get
            {
                EnsureLoaded();
                return _color;
            }
        }

        public static string TextColorHex
        {
            get
            {
                EnsureLoaded();
                return _textColor;
            }
        }

        public static Brush Background
        {
            get { return new SolidColorBrush(ParseHex(ColorHex, 0x34, 0x9D, 0x7D)); }
        }

        public static Brush Foreground
        {
            get { return new SolidColorBrush(ParseHex(TextColorHex, 0xFF, 0xFF, 0xFF)); }
        }

        public static void Reset()
        {
            _color = DefaultColor;
            _textColor = DefaultTextColor;
            _logoUrl = "";
            _loaded = true;
            try
            {
                if (File.Exists(CacheFile))
                    File.Delete(CacheFile);
            }
            catch
            {
            }
        }

        public static void Apply(string color, string textColor, string logoUrl = null)
        {
            _color = NormalizeHex(color, DefaultColor);
            _textColor = NormalizeHex(textColor, DefaultTextColor);
            if (logoUrl != null)
                _logoUrl = (logoUrl ?? "").Trim();
            _loaded = true;
            try
            {
                Directory.CreateDirectory(CacheDir);
                File.WriteAllText(CacheFile, _color + "\n" + _textColor + "\n" + _logoUrl);
            }
            catch
            {
            }
        }

        private static void EnsureLoaded()
        {
            if (_loaded)
                return;
            _loaded = true;
            try
            {
                if (!File.Exists(CacheFile))
                    return;
                var lines = File.ReadAllLines(CacheFile);
                if (lines.Length > 0)
                    _color = NormalizeHex(lines[0], DefaultColor);
                if (lines.Length > 1)
                    _textColor = NormalizeHex(lines[1], DefaultTextColor);
                if (lines.Length > 2)
                    _logoUrl = (lines[2] ?? "").Trim();
            }
            catch
            {
            }
        }

        private static string CacheDir
        {
            get
            {
                return Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                    "Tandem",
                    "AutocadPlugin");
            }
        }

        private static string CacheFile
        {
            get { return Path.Combine(CacheDir, "plantilla-theme.txt"); }
        }

        private static string NormalizeHex(string raw, string fallback)
        {
            var t = (raw ?? "").Trim();
            if (t.Length == 0)
                return fallback;
            if (t[0] != '#')
                t = "#" + t;
            if (t.Length != 4 && t.Length != 7)
                return fallback;
            return t;
        }

        private static Color ParseHex(string hex, byte dr, byte dg, byte db)
        {
            try
            {
                var t = (hex ?? "").Trim();
                if (t.StartsWith("#"))
                    t = t.Substring(1);
                if (t.Length == 3)
                {
                    var r = byte.Parse(new string(t[0], 2), NumberStyles.HexNumber);
                    var g = byte.Parse(new string(t[1], 2), NumberStyles.HexNumber);
                    var b = byte.Parse(new string(t[2], 2), NumberStyles.HexNumber);
                    return Color.FromRgb(r, g, b);
                }
                if (t.Length == 6)
                {
                    var r = byte.Parse(t.Substring(0, 2), NumberStyles.HexNumber);
                    var g = byte.Parse(t.Substring(2, 2), NumberStyles.HexNumber);
                    var b = byte.Parse(t.Substring(4, 2), NumberStyles.HexNumber);
                    return Color.FromRgb(r, g, b);
                }
            }
            catch
            {
            }
            return Color.FromRgb(dr, dg, db);
        }
    }
}
