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
        private const string AdjustableCode = "10000221";
        private const string AdjustableSlideCode = "10000221B";

        internal static void Place(
            List<Atk60ElementPaintItem> outElements,
            List<Desing2FormworkWallDto> walls,
            List<Atk60WallPaintAnchor> anchors)
        {
            if (outElements == null || anchors == null || anchors.Count < 2)
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
            for (var a = 0; a < runs.Count; a++)
            {
                for (var b = a + 1; b < runs.Count; b++)
                {
                    TryPlace(outElements, runs[a], runs[b], seen);
                }
            }
        }

        private static void TryPlace(List<Atk60ElementPaintItem> outElements, Run a, Run b, HashSet<string> seen)
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
            // Cara exterior: el módulo llega hasta el angular. El tablón no va ahí.
            // Cara interior: el sobrante es el tablón, entre la esquina interior y el panel,
            // con grapa variable. En ese hueco no hay grapa fija.
            var woodInside = remateMm > 10;
            var showPrimary = woodInside ? exteriorPrimary : true;
            var showOpposite = woodInside ? !exteriorPrimary : true;
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
