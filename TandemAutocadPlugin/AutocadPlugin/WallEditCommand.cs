using System;
using System.Collections.Generic;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Geometry;
using Autodesk.AutoCAD.Runtime;
using AcadApp = Autodesk.AutoCAD.ApplicationServices.Application;

[assembly: CommandClass(typeof(AutocadPlugin.WallEditCommand))]

namespace AutocadPlugin
{
    /// <summary>
    /// Doble clic en el muro: el mismo formulario que Desing (Modificar muro).
    /// </summary>
    public class WallEditCommand
    {
        private const string Layer3d = "TANDEM_MURO_3D";
        private static ObjectId _pending = ObjectId.Null;
        private static bool _hooked;

        public static void Hook()
        {
            if (_hooked)
                return;
            AcadApp.BeginDoubleClick += OnDoubleClick;
            _hooked = true;
        }

        public static void Unhook()
        {
            if (!_hooked)
                return;
            AcadApp.BeginDoubleClick -= OnDoubleClick;
            _hooked = false;
        }

        private static void OnDoubleClick(object sender, BeginDoubleClickEventArgs e)
        {
            try
            {
                if (Convert.ToInt16(AcadApp.GetSystemVariable("CMDACTIVE")) != 0)
                    return;
                var doc = AcadApp.DocumentManager.MdiActiveDocument;
                if (doc == null)
                    return;
                ObjectId axisId;
                if (!TryResolveAxis(doc, e.Location, out axisId))
                    return;
                _pending = axisId;
                doc.SendStringToExecute("\x03\x03_.TANDEM_MUROATRIBUTOS ", true, false, false);
            }
            catch
            {
            }
        }

        [CommandMethod("TANDEM_MUROATRIBUTOS")]
        public void Run()
        {
            var doc = AcadApp.DocumentManager.MdiActiveDocument;
            if (doc == null)
                return;
            var axisId = _pending;
            _pending = ObjectId.Null;
            if (axisId.IsNull || !axisId.IsValid)
            {
                doc.Editor.WriteMessage("\n[Tandem] Doble clic en el eje, la cara o el muro 3D.\n");
                return;
            }

            double lengthM;
            double widthM;
            WallEditAttrs attrs;
            int groupId;
            if (!ReadAxis(doc.Database, axisId, out lengthM, out widthM, out attrs, out groupId))
            {
                doc.Editor.WriteMessage("\n[Tandem] Ese objeto no es un muro.\n");
                return;
            }

            var form = WallFormHost.Show(lengthM, widthM, attrs, HeadingText(doc.Database, axisId), groupId);
            if (form == null || form.Attrs == null)
                return;

            try
            {
                using (doc.LockDocument())
                    Apply(doc.Database, axisId, form.LengthM, form.WidthM, form.Attrs, form.HeightAll, form.ConfigAll, form.Group);
                Wall3dCommand.RebuildFromFaces(form.HeightM * 1000.0);
                doc.Editor.WriteMessage("\n[Tandem] Atributos guardados en el muro. El sólido usa la altura y la planta nuevas. Vuelve a encofrar para los paneles.\n");
            }
            catch (System.Exception ex)
            {
                doc.Editor.WriteMessage("\n[Tandem] " + ex.Message + "\n");
            }
        }

        private static bool ReadAxis(Database db, ObjectId axisId, out double lengthM, out double widthM, out WallEditAttrs attrs, out int groupId)
        {
            lengthM = 0;
            widthM = 0.30;
            attrs = new WallEditAttrs();
            groupId = 0;
            using (var tr = db.TransactionManager.StartTransaction())
            {
                var line = tr.GetObject(axisId, OpenMode.ForRead) as Line;
                if (line == null
                    || !string.Equals(line.Layer, WallCadXData.LayerAxis, StringComparison.OrdinalIgnoreCase))
                    return false;
                string role;
                double thickness;
                long wallDbId;
                bool special;
                WallCadXData.Read(line, out role, out groupId, out thickness, out wallDbId, out special);
                lengthM = CadUnits.ToMillimeters(line.StartPoint.DistanceTo(line.EndPoint)) / 1000.0;
                widthM = CadUnits.ToMillimeters(thickness) / 1000.0;
                if (widthM < 0.02)
                    widthM = 0.30;
                attrs = WallEditAttrs.Load(WallCadXData.ReadAttrsJson(line));
                tr.Commit();
            }
            return true;
        }

