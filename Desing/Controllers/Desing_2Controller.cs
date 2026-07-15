using DAL;

using Desing.Helpers;

using Desing.Models;
using Desing.Repositories.RepositoryAtk60;
using Desing.Repositories.RepositoryCommun;
using Desing.Resources;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Web;
using System.Web.Mvc;



namespace Desing.Controllers

{

    /// <summary>

    /// Espacio de diseño «v2» (visor Three.js / STL). La edición de diseño creado desde la oferta puede redirigir aquí con el STL correspondiente.

    /// </summary>

    [Authorize]

    public class Desing_2Controller : BaseController

    {

        /// <summary>

        /// Visor STL (misma pieza DOM y script que Artículos maestros).

        /// Query opcional: <paramref name="stlUrl"/> (virtual ~/…), <paramref name="offerId"/>, <paramref name="designId"/>.

        /// Sin STL la escena arranca vacía; con <paramref name="autoLoad"/> = 1 y STL válido se carga al iniciar (enlaces desde la oferta).

        /// </summary>

        public ActionResult Viewer(string stlUrl, long? offerId, long? designId, int? autoLoad)

        {

            string resolvedContentUrl = null;

            if (!string.IsNullOrWhiteSpace(stlUrl))

            {

                var virt = ApplicationStlUrlHelper.TryGetTrustedStlVirtualPath(stlUrl);

                if (virt == null)

                {

                    return HttpNotFound();

                }



                resolvedContentUrl = Url.Content(virt);

            }



            var auto = autoLoad == 1 && !string.IsNullOrEmpty(resolvedContentUrl);



            var statusFooter = auto

                ? Jobside.OfferWorkspace_Designs_ViewerAutoLoadPending

                : Jobside.OfferWorkspace_Designs_ViewerCanvasEmpty;



            var model = new Desing2ViewerPageModel

            {

                TextColor1Hex = "#efdf34",

                TextColor2Hex = "#a18c8c",

                InitialStlUrl = resolvedContentUrl,

                InitialStlLabel = offerId.HasValue

                    ? "Diseño — oferta " + offerId.Value.ToString(CultureInfo.InvariantCulture)

                    : "Planta 3D",

                AutoLoadInitialStl = auto,

                InitialStatusFooter = statusFooter,

                OfferId = offerId,

                DesignId = designId,

                BrandLogoUrl = ResolvePlantillaLogoUrl(Url, ViewBag.PlantillaLogo as string)

            };



            FillOfferDesignContext(model, offerId, designId);



            ViewBag.BodyHtmlClass = "desing2-stl-fullpage";



            return View(model);

        }



        /// <summary>
        /// Guarda el estado topológico de muros/polilíneas que prepara el visor Desing_2.
        /// </summary>
        [HttpPost]
        public JsonResult SaveWallConnections()
        {
            try
            {
                if (Request.InputStream.CanSeek)
                {
                    Request.InputStream.Position = 0;
                }
                string rawJson;
                using (var reader = new StreamReader(Request.InputStream, Encoding.UTF8))
                {
                    rawJson = reader.ReadToEnd();
                }

                if (string.IsNullOrWhiteSpace(rawJson))
                {
                    return Json(new { Exito = false, Mensaje = "JSON vacío." });
                }

                var parsed = JToken.Parse(rawJson);
                var dir = Server.MapPath("~/IA/Communication");
                if (!Directory.Exists(dir))
                {
                    Directory.CreateDirectory(dir);
                }

                var targetPath = Path.Combine(dir, "WallConnections.json");
                var formatted = JsonConvert.SerializeObject(parsed, Formatting.Indented);
                System.IO.File.WriteAllText(targetPath, formatted, new UTF8Encoding(false));

                return Json(new { Exito = true, Path = targetPath });
            }
            catch (JsonReaderException ex)
            {
                return Json(new { Exito = false, Mensaje = "JSON inválido: " + ex.Message });
            }
            catch (Exception ex)
            {
                return Json(new { Exito = false, Mensaje = ex.Message });
            }
        }

