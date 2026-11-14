using System;
using System.Collections.Generic;
using System.Linq;
using Desing.Repositories.Repository_Desing_2.Atk60;

namespace Desing.Repositories.RepositoryAtk60.ModulosATK60
{
    internal static class Modulo270HeightPanelCatalog
    {
        private const string BaseGlbPath = "/Content/DesignTools/Atk-60NewGeneration/GLB/";
        private const double MaxSupportedWallHeightMm = 12000d;

        internal static Modulo270Layout Resolve(double wallHeightMm)
        {
            return ResolveForModule(wallHeightMm, 2700);
        }

        internal static Modulo270Layout ResolveForModule(double wallHeightMm, int moduleLengthMm)
        {
            return ResolveForModule(wallHeightMm, moduleLengthMm, true);
        }

        internal static Modulo270Layout ResolveForModule(double wallHeightMm, int moduleLengthMm, bool useAdjustable750)
        {
            return ResolveForModule(wallHeightMm, moduleLengthMm, useAdjustable750, true);
        }

        internal static Modulo270Layout ResolveForModule(
            double wallHeightMm,
            int moduleLengthMm,
            bool useAdjustable750,
            bool verticalPanels)
        {
            if (moduleLengthMm < 300)
            {
                moduleLengthMm = 2700;
            }

            var target = NormalizeTargetHeightMm(wallHeightMm);

            // Modulos estrechos (0,30..0,90): solo paneles verticales.
            // El de 0,75 no entra si el muro no usa el regulable: se parte en 0,45 + 0,30.
            if (IsNarrowLengthModule(moduleLengthMm) && (useAdjustable750 || moduleLengthMm != 750))
            {
                return BuildNarrowModuleVerticalStack((int)target, moduleLengthMm);
            }

            // Sin paneles verticales, el 2,70, el 2,40 y el 1,20 van enteros tumbados.
            // El resto de módulos no cambia.
            if (!verticalPanels && IsTumbadoWhenVerticalOff(moduleLengthMm))
            {
                return RemapLayoutToModuleLength(BuildGreedyTumbadoLayout(target), moduleLengthMm, useAdjustable750);
            }

            var layout = target > 2700d
                ? BuildTallVerticalStackLayout(target)
                : BuildLayoutForNormalizedHeight(target);

            return RemapLayoutToModuleLength(layout, moduleLengthMm, useAdjustable750);
        }

        /// <summary>
        /// Igual que <see cref="ResolveForModule(double,int,bool,bool)"/>, pero respetando los paneles
        /// que hay en obra para ese muro (<see cref="Atk60PanelStock"/>). Si el módulo normal usa un ancho
        /// que no hay (o se pasa del máximo), se arma con columnas de pie de los anchos que sí hay,
        /// las menos posibles. Si no hay combinación exacta, se deja el módulo normal y se avisa.
        /// Sin regla (stock null) devuelve lo mismo que ResolveForModule.
        /// </summary>
        internal static Modulo270Layout ResolveForModuleStock(
            double wallHeightMm,
            int moduleLengthMm,
            bool useAdjustable750,
            bool verticalPanels,
            Atk60PanelStock stock,
            int faces)
        {
            var normal = ResolveForModule(wallHeightMm, moduleLengthMm, useAdjustable750, verticalPanels);
            if (stock == null || stock.IsEmpty || normal == null || normal.Pieces == null || normal.Pieces.Count == 0)
            {
                return normal;
            }

            var normalPanels = Atk60PanelStock.PanelsByWidth(normal, faces);
            if (stock.Fits(normalPanels))
            {
                stock.Consume(normalPanels);
                return normal;
            }

            var reshaped = ReshapeWithStock(normal, moduleLengthMm, stock, faces);
            if (reshaped != null)
            {
                stock.Consume(Atk60PanelStock.PanelsByWidth(reshaped, faces));
                stock.ReplacedModules++;
                return reshaped;
            }

            var target = (int)NormalizeTargetHeightMm(wallHeightMm);
            var perColumn = BuildNarrowModuleVerticalStack(target, 300).Pieces.Count * Math.Max(1, faces);
            var columns = DecomposeWithStock(moduleLengthMm, stock, perColumn);
            if (columns == null)
            {
                stock.Notes.Add("sin-combinacion:" + moduleLengthMm.ToString(System.Globalization.CultureInfo.InvariantCulture));
                stock.Consume(normalPanels);
                return normal;
            }

            var pieces = new List<Modulo270PieceLayout>();
            var along = 0;
            for (var c = 0; c < columns.Count; c++)
            {
                var stack = BuildNarrowModuleVerticalStack(target, columns[c]);
                for (var p = 0; p < stack.Pieces.Count; p++)
                {
                    var piece = stack.Pieces[p];
                    pieces.Add(PieceVertical(
                        ResolveVerticalGlbCode(piece.PieceHeightMm, columns[c]),
                        columns[c], piece.PieceHeightMm, along, piece.UpOffsetMm));
                }

                along += columns[c];
            }

            var layout = Build(target, pieces.ToArray());
            stock.Consume(Atk60PanelStock.PanelsByWidth(layout, faces));
            stock.ReplacedModules++;
            return layout;
        }

