using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace Desing.Helpers
{
    /// <summary>
    /// Catálogo de comandos conocidos (para sugerir el <c>TextCode</c> al crear una función):
    /// controladores y acciones MVC por reflexión + comandos JS de Desing_2
    /// (<c>data-ma-stl-tool</c>, <c>data-ma-stl-mode</c>, <c>data-ma-system</c>, <c>data-plugin-menu</c>).
    /// </summary>
    public static class FunctionCatalog
    {
        public const string JsModule = "Desing_2 (comandos)";

        /// <summary>Asistentes IA del visor: botón IA, pestañas «Asistente IA» y endpoints <c>Desing_2/Ai*</c>.</summary>
        public const string AiCode = "IA";

        /// <summary>Botón «Traducción simultánea» del chat IA (JS). Si la función está activa, solo la ven los roles que la tengan.</summary>
        public const string AiSimultaneousTranslationCode = "IA.TraduccionSimultanea";

        public sealed class Entry
        {
            public string Code { get; set; }
            public string Module { get; set; }
        }

        private static readonly string[] JsTools =
        {
            "copy", "delete", "extend", "insert-corner", "insert-curved-wall", "insert-enclosure",
            "insert-worker", "move", "offset", "ortho15", "pillars", "polyline", "stretch", "trim", "wall-2d"
        };

        private static readonly string[] JsModes = { "formwork", "lines", "wall-3d" };
        private static readonly string[] JsSystems = { "Atk-60", "Sistema 2", "Sistema 3", "Sistema 4", "Sistema 5" };
        private static readonly string[] JsPluginMenus = { "convert-blocks", "save-walls", "show-blocks", "show-home", "update-library" };

        private static readonly Lazy<IList<Entry>> All = new Lazy<IList<Entry>>(Build);

        public static IList<Entry> Entries => All.Value;

        /// <summary>Módulo sugerido para un código (controlador o «Desing_2 (comandos)»).</summary>
        public static string ModuleFor(string code)
        {
            if (string.IsNullOrWhiteSpace(code))
            {
                return null;
            }

            var match = Entries.FirstOrDefault(e => string.Equals(e.Code, code.Trim(), StringComparison.OrdinalIgnoreCase));
            if (match != null)
            {
                return match.Module;
            }

            var dot = code.IndexOf('.');
            return dot > 0 ? code.Substring(0, dot) : code.Trim();
        }

        private static IList<Entry> Build()
        {
            var list = new List<Entry>();
            var controllers = typeof(FunctionCatalog).Assembly.GetTypes()
                .Where(t => typeof(Controller).IsAssignableFrom(t) && !t.IsAbstract
                            && t.Name.EndsWith("Controller", StringComparison.Ordinal)
                            && t.Name != "BaseController")
                .OrderBy(t => t.Name);

            foreach (var type in controllers)
            {
                var name = type.Name.Substring(0, type.Name.Length - "Controller".Length);
                list.Add(new Entry { Code = name, Module = name });

                var actions = type.GetMethods(BindingFlags.Instance | BindingFlags.Public)
                    .Where(m => !m.IsSpecialName
                                && m.DeclaringType != null
                                && typeof(Controller).IsAssignableFrom(m.DeclaringType)
                                && m.DeclaringType != typeof(Controller)
                                && m.DeclaringType.Name != "BaseController"
                                && IsActionResult(m.ReturnType)
                                && !m.IsDefined(typeof(NonActionAttribute), true))
                    .Select(m =>
                    {
                        var alias = m.GetCustomAttribute<ActionNameAttribute>(true);
                        return alias != null ? alias.Name : m.Name;
                    })
                    .Distinct(StringComparer.OrdinalIgnoreCase)
                    .OrderBy(n => n);

                list.AddRange(actions.Select(a => new Entry { Code = name + "." + a, Module = name }));

                var required = type.GetCustomAttributes<RequireFunctionAttribute>(true)
                    .Concat(type.GetMethods(BindingFlags.Instance | BindingFlags.Public)
                        .SelectMany(m => m.GetCustomAttributes<RequireFunctionAttribute>(true)))
                    .Select(a => a.Code)
                    .Where(c => !string.IsNullOrWhiteSpace(c))
                    .Distinct(StringComparer.OrdinalIgnoreCase);
                list.AddRange(required.Select(c => new Entry { Code = c, Module = name }));
            }

            list.Add(new Entry { Code = AiCode, Module = JsModule });
            list.Add(new Entry { Code = AiSimultaneousTranslationCode, Module = JsModule });
            list.AddRange(JsTools.Select(t => new Entry { Code = "Tool." + t, Module = JsModule }));
            list.AddRange(JsModes.Select(t => new Entry { Code = "Mode." + t, Module = JsModule }));
            list.AddRange(JsSystems.Select(t => new Entry { Code = "System." + t, Module = JsModule }));
            list.AddRange(JsPluginMenus.Select(t => new Entry { Code = "Plugin." + t, Module = JsModule }));
            return list;
        }

        private static bool IsActionResult(Type t)
        {
            if (typeof(ActionResult).IsAssignableFrom(t))
            {
                return true;
            }

            return t.IsGenericType
                && t.GetGenericTypeDefinition() == typeof(Task<>)
                && typeof(ActionResult).IsAssignableFrom(t.GetGenericArguments()[0]);
        }
    }
}
