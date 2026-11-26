using DAL;
using Desing.Helpers;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace Desing.Repositories.RepositoryDesing2
{
    public sealed class DesignAssemblyViewSummary
    {
        public int FaceId { get; set; }
        public string FaceName { get; set; }
        public string WallId { get; set; }
        public double WallAngleDeg { get; set; }
        public int ElementCount { get; set; }
    }

    public sealed class DesignAssemblyViewsResult
    {
        public int Pieces { get; set; }
        public int Stamped { get; set; }
        public int WithoutFace { get; set; }
        /// <summary>Piezas guardadas que no están sobre ningún muro actual (encofrado antiguo): no entran en las vistas.</summary>
        public int Outside { get; set; }
        /// <summary>Sin cambios en muros ni piezas desde la última vez: se devuelven las vistas guardadas.</summary>
        public bool Reused { get; set; }
        public int Elements { get; set; }
        public List<DesignAssemblyViewSummary> Views { get; set; }
    }

    /// <summary>Vista guardada con sus piezas, para el visor de vistas (Desing_2/AssemblyView).</summary>
    public sealed class DesignAssemblyViewDto
    {
        public int FaceId { get; set; }
        public string FaceName { get; set; }
        public string FaceSide { get; set; }
        public string WallId { get; set; }
        public long WallDbId { get; set; }
        public double WallAngleDeg { get; set; }
        public double FaceNormalDeg { get; set; }
        public double P1X { get; set; }
        public double P1Z { get; set; }
        public double P2X { get; set; }
        public double P2Z { get; set; }
        public double BaseY { get; set; }
        public double LengthMm { get; set; }
        public double HeightMm { get; set; }
        /// <summary>Derecha de quien mira la cara de frente (P1 → P2), en planta de escena.</summary>
        public double UX { get; set; }
        public double UZ { get; set; }
        /// <summary>Normal de la cara (hacia quien la mira).</summary>
        public double NX { get; set; }
        public double NZ { get; set; }
        public List<DesignAssemblyViewElementDto> Elements { get; set; }
    }

    public sealed class DesignAssemblyViewElementDto
    {
        public long? ArticleId { get; set; }
        public long? MasterArticleId { get; set; }
        public string Code { get; set; }
        public string Name { get; set; }
        public long Sequence { get; set; }
        public double X { get; set; }
        public double Y { get; set; }
        public double Z { get; set; }
        public double RotationX { get; set; }
        public double RotationY { get; set; }
        public double RotationZ { get; set; }
        public double LocalX { get; set; }
        public double LocalY { get; set; }
        public double Depth { get; set; }
        public bool Through { get; set; }
        public string StlUrl { get; set; }
        public string StlPhenolicUrl { get; set; }
        /// <summary>Panel o esquina de encofrado: entra en las cotas y en el modo colores.</summary>
        public bool IsPanel { get; set; }
        /// <summary>Remate (tablón) entre paneles: entra en la cadena de cotas.</summary>
        public bool IsFiller { get; set; }
        /// <summary>Panel tumbado (suplemento): no se pinta en el modo colores.</summary>
        public bool Lying { get; set; }
        /// <summary>Ancho y alto del maestro (mm); 0 si no hay maestro.</summary>
        public double NominalWidthMm { get; set; }
        public double NominalHeightMm { get; set; }
        /// <summary>Color del maestro (#RRGGBB) para el modo colores; null si no tiene.</summary>
        public string Color { get; set; }
    }

    /// <summary>Clasifica las piezas de una vista para las cotas y el modo colores del visor de vistas.</summary>
    public static class DesignAssemblyPieceKind
    {
        private static readonly Regex PanelCode = new Regex(@"^E?(?:12|24|27|3120)\d{4,}$", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);

        /// <summary>
        /// Panel (o esquina) de encofrado: el maestro trae ancho y alto de panel en metros. Sin maestro, por el código.
        /// Angulares, barras y grapas tienen ancho/alto 0 o fuera de rango y no cuentan.
        /// </summary>
        public static bool IsPanel(string code, Tsql_Master_Articles master, out double nominalWidthMm, out double nominalHeightMm)
        {
            nominalWidthMm = 0d;
            nominalHeightMm = 0d;
            if (master != null)
            {
                var w = master.NumberWidth ?? 0d;
                var h = master.NumberHigh ?? 0d;
                if (w > 0d && w < 5d && h > 0d && h < 10d)
                {
                    nominalWidthMm = Math.Round(w * 1000d);
                    nominalHeightMm = Math.Round(h * 1000d);
                    return true;
                }

                return false;
            }

            return PanelCode.IsMatch(FormworkPanelColorRepository.ArticleCodeKey(code));
        }

        public static bool IsFiller(string code)
        {
            return !string.IsNullOrWhiteSpace(code) && code.Trim().StartsWith("REMATE", StringComparison.OrdinalIgnoreCase);
        }
    }

    /// <summary>
    /// US «Generar plano de montaje»: una vista por cara (F-n) de cada muro con sus piezas.
    /// Pone NumberFaceId a las piezas que no lo tienen y rehace las vistas del diseño
    /// (las anteriores pasan a borrado lógico). Llamar dentro de una transacción.
    /// Las tablas nuevas aún no están en Model.edmx: se escriben con SQL.
    /// </summary>
    public sealed class DesignAssemblyViewRepository
    {
        private const double DefaultHeightMm = 2700d;
        private const int ElementRowsPerInsert = 100;
        private const int IdsPerUpdate = 500;

        private readonly ConexionData _db;

        public DesignAssemblyViewRepository(ConexionData db)
        {
            _db = db;
        }

        private sealed class Member
        {
            public TSql_DesignWallArticle Piece;
            public bool Through;
            public double LocalX;
            public double LocalY;
            public double Depth;
        }

        public DesignAssemblyViewsResult Generate(long designId, string userId)
        {
            var walls = _db.TSql_DesignWall.AsNoTracking()
                .Where(w => w.LinkDesign_V2 == designId && !w.Is_Delete && w.Is_Active)
                .OrderBy(w => w.NumberLineId ?? w.IdObject)
                .ThenBy(w => w.IdObject)
                .ToList();
            Dictionary<long, DesignWallFaceAxis> axisByWallRow;
            var axes = BuildAxes(walls, out axisByWallRow);
            var faces = DesignWallFaceBuilder.Build(axes);

            var faceById = faces.ToDictionary(f => f.Id);
            var frames = faces.ToDictionary(f => f.Id, f => DesignWallFaceFrame.From(f));
            var exteriorByAxis = new Dictionary<DesignWallFaceAxis, DesignWallFace>();
            var interiorByAxis = new Dictionary<DesignWallFaceAxis, DesignWallFace>();
            for (var i = 0; i < faces.Count; i++)
            {
                (faces[i].IsInterior ? interiorByAxis : exteriorByAxis)[faces[i].Axis] = faces[i];
            }

            var pieces = new DesignWallArticleRepository(_db).QueryLatestActive(designId)
                .OrderBy(a => a.LinkDesignWall)
                .ThenBy(a => a.NumberSequence)
                .ThenBy(a => a.IdObject)
                .ToList();
            var saved = ReadSavedFaceIds(designId);

            var members = faces.ToDictionary(f => f.Id, f => new List<Member>());
            var stamp = new Dictionary<int, List<long>>();
            var clear = new List<long>();
            var withoutFace = 0;
            var outside = 0;
            for (var i = 0; i < pieces.Count; i++)
            {
                var piece = pieces[i];
                var x = piece.NumberInsertX;
                var z = piece.NumberInsertZ;
                DesignWallFaceAxis axis;
                axisByWallRow.TryGetValue(piece.LinkDesignWall, out axis);

                // Cara calculada de nuevo (los F-n cambian si cambian los muros): la de su muro o, si no está
                // encima, la más cercana de cualquier muro sobre la que esté.
                bool through = false;
                DesignWallFace face = null;
                if (axis != null)
                {
                    bool onAxis;
                    var own = PickOnAxis(axis, exteriorByAxis, interiorByAxis, x, z, out onAxis);
                    if (own != null && FaceHolds(own, frames, x, z))
                    {
                        face = own;
                        through = onAxis;
                    }
                }

                if (face == null)
                {
                    var best = double.MaxValue;
                    foreach (var other in exteriorByAxis.Keys.Concat(interiorByAxis.Keys).Distinct())
                    {
                        bool onAxis;
                        var candidate = PickOnAxis(other, exteriorByAxis, interiorByAxis, x, z, out onAxis);
                        double depth;
                        if (candidate == null || !frames[candidate.Id].Holds(x, z, candidate.Axis.ThicknessMm, out depth))
                        {
                            continue;
                        }

                        if (Math.Abs(depth) < best)
                        {
                            best = Math.Abs(depth);
                            face = candidate;
                            through = onAxis;
                        }
                    }
                }

                int savedId;
                var hasSaved = saved.TryGetValue(piece.IdObject, out savedId);
                if (face == null)
                {
                    if (hasSaved)
                    {
                        clear.Add(piece.IdObject);
                    }
                }
                else if (!hasSaved || savedId != face.Id)
                {
                    List<long> ids;
                    if (!stamp.TryGetValue(face.Id, out ids))
                    {
                        ids = new List<long>();
                        stamp[face.Id] = ids;
                    }

                    ids.Add(piece.IdObject);
                }

                if (face == null)
                {
                    if (faces.Count > 0)
                    {
                        outside++;
                    }
                    else
                    {
                        withoutFace++;
                    }

                    continue;
                }

                members[face.Id].Add(ToMember(face, frames[face.Id], piece, through));
                if (through)
                {
                    DesignWallFace ext;
                    DesignWallFace inn;
                    exteriorByAxis.TryGetValue(face.Axis, out ext);
                    interiorByAxis.TryGetValue(face.Axis, out inn);
                    var other = face == ext ? inn : ext;
                    if (other != null)
                    {
                        members[other.Id].Add(ToMember(other, frames[other.Id], piece, true));
                    }
                }
            }

            var stamped = StampFaceIds(stamp, userId) + ClearFaceIds(clear, userId);
            DeleteViews(designId);

            var now = DateTime.Now;
            var summaries = new List<DesignAssemblyViewSummary>(faces.Count);
            var elementCount = 0;
            for (var i = 0; i < faces.Count; i++)
            {
                var face = faces[i];
                var list = members[face.Id]
                    .OrderBy(m => m.LocalX)
                    .ThenBy(m => m.LocalY)
                    .ThenBy(m => m.Piece.IdObject)
                    .ToList();
                var frame = frames[face.Id];
                var name = face.IdText + " " + face.SideName;
                var viewId = InsertView(designId, face, frame, name, list.Count, userId, now);
                InsertElements(viewId, face, list, userId, now);
                elementCount += list.Count;
                summaries.Add(new DesignAssemblyViewSummary
                {
                    FaceId = face.Id,
                    FaceName = name,
                    WallId = face.WallId,
                    WallAngleDeg = frame.WallAngleDeg,
                    ElementCount = list.Count
                });
            }

            return new DesignAssemblyViewsResult
            {
                Pieces = pieces.Count,
                Stamped = stamped,
                WithoutFace = withoutFace,
                Outside = outside,
                Elements = elementCount,
                Views = summaries
            };
        }

        private static DesignWallFace PickOnAxis(
            DesignWallFaceAxis axis,
            Dictionary<DesignWallFaceAxis, DesignWallFace> exteriorByAxis,
            Dictionary<DesignWallFaceAxis, DesignWallFace> interiorByAxis,
            double x,
            double z,
            out bool through)
        {
            DesignWallFace ext;
            DesignWallFace inn;
            exteriorByAxis.TryGetValue(axis, out ext);
            interiorByAxis.TryGetValue(axis, out inn);
            return DesignWallFaceAssigner.Pick(ext, inn, x, z, out through);
        }

        private static bool FaceHolds(DesignWallFace face, Dictionary<int, DesignWallFaceFrame> frames, double x, double z)
        {
            double depth;
            return frames[face.Id].Holds(x, z, face.Axis.ThicknessMm, out depth);
        }

        /// <summary>Vista activa de la cara F-n del diseño con sus piezas y los STL de cada una. Null si no existe.</summary>
        public DesignAssemblyViewDto LoadView(long designId, int faceId)
        {
            var head = _db.Database.SqlQuery<ViewRow>(
                "SELECT TOP 1 v.IdObject, v.NumberFaceId, v.TextFaceName, v.TextFaceSide, v.LinkDesignWall, w.TextClientWallId, " +
                "v.NumberWallAngleDeg, v.NumberFaceNormalDeg, v.NumberP1X, v.NumberP1Z, v.NumberP2X, v.NumberP2Z, v.NumberBaseY, v.NumberLengthMm, v.NumberHeightMm " +
                "FROM dbo.TSql_DesignAssemblyView v LEFT JOIN dbo.TSql_DesignWall w ON w.IdObject = v.LinkDesignWall " +
                "WHERE v.LinkDesign_V2 = @p0 AND v.NumberFaceId = @p1 AND v.Is_Delete = 0 ORDER BY v.IdObject DESC",
                designId, faceId).FirstOrDefault();
            if (head == null)
            {
                return null;
            }

            var rows = _db.Database.SqlQuery<ElementRow>(
                "SELECT LinkDesignWallArticle, LinkMasterArticle, TextCode, NumberSequence, NumberInsertX, NumberInsertY, NumberInsertZ, " +
                "NumberXRotation, NumberYRotation, NumberZRotation, NumberLocalX, NumberLocalY, NumberLocalDepth, Is_Through " +
                "FROM dbo.TSql_DesignAssemblyViewElement WHERE LinkDesignAssemblyView = @p0 AND Is_Delete = 0 ORDER BY NumberSequence, IdObject",
                head.IdObject).ToList();

            var masterIds = rows.Where(r => r.LinkMasterArticle.HasValue).Select(r => r.LinkMasterArticle.Value).Distinct().ToList();
            var masters = masterIds.Count == 0
                ? new Dictionary<long, Tsql_Master_Articles>()
                : _db.Tsql_Master_Articles.AsNoTracking()
                    .Where(m => masterIds.Contains(m.IdObject) && m.AddIsActive)
                    .ToList()
                    .ToDictionary(m => m.IdObject);
            var codesWithoutMaster = rows
                .Where(r => !r.LinkMasterArticle.HasValue && !string.IsNullOrWhiteSpace(r.TextCode))
                .Select(r => r.TextCode.Trim())
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList();
            var masterByCode = new Dictionary<string, Tsql_Master_Articles>(StringComparer.OrdinalIgnoreCase);
            if (codesWithoutMaster.Count > 0)
            {
                var found = _db.Tsql_Master_Articles.AsNoTracking()
                    .Where(m => m.AddIsActive && codesWithoutMaster.Contains(m.TextCode))
                    .OrderBy(m => m.IdObject)
                    .ToList();
                for (var i = 0; i < found.Count; i++)
                {
                    var code = (found[i].TextCode ?? string.Empty).Trim();
                    if (code.Length > 0 && !masterByCode.ContainsKey(code))
                    {
                        masterByCode[code] = found[i];
                    }
                }
            }

            var dx = head.NumberP2X - head.NumberP1X;
            var dz = head.NumberP2Z - head.NumberP1Z;
            var len = Math.Sqrt(dx * dx + dz * dz);
            var ux = len > 1e-6 ? dx / len : 1d;
            var uz = len > 1e-6 ? dz / len : 0d;

            var elements = new List<DesignAssemblyViewElementDto>(rows.Count);
            for (var i = 0; i < rows.Count; i++)
            {
                var r = rows[i];
                Tsql_Master_Articles master = null;
                if (r.LinkMasterArticle.HasValue)
                {
                    masters.TryGetValue(r.LinkMasterArticle.Value, out master);
                }
                else if (!string.IsNullOrWhiteSpace(r.TextCode))
                {
                    masterByCode.TryGetValue(r.TextCode.Trim(), out master);
                }

                string stl;
                string phenolic;
                DesignWallArticleRepository.ResolveStlPair(master, r.TextCode, false, out stl, out phenolic);
                double nominalWidthMm;
                double nominalHeightMm;
                var isPanel = DesignAssemblyPieceKind.IsPanel(r.TextCode, master, out nominalWidthMm, out nominalHeightMm);
                elements.Add(new DesignAssemblyViewElementDto
                {
                    ArticleId = r.LinkDesignWallArticle,
                    MasterArticleId = r.LinkMasterArticle ?? (master != null ? (long?)master.IdObject : null),
                    Code = r.TextCode,
                    Name = master != null ? master.TextLabel : null,
                    Sequence = r.NumberSequence,
                    X = r.NumberInsertX,
                    Y = r.NumberInsertY,
                    Z = r.NumberInsertZ,
                    RotationX = r.NumberXRotation,
                    RotationY = r.NumberYRotation,
                    RotationZ = r.NumberZRotation,
                    LocalX = r.NumberLocalX,
                    LocalY = r.NumberLocalY,
                    Depth = r.NumberLocalDepth,
                    Through = r.Is_Through,
                    StlUrl = stl,
                    StlPhenolicUrl = phenolic,
                    IsPanel = isPanel,
                    IsFiller = DesignAssemblyPieceKind.IsFiller(r.TextCode),
                    Lying = Math.Abs(r.NumberXRotation - 90d) < 1d,
                    NominalWidthMm = nominalWidthMm,
                    NominalHeightMm = nominalHeightMm,
                    Color = isPanel && master != null ? FormworkPanelColorRepository.NormalizeColor(master.TextColor1) : null
                });
            }

            return new DesignAssemblyViewDto
            {
                FaceId = head.NumberFaceId,
                FaceName = head.TextFaceName,
                FaceSide = head.TextFaceSide,
                WallId = head.TextClientWallId,
                WallDbId = head.LinkDesignWall,
                WallAngleDeg = head.NumberWallAngleDeg,
                FaceNormalDeg = head.NumberFaceNormalDeg,
                P1X = head.NumberP1X,
                P1Z = head.NumberP1Z,
                P2X = head.NumberP2X,
                P2Z = head.NumberP2Z,
                BaseY = head.NumberBaseY,
                LengthMm = head.NumberLengthMm,
                HeightMm = head.NumberHeightMm,
                UX = ux,
                UZ = uz,
                // U = (nz, −nx)  ⇒  N = (−uz, ux).
                NX = -uz,
                NZ = ux,
                Elements = elements
            };
        }

        private sealed class ViewRow
        {
            public long IdObject { get; set; }
            public int NumberFaceId { get; set; }
            public string TextFaceName { get; set; }
            public string TextFaceSide { get; set; }
            public long LinkDesignWall { get; set; }
            public string TextClientWallId { get; set; }
            public double NumberWallAngleDeg { get; set; }
            public double NumberFaceNormalDeg { get; set; }
            public double NumberP1X { get; set; }
            public double NumberP1Z { get; set; }
            public double NumberP2X { get; set; }
            public double NumberP2Z { get; set; }
            public double NumberBaseY { get; set; }
            public double NumberLengthMm { get; set; }
            public double NumberHeightMm { get; set; }
        }

        private sealed class ElementRow
        {
            public long? LinkDesignWallArticle { get; set; }
            public long? LinkMasterArticle { get; set; }
            public string TextCode { get; set; }
            public long NumberSequence { get; set; }
            public double NumberInsertX { get; set; }
            public double NumberInsertY { get; set; }
            public double NumberInsertZ { get; set; }
            public double NumberXRotation { get; set; }
            public double NumberYRotation { get; set; }
            public double NumberZRotation { get; set; }
            public double NumberLocalX { get; set; }
            public double NumberLocalY { get; set; }
            public double NumberLocalDepth { get; set; }
            public bool Is_Through { get; set; }
        }

        /// <summary>
        /// Vistas guardadas si siguen valiendo: existen y son posteriores al último cambio de los muros y de las
        /// piezas del diseño (altas, cambios y borrados). Null si hay que generarlas de nuevo.
        /// </summary>
        public DesignAssemblyViewsResult LoadCurrent(long designId)
        {
            var viewsAt = _db.Database.SqlQuery<DateTime?>(
                "SELECT MIN(AddDateMade) FROM dbo.TSql_DesignAssemblyView WHERE LinkDesign_V2 = @p0 AND Is_Delete = 0",
                designId).FirstOrDefault();
            if (!viewsAt.HasValue)
            {
                return null;
            }

            var changedAt = _db.Database.SqlQuery<DateTime?>(
                "SELECT MAX(d) FROM (" +
                "SELECT MAX(ISNULL(AddLastDateChange, AddDateMade)) d FROM dbo.TSql_DesignWall WHERE LinkDesign_V2 = @p0 " +
                "UNION ALL SELECT MAX(ISNULL(AddLastDateChange, AddDateMade)) FROM dbo.TSql_DesignWallArticle WHERE LinkDesign_V2 = @p0) x",
                designId).FirstOrDefault();
            if (changedAt.HasValue && changedAt.Value > viewsAt.Value.AddMilliseconds(500))
            {
                return null;
            }

            var views = _db.Database.SqlQuery<DesignAssemblyViewSummary>(
                "SELECT v.NumberFaceId AS FaceId, v.TextFaceName AS FaceName, " +
                "COALESCE(w.TextClientWallId, CAST(w.NumberWallGroupId AS NVARCHAR(50))) AS WallId, " +
                "v.NumberWallAngleDeg AS WallAngleDeg, v.NumberElementCount AS ElementCount " +
                "FROM dbo.TSql_DesignAssemblyView v LEFT JOIN dbo.TSql_DesignWall w ON w.IdObject = v.LinkDesignWall " +
                "WHERE v.LinkDesign_V2 = @p0 AND v.Is_Delete = 0 ORDER BY v.NumberFaceId",
                designId).ToList();
            var placed = _db.Database.SqlQuery<int>(
                "SELECT COUNT(DISTINCT e.LinkDesignWallArticle) FROM dbo.TSql_DesignAssemblyViewElement e " +
                "JOIN dbo.TSql_DesignAssemblyView v ON v.IdObject = e.LinkDesignAssemblyView " +
                "WHERE v.LinkDesign_V2 = @p0 AND v.Is_Delete = 0 AND e.Is_Delete = 0",
                designId).FirstOrDefault();
            var elements = views.Sum(v => v.ElementCount);
            var pieces = new DesignWallArticleRepository(_db).QueryLatestActive(designId).Count();
            return new DesignAssemblyViewsResult
            {
                Pieces = pieces,
                Stamped = 0,
                WithoutFace = 0,
                Outside = Math.Max(0, pieces - placed),
                Elements = elements,
                Views = views,
                Reused = true
            };
        }

        /// <summary>
        /// Mismos ejes que el visor (maStlWallFacesCollectAxisInputs): solo TextWallRole = 'axis'.
        /// Grosor = 2 × NumberOffsetMm de su primera cara; si no tiene, NumberWidth.
        /// </summary>
        private static List<DesignWallFaceAxis> BuildAxes(IList<TSql_DesignWall> walls, out Dictionary<long, DesignWallFaceAxis> axisByWallRow)
        {
            axisByWallRow = new Dictionary<long, DesignWallFaceAxis>();
            var axes = new List<DesignWallFaceAxis>();
            var axisByLineId = new Dictionary<long, DesignWallFaceAxis>();
            for (var i = 0; i < walls.Count; i++)
            {
                var w = walls[i];
                if (!string.Equals(w.TextWallRole, "axis", StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                var lineId = w.NumberLineId ?? w.IdObject;
                var axis = new DesignWallFaceAxis
                {
                    Id = lineId,
                    WallDbId = w.IdObject,
                    WallId = !string.IsNullOrWhiteSpace(w.TextClientWallId)
                        ? w.TextClientWallId
                        : (w.NumberWallGroupId.HasValue ? w.NumberWallGroupId.Value.ToString(CultureInfo.InvariantCulture) : null),
                    P1X = w.NumberP1X,
                    P1Z = w.NumberP1Z,
                    P2X = w.NumberP2X,
                    P2Z = w.NumberP2Z,
                    ThicknessMm = FaceThicknessMm(walls, lineId) ?? MetersToMm(w.NumberWidth) ?? 0d,
                    HeightMm = Math.Max(300d, MetersToMm(w.NumberHeight) ?? DefaultHeightMm),
                    BaseY = Math.Min(w.NumberP1Y, w.NumberP2Y)
                };
                axes.Add(axis);
                axisByWallRow[w.IdObject] = axis;
                axisByLineId[lineId] = axis;
            }

            // Piezas enlazadas a una línea de cara: van con su eje.
            for (var i = 0; i < walls.Count; i++)
            {
                var w = walls[i];
                DesignWallFaceAxis axis;
                if (string.Equals(w.TextWallRole, "face", StringComparison.OrdinalIgnoreCase)
                    && w.NumberLinkOffsetFromLineId.HasValue
                    && axisByLineId.TryGetValue(w.NumberLinkOffsetFromLineId.Value, out axis))
                {
                    axisByWallRow[w.IdObject] = axis;
                }
            }

            return axes;
        }

        private static double? FaceThicknessMm(IList<TSql_DesignWall> walls, long axisLineId)
        {
            for (var i = 0; i < walls.Count; i++)
            {
                var w = walls[i];
                if (string.Equals(w.TextWallRole, "face", StringComparison.OrdinalIgnoreCase)
                    && w.NumberLinkOffsetFromLineId == axisLineId
                    && w.NumberOffsetMm.HasValue)
                {
                    return Math.Abs(w.NumberOffsetMm.Value) * 2d;
                }
            }

            return null;
        }

        private static double? MetersToMm(double? meters)
        {
            return meters.HasValue && meters.Value > 0d ? meters.Value * 1000d : (double?)null;
        }

        private static Member ToMember(DesignWallFace face, DesignWallFaceFrame frame, TSql_DesignWallArticle piece, bool through)
        {
            double localX;
            double depth;
            frame.Local(piece.NumberInsertX, piece.NumberInsertZ, out localX, out depth);
            return new Member
            {
                Piece = piece,
                Through = through,
                LocalX = localX,
                LocalY = piece.NumberInsertY - face.Axis.BaseY,
                Depth = depth
            };
        }

        private Dictionary<long, int> ReadSavedFaceIds(long designId)
        {
            var rows = _db.Database.SqlQuery<SavedFaceRow>(
                "SELECT IdObject, NumberFaceId FROM dbo.TSql_DesignWallArticle WHERE LinkDesign_V2 = @p0 AND Is_Delete = 0 AND NumberFaceId IS NOT NULL",
                designId).ToList();
            var map = new Dictionary<long, int>(rows.Count);
            for (var i = 0; i < rows.Count; i++)
            {
                map[rows[i].IdObject] = rows[i].NumberFaceId;
            }

            return map;
        }

        private sealed class SavedFaceRow
        {
            public long IdObject { get; set; }
            public int NumberFaceId { get; set; }
        }

        private int StampFaceIds(Dictionary<int, List<long>> stamp, string userId)
        {
            var total = 0;
            foreach (var pair in stamp)
            {
                var ids = pair.Value;
                for (var start = 0; start < ids.Count; start += IdsPerUpdate)
                {
                    var chunk = ids.Skip(start).Take(IdsPerUpdate)
                        .Select(id => id.ToString(CultureInfo.InvariantCulture));
                    total += _db.Database.ExecuteSqlCommand(
                        "UPDATE dbo.TSql_DesignWallArticle SET NumberFaceId = @p0, LinModifiedBy = @p1, AddLastDateChange = @p2, Ntimeschanged = Ntimeschanged + 1 WHERE IdObject IN (" + string.Join(",", chunk) + ")",
                        pair.Key,
                        userId ?? "",
                        DateTime.Now);
                }
            }

            return total;
        }

        private int ClearFaceIds(List<long> ids, string userId)
        {
            var total = 0;
            for (var start = 0; start < ids.Count; start += IdsPerUpdate)
            {
                var chunk = ids.Skip(start).Take(IdsPerUpdate).Select(id => id.ToString(CultureInfo.InvariantCulture));
                total += _db.Database.ExecuteSqlCommand(
                    "UPDATE dbo.TSql_DesignWallArticle SET NumberFaceId = NULL, LinModifiedBy = @p0, AddLastDateChange = @p1, Ntimeschanged = Ntimeschanged + 1 WHERE IdObject IN (" + string.Join(",", chunk) + ")",
                    userId ?? "",
                    DateTime.Now);
            }

            return total;
        }

        /// <summary>
        /// Borra de verdad (no lógico) las vistas y sus piezas de este diseño antes de volver a generarlas: son datos
        /// calculados, se rehacen enteros y no deben quedar restos de muros anteriores. Lo pidió Juan (25/11/2026).
        /// Solo toca este diseño.
        /// </summary>
        private void DeleteViews(long designId)
        {
            _db.Database.ExecuteSqlCommand(
                "DELETE e FROM dbo.TSql_DesignAssemblyViewElement e JOIN dbo.TSql_DesignAssemblyView v ON v.IdObject = e.LinkDesignAssemblyView WHERE v.LinkDesign_V2 = @p0",
                designId);
            _db.Database.ExecuteSqlCommand(
                "DELETE FROM dbo.TSql_DesignAssemblyView WHERE LinkDesign_V2 = @p0",
                designId);
        }

        private long InsertView(long designId, DesignWallFace face, DesignWallFaceFrame frame, string name, int count, string userId, DateTime at)
        {
            var label = name + " · Muro " + (face.WallId ?? "");
            return _db.Database.SqlQuery<long>(
                "INSERT INTO dbo.TSql_DesignAssemblyView (TextLabel, LinkDesign_V2, LinkDesignWall, NumberFaceId, TextFaceName, TextFaceSide, " +
                "NumberWallAngleDeg, NumberFaceNormalDeg, NumberP1X, NumberP1Z, NumberP2X, NumberP2Z, NumberBaseY, NumberLengthMm, NumberHeightMm, " +
                "NumberElementCount, LinkMadeBy, AddDateMade) OUTPUT INSERTED.IdObject VALUES " +
                "(@p0, @p1, @p2, @p3, @p4, @p5, @p6, @p7, @p8, @p9, @p10, @p11, @p12, @p13, @p14, @p15, @p16, @p17)",
                Truncate(label, 500),
                designId,
                face.Axis.WallDbId,
                face.Id,
                Truncate(name, 100),
                face.SideName,
                frame.WallAngleDeg,
                frame.NormalDeg,
                frame.P1X,
                frame.P1Z,
                frame.P2X,
                frame.P2Z,
                face.Axis.BaseY,
                frame.LengthMm,
                face.Axis.HeightMm,
                count,
                userId ?? "",
                at).Single();
        }

        private void InsertElements(long viewId, DesignWallFace face, IList<Member> list, string userId, DateTime at)
        {
            for (var start = 0; start < list.Count; start += ElementRowsPerInsert)
            {
                var end = Math.Min(list.Count, start + ElementRowsPerInsert);
                var sql = new StringBuilder(
                    "INSERT INTO dbo.TSql_DesignAssemblyViewElement (TextLabel, LinkDesignAssemblyView, LinkDesignWallArticle, LinkMasterArticle, TextCode, " +
                    "NumberSequence, NumberInsertX, NumberInsertY, NumberInsertZ, NumberXRotation, NumberYRotation, NumberZRotation, " +
                    "NumberLocalX, NumberLocalY, NumberLocalDepth, Is_Through, LinkMadeBy, AddDateMade) VALUES ");
                var ps = new List<SqlParameter>
                {
                    new SqlParameter("@view", SqlDbType.BigInt) { Value = viewId },
                    new SqlParameter("@by", SqlDbType.NVarChar, 128) { Value = userId ?? "" },
                    new SqlParameter("@at", SqlDbType.DateTime) { Value = at }
                };
                for (var i = start; i < end; i++)
                {
                    var m = list[i];
                    var p = m.Piece;
                    var code = string.IsNullOrWhiteSpace(p.TextCode) ? null : p.TextCode.Trim();
                    var k = "r" + (i - start).ToString(CultureInfo.InvariantCulture);
                    if (i > start)
                    {
                        sql.Append(',');
                    }

                    sql.Append("(@").Append(k).Append("l,@view,@").Append(k).Append("a,@").Append(k).Append("m,@").Append(k).Append("c,@")
                        .Append(k).Append("s,@").Append(k).Append("x,@").Append(k).Append("y,@").Append(k).Append("z,@")
                        .Append(k).Append("rx,@").Append(k).Append("ry,@").Append(k).Append("rz,@")
                        .Append(k).Append("lx,@").Append(k).Append("ly,@").Append(k).Append("ld,@").Append(k).Append("t,@by,@at)");
                    ps.Add(Text("@" + k + "l", Truncate((code ?? "Artículo") + " · " + face.IdText, 500), 500));
                    ps.Add(new SqlParameter("@" + k + "a", SqlDbType.BigInt) { Value = p.IdObject });
                    ps.Add(new SqlParameter("@" + k + "m", SqlDbType.BigInt) { Value = (object)p.LinkMasterArticle ?? DBNull.Value });
                    ps.Add(Text("@" + k + "c", Truncate(code, 50), 50));
                    ps.Add(new SqlParameter("@" + k + "s", SqlDbType.BigInt) { Value = (long)(i + 1) });
                    ps.Add(Number("@" + k + "x", p.NumberInsertX));
                    ps.Add(Number("@" + k + "y", p.NumberInsertY));
                    ps.Add(Number("@" + k + "z", p.NumberInsertZ));
                    ps.Add(Number("@" + k + "rx", p.NumberXRotation));
                    ps.Add(Number("@" + k + "ry", p.NumberYRotation));
                    ps.Add(Number("@" + k + "rz", p.NumberZRotation));
                    ps.Add(Number("@" + k + "lx", m.LocalX));
                    ps.Add(Number("@" + k + "ly", m.LocalY));
                    ps.Add(Number("@" + k + "ld", m.Depth));
                    ps.Add(new SqlParameter("@" + k + "t", SqlDbType.Bit) { Value = m.Through });
                }

                _db.Database.ExecuteSqlCommand(sql.ToString(), ps.ToArray());
            }
        }

        private static SqlParameter Text(string name, string value, int size)
        {
            return new SqlParameter(name, SqlDbType.NVarChar, size) { Value = (object)value ?? DBNull.Value };
        }

        private static SqlParameter Number(string name, double value)
        {
            return new SqlParameter(name, SqlDbType.Float) { Value = value };
        }

        private static string Truncate(string value, int max)
        {
            if (value == null)
            {
                return null;
            }

            return value.Length <= max ? value : value.Substring(0, max);
        }
    }
}
