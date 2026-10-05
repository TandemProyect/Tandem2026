using System;
using System.Collections.Generic;
using AutocadPlugin.Models;
using AutocadPlugin.UI.Views;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.Colors;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Geometry;
using Autodesk.AutoCAD.Runtime;
using Newtonsoft.Json;
using AcadApp = Autodesk.AutoCAD.ApplicationServices.Application;

[assembly: CommandClass(typeof(AutocadPlugin.FormworkCommand))]

namespace AutocadPlugin
{
    /// <summary>
    /// Recoge muros del DWG y pide poses a Atk60WallsRepository.SolveFromIdsJson
    /// (la misma que Desing). Inserta DWG 3DRef por defecto (más ligero que 3D).
    /// </summary>
    public class FormworkCommand
    {
        public const string CommandName = "TANDEM_ENCOFRAR";
        private const string LayerAxis = "TANDEM_MURO_EJE";
        private const string LayerFormwork = "TANDEM_ENCOFRADO";
        private const string AppName = "TANDEM";
        private const double DefaultHeightM = 2.70;

        [CommandMethod(CommandName)]
        [CommandMethod("ENCOFRAR")]
        public void Run()
        {
            Document doc = AcadApp.DocumentManager.MdiActiveDocument;
            if (doc == null) return;
            Editor ed = doc.Editor;
            Database db = doc.Database;

            var walls = CollectAxisWalls(db);
            if (walls.Count == 0)
            {
                ed.WriteMessage("\nNo hay muros rectos (capa TANDEM_MURO_EJE). Dibuja muros 2D o abre un diseño.\n");
                return;
            }

            var idsJson = BuildIdsJson(walls);
            ed.WriteMessage($"\nEncofrar ATK-60 — {walls.Count} muro(s) recto(s) → misma lógica que Desing...\n");

            Atk60FormworkResponse resp = null;
            Wall3dProgressWindow overlay = null;
            try
            {
                overlay = Wall3dProgressWindow.ShowOverAcad();
                var api = new MVCApiService();
                resp = overlay.Wait(() =>
                    api.EncofrarAtk60Async(idsJson)
                        .ConfigureAwait(false).GetAwaiter().GetResult());
            }
            catch (System.Exception ex)
            {
                ed.WriteMessage("\n" + PluginExceptionHelper.Format(ex) + "\n");
                return;
            }
            finally
            {
                try { overlay?.Close(); } catch { }
            }

            if (resp == null || !resp.Exito)
            {
                ed.WriteMessage("\n" + (resp != null && !string.IsNullOrWhiteSpace(resp.Mensaje)
                    ? resp.Mensaje
                    : "No se pudo encofrar.") + "\n");
                return;
            }

            var elements = resp.ElementsForThreeJs != null
                ? resp.ElementsForThreeJs.Elements
                : null;
            if (elements == null || elements.Count == 0)
            {
                ed.WriteMessage("\nEl servidor no devolvió paneles. Revisa longitudes de muro.\n");
                return;
            }

            int inserted;
            int skipped;
            try
            {
                using (doc.LockDocument())
                    InsertPanels(doc, elements, out inserted, out skipped);
            }
            catch (System.Exception ex)
            {
                ed.WriteMessage("\n" + PluginExceptionHelper.Format(ex) + "\n");
                return;
            }

            ed.WriteMessage(
                $"\nEncofrado ATK-60: {inserted} panel(es) DWG"
                + (skipped > 0 ? $", {skipped} sin DWG local" : "")
                + $" (muros {resp.WallsCount}, piezas {elements.Count}).\n");
        }

        private sealed class AxisWall
        {
            public string Id;
            public double StartXmm;
            public double StartYmm;
            public double EndXmm;
            public double EndYmm;
            public double ThicknessM;
            public double LengthMm;
        }

