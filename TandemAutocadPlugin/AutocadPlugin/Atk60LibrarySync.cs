using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Autodesk.AutoCAD.ApplicationServices;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using AcadApp = Autodesk.AutoCAD.ApplicationServices.Application;

namespace AutocadPlugin
{
    /// <summary>
    /// Caché local del bloquing (escribible, sin admin):
    /// %LocalAppData%\AtDesing\Content\Data\Block\{3D,3DRef,Xr,Snaps}
    /// %LocalAppData%\AtDesing\Content\Data\db\bloquing.json
    /// </summary>
    internal static class Atk60LibrarySync
    {
        public const string IndexFileName = "bloquing.json";
        public const string ProductFolder = "AtDesing";
        private const int MaxParallel = 4;

        private static readonly object Gate = new object();
        private static readonly ConcurrentDictionary<string, string> LocalByKey =
            new ConcurrentDictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        private static readonly ConcurrentDictionary<string, string> UrlByKey =
            new ConcurrentDictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        private static readonly ConcurrentDictionary<string, Task<bool>> Inflight =
            new ConcurrentDictionary<string, Task<bool>>(StringComparer.OrdinalIgnoreCase);

        private static HttpClient _http;
        private static Task _running;
        private static JArray _pendingCatalog;
        public static Action AfterSync;

        public static string ProductRoot()
        {
            return Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                ProductFolder);
        }

        public static string CacheDir()
        {
            var dir = Path.Combine(ProductRoot(), "Content", "Data", "Block");
            Directory.CreateDirectory(dir);
            Directory.CreateDirectory(Path.Combine(dir, "3D"));
            Directory.CreateDirectory(Path.Combine(dir, "3DRef"));
            Directory.CreateDirectory(Path.Combine(dir, "Xr"));
            Directory.CreateDirectory(Path.Combine(dir, "Snaps"));
            return dir;
        }

        public static string IndexPath()
        {
            var db = Path.Combine(ProductRoot(), "Content", "Data", "db");
            Directory.CreateDirectory(db);
            return Path.Combine(db, IndexFileName);
        }

        public static void EnsureFolders()
        {
            CacheDir();
            IndexPath();
            LoadIndexFromDisk();
        }

        public static void StartInBackground()
        {
            lock (Gate)
            {
                if (_running != null && !_running.IsCompleted)
                {
                    CadLine("[Tandem] Biblioteca: actualización ya en curso.");
                    return;
                }
                CadLine("[Tandem] Biblioteca: comprobando actualizaciones…");
                _running = Task.Run(() =>
                {
                    try
                    {
                        SyncNow();
                        var done = AfterSync;
                        if (done != null)
                            done();
                    }
                    catch (Exception ex) { CadLine("[Tandem] Biblioteca: " + ex.Message); }
                });
            }
        }

        public static void EnsureStarted()
        {
            EnsureFolders();
        }

        public static void ClearMemory()
        {
            LocalByKey.Clear();
            UrlByKey.Clear();
            Inflight.Clear();
            _pendingCatalog = null;
        }

        public static string ReadCatalogJson()
        {
            var arr = ReadCatalogFromDisk();
            return arr != null ? arr.ToString(Formatting.None) : "[]";
        }

        private static JArray ReadCatalogFromDisk()
        {
            var root = ReadIndexFile();
            return root?["catalog"] as JArray ?? root?["Catalog"] as JArray;
        }

        public static bool TryFindLocal(string codeName, string view, out string path)
        {
            path = null;
            var key = Key(codeName, view);
            if (string.IsNullOrWhiteSpace(key))
                return false;
            LoadIndexFromDisk();
            string stored;
            if (!LocalByKey.TryGetValue(key, out stored) || string.IsNullOrWhiteSpace(stored))
                return false;
            if (!IsUsable(stored))
                return false;
            path = stored;
            return true;
        }

