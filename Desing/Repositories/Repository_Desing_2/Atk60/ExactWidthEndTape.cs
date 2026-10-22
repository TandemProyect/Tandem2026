using Desing.Repositories.RepositoryAtk60;
using Desing.Repositories.RepositoryAtk60.ModulosATK60;
using Desing.Repositories.RepositoryCommun;
using System;
using System.Collections.Generic;

namespace Desing.Repositories.Repository_Desing_2.Atk60
{
    /// <summary>
    /// Cierre de los dos extremos.
    /// 0,30, 0,45, 0,60 y 0,90: panel (TapeS4 / TapeS5) o tape de solape.
    /// Menos de 0,30: relleno naranja entre dos tablones laterales y placa (TapeS3).
    /// TapeS1: 0,30 m más de encofrado, naranja de 22 mm, dos tablones de 75 mm y barras.
    /// TapeS2: el mismo naranja y, en el sitio de los tablones, fenólico de 22 mm.
    /// Sin rigidizador. Los tirantes son los del fin del panel suplementario.
    /// El final es el simétrico del inicio. Un extremo unido a otro muro no se cierra.
    /// </summary>
    internal static class ExactWidthEndTape
    {
        private const double BoardDepthMm = 120d;
        private const string ClampPath = "../../Content/DesignTools/Stl/ATK60/10004220.stl";
        private const string ClampCode = "10004220";
        private const string VariableClampPath = "../../Content/DesignTools/Stl/ATK60/10000221.stl";
        private const string VariableClampCode = "10000221";
        private const string VariableClampSlidePath = "../../Content/DesignTools/Stl/ATK60/10000221-2.stl";
        private const string VariableClampSlideCode = "10000221B";
        private const double VariableClampRaiseMm = 200d;

        private const string PlatePath = "../../Content/DesignTools/Stl/ATK60/10443020.stl";
        private const string PlateCode = "10443020";
        private const string StiffenerPath = "../../Content/DesignTools/Stl/ATK60/1850162.stl";
        private const string StiffenerCode = "1850162";
        private const string HookCode = "1920811";
        private const double TapeS1SupplementMm = 300d;
        private const double TapeS1OrangeMm = 22d;
        private const double TapeS1SideMm = 75d;
        private const double TapeS1StiffenerDepthMm = 60d;
        private const double TapeS1HookIntoProfileMm = 50d;

        /// <summary>
        /// Menos de 0,30 m (TapeS3): igual que FilerSEMeS01.
        /// Naranja de 22 mm entre las caras, 120 mm por fuera de cada una,
        /// y un tablón de 120 × 75 mm en cada lado. La placa va en esa cara exterior.
        /// </summary>
        private static void AddUnder300(
            List<Atk60ElementPaintItem> outElements,
            Desing2FormworkWallDto wall,
            Atk60WallPaintAnchor anchor,
            AttributesList attrs,
            string idWall,
            bool atStart,
            double alongMm,
            double ux,
            double uz,
            int widthMm,
            double wallHeightMm,
            double wallLengthMm,
            double coverMm,
            bool withYellowModule,
            double solapeEachEndMm)
        {
            double originX;
            double originZ;
            double rotY;
            EndPose(anchor, atStart, alongMm, ux, uz, widthMm, out originX, out originZ, out rotY);

            var tape = ResolveTape(attrs, atStart);
            if (tape == "TapeS2")
            {
                AddTapeS2(
                    outElements, wall, anchor, idWall,
                    atStart, alongMm, ux, uz, widthMm,
                    wallHeightMm, wallLengthMm, originX, originZ, rotY, coverMm, withYellowModule, solapeEachEndMm);
                return;
            }

            if (tape == "TapeS1")
            {
                AddTapeS1(
                    outElements, wall, anchor, idWall,
                    atStart, alongMm, ux, uz, widthMm,
                    wallHeightMm, wallLengthMm, originX, originZ, rotY, coverMm, withYellowModule, solapeEachEndMm);
                return;
            }

            // Pegados al extremo del muro. El Desing antiguo sumaba 22 y 73 mm
            // porque su coordenada no era el canto del panel.
            outElements.Add(SideWood(anchor, idWall, originX, originZ, rotY, widthMm + 240d, -120d, 22d, 0d, "wood-orange", wallHeightMm, wallLengthMm, alongMm));
            outElements.Add(SideWood(anchor, idWall, originX, originZ, rotY, 120d, -120d, 75d, 0d, "wood-side", wallHeightMm, wallLengthMm, alongMm));
            outElements.Add(SideWood(anchor, idWall, originX, originZ, rotY, 120d, widthMm, 75d, 0d, "wood-side", wallHeightMm, wallLengthMm, alongMm));

            var bands = HeightBands(wallHeightMm);
            for (var i = 0; i < bands.Count; i++)
            {
                var heights = Under300HeightsMm(bands[i].HeightMm);
                for (var hi = 0; hi < heights.Length; hi++)
                {
                    var upMm = bands[i].UpMm + heights[hi];
                    AddSidePlate(outElements, anchor, idWall, alongMm, upMm, ux, uz, widthMm, wallHeightMm, wallLengthMm, true, atStart);
                    AddSidePlate(outElements, anchor, idWall, alongMm, upMm, ux, uz, widthMm, wallHeightMm, wallLengthMm, false, atStart);
                }
            }
        }

        /// <summary>
        /// TapeS2, como FilerTapeS2. Mismo suplemento y mismo fenólico de canto que el TapeS1.
        /// Donde el TapeS1 lleva tablón de 75 mm, aquí hay fenólico naranja de 22 mm.
        /// No hay rigidizador ni gancho. Los tirantes son los del fin de ese panel.
        /// </summary>
        private static void AddTapeS2(
            List<Atk60ElementPaintItem> outElements,
            Desing2FormworkWallDto wall,
            Atk60WallPaintAnchor anchor,
            string idWall,
            bool atStart,
            double alongMm,
            double ux,
            double uz,
            int widthMm,
            double wallHeightMm,
            double wallLengthMm,
            double originX,
            double originZ,
            double rotY,
            double coverMm,
            bool withYellowModule,
            double solapeEachEndMm)
        {
            var moduleStartMm = SupplementStartMm(atStart, alongMm, coverMm);
            var outerRunMm = solapeEachEndMm > 1d
                ? solapeEachEndMm
                : TapeS1SupplementMm - Math.Max(0d, coverMm);
            if (withYellowModule)
            {
                outElements.AddRange(Atk60ModuleAssembly.Build(
                    wall, anchor, 1, TapeS1SupplementMm, moduleStartMm, 0, 1, false, true));
                PlaceSupplementJointClamps(
                    outElements, wall, anchor, idWall, SupplementJointMm(atStart, moduleStartMm), ux, uz, widthMm, wallHeightMm, wallLengthMm);
            }

            outElements.Add(SideWood(
                anchor, idWall, originX, originZ, rotY,
                widthMm, 0d, TapeS1OrangeMm, 0d,
                "wood-orange", wallHeightMm, wallLengthMm, alongMm));
            var runMm = outerRunMm - TapeS1OrangeMm;
            outElements.Add(SideWood(
                anchor, idWall, originX, originZ, rotY,
                TapeS1OrangeMm, 0d, runMm, TapeS1OrangeMm,
                "wood-orange", wallHeightMm, wallLengthMm, alongMm));
            outElements.Add(SideWood(
                anchor, idWall, originX, originZ, rotY,
                TapeS1OrangeMm, widthMm - TapeS1OrangeMm, runMm, TapeS1OrangeMm,
                "wood-orange", wallHeightMm, wallLengthMm, alongMm));
        }

