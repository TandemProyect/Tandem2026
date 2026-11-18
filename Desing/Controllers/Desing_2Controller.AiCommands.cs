using Desing.Helpers;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Web.Mvc;

namespace Desing.Controllers
{
    /// <summary>
    /// Catálogo de comandos de la IA del visor Desing_2 (pt107u, 14/11/2026): comandos activos, palabras efectivas del
    /// usuario y plantillas de respuesta de su entorno de conversación (tablas dbo.TSql_Ai*).
    /// SQL directo con <c>db.Database.SqlQuery</c> (sin entidades de Model.edmx): funciona aunque el modelo aún no
    /// tenga las tablas nuevas y no obliga a regenerarlo. Solo lectura, salvo <see cref="SaveAiUserSettings"/>
    /// (una fila por usuario en dbo.TSql_AiUserSettings).
    /// </summary>
    public partial class Desing_2Controller
    {
        /// <summary>Función «IA»: si está activa, solo la usan los roles que la tengan (botones y endpoints de la IA).</summary>
        private const string AiFunctionCode = FunctionCatalog.AiCode;

        private static readonly Regex AiLangRegex = new Regex("^[a-z]{2,3}(-[a-z0-9]{2,8}){0,2}$", RegexOptions.CultureInvariant);
        private static readonly Regex AiEnvCodeRegex = new Regex("^[A-Z][A-Z0-9_]{1,39}$", RegexOptions.CultureInvariant);

        private static readonly string[] AiCatalogTables =
        {
            "TSql_AiCommand", "TSql_AiCommandPhrase", "TSql_AiCommandUser", "TSql_AiCommandUserPhrase",
            "TSql_AiEnvironment", "TSql_AiEnvironmentReply", "TSql_AiUserSettings"
        };

