using System;
using System.Collections.Generic;
using AutocadPlugin.Models;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;

namespace AutocadPlugin
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
                if (articles[i] != null && !string.IsNullOrWhiteSpace(articles[i].TextCode))
                    Cache.Add(articles[i]);
            }
        }

        public static void Enter2d(Document doc)
        {
            if (doc == null)
                return;
            using (doc.LockDocument())
            using (var tr = doc.Database.TransactionManager.StartTransaction())
            {
                CaptureUnlocked(tr, doc.Database);
                ErasePanels(tr, doc.Database);
                EraseLayer(tr, doc.Database, Layer3d);
                tr.Commit();
            }
        }

        public static void CaptureAndErasePanels(Document doc)
        {
            if (doc == null)
                return;
            using (doc.LockDocument())
            using (var tr = doc.Database.TransactionManager.StartTransaction())
            {
                CaptureUnlocked(tr, doc.Database);
                ErasePanels(tr, doc.Database);
                tr.Commit();
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
                    var yaw = item.RotationZ * Math.PI / 180.0;
                    var orient = Matrix3d.Rotation(yaw, Vector3d.ZAxis, Point3d.Origin)
                        * BlockInsertCommand.PanelOrient(meterToDwg, tumbado);
                    if (Math.Abs(item.RotationY - 180) < 1 || Math.Abs(item.RotationY + 180) < 1)
                        orient = Matrix3d.Rotation(Math.PI, Vector3d.ZAxis, Point3d.Origin) * orient;
                    double pieceW;
                    double pieceH;
                    BlockInsertCommand.PanelSizeMeters(item.TextCode, out pieceW, out pieceH);
                    var br = new BlockReference(Point3d.Origin, blockId);
                    BlockInsertCommand.ApplyFormworkPanelMatrix(br, at, orient, pieceW * meterToDwg);
                    ms.AppendEntity(br);
                    tr.AddNewlyCreatedDBObject(br, true);
                    BlockInsertCommand.ApplyAtkXData(
                        br, tr, db, item.TextCode, view, "PANEL",
                        tumbado ? 90 : 0, item.WallDbId);
                    restored++;
                }
                tr.Commit();
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
                        var d = WallCadXData.DistToSegment(br.Position, axes[i].StartPoint, axes[i].EndPoint);
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
                        br.Position, axis.StartPoint, axis.EndPoint, br.BlockTransform,
                        out poseX, out poseY, out poseZ);
                }

                list.Add(new WallArticleDto
                {
                    WallDbId = wallId,
                    TextCode = code,
                    TextView = view,
                    InsertMm = WallCadXData.ToDesingMm(br.Position),
                    RotationX = poseX,
                    RotationY = poseY,
                    RotationZ = poseZ,
                    TextHandleCad = br.Handle.ToString()
                });
            }
        }
    }
}
