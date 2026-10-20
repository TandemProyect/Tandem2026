using Desing.Repositories.RepositoryAtk60.ModulosATK60;
using Desing.Repositories.RepositoryCommun;
using System;
using System.Collections.Generic;

namespace Desing.Repositories.Repository_Desing_2.Atk60
{
    /// <summary>
    /// Tirante completo: placa y barra Dywidag.
    /// La placa va en el agujero, a 120 mm de la cara. La barra cruza por el centro del muro.
    /// En cada franja tumbada el tirante va en el borde alto, a los dos lados.
    /// </summary>
    internal static class FormTie
    {
        private const double HoleInsetMm = 43.714d;
        private const string PlatePath = "../../Content/DesignTools/Stl/ATK60/10443020.stl";
        private const string WingPath = "../../Content/DesignTools/Stl/ATK60/1920894.stl";
        private const string NutPath = "../../Content/DesignTools/Stl/ATK60/7238001.stl";
        private const string BarPath = "../../Content/DesignTools/Stl/ATK60/dywidag.stl";

        internal static void Place(
            List<Atk60ElementPaintItem> outElements,
            Modulo270Layout layout,
            Atk60WallPaintAnchor anchor,
            double moduleBaseAlongMm,
            int moduleIndex,
            int countInWall,
            double yawRad,
            double ux,
            double uz,
            double faceSign,
            double wallHeightMm,
            double wallThicknessMm,
            double wallLengthMm,
            string idWall)
        {
            var isLast = moduleIndex >= countInWall;
            double rightmost = -1d;
            foreach (var piece in layout.Pieces)
            {
                if (IsTumbado(piece))
                {
                    continue;
                }

                var end = piece.AlongOffsetMm + piece.PieceWidthMm;
                if (end > rightmost)
                {
                    rightmost = end;
                }
            }

            foreach (var piece in layout.Pieces)
            {
                if (IsTumbado(piece))
                {
                    // El agujero alto es el mismo canto que en vertical: 43,7 mm
                    // bajo el borde superior de esta franja, no el de la de abajo.
                    var topHole = piece.PieceHeightMm - HoleInsetMm;
                    if (topHole < 1d)
                    {
                        continue;
                    }

                    var alongInset = TumbadoAlongInsetMm(piece.PieceWidthMm);
                    var pieceTop = piece.UpOffsetMm + piece.PieceHeightMm;
                    var heightLimit = wallHeightMm > pieceTop ? wallHeightMm : pieceTop;
                    PlaceRow(
                        outElements, anchor, idWall, moduleBaseAlongMm + piece.AlongOffsetMm,
                        piece.UpOffsetMm, new[] { alongInset, piece.PieceWidthMm - alongInset },
                        new[] { topHole },
                        false, yawRad, ux, uz, faceSign,
                        heightLimit, wallThicknessMm, wallLengthMm, piece.PieceWidthMm);
                    continue;
                }

                var heights = VerticalHeights(piece.PieceHeightMm);
                if (heights.Length == 0)
                {
                    continue;
                }

                var alongs = new List<double> { HoleInsetMm };
                var atFreeEnd = isLast && Math.Abs((piece.AlongOffsetMm + piece.PieceWidthMm) - rightmost) < 1d;
                if (atFreeEnd)
                {
                    alongs.Add(piece.PieceWidthMm - HoleInsetMm);
                }

                PlaceRow(
                    outElements, anchor, idWall, moduleBaseAlongMm + piece.AlongOffsetMm,
                    piece.UpOffsetMm, alongs.ToArray(), heights,
                    false, yawRad, ux, uz, faceSign,
                    wallHeightMm, wallThicknessMm, wallLengthMm, piece.PieceWidthMm);
            }
        }

