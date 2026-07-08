using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Globalization;
using System.Linq;
using System.Security.Principal;
using System.Threading;
using System.Web;
using System.Web.Caching;
using DAL;
using Desing.Models;
using Microsoft.AspNet.Identity;

namespace Desing.Helpers
{
    /// <summary>
    /// Cookie UI + consulta de idiomas activos y rutas de bandera (TSql_Countrys.TextFlag).
    /// Cookie principal: <see cref="LanguageCookieName"/> (TextCode o token i:IdObject / IdObject numérico).
    /// Compatibilidad: <see cref="LegacyUiCultureCookieName"/>.
    /// </summary>
    public static class LanguageUiHelper
    {
        /// <summary>Cookie persistente con idioma UI (TextCode preferente; ver <see cref="TryParseLanguageToken"/>).</summary>
        public const string LanguageCookieName = "tandem_lang";

        /// <summary>Cookie histórica; se mantiene sincronizada al escribir el idioma.</summary>
        public const string LegacyUiCultureCookieName = "tandem_ui_culture";

        [Obsolete("Usar LanguageCookieName o LegacyUiCultureCookieName.")]
        public const string UiCultureCookieName = LegacyUiCultureCookieName;

        /// <summary><see cref="System.Web.HttpContext.Current"/>.Items: idioma UI fijado por empresa (IdObject <see cref="TSql_language"/>).</summary>
        public const string ItemKeyCompanyLanguageId = "TandemCompanyLanguageId";

        /// <summary>Items: <see cref="TSql_language.TextCode"/> impuesto por empresa.</summary>
        public const string ItemKeyCompanyLanguageCode = "TandemCompanyLanguageCode";

        /// <summary>Items: si es true, el selector manual de idioma no aplica (la empresa manda).</summary>
        public const string ItemKeyCompanyLanguageLocked = "TandemCompanyLanguageLocked";

        private const string CacheKeyPrefix = "TandemLangCode_Id_";
        private const string CacheKeyLanguageIdByCodePrefix = "TandemLangId_Code_";

        /// <summary>
        /// Convierte rutas tipo content/images/... a ~/Content/... para Url.Content.
        /// </summary>
        public static string NormalizeCountryFlagVirtualPath(string raw)
        {
            if (string.IsNullOrWhiteSpace(raw))
                return null;

            var p = raw.Trim().Replace('\\', '/');
            if (p.StartsWith("~/", StringComparison.Ordinal))
                p = p.Substring(2).TrimStart('/');
            while (p.StartsWith("../", StringComparison.Ordinal))
                p = p.Substring(3);

            p = p.TrimStart('/');
            if (p.StartsWith("content/", StringComparison.OrdinalIgnoreCase))
                p = "Content/" + p.Substring("content/".Length);
            else if (!p.StartsWith("Content/", StringComparison.OrdinalIgnoreCase))
                p = "Content/" + p.TrimStart('/');

            return "~/" + p.TrimStart('/');
        }

        public static CultureInfo ResolveCulture(string textCode)
        {
            if (string.IsNullOrWhiteSpace(textCode))
                textCode = "es";

            textCode = textCode.Trim();
            try
            {
                return CultureInfo.CreateSpecificCulture(textCode);
            }
            catch (CultureNotFoundException)
            {
                try
                {
                    return new CultureInfo(textCode);
                }
                catch (CultureNotFoundException)
                {
                    return CultureInfo.CreateSpecificCulture("es");
                }
            }
        }

        /// <summary>
        /// Valor bruto de cookie idioma (sin resolver IdObject a TextCode).
        /// </summary>
        public static string ReadRawLanguageCookieValue(HttpRequestBase request)
        {
            try
            {
                if (request == null)
                    return null;
                var primary = request.Cookies[LanguageCookieName];
                if (primary != null && !string.IsNullOrWhiteSpace(primary.Value))
                    return primary.Value.Trim();
                var legacy = request.Cookies[LegacyUiCultureCookieName];
                if (legacy != null && !string.IsNullOrWhiteSpace(legacy.Value))
                    return legacy.Value.Trim();
            }
            catch
            {
                /* ignorar */
            }

            return null;
        }

