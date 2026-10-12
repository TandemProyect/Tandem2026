using Desing.Repositories.RepositoryAtk60.ModulosATK60;
using Desing.Repositories.RepositoryCommun;
using System.Collections.Generic;

namespace Desing.Repositories.Repository_Desing_2.Atk60
{
    /// <summary>
    /// Rigidizador de la junta horizontal, con su gancho y la placa que lo sigue.
    /// 1850162 (0,75 m) en la junta. 1850163 (1,20 m) cuando el mismo módulo
    /// tiene franja de 0,30 y de 0,45.
    /// </summary>
    internal static class PanelStiffener
    {
        private const string ShortPath = "../../Content/DesignTools/Stl/ATK60/1850162.stl";
        private const string ShortCode = "1850162";
        private const string LongPath = "../../Content/DesignTools/Stl/ATK60/1850163.stl";
        private const string LongCode = "1850163";
        private const string HookPath = "../../Content/DesignTools/Stl/ATK60/1850164.stl";
        private const string HookCode = "1850164";
        private const string FollowerPlatePath = "../../Content/DesignTools/Stl/ATK60/10443020.stl";
        private const string FollowerPlateCode = "10443020";

        internal static void Place(
            List<Atk60ElementPaintItem> outElements,
            List<Modulo270PieceLayout> pieces,
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
            bool showOutside = true,
            bool showInside = true)
        {
            var has300 = false;
            var has450 = false;
            var stripBottom = int.MaxValue;
            var stripTop = 0;
            var upperHeightBySeam = new Dictionary<int, int>();
            for (var i = 0; i < pieces.Count; i++)
            {
                var piece = pieces[i];
                if (piece == null)
                {
                    continue;
                }

                if (piece.PieceHeightMm == 300)
                {
                    has300 = true;
                }

                if (piece.PieceHeightMm == 450)
                {
                    has450 = true;
                }

                if (piece.PieceHeightMm == 300 || piece.PieceHeightMm == 450)
                {
                    if (piece.UpOffsetMm < stripBottom)
                    {
                        stripBottom = piece.UpOffsetMm;
                    }

                    var top = piece.UpOffsetMm + piece.PieceHeightMm;
                    if (top > stripTop)
                    {
                        stripTop = top;
                    }
                }

                if (piece.UpOffsetMm <= 1)
                {
                    continue;
                }

                int known;
                if (!upperHeightBySeam.TryGetValue(piece.UpOffsetMm, out known) || piece.PieceHeightMm < known)
                {
                    upperHeightBySeam[piece.UpOffsetMm] = piece.PieceHeightMm;
                }
            }

            var pair300And450 = has300 && has450;
            var alongLocal = AlongLocals(moduleLengthMm);

            foreach (var seam in upperHeightBySeam)
            {
                var upperMm = seam.Value;
                if (pair300And450 && (upperMm == 300 || upperMm == 450))
                {
                    continue;
                }

                PlaceBars(
                    outElements, anchor, idWall,
                    moduleBaseAlongMm, alongLocal,
                    seam.Key, yawRad, ux, uz, faceSign,
                    wallHeightMm, wallThicknessMm, wallLengthMm,
                    ShortCode, ShortPath, showOutside, showInside);
            }

            if (pair300And450 && stripTop > stripBottom)
            {
                var cover = stripTop - stripBottom;
                var extra = System.Math.Max(0d, 1200d - cover);
                var center = stripBottom + ((cover - extra) * 0.5);
                PlaceBars(
                    outElements, anchor, idWall,
                    moduleBaseAlongMm, alongLocal,
                    center, yawRad, ux, uz, faceSign,
                    wallHeightMm, wallThicknessMm, wallLengthMm,
                    LongCode, LongPath, showOutside, showInside);
            }
        }

        /// <summary>
        /// Puestos del rigidizador a lo largo del módulo.
        /// A partir de 1,50 m: a 300 mm de cada extremo. Si es más corto, al centro.
        /// </summary>
        internal static double[] AlongLocals(double moduleLengthMm)
        {
            if (moduleLengthMm >= 1500d)
            {
                return new[] { 300d, moduleLengthMm - 300d };
            }

            return new[] { moduleLengthMm * 0.5 };
        }

        private static void PlaceBars(
            List<Atk60ElementPaintItem> outElements,
            Atk60WallPaintAnchor anchor,
            string idWall,
            double moduleBaseAlongMm,
            double[] alongLocal,
            double upMm,
            double yawRad,
            double ux,
            double uz,
            double faceSign,
            double wallHeightMm,
            double wallThicknessMm,
            double wallLengthMm,
            string code,
            string path,
            bool showOutside,
            bool showInside)
        {
            for (var ai = 0; ai < alongLocal.Length; ai++)
            {
                var alongMm = moduleBaseAlongMm + alongLocal[ai];
                if (alongMm < 30d || alongMm > wallLengthMm - 30d)
                {
                    continue;
                }

                Atk60FaceInsertion.BothFaces(
                    outElements, anchor, idWall, path,
                    alongMm, upMm, yawRad, ux, uz, faceSign,
                    wallHeightMm, wallThicknessMm, wallLengthMm,
                    Atk60FaceInsertion.PanelFaceMm, 0d, code, "blue",
                    false, showOutside, showInside);

                var nutOffset = code == LongCode ? 300d : 200d;
                var nutHeights = new[] { upMm - nutOffset, upMm + nutOffset };
                var atPanelEnd = alongLocal.Length > 1 && ai == alongLocal.Length - 1;
                for (var ni = 0; ni < nutHeights.Length; ni++)
                {
                    Atk60FaceInsertion.BothFaces(
                        outElements, anchor, idWall, HookPath,
                        alongMm, nutHeights[ni], yawRad, ux, uz, faceSign,
                        wallHeightMm, wallThicknessMm, wallLengthMm,
                        Atk60FaceInsertion.PanelFaceMm, 0d, HookCode, "blue",
                        atPanelEnd, showOutside, showInside);
                    Atk60FaceInsertion.BothFaces(
                        outElements, anchor, idWall, FollowerPlatePath,
                        alongMm, nutHeights[ni], yawRad, ux, uz, faceSign,
                        wallHeightMm, wallThicknessMm, wallLengthMm,
                        Atk60FaceInsertion.PanelFaceMm, 0d, FollowerPlateCode, "zinc",
                        false, showOutside, showInside);
                }
            }
        }
    }
}