        /// <summary>
        /// Misma forma que el módulo normal, cambiando solo lo que no hay: cada banda de paneles de pie
        /// se reparte otra vez en columnas de los anchos que hay, y cada panel tumbado de un ancho que no
        /// hay se cambia por tumbados de los anchos que hay que suman lo mismo (0,90 → 0,60 + 0,30).
        /// null si alguna banda no tiene combinación exacta.
        /// </summary>
        private static Modulo270Layout ReshapeWithStock(Modulo270Layout normal, int moduleLengthMm, Atk60PanelStock stock, int faces)
        {
            var perPiece = Math.Max(1, faces);
            var taken = new Dictionary<int, int>();
            var probe = new Atk60PanelStock();
            foreach (var w in stock.ExcludedMm)
            {
                probe.ExcludedMm.Add(w);
            }

            Action syncProbe = () =>
            {
                probe.MaxPanels.Clear();
                foreach (var kv in stock.MaxPanels)
                {
                    int t;
                    taken.TryGetValue(kv.Key, out t);
                    probe.MaxPanels[kv.Key] = Math.Max(0, stock.Remaining(kv.Key) - t);
                }
            };

            var pieces = new List<Modulo270PieceLayout>();
            var bands = new List<string>();
            for (var i = 0; i < normal.Pieces.Count; i++)
            {
                var piece = normal.Pieces[i];
                if (piece == null)
                {
                    continue;
                }

                var tumbado = string.Equals(piece.Orientation, "Tumbado", StringComparison.OrdinalIgnoreCase);
                if (tumbado)
                {
                    syncProbe();
                    var rows = DecomposeWithStock(piece.PieceHeightMm, probe, perPiece);
                    if (rows == null)
                    {
                        return null;
                    }

                    var up = piece.UpOffsetMm;
                    for (var r = 0; r < rows.Count; r++)
                    {
                        pieces.Add(PieceTumbado(ResolveVerticalGlbCode(piece.PieceWidthMm, rows[r]), rows[r], piece.PieceWidthMm, up));
                        up += rows[r];
                        int t;
                        taken.TryGetValue(rows[r], out t);
                        taken[rows[r]] = t + perPiece;
                    }

                    continue;
                }

                var key = piece.UpOffsetMm.ToString() + ":" + piece.PieceHeightMm.ToString();
                if (bands.Contains(key))
                {
                    continue;
                }

                bands.Add(key);
                var span = 0;
                for (var j = 0; j < normal.Pieces.Count; j++)
                {
                    var other = normal.Pieces[j];
                    if (other != null
                        && !string.Equals(other.Orientation, "Tumbado", StringComparison.OrdinalIgnoreCase)
                        && other.UpOffsetMm == piece.UpOffsetMm
                        && other.PieceHeightMm == piece.PieceHeightMm)
                    {
                        span += other.PieceWidthMm;
                    }
                }

                syncProbe();
                var columns = DecomposeWithStock(span, probe, perPiece);
                if (columns == null)
                {
                    return null;
                }

                var along = 0;
                for (var c = 0; c < columns.Count; c++)
                {
                    pieces.Add(PieceVertical(
                        ResolveVerticalGlbCode(piece.PieceHeightMm, columns[c]),
                        columns[c], piece.PieceHeightMm, along, piece.UpOffsetMm));
                    along += columns[c];
                    int t;
                    taken.TryGetValue(columns[c], out t);
                    taken[columns[c]] = t + perPiece;
                }
            }

            return pieces.Count == 0 ? null : Build(normal.CatalogHeightMm, pieces.ToArray());
        }