        /// <summary>
        /// Guarda un diagnóstico ampliado de muros en una ruta local estable para análisis con agentes.
        /// </summary>
        [HttpPost]
        public JsonResult SaveWallDiagnostics()
        {
            try
            {
                if (Request.InputStream.CanSeek)
                {
                    Request.InputStream.Position = 0;
                }

                string rawJson;
                using (var reader = new StreamReader(Request.InputStream, Encoding.UTF8))
                {
                    rawJson = reader.ReadToEnd();
                }

                if (string.IsNullOrWhiteSpace(rawJson))
                {
                    return Json(new { Exito = false, Mensaje = "JSON vacío." });
                }

                var parsed = JToken.Parse(rawJson);
                var dir = @"C:\temp";
                if (!Directory.Exists(dir))
                {
                    Directory.CreateDirectory(dir);
                }

                var targetPath = Path.Combine(dir, "WallDiagnostics.json");
                var formatted = JsonConvert.SerializeObject(parsed, Formatting.Indented);
                System.IO.File.WriteAllText(targetPath, formatted, new UTF8Encoding(false));

                return Json(new { Exito = true, Path = targetPath });
            }
            catch (JsonReaderException ex)
            {
                return Json(new { Exito = false, Mensaje = "JSON inválido: " + ex.Message });
            }
            catch (Exception ex)
            {
                return Json(new { Exito = false, Mensaje = ex.Message });
            }
        }

        /// <summary>
        /// Primer endpoint de encofrado por sistema (ATK-60).
        /// Recibe lista de muros con atributos dinámicos para preparar la lógica de materiales.
        /// </summary>
        [HttpPost]
        [ActionName("GetWallsAtk-60")]
        public JsonResult GetWallsAtk60(Desing2WallIdsRequest idsRequest)
        {
            try
            {
                var jsonRaw = idsRequest != null ? idsRequest.IdsJson : null;
                const string buildStamp = "ATK60-BACKEND-2026-07-15-03:00";
                var repository = new Atk60WallsRepository(new FormworkJsonCommonRepository());
                var payload = repository.BuildPayloadFromIdsJson(jsonRaw);

                var walls = payload.Walls;
                var modulos = repository.GetWallsForCadSystems(walls);
                var elementsForThreeJs = repository.BuildThreeJsPaintPayload(walls, modulos);
                SaveAtk60RequestDebugToTem(jsonRaw, walls, buildStamp);

                // Aqui insertaremos la logica de encofrado ATK-60 a partir de la lista de muros rectos.
                // Cada item ya llega con su Id y con sus atributos dinamicos.
                return Json(new
                {
                    Exito = true,
                    System = payload.System,
                    WallsCount = walls != null ? walls.Count : 0,
                    ListCount = payload.List != null ? payload.List.Count : 0,
                    IdsJsonCount = !string.IsNullOrWhiteSpace(jsonRaw) ? (walls != null ? walls.Count : 0) : 0,
                    IdsJsonLength = string.IsNullOrWhiteSpace(jsonRaw) ? 0 : jsonRaw.Length,
                    ModulosCount = modulos != null ? modulos.Count : 0,
                    Modulos = modulos,
                    ElementsForThreeJsCount = elementsForThreeJs != null && elementsForThreeJs.Elements != null
                        ? elementsForThreeJs.Elements.Count
                        : 0,
                    ElementsForThreeJs = elementsForThreeJs,
                    Walls = walls
                });
            }
            catch (Exception ex)
            {
                return Json(new { Exito = false, Mensaje = ex.Message });
            }
        }

        private static string SaveAtk60RequestDebugToTem(string jsonRaw, List<Desing2FormworkWallDto> walls, string buildStamp)
        {
            var dir = @"C:\tem";
            if (!Directory.Exists(dir))
            {
                Directory.CreateDirectory(dir);
            }

            var walls3d = BuildAtk60Walls3dDebug(walls);
            var corners3d = BuildAtk60CornersDebug(walls);
            var debug = new
            {
                GeneratedAtUtc = DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ss.fffZ", CultureInfo.InvariantCulture),
                Source = "Desing_2Controller.GetWallsAtk60",
                BuildStamp = buildStamp,
                RawIdsJsonLength = string.IsNullOrWhiteSpace(jsonRaw) ? 0 : jsonRaw.Length,
                WallsCount = walls != null ? walls.Count : 0,
                Walls = walls ?? new List<Desing2FormworkWallDto>(),
                Walls3D = walls3d,
                Corners3D = corners3d,
            };

            var targetPath = Path.Combine(dir, "Atk60RequestWallsDebug.json");
            var formatted = JsonConvert.SerializeObject(debug, Formatting.Indented);
            System.IO.File.WriteAllText(targetPath, formatted, new UTF8Encoding(false));
            return targetPath;
        }

