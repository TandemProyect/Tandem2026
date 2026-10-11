using Desing.Repositories.RepositoryCommun;
using System.Collections.Generic;

namespace Desing.Repositories.Repository_Desing_2.Atk60
{
    /// <summary>
    /// Remate de madera del extremo del muro, con la grapa regulable.
    /// </summary>
    internal static class EndFiller
    {
        private const double DepthMm = 120d;

        internal static List<Atk60ElementPaintItem> Build(
            Desing2FormworkWallDto wall,
            Atk60WallPaintAnchor anchor,
            double fillerLengthMm,
            double startAlongMm)
        {
            var outElements = new List<Atk60ElementPaintItem>();
            if (wall == null || anchor == null || fillerLengthMm <= 1d)
            {
                return outElements;
            }

            var attrs = wall.Attributes;
            var yawRad = Atk60FaceInsertion.NormalizeYawToRad(anchor.RotY);
            var renderYawRad = -yawRad;
            var ux = System.Math.Cos(yawRad);
            var uz = System.Math.Sin(yawRad);
            var faceSign = anchor.FaceSign >= 0d ? 1d : -1d;
            var faceIsRightSide = faceSign < 0d;
            var wallHeightMm = Atk60FaceInsertion.ResolveWallHeightMm(attrs);
            var wallThicknessMm = Atk60FaceInsertion.ResolveWallThicknessMm(attrs);
            var wallLengthMm = Atk60FaceInsertion.ResolveWallLengthMm(attrs);
            var idWall = !string.IsNullOrWhiteSpace(wall.WallId)
                ? wall.WallId
                : (!string.IsNullOrWhiteSpace(wall.Id) ? wall.Id : wall.LineId);

            var baseX = anchor.X + (ux * startAlongMm);
            var baseY = anchor.Y;
            var baseZ = anchor.Z + (uz * startAlongMm);
            var primaryX = baseX + (faceIsRightSide ? ux * fillerLengthMm : 0d);
            var primaryZ = baseZ + (faceIsRightSide ? uz * fillerLengthMm : 0d);
            var primaryRotY = renderYawRad + (faceIsRightSide ? System.Math.PI : 0d);
            var oppositeBaseX = baseX - (anchor.NormalX * wallThicknessMm);
            var oppositeBaseZ = baseZ - (anchor.NormalZ * wallThicknessMm);
            var oppositeX = oppositeBaseX + (!faceIsRightSide ? ux * fillerLengthMm : 0d);
            var oppositeZ = oppositeBaseZ + (!faceIsRightSide ? uz * fillerLengthMm : 0d);
            var oppositeRotY = renderYawRad + (!faceIsRightSide ? System.Math.PI : 0d);

            outElements.Add(Create(anchor, idWall, primaryX, baseY, primaryZ, primaryRotY, false, fillerLengthMm, wallHeightMm, wallThicknessMm, wallLengthMm, startAlongMm));
            outElements.Add(Create(anchor, idWall, oppositeX, baseY, oppositeZ, oppositeRotY, true, fillerLengthMm, wallHeightMm, wallThicknessMm, wallLengthMm, startAlongMm));

            PanelClamp.PlaceOnEndFiller(
                outElements, anchor, idWall,
                startAlongMm, fillerLengthMm,
                yawRad, ux, uz, faceSign,
                wallHeightMm, wallThicknessMm, wallLengthMm);

            return outElements;
        }

        private static Atk60ElementPaintItem Create(
            Atk60WallPaintAnchor anchor,
            string idWall,
            double x,
            double y,
            double z,
            double rotY,
            bool isMirrored,
            double fillerLengthMm,
            double wallHeightMm,
            double wallThicknessMm,
            double wallLengthMm,
            double startAlongMm)
        {
            return new Atk60ElementPaintItem
            {
                IdWall = !string.IsNullOrWhiteSpace(idWall) ? idWall : anchor.IdWall,
                ElementType = "Remate",
                ElementCode = "REMATE_WOOD",
                Orientation = "Vertical",
                ImportPath = string.Empty,
                Color = "wood-remate",
                X = x,
                Y = y,
                Z = z,
                RotX = anchor.RotX,
                RotY = rotY,
                RotZ = anchor.RotZ,
                NormalX = isMirrored ? -anchor.NormalX : anchor.NormalX,
                NormalZ = isMirrored ? -anchor.NormalZ : anchor.NormalZ,
                FaceSign = isMirrored ? -anchor.FaceSign : anchor.FaceSign,
                IsMirrored = isMirrored,
                ModuleLengthMm = fillerLengthMm,
                ModuleIndex = 1,
                ModuleCountInWall = 1,
                WallHeightMm = wallHeightMm,
                WallThicknessMm = DepthMm,
                WallLengthMm = wallLengthMm,
                PieceWidthMm = fillerLengthMm,
                PieceHeightMm = wallHeightMm,
                LocalAlongMm = startAlongMm,
                LocalUpMm = 0,
                InsertOffsetX = 0,
                InsertOffsetY = 0,
                InsertOffsetZ = 0,
                BaseRotX = 0,
                BaseRotY = 0,
                BaseRotZ = 0,
                UseStrictPose = true,
                PieceIndexInModule = 1,
                PieceCountInModule = 1,
                CatalogHeightMm = wallHeightMm,
            };
        }
    }
}