        /// <summary>
        /// TapeS1, como FilerSExS01. El encofrado sigue 0,30 m más allá del hormigón.
        /// El fenólico naranja de 22 mm va pegado al canto del muro. Cada tablón de 75 mm
        /// sale de esa cara exterior, en el canto del fenólico. El fijador 1850164 no se pinta.
        /// </summary>
        private static void AddTapeS1(
            List<Atk60ElementPaintItem> outElements,
            Desing2FormworkWallDto wall,
            Atk60WallPaintAnchor anchor,
            string idWall,
            bool atStart,
            double alongMm,
            double ux,
            double uz,
            int widthMm,
            double wallHeightMm,
            double wallLengthMm,
            double originX,
            double originZ,
            double rotY,
            double coverMm,
            bool withYellowModule,
            double solapeEachEndMm)
        {
            var outSign = atStart ? -1d : 1d;
            var moduleStartMm = SupplementStartMm(atStart, alongMm, coverMm);
            var outerRunMm = solapeEachEndMm > 1d
                ? solapeEachEndMm
                : TapeS1SupplementMm - Math.Max(0d, coverMm);
            if (withYellowModule)
            {
                // El TapeS1 no lleva el tirante del canto: lo cierran las placas.
                outElements.AddRange(Atk60ModuleAssembly.Build(
                    wall, anchor, 1, TapeS1SupplementMm, moduleStartMm, 0, 1, false, !atStart,
                    false, false, 0d, -1d, false, true));
                PlaceSupplementJointClamps(
                    outElements, wall, anchor, idWall, SupplementJointMm(atStart, moduleStartMm), ux, uz, widthMm, wallHeightMm, wallLengthMm);
            }

            outElements.Add(SideWood(
                anchor, idWall, originX, originZ, rotY,
                widthMm, 0d, TapeS1OrangeMm, 0d,
                "wood-orange", wallHeightMm, wallLengthMm, alongMm));
            var plankRunMm = outerRunMm - TapeS1OrangeMm;
            outElements.Add(SideWood(
                anchor, idWall, originX, originZ, rotY,
                TapeS1SideMm, 0d, plankRunMm, TapeS1OrangeMm,
                "wood-side", wallHeightMm, wallLengthMm, alongMm));
            outElements.Add(SideWood(
                anchor, idWall, originX, originZ, rotY,
                TapeS1SideMm, widthMm - TapeS1SideMm, plankRunMm, TapeS1OrangeMm,
                "wood-side", wallHeightMm, wallLengthMm, alongMm));

            var stiffenerAlongMm = alongMm + (outSign * (outerRunMm + (TapeS1StiffenerDepthMm * 0.5d)));
            var tieAlongMm = stiffenerAlongMm + (outSign * (TapeS1StiffenerDepthMm * 0.5d));
            var outX = ux * outSign;
            var outZ = uz * outSign;
            var nx = anchor.NormalX;
            var nz = anchor.NormalZ;
            var nLen = Math.Sqrt((nx * nx) + (nz * nz));
            if (nLen > 1e-6)
            {
                nx /= nLen;
                nz /= nLen;
            }

            var yaw = Math.Atan2(outZ, outX);
            var faceGapMm = Atk60FaceInsertion.PanelFaceMm - 60d;
            var hookAlongMm = tieAlongMm - (outSign * TapeS1HookIntoProfileMm);
            var bands = HeightBands(wallHeightMm);
            for (var i = 0; i < bands.Count; i++)
            {
                var heights = HookHeightsMm(bands[i].HeightMm);
                for (var hi = 0; hi < heights.Length; hi++)
                {
                    var upMm = bands[i].UpMm + heights[hi];
                    AddTapeS1Stiffener(
                        outElements, anchor, idWall,
                        stiffenerAlongMm, upMm, ux, uz, widthMm, wallHeightMm, wallLengthMm);
                    // Inicio: RotZ 3π/2. Final: π/2, los mismos 180°.
                    // Por encima de una vuelta el visor lee el ángulo como grados.
                    var plateRotZ = atStart ? Math.PI : 0d;
                    AddSidePlate(
                        outElements, anchor, idWall, alongMm, upMm, ux, uz,
                        widthMm, wallHeightMm, wallLengthMm, true, atStart, tieAlongMm, 0d, true, true, plateRotZ);
                    AddSidePlate(
                        outElements, anchor, idWall, alongMm, upMm, ux, uz,
                        widthMm, wallHeightMm, wallLengthMm, false, atStart, tieAlongMm, 0d, false, true, plateRotZ);
                    var plateY = anchor.Y + upMm;
                    AddTapeS1Hook(
                        outElements, anchor, idWall, yaw,
                        anchor.X + (ux * hookAlongMm) + (nx * faceGapMm),
                        plateY,
                        anchor.Z + (uz * hookAlongMm) + (nz * faceGapMm));
                    AddTapeS1Hook(
                        outElements, anchor, idWall, yaw,
                        anchor.X + (ux * hookAlongMm) - (nx * (widthMm + faceGapMm)),
                        plateY,
                        anchor.Z + (uz * hookAlongMm) - (nz * (widthMm + faceGapMm)));
                }
            }
        }

        internal static void PlaceSharedJoint(
            List<Atk60ElementPaintItem> outElements,
            Desing2FormworkWallDto wall,
            Atk60WallPaintAnchor anchor,
            double alongMm)
        {
            if (wall == null || anchor == null)
            {
                return;
            }

            var attrs = wall.Attributes;
            var yawRad = Atk60FaceInsertion.NormalizeYawToRad(anchor.RotY);
            var ux = Math.Cos(yawRad);
            var uz = Math.Sin(yawRad);
            var wallHeightMm = Atk60FaceInsertion.ResolveWallHeightMm(attrs);
            var wallLengthMm = WallRunMm(anchor, attrs);
            var widthMm = (int)Math.Round(Atk60FaceInsertion.ResolveWallThicknessMm(attrs));
            var idWall = !string.IsNullOrWhiteSpace(wall.WallId)
                ? wall.WallId
                : (!string.IsNullOrWhiteSpace(wall.Id) ? wall.Id : wall.LineId);
            PlaceSupplementJointClamps(
                outElements, wall, anchor, idWall, alongMm, ux, uz, widthMm, wallHeightMm, wallLengthMm, 600d);
        }

        /// <summary>
        /// Grapa fija en la junta del panel de 0,30 m. Ese canto queda fuera del muro.
        /// </summary>
        private static void PlaceSupplementJointClamps(
            List<Atk60ElementPaintItem> outElements,
            Desing2FormworkWallDto wall,
            Atk60WallPaintAnchor anchor,
            string idWall,
            double alongMm,
            double ux,
            double uz,
            int widthMm,
            double wallHeightMm,
            double wallLengthMm,
            double moduleLengthMm = TapeS1SupplementMm)
        {
            var attrs = wall != null ? wall.Attributes : null;
            var yawRad = Atk60FaceInsertion.NormalizeYawToRad(anchor.RotY);
            var faceSign = anchor.FaceSign >= 0d ? 1d : -1d;
            var supplementLayout = Modulo270HeightPanelCatalog.ResolveForModule(
                wallHeightMm,
                (int)moduleLengthMm,
                Atk60FaceInsertion.UseAdjustable750(attrs),
                Atk60FaceInsertion.FlagOn(attrs != null ? attrs._CHeckTypeFormworkMode : null));
            PanelClamp.PlaceBeyondWall(
                outElements, anchor, idWall, alongMm, supplementLayout,
                yawRad, ux, uz, faceSign, wallHeightMm, widthMm, wallLengthMm);
        }

        /// <summary>
        /// El tornillo del 1920811 pasa por el centro de la placa.
        /// </summary>
        private static void AddTapeS1Hook(
            List<Atk60ElementPaintItem> outElements,
            Atk60WallPaintAnchor anchor,
            string idWall,
            double yaw,
            double plateX,
            double plateY,
            double plateZ)
        {
            const double rodY = 0.6d;
            const double rodZ = 83.6d;
            var sin = Math.Sin(yaw);
            var cos = Math.Cos(yaw);
            AddPillarPiece(
                outElements, anchor, idWall, HookCode, "hook",
                plateX - (rodZ * sin),
                plateY - rodY,
                plateZ - (rodZ * cos),
                yaw, Math.PI, 0, 0d, false);
        }

        /// <summary>
        /// Un solo 1850162. El centro del STL (750 mm) cae en el eje del muro.
        /// </summary>
        private static void AddTapeS1Stiffener(
            List<Atk60ElementPaintItem> outElements,
            Atk60WallPaintAnchor anchor,
            string idWall,
            double barAlongMm,
            double upMm,
            double ux,
            double uz,
            int widthMm,
            double wallHeightMm,
            double wallLengthMm)
        {
            const double centerXmm = -1.6d;
            const double centerYmm = -50.3d;
            const double centerZmm = 19.8d;
            var baseX = -centerZmm;
            var baseY = centerXmm;
            var baseZ = -centerYmm;
            var wallYaw = Math.Atan2(-anchor.NormalX, anchor.NormalZ);
            var alpha = (Math.PI * 0.5d) - wallYaw;
            var cos = Math.Cos(alpha);
            var sin = Math.Sin(alpha);
            var offsetX = (baseX * cos) + (baseZ * sin);
            var offsetY = baseY;
            var offsetZ = (-baseX * sin) + (baseZ * cos);
            var mid = widthMm * 0.5d;
            var faceSign = anchor.FaceSign >= 0d ? 1d : -1d;
            Atk60FaceInsertion.OneFace(
                outElements, anchor, idWall, StiffenerPath,
                anchor.X + (ux * barAlongMm) - (anchor.NormalX * mid) - offsetX,
                anchor.Y + upMm - offsetY,
                anchor.Z + (uz * barAlongMm) - (anchor.NormalZ * mid) - offsetZ,
                0d,
                Math.PI * 0.5d,
                anchor.NormalX,
                anchor.NormalZ,
                faceSign,
                false,
                barAlongMm,
                upMm,
                wallHeightMm,
                widthMm,
                wallLengthMm,
                StiffenerCode,
                "blue");
        }

        /// <summary>
        /// Placa en el medio del ancho del tablón.
        /// Hacia fuera del canto: 37,5 mm y, además, media anchura (60 mm).
        /// La rotación no se toca.
        /// </summary>
        private static void AddSidePlate(
            List<Atk60ElementPaintItem> outElements,
            Atk60WallPaintAnchor anchor,
            string idWall,
            double alongMm,
            double upMm,
            double ux,
            double uz,
            int widthMm,
            double wallHeightMm,
            double wallLengthMm,
            bool primaryFace,
            bool atStart,
            double? plateAlongOverrideMm = null,
            double faceOutExtraMm = 0d,
            bool extraHalfTurn = false,
            bool lockMateYaw = false,
            double extraRotZ = 0d)
        {
            const double tablonDepthMm = 75d;
            const double tablonWidthMm = 120d;
            var outSign = atStart ? -1d : 1d;
            var plateAlong = plateAlongOverrideMm
                ?? (alongMm + (outSign * ((tablonDepthMm * 0.5d) + (tablonWidthMm * 0.5d))));
            var faceGapMm = Atk60FaceInsertion.PanelFaceMm - (tablonWidthMm * 0.5d) + faceOutExtraMm;
            var nx = anchor.NormalX;
            var nz = anchor.NormalZ;
            var faceNx = primaryFace ? nx : -nx;
            var faceNz = primaryFace ? nz : -nz;
            var x = anchor.X + (ux * plateAlong) + (faceNx * faceGapMm);
            var z = anchor.Z + (uz * plateAlong) + (faceNz * faceGapMm);
            if (!primaryFace)
            {
                x -= nx * widthMm;
                z -= nz * widthMm;
            }

            var plateRotY = Math.Atan2(nx, nz) + Math.PI;
            if (!lockMateYaw)
            {
                plateRotY = Math.Atan2(faceNx, faceNz) + Math.PI;
                var rightX = -uz;
                var rightZ = ux;
                if (!atStart && (faceNx * rightX) + (faceNz * rightZ) > 0d)
                {
                    plateRotY += Math.PI;
                }
            }
            else if (!primaryFace)
            {
                plateRotY += Math.PI;
            }

            if (extraHalfTurn)
            {
                plateRotY += Math.PI;
            }

            outElements.Add(new Atk60ElementPaintItem
            {
                IdWall = !string.IsNullOrWhiteSpace(idWall) ? idWall : anchor.IdWall,
                ElementType = "Panel",
                ElementCode = PlateCode,
                Orientation = "pillar",
                ImportPath = PlatePath,
                Color = "zinc",
                X = x,
                Y = anchor.Y + upMm,
                Z = z,
                RotX = Math.PI * 0.5,
                RotY = plateRotY,
                RotZ = (Math.PI * 0.5) + extraRotZ,
                NormalX = faceNx,
                NormalZ = faceNz,
                FaceSign = anchor.FaceSign,
                UseStrictPose = true,
                PieceWidthMm = 117d,
                PieceHeightMm = 117d,
                WallHeightMm = wallHeightMm,
                WallThicknessMm = widthMm,
                WallLengthMm = wallLengthMm,
                LocalAlongMm = plateAlong,
                LocalUpMm = upMm,
            });
        }

