using System;
using System.Globalization;
using System.Text.RegularExpressions;

namespace Desing.Helpers
{
    /// <summary>Sanitización de <c>TSql_Branch.Attcolor</c> (HEX #RRGGBB) y estilos de badge.</summary>
    public static class BranchColorHelper
    {
        private static readonly Regex Hex6 = new Regex(@"^#[0-9A-Fa-f]{6}$", RegexOptions.Compiled);

        /// <summary>Null/blank → <c>null</c> (OK). Valor no vacío debe ser <c>#RRGGBB</c>.</summary>
        public static bool TryNormalizeAttcolor(string raw, out string normalized)
        {
            normalized = null;
            if (string.IsNullOrWhiteSpace(raw))
                return true;
            var t = raw.Trim();
            if (!Hex6.IsMatch(t))
                return false;
            normalized = t.ToLowerInvariant();
            return true;
        }

        /// <summary>Estilo inline para un badge con fondo <paramref name="attcolor"/> y texto con contraste.</summary>
        public static string BadgeInlineStyle(string attcolor)
        {
            if (!TryNormalizeAttcolor(attcolor, out var hex))
                return null;
            int r = int.Parse(hex.Substring(1, 2), NumberStyles.HexNumber);
            int g = int.Parse(hex.Substring(3, 2), NumberStyles.HexNumber);
            int b = int.Parse(hex.Substring(5, 2), NumberStyles.HexNumber);
            var lum = (0.299 * r + 0.587 * g + 0.114 * b) / 255.0;
            var fg = lum > 0.55 ? "#212529" : "#ffffff";
            return "background-color:" + hex + ";color:" + fg;
        }

        /// <summary>Valor válido para <c>type="color"</c> (#RRGGBB); gris medio si no hay color.</summary>
        public static string HexForColorInput(string attcolor)
        {
            if (TryNormalizeAttcolor(attcolor, out var hex))
                return hex;
            return "#808080";
        }
    }
}
