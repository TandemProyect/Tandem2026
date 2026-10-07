using System;
using System.Collections.Generic;
using System.Globalization;
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
    /// INSERT de panel ATK-60. En un muro recto hay 4 marcas inferiores
    /// (2 caras × 2 extremos). El vértice elegido fija cara y sentido;
    /// el simétrico va a la cara contraria. Si ya hay paneles, también
    /// engancha a sus vértices. Salvar persiste en BD.
    /// </summary>
    public class BlockInsertCommand
    {
        public const string CommandName = "TANDEM_INSERTBLOQUE";
        internal const string AppName = "TANDEM";
        private static BlockInsertRequest _pending;
        private static BlockInsertRequest _last;

        public static void QueueFromPalette(JObject obj)
        {
            _pending = BlockInsertRequest.FromJson(obj);
            if (_pending != null)
                _last = _pending;
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
            var req = _pending ?? _last;
            _pending = null;
            Document doc = AcadApp.DocumentManager.MdiActiveDocument;
            if (doc == null) return;
            Editor ed = doc.Editor;

            if (req == null || string.IsNullOrWhiteSpace(req.CodeName))
            {
                ed.WriteMessage("\n[Tandem] No hay panel para insertar. Elige uno en la biblioteca.\n");
                return;
            }

            _last = req;

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
            var walls = CollectAxisHosts(doc.Database, hosts);
            AddStraightWallMarks(hosts, walls);

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
                var placed = DragVisiblePanel(doc, ed, blockId, req, meterToDwg, newW, newH, overlay, walls, out snapped);
                if (!placed)
                    return;
                ed.WriteMessage("\n[Tandem] Insertado " + blockName
                    + (snapped ? " (enganchado)" : "")
                    + ". Intro para insertar otro igual.\n");
            }
            catch (System.Exception ex)
            {
                ed.WriteMessage("\n[Tandem] No se pudo insertar: " + ex.Message + "\n");
            }
        }

        internal static void PanelSizeMeters(string codeName, out double width, out double height)
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
            public long WallDbId;
            public ObjectId WallLineId;
            public bool FromWall;
        }

        private sealed class AxisHost
        {
            public ObjectId LineId;
            public Point3d A;
            public Point3d B;
            public Point3d BottomLeft;
            public Point3d BottomRight;
            public Vector3d Along;
            public Vector3d ThickDir;
            public double Thickness;
            public long WallDbId;
            public bool IsSpecial;
            public bool HasPanels;
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
                    if (string.Equals(br.Layer, WallArticleCad.LayerFormwork, StringComparison.OrdinalIgnoreCase))
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
                    Vector3d wDir;
                    Vector3d hDir;
                    Vector3d tDir;
                    WallAlignedAxes(tf, out wDir, out hDir, out tDir);
                    var wallId = WallIdFromInsert(br);
                    foreach (var snap in snaps)
                    {
                        pts.Add(new HostVertex
                        {
                            World = snap.LocalMeters.TransformBy(tf),
                            Id = snap.Id,
                            WidthDir = wDir,
                            HeightDir = hDir,
                            ThickDir = tDir,
                            WallDbId = wallId
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

        private static List<AxisHost> CollectAxisHosts(Database db, IList<HostVertex> hosts)
        {
            var walls = new List<AxisHost>();
            using (var tr = db.TransactionManager.StartTransaction())
            {
                var bt = (BlockTable)tr.GetObject(db.BlockTableId, OpenMode.ForRead);
                var ms = (BlockTableRecord)tr.GetObject(bt[BlockTableRecord.ModelSpace], OpenMode.ForRead);
                foreach (ObjectId id in ms)
                {
                    var line = tr.GetObject(id, OpenMode.ForRead) as Line;
                    if (line == null)
                        continue;
                    if (!string.Equals(line.Layer, WallCadXData.LayerAxis, StringComparison.OrdinalIgnoreCase))
                        continue;
                    var a = line.StartPoint;
                    var b = line.EndPoint;
                    if (a.DistanceTo(b) < 1e-9)
                        continue;

                    string role;
                    int groupId;
                    double thickness;
                    long wallDbId;
                    bool isSpecial;
                    WallCadXData.Read(line, out role, out groupId, out thickness, out wallDbId, out isSpecial);
                    Point3d bl;
                    Point3d br;
                    PickBottom(a, b, out bl, out br);
                    var along = br - bl;
                    if (along.Length < 1e-9)
                        continue;
                    along = along.GetNormal();
                    var thick = new Vector3d(-along.Y, along.X, 0);
                    if (thick.Length < 1e-9)
                        thick = Vector3d.YAxis;
                    else
                        thick = thick.GetNormal();
                    walls.Add(new AxisHost
                    {
                        LineId = id,
                        A = a,
                        B = b,
                        BottomLeft = bl,
                        BottomRight = br,
                        Along = along,
                        ThickDir = thick,
                        Thickness = thickness,
                        WallDbId = wallDbId,
                        IsSpecial = isSpecial
                    });
                }
                tr.Commit();
            }

            var panelTol = CadUnits.FromMillimeters(80);
            foreach (var wall in walls)
            {
                var band = Math.Max(wall.Thickness * 1.6, panelTol);
                foreach (var host in hosts)
                {
                    if (host.FromWall)
                        continue;
                    if (host.WallDbId > 0 && wall.WallDbId > 0)
                    {
                        if (host.WallDbId == wall.WallDbId)
                        {
                            wall.HasPanels = true;
                            break;
                        }
                        continue;
                    }
                    if (WallCadXData.DistToSegment(host.World, wall.A, wall.B) <= band)
                    {
                        wall.HasPanels = true;
                        break;
                    }
                }
            }

            return walls;
        }

        /// <summary>
        /// Cuatro marcas en la base del prisma: dos caras × dos extremos.
        /// En un L se apartan del nudo para que el snap sea del muro, no de la esquina.
        /// Cara +: izquierda p1, derecha p2, sentido +Along.
        /// Cara −: izquierda p2, derecha p1, sentido −Along (el contrario).
        /// </summary>
        private static void AddStraightWallMarks(IList<HostVertex> hosts, IList<AxisHost> walls)
        {
            if (hosts == null || walls == null)
                return;
            var joinTol = CadUnits.FromMillimeters(40);
            foreach (var wall in walls)
            {
                Vector3d n;
                double half;
                if (!TryWallNormal(wall, out n, out half))
                    continue;
                var along = new Vector3d(wall.Along.X, wall.Along.Y, 0);
                if (along.Length < 1e-9)
                    continue;
                along = along.GetNormal();
                var len = wall.BottomLeft.DistanceTo(wall.BottomRight);
                var cap = Math.Min(CadUnits.FromMillimeters(150), len * 0.2);
                var dStart = EndShared(wall, wall.BottomLeft, walls, joinTol) ? cap : 0;
                var dEnd = EndShared(wall, wall.BottomRight, walls, joinTol) ? cap : 0;
                if (dStart + dEnd > len * 0.6)
                {
                    dStart = 0;
                    dEnd = 0;
                }

                var p1 = wall.BottomLeft + along * dStart;
                var p2 = wall.BottomRight - along * dEnd;
                var z = Math.Min(wall.BottomLeft.Z, wall.BottomRight.Z);
                hosts.Add(WallMark(wall, p1, n, along, half, z, "V_BL"));
                hosts.Add(WallMark(wall, p2, n, along, half, z, "V_BR"));
                hosts.Add(WallMark(wall, p2, -n, -along, half, z, "V_BL"));
                hosts.Add(WallMark(wall, p1, -n, -along, half, z, "V_BR"));
            }
        }

        private static HostVertex WallMark(
            AxisHost wall, Point3d onAxis, Vector3d thick, Vector3d width,
            double half, double z, string id)
        {
            return new HostVertex
            {
                World = new Point3d(onAxis.X + thick.X * half, onAxis.Y + thick.Y * half, z),
                Id = id,
                WidthDir = width,
                HeightDir = Vector3d.ZAxis,
                ThickDir = thick,
                WallDbId = wall.WallDbId,
                WallLineId = wall.LineId,
                FromWall = true
            };
        }

        private static bool EndShared(AxisHost wall, Point3d end, IList<AxisHost> walls, double tol)
        {
            foreach (var other in walls)
            {
                if (ReferenceEquals(other, wall))
                    continue;
                if (NearXY(end, other.BottomLeft, tol) || NearXY(end, other.BottomRight, tol))
                    return true;
            }
            return false;
        }

        private static bool NearXY(Point3d a, Point3d b, double tol)
        {
            var dx = a.X - b.X;
            var dy = a.Y - b.Y;
            return dx * dx + dy * dy <= tol * tol;
        }

        private static double DistXY(Point3d a, Point3d b)
        {
            var dx = a.X - b.X;
            var dy = a.Y - b.Y;
            return Math.Sqrt(dx * dx + dy * dy);
        }

        private static Point3d PlaceAtWallVertex(
            HostVertex host, Matrix3d orient, double newW, double newH, bool tumbado)
        {
            if (host == null)
                return Point3d.Origin;
            var id = (host.Id ?? "").ToUpperInvariant();
            Point3d local;
            if (tumbado)
            {
                switch (id)
                {
                    case "V_BR":
                        local = new Point3d(0, 0, newH);
                        break;
                    case "V_TR":
                        local = new Point3d(newW, 0, newH);
                        break;
                    case "V_TL":
                        local = new Point3d(newW, 0, 0);
                        break;
                    default:
                        local = Point3d.Origin;
                        break;
                }
            }
            else
            {
                local = CornerLocalMeters(id, newW, newH);
            }
            var offset = local.TransformBy(orient) - Point3d.Origin;
            return host.World - offset;
        }

        /// <summary>
        /// Eje canónico horario: p1→p2 hacia +X; si es vertical, hacia +Y.
        /// Igual que CanonicalizeClockwiseWallAxis del encofrado automático.
        /// </summary>
        internal static void PickBottom(Point3d a, Point3d b, out Point3d left, out Point3d right)
        {
            if (a.X < b.X - 1e-6 || (Math.Abs(a.X - b.X) <= 1e-6 && a.Y <= b.Y))
            {
                left = a;
                right = b;
                return;
            }

            left = b;
            right = a;
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
            IList<AxisHost> walls,
            out bool snapped)
        {
            snapped = false;
            var db = doc.Database;
            var tumbado = req.RotationDeg == 90;
            var jig = new PanelDragJig(blockId, overlay, walls, meterToDwg, tumbado, newW, newH);
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
            var wall = jig.Wall;
            var wallDbId = wall != null ? wall.WallDbId : 0;
            var widthDwg = newW * meterToDwg;
            var alongDwg = (jig.RotationDeg == 90 ? newH : newW) * meterToDwg;
            var emptyStanding = wall != null && !wall.HasPanels && jig.RotationDeg != 90;
            var at1 = jig.At;
            var orient1 = jig.Orient;
            var at2 = jig.At;
            var orient2 = jig.Orient;
            var hasMirror = false;
            if (emptyStanding && !jig.SnapToPanel)
            {
                hasMirror = TryFormworkFacePoses(
                    wall, jig.At, meterToDwg, false, widthDwg,
                    out at1, out orient1, out at2, out orient2);
            }
            else if (wall != null)
            {
                // El 180° del simétrico da la vuelta al panel. El origen se corre
                // el largo de la pieza (0,90 de pie, 2,70 tumbado) para que la
                // huella siga en la misma estación. Un panel de pie no ocupa
                // el hueco de uno a 90°.
                hasMirror = TryOppositeFromSnapped(jig.At, jig.Orient, wall, alongDwg, out at2, out orient2)
                    && !PoseOccupied(doc.Database, at2, CadUnits.FromMillimeters(40), wallDbId, jig.RotationDeg);
            }

            using (doc.LockDocument())
            using (var tr = db.TransactionManager.StartTransaction())
            {
                var ms = (BlockTableRecord)tr.GetObject(db.CurrentSpaceId, OpenMode.ForWrite);
                var spin = emptyStanding;
                AppendPanel(ms, tr, db, blockId, at1, orient1, req, jig.RotationDeg, wallDbId, widthDwg, spin);
                if (hasMirror && at2.DistanceTo(at1) >= CadUnits.FromMillimeters(20))
                    AppendPanel(ms, tr, db, blockId, at2, orient2, req, jig.RotationDeg, wallDbId, alongDwg, spin);
                if (wall != null && !wall.LineId.IsNull)
                    StampWallSpecial(tr, wall.LineId, wallDbId);
                tr.Commit();
            }
            return true;
        }

        private static void StampWallSpecial(Transaction tr, ObjectId wallLineId, long wallDbId)
        {
            var line = tr.GetObject(wallLineId, OpenMode.ForWrite) as Line;
            if (line == null)
                return;
            string role;
            int groupId;
            double thickness;
            long oldId;
            bool special;
            WallCadXData.Read(line, out role, out groupId, out thickness, out oldId, out special);
            WallCadXData.Write(line, role, groupId, thickness, wallDbId > 0 ? wallDbId : oldId, true);
        }

        private static ObjectId AppendPanel(
            BlockTableRecord ms,
            Transaction tr,
            Database db,
            ObjectId blockId,
            Point3d at,
            Matrix3d orient,
            BlockInsertRequest req,
            int rotDeg,
            long wallDbId,
            double widthDwg,
            bool formworkSpin)
        {
            var br = new BlockReference(Point3d.Origin, blockId);
            if (formworkSpin)
                ApplyFormworkPanelMatrix(br, at, orient, widthDwg);
            else
                ApplyPanelMatrix(br, at, orient);
            ms.AppendEntity(br);
            tr.AddNewlyCreatedDBObject(br, true);
            ApplyAtkXData(br, tr, db, req.CodeName, req.View ?? "3dref", "PANEL", rotDeg, wallDbId, at);
            return br.ObjectId;
        }

        /// <summary>
        /// Misma regla que el encofrado automático (Modulo270PanelElementGenerator):
        /// eje canónico horario (p1→p2 hacia +X, o +Y si es vertical), inserto en
        /// inferior-izquierda de la cara, 180° en la cara opuesta y el punto avanza
        /// una anchura de pieza para no desplazar la huella.
        /// </summary>
        private static bool TryFormworkFacePoses(
            AxisHost wall,
            Point3d cursor,
            double scale,
            bool tumbado,
            double widthDwg,
            out Point3d at1,
            out Matrix3d orient1,
            out Point3d at2,
            out Matrix3d orient2)
        {
            at1 = cursor;
            at2 = cursor;
            orient1 = PanelOrient(scale, tumbado);
            orient2 = orient1;
            if (wall == null)
                return false;

            Vector3d n;
            double half;
            if (!TryWallNormal(wall, out n, out half))
                return false;

            var along = new Vector3d(wall.Along.X, wall.Along.Y, 0);
            if (along.Length < 1e-9)
                return false;
            along = along.GetNormal();
            var yaw = Math.Atan2(along.Y, along.X);
            var outward = FaceOutward(wall, cursor);
            var faceIsRightSide = outward.DotProduct(n) < 0;
            var start = wall.BottomLeft;
            var basePt = new Point3d(
                start.X + outward.X * half,
                start.Y + outward.Y * half,
                start.Z);
            var shiftX = along.X * widthDwg;
            var shiftY = along.Y * widthDwg;

            at1 = faceIsRightSide
                ? new Point3d(basePt.X + shiftX, basePt.Y + shiftY, basePt.Z)
                : basePt;
            orient1 = Matrix3d.Rotation(yaw + (faceIsRightSide ? Math.PI : 0), Vector3d.ZAxis, Point3d.Origin)
                * PanelOrient(scale, tumbado);

            var thickness = half * 2.0;
            var oppBase = new Point3d(
                basePt.X - outward.X * thickness,
                basePt.Y - outward.Y * thickness,
                basePt.Z);
            at2 = !faceIsRightSide
                ? new Point3d(oppBase.X + shiftX, oppBase.Y + shiftY, oppBase.Z)
                : oppBase;
            orient2 = Matrix3d.Rotation(yaw + (!faceIsRightSide ? Math.PI : 0), Vector3d.ZAxis, Point3d.Origin)
                * PanelOrient(scale, tumbado);

            return at1.DistanceTo(at2) >= CadUnits.FromMillimeters(20);
        }

        private static bool TryOppositeFromSnapped(
            Point3d at,
            Matrix3d orient,
            AxisHost wall,
            double alongDwg,
            out Point3d at2,
            out Matrix3d orient2)
        {
            at2 = at;
            orient2 = orient;
            if (wall == null)
                return false;
            Vector3d n;
            double half;
            if (!TryWallNormal(wall, out n, out half))
                return false;
            var p = ProjectOnAxis(at, wall);
            var side = (at.X - p.X) * n.X + (at.Y - p.Y) * n.Y;
            at2 = new Point3d(at.X - 2.0 * side * n.X, at.Y - 2.0 * side * n.Y, at.Z);
            var alongPanel = OrientIsTumbado(orient)
                ? Vector3d.ZAxis.TransformBy(orient)
                : Vector3d.XAxis.TransformBy(orient);
            var along = new Vector3d(alongPanel.X, alongPanel.Y, 0);
            if (along.Length < 1e-9)
                along = new Vector3d(wall.Along.X, wall.Along.Y, 0);
            if (along.Length > 1e-9)
                along = along.GetNormal();
            at2 = new Point3d(at2.X + along.X * alongDwg, at2.Y + along.Y * alongDwg, at2.Z);
            orient2 = Matrix3d.Rotation(Math.PI, Vector3d.ZAxis, Point3d.Origin) * orient;
            return at2.DistanceTo(at) >= CadUnits.FromMillimeters(20);
        }

        private static bool PoseOccupied(Database db, Point3d at, double tol, long wallDbId, int rotDeg)
        {
            if (db == null)
                return false;
            using (var tr = db.TransactionManager.StartTransaction())
            {
                var bt = (BlockTable)tr.GetObject(db.BlockTableId, OpenMode.ForRead);
                var ms = (BlockTableRecord)tr.GetObject(bt[BlockTableRecord.ModelSpace], OpenMode.ForRead);
                foreach (ObjectId id in ms)
                {
                    var br = tr.GetObject(id, OpenMode.ForRead) as BlockReference;
                    if (br == null)
                        continue;
                    if (string.Equals(br.Layer, WallArticleCad.LayerFormwork, StringComparison.OrdinalIgnoreCase))
                        continue;
                    string code;
                    string view;
                    string role;
                    int rot;
                    if (!TryReadAtk(br, out code, out view, out role, out rot))
                        continue;
                    if (rot != rotDeg)
                        continue;
                    var otherWall = WallIdFromInsert(br);
                    if (wallDbId > 0 && otherWall > 0 && otherWall != wallDbId)
                        continue;
                    if (InsertWorld(br).DistanceTo(at) <= tol)
                    {
                        tr.Commit();
                        return true;
                    }
                }
                tr.Commit();
            }
            return false;
        }

        private static void WallAlignedAxes(
            Matrix3d tf, out Vector3d wDir, out Vector3d hDir, out Vector3d tDir)
        {
            var x = Vector3d.XAxis.TransformBy(tf);
            var y = Vector3d.YAxis.TransformBy(tf);
            var z = Vector3d.ZAxis.TransformBy(tf);
            tDir = y.Length > 1e-9 ? y.GetNormal() : Vector3d.YAxis;
            if (OrientIsTumbado(tf))
            {
                hDir = x.Length > 1e-9 ? x.GetNormal() : Vector3d.ZAxis;
                var along = new Vector3d(z.X, z.Y, 0);
                if (along.Length < 1e-9)
                    along = new Vector3d(-hDir.Y, hDir.X, 0);
                wDir = along.Length > 1e-9 ? along.GetNormal() : Vector3d.XAxis;
                return;
            }

            wDir = x.Length > 1e-9 ? x.GetNormal() : Vector3d.XAxis;
            hDir = z.Length > 1e-9 ? z.GetNormal() : Vector3d.ZAxis;
        }

        private static bool TryWallNormal(AxisHost wall, out Vector3d n, out double half)
        {
            n = Vector3d.YAxis;
            half = 0;
            if (wall == null)
                return false;
            n = new Vector3d(wall.ThickDir.X, wall.ThickDir.Y, 0);
            if (n.Length < 1e-9)
                return false;
            n = n.GetNormal();
            var thickness = wall.Thickness;
            if (thickness < CadUnits.FromMillimeters(20))
                thickness = CadUnits.FromMillimeters(300);
            half = thickness * 0.5;
            return half >= CadUnits.FromMillimeters(10);
        }

        private static Point3d ProjectOnAxis(Point3d at, AxisHost wall)
        {
            var a = wall.A;
            var b = wall.B;
            var ab = new Vector3d(b.X - a.X, b.Y - a.Y, 0);
            var ap = new Vector3d(at.X - a.X, at.Y - a.Y, 0);
            var len2 = ab.DotProduct(ab);
            var t = 0d;
            if (len2 > 1e-18)
                t = Math.Max(0, Math.Min(1, ap.DotProduct(ab) / len2));
            return new Point3d(a.X + ab.X * t, a.Y + ab.Y * t, at.Z);
        }

        private static Vector3d FaceOutward(AxisHost wall, Point3d from)
        {
            Vector3d n;
            double half;
            if (!TryWallNormal(wall, out n, out half))
                return Vector3d.YAxis;
            var p = ProjectOnAxis(from, wall);
            var side = (from.X - p.X) * n.X + (from.Y - p.Y) * n.Y;
            return side < 0 ? -n : n;
        }

        internal static void ArticleRotations(
            Point3d at,
            Point3d axisA,
            Point3d axisB,
            Matrix3d orient,
            out int rotX,
            out int rotY,
            out double rotZ)
        {
            rotX = OrientIsTumbado(orient) ? 90 : 0;
            Point3d left;
            Point3d right;
            PickBottom(axisA, axisB, out left, out right);
            var along = new Vector3d(right.X - left.X, right.Y - left.Y, 0);
            rotZ = 0;
            if (along.Length > 1e-9)
            {
                along = along.GetNormal();
                rotZ = Math.Atan2(along.Y, along.X) * 180.0 / Math.PI;
            }
            var n = new Vector3d(-along.Y, along.X, 0);
            if (n.Length < 1e-9)
            {
                rotY = 0;
                return;
            }
            n = n.GetNormal();
            var ap = new Vector3d(at.X - left.X, at.Y - left.Y, 0);
            var ab = new Vector3d(right.X - left.X, right.Y - left.Y, 0);
            var len2 = ab.DotProduct(ab);
            var t = 0d;
            if (len2 > 1e-18)
                t = Math.Max(0, Math.Min(1, ap.DotProduct(ab) / len2));
            var px = left.X + ab.X * t;
            var py = left.Y + ab.Y * t;
            var side = (at.X - px) * n.X + (at.Y - py) * n.Y;
            rotY = side < 0 ? 180 : 0;
        }

        internal static double YawDegFromOrient(Matrix3d orient)
        {
            var tumbado = OrientIsTumbado(orient);
            var along = tumbado
                ? Vector3d.ZAxis.TransformBy(orient)
                : Vector3d.XAxis.TransformBy(orient);
            var v = new Vector3d(along.X, along.Y, 0);
            if (v.Length < 1e-9)
            {
                var y = Vector3d.YAxis.TransformBy(orient);
                v = new Vector3d(-y.Y, y.X, 0);
            }
            if (v.Length < 1e-9)
                return 0;
            return Math.Atan2(v.Y, v.X) * 180.0 / Math.PI;
        }

        /// <summary>
        /// Giro en planta del bloque ya insertado, el que deja la cara Y=0
        /// contra el muro. No es el rumbo del eje: un panel enganchado a otro
        /// que ya dio la vuelta no coincide con RotationY.
        /// 4000 + yaw = pose visual. +1000 más = el primer panel de muro vacío,
        /// que lleva el 180° de encofrado sobre el centro.
        /// </summary>
        internal static double EncodePoseZ(BlockReference br)
        {
            var yaw = VisualYawDeg(br.BlockTransform);
            var spun = false;
            if (!OrientIsTumbado(br.BlockTransform))
            {
                var delta = br.Position.DistanceTo(InsertWorld(br));
                spun = delta > CadUnits.FromMillimeters(50);
            }
            return yaw + 4000.0 + (spun ? 1000.0 : 0.0);
        }

        internal static void DecodePoseZ(double raw, out double yawDeg, out bool spun, out bool encoded)
        {
            spun = false;
            encoded = Math.Abs(raw) >= 3000.0;
            if (!encoded)
            {
                yawDeg = raw;
                return;
            }
            var sign = raw < 0 ? -1.0 : 1.0;
            if (Math.Abs(raw) >= 4500.0)
            {
                spun = true;
                yawDeg = raw - sign * 5000.0;
            }
            else
                yawDeg = raw - sign * 4000.0;
        }

        private static double VisualYawDeg(Matrix3d tf)
        {
            var y = Vector3d.YAxis.TransformBy(tf);
            var v = new Vector3d(y.X, y.Y, 0);
            if (v.Length < 1e-9)
                return 0;
            v = v.GetNormal();
            return Math.Atan2(-v.X, v.Y) * 180.0 / Math.PI;
        }

        internal static bool OrientIsMirrored(Matrix3d orient)
        {
            var x = Vector3d.XAxis.TransformBy(orient);
            var y = Vector3d.YAxis.TransformBy(orient);
            var z = Vector3d.ZAxis.TransformBy(orient);
            if (x.Length < 1e-9 || y.Length < 1e-9 || z.Length < 1e-9)
                return false;
            return x.CrossProduct(y).DotProduct(z) < 0;
        }

        private sealed class PanelDragJig : DrawJig
        {
            private readonly ObjectId _blockId;
            private readonly VertexOverlay _overlay;
            private readonly IList<AxisHost> _walls;
            private readonly double _scale;
            private readonly bool _tumbado;
            private readonly double _newW;
            private readonly double _newH;
            private Point3d _at = Point3d.Origin;
            private Point3d? _hot;
            private Matrix3d _orient;
            private BlockReference _ghost;
            private AxisHost _wall;

            public PanelDragJig(
                ObjectId blockId,
                VertexOverlay overlay,
                IList<AxisHost> walls,
                double scale,
                bool tumbado,
                double newW,
                double newH)
            {
                _blockId = blockId;
                _overlay = overlay;
                _walls = walls ?? new List<AxisHost>();
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
            public bool SnapToPanel { get; private set; }
            public AxisHost Wall { get { return _wall; } }

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
                    "\nElige un vértice inferior del muro (4 marcas). ESC cancela: ");
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
                var nearPanel = _overlay.TryNearestPanel(res.Value, out hit);
                var wall = NearestWall(res.Value, true);
                HostVertex wallHit = null;
                var nearWallMark = false;
                if (wall != null)
                {
                    var maxD = wall.HasPanels
                        ? Math.Max(wall.Thickness * 1.8, CadUnits.FromMillimeters(250))
                        : double.MaxValue;
                    nearWallMark = _overlay.TryNearestOnWallFace(wall, res.Value, maxD, out wallHit);
                }

                Matrix3d orient;
                int rotDeg;
                Point3d next;
                Point3d? hot;
                bool snap;
                bool snapPanel;
                if (wall != null && wall.HasPanels && nearPanel)
                {
                    orient = OrientOnHost(hit, _scale, _tumbado);
                    rotDeg = _tumbado ? 90 : 0;
                    next = _overlay.Place(hit, res.Value, orient, _newW, _newH);
                    hot = hit.World;
                    snap = true;
                    snapPanel = true;
                }
                else if (nearWallMark)
                {
                    orient = OrientOnHost(wallHit, _scale, _tumbado);
                    rotDeg = _tumbado ? 90 : 0;
                    wall = WallOf(wallHit) ?? wall;
                    next = PlaceAtWallVertex(wallHit, orient, _newW, _newH, _tumbado);
                    hot = wallHit.World;
                    snap = true;
                    snapPanel = true;
                }
                else if (nearPanel)
                {
                    orient = OrientOnHost(hit, _scale, _tumbado);
                    rotDeg = _tumbado ? 90 : 0;
                    next = _overlay.Place(hit, res.Value, orient, _newW, _newH);
                    hot = hit.World;
                    snap = true;
                    snapPanel = true;
                    wall = WallOf(hit);
                }
                else if (wall != null)
                {
                    orient = MatchHostSentido(HostFromWall(wall), PanelOrient(_scale, _tumbado));
                    rotDeg = _tumbado ? 90 : 0;
                    next = res.Value;
                    hot = null;
                    snap = false;
                    snapPanel = false;
                }
                else
                {
                    orient = PanelOrient(_scale, _tumbado);
                    rotDeg = _tumbado ? 90 : 0;
                    next = res.Value;
                    hot = null;
                    snap = false;
                    snapPanel = false;
                }

                if (next.IsEqualTo(_at, new Tolerance(1e-6, 1e-6)) && Snapped == snap
                    && RotationDeg == rotDeg
                    && SameOrient(_orient, orient)
                    && hot.HasValue == _hot.HasValue
                    && (!hot.HasValue || hot.Value.IsEqualTo(_hot.Value, new Tolerance(1e-6, 1e-6)))
                    && ReferenceEquals(_wall, wall))
                    return SamplerStatus.NoChange;
                _at = next;
                _hot = hot;
                _orient = orient;
                RotationDeg = rotDeg;
                Snapped = snap;
                SnapToPanel = snapPanel;
                _wall = wall;
                return SamplerStatus.OK;
            }

            protected override bool WorldDraw(WorldDraw draw)
            {
                if (draw == null)
                    return true;
                if (_ghost == null)
                    _ghost = new BlockReference(Point3d.Origin, _blockId);
                if (_wall != null && !_tumbado && !_wall.HasPanels)
                    ApplyFormworkPanelMatrix(_ghost, _at, _orient, _newW * _scale);
                else
                    ApplyPanelMatrix(_ghost, _at, _orient);
                draw.Geometry.Draw(_ghost);
                _overlay.WorldDraw(draw, _hot);
                return true;
            }

            private AxisHost NearestWall(Point3d cursor, bool requireNear)
            {
                AxisHost best = null;
                var bestDist = double.MaxValue;
                foreach (var w in _walls)
                {
                    var d = WallCadXData.DistToSegment(cursor, w.A, w.B);
                    var tol = Math.Max(w.Thickness * 2.2, CadUnits.FromMillimeters(400));
                    if (requireNear && d > tol)
                        continue;
                    if (!requireNear && d > tol)
                        continue;
                    if (d < bestDist)
                    {
                        bestDist = d;
                        best = w;
                    }
                }
                return best;
            }

            private AxisHost WallOf(HostVertex hit)
            {
                if (hit == null)
                    return null;
                if (!hit.WallLineId.IsNull)
                {
                    foreach (var w in _walls)
                    {
                        if (w.LineId == hit.WallLineId)
                            return w;
                    }
                }
                if (hit.WallDbId > 0)
                {
                    foreach (var w in _walls)
                    {
                        if (w.WallDbId == hit.WallDbId)
                            return w;
                    }
                }

                return NearestWall(hit.World, true);
            }

            private static HostVertex HostFromWall(AxisHost wall)
            {
                return new HostVertex
                {
                    World = wall.BottomLeft,
                    Id = "V_BL",
                    WidthDir = wall.Along,
                    HeightDir = Vector3d.ZAxis,
                    ThickDir = wall.ThickDir,
                    WallDbId = wall.WallDbId,
                    WallLineId = wall.LineId,
                    FromWall = true
                };
            }
        }

        internal static Matrix3d PanelOrient(double scale, bool tumbado)
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

        internal static bool OrientIsTumbado(Matrix3d orient)
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

        internal static void ApplyPanelMatrix(BlockReference br, Point3d at, Matrix3d orient)
        {
            br.BlockTransform = Matrix3d.Displacement(at.GetAsVector()) * orient;
        }

        /// <summary>
        /// El DWG mira al revés que el STL: 180° en vertical sobre el centro
        /// de la base, para no desplazar la huella del muro (mismo criterio que FormworkCommand).
        /// </summary>
        internal static void ApplyFormworkPanelMatrix(BlockReference br, Point3d at, Matrix3d orient, double widthDwg)
        {
            var along = Vector3d.XAxis.TransformBy(orient);
            if (along.Length > 1e-9)
                along = along.GetNormal();
            var halfW = widthDwg > 1e-9 ? widthDwg * 0.5 : 0;
            var pivot = at + along * halfW;
            br.BlockTransform = Matrix3d.Rotation(Math.PI, Vector3d.ZAxis, pivot)
                * Matrix3d.Displacement(at.GetAsVector())
                * orient;
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

        internal static long WallIdFromInsert(BlockReference br)
        {
            if (br == null || br.XData == null)
                return 0;
            var inApp = false;
            foreach (var t in br.XData.AsArray())
            {
                if (t.TypeCode == 1001)
                {
                    inApp = string.Equals(t.Value as string, AppName, StringComparison.OrdinalIgnoreCase);
                    continue;
                }
                if (!inApp || t.TypeCode != 1000)
                    continue;
                var text = (t.Value as string ?? "").Trim();
                if (!text.StartsWith("WID:", StringComparison.OrdinalIgnoreCase))
                    continue;
                long id;
                if (long.TryParse(text.Substring(4), out id))
                    return id;
            }
            return 0;
        }

        internal static Point3d InsertWorld(BlockReference br)
        {
            if (br == null)
                return Point3d.Origin;
            if (br.XData != null)
            {
                var inApp = false;
                foreach (var t in br.XData.AsArray())
                {
                    if (t.TypeCode == 1001)
                    {
                        inApp = string.Equals(t.Value as string, AppName, StringComparison.OrdinalIgnoreCase);
                        continue;
                    }
                    if (!inApp || t.TypeCode != 1000)
                        continue;
                    var text = (t.Value as string ?? "").Trim();
                    if (!text.StartsWith("AT:", StringComparison.OrdinalIgnoreCase))
                        continue;
                    Point3d parsed;
                    if (TryParseInsertAt(text.Substring(3), out parsed))
                        return parsed;
                }
            }
            return br.Position;
        }

        private static bool TryParseInsertAt(string raw, out Point3d at)
        {
            at = Point3d.Origin;
            if (string.IsNullOrWhiteSpace(raw))
                return false;
            var parts = raw.Split(',');
            if (parts.Length < 3)
                return false;
            double x;
            double y;
            double z;
            if (!double.TryParse(parts[0], NumberStyles.Float, CultureInfo.InvariantCulture, out x)
                || !double.TryParse(parts[1], NumberStyles.Float, CultureInfo.InvariantCulture, out y)
                || !double.TryParse(parts[2], NumberStyles.Float, CultureInfo.InvariantCulture, out z))
                return false;
            at = new Point3d(x, y, z);
            return true;
        }

        internal static void ApplyAtkXData(
            BlockReference br, Transaction tr, Database db,
            string codeName, string view, string role, int rotDeg, long wallDbId = 0)
        {
            ApplyAtkXData(br, tr, db, codeName, view, role, rotDeg, wallDbId, null);
        }

        internal static void ApplyAtkXData(
            BlockReference br, Transaction tr, Database db,
            string codeName, string view, string role, int rotDeg, long wallDbId, Point3d? insertAt)
        {
            EnsureRegApp(tr, db);
            var values = new List<TypedValue>
            {
                new TypedValue(1001, AppName),
                new TypedValue(1000, codeName ?? ""),
                new TypedValue(1000, view ?? "3dref"),
                new TypedValue(1000, role ?? "PANEL"),
                new TypedValue(1070, rotDeg)
            };
            if (wallDbId > 0)
                values.Add(new TypedValue(1000, "WID:" + wallDbId.ToString()));
            if (insertAt.HasValue)
            {
                var p = insertAt.Value;
                values.Add(new TypedValue(
                    1000,
                    string.Format(CultureInfo.InvariantCulture, "AT:{0:R},{1:R},{2:R}", p.X, p.Y, p.Z)));
            }
            br.XData = new ResultBuffer(values.ToArray());
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

            public bool TryNearestPanel(Point3d cursor, out HostVertex nearest)
            {
                nearest = null;
                var best = double.MaxValue;
                foreach (var p in _hosts)
                {
                    if (p.FromWall)
                        continue;
                    var d = ScreenDist(cursor, p.World);
                    if (d < best)
                    {
                        best = d;
                        nearest = p;
                    }
                }
                return nearest != null && best <= PixelTol;
            }

            public bool TryNearestOnWallFace(AxisHost wall, Point3d cursor, double maxDist, out HostVertex nearest)
            {
                nearest = null;
                if (wall == null)
                    return false;
                var outward = FaceOutward(wall, cursor);
                var best = double.MaxValue;
                foreach (var p in _hosts)
                {
                    if (!p.FromWall || !SameWall(p, wall))
                        continue;
                    if (p.ThickDir.DotProduct(outward) <= 0)
                        continue;
                    var d = DistXY(cursor, p.World);
                    if (d < best)
                    {
                        best = d;
                        nearest = p;
                    }
                }
                return nearest != null && best <= maxDist;
            }

            private static bool SameWall(HostVertex p, AxisHost wall)
            {
                if (p == null || wall == null)
                    return false;
                if (!p.WallLineId.IsNull && !wall.LineId.IsNull)
                    return p.WallLineId == wall.LineId;
                return p.WallDbId > 0 && wall.WallDbId > 0 && p.WallDbId == wall.WallDbId;
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
                    var isWall = p.FromWall;
                    var r = isHot ? mark * 1.7 : (isWall ? mark : mark * 0.65);
                    draw.SubEntityTraits.Color = (short)(isHot ? 30 : (isWall ? 4 : 150));
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
