using System;
using System.Net.Http;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace ZwcadPlugin
{
    /// <summary>
    /// Convierte excepciones de red/HTTP (incl. AggregateException de Task.Result)
    /// en mensajes legibles para el usuario en ZWCAD.
    /// </summary>
    public static class PluginExceptionHelper
    {
        private const string DefaultBaseUrl = "https://localhost:44384/";

        public static string Format(Exception ex, string baseUrl = null)
        {
            var root = Unwrap(ex);
            var hint = NormalizeBaseUrl(baseUrl ?? ResolveBaseUrlFromEnv());
            var rootMessage = root?.Message ?? "Error desconocido";

            if (IsConnectionRefused(root))
            {
                return "No se puede conectar al servidor MVC en " + hint +
                       " Comprueba que el proyecto Desing esté iniciado en Visual Studio (IIS Express). " +
                       "Variable opcional: TANDEM_MVC_BASE_URL.";
            }

            if (IsSslError(root))
            {
                return "Error de certificado SSL al conectar con " + hint +
                       " Inicia Desing con IIS Express en Visual Studio (certificado de desarrollo).";
            }

            if (IsTimeout(root))
            {
                return "Tiempo de espera agotado al contactar " + hint +
                       " Verifica que Desing esté en ejecución. El análisis de imagen puede tardar hasta 2 minutos.";
            }

            if (root is HttpRequestException)
            {
                return "Error de red al contactar el servidor MVC en " + hint + " Detalle: " + rootMessage;
            }

            if (rootMessage.IndexOf("Error del servidor", StringComparison.OrdinalIgnoreCase) >= 0 ||
                rootMessage.IndexOf("OPENAI_APIKEY", StringComparison.OrdinalIgnoreCase) >= 0 ||
                rootMessage.IndexOf("JSON", StringComparison.OrdinalIgnoreCase) >= 0 ||
                rootMessage.IndexOf("HTML", StringComparison.OrdinalIgnoreCase) >= 0)
            {
                return rootMessage;
            }

            if (string.Equals(rootMessage, "One or more errors occurred.", StringComparison.OrdinalIgnoreCase) ||
                string.Equals(ex?.Message, "One or more errors occurred.", StringComparison.OrdinalIgnoreCase))
            {
                return "Error de comunicación con el servidor MVC en " + hint +
                       " Comprueba que Desing esté iniciado en Visual Studio (IIS Express).";
            }

            return rootMessage;
        }

        public static Exception Wrap(string context, Exception ex, string baseUrl = null)
        {
            return new Exception(context + ": " + Format(ex, baseUrl), ex);
        }

        public static string ResolveBaseUrlFromEnv()
        {
            var url = Environment.GetEnvironmentVariable("TANDEM_MVC_BASE_URL");
            return NormalizeBaseUrl(string.IsNullOrWhiteSpace(url) ? DefaultBaseUrl : url);
        }

        public static string NormalizeBaseUrl(string url)
        {
            if (string.IsNullOrWhiteSpace(url))
                return DefaultBaseUrl;
            url = url.Trim();
            return url.EndsWith("/") ? url : url + "/";
        }

        private static Exception Unwrap(Exception ex)
        {
            if (ex == null) return null;

            while (ex is AggregateException aggregate && aggregate.InnerExceptions.Count == 1)
                ex = aggregate.InnerExceptions[0];

            if (ex is AggregateException flatAggregate)
                ex = flatAggregate.Flatten().InnerException ?? ex;

            while (ex.InnerException != null &&
                   (ex is AggregateException ||
                    string.IsNullOrWhiteSpace(ex.Message) ||
                    string.Equals(ex.Message, "One or more errors occurred.", StringComparison.OrdinalIgnoreCase)))
            {
                ex = ex.InnerException;
            }

            return ex;
        }

        private static bool IsConnectionRefused(Exception ex)
        {
            if (ex is SocketException) return true;

            var msg = (ex?.Message ?? string.Empty).ToLowerInvariant();
            return msg.Contains("connection refused") ||
                   msg.Contains("denegó expresamente") ||
                   msg.Contains("actively refused") ||
                   msg.Contains("no connection could be made") ||
                   msg.Contains("unable to connect") ||
                   msg.Contains("no se puede establecer una conexión") ||
                   msg.Contains("connection reset");
        }

        private static bool IsSslError(Exception ex)
        {
            var msg = (ex?.Message ?? string.Empty).ToLowerInvariant();
            return msg.Contains("ssl") ||
                   msg.Contains("certificate") ||
                   msg.Contains("certificado") ||
                   msg.Contains("trust relationship");
        }

        private static bool IsTimeout(Exception ex)
        {
            if (ex is TaskCanceledException || ex is TimeoutException)
                return true;

            var msg = (ex?.Message ?? string.Empty).ToLowerInvariant();
            return msg.Contains("timeout") || msg.Contains("timed out") || msg.Contains("tiempo de espera");
        }
    }
}
