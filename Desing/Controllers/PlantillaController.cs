using DAL;
using DataTables.Mvc;
using Desing.Helpers;
using Desing.Models;
using Desing.Resources;
using Microsoft.AspNet.Identity;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Data.SqlClient;
using System.Web;
using System.Web.Mvc;

namespace Desing.Controllers
{
    /// <summary>
    /// Gestion de plantillas de estilo (color + logo + marca) por empresa.
    /// La plantilla se asigna por empresa (TSql_Company.LinPlantilla); todos los empleados de esa empresa la heredan.
    /// La plantilla marcada como "por defecto" se aplica cuando una empresa no tiene <c>LinPlantilla</c> propia
    /// y al crear un nuevo empleado/empresa.
    /// </summary>
    [Authorize]
    public class PlantillaController : BaseController
    {
        // ---------------------------------------------------------------------
        // INDEX + DataTable (patron Materio + applyListDefaults)
        // ---------------------------------------------------------------------
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult Details(long id)
        {
            TSql_Plantilla entity;
            try
            {
                entity = db.TSql_Plantilla.FirstOrDefault(p => p.SysObjectID == id && !p.AttIsDeleted);
            }
            catch (Exception ex)
            {
                var handled = RedirectIfPlantillaDataModelMismatch(ex);
                if (handled != null) return handled;
                throw;
            }

            if (entity == null)
            {
                return HttpNotFound();
            }
            return View(entity);
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
                        AttBrandText = p.AttBrandText,
                        AttColor = p.AttColor,
                        AttLogo = p.AttLogo,
                        AttIsDefault = p.AttIsDefault,
                        AttCreated = p.AttCreated
                    });

                var totalCount = query.Count();