        private static List<object> BuildAtk60Walls3dDebug(List<Desing2FormworkWallDto> walls)
        {
            var result = new List<object>();
            if (walls == null || walls.Count == 0)
            {
                return result;
            }

            for (var i = 0; i < walls.Count; i++)
            {
                var wall = walls[i];
                if (wall == null)
                {
                    continue;
                }

                double sx;
                double sz;
                double ex;
                double ez;
                if (!TryGetWallEndpointsXzMm(wall, out sx, out sz, out ex, out ez))
                {
                    continue;
                }

                var dx = ex - sx;
                var dz = ez - sz;
                var lengthMm = Math.Sqrt(dx * dx + dz * dz);
                var yawRad = Math.Atan2(dz, dx);
                var idWall = !string.IsNullOrWhiteSpace(wall.WallId)
                    ? wall.WallId
                    : (!string.IsNullOrWhiteSpace(wall.Id) ? wall.Id : wall.LineId);

                var attrs = wall.Attributes;
                var widthMm = ResolveWallWidthMmDebug(attrs);
                var heightMm = ResolveWallHeightMmDebug(attrs);

                result.Add(new
                {
                    IdWall = idWall,
                    StartX = sx,
                    StartZ = sz,
                    EndX = ex,
                    EndZ = ez,
                    LengthMm = lengthMm,
                    YawRad = yawRad,
                    WidthMm = widthMm,
                    HeightMm = heightMm,
                });
            }

            return result;
        }

        private static List<object> BuildAtk60CornersDebug(List<Desing2FormworkWallDto> walls)
        {
            var result = new List<object>();
            if (walls == null || walls.Count == 0)
            {
                return result;
            }

            var nodes = new Dictionary<string, CornerNodeInfo>(StringComparer.OrdinalIgnoreCase);
            var endpointsByWall = new List<WallEndpointInfo>();

            for (var i = 0; i < walls.Count; i++)
            {
                var wall = walls[i];
                if (wall == null)
                {
                    continue;
                }

                double sx;
                double sz;
                double ex;
                double ez;
                if (!TryGetWallEndpointsXzMm(wall, out sx, out sz, out ex, out ez))
                {
                    continue;
                }

                var idWall = !string.IsNullOrWhiteSpace(wall.WallId)
                    ? wall.WallId
                    : (!string.IsNullOrWhiteSpace(wall.Id) ? wall.Id : wall.LineId);

                var startKey = BuildCornerKey(sx, sz);
                var endKey = BuildCornerKey(ex, ez);
                var dx = ex - sx;
                var dz = ez - sz;
                var len = Math.Sqrt(dx * dx + dz * dz);
                if (len < 1e-6)
                {
                    continue;
                }

                var ux = dx / len;
                var uz = dz / len;

                EnsureNode(nodes, startKey, sx, sz).Vectors.Add(new CornerVector { X = ux, Z = uz });
                EnsureNode(nodes, endKey, ex, ez).Vectors.Add(new CornerVector { X = -ux, Z = -uz });

                endpointsByWall.Add(new WallEndpointInfo
                {
                    IdWall = idWall,
                    StartKey = startKey,
                    EndKey = endKey,
                    StartX = sx,
                    StartZ = sz,
                    EndX = ex,
                    EndZ = ez,
                });
            }

            foreach (var ep in endpointsByWall)
            {
                CornerNodeInfo startNode;
                CornerNodeInfo endNode;
                nodes.TryGetValue(ep.StartKey, out startNode);
                nodes.TryGetValue(ep.EndKey, out endNode);

                result.Add(new
                {
                    IdWall = ep.IdWall,
                    Start = new
                    {
                        X = ep.StartX,
                        Z = ep.StartZ,
                        NodeKey = ep.StartKey,
                        Degree = startNode != null ? startNode.Vectors.Count : 0,
                        Type = ClassifyCornerType(startNode),
                    },
                    End = new
                    {
                        X = ep.EndX,
                        Z = ep.EndZ,
                        NodeKey = ep.EndKey,
                        Degree = endNode != null ? endNode.Vectors.Count : 0,
                        Type = ClassifyCornerType(endNode),
                    },
                });
            }

            return result;
        }

        private static CornerNodeInfo EnsureNode(Dictionary<string, CornerNodeInfo> nodes, string key, double x, double z)
        {
            CornerNodeInfo node;
            if (nodes.TryGetValue(key, out node) && node != null)
            {
                return node;
            }

            node = new CornerNodeInfo
            {
                Key = key,
                X = x,
                Z = z,
                Vectors = new List<CornerVector>(),
            };
            nodes[key] = node;
            return node;
        }

        private static string BuildCornerKey(double x, double z)
        {
            var kx = (int)Math.Round(x);
            var kz = (int)Math.Round(z);
            return kx.ToString(CultureInfo.InvariantCulture) + "|" + kz.ToString(CultureInfo.InvariantCulture);
        }

