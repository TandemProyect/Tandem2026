using Desing.Repositories.RepositoryAtk60.ModulosATK60;
using Desing.Repositories.RepositoryCommun;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Runtime.CompilerServices;

namespace Desing.Repositories.Repository_Desing_2.Atk60
{
    /// <summary>
    /// Paneles concretos que el usuario cambia por otros (Asistente IA, «de 13 paneles de 0,90 solo tengo 10»).
    /// Llega en el atributo del muro <c>_PanelReplace</c>, una lista de reglas:
    /// [ { "alongMm": 5400, "upMm": 0, "pieceWidthMm": 900, "pieceHeightMm": 2700, "orientation": "Vertical", "useMm": [600, 300] } ].
    /// alongMm/upMm/pieceWidthMm/pieceHeightMm/orientation son los del panel que devuelve el servidor
    /// (LocalAlongMm, LocalUpMm, PieceWidthMm, PieceHeightMm, Orientation). useMm = anchos de los paneles nuevos,
    /// en orden desde el inicio del panel (de pie: a lo largo del muro; tumbado: de abajo arriba). Deben sumar
    /// el ancho del panel que se cambia. Se cambian las dos caras a la vez (el tirante ata las dos), y las grapas,
    /// rigidizadores y tirantes se recalculan con el módulo nuevo.
    /// Sin el atributo, o si una regla no casa con ningún panel, el encofrado sale igual que antes.
    /// Solo afecta a los paneles del tramo recto (no esquinas, cara E, tapes ni curvo).
    /// </summary>
    internal sealed class Atk60PanelReplace
    {
        private const double AlongToleranceMm = 2d;
        private static readonly int[] Widths = { 900, 750, 600, 450, 300 };
        private static readonly ConditionalWeakTable<Desing2FormworkWallDto, Atk60PanelReplace> Cache =
            new ConditionalWeakTable<Desing2FormworkWallDto, Atk60PanelReplace>();
        private static readonly Atk60PanelReplace None = new Atk60PanelReplace();

        internal readonly List<Rule> Rules = new List<Rule>();

        internal sealed class Rule
        {
            internal double AlongMm;
            internal int UpMm;
            internal int PieceWidthMm;
            internal int PieceHeightMm;
            internal string Orientation;
            internal readonly List<int> UseMm = new List<int>();
            internal int Applied;
        }

        /// <summary>Reglas del muro, o null si el muro no trae <c>_PanelReplace</c> válido.</summary>
        internal static Atk60PanelReplace For(Desing2FormworkWallDto wall)
        {
            if (wall == null || wall.Attributes == null || wall.Attributes.Extra == null)
            {
                return null;
            }

            var found = Cache.GetValue(wall, Parse);
            return found == null || found.Rules.Count == 0 ? null : found;
        }

        /// <summary>
        /// Módulo con los paneles de las reglas cambiados. Si ninguna regla casa con este módulo,
        /// devuelve el mismo objeto <paramref name="layout"/> (no se copia ni se toca).
        /// </summary>
        internal Modulo270Layout Apply(Modulo270Layout layout, double moduleBaseAlongMm)
        {
            if (layout == null || layout.Pieces == null || layout.Pieces.Count == 0 || Rules.Count == 0)
            {
                return layout;
            }

            List<Modulo270PieceLayout> pieces = null;
            for (var i = 0; i < layout.Pieces.Count; i++)
            {
                var piece = layout.Pieces[i];
                var rule = piece != null ? Match(piece, moduleBaseAlongMm + piece.AlongOffsetMm) : null;
                if (rule == null)
                {
                    if (pieces != null)
                    {
                        pieces.Add(piece);
                    }

                    continue;
                }

                if (pieces == null)
                {
                    pieces = new List<Modulo270PieceLayout>();
                    for (var k = 0; k < i; k++)
                    {
                        pieces.Add(layout.Pieces[k]);
                    }
                }

                var offset = 0;
                for (var u = 0; u < rule.UseMm.Count; u++)
                {
                    pieces.Add(Modulo270HeightPanelCatalog.ResizePanelPiece(piece, rule.UseMm[u], offset));
                    offset += rule.UseMm[u];
                }

                rule.Applied++;
            }

            if (pieces == null)
            {
                return layout;
            }

            return new Modulo270Layout { CatalogHeightMm = layout.CatalogHeightMm, Pieces = pieces };
        }

