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
            int moduleCountInWall,
            bool suppressLastEndJoint = false)
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
                var moduleIndex = moduleIndexOffset + i + 1;
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

                AddPanelJointUnions(
                    outElements,
                    layout,
                    anchor,
                    moduleBaseAlongMm,
                    moduleLengthMm,
                    moduleIndex,
                    countInWall,
                    yawRad,
                    ux,
                    uz,
                    faceSign,
                    wallHeightMm,
                    wallThicknessMm,
                    wallLengthMm,
                    idWall,
                    suppressLastEndJoint && i == moduleCount - 1);
            }

            return outElements;
        }

        /// <summary>
        /// Grapa fija (UnionHorizonal / 10004220) en las juntas.
        /// Panel vertical: en la junta vertical, a las alturas de esa familia.
        /// Panel tumbado: en la junta horizontal, girada, y en el extremo si sigue otro módulo.
        /// Las dos caras del muro.
        /// </summary>
        private static void AddPanelJointUnions(
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
            const double faceGapMm = 135d;
            const string unionPath = "../../Content/DesignTools/Stl/ATK60/10004220.stl";
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
                    var heights = VerticalUnionHeightsMm(piece.PieceHeightMm);
                    for (var hi = 0; hi < heights.Length; hi++)
                    {
                        var upMm = piece.UpOffsetMm + heights[hi];
                        AddUnionBothFaces(
                            outElements, anchor, idWall, unionPath,
                            alongMm, upMm, yawRad, ux, uz, faceSign,
                            wallHeightMm, wallThicknessMm, wallLengthMm,
                            faceGapMm, 0d, "10004220", "zinc");
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

                            AddUnionBothFaces(
                                outElements, anchor, idWall, unionPath,
                                seamAlongMm, piece.UpOffsetMm, yawRad, ux, uz, faceSign,
                                wallHeightMm, wallThicknessMm, wallLengthMm,
                                faceGapMm, -Math.PI * 0.5, "10004220", "zinc");
                        }
                    }

                    var endJoint = piece.AlongOffsetMm + piece.PieceWidthMm;
                    if (suppressModuleEndJoint && Math.Abs(endJoint - moduleLengthMm) < 25d)
                    {
                        // El extremo da al remate: ahí va la grapa regulable.
                    }
                    else if (IsInternalJoint(pieces, pi, endJoint, moduleLengthMm, moduleIndex, countInWall, moduleBaseAlongMm, wallLengthMm))
                    {
                        var heights = VerticalUnionHeightsMm(piece.PieceHeightMm);
                        for (var hi = 0; hi < heights.Length; hi++)
                        {
                            AddUnionBothFaces(
                                outElements, anchor, idWall, unionPath,
                                moduleBaseAlongMm + endJoint,
                                piece.UpOffsetMm + heights[hi],
                                yawRad, ux, uz, faceSign,
                                wallHeightMm, wallThicknessMm, wallLengthMm,
                                faceGapMm, 0d, "10004220", "zinc");
                        }
                    }
                }
            }

            AddVerticalCourseUnions(
                outElements, pieces, anchor, idWall,
                moduleBaseAlongMm, moduleLengthMm,
                yawRad, ux, uz, faceSign,
                wallHeightMm, wallThicknessMm, wallLengthMm, faceGapMm);
        }

        /// <summary>
        /// Rigidizador de la junta horizontal.
        /// 1850162 (0,75 m) en la junta, también en el muro de 3,00 m (2,70 + 0,30).
        /// 1850163 (1,20 m) solo si en el mismo módulo hay franja de 0,30 y de 0,45
        /// (por ejemplo 2,70 + 0,45 + 0,30): una barra cubre las dos.
        /// </summary>
        private static void AddVerticalCourseUnions(
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
            double faceGapMm)
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

            double[] alongLocal;
            if (moduleLengthMm >= 1500d)
            {
                alongLocal = new[] { 300d, moduleLengthMm - 300d };
            }
            else
            {
                alongLocal = new[] { moduleLengthMm * 0.5 };
            }

            foreach (var seam in upperHeightBySeam)
            {
                var upperMm = seam.Value;
                if (pair300And450 && (upperMm == 300 || upperMm == 450))
                {
                    continue;
                }

                PlaceCourseBars(
                    outElements, anchor, idWall,
                    moduleBaseAlongMm, alongLocal,
                    seam.Key, yawRad, ux, uz, faceSign,
                    wallHeightMm, wallThicknessMm, wallLengthMm, faceGapMm,
                    "1850162",
                    "../../Content/DesignTools/Stl/ATK60/1850162.stl");
            }

            if (pair300And450 && stripTop > stripBottom)
            {
                var cover = stripTop - stripBottom;
                var extra = Math.Max(0d, 1200d - cover);
                var center = stripBottom + ((cover - extra) * 0.5);
                PlaceCourseBars(
                    outElements, anchor, idWall,
                    moduleBaseAlongMm, alongLocal,
                    center, yawRad, ux, uz, faceSign,
                    wallHeightMm, wallThicknessMm, wallLengthMm, faceGapMm,
                    "1850163",
                    "../../Content/DesignTools/Stl/ATK60/1850163.stl");
            }
        }

        private static void PlaceCourseBars(
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
            double faceGapMm,
            string code,
            string path)
        {
            for (var ai = 0; ai < alongLocal.Length; ai++)
            {
                var alongMm = moduleBaseAlongMm + alongLocal[ai];
                if (alongMm < 30d || alongMm > wallLengthMm - 30d)
                {
                    continue;
                }

                // La piel interior de la barra está a 20 mm. La cara del panel está a 120 mm.
                const double barOnPanelMm = 100d;
                AddUnionBothFaces(
                    outElements, anchor, idWall, path,
                    alongMm, upMm, yawRad, ux, uz, faceSign,
                    wallHeightMm, wallThicknessMm, wallLengthMm,
                    barOnPanelMm, 0d, code, "blue");

                // La placa va con la barra: apoya en su cara exterior, a 80 mm.
                var nutOffset = code == "1850163" ? 300d : 200d;
                const double barOuterFaceMm = 80d;
                const double hookEyeMm = 21d;
                const double panelFaceInsetMm = 15d;
                var nutHeights = new[] { upMm - nutOffset, upMm + nutOffset };
                var atPanelEnd = alongLocal.Length > 1 && ai == alongLocal.Length - 1;
                for (var ni = 0; ni < nutHeights.Length; ni++)
                {
                    AddUnionBothFaces(
                        outElements, anchor, idWall,
                        "../../Content/DesignTools/Stl/ATK60/1850164.stl",
                        alongMm, nutHeights[ni], yawRad, ux, uz, faceSign,
                        wallHeightMm, wallThicknessMm, wallLengthMm,
                        faceGapMm - panelFaceInsetMm + hookEyeMm, 0d, "1850164", "blue",
                        atPanelEnd);
                    AddUnionBothFaces(
                        outElements, anchor, idWall,
                        "../../Content/DesignTools/Stl/ATK60/10443020.stl",
                        alongMm, nutHeights[ni], yawRad, ux, uz, faceSign,
                        wallHeightMm, wallThicknessMm, wallLengthMm,
                        barOnPanelMm + barOuterFaceMm, 0d, "10443020", "zinc");
                }
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

        private static double[] VerticalUnionHeightsMm(int pieceHeightMm)
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

        private static void AddUnionBothFaces(
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
            bool atPanelEnd = false)
        {
            var jointX = anchor.X + (ux * alongMm);
            var jointY = anchor.Y + upMm;
            var jointZ = anchor.Z + (uz * alongMm);
            var primaryYaw = -yawRad + (faceSign < 0d ? Math.PI : 0d);
            AddUnionFace(
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

            var oppositeX = jointX - (anchor.NormalX * wallThicknessMm);
            var oppositeZ = jointZ - (anchor.NormalZ * wallThicknessMm);
            AddUnionFace(
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

        private static void AddUnionFace(
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

            AddRegulableRemateUnions(
                outElements, anchor, idWall,
                startAlongMm, remateLengthMm,
                yawRad, ux, uz, faceSign,
                wallHeightMm, wallThicknessMm, wallLengthMm);

            return outElements;
        }

        /// <summary>
        /// Grapa regulable del remate de madera (10–300 mm).
        /// Dos piezas: 10000221 fija en el inicio del remate y 10000221B
        /// desplazada la longitud de la madera. Las dos caras.
        /// Alturas como el remate antiguo: cada tramo de 2,70 m a 450, 1350 y 2250;
        /// resto de 2,40 a 450 y 1900; resto de hasta 1,20 a 200 y 700.
        /// </summary>
        private static void AddRegulableRemateUnions(
            List<Atk60ElementPaintItem> outElements,
            Atk60WallPaintAnchor anchor,
            string idWall,
            double startAlongMm,
            double remateLengthMm,
            double yawRad,
            double ux,
            double uz,
            double faceSign,
            double wallHeightMm,
            double wallThicknessMm,
            double wallLengthMm)
        {
            if (remateLengthMm < 10d || remateLengthMm > 300d)
            {
                return;
            }

            const double faceGapMm = 135d;
            const string fixedPath = "../../Content/DesignTools/Stl/ATK60/10000221.stl";
            const string slidePath = "../../Content/DesignTools/Stl/ATK60/10000221-2.stl";
            var heights = RegulableRemateHeightsMm(wallHeightMm);
            var slideAlongMm = startAlongMm + remateLengthMm;
            for (var hi = 0; hi < heights.Length; hi++)
            {
                AddUnionBothFaces(
                    outElements, anchor, idWall, fixedPath,
                    startAlongMm, heights[hi], yawRad, ux, uz, faceSign,
                    wallHeightMm, wallThicknessMm, wallLengthMm,
                    faceGapMm, 0d, "10000221", "zinc");
                AddUnionBothFaces(
                    outElements, anchor, idWall, slidePath,
                    slideAlongMm, heights[hi], yawRad, ux, uz, faceSign,
                    wallHeightMm, wallThicknessMm, wallLengthMm,
                    faceGapMm, 0d, "10000221B", "zinc");
            }
        }

        private static double[] RegulableRemateHeightsMm(double wallHeightMm)
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
                AddRemateHeight(found, baseMm + 450d, height);
                AddRemateHeight(found, baseMm + 1350d, height);
                AddRemateHeight(found, baseMm + 2250d, height);
                baseMm += 2700;
            }

            if (rest > 0 && rest <= 1200)
            {
                AddRemateHeight(found, baseMm + 200d, height);
                AddRemateHeight(found, baseMm + 700d, height);
            }
            else if (rest > 1200 && rest <= 2400)
            {
                AddRemateHeight(found, baseMm + 450d, height);
                AddRemateHeight(found, baseMm + 1900d, height);
            }

            return found.ToArray();
        }

        private static void AddRemateHeight(List<double> found, double upMm, int wallHeightMm)
        {
            if (upMm >= 10d && upMm <= wallHeightMm)
            {
                found.Add(upMm);
            }
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
