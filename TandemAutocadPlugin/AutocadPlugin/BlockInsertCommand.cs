using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Geometry;
using Autodesk.AutoCAD.GraphicsInterface;
using Autodesk.AutoCAD.Runtime;
using Newtonsoft.Json.Linq;
using AcadApp = Autodesk.AutoCAD.ApplicationServices.Application;
using AcColor = Autodesk.AutoCAD.Colors.Color;

[assembly: CommandClass(typeof(AutocadPlugin.BlockInsertCommand))]

namespace AutocadPlugin
{
    /// <summary>
    /// INSERT de panel ATK-60. El punto es siempre libre: el bloque sigue al cursor.
    /// Si el cursor pasa cerca de un vértice de otro panel, ese nudo se ilumina;
    /// un clic en ese momento engancha. Si no, se coloca donde esté el cursor.
    /// </summary>
    public class BlockInsertCommand
    {
        public const string CommandName = "TANDEM_INSERTBLOQUE";
        private const string AppName = "TANDEM";
        private static BlockInsertRequest _pending;

        public static void QueueFromPalette(JObject obj)
        {
            _pending = BlockInsertRequest.FromJson(obj);
        }

        internal static void FocusDrawing()
        {
            try { Autodesk.AutoCAD.Internal.Utils.SetFocusToDwgView(); }
            catch { }
            try
            {
                var doc = AcadApp.DocumentManager.MdiActiveDocument;
                if (doc != null && doc.Window != null)
                {
                    try { doc.Window.Focus(); } catch { }
                    try
                    {
                        var hwnd = doc.Window.Handle;
                        if (hwnd != IntPtr.Zero)
                            SetForegroundWindow(hwnd);
                    }
                    catch { }
                }
            }
            catch { }
            try
            {
                var p = System.Windows.Forms.Cursor.Position;
                System.Windows.Forms.Cursor.Position = new System.Drawing.Point(p.X + 1, p.Y);
                System.Windows.Forms.Cursor.Position = p;
            }
            catch { }
        }

        [DllImport("user32.dll")]
        private static extern bool SetForegroundWindow(IntPtr hWnd);

        [CommandMethod(CommandName)]
        public void Run()
        {
            var req = _pending;
            _pending = null;
            Document doc = AcadApp.DocumentManager.MdiActiveDocument;
            if (doc == null) return;
            Editor ed = doc.Editor;

            if (req == null || string.IsNullOrWhiteSpace(req.CodeName))
            {
                ed.WriteMessage("\n[Tandem] No hay panel para insertar. Elige uno en la biblioteca.\n");
                return;
            }

            var view = (req.View ?? "3dref").Trim();
            if (string.Equals(view, "alzado", StringComparison.OrdinalIgnoreCase)
                || string.Equals(view, "planta", StringComparison.OrdinalIgnoreCase))
            {
                ed.WriteMessage("\n[Tandem] Alzado y Planta no están activos. Usa 3D o 3DRef.\n");
                return;
            }

            var dwg = Atk60DwgResolver.ResolveDwg(req.CodeName, view);
            if (string.IsNullOrWhiteSpace(dwg))
            {
                ed.WriteMessage("\n[Tandem] No se encontró el DWG de " + req.CodeName + " (" + view + ").\n");
                return;
            }

            var blockName = string.Equals(view, "3d", StringComparison.OrdinalIgnoreCase)
                ? req.CodeName
                : req.CodeName + "R";
            var meterToDwg = CadUnits.FromMillimeters(1000.0);
            var hosts = CollectHostVertices(doc.Database, "PANEL");

            ObjectId blockId;
            try
            {
                blockId = EnsureBlockDefinition(doc, dwg, blockName);
            }
            catch (System.Exception ex)
            {
                ed.WriteMessage("\n[Tandem] No se pudo cargar el DWG: " + ex.Message + "\n");
                return;
            }

            var overlay = new VertexOverlay(hosts);
            bool snapped;
            try
            {
                var placed = DragVisiblePanel(doc, ed, blockId, req, meterToDwg, overlay, out snapped);
                if (!placed)
                    return;
                ed.WriteMessage("\n[Tandem] Insertado " + blockName
                    + (snapped ? " (enganchado a vértice)" : "") + "\n");
            }
            catch (System.Exception ex)
            {
                ed.WriteMessage("\n[Tandem] No se pudo insertar: " + ex.Message + "\n");
            }
        }

