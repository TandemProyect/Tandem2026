using DAL;
using DataTables.Mvc;
using Desing.Helpers;
using Desing.Models;
using Desing.Resources;
using Microsoft.AspNet.Identity;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.SqlClient;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Net.Mail;
using System.Web;
using System.Web.Mvc;
using static SendMail.Models;

namespace Desing.Controllers
{
    public class EmployeeController : BaseController
    {
        /* ============================================================
           Index + DataTables JSON (patrón Materio + applyListDefaults)
           ============================================================ */

        public ActionResult Index()
        {
            return View();
        }

        [OutputCache(Duration = 1)]
        public JsonResult ListEmployee([ModelBinder(typeof(DataTablesBinder))] IDataTablesRequest requestModel)
        {
            try
            {
                IQueryable<EmployeeViewModel> query = from user in db.AspNetUsers
                                                     join employee in db.TSql_Employee on user.Id equals employee.LinAspNetUsert
                                                     join company in db.TSql_Company on employee.LinCompany equals company.SysObjectID
                                                     join design in (from d in db.TSql_Design
                                                                     group d by d.LinCreatedBy into g
                                                                     select new { LinCreatedBy = g.Key, NDesing = g.Count() }) on user.Id equals design.LinCreatedBy into designGroup
                                                     from totalDesign in designGroup.DefaultIfEmpty()
                                                     where employee.AttIsDeleted == false
                                                     select new EmployeeViewModel
                                                     {
                                                         SysObjectID = employee.SysObjectID,
                                                         userId = user.Id,
                                                         TotalDesing = totalDesign != null ? totalDesign.NDesing : 0,
                                                         AttName = employee.AttName,
                                                         AttSurname = employee.AttSurname,
                                                         AttPhotoMenu = employee.AttPhotoMenu,
                                                         AttCreated = employee.AttCreated,
                                                         AddLeter = company.AddLeter,
                                                         AddCompany = company.TextLabel,
                                                         AttPassAspNetUsert = employee.AttPassAspNetUsert,
                                                         EmailConfirmed = user.EmailConfirmed,
                                                         UserName = user.UserName
                                                     };

                var totalCount = query.Count();

                if (!string.IsNullOrEmpty(requestModel.Search.Value))
                {
                    var value = requestModel.Search.Value.Trim();
                    query = query.Where(p =>
                        (p.AttName ?? "").Contains(value) ||
                        (p.AttSurname ?? "").Contains(value) ||
                        (p.UserName ?? "").Contains(value) ||
                        (p.AddCompany ?? "").Contains(value) ||
                        (p.AddLeter ?? "").Contains(value));
                }

                var filteredCount = query.Count();

                var sortedColumns = requestModel.Columns.GetSortedColumns();
                var orderByString = string.Empty;
                foreach (var column in sortedColumns)
                {
                    string orderColumn;
                    switch (column.Data)
                    {
                        case "UserName": orderColumn = "UserName"; break;
                        case "AttName": orderColumn = "AttName"; break;
                        case "AttSurname": orderColumn = "AttSurname"; break;
                        case "AddLeter": orderColumn = "AddLeter"; break;
                        case "AddCompany": orderColumn = "AddCompany"; break;
                        case "AttCreated": orderColumn = "AttCreated"; break;
                        case "EmailConfirmed": orderColumn = "EmailConfirmed"; break;
                        case "TotalDesing": orderColumn = "TotalDesing"; break;
                        default: orderColumn = "AttName"; break;
                    }
                    orderByString += orderByString != string.Empty ? "," : "";
                    orderByString += orderColumn + (column.SortDirection == Column.OrderDirection.Ascendant ? " asc" : " desc");
                }
                query = query.OrderBy(string.IsNullOrEmpty(orderByString) ? "AttName asc" : orderByString);
                query = query.ApplyDataTablesPaging(requestModel.Start, requestModel.Length);

                var avatarRoot = Url.Content("~/");
                var avatarDefault = Url.Content("~/Files/RRHH/User/AttPhotoMenu/User.png");
                var ttEdit = HttpUtility.HtmlAttributeEncode(Employee.List_LinkEditTooltip);
                var ttDelete = HttpUtility.HtmlAttributeEncode(Employee.List_LinkDeleteTooltip);
                var ttToggle = HttpUtility.HtmlAttributeEncode(Employee.List_LinkToggleTooltip);
                var ttSendMail = HttpUtility.HtmlAttributeEncode(Employee.List_LinkSendMailTooltip);
                var stateActive = Employee.State_AccountConfirmed;
                var stateInactive = Employee.State_AccountUnconfirmed;

                var data = query.ToList().Select(p =>
                {
                    var fullName = ((p.AttName ?? "") + " " + (p.AttSurname ?? "")).Trim();
                    var fullNamePlain = fullName.Length == 0 ? (p.UserName ?? "") : fullName;
                    var fullNameEnc = HttpUtility.HtmlEncode(fullNamePlain);
                    var nameCell = "<a href=\"" + Url.Action("Edit_Employee", "Employee", new { Id = p.SysObjectID }) +
                                   "\" title=\"" + HttpUtility.HtmlAttributeEncode(Employee.List_LinkEditTooltip) + "\">" +
                                   fullNameEnc + "</a>";

                    var avatarPath = (p.AttPhotoMenu ?? string.Empty).Trim();
                    avatarPath = System.Text.RegularExpressions.Regex.Replace(avatarPath, "^~/", "");
                    avatarPath = System.Text.RegularExpressions.Regex.Replace(avatarPath, "^(\\.\\./)+", "");
                    var avatarUrl = string.IsNullOrEmpty(avatarPath) ? avatarDefault : avatarRoot + avatarPath;
                    var avatarHtml = "<img style=\"height:35px;width:35px;object-fit:cover;border-radius:50%;\" src=\"" +
                                     HttpUtility.HtmlAttributeEncode(avatarUrl) + "\" alt=\"\" " +
                                     "onerror=\"this.onerror=null;this.src='" +
                                     HttpUtility.HtmlAttributeEncode(avatarDefault) + "';\" />";

                    var editBtn =
                        "<a title=\"" + ttEdit + "\" href=\"" +
                        Url.Action("Edit_Employee", "Employee", new { Id = p.SysObjectID }) +
                        "\" class=\"btn btn-warning btn-xs\"><span class=\"fas fa-edit\" aria-hidden=\"true\"></span></a>";
                    var toggleBtn =
                        "<a title=\"" + ttToggle + "\" href=\"#\" onclick=\"ToggleEmployee('" + p.SysObjectID +
                        "'); return false;\" class=\"btn btn-info btn-xs\"><span class=\"fas fa-sync\" aria-hidden=\"true\"></span></a>";
                    var sendBtn =
                        "<a title=\"" + ttSendMail + "\" href=\"#\" onclick=\"SendmailToEmployee('" + p.SysObjectID +
                        "'); return false;\" class=\"btn btn-success btn-xs\"><span class=\"fa fa-envelope-open\" aria-hidden=\"true\"></span></a>";
                    var deleteBtn = (p.TotalDesing == 0)
                        ? "<a title=\"" + ttDelete + "\" href=\"#\" onclick=\"DeleteEmployee('" + p.SysObjectID +
                          "'); return false;\" class=\"btn btn-danger btn-xs\"><span class=\"fas fa-trash-alt\" aria-hidden=\"true\"></span></a>"
                        : "";

                    var rowActions =
                        "<div class=\"d-inline-flex align-items-center gap-2\" role=\"group\">" +
                        toggleBtn + sendBtn + editBtn +
                        (string.IsNullOrEmpty(deleteBtn) ? "" : deleteBtn) +
                        "</div>";

                    return new
                    {
                        SysObjectID = p.SysObjectID,
                        Avatar = avatarHtml,
                        UserName = p.UserName ?? "",
                        TextLabelPlain = fullNamePlain,
                        TextLabel = nameCell,
                        AttName = p.AttName ?? "",
                        AttSurname = p.AttSurname ?? "",
                        AddLeter = p.AddLeter ?? "",
                        AddCompany = p.AddCompany ?? "",
                        AttPassAspNetUsert = p.AttPassAspNetUsert ?? "",
                        AttCreated = p.AttCreated.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture),
                        TotalDesing = p.TotalDesing,
                        EmailConfirmed = p.EmailConfirmed,
                        StatusText = p.EmailConfirmed ? stateActive : stateInactive,
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

        /* ============================================================
           Delete (logical) + Toggle account (EmailConfirmed)
           ============================================================ */

        [HttpPost]
        public JsonResult DeleteEmployee(long id)
        {
            var employee = db.TSql_Employee.FirstOrDefault(x => x.SysObjectID == id);
            if (employee == null)
                return Json(new { IsOk = false, Message = Employee.Err_EmployeeNotFound });

            var hasDesigns = db.TSql_Design.Any(d => d.LinCreatedBy == employee.LinAspNetUsert);
            if (hasDesigns)
                return Json(new { IsOk = false, Message = Employee.Err_CannotDeleteRelated });

            var defaults = db.TSql_DefaultDesign.Where(x => x.LinkAspNetUsers == employee.LinAspNetUsert).ToList();
            foreach (var d in defaults) db.TSql_DefaultDesign.Remove(d);

            employee.AttIsDeleted = true;
            employee.LinModifiedBy = User.Identity.GetUserId();
            employee.AttLastModification = DateTime.UtcNow;
            employee.SysUpdateNumber = employee.SysUpdateNumber + 1;
            db.SaveChanges();

            return Json(new { IsOk = true, Message = Employee.Msg_EmployeeDeleted });
        }

        [HttpPost]
        public JsonResult ToggleEmployee(long id)
        {
            var employee = db.TSql_Employee.FirstOrDefault(x => x.SysObjectID == id);
            if (employee == null)
                return Json(new { IsOk = false, Message = Employee.Err_EmployeeNotFound });

            var user = db.AspNetUsers.FirstOrDefault(x => x.Id == employee.LinAspNetUsert);
            if (user == null)
                return Json(new { IsOk = false, Message = Employee.Err_UserNotFound });

            user.EmailConfirmed = !user.EmailConfirmed;
            employee.LinModifiedBy = User.Identity.GetUserId();
            employee.AttLastModification = DateTime.UtcNow;
            employee.SysUpdateNumber = employee.SysUpdateNumber + 1;
            db.SaveChanges();

            return Json(new
            {
                IsOk = true,
                Message = user.EmailConfirmed ? Employee.Msg_EmployeeActivated : Employee.Msg_EmployeeDeactivated
            });
        }

        /* ============================================================
           Send mail (legacy SMTP envelope, mail send disabled below).
           ============================================================ */

        public ActionResult SendMailUser(long Id)
        {
            try
            {
                var employee = db.TSql_Employee.FirstOrDefault(x => x.SysObjectID == Id);
                if (employee == null)
                    return Content("Error: " + Employee.Err_EmployeeNotFound, "text/plain");

                var user = db.AspNetUsers.FirstOrDefault(x => x.Id == employee.LinAspNetUsert);
                if (user == null)
                    return Content("Error: " + Employee.Err_UserNotFound, "text/plain");

                MailModel Model = new MailModel
                {
                    To = user.Email,
                    From = "admin@atenko.net",
                    Subject = "Envio de contraseña",
                    Body = "Envio de contraseña al usuario:  " + employee.AttName + " " + employee.AttSurname +
                           " La contraseña requerida es:  " + employee.AttPassAspNetUsert
                };
                SendMail(Model);
                return Content("Success: " + Employee.Msg_MailSent, "text/plain");
            }
            catch (Exception ex)
            {
                return Content("Error: " + ex.Message, "text/plain");
            }
        }

        /* ============================================================
           MySpace
           ============================================================ */

        public ActionResult MySapce()
        {
            try
            {
                string userId = User.Identity.GetUserId();
                AspNetUsers aspNetUser = db.AspNetUsers.Find(userId);
                TSql_Employee employee = db.TSql_Employee.FirstOrDefault(x => x.LinAspNetUsert == userId);
                if (employee == null) return RedirectToAction("Index");

                var totalDesigns = db.TSql_Design.Count(x => x.LinCreatedBy == employee.LinAspNetUsert);

                EmployeeViewMySpaceModel Model = new EmployeeViewMySpaceModel
                {
                    FullName = $"{employee.AttName} {employee.AttSurname}",
                    UserName = aspNetUser != null ? aspNetUser.UserName : string.Empty,
                    AttPhoto = employee.AttPhotoMenu,
                    EmailConfirmed = aspNetUser != null && aspNetUser.EmailConfirmed,
                    AccountCreationDate = employee.AttCreated,
                    TotalDesigns = totalDesigns
                };
                return View("Myspace", Model);
            }
            catch (Exception ex)
            {
                return RedirectToAction("Error500", ex.Message);
            }
        }

        /* ============================================================
           Legacy modal endpoints (Delete / Active popups). Mantenidos
           para no romper enlaces directos; las acciones modernas
           (DeleteEmployee / ToggleEmployee) son las recomendadas.
           ============================================================ */

        public ActionResult Delete()
        {
            try
            {
                var id = Session["IDDesign"];
                TSql_Employee employee = db.TSql_Employee.Find(id);
                return PartialView("_Delete", employee);
            }
            catch (Exception ex)
            {
                return RedirectToAction("Error500", ex.Message);
            }
        }

        public ActionResult _Delete(long Id)
        {
            try
            {
                Session["IDDesign"] = Id;
                return Json(new { data = true, IsOk = true });
            }
            catch (Exception ex)
            {
                return RedirectToAction("Error500", ex);
            }
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(long id)
        {
            try
            {
                TSql_Employee employee = db.TSql_Employee.Find(id);
                if (employee == null)
                    return RedirectToAction("Index");

                var defaults = db.TSql_DefaultDesign.Where(x => x.LinkAspNetUsers == employee.LinAspNetUsert).ToList();
                foreach (var d in defaults) db.TSql_DefaultDesign.Remove(d);

                employee.AttIsDeleted = true;
                employee.LinModifiedBy = User.Identity.GetUserId();
                employee.AttLastModification = DateTime.UtcNow;
                employee.SysUpdateNumber = employee.SysUpdateNumber + 1;
                db.SaveChanges();
                TempData.Clear();
                TempData["ToastType"] = "Act";
                TempData["ToastTitle"] = Employee.ToastTitle_DeleteEmployee;
                TempData["ToastMessage"] = Employee.ToastMessage_EmployeeDeleted;
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                return RedirectToAction("Error500", ex.Message);
            }
        }

        public ActionResult Active()
        {
            try
            {
                long id = (long)Session["IDDesign"];
                TSql_Employee employee = db.TSql_Employee.FirstOrDefault(x => x.SysObjectID == id);
                if (employee == null) return RedirectToAction("Index");

                var user = db.AspNetUsers.FirstOrDefault(x => x.Id == employee.LinAspNetUsert);
                var isActive = user != null && user.EmailConfirmed;
                var name = (employee.AttName ?? "") + " " + (employee.AttSurname ?? "");
                name = name.Trim();

                ViewBag.ActiveNoActive = isActive
                    ? Employee.Modal_ActiveLegendActivated
                    : Employee.Modal_ActiveLegendDeactivated;
                ViewBag.Mesaje = string.Format(
                    isActive ? Employee.Modal_ActiveQuestionDeactivate : Employee.Modal_ActiveQuestionActivate,
                    name);
                ViewBag.Button = isActive ? Employee.Modal_BtnDeactivate : Employee.Modal_BtnActivate;
                return PartialView("_Active", employee);
            }
            catch (Exception ex)
            {
                return RedirectToAction("Error500", ex.Message);
            }
        }

        public ActionResult _Active(long Id)
        {
            try
            {
                Session["IDDesign"] = Id;
                return Json(new { data = true, IsOk = true });
            }
            catch (Exception ex)
            {
                return RedirectToAction("Error500", ex);
            }
        }

        [HttpPost, ActionName("Active")]
        [ValidateAntiForgeryToken]
        public ActionResult ActiveConfirmed(long id)
        {
            try
            {
                TSql_Employee employee = db.TSql_Employee.FirstOrDefault(x => x.SysObjectID == id);
                if (employee == null) return RedirectToAction("Index");

                var user = db.AspNetUsers.FirstOrDefault(x => x.Id == employee.LinAspNetUsert);
                if (user == null) return RedirectToAction("Index");

                user.EmailConfirmed = !user.EmailConfirmed;
                db.SaveChanges();
                TempData.Clear();
                TempData["ToastType"] = "Act";
                TempData["ToastTitle"] = Employee.ToastTitle_ToggleEmployee;
                TempData["ToastMessage"] = user.EmailConfirmed
                    ? Employee.ToastMessage_EmployeeActivated
                    : Employee.ToastMessage_EmployeeDeactivated;
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                return Json(ex.Message);
            }
        }

        /* ============================================================
           Create / Edit (basado en sesión heredada del flujo de Account)
           ============================================================ */

        public ActionResult Edit_Employee(long Id)
        {
            try
            {
                TSql_Employee employee = db.TSql_Employee.FirstOrDefault(x => x.SysObjectID == Id);
                if (employee == null) return RedirectToAction("Index");

                var user = db.AspNetUsers.FirstOrDefault(x => x.Id == employee.LinAspNetUsert);
                Session["EmployeeID"] = Id;
                Session["ItComeFrom"] = "Edit_Employee";
                Session["userSystem"] = user?.Id;
                Session["userVscad"] = user?.UserName;
                Session["passVscad"] = employee.AttPassAspNetUsert;
                return RedirectToAction("Create_Employee", "Employee");
            }
            catch (Exception ex)
            {
                return RedirectToAction("Error500", ex);
            }
        }

        public ActionResult Create_Employee()
        {
            var iTComeFrom = Session["ItComeFrom"] as string;
            var isEdit = string.Equals(iTComeFrom, "Edit_Employee", StringComparison.OrdinalIgnoreCase);
            if (!isEdit)
            {
                Session["EmployeeID"] = "";
            }

            ViewBag.MessageHeat = isEdit ? Employee.Page_EditTitle : Employee.Page_CreateTitle;

            var userSystem = Session["userSystem"]?.ToString();
            var userVscad = Session["userVscad"]?.ToString();
            var passVscad = Session["passVscad"]?.ToString();
            if (string.IsNullOrWhiteSpace(userSystem) || string.IsNullOrWhiteSpace(userVscad))
            {
                return RedirectToAction("Index");
            }

            long employeeIdSesion;
            bool tieneEmployeeIdSesion = long.TryParse(Session["EmployeeID"]?.ToString(), out employeeIdSesion) && employeeIdSesion > 0;
            TSql_Employee employee = null;
            if (isEdit && tieneEmployeeIdSesion)
                employee = db.TSql_Employee.FirstOrDefault(x => x.SysObjectID == employeeIdSesion);
            if (employee == null)
                employee = db.TSql_Employee.FirstOrDefault(x => x.LinAspNetUsert == userSystem);

            var attName = "";
            var attSurname = "";
            var attPhotoMenu = "~/Files/RRHH/User/AttPhotoMenu/user.png";
            var linCompany = 1;
            if (employee != null)
            {
                attName = employee.AttName;
                attSurname = employee.AttSurname;
                attPhotoMenu = employee.AttPhotoMenu;
                linCompany = (int)employee.LinCompany;
            }
            var authInfo = ObtenerPrimerEquipoAutorizado(userSystem);

            ViewBag.LinCompany = new SelectList(db.TSql_Company.Where(u => u.BitIsDeleted == false), "SysObjectID", "TextLabel");

            EmployeeViewModel Model = new EmployeeViewModel
            {
                AttName = attName,
                AttSurname = attSurname,
                userSystem = userVscad,
                LinAspNetUsert = userSystem,
                AttPassAspNetUsert = passVscad,
                LinCompany = linCompany,
                AttPhotoMenu = attPhotoMenu,
                DeviceId = authInfo.DeviceId,
                DeviceName = authInfo.MachineName,
                DeviceAllowed = authInfo.Allowed ?? true,
                EmployeeID = employee?.SysObjectID ?? 0,
                IsEdit = isEdit,
            };
            return View(Model);
        }

        [HttpPost]
        public ActionResult Create_Employee([Bind(Include = "AttName, AttSurname, AttPhoto,AttPhotoMenu, LinCompany, LinBusiness, LinAspNetUsert, AttPassAspNetUsert, userSystem, DeviceId, DeviceName, DeviceAllowed, EmployeeID, IsEdit")] EmployeeViewModel model, HttpPostedFileBase file1)
        {
            return SaveEmployee(model, file1, false);
        }

        [HttpPost]
        public ActionResult Update_Employee([Bind(Include = "AttName, AttSurname, AttPhoto,AttPhotoMenu, LinCompany, LinBusiness, LinAspNetUsert, AttPassAspNetUsert, userSystem, DeviceId, DeviceName, DeviceAllowed, EmployeeID, IsEdit")] EmployeeViewModel model, HttpPostedFileBase file1)
        {
            return SaveEmployee(model, file1, true);
        }

        private ActionResult SaveEmployee(EmployeeViewModel model, HttpPostedFileBase file1, bool forceEdit)
        {
            try
            {
                var iTComeFrom = Session["ItComeFrom"] as string ?? "";
                var isEdit = forceEdit || model.IsEdit || string.Equals(iTComeFrom, "Edit_Employee", StringComparison.OrdinalIgnoreCase);
                var changeFoto = true;
                long employeeId = 0;

                if (model.EmployeeID > 0) employeeId = model.EmployeeID;
                if (employeeId <= 0) long.TryParse(Session["EmployeeID"]?.ToString(), out employeeId);
                var existing = db.TSql_Employee.FirstOrDefault(x => x.SysObjectID == employeeId);
                if (existing != null && isEdit)
                {
                    var check = existing.AttPhotoMenu?.Length >= 35 ? existing.AttPhotoMenu.Substring(35) : "";
                    if (file1 == null) changeFoto = false;
                    else if (file1.FileName == check) changeFoto = false;
                }

                string userId = User.Identity.GetUserId();
                string nombreOriginal = "";
                string extensionOriginal = "";
                string rutaTemp = "";
                string rutaNew = "";
                if (changeFoto)
                {
                    rutaTemp = Server.MapPath("~") + @"\Files\RRHH\User\AttPhotoMenu\Temp\";
                    rutaNew = Server.MapPath("~") + @"\Files\RRHH\User\AttPhotoMenu\";

                    if (file1 != null && file1.ContentLength > 0)
                    {
                        nombreOriginal = Path.GetFileName(file1.FileName);
                        extensionOriginal = Path.GetExtension(file1.FileName);
                    }
                    else
                    {
                        model.AttPhotoMenu = "../../Files/RRHH/User/AttPhotoMenu/User.png";
                    }
                    if (file1 != null && file1.ContentLength > 0)
                    {
                        file1.SaveAs(Path.Combine(rutaTemp, "_" + nombreOriginal));
                        ImageHelper.RedimensionarImagen(rutaTemp, "_" + nombreOriginal, 100, 100, 0);
                        model.AttPhotoMenu = "../../Files/RRHH/User/AttPhotoMenu/Temp/__" + nombreOriginal;
                    }
                }

                TSql_Employee newEmployee = new TSql_Employee
                {
                    AttName = model.AttName,
                    AttSurname = model.AttSurname,
                    AttPhoto = model.AttPhoto,
                    AttPhotoMenu = model.AttPhotoMenu,
                    LinCompany = model.LinCompany,
                    LinBusiness = model.LinBusiness,
                    SysUpdateNumber = 1,
                    LinAspNetUsert = model.LinAspNetUsert,
                    AttPassAspNetUsert = model.AttPassAspNetUsert,
                    AttIsDeleted = false,
                    Linlanguage = 1,
                    LinCreatedBy = userId,
                    AttCreated = DateTime.UtcNow,
                    LinModifiedBy = userId,
                    AttLastModification = DateTime.UtcNow,
                };
                if (iTComeFrom != "Edit_Employee")
                {
                    db.TSql_Employee.Add(newEmployee);
                    db.SaveChanges();
                }
                if (changeFoto && file1 != null && file1.ContentLength > 0)
                {
                    newEmployee.AttPhotoMenu = "../../Files/RRHH/User/AttPhotoMenu/" + newEmployee.SysObjectID.ToString() + ".1" + extensionOriginal;
                    System.IO.File.Move(rutaTemp + @"\__" + nombreOriginal,
                        rutaNew + @"\" + newEmployee.SysObjectID.ToString() + ".1" + extensionOriginal);
                }

                TSql_DefaultDesign config = new TSql_DefaultDesign
                {
                    LinkAspNetUsers = model.LinAspNetUsert,
                    NumberClosingStartEndWall = 1,
                    NumberWallheigh = 1,
                    NumberwallWidth = 1,
                    NumberwallLength = 1,
                    LinkEnvironment = 2,
                    ExitingPanel2400 = true,
                    LinTapeInialEndWallIsLessThan300 = 1,
                    LinTapeInialEndWallIMoreThan300 = 1,
                    AddTapeWidthExactIfPossible = true,
                    IsSolutionCornerWithUniversalPanel = true,
                    OrbitControlsType = 1,
                    Ntimeschanged = 1,
                    LinkMadeBy = userId,
                    LinModifiedBy = userId,
                    AddChangeBy = userId,
                    AddDateMade = DateTime.UtcNow,
                    AddLastDateChange = DateTime.UtcNow,
                    BitIsDeleted = false,
                };
                if (!isEdit)
                {
                    db.TSql_DefaultDesign.Add(config);
                    db.SaveChanges();
                    UpsertPluginDeviceAuth(model, userId);

                    TempData.Clear();
                    TempData["ToastType"] = "Act";
                    TempData["ToastTitle"] = Employee.ToastTitle_CreateEmployee;
                    TempData["ToastMessage"] = Employee.ToastMessage_EmployeeSaved;
                }
                if (isEdit && existing != null)
                {
                    existing.AttName = newEmployee.AttName;
                    existing.AttSurname = newEmployee.AttSurname;
                    existing.AttPhoto = newEmployee.AttPhoto;
                    if (changeFoto) existing.AttPhotoMenu = newEmployee.AttPhotoMenu;
                    existing.LinCompany = newEmployee.LinCompany;
                    existing.LinBusiness = newEmployee.LinBusiness;
                    existing.SysUpdateNumber = newEmployee.SysUpdateNumber;
                    existing.LinAspNetUsert = newEmployee.LinAspNetUsert;
                    existing.AttPassAspNetUsert = newEmployee.AttPassAspNetUsert;
                    existing.AttIsDeleted = false;
                    existing.Linlanguage = 1;
                    existing.LinModifiedBy = userId;
                    existing.AttLastModification = DateTime.UtcNow;
                    db.SaveChanges();
                    UpsertPluginDeviceAuth(model, userId);
                    TempData.Clear();
                    TempData["ToastType"] = "Editar";
                    TempData["ToastTitle"] = Employee.ToastTitle_EditEmployee;
                    TempData["ToastMessage"] = Employee.ToastMessage_EmployeeUpdated;
                }
                Session.Clear();
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                return Json(ex.Message);
            }
        }

        /* ============================================================
           Plugin device auth (sin cambios funcionales)
           ============================================================ */

        private sealed class PluginDeviceAuthInfo
        {
            public string DeviceId { get; set; }
            public string MachineName { get; set; }
            public bool? Allowed { get; set; }
        }

        private PluginDeviceAuthInfo ObtenerPrimerEquipoAutorizado(string aspNetUserId)
        {
            if (string.IsNullOrWhiteSpace(aspNetUserId)) return new PluginDeviceAuthInfo();
            if (!ExisteTablaPluginAuth()) return new PluginDeviceAuthInfo();

            const string sql = @"
SELECT TOP 1 DeviceId, MachineName, Allowed
FROM dbo.TSql_PluginDeviceAuth
WHERE LinAspNetUsert = @UserId
ORDER BY SysObjectID DESC";

            try
            {
                var row = db.Database.SqlQuery<PluginDeviceAuthInfo>(sql, new SqlParameter("@UserId", aspNetUserId)).FirstOrDefault();
                return row ?? new PluginDeviceAuthInfo();
            }
            catch
            {
                return new PluginDeviceAuthInfo();
            }
        }

        private bool UpsertPluginDeviceAuth(EmployeeViewModel model, string actorUserId)
        {
            if (model == null) return false;
            if (string.IsNullOrWhiteSpace(model.DeviceId)) return false;
            if (string.IsNullOrWhiteSpace(model.LinAspNetUsert)) return false;
            if (!ExisteTablaPluginAuth()) return false;

            const string sql = @"
IF EXISTS (SELECT 1 FROM dbo.TSql_PluginDeviceAuth WHERE DeviceId = @DeviceId)
BEGIN
    UPDATE dbo.TSql_PluginDeviceAuth
       SET LinAspNetUsert = @LinAspNetUsert,
           MachineName = @MachineName,
           Allowed = @Allowed,
           IsActive = @Allowed,
           IsRevoked = CASE WHEN @Allowed = 1 THEN 0 ELSE IsRevoked END,
           Estado = CASE WHEN @Allowed = 1 THEN 'Activo' ELSE 'Bloqueado' END,
           AttIsDeleted = 0,
           LinModifiedBy = @Actor,
           AttLastModification = GETUTCDATE()
     WHERE DeviceId = @DeviceId;
END
ELSE
BEGIN
    INSERT INTO dbo.TSql_PluginDeviceAuth
    (
        DeviceId, LinAspNetUsert, MachineName, UsuarioWindows, PluginVersion,
        Allowed, IsActive, IsRevoked, Estado, AttIsDeleted,
        LastCheckUtc, LinCreatedBy, AttCreated, LinModifiedBy, AttLastModification
    )
    VALUES
    (
        @DeviceId, @LinAspNetUsert, @MachineName, @UsuarioWindows, @PluginVersion,
        @Allowed, @Allowed, 0, CASE WHEN @Allowed = 1 THEN 'Activo' ELSE 'Bloqueado' END, 0,
        GETUTCDATE(), @Actor, GETUTCDATE(), @Actor, GETUTCDATE()
    );
END";

            var parameters = new List<SqlParameter>
            {
                new SqlParameter("@DeviceId", model.DeviceId),
                new SqlParameter("@LinAspNetUsert", model.LinAspNetUsert),
                new SqlParameter("@MachineName", (object)(model.DeviceName ?? string.Empty)),
                new SqlParameter("@UsuarioWindows", DBNull.Value),
                new SqlParameter("@PluginVersion", DBNull.Value),
                new SqlParameter("@Allowed", model.DeviceAllowed),
                new SqlParameter("@Actor", (object)(actorUserId ?? string.Empty))
            };

            try
            {
                db.Database.ExecuteSqlCommand(sql, parameters.ToArray());
                return true;
            }
            catch
            {
                return false;
            }
        }

        private bool ExisteTablaPluginAuth()
        {
            const string sql = @"
SELECT COUNT(1)
FROM INFORMATION_SCHEMA.TABLES
WHERE TABLE_SCHEMA = 'dbo'
  AND TABLE_NAME = 'TSql_PluginDeviceAuth'
  AND TABLE_TYPE = 'BASE TABLE'";

            try
            {
                return db.Database.SqlQuery<int>(sql).FirstOrDefault() > 0;
            }
            catch
            {
                return false;
            }
        }

        private void SendMail(MailModel _objModelMail)
        {
            if (ModelState.IsValid)
            {
                MailMessage mail = new MailMessage();
                mail.To.Add(_objModelMail.To);
                mail.From = new MailAddress(_objModelMail.From);
                mail.Subject = _objModelMail.Subject;
                mail.Body = _objModelMail.Body;
                mail.IsBodyHtml = true;
                SmtpClient smtp = new SmtpClient
                {
                    Host = "mail5005.smarterasp.net",
                    Port = 587,
                    UseDefaultCredentials = false,
                    Credentials = new System.Net.NetworkCredential("admin@atenko.net", "AngelyJuan01@"),
                    EnableSsl = true
                };
                smtp.Send(mail);
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