        /// <summary>
        /// Interpreta el token de cookie: TextCode libre, <c>i:123</c>, o solo dígitos como IdObject.
        /// </summary>
        public static bool TryParseLanguageToken(string raw, out long? languageId, out string textCodeOrNull)
        {
            languageId = null;
            textCodeOrNull = null;
            if (string.IsNullOrWhiteSpace(raw))
                return false;
            raw = raw.Trim();
            if (raw.StartsWith("i:", StringComparison.OrdinalIgnoreCase))
            {
                var idPart = raw.Substring(2).Trim();
                long id;
                if (long.TryParse(idPart, out id) && id > 0)
                {
                    languageId = id;
                    return true;
                }
                return false;
            }
            if (raw.Length > 0 && raw.All(char.IsDigit))
            {
                long id;
                if (long.TryParse(raw, out id) && id > 0)
                {
                    languageId = id;
                    return true;
                }
            }
            textCodeOrNull = raw;
            return true;
        }

        private static string LookupTextCodeByLanguageId(long id)
        {
            var cache = HttpRuntime.Cache;
            var key = CacheKeyPrefix + id;
            var hit = cache[key] as string;
            if (!string.IsNullOrEmpty(hit))
                return hit;

            try
            {
                using (var db = new ConexionData())
                {
                    var code = db.Database.SqlQuery<string>(
                        @"SELECT TOP 1 TextCode FROM dbo.TSql_Language
                          WHERE IdObject = @p0 AND Is_Delete = 0 AND Is_Active = 1",
                        id).FirstOrDefault();
                    if (!string.IsNullOrWhiteSpace(code))
                    {
                        code = code.Trim();
                        cache.Insert(key, code, null, Cache.NoAbsoluteExpiration, TimeSpan.FromMinutes(15));
                        return code;
                    }
                }
            }
            catch
            {
                /* ignorar */
            }

            return null;
        }

        /// <summary>
        /// Código de cultura UI (ej. es, en) ya resuelto desde cookie (incl. IdObject).
        /// </summary>
        public static string ReadResolvedUiCultureCode(HttpRequestBase request)
        {
            try
            {
                var ctx = HttpContext.Current;
                if (ctx != null)
                {
                    var forced = ctx.Items[ItemKeyCompanyLanguageCode] as string;
                    if (!string.IsNullOrWhiteSpace(forced))
                        return forced.Trim();
                }
            }
            catch
            {
                /* ignorar */
            }

            var raw = ReadRawLanguageCookieValue(request);
            if (string.IsNullOrWhiteSpace(raw))
                return "es";

            long? langId;
            string codeToken;
            if (!TryParseLanguageToken(raw, out langId, out codeToken))
                return "es";

            if (langId.HasValue)
            {
                var fromId = LookupTextCodeByLanguageId(langId.Value);
                return string.IsNullOrWhiteSpace(fromId) ? "es" : fromId.Trim();
            }

            return string.IsNullOrWhiteSpace(codeToken) ? "es" : codeToken.Trim();
        }

        [Obsolete("Usar ReadResolvedUiCultureCode.")]
        public static string ReadUiCultureCode(HttpRequestBase request)
        {
            return ReadResolvedUiCultureCode(request);
        }

        /// <summary>
        /// Aplica cultura al hilo actual. Devuelve el TextCode efectivo.
        /// </summary>
        public static string ApplyCultureFromCookie(HttpRequestBase request)
        {
            var code = ReadResolvedUiCultureCode(request);
            try
            {
                var ci = ResolveCulture(code);
                System.Threading.Thread.CurrentThread.CurrentCulture = ci;
                System.Threading.Thread.CurrentThread.CurrentUICulture = ci;
            }
            catch
            {
                try
                {
                    var ci = CultureInfo.CreateSpecificCulture("es");
                    System.Threading.Thread.CurrentThread.CurrentCulture = ci;
                    System.Threading.Thread.CurrentThread.CurrentUICulture = ci;
                }
                catch { /* ignorar */ }
            }

            return code;
        }

        /// <summary>
        /// Desde <see cref="HttpRequest"/> (Global.asax).
        /// </summary>
        public static void ApplyCultureEarly(HttpRequest request)
        {
            if (request == null)
                return;
            ApplyCultureFromCookie(new HttpRequestWrapper(request));
        }

