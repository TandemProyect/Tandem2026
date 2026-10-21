using Desing.Repositories.RepositoryAtk60.ModulosATK60;
using Desing.Repositories.RepositoryCommun;
using System;
using System.Collections.Generic;

namespace Desing.Repositories.Repository_Desing_2.Atk60
{
    /// <summary>
    /// Arma un módulo: paneles, grapas, rigidizadores, tirantes y puntales, en ese orden.
    /// Cada elemento vive en su clase. Aquí solo se decide el orden.
    /// </summary>
    internal static class Atk60ModuleAssembly
    {
        private const double ModuleLengthMm = 2700d;

        internal static List<Atk60ElementPaintItem> Build(
            Desing2FormworkWallDto wall,
            Atk60WallPaintAnchor anchor,
            long module270Count)
        {
            return Build(wall, anchor, module270Count, ModuleLengthMm, 0d, 0, (int)module270Count);
        }

        internal static List<Atk60ElementPaintItem> Build(
            Desing2FormworkWallDto wall,
            Atk60WallPaintAnchor anchor,
            long moduleCount,
            double moduleLengthMm,
            double startAlongMm,
            int moduleIndexOffset,
            int moduleCountInWall,
            bool suppressLastEndJoint = false,
            bool tiesBeyondWall = false,
            bool trimStartTie = false,
            bool trimEndTie = false,
            double trimStartJointMm = 0d,
            double trimEndJointMm = -1d,
            bool skipStartTie = false,
            bool skipFreeEndTie = false)
        {
            var outElements = new List<Atk60ElementPaintItem>();
            if (wall == null || anchor == null || moduleCount <= 0 || moduleLengthMm < 299d)
            {
                return outElements;
            }

            var attrs = wall.Attributes;
            var yawRad = Atk60FaceInsertion.NormalizeYawToRad(anchor.RotY);
            var ux = Math.Cos(yawRad);
            var uz = Math.Sin(yawRad);
            var faceSign = anchor.FaceSign >= 0d ? 1d : -1d;
            var wallHeightMm = Atk60FaceInsertion.ResolveWallHeightMm(attrs);
            var wallThicknessMm = Atk60FaceInsertion.ResolveWallThicknessMm(attrs);
            var wallLengthMm = Atk60FaceInsertion.ResolveWallLengthMm(attrs);
            var useAdjustable750 = Atk60FaceInsertion.UseAdjustable750(attrs);
            var verticalPanels = Atk60FaceInsertion.FlagOn(attrs != null ? attrs._CHeckTypeFormworkMode : null);
            var layout = Modulo270HeightPanelCatalog.ResolveForModule(
                wallHeightMm, (int)Math.Round(moduleLengthMm), useAdjustable750, verticalPanels);
            var idWall = !string.IsNullOrWhiteSpace(wall.WallId)
                ? wall.WallId
                : (!string.IsNullOrWhiteSpace(wall.Id) ? wall.Id : wall.LineId);

            if (layout == null || layout.Pieces == null || layout.Pieces.Count == 0)
            {
                return outElements;
            }

            var countInWall = moduleCountInWall > 0 ? moduleCountInWall : (int)moduleCount;
            for (var i = 0; i < moduleCount; i++)
            {
                var moduleBaseAlongMm = startAlongMm + (i * moduleLengthMm);
                var moduleIndex = moduleIndexOffset + i + 1;
                for (var pi = 0; pi < layout.Pieces.Count; pi++)
                {
                    PanelFormwork.Place(
                        outElements, layout.Pieces[pi], layout, pi, anchor, idWall,
                        moduleBaseAlongMm, moduleLengthMm, moduleIndex, countInWall,
                        yawRad, ux, uz, faceSign,
                        wallHeightMm, wallThicknessMm, wallLengthMm);
                }

                var suppressEnd = suppressLastEndJoint && i == moduleCount - 1;
                PanelClamp.PlaceOnModule(
                    outElements, layout, anchor,
                    moduleBaseAlongMm, moduleLengthMm, moduleIndex, countInWall,
                    yawRad, ux, uz, faceSign,
                    wallHeightMm, wallThicknessMm, wallLengthMm, idWall,
                    suppressEnd);
                PanelStiffener.Place(
                    outElements, layout.Pieces, anchor, idWall,
                    moduleBaseAlongMm, moduleLengthMm,
                    yawRad, ux, uz, faceSign,
                    wallHeightMm, wallThicknessMm, wallLengthMm,
                    Atk60FaceInsertion.FlagOn(attrs != null ? attrs._CHeckRijiOutside : null),
                    Atk60FaceInsertion.FlagOn(attrs != null ? attrs._CHeckRijiInside : null));
                FormTie.Place(
                    outElements, layout, anchor,
                    moduleBaseAlongMm, moduleIndex, countInWall,
                    yawRad, ux, uz, faceSign,
                    wallHeightMm, wallThicknessMm, wallLengthMm, idWall, tiesBeyondWall,
                    trimStartTie, trimEndTie, trimStartJointMm, trimEndJointMm,
                    skipStartTie, skipFreeEndTie);
            }

            return outElements;
        }
    }
}
