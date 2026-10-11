using Desing.Repositories.RepositoryAtk60.ModulosATK60;
using Desing.Repositories.RepositoryCommun;
using System;
using System.Collections.Generic;

namespace Desing.Repositories.Repository_Desing_2.Atk60
{
    /// <summary>
    /// Encofrado de paneles. Las dos caras del muro.
    /// </summary>
    internal static class PanelFormwork
    {
        internal static void Place(
            List<Atk60ElementPaintItem> outElements,
            Modulo270PieceLayout piece,
            Modulo270Layout layout,
            int pieceIndex,
            Atk60WallPaintAnchor anchor,
            string idWall,
            double moduleBaseAlongMm,
            double moduleLengthMm,
            int moduleIndex,
            int countInWall,
            double yawRad,
            double ux,
            double uz,
            double faceSign,
            double wallHeightMm,
            double wallThicknessMm,
            double wallLengthMm)
        {
            var renderYawRad = -yawRad;
            var alongMm = moduleBaseAlongMm + piece.AlongOffsetMm;
            var baseX = anchor.X + (ux * alongMm);
            var isTumbado = string.Equals(piece.Orientation, "Tumbado", StringComparison.OrdinalIgnoreCase);
            var baseY = anchor.Y + piece.UpOffsetMm + (isTumbado ? piece.PieceHeightMm : 0d);
            var baseZ = anchor.Z + (uz * alongMm);
            var faceIsRightSide = faceSign < 0d;
            var primaryX = baseX + (faceIsRightSide ? ux * piece.PieceWidthMm : 0d);
            var primaryZ = baseZ + (faceIsRightSide ? uz * piece.PieceWidthMm : 0d);
            var primaryRotY = renderYawRad + (faceIsRightSide ? Math.PI : 0d);
            var oppositeBaseX = baseX - (anchor.NormalX * wallThicknessMm);
            var oppositeBaseZ = baseZ - (anchor.NormalZ * wallThicknessMm);
            var oppositeX = oppositeBaseX + (!faceIsRightSide ? ux * piece.PieceWidthMm : 0d);
            var oppositeZ = oppositeBaseZ + (!faceIsRightSide ? uz * piece.PieceWidthMm : 0d);
            var oppositeRotY = renderYawRad + (!faceIsRightSide ? Math.PI : 0d);

            outElements.Add(Create(
                piece, layout, pieceIndex, anchor, idWall,
                primaryX, baseY, primaryZ, primaryRotY,
                anchor.NormalX, anchor.NormalZ, anchor.FaceSign, false,
                moduleLengthMm, moduleIndex, countInWall,
                wallHeightMm, wallThicknessMm, wallLengthMm,
                alongMm));

            outElements.Add(Create(
                piece, layout, pieceIndex, anchor, idWall,
                oppositeX, baseY, oppositeZ, oppositeRotY,
                -anchor.NormalX, -anchor.NormalZ, -anchor.FaceSign, true,
                moduleLengthMm, moduleIndex, countInWall,
                wallHeightMm, wallThicknessMm, wallLengthMm,
                alongMm));
        }

        private static Atk60ElementPaintItem Create(
            Modulo270PieceLayout piece,
            Modulo270Layout layout,
            int pieceIndex,
            Atk60WallPaintAnchor anchor,
            string idWall,
            double x,
            double y,
            double z,
            double rotY,
            double normalX,
            double normalZ,
            double faceSign,
            bool mirrored,
            double moduleLengthMm,
            int moduleIndex,
            int countInWall,
            double wallHeightMm,
            double wallThicknessMm,
            double wallLengthMm,
            double alongMm)
        {
            return new Atk60ElementPaintItem
            {
                IdWall = !string.IsNullOrWhiteSpace(idWall) ? idWall : anchor.IdWall,
                ElementType = "Panel",
                ElementCode = piece.ElementCode,
                Orientation = piece.Orientation,
                ImportPath = piece.ImportPath,
                Color = "frame-yellow",
                X = x,
                Y = y,
                Z = z,
                RotX = anchor.RotX,
                RotY = rotY,
                RotZ = anchor.RotZ,
                NormalX = normalX,
                NormalZ = normalZ,
                FaceSign = faceSign,
                IsMirrored = mirrored,
                ModuleLengthMm = moduleLengthMm,
                ModuleIndex = moduleIndex,
                ModuleCountInWall = countInWall,
                WallHeightMm = wallHeightMm,
                WallThicknessMm = wallThicknessMm,
                WallLengthMm = wallLengthMm,
                PieceWidthMm = piece.PieceWidthMm,
                PieceHeightMm = piece.PieceHeightMm,
                LocalAlongMm = alongMm,
                LocalUpMm = piece.UpOffsetMm,
                InsertOffsetX = piece.InsertOffsetX,
                InsertOffsetY = piece.InsertOffsetY,
                InsertOffsetZ = piece.InsertOffsetZ,
                BaseRotX = piece.BaseRotX,
                BaseRotY = piece.BaseRotY,
                BaseRotZ = piece.BaseRotZ,
                UseStrictPose = piece.UseStrictPose,
                PieceIndexInModule = pieceIndex + 1,
                PieceCountInModule = layout.Pieces.Count,
                CatalogHeightMm = layout.CatalogHeightMm,
            };
        }
    }
}
