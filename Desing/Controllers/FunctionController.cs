using DataTables.Mvc;
using Desing.Helpers;
using Desing.Models;
using Desing.Resources;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.Mvc;

namespace Desing.Controllers
{
    /// <summary>
    /// CRUD de funciones (<c>TSql_Functions</c>). Una función activa protege el comando
    /// de su <c>TextCode</c> (controlador, acción, línea de menú o comando JS); sin función,
    /// el comando es libre para todos. Protegido por la función «Function».
    /// </summary>
    [Authorize]
    public class FunctionController : BaseController
    {
        private static readonly Regex CodeRegex = new Regex(@"^[A-Za-z0-9_.\- ]+$", RegexOptions.Compiled);
        private static readonly string[] ExemptCodes = { "Account", "UiLanguage" };

        public ActionResult Index()
        {
            return View();
        }

        public ActionResult Details(long id)
        {
            var f = SecurityRepository.GetFunction(db, id);
            if (f == null)
            {
                return HttpNotFound();
            }

            var roleIds = new HashSet<long>(SecurityRepository.GetFunctionRoleIds(db, id));
            ViewBag.Roles = SecurityRepository.ListRoles(db).Where(r => roleIds.Contains(r.IdObject)).ToList();
            ViewBag.Audit = IntranetAuditHelper.BuildDisplay(
                db, f.LinkMadeBy, f.LinModifiedBy, f.AddDateMade, f.AddLastDateChange, f.Ntimeschanged);
            return View(f);
        }

