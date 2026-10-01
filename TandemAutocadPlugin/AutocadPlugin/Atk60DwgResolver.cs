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

            var fromArticle = TryLocalOrDownload(articleUrl, folder, file);
            if (!string.IsNullOrWhiteSpace(fromArticle))
                return fromArticle;

            var remembered = UrlFromRemembered(codeName, view);
            fromArticle = TryLocalOrDownload(remembered, folder, file);
            if (!string.IsNullOrWhiteSpace(fromArticle))
                return fromArticle;

            var fromDb = FetchArticleUrl(codeName, view);
            fromArticle = TryLocalOrDownload(fromDb, folder, file);
            if (!string.IsNullOrWhiteSpace(fromArticle))
                return fromArticle;

            foreach (var path in CandidateDwgPaths(folder, file, codeName, view))
            {
                if (File.Exists(path))
                    return path;
            }

            var url = MvcServerSettings.CurrentUrl()
                + "Content/DesignTools/DWG/AtkSystem60/" + folder + "/" + file;
            var cached = Path.Combine(CacheDir(), folder, file);
            if (TryDownload(url, cached))
                return cached;
            var name = (codeName ?? "").Trim();
            if (string.Equals(folder, "3DRef", StringComparison.OrdinalIgnoreCase))
            {
                var dxfFile = name + "R.dxf";
                var dxfUrl = MvcServerSettings.CurrentUrl()
                    + "Content/DesignTools/DWG/AtkSystem60/3DRef/" + dxfFile;
                var dxfCached = Path.Combine(CacheDir(), "3DRef", dxfFile);
                if (TryDownload(dxfUrl, dxfCached))
                    return dxfCached;
            }
            if (string.Equals(folder, "3D", StringComparison.OrdinalIgnoreCase))
            {
                var dxfFile = name + ".dxf";
                var dxfUrl = MvcServerSettings.CurrentUrl()
                    + "Content/DesignTools/DWG/AtkSystem60/3D/" + dxfFile;
                var dxfCached = Path.Combine(CacheDir(), "3D", dxfFile);
                if (TryDownload(dxfUrl, dxfCached))
                    return dxfCached;
            }
            if (string.Equals(folder, "Xr", StringComparison.OrdinalIgnoreCase))
            {
                var dxfFile = name + "X.dxf";
                var dxfUrl = MvcServerSettings.CurrentUrl()
                    + "Content/DesignTools/DWG/AtkSystem60/Xr/" + dxfFile;
                var dxfCached = Path.Combine(CacheDir(), "Xr", dxfFile);
                if (TryDownload(dxfUrl, dxfCached))
                    return dxfCached;
            }
            return null;
        }

        public static string FindSnapJson(string codeName)
        {
            if (string.IsNullOrWhiteSpace(codeName))
                return null;
            var file = codeName.Trim() + ".json";
            foreach (var root in LibraryRoots())
            {
                var path = Path.Combine(root, "Snaps", file);
                if (File.Exists(path))
                    return path;
            }
            var cached = Path.Combine(CacheDir(), "Snaps", file);
            if (File.Exists(cached))
                return cached;
            var url = MvcServerSettings.CurrentUrl()
                + "Content/DesignTools/DWG/AtkSystem60/Snaps/" + file;
            if (TryDownload(url, cached))
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

        private static System.Collections.Generic.IEnumerable<string> CandidateDwgPaths(
            string folder, string file, string codeName, string view)
        {
            var name = (codeName ?? "").Trim();
            foreach (var root in LibraryRoots())
            {
                yield return Path.Combine(root, folder, file);
                if (string.Equals(folder, "3DRef", StringComparison.OrdinalIgnoreCase))
                {
                    yield return Path.Combine(root, folder, name + ".dwg");
                    yield return Path.Combine(root, folder, name + "R.dxf");
                    yield return Path.Combine(root, folder, name + ".dxf");
                }
                if (string.Equals(folder, "3D", StringComparison.OrdinalIgnoreCase))
                    yield return Path.Combine(root, folder, name + ".dxf");
                if (string.Equals(folder, "Xr", StringComparison.OrdinalIgnoreCase))
                {
                    yield return Path.Combine(root, folder, name + ".dwg");
                    yield return Path.Combine(root, folder, name + "X.dxf");
                    yield return Path.Combine(root, folder, name + ".dxf");
                }
            }
            yield return Path.Combine(CacheDir(), folder, file);
            if (string.Equals(folder, "3DRef", StringComparison.OrdinalIgnoreCase))
                yield return Path.Combine(CacheDir(), folder, name + "R.dxf");
            if (string.Equals(folder, "3D", StringComparison.OrdinalIgnoreCase))
                yield return Path.Combine(CacheDir(), folder, name + ".dxf");
            if (string.Equals(folder, "Xr", StringComparison.OrdinalIgnoreCase))
                yield return Path.Combine(CacheDir(), folder, name + "X.dxf");
        }

        private static System.Collections.Generic.IEnumerable<string> LibraryRoots()
        {
            var env = Environment.GetEnvironmentVariable("TANDEM_ATK60_DWG");
            if (!string.IsNullOrWhiteSpace(env))
                yield return env.Trim();

            yield return @"C:\00_Tandem2026\Desing\Content\DesignTools\DWG\AtkSystem60";

            var asm = Path.GetDirectoryName(typeof(Atk60DwgResolver).Assembly.Location);
            if (!string.IsNullOrWhiteSpace(asm))
            {
                var walk = new DirectoryInfo(asm);
                for (var i = 0; i < 8 && walk != null; i++, walk = walk.Parent)
                {
                    var candidate = Path.Combine(walk.FullName, "Desing", "Content", "DesignTools", "DWG", "AtkSystem60");
                    if (Directory.Exists(candidate))
                        yield return candidate;
                }
            }
        }

        private static string CacheDir()
        {
            var dir = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                "Tandem",
                "AutocadPlugin",
                "dwg-cache");
            Directory.CreateDirectory(dir);
            return dir;
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

        private static string FetchArticleUrl(string codeName, string view)
        {
            try
            {
                var q = "DesignToolsAutocad/PluginBlockDwg?codeName="
                    + Uri.EscapeDataString(codeName ?? "")
                    + "&view=" + Uri.EscapeDataString(view ?? "3dref");
                var abs = AbsoluteUrl(q);
                using (var handler = new HttpClientHandler
                {
                    ServerCertificateCustomValidationCallback = (_, __, ___, ____) => true
                })
                using (var http = new HttpClient(handler) { Timeout = TimeSpan.FromSeconds(20) })
                {
                    var json = http.GetStringAsync(abs).GetAwaiter().GetResult();
                    if (string.IsNullOrWhiteSpace(json))
                        return null;
                    var obj = JObject.Parse(json);
                    var url = ((string)obj["url"] ?? (string)obj["Url"] ?? "").Trim();
                    return string.IsNullOrWhiteSpace(url) ? null : url;
                }
            }
            catch
            {
                return null;
            }
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
                if (TryDownload(abs, dest))
                    return dest;
                if (File.Exists(dest))
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
