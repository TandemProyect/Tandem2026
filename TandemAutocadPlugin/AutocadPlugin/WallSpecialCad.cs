using System;
using System.Collections.Generic;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;
using AcadApp = Autodesk.AutoCAD.ApplicationServices.Application;

namespace AutocadPlugin
{
    /// <summary>
    /// Is_Special: hay paneles manuales, o se ha tocado el encofrado automático.
    /// Si se borran todos los paneles manuales y el auto no se ha editado, vuelve a false.
    /// </summary>
    internal static class WallSpecialCad
    {
        private static readonly HashSet<ObjectId> FormworkEditedAxes = new HashSet<ObjectId>();
        private static readonly List<Point3d> PendingFormworkEraseAt = new List<Point3d>();
        private static readonly HashSet<Document> Hooked = new HashSet<Document>();
        private static bool _bound;
        private static bool _pendingRecalc;

        public static bool SuppressEraseWatch { get; set; }

        public static void Bind()
        {
            if (_bound)
                return;
            _bound = true;
            try
            {
                var dm = AcadApp.DocumentManager;
                dm.DocumentCreated += (s, e) => Hook(e.Document);
                foreach (Document doc in dm)
                    Hook(doc);
            }
            catch
            {
            }
        }

        public static void MarkFormworkEdited(ObjectId axisId)
        {
            if (!axisId.IsNull)
                FormworkEditedAxes.Add(axisId);
        }

        public static void Recalc(Database db)
        {
            if (db == null)
                return;
            using (var tr = db.TransactionManager.StartTransaction())
            {
                RecalcUnlocked(tr, db);
                tr.Commit();
            }
        }

        private static void ConsumePendingFormworkErases(Transaction tr, Database db)
        {
            if (PendingFormworkEraseAt.Count == 0 || tr == null || db == null)
                return;
            var bt = (BlockTable)tr.GetObject(db.BlockTableId, OpenMode.ForRead);
            var ms = (BlockTableRecord)tr.GetObject(bt[BlockTableRecord.ModelSpace], OpenMode.ForRead);
            var axes = new List<Line>();
            foreach (ObjectId id in ms)
            {
                var line = tr.GetObject(id, OpenMode.ForRead) as Line;
                if (line != null
                    && string.Equals(line.Layer, WallCadXData.LayerAxis, StringComparison.OrdinalIgnoreCase))
                    axes.Add(line);
            }

            for (var i = 0; i < PendingFormworkEraseAt.Count; i++)
            {
                var at = PendingFormworkEraseAt[i];
                Line best = null;
                var bestD = double.MaxValue;
                for (var a = 0; a < axes.Count; a++)
                {
                    var d = WallCadXData.DistToSegment(at, axes[a].StartPoint, axes[a].EndPoint);
                    if (d < bestD)
                    {
                        bestD = d;
                        best = axes[a];
                    }
                }
                if (best != null && bestD <= CadUnits.FromMillimeters(800))
                    FormworkEditedAxes.Add(best.ObjectId);
            }
            PendingFormworkEraseAt.Clear();
        }

        public static void RecalcUnlocked(Transaction tr, Database db)
        {
            if (tr == null || db == null)
                return;
            ConsumePendingFormworkErases(tr, db);
            _pendingRecalc = false;
            var axes = new List<Line>();
            var manuals = new List<Point3d>();
            var formworkPts = new List<Point3d>();
            var bt = (BlockTable)tr.GetObject(db.BlockTableId, OpenMode.ForRead);
            var ms = (BlockTableRecord)tr.GetObject(bt[BlockTableRecord.ModelSpace], OpenMode.ForRead);
            foreach (ObjectId id in ms)
            {
                var line = tr.GetObject(id, OpenMode.ForRead) as Line;
                if (line != null
                    && string.Equals(line.Layer, WallCadXData.LayerAxis, StringComparison.OrdinalIgnoreCase))
                {
                    axes.Add(line);
                }
            }

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
                var at = BlockInsertCommand.InsertWorld(br);
                if (string.Equals(br.Layer, WallArticleCad.LayerFormwork, StringComparison.OrdinalIgnoreCase))
                    formworkPts.Add(at);
                else
                    manuals.Add(at);
            }

            foreach (var axis in axes)
            {
                string role;
                int groupId;
                double thickness;
                long wallDbId;
                bool oldSpecial;
                WallCadXData.Read(axis, out role, out groupId, out thickness, out wallDbId, out oldSpecial);
                var hasManual = false;
                var hasFormwork = false;
                var band = Math.Max(thickness * 1.6, CadUnits.FromMillimeters(80));
                for (var i = 0; i < manuals.Count; i++)
                {
                    if (WallCadXData.DistToSegment(manuals[i], axis.StartPoint, axis.EndPoint) <= band)
                    {
                        hasManual = true;
                        break;
                    }
                }
                for (var f = 0; f < formworkPts.Count; f++)
                {
                    if (WallCadXData.DistToSegment(formworkPts[f], axis.StartPoint, axis.EndPoint) <= band)
                    {
                        hasFormwork = true;
                        break;
                    }
                }

                var special = hasManual
                    || FormworkEditedAxes.Contains(axis.ObjectId)
                    || (oldSpecial && hasFormwork);
                if (special == oldSpecial)
                    continue;
                axis.UpgradeOpen();
                WallCadXData.Write(axis, role, groupId, thickness, wallDbId, special);
            }
        }

        private static void Hook(Document doc)
        {
            if (doc == null || Hooked.Contains(doc))
                return;
            Hooked.Add(doc);
            try
            {
                doc.Database.ObjectErased += OnObjectErased;
                doc.CommandEnded += OnCommandEnded;
            }
            catch
            {
            }
        }

        private static void OnObjectErased(object sender, ObjectErasedEventArgs e)
        {
            if (SuppressEraseWatch || e == null || !e.Erased)
                return;
            var br = e.DBObject as BlockReference;
            if (br == null)
                return;
            var formwork = string.Equals(br.Layer, WallArticleCad.LayerFormwork, StringComparison.OrdinalIgnoreCase);
            string code;
            string view;
            string role;
            int rot;
            if (!formwork && !BlockInsertCommand.TryReadAtk(br, out code, out view, out role, out rot))
                return;
            _pendingRecalc = true;
            if (!formwork)
                return;
            try
            {
                PendingFormworkEraseAt.Add(BlockInsertCommand.InsertWorld(br));
            }
            catch
            {
            }
        }

        private static void OnCommandEnded(object sender, CommandEventArgs e)
        {
            if (SuppressEraseWatch || e == null || sender == null)
                return;
            if (!_pendingRecalc && PendingFormworkEraseAt.Count == 0)
                return;
            var doc = sender as Document;
            if (doc == null)
                return;
            try
            {
                using (doc.LockDocument())
                using (var tr = doc.Database.TransactionManager.StartTransaction())
                {
                    ConsumePendingFormworkErases(tr, doc.Database);
                    RecalcUnlocked(tr, doc.Database);
                    tr.Commit();
                }
                WallSaveCommand.PersistSilent(doc);
            }
            catch
            {
            }
        }
    }
}