        public static bool WaitForFile(string codeName, string view, TimeSpan timeout)
        {
            var key = Key(codeName, view);
            if (string.IsNullOrWhiteSpace(key))
                return false;
            var deadline = DateTime.UtcNow + timeout;
            string path;
            while (DateTime.UtcNow < deadline)
            {
                if (TryFindLocal(codeName, view, out path))
                    return true;
                Task<bool> job;
                if (Inflight.TryGetValue(key, out job))
                {
                    try { job.Wait(deadline - DateTime.UtcNow); } catch { }
                    return TryFindLocal(codeName, view, out path);
                }
                var running = _running;
                if (running == null || running.IsCompleted)
                    return false;
                Thread.Sleep(120);
            }
            return TryFindLocal(codeName, view, out path);
        }

        public static bool TryDownloadOne(string codeName, string view)
        {
            var key = Key(codeName, view);
            string url;
            if (!UrlByKey.TryGetValue(key, out url) || string.IsNullOrWhiteSpace(url))
                return false;
            string folder;
            string file;
            Normalize(view, codeName, out folder, out file);
            var dest = Path.Combine(CacheDir(), folder, file);
            if (!DownloadTo(url, dest))
                return false;
            LocalByKey[key] = dest;
            return true;
        }

        private static void SyncNow()
        {
            var server = (MvcServerSettings.CurrentUrl() ?? "").Trim();
            LoadIndexFromDisk();
            var remote = FetchRemote();
            if (remote == null || remote.Count == 0)
            {
                CadLine("[Tandem] Biblioteca: no hay manifiesto en el servidor. Se usa " + IndexFileName + " local.");
                return;
            }

            var previous = ReadIndexFile();
            var prevVersions = ReadVersions(previous);
            var pending = new List<RemoteFile>();
            foreach (var item in remote)
            {
                var key = Key(item.Code, item.View);
                UrlByKey[key] = item.Url;
                var dest = Path.Combine(CacheDir(), item.Folder, item.File);
                string oldVersion;
                var sameVersion = prevVersions.TryGetValue(key, out oldVersion)
                    && string.Equals(oldVersion, item.Version, StringComparison.Ordinal);
                if (sameVersion && IsUsable(dest))
                {
                    LocalByKey[key] = dest;
                    continue;
                }
                pending.Add(item);
            }

            if (pending.Count == 0)
            {
                WriteIndex(server, remote);
                CadLine("[Tandem] Biblioteca al día (" + remote.Count + " en " + IndexFileName + ").");
                return;
            }

            CadLine("[Tandem] Biblioteca: descargando " + pending.Count + " archivo(s)…");
            var gate = new SemaphoreSlim(MaxParallel);
            var jobs = new List<Task>();
            var ok = 0;
            foreach (var item in pending)
            {
                var local = item;
                var key = Key(local.Code, local.View);
                var dest = Path.Combine(CacheDir(), local.Folder, local.File);
                var job = Task.Run(() =>
                {
                    gate.Wait();
                    try
                    {
                        if (DownloadTo(local.Url, dest))
                        {
                            LocalByKey[key] = dest;
                            Interlocked.Increment(ref ok);
                        }
                    }
                    finally { gate.Release(); }
                });
                Inflight[key] = job.ContinueWith(t => LocalByKey.ContainsKey(key));
                jobs.Add(job);
            }
            Task.WaitAll(jobs.ToArray());
            foreach (var item in pending)
                Inflight.TryRemove(Key(item.Code, item.View), out _);

            WriteIndex(server, remote);
            CadLine("[Tandem] Biblioteca: " + ok + "/" + pending.Count + " nuevos. Índice " + IndexFileName + ".");
        }