        /// <summary>
        /// Fija cultura en el hilo actual sin depender de cookies (misma petición, p. ej. idioma por empresa).
        /// </summary>
        public static void ApplyCultureExplicit(string textCode)
        {
            try
            {
                var ci = ResolveCulture(string.IsNullOrWhiteSpace(textCode) ? "es" : textCode.Trim());
                Thread.CurrentThread.CurrentCulture = ci;
                Thread.CurrentThread.CurrentUICulture = ci;
            }
            catch
            {
                try
                {
                    var ci = CultureInfo.CreateSpecificCulture("es");
                    Thread.CurrentThread.CurrentCulture = ci;
                    Thread.CurrentThread.CurrentUICulture = ci;
                }
                catch
                {
                    /* ignorar */
                }
            }
        }

        /// <summary>
        /// Escribe cookies de idioma (principal + legacy) con TextCode normalizado.
        /// </summary>
        public static void WriteLanguageCookies(HttpResponseBase response, string textCode)
        {
            if (response == null)
                return;

            if (string.IsNullOrWhiteSpace(textCode))
                textCode = "es";

            textCode = textCode.Trim();

            void Set(string name, string value)
            {
                var cookie = new HttpCookie(name, value)
                {
                    Expires = DateTime.UtcNow.AddYears(1),
                    HttpOnly = true,
                    Path = "/"
                };
                response.Cookies.Set(cookie);
            }

            Set(LanguageCookieName, textCode);
            Set(LegacyUiCultureCookieName, textCode);
        }

        [Obsolete("Usar WriteLanguageCookies.")]
        public static void WriteCultureCookie(HttpResponseBase response, string textCode)
        {
            WriteLanguageCookies(response, textCode);
        }

