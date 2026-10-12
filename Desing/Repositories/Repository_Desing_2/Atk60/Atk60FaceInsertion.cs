using Desing.Repositories.RepositoryAtk60;
using Desing.Repositories.RepositoryCommun;
using System;
using System.Collections.Generic;

namespace Desing.Repositories.Repository_Desing_2.Atk60
{
    /// <summary>
    /// Punto de inserción de un artículo: coordenada del muro, su rotación
    /// y 120 mm hacia el exterior. El origen del STL es ese punto.
    /// </summary>
    internal static class Atk60FaceInsertion
    {
        internal const double PanelFaceMm = 120d;

        internal static void BothFaces(
            List<Atk60ElementPaintItem> outElements,
            Atk60WallPaintAnchor anchor,
            string idWall,
            string unionPath,
            double alongMm,
            double upMm,
            double yawRad,
            double ux,
            double uz,
            double faceSign,
            double wallHeightMm,
            double wallThicknessMm,
            double wallLengthMm,
            double faceGapMm,
            double rotZ,
            string elementCode,
            string color,
            bool atPanelEnd = false,
            bool showPrimary = true,
            bool showOpposite = true)
        {
            var jointX = anchor.X + (ux * alongMm);
            var jointY = anchor.Y + upMm;
            var jointZ = anchor.Z + (uz * alongMm);
            var primaryYaw = -yawRad + (faceSign < 0d ? Math.PI : 0d);
            if (showPrimary)
            {
                OneFace(
                outElements, anchor, idWall, unionPath,
                jointX + (anchor.NormalX * faceGapMm),
                jointY,
                jointZ + (anchor.NormalZ * faceGapMm),
                primaryYaw,
                rotZ,
                anchor.NormalX,
                anchor.NormalZ,
                faceSign,
                false,
                alongMm,
                upMm,
                wallHeightMm,
                wallThicknessMm,
                wallLengthMm,
                elementCode,
                color,
                atPanelEnd);
            }

            if (!showOpposite)
            {
                return;
            }

            var oppositeX = jointX - (anchor.NormalX * wallThicknessMm);
            var oppositeZ = jointZ - (anchor.NormalZ * wallThicknessMm);
            OneFace(
                outElements, anchor, idWall, unionPath,
                oppositeX - (anchor.NormalX * faceGapMm),
                jointY,
                oppositeZ - (anchor.NormalZ * faceGapMm),
                primaryYaw + Math.PI,
                rotZ,
                -anchor.NormalX,
                -anchor.NormalZ,
                -faceSign,
                true,
                alongMm,
                upMm,
                wallHeightMm,
                wallThicknessMm,
                wallLengthMm,
                elementCode,
                color,
                atPanelEnd);
        }

        internal static void OneFace(
            List<Atk60ElementPaintItem> outElements,
            Atk60WallPaintAnchor anchor,
            string idWall,
            string unionPath,
            double x,
            double y,
            double z,
            double rotY,
            double rotZ,
            double normalX,
            double normalZ,
            double faceSign,
            bool mirrored,
            double alongMm,
            double upMm,
            double wallHeightMm,
            double wallThicknessMm,
            double wallLengthMm,
            string elementCode,
            string color,
            bool atPanelEnd = false)
        {
            outElements.Add(new Atk60ElementPaintItem
            {
                IdWall = !string.IsNullOrWhiteSpace(idWall) ? idWall : anchor.IdWall,
                ElementType = "Union",
                ElementCode = elementCode,
                Orientation = Math.Abs(rotZ) > 0.01 ? "Horizontal" : "Vertical",
                ImportPath = unionPath,
                Color = color,
                X = x,
                Y = y,
                Z = z,
                RotX = 0,
                RotY = rotY,
                RotZ = rotZ,
                NormalX = normalX,
                NormalZ = normalZ,
                FaceSign = faceSign,
                IsMirrored = mirrored,
                WallHeightMm = wallHeightMm,
                WallThicknessMm = wallThicknessMm,
                WallLengthMm = wallLengthMm,
                PieceWidthMm = 195,
                PieceHeightMm = 104,
                LocalAlongMm = alongMm,
                LocalUpMm = upMm,
                BaseRotZ = atPanelEnd ? 1d : 0d,
                UseStrictPose = true,
            });
        }

        internal static double ResolveWallHeightMm(AttributesList attrs)
        {
            var h = ToSceneMm(attrs != null ? (attrs._DataHeight ?? attrs.ExtraValueAsDouble("_DataHeight")) : null);
            return h > 1d ? h : 2700d;
        }

        internal static double ResolveWallThicknessMm(AttributesList attrs)
        {
            var t = ToSceneMm(attrs != null ? (attrs._DataWith ?? attrs.ExtraValueAsDouble("_DataWith")) : null);
            return t > 1d ? Math.Abs(t) : 300d;
        }

        internal static double ResolveWallLengthMm(AttributesList attrs)
        {
            var l = ToSceneMm(attrs != null ? (attrs._Datalong ?? attrs.ExtraValueAsDouble("_Datalong")) : null);
            return l > 1d ? Math.Abs(l) : 0d;
        }

        /// <summary>Un check ausente cuenta como marcado.</summary>
        internal static bool FlagOn(bool? flag)
        {
            return !flag.HasValue || flag.Value;
        }

        /// <summary>
        /// Panel de 0,75 m regulable. Si el muro no lo pide, el hueco se parte en 0,30 + 0,45.
        /// </summary>
        internal static bool UseAdjustable750(AttributesList attrs)
        {
            if (attrs == null)
            {
                return true;
            }

            if (attrs._CHeck750R.HasValue)
            {
                return attrs._CHeck750R.Value;
            }

            return FlagOn(attrs._IsUniversalPanel);
        }

        internal static double NormalizeYawToRad(double yaw)
        {
            var abs = Math.Abs(yaw);
            return abs > (Math.PI * 2 + 1e-6)
                ? (yaw * Math.PI / 180d)
                : yaw;
        }

        private static double ToSceneMm(double? value)
        {
            if (!value.HasValue)
            {
                return 0d;
            }

            var abs = Math.Abs(value.Value);
            return abs <= 50d ? value.Value * 1000d : value.Value;
        }
    }
}
