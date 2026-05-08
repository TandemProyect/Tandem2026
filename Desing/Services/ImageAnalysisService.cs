using System;
using System.Collections.Generic;
using System.Configuration;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using Desing.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Desing.Services
{
    public class ImageAnalysisService
    {
        private static readonly HttpClient _httpClient = new HttpClient();
        private readonly string _apiKey;

        private const string PROMPT = @"
Analyze this technical drawing and extract all straight line segments that form the main structural layout.

For each thick line in the drawing, extract its center axis as a single line segment.

IMPORTANT: Look for dimension annotations written near walls:
- Thickness can be written as E/e (examples: E 0,30 | e=0.30 | espesor 0.30).
- Height can be written as H/h (examples: H 2,70 | h=2.30 | altura 2.70).
Extract both in METERS:
- espesorMuro
- alturaMuro
If one annotation is missing, set that value to null.

Return ONLY a JSON object with this exact format, no markdown, no extra text:
{
  ""escala"": ""brief scale note"",
  ""espesorMuro"": 0.30,
  ""alturaMuro"": 2.70,
  ""lineas"": [
    { ""inicioX"": 0.0, ""inicioY"": 0.0, ""finX"": 5.0, ""finY"": 0.0 },
    { ""inicioX"": 5.0, ""inicioY"": 0.0, ""finX"": 5.0, ""finY"": 4.0 }
  ]
}

Rules:
- Unit: METERS. If annotation shows 5.00, coordinate is 5.0.
- Accept comma or dot decimals in annotations (0,30 == 0.30).
- Origin (0,0): bottom-left corner of the drawing boundary.
- Extract only the main structural lines. Ignore annotations, numbers, text, furniture symbols.
- Each thick line = ONE center axis line. Do not extract both edges.
- Where lines meet at a corner, each line ends exactly at the intersection point.
- Use decimal point: 3.5 not 3,5.
- Lines that meet at corners should be perpendicular/orthogonal.
- Output ONLY the JSON. No explanations.
";

        public ImageAnalysisService()
        {
            _apiKey = ConfigurationManager.AppSettings["OPENAI_APIKEY"];
            if (string.IsNullOrEmpty(_apiKey) || _apiKey == "INSERTAR_API_KEY_AQUI")
                throw new InvalidOperationException("OPENAI_APIKEY no configurada en Web.config");
        }

        public async Task<(List<LineaDTO> Lineas, double? EspesorMuro, double? AlturaMuro)> AnalizarImagenAsync(byte[] imagenBytes, string mimeType = "image/jpeg")
        {
            string base64 = Convert.ToBase64String(imagenBytes);

            var requestBody = new
            {
                model = "gpt-4o",
                max_tokens = 2000,
                messages = new[]
                {
                    new
                    {
                        role = "user",
                        content = new object[]
                        {
                            new { type = "text", text = PROMPT },
                            new
                            {
                                type = "image_url",
                                image_url = new { url = $"data:{mimeType};base64,{base64}" }
                            }
                        }
                    }
                }
            };

            var json = JsonConvert.SerializeObject(requestBody);
            var request = new HttpRequestMessage(HttpMethod.Post, "https://api.openai.com/v1/chat/completions");
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _apiKey);
            request.Content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _httpClient.SendAsync(request);
            var responseBody = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
                throw new Exception($"OpenAI error {response.StatusCode}: {responseBody}");

            var parsed = JObject.Parse(responseBody);
            var content = parsed["choices"]?[0]?["message"]?["content"]?.ToString();

            System.Diagnostics.Debug.WriteLine($"[GPT-4o] Raw response: {content}");
            System.IO.File.WriteAllText(@"c:\temp\gpt4o_debug.json", content ?? "(null)");

            return ParseLineasDesdeRespuesta(content);
        }

        private (List<LineaDTO> Lineas, double? EspesorMuro, double? AlturaMuro) ParseLineasDesdeRespuesta(string content)
        {
            if (string.IsNullOrEmpty(content))
                throw new Exception("Respuesta vacía de OpenAI");

            int start = content.IndexOf('{');
            int end = content.LastIndexOf('}');
            if (start < 0 || end < 0)
                throw new Exception($"No se encontró JSON en la respuesta: {content}");

            var jsonStr = content.Substring(start, end - start + 1);
            var obj = JObject.Parse(jsonStr);
            var lineasJson = obj["lineas"] as JArray;

            if (lineasJson == null)
                throw new Exception("El JSON no contiene la propiedad 'lineas'");

            double? espesorMuro = ExtractMetricValue(obj["espesorMuro"]);
            double? alturaMuro = ExtractMetricValue(obj["alturaMuro"]);

            const double METROS_A_MM = 1000.0;
            var lineas = new List<LineaDTO>();
            foreach (var l in lineasJson)
            {
                lineas.Add(new LineaDTO
                {
                    Tipo     = "Line",
                    InicioX  = (l["inicioX"]?.Value<double>() ?? 0) * METROS_A_MM,
                    InicioY  = (l["inicioY"]?.Value<double>() ?? 0) * METROS_A_MM,
                    FinX     = (l["finX"]?.Value<double>()    ?? 0) * METROS_A_MM,
                    FinY     = (l["finY"]?.Value<double>()    ?? 0) * METROS_A_MM,
                    Vertices = new List<PuntoDTO>()
                });
            }
            return (lineas, espesorMuro, alturaMuro);
        }

        private static double? ExtractMetricValue(JToken token)
        {
            if (token == null || token.Type == JTokenType.Null)
                return null;

            if (token.Type == JTokenType.Float || token.Type == JTokenType.Integer)
                return token.Value<double>();

            var text = token.ToString();
            if (string.IsNullOrWhiteSpace(text))
                return null;

            // Permite textos como: "E 0,30", "h=2.70", "altura: 2,70 m"
            var match = System.Text.RegularExpressions.Regex.Match(
                text,
                @"[-+]?\d+(?:[.,]\d+)?",
                System.Text.RegularExpressions.RegexOptions.CultureInvariant);

            if (!match.Success)
                return null;

            var normalized = match.Value.Replace(',', '.');
            if (double.TryParse(
                normalized,
                System.Globalization.NumberStyles.Float,
                System.Globalization.CultureInfo.InvariantCulture,
                out var value))
            {
                return value;
            }

            return null;
        }
    }
}