                if (!string.IsNullOrEmpty(requestModel.Search.Value))
                {
                    var value = requestModel.Search.Value.Trim();
                    query = query.Where(p => (p.AttName ?? "").Contains(value) ||
                                             (p.AttBrandText ?? "").Contains(value) ||
                                             (p.AttColor ?? "").Contains(value));
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
                        case "AttName":
                        case "TextLabel":
                            orderColumn = "AttName"; break;
                        case "AttBrandText": orderColumn = "AttBrandText"; break;
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

                var ttOpen = HttpUtility.HtmlAttributeEncode(Plantilla.List_LinkOpenTooltip);
                var ttEdit = HttpUtility.HtmlAttributeEncode(Plantilla.List_LinkEditTooltip);
                var ttDelete = HttpUtility.HtmlAttributeEncode(Plantilla.List_LinkDeleteTooltip);
                var ttDeleteLocked = HttpUtility.HtmlAttributeEncode(Plantilla.List_DefaultLockedTooltip);
                var labelDefault = HttpUtility.HtmlEncode(Plantilla.State_Default);
                var labelNotDefault = HttpUtility.HtmlEncode(Plantilla.State_NotDefault);
                var logoFallback = Url.Content("~/Content/images/Login/at.png");

                var data = query.ToList().Select(p =>
                {
                    var namePlain = p.AttName ?? "";
                    var nameCell =
                        "<a title=\"" + ttOpen + "\" href=\"" +
                        Url.Content("~/Plantilla/Details/" + p.SysObjectID) + "\">" +
                        HttpUtility.HtmlEncode(namePlain) + "</a>";

                    var color = string.IsNullOrWhiteSpace(p.AttColor) ? "#349d7d" : p.AttColor;
                    var colorBadge =
                        "<span style=\"display:inline-block;width:22px;height:22px;border-radius:4px;border:1px solid #ccc;vertical-align:middle;background:" +
                        HttpUtility.HtmlAttributeEncode(color) + "\"></span> <code>" +
                        HttpUtility.HtmlEncode(color) + "</code>";

                    var logoPath = string.IsNullOrWhiteSpace(p.AttLogo) ? logoFallback : Url.Content("~" + p.AttLogo);
                    var logoPreview = "<img src=\"" + HttpUtility.HtmlAttributeEncode(logoPath) +
                                      "\" style=\"height:24px;background:#fff;padding:2px;border:1px solid #eee;border-radius:4px\" alt=\"\" />";

                    var defaultBadge = p.AttIsDefault
                        ? "<span class=\"badge bg-label-success\">" + labelDefault + "</span>"
                        : "<span class=\"text-muted\">" + labelNotDefault + "</span>";

                    var editBtn =
                        "<a title=\"" + ttEdit + "\" href=\"" +
                        Url.Action("Edit", "Plantilla", new { Id = p.SysObjectID }) +
                        "\" class=\"btn btn-warning btn-xs\"><span class=\"fas fa-edit\" aria-hidden=\"true\"></span></a>";

                    string deleteBtn;
                    if (p.AttIsDefault)
                    {
                        deleteBtn =
                            "<a title=\"" + ttDeleteLocked + "\" class=\"btn btn-secondary btn-xs disabled\" aria-disabled=\"true\" tabindex=\"-1\">" +
                            "<span class=\"fas fa-trash-alt\" aria-hidden=\"true\"></span></a>";
                    }
                    else
                    {
                        deleteBtn =
                            "<a title=\"" + ttDelete + "\" href=\"#\" onclick=\"DeletePlantilla(" + p.SysObjectID +
                            "); return false;\" class=\"btn btn-danger btn-xs\"><span class=\"fas fa-trash-alt\" aria-hidden=\"true\"></span></a>";
                    }

                    var rowActions =
                        "<div class=\"d-inline-flex align-items-center gap-2\" role=\"group\">" +
                        editBtn + deleteBtn + "</div>";

                    return new
                    {
                        SysObjectID = p.SysObjectID,
                        TextLabel = nameCell,
                        TextLabelPlain = namePlain,
                        AttName = namePlain,
                        AttBrandText = p.AttBrandText ?? "",
                        AttColor = color,
                        AttLogo = p.AttLogo ?? "",
                        colorBadge,
                        logoPreview,
                        defaultBadge,
                        AttIsDefault = p.AttIsDefault,
                        AttCreated = p.AttCreated.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture),
                        rowActions
                    };
                }).ToList();

                return Json(DataTablesMvcJson.Create(requestModel.Draw, data, filteredCount, totalCount), JsonRequestBehavior.AllowGet);
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
            ViewBag.MessageHeat = Plantilla.Page_CreateTitle;
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
            return View(model);
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

            ValidatePlantillaServer(model, isCreate: true);

            if (!ModelState.IsValid)
            {
                ViewBag.MessageHeat = Plantilla.Page_CreateTitle;
                return View(model);
            }