        /// <summary>
        /// GET Desing_2/AiCommands?context=design|wall&amp;lang=es|en|es-ar…&amp;env=EXECUTIVE…&amp;v=versión
        /// Devuelve { ok, version, context, lang, environment, environments[], commands[{ …, phrases[] }], replies{ es, en } }.
        /// Palabras efectivas = por defecto (si el usuario no las sustituye) − excluidas + las suyas; frases sin entorno
        /// + las del entorno activo; idiomas base siempre + la variante pedida. Si <paramref name="v"/> o If-None-Match
        /// coinciden con la versión: { ok, notModified, version }. Faltan tablas: { ok:false, missing:true }.
        /// </summary>
        [HttpGet]
        [RequireFunction(AiFunctionCode)]
        public ActionResult AiCommands(string context, string lang, string env, string v)
        {
            try
            {
                if (!AiCatalogTablesReady())
                {
                    return AiJson(new JObject { ["ok"] = false, ["missing"] = true, ["message"] = "Faltan las tablas TSql_Ai* (scripts 2026-11-14)." });
                }

                var ctx = string.Equals((context ?? "").Trim(), "wall", StringComparison.OrdinalIgnoreCase) ? "wall" : "design";
                var userId = IntranetAuditHelper.ResolveCurrentUserId(User);
                var reqLang = AiNormalizeLang(lang) ?? "es";
                var reqBase = AiBaseLang(reqLang);

                var envs = db.Database.SqlQuery<AiEnvironmentRow>(
                    "SELECT IdObject, TextCode, TextNameEs, TextNameEn, TextDescription, Is_Default, NumberOrder " +
                    "FROM dbo.TSql_AiEnvironment WHERE Is_Delete = 0 AND Is_Active = 1 ORDER BY NumberOrder, IdObject").ToList();

                AiUserSettingsRow settings = null;
                if (!string.IsNullOrEmpty(userId))
                {
                    settings = db.Database.SqlQuery<AiUserSettingsRow>(
                        "SELECT TOP 1 LinkEnvironment, TextLanguageCode FROM dbo.TSql_AiUserSettings " +
                        "WHERE LinkAspNetUsers = @p0 AND Is_Delete = 0 AND Is_Active = 1 ORDER BY IdObject DESC", userId).FirstOrDefault();
                }

                /* Idioma: la variante preferida del usuario si es del mismo idioma base que el pedido. */
                var prefLang = settings != null ? AiNormalizeLang(settings.TextLanguageCode) : null;
                if (prefLang != null && reqLang == reqBase && AiBaseLang(prefLang) == reqBase)
                {
                    reqLang = prefLang;
                }

                /* Entorno: el pedido → el del usuario → el de por defecto. */
                var defEnv = envs.FirstOrDefault(e => e.Is_Default) ?? envs.FirstOrDefault();
                AiEnvironmentRow curEnv = null;
                var envCode = (env ?? "").Trim().ToUpperInvariant();
                if (envCode.Length > 0)
                {
                    curEnv = envs.FirstOrDefault(e => string.Equals(e.TextCode, envCode, StringComparison.OrdinalIgnoreCase));
                }
                if (curEnv == null && settings != null && settings.LinkEnvironment.HasValue)
                {
                    curEnv = envs.FirstOrDefault(e => e.IdObject == settings.LinkEnvironment.Value);
                }
                if (curEnv == null)
                {
                    curEnv = defEnv;
                }
                var curEnvId = curEnv != null ? curEnv.IdObject : -1L;
                var defEnvId = defEnv != null ? defEnv.IdObject : -1L;

                var commands = db.Database.SqlQuery<AiCommandRow>(
                    "SELECT IdObject, TextCode, TextContext, TextGroup, TextNameEs, TextNameEn, TextHandler, TextAiCommandId, " +
                    "Is_Destructive, Is_Confirm, Is_AiImplemented, NumberOrder " +
                    "FROM dbo.TSql_AiCommand WHERE Is_Delete = 0 AND Is_Active = 1 AND TextContext IN (@p0, N'common') " +
                    "ORDER BY NumberOrder, IdObject", ctx).ToList();

                var phrases = db.Database.SqlQuery<AiPhraseRow>(
                    "SELECT p.IdObject, p.LinkAiCommand, p.TextLanguageCode, p.TextPhrase, p.TextPhraseNorm, p.TextKind, p.TextConcept, " +
                    "p.NumberWeight, p.LinkEnvironment, p.NumberOrder, p.TextLabel " +
                    "FROM dbo.TSql_AiCommandPhrase p INNER JOIN dbo.TSql_AiCommand c ON c.IdObject = p.LinkAiCommand " +
                    "WHERE p.Is_Delete = 0 AND p.Is_Active = 1 AND c.Is_Delete = 0 AND c.Is_Active = 1 AND c.TextContext IN (@p0, N'common') " +
                    "AND (p.LinkEnvironment IS NULL OR p.LinkEnvironment = @p1) " +
                    "AND (CHARINDEX(N'-', p.TextLanguageCode) = 0 OR p.TextLanguageCode = @p2) " +
                    "ORDER BY p.NumberOrder, p.IdObject", ctx, curEnvId, reqLang).ToList();

                var userCmds = new List<AiCommandUserRow>();
                var userPhrases = new List<AiUserPhraseRow>();
                if (!string.IsNullOrEmpty(userId))
                {
                    userCmds = db.Database.SqlQuery<AiCommandUserRow>(
                        "SELECT LinkAiCommand, Is_UseDefaultPhrases, NumberOrder, Is_Active FROM dbo.TSql_AiCommandUser " +
                        "WHERE LinkAspNetUsers = @p0 AND Is_Delete = 0", userId).ToList();
                    userPhrases = db.Database.SqlQuery<AiUserPhraseRow>(
                        "SELECT IdObject, LinkAiCommand, LinkAiCommandPhrase, TextLanguageCode, TextPhrase, TextPhraseNorm, Is_Exclude, LinkEnvironment, TextLabel " +
                        "FROM dbo.TSql_AiCommandUserPhrase WHERE LinkAspNetUsers = @p0 AND Is_Delete = 0 AND Is_Active = 1 " +
                        "AND (LinkEnvironment IS NULL OR LinkEnvironment = @p1) " +
                        "AND (CHARINDEX(N'-', TextLanguageCode) = 0 OR TextLanguageCode = @p2) ORDER BY IdObject",
                        userId, curEnvId, reqLang).ToList();
                }

                var replies = db.Database.SqlQuery<AiReplyRow>(
                    "SELECT LinkEnvironment, TextLanguageCode, TextMessageKey, TextTemplate FROM dbo.TSql_AiEnvironmentReply " +
                    "WHERE Is_Delete = 0 AND Is_Active = 1 AND LinkEnvironment IN (@p0, @p1)", curEnvId, defEnvId).ToList();

                var payload = BuildAiCatalogPayload(ctx, reqLang, curEnv, envs, commands, phrases, userCmds, userPhrases, replies, defEnvId);
                var body = payload.ToString(Formatting.None);
                var version = AiSha1(body);

                Response.AppendHeader("ETag", "\"" + version + "\"");
                Response.AppendHeader("Cache-Control", "private, no-cache");
                var inm = (Request.Headers["If-None-Match"] ?? "").Trim().Trim('"');
                if (string.Equals(v, version, StringComparison.Ordinal) || string.Equals(inm, version, StringComparison.Ordinal))
                {
                    return AiJson(new JObject { ["ok"] = true, ["notModified"] = true, ["version"] = version });
                }
                payload.AddFirst(new JProperty("version", version));
                payload.AddFirst(new JProperty("ok", true));
                return AiJson(payload);
            }
            catch (Exception ex)
            {
                return AiJson(new JObject { ["ok"] = false, ["message"] = ex.Message });
            }
        }

