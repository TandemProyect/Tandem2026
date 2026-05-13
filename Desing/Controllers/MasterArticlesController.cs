using DataTables.Mvc;
using Desing.Helpers;
using Desing.Models;
using Desing.Services;
using Microsoft.AspNet.Identity;
using System;
using System.Data.Entity;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Threading;
using System.Threading.Tasks;
using System.Text;
using System.Web;
using System.Web.Mvc;

namespace Desing.Controllers
{
    public class MasterArticlesController : BaseController
    {
        private const long BlockFileMaxBytes = 50L * 1024 * 1024;

        private static readonly string[] BlockLinkKeys =
        {
            "LinkBlockDwgPlant3D",
            "LinkBlockDwgVerticalElevation3D",
            "LinkBlockDwgHorizontalElevation3D",
            "LinkBlockDwgPlantMckUp",
            "LinkBlockDwgVerticalElevationMockUp",
            "LinkBlockDwgHorizontalElevationMockUp",
            "LinkBlockDwgPlantStl",
            "LinkBlockDwgVerticalElevationStl",
            "LinkBlockDwgHorizontalElevationStl"
        };

        private static readonly HashSet<string> BlockLinkKeysDwgOnly = new HashSet<string>(StringComparer.Ordinal)
        {
            "LinkBlockDwgPlant3D",
            "LinkBlockDwgVerticalElevation3D",
            "LinkBlockDwgHorizontalElevation3D",
            "LinkBlockDwgPlantMckUp",
            "LinkBlockDwgVerticalElevationMockUp",
            "LinkBlockDwgHorizontalElevationMockUp"
        };

        private static readonly HashSet<string> BlockLinkKeysStlOnly = new HashSet<string>(StringComparer.Ordinal)
        {
            "LinkBlockDwgPlantStl",
            "LinkBlockDwgVerticalElevationStl",
            "LinkBlockDwgHorizontalElevationStl"
        };

        private static string[] AllowedExtensionsForBlockKey(string key)
        {
            if (BlockLinkKeysDwgOnly.Contains(key))
            {
                return new[] { ".dwg" };
            }
            if (BlockLinkKeysStlOnly.Contains(key))
            {
                return new[] { ".stl" };
            }
            return new string[0];
        }

        private static string AllowedExtensionsHumanLabel(string key)
        {
            var exts = AllowedExtensionsForBlockKey(key);
            return exts.Length == 0 ? "" : string.Join(", ", exts);
        }

        /// <summary>
        /// ContentLength puede ser -1 (longitud desconocida); tratarlo como adjunto si el cliente envió nombre de archivo.
        /// </summary>
        private static bool HasPostedNonEmptyFile(HttpPostedFileBase posted)
        {
            if (posted == null)
            {
                return false;
            }
            if (posted.ContentLength > 0)
            {
                return true;
            }
            if (posted.ContentLength == 0)
            {
                return false;
            }
            return !string.IsNullOrWhiteSpace(posted.FileName);
        }

        private static string NormalizeAppRelativeVirtualPath(string path)
        {
            if (string.IsNullOrWhiteSpace(path))
            {
                return null;
            }
            var t = path.Trim().Replace('\\', '/');
            if (t.StartsWith("~/", StringComparison.Ordinal))
            {
                return t;
            }
            if (t.StartsWith("/", StringComparison.Ordinal))
            {
                return "~" + t;
            }
            return "~/" + t.TrimStart('/');
        }

        public ActionResult Index()
        {
            return View();
        }

