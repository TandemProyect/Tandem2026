using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using Newtonsoft.Json.Linq;

namespace AutocadPlugin
{
    internal static class Atk60DwgResolver
    {
        private static readonly Dictionary<string, ArticleDwgUrls> Remembered =
            new Dictionary<string, ArticleDwgUrls>(StringComparer.OrdinalIgnoreCase);

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

            var cached = Path.Combine(CacheDir(), folder, file);
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
                && articleUrl.StartsWith(CacheDir(), StringComparison.OrdinalIgnoreCase))
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
            var cached = Path.Combine(CacheDir(), "Snaps", file);
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

        private static string CacheDir()
        {
            return Atk60LibrarySync.CacheDir();
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

        private static bool TryDownload(string url, string dest)
        {
            try
            {
                var folder = Path.GetDirectoryName(dest);
                if (!string.IsNullOrWhiteSpace(folder))
                    Directory.CreateDirectory(folder);
                using (var handler = new HttpClientHandler
                {
                    ServerCertificateCustomValidationCallback = (_, __, ___, ____) => true
                })
                using (var http = new HttpClient(handler) { Timeout = TimeSpan.FromSeconds(60) })
                {
                    var bytes = http.GetByteArrayAsync(url).GetAwaiter().GetResult();
                    if (bytes == null || bytes.Length < 64)
                        return false;
                    File.WriteAllBytes(dest, bytes);
                    return File.Exists(dest);
                }
            }
            catch
            {
                return false;
            }
        }

        private static string UrlFromRemembered(string codeName, string view)
        {
            ArticleDwgUrls set;
            if (!Remembered.TryGetValue((codeName ?? "").Trim(), out set) || set == null)
                return null;
            if (string.Equals(view, "3d", StringComparison.OrdinalIgnoreCase))
                return set.Url3D;
            if (string.Equals(view, "xr", StringComparison.OrdinalIgnoreCase))
                return set.UrlXr;
            return set.Url3DRef;
        }

        private static string TryLocalOrDownload(string url, string folder, string file)
        {
            url = (url ?? "").Trim();
            if (string.IsNullOrWhiteSpace(url))
                return null;
            if (File.Exists(url))
                return url;
            try
            {
                var abs = AbsoluteUrl(url);
                var destName = file;
                try
                {
                    var fromUrl = Path.GetFileName(new Uri(abs).AbsolutePath);
                    if (!string.IsNullOrWhiteSpace(fromUrl))
                        destName = fromUrl;
                }
                catch
                {
                }
                var dest = Path.Combine(CacheDir(), folder, destName);
                if (IsUsable(dest))
                    return dest;
                if (TryDownload(abs, dest))
                    return dest;
            }
            catch
            {
            }
            return null;
        }

        private static string AbsoluteUrl(string url)
        {
            var t = (url ?? "").Trim();
            if (string.IsNullOrWhiteSpace(t))
                return t;
            if (t.StartsWith("http", StringComparison.OrdinalIgnoreCase))
                return t;
            var root = MvcServerSettings.CurrentUrl().TrimEnd('/');
            return root + "/" + t.TrimStart('~', '/');
        }

        private sealed class ArticleDwgUrls
        {
            public string Url3D { get; set; }
            public string Url3DRef { get; set; }
            public string UrlXr { get; set; }
        }
    }
}
