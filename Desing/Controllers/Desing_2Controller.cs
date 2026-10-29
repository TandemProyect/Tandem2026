using DAL;
using Desing.Helpers;

using Desing.Models;
using Desing.Repositories.RepositoryAtk60;
using Desing.Repositories.RepositoryCommun;
using Desing.Repositories.RepositoryDesing2;
using Desing.Resources;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
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

                InitialViewerStateJson = LoadInitialViewerStateJson(designId),

                PropSpacingMm = ReadDesignPropSpacingMm(designId),

                BrandLogoUrl = ResolvePlantillaLogoUrl(Url, (ViewBag.PlantillaMark as string) ?? (ViewBag.PlantillaLogo as string))

            };



            FillOfferDesignContext(model, offerId, designId);



            ViewBag.BodyHtmlClass = "desing2-stl-fullpage";
            ViewBag.SkipIntranetDataTables = true;



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
        /// Persiste los muros del visor Desing_2 en dbo.TSql_DesignWall (borrado lógico + altas).
        /// </summary>
        [HttpPost]
        public JsonResult SaveDesignWalls()
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

                var request = JsonConvert.DeserializeObject<Desing2DesignWallSaveRequest>(rawJson);
                if (request == null || request.DesignId <= 0)
                {
                    return Json(new { Exito = false, Mensaje = "Diseño no indicado." });
                }

                db.Database.CommandTimeout = 15;
                var repository = new DesignWallRepository(db);
                var design = repository.FindActiveDesign(request.DesignId);
                if (design == null)
                {
                    return Json(new { Exito = false, Mensaje = "Diseño no encontrado." });
                }

                var userId = IntranetAuditHelper.ResolveCurrentUserId(User);
                int saved = 0;
                int articleCount = 0;
                DesignWallArticleRepository articleRepo = null;
                using (var trans = db.Database.BeginTransaction())
                {
                    if (request.ArticlesOnly)
                    {
                        articleRepo = new DesignWallArticleRepository(db);
                        articleCount = articleRepo.ReplaceAll(request.DesignId, request.Articles, userId);
                        db.SaveChanges();
                    }
                    else
                    {
                        saved = repository.ReplaceWalls(request.DesignId, request.Lines, userId);
                        if (request.Workers != null)
                        {
                            new DesignWorkerRepository(db).Replace(request.DesignId, request.Workers, userId);
                        }
                        db.SaveChanges();
                    }
                    trans.Commit();
                }

                return Json(new
                {
                    Exito = true,
                    Count = saved,
                    ArticleCount = articleCount,
                    DesignId = request.DesignId
                });
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
        /// Sustituye los muros del diseño abierto por los de un archivo de intercambio,
        /// y los artículos de los muros especiales. No toca el encofrado generado.
        /// </summary>
        [HttpPost]
        public JsonResult ImportWallExchange()
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

                var request = JsonConvert.DeserializeObject<Desing2WallExchangeRequest>(rawJson);
                if (request == null || request.DesignId <= 0)
                {
                    return Json(new { Exito = false, Mensaje = "Diseño no indicado." });
                }

                var trustIds = request.SourceDesignId > 0 && request.SourceDesignId == request.DesignId;
                if (!trustIds)
                {
                    if (request.Walls != null)
                    {
                        for (var i = 0; i < request.Walls.Count; i++)
                        {
                            if (request.Walls[i] != null)
                            {
                                request.Walls[i].WallDbId = null;
                            }
                        }
                    }
                    if (request.SpecialArticles != null)
                    {
                        for (var i = 0; i < request.SpecialArticles.Count; i++)
                        {
                            if (request.SpecialArticles[i] != null)
                            {
                                request.SpecialArticles[i].WallDbId = null;
                            }
                        }
                    }
                }

                db.Database.CommandTimeout = 30;
                var repository = new DesignWallRepository(db);
                var design = repository.FindActiveDesign(request.DesignId);
                if (design == null)
                {
                    return Json(new { Exito = false, Mensaje = "Diseño no encontrado." });
                }

                var userId = IntranetAuditHelper.ResolveCurrentUserId(User);
                int saved;
                int articleCount;
                using (var trans = db.Database.BeginTransaction())
                {
                    saved = repository.ReplaceWalls(request.DesignId, request.Walls, userId, true);
                    db.SaveChanges();
                    articleCount = new DesignWallArticleRepository(db)
                        .ReplaceArticlesOnSpecialWalls(request.DesignId, request.SpecialArticles, userId, trustIds);
                    db.SaveChanges();
                    trans.Commit();
                }

                return Json(new
                {
                    Exito = true,
                    Count = saved,
                    ArticleCount = articleCount,
                    DesignId = request.DesignId
                });
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
        /// El primer usuario del día arranca la limpieza y recibe los segundos previstos.
        /// </summary>
        [HttpPost]
        public JsonResult BeginArticleMaintenance()
        {
            return Json(DesignWallArticleRepository.TryBeginDailyPurge());
        }

        [HttpGet]
        public JsonResult ArticleMaintenanceState()
        {
            return Json(new
            {
                running = DesignWallArticleRepository.MaintenanceSecondsLeft() > 0 && DesignWallArticleRepository.IsMaintenanceNoticeDue(),
                seconds = DesignWallArticleRepository.MaintenanceSecondsLeft()
            }, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// Guarda la foto del visor en Content/images/desing/yyyy-MM y la ruta en AttPhoto.
        /// Si el fichero ya existe, se sustituye.
        /// </summary>
        [HttpPost]
        public JsonResult SaveDesignPhoto()
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

                var body = JObject.Parse(rawJson);
                var designId = body.Value<long?>("DesignId") ?? 0;
                var pngBase64 = body.Value<string>("PngBase64") ?? "";
                if (designId <= 0)
                {
                    return Json(new { Exito = false, Mensaje = "Diseño no indicado." });
                }

                var design = db.TSql_Design_V2.AsNoTracking()
                    .Where(d => d.SysObjectID == designId && !d.AttIsDeleted)
                    .Select(d => new { d.SysObjectID, d.AttLabel })
                    .FirstOrDefault();
                if (design == null)
                {
                    return Json(new { Exito = false, Mensaje = "Diseño no encontrado." });
                }

                byte[] png;
                try
                {
                    png = DecodeDesignPhotoPng(pngBase64);
                }
                catch (Exception ex)
                {
                    return Json(new { Exito = false, Mensaje = ex.Message });
                }

                var folder = DateTime.Now.ToString("yyyy-MM", CultureInfo.InvariantCulture);
                var fileName = SafeDesignPhotoFileName(design.AttLabel, design.SysObjectID);
                var virtualPath = "~/Content/images/desing/" + folder + "/" + fileName;
                var dir = Server.MapPath("~/Content/images/desing/" + folder);
                Directory.CreateDirectory(dir);
                var physical = Path.Combine(dir, fileName);
                System.IO.File.WriteAllBytes(physical, png);
                var photoVersion = DateTime.UtcNow.Ticks.ToString(CultureInfo.InvariantCulture);
                var photoUrl = Url.Content(virtualPath) + "?v=" + photoVersion;

                var userId = IntranetAuditHelper.ResolveCurrentUserId(User) ?? "";
                var updated = db.Database.ExecuteSqlCommand(
                    "UPDATE dbo.TSql_Design_V2 SET AttPhoto = @p0, AttChange = @p1, LinModifiedBy = @p2, SysUpdateNumber = SysUpdateNumber + 1 WHERE SysObjectID = @p3 AND AttIsDeleted = 0",
                    virtualPath,
                    DateTime.Now,
                    userId,
                    design.SysObjectID);
                if (updated != 1)
                {
                    return Json(new { Exito = false, Mensaje = "No se pudo guardar la ruta de la foto." });
                }

                return Json(new
                {
                    Exito = true,
                    DesignId = design.SysObjectID,
                    AttPhoto = virtualPath,
                    PhotoUrl = photoUrl,
                    Version = photoVersion,
                    FileName = fileName
                });
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

        [HttpPost]
        public JsonResult SaveViewerState()
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

                var request = JsonConvert.DeserializeObject<Desing2ViewerStateSaveRequest>(rawJson);
                if (request == null || request.DesignId <= 0)
                {
                    return Json(new { Exito = false, Mensaje = "Diseño no indicado." });
                }

                var state = NormalizeViewerState(request.State);
                if (state == null)
                {
                    return Json(new { Exito = false, Mensaje = "Vista no válida." });
                }

                var stateJson = state.ToString(Formatting.None);
                if (Encoding.UTF8.GetByteCount(stateJson) > 32 * 1024)
                {
                    return Json(new { Exito = false, Mensaje = "La vista es demasiado grande." });
                }

                var activeType = ResolveViewerStateActiveCameraType(state);
                var userId = IntranetAuditHelper.ResolveCurrentUserId(User) ?? "";
                var updated = db.Database.ExecuteSqlCommand(
                    "UPDATE dbo.TSql_Design_V2 SET AttActiveCameraType = @p0, AttChange = @p1, LinModifiedBy = @p2, SysUpdateNumber = SysUpdateNumber + 1 WHERE SysObjectID = @p3 AND AttIsDeleted = 0",
                    activeType,
                    DateTime.Now,
                    userId,
                    request.DesignId);
                if (updated != 1)
                {
                    return Json(new { Exito = false, Mensaje = "Diseño no encontrado." });
                }

                var serverStateSaved = TrySaveOptionalViewerStateJson(request.DesignId, stateJson);
                var legacyCameraSaved = TrySaveLegacyDesignCameras(request.DesignId, userId, activeType, state);

                return Json(new
                {
                    Exito = true,
                    DesignId = request.DesignId,
                    ActiveCameraType = activeType,
                    ServerStateSaved = serverStateSaved,
                    LegacyCameraSaved = legacyCameraSaved
                });
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

        [HttpGet]
        public JsonResult LoadPhasePlan(long designId)
        {
            try
            {
                if (designId <= 0)
                {
                    return Json(new { Exito = false, Mensaje = "Diseño no indicado." }, JsonRequestBehavior.AllowGet);
                }

                var raw = TryLoadPhasePlanJson(designId);
                var walls = TryLoadPhaseDetails(designId);
                return Json(new { Exito = true, DesignId = designId, PlanJson = raw ?? "", Walls = walls }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { Exito = false, Mensaje = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpPost]
        public JsonResult SavePhasePlan()
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

                var request = JsonConvert.DeserializeObject<Desing2PhasePlanSaveRequest>(rawJson);
                if (request == null || request.DesignId <= 0)
                {
                    return Json(new { Exito = false, Mensaje = "Diseño no indicado." });
                }

                if (request.Plan == null || request.Plan.Type != JTokenType.Object)
                {
                    return Json(new { Exito = false, Mensaje = "El plan de fases no es válido." });
                }

                var planJson = request.Plan.ToString(Formatting.None);
                if (Encoding.UTF8.GetByteCount(planJson) > 512 * 1024)
                {
                    return Json(new { Exito = false, Mensaje = "El plan de fases es demasiado grande." });
                }

                var repository = new DesignWallRepository(db);
                if (repository.FindActiveDesign(request.DesignId) == null)
                {
                    return Json(new { Exito = false, Mensaje = "Diseño no encontrado." });
                }

                var userId = IntranetAuditHelper.ResolveCurrentUserId(User) ?? "";
                int detailCount;
                string detailError;
                if (!TryReplacePhaseDetails(request.DesignId, userId, request.Plan, out detailCount, out detailError))
                {
                    return Json(new { Exito = false, Mensaje = detailError ?? "No se han podido guardar las fases." });
                }

                TrySavePhasePlanJson(request.DesignId, userId, planJson);

                return Json(new { Exito = true, DesignId = request.DesignId, Count = detailCount });
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

        private static byte[] DecodeDesignPhotoPng(string raw)
        {
            if (string.IsNullOrWhiteSpace(raw))
            {
                throw new InvalidOperationException("Foto vacía.");
            }

            var comma = raw.IndexOf(',');
            if (raw.StartsWith("data:", StringComparison.OrdinalIgnoreCase) && comma >= 0)
            {
                raw = raw.Substring(comma + 1);
            }

            byte[] png;
            try
            {
                png = Convert.FromBase64String(raw.Trim());
            }
            catch (FormatException)
            {
                throw new InvalidOperationException("La foto no es una imagen válida.");
            }

            if (png.Length < 8 || png.Length > 12 * 1024 * 1024)
            {
                throw new InvalidOperationException("La foto no tiene un tamaño válido.");
            }

            if (png[0] != 0x89 || png[1] != 0x50 || png[2] != 0x4E || png[3] != 0x47)
            {
                throw new InvalidOperationException("La foto no es un PNG.");
            }

            return png;
        }

        private static string SafeDesignPhotoFileName(string label, long designId)
        {
            var name = (label ?? "").Trim();
            foreach (var c in Path.GetInvalidFileNameChars())
            {
                name = name.Replace(c, ' ');
            }

            name = Regex.Replace(name, @"\s+", " ").Trim().Trim('.');
            if (name.EndsWith(".png", StringComparison.OrdinalIgnoreCase))
            {
                name = name.Substring(0, name.Length - 4).Trim().Trim('.');
            }

            if (string.IsNullOrEmpty(name))
            {
                name = "diseno-" + designId.ToString(CultureInfo.InvariantCulture);
            }

            if (name.Length > 120)
            {
                name = name.Substring(0, 120).Trim().Trim('.');
            }

            return name + ".png";
        }

        /// <summary>
        /// Recuento de artículos del diseño: una lectura de las piezas y una del maestro.
        /// </summary>
        [HttpGet]
        public JsonResult ListDesignMaterialCount(long designId)
        {
            try
            {
                if (designId <= 0)
                {
                    return Json(new { ok = true, rows = new object[0] }, JsonRequestBehavior.AllowGet);
                }

                var pieces = new DesignWallArticleRepository(db).QueryLatestActive(designId)
                    .Select(a => new { a.TextCode, a.LinkMasterArticle })
                    .ToList();

                var groupedMap = new Dictionary<string, MaterialCountGroup>(StringComparer.OrdinalIgnoreCase);
                for (var i = 0; i < pieces.Count; i++)
                {
                    AddSceneMaterialCountGroup(groupedMap, pieces[i].TextCode, 1, pieces[i].LinkMasterArticle, null);
                }
                var grouped = ResolveReplacementMaterialGroups(groupedMap.Values.ToList());

                var codes = grouped.Select(g => g.Code).ToList();
                var ids = grouped.Where(g => g.MasterId.HasValue).Select(g => g.MasterId.Value).Distinct().ToList();
                var masterQuery = db.Tsql_Master_Articles.AsNoTracking().Where(m => codes.Contains(m.TextCode) || codes.Contains(m.AddAtenkoCode));
                if (ids.Count > 0)
                {
                    masterQuery = db.Tsql_Master_Articles.AsNoTracking()
                        .Where(m => ids.Contains(m.IdObject) || codes.Contains(m.TextCode) || codes.Contains(m.AddAtenkoCode));
                }
                var masters = codes.Count == 0
                    ? new List<MasterArticleCountRow>()
                    : masterQuery.Select(m => new MasterArticleCountRow
                    {
                        IdObject = m.IdObject,
                        TextCode = m.TextCode,
                        AddAtenkoCode = m.AddAtenkoCode,
                        TextLabel = m.TextLabel,
                        NumberWeight = m.NumberWeight,
                        NumberMts2 = m.NumberMts2
                    }).ToList();

                var rows = grouped
                    .OrderBy(g => g.Code)
                    .Select(g =>
                    {
                        var master = masters.FirstOrDefault(m => g.MasterId.HasValue && m.IdObject == g.MasterId.Value)
                            ?? masters.FirstOrDefault(m => string.Equals(m.TextCode, g.Code, StringComparison.OrdinalIgnoreCase))
                            ?? masters.FirstOrDefault(m => string.Equals(m.AddAtenkoCode, g.Code, StringComparison.OrdinalIgnoreCase));
                        var code = master == null
                            ? g.Code
                            : (!string.IsNullOrWhiteSpace(master.AddAtenkoCode) ? master.AddAtenkoCode : (master.TextCode ?? g.Code));
                        var name = master == null ? "" : (master.TextLabel ?? "");
                        var unitWeight = master == null ? 0d : (master.NumberWeight ?? 0d);
                        var unitMts2 = master == null ? 0d : (master.NumberMts2 ?? 0d);
                        return new
                        {
                            code = code,
                            name = name,
                            qty = g.Qty,
                            unitWeight = unitWeight,
                            totalWeight = unitWeight * g.Qty,
                            unitMts2 = unitMts2,
                            totalMts2 = unitMts2 * g.Qty
                        };
                    })
                    .ToList();

                var result = Json(new { ok = true, rows = rows }, JsonRequestBehavior.AllowGet);
                result.MaxJsonLength = int.MaxValue;
                return result;
            }
            catch (Exception ex)
            {
                return Json(new { ok = false, message = ex.Message, rows = new object[0] }, JsonRequestBehavior.AllowGet);
            }
        }

        /// <summary>
        /// Pone el nombre del maestro a las piezas que ya están encofradas en el visor. No escribe en la base.
        /// </summary>
        [HttpPost]
        public JsonResult PreviewMaterialCount()
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

                var groupedMap = new Dictionary<string, MaterialCountGroup>(StringComparer.OrdinalIgnoreCase);
                if (!string.IsNullOrWhiteSpace(rawJson))
                {
                    var parsed = JToken.Parse(rawJson);
                    var items = parsed["items"] as JArray;
                    if (items != null)
                    {
                        foreach (var item in items)
                        {
                            var qty = 1;
                            var qtyToken = item["qty"];
                            if (qtyToken != null && qtyToken.Type != JTokenType.Null)
                            {
                                int parsedQty;
                                if (int.TryParse(qtyToken.ToString(), NumberStyles.Integer, CultureInfo.InvariantCulture, out parsedQty) && parsedQty > 0)
                                {
                                    qty = parsedQty;
                                }
                            }
                            AddSceneMaterialCountGroup(groupedMap, item.Value<string>("code"), qty, null, item.Value<string>("sceneCode"));
                        }
                    }
                }

                var grouped = ResolveReplacementMaterialGroups(groupedMap.Values.ToList());
                var codes = grouped.Select(g => g.Code).ToList();
                var ids = grouped.Where(g => g.MasterId.HasValue).Select(g => g.MasterId.Value).Distinct().ToList();
                var masters = codes.Count == 0
                    ? new List<MasterArticleCountRow>()
                    : db.Tsql_Master_Articles.AsNoTracking()
                        .Where(m => ids.Contains(m.IdObject) || codes.Contains(m.TextCode) || codes.Contains(m.AddAtenkoCode))
                        .Select(m => new MasterArticleCountRow
                        {
                            IdObject = m.IdObject,
                            TextCode = m.TextCode,
                            AddAtenkoCode = m.AddAtenkoCode,
                            TextLabel = m.TextLabel,
                            TextColor1 = m.TextColor1,
                            TextColor2 = m.TextColor2,
                            NumberWeight = m.NumberWeight,
                            NumberMts2 = m.NumberMts2,
                            NumberHigh = m.NumberHigh,
                            NumberWidth = m.NumberWidth,
                            LinkBlockDwgPlantStl = m.LinkBlockDwgPlantStl,
                            LinkBlockDwgVerticalElevationStl = m.LinkBlockDwgVerticalElevationStl,
                            LinkBlockDwgHorizontalElevationStl = m.LinkBlockDwgHorizontalElevationStl,
                            LinkBlockDwg3D = m.LinkBlockDwg3D,
                            LinkBlockDwgPlant3D = m.LinkBlockDwgPlant3D
                        })
                        .ToList();

                var rows = grouped
                    .OrderBy(g => g.Code)
                    .Select(g =>
                    {
                        var master = masters.FirstOrDefault(m => g.MasterId.HasValue && m.IdObject == g.MasterId.Value)
                            ?? masters.FirstOrDefault(m => string.Equals(m.TextCode, g.Code, StringComparison.OrdinalIgnoreCase))
                            ?? masters.FirstOrDefault(m => string.Equals(m.AddAtenkoCode, g.Code, StringComparison.OrdinalIgnoreCase));
                        var code = master == null
                            ? g.Code
                            : (!string.IsNullOrWhiteSpace(master.AddAtenkoCode) ? master.AddAtenkoCode : (master.TextCode ?? g.Code));
                        var name = master == null ? "" : (master.TextLabel ?? "");
                        var stl = ResolveCountArticleStl(master, g.Code);
                        var unitWeight = master == null ? 0d : (master.NumberWeight ?? 0d);
                        var unitMts2 = master == null ? 0d : (master.NumberMts2 ?? 0d);
                        return new
                        {
                            code = code,
                            sceneCode = !string.IsNullOrWhiteSpace(g.SceneCode) ? g.SceneCode : g.Code,
                            name = name,
                            qty = g.Qty,
                            unitWeight = unitWeight,
                            totalWeight = unitWeight * g.Qty,
                            unitMts2 = unitMts2,
                            totalMts2 = unitMts2 * g.Qty,
                            stlUrl = stl.StlUrl,
                            stlUrl2 = stl.StlUrl2,
                            color1 = stl.Color1,
                            color2 = stl.Color2
                        };
                    })
                    .ToList();

                return Json(new { ok = true, rows = rows });
            }
            catch (Exception ex)
            {
                return Json(new { ok = false, message = ex.Message, rows = new object[0] });
            }
        }

        /// <summary>
        /// Dice si este diseño ya tiene piezas guardadas y si los muros son posteriores.
        /// </summary>
        [HttpGet]
        public JsonResult MaterialCountState(long designId)
        {
            try
            {
                if (designId <= 0)
                {
                    return Json(new { ok = true, pieceCount = 0, stale = false }, JsonRequestBehavior.AllowGet);
                }

                var pieceDates = new DesignWallArticleRepository(db).QueryLatestActive(designId)
                    .Select(a => a.AddDateMade)
                    .ToList();
                var wallDates = db.TSql_DesignWall.AsNoTracking()
                    .Where(w => w.LinkDesign_V2 == designId && !w.Is_Delete)
                    .Select(w => new { w.AddDateMade, w.AddLastDateChange })
                    .ToList();

                DateTime? pieceAt = pieceDates.Count == 0 ? (DateTime?)null : pieceDates.Max();
                DateTime? wallAt = null;
                for (var i = 0; i < wallDates.Count; i++)
                {
                    var stamp = wallDates[i].AddLastDateChange ?? wallDates[i].AddDateMade;
                    if (!wallAt.HasValue || stamp > wallAt.Value)
                    {
                        wallAt = stamp;
                    }
                }

                var stale = pieceAt.HasValue && wallAt.HasValue && wallAt.Value > pieceAt.Value.AddSeconds(2);
                return Json(new { ok = true, pieceCount = pieceDates.Count, stale = stale }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { ok = false, message = ex.Message, pieceCount = 0, stale = true }, JsonRequestBehavior.AllowGet);
            }
        }

        /// <summary>
        /// Borrado lógico de las piezas guardadas de un diseño, antes de rehacer el recuento.
        /// </summary>
        [HttpPost]
        public JsonResult ClearDesignMaterialCount(long designId)
        {
            try
            {
                if (designId <= 0)
                {
                    return Json(new { ok = false, message = "Diseño no indicado." });
                }

                var userId = IntranetAuditHelper.ResolveCurrentUserId(User);
                var deleted = new DesignWallArticleRepository(db).SoftDeleteByDesign(designId, userId);
                return Json(new { ok = true, deleted = deleted });
            }
            catch (Exception ex)
            {
                return Json(new { ok = false, message = ex.Message });
            }
        }

        private static string NormalizeMaterialCode(string raw)
        {
            var code = (raw ?? "").Trim();
            if (code.StartsWith("Atk60_", StringComparison.OrdinalIgnoreCase) && code.Length > 6)
            {
                code = code.Substring(6).Trim();
            }
            return code;
        }

        private static string NormalizeMaterialCountCode(string raw)
        {
            var code = NormalizeMaterialCode(raw);
            if (code.Length == 0 || code.EndsWith("_PHENOLIC", StringComparison.OrdinalIgnoreCase))
            {
                return "";
            }

            if (code.EndsWith("_FRAME", StringComparison.OrdinalIgnoreCase))
            {
                code = code.Substring(0, code.Length - 6).Trim();
            }

            return string.Equals(code, "10000221B", StringComparison.OrdinalIgnoreCase) ? "" : code;
        }

        private static void AddMaterialCountGroup(IDictionary<string, MaterialCountGroup> groups, string rawCode, int qty, long? masterId, string sceneCode)
        {
            if (groups == null || qty <= 0)
            {
                return;
            }

            var code = NormalizeMaterialCountCode(rawCode);
            if (code.Length == 0)
            {
                return;
            }

            MaterialCountGroup group;
            if (!groups.TryGetValue(code, out group))
            {
                var scene = NormalizeMaterialCode(sceneCode);
                group = new MaterialCountGroup
                {
                    Code = code,
                    SceneCode = string.IsNullOrWhiteSpace(scene) ? code : scene
                };
                groups[code] = group;
            }

            group.Qty += qty;
            if (!group.MasterId.HasValue && masterId.HasValue)
            {
                group.MasterId = masterId;
            }
        }

        private static void AddSceneMaterialCountGroup(IDictionary<string, MaterialCountGroup> groups, string rawCode, int qty, long? masterId, string sceneCode)
        {
            var code = NormalizeMaterialCountCode(rawCode);
            if (string.IsNullOrWhiteSpace(code) || qty <= 0)
            {
                return;
            }

            if (IsLegacyPropHiddenPart(code))
            {
                return;
            }

            var propMaterials = LegacyPropMaterialCodes(code);
            if (propMaterials != null)
            {
                for (var i = 0; i < propMaterials.Length; i++)
                {
                    AddMaterialCountGroup(groups, propMaterials[i], qty, null, code);
                }
                return;
            }

            AddMaterialCountGroup(groups, code, qty, masterId, sceneCode);
        }

        private static bool IsLegacyPropHiddenPart(string code)
        {
            return !string.IsNullOrWhiteSpace(code)
                && code.StartsWith("H0_2", StringComparison.OrdinalIgnoreCase);
        }

        private static string[] LegacyPropMaterialCodes(string code)
        {
            if (string.IsNullOrWhiteSpace(code))
            {
                return null;
            }

            if (code.StartsWith("HTipo4-41", StringComparison.OrdinalIgnoreCase))
            {
                return new[] { "551", "551", "738", "738", "738", "550", "550" };
            }

            if (code.StartsWith("HTipo3-2", StringComparison.OrdinalIgnoreCase))
            {
                return new[] { "551", "551", "737", "737", "737", "550", "550" };
            }

            if (code.StartsWith("HTipo3", StringComparison.OrdinalIgnoreCase))
            {
                return new[] { "551", "551", "736", "736", "736", "550", "550" };
            }

            if (code.StartsWith("HTipo2", StringComparison.OrdinalIgnoreCase))
            {
                return new[] { "1920804", "1900144", "546" };
            }

            if (code.StartsWith("HTipo1", StringComparison.OrdinalIgnoreCase))
            {
                return new[] { "1920804", "1900144", "1900135" };
            }

            if (code.StartsWith("HTipo0", StringComparison.OrdinalIgnoreCase))
            {
                return new[] { "1920804", "1900144", "1900134" };
            }

            return null;
        }

        private List<MaterialCountGroup> ResolveReplacementMaterialGroups(List<MaterialCountGroup> groups)
        {
            if (groups == null || groups.Count == 0)
            {
                return new List<MaterialCountGroup>();
            }

            var codes = groups.Select(g => g.Code).Where(c => !string.IsNullOrWhiteSpace(c)).Distinct(StringComparer.OrdinalIgnoreCase).ToList();
            var directIds = groups.Where(g => g.MasterId.HasValue).Select(g => g.MasterId.Value).Distinct().ToList();
            if (codes.Count == 0 && directIds.Count == 0)
            {
                return groups;
            }

            var masters = db.Tsql_Master_Articles.AsNoTracking()
                .Where(m => directIds.Contains(m.IdObject) || codes.Contains(m.TextCode) || codes.Contains(m.AddAtenkoCode))
                .Select(m => new MasterArticleCountRow
                {
                    IdObject = m.IdObject,
                    TextCode = m.TextCode,
                    AddAtenkoCode = m.AddAtenkoCode,
                    TextLabel = m.TextLabel
                })
                .ToList();

            var parentIds = new List<long>();
            for (var i = 0; i < groups.Count; i++)
            {
                var master = FindCountMaster(masters, groups[i]);
                if (master == null)
                {
                    continue;
                }

                if (!groups[i].MasterId.HasValue)
                {
                    groups[i].MasterId = master.IdObject;
                }

                if (!parentIds.Contains(master.IdObject))
                {
                    parentIds.Add(master.IdObject);
                }
            }

            if (parentIds.Count == 0)
            {
                return groups;
            }

            var replacements = db.Tsql_RemplaceArticleStok.AsNoTracking()
                .Where(r => parentIds.Contains(r.LinkMaster_Articles))
                .Select(r => new { r.IdObject, r.LinkMaster_Articles })
                .ToList();
            if (replacements.Count == 0)
            {
                return groups;
            }

            var replacementIds = replacements.Select(r => r.IdObject).Distinct().ToList();
            var childLinks = db.Tsql_RemplaceArticleStokChildren.AsNoTracking()
                .Where(c => replacementIds.Contains(c.LinkRemplaceArticleStok))
                .Select(c => new { c.LinkRemplaceArticleStok, c.LinkMaster_Articles })
                .ToList();
            if (childLinks.Count == 0)
            {
                return groups;
            }

            var childIds = childLinks.Select(c => c.LinkMaster_Articles).Distinct().ToList();
            var childMasters = db.Tsql_Master_Articles.AsNoTracking()
                .Where(m => childIds.Contains(m.IdObject))
                .Select(m => new MasterArticleCountRow
                {
                    IdObject = m.IdObject,
                    TextCode = m.TextCode,
                    AddAtenkoCode = m.AddAtenkoCode,
                    TextLabel = m.TextLabel
                })
                .ToList()
                .ToDictionary(m => m.IdObject);

            var output = new Dictionary<string, MaterialCountGroup>(StringComparer.OrdinalIgnoreCase);
            for (var i = 0; i < groups.Count; i++)
            {
                var group = groups[i];
                var master = FindCountMaster(masters, group);
                var expanded = false;
                if (master != null)
                {
                    var groupReplacements = replacements.Where(r => r.LinkMaster_Articles == master.IdObject).ToList();
                    for (var r = 0; r < groupReplacements.Count; r++)
                    {
                        var replacementId = groupReplacements[r].IdObject;
                        var children = childLinks.Where(c => c.LinkRemplaceArticleStok == replacementId).ToList();
                        for (var c = 0; c < children.Count; c++)
                        {
                            MasterArticleCountRow childMaster;
                            if (!childMasters.TryGetValue(children[c].LinkMaster_Articles, out childMaster))
                            {
                                continue;
                            }

                            var childCode = CountCodeForMaster(childMaster, group.Code);
                            AddMaterialCountGroup(output, childCode, group.Qty, childMaster.IdObject, group.SceneCode);
                            expanded = true;
                        }
                    }
                }

                if (!expanded)
                {
                    AddMaterialCountGroup(output, group.Code, group.Qty, group.MasterId, group.SceneCode);
                }
            }

            return output.Values.ToList();
        }

        private static MasterArticleCountRow FindCountMaster(List<MasterArticleCountRow> masters, MaterialCountGroup group)
        {
            if (masters == null || group == null)
            {
                return null;
            }

            if (group.MasterId.HasValue)
            {
                var byId = masters.FirstOrDefault(m => m.IdObject == group.MasterId.Value);
                if (byId != null)
                {
                    return byId;
                }
            }

            return masters.FirstOrDefault(m => string.Equals(m.TextCode, group.Code, StringComparison.OrdinalIgnoreCase))
                ?? masters.FirstOrDefault(m => string.Equals(m.AddAtenkoCode, group.Code, StringComparison.OrdinalIgnoreCase));
        }

        private static string CountCodeForMaster(MasterArticleCountRow master, string fallback)
        {
            if (master == null)
            {
                return fallback;
            }

            return !string.IsNullOrWhiteSpace(master.AddAtenkoCode)
                ? master.AddAtenkoCode
                : (master.TextCode ?? fallback);
        }

        /// <summary>
        /// Devuelve el snapshot de muros activos de un diseño V2 para restaurar el visor.
        /// </summary>
        [HttpGet]
        public JsonResult GetDesignWalls(long designId)
        {
            try
            {
                if (designId <= 0)
                {
                    return Json(new { Exito = false, Mensaje = "Diseño no indicado." }, JsonRequestBehavior.AllowGet);
                }

                var repository = new DesignWallRepository(db);
                var design = repository.FindActiveDesign(designId);
                if (design == null)
                {
                    return Json(new { Exito = false, Mensaje = "Diseño no encontrado." }, JsonRequestBehavior.AllowGet);
                }

                var snapshot = repository.LoadSnapshot(designId);
                var result = Json(new
                {
                    Exito = true,
                    DesignId = designId,
                    Snapshot = snapshot
                }, JsonRequestBehavior.AllowGet);
                result.MaxJsonLength = int.MaxValue;
                return result;
            }
            catch (Exception ex)
            {
                return Json(new { Exito = false, Mensaje = ex.Message }, JsonRequestBehavior.AllowGet);
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
                const string buildStamp = "ATK60-BACKEND-2026-07-17-FACESIGN-POSE";
                var solved = new Atk60WallsRepository(new FormworkJsonCommonRepository())
                    .SolveFromIdsJson(jsonRaw);
                var walls = solved.Walls;
                var modulos = solved.Modulos;
                var elementsForThreeJs = solved.ElementsForThreeJs;
                try
                {
                    SaveAtk60RequestDebugToTemp(jsonRaw, walls, modulos, elementsForThreeJs, buildStamp);
                }
                catch
                {
                    // El servidor publicado no puede escribir C:\temp. Eso no debe anular el encofrado.
                }

                var result = Json(new
                {
                    Exito = true,
                    System = solved.System,
                    WallsCount = walls != null ? walls.Count : 0,
                    ListCount = walls != null ? walls.Count : 0,
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
                result.MaxJsonLength = int.MaxValue;
                return result;
            }
            catch (Exception ex)
            {
                return Json(new { Exito = false, Mensaje = ex.Message });
            }
        }

        private static string SaveAtk60RequestDebugToTemp(
            string jsonRaw,
            List<Desing2FormworkWallDto> walls,
            List<ModulosAtk60Wall> modulos,
            Atk60ThreeJsPaintPayload elementsForThreeJs,
            string buildStamp)
        {
            var dir = @"C:\temp";
            if (!Directory.Exists(dir))
            {
                Directory.CreateDirectory(dir);
            }

            var walls3d = BuildAtk60Walls3dDebug(walls);
            var corners3d = BuildAtk60CornersDebug(walls);
            var panels3d = BuildAtk60PanelsDebug(elementsForThreeJs);
            var debug = new
            {
                GeneratedAtUtc = DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ss.fffZ", CultureInfo.InvariantCulture),
                Source = "Desing_2Controller.GetWallsAtk60",
                BuildStamp = buildStamp,
                RawIdsJsonLength = string.IsNullOrWhiteSpace(jsonRaw) ? 0 : jsonRaw.Length,
                WallsCount = walls != null ? walls.Count : 0,
                ModulosCount = modulos != null ? modulos.Count : 0,
                PanelsCount = panels3d != null ? panels3d.Count : 0,
                Walls = walls ?? new List<Desing2FormworkWallDto>(),
                Modulos = modulos ?? new List<ModulosAtk60Wall>(),
                Walls3D = walls3d,
                Corners3D = corners3d,
                ElementsForThreeJs = elementsForThreeJs ?? new Atk60ThreeJsPaintPayload(),
                Panels3D = panels3d,
            };

            var targetPath = Path.Combine(dir, "Atk60RequestWallsDebug.json");
            var formatted = JsonConvert.SerializeObject(debug, Formatting.Indented);
            System.IO.File.WriteAllText(targetPath, formatted, new UTF8Encoding(false));

            var panelsOnlyPath = Path.Combine(dir, "Atk60PanelsDebug.json");
            var panelsOnly = new
            {
                GeneratedAtUtc = DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ss.fffZ", CultureInfo.InvariantCulture),
                Source = "Desing_2Controller.GetWallsAtk60",
                BuildStamp = buildStamp,
                PanelsCount = panels3d != null ? panels3d.Count : 0,
                Panels3D = panels3d,
            };
            var panelsFormatted = JsonConvert.SerializeObject(panelsOnly, Formatting.Indented);
            System.IO.File.WriteAllText(panelsOnlyPath, panelsFormatted, new UTF8Encoding(false));

            return targetPath;
        }

        private static List<object> BuildAtk60PanelsDebug(Atk60ThreeJsPaintPayload elementsForThreeJs)
        {
            var result = new List<object>();
            if (elementsForThreeJs == null || elementsForThreeJs.Elements == null || elementsForThreeJs.Elements.Count == 0)
            {
                return result;
            }

            for (var i = 0; i < elementsForThreeJs.Elements.Count; i++)
            {
                var item = elementsForThreeJs.Elements[i];
                if (item == null)
                {
                    continue;
                }

                result.Add(new
                {
                    Index = i + 1,
                    item.IdWall,
                    item.ElementType,
                    item.ElementCode,
                    item.Orientation,
                    item.ImportPath,
                    item.ModuleIndex,
                    item.ModuleCountInWall,
                    item.PieceIndexInModule,
                    item.PieceCountInModule,
                    item.CatalogHeightMm,
                    item.WallLengthMm,
                    item.WallHeightMm,
                    item.WallThicknessMm,
                    item.PieceWidthMm,
                    item.PieceHeightMm,
                    item.LocalAlongMm,
                    item.LocalUpMm,
                    item.X,
                    item.Y,
                    item.Z,
                    item.RotX,
                    item.RotY,
                    item.RotZ,
                });
            }

            return result;
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

        private sealed class Desing2ViewerStateSaveRequest
        {
            public long DesignId { get; set; }
            public JObject State { get; set; }
        }

        private sealed class Desing2PhasePlanSaveRequest
        {
            public long DesignId { get; set; }
            public JObject Plan { get; set; }
        }

        private sealed class PhaseDetailRow
        {
            public long NumberPhase { get; set; }
            public string TextWallId { get; set; }
        }

        private static string ResolvePlantillaLogoUrl(UrlHelper url, string plantillaLogoRaw)

        {

            var plantillaLogo = string.IsNullOrWhiteSpace(plantillaLogoRaw)

                ? PluginCadDeviceHelper.DefaultMarkVirtualPath

                : plantillaLogoRaw.Trim();

            if (plantillaLogo.StartsWith("http", StringComparison.OrdinalIgnoreCase)

                || plantillaLogo.StartsWith("//", StringComparison.Ordinal))

            {

                return plantillaLogo;

            }



            return url.Content("~" + (plantillaLogo.StartsWith("/") ? plantillaLogo : "/" + plantillaLogo));

        }

        private string LoadInitialViewerStateJson(long? designId)
        {
            if (!designId.HasValue || designId.Value <= 0)
            {
                return null;
            }

            var optionalState = TryLoadOptionalViewerStateJson(designId.Value);
            if (!string.IsNullOrWhiteSpace(optionalState))
            {
                return optionalState;
            }

            return TryBuildViewerStateJsonFromLegacyCameras(designId.Value);
        }

        private string TryLoadOptionalViewerStateJson(long designId)
        {
            try
            {
                var exists = db.Database.SqlQuery<int>(
                    "SELECT CASE WHEN COL_LENGTH('dbo.TSql_Design_V2', 'AttViewerStateJson') IS NULL THEN 0 ELSE 1 END")
                    .FirstOrDefault();
                if (exists != 1)
                {
                    return null;
                }

                var raw = db.Database.SqlQuery<string>(
                    "SELECT CAST(AttViewerStateJson AS nvarchar(max)) FROM dbo.TSql_Design_V2 WHERE SysObjectID = @p0 AND AttIsDeleted = 0",
                    designId)
                    .FirstOrDefault();
                var state = NormalizeViewerState(raw);
                return state == null ? null : state.ToString(Formatting.None);
            }
            catch
            {
                return null;
            }
        }

        private string TryBuildViewerStateJsonFromLegacyCameras(long designId)
        {
            try
            {
                var userId = IntranetAuditHelper.ResolveCurrentUserId(User);
                if (string.IsNullOrWhiteSpace(userId))
                {
                    return null;
                }

                var design = db.TSql_Design_V2.FirstOrDefault(d => d.SysObjectID == designId && !d.AttIsDeleted);
                if (design == null)
                {
                    return null;
                }

                var cameras = db.TSql_DesignCamera
                    .Where(c => c.LinDesaing == designId && c.LinUser == userId && c.AttIsActive)
                    .ToList();
                if (cameras.Count == 0)
                {
                    return null;
                }

                var state = new JObject
                {
                    ["v"] = 1,
                    ["activeCamera"] = design.AttActiveCameraType == 1L ? "iso" : "ortho",
                    ["target"] = BuildViewerStateVector(0d, 0d, 0d)
                };

                foreach (var cam in cameras)
                {
                    var cameraState = new JObject
                    {
                        ["position"] = BuildViewerStateVector(cam.AttPositionX, cam.AttPositionY, cam.AttPositionZ),
                        ["target"] = BuildViewerStateVector(cam.AttTargetX, cam.AttTargetY, cam.AttTargetZ),
                        ["zoom"] = cam.AttZoom
                    };
                    if (cam.AttCameraType == 1L)
                    {
                        state["cameraIso"] = cameraState;
                    }
                    else
                    {
                        state["cameraOrtho"] = cameraState;
                    }
                }

                return state.ToString(Formatting.None);
            }
            catch
            {
                return null;
            }
        }

        private static JArray BuildViewerStateVector(double x, double y, double z)
        {
            return new JArray(x, y, z);
        }

        private bool TryReplacePhaseDetails(long designId, string userId, JObject plan, out int count, out string error)
        {
            count = 0;
            error = null;
            var rows = ReadPhaseWalls(plan);
            if (rows.Count == 0)
            {
                error = "El plan no trae muros por fase.";
                return false;
            }

            try
            {
                var exists = db.Database.SqlQuery<int>(
                    "SELECT CASE WHEN OBJECT_ID(N'dbo.TSql_DesingFasesDetail', N'U') IS NULL THEN 0 ELSE 1 END")
                    .FirstOrDefault();
                if (exists != 1)
                {
                    error = "Falta la tabla TSql_DesingFasesDetail.";
                    return false;
                }

                var now = DateTime.Now;
                var madeBy = string.IsNullOrWhiteSpace(userId) ? "" : userId;
                using (var tx = db.Database.BeginTransaction())
                {
                    db.Database.ExecuteSqlCommand(
                        "UPDATE dbo.TSql_DesingFasesDetail SET Is_Delete = 1, Is_Active = 0, LinModifiedBy = @p0, AddLastDateChange = @p1, Ntimeschanged = Ntimeschanged + 1 WHERE LinkDesign_V2 = @p2 AND Is_Delete = 0",
                        madeBy,
                        now,
                        designId);

                    foreach (var row in rows)
                    {
                        var label = "Fase " + row.NumberPhase.ToString(CultureInfo.InvariantCulture) + " · muro " + row.TextWallId;
                        if (label.Length > 500)
                        {
                            label = label.Substring(0, 500);
                        }

                        db.Database.ExecuteSqlCommand(
                            "INSERT INTO dbo.TSql_DesingFasesDetail (TextLabel, LinkDesign_V2, NumberPhase, TextWallId, Is_Delete, Is_Active, LinkMadeBy, LinModifiedBy, AddDateMade, AddLastDateChange, Ntimeschanged) VALUES (@p0, @p1, @p2, @p3, 0, 1, @p4, NULL, @p5, NULL, 0)",
                            label,
                            designId,
                            row.NumberPhase,
                            row.TextWallId,
                            madeBy,
                            now);
                    }

                    tx.Commit();
                }

                count = rows.Count;
                return true;
            }
            catch (Exception ex)
            {
                error = ex.Message;
                return false;
            }
        }

        private static List<PhaseDetailRow> ReadPhaseWalls(JObject plan)
        {
            var list = new List<PhaseDetailRow>();
            var byWall = plan["byWall"] as JObject ?? plan["ByWall"] as JObject;
            if (byWall == null)
            {
                return list;
            }

            foreach (var prop in byWall.Properties())
            {
                var wallId = (prop.Name ?? "").Trim();
                if (wallId.Length == 0 || wallId.Length > 128)
                {
                    continue;
                }

                long phase;
                if (!long.TryParse(prop.Value == null ? "" : prop.Value.ToString(), NumberStyles.Integer, CultureInfo.InvariantCulture, out phase) || phase < 1)
                {
                    continue;
                }

                list.Add(new PhaseDetailRow { NumberPhase = phase, TextWallId = wallId });
            }

            return list;
        }

        private List<object> TryLoadPhaseDetails(long designId)
        {
            try
            {
                var exists = db.Database.SqlQuery<int>(
                    "SELECT CASE WHEN OBJECT_ID(N'dbo.TSql_DesingFasesDetail', N'U') IS NULL THEN 0 ELSE 1 END")
                    .FirstOrDefault();
                if (exists != 1)
                {
                    return new List<object>();
                }

                var rows = db.Database.SqlQuery<PhaseDetailRow>(
                    "SELECT NumberPhase, TextWallId FROM dbo.TSql_DesingFasesDetail WHERE LinkDesign_V2 = @p0 AND Is_Delete = 0 AND Is_Active = 1 ORDER BY NumberPhase, TextWallId",
                    designId)
                    .ToList();
                var result = new List<object>();
                foreach (var row in rows)
                {
                    result.Add(new { Phase = row.NumberPhase, WallId = row.TextWallId });
                }

                return result;
            }
            catch
            {
                return new List<object>();
            }
        }

        private string TryLoadPhasePlanJson(long designId)
        {
            try
            {
                var exists = db.Database.SqlQuery<int>(
                    "SELECT CASE WHEN COL_LENGTH('dbo.TSql_Design_V2', 'AttPhasePlanJson') IS NULL THEN 0 ELSE 1 END")
                    .FirstOrDefault();
                if (exists != 1)
                {
                    return null;
                }

                return db.Database.SqlQuery<string>(
                    "SELECT CAST(AttPhasePlanJson AS nvarchar(max)) FROM dbo.TSql_Design_V2 WHERE SysObjectID = @p0 AND AttIsDeleted = 0",
                    designId)
                    .FirstOrDefault();
            }
            catch
            {
                return null;
            }
        }

        private bool TrySavePhasePlanJson(long designId, string userId, string planJson)
        {
            try
            {
                var exists = db.Database.SqlQuery<int>(
                    "SELECT CASE WHEN COL_LENGTH('dbo.TSql_Design_V2', 'AttPhasePlanJson') IS NULL THEN 0 ELSE 1 END")
                    .FirstOrDefault();
                if (exists != 1)
                {
                    return false;
                }

                var updated = db.Database.ExecuteSqlCommand(
                    "UPDATE dbo.TSql_Design_V2 SET AttPhasePlanJson = @p0, AttChange = @p1, LinModifiedBy = @p2, SysUpdateNumber = SysUpdateNumber + 1 WHERE SysObjectID = @p3 AND AttIsDeleted = 0",
                    planJson,
                    DateTime.Now,
                    userId ?? "",
                    designId);
                return updated == 1;
            }
            catch
            {
                return false;
            }
        }

        private bool TrySaveOptionalViewerStateJson(long designId, string stateJson)
        {
            try
            {
                var exists = db.Database.SqlQuery<int>(
                    "SELECT CASE WHEN COL_LENGTH('dbo.TSql_Design_V2', 'AttViewerStateJson') IS NULL THEN 0 ELSE 1 END")
                    .FirstOrDefault();
                if (exists != 1)
                {
                    return false;
                }

                db.Database.ExecuteSqlCommand(
                    "UPDATE dbo.TSql_Design_V2 SET AttViewerStateJson = @p0 WHERE SysObjectID = @p1 AND AttIsDeleted = 0",
                    stateJson,
                    designId);
                return true;
            }
            catch
            {
                return false;
            }
        }

        private bool TrySaveLegacyDesignCameras(long designId, string userId, long activeType, JObject state)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(userId))
                {
                    return false;
                }

                var oldDesign = db.TSql_Design.FirstOrDefault(d => d.SysObjectID == designId && !d.AttIsDeleted);
                if (oldDesign == null)
                {
                    return false;
                }

                var target = ReadViewerStateVector(state["target"]);
                var existing = db.TSql_DesignCamera
                    .Where(c => c.LinDesaing == designId && c.LinUser == userId && (c.AttCameraType == 0L || c.AttCameraType == 1L))
                    .ToList();
                foreach (var row in existing)
                {
                    db.TSql_DesignCamera.Remove(row);
                }

                AddLegacyDesignCamera(designId, userId, 0L, state["cameraOrtho"] as JObject, target);
                AddLegacyDesignCamera(designId, userId, 1L, state["cameraIso"] as JObject, target);
                oldDesign.AttActiveCameraType = activeType;
                oldDesign.AttChange = DateTime.Now;
                oldDesign.LinModifiedBy = userId;
                oldDesign.SysUpdateNumber += 1;
                db.SaveChanges();
                return true;
            }
            catch
            {
                return false;
            }
        }

        private void AddLegacyDesignCamera(long designId, string userId, long cameraType, JObject cameraState, double[] target)
        {
            if (cameraState == null)
            {
                return;
            }

            var position = ReadViewerStateVector(cameraState["position"]);
            var zoom = cameraState.Value<double?>("zoom") ?? 1d;
            db.TSql_DesignCamera.Add(new TSql_DesignCamera
            {
                LinUser = userId,
                LinDesaing = designId,
                AttCameraType = cameraType,
                AttPositionX = position[0],
                AttPositionY = position[1],
                AttPositionZ = position[2],
                AttTargetX = target[0],
                AttTargetY = target[1],
                AttTargetZ = target[2],
                AttZoom = zoom,
                AttLastModification = DateTime.UtcNow,
                AttIsActive = true,
                LinModifiedBy = userId
            });
        }

        private static JObject NormalizeViewerState(string raw)
        {
            if (string.IsNullOrWhiteSpace(raw))
            {
                return null;
            }

            try
            {
                return NormalizeViewerState(JObject.Parse(raw));
            }
            catch
            {
                return null;
            }
        }

        private static JObject NormalizeViewerState(JObject state)
        {
            if (state == null)
            {
                return null;
            }

            var version = state.Value<int?>("v") ?? 0;
            if (version != 1)
            {
                return null;
            }

            NormalizeViewerCameraState(state["cameraOrtho"] as JObject);
            NormalizeViewerCameraState(state["cameraIso"] as JObject);
            return state;
        }

        private static void NormalizeViewerCameraState(JObject cameraState)
        {
            if (cameraState == null)
            {
                return;
            }

            var zoom = cameraState.Value<double?>("zoom");
            if (zoom.HasValue && (!double.IsNaN(zoom.Value) && !double.IsInfinity(zoom.Value)))
            {
                cameraState["zoom"] = Math.Max(0.01d, Math.Min(zoom.Value, 1000d));
            }
            else
            {
                cameraState.Remove("zoom");
            }
        }

        private static long ResolveViewerStateActiveCameraType(JObject state)
        {
            var active = (state.Value<string>("activeCamera") ?? "").Trim();
            return active.Equals("iso", StringComparison.OrdinalIgnoreCase) ? 1L : 0L;
        }

        private static double[] ReadViewerStateVector(JToken token)
        {
            var values = new[] { 0d, 0d, 0d };
            var array = token as JArray;
            if (array == null)
            {
                return values;
            }

            for (var i = 0; i < values.Length && i < array.Count; i++)
            {
                var n = array[i].Value<double?>();
                if (n.HasValue && !double.IsNaN(n.Value) && !double.IsInfinity(n.Value))
                {
                    values[i] = n.Value;
                }
            }

            return values;
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

        private sealed class MaterialCountGroup
        {
            public string Code { get; set; }
            public string SceneCode { get; set; }
            public int Qty { get; set; }
            public long? MasterId { get; set; }
        }

        private sealed class MasterArticleCountRow
        {
            public long IdObject { get; set; }
            public string TextCode { get; set; }
            public string AddAtenkoCode { get; set; }
            public string TextLabel { get; set; }
            public string TextColor1 { get; set; }
            public string TextColor2 { get; set; }
            public double? NumberWeight { get; set; }
            public double? NumberMts2 { get; set; }
            public double? NumberHigh { get; set; }
            public double? NumberWidth { get; set; }
            public string LinkBlockDwgPlantStl { get; set; }
            public string LinkBlockDwgVerticalElevationStl { get; set; }
            public string LinkBlockDwgHorizontalElevationStl { get; set; }
            public string LinkBlockDwg3D { get; set; }
            public string LinkBlockDwgPlant3D { get; set; }
        }

        private sealed class CountArticleStlInfo
        {
            public string StlUrl { get; set; }
            public string StlUrl2 { get; set; }
            public string Color1 { get; set; }
            public string Color2 { get; set; }
        }

        /// <summary>
        /// Misma resolución que el maestro de artículos y el plugin: panel ATK-60, STL del bloque, o fichero en ATK60.
        /// </summary>
        private CountArticleStlInfo ResolveCountArticleStl(MasterArticleCountRow master, string sceneCode)
        {
            var info = new CountArticleStlInfo
            {
                Color1 = NormalizeCountArticleHex(master == null ? null : master.TextColor1),
                Color2 = NormalizeCountArticleHex(master == null ? null : master.TextColor2)
            };

            if (IsAdjustableClampCode(sceneCode)
                || IsAdjustableClampCode(master == null ? null : master.TextCode)
                || IsAdjustableClampCode(master == null ? null : master.AddAtenkoCode))
            {
                info.StlUrl = PublicStlIfExists("~/Content/DesignTools/Stl/ATK60/10000221.stl");
                info.StlUrl2 = PublicStlIfExists("~/Content/DesignTools/Stl/ATK60/10000221-2.stl");
                return info;
            }

            if (master != null)
            {
                var stub = new Tsql_Master_Articles
                {
                    TextCode = master.TextCode,
                    AddAtenkoCode = master.AddAtenkoCode,
                    TextLabel = master.TextLabel,
                    NumberHigh = master.NumberHigh,
                    NumberWidth = master.NumberWidth
                };
                string frame;
                string phenolic;
                if (PluginCadAtk60PanelStlHelper.TryResolve(stub, out frame, out phenolic))
                {
                    info.StlUrl = PublicStlIfExists(frame);
                    info.StlUrl2 = PublicStlIfExists(phenolic);
                }

                if (string.IsNullOrWhiteSpace(info.StlUrl))
                {
                    var candidates = new[]
                    {
                        master.LinkBlockDwgPlantStl,
                        master.LinkBlockDwgVerticalElevationStl,
                        master.LinkBlockDwgHorizontalElevationStl
                    };
                    for (var i = 0; i < candidates.Length && string.IsNullOrWhiteSpace(info.StlUrl); i++)
                    {
                        info.StlUrl = PublicStlIfExists(candidates[i]);
                    }

                    if (string.IsNullOrWhiteSpace(info.StlUrl))
                    {
                        var dwg = FirstCountNonEmpty(master.LinkBlockDwg3D, master.LinkBlockDwgPlant3D);
                        if (!string.IsNullOrWhiteSpace(dwg))
                        {
                            var sibling = Path.ChangeExtension(dwg.Trim().Replace('\\', '/'), ".stl");
                            info.StlUrl = PublicStlIfExists(sibling);
                        }
                    }

                    if (!string.IsNullOrWhiteSpace(info.StlUrl))
                    {
                        info.StlUrl2 = PublicStlIfExists(Stem2StlVirtual(info.StlUrl));
                        if (string.IsNullOrWhiteSpace(info.StlUrl2))
                        {
                            info.StlUrl2 = PublicStlIfExists(PhenolicStlVirtual(info.StlUrl));
                        }
                    }
                }
            }

            if (string.IsNullOrWhiteSpace(info.StlUrl))
            {
                var names = new[]
                {
                    sceneCode,
                    master == null ? null : master.TextCode,
                    master == null ? null : master.AddAtenkoCode
                };
                for (var i = 0; i < names.Length && string.IsNullOrWhiteSpace(info.StlUrl); i++)
                {
                    var name = CountArticleStlFileName(names[i]);
                    if (name == null)
                    {
                        continue;
                    }

                    info.StlUrl = PublicStlIfExists("~/Content/DesignTools/Stl/ATK60/" + name + ".stl");
                    if (!string.IsNullOrWhiteSpace(info.StlUrl))
                    {
                        info.StlUrl2 = PublicStlIfExists("~/Content/DesignTools/Stl/ATK60/" + name + "_F.stl");
                    }
                }
            }

            return info;
        }

        private static bool IsAdjustableClampCode(string code)
        {
            if (string.IsNullOrWhiteSpace(code))
            {
                return false;
            }

            var key = NormalizeMaterialCode(code);
            return string.Equals(key, "10000221", StringComparison.OrdinalIgnoreCase)
                || string.Equals(key, "10000221B", StringComparison.OrdinalIgnoreCase)
                || string.Equals(key, "1120100040", StringComparison.OrdinalIgnoreCase);
        }

        private static string NormalizeCountArticleHex(string input)
        {
            if (string.IsNullOrWhiteSpace(input))
            {
                return "#000000";
            }

            var t = input.Trim();
            if (t.Length > 0 && t[0] != '#')
            {
                t = "#" + t;
            }

            if (t.Length > 7)
            {
                t = t.Substring(0, 7);
            }

            if (Regex.IsMatch(t, "^#[0-9a-fA-F]{6}$") || Regex.IsMatch(t, "^#[0-9a-fA-F]{3}$"))
            {
                return t;
            }

            return "#000000";
        }

        private static string FirstCountNonEmpty(params string[] values)
        {
            if (values == null)
            {
                return null;
            }

            for (var i = 0; i < values.Length; i++)
            {
                if (!string.IsNullOrWhiteSpace(values[i]))
                {
                    return values[i].Trim();
                }
            }

            return null;
        }

        private static string CountArticleStlFileName(string code)
        {
            if (string.IsNullOrWhiteSpace(code))
            {
                return null;
            }

            var name = code.Trim();
            if (name.StartsWith("PANEL_", StringComparison.OrdinalIgnoreCase))
            {
                name = name.Substring(6);
            }

            if (name.EndsWith("_FRAME", StringComparison.OrdinalIgnoreCase))
            {
                name = name.Substring(0, name.Length - 6);
            }

            return Regex.IsMatch(name, "^[A-Za-z0-9._-]+$") ? name : null;
        }

        private static string Stem2StlVirtual(string publicUrl)
        {
            var path = CountArticleStlPathOnly(publicUrl);
            if (path == null || path.Length < 5)
            {
                return null;
            }

            return path.Substring(0, path.Length - 4) + "2.stl";
        }

        private static string PhenolicStlVirtual(string publicUrl)
        {
            var path = CountArticleStlPathOnly(publicUrl);
            if (path == null || path.Length < 5)
            {
                return null;
            }

            if (path.EndsWith("P.stl", StringComparison.OrdinalIgnoreCase))
            {
                return path.Substring(0, path.Length - 5) + "P2.stl";
            }

            return path.Substring(0, path.Length - 4) + "_F.stl";
        }

        private static string CountArticleStlPathOnly(string publicUrl)
        {
            if (string.IsNullOrWhiteSpace(publicUrl))
            {
                return null;
            }

            var path = publicUrl.Trim().Replace('\\', '/');
            var q = path.IndexOf('?');
            if (q >= 0)
            {
                path = path.Substring(0, q);
            }

            return path.EndsWith(".stl", StringComparison.OrdinalIgnoreCase) ? path : null;
        }

        private string PublicStlIfExists(string virtualPath)
        {
            if (string.IsNullOrWhiteSpace(virtualPath))
            {
                return null;
            }

            var t = virtualPath.Trim().Replace('\\', '/');
            var q = t.IndexOf('?');
            if (q >= 0)
            {
                t = t.Substring(0, q);
            }

            if (t.StartsWith("http", StringComparison.OrdinalIgnoreCase) || t.StartsWith("//", StringComparison.Ordinal))
            {
                return null;
            }

            string appRel;
            if (t.StartsWith("~/", StringComparison.Ordinal))
            {
                appRel = t;
            }
            else if (t.StartsWith("/", StringComparison.Ordinal))
            {
                appRel = "~" + t;
            }
            else
            {
                appRel = "~/" + t.TrimStart('/');
            }

            if (!appRel.EndsWith(".stl", StringComparison.OrdinalIgnoreCase))
            {
                return null;
            }

            try
            {
                var physical = Server.MapPath(appRel);
                if (string.IsNullOrWhiteSpace(physical) || !System.IO.File.Exists(physical))
                {
                    return null;
                }

                return Url.Content(appRel);
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// Grupos del diseño: número y nombre en dbo.TSql_DesgloseGrupo.
        /// </summary>
        [HttpGet]
        public JsonResult ListDesignGroups(long designId)
        {
            try
            {
                if (designId <= 0)
                {
                    return Json(new { ok = true, groups = new object[0] }, JsonRequestBehavior.AllowGet);
                }

                return Json(new { ok = true, groups = ReadDesignGroups(designId), nextNumber = ReadNextGroupNumber(designId) }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { ok = false, message = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        /// <summary>
        /// Crea, renombra y borra (lógico) los grupos de un diseño en un solo paso.
        /// </summary>
        [HttpPost]
        public JsonResult SaveDesignGroups()
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
                    return Json(new { ok = false, message = "JSON vacío." });
                }

                var body = JObject.Parse(rawJson);
                var designId = body.Value<long?>("designId") ?? 0;
                if (designId <= 0)
                {
                    return Json(new { ok = false, message = "Diseño no indicado." });
                }

                var exists = db.Database.SqlQuery<int>(
                    "SELECT COUNT(1) FROM dbo.TSql_Design_V2 WHERE SysObjectID = @p0 AND AttIsDeleted = 0",
                    designId).FirstOrDefault();
                if (exists != 1)
                {
                    return Json(new { ok = false, message = "Diseño no encontrado." });
                }

                var existing = db.Database.SqlQuery<DesignGroupRow>(
                    "SELECT IdObject, TextLabel, NumberGroup FROM dbo.TSql_DesgloseGrupo WHERE LinkDesign_V2 = @p0 AND Is_Delete = 0",
                    designId).ToList();
                var usedEver = db.Database.SqlQuery<GroupNumberRow>(
                    "SELECT IdObject, NumberGroup FROM dbo.TSql_DesgloseGrupo WHERE LinkDesign_V2 = @p0",
                    designId).ToList();
                var byId = new Dictionary<long, DesignGroupRow>();
                for (var i = 0; i < existing.Count; i++)
                {
                    byId[existing[i].IdObject] = existing[i];
                }

                var deleteIds = new List<long>();
                var deletedToken = body["deleted"] as JArray;
                if (deletedToken != null)
                {
                    for (var i = 0; i < deletedToken.Count; i++)
                    {
                        var id = deletedToken[i].Value<long?>() ?? 0;
                        if (id <= 0)
                        {
                            continue;
                        }

                        if (!byId.ContainsKey(id))
                        {
                            return Json(new { ok = false, message = "Grupo no encontrado." });
                        }

                        if (!deleteIds.Contains(id))
                        {
                            deleteIds.Add(id);
                        }
                    }
                }

                var updates = new List<DesignGroupWrite>();
                var inserts = new List<DesignGroupWrite>();
                var usedNumbers = new HashSet<long>();
                var rowsToken = body["rows"] as JArray;
                if (rowsToken != null)
                {
                    for (var i = 0; i < rowsToken.Count; i++)
                    {
                        var row = rowsToken[i] as JObject;
                        if (row == null)
                        {
                            continue;
                        }

                        var id = row.Value<long?>("id") ?? 0;
                        var number = row.Value<long?>("number");
                        var name = (row.Value<string>("name") ?? string.Empty).Trim();
                        if (id <= 0 && string.IsNullOrEmpty(name) && !number.HasValue)
                        {
                            continue;
                        }

                        if (!number.HasValue || number.Value < 0 || string.IsNullOrEmpty(name))
                        {
                            return Json(new { ok = false, message = "Cada grupo necesita número y nombre." });
                        }

                        if (name.Length > 500)
                        {
                            name = name.Substring(0, 500);
                        }

                        if (!usedNumbers.Add(number.Value))
                        {
                            return Json(new { ok = false, message = "El número de grupo está repetido." });
                        }

                        if (id > 0)
                        {
                            if (!byId.ContainsKey(id) || deleteIds.Contains(id))
                            {
                                return Json(new { ok = false, message = "Grupo no encontrado." });
                            }

                            updates.Add(new DesignGroupWrite { Id = id, Number = number.Value, Name = name });
                        }
                        else
                        {
                            inserts.Add(new DesignGroupWrite { Number = number.Value, Name = name });
                        }
                    }
                }

                for (var i = 0; i < existing.Count; i++)
                {
                    var row = existing[i];
                    if (deleteIds.Contains(row.IdObject))
                    {
                        continue;
                    }

                    var updated = false;
                    for (var u = 0; u < updates.Count; u++)
                    {
                        if (updates[u].Id == row.IdObject)
                        {
                            updated = true;
                            break;
                        }
                    }

                    if (!updated && !usedNumbers.Add(row.NumberGroup))
                    {
                        return Json(new { ok = false, message = "El número de grupo está repetido." });
                    }
                }

                for (var i = 0; i < inserts.Count; i++)
                {
                    if (GroupNumberWasUsed(usedEver, inserts[i].Number, 0))
                    {
                        return Json(new { ok = false, message = "Ese número de grupo ya se ha usado." });
                    }
                }

                for (var i = 0; i < updates.Count; i++)
                {
                    if (GroupNumberWasUsed(usedEver, updates[i].Number, updates[i].Id))
                    {
                        return Json(new { ok = false, message = "Ese número de grupo ya se ha usado." });
                    }
                }

                var userId = IntranetAuditHelper.ResolveCurrentUserId(User) ?? string.Empty;
                var now = DateTime.Now;
                using (var tx = db.Database.BeginTransaction())
                {
                    for (var i = 0; i < deleteIds.Count; i++)
                    {
                        db.Database.ExecuteSqlCommand(
                            @"UPDATE dbo.TSql_DesgloseGrupo
                              SET Is_Delete = 1, LinModifiedBy = @p0, AddLastDateChange = @p1, Ntimeschanged = Ntimeschanged + 1
                              WHERE IdObject = @p2 AND LinkDesign_V2 = @p3 AND Is_Delete = 0",
                            userId,
                            now,
                            deleteIds[i],
                            designId);
                    }

                    for (var i = 0; i < updates.Count; i++)
                    {
                        var row = updates[i];
                        db.Database.ExecuteSqlCommand(
                            @"UPDATE dbo.TSql_DesgloseGrupo
                              SET TextLabel = @p0, NumberGroup = @p1, LinModifiedBy = @p2, AddLastDateChange = @p3, Ntimeschanged = Ntimeschanged + 1
                              WHERE IdObject = @p4 AND LinkDesign_V2 = @p5 AND Is_Delete = 0",
                            row.Name,
                            row.Number,
                            userId,
                            now,
                            row.Id,
                            designId);
                    }

                    for (var i = 0; i < inserts.Count; i++)
                    {
                        var row = inserts[i];
                        db.Database.ExecuteSqlCommand(
                            @"INSERT INTO dbo.TSql_DesgloseGrupo
                                (TextLabel, LinkDesign_V2, NumberGroup, Is_Delete, Is_Active, LinkMadeBy, LinModifiedBy, AddDateMade, AddLastDateChange, Ntimeschanged)
                              VALUES (@p0, @p1, @p2, 0, 1, @p3, NULL, @p4, NULL, 0)",
                            row.Name,
                            designId,
                            row.Number,
                            userId,
                            now);
                    }

                    tx.Commit();
                }

                return Json(new { ok = true, groups = ReadDesignGroups(designId), nextNumber = ReadNextGroupNumber(designId) });
            }
            catch (JsonReaderException ex)
            {
                return Json(new { ok = false, message = "JSON inválido: " + ex.Message });
            }
            catch (Exception ex)
            {
                var text = ex.ToString();
                if (text.IndexOf("UX_TSql_DesgloseGrupo_Design_NumberGroup", StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    return Json(new { ok = false, message = "El número de grupo está repetido." });
                }

                return Json(new { ok = false, message = ex.Message });
            }
        }

        private IList<object> ReadDesignGroups(long designId)
        {
            var rows = db.Database.SqlQuery<DesignGroupRow>(
                "SELECT IdObject, TextLabel, NumberGroup FROM dbo.TSql_DesgloseGrupo WHERE LinkDesign_V2 = @p0 AND Is_Delete = 0 ORDER BY NumberGroup, IdObject",
                designId).ToList();
            var list = new List<object>(rows.Count);
            for (var i = 0; i < rows.Count; i++)
            {
                list.Add(new
                {
                    id = rows[i].IdObject,
                    number = rows[i].NumberGroup,
                    name = rows[i].TextLabel
                });
            }

            return list;
        }

        private long ReadNextGroupNumber(long designId)
        {
            var max = db.Database.SqlQuery<long>(
                "SELECT CAST(ISNULL(MAX(NumberGroup), 0) AS bigint) FROM dbo.TSql_DesgloseGrupo WHERE LinkDesign_V2 = @p0",
                designId).FirstOrDefault();
            if (max < 0)
            {
                max = 0;
            }

            return max + 1;
        }

        private static bool GroupNumberWasUsed(List<GroupNumberRow> usedEver, long number, long exceptId)
        {
            for (var i = 0; i < usedEver.Count; i++)
            {
                if (usedEver[i].NumberGroup == number && usedEver[i].IdObject != exceptId)
                {
                    return true;
                }
            }

            return false;
        }

        public sealed class GroupNumberRow
        {
            public long IdObject { get; set; }
            public long NumberGroup { get; set; }
        }

        public sealed class DesignGroupRow
        {
            public long IdObject { get; set; }
            public string TextLabel { get; set; }
            public long NumberGroup { get; set; }
        }

        public sealed class DesignGroupWrite
        {
            public long Id { get; set; }
            public long Number { get; set; }
            public string Name { get; set; }
        }

        private const double DesignPropSpacingDefaultMm = 2400d;

        private double ReadDesignPropSpacingMm(long? designId)
        {
            if (!designId.HasValue || designId.Value <= 0)
            {
                return DesignPropSpacingDefaultMm;
            }

            try
            {
                var value = db.Database.SqlQuery<double>(
                    "SELECT CAST(ISNULL(NumberPropSpacingMm, 2400) AS float) FROM dbo.TSql_Design_V2 WHERE SysObjectID = @p0 AND AttIsDeleted = 0",
                    designId.Value).FirstOrDefault();
                if (value < 300d || value > 6000d)
                {
                    return DesignPropSpacingDefaultMm;
                }

                return value;
            }
            catch
            {
                return DesignPropSpacingDefaultMm;
            }
        }

        /// <summary>
        /// Guarda la distancia máxima entre puntales del diseño (mm). NULL en lectura = 2,40 m.
        /// </summary>
        [HttpPost]
        public JsonResult SavePropSpacing(long designId, double spacingMm)
        {
            try
            {
                if (designId <= 0)
                {
                    return Json(new { Exito = false, Mensaje = "Diseño no indicado." });
                }

                if (spacingMm > 0d && spacingMm < 20d)
                {
                    spacingMm *= 1000d;
                }

                if (spacingMm < 300d || spacingMm > 6000d)
                {
                    return Json(new { Exito = false, Mensaje = "La distancia tiene que estar entre 0,30 m y 6 m." });
                }

                var userId = IntranetAuditHelper.ResolveCurrentUserId(User) ?? "";
                var updated = db.Database.ExecuteSqlCommand(
                    "UPDATE dbo.TSql_Design_V2 SET NumberPropSpacingMm = @p0, AttChange = @p1, LinModifiedBy = @p2, SysUpdateNumber = SysUpdateNumber + 1 WHERE SysObjectID = @p3 AND AttIsDeleted = 0",
                    spacingMm,
                    DateTime.Now,
                    userId,
                    designId);
                if (updated != 1)
                {
                    return Json(new { Exito = false, Mensaje = "Diseño no encontrado." });
                }

                return Json(new { Exito = true, SpacingMm = spacingMm });
            }
            catch (Exception ex)
            {
                return Json(new { Exito = false, Mensaje = ex.Message });
            }
        }

    }

}