        private static double[] Under300HeightsMm(int bandHeightMm)
        {
            if (bandHeightMm <= 1200) return new[] { 250d, 1150d };
            if (bandHeightMm <= 2400) return new[] { 550d, 1400d, 2250d };
            return new[] { 650d, 1400d, 2200d };
        }

        private static void CoverMm(
            Desing2FormworkWallDto wall,
            double absorbedRemateMm,
            out double startMm,
            out double endMm)
        {
            startMm = 0d;
            endMm = 0d;
            if (absorbedRemateMm <= 1d || absorbedRemateMm >= 150d)
            {
                return;
            }

            if (ReplacesRemate(wall, false))
            {
                endMm = absorbedRemateMm;
                return;
            }

            if (ReplacesRemate(wall, true))
            {
                startMm = absorbedRemateMm;
            }
        }

        private static double SupplementStartMm(bool atStart, double alongMm, double coverMm)
        {
            var cover = Math.Max(0d, coverMm);
            if (atStart)
            {
                return alongMm - (TapeS1SupplementMm - cover);
            }

            return alongMm - cover;
        }

        private static double SupplementJointMm(bool atStart, double moduleStartMm)
        {
            if (atStart)
            {
                return moduleStartMm + TapeS1SupplementMm;
            }

            return moduleStartMm;
        }

        /// <summary>
        /// El tirante del canto del panel solo va en el TapeS2.
        /// En el resto el extremo lo cierran el angular, la unión o las grapas.
        /// </summary>
        internal static bool SkipsPanelEndTie(Desing2FormworkWallDto wall, bool atStart)
        {
            var attrs = wall != null ? wall.Attributes : null;
            if (!EndIsFree(attrs, atStart))
            {
                return false;
            }

            if (IsMiddleWood(attrs, atStart) || IsWideEndWood(attrs, atStart))
            {
                return true;
            }

            var tape = ResolveTape(attrs, atStart);
            if (tape == "TapeS7" || tape == "TapeS8")
            {
                return true;
            }

            var mm = (int)Math.Round(Atk60FaceInsertion.ResolveWallThicknessMm(attrs));
            var closes = (mm > 20 && mm < 300)
                || mm == 300 || mm == 450 || mm == 600 || mm == 900;
            if (!closes)
            {
                return false;
            }

            return ResolveTape(attrs, atStart) != "TapeS2";
        }

        /// <summary>
        /// El suplemento ya trae su tirante. El del panel anterior, pegado a ese, no se pinta.
        /// TapeS2 en los dos cantos. TapeS1 solo en el final: en el inicio ya queda uno.
        /// </summary>
        internal static bool EndUsesPanelTies(Desing2FormworkWallDto wall, bool atStart)
        {
            var attrs = wall != null ? wall.Attributes : null;
            if (!EndIsFree(attrs, atStart))
            {
                return false;
            }

            var tape = ResolveTape(attrs, atStart);
            if (tape == "TapeS2")
            {
                return true;
            }

            return tape == "TapeS1" && !atStart;
        }

        /// <summary>
        /// El solape (TapeS1 o TapeS2) sustituye al remate de madera.
        /// El cierre de panel y taco no lo sustituye.
        /// </summary>
        internal static bool ReplacesRemate(Desing2FormworkWallDto wall, bool atStart)
        {
            var attrs = wall != null ? wall.Attributes : null;
            if (!EndIsFree(attrs, atStart))
            {
                return false;
            }

            if (IsPanelAndWood(attrs, atStart) || IsMiddleWood(attrs, atStart) || IsWideEndWood(attrs, atStart))
            {
                return false;
            }

            var tape = ResolveTape(attrs, atStart);
            return tape == "TapeS1" || tape == "TapeS2";
        }

        /// <summary>
        /// El remate sigue en el muro. Solo desaparece si un extremo libre, o los dos, lleva solape.
        /// </summary>
        internal static bool SuppressesRemate(Desing2FormworkWallDto wall)
        {
            var attrs = wall != null ? wall.Attributes : null;
            if (IsPanelAndWood(attrs, true) || IsPanelAndWood(attrs, false)
                || IsMiddleWood(attrs, true) || IsMiddleWood(attrs, false)
                || IsWideEndWood(attrs, true) || IsWideEndWood(attrs, false))
            {
                return false;
            }

            return ReplacesRemate(wall, true) || ReplacesRemate(wall, false);
        }

        internal static double RunMm(Atk60WallPaintAnchor anchor, Desing2FormworkWallDto wall)
        {
            return WallRunMm(anchor, wall != null ? wall.Attributes : null);
        }

        internal static List<Atk60ElementPaintItem> Place(
            Desing2FormworkWallDto wall,
            Atk60WallPaintAnchor anchor,
            double absorbedRemateMm = 0d,
            bool withYellowModule = true,
            double solapeEachEndMm = 0d)
        {
            var outElements = new List<Atk60ElementPaintItem>();
            if (wall == null || anchor == null || PillarFormwork.IsPillar(wall))
            {
                return outElements;
            }

            var attrs = wall.Attributes;
            var thicknessMm = Atk60FaceInsertion.ResolveWallThicknessMm(attrs);
            var mm = (int)Math.Round(thicknessMm);
            var yawRad = Atk60FaceInsertion.NormalizeYawToRad(anchor.RotY);
            var ux = Math.Cos(yawRad);
            var uz = Math.Sin(yawRad);
            var wallHeightMm = Atk60FaceInsertion.ResolveWallHeightMm(attrs);
            var wallLengthMm = WallRunMm(anchor, attrs);
            var idWall = !string.IsNullOrWhiteSpace(wall.WallId)
                ? wall.WallId
                : (!string.IsNullOrWhiteSpace(wall.Id) ? wall.Id : wall.LineId);

            double coverStartMm;
            double coverEndMm;
            CoverMm(wall, absorbedRemateMm, out coverStartMm, out coverEndMm);

            if (mm > 20 && mm < 300)
            {
                if (EndIsFree(attrs, true))
                {
                    AddUnder300(outElements, wall, anchor, attrs, idWall, true, 0d, ux, uz, mm, wallHeightMm, wallLengthMm, coverStartMm, withYellowModule, solapeEachEndMm);
                }

                if (wallLengthMm > 1d && EndIsFree(attrs, false))
                {
                    AddUnder300(outElements, wall, anchor, attrs, idWall, false, wallLengthMm, ux, uz, mm, wallHeightMm, wallLengthMm, coverEndMm, withYellowModule, solapeEachEndMm);
                }

                return outElements;
            }

            if (IsMiddleWood(attrs, true) || IsMiddleWood(attrs, false)
                || IsWideEndWood(attrs, true) || IsWideEndWood(attrs, false))
            {
                if (EndIsFree(attrs, true))
                {
                    if (IsMiddleWood(attrs, true))
                    {
                        AddTapeS11(outElements, anchor, idWall, true, 0d, ux, uz, mm, wallHeightMm, wallLengthMm);
                    }
                    else if (IsWideEndWood(attrs, true))
                    {
                        AddTapeS7(outElements, anchor, idWall, true, 0d, ux, uz, mm, wallHeightMm, wallLengthMm);
                    }
                }

                if (wallLengthMm > 1d && EndIsFree(attrs, false))
                {
                    if (IsMiddleWood(attrs, false))
                    {
                        AddTapeS11(outElements, anchor, idWall, false, wallLengthMm, ux, uz, mm, wallHeightMm, wallLengthMm);
                    }
                    else if (IsWideEndWood(attrs, false))
                    {
                        AddTapeS7(outElements, anchor, idWall, false, wallLengthMm, ux, uz, mm, wallHeightMm, wallLengthMm);
                    }
                }

                return outElements;
            }

            if (IsPanelAndWood(attrs, true) || IsPanelAndWood(attrs, false))
            {
                if (EndIsFree(attrs, true))
                {
                    AddTapeS7(outElements, anchor, idWall, true, 0d, ux, uz, mm, wallHeightMm, wallLengthMm);
                }

                if (wallLengthMm > 1d && EndIsFree(attrs, false))
                {
                    AddTapeS7(outElements, anchor, idWall, false, wallLengthMm, ux, uz, mm, wallHeightMm, wallLengthMm);
                }

                return outElements;
            }

            var widthMm = ExactWidthMm(thicknessMm);
            if (!widthMm.HasValue)
            {
                return outElements;
            }

            var layout = Modulo270HeightPanelCatalog.ResolveForModule(wallHeightMm, widthMm.Value, true, true);

            if (EndIsFree(attrs, true))
            {
                AddEnd(outElements, wall, anchor, attrs, layout, idWall, true, 0d, ux, uz, widthMm.Value, wallHeightMm, wallLengthMm, coverStartMm, withYellowModule, solapeEachEndMm);
            }

            if (wallLengthMm > 1d && EndIsFree(attrs, false))
            {
                AddEnd(outElements, wall, anchor, attrs, layout, idWall, false, wallLengthMm, ux, uz, widthMm.Value, wallHeightMm, wallLengthMm, coverEndMm, withYellowModule, solapeEachEndMm);
            }

            return outElements;
        }