        public ActionResult Create()
        {
            PopulateLinkSystem(null);
            var article = new DAL.Tsql_Master_Articles
            {
                AddIsActive = true,
                IInsertinMaterArticles = false,
                LinkSystem = 0
            };
            return View(article);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "TextCode,TextLabel,NumberHigh,NumberWidth,NumberLong,NumberWeight,NumberMts2,NumberMts3,TextBlockNumber,TextStlNumber,TextColor1,TextColor2,LinkSystem,AddIsActive,AddAtenkoCode,IInsertinMaterArticles,LinkBlockDwgPlant3D,LinkBlockDwgVerticalElevation3D,LinkBlockDwgHorizontalElevation3D,LinkBlockDwgPlantMckUp,LinkBlockDwgVerticalElevationMockUp,LinkBlockDwgHorizontalElevationMockUp,LinkBlockDwgPlantStl,LinkBlockDwgVerticalElevationStl,LinkBlockDwgHorizontalElevationStl")] DAL.Tsql_Master_Articles model)
        {
            model.TextCode = (model.TextCode ?? string.Empty).Trim();
            model.TextLabel = (model.TextLabel ?? string.Empty).Trim();
            if (string.IsNullOrEmpty(model.TextCode))
            {
                ModelState.AddModelError("TextCode", "El código es obligatorio.");
            }
            if (string.IsNullOrEmpty(model.TextLabel))
            {
                ModelState.AddModelError("TextLabel", "La descripción es obligatoria.");
            }
            if (model.LinkSystem <= 0)
            {
                ModelState.AddModelError("LinkSystem", "Seleccione un sistema.");
            }
            else if (!AllowedLinkSystemIds().Contains(model.LinkSystem))
            {
                ModelState.AddModelError("LinkSystem", "El sistema seleccionado no es válido.");
            }
            if (model.TextColor2 != null && model.TextColor2.Length > 10)
            {
                model.TextColor2 = model.TextColor2.Substring(0, 10);
            }
            if (db.Tsql_Master_Articles.Any(a => a.LinkSystem == model.LinkSystem && a.TextCode == model.TextCode))
            {
                ModelState.AddModelError("TextCode", "Ya existe un artículo con este código en el mismo sistema.");
            }

            PreValidateBlockFiles(Request);

            if (!ModelState.IsValid)
            {
                PopulateLinkSystem(model.LinkSystem > 0 ? (long?)model.LinkSystem : null);
                return View(model);
            }

            var userId = User.Identity.GetUserId();
            var now = DateTime.UtcNow;
            model.AddAtenkoCode = string.IsNullOrWhiteSpace(model.AddAtenkoCode) ? null : model.AddAtenkoCode.Trim();
            model.TextBlockNumber = string.IsNullOrWhiteSpace(model.TextBlockNumber) ? null : model.TextBlockNumber.Trim();
            model.TextStlNumber = string.IsNullOrWhiteSpace(model.TextStlNumber) ? null : model.TextStlNumber.Trim();
            model.TextColor1 = string.IsNullOrWhiteSpace(model.TextColor1) ? null : model.TextColor1.Trim();
            model.TextColor2 = string.IsNullOrWhiteSpace(model.TextColor2) ? null : model.TextColor2.Trim();
            model.LinkMadeBy = userId;
            model.AddChangeBy = userId;
            model.AddDateMade = now;
            model.AddLastDateChange = now;
            model.Ntimeschanged = 1;

            db.Tsql_Master_Articles.Add(model);
            db.SaveChanges();

            MergeArticleBlockFiles(model, Request);
            db.SaveChanges();
            if (!ModelState.IsValid)
            {
                var errs = string.Join(" ", ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).Where(m => !string.IsNullOrWhiteSpace(m)));
                TempData["ToastMessage"] = string.IsNullOrWhiteSpace(errs)
                    ? "Artículo creado. Revise los adjuntos en la edición."
                    : "Artículo creado. " + errs;
                return RedirectToAction("Edit", new { id = model.IdObject });
            }

            TempData["ToastType"] = "Act";
            TempData["ToastTitle"] = "Artículos";
            TempData["ToastMessage"] = "Artículo creado correctamente.";
            return RedirectToAction("Index");
        }

        public ActionResult Edit(long id)
        {
            var article = db.Tsql_Master_Articles.FirstOrDefault(a => a.IdObject == id);
            if (article == null)
            {
                return HttpNotFound();
            }
            PopulateLinkSystem(article.LinkSystem);
            PopulateMasterArticleStlPreviewViewData(article, article.IdObject);
            return View(article);
        }

        public ActionResult Details(long id)
        {
            var row = (from article in db.Tsql_Master_Articles
                       join sys in db.TSql_System on article.LinkSystem equals sys.IdObject
                       join comp in db.TSql_Company on sys.LinkCompany equals comp.SysObjectID
                       where article.IdObject == id
                       select new { article, CompanyText = comp.TextLabel, SystemText = sys.TextLabel }).FirstOrDefault();
            if (row == null)
            {
                return HttpNotFound();
            }

            ViewBag.Title = "Detalles del artículo";
            var slots = BuildMasterArticleAttachmentSlots(row.article).ToList();
            EnrichAttachmentSlotsWithStlPreview(slots, row.article.IdObject);
            var vm = new MasterArticleDetailsViewModel
            {
                Article = row.article,
                CompanyTextLabel = row.CompanyText,
                SystemTextLabel = row.SystemText,
                AttachmentSlots = slots
            };
            return View(vm);
        }

        /// <summary>
        /// Sirve el DXF gemelo para el visor three-dxf: mismo directorio y nombre base que el .dwg (p. ej. <c>27104209.dxf</c> junto a <c>27104209.dwg</c>).
        /// El DXF debe existir en disco (exportación manual o herramienta al guardar).
        /// </summary>
        [HttpGet]
        public ActionResult MasterArticleViewerDxf(long id, string slotKey)
        {
            if (string.IsNullOrWhiteSpace(slotKey) || !BlockLinkKeysDwgOnly.Contains(slotKey))
            {
                return HttpNotFound();
            }

            if (!TryResolveMasterArticleDwgPhysicalPath(id, slotKey, out var dwgPhysical, out _))
            {
                return HttpNotFound();
            }

            var dxfPath = MasterArticleViewerDxfConverter.GetSiblingPreviewDxfPhysicalPath(dwgPhysical);
            if (!System.IO.File.Exists(dxfPath))
            {
                Response.TrySkipIisCustomErrors = true;
                Response.StatusCode = 404;
                return Content(
                    "No se encontró el DXF gemelo (mismo nombre y carpeta que el .dwg, extensión .dxf). Coloque el archivo en el servidor o genérelo al guardar el artículo.",
                    "text/plain",
                    Encoding.UTF8);
            }

            Response.Cache.SetCacheability(HttpCacheability.NoCache);
            return File(dxfPath, "application/dxf", Path.GetFileName(dxfPath));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "IdObject,TextCode,TextLabel,NumberHigh,NumberWidth,NumberLong,NumberWeight,NumberMts2,NumberMts3,TextBlockNumber,TextStlNumber,TextColor1,TextColor2,LinkSystem,AddIsActive,AddAtenkoCode,IInsertinMaterArticles,LinkBlockDwgPlant3D,LinkBlockDwgVerticalElevation3D,LinkBlockDwgHorizontalElevation3D,LinkBlockDwgPlantMckUp,LinkBlockDwgVerticalElevationMockUp,LinkBlockDwgHorizontalElevationMockUp,LinkBlockDwgPlantStl,LinkBlockDwgVerticalElevationStl,LinkBlockDwgHorizontalElevationStl")] DAL.Tsql_Master_Articles model)
        {
            var article = db.Tsql_Master_Articles.FirstOrDefault(a => a.IdObject == model.IdObject);
            if (article == null)
            {
                return HttpNotFound();
            }

            model.TextCode = (model.TextCode ?? string.Empty).Trim();
            model.TextLabel = (model.TextLabel ?? string.Empty).Trim();
            if (string.IsNullOrEmpty(model.TextCode))
            {
                ModelState.AddModelError("TextCode", "El código es obligatorio.");
            }
            if (string.IsNullOrEmpty(model.TextLabel))
            {
                ModelState.AddModelError("TextLabel", "La descripción es obligatoria.");
            }
            if (model.LinkSystem <= 0)
            {
                ModelState.AddModelError("LinkSystem", "Seleccione un sistema.");
            }
            else if (!AllowedLinkSystemIds().Contains(model.LinkSystem))
            {
                ModelState.AddModelError("LinkSystem", "El sistema seleccionado no es válido.");
            }
            if (model.TextColor2 != null && model.TextColor2.Length > 10)
            {
                model.TextColor2 = model.TextColor2.Substring(0, 10);
            }
            if (db.Tsql_Master_Articles.Any(a => a.IdObject != model.IdObject && a.LinkSystem == model.LinkSystem && a.TextCode == model.TextCode))
            {
                ModelState.AddModelError("TextCode", "Ya existe otro artículo con este código en el mismo sistema.");
            }

            PreValidateBlockFiles(Request);

            if (!ModelState.IsValid)
            {
                PopulateLinkSystem(model.LinkSystem);
                PopulateMasterArticleStlPreviewViewData(model, model.IdObject);
                return View(model);
            }

            article.AddAtenkoCode = string.IsNullOrWhiteSpace(model.AddAtenkoCode) ? null : model.AddAtenkoCode.Trim();
            article.TextCode = model.TextCode;
            article.TextLabel = model.TextLabel;
            article.NumberHigh = model.NumberHigh;
            article.NumberWidth = model.NumberWidth;
            article.NumberLong = model.NumberLong;
            article.NumberWeight = model.NumberWeight;
            article.NumberMts2 = model.NumberMts2;
            article.NumberMts3 = model.NumberMts3;
            article.TextBlockNumber = string.IsNullOrWhiteSpace(model.TextBlockNumber) ? null : model.TextBlockNumber.Trim();
            article.TextStlNumber = string.IsNullOrWhiteSpace(model.TextStlNumber) ? null : model.TextStlNumber.Trim();
            article.TextColor1 = string.IsNullOrWhiteSpace(model.TextColor1) ? null : model.TextColor1.Trim();
            article.TextColor2 = string.IsNullOrWhiteSpace(model.TextColor2) ? null : model.TextColor2.Trim();
            article.LinkSystem = model.LinkSystem;
            article.AddIsActive = model.AddIsActive;
            article.IInsertinMaterArticles = model.IInsertinMaterArticles;
            CopyLinkStringsFromModel(article, model);
            article.AddChangeBy = User.Identity.GetUserId();
            article.AddLastDateChange = DateTime.UtcNow;
            article.Ntimeschanged = article.Ntimeschanged + 1;

            MergeArticleBlockFiles(article, Request);
            if (!ModelState.IsValid)
            {
                PopulateLinkSystem(model.LinkSystem);
                PopulateMasterArticleStlPreviewViewData(article, article.IdObject);
                return View(article);
            }

            db.SaveChanges();

            TempData["ToastType"] = "Act";
            TempData["ToastTitle"] = "Artículos";
            TempData["ToastMessage"] = "Artículo actualizado correctamente.";
            return RedirectToAction("Index");
        }

        /// <summary>Activa o desactiva el artículo desde la lista (GET; confirmación en el cliente, igual que eliminar).</summary>
        public ActionResult SetArticleActive(long id, bool active)
        {
            var article = db.Tsql_Master_Articles.FirstOrDefault(a => a.IdObject == id);
            if (article == null)
            {
                return HttpNotFound();
            }
            article.AddIsActive = active;
            article.AddChangeBy = User.Identity.GetUserId();
            article.AddLastDateChange = DateTime.UtcNow;
            article.Ntimeschanged = article.Ntimeschanged + 1;
            db.SaveChanges();
            TempData["ToastMessage"] = active ? "Artículo activado." : "Artículo desactivado.";
            return RedirectToAction("Index");
        }

        /// <summary>Elimina el artículo (GET; confirmación en el cliente). Bloqueado si hay referencias en stock o listas temporales.</summary>
        public ActionResult Delete(long id)
        {
            var article = db.Tsql_Master_Articles.FirstOrDefault(a => a.IdObject == id);
            if (article == null)
            {
                TempData["ToastType"] = "Error";
                TempData["ToastMessage"] = "El artículo no existe o ya fue eliminado.";
                return RedirectToAction("Index");
            }

            if (db.Tsql_RemplaceArticleStok.Any(r => r.LinkMaster_Articles == id)
                || db.Tsql_RemplaceArticleStokChildren.Any(c => c.LinkMaster_Articles == id)
                || db.TSql_TemporalList.Any(t => t.linkMasterArticles == id))
            {
                TempData["ToastType"] = "Error";
                TempData["ToastMessage"] = "No se puede eliminar: el artículo está en uso (stock de reemplazo o listas temporales).";
                return RedirectToAction("Index");
            }

            var attachmentPaths = BlockLinkKeys
                .Select(k => GetBlockVirtualPathForSlot(article, k))
                .Where(v => !string.IsNullOrWhiteSpace(v))
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList();

            db.Tsql_Master_Articles.Remove(article);
            try
            {
                db.SaveChanges();
            }
            catch (Exception ex)
            {
                TempData["ToastType"] = "Error";
                TempData["ToastMessage"] = "No se pudo eliminar el artículo. " + ex.Message;
                return RedirectToAction("Index");
            }

            foreach (var v in attachmentPaths)
            {
                TryDeletePhysicalBlockFileIfOwned(v, null);
                if (v.EndsWith(".dwg", StringComparison.OrdinalIgnoreCase))
                {
                    TryDeleteViewerDxfSidecar(v, id);
                }
            }

            try
            {
                var legacyDir = Server.MapPath("~/Files/MasterArticles/blocks/" + id);
                if (Directory.Exists(legacyDir))
                {
                    Directory.Delete(legacyDir, true);
                }
            }
            catch
            {
                // Adjuntos: no impedir éxito si falla el borrado en disco
            }

            TempData["ToastMessage"] = "Artículo eliminado correctamente.";
            return RedirectToAction("Index");
        }

        private static void CopyLinkStringsFromModel(DAL.Tsql_Master_Articles article, DAL.Tsql_Master_Articles model)
        {
            article.LinkBlockDwgPlant3D = string.IsNullOrWhiteSpace(model.LinkBlockDwgPlant3D) ? null : model.LinkBlockDwgPlant3D.Trim();
            article.LinkBlockDwgVerticalElevation3D = string.IsNullOrWhiteSpace(model.LinkBlockDwgVerticalElevation3D) ? null : model.LinkBlockDwgVerticalElevation3D.Trim();
            article.LinkBlockDwgHorizontalElevation3D = string.IsNullOrWhiteSpace(model.LinkBlockDwgHorizontalElevation3D) ? null : model.LinkBlockDwgHorizontalElevation3D.Trim();
            article.LinkBlockDwgPlantMckUp = string.IsNullOrWhiteSpace(model.LinkBlockDwgPlantMckUp) ? null : model.LinkBlockDwgPlantMckUp.Trim();
            article.LinkBlockDwgVerticalElevationMockUp = string.IsNullOrWhiteSpace(model.LinkBlockDwgVerticalElevationMockUp) ? null : model.LinkBlockDwgVerticalElevationMockUp.Trim();
            article.LinkBlockDwgHorizontalElevationMockUp = string.IsNullOrWhiteSpace(model.LinkBlockDwgHorizontalElevationMockUp) ? null : model.LinkBlockDwgHorizontalElevationMockUp.Trim();
            article.LinkBlockDwgPlantStl = string.IsNullOrWhiteSpace(model.LinkBlockDwgPlantStl) ? null : model.LinkBlockDwgPlantStl.Trim();
            article.LinkBlockDwgVerticalElevationStl = string.IsNullOrWhiteSpace(model.LinkBlockDwgVerticalElevationStl) ? null : model.LinkBlockDwgVerticalElevationStl.Trim();
            article.LinkBlockDwgHorizontalElevationStl = string.IsNullOrWhiteSpace(model.LinkBlockDwgHorizontalElevationStl) ? null : model.LinkBlockDwgHorizontalElevationStl.Trim();
        }

        private void PreValidateBlockFiles(HttpRequestBase request)
        {
            foreach (var key in BlockLinkKeys)
            {
                var posted = request.Files["blockFile_" + key];
                if (!HasPostedNonEmptyFile(posted))
                {
                    continue;
                }
                if (posted.ContentLength > 0 && posted.ContentLength > BlockFileMaxBytes)
                {
                    ModelState.AddModelError(string.Empty, "Archivo demasiado grande (máx. 50 MB) para el bloque " + key + ".");
                    continue;
                }
                var ext = Path.GetExtension(posted.FileName);
                if (string.IsNullOrEmpty(ext))
                {
                    ModelState.AddModelError(string.Empty, "Extensión no válida en archivo para " + key + ".");
                    continue;
                }
                ext = ext.ToLowerInvariant();
                var allowed = AllowedExtensionsForBlockKey(key);
                if (allowed.Length == 0 || !allowed.Contains(ext))
                {
                    ModelState.AddModelError(string.Empty,
                        "Solo " + AllowedExtensionsHumanLabel(key) + " para el bloque " + key + ".");
                }
            }
        }

        private void MergeArticleBlockFiles(DAL.Tsql_Master_Articles article, HttpRequestBase request)
        {
            foreach (var key in BlockLinkKeys)
            {
                var clearKey = "clear_" + key;
                if (string.Equals(request.Form[clearKey], "true", StringComparison.OrdinalIgnoreCase))
                {
                    var previousClear = GetBlockVirtualPathForSlot(article, key);
                    SetArticleLink(article, key, null);
                    TryDeletePhysicalBlockFileIfOwned(previousClear);
                    if (BlockLinkKeysDwgOnly.Contains(key))
                    {
                        TryDeleteViewerDxfSidecar(previousClear, article.IdObject);
                    }
                    continue;
                }

                var posted = request.Files["blockFile_" + key];
                if (!HasPostedNonEmptyFile(posted))
                {
                    continue;
                }

                var previous = GetBlockVirtualPathForSlot(article, key);
                if (!TrySaveBlockFile(posted, BlockFileMaxBytes, AllowedExtensionsForBlockKey(key), out var virtualPath, out var error))
                {
                    ModelState.AddModelError(string.Empty, error);
                    continue;
                }

                SetArticleLink(article, key, virtualPath);
                TryDeletePhysicalBlockFileIfOwned(previous, virtualPath);
                if (virtualPath.EndsWith(".dwg", StringComparison.OrdinalIgnoreCase))
                {
                    TryDeleteViewerDxfSidecar(virtualPath, article.IdObject);
                }
            }
        }

        /// <summary>Borra el DXF auxiliar legado <c>*.dwg.viewer.dxf</c> junto al DWG indicado (si existe).</summary>
        private void TryDeleteViewerDxfSidecar(string dwgVirtualPath, long articleId)
        {
            if (string.IsNullOrWhiteSpace(dwgVirtualPath))
            {
                return;
            }
            if (!MasterArticleViewerDxfConverter.TryMapAppRelativeDwgToPhysical(Server, articleId, dwgVirtualPath, out var phys, out _))
            {
                return;
            }
            var sidecar = MasterArticleViewerDxfConverter.GetViewerDxfPath(phys);
            try
            {
                if (System.IO.File.Exists(sidecar))
                {
                    System.IO.File.Delete(sidecar);
                }
            }
            catch
            {
                // ignore
            }
        }

        /// <summary>
        /// Borra en disco un adjunto previo solo si está bajo ~/Files/MasterArticles/blocks/.
        /// </summary>
        private void TryDeletePhysicalBlockFileIfOwned(string previousVirtual, string newVirtual = null)
        {
            if (string.IsNullOrWhiteSpace(previousVirtual))
            {
                return;
            }
            var prev = previousVirtual.Trim();
            var nv = (newVirtual ?? string.Empty).Trim();
            if (nv.Length > 0 && string.Equals(prev, nv, StringComparison.OrdinalIgnoreCase))
            {
                return;
            }
            var blocksRoot = Path.GetFullPath(Server.MapPath("~/Files/MasterArticles/blocks"));
            string physical;
            try
            {
                var norm = NormalizeAppRelativeVirtualPath(prev);
                physical = string.IsNullOrEmpty(norm) ? null : Path.GetFullPath(Server.MapPath(norm));
            }
            catch
            {
                return;
            }
            if (string.IsNullOrEmpty(physical) || !physical.StartsWith(blocksRoot + Path.DirectorySeparatorChar, StringComparison.OrdinalIgnoreCase))
            {
                return;
            }
            try
            {
                if (System.IO.File.Exists(physical))
                {
                    System.IO.File.Delete(physical);
                }
            }
            catch
            {
                // no bloquear guardado si falla limpieza
            }
        }

        private static void SetArticleLink(DAL.Tsql_Master_Articles article, string key, string value)
        {
            switch (key)
            {
                case "LinkBlockDwgPlant3D": article.LinkBlockDwgPlant3D = value; break;
                case "LinkBlockDwgVerticalElevation3D": article.LinkBlockDwgVerticalElevation3D = value; break;
                case "LinkBlockDwgHorizontalElevation3D": article.LinkBlockDwgHorizontalElevation3D = value; break;
                case "LinkBlockDwgPlantMckUp": article.LinkBlockDwgPlantMckUp = value; break;
                case "LinkBlockDwgVerticalElevationMockUp": article.LinkBlockDwgVerticalElevationMockUp = value; break;
                case "LinkBlockDwgHorizontalElevationMockUp": article.LinkBlockDwgHorizontalElevationMockUp = value; break;
                case "LinkBlockDwgPlantStl": article.LinkBlockDwgPlantStl = value; break;
                case "LinkBlockDwgVerticalElevationStl": article.LinkBlockDwgVerticalElevationStl = value; break;
                case "LinkBlockDwgHorizontalElevationStl": article.LinkBlockDwgHorizontalElevationStl = value; break;
            }
        }

        private bool TrySaveBlockFile(HttpPostedFileBase file, long maxBytes, string[] allowedLowercaseExtensions, out string virtualPath, out string error)
        {
            virtualPath = null;
            error = null;
            if (!HasPostedNonEmptyFile(file))
            {
                error = "Archivo vacío.";
                return false;
            }
            if (file.ContentLength > 0 && file.ContentLength > maxBytes)
            {
                error = "El archivo supera el tamaño máximo permitido (50 MB).";
                return false;
            }

            var ext = Path.GetExtension(file.FileName);
            if (string.IsNullOrEmpty(ext))
            {
                error = "El archivo no tiene extensión. Se permiten: " + string.Join(", ", allowedLowercaseExtensions) + ".";
                return false;
            }
            ext = ext.ToLowerInvariant();
            if (allowedLowercaseExtensions == null || allowedLowercaseExtensions.Length == 0 || !allowedLowercaseExtensions.Contains(ext))
            {
                error = "Solo se permiten archivos " + string.Join(", ", allowedLowercaseExtensions) + " para este campo.";
                return false;
            }

            var folderRel = "~/Files/MasterArticles/blocks/";
            var folderPhysical = Server.MapPath(folderRel);
            if (!Directory.Exists(folderPhysical))
            {
                Directory.CreateDirectory(folderPhysical);
            }

            // Mismo nombre que el adjunto (p. ej. para insertar en ZWCAD); solo se sanitizan caracteres no válidos en Windows.
            var fileName = Path.GetFileName(file.FileName);
            if (string.IsNullOrWhiteSpace(fileName))
            {
                fileName = "archivo" + ext;
            }
            foreach (var c in Path.GetInvalidFileNameChars())
            {
                fileName = fileName.Replace(c, '_');
            }
            if (!allowedLowercaseExtensions.Contains(Path.GetExtension(fileName).ToLowerInvariant()))
            {
                fileName = Path.GetFileNameWithoutExtension(fileName) + ext;
            }
            if (string.IsNullOrWhiteSpace(Path.GetFileNameWithoutExtension(fileName)))
            {
                fileName = "archivo" + ext;
            }
            if (fileName.IndexOfAny(new[] { '/', '\\' }) >= 0)
            {
                error = "Nombre de archivo no válido.";
                return false;
            }

            var fullPath = Path.Combine(folderPhysical, fileName);
            try
            {
                if (System.IO.File.Exists(fullPath))
                {
                    System.IO.File.Delete(fullPath);
                }
            }
            catch (Exception ex)
            {
                error = "No se pudo sobrescribir el archivo existente: " + ex.Message;
                return false;
            }

            file.SaveAs(fullPath);
            try
            {
                var written = new FileInfo(fullPath).Length;
                if (written > maxBytes)
                {
                    try
                    {
                        System.IO.File.Delete(fullPath);
                    }
                    catch
                    {
                        // ignore
                    }
                    error = "El archivo supera el tamaño máximo permitido (50 MB).";
                    return false;
                }
            }
            catch
            {
                // si no se puede comprobar tamaño, continuar
            }

            virtualPath = folderRel + fileName;
            return true;
        }

        private static string AttachmentViewerKind(string virtualPath, string defaultWhenPresent)
        {
            if (string.IsNullOrWhiteSpace(virtualPath))
            {
                return "none";
            }
            var ext = Path.GetExtension(virtualPath).ToLowerInvariant();
            switch (ext)
            {
                case ".3ds":
                    return "none";
                case ".stl":
                    return "stl";
                case ".dwg":
                    return "dwg";
                default:
                    return defaultWhenPresent;
            }
        }

        private static IReadOnlyList<MasterArticleAttachmentSlot> BuildMasterArticleAttachmentSlots(DAL.Tsql_Master_Articles a)
        {
            var list = new List<MasterArticleAttachmentSlot>();
            void slot(string slotKey, string label, string path, string defaultKind)
            {
                var p = string.IsNullOrWhiteSpace(path) ? null : path.Trim();
                var kind = p == null ? "none" : AttachmentViewerKind(p, defaultKind);
                list.Add(new MasterArticleAttachmentSlot { SlotKey = slotKey, Label = label, VirtualPath = p, ViewerKind = kind });
            }
            slot("LinkBlockDwgPlant3D", "Planta 3D", a.LinkBlockDwgPlant3D, "dwg");
            slot("LinkBlockDwgVerticalElevation3D", "Elevación vertical 3D", a.LinkBlockDwgVerticalElevation3D, "dwg");
            slot("LinkBlockDwgHorizontalElevation3D", "Elevación horizontal 3D", a.LinkBlockDwgHorizontalElevation3D, "dwg");
            slot("LinkBlockDwgPlantMckUp", "Planta mock-up", a.LinkBlockDwgPlantMckUp, "dwg");
            slot("LinkBlockDwgVerticalElevationMockUp", "Elevación vertical mock-up", a.LinkBlockDwgVerticalElevationMockUp, "dwg");
            slot("LinkBlockDwgHorizontalElevationMockUp", "Elevación horizontal mock-up", a.LinkBlockDwgHorizontalElevationMockUp, "dwg");
            slot("LinkBlockDwgPlantStl", "Planta STL", a.LinkBlockDwgPlantStl, "stl");
            slot("LinkBlockDwgVerticalElevationStl", "Elevación vertical STL", a.LinkBlockDwgVerticalElevationStl, "stl");
            slot("LinkBlockDwgHorizontalElevationStl", "Elevación horizontal STL", a.LinkBlockDwgHorizontalElevationStl, "stl");
            return list;
        }

        private void EnrichAttachmentSlotsWithSiblingDxf(List<MasterArticleAttachmentSlot> slots, long articleId)
        {
            foreach (var s in slots)
            {
                if (s.ViewerKind != "dwg" || string.IsNullOrWhiteSpace(s.VirtualPath))
                {
                    continue;
                }
                var dwgV = s.VirtualPath.Trim();
                s.SiblingDxfVirtualPath = Path.ChangeExtension(dwgV, ".dxf");
                s.SiblingDxfExists = false;
                if (MasterArticleViewerDxfConverter.TryMapAppRelativeDwgToPhysical(Server, articleId, dwgV, out var physDwg, out _)
                    && System.IO.File.Exists(MasterArticleViewerDxfConverter.GetSiblingPreviewDxfPhysicalPath(physDwg)))
                {
                    s.SiblingDxfExists = true;
                }
            }
        }

        private void EnrichAttachmentSlotsWithStlPreview(List<MasterArticleAttachmentSlot> slots, long articleId)
        {
            foreach (var s in slots)
            {
                s.StlPreviewVirtualPath = null;
                s.StlPreviewExists = false;
                if (s.ViewerKind == "dwg" && !string.IsNullOrWhiteSpace(s.VirtualPath))
                {
                    var dwgV = s.VirtualPath.Trim();
                    s.StlPreviewVirtualPath = Path.ChangeExtension(dwgV, ".stl");
                    if (MasterArticleViewerDxfConverter.TryMapAppRelativeDwgToPhysical(Server, articleId, dwgV, out var physDwg, out _))
                    {
                        var physStl = Path.ChangeExtension(physDwg, ".stl");
                        s.StlPreviewExists = System.IO.File.Exists(physStl);
                    }
                }
                else if (s.ViewerKind == "stl" && !string.IsNullOrWhiteSpace(s.VirtualPath))
                {
                    var stlV = s.VirtualPath.Trim();
                    s.StlPreviewVirtualPath = stlV;
                    s.StlPreviewExists = MasterArticleViewerDxfConverter.TryMapAppRelativeStlToPhysical(Server, articleId, stlV, out _, out _);
                }
            }
        }

        private void PopulateMasterArticleStlPreviewViewData(DAL.Tsql_Master_Articles sourceForSlots, long articleId)
        {
            var slots = BuildMasterArticleAttachmentSlots(sourceForSlots).ToList();
            EnrichAttachmentSlotsWithStlPreview(slots, articleId);
            ViewData["MasterArticleStlPreview"] = new MasterArticleStlPreviewSectionModel { AttachmentSlots = slots };
        }

        private string ArticleLinkCellHtml(string storedPath)
        {
            if (string.IsNullOrWhiteSpace(storedPath))
            {
                return "";
            }
            var href = Url.Content(storedPath.StartsWith("~/", StringComparison.Ordinal) ? storedPath : "~/" + storedPath.TrimStart('/'));
            var safeHref = System.Web.HttpUtility.HtmlAttributeEncode(href);
            return "<a class=\"btn btn-sm btn-outline-primary\" target=\"_blank\" rel=\"noopener\" href=\"" + safeHref + "\" title=\"Abrir\"><i class=\"fas fa-paperclip\" aria-hidden=\"true\"></i></a>";
        }

        [HttpGet]
        public async Task<JsonResult> ApsAccessToken()
        {
            if (!AutodeskApsClient.IsConfigured)
            {
                return Json(new { error = "aps_not_configured" }, JsonRequestBehavior.AllowGet);
            }
            try
            {
                var (token, exp) = await AutodeskApsClient.GetTwoLeggedTokenAsync(CancellationToken.None).ConfigureAwait(false);
                return Json(new { access_token = token, expires_in = exp }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { error = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpGet]
        public async Task<JsonResult> ApsDwgViewerPayload(long id, string slotKey)
        {
            if (!AutodeskApsClient.IsConfigured)
            {
                return Json(new { ok = false, error = "Autodesk APS no está configurado (ClientId y ClientSecret en Web.config)." }, JsonRequestBehavior.AllowGet);
            }
            if (string.IsNullOrWhiteSpace(slotKey) || !BlockLinkKeysDwgOnly.Contains(slotKey))
            {
                return Json(new { ok = false, error = "Parámetro slotKey no válido." }, JsonRequestBehavior.AllowGet);
            }
            if (!TryResolveMasterArticleDwgPhysicalPath(id, slotKey, out var physicalPath, out var resolveError))
            {
                return Json(new { ok = false, error = resolveError }, JsonRequestBehavior.AllowGet);
            }
            try
            {
                var urn = await AutodeskApsClient.GetOrCreateViewerUrnAsync(physicalPath, CancellationToken.None).ConfigureAwait(false);
                return Json(new { ok = true, urn }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { ok = false, error = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        private bool TryResolveMasterArticleDwgPhysicalPath(long articleId, string slotKey, out string physicalPath, out string error)
        {
            physicalPath = null;
            error = null;
            var article = db.Tsql_Master_Articles.FirstOrDefault(a => a.IdObject == articleId);
            if (article == null)
            {
                error = "Artículo no encontrado.";
                return false;
            }
            var virtualPath = GetBlockVirtualPathForSlot(article, slotKey);
            if (string.IsNullOrWhiteSpace(virtualPath))
            {
                error = "No hay archivo DWG en este bloque.";
                return false;
            }
            return MasterArticleViewerDxfConverter.TryMapAppRelativeDwgToPhysical(Server, articleId, virtualPath, out physicalPath, out error);
        }

        private static string GetBlockVirtualPathForSlot(DAL.Tsql_Master_Articles a, string slotKey)
        {
            switch (slotKey)
            {
                case "LinkBlockDwgPlant3D": return a.LinkBlockDwgPlant3D;
                case "LinkBlockDwgVerticalElevation3D": return a.LinkBlockDwgVerticalElevation3D;
                case "LinkBlockDwgHorizontalElevation3D": return a.LinkBlockDwgHorizontalElevation3D;
                case "LinkBlockDwgPlantMckUp": return a.LinkBlockDwgPlantMckUp;
                case "LinkBlockDwgVerticalElevationMockUp": return a.LinkBlockDwgVerticalElevationMockUp;
                case "LinkBlockDwgHorizontalElevationMockUp": return a.LinkBlockDwgHorizontalElevationMockUp;
                case "LinkBlockDwgPlantStl": return a.LinkBlockDwgPlantStl;
                case "LinkBlockDwgVerticalElevationStl": return a.LinkBlockDwgVerticalElevationStl;
                case "LinkBlockDwgHorizontalElevationStl": return a.LinkBlockDwgHorizontalElevationStl;
                default: return null;
            }
        }

        private long[] AllowedLinkSystemIds()
        {
            return (from s in db.TSql_System
                    join c in db.TSql_Company on s.LinkCompany equals c.SysObjectID
                    where s.AddIsActive && !c.BitIsDeleted
                    select s.IdObject).ToArray();
        }

        private void PopulateLinkSystem(long? selectedId)
        {
            var systems =
                from s in db.TSql_System
                join c in db.TSql_Company on s.LinkCompany equals c.SysObjectID
                where s.AddIsActive && !c.BitIsDeleted
                orderby c.TextLabel, s.TextLabel
                select new { s.IdObject, Label = c.TextLabel + " — " + s.TextLabel };
            // No usar ViewBag.LinkSystem: colisiona con ViewData["LinkSystem"] (propiedad del modelo) y rompe DropDownListFor.
            ViewBag.ArticleLinkSystemSelectList = new SelectList(systems.ToList(), "IdObject", "Label", selectedId);
        }

        public JsonResult ListMasterArticles([ModelBinder(typeof(DataTablesBinder))] IDataTablesRequest requestModel)
        {
            try
            {
                IQueryable<ListMasterArticle> query = from masterArticles in db.Tsql_Master_Articles
                                                      join system in db.TSql_System on masterArticles.LinkSystem equals system.IdObject
                                                      join company in db.TSql_Company on system.LinkCompany equals company.SysObjectID
                                                      select new ListMasterArticle
                                                      {
                                                          IdObject = masterArticles.IdObject,
                                                          CompanyTextLabel = company.TextLabel,
                                                          System_TextLabel = system.TextLabel,
                                                          TextCode = masterArticles.TextCode,
                                                          TextLabel = masterArticles.TextLabel,
                                                          NumberHigh = masterArticles.NumberHigh,
                                                          NumberWidth = masterArticles.NumberWidth,
                                                          NumberLong = masterArticles.NumberLong,
                                                          NumberWeight = masterArticles.NumberWeight,
                                                          NumberMts2 = masterArticles.NumberMts2,
                                                          NumberMts3 = masterArticles.NumberMts3,
                                                          TextBlockNumber = masterArticles.TextBlockNumber,
                                                          TextStlNumber = masterArticles.TextStlNumber,
                                                          TextColor1 = masterArticles.TextColor1,
                                                          TextColor2 = masterArticles.TextColor2,
                                                          AddChangeBy = masterArticles.AddLastDateChange,
                                                          AddIsActive = masterArticles.AddIsActive,
                                                          LinkBlockDwgPlant3D = masterArticles.LinkBlockDwgPlant3D,
                                                          LinkBlockDwgVerticalElevation3D = masterArticles.LinkBlockDwgVerticalElevation3D,
                                                          LinkBlockDwgHorizontalElevation3D = masterArticles.LinkBlockDwgHorizontalElevation3D,
                                                          LinkBlockDwgPlantMckUp = masterArticles.LinkBlockDwgPlantMckUp,
                                                          LinkBlockDwgVerticalElevationMockUp = masterArticles.LinkBlockDwgVerticalElevationMockUp,
                                                          LinkBlockDwgHorizontalElevationMockUp = masterArticles.LinkBlockDwgHorizontalElevationMockUp,
                                                          LinkBlockDwgPlantStl = masterArticles.LinkBlockDwgPlantStl,
                                                          LinkBlockDwgVerticalElevationStl = masterArticles.LinkBlockDwgVerticalElevationStl,
                                                          LinkBlockDwgHorizontalElevationStl = masterArticles.LinkBlockDwgHorizontalElevationStl,
                                                          IInsertinMaterArticles = masterArticles.IInsertinMaterArticles,
                                                      };

                var totalCount = query.Count();

                if (requestModel.Search.Value != String.Empty)
                {
                    var value = requestModel.Search.Value.Trim();
                    query = query.Where(p => p.CompanyTextLabel.Contains(value) ||
                                             p.System_TextLabel.Contains(value) ||
                                             p.TextCode.Contains(value) ||
                                             p.TextLabel.Contains(value) ||
                                             (p.NumberHigh != null && p.NumberHigh.ToString().Contains(value)) ||
                                             (p.NumberWidth != null && p.NumberWidth.ToString().Contains(value)) ||
                                             (p.NumberLong != null && p.NumberLong.ToString().Contains(value)) ||
                                             (p.NumberWeight != null && p.NumberWeight.ToString().Contains(value)) ||
                                             (p.NumberMts2 != null && p.NumberMts2.ToString().Contains(value)) ||
                                             (p.NumberMts3 != null && p.NumberMts3.ToString().Contains(value)) ||
                                             (p.TextBlockNumber != null && p.TextBlockNumber.Contains(value)) ||
                                             (p.TextStlNumber != null && p.TextStlNumber.Contains(value)) ||
                                             (p.TextColor1 != null && p.TextColor1.Contains(value)) ||
                                             (p.TextColor2 != null && p.TextColor2.Contains(value)) ||
                                             p.AddChangeBy.ToString().Contains(value) ||
                                             p.AddIsActive.ToString().Contains(value) ||
                                             (p.LinkBlockDwgPlant3D != null && p.LinkBlockDwgPlant3D.Contains(value)) ||
                                             (p.LinkBlockDwgVerticalElevation3D != null && p.LinkBlockDwgVerticalElevation3D.Contains(value)) ||
                                             (p.LinkBlockDwgHorizontalElevation3D != null && p.LinkBlockDwgHorizontalElevation3D.Contains(value)) ||
                                             (p.LinkBlockDwgPlantMckUp != null && p.LinkBlockDwgPlantMckUp.Contains(value)) ||
                                             (p.LinkBlockDwgVerticalElevationMockUp != null && p.LinkBlockDwgVerticalElevationMockUp.Contains(value)) ||
                                             (p.LinkBlockDwgHorizontalElevationMockUp != null && p.LinkBlockDwgHorizontalElevationMockUp.Contains(value)) ||
                                             (p.LinkBlockDwgPlantStl != null && p.LinkBlockDwgPlantStl.Contains(value)) ||
                                             (p.LinkBlockDwgVerticalElevationStl != null && p.LinkBlockDwgVerticalElevationStl.Contains(value)) ||
                                             (p.LinkBlockDwgHorizontalElevationStl != null && p.LinkBlockDwgHorizontalElevationStl.Contains(value)) ||
                                             p.IInsertinMaterArticles.ToString().Contains(value)
                    );
                }

                var filteredCount = query.Count();

                var sortedColumns = requestModel.Columns.GetSortedColumns();
                var orderByString = String.Empty;
                string orderColumn = "";
                foreach (var column in sortedColumns)
                {
                    switch (column.Data)
                    {
                        case "CompanyTextLabel": orderColumn = "CompanyTextLabel"; break;
                        case "System_TextLabel": orderColumn = "System_TextLabel"; break;
                        case "TextCode": orderColumn = "TextCode"; break;
                        case "TextLabel": orderColumn = "TextLabel"; break;
                        case "NumberHigh": orderColumn = "NumberHigh"; break;
                        case "NumberWidth": orderColumn = "NumberWidth"; break;
                        case "NumberLong": orderColumn = "NumberLong"; break;
                        case "NumberWeight": orderColumn = "NumberWeight"; break;
                        case "NumberMts2": orderColumn = "NumberMts2"; break;
                        case "NumberMts3": orderColumn = "NumberMts3"; break;
                        case "TextBlockNumber": orderColumn = "TextBlockNumber"; break;
                        case "TextStlNumber": orderColumn = "TextStlNumber"; break;
                        case "TextColor1": orderColumn = "TextColor1"; break;
                        case "TextColor2": orderColumn = "TextColor2"; break;
                        case "AddChangeBy": orderColumn = "AddChangeBy"; break;
                        case "AddIsActive": orderColumn = "AddIsActive"; break;
                        case "LinkBlockDwgPlant3D": orderColumn = "LinkBlockDwgPlant3D"; break;
                        case "LinkBlockDwgVerticalElevation3D": orderColumn = "LinkBlockDwgVerticalElevation3D"; break;
                        case "LinkBlockDwgHorizontalElevation3D": orderColumn = "LinkBlockDwgHorizontalElevation3D"; break;
                        case "LinkBlockDwgPlantMckUp": orderColumn = "LinkBlockDwgPlantMckUp"; break;
                        case "LinkBlockDwgVerticalElevationMockUp": orderColumn = "LinkBlockDwgVerticalElevationMockUp"; break;
                        case "LinkBlockDwgHorizontalElevationMockUp": orderColumn = "LinkBlockDwgHorizontalElevationMockUp"; break;
                        case "LinkBlockDwgPlantStl": orderColumn = "LinkBlockDwgPlantStl"; break;
                        case "LinkBlockDwgVerticalElevationStl": orderColumn = "LinkBlockDwgVerticalElevationStl"; break;
                        case "LinkBlockDwgHorizontalElevationStl": orderColumn = "LinkBlockDwgHorizontalElevationStl"; break;
                        case "IInsertinMaterArticles": orderColumn = "IInsertinMaterArticles"; break;
                        default: orderColumn = "CompanyTextLabel"; break;
                    }
                    orderByString += orderByString != String.Empty ? "," : "";
                    orderByString += orderColumn + (column.SortDirection == Column.OrderDirection.Ascendant ? " asc" : " desc");
                }
                query = query.OrderBy(orderByString == String.Empty ? "TextLabel asc" : orderByString);
                query = query.ApplyDataTablesPaging(requestModel.Start, requestModel.Length);

                bool allowEdit = true;
                bool allowDelete = true;
                var data = query.ToList().Select(p =>
                {
                    var btnDetails = "<a title='Detalles del artículo' href='" + Url.Action("Details", "MasterArticles", new { id = p.IdObject }) + "' class=\"btn btn-info btn-xs\"><span class=\"fas fa-file-alt\" aria-hidden=\"true\"></span></a>";
                    var btnEdit = "<a title='Editar artículo' href='" + Url.Action("Edit", "MasterArticles", new { id = p.IdObject }) + "' class=\"btn btn-warning btn-xs\"><span class=\"fas fa-edit\" aria-hidden=\"true\"></span></a>";
                    var btnDelete = "<a title='Eliminar artículo' href='" + Url.Action("Delete", "MasterArticles", new { id = p.IdObject }) + "' class=\"btn btn-danger btn-xs\" onclick=\"return confirm('¿Eliminar este artículo?');\"><span class=\"fas fa-trash-alt\" aria-hidden=\"true\"></span></a>";
                    var btnToggle = p.AddIsActive
                        ? "<a title='Desactivar artículo' href='" + Url.Action("SetArticleActive", "MasterArticles", new { id = p.IdObject, active = false }) + "' class=\"btn btn-secondary btn-xs\" onclick=\"return confirm('¿Desactivar este artículo? Seguirá en la base de datos pero no se usará como activo.');\"><span class=\"fas fa-toggle-on\" aria-hidden=\"true\"></span></a>"
                        : "<a title='Activar artículo' href='" + Url.Action("SetArticleActive", "MasterArticles", new { id = p.IdObject, active = true }) + "' class=\"btn btn-success btn-xs\" onclick=\"return confirm('¿Activar este artículo?');\"><span class=\"fas fa-toggle-off\" aria-hidden=\"true\"></span></a>";
                    var actionsHtml = allowEdit ? (btnDetails + "&nbsp;" + btnEdit + "&nbsp;" + btnDelete + "&nbsp;" + btnToggle) : string.Empty;
                    return new
                    {
                        emptyColumn = actionsHtml,
                        SysObjectID = p.IdObject,
                        CompanyTextLabel = p.CompanyTextLabel,
                        System_TextLabel = p.System_TextLabel,
                        TextCode = p.TextCode,
                        TextLabel = p.TextLabel,
                        NumberHigh = p.NumberHigh,
                        NumberWidth = p.NumberWidth,
                        NumberLong = p.NumberLong,
                        NumberWeight = p.NumberWeight,
                        NumberMts2 = p.NumberMts2,
                        NumberMts3 = p.NumberMts3,
                        TextBlockNumber = p.TextBlockNumber,
                        TextStlNumber = p.TextStlNumber,
                        TextColor1 = p.TextColor1,
                        TextColor2 = p.TextColor2,
                        AddChangeBy = p.AddChangeBy,
                        AddIsActive = p.AddIsActive,
                        LinkBlockDwgPlant3D = ArticleLinkCellHtml(p.LinkBlockDwgPlant3D),
                        LinkBlockDwgVerticalElevation3D = ArticleLinkCellHtml(p.LinkBlockDwgVerticalElevation3D),
                        LinkBlockDwgHorizontalElevation3D = ArticleLinkCellHtml(p.LinkBlockDwgHorizontalElevation3D),
                        LinkBlockDwgPlantMckUp = ArticleLinkCellHtml(p.LinkBlockDwgPlantMckUp),
                        LinkBlockDwgVerticalElevationMockUp = ArticleLinkCellHtml(p.LinkBlockDwgVerticalElevationMockUp),
                        LinkBlockDwgHorizontalElevationMockUp = ArticleLinkCellHtml(p.LinkBlockDwgHorizontalElevationMockUp),
                        LinkBlockDwgPlantStl = ArticleLinkCellHtml(p.LinkBlockDwgPlantStl),
                        LinkBlockDwgVerticalElevationStl = ArticleLinkCellHtml(p.LinkBlockDwgVerticalElevationStl),
                        LinkBlockDwgHorizontalElevationStl = ArticleLinkCellHtml(p.LinkBlockDwgHorizontalElevationStl),
                        IInsertinMaterArticles = p.IInsertinMaterArticles ? "<span class=\"badge bg-label-success\">Sí</span>" : "<span class=\"badge bg-label-secondary\">No</span>",
                        allowEdit = allowEdit,
                        allowDelete = allowDelete
                    };
                }).ToList();
                return Json(new DataTablesResponse(requestModel.Draw, data, filteredCount, totalCount), JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                var draw = requestModel != null ? requestModel.Draw : 0;
                return Json(new
                {
                    draw,
                    recordsTotal = 0,
                    recordsFiltered = 0,
                    data = new object[0],
                    error = ex.Message
                }, JsonRequestBehavior.AllowGet);
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