        /// <summary>
        /// Anchos de columna que suman el módulo exacto con los paneles que hay: primero las menos
        /// columnas y, a igualdad, los paneles más anchos. null si no hay combinación.
        /// </summary>
        private static List<int> DecomposeWithStock(int lengthMm, Atk60PanelStock stock, int panelsPerColumn)
        {
            if (lengthMm < 300)
            {
                return null;
            }

            var widths = new List<int>();
            for (var i = 0; i < Atk60PanelStock.Widths.Length; i++)
            {
                if (stock.Allows(Atk60PanelStock.Widths[i]))
                {
                    widths.Add(Atk60PanelStock.Widths[i]);
                }
            }

            if (widths.Count == 0)
            {
                return null;
            }

            var maxColumns = lengthMm / 300;
            for (var n = 1; n <= maxColumns; n++)
            {
                var pick = new List<int>();
                if (PickColumns(lengthMm, n, 0, widths, stock, panelsPerColumn, new Dictionary<int, int>(), pick))
                {
                    return pick;
                }
            }

            return null;
        }

        private static bool PickColumns(
            int remainingMm,
            int columnsLeft,
            int fromIndex,
            List<int> widths,
            Atk60PanelStock stock,
            int panelsPerColumn,
            Dictionary<int, int> used,
            List<int> pick)
        {
            if (columnsLeft == 0)
            {
                return remainingMm == 0;
            }

            for (var i = fromIndex; i < widths.Count; i++)
            {
                var w = widths[i];
                if (w > remainingMm)
                {
                    continue;
                }

                int n;
                used.TryGetValue(w, out n);
                var remaining = stock.Remaining(w);
                if (remaining != int.MaxValue && (n + 1) * panelsPerColumn > remaining)
                {
                    continue;
                }

                used[w] = n + 1;
                pick.Add(w);
                if (PickColumns(remainingMm - w, columnsLeft - 1, i, widths, stock, panelsPerColumn, used, pick))
                {
                    return true;
                }

                pick.RemoveAt(pick.Count - 1);
                used[w] = n;
            }

            return false;
        }

        private static bool IsTumbadoWhenVerticalOff(int moduleLengthMm)
        {
            return moduleLengthMm == 2700
                || moduleLengthMm == 2400
                || moduleLengthMm == 1200;
        }

        private static bool IsNarrowLengthModule(int moduleLengthMm)
        {
            return moduleLengthMm == 300
                || moduleLengthMm == 450
                || moduleLengthMm == 600
                || moduleLengthMm == 750
                || moduleLengthMm == 900;
        }

        private static Modulo270Layout BuildNarrowModuleVerticalStack(int targetHeightMm, int widthMm)
        {
            var pieces = new List<Modulo270PieceLayout>();
            var remaining = targetHeightMm;
            var up = 0;

            while (remaining >= 299)
            {
                int familyH;
                if (remaining > 2400)
                {
                    familyH = 2700;
                }
                else if (remaining > 1200)
                {
                    familyH = 2400;
                }
                else
                {
                    familyH = 1200;
                }

                var glb = ResolveVerticalGlbCode(familyH, widthMm);
                pieces.Add(PieceVertical(glb, widthMm, familyH, 0, up));
                remaining -= familyH;
                up += familyH;
            }

            if (pieces.Count == 0)
            {
                var glb = ResolveVerticalGlbCode(1200, widthMm);
                pieces.Add(PieceVertical(glb, widthMm, 1200, 0, 0));
            }

            return Build(targetHeightMm, pieces.ToArray());
        }

        private static double NormalizeTargetHeightMm(double wallHeightMm)
        {
            var h = Math.Max(300d, Math.Min(MaxSupportedWallHeightMm, wallHeightMm));
            return Math.Ceiling(h / 150d) * 150d;
        }

        private static Modulo270Layout BuildLayoutForNormalizedHeight(double targetHeightMm)
        {
            Modulo270Layout layout;
            if (!TryGetKnownLayout(targetHeightMm, out layout) || layout == null)
            {
                layout = targetHeightMm > 2700d
                    ? BuildMixedVerticalPlusTumbadoLayout(targetHeightMm)
                    : BuildGreedyTumbadoLayout(targetHeightMm);
            }

            return layout;
        }