        [OutputCache(Duration = 1)]
        public JsonResult ListFunctions([ModelBinder(typeof(DataTablesBinder))] IDataTablesRequest requestModel)
        {
            try
            {
                var query = SecurityRepository.ListFunctions(db).AsQueryable();
                var totalCount = query.Count();

                if (!string.IsNullOrEmpty(requestModel.Search.Value))
                {
                    var value = requestModel.Search.Value.Trim();
                    query = query.Where(p =>
                        (p.TextLabel ?? "").IndexOf(value, StringComparison.OrdinalIgnoreCase) >= 0
                        || (p.TextCode ?? "").IndexOf(value, StringComparison.OrdinalIgnoreCase) >= 0
                        || (p.TextModule ?? "").IndexOf(value, StringComparison.OrdinalIgnoreCase) >= 0);
                }

                var filteredCount = query.Count();

                var orderByString = string.Empty;
                foreach (var column in requestModel.Columns.GetSortedColumns())
                {
                    string orderColumn;
                    switch (column.Data)
                    {
                        case "TextCode": orderColumn = "TextCode"; break;
                        case "TextModule": orderColumn = "TextModule"; break;
                        case "NRoles": orderColumn = "NRoles"; break;
                        case "activeBadge": orderColumn = "Is_Active"; break;
                        default: orderColumn = "TextLabel"; break;
                    }
                    orderByString += orderByString != string.Empty ? "," : "";
                    orderByString += orderColumn + (column.SortDirection == Column.OrderDirection.Ascendant ? " asc" : " desc");
                }

                query = query.OrderBy(string.IsNullOrEmpty(orderByString) ? "TextModule asc, TextLabel asc" : orderByString);
                query = query.ApplyDataTablesPaging(requestModel.Start, requestModel.Length);

                var ttOpen = HttpUtility.HtmlAttributeEncode(Security.Function_List_OpenTooltip);
                var ttEdit = HttpUtility.HtmlAttributeEncode(Security.Function_List_EditTooltip);
                var ttDelete = HttpUtility.HtmlAttributeEncode(Security.Function_List_DeleteTooltip);
                var lblProtected = HttpUtility.HtmlEncode(Security.Function_State_Protected);
                var lblFree = HttpUtility.HtmlEncode(Security.Function_State_Free);

                var data = query.ToList().Select(p =>
                {
                    var namePlain = p.TextLabel ?? "";
                    return new
                    {
                        IdObject = p.IdObject,
                        TextLabel = "<a title=\"" + ttOpen + "\" href=\"" + Url.Action("Details", new { id = p.IdObject }) + "\">" +
                                    HttpUtility.HtmlEncode(namePlain) + "</a>",
                        TextLabelPlain = namePlain,
                        TextCode = "<code>" + HttpUtility.HtmlEncode(p.TextCode ?? "") + "</code>",
                        TextCodePlain = p.TextCode ?? "",
                        TextModule = HttpUtility.HtmlEncode(p.TextModule ?? ""),
                        NRoles = p.NRoles,
                        Is_Active = p.Is_Active,
                        activeBadge = p.Is_Active
                            ? "<span class=\"badge bg-label-success\">" + lblProtected + "</span>"
                            : "<span class=\"badge bg-label-secondary\">" + lblFree + "</span>",
                        rowActions = "<div class=\"d-inline-flex align-items-center gap-2\" role=\"group\">" +
                                     "<a title=\"" + ttEdit + "\" href=\"" + Url.Action("Edit", new { id = p.IdObject }) +
                                     "\" class=\"btn btn-warning btn-xs\"><span class=\"fas fa-edit\" aria-hidden=\"true\"></span></a>" +
                                     "<a title=\"" + ttDelete + "\" href=\"#\" onclick=\"DeleteFunction(" + p.IdObject +
                                     "); return false;\" class=\"btn btn-danger btn-xs\"><span class=\"fas fa-trash-alt\" aria-hidden=\"true\"></span></a>" +
                                     "</div>"
                    };
                }).ToList();

                return Json(DataTablesMvcJson.Create(requestModel.Draw, data, filteredCount, totalCount), JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(ex.Message);
            }
        }

        public ActionResult Create(string code = null)
        {
            var f = new SecurityFunctionRow { Is_Active = true, TextCode = code, TextModule = FunctionCatalog.ModuleFor(code) };
            return View(BuildForm(f, new List<long>()));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(SecurityFunctionFormViewModel vm)
        {
            var posted = Normalize(vm?.Function);
            Validate(posted, null);
            if (!ModelState.IsValid)
            {
                return View(BuildForm(posted, vm?.SelectedRoleIds));
            }

            var userId = IntranetAuditHelper.ResolveCurrentUserId(User);
            var id = SecurityRepository.InsertFunction(db, posted, userId);
            SecurityRepository.SyncFunctionRoles(db, id, vm.SelectedRoleIds, userId);

            TempData["ToastType"] = "Act";
            TempData["ToastTitle"] = Security.Function_Toast_CreateTitle;
            TempData["ToastMessage"] = string.Format(Security.Function_Toast_Created, posted.TextLabel);
            return RedirectToAction("Details", new { id });
        }

        public ActionResult Edit(long id)
        {
            var f = SecurityRepository.GetFunction(db, id);
            if (f == null)
            {
                return NotFoundToIndex();
            }

            return View(BuildForm(f, SecurityRepository.GetFunctionRoleIds(db, id)));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(SecurityFunctionFormViewModel vm)
        {
            var existing = vm?.Function != null ? SecurityRepository.GetFunction(db, vm.Function.IdObject) : null;
            if (existing == null)
            {
                return NotFoundToIndex();
            }

            var posted = Normalize(vm.Function);
            posted.IdObject = existing.IdObject;
            Validate(posted, existing.IdObject);
            if (!ModelState.IsValid)
            {
                return View(BuildForm(posted, vm.SelectedRoleIds));
            }

            var userId = IntranetAuditHelper.ResolveCurrentUserId(User);
            SecurityRepository.UpdateFunction(db, posted, userId);
            SecurityRepository.SyncFunctionRoles(db, existing.IdObject, vm.SelectedRoleIds, userId);

            TempData["ToastType"] = "Act";
            TempData["ToastTitle"] = Security.Function_Toast_EditTitle;
            TempData["ToastMessage"] = string.Format(Security.Function_Toast_Updated, posted.TextLabel);
            return RedirectToAction("Index");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public JsonResult DeleteFunction(long id)
        {
            var f = SecurityRepository.GetFunction(db, id);
            if (f == null)
            {
                return Json(new { IsOk = false, Message = Security.Function_Err_NotFound });
            }

            SecurityRepository.SoftDeleteFunction(db, id, IntranetAuditHelper.ResolveCurrentUserId(User));
            return Json(new { IsOk = true, Message = string.Format(Security.Function_Toast_Deleted, f.TextLabel) });
        }

        private ActionResult NotFoundToIndex()
        {
            TempData["ToastType"] = "Error";
            TempData["ToastTitle"] = Security.Function_Toast_EditTitle;
            TempData["ToastMessage"] = Security.Function_Err_NotFound;
            return RedirectToAction("Index");
        }

        private SecurityFunctionFormViewModel BuildForm(SecurityFunctionRow f, IList<long> roleIds)
        {
            return new SecurityFunctionFormViewModel
            {
                Function = f,
                AvailableRoles = SecurityRepository.ListRoles(db)
                    .Select(r => new SecurityRoleOption { IdObject = r.IdObject, TextLabel = r.TextLabel, TextDescription = r.TextDescription })
                    .ToList(),
                SelectedRoleIds = roleIds ?? new List<long>(),
                Catalog = FunctionCatalog.Entries
                    .Select(e => new FunctionCatalogOption { Code = e.Code, Module = e.Module })
                    .ToList()
            };
        }

        private static SecurityFunctionRow Normalize(SecurityFunctionRow f)
        {
            f = f ?? new SecurityFunctionRow { Is_Active = true };
            f.TextLabel = (f.TextLabel ?? "").Trim();
            f.TextCode = (f.TextCode ?? "").Trim();
            f.TextModule = string.IsNullOrWhiteSpace(f.TextModule) ? FunctionCatalog.ModuleFor(f.TextCode) : f.TextModule.Trim();
            f.TextDescription = string.IsNullOrWhiteSpace(f.TextDescription) ? null : f.TextDescription.Trim();
            return f;
        }

        private void Validate(SecurityFunctionRow f, long? excludeId)
        {
            if (string.IsNullOrWhiteSpace(f.TextLabel))
            {
                ModelState.AddModelError("Function.TextLabel", Security.Function_Val_NameRequired);
            }
            else if (f.TextLabel.Length > 500)
            {
                ModelState.AddModelError("Function.TextLabel", Security.Function_Val_NameTooLong);
            }

            if (string.IsNullOrWhiteSpace(f.TextCode))
            {
                ModelState.AddModelError("Function.TextCode", Security.Function_Val_CodeRequired);
            }
            else if (f.TextCode.Length > 200)
            {
                ModelState.AddModelError("Function.TextCode", Security.Function_Val_CodeTooLong);
            }
            else if (!CodeRegex.IsMatch(f.TextCode))
            {
                ModelState.AddModelError("Function.TextCode", Security.Function_Val_CodeInvalid);
            }
            else if (ExemptCodes.Any(c => f.TextCode.Equals(c, StringComparison.OrdinalIgnoreCase)
                                          || f.TextCode.StartsWith(c + ".", StringComparison.OrdinalIgnoreCase)))
            {
                ModelState.AddModelError("Function.TextCode", Security.Function_Val_CodeExempt);
            }
            else if (SecurityRepository.FunctionCodeExists(db, f.TextCode, excludeId))
            {
                ModelState.AddModelError("Function.TextCode", Security.Function_Val_DuplicateCode);
            }

            if (f.TextModule != null && f.TextModule.Length > 100)
            {
                ModelState.AddModelError("Function.TextModule", Security.Function_Val_ModuleTooLong);
            }

            if (f.TextDescription != null && f.TextDescription.Length > 2000)
            {
                f.TextDescription = f.TextDescription.Substring(0, 2000);
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