        private static void AddEnd(
            List<Atk60ElementPaintItem> outElements,
            Desing2FormworkWallDto wall,
            Atk60WallPaintAnchor anchor,
            AttributesList attrs,
            Modulo270Layout layout,
            string idWall,
            bool atStart,
            double alongMm,
            double ux,
            double uz,
            int widthMm,
            double wallHeightMm,
            double wallLengthMm,
            double coverMm,
            bool withYellowModule,
            double solapeEachEndMm)
        {
            var tape = ResolveTape(attrs, atStart);
            double originX;
            double originZ;
            double rotY;
            EndPose(anchor, atStart, alongMm, ux, uz, widthMm, out originX, out originZ, out rotY);

            if (tape == "TapeS2")
            {
                AddTapeS2(
                    outElements, wall, anchor, idWall,
                    atStart, alongMm, ux, uz, widthMm,
                    wallHeightMm, wallLengthMm, originX, originZ, rotY, coverMm, withYellowModule, solapeEachEndMm);
                return;
            }

            if (tape == "TapeS1")
            {
                AddTapeS1(
                    outElements, wall, anchor, idWall,
                    atStart, alongMm, ux, uz, widthMm,
                    wallHeightMm, wallLengthMm, originX, originZ, rotY, coverMm, withYellowModule, solapeEachEndMm);
                return;
            }

            if (tape == "TapeS5")
            {
                AddSolution2(
                    outElements, anchor, idWall,
                    atStart, alongMm, ux, uz, widthMm, wallHeightMm);
                return;
            }

            if (layout == null || layout.Pieces == null)
            {
                return;
            }

            for (var i = 0; i < layout.Pieces.Count; i++)
            {
                var piece = layout.Pieces[i];
                if (piece == null)
                {
                    continue;
                }

                var y = anchor.Y + piece.UpOffsetMm;
                if (string.Equals(piece.Orientation, "Tumbado", StringComparison.OrdinalIgnoreCase))
                {
                    y += piece.PieceHeightMm;
                }

                outElements.Add(new Atk60ElementPaintItem
                {
                    IdWall = !string.IsNullOrWhiteSpace(idWall) ? idWall : anchor.IdWall,
                    ElementType = "Panel",
                    ElementCode = piece.ElementCode,
                    Orientation = piece.Orientation,
                    ImportPath = piece.ImportPath,
                    Color = "frame-yellow",
                    X = originX,
                    Y = y,
                    Z = originZ,
                    RotX = 0,
                    RotY = rotY,
                    RotZ = 0,
                    NormalX = atStart ? -ux : ux,
                    NormalZ = atStart ? -uz : uz,
                    FaceSign = anchor.FaceSign,
                    IsMirrored = !atStart,
                    ModuleLengthMm = widthMm,
                    ModuleIndex = 1,
                    ModuleCountInWall = 1,
                    WallHeightMm = wallHeightMm,
                    WallThicknessMm = widthMm,
                    WallLengthMm = wallLengthMm,
                    PieceWidthMm = piece.PieceWidthMm,
                    PieceHeightMm = piece.PieceHeightMm,
                    LocalAlongMm = alongMm,
                    LocalUpMm = piece.UpOffsetMm,
                    UseStrictPose = true,
                    PieceIndexInModule = i + 1,
                    PieceCountInModule = layout.Pieces.Count,
                    CatalogHeightMm = layout.CatalogHeightMm,
                });
            }

            AddAnglesAndClamps(
                outElements, anchor, layout, idWall,
                atStart, alongMm, ux, uz, widthMm, wallHeightMm, wallLengthMm);
        }

        /// <summary>
        /// TapeS7 del Desing antiguo, y el TapeS8 del formulario (0,44 y el resto
        /// que no es múltiplo de 0,050). Angular en las dos caras, el panel que cabe
        /// y el taco en el sobrante. En 0,44 m: panel de 0,30 y taco de 0,14.
        /// La grapa fija del muro se copia en las dos caras: la del panel y la del angular del tablón.
        /// En el canto une el panel de fondo con el angular. El tablón lo cierra la regulable.
        /// </summary>
        private static bool IsPanelAndWood(AttributesList attrs, bool atStart)
        {
            var tape = ResolveTape(attrs, atStart);
            if (tape != "TapeS7" && tape != "TapeS8")
            {
                return false;
            }

            var mm = (int)Math.Round(Atk60FaceInsertion.ResolveWallThicknessMm(attrs));
            return mm > 300 && mm < 600;
        }

        /// <summary>
        /// Solución 2 del espesor mayor de 0,60 m que no es múltiplo de 0,050.
        /// En el formulario se guarda como TapeS12. Angular, paneles y taco en el medio.
        /// </summary>
        private static bool IsMiddleWood(AttributesList attrs, bool atStart)
        {
            if (IsSolapeEnd(attrs, atStart))
            {
                return false;
            }

            var stored = StoredId(attrs, atStart);
            return string.Equals(stored, "TapeS12", StringComparison.OrdinalIgnoreCase);
        }

        /// <summary>
        /// Solución 1 de ese mismo espesor. Es el TapeS7: el panel que cabe y el taco en el extremo.
        /// En 0,67 m el panel es de 0,60 y el taco de 0,07. Se guarda como TapeS13.
        /// </summary>
        private static bool IsWideEndWood(AttributesList attrs, bool atStart)
        {
            if (IsSolapeEnd(attrs, atStart))
            {
                return false;
            }

            var stored = StoredId(attrs, atStart);
            return string.Equals(stored, "TapeS13", StringComparison.OrdinalIgnoreCase);
        }

        private static bool IsSolapeEnd(AttributesList attrs, bool atStart)
        {
            if (Flag(attrs, atStart ? "_ChekSolapeIni" : "_ChekSolapeEnd"))
            {
                return true;
            }

            var stored = StoredId(attrs, atStart);
            return string.Equals(stored, "TapeS1", StringComparison.OrdinalIgnoreCase)
                || string.Equals(stored, "TapeS2", StringComparison.OrdinalIgnoreCase);
        }

        private static void AddTapeS7(
            List<Atk60ElementPaintItem> outElements,
            Atk60WallPaintAnchor anchor,
            string idWall,
            bool atStart,
            double alongMm,
            double ux,
            double uz,
            int thicknessMm,
            double wallHeightMm,
            double wallLengthMm)
        {
            var panelMm = 300;
            if (thicknessMm >= 450) panelMm = 450;
            if (thicknessMm >= 600) panelMm = 600;
            if (thicknessMm >= 750) panelMm = 750;
            if (thicknessMm >= 900) panelMm = 900;
            if (panelMm > thicknessMm) panelMm = 300;
            var woodMm = thicknessMm - panelMm;
            if (woodMm < 1)
            {
                return;
            }

            var end = atStart ? -1d : 1d;
            var outX = ux * end;
            var outZ = uz * end;
            var nx = anchor.NormalX;
            var nz = anchor.NormalZ;
            var nLen = Math.Sqrt((nx * nx) + (nz * nz));
            if (nLen > 1e-6)
            {
                nx /= nLen;
                nz /= nLen;
            }

            var yaw = Math.Atan2(outZ, outX);
            var wallId = !string.IsNullOrWhiteSpace(idWall) ? idWall : anchor.IdWall;
            double originX;
            double originZ;
            double rotY;
            EndPose(anchor, atStart, alongMm, ux, uz, thicknessMm, out originX, out originZ, out rotY);

            // El +X del taco, con este RotY, es (outZ, -outX). Si eso aumenta el espesor,
            // el taco sale desde el panel. Si no, sale desde la cara de detrás.
            var acrossX = outZ;
            var acrossZ = -outX;
            var woodFromPanel = ((-nx * acrossX) + (-nz * acrossZ)) >= 0d;
            double woodX;
            double woodZ;
            EndOffset(
                anchor, alongMm, ux, uz, nx, nz, outX, outZ,
                woodFromPanel ? panelMm : thicknessMm, 0d, out woodX, out woodZ);
            var wood = Wood(anchor, wallId, woodX, woodZ, rotY, woodMm, wallHeightMm, wallLengthMm, alongMm);
            wood.Color = "wood-side";
            wood.WallThicknessMm = BoardDepthMm;
            outElements.Add(wood);

            var bands = HeightBands(wallHeightMm);
            for (var i = 0; i < bands.Count; i++)
            {
                var baseY = anchor.Y + bands[i].UpMm;
                var height = bands[i].HeightMm;
                AddStopPanel(
                    outElements, anchor, wallId, alongMm, ux, uz, outX, outZ, nx, nz, yaw,
                    0d, panelMm, FittingStopCode(panelMm, height), baseY, height, wallHeightMm);
                AddAngle(
                    outElements, anchor, wallId, true, alongMm, ux, uz, outX, outZ, nx, nz,
                    thicknessMm, baseY, height, wallHeightMm, wallLengthMm);
                AddAngle(
                    outElements, anchor, wallId, false, alongMm, ux, uz, outX, outZ, nx, nz,
                    thicknessMm, baseY, height, wallHeightMm, wallLengthMm);

                var clampHeights = ClampHeightsMm(height);
                var wallYaw = Math.Atan2(uz, ux);
                var faceSign = anchor.FaceSign >= 0d ? 1d : -1d;
                for (var hi = 0; hi < clampHeights.Length; hi++)
                {
                    var upMm = bands[i].UpMm + clampHeights[hi];
                    Atk60FaceInsertion.BothFaces(
                        outElements, anchor, wallId, ClampPath,
                        alongMm, upMm, wallYaw, ux, uz, faceSign,
                        wallHeightMm, thicknessMm, wallLengthMm,
                        Atk60FaceInsertion.PanelFaceMm, 0d, ClampCode, "zinc",
                        false, true, true);
                    AddEndClamp(
                        outElements, anchor, wallId, alongMm, ux, uz, outX, outZ, nx, nz,
                        40d, anchor.Y + upMm, upMm, wallHeightMm, thicknessMm, wallLengthMm);
                    AddEndClamp(
                        outElements, anchor, wallId, alongMm, ux, uz, outX, outZ, nx, nz,
                        panelMm - 40d, anchor.Y + upMm, upMm, wallHeightMm, thicknessMm, wallLengthMm);

                    var variableUp = upMm + VariableClampRaiseMm;
                    if (variableUp > bands[i].UpMm + height - 40d)
                    {
                        continue;
                    }

                    AddEndClampPiece(
                        outElements, anchor, wallId, alongMm, ux, uz, outX, outZ, nx, nz,
                        panelMm, anchor.Y + variableUp, variableUp, wallHeightMm, thicknessMm, wallLengthMm,
                        VariableClampPath, VariableClampCode);
                    AddEndClampPiece(
                        outElements, anchor, wallId, alongMm, ux, uz, outX, outZ, nx, nz,
                        thicknessMm, anchor.Y + variableUp, variableUp, wallHeightMm, thicknessMm, wallLengthMm,
                        VariableClampSlidePath, VariableClampSlideCode);
                }
            }
        }