        private static string ClassifyCornerType(CornerNodeInfo node)
        {
            if (node == null || node.Vectors == null)
            {
                return "Unknown";
            }

            var degree = node.Vectors.Count;
            if (degree <= 0) return "Unknown";
            if (degree == 1) return "End";
            if (degree >= 4) return "X";
            if (degree == 3) return "T";

            var v1 = node.Vectors[0];
            var v2 = node.Vectors[1];
            var dot = (v1.X * v2.X) + (v1.Z * v2.Z);
            return Math.Abs(dot) >= 0.985 ? "I" : "L";
        }

        private static bool TryGetWallEndpointsXzMm(Desing2FormworkWallDto wall, out double sx, out double sz, out double ex, out double ez)
        {
            sx = sz = ex = ez = 0d;
            if (wall == null)
            {
                return false;
            }

            var attrs = wall.Attributes;
            var p1 = AttrExtraToken(attrs, "p1") as JObject;
            var p2 = AttrExtraToken(attrs, "p2") as JObject;

            var p1x = p1 != null ? TokenNumber(p1, "xMm") ?? TokenNumber(p1, "x") : null;
            var p1z = p1 != null ? TokenNumber(p1, "zMm") ?? TokenNumber(p1, "z") : null;
            var p2x = p2 != null ? TokenNumber(p2, "xMm") ?? TokenNumber(p2, "x") : null;
            var p2z = p2 != null ? TokenNumber(p2, "zMm") ?? TokenNumber(p2, "z") : null;

            var startX = SceneMm(p1x ?? (attrs != null ? attrs.ExtraValueAsDouble("InicioX") : null));
            var startZ = SceneMm(p1z ?? (attrs != null ? attrs.ExtraValueAsDouble("InicioY") : null));
            var endX = SceneMm(p2x ?? (attrs != null ? attrs.ExtraValueAsDouble("FinX") : null));
            var endZ = SceneMm(p2z ?? (attrs != null ? attrs.ExtraValueAsDouble("FinY") : null));

            if (!startX.HasValue || !startZ.HasValue || !endX.HasValue || !endZ.HasValue)
            {
                return false;
            }

            sx = startX.Value;
            sz = startZ.Value;
            ex = endX.Value;
            ez = endZ.Value;
            return true;
        }

        private static double? SceneMm(double? v)
        {
            if (!v.HasValue)
            {
                return null;
            }

            var abs = Math.Abs(v.Value);
            return abs <= 50d ? v.Value * 1000d : v.Value;
        }

        private static double ResolveWallWidthMmDebug(AttributesList attrs)
        {
            if (attrs == null)
            {
                return 300d;
            }

            var fromDataWith = SceneMm(attrs._DataWith ?? attrs.ExtraValueAsDouble("_DataWith"));
            if (fromDataWith.HasValue && fromDataWith.Value > 1)
            {
                return Math.Abs(fromDataWith.Value);
            }

            var fromThickness = SceneMm(attrs.ExtraValueAsDouble("ThicknessMm"));
            if (fromThickness.HasValue && fromThickness.Value > 1)
            {
                return Math.Abs(fromThickness.Value);
            }

            return 300d;
        }

        private static double ResolveWallHeightMmDebug(AttributesList attrs)
        {
            if (attrs == null)
            {
                return 2700d;
            }

            var fromDataHeight = SceneMm(attrs._DataHeight ?? attrs.ExtraValueAsDouble("_DataHeight"));
            if (fromDataHeight.HasValue && fromDataHeight.Value > 1)
            {
                return Math.Abs(fromDataHeight.Value);
            }

            var fromHeight = SceneMm(attrs.ExtraValueAsDouble("HeightMm"));
            if (fromHeight.HasValue && fromHeight.Value > 1)
            {
                return Math.Abs(fromHeight.Value);
            }

            return 2700d;
        }

        private static double? TokenNumber(JObject obj, string key)
        {
            if (obj == null || string.IsNullOrWhiteSpace(key))
            {
                return null;
            }

            JToken token;
            if (!obj.TryGetValue(key, out token) || token == null || token.Type == JTokenType.Null)
            {
                var prop = obj.Properties().FirstOrDefault(p =>
                    string.Equals(p.Name, key, StringComparison.OrdinalIgnoreCase));
                token = prop != null ? prop.Value : null;
                if (token == null || token.Type == JTokenType.Null)
                {
                    return null;
                }
            }

            if (token.Type == JTokenType.Float || token.Type == JTokenType.Integer)
            {
                return token.Value<double>();
            }

            var raw = token.ToString();
            double v;
            if (double.TryParse(raw, NumberStyles.Float | NumberStyles.AllowThousands, CultureInfo.InvariantCulture, out v))
            {
                return v;
            }
            if (double.TryParse(raw, NumberStyles.Float | NumberStyles.AllowThousands, CultureInfo.GetCultureInfo("es-ES"), out v))
            {
                return v;
            }

            raw = raw.Replace(',', '.');
            if (double.TryParse(raw, NumberStyles.Float | NumberStyles.AllowThousands, CultureInfo.InvariantCulture, out v))
            {
                return v;
            }

            return null;
        }

