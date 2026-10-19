using System;
using System.IO;
using System.Reflection;
using System.Web;

namespace Desing.Helpers
{
    public static class ReleaseVersionHelper
    {
        private static readonly Lazy<string> ReleaseNumberLazy =
            new Lazy<string>(BuildReleaseNumber);

        public const string ProductVersion = "v2.00";

        public static string CurrentReleaseNumber
        {
            get { return ReleaseNumberLazy.Value; }
        }

        private static string BuildReleaseNumber()
        {
            var stamp = ResolveApplicationStamp();
            return stamp.ToString("dd/MM/yyyy HH:mm") + " " + ProductVersion;
        }

        private static DateTime ResolveApplicationStamp()
        {
            DateTime utc;
            var assemblyPath = Assembly.GetExecutingAssembly().Location;
            if (!string.IsNullOrWhiteSpace(assemblyPath) && File.Exists(assemblyPath))
            {
                utc = File.GetLastWriteTimeUtc(assemblyPath);
            }
            else
            {
                var webConfigPath = HttpContext.Current != null
                    ? HttpContext.Current.Server.MapPath("~/Web.config")
                    : null;
                utc = !string.IsNullOrWhiteSpace(webConfigPath) && File.Exists(webConfigPath)
                    ? File.GetLastWriteTimeUtc(webConfigPath)
                    : DateTime.UtcNow;
            }

            var madrid = TimeZoneInfo.FindSystemTimeZoneById("Romance Standard Time");
            return TimeZoneInfo.ConvertTimeFromUtc(DateTime.SpecifyKind(utc, DateTimeKind.Utc), madrid);
        }
    }
}
