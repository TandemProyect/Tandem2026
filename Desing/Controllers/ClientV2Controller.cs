using DAL;
using DataTables.Mvc;
using Desing.Helpers;
using Desing.Models;
using Microsoft.AspNet.Identity;
using System;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Web;
using System.Web.Mvc;

namespace Desing.Controllers
{
    public class ClientV2Controller : BaseController
    {
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult Details(long id)
        {
            var entity = db.TSql_Client_V2.FirstOrDefault(x => x.IdObject == id && !x.Is_Delete);
            if (entity == null)
            {
                return HttpNotFound();
            }
            return View(entity);
        }

        public ActionResult Create()
        {
            PopulateMethodOfPayment(null);
            var model = new TSql_Client_V2
            {
                Is_Active = true,
                Path_Ico = "",
                Path_Logo = ""
            };
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(
            [Bind(Include = "TextLabel,TextCode,TextTaxId,TextEmail,TextPhone,LinkMethodOfPayment,Is_Active,Path_Ico,Path_Logo")] TSql_Client_V2 model,
            HttpPostedFileBase icoFile,
            HttpPostedFileBase logoFile)
        {
            ApplyUploadedPaths(model, icoFile, logoFile);

            if (string.IsNullOrWhiteSpace(model.TextLabel))
            {
                ModelState.AddModelError("TextLabel", "El nombre del cliente es obligatorio.");
            }

            if (db.TSql_Client_V2.Any(x => !x.Is_Delete && x.TextLabel == model.TextLabel))
            {
                ModelState.AddModelError("TextLabel", "Ya existe un cliente con ese nombre.");
            }

            if (!ModelState.IsValid)
            {
                PopulateMethodOfPayment(model.LinkMethodOfPayment);
                return View(model);
            }

            if (string.IsNullOrEmpty(model.Path_Ico)) model.Path_Ico = "";
            if (string.IsNullOrEmpty(model.Path_Logo)) model.Path_Logo = "";

            IntranetAuditHelper.SetAuditOnCreate(model, User);

            db.TSql_Client_V2.Add(model);
            db.SaveChanges();

            TempData["ToastType"] = "Act";
            TempData["ToastTitle"] = "Cliente";
            TempData["ToastMessage"] = "Cliente creado correctamente.";
            return RedirectToAction("Index");
        }

        public ActionResult Edit(long id)
        {
            var entity = db.TSql_Client_V2.FirstOrDefault(x => x.IdObject == id && !x.Is_Delete);
            if (entity == null)
            {
                return HttpNotFound();
            }
            PopulateMethodOfPayment(entity.LinkMethodOfPayment);
            return View(entity);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(
            [Bind(Include = "IdObject,TextLabel,TextCode,TextTaxId,TextEmail,TextPhone,LinkMethodOfPayment,Is_Active,Path_Ico,Path_Logo")] TSql_Client_V2 model,
            HttpPostedFileBase icoFile,
            HttpPostedFileBase logoFile)
        {
            var entity = db.TSql_Client_V2.FirstOrDefault(x => x.IdObject == model.IdObject && !x.Is_Delete);
            if (entity == null)
            {
                return HttpNotFound();
            }

            ApplyUploadedPaths(model, icoFile, logoFile);

            if (string.IsNullOrWhiteSpace(model.TextLabel))
            {
                ModelState.AddModelError("TextLabel", "El nombre del cliente es obligatorio.");
            }

            if (db.TSql_Client_V2.Any(x => x.IdObject != model.IdObject && !x.Is_Delete && x.TextLabel == model.TextLabel))
            {
                ModelState.AddModelError("TextLabel", "Ya existe otro cliente con ese nombre.");
            }

            if (!ModelState.IsValid)
            {
                PopulateMethodOfPayment(model.LinkMethodOfPayment);
                return View(model);
            }

            entity.TextLabel = model.TextLabel;
            entity.TextCode = model.TextCode;
            entity.TextTaxId = model.TextTaxId;
            entity.TextEmail = model.TextEmail;
            entity.TextPhone = model.TextPhone;
            entity.LinkMethodOfPayment = model.LinkMethodOfPayment;
            entity.Is_Active = model.Is_Active;
            if (!string.IsNullOrEmpty(model.Path_Ico)) entity.Path_Ico = model.Path_Ico;
            if (!string.IsNullOrEmpty(model.Path_Logo)) entity.Path_Logo = model.Path_Logo;

            IntranetAuditHelper.SetAuditOnUpdate(entity, User);

            db.SaveChanges();

            TempData["ToastType"] = "Act";
            TempData["ToastTitle"] = "Cliente";
            TempData["ToastMessage"] = "Cliente actualizado correctamente.";
            return RedirectToAction("Index");
        }

        [OutputCache(Duration = 1)]
        public JsonResult ListClientV2([ModelBinder(typeof(DataTablesBinder))] IDataTablesRequest requestModel)
        {
            try
            {
                IQueryable<ClientV2ListItem> query = db.TSql_Client_V2
                    .Where(c => !c.Is_Delete)
                    .Select(c => new ClientV2ListItem
                    {
                        IdObject = c.IdObject,
                        TextLabel = c.TextLabel,
                        TextCode = c.TextCode,
                        Path_Ico = c.Path_Ico,
                        Path_Logo = c.Path_Logo,
                        Is_Active = c.Is_Active,
                        Is_Delete = c.Is_Delete
                    });

                var totalCount = query.Count();

                if (!string.IsNullOrEmpty(requestModel.Search.Value))
                {
                    var value = requestModel.Search.Value.Trim();
                    query = query.Where(p => (p.TextLabel ?? "").Contains(value) ||
                                             (p.TextCode ?? "").Contains(value));
                }

                var filteredCount = query.Count();

                var sortedColumns = requestModel.Columns.GetSortedColumns();
                var orderByString = string.Empty;
                foreach (var column in sortedColumns)
                {
                    var orderColumn = column.Data == "TextCode" ? "TextCode" : "TextLabel";
                    orderByString += orderByString != string.Empty ? "," : "";
                    orderByString += orderColumn + (column.SortDirection == Column.OrderDirection.Ascendant ? " asc" : " desc");
                }
                query = query.OrderBy(string.IsNullOrEmpty(orderByString) ? "TextLabel asc" : orderByString);
                query = query.ApplyDataTablesPaging(requestModel.Start, requestModel.Length);

                var data = query.ToList().Select(p => new
                {
                    IdObject = p.IdObject,
                    TextLabel = "<a href='" + Url.Action("Details", new { id = p.IdObject }) + "'>" + HttpUtility.HtmlEncode(p.TextLabel) + "</a>",
                    TextCode = p.TextCode ?? "",
                    logoPreview = string.IsNullOrEmpty(p.Path_Logo)
                        ? ""
                        : "<img src=\"" + Url.Content(p.Path_Logo.StartsWith("~") ? p.Path_Logo : "~" + p.Path_Logo) + "\" style=\"height:24px\" alt=\"\" />",
                    Is_Active = p.Is_Active,
                    activeBadge = p.Is_Active
                        ? "<span class=\"badge bg-label-success\">Activo</span>"
                        : "<span class=\"badge bg-label-secondary\">Inactivo</span>",
                    buttonEdit = "<a title='Editar' href='" + Url.Action("Edit", new { id = p.IdObject }) + "' class=\"btn btn-warning btn-xs\"><span class=\"fas fa-edit\"></span></a>",
                    buttonDelete = "<a title='Eliminar' onclick=\"deleteClientV2(" + p.IdObject + ")\" class=\"btn btn-danger btn-xs\"><span class=\"fas fa-trash-alt\"></span></a>"
                }).ToList();

                return Json(DataTablesMvcJson.Create(requestModel.Draw, data, filteredCount, totalCount), JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(ex.Message);
            }
        }

        [HttpPost]
        public JsonResult DeleteClientV2(long id)
        {
            var entity = db.TSql_Client_V2.FirstOrDefault(x => x.IdObject == id && !x.Is_Delete);
            if (entity == null)
            {
                return Json(new { IsOk = false, Message = "Cliente no encontrado." });
            }

            if (db.TSql_Jobside.Any(j => j.LinkClient_V2 == id && !j.Is_Delete))
            {
                return Json(new { IsOk = false, Message = "No se puede eliminar: tiene obras asociadas." });
            }

            IntranetAuditHelper.SetAuditOnDelete(entity, User);
            db.SaveChanges();

            return Json(new { IsOk = true, Message = "Cliente eliminado correctamente." });
        }

        private void ApplyUploadedPaths(TSql_Client_V2 model, HttpPostedFileBase icoFile, HttpPostedFileBase logoFile)
        {
            string error;
            var icoPath = IntranetFileHelper.TrySaveClientV2File(icoFile, "ico", out error);
            if (error != null)
            {
                ModelState.AddModelError("Path_Ico", error);
            }
            else if (!string.IsNullOrEmpty(icoPath))
            {
                model.Path_Ico = icoPath;
                ModelState.Remove("Path_Ico");
            }

            var logoPath = IntranetFileHelper.TrySaveClientV2File(logoFile, "logo", out error);
            if (error != null)
            {
                ModelState.AddModelError("Path_Logo", error);
            }
            else if (!string.IsNullOrEmpty(logoPath))
            {
                model.Path_Logo = logoPath;
                ModelState.Remove("Path_Logo");
            }
        }

        private void PopulateMethodOfPayment(long? selected)
        {
            // TSql_MethodOfPayment no está en el modelo; desplegable preparado para cuando exista la tabla.
            ViewBag.LinkMethodOfPayment = new SelectList(
                Enumerable.Empty<SelectListItem>(),
                "Value",
                "Text",
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
