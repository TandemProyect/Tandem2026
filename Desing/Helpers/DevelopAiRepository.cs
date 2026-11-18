using DAL;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Globalization;
using System.Linq;

namespace Desing.Helpers
{
    /// <summary>Fila del catálogo IA (TSql_AiCommand + frases del idioma elegido).</summary>
    public class DevelopAiCommandRow
    {
        public long IdObject { get; set; }
        public string TextCode { get; set; }
        public string TextGroup { get; set; }
        public string TextName { get; set; }
        public string TextOrders { get; set; }
        public string TextMeaning { get; set; }
        public string TextHandler { get; set; }
        public bool IsAiImplemented { get; set; }
        public int NumberOrder { get; set; }
    }

    /// <summary>
    /// Lectura del catálogo IA para Develop › IA. Las frases de una variante
    /// (es-ar) incluyen las del idioma base (es), igual que el intérprete.
    /// </summary>
    public static class DevelopAiRepository
    {
        public static bool TableExists(ConexionData db)
        {
            return db.Database.SqlQuery<int>(
                "SELECT CASE WHEN OBJECT_ID(N'dbo.TSql_AiCommand', N'U') IS NULL THEN 0 ELSE 1 END").First() == 1;
        }

        public static List<string> ListLanguages(ConexionData db)
        {
            if (!TableExists(db))
            {
                return new List<string>();
            }

            return db.TSql_AiCommandPhrase.AsNoTracking()
                .Where(p => !p.Is_Delete && p.Is_Active && p.TextLanguageCode != null && p.TextLanguageCode != "")
                .Select(p => p.TextLanguageCode)
                .Distinct()
                .ToList()
                .Select(c => c.Trim().ToLowerInvariant())
                .Where(c => c.Length > 0)
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .OrderBy(c => c, StringComparer.OrdinalIgnoreCase)
                .ToList();
        }

        public static string ResolveLanguage(IList<string> languages, string requested, CultureInfo ui)
        {
            if (languages == null || languages.Count == 0)
            {
                return "es";
            }

            var candidates = new[]
            {
                requested,
                ui == null ? null : ui.Name,
                ui == null ? null : ui.TwoLetterISOLanguageName,
                "es"
            };

            foreach (var candidate in candidates)
            {
                var match = MatchLanguage(languages, candidate);
                if (match != null)
                {
                    return match;
                }
            }

            return languages[0];
        }

        public static List<DevelopAiCommandRow> ListCommands(ConexionData db, string language)
        {
            if (!TableExists(db))
            {
                return new List<DevelopAiCommandRow>();
            }

            var lang = string.IsNullOrWhiteSpace(language) ? "es" : language.Trim().ToLowerInvariant();
            var commands = db.TSql_AiCommand.AsNoTracking()
                .Where(c => !c.Is_Delete && c.Is_Active)
                .Select(c => new
                {
                    c.IdObject,
                    c.TextCode,
                    c.TextGroup,
                    c.TextNameEs,
                    c.TextNameEn,
                    c.TextDescription,
                    c.TextHandler,
                    c.TextParamsJson,
                    c.Is_AiImplemented,
                    c.NumberOrder
                })
                .ToList();

            var phrases = db.TSql_AiCommandPhrase.AsNoTracking()
                .Where(p => !p.Is_Delete && p.Is_Active)
                .Select(p => new
                {
                    p.LinkAiCommand,
                    p.TextLanguageCode,
                    p.TextPhrase,
                    p.NumberOrder,
                    p.LinkEnvironment
                })
                .ToList();

            var environments = db.TSql_AiEnvironment.AsNoTracking()
                .Where(e => !e.Is_Delete)
                .Select(e => new { e.IdObject, e.TextNameEs, e.TextNameEn })
                .ToList()
                .ToDictionary(e => e.IdObject, e => LocalizedName(e.TextNameEs, e.TextNameEn, lang));

            var byCommand = phrases
                .Where(p => PhraseMatchesLanguage(p.TextLanguageCode, lang))
                .GroupBy(p => p.LinkAiCommand)
                .ToDictionary(g => g.Key, g => g.OrderBy(p => p.NumberOrder).ThenBy(p => p.TextPhrase).ToList());

            return commands.Select(c =>
            {
                var name = LocalizedName(c.TextNameEs, c.TextNameEn, lang);
                var orders = new List<string>();
                if (byCommand.TryGetValue(c.IdObject, out var list))
                {
                    foreach (var phrase in list)
                    {
                        var text = (phrase.TextPhrase ?? "").Trim();
                        if (text.Length == 0)
                        {
                            continue;
                        }

                        if (phrase.LinkEnvironment.HasValue
                            && environments.TryGetValue(phrase.LinkEnvironment.Value, out var env)
                            && !string.IsNullOrWhiteSpace(env))
                        {
                            text += " [" + env + "]";
                        }

                        orders.Add(text);
                    }
                }

                return new DevelopAiCommandRow
                {
                    IdObject = c.IdObject,
                    TextCode = c.TextCode ?? "",
                    TextGroup = c.TextGroup ?? "",
                    TextName = name,
                    TextOrders = string.Join(", ", orders),
                    TextMeaning = BuildMeaning(c.TextDescription, c.TextParamsJson, name),
                    TextHandler = c.TextHandler ?? "",
                    IsAiImplemented = c.Is_AiImplemented,
                    NumberOrder = c.NumberOrder
                };
            }).ToList();
        }