        private static Modulo270Layout BuildTallVerticalStackLayout(double targetHeightMm)
        {
            var pieces = new List<Modulo270PieceLayout>();
            var remaining = targetHeightMm;
            var up = 0;

            while (remaining > 2700d && remaining - 2700d >= 299.5d)
            {
                Add2700VerticalBand(pieces, up);
                remaining -= 2700d;
                up += 2700;
            }

            var top = BuildLayoutForNormalizedHeight(remaining);
            if (top != null && top.Pieces != null)
            {
                for (var i = 0; i < top.Pieces.Count; i++)
                {
                    pieces.Add(OffsetPiece(top.Pieces[i], up));
                }
            }

            if (pieces.Count == 0)
            {
                return BuildLayoutForNormalizedHeight(targetHeightMm);
            }

            return Build((int)targetHeightMm, pieces.ToArray());
        }

        private static void Add2700VerticalBand(List<Modulo270PieceLayout> pieces, int upOffsetMm)
        {
            pieces.Add(PieceVertical("27904209", 900, 2700, 0, upOffsetMm));
            pieces.Add(PieceVertical("27904209", 900, 2700, 900, upOffsetMm));
            pieces.Add(PieceVertical("27904209", 900, 2700, 1800, upOffsetMm));
        }

        private static Modulo270PieceLayout OffsetPiece(Modulo270PieceLayout source, int upOffsetMm)
        {
            if (source == null)
            {
                return null;
            }

            return new Modulo270PieceLayout
            {
                ElementCode = source.ElementCode,
                ImportPath = source.ImportPath,
                Orientation = source.Orientation,
                PieceWidthMm = source.PieceWidthMm,
                PieceHeightMm = source.PieceHeightMm,
                AlongOffsetMm = source.AlongOffsetMm,
                UpOffsetMm = source.UpOffsetMm + upOffsetMm,
                InsertOffsetX = source.InsertOffsetX,
                InsertOffsetY = source.InsertOffsetY,
                InsertOffsetZ = source.InsertOffsetZ,
                BaseRotX = source.BaseRotX,
                BaseRotY = source.BaseRotY,
                BaseRotZ = source.BaseRotZ,
                UseStrictPose = source.UseStrictPose
            };
        }

        private static bool TryGetKnownLayout(double targetHeightMm, out Modulo270Layout layout)
        {
            var p030 = PieceTumbado("27304205", 300, 2700, 0);
            var p045 = PieceTumbado("27454206", 450, 2700, 0);
            var p060 = PieceTumbado("27604207", 600, 2700, 0);
            var p075 = PieceTumbado("27754219", 750, 2700, 0);
            var p090 = PieceTumbado("27904209", 900, 2700, 0);

            switch ((int)targetHeightMm)
            {
                case 300:
                    layout = Build((int)targetHeightMm, p030);
                    return true;
                case 450:
                    layout = Build((int)targetHeightMm, p045);
                    return true;
                case 600:
                    layout = Build((int)targetHeightMm, p060);
                    return true;
                case 750:
                    layout = Build((int)targetHeightMm, p075);
                    return true;
                case 900:
                    layout = Build((int)targetHeightMm, p090);
                    return true;
                case 1050:
                    layout = Build((int)targetHeightMm,
                        p060,
                        PieceTumbado("27454206", 450, 2700, 600));
                    return true;
                case 1200:
                    layout = Build((int)targetHeightMm,
                        PieceVertical("12904215", 900, 1200, 0),
                        PieceVertical("12904215", 900, 1200, 900),
                        PieceVertical("12904215", 900, 1200, 1800));
                    return true;
                case 1350:
                    layout = Build((int)targetHeightMm,
                        p090,
                        PieceTumbado("27454206", 450, 2700, 900));
                    return true;
                case 1500:
                    layout = Build((int)targetHeightMm,
                        PieceVertical("12904215", 900, 1200, 0),
                        PieceVertical("12904215", 900, 1200, 900),
                        PieceVertical("12904215", 900, 1200, 1800),
                        PieceTumbado("27304205", 300, 2700, 1200));
                    return true;
                case 1650:
                    layout = Build((int)targetHeightMm,
                        PieceVertical("12904215", 900, 1200, 0),
                        PieceVertical("12904215", 900, 1200, 900),
                        PieceVertical("12904215", 900, 1200, 1800),
                        PieceTumbado("27454206", 450, 2700, 1200));
                    return true;
                case 1800:
                    layout = Build((int)targetHeightMm,
                        PieceVertical("12904215", 900, 1200, 0),
                        PieceVertical("12904215", 900, 1200, 900),
                        PieceVertical("12904215", 900, 1200, 1800),
                        PieceTumbado("27604207", 600, 2700, 1200));
                    return true;
                case 1950:
                    layout = Build((int)targetHeightMm,
                        PieceVertical("12904215", 900, 1200, 0),
                        PieceVertical("12904215", 900, 1200, 900),
                        PieceVertical("12904215", 900, 1200, 1800),
                        PieceTumbado("27754219", 750, 2700, 1200));
                    return true;
                case 2100:
                    layout = Build((int)targetHeightMm,
                        PieceVertical("12904215", 900, 1200, 0),
                        PieceVertical("12904215", 900, 1200, 900),
                        PieceVertical("12904215", 900, 1200, 1800),
                        PieceTumbado("27904209", 900, 2700, 1200));
                    return true;
                case 2400:
                    layout = Build((int)targetHeightMm,
                        PieceVertical("24904240", 900, 2400, 0),
                        PieceVertical("24904240", 900, 2400, 900),
                        PieceVertical("24904240", 900, 2400, 1800));
                    return true;
                case 2550:
                    layout = Build((int)targetHeightMm,
                        PieceVertical("12904215", 900, 1200, 0),
                        PieceVertical("12904215", 900, 1200, 900),
                        PieceVertical("12904215", 900, 1200, 1800),
                        PieceTumbado("27904209", 900, 2700, 1200),
                        PieceTumbado("27454206", 450, 2700, 2100));
                    return true;
                case 2700:
                    layout = Build((int)targetHeightMm,
                        PieceVertical("27904209", 900, 2700, 0),
                        PieceVertical("27904209", 900, 2700, 900),
                        PieceVertical("27904209", 900, 2700, 1800));
                    return true;
            }

            layout = null;
            return false;
        }

