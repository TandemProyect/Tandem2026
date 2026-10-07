using System;
using System.Collections.Generic;
using ZwcadPlugin.Models;
using ZwcadPlugin.UI.Views;
using ZwSoft.ZwCAD.ApplicationServices;
using ZwSoft.ZwCAD.Colors;
using ZwSoft.ZwCAD.DatabaseServices;
using ZwSoft.ZwCAD.EditorInput;
using ZwSoft.ZwCAD.Geometry;
using ZwSoft.ZwCAD.Runtime;
using AcadApp = ZwSoft.ZwCAD.ApplicationServices.Application;

[assembly: CommandClass(typeof(ZwcadPlugin.Wall3dCommand))]

namespace ZwcadPlugin
{
    /// <summary>
    /// Levanta muros/esquinas 3D con la misma API que Desing_2:
    /// DesignToolsAutocad/ProcesarLineasZwcad → LCornerDetector.
    /// Aquí solo se recolectan caras 2D y se extruyen los perímetros ModelDesing.
    /// </summary>
    public class Wall3dCommand
    {
        public const string CommandName = "TANDEM_MURO3D";
        private const string Layer3d = "TANDEM_MURO_3D";
        private const string FaceLayer = "TANDEM_MURO_CARA";
        private const double DefaultHeightMm = 2700;

        [CommandMethod(CommandName)]
        [CommandMethod("GENERAR3D")]
        public void GenerarMuros3d()
        {
            Run(replaceExisting: true);
        }

        [CommandMethod("REGENERAR3D")]
        public void RegenerarMuros3d()
        {
            Run(replaceExisting: true);
        }

        private static void Run(bool replaceExisting)
        {
            Document doc = AcadApp.DocumentManager.MdiActiveDocument;
            if (doc == null) return;
            Editor ed = doc.Editor;
            Database db = doc.Database;

            List<LineaDTO> faces = CollectFaceLines(db);
            if (faces.Count == 0)
            {
                ed.WriteMessage("\nNo hay caras de muro 2D (capa TANDEM_MURO_CARA). Dibuja muros 2D primero.\n");
                return;
            }

            WallArticleCad.CaptureAndErasePanels(doc);
            ed.WriteMessage($"\nGenerar muros 3D — {faces.Count} cara(s) → LCornerDetector (Desing_2)...\n");

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
            int solids = 0;
            int restored = 0;
            Wall3dProgressWindow overlay = null;
            try
            {
                overlay = Wall3dProgressWindow.ShowOverAcad();
                var api = new MVCApiService();
                respuesta = overlay.Wait(() =>
                    api.EnviarLineasSeleccionadasAsync(seleccion)
                        .ConfigureAwait(false).GetAwaiter().GetResult());

                if (respuesta == null || !respuesta.Exito || respuesta.Datos == null)
                {
                    ed.WriteMessage("\n" + (respuesta?.Mensaje ?? "Sin respuesta del detector.") + "\n");
                    return;
                }

                solids = DrawModelDesingSolids(db, respuesta.Datos, replaceExisting);
            }
            catch (System.Exception ex)
            {
                ed.WriteMessage("\n" + PluginExceptionHelper.Format(ex) + "\n");
                return;
            }
            finally
            {
                try { overlay?.Close(); } catch { }
                restored = WallArticleCad.RestoreCached(doc);
            }

            if (respuesta == null || respuesta.Datos == null)
                return;
            DeteccionEsquinasLDTO datos = respuesta.Datos;
            ed.WriteMessage(
                $"\n{respuesta.Mensaje}\n" +
                $"Muros rectos: {datos.TotalMurosRectos}, esquinas L: {datos.TotalEsquinasDetectadas}, sólidos: {solids}" +
                (restored > 0 ? $", paneles: {restored}" : "") + ".\n");
            if (solids == 0)
                ed.WriteMessage("No se pudo extruir ningún perímetro ModelDesing. Revisa las caras 2D (capa TANDEM_MURO_CARA).\n");
        }

        private static List<LineaDTO> CollectFaceLines(Database db)
        {
            var lineas = new List<LineaDTO>();
            using (var tr = db.TransactionManager.StartTransaction())
            {
                var bt = (BlockTable)tr.GetObject(db.BlockTableId, OpenMode.ForRead);
                var ms = (BlockTableRecord)tr.GetObject(bt[BlockTableRecord.ModelSpace], OpenMode.ForRead);
                foreach (ObjectId id in ms)
                {
                    var line = tr.GetObject(id, OpenMode.ForRead) as Line;
                    if (line == null) continue;
                    if (!string.Equals(line.Layer, FaceLayer, StringComparison.OrdinalIgnoreCase))
                        continue;

                    Point3d a = line.StartPoint;
                    Point3d b = line.EndPoint;
                    lineas.Add(new LineaDTO
                    {
                        Tipo = "Line",
                        InicioX = CadUnits.ToMillimeters(a.X),
                        InicioY = CadUnits.ToMillimeters(a.Y),
                        InicioZ = 0,
                        FinX = CadUnits.ToMillimeters(b.X),
                        FinY = CadUnits.ToMillimeters(b.Y),
                        FinZ = 0,
                        Layer = line.Layer,
                        Color = "",
                        Longitud = CadUnits.ToMillimeters(a.DistanceTo(b)),
                        Vertices = null
                    });
                }
                tr.Commit();
            }
            return lineas;
        }

