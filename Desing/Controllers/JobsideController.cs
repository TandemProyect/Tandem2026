using DAL;
using DataTables.Mvc;
using Desing.Helpers;
using Desing.Models;
using Desing.Resources;
using System;
using System.Data.Entity;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Web;
using System.Web.Mvc;

namespace Desing.Controllers
{
    /// <summary>
    /// CRUD para obras (TSql_Jobside). Sigue el patron Materio + DataTables
    /// estandar (rowActions, TextLabelPlain, exportOptsPlainVisible) y delega
    /// los textos a Desing.Resources.Jobside (.resx + DbBackedResourceManager)
    /// reutilizando Desing.Resources.Common para botones y bloques de direccion.
    ///
    /// Auditoria estandar via IntranetAuditHelper (LinkMadeBy / LinModifiedBy /
    /// AddDateMade / AddLastDateChange / Ntimeschanged) y borrado logico
    /// (Is_Delete = true) con bloqueo si la obra tiene documentos u ofertas.
    /// </summary>
    [Authorize]
    public class JobsideController : BaseController
    {
        private const string JobsideBindFields =
            "TextContractRef,TextJobsideNotes,LinBranch," +
            "TextLabel,LinkClient_V2,Is_Active,BitBillSameAsLoc," +
            "Loc_Place_Id,Loc_Formatted_Address,Loc_Lat,Loc_Lng,Loc_Street_Number,Loc_Route,Loc_Subpremise," +
            "Loc_Locality,Loc_Admin_Area_1,Loc_Admin_Area_2,Loc_Postal_Code,Loc_Country_Code,Loc_Country_Name,Loc_Address_Components_Json," +
            "Bill_Place_Id,Bill_Formatted_Address,Bill_Lat,Bill_Lng,Bill_Street_Number,Bill_Route,Bill_Subpremise," +
            "Bill_Locality,Bill_Admin_Area_1,Bill_Admin_Area_2,Bill_Postal_Code,Bill_Country_Code,Bill_Country_Name,Bill_Address_Components_Json";

        // ---------------------------------------------------------------------
        // INDEX + DataTable (patron Materio + applyListDefaults)
        // ---------------------------------------------------------------------
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult Details(long id)
        {
            var entity = db.TSql_Jobside
                .Include("TSql_Client_V2")
                .Include("TSql_Branch")
                .FirstOrDefault(x => x.IdObject == id && !x.Is_Delete);
            if (entity == null)
            {
                return HttpNotFound();
            }

            var docTypes = db.TSql_DocumentType
                .Where(t => !t.Is_Delete && t.Is_Active)
                .OrderBy(t => t.TextLabel)
                .ToList();
            ViewBag.JobsideDocumentTypes = new SelectList(docTypes, "IdObject", "TextLabel");
            ViewBag.JobsideHasDocumentTypes = docTypes.Count > 0;

            return View(entity);
        }

