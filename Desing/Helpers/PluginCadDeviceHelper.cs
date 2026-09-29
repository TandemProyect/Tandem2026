using DAL;
using System;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Desing.Helpers
{
    /// <summary>
    /// Equipo del plugin CAD: auto-login si está registrado y activo;
    /// si no, el login correcto lo da de alta ligado al usuario.
    /// </summary>
    public static class PluginCadDeviceHelper
    {
        public const string CookieName = "tandem_plugin_device";
        public const string LogoCookieName = "tandem_plugin_company_logo";
        public const string DefaultLogoVirtualPath = "/Content/images/tDesing/t-desing-net.png";

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
        public static bool IsUserAllowed(ConexionData db, string userId)
        {
            if (db == null || string.IsNullOrWhiteSpace(userId))
                return false;

            var user = db.AspNetUsers.FirstOrDefault(u => u.Id == userId);
            if (user == null || !user.EmailConfirmed)
                return false;
            if (user.LockoutEnabled
                && user.LockoutEndDateUtc.HasValue
                && user.LockoutEndDateUtc.Value > DateTime.UtcNow)
                return false;

            return db.TSql_Employee.Any(e => e.LinAspNetUsert == userId && !e.AttIsDeleted);
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
        /// Alta o actualización tras un login correcto. No reactiva equipos bloqueados.
        /// </summary>
        public static void RegisterAfterLogin(ConexionData db, Snapshot snap, string userId)
        {
            if (db == null || snap == null || string.IsNullOrWhiteSpace(snap.DeviceId) || string.IsNullOrWhiteSpace(userId))
                return;

            var now = DateTime.UtcNow;
            var row = Find(db, snap.DeviceId);
            if (IsBlocked(row))
                return;

            if (row == null)
            {
                row = new TSql_PluginDeviceAuth
                {
                    DeviceId = Trunc(snap.DeviceId, 128),
                    LinAspNetUsert = userId,
                    MachineName = Trunc(snap.MachineName, 128),
                    UsuarioWindows = Trunc(snap.UsuarioWindows, 128),
                    PluginVersion = Trunc(snap.PluginVersion, 50),
                    Allowed = true,
                    IsActive = true,
                    IsRevoked = false,
                    Estado = "Activo",
                    AttIsDeleted = false,
                    LastCheckUtc = now,
                    LinCreatedBy = userId,
                    AttCreated = now,
                    LinModifiedBy = userId,
                    AttLastModification = now
                };
                db.TSql_PluginDeviceAuth.Add(row);
            }
            else
            {
                row.LinAspNetUsert = userId;
                ApplySnapshot(row, snap);
                row.Allowed = true;
                row.IsActive = true;
                row.IsRevoked = false;
                row.Estado = "Activo";
                row.AttIsDeleted = false;
                row.LastCheckUtc = now;
                row.LinModifiedBy = userId;
                row.AttLastModification = now;
            }

            db.SaveChanges();
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
    }
}