        /// <summary>
        /// POST Desing_2/SaveAiUserSettings — cuerpo JSON { env: "COLLOQUIAL", lang?: "es-ar" } (lang null = el de la interfaz).
        /// Guarda el entorno de conversación del usuario (una fila por usuario en dbo.TSql_AiUserSettings).
        /// </summary>
        [HttpPost]
        [RequireFunction(AiFunctionCode)]
        public ActionResult SaveAiUserSettings()
        {
            try
            {
                if (!AiCatalogTablesReady())
                {
                    return AiJson(new JObject { ["ok"] = false, ["missing"] = true, ["message"] = "Faltan las tablas TSql_Ai*." });
                }
                var userId = IntranetAuditHelper.ResolveCurrentUserId(User);
                if (string.IsNullOrEmpty(userId))
                {
                    return AiJson(new JObject { ["ok"] = false, ["message"] = "Usuario no identificado." });
                }
                if (Request.InputStream.CanSeek)
                {
                    Request.InputStream.Position = 0;
                }
                string rawJson;
                using (var reader = new StreamReader(Request.InputStream, Encoding.UTF8))
                {
                    rawJson = reader.ReadToEnd();
                }
                var obj = string.IsNullOrWhiteSpace(rawJson) ? new JObject() : JObject.Parse(rawJson);
                var envCode = ((string)obj["env"] ?? "").Trim().ToUpperInvariant();
                if (!AiEnvCodeRegex.IsMatch(envCode))
                {
                    return AiJson(new JObject { ["ok"] = false, ["message"] = "Entorno no válido." });
                }
                var envRow = db.Database.SqlQuery<AiEnvironmentRow>(
                    "SELECT IdObject, TextCode, TextNameEs, TextNameEn, TextDescription, Is_Default, NumberOrder " +
                    "FROM dbo.TSql_AiEnvironment WHERE Is_Delete = 0 AND Is_Active = 1 AND TextCode = @p0", envCode).FirstOrDefault();
                if (envRow == null)
                {
                    return AiJson(new JObject { ["ok"] = false, ["message"] = "Entorno no encontrado: " + envCode });
                }
                var setLang = obj["lang"] != null;
                var langValue = setLang ? AiNormalizeLang((string)obj["lang"]) : null;

                var sql =
                    "IF EXISTS (SELECT 1 FROM dbo.TSql_AiUserSettings WHERE LinkAspNetUsers = @u AND Is_Delete = 0) " +
                    "UPDATE dbo.TSql_AiUserSettings SET LinkEnvironment = @e, " +
                    "TextLanguageCode = CASE WHEN @setLang = 1 THEN @l ELSE TextLanguageCode END, " +
                    "TextLabel = @label, Is_Active = 1, LinModifiedBy = @u, AddLastDateChange = GETDATE(), Ntimeschanged = Ntimeschanged + 1 " +
                    "WHERE LinkAspNetUsers = @u AND Is_Delete = 0; " +
                    "ELSE INSERT INTO dbo.TSql_AiUserSettings (LinkAspNetUsers, LinkEnvironment, TextLanguageCode, TextLabel, LinkMadeBy) " +
                    "VALUES (@u, @e, @l, @label, @u);";
                db.Database.ExecuteSqlCommand(sql,
                    new SqlParameter("@u", userId),
                    new SqlParameter("@e", envRow.IdObject),
                    new SqlParameter("@setLang", setLang ? 1 : 0),
                    new SqlParameter("@l", (object)langValue ?? DBNull.Value),
                    new SqlParameter("@label", "IA · entorno " + envRow.TextCode));

                return AiJson(new JObject
                {
                    ["ok"] = true,
                    ["environment"] = AiEnvJson(envRow),
                });
            }
            catch (JsonReaderException ex)
            {
                return AiJson(new JObject { ["ok"] = false, ["message"] = "JSON inválido: " + ex.Message });
            }
            catch (Exception ex)
            {
                return AiJson(new JObject { ["ok"] = false, ["message"] = ex.Message });
            }
        }

