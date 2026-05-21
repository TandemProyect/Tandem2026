using DAL;
using DataTables.Mvc;
using Desing.Helpers;
using Desing.Models;
using Desing.Resources;
using Microsoft.AspNet.Identity;
using System;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.Mvc;

namespace Desing.Controllers
{
    /// <summary>
    /// CRUD para clientes (TSql_Client_V2). Sigue el patron Materio + DataTables
    /// estandar (rowActions, TextLabelPlain, exportOptsPlainVisible) y delega los
    /// textos a Desing.Resources.ClientV2 (.resx + DbBackedResourceManager).
    /// </summary>
    [Authorize]
    public class ClientV2Controller : BaseController
    {
        private const string ClientV2GooglePlacesLocBindFields =
            "Loc_Place_Id,Loc_Formatted_Address,Loc_Lat,Loc_Lng,Loc_Street_Number,Loc_Route,Loc_Subpremise," +
            "Loc_Locality,Loc_Admin_Area_1,Loc_Admin_Area_2,Loc_Postal_Code,Loc_Country_Code,Loc_Country_Name,Loc_Address_Components_Json";

        private const string ClientV2CreateBindInclude =
            "TextLabel,TextCode,TextTaxId,TextEmail,TextPhone,LinkMethodOfPayment,Is_Active,Path_Ico,Path_Logo," +
            ClientV2GooglePlacesLocBindFields;

        private const string ClientV2EditBindInclude =
            "IdObject,TextLabel,TextCode,TextTaxId,TextEmail,TextPhone,LinkMethodOfPayment,Is_Active,Path_Ico,Path_Logo," +
            ClientV2GooglePlacesLocBindFields;