        private static List<RemoteFile> FetchRemote()
        {
            try
            {
                var abs = AbsoluteUrl("DesignToolsAutocad/PluginBlockLibrary");
                var json = Http().GetStringAsync(abs).GetAwaiter().GetResult();
                if (string.IsNullOrWhiteSpace(json))
                    return null;
                var root = JObject.Parse(json);
                _pendingCatalog = root["Catalog"] as JArray ?? root["catalog"] as JArray;
                var arr = root["Files"] as JArray ?? root["files"] as JArray;
                if (arr == null)
                    return null;
                var list = new List<RemoteFile>();
                foreach (var token in arr)
                {
                    var file = new RemoteFile
                    {
                        Code = Read(token, "Code", "code"),
                        View = Read(token, "View", "view"),
                        Folder = Read(token, "Folder", "folder"),
                        File = Read(token, "File", "file"),
                        Url = Read(token, "Url", "url"),
                        Version = Read(token, "Version", "version")
                    };
                    if (string.IsNullOrWhiteSpace(file.Code) || string.IsNullOrWhiteSpace(file.View))
                        continue;
                    if (string.IsNullOrWhiteSpace(file.Folder) || string.IsNullOrWhiteSpace(file.File))
                        continue;
                    if (string.IsNullOrWhiteSpace(file.Url))
                        continue;
                    list.Add(file);
                }
                return list;
            }
            catch
            {
                return null;
            }
        }

        private static void WriteIndex(string server, List<RemoteFile> remote)
        {
            var byCode = new SortedDictionary<string, JObject>(StringComparer.OrdinalIgnoreCase);
            foreach (var item in remote)
            {
                JObject block;
                if (!byCode.TryGetValue(item.Code, out block))
                {
                    block = new JObject { ["code"] = item.Code, ["files"] = new JObject() };
                    byCode[item.Code] = block;
                }
                var files = (JObject)block["files"];
                var rel = item.Folder.TrimEnd('\\', '/') + "/" + item.File;
                var dest = Path.Combine(CacheDir(), item.Folder, item.File);
                files[item.View.ToLowerInvariant()] = new JObject
                {
                    ["path"] = rel.Replace('\\', '/'),
                    ["version"] = item.Version ?? "",
                    ["ready"] = IsUsable(dest)
                };
                if (IsUsable(dest))
                    LocalByKey[Key(item.Code, item.View)] = dest;
                UrlByKey[Key(item.Code, item.View)] = item.Url;
            }

            var root = new JObject
            {
                ["updatedUtc"] = DateTime.UtcNow.ToString("o"),
                ["server"] = server ?? "",
                ["blocks"] = new JArray(byCode.Values),
                ["catalog"] = _pendingCatalog ?? ReadCatalogFromDisk() ?? new JArray()
            };
            var path = IndexPath();
            var tmp = path + ".tmp";
            File.WriteAllText(tmp, root.ToString(Formatting.Indented));
            if (File.Exists(path))
                File.Replace(tmp, path, null);
            else
                File.Move(tmp, path);
        }

        private static void LoadIndexFromDisk()
        {
            var path = IndexPath();
            if (!File.Exists(path))
                return;
            JObject root;
            try { root = JObject.Parse(File.ReadAllText(path)); }
            catch { return; }
            var blocks = root["blocks"] as JArray;
            if (blocks == null)
                return;
            foreach (var block in blocks)
            {
                var code = ((string)block["code"] ?? "").Trim();
                var files = block["files"] as JObject;
                if (string.IsNullOrWhiteSpace(code) || files == null)
                    continue;
                foreach (var prop in files.Properties())
                {
                    var rel = ((string)prop.Value["path"] ?? "").Trim();
                    if (string.IsNullOrWhiteSpace(rel))
                        continue;
                    var dest = Path.Combine(CacheDir(), rel.Replace('/', Path.DirectorySeparatorChar));
                    if (IsUsable(dest))
                        LocalByKey[Key(code, prop.Name)] = dest;
                }
            }
        }

        private static JObject ReadIndexFile()
        {
            var path = IndexPath();
            if (!File.Exists(path))
                return null;
            try { return JObject.Parse(File.ReadAllText(path)); }
            catch { return null; }
        }

