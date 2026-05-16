using DAL;
using DataTables.Mvc;
using Desing.Helpers;
using Desing.Models;
using Microsoft.AspNet.Identity;
using System;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Net.Mail;
using System.Data.SqlClient;
using System.Collections.Generic;
using System.Web;
using System.Web.Mvc;
using static SendMail.Models;

namespace Desing.Controllers
{
    public class EmployeeController : BaseController
    {
        public object EmployeeId { get; private set; }

        public ActionResult SendMailUser(long Id)
        {
            try
            {
                var employeeId = db.TSql_Employee.FirstOrDefault(x => x.SysObjectID == Id);
                if (employeeId == null)
                {
                    return Content("Error: Empleado no encontrado");

                }

                var UserId = db.AspNetUsers.FirstOrDefault(x => x.Id == employeeId.LinAspNetUsert);
                if (UserId == null)
                {
                    return Content("Error: Usuario no encontrado");
                }
                if (employeeId == null) { return null; }
                MailModel Model = new MailModel();
                Model.To = UserId.Email;
                Model.From = "admin@atenko.net";
                Model.Subject = "Envio de contraseña";
                //Model.Body = @"<html><body><p><strong> El usuario:  "" + model.userSystem + "" a sido creado en el sistema con la contraseña:  "" + model.AttPassAspNetUsert</strong></p><br></br><p> Time for bed </p></body></html>";
                Model.Body = @"Envio de contraseña al usuario:  " + employeeId.AttName + " " + employeeId.AttSurname + " La contraseña requerida es:  " + employeeId.AttPassAspNetUsert;
                SendMail(Model);
                return Content("Success: El correo se envio correctamente.", "text/plain");
            }
            catch (Exception ex)
            {
                return RedirectToAction("Error500", ex.Message);
            }
        }


        //MySpace
        public ActionResult MySapce()
        {
            try
            {
                string UserId = User.Identity.GetUserId();
                AspNetUsers aspNetUser = db.AspNetUsers.Find(UserId);
                TSql_Employee employeeId = db.TSql_Employee.FirstOrDefault(x => x.LinAspNetUsert == UserId);
                var totalDesigns = db.TSql_Design.Where(x => x.LinCreatedBy == employeeId.LinAspNetUsert).Count();

                if (employeeId == null) { return null; }
                EmployeeViewMySpaceModel Model = new EmployeeViewMySpaceModel
                {
                    FullName = $"{employeeId.AttName} {employeeId.AttSurname}",
                    UserName = aspNetUser.UserName,
                    AttPhoto = employeeId.AttPhotoMenu,
                    EmailConfirmed = aspNetUser.EmailConfirmed,
                    AccountCreationDate = employeeId.AttCreated,
                    TotalDesigns = totalDesigns

                };
                return View("Myspace", Model);
            }
            catch (Exception ex)
            {
                return RedirectToAction("Error500", ex.Message);
            }
        }

        public ActionResult Delete()
        {
            try
            {
                var Id = Session["IDDesign"];
                TSql_Employee EmployeeId = db.TSql_Employee.Find(Id);
                return PartialView("_Delete", EmployeeId);
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

                //db.Movies.Remove(movie);
                string UserId = User.Identity.GetUserId();
                TSql_Employee Employee = db.TSql_Employee.Find(id);
                var Default = db.TSql_DefaultDesign.Where(x => x.LinkAspNetUsers == Employee.LinAspNetUsert).FirstOrDefault();
                if (Default != null)
                {
                    db.TSql_DefaultDesign.Remove(Default);
                }
                if (Employee != null)
                {
                    db.TSql_Employee.Remove(Employee);
                }
                db.SaveChanges();
                TempData.Clear();
                TempData["ToastType"] = "Act";
                TempData["ToastTitle"] = "Eliminar Usuario";
                TempData["ToastMessage"] = "El Usuario " + Employee.AttName + " a sido eliminado correctamente así como sus setup por defecto";
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                return RedirectToAction("Error500", ex.Message);
            }
        }

