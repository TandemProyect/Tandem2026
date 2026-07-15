using Desing.Repositories.RepositoryCommun;
using System;
using System.Collections.Generic;

namespace Desing.Repositories.RepositoryAtk60.ModulosATK60
{
    internal static class Modulo270PanelElementGenerator
    {
        private const double ModuleLengthMm = 2700d;

        public static List<Atk60ElementPaintItem> Build(
            Desing2FormworkWallDto wall,
            Atk60WallPaintAnchor anchor,
            long module270Count)
        {
            var outElements = new List<Atk60ElementPaintItem>();
            if (wall == null || anchor == null || module270Count <= 0)
            {
                return outElements;
            }

            var attrs = wall.Attributes;
            var yawRad = NormalizeYawToRad(anchor.RotY);
            var ux = Math.Cos(yawRad);
            var uz = Math.Sin(yawRad);

            var wallHeightMm = ResolveWallHeightMm(attrs);
            var wallThicknessMm = ResolveWallThicknessMm(attrs);
            var wallLengthMm = ResolveWallLengthMm(attrs);
            var layout = Modulo270HeightPanelCatalog.Resolve(wallHeightMm);
            var idWall = !string.IsNullOrWhiteSpace(wall.WallId)
                ? wall.WallId
                : (!string.IsNullOrWhiteSpace(wall.Id) ? wall.Id : wall.LineId);

            if (layout == null || layout.Pieces == null || layout.Pieces.Count == 0)
            {
                return outElements;
            }

            for (var i = 0; i < module270Count; i++)
            {
                var moduleBaseAlongMm = i * ModuleLengthMm;
                for (var pi = 0; pi < layout.Pieces.Count; pi++)
                {
                    var piece = layout.Pieces[pi];
                    var alongMm = moduleBaseAlongMm + piece.AlongOffsetMm;
                    var baseX = anchor.X + (ux * alongMm);
                    var baseY = anchor.Y + piece.UpOffsetMm;
                    var baseZ = anchor.Z + (uz * alongMm);

                    outElements.Add(new Atk60ElementPaintItem
                    {
                        IdWall = !string.IsNullOrWhiteSpace(idWall) ? idWall : anchor.IdWall,
                        ElementType = "Panel",
                        ElementCode = piece.ElementCode,
                        Orientation = piece.Orientation,
                        ImportPath = piece.ImportPath,
                        Color = "frame-yellow",
                        X = baseX,
                        Y = baseY,
                        Z = baseZ,
                        RotX = anchor.RotX,
                        RotY = yawRad,
                        RotZ = anchor.RotZ,
                        NormalX = anchor.NormalX,
                        NormalZ = anchor.NormalZ,
                        FaceSign = anchor.FaceSign,
                        ModuleLengthMm = ModuleLengthMm,
                        ModuleIndex = i + 1,
                        ModuleCountInWall = (int)module270Count,
                        WallHeightMm = wallHeightMm,
                        WallThicknessMm = wallThicknessMm,
                        WallLengthMm = wallLengthMm,
                        PieceWidthMm = piece.PieceWidthMm,
                        PieceHeightMm = piece.PieceHeightMm,
                        LocalAlongMm = alongMm,
                        LocalUpMm = piece.UpOffsetMm,
                        PieceIndexInModule = pi + 1,
                        PieceCountInModule = layout.Pieces.Count,
                        CatalogHeightMm = layout.CatalogHeightMm,
                    });

                    // Cara simetrica: mismo panel en la cara opuesta del muro.
                    // Se traslada un espesor de muro y se invierte normal/signo para que JS pinte "hacia fuera" en ambos lados.
                    outElements.Add(new Atk60ElementPaintItem
                    {
                        IdWall = !string.IsNullOrWhiteSpace(idWall) ? idWall : anchor.IdWall,
                        ElementType = "Panel",
                        ElementCode = piece.ElementCode,
                        Orientation = piece.Orientation,
                        ImportPath = piece.ImportPath,
                        Color = "frame-yellow",
                        X = baseX - (anchor.NormalX * wallThicknessMm),
                        Y = baseY,
                        Z = baseZ - (anchor.NormalZ * wallThicknessMm),
                        RotX = anchor.RotX,
                        RotY = yawRad,
                        RotZ = anchor.RotZ,
                        NormalX = -anchor.NormalX,
                        NormalZ = -anchor.NormalZ,
                        FaceSign = -anchor.FaceSign,
                        ModuleLengthMm = ModuleLengthMm,
                        ModuleIndex = i + 1,
                        ModuleCountInWall = (int)module270Count,
                        WallHeightMm = wallHeightMm,
                        WallThicknessMm = wallThicknessMm,
                        WallLengthMm = wallLengthMm,
                        PieceWidthMm = piece.PieceWidthMm,
                        PieceHeightMm = piece.PieceHeightMm,
                        LocalAlongMm = alongMm,
                        LocalUpMm = piece.UpOffsetMm,
                        PieceIndexInModule = pi + 1,
                        PieceCountInModule = layout.Pieces.Count,
                        CatalogHeightMm = layout.CatalogHeightMm,
                    });
                }
            }

            return outElements;
        }

        private static double ResolveWallHeightMm(AttributesList attrs)
        {
            var h = ToSceneMm(attrs != null ? (attrs._DataHeight ?? attrs.ExtraValueAsDouble("_DataHeight")) : null);
            return h > 1d ? h : 2700d;
        }

        private static double ResolveWallThicknessMm(AttributesList attrs)
        {
            var t = ToSceneMm(attrs != null ? (attrs._DataWith ?? attrs.ExtraValueAsDouble("_DataWith")) : null);
            return t > 1d ? Math.Abs(t) : 300d;
        }

        private static double ResolveWallLengthMm(AttributesList attrs)
        {
            var l = ToSceneMm(attrs != null ? (attrs._Datalong ?? attrs.ExtraValueAsDouble("_Datalong")) : null);
            return l > 1d ? Math.Abs(l) : 0d;
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

        private static double NormalizeYawToRad(double yaw)
        {
            var abs = Math.Abs(yaw);
            return abs > (Math.PI * 2 + 1e-6)
                ? (yaw * Math.PI / 180d)
                : yaw;
        }
    }
}
