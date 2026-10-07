using System;
using System.Collections.Generic;
using ZwcadPlugin.Models;
using ZwSoft.ZwCAD.ApplicationServices;
using ZwSoft.ZwCAD.DatabaseServices;
using ZwSoft.ZwCAD.Geometry;

namespace ZwcadPlugin
{
    /// <summary>
    /// Paneles ATK del DWG: en 2D no se ven; al levantar 3D se vuelven a insertar.
    /// Igual que Desing (modo muro 2D / muro 3D).
    /// </summary>
    internal static class WallArticleCad
    {
        public const string Layer3d = "TANDEM_MURO_3D";
        public const string LayerFormwork = "TANDEM_ENCOFRADO";

        private static readonly List<WallArticleDto> Cache = new List<WallArticleDto>();

        public static void Remember(IList<WallArticleDto> articles)
        {
            Cache.Clear();
            if (articles == null)
                return;
            for (var i = 0; i < articles.Count; i++)
            {
                if (articles[i] != null && !string.IsNullOrWhiteSpace(articles[i].TextCode)
                    && !SamePose(Cache, articles[i]))
                    Cache.Add(articles[i]);
            }
        }

        private static bool SamePose(IList<WallArticleDto> list, WallArticleDto item)
        {
            var insert = item.InsertMm;
            if (insert == null)
                return false;
            for (var i = 0; i < list.Count; i++)
            {
                var other = list[i];
                if (other == null || other.InsertMm == null)
                    continue;
                if (!string.Equals(other.TextCode, item.TextCode, StringComparison.OrdinalIgnoreCase))
                    continue;
                if (Math.Abs(other.RotationX - item.RotationX) > 1)
                    continue;
                if (Math.Abs(other.RotationY - item.RotationY) > 1)
                    continue;
                if (Math.Abs(other.RotationZ - item.RotationZ) > 1)
                    continue;
                var o = other.InsertMm;
                if (Math.Abs((o.X ?? 0) - (insert.X ?? 0)) > 25)
                    continue;
                if (Math.Abs((o.Y ?? 0) - (insert.Y ?? 0)) > 25)
                    continue;
                if (Math.Abs((o.Z ?? 0) - (insert.Z ?? 0)) > 25)
                    continue;
                return true;
            }
            return false;
        }

        public static void Enter2d(Document doc)
        {
            if (doc == null)
                return;
            var prev = WallSpecialCad.SuppressEraseWatch;
            WallSpecialCad.SuppressEraseWatch = true;
            try
            {
                using (doc.LockDocument())
                using (var tr = doc.Database.TransactionManager.StartTransaction())
                {
                    CaptureUnlocked(tr, doc.Database);
                    ErasePanels(tr, doc.Database);
                    EraseLayer(tr, doc.Database, Layer3d);
                    tr.Commit();
                }
            }
            finally
            {
                WallSpecialCad.SuppressEraseWatch = prev;
            }
        }

        public static void CaptureAndErasePanels(Document doc)
        {
            if (doc == null)
                return;
            var prev = WallSpecialCad.SuppressEraseWatch;
            WallSpecialCad.SuppressEraseWatch = true;
            try
            {
                using (doc.LockDocument())
                using (var tr = doc.Database.TransactionManager.StartTransaction())
                {
                    CaptureUnlocked(tr, doc.Database);
                    ErasePanels(tr, doc.Database);
                    tr.Commit();
                }
            }
            finally
            {
                WallSpecialCad.SuppressEraseWatch = prev;
            }
        }

        public static int RestoreCached(Document doc)
        {
            return Restore(doc, Cache);
        }

        public static List<WallArticleDto> Collect(Database db)
        {
            var list = new List<WallArticleDto>();
            if (db == null)
                return list;
            using (var tr = db.TransactionManager.StartTransaction())
            {
                CollectInto(tr, db, list);
                tr.Commit();
            }
            return list;
        }