        /// <summary>Arma el catálogo (sin «ok» ni «version»). Puro: solo con las filas leídas.</summary>
        internal static JObject BuildAiCatalogPayload(string ctx, string reqLang, AiEnvironmentRow curEnv, List<AiEnvironmentRow> envs,
            List<AiCommandRow> commands, List<AiPhraseRow> phrases, List<AiCommandUserRow> userCmds, List<AiUserPhraseRow> userPhrases,
            List<AiReplyRow> replies, long defEnvId)
        {
            var reqBase = AiBaseLang(reqLang);
            var userByCmd = new Dictionary<long, AiCommandUserRow>();
            foreach (var uc in userCmds)
            {
                userByCmd[uc.LinkAiCommand] = uc;
            }
            var phrasesByCmd = phrases.GroupBy(p => p.LinkAiCommand).ToDictionary(g => g.Key, g => g.ToList());
            var userPhrByCmd = userPhrases.GroupBy(p => p.LinkAiCommand).ToDictionary(g => g.Key, g => g.ToList());

            var cmdArr = new JArray();
            var ordered = commands
                .Select((c, i) => new { c, i, u = userByCmd.ContainsKey(c.IdObject) ? userByCmd[c.IdObject] : null })
                .Where(x => x.u == null || x.u.Is_Active)
                .OrderBy(x => x.u != null && x.u.NumberOrder.HasValue ? x.u.NumberOrder.Value : x.c.NumberOrder)
                .ThenBy(x => x.i);
            foreach (var x in ordered)
            {
                var c = x.c;
                List<AiPhraseRow> defs;
                if (!phrasesByCmd.TryGetValue(c.IdObject, out defs)) defs = new List<AiPhraseRow>();
                List<AiUserPhraseRow> mine;
                if (!userPhrByCmd.TryGetValue(c.IdObject, out mine)) mine = new List<AiUserPhraseRow>();

                var excludedIds = new HashSet<long>(mine.Where(m => m.Is_Exclude && m.LinkAiCommandPhrase.HasValue).Select(m => m.LinkAiCommandPhrase.Value));
                var excludedNorms = new HashSet<string>(mine.Where(m => m.Is_Exclude && !m.LinkAiCommandPhrase.HasValue)
                    .Select(m => (m.TextLanguageCode ?? "") + "|" + (m.TextPhraseNorm ?? "")), StringComparer.Ordinal);
                var useDefaults = x.u == null || x.u.Is_UseDefaultPhrases;

                /* Concepto y peso de las frases propias del usuario: los de las palabras clave del comando. */
                var keyDefs = defs.Where(p => p.TextKind == "keyword").ToList();
                var mainConcept = keyDefs.Where(p => !string.IsNullOrEmpty(p.TextConcept)).GroupBy(p => p.TextConcept)
                    .OrderByDescending(g => g.Count()).Select(g => g.Key).FirstOrDefault();
                decimal? mainWeight = keyDefs.Where(p => p.NumberWeight.HasValue).Select(p => p.NumberWeight).DefaultIfEmpty(null).Max();

                var arr = new JArray();
                var seen = new HashSet<string>(StringComparer.Ordinal);
                if (useDefaults)
                {
                    foreach (var dp in defs)
                    {
                        if (excludedIds.Contains(dp.IdObject)) continue;
                        if (excludedNorms.Contains((dp.TextLanguageCode ?? "") + "|" + (dp.TextPhraseNorm ?? ""))) continue;
                        if (!seen.Add(dp.TextLanguageCode + "|" + dp.TextKind + "|" + dp.TextPhraseNorm)) continue;
                        var kind = dp.TextKind;
                        var concept = dp.TextConcept;
                        /* TextKind sigue siendo keyword (el check de la tabla no admite otro). TextLabel marca el margen. */
                        if (string.Equals(dp.TextLabel, "learned", StringComparison.Ordinal))
                        {
                            kind = "learned";
                            concept = null;
                        }
                        arr.Add(AiPhraseJson(dp.TextPhrase, dp.TextPhraseNorm, kind, concept, dp.NumberWeight, dp.TextLanguageCode,
                            dp.LinkEnvironment.HasValue, false));
                    }
                }
                foreach (var up in mine.Where(m => !m.Is_Exclude))
                {
                    var learned = string.Equals(up.TextLabel, "learned", StringComparison.Ordinal);
                    if (!seen.Add(up.TextLanguageCode + "|" + (learned ? "learned" : "keyword") + "|" + up.TextPhraseNorm)) continue;
                    arr.Add(learned
                        ? AiPhraseJson(up.TextPhrase, up.TextPhraseNorm, "learned", null, null, up.TextLanguageCode, up.LinkEnvironment.HasValue, true)
                        : AiPhraseJson(up.TextPhrase, up.TextPhraseNorm, "keyword", mainConcept, mainWeight, up.TextLanguageCode, up.LinkEnvironment.HasValue, true));
                }

                var cmdJson = new JObject
                {
                    ["code"] = c.TextCode,
                    ["context"] = c.TextContext,
                    ["group"] = c.TextGroup,
                    ["nameEs"] = c.TextNameEs,
                    ["nameEn"] = c.TextNameEn,
                    ["handler"] = c.TextHandler,
                    ["aiId"] = c.TextAiCommandId,
                    ["confirm"] = c.Is_Confirm,
                    ["destructive"] = c.Is_Destructive,
                    ["implemented"] = c.Is_AiImplemented,
                    ["useDefaults"] = useDefaults,
                    ["phrases"] = arr,
                };
                /* Palabras por defecto que el usuario ha quitado (el JS también las quita de su vocabulario). */
                var excludedArr = new JArray(mine.Where(m => m.Is_Exclude && !string.IsNullOrEmpty(m.TextPhraseNorm))
                    .Select(m => m.TextPhraseNorm).Distinct(StringComparer.Ordinal).ToArray());
                if (excludedArr.Count > 0) cmdJson["excluded"] = excludedArr;
                cmdArr.Add(cmdJson);
            }

            /* Plantillas por idioma base: ejecutivo (base ← variante) ← entorno activo (base ← variante). */
            var repliesJson = new JObject();
            var bases = new List<string> { "es", "en" };
            if (!bases.Contains(reqBase)) bases.Add(reqBase);
            var curEnvId = curEnv != null ? curEnv.IdObject : -1L;
            foreach (var b in bases)
            {
                var map = new SortedDictionary<string, string>(StringComparer.Ordinal);
                var layers = new List<Tuple<long, string>>
                {
                    Tuple.Create(defEnvId, b),
                    Tuple.Create(defEnvId, reqBase == b && reqLang != b ? reqLang : null),
                    Tuple.Create(curEnvId, b),
                    Tuple.Create(curEnvId, reqBase == b && reqLang != b ? reqLang : null),
                };
                foreach (var layer in layers)
                {
                    if (layer.Item2 == null) continue;
                    foreach (var rr in replies.Where(r => r.LinkEnvironment == layer.Item1 && string.Equals(r.TextLanguageCode, layer.Item2, StringComparison.OrdinalIgnoreCase)))
                    {
                        map[rr.TextMessageKey] = rr.TextTemplate;
                    }
                }
                var o = new JObject();
                foreach (var kv in map) o[kv.Key] = kv.Value;
                repliesJson[b] = o;
            }

            var envArr = new JArray();
            foreach (var en in envs) envArr.Add(AiEnvJson(en));
            return new JObject
            {
                ["context"] = ctx,
                ["lang"] = reqLang,
                ["environment"] = curEnv != null ? AiEnvJson(curEnv) : null,
                ["environments"] = envArr,
                ["commands"] = cmdArr,
                ["replies"] = repliesJson,
            };
        }

