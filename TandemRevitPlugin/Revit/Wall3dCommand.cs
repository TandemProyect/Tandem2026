using System;
using System.Collections.Generic;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using TandemRevit.Models;
using TandemRevit.UI.Views;

namespace TandemRevit
{
    public static class Wall3dCommand
    {
        private const string FaceStyle = "TANDEM_MURO_CARA";
        private const string SolidName = "TANDEM_MURO_3D";
        private const double DefaultHeightMm = 2700;

        public static Result Run(UIDocument uidoc)
        {
            if (uidoc?.Document == null) return Result.Cancelled;
            Document doc = uidoc.Document;

            List<LineaDTO> faces = CollectFaceLines(doc);
            if (faces.Count == 0)
            {
                TaskDialog.Show("Tandem 2026", "No hay caras de muro 2D (estilo TANDEM_MURO_CARA). Dibuja muros 2D primero.");
                return Result.Cancelled;
            }

            var seleccion = new SeleccionLineasDTO
            {
                Lineas = faces,
                TotalSeleccionados = faces.Count,
                TotalLineas = faces.Count,
                TotalPolilineas = 0,
                FechaSeleccion = DateTime.Now,
                Usuario = Environment.UserName,
                AlturaMuroMm = DefaultHeightMm
            };

            ApiResponse<DeteccionEsquinasLDTO> respuesta = null;
            Wall3dProgressWindow overlay = null;
            int solids = 0;
            try
            {
                overlay = Wall3dProgressWindow.ShowCentered();
                var api = new MVCApiService();
                respuesta = overlay.Wait(() =>
                    api.EnviarLineasSeleccionadasAsync(seleccion)
                        .ConfigureAwait(false).GetAwaiter().GetResult());

                if (respuesta == null || !respuesta.Exito || respuesta.Datos == null)
                {
                    TaskDialog.Show("Tandem 2026", respuesta?.Mensaje ?? "Sin respuesta del detector.");
                    return Result.Failed;
                }

                using (var t = new Transaction(doc, "Tandem muros 3D"))
                {
                    t.Start();
                    DeleteExistingSolids(doc);
                    solids = DrawModelDesingSolids(doc, respuesta.Datos);
                    t.Commit();
                }
            }
            catch (Exception ex)
            {
                TaskDialog.Show("Tandem 2026", PluginExceptionHelper.Format(ex));
                return Result.Failed;
            }
            finally
            {
                try { overlay?.Close(); } catch { }
            }

            TaskDialog.Show(
                "Tandem 2026",
                respuesta.Mensaje + "\nMuros rectos: " + respuesta.Datos.TotalMurosRectos +
                ", esquinas L: " + respuesta.Datos.TotalEsquinasDetectadas +
                ", sólidos: " + solids + ".");
            return Result.Succeeded;
        }

        private static List<LineaDTO> CollectFaceLines(Document doc)
        {
            var lineas = new List<LineaDTO>();
            var curves = new FilteredElementCollector(doc).OfClass(typeof(CurveElement));
            foreach (CurveElement ce in curves)
            {
                var gs = ce.LineStyle as GraphicsStyle;
                if (gs?.GraphicsStyleCategory == null) continue;
                if (!string.Equals(gs.GraphicsStyleCategory.Name, FaceStyle, StringComparison.OrdinalIgnoreCase))
                    continue;
                Curve c = ce.GeometryCurve;
                if (c == null) continue;
                XYZ a = c.GetEndPoint(0);
                XYZ b = c.GetEndPoint(1);
                lineas.Add(new LineaDTO
                {
                    Tipo = "Line",
                    InicioX = CadUnits.ToMillimeters(a.X),
                    InicioY = CadUnits.ToMillimeters(a.Y),
                    InicioZ = 0,
                    FinX = CadUnits.ToMillimeters(b.X),
                    FinY = CadUnits.ToMillimeters(b.Y),
                    FinZ = 0,
                    Layer = FaceStyle,
                    Color = "",
                    Longitud = CadUnits.ToMillimeters(a.DistanceTo(b))
                });
            }
            return lineas;
        }

        private static void DeleteExistingSolids(Document doc)
        {
            var doomed = new List<ElementId>();
            foreach (DirectShape ds in new FilteredElementCollector(doc).OfClass(typeof(DirectShape)))
            {
                if (string.Equals(ds.Name, SolidName, StringComparison.OrdinalIgnoreCase))
                    doomed.Add(ds.Id);
            }
            if (doomed.Count > 0)
                doc.Delete(doomed);
        }

        private static int DrawModelDesingSolids(Document doc, DeteccionEsquinasLDTO datos)
        {
            if (datos.PolilineasADibujar == null) return 0;
            int created = 0;
            foreach (PolilineaDTO poly in datos.PolilineasADibujar)
            {
                if (poly == null || poly.Vertices == null || poly.Vertices.Count < 3) continue;
                if (!string.Equals(poly.Capa, "ModelDesing", StringComparison.OrdinalIgnoreCase)) continue;
                if (poly.AlturaExtrusion <= 0) continue;
                if (TryCreateSolid(doc, poly))
                    created++;
            }
            return created;
        }

        private static bool TryCreateSolid(Document doc, PolilineaDTO poly)
        {
            try
            {
                var pts = new List<XYZ>();
                XYZ first = null;
                XYZ last = null;
                foreach (PuntoDTO v in poly.Vertices)
                {
                    if (v == null) continue;
                    var pt = new XYZ(CadUnits.FromMillimeters(v.X), CadUnits.FromMillimeters(v.Y), 0);
                    if (first == null)
                        first = pt;
                    else if (pt.DistanceTo(last) < 1e-9)
                        continue;
                    pts.Add(pt);
                    last = pt;
                }
                if (pts.Count >= 3 && first != null && last.DistanceTo(first) < 1e-9)
                    pts.RemoveAt(pts.Count - 1);
                if (pts.Count < 3) return false;

                var loop = new CurveLoop();
                for (int i = 0; i < pts.Count; i++)
                {
                    XYZ a = pts[i];
                    XYZ b = pts[(i + 1) % pts.Count];
                    if (a.DistanceTo(b) < 1e-9) continue;
                    loop.Append(Line.CreateBound(a, b));
                }

                double height = CadUnits.FromMillimeters(poly.AlturaExtrusion);
                Solid solid = GeometryCreationUtilities.CreateExtrusionGeometry(
                    new List<CurveLoop> { loop }, XYZ.BasisZ, height);
                DirectShape ds = DirectShape.CreateElement(doc, new ElementId(BuiltInCategory.OST_GenericModel));
                ds.SetShape(new List<GeometryObject> { solid });
                ds.Name = SolidName;
                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}
