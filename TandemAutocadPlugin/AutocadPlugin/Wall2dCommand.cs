using System;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Geometry;
using Autodesk.AutoCAD.Runtime;
using AcadApp = Autodesk.AutoCAD.ApplicationServices.Application;

[assembly: CommandClass(typeof(AutocadPlugin.Wall2dCommand))]

namespace AutocadPlugin
{
    /// <summary>
    /// Muro 2D al estilo Desing_2: cadena de tramos (eje + dos caras).
    /// El 3D se levanta con TANDEM_MURO3D / GENERAR3D (LCornerDetector compartido).
    /// </summary>
    public class Wall2dCommand
    {
        public const string CommandName = "TANDEM_MURO2D";
        private const string AppName = "TANDEM";
        private const string LayerAxis = "TANDEM_MURO_EJE";
        private const string LayerFace = "TANDEM_MURO_CARA";

        [CommandMethod("TANDEM_VER2D")]
        public void Show2d()
        {
            Document doc = AcadApp.DocumentManager.MdiActiveDocument;
            if (doc == null) return;
            WallArticleCad.Enter2d(doc);
            doc.Editor.WriteMessage("\nVista 2D: se quitaron los muros 3D y los paneles.\n");
        }

        [CommandMethod("TANDEM_LINEA")]
        public void DrawLineAfter2d()
        {
            Document doc = AcadApp.DocumentManager.MdiActiveDocument;
            if (doc == null) return;
            WallArticleCad.Enter2d(doc);
            doc.SendStringToExecute("._PLINE ", true, false, false);
        }

        [CommandMethod(CommandName)]
        public void DrawWall2d()
        {
            Document doc = AcadApp.DocumentManager.MdiActiveDocument;
            if (doc == null) return;
            Editor ed = doc.Editor;
            Database db = doc.Database;
            WallArticleCad.Enter2d(doc);

            double thickness = ResolveWallThickness();
            double half = thickness * 0.5;
            int groupId = unchecked(Environment.TickCount & 0x7fffffff);

            PromptPointResult firstRes = ed.GetPoint("\nPunto inicial del muro: ");
            if (firstRes.Status != PromptStatus.OK) return;

            Point3d chainStart = firstRes.Value;
            Point3d current = chainStart;
            Point3d prevA = chainStart;
            Point3d prevB = chainStart;
            ObjectId prevPlusId = ObjectId.Null;
            ObjectId prevMinusId = ObjectId.Null;
            int segments = 0;

            ed.WriteMessage($"\nMuro 2D — espesor {thickness} (unidades del dibujo). Enter termina, Cerrar cierra el recinto.\n");

            while (true)
            {
                var opts = new PromptPointOptions("\nPunto siguiente del muro: ");
                opts.UseBasePoint = true;
                opts.BasePoint = current;
                opts.UseDashedLine = true;
                opts.AllowNone = true;
                if (segments >= 2)
                    opts.Keywords.Add("Cerrar");

                PromptPointResult nextRes = ed.GetPoint(opts);
                if (nextRes.Status == PromptStatus.None || nextRes.Status == PromptStatus.Cancel)
                    break;

                Point3d next;
                if (nextRes.Status == PromptStatus.Keyword)
                {
                    if (current.DistanceTo(chainStart) < 1e-8) break;
                    next = chainStart;
                }
                else if (nextRes.Status == PromptStatus.OK)
                {
                    next = nextRes.Value;
                }
                else
                {
                    break;
                }

                if (next.DistanceTo(current) < 1e-8) continue;

                CommitSegment(
                    db,
                    current,
                    next,
                    half,
                    thickness,
                    groupId,
                    segments > 0,
                    prevA,
                    prevB,
                    ref prevPlusId,
                    ref prevMinusId);

                prevA = current;
                prevB = next;
                current = next;
                segments++;

                if (nextRes.Status == PromptStatus.Keyword)
                    break;
            }

            ed.WriteMessage(segments == 0
                ? "\nMuro 2D cancelado.\n"
                : $"\nMuro 2D: {segments} tramo(s) en capas {LayerAxis} / {LayerFace}.\n");
        }

