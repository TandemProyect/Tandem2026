using Desing.Repositories.RepositoryAtk60.ModulosATK60;
using Desing.Repositories.RepositoryCommun;
using System;
using System.Collections.Generic;

namespace Desing.Repositories.Repository_Desing_2.Atk60
{
    /// <summary>
    /// Esquina en L con angulares. El panel de esquina cierra por fuera.
    /// El angular cierra por dentro. Las grapas fijas van en el taladro del manual,
    /// en las dos caras y en los dos muros.
    /// </summary>
    internal static class LCornerFormwork
    {
        private const double LegMm = 300d;
        private const double ReachMm = 450d;
        private const string FixedPath = "../../Content/DesignTools/Stl/ATK60/10004220.stl";
        private const string FixedCode = "10004220";
        private const string StiffPath = "../../Content/DesignTools/Stl/ATK60/1850162.stl";
        private const string StiffCode = "1850162";
        private const string HookPath = "../../Content/DesignTools/Stl/ATK60/1850164.stl";
        private const string HookCode = "1850164";
        private const string AdjustablePath = "../../Content/DesignTools/Stl/ATK60/10000221.stl";
        private const string AdjustableCode = "10000221";
        private const string AdjustableSlidePath = "../../Content/DesignTools/Stl/ATK60/10000221-2.stl";
        private const string AdjustableSlideCode = "10000221B";

        internal static void Place(
            List<Atk60ElementPaintItem> outElements,
            List<Desing2FormworkWallDto> walls,
            List<Atk60WallPaintAnchor> anchors)
        {
            if (outElements == null || anchors == null)
            {
                return;
            }

            var runs = new List<Run>();
            for (var i = 0; i < anchors.Count; i++)
            {
                var anchor = anchors[i];
                if (anchor == null || anchor.Debug == null)
                {
                    continue;
                }

                var run = RunOf(anchor, WallOf(walls, anchor.IdWall));
                if (run != null)
                {
                    runs.Add(run);
                }
            }

            var seen = new HashSet<string>(StringComparer.Ordinal);
            var tVertices = new List<double[]>();
            if (anchors.Count >= 2)
            {
                PlaceTPanels(outElements, runs, tVertices);
                for (var a = 0; a < runs.Count; a++)
                {
                    for (var b = a + 1; b < runs.Count; b++)
                    {
                        TryPlace(outElements, runs, runs[a], runs[b], seen);
                    }
                }
            }

            SumRematesAtCorner(outElements, runs);
        }

        /// <summary>
        /// Si hay remate en la esquina y otro en el tramo, los paneles de en
        /// medio se corren y el hueco del tramo se cierra. Ningún tablón pasa
        /// de 0,15. No se suman los dos huecos: eso pintaba un tablón de 0,27.
        /// </summary>
        private static void SumRematesAtCorner(List<Atk60ElementPaintItem> items, List<Run> runs)
        {
            if (items == null || runs == null)
            {
                return;
            }

            for (var r = 0; r < runs.Count; r++)
            {
                SumRunRemates(items, runs[r]);
            }
        }

        private static void SumRunRemates(List<Atk60ElementPaintItem> items, Run run)
        {
            if (run == null)
            {
                return;
            }

            var woods = new List<Atk60ElementPaintItem>();
            for (var i = 0; i < items.Count; i++)
            {
                var item = items[i];
                if (item != null && SameWall(item.IdWall, run.Id) && IsRemateItem(item))
                {
                    woods.Add(item);
                }
            }

            if (woods.Count < 2)
            {
                return;
            }

            var stations = ClusterRemates(woods);
            if (stations.Count < 2)
            {
                return;
            }

            var corner = -1;
            var cornerDist = double.MaxValue;
            var cornerLen = -1d;
            for (var s = 0; s < stations.Count; s++)
            {
                var dist = StationEndDist(stations[s], run);
                var len = StationLength(stations[s]);
                var nearer = dist < cornerDist - 1d;
                var sameAndLonger = Math.Abs(dist - cornerDist) <= 1d && len > cornerLen;
                if (nearer || sameAndLonger)
                {
                    corner = s;
                    cornerDist = dist;
                    cornerLen = len;
                }
            }

            if (corner < 0 || cornerDist > 1100d)
            {
                return;
            }

            var kept = stations[corner];
            var cornerFace = CornerFaceWood(kept);
            if (cornerFace == null || run.Wall == null || run.Anchor == null)
            {
                return;
            }

            var grow = 0d;
            List<Atk60ElementPaintItem> gap = null;
            for (var s = 0; s < stations.Count; s++)
            {
                if (s == corner)
                {
                    continue;
                }

                var stationLen = StationLength(stations[s]);
                var hole = stationLen;
                if (hole <= 1d)
                {
                    continue;
                }

                ShiftPanelsAcrossGap(items, run, kept, stations[s], hole);
                DropRemateStation(items, stations[s], kept);
                grow += hole;
                gap = stations[s];
            }

            if (grow <= 1d || gap == null)
            {
                return;
            }

            // Tope 0,15. Sumar esquina + tramo (0,12 + 0,15) daba 0,27 y
            // saltaba el reparto de paneles. El tramo ya lo cierran los paneles.
            const double maxWoodMm = 150d;
            var longLen = cornerLen > maxWoodMm ? maxWoodMm : cornerLen;
            var shortLen = grow > maxWoodMm ? maxWoodMm : grow;
            if (shortLen > longLen - 1d && longLen > 1d)
            {
                shortLen = longLen - 1d;
            }

            double longStart;
            double shortStart;
            if (!BoardStarts(run, cornerFace, gap, longLen, shortLen, out longStart, out shortStart))
            {
                return;
            }

            PullBoardsOffPanels(items, run, cornerFace, gap, longLen, shortLen, ref longStart, ref shortStart);
            // El corto ya está en su sitio. El largo conserva el largo y se corre ahí.
            longStart = shortStart;

            DropWoodsAndClamps(items, kept);
            var primaryIsLong = cornerFace.FaceSign >= 0d;
            AddFaceBoard(items, run, longLen, longStart, primaryIsLong, !primaryIsLong, !primaryIsLong);
            AddFaceBoard(items, run, shortLen, shortStart, !primaryIsLong, primaryIsLong, primaryIsLong);
            StripFixedOnBoards(items, run, longStart, shortStart, longLen, shortLen);
        }

        /// <summary>
        /// El tablón de la esquina y el de la otra cara salen con el giro del muro.
        /// No se buscan: la cota usa el largo que se acaba de colocar.
        /// </summary>
        private static void AddFaceBoard(
            List<Atk60ElementPaintItem> items,
            Run run,
            double length,
            double startAlong,
            bool showPrimary,
            bool showOpposite,
            bool uprightOpposite)
        {
            if (length <= 1d || (!showPrimary && !showOpposite))
            {
                return;
            }

            var built = EndFiller.Build(
                run.Wall, run.Anchor, length, startAlong, showPrimary, showOpposite, uprightOpposite);
            for (var i = 0; i < built.Count; i++)
            {
                var item = built[i];
                if (item == null)
                {
                    continue;
                }

                item.IdWall = run.Id;
                items.Add(item);
            }
        }

        private static bool BoardStarts(
            Run run,
            Atk60ElementPaintItem cornerFace,
            List<Atk60ElementPaintItem> gap,
            double longLen,
            double shortLen,
            out double longStart,
            out double shortStart)
        {
            longStart = 0d;
            shortStart = 0d;
            if (cornerFace == null || gap == null || gap.Count == 0 || run.Anchor == null)
            {
                return false;
            }

            var yaw = Atk60FaceInsertion.NormalizeYawToRad(run.Anchor.RotY);
            var ux = Math.Cos(yaw);
            var uz = Math.Sin(yaw);
            var cornerAt = ((cornerFace.X - run.Anchor.X) * ux) + ((cornerFace.Z - run.Anchor.Z) * uz);
            var gapAt = ((gap[0].X - run.Anchor.X) * ux) + ((gap[0].Z - run.Anchor.Z) * uz);
            if (gapAt >= cornerAt)
            {
                longStart = cornerAt;
                shortStart = cornerAt;
            }
            else
            {
                longStart = cornerAt - longLen;
                shortStart = cornerAt - shortLen;
            }

            return true;
        }