        /// <summary>
        /// Sube un fichero y crea <see cref="TSql_Document"/> con
        /// <see cref="TSql_Document.LinkJobside"/> y auditoria estandar.
        /// El fichero queda en <c>~/Files/Jobside/{año}/</c> (año según Europa/Madrid),
        /// con nombre <c>{nombre obra}-{IdObject documento}{extension}</c>.
        /// Tamano: como minimo entre <see cref="TSql_DocumentType.NumberMaxFileSizeBytes"/>
        /// y la extension enlazanda (si el tipo tiene extensiones N:N).
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public JsonResult UploadJobsideDocument(long jobsideId, long documentTypeId, HttpPostedFileBase file, string description)
        {
            var jobsideRow = db.TSql_Jobside.FirstOrDefault(j => j.IdObject == jobsideId && !j.Is_Delete);
            if (jobsideRow == null)
            {
                return Json(new { ok = false, message = Jobside.Err_JobsideNotFound });
            }

            var docType = db.TSql_DocumentType
                .FirstOrDefault(t => t.IdObject == documentTypeId && !t.Is_Delete && t.Is_Active);
            if (docType == null)
            {
                return Json(new { ok = false, message = Jobside.Docs_Val_DocTypeInvalid });
            }

            if (file == null || file.ContentLength <= 0)
            {
                return Json(new { ok = false, message = Jobside.Docs_Val_NoFile });
            }

            var extFromName = Path.GetExtension(file.FileName);
            if (string.IsNullOrEmpty(extFromName))
            {
                return Json(new { ok = false, message = Jobside.Docs_Val_ExtensionMissing });
            }
            var extKey = extFromName.ToLowerInvariant();

            var bridgeRows = (from l in db.TSql_DocumentTypeExtension
                              join e in db.TSql_Extension on l.LinkExtension equals e.IdObject
                              where !l.Is_Delete && l.Is_Active && !e.Is_Delete && e.Is_Active
                                    && l.LinkDocumentType == documentTypeId
                              select new
                              {
                                  l.IdObject,
                                  e.TextLabel,
                                  e.NumberMaxFileSizeBytes
                              }).ToList();

            long? linkBridgeId = null;
            long maxBytes = docType.NumberMaxFileSizeBytes > 0
                ? docType.NumberMaxFileSizeBytes
                : long.MaxValue;

            if (bridgeRows.Count > 0)
            {
                var match = bridgeRows.FirstOrDefault(r => ExtensionLabelMatches(extKey, r.TextLabel));
                if (match == null)
                {
                    return Json(new { ok = false, message = Jobside.Docs_Val_ExtensionNotAllowed });
                }
                linkBridgeId = match.IdObject;
                var extCap = match.NumberMaxFileSizeBytes > 0
                    ? match.NumberMaxFileSizeBytes
                    : long.MaxValue;
                maxBytes = Math.Min(maxBytes, extCap);
            }

            if (maxBytes > 0 && maxBytes < long.MaxValue && file.ContentLength > maxBytes)
            {
                return Json(new
                {
                    ok = false,
                    message = string.Format(Jobside.Docs_Val_FileTooLarge, FormatBytes(maxBytes))
                });
            }

            var desc = (description ?? "").Trim();
            if (string.IsNullOrEmpty(desc))
            {
                desc = Path.GetFileName(file.FileName) ?? Jobside.Docs_Upload_File;
            }

            var year = JobsideCodeHelper.GetSpainLocalNow().Year;
            var folderRel = "~/Files/Jobside/" + year.ToString(CultureInfo.InvariantCulture) + "/";
            var folderPhysical = Server.MapPath(folderRel);
            if (!Directory.Exists(folderPhysical))
            {
                Directory.CreateDirectory(folderPhysical);
            }

            var tempLeaf = Guid.NewGuid().ToString("N", CultureInfo.InvariantCulture) + extKey;
            var physicalTemp = Path.Combine(folderPhysical, tempLeaf);
            var virtTemp = (folderRel + tempLeaf).Replace('\\', '/');

            try
            {
                file.SaveAs(physicalTemp);
            }
            catch (Exception ex)
            {
                return Json(new { ok = false, message = Jobside.Docs_Val_SaveFailed + " (" + ex.Message + ")" });
            }

            var physicalNormTemp = Path.GetFullPath(physicalTemp);
            var appRoot = Path.GetFullPath(Server.MapPath("~/"));
            if (!physicalNormTemp.StartsWith(appRoot, StringComparison.OrdinalIgnoreCase))
            {
                TryDeleteFileQuiet(physicalTemp);
                return Json(new { ok = false, message = Jobside.Docs_Val_SaveFailed });
            }

            string physicalFinal = null;
            using (var tran = db.Database.BeginTransaction())
            {
                try
                {
                    var doc = new TSql_Document
                    {
                        AddDescription = desc,
                        LinkJobside = jobsideId,
                        LinkDocumentType = documentTypeId,
                        LinkExtension = linkBridgeId,
                        AddPath = virtTemp
                    };
                    IntranetAuditHelper.SetAuditOnCreate(doc, User);
                    db.TSql_Document.Add(doc);
                    db.SaveChanges();

                    var stem = SanitizeJobsideDocumentFileStem(jobsideRow.TextLabel, jobsideId);
                    var finalLeaf = stem + "-" + doc.IdObject.ToString(CultureInfo.InvariantCulture) + extKey;
                    physicalFinal = Path.Combine(folderPhysical, finalLeaf);

                    if (System.IO.File.Exists(physicalFinal))
                    {
                        System.IO.File.Delete(physicalFinal);
                    }

                    System.IO.File.Move(physicalTemp, physicalFinal);

                    doc.AddPath = (folderRel + finalLeaf).Replace('\\', '/');
                    IntranetAuditHelper.SetAuditOnUpdate(doc, User);
                    db.SaveChanges();

                    tran.Commit();
                }
                catch (Exception ex)
                {
                    tran.Rollback();
                    TryDeleteFileQuiet(physicalTemp);
                    if (!string.IsNullOrEmpty(physicalFinal))
                    {
                        TryDeleteFileQuiet(physicalFinal);
                    }

                    return Json(new { ok = false, message = Jobside.Docs_Val_SaveFailed + " (" + ex.Message + ")" });
                }
            }

            return Json(new { ok = true, message = Jobside.Docs_Upload_Success });
        }

