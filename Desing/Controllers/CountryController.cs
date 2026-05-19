using DAL;
using DataTables.Mvc;
using Desing.Helpers;
using Desing.Resources;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Web;
using System.Web.Mvc;

namespace Desing.Controllers
{
    /// <summary>
    /// CRUD del catalogo de paises (<see cref="TSql_Countrys"/>). Patron Materio +
    /// DataTables (rowActions, TextLabelPlain, exportOptsPlainVisible,
    /// colReorder fijo a la derecha) e i18n <see cref="Desing.Resources.Country"/>
    /// con <see cref="DbBackedResourceManager"/> (TextModule "Country").
    ///
    /// Auditoria legacy: <c>LinkMadeBy</c>, <c>LinkChangeBy</c>, <c>AddDateMade</c>,
    /// <c>AddLastDateChange</c>, <c>Ntimeschanged</c> (sin <c>Is_Delete</c>).
    /// Eliminacion fisica solo si ningun <see cref="TSql_language"/> activo ni
    /// <see cref="TSql_Company"/> activa enlaza el pais.
    /// </summary>
    [Authorize]
    public class CountryController : BaseController
    {
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult Details(long id)
        {
            var entity = db.TSql_Countrys.FirstOrDefault(c => c.IdObject == id);
            if (entity == null)
            {
                return HttpNotFound();
            }

            ViewBag.Audit = IntranetAuditHelper.BuildDisplay(
                db,
                entity.LinkMadeBy,
                linModifiedBy: null,
                addChangeBy: entity.LinkChangeBy,
                entity.AddDateMade,
                entity.AddLastDateChange,
                entity.Ntimeschanged);

            return View(entity);
        }