        private static string HeadingText(Database db, ObjectId axisId)
        {
            using (var tr = db.TransactionManager.StartTransaction())
            {
                var line = tr.GetObject(axisId, OpenMode.ForRead) as Line;
                if (line == null)
                    return "";
                var dx = line.EndPoint.X - line.StartPoint.X;
                var dy = line.EndPoint.Y - line.StartPoint.Y;
                var deg = Math.Atan2(dy, dx) * (180.0 / Math.PI);
                var rounded = Math.Round(deg, 1);
                return "Sentido del muro: "
                    + rounded.ToString("0.0", System.Globalization.CultureInfo.InvariantCulture).Replace('.', ',')
                    + "°. El inicio es el primer punto y el final el segundo.";
            }
        }

        private static void Apply(
            Database db,
            ObjectId axisId,
            double lengthM,
            double widthM,
            WallEditAttrs attrs,
            bool heightAll,
            bool configAll,
            int? newGroup)
        {
            var json = attrs.ToJson();
            if (("ATR:" + json).Length > 255)
                throw new InvalidOperationException("Los atributos no caben en el muro.");

            using (var tr = db.TransactionManager.StartTransaction())
            {
                Wall2dCommand.EnsureAppAndLayers(tr, db);
                var axis = tr.GetObject(axisId, OpenMode.ForWrite) as Line;
                if (axis == null)
                    throw new InvalidOperationException("No está el eje del muro.");

                string role;
                int groupId;
                double oldThick;
                long wallDbId;
                bool special;
                WallCadXData.Read(axis, out role, out groupId, out oldThick, out wallDbId, out special);
                var previousGroup = groupId;
                if (newGroup.HasValue && newGroup.Value >= 0)
                    groupId = newGroup.Value;

                var dir = new Vector3d(axis.EndPoint.X - axis.StartPoint.X, axis.EndPoint.Y - axis.StartPoint.Y, 0);
                if (dir.Length > 1e-9)
                {
                    dir = dir.GetNormal();
                    var newLen = CadUnits.FromMillimeters(lengthM * 1000.0);
                    axis.EndPoint = new Point3d(
                        axis.StartPoint.X + dir.X * newLen,
                        axis.StartPoint.Y + dir.Y * newLen,
                        axis.StartPoint.Z);
                }

                var thickDwg = CadUnits.FromMillimeters(widthM * 1000.0);
                var half = thickDwg * 0.5;
                var perp = new Vector3d(-dir.Y, dir.X, 0);
                if (perp.Length < 1e-9)
                    perp = Vector3d.YAxis;
                else
                    perp = perp.GetNormal();

                var bt = (BlockTable)tr.GetObject(db.BlockTableId, OpenMode.ForRead);
                var ms = (BlockTableRecord)tr.GetObject(bt[BlockTableRecord.ModelSpace], OpenMode.ForRead);
                var band = Math.Max(Math.Abs(oldThick) * 1.6, CadUnits.FromMillimeters(800));
                foreach (ObjectId id in ms)
                {
                    if (id == axisId)
                        continue;
                    var face = tr.GetObject(id, OpenMode.ForRead) as Line;
                    if (face == null)
                        continue;
                    if (!string.Equals(face.Layer, WallCadXData.LayerFace, StringComparison.OrdinalIgnoreCase))
                        continue;
                    string faceRole;
                    int faceGroup;
                    double faceThick;
                    long faceWall;
                    bool faceSpecial;
                    WallCadXData.Read(face, out faceRole, out faceGroup, out faceThick, out faceWall, out faceSpecial);
                    if (previousGroup != 0 && faceGroup != previousGroup)
                        continue;
                    if (!NearAxis(face, axis, band))
                        continue;

                    var mid = new Point3d(
                        (face.StartPoint.X + face.EndPoint.X) * 0.5,
                        (face.StartPoint.Y + face.EndPoint.Y) * 0.5,
                        axis.StartPoint.Z);
                    var axisMid = new Point3d(
                        (axis.StartPoint.X + axis.EndPoint.X) * 0.5,
                        (axis.StartPoint.Y + axis.EndPoint.Y) * 0.5,
                        axis.StartPoint.Z);
                    var sign = (mid.X - axisMid.X) * perp.X + (mid.Y - axisMid.Y) * perp.Y >= 0 ? 1d : -1d;
                    var off = perp * (sign * half);
                    face.UpgradeOpen();
                    face.StartPoint = axis.StartPoint + off;
                    face.EndPoint = axis.EndPoint + off;
                    WallCadXData.Write(face, faceRole, groupId, thickDwg, faceWall, faceSpecial);
                }

                WallCadXData.Write(axis, role, groupId, thickDwg, wallDbId, special, json);

                if (heightAll || configAll)
                {
                    foreach (ObjectId id in ms)
                    {
                        if (id == axisId)
                            continue;
                        var other = tr.GetObject(id, OpenMode.ForRead) as Line;
                        if (other == null)
                            continue;
                        if (!string.Equals(other.Layer, WallCadXData.LayerAxis, StringComparison.OrdinalIgnoreCase))
                            continue;
                        string oRole;
                        int oGroup;
                        double oThick;
                        long oWall;
                        bool oSpecial;
                        WallCadXData.Read(other, out oRole, out oGroup, out oThick, out oWall, out oSpecial);
                        var otherAttrs = WallEditAttrs.Load(WallCadXData.ReadAttrsJson(other));
                        if (heightAll)
                            otherAttrs.HeightM = attrs.HeightM;
                        if (configAll)
                            CopyConfig(attrs, otherAttrs);
                        other.UpgradeOpen();
                        WallCadXData.Write(other, oRole, oGroup, oThick, oWall, oSpecial, otherAttrs.ToJson());
                    }
                }

                tr.Commit();
            }
        }