        /// <summary>
        /// Documentos asociados a la obra via <see cref="TSql_Document.LinkJobside"/>.
        /// </summary>
        [OutputCache(Duration = 1)]
        public JsonResult ListJobsideDocuments(long jobsideId, [ModelBinder(typeof(DataTablesBinder))] IDataTablesRequest requestModel)
        {
            try
            {
                if (!db.TSql_Jobside.Any(j => j.IdObject == jobsideId && !j.Is_Delete))
                {
                    return Json(
                        DataTablesMvcJson.Create(requestModel.Draw, new object[0], 0, 0),
                        JsonRequestBehavior.AllowGet);
                }

                IQueryable<JobsideDocumentListItem> query =
                    from d in db.TSql_Document
                    join t in db.TSql_DocumentType on d.LinkDocumentType equals t.IdObject into tg
                    from t in tg.DefaultIfEmpty()
                    where !d.Is_Delete && d.LinkJobside == jobsideId
                    select new JobsideDocumentListItem
                    {
                        IdObject = d.IdObject,
                        AddDescription = d.AddDescription,
                        AddPath = d.AddPath,
                        DocumentTypeName = t != null ? t.TextLabel : "",
                        AddDateMade = d.AddDateMade
                    };

                var totalCount = query.Count();

                if (!string.IsNullOrEmpty(requestModel.Search.Value))
                {
                    var value = requestModel.Search.Value.Trim();
                    query = query.Where(p => (p.AddDescription ?? "").Contains(value)
                                             || (p.DocumentTypeName ?? "").Contains(value)
                                             || (p.AddPath ?? "").Contains(value));
                }

                var filteredCount = query.Count();

                var sortedColumns = requestModel.Columns.GetSortedColumns();
                var orderByString = string.Empty;
                foreach (var column in sortedColumns)
                {
                    string orderColumn;
                    switch (column.Data)
                    {
                        case "displayName":
                        case "displayNamePlain":
                            orderColumn = "AddDescription";
                            break;
                        case "documentTypeName":
                            orderColumn = "DocumentTypeName";
                            break;
                        case "fileExtension":
                            orderColumn = "AddPath";
                            break;
                        case "addDateMade":
                        case "addDateMadeFmt":
                            orderColumn = "AddDateMade";
                            break;
                        default:
                            orderColumn = "AddDateMade";
                            break;
                    }
                    orderByString += orderByString != string.Empty ? "," : "";
                    orderByString += orderColumn +
                        (column.SortDirection == Column.OrderDirection.Ascendant ? " asc" : " desc");
                }
                query = query.OrderBy(string.IsNullOrEmpty(orderByString) ? "AddDateMade desc" : orderByString);
                query = query.ApplyDataTablesPaging(requestModel.Start, requestModel.Length);

                var rows = query.ToList();
                var ttDownload = HttpUtility.HtmlAttributeEncode(Jobside.Docs_DownloadTooltip);
                var lblNoFile = HttpUtility.HtmlEncode(Jobside.Docs_NoFile);

                var data = rows.Select(p =>
                {
                    var fileName = !string.IsNullOrWhiteSpace(p.AddPath)
                        ? Path.GetFileName(p.AddPath.Replace('\\', '/'))
                        : null;
                    var ext = !string.IsNullOrWhiteSpace(p.AddPath) ? Path.GetExtension(p.AddPath) : "";
                    if (string.IsNullOrEmpty(ext) && !string.IsNullOrEmpty(fileName))
                    {
                        ext = Path.GetExtension(fileName);
                    }

                    var displayPlain = !string.IsNullOrWhiteSpace(p.AddDescription)
                        ? p.AddDescription.Trim()
                        : (fileName ?? "");
                    if (string.IsNullOrEmpty(displayPlain))
                    {
                        displayPlain = Jobside.Details_NoValue;
                    }

                    var hasPath = !string.IsNullOrWhiteSpace(p.AddPath);
                    string downloadCell;
                    if (hasPath)
                    {
                        downloadCell =
                            "<a title=\"" + ttDownload + "\" href=\"" +
                            Url.Action("DownloadJobsideDocument", new { id = p.IdObject }) +
                            "\" class=\"btn btn-sm btn-outline-primary\"><span class=\"fas fa-download\" aria-hidden=\"true\"></span></a>";
                    }
                    else
                    {
                        downloadCell = "<span class=\"text-muted small\">" + lblNoFile + "</span>";
                    }

                    var actions = "<div class=\"d-inline-flex align-items-center gap-1\">" + downloadCell + "</div>";

                    return new
                    {
                        IdObject = p.IdObject,
                        displayName = HttpUtility.HtmlEncode(displayPlain),
                        displayNamePlain = displayPlain,
                        documentTypeName = HttpUtility.HtmlEncode(p.DocumentTypeName ?? ""),
                        fileExtension = HttpUtility.HtmlEncode(string.IsNullOrEmpty(ext) ? Jobside.Details_NoValue : ext),
                        addDateMade = p.AddDateMade,
                        addDateMadeFmt = HttpUtility.HtmlEncode(p.AddDateMade.ToString("g")),
                        rowActions = actions
                    };
                }).ToList();

                return Json(
                    DataTablesMvcJson.Create(requestModel.Draw, data, filteredCount, totalCount),
                    JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(ex.Message);
            }
        }

        /// <summary>
        /// Descarga el fichero de un documento de obra si <see cref="TSql_Document.AddPath"/>
        /// resuelve bajo el directorio de la aplicacion.
        /// </summary>
        public ActionResult DownloadJobsideDocument(long id)
        {
            var doc = db.TSql_Document.FirstOrDefault(d => d.IdObject == id && !d.Is_Delete);
            if (doc == null || !doc.LinkJobside.HasValue)
            {
                return HttpNotFound();
            }
            if (!db.TSql_Jobside.Any(j => j.IdObject == doc.LinkJobside.Value && !j.Is_Delete))
            {
                return HttpNotFound();
            }

            if (!TryResolveDocumentPhysicalPath(doc.AddPath, out var physical, out _))
            {
                return HttpNotFound();
            }

            var downloadName = Path.GetFileName(physical);
            return File(physical, "application/octet-stream", downloadName);
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
                                                       AddNJobside = j.AddNJobside ?? "",
                                                       AddNJobsideClient = j.AddNJobsideClient ?? "",
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
                    query = query.Where(p => (p.TextLabel ?? "").Contains(value)
                                          || (p.ClientName ?? "").Contains(value)
                                          || (p.Loc_Formatted_Address ?? "").Contains(value)
                                          || (p.AddNJobside ?? "").Contains(value)
                                          || (p.AddNJobsideClient ?? "").Contains(value));
                }

                var filteredCount = query.Count();

                var sortedColumns = requestModel.Columns.GetSortedColumns();
                var orderByString = string.Empty;
                foreach (var column in sortedColumns)
                {
                    string orderColumn;
                    switch (column.Data)
                    {
                        case "AddNJobside":
                            orderColumn = "AddNJobside"; break;
                        case "AddNJobsideClient":
                            orderColumn = "AddNJobsideClient"; break;
                        case "TextLabel":
                        case "TextLabelPlain":
                            orderColumn = "TextLabel"; break;
                        case "ClientName": orderColumn = "ClientName"; break;
                        case "Loc_Formatted_Address": orderColumn = "Loc_Formatted_Address"; break;
                        case "Is_Active":
                        case "activeBadge": orderColumn = "Is_Active"; break;
                        default: orderColumn = "AddNJobside"; break;
                    }
                    orderByString += orderByString != string.Empty ? "," : "";
                    orderByString += orderColumn +
                        (column.SortDirection == Column.OrderDirection.Ascendant ? " asc" : " desc");
                }
                query = query.OrderBy(string.IsNullOrEmpty(orderByString) ? "AddNJobside asc, TextLabel asc" : orderByString);
                query = query.ApplyDataTablesPaging(requestModel.Start, requestModel.Length);

                var rows = query.ToList();

                /* Dependencias para bloquear borrado (lookup en bulk para evitar N+1). */
                var ids = rows.Select(r => r.IdObject).ToList();
                var idsWithDocuments = db.TSql_Document
                    .Where(d => !d.Is_Delete && d.LinkJobside.HasValue && ids.Contains(d.LinkJobside.Value))
                    .Select(d => d.LinkJobside.Value)
                    .Distinct()
                    .ToList()
                    .ToHashSet();
                var idsWithOffers = db.TSql_Offers
                    .Where(o => !o.Is_Delete && ids.Contains(o.LinkJobside))
                    .Select(o => o.LinkJobside)
                    .Distinct()
                    .ToList()
                    .ToHashSet();

                var ttWorkspace = HttpUtility.HtmlAttributeEncode(Jobside.List_LinkWorkspaceTooltip);
                var ttEdit = HttpUtility.HtmlAttributeEncode(Jobside.List_LinkEditTooltip);
                var ttDelete = HttpUtility.HtmlAttributeEncode(Jobside.List_LinkDeleteTooltip);
                var ttDeleteDocuments = HttpUtility.HtmlAttributeEncode(Jobside.List_LinkDeleteLockedDocumentsTooltip);
                var ttDeleteOffers = HttpUtility.HtmlAttributeEncode(Jobside.List_LinkDeleteLockedOffersTooltip);
                var lblActive = HttpUtility.HtmlEncode(Jobside.State_Active);
                var lblInactive = HttpUtility.HtmlEncode(Jobside.State_Inactive);

                var data = rows.Select(p =>
                {
                    var namePlain = p.TextLabel ?? "";
                    var nameCell = HttpUtility.HtmlEncode(namePlain);

                    var workspaceBtn =
                        "<a title=\"" + ttWorkspace + "\" href=\"" +
                        Url.Action("Details", new { id = p.IdObject }) +
                        "\" class=\"btn btn-info btn-xs\"><span class=\"fas fa-eye\" aria-hidden=\"true\"></span></a>";

                    var activeBadge = p.Is_Active
                        ? "<span class=\"badge bg-label-success\">" + lblActive + "</span>"
                        : "<span class=\"badge bg-label-secondary\">" + lblInactive + "</span>";

                    var editBtn =
                        "<a title=\"" + ttEdit + "\" href=\"" +
                        Url.Action("Edit", new { id = p.IdObject }) +
                        "\" class=\"btn btn-warning btn-xs\"><span class=\"fas fa-edit\" aria-hidden=\"true\"></span></a>";

                    string deleteBtn;
                    string lockedTooltip = null;
                    if (idsWithDocuments.Contains(p.IdObject)) lockedTooltip = ttDeleteDocuments;
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
                            "<a title=\"" + ttDelete + "\" href=\"#\" onclick=\"DeleteJobside(" + p.IdObject +
                            "); return false;\" class=\"btn btn-danger btn-xs\"><span class=\"fas fa-trash-alt\" aria-hidden=\"true\"></span></a>";
                    }

                    var rowActions =
                        "<div class=\"d-inline-flex align-items-center gap-2\" role=\"group\">" +
                        workspaceBtn + editBtn + deleteBtn + "</div>";

                    var codePlain = p.AddNJobside ?? "";
                    var codeClientPlain = p.AddNJobsideClient ?? "";

                    return new
                    {
                        IdObject = p.IdObject,
                        AddNJobside = HttpUtility.HtmlEncode(codePlain),
                        AddNJobsidePlain = codePlain,
                        AddNJobsideClient = HttpUtility.HtmlEncode(codeClientPlain),
                        AddNJobsideClientPlain = codeClientPlain,
                        TextLabel = nameCell,
                        TextLabelPlain = namePlain,
                        ClientName = HttpUtility.HtmlEncode(p.ClientName ?? ""),
                        Loc_Formatted_Address = HttpUtility.HtmlEncode(p.Loc_Formatted_Address ?? ""),
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
            ModelState.Clear();
            PopulateClients(null);
            PopulateBranches(0);
            return View(new TSql_Jobside { Is_Active = true, BitBillSameAsLoc = true });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = JobsideBindFields)] TSql_Jobside model)
        {
            if (model.BitBillSameAsLoc)
            {
                CopyLocToBill(model);
            }

            ValidateJobsideServer(model, isCreate: true);

            if (!ModelState.IsValid)
            {
                PopulateClients(model.LinkClient_V2);
                PopulateBranches(model.LinBranch);
                return View(model);
            }

            model.TextJobsideNotes = string.IsNullOrWhiteSpace(model.TextJobsideNotes)
                ? null
                : model.TextJobsideNotes.Trim();

            IntranetAuditHelper.SetAuditOnCreate(model, User);
            model.AddNJobside = null;

            using (var tran = db.Database.BeginTransaction())
            {
                try
                {
                    db.TSql_Jobside.Add(model);
                    db.SaveChanges();

                    model.AddNJobside = JobsideCodeHelper.BuildAddNJobside(
                        model.IdObject,
                        JobsideCodeHelper.GetSpainLocalNow());
                    db.SaveChanges();
                    tran.Commit();
                }
                catch
                {
                    tran.Rollback();
                    throw;
                }
            }

            TempData["ToastType"] = "Act";
            TempData["ToastTitle"] = Jobside.ToastTitle_CreateJobside;
            TempData["ToastMessage"] = string.Format(Jobside.ToastMessage_JobsideCreated, model.TextLabel);
            return RedirectToAction("Index");
        }