        public static void ErasePanels(Transaction tr, Database db)
        {
            var bt = (BlockTable)tr.GetObject(db.BlockTableId, OpenMode.ForRead);
            var ms = (BlockTableRecord)tr.GetObject(bt[BlockTableRecord.ModelSpace], OpenMode.ForRead);
            var erase = new List<Entity>();
            foreach (ObjectId id in ms)
            {
                var br = tr.GetObject(id, OpenMode.ForRead) as BlockReference;
                if (br == null)
                    continue;
                string code;
                string view;
                string role;
                int rot;
                if (!BlockInsertCommand.TryReadAtk(br, out code, out view, out role, out rot))
                    continue;
                erase.Add(br);
            }

            foreach (var ent in erase)
            {
                ent.UpgradeOpen();
                ent.Erase();
            }
        }

        public static void EraseLayer(Transaction tr, Database db, string layerName)
        {
            var layers = (LayerTable)tr.GetObject(db.LayerTableId, OpenMode.ForRead);
            if (!layers.Has(layerName))
                return;
            var bt = (BlockTable)tr.GetObject(db.BlockTableId, OpenMode.ForRead);
            var ms = (BlockTableRecord)tr.GetObject(bt[BlockTableRecord.ModelSpace], OpenMode.ForRead);
            var doomed = new List<Entity>();
            foreach (ObjectId id in ms)
            {
                var ent = tr.GetObject(id, OpenMode.ForRead) as Entity;
                if (ent != null && string.Equals(ent.Layer, layerName, StringComparison.OrdinalIgnoreCase))
                    doomed.Add(ent);
            }
            foreach (var ent in doomed)
            {
                ent.UpgradeOpen();
                ent.Erase();
            }
        }

        public static int Restore(Document doc, IList<WallArticleDto> articles)
        {
            if (doc == null || articles == null || articles.Count == 0)
                return 0;

            var restored = 0;
            var db = doc.Database;
            var meterToDwg = CadUnits.FromMillimeters(1000.0);
            var cache = new Dictionary<string, ObjectId>(StringComparer.OrdinalIgnoreCase);
            var prev = WallSpecialCad.SuppressEraseWatch;
            WallSpecialCad.SuppressEraseWatch = true;

            foreach (var item in articles)
            {
                if (item == null || string.IsNullOrWhiteSpace(item.TextCode))
                    continue;
                var view = BlockInsertCommand.NormalizeView(item.TextView);
                var key = item.TextCode + "|" + view;
                if (cache.ContainsKey(key))
                    continue;
                var dwg = Atk60DwgResolver.ResolveDwg(item.TextCode, view);
                if (string.IsNullOrWhiteSpace(dwg))
                    continue;
                cache[key] = BlockInsertCommand.EnsureBlockDefinition(
                    doc, dwg, BlockInsertCommand.BlockNameFor(item.TextCode, view));
            }

            try
            {
            using (doc.LockDocument())
            using (var tr = db.TransactionManager.StartTransaction())
            {
                var bt = (BlockTable)tr.GetObject(db.BlockTableId, OpenMode.ForRead);
                var ms = (BlockTableRecord)tr.GetObject(bt[BlockTableRecord.ModelSpace], OpenMode.ForWrite);
                foreach (var item in articles)
                {
                    if (item == null || string.IsNullOrWhiteSpace(item.TextCode))
                        continue;
                    var view = BlockInsertCommand.NormalizeView(item.TextView);
                    ObjectId blockId;
                    if (!cache.TryGetValue(item.TextCode + "|" + view, out blockId) || blockId.IsNull)
                        continue;

                    var at = WallCadXData.ToAcad(item.InsertMm);
                    var tumbado = Math.Abs(item.RotationX - 90) < 1;
                    double yawDeg;
                    bool spun;
                    bool encoded;
                    BlockInsertCommand.DecodePoseZ(item.RotationZ, out yawDeg, out spun, out encoded);
                    var yaw = yawDeg * Math.PI / 180.0;
                    var drawScale = BlockInsertCommand.ReferenceScale(db, blockId, meterToDwg);
                    var orient = Matrix3d.Rotation(yaw, Vector3d.ZAxis, Point3d.Origin)
                        * BlockInsertCommand.PanelOrient(drawScale, tumbado);
                    if (!encoded && (Math.Abs(item.RotationY - 180) < 1 || Math.Abs(item.RotationY + 180) < 1))
                        orient = Matrix3d.Rotation(Math.PI, Vector3d.ZAxis, Point3d.Origin) * orient;
                    var br = new BlockReference(Point3d.Origin, blockId);
                    if (encoded && spun && !tumbado)
                    {
                        double panelW;
                        double panelH;
                        BlockInsertCommand.PanelSizeMeters(item.TextCode, out panelW, out panelH);
                        var pre = Matrix3d.Rotation(yaw - Math.PI, Vector3d.ZAxis, Point3d.Origin)
                            * BlockInsertCommand.PanelOrient(drawScale, false);
                        BlockInsertCommand.ApplyFormworkPanelMatrix(br, at, pre, panelW * meterToDwg);
                    }
                    else
                        BlockInsertCommand.ApplyPanelMatrix(br, at, orient);
                    ms.AppendEntity(br);
                    tr.AddNewlyCreatedDBObject(br, true);
                    BlockInsertCommand.ApplyAtkXData(
                        br, tr, db, item.TextCode, view, "PANEL",
                        tumbado ? 90 : 0, item.WallDbId, at);
                    restored++;
                }
                tr.Commit();
            }
            }
            finally
            {
                WallSpecialCad.SuppressEraseWatch = prev;
            }

            return restored;
        }