        private static List<Point3d> CollectHostVertices(Database db, string role)
        {
            var pts = new List<Point3d>();
            var cache = new Dictionary<string, IList<Atk60Snap>>(StringComparer.OrdinalIgnoreCase);
            using (var tr = db.TransactionManager.StartTransaction())
            {
                var bt = (BlockTable)tr.GetObject(db.BlockTableId, OpenMode.ForRead);
                var ms = (BlockTableRecord)tr.GetObject(bt[BlockTableRecord.ModelSpace], OpenMode.ForRead);
                foreach (ObjectId id in ms)
                {
                    var br = tr.GetObject(id, OpenMode.ForRead) as BlockReference;
                    if (br == null)
                        continue;
                    var codeName = CodeNameFromInsert(br);
                    if (string.IsNullOrWhiteSpace(codeName))
                        continue;
                    IList<Atk60Snap> snaps;
                    if (!cache.TryGetValue(codeName, out snaps))
                    {
                        snaps = Atk60SnapCatalog.Load(codeName, role);
                        cache[codeName] = snaps;
                    }
                    foreach (var snap in snaps)
                        pts.Add(snap.LocalMeters.TransformBy(br.BlockTransform));
                }
                tr.Commit();
            }
            return pts;
        }

        private static string CodeNameFromInsert(BlockReference br)
        {
            if (br.XData != null)
            {
                var tv = br.XData.AsArray();
                for (var i = 0; i < tv.Length - 1; i++)
                {
                    if (tv[i].TypeCode == 1001
                        && string.Equals(tv[i].Value as string, AppName, StringComparison.OrdinalIgnoreCase)
                        && tv[i + 1].TypeCode == 1000)
                    {
                        var xd = (tv[i + 1].Value as string ?? "").Trim();
                        if (IsAtkCodeName(xd))
                            return xd;
                    }
                }
            }

            var name = (br.Name ?? "").Trim();
            if (name.EndsWith("R", StringComparison.OrdinalIgnoreCase) && name.Length > 1)
                name = name.Substring(0, name.Length - 1);
            return IsAtkCodeName(name) ? name : null;
        }

        private static bool IsAtkCodeName(string name)
        {
            if (string.IsNullOrWhiteSpace(name) || name.Length < 7 || name.Length > 12)
                return false;
            foreach (var c in name)
            {
                if (c < '0' || c > '9')
                    return false;
            }
            return true;
        }

        private static ObjectId EnsureBlockDefinition(Document doc, string dwgPath, string blockName)
        {
            var db = doc.Database;
            using (doc.LockDocument())
            using (var tr = db.TransactionManager.StartTransaction())
            {
                var bt = (BlockTable)tr.GetObject(db.BlockTableId, OpenMode.ForRead);
                if (!bt.Has(blockName))
                {
                    bt.UpgradeOpen();
                    using (var source = new Database(false, true))
                    {
                        source.ReadDwgFile(dwgPath, FileOpenMode.OpenForReadAndAllShare, true, "");
                        db.Insert(blockName, source, true);
                    }
                    bt = (BlockTable)tr.GetObject(db.BlockTableId, OpenMode.ForRead);
                }
                var id = bt[blockName];
                tr.Commit();
                return id;
            }
        }

        private static bool DragVisiblePanel(
            Document doc,
            Editor ed,
            ObjectId blockId,
            BlockInsertRequest req,
            double meterToDwg,
            VertexOverlay overlay,
            out bool snapped)
        {
            snapped = false;
            var db = doc.Database;
            var tumbado = req.RotationDeg == 90;
            var jig = new PanelDragJig(blockId, overlay, meterToDwg, tumbado);
            FocusDrawing();
            var oldOsmode = AcadApp.GetSystemVariable("OSMODE");
            var oldOrtho = AcadApp.GetSystemVariable("ORTHOMODE");
            PromptResult drag;
            try
            {
                AcadApp.SetSystemVariable("OSMODE", 0);
                AcadApp.SetSystemVariable("ORTHOMODE", 0);
                drag = ed.Drag(jig);
            }
            finally
            {
                jig.ReleaseGhost();
                try { AcadApp.SetSystemVariable("OSMODE", oldOsmode); } catch { }
                try { AcadApp.SetSystemVariable("ORTHOMODE", oldOrtho); } catch { }
            }

            if (drag.Status != PromptStatus.OK)
                return false;

            snapped = jig.Snapped;
            using (doc.LockDocument())
            using (var tr = db.TransactionManager.StartTransaction())
            {
                var ms = (BlockTableRecord)tr.GetObject(db.CurrentSpaceId, OpenMode.ForWrite);
                var br = new BlockReference(Point3d.Origin, blockId);
                ApplyPanelTransform(br, jig.At, meterToDwg, tumbado);
                ms.AppendEntity(br);
                tr.AddNewlyCreatedDBObject(br, true);
                EnsureRegApp(tr, db);
                br.XData = new ResultBuffer(
                    new TypedValue(1001, AppName),
                    new TypedValue(1000, req.CodeName ?? ""),
                    new TypedValue(1000, req.View ?? "3dref"),
                    new TypedValue(1000, "PANEL"),
                    new TypedValue(1070, req.RotationDeg));
                tr.Commit();
            }
            return true;
        }

        private sealed class PanelDragJig : DrawJig
        {
            private readonly ObjectId _blockId;
            private readonly VertexOverlay _overlay;
            private readonly double _scale;
            private readonly bool _tumbado;
            private Point3d _at = Point3d.Origin;
            private BlockReference _ghost;