        /// <summary>
        /// Angular, paneles, taco en el medio y paneles, angular.
        /// En 0,67 m: panel de 0,30, taco de 0,07 y panel de 0,30.
        /// La grapa fija une cada angular con su panel. La variable cierra el taco.
        /// </summary>
        private static void AddTapeS11(
            List<Atk60ElementPaintItem> outElements,
            Atk60WallPaintAnchor anchor,
            string idWall,
            bool atStart,
            double alongMm,
            double ux,
            double uz,
            int thicknessMm,
            double wallHeightMm,
            double wallLengthMm)
        {
            List<int> primaryPanels;
            List<int> oppositePanels;
            int woodMm;
            if (!TryMiddleLayout(thicknessMm, out primaryPanels, out woodMm, out oppositePanels))
            {
                return;
            }

            var woodStart = 0;
            for (var p = 0; p < primaryPanels.Count; p++)
            {
                woodStart += primaryPanels[p];
            }

            var end = atStart ? -1d : 1d;
            var outX = ux * end;
            var outZ = uz * end;
            var nx = anchor.NormalX;
            var nz = anchor.NormalZ;
            var nLen = Math.Sqrt((nx * nx) + (nz * nz));
            if (nLen > 1e-6)
            {
                nx /= nLen;
                nz /= nLen;
            }

            var yaw = Math.Atan2(outZ, outX);
            var wallId = !string.IsNullOrWhiteSpace(idWall) ? idWall : anchor.IdWall;
            double originX;
            double originZ;
            double rotY;
            EndPose(anchor, atStart, alongMm, ux, uz, thicknessMm, out originX, out originZ, out rotY);

            var acrossX = outZ;
            var acrossZ = -outX;
            var woodGrowsAcross = ((-nx * acrossX) + (-nz * acrossZ)) >= 0d;
            double woodX;
            double woodZ;
            EndOffset(
                anchor, alongMm, ux, uz, nx, nz, outX, outZ,
                woodGrowsAcross ? woodStart : woodStart + woodMm, 0d, out woodX, out woodZ);
            var wood = Wood(anchor, wallId, woodX, woodZ, rotY, woodMm, wallHeightMm, wallLengthMm, alongMm);
            wood.Color = "wood-side";
            wood.WallThicknessMm = BoardDepthMm;
            outElements.Add(wood);

            var bands = HeightBands(wallHeightMm);
            for (var i = 0; i < bands.Count; i++)
            {
                var baseY = anchor.Y + bands[i].UpMm;
                var height = bands[i].HeightMm;
                var t = 0d;
                for (var p = 0; p < primaryPanels.Count; p++)
                {
                    var panelMm = primaryPanels[p];
                    AddStopPanel(
                        outElements, anchor, wallId, alongMm, ux, uz, outX, outZ, nx, nz, yaw,
                        t, panelMm, FittingStopCode(panelMm, height), baseY, height, wallHeightMm);
                    t += panelMm;
                }

                t = woodStart + woodMm;
                for (var p = 0; p < oppositePanels.Count; p++)
                {
                    var panelMm = oppositePanels[p];
                    AddStopPanel(
                        outElements, anchor, wallId, alongMm, ux, uz, outX, outZ, nx, nz, yaw,
                        t, panelMm, FittingStopCode(panelMm, height), baseY, height, wallHeightMm);
                    t += panelMm;
                }

                AddAngle(
                    outElements, anchor, wallId, true, alongMm, ux, uz, outX, outZ, nx, nz,
                    thicknessMm, baseY, height, wallHeightMm, wallLengthMm);
                AddAngle(
                    outElements, anchor, wallId, false, alongMm, ux, uz, outX, outZ, nx, nz,
                    thicknessMm, baseY, height, wallHeightMm, wallLengthMm);

                var clampHeights = ClampHeightsMm(height);
                var wallYaw = Math.Atan2(uz, ux);
                var faceSign = anchor.FaceSign >= 0d ? 1d : -1d;
                for (var hi = 0; hi < clampHeights.Length; hi++)
                {
                    var upMm = bands[i].UpMm + clampHeights[hi];
                    Atk60FaceInsertion.BothFaces(
                        outElements, anchor, wallId, ClampPath,
                        alongMm, upMm, wallYaw, ux, uz, faceSign,
                        wallHeightMm, thicknessMm, wallLengthMm,
                        Atk60FaceInsertion.PanelFaceMm, 0d, ClampCode, "zinc",
                        false, true, true);
                    AddEndClamp(
                        outElements, anchor, wallId, alongMm, ux, uz, outX, outZ, nx, nz,
                        40d, anchor.Y + upMm, upMm, wallHeightMm, thicknessMm, wallLengthMm);
                    AddEndClamp(
                        outElements, anchor, wallId, alongMm, ux, uz, outX, outZ, nx, nz,
                        thicknessMm - 40d, anchor.Y + upMm, upMm, wallHeightMm, thicknessMm, wallLengthMm);
                    AddPanelSeamClamps(
                        outElements, anchor, wallId, alongMm, ux, uz, outX, outZ, nx, nz,
                        primaryPanels, 0d, anchor.Y + upMm, upMm, wallHeightMm, thicknessMm, wallLengthMm);
                    AddPanelSeamClamps(
                        outElements, anchor, wallId, alongMm, ux, uz, outX, outZ, nx, nz,
                        oppositePanels, woodStart + woodMm, anchor.Y + upMm, upMm, wallHeightMm, thicknessMm, wallLengthMm);

                    var variableUp = upMm + VariableClampRaiseMm;
                    if (variableUp > bands[i].UpMm + height - 40d)
                    {
                        continue;
                    }

                    AddEndClampPiece(
                        outElements, anchor, wallId, alongMm, ux, uz, outX, outZ, nx, nz,
                        woodStart, anchor.Y + variableUp, variableUp, wallHeightMm, thicknessMm, wallLengthMm,
                        VariableClampPath, VariableClampCode);
                    AddEndClampPiece(
                        outElements, anchor, wallId, alongMm, ux, uz, outX, outZ, nx, nz,
                        woodStart + woodMm, anchor.Y + variableUp, variableUp, wallHeightMm, thicknessMm, wallLengthMm,
                        VariableClampSlidePath, VariableClampSlideCode);
                }
            }
        }

        private static void AddPanelSeamClamps(
            List<Atk60ElementPaintItem> outElements,
            Atk60WallPaintAnchor anchor,
            string wallId,
            double alongMm,
            double ux,
            double uz,
            double outX,
            double outZ,
            double nx,
            double nz,
            List<int> panels,
            double tStart,
            double y,
            double upMm,
            double wallHeightMm,
            int thicknessMm,
            double wallLengthMm)
        {
            if (panels == null || panels.Count < 2)
            {
                return;
            }

            var seam = tStart;
            for (var i = 0; i < panels.Count - 1; i++)
            {
                seam += panels[i];
                AddEndClamp(
                    outElements, anchor, wallId, alongMm, ux, uz, outX, outZ, nx, nz,
                    seam - 40d, y, upMm, wallHeightMm, thicknessMm, wallLengthMm);
                AddEndClamp(
                    outElements, anchor, wallId, alongMm, ux, uz, outX, outZ, nx, nz,
                    seam + 40d, y, upMm, wallHeightMm, thicknessMm, wallLengthMm);
            }
        }

