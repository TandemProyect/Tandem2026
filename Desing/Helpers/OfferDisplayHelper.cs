using System;
using System.Globalization;
using System.Text.RegularExpressions;
using System.Web;

namespace Desing.Helpers
{
    /// <summary>
    /// Celda HTML (oferta) con color de estado según <c>TSql_OfferState.AddColor</c> (HEX #RGB / #RRGGBB)
    /// y contraste de texto legible (mismo criterio que <see cref="BranchColorHelper"/>).
    /// </summary>
    public static class OfferDisplayHelper
    {
        private static readonly Regex HexColorRegex = new Regex(
            "^#(?:[0-9a-fA-F]{3}|[0-9a-fA-F]{6})$",
            RegexOptions.Compiled);

        /// <summary>
        /// Número de oferta: <c>span</c> con fondo de estado si el color es válido; si no, texto escapado.
        /// </summary>
        public static string BuildOfferNumberCellHtml(string numberPlain, string addColorRaw)
        {
            return BuildOfferStateSwatchCellHtml(numberPlain ?? "", addColorRaw, "tandem-offer-number-swatch");
        }

        private static string BuildOfferStateSwatchCellHtml(string plain, string addColorRaw, string swatchCssClass)
        {
            var encoded = HttpUtility.HtmlEncode(plain);

            if (string.IsNullOrWhiteSpace(addColorRaw))
            {
                return encoded;
            }

            if (!TryNormalizeOfferStateHex(addColorRaw.Trim(), out var hex6))
            {
                return encoded;
            }

            int r = int.Parse(hex6.Substring(1, 2), NumberStyles.HexNumber);
            int g = int.Parse(hex6.Substring(3, 2), NumberStyles.HexNumber);
            int b = int.Parse(hex6.Substring(5, 2), NumberStyles.HexNumber);
            var lum = (0.299 * r + 0.587 * g + 0.114 * b) / 255.0;
            var fg = lum > 0.55 ? "#212529" : "#ffffff";

            var style = "background-color:" + hex6 + ";color:" + fg;
            return "<span class=\"rounded px-2 py-1 d-inline-block " + swatchCssClass + "\" style=\"" +
                   HttpUtility.HtmlAttributeEncode(style) + "\">" + encoded + "</span>";
        }

        /// <summary>
        /// Normaliza a <c>#rrggbb</c> minúsculas o devuelve <c>false</c>.
        /// </summary>
        public static bool TryNormalizeOfferStateHex(string trimmed, out string hex6Lower)
        {
            hex6Lower = null;
            if (string.IsNullOrEmpty(trimmed) || !HexColorRegex.IsMatch(trimmed))
            {
                return false;
            }

            if (trimmed.Length == 7)
            {
                hex6Lower = "#" + trimmed.Substring(1).ToLowerInvariant();
                return true;
            }

            if (trimmed.Length == 4)
            {
                var a = char.ToLowerInvariant(trimmed[1]);
                var b = char.ToLowerInvariant(trimmed[2]);
                var c = char.ToLowerInvariant(trimmed[3]);
                hex6Lower = string.Format(CultureInfo.InvariantCulture, "#{0}{0}{1}{1}{2}{2}", a, b, c);
                return true;
            }

            return false;
        }
    }
}
