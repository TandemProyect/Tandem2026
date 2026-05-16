using System;
using System.IO;
using System.Web;

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
            return VirtualPathUtility.ToAbsolute(ClientV2Folder + fileName).Replace('\\', '/');
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
            return VirtualPathUtility.ToAbsolute(ExtensionIcoFolder + fileName).Replace('\\', '/');
        }
    }
}
