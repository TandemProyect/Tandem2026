using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Configuration;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace Desing.Services
{
    /// <summary>Lo leído en un boceto y el marco de píxeles en que vienen sus coordenadas.</summary>
    public sealed class SketchImageAnalysis
    {
        public SketchReading Lectura { get; set; }

        /// <summary>Tamaño de la imagen enviada al modelo (ya girada según EXIF): marco de las coordenadas.</summary>
        public int ImagenAncho { get; set; }

        public int ImagenAlto { get; set; }

        public string Modelo { get; set; }
    }

    /// <summary>
    /// Lee bocetos de planta con un modelo de visión de OpenAI. El modelo solo describe lo que ve (contorno exterior
    /// del muro, cotas con sus referencias, E y h); las medidas las resuelve <see cref="SketchPlanSolver"/>.
    /// Modelo configurable en appSettings <c>OPENAI_SKETCH_MODEL</c> (gpt-4o lee mal la geometría de los bocetos).
    /// </summary>
    public class ImageAnalysisService
    {
        private const string ModeloPorDefecto = "gpt-6.2-sol";
        private const int LadoMaximoPx = 2048;
        private const string DebugDir = @"c:\temp";
        private const string DebugFile = @"c:\temp\boceto_gpt_debug.json";

        private static readonly HttpClient _httpClient = new HttpClient { Timeout = TimeSpan.FromMinutes(3) };

        private const string PROMPT = @"Eres un LECTOR de bocetos de planta dibujados a mano (foto de un papel). NO calcules, NO corrijas y NO cierres nada: describe solo lo que ves.
La imagen mide {W}x{H} píxeles. Usa coordenadas de PÍXEL de esta imagen: x hacia la derecha, y hacia abajo, origen arriba a la izquierda.

Cómo se dibuja el boceto:
- MURO: DOBLE línea paralela (cara exterior y cara interior). La separación entre ambas es el espesor.
- COTA: línea fina SENCILLA con marcas en los extremos (/, x, flechas o puntos) y líneas de referencia que llegan hasta el muro. Lleva un número en metros con coma decimal (7,00 = 7.0). Puede haber cotas TOTALES y cotas PARCIALES encadenadas en la misma línea.
- ANOTACIONES: ""E=0,30"" es el espesor del muro en metros; ""h=3,00"" o ""H=3,00"" es la altura del muro en metros.

Devuelve SOLO este JSON (números con punto decimal):
{
  ""contornoExterior"": [[x,y], ...],
  ""cotas"": [ {""valor"": 7.0, ""eje"": ""H"", ""p1"": [x,y], ""p2"": [x,y], ""v1"": 0, ""v2"": 1} ],
  ""espesor"": 0.3,
  ""altura"": 3.0,
  ""textos"": [""...""]
}

Reglas:
1. ""contornoExterior"": esquinas de la CARA EXTERIOR del muro (la línea más de fuera de la doble línea), una por cada esquina, incluidas las de entrantes o muescas. Empieza en la esquina superior izquierda y recorre en sentido horario. No repitas el primer punto al final. Si no ves muros, devuelve [].
2. ""cotas"": una entrada por cada número de cota (no incluyas E ni h).
   - ""eje"": ""H"" si la cota mide una distancia horizontal (línea de cota horizontal); ""V"" si mide una distancia vertical.
   - ""p1"" y ""p2"": píxeles de los dos extremos de la línea de cota (donde están las marcas). Para ""H"", p1 es el extremo izquierdo; para ""V"", p1 es el extremo superior.
   - ""v1"" y ""v2"": índice (base 0) del vértice de ""contornoExterior"" al que llega la línea de referencia de p1 y de p2; null si no llega a ninguno.
3. ""espesor"" y ""altura"" en metros, o null si no aparecen.
4. ""textos"": todos los textos que leas, tal cual.";

        private readonly string _apiKey;
        private readonly string _modelo;

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

            _modelo = ConfigurationManager.AppSettings["OPENAI_SKETCH_MODEL"];
            if (string.IsNullOrWhiteSpace(_modelo))
                _modelo = ModeloPorDefecto;
            _modelo = _modelo.Trim();
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

        /// <summary>Envía el boceto al modelo de visión y devuelve lo leído (sin resolver medidas).</summary>
        public async Task<SketchImageAnalysis> AnalizarBocetoAsync(byte[] imagenBytes)
        {
            var jpeg = NormalizarImagen(imagenBytes, out int ancho, out int alto);
            var prompt = PROMPT
                .Replace("{W}", ancho.ToString(CultureInfo.InvariantCulture))
                .Replace("{H}", alto.ToString(CultureInfo.InvariantCulture));
            var contenido = await LlamarModeloAsync(jpeg, prompt).ConfigureAwait(false);
            return new SketchImageAnalysis
            {
                Lectura = SketchPlanSolver.LeerRespuestaModelo(contenido),
                ImagenAncho = ancho,
                ImagenAlto = alto,
                Modelo = _modelo
            };
        }

        private async Task<string> LlamarModeloAsync(byte[] jpeg, string prompt)
        {
            var mensaje = new JObject
            {
                ["role"] = "user",
                ["content"] = new JArray
                {
                    new JObject { ["type"] = "text", ["text"] = prompt },
                    new JObject
                    {
                        ["type"] = "image_url",
                        ["image_url"] = new JObject
                        {
                            ["url"] = "data:image/jpeg;base64," + Convert.ToBase64String(jpeg),
                            ["detail"] = "high"
                        }
                    }
                }
            };
            var body = new JObject
            {
                ["model"] = _modelo,
                ["response_format"] = new JObject { ["type"] = "json_object" },
                ["messages"] = new JArray { mensaje }
            };
            // gpt-4.x usa max_tokens/temperature; los modelos de razonamiento (gpt-5+, o*) max_completion_tokens.
            if (_modelo.StartsWith("gpt-4", StringComparison.OrdinalIgnoreCase))
            {
                body["max_tokens"] = 3000;
                body["temperature"] = 0;
            }
            else
            {
                body["max_completion_tokens"] = 16000;
            }

            var request = new HttpRequestMessage(HttpMethod.Post, "https://api.openai.com/v1/chat/completions")
            {
                Content = new StringContent(body.ToString(Formatting.None), Encoding.UTF8, "application/json")
            };
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _apiKey);

            HttpResponseMessage response;
            try
            {
                response = await _httpClient.SendAsync(request).ConfigureAwait(false);
            }
            catch (TaskCanceledException)
            {
                throw new TimeoutException(
                    "La IA no ha respondido en " + (int)_httpClient.Timeout.TotalMinutes +
                    " minutos. Vuelve a intentarlo en un momento.");
            }

            var responseBody = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
            if (!response.IsSuccessStatusCode)
                throw new InvalidOperationException(MensajeErrorOpenAi(response.StatusCode, responseBody));

            var content = JObject.Parse(responseBody)["choices"]?[0]?["message"]?["content"]?.ToString();
            GuardarDebug(content);
            return content;
        }

        private string MensajeErrorOpenAi(HttpStatusCode status, string responseBody)
        {
            if (status == HttpStatusCode.Unauthorized)
            {
                return "Clave de OpenAI inválida o caducada. Revise OPENAI_APIKEY en Web.config o variable de entorno " +
                    "(https://platform.openai.com/account/api-keys).";
            }

            string code = null;
            string detalle = null;
            try
            {
                var error = JObject.Parse(responseBody)["error"];
                code = error?["code"]?.ToString();
                detalle = error?["message"]?.ToString();
            }
            catch (JsonException)
            {
                // Cuerpo no JSON: basta con el código HTTP.
            }

            if (string.Equals(code, "model_not_found", StringComparison.OrdinalIgnoreCase))
            {
                return "El modelo «" + _modelo + "» no está disponible con esta clave de OpenAI: " +
                    "indica otro en OPENAI_SKETCH_MODEL (appSettings).";
            }

            return "OpenAI error " + (int)status + (string.IsNullOrWhiteSpace(detalle) ? "" : ": " + detalle);
        }

        private void GuardarDebug(string content)
        {
            try
            {
                if (!Directory.Exists(DebugDir))
                    return;
                File.WriteAllText(
                    DebugFile,
                    "// " + DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture) + " UTC — " +
                    _modelo + "\n" + (content ?? "(null)"));
            }
            catch (IOException)
            {
                // Depuración opcional: nunca debe romper la importación.
            }
            catch (UnauthorizedAccessException)
            {
            }
        }

        /// <summary>
        /// Gira la foto según su EXIF (móviles), limita el lado mayor y la pasa a JPEG: el modelo y el visor
        /// trabajan así sobre el mismo marco de píxeles.
        /// </summary>
        private static byte[] NormalizarImagen(byte[] bytes, out int ancho, out int alto)
        {
            using (var entrada = new MemoryStream(bytes))
            using (var original = Image.FromStream(entrada, true, true))
            {
                var giro = GiroExif(original);
                if (giro != RotateFlipType.RotateNoneFlipNone)
                    original.RotateFlip(giro);

                double escala = Math.Min(1.0, (double)LadoMaximoPx / Math.Max(original.Width, original.Height));
                ancho = Math.Max(1, (int)Math.Round(original.Width * escala));
                alto = Math.Max(1, (int)Math.Round(original.Height * escala));
                using (var lienzo = new Bitmap(ancho, alto, PixelFormat.Format24bppRgb))
                {
                    using (var g = Graphics.FromImage(lienzo))
                    {
                        g.Clear(Color.White);
                        g.InterpolationMode = InterpolationMode.HighQualityBicubic;
                        g.DrawImage(original, new Rectangle(0, 0, ancho, alto));
                    }

                    var codec = ImageCodecInfo.GetImageEncoders().First(c => c.FormatID == ImageFormat.Jpeg.Guid);
                    using (var parametros = new EncoderParameters(1))
                    using (var salida = new MemoryStream())
                    {
                        parametros.Param[0] = new EncoderParameter(System.Drawing.Imaging.Encoder.Quality, 90L);
                        lienzo.Save(salida, codec, parametros);
                        return salida.ToArray();
                    }
                }
            }
        }

        private static RotateFlipType GiroExif(Image imagen)
        {
            const int OrientacionExif = 0x0112;
            if (!imagen.PropertyIdList.Contains(OrientacionExif))
                return RotateFlipType.RotateNoneFlipNone;
            var valor = imagen.GetPropertyItem(OrientacionExif).Value;
            int orientacion = valor != null && valor.Length >= 2 ? BitConverter.ToUInt16(valor, 0) : 1;
            switch (orientacion)
            {
                case 2: return RotateFlipType.RotateNoneFlipX;
                case 3: return RotateFlipType.Rotate180FlipNone;
                case 4: return RotateFlipType.Rotate180FlipX;
                case 5: return RotateFlipType.Rotate90FlipX;
                case 6: return RotateFlipType.Rotate90FlipNone;
                case 7: return RotateFlipType.Rotate270FlipX;
                case 8: return RotateFlipType.Rotate270FlipNone;
                default: return RotateFlipType.RotateNoneFlipNone;
            }
        }
    }
}