        /// <summary>
        /// Paneles de 0,90, 0,75, 0,60, 0,45 o 0,30. El taco, de 10 a 300 mm, queda entre los dos lados.
        /// Primero se cubre el espesor. Si empatan, el taco se acerca al centro.
        /// </summary>
        private static bool TryMiddleLayout(
            int thicknessMm,
            out List<int> primaryPanels,
            out int woodMm,
            out List<int> oppositePanels)
        {
            primaryPanels = null;
            oppositePanels = null;
            woodMm = 0;
            if (thicknessMm < 610)
            {
                return false;
            }

            var sizes = new[] { 900, 750, 600, 450, 300 };
            var current = new List<int>();
            List<int> best = null;
            var bestWood = int.MaxValue;
            var bestImbalance = int.MaxValue;
            var bestMask = 0;
            SearchMiddle(sizes, thicknessMm, current, 900, ref best, ref bestWood, ref bestImbalance, ref bestMask);
            if (best == null)
            {
                return false;
            }

            primaryPanels = new List<int>();
            oppositePanels = new List<int>();
            for (var i = 0; i < best.Count; i++)
            {
                if ((bestMask & (1 << i)) != 0)
                {
                    primaryPanels.Add(best[i]);
                }
                else
                {
                    oppositePanels.Add(best[i]);
                }
            }

            primaryPanels.Sort((a, b) => b.CompareTo(a));
            oppositePanels.Sort((a, b) => a.CompareTo(b));
            woodMm = bestWood;
            return primaryPanels.Count > 0 && oppositePanels.Count > 0;
        }

        private static void SearchMiddle(
            int[] sizes,
            int thicknessMm,
            List<int> current,
            int maxSize,
            ref List<int> best,
            ref int bestWood,
            ref int bestImbalance,
            ref int bestMask)
        {
            if (current.Count >= 2)
            {
                var sum = 0;
                for (var i = 0; i < current.Count; i++)
                {
                    sum += current[i];
                }

                var wood = thicknessMm - sum;
                if (wood >= 10 && wood <= 300)
                {
                    int imbalance;
                    int mask;
                    if (MiddleSplit(current, out imbalance, out mask)
                        && (wood < bestWood
                            || (wood == bestWood && imbalance < bestImbalance)
                            || (wood == bestWood && imbalance == bestImbalance && (best == null || current.Count < best.Count))))
                    {
                        best = new List<int>(current);
                        bestWood = wood;
                        bestImbalance = imbalance;
                        bestMask = mask;
                    }
                }
            }

            if (current.Count >= 8)
            {
                return;
            }

            for (var s = 0; s < sizes.Length; s++)
            {
                var size = sizes[s];
                if (size > maxSize)
                {
                    continue;
                }

                var sum = 0;
                for (var i = 0; i < current.Count; i++)
                {
                    sum += current[i];
                }

                if (sum + size > thicknessMm - 10)
                {
                    continue;
                }

                current.Add(size);
                SearchMiddle(sizes, thicknessMm, current, size, ref best, ref bestWood, ref bestImbalance, ref bestMask);
                current.RemoveAt(current.Count - 1);
            }
        }

        private static bool MiddleSplit(List<int> panels, out int imbalance, out int mask)
        {
            imbalance = int.MaxValue;
            mask = 0;
            var n = panels.Count;
            if (n < 2 || n > 12)
            {
                return false;
            }

            var total = 0;
            for (var i = 0; i < n; i++)
            {
                total += panels[i];
            }

            var limit = 1 << n;
            var found = false;
            for (var candidate = 1; candidate < limit - 1; candidate++)
            {
                if ((candidate & 1) == 0)
                {
                    continue;
                }

                var left = 0;
                var leftCount = 0;
                for (var i = 0; i < n; i++)
                {
                    if ((candidate & (1 << i)) != 0)
                    {
                        left += panels[i];
                        leftCount++;
                    }
                }

                if (leftCount == 0 || leftCount == n)
                {
                    continue;
                }

                var diff = Math.Abs((left * 2) - total);
                var countGap = Math.Abs((leftCount * 2) - n);
                var bestCountGap = Math.Abs((BitCount(mask) * 2) - n);
                if (!found || diff < imbalance || (diff == imbalance && countGap < bestCountGap))
                {
                    imbalance = diff;
                    mask = candidate;
                    found = true;
                }
            }

            return found;
        }

        private static int BitCount(int value)
        {
            var count = 0;
            while (value != 0)
            {
                count += value & 1;
                value >>= 1;
            }

            return count;
        }

        private static string FittingStopCode(int panelMm, int heightMm)
        {
            var family = heightMm <= 1200 ? 1200 : (heightMm <= 2400 ? 2400 : 2700);
            if (family == 1200)
            {
                if (panelMm >= 900) return "12904215";
                if (panelMm >= 750) return "12754120";
                if (panelMm >= 600) return "12604213";
                if (panelMm >= 450) return "12454212";
                return "12304211";
            }

            if (family == 2400)
            {
                if (panelMm >= 900) return "24904240";
                if (panelMm >= 750) return "24754224";
                if (panelMm >= 600) return "24604242";
                if (panelMm >= 450) return "24454243";
                return "24304244";
            }

            if (panelMm >= 900) return "27904209";
            if (panelMm >= 750) return "27754219";
            if (panelMm >= 600) return "27604207";
            if (panelMm >= 450) return "27454206";
            return "27304205";
        }

        /// <summary>
        /// Solución 2 del Desing antiguo (TapeS5).
        /// Hasta 0,35 m: panel regulable de 0,75 y gancho de cierre en las dos caras.
        /// De 0,45 en adelante: el regulable (y un 0,30 si no cubre) con el sobrante
        /// hacia una cara, angular en la otra y gancho de cierre en la del sobrante.
        /// </summary>
        private static void AddSolution2(
            List<Atk60ElementPaintItem> outElements,
            Atk60WallPaintAnchor anchor,
            string idWall,
            bool atStart,
            double alongMm,
            double ux,
            double uz,
            int widthMm,
            double wallHeightMm)
        {
            var end = atStart ? -1d : 1d;
            var outX = ux * end;
            var outZ = uz * end;
            var nx = anchor.NormalX;
            var nz = anchor.NormalZ;
            var nLen = Math.Sqrt((nx * nx) + (nz * nz));
            if (nLen > 1e-6)
            {
                nx /= nLen;
                nz /= nLen;
            }

            var yaw = Math.Atan2(outZ, outX);
            var wallId = !string.IsNullOrWhiteSpace(idWall) ? idWall : anchor.IdWall;
            var narrow = widthMm <= 350;
            var bands = HeightBands(wallHeightMm);

            for (var i = 0; i < bands.Count; i++)
            {
                var up = bands[i].UpMm;
                var height = bands[i].HeightMm;
                var baseY = anchor.Y + up;

                if (narrow)
                {
                    AddStopPanel(outElements, anchor, wallId, alongMm, ux, uz, outX, outZ, nx, nz, yaw, widthMm * 0.5 - 350d, 750d, AdjustableCode(height), baseY, height, wallHeightMm);
                }
                else if (widthMm <= 550)
                {
                    AddStopPanel(outElements, anchor, wallId, alongMm, ux, uz, outX, outZ, nx, nz, yaw, widthMm - 750d, 750d, AdjustableCode(height), baseY, height, wallHeightMm);
                }
                else
                {
                    AddStopPanel(outElements, anchor, wallId, alongMm, ux, uz, outX, outZ, nx, nz, yaw, widthMm - 300d, 300d, NarrowCode(height), baseY, height, wallHeightMm);
                    AddStopPanel(outElements, anchor, wallId, alongMm, ux, uz, outX, outZ, nx, nz, yaw, widthMm - 1050d, 750d, AdjustableCode(height), baseY, height, wallHeightMm);
                    var seamHeights = ClampHeightsMm(height);
                    for (var hi = 0; hi < seamHeights.Length; hi++)
                    {
                        AddEndClamp(
                            outElements, anchor, wallId, alongMm, ux, uz, outX, outZ, nx, nz,
                            widthMm - 300d, anchor.Y + up + seamHeights[hi], up + seamHeights[hi],
                            wallHeightMm, widthMm, alongMm > 1d ? alongMm : widthMm);
                    }
                }

                var hooks = HookHeightsMm(height);
                for (var hi = 0; hi < hooks.Length; hi++)
                {
                    var y = baseY + hooks[hi];
                    AddClosure(outElements, anchor, wallId, alongMm, ux, uz, outX, outZ, nx, nz, yaw, -60d, 0d, y, "1920811", Math.PI, false);
                    AddClosure(outElements, anchor, wallId, alongMm, ux, uz, outX, outZ, nx, nz, yaw, -150d, 120d, y, "10443020", Math.PI * 0.5, false);
                    if (narrow)
                    {
                        AddClosure(outElements, anchor, wallId, alongMm, ux, uz, outX, outZ, nx, nz, yaw, widthMm + 60d, 0d, y, "1920811", 0d, true);
                        AddClosure(outElements, anchor, wallId, alongMm, ux, uz, outX, outZ, nx, nz, yaw, widthMm + 140d, 120d, y, "10443020", Math.PI * 0.5, false);
                    }
                }

                if (!narrow)
                {
                    AddAngle(outElements, anchor, wallId, false, alongMm, ux, uz, outX, outZ, nx, nz, widthMm, baseY, height, wallHeightMm, alongMm > 1d ? alongMm : widthMm);
                }
            }
        }

        private static void AddStopPanel(
            List<Atk60ElementPaintItem> outElements,
            Atk60WallPaintAnchor anchor,
            string idWall,
            double alongMm,
            double ux,
            double uz,
            double outX,
            double outZ,
            double nx,
            double nz,
            double yaw,
            double tStartMm,
            double pieceWidthMm,
            string code,
            double baseY,
            int heightMm,
            double wallHeightMm)
        {
            var growsTowardOpposite = ((-outZ * nx) + (outX * nz)) >= 0d;
            var originT = growsTowardOpposite ? tStartMm : tStartMm + pieceWidthMm;
            double x;
            double z;
            EndOffset(anchor, alongMm, ux, uz, nx, nz, outX, outZ, originT, 0d, out x, out z);
            AddPillarPiece(outElements, anchor, idWall, code, "frame-yellow", x, baseY, z, yaw, -Math.PI * 1.5, heightMm, wallHeightMm, false);
            AddPillarPiece(outElements, anchor, idWall, code, "phenolic", x, baseY, z, yaw, -Math.PI * 1.5, heightMm, wallHeightMm, true);
        }

