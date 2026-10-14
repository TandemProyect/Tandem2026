using DAL;
using Desing.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Desing.Repositories.RepositoryDesing2
{
    public sealed class PluginSaveWallArticleRequest
    {
        public long DesignId { get; set; }
        public string DeviceId { get; set; }
        public long? WallDbId { get; set; }
        public long? MasterArticleId { get; set; }
        public string CodeName { get; set; }
        public string View { get; set; }
        public Desing2XyzMmDto P1Mm { get; set; }
        public Desing2XyzMmDto P2Mm { get; set; }
        public double? DataWith { get; set; }
        public double? DataLong { get; set; }
        public double? DataHeight { get; set; }
        public string TextSystem { get; set; }
        public Desing2XyzMmDto InsertMm { get; set; }
        public double RotationX { get; set; }
        public double RotationY { get; set; }
        public double RotationZ { get; set; }
        public string HandleCad { get; set; }
    }

    public sealed class PluginSaveWallArticleResult
    {
        public long WallId { get; set; }
        public long ArticleId { get; set; }
        public long Sequence { get; set; }
        public TSql_DesignWallArticle Article { get; set; }
    }

    public sealed class Desing2DesignWallArticleDto
    {
        public long IdObject { get; set; }
        public long WallDbId { get; set; }
        public long? MasterArticleId { get; set; }
        public string TextCode { get; set; }
        public string TextView { get; set; }
        public long NumberSequence { get; set; }
        public Desing2XyzMmDto InsertMm { get; set; }
        public double RotationX { get; set; }
        public double RotationY { get; set; }
        public double RotationZ { get; set; }
        public string TextHandleCad { get; set; }
        public string StlUrl { get; set; }
        public string StlPhenolicUrl { get; set; }
    }

    public sealed class DesignWallArticleRepository
    {
        private readonly ConexionData _db;
        private readonly DesignWallRepository _walls;

        public DesignWallArticleRepository(ConexionData db)
        {
            _db = db;
            _walls = new DesignWallRepository(db);
        }

        public PluginSaveWallArticleResult SaveManual(PluginSaveWallArticleRequest request, string userId)
        {
            if (request == null)
            {
                throw new ArgumentNullException("request");
            }

            var wall = _walls.FindOrCreateAxisForManual(request, userId);
            if (wall.IdObject == 0)
            {
                _db.SaveChanges();
            }

            var seq = NextSequence(wall.IdObject);
            var code = Truncate((request.CodeName ?? string.Empty).Trim(), 50);
            var view = NormalizeView(request.View);
            var masterId = ResolveMasterArticleId(request.MasterArticleId, code);
            var insert = request.InsertMm ?? new Desing2XyzMmDto();
            var label = string.IsNullOrWhiteSpace(code) ? "Artículo" : (code + " " + view);

            var article = new TSql_DesignWallArticle
            {
                TextLabel = Truncate(label, 500),
                LinkDesign_V2 = request.DesignId,
                LinkDesignWall = wall.IdObject,
                LinkMasterArticle = masterId,
                TextCode = code,
                TextView = Truncate(view, 20),
                NumberSequence = seq,
                NumberInsertX = insert.X ?? 0d,
                NumberInsertY = insert.Y ?? 0d,
                NumberInsertZ = insert.Z ?? 0d,
                NumberXRotation = request.RotationX,
                NumberYRotation = request.RotationY,
                NumberZRotation = request.RotationZ,
                TextHandleCad = Truncate(request.HandleCad, 50)
            };
            IntranetAuditHelper.SetAuditOnCreate(article, userId);
            _db.TSql_DesignWallArticle.Add(article);

            return new PluginSaveWallArticleResult
            {
                WallId = wall.IdObject,
                Article = article,
                Sequence = seq
            };
        }

        /// <summary>
        /// Sustituye los artículos del diseño por la lista recibida.
        /// Solo debe llamarse cuando el cliente envía Articles (icono Salvar de AutoCAD).
        /// Si Articles es null no se invoca: Desing no borra lo guardado en CAD.
        /// Lista vacía = borrar lógico todos los artículos del diseño.
        /// </summary>
        public int SoftDeleteByDesign(long designId, string userId)
        {
            if (designId <= 0)
            {
                return 0;
            }

            return _db.Database.ExecuteSqlCommand(
                "UPDATE dbo.TSql_DesignWallArticle SET Is_Delete = 1, LinModifiedBy = @p0, AddLastDateChange = @p1, Ntimeschanged = Ntimeschanged + 1 WHERE LinkDesign_V2 = @p2 AND Is_Delete = 0",
                userId ?? "",
                DateTime.Now,
                designId);
        }

        private readonly List<TSql_DesignWallArticle> _batch = new List<TSql_DesignWallArticle>();
        private long _batchDesignId;
        private DateTime _batchAt;

        public int ReplaceAll(long designId, IList<PluginSaveWallArticleRequest> articles, string userId)
        {
            _batch.Clear();
            _batchDesignId = designId;
            _batchAt = DateTime.Now;

            if (articles == null || articles.Count == 0)
            {
                return 0;
            }

            var walls = _db.TSql_DesignWall
                .Where(w => w.LinkDesign_V2 == designId && !w.Is_Delete)
                .ToList();
            var codes = new List<string>();
            for (var c = 0; c < articles.Count; c++)
            {
                var code = articles[c] == null ? "" : (articles[c].CodeName ?? "").Trim();
                if (code.Length > 0 && !codes.Contains(code))
                {
                    codes.Add(code);
                }
            }

            var masters = codes.Count == 0
                ? new List<Tsql_Master_Articles>()
                : _db.Tsql_Master_Articles.AsNoTracking()
                    .Where(m => m.AddIsActive && (codes.Contains(m.TextCode) || codes.Contains(m.AddAtenkoCode) || codes.Contains(m.TextBlockNumber)))
                    .ToList();

            var seqByWall = new Dictionary<long, long>();
            var count = 0;
            for (var i = 0; i < articles.Count; i++)
            {
                var request = articles[i];
                if (request == null || string.IsNullOrWhiteSpace(request.CodeName))
                {
                    continue;
                }

                request.DesignId = designId;
                var wall = _walls.ResolveExistingWall(request, walls);
                if (wall == null)
                {
                    throw new InvalidOperationException("Este diseño no tiene muros guardados.");
                }

                long seq;
                if (!seqByWall.TryGetValue(wall.IdObject, out seq))
                {
                    seq = 0;
                }
                seq++;
                seqByWall[wall.IdObject] = seq;

                var code = Truncate((request.CodeName ?? string.Empty).Trim(), 50);
                var view = NormalizeView(request.View);
                var masterId = FindMasterId(masters, request.MasterArticleId, code);
                var insert = request.InsertMm ?? new Desing2XyzMmDto();
                var label = string.IsNullOrWhiteSpace(code) ? "Artículo" : (code + " " + view);

                var article = new TSql_DesignWallArticle
                {
                    TextLabel = Truncate(label, 500),
                    LinkDesign_V2 = designId,
                    LinkDesignWall = wall.IdObject,
                    LinkMasterArticle = masterId,
                    TextCode = code,
                    TextView = Truncate(view, 20),
                    NumberSequence = seq,
                    NumberInsertX = insert.X ?? 0d,
                    NumberInsertY = insert.Y ?? 0d,
                    NumberInsertZ = insert.Z ?? 0d,
                    NumberXRotation = request.RotationX,
                    NumberYRotation = request.RotationY,
                    NumberZRotation = request.RotationZ,
                    TextHandleCad = Truncate(request.HandleCad, 50)
                };
                IntranetAuditHelper.SetAuditOnCreate(article, userId);
                article.AddDateMade = _batchAt;
                _db.TSql_DesignWallArticle.Add(article);
                _batch.Add(article);
                count++;
            }

            return count;
        }

        /// <summary>
        /// Tras guardar el recuento nuevo, marca borradas solo las piezas anteriores de este diseño.
        /// No espera: el aviso ya puede cerrarse. El recuento visible es el lote recién guardado.
        /// </summary>
        public void QueueRetirePrevious(string userId)
        {
            if (_batchDesignId <= 0)
            {
                return;
            }

            long? keepFromId = null;
            if (_batch.Count > 0)
            {
                var min = _batch[0].IdObject;
                for (var i = 1; i < _batch.Count; i++)
                {
                    if (_batch[i].IdObject < min)
                    {
                        min = _batch[i].IdObject;
                    }
                }
                if (min <= 0)
                {
                    return;
                }
                keepFromId = min;
            }

            var designId = _batchDesignId;
            var user = userId ?? "";
            System.Web.Hosting.HostingEnvironment.QueueBackgroundWorkItem(delegate
            {
                try
                {
                    using (var db = new ConexionData())
                    {
                        db.Database.CommandTimeout = 0;
                        if (keepFromId.HasValue)
                        {
                            db.Database.ExecuteSqlCommand(
                                "UPDATE dbo.TSql_DesignWallArticle SET Is_Delete = 1, LinModifiedBy = @p0, AddLastDateChange = @p1, Ntimeschanged = Ntimeschanged + 1 WHERE LinkDesign_V2 = @p2 AND Is_Delete = 0 AND IdObject < @p3",
                                user,
                                DateTime.Now,
                                designId,
                                keepFromId.Value);
                        }
                        else
                        {
                            db.Database.ExecuteSqlCommand(
                                "UPDATE dbo.TSql_DesignWallArticle SET Is_Delete = 1, LinModifiedBy = @p0, AddLastDateChange = @p1, Ntimeschanged = Ntimeschanged + 1 WHERE LinkDesign_V2 = @p2 AND Is_Delete = 0",
                                user,
                                DateTime.Now,
                                designId);
                        }
                    }
                }
                catch
                {
                }
            });
        }

        private static long? FindMasterId(List<Tsql_Master_Articles> masters, long? requestedId, string code)
        {
            if (requestedId.HasValue && requestedId.Value > 0)
            {
                for (var i = 0; i < masters.Count; i++)
                {
                    if (masters[i].IdObject == requestedId.Value)
                    {
                        return masters[i].IdObject;
                    }
                }
            }

            if (string.IsNullOrWhiteSpace(code))
            {
                return null;
            }

            for (var i = 0; i < masters.Count; i++)
            {
                var master = masters[i];
                if (string.Equals(master.TextCode, code, StringComparison.OrdinalIgnoreCase)
                    || string.Equals(master.AddAtenkoCode, code, StringComparison.OrdinalIgnoreCase)
                    || string.Equals(master.TextBlockNumber, code, StringComparison.OrdinalIgnoreCase))
                {
                    return master.IdObject;
                }
            }

            return null;
        }

        public List<Desing2DesignWallArticleDto> ListActive(long designId)
        {
            var rows = QueryLatestActive(designId)
                .OrderBy(a => a.LinkDesignWall)
                .ThenBy(a => a.NumberSequence)
                .ThenBy(a => a.IdObject)
                .ToList();

            var masterIds = new HashSet<long>();
            for (var i = 0; i < rows.Count; i++)
            {
                if (rows[i].LinkMasterArticle.HasValue && rows[i].LinkMasterArticle.Value > 0)
                {
                    masterIds.Add(rows[i].LinkMasterArticle.Value);
                }
            }

            var masters = masterIds.Count == 0
                ? new Dictionary<long, Tsql_Master_Articles>()
                : _db.Tsql_Master_Articles
                    .AsNoTracking()
                    .Where(m => masterIds.Contains(m.IdObject) && m.AddIsActive)
                    .ToList()
                    .ToDictionary(m => m.IdObject);

            var list = new List<Desing2DesignWallArticleDto>(rows.Count);
            for (var i = 0; i < rows.Count; i++)
            {
                var a = rows[i];
                Tsql_Master_Articles master = null;
                if (a.LinkMasterArticle.HasValue)
                {
                    masters.TryGetValue(a.LinkMasterArticle.Value, out master);
                }

                string stlUrl;
                string stlPhenolicUrl;
                ResolveStlPair(master, a.TextCode, false, out stlUrl, out stlPhenolicUrl);

                list.Add(new Desing2DesignWallArticleDto
                {
                    IdObject = a.IdObject,
                    WallDbId = a.LinkDesignWall,
                    MasterArticleId = a.LinkMasterArticle,
                    TextCode = a.TextCode,
                    TextView = a.TextView,
                    NumberSequence = a.NumberSequence,
                    InsertMm = new Desing2XyzMmDto
                    {
                        X = a.NumberInsertX,
                        Y = a.NumberInsertY,
                        Z = a.NumberInsertZ
                    },
                    RotationX = a.NumberXRotation,
                    RotationY = a.NumberYRotation,
                    RotationZ = a.NumberZRotation,
                    TextHandleCad = a.TextHandleCad,
                    StlUrl = stlUrl,
                    StlPhenolicUrl = stlPhenolicUrl
                });
            }

            return list;
        }

        private static void ResolveStlPair(
            Tsql_Master_Articles master,
            string code,
            bool tumbado,
            out string stlUrl,
            out string stlPhenolicUrl)
        {
            stlUrl = null;
            stlPhenolicUrl = null;
            if (master != null)
            {
                string frame;
                string phenolic;
                if (PluginCadAtk60PanelStlHelper.TryResolve(master, out frame, out phenolic))
                {
                    stlUrl = ToContentUrl(ApplyTumbadoFile(frame, tumbado));
                    stlPhenolicUrl = ToContentUrl(ApplyTumbadoFile(phenolic, tumbado));
                }
            }

            if (string.IsNullOrWhiteSpace(stlUrl) && !string.IsNullOrWhiteSpace(code))
            {
                var name = code.Trim();
                stlUrl = ToContentUrl("~/Content/DesignTools/Stl/ATK60/" + name + (tumbado ? "T" : "") + ".stl");
                stlPhenolicUrl = ToContentUrl("~/Content/DesignTools/Stl/ATK60/" + name + (tumbado ? "T" : "") + "_F.stl");
            }
        }

        private static string ApplyTumbadoFile(string path, bool tumbado)
        {
            if (!tumbado || string.IsNullOrWhiteSpace(path))
            {
                return path;
            }

            var n = path.Replace('\\', '/');
            const string phenolic = "_F.stl";
            const string plain = ".stl";
            if (n.EndsWith(phenolic, StringComparison.OrdinalIgnoreCase))
            {
                if (n.EndsWith("T_F.stl", StringComparison.OrdinalIgnoreCase))
                {
                    return n;
                }

                return n.Substring(0, n.Length - phenolic.Length) + "T_F.stl";
            }

            if (n.EndsWith(plain, StringComparison.OrdinalIgnoreCase)
                && !n.EndsWith("T.stl", StringComparison.OrdinalIgnoreCase))
            {
                return n.Substring(0, n.Length - plain.Length) + "T.stl";
            }

            return n;
        }

        private static string ToContentUrl(string raw)
        {
            if (string.IsNullOrWhiteSpace(raw))
            {
                return null;
            }

            var n = raw.Trim().Replace('\\', '/');
            var idx = n.IndexOf("/Content/", StringComparison.OrdinalIgnoreCase);
            if (idx < 0)
            {
                idx = n.IndexOf("Content/", StringComparison.OrdinalIgnoreCase);
                if (idx >= 0)
                {
                    return "/" + n.Substring(idx);
                }
            }
            else
            {
                return n.Substring(idx);
            }

            if (n.StartsWith("~/", StringComparison.Ordinal))
            {
                return n.Substring(1);
            }

            return n.StartsWith("/", StringComparison.Ordinal) ? n : "/" + n;
        }

        private long NextSequence(long wallId)
        {
            if (wallId <= 0)
            {
                return 1;
            }

            var max = _db.TSql_DesignWallArticle
                .Where(a => a.LinkDesignWall == wallId && !a.Is_Delete)
                .Select(a => (long?)a.NumberSequence)
                .Max();
            return (max ?? 0) + 1;
        }

        private long? ResolveMasterArticleId(long? requestedId, string code)
        {
            if (requestedId.HasValue && requestedId.Value > 0)
            {
                var byId = _db.Tsql_Master_Articles
                    .AsNoTracking()
                    .FirstOrDefault(a => a.IdObject == requestedId.Value && a.AddIsActive);
                if (byId != null)
                {
                    return byId.IdObject;
                }
            }

            if (string.IsNullOrWhiteSpace(code))
            {
                return null;
            }

            var byCode = _db.Tsql_Master_Articles
                .AsNoTracking()
                .FirstOrDefault(a =>
                    a.AddIsActive
                    && (a.TextCode == code
                        || a.AddAtenkoCode == code
                        || a.TextBlockNumber == code));
            return byCode != null ? (long?)byCode.IdObject : null;
        }

        private static string NormalizeView(string view)
        {
            if (string.Equals(view, "3d", StringComparison.OrdinalIgnoreCase))
            {
                return "3D";
            }

            if (string.Equals(view, "xr", StringComparison.OrdinalIgnoreCase))
            {
                return "Xr";
            }

            return "3DRef";
        }

        private static string Truncate(string value, int max)
        {
            if (string.IsNullOrEmpty(value))
            {
                return value;
            }

            return value.Length <= max ? value : value.Substring(0, max);
        }
    }
}