        private static int DrawModelDesingSolids(Database db, DeteccionEsquinasLDTO datos, bool replaceExisting)
        {
            if (datos.PolilineasADibujar == null) return 0;
            int created = 0;

            using (var tr = db.TransactionManager.StartTransaction())
            {
                EnsureLayer(tr, db, Layer3d, 253);
                var bt = (BlockTable)tr.GetObject(db.BlockTableId, OpenMode.ForRead);
                var ms = (BlockTableRecord)tr.GetObject(bt[BlockTableRecord.ModelSpace], OpenMode.ForWrite);

                if (replaceExisting)
                    EraseLayerEntities(tr, ms, Layer3d);

                foreach (PolilineaDTO poly in datos.PolilineasADibujar)
                {
                    if (poly == null || poly.Vertices == null || poly.Vertices.Count < 3) continue;
                    if (!string.Equals(poly.Capa, "ModelDesing", StringComparison.OrdinalIgnoreCase)) continue;
                    if (poly.AlturaExtrusion <= 0) continue;

                    if (TryAppendExtrudedSolid(tr, ms, poly))
                        created++;
                }

                tr.Commit();
            }

            return created;
        }

        private static bool TryAppendExtrudedSolid(Transaction tr, BlockTableRecord ms, PolilineaDTO poly)
        {
            try
            {
                var pl = new Polyline();
                pl.SetDatabaseDefaults();
                Point2d? first = null;
                Point2d lastAdded = default;
                int added = 0;
                for (int i = 0; i < poly.Vertices.Count; i++)
                {
                    var v = poly.Vertices[i];
                    if (v == null) continue;
                    var pt = new Point2d(CadUnits.FromMillimeters(v.X), CadUnits.FromMillimeters(v.Y));
                    if (first == null)
                        first = pt;
                    else if (pt.GetDistanceTo(lastAdded) < 1e-9)
                        continue;
                    pl.AddVertexAt(pl.NumberOfVertices, pt, 0, 0, 0);
                    lastAdded = pt;
                    added++;
                }
                if (added >= 4 && first != null && lastAdded.GetDistanceTo(first.Value) < 1e-9)
                    pl.RemoveVertexAt(pl.NumberOfVertices - 1);
                if (pl.NumberOfVertices < 3)
                {
                    pl.Dispose();
                    return false;
                }
                pl.Closed = true;

                var curves = new DBObjectCollection { pl };
                DBObjectCollection regions = Region.CreateFromCurves(curves);
                pl.Dispose();
                if (regions == null || regions.Count == 0) return false;

                var region = regions[0] as Region;
                if (region == null) return false;

                double height = CadUnits.FromMillimeters(poly.AlturaExtrusion);
                var solid = new Solid3d();
                solid.Extrude(region, height, 0);
                solid.Layer = Layer3d;
                ms.AppendEntity(solid);
                tr.AddNewlyCreatedDBObject(solid, true);
                region.Dispose();
                for (int i = 1; i < regions.Count; i++)
                    regions[i].Dispose();
                return true;
            }
            catch
            {
                return false;
            }
        }

        private static void EraseLayerEntities(Transaction tr, BlockTableRecord ms, string layer)
        {
            var doomed = new List<ObjectId>();
            foreach (ObjectId id in ms)
            {
                var ent = tr.GetObject(id, OpenMode.ForRead) as Entity;
                if (ent != null && string.Equals(ent.Layer, layer, StringComparison.OrdinalIgnoreCase))
                    doomed.Add(id);
            }
            foreach (ObjectId id in doomed)
            {
                var ent = tr.GetObject(id, OpenMode.ForWrite) as Entity;
                ent?.Erase();
            }
        }

        private static void EnsureLayer(Transaction tr, Database db, string name, short color)
        {
            var layers = (LayerTable)tr.GetObject(db.LayerTableId, OpenMode.ForRead);
            if (layers.Has(name)) return;
            layers.UpgradeOpen();
            var rec = new LayerTableRecord
            {
                Name = name,
                Color = Color.FromColorIndex(ColorMethod.ByAci, color)
            };
            layers.Add(rec);
            tr.AddNewlyCreatedDBObject(rec, true);
        }
    }
}

