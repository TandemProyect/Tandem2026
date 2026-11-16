using Desing.Helpers;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Web.Mvc;

namespace Desing.Controllers
{
    /// <summary>
    /// Frases que la IA no comprendió (pt119u, 15/11/2026). Cuando un asistente (IA del diseño, IA del muro) contesta
    /// «No he entendido…», el visor manda la frase aquí sin esperar respuesta y se guarda en
    /// <c>~/App_Data/IA/frases-no-comprendidas.json</c> (UTF-8, indentado, fuera de git y que el FTP no borra).
    /// La misma frase normalizada del mismo asistente no se repite: suma <c>veces</c> y cambia <c>ultimaFecha</c>.
    /// Revisión en el chat: «abre el .json» → se analizan una a una → las comprendidas pasan al catálogo
    /// (TSql_AiCommandPhrase en las dos BD + ai-command-catalog.js) y se marcan «resuelta».
    /// </summary>
    public partial class Desing_2Controller
    {
        internal const string AiUnknownRelPath = "~/App_Data/IA/frases-no-comprendidas.json";
        private static readonly object AiUnknownLock = new object();
        private static readonly Regex AiAssistantRegex = new Regex("^[a-z][a-z0-9_-]{0,19}$", RegexOptions.CultureInvariant);

        private string AiUnknownFilePath()
        {
            return Server.MapPath(AiUnknownRelPath);
        }

        /// <summary>
        /// POST Desing_2/SaveAiUnknownPhrase — cuerpo JSON { frase, asistente: "design"|"wall"|…, idioma?, contexto?: { … } }.
        /// Devuelve { ok, id, veces, nueva }. Nunca lanza: si falla, { ok:false, message } (el visor no lo enseña).
        /// </summary>
        [HttpPost]
        [RequireFunction(AiFunctionCode)]
        public ActionResult SaveAiUnknownPhrase()
        {
            try
            {
                var obj = AiReadJsonBody();
                var user = AiUnknownUserName();
                var result = AiUnknownAppend(AiUnknownFilePath(), obj, AiUnknownNow(), user);
                return AiJson(result);
            }
            catch (Exception ex)
            {
                return AiJson(new JObject { ["ok"] = false, ["message"] = ex.Message });
            }
        }

        /// <summary>
        /// GET Desing_2/AiUnknownPhrases?estado=pendiente|resuelta|descartada (vacío = todas).
        /// Devuelve { ok, archivo, total, pendientes, items[] } con las más vistas primero.
        /// </summary>
        [HttpGet]
        [RequireFunction(AiFunctionCode)]
        public ActionResult AiUnknownPhrases(string estado)
        {
            try
            {
                return AiJson(AiUnknownList(AiUnknownFilePath(), estado));
            }
            catch (Exception ex)
            {
                return AiJson(new JObject { ["ok"] = false, ["message"] = ex.Message });
            }
        }

        /// <summary>
        /// POST Desing_2/ResolveAiUnknownPhrase — cuerpo JSON { id, comandoAsignado?: "WALL_IDS", estado?: "resuelta"|"descartada"|"pendiente", nota? }.
        /// Marca la entrada (por defecto «resuelta») con <c>fechaResuelta</c>. Devuelve { ok, item }.
        /// </summary>
        [HttpPost]
        [RequireFunction(AiFunctionCode)]
        public ActionResult ResolveAiUnknownPhrase()
        {
            try
            {
                var obj = AiReadJsonBody();
                return AiJson(AiUnknownResolve(AiUnknownFilePath(), obj, AiUnknownNow(), AiUnknownUserName()));
            }
            catch (Exception ex)
            {
                return AiJson(new JObject { ["ok"] = false, ["message"] = ex.Message });
            }
        }

        private JObject AiReadJsonBody()
        {
            if (Request.InputStream.CanSeek)
            {
                Request.InputStream.Position = 0;
            }
            string raw;
            using (var reader = new StreamReader(Request.InputStream, Encoding.UTF8))
            {
                raw = reader.ReadToEnd();
            }
            return string.IsNullOrWhiteSpace(raw) ? new JObject() : (AiParseNoDates(raw) as JObject ?? new JObject());
        }

        /// <summary>JSON sin convertir fechas (las ISO con desfase se quedan como texto, tal cual).</summary>
        internal static JToken AiParseNoDates(string raw)
        {
            using (var reader = new JsonTextReader(new StringReader(raw)) { DateParseHandling = DateParseHandling.None })
            {
                return JToken.ReadFrom(reader);
            }
        }