        // ---------------------------------------------------------------------
        // INDEX + DataTable (patron Materio + applyListDefaults)
        // ---------------------------------------------------------------------
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
                        TextTaxId = c.TextTaxId,
                        TextEmail = c.TextEmail,
                        TextPhone = c.TextPhone,
                        Path_Ico = c.Path_Ico,
                        Path_Logo = c.Path_Logo,
                        Is_Active = c.Is_Active,
                        Is_Delete = c.Is_Delete
                    });

                var totalCount = query.Count();

                if (!string.IsNullOrEmpty(requestModel.Search.Value))
                {
                    var value = requestModel.Search.Value.Trim();
                    query = query.Where(p => (p.TextLabel ?? "").Contains(value)
                                          || (p.TextCode ?? "").Contains(value)
                                          || (p.TextTaxId ?? "").Contains(value)
                                          || (p.TextEmail ?? "").Contains(value)
                                          || (p.TextPhone ?? "").Contains(value));
                }

                var filteredCount = query.Count();

                // Sort
                var sortedColumns = requestModel.Columns.GetSortedColumns();
                var orderByString = string.Empty;
                foreach (var column in sortedColumns)
                {
                    string orderColumn;
                    switch (column.Data)
                    {
                        case "TextLabel":
                        case "TextLabelPlain":
                            orderColumn = "TextLabel"; break;
                        case "TextCode": orderColumn = "TextCode"; break;
                        case "TextTaxId": orderColumn = "TextTaxId"; break;
                        case "TextEmail": orderColumn = "TextEmail"; break;
                        case "TextPhone": orderColumn = "TextPhone"; break;
                        case "Is_Active":
                        case "activeBadge": orderColumn = "Is_Active"; break;
                        default: orderColumn = "TextLabel"; break;
                    }
                    orderByString += orderByString != string.Empty ? "," : "";
                    orderByString += orderColumn +
                        (column.SortDirection == Column.OrderDirection.Ascendant ? " asc" : " desc");
                }
                query = query.OrderBy(string.IsNullOrEmpty(orderByString) ? "TextLabel asc" : orderByString);
                query = query.ApplyDataTablesPaging(requestModel.Start, requestModel.Length);

                var rows = query.ToList();

                // Dependencias para bloquear borrado (lookup en bulk para evitar N+1).
                var ids = rows.Select(r => r.IdObject).ToList();
                var idsWithJobsides = db.TSql_Jobside
                    .Where(j => !j.Is_Delete && j.LinkClient_V2.HasValue && ids.Contains(j.LinkClient_V2.Value))
                    .Select(j => j.LinkClient_V2.Value)
                    .Distinct()
                    .ToList()
                    .ToHashSet();
                var idsWithDocuments = db.TSql_Document
                    .Where(d => !d.Is_Delete && d.LinkClient_V2.HasValue && ids.Contains(d.LinkClient_V2.Value))
                    .Select(d => d.LinkClient_V2.Value)
                    .Distinct()
                    .ToList()
                    .ToHashSet();
                var idsWithOffers = db.TSql_Offers
                    .Where(o => !o.Is_Delete && ids.Contains(o.LinkClient_V2))
                    .Select(o => o.LinkClient_V2)
                    .Distinct()
                    .ToList()
                    .ToHashSet();

                var ttOpen = HttpUtility.HtmlAttributeEncode(ClientV2.List_LinkOpenTooltip);
                var ttEdit = HttpUtility.HtmlAttributeEncode(ClientV2.List_LinkEditTooltip);
                var ttDelete = HttpUtility.HtmlAttributeEncode(ClientV2.List_LinkDeleteTooltip);
                var ttDeleteJobsides = HttpUtility.HtmlAttributeEncode(ClientV2.List_LinkDeleteLockedJobsidesTooltip);
                var ttDeleteDocuments = HttpUtility.HtmlAttributeEncode(ClientV2.List_LinkDeleteLockedDocumentsTooltip);
                var ttDeleteOffers = HttpUtility.HtmlAttributeEncode(ClientV2.List_LinkDeleteLockedOffersTooltip);
                var lblActive = HttpUtility.HtmlEncode(ClientV2.State_Active);
                var lblInactive = HttpUtility.HtmlEncode(ClientV2.State_Inactive);

                var data = rows.Select(p =>
                {
                    var namePlain = p.TextLabel ?? "";
                    var nameCell =
                        "<a title=\"" + ttOpen + "\" href=\"" +
                        Url.Action("Details", new { id = p.IdObject }) + "\">" +
                        HttpUtility.HtmlEncode(namePlain) + "</a>";

                    var logoBoxStyle =
                        "display:inline-flex;align-items:center;justify-content:center;" +
                        "width:40px;height:40px;flex-shrink:0;border:1px solid #e8e8e8;" +
                        "border-radius:6px;background:#fff;box-sizing:border-box";
                    var logoImgStyle =
                        "width:40px;height:40px;object-fit:contain;display:block;" +
                        "padding:2px;box-sizing:border-box";
                    var logoPlain = "";
                    string logoPreview;
                    if (string.IsNullOrWhiteSpace(p.Path_Logo))
                    {
                        logoPreview =
                            "<span class=\"text-muted tandem-client-logo-empty\" style=\"" + logoBoxStyle + "\" title=\"\"></span>";
                    }
                    else
                    {
                        logoPlain = (p.Path_Logo ?? "").Trim();
                        var logoVp = IntranetFileHelper.NormalizeUploadedWebPath(p.Path_Logo);
                        var logoSrc = IntranetFileHelper.ResolvePublicUrl(Url, logoVp);
                        if (string.IsNullOrWhiteSpace(logoSrc))
                        {
                            logoPreview =
                                "<span class=\"text-muted tandem-client-logo-empty\" style=\"" + logoBoxStyle + "\" title=\"\"></span>";
                        }
                        else
                        {
                            logoPreview =
                                "<span class=\"tandem-client-logo-cell\" style=\"" + logoBoxStyle + "\">" +
                                "<img src=\"" + HttpUtility.HtmlAttributeEncode(logoSrc) + "\" " +
                                "style=\"" + logoImgStyle + "\" width=\"40\" height=\"40\" alt=\"\" /></span>";
                        }
                    }

                    var activeBadge = p.Is_Active
                        ? "<span class=\"badge bg-label-success\">" + lblActive + "</span>"
                        : "<span class=\"badge bg-label-secondary\">" + lblInactive + "</span>";

                    var editBtn =
                        "<a title=\"" + ttEdit + "\" href=\"" +
                        Url.Action("Edit", new { id = p.IdObject }) +
                        "\" class=\"btn btn-warning btn-xs\"><span class=\"fas fa-edit\" aria-hidden=\"true\"></span></a>";

                    string deleteBtn;
                    string lockedTooltip = null;
                    if (idsWithJobsides.Contains(p.IdObject)) lockedTooltip = ttDeleteJobsides;
                    else if (idsWithDocuments.Contains(p.IdObject)) lockedTooltip = ttDeleteDocuments;
                    else if (idsWithOffers.Contains(p.IdObject)) lockedTooltip = ttDeleteOffers;

                    if (lockedTooltip != null)
                    {
                        deleteBtn =
                            "<a title=\"" + lockedTooltip + "\" class=\"btn btn-secondary btn-xs disabled\" aria-disabled=\"true\" tabindex=\"-1\">" +
                            "<span class=\"fas fa-trash-alt\" aria-hidden=\"true\"></span></a>";
                    }
                    else
                    {
                        deleteBtn =
                            "<a title=\"" + ttDelete + "\" href=\"#\" onclick=\"DeleteClientV2(" + p.IdObject +
                            "); return false;\" class=\"btn btn-danger btn-xs\"><span class=\"fas fa-trash-alt\" aria-hidden=\"true\"></span></a>";
                    }

                    var rowActions =
                        "<div class=\"d-inline-flex align-items-center gap-2\" role=\"group\">" +
                        editBtn + deleteBtn + "</div>";

                    return new
                    {
                        IdObject = p.IdObject,
                        TextLabel = nameCell,
                        TextLabelPlain = namePlain,
                        TextCode = p.TextCode ?? "",
                        TextTaxId = p.TextTaxId ?? "",
                        TextEmail = p.TextEmail ?? "",
                        TextPhone = p.TextPhone ?? "",
                        logoPreview,
                        logoPlain,
                        Is_Active = p.Is_Active,
                        activeBadge,
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

        // ---------------------------------------------------------------------
        // CREATE
        // ---------------------------------------------------------------------
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
            [Bind(Include = ClientV2CreateBindInclude)] TSql_Client_V2 model,
            HttpPostedFileBase icoFile,
            HttpPostedFileBase logoFile)
        {
            ApplyUploadedPaths(model, icoFile, logoFile);
            ValidateClientServer(model, isCreate: true);

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
            TempData["ToastTitle"] = ClientV2.ToastTitle_CreateClient;
            TempData["ToastMessage"] = string.Format(ClientV2.ToastMessage_ClientCreated, model.TextLabel);
            return RedirectToAction("Index");
        }

        // ---------------------------------------------------------------------
        // EDIT
        // ---------------------------------------------------------------------
        public ActionResult Edit(long id)
        {
            var entity = db.TSql_Client_V2.FirstOrDefault(x => x.IdObject == id && !x.Is_Delete);
            if (entity == null)
            {
                TempData["ToastType"] = "Error";
                TempData["ToastTitle"] = ClientV2.ToastTitle_EditClient;
                TempData["ToastMessage"] = ClientV2.Err_ClientNotFound;
                return RedirectToAction("Index");
            }
            PopulateMethodOfPayment(entity.LinkMethodOfPayment);
            return View(entity);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(
            [Bind(Include = ClientV2EditBindInclude)] TSql_Client_V2 model,
            HttpPostedFileBase icoFile,
            HttpPostedFileBase logoFile)
        {
            var entity = db.TSql_Client_V2.FirstOrDefault(x => x.IdObject == model.IdObject && !x.Is_Delete);
            if (entity == null)
            {
                TempData["ToastType"] = "Error";
                TempData["ToastTitle"] = ClientV2.ToastTitle_EditClient;
                TempData["ToastMessage"] = ClientV2.Err_ClientNotFound;
                return RedirectToAction("Index");
            }

            ApplyUploadedPaths(model, icoFile, logoFile);
            ValidateClientServer(model, isCreate: false);

            if (!ModelState.IsValid)
            {
                PopulateMethodOfPayment(model.LinkMethodOfPayment);
                return View(model);
            }

            entity.TextLabel = (model.TextLabel ?? "").Trim();
            entity.TextCode = model.TextCode;
            entity.TextTaxId = model.TextTaxId;
            entity.TextEmail = model.TextEmail;
            entity.TextPhone = model.TextPhone;
            entity.LinkMethodOfPayment = model.LinkMethodOfPayment;
            entity.Is_Active = model.Is_Active;
            if (!string.IsNullOrEmpty(model.Path_Ico)) entity.Path_Ico = model.Path_Ico;
            if (!string.IsNullOrEmpty(model.Path_Logo)) entity.Path_Logo = model.Path_Logo;

            CopyClientV2GoogleLocFields(entity, model);

            IntranetAuditHelper.SetAuditOnUpdate(entity, User);

            db.SaveChanges();

            TempData["ToastType"] = "Act";
            TempData["ToastTitle"] = ClientV2.ToastTitle_EditClient;
            TempData["ToastMessage"] = string.Format(ClientV2.ToastMessage_ClientUpdated, entity.TextLabel);
            return RedirectToAction("Index");
        }

        // ---------------------------------------------------------------------
        // DELETE (logico). Bloqueada si hay obras, documentos u ofertas asociadas.
        // ---------------------------------------------------------------------
        [HttpPost]
        public JsonResult DeleteClientV2(long id)
        {
            var entity = db.TSql_Client_V2.FirstOrDefault(x => x.IdObject == id && !x.Is_Delete);
            if (entity == null)
            {
                return Json(new { IsOk = false, Message = ClientV2.Err_ClientNotFound });
            }

            if (db.TSql_Jobside.Any(j => j.LinkClient_V2 == id && !j.Is_Delete))
            {
                return Json(new { IsOk = false, Message = ClientV2.Err_CannotDeleteHasJobsides });
            }
            if (db.TSql_Document.Any(d => !d.Is_Delete && d.LinkClient_V2 == id))
            {
                return Json(new { IsOk = false, Message = ClientV2.Err_CannotDeleteHasDocuments });
            }
            if (db.TSql_Offers.Any(o => !o.Is_Delete && o.LinkClient_V2 == id))
            {
                return Json(new { IsOk = false, Message = ClientV2.Err_CannotDeleteHasOffers });
            }

            var nombre = entity.TextLabel ?? "";

            IntranetAuditHelper.SetAuditOnDelete(entity, User);
            db.SaveChanges();

            return Json(new
            {
                IsOk = true,
                Message = string.Format(ClientV2.ToastMessage_ClientDeleted, nombre)
            });
        }

        // ---------------------------------------------------------------------
        // Validacion servidor (mensajes traducidos)
        // ---------------------------------------------------------------------
        private void ValidateClientServer(TSql_Client_V2 model, bool isCreate)
        {
            if (model == null) return;

            // TextLabel: requerido + unico.
            ClearFieldErrors("TextLabel");
            if (string.IsNullOrWhiteSpace(model.TextLabel))
            {
                ModelState.AddModelError("TextLabel", ClientV2.Val_NameRequired);
            }
            else
            {
                var nameNorm = model.TextLabel.Trim();
                bool duplicate = isCreate
                    ? db.TSql_Client_V2.Any(x => !x.Is_Delete && x.TextLabel == nameNorm)
                    : db.TSql_Client_V2.Any(x => !x.Is_Delete && x.IdObject != model.IdObject && x.TextLabel == nameNorm);
                if (duplicate)
                {
                    ModelState.AddModelError("TextLabel",
                        isCreate ? ClientV2.Val_DuplicateNameCreate : ClientV2.Val_DuplicateNameEdit);
                }
            }

            // Email opcional: si esta presente, validar formato simple.
            ClearFieldErrors("TextEmail");
            if (!string.IsNullOrWhiteSpace(model.TextEmail))
            {
                if (!EmailRegex.IsMatch(model.TextEmail.Trim()))
                {
                    ModelState.AddModelError("TextEmail", ClientV2.Val_EmailFormat);
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

        private static readonly Regex EmailRegex = new Regex(
            @"^[^\s@]+@[^\s@]+\.[^\s@]+$",
            RegexOptions.Compiled | RegexOptions.IgnoreCase);

        // ---------------------------------------------------------------------
        // Helpers especificos del modulo (uploads, select de metodo de pago).
        // ---------------------------------------------------------------------
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

        private static void CopyClientV2GoogleLocFields(TSql_Client_V2 entity, TSql_Client_V2 model)
        {
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
        }

        private void PopulateMethodOfPayment(long? selected)
        {
            // TSql_MethodOfPayment aun no esta en el modelo EDMX; el desplegable
            // queda preparado para cuando se incorpore la tabla.
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