        private bool AiCatalogTablesReady()
        {
            var names = string.Join(",", AiCatalogTables.Select(t => "N'" + t + "'"));
            var tables = db.Database.SqlQuery<int>("SELECT COUNT(*) FROM sys.tables WHERE schema_id = SCHEMA_ID(N'dbo') AND name IN (" + names + ")").FirstOrDefault();
            if (tables != AiCatalogTables.Length) return false;
            var cols = db.Database.SqlQuery<int>(
                "SELECT COUNT(*) FROM sys.columns WHERE name = N'LinkEnvironment' AND object_id IN " +
                "(OBJECT_ID(N'dbo.TSql_AiCommandPhrase'), OBJECT_ID(N'dbo.TSql_AiCommandUserPhrase'))").FirstOrDefault();
            return cols == 2;
        }

        private ContentResult AiJson(JObject o)
        {
            return Content(o.ToString(Formatting.None), "application/json", Encoding.UTF8);
        }

        private static JObject AiEnvJson(AiEnvironmentRow e)
        {
            return new JObject
            {
                ["code"] = e.TextCode,
                ["nameEs"] = e.TextNameEs,
                ["nameEn"] = e.TextNameEn,
                ["description"] = e.TextDescription,
                ["isDefault"] = e.Is_Default,
            };
        }