        private string AiUnknownUserName()
        {
            try
            {
                return User != null && User.Identity != null ? (User.Identity.Name ?? "") : "";
            }
            catch
            {
                return "";
            }
        }

        /// <summary>Hora de Madrid con su desfase («2026-11-15T20:51:07+01:00»); si el servidor no la tiene, la local.</summary>
        internal static DateTimeOffset AiUnknownNow()
        {
            var utc = DateTimeOffset.UtcNow;
            try
            {
                var tz = TimeZoneInfo.FindSystemTimeZoneById("Romance Standard Time");
                return TimeZoneInfo.ConvertTime(utc, tz);
            }
            catch
            {
                try
                {
                    return TimeZoneInfo.ConvertTime(utc, TimeZoneInfo.FindSystemTimeZoneById("Europe/Madrid"));
                }
                catch
                {
                    return DateTimeOffset.Now;
                }
            }
        }

        internal static string AiUnknownIso(DateTimeOffset t)
        {
            return t.ToString("yyyy-MM-dd'T'HH:mm:sszzz", CultureInfo.InvariantCulture);
        }

        /// <summary>Clave de duplicados: minúsculas, sin tildes (la ñ se queda), sin signos y con espacios simples.</summary>
        internal static string AiUnknownNorm(string text)
        {
            var s = (text ?? "").ToLowerInvariant().Replace('ñ', '\u0001').Normalize(NormalizationForm.FormD);
            var sb = new StringBuilder(s.Length);
            foreach (var ch in s)
            {
                if (CharUnicodeInfo.GetUnicodeCategory(ch) == UnicodeCategory.NonSpacingMark) continue;
                sb.Append(ch == '\u0001' ? 'ñ' : ch);
            }
            var r = Regex.Replace(sb.ToString(), "[^a-z0-9ñ]+", " ");
            return r.Trim();
        }

        private static string AiClip(string s, int max)
        {
            s = (s ?? "").Trim();
            return s.Length > max ? s.Substring(0, max) : s;
        }

        private static JArray AiUnknownRead(string path)
        {
            if (!System.IO.File.Exists(path)) return new JArray();
            var raw = System.IO.File.ReadAllText(path, Encoding.UTF8);
            if (string.IsNullOrWhiteSpace(raw)) return new JArray();
            var tok = AiParseNoDates(raw);
            if (tok is JArray) return (JArray)tok;
            var items = tok["items"] as JArray;
            return items ?? new JArray();
        }

        /// <summary>Escribe entero (temporal + reemplazo): UTF-8 sin BOM, indentado.</summary>
        private static void AiUnknownWrite(string path, JArray items)
        {
            var dir = Path.GetDirectoryName(path);
            if (!string.IsNullOrEmpty(dir)) Directory.CreateDirectory(dir);
            var tmp = path + ".tmp";
            System.IO.File.WriteAllText(tmp, items.ToString(Formatting.Indented), new UTF8Encoding(false));
            if (System.IO.File.Exists(path))
            {
                System.IO.File.Copy(tmp, path, true);
                System.IO.File.Delete(tmp);
            }
            else
            {
                System.IO.File.Move(tmp, path);
            }
        }