        // ---------------------------------------------------------------------
        // EDIT
        // ---------------------------------------------------------------------
        public ActionResult Edit(long id)
        {
            var entity = db.TSql_Jobside.FirstOrDefault(x => x.IdObject == id && !x.Is_Delete);
            if (entity == null)
            {
                TempData["ToastType"] = "Error";
                TempData["ToastTitle"] = Jobside.ToastTitle_EditJobside;
                TempData["ToastMessage"] = Jobside.Err_JobsideNotFound;
                return RedirectToAction("Index");
            }
            PopulateClients(entity.LinkClient_V2);
            PopulateBranches(entity.LinBranch);
            return View(entity);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "IdObject," + JobsideBindFields)] TSql_Jobside model)
        {
            var entity = db.TSql_Jobside.FirstOrDefault(x => x.IdObject == model.IdObject && !x.Is_Delete);
            if (entity == null)
            {
                TempData["ToastType"] = "Error";
                TempData["ToastTitle"] = Jobside.ToastTitle_EditJobside;
                TempData["ToastMessage"] = Jobside.Err_JobsideNotFound;
                return RedirectToAction("Index");
            }

            if (model.BitBillSameAsLoc)
            {
                CopyLocToBill(model);
            }

            ValidateJobsideServer(model, isCreate: false);

            if (!ModelState.IsValid)
            {
                PopulateClients(model.LinkClient_V2);
                PopulateBranches(model.LinBranch);
                return View(model);
            }

            CopyJobsideFields(entity, model);
            /* AddNJobside no entra en Bind ni se copia: inmutable desde el primer guardado. */
            IntranetAuditHelper.SetAuditOnUpdate(entity, User);
            db.SaveChanges();

            TempData["ToastType"] = "Act";
            TempData["ToastTitle"] = Jobside.ToastTitle_EditJobside;
            TempData["ToastMessage"] = string.Format(Jobside.ToastMessage_JobsideUpdated, entity.TextLabel);
            return RedirectToAction("Index");
        }

        // ---------------------------------------------------------------------
        // DELETE (logico). Bloqueada si hay documentos u ofertas asociadas.
        // ---------------------------------------------------------------------
        [HttpPost]
        public JsonResult DeleteJobside(long id)
        {
            var entity = db.TSql_Jobside.FirstOrDefault(x => x.IdObject == id && !x.Is_Delete);
            if (entity == null)
            {
                return Json(new { IsOk = false, Message = Jobside.Err_JobsideNotFound });
            }

            if (db.TSql_Document.Any(d => !d.Is_Delete && d.LinkJobside == id))
            {
                return Json(new { IsOk = false, Message = Jobside.Err_CannotDeleteHasDocuments });
            }
            if (db.TSql_Offers.Any(o => !o.Is_Delete && o.LinkJobside == id))
            {
                return Json(new { IsOk = false, Message = Jobside.Err_CannotDeleteHasOffers });
            }

            var nombre = entity.TextLabel ?? "";

            IntranetAuditHelper.SetAuditOnDelete(entity, User);
            db.SaveChanges();

            return Json(new
            {
                IsOk = true,
                Message = string.Format(Jobside.ToastMessage_JobsideDeleted, nombre)
            });
        }

        // ---------------------------------------------------------------------
        // Validacion servidor (mensajes traducidos)
        // ---------------------------------------------------------------------
        private void ValidateJobsideServer(TSql_Jobside model, bool isCreate)
        {
            if (model == null) return;

            ClearFieldErrors("TextContractRef");
            model.TextContractRef = string.IsNullOrWhiteSpace(model.TextContractRef)
                ? string.Empty
                : model.TextContractRef.Trim();

            ClearFieldErrors("TextLabel");
            if (string.IsNullOrWhiteSpace(model.TextLabel))
            {
                ModelState.AddModelError("TextLabel", Jobside.Val_NameRequired);
            }
            else
            {
                var nameNorm = model.TextLabel.Trim();
                bool duplicate = isCreate
                    ? db.TSql_Jobside.Any(x => !x.Is_Delete
                                            && x.LinkClient_V2 == model.LinkClient_V2
                                            && x.TextLabel == nameNorm)
                    : db.TSql_Jobside.Any(x => !x.Is_Delete
                                            && x.IdObject != model.IdObject
                                            && x.LinkClient_V2 == model.LinkClient_V2
                                            && x.TextLabel == nameNorm);
                if (duplicate)
                {
                    ModelState.AddModelError("TextLabel",
                        isCreate ? Jobside.Val_DuplicateNameCreate : Jobside.Val_DuplicateNameEdit);
                }
            }

            ClearFieldErrors("LinkClient_V2");
            if (model.LinkClient_V2.HasValue)
            {
                var clientId = model.LinkClient_V2.Value;
                bool clientOk = db.TSql_Client_V2.Any(c => !c.Is_Delete && c.IdObject == clientId);
                if (!clientOk)
                {
                    ModelState.AddModelError("LinkClient_V2", Jobside.Val_ClientInvalid);
                }
            }

            ClearFieldErrors("LinBranch");
            if (model.LinBranch <= 0)
            {
                ModelState.AddModelError("LinBranch", Jobside.Val_BranchRequired);
            }
            else if (!db.TSql_Branch.Any(b => b.SysObjectID == model.LinBranch))
            {
                ModelState.AddModelError("LinBranch", Jobside.Val_BranchInvalid);
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

        // ---------------------------------------------------------------------
        // Helpers especificos del modulo
        // ---------------------------------------------------------------------
        private static void CopyJobsideFields(TSql_Jobside entity, TSql_Jobside model)
        {
            entity.TextContractRef = (model.TextContractRef ?? "").Trim();
            entity.TextJobsideNotes = string.IsNullOrWhiteSpace(model.TextJobsideNotes)
                ? null
                : model.TextJobsideNotes.Trim();
            entity.TextLabel = (model.TextLabel ?? "").Trim();
            entity.LinkClient_V2 = model.LinkClient_V2;
            entity.LinBranch = model.LinBranch;
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

        private bool TryResolveDocumentPhysicalPath(string addPath, out string physicalPath, out string error)
        {
            physicalPath = null;
            error = null;
            if (string.IsNullOrWhiteSpace(addPath))
            {
                error = "empty";
                return false;
            }

            var trimmed = addPath.Trim().Replace('\\', '/');
            try
            {
                string mapped;
                if (trimmed.StartsWith("~/", StringComparison.Ordinal))
                {
                    mapped = Server.MapPath(trimmed);
                }
                else if (trimmed.StartsWith("/", StringComparison.Ordinal))
                {
                    mapped = Server.MapPath("~" + trimmed);
                }
                else if (Path.IsPathRooted(trimmed))
                {
                    mapped = trimmed;
                }
                else
                {
                    mapped = Server.MapPath("~/" + trimmed.TrimStart('/'));
                }

                physicalPath = Path.GetFullPath(mapped);
                var appRoot = Path.GetFullPath(Server.MapPath("~/"));
                if (!physicalPath.StartsWith(appRoot, StringComparison.OrdinalIgnoreCase))
                {
                    error = "outside";
                    return false;
                }

                if (!System.IO.File.Exists(physicalPath))
                {
                    error = "missing";
                    return false;
                }

                return true;
            }
            catch (Exception ex)
            {
                error = ex.Message;
                return false;
            }
        }

        private static bool ExtensionLabelMatches(string fileExtLowerWithDot, string extensionCatalogLabel)
        {
            var norm = NormalizeExtensionLabel(extensionCatalogLabel);
            return norm != null && norm == fileExtLowerWithDot;
        }

        private static string NormalizeExtensionLabel(string label)
        {
            if (string.IsNullOrWhiteSpace(label)) return null;
            var s = label.Trim().ToLowerInvariant();
            if (!s.StartsWith(".", StringComparison.Ordinal))
            {
                s = "." + s;
            }
            return s;
        }

        private static void TryDeleteFileQuiet(string path)
        {
            if (string.IsNullOrWhiteSpace(path)) return;
            try
            {
                if (System.IO.File.Exists(path))
                {
                    System.IO.File.Delete(path);
                }
            }
            catch
            {
                /* ignore */
            }
        }

        /// <summary>
        /// Prefijo de nombre de fichero: nombre de obra (<see cref="TSql_Jobside.TextLabel"/>) saneado.
        /// Si falta, <c>Obra_{jobsideId}</c>. Longitud acotada para rutas Windows.
        /// </summary>
        private static string SanitizeJobsideDocumentFileStem(string textLabel, long jobsideId)
        {
            var raw = string.IsNullOrWhiteSpace(textLabel)
                ? "Obra_" + jobsideId.ToString(CultureInfo.InvariantCulture)
                : textLabel.Trim();
            foreach (var c in Path.GetInvalidFileNameChars())
            {
                raw = raw.Replace(c, '_');
            }

            raw = raw.Trim(' ', '.');
            if (string.IsNullOrEmpty(raw))
            {
                raw = "Obra_" + jobsideId.ToString(CultureInfo.InvariantCulture);
            }

            const int maxStem = 160;
            if (raw.Length > maxStem)
            {
                raw = raw.Substring(0, maxStem).TrimEnd('.', ' ', '_');
            }

            return raw;
        }

        private static string FormatBytes(long bytes)
        {
            const long KB = 1024L;
            const long MB = KB * 1024L;
            const long GB = MB * 1024L;
            if (bytes <= 0) return "0 B";
            if (bytes >= GB) return FormatUnit(bytes, GB, "GB");
            if (bytes >= MB) return FormatUnit(bytes, MB, "MB");
            if (bytes >= KB) return FormatUnit(bytes, KB, "KB");
            return bytes.ToString(CultureInfo.InvariantCulture) + " B";
        }

        private static string FormatUnit(long bytes, long unit, string suffix)
        {
            var value = bytes / (double)unit;
            return Math.Abs(value - Math.Round(value)) < 0.05
                ? string.Format(CultureInfo.InvariantCulture, "{0:0} {1}", value, suffix)
                : string.Format(CultureInfo.InvariantCulture, "{0:0.#} {1}", value, suffix);
        }

        private void PopulateClients(long? selected)
        {
            ViewBag.LinkClient_V2 = new SelectList(
                db.TSql_Client_V2
                    .Where(c => !c.Is_Delete && c.Is_Active)
                    .OrderBy(c => c.TextLabel)
                    .Select(c => new { c.IdObject, c.TextLabel })
                    .ToList(),
                "IdObject",
                "TextLabel",
                selected);
        }

        private void PopulateBranches(long selectedSysObjectId)
        {
            ViewBag.LinBranch = new SelectList(
                db.TSql_Branch
                    .OrderBy(b => b.AttLabel)
                    .Select(b => new { b.SysObjectID, b.AttLabel })
                    .ToList(),
                "SysObjectID",
                "AttLabel",
                selectedSysObjectId);
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