        //Active
        public ActionResult Active()
        {
            try
            {
                long Id = (long)Session["IDDesign"];

                TSql_Employee EmployeeId = db.TSql_Employee.FirstOrDefault(x => x.SysObjectID == Id);
                var UserId = db.AspNetUsers.Where(x => x.Id == EmployeeId.LinAspNetUsert);
                var ActiveNoActive = "El usuario esta activado";
                var Mesaje = "¿Quieres Desactivar el usuario " + EmployeeId.AttName + " ?";
                var Button = "Desactivar";
                bool ActDesc = UserId.FirstOrDefault().EmailConfirmed;
                if (ActDesc != true)
                {
                    ActiveNoActive = "El usuario esta desactivado";
                    Mesaje = "¿Quieres Activar el usuario " + EmployeeId.AttName + " ?";
                    Button = "Activar";
                }
                ViewBag.ActiveNoActive = ActiveNoActive;
                ViewBag.Mesaje = Mesaje;
                ViewBag.Button = Button;
                return PartialView("_Active", EmployeeId);
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
                TSql_Employee EmployeeId = db.TSql_Employee.FirstOrDefault(x => x.SysObjectID == id);

                var ActiveNoActive = true;
                var UserId = db.AspNetUsers.Where(x => x.Id == EmployeeId.LinAspNetUsert);
                var ActDesc = UserId.FirstOrDefault().EmailConfirmed;

                if (ActDesc == true)
                {
                    ActiveNoActive = false;
                }

                UserId.FirstOrDefault().EmailConfirmed = ActiveNoActive;
                db.SaveChanges();
                TempData.Clear();
                TempData["ToastType"] = "Act";
                TempData["ToastTitle"] = "Ativar o descativar Usuario";
                TempData["ToastMessage"] = "El Usuario " + EmployeeId.AttName + " a sido Modificado";
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                return Json(ex.Message);
            }
        }
        //Index
        public ActionResult Index()
        {
            TSql_Employee employee = db.TSql_Employee.Find(10007);

            return View(employee);
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

                // Apply filters
                if (requestModel.Search.Value != String.Empty)
                {
                    var value = requestModel.Search.Value.Trim();
                    query = query.Where(p => p.AttName.Contains(value) ||
                                             p.AttSurname.Contains(value)
                    );
                }

                var filteredCount = query.Count();
                // Sort
                var sortedColumns = requestModel.Columns.GetSortedColumns();
                var orderByString = String.Empty;
                string orderColumn = "";
                foreach (var column in sortedColumns)
                {
                    switch (column.Data)
                    {

                        case "UserName":
                            orderColumn = "UserName";
                            break;
                        case "AttName":
                            orderColumn = "AttName";
                            break;
                        case "AttSurname":
                            orderColumn = "AttSurname";
                            break;
                        case "AddLeter":
                            orderColumn = "AddLeter";
                            break;
                        case "AddCompany":
                            orderColumn = "AddCompany";
                            break;
                        case "AttPassAspNetUsert":
                            orderColumn = "AttPassAspNetUsert";
                            break;
                        case "AttCreated":
                            orderColumn = "AttCreated";
                            break;
                        case "EmailConfirmed":
                            orderColumn = "EmailConfirmed";
                            break;
                        case "TotalDesing":
                            orderColumn = "TotalDesing";
                            break;
                        default:
                            orderColumn = "AttName";
                            break;
                    }
                    orderByString += orderByString != String.Empty ? "," : "";
                    orderByString += (column.Data == "UserName" ? "UserName" : orderColumn) + (column.SortDirection == Column.OrderDirection.Ascendant ? " asc" : " desc");
                }
                query = query.OrderBy(orderByString == String.Empty ? "name asc" : orderByString);
                // Paging
                query = query.ApplyDataTablesPaging(requestModel.Start, requestModel.Length);

                // Rights
                bool allowEdit = true;
                bool allowDelete = true;

                var data = query.ToList().Select(p => new
                {
                    UserName = p.UserName,
                    emptyColumn = "",
                    Counter1 = 0,
                    Counter2 = 0,
                    TotalDesing = p.TotalDesing,
                    SysObjectID = p.SysObjectID,
                    AttName = p.AttName,
                    AttSurname = p.AttSurname,
                    AttPhotoMenu = p.AttPhotoMenu,
                    AddLeter = p.AddLeter,
                    AddCompany = p.AddCompany,
                    AttPassAspNetUsert = p.AttPassAspNetUsert,
                    AttCreated = p.AttCreated.ToShortDateString(),
                    EmailConfirmed = p.EmailConfirmed,
                    AttIsDeleted = false,
                    allowEdit = allowEdit,
                    allowDelete = allowDelete,
                    buttonActive = "<a title='Activar / Descativar Empleado'  onclick=ActiveDesactive('" + p.SysObjectID + "')   class=\"btn btn-info btn-xs\"><span class=\"fas fa-sync\" aria-hidden=\"true\"></span></a>",
                    buttonEdit = "<a title='Editar Empleado'  href='" + Url.Content("~/Employee/Edit_Employee/" + p.SysObjectID) + "' class=\"btn btn-warning btn-xs\"><span class=\"fas fa-edit\" aria-hidden=\"true\"></span></a>",
                    buttonDelete = "<a title=' Eliminar Empreado '            onclick=DeleteEmployee('" + p.SysObjectID + "')    class=\"btn btn-danger btn-xs\"  data-modalpaging><span class=\"fas fa-trash-alt\" aria-hidden=\"true\"></span></a>",
                    buttonSendMail = "<a title=' Enviar Mail '         onclick=SendmailToEmployee('" + p.SysObjectID + "') class=\"btn btn-success btn-xs\"><span class=\"fa fa-envelope-open\" aria-hidden=\"true\"></span></a>",
                }).ToList();
                return Json(DataTablesMvcJson.Create(requestModel.Draw, data, filteredCount, totalCount), JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(ex.Message);
            }
        }
        //Aqui Angel
        //onclick=SendmailToEmployee('" + p.SysObjectID + "')

