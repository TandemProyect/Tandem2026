using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.Mvc;
using DAL;
using DataTables.Mvc;
using Desing.Helpers;
using Desing.Models;
using Desing.Resources;

namespace Desing.Controllers
{
    /// <summary>
    /// CRUD del catalogo "Idiomas" (TSql_Language). Modulo delicado: alimenta el
    /// switcher del navbar y DbBackedResourceManager (override BD del .resx).
    ///
    /// Sigue el patron Materio + DataTables estandar (rowActions con Edit + Delete,
    /// TextLabelPlain, exportOptsPlainVisible, colReorder fijo a la derecha) y
    /// delega los textos a Desing.Resources.Language (.resx + DbBackedResourceManager)
    /// reutilizando Desing.Resources.Common para botones y mensajes genericos.
    ///
    /// Auditoria estandar via IntranetAuditHelper y borrado logico (Is_Delete = true)
    /// con bloqueo si el idioma es el predeterminado (Is_Default = true) o si esta
    /// enlazado a empresas activas (TSql_Company.LinkLanguage).
    ///
    /// La logica de seleccion de idioma (cookie tandem_lang, BaseController,
    /// LanguageUiHelper, UiLanguageController, AccountController) NO se toca:
    /// solo migra textos y patron visual.
    /// </summary>
    [Authorize]
    public class LanguageController : BaseController
    {
        private static readonly Regex TextCodeRegex = new Regex(@"^[a-zA-Z]{2}(-[a-zA-Z]{2,8})?$", RegexOptions.Compiled);

        private sealed class LanguageListRow
        {
            public long IdObject { get; set; }
            public string TextLabel { get; set; }
            public string TextCode { get; set; }
            public string TextNativeName { get; set; }
            public bool Is_Default { get; set; }
            public bool Is_Active { get; set; }
            public string FlagRaw { get; set; }
            public string CountryLabel { get; set; }
        }

        // ---------------------------------------------------------------------
        // INDEX + DataTable (patron Materio + applyListDefaults)
        // ---------------------------------------------------------------------
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult Details(long id)
        {
            var entity = db.TSql_language.FirstOrDefault(l => l.IdObject == id && !l.Is_Delete);
            if (entity == null)
            {
                return HttpNotFound();
            }

            ViewBag.Audit = IntranetAuditHelper.BuildDisplay(
                db,
                entity.LinkMadeBy,
                entity.LinModifiedBy,
                entity.AddDateMade,
                entity.AddLastDateChange,
                entity.Ntimeschanged);

            ViewBag.CountryBootstrap = BuildLanguageCountryBootstrap(entity.LinkCountry);

            return View(entity);
        }

