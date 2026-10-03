using DAL;
using System;
using System.Linq;
using System.Web;
using System.Web.Caching;
using System.Web.Mvc;

namespace Desing.Helpers
{
    /// <summary>
    /// Equipo del plugin CAD: el alta se hace en Personal.
    /// El plugin solo toca un equipo ya autorizado.
    /// </summary>
    public static class PluginCadDeviceHelper
    {
        public const string CookieName = "tandem_plugin_device";
        public const string LogoCookieName = "tandem_plugin_company_logo";
        public const string DefaultLogoVirtualPath = "/Content/images/tDesing/t-desing-net.png";
        private static readonly TimeSpan AuthCacheDuration = TimeSpan.FromSeconds(45);

        public sealed class Snapshot
        {
            public string DeviceId { get; set; }
            public string MachineName { get; set; }
            public string UsuarioWindows { get; set; }
            public string PluginVersion { get; set; }
        }

        public static void WriteCookie(HttpResponseBase response, HttpRequestBase request, Snapshot snap)
        {
            if (response == null || snap == null || string.IsNullOrWhiteSpace(snap.DeviceId))
                return;

            var cookie = new HttpCookie(CookieName)
            {
                HttpOnly = true,
                Secure = request != null && request.IsSecureConnection,
                Path = "/",
                Value = Encode(snap)
            };
            response.Cookies.Set(cookie);
        }

        public static Snapshot TryReadCookie(HttpRequestBase request)
        {
            if (request == null || request.Cookies == null)
                return null;
            var cookie = request.Cookies[CookieName];
            if (cookie == null || string.IsNullOrWhiteSpace(cookie.Value))
                return null;
            return Decode(cookie.Value);
        }

        public static TSql_PluginDeviceAuth Find(ConexionData db, string deviceId)
        {
            if (db == null || string.IsNullOrWhiteSpace(deviceId))
                return null;
            return db.TSql_PluginDeviceAuth.FirstOrDefault(d => d.DeviceId == deviceId);
        }

        public static bool IsTrusted(TSql_PluginDeviceAuth row)
        {
            return row != null
                && !row.AttIsDeleted
                && row.Allowed
                && row.IsActive
                && !row.IsRevoked
                && !string.IsNullOrWhiteSpace(row.LinAspNetUsert);
        }

        public static bool IsBlocked(TSql_PluginDeviceAuth row)
        {
            if (row == null || row.AttIsDeleted)
                return false;
            return row.IsRevoked || !row.Allowed || !row.IsActive;
        }

        /// <summary>
        /// Usuario CAD permitido: cuenta activa (EmailConfirmed) y empleado no borrado.
        /// </summary>
        public static bool IsCadDeveloper(ConexionData db, string userId)
        {
            if (db == null || string.IsNullOrWhiteSpace(userId))
                return false;
            bool cached;
            if (TryGetCachedBool("PluginCad_Dev_" + userId, out cached))
                return cached;
            try
            {
                var ok = db.Database.SqlQuery<int>(
                    @"SELECT TOP 1 CASE WHEN Is_CadDeveloper = 1 THEN 1 ELSE 0 END
                      FROM dbo.TSql_Employee
                      WHERE LinAspNetUsert = @p0 AND AttIsDeleted = 0",
                    userId).FirstOrDefault() == 1;
                RememberBool("PluginCad_Dev_" + userId, ok);
                return ok;
            }
            catch
            {
                return false;
            }
        }

        public static bool AllowsPluginOnThisPc(ConexionData db, string userId, Snapshot snap)
        {
            if (IsCadDeveloper(db, userId))
                return true;

            var registered = FindAuthorizedForUser(db, userId);
            if (registered == null)
                return true;

            if (snap != null && !string.IsNullOrWhiteSpace(snap.DeviceId)
                && string.Equals(registered.DeviceId, snap.DeviceId, StringComparison.OrdinalIgnoreCase))
                return true;
            if (snap != null && MachineMatches(registered, snap.MachineName))
                return true;
            return false;
        }

        public static bool IsUserAllowed(ConexionData db, string userId)
        {
            if (db == null || string.IsNullOrWhiteSpace(userId))
                return false;
            bool cached;
            if (TryGetCachedBool("PluginCad_Allowed_" + userId, out cached))
                return cached;

            var user = db.AspNetUsers.FirstOrDefault(u => u.Id == userId);
            var ok = false;
            if (user != null && user.EmailConfirmed)
            {
                if (!(user.LockoutEnabled
                    && user.LockoutEndDateUtc.HasValue
                    && user.LockoutEndDateUtc.Value > DateTime.UtcNow))
                    ok = db.TSql_Employee.Any(e => e.LinAspNetUsert == userId && !e.AttIsDeleted);
            }
            RememberBool("PluginCad_Allowed_" + userId, ok);
            return ok;
        }

