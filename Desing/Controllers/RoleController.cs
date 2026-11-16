using DataTables.Mvc;
using Desing.Helpers;
using Desing.Models;
using Desing.Resources;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Web;
using System.Web.Mvc;

namespace Desing.Controllers
{
    /// <summary>
    /// CRUD de roles (<c>TSql_Roles_V2</c>) con sus funciones (<c>TSql_RoleFunction</c>) y
    /// usuarios (<c>TSql_UserRole</c>). Patrón Materio + DataTables como <see cref="OfferStateController"/>.
    /// Protegido por la función «Role» (semilla 2026-11-16_seed_TSql_Roles_V2_Functions.sql).
    /// </summary>
    [Authorize]
    public class RoleController : BaseController
    {
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult Details(long id)
        {
            var role = SecurityRepository.GetRole(db, id);
            if (role == null)
            {
                return HttpNotFound();
            }

            var functionIds = new HashSet<long>(SecurityRepository.GetRoleFunctionIds(db, id));
            var userIds = new HashSet<string>(SecurityRepository.GetRoleUserIds(db, id), StringComparer.OrdinalIgnoreCase);
            ViewBag.Functions = SecurityRepository.ListFunctions(db).Where(f => functionIds.Contains(f.IdObject)).ToList();
            ViewBag.Users = SecurityRepository.ListUsers(db).Where(u => userIds.Contains(u.Id)).ToList();
            ViewBag.Audit = IntranetAuditHelper.BuildDisplay(
                db, role.LinkMadeBy, role.LinModifiedBy, role.AddDateMade, role.AddLastDateChange, role.Ntimeschanged);
            return View(role);
        }