        private static Modulo270Layout BuildGreedyTumbadoLayout(double targetHeightMm)
        {
            var options = new[]
            {
                new { H = 900, Glb = "27904209" },
                new { H = 750, Glb = "27754219" },
                new { H = 600, Glb = "27604207" },
                new { H = 450, Glb = "27454206" },
                new { H = 300, Glb = "27304205" },
            };

            var remaining = targetHeightMm;
            var up = 0;
            var pieces = new List<Modulo270PieceLayout>();

            for (var i = 0; i < options.Length && remaining >= 299.5; i++)
            {
                while (remaining >= options[i].H - 0.5)
                {
                    pieces.Add(PieceTumbado(options[i].Glb, options[i].H, 2700, up));
                    remaining -= options[i].H;
                    up += options[i].H;
                }
            }

            if (pieces.Count == 0)
            {
                pieces.Add(PieceTumbado("27304205", 300, 2700, 0));
            }

            return Build((int)targetHeightMm, pieces.ToArray());
        }

        private static Modulo270Layout BuildMixedVerticalPlusTumbadoLayout(double targetHeightMm)
        {
            var pieces = new List<Modulo270PieceLayout>();

            var baseOptions = new[]
            {
                new { BaseH = 2700, Glb = "27904209" },
                new { BaseH = 2400, Glb = "24904240" },
                new { BaseH = 1200, Glb = "12904215" },
            };

            var selectedBase = baseOptions[0];
            for (var i = 0; i < baseOptions.Length; i++)
            {
                var rem = targetHeightMm - baseOptions[i].BaseH;
                if (Math.Abs(rem) < 0.5 || rem >= 299.5)
                {
                    selectedBase = baseOptions[i];
                    break;
                }
            }

            pieces.Add(PieceVertical(selectedBase.Glb, 900, selectedBase.BaseH, 0));
            pieces.Add(PieceVertical(selectedBase.Glb, 900, selectedBase.BaseH, 900));
            pieces.Add(PieceVertical(selectedBase.Glb, 900, selectedBase.BaseH, 1800));

            var remaining = targetHeightMm - selectedBase.BaseH;
            var up = selectedBase.BaseH;
            var topOptions = new[]
            {
                new { H = 900, Glb = "27904209" },
                new { H = 750, Glb = "27754219" },
                new { H = 600, Glb = "27604207" },
                new { H = 450, Glb = "27454206" },
                new { H = 300, Glb = "27304205" },
            };

            for (var i = 0; i < topOptions.Length && remaining >= 299.5; i++)
            {
                while (remaining >= topOptions[i].H - 0.5)
                {
                    pieces.Add(PieceTumbado(topOptions[i].Glb, topOptions[i].H, 2700, up));
                    remaining -= topOptions[i].H;
                    up += topOptions[i].H;
                }
            }

            return Build((int)targetHeightMm, pieces.ToArray());
        }

