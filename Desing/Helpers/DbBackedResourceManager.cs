using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Configuration;
using System.Data.Entity;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Resources;
using DAL;

namespace Desing.Helpers
{
    /// <summary>
    /// <see cref="ResourceManager"/> que primero consulta <c>TSql_UiTranslation</c>
    /// y, si no encuentra valor, delega en el <c>.resx</c> embebido (comportamiento estandar).
    ///
    /// Permite editar las cadenas de un modulo (p. ej. <c>Company</c>) en caliente sin
    /// recompilar: lo importado/editado en BD prevalece, y el <c>.resx</c> queda como
    /// semilla y fallback. Cachea por <c>(modulo, codigo idioma)</c> y se invalida tras
    /// cada importacion via <see cref="Invalidate"/>.
    /// </summary>
    public sealed class DbBackedResourceManager : ResourceManager
    {
        /// <summary>Valor usado como <c>TextModule</c> al filtrar filas en BD.</summary>
        public string TextModule { get; }

        // Cache: (module, normalized textCode) -> { resourceKey -> textValue }
        private static readonly ConcurrentDictionary<string, Dictionary<string, string>> _cache =
            new ConcurrentDictionary<string, Dictionary<string, string>>(StringComparer.OrdinalIgnoreCase);
        private static readonly ConcurrentDictionary<string, long> _languageIdCache =
            new ConcurrentDictionary<string, long>(StringComparer.OrdinalIgnoreCase);

        public DbBackedResourceManager(string baseName, Assembly assembly, string textModule)
            : base(baseName, assembly)
        {
            TextModule = string.IsNullOrWhiteSpace(textModule) ? "" : textModule.Trim();
        }

        /// <summary>
        /// Limpia la cache. Llamar despues de un import o cambio masivo en
        /// <c>TSql_UiTranslation</c> para que los siguientes <c>GetString</c> vean los nuevos valores.
        /// </summary>
        public static void Invalidate()
        {
            _cache.Clear();
            _languageIdCache.Clear();
        }

        public override string GetString(string name, CultureInfo culture)
        {
            if (string.IsNullOrWhiteSpace(name))
                return base.GetString(name, culture);

            try
            {
                var code = NormalizeCode(culture);
                var dbValue = Lookup(TextModule, code, name);
                if (!string.IsNullOrEmpty(dbValue))
                    return dbValue;
            }
            catch
            {
                /* nunca dejar caer una vista por error en la BD: cae al .resx */
            }

            return base.GetString(name, culture);
        }

        public override string GetString(string name)
        {
            return GetString(name, CultureInfo.CurrentUICulture);
        }

        private static string NormalizeCode(CultureInfo culture)
        {
            if (culture == null || culture.Equals(CultureInfo.InvariantCulture))
                return "";

            var iso = culture.TwoLetterISOLanguageName;
            if (string.IsNullOrEmpty(iso) || string.Equals(iso, "iv", StringComparison.OrdinalIgnoreCase))
                iso = culture.Name;

            return (iso ?? "").Trim().ToLowerInvariant();
        }

        private static string Lookup(string module, string code, string key)
        {
            if (string.IsNullOrEmpty(module))
                return null;

            var cacheKey = module + "|" + (code ?? "");
            var dict = _cache.GetOrAdd(cacheKey, _ => Load(module, code));
            if (dict == null)
                return null;

            string v;
            return dict.TryGetValue(key, out v) ? v : null;
        }

        private static Dictionary<string, string> Load(string module, string code)
        {
            var sw = Stopwatch.StartNew();
            var result = new Dictionary<string, string>(StringComparer.Ordinal);
            try
            {
                using (var db = new ConexionData())
                {
                    var langId = ResolveLanguageId(db, code);

                    if (!langId.HasValue)
                        return result;

                    var rows = db.TSql_UiTranslation.AsNoTracking()
                        .Where(t => t.TextModule == module
                                 && t.LinkLanguage == langId.Value
                                 && !t.Is_Delete
                                 && t.Is_Active)
                        .Select(t => new { t.TextResourceKey, t.TextValue })
                        .ToList();

                    foreach (var r in rows)
                    {
                        if (!string.IsNullOrEmpty(r.TextResourceKey)
                            && !string.IsNullOrEmpty(r.TextValue)
                            && !result.ContainsKey(r.TextResourceKey))
                        {
                            result[r.TextResourceKey] = r.TextValue;
                        }
                    }
                }
            }
            catch
            {
                /* devolver lo cargado hasta ahora; en peor caso, diccionario vacio */
            }

            sw.Stop();
            TraceStartupTiming(
                "DbBackedResourceManager.Load module=" + module + " code=" + (code ?? "") + " rows=" + result.Count,
                sw.ElapsedMilliseconds);
            return result;
        }

        private static long? ResolveLanguageId(ConexionData db, string code)
        {
            var normalizedCode = (code ?? "").Trim().ToLowerInvariant();
            var cacheKey = string.IsNullOrEmpty(normalizedCode) ? "__default__" : normalizedCode;
            long cached;
            if (_languageIdCache.TryGetValue(cacheKey, out cached))
                return cached > 0 ? (long?)cached : null;

            long? langId;
            if (string.IsNullOrEmpty(normalizedCode))
            {
                langId = db.TSql_language.AsNoTracking()
                    .Where(l => l.Is_Default && !l.Is_Delete && l.Is_Active)
                    .Select(l => (long?)l.IdObject)
                    .FirstOrDefault();
            }
            else
            {
                langId = db.TSql_language.AsNoTracking()
                    .Where(l => l.TextCode == normalizedCode && !l.Is_Delete && l.Is_Active)
                    .Select(l => (long?)l.IdObject)
                    .FirstOrDefault();
            }

            _languageIdCache[cacheKey] = langId.GetValueOrDefault();
            return langId;
        }

        private static void TraceStartupTiming(string label, long elapsedMs)
        {
            if (!string.Equals(ConfigurationManager.AppSettings["TandemStartupTiming"], "true", StringComparison.OrdinalIgnoreCase))
                return;

            Debug.WriteLine("[TandemStartupTiming] " + label + " = " + elapsedMs + " ms");
        }
    }
}
