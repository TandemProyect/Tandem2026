using System;
using System.Collections.Generic;
using System.IO;
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
        internal const string AppName = "TANDEM";
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
                ed.WriteMessage("\n[Tandem] Alzado y Planta no están activos. Usa 3D, 3DRef o Xr.\n");
                return;
            }

            Atk60DwgResolver.RememberArticleUrls(req.CodeName, req.DwgUrl3D, req.DwgUrl3DRef, req.DwgUrlXr);
            var dwg = Atk60DwgResolver.ResolveDwg(req.CodeName, view, req.UrlForView(view));
            if (string.IsNullOrWhiteSpace(dwg))
            {
                ed.WriteMessage("\n[Tandem] No se encontró el DWG de " + req.CodeName + " (" + view + "). Pulsa Actualizar en bloquing.\n");
                return;
            }

            var blockName = BlockNameFor(req.CodeName, view);
            var meterToDwg = CadUnits.FromMillimeters(1000.0);
            double newW;
            double newH;
            PanelSizeMeters(req.CodeName, out newW, out newH);
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

            var overlay = new VertexOverlay(hosts, ed);
            bool snapped;
            try
            {
                var placed = DragVisiblePanel(doc, ed, blockId, req, meterToDwg, newW, newH, overlay, out snapped);
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

        private static void PanelSizeMeters(string codeName, out double width, out double height)
        {
            width = 0.9;
            height = 2.7;
            var snaps = Atk60SnapCatalog.Load(codeName, "PANEL");
            if (snaps == null)
                return;
            foreach (var snap in snaps)
            {
                var id = (snap.Id ?? "").ToUpperInvariant();
                if (id == "V_BR")
                    width = snap.LocalMeters.X;
                else if (id == "V_TL")
                    height = snap.LocalMeters.Z;
            }
        }

        private sealed class HostVertex
        {
            public Point3d World;
            public string Id;
            public Vector3d WidthDir;
            public Vector3d HeightDir;
            public Vector3d ThickDir;
        }

        private static List<HostVertex> CollectHostVertices(Database db, string role)
        {
            var pts = new List<HostVertex>();
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
                    var tf = br.BlockTransform;
                    var wVec = Vector3d.XAxis.TransformBy(tf);
                    var tVec = Vector3d.YAxis.TransformBy(tf);
                    var hVec = Vector3d.ZAxis.TransformBy(tf);
                    var wDir = wVec.Length > 1e-9 ? wVec.GetNormal() : Vector3d.XAxis;
                    var tDir = tVec.Length > 1e-9 ? tVec.GetNormal() : Vector3d.YAxis;
                    var hDir = hVec.Length > 1e-9 ? hVec.GetNormal() : Vector3d.ZAxis;
                    foreach (var snap in snaps)
                    {
                        pts.Add(new HostVertex
                        {
                            World = snap.LocalMeters.TransformBy(tf),
                            Id = snap.Id,
                            WidthDir = wDir,
                            HeightDir = hDir,
                            ThickDir = tDir
                        });
                    }
                }
                tr.Commit();
            }
            return pts;
        }

        internal static string CodeNameFromInsert(BlockReference br)
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

        internal static bool IsAtkCodeName(string name)
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

        internal static ObjectId EnsureBlockDefinition(Document doc, string dwgPath, string blockName)
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
                        if (dwgPath.EndsWith(".dxf", StringComparison.OrdinalIgnoreCase))
                            source.DxfIn(dwgPath, Path.Combine(Path.GetTempPath(), "tandem-dxfin.log"));
                        else
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
            double newW,
            double newH,
            VertexOverlay overlay,
            out bool snapped)
        {
            snapped = false;
            var db = doc.Database;
            var tumbado = req.RotationDeg == 90;
            var jig = new PanelDragJig(blockId, overlay, meterToDwg, tumbado, newW, newH);
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
                ApplyPanelMatrix(br, jig.At, jig.Orient);
                ms.AppendEntity(br);
                tr.AddNewlyCreatedDBObject(br, true);
                ApplyAtkXData(br, tr, db, req.CodeName, req.View ?? "3dref", "PANEL", jig.RotationDeg);
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
            private readonly double _newW;
            private readonly double _newH;
            private Point3d _at = Point3d.Origin;
            private Point3d? _hot;
            private Matrix3d _orient;
            private BlockReference _ghost;

            public PanelDragJig(ObjectId blockId, VertexOverlay overlay, double scale, bool tumbado, double newW, double newH)
            {
                _blockId = blockId;
                _overlay = overlay;
                _scale = scale;
                _tumbado = tumbado;
                _newW = newW;
                _newH = newH;
                _orient = PanelOrient(scale, tumbado);
                RotationDeg = tumbado ? 90 : 0;
            }

            public Point3d At { get { return _at; } }
            public Matrix3d Orient { get { return _orient; } }
            public int RotationDeg { get; private set; }
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

                HostVertex hit;
                var near = _overlay.TryNearest(res.Value, out hit);
                Matrix3d orient;
                int rotDeg;
                if (near)
                {
                    orient = OrientOnHost(hit, _scale, _tumbado);
                    rotDeg = _tumbado || HostIsTumbado(hit) ? 90 : 0;
                }
                else
                {
                    orient = PanelOrient(_scale, _tumbado);
                    rotDeg = _tumbado ? 90 : 0;
                }
                var next = near
                    ? _overlay.Place(hit, res.Value, orient, _newW, _newH)
                    : res.Value;
                var hot = near ? hit.World : (Point3d?)null;
                if (next.IsEqualTo(_at, new Tolerance(1e-6, 1e-6)) && Snapped == near
                    && RotationDeg == rotDeg
                    && SameOrient(_orient, orient)
                    && hot.HasValue == _hot.HasValue
                    && (!hot.HasValue || hot.Value.IsEqualTo(_hot.Value, new Tolerance(1e-6, 1e-6))))
                    return SamplerStatus.NoChange;
                _at = next;
                _hot = hot;
                _orient = orient;
                RotationDeg = rotDeg;
                Snapped = near;
                return SamplerStatus.OK;
            }

            protected override bool WorldDraw(WorldDraw draw)
            {
                if (draw == null)
                    return true;
                if (_ghost == null)
                    _ghost = new BlockReference(Point3d.Origin, _blockId);
                ApplyPanelMatrix(_ghost, _at, _orient);
                draw.Geometry.Draw(_ghost);
                _overlay.WorldDraw(draw, _hot);
                return true;
            }
        }

        private static Matrix3d PanelOrient(double scale, bool tumbado)
        {
            var m = Matrix3d.Scaling(scale, Point3d.Origin);
            if (tumbado)
                m = Matrix3d.Rotation(-Math.PI / 2.0, Vector3d.YAxis, Point3d.Origin) * m;
            return m;
        }

        private static bool HostIsTumbado(HostVertex host)
        {
            return Math.Abs(host.HeightDir.DotProduct(Vector3d.ZAxis)) < 0.7;
        }

        private static bool OrientIsTumbado(Matrix3d orient)
        {
            var h = Vector3d.ZAxis.TransformBy(orient);
            return h.Length > 1e-9 && Math.Abs(h.GetNormal().DotProduct(Vector3d.ZAxis)) < 0.7;
        }

        /// <summary>
        /// Conserva 0°/90° de la paleta y solo gira en planta para compartir
        /// la cara (sentido) del panel anfitrión. Si el usuario va a 0° y el
        /// anfitrión ya está tumbado, entonces sí copia esa tumbada.
        /// </summary>
        private static Matrix3d OrientOnHost(HostVertex host, double scale, bool tumbado)
        {
            if (!tumbado && HostIsTumbado(host))
                return OrientFromHost(host, scale);
            return MatchHostSentido(host, PanelOrient(scale, tumbado));
        }

        private static Matrix3d OrientFromHost(HostVertex host, double scale)
        {
            var x = host.WidthDir.Length > 1e-9 ? host.WidthDir.GetNormal() : Vector3d.XAxis;
            var y = host.ThickDir.Length > 1e-9 ? host.ThickDir.GetNormal() : Vector3d.YAxis;
            var z = host.HeightDir.Length > 1e-9 ? host.HeightDir.GetNormal() : Vector3d.ZAxis;
            if (x.CrossProduct(y).DotProduct(z) < 0)
                y = -y;
            return Matrix3d.AlignCoordinateSystem(
                Point3d.Origin, Vector3d.XAxis, Vector3d.YAxis, Vector3d.ZAxis,
                Point3d.Origin, x, y, z)
                * Matrix3d.Scaling(scale, Point3d.Origin);
        }

        private static Matrix3d MatchHostSentido(HostVertex host, Matrix3d baseOrient)
        {
            var mine = Vector3d.YAxis.TransformBy(baseOrient);
            var theirs = host.ThickDir;
            var from = new Vector3d(mine.X, mine.Y, 0);
            var to = new Vector3d(theirs.X, theirs.Y, 0);
            if (from.Length < 1e-9 || to.Length < 1e-9)
                return baseOrient;
            from = from.GetNormal();
            to = to.GetNormal();
            var yaw = Math.Atan2(from.CrossProduct(to).Z, from.DotProduct(to));
            if (Math.Abs(yaw) < 1e-9)
                return baseOrient;
            return Matrix3d.Rotation(yaw, Vector3d.ZAxis, Point3d.Origin) * baseOrient;
        }

        private static bool SameOrient(Matrix3d a, Matrix3d b)
        {
            var ta = new Tolerance(1e-6, 1e-6);
            var ca = a.CoordinateSystem3d;
            var cb = b.CoordinateSystem3d;
            return ca.Xaxis.IsEqualTo(cb.Xaxis, ta) && ca.Zaxis.IsEqualTo(cb.Zaxis, ta);
        }

        private static void ApplyPanelMatrix(BlockReference br, Point3d at, Matrix3d orient)
        {
            br.BlockTransform = Matrix3d.Displacement(at.GetAsVector()) * orient;
        }

        private static Point3d CornerLocalMeters(string id, double width, double height)
        {
            switch ((id ?? "").ToUpperInvariant())
            {
                case "V_BR":
                    return new Point3d(width, 0, 0);
                case "V_TL":
                    return new Point3d(0, 0, height);
                case "V_TR":
                    return new Point3d(width, 0, height);
                default:
                    return Point3d.Origin;
            }
        }

        internal static bool TryReadAtk(BlockReference br, out string codeName, out string view, out string role, out int rotDeg)
        {
            codeName = CodeNameFromInsert(br);
            view = "";
            role = "PANEL";
            rotDeg = 0;
            if (string.IsNullOrWhiteSpace(codeName))
                return false;

            if (br.XData != null)
            {
                var tv = br.XData.AsArray();
                var texts = new List<string>();
                var inApp = false;
                foreach (var t in tv)
                {
                    if (t.TypeCode == 1001)
                    {
                        inApp = string.Equals(t.Value as string, AppName, StringComparison.OrdinalIgnoreCase);
                        continue;
                    }
                    if (!inApp)
                        continue;
                    if (t.TypeCode == 1000)
                        texts.Add((t.Value as string) ?? "");
                    else if (t.TypeCode == 1070)
                        rotDeg = Convert.ToInt32(t.Value);
                }
                if (texts.Count >= 2)
                    view = texts[1];
                if (texts.Count >= 3)
                    role = texts[2];
            }

            if (string.IsNullOrWhiteSpace(view))
            {
                var n = (br.Name ?? "").Trim();
                if (n.EndsWith("X", StringComparison.OrdinalIgnoreCase))
                    view = "xr";
                else if (n.EndsWith("R", StringComparison.OrdinalIgnoreCase))
                    view = "3dref";
                else
                    view = "3d";
            }
            return true;
        }

        internal static string NormalizeView(string view)
        {
            if (string.Equals(view, "3d", StringComparison.OrdinalIgnoreCase))
                return "3d";
            if (string.Equals(view, "xr", StringComparison.OrdinalIgnoreCase))
                return "xr";
            return "3dref";
        }

        internal static string BlockNameFor(string codeName, string view)
        {
            var v = NormalizeView(view);
            if (v == "3d")
                return codeName;
            if (v == "xr")
                return codeName + "X";
            return codeName + "R";
        }

        internal static void ApplyAtkXData(
            BlockReference br, Transaction tr, Database db,
            string codeName, string view, string role, int rotDeg)
        {
            EnsureRegApp(tr, db);
            br.XData = new ResultBuffer(
                new TypedValue(1001, AppName),
                new TypedValue(1000, codeName ?? ""),
                new TypedValue(1000, view ?? "3dref"),
                new TypedValue(1000, role ?? "PANEL"),
                new TypedValue(1070, rotDeg));
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
            private readonly IList<HostVertex> _hosts;
            private readonly Editor _ed;
            private const double PixelTol = 56;

            public VertexOverlay(IList<HostVertex> hosts, Editor ed)
            {
                _hosts = hosts ?? new List<HostVertex>();
                _ed = ed;
            }

            public bool TryNearest(Point3d cursor, out HostVertex nearest)
            {
                nearest = null;
                var best = double.MaxValue;
                foreach (var p in _hosts)
                {
                    var d = ScreenDist(cursor, p.World);
                    if (d < best)
                    {
                        best = d;
                        nearest = p;
                    }
                }
                return nearest != null && best <= PixelTol;
            }

            public Point3d Place(HostVertex host, Point3d cursor, Matrix3d newOrient, double newW, double newH)
            {
                double sideScore;
                double stackScore;
                ScreenAxisScores(host, cursor, out sideScore, out stackScore);
                var id = (host.Id ?? "").ToUpperInvariant();
                var left = id == "V_BL" || id == "V_TL";
                var top = id == "V_TL" || id == "V_TR";
                var clearlySide = Math.Abs(sideScore) > Math.Abs(stackScore) * 1.2;
                var clearlyUp = stackScore > Math.Abs(sideScore) * 1.2;
                var mixed = OrientIsTumbado(newOrient) != HostIsTumbado(host);
                bool stack;
                if (mixed && top)
                    stack = clearlyUp;
                else if (top)
                    stack = clearlyUp || !clearlySide;
                else
                    stack = Math.Abs(stackScore) > Math.Abs(sideScore) * 1.2 && stackScore < 0;

                var mate = PickMate(host, newOrient, newW, newH, left, top, stack);
                var local = CornerLocalMeters(mate, newW, newH);
                var offset = local.TransformBy(newOrient) - Point3d.Origin;
                return host.World - offset;
            }

            private static string PickMate(
                HostVertex host, Matrix3d newOrient, double newW, double newH,
                bool left, bool top, bool stack)
            {
                var names = new[] { "V_BL", "V_BR", "V_TL", "V_TR" };
                var best = "V_BL";
                var bestScore = double.MinValue;
                foreach (var name in names)
                {
                    var off = CornerLocalMeters(name, newW, newH).TransformBy(newOrient) - Point3d.Origin;
                    var w = off.DotProduct(host.WidthDir);
                    var h = off.DotProduct(host.HeightDir);
                    var z = off.Z;
                    double score;
                    if (stack)
                    {
                        var lr = left ? -w : w;
                        var tb = top ? -h : h;
                        score = tb * 1e6 + lr;
                    }
                    else
                    {
                        var lr = left ? w : -w;
                        var tb = top ? z : -z;
                        score = lr * 1e6 + tb;
                    }
                    if (score > bestScore)
                    {
                        bestScore = score;
                        best = name;
                    }
                }
                return best;
            }

            private void ScreenAxisScores(HostVertex host, Point3d cursor, out double sideScore, out double stackScore)
            {
                sideScore = 0;
                stackScore = 0;
                try
                {
                    var vp = Convert.ToInt32(AcadApp.GetSystemVariable("CVPORT"));
                    var hs = _ed.PointToScreen(host.World, vp);
                    var cs = _ed.PointToScreen(cursor, vp);
                    var step = 250.0;
                    var wEnd = _ed.PointToScreen(host.World + host.WidthDir * step, vp);
                    var hEnd = _ed.PointToScreen(host.World + host.HeightDir * step, vp);
                    var sdx = cs.X - hs.X;
                    var sdy = cs.Y - hs.Y;
                    sideScore = sdx * (wEnd.X - hs.X) + sdy * (wEnd.Y - hs.Y);
                    stackScore = sdx * (hEnd.X - hs.X) + sdy * (hEnd.Y - hs.Y);
                    var id = (host.Id ?? "").ToUpperInvariant();
                    if (id == "V_BL" || id == "V_TL")
                        sideScore = -sideScore;
                }
                catch
                {
                }
            }

            private double ScreenDist(Point3d a, Point3d b)
            {
                try
                {
                    var vp = Convert.ToInt32(AcadApp.GetSystemVariable("CVPORT"));
                    var sa = _ed.PointToScreen(a, vp);
                    var sb = _ed.PointToScreen(b, vp);
                    var dx = sa.X - sb.X;
                    var dy = sa.Y - sb.Y;
                    return Math.Sqrt(dx * dx + dy * dy);
                }
                catch
                {
                    return ViewPlaneDist(a, b);
                }
            }

            private double ViewPlaneDist(Point3d a, Point3d b)
            {
                try
                {
                    using (var view = _ed.GetCurrentView())
                    {
                        var z = view.ViewDirection;
                        if (z.IsZeroLength())
                            z = Vector3d.ZAxis;
                        else
                            z = z.GetNormal();
                        var x = Math.Abs(z.DotProduct(Vector3d.ZAxis)) > 0.999
                            ? Vector3d.XAxis
                            : Vector3d.ZAxis.CrossProduct(z).GetNormal();
                        var y = z.CrossProduct(x).GetNormal();
                        if (Math.Abs(view.ViewTwist) > 1e-9)
                        {
                            var twist = Matrix3d.Rotation(-view.ViewTwist, z, view.Target);
                            x = x.TransformBy(twist);
                            y = y.TransformBy(twist);
                        }
                        var v = a - b;
                        var dx = v.DotProduct(x);
                        var dy = v.DotProduct(y);
                        return Math.Sqrt(dx * dx + dy * dy);
                    }
                }
                catch
                {
                    return a.DistanceTo(b);
                }
            }

            public void WorldDraw(WorldDraw draw, Point3d? hot)
            {
                if (draw == null || _hosts.Count == 0)
                    return;
                var mark = CadUnits.FromMillimeters(180);
                foreach (var p in _hosts)
                {
                    var isHot = hot.HasValue && p.World.DistanceTo(hot.Value) < 1e-4;
                    var r = isHot ? mark * 1.7 : mark;
                    draw.SubEntityTraits.Color = (short)(isHot ? 30 : 150);
                    draw.Geometry.Circle(p.World, r, Vector3d.ZAxis);
                    draw.Geometry.Circle(p.World, r, Vector3d.XAxis);
                    draw.Geometry.Circle(p.World, r, Vector3d.YAxis);
                    if (isHot)
                    {
                        draw.Geometry.Circle(p.World, r * 0.4, Vector3d.ZAxis);
                        draw.Geometry.Circle(p.World, r * 0.4, Vector3d.XAxis);
                        draw.Geometry.Circle(p.World, r * 0.4, Vector3d.YAxis);
                    }
                }
            }
        }
    }
}