        private static JObject AiPhraseJson(string text, string norm, string kind, string concept, decimal? weight, string lang, bool env, bool user)
        {
            var o = new JObject { ["p"] = text, ["n"] = norm, ["k"] = kind };
            if (!string.IsNullOrEmpty(concept)) o["c"] = concept;
            if (weight.HasValue) o["w"] = weight.Value;
            o["l"] = lang;
            if (env) o["e"] = 1;
            if (user) o["u"] = 1;
            return o;
        }

        internal static string AiNormalizeLang(string raw)
        {
            var s = (raw ?? "").Trim().ToLowerInvariant().Replace('_', '-');
            return AiLangRegex.IsMatch(s) ? s : null;
        }

        internal static string AiBaseLang(string lang)
        {
            var s = AiNormalizeLang(lang) ?? "es";
            var i = s.IndexOf('-');
            return i > 0 ? s.Substring(0, i) : s;
        }

        private static string AiSha1(string s)
        {
            using (var sha = SHA1.Create())
            {
                var hash = sha.ComputeHash(Encoding.UTF8.GetBytes(s));
                var sb = new StringBuilder(16);
                for (var i = 0; i < 8; i++) sb.Append(hash[i].ToString("x2"));
                return sb.ToString();
            }
        }
    }

    /* Filas de las consultas del catálogo (propiedades = columnas; tipos exactos para SqlQuery). */
    public sealed class AiEnvironmentRow
    {
        public long IdObject { get; set; }
        public string TextCode { get; set; }
        public string TextNameEs { get; set; }
        public string TextNameEn { get; set; }
        public string TextDescription { get; set; }
        public bool Is_Default { get; set; }
        public int NumberOrder { get; set; }
    }

    public sealed class AiUserSettingsRow
    {
        public long? LinkEnvironment { get; set; }
        public string TextLanguageCode { get; set; }
    }

    public sealed class AiCommandRow
    {
        public long IdObject { get; set; }
        public string TextCode { get; set; }
        public string TextContext { get; set; }
        public string TextGroup { get; set; }
        public string TextNameEs { get; set; }
        public string TextNameEn { get; set; }
        public string TextHandler { get; set; }
        public string TextAiCommandId { get; set; }
        public bool Is_Destructive { get; set; }
        public bool Is_Confirm { get; set; }
        public bool Is_AiImplemented { get; set; }
        public int NumberOrder { get; set; }
    }

    public sealed class AiPhraseRow
    {
        public long IdObject { get; set; }
        public long LinkAiCommand { get; set; }
        public string TextLanguageCode { get; set; }
        public string TextPhrase { get; set; }
        public string TextPhraseNorm { get; set; }
        public string TextKind { get; set; }
        public string TextConcept { get; set; }
        public decimal? NumberWeight { get; set; }
        public long? LinkEnvironment { get; set; }
        public int NumberOrder { get; set; }
        public string TextLabel { get; set; }
    }

    public sealed class AiLearnCommandRow
    {
        public long IdObject { get; set; }
        public string TextCode { get; set; }
        public string TextContext { get; set; }
        public string TextHandler { get; set; }
        public string TextAiCommandId { get; set; }
        public bool Is_AiImplemented { get; set; }
    }

    public sealed class AiCommandUserRow
    {
        public long LinkAiCommand { get; set; }
        public bool Is_UseDefaultPhrases { get; set; }
        public int? NumberOrder { get; set; }
        public bool Is_Active { get; set; }
    }

    public sealed class AiUserPhraseRow
    {
        public long IdObject { get; set; }
        public long LinkAiCommand { get; set; }
        public long? LinkAiCommandPhrase { get; set; }
        public string TextLanguageCode { get; set; }
        public string TextPhrase { get; set; }
        public string TextPhraseNorm { get; set; }
        public bool Is_Exclude { get; set; }
        public long? LinkEnvironment { get; set; }
        public string TextLabel { get; set; }
    }

    public sealed class AiReplyRow
    {
        public long LinkEnvironment { get; set; }
        public string TextLanguageCode { get; set; }
        public string TextMessageKey { get; set; }
        public string TextTemplate { get; set; }
    }
}
