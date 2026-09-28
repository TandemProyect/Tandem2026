using System;
using System.Collections.Generic;
using AutocadPlugin.Models;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Geometry;
using Autodesk.AutoCAD.Runtime;
using AcadApp = Autodesk.AutoCAD.ApplicationServices.Application;

[assembly: CommandClass(typeof(AutocadPlugin.WallImportCommand))]

namespace AutocadPlugin
{
    /// <summary>
    /// Dibuja en AutoCAD los muros de un diseño V2 (TSql_DesignWall), mismas capas que muro 2D.
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
            Draw(doc, id, snap);
        }

        public static void OpenFromPalette(long designId, WallSnapshotDto snapshot)
        {
            var doc = AcadApp.DocumentManager.MdiActiveDocument;
            if (doc == null) return;
            if (snapshot == null || snapshot.Lines == null || snapshot.Lines.Count == 0)
            {
                doc.Editor.WriteMessage($"\nDiseño {designId}: no hay muros en TSql_DesignWall.\n");
                return;
            }

            try
            {
                using (doc.LockDocument())
                {
                    Draw(doc, designId, snapshot);
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

                var bt = (BlockTable)tr.GetObject(db.BlockTableId, OpenMode.ForRead);
                var ms = (BlockTableRecord)tr.GetObject(bt[BlockTableRecord.ModelSpace], OpenMode.ForWrite);

                foreach (var row in lines)
                {
                    if (row == null || row.P1Mm == null || row.P2Mm == null) continue;
                    Point3d a = ToAcad(row.P1Mm);
                    Point3d b = ToAcad(row.P2Mm);
                    if (a.DistanceTo(b) < 1e-9) continue;

                    string role = (row.WallRole ?? "axis").Trim().ToLowerInvariant();
                    string layer = role == "face" ? LayerFace : LayerAxis;
                    var line = new Line(a, b) { Layer = layer };
                    ms.AppendEntity(line);
                    tr.AddNewlyCreatedDBObject(line, true);
                    drawn++;
                }

                tr.Commit();
            }

            ed.WriteMessage($"\nDiseño {designId}: {drawn} muro(s) en {LayerAxis} / {LayerFace}.\n");
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
