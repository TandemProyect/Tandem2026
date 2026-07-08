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

        public static string CurrentReleaseNumber
        {
            get { return ReleaseNumberLazy.Value; }
        }

        private static string BuildReleaseNumber()
        {
            var stamp = ResolveApplicationStamp();
            return stamp.ToString("yyyyMMdd.HHmm");
        }

        private static DateTime ResolveApplicationStamp()
        {
            var assemblyPath = Assembly.GetExecutingAssembly().Location;
            if (!string.IsNullOrWhiteSpace(assemblyPath) && File.Exists(assemblyPath))
            {
                return File.GetLastWriteTime(assemblyPath);
            }

            var webConfigPath = HttpContext.Current != null
                ? HttpContext.Current.Server.MapPath("~/Web.config")
                : null;
            if (!string.IsNullOrWhiteSpace(webConfigPath) && File.Exists(webConfigPath))
            {
                return File.GetLastWriteTime(webConfigPath);
            }

            return DateTime.Now;
        }
    }
}
