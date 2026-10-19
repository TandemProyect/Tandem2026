using Desing.Repositories.RepositoryCommun;
using System;
using System.Collections.Generic;

namespace Desing.Repositories.Repository_Desing_2.Atk60
{
    /// <summary>
    /// Puntal de aplome. Dos por módulo largo, en las dos caras.
    /// El origen del STL queda en la cara del panel, a ras de suelo:
    /// coordenada del muro, su rotación y 120 mm hacia el exterior.
    /// </summary>
    internal static class PushPullProp
    {
        private const double PanelFaceMm = 120d;
        private const double MinWallHeightMm = 2000d;
        private const double LowerPartFromMm = 1200d;
        private const double DefaultMaxSpacingMm = 2400d;
        /// <summary>NEVI 2.9: agujero del cabezal puntal, a 50 mm de la cara exterior de la costilla.</summary>
        private const double HousingFromOuterFaceMm = 50d;
        /// <summary>
        /// El pasador del h270 queda a 2 455 mm y 107 mm por fuera del origen.
        /// El taladro superior del panel de 2,70 m está a 2 540 mm y 50 mm por dentro de la cara.
        /// </summary>
        private const double H270PinRaiseMm = 85d;
        private const double H270PinIntoFaceMm = 157d;
        /// <summary>
        /// Pasador del h270: 30,5 mm a lo largo de la cara, 114 mm por fuera
        /// del origen y a 2 450 mm. NEVI 2.9: el agujero del cabezal está
        /// a 50 mm de la cara exterior de la costilla. En el 27104219 ese
        /// agujero superior queda a 2 368 mm.
        /// </summary>
        private const double H270PinAlongMm = 30.5d;
        private const double H270PinOutMm = 114d;
        private const double H270PinUpMm = 2450d;
        private const double H270RibHoleInsideMm = 50d;
        private const double H270RibHoleUpMm = 2368d;

        internal static void PlaceAlongWall(
            List<Atk60ElementPaintItem> outElements,
            Desing2FormworkWallDto wall,
            Atk60WallPaintAnchor anchor)
        {
            if (outElements == null || wall == null || anchor == null)
            {
                return;
            }

            var attrs = wall.Attributes;
            var wallHeightMm = Atk60FaceInsertion.ResolveWallHeightMm(attrs);
            if (wallHeightMm < MinWallHeightMm)
            {
                return;
            }

            var upperOutside = Atk60FaceInsertion.FlagOn(attrs != null ? attrs._CHeckPropOutside : null);
            var upperInside = Atk60FaceInsertion.FlagOn(attrs != null ? attrs._CHeckPropInside : null);
            var lowerOutside = Atk60FaceInsertion.FlagOn(attrs != null ? attrs._CHeckPropOutsideInf : null);
            var lowerInside = Atk60FaceInsertion.FlagOn(attrs != null ? attrs._CHeckPropInsideInf : null);
            if (!upperOutside && !upperInside && !lowerOutside && !lowerInside)
            {
                return;
            }

            var idWall = !string.IsNullOrWhiteSpace(wall.WallId)
                ? wall.WallId
                : (!string.IsNullOrWhiteSpace(wall.Id) ? wall.Id : wall.LineId);
            var wallLengthMm = Atk60FaceInsertion.ResolveWallLengthMm(attrs);
            var maxSpacingMm = ResolveMaxSpacingMm(attrs);
            var stations = Distribute(HousingsAlongWall(outElements, idWall), wallLengthMm, maxSpacingMm);
            if (stations.Count == 0)
            {
                return;
            }

            var parts = PartsForHeight(wallHeightMm);
            if (parts.Count == 0)
            {
                return;
            }

            var yawRad = Atk60FaceInsertion.NormalizeYawToRad(anchor.RotY);
            var ux = Math.Cos(yawRad);
            var uz = Math.Sin(yawRad);
            var faceSign = anchor.FaceSign >= 0d ? 1d : -1d;
            var wallThicknessMm = Atk60FaceInsertion.ResolveWallThicknessMm(attrs);

            for (var si = 0; si < stations.Count; si++)
            {
                var alongMm = stations[si];
                if (alongMm < 30d || alongMm > wallLengthMm - 30d)
                {
                    continue;
                }

                for (var pi = 0; pi < parts.Count; pi++)
                {
                    var part = parts[pi];
                    var showOutside = part.Lower ? lowerOutside : upperOutside;
                    var showInside = part.Lower ? lowerInside : upperInside;
                    if (!showOutside && !showInside)
                    {
                        continue;
                    }

                    AddBothFaces(
                        outElements, anchor, idWall,
                        part.Path, part.Code, part.Color,
                        alongMm, yawRad, ux, uz, faceSign,
                        wallHeightMm, wallThicknessMm, wallLengthMm,
                        showOutside, showInside, !part.Lower);
                }
            }
        }

