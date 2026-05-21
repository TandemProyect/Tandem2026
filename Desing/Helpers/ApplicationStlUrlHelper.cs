using System;
using System.IO;
using System.Linq;

namespace Desing.Helpers
{
    /// <summary>
    /// Normaliza rutas de fichero STL almacenadas en BD (p. ej. <see cref="DAL.TSql_Design_V2.AttThumbnail"/>)
    /// a una ruta virtual tipo <c>~/...</c> segura para pasarla como <c>stlUrl</c> al visor: solo prefijos de aplicación
    /// bajo <c>~/Files/</c> o <c>~/Content/DesignTools/</c>, sin traversal, y extensión <c>.stl</c>.
    /// </summary>
    public static class ApplicationStlUrlHelper
    {
        private static readonly string[] TrustedLowerPrefixes =
        {
            "~/files/",
            "~/content/designtools/",
        };

        /// <summary>
        /// Devuelve una ruta <c>~/...</c> con barras normales, o null si no es usable como STL de confianza.
        /// </summary>
        public static string TryGetTrustedStlVirtualPath(string pathFromDatabase)
        {
            if (string.IsNullOrWhiteSpace(pathFromDatabase))
            {
                return null;
            }

            var raw = pathFromDatabase.Trim().Replace('\\', '/');
            if (raw.IndexOf("..", StringComparison.Ordinal) >= 0)
            {
                return null;
            }

            if (raw.StartsWith("http://", StringComparison.OrdinalIgnoreCase)
                || raw.StartsWith("https://", StringComparison.OrdinalIgnoreCase)
                || raw.StartsWith("//", StringComparison.Ordinal))
            {
                return null;
            }

            string virt;
            if (raw.StartsWith("~/", StringComparison.Ordinal))
            {
                virt = raw;
            }
            else if (raw.StartsWith("/"))
            {
                virt = "~" + raw;
            }
            else
            {
                virt = "~/" + raw.TrimStart('/');
            }

            if (virt.Length < 3)
            {
                return null;
            }

            var lower = virt.ToLowerInvariant();
            if (!TrustedLowerPrefixes.Any(p => lower.StartsWith(p, StringComparison.Ordinal)))
            {
                return null;
            }

            var ext = Path.GetExtension(virt);
            if (string.IsNullOrEmpty(ext) || !ext.Equals(".stl", StringComparison.OrdinalIgnoreCase))
            {
                return null;
            }

            return virt;
        }
    }
}