        [OutputCache(Duration = 1)]
        public JsonResult ListLanguages([ModelBinder(typeof(DataTablesBinder))] IDataTablesRequest requestModel)
        {
            try
            {
                IQueryable<LanguageListRow> query =
                    from l in db.TSql_language
                    join c in db.TSql_Countrys on l.LinkCountry equals c.IdObject into cg
                    from c in cg.DefaultIfEmpty()
                    where !l.Is_Delete
                    select new LanguageListRow
                    {
                        IdObject = l.IdObject,
                        TextLabel = l.TextLabel,
                        TextCode = l.TextCode,
                        TextNativeName = l.TextNativeName,
                        Is_Default = l.Is_Default,
                        Is_Active = l.Is_Active,
                        FlagRaw = c != null ? c.TextFlag : null,
                        CountryLabel = c != null ? c.TextLabel : ""
                    };

                var totalCount = query.Count();

                if (!string.IsNullOrEmpty(requestModel.Search.Value))
                {
                    var value = requestModel.Search.Value.Trim();
                    query = query.Where(p =>
                        (p.TextLabel ?? "").Contains(value) ||
                        (p.TextCode ?? "").Contains(value) ||
                        (p.TextNativeName ?? "").Contains(value) ||
                        (p.CountryLabel ?? "").Contains(value));
                }

                var filteredCount = query.Count();

                var sortedColumns = requestModel.Columns.GetSortedColumns();
                var orderByString = string.Empty;
                foreach (var column in sortedColumns)
                {
                    string orderColumn;
                    switch (column.Data)
                    {
                        case "TextLabel":
                        case "TextLabelPlain":
                            orderColumn = "TextLabel"; break;
                        case "TextCode":
                            orderColumn = "TextCode"; break;
                        case "TextNativeName":
                            orderColumn = "TextNativeName"; break;
                        case "CountryLabel":
                            orderColumn = "CountryLabel"; break;
                        case "Is_Default":
                        case "defaultBadge":
                            orderColumn = "Is_Default"; break;
                        case "Is_Active":
                        case "activeBadge":
                            orderColumn = "Is_Active"; break;
                        default:
                            orderColumn = "TextLabel"; break;
                    }
                    orderByString += orderByString != string.Empty ? "," : "";
                    orderByString += orderColumn + (column.SortDirection == Column.OrderDirection.Ascendant ? " asc" : " desc");
                }

                query = query.OrderBy(string.IsNullOrEmpty(orderByString) ? "Is_Default desc,TextLabel asc" : orderByString);
                query = query.ApplyDataTablesPaging(requestModel.Start, requestModel.Length);

                var rows = query.ToList();
                var ids = rows.ConvertAll(r => r.IdObject);

                // Bloqueo de borrado: idiomas usados por alguna empresa activa
                // via TSql_Company.LinkLanguage (relacion nullable).
                var idsWithCompanies = ids.Count == 0
                    ? new HashSet<long>()
                    : db.TSql_Company
                        .Where(c => !c.BitIsDeleted
                                 && c.LinkLanguage.HasValue
                                 && ids.Contains(c.LinkLanguage.Value))
                        .Select(c => c.LinkLanguage.Value)
                        .Distinct()
                        .ToList()
                        .ToHashSet();

                var ttOpen = HttpUtility.HtmlAttributeEncode(Language.List_LinkOpenTooltip);
                var ttEdit = HttpUtility.HtmlAttributeEncode(Language.List_LinkEditTooltip);
                var ttDelete = HttpUtility.HtmlAttributeEncode(Language.List_LinkDeleteTooltip);
                var ttDeleteDefault = HttpUtility.HtmlAttributeEncode(Language.List_LinkDeleteLockedDefaultTooltip);
                var ttDeleteCompanies = HttpUtility.HtmlAttributeEncode(Language.List_LinkDeleteLockedCompaniesTooltip);
                var lblActive = HttpUtility.HtmlEncode(Language.State_Active);
                var lblInactive = HttpUtility.HtmlEncode(Language.State_Inactive);
                var lblDefaultYes = HttpUtility.HtmlEncode(Language.State_DefaultYes);
                var lblDefaultNo = HttpUtility.HtmlEncode(Language.State_DefaultNo);
                var lblNoFlag = HttpUtility.HtmlEncode(Language.List_NoFlag);
                var lblNoNative = HttpUtility.HtmlEncode(Language.List_NoNativeName);

                var data = rows.Select(p =>
                {
                    var flagVp = LanguageUiHelper.NormalizeCountryFlagVirtualPath(p.FlagRaw);
                    string flagImg;
                    if (string.IsNullOrEmpty(flagVp))
                    {
                        flagImg = "<span class=\"text-muted\">" + lblNoFlag + "</span>";
                    }
                    else
                    {
                        var src = HttpUtility.HtmlAttributeEncode(Url.Content(flagVp));
                        flagImg = "<img src=\"" + src + "\" alt=\"\" width=\"28\" height=\"20\" class=\"rounded border\" style=\"object-fit:cover\" " +
                                  "onerror=\"this.style.display='none'; var s=this.nextElementSibling; if(s) s.classList.remove('d-none');\" />" +
                                  "<span class=\"text-muted d-none\">" + lblNoFlag + "</span>";
                    }

                    var defaultBadge = p.Is_Default
                        ? "<span class=\"badge bg-label-primary\">" + lblDefaultYes + "</span>"
                        : "<span class=\"text-muted\">" + lblDefaultNo + "</span>";

                    var native = string.IsNullOrWhiteSpace(p.TextNativeName)
                        ? lblNoNative
                        : HttpUtility.HtmlEncode(p.TextNativeName);

                    var namePlain = p.TextLabel ?? "";
                    var nameCell =
                        "<a title=\"" + ttOpen + "\" href=\"" +
                        Url.Action("Details", new { id = p.IdObject }) + "\">" +
                        HttpUtility.HtmlEncode(namePlain) + "</a>";

                    var editBtn =
                        "<a title=\"" + ttEdit + "\" aria-label=\"" + ttEdit + "\" href=\"" +
                        Url.Action("Edit", new { id = p.IdObject }) +
                        "\" class=\"btn btn-warning btn-xs\"><span class=\"fas fa-edit\" aria-hidden=\"true\"></span></a>";

                    string deleteBtn;
                    if (p.Is_Default)
                    {
                        deleteBtn =
                            "<a title=\"" + ttDeleteDefault + "\" aria-label=\"" + ttDeleteDefault +
                            "\" class=\"btn btn-secondary btn-xs disabled\" aria-disabled=\"true\" tabindex=\"-1\">" +
                            "<span class=\"fas fa-trash-alt\" aria-hidden=\"true\"></span></a>";
                    }
                    else if (idsWithCompanies.Contains(p.IdObject))
                    {
                        deleteBtn =
                            "<a title=\"" + ttDeleteCompanies + "\" aria-label=\"" + ttDeleteCompanies +
                            "\" class=\"btn btn-secondary btn-xs disabled\" aria-disabled=\"true\" tabindex=\"-1\">" +
                            "<span class=\"fas fa-trash-alt\" aria-hidden=\"true\"></span></a>";
                    }
                    else
                    {
                        deleteBtn =
                            "<a title=\"" + ttDelete + "\" aria-label=\"" + ttDelete +
                            "\" href=\"#\" onclick=\"DeleteLanguage(" + p.IdObject +
                            "); return false;\" class=\"btn btn-danger btn-xs\"><span class=\"fas fa-trash-alt\" aria-hidden=\"true\"></span></a>";
                    }

                    var rowActions =
                        "<div class=\"d-inline-flex align-items-center gap-2\" role=\"group\">" +
                        editBtn + deleteBtn + "</div>";

                    return new
                    {
                        IdObject = p.IdObject,
                        flagImg,
                        TextLabel = nameCell,
                        TextLabelPlain = namePlain,
                        TextCode = "<code>" + HttpUtility.HtmlEncode(p.TextCode ?? "") + "</code>",
                        TextCodePlain = p.TextCode ?? "",
                        TextNativeName = native,
                        TextNativeNamePlain = p.TextNativeName ?? "",
                        defaultBadge,
                        Is_Default = p.Is_Default,
                        activeBadge = p.Is_Active
                            ? "<span class=\"badge bg-label-success\">" + lblActive + "</span>"
                            : "<span class=\"badge bg-label-secondary\">" + lblInactive + "</span>",
                        Is_Active = p.Is_Active,
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
            ViewBag.IsEdit = false;
            ViewBag.LanguageCountryBootstrap = BuildLanguageCountryBootstrap(linkCountryId: null);
            var model = new TSql_language
            {
                Is_Active = true,
                Is_Default = false
            };
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(TSql_language model)
        {
            ViewBag.IsEdit = false;
            ViewBag.LanguageCountryBootstrap = BuildLanguageCountryBootstrap(model?.LinkCountry);

            NormalizeLanguageInput(model);
            ValidateLanguage(model, isCreate: true);

            if (!ModelState.IsValid)
                return View(model);

            // Crear con auditoria estandar; no permitir marcar como predeterminado
            // desde Create para evitar tener que desmarcar otros en el mismo flujo.
            var nuevo = new TSql_language
            {
                TextLabel = model.TextLabel?.Trim(),
                TextCode = model.TextCode?.Trim(),
                TextNativeName = string.IsNullOrWhiteSpace(model.TextNativeName)
                    ? null
                    : model.TextNativeName.Trim(),
                LinkCountry = model.LinkCountry,
                Is_Active = model.Is_Active,
                Is_Default = false
            };
            IntranetAuditHelper.SetAuditOnCreate(nuevo, User);

            db.TSql_language.Add(nuevo);
            db.SaveChanges();

            TempData["ToastType"] = "Act";
            TempData["ToastTitle"] = Language.ToastTitle_CreateLanguage;
            TempData["ToastMessage"] = string.Format(Language.ToastMessage_LanguageCreated, nuevo.TextLabel);
            return RedirectToAction(nameof(Index));
        }

        // ---------------------------------------------------------------------
        // EDIT
        // ---------------------------------------------------------------------
        public ActionResult Edit(long id)
        {
            var entity = db.TSql_language.FirstOrDefault(l => l.IdObject == id && !l.Is_Delete);
            if (entity == null)
            {
                TempData["ToastType"] = "Error";
                TempData["ToastTitle"] = Language.ToastTitle_EditLanguage;
                TempData["ToastMessage"] = Language.Err_LanguageNotFound;
                return RedirectToAction(nameof(Index));
            }

            ViewBag.LanguageCountryBootstrap = BuildLanguageCountryBootstrap(entity.LinkCountry);
            ViewBag.IsEdit = true;
            return View(entity);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(TSql_language model)
        {
            ViewBag.IsEdit = true;
            ViewBag.LanguageCountryBootstrap = BuildLanguageCountryBootstrap(model?.LinkCountry);

            if (model == null)
            {
                TempData["ToastType"] = "Error";
                TempData["ToastTitle"] = Language.ToastTitle_EditLanguage;
                TempData["ToastMessage"] = Language.Err_LanguageNotFound;
                return RedirectToAction(nameof(Index));
            }

            var entity = db.TSql_language.FirstOrDefault(l => l.IdObject == model.IdObject && !l.Is_Delete);
            if (entity == null)
            {
                TempData["ToastType"] = "Error";
                TempData["ToastTitle"] = Language.ToastTitle_EditLanguage;
                TempData["ToastMessage"] = Language.Err_LanguageNotFound;
                return RedirectToAction(nameof(Index));
            }

            NormalizeLanguageInput(model);
            ValidateLanguage(model, isCreate: false, excludeId: entity.IdObject);

            // Si el usuario intenta desmarcar Is_Default sin asignarlo a otro idioma,
            // bloquear: siempre debe existir un idioma por defecto.
            if (!model.Is_Default && entity.Is_Default)
            {
                ModelState.AddModelError(nameof(model.Is_Default), Language.Val_DefaultMustExist);
            }

            if (!ModelState.IsValid)
                return View(model);

            entity.TextLabel = model.TextLabel?.Trim();
            entity.TextCode = model.TextCode?.Trim();
            entity.TextNativeName = string.IsNullOrWhiteSpace(model.TextNativeName)
                ? null
                : model.TextNativeName.Trim();
            entity.LinkCountry = model.LinkCountry;
            entity.Is_Active = model.Is_Active;

            // Si se marca este idioma como predeterminado, desmarcar los demas en
            // la misma transaccion (solo puede haber uno).
            if (model.Is_Default)
            {
                var others = db.TSql_language.Where(l =>
                    l.IdObject != entity.IdObject && !l.Is_Delete && l.Is_Default).ToList();
                foreach (var o in others)
                {
                    o.Is_Default = false;
                    IntranetAuditHelper.SetAuditOnUpdate(o, User);
                }
            }

            entity.Is_Default = model.Is_Default;
            IntranetAuditHelper.SetAuditOnUpdate(entity, User);

            db.SaveChanges();

            // Tras editar TextCode / Is_Default / Is_Active el resolver de cultura
            // (DbBackedResourceManager.Load) podria mirar otra fila distinta.
            // Invalidar para que el cambio sea visible en caliente.
            DbBackedResourceManager.Invalidate();

            TempData["ToastType"] = "Act";
            TempData["ToastTitle"] = Language.ToastTitle_EditLanguage;
            TempData["ToastMessage"] = string.Format(Language.ToastMessage_LanguageUpdated, entity.TextLabel);
            return RedirectToAction(nameof(Index));
        }

        // ---------------------------------------------------------------------
        // DELETE (logico). Bloqueada si Is_Default o si hay empresas con LinkLanguage.
        // ---------------------------------------------------------------------
        [HttpPost]
        [ValidateAntiForgeryToken]
        public JsonResult DeleteLanguage(long id)
        {
            var entity = db.TSql_language.FirstOrDefault(l => l.IdObject == id && !l.Is_Delete);
            if (entity == null)
            {
                return Json(new { IsOk = false, Message = Language.Err_LanguageNotFound });
            }

            if (entity.Is_Default)
            {
                return Json(new { IsOk = false, Message = Language.Err_CannotDeleteIsDefault });
            }

            if (db.TSql_Company.Any(c => !c.BitIsDeleted
                                      && c.LinkLanguage.HasValue
                                      && c.LinkLanguage.Value == id))
            {
                return Json(new { IsOk = false, Message = Language.Err_CannotDeleteHasCompanies });
            }

            var nombre = entity.TextLabel ?? "";
            IntranetAuditHelper.SetAuditOnDelete(entity, User);
            db.SaveChanges();

            // Las traducciones de UiTranslation pueden seguir referenciando este idioma,
            // pero la lectura ya filtra por TSql_language activo: invalidamos la cache
            // para que el siguiente GetString reabra el lookup.
            DbBackedResourceManager.Invalidate();

            return Json(new
            {
                IsOk = true,
                Message = string.Format(Language.ToastMessage_LanguageDeleted, nombre)
            });
        }

        // ---------------------------------------------------------------------
        // Catalogo de paises (autocompletado + lista). Sin antiforgery; GET.
        // ---------------------------------------------------------------------

        /// <summary>Catálogo de países (JSON) para autocompletar; GET sin antiforgery. Incluye todas las filas de <see cref="TSql_Countrys"/> (el catálogo completo del desplegable debe ser coherente con <see cref="ListCountriesCatalog"/>).</summary>
        [HttpGet]
        public JsonResult SearchCountries(string q)
        {
            q = q == null ? string.Empty : q.Trim();
            if (q.Length < 3)
                return Json(new object[0], JsonRequestBehavior.AllowGet);

            var term = q.ToLowerInvariant();

            var rows = db.TSql_Countrys
                .AsNoTracking()
                .Where(c => c.TextLabel.ToLower().Contains(term))
                .OrderBy(c => c.TextLabel)
                .Take(40)
                .Select(c => new { c.IdObject, c.TextLabel, c.TextIso2, c.TextIso3, c.TextFlag })
                .ToList();

            var data = rows.ConvertAll(r =>
            {
                var flagVp = LanguageUiHelper.NormalizeCountryFlagVirtualPath(r.TextFlag);
                var flagUrl = string.IsNullOrEmpty(flagVp) ? null : Url.Content(flagVp);
                return new
                {
                    id = r.IdObject,
                    label = r.TextLabel,
                    iso2 = r.TextIso2 ?? "",
                    iso3 = r.TextIso3 ?? "",
                    flagUrl
                };
            });

            return Json(data, JsonRequestBehavior.AllowGet);
        }

        /// <summary>Listado único del catálogo de países (desplegable). Todas las filas de <see cref="TSql_Countrys"/> hasta <paramref name="take"/> (máx. 500); sin filtro por <see cref="TSql_Countrys.AddIsActive"/> para que el listado coincida con el volumen real del catálogo (~países ISO).</summary>
        [HttpGet]
        public JsonResult ListCountriesCatalog(int take = 500)
        {
            if (take < 1) take = 1;
            if (take > 500) take = 500;

            var rows = db.TSql_Countrys
                .AsNoTracking()
                .OrderBy(c => c.TextLabel)
                .Take(take)
                .Select(c => new { c.IdObject, c.TextLabel, c.TextIso2, c.TextIso3, c.TextFlag })
                .ToList();

            var data = rows.ConvertAll(r =>
            {
                var flagVp = LanguageUiHelper.NormalizeCountryFlagVirtualPath(r.TextFlag);
                var flagUrl = string.IsNullOrEmpty(flagVp) ? null : Url.Content(flagVp);
                return new
                {
                    id = r.IdObject,
                    label = r.TextLabel,
                    iso2 = r.TextIso2 ?? "",
                    iso3 = r.TextIso3 ?? "",
                    flagUrl
                };
            });

            return Json(data, JsonRequestBehavior.AllowGet);
        }

        // ---------------------------------------------------------------------
        // Helpers privados
        // ---------------------------------------------------------------------
        private LanguageCountryFormBootstrap BuildLanguageCountryBootstrap(long? linkCountryId)
        {
            if (!linkCountryId.HasValue)
                return new LanguageCountryFormBootstrap();

            var c = db.TSql_Countrys
                .AsNoTracking()
                .FirstOrDefault(x => x.IdObject == linkCountryId.Value);

            if (c == null)
                return new LanguageCountryFormBootstrap();

            var flagVp = LanguageUiHelper.NormalizeCountryFlagVirtualPath(c.TextFlag);
            return new LanguageCountryFormBootstrap
            {
                Id = c.IdObject,
                Label = c.TextLabel,
                Iso2 = c.TextIso2,
                Iso3 = c.TextIso3,
                FlagUrl = string.IsNullOrEmpty(flagVp) ? null : Url.Content(flagVp)
            };
        }

        private static void NormalizeLanguageInput(TSql_language model)
        {
            if (model == null)
                return;
            model.TextLabel = model.TextLabel?.Trim();
            model.TextCode = model.TextCode?.Trim();
            if (!string.IsNullOrEmpty(model.TextCode))
                model.TextCode = model.TextCode.ToLowerInvariant();
            if (!string.IsNullOrEmpty(model.TextNativeName))
                model.TextNativeName = model.TextNativeName.Trim();
        }

        private void ValidateLanguage(TSql_language model, bool isCreate, long? excludeId = null)
        {
            if (model == null)
                return;

            ClearFieldErrors(nameof(model.TextLabel));
            ClearFieldErrors(nameof(model.TextCode));
            ClearFieldErrors(nameof(model.TextNativeName));

            if (string.IsNullOrWhiteSpace(model.TextLabel))
            {
                ModelState.AddModelError(nameof(model.TextLabel), Language.Val_NameRequired);
            }
            else if (model.TextLabel.Trim().Length > 500)
            {
                ModelState.AddModelError(nameof(model.TextLabel), Language.Val_NameTooLong);
            }

            if (string.IsNullOrWhiteSpace(model.TextCode))
            {
                ModelState.AddModelError(nameof(model.TextCode), Language.Val_CodeRequired);
            }
            else if (model.TextCode.Length > 20)
            {
                ModelState.AddModelError(nameof(model.TextCode), Language.Val_CodeTooLong);
            }
            else if (!TextCodeRegex.IsMatch(model.TextCode))
            {
                ModelState.AddModelError(nameof(model.TextCode), Language.Val_CodeFormat);
            }
            else
            {
                var dup = db.TSql_language.Any(l =>
                    !l.Is_Delete &&
                    l.TextCode == model.TextCode &&
                    (!excludeId.HasValue || l.IdObject != excludeId.Value));

                if (dup)
                    ModelState.AddModelError(nameof(model.TextCode), Language.Val_CodeDuplicate);
            }

            if (!string.IsNullOrEmpty(model.TextNativeName) && model.TextNativeName.Length > 100)
            {
                ModelState.AddModelError(nameof(model.TextNativeName), Language.Val_NativeNameTooLong);
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