        private static void CaptureUnlocked(Transaction tr, Database db)
        {
            var found = new List<WallArticleDto>();
            CollectInto(tr, db, found);
            if (found.Count > 0)
            {
                Cache.Clear();
                Cache.AddRange(found);
            }
        }

        private static void CollectInto(Transaction tr, Database db, IList<WallArticleDto> list)
        {
            var bt = (BlockTable)tr.GetObject(db.BlockTableId, OpenMode.ForRead);
            var ms = (BlockTableRecord)tr.GetObject(bt[BlockTableRecord.ModelSpace], OpenMode.ForRead);
            var axes = new List<Line>();
            foreach (ObjectId id in ms)
            {
                var line = tr.GetObject(id, OpenMode.ForRead) as Line;
                if (line == null)
                    continue;
                if (string.Equals(line.Layer, WallCadXData.LayerAxis, StringComparison.OrdinalIgnoreCase))
                    axes.Add(line);
            }

            foreach (ObjectId id in ms)
            {
                var br = tr.GetObject(id, OpenMode.ForRead) as BlockReference;
                if (br == null)
                    continue;
                if (string.Equals(br.Layer, LayerFormwork, StringComparison.OrdinalIgnoreCase))
                    continue;

                string code;
                string view;
                string role;
                int rotDeg;
                if (!BlockInsertCommand.TryReadAtk(br, out code, out view, out role, out rotDeg))
                    continue;

                int poseX = rotDeg;
                int poseY = 0;
                double poseZ = BlockInsertCommand.YawDegFromOrient(br.BlockTransform);
                Line axis = null;
                var wallId = BlockInsertCommand.WallIdFromInsert(br);
                var insertAt = BlockInsertCommand.InsertWorld(br);
                if (wallId > 0)
                {
                    for (var i = 0; i < axes.Count; i++)
                    {
                        string axisRole;
                        int groupId;
                        double thickness;
                        long axisWallId;
                        bool special;
                        WallCadXData.Read(axes[i], out axisRole, out groupId, out thickness, out axisWallId, out special);
                        if (axisWallId == wallId)
                        {
                            axis = axes[i];
                            break;
                        }
                    }
                }
                if (axis == null)
                {
                    var best = double.MaxValue;
                    for (var i = 0; i < axes.Count; i++)
                    {
                        var d = WallCadXData.DistToSegment(insertAt, axes[i].StartPoint, axes[i].EndPoint);
                        if (d < best)
                        {
                            best = d;
                            axis = axes[i];
                        }
                    }
                }
                if (axis != null)
                {
                    BlockInsertCommand.ArticleRotations(
                        insertAt, axis.StartPoint, axis.EndPoint, br.BlockTransform,
                        out poseX, out poseY, out poseZ);
                }
                poseZ = BlockInsertCommand.EncodePoseZ(br);

                list.Add(new WallArticleDto
                {
                    WallDbId = wallId,
                    TextCode = code,
                    TextView = view,
                    InsertMm = WallCadXData.ToDesingMm(insertAt),
                    RotationX = poseX,
                    RotationY = poseY,
                    RotationZ = poseZ,
                    TextHandleCad = br.Handle.ToString()
                });
            }
        }
    }
}