        private static void CommitSegment(
            Database db,
            Point3d a,
            Point3d b,
            double half,
            double thickness,
            int groupId,
            bool miterWithPrevious,
            Point3d prevA,
            Point3d prevB,
            ref ObjectId prevPlusId,
            ref ObjectId prevMinusId)
        {
            Vector3d n = Perp2d(a, b, half);
            Point3d plusA = a + n;
            Point3d plusB = b + n;
            Point3d minusA = a - n;
            Point3d minusB = b - n;

            if (miterWithPrevious && !prevPlusId.IsNull && !prevMinusId.IsNull)
            {
                Vector3d prevN = Perp2d(prevA, prevB, half);
                Point3d prevPlusA = prevA + prevN;
                Point3d prevPlusB = prevB + prevN;
                Point3d prevMinusA = prevA - prevN;
                Point3d prevMinusB = prevB - prevN;

                if (TryIntersect(prevPlusA, prevPlusB, plusA, plusB, out Point3d miterPlus))
                {
                    plusA = miterPlus;
                    UpdateLineEnd(db, prevPlusId, miterPlus);
                }
                if (TryIntersect(prevMinusA, prevMinusB, minusA, minusB, out Point3d miterMinus))
                {
                    minusA = miterMinus;
                    UpdateLineEnd(db, prevMinusId, miterMinus);
                }
            }

            using (var tr = db.TransactionManager.StartTransaction())
            {
                EnsureAppAndLayers(tr, db);
                BlockTable bt = (BlockTable)tr.GetObject(db.BlockTableId, OpenMode.ForRead);
                BlockTableRecord ms = (BlockTableRecord)tr.GetObject(bt[BlockTableRecord.ModelSpace], OpenMode.ForWrite);

                AppendLine(tr, ms, a, b, LayerAxis, "axis", groupId, thickness);
                prevPlusId = AppendLine(tr, ms, plusA, plusB, LayerFace, "face", groupId, thickness);
                prevMinusId = AppendLine(tr, ms, minusA, minusB, LayerFace, "face", groupId, thickness);
                tr.Commit();
            }
        }

        private static ObjectId AppendLine(
            Transaction tr,
            BlockTableRecord ms,
            Point3d p1,
            Point3d p2,
            string layer,
            string role,
            int groupId,
            double thickness)
        {
            var line = new Line(p1, p2);
            line.Layer = layer;
            WallCadXData.Write(line, role, groupId, thickness, 0, false);
            ObjectId id = ms.AppendEntity(line);
            tr.AddNewlyCreatedDBObject(line, true);
            return id;
        }

        private static void UpdateLineEnd(Database db, ObjectId id, Point3d end)
        {
            if (id.IsNull) return;
            using (var tr = db.TransactionManager.StartTransaction())
            {
                var line = tr.GetObject(id, OpenMode.ForWrite) as Line;
                if (line != null)
                    line.EndPoint = end;
                tr.Commit();
            }
        }

        internal static void EnsureAppAndLayers(Transaction tr, Database db)
        {
            var apps = (RegAppTable)tr.GetObject(db.RegAppTableId, OpenMode.ForRead);
            if (!apps.Has(AppName))
            {
                apps.UpgradeOpen();
                var rec = new RegAppTableRecord { Name = AppName };
                apps.Add(rec);
                tr.AddNewlyCreatedDBObject(rec, true);
            }

            var layers = (LayerTable)tr.GetObject(db.LayerTableId, OpenMode.ForRead);
            EnsureLayer(tr, layers, LayerAxis, 8);
            EnsureLayer(tr, layers, LayerFace, 7);
        }

        private static void EnsureLayer(Transaction tr, LayerTable layers, string name, short color)
        {
            if (layers.Has(name)) return;
            layers.UpgradeOpen();
            var rec = new LayerTableRecord
            {
                Name = name,
                Color = Autodesk.AutoCAD.Colors.Color.FromColorIndex(Autodesk.AutoCAD.Colors.ColorMethod.ByAci, color)
            };
            layers.Add(rec);
            tr.AddNewlyCreatedDBObject(rec, true);
        }

        private static Vector3d Perp2d(Point3d a, Point3d b, double half)
        {
            var d = new Vector3d(b.X - a.X, b.Y - a.Y, 0);
            if (d.Length < 1e-12)
                return new Vector3d(0, half, 0);
            d = d.GetNormal();
            return new Vector3d(-d.Y, d.X, 0) * half;
        }

        private static bool TryIntersect(Point3d a1, Point3d a2, Point3d b1, Point3d b2, out Point3d hit)
        {
            hit = a2;
            double x1 = a1.X, y1 = a1.Y, x2 = a2.X, y2 = a2.Y;
            double x3 = b1.X, y3 = b1.Y, x4 = b2.X, y4 = b2.Y;
            double den = (x1 - x2) * (y3 - y4) - (y1 - y2) * (x3 - x4);
            if (Math.Abs(den) < 1e-12) return false;
            double t = ((x1 - x3) * (y3 - y4) - (y1 - y3) * (x3 - x4)) / den;
            hit = new Point3d(x1 + t * (x2 - x1), y1 + t * (y2 - y1), a1.Z);
            return true;
        }

        /// <summary>
        /// Espesor Desing_2 por defecto: 0,30 m. Si el DWG está en mm, 300.
        /// </summary>
        internal static double ResolveWallThickness()
        {
            try
            {
                int units = Convert.ToInt32(AcadApp.GetSystemVariable("INSUNITS"));
                if (units == 4) return 300.0;
                if (units == 5) return 30.0;
            }
            catch
            {
            }
            return 0.30;
        }
    }
}
