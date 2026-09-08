using Desing.Repositories.RepositoryCommun;
using System;
using System.Collections.Generic;

namespace Desing.Repositories.RepositoryAtk60.ModulosATK60
{
    internal static class Modulo270PanelElementGenerator
    {
        private const double ModuleLengthMm = 2700d;
        private const double RemateDepthMm = 120d;

        public static List<Atk60ElementPaintItem> Build(
            Desing2FormworkWallDto wall,
            Atk60WallPaintAnchor anchor,
            long module270Count)
        {
            return Build(wall, anchor, module270Count, ModuleLengthMm, 0d, 0, (int)module270Count);
        }

        public static List<Atk60ElementPaintItem> Build(
            Desing2FormworkWallDto wall,
            Atk60WallPaintAnchor anchor,
            long moduleCount,
            double moduleLengthMm,
            double startAlongMm,
            int moduleIndexOffset,
            int moduleCountInWall)
        {
            var outElements = new List<Atk60ElementPaintItem>();
            if (wall == null || anchor == null || moduleCount <= 0 || moduleLengthMm < 299d)
            {
                return outElements;
            }

            var attrs = wall.Attributes;
            var yawRad = NormalizeYawToRad(anchor.RotY);
            var renderYawRad = -yawRad;
            var ux = Math.Cos(yawRad);
            var uz = Math.Sin(yawRad);
            var faceSign = anchor.FaceSign >= 0d ? 1d : -1d;

            var wallHeightMm = ResolveWallHeightMm(attrs);
            var wallThicknessMm = ResolveWallThicknessMm(attrs);
            var wallLengthMm = ResolveWallLengthMm(attrs);
            var layout = Modulo270HeightPanelCatalog.ResolveForModule(wallHeightMm, (int)Math.Round(moduleLengthMm));
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
                for (var pi = 0; pi < layout.Pieces.Count; pi++)
                {
                    var piece = layout.Pieces[pi];
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
                    var moduleIndex = moduleIndexOffset + i + 1;

                    outElements.Add(new Atk60ElementPaintItem
                    {
                        IdWall = !string.IsNullOrWhiteSpace(idWall) ? idWall : anchor.IdWall,
                        ElementType = "Panel",
                        ElementCode = piece.ElementCode,
                        Orientation = piece.Orientation,
                        ImportPath = piece.ImportPath,
                        Color = "frame-yellow",
                        X = primaryX,
                        Y = baseY,
                        Z = primaryZ,
                        RotX = anchor.RotX,
                        RotY = primaryRotY,
                        RotZ = anchor.RotZ,
                        NormalX = anchor.NormalX,
                        NormalZ = anchor.NormalZ,
                        FaceSign = anchor.FaceSign,
                        IsMirrored = false,
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
                        PieceIndexInModule = pi + 1,
                        PieceCountInModule = layout.Pieces.Count,
                        CatalogHeightMm = layout.CatalogHeightMm,
                    });

                    // Cara simetrica: mismo panel en la cara opuesta del muro.
                    // Se traslada un espesor de muro y se rota 180 grados.
                    // Al rotar sobre el punto inferior-izquierdo, el punto de insercion debe avanzar una anchura de pieza.
                    outElements.Add(new Atk60ElementPaintItem
                    {
                        IdWall = !string.IsNullOrWhiteSpace(idWall) ? idWall : anchor.IdWall,
                        ElementType = "Panel",
                        ElementCode = piece.ElementCode,
                        Orientation = piece.Orientation,
                        ImportPath = piece.ImportPath,
                        Color = "frame-yellow",
                        X = oppositeX,
                        Y = baseY,
                        Z = oppositeZ,
                        RotX = anchor.RotX,
                        RotY = oppositeRotY,
                        RotZ = anchor.RotZ,
                        NormalX = -anchor.NormalX,
                        NormalZ = -anchor.NormalZ,
                        FaceSign = -anchor.FaceSign,
                        IsMirrored = true,
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
                        PieceIndexInModule = pi + 1,
                        PieceCountInModule = layout.Pieces.Count,
                        CatalogHeightMm = layout.CatalogHeightMm,
                    });
                }
            }

            return outElements;
        }

        public static List<Atk60ElementPaintItem> BuildRemate(
            Desing2FormworkWallDto wall,
            Atk60WallPaintAnchor anchor,
            double remateLengthMm,
            double startAlongMm)
        {
            var outElements = new List<Atk60ElementPaintItem>();
            if (wall == null || anchor == null || remateLengthMm <= 1d)
            {
                return outElements;
            }

            var attrs = wall.Attributes;
            var yawRad = NormalizeYawToRad(anchor.RotY);
            var renderYawRad = -yawRad;
            var ux = Math.Cos(yawRad);
            var uz = Math.Sin(yawRad);
            var faceSign = anchor.FaceSign >= 0d ? 1d : -1d;
            var faceIsRightSide = faceSign < 0d;
            var wallHeightMm = ResolveWallHeightMm(attrs);
            var wallThicknessMm = ResolveWallThicknessMm(attrs);
            var wallLengthMm = ResolveWallLengthMm(attrs);
            var idWall = !string.IsNullOrWhiteSpace(wall.WallId)
                ? wall.WallId
                : (!string.IsNullOrWhiteSpace(wall.Id) ? wall.Id : wall.LineId);

            var baseX = anchor.X + (ux * startAlongMm);
            var baseY = anchor.Y;
            var baseZ = anchor.Z + (uz * startAlongMm);
            var primaryX = baseX + (faceIsRightSide ? ux * remateLengthMm : 0d);
            var primaryZ = baseZ + (faceIsRightSide ? uz * remateLengthMm : 0d);
            var primaryRotY = renderYawRad + (faceIsRightSide ? Math.PI : 0d);
            var oppositeBaseX = baseX - (anchor.NormalX * wallThicknessMm);
            var oppositeBaseZ = baseZ - (anchor.NormalZ * wallThicknessMm);
            var oppositeX = oppositeBaseX + (!faceIsRightSide ? ux * remateLengthMm : 0d);
            var oppositeZ = oppositeBaseZ + (!faceIsRightSide ? uz * remateLengthMm : 0d);
            var oppositeRotY = renderYawRad + (!faceIsRightSide ? Math.PI : 0d);

            outElements.Add(CreateRemateItem(wall, anchor, idWall, primaryX, baseY, primaryZ, primaryRotY, false, remateLengthMm, wallHeightMm, wallThicknessMm, wallLengthMm, startAlongMm));
            outElements.Add(CreateRemateItem(wall, anchor, idWall, oppositeX, baseY, oppositeZ, oppositeRotY, true, remateLengthMm, wallHeightMm, wallThicknessMm, wallLengthMm, startAlongMm));

            return outElements;
        }

        private static Atk60ElementPaintItem CreateRemateItem(
            Desing2FormworkWallDto wall,
            Atk60WallPaintAnchor anchor,
            string idWall,
            double x,
            double y,
            double z,
            double rotY,
            bool isMirrored,
            double remateLengthMm,
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
                ModuleLengthMm = remateLengthMm,
                ModuleIndex = 1,
                ModuleCountInWall = 1,
                WallHeightMm = wallHeightMm,
                WallThicknessMm = RemateDepthMm,
                WallLengthMm = wallLengthMm,
                PieceWidthMm = remateLengthMm,
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