        private Rule Match(Modulo270PieceLayout piece, double alongMm)
        {
            for (var r = 0; r < Rules.Count; r++)
            {
                var rule = Rules[r];
                if (Math.Abs(rule.AlongMm - alongMm) <= AlongToleranceMm
                    && Math.Abs(rule.UpMm - piece.UpOffsetMm) <= 2
                    && rule.PieceWidthMm == piece.PieceWidthMm
                    && rule.PieceHeightMm == piece.PieceHeightMm
                    && string.Equals(rule.Orientation ?? "Vertical", piece.Orientation ?? "Vertical", StringComparison.OrdinalIgnoreCase))
                {
                    return rule;
                }
            }

            return null;
        }

        private static Atk60PanelReplace Parse(Desing2FormworkWallDto wall)
        {
            JToken raw;
            if (!wall.Attributes.Extra.TryGetValue("_PanelReplace", out raw) || raw == null || raw.Type == JTokenType.Null)
            {
                return None;
            }

            try
            {
                if (raw.Type == JTokenType.String)
                {
                    var text = raw.Value<string>();
                    if (string.IsNullOrWhiteSpace(text))
                    {
                        return None;
                    }

                    raw = JToken.Parse(text);
                }

                var list = raw as JArray;
                if (list == null)
                {
                    return None;
                }

                var result = new Atk60PanelReplace();
                foreach (var item in list)
                {
                    var obj = item as JObject;
                    var rule = obj != null ? ParseRule(obj) : null;
                    if (rule != null)
                    {
                        result.Rules.Add(rule);
                    }
                }

                return result;
            }
            catch
            {
                return None;
            }
        }

        private static Rule ParseRule(JObject obj)
        {
            var rule = new Rule
            {
                AlongMm = Num(obj["alongMm"], double.NaN),
                UpMm = (int)Math.Round(Num(obj["upMm"], 0d)),
                PieceWidthMm = (int)Math.Round(Num(obj["pieceWidthMm"], 0d)),
                PieceHeightMm = (int)Math.Round(Num(obj["pieceHeightMm"], 0d)),
                Orientation = obj["orientation"] != null && obj["orientation"].Type == JTokenType.String
                    ? obj["orientation"].Value<string>()
                    : "Vertical",
            };
            if (double.IsNaN(rule.AlongMm) || rule.PieceWidthMm <= 0 || rule.PieceHeightMm <= 0)
            {
                return null;
            }

            var use = obj["useMm"] as JArray;
            if (use == null || use.Count < 2)
            {
                return null;
            }

            var sum = 0;
            foreach (var u in use)
            {
                var mm = (int)Math.Round(Num(u, 0d));
                if (Array.IndexOf(Widths, mm) < 0)
                {
                    return null;
                }

                rule.UseMm.Add(mm);
                sum += mm;
            }

            var tumbado = string.Equals(rule.Orientation, "Tumbado", StringComparison.OrdinalIgnoreCase);
            var nominal = tumbado ? rule.PieceHeightMm : rule.PieceWidthMm;
            return sum == nominal ? rule : null;
        }

        private static double Num(JToken token, double fallback)
        {
            if (token != null && (token.Type == JTokenType.Integer || token.Type == JTokenType.Float))
            {
                var direct = token.Value<double>();
                return double.IsNaN(direct) || double.IsInfinity(direct) ? fallback : direct;
            }

            double value;
            if (token == null || token.Type == JTokenType.Null
                || !double.TryParse(token.ToString(), NumberStyles.Float, CultureInfo.InvariantCulture, out value)
                || double.IsInfinity(value))
            {
                return fallback;
            }

            return value;
        }
    }
}