        public static string ResolveCompanyLogoAbsoluteUrl(ConexionData db, string userId, HttpRequestBase request, UrlHelper url)
        {
            if (db == null || string.IsNullOrWhiteSpace(userId) || request == null || url == null)
                return null;

            var employee = db.TSql_Employee.FirstOrDefault(e => e.LinAspNetUsert == userId && !e.AttIsDeleted);
            if (employee == null)
                return null;

            var company = db.TSql_Company.FirstOrDefault(c => c.SysObjectID == employee.LinCompany && !c.BitIsDeleted);
            if (company == null || string.IsNullOrWhiteSpace(company.TextLogo))
                return null;

            var normalized = IntranetFileHelper.NormalizeUploadedWebPath(company.TextLogo);
            var publicUrl = IntranetFileHelper.ResolvePublicUrl(url, normalized);
            if (string.IsNullOrWhiteSpace(publicUrl))
                return null;
            if (publicUrl.StartsWith("http://", StringComparison.OrdinalIgnoreCase)
                || publicUrl.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
                return publicUrl;
            if (publicUrl.StartsWith("//", StringComparison.Ordinal))
                return (request.Url != null ? request.Url.Scheme : "https") + ":" + publicUrl;

            var origin = request.Url != null ? request.Url.GetLeftPart(UriPartial.Authority) : "";
            if (string.IsNullOrWhiteSpace(origin))
                return publicUrl;
            if (!publicUrl.StartsWith("/", StringComparison.Ordinal))
                publicUrl = "/" + publicUrl.TrimStart('/');
            return origin + publicUrl;
        }

        public static void WriteLogoCookie(HttpResponseBase response, HttpRequestBase request, string logoUrl)
        {
            if (response == null)
                return;
            if (string.IsNullOrWhiteSpace(logoUrl))
            {
                ClearLogoCookie(response);
                return;
            }

            var cookie = new HttpCookie(LogoCookieName)
            {
                HttpOnly = true,
                Secure = request != null && request.IsSecureConnection,
                Path = "/",
                Value = logoUrl.Trim(),
                Expires = DateTime.UtcNow.AddDays(365)
            };
            response.Cookies.Set(cookie);
        }

        public static void ClearLogoCookie(HttpResponseBase response)
        {
            if (response == null)
                return;
            var cookie = new HttpCookie(LogoCookieName)
            {
                HttpOnly = true,
                Path = "/",
                Value = "",
                Expires = DateTime.UtcNow.AddDays(-1)
            };
            response.Cookies.Set(cookie);
        }

        public static string TryReadLogoCookie(HttpRequestBase request)
        {
            if (request == null || request.Cookies == null)
                return null;
            var cookie = request.Cookies[LogoCookieName];
            if (cookie == null || string.IsNullOrWhiteSpace(cookie.Value))
                return null;
            return cookie.Value.Trim();
        }

        /// <summary>
        /// El plugin no da de alta equipos. Solo toca una fila ya creada en Personal
        /// (mismo usuario y DeviceId, o mismo nombre de equipo si el DeviceId aún está vacío).
        /// </summary>
        public static void RegisterAfterLogin(ConexionData db, Snapshot snap, string userId)
        {
            if (db == null || snap == null || string.IsNullOrWhiteSpace(userId))
                return;

            var row = Find(db, snap.DeviceId);
            if (row == null)
                row = FindAuthorizedByUserAndMachine(db, userId, snap.MachineName);
            if (row == null || IsBlocked(row))
                return;
            if (!string.Equals(row.LinAspNetUsert, userId, StringComparison.Ordinal))
                return;
            if (!string.IsNullOrWhiteSpace(row.DeviceId)
                && !string.IsNullOrWhiteSpace(snap.DeviceId)
                && !string.Equals(row.DeviceId, snap.DeviceId, StringComparison.OrdinalIgnoreCase))
                return;

            if (string.IsNullOrWhiteSpace(row.DeviceId) && !string.IsNullOrWhiteSpace(snap.DeviceId))
                row.DeviceId = Trunc(snap.DeviceId, 128);

            ApplySnapshot(row, snap);
            row.LastCheckUtc = DateTime.UtcNow;
            row.LinModifiedBy = userId;
            row.AttLastModification = DateTime.UtcNow;
            db.SaveChanges();
        }

        public static TSql_PluginDeviceAuth FindAuthorizedForUser(ConexionData db, string userId)
        {
            if (db == null || string.IsNullOrWhiteSpace(userId))
                return null;
            return db.TSql_PluginDeviceAuth
                .Where(d => d.LinAspNetUsert == userId && !d.AttIsDeleted)
                .OrderByDescending(d => d.SysObjectID)
                .ToList()
                .FirstOrDefault(d => !IsBlocked(d));
        }

        public static TSql_PluginDeviceAuth FindAuthorizedByUserAndMachine(ConexionData db, string userId, string machineName)
        {
            var machine = (machineName ?? "").Trim();
            if (db == null || string.IsNullOrWhiteSpace(userId) || string.IsNullOrWhiteSpace(machine))
                return null;
            return db.TSql_PluginDeviceAuth
                .Where(d => d.LinAspNetUsert == userId && !d.AttIsDeleted)
                .ToList()
                .FirstOrDefault(d =>
                    !IsBlocked(d)
                    && string.Equals((d.MachineName ?? "").Trim(), machine, StringComparison.OrdinalIgnoreCase));
        }

        public static bool MachineMatches(TSql_PluginDeviceAuth row, string machineName)
        {
            if (row == null)
                return false;
            var expected = (row.MachineName ?? "").Trim();
            var actual = (machineName ?? "").Trim();
            return expected.Length > 0
                && actual.Length > 0
                && string.Equals(expected, actual, StringComparison.OrdinalIgnoreCase);
        }

        public static void Touch(ConexionData db, TSql_PluginDeviceAuth row, Snapshot snap, string userId)
        {
            if (db == null || row == null)
                return;
            var now = DateTime.UtcNow;
            ApplySnapshot(row, snap);
            row.LastCheckUtc = now;
            row.LinModifiedBy = userId ?? row.LinModifiedBy;
            row.AttLastModification = now;
            db.SaveChanges();
        }

        private static void ApplySnapshot(TSql_PluginDeviceAuth row, Snapshot snap)
        {
            if (row == null || snap == null)
                return;
            if (!string.IsNullOrWhiteSpace(snap.MachineName))
                row.MachineName = Trunc(snap.MachineName, 128);
            if (!string.IsNullOrWhiteSpace(snap.UsuarioWindows))
                row.UsuarioWindows = Trunc(snap.UsuarioWindows, 128);
            if (!string.IsNullOrWhiteSpace(snap.PluginVersion))
                row.PluginVersion = Trunc(snap.PluginVersion, 50);
        }

        private static string Encode(Snapshot snap)
        {
            return string.Join("|",
                HttpUtility.UrlEncode(snap.DeviceId ?? ""),
                HttpUtility.UrlEncode(snap.MachineName ?? ""),
                HttpUtility.UrlEncode(snap.UsuarioWindows ?? ""),
                HttpUtility.UrlEncode(snap.PluginVersion ?? ""));
        }

        private static Snapshot Decode(string raw)
        {
            var parts = (raw ?? "").Split(new[] { '|' }, 4);
            return new Snapshot
            {
                DeviceId = parts.Length > 0 ? HttpUtility.UrlDecode(parts[0]) : null,
                MachineName = parts.Length > 1 ? HttpUtility.UrlDecode(parts[1]) : null,
                UsuarioWindows = parts.Length > 2 ? HttpUtility.UrlDecode(parts[2]) : null,
                PluginVersion = parts.Length > 3 ? HttpUtility.UrlDecode(parts[3]) : null
            };
        }

        private static string Trunc(string value, int max)
        {
            if (string.IsNullOrEmpty(value))
                return value;
            return value.Length <= max ? value : value.Substring(0, max);
        }

        private static bool TryGetCachedBool(string key, out bool value)
        {
            var hit = HttpRuntime.Cache[key];
            if (hit is bool)
            {
                value = (bool)hit;
                return true;
            }
            value = false;
            return false;
        }

        private static void RememberBool(string key, bool value)
        {
            HttpRuntime.Cache.Insert(
                key,
                value,
                null,
                DateTime.UtcNow.Add(AuthCacheDuration),
                Cache.NoSlidingExpiration);
        }
    }
}
