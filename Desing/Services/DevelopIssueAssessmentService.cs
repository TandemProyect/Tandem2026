using Desing.Helpers;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace Desing.Services
{
    /// <summary>
    /// Valoración orientativa de una US o un bug con OpenAI (texto + capturas).
    /// No conoce el código: el agente de programación la afina (ver Docs/areas/intranet/us-bugs-agente.md).
    /// </summary>
    public static class DevelopIssueAssessmentService
    {
        public const string Model = "gpt-4o";
        private const int MaxImages = 6;
        private static readonly HttpClient Http = new HttpClient { Timeout = TimeSpan.FromSeconds(90) };

        private const string SystemPrompt = @"Eres un analista técnico de una intranet ASP.NET MVC 4.8 (C#, Razor, jQuery, SQL Server) con un visor 3D de encofrados (Three.js) y plugins CAD.
Recibes una US o un bug registrado por el equipo: título, tipo, urgencia, observaciones y capturas de pantalla.
Valora cuánto costaría resolverlo. No ves el código, así que sé prudente y dilo si faltan datos.
Responde SOLO JSON:
{""size"":""SMALL|MEDIUM|LARGE"",""hours"":número,""area"":""pantalla o módulo afectado"",""analysis"":""texto""}
- SMALL: hasta 2 horas, un cambio localizado. MEDIUM: de 2 a 8 horas, varias piezas. LARGE: más de 8 horas o requiere decisiones de diseño.
- analysis: en el idioma indicado, máximo 150 palabras: qué se pide o qué falla, qué se ve en las capturas, causa probable, pasos de la solución y dudas abiertas.";

        public static bool IsConfigured => !string.IsNullOrEmpty(ApiKey());

        public static async Task<DevelopIssueAssessmentRow> AssessAsync(
            DevelopIssueRow issue, IEnumerable<DevelopIssueDetailRow> details, Func<string, string> mapPath, string language)
        {
            var key = ApiKey();
            if (string.IsNullOrEmpty(key)) throw new InvalidOperationException("OPENAI_APIKEY");

            var rows = details.ToList();
            var text = new StringBuilder()
                .AppendLine("Idioma de la respuesta: " + language)
                .AppendLine("Tipo: " + issue.TextType)
                .AppendLine("Urgencia: " + issue.TextUrgency)
                .AppendLine("Título: " + issue.TextLabel)
                .AppendLine("Observaciones:");
            foreach (var d in rows.Where(d => !string.IsNullOrWhiteSpace(d.TextObservation)))
                text.AppendLine("- " + d.TextObservation.Trim());

            var content = new List<object> { new { type = "text", text = text.ToString() } };
            foreach (var d in rows.Where(d => !string.IsNullOrWhiteSpace(d.TextImagePath)).Take(MaxImages))
            {
                var file = mapPath("~" + d.TextImagePath);
                if (!File.Exists(file)) continue;
                var ext = Path.GetExtension(file).TrimStart('.').ToLowerInvariant();
                var mime = "image/" + (ext == "jpg" ? "jpeg" : ext);
                content.Add(new
                {
                    type = "image_url",
                    image_url = new { url = "data:" + mime + ";base64," + Convert.ToBase64String(File.ReadAllBytes(file)) }
                });
            }

            var body = new
            {
                model = Model,
                max_tokens = 700,
                response_format = new { type = "json_object" },
                messages = new object[]
                {
                    new { role = "system", content = SystemPrompt },
                    new { role = "user", content = content }
                }
            };

            var request = new HttpRequestMessage(HttpMethod.Post, "https://api.openai.com/v1/chat/completions");
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", key);
            request.Content = new StringContent(JsonConvert.SerializeObject(body), Encoding.UTF8, "application/json");

            var response = await Http.SendAsync(request).ConfigureAwait(false);
            var responseBody = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
            if (!response.IsSuccessStatusCode) throw new Exception("OpenAI " + (int)response.StatusCode);

            var json = JObject.Parse(JObject.Parse(responseBody)["choices"][0]["message"]["content"].ToString());
            var size = ((string)json["size"] ?? "").Trim().ToUpperInvariant();
            decimal hours;
            var hasHours = decimal.TryParse(Convert.ToString(json["hours"], CultureInfo.InvariantCulture),
                NumberStyles.Float, CultureInfo.InvariantCulture, out hours);

            return new DevelopIssueAssessmentRow
            {
                LinkDevelopIssue = issue.IdObject,
                TextSource = "AI",
                TextSize = DevelopIssueCodes.Sizes.Contains(size) ? size : "MEDIUM",
                NumberHours = hasHours ? Math.Min(Math.Max(hours, 0), 9999) : (decimal?)null,
                TextArea = (string)json["area"],
                TextAnalysis = (string)json["analysis"],
                TextModel = Model
            };
        }

        private static string ApiKey()
        {
            var key = Environment.GetEnvironmentVariable("OPENAI_APIKEY");
            if (string.IsNullOrWhiteSpace(key)) key = ConfigurationManager.AppSettings["OPENAI_APIKEY"];
            key = (key ?? "").Trim();
            return key.StartsWith("sk-", StringComparison.Ordinal) ? key : null;
        }
    }
}
