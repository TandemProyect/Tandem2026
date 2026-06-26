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
Analiza ÚNICAMENTE la imagen adjunta. CADA imagen es un boceto distinto: no reutilices medidas ni formas de peticiones anteriores.

FORMATO DE BOLETO (si la imagen lo sigue): una línea gruesa = perímetro EXTERIOR; cada tramo tiene UN número al lado (sin flechas ni líneas de cota); E=espesor y H=altura en un rincón.
   ORIENTACIÓN DE LA COTA (regla clave): el número se escribe PARALELO al muro que mide.
   - Número en horizontal → cota de tramo horizontal (E o W en recorrido).
   - Número en vertical → cota de tramo vertical (N o S en recorrido).
   Usa la orientación del texto para asignar cada cota al tramo correcto y al eje H/V.

1) Cotas globales:
   - espesorMuro: leer la anotación E=… de la línea de cota DEL ESPESOR (entre las dos líneas paralelas del muro). Ej: E=0,30 → 0.3. No confundir con cotas del perímetro (0,4 / 0,5).
   - alturaMuro: H/h en metros (uniforme), o null.

2) Geometría EXTERIOR en METROS. Origen (0,0) = esquina inferior izquierda exterior.
   Recorre el perímetro en SENTIDO HORARIO empezando por la BASE INFERIOR (primer tramo = E):
   - Un elemento de ""recorrido"" por cada arista, con la cota de ESE tramo (5,50 → 5.5).
   - Ignora líneas finas de cota, flechas y ticks; solo el trazo grueso exterior y los números.

   ESCALONES / MUESCAS (trazo exterior, sentido horario):
   - Borde INFERIOR: muesca hacia abajo → S, E, N (3 tramos).
   - Borde SUPERIOR: muesca hacia abajo (dentro del recinto) → al ir en W: S, W, S o secuencia equivalente.
   - Borde IZQUIERDO: muesca hacia dentro → E, N, W (o equivalente en orden horario).
   - Cada escalón = 3 tramos separados (vertical + horizontal + vertical).

   ""cotasEtiquetadas"" es OBLIGATORIO: lista TODAS las cotas del perímetro en orden horario desde base inferior izquierda.
   Cada entrada: {""len"":5.0,""eje"":""H""} o ""V"" según orientación del número en la imagen. No incluyas E=0,30 en esta lista.

Responde SOLO JSON (sin markdown):
{
  ""escala"": ""nota"",
  ""espesorMuro"": null,
  ""alturaMuro"": null,
  ""recorrido"": [{""dir"":""E"",""len"":0},{""dir"":""N"",""len"":0}],
  ""cotasEtiquetadas"": [{""len"":5.5,""eje"":""V""},{""len"":5,""eje"":""H""}],
  ""vertices"": [[x,y],...],
  ""lineas"": [{""inicioX"":0,""inicioY"":0,""finX"":0,""finY"":0}]
}

Obligatorio: ""cotasEtiquetadas"" (todas las cotas del perímetro, en orden, con eje H/V).
También ""recorrido"" con dir+len si puedes; debe CERRAR en (0,0).
También ""cotasVisibles"": [todos los números en metros].
No repitas el punto inicial al cerrar. No incluyas bloque ""cotas"". No simplifiques la forma.
";

        private const string RETRY_PROMPT = @"
La geometría anterior era incorrecta (salientes al revés o tramos omitidos). Revisa la imagen.

1) ""cotasEtiquetadas"": cada número en orden del perímetro con ""eje"":""H"" si el texto está horizontal, ""V"" si está vertical (paralelo al muro).
2) ""recorrido"": cada arista del trazo GRUESO EXTERIOR, horario desde esquina inferior izquierda, primer tramo E (base).
   La dirección E/W/N/S debe ser coherente con ""eje"" H/V de cada cota.
   Formato: [{""dir"":""E"",""len"":0},{""dir"":""S"",""len"":0},...]
   - Escalón en borde inferior hacia afuera: S, E/W, N (no al revés).
   - Escalón en borde superior hacia afuera: N, E/W, S (no al revés).
3) El recorrido debe CERRAR en (0,0): suma de dx = 0 y suma de dy = 0.
4) espesorMuro y alturaMuro si aparecen.

