using System;
using System.Configuration;
using System.Data.SqlClient;
using System.Diagnostics;

namespace Desing.Helpers
{
    /// <summary>
    /// Ping SQL sin EDMX. El plugin y el panel de conexión lo usan a menudo.
    /// </summary>
    public static class SqlConnectionPing
    {
        public sealed class Result
        {
            public bool Ok { get; set; }
            public long Ms { get; set; }
        }

        public static Result SelectOne(string connectionName = "ConexionData")
        {
            var raw = ConfigurationManager.ConnectionStrings[connectionName]?.ConnectionString;
            return SelectOneRaw(raw);
        }

        public static Result SelectOneRaw(string rawConnection)
        {
            var sw = Stopwatch.StartNew();
            try
            {
                var sql = Unwrap(rawConnection);
                if (string.IsNullOrWhiteSpace(sql))
                    return new Result { Ok = false, Ms = -1 };
                using (var cn = new SqlConnection(sql))
                {
                    cn.Open();
                    using (var cmd = new SqlCommand("SELECT CAST(1 AS INT)", cn))
                        cmd.ExecuteScalar();
                }
                return new Result { Ok = true, Ms = sw.ElapsedMilliseconds };
            }
            catch
            {
                return new Result { Ok = false, Ms = -1 };
            }
        }

        public static string Unwrap(string raw)
        {
            if (string.IsNullOrWhiteSpace(raw))
                return "";
            const string key = "provider connection string=";
            var i = raw.IndexOf(key, StringComparison.OrdinalIgnoreCase);
            if (i < 0)
                return raw;
            var rest = raw.Substring(i + key.Length).Trim();
            if (rest.StartsWith("\"", StringComparison.Ordinal))
            {
                var end = rest.IndexOf('"', 1);
                if (end > 1)
                    return rest.Substring(1, end - 1);
            }
            return rest.Trim().TrimEnd(';');
        }
    }
}