        private static Modulo270Layout Build(int catalogHeightMm, params Modulo270PieceLayout[] pieces)
        {
            return new Modulo270Layout
            {
                CatalogHeightMm = catalogHeightMm,
                Pieces = pieces != null ? pieces.ToList() : new List<Modulo270PieceLayout>()
            };
        }

        private static Modulo270Layout RemapLayoutToModuleLength(Modulo270Layout source, int moduleLengthMm, bool useAdjustable750)
        {
            if (source == null || source.Pieces == null || source.Pieces.Count == 0)
            {
                return source;
            }

            var columns = DecomposeLengthToPanelWidths(moduleLengthMm, useAdjustable750);
            if (columns.Count == 0)
            {
                columns.Add(Math.Max(300, moduleLengthMm));
            }

            var pieces = new List<Modulo270PieceLayout>();
            var seenVerticalKeys = new HashSet<string>(StringComparer.Ordinal);
            for (var i = 0; i < source.Pieces.Count; i++)
            {
                var template = source.Pieces[i];
                if (template == null)
                {
                    continue;
                }

                var isTumbado = string.Equals(template.Orientation, "Tumbado", StringComparison.OrdinalIgnoreCase);
                if (isTumbado)
                {
                    // Tumbado solo en 2,70 / 2,40 / 1,20. El GLB largo debe coincidir
                    // con el modulo (si no, el de 2,70 se sale encima del 0,45).
                    var glb = ResolveVerticalGlbCode(moduleLengthMm, template.PieceHeightMm);
                    pieces.Add(PieceTumbado(glb, template.PieceHeightMm, moduleLengthMm, template.UpOffsetMm));
                    continue;
                }

                var key = template.UpOffsetMm.ToString() + ":" + template.PieceHeightMm.ToString();
                if (!seenVerticalKeys.Add(key))
                {
                    continue;
                }

                var along = 0;
                for (var c = 0; c < columns.Count; c++)
                {
                    var width = columns[c];
                    var glb = ResolveVerticalGlbCode(template.PieceHeightMm, width);
                    pieces.Add(PieceVertical(glb, width, template.PieceHeightMm, along, template.UpOffsetMm));
                    along += width;
                }
            }

            return Build(source.CatalogHeightMm, pieces.ToArray());
        }

        private static List<int> DecomposeLengthToPanelWidths(int lengthMm, bool useAdjustable750)
        {
            var options = useAdjustable750
                ? new[] { 900, 750, 600, 450, 300 }
                : new[] { 900, 750, 600, 450, 300 };
            var columns = new List<int>();
            var remaining = Math.Max(0, lengthMm);
            for (var i = 0; i < options.Length && remaining >= 299; i++)
            {
                while (remaining >= options[i])
                {
                    var left = remaining - options[i];
                    // 1,05 no es un 0,90 con un hueco de 0,15: es 0,75 + 0,30.
                    if (left != 0 && left < 300)
                    {
                        break;
                    }

                    columns.Add(options[i]);
                    remaining -= options[i];
                }
            }

            return columns;
        }

        /// <summary>
        /// Panel igual que <paramref name="source"/> (misma altura, banda y orientación) pero de otro ancho,
        /// desplazado <paramref name="offsetMm"/> desde su inicio: a lo largo del muro si es de pie, hacia
        /// arriba si es tumbado. Lo usa <c>Atk60PanelReplace</c> para partir un panel (0,90 → 0,60 + 0,30).
        /// </summary>
        internal static Modulo270PieceLayout ResizePanelPiece(Modulo270PieceLayout source, int panelWidthMm, int offsetMm)
        {
            if (source == null)
            {
                return null;
            }

            if (string.Equals(source.Orientation, "Tumbado", StringComparison.OrdinalIgnoreCase))
            {
                return PieceTumbado(
                    ResolveVerticalGlbCode(source.PieceWidthMm, panelWidthMm),
                    panelWidthMm, source.PieceWidthMm, source.UpOffsetMm + offsetMm);
            }

            return PieceVertical(
                ResolveVerticalGlbCode(source.PieceHeightMm, panelWidthMm),
                panelWidthMm, source.PieceHeightMm, source.AlongOffsetMm + offsetMm, source.UpOffsetMm);
        }