        /// <summary>
        /// IdObject de <see cref="TSql_language"/> activo para la cookie actual, o null.
        /// </summary>
        public static long? TryResolveLanguageId(ConexionData db, HttpRequestBase request)
        {
            try
            {
                var ctx = HttpContext.Current;
                if (ctx != null && ctx.Items[ItemKeyCompanyLanguageId] is long lidCompany && lidCompany > 0)
                    return lidCompany;
            }
            catch
            {
                /* ignorar */
            }

            if (db == null || request == null)
                return null;

            var code = ReadResolvedUiCultureCode(request);
            try
            {
                var cache = HttpRuntime.Cache;
                var cacheKey = CacheKeyLanguageIdByCodePrefix + (code ?? "").Trim().ToLowerInvariant();
                var cached = cache[cacheKey];
                if (cached is long)
                    return (long)cached;

                var id = db.TSql_language
                    .AsNoTracking()
                    .Where(l => l.TextCode == code && !l.Is_Delete && l.Is_Active)
                    .Select(l => (long?)l.IdObject)
                    .FirstOrDefault();
                if (id.HasValue)
                {
                    cache.Insert(cacheKey, id.Value, null, Cache.NoAbsoluteExpiration, TimeSpan.FromMinutes(15));
                }
                return id;
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// Texto UI para la clave en el idioma resuelto (primera coincidencia activa y no borrada).
        /// </summary>
        public static string TryGetUiString(ConexionData db, HttpRequestBase request, string resourceKey, string module = null)
        {
            if (db == null || string.IsNullOrWhiteSpace(resourceKey))
                return null;

            var langId = TryResolveLanguageId(db, request);
            if (!langId.HasValue)
                return null;

            try
            {
                var q = db.TSql_UiTranslation.Where(t =>
                    t.TextResourceKey == resourceKey &&
                    t.LinkLanguage == langId.Value &&
                    !t.Is_Delete &&
                    t.Is_Active);

                if (!string.IsNullOrWhiteSpace(module))
                    q = q.Where(t => t.TextModule == module);

                return q.Select(t => t.TextValue).FirstOrDefault();
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// Texto UI: idioma de la cookie; si no hay fila, mismo <paramref name="resourceKey"/> en el idioma por defecto de BD;
        /// si tampoco existe, <paramref name="fallbackLiteral"/>.
        /// </summary>
        public static string GetUiStringWithFallback(ConexionData db, HttpRequestBase request, string resourceKey, string fallbackLiteral, string module = null)
        {
            if (string.IsNullOrWhiteSpace(resourceKey))
                return fallbackLiteral ?? string.Empty;

            var primary = TryGetUiString(db, request, resourceKey, module);
            if (!string.IsNullOrWhiteSpace(primary))
                return primary;

            try
            {
                var defLangId = db.TSql_language
                    .Where(l => l.Is_Default && !l.Is_Delete && l.Is_Active)
                    .Select(l => (long?)l.IdObject)
                    .FirstOrDefault();
                if (!defLangId.HasValue)
                    return fallbackLiteral ?? string.Empty;

                var q = db.TSql_UiTranslation.Where(t =>
                    t.TextResourceKey == resourceKey &&
                    t.LinkLanguage == defLangId.Value &&
                    !t.Is_Delete &&
                    t.Is_Active);

                if (!string.IsNullOrWhiteSpace(module))
                    q = q.Where(t => t.TextModule == module);

                var defText = q.Select(t => t.TextValue).FirstOrDefault();
                return string.IsNullOrWhiteSpace(defText) ? (fallbackLiteral ?? string.Empty) : defText;
            }
            catch
            {
                return fallbackLiteral ?? string.Empty;
            }
        }

        /// <summary>
        /// Si el usuario es empleado y su empresa tiene <see cref="TSql_Company.LinkLanguage"/>, devuelve ese idioma (bloquea selector manual).
        /// </summary>
        public static bool TryGetLockedCompanyUiLanguage(ConexionData db, IPrincipal user, out long? langId, out string textCode)
        {
            langId = null;
            textCode = null;
            if (db == null || user?.Identity?.IsAuthenticated != true)
                return false;

            var idUser = user.Identity.GetUserId();
            if (string.IsNullOrEmpty(idUser))
                return false;

            try
            {
                var employee = db.TSql_Employee.AsNoTracking().FirstOrDefault(e => e.LinAspNetUsert == idUser);
                if (employee == null)
                    return false;

                var company = db.TSql_Company.AsNoTracking().FirstOrDefault(c =>
                    c.SysObjectID == employee.LinCompany && !c.BitIsDeleted);
                if (company?.LinkLanguage == null)
                    return false;

                var lang = db.TSql_language.AsNoTracking().FirstOrDefault(l =>
                    l.IdObject == company.LinkLanguage.Value && !l.Is_Delete && l.Is_Active);
                if (lang == null || string.IsNullOrWhiteSpace(lang.TextCode))
                    return false;

                langId = lang.IdObject;
                textCode = lang.TextCode.Trim();
                return true;
            }
            catch
            {
                return false;
            }
        }

        public static IList<LanguageNavItem> TryGetActiveLanguages(ConexionData db)
        {
            if (db == null)
                return new List<LanguageNavItem>();

            const string sql = @"
SELECT l.IdObject AS IdObject, l.TextCode AS TextCode, l.TextLabel AS TextLabel,
       CAST(l.Is_Default AS BIT) AS IsDefault, c.TextFlag AS TextFlagRaw
FROM dbo.TSql_Language l
LEFT JOIN dbo.TSql_Countrys c ON c.IdObject = l.LinkCountry
WHERE l.Is_Delete = 0 AND l.Is_Active = 1
ORDER BY l.Is_Default DESC, l.TextLabel";

            try
            {
                return db.Database.SqlQuery<LanguageNavItem>(sql).ToList();
            }
            catch
            {
                return new List<LanguageNavItem>();
            }
        }

        /// <summary>
        /// TextCode del idioma marcado por defecto en BD, o null.
        /// </summary>
        public static string TryGetDefaultLanguageTextCode(ConexionData db)
        {
            if (db == null)
                return null;
            try
            {
                return db.TSql_language
                    .Where(l => l.Is_Default && !l.Is_Delete && l.Is_Active)
                    .OrderByDescending(l => l.IdObject)
                    .Select(l => l.TextCode)
                    .FirstOrDefault();
            }
            catch
            {
                return null;
            }
        }
    }
}