        private static void AddClosure(
            List<Atk60ElementPaintItem> outElements,
            Atk60WallPaintAnchor anchor,
            string idWall,
            double alongMm,
            double ux,
            double uz,
            double outX,
            double outZ,
            double nx,
            double nz,
            double yaw,
            double tMm,
            double outMm,
            double y,
            string code,
            double rotZ,
            bool oppositeHook)
        {
            double x;
            double z;
            EndOffset(anchor, alongMm, ux, uz, nx, nz, outX, outZ, tMm, outMm, out x, out z);
            AddPillarPiece(
                outElements, anchor, idWall, code, code == "1920811" ? "hook" : "nut",
                x, y, z, oppositeHook ? yaw + Math.PI : yaw, rotZ, 0, 0d, false);
        }

        private static void AddPillarPiece(
            List<Atk60ElementPaintItem> outElements,
            Atk60WallPaintAnchor anchor,
            string idWall,
            string code,
            string color,
            double x,
            double y,
            double z,
            double yaw,
            double rotZ,
            int heightMm,
            double wallHeightMm,
            bool phenolic)
        {
            var path = phenolic
                ? "../../Content/DesignTools/Stl/ATK60/" + code + "_F.stl"
                : "../../Content/DesignTools/Stl/ATK60/" + code + ".stl";
            outElements.Add(new Atk60ElementPaintItem
            {
                IdWall = idWall,
                ElementType = "Panel",
                ElementCode = code,
                Orientation = "pillar",
                ImportPath = path,
                Color = color,
                X = x,
                Y = y,
                Z = z,
                RotX = code == "10443020" ? Math.PI * 0.5 : Math.PI * 1.5,
                RotY = yaw,
                RotZ = rotZ,
                UseStrictPose = true,
                PieceWidthMm = 750d,
                PieceHeightMm = heightMm,
                WallHeightMm = wallHeightMm,
                FaceSign = anchor.FaceSign,
            });
        }

        private static void EndOffset(
            Atk60WallPaintAnchor anchor,
            double alongMm,
            double ux,
            double uz,
            double nx,
            double nz,
            double outX,
            double outZ,
            double tMm,
            double outMm,
            out double x,
            out double z)
        {
            x = anchor.X + (ux * alongMm) - (nx * tMm) + (outX * outMm);
            z = anchor.Z + (uz * alongMm) - (nz * tMm) + (outZ * outMm);
        }

        private static List<HeightBand> HeightBands(double wallHeightMm)
        {
            var bands = new List<HeightBand>();
            var full = (int)((wallHeightMm + 249d) / 2700d);
            var used = 0;
            for (var i = 0; i < full; i++)
            {
                bands.Add(new HeightBand { UpMm = used, HeightMm = 2700 });
                used += 2700;
            }

            var rest = (int)Math.Round(wallHeightMm - used);
            if (rest > 1)
            {
                if (rest <= 1200) bands.Add(new HeightBand { UpMm = used, HeightMm = 1200 });
                else if (rest <= 2400) bands.Add(new HeightBand { UpMm = used, HeightMm = 2400 });
                else bands.Add(new HeightBand { UpMm = used, HeightMm = 2700 });
            }

            if (bands.Count == 0)
            {
                bands.Add(new HeightBand { UpMm = 0, HeightMm = 2700 });
            }

            return bands;
        }

        private static string AdjustableCode(int heightMm)
        {
            if (heightMm <= 1200) return "12104120";
            if (heightMm <= 2400) return "24104224";
            return "27104219";
        }

        private static string NarrowCode(int heightMm)
        {
            if (heightMm <= 1200) return "12304211";
            if (heightMm <= 2400) return "24304244";
            return "27304205";
        }

        private static double[] HookHeightsMm(int pieceHeightMm)
        {
            if (pieceHeightMm <= 1200) return new[] { 350d, 1150d };
            if (pieceHeightMm <= 2400) return new[] { 400d, 2000d };
            return new[] { 550d, 1350d, 2150d };
        }

        private sealed class HeightBand
        {
            public int UpMm;
            public int HeightMm;
        }

        /// <summary>
        /// Angular de esquina en las dos caras y grapa fija en la junta.
        /// El angular mide el canto en X/Y y la altura en Z: la escala usa el canto
        /// para no estirarlo.
        /// </summary>
        private static void AddAnglesAndClamps(
            List<Atk60ElementPaintItem> outElements,
            Atk60WallPaintAnchor anchor,
            Modulo270Layout layout,
            string idWall,
            bool atStart,
            double alongMm,
            double ux,
            double uz,
            int widthMm,
            double wallHeightMm,
            double wallLengthMm)
        {
            if (layout == null || layout.Pieces == null)
            {
                return;
            }

            var end = atStart ? -1d : 1d;
            var outX = ux * end;
            var outZ = uz * end;
            var nx = anchor.NormalX;
            var nz = anchor.NormalZ;
            var nLen = Math.Sqrt((nx * nx) + (nz * nz));
            if (nLen > 1e-6)
            {
                nx /= nLen;
                nz /= nLen;
            }

            var yawRad = Math.Atan2(uz, ux);
            var faceSign = anchor.FaceSign >= 0d ? 1d : -1d;
            var wallId = !string.IsNullOrWhiteSpace(idWall) ? idWall : anchor.IdWall;

            for (var i = 0; i < layout.Pieces.Count; i++)
            {
                var piece = layout.Pieces[i];
                if (piece == null || piece.PieceHeightMm < 200)
                {
                    continue;
                }

                var baseY = anchor.Y + piece.UpOffsetMm;
                AddAngle(outElements, anchor, wallId, true, alongMm, ux, uz, outX, outZ, nx, nz, widthMm, baseY, piece.PieceHeightMm, wallHeightMm, wallLengthMm);
                AddAngle(outElements, anchor, wallId, false, alongMm, ux, uz, outX, outZ, nx, nz, widthMm, baseY, piece.PieceHeightMm, wallHeightMm, wallLengthMm);

                var heights = ClampHeightsMm(piece.PieceHeightMm);
                for (var hi = 0; hi < heights.Length; hi++)
                {
                    var upMm = piece.UpOffsetMm + heights[hi];
                    Atk60FaceInsertion.BothFaces(
                        outElements, anchor, wallId, ClampPath,
                        alongMm, upMm, yawRad, ux, uz, faceSign,
                        wallHeightMm, widthMm, wallLengthMm,
                        Atk60FaceInsertion.PanelFaceMm, 0d, ClampCode, "zinc");

                    var y = anchor.Y + upMm;
                    AddEndClamp(outElements, anchor, wallId, alongMm, ux, uz, outX, outZ, nx, nz, 40d, y, upMm, wallHeightMm, widthMm, wallLengthMm);
                    AddEndClamp(outElements, anchor, wallId, alongMm, ux, uz, outX, outZ, nx, nz, widthMm - 40d, y, upMm, wallHeightMm, widthMm, wallLengthMm);
                }
            }
        }

        private static void AddAngle(
            List<Atk60ElementPaintItem> outElements,
            Atk60WallPaintAnchor anchor,
            string idWall,
            bool primaryFace,
            double alongMm,
            double ux,
            double uz,
            double outX,
            double outZ,
            double nx,
            double nz,
            int widthMm,
            double baseY,
            int pieceHeightMm,
            double wallHeightMm,
            double wallLengthMm)
        {
            var faceNx = primaryFace ? nx : -nx;
            var faceNz = primaryFace ? nz : -nz;
            var x = anchor.X + (ux * alongMm);
            var z = anchor.Z + (uz * alongMm);
            if (!primaryFace)
            {
                x -= nx * widthMm;
                z -= nz * widthMm;
            }

            var code = AngleCode(pieceHeightMm);
            outElements.Add(new Atk60ElementPaintItem
            {
                IdWall = idWall,
                ElementType = "Panel",
                ElementCode = code,
                Orientation = "Vertical",
                ImportPath = "../../Content/DesignTools/Stl/ATK60/EEx" + code + ".stl",
                Color = "frame-yellow",
                X = x,
                Y = baseY,
                Z = z,
                // RotX deja el largo en vertical. El giro en planta va en RotZ:
                // con RotY el angular se tumba.
                RotX = -Math.PI * 0.5,
                RotY = 0,
                RotZ = AngleYaw(outX, outZ, faceNx, faceNz),
                NormalX = faceNx,
                NormalZ = faceNz,
                FaceSign = anchor.FaceSign,
                IsMirrored = !primaryFace,
                ModuleLengthMm = widthMm,
                WallHeightMm = wallHeightMm,
                WallThicknessMm = widthMm,
                WallLengthMm = wallLengthMm,
                PieceWidthMm = Atk60FaceInsertion.PanelFaceMm,
                PieceHeightMm = Atk60FaceInsertion.PanelFaceMm,
                LocalAlongMm = alongMm,
                LocalUpMm = baseY - anchor.Y,
                UseStrictPose = true,
                PieceIndexInModule = 1,
                PieceCountInModule = 1,
                CatalogHeightMm = pieceHeightMm,
            });
        }

