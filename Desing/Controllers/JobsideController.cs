using DAL;
using DataTables.Mvc;
using Desing.Models;
using Desing.Helpers;
using Microsoft.AspNet.Identity;
using System;
using System.Data.Entity;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Web;
using System.Web.Mvc;

namespace Desing.Controllers
{
    public class JobsideController : BaseController
    {
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult Details(long id)
        {
            var entity = db.TSql_Jobside
                .Include("TSql_Client_V2")
                .FirstOrDefault(x => x.IdObject == id && !x.Is_Delete);
            if (entity == null)
            {
                return HttpNotFound();
            }
            return View(entity);
        }

        public ActionResult Create()
        {
            PopulateClients(null);
            return View(new TSql_Jobside { Is_Active = true, BitBillSameAsLoc = true });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = JobsideBindFields)] TSql_Jobside model)
        {
            if (string.IsNullOrWhiteSpace(model.TextLabel))
            {
                ModelState.AddModelError("TextLabel", "El nombre de la obra es obligatorio.");
            }

            if (model.BitBillSameAsLoc)
            {
                CopyLocToBill(model);
            }

            if (!ModelState.IsValid)
            {
                PopulateClients(model.LinkClient_V2);
                return View(model);
            }

            IntranetAuditHelper.SetAuditOnCreate(model, User);
            db.TSql_Jobside.Add(model);
            db.SaveChanges();

            TempData["ToastType"] = "Act";
            TempData["ToastTitle"] = "Obra";
            TempData["ToastMessage"] = "Obra creada correctamente.";
            return RedirectToAction("Index");
        }

        public ActionResult Edit(long id)
        {
            var entity = db.TSql_Jobside.FirstOrDefault(x => x.IdObject == id && !x.Is_Delete);
            if (entity == null)
            {
                return HttpNotFound();
            }
            PopulateClients(entity.LinkClient_V2);
            return View(entity);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "IdObject," + JobsideBindFields)] TSql_Jobside model)
        {
            var entity = db.TSql_Jobside.FirstOrDefault(x => x.IdObject == model.IdObject && !x.Is_Delete);
            if (entity == null)
            {
                return HttpNotFound();
            }

            if (string.IsNullOrWhiteSpace(model.TextLabel))
            {
                ModelState.AddModelError("TextLabel", "El nombre de la obra es obligatorio.");
            }

            if (model.BitBillSameAsLoc)
            {
                CopyLocToBill(model);
            }

            if (!ModelState.IsValid)
            {
                PopulateClients(model.LinkClient_V2);
                return View(model);
            }

            CopyJobsideFields(entity, model);
            IntranetAuditHelper.SetAuditOnUpdate(entity, User);
            db.SaveChanges();

            TempData["ToastType"] = "Act";
            TempData["ToastTitle"] = "Obra";
            TempData["ToastMessage"] = "Obra actualizada correctamente.";
            return RedirectToAction("Index");
        }

        [OutputCache(Duration = 1)]
        public JsonResult ListJobside([ModelBinder(typeof(DataTablesBinder))] IDataTablesRequest requestModel)
        {
            try
            {
                IQueryable<JobsideListItem> query = from j in db.TSql_Jobside
                                                   join c in db.TSql_Client_V2 on j.LinkClient_V2 equals c.IdObject into cg
                                                   from c in cg.DefaultIfEmpty()
                                                   where !j.Is_Delete
                                                   select new JobsideListItem
                                                   {
                                                       IdObject = j.IdObject,
                                                       TextLabel = j.TextLabel,
                                                       ClientName = c != null ? c.TextLabel : "",
                                                       Loc_Formatted_Address = j.Loc_Formatted_Address,
                                                       Is_Active = j.Is_Active,
                                                       Is_Delete = j.Is_Delete
                                                   };

                var totalCount = query.Count();

                if (!string.IsNullOrEmpty(requestModel.Search.Value))
                {
                    var value = requestModel.Search.Value.Trim();
                    query = query.Where(p => (p.TextLabel ?? "").Contains(value) ||
                                             (p.ClientName ?? "").Contains(value) ||
                                             (p.Loc_Formatted_Address ?? "").Contains(value));
                }

                var filteredCount = query.Count();

                var sortedColumns = requestModel.Columns.GetSortedColumns();
                var orderByString = string.Empty;
                foreach (var column in sortedColumns)
                {
                    var orderColumn = column.Data == "ClientName" ? "ClientName"
                        : column.Data == "Loc_Formatted_Address" ? "Loc_Formatted_Address"
                        : "TextLabel";
                    orderByString += orderByString != string.Empty ? "," : "";
                    orderByString += orderColumn + (column.SortDirection == Column.OrderDirection.Ascendant ? " asc" : " desc");
                }
                query = query.OrderBy(string.IsNullOrEmpty(orderByString) ? "TextLabel asc" : orderByString);
                query = query.ApplyDataTablesPaging(requestModel.Start, requestModel.Length);

                var data = query.ToList().Select(p => new
                {
                    IdObject = p.IdObject,
                    TextLabel = "<a href='" + Url.Action("Details", new { id = p.IdObject }) + "'>" + HttpUtility.HtmlEncode(p.TextLabel) + "</a>",
                    ClientName = HttpUtility.HtmlEncode(p.ClientName ?? ""),
                    Loc_Formatted_Address = HttpUtility.HtmlEncode(p.Loc_Formatted_Address ?? ""),
                    activeBadge = p.Is_Active
                        ? "<span class=\"badge bg-label-success\">Activo</span>"
                        : "<span class=\"badge bg-label-secondary\">Inactivo</span>",
                    buttonEdit = "<a title='Editar' href='" + Url.Action("Edit", new { id = p.IdObject }) + "' class=\"btn btn-warning btn-xs\"><span class=\"fas fa-edit\"></span></a>",
                    buttonDelete = "<a title='Eliminar' onclick=\"deleteJobside(" + p.IdObject + ")\" class=\"btn btn-danger btn-xs\"><span class=\"fas fa-trash-alt\"></span></a>"
                }).ToList();

                return Json(DataTablesMvcJson.Create(requestModel.Draw, data, filteredCount, totalCount), JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(ex.Message);
            }
        }

        [HttpPost]
        public JsonResult DeleteJobside(long id)
        {
            var entity = db.TSql_Jobside.FirstOrDefault(x => x.IdObject == id && !x.Is_Delete);
            if (entity == null)
            {
                return Json(new { IsOk = false, Message = "Obra no encontrada." });
            }

            IntranetAuditHelper.SetAuditOnDelete(entity, User);
            db.SaveChanges();

            return Json(new { IsOk = true, Message = "Obra eliminada correctamente." });
        }

        private const string JobsideBindFields =
            "TextLabel,LinkClient_V2,Is_Active,BitBillSameAsLoc," +
            "Loc_Place_Id,Loc_Formatted_Address,Loc_Lat,Loc_Lng,Loc_Street_Number,Loc_Route,Loc_Subpremise," +
            "Loc_Locality,Loc_Admin_Area_1,Loc_Admin_Area_2,Loc_Postal_Code,Loc_Country_Code,Loc_Country_Name,Loc_Address_Components_Json," +
            "Bill_Place_Id,Bill_Formatted_Address,Bill_Lat,Bill_Lng,Bill_Street_Number,Bill_Route,Bill_Subpremise," +
            "Bill_Locality,Bill_Admin_Area_1,Bill_Admin_Area_2,Bill_Postal_Code,Bill_Country_Code,Bill_Country_Name,Bill_Address_Components_Json";

        private static void CopyJobsideFields(TSql_Jobside entity, TSql_Jobside model)
        {
            entity.TextLabel = model.TextLabel;
            entity.LinkClient_V2 = model.LinkClient_V2;
            entity.Is_Active = model.Is_Active;
            entity.BitBillSameAsLoc = model.BitBillSameAsLoc;
            entity.Loc_Place_Id = model.Loc_Place_Id;
            entity.Loc_Formatted_Address = model.Loc_Formatted_Address;
            entity.Loc_Lat = model.Loc_Lat;
            entity.Loc_Lng = model.Loc_Lng;
            entity.Loc_Street_Number = model.Loc_Street_Number;
            entity.Loc_Route = model.Loc_Route;
            entity.Loc_Subpremise = model.Loc_Subpremise;
            entity.Loc_Locality = model.Loc_Locality;
            entity.Loc_Admin_Area_1 = model.Loc_Admin_Area_1;
            entity.Loc_Admin_Area_2 = model.Loc_Admin_Area_2;
            entity.Loc_Postal_Code = model.Loc_Postal_Code;
            entity.Loc_Country_Code = model.Loc_Country_Code;
            entity.Loc_Country_Name = model.Loc_Country_Name;
            entity.Loc_Address_Components_Json = model.Loc_Address_Components_Json;
            entity.Bill_Place_Id = model.Bill_Place_Id;
            entity.Bill_Formatted_Address = model.Bill_Formatted_Address;
            entity.Bill_Lat = model.Bill_Lat;
            entity.Bill_Lng = model.Bill_Lng;
            entity.Bill_Street_Number = model.Bill_Street_Number;
            entity.Bill_Route = model.Bill_Route;
            entity.Bill_Subpremise = model.Bill_Subpremise;
            entity.Bill_Locality = model.Bill_Locality;
            entity.Bill_Admin_Area_1 = model.Bill_Admin_Area_1;
            entity.Bill_Admin_Area_2 = model.Bill_Admin_Area_2;
            entity.Bill_Postal_Code = model.Bill_Postal_Code;
            entity.Bill_Country_Code = model.Bill_Country_Code;
            entity.Bill_Country_Name = model.Bill_Country_Name;
            entity.Bill_Address_Components_Json = model.Bill_Address_Components_Json;
        }

        private static void CopyLocToBill(TSql_Jobside model)
        {
            model.Bill_Place_Id = model.Loc_Place_Id;
            model.Bill_Formatted_Address = model.Loc_Formatted_Address;
            model.Bill_Lat = model.Loc_Lat;
            model.Bill_Lng = model.Loc_Lng;
            model.Bill_Street_Number = model.Loc_Street_Number;
            model.Bill_Route = model.Loc_Route;
            model.Bill_Subpremise = model.Loc_Subpremise;
            model.Bill_Locality = model.Loc_Locality;
            model.Bill_Admin_Area_1 = model.Loc_Admin_Area_1;
            model.Bill_Admin_Area_2 = model.Loc_Admin_Area_2;
            model.Bill_Postal_Code = model.Loc_Postal_Code;
            model.Bill_Country_Code = model.Loc_Country_Code;
            model.Bill_Country_Name = model.Loc_Country_Name;
            model.Bill_Address_Components_Json = model.Loc_Address_Components_Json;
        }

        private void PopulateClients(long? selected)
        {
            ViewBag.LinkClient_V2 = new SelectList(
                db.TSql_Client_V2.Where(c => !c.Is_Delete && c.Is_Active).OrderBy(c => c.TextLabel).Select(c => new { c.IdObject, c.TextLabel }).ToList(),
                "IdObject",
                "TextLabel",
                selected);
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
