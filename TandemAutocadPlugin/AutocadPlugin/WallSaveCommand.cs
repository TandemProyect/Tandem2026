using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using AutocadPlugin.Models;
using Newtonsoft.Json;
using AutocadPlugin.UI.Views;
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
    /// Guarda muros y artículos insertados a mano al pulsar el icono Salvar.
    /// Insertar un panel solo lo deja en el DWG; la BD se actualiza aquí.
    /// </summary>
    public class WallSaveCommand
    {
        public const string CommandName = "TANDEM_SALVAR";
        private const string LayerAxis = "TANDEM_MURO_EJE";
        private const string LayerFace = "TANDEM_MURO_CARA";
        private const string LayerFormwork = "TANDEM_ENCOFRADO";
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

            List<WallLineDto> lines;
            List<PluginSaveWallArticleRequest> articles;
            Collect(doc.Database, designId, out lines, out articles);
            if (lines.Count == 0)
            {
                ed.WriteMessage("\nNo hay muros (capas TANDEM_MURO_EJE / TANDEM_MURO_CARA).\n");
                return;
            }

            ed.WriteMessage($"\nSalvar diseño {designId}: {lines.Count} línea(s), {articles.Count} artículo(s)...\n");
            ed.WriteMessage("\nPosiciones en c:\\temp\\Posicion.json\n");
            Wall3dProgressWindow overlay = null;
            PluginSaveWallsResponse resp = null;
            try
            {
                overlay = Wall3dProgressWindow.ShowOverAcad("Salvando en base de datos…");
                var api = new MVCApiService();
                resp = overlay.Wait(() =>
                    api.SaveDesignWallsAsync(designId, PluginDeviceId.Current(), lines, articles)
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
                    : "No se pudieron guardar los muros.") + "\n");
                return;
            }

            TandemToastWindow.ShowSuccess("Diseño salvado");
            ed.WriteMessage($"\nGuardados {resp.Count} muro(s) y {resp.ArticleCount} artículo(s) en el diseño {designId}.\n");
        }

        /// <summary>
        /// Tras borrar el último panel o un elemento de encofrado automático:
        /// actualiza Is_Special en SQL sin bloquear el hilo de AutoCAD.
        /// </summary>
        public static void PersistSilent(Document doc)
        {
            if (doc == null)
                return;
            var designId = WallImportCommand.CurrentDesignId;
            if (designId <= 0)
                return;
            List<WallLineDto> lines;
            List<PluginSaveWallArticleRequest> articles;
            try
            {
                Collect(doc.Database, designId, out lines, out articles);
            }
            catch
            {
                return;
            }
            if (lines == null || lines.Count == 0)
                return;
            System.Threading.Tasks.Task.Run(() =>
            {
                try
                {
                    var api = new MVCApiService();
                    api.SaveDesignWallsAsync(designId, PluginDeviceId.Current(), lines, articles)
                        .ConfigureAwait(false).GetAwaiter().GetResult();
                }
                catch
                {
                }
            });
        }

        private static void Collect(
            Database db,
            long designId,
            out List<WallLineDto> lines,
            out List<PluginSaveWallArticleRequest> articles)
        {
            lines = new List<WallLineDto>();
            articles = new List<PluginSaveWallArticleRequest>();
            var axes = new List<AxisSnap>();
            var shots = new List<object>();
            using (var tr = db.TransactionManager.StartTransaction())
            {
                WallSpecialCad.RecalcUnlocked(tr, db);
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
                    string xdRole;
                    int groupId;
                    double thicknessDwg;
                    long wallDbId;
                    bool isSpecial;
                    WallCadXData.Read(line, out xdRole, out groupId, out thicknessDwg, out wallDbId, out isSpecial);
                    var role = isFace ? "face" : "axis";
                    var thicknessM = CadUnits.ToMillimeters(thicknessDwg) / 1000.0;
                    var lenM = CadUnits.ToMillimeters(a.DistanceTo(b)) / 1000.0;
                    var p1 = WallCadXData.ToDesingMm(a);
                    var p2 = WallCadXData.ToDesingMm(b);
                    lines.Add(new WallLineDto
                    {
                        Id = n,
                        WallRole = role,
                        WallGroupId = groupId > 0 ? groupId : (long?)null,
                        P1Mm = p1,
                        P2Mm = p2,
                        DataLong = lenM,
                        DataWith = thicknessM,
                        DataHeight = DefaultHeightM,
                        TextSystem = "Atk-60",
                        WallDbId = wallDbId > 0 ? wallDbId : (long?)null,
                        IsSpecial = isSpecial
                    });

                    if (isAxis)
                    {
                        axes.Add(new AxisSnap
                        {
                            A = a,
                            B = b,
                            P1Mm = p1,
                            P2Mm = p2,
                            Thickness = thicknessDwg,
                            DataWith = thicknessM,
                            DataLong = lenM,
                            WallDbId = wallDbId,
                            IsSpecial = isSpecial
                        });
                    }
                }

                foreach (ObjectId id in ms)
                {
                    var br = tr.GetObject(id, OpenMode.ForRead) as BlockReference;
                    if (br == null) continue;
                    var isFormwork = string.Equals(br.Layer, LayerFormwork, StringComparison.OrdinalIgnoreCase);

                    string code;
                    string view;
                    string role;
                    int rotDeg;
                    if (!BlockInsertCommand.TryReadAtk(br, out code, out view, out role, out rotDeg))
                        continue;

                    var wallDbId = BlockInsertCommand.WallIdFromInsert(br);
                    AxisSnap axis = null;
                    if (wallDbId > 0)
                    {
                        for (var i = 0; i < axes.Count; i++)
                        {
                            if (axes[i].WallDbId == wallDbId)
                            {
                                axis = axes[i];
                                break;
                            }
                        }
                    }
                    if (axis == null && !TryNearestAxis(BlockInsertCommand.InsertWorld(br), axes, out axis))
                    {
                        if (wallDbId <= 0)
                            continue;
                    }
                    if (isFormwork && (axis == null || !axis.IsSpecial))
                        continue;

                    var insertAt = BlockInsertCommand.InsertWorld(br);
                    var blockPos = br.Position;
                    var insert = WallCadXData.ToDesingMm(insertAt);
                    var blockMm = WallCadXData.ToDesingMm(blockPos);
                    int poseX = rotDeg;
                    int poseY = 0;
                    double poseZ = BlockInsertCommand.YawDegFromOrient(br.BlockTransform);
                    if (axis != null)
                    {
                        BlockInsertCommand.ArticleRotations(
                            insertAt, axis.A, axis.B, br.BlockTransform,
                            out poseX, out poseY, out poseZ);
                    }
                    poseZ = BlockInsertCommand.EncodePoseZ(br);
                    articles.Add(new PluginSaveWallArticleRequest
                    {
                        DesignId = designId,
                        WallDbId = wallDbId > 0
                            ? wallDbId
                            : (axis != null && axis.WallDbId > 0 ? axis.WallDbId : (long?)null),
                        CodeName = code,
                        View = view,
                        P1Mm = axis != null ? axis.P1Mm : null,
                        P2Mm = axis != null ? axis.P2Mm : null,
                        DataWith = axis != null ? axis.DataWith : (double?)null,
                        DataLong = axis != null ? axis.DataLong : (double?)null,
                        DataHeight = DefaultHeightM,
                        TextSystem = "Atk-60",
                        InsertMm = insert,
                        RotationX = poseX,
                        RotationY = poseY,
                        RotationZ = poseZ,
                        HandleCad = br.Handle.ToString()
                    });
                    shots.Add(new
                    {
                        handle = br.Handle.ToString(),
                        code,
                        view,
                        role,
                        isFormwork,
                        wallDbId = wallDbId > 0
                            ? wallDbId
                            : (axis != null && axis.WallDbId > 0 ? axis.WallDbId : 0),
                        rotationX = poseX,
                        rotationY = poseY,
                        rotationZ = poseZ,
                        logicalDwg = DwgPoint(insertAt),
                        blockPositionDwg = DwgPoint(blockPos),
                        deltaBlockMinusLogicalMm = new
                        {
                            x = CadUnits.ToMillimeters(blockPos.X - insertAt.X),
                            y = CadUnits.ToMillimeters(blockPos.Y - insertAt.Y),
                            z = CadUnits.ToMillimeters(blockPos.Z - insertAt.Z)
                        },
                        sentToDatabaseMm = MmPoint(insert),
                        blockPositionAsDesingMm = MmPoint(blockMm),
                        wall = axis == null ? null : new
                        {
                            wallDbId = axis.WallDbId,
                            isSpecial = axis.IsSpecial,
                            thicknessM = axis.DataWith,
                            lengthM = axis.DataLong,
                            p1Dwg = DwgPoint(axis.A),
                            p2Dwg = DwgPoint(axis.B),
                            p1Mm = MmPoint(axis.P1Mm),
                            p2Mm = MmPoint(axis.P2Mm)
                        }
                    });

                    if (axis != null)
                        MarkLineSpecial(lines, axis);
                }
                tr.Commit();
            }

            WritePosicionJson(designId, axes, shots);
        }

        /// <summary>
        /// Foto de lo que hay en el DWG frente a lo que se manda a SQL.
        /// logicalDwg / sentToDatabaseMm es el origen AT: (lo que debe recuperarse).
        /// blockPosition es el Position de AutoCAD tras el giro 180° del DWG.
        /// </summary>
        private static void WritePosicionJson(long designId, IList<AxisSnap> axes, IList<object> shots)
        {
            try
            {
                var walls = new List<object>();
                if (axes != null)
                {
                    foreach (var axis in axes)
                    {
                        walls.Add(new
                        {
                            wallDbId = axis.WallDbId,
                            isSpecial = axis.IsSpecial,
                            thicknessM = axis.DataWith,
                            lengthM = axis.DataLong,
                            p1Dwg = DwgPoint(axis.A),
                            p2Dwg = DwgPoint(axis.B),
                            p1Mm = MmPoint(axis.P1Mm),
                            p2Mm = MmPoint(axis.P2Mm)
                        });
                    }
                }

                var doc = new
                {
                    capturedAt = DateTime.Now.ToString("o", CultureInfo.InvariantCulture),
                    designId,
                    units = new
                    {
                        dwg = "unidades de dibujo de AutoCAD",
                        desingMm = "X planta, Y alzado, Z planta (ToDesingMm)"
                    },
                    note = "sentToDatabaseMm debe coincidir con NumberInsertX/Y/Z. Si blockPositionAsDesingMm difiere, Position no es el origen.",
                    walls,
                    articles = shots ?? new List<object>()
                };
                Directory.CreateDirectory(@"c:\temp");
                File.WriteAllText(
                    @"c:\temp\Posicion.json",
                    JsonConvert.SerializeObject(doc, Formatting.Indented));
            }
            catch
            {
            }
        }

        private static object DwgPoint(Point3d p)
        {
            return new { x = p.X, y = p.Y, z = p.Z };
        }

        private static object MmPoint(XyzMmDto p)
        {
            if (p == null)
                return null;
            return new { x = p.X, y = p.Y, z = p.Z };
        }

        private static void MarkLineSpecial(List<WallLineDto> lines, AxisSnap axis)
        {
            for (var i = 0; i < lines.Count; i++)
            {
                var line = lines[i];
                if (line == null || !string.Equals(line.WallRole, "axis", StringComparison.OrdinalIgnoreCase))
                    continue;
                if (axis.WallDbId > 0 && line.WallDbId == axis.WallDbId)
                {
                    line.IsSpecial = true;
                    continue;
                }
                if (SamePoint(line.P1Mm, axis.P1Mm) && SamePoint(line.P2Mm, axis.P2Mm))
                    line.IsSpecial = true;
            }
        }

        private static bool SamePoint(XyzMmDto a, XyzMmDto b)
        {
            if (a == null || b == null)
                return false;
            return Math.Abs((a.X ?? 0) - (b.X ?? 0)) < 0.5
                && Math.Abs((a.Y ?? 0) - (b.Y ?? 0)) < 0.5
                && Math.Abs((a.Z ?? 0) - (b.Z ?? 0)) < 0.5;
        }

        private static bool TryNearestAxis(Point3d at, IList<AxisSnap> axes, out AxisSnap nearest)
        {
            nearest = null;
            var best = double.MaxValue;
            for (var i = 0; i < axes.Count; i++)
            {
                var axis = axes[i];
                var dist = WallCadXData.DistToSegment(at, axis.A, axis.B);
                var band = Math.Max(axis.Thickness * 1.6, CadUnits.FromMillimeters(400));
                if (dist > band || dist >= best)
                    continue;
                best = dist;
                nearest = axis;
            }
            return nearest != null;
        }

        private sealed class AxisSnap
        {
            public Point3d A;
            public Point3d B;
            public XyzMmDto P1Mm;
            public XyzMmDto P2Mm;
            public double Thickness;
            public double DataWith;
            public double DataLong;
            public long WallDbId;
            public bool IsSpecial;
        }
    }
}
