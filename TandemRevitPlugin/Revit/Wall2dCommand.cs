using System;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;

namespace TandemRevit
{
    public static class Wall2dCommand
    {
        private const string LayerAxis = "TANDEM_MURO_EJE";
        private const string LayerFace = "TANDEM_MURO_CARA";

        public static Result Run(UIDocument uidoc)
        {
            if (uidoc?.Document == null) return Result.Cancelled;
            Document doc = uidoc.Document;
            if (!(uidoc.ActiveView is ViewPlan plan) || plan.GenLevel == null)
            {
                TaskDialog.Show("Tandem 2026", "Abre una vista de planta para dibujar el muro 2D.");
                return Result.Cancelled;
            }

            double z = plan.GenLevel.Elevation;
            double thickness = CadUnits.FromMillimeters(300);
            double half = thickness * 0.5;

            XYZ chainStart;
            try
            {
                chainStart = uidoc.Selection.PickPoint(
                    ObjectSnapTypes.Endpoints | ObjectSnapTypes.Intersections | ObjectSnapTypes.Perpendicular,
                    "Punto inicial del muro");
            }
            catch (Autodesk.Revit.Exceptions.OperationCanceledException)
            {
                return Result.Cancelled;
            }

            chainStart = new XYZ(chainStart.X, chainStart.Y, z);
            XYZ current = chainStart;
            XYZ prevA = chainStart;
            XYZ prevB = chainStart;
            ModelCurve prevPlus = null;
            ModelCurve prevMinus = null;
            int segments = 0;

            while (true)
            {
                XYZ next;
                try
                {
                    next = uidoc.Selection.PickPoint(
                        ObjectSnapTypes.Endpoints | ObjectSnapTypes.Intersections | ObjectSnapTypes.Perpendicular,
                        "Punto siguiente del muro (Esc termina)");
                }
                catch (Autodesk.Revit.Exceptions.OperationCanceledException)
                {
                    break;
                }

                next = new XYZ(next.X, next.Y, z);
                if (next.DistanceTo(current) < 1e-8) continue;

                using (var t = new Transaction(doc, "Tandem muro 2D"))
                {
                    t.Start();
                    SketchPlane plane = EnsureSketchPlane(doc, z);
                    GraphicsStyle axisStyle = EnsureLineStyle(doc, LayerAxis, new Color(128, 128, 128));
                    GraphicsStyle faceStyle = EnsureLineStyle(doc, LayerFace, new Color(0, 0, 0));

                    XYZ n = Perp2d(current, next, half);
                    XYZ plusA = current + n;
                    XYZ plusB = next + n;
                    XYZ minusA = current - n;
                    XYZ minusB = next - n;

                    if (segments > 0 && prevPlus != null && prevMinus != null)
                    {
                        XYZ prevN = Perp2d(prevA, prevB, half);
                        if (TryIntersect(prevA + prevN, prevB + prevN, plusA, plusB, out XYZ miterPlus))
                        {
                            plusA = miterPlus;
                            SetEnd(prevPlus, miterPlus);
                        }
                        if (TryIntersect(prevA - prevN, prevB - prevN, minusA, minusB, out XYZ miterMinus))
                        {
                            minusA = miterMinus;
                            SetEnd(prevMinus, miterMinus);
                        }
                    }

                    CreateModelLine(doc, plane, current, next, axisStyle);
                    prevPlus = CreateModelLine(doc, plane, plusA, plusB, faceStyle);
                    prevMinus = CreateModelLine(doc, plane, minusA, minusB, faceStyle);
                    t.Commit();
                }

                prevA = current;
                prevB = next;
                current = next;
                segments++;
            }

            if (segments == 0)
                return Result.Cancelled;
            TaskDialog.Show("Tandem 2026", "Muro 2D: " + segments + " tramo(s) en estilos " + LayerAxis + " / " + LayerFace + ".");
            return Result.Succeeded;
        }

        private static SketchPlane EnsureSketchPlane(Document doc, double z)
        {
            Plane plane = Plane.CreateByNormalAndOrigin(XYZ.BasisZ, new XYZ(0, 0, z));
            return SketchPlane.Create(doc, plane);
        }

        private static GraphicsStyle EnsureLineStyle(Document doc, string name, Color color)
        {
            Category lines = doc.Settings.Categories.get_Item(BuiltInCategory.OST_Lines);
            Category sub = null;
            foreach (Category c in lines.SubCategories)
            {
                if (string.Equals(c.Name, name, StringComparison.OrdinalIgnoreCase))
                {
                    sub = c;
                    break;
                }
            }
            if (sub == null)
            {
                sub = doc.Settings.Categories.NewSubcategory(lines, name);
                sub.LineColor = color;
            }
            return sub.GetGraphicsStyle(GraphicsStyleType.Projection);
        }

        private static ModelCurve CreateModelLine(Document doc, SketchPlane plane, XYZ a, XYZ b, GraphicsStyle style)
        {
            ModelCurve curve = doc.Create.NewModelCurve(Line.CreateBound(a, b), plane);
            if (style != null)
                curve.LineStyle = style;
            return curve;
        }

        private static void SetEnd(ModelCurve curve, XYZ end)
        {
            Curve geom = curve.GeometryCurve;
            XYZ start = geom.GetEndPoint(0);
            curve.SetGeometryCurve(Line.CreateBound(start, new XYZ(end.X, end.Y, start.Z)), true);
        }

        private static XYZ Perp2d(XYZ a, XYZ b, double half)
        {
            var d = new XYZ(b.X - a.X, b.Y - a.Y, 0);
            if (d.GetLength() < 1e-12)
                return new XYZ(0, half, 0);
            d = d.Normalize();
            return new XYZ(-d.Y, d.X, 0) * half;
        }

        private static bool TryIntersect(XYZ a1, XYZ a2, XYZ b1, XYZ b2, out XYZ hit)
        {
            hit = a2;
            double x1 = a1.X, y1 = a1.Y, x2 = a2.X, y2 = a2.Y;
            double x3 = b1.X, y3 = b1.Y, x4 = b2.X, y4 = b2.Y;
            double den = (x1 - x2) * (y3 - y4) - (y1 - y2) * (x3 - x4);
            if (Math.Abs(den) < 1e-12) return false;
            double t = ((x1 - x3) * (y3 - y4) - (y1 - y3) * (x3 - x4)) / den;
            hit = new XYZ(x1 + t * (x2 - x1), y1 + t * (y2 - y1), a1.Z);
            return true;
        }
    }
}