        private static void PlaceRow(
            List<Atk60ElementPaintItem> outElements,
            Atk60WallPaintAnchor anchor,
            string idWall,
            double alongOriginMm,
            double upOriginMm,
            double[] alongLocal,
            double[] heightsMm,
            bool wingOnLowRow,
            double yawRad,
            double ux,
            double uz,
            double faceSign,
            double wallHeightMm,
            double wallThicknessMm,
            double wallLengthMm,
            double moduleLengthMm)
        {
            foreach (var local in alongLocal)
            {
                if (local < -1d || local > moduleLengthMm + 1d)
                {
                    continue;
                }

                var alongMm = alongOriginMm + local;
                if (alongMm < -1d || alongMm > wallLengthMm + 1d)
                {
                    continue;
                }

                // Menos de 0,30: el cierre lo pone el tablón del extremo.
                // El tirante del panel, a 44 mm del borde, caía en el fenólico.
                if (wallThicknessMm < 300d
                    && (alongMm < 100d || (wallLengthMm > 1d && alongMm > wallLengthMm - 100d)))
                {
                    continue;
                }

                foreach (var height in heightsMm)
                {
                    var wing = wingOnLowRow && height < 50d;
                    var upMm = upOriginMm + height;
                    if (height < 0d || upMm < -1d || upMm > wallHeightMm + 1d)
                    {
                        continue;
                    }

                    var path = wing ? WingPath : PlatePath;
                    var code = wing ? "1920894" : "10443020";
                    var plateUpMm = upMm + (wing ? 10d : 0d);
                    Atk60FaceInsertion.BothFaces(
                        outElements, anchor, idWall, path,
                        alongMm, plateUpMm, yawRad, ux, uz, faceSign,
                        wallHeightMm, wallThicknessMm, wallLengthMm,
                        Atk60FaceInsertion.PanelFaceMm, 0d, code, "zinc");
                    PlaceBar(
                        outElements, anchor, idWall,
                        alongMm, upMm, yawRad, ux, uz, faceSign,
                        wallHeightMm, wallThicknessMm, wallLengthMm);
                    if (!wing)
                    {
                        continue;
                    }

                    Atk60FaceInsertion.BothFaces(
                        outElements, anchor, idWall, NutPath,
                        alongMm, upMm, yawRad, ux, uz, faceSign,
                        wallHeightMm, wallThicknessMm, wallLengthMm,
                        Atk60FaceInsertion.PanelFaceMm, 0d, "7238001", "zinc");
                }
            }
        }

        private static void PlaceBar(
            List<Atk60ElementPaintItem> outElements,
            Atk60WallPaintAnchor anchor,
            string idWall,
            double alongMm,
            double upMm,
            double yawRad,
            double ux,
            double uz,
            double faceSign,
            double wallHeightMm,
            double wallThicknessMm,
            double wallLengthMm)
        {
            var jointX = anchor.X + (ux * alongMm);
            var jointY = anchor.Y + upMm;
            var jointZ = anchor.Z + (uz * alongMm);
            var mid = wallThicknessMm * 0.5d;
            var primaryYaw = -yawRad + (faceSign < 0d ? Math.PI : 0d);
            Atk60FaceInsertion.OneFace(
                outElements, anchor, idWall, BarPath,
                jointX - (anchor.NormalX * mid),
                jointY,
                jointZ - (anchor.NormalZ * mid),
                primaryYaw,
                0d,
                anchor.NormalX,
                anchor.NormalZ,
                faceSign,
                false,
                alongMm,
                upMm,
                wallHeightMm,
                wallThicknessMm,
                wallLengthMm,
                BarCode(wallThicknessMm),
                "zinc");
        }

        private static string BarCode(double wallThicknessMm)
        {
            var span = wallThicknessMm + 240d + 150d + 100d;
            int step;
            if (span > 2400d)
            {
                step = 2500;
            }
            else if (span <= 500d)
            {
                step = 500;
            }
            else
            {
                step = (int)(Math.Ceiling(span / 100d) * 100d);
            }

            return "230" + (step / 10).ToString("000");
        }

        /// <summary>
        /// Misma distancia al extremo que el panel vertical de esa longitud:
        /// 550 mm en 2,40 y 2,70; 350 mm en 1,20.
        /// </summary>
        private static double TumbadoAlongInsetMm(int pieceWidthMm)
        {
            if (pieceWidthMm >= 2000)
            {
                return 550d;
            }

            if (pieceWidthMm >= 1000)
            {
                return 350d;
            }

            return HoleInsetMm;
        }

        private static double[] VerticalHeights(int pieceHeightMm)
        {
            double fromEnd;
            if (pieceHeightMm >= 2000)
            {
                fromEnd = 550d;
            }
            else if (pieceHeightMm >= 1000)
            {
                fromEnd = 350d;
            }
            else
            {
                return new double[0];
            }

            var fromHead = pieceHeightMm - fromEnd;
            if (fromHead <= fromEnd + 1d)
            {
                return new[] { fromEnd };
            }

            return new[] { fromEnd, fromHead };
        }

        private static bool IsTumbado(Modulo270PieceLayout piece)
        {
            return piece != null
                && string.Equals(piece.Orientation, "Tumbado", StringComparison.OrdinalIgnoreCase);
        }

    }
}
