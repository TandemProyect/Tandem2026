using Desing.Helpers;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
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
    /// Revisión en el visor: «analiza el .json». El programador (función DevelopAi) guarda la frase como
    /// defecto para todos (es y en, sin concepto). Un usuario normal la guarda solo para él. No se inventan comandos.
    /// </summary>
    public partial class Desing_2Controller
    {
        internal const string AiUnknownRelPath = "~/App_Data/IA/frases-no-comprendidas.json";
        private static readonly object AiUnknownLock = new object();
        private static readonly Regex AiAssistantRegex = new Regex("^[a-z][a-z0-9_-]{0,19}$", RegexOptions.CultureInvariant);
        private static readonly Regex AiLearnCodeRegex = new Regex("^[A-Z][A-Z0-9_]{1,59}$", RegexOptions.CultureInvariant);

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
                var result = AiUnknownList(AiUnknownFilePath(), estado);
                var programmer = AiIsProgrammer();
                var user = AiUnknownUserName();
                if (!programmer)
                {
                    var items = result["items"] as JArray ?? new JArray();
                    var mine = new JArray();
                    foreach (var e in items.OfType<JObject>())
                    {
                        if (AiSameUser((string)e["usuario"], user)) mine.Add(e);
                    }
                    result["items"] = mine;
                    result["total"] = mine.Count;
                    result["pendientes"] = mine.OfType<JObject>().Count(e => string.Equals((string)e["estado"], "pendiente", StringComparison.Ordinal));
                }
                result["programmer"] = programmer;
                return AiJson(result);
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
                var only = AiIsProgrammer() ? null : AiUnknownUserName();
                return AiJson(AiUnknownResolve(AiUnknownFilePath(), obj, AiUnknownNow(), AiUnknownUserName(), only));
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

        /// <summary>
        /// POST Desing_2/LearnAiUnknownPhrase — cuerpo { id, code, phrase, lang?, otherPhrase?, scope?: "default"|"user", estado?: "descartada" }.
        /// Programador (DevelopAi activa y concedida): frase por defecto para todos, sin concepto. Si no, solo la suya.
        /// No crea comandos. Rechaza empalme / no implementado, frase ajena y frase ya asignada a otro comando.
        /// </summary>
        [HttpPost]
        [RequireFunction(AiFunctionCode)]
        public ActionResult LearnAiUnknownPhrase()
        {
            try
            {
                if (!AiCatalogTablesReady())
                {
                    return AiJson(new JObject { ["ok"] = false, ["reason"] = "missing", ["message"] = "Faltan las tablas TSql_Ai*." });
                }
                var body = AiReadJsonBody();
                var programmer = AiIsProgrammer();
                var userName = AiUnknownUserName();
                var userId = IntranetAuditHelper.ResolveCurrentUserId(User);
                if (string.IsNullOrWhiteSpace(userId) || userId == "system")
                {
                    return AiJson(new JObject { ["ok"] = false, ["reason"] = "forbidden", ["message"] = "Usuario no identificado." });
                }
                var id = (long?)body["id"];
                if (!id.HasValue) return AiJson(new JObject { ["ok"] = false, ["reason"] = "missing", ["message"] = "Falta el id." });
                var item = AiUnknownFind(AiUnknownFilePath(), id.Value);
                if (item == null) return AiJson(new JObject { ["ok"] = false, ["reason"] = "missing", ["message"] = "No existe la entrada " + id.Value + "." });
                if (!programmer && !AiSameUser((string)item["usuario"], userName))
                {
                    return AiJson(new JObject { ["ok"] = false, ["reason"] = "forbidden", ["message"] = "Esa frase no es tuya." });
                }
                var estado = AiClip((string)body["estado"], 20).ToLowerInvariant();
                if (estado == "descartada")
                {
                    body["estado"] = "descartada";
                    return AiJson(AiUnknownResolve(AiUnknownFilePath(), body, AiUnknownNow(), userName, programmer ? null : userName));
                }
                var asistente = AiClip((string)item["asistente"], 20).ToLowerInvariant();
                if (asistente.Length > 0 && asistente != "design")
                {
                    return AiJson(new JObject { ["ok"] = false, ["reason"] = "wall", ["message"] = "Esa frase es de otro asistente." });
                }
                var code = AiClip((string)body["code"], 60).ToUpperInvariant();
                if (!AiLearnCodeRegex.IsMatch(code))
                {
                    return AiJson(new JObject { ["ok"] = false, ["reason"] = "unknown", ["message"] = "Comando no válido." });
                }
                var cmd = db.Database.SqlQuery<AiLearnCommandRow>(
                    "SELECT TOP 1 IdObject, TextCode, TextContext, TextHandler, TextAiCommandId, Is_AiImplemented " +
                    "FROM dbo.TSql_AiCommand WHERE Is_Delete = 0 AND Is_Active = 1 AND TextCode = @p0", code).FirstOrDefault();
                var why = AiLearnReject(cmd);
                if (why != null)
                {
                    return AiJson(new JObject { ["ok"] = false, ["reason"] = why, ["message"] = why == "unavailable" ? "Ese comando no está en el visor." : "Ese comando no es del diseño." });
                }
                var scope = programmer ? "default" : "user";
                var phrases = AiLearnPhrases(body, item);
                if (phrases.Count == 0)
                {
                    return AiJson(new JObject { ["ok"] = false, ["reason"] = "empty", ["message"] = "Frase vacía." });
                }
                JObject saved;
                using (var tx = db.Database.BeginTransaction())
                {
                    saved = scope == "default"
                        ? AiInsertDefaultPhrases(cmd, phrases, userId)
                        : AiInsertUserPhrases(cmd, phrases, userId);
                    if (saved["ok"] != null && !(bool)saved["ok"])
                    {
                        tx.Rollback();
                        return AiJson(saved);
                    }
                    tx.Commit();
                }
                body["estado"] = "resuelta";
                body["comandoAsignado"] = cmd.TextCode;
                var resolved = AiUnknownResolve(AiUnknownFilePath(), body, AiUnknownNow(), userName, programmer ? null : userName);
                saved["scope"] = scope;
                saved["code"] = cmd.TextCode;
                saved["resolved"] = resolved["ok"];
                return AiJson(saved);
            }
            catch (Exception ex)
            {
                return AiJson(new JObject { ["ok"] = false, ["reason"] = "fail", ["message"] = ex.Message });
            }
        }

        /// <summary>DevelopAi tiene que existir y estar concedida. Si no está sembrada, nadie escribe frases para todos.</summary>
        private bool AiIsProgrammer()
        {
            var userId = IntranetAuditHelper.ResolveCurrentUserId(User);
            if (string.IsNullOrWhiteSpace(userId) || userId == "system") return false;
            var protectedCodes = FunctionAccessService.GetProtectedCodes();
            if (protectedCodes == null || !protectedCodes.Contains("DevelopAi")) return false;
            return FunctionAccessService.Can(userId, "DevelopAi");
        }

        internal static bool AiSameUser(string stored, string current)
        {
            stored = (stored ?? "").Trim();
            current = (current ?? "").Trim();
            return stored.Length > 0 && current.Length > 0 && string.Equals(stored, current, StringComparison.OrdinalIgnoreCase);
        }

        internal static JObject AiUnknownFind(string path, long id)
        {
            lock (AiUnknownLock)
            {
                var hit = AiUnknownRead(path).OfType<JObject>().FirstOrDefault(e => ((long?)e["id"] ?? -1L) == id);
                return hit == null ? null : (JObject)hit.DeepClone();
            }
        }

        /// <summary>null si se puede aprender; si no, unavailable | unknown.</summary>
        internal static string AiLearnReject(AiLearnCommandRow cmd)
        {
            if (cmd == null || !string.Equals(cmd.TextContext, "design", StringComparison.OrdinalIgnoreCase)) return "unknown";
            if (cmd.TextCode.StartsWith("ENV_", StringComparison.Ordinal) || cmd.TextCode.StartsWith("COMMON_", StringComparison.Ordinal)) return "unknown";
            var handler = cmd.TextHandler ?? "";
            var aiId = cmd.TextAiCommandId ?? "";
            if (!cmd.Is_AiImplemented || aiId.Equals("notAvailable", StringComparison.OrdinalIgnoreCase) || handler.IndexOf("notAvailable", StringComparison.OrdinalIgnoreCase) >= 0)
            {
                return "unavailable";
            }
            return null;
        }

        /// <summary>Misma clave que maStlWallAiNormalize: minúsculas, sin tildes (ñ→n), sin signos.</summary>
        internal static string AiPhraseNorm(string text)
        {
            var s = (text ?? "").ToLowerInvariant().Normalize(NormalizationForm.FormD);
            var sb = new StringBuilder(s.Length);
            foreach (var ch in s)
            {
                if (CharUnicodeInfo.GetUnicodeCategory(ch) == UnicodeCategory.NonSpacingMark) continue;
                sb.Append(ch);
            }
            return Regex.Replace(sb.ToString(), "[^a-z0-9]+", " ").Trim();
        }

        private static List<AiLearnPhrase> AiLearnPhrases(JObject body, JObject item)
        {
            var list = new List<AiLearnPhrase>();
            var phrase = AiClip((string)body["phrase"], 200);
            if (phrase.Length == 0) phrase = AiClip((string)item["frase"], 200);
            var lang = AiBaseLang(AiNormalizeLang((string)body["lang"]) ?? AiNormalizeLang((string)item["idioma"]) ?? "es");
            if (lang != "es" && lang != "en") lang = "es";
            AddLearnPhrase(list, lang, phrase);
            var other = AiClip((string)body["otherPhrase"], 200);
            var otherLang = lang == "en" ? "es" : "en";
            AddLearnPhrase(list, otherLang, other);
            return list;
        }

        private static void AddLearnPhrase(List<AiLearnPhrase> list, string lang, string phrase)
        {
            var norm = AiPhraseNorm(phrase);
            if (norm.Length == 0 || norm.Length > 200) return;
            if (list.Any(p => p.Lang == lang && p.Norm == norm)) return;
            list.Add(new AiLearnPhrase { Lang = lang, Phrase = AiClip(phrase, 200), Norm = norm });
        }

        private JObject AiInsertDefaultPhrases(AiLearnCommandRow cmd, List<AiLearnPhrase> phrases, string userId)
        {
            foreach (var ph in phrases)
            {
                var owners = db.Database.SqlQuery<string>(
                    "SELECT c.TextCode FROM dbo.TSql_AiCommandPhrase p INNER JOIN dbo.TSql_AiCommand c ON c.IdObject = p.LinkAiCommand " +
                    "WHERE p.Is_Delete = 0 AND p.TextContext = @p0 AND p.TextLanguageCode = @p1 AND p.TextPhraseNorm = @p2",
                    cmd.TextContext, ph.Lang, ph.Norm).ToList();
                if (owners.Any(c => !string.Equals(c, cmd.TextCode, StringComparison.OrdinalIgnoreCase)))
                {
                    return new JObject { ["ok"] = false, ["reason"] = "conflict", ["command"] = owners.First(c => !string.Equals(c, cmd.TextCode, StringComparison.OrdinalIgnoreCase)), ["message"] = "Esa frase ya está asignada." };
                }
                if (owners.Count > 0) continue;
                db.Database.ExecuteSqlCommand(
                    "INSERT INTO dbo.TSql_AiCommandPhrase (LinkAiCommand, TextContext, TextLanguageCode, TextPhrase, TextPhraseNorm, TextKind, TextConcept, NumberWeight, NumberOrder, TextLabel, Is_Delete, Is_Active, LinkMadeBy, LinModifiedBy, AddDateMade, AddLastDateChange, Ntimeschanged, LinkEnvironment) " +
                    "VALUES (@cmd, @ctx, @lang, @phrase, @norm, N'keyword', NULL, NULL, 9000, N'learned', 0, 1, @user, NULL, GETDATE(), NULL, 0, NULL)",
                    new SqlParameter("@cmd", cmd.IdObject),
                    new SqlParameter("@ctx", cmd.TextContext),
                    new SqlParameter("@lang", ph.Lang),
                    new SqlParameter("@phrase", ph.Phrase),
                    new SqlParameter("@norm", ph.Norm),
                    new SqlParameter("@user", AiClip(userId, 128)));
            }
            return new JObject { ["ok"] = true, ["scope"] = "default" };
        }

        private JObject AiInsertUserPhrases(AiLearnCommandRow cmd, List<AiLearnPhrase> phrases, string userId)
        {
            foreach (var ph in phrases)
            {
                var owners = db.Database.SqlQuery<string>(
                    "SELECT c.TextCode FROM dbo.TSql_AiCommandUserPhrase p INNER JOIN dbo.TSql_AiCommand c ON c.IdObject = p.LinkAiCommand " +
                    "WHERE p.Is_Delete = 0 AND p.Is_Exclude = 0 AND p.LinkAspNetUsers = @p0 AND p.TextContext = @p1 AND p.TextLanguageCode = @p2 AND p.TextPhraseNorm = @p3",
                    userId, cmd.TextContext, ph.Lang, ph.Norm).ToList();
                if (owners.Any(c => !string.Equals(c, cmd.TextCode, StringComparison.OrdinalIgnoreCase)))
                {
                    return new JObject { ["ok"] = false, ["reason"] = "conflict", ["command"] = owners.First(c => !string.Equals(c, cmd.TextCode, StringComparison.OrdinalIgnoreCase)), ["message"] = "Esa frase ya está asignada." };
                }
                if (owners.Count > 0)
                {
                    db.Database.ExecuteSqlCommand(
                        "UPDATE dbo.TSql_AiCommandUserPhrase SET TextLabel = N'learned', LinModifiedBy = @user, AddLastDateChange = GETDATE(), Ntimeschanged = Ntimeschanged + 1 " +
                        "WHERE Is_Delete = 0 AND Is_Exclude = 0 AND LinkAspNetUsers = @user AND TextContext = @ctx AND TextLanguageCode = @lang AND TextPhraseNorm = @norm",
                        new SqlParameter("@user", AiClip(userId, 128)),
                        new SqlParameter("@ctx", cmd.TextContext),
                        new SqlParameter("@lang", ph.Lang),
                        new SqlParameter("@norm", ph.Norm));
                    continue;
                }
                db.Database.ExecuteSqlCommand(
                    "INSERT INTO dbo.TSql_AiCommandUserPhrase (LinkAspNetUsers, LinkAiCommand, LinkAiCommandPhrase, TextContext, TextLanguageCode, TextPhrase, TextPhraseNorm, Is_Exclude, TextLabel, Is_Delete, Is_Active, LinkMadeBy, LinModifiedBy, AddDateMade, AddLastDateChange, Ntimeschanged, LinkEnvironment) " +
                    "VALUES (@user, @cmd, NULL, @ctx, @lang, @phrase, @norm, 0, N'learned', 0, 1, @user, NULL, GETDATE(), NULL, 0, NULL)",
                    new SqlParameter("@user", AiClip(userId, 128)),
                    new SqlParameter("@cmd", cmd.IdObject),
                    new SqlParameter("@ctx", cmd.TextContext),
                    new SqlParameter("@lang", ph.Lang),
                    new SqlParameter("@phrase", ph.Phrase),
                    new SqlParameter("@norm", ph.Norm));
            }
            return new JObject { ["ok"] = true, ["scope"] = "user" };
        }

        private sealed class AiLearnPhrase
        {
            public string Lang { get; set; }
            public string Phrase { get; set; }
            public string Norm { get; set; }
        }

        internal static JObject AiUnknownResolve(string path, JObject body, DateTimeOffset now, string user, string onlyUser)
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
                if (!string.IsNullOrEmpty(onlyUser) && !AiSameUser((string)hit["usuario"], onlyUser))
                {
                    return new JObject { ["ok"] = false, ["reason"] = "forbidden", ["message"] = "Esa frase no es tuya." };
                }
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