        /// <summary>
        /// El tablón no puede entrar en el panel. Si el canto libre cae dentro
        /// de un panel amarillo, se corre hasta el canto de ese panel.
        /// </summary>
        private static void PullBoardsOffPanels(
            List<Atk60ElementPaintItem> items,
            Run run,
            Atk60ElementPaintItem cornerFace,
            List<Atk60ElementPaintItem> gap,
            double longLen,
            double shortLen,
            ref double longStart,
            ref double shortStart)
        {
            if (run.Anchor == null || cornerFace == null || gap == null || gap.Count == 0)
            {
                return;
            }

            var yaw = Atk60FaceInsertion.NormalizeYawToRad(run.Anchor.RotY);
            var ux = Math.Cos(yaw);
            var uz = Math.Sin(yaw);
            var cornerAt = ((cornerFace.X - run.Anchor.X) * ux) + ((cornerFace.Z - run.Anchor.Z) * uz);
            var gapAt = ((gap[0].X - run.Anchor.X) * ux) + ((gap[0].Z - run.Anchor.Z) * uz);
            var towardPos = gapAt >= cornerAt;
            var far = towardPos ? longStart + longLen : longStart;
            var best = double.MaxValue;
            var edge = far;
            var found = false;
            for (var i = 0; i < items.Count; i++)
            {
                var item = items[i];
                if (item == null || !SameWall(item.IdWall, run.Id))
                {
                    continue;
                }

                if (!string.Equals(item.ElementType, "Panel", StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                var color = item.Color ?? string.Empty;
                if (!string.Equals(color, "frame-yellow", StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                var width = item.PieceWidthMm > 1d ? item.PieceWidthMm : item.ModuleLengthMm;
                if (width <= 1d)
                {
                    continue;
                }

                var start = item.LocalAlongMm;
                var end = start + width;
                var near = towardPos ? start : end;
                if (far < start - 30d || far > end + 30d)
                {
                    continue;
                }

                var dist = Math.Abs(far - near);
                if (dist < best)
                {
                    best = dist;
                    edge = near;
                    found = true;
                }
            }

            if (!found || best <= 1d)
            {
                return;
            }

            if (towardPos)
            {
                longStart = edge - longLen;
                shortStart = edge - shortLen;
            }
            else
            {
                longStart = edge;
                shortStart = edge;
            }
        }

        /// <summary>
        /// En el tablón la junta es de grapa variable. La fija no se deja.
        /// </summary>
        private static void StripFixedOnBoards(
            List<Atk60ElementPaintItem> items,
            Run run,
            double longStart,
            double shortStart,
            double longLen,
            double shortLen)
        {
            var yaw = Atk60FaceInsertion.NormalizeYawToRad(run.Anchor.RotY);
            var ux = Math.Cos(yaw);
            var uz = Math.Sin(yaw);
            var from = Math.Min(longStart, shortStart) - 80d;
            var to = Math.Max(longStart + longLen, shortStart + shortLen) + 80d;
            for (var i = items.Count - 1; i >= 0; i--)
            {
                var item = items[i];
                if (item == null || !SameWall(item.IdWall, run.Id))
                {
                    continue;
                }

                if (!string.Equals(item.ElementCode, FixedCode, StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                var along = ((item.X - run.Anchor.X) * ux) + ((item.Z - run.Anchor.Z) * uz);
                if (along < from || along > to)
                {
                    continue;
                }

                items.RemoveAt(i);
            }
        }

        private static void DropWoodsAndClamps(List<Atk60ElementPaintItem> items, List<Atk60ElementPaintItem> woods)
        {
            for (var i = items.Count - 1; i >= 0; i--)
            {
                var item = items[i];
                if (item == null)
                {
                    continue;
                }

                var drop = false;
                for (var s = 0; s < woods.Count; s++)
                {
                    if (ReferenceEquals(item, woods[s]))
                    {
                        drop = true;
                        break;
                    }
                }

                if (!drop && IsVariableClamp(item))
                {
                    drop = NearRemates(item, woods);
                }

                if (drop)
                {
                    items.RemoveAt(i);
                }
            }
        }

        private static Atk60ElementPaintItem CornerFaceWood(List<Atk60ElementPaintItem> station)
        {
            Atk60ElementPaintItem best = null;
            var bestLen = -1d;
            for (var i = 0; i < station.Count; i++)
            {
                var len = station[i].PieceWidthMm > 1d ? station[i].PieceWidthMm : station[i].ModuleLengthMm;
                if (len > bestLen)
                {
                    best = station[i];
                    bestLen = len;
                }
            }

            return best;
        }

        /// <summary>
        /// Corre hacia el hueco lo que hay entre la esquina y el remate del tramo.
        /// </summary>
        private static void ShiftPanelsAcrossGap(
            List<Atk60ElementPaintItem> items,
            Run run,
            List<Atk60ElementPaintItem> corner,
            List<Atk60ElementPaintItem> gap,
            double hole)
        {
            double cornerSide;
            double gapSide;
            double toward;
            if (!GapSpan(run, corner, gap, out cornerSide, out gapSide, out toward))
            {
                return;
            }

            var slack = Math.Min(20d, Math.Max(8d, hole * 0.35d));
            var gapLimit = gapSide + (toward * slack);
            var shift = toward * hole;
            for (var i = 0; i < items.Count; i++)
            {
                var item = items[i];
                if (item == null || !SameWall(item.IdWall, run.Id) || IsRemateItem(item) || IsVariableClamp(item))
                {
                    continue;
                }

                var along = AlongOf(item, run);
                var between = toward > 0d
                    ? along >= cornerSide - 25d && along < gapLimit
                    : along <= cornerSide + 25d && along > gapLimit;
                if (!between)
                {
                    continue;
                }

                TranslateAlong(item, run, shift);
            }
        }

        private static bool GapSpan(
            Run run,
            List<Atk60ElementPaintItem> corner,
            List<Atk60ElementPaintItem> gap,
            out double cornerSide,
            out double gapSide,
            out double toward)
        {
            cornerSide = 0d;
            gapSide = 0d;
            toward = 1d;
            if (corner == null || corner.Count == 0 || gap == null || gap.Count == 0)
            {
                return false;
            }

            var gapAlong = AlongOf(gap[0], run);
            var best = double.MaxValue;
            for (var i = 0; i < corner.Count; i++)
            {
                double ax;
                double az;
                double bx;
                double bz;
                PieceEnds(corner[i], out ax, out az, out bx, out bz);
                var aAlong = ((ax - run.Sx) * run.Ux) + ((az - run.Sz) * run.Uz);
                var bAlong = ((bx - run.Sx) * run.Ux) + ((bz - run.Sz) * run.Uz);
                var aDist = Math.Abs(aAlong - gapAlong);
                var bDist = Math.Abs(bAlong - gapAlong);
                var side = aDist <= bDist ? aAlong : bAlong;
                var dist = Math.Min(aDist, bDist);
                if (dist < best)
                {
                    best = dist;
                    cornerSide = side;
                }
            }

            var cornerAlong = AlongOf(corner[0], run);
            best = double.MaxValue;
            for (var i = 0; i < gap.Count; i++)
            {
                double ax;
                double az;
                double bx;
                double bz;
                PieceEnds(gap[i], out ax, out az, out bx, out bz);
                var aAlong = ((ax - run.Sx) * run.Ux) + ((az - run.Sz) * run.Uz);
                var bAlong = ((bx - run.Sx) * run.Ux) + ((bz - run.Sz) * run.Uz);
                var aDist = Math.Abs(aAlong - cornerAlong);
                var bDist = Math.Abs(bAlong - cornerAlong);
                var side = aDist <= bDist ? aAlong : bAlong;
                var dist = Math.Min(aDist, bDist);
                if (dist < best)
                {
                    best = dist;
                    gapSide = side;
                }
            }

            toward = gapSide >= cornerSide ? 1d : -1d;
            return Math.Abs(gapSide - cornerSide) > 1d;
        }

        private static void PieceEnds(
            Atk60ElementPaintItem wood,
            out double ax,
            out double az,
            out double bx,
            out double bz)
        {
            double lx;
            double ly;
            double lz;
            RemateLocalX(wood, out lx, out ly, out lz);
            var len = wood.PieceWidthMm > 1d ? wood.PieceWidthMm : wood.ModuleLengthMm;
            ax = wood.X;
            az = wood.Z;
            bx = wood.X + (lx * len);
            bz = wood.Z + (lz * len);
        }

        private static double AlongOf(Atk60ElementPaintItem item, Run run)
        {
            return ((item.X - run.Sx) * run.Ux) + ((item.Z - run.Sz) * run.Uz);
        }

        private static void TranslateAlong(Atk60ElementPaintItem item, Run run, double shift)
        {
            item.X += run.Ux * shift;
            item.Z += run.Uz * shift;
            item.LocalAlongMm += shift;
        }

        private static void DropRemateStation(
            List<Atk60ElementPaintItem> items,
            List<Atk60ElementPaintItem> station,
            List<Atk60ElementPaintItem> kept)
        {
            for (var i = items.Count - 1; i >= 0; i--)
            {
                var item = items[i];
                if (item == null)
                {
                    continue;
                }

                var drop = false;
                for (var s = 0; s < station.Count; s++)
                {
                    if (ReferenceEquals(item, station[s]))
                    {
                        drop = true;
                        break;
                    }
                }

                if (!drop && IsVariableClamp(item) && !NearRemates(item, kept))
                {
                    drop = NearRemates(item, station);
                }

                if (drop)
                {
                    items.RemoveAt(i);
                }
            }
        }

        private static bool NearRemates(Atk60ElementPaintItem item, List<Atk60ElementPaintItem> woods)
        {
            for (var s = 0; s < woods.Count; s++)
            {
                var wood = woods[s];
                double lx;
                double ly;
                double lz;
                RemateLocalX(wood, out lx, out ly, out lz);
                var len = wood.PieceWidthMm > 1d ? wood.PieceWidthMm : wood.ModuleLengthMm;
                var farX = wood.X + (lx * len);
                var farZ = wood.Z + (lz * len);
                var backX = wood.X - (lx * len);
                var backZ = wood.Z - (lz * len);
                if (NearPoint(item, wood.X, wood.Z) || NearPoint(item, farX, farZ) || NearPoint(item, backX, backZ))
                {
                    return true;
                }
            }

            return false;
        }

        private static bool NearPoint(Atk60ElementPaintItem item, double x, double z)
        {
            var dx = item.X - x;
            var dz = item.Z - z;
            return (dx * dx) + (dz * dz) <= 220d * 220d;
        }

        private static List<List<Atk60ElementPaintItem>> ClusterRemates(List<Atk60ElementPaintItem> woods)
        {
            var stations = new List<List<Atk60ElementPaintItem>>();
            var used = new bool[woods.Count];
            for (var i = 0; i < woods.Count; i++)
            {
                if (used[i])
                {
                    continue;
                }

                var station = new List<Atk60ElementPaintItem>();
                station.Add(woods[i]);
                used[i] = true;
                var grew = true;
                while (grew)
                {
                    grew = false;
                    for (var j = 0; j < woods.Count; j++)
                    {
                        if (used[j])
                        {
                            continue;
                        }

                        for (var k = 0; k < station.Count; k++)
                        {
                            if (!SameRemateStation(station[k], woods[j]))
                            {
                                continue;
                            }

                            station.Add(woods[j]);
                            used[j] = true;
                            grew = true;
                            break;
                        }
                    }
                }

                stations.Add(station);
            }

            return stations;
        }

        private static bool SameRemateStation(Atk60ElementPaintItem a, Atk60ElementPaintItem b)
        {
            var dx = a.X - b.X;
            var dz = a.Z - b.Z;
            var dist2 = (dx * dx) + (dz * dz);
            var along = Math.Abs(a.LocalAlongMm - b.LocalAlongMm) <= 80d;
            if (along && dist2 <= 500d * 500d)
            {
                return true;
            }

            return dist2 <= 250d * 250d;
        }

        private static double StationLength(List<Atk60ElementPaintItem> station)
        {
            var length = 0d;
            for (var i = 0; i < station.Count; i++)
            {
                var piece = station[i].PieceWidthMm;
                if (piece < 1d)
                {
                    piece = station[i].ModuleLengthMm;
                }

                if (piece > length)
                {
                    length = piece;
                }
            }

            return length;
        }

        private static double StationEndDist(List<Atk60ElementPaintItem> station, Run run)
        {
            var best = double.MaxValue;
            for (var i = 0; i < station.Count; i++)
            {
                var item = station[i];
                var ds = ((item.X - run.Sx) * (item.X - run.Sx)) + ((item.Z - run.Sz) * (item.Z - run.Sz));
                var de = ((item.X - run.Ex) * (item.X - run.Ex)) + ((item.Z - run.Ez) * (item.Z - run.Ez));
                var dist = Math.Sqrt(Math.Min(ds, de));
                if (dist < best)
                {
                    best = dist;
                }
            }

            return best;
        }

        private static void RemateLocalX(Atk60ElementPaintItem item, out double x, out double y, out double z)
        {
            var cy = Math.Cos(item.RotY);
            var sy = Math.Sin(item.RotY);
            var cz = Math.Cos(item.RotZ);
            var sz = Math.Sin(item.RotZ);
            x = cz * cy;
            y = sz * cy;
            z = -sy;
        }

        private static bool IsRemateItem(Atk60ElementPaintItem item)
        {
            var code = item.ElementCode ?? string.Empty;
            return string.Equals(item.ElementType, "Remate", StringComparison.OrdinalIgnoreCase)
                || string.Equals(code, "REMATE_WOOD", StringComparison.OrdinalIgnoreCase);
        }

        private static bool IsVariableClamp(Atk60ElementPaintItem item)
        {
            if (item == null)
            {
                return false;
            }

            var code = item.ElementCode ?? string.Empty;
            return string.Equals(code, AdjustableCode, StringComparison.OrdinalIgnoreCase)
                || string.Equals(code, AdjustableSlideCode, StringComparison.OrdinalIgnoreCase);
        }

        /// <summary>
        /// Tipo 1 de la T: un panel de esquina en cada rincón interior.
        /// El giro es el de la L: las dos normales hacia el cuerpo, y el
        /// extremo (0, 300) cae en el punto rojo o en el azul.
        /// </summary>
        private static void PlaceTPanels(
            List<Atk60ElementPaintItem> outElements,
            List<Run> runs,
            List<double[]> tVertices)
        {
            if (outElements == null || runs == null || runs.Count < 2)
            {
                return;
            }

            var ends = new List<EndHit>();
            for (var i = 0; i < runs.Count; i++)
            {
                ends.Add(new EndHit { Run = runs[i], AtEnd = false });
                ends.Add(new EndHit { Run = runs[i], AtEnd = true });
            }

            var used = new bool[ends.Count];
            for (var i = 0; i < ends.Count; i++)
            {
                if (used[i])
                {
                    continue;
                }

                double ix;
                double iz;
                if (!EndPoint(ends[i].Run, ends[i].AtEnd, out ix, out iz))
                {
                    continue;
                }

                var cluster = new List<EndHit>();
                cluster.Add(ends[i]);
                used[i] = true;
                for (var j = i + 1; j < ends.Count; j++)
                {
                    if (used[j])
                    {
                        continue;
                    }

                    double jx;
                    double jz;
                    if (!EndPoint(ends[j].Run, ends[j].AtEnd, out jx, out jz))
                    {
                        continue;
                    }

                    var dx = jx - ix;
                    var dz = jz - iz;
                    if ((dx * dx) + (dz * dz) > 150d * 150d)
                    {
                        continue;
                    }

                    cluster.Add(ends[j]);
                    used[j] = true;
                }

                if (cluster.Count >= 3)
                {
                    PlaceTFromCluster(outElements, cluster, tVertices);
                }
            }

            for (var bi = 0; bi < ends.Count; bi++)
            {
                PlaceTFromBody(outElements, ends[bi], runs, tVertices);
            }

            PlaceTFromFacingGaps(outElements, runs, tVertices);
        }

        private static void PlaceTFromCluster(
            List<Atk60ElementPaintItem> outElements,
            List<EndHit> cluster,
            List<double[]> tVertices)
        {
            EndHit throughA = null;
            EndHit throughB = null;
            var best = 1d;
            for (var i = 0; i < cluster.Count; i++)
            {
                double aix;
                double aiz;
                IntoFromEnd(cluster[i], out aix, out aiz);
                for (var j = i + 1; j < cluster.Count; j++)
                {
                    double bix;
                    double biz;
                    IntoFromEnd(cluster[j], out bix, out biz);
                    var dot = (aix * bix) + (aiz * biz);
                    if (dot < best)
                    {
                        best = dot;
                        throughA = cluster[i];
                        throughB = cluster[j];
                    }
                }
            }

            if (throughA == null || best > -0.85d)
            {
                return;
            }

            double tax;
            double taz;
            IntoFromEnd(throughA, out tax, out taz);
            EndHit branch = null;
            EndHit branchOther = null;
            var branches = 0;
            for (var i = 0; i < cluster.Count; i++)
            {
                if (ReferenceEquals(cluster[i], throughA) || ReferenceEquals(cluster[i], throughB))
                {
                    continue;
                }

                double ix;
                double iz;
                IntoFromEnd(cluster[i], out ix, out iz);
                if (Math.Abs((ix * tax) + (iz * taz)) >= 0.35d)
                {
                    continue;
                }

                branches++;
                if (branch == null)
                {
                    branch = cluster[i];
                }
                else if (branchOther == null)
                {
                    branchOther = cluster[i];
                }
            }

            double vxEarly = 0d;
            double vzEarly = 0d;
            for (var i = 0; i < cluster.Count; i++)
            {
                double x;
                double z;
                EndPoint(cluster[i].Run, cluster[i].AtEnd, out x, out z);
                vxEarly += x;
                vzEarly += z;
            }

            vxEarly /= cluster.Count;
            vzEarly /= cluster.Count;
            if (branches == 2 && branch != null && branchOther != null
                && OppositeEnds(branch, branchOther))
            {
                var throughThickPlus = (throughA.Run.ThicknessMm + throughB.Run.ThicknessMm) * 0.5d;
                PlacePlus(
                    outElements, throughA, throughB, branch, branchOther,
                    throughThickPlus, vxEarly, vzEarly, tVertices);
                return;
            }

            if (branches != 1 || branch == null)
            {
                return;
            }

            var wallIds = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            for (var w = 0; w < cluster.Count; w++)
            {
                if (cluster[w].Run != null && !string.IsNullOrWhiteSpace(cluster[w].Run.Id))
                {
                    wallIds.Add(cluster[w].Run.Id);
                }
            }

            if (wallIds.Count < 3)
            {
                return;
            }

            double vx = 0d;
            double vz = 0d;
            for (var i = 0; i < cluster.Count; i++)
            {
                double x;
                double z;
                EndPoint(cluster[i].Run, cluster[i].AtEnd, out x, out z);
                vx += x;
                vz += z;
            }

            vx /= cluster.Count;
            vz /= cluster.Count;
            StripTapePanelAtEnd(outElements, throughA.Run, throughA.AtEnd);
            StripTapePanelAtEnd(outElements, throughB.Run, throughB.AtEnd);
            var throughThick = (throughA.Run.ThicknessMm + throughB.Run.ThicknessMm) * 0.5d;
            PlaceTPair(outElements, branch, throughA.Run, throughThick, vx, vz, tVertices, throughA, throughB);
        }

        private static void PlaceTFromBody(
            List<Atk60ElementPaintItem> outElements,
            EndHit branchEnd,
            List<Run> runs,
            List<double[]> tVertices)
        {
            double ex;
            double ez;
            if (!EndPoint(branchEnd.Run, branchEnd.AtEnd, out ex, out ez))
            {
                return;
            }

            if (NearTVertex(tVertices, ex, ez))
            {
                return;
            }

            for (var i = 0; i < runs.Count; i++)
            {
                var through = runs[i];
                if (ReferenceEquals(through, branchEnd.Run))
                {
                    continue;
                }

                var parallel = Math.Abs((branchEnd.Run.Ux * through.Ux) + (branchEnd.Run.Uz * through.Uz));
                if (parallel > 0.85d)
                {
                    continue;
                }

                var rx = ex - through.Sx;
                var rz = ez - through.Sz;
                var along = (rx * through.Ux) + (rz * through.Uz);
                var margin = Math.Max(200d, through.ThicknessMm);
                if (along < margin || along > through.LengthMm - margin)
                {
                    continue;
                }

                var lateral = Math.Abs((rx * through.Uz) - (rz * through.Ux));
                if (lateral > (through.ThicknessMm * 0.5d) + 80d)
                {
                    continue;
                }

                // Cerca de un extremo es una L: la esquina interior la pone TryPlace.
                var endClear = Math.Max(900d, through.ThicknessMm + LegMm);
                if (along < endClear || along > through.LengthMm - endClear)
                {
                    continue;
                }

                var vx = through.Sx + (through.Ux * along);
                var vz = through.Sz + (through.Uz * along);
                if (NearTVertex(tVertices, vx, vz))
                {
                    continue;
                }

                var opposite = OppositeBranchNear(runs, branchEnd, through, vx, vz);
                if (opposite != null)
                {
                    PlacePlus(
                        outElements, null, null, branchEnd, opposite,
                        through.ThicknessMm, vx, vz, tVertices);
                    return;
                }

                PlaceTPair(outElements, branchEnd, through, through.ThicknessMm, vx, vz, tVertices, null, null);
                return;
            }
        }

        private static void PlaceTPair(
            List<Atk60ElementPaintItem> outElements,
            EndHit branchEnd,
            Run through,
            double throughThickMm,
            double vx,
            double vz,
            List<double[]> tVertices,
            EndHit throughEndA,
            EndHit throughEndB)
        {
            if (NearTVertex(tVertices, vx, vz))
            {
                return;
            }

            double intoBx;
            double intoBz;
            IntoFromEnd(branchEnd, out intoBx, out intoBz);
            var halfBranch = branchEnd.Run.ThicknessMm * 0.5d;
            var px = -intoBz;
            var pz = intoBx;
            // Mismo espesor: las dos esquinas en el plano de siempre.
            // Distinto: la del brazo fino en Cara A (la intersección del escalón)
            // y la del grueso en Cara D. El giro no cambia.
            double nearThin;
            double nearThick;
            double thinSign;
            double stepMm;
            Run thinRun;
            double thinIntoX;
            double thinIntoZ;
            if (!TryTipo2Step(
                throughThickMm, throughEndA, throughEndB, px, pz,
                out nearThin, out nearThick, out thinSign, out stepMm, out thinRun,
                out thinIntoX, out thinIntoZ))
            {
                nearThin = throughThickMm * 0.5d;
                nearThick = nearThin;
                thinSign = 1d;
                stepMm = 0d;
                thinRun = null;
            }

            var firstNear = thinSign > 0d ? nearThin : nearThick;
            var secondNear = thinSign > 0d ? nearThick : nearThin;
            StripTapePanelAtEnd(outElements, branchEnd.Run, branchEnd.AtEnd);
            if (throughEndA != null)
            {
                StripTapePanelAtEnd(outElements, throughEndA.Run, throughEndA.AtEnd);
            }

            if (throughEndB != null)
            {
                StripTapePanelAtEnd(outElements, throughEndB.Run, throughEndB.AtEnd);
            }

            PlaceTPanel(
                outElements, branchEnd.Run,
                vx + (intoBx * firstNear) + (px * halfBranch),
                vz + (intoBz * firstNear) + (pz * halfBranch),
                -intoBx, -intoBz, -px, -pz, Math.PI);
            PlaceTPanel(
                outElements, branchEnd.Run,
                vx + (intoBx * secondNear) - (px * halfBranch),
                vz + (intoBz * secondNear) - (pz * halfBranch),
                -intoBx, -intoBz, px, pz, Math.PI);
            if (through != null)
            {
                PlaceCaraE(outElements, through, branchEnd.Run.ThicknessMm, vx, vz, intoBx, intoBz);
            }

            PlaceClamps(outElements, branchEnd.Run, branchEnd.AtEnd);
            if (throughEndA != null)
            {
                PlaceClamps(outElements, throughEndA.Run, throughEndA.AtEnd);
            }

            if (throughEndB != null)
            {
                PlaceClamps(outElements, throughEndB.Run, throughEndB.AtEnd);
            }

            if (thinRun != null && stepMm > 1d)
            {
                var delta = stepMm * 0.5d;
                ShiftWall(outElements, thinRun.Id, vx, vz, -intoBx * delta, -intoBz * delta);
                // La pata de la esquina llega 300 mm desde el punto de inserción.
                // El tablón es el sobrante, justo después de esa pata, hasta el panel vertical.
                var reach = thinRun.HeightMm >= 2000d ? LegMm : 294d;
                var yawThin = PanelYaw(-intoBx, -intoBz, -thinSign * px, -thinSign * pz) + Math.PI;
                var legYx = Math.Sin(yawThin);
                var legYz = -Math.Cos(yawThin);
                var legXx = Math.Cos(yawThin);
                var legXz = Math.Sin(yawThin);
                var originInto = ((-reach * legYx) * intoBx) + ((-reach * legYz) * intoBz);
                var xEndInto = originInto + ((reach * legXx) * intoBx) + ((reach * legXz) * intoBz);
                var advance = Math.Max(0d, Math.Max(originInto, xEndInto));
                var redX = vx + (intoBx * (nearThin + advance)) + (thinSign * px * halfBranch);
                var redZ = vz + (intoBz * (nearThin + advance)) + (thinSign * pz * halfBranch);
                PlaceStepRemate(
                    outElements, branchEnd.Run,
                    redX, redZ, intoBx, intoBz, thinIntoX, thinIntoZ, stepMm);
            }

            tVertices.Add(new[] { vx, vz });
        }

        /// <summary>
        /// Cruce en +. Las esquinas interiores son las de la T, y la otra
        /// mitad es la misma pareja al otro lado. Sin cara E y sin angular.
        /// </summary>
        private static void PlacePlus(
            List<Atk60ElementPaintItem> outElements,
            EndHit throughA,
            EndHit throughB,
            EndHit branchA,
            EndHit branchB,
            double throughThickMm,
            double vx,
            double vz,
            List<double[]> tVertices)
        {
            if (branchA == null || branchB == null || branchA.Run == null || branchB.Run == null)
            {
                return;
            }

            if (NearTVertex(tVertices, vx, vz))
            {
                return;
            }

            StripTapePanelAtEnd(outElements, branchA.Run, branchA.AtEnd);
            StripTapePanelAtEnd(outElements, branchB.Run, branchB.AtEnd);
            if (throughA != null)
            {
                StripTapePanelAtEnd(outElements, throughA.Run, throughA.AtEnd);
            }

            if (throughB != null)
            {
                StripTapePanelAtEnd(outElements, throughB.Run, throughB.AtEnd);
            }

            PlacePlusSide(outElements, branchA, throughThickMm, vx, vz);
            PlacePlusSide(outElements, branchB, throughThickMm, vx, vz);
            PlaceClamps(outElements, branchA.Run, branchA.AtEnd);
            PlaceClamps(outElements, branchB.Run, branchB.AtEnd);
            if (throughA != null)
            {
                PlaceClamps(outElements, throughA.Run, throughA.AtEnd);
            }

            if (throughB != null)
            {
                PlaceClamps(outElements, throughB.Run, throughB.AtEnd);
            }

            tVertices.Add(new[] { vx, vz });
        }

        /// <summary>
        /// Los dos paneles de esquina interior de un lado, simétricos.
        /// El otro lado del + repite la llamada con el muro contrario.
        /// </summary>
        private static void PlacePlusSide(
            List<Atk60ElementPaintItem> outElements,
            EndHit branchEnd,
            double throughThickMm,
            double vx,
            double vz)
        {
            double intoBx;
            double intoBz;
            IntoFromEnd(branchEnd, out intoBx, out intoBz);
            var halfBranch = branchEnd.Run.ThicknessMm * 0.5d;
            var near = throughThickMm * 0.5d;
            var px = -intoBz;
            var pz = intoBx;
            PlaceTPanel(
                outElements, branchEnd.Run,
                vx + (intoBx * near) + (px * halfBranch),
                vz + (intoBz * near) + (pz * halfBranch),
                -intoBx, -intoBz, -px, -pz, Math.PI);
            PlaceTPanel(
                outElements, branchEnd.Run,
                vx + (intoBx * near) - (px * halfBranch),
                vz + (intoBz * near) - (pz * halfBranch),
                -intoBx, -intoBz, px, pz, Math.PI);
        }

        private static bool OppositeEnds(EndHit a, EndHit b)
        {
            if (a == null || b == null || a.Run == null || b.Run == null)
            {
                return false;
            }

            if (SameWall(a.Run.Id, b.Run.Id))
            {
                return false;
            }

            double aix;
            double aiz;
            double bix;
            double biz;
            IntoFromEnd(a, out aix, out aiz);
            IntoFromEnd(b, out bix, out biz);
            return (aix * bix) + (aiz * biz) < -0.85d;
        }

        private static bool SameBranchEnd(EndHit a, EndHit b)
        {
            return a != null && b != null && a.Run != null && b.Run != null
                && SameWall(a.Run.Id, b.Run.Id) && a.AtEnd == b.AtEnd;
        }

        private static EndHit OppositeBranchNear(
            List<Run> runs,
            EndHit branchEnd,
            Run through,
            double vx,
            double vz)
        {
            if (runs == null || branchEnd == null || branchEnd.Run == null)
            {
                return null;
            }

            for (var i = 0; i < runs.Count; i++)
            {
                var run = runs[i];
                if (run == null || ReferenceEquals(run, branchEnd.Run) || ReferenceEquals(run, through))
                {
                    continue;
                }

                if (Math.Abs((run.Ux * through.Ux) + (run.Uz * through.Uz)) > 0.35d)
                {
                    continue;
                }

                var flags = new[] { false, true };
                for (var f = 0; f < flags.Length; f++)
                {
                    double ex;
                    double ez;
                    if (!EndPoint(run, flags[f], out ex, out ez))
                    {
                        continue;
                    }

                    var dx = ex - vx;
                    var dz = ez - vz;
                    if ((dx * dx) + (dz * dz) > 900d * 900d)
                    {
                        continue;
                    }

                    var hit = new EndHit { Run = run, AtEnd = flags[f] };
                    if (OppositeEnds(branchEnd, hit))
                    {
                        return hit;
                    }
                }
            }

            return null;
        }

        /// <summary>
        /// Tipo 2: los dos atravesados no miden igual. Cara A es la cara fina
        /// hacia la rama, después de correr el muro hacia fuera. Cara D es la
        /// del grueso. El escalón entre las dos es el espesor que falta.
        /// </summary>
        private static bool TryTipo2Step(
            double throughThickMm,
            EndHit throughEndA,
            EndHit throughEndB,
            double px,
            double pz,
            out double nearThin,
            out double nearThick,
            out double thinSign,
            out double stepMm,
            out Run thinRun,
            out double thinIntoX,
            out double thinIntoZ)
        {
            nearThick = throughThickMm * 0.5d;
            nearThin = nearThick;
            thinSign = 1d;
            stepMm = 0d;
            thinRun = null;
            thinIntoX = px;
            thinIntoZ = pz;
            if (throughEndA == null || throughEndB == null
                || throughEndA.Run == null || throughEndB.Run == null)
            {
                return false;
            }

            var eA = throughEndA.Run.ThicknessMm;
            var eB = throughEndB.Run.ThicknessMm;
            var eRef = Math.Max(eA, eB);
            var eThin = Math.Min(eA, eB);
            stepMm = eRef - eThin;
            if (stepMm <= 1d)
            {
                return false;
            }

            nearThick = eRef * 0.5d;
            nearThin = eThin - nearThick;
            var thinEnd = eA <= eB ? throughEndA : throughEndB;
            thinRun = thinEnd.Run;
            IntoFromEnd(thinEnd, out thinIntoX, out thinIntoZ);
            var dot = (thinIntoX * px) + (thinIntoZ * pz);
            thinSign = dot >= 0d ? 1d : -1d;
            return true;
        }

        private static void ShiftWall(
            List<Atk60ElementPaintItem> items,
            string wallId,
            double vertexX,
            double vertexZ,
            double dx,
            double dz)
        {
            if (items == null || (Math.Abs(dx) < 0.01d && Math.Abs(dz) < 0.01d))
            {
                return;
            }

            // Junto a la esquina no se corre nada: ahí el panel se metía en el muro.
            // El ancla de un panel largo puede quedar a más de 800 mm y el panel
            // igual llega a la esquina: ese tampoco se corre. Remates y grapas
            // variables se quedan donde están. El resto del muro fino se corre junto.
            var keep2 = 800d * 800d;
            for (var i = 0; i < items.Count; i++)
            {
                var item = items[i];
                if (item == null || !SameWall(item.IdWall, wallId))
                {
                    continue;
                }

                var code = item.ElementCode ?? string.Empty;
                var isRemate = string.Equals(item.ElementType, "Remate", StringComparison.OrdinalIgnoreCase)
                    || string.Equals(code, "REMATE_WOOD", StringComparison.OrdinalIgnoreCase);
                var isVariable = string.Equals(code, AdjustableCode, StringComparison.OrdinalIgnoreCase)
                    || string.Equals(code, AdjustableSlideCode, StringComparison.OrdinalIgnoreCase);
                if (isRemate || isVariable)
                {
                    continue;
                }

                var px = item.X - vertexX;
                var pz = item.Z - vertexZ;
                var dist2 = (px * px) + (pz * pz);
                if (dist2 <= keep2 || StraightPanelReachesCorner(item, dist2))
                {
                    continue;
                }

                item.X += dx;
                item.Z += dz;
            }
        }

        /// <summary>
        /// Panel recto cuyo cuerpo llega a la esquina aunque el ancla esté más lejos.
        /// Si se corre con el muro fino, ese panel entra en el muro de la T.
        /// </summary>
        private static bool StraightPanelReachesCorner(Atk60ElementPaintItem item, double dist2)
        {
            if (item == null
                || !string.Equals(item.ElementType, "Panel", StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }

            var orientation = item.Orientation ?? string.Empty;
            if (!string.Equals(orientation, "Vertical", StringComparison.OrdinalIgnoreCase)
                && !string.Equals(orientation, "Tumbado", StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }

            var reach = item.PieceWidthMm;
            if (reach <= 1d)
            {
                return false;
            }

            var pieceEnd = item.LocalAlongMm + reach;
            var reachesWallEnd = item.WallLengthMm - pieceEnd <= 800d;
            var reachesWallStart = item.LocalAlongMm <= 800d;
            if (!reachesWallEnd && !reachesWallStart)
            {
                return false;
            }

            var dist = Math.Sqrt(dist2);
            return dist - reach <= 800d;
        }

        /// <summary>
        /// Taco entre la esquina interior y el panel vertical.
        /// A lo largo mide el hueco, nunca más de 0,15. El ancho es 0,12, el del panel.
        /// Grapa variable en los dos cantos. La fija de ese hueco no une dos paneles.
        /// </summary>
        private static void PlaceStepRemate(
            List<Atk60ElementPaintItem> outElements,
            Run owner,
            double redX,
            double redZ,
            double intoBx,
            double intoBz,
            double thinIntoX,
            double thinIntoZ,
            double stepMm)
        {
            if (owner == null || owner.Anchor == null || stepMm <= 1d)
            {
                return;
            }

            var length = stepMm > 150d ? 150d : stepMm;
            var depth = Atk60FaceInsertion.PanelFaceMm;
            var originX = redX;
            var originZ = redZ;
            var leftX = -thinIntoZ;
            var leftZ = thinIntoX;
            var growsOut = (leftX * intoBx) + (leftZ * intoBz) >= 0d;
            if (!growsOut)
            {
                originX += intoBx * depth;
                originZ += intoBz * depth;
            }

            var height = owner.HeightMm > 1d ? owner.HeightMm : 2700d;
            var yaw = Math.Atan2(thinIntoZ, thinIntoX);
            outElements.Add(new Atk60ElementPaintItem
            {
                IdWall = owner.Id,
                ElementType = "Remate",
                ElementCode = "REMATE_WOOD",
                Orientation = "Vertical",
                ImportPath = string.Empty,
                Color = "wood-remate",
                X = originX,
                Y = owner.Anchor.Y,
                Z = originZ,
                RotX = 0d,
                RotY = -yaw,
                RotZ = 0d,
                NormalX = intoBx,
                NormalZ = intoBz,
                FaceSign = owner.Anchor.FaceSign,
                IsMirrored = false,
                ModuleLengthMm = length,
                ModuleIndex = 1,
                ModuleCountInWall = 1,
                WallHeightMm = height,
                WallThicknessMm = depth,
                WallLengthMm = owner.LengthMm,
                PieceWidthMm = length,
                PieceHeightMm = height,
                LocalAlongMm = 0d,
                LocalUpMm = 0d,
                InsertOffsetX = 0d,
                InsertOffsetY = 0d,
                InsertOffsetZ = 0d,
                BaseRotX = 0d,
                BaseRotY = 0d,
                BaseRotZ = 0d,
                UseStrictPose = true,
                PieceIndexInModule = 1,
                PieceCountInModule = 1,
                CatalogHeightMm = height,
            });

            StripUnionAtRemate(
                outElements, originX, originZ, thinIntoX, thinIntoZ, leftX, leftZ, length, depth);
            PlaceRemateVariableClamps(
                outElements, owner, originX, originZ, thinIntoX, thinIntoZ, intoBx, intoBz, length, depth, growsOut);
        }

        /// <summary>
        /// Grapa regulable en los dos cantos del tablón, en la cara de fuera.
        /// </summary>
        private static void PlaceRemateVariableClamps(
            List<Atk60ElementPaintItem> outElements,
            Run owner,
            double originX,
            double originZ,
            double alongX,
            double alongZ,
            double intoBx,
            double intoBz,
            double length,
            double depth,
            bool growsOut)
        {
            var faceX = growsOut ? originX + (intoBx * depth) : originX;
            var faceZ = growsOut ? originZ + (intoBz * depth) : originZ;
            var endX = faceX + (alongX * length);
            var endZ = faceZ + (alongZ * length);
            var yaw = -Math.Atan2(alongZ, alongX);
            var bands = Bands(owner.HeightMm);
            var up = 0d;
            for (var i = 0; i < bands.Count; i++)
            {
                var heights = JointHeights(bands[i]);
                for (var hi = 0; hi < heights.Length; hi++)
                {
                    var y = owner.Anchor.Y + up + heights[hi];
                    AddVariableClamp(outElements, owner, faceX, y, faceZ, yaw, intoBx, intoBz, AdjustablePath, AdjustableCode);
                    AddVariableClamp(outElements, owner, endX, y, endZ, yaw, intoBx, intoBz, AdjustableSlidePath, AdjustableSlideCode);
                }

                up += bands[i];
            }
        }

        private static void AddVariableClamp(
            List<Atk60ElementPaintItem> outElements,
            Run owner,
            double x,
            double y,
            double z,
            double yaw,
            double normalX,
            double normalZ,
            string path,
            string code)
        {
            outElements.Add(new Atk60ElementPaintItem
            {
                IdWall = owner.Id,
                ElementType = "Union",
                ElementCode = code,
                Orientation = "Vertical",
                ImportPath = path,
                Color = "zinc",
                X = x,
                Y = y,
                Z = z,
                RotX = 0d,
                RotY = yaw,
                RotZ = 0d,
                NormalX = normalX,
                NormalZ = normalZ,
                FaceSign = owner.Anchor.FaceSign,
                IsMirrored = false,
                WallHeightMm = owner.HeightMm,
                WallThicknessMm = Atk60FaceInsertion.PanelFaceMm,
                WallLengthMm = owner.LengthMm,
                PieceWidthMm = 195d,
                PieceHeightMm = 104d,
                LocalAlongMm = 0d,
                LocalUpMm = y - owner.Anchor.Y,
                UseStrictPose = true,
            });
        }

        /// <summary>
        /// La grapa fija de la zona del tablón no une dos paneles.
        /// </summary>
        private static void StripUnionAtRemate(
            List<Atk60ElementPaintItem> items,
            double originX,
            double originZ,
            double alongX,
            double alongZ,
            double leftX,
            double leftZ,
            double length,
            double depth)
        {
            if (items == null)
            {
                return;
            }

            var margin = 40d;
            var cap = length + depth + margin;
            var cap2 = cap * cap;
            for (var i = items.Count - 1; i >= 0; i--)
            {
                var item = items[i];
                if (item == null || !string.Equals(item.ElementCode, FixedCode, StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                var dx = item.X - originX;
                var dz = item.Z - originZ;
                if ((dx * dx) + (dz * dz) > cap2)
                {
                    continue;
                }

                var along = (dx * alongX) + (dz * alongZ);
                var across = (dx * leftX) + (dz * leftZ);
                if (along < -margin || along > length + margin || across < -margin || across > depth + margin)
                {
                    continue;
                }

                items.RemoveAt(i);
            }
        }

        private static void PlaceTPanel(
            List<Atk60ElementPaintItem> outElements,
            Run owner,
            double x,
            double z,
            double inOx,
            double inOz,
            double inTx,
            double inTz,
            double yawExtra)
        {
            var yaw = PanelYaw(inOx, inOz, inTx, inTz) + yawExtra;
            var bands = Bands(owner.HeightMm);
            var up = 0d;
            for (var i = 0; i < bands.Count; i++)
            {
                var band = bands[i];
                string panelPath;
                string panelCode;
                string anglePath;
                string angleCode;
                PieceOf(band, out panelPath, out panelCode, out anglePath, out angleCode);
                var facePath = panelPath.Replace(".stl", "_F.stl");
                var tipY = band >= 2000 ? -300d : -294d;
                AddStoodPiece(
                    outElements, owner, panelPath, panelCode,
                    x, z, up, yaw, false, 0d, tipY, LegMm, band);
                AddStoodPiece(
                    outElements, owner, facePath, panelCode,
                    x, z, up, yaw, false, 0d, tipY, LegMm, band);
                up += band;
            }
        }

        /// <summary>
        /// Los rectos ya vienen recortados. La T se ve como dos tramos
        /// colineales enfrentados y el muro que llega apuntando al hueco.
        /// </summary>
        private static void PlaceTFromFacingGaps(
            List<Atk60ElementPaintItem> outElements,
            List<Run> runs,
            List<double[]> tVertices)
        {
            for (var i = 0; i < runs.Count; i++)
            {
                for (var j = i + 1; j < runs.Count; j++)
                {
                    var a = runs[i];
                    var b = runs[j];
                    if (Math.Abs((a.Ux * b.Ux) + (a.Uz * b.Uz)) < 0.85d)
                    {
                        continue;
                    }

                    EndHit endA;
                    EndHit endB;
                    double vx;
                    double vz;
                    if (!FacingGap(a, b, out endA, out endB, out vx, out vz) || NearTVertex(tVertices, vx, vz))
                    {
                        continue;
                    }

                    EndHit branch = null;
                    EndHit branchOther = null;
                    var best = 900d * 900d;
                    var bestOther = 900d * 900d;
                    for (var k = 0; k < runs.Count; k++)
                    {
                        var run = runs[k];
                        if (ReferenceEquals(run, a) || ReferenceEquals(run, b))
                        {
                            continue;
                        }

                        if (Math.Abs((run.Ux * a.Ux) + (run.Uz * a.Uz)) > 0.35d)
                        {
                            continue;
                        }

                        var ends = new[] { false, true };
                        for (var e = 0; e < ends.Length; e++)
                        {
                            double ex;
                            double ez;
                            if (!EndPoint(run, ends[e], out ex, out ez))
                            {
                                continue;
                            }

                            var dx = ex - vx;
                            var dz = ez - vz;
                            var dist2 = (dx * dx) + (dz * dz);
                            if (dist2 >= 900d * 900d)
                            {
                                continue;
                            }

                            var hit = new EndHit { Run = run, AtEnd = ends[e] };
                            if (dist2 < best)
                            {
                                bestOther = best;
                                branchOther = branch;
                                best = dist2;
                                branch = hit;
                            }
                            else if (dist2 < bestOther && !SameBranchEnd(branch, hit))
                            {
                                bestOther = dist2;
                                branchOther = hit;
                            }
                        }
                    }

                    if (branch == null)
                    {
                        continue;
                    }

                    var throughThick = (a.ThicknessMm + b.ThicknessMm) * 0.5d;
                    if (branchOther != null && OppositeEnds(branch, branchOther))
                    {
                        PlacePlus(
                            outElements, endA, endB, branch, branchOther,
                            throughThick, vx, vz, tVertices);
                        continue;
                    }

                    PlaceTPair(outElements, branch, a, throughThick, vx, vz, tVertices, endA, endB);
                }
            }
        }

        private static bool FacingGap(Run a, Run b, out EndHit endA, out EndHit endB, out double vx, out double vz)
        {
            endA = null;
            endB = null;
            vx = 0d;
            vz = 0d;
            var best = double.MaxValue;
            var flags = new[] { false, true };
            for (var ia = 0; ia < flags.Length; ia++)
            {
                for (var ib = 0; ib < flags.Length; ib++)
                {
                    double ax;
                    double az;
                    double bx;
                    double bz;
                    if (!EndPoint(a, flags[ia], out ax, out az) || !EndPoint(b, flags[ib], out bx, out bz))
                    {
                        continue;
                    }

                    var dx = bx - ax;
                    var dz = bz - az;
                    var dist2 = (dx * dx) + (dz * dz);
                    if (dist2 < 200d * 200d || dist2 > 2500d * 2500d || dist2 >= best)
                    {
                        continue;
                    }

                    var dist = Math.Sqrt(dist2);
                    double aix;
                    double aiz;
                    double bix;
                    double biz;
                    IntoFromEnd(new EndHit { Run = a, AtEnd = flags[ia] }, out aix, out aiz);
                    IntoFromEnd(new EndHit { Run = b, AtEnd = flags[ib] }, out bix, out biz);
                    if (((aix * dx) + (aiz * dz)) / dist > -0.5d)
                    {
                        continue;
                    }

                    if (((bix * -dx) + (biz * -dz)) / dist > -0.5d)
                    {
                        continue;
                    }

                    best = dist2;
                    endA = new EndHit { Run = a, AtEnd = flags[ia] };
                    endB = new EndHit { Run = b, AtEnd = flags[ib] };
                    vx = (ax + bx) * 0.5d;
                    vz = (az + bz) * 0.5d;
                }
            }

            return endA != null;
        }

        /// <summary>
        /// Cara E: espesor + 0,30 + 0,30, sin angular.
        /// Hasta 0,90, un panel y el remate al final. Por encima, dos paneles
        /// y el remate en medio. Las grapas regulables van en el remate.
        /// </summary>
        private static void PlaceCaraE(
            List<Atk60ElementPaintItem> outElements,
            Run through,
            double branchThickMm,
            double vx,
            double vz,
            double intoBx,
            double intoBz)
        {
            if (through.Anchor == null)
            {
                return;
            }

            var spanMm = (int)Math.Round(branchThickMm + 600d);
            if (spanMm < 300)
            {
                spanMm = 300;
            }

            int panelA;
            int panelB;
            int remateMm;
            bool remateMiddle;
            PlanCaraE(spanMm, out panelA, out panelB, out remateMm, out remateMiddle);
            var half = spanMm * 0.5d;
            var startX = vx - (through.Ux * half);
            var startZ = vz - (through.Uz * half);
            var along = ((startX - through.Anchor.X) * through.Ux) + ((startZ - through.Anchor.Z) * through.Uz);
            var dot = (through.Anchor.NormalX * intoBx) + (through.Anchor.NormalZ * intoBz);
            var showPrimary = dot < 0d;
            var showOpposite = !showPrimary;
            var yaw = Math.Atan2(through.Uz, through.Ux);

            PlaceCaraEPanel(outElements, through, along, panelA, yaw, showPrimary, showOpposite);
            along += panelA;
            if (panelB > 0 && remateMiddle && remateMm >= 10)
            {
                PlaceCaraERemate(outElements, through, along, remateMm, showPrimary, showOpposite);
                along += remateMm;
            }
            else if (panelB > 0 && remateMm < 10)
            {
                PlaceUnionAlong(outElements, through, along, showPrimary, showOpposite);
            }

            if (panelB > 0)
            {
                PlaceCaraEPanel(outElements, through, along, panelB, yaw, showPrimary, showOpposite);
                along += panelB;
            }

            if (!remateMiddle && remateMm >= 10)
            {
                PlaceCaraERemate(outElements, through, along, remateMm, showPrimary, showOpposite);
            }
        }

        private static void PlanCaraE(int spanMm, out int panelA, out int panelB, out int remateMm, out bool remateMiddle)
        {
            var sizes = new[] { 900, 750, 600, 450, 300 };
            panelA = 300;
            panelB = 0;
            remateMm = 0;
            remateMiddle = false;
            if (spanMm <= 900)
            {
                for (var i = 0; i < sizes.Length; i++)
                {
                    if (sizes[i] <= spanMm)
                    {
                        panelA = sizes[i];
                        break;
                    }
                }

                remateMm = spanMm - panelA;
                return;
            }

            var bestSum = 0;
            var bestSpread = int.MaxValue;
            for (var i = 0; i < sizes.Length; i++)
            {
                for (var j = i; j < sizes.Length; j++)
                {
                    var sum = sizes[i] + sizes[j];
                    if (sum > spanMm)
                    {
                        continue;
                    }

                    var spread = Math.Abs(sizes[i] - sizes[j]);
                    if (sum < bestSum || (sum == bestSum && spread >= bestSpread))
                    {
                        continue;
                    }

                    bestSum = sum;
                    bestSpread = spread;
                    panelA = Math.Max(sizes[i], sizes[j]);
                    panelB = Math.Min(sizes[i], sizes[j]);
                    remateMm = spanMm - sum;
                    remateMiddle = true;
                }
            }
        }

        private static void PlaceCaraEPanel(
            List<Atk60ElementPaintItem> outElements,
            Run through,
            double along,
            int width,
            double yaw,
            bool showPrimary,
            bool showOpposite)
        {
            var layout = Modulo270HeightPanelCatalog.ResolveForModule(through.HeightMm, width, true, true);
            if (layout == null || layout.Pieces == null)
            {
                return;
            }

            for (var pi = 0; pi < layout.Pieces.Count; pi++)
            {
                PanelFormwork.Place(
                    outElements, layout.Pieces[pi], layout, pi, through.Anchor, through.Id,
                    along, width, 1, 1,
                    yaw, through.Ux, through.Uz, through.Anchor.FaceSign,
                    through.HeightMm, through.ThicknessMm, through.LengthMm,
                    showPrimary, showOpposite);
            }

            PlaceLayoutUnions(outElements, through, along, layout, showPrimary, showOpposite);
        }

        private static void PlaceCaraERemate(
            List<Atk60ElementPaintItem> outElements,
            Run through,
            double along,
            int remateMm,
            bool showPrimary,
            bool showOpposite)
        {
            if (through.Wall == null || remateMm < 10)
            {
                return;
            }

            outElements.AddRange(EndFiller.Build(
                through.Wall, through.Anchor, remateMm, along,
                showPrimary, showOpposite));
        }

        private static void IntoFromEnd(EndHit end, out double ix, out double iz)
        {
            if (end.AtEnd)
            {
                ix = -end.Run.Ux;
                iz = -end.Run.Uz;
                return;
            }

            ix = end.Run.Ux;
            iz = end.Run.Uz;
        }

        /// <summary>
        /// En la T hay un tercer muro en el mismo vértice. Esa esquina ya
        /// la cubre el panel de la T. Una L solo tiene dos muros.
        /// </summary>
        private static bool ThirdWallNear(List<Run> runs, Run a, Run b, double x, double z)
        {
            if (runs == null)
            {
                return false;
            }

            var limit = 1100d * 1100d;
            for (var i = 0; i < runs.Count; i++)
            {
                var run = runs[i];
                if (run == null || ReferenceEquals(run, a) || ReferenceEquals(run, b))
                {
                    continue;
                }

                if (SameWall(run.Id, a.Id) || SameWall(run.Id, b.Id))
                {
                    continue;
                }

                double sx;
                double sz;
                double ex;
                double ez;
                EndPoint(run, false, out sx, out sz);
                EndPoint(run, true, out ex, out ez);
                var ds = ((sx - x) * (sx - x)) + ((sz - z) * (sz - z));
                var de = ((ex - x) * (ex - x)) + ((ez - z) * (ez - z));
                if (ds <= limit || de <= limit)
                {
                    return true;
                }
            }

            return false;
        }

        private static bool NearTVertex(List<double[]> vertices, double x, double z)
        {
            if (vertices == null)
            {
                return false;
            }

            for (var i = 0; i < vertices.Count; i++)
            {
                var dx = vertices[i][0] - x;
                var dz = vertices[i][1] - z;
                if ((dx * dx) + (dz * dz) <= 250d * 250d)
                {
                    return true;
                }
            }

            return false;
        }

        private sealed class EndHit
        {
            public Run Run;
            public bool AtEnd;
        }

        private static void TryPlace(
            List<Atk60ElementPaintItem> outElements,
            List<Run> runs,
            Run a,
            Run b,
            HashSet<string> seen)
        {
            var endsA = new[] { true, false };
            var endsB = new[] { true, false };
            for (var ia = 0; ia < endsA.Length; ia++)
            {
                for (var ib = 0; ib < endsB.Length; ib++)
                {
                    double ax;
                    double az;
                    double bx;
                    double bz;
                    if (!EndPoint(a, endsA[ia], out ax, out az) || !EndPoint(b, endsB[ib], out bx, out bz))
                    {
                        continue;
                    }

                    double vx;
                    double vz;
                    if (!EndsMeet(
                        ax, az, OutX(a, endsA[ia]), OutZ(a, endsA[ia]),
                        bx, bz, OutX(b, endsB[ib]), OutZ(b, endsB[ib]),
                        out vx, out vz))
                    {
                        continue;
                    }

                    var intoAx = -OutX(a, endsA[ia]);
                    var intoAz = -OutZ(a, endsA[ia]);
                    var intoBx = -OutX(b, endsB[ib]);
                    var intoBz = -OutZ(b, endsB[ib]);
                    if (Math.Abs((intoAx * intoBx) + (intoAz * intoBz)) >= 0.45d)
                    {
                        continue;
                    }

                    var key = VertexKey(vx, vz);
                    if (!seen.Add(key))
                    {
                        continue;
                    }

                    if (ThirdWallNear(runs, a, b, vx, vz))
                    {
                        continue;
                    }

                    var owner = string.Compare(a.Id, b.Id, StringComparison.OrdinalIgnoreCase) <= 0 ? a : b;
                    var ownerAtEnd = ReferenceEquals(owner, a) ? endsA[ia] : endsB[ib];
                    var other = ReferenceEquals(owner, a) ? b : a;
                    var otherAtEnd = ReferenceEquals(owner, a) ? endsB[ib] : endsA[ia];
                    StripTapePanelAtEnd(outElements, owner, ownerAtEnd);
                    StripTapePanelAtEnd(outElements, other, otherAtEnd);
                    PlaceCorner(outElements, owner, ownerAtEnd, other, otherAtEnd);
                    PlaceEndClamps(outElements, a, endsA[ia], b.ThicknessMm, intoBx, intoBz);
                    PlaceEndClamps(outElements, b, endsB[ib], a.ThicknessMm, intoAx, intoAz);
                    PlaceStiffenerIfStacked(outElements, owner, ownerAtEnd);
                }
            }
        }

        /// <summary>
        /// El recto ya no acaba a 450 mm del vértice. Las dos rectas se cruzan
        /// hacia fuera del extremo, como máximo a un módulo de 1,20 m.
        /// </summary>
        private static bool EndsMeet(
            double ax,
            double az,
            double aox,
            double aoz,
            double bx,
            double bz,
            double box,
            double boz,
            out double ix,
            out double iz)
        {
            ix = 0d;
            iz = 0d;
            var det = (aox * boz) - (aoz * box);
            if (Math.Abs(det) < 0.45d)
            {
                return false;
            }

            var dx = bx - ax;
            var dz = bz - az;
            var ta = ((dx * boz) - (dz * box)) / det;
            var tb = ((dx * aoz) - (dz * aox)) / det;
            if (ta < -80d || tb < -80d || ta > 1600d || tb > 1600d)
            {
                return false;
            }

            ix = ax + (aox * ta);
            iz = az + (aoz * ta);
            return true;
        }

        /// <summary>
        /// El tape del extremo de esquina deja su panel girado. Ahí va la esquina, no el tape.
        /// </summary>
        private static void StripTapePanelAtEnd(List<Atk60ElementPaintItem> items, Run run, bool atEnd)
        {
            if (items == null || run == null)
            {
                return;
            }

            double endX;
            double endZ;
            if (!EndPoint(run, atEnd, out endX, out endZ))
            {
                return;
            }

            for (var i = items.Count - 1; i >= 0; i--)
            {
                var item = items[i];
                if (item == null || !string.Equals(item.Orientation, "pillar", StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                if (!string.Equals(item.IdWall, run.Id, StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                var dx = item.X - endX;
                var dz = item.Z - endZ;
                if ((dx * dx) + (dz * dz) > 500d * 500d)
                {
                    continue;
                }

                items.RemoveAt(i);
            }
        }

        private static void PlaceCorner(
            List<Atk60ElementPaintItem> outElements,
            Run owner,
            bool ownerAtEnd,
            Run other,
            bool otherAtEnd)
        {
            var intoOx = -OutX(owner, ownerAtEnd);
            var intoOz = -OutZ(owner, ownerAtEnd);
            var intoTx = -OutX(other, otherAtEnd);
            var intoTz = -OutZ(other, otherAtEnd);
            var ownerInside = ((owner.NormalX * intoTx) + (owner.NormalZ * intoTz)) > 0d;
            var otherInside = ((other.NormalX * intoOx) + (other.NormalZ * intoOz)) > 0d;
            double angleX;
            double angleZ;
            double outX;
            double outZ;
            FaceEnd(owner, ownerAtEnd, ownerInside, out angleX, out angleZ, out outX, out outZ);
            double otherX;
            double otherZ;
            double otherOutX;
            double otherOutZ;
            FaceEnd(other, otherAtEnd, otherInside, out otherX, out otherZ, out otherOutX, out otherOutZ);

            double cornerX;
            double cornerZ;
            if (!FaceCorner(angleX, angleZ, outX, outZ, otherX, otherZ, otherOutX, otherOutZ, out cornerX, out cornerZ))
            {
                cornerX = angleX + (outX * ReachMm);
                cornerZ = angleZ + (outZ * ReachMm);
            }

            var angleYaw = Math.Atan2(-intoOz, intoOx);
            var angleDot = (Math.Sin(angleYaw) * -intoTx) + (Math.Cos(angleYaw) * -intoTz);
            if (angleDot <= 0.5d)
            {
                angleYaw = Math.Atan2(-intoTz, intoTx);
            }

            angleX = cornerX;
            angleZ = cornerZ;
            double inOx;
            double inOz;
            double inTx;
            double inTz;
            TowardBody(owner, angleX, angleZ, out inOx, out inOz);
            TowardBody(other, otherX, otherZ, out inTx, out inTz);
            // La inserción (punto verde) cae en la esquina interior, el punto rojo.
            var panelX = cornerX + (inOx * owner.ThicknessMm) + (inTx * other.ThicknessMm);
            var panelZ = cornerZ + (inOz * owner.ThicknessMm) + (inTz * other.ThicknessMm);
            var yaw = PanelYaw(inOx, inOz, inTx, inTz);
            var bands = Bands(owner.HeightMm);
            var up = 0d;
            for (var i = 0; i < bands.Count; i++)
            {
                var band = bands[i];
                string panelPath;
                string panelCode;
                string anglePath;
                string angleCode;
                PieceOf(band, out panelPath, out panelCode, out anglePath, out angleCode);
                var facePath = panelPath.Replace(".stl", "_F.stl");
                // Inserción en el extremo de la pata que marcaste, no en la esquina opuesta.
                // 2,40 y 2,70: (0, 300). El de 1,20 llega a Y 294.
                var tipX = 0d;
                var tipY = band >= 2000 ? -300d : -294d;
                AddStoodPiece(
                    outElements, owner, panelPath, panelCode,
                    panelX, panelZ, up, yaw, false, tipX, tipY, LegMm, band);
                AddStoodPiece(
                    outElements, owner, facePath, panelCode,
                    panelX, panelZ, up, yaw, false, tipX, tipY, LegMm, band);
                AddStoodPiece(
                    outElements, owner, anglePath, angleCode,
                    angleX, angleZ, up, angleYaw, false, 0d, 0d, 120d, band);
                up += band;
            }

            var ownerCover = PlaceExteriorCover(outElements, owner, ownerAtEnd, ownerInside, other.ThicknessMm);
            var otherCover = PlaceExteriorCover(outElements, other, otherAtEnd, otherInside, owner.ThicknessMm);
            var stepMm = Math.Abs(owner.ThicknessMm - other.ThicknessMm);
            var unequal = stepMm > 5d && stepMm <= 600d;
            if (!ownerCover)
            {
                PlaceGap(outElements, owner, ownerAtEnd, ownerInside, cornerX, cornerZ, !unequal, other.ThicknessMm);
            }

            if (!otherCover)
            {
                PlaceGap(outElements, other, otherAtEnd, otherInside, cornerX, cornerZ, !unequal, owner.ThicknessMm);
            }

            if (unequal && !ownerCover && !otherCover)
            {
                PlaceStepWood(
                    outElements, owner, ownerAtEnd, ownerInside,
                    other, otherAtEnd, otherInside, stepMm);
            }
            else
            {
                // El módulo de fuera ya cierra ese extremo. Medirlo otra vez
                // ponía un panel y un tablón de más, justo en la esquina.
                if (!ownerCover)
                {
                    PlaceInteriorGap(outElements, owner, ownerAtEnd, ownerInside, other.ThicknessMm, panelX, panelZ);
                }

                if (!otherCover)
                {
                    PlaceInteriorGap(outElements, other, otherAtEnd, otherInside, owner.ThicknessMm, panelX, panelZ);
                }
            }
        }

        /// <summary>
        /// Hueco de la cara interior, entre el último panel y la pata de la esquina.
        /// Si pasa de 0,15 entra un panel. El taco es el resto, como mucho 0,15, y mide 0,12 de ancho.
        /// </summary>
        private static void PlaceInteriorGap(
            List<Atk60ElementPaintItem> outElements,
            Run run,
            bool atEnd,
            bool inside,
            double otherThicknessMm,
            double cornerPanelX,
            double cornerPanelZ)
        {
            if (run == null || run.Wall == null || run.Anchor == null)
            {
                return;
            }

            double faceX;
            double faceZ;
            double outX;
            double outZ;
            FaceEnd(run, atEnd, inside, out faceX, out faceZ, out outX, out outZ);
            var toCorner = ((cornerPanelX - faceX) * outX) + ((cornerPanelZ - faceZ) * outZ);
            if (toCorner < 10d)
            {
                toCorner = ((cornerPanelX - faceX) * -outX) + ((cornerPanelZ - faceZ) * -outZ);
            }

            int moduleMm;
            int catalogRemate;
            ExteriorCoverMm(otherThicknessMm, out moduleMm, out catalogRemate);
            // El hueco es el que queda hasta la esquina. Si la cuenta sale vacía, vale el sobrante del módulo.
            var gap = toCorner;
            if (gap < 10d)
            {
                gap = catalogRemate;
            }

            if (gap < 10d || gap > 1500d)
            {
                return;
            }

            var panel = 0;
            var sizes = new[] { 900, 750, 600, 450, 300 };
            for (var i = 0; i < sizes.Length; i++)
            {
                if (sizes[i] <= gap && gap - sizes[i] <= 150d)
                {
                    panel = sizes[i];
                    break;
                }
            }

            var wood = gap - panel;
            if (wood > 150d)
            {
                wood = 150d;
            }

            var exteriorPrimary = !inside;
            var showPrimary = !exteriorPrimary;
            var showOpposite = exteriorPrimary;
            var yaw = Math.Atan2(run.Uz, run.Ux);
            var faceSign = run.Anchor.FaceSign >= 0d ? 1d : -1d;
            // El panel queda junto al muro. El taco queda junto a la esquina.
            var panelAlong = atEnd ? run.LengthMm : -panel;
            var woodAlong = atEnd ? run.LengthMm + panel : -gap;
            if (panel >= 300)
            {
                var layout = Modulo270HeightPanelCatalog.ResolveForModule(run.HeightMm, panel, true, true);
                if (layout != null && layout.Pieces != null)
                {
                    for (var pi = 0; pi < layout.Pieces.Count; pi++)
                    {
                        PanelFormwork.Place(
                            outElements, layout.Pieces[pi], layout, pi, run.Anchor, run.Id,
                            panelAlong, panel, 1, 1,
                            yaw, run.Ux, run.Uz, faceSign,
                            run.HeightMm, run.ThicknessMm, run.LengthMm,
                            showPrimary, showOpposite);
                    }
                }
            }

            if (wood > 10d)
            {
                if (panel < 300)
                {
                    woodAlong = atEnd ? run.LengthMm + (gap - wood) : -gap;
                }

                outElements.AddRange(EndFiller.Build(
                    run.Wall, run.Anchor, wood, woodAlong, showPrimary, showOpposite, true));
            }
        }

        /// <summary>
        /// Recorte del recto desde el vértice del eje, igual que el muro gris.
        /// La cara a lo largo de este muro mide el módulo del otro espesor más 300 mm.
        /// </summary>
        internal static double AxisTrimMm(
            double intoX,
            double intoZ,
            double otherX,
            double otherZ,
            double selfThicknessMm,
            double otherThicknessMm)
        {
            int moduleMm;
            int remateMm;
            ExteriorCoverMm(otherThicknessMm, out moduleMm, out remateMm);
            var cross = (intoX * otherZ) - (intoZ * otherX);
            if (Math.Abs(cross) < 1e-9)
            {
                return moduleMm;
            }

            double n1x;
            double n1z;
            double n2x;
            double n2z;
            PlanNormal(intoX, intoZ, cross > 0d ? -1 : 1, out n1x, out n1z);
            PlanNormal(otherX, otherZ, cross > 0d ? 1 : -1, out n2x, out n2z);
            var ex1 = n1x * selfThicknessMm * 0.5d;
            var ez1 = n1z * selfThicknessMm * 0.5d;
            var ex2 = n2x * otherThicknessMm * 0.5d;
            var ez2 = n2z * otherThicknessMm * 0.5d;
            var dx = ex2 - ex1;
            var dz = ez2 - ez1;
            var det = (intoX * otherZ) - (intoZ * otherX);
            if (Math.Abs(det) < 1e-9)
            {
                return moduleMm;
            }

            var alongToOuter = ((dx * otherZ) - (dz * otherX)) / det;
            return Math.Max(0d, alongToOuter + moduleMm);
        }

        private static void PlanNormal(double ux, double uz, int sign, out double nx, out double nz)
        {
            var s = sign >= 0 ? 1d : -1d;
            nx = -uz * s;
            nz = ux * s;
        }

        /// <summary>
        /// Espesor del otro muro más la esquina de 300 mm.
        /// 470 + 300 = 770, el exterior sube a 900 y el interior se queda en 130 de madera.
        /// </summary>
        internal static void ExteriorCoverMm(double otherThicknessMm, out int moduleMm, out int remateMm)
        {
            var span = (int)Math.Round(otherThicknessMm + LegMm);
            var module = 300;
            if (span > 300)
            {
                module = 450;
            }

            if (span > 450)
            {
                module = 600;
            }

            if (span > 600)
            {
                module = 750;
            }

            if (span > 750)
            {
                module = 900;
            }

            if (span > 900)
            {
                module = 1050;
            }

            if (span > 1050)
            {
                module = 1200;
            }

            moduleMm = module;
            remateMm = module - span;
            if (remateMm < 0)
            {
                remateMm = 0;
            }
        }

        /// <summary>
        /// Panel del módulo en la cara de fuera y remate de madera en la de dentro.
        /// El muro ya viene acortado hasta ese módulo.
        /// </summary>
        private static bool PlaceExteriorCover(
            List<Atk60ElementPaintItem> outElements,
            Run run,
            bool atEnd,
            bool inside,
            double otherThicknessMm)
        {
            int moduleMm;
            int remateMm;
            ExteriorCoverMm(otherThicknessMm, out moduleMm, out remateMm);
            if (moduleMm < 300 || run.Wall == null)
            {
                return false;
            }

            var yaw = Math.Atan2(run.Uz, run.Ux);
            var faceSign = run.Anchor.FaceSign >= 0d ? 1d : -1d;
            var exteriorPrimary = !inside;
            // El módulo recto solo en la cara de fuera. En la de dentro está
            // la esquina interior: un panel de muro ahí la tapa.
            var woodInside = remateMm > 10;
            var showPrimary = exteriorPrimary;
            var showOpposite = !exteriorPrimary;
            var layout = Modulo270HeightPanelCatalog.ResolveForModule(run.HeightMm, moduleMm, true, true);
            var usedMm = LayoutSpanMm(layout);
            if (usedMm < 1d)
            {
                usedMm = moduleMm;
            }

            var panelAlong = atEnd ? run.LengthMm + (moduleMm - usedMm) : -moduleMm;
            if (layout != null && layout.Pieces != null && usedMm >= 300d)
            {
                for (var pi = 0; pi < layout.Pieces.Count; pi++)
                {
                    PanelFormwork.Place(
                        outElements, layout.Pieces[pi], layout, pi, run.Anchor, run.Id,
                        panelAlong, moduleMm, 1, 1,
                        yaw, run.Ux, run.Uz, faceSign,
                        run.HeightMm, run.ThicknessMm, run.LengthMm,
                        showPrimary, showOpposite);
                }
            }

            if (woodInside)
            {
                var woodAlong = atEnd ? run.LengthMm : -(double)remateMm;
                outElements.AddRange(EndFiller.Build(
                    run.Wall, run.Anchor, remateMm, woodAlong, !exteriorPrimary, exteriorPrimary, true));
            }

            // Solo el canto del angular, en la cara donde está el panel.
            PlaceCornerSeamClamps(outElements, run, atEnd, moduleMm, showPrimary, showOpposite);
            PlaceLayoutUnions(outElements, run, panelAlong, layout, showPrimary, showOpposite);
            return true;
        }

        /// <summary>
        /// Ancho que ocupan los paneles verticales del módulo, de punta a punta.
        /// </summary>
        private static double LayoutSpanMm(Modulo270Layout layout)
        {
            var used = 0d;
            if (layout == null || layout.Pieces == null)
            {
                return used;
            }

            for (var i = 0; i < layout.Pieces.Count; i++)
            {
                var piece = layout.Pieces[i];
                if (piece == null)
                {
                    continue;
                }

                var end = piece.AlongOffsetMm + piece.PieceWidthMm;
                if (end > used)
                {
                    used = end;
                }
            }

            return used;
        }

        /// <summary>
        /// Grapa fija en el canto del angular, las dos caras.
        /// </summary>
        private static void PlaceCornerSeamClamps(
            List<Atk60ElementPaintItem> outElements,
            Run run,
            bool atEnd,
            int moduleMm,
            bool showPrimary,
            bool showOpposite)
        {
            var cornerAlong = atEnd ? run.LengthMm + moduleMm : -moduleMm;
            PlaceUnionAlong(outElements, run, cornerAlong, showPrimary, showOpposite);
        }

        /// <summary>
        /// Dos paneles juntos: una grapa de unión en esa junta, las dos caras.
        /// 2,70: 450, 1 350 y 2 250. 2,40: 400 y 2 000. 1,20: 200 y 800.
        /// </summary>
        private static void PlaceUnionAlong(
            List<Atk60ElementPaintItem> outElements,
            Run run,
            double alongMm,
            bool showPrimary = true,
            bool showOpposite = true)
        {
            var bands = Bands(run.HeightMm);
            var up = 0d;
            var clampYaw = Atk60FaceInsertion.NormalizeYawToRad(run.Anchor.RotY);
            for (var i = 0; i < bands.Count; i++)
            {
                var heights = JointHeights(bands[i]);
                for (var hi = 0; hi < heights.Length; hi++)
                {
                    Atk60FaceInsertion.BothFaces(
                        outElements, run.Anchor, run.Id, FixedPath,
                        alongMm, up + heights[hi], clampYaw, run.Ux, run.Uz, run.Anchor.FaceSign,
                        run.HeightMm, run.ThicknessMm, run.LengthMm,
                        Atk60FaceInsertion.PanelFaceMm, 0d, FixedCode, "zinc",
                        false, showPrimary, showOpposite);
                }

                up += bands[i];
            }
        }

        /// <summary>
        /// Junta vertical entre dos piezas del mismo módulo. No es el canto del angular.
        /// </summary>
        private static void PlaceLayoutUnions(
            List<Atk60ElementPaintItem> outElements,
            Run run,
            double moduleBaseAlongMm,
            Modulo270Layout layout,
            bool showPrimary = true,
            bool showOpposite = true)
        {
            if (layout == null || layout.Pieces == null)
            {
                return;
            }

            var pieces = layout.Pieces;
            var clampYaw = Atk60FaceInsertion.NormalizeYawToRad(run.Anchor.RotY);
            for (var i = 0; i < pieces.Count; i++)
            {
                var piece = pieces[i];
                if (piece == null
                    || string.Equals(piece.Orientation, "Tumbado", StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                var end = piece.AlongOffsetMm + piece.PieceWidthMm;
                var meets = false;
                for (var j = 0; j < pieces.Count; j++)
                {
                    if (j == i)
                    {
                        continue;
                    }

                    var other = pieces[j];
                    if (other == null
                        || string.Equals(other.Orientation, "Tumbado", StringComparison.OrdinalIgnoreCase))
                    {
                        continue;
                    }

                    if (other.UpOffsetMm != piece.UpOffsetMm)
                    {
                        continue;
                    }

                    if (Math.Abs(other.AlongOffsetMm - end) <= 25d)
                    {
                        meets = true;
                        break;
                    }
                }

                if (!meets)
                {
                    continue;
                }

                var heights = JointHeights(piece.PieceHeightMm);
                var world = moduleBaseAlongMm + end;
                for (var hi = 0; hi < heights.Length; hi++)
                {
                    Atk60FaceInsertion.BothFaces(
                        outElements, run.Anchor, run.Id, FixedPath,
                        world, piece.UpOffsetMm + heights[hi], clampYaw, run.Ux, run.Uz, run.Anchor.FaceSign,
                        run.HeightMm, run.ThicknessMm, run.LengthMm,
                        Atk60FaceInsertion.PanelFaceMm, 0d, FixedCode, "zinc",
                        false, showPrimary, showOpposite);
                }
            }
        }

        /// <summary>
        /// Hueco entre el último panel del muro y la esquina.
        /// Cara A y cara B pueden medir distinto: se parte en 0,90, 0,75, 0,60, 0,45 y 0,30.
        /// </summary>
        private static void PlaceGap(
            List<Atk60ElementPaintItem> outElements,
            Run run,
            bool atEnd,
            bool inside,
            double cornerX,
            double cornerZ,
            bool jointClamps,
            double otherThicknessMm)
        {
            double faceX;
            double faceZ;
            double outX;
            double outZ;
            FaceEnd(run, atEnd, inside, out faceX, out faceZ, out outX, out outZ);
            var gap = ((cornerX - faceX) * outX) + ((cornerZ - faceZ) * outZ);
            var gapMm = (int)Math.Floor(gap + 0.5d);
            if (gapMm > 2700)
            {
                return;
            }

            // 0,46 contra 0,30: el hueco es el espesor del otro muro, no un panel de 0,45.
            if (!jointClamps && otherThicknessMm > 1d && Math.Abs(gapMm - otherThicknessMm) <= 40d)
            {
                return;
            }

            var widths = PackGap(gapMm);
            if (widths.Count == 0)
            {
                return;
            }

            var packed = 0;
            for (var i = 0; i < widths.Count; i++)
            {
                packed += widths[i];
            }

            var yaw = Math.Atan2(run.Uz, run.Ux);
            var along = atEnd ? run.LengthMm + (gapMm - packed) : -gapMm;
            for (var w = 0; w < widths.Count; w++)
            {
                var width = widths[w];
                var layout = Modulo270HeightPanelCatalog.ResolveForModule(run.HeightMm, width, true, true);
                if (layout != null && layout.Pieces != null && layout.Pieces.Count > 0)
                {
                    for (var pi = 0; pi < layout.Pieces.Count; pi++)
                    {
                        PanelFormwork.Place(
                            outElements, layout.Pieces[pi], layout, pi, run.Anchor, run.Id,
                            along, width, 1, 1,
                            yaw, run.Ux, run.Uz, run.Anchor.FaceSign,
                            run.HeightMm, run.ThicknessMm, run.LengthMm);
                    }

                    PlaceLayoutUnions(outElements, run, along, layout);
                }

                along += width;
                if (w + 1 < widths.Count)
                {
                    PlaceUnionAlong(outElements, run, along);
                }
            }

            // En el escalón de espesor esa junta la cierra el tablón, no la grapa fija.
            if (!jointClamps)
            {
                return;
            }

            // Junta del angular con el panel. No es la del vano, que chocaba con el tirante.
            var jointAlong = atEnd ? run.LengthMm + gapMm : -gapMm;
            var bands = Bands(run.HeightMm);
            var up = 0d;
            var clampYaw = Atk60FaceInsertion.NormalizeYawToRad(run.Anchor.RotY);
            for (var i = 0; i < bands.Count; i++)
            {
                var heights = JointHeights(bands[i]);
                for (var hi = 0; hi < heights.Length; hi++)
                {
                    Atk60FaceInsertion.BothFaces(
                        outElements, run.Anchor, run.Id, FixedPath,
                        jointAlong, up + heights[hi], clampYaw, run.Ux, run.Uz, run.Anchor.FaceSign,
                        run.HeightMm, run.ThicknessMm, run.LengthMm,
                        Atk60FaceInsertion.PanelFaceMm, 0d, FixedCode, "zinc");
                }

                up += bands[i];
            }
        }

        private static List<int> PackGap(int gapMm)
        {
            var widths = new List<int>();
            var left = gapMm;
            var sizes = new[] { 900, 750, 600, 450, 300 };
            for (var i = 0; i < sizes.Length; i++)
            {
                while (left >= sizes[i])
                {
                    widths.Add(sizes[i]);
                    left -= sizes[i];
                }
            }

            return widths;
        }

        /// <summary>
        /// Giro de 90° en sentido horario, el que ya cuadraba.
        /// El panel queda en la cara del muro vertical.
        /// </summary>
        private static double PanelYaw(double inOx, double inOz, double inTx, double inTz)
        {
            var yaw = Math.Atan2(inOz, -inOx);
            var dot = (Math.Sin(yaw) * inTx) + (Math.Cos(yaw) * inTz);
            if (dot <= 0.5d)
            {
                yaw = Math.Atan2(inTz, -inTx);
            }

            return yaw - (Math.PI * 0.5d);
        }

        private static bool FaceCorner(
            double px,
            double pz,
            double dx,
            double dz,
            double qx,
            double qz,
            double ex,
            double ez,
            out double x,
            out double z)
        {
            x = px;
            z = pz;
            var den = (dx * ez) - (dz * ex);
            if (Math.Abs(den) < 1e-6)
            {
                return false;
            }

            var s = (((qx - px) * ez) - ((qz - pz) * ex)) / den;
            x = px + (s * dx);
            z = pz + (s * dz);
            return true;
        }

        private static void FaceEnd(
            Run run,
            bool atEnd,
            bool inside,
            out double x,
            out double z,
            out double outX,
            out double outZ)
        {
            var along = atEnd ? run.LengthMm : 0d;
            x = run.Anchor.X + (run.Ux * along);
            z = run.Anchor.Z + (run.Uz * along);
            if (inside)
            {
                x -= run.Anchor.NormalX * run.ThicknessMm;
                z -= run.Anchor.NormalZ * run.ThicknessMm;
            }

            outX = atEnd ? run.Ux : -run.Ux;
            outZ = atEnd ? run.Uz : -run.Uz;
        }

        /// <summary>
        /// Hacia el cuerpo del muro, no hacia fuera. Si no, el panel de esquina
        /// sale al exterior cuando el otro muro es más grueso.
        /// </summary>
        private static void TowardBody(Run run, double faceX, double faceZ, out double ix, out double iz)
        {
            ix = -run.Uz;
            iz = run.Ux;
            var mx = ((run.Sx + run.Ex) * 0.5d) - faceX;
            var mz = ((run.Sz + run.Ez) * 0.5d) - faceZ;
            if ((mx * ix) + (mz * iz) < 0d)
            {
                ix = -ix;
                iz = -iz;
            }
        }

        /// <summary>
        /// Escalón entre dos espesores. Tablón y grapa variable en la cara de fuera
        /// y en la de dentro. No dentro del hormigón.
        /// </summary>
        private static void PlaceStepWood(
            List<Atk60ElementPaintItem> outElements,
            Run owner,
            bool ownerAtEnd,
            bool ownerInside,
            Run other,
            bool otherAtEnd,
            bool otherInside,
            double stepMm)
        {
            StripEndRemate(outElements, owner, ownerAtEnd);
            StripEndRemate(outElements, other, otherAtEnd);

            var thick = owner.ThicknessMm >= other.ThicknessMm ? owner : other;
            var thickAtEnd = ReferenceEquals(thick, owner) ? ownerAtEnd : otherAtEnd;
            if (thick.Wall == null)
            {
                return;
            }

            var along = thickAtEnd ? thick.LengthMm : -stepMm;
            outElements.AddRange(EndFiller.Build(
                thick.Wall,
                thick.Anchor,
                stepMm,
                along,
                true,
                true));
        }

        private static void StripEndRemate(List<Atk60ElementPaintItem> items, Run run, bool atEnd)
        {
            var window = Math.Min(800d, Math.Max(200d, run.LengthMm * 0.4d));
            var endAlong = atEnd ? run.LengthMm : 0d;
            var cuts = new List<double>();
            for (var i = items.Count - 1; i >= 0; i--)
            {
                var item = items[i];
                if (item == null || !SameWall(item.IdWall, run.Id))
                {
                    continue;
                }

                var code = item.ElementCode ?? string.Empty;
                var isRemate = string.Equals(code, "REMATE_WOOD", StringComparison.OrdinalIgnoreCase)
                    || string.Equals(item.ElementType, "Remate", StringComparison.OrdinalIgnoreCase);
                if (!isRemate)
                {
                    continue;
                }

                var start = item.LocalAlongMm;
                var end = start + Math.Max(item.PieceWidthMm, item.ModuleLengthMm);
                var endX = atEnd ? run.Ex : run.Sx;
                var endZ = atEnd ? run.Ez : run.Sz;
                var dx = item.X - endX;
                var dz = item.Z - endZ;
                var nearPoint = (dx * dx) + (dz * dz) <= 900d * 900d;
                var nearAlong = Math.Abs(start - endAlong) <= window || Math.Abs(end - endAlong) <= window;
                if (!nearAlong && !nearPoint)
                {
                    continue;
                }

                cuts.Add(start);
                cuts.Add(end);
                items.RemoveAt(i);
            }

            if (cuts.Count == 0)
            {
                return;
            }

            for (var i = items.Count - 1; i >= 0; i--)
            {
                var item = items[i];
                if (item == null || !SameWall(item.IdWall, run.Id))
                {
                    continue;
                }

                var code = item.ElementCode ?? string.Empty;
                if (code != AdjustableCode && code != AdjustableSlideCode)
                {
                    continue;
                }

                for (var c = 0; c < cuts.Count; c++)
                {
                    if (Math.Abs(item.LocalAlongMm - cuts[c]) <= 40d)
                    {
                        items.RemoveAt(i);
                        break;
                    }
                }
            }
        }

        private static bool SameWall(string id, string other)
        {
            return !string.IsNullOrWhiteSpace(id)
                && string.Equals(id, other, StringComparison.OrdinalIgnoreCase);
        }

        /// <summary>
        /// Grapa fija donde el panel recto se junta con el de la esquina.
        /// Si en la cara interior hay tablón, esa junta es de grapa variable:
        /// la fija no une dos paneles y no se pone.
        /// </summary>
        private static void PlaceEndClamps(
            List<Atk60ElementPaintItem> outElements,
            Run run,
            bool atEnd,
            double otherThicknessMm,
            double intoOtherX,
            double intoOtherZ)
        {
            int moduleMm;
            int remateMm;
            ExteriorCoverMm(otherThicknessMm, out moduleMm, out remateMm);
            var inside = ((run.NormalX * intoOtherX) + (run.NormalZ * intoOtherZ)) > 0d;
            var exteriorPrimary = !inside;
            var woodInside = remateMm > 10;
            var showPrimary = woodInside ? exteriorPrimary : true;
            var showOpposite = woodInside ? !exteriorPrimary : true;
            PlaceClamps(outElements, run, atEnd, showPrimary, showOpposite);
        }

        private static void PlaceClamps(
            List<Atk60ElementPaintItem> outElements,
            Run run,
            bool atEnd,
            bool showPrimary = true,
            bool showOpposite = true)
        {
            var along = atEnd ? run.LengthMm : 0d;
            var bands = Bands(run.HeightMm);
            var up = 0d;
            var yaw = Atk60FaceInsertion.NormalizeYawToRad(run.Anchor.RotY);
            for (var i = 0; i < bands.Count; i++)
            {
                var heights = JointHeights(bands[i]);
                for (var hi = 0; hi < heights.Length; hi++)
                {
                    Atk60FaceInsertion.BothFaces(
                        outElements, run.Anchor, run.Id, FixedPath,
                        along, up + heights[hi], yaw, run.Ux, run.Uz, run.Anchor.FaceSign,
                        run.HeightMm, run.ThicknessMm, run.LengthMm,
                        Atk60FaceInsertion.PanelFaceMm, 0d, FixedCode, "zinc",
                        false, showPrimary, showOpposite);
                }

                up += bands[i];
            }
        }

        private static void PlaceStiffenerIfStacked(List<Atk60ElementPaintItem> outElements, Run run, bool atEnd)
        {
            var bands = Bands(run.HeightMm);
            if (bands.Count < 2)
            {
                return;
            }

            var along = atEnd ? run.LengthMm + (LegMm * 0.5d) : -LegMm * 0.5d;
            var up = 0d;
            var yaw = Atk60FaceInsertion.NormalizeYawToRad(run.Anchor.RotY);
            for (var i = 0; i < bands.Count - 1; i++)
            {
                up += bands[i];
                Atk60FaceInsertion.BothFaces(
                    outElements, run.Anchor, run.Id, StiffPath,
                    along, up, yaw, run.Ux, run.Uz, run.Anchor.FaceSign,
                    run.HeightMm, run.ThicknessMm, run.LengthMm,
                    Atk60FaceInsertion.PanelFaceMm, 0d, StiffCode, "blue");
                Atk60FaceInsertion.BothFaces(
                    outElements, run.Anchor, run.Id, HookPath,
                    along, up - 200d, yaw, run.Ux, run.Uz, run.Anchor.FaceSign,
                    run.HeightMm, run.ThicknessMm, run.LengthMm,
                    Atk60FaceInsertion.PanelFaceMm, 0d, HookCode, "blue");
                Atk60FaceInsertion.BothFaces(
                    outElements, run.Anchor, run.Id, HookPath,
                    along, up + 200d, yaw, run.Ux, run.Uz, run.Anchor.FaceSign,
                    run.HeightMm, run.ThicknessMm, run.LengthMm,
                    Atk60FaceInsertion.PanelFaceMm, 0d, HookCode, "blue");
            }
        }

        /// <summary>
        /// El STL trae la altura en Z y las patas en X/Y, ya en milímetros.
        /// RotX = -90° lo pone de pie. El giro de planta va en RotZ: con RotY se tumba.
        /// En el panel, el desplazamiento en planta son los dos espesores.
        /// </summary>
        private static void AddStoodPiece(
            List<Atk60ElementPaintItem> outElements,
            Run run,
            string path,
            string code,
            double x,
            double z,
            double upMm,
            double yaw,
            bool mirror,
            double insertOffsetX,
            double insertOffsetY,
            double legMm,
            int heightMm)
        {
            outElements.Add(new Atk60ElementPaintItem
            {
                IdWall = run.Id,
                ElementType = "Panel",
                ElementCode = code,
                Orientation = mirror ? "LCornerMirror" : "LCorner",
                ImportPath = path,
                Color = "frame-corner",
                X = x,
                Y = run.Anchor.Y + upMm,
                Z = z,
                RotX = -Math.PI * 0.5d,
                RotY = 0d,
                RotZ = yaw,
                NormalX = run.NormalX,
                NormalZ = run.NormalZ,
                FaceSign = run.Anchor.FaceSign,
                IsMirrored = mirror,
                ModuleLengthMm = legMm,
                ModuleIndex = 1,
                ModuleCountInWall = 1,
                WallHeightMm = run.HeightMm,
                WallThicknessMm = run.ThicknessMm,
                WallLengthMm = run.LengthMm,
                PieceWidthMm = legMm,
                PieceHeightMm = heightMm,
                LocalAlongMm = 0d,
                LocalUpMm = upMm,
                InsertOffsetX = insertOffsetX,
                InsertOffsetY = insertOffsetY,
                UseStrictPose = true,
                CatalogHeightMm = heightMm,
            });
        }

        private static void PieceOf(int band, out string panelPath, out string panelCode, out string anglePath, out string angleCode)
        {
            if (band >= 2500)
            {
                panelPath = "../../Content/DesignTools/Stl/ATK60/E27004210.stl";
                panelCode = "E27004210";
                anglePath = "../../Content/DesignTools/Stl/ATK60/EEx27000000.stl";
                angleCode = "27000000";
                return;
            }

            if (band >= 2000)
            {
                panelPath = "../../Content/DesignTools/Stl/ATK60/E24004217.stl";
                panelCode = "E24004217";
                anglePath = "../../Content/DesignTools/Stl/ATK60/EEx24000000.stl";
                angleCode = "24000000";
                return;
            }

            panelPath = "../../Content/DesignTools/Stl/ATK60/E12004216.stl";
            panelCode = "E12004216";
            anglePath = "../../Content/DesignTools/Stl/ATK60/EEx12000000.stl";
            angleCode = "12000000";
        }

        /// <summary>
        /// Junta normal. La de al lado de una esquina de 90° lleva una grapa más;
        /// el manual no da su altura, así que aquí no se añade.
        /// </summary>
        private static double[] JointHeights(int band)
        {
            if (band >= 2500)
            {
                return new[] { 450d, 1350d, 2250d };
            }

            if (band >= 2000)
            {
                return new[] { 400d, 2000d };
            }

            return new[] { 200d, 800d };
        }

        private static List<int> Bands(double heightMm)
        {
            var bands = new List<int>();
            var left = (int)Math.Round(heightMm);
            if (left < 400)
            {
                left = 2700;
            }

            while (left >= 2500)
            {
                bands.Add(2700);
                left -= 2700;
            }

            if (left > 1800)
            {
                bands.Add(2400);
            }
            else if (left > 400)
            {
                bands.Add(1200);
            }

            if (bands.Count == 0)
            {
                bands.Add(2700);
            }

            return bands;
        }

        private static Run RunOf(Atk60WallPaintAnchor anchor, Desing2FormworkWallDto wall)
        {
            var debug = anchor.Debug;
            var dx = debug.EndX - debug.StartX;
            var dz = debug.EndZ - debug.StartZ;
            var len = Math.Sqrt((dx * dx) + (dz * dz));
            if (len < 100d)
            {
                return null;
            }

            var height = wall != null && wall.Attributes != null && wall.Attributes._DataHeight.HasValue
                ? wall.Attributes._DataHeight.Value * 1000d
                : 2700d;
            if (height < 400d)
            {
                height = 2700d;
            }

            var thickness = debug.WidthMm > 1d ? debug.WidthMm : 300d;
            return new Run
            {
                Anchor = anchor,
                Wall = wall,
                Id = anchor.IdWall,
                Sx = debug.StartX,
                Sz = debug.StartZ,
                Ex = debug.EndX,
                Ez = debug.EndZ,
                Ux = dx / len,
                Uz = dz / len,
                LengthMm = len,
                HeightMm = height,
                ThicknessMm = thickness,
                NormalX = anchor.NormalX,
                NormalZ = anchor.NormalZ,
            };
        }

        private static Desing2FormworkWallDto WallOf(List<Desing2FormworkWallDto> walls, string id)
        {
            if (walls == null || string.IsNullOrWhiteSpace(id))
            {
                return null;
            }

            for (var i = 0; i < walls.Count; i++)
            {
                var wall = walls[i];
                if (wall == null)
                {
                    continue;
                }

                var key = !string.IsNullOrWhiteSpace(wall.WallId)
                    ? wall.WallId
                    : (!string.IsNullOrWhiteSpace(wall.Id) ? wall.Id : wall.LineId);
                if (string.Equals(key, id, StringComparison.OrdinalIgnoreCase))
                {
                    return wall;
                }
            }

            return null;
        }

        private static bool EndPoint(Run run, bool atEnd, out double x, out double z)
        {
            x = atEnd ? run.Ex : run.Sx;
            z = atEnd ? run.Ez : run.Sz;
            return true;
        }

        private static double OutX(Run run, bool atEnd)
        {
            return atEnd ? run.Ux : -run.Ux;
        }

        private static double OutZ(Run run, bool atEnd)
        {
            return atEnd ? run.Uz : -run.Uz;
        }

        private static string VertexKey(double x, double z)
        {
            return ((int)Math.Round(x / 20d)).ToString() + "|" + ((int)Math.Round(z / 20d)).ToString();
        }

        private sealed class Run
        {
            public Atk60WallPaintAnchor Anchor;
            public Desing2FormworkWallDto Wall;
            public string Id;
            public double Sx;
            public double Sz;
            public double Ex;
            public double Ez;
            public double Ux;
            public double Uz;
            public double LengthMm;
            public double HeightMm;
            public double ThicknessMm;
            public double NormalX;
            public double NormalZ;
        }
    }
}
