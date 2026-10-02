using System;
using System.Globalization;
using System.Text;
using System.Web;
using System.Web.Security;

namespace Desing.Helpers
{
    /// <summary>
    /// Enlace de instalación del plugin: solo por correo, no está en el menú.
    /// </summary>
    public static class PluginInstallLink
    {
        private const string Purpose = "AtDesing.PluginInstall.v1";
        public static readonly TimeSpan DefaultLife = TimeSpan.FromDays(7);

        public static string Create(long employeeId, string userId)
        {
            var exp = DateTime.UtcNow.Add(DefaultLife).Ticks;
            var raw = employeeId.ToString(CultureInfo.InvariantCulture)
                + "|" + (userId ?? "")
                + "|" + exp.ToString(CultureInfo.InvariantCulture);
            var bytes = MachineKey.Protect(Encoding.UTF8.GetBytes(raw), Purpose);
            return HttpServerUtility.UrlTokenEncode(bytes);
        }

        public static bool TryRead(string token, out long employeeId, out string userId, out string error)
        {
            employeeId = 0;
            userId = null;
            error = "invalid";
            if (string.IsNullOrWhiteSpace(token))
                return false;
            try
            {
                var bytes = HttpServerUtility.UrlTokenDecode(token.Trim());
                if (bytes == null || bytes.Length == 0)
                    return false;
                var raw = Encoding.UTF8.GetString(MachineKey.Unprotect(bytes, Purpose));
                var parts = raw.Split('|');
                if (parts.Length != 3)
                    return false;
                if (!long.TryParse(parts[0], NumberStyles.Integer, CultureInfo.InvariantCulture, out employeeId))
                    return false;
                userId = parts[1];
                long ticks;
                if (!long.TryParse(parts[2], NumberStyles.Integer, CultureInfo.InvariantCulture, out ticks))
                    return false;
                if (DateTime.UtcNow.Ticks > ticks)
                {
                    error = "expired";
                    return false;
                }
                error = null;
                return !string.IsNullOrWhiteSpace(userId);
            }
            catch
            {
                return false;
            }
        }
    }
}
