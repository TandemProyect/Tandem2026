using System;
using System.Collections.Generic;
using ZwcadPlugin.Models;
using ZwSoft.ZwCAD.ApplicationServices;
using ZwSoft.ZwCAD.DatabaseServices;
using ZwSoft.ZwCAD.EditorInput;
using ZwSoft.ZwCAD.Geometry;
using ZwSoft.ZwCAD.Runtime;
using AcadApp = ZwSoft.ZwCAD.ApplicationServices.Application;

[assembly: CommandClass(typeof(ZwcadPlugin.WallImportCommand))]

namespace ZwcadPlugin
{
    /// <summary>
    /// Dibuja en ZWCAD los muros de un diseño V2 (TSql_DesignWall), mismas capas que muro 2D.
    /// Coordenadas Desing_2 en mm (planta XZ) → XY del DWG.
    /// </summary>
    public class WallImportCommand
    {
        public const string CommandName = "TANDEM_ABRIRDISENO";
        private const string LayerAxis = "TANDEM_MURO_EJE";
        private const string LayerFace = "TANDEM_MURO_CARA";
        private const string Layer3d = "TANDEM_MURO_3D";

        internal static long PendingDesignId;
        internal static WallSnapshotDto PendingSnapshot;
        internal static long CurrentDesignId;

        [CommandMethod(CommandName)]
        public void OpenPending()
        {
            var snap = PendingSnapshot;
            var id = PendingDesignId;
            PendingSnapshot = null;
            PendingDesignId = 0;
            var doc = AcadApp.DocumentManager.MdiActiveDocument;
            if (doc == null) return;
            if (snap == null)
            {
                doc.Editor.WriteMessage("\nNo hay diseño pendiente. Elige uno en la paleta Tandem.\n");
                return;
            }
            CurrentDesignId = id;
            Draw(doc, id, snap);
            AfterLoad(doc);
        }

        public static void OpenFromPalette(long designId, WallSnapshotDto snapshot)
        {
            var doc = AcadApp.DocumentManager.MdiActiveDocument;
            if (doc == null) return;
            CurrentDesignId = designId;
            if (snapshot == null || snapshot.Lines == null || snapshot.Lines.Count == 0)
            {
                doc.Editor.WriteMessage($"\nDiseño {designId}: sin muros guardados. Dibuja y pulsa Salvar.\n");
                AfterLoad(doc);
                return;
            }

            try
            {
                using (doc.LockDocument())
                {
                    Draw(doc, designId, snapshot);
                    AfterLoad(doc);
                }
            }
            catch
            {
                PendingDesignId = designId;
                PendingSnapshot = snapshot;
                doc.SendStringToExecute("\x03\x03" + CommandName + " ", true, false, false);
            }
        }

        private static void Draw(Document doc, long designId, WallSnapshotDto snapshot)
        {
            Editor ed = doc.Editor;
            Database db = doc.Database;
            var lines = snapshot.Lines ?? new List<WallLineDto>();
            int drawn = 0;

            using (var tr = db.TransactionManager.StartTransaction())
            {
                Wall2dCommand.EnsureAppAndLayers(tr, db);
                ClearLayer(tr, db, LayerAxis);
                ClearLayer(tr, db, LayerFace);
                ClearLayer(tr, db, Layer3d);
                WallArticleCad.ErasePanels(tr, db);

                var bt = (BlockTable)tr.GetObject(db.BlockTableId, OpenMode.ForRead);
                var ms = (BlockTableRecord)tr.GetObject(bt[BlockTableRecord.ModelSpace], OpenMode.ForWrite);

                var drawnKeys = new List<string>();
                foreach (var row in lines)
                {
                    if (row == null || row.P1Mm == null || row.P2Mm == null) continue;
                    Point3d a = ToAcad(row.P1Mm);
                    Point3d b = ToAcad(row.P2Mm);
                    if (a.DistanceTo(b) < 1e-9) continue;
                    string role = (row.WallRole ?? "axis").Trim().ToLowerInvariant();
                    if (AlreadyDrawn(drawnKeys, role, a, b))
                        continue;
                    drawnKeys.Add(LineKey(role, a, b));

                    string layer = role == "face" ? LayerFace : LayerAxis;
                    var line = new Line(a, b) { Layer = layer };
                    var thickness = CadUnits.FromMillimeters((row.DataWith ?? 0.30) * 1000.0);
                    var groupId = row.WallGroupId.HasValue ? (int)row.WallGroupId.Value : 0;
                    WallCadXData.Write(
                        line,
                        role,
                        groupId,
                        thickness,
                        row.WallDbId ?? 0,
                        row.IsSpecial == true);
                    ms.AppendEntity(line);
                    tr.AddNewlyCreatedDBObject(line, true);
                    drawn++;
                }

                tr.Commit();
            }

            var articles = snapshot.Articles ?? new List<WallArticleDto>();
            WallArticleCad.Remember(articles);
            ed.WriteMessage(
                $"\nDiseño {designId}: {drawn} muro(s) en {LayerAxis} / {LayerFace}"
                + (articles.Count > 0
                    ? $". {articles.Count} artículo(s) se insertarán al levantar el 3D."
                    : ".")
                + "\n");
        }

