using System;
using System.Net.Http;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;

namespace OpenCadStudioPluging
{
    internal static class MvcPingClient
    {
        public static async Task<int> PingAsync()
        {
            var baseUrl = MvcServerSettings.CurrentUrl();
            var url = baseUrl + "DesignToolsAutocad/PluginPing?light=1";

            using (var client = new HttpClient(CreateHandler()) { Timeout = TimeSpan.FromSeconds(10) })
            {
                var response = await client.GetAsync(url).ConfigureAwait(false);
                var body = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
                if (!response.IsSuccessStatusCode)
                {
                    Console.Error.WriteLine("Error HTTP " + (int)response.StatusCode + " al conectar con " + url);
                    Console.Error.WriteLine(body);
                    return 1;
                }

                Console.WriteLine("Conexion MVC OK: " + MvcServerSettings.CurrentLabel() + " - " + url);
                return 0;
            }
        }

        private static HttpClientHandler CreateHandler()
        {
            return new HttpClientHandler
            {
                ServerCertificateCustomValidationCallback = AcceptLocalDevCertificate
            };
        }

        private static bool AcceptLocalDevCertificate(
            HttpRequestMessage request,
            X509Certificate2 certificate,
            X509Chain chain,
            SslPolicyErrors errors)
        {
            if (errors == SslPolicyErrors.None)
                return true;
            if (MvcServerSettings.IsProduction())
                return false;

            var host = request != null && request.RequestUri != null ? request.RequestUri.Host : "";
            return string.Equals(host, "localhost", StringComparison.OrdinalIgnoreCase)
                || string.Equals(host, "127.0.0.1", StringComparison.OrdinalIgnoreCase)
                || string.Equals(host, "::1", StringComparison.OrdinalIgnoreCase);
        }
    }
}