        /// <summary>Añade la frase (o suma una vez a la que ya estaba). Puro salvo el archivo; con candado.</summary>
        internal static JObject AiUnknownAppend(string path, JObject body, DateTimeOffset now, string user)
        {
            var frase = AiClip((string)body["frase"], 500);
            var key = AiUnknownNorm(frase);
            if (key.Length == 0) return new JObject { ["ok"] = false, ["message"] = "Frase vacía." };
            var asistente = AiClip((string)body["asistente"], 20).ToLowerInvariant();
            if (!AiAssistantRegex.IsMatch(asistente)) asistente = "otro";
            var idioma = AiNormalizeLang((string)body["idioma"]);
            var ctxIn = body["contexto"] as JObject ?? new JObject();
            var contexto = new JObject();
            foreach (var prop in ctxIn.Properties().Take(12))
            {
                var name = AiClip(prop.Name, 40);
                if (name.Length == 0) continue;
                var v = prop.Value;
                if (v == null || v.Type == JTokenType.Null) contexto[name] = null;
                else if (v.Type == JTokenType.Integer || v.Type == JTokenType.Float || v.Type == JTokenType.Boolean) contexto[name] = v;
                else contexto[name] = AiClip(v.Type == JTokenType.String ? (string)v : v.ToString(Formatting.None), 1000);
            }
            var iso = AiUnknownIso(now);
            lock (AiUnknownLock)
            {
                var items = AiUnknownRead(path);
                var hit = items.OfType<JObject>().FirstOrDefault(e =>
                    string.Equals((string)e["asistente"], asistente, StringComparison.Ordinal) &&
                    string.Equals(AiUnknownNorm((string)e["frase"]), key, StringComparison.Ordinal));
                if (hit != null)
                {
                    hit["veces"] = ((int?)hit["veces"] ?? 1) + 1;
                    hit["ultimaFecha"] = iso;
                    hit["contexto"] = contexto;
                    if (!string.IsNullOrEmpty(user)) hit["usuario"] = user;
                    /* Si se resolvió y vuelve a fallar, se reabre para revisarla otra vez. */
                    if (!string.Equals((string)hit["estado"], "pendiente", StringComparison.Ordinal)) hit["estado"] = "pendiente";
                    AiUnknownWrite(path, items);
                    return new JObject { ["ok"] = true, ["id"] = hit["id"], ["veces"] = hit["veces"], ["nueva"] = false };
                }
                var nextId = items.OfType<JObject>().Select(e => (long?)e["id"] ?? 0L).DefaultIfEmpty(0L).Max() + 1;
                var entry = new JObject
                {
                    ["id"] = nextId,
                    ["fecha"] = iso,
                    ["ultimaFecha"] = iso,
                    ["frase"] = frase,
                    ["asistente"] = asistente,
                    ["idioma"] = idioma,
                    ["contexto"] = contexto,
                    ["usuario"] = user ?? "",
                    ["veces"] = 1,
                    ["estado"] = "pendiente",
                    ["comandoAsignado"] = null,
                };
                items.Add(entry);
                AiUnknownWrite(path, items);
                return new JObject { ["ok"] = true, ["id"] = nextId, ["veces"] = 1, ["nueva"] = true };
            }
        }

        internal static JObject AiUnknownList(string path, string estado)
        {
            JArray items;
            lock (AiUnknownLock)
            {
                items = AiUnknownRead(path);
            }
            var est = (estado ?? "").Trim().ToLowerInvariant();
            var list = items.OfType<JObject>()
                .Where(e => est.Length == 0 || string.Equals((string)e["estado"], est, StringComparison.Ordinal))
                .OrderByDescending(e => (int?)e["veces"] ?? 1).ThenBy(e => (long?)e["id"] ?? 0L).ToList();
            return new JObject
            {
                ["ok"] = true,
                ["archivo"] = AiUnknownRelPath,
                ["total"] = items.Count,
                ["pendientes"] = items.OfType<JObject>().Count(e => string.Equals((string)e["estado"], "pendiente", StringComparison.Ordinal)),
                ["items"] = new JArray(list),
            };
        }

        internal static JObject AiUnknownResolve(string path, JObject body, DateTimeOffset now, string user)
        {
            var id = (long?)body["id"];
            if (!id.HasValue) return new JObject { ["ok"] = false, ["message"] = "Falta el id." };
            var estado = AiClip((string)body["estado"], 20).ToLowerInvariant();
            if (estado.Length == 0) estado = "resuelta";
            if (estado != "resuelta" && estado != "descartada" && estado != "pendiente")
            {
                return new JObject { ["ok"] = false, ["message"] = "Estado no válido: " + estado };
            }
            lock (AiUnknownLock)
            {
                var items = AiUnknownRead(path);
                var hit = items.OfType<JObject>().FirstOrDefault(e => ((long?)e["id"] ?? -1L) == id.Value);
                if (hit == null) return new JObject { ["ok"] = false, ["message"] = "No existe la entrada " + id.Value + "." };
                hit["estado"] = estado;
                if (body["comandoAsignado"] != null) hit["comandoAsignado"] = AiClip((string)body["comandoAsignado"], 60);
                if (body["nota"] != null) hit["nota"] = AiClip((string)body["nota"], 500);
                hit["fechaResuelta"] = estado == "pendiente" ? null : AiUnknownIso(now);
                if (!string.IsNullOrEmpty(user)) hit["resueltaPor"] = user;
                AiUnknownWrite(path, items);
                return new JObject { ["ok"] = true, ["item"] = hit };
            }
        }
    }
}
