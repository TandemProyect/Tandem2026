using System;
using System.IO;
using System.Net.Http;

namespace AutocadPlugin
{
    internal static class Atk60DwgResolver
    {
        public static string ResolveDwg(string codeName, string view)
        {
            string folder;
            string file;
            NormalizeView(view, codeName, out folder, out file);
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
            folder = "3DRef";
            file = name + "R.dwg";
        }

        private static System.Collections.Generic.IEnumerable<string> CandidateDwgPaths(
            string folder, string file, string codeName, string view)
        {
            foreach (var root in LibraryRoots())
            {
                yield return Path.Combine(root, folder, file);
                if (string.Equals(folder, "3DRef", StringComparison.OrdinalIgnoreCase))
                    yield return Path.Combine(root, folder, (codeName ?? "").Trim() + ".dwg");
            }
            yield return Path.Combine(CacheDir(), folder, file);
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
    }
}
