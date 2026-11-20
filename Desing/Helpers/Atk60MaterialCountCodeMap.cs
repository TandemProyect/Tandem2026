using System;

namespace Desing.Helpers
{
    /// <summary>
    /// El visor ATK-60 usa el nombre del GLB (27754219.glb). El maestro comercial
    /// de los paneles universales 0,75 es otro código (27104219 / Atenko 3120270075).
    /// El recuento agrupa y nombra por el maestro, sin duplicar filas en BD.
    /// </summary>
    public static class Atk60MaterialCountCodeMap
    {
        public static string ToMasterTextCode(string code)
        {
            if (string.IsNullOrWhiteSpace(code))
            {
                return "";
            }

            var c = code.Trim();
            if (c.StartsWith("PANEL_", StringComparison.OrdinalIgnoreCase) && c.Length > 6)
            {
                c = c.Substring(6).Trim();
            }

            if (string.Equals(c, "27754219", StringComparison.OrdinalIgnoreCase))
            {
                return "27104219";
            }

            if (string.Equals(c, "24754224", StringComparison.OrdinalIgnoreCase))
            {
                return "24104224";
            }

            if (string.Equals(c, "12754120", StringComparison.OrdinalIgnoreCase))
            {
                return "12104120";
            }

            return c;
        }
    }
}
