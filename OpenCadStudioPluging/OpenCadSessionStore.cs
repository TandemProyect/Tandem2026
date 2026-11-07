using System;
using System.IO;
using System.Text.Json;

namespace OpenCadStudioPluging
{
    internal static class OpenCadSessionStore
    {
        public static bool HasRecentForCurrentBoot()
        {
            var data = Read();
            if (data == null)
                return false;
            if (!SameServer(data.Server, MvcServerSettings.CurrentUrl()))
                return false;
            if (!IsSameBootMarker(data.BootMarker, CurrentBootMarker()))
                return false;
            return data.Utc.AddDays(2) >= DateTime.UtcNow;
        }

        public static void RememberCurrentBoot()
        {
            OpenCadStudioPluginEnvironment.EnsureFolders();
            var path = OpenCadStudioPluginEnvironment.SessionStorePath();
            var json = JsonSerializer.Serialize(new SessionRecord
            {
                Server = MvcServerSettings.CurrentUrl() ?? "",
                BootMarker = CurrentBootMarker(),
                Utc = DateTime.UtcNow
            });
            File.WriteAllText(path, json);
        }

        public static void Clear()
        {
            var path = OpenCadStudioPluginEnvironment.SessionStorePath();
            if (File.Exists(path))
                File.Delete(path);
        }

        internal static string CurrentBootMarker()
        {
            var bootUtc = DateTime.UtcNow - TimeSpan.FromMilliseconds(Environment.TickCount64);
            return new DateTime(bootUtc.Ticks - (bootUtc.Ticks % TimeSpan.TicksPerMinute), DateTimeKind.Utc)
                .ToString("yyyyMMddHHmm");
        }

        internal static bool IsSameBootMarker(string left, string right)
        {
            return string.Equals((left ?? "").Trim(), (right ?? "").Trim(), StringComparison.Ordinal);
        }

        private static bool SameServer(string left, string right)
        {
            return string.Equals(Normalize(left), Normalize(right), StringComparison.OrdinalIgnoreCase);
        }

        private static string Normalize(string url)
        {
            return (url ?? "").Trim().TrimEnd('/');
        }

        private static SessionRecord Read()
        {
            var path = OpenCadStudioPluginEnvironment.SessionStorePath();
            if (!File.Exists(path))
                return null;

            var json = File.ReadAllText(path);
            return JsonSerializer.Deserialize<SessionRecord>(json);
        }

        private sealed class SessionRecord
        {
            public string Server { get; set; }
            public string BootMarker { get; set; }
            public DateTime Utc { get; set; }
        }
    }
}