        [OutputCache(Duration = 1)]
        public JsonResult ListRoles([ModelBinder(typeof(DataTablesBinder))] IDataTablesRequest requestModel)
        {
            try
            {
                var query = SecurityRepository.ListRoles(db).AsQueryable();
                var totalCount = query.Count();

                if (!string.IsNullOrEmpty(requestModel.Search.Value))
                {
                    var value = requestModel.Search.Value.Trim();
                    query = query.Where(p =>
                        (p.TextLabel ?? "").IndexOf(value, StringComparison.OrdinalIgnoreCase) >= 0
                        || (p.TextCode ?? "").IndexOf(value, StringComparison.OrdinalIgnoreCase) >= 0
                        || (p.TextDescription ?? "").IndexOf(value, StringComparison.OrdinalIgnoreCase) >= 0);
                }

                var filteredCount = query.Count();

                var orderByString = string.Empty;
                foreach (var column in requestModel.Columns.GetSortedColumns())
                {
                    string orderColumn;
                    switch (column.Data)
                    {
                        case "TextCode": orderColumn = "TextCode"; break;
                        case "NFunctions": orderColumn = "NFunctions"; break;
                        case "NUsers": orderColumn = "NUsers"; break;
                        case "activeBadge": orderColumn = "Is_Active"; break;
                        default: orderColumn = "TextLabel"; break;
                    }
                    orderByString += orderByString != string.Empty ? "," : "";
                    orderByString += orderColumn + (column.SortDirection == Column.OrderDirection.Ascendant ? " asc" : " desc");
                }

                query = query.OrderBy(string.IsNullOrEmpty(orderByString) ? "TextLabel asc" : orderByString);
                query = query.ApplyDataTablesPaging(requestModel.Start, requestModel.Length);

                var ttOpen = HttpUtility.HtmlAttributeEncode(Security.Role_List_OpenTooltip);
                var ttEdit = HttpUtility.HtmlAttributeEncode(Security.Role_List_EditTooltip);
                var ttDelete = HttpUtility.HtmlAttributeEncode(Security.Role_List_DeleteTooltip);
                var ttLocked = HttpUtility.HtmlAttributeEncode(Security.Role_List_DeleteLockedTooltip);
                var lblActive = HttpUtility.HtmlEncode(Security.State_Active);
                var lblInactive = HttpUtility.HtmlEncode(Security.State_Inactive);
                var lblSystem = HttpUtility.HtmlEncode(Security.Role_Badge_System);
                var lblAll = HttpUtility.HtmlEncode(Security.Role_Badge_AllFunctions);

                var data = query.ToList().Select(p =>
                {
                    var namePlain = p.TextLabel ?? "";
                    var nameCell = "<a title=\"" + ttOpen + "\" href=\"" + Url.Action("Details", new { id = p.IdObject }) + "\">" +
                                   HttpUtility.HtmlEncode(namePlain) + "</a>";
                    if (p.Is_System)
                    {
                        nameCell += " <span class=\"badge bg-label-info ms-1\">" + lblSystem + "</span>";
                    }

                    var activeBadge = p.Is_Active
                        ? "<span class=\"badge bg-label-success\">" + lblActive + "</span>"
                        : "<span class=\"badge bg-label-secondary\">" + lblInactive + "</span>";

                    var functionsCell = p.Is_AllFunctions
                        ? "<span class=\"badge bg-label-primary\">" + lblAll + "</span>"
                        : p.NFunctions.ToString();

                    var editBtn = "<a title=\"" + ttEdit + "\" href=\"" + Url.Action("Edit", new { id = p.IdObject }) +
                                  "\" class=\"btn btn-warning btn-xs\"><span class=\"fas fa-edit\" aria-hidden=\"true\"></span></a>";
                    var deleteBtn = p.Is_System
                        ? "<a title=\"" + ttLocked + "\" class=\"btn btn-secondary btn-xs disabled\" aria-disabled=\"true\" tabindex=\"-1\">" +
                          "<span class=\"fas fa-trash-alt\" aria-hidden=\"true\"></span></a>"
                        : "<a title=\"" + ttDelete + "\" href=\"#\" onclick=\"DeleteRole(" + p.IdObject +
                          "); return false;\" class=\"btn btn-danger btn-xs\"><span class=\"fas fa-trash-alt\" aria-hidden=\"true\"></span></a>";

                    return new
                    {
                        IdObject = p.IdObject,
                        TextLabel = nameCell,
                        TextLabelPlain = namePlain,
                        TextCode = HttpUtility.HtmlEncode(p.TextCode ?? ""),
                        NFunctions = functionsCell,
                        NFunctionsPlain = p.Is_AllFunctions ? Security.Role_Badge_AllFunctions : p.NFunctions.ToString(),
                        NUsers = p.NUsers,
                        Is_Active = p.Is_Active,
                        activeBadge,
                        rowActions = "<div class=\"d-inline-flex align-items-center gap-2\" role=\"group\">" + editBtn + deleteBtn + "</div>"
                    };
                }).ToList();

                return Json(DataTablesMvcJson.Create(requestModel.Draw, data, filteredCount, totalCount), JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(ex.Message);
            }
        }

        public ActionResult Create()
        {
            var vm = BuildForm(new SecurityRoleRow { Is_Active = true }, new List<long>(), new List<string>());
            return Request.IsAjaxRequest() ? (ActionResult)PartialView("_CreateModal", vm) : View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(SecurityRoleFormViewModel vm)
        {
            var posted = Normalize(vm?.Role);
            Validate(posted, null);
            if (!ModelState.IsValid)
            {
                var form = BuildForm(posted, vm?.SelectedFunctionIds, vm?.SelectedUserIds);
                return Request.IsAjaxRequest() ? (ActionResult)PartialView("_CreateModal", form) : View(form);
            }

            var userId = IntranetAuditHelper.ResolveCurrentUserId(User);
            var id = SecurityRepository.InsertRole(db, posted, userId);
            SecurityRepository.SyncRoleFunctions(db, id, vm.SelectedFunctionIds, userId);
            SecurityRepository.SyncRoleUsers(db, id, vm.SelectedUserIds, userId);

            TempData["ToastType"] = "Act";
            TempData["ToastTitle"] = Security.Role_Toast_CreateTitle;
            if (Request.IsAjaxRequest())
            {
                return Json(new { IsOk = true, Id = id, Message = string.Format(Security.Role_Toast_Created, posted.TextLabel) });
            }

            TempData["ToastMessage"] = string.Format(Security.Role_Toast_Created, posted.TextLabel);
            return RedirectToAction("Details", new { id });
        }

        public ActionResult Edit(long id)
        {
            var role = SecurityRepository.GetRole(db, id);
            if (role == null)
            {
                return NotFoundToIndex();
            }

            return View(BuildForm(role, SecurityRepository.GetRoleFunctionIds(db, id), SecurityRepository.GetRoleUserIds(db, id)));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(SecurityRoleFormViewModel vm)
        {
            var existing = vm?.Role != null ? SecurityRepository.GetRole(db, vm.Role.IdObject) : null;
            if (existing == null)
            {
                return NotFoundToIndex();
            }

            var posted = Normalize(vm.Role);
            posted.IdObject = existing.IdObject;
            posted.Is_System = existing.Is_System;
            Validate(posted, existing.IdObject);
            if (!ModelState.IsValid)
            {
                return View(BuildForm(posted, vm.SelectedFunctionIds, vm.SelectedUserIds));
            }

            var userId = IntranetAuditHelper.ResolveCurrentUserId(User);
            SecurityRepository.UpdateRole(db, posted, userId);
            SecurityRepository.SyncRoleFunctions(db, existing.IdObject, vm.SelectedFunctionIds, userId);
            SecurityRepository.SyncRoleUsers(db, existing.IdObject, vm.SelectedUserIds, userId);

            TempData["ToastType"] = "Act";
            TempData["ToastTitle"] = Security.Role_Toast_EditTitle;
            TempData["ToastMessage"] = string.Format(Security.Role_Toast_Updated, posted.TextLabel);
            return RedirectToAction("Index");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public JsonResult DeleteRole(long id)
        {
            var role = SecurityRepository.GetRole(db, id);
            if (role == null)
            {
                return Json(new { IsOk = false, Message = Security.Role_Err_NotFound });
            }

            if (role.Is_System)
            {
                return Json(new { IsOk = false, Message = Security.Role_Err_CannotDeleteSystem });
            }

            SecurityRepository.SoftDeleteRole(db, id, IntranetAuditHelper.ResolveCurrentUserId(User));
            return Json(new { IsOk = true, Message = string.Format(Security.Role_Toast_Deleted, role.TextLabel) });
        }

        private ActionResult NotFoundToIndex()
        {
            TempData["ToastType"] = "Error";
            TempData["ToastTitle"] = Security.Role_Toast_EditTitle;
            TempData["ToastMessage"] = Security.Role_Err_NotFound;
            return RedirectToAction("Index");
        }

        private SecurityRoleFormViewModel BuildForm(SecurityRoleRow role, IList<long> functionIds, IList<string> userIds)
        {
            return new SecurityRoleFormViewModel
            {
                Role = role,
                AvailableFunctions = SecurityRepository.ListFunctions(db),
                SelectedFunctionIds = functionIds ?? new List<long>(),
                AvailableUsers = SecurityRepository.ListUsers(db),
                SelectedUserIds = userIds ?? new List<string>()
            };
        }

        private static SecurityRoleRow Normalize(SecurityRoleRow role)
        {
            role = role ?? new SecurityRoleRow { Is_Active = true };
            role.TextLabel = (role.TextLabel ?? "").Trim();
            role.TextCode = string.IsNullOrWhiteSpace(role.TextCode) ? null : role.TextCode.Trim().ToUpperInvariant();
            role.TextDescription = string.IsNullOrWhiteSpace(role.TextDescription) ? null : role.TextDescription.Trim();
            return role;
        }

        private void Validate(SecurityRoleRow role, long? excludeId)
        {
            if (string.IsNullOrWhiteSpace(role.TextLabel))
            {
                ModelState.AddModelError("Role.TextLabel", Security.Role_Val_NameRequired);
            }
            else if (role.TextLabel.Length > 500)
            {
                ModelState.AddModelError("Role.TextLabel", Security.Role_Val_NameTooLong);
            }
            else if (SecurityRepository.RoleLabelExists(db, role.TextLabel, excludeId))
            {
                ModelState.AddModelError("Role.TextLabel", Security.Role_Val_DuplicateName);
            }

            if (role.TextCode != null)
            {
                if (role.TextCode.Length > 100)
                {
                    ModelState.AddModelError("Role.TextCode", Security.Role_Val_CodeTooLong);
                }
                else if (SecurityRepository.RoleCodeExists(db, role.TextCode, excludeId))
                {
                    ModelState.AddModelError("Role.TextCode", Security.Role_Val_DuplicateCode);
                }
            }

            if (role.TextDescription != null && role.TextDescription.Length > 2000)
            {
                role.TextDescription = role.TextDescription.Substring(0, 2000);
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