        [OutputCache(Duration = 1)]
        public JsonResult ListCountries([ModelBinder(typeof(DataTablesBinder))] IDataTablesRequest requestModel)
        {
            try
            {
                var query = db.TSql_Countrys.AsNoTracking().Select(c => new
                {
                    c.IdObject,
                    c.TextLabel,
                    c.TextIso2,
                    c.TextIso3,
                    c.NumberIso,
                    c.TextFlag,
                    c.AddIsActive
                });

                var totalCount = query.Count();

                if (!string.IsNullOrEmpty(requestModel.Search.Value))
                {
                    var value = requestModel.Search.Value.Trim();
                    query = query.Where(p =>
                        (p.TextLabel ?? "").Contains(value) ||
                        (p.TextIso2 ?? "").Contains(value) ||
                        (p.TextIso3 ?? "").Contains(value) ||
                        (p.NumberIso ?? "").Contains(value));
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
                            orderColumn = "TextLabel";
                            break;
                        case "iso2Cell":
                        case "TextIso2":
                            orderColumn = "TextIso2";
                            break;
                        case "iso3Cell":
                        case "TextIso3":
                            orderColumn = "TextIso3";
                            break;
                        case "numberIsoCell":
                        case "NumberIso":
                            orderColumn = "NumberIso";
                            break;
                        case "AddIsActive":
                        case "activeBadge":
                            orderColumn = "AddIsActive";
                            break;
                        default:
                            orderColumn = "TextLabel";
                            break;
                    }

                    orderByString += orderByString != string.Empty ? "," : "";
                    orderByString += orderColumn +
                        (column.SortDirection == Column.OrderDirection.Ascendant ? " asc" : " desc");
                }

                query = query.OrderBy(string.IsNullOrEmpty(orderByString) ? "TextLabel asc" : orderByString);
                query = query.ApplyDataTablesPaging(requestModel.Start, requestModel.Length);

                var rows = query.ToList();
                var ids = rows.ConvertAll(r => r.IdObject);

                var idsWithLanguages = ids.Count == 0
                    ? new HashSet<long>()
                    : db.TSql_language
                        .Where(l => !l.Is_Delete && l.LinkCountry.HasValue && ids.Contains(l.LinkCountry.Value))
                        .Select(l => l.LinkCountry.Value)
                        .Distinct()
                        .ToList()
                        .ToHashSet();

                var idsWithCompanies = ids.Count == 0
                    ? new HashSet<long>()
                    : db.TSql_Company
                        .Where(c => !c.BitIsDeleted
                                 && c.LinkCountry.HasValue
                                 && ids.Contains(c.LinkCountry.Value))
                        .Select(c => c.LinkCountry.Value)
                        .Distinct()
                        .ToList()
                        .ToHashSet();

                var ttOpen = HttpUtility.HtmlAttributeEncode(Country.List_LinkOpenTooltip);
                var ttEdit = HttpUtility.HtmlAttributeEncode(Country.List_LinkEditTooltip);
                var ttDelete = HttpUtility.HtmlAttributeEncode(Country.List_LinkDeleteTooltip);
                var ttDeleteLang = HttpUtility.HtmlAttributeEncode(Country.List_LinkDeleteLockedLanguagesTooltip);
                var ttDeleteCo = HttpUtility.HtmlAttributeEncode(Country.List_LinkDeleteLockedCompaniesTooltip);
                var lblActive = HttpUtility.HtmlEncode(Country.State_Active);
                var lblInactive = HttpUtility.HtmlEncode(Country.State_Inactive);
                var lblNoFlag = HttpUtility.HtmlEncode(Country.List_NoFlag);

                var data = rows.Select(p =>
                {
                    var namePlain = p.TextLabel ?? "";
                    var nameCell =
                        "<a title=\"" + ttOpen + "\" href=\"" +
                        Url.Action("Details", new { id = p.IdObject }) + "\">" +
                        HttpUtility.HtmlEncode(namePlain) + "</a>";

                    var flagVp = LanguageUiHelper.NormalizeCountryFlagVirtualPath(p.TextFlag);
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

                    var iso2Plain = p.TextIso2 ?? "";
                    var iso3Plain = p.TextIso3 ?? "";
                    var numPlain = p.NumberIso ?? "";

                    var activeBadge = p.AddIsActive
                        ? "<span class=\"badge bg-label-success\">" + lblActive + "</span>"
                        : "<span class=\"badge bg-label-secondary\">" + lblInactive + "</span>";

                    var editBtn =
                        "<a title=\"" + ttEdit + "\" aria-label=\"" + ttEdit + "\" href=\"" +
                        Url.Action("Edit", new { id = p.IdObject }) +
                        "\" class=\"btn btn-warning btn-xs\"><span class=\"fas fa-edit\" aria-hidden=\"true\"></span></a>";

                    string deleteBtn;
                    if (idsWithLanguages.Contains(p.IdObject))
                    {
                        deleteBtn =
                            "<a title=\"" + ttDeleteLang + "\" aria-label=\"" + ttDeleteLang +
                            "\" class=\"btn btn-secondary btn-xs disabled\" aria-disabled=\"true\" tabindex=\"-1\">" +
                            "<span class=\"fas fa-trash-alt\" aria-hidden=\"true\"></span></a>";
                    }
                    else if (idsWithCompanies.Contains(p.IdObject))
                    {
                        deleteBtn =
                            "<a title=\"" + ttDeleteCo + "\" aria-label=\"" + ttDeleteCo +
                            "\" class=\"btn btn-secondary btn-xs disabled\" aria-disabled=\"true\" tabindex=\"-1\">" +
                            "<span class=\"fas fa-trash-alt\" aria-hidden=\"true\"></span></a>";
                    }
                    else
                    {
                        deleteBtn =
                            "<a title=\"" + ttDelete + "\" aria-label=\"" + ttDelete +
                            "\" href=\"#\" onclick=\"DeleteCountry(" + p.IdObject +
                            "); return false;\" class=\"btn btn-danger btn-xs\"><span class=\"fas fa-trash-alt\" aria-hidden=\"true\"></span></a>";
                    }

                    var rowActions =
                        "<div class=\"d-inline-flex align-items-center gap-2\" role=\"group\">" +
                        editBtn + deleteBtn + "</div>";

                    return new
                    {
                        p.IdObject,
                        flagImg,
                        TextLabel = nameCell,
                        TextLabelPlain = namePlain,
                        iso2Cell = HttpUtility.HtmlEncode(iso2Plain),
                        TextIso2Plain = iso2Plain,
                        iso3Cell = HttpUtility.HtmlEncode(iso3Plain),
                        TextIso3Plain = iso3Plain,
                        numberIsoCell = HttpUtility.HtmlEncode(numPlain),
                        NumberIsoPlain = numPlain,
                        AddIsActive = p.AddIsActive,
                        activeBadge,
                        rowActions
                    };
                }).ToList();

                return Json(DataTablesMvcJson.Create(requestModel.Draw, data, filteredCount, totalCount),
                    JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(ex.Message);
            }
        }

