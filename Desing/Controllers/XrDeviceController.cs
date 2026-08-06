using DataTables.Mvc;
using Desing.Helpers;
using Desing.Models.TandemXr;
using Desing.Resources;
using Microsoft.AspNet.Identity;
using System;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Desing.Controllers
{
    /// <summary>
    /// CRUD Intranet de dispositivos XR (Quest / tablet) para «Enviar a XR».
    /// Persistencia vía SQL (<see cref="XrDeviceQueries"/>) hasta Update Model EDMX.
    /// </summary>
    [Authorize]
    public class XrDeviceController : BaseController
    {
        public ActionResult Index()
        {
            EnsureTablesOrToast();
            return View();
        }

        public ActionResult Details(long id)
        {
            if (!XrDeviceQueries.TableExists(db.Database))
            {
                return TablesMissingRedirect();
            }

            var entity = XrDeviceQueries.GetById(db.Database, id);
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

            return View(entity);
        }

        [OutputCache(Duration = 1)]
        public JsonResult ListXrDevices([ModelBinder(typeof(DataTablesBinder))] IDataTablesRequest requestModel)
        {
            try
            {
                if (!XrDeviceQueries.TableExists(db.Database))
                {
                    return Json(DataTablesMvcJson.Create(requestModel.Draw, new object[0], 0, 0),
                        JsonRequestBehavior.AllowGet);
                }

                var all = XrDeviceQueries.ListActive(db.Database);
                var totalCount = all.Count;

                var filtered = all.AsEnumerable();
                if (!string.IsNullOrEmpty(requestModel.Search.Value))
                {
                    var value = requestModel.Search.Value.Trim().ToLowerInvariant();
                    filtered = filtered.Where(p =>
                        (p.TextLabel ?? "").ToLowerInvariant().Contains(value)
                        || (p.TextDeviceType ?? "").ToLowerInvariant().Contains(value)
                        || (p.TextPairingCode ?? "").ToLowerInvariant().Contains(value));
                }

                var filteredList = filtered.ToList();
                var filteredCount = filteredList.Count;

                var sorted = requestModel.Columns.GetSortedColumns().FirstOrDefault();
                var asc = sorted == null || sorted.SortDirection == Column.OrderDirection.Ascendant;
                var col = sorted != null ? sorted.Data : "TextLabel";
                switch (col)
                {
                    case "deviceTypePlain":
                    case "deviceTypeBadge":
                        filteredList = asc
                            ? filteredList.OrderBy(x => x.TextDeviceType).ToList()
                            : filteredList.OrderByDescending(x => x.TextDeviceType).ToList();
                        break;
                    case "TextPairingCode":
                        filteredList = asc
                            ? filteredList.OrderBy(x => x.TextPairingCode).ToList()
                            : filteredList.OrderByDescending(x => x.TextPairingCode).ToList();
                        break;
                    case "Is_Active":
                    case "activeBadge":
                        filteredList = asc
                            ? filteredList.OrderBy(x => x.Is_Active).ToList()
                            : filteredList.OrderByDescending(x => x.Is_Active).ToList();
                        break;
                    case "pairedBadge":
                        filteredList = asc
                            ? filteredList.OrderBy(x => x.Is_Paired).ToList()
                            : filteredList.OrderByDescending(x => x.Is_Paired).ToList();
                        break;
                    default:
                        filteredList = asc
                            ? filteredList.OrderBy(x => x.TextLabel).ToList()
                            : filteredList.OrderByDescending(x => x.TextLabel).ToList();
                        break;
                }

                var page = filteredList
                    .Skip(requestModel.Start)
                    .Take(requestModel.Length > 0 ? requestModel.Length : filteredList.Count)
                    .ToList();

                var ttOpen = HttpUtility.HtmlAttributeEncode(XrDevice.List_LinkOpenTooltip);
                var ttEdit = HttpUtility.HtmlAttributeEncode(XrDevice.List_LinkEditTooltip);
                var ttDelete = HttpUtility.HtmlAttributeEncode(XrDevice.List_LinkDeleteTooltip);
                var lblActive = HttpUtility.HtmlEncode(XrDevice.State_Active);
                var lblInactive = HttpUtility.HtmlEncode(XrDevice.State_Inactive);
                var lblPaired = HttpUtility.HtmlEncode(XrDevice.State_Paired);
                var lblUnpaired = HttpUtility.HtmlEncode(XrDevice.State_Unpaired);
                var lblQuest = HttpUtility.HtmlEncode(XrDevice.Type_Quest);
                var lblTablet = HttpUtility.HtmlEncode(XrDevice.Type_Tablet);

                var data = page.Select(p =>
                {
                    var namePlain = p.TextLabel ?? "";
                    var nameCell =
                        "<a title=\"" + ttOpen + "\" href=\"" +
                        Url.Action("Details", new { id = p.IdObject }) + "\">" +
                        HttpUtility.HtmlEncode(namePlain) + "</a>";

                    var typeLabel = string.Equals(p.TextDeviceType, XrDeviceTypes.Tablet, StringComparison.OrdinalIgnoreCase)
                        ? lblTablet
                        : lblQuest;
                    var typeBadge = "<span class=\"badge bg-label-info\">" + typeLabel + "</span>";

                    var activeBadge = p.Is_Active
                        ? "<span class=\"badge bg-label-success\">" + lblActive + "</span>"
                        : "<span class=\"badge bg-label-secondary\">" + lblInactive + "</span>";

                    var pairedBadge = p.Is_Paired
                        ? "<span class=\"badge bg-label-success\">" + lblPaired + "</span>"
                        : "<span class=\"badge bg-label-warning\">" + lblUnpaired + "</span>";

                    var editBtn =
                        "<a title=\"" + ttEdit + "\" href=\"" +
                        Url.Action("Edit", new { id = p.IdObject }) +
                        "\" class=\"btn btn-warning btn-xs\"><span class=\"fas fa-edit\" aria-hidden=\"true\"></span></a>";
                    var deleteBtn =
                        "<a title=\"" + ttDelete + "\" href=\"#\" onclick=\"DeleteXrDevice(" + p.IdObject +
                        "); return false;\" class=\"btn btn-danger btn-xs\"><span class=\"fas fa-trash-alt\" aria-hidden=\"true\"></span></a>";
                    var rowActions =
                        "<div class=\"d-inline-flex align-items-center gap-2\" role=\"group\">" +
                        editBtn + deleteBtn + "</div>";

                    return new
                    {
                        IdObject = p.IdObject,
                        TextLabel = nameCell,
                        TextLabelPlain = namePlain,
                        deviceTypeBadge = typeBadge,
                        deviceTypePlain = p.TextDeviceType ?? "",
                        TextPairingCode = HttpUtility.HtmlEncode(p.TextPairingCode ?? ""),
                        pairedBadge,
                        activeBadge,
                        Is_Active = p.Is_Active,
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
            if (!XrDeviceQueries.TableExists(db.Database))
            {
                return TablesMissingRedirect();
            }

            var entity = new XrDeviceEntity
            {
                Is_Active = true,
                TextDeviceType = XrDeviceTypes.Quest,
                TextPairingCode = XrDeviceQueries.NewPairingCode()
            };
            return View(entity);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(
            [Bind(Include = "TextLabel,TextDeviceType,TextPairingCode,TextNotes,Is_Active")] XrDeviceEntity entity)
        {
            if (!XrDeviceQueries.TableExists(db.Database))
            {
                return TablesMissingRedirect();
            }

            if (entity == null)
            {
                entity = new XrDeviceEntity { Is_Active = true };
            }

            TrimStrings(entity);
            if (string.IsNullOrWhiteSpace(entity.TextPairingCode))
            {
                entity.TextPairingCode = XrDeviceQueries.NewPairingCode();
            }

            ValidateEntity(entity, null);

            if (!ModelState.IsValid)
            {
                return View(entity);
            }

            var userId = User.Identity.GetUserId() ?? "";
            entity.TextLabel = (entity.TextLabel ?? "").Trim();
            entity.TextDeviceType = NormalizeType(entity.TextDeviceType);
            entity.TextPairingCode = (entity.TextPairingCode ?? "").Trim().ToUpperInvariant();
            entity.TextNotes = string.IsNullOrWhiteSpace(entity.TextNotes) ? null : entity.TextNotes.Trim();
            entity.Is_Paired = false;
            entity.LinkMadeBy = userId;
            entity.AddDateMade = DateTime.Now;
            entity.Is_Delete = false;
            entity.Ntimeschanged = 0;

            XrDeviceQueries.Insert(db.Database, entity);

            TempData["ToastType"] = "Act";
            TempData["ToastTitle"] = XrDevice.ToastTitle_Create;
            TempData["ToastMessage"] = string.Format(XrDevice.ToastMessage_Created, entity.TextLabel);
            return RedirectToAction("Index");
        }

        public ActionResult Edit(long id)
        {
            if (!XrDeviceQueries.TableExists(db.Database))
            {
                return TablesMissingRedirect();
            }

            var entity = XrDeviceQueries.GetById(db.Database, id);
            if (entity == null)
            {
                TempData["ToastType"] = "Error";
                TempData["ToastTitle"] = XrDevice.ToastTitle_Edit;
                TempData["ToastMessage"] = XrDevice.Err_NotFound;
                return RedirectToAction("Index");
            }

            return View(entity);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(
            [Bind(Include = "IdObject,TextLabel,TextDeviceType,TextPairingCode,TextNotes,Is_Active")] XrDeviceEntity posted)
        {
            if (!XrDeviceQueries.TableExists(db.Database))
            {
                return TablesMissingRedirect();
            }

            if (posted == null)
            {
                TempData["ToastType"] = "Error";
                TempData["ToastTitle"] = XrDevice.ToastTitle_Edit;
                TempData["ToastMessage"] = XrDevice.Err_NotFound;
                return RedirectToAction("Index");
            }

            var entity = XrDeviceQueries.GetById(db.Database, posted.IdObject);
            if (entity == null)
            {
                TempData["ToastType"] = "Error";
                TempData["ToastTitle"] = XrDevice.ToastTitle_Edit;
                TempData["ToastMessage"] = XrDevice.Err_NotFound;
                return RedirectToAction("Index");
            }

            TrimStrings(posted);
            ValidateEntity(posted, posted.IdObject);

            if (!ModelState.IsValid)
            {
                entity.TextLabel = posted.TextLabel;
                entity.TextDeviceType = posted.TextDeviceType;
                entity.TextPairingCode = posted.TextPairingCode;
                entity.TextNotes = posted.TextNotes;
                entity.Is_Active = posted.Is_Active;
                return View(entity);
            }

            entity.TextLabel = (posted.TextLabel ?? "").Trim();
            entity.TextDeviceType = NormalizeType(posted.TextDeviceType);
            entity.TextPairingCode = (posted.TextPairingCode ?? "").Trim().ToUpperInvariant();
            entity.TextNotes = string.IsNullOrWhiteSpace(posted.TextNotes) ? null : posted.TextNotes.Trim();
            entity.Is_Active = posted.Is_Active;
            entity.LinModifiedBy = User.Identity.GetUserId();
            entity.AddLastDateChange = DateTime.Now;
            entity.Ntimeschanged += 1;

            XrDeviceQueries.Update(db.Database, entity);

            TempData["ToastType"] = "Act";
            TempData["ToastTitle"] = XrDevice.ToastTitle_Edit;
            TempData["ToastMessage"] = string.Format(XrDevice.ToastMessage_Updated, entity.TextLabel);
            return RedirectToAction("Index");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public JsonResult DeleteXrDevice(long id)
        {
            if (!XrDeviceQueries.TableExists(db.Database))
            {
                return Json(new { IsOk = false, Message = XrDevice.Err_TablesMissing });
            }

            var entity = XrDeviceQueries.GetById(db.Database, id);
            if (entity == null)
            {
                return Json(new { IsOk = false, Message = XrDevice.Err_NotFound });
            }

            var nombre = entity.TextLabel ?? "";
            entity.LinModifiedBy = User.Identity.GetUserId();
            entity.AddLastDateChange = DateTime.Now;
            entity.Ntimeschanged += 1;
            XrDeviceQueries.SoftDelete(db.Database, entity);

            return Json(new
            {
                IsOk = true,
                Message = string.Format(XrDevice.ToastMessage_Deleted, nombre)
            });
        }

        private void EnsureTablesOrToast()
        {
            if (!XrDeviceQueries.TableExists(db.Database))
            {
                TempData["ToastType"] = "Error";
                TempData["ToastTitle"] = XrDevice.ToastTitle_Create;
                TempData["ToastMessage"] = XrDevice.Err_TablesMissing;
            }
        }

        private ActionResult TablesMissingRedirect()
        {
            TempData["ToastType"] = "Error";
            TempData["ToastTitle"] = XrDevice.ToastTitle_Create;
            TempData["ToastMessage"] = XrDevice.Err_TablesMissing;
            return RedirectToAction("Index", "Home");
        }

        private static string NormalizeType(string type)
        {
            if (string.Equals(type, XrDeviceTypes.Tablet, StringComparison.OrdinalIgnoreCase))
            {
                return XrDeviceTypes.Tablet;
            }

            return XrDeviceTypes.Quest;
        }

        private static void TrimStrings(XrDeviceEntity model)
        {
            if (model == null) return;
            if (!string.IsNullOrEmpty(model.TextLabel)) model.TextLabel = model.TextLabel.Trim();
            if (!string.IsNullOrEmpty(model.TextDeviceType)) model.TextDeviceType = model.TextDeviceType.Trim();
            if (!string.IsNullOrEmpty(model.TextPairingCode)) model.TextPairingCode = model.TextPairingCode.Trim();
            if (!string.IsNullOrEmpty(model.TextNotes)) model.TextNotes = model.TextNotes.Trim();
        }

        private void ValidateEntity(XrDeviceEntity model, long? excludeId)
        {
            ClearFieldErrors("TextLabel");
            if (model == null || string.IsNullOrWhiteSpace(model.TextLabel))
            {
                ModelState.AddModelError("TextLabel", XrDevice.Val_NameRequired);
            }
            else if (model.TextLabel.Trim().Length > 500)
            {
                ModelState.AddModelError("TextLabel", XrDevice.Val_NameTooLong);
            }
            else if (XrDeviceQueries.LabelExists(db.Database, model.TextLabel, excludeId))
            {
                ModelState.AddModelError("TextLabel", excludeId.HasValue
                    ? XrDevice.Val_DuplicateNameEdit
                    : XrDevice.Val_DuplicateNameCreate);
            }

            ClearFieldErrors("TextDeviceType");
            var t = NormalizeType(model?.TextDeviceType);
            if (model != null) model.TextDeviceType = t;

            ClearFieldErrors("TextPairingCode");
            var code = (model?.TextPairingCode ?? "").Trim();
            if (code.Length == 0)
            {
                ModelState.AddModelError("TextPairingCode", XrDevice.Val_PairingRequired);
            }
            else if (code.Length > 50)
            {
                ModelState.AddModelError("TextPairingCode", XrDevice.Val_PairingTooLong);
            }
            else if (XrDeviceQueries.PairingCodeExists(db.Database, code.ToUpperInvariant(), excludeId))
            {
                ModelState.AddModelError("TextPairingCode", XrDevice.Val_DuplicatePairing);
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
