using System;
using System.IO;
using System.Net.Http;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using System.Collections.Generic;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows;

namespace FreeCadPluging
{
    internal static class Program
    {
        private const string LocalUrl = "https://localhost:44384/";
        private const string ProductionUrl = "https://tdesing.net/";

        [STAThread]
        private static int Main(string[] args)
        {
            try
            {
                return MainAsync(args ?? Array.Empty<string>()).GetAwaiter().GetResult();
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine(FormatPluginError(ex));
                return 1;
            }
        }

        private static async Task<int> MainAsync(string[] args)
        {
            var command = args.Length > 0 ? (args[0] ?? "").Trim().ToLowerInvariant() : "show-server";
            switch (command)
            {
                case "ping":
                case "connect":
                    return await PingAsync().ConfigureAwait(false);
                case "set-local":
                    SetTarget("local");
                    Console.WriteLine("Servidor cambiado a local (localhost:44384). ");
                    return 0;
                case "set-production":
                    SetTarget("production");
                    Console.WriteLine("Servidor cambiado a produccion (tdesing.net). ");
                    return 0;
                case "show-server":
                    Console.WriteLine("Servidor MVC: " + CurrentLabel() + " - " + CurrentUrl());
                    return 0;
                case "connect-ui":
                case "prepare":
                case "atdesing":
                    return RunConnectUi();
                case "wall-2d":
                    Console.WriteLine("Muro 2D en FreeCAD pendiente. La UI y API seran comunes via MVC.");
                    return 0;
                case "wall-3d":
                    Console.WriteLine("Generar 3D en FreeCAD pendiente. Se usara LCornerDetector por MVC.");
                    return 0;
                case "formwork":
                case "encofrar":
                    Console.WriteLine("Encofrado FreeCAD iniciado. El adaptador FreeCAD insertara los objetos ATK60 disponibles.");
                    return 0;
                case "formwork-solve":
                    return await SolveFormworkAsync(args).ConfigureAwait(false);
                default:
                    Console.Error.WriteLine("Comando FreeCadPluging no reconocido: " + command);
                    return 2;
            }
        }

        private static int RunConnectUi()
        {
            FreeCadPluginEnvironment.EnsureFolders();
            var app = new Application
            {
                ShutdownMode = ShutdownMode.OnLastWindowClose
            };
            FreeCadPaletteHost.Show();
            return app.Run();
        }

        private static async Task<int> SolveFormworkAsync(string[] args)
        {
            if (args.Length < 2 || string.IsNullOrWhiteSpace(args[1]))
            {
                Console.Error.WriteLine("Uso: FreeCadPluging.exe formwork-solve <ids-json-file>");
                return 2;
            }

            var idsJsonPath = args[1];
            if (!File.Exists(idsJsonPath))
            {
                Console.Error.WriteLine("No existe el archivo IdsJson: " + idsJsonPath);
                return 2;
            }

            var idsJson = File.ReadAllText(idsJsonPath);
            using (var client = new HttpClient(CreateHandler()) { Timeout = TimeSpan.FromSeconds(120) })
            using (var content = new FormUrlEncodedContent(new[]
            {
                new KeyValuePair<string, string>("IdsJson", idsJson ?? "")
            }))
            {
                var url = CurrentUrl() + "DesignToolsAutocad/PluginEncofrarAtk60";
                var response = await client.PostAsync(url, content).ConfigureAwait(false);
                var body = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
                if (!response.IsSuccessStatusCode)
                {
                    Console.Error.WriteLine("Error HTTP " + (int)response.StatusCode + " al encofrar con " + url);
                    Console.Error.WriteLine(body);
                    return 1;
                }

                Console.WriteLine(body);
                return 0;
            }
        }

        private static async Task<int> PingAsync()
        {
            var baseUrl = CurrentUrl();
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

                Console.WriteLine("Conexion OK: " + CurrentLabel() + " - " + url);
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
            if (IsProduction())
                return false;
            var host = request != null && request.RequestUri != null ? request.RequestUri.Host : "";
            return string.Equals(host, "localhost", StringComparison.OrdinalIgnoreCase)
                || string.Equals(host, "127.0.0.1", StringComparison.OrdinalIgnoreCase)
                || string.Equals(host, "::1", StringComparison.OrdinalIgnoreCase);
        }

        internal static string CurrentUrl()
        {
            var env = Environment.GetEnvironmentVariable("TANDEM_MVC_BASE_URL");
            if (!string.IsNullOrWhiteSpace(env))
                return NormalizeBaseUrl(env);

            var saved = ReadTarget();
            if (string.Equals(saved, "production", StringComparison.OrdinalIgnoreCase)
                || string.Equals(saved, "produccion", StringComparison.OrdinalIgnoreCase)
                || string.Equals(saved, "prod", StringComparison.OrdinalIgnoreCase))
                return ProductionUrl;
            if (saved.StartsWith("http", StringComparison.OrdinalIgnoreCase))
                return NormalizeBaseUrl(saved);
            return LocalUrl;
        }

        internal static string CurrentLabel()
        {
            return IsProduction() ? "produccion (tdesing.net)" : "local (localhost:44384)";
        }

        internal static bool IsProduction()
        {
            return CurrentUrl().IndexOf("tdesing.net", StringComparison.OrdinalIgnoreCase) >= 0;
        }

        private static string NormalizeBaseUrl(string url)
        {
            if (string.IsNullOrWhiteSpace(url))
                return LocalUrl;
            url = url.Trim();
            if (url.StartsWith("https://www.tdesing.net", StringComparison.OrdinalIgnoreCase))
                url = ProductionUrl.TrimEnd('/') + url.Substring("https://www.tdesing.net".Length);
            return url.EndsWith("/", StringComparison.Ordinal) ? url : url + "/";
        }

        private static string ReadTarget()
        {
            try
            {
                var path = SettingsPath();
                if (!File.Exists(path))
                    return "local";
                using (var doc = JsonDocument.Parse(File.ReadAllText(path)))
                {
                    if (doc.RootElement.TryGetProperty("target", out var value))
                        return value.GetString() ?? "local";
                }
            }
            catch
            {
            }
            return "local";
        }

        private static void SetTarget(string target)
        {
            var path = SettingsPath();
            Directory.CreateDirectory(Path.GetDirectoryName(path));
            File.WriteAllText(path, "{\"target\":\"" + (target ?? "local") + "\"}");
        }

        private static string SettingsPath()
        {
            return Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                "Tandem",
                "FreecadPlugin",
                "mvc-target.json");
        }

        private static string FormatPluginError(Exception ex)
        {
            var root = ex;
            while (root.InnerException != null)
                root = root.InnerException;
            return "Error FreeCadPluging: " + root.Message;
        }
    }
}