        private static void CopyConfig(WallEditAttrs from, WallEditAttrs to)
        {
            to.BracketIn = from.BracketIn;
            to.BracketOut = from.BracketOut;
            to.StiffIn = from.StiffIn;
            to.StiffOut = from.StiffOut;
            to.Panel750 = from.Panel750;
            to.VerticalPanels = from.VerticalPanels;
            to.PropIn = from.PropIn;
            to.PropOut = from.PropOut;
            to.PropInInf = from.PropInInf;
            to.PropOutInf = from.PropOutInf;
            to.TapeIni = from.TapeIni;
            to.TapeEnd = from.TapeEnd;
            to.SolapeIni = from.SolapeIni;
            to.SolapeEnd = from.SolapeEnd;
        }

        private static bool NearAxis(Line face, Line axis, double band)
        {
            var mid = new Point3d(
                (face.StartPoint.X + face.EndPoint.X) * 0.5,
                (face.StartPoint.Y + face.EndPoint.Y) * 0.5,
                0);
            return WallCadXData.DistToSegment(mid, axis.StartPoint, axis.EndPoint) <= band;
        }

        private static bool TryResolveAxis(Document doc, Point3d at, out ObjectId axisId)
        {
            axisId = ObjectId.Null;
            var ed = doc.Editor;
            var hit = ObjectId.Null;
            try
            {
                var implied = ed.SelectImplied();
                if (implied.Status == PromptStatus.OK && implied.Value != null && implied.Value.Count > 0)
                    hit = implied.Value[0].ObjectId;
            }
            catch
            {
            }

            if (hit.IsNull)
            {
                try
                {
                    double aperture;
                    using (var view = ed.GetCurrentView())
                        aperture = Math.Max(view.Height * 0.02, CadUnits.FromMillimeters(80));
                    var res = ed.SelectCrossingWindow(
                        new Point3d(at.X - aperture, at.Y - aperture, at.Z),
                        new Point3d(at.X + aperture, at.Y + aperture, at.Z));
                    if (res.Status == PromptStatus.OK && res.Value != null && res.Value.Count > 0)
                        hit = res.Value[0].ObjectId;
                }
                catch
                {
                }
            }

            if (hit.IsNull)
                return false;

            using (var tr = doc.Database.TransactionManager.StartTransaction())
            {
                var ent = tr.GetObject(hit, OpenMode.ForRead) as Entity;
                axisId = Resolve(tr, doc.Database, ent, at);
                tr.Commit();
            }
            return !axisId.IsNull;
        }

