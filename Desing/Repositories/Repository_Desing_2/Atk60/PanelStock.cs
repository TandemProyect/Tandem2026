using Desing.Repositories.RepositoryAtk60.ModulosATK60;
using Desing.Repositories.RepositoryCommun;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Runtime.CompilerServices;

namespace Desing.Repositories.Repository_Desing_2.Atk60
{
    /// <summary>
    /// Paneles que no hay en obra para un muro (Asistente IA, «no tengo paneles de 0,90»).
    /// Llega en el atributo del muro <c>_PanelStock</c>: { "excludeMm": [900], "maxMm": { "900": 3 } }.
    /// excludeMm = anchos que no se usan; maxMm = paneles de ese ancho como mucho en el muro (las dos caras).
    /// Sin el atributo no cambia nada: el encofrado sale igual que antes.
    /// Solo afecta a los paneles del tramo recto y a los de esquina L / cara E / hueco de esquina.
    /// No toca tapes, tablones ni el muro curvo.
    /// </summary>
    internal sealed class Atk60PanelStock
    {
        internal static readonly int[] Widths = { 900, 750, 600, 450, 300 };
        private static readonly ConditionalWeakTable<Desing2FormworkWallDto, Atk60PanelStock> Cache =
            new ConditionalWeakTable<Desing2FormworkWallDto, Atk60PanelStock>();
        private static readonly Atk60PanelStock None = new Atk60PanelStock();

        internal readonly HashSet<int> ExcludedMm = new HashSet<int>();
        internal readonly Dictionary<int, int> MaxPanels = new Dictionary<int, int>();
        internal readonly Dictionary<int, int> UsedPanels = new Dictionary<int, int>();
        internal readonly List<string> Notes = new List<string>();
        internal int ReplacedModules;

        internal bool IsEmpty
        {
            get { return ExcludedMm.Count == 0 && MaxPanels.Count == 0; }
        }

        /// <summary>Regla del muro, o null si el muro no trae <c>_PanelStock</c>.</summary>
        internal static Atk60PanelStock For(Desing2FormworkWallDto wall)
        {
            if (wall == null || wall.Attributes == null || wall.Attributes.Extra == null)
            {
                return null;
            }

            var found = Cache.GetValue(wall, Parse);
            return found == null || found.IsEmpty ? null : found;
        }

        /// <summary>Avisos de todos los muros con regla, para la respuesta del servidor.</summary>
        internal static List<object> CollectNotes(IEnumerable<Desing2FormworkWallDto> walls)
        {
            var list = new List<object>();
            if (walls == null)
            {
                return list;
            }

            foreach (var wall in walls)
            {
                Atk60PanelStock stock;
                if (wall == null || !Cache.TryGetValue(wall, out stock) || stock == null || stock.IsEmpty)
                {
                    continue;
                }

                var id = !string.IsNullOrWhiteSpace(wall.WallId)
                    ? wall.WallId
                    : (!string.IsNullOrWhiteSpace(wall.Id) ? wall.Id : wall.LineId);
                list.Add(new
                {
                    IdWall = id,
                    ExcludedMm = stock.ExcludedMm.OrderByDescending(x => x).ToList(),
                    MaxPanels = stock.MaxPanels.ToDictionary(k => k.Key.ToString(CultureInfo.InvariantCulture), v => v.Value),
                    UsedPanels = stock.UsedPanels.ToDictionary(k => k.Key.ToString(CultureInfo.InvariantCulture), v => v.Value),
                    ReplacedModules = stock.ReplacedModules,
                    Notes = stock.Notes.Distinct().ToList()
                });
            }

            return list;
        }

        private static Atk60PanelStock Parse(Desing2FormworkWallDto wall)
        {
            JToken raw;
            if (!wall.Attributes.Extra.TryGetValue("_PanelStock", out raw) || raw == null || raw.Type == JTokenType.Null)
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

                var obj = raw as JObject;
                if (obj == null)
                {
                    return None;
                }

                var stock = new Atk60PanelStock();
                var excluded = obj["excludeMm"] as JArray;
                if (excluded != null)
                {
                    foreach (var item in excluded)
                    {
                        var mm = ToWidthMm(item);
                        if (mm > 0)
                        {
                            stock.ExcludedMm.Add(mm);
                        }
                    }
                }

                var max = obj["maxMm"] as JObject;
                if (max != null)
                {
                    foreach (var prop in max.Properties())
                    {
                        double key;
                        if (!double.TryParse(prop.Name, NumberStyles.Float, CultureInfo.InvariantCulture, out key))
                        {
                            continue;
                        }

                        var mm = NormalizeWidthMm(key);
                        int count;
                        if (mm <= 0 || prop.Value == null
                            || !int.TryParse(prop.Value.ToString(), NumberStyles.Integer, CultureInfo.InvariantCulture, out count)
                            || count < 0)
                        {
                            continue;
                        }

                        if (count == 0)
                        {
                            stock.ExcludedMm.Add(mm);
                        }
                        else
                        {
                            stock.MaxPanels[mm] = count;
                        }
                    }
                }

                return stock;
            }
            catch
            {
                return None;
            }
        }

        private static int ToWidthMm(JToken item)
        {
            double value;
            if (item == null
                || !double.TryParse(item.ToString(), NumberStyles.Float, CultureInfo.InvariantCulture, out value))
            {
                return 0;
            }

            return NormalizeWidthMm(value);
        }

        /// <summary>0,9 (m) o 900 (mm) → 900. Solo anchos del catálogo.</summary>
        private static int NormalizeWidthMm(double value)
        {
            var mm = value < 10d ? (int)Math.Round(value * 1000d) : (int)Math.Round(value);
            return Array.IndexOf(Widths, mm) >= 0 ? mm : 0;
        }

        internal bool Allows(int widthMm)
        {
            return !ExcludedMm.Contains(widthMm);
        }

        internal int Remaining(int widthMm)
        {
            int max;
            if (!MaxPanels.TryGetValue(widthMm, out max))
            {
                return int.MaxValue;
            }

            int used;
            UsedPanels.TryGetValue(widthMm, out used);
            return Math.Max(0, max - used);
        }

        /// <summary>Paneles de cada ancho en el módulo (las caras que se pintan).</summary>
        internal static Dictionary<int, int> PanelsByWidth(Modulo270Layout layout, int faces)
        {
            var map = new Dictionary<int, int>();
            if (layout == null || layout.Pieces == null)
            {
                return map;
            }

            for (var i = 0; i < layout.Pieces.Count; i++)
            {
                var piece = layout.Pieces[i];
                if (piece == null)
                {
                    continue;
                }

                var width = string.Equals(piece.Orientation, "Tumbado", StringComparison.OrdinalIgnoreCase)
                    ? piece.PieceHeightMm
                    : piece.PieceWidthMm;
                int n;
                map.TryGetValue(width, out n);
                map[width] = n + Math.Max(1, faces);
            }

            return map;
        }

        internal bool Fits(Dictionary<int, int> panels)
        {
            foreach (var kv in panels)
            {
                if (!Allows(kv.Key) || kv.Value > Remaining(kv.Key))
                {
                    return false;
                }
            }

            return true;
        }

        internal void Consume(Dictionary<int, int> panels)
        {
            foreach (var kv in panels)
            {
                int used;
                UsedPanels.TryGetValue(kv.Key, out used);
                UsedPanels[kv.Key] = used + kv.Value;
            }
        }
    }
}
