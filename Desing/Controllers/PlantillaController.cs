using DAL;
using DataTables.Mvc;
using Desing.Helpers;
using Desing.Models;
using Microsoft.AspNet.Identity;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Data.SqlClient;
using System.Web;
using System.Web.Mvc;

namespace Desing.Controllers
{
    /// <summary>
    /// Gestion de plantillas de estilo (color + logo) por usuario.
    /// La plantilla se asigna por empresa (TSql_Company.LinPlantilla); todos los empleados de esa empresa la heredan.
    /// La plantilla marcada como "por defecto" se asigna automaticamente al crear empleado.
    /// </summary>
    [Authorize]
    public class PlantillaController : BaseController
    {
        // ---------------------------------------------------------------------
        // INDEX + DataTable
        // ---------------------------------------------------------------------
        public ActionResult Index()
        {
            return View();
        }

        [OutputCache(Duration = 1)]
        public JsonResult ListPlantilla([ModelBinder(typeof(DataTablesBinder))] IDataTablesRequest requestModel)
        {
            try
            {
                IQueryable<PlantillaListItem> query = db.TSql_Plantilla
                    .Where(p => !p.AttIsDeleted)
                    .Select(p => new PlantillaListItem
                    {
                        SysObjectID = p.SysObjectID,
                        AttName = p.AttName,
                        AttColor = p.AttColor,
                        AttLogo = p.AttLogo,
                        AttIsDefault = p.AttIsDefault,
                        AttCreated = p.AttCreated
                    });

                var totalCount = query.Count();

                if (!string.IsNullOrEmpty(requestModel.Search.Value))
                {
                    var value = requestModel.Search.Value.Trim();
                    query = query.Where(p => p.AttName.Contains(value) ||
                                             p.AttColor.Contains(value));
                }

                var filteredCount = query.Count();

                // Sort
                var sortedColumns = requestModel.Columns.GetSortedColumns();
                var orderByString = string.Empty;
                foreach (var column in sortedColumns)
                {
                    string orderColumn;
                    switch (column.Data)
                    {
                        case "AttName": orderColumn = "AttName"; break;
                        case "AttColor": orderColumn = "AttColor"; break;
                        case "AttIsDefault": orderColumn = "AttIsDefault"; break;
                        case "AttCreated": orderColumn = "AttCreated"; break;
                        default: orderColumn = "AttName"; break;
                    }
                    orderByString += orderByString != string.Empty ? "," : "";
                    orderByString += orderColumn +
                        (column.SortDirection == Column.OrderDirection.Ascendant ? " asc" : " desc");
                }
                query = query.OrderBy(string.IsNullOrEmpty(orderByString) ? "AttIsDefault desc, AttName asc" : orderByString);

                query = query.ApplyDataTablesPaging(requestModel.Start, requestModel.Length);

                var data = query.ToList().Select(p => new
                {
                    SysObjectID = p.SysObjectID,
                    AttName = p.AttName,
                    AttColor = p.AttColor,
                    AttLogo = p.AttLogo,
                    AttIsDefault = p.AttIsDefault,
                    AttCreated = p.AttCreated.ToShortDateString(),
                    colorBadge = "<span style=\"display:inline-block;width:22px;height:22px;border-radius:4px;border:1px solid #ccc;vertical-align:middle;background:" + (p.AttColor ?? "#349d7d") + "\"></span> <code>" + (p.AttColor ?? "") + "</code>",
                    logoPreview = "<img src=\"" + Url.Content("~" + (p.AttLogo ?? "/Content/images/Login/at.png")) + "\" style=\"height:24px\" />",
                    defaultBadge = p.AttIsDefault
                        ? "<span class=\"badge bg-label-success\">Por defecto</span>"
                        : "",
                    buttonEdit = "<a title='Editar plantilla' href='" + Url.Action("Edit", "Plantilla", new { Id = p.SysObjectID }) + "' class=\"btn btn-warning btn-xs\"><span class=\"fas fa-edit\"></span></a>",
                    buttonDelete = p.AttIsDefault
                        ? ""
                        : "<a title='Eliminar plantilla' href='" + Url.Action("Delete", "Plantilla", new { Id = p.SysObjectID }) + "' class=\"btn btn-danger btn-xs\" onclick=\"return confirm('¿Eliminar esta plantilla?');\"><span class=\"fas fa-trash-alt\"></span></a>"
                }).ToList();

                return Json(new DataTablesResponse(requestModel.Draw, data, filteredCount, totalCount), JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(ex.Message);
            }
        }

        // ---------------------------------------------------------------------
        // CREATE
        // ---------------------------------------------------------------------
        public ActionResult Create()
        {
            ViewBag.MessageHeat = "Crear plantilla";
            var model = new PlantillaViewModel
            {
                AttColor = "#349d7d",
                AttLogo = "/Content/images/Login/at.png",
                AttFavicon = "/assets/client/images/Default/Ico/at.ico",
                AttIsDefault = false,
                IsEdit = false,
                AttBrandText = "T Desing.net",
                AttBrandAccentColor = "#f29100",
                AttBrandTextColor = ""
            };
            return View("Edit", model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(PlantillaViewModel model, HttpPostedFileBase logoFile, HttpPostedFileBase faviconFile)
        {
            // Si subio un archivo de logo, guardamos y sobreescribimos AttLogo antes de validar.
            string logoSaveError;
            string savedLogoPath = TrySaveLogoFile(logoFile, out logoSaveError);
            if (logoSaveError != null)
            {
                ModelState.AddModelError("AttLogo", logoSaveError);
            }
            else if (!string.IsNullOrEmpty(savedLogoPath))
            {
                model.AttLogo = savedLogoPath;
                ModelState.Remove("AttLogo");
            }

            // Favicon (opcional).
            string faviconSaveError;
            string savedFaviconPath = TrySaveFaviconFile(faviconFile, out faviconSaveError);
            if (faviconSaveError != null)
            {
                ModelState.AddModelError("AttFavicon", faviconSaveError);
            }
            else if (!string.IsNullOrEmpty(savedFaviconPath))
            {
                model.AttFavicon = savedFaviconPath;
                ModelState.Remove("AttFavicon");
            }

            if (!ModelState.IsValid)
            {
                ViewBag.MessageHeat = "Crear plantilla";
                return View("Edit", model);
            }

            string userId = User.Identity.GetUserId();
            DateTime now = DateTime.UtcNow;

            // Si se marca como por defecto, desmarcamos las anteriores.
            if (model.AttIsDefault)
            {
                foreach (var def in db.TSql_Plantilla.Where(p => p.AttIsDefault && !p.AttIsDeleted))
                {
                    def.AttIsDefault = false;
                    def.LinModifiedBy = userId;
                    def.AttLastModification = now;
                }
            }

            var plantilla = new TSql_Plantilla
            {
                AttName = model.AttName,
                AttBrandText = string.IsNullOrWhiteSpace(model.AttBrandText) ? "T Desing.net" : model.AttBrandText.Trim(),
                AttBrandTextColor = string.IsNullOrWhiteSpace(model.AttBrandTextColor) ? null : model.AttBrandTextColor.Trim(),
                AttBrandAccentColor = string.IsNullOrWhiteSpace(model.AttBrandAccentColor) ? "#f29100" : model.AttBrandAccentColor.Trim(),
                AttColor = model.AttColor,
                AttLogo = model.AttLogo,
                AttFavicon = model.AttFavicon,
                AttIsDefault = model.AttIsDefault,
                AttIsDeleted = false,
                LinCreatedBy = userId,
                AttCreated = now,
                LinModifiedBy = userId,
                AttLastModification = now,
                SysUpdateNumber = 0
            };

            db.TSql_Plantilla.Add(plantilla);
            db.SaveChanges();

            TempData["ToastType"] = "Act";
            TempData["ToastTitle"] = "Crear plantilla";
            TempData["ToastMessage"] = "Plantilla \"" + plantilla.AttName + "\" creada correctamente.";
            return RedirectToAction("Index");
        }

        // ---------------------------------------------------------------------
        // EDIT
        // ---------------------------------------------------------------------
        public ActionResult Edit(long Id)
        {
            TSql_Plantilla entity;
            try
            {
                entity = db.TSql_Plantilla.FirstOrDefault(p => p.SysObjectID == Id && !p.AttIsDeleted);
            }
            catch (Exception ex)
            {
                var handled = RedirectIfPlantillaDataModelMismatch(ex);
                if (handled != null)
                    return handled;
                throw;
            }

            if (entity == null)
            {
                TempData["ToastType"] = "Error";
                TempData["ToastTitle"] = "Editar plantilla";
                TempData["ToastMessage"] = "La plantilla no existe.";
                return RedirectToAction("Index");
            }

            ViewBag.MessageHeat = "Editar plantilla";
            var model = new PlantillaViewModel
            {
                SysObjectID = entity.SysObjectID,
                AttName = entity.AttName,
                AttBrandText = string.IsNullOrWhiteSpace(entity.AttBrandText) ? "T Desing.net" : entity.AttBrandText,
                AttBrandTextColor = entity.AttBrandTextColor ?? "",
                AttBrandAccentColor = string.IsNullOrWhiteSpace(entity.AttBrandAccentColor) ? "#f29100" : entity.AttBrandAccentColor,
                AttColor = entity.AttColor,
                AttLogo = entity.AttLogo,
                AttFavicon = string.IsNullOrWhiteSpace(entity.AttFavicon)
                    ? "/assets/client/images/Default/Ico/at.ico"
                    : entity.AttFavicon,
                AttIsDefault = entity.AttIsDefault,
                IsEdit = true
            };
            return View("Edit", model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(PlantillaViewModel model, HttpPostedFileBase logoFile, HttpPostedFileBase faviconFile)
        {
            string logoSaveError;
            string savedLogoPath = TrySaveLogoFile(logoFile, out logoSaveError);
            if (logoSaveError != null)
            {
                ModelState.AddModelError("AttLogo", logoSaveError);
            }
            else if (!string.IsNullOrEmpty(savedLogoPath))
            {
                model.AttLogo = savedLogoPath;
                ModelState.Remove("AttLogo");
            }

            string faviconSaveError;
            string savedFaviconPath = TrySaveFaviconFile(faviconFile, out faviconSaveError);
            if (faviconSaveError != null)
            {
                ModelState.AddModelError("AttFavicon", faviconSaveError);
            }
            else if (!string.IsNullOrEmpty(savedFaviconPath))
            {
                model.AttFavicon = savedFaviconPath;
                ModelState.Remove("AttFavicon");
            }

            if (!ModelState.IsValid)
            {
                ViewBag.MessageHeat = "Editar plantilla";
                return View("Edit", model);
            }

            TSql_Plantilla entity;
            try
            {
                entity = db.TSql_Plantilla.FirstOrDefault(p => p.SysObjectID == model.SysObjectID && !p.AttIsDeleted);
            }
            catch (Exception ex)
            {
                var handled = RedirectIfPlantillaDataModelMismatch(ex);
                if (handled != null)
                    return handled;
                throw;
            }

            if (entity == null)
            {
                TempData["ToastType"] = "Error";
                TempData["ToastTitle"] = "Editar plantilla";
                TempData["ToastMessage"] = "La plantilla no existe.";
                return RedirectToAction("Index");
            }

            string userId = User.Identity.GetUserId();
            DateTime now = DateTime.UtcNow;

            if (model.AttIsDefault && !entity.AttIsDefault)
            {
                foreach (var def in db.TSql_Plantilla.Where(p => p.AttIsDefault && !p.AttIsDeleted && p.SysObjectID != entity.SysObjectID))
                {
                    def.AttIsDefault = false;
                    def.LinModifiedBy = userId;
                    def.AttLastModification = now;
                }
            }

            entity.AttName = model.AttName;
            entity.AttBrandText = string.IsNullOrWhiteSpace(model.AttBrandText) ? "T Desing.net" : model.AttBrandText.Trim();
            entity.AttBrandTextColor = string.IsNullOrWhiteSpace(model.AttBrandTextColor) ? null : model.AttBrandTextColor.Trim();
            entity.AttBrandAccentColor = string.IsNullOrWhiteSpace(model.AttBrandAccentColor) ? "#f29100" : model.AttBrandAccentColor.Trim();
            entity.AttColor = model.AttColor;
            entity.AttLogo = model.AttLogo;
            entity.AttFavicon = model.AttFavicon;
            entity.AttIsDefault = model.AttIsDefault;
            entity.LinModifiedBy = userId;
            entity.AttLastModification = now;
            entity.SysUpdateNumber = entity.SysUpdateNumber + 1;
            db.SaveChanges();

            TempData["ToastType"] = "Editar";
            TempData["ToastTitle"] = "Editar plantilla";
            TempData["ToastMessage"] = "Plantilla \"" + entity.AttName + "\" actualizada.";
            return RedirectToAction("Index");
        }

        // ---------------------------------------------------------------------
        // DELETE (soft-delete; bloqueada si es la default)
        // ---------------------------------------------------------------------
        public ActionResult Delete(long Id)
        {
            var entity = db.TSql_Plantilla.FirstOrDefault(p => p.SysObjectID == Id && !p.AttIsDeleted);
            if (entity == null)
            {
                return RedirectToAction("Index");
            }
            if (entity.AttIsDefault)
            {
                TempData["ToastType"] = "Error";
                TempData["ToastTitle"] = "Eliminar plantilla";
                TempData["ToastMessage"] = "No se puede eliminar la plantilla por defecto.";
                return RedirectToAction("Index");
            }

            // Reasignar empleados que la tuvieran a la plantilla por defecto.
            long? defaultId = db.TSql_Plantilla
                .Where(p => p.AttIsDefault && !p.AttIsDeleted)
                .Select(p => (long?)p.SysObjectID)
                .FirstOrDefault();

            var afectados = db.TSql_Company.Where(c => !c.BitIsDeleted && c.LinPlantilla == Id);
            foreach (var comp in afectados)
            {
                comp.LinPlantilla = defaultId;
            }

            entity.AttIsDeleted = true;
            entity.LinModifiedBy = User.Identity.GetUserId();
            entity.AttLastModification = DateTime.UtcNow;
            db.SaveChanges();

            TempData["ToastType"] = "Act";
            TempData["ToastTitle"] = "Eliminar plantilla";
            TempData["ToastMessage"] = "Plantilla \"" + entity.AttName + "\" eliminada.";
            return RedirectToAction("Index");
        }

        // ---------------------------------------------------------------------
        // Helpers reutilizables (SelectList y plantilla por defecto).
        // ---------------------------------------------------------------------

        /// <summary>
        /// Si la base de datos no tiene las columnas de marca (AttBrand*), EF falla al
        /// materializar TSql_Plantilla. Devuelve un redirect con mensaje claro; si no
        /// aplica, devuelve null.
        /// </summary>
        private ActionResult RedirectIfPlantillaDataModelMismatch(Exception ex)
        {
            if (!IsPlantillaSchemaOrMaterializationError(ex))
                return null;

            TempData["ToastType"] = "Error";
            TempData["ToastTitle"] = "Base de datos";
            TempData["ToastMessage"] =
                "La tabla TSql_Plantilla no incluye las columnas de marca (AttBrandText, AttBrandTextColor, AttBrandAccentColor). " +
                "Ejecuta en el servidor SQL el script: App_Data/Sql/TSql_Plantilla_add_brand_text_and_colors.sql (carpeta del proyecto Desing).";
            return RedirectToAction("Index");
        }

        private static bool IsPlantillaSchemaOrMaterializationError(Exception ex)
        {
            // Captura cualquier anidamiento raro de EF + SqlException.
            try
            {
                var dump = ex.ToString();
                if (dump.IndexOf("Invalid column name", StringComparison.OrdinalIgnoreCase) >= 0 &&
                    dump.IndexOf("AttBrand", StringComparison.OrdinalIgnoreCase) >= 0)
                    return true;
            }
            catch { /* ignorar */ }

            for (var e = ex; e != null; e = e.InnerException)
            {
                var m = e.Message ?? "";
                // SQL Server (EN): Invalid column name 'AttBrandText' ...
                if (m.IndexOf("Invalid column name", StringComparison.OrdinalIgnoreCase) >= 0 &&
                    m.IndexOf("AttBrand", StringComparison.OrdinalIgnoreCase) >= 0)
                    return true;
                // SQL Server (ES, aprox.): ... nombre de columna 'AttBrandText' ...
                if (m.IndexOf("AttBrand", StringComparison.OrdinalIgnoreCase) >= 0 &&
                    m.IndexOf("columna", StringComparison.OrdinalIgnoreCase) >= 0 &&
                    (m.IndexOf("válid", StringComparison.OrdinalIgnoreCase) >= 0 || m.IndexOf("valido", StringComparison.OrdinalIgnoreCase) >= 0))
                    return true;
                // 207 = columna invalida (mensaje puede estar localizado)
                var sql = e as SqlException;
                if (sql != null && sql.Number == 207)
                {
                    for (var i = 0; i < sql.Errors.Count; i++)
                    {
                        var em = sql.Errors[i].Message ?? "";
                        if (em.IndexOf("AttBrand", StringComparison.OrdinalIgnoreCase) >= 0)
                            return true;
                    }
                }
                // EF: propiedad no-nullable recibe NULL desde la BD
                if (m.IndexOf("AttBrand", StringComparison.OrdinalIgnoreCase) >= 0 &&
                    m.IndexOf("could not be set", StringComparison.OrdinalIgnoreCase) >= 0)
                    return true;
            }

            return false;
        }

        public static IEnumerable<SelectListItem> GetSelectList(ConexionData db, long? selected)
        {
            return db.TSql_Plantilla
                .Where(p => !p.AttIsDeleted)
                .OrderByDescending(p => p.AttIsDefault)
                .ThenBy(p => p.AttName)
                .ToList()
                .Select(p => new SelectListItem
                {
                    Value = p.SysObjectID.ToString(),
                    Text = p.AttName + (p.AttIsDefault ? " (por defecto)" : ""),
                    Selected = selected.HasValue && selected.Value == p.SysObjectID
                })
                .ToList();
        }

        public static long? GetDefaultPlantillaId(ConexionData db)
        {
            return db.TSql_Plantilla
                .Where(p => p.AttIsDefault && !p.AttIsDeleted)
                .Select(p => (long?)p.SysObjectID)
                .FirstOrDefault();
        }

        // ---------------------------------------------------------------------
        // Subida de logo desde el cliente.
        // ---------------------------------------------------------------------
        private static readonly string[] AllowedLogoExtensions =
            new[] { ".png", ".jpg", ".jpeg", ".gif", ".svg", ".webp", ".ico" };

        private const long MaxLogoSizeBytes = 2 * 1024 * 1024; // 2 MB

        // Limites maximos del logo despues de redimensionar (proporcional).
        // Si el archivo original ya es mas pequeno, se deja como esta.
        private const int MaxLogoWidth = 600;
        private const int MaxLogoHeight = 200;

        /// <summary>
        /// Si el usuario subio un archivo, lo valida (extension/tamano) y lo guarda en
        /// ~/Files/Plantilla/ con un nombre unico. Devuelve la ruta web relativa
        /// (ej: "/Files/Plantilla/logo_20260510_153012_abc123.png") o null si no se subio nada.
        /// Si hay error, lo expone via parametro 'error'.
        /// </summary>
        private string TrySaveLogoFile(HttpPostedFileBase logoFile, out string error)
        {
            error = null;
            if (logoFile == null || logoFile.ContentLength <= 0)
                return null;

            var extension = (Path.GetExtension(logoFile.FileName) ?? "").ToLowerInvariant();
            if (string.IsNullOrEmpty(extension) || Array.IndexOf(AllowedLogoExtensions, extension) < 0)
            {
                error = "Formato no permitido. Usa: " + string.Join(", ", AllowedLogoExtensions);
                return null;
            }
            if (logoFile.ContentLength > MaxLogoSizeBytes)
            {
                error = "El archivo supera el tamano maximo permitido (2 MB).";
                return null;
            }

            try
            {
                var folderPhysical = Server.MapPath("~/Files/Plantilla/");
                if (!Directory.Exists(folderPhysical))
                {
                    Directory.CreateDirectory(folderPhysical);
                }

                var safeBase = Path.GetFileNameWithoutExtension(logoFile.FileName);
                if (string.IsNullOrWhiteSpace(safeBase)) safeBase = "logo";
                foreach (var ch in Path.GetInvalidFileNameChars())
                    safeBase = safeBase.Replace(ch, '_');
                safeBase = safeBase.Replace(' ', '_');
                if (safeBase.Length > 40) safeBase = safeBase.Substring(0, 40);

                var fileName = string.Format(
                    "{0}_{1:yyyyMMdd_HHmmss}_{2}{3}",
                    safeBase,
                    DateTime.UtcNow,
                    Guid.NewGuid().ToString("N").Substring(0, 6),
                    extension);

                var fullPath = Path.Combine(folderPhysical, fileName);
                logoFile.SaveAs(fullPath);

                // Redimensionar proporcionalmente si es un formato rasterizado y excede los limites.
                // SVG (vectorial) e ICO (multi-resolucion) se dejan tal cual.
                if (extension != ".svg" && extension != ".ico")
                {
                    ResizeLogoIfNeeded(fullPath, extension);
                }

                return "/Files/Plantilla/" + fileName;
            }
            catch (Exception ex)
            {
                error = "No se pudo guardar el archivo: " + ex.Message;
                return null;
            }
        }

        // Extensiones admitidas para favicon. Se guardan sin redimensionar
        // (los .ico y .svg son formatos especiales; PNG tambien se mantiene).
        private static readonly string[] AllowedFaviconExtensions =
            new[] { ".ico", ".png", ".svg", ".jpg", ".jpeg", ".gif" };

        // Favicons son archivos pequenos; con 512 KB sobra.
        private const long MaxFaviconSizeBytes = 512 * 1024;

        /// <summary>
        /// Guarda en ~/Files/Plantilla/ el favicon subido. Valida extension y tamano.
        /// Devuelve la ruta web relativa, null si no se subio nada, o expone un error
        /// por el parametro 'error'.
        /// </summary>
        private string TrySaveFaviconFile(HttpPostedFileBase faviconFile, out string error)
        {
            error = null;
            if (faviconFile == null || faviconFile.ContentLength <= 0)
                return null;

            var extension = (Path.GetExtension(faviconFile.FileName) ?? "").ToLowerInvariant();
            if (string.IsNullOrEmpty(extension) || Array.IndexOf(AllowedFaviconExtensions, extension) < 0)
            {
                error = "Formato no permitido para el favicon. Usa: " + string.Join(", ", AllowedFaviconExtensions);
                return null;
            }
            if (faviconFile.ContentLength > MaxFaviconSizeBytes)
            {
                error = "El favicon supera el tamano maximo permitido (512 KB).";
                return null;
            }

            try
            {
                var folderPhysical = Server.MapPath("~/Files/Plantilla/");
                if (!Directory.Exists(folderPhysical))
                {
                    Directory.CreateDirectory(folderPhysical);
                }

                var safeBase = Path.GetFileNameWithoutExtension(faviconFile.FileName);
                if (string.IsNullOrWhiteSpace(safeBase)) safeBase = "favicon";
                foreach (var ch in Path.GetInvalidFileNameChars())
                    safeBase = safeBase.Replace(ch, '_');
                safeBase = safeBase.Replace(' ', '_');
                if (safeBase.Length > 40) safeBase = safeBase.Substring(0, 40);

                var fileName = string.Format(
                    "favicon_{0}_{1:yyyyMMdd_HHmmss}_{2}{3}",
                    safeBase,
                    DateTime.UtcNow,
                    Guid.NewGuid().ToString("N").Substring(0, 6),
                    extension);

                var fullPath = Path.Combine(folderPhysical, fileName);
                faviconFile.SaveAs(fullPath);
                return "/Files/Plantilla/" + fileName;
            }
            catch (Exception ex)
            {
                error = "No se pudo guardar el favicon: " + ex.Message;
                return null;
            }
        }

        /// <summary>
        /// Redimensiona el archivo en disco preservando la proporcion si supera
        /// MaxLogoWidth o MaxLogoHeight. Mantiene transparencia para PNG/GIF/WEBP.
        /// </summary>
        private static void ResizeLogoIfNeeded(string fullPath, string extension)
        {
            // Cargamos en memoria, cerramos el archivo original y luego sobreescribimos.
            byte[] originalBytes = System.IO.File.ReadAllBytes(fullPath);
            using (var ms = new MemoryStream(originalBytes))
            using (var original = Image.FromStream(ms))
            {
                int srcW = original.Width;
                int srcH = original.Height;

                if (srcW <= MaxLogoWidth && srcH <= MaxLogoHeight)
                {
                    return; // No hace falta redimensionar.
                }

                double ratio = Math.Min(
                    (double)MaxLogoWidth / srcW,
                    (double)MaxLogoHeight / srcH);
                int dstW = Math.Max(1, (int)Math.Round(srcW * ratio));
                int dstH = Math.Max(1, (int)Math.Round(srcH * ratio));

                using (var bmp = new Bitmap(dstW, dstH, PixelFormat.Format32bppArgb))
                {
                    bmp.SetResolution(original.HorizontalResolution, original.VerticalResolution);
                    using (var g = Graphics.FromImage(bmp))
                    {
                        g.CompositingMode = CompositingMode.SourceCopy;
                        g.CompositingQuality = CompositingQuality.HighQuality;
                        g.InterpolationMode = InterpolationMode.HighQualityBicubic;
                        g.SmoothingMode = SmoothingMode.HighQuality;
                        g.PixelOffsetMode = PixelOffsetMode.HighQuality;
                        g.DrawImage(original, new Rectangle(0, 0, dstW, dstH));
                    }

                    ImageFormat targetFormat;
                    switch (extension)
                    {
                        case ".jpg":
                        case ".jpeg":
                            targetFormat = ImageFormat.Jpeg;
                            break;
                        case ".gif":
                            targetFormat = ImageFormat.Gif;
                            break;
                        case ".webp":
                            // GDI+ no soporta WEBP nativo; reescribimos como PNG conservando calidad+transparencia.
                            targetFormat = ImageFormat.Png;
                            break;
                        default:
                            targetFormat = ImageFormat.Png;
                            break;
                    }

                    bmp.Save(fullPath, targetFormat);
                }
            }
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
        }
    }
}