        private static Dictionary<string, string> ReadVersions(JObject root)
        {
            var map = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            var blocks = root?["blocks"] as JArray;
            if (blocks == null)
                return map;
            foreach (var block in blocks)
            {
                var code = ((string)block["code"] ?? "").Trim();
                var files = block["files"] as JObject;
                if (string.IsNullOrWhiteSpace(code) || files == null)
                    continue;
                foreach (var prop in files.Properties())
                    map[Key(code, prop.Name)] = ((string)prop.Value["version"] ?? "").Trim();
            }
            return map;
        }

        private static bool DownloadTo(string url, string dest)
        {
            try
            {
                var folder = Path.GetDirectoryName(dest);
                if (!string.IsNullOrWhiteSpace(folder))
                    Directory.CreateDirectory(folder);
                var bytes = Http().GetByteArrayAsync(AbsoluteUrl(url)).GetAwaiter().GetResult();
                if (bytes == null || bytes.Length < 64)
                    return false;
                var tmp = dest + ".tmp";
                File.WriteAllBytes(tmp, bytes);
                if (File.Exists(dest))
                    File.Replace(tmp, dest, null);
                else
                    File.Move(tmp, dest);
                return File.Exists(dest) && new FileInfo(dest).Length >= 64;
            }
            catch
            {
                return false;
            }
        }

        private static HttpClient Http()
        {
            if (_http != null)
                return _http;
            lock (Gate)
            {
                if (_http != null)
                    return _http;
                var handler = new HttpClientHandler
                {
                    ServerCertificateCustomValidationCallback = (_, __, ___, ____) => true
                };
                _http = new HttpClient(handler) { Timeout = TimeSpan.FromSeconds(60) };
                return _http;
            }
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

        private static string Key(string code, string view)
        {
            var c = (code ?? "").Trim();
            var v = NormalizeViewName(view);
            if (string.IsNullOrWhiteSpace(c) || string.IsNullOrWhiteSpace(v))
                return null;
            return c + "|" + v;
        }

        private static string NormalizeViewName(string view)
        {
            var v = (view ?? "").Trim().ToLowerInvariant();
            if (v == "3d") return "3d";
            if (v == "xr") return "xr";
            if (v == "snap") return "snap";
            return "3dref";
        }

        private static void Normalize(string view, string codeName, out string folder, out string file)
        {
            var name = (codeName ?? "").Trim();
            var v = NormalizeViewName(view);
            if (v == "3d") { folder = "3D"; file = name + ".dwg"; return; }
            if (v == "xr") { folder = "Xr"; file = name + "X.dwg"; return; }
            if (v == "snap") { folder = "Snaps"; file = name + ".json"; return; }
            folder = "3DRef";
            file = name + "R.dwg";
        }

        private static bool IsUsable(string path)
        {
            try { return !string.IsNullOrWhiteSpace(path) && File.Exists(path) && new FileInfo(path).Length >= 64; }
            catch { return false; }
        }

        private static string Read(JToken token, params string[] names)
        {
            foreach (var name in names)
            {
                var value = ((string)token[name] ?? "").Trim();
                if (!string.IsNullOrWhiteSpace(value))
                    return value;
            }
            return "";
        }

        private static void CadLine(string text)
        {
            try
            {
                EventHandler handler = null;
                handler = (s, e) =>
                {
                    AcadApp.Idle -= handler;
                    try
                    {
                        var ed = AcadApp.DocumentManager.MdiActiveDocument?.Editor;
                        ed?.WriteMessage("\n" + text + "\n");
                    }
                    catch { }
                };
                AcadApp.Idle += handler;
            }
            catch { }
        }

        private sealed class RemoteFile
        {
            public string Code;
            public string View;
            public string Folder;
            public string File;
            public string Url;
            public string Version;
        }
    }
}