        private static void AddEndClamp(
            List<Atk60ElementPaintItem> outElements,
            Atk60WallPaintAnchor anchor,
            string idWall,
            double alongMm,
            double ux,
            double uz,
            double outX,
            double outZ,
            double nx,
            double nz,
            double acrossMm,
            double y,
            double upMm,
            double wallHeightMm,
            int widthMm,
            double wallLengthMm,
            double rotZ = 0d)
        {
            AddEndClampPiece(
                outElements, anchor, idWall, alongMm, ux, uz, outX, outZ, nx, nz,
                acrossMm, y, upMm, wallHeightMm, widthMm, wallLengthMm,
                ClampPath, ClampCode, rotZ);
        }

        private static void AddEndClampPiece(
            List<Atk60ElementPaintItem> outElements,
            Atk60WallPaintAnchor anchor,
            string idWall,
            double alongMm,
            double ux,
            double uz,
            double outX,
            double outZ,
            double nx,
            double nz,
            double acrossMm,
            double y,
            double upMm,
            double wallHeightMm,
            int widthMm,
            double wallLengthMm,
            string path,
            string code,
            double rotZ = 0d)
        {
            var x = anchor.X + (ux * alongMm) - (nx * acrossMm) + (outX * Atk60FaceInsertion.PanelFaceMm);
            var z = anchor.Z + (uz * alongMm) - (nz * acrossMm) + (outZ * Atk60FaceInsertion.PanelFaceMm);
            Atk60FaceInsertion.OneFace(
                outElements, anchor, idWall, path,
                x, y, z, 0d, rotZ,
                outX, outZ, anchor.FaceSign, false,
                alongMm, upMm,
                wallHeightMm, widthMm, wallLengthMm,
                code, "zinc");
        }

        /// <summary>
        /// Giro en planta (RotZ) con RotX = -90°. El -X del angular cae en (ax, az)
        /// y el -Y en (bx, bz): los dos cantos hacia fuera de la esquina.
        /// </summary>
        private static double AngleYaw(double ax, double az, double bx, double bz)
        {
            var yaw = Math.Atan2(az, -ax);
            var dot = (Math.Sin(yaw) * bx) + (Math.Cos(yaw) * bz);
            if (dot > 0.5d)
            {
                return yaw;
            }

            return Math.Atan2(bz, -bx);
        }

        private static string AngleCode(int pieceHeightMm)
        {
            if (pieceHeightMm >= 2500)
            {
                return "27000000";
            }

            if (pieceHeightMm >= 2000)
            {
                return "24000000";
            }

            return "12000000";
        }

        private static double[] ClampHeightsMm(int pieceHeightMm)
        {
            if (pieceHeightMm >= 2500)
            {
                return new[] { 450d, 1350d, 2250d };
            }

            if (pieceHeightMm >= 2000)
            {
                return new[] { 400d, 2000d };
            }

            if (pieceHeightMm >= 1000)
            {
                return new[] { 600d };
            }

            return new[] { Math.Max(50d, pieceHeightMm * 0.5) };
        }

        /// <summary>
        /// El ancho del panel (eje local +X) cruza el espesor.
        /// El canto (eje local +Z) sale hacia fuera del muro.
        /// </summary>
        private static void EndPose(
            Atk60WallPaintAnchor anchor,
            bool atStart,
            double alongMm,
            double ux,
            double uz,
            double thicknessMm,
            out double originX,
            out double originZ,
            out double rotY)
        {
            var faceSign = anchor.FaceSign >= 0d ? 1d : -1d;
            var primaryX = anchor.X + (ux * alongMm);
            var primaryZ = anchor.Z + (uz * alongMm);
            var oppositeX = primaryX - (anchor.NormalX * thicknessMm);
            var oppositeZ = primaryZ - (anchor.NormalZ * thicknessMm);
            var fromOpposite = atStart ? faceSign >= 0d : faceSign < 0d;
            originX = fromOpposite ? oppositeX : primaryX;
            originZ = fromOpposite ? oppositeZ : primaryZ;

            var sin = atStart ? -ux : ux;
            var cos = atStart ? -uz : uz;
            rotY = Math.Atan2(sin, cos);
        }

        private static Atk60ElementPaintItem Wood(
            Atk60WallPaintAnchor anchor,
            string idWall,
            double originX,
            double originZ,
            double rotY,
            int widthMm,
            double wallHeightMm,
            double wallLengthMm,
            double alongMm)
        {
            return new Atk60ElementPaintItem
            {
                IdWall = !string.IsNullOrWhiteSpace(idWall) ? idWall : anchor.IdWall,
                ElementType = "Remate",
                ElementCode = "REMATE_WOOD",
                Orientation = "Vertical",
                ImportPath = string.Empty,
                Color = "wood-remate",
                X = originX,
                Y = anchor.Y,
                Z = originZ,
                RotX = 0,
                RotY = rotY,
                RotZ = 0,
                NormalX = 0,
                NormalZ = 0,
                FaceSign = anchor.FaceSign,
                IsMirrored = false,
                ModuleLengthMm = widthMm,
                ModuleIndex = 1,
                ModuleCountInWall = 1,
                WallHeightMm = wallHeightMm,
                WallThicknessMm = BoardDepthMm,
                WallLengthMm = wallLengthMm,
                PieceWidthMm = widthMm,
                PieceHeightMm = wallHeightMm,
                LocalAlongMm = alongMm,
                LocalUpMm = 0,
                UseStrictPose = true,
                PieceIndexInModule = 1,
                PieceCountInModule = 1,
                CatalogHeightMm = wallHeightMm,
            };
        }

        private static Atk60ElementPaintItem SideWood(
            Atk60WallPaintAnchor anchor,
            string idWall,
            double originX,
            double originZ,
            double rotY,
            double widthMm,
            double offsetAcrossMm,
            double depthMm,
            double offsetOutMm,
            string color,
            double wallHeightMm,
            double wallLengthMm,
            double alongMm)
        {
            var piece = Wood(anchor, idWall, originX, originZ, rotY, (int)Math.Round(widthMm), wallHeightMm, wallLengthMm, alongMm);
            piece.Color = color;
            piece.PieceWidthMm = widthMm;
            piece.WallThicknessMm = depthMm;
            piece.InsertOffsetX = offsetAcrossMm;
            piece.InsertOffsetZ = offsetOutMm;
            return piece;
        }

        private static int? ExactWidthMm(double thicknessMm)
        {
            var mm = (int)Math.Round(thicknessMm);
            if (mm == 300 || mm == 450 || mm == 600 || mm == 900)
            {
                return mm;
            }

            return null;
        }

        private static string ResolveTape(AttributesList attrs, bool atStart)
        {
            var stored = StoredId(attrs, atStart);
            if (string.Equals(stored, "TapeS1", StringComparison.OrdinalIgnoreCase))
            {
                return "TapeS1";
            }

            if (string.Equals(stored, "TapeS2", StringComparison.OrdinalIgnoreCase))
            {
                return "TapeS2";
            }

            if (string.Equals(stored, "TapeS5", StringComparison.OrdinalIgnoreCase))
            {
                return "TapeS5";
            }

            if (string.Equals(stored, "Solucion_2", StringComparison.OrdinalIgnoreCase))
            {
                return "TapeS8";
            }

            if (string.Equals(stored, "TapeS7", StringComparison.OrdinalIgnoreCase)
                || string.Equals(stored, "TapeS8", StringComparison.OrdinalIgnoreCase)
                || string.Equals(stored, "TapeS12", StringComparison.OrdinalIgnoreCase)
                || string.Equals(stored, "TapeS13", StringComparison.OrdinalIgnoreCase))
            {
                return stored;
            }

            var solape = Flag(attrs, atStart ? "_ChekSolapeIni" : "_ChekSolapeEnd");
            return solape ? "TapeS1" : "TapeS4";
        }

        private static string StoredId(AttributesList attrs, bool atStart)
        {
            if (attrs == null)
            {
                return null;
            }

            if (atStart)
            {
                return Known(attrs._Tape_1) ? attrs._Tape_1 : attrs.ExtraValueAsString("_Tape_1");
            }

            if (Known(attrs._Tape_2))
            {
                return attrs._Tape_2;
            }

            var fromExtra = attrs.ExtraValueAsString("_Tape_2");
            if (Known(fromExtra))
            {
                return fromExtra;
            }

            return attrs.ExtraValueAsString("_Tape_0");
        }

        private static bool Known(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return false;
            }

            return value.StartsWith("TapeS", StringComparison.OrdinalIgnoreCase)
                || string.Equals(value, "Solucion_2", StringComparison.OrdinalIgnoreCase);
        }

        private static bool EndIsFree(AttributesList attrs, bool atStart)
        {
            if (attrs == null)
            {
                return true;
            }

            var id = atStart ? attrs._Idconnection_1 : attrs._Idconnection_2;
            if (string.IsNullOrWhiteSpace(id))
            {
                id = attrs.ExtraValueAsString(atStart ? "_Idconnection_1" : "_Idconnection_2");
            }

            return string.IsNullOrWhiteSpace(id)
                || string.Equals(id, "null", StringComparison.OrdinalIgnoreCase)
                || id == "0";
        }

        private static bool Flag(AttributesList attrs, string key)
        {
            var value = attrs != null ? attrs.ExtraValueAsString(key) : null;
            return string.Equals(value, "true", StringComparison.OrdinalIgnoreCase)
                || string.Equals(value, "1", StringComparison.OrdinalIgnoreCase);
        }

        private static double WallRunMm(Atk60WallPaintAnchor anchor, AttributesList attrs)
        {
            if (anchor.Debug != null)
            {
                var dx = anchor.Debug.EndX - anchor.Debug.StartX;
                var dz = anchor.Debug.EndZ - anchor.Debug.StartZ;
                var len = Math.Sqrt((dx * dx) + (dz * dz));
                if (len > 1d)
                {
                    return len;
                }
            }

            return Atk60FaceInsertion.ResolveWallLengthMm(attrs);
        }
    }
}