            string userId = User.Identity.GetUserId();
            DateTime now = DateTime.UtcNow;

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
                AttName = (model.AttName ?? "").Trim(),
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
            TempData["ToastTitle"] = Plantilla.ToastTitle_CreatePlantilla;
            TempData["ToastMessage"] = string.Format(Plantilla.ToastMessage_PlantillaCreated, plantilla.AttName);
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
                TempData["ToastTitle"] = Plantilla.ToastTitle_EditPlantilla;
                TempData["ToastMessage"] = Plantilla.Err_PlantillaNotFound;
                return RedirectToAction("Index");
            }

            ViewBag.MessageHeat = Plantilla.Page_EditTitle;
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
            return View(model);
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

            ValidatePlantillaServer(model, isCreate: false);

            if (!ModelState.IsValid)
            {
                ViewBag.MessageHeat = Plantilla.Page_EditTitle;
                return View(model);
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
                TempData["ToastTitle"] = Plantilla.ToastTitle_EditPlantilla;
                TempData["ToastMessage"] = Plantilla.Err_PlantillaNotFound;
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

            entity.AttName = (model.AttName ?? "").Trim();
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
            TempData["ToastTitle"] = Plantilla.ToastTitle_EditPlantilla;
            TempData["ToastMessage"] = string.Format(Plantilla.ToastMessage_PlantillaUpdated, entity.AttName);
            return RedirectToAction("Index");
        }

        // ---------------------------------------------------------------------
        // DELETE (logico). Bloqueada si es la default o si hay empresas que la usan.
        // ---------------------------------------------------------------------
        [HttpPost]
        public JsonResult DeletePlantilla(long id)
        {
            var entity = db.TSql_Plantilla.FirstOrDefault(p => p.SysObjectID == id && !p.AttIsDeleted);
            if (entity == null)
                return Json(new { IsOk = false, Message = Plantilla.Err_PlantillaNotFound });

            if (entity.AttIsDefault)
                return Json(new { IsOk = false, Message = Plantilla.Err_CannotDeleteDefault });

            var nombre = entity.AttName ?? "";

            entity.AttIsDeleted = true;
            entity.LinModifiedBy = User.Identity.GetUserId();
            entity.AttLastModification = DateTime.UtcNow;
            entity.SysUpdateNumber = entity.SysUpdateNumber + 1;

            // Si alguna empresa usa esta plantilla, se reasigna a la plantilla por defecto
            // (mantiene compatibilidad con LinPlantilla = FK; sin esto, la FK quedaria colgando).
            long? defaultId = db.TSql_Plantilla
                .Where(p => p.AttIsDefault && !p.AttIsDeleted && p.SysObjectID != entity.SysObjectID)
                .Select(p => (long?)p.SysObjectID)
                .FirstOrDefault();
            var afectados = db.TSql_Company.Where(c => !c.BitIsDeleted && c.LinPlantilla == id).ToList();
            foreach (var comp in afectados)
            {
                comp.LinPlantilla = defaultId;
            }

            db.SaveChanges();

            return Json(new
            {
                IsOk = true,
                Message = string.Format(Plantilla.ToastMessage_PlantillaDeleted, nombre)
            });
        }

        // Compatibilidad: link GET legado con confirm() del navegador.
        public ActionResult Delete(long Id)
        {
            var entity = db.TSql_Plantilla.FirstOrDefault(p => p.SysObjectID == Id && !p.AttIsDeleted);
            if (entity == null)
            {
                TempData["ToastType"] = "Error";
                TempData["ToastTitle"] = Plantilla.ToastTitle_DeletePlantilla;
                TempData["ToastMessage"] = Plantilla.Err_PlantillaNotFound;
                return RedirectToAction("Index");
            }
            if (entity.AttIsDefault)
            {
                TempData["ToastType"] = "Error";
                TempData["ToastTitle"] = Plantilla.ToastTitle_DeletePlantilla;
                TempData["ToastMessage"] = Plantilla.Err_CannotDeleteDefault;
                return RedirectToAction("Index");
            }

            long? defaultId = db.TSql_Plantilla
                .Where(p => p.AttIsDefault && !p.AttIsDeleted && p.SysObjectID != entity.SysObjectID)
                .Select(p => (long?)p.SysObjectID)
                .FirstOrDefault();

            var afectados = db.TSql_Company.Where(c => !c.BitIsDeleted && c.LinPlantilla == Id);
            foreach (var comp in afectados)
            {
                comp.LinPlantilla = defaultId;
            }

            var nombre = entity.AttName ?? "";
            entity.AttIsDeleted = true;
            entity.LinModifiedBy = User.Identity.GetUserId();
            entity.AttLastModification = DateTime.UtcNow;
            entity.SysUpdateNumber = entity.SysUpdateNumber + 1;
            db.SaveChanges();

            TempData["ToastType"] = "Act";
            TempData["ToastTitle"] = Plantilla.ToastTitle_DeletePlantilla;
            TempData["ToastMessage"] = string.Format(Plantilla.ToastMessage_PlantillaDeleted, nombre);
            return RedirectToAction("Index");
        }

        // ---------------------------------------------------------------------
        // Validacion servidor (mensajes traducidos)
        // ---------------------------------------------------------------------
        /// <summary>
        /// Validacion servidor en mensajes traducidos (Plantilla.Val_*). Sustituye los
        /// errores que pudieran venir de las DataAnnotations en español del ViewModel
        /// (Required / RegularExpression) por la version del .resx, y añade comprobaciones
        /// que no se pueden hacer en cliente (duplicados).
        /// </summary>
        private void ValidatePlantillaServer(PlantillaViewModel model, bool isCreate)
        {
            if (model == null) return;

            // AttName: required + duplicate (i18n).
            ClearFieldErrors("AttName");
            if (string.IsNullOrWhiteSpace(model.AttName))
            {
                ModelState.AddModelError("AttName", Plantilla.Val_NameRequired);
            }
            else
            {
                var nameNorm = model.AttName.Trim();
                bool duplicate = isCreate
                    ? db.TSql_Plantilla.Any(p => !p.AttIsDeleted && p.AttName == nameNorm)
                    : db.TSql_Plantilla.Any(p => !p.AttIsDeleted && p.SysObjectID != model.SysObjectID && p.AttName == nameNorm);
                if (duplicate)
                {
                    ModelState.AddModelError("AttName",
                        isCreate ? Plantilla.Val_DuplicateNameCreate : Plantilla.Val_DuplicateNameEdit);
                }
            }

            ClearFieldErrors("AttBrandText");
            if (string.IsNullOrWhiteSpace(model.AttBrandText))
            {
                ModelState.AddModelError("AttBrandText", Plantilla.Val_BrandTextRequired);
            }

            ClearFieldErrors("AttColor");
            if (string.IsNullOrWhiteSpace(model.AttColor))
            {
                ModelState.AddModelError("AttColor", Plantilla.Val_MainColorRequired);
            }
            else if (!HexColorRegex.IsMatch(model.AttColor.Trim()))
            {
                ModelState.AddModelError("AttColor", Plantilla.Val_MainColorHexFormat);
            }

            ClearFieldErrors("AttBrandAccentColor");
            if (string.IsNullOrWhiteSpace(model.AttBrandAccentColor))
            {
                ModelState.AddModelError("AttBrandAccentColor", Plantilla.Val_AccentColorRequired);
            }
            else if (!HexColorRegex.IsMatch(model.AttBrandAccentColor.Trim()))
            {
                ModelState.AddModelError("AttBrandAccentColor", Plantilla.Val_AccentColorHexFormat);
            }

            ClearFieldErrors("AttBrandTextColor");
            if (!string.IsNullOrWhiteSpace(model.AttBrandTextColor) &&
                !HexColorRegex.IsMatch(model.AttBrandTextColor.Trim()))
            {
                ModelState.AddModelError("AttBrandTextColor", Plantilla.Val_BrandTextColorHexFormat);
            }

            if (string.IsNullOrWhiteSpace(model.AttLogo))
            {
                ClearFieldErrors("AttLogo");
                ModelState.AddModelError("AttLogo", Plantilla.Val_LogoPathRequired);
            }
        }

        private void ClearFieldErrors(string field)
        {
            System.Web.Mvc.ModelState state;
            if (ModelState.TryGetValue(field, out state) && state != null && state.Errors != null)
            {
                state.Errors.Clear();
            }
        }

        private static readonly System.Text.RegularExpressions.Regex HexColorRegex =
            new System.Text.RegularExpressions.Regex(
                "^#([A-Fa-f0-9]{6}|[A-Fa-f0-9]{3})$",
                System.Text.RegularExpressions.RegexOptions.Compiled);

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
            TempData["ToastTitle"] = Plantilla.ToastTitle_DbError;
            TempData["ToastMessage"] = Plantilla.ToastMessage_DbBrandColumnsMissing;
            return RedirectToAction("Index");
        }

        private static bool IsPlantillaSchemaOrMaterializationError(Exception ex)
        {
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
                if (m.IndexOf("Invalid column name", StringComparison.OrdinalIgnoreCase) >= 0 &&
                    m.IndexOf("AttBrand", StringComparison.OrdinalIgnoreCase) >= 0)
                    return true;
                if (m.IndexOf("AttBrand", StringComparison.OrdinalIgnoreCase) >= 0 &&
                    m.IndexOf("columna", StringComparison.OrdinalIgnoreCase) >= 0 &&
                    (m.IndexOf("válid", StringComparison.OrdinalIgnoreCase) >= 0 || m.IndexOf("valido", StringComparison.OrdinalIgnoreCase) >= 0))
                    return true;
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
                    Text = p.AttName + (p.AttIsDefault ? " (" + Plantilla.State_Default + ")" : ""),
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

        private const int MaxLogoWidth = 600;
        private const int MaxLogoHeight = 200;

        private string TrySaveLogoFile(HttpPostedFileBase logoFile, out string error)
        {
            error = null;
            if (logoFile == null || logoFile.ContentLength <= 0)
                return null;

            var extension = (Path.GetExtension(logoFile.FileName) ?? "").ToLowerInvariant();
            if (string.IsNullOrEmpty(extension) || Array.IndexOf(AllowedLogoExtensions, extension) < 0)
            {
                error = string.Format(Plantilla.Err_LogoFormatNotAllowed, string.Join(", ", AllowedLogoExtensions));
                return null;
            }
            if (logoFile.ContentLength > MaxLogoSizeBytes)
            {
                error = Plantilla.Err_LogoTooLarge;
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

                if (extension != ".svg" && extension != ".ico")
                {
                    ResizeLogoIfNeeded(fullPath, extension);
                }

                return "/Files/Plantilla/" + fileName;
            }
            catch (Exception ex)
            {
                error = string.Format(Plantilla.Err_LogoSaveFailed, ex.Message);
                return null;
            }
        }

        private static readonly string[] AllowedFaviconExtensions =
            new[] { ".ico", ".png", ".svg", ".jpg", ".jpeg", ".gif" };

        private const long MaxFaviconSizeBytes = 512 * 1024;

        private string TrySaveFaviconFile(HttpPostedFileBase faviconFile, out string error)
        {
            error = null;
            if (faviconFile == null || faviconFile.ContentLength <= 0)
                return null;

            var extension = (Path.GetExtension(faviconFile.FileName) ?? "").ToLowerInvariant();
            if (string.IsNullOrEmpty(extension) || Array.IndexOf(AllowedFaviconExtensions, extension) < 0)
            {
                error = string.Format(Plantilla.Err_FaviconFormatNotAllowed, string.Join(", ", AllowedFaviconExtensions));
                return null;
            }
            if (faviconFile.ContentLength > MaxFaviconSizeBytes)
            {
                error = Plantilla.Err_FaviconTooLarge;
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
                error = string.Format(Plantilla.Err_FaviconSaveFailed, ex.Message);
                return null;
            }
        }

        /// <summary>
        /// Redimensiona el archivo en disco preservando la proporcion si supera
        /// MaxLogoWidth o MaxLogoHeight. Mantiene transparencia para PNG/GIF/WEBP.
        /// </summary>
        private static void ResizeLogoIfNeeded(string fullPath, string extension)
        {
            byte[] originalBytes = System.IO.File.ReadAllBytes(fullPath);
            using (var ms = new MemoryStream(originalBytes))
            using (var original = Image.FromStream(ms))
            {
                int srcW = original.Width;
                int srcH = original.Height;

                if (srcW <= MaxLogoWidth && srcH <= MaxLogoHeight)
                {
                    return;
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
