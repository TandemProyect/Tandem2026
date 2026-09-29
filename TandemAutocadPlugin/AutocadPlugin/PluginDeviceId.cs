using System;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;

namespace AutocadPlugin
{
    /// <summary>
    /// Identificador estable del equipo para dbo.TSql_PluginDeviceAuth.
    /// </summary>
    public static class PluginDeviceId
    {
        public static string Current()
        {
            var seed = string.Concat(
                Environment.MachineName, "|",
                Environment.UserName, "|",
                Environment.UserDomainName, "|",
                Environment.OSVersion, "|",
                MachineGuid());
            using (var sha = SHA256.Create())
            {
                byte[] hash = sha.ComputeHash(Encoding.UTF8.GetBytes(seed));
                var sb = new StringBuilder(hash.Length * 2);
                foreach (byte b in hash)
                    sb.Append(b.ToString("x2"));
                return sb.ToString();
            }
        }

        public static string WindowsUser()
        {
            var domain = Environment.UserDomainName ?? "";
            var user = Environment.UserName ?? "";
            if (string.IsNullOrEmpty(domain))
                return user;
            return domain + "\\" + user;
        }

        public static string PluginVersion()
        {
            try
            {
                var v = Assembly.GetExecutingAssembly().GetName().Version;
                return v != null ? v.ToString() : "";
            }
            catch
            {
                return "";
            }
        }

        private static string MachineGuid()
        {
            try
            {
                using (var key = Microsoft.Win32.Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\Cryptography"))
                {
                    return key?.GetValue("MachineGuid")?.ToString() ?? "NO_GUID";
                }
            }
            catch
            {
                return "NO_GUID";
            }
        }
    }
}