        public ActionResult Create()
        {
            return View(new TSql_Countrys { AddIsActive = true });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(TSql_Countrys model)
        {
            NormalizeCountry(model);
            ValidateCountry(model, isCreate: true);

            if (!ModelState.IsValid)
                return View(model);

            var userId = IntranetAuditHelper.ResolveCurrentUserId(User);
            var now = DateTime.Now;

            var entity = new TSql_Countrys
            {
                TextLabel = model.TextLabel?.Trim(),
                TextIso2 = NullIfWhite(model.TextIso2),
                TextIso3 = NullIfWhite(model.TextIso3),
                NumberIso = NullIfWhite(model.NumberIso),
                TextFlag = NullIfWhite(model.TextFlag),
                AddIsActive = model.AddIsActive,
                LinkMadeBy = userId,
                LinkChangeBy = userId,
                AddDateMade = now,
                AddLastDateChange = now,
                Ntimeschanged = 0
            };

            db.TSql_Countrys.Add(entity);
            db.SaveChanges();

            TempData["ToastMessage"] = string.Format(Country.ToastMessage_CountryCreated, entity.TextLabel ?? "");
            return RedirectToAction(nameof(Index));
        }

        public ActionResult Edit(long id)
        {
            var entity = db.TSql_Countrys.FirstOrDefault(c => c.IdObject == id);
            if (entity == null)
            {
                TempData["ToastMessage"] = Country.Err_CountryNotFound;
                return RedirectToAction(nameof(Index));
            }

            return View(entity);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(TSql_Countrys model)
        {
            if (model == null)
            {
                TempData["ToastMessage"] = Country.Err_CountryNotFound;
                return RedirectToAction(nameof(Index));
            }

            var entity = db.TSql_Countrys.FirstOrDefault(c => c.IdObject == model.IdObject);
            if (entity == null)
            {
                TempData["ToastMessage"] = Country.Err_CountryNotFound;
                return RedirectToAction(nameof(Index));
            }

            entity.TextLabel = model.TextLabel?.Trim();
            entity.TextIso2 = NullIfWhite(model.TextIso2);
            entity.TextIso3 = NullIfWhite(model.TextIso3);
            entity.NumberIso = NullIfWhite(model.NumberIso);
            entity.TextFlag = NullIfWhite(model.TextFlag);
            entity.AddIsActive = model.AddIsActive;

            NormalizeCountry(entity);
            ValidateCountry(entity, isCreate: false, excludeId: entity.IdObject);

            if (!ModelState.IsValid)
                return View(entity);

            var userId = IntranetAuditHelper.ResolveCurrentUserId(User);
            entity.LinkChangeBy = userId;
            entity.AddLastDateChange = DateTime.Now;
            entity.Ntimeschanged++;

            db.SaveChanges();

            TempData["ToastMessage"] = string.Format(Country.ToastMessage_CountryUpdated, entity.TextLabel ?? "");
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public JsonResult DeleteCountry(long id)
        {
            var entity = db.TSql_Countrys.FirstOrDefault(c => c.IdObject == id);
            if (entity == null)
            {
                return Json(new { IsOk = false, Message = Country.Err_CountryNotFound });
            }

            if (db.TSql_language.Any(l => !l.Is_Delete && l.LinkCountry == id))
            {
                return Json(new { IsOk = false, Message = Country.Err_CannotDeleteHasLanguages });
            }

            if (db.TSql_Company.Any(c => !c.BitIsDeleted && c.LinkCountry == id))
            {
                return Json(new { IsOk = false, Message = Country.Err_CannotDeleteHasCompanies });
            }

            var nombre = entity.TextLabel ?? "";
            db.TSql_Countrys.Remove(entity);
            db.SaveChanges();

            return Json(new
            {
                IsOk = true,
                Message = string.Format(Country.ToastMessage_CountryDeleted, nombre)
            });
        }

        private static string NullIfWhite(string s)
        {
            return string.IsNullOrWhiteSpace(s) ? null : s.Trim();
        }

        private void NormalizeCountry(TSql_Countrys model)
        {
            if (model == null) return;
            model.TextLabel = model.TextLabel?.Trim();
        }

        private void ValidateCountry(TSql_Countrys model, bool isCreate, long? excludeId = null)
        {
            if (string.IsNullOrWhiteSpace(model.TextLabel))
                ModelState.AddModelError(nameof(model.TextLabel), Country.Val_NameRequired);
            else if (model.TextLabel.Length > 500)
                ModelState.AddModelError(nameof(model.TextLabel), Country.Val_NameTooLong);

            if (!string.IsNullOrEmpty(model.TextIso2) && model.TextIso2.Length > 50)
                ModelState.AddModelError(nameof(model.TextIso2), Country.Val_Iso2TooLong);
            if (!string.IsNullOrEmpty(model.TextIso3) && model.TextIso3.Length > 50)
                ModelState.AddModelError(nameof(model.TextIso3), Country.Val_Iso3TooLong);
            if (!string.IsNullOrEmpty(model.NumberIso) && model.NumberIso.Length > 50)
                ModelState.AddModelError(nameof(model.NumberIso), Country.Val_NumberIsoTooLong);
            if (!string.IsNullOrEmpty(model.TextFlag) && model.TextFlag.Length > 200)
                ModelState.AddModelError(nameof(model.TextFlag), Country.Val_FlagPathTooLong);

            if (!ModelState.IsValid) return;

            var label = model.TextLabel.Trim();
            var dup = db.TSql_Countrys.Any(c =>
                c.TextLabel.ToLower() == label.ToLower()
                && (!excludeId.HasValue || c.IdObject != excludeId.Value));

            if (dup)
            {
                ModelState.AddModelError(
                    nameof(model.TextLabel),
                    isCreate ? Country.Val_DuplicateNameCreate : Country.Val_DuplicateNameEdit);
            }
        }
    }
}
