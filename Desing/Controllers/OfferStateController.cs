using DAL;
using DataTables.Mvc;
using Desing.Helpers;
using Desing.Resources;
using System;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.Mvc;

namespace Desing.Controllers
{
    /// <summary>
    /// CRUD del catálogo <c>TSql_OfferState</c> (estados de oferta comercial).
    /// Patrón Materio + DataTables como <see cref="ExtensionController"/>.
    /// Borrado bloqueado si existen ofertas (<c>TSql_Offers.LinkOfferState</c>) no borradas.
    /// </summary>
    [Authorize]
    public class OfferStateController : BaseController
    {
        private static readonly Regex HexColorRegex = new Regex(
            "^#(?:[0-9a-fA-F]{3}|[0-9a-fA-F]{6})$",
            RegexOptions.Compiled);

        public ActionResult Index()
        {
            return View();
        }

        public ActionResult Details(long id)
        {
            var entity = LoadEntity(id);
            if (entity == null)
            {
                return HttpNotFound();
            }

            ViewBag.Audit = IntranetAuditHelper.BuildDisplay(
                db,
                entity.LinkMadeBy,
                entity.LinModifiedBy,
                entity.AddChangeBy,
                entity.AddDateMade,
                null,
                entity.Ntimeschanged);

            return View(entity);
        }