        private static List<AxisWall> CollectAxisWalls(Database db)
        {
            var walls = new List<AxisWall>();
            using (var tr = db.TransactionManager.StartTransaction())
            {
                var bt = (BlockTable)tr.GetObject(db.BlockTableId, OpenMode.ForRead);
                var ms = (BlockTableRecord)tr.GetObject(bt[BlockTableRecord.ModelSpace], OpenMode.ForRead);
                var n = 0;
                foreach (ObjectId id in ms)
                {
                    var line = tr.GetObject(id, OpenMode.ForRead) as Line;
                    if (line == null) continue;
                    if (!string.Equals(line.Layer, LayerAxis, StringComparison.OrdinalIgnoreCase))
                        continue;

                    string xdRole;
                    int groupId;
                    double thicknessDwg;
                    long wallDbId;
                    bool isSpecial;
                    WallCadXData.Read(line, out xdRole, out groupId, out thicknessDwg, out wallDbId, out isSpecial);
                    if (isSpecial)
                        continue;

                    n++;
                    var a = line.StartPoint;
                    var b = line.EndPoint;
                    var sx = CadUnits.ToMillimeters(a.X);
                    var sy = CadUnits.ToMillimeters(a.Y);
                    var ex = CadUnits.ToMillimeters(b.X);
                    var ey = CadUnits.ToMillimeters(b.Y);
                    var len = Math.Sqrt((ex - sx) * (ex - sx) + (ey - sy) * (ey - sy));
                    if (len < 1)
                        continue;

                    walls.Add(new AxisWall
                    {
                        Id = "axis-" + n,
                        StartXmm = sx,
                        StartYmm = sy,
                        EndXmm = ex,
                        EndYmm = ey,
                        ThicknessM = ThicknessMeters(line),
                        LengthMm = len
                    });
                }
                tr.Commit();
            }
            return walls;
        }

        private static double ThicknessMeters(Line line)
        {
            try
            {
                if (line.XData != null)
                {
                    var tv = line.XData.AsArray();
                    var inApp = false;
                    foreach (var t in tv)
                    {
                        if (t.TypeCode == (int)DxfCode.ExtendedDataRegAppName)
                        {
                            inApp = string.Equals(t.Value as string, AppName, StringComparison.OrdinalIgnoreCase);
                            continue;
                        }
                        if (inApp && t.TypeCode == (int)DxfCode.ExtendedDataReal)
                            return CadUnits.ToMillimeters(Convert.ToDouble(t.Value)) / 1000.0;
                    }
                }
            }
            catch
            {
            }
            return 0.30;
        }

        private static string BuildIdsJson(List<AxisWall> walls)
        {
            var list = new List<object>();
            foreach (var w in walls)
            {
                list.Add(new
                {
                    Id = w.Id,
                    LineId = w.Id,
                    WallId = w.Id,
                    Attributes = new Dictionary<string, object>
                    {
                        { "_Datalong", w.LengthMm / 1000.0 },
                        { "_DataWith", w.ThicknessM },
                        { "_DataHeight", DefaultHeightM },
                        { "InicioX", w.StartXmm },
                        { "InicioY", w.StartYmm },
                        { "InicioZ", 0d },
                        { "FinX", w.EndXmm },
                        { "FinY", w.EndYmm },
                        { "FinZ", 0d },
                        { "Tipo", "Line" }
                    }
                });
            }
            return JsonConvert.SerializeObject(list);
        }

