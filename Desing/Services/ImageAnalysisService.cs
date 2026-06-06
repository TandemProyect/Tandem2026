using System;
using System.Collections.Generic;
using System.Linq;
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
Analiza ÚNICAMENTE la imagen adjunta (cada imagen es distinta; no uses ejemplos de otros bocetos).

Planta de muros manuscrita (doble línea = espesor). Extrae la geometría EXTERIOR del trazo grueso.

1) Cotas globales:
   - espesorMuro: E/e en metros (uniforme).
   - alturaMuro: H/h en metros (uniforme), o null.

2) Geometría en METROS. Origen (0,0) = esquina inferior izquierda del perímetro exterior.
   Método preferido — recorre el perímetro en sentido horario empezando por la base inferior:
   - Lee CADA número escrito junto a cada arista (5,50 → 5.5; 8,30 → 8.3).
   - Cada arista = un elemento de ""recorrido"" con la cota exacta de esa arista.
   - Salientes: sube (N), tramo horizontal (E/W), baja (S) como tramos separados.
   - Comprueba: la suma de tramos horizontales superiores debe coincidir con el ancho inferior.
   - Ignora flechas, ticks y líneas finas de cota.

Responde SOLO JSON (sin markdown):
{
  ""escala"": ""nota"",
  ""espesorMuro"": null,
  ""alturaMuro"": null,
  ""recorrido"": [{""dir"":""E"",""len"":0},{""dir"":""N"",""len"":0}],
  ""vertices"": [[x,y],...],
  ""lineas"": [{""inicioX"":0,""inicioY"":0,""finX"":0,""finY"":0}]
}

Obligatorio: ""recorrido"" con TODAS las aristas en orden (una entrada por cada cota del dibujo).
También incluye ""cotasVisibles"": [lista de todos los números que leas en la imagen, en metros].
No repitas el punto inicial al cerrar. No incluyas bloque ""cotas"". No simplifiques la forma.
";

        private const string RETRY_PROMPT = @"
Revisa de nuevo la imagen adjunta. La geometría anterior era incorrecta.

1) Escribe en ""cotasVisibles"" TODOS los números del boceto (ej: 5.5, 5, 2, 3, 2, 7, 8.3, 7, 0.3, 3).
2) Construye ""recorrido"": cada arista del perímetro EXTERIOR, sentido horario desde esquina inferior izquierda.
   Formato: [{""dir"":""E"",""len"":23.3},{""dir"":""N"",""len"":7},...]
   - E/W = horizontal, N/S = vertical.
   - Saliente: N (sube), E o W (techo), S (baja) = 3 tramos separados.
3) La suma de tramos horizontales del techo = ancho de la base inferior.
4) espesorMuro (E) y alturaMuro (H) si aparecen.

