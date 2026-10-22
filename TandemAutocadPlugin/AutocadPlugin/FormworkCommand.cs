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
                overlay = Wall3dProgressWindow.ShowOverAcad("Encofrando ATK-60…");
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

            ed.WriteMessage($"\nServidor: {elements.Count} pieza(s). Insertando paneles...\n");

            int inserted;
            int panels;
            int clamps;
            int stiffeners;
            int skipped;
            var problems = new List<string>();
            try
            {
                InsertPanels(doc, elements, problems, out inserted, out panels, out clamps, out stiffeners, out skipped);
            }
            catch (System.Exception ex)
            {
                ed.WriteMessage("\n" + PluginExceptionHelper.Format(ex) + "\n");
                return;
            }

            ed.WriteMessage(
                $"\nEncofrado ATK-60: {inserted} pieza(s) DWG ({panels} paneles, {clamps} grapas, {stiffeners} rigidizadores)"
                + (skipped > 0 ? $", {skipped} sin DWG local" : "")
                + $" (muros {resp.WallsCount}, piezas {elements.Count}).\n");
            for (var i = 0; i < problems.Count; i++)
                ed.WriteMessage("\n" + problems[i] + "\n");
        }

        private sealed class AxisWall
        {
            public string Id;
            public long WallDbId;
            public double StartXmm;
            public double StartYmm;
            public double EndXmm;
            public double EndYmm;
            public double ThicknessM;
            public double LengthMm;
            public double HeightM;
            public WallEditAttrs Attrs;
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

                    var attrs = WallEditAttrs.Load(WallCadXData.ReadAttrsJson(line));
                    walls.Add(new AxisWall
                    {
                        Id = wallDbId > 0
                            ? wallDbId.ToString(System.Globalization.CultureInfo.InvariantCulture)
                            : "axis-" + n,
                        WallDbId = wallDbId,
                        StartXmm = sx,
                        StartYmm = sy,
                        EndXmm = ex,
                        EndYmm = ey,
                        ThicknessM = ThicknessMeters(line),
                        LengthMm = len,
                        HeightM = attrs.HeightM > 0 ? attrs.HeightM : DefaultHeightM,
                        Attrs = attrs
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
                    Attributes = WallAttributes(w)
                });
            }
            return JsonConvert.SerializeObject(list);
        }

        private static Dictionary<string, object> WallAttributes(AxisWall w)
        {
            var a = w.Attrs ?? new WallEditAttrs();
            var map = new Dictionary<string, object>
            {
                { "_Datalong", w.LengthMm / 1000.0 },
                { "_DataWith", w.ThicknessM },
                { "_DataHeight", w.HeightM > 0 ? w.HeightM : DefaultHeightM },
                { "InicioX", w.StartXmm },
                { "InicioY", w.StartYmm },
                { "InicioZ", 0d },
                { "FinX", w.EndXmm },
                { "FinY", w.EndYmm },
                { "FinZ", 0d },
                { "Tipo", "Line" },
                { "_CHeckBracketInside", a.BracketIn },
                { "_CHeckBracketOutside", a.BracketOut },
                { "_CHeckRijiInside", a.StiffIn },
                { "_CHeckRijiOutside", a.StiffOut },
                { "_CHeck750R", a.Panel750 },
                { "_IsUniversalPanel", a.Panel750 },
                { "_CHeckTypeFormworkMode", a.VerticalPanels },
                { "_CHeckPropInside", a.PropIn },
                { "_CHeckPropOutside", a.PropOut },
                { "_CHeckPropInsideInf", a.PropInInf },
                { "_CHeckPropOutsideInf", a.PropOutInf },
                { "_ChekSolapeIni", a.SolapeIni },
                { "_ChekSolapeEnd", a.SolapeEnd }
            };
            if (!string.IsNullOrWhiteSpace(a.TapeIni))
                map["_Tape_1"] = a.TapeIni;
            if (!string.IsNullOrWhiteSpace(a.TapeEnd))
                map["_Tape_2"] = a.TapeEnd;
            return map;
        }

        private static void InsertPanels(
            Document doc,
            List<Atk60FormworkElement> elements,
            List<string> problems,
            out int inserted,
            out int panels,
            out int clamps,
            out int stiffeners,
            out int skipped)
        {
            inserted = 0;
            panels = 0;
            clamps = 0;
            stiffeners = 0;
            skipped = 0;
            var db = doc.Database;
            var prev = WallSpecialCad.SuppressEraseWatch;
            WallSpecialCad.SuppressEraseWatch = true;
            var meterToDwg = CadUnits.FromMillimeters(1000.0);
            const string view = "3dref";
            var blockCache = new Dictionary<string, ObjectId>(StringComparer.OrdinalIgnoreCase);

            try
            {
            // Primero paneles y grapas, y se confirman. Si el DWG del
            // rigidizador no abre, el muro ya queda encofrado.
            LoadResolvedBlocks(doc, elements, blockCache, problems, stiffenersOnly: false);
            PlaceResolved(doc, db, elements, blockCache, view, meterToDwg, eraseFirst: true, stiffenersOnly: false,
                ref inserted, ref panels, ref clamps, ref stiffeners, ref skipped);
            LoadResolvedBlocks(doc, elements, blockCache, problems, stiffenersOnly: true);
            PlaceResolved(doc, db, elements, blockCache, view, meterToDwg, eraseFirst: false, stiffenersOnly: true,
                ref inserted, ref panels, ref clamps, ref stiffeners, ref skipped);
            }
            finally
            {
                WallSpecialCad.SuppressEraseWatch = prev;
            }
        }

        private static void LoadResolvedBlocks(
            Document doc,
            List<Atk60FormworkElement> elements,
            Dictionary<string, ObjectId> blockCache,
            List<string> problems,
            bool stiffenersOnly)
        {
            const string view = "3dref";
            foreach (var item in elements)
            {
                if (item == null)
                    continue;
                var code = CodeNameFrom(item.ElementCode, item.ImportPath);
                if (string.IsNullOrWhiteSpace(code) || blockCache.ContainsKey(code))
                    continue;
                if (IsStiffener(code) != stiffenersOnly)
                    continue;
                string dwg;
                try
                {
                    dwg = Atk60DwgResolver.ResolveDwg(code, view);
                }
                catch (System.Exception ex)
                {
                    problems.Add(code + ": " + ex.Message);
                    blockCache[code] = ObjectId.Null;
                    continue;
                }
                if (string.IsNullOrWhiteSpace(dwg))
                {
                    blockCache[code] = ObjectId.Null;
                    continue;
                }
                try
                {
                    blockCache[code] = BlockInsertCommand.EnsureBlockDefinition(
                        doc, dwg, BlockInsertCommand.BlockNameFor(code, view));
                }
                catch (System.Exception ex)
                {
                    problems.Add(code + ": no se abrió el DWG (" + ex.Message + ")");
                    blockCache[code] = ObjectId.Null;
                }
            }
        }

        private static void PlaceResolved(
            Document doc,
            Database db,
            List<Atk60FormworkElement> elements,
            Dictionary<string, ObjectId> blockCache,
            string view,
            double meterToDwg,
            bool eraseFirst,
            bool stiffenersOnly,
            ref int inserted,
            ref int panels,
            ref int clamps,
            ref int stiffeners,
            ref int skipped)
        {
            using (doc.LockDocument())
            using (var tr = db.TransactionManager.StartTransaction())
            {
                EnsureFormworkLayer(tr, db);
                var bt = (BlockTable)tr.GetObject(db.BlockTableId, OpenMode.ForRead);
                var ms = (BlockTableRecord)tr.GetObject(bt[BlockTableRecord.ModelSpace], OpenMode.ForWrite);
                if (eraseFirst)
                    EraseLayerEntities(tr, ms, LayerFormwork);

                foreach (var item in elements)
                {
                    if (item == null)
                        continue;
                    var code = CodeNameFrom(item.ElementCode, item.ImportPath);
                    if (string.IsNullOrWhiteSpace(code) || IsStiffener(code) != stiffenersOnly)
                        continue;
                    ObjectId blockId;
                    if (!blockCache.TryGetValue(code, out blockId) || blockId.IsNull)
                    {
                        skipped++;
                        continue;
                    }

                    var tumbado = string.Equals(item.Orientation, "Tumbado", StringComparison.OrdinalIgnoreCase);
                    // El servidor sube el tumbado una altura de pieza: en el visor
                    // el giro lo deja colgando de ese punto. El DWG, al tumbarlo,
                    // crece hacia arriba, así que aquí el origen va al suelo de la hilada.
                    var upMm = item.Y;
                    if (tumbado && item.PieceHeightMm > 1)
                        upMm -= item.PieceHeightMm;
                    var at = new Point3d(
                        CadUnits.FromMillimeters(item.X),
                        CadUnits.FromMillimeters(item.Z),
                        CadUnits.FromMillimeters(upMm));
                    Matrix3d placed;
                    string role;
                    if (IsFixedClamp(code) || IsStiffener(code))
                    {
                        // Origen del punto = el del visor (ancla y 120 mm). No usa el giro de panel.
                        // El reflejo del paso a Z arriba va en la escala: BlockTransform
                        // rechaza un determinante negativo.
                        var seam = string.Equals(item.Orientation, "Horizontal", StringComparison.OrdinalIgnoreCase);
                        placed = FixedClampRotation(item, at, IsStiffener(code), IsShortStiffener(code) && seam);
                        if (IsStiffener(code))
                        {
                            // El DWG no está en los ejes del STL: el largo va en +Y y el
                            // grueso en +Z. El STL lleva el largo en Z y el cuerpo en −Y.
                            placed = placed * StiffenerBlockToStl(meterToDwg, code);
                        }
                        role = IsStiffener(code) ? "RIGI" : "UNION";
                        tumbado = false;
                    }
                    else
                    {
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
                        placed = Matrix3d.Rotation(Math.PI, Vector3d.ZAxis, pivot)
                            * Matrix3d.Displacement(at.GetAsVector())
                            * orient;
                        role = "PANEL";
                    }

                    var br = new BlockReference(Point3d.Origin, blockId);
                    br.Layer = LayerFormwork;
                    br.BlockTransform = placed;
                    if (role == "UNION" || role == "RIGI")
                        br.ScaleFactors = new Scale3d(meterToDwg, -meterToDwg, meterToDwg);
                    ms.AppendEntity(br);
                    tr.AddNewlyCreatedDBObject(br, true);
                    long wallId = 0;
                    if (!string.IsNullOrWhiteSpace(item.IdWall))
                    {
                        long.TryParse(
                            item.IdWall,
                            System.Globalization.NumberStyles.Integer,
                            System.Globalization.CultureInfo.InvariantCulture,
                            out wallId);
                    }
                    BlockInsertCommand.ApplyAtkXData(
                        br, tr, db, code, view, role, tumbado ? 90 : 0, wallId, at);
                    inserted++;
                    if (role == "RIGI")
                        stiffeners++;
                    else if (role == "UNION")
                        clamps++;
                    else
                        panels++;
                }

                tr.Commit();
            }
        }

        private static bool IsFixedClamp(string code)
        {
            return string.Equals(code, "10004220", StringComparison.OrdinalIgnoreCase);
        }

        private static bool IsShortStiffener(string code)
        {
            return string.Equals(code, "1850162", StringComparison.OrdinalIgnoreCase);
        }

        private static bool IsStiffener(string code)
        {
            return IsShortStiffener(code)
                || string.Equals(code, "1850163", StringComparison.OrdinalIgnoreCase);
        }

        /// <summary>
        /// El bloque está en metros, con el largo en +Y (de 0 a L), el ancho en +X
        /// (0 a 105 mm) y el grueso en +Z (0 a 60 mm). El STL tiene el largo en Z,
        /// centrado, y el cuerpo en −Y. Esta matriz se aplica antes de la escala.
        /// </summary>
        private static Matrix3d StiffenerBlockToStl(double meterToDwg, string code)
        {
            var length = IsShortStiffener(code) ? 0.75d : 1.60d;
            var local = Matrix3d.Rotation(Math.PI * 0.5, Vector3d.XAxis, Point3d.Origin)
                * Matrix3d.Displacement(new Vector3d(-0.0525d, -length * 0.5d + 0.02d, 0.020d));
            var s = meterToDwg;
            var scale = Matrix3d.AlignCoordinateSystem(
                Point3d.Origin, Vector3d.XAxis, Vector3d.YAxis, Vector3d.ZAxis,
                Point3d.Origin,
                new Vector3d(s, 0d, 0d),
                new Vector3d(0d, -s, 0d),
                new Vector3d(0d, 0d, s));
            return scale * local * scale.Inverse();
        }

        /// <summary>
        /// Giro del visor, pasado a Z arriba. La grapa usa el mismo Rx en las dos
        /// caras para que la marca Atenko quede derecha. El rigidizador sí sigue
        /// el reflejo del visor.
        /// </summary>
        private static Matrix3d FixedClampRotation(Atk60FormworkElement item, Point3d at)
        {
            return FixedClampRotation(item, at, false, false);
        }

        /// <param name="honorMirror">El visor gira la cara de detrás al revés. La grapa no: la marca Atenko tiene que quedar derecha.</param>
        /// <param name="planQuarter">Solo el rigidizador corto en junta horizontal: +90° en planta, como el visor.</param>
        private static Matrix3d FixedClampRotation(
            Atk60FormworkElement item, Point3d at, bool honorMirror, bool planQuarter)
        {
            var seam = item != null
                && string.Equals(item.Orientation, "Horizontal", StringComparison.OrdinalIgnoreCase);
            var mirrored = honorMirror && item != null && item.IsMirrored;
            var nx = item != null ? item.NormalX : 0d;
            var nz = item != null ? item.NormalZ : 1d;
            if (mirrored)
            {
                nx = -nx;
                nz = -nz;
            }
            if (nx * nx + nz * nz < 1e-12)
            {
                nx = 0d;
                nz = 1d;
            }
            var len = Math.Sqrt(nx * nx + nz * nz);
            nx /= len;
            nz /= len;
            var wallYaw = Math.Atan2(-nx, nz);

            double rx;
            double ry;
            double rz;
            if (seam)
            {
                rx = 0d;
                ry = mirrored ? Math.PI * 0.5 : Math.PI * 1.5;
                rz = Math.PI * 0.5;
            }
            else
            {
                rx = mirrored ? Math.PI * 0.5 : -Math.PI * 0.5;
                ry = 0d;
                rz = 0d;
            }

            var q = Quat.FromAxisY(-wallYaw).Multiply(Quat.FromEulerXyz(rx, ry, rz));
            if (planQuarter)
                q = Quat.FromAxisY(Math.PI * 0.5).Multiply(q);
            double m00, m01, m02, m10, m11, m12, m20, m21, m22;
            q.ToMatrix(out m00, out m01, out m02, out m10, out m11, out m12, out m20, out m21, out m22);

            // S cambia (x,y,z) del visor por (x,z,y). Refleja, así que se
            // compensa con la escala Y negativa de la inserción.
            var x = new Vector3d(m00, m20, m10);
            var y = new Vector3d(-m01, -m21, -m11);
            var z = new Vector3d(m02, m22, m12);
            var rot = Matrix3d.AlignCoordinateSystem(
                Point3d.Origin, Vector3d.XAxis, Vector3d.YAxis, Vector3d.ZAxis,
                Point3d.Origin, x, y, z);
            return Matrix3d.Displacement(at.GetAsVector()) * rot;
        }

        private struct Quat
        {
            public double X, Y, Z, W;

            public static Quat FromEulerXyz(double rx, double ry, double rz)
            {
                var c1 = Math.Cos(rx * 0.5);
                var c2 = Math.Cos(ry * 0.5);
                var c3 = Math.Cos(rz * 0.5);
                var s1 = Math.Sin(rx * 0.5);
                var s2 = Math.Sin(ry * 0.5);
                var s3 = Math.Sin(rz * 0.5);
                return new Quat
                {
                    X = s1 * c2 * c3 + c1 * s2 * s3,
                    Y = c1 * s2 * c3 - s1 * c2 * s3,
                    Z = c1 * c2 * s3 + s1 * s2 * c3,
                    W = c1 * c2 * c3 - s1 * s2 * s3
                };
            }

            public static Quat FromAxisY(double angle)
            {
                var h = angle * 0.5;
                return new Quat { X = 0d, Y = Math.Sin(h), Z = 0d, W = Math.Cos(h) };
            }

            public Quat Multiply(Quat b)
            {
                return new Quat
                {
                    X = W * b.X + X * b.W + Y * b.Z - Z * b.Y,
                    Y = W * b.Y - X * b.Z + Y * b.W + Z * b.X,
                    Z = W * b.Z + X * b.Y - Y * b.X + Z * b.W,
                    W = W * b.W - X * b.X - Y * b.Y - Z * b.Z
                };
            }

            public void ToMatrix(
                out double m00, out double m01, out double m02,
                out double m10, out double m11, out double m12,
                out double m20, out double m21, out double m22)
            {
                var x2 = X + X;
                var y2 = Y + Y;
                var z2 = Z + Z;
                var xx = X * x2;
                var xy = X * y2;
                var xz = X * z2;
                var yy = Y * y2;
                var yz = Y * z2;
                var zz = Z * z2;
                var wx = W * x2;
                var wy = W * y2;
                var wz = W * z2;
                m00 = 1d - (yy + zz);
                m01 = xy - wz;
                m02 = xz + wy;
                m10 = xy + wz;
                m11 = 1d - (xx + zz);
                m12 = yz - wx;
                m20 = xz - wy;
                m21 = yz + wx;
                m22 = 1d - (xx + yy);
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