        private static void InsertPanels(
            Document doc,
            List<Atk60FormworkElement> elements,
            out int inserted,
            out int skipped)
        {
            inserted = 0;
            skipped = 0;
            var db = doc.Database;
            var meterToDwg = CadUnits.FromMillimeters(1000.0);
            const string view = "3dref";
            var blockCache = new Dictionary<string, ObjectId>(StringComparer.OrdinalIgnoreCase);

            foreach (var item in elements)
            {
                if (item == null)
                    continue;
                var code = CodeNameFrom(item.ElementCode, item.ImportPath);
                if (string.IsNullOrWhiteSpace(code) || blockCache.ContainsKey(code))
                    continue;
                var dwg = Atk60DwgResolver.ResolveDwg(code, view);
                if (string.IsNullOrWhiteSpace(dwg))
                    continue;
                blockCache[code] = BlockInsertCommand.EnsureBlockDefinition(
                    doc, dwg, BlockInsertCommand.BlockNameFor(code, view));
            }

            using (var tr = db.TransactionManager.StartTransaction())
            {
                EnsureFormworkLayer(tr, db);
                var bt = (BlockTable)tr.GetObject(db.BlockTableId, OpenMode.ForRead);
                var ms = (BlockTableRecord)tr.GetObject(bt[BlockTableRecord.ModelSpace], OpenMode.ForWrite);
                EraseLayerEntities(tr, ms, LayerFormwork);

                foreach (var item in elements)
                {
                    if (item == null)
                        continue;
                    var code = CodeNameFrom(item.ElementCode, item.ImportPath);
                    ObjectId blockId;
                    if (string.IsNullOrWhiteSpace(code)
                        || !blockCache.TryGetValue(code, out blockId)
                        || blockId.IsNull)
                    {
                        skipped++;
                        continue;
                    }

                    var at = new Point3d(
                        CadUnits.FromMillimeters(item.X),
                        CadUnits.FromMillimeters(item.Z),
                        CadUnits.FromMillimeters(item.Y));
                    var tumbado = string.Equals(item.Orientation, "Tumbado", StringComparison.OrdinalIgnoreCase);
                    // Three.js gira en Y (mano izquierda al pasar a Z de AutoCAD) → -RotY.
                    // El DWG mira al revés que el STL: 180° en vertical sobre el centro
                    // de la base, para no desplazar la huella del muro.
                    var yaw = -item.RotY;
                    var orient = Matrix3d.Rotation(yaw, Vector3d.ZAxis, Point3d.Origin)
                        * BlockInsertCommand.PanelOrient(meterToDwg, tumbado);
                    var widthMm = item.PieceWidthMm > 1 ? item.PieceWidthMm : 900d;
                    var along = Vector3d.XAxis.TransformBy(orient);
                    if (along.Length > 1e-9)
                        along = along.GetNormal();
                    var pivot = at + along * (CadUnits.FromMillimeters(widthMm) * 0.5);
                    var placed = Matrix3d.Rotation(Math.PI, Vector3d.ZAxis, pivot)
                        * Matrix3d.Displacement(at.GetAsVector())
                        * orient;

                    var br = new BlockReference(Point3d.Origin, blockId);
                    br.Layer = LayerFormwork;
                    br.BlockTransform = placed;
                    ms.AppendEntity(br);
                    tr.AddNewlyCreatedDBObject(br, true);
                    BlockInsertCommand.ApplyAtkXData(br, tr, db, code, view, "PANEL", tumbado ? 90 : 0);
                    inserted++;
                }

                tr.Commit();
            }
        }

        private static string CodeNameFrom(string elementCode, string importPath)
        {
            var code = (elementCode ?? "").Trim();
            const string prefix = "PANEL_";
            if (code.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
                code = code.Substring(prefix.Length);
            if (LooksLikeCode(code))
                return code;

            if (!string.IsNullOrWhiteSpace(importPath))
            {
                var file = System.IO.Path.GetFileNameWithoutExtension(importPath);
                if (!string.IsNullOrWhiteSpace(file))
                {
                    if (file.EndsWith("_F", StringComparison.OrdinalIgnoreCase))
                        file = file.Substring(0, file.Length - 2);
                    if (LooksLikeCode(file))
                        return file;
                }
            }
            return null;
        }

        private static bool LooksLikeCode(string code)
        {
            if (string.IsNullOrWhiteSpace(code))
                return false;
            var n = 0;
            foreach (var c in code)
            {
                if (char.IsLetterOrDigit(c))
                    n++;
            }
            return n >= 4;
        }

        private static void EnsureFormworkLayer(Transaction tr, Database db)
        {
            var layers = (LayerTable)tr.GetObject(db.LayerTableId, OpenMode.ForRead);
            if (layers.Has(LayerFormwork))
                return;
            layers.UpgradeOpen();
            var rec = new LayerTableRecord
            {
                Name = LayerFormwork,
                Color = Color.FromColorIndex(ColorMethod.ByAci, 50)
            };
            layers.Add(rec);
            tr.AddNewlyCreatedDBObject(rec, true);
        }

        private static void EraseLayerEntities(Transaction tr, BlockTableRecord ms, string layer)
        {
            var ids = new List<ObjectId>();
            foreach (ObjectId id in ms)
            {
                var ent = tr.GetObject(id, OpenMode.ForRead) as Entity;
                if (ent != null && string.Equals(ent.Layer, layer, StringComparison.OrdinalIgnoreCase))
                    ids.Add(id);
            }
            foreach (var id in ids)
            {
                var ent = tr.GetObject(id, OpenMode.ForWrite) as Entity;
                if (ent != null)
                    ent.Erase();
            }
        }
    }
}