            public PanelDragJig(ObjectId blockId, VertexOverlay overlay, double scale, bool tumbado)
            {
                _blockId = blockId;
                _overlay = overlay;
                _scale = scale;
                _tumbado = tumbado;
            }

            public Point3d At { get { return _at; } }
            public bool Snapped { get; private set; }

            public void ReleaseGhost()
            {
                if (_ghost == null)
                    return;
                try { _ghost.Dispose(); } catch { }
                _ghost = null;
            }

            protected override SamplerStatus Sampler(JigPrompts prompts)
            {
                var opts = new JigPromptPointOptions(
                    "\nMueve el panel (parte de 0,0). Cerca de un vértice se ilumina; clic coloca, ESC cancela: ");
                opts.UserInputControls =
                    UserInputControls.Accept3dCoordinates
                    | UserInputControls.GovernedByOrthoMode
                    | UserInputControls.GovernedByUCSDetect;
                opts.Cursor = CursorType.Crosshair;
                opts.UseBasePoint = true;
                opts.BasePoint = Point3d.Origin;
                var res = prompts.AcquirePoint(opts);
                if (res.Status != PromptStatus.OK)
                    return SamplerStatus.Cancel;

                Point3d nearest;
                var near = _overlay.TryNearest(res.Value, out nearest);
                var next = near ? nearest : res.Value;
                if (next.IsEqualTo(_at, new Tolerance(1e-6, 1e-6)) && Snapped == near)
                    return SamplerStatus.NoChange;
                _at = next;
                Snapped = near;
                return SamplerStatus.OK;
            }

            protected override bool WorldDraw(WorldDraw draw)
            {
                if (draw == null)
                    return true;
                if (_ghost == null)
                    _ghost = new BlockReference(Point3d.Origin, _blockId);
                ApplyPanelTransform(_ghost, _at, _scale, _tumbado);
                draw.Geometry.Draw(_ghost);
                _overlay.WorldDraw(draw, Snapped ? (Point3d?)_at : null);
                return true;
            }
        }

        private static void ApplyPanelTransform(BlockReference br, Point3d at, double scale, bool tumbado)
        {
            var m = Matrix3d.Scaling(scale, Point3d.Origin);
            if (tumbado)
                m = Matrix3d.Rotation(-Math.PI / 2.0, Vector3d.XAxis, Point3d.Origin) * m;
            m = Matrix3d.Displacement(at.GetAsVector()) * m;
            br.BlockTransform = m;
        }

        private static void EnsureRegApp(Transaction tr, Database db)
        {
            var rat = (RegAppTable)tr.GetObject(db.RegAppTableId, OpenMode.ForRead);
            if (rat.Has(AppName))
                return;
            rat.UpgradeOpen();
            var rec = new RegAppTableRecord { Name = AppName };
            rat.Add(rec);
            tr.AddNewlyCreatedDBObject(rec, true);
        }

        private sealed class VertexOverlay
        {
            private readonly IList<Point3d> _hosts;
            private readonly double _tol;

            public VertexOverlay(IList<Point3d> hosts)
            {
                _hosts = hosts ?? new List<Point3d>();
                _tol = CadUnits.FromMillimeters(450);
            }

            public bool TryNearest(Point3d cursor, out Point3d nearest)
            {
                nearest = Point3d.Origin;
                var best = double.MaxValue;
                foreach (var p in _hosts)
                {
                    var d3 = p.DistanceTo(cursor);
                    var dxy = new Point3d(p.X, p.Y, 0).DistanceTo(new Point3d(cursor.X, cursor.Y, 0));
                    var d = d3 < dxy ? d3 : dxy;
                    if (d < best)
                    {
                        best = d;
                        nearest = p;
                    }
                }
                return best <= _tol;
            }

            public void WorldDraw(WorldDraw draw, Point3d? hot)
            {
                if (draw == null || _hosts.Count == 0)
                    return;
                var mark = CadUnits.FromMillimeters(180);
                foreach (var p in _hosts)
                {
                    var isHot = hot.HasValue && p.DistanceTo(hot.Value) < 1e-4;
                    var r = isHot ? mark * 1.7 : mark;
                    draw.SubEntityTraits.Color = (short)(isHot ? 30 : 150);
                    draw.Geometry.Circle(p, r, Vector3d.ZAxis);
                    draw.Geometry.Circle(p, r, Vector3d.XAxis);
                    draw.Geometry.Circle(p, r, Vector3d.YAxis);
                    if (isHot)
                    {
                        draw.Geometry.Circle(p, r * 0.4, Vector3d.ZAxis);
                        draw.Geometry.Circle(p, r * 0.4, Vector3d.XAxis);
                        draw.Geometry.Circle(p, r * 0.4, Vector3d.YAxis);
                    }
                }
            }
        }
    }
}
