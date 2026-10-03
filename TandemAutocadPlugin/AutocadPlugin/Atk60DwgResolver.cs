using System;
using System.Collections.Concurrent;
using System.IO;

namespace AutocadPlugin
{
    internal static class Atk60DwgResolver
    {
        private static readonly ConcurrentDictionary<string, ArticleDwgUrls> Remembered =
            new ConcurrentDictionary<string, ArticleDwgUrls>(StringComparer.OrdinalIgnoreCase);

        public static void RememberArticleUrls(string codeName, string url3d, string url3dRef, string urlXr)
        {
            var key = (codeName ?? "").Trim();
            if (string.IsNullOrWhiteSpace(key))
                return;
            Remembered[key] = new ArticleDwgUrls
            {
                Url3D = url3d,
                Url3DRef = url3dRef,
                UrlXr = urlXr
            };
        }

        public static string ResolveDwg(string codeName, string view, string articleUrl = null)
        {
            string folder;
            string file;
            NormalizeView(view, codeName, out folder, out file);

            var cached = Path.Combine(BlockRoot(), folder, file);
            if (IsUsable(cached))
                return cached;

            string fromIndex;
            if (Atk60LibrarySync.TryFindLocal(codeName, view, out fromIndex) && IsUsable(fromIndex))
                return fromIndex;

            if (Atk60LibrarySync.EnsureCached(codeName, view, file) && IsUsable(cached))
                return cached;

            if (!string.IsNullOrWhiteSpace(articleUrl)
                && File.Exists(articleUrl)
                && IsUsable(articleUrl)
                && articleUrl.StartsWith(BlockRoot(), StringComparison.OrdinalIgnoreCase))
                return articleUrl;

            return null;
        }

        public static string FindSnapJson(string codeName)
        {
            if (string.IsNullOrWhiteSpace(codeName))
                return null;
            Atk60LibrarySync.EnsureStarted();
            string fromIndex;
            if (Atk60LibrarySync.TryFindLocal(codeName, "snap", out fromIndex) && IsUsable(fromIndex))
                return fromIndex;
            var file = codeName.Trim() + ".json";
            var cached = Path.Combine(BlockRoot(), "Snaps", file);
            if (IsUsable(cached))
                return cached;
            if (Atk60LibrarySync.EnsureCached(codeName, "snap") && IsUsable(cached))
                return cached;
            return null;
        }

        private static void NormalizeView(string view, string codeName, out string folder, out string file)
        {
            var name = (codeName ?? "").Trim();
            if (string.Equals(view, "3d", StringComparison.OrdinalIgnoreCase))
            {
                folder = "3D";
                file = name + ".dwg";
                return;
            }
            if (string.Equals(view, "xr", StringComparison.OrdinalIgnoreCase))
            {
                folder = "Xr";
                file = name + "X.dwg";
                return;
            }
            folder = "3DRef";
            file = name + "R.dwg";
        }

        private static string BlockRoot()
        {
            return Atk60LibrarySync.BlockRoot();
        }

        private static bool IsUsable(string path)
        {
            try
            {
                return !string.IsNullOrWhiteSpace(path)
                    && File.Exists(path)
                    && new FileInfo(path).Length >= 64;
            }
            catch
            {
                return false;
            }
        }

        private sealed class ArticleDwgUrls
        {
            public string Url3D { get; set; }
            public string Url3DRef { get; set; }
            public string UrlXr { get; set; }
        }
    }
}