Responde SOLO JSON con ""cotasEtiquetadas"" obligatorio y ""recorrido"" que cierre (sin vertices ni lineas).
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
                System.Diagnostics.Debug.WriteLine($"[ImageAnalysis] Reintento GPT: {info?.MotivoRechazo}");
                content = await LlamarGptVisionAsync(imagenBytes, mimeType, RETRY_PROMPT, "intento-2");
                var resultado2 = ParseLineasDesdeRespuesta(content, out info);
                resultado = (
                    resultado2.Lineas,
                    resultado2.EspesorMuro ?? resultado.EspesorMuro,
                    resultado2.AlturaMuro ?? resultado.AlturaMuro);
            }

            string motivoFinal = null;
            bool perimetroOk = resultado.Lineas != null
                && SketchWallBuilder.ValidarPerimetroBoceto(resultado.Lineas, out motivoFinal);
            if (!perimetroOk)
            {
                throw new Exception(
                    "No se pudo reconstruir un perímetro cerrado válido desde la imagen. " +
                    (motivoFinal ?? info?.MotivoRechazo ?? "GPT leyó mal las cotas") +
                    ". Revise el boceto (números paralelos al muro, E=0,30) o c:\\temp\\gpt4o_debug.json");
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
            if (info == null || lineas == null || lineas.Count < 3)
                return true;

            return !info.PerimetroValido;
        }

        private sealed class ParseInfo
        {
            public string Fuente { get; set; }
            public bool UsoRecorrido { get; set; }
            public bool RecorridoCierra { get; set; }
            public bool PerimetroValido { get; set; }
            public string MotivoRechazo { get; set; }
            public List<double> CotasVisibles { get; set; }
        }

        private static bool AceptarLineas(List<LineaDTO> candidatas, string fuente, out List<LineaDTO> lineas, ParseInfo info)
        {
            lineas = null;
            if (candidatas == null || candidatas.Count == 0)
                return false;

            if (!SketchWallBuilder.ValidarPerimetroBoceto(candidatas, out var motivo))
            {
                info.MotivoRechazo = fuente + ": " + motivo;
                return false;
            }

            lineas = candidatas;
            info.Fuente = fuente;
            info.PerimetroValido = true;
            return true;
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

            double? espesorMuro = ExtractEspesorMuro(obj, content);
            double? alturaMuro = ExtractAlturaMuro(obj, content);
            info.CotasVisibles = ExtraerCotasVisibles(obj["cotasVisibles"]);

            List<LineaDTO> lineas = null;

            info.RecorridoCierra = SketchWallBuilder.RecorridoJsonCierra(obj["recorrido"]);
            bool cotasCoherentes = SketchWallBuilder.CotasPerimetroCoherentes(
                obj["cotasEtiquetadas"], obj["cotasVisibles"]);
            var rechazos = new List<string>();
            if (!cotasCoherentes)
                rechazos.Add("cotasEtiquetadas y cotasVisibles difieren en cantidad (se intentan otras rutas)");

            var dimsPerimetro = FiltrarCotasPerimetroParaImagen(obj["cotasVisibles"]);

            // Probar todas las rutas; la incoherencia de cotas no bloquea el resto.
            if (lineas == null
                && obj["cotasEtiquetadas"] != null
                && SketchWallBuilder.TryLineasDesdeCotasEtiquetadasJson(obj["cotasEtiquetadas"], out var desdeEtiquetas)
                && AceptarLineas(desdeEtiquetas, "cotasEtiquetadas", out lineas, info))
            {
                info.UsoRecorrido = true;
                info.RecorridoCierra = true;
            }

            if (lineas == null
                && obj["recorrido"] != null
                && info.RecorridoCierra
                && SketchWallBuilder.TryLineasDesdeRecorridoJson(obj["recorrido"], out var desdeRecorrido)
                && AceptarLineas(desdeRecorrido, "recorrido", out lineas, info))
            {
                info.UsoRecorrido = true;
            }

            bool recorridoCompletadoOk = false;
            if (lineas == null
                && obj["recorrido"] != null
                && SketchWallBuilder.TryLineasDesdeRecorridoCompletadoJson(obj["recorrido"], out var desdeRecCompleto)
                && AceptarLineas(desdeRecCompleto, "recorrido+cierre", out lineas, info))
            {
                info.UsoRecorrido = true;
                info.RecorridoCierra = true;
                recorridoCompletadoOk = true;
            }

            if (lineas == null && obj["recorrido"] != null && !info.RecorridoCierra && !recorridoCompletadoOk)
            {
                rechazos.Add("recorrido: no cierra en el origen");
            }

            if (lineas == null
                && obj["recorrido"] != null && obj["cotasEtiquetadas"] != null
                && SketchWallBuilder.TryLineasDesdeRecorridoConCotasEtiquetadas(
                    obj["recorrido"], obj["cotasEtiquetadas"], out var desdeMixto)
                && AceptarLineas(desdeMixto, "recorrido+cotasEtiquetadas", out lineas, info))
            {
                info.UsoRecorrido = true;
                info.RecorridoCierra = true;
            }

            if (lineas == null
                && obj["recorrido"] != null
                && dimsPerimetro != null
                && SketchWallBuilder.TryLineasDesdeRecorridoConLongitudes(
                    obj["recorrido"], dimsPerimetro, out var desdeRecLong)
                && AceptarLineas(desdeRecLong, "recorrido+cotasVisibles", out lineas, info))
            {
                info.UsoRecorrido = true;
                info.RecorridoCierra = true;
            }

            if (lineas == null
                && obj["cotasVisibles"] != null
                && SketchWallBuilder.TryLineasDesdeCotasVisiblesPerimetro(obj["cotasVisibles"], out var desdeCotas)
                && AceptarLineas(desdeCotas, "cotasVisibles", out lineas, info))
            {
                info.UsoRecorrido = true;
                info.RecorridoCierra = true;
            }

            if (lineas == null || lineas.Count == 0)
            {
                if (!string.IsNullOrEmpty(info.MotivoRechazo))
                    rechazos.Add(info.MotivoRechazo);
                info.MotivoRechazo = rechazos.Count > 0
                    ? string.Join("; ", rechazos.Distinct())
                    : "Ninguna estrategia produjo un perímetro cerrado válido";
                throw new Exception(
                    "GPT no devolvió un perímetro válido. " + info.MotivoRechazo +
                    ". Revise c:\\temp\\gpt4o_debug.json");
            }

            System.Diagnostics.Debug.WriteLine(
                $"[ImageAnalysis] Imagen actual → {info.Fuente}, {lineas.Count} tramos (validado).");
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

        private static double? ExtractEspesorMuro(JObject obj, string rawContent)
        {
            var token = obj["espesorMuro"];
            if (token != null && token.Type != JTokenType.Null)
            {
                double? directo = token.Type == JTokenType.Float || token.Type == JTokenType.Integer
                    ? token.Value<double>()
                    : ParseNumeroBoceto(token.ToString());
                if (EsEspesorRazonable(directo))
                    return directo;
            }

            if (!string.IsNullOrEmpty(rawContent))
            {
                var norm = NormalizarTextoBoceto(rawContent);

                var jsonMatch = System.Text.RegularExpressions.Regex.Match(
                    norm,
                    @"""espesorMuro""\s*:\s*([\d][\d.,]*)",
                    System.Text.RegularExpressions.RegexOptions.CultureInvariant | System.Text.RegularExpressions.RegexOptions.IgnoreCase);
                if (jsonMatch.Success)
                {
                    var parsed = ParseDecimalInvariant(jsonMatch.Groups[1].Value);
                    if (EsEspesorRazonable(parsed))
                        return parsed;
                }

                var eMatch = System.Text.RegularExpressions.Regex.Match(
                    norm,
                    @"\b[Ee]\s*[=:]\s*([\d][\d.,]*)",
                    System.Text.RegularExpressions.RegexOptions.CultureInvariant);
                if (eMatch.Success)
                {
                    var parsed = ParseNumeroBoceto(eMatch.Groups[1].Value);
                    if (EsEspesorRazonable(parsed))
                        return parsed;
                }
            }

            var cotas = ExtraerCotasVisibles(obj["cotasVisibles"]);
            foreach (var c in cotas.Where(c => EsEspesorRazonable(c)).OrderBy(v => v))
                return c;

            return null;
        }

        private static double? ExtractAlturaMuro(JObject obj, string rawContent)
        {
            var fromField = ExtractMetricValue(obj["alturaMuro"]);
            if (fromField.HasValue && fromField.Value >= 1.5 && fromField.Value <= 6.0)
                return fromField;

            if (!string.IsNullOrEmpty(rawContent))
            {
                var norm = NormalizarTextoBoceto(rawContent);
                var hMatch = System.Text.RegularExpressions.Regex.Match(
                    norm,
                    @"\b[Hh]\s*[=:]\s*([\d][\d.,]*)",
                    System.Text.RegularExpressions.RegexOptions.CultureInvariant);
                if (hMatch.Success)
                {
                    var parsed = ParseNumeroBoceto(hMatch.Groups[1].Value);
                    if (parsed.HasValue && parsed.Value >= 1.5 && parsed.Value <= 6.0)
                        return parsed;
                }
            }

            return null;
        }

        private static bool EsEspesorRazonable(double? metros)
        {
            return metros.HasValue && metros.Value >= 0.08 && metros.Value <= 0.55;
        }

        private static string NormalizarTextoBoceto(string text)
        {
            if (string.IsNullOrEmpty(text))
                return text;
            return text
                .Replace('ø', '0').Replace('Ø', '0').Replace('∅', '0')
                .Replace('º', '0').Replace('°', '0');
        }

        private static double? ParseNumeroBoceto(string fragment)
        {
            if (string.IsNullOrWhiteSpace(fragment))
                return null;

            fragment = NormalizarTextoBoceto(fragment.Trim());
            var match = System.Text.RegularExpressions.Regex.Match(
                fragment,
                @"\d+(?:[.,]\d+)?",
                System.Text.RegularExpressions.RegexOptions.CultureInvariant);
            if (!match.Success)
                return null;

            return ParseDecimalInvariant(match.Value);
        }

        private static double? ParseDecimalInvariant(string value)
        {
            var normalized = value.Replace(',', '.');
            if (double.TryParse(
                normalized,
                System.Globalization.NumberStyles.Float,
                System.Globalization.CultureInfo.InvariantCulture,
                out var parsed))
            {
                return parsed;
            }

            return null;
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

            text = NormalizarTextoBoceto(text);
            return ParseNumeroBoceto(text);
        }

        private static List<double> FiltrarCotasPerimetroParaImagen(JToken cotasToken)
        {
            var lista = ExtraerCotasVisibles(cotasToken);
            if (lista == null || lista.Count == 0)
                return null;

            var filtrada = new List<double>();
            foreach (var c in lista)
            {
                if (c >= 0.12 && c <= 0.55)
                    continue;
                filtrada.Add(c);
            }

            return filtrada.Count >= 4 ? filtrada : null;
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
