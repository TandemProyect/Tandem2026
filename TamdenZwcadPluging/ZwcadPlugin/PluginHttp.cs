using System;
using System.Net.Http;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;

namespace ZwcadPlugin
{
    /// <summary>
    /// HTTP del plugin: keep-alive y TLS estricto en tdesing.net.
    /// El bypass de certificado solo vale para IIS Express (localhost).
    /// </summary>
    internal static class PluginHttp
    {
        public static HttpClientHandler CreateHandler()
        {
            return new HttpClientHandler
            {
                ServerCertificateCustomValidationCallback = AcceptLocalDevCertificate
            };
        }

        public static bool AcceptLocalDevCertificate(
            HttpRequestMessage request,
            X509Certificate2 certificate,
            X509Chain chain,
            SslPolicyErrors errors)
        {
            if (errors == SslPolicyErrors.None)
                return true;
            if (MvcServerSettings.IsProduction())
                return false;
            var host = request != null && request.RequestUri != null
                ? request.RequestUri.Host
                : "";
            return IsLoopback(host);
        }

        public static bool IsLoopback(string host)
        {
            if (string.IsNullOrWhiteSpace(host))
                return false;
            return host.Equals("localhost", StringComparison.OrdinalIgnoreCase)
                || host.Equals("127.0.0.1", StringComparison.OrdinalIgnoreCase)
                || host.Equals("::1", StringComparison.OrdinalIgnoreCase);
        }
    }
}

