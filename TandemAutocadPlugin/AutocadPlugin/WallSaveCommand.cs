using System;
using System.Collections.Generic;
using AutocadPlugin.Models;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Geometry;
using Autodesk.AutoCAD.Runtime;
using AcadApp = Autodesk.AutoCAD.ApplicationServices.Application;

[assembly: CommandClass(typeof(AutocadPlugin.WallSaveCommand))]

namespace AutocadPlugin
{
    /// <summary>
    /// Guarda los muros del DWG en TSql_DesignWall con el mismo
    /// DesignWallRepository.ReplaceWalls que Desing_2/SaveDesignWalls.
    /// </summary>
    public class WallSaveCommand
    {
        public const string CommandName = "TANDEM_SALVAR";
        private const string LayerAxis = "TANDEM_MURO_EJE";
        private const string LayerFace = "TANDEM_MURO_CARA";
        private const string AppName = "TANDEM";
        private const double DefaultHeightM = 2.70;

        [CommandMethod(CommandName)]
        [CommandMethod("SALVAR")]
        public void Run()
        {
            Document doc = AcadApp.DocumentManager.MdiActiveDocument;
            if (doc == null) return;
            Editor ed = doc.Editor;

            var designId = WallImportCommand.CurrentDesignId;
            if (designId <= 0)
            {
                ed.WriteMessage("\nAbre un diseño en el menú general para poder salvar los muros.\n");
                return;
            }

            var lines = CollectWalls(doc.Database);
            if (lines.Count == 0)
            {
                ed.WriteMessage("\nNo hay muros (capas TANDEM_MURO_EJE / TANDEM_MURO_CARA).\n");
                return;
            }

            ed.WriteMessage($"\nSalvar diseño {designId}: {lines.Count} línea(s) → TSql_DesignWall...\n");
            try
            {
                var api = new MVCApiService();
                var resp = api.SaveDesignWallsAsync(designId, PluginDeviceId.Current(), lines)
                    .ConfigureAwait(false).GetAwaiter().GetResult();
                if (resp == null || !resp.Exito)
                {
                    ed.WriteMessage("\n" + (resp != null && !string.IsNullOrWhiteSpace(resp.Mensaje)
                        ? resp.Mensaje
                        : "No se pudieron guardar los muros.") + "\n");
                    return;
                }

                ed.WriteMessage($"\nGuardados {resp.Count} muro(s) en el diseño {designId}.\n");
            }
            catch (System.Exception ex)
            {
                ed.WriteMessage("\n" + PluginExceptionHelper.Format(ex) + "\n");
            }
        }

        private static List<WallLineDto> CollectWalls(Database db)
        {
            var lines = new List<WallLineDto>();
            using (var tr = db.TransactionManager.StartTransaction())
            {
                var bt = (BlockTable)tr.GetObject(db.BlockTableId, OpenMode.ForRead);
                var ms = (BlockTableRecord)tr.GetObject(bt[BlockTableRecord.ModelSpace], OpenMode.ForRead);
                long n = 0;
                foreach (ObjectId id in ms)
                {
                    var line = tr.GetObject(id, OpenMode.ForRead) as Line;
                    if (line == null) continue;
                    var isAxis = string.Equals(line.Layer, LayerAxis, StringComparison.OrdinalIgnoreCase);
                    var isFace = string.Equals(line.Layer, LayerFace, StringComparison.OrdinalIgnoreCase);
                    if (!isAxis && !isFace) continue;

                    var a = line.StartPoint;
                    var b = line.EndPoint;
                    if (a.DistanceTo(b) < 1e-9) continue;

                    n++;
                    var role = isFace ? "face" : "axis";
                    int groupId;
                    double thicknessM;
                    ReadXData(line, out groupId, out thicknessM);
                    var lenM = CadUnits.ToMillimeters(a.DistanceTo(b)) / 1000.0;
                    lines.Add(new WallLineDto
                    {
                        Id = n,
                        WallRole = role,
                        WallGroupId = groupId > 0 ? groupId : (long?)null,
                        P1Mm = ToDesingMm(a),
                        P2Mm = ToDesingMm(b),
                        DataLong = lenM,
                        DataWith = thicknessM,
                        DataHeight = DefaultHeightM,
                        TextSystem = "Atk-60"
                    });
                }
                tr.Commit();
            }
            return lines;
        }

        private static XyzMmDto ToDesingMm(Point3d p)
        {
            return new XyzMmDto
            {
                X = CadUnits.ToMillimeters(p.X),
                Y = CadUnits.ToMillimeters(p.Z),
                Z = CadUnits.ToMillimeters(p.Y)
            };
        }

        private static void ReadXData(Line line, out int groupId, out double thicknessM)
        {
            groupId = 0;
            thicknessM = 0.30;
            try
            {
                if (line.XData == null) return;
                var inApp = false;
                foreach (var t in line.XData.AsArray())
                {
                    if (t.TypeCode == (int)DxfCode.ExtendedDataRegAppName)
                    {
                        inApp = string.Equals(t.Value as string, AppName, StringComparison.OrdinalIgnoreCase);
                        continue;
                    }
                    if (!inApp) continue;
                    if (t.TypeCode == (int)DxfCode.ExtendedDataInteger32)
                        groupId = Convert.ToInt32(t.Value);
                    else if (t.TypeCode == (int)DxfCode.ExtendedDataReal)
                        thicknessM = CadUnits.ToMillimeters(Convert.ToDouble(t.Value)) / 1000.0;
                }
            }
            catch
            {
            }
        }
    }
}