Responde SOLO JSON con ""recorrido"" obligatorio (no uses vertices ni lineas).
";

        public ImageAnalysisService()
        {
            // Seguridad: priorizar variable de entorno para evitar secretos en archivos versionados.
            _apiKey = Environment.GetEnvironmentVariable("OPENAI_APIKEY");
            if (string.IsNullOrWhiteSpace(_apiKey))
                _apiKey = ConfigurationManager.AppSettings["OPENAI_APIKEY"];

            if (IsPlaceholderOrMissingApiKey(_apiKey))
            {
                throw new InvalidOperationException(
                    "OPENAI_APIKEY no configurada. Opciones: (1) copiar Web.GoogleMaps.config.example → Web.GoogleMaps.config " +
                    "y poner su clave sk-… en OPENAI_APIKEY; (2) variable de entorno OPENAI_APIKEY antes de iniciar IIS Express; " +
                    "(3) Web.config local sin commitear. https://platform.openai.com/account/api-keys");
            }
        }

        private static bool IsPlaceholderOrMissingApiKey(string key)
        {
            if (string.IsNullOrWhiteSpace(key)) return true;
            var k = key.Trim();
            if (string.Equals(k, "INSERTAR_API_KEY_AQUI", StringComparison.OrdinalIgnoreCase)) return true;
            if (k.StartsWith("REPLACE_", StringComparison.OrdinalIgnoreCase)) return true;
            if (k.IndexOf("LOCAL_SECRET", StringComparison.OrdinalIgnoreCase) >= 0) return true;
            return false;
        }

        public async Task<(List<LineaDTO> Lineas, double? EspesorMuro, double? AlturaMuro)> AnalizarImagenAsync(byte[] imagenBytes, string mimeType = "image/jpeg")
        {
            var content = await LlamarGptVisionAsync(imagenBytes, mimeType, PROMPT, "intento-1");
            var resultado = ParseLineasDesdeRespuesta(content, out var info);

            if (DebeReintentar(info, resultado.Lineas))
            {
                System.Diagnostics.Debug.WriteLine($"[ImageAnalysis] Reintento GPT: {info}");
                content = await LlamarGptVisionAsync(imagenBytes, mimeType, RETRY_PROMPT, "intento-2");
                resultado = ParseLineasDesdeRespuesta(content, out info);
            }

            return resultado;
        }

        private async Task<string> LlamarGptVisionAsync(byte[] imagenBytes, string mimeType, string prompt, string etiqueta)
        {
            string base64 = Convert.ToBase64String(imagenBytes);

            var requestBody = new
            {
                model = "gpt-4o",
                max_tokens = 3000,
                messages = new[]
                {
                    new
                    {
                        role = "user",
                        content = new object[]
                        {
                            new { type = "text", text = prompt },
                            new
                            {
                                type = "image_url",
                                image_url = new
                                {
                                    url = $"data:{mimeType};base64,{base64}",
                                    detail = "high"
                                }
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
            {
                if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                {
                    throw new InvalidOperationException(
                        "Clave de OpenAI inválida o caducada. Revise OPENAI_APIKEY en Web.config o variable de entorno " +
                        "(https://platform.openai.com/account/api-keys).");
                }
                throw new Exception($"OpenAI error {response.StatusCode}");
            }

            var parsed = JObject.Parse(responseBody);
            var content = parsed["choices"]?[0]?["message"]?["content"]?.ToString();

            System.Diagnostics.Debug.WriteLine($"[GPT-4o] {etiqueta}: {content}");
            System.IO.File.WriteAllText(
                @"c:\temp\gpt4o_debug.json",
                "// " + DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss") + " UTC — " + etiqueta + "\n"
                + (content ?? "(null)"));

            return content;
        }

        private static bool DebeReintentar(ParseInfo info, List<LineaDTO> lineas)
        {
            if (info == null)
                return true;

            if (!info.UsoRecorrido)
                return true;

            if (lineas == null || lineas.Count < 3)
                return true;

            if (!info.RecorridoCierra)
                return true;

            SketchWallBuilder.ObtenerBoundsPublico(lineas, out double minX, out _, out double maxX, out _);
            double anchoM = (maxX - minX) / 1000.0;

            if (info.CotasVisibles != null && info.CotasVisibles.Count >= 3)
            {
                var horizontales = info.CotasVisibles.Where(c => c >= 3.0 && c <= 50.0).OrderByDescending(c => c).ToList();
                if (horizontales.Count >= 2)
                {
                    double sumaTop = horizontales.Take(4).Sum();
                    if (sumaTop > anchoM * 1.25)
                        return true;
                }
            }

            return false;
        }

        private sealed class ParseInfo
        {
            public bool UsoRecorrido { get; set; }
            public bool RecorridoCierra { get; set; }
            public List<double> CotasVisibles { get; set; }
        }

        private (List<LineaDTO> Lineas, double? EspesorMuro, double? AlturaMuro) ParseLineasDesdeRespuesta(string content)
        {
            return ParseLineasDesdeRespuesta(content, out _);
        }

        private (List<LineaDTO> Lineas, double? EspesorMuro, double? AlturaMuro) ParseLineasDesdeRespuesta(string content, out ParseInfo info)
        {
            info = new ParseInfo();

            if (string.IsNullOrEmpty(content))
                throw new Exception("Respuesta vacía de OpenAI");

            content = content.Trim();
            if (content.StartsWith("```", StringComparison.Ordinal))
            {
                var nl = content.IndexOf('\n');
                if (nl >= 0)
                    content = content.Substring(nl + 1);
                var fence = content.LastIndexOf("```", StringComparison.Ordinal);
                if (fence >= 0)
                    content = content.Substring(0, fence);
                content = content.Trim();
            }

            int start = content.IndexOf('{');
            int end = content.LastIndexOf('}');
            if (start < 0 || end < 0)
                throw new Exception($"No se encontró JSON en la respuesta: {content}");

            var jsonStr = content.Substring(start, end - start + 1);
            var obj = JObject.Parse(jsonStr);

            double? espesorMuro = ExtractMetricValue(obj["espesorMuro"]);
            double? alturaMuro = ExtractMetricValue(obj["alturaMuro"]);
            info.CotasVisibles = ExtraerCotasVisibles(obj["cotasVisibles"]);

            List<LineaDTO> lineas = null;
            string fuente = null;

            info.RecorridoCierra = SketchWallBuilder.RecorridoJsonCierra(obj["recorrido"]);

            // Prioridad: recorrido GPT válido → reconstrucción desde cotasVisibles → lineas → vertices.
            if (obj["recorrido"] != null
                && info.RecorridoCierra
                && SketchWallBuilder.TryLineasDesdeRecorridoJson(obj["recorrido"], out var desdeRecorrido)
                && desdeRecorrido != null && desdeRecorrido.Count > 0)
            {
                lineas = desdeRecorrido;
                fuente = "recorrido";
                info.UsoRecorrido = true;
            }
            else if (obj["cotasVisibles"] != null
                && SketchWallBuilder.TryLineasDesdeCotasVisiblesOrdenadas(obj["cotasVisibles"], out var desdeCotas)
                && desdeCotas != null && desdeCotas.Count > 0)
            {
                lineas = desdeCotas;
                fuente = "cotasVisibles";
                info.UsoRecorrido = true;
                info.RecorridoCierra = true;
            }
            else if (obj["recorrido"] != null
                && SketchWallBuilder.TryLineasDesdeRecorridoJson(obj["recorrido"], out desdeRecorrido)
                && desdeRecorrido != null && desdeRecorrido.Count > 0)
            {
                lineas = desdeRecorrido;
                fuente = "recorrido-incompleto";
                info.UsoRecorrido = true;
            }
            else
            {
                var desdeLineas = ParseLineasArray(obj["lineas"] as JArray);
                List<LineaDTO> desdeVertices = null;
                if (SketchWallBuilder.TryLineasDesdeVerticesJson(obj["vertices"], out var verts))
                    desdeVertices = verts;

                if (desdeLineas != null && desdeLineas.Count > 0)
                {
                    lineas = desdeLineas;
                    fuente = "lineas";
                }
                else if (desdeVertices != null && desdeVertices.Count > 0)
                {
                    lineas = desdeVertices;
                    fuente = "vertices";
                }
            }

            if (lineas == null || lineas.Count == 0)
            {
                throw new Exception(
                    "GPT no devolvió geometría utilizable (vertices o lineas). " +
                    "Reintente con un boceto más claro o revise c:\\temp\\gpt4o_debug.json");
            }

            System.Diagnostics.Debug.WriteLine(
                $"[ImageAnalysis] Imagen actual → {fuente}, {lineas.Count} tramos (sin plantilla cotas).");
            return (lineas, espesorMuro, alturaMuro);
        }

        private static List<LineaDTO> ParseLineasArray(JArray lineasJson)
        {
            if (lineasJson == null)
                return new List<LineaDTO>();

            const double METROS_A_MM = 1000.0;
            var lineas = new List<LineaDTO>();
            foreach (var l in lineasJson)
            {
                lineas.Add(new LineaDTO
                {
                    Tipo = "Line",
                    InicioX = (l["inicioX"]?.Value<double>() ?? 0) * METROS_A_MM,
                    InicioY = (l["inicioY"]?.Value<double>() ?? 0) * METROS_A_MM,
                    FinX = (l["finX"]?.Value<double>() ?? 0) * METROS_A_MM,
                    FinY = (l["finY"]?.Value<double>() ?? 0) * METROS_A_MM,
                    Vertices = new List<PuntoDTO>()
                });
            }
            return lineas;
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

        private static List<double> ExtraerCotasVisibles(JToken token)
        {
            var lista = new List<double>();
            if (token is JArray arr)
            {
                foreach (var item in arr)
                {
                    var v = ExtractMetricValue(item);
                    if (v.HasValue && v.Value > 0)
                        lista.Add(v.Value);
                }
            }
            return lista;
        }
    }
}