        private static ObjectId Resolve(Transaction tr, Database db, Entity ent, Point3d at)
        {
            if (ent == null)
                return ObjectId.Null;
            var line = ent as Line;
            if (line != null && string.Equals(line.Layer, WallCadXData.LayerAxis, StringComparison.OrdinalIgnoreCase))
                return line.ObjectId;
            if (line != null && string.Equals(line.Layer, WallCadXData.LayerFace, StringComparison.OrdinalIgnoreCase))
                return AxisForFace(tr, db, line);

            var br = ent as BlockReference;
            if (br != null)
            {
                var wallId = BlockInsertCommand.WallIdFromInsert(br);
                var byId = AxisByWallId(tr, db, wallId);
                if (!byId.IsNull)
                    return byId;
                return NearestAxis(tr, db, br.Position);
            }

            if (string.Equals(ent.Layer, Layer3d, StringComparison.OrdinalIgnoreCase))
                return NearestAxis(tr, db, at);
            return ObjectId.Null;
        }

        private static ObjectId AxisForFace(Transaction tr, Database db, Line face)
        {
            string role;
            int groupId;
            double thickness;
            long wallDbId;
            bool special;
            WallCadXData.Read(face, out role, out groupId, out thickness, out wallDbId, out special);
            var band = Math.Max(Math.Abs(thickness) * 2.2, CadUnits.FromMillimeters(800));
            ObjectId best = ObjectId.Null;
            var bestDist = double.MaxValue;
            foreach (var id in AxisIds(tr, db))
            {
                var axis = tr.GetObject(id, OpenMode.ForRead) as Line;
                if (axis == null)
                    continue;
                string aRole;
                int aGroup;
                double aThick;
                long aWall;
                bool aSpecial;
                WallCadXData.Read(axis, out aRole, out aGroup, out aThick, out aWall, out aSpecial);
                if (groupId != 0 && aGroup != groupId)
                    continue;
                var mid = new Point3d(
                    (face.StartPoint.X + face.EndPoint.X) * 0.5,
                    (face.StartPoint.Y + face.EndPoint.Y) * 0.5,
                    0);
                var d = WallCadXData.DistToSegment(mid, axis.StartPoint, axis.EndPoint);
                if (d < bestDist && d <= band)
                {
                    bestDist = d;
                    best = id;
                }
            }
            return best;
        }

        private static ObjectId AxisByWallId(Transaction tr, Database db, long wallId)
        {
            if (wallId <= 0)
                return ObjectId.Null;
            foreach (var id in AxisIds(tr, db))
            {
                var axis = tr.GetObject(id, OpenMode.ForRead) as Line;
                if (axis == null)
                    continue;
                string role;
                int groupId;
                double thickness;
                long wallDbId;
                bool special;
                WallCadXData.Read(axis, out role, out groupId, out thickness, out wallDbId, out special);
                if (wallDbId == wallId)
                    return id;
            }
            return ObjectId.Null;
        }

        private static ObjectId NearestAxis(Transaction tr, Database db, Point3d at)
        {
            ObjectId best = ObjectId.Null;
            var bestDist = double.MaxValue;
            var limit = CadUnits.FromMillimeters(2000);
            foreach (var id in AxisIds(tr, db))
            {
                var axis = tr.GetObject(id, OpenMode.ForRead) as Line;
                if (axis == null)
                    continue;
                var d = WallCadXData.DistToSegment(at, axis.StartPoint, axis.EndPoint);
                if (d < bestDist && d <= limit)
                {
                    bestDist = d;
                    best = id;
                }
            }
            return best;
        }

        private static List<ObjectId> AxisIds(Transaction tr, Database db)
        {
            var ids = new List<ObjectId>();
            var bt = (BlockTable)tr.GetObject(db.BlockTableId, OpenMode.ForRead);
            var ms = (BlockTableRecord)tr.GetObject(bt[BlockTableRecord.ModelSpace], OpenMode.ForRead);
            foreach (ObjectId id in ms)
            {
                var line = tr.GetObject(id, OpenMode.ForRead) as Line;
                if (line != null && string.Equals(line.Layer, WallCadXData.LayerAxis, StringComparison.OrdinalIgnoreCase))
                    ids.Add(id);
            }
            return ids;
        }
    }
}