        public static string LocalizedName(string nameEs, string nameEn, string language)
        {
            var useEn = UsesEnglish(language);
            var primary = useEn ? nameEn : nameEs;
            var fallback = useEn ? nameEs : nameEn;
            if (!string.IsNullOrWhiteSpace(primary))
            {
                return primary.Trim();
            }

            return string.IsNullOrWhiteSpace(fallback) ? "" : fallback.Trim();
        }

        public static bool PhraseMatchesLanguage(string phraseLanguage, string selected)
        {
            var phrase = (phraseLanguage ?? "").Trim().ToLowerInvariant();
            var sel = string.IsNullOrWhiteSpace(selected) ? "es" : selected.Trim().ToLowerInvariant();
            if (phrase.Length == 0)
            {
                return false;
            }

            if (phrase == sel)
            {
                return true;
            }

            var dash = sel.IndexOf('-');
            return dash > 0 && phrase == sel.Substring(0, dash);
        }

        public static string BuildMeaning(string description, string paramsJson, string localizedName)
        {
            if (!string.IsNullOrWhiteSpace(description))
            {
                return description.Trim();
            }

            if (string.IsNullOrWhiteSpace(paramsJson))
            {
                return localizedName ?? "";
            }

            try
            {
                var items = JArray.Parse(paramsJson);
                var parts = new List<string>();
                foreach (var token in items)
                {
                    var help = (string)token["help"];
                    if (string.IsNullOrWhiteSpace(help))
                    {
                        continue;
                    }

                    var name = ((string)token["name"] ?? "").Trim();
                    var required = token["required"] != null && token["required"].Type == JTokenType.Boolean && (bool)token["required"];
                    parts.Add(string.IsNullOrEmpty(name) ? help.Trim() : name + (required ? "*: " : ": ") + help.Trim());
                }

                return parts.Count == 0 ? (localizedName ?? "") : string.Join(" · ", parts);
            }
            catch (Exception)
            {
                return localizedName ?? "";
            }
        }

        private static bool UsesEnglish(string language)
        {
            var lang = (language ?? "").Trim().ToLowerInvariant();
            return lang == "en" || lang.StartsWith("en-", StringComparison.Ordinal);
        }

        private static string MatchLanguage(IList<string> languages, string candidate)
        {
            if (string.IsNullOrWhiteSpace(candidate))
            {
                return null;
            }

            var wanted = candidate.Trim().ToLowerInvariant();
            return languages.FirstOrDefault(l => string.Equals(l, wanted, StringComparison.OrdinalIgnoreCase));
        }
    }
}