        //Edit
        public ActionResult Edit_Employee(long Id)
        {
            try
            {
                TSql_Employee EmployeeId = db.TSql_Employee.FirstOrDefault(x => x.SysObjectID == Id);
                var UserId = db.AspNetUsers.Where(x => x.Id == EmployeeId.LinAspNetUsert);
                Session["EmployeeID"] = Id;
                Session["ItComeFrom"] = "Edit_Employee";
                Session["userSystem"] = UserId.FirstOrDefault().Id;
                Session["userVscad"] = UserId.FirstOrDefault().UserName;
                Session["passVscad"] = EmployeeId.AttPassAspNetUsert;
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

            ViewBag.MessageHeat = "Crear Usuario";
            if (iTComeFrom == "Edit_Employee")
            {
                ViewBag.MessageHeat = "Editar Usuario";
            };

            var userSystem = Session["userSystem"]?.ToString();
            var userVscad = Session["userVscad"]?.ToString();
            var passVscad = Session["passVscad"]?.ToString();
            if (string.IsNullOrWhiteSpace(userSystem) || string.IsNullOrWhiteSpace(userVscad))
            {
                // Sin contexto de usuario en sesión no se puede crear/editar empleado.
                return RedirectToAction("Index");
            }

            long employeeIdSesion;
            bool tieneEmployeeIdSesion = long.TryParse(Session["EmployeeID"]?.ToString(), out employeeIdSesion) && employeeIdSesion > 0;
            TSql_Employee EmployeeId = null;
            if (isEdit && tieneEmployeeIdSesion)
                EmployeeId = db.TSql_Employee.FirstOrDefault(x => x.SysObjectID == employeeIdSesion);
            if (EmployeeId == null)
                EmployeeId = db.TSql_Employee.FirstOrDefault(x => x.LinAspNetUsert == userSystem);
            //Session["EmployeeID"] = EmployeeId.SysObjectID;
            var attName = "";
            var attSurname = "";
            var attPhotoMenu = "~/Files/RRHH/User/AttPhotoMenu/user.png";
            var linCompany = 1;
            if (EmployeeId != null)
            {
                attName = EmployeeId.AttName;
                attSurname = EmployeeId.AttSurname;
                attPhotoMenu = EmployeeId.AttPhotoMenu;
                linCompany = (int)EmployeeId.LinCompany;
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
                EmployeeID = EmployeeId?.SysObjectID ?? 0,
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
                //var di2 = Server.MapPath("~") + @"\Files\RRHH\User\AttPhotoMenu\Temp\";
                //System.IO.DirectoryInfo di = new DirectoryInfo(di2);

                //foreach (FileInfo file in di.GetFiles())
                //{
                //    file.Delete();
                //}
                //foreach (DirectoryInfo dir in di.GetDirectories())
                //{
                //    dir.Delete(true);
                //}

                var iTComeFrom = Session["ItComeFrom"] as string ?? "";
                var isEdit = forceEdit || model.IsEdit || string.Equals(iTComeFrom, "Edit_Employee", StringComparison.OrdinalIgnoreCase);
                var ChangeFoto = true;
                long EmployeeID = 0;

                if (model.EmployeeID > 0) EmployeeID = model.EmployeeID;
                if (EmployeeID <= 0) long.TryParse(Session["EmployeeID"]?.ToString(), out EmployeeID);
                var EmployeeId = db.TSql_Employee.FirstOrDefault(x => x.SysObjectID == EmployeeID);
                if (EmployeeId != null)
                {
                    if (isEdit)
                    {
                        var check = "";
                        if (EmployeeId.AttPhotoMenu != null)
                        {
                            check = EmployeeId.AttPhotoMenu.Substring(35);
                        }
                        if (file1 == null)
                        {
                            ChangeFoto = false;
                        }
                        else
                        {
                            if (file1.FileName == check)
                            {
                                ChangeFoto = false;
                            }
                        }
                    }
                }

                string UserId = User.Identity.GetUserId();
                string nombre_original1 = "";
                string extension_original1 = "";
                string rutaTemp = "";
                string rutaNew = "";
                if (ChangeFoto == true)
                {
                    rutaTemp = Server.MapPath("~") + @"\Files\RRHH\User\AttPhotoMenu\Temp\";
                    rutaNew = Server.MapPath("~") + @"\Files\RRHH\User\AttPhotoMenu\";

                    if (file1 != null && file1.ContentLength > 0)
                    {
                        nombre_original1 = Path.GetFileName(file1.FileName);
                        extension_original1 = Path.GetExtension(file1.FileName);
                    }
                    else
                    {
                        model.AttPhotoMenu = "../../Files/RRHH/User/AttPhotoMenu/User.png";
                    }
                    if (file1 != null && file1.ContentLength > 0)
                    {
                        file1.SaveAs(Path.Combine(rutaTemp, "_" + nombre_original1));
                        ImageHelper.RedimensionarImagen(rutaTemp, "_" + nombre_original1, 100, 100, 0);

                        model.AttPhotoMenu = "../../Files/RRHH/User/AttPhotoMenu/Temp/__" + nombre_original1;
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
                    LinCreatedBy = UserId,
                    AttCreated = DateTime.UtcNow,
                    LinModifiedBy = UserId,
                    AttLastModification = DateTime.UtcNow,
                };
                if (iTComeFrom != "Edit_Employee")
                {
                    db.TSql_Employee.Add(newEmployee);
                    db.SaveChanges();
                };
                if (ChangeFoto == true)
                {
                    if (file1 != null && file1.ContentLength > 0)
                    {
                        newEmployee.AttPhotoMenu = "../../Files/RRHH/User/AttPhotoMenu/" + newEmployee.SysObjectID.ToString() + ".1" + extension_original1;
                        System.IO.File.Move(rutaTemp + @"\__" + nombre_original1,
                        rutaNew + @"\" + newEmployee.SysObjectID.ToString() + ".1" + extension_original1);
                    }
                }
                TSql_DefaultDesign Config = new TSql_DefaultDesign
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
                    LinkMadeBy = UserId,
                    LinModifiedBy = UserId,
                    AddChangeBy = UserId,
                    AddDateMade = DateTime.UtcNow,
                    AddLastDateChange = DateTime.UtcNow,
                    BitIsDeleted = false,
                };
                if (!isEdit)
                {
                    db.TSql_DefaultDesign.Add(Config);
                    db.SaveChanges();
                    var equipoRegistrado = UpsertPluginDeviceAuth(model, UserId);

                    MailModel Model = new MailModel();
                    Model.To = model.userSystem;
                    Model.From = "admin@atenko.net";
                    Model.Subject = "Alta en la aplicación Atenko.net";
                    //Model.Body = @"<html><body><p><strong> El usuario:  "" + model.userSystem + "" a sido creado en el sistema con la contraseña:  "" + model.AttPassAspNetUsert</strong></p><br></br><p> Time for bed </p></body></html>";
                    Model.Body = @"El usuario:  " + model.userSystem + " a sido creado en el sistema con la contraseña:  " + model.AttPassAspNetUsert;
                    if (equipoRegistrado)
                    {
                        Model.Body += @" | Equipo autorizado: " + (model.DeviceName ?? "(sin nombre)") + @" | DeviceId: " + model.DeviceId;
                    }
                    // Envío de correo desactivado temporalmente.
                    // Lo reactivaremos cuando se cierre la configuración SMTP/certificados.
                    // bool mailOk = true;
                    // string mailError = "";
                    // try
                    // {
                    //     SendMail(Model);
                    // }
                    // catch (System.Exception mailEx)
                    // {
                    //     // No bloquear el alta de empleado/equipo por fallo de SMTP.
                    //     mailOk = false;
                    //     mailError = mailEx.Message;
                    //     System.Diagnostics.Debug.WriteLine("Advertencia envío mail (no bloqueante): " + mailEx.Message);
                    // }
                    TempData.Clear();
                    TempData["ToastType"] = "Act";
                    TempData["ToastTitle"] = "Crear diseño";
                    TempData["ToastMessage"] = "Usuario/equipo guardado correctamente.";
                };
                if (isEdit)
                {

                    EmployeeId.AttName = newEmployee.AttName;
                    EmployeeId.AttSurname = newEmployee.AttSurname;
                    EmployeeId.AttPhoto = newEmployee.AttPhoto;
                    if (ChangeFoto == true)
                    {
                        EmployeeId.AttPhotoMenu = newEmployee.AttPhotoMenu;
                    }

                    EmployeeId.LinCompany = newEmployee.LinCompany;
                    EmployeeId.LinBusiness = newEmployee.LinBusiness;
                    EmployeeId.SysUpdateNumber = newEmployee.SysUpdateNumber;
                    EmployeeId.LinAspNetUsert = newEmployee.LinAspNetUsert;
                    EmployeeId.AttPassAspNetUsert = newEmployee.AttPassAspNetUsert;
                    EmployeeId.AttIsDeleted = false;
                    EmployeeId.Linlanguage = 1;
                    EmployeeId.LinCreatedBy = UserId;
                    EmployeeId.LinModifiedBy = UserId;
                    EmployeeId.AttLastModification = DateTime.UtcNow;
                    db.SaveChanges();
                    UpsertPluginDeviceAuth(model, UserId);
                    TempData.Clear();
                    TempData["ToastType"] = "Editar";
                    TempData["ToastTitle"] = "Editar Usuario";
                    TempData["ToastMessage"] = "El Usuario " + EmployeeId.AttName + " a sido Modificado";
                };
                Session.Clear();
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                return Json(ex.Message);
            }
        }

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
                string Body = _objModelMail.Body;
                mail.Body = Body;
                mail.IsBodyHtml = true;
                SmtpClient smtp = new SmtpClient();
                smtp.Host = "mail5005.smarterasp.net";
                smtp.Port = 587;
                smtp.UseDefaultCredentials = false;
                smtp.Credentials = new System.Net.NetworkCredential("admin@atenko.net", "AngelyJuan01@");
                smtp.EnableSsl = true;
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