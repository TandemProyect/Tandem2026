using DataTables.Mvc;
using Desing.Helpers;
using Desing.Models;
using Microsoft.AspNet.Identity;
using System;
using System.Data.Entity;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Web.Mvc;

namespace Desing.Controllers
{
    public class TSql_CompanyController : BaseController
    {
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult Details(long id)
        {
            var company = db.TSql_Company
                .Include(x => x.TSql_Countrys)
                .FirstOrDefault(x => x.SysObjectID == id);
            if (company == null)
            {
                return HttpNotFound();
            }
            return View(company);
        }

        public ActionResult Create()
        {
            PopulateCountries();
            PopulatePlantillas(null);
            return View(new DAL.TSql_Company());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "AddLeter,TextLabel,TextDescription,TextLogo,TextAddress_1,TextAddress_2,TextPostal_Code,TextTown_1,TextTown_2,LinkCountry,LinPlantilla")] DAL.TSql_Company model)
        {
            if (string.IsNullOrWhiteSpace(model.TextLabel))
            {
                ModelState.AddModelError("TextLabel", "El nombre de la empresa es obligatorio.");
            }

            if (db.TSql_Company.Any(x => !x.BitIsDeleted && x.TextLabel == model.TextLabel))
            {
                ModelState.AddModelError("TextLabel", "Ya existe una empresa con ese nombre.");
            }

            if (!ModelState.IsValid)
            {
                PopulateCountries(model.LinkCountry);
                PopulatePlantillas(model.LinPlantilla);
                return View(model);
            }

            var userId = User.Identity.GetUserId();
            model.LinkMadeBy = userId;
            model.AddChangeBy = userId;
            model.AddDateMade = DateTime.UtcNow;
            model.AddLastDateChange = DateTime.UtcNow;
            model.Ntimeschanged = 1;
            model.BitIsDeleted = false;
            db.TSql_Company.Add(model);
            db.SaveChanges();

            TempData["ToastType"] = "Act";
            TempData["ToastTitle"] = "Crear empresa";
            TempData["ToastMessage"] = "Empresa creada correctamente.";
            return RedirectToAction("Index");
        }

        public ActionResult Edit(long id)
        {
            var company = db.TSql_Company.FirstOrDefault(x => x.SysObjectID == id);
            if (company == null)
            {
                return HttpNotFound();
            }
            PopulateCountries(company.LinkCountry);
            PopulatePlantillas(company.LinPlantilla);
            return View(company);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "SysObjectID,AddLeter,TextLabel,TextDescription,TextLogo,TextAddress_1,TextAddress_2,TextPostal_Code,TextTown_1,TextTown_2,LinkCountry,LinPlantilla")] DAL.TSql_Company model)
        {
            var company = db.TSql_Company.FirstOrDefault(x => x.SysObjectID == model.SysObjectID);
            if (company == null)
            {
                return HttpNotFound();
            }

            if (string.IsNullOrWhiteSpace(model.TextLabel))
            {
                ModelState.AddModelError("TextLabel", "El nombre de la empresa es obligatorio.");
            }

            if (db.TSql_Company.Any(x => x.SysObjectID != model.SysObjectID && !x.BitIsDeleted && x.TextLabel == model.TextLabel))
            {
                ModelState.AddModelError("TextLabel", "Ya existe otra empresa con ese nombre.");
            }

            if (!ModelState.IsValid)
            {
                PopulateCountries(model.LinkCountry);
                PopulatePlantillas(model.LinPlantilla);
                return View(model);
            }

            company.AddLeter = model.AddLeter;
            company.TextLabel = model.TextLabel;
            company.TextDescription = model.TextDescription;
            company.TextLogo = model.TextLogo;
            company.TextAddress_1 = model.TextAddress_1;
            company.TextAddress_2 = model.TextAddress_2;
            company.TextPostal_Code = model.TextPostal_Code;
            company.TextTown_1 = model.TextTown_1;
            company.TextTown_2 = model.TextTown_2;
            company.LinkCountry = model.LinkCountry;
            company.LinPlantilla = model.LinPlantilla;
            company.AddChangeBy = User.Identity.GetUserId();
            company.AddLastDateChange = DateTime.UtcNow;
            company.Ntimeschanged = company.Ntimeschanged + 1;
            db.SaveChanges();

            TempData["ToastType"] = "Act";
            TempData["ToastTitle"] = "Editar empresa";
            TempData["ToastMessage"] = "Empresa actualizada correctamente.";
            return RedirectToAction("Index");
        }

        [OutputCache(Duration = 1)]
        public JsonResult ListTSql_Company([ModelBinder(typeof(DataTablesBinder))] IDataTablesRequest requestModel)
        {
            try
            {
                IQueryable<CompanyModel> query = from company in db.TSql_Company
                                                 join country in db.TSql_Countrys on company.LinkCountry equals country.IdObject into countryGroup
                                                 from country in countryGroup.DefaultIfEmpty()
                                                 select new CompanyModel
                                                 {
                                                     SysObjectID = company.SysObjectID,
                                                     AddLeter = company.AddLeter,
                                                     TextLabel = company.TextLabel,
                                                     TextLogo = company.TextLogo,
                                                     TextDescription = company.TextDescription,
                                                     TextAddress_1 = company.TextAddress_1,
                                                     TextAddress_2 = company.TextAddress_2,
                                                     TextPostal_Code = company.TextPostal_Code,
                                                     TextTown_1 = company.TextTown_1,
                                                     Country = country.TextLabel,
                                                     TextFlag = country.TextFlag,
                                                     BitIsDeleted = company.BitIsDeleted
                                                 };

                var totalCount = query.Count();

                // Apply filters
                if (requestModel.Search.Value != String.Empty)
                {
                    var value = requestModel.Search.Value.Trim();
                    query = query.Where(p => (p.AddLeter ?? "").Contains(value) ||
                                             (p.TextLabel ?? "").Contains(value) ||
                                             (p.TextDescription ?? "").Contains(value) ||
                                             (p.TextAddress_1 ?? "").Contains(value) ||
                                             (p.TextAddress_2 ?? "").Contains(value) ||
                                             (p.TextPostal_Code.HasValue && p.TextPostal_Code.Value.ToString().Contains(value)) ||
                                             (p.Country ?? "").Contains(value)

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
                        case "AddLeter":
                            orderColumn = "AddLeter";
                            break;
                        case "TextLabel":
                            orderColumn = "TextLabel";
                            break;
                        case "TextAddress_1":
                            orderColumn = "TextAddress_1";
                            break;
                        case "TextAddress_2":
                            orderColumn = "TextAddress_2";
                            break;
                        case "TextPostal_Code":
                            orderColumn = "TextPostal_Code";
                            break;
                        case "country":
                            orderColumn = "Country";
                            break;
                        default:
                            orderColumn = "TextLabel";
                            break;
                    }
                    orderByString += orderByString != String.Empty ? "," : "";
                    orderByString += (column.Data == "TextLabel" ? "TextLabel" : orderColumn) + (column.SortDirection == Column.OrderDirection.Ascendant ? " asc" : " desc");
                }
                query = query.OrderBy(orderByString == String.Empty ? "TextLabel asc" : orderByString);
                // Paging
                query = query.ApplyDataTablesPaging(requestModel.Start, requestModel.Length);

                var data = query.ToList().Select(p => new
                {
                    emptyColumn = "",
                    SysObjectID = p.SysObjectID,
                    TextLabel = "<a title='Abrir Empresa' href='" + Url.Content("~/TSql_Company/Details/" + p.SysObjectID) + "'>" + p.TextLabel + "</a>",
                    AddLeter = p.AddLeter,
                    TextLogo = p.TextLogo,
                    TextDescription = p.TextDescription,
                    TextAddress_1 = p.TextAddress_1,
                    TextAddress_2 = p.TextAddress_2,
                    TextPostal_Code = p.TextPostal_Code,
                    TextTown_1 = p.TextTown_1,
                    TextFlag = p.Country ?? "",
                    Country = p.Country ?? "",
                    BitIsDeleted = p.BitIsDeleted,
                    HasDependencies = HasDependencies(p.SysObjectID),
                    buttonEdit = "<a title='Editar empresa' href='" + Url.Content("~/TSql_Company/Edit/" + p.SysObjectID) + "' class=\"btn btn-warning btn-xs\"><span class=\"fas fa-edit\" aria-hidden=\"true\"></span></a>",
                    buttonDelete = "<a title='Eliminar empresa' onclick=\"DeleteCompany('" + p.SysObjectID + "')\" class=\"btn btn-danger btn-xs\"><span class=\"fas fa-trash-alt\" aria-hidden=\"true\"></span></a>",
                    buttonDisable = "<a title='Desactivar empresa' onclick=\"ToggleCompany('" + p.SysObjectID + "')\" class=\"btn btn-info btn-xs\"><span class=\"fas fa-sync\" aria-hidden=\"true\"></span></a>"
                }).ToList();
                return Json(new DataTablesResponse(requestModel.Draw, data, filteredCount, totalCount), JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(ex.Message);
            }
        }

        [HttpPost]
        public JsonResult DeleteCompany(long id)
        {
            var company = db.TSql_Company.FirstOrDefault(x => x.SysObjectID == id);
            if (company == null)
            {
                return Json(new { IsOk = false, Message = "Empresa no encontrada." });
            }

            if (HasDependencies(id))
            {
                return Json(new { IsOk = false, Message = "No se puede eliminar. La empresa tiene datos relacionados." });
            }

            db.TSql_Company.Remove(company);
            db.SaveChanges();
            return Json(new { IsOk = true, Message = "Empresa eliminada correctamente." });
        }

        [HttpPost]
        public JsonResult ToggleCompany(long id)
        {
            var company = db.TSql_Company.FirstOrDefault(x => x.SysObjectID == id);
            if (company == null)
            {
                return Json(new { IsOk = false, Message = "Empresa no encontrada." });
            }

            company.BitIsDeleted = !company.BitIsDeleted;
            company.AddChangeBy = User.Identity.GetUserId();
            company.AddLastDateChange = DateTime.UtcNow;
            company.Ntimeschanged = company.Ntimeschanged + 1;
            db.SaveChanges();

            return Json(new
            {
                IsOk = true,
                Message = company.BitIsDeleted ? "Empresa desactivada." : "Empresa activada."
            });
        }

        private bool HasDependencies(long companyId)
        {
            var hasEmployees = db.TSql_Employee.Any(x => x.LinCompany == companyId && !x.AttIsDeleted);
            var hasBranches = db.TSql_Branch.Any(x => x.LinCompany == companyId);
            return hasEmployees || hasBranches;
        }

        private void PopulateCountries(long? selected = null)
        {
            ViewBag.LinkCountry = new SelectList(
                db.TSql_Countrys.OrderBy(x => x.TextLabel).ToList(),
                "IdObject",
                "TextLabel",
                selected
            );
        }

        /// <summary>
        /// Lista de plantillas para el desplegable de empresa (incluye opcion vacia = usar plantilla global por defecto).
        /// </summary>
        private void PopulatePlantillas(long? selected = null)
        {
            var list = new System.Collections.Generic.List<SelectListItem>
            {
                new SelectListItem { Value = "", Text = "— Heredar plantilla global por defecto —" }
            };
            foreach (var item in PlantillaController.GetSelectList(db, selected))
            {
                item.Selected = false;
                list.Add(item);
            }
            if (!selected.HasValue)
                list[0].Selected = true;
            else
            {
                list[0].Selected = false;
                var match = list.FirstOrDefault(x => x.Value == selected.Value.ToString());
                if (match != null) match.Selected = true;
            }
            ViewBag.LinPlantilla = list;
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