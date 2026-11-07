using System;
using System.Collections.Generic;
using System.Globalization;
using System.Net.Http;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace OpenCadStudioPluging
{
    internal static class MvcWall2dStepClient
    {
        public static async Task<List<Wall2dStepLineDto>> CalculateStepAsync(
            List<Wall2dPointDto> points,
            double thickness,
            bool reserveEndForNextStep,
            List<Wall2dAxisSegmentDto> existingAxisSegments)
        {
            if (points == null || points.Count < 2)
                throw new InvalidOperationException("Wall2D step requiere al menos 2 puntos.");

            var baseUrl = MvcServerSettings.CurrentUrl();
            var url = baseUrl + "DesignToolsAutocad/PluginWall2dStep";
            var body = new Wall2dStepRequestDto
            {
                Points = points,
                Thickness = thickness,
                ReserveEndForNextStep = reserveEndForNextStep,
                ExistingAxisSegments = existingAxisSegments
            };

            var json = JsonSerializer.Serialize(body);
            using (var client = new HttpClient(CreateHandler()) { Timeout = TimeSpan.FromSeconds(20) })
            using (var content = new StringContent(json, Encoding.UTF8, "application/json"))
            {
                var response = await client.PostAsync(url, content).ConfigureAwait(false);
                var payload = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
                if (!response.IsSuccessStatusCode)
                {
                    throw new InvalidOperationException(
                        "Error HTTP " + (int)response.StatusCode + " al calcular wall2d step: " + payload);
                }

                var parsed = JsonSerializer.Deserialize<ApiResponseDto<Wall2dStepResponseDto>>(
                    payload,
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                if (parsed == null)
                    throw new InvalidOperationException("Respuesta vacía de PluginWall2dStep.");
                if (!parsed.Exito)
                    throw new InvalidOperationException(parsed.Mensaje ?? "PluginWall2dStep devolvió Exito=false.");
                if (parsed.Datos == null || parsed.Datos.Lines == null)
                    throw new InvalidOperationException("PluginWall2dStep no devolvió líneas.");

                return parsed.Datos.Lines;
            }
        }

        public static List<Wall2dPointDto> ParsePointsArg(string raw)
        {
            var points = new List<Wall2dPointDto>();
            if (string.IsNullOrWhiteSpace(raw))
                return points;

            var chunks = raw.Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries);
            foreach (var chunk in chunks)
            {
                var parts = chunk.Split(',');
                if (parts.Length < 2)
                    continue;

                if (!double.TryParse(parts[0], NumberStyles.Float, CultureInfo.InvariantCulture, out var x))
                    continue;
                if (!double.TryParse(parts[1], NumberStyles.Float, CultureInfo.InvariantCulture, out var y))
                    continue;

                var z = 0.0;
                if (parts.Length >= 3)
                    double.TryParse(parts[2], NumberStyles.Float, CultureInfo.InvariantCulture, out z);

                points.Add(new Wall2dPointDto { X = x, Y = y, Z = z });
            }

            return points;
        }

        public static List<Wall2dAxisSegmentDto> ParseAxisSegmentsArg(string raw)
        {
            var segments = new List<Wall2dAxisSegmentDto>();
            if (string.IsNullOrWhiteSpace(raw))
                return segments;

            var chunks = raw.Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries);
            foreach (var chunk in chunks)
            {
                var parts = chunk.Split(',');
                if (parts.Length < 6)
                    continue;

                if (!double.TryParse(parts[0], NumberStyles.Float, CultureInfo.InvariantCulture, out var x1))
                    continue;
                if (!double.TryParse(parts[1], NumberStyles.Float, CultureInfo.InvariantCulture, out var y1))
                    continue;
                if (!double.TryParse(parts[2], NumberStyles.Float, CultureInfo.InvariantCulture, out var z1))
                    continue;
                if (!double.TryParse(parts[3], NumberStyles.Float, CultureInfo.InvariantCulture, out var x2))
                    continue;
                if (!double.TryParse(parts[4], NumberStyles.Float, CultureInfo.InvariantCulture, out var y2))
                    continue;
                if (!double.TryParse(parts[5], NumberStyles.Float, CultureInfo.InvariantCulture, out var z2))
                    continue;

                segments.Add(new Wall2dAxisSegmentDto
                {
                    Start = new Wall2dPointDto { X = x1, Y = y1, Z = z1 },
                    End = new Wall2dPointDto { X = x2, Y = y2, Z = z2 }
                });
            }

            return segments;
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

    internal sealed class Wall2dPointDto
    {
        public double X { get; set; }
        public double Y { get; set; }
        public double Z { get; set; }
    }

    internal sealed class Wall2dStepRequestDto
    {
        public List<Wall2dPointDto> Points { get; set; }
        public double Thickness { get; set; }
        public bool ReserveEndForNextStep { get; set; }
        public List<Wall2dAxisSegmentDto> ExistingAxisSegments { get; set; }
    }

    internal sealed class Wall2dAxisSegmentDto
    {
        public Wall2dPointDto Start { get; set; }
        public Wall2dPointDto End { get; set; }
    }

    internal sealed class Wall2dStepLineDto
    {
        public string Role { get; set; }
        public Wall2dPointDto Start { get; set; }
        public Wall2dPointDto End { get; set; }
    }

    internal sealed class Wall2dStepResponseDto
    {
        public List<Wall2dStepLineDto> Lines { get; set; }
        public string Message { get; set; }
    }

    internal sealed class ApiResponseDto<T>
    {
        public bool Exito { get; set; }
        public string Mensaje { get; set; }
        public T Datos { get; set; }
    }
}