        private static void AfterLoad(Document doc)
        {
            PaletteHost.HideHomeForm();
            try
            {
                ZoomExtents(doc);
            }
            catch
            {
                try { doc.SendStringToExecute("._ZOOM _E ", true, false, false); } catch { }
            }
        }

        private static void ZoomExtents(Document doc)
        {
            if (doc == null) return;
            var db = doc.Database;
            var ed = doc.Editor;
            db.UpdateExt(true);
            Extents3d ext;
            try
            {
                ext = new Extents3d(db.Extmin, db.Extmax);
            }
            catch
            {
                return;
            }

            var min = ext.MinPoint;
            var max = ext.MaxPoint;
            if (min.GetAsVector().Length < 1e-9 && max.GetAsVector().Length < 1e-9)
                return;

            var dx = max.X - min.X;
            var dy = max.Y - min.Y;
            if (dx < 1e-6) dx = 1;
            if (dy < 1e-6) dy = 1;
            var padX = dx * 0.05;
            var padY = dy * 0.05;

            using (var view = ed.GetCurrentView())
            {
                view.CenterPoint = new Point2d((min.X + max.X) * 0.5, (min.Y + max.Y) * 0.5);
                view.Width = dx + padX * 2;
                view.Height = dy + padY * 2;
                ed.SetCurrentView(view);
            }
        }

        private static bool AlreadyDrawn(List<string> keys, string role, Point3d a, Point3d b)
        {
            return keys.Contains(LineKey(role, a, b));
        }

        private static string LineKey(string role, Point3d a, Point3d b)
        {
            var ax = Math.Round(a.X / 25.0);
            var ay = Math.Round(a.Y / 25.0);
            var bx = Math.Round(b.X / 25.0);
            var by = Math.Round(b.Y / 25.0);
            var left = ax < bx || (ax == bx && ay <= by);
            var x1 = left ? ax : bx;
            var y1 = left ? ay : by;
            var x2 = left ? bx : ax;
            var y2 = left ? by : ay;
            return role + "|" + x1 + "," + y1 + "|" + x2 + "," + y2;
        }

        private static Point3d ToAcad(XyzMmDto p)
        {
            double x = p.X ?? 0;
            double yPlan = p.Z ?? p.Y ?? 0;
            double z = p.Z.HasValue ? (p.Y ?? 0) : 0;
            return new Point3d(
                CadUnits.FromMillimeters(x),
                CadUnits.FromMillimeters(yPlan),
                CadUnits.FromMillimeters(z));
        }

        private static void ClearLayer(Transaction tr, Database db, string layerName)
        {
            var layers = (LayerTable)tr.GetObject(db.LayerTableId, OpenMode.ForRead);
            if (!layers.Has(layerName)) return;

            var bt = (BlockTable)tr.GetObject(db.BlockTableId, OpenMode.ForRead);
            var ms = (BlockTableRecord)tr.GetObject(bt[BlockTableRecord.ModelSpace], OpenMode.ForRead);
            var toErase = new List<Entity>();
            foreach (ObjectId id in ms)
            {
                var ent = tr.GetObject(id, OpenMode.ForRead) as Entity;
                if (ent == null) continue;
                if (string.Equals(ent.Layer, layerName, StringComparison.OrdinalIgnoreCase))
                    toErase.Add(ent);
            }

            foreach (var ent in toErase)
            {
                ent.UpgradeOpen();
                ent.Erase();
            }
        }
    }
}

