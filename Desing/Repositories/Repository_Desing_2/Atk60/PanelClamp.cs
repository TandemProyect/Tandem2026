using Desing.Repositories.RepositoryAtk60.ModulosATK60;
using Desing.Repositories.RepositoryCommun;
using System;
using System.Collections.Generic;

namespace Desing.Repositories.Repository_Desing_2.Atk60
{
    /// <summary>
    /// Grapa fija en las juntas y grapa regulable en el remate de madera.
    /// </summary>
    internal static class PanelClamp
    {
        private const string FixedPath = "../../Content/DesignTools/Stl/ATK60/10004220.stl";
        private const string FixedCode = "10004220";
        private const string AdjustablePath = "../../Content/DesignTools/Stl/ATK60/10000221.stl";
        private const string AdjustableCode = "10000221";
        private const string AdjustableSlidePath = "../../Content/DesignTools/Stl/ATK60/10000221-2.stl";
        private const string AdjustableSlideCode = "10000221B";

        internal static void PlaceOnModule(
            List<Atk60ElementPaintItem> outElements,
            Modulo270Layout layout,
            Atk60WallPaintAnchor anchor,
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
            double wallLengthMm,
            string idWall,
            bool suppressModuleEndJoint = false)
        {
            var pieces = layout.Pieces;
            for (var pi = 0; pi < pieces.Count; pi++)
            {
                var piece = pieces[pi];
                var tumbado = string.Equals(piece.Orientation, "Tumbado", StringComparison.OrdinalIgnoreCase);
                if (!tumbado)
                {
                    var localJoint = piece.AlongOffsetMm + piece.PieceWidthMm;
                    if (suppressModuleEndJoint && Math.Abs(localJoint - moduleLengthMm) < 25d)
                    {
                        continue;
                    }

                    if (!IsInternalJoint(pieces, pi, localJoint, moduleLengthMm, moduleIndex, countInWall, moduleBaseAlongMm, wallLengthMm))
                    {
                        continue;
                    }

                    var alongMm = moduleBaseAlongMm + localJoint;
                    var heights = VerticalHeightsMm(piece.PieceHeightMm);
                    for (var hi = 0; hi < heights.Length; hi++)
                    {
                        var upMm = piece.UpOffsetMm + heights[hi];
                        Atk60FaceInsertion.BothFaces(
                            outElements, anchor, idWall, FixedPath,
                            alongMm, upMm, yawRad, ux, uz, faceSign,
                            wallHeightMm, wallThicknessMm, wallLengthMm,
                            Atk60FaceInsertion.PanelFaceMm, 0d, FixedCode, "zinc");
                    }
                }
                else
                {
                    if (piece.UpOffsetMm > 1d)
                    {
                        var seamAlong = new[] { 200d, 1350d, 2500d };
                        for (var si = 0; si < seamAlong.Length; si++)
                        {
                            var local = seamAlong[si];
                            if (local < 80d || local > piece.PieceWidthMm - 80d)
                            {
                                continue;
                            }

                            var seamAlongMm = moduleBaseAlongMm + piece.AlongOffsetMm + local;
                            if (seamAlongMm < 30d || seamAlongMm > wallLengthMm - 30d)
                            {
                                continue;
                            }

                            Atk60FaceInsertion.BothFaces(
                                outElements, anchor, idWall, FixedPath,
                                seamAlongMm, piece.UpOffsetMm, yawRad, ux, uz, faceSign,
                                wallHeightMm, wallThicknessMm, wallLengthMm,
                                Atk60FaceInsertion.PanelFaceMm, -Math.PI * 0.5, FixedCode, "zinc");
                        }
                    }

                    var endJoint = piece.AlongOffsetMm + piece.PieceWidthMm;
                    if (suppressModuleEndJoint && Math.Abs(endJoint - moduleLengthMm) < 25d)
                    {
                        continue;
                    }

                    if (IsInternalJoint(pieces, pi, endJoint, moduleLengthMm, moduleIndex, countInWall, moduleBaseAlongMm, wallLengthMm))
                    {
                        var heights = VerticalHeightsMm(piece.PieceHeightMm);
                        for (var hi = 0; hi < heights.Length; hi++)
                        {
                            Atk60FaceInsertion.BothFaces(
                                outElements, anchor, idWall, FixedPath,
                                moduleBaseAlongMm + endJoint,
                                piece.UpOffsetMm + heights[hi],
                                yawRad, ux, uz, faceSign,
                                wallHeightMm, wallThicknessMm, wallLengthMm,
                                Atk60FaceInsertion.PanelFaceMm, 0d, FixedCode, "zinc");
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Grapa fija en la junta del TapeS2. Ese canto queda fuera del muro,
        /// así que la junta entre módulos no la ve PlaceOnModule.
        /// </summary>
        internal static void PlaceBeyondWall(
            List<Atk60ElementPaintItem> outElements,
            Atk60WallPaintAnchor anchor,
            string idWall,
            double alongMm,
            Modulo270Layout layout,
            double yawRad,
            double ux,
            double uz,
            double faceSign,
            double wallHeightMm,
            double wallThicknessMm,
            double wallLengthMm)
        {
            if (layout == null || layout.Pieces == null)
            {
                return;
            }

            for (var pi = 0; pi < layout.Pieces.Count; pi++)
            {
                var piece = layout.Pieces[pi];
                if (string.Equals(piece.Orientation, "Tumbado", StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                var heights = VerticalHeightsMm(piece.PieceHeightMm);
                for (var hi = 0; hi < heights.Length; hi++)
                {
                    Atk60FaceInsertion.BothFaces(
                        outElements, anchor, idWall, FixedPath,
                        alongMm, piece.UpOffsetMm + heights[hi], yawRad, ux, uz, faceSign,
                        wallHeightMm, wallThicknessMm, wallLengthMm,
                        Atk60FaceInsertion.PanelFaceMm, 0d, FixedCode, "zinc");
                }
            }
        }

        /// <summary>
        /// Grapa regulable del remate (10–300 mm): fija al inicio y corredera al final de la madera.
        /// </summary>
        internal static void PlaceOnEndFiller(
            List<Atk60ElementPaintItem> outElements,
            Atk60WallPaintAnchor anchor,
            string idWall,
            double startAlongMm,
            double fillerLengthMm,
            double yawRad,
            double ux,
            double uz,
            double faceSign,
            double wallHeightMm,
            double wallThicknessMm,
            double wallLengthMm)
        {
            if (fillerLengthMm < 10d || fillerLengthMm > 300d)
            {
                return;
            }

            var heights = EndFillerHeightsMm(wallHeightMm);
            var slideAlongMm = startAlongMm + fillerLengthMm;
            for (var hi = 0; hi < heights.Length; hi++)
            {
                Atk60FaceInsertion.BothFaces(
                    outElements, anchor, idWall, AdjustablePath,
                    startAlongMm, heights[hi], yawRad, ux, uz, faceSign,
                    wallHeightMm, wallThicknessMm, wallLengthMm,
                    Atk60FaceInsertion.PanelFaceMm, 0d, AdjustableCode, "zinc");
                Atk60FaceInsertion.BothFaces(
                    outElements, anchor, idWall, AdjustableSlidePath,
                    slideAlongMm, heights[hi], yawRad, ux, uz, faceSign,
                    wallHeightMm, wallThicknessMm, wallLengthMm,
                    Atk60FaceInsertion.PanelFaceMm, 0d, AdjustableSlideCode, "zinc");
            }
        }

        private static bool IsInternalJoint(
            List<Modulo270PieceLayout> pieces,
            int pieceIndex,
            double localJointMm,
            double moduleLengthMm,
            int moduleIndex,
            int countInWall,
            double moduleBaseAlongMm,
            double wallLengthMm)
        {
            var alongMm = moduleBaseAlongMm + localJointMm;
            if (alongMm < 30d || alongMm > wallLengthMm - 30d)
            {
                return false;
            }

            for (var i = 0; i < pieces.Count; i++)
            {
                if (i == pieceIndex)
                {
                    continue;
                }

                if (Math.Abs(pieces[i].AlongOffsetMm - localJointMm) < 25d)
                {
                    return true;
                }
            }

            return moduleIndex < countInWall && Math.Abs(localJointMm - moduleLengthMm) < 25d;
        }

        private static double[] VerticalHeightsMm(int pieceHeightMm)
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

        private static double[] EndFillerHeightsMm(double wallHeightMm)
        {
            var height = (int)Math.Round(wallHeightMm);
            if (height < 200)
            {
                return new double[0];
            }

            var found = new List<double>();
            var n = (height + 299) / 2700;
            var rest = height - (2700 * n);
            var baseMm = 0;
            for (var i = 0; i < n; i++)
            {
                AddHeight(found, baseMm + 450d, height);
                AddHeight(found, baseMm + 1350d, height);
                AddHeight(found, baseMm + 2250d, height);
                baseMm += 2700;
            }

            if (rest > 0 && rest <= 1200)
            {
                AddHeight(found, baseMm + 200d, height);
                AddHeight(found, baseMm + 700d, height);
            }
            else if (rest > 1200 && rest <= 2400)
            {
                AddHeight(found, baseMm + 450d, height);
                AddHeight(found, baseMm + 1900d, height);
            }

            return found.ToArray();
        }

        private static void AddHeight(List<double> found, double upMm, int wallHeightMm)
        {
            if (upMm >= 10d && upMm <= wallHeightMm)
            {
                found.Add(upMm);
            }
        }
    }
}
