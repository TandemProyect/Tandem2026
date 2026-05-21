using DAL;
using DataTables.Mvc;
using Desing.Helpers;
using Desing.Models;
using Desing.Resources;
using System.Collections.Generic;
using System;
using System.Configuration;
using System.Data;
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
        /// <summary>
        /// Valor por defecto si <c>appSettings:OfferDocumentTypeTextCode</c> no está definido o está vacío.
        /// La resolución del tipo de documento de oferta intenta <see cref="TSql_DocumentType.TextCode"/> (trim, sin distinguir mayúsculas),
        /// luego <see cref="TSql_DocumentType.TextLabel"/> igual, y por último <see cref="TSql_DocumentType.TextLabel"/> que contenga el token.
        /// </summary>
        private const string DefaultOfferDocumentTypeTextCode = "Oferta";

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
            ViewBag.JobsideHasOfferStates = db.TSql_OfferState.Any(s => !s.Is_Delete && s.Is_Active);

            return View(entity);
        }

        /// <summary>
        /// Espacio de trabajo de una oferta (lectura contextual de obra + oferta).
        /// Valida obra activa igual que otros GET de obra; mismo patrón de Includes que <see cref="SaveJobsideOffer"/>.
        /// </summary>
        public ActionResult OfferDetails(long id)
        {
            var offer = db.TSql_Offers
                .Include("TSql_Client_V2")
                .Include("TSql_OfferState")
                .Include("TSql_Jobside")
                .Include("TSql_Jobside.TSql_Client_V2")
                .Include("TSql_Jobside.TSql_Branch")
                .Include("TSql_Jobside.TSql_Branch.TSql_Company")
                .Include("TSql_Design_V2")
                .FirstOrDefault(o => o.IdObject == id && !o.Is_Delete);

            if (offer == null)
            {
                return HttpNotFound();
            }

            var jobside = offer.TSql_Jobside;
            if (jobside == null || jobside.Is_Delete)
            {
                return HttpNotFound();
            }

            ViewBag.OfferWorkspaceHasOfferDocType = TryGetOfferDocumentTypeId().HasValue;

            var designRows = offer.TSql_Design_V2
                .Where(d => !d.AttIsDeleted)
                .OrderBy(d => d.AttLabel)
                .Select(d => new JobsideOfferDesignRowVm
                {
                    DesignId = d.SysObjectID,
                    Title = d.AttLabel ?? string.Empty,
                    StlVirtualPath = ApplicationStlUrlHelper.TryGetTrustedStlVirtualPath(d.AttThumbnail)
                })
                .ToList();
            ViewBag.OfferWorkspaceDesigns = designRows;

            return View(offer);
        }

        /// <summary>
        /// Alta de <see cref="TSql_Design_V2"/> desde el espacio de trabajo de la oferta.
        /// Auditoría Diseño-V2: <see cref="TSql_Design_V2.LinCreatedBy"/>, <see cref="TSql_Design_V2.LinModifiedBy"/>,
        /// <see cref="TSql_Design_V2.AttCreated"/> / <see cref="TSql_Design_V2.AttChange"/> (patrón análogo a Intranet; la entidad no usa IdObject/TextLabel estándar).
        /// Borrado lógico propio de la tabla: <see cref="TSql_Design_V2.AttIsDeleted"/>.
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public JsonResult SaveOfferDesign(long offerId, long jobsideId, string attLabel, string attDescription, string attThumbnail)
        {
            var uid = IntranetAuditHelper.ResolveCurrentUserId(User);

            var offer = db.TSql_Offers
                .AsNoTracking()
                .FirstOrDefault(o => o.IdObject == offerId && !o.Is_Delete);

            if (offer == null)
            {
                return Json(new { ok = false, message = Jobside.Offers_Val_NotFound });
            }

            if (offer.LinkJobside != jobsideId)
            {
                return Json(new { ok = false, message = Jobside.Offers_Val_WrongJobside });
            }

            var jobsideRow = db.TSql_Jobside.AsNoTracking()
                .FirstOrDefault(j => j.IdObject == jobsideId && !j.Is_Delete);
            if (jobsideRow == null)
            {
                return Json(new { ok = false, message = Jobside.Err_JobsideNotFound });
            }

            var label = (attLabel ?? "").Trim();
            if (string.IsNullOrEmpty(label))
            {
                return Json(new { ok = false, message = Jobside.OfferWorkspace_Designs_Val_LabelRequired });
            }

            if (label.Length > 500)
            {
                label = label.Substring(0, 500);
            }

            string thumbStored = null;
            var thumbRaw = (attThumbnail ?? "").Trim();
            if (!string.IsNullOrEmpty(thumbRaw))
            {
                var trustedPath = ApplicationStlUrlHelper.TryGetTrustedStlVirtualPath(thumbRaw);
                if (trustedPath == null)
                {
                    return Json(new { ok = false, message = Jobside.OfferWorkspace_Designs_Val_InvalidStlPath });
                }

                thumbStored = trustedPath;
            }

            var descTrim = string.IsNullOrWhiteSpace(attDescription)
                ? null
                : attDescription.Trim();

            if (descTrim != null && descTrim.Length > 2000)
            {
                descTrim = descTrim.Substring(0, 2000);
            }

            var now = DateTime.Now;
            var entity = new TSql_Design_V2
            {
                AttLabel = label,
                AttDescription = descTrim,
                AttCenterX = 0d,
                AttCenterY = 0d,
                AttCreated = now,
                AttChange = now,
                AttIsDeleted = false,
                AttThumbnail = thumbStored,
                AttActiveCameraType = 0L,
                LinCreatedBy = uid,
                LinModifiedBy = uid,
                SysUpdateNumber = 0L,
                ItIsShared = false,
                ItIsSharedMyGrup = false,
                IsRenderAt60 = false,
                LinkOffers = offerId
            };

            db.TSql_Design_V2.Add(entity);
            db.SaveChanges();

            var rowVm = new JobsideOfferDesignRowVm
            {
                DesignId = entity.SysObjectID,
                Title = entity.AttLabel ?? string.Empty,
                StlVirtualPath = ApplicationStlUrlHelper.TryGetTrustedStlVirtualPath(entity.AttThumbnail)
            };

            var partialModel = new OfferWorkspaceDesignRowDisplayModel
            {
                OfferId = offerId,
                Row = rowVm
            };

            string rowHtml;
            try
            {
                rowHtml = RenderPartialViewToString("_OfferWorkspaceDesignRow", partialModel);
            }
            catch
            {
                return Json(new { ok = false, message = Jobside.OfferWorkspace_Designs_SaveFailed });
            }

            return Json(new
            {
                ok = true,
                message = Jobside.OfferWorkspace_Designs_SaveSuccess,
                designId = entity.SysObjectID,
                rowHtml
            });
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
                    join o in db.TSql_Offers on d.LinkOffer equals o.IdObject into og
                    from o in og.DefaultIfEmpty()
                    where !d.Is_Delete && d.LinkJobside == jobsideId
                    select new JobsideDocumentListItem
                    {
                        IdObject = d.IdObject,
                        AddDescription = d.AddDescription,
                        AddPath = d.AddPath,
                        DocumentTypeName = t != null ? t.TextLabel : "",
                        OfferNumberPlain = d.LinkOffer != null && o != null
                            ? ((o.AddOfferNumber ?? "").Trim())
                            : "",
                        AddDateMade = d.AddDateMade
                    };

                var totalCount = query.Count();

                if (!string.IsNullOrEmpty(requestModel.Search.Value))
                {
                    var value = requestModel.Search.Value.Trim();
                    query = query.Where(p => (p.AddDescription ?? "").Contains(value)
                                             || (p.DocumentTypeName ?? "").Contains(value)
                                             || (p.AddPath ?? "").Contains(value)
                                             || (p.OfferNumberPlain ?? "").Contains(value));
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
                        case "offerNumber":
                        case "offerNumberPlain":
                            orderColumn = "OfferNumberPlain";
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

                    var offerPlain = string.IsNullOrWhiteSpace(p.OfferNumberPlain)
                        ? string.Empty
                        : p.OfferNumberPlain.Trim();

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
                        offerNumber = HttpUtility.HtmlEncode(offerPlain),
                        offerNumberPlain = offerPlain,
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

        /// <summary>
        /// Sube un documento de oferta: <see cref="TSql_Document.LinkOffer"/> + <see cref="TSql_Document.LinkJobside"/>,
        /// tipo resuelto por <see cref="TryGetOfferDocumentTypeId"/> (catálogo <see cref="TSql_DocumentType"/>).
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public JsonResult UploadOfferDocument(long offerId, long jobsideId, HttpPostedFileBase file, string description)
        {
            if (!TryGetOfferInJobside(offerId, jobsideId, out var offerRow))
            {
                return Json(new { ok = false, message = Jobside.Offers_Val_NotFound });
            }

            var documentTypeId = TryGetOfferDocumentTypeId();
            if (!documentTypeId.HasValue)
            {
                return Json(new { ok = false, message = Jobside.Err_OfferDocumentTypeMissing });
            }

            var docType = db.TSql_DocumentType
                .FirstOrDefault(t => t.IdObject == documentTypeId.Value && !t.Is_Delete);
            if (docType == null)
            {
                return Json(new { ok = false, message = Jobside.Err_OfferDocumentTypeMissing });
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
                                    && l.LinkDocumentType == documentTypeId.Value
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
            var folderRel = "~/Files/Offer/" + year.ToString(CultureInfo.InvariantCulture) + "/";
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
                        LinkOffer = offerId,
                        LinkDocumentType = documentTypeId.Value,
                        LinkExtension = linkBridgeId,
                        AddPath = virtTemp
                    };
                    IntranetAuditHelper.SetAuditOnCreate(doc, User);
                    db.TSql_Document.Add(doc);
                    db.SaveChanges();

                    var stem = SanitizeOfferDocumentFileStem(offerRow.AddOfferNumber, offerRow.TextLabel, offerId);
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
        /// Documentos del espacio de oferta filtrados por <see cref="TSql_Document.LinkOffer"/>.
        /// </summary>
        [OutputCache(Duration = 1)]
        public JsonResult ListOfferDocuments(long offerId, long jobsideId, [ModelBinder(typeof(DataTablesBinder))] IDataTablesRequest requestModel)
        {
            try
            {
                if (!TryGetOfferInJobside(offerId, jobsideId, out _))
                {
                    return Json(
                        DataTablesMvcJson.Create(requestModel.Draw, new object[0], 0, 0),
                        JsonRequestBehavior.AllowGet);
                }

                IQueryable<JobsideDocumentListItem> query =
                    from d in db.TSql_Document
                    join t in db.TSql_DocumentType on d.LinkDocumentType equals t.IdObject into tg
                    from t in tg.DefaultIfEmpty()
                    join o in db.TSql_Offers on d.LinkOffer equals o.IdObject into og
                    from o in og.DefaultIfEmpty()
                    where !d.Is_Delete && d.LinkOffer == offerId
                    select new JobsideDocumentListItem
                    {
                        IdObject = d.IdObject,
                        AddDescription = d.AddDescription,
                        AddPath = d.AddPath,
                        DocumentTypeName = t != null ? t.TextLabel : "",
                        OfferNumberPlain = o != null ? ((o.AddOfferNumber ?? "").Trim()) : "",
                        AddDateMade = d.AddDateMade
                    };

                var totalCount = query.Count();

                if (!string.IsNullOrEmpty(requestModel.Search.Value))
                {
                    var value = requestModel.Search.Value.Trim();
                    query = query.Where(p => (p.AddDescription ?? "").Contains(value)
                                             || (p.DocumentTypeName ?? "").Contains(value)
                                             || (p.AddPath ?? "").Contains(value)
                                             || (p.OfferNumberPlain ?? "").Contains(value));
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
                        case "offerNumber":
                        case "offerNumberPlain":
                            orderColumn = "OfferNumberPlain";
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
                var ttDelete = HttpUtility.HtmlAttributeEncode(Jobside.Docs_Offer_DeleteTooltip);
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

                    var offerPlain = string.IsNullOrWhiteSpace(p.OfferNumberPlain)
                        ? string.Empty
                        : p.OfferNumberPlain.Trim();

                    var hasPath = !string.IsNullOrWhiteSpace(p.AddPath);
                    string downloadCell;
                    if (hasPath)
                    {
                        downloadCell =
                            "<a title=\"" + ttDownload + "\" href=\"" +
                            Url.Action("DownloadOfferDocument", new { id = p.IdObject }) +
                            "\" class=\"btn btn-sm btn-outline-primary\"><span class=\"fas fa-download\" aria-hidden=\"true\"></span></a>";
                    }
                    else
                    {
                        downloadCell = "<span class=\"text-muted small\">" + lblNoFile + "</span>";
                    }

                    var idStr = p.IdObject.ToString(CultureInfo.InvariantCulture);
                    var deleteBtn =
                        "<button type=\"button\" class=\"btn btn-sm btn-outline-danger tandem-offer-doc-delete\" " +
                        "data-doc-id=\"" + idStr + "\" title=\"" + ttDelete + "\">" +
                        "<span class=\"fas fa-trash\" aria-hidden=\"true\"></span></button>";

                    var actions = "<div class=\"d-inline-flex align-items-center gap-1\">" + downloadCell + deleteBtn + "</div>";

                    return new
                    {
                        IdObject = p.IdObject,
                        displayName = HttpUtility.HtmlEncode(displayPlain),
                        displayNamePlain = displayPlain,
                        documentTypeName = HttpUtility.HtmlEncode(p.DocumentTypeName ?? ""),
                        offerNumber = HttpUtility.HtmlEncode(offerPlain),
                        offerNumberPlain = offerPlain,
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
        /// Descarga un fichero de documento de oferta (<see cref="TSql_Document.LinkOffer"/>).
        /// </summary>
        public ActionResult DownloadOfferDocument(long id)
        {
            var doc = db.TSql_Document.FirstOrDefault(d => d.IdObject == id && !d.Is_Delete);
            if (doc == null || !doc.LinkOffer.HasValue)
            {
                return HttpNotFound();
            }

            var offer = db.TSql_Offers.FirstOrDefault(o => o.IdObject == doc.LinkOffer.Value && !o.Is_Delete);
            if (offer == null)
            {
                return HttpNotFound();
            }

            if (!db.TSql_Jobside.Any(j => j.IdObject == offer.LinkJobside && !j.Is_Delete))
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

        /// <summary>Borrado lógico de un documento de oferta.</summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public JsonResult DeleteOfferDocument(long id, long offerId, long jobsideId)
        {
            if (!TryGetOfferInJobside(offerId, jobsideId, out _))
            {
                return Json(new { ok = false, message = Jobside.Offers_Val_NotFound });
            }

            var doc = db.TSql_Document.FirstOrDefault(d =>
                d.IdObject == id && !d.Is_Delete && d.LinkOffer == offerId);
            if (doc == null)
            {
                return Json(new { ok = false, message = Jobside.Docs_Offer_DeleteFailed });
            }

            try
            {
                IntranetAuditHelper.SetAuditOnDelete(doc, User);
                db.SaveChanges();
                return Json(new { ok = true, message = Jobside.Docs_Offer_DeleteSuccess });
            }
            catch (Exception ex)
            {
                return Json(new { ok = false, message = Jobside.Docs_Offer_DeleteFailed + " (" + ex.Message + ")" });
            }
        }

        /// <summary>
        /// Celda cliente (logo ~40px + nombre) para DataTables de ofertas en obra; mismo criterio que <see cref="ClientV2Controller"/>.
        /// </summary>
        private string BuildJobsideOffersClientCellHtml(string clientNamePlain, string pathLogo)
        {
            var nameEnc = HttpUtility.HtmlEncode(clientNamePlain ?? "");
            const string logoBoxStyle =
                "display:inline-flex;align-items:center;justify-content:center;" +
                "width:40px;height:40px;flex-shrink:0;border:1px solid #e8e8e8;" +
                "border-radius:6px;background:#fff;box-sizing:border-box";
            const string logoImgStyle =
                "width:40px;height:40px;object-fit:contain;display:block;" +
                "padding:2px;box-sizing:border-box";

            string logoPart;
            if (string.IsNullOrWhiteSpace(pathLogo))
            {
                logoPart =
                    "<span class=\"text-muted tandem-client-logo-empty\" style=\"" + logoBoxStyle + "\" title=\"\"></span>";
            }
            else
            {
                var logoVp = IntranetFileHelper.NormalizeUploadedWebPath(pathLogo);
                var logoSrc = IntranetFileHelper.ResolvePublicUrl(Url, logoVp);
                if (string.IsNullOrWhiteSpace(logoSrc))
                {
                    logoPart =
                        "<span class=\"text-muted tandem-client-logo-empty\" style=\"" + logoBoxStyle + "\" title=\"\"></span>";
                }
                else
                {
                    logoPart =
                        "<span class=\"tandem-client-logo-cell\" style=\"" + logoBoxStyle + "\">" +
                        "<img src=\"" + HttpUtility.HtmlAttributeEncode(logoSrc) + "\" " +
                        "style=\"" + logoImgStyle + "\" width=\"40\" height=\"40\" alt=\"\" /></span>";
                }
            }

            return "<div class=\"d-flex align-items-center gap-2 tandem-jobside-offer-client-cell\">" +
                   logoPart + "<span class=\"text-break\">" + nameEnc + "</span></div>";
        }

        /// <summary>
        /// Ofertas (<see cref="TSql_Offers"/>) asociadas a la obra via
        /// <see cref="TSql_Offers.LinkJobside"/>.
        /// </summary>
        [OutputCache(Duration = 1)]
        public JsonResult ListJobsideOffers(long jobsideId, [ModelBinder(typeof(DataTablesBinder))] IDataTablesRequest requestModel)
        {
            try
            {
                if (!db.TSql_Jobside.Any(j => j.IdObject == jobsideId && !j.Is_Delete))
                {
                    return Json(
                        DataTablesMvcJson.Create(requestModel.Draw, new object[0], 0, 0),
                        JsonRequestBehavior.AllowGet);
                }

                IQueryable<JobsideOfferListItem> query =
                    from o in db.TSql_Offers
                    join s in db.TSql_OfferState on o.LinkOfferState equals s.IdObject into sg
                    from s in sg.DefaultIfEmpty()
                    join cl in db.TSql_Client_V2 on o.LinkClient_V2 equals cl.IdObject into clg
                    from cl in clg.DefaultIfEmpty()
                    where !o.Is_Delete && o.LinkJobside == jobsideId
                    select new JobsideOfferListItem
                    {
                        IdObject = o.IdObject,
                        AddOfferNumber = o.AddOfferNumber,
                        TextLabel = o.TextLabel,
                        OfferStateAddColor = s != null ? s.AddColor : null,
                        ClientName = cl != null ? cl.TextLabel : "",
                        ClientPathLogo = cl != null ? cl.Path_Logo : null,
                        StateName = s != null ? s.TextLabel : "",
                        Is_Active = o.Is_Active,
                        AddDateMade = o.AddDateMade
                    };

                var totalCount = query.Count();

                if (!string.IsNullOrEmpty(requestModel.Search.Value))
                {
                    var value = requestModel.Search.Value.Trim();
                    query = query.Where(p => (p.AddOfferNumber ?? "").Contains(value)
                                             || (p.TextLabel ?? "").Contains(value)
                                             || (p.StateName ?? "").Contains(value)
                                             || (p.ClientName ?? "").Contains(value));
                }

                var filteredCount = query.Count();

                var sortedColumns = requestModel.Columns.GetSortedColumns();
                var orderByString = string.Empty;
                foreach (var column in sortedColumns)
                {
                    string orderColumn;
                    switch (column.Data)
                    {
                        case "addOfferNumber":
                            orderColumn = "AddOfferNumber";
                            break;
                        case "textLabel":
                        case "textLabelPlain":
                            orderColumn = "TextLabel";
                            break;
                        case "stateName":
                            orderColumn = "StateName";
                            break;
                        case "clientName":
                            orderColumn = "ClientName";
                            break;
                        case "addDateMade":
                        case "addDateMadeFmt":
                            orderColumn = "AddDateMade";
                            break;
                        case "activeBadge":
                        case "Is_Active":
                            orderColumn = "Is_Active";
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
                var ttEdit = HttpUtility.HtmlAttributeEncode(Jobside.Offers_EditTooltip);
                var ttDetails = HttpUtility.HtmlAttributeEncode(Jobside.Offers_DetailsTooltip);
                var lblActive = HttpUtility.HtmlEncode(Jobside.State_Active);
                var lblInactive = HttpUtility.HtmlEncode(Jobside.State_Inactive);

                var data = rows.Select(p =>
                {
                    var namePlain = p.TextLabel ?? "";
                    var nameCell = HttpUtility.HtmlEncode(namePlain);

                    var numRaw = (p.AddOfferNumber ?? "").Trim();
                    var numForDisplay = string.IsNullOrEmpty(numRaw) ? Jobside.Details_NoValue : numRaw;
                    var numCell = OfferDisplayHelper.BuildOfferNumberCellHtml(numForDisplay, p.OfferStateAddColor);

                    var clientNamePlain = p.ClientName ?? "";
                    var clientCell = BuildJobsideOffersClientCellHtml(clientNamePlain, p.ClientPathLogo);

                    var activeBadge = p.Is_Active
                        ? "<span class=\"badge bg-label-success\">" + lblActive + "</span>"
                        : "<span class=\"badge bg-label-secondary\">" + lblInactive + "</span>";

                    var editBtn =
                        "<button type=\"button\" class=\"btn btn-warning btn-xs tandem-jobside-offer-edit-row\" " +
                        "data-offer-id=\"" + p.IdObject + "\" title=\"" + ttEdit + "\" aria-label=\"" + ttEdit + "\">" +
                        "<span class=\"fas fa-edit\" aria-hidden=\"true\"></span></button>";

                    var detailsHref = HttpUtility.HtmlAttributeEncode(
                        Url.Action("OfferDetails", "Jobside", new { id = p.IdObject }));
                    var detailsBtn =
                        "<a class=\"btn btn-info btn-xs\" href=\"" + detailsHref + "\" " +
                        "title=\"" + ttDetails + "\" aria-label=\"" + ttDetails + "\">" +
                        "<span class=\"fas fa-eye\" aria-hidden=\"true\"></span></a>";

                    var actions = "<div class=\"d-inline-flex align-items-center gap-2 flex-wrap justify-content-end\" role=\"group\">" +
                                  editBtn + detailsBtn + "</div>";

                    return new
                    {
                        IdObject = p.IdObject,
                        addOfferNumber = numCell,
                        addOfferNumberPlain = numRaw,
                        textLabel = nameCell,
                        textLabelPlain = namePlain,
                        clientCell,
                        clientNamePlain = clientNamePlain,
                        stateName = HttpUtility.HtmlEncode(p.StateName ?? ""),
                        addDateMade = p.AddDateMade,
                        addDateMadeFmt = HttpUtility.HtmlEncode(p.AddDateMade.ToString("g")),
                        Is_Active = p.Is_Active,
                        activeBadge,
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
        /// Partial de formulario crear/editar oferta (modal en <c>Details</c>).
        /// </summary>
        public ActionResult JobsideOfferForm(long jobsideId, long? id)
        {
            var jobsideRow = db.TSql_Jobside.FirstOrDefault(j => j.IdObject == jobsideId && !j.Is_Delete);
            if (jobsideRow == null)
            {
                return HttpNotFound();
            }

            if (id == null)
            {
                PopulateOfferStates(null);
                PopulateClients(jobsideRow.LinkClient_V2);
                var createModel = new JobsideOfferFormModel
                {
                    JobsideId = jobsideId,
                    Is_Active = true,
                    LinkClient_V2 = jobsideRow.LinkClient_V2 ?? 0
                };
                return PartialView("_JobsideOfferForm", createModel);
            }

            var offer = db.TSql_Offers.FirstOrDefault(o => o.IdObject == id.Value && !o.Is_Delete);
            if (offer == null)
            {
                return HttpNotFound();
            }
            if (offer.LinkJobside != jobsideId)
            {
                return new HttpStatusCodeResult(400);
            }

            PopulateOfferStates(offer.LinkOfferState);
            PopulateClients(offer.LinkClient_V2);
            var editModel = new JobsideOfferFormModel
            {
                IdObject = offer.IdObject,
                JobsideId = jobsideId,
                AddOfferNumber = offer.AddOfferNumber,
                TextLabel = offer.TextLabel,
                AddDescription = offer.AddDescription,
                LinkClient_V2 = offer.LinkClient_V2,
                LinkOfferState = offer.LinkOfferState,
                Is_Active = offer.Is_Active
            };
            return PartialView("_JobsideOfferForm", editModel);
        }

        /// <summary>
        /// Crear o actualizar oferta desde el modal del espacio de trabajo de obra.
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public JsonResult SaveJobsideOffer(JobsideOfferFormModel model)
        {
            if (model == null)
            {
                return Json(new { ok = false, message = Jobside.Offers_SaveFailed });
            }

            var jobsideRow = db.TSql_Jobside
                .Include("TSql_Branch")
                .Include("TSql_Branch.TSql_Company")
                .FirstOrDefault(j => j.IdObject == model.JobsideId && !j.Is_Delete);
            if (jobsideRow == null)
            {
                return Json(new { ok = false, message = Jobside.Err_JobsideNotFound });
            }

            ValidateJobsideOfferServer(model);
            if (!ModelState.IsValid)
            {
                var err = ModelState.Values.SelectMany(v => v.Errors).FirstOrDefault()?.ErrorMessage;
                return Json(new { ok = false, message = string.IsNullOrEmpty(err) ? Jobside.Offers_SaveFailed : err });
            }

            var name = (model.TextLabel ?? "").Trim();
            var desc = string.IsNullOrWhiteSpace(model.AddDescription)
                ? null
                : model.AddDescription.Trim();

            if (!jobsideRow.LinkClient_V2.HasValue && model.LinkClient_V2 <= 0)
            {
                return Json(new { ok = false, message = Jobside.Offers_Val_JobsideNeedsClient });
            }

            var resolvedClientId = model.LinkClient_V2 > 0
                ? model.LinkClient_V2
                : jobsideRow.LinkClient_V2.Value;

            if (!db.TSql_Client_V2.Any(c => c.IdObject == resolvedClientId && !c.Is_Delete && c.Is_Active))
            {
                return Json(new { ok = false, message = Jobside.Val_ClientInvalid });
            }

            if (!model.IdObject.HasValue || model.IdObject.Value <= 0)
            {
                if (jobsideRow.TSql_Branch == null)
                {
                    return Json(new { ok = false, message = Jobside.Offers_Val_BranchOrCompanyMissing });
                }

                if (string.IsNullOrWhiteSpace(jobsideRow.AddNJobside))
                {
                    return Json(new { ok = false, message = Jobside.Offers_Val_JobsideCodePending });
                }

                var coLetter = jobsideRow.TSql_Branch.TSql_Company != null
                    ? jobsideRow.TSql_Branch.TSql_Company.AddLetter
                    : null;
                var brLetter = jobsideRow.TSql_Branch.AddLetter;

                string addOfferNumber;
                using (var tran = db.Database.BeginTransaction(IsolationLevel.Serializable))
                {
                    try
                    {
                        addOfferNumber = OfferNumberHelper.AllocateNextOfferNumber(db, coLetter, brLetter, jobsideRow.AddNJobside);
                        var entity = new TSql_Offers
                        {
                            AddOfferNumber = addOfferNumber,
                            TextLabel = name,
                            AddDescription = desc,
                            LinkJobside = model.JobsideId,
                            LinkClient_V2 = resolvedClientId,
                            LinkOfferState = model.LinkOfferState
                        };
                        IntranetAuditHelper.SetAuditOnCreate(entity, User);
                        entity.Is_Active = model.Is_Active;
                        db.TSql_Offers.Add(entity);
                        db.SaveChanges();
                        tran.Commit();
                    }
                    catch
                    {
                        tran.Rollback();
                        throw;
                    }
                }

                return Json(new { ok = true, message = Jobside.Offers_SaveSuccess, addOfferNumber });
            }

            var existing = db.TSql_Offers.FirstOrDefault(o => o.IdObject == model.IdObject.Value && !o.Is_Delete);
            if (existing == null)
            {
                return Json(new { ok = false, message = Jobside.Offers_Val_NotFound });
            }
            if (existing.LinkJobside != model.JobsideId)
            {
                return Json(new { ok = false, message = Jobside.Offers_Val_WrongJobside });
            }

            /* AddOfferNumber: solo se asigna en alta; no reasignar ni confiar en POST. */
            existing.TextLabel = name;
            existing.AddDescription = desc;
            existing.LinkOfferState = model.LinkOfferState;
            existing.LinkClient_V2 = resolvedClientId;
            existing.Is_Active = model.Is_Active;
            IntranetAuditHelper.SetAuditOnUpdate(existing, User);
            db.SaveChanges();
            return Json(new { ok = true, message = Jobside.Offers_SaveSuccess, addOfferNumber = existing.AddOfferNumber });
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

        private bool TryGetOfferInJobside(long offerId, long jobsideId, out TSql_Offers offer)
        {
            offer = null;
            var o = db.TSql_Offers.FirstOrDefault(x => x.IdObject == offerId && !x.Is_Delete);
            if (o == null || o.LinkJobside != jobsideId)
            {
                return false;
            }

            if (!db.TSql_Jobside.Any(j => j.IdObject == jobsideId && !j.Is_Delete))
            {
                return false;
            }

            offer = o;
            return true;
        }

        /// <summary>
        /// Id de <see cref="TSql_DocumentType"/> para adjuntos de oferta: no borrado; por defecto preferimos <see cref="TSql_DocumentType.Is_Active"/>,
        /// pero si solo existe un tipo «Oferta» inactivo también se usa (coincide con catálogos creados a mano sin activar).
        /// Coincidencia por <see cref="TSql_DocumentType.TextCode"/> o <see cref="TSql_DocumentType.TextLabel"/> en cliente para evitar límites de traducción LINQ (Trim/ToUpper en servidor).
        /// Opcional: <c>appSettings:OfferDocumentTypeId</c> (BIGINT) fuerza el id si la fila existe y no está borrada.
        /// </summary>
        private long? TryGetOfferDocumentTypeId()
        {
            long forcedId;
            try
            {
                var rawId = ConfigurationManager.AppSettings["OfferDocumentTypeId"];
                if (!string.IsNullOrWhiteSpace(rawId) && long.TryParse(rawId.Trim(), NumberStyles.Integer, CultureInfo.InvariantCulture, out forcedId) && forcedId > 0)
                {
                    var forced = db.TSql_DocumentType.FirstOrDefault(t => t.IdObject == forcedId && !t.Is_Delete);
                    if (forced != null)
                    {
                        return forced.IdObject;
                    }
                }
            }
            catch (ConfigurationErrorsException)
            {
                // Seguir con resolución por código/etiqueta.
            }

            var token = GetOfferDocumentTypeMatchToken();
            if (string.IsNullOrWhiteSpace(token))
            {
                token = DefaultOfferDocumentTypeTextCode;
            }

            token = token.Trim();

            var activeId = TryResolveOfferDocumentTypeIdFromCatalog(requireActive: true, matchToken: token);
            if (activeId.HasValue)
            {
                return activeId;
            }

            return TryResolveOfferDocumentTypeIdFromCatalog(requireActive: false, matchToken: token);
        }

        /// <summary>Busca tipo de documento de oferta por código o etiqueta (orden: código exacto, etiqueta exacta, etiqueta contiene token).</summary>
        private long? TryResolveOfferDocumentTypeIdFromCatalog(bool requireActive, string matchToken)
        {
            if (string.IsNullOrWhiteSpace(matchToken))
            {
                return null;
            }

            var rows = db.TSql_DocumentType
                .Where(t => !t.Is_Delete && (!requireActive || t.Is_Active))
                .OrderBy(t => t.IdObject)
                .Select(t => new { t.IdObject, t.TextCode, t.TextLabel })
                .ToList();

            foreach (var t in rows)
            {
                var code = (t.TextCode ?? string.Empty).Trim();
                if (code.Length > 0 && string.Equals(code, matchToken, StringComparison.OrdinalIgnoreCase))
                {
                    return t.IdObject;
                }
            }

            foreach (var t in rows)
            {
                var lbl = (t.TextLabel ?? string.Empty).Trim();
                if (lbl.Length > 0 && string.Equals(lbl, matchToken, StringComparison.OrdinalIgnoreCase))
                {
                    return t.IdObject;
                }
            }

            foreach (var t in rows)
            {
                var lbl = (t.TextLabel ?? string.Empty).Trim();
                if (lbl.Length > 0 && lbl.IndexOf(matchToken, StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    return t.IdObject;
                }
            }

            return null;
        }

        /// <summary>Token para buscar <see cref="TSql_DocumentType"/> de ofertas; sobrescribible vía Web.config <c>OfferDocumentTypeTextCode</c>.</summary>
        private static string GetOfferDocumentTypeMatchToken()
        {
            try
            {
                var raw = ConfigurationManager.AppSettings["OfferDocumentTypeTextCode"];
                if (!string.IsNullOrWhiteSpace(raw))
                {
                    return raw.Trim();
                }
            }
            catch (ConfigurationErrorsException)
            {
                // Mantener default en entornos con app.config inválido.
            }

            return DefaultOfferDocumentTypeTextCode;
        }

        /// <summary>Prefijo de nombre de fichero para adjuntos de oferta.</summary>
        private static string SanitizeOfferDocumentFileStem(string addOfferNumber, string textLabel, long offerId)
        {
            var raw = !string.IsNullOrWhiteSpace(addOfferNumber)
                ? addOfferNumber.Trim()
                : null;
            if (string.IsNullOrEmpty(raw) && !string.IsNullOrWhiteSpace(textLabel))
            {
                raw = textLabel.Trim();
            }

            if (string.IsNullOrEmpty(raw))
            {
                raw = "Oferta_" + offerId.ToString(CultureInfo.InvariantCulture);
            }

            foreach (var c in Path.GetInvalidFileNameChars())
            {
                raw = raw.Replace(c, '_');
            }

            raw = raw.Trim(' ', '.');
            if (string.IsNullOrEmpty(raw))
            {
                raw = "Oferta_" + offerId.ToString(CultureInfo.InvariantCulture);
            }

            const int maxStem = 160;
            if (raw.Length > maxStem)
            {
                raw = raw.Substring(0, maxStem).TrimEnd('.', ' ', '_');
            }

            return raw;
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

        private void PopulateOfferStates(long? selectedId)
        {
            var states = db.TSql_OfferState
                .Where(s => !s.Is_Delete && s.Is_Active)
                .OrderBy(s => s.TextLabel)
                .Select(s => new { s.IdObject, s.TextLabel })
                .ToList();
            ViewBag.JobsideOfferStates = new SelectList(states, "IdObject", "TextLabel", selectedId);
        }

        private void ValidateJobsideOfferServer(JobsideOfferFormModel model)
        {
            if (model == null)
            {
                return;
            }

            ClearFieldErrors("TextLabel");
            if (string.IsNullOrWhiteSpace(model.TextLabel))
            {
                ModelState.AddModelError("TextLabel", Jobside.Offers_Val_NameRequired);
            }

            ClearFieldErrors("LinkOfferState");
            if (model.LinkOfferState <= 0 ||
                !db.TSql_OfferState.Any(s => s.IdObject == model.LinkOfferState && !s.Is_Delete && s.Is_Active))
            {
                ModelState.AddModelError("LinkOfferState", Jobside.Offers_Val_StateInvalid);
            }

            ClearFieldErrors("LinkClient_V2");
            if (model.LinkClient_V2 > 0)
            {
                if (!db.TSql_Client_V2.Any(c => c.IdObject == model.LinkClient_V2 && !c.Is_Delete && c.Is_Active))
                {
                    ModelState.AddModelError("LinkClient_V2", Jobside.Val_ClientInvalid);
                }
            }
            else
            {
                var js = db.TSql_Jobside.FirstOrDefault(j => j.IdObject == model.JobsideId && !j.Is_Delete);
                if (js != null && !js.LinkClient_V2.HasValue)
                {
                    ModelState.AddModelError("LinkClient_V2", Jobside.Offers_Val_JobsideNeedsClient);
                }
            }
        }

        private string RenderPartialViewToString(string viewName, object model)
        {
            using (var sw = new StringWriter())
            {
                var vr = ViewEngines.Engines.FindPartialView(ControllerContext, viewName);
                if (vr.View == null)
                {
                    throw new InvalidOperationException("Vista parcial no encontrada: " + viewName);
                }

                var viewData = new ViewDataDictionary { Model = model };
                var vc = new ViewContext(ControllerContext, vr.View, viewData, TempData, sw);
                vr.View.Render(vc, sw);
                return sw.ToString();
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
