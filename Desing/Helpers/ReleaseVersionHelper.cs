using System;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Web;
using System.Web.Hosting;
using Newtonsoft.Json.Linq;

namespace Desing.Helpers
{
    /// <summary>
    /// «Última publicación: dd/MM/yyyy HH:mm v2.00» (login y pie).
    /// Fecha: <c>App_Data/publish-info.json</c> que escribe el target <c>Desing2WritePublishInfo</c> de Design.csproj en cada
    /// publicación (Web Deploy / carpeta), siempre con contenido nuevo para que Web Deploy lo suba. Si no existe (desarrollo),
    /// se usa la fecha de bin/Desing.dll. Se muestra en hora de Madrid y se relee cuando cambia el archivo (App_Data no reinicia la app).
    /// </summary>
    public static class ReleaseVersionHelper
    {
        public const string ProductVersion = "v2.00";

        private const string PublishInfoVirtualPath = "~/App_Data/publish-info.json";
        private static readonly TimeSpan RecheckInterval = TimeSpan.FromSeconds(30);
        private static readonly object Sync = new object();
        private static string _cached;
        private static DateTime _cachedFileStampUtc = DateTime.MinValue;
        private static DateTime _nextCheckUtc = DateTime.MinValue;

        public static string CurrentReleaseNumber
        {
            get
            {
                var now = DateTime.UtcNow;
                lock (Sync)
                {
                    if (_cached != null && now < _nextCheckUtc) return _cached;
                    _nextCheckUtc = now + RecheckInterval;
                    var infoPath = PublishInfoPath();
                    var fileStamp = infoPath != null && File.Exists(infoPath) ? File.GetLastWriteTimeUtc(infoPath) : DateTime.MinValue;
                    if (_cached == null || fileStamp != _cachedFileStampUtc)
                    {
                        _cachedFileStampUtc = fileStamp;
                        _cached = ToMadrid(ResolveApplicationStampUtc(infoPath)).ToString("dd/MM/yyyy HH:mm", CultureInfo.InvariantCulture) + " " + ProductVersion;
                    }
                    return _cached;
                }
            }
        }

        private static string PublishInfoPath()
        {
            try
            {
                return HostingEnvironment.IsHosted
                    ? HostingEnvironment.MapPath(PublishInfoVirtualPath)
                    : Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "App_Data", "publish-info.json");
            }
            catch (Exception)
            {
                return null;
            }
        }

        private static DateTime ResolveApplicationStampUtc(string infoPath)
        {
            // 1) Fecha de publicación escrita por MSBuild (UTC, ISO 8601).
            try
            {
                if (infoPath != null && File.Exists(infoPath))
                {
                    var json = JObject.Parse(File.ReadAllText(infoPath));
                    DateTime published;
                    var raw = (string)json["publishedUtc"];
                    if (!string.IsNullOrWhiteSpace(raw)
                        && DateTime.TryParse(raw, CultureInfo.InvariantCulture, DateTimeStyles.AdjustToUniversal | DateTimeStyles.AssumeUniversal, out published))
                    {
                        return DateTime.SpecifyKind(published, DateTimeKind.Utc);
                    }
                }
            }
            catch (Exception)
            {
                // archivo corrupto o a medio subir: usar la fecha de la DLL
            }

            // 2) bin/Desing.dll (no la copia sombra de Temporary ASP.NET Files).
            var binDll = HostingEnvironment.IsHosted ? HostingEnvironment.MapPath("~/bin/Desing.dll") : null;
            if (!string.IsNullOrWhiteSpace(binDll) && File.Exists(binDll)) return File.GetLastWriteTimeUtc(binDll);

            var assemblyPath = Assembly.GetExecutingAssembly().Location;
            if (!string.IsNullOrWhiteSpace(assemblyPath) && File.Exists(assemblyPath)) return File.GetLastWriteTimeUtc(assemblyPath);

            var webConfigPath = HttpContext.Current != null ? HttpContext.Current.Server.MapPath("~/Web.config") : null;
            return !string.IsNullOrWhiteSpace(webConfigPath) && File.Exists(webConfigPath)
                ? File.GetLastWriteTimeUtc(webConfigPath)
                : DateTime.UtcNow;
        }

        private static DateTime ToMadrid(DateTime utc)
        {
            try
            {
                var madrid = TimeZoneInfo.FindSystemTimeZoneById("Romance Standard Time");
                return TimeZoneInfo.ConvertTimeFromUtc(DateTime.SpecifyKind(utc, DateTimeKind.Utc), madrid);
            }
            catch (TimeZoneNotFoundException)
            {
                return utc.ToLocalTime();
            }
        }
    }
}