        private static JToken AttrExtraToken(AttributesList attrs, string key)
        {
            if (attrs == null || attrs.Extra == null || string.IsNullOrWhiteSpace(key))
            {
                return null;
            }

            JToken token;
            if (attrs.Extra.TryGetValue(key, out token) && token != null && token.Type != JTokenType.Null)
            {
                return token;
            }

            var match = attrs.Extra.FirstOrDefault(kv =>
                string.Equals(kv.Key, key, StringComparison.OrdinalIgnoreCase));
            return match.Value;
        }

        private sealed class WallEndpointInfo
        {
            public string IdWall { get; set; }
            public string StartKey { get; set; }
            public string EndKey { get; set; }
            public double StartX { get; set; }
            public double StartZ { get; set; }
            public double EndX { get; set; }
            public double EndZ { get; set; }
        }

        private sealed class CornerNodeInfo
        {
            public string Key { get; set; }
            public double X { get; set; }
            public double Z { get; set; }
            public List<CornerVector> Vectors { get; set; }
        }

        private sealed class CornerVector
        {
            public double X { get; set; }
            public double Z { get; set; }
        }

        private static string ResolvePlantillaLogoUrl(UrlHelper url, string plantillaLogoRaw)

        {

            var plantillaLogo = string.IsNullOrWhiteSpace(plantillaLogoRaw)

                ? "/Content/images/Login/at.png"

                : plantillaLogoRaw.Trim();

            if (plantillaLogo.StartsWith("http", StringComparison.OrdinalIgnoreCase)

                || plantillaLogo.StartsWith("//", StringComparison.Ordinal))

            {

                return plantillaLogo;

            }



            return url.Content("~" + (plantillaLogo.StartsWith("/") ? plantillaLogo : "/" + plantillaLogo));

        }



        /// <summary>

        /// Rellena <see cref="Desing2ViewerPageModel.ContextSubtitleLine"/> con obra / oferta / diseño si hay ids válidos en BD.

        /// </summary>

        private void FillOfferDesignContext(Desing2ViewerPageModel model, long? offerId, long? designId)

        {

            string jobsideCode = null;

            string offerNumber = null;

            string offerName = null;

            string designPart = null;
            if (offerId.HasValue)

            {
                var offer = db.TSql_Offers.AsNoTracking().FirstOrDefault(o => o.IdObject == offerId.Value && !o.Is_Delete);

                if (offer != null)

                {

                    offerNumber = offer.AddOfferNumber;

                    offerName = offer.TextLabel;

                    var js = db.TSql_Jobside.AsNoTracking().FirstOrDefault(j => j.IdObject == offer.LinkJobside && !j.Is_Delete);

                    if (js != null)

                    {

                        jobsideCode = js.AddNJobside;

                    }

                }

            }



            if (designId.HasValue)

            {

                if (offerId.HasValue)

                {

                    var d = db.TSql_Design_V2.AsNoTracking().FirstOrDefault(x =>

                        x.SysObjectID == designId.Value && x.LinkOffers == offerId.Value && !x.AttIsDeleted);

                    if (d != null && !string.IsNullOrWhiteSpace(d.AttLabel))

                    {

                        designPart = d.AttLabel.Trim() + " (#" +

                                     designId.Value.ToString(CultureInfo.InvariantCulture) + ")";

                    }

                }



                if (designPart == null)

                {

                    designPart = "#" + designId.Value.ToString(CultureInfo.InvariantCulture);

                }

            }



            var parts = new List<string>();

            if (!string.IsNullOrWhiteSpace(jobsideCode))

            {

                parts.Add(jobsideCode.Trim());

            }



            if (!string.IsNullOrWhiteSpace(offerNumber))

            {

                parts.Add(offerNumber.Trim());

            }



            if (!string.IsNullOrWhiteSpace(offerName))

            {

                parts.Add(offerName.Trim());

            }



            if (!string.IsNullOrWhiteSpace(designPart))

            {

                parts.Add(designPart.Trim());

            }



            model.ContextSubtitleLine = parts.Count > 0 ? string.Join(" — ", parts) : null;

        }

    }

}