        [OutputCache(Duration = 1)]
        public JsonResult ListOfferStates([ModelBinder(typeof(DataTablesBinder))] IDataTablesRequest requestModel)
        {
            try
            {
                var query = db.TSql_OfferState
                    .Where(s => !s.Is_Delete)
                    .Select(s => new
                    {
                        s.IdObject,
                        s.TextLabel,
                        s.AddColor,
                        s.Is_Active,
                        s.Is_Delete
                    });

                var totalCount = query.Count();

                if (!string.IsNullOrEmpty(requestModel.Search.Value))
                {
                    var value = requestModel.Search.Value.Trim();
                    query = query.Where(p =>
                        (p.TextLabel ?? "").Contains(value)
                        || (p.AddColor ?? "").Contains(value));
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
                        case "colorPlain":
                        case "colorCell":
                            orderColumn = "AddColor";
                            break;
                        case "Is_Active":
                        case "activeBadge":
                            orderColumn = "Is_Active";
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

                var idsWithOffers = db.TSql_Offers
                    .Where(o => !o.Is_Delete && ids.Contains(o.LinkOfferState))
                    .Select(o => o.LinkOfferState)
                    .Distinct()
                    .ToList()
                    .ToHashSet();

                var ttOpen = HttpUtility.HtmlAttributeEncode(OfferState.List_LinkOpenTooltip);
                var ttEdit = HttpUtility.HtmlAttributeEncode(OfferState.List_LinkEditTooltip);
                var ttDelete = HttpUtility.HtmlAttributeEncode(OfferState.List_LinkDeleteTooltip);
                var ttDeleteLocked = HttpUtility.HtmlAttributeEncode(OfferState.List_LinkDeleteLockedOffersTooltip);
                var lblActive = HttpUtility.HtmlEncode(OfferState.State_Active);
                var lblInactive = HttpUtility.HtmlEncode(OfferState.State_Inactive);

                var data = rows.Select(p =>
                {
                    var namePlain = p.TextLabel ?? "";
                    var nameCell =
                        "<a title=\"" + ttOpen + "\" href=\"" +
                        Url.Action("Details", new { id = p.IdObject }) + "\">" +
                        HttpUtility.HtmlEncode(namePlain) + "</a>";

                    var activeBadge = p.Is_Active
                        ? "<span class=\"badge bg-label-success\">" + lblActive + "</span>"
                        : "<span class=\"badge bg-label-secondary\">" + lblInactive + "</span>";

                    var editBtn =
                        "<a title=\"" + ttEdit + "\" href=\"" +
                        Url.Action("Edit", new { id = p.IdObject }) +
                        "\" class=\"btn btn-warning btn-xs\"><span class=\"fas fa-edit\" aria-hidden=\"true\"></span></a>";

                    string deleteBtn;
                    if (idsWithOffers.Contains(p.IdObject))
                    {
                        deleteBtn =
                            "<a title=\"" + ttDeleteLocked + "\" class=\"btn btn-secondary btn-xs disabled\" aria-disabled=\"true\" tabindex=\"-1\">" +
                            "<span class=\"fas fa-trash-alt\" aria-hidden=\"true\"></span></a>";
                    }
                    else
                    {
                        deleteBtn =
                            "<a title=\"" + ttDelete + "\" href=\"#\" onclick=\"DeleteOfferState(" + p.IdObject +
                            "); return false;\" class=\"btn btn-danger btn-xs\"><span class=\"fas fa-trash-alt\" aria-hidden=\"true\"></span></a>";
                    }

                    var rowActions =
                        "<div class=\"d-inline-flex align-items-center gap-2\" role=\"group\">" +
                        editBtn + deleteBtn + "</div>";

                    var colorPlain = (p.AddColor ?? "").Trim();

                    return new
                    {
                        IdObject = p.IdObject,
                        TextLabel = nameCell,
                        TextLabelPlain = namePlain,
                        colorCell = BuildColorCell(colorPlain),
                        colorPlain,
                        Is_Active = p.Is_Active,
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
            var entity = new TSql_OfferState { Is_Active = true };
            return View(entity);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(
            [Bind(Include = "TextLabel,AddColor,Is_Active")] TSql_OfferState entity)
        {
            if (entity == null)
            {
                entity = new TSql_OfferState { Is_Active = true };
            }

            TrimOfferStateStrings(entity);
            ValidateOfferState(entity, null);

            if (!ModelState.IsValid)
            {
                return View(entity);
            }

            var nueva = new TSql_OfferState
            {
                TextLabel = (entity.TextLabel ?? string.Empty).Trim(),
                AddColor = NormalizeColor(entity.AddColor),
                Is_Active = entity.Is_Active
            };

            IntranetAuditHelper.SetAuditOnCreate(nueva, User);
            db.TSql_OfferState.Add(nueva);
            db.SaveChanges();

            TempData["ToastType"] = "Act";
            TempData["ToastTitle"] = OfferState.ToastTitle_CreateOfferState;
            TempData["ToastMessage"] = string.Format(OfferState.ToastMessage_OfferStateCreated, nueva.TextLabel);
            return RedirectToAction("Index");
        }

        public ActionResult Edit(long id)
        {
            var entity = LoadEntity(id);
            if (entity == null)
            {
                TempData["ToastType"] = "Error";
                TempData["ToastTitle"] = OfferState.ToastTitle_EditOfferState;
                TempData["ToastMessage"] = OfferState.Err_OfferStateNotFound;
                return RedirectToAction("Index");
            }

            return View(entity);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(
            [Bind(Include = "IdObject,TextLabel,AddColor,Is_Active")] TSql_OfferState posted)
        {
            if (posted == null)
            {
                TempData["ToastType"] = "Error";
                TempData["ToastTitle"] = OfferState.ToastTitle_EditOfferState;
                TempData["ToastMessage"] = OfferState.Err_OfferStateNotFound;
                return RedirectToAction("Index");
            }

            var entity = LoadEntity(posted.IdObject);
            if (entity == null)
            {
                TempData["ToastType"] = "Error";
                TempData["ToastTitle"] = OfferState.ToastTitle_EditOfferState;
                TempData["ToastMessage"] = OfferState.Err_OfferStateNotFound;
                return RedirectToAction("Index");
            }

            TrimOfferStateStrings(posted);
            ValidateOfferState(posted, posted.IdObject);

            if (!ModelState.IsValid)
            {
                entity.TextLabel = posted.TextLabel;
                entity.AddColor = posted.AddColor;
                entity.Is_Active = posted.Is_Active;
                return View(entity);
            }

            entity.TextLabel = (posted.TextLabel ?? string.Empty).Trim();
            entity.AddColor = NormalizeColor(posted.AddColor);
            entity.Is_Active = posted.Is_Active;
            IntranetAuditHelper.SetAuditOnUpdate(entity, User);
            db.SaveChanges();

            TempData["ToastType"] = "Act";
            TempData["ToastTitle"] = OfferState.ToastTitle_EditOfferState;
            TempData["ToastMessage"] = string.Format(OfferState.ToastMessage_OfferStateUpdated, entity.TextLabel);
            return RedirectToAction("Index");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public JsonResult DeleteOfferState(long id)
        {
            var entity = LoadEntity(id);
            if (entity == null)
            {
                return Json(new { IsOk = false, Message = OfferState.Err_OfferStateNotFound });
            }

            if (db.TSql_Offers.Any(o => !o.Is_Delete && o.LinkOfferState == id))
            {
                return Json(new { IsOk = false, Message = OfferState.Err_CannotDeleteHasOffers });
            }

            var nombre = entity.TextLabel ?? "";
            IntranetAuditHelper.SetAuditOnDelete(entity, User);
            db.SaveChanges();

            return Json(new
            {
                IsOk = true,
                Message = string.Format(OfferState.ToastMessage_OfferStateDeleted, nombre)
            });
        }

        private TSql_OfferState LoadEntity(long id)
        {
            return db.TSql_OfferState.FirstOrDefault(x => x.IdObject == id && !x.Is_Delete);
        }

        private static string NormalizeColor(string color)
        {
            if (string.IsNullOrWhiteSpace(color))
            {
                return null;
            }

            var t = color.Trim();
            return t.Length == 0 ? null : t;
        }

        private static void TrimOfferStateStrings(TSql_OfferState model)
        {
            if (model == null)
            {
                return;
            }

            if (!string.IsNullOrEmpty(model.TextLabel))
            {
                model.TextLabel = model.TextLabel.Trim();
            }

            model.AddColor = NormalizeColor(model.AddColor);
        }

        private void ValidateOfferState(TSql_OfferState model, long? excludeId)
        {
            ClearFieldErrors("TextLabel");
            if (model == null || string.IsNullOrWhiteSpace(model.TextLabel))
            {
                ModelState.AddModelError("TextLabel", OfferState.Val_NameRequired);
                return;
            }

            var label = model.TextLabel.Trim();
            if (label.Length > 500)
            {
                ModelState.AddModelError("TextLabel", OfferState.Val_NameTooLong);
                return;
            }

            var labelLower = label.ToLower();
            var existsQuery = db.TSql_OfferState.Where(x =>
                !x.Is_Delete &&
                x.TextLabel.ToLower() == labelLower);

            if (excludeId.HasValue)
            {
                existsQuery = existsQuery.Where(x => x.IdObject != excludeId.Value);
            }

            if (existsQuery.Any())
            {
                ModelState.AddModelError("TextLabel", excludeId.HasValue
                    ? OfferState.Val_DuplicateNameEdit
                    : OfferState.Val_DuplicateNameCreate);
            }

            ClearFieldErrors("AddColor");
            var c = model.AddColor;
            if (!string.IsNullOrWhiteSpace(c))
            {
                var t = c.Trim();
                if (!HexColorRegex.IsMatch(t))
                {
                    ModelState.AddModelError("AddColor", OfferState.Val_ColorInvalidHex);
                }
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

        private string BuildColorCell(string colorPlain)
        {
            if (string.IsNullOrWhiteSpace(colorPlain))
            {
                return "<span class=\"text-muted\">" + HttpUtility.HtmlEncode(OfferState.Details_NoColor) + "</span>";
            }

            var safe = HttpUtility.HtmlEncode(colorPlain);
            var swatch = "";
            if (HexColorRegex.IsMatch(colorPlain.Trim()))
            {
                var css = HttpUtility.HtmlAttributeEncode(colorPlain.Trim());
                swatch =
                    "<span class=\"d-inline-block rounded border me-2 align-middle\" " +
                    "style=\"width:18px;height:18px;background-color:" + css +
                    ";vertical-align:middle\" title=\"" + safe + "\"></span>";
            }

            return swatch + "<span class=\"align-middle\">" + safe + "</span>";
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