        private static double ResolveMaxSpacingMm(AttributesList attrs)
        {
            var raw = attrs != null ? attrs._PropSpacingMm : null;
            if (!raw.HasValue || raw.Value <= 0d)
            {
                return DefaultMaxSpacingMm;
            }

            var value = raw.Value < 20d ? raw.Value * 1000d : raw.Value;
            if (value < 300d) return 300d;
            if (value > 6000d) return 6000d;
            return value;
        }

        private static List<int> HousingsAlongWall(List<Atk60ElementPaintItem> elements, string idWall)
        {
            var found = new List<int>();
            for (var i = 0; i < elements.Count; i++)
            {
                var item = elements[i];
                if (item == null || !string.Equals(item.ElementType, "Panel", StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                if (!string.IsNullOrWhiteSpace(idWall)
                    && !string.Equals(item.IdWall, idWall, StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                var width = item.PieceWidthMm;
                var origin = item.LocalAlongMm;
                if (width < 80d)
                {
                    continue;
                }

                AddHousing(found, origin + HousingFromOuterFaceMm);
                AddHousing(found, origin + width - HousingFromOuterFaceMm);
                if (width >= 1200d)
                {
                    AddHousing(found, origin + 300d);
                    AddHousing(found, origin + width - 300d);
                }

                if (width >= 2000d)
                {
                    AddHousing(found, origin + 600d);
                    AddHousing(found, origin + width - 600d);
                }
            }

            found.Sort();
            return found;
        }

        private static void AddHousing(List<int> found, double mm)
        {
            var n = (int)Math.Round(mm);
            if (n < 40)
            {
                return;
            }

            for (var i = 0; i < found.Count; i++)
            {
                if (Math.Abs(found[i] - n) < 20)
                {
                    return;
                }
            }

            found.Add(n);
        }

        /// <summary>
        /// El máximo no se rebasa. El paso P cabe en el muro (P = largo / número).
        /// El primero queda a P/2 del inicio y el último a P/2 del final.
        /// Cada puesto se acerca al alojamiento del panel más próximo.
        /// </summary>
        private static List<double> Distribute(List<int> housings, double wallLengthMm, double maxSpacingMm)
        {
            var usable = new List<double>();
            var end = wallLengthMm - 30d;
            for (var i = 0; i < housings.Count; i++)
            {
                var h = (double)housings[i];
                if (h >= 40d && h <= end)
                {
                    usable.Add(h);
                }
            }

            if (usable.Count == 0 || maxSpacingMm < 300d || wallLengthMm < 80d)
            {
                return new List<double>();
            }

            var minimum = (int)Math.Ceiling(wallLengthMm / maxSpacingMm);
            if (minimum < 1)
            {
                minimum = 1;
            }

            if (minimum > usable.Count)
            {
                minimum = usable.Count;
            }

            List<double> fallback = null;
            var limit = usable.Count < minimum + 6 ? usable.Count : minimum + 6;
            for (var count = minimum; count <= limit; count++)
            {
                var pitch = wallLengthMm / count;
                var ideals = new double[count];
                for (var k = 0; k < count; k++)
                {
                    ideals[k] = (k + 0.5d) * pitch;
                }

                var snapped = SnapToHousings(usable, ideals);
                if (snapped == null || snapped.Count != count)
                {
                    continue;
                }

                if (fallback == null)
                {
                    fallback = snapped;
                }

                if (GapsWithinMax(snapped, wallLengthMm, maxSpacingMm))
                {
                    return snapped;
                }
            }

            return fallback ?? new List<double>();
        }

        /// <summary>
        /// Elige un alojamiento por puesto ideal, en orden, con el menor desvío acumulado.
        /// </summary>
        private static List<double> SnapToHousings(List<double> usable, double[] ideals)
        {
            var count = ideals.Length;
            var housingCount = usable.Count;
            if (count < 1 || count > housingCount)
            {
                return null;
            }

            var cost = new double[count, housingCount];
            var previous = new int[count, housingCount];
            for (var k = 0; k < count; k++)
            {
                for (var i = 0; i < housingCount; i++)
                {
                    cost[k, i] = double.PositiveInfinity;
                    previous[k, i] = -1;
                }
            }

            for (var i = 0; i < housingCount; i++)
            {
                cost[0, i] = Math.Abs(usable[i] - ideals[0]);
            }

            for (var k = 1; k < count; k++)
            {
                for (var i = k; i < housingCount; i++)
                {
                    var best = double.PositiveInfinity;
                    var bestIndex = -1;
                    for (var j = k - 1; j < i; j++)
                    {
                        if (double.IsInfinity(cost[k - 1, j]) || usable[i] < usable[j] + 80d)
                        {
                            continue;
                        }

                        var next = cost[k - 1, j] + Math.Abs(usable[i] - ideals[k]);
                        if (next < best)
                        {
                            best = next;
                            bestIndex = j;
                        }
                    }

                    cost[k, i] = best;
                    previous[k, i] = bestIndex;
                }
            }

            var endIndex = -1;
            var endCost = double.PositiveInfinity;
            for (var i = count - 1; i < housingCount; i++)
            {
                if (cost[count - 1, i] < endCost)
                {
                    endCost = cost[count - 1, i];
                    endIndex = i;
                }
            }

            if (endIndex < 0 || double.IsInfinity(endCost))
            {
                return null;
            }

            var indexes = new int[count];
            indexes[count - 1] = endIndex;
            for (var k = count - 1; k > 0; k--)
            {
                indexes[k - 1] = previous[k, indexes[k]];
                if (indexes[k - 1] < 0)
                {
                    return null;
                }
            }

            var chosen = new List<double>(count);
            for (var k = 0; k < count; k++)
            {
                chosen.Add(usable[indexes[k]]);
            }

            return chosen;
        }

        private static bool GapsWithinMax(List<double> chosen, double wallLengthMm, double maxSpacingMm)
        {
            if (chosen == null || chosen.Count == 0)
            {
                return false;
            }

            var limit = maxSpacingMm + 80d;
            if (chosen[0] > limit || wallLengthMm - chosen[chosen.Count - 1] > limit)
            {
                return false;
            }

            for (var i = 1; i < chosen.Count; i++)
            {
                if (chosen[i] - chosen[i - 1] > limit)
                {
                    return false;
                }
            }

            return true;
        }

        internal static void Place(
            List<Atk60ElementPaintItem> outElements,
            Atk60WallPaintAnchor anchor,
            string idWall,
            double moduleBaseAlongMm,
            double moduleLengthMm,
            double yawRad,
            double ux,
            double uz,
            double faceSign,
            double wallHeightMm,
            double wallThicknessMm,
            double wallLengthMm,
            AttributesList attrs)
        {
            if (outElements == null || anchor == null || wallHeightMm < MinWallHeightMm)
            {
                return;
            }

            var upperOutside = Atk60FaceInsertion.FlagOn(attrs != null ? attrs._CHeckPropOutside : null);
            var upperInside = Atk60FaceInsertion.FlagOn(attrs != null ? attrs._CHeckPropInside : null);
            var lowerOutside = Atk60FaceInsertion.FlagOn(attrs != null ? attrs._CHeckPropOutsideInf : null);
            var lowerInside = Atk60FaceInsertion.FlagOn(attrs != null ? attrs._CHeckPropInsideInf : null);
            if (!upperOutside && !upperInside && !lowerOutside && !lowerInside)
            {
                return;
            }

            var stations = Stations(moduleLengthMm);
            if (stations.Count == 0)
            {
                return;
            }

            var parts = PartsForHeight(wallHeightMm);

            if (parts.Count == 0)
            {
                return;
            }

            for (var si = 0; si < stations.Count; si++)
            {
                var alongMm = moduleBaseAlongMm + stations[si];
                if (alongMm < 30d || alongMm > wallLengthMm - 30d)
                {
                    continue;
                }

                for (var pi = 0; pi < parts.Count; pi++)
                {
                    var part = parts[pi];
                    var showOutside = part.Lower ? lowerOutside : upperOutside;
                    var showInside = part.Lower ? lowerInside : upperInside;
                    if (!showOutside && !showInside)
                    {
                        continue;
                    }

                    AddBothFaces(
                        outElements, anchor, idWall,
                        part.Path, part.Code, part.Color,
                        alongMm, yawRad, ux, uz, faceSign,
                        wallHeightMm, wallThicknessMm, wallLengthMm,
                        showOutside, showInside, !part.Lower);
                }
            }
        }

        /// <summary>
        /// A 600 mm de cada extremo si el módulo llega a 2 m.
        /// Entre 1,20 m y 2 m, a 300 mm de cada extremo.
        /// </summary>
        private static List<double> Stations(double lengthMm)
        {
            var stations = new List<double>();
            var inset = 0d;
            if (lengthMm >= 2000d)
            {
                inset = 600d;
            }
            else if (lengthMm >= 1200d)
            {
                inset = 300d;
            }

            if (inset < 1d)
            {
                return stations;
            }

            stations.Add(inset);
            var far = lengthMm - inset;
            if (far - inset >= 400d)
            {
                stations.Add(far);
            }

            return stations;
        }

        private struct PropPart
        {
            public string Path;
            public string Code;
            public string Color;
            public bool Lower;
        }

        /// <summary>
        /// Misma tabla que el catálogo anterior. El cuerpo es la pieza exterior
        /// y el tubo es la interior. Por encima de 1,20 m se añade la de abajo.
        /// De 2,26 m a 2,55 m sigue siendo el puntal de 2,40 m. Por encima de 4,95 m
        /// el cuerpo es de una sola pieza, sin tubo interior.
        /// </summary>
        private static List<PropPart> PartsForHeight(double heightMm)
        {
            var key = "H03";
            if (heightMm > 300d && heightMm <= 455d) key = "H045";
            else if (heightMm > 455d && heightMm <= 605d) key = "H060";
            else if (heightMm > 605d && heightMm <= 755d) key = "H075";
            else if (heightMm > 755d && heightMm <= 905d) key = "H090";
            else if (heightMm > 905d && heightMm <= 1105d) key = "H0110";
            else if (heightMm > 1105d && heightMm <= 1205d) key = "H0120";
            else if (heightMm > 1205d && heightMm <= 1505d) key = "H0150";
            else if (heightMm > 1505d && heightMm <= 1805d) key = "H0180";
            else if (heightMm > 1805d && heightMm <= 1955d) key = "H0195";
            else if (heightMm > 1955d && heightMm <= 2105d) key = "H0210";
            else if (heightMm > 2105d && heightMm <= 2255d) key = "H0225";
            else if (heightMm > 2255d && heightMm <= 2555d) key = "H0240";
            else if (heightMm > 2555d && heightMm <= 2705d) key = "H0270";
            else if (heightMm > 2705d && heightMm <= 3605d) key = "H0300";
            else if (heightMm > 3605d && heightMm <= 4955d) key = "H0375";
            else if (heightMm > 4955d && heightMm <= 5405d) key = "HTipo3";
            else if (heightMm > 5405d && heightMm <= 5705d) key = "HTipo3-2";
            else if (heightMm > 5705d && heightMm <= 7350d) key = "HTipo4-41";

            string bodyFile;
            string bodyCode;
            string innerFile;
            string innerCode;
            string lowerFile;
            string lowerCode;
            string lowerInnerFile;
            string lowerInnerCode;
            switch (key)
            {
                case "H045":
                    bodyFile = "h045.stl"; bodyCode = "HTipo0"; innerFile = "h045_2.stl"; innerCode = "H0_2045";
                    lowerFile = "h270Inf.stl"; lowerCode = ""; lowerInnerFile = ""; lowerInnerCode = "";
                    break;
                case "H060":
                    bodyFile = "h060.stl"; bodyCode = "HTipo0"; innerFile = "h060_2.stl"; innerCode = "H0_2060";
                    lowerFile = "h270Inf.stl"; lowerCode = "H03"; lowerInnerFile = "h270Inf_2.stl"; lowerInnerCode = "H0_203";
                    break;
                case "H075":
                    bodyFile = "h075.stl"; bodyCode = "HTipo0"; innerFile = "h075_2.stl"; innerCode = "H0_2075";
                    lowerFile = "h270Inf.stl"; lowerCode = "H03"; lowerInnerFile = "h270Inf_2.stl"; lowerInnerCode = "H0_203";
                    break;
                case "H090":
                    bodyFile = "h090.stl"; bodyCode = "HTipo0"; innerFile = "h090_2.stl"; innerCode = "H0_2090";
                    lowerFile = "h270Inf.stl"; lowerCode = "H03"; lowerInnerFile = "h270Inf_2.stl"; lowerInnerCode = "H0_203";
                    break;
                case "H0110":
                    bodyFile = "h110.stl"; bodyCode = "HTipo0"; innerFile = "h110_2.stl"; innerCode = "H0_2110";
                    lowerFile = "h270Inf.stl"; lowerCode = "H03"; lowerInnerFile = "h270Inf_2.stl"; lowerInnerCode = "H0_203";
                    break;
                case "H0120":
                    bodyFile = "h120.stl"; bodyCode = "HTipo0"; innerFile = "h120_2.stl"; innerCode = "H0_2120";
                    lowerFile = "h270Inf.stl"; lowerCode = "H03"; lowerInnerFile = "h270Inf_2.stl"; lowerInnerCode = "H0_203";
                    break;
                case "H0150":
                    bodyFile = "h150.stl"; bodyCode = "HTipo0"; innerFile = "h150_2.stl"; innerCode = "H0_2150";
                    lowerFile = "h270Inf.stl"; lowerCode = "H03"; lowerInnerFile = "h270Inf_2.stl"; lowerInnerCode = "H0_203";
                    break;
                case "H0180":
                    bodyFile = "h180.stl"; bodyCode = "HTipo1"; innerFile = "h180_2.stl"; innerCode = "H0_2180";
                    lowerFile = "h270Inf.stl"; lowerCode = "H0_237"; lowerInnerFile = "h270Inf_2.stl"; lowerInnerCode = "H0_203";
                    break;
                case "H0195":
                    bodyFile = "h195.stl"; bodyCode = "HTipo1"; innerFile = "h195_2.stl"; innerCode = "H0_2195";
                    lowerFile = "h270Inf.stl"; lowerCode = "H0_237"; lowerInnerFile = "h270Inf_2.stl"; lowerInnerCode = "H0_203";
                    break;
                case "H0210":
                    bodyFile = "h210.stl"; bodyCode = "HTipo1"; innerFile = "h210_2.stl"; innerCode = "H0_2210";
                    lowerFile = "h270Inf.stl"; lowerCode = "H0_237"; lowerInnerFile = "h270Inf_2.stl"; lowerInnerCode = "H0_203";
                    break;
                case "H0225":
                    bodyFile = "h225.stl"; bodyCode = "HTipo1"; innerFile = "h225_2.stl"; innerCode = "H0_2225";
                    lowerFile = "h270Inf.stl"; lowerCode = "H0_237"; lowerInnerFile = "h270Inf_2.stl"; lowerInnerCode = "H0_203";
                    break;
                case "H0240":
                    bodyFile = "h240.stl"; bodyCode = "HTipo1"; innerFile = "h240_2.stl"; innerCode = "H0_2240";
                    lowerFile = "h270Inf.stl"; lowerCode = "H0_237"; lowerInnerFile = "h270Inf_2.stl"; lowerInnerCode = "H0_203";
                    break;
                case "H0270":
                    bodyFile = "h270.stl"; bodyCode = "HTipo1"; innerFile = "h270_2.stl"; innerCode = "H0_2270";
                    lowerFile = "h270Inf.stl"; lowerCode = "H0_237"; lowerInnerFile = "h270Inf_2.stl"; lowerInnerCode = "H0_203";
                    break;
                case "H0300":
                    bodyFile = "h300.stl"; bodyCode = "HTipo1"; innerFile = "h300_2.stl"; innerCode = "H0_2300";
                    lowerFile = "h270Inf.stl"; lowerCode = "H0_237"; lowerInnerFile = "h270Inf_2.stl"; lowerInnerCode = "H0_203";
                    break;
                case "H0375":
                    bodyFile = "h375.stl"; bodyCode = "HTipo2"; innerFile = "h375_2.stl"; innerCode = "H0_2375";
                    lowerFile = "h375Inf.stl"; lowerCode = "H0_237"; lowerInnerFile = "h375Inf_2.stl"; lowerInnerCode = "H0_237";
                    break;
                case "HTipo3":
                    bodyFile = "Puntal_Tipo3.stl"; bodyCode = "HTipo3"; innerFile = ""; innerCode = "";
                    lowerFile = "Puntal_Tipo3Inf.stl"; lowerCode = "HTipo1"; lowerInnerFile = ""; lowerInnerCode = "";
                    break;
                case "HTipo3-2":
                    bodyFile = "Puntal_Tipo3-2.stl"; bodyCode = "HTipo3-2"; innerFile = ""; innerCode = "";
                    lowerFile = "Puntal_Tipo3Inf.stl"; lowerCode = "HTipo1"; lowerInnerFile = ""; lowerInnerCode = "";
                    break;
                case "HTipo4-41":
                    bodyFile = "Puntal_Tipo3-41.stl"; bodyCode = "HTipo4-41"; innerFile = ""; innerCode = "";
                    lowerFile = "Puntal_Tipo3Inf.stl"; lowerCode = "HTipo1"; lowerInnerFile = ""; lowerInnerCode = "";
                    break;
                default:
                    bodyFile = "h03.stl"; bodyCode = "HTipo0"; innerFile = "h03_2.stl"; innerCode = "H0_203";
                    lowerFile = ""; lowerCode = ""; lowerInnerFile = ""; lowerInnerCode = "";
                    break;
            }

            var parts = new List<PropPart>();
            AddPart(parts, bodyFile, bodyCode, "prop", false);
            AddPart(parts, innerFile, innerCode, "prop-inner", false);
            if (heightMm > LowerPartFromMm)
            {
                AddPart(parts, lowerFile, lowerCode, "prop", true);
                AddPart(parts, lowerInnerFile, lowerInnerCode, "prop-inner", true);
            }

            return parts;
        }

        private static void AddPart(List<PropPart> parts, string file, string code, string color, bool lower)
        {
            if (string.IsNullOrWhiteSpace(file) || string.IsNullOrWhiteSpace(code))
            {
                return;
            }

            parts.Add(new PropPart
            {
                Path = "../../Content/DesignTools/Stl/ATK60/" + file,
                Code = code,
                Color = color,
                Lower = lower,
            });
        }

        /// <summary>
        /// Un puntal en un punto, de pie, mirando con la normal. Por debajo de 2 m no se pone.
        /// Por encima de 1,20 m añade también el inferior.
        /// </summary>
        internal static void PlaceSingle(
            List<Atk60ElementPaintItem> outElements,
            string idWall,
            double x,
            double y,
            double z,
            double rotY,
            double normalX,
            double normalZ,
            double heightMm)
        {
            if (outElements == null || heightMm < MinWallHeightMm)
            {
                return;
            }

            var parts = PartsForHeight(heightMm);
            for (var i = 0; i < parts.Count; i++)
            {
                var part = parts[i];
                if (part.Lower && heightMm <= LowerPartFromMm)
                {
                    continue;
                }

                var sx = x;
                var sy = y;
                var sz = z;
                if (!part.Lower)
                {
                    SeatOnRibHole(ref sx, ref sy, ref sz, normalX, normalZ, heightMm);
                }

                AddFace(
                    outElements, null, idWall, part.Path, part.Code, part.Color,
                    sx, sy, sz, rotY, normalX, normalZ, 1d,
                    0d, heightMm, 0d, 0d);
            }
        }

        private static void AddBothFaces(
            List<Atk60ElementPaintItem> outElements,
            Atk60WallPaintAnchor anchor,
            string idWall,
            string path,
            string code,
            string color,
            double alongMm,
            double yawRad,
            double ux,
            double uz,
            double faceSign,
            double wallHeightMm,
            double wallThicknessMm,
            double wallLengthMm,
            bool showOutside,
            bool showInside,
            bool seatHead)
        {
            var jointX = anchor.X + (ux * alongMm);
            var jointZ = anchor.Z + (uz * alongMm);
            if (showOutside)
            {
                var faceX = jointX + (anchor.NormalX * PanelFaceMm);
                var faceY = anchor.Y;
                var faceZ = jointZ + (anchor.NormalZ * PanelFaceMm);
                if (seatHead)
                {
                    SeatUpperHead(ref faceX, ref faceY, ref faceZ, anchor.NormalX, anchor.NormalZ, wallHeightMm);
                }

                AddFace(
                    outElements, anchor, idWall, path, code, color,
                    faceX,
                    faceY,
                    faceZ,
                    -yawRad + (faceSign < 0d ? Math.PI : 0d),
                    anchor.NormalX,
                    anchor.NormalZ,
                    faceSign,
                    alongMm,
                    wallHeightMm,
                    wallThicknessMm,
                    wallLengthMm);
            }

            if (!showInside)
            {
                return;
            }

            var oppositeX = jointX - (anchor.NormalX * wallThicknessMm);
            var oppositeZ = jointZ - (anchor.NormalZ * wallThicknessMm);
            var inX = oppositeX - (anchor.NormalX * PanelFaceMm);
            var inY = anchor.Y;
            var inZ = oppositeZ - (anchor.NormalZ * PanelFaceMm);
            if (seatHead)
            {
                SeatUpperHead(ref inX, ref inY, ref inZ, -anchor.NormalX, -anchor.NormalZ, wallHeightMm);
            }

            AddFace(
                outElements, anchor, idWall, path, code, color,
                inX,
                inY,
                inZ,
                -yawRad + (faceSign < 0d ? Math.PI : 0d) + Math.PI,
                -anchor.NormalX,
                -anchor.NormalZ,
                -faceSign,
                alongMm,
                wallHeightMm,
                wallThicknessMm,
                wallLengthMm);
        }

        /// <summary>
        /// Desfase del pasador a lo largo de la cara. Solo el h270; el resto
        /// se apoya en el taladro sin ese avance.
        /// </summary>
        internal static double ProfilePinLeadMm(double heightMm)
        {
            if (heightMm <= 2555d || heightMm > 2705d)
            {
                return 0d;
            }

            return H270PinAlongMm;
        }

        /// <summary>
        /// Mete el pasador del h270 en el agujero de la costilla: 50 mm por
        /// dentro de la cara exterior, a la altura de ese agujero.
        /// </summary>
        private static void SeatOnRibHole(
            ref double x,
            ref double y,
            ref double z,
            double normalX,
            double normalZ,
            double heightMm)
        {
            if (heightMm <= 2555d || heightMm > 2705d)
            {
                return;
            }

            var len = Math.Sqrt((normalX * normalX) + (normalZ * normalZ));
            if (len < 1e-6)
            {
                return;
            }

            var intoFace = H270PinOutMm + H270RibHoleInsideMm;
            y += H270RibHoleUpMm - H270PinUpMm;
            x -= (normalX / len) * intoFace;
            z -= (normalZ / len) * intoFace;
        }

        /// <summary>
        /// Sube el pasador del h270 al taladro superior y lo mete 50 mm por dentro de la cara.
        /// </summary>
        private static void SeatUpperHead(
            ref double x,
            ref double y,
            ref double z,
            double normalX,
            double normalZ,
            double heightMm)
        {
            if (heightMm <= 2555d || heightMm > 2705d)
            {
                return;
            }

            var len = Math.Sqrt((normalX * normalX) + (normalZ * normalZ));
            if (len < 1e-6)
            {
                return;
            }

            y += H270PinRaiseMm;
            x -= (normalX / len) * H270PinIntoFaceMm;
            z -= (normalZ / len) * H270PinIntoFaceMm;
        }

        private static void AddFace(
            List<Atk60ElementPaintItem> outElements,
            Atk60WallPaintAnchor anchor,
            string idWall,
            string path,
            string code,
            string color,
            double x,
            double y,
            double z,
            double rotY,
            double normalX,
            double normalZ,
            double faceSign,
            double alongMm,
            double wallHeightMm,
            double wallThicknessMm,
            double wallLengthMm)
        {
            outElements.Add(new Atk60ElementPaintItem
            {
                IdWall = !string.IsNullOrWhiteSpace(idWall) ? idWall : (anchor != null ? anchor.IdWall : null),
                ElementType = "Union",
                ElementCode = code,
                Orientation = "Vertical",
                ImportPath = path,
                Color = color,
                X = x,
                Y = y,
                Z = z,
                RotX = 0,
                RotY = rotY,
                RotZ = 0,
                NormalX = normalX,
                NormalZ = normalZ,
                FaceSign = faceSign,
                IsMirrored = false,
                WallHeightMm = wallHeightMm,
                WallThicknessMm = wallThicknessMm,
                WallLengthMm = wallLengthMm,
                LocalAlongMm = alongMm,
                LocalUpMm = 0d,
                UseStrictPose = true,
            });
        }
    }
}
