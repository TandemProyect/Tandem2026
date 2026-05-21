using System;
using System.IO;
using System.Web;
using System.Web.Mvc;

namespace Desing.Helpers
{
    public static class IntranetFileHelper
    {
        private const string ClientV2Folder = "~/Files/Intranet/ClientV2/";
        private const string ExtensionIcoFolder = "~/Files/Intranet/ExtensionIco/";

        public static string TrySaveClientV2File(HttpPostedFileBase file, string prefix, out string error)
        {
            error = null;
            if (file == null || file.ContentLength == 0)
            {
                return null;
            }

            var ext = Path.GetExtension(file.FileName);
            if (string.IsNullOrEmpty(ext))
            {
                error = "El archivo debe tener extensión.";
                return null;
            }

            ext = ext.ToLowerInvariant();
            if (ext != ".png" && ext != ".jpg" && ext != ".jpeg" && ext != ".gif" && ext != ".ico" && ext != ".webp")
            {
                error = "Formato no permitido (png, jpg, gif, ico, webp).";
                return null;
            }

            var physicalDir = HttpContext.Current.Server.MapPath(ClientV2Folder);
            if (!Directory.Exists(physicalDir))
            {
                Directory.CreateDirectory(physicalDir);
            }

            var fileName = prefix + "_" + DateTime.UtcNow.ToString("yyyyMMdd_HHmmss") + "_" + Guid.NewGuid().ToString("N").Substring(0, 8) + ext;
            var physicalPath = Path.Combine(physicalDir, fileName);
            file.SaveAs(physicalPath);
            // Guardar siempre como ~/... para Url.Content (ToAbsolute produce /Files/... y rompe en algunos casos).
            return (ClientV2Folder + fileName).Replace('\\', '/');
        }

        /// <summary>
        /// Convierte rutas de ficheros servidos por la app (~/, /..., relativas) a virtual ~/... para <see cref="UrlHelper.Content"/>.
        /// Compatibilidad: registros antiguos guardados con <c>VirtualPathUtility.ToAbsolute</c> (/Files/...),
        /// rutas físicas Windows bajo el directorio de la aplicación y URLs absolutas (http(s), //...).
        /// </summary>
        public static string NormalizeUploadedWebPath(string stored)
        {
            if (string.IsNullOrWhiteSpace(stored))
            {
                return null;
            }

            var trimmed = stored.Trim();
            if (LooksLikeWindowsFileSystemPath(trimmed))
            {
                return TryMapPhysicalUnderAppToVirtual(trimmed);
            }

            var p = trimmed.Replace('\\', '/');
            if (p.StartsWith("~", StringComparison.Ordinal) && !p.StartsWith("~/", StringComparison.Ordinal))
            {
                p = "~/" + p.TrimStart('~').TrimStart('/');
            }

            if (p.StartsWith("~/", StringComparison.Ordinal))
            {
                return p;
            }

            if (p.StartsWith("http://", StringComparison.OrdinalIgnoreCase) ||
                p.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
            {
                return p;
            }

            if (p.Length >= 2 && p[0] == '/' && p[1] == '/')
            {
                return p;
            }

            try
            {
                if (!p.StartsWith("/", StringComparison.Ordinal))
                {
                    p = "/" + p.TrimStart('/');
                }

                return VirtualPathUtility.ToAppRelative(p);
            }
            catch
            {
                return p.StartsWith("/", StringComparison.Ordinal) ? ("~" + p) : ("~/" + p.TrimStart('/'));
            }
        }

        /// <summary>
        /// Normaliza a URL lista para el navegador: no aplica <see cref="UrlHelper.Content"/> a http(s) ni a //.
        /// </summary>
        public static string ResolvePublicUrl(UrlHelper url, string normalizedVp)
        {
            if (url == null)
            {
                throw new ArgumentNullException(nameof(url));
            }

            if (string.IsNullOrWhiteSpace(normalizedVp))
            {
                return null;
            }

            var p = normalizedVp.Trim();
            if (p.Length >= 2 && p[0] == '/' && p[1] == '/')
            {
                return p;
            }

            if (p.StartsWith("http://", StringComparison.OrdinalIgnoreCase) ||
                p.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
            {
                return p;
            }

            return url.Content(p);
        }

        private static bool LooksLikeWindowsFileSystemPath(string t)
        {
            if (string.IsNullOrEmpty(t))
            {
                return false;
            }

            t = t.Trim();
            if (t.Length >= 3 && char.IsLetter(t[0]) && t[1] == ':' && (t[2] == '\\' || t[2] == '/'))
            {
                return true;
            }

            return t.Length >= 2 && t[0] == '\\' && t[1] == '\\';
        }

        private static string TryMapPhysicalUnderAppToVirtual(string physicalPath)
        {
            if (HttpContext.Current == null)
            {
                return null;
            }

            try
            {
                var normalizedFs = physicalPath.Replace('/', Path.DirectorySeparatorChar);
                var full = Path.GetFullPath(normalizedFs);
                var appRoot = HttpContext.Current.Server.MapPath("~/");
                if (string.IsNullOrEmpty(appRoot))
                {
                    return null;
                }

                var appFull = Path.GetFullPath(appRoot);
                if (!appFull.EndsWith(Path.DirectorySeparatorChar.ToString(), StringComparison.Ordinal))
                {
                    appFull += Path.DirectorySeparatorChar;
                }

                if (full.StartsWith(appFull, StringComparison.OrdinalIgnoreCase))
                {
                    var rel = full.Substring(appFull.Length).Replace('\\', '/').TrimStart('/');
                    return string.IsNullOrEmpty(rel) ? "~/" : ("~/" + rel);
                }
            }
            catch
            {
                // ignorado — no es una ruta física válida bajo la app
            }

            return null;
        }

        public static string TrySaveExtensionIcoFile(HttpPostedFileBase file, string fileNamePrefix, out string error)
        {
            error = null;
            if (file == null || file.ContentLength == 0)
            {
                return null;
            }

            var ext = Path.GetExtension(file.FileName);
            if (string.IsNullOrEmpty(ext))
            {
                error = "El archivo debe tener extensión.";
                return null;
            }

            ext = ext.ToLowerInvariant();
            if (ext != ".png" && ext != ".jpg" && ext != ".jpeg" && ext != ".gif" && ext != ".ico" && ext != ".webp")
            {
                error = "Formato no permitido (png, jpg, gif, ico, webp).";
                return null;
            }

            var prefix = string.IsNullOrWhiteSpace(fileNamePrefix) ? "ico" : fileNamePrefix.Trim();

            var physicalDir = HttpContext.Current.Server.MapPath(ExtensionIcoFolder);
            if (!Directory.Exists(physicalDir))
            {
                Directory.CreateDirectory(physicalDir);
            }

            var fileName =
                prefix + "_" + DateTime.UtcNow.ToString("yyyyMMdd_HHmmss") + "_" +
                Guid.NewGuid().ToString("N").Substring(0, 8) + ext;
            var physicalPath = Path.Combine(physicalDir, fileName);
            file.SaveAs(physicalPath);
            return (ExtensionIcoFolder + fileName).Replace('\\', '/');
        }
    }
}