        private static string ResolveVerticalGlbCode(int heightMm, int widthMm)
        {
            var family = 2700;
            if (heightMm <= 1200)
            {
                family = 1200;
            }
            else if (heightMm <= 2400)
            {
                family = 2400;
            }

            if (family == 1200)
            {
                switch (widthMm)
                {
                    case 900: return "12904215";
                    case 750: return "12754120";
                    case 600: return "12604213";
                    case 450: return "12454212";
                    case 300: return "12304211";
                }
            }
            else if (family == 2400)
            {
                switch (widthMm)
                {
                    case 900: return "24904240";
                    case 750: return "24754224";
                    case 600: return "24604242";
                    case 450: return "24454243";
                    case 300: return "24304244";
                }
            }

            switch (widthMm)
            {
                case 750: return "27754219";
                case 600: return "27604207";
                case 450: return "27454206";
                case 300: return "27304205";
                default: return "27904209";
            }
        }

        private static string ExtractGlbCode(string elementCode)
        {
            if (string.IsNullOrWhiteSpace(elementCode))
            {
                return "27904209";
            }

            const string prefix = "PANEL_";
            if (elementCode.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
            {
                return elementCode.Substring(prefix.Length);
            }

            return elementCode;
        }

        private static Modulo270PieceLayout PieceVertical(string glbCode, int widthMm, int heightMm, int alongOffsetMm)
        {
            return PieceVertical(glbCode, widthMm, heightMm, alongOffsetMm, 0);
        }

        private static Modulo270PieceLayout PieceVertical(string glbCode, int widthMm, int heightMm, int alongOffsetMm, int upOffsetMm)
        {
            return new Modulo270PieceLayout
            {
                ElementCode = "PANEL_" + glbCode,
                Orientation = "Vertical",
                ImportPath = BaseGlbPath + glbCode + ".glb",
                PieceWidthMm = widthMm,
                PieceHeightMm = heightMm,
                AlongOffsetMm = alongOffsetMm,
                UpOffsetMm = upOffsetMm,
                InsertOffsetX = 0,
                InsertOffsetY = 0,
                InsertOffsetZ = 0,
                BaseRotX = 0,
                BaseRotY = 0,
                BaseRotZ = 0,
                UseStrictPose = true,
            };
        }

        private static Modulo270PieceLayout PieceTumbado(string glbCode, int heightMm, int widthMm, int upOffsetMm)
        {
            return new Modulo270PieceLayout
            {
                ElementCode = "PANEL_" + glbCode,
                Orientation = "Tumbado",
                ImportPath = BaseGlbPath + glbCode + ".glb",
                PieceWidthMm = widthMm,
                PieceHeightMm = heightMm,
                AlongOffsetMm = 0,
                UpOffsetMm = upOffsetMm,
                InsertOffsetX = 0,
                InsertOffsetY = 0,
                InsertOffsetZ = 0,
                BaseRotX = 0,
                BaseRotY = 0,
                BaseRotZ = -Math.PI * 0.5,
                UseStrictPose = true,
            };
        }
    }

    internal sealed class Modulo270Layout
    {
        public int CatalogHeightMm { get; set; }
        public List<Modulo270PieceLayout> Pieces { get; set; }
    }

    internal sealed class Modulo270PieceLayout
    {
        public string ElementCode { get; set; }
        public string Orientation { get; set; }
        public string ImportPath { get; set; }
        public int PieceWidthMm { get; set; }
        public int PieceHeightMm { get; set; }
        public int AlongOffsetMm { get; set; }
        public int UpOffsetMm { get; set; }
        public double InsertOffsetX { get; set; }
        public double InsertOffsetY { get; set; }
        public double InsertOffsetZ { get; set; }
        public double BaseRotX { get; set; }
        public double BaseRotY { get; set; }
        public double BaseRotZ { get; set; }
        public bool UseStrictPose { get; set; }
    }
}
