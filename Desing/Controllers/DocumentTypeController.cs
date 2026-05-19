using DAL;
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
    /// CRUD del catalogo "Tipos de documento" (TSql_DocumentType). Sigue el
    /// patron Materio + DataTables estandar (rowActions, TextLabelPlain,
    /// exportOptsPlainVisible, colReorder fijo a la derecha) y delega los
    /// textos a Desing.Resources.DocumentType (.resx + DbBackedResourceManager)
    /// reutilizando Desing.Resources.Common para botones y mensajes genericos.
    ///
    /// Auditoria estandar via IntranetAuditHelper (LinkMadeBy / LinModifiedBy /
    /// AddDateMade / AddLastDateChange / Ntimeschanged) y borrado logico
    /// (Is_Delete = true) con bloqueo si el tipo tiene documentos asociados
    /// (TSql_Document.LinkDocumentType).
    ///
    /// Mantiene la sincronizacion N:N con TSql_DocumentTypeExtension
    /// (extensiones permitidas) preservando la logica original.
    /// </summary>
    [Authorize]
    public class DocumentTypeController : BaseController
    {
        // ---------------------------------------------------------------------
        // INDEX + DataTable (patron Materio + applyListDefaults)
        // ---------------------------------------------------------------------
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult Details(long id)
        {
            var entity = db.TSql_DocumentType.FirstOrDefault(x => x.IdObject == id && !x.Is_Delete);
            if (entity == null)
            {
                return HttpNotFound();
            }

            ViewBag.Audit = IntranetAuditHelper.BuildDisplay(
                db,
                entity.LinkMadeBy,
                entity.LinModifiedBy,
                entity.AddDateMade,
                entity.AddLastDateChange,
                entity.Ntimeschanged);

            var extensionesDoc = LoadAssignedExtensions(entity.IdObject);
            ViewBag.ExtensionesAsignadas = extensionesDoc;
            ViewBag.ExtensionPathIcoById =
                ExtensionPathIcoQueries.LoadPathIcoMap(db.Database, extensionesDoc.Select(e => e.IdObject));

            return View(entity);
        }

        [OutputCache(Duration = 1)]
        public JsonResult ListDocumentTypes([ModelBinder(typeof(DataTablesBinder))] IDataTablesRequest requestModel)
        {
            try
            {
                IQueryable<DocumentTypeListItem> query = db.TSql_DocumentType
                    .Where(d => !d.Is_Delete)
                    .Select(d => new DocumentTypeListItem
                    {
                        IdObject = d.IdObject,
                        TextLabel = d.TextLabel,
                        TextCode = d.TextCode,
                        TextDescription = d.TextDescription,
                        Is_Active = d.Is_Active,
                        Is_Delete = d.Is_Delete
                    });

                var totalCount = query.Count();

                if (!string.IsNullOrEmpty(requestModel.Search.Value))
                {
                    var value = requestModel.Search.Value.Trim();
                    query = query.Where(p => (p.TextLabel ?? "").Contains(value)
                                          || (p.TextCode ?? "").Contains(value)
                                          || (p.TextDescription ?? "").Contains(value));
                }

                var filteredCount = query.Count();

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
                        case "TextDescription": orderColumn = "TextDescription"; break;
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
                var ids = rows.Select(r => r.IdObject).ToList();

                // Pre-cargar las extensiones asignadas a esta pagina (1 query)
                // para no hacer N+1.
                var extensionsByDocType = LoadAssignedExtensionsBatch(ids);

                // Dependencias para bloquear el borrado: tipos con documentos
                // asociados (TSql_Document.LinkDocumentType, NOT NULL).
                var idsWithDocuments = db.TSql_Document
                    .Where(d => !d.Is_Delete && ids.Contains(d.LinkDocumentType))
                    .Select(d => d.LinkDocumentType)
                    .Distinct()
                    .ToList()
                    .ToHashSet();

                var ttOpen = HttpUtility.HtmlAttributeEncode(DocumentType.List_LinkOpenTooltip);
                var ttEdit = HttpUtility.HtmlAttributeEncode(DocumentType.List_LinkEditTooltip);
                var ttDelete = HttpUtility.HtmlAttributeEncode(DocumentType.List_LinkDeleteTooltip);
                var ttDeleteDocuments = HttpUtility.HtmlAttributeEncode(DocumentType.List_LinkDeleteLockedDocumentsTooltip);
                var lblActive = HttpUtility.HtmlEncode(DocumentType.State_Active);
                var lblInactive = HttpUtility.HtmlEncode(DocumentType.State_Inactive);

                var data = rows.Select(p =>
                {
                    var namePlain = p.TextLabel ?? "";
                    var nameCell =
                        "<a title=\"" + ttOpen + "\" href=\"" +
                        Url.Action("Details", new { id = p.IdObject }) + "\">" +
                        HttpUtility.HtmlEncode(namePlain) + "</a>";

                    var activeBadge = p.Is_Active
                        ? "<span class=\"badge bg-label-success\">" + lblActive + "</span>"
                        : "<span class=\"badge bg-label-secondary\">" + lblInactive + "</span>";

                    var editBtn =
                        "<a title=\"" + ttEdit + "\" href=\"" +
                        Url.Action("Edit", new { id = p.IdObject }) +
                        "\" class=\"btn btn-warning btn-xs\"><span class=\"fas fa-edit\" aria-hidden=\"true\"></span></a>";

                    string deleteBtn;
                    if (idsWithDocuments.Contains(p.IdObject))
                    {
                        deleteBtn =
                            "<a title=\"" + ttDeleteDocuments + "\" class=\"btn btn-secondary btn-xs disabled\" aria-disabled=\"true\" tabindex=\"-1\">" +
                            "<span class=\"fas fa-trash-alt\" aria-hidden=\"true\"></span></a>";
                    }
                    else
                    {
                        deleteBtn =
                            "<a title=\"" + ttDelete + "\" href=\"#\" onclick=\"DeleteDocumentType(" + p.IdObject +
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
                        TextCode = HttpUtility.HtmlEncode(p.TextCode ?? ""),
                        TextDescription = HttpUtility.HtmlEncode(p.TextDescription ?? ""),
                        Extensions = BuildExtensionBadgesWithIcons(
                            extensionsByDocType.ContainsKey(p.IdObject)
                                ? extensionsByDocType[p.IdObject]
                                : null),
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
            var vm = new DocumentTypeFormViewModel
            {
                DocumentType = new TSql_DocumentType { Is_Active = true },
                ExtensionesDisponibles = LoadAvailableExtensions(),
                IdExtensionesSeleccionadas = new List<long>()
            };
            PopulateExtensionPathIcoMap(vm);
            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(DocumentTypeFormViewModel vm)
        {
            if (vm == null)
            {
                vm = new DocumentTypeFormViewModel();
            }
            if (vm.DocumentType == null)
            {
                vm.DocumentType = new TSql_DocumentType { Is_Active = true };
            }
            if (vm.IdExtensionesSeleccionadas == null)
            {
                vm.IdExtensionesSeleccionadas = new List<long>();
            }

            ValidateDocumentType(vm.DocumentType, null);

            if (!ModelState.IsValid)
            {
                vm.ExtensionesDisponibles = LoadAvailableExtensions();
                PopulateExtensionPathIcoMap(vm);
                return View(vm);
            }

            var entity = new TSql_DocumentType
            {
                TextLabel = (vm.DocumentType.TextLabel ?? "").Trim(),
                TextCode = vm.DocumentType.TextCode,
                TextDescription = vm.DocumentType.TextDescription,
                Is_Active = vm.DocumentType.Is_Active
            };
            IntranetAuditHelper.SetAuditOnCreate(entity, User);

            db.TSql_DocumentType.Add(entity);
            db.SaveChanges();

            SyncExtensions(entity, vm.IdExtensionesSeleccionadas);

            TempData["ToastType"] = "Act";
            TempData["ToastTitle"] = DocumentType.ToastTitle_CreateDocumentType;
            TempData["ToastMessage"] = string.Format(DocumentType.ToastMessage_DocumentTypeCreated, entity.TextLabel);
            return RedirectToAction("Index");
        }

        // ---------------------------------------------------------------------
        // EDIT
        // ---------------------------------------------------------------------
        public ActionResult Edit(long id)
        {
            var entity = db.TSql_DocumentType.FirstOrDefault(x => x.IdObject == id && !x.Is_Delete);
            if (entity == null)
            {
                TempData["ToastType"] = "Error";
                TempData["ToastTitle"] = DocumentType.ToastTitle_EditDocumentType;
                TempData["ToastMessage"] = DocumentType.Err_DocumentTypeNotFound;
                return RedirectToAction("Index");
            }

            var vm = new DocumentTypeFormViewModel
            {
                DocumentType = entity,
                ExtensionesDisponibles = LoadAvailableExtensions(),
                IdExtensionesSeleccionadas = LoadAssignedExtensionIds(entity.IdObject)
            };
            PopulateExtensionPathIcoMap(vm);
            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(DocumentTypeFormViewModel vm)
        {
            if (vm == null || vm.DocumentType == null)
            {
                TempData["ToastType"] = "Error";
                TempData["ToastTitle"] = DocumentType.ToastTitle_EditDocumentType;
                TempData["ToastMessage"] = DocumentType.Err_DocumentTypeNotFound;
                return RedirectToAction("Index");
            }
            if (vm.IdExtensionesSeleccionadas == null)
            {
                vm.IdExtensionesSeleccionadas = new List<long>();
            }

            var entity = db.TSql_DocumentType.FirstOrDefault(x => x.IdObject == vm.DocumentType.IdObject && !x.Is_Delete);
            if (entity == null)
            {
                TempData["ToastType"] = "Error";
                TempData["ToastTitle"] = DocumentType.ToastTitle_EditDocumentType;
                TempData["ToastMessage"] = DocumentType.Err_DocumentTypeNotFound;
                return RedirectToAction("Index");
            }

            ValidateDocumentType(vm.DocumentType, vm.DocumentType.IdObject);

            if (!ModelState.IsValid)
            {
                vm.ExtensionesDisponibles = LoadAvailableExtensions();
                // Rehidratar el formulario con la entidad persistida + lo
                // enviado por el usuario para que el partial muestre los
                // valores que provocaron el error de validacion.
                entity.TextLabel = vm.DocumentType.TextLabel;
                entity.TextCode = vm.DocumentType.TextCode;
                entity.TextDescription = vm.DocumentType.TextDescription;
                entity.Is_Active = vm.DocumentType.Is_Active;
                vm.DocumentType = entity;
                PopulateExtensionPathIcoMap(vm);
                return View(vm);
            }

            entity.TextLabel = (vm.DocumentType.TextLabel ?? "").Trim();
            entity.TextCode = vm.DocumentType.TextCode;
            entity.TextDescription = vm.DocumentType.TextDescription;
            entity.Is_Active = vm.DocumentType.Is_Active;
            IntranetAuditHelper.SetAuditOnUpdate(entity, User);

            db.SaveChanges();

            SyncExtensions(entity, vm.IdExtensionesSeleccionadas);

            TempData["ToastType"] = "Act";
            TempData["ToastTitle"] = DocumentType.ToastTitle_EditDocumentType;
            TempData["ToastMessage"] = string.Format(DocumentType.ToastMessage_DocumentTypeUpdated, entity.TextLabel);
            return RedirectToAction("Index");
        }

        // ---------------------------------------------------------------------
        // DELETE (logico). Bloqueada si hay documentos asociados.
        // ---------------------------------------------------------------------
        [HttpPost]
        public JsonResult DeleteDocumentType(long id)
        {
            var entity = db.TSql_DocumentType.FirstOrDefault(x => x.IdObject == id && !x.Is_Delete);
            if (entity == null)
            {
                return Json(new { IsOk = false, Message = DocumentType.Err_DocumentTypeNotFound });
            }

            if (db.TSql_Document.Any(d => !d.Is_Delete && d.LinkDocumentType == id))
            {
                return Json(new { IsOk = false, Message = DocumentType.Err_CannotDeleteHasDocuments });
            }

            var nombre = entity.TextLabel ?? "";

            // Soft-delete del tipo de documento.
            IntranetAuditHelper.SetAuditOnDelete(entity, User);

            // Soft-delete en cascada de los enlaces N:N activos.
            var activeLinks = db.TSql_DocumentTypeExtension
                .Where(l => l.LinkDocumentType == id && !l.Is_Delete)
                .ToList();
            foreach (var link in activeLinks)
            {
                IntranetAuditHelper.SetAuditOnDelete(link, User);
            }

            db.SaveChanges();

            return Json(new
            {
                IsOk = true,
                Message = string.Format(DocumentType.ToastMessage_DocumentTypeDeleted, nombre)
            });
        }

        // ---------------------------------------------------------------------
        // Validacion servidor (mensajes traducidos)
        // ---------------------------------------------------------------------
        private void ValidateDocumentType(TSql_DocumentType model, long? excludeId)
        {
            if (model == null) return;

            ClearFieldErrors("DocumentType.TextLabel");
            if (string.IsNullOrWhiteSpace(model.TextLabel))
            {
                ModelState.AddModelError("DocumentType.TextLabel", DocumentType.Val_NameRequired);
            }

            ClearFieldErrors("DocumentType.TextCode");
            if (!string.IsNullOrWhiteSpace(model.TextCode))
            {
                var codeNorm = model.TextCode.Trim();
                bool duplicate = db.TSql_DocumentType.Any(x =>
                    !x.Is_Delete
                    && x.TextCode == codeNorm
                    && (!excludeId.HasValue || x.IdObject != excludeId.Value));
                if (duplicate)
                {
                    ModelState.AddModelError("DocumentType.TextCode",
                        excludeId.HasValue
                            ? DocumentType.Val_DuplicateCodeEdit
                            : DocumentType.Val_DuplicateCodeCreate);
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

        /* ===================================================================
           Helpers privados (extensiones permitidas N:N)
           =================================================================== */

        /// <summary>
        /// Sincroniza los enlaces TSql_DocumentTypeExtension del tipo dado con
        /// el conjunto de IDs marcados en el formulario:
        ///   - Soft-delete de los enlaces activos que ya no estan seleccionados.
        ///   - Reactivacion de enlaces previamente borrados que vuelven a estarlo.
        ///   - Insercion de los enlaces nuevos.
        /// </summary>
        private void SyncExtensions(TSql_DocumentType documentType, IList<long> selectedExtensionIds)
        {
            var seleccionados = (selectedExtensionIds ?? new List<long>())
                .Where(id => id > 0)
                .Distinct()
                .ToList();

            // Filtrar solo extensiones que existan, esten activas y no borradas.
            var validIds = db.TSql_Extension
                .Where(e => !e.Is_Delete && e.Is_Active && seleccionados.Contains(e.IdObject))
                .Select(e => e.IdObject)
                .ToList();

            // Snapshot completo (incluyendo borradas) para poder reactivar.
            var existentes = db.TSql_DocumentTypeExtension
                .Where(l => l.LinkDocumentType == documentType.IdObject)
                .ToList();

            // 1) Soft-delete de los activos que ya no esten marcados.
            foreach (var link in existentes.Where(l => !l.Is_Delete && !validIds.Contains(l.LinkExtension)))
            {
                IntranetAuditHelper.SetAuditOnDelete(link, User);
            }

            // 2) Reactivar enlaces previamente borrados que vuelven a marcarse.
            foreach (var link in existentes.Where(l => l.Is_Delete && validIds.Contains(l.LinkExtension)))
            {
                link.Is_Delete = false;
                link.Is_Active = true;
                IntranetAuditHelper.SetAuditOnUpdate(link, User);
            }

            // 3) Crear los enlaces nuevos (no existian en absoluto).
            var idsExistentes = existentes.Select(l => l.LinkExtension).ToList();
            foreach (var idExt in validIds.Where(id => !idsExistentes.Contains(id)))
            {
                var nuevo = new TSql_DocumentTypeExtension
                {
                    LinkDocumentType = documentType.IdObject,
                    LinkExtension = idExt,
                    Is_Active = true
                };
                IntranetAuditHelper.SetAuditOnCreate(nuevo, User);
                db.TSql_DocumentTypeExtension.Add(nuevo);
            }

            db.SaveChanges();
        }

        private List<TSql_Extension> LoadAvailableExtensions()
        {
            return db.TSql_Extension
                .Where(e => !e.Is_Delete && e.Is_Active)
                .OrderBy(e => e.TextLabel)
                .ToList();
        }

        private List<long> LoadAssignedExtensionIds(long documentTypeId)
        {
            return db.TSql_DocumentTypeExtension
                .Where(l => l.LinkDocumentType == documentTypeId && !l.Is_Delete)
                .Select(l => l.LinkExtension)
                .ToList();
        }

        private List<TSql_Extension> LoadAssignedExtensions(long documentTypeId)
        {
            var extensionIds = LoadAssignedExtensionIds(documentTypeId);
            if (extensionIds.Count == 0)
            {
                return new List<TSql_Extension>();
            }
            return db.TSql_Extension
                .Where(e => extensionIds.Contains(e.IdObject) && !e.Is_Delete)
                .OrderBy(e => e.TextLabel)
                .ToList();
        }

        private Dictionary<long, List<TSql_Extension>> LoadAssignedExtensionsBatch(IList<long> documentTypeIds)
        {
            var result = new Dictionary<long, List<TSql_Extension>>();
            if (documentTypeIds == null || documentTypeIds.Count == 0)
            {
                return result;
            }

            var pairs = (from l in db.TSql_DocumentTypeExtension
                         join e in db.TSql_Extension on l.LinkExtension equals e.IdObject
                         where !l.Is_Delete
                            && !e.Is_Delete
                            && documentTypeIds.Contains(l.LinkDocumentType)
                         orderby e.TextLabel
                         select new { l.LinkDocumentType, Extension = e }).ToList();

            foreach (var p in pairs)
            {
                if (!result.ContainsKey(p.LinkDocumentType))
                {
                    result[p.LinkDocumentType] = new List<TSql_Extension>();
                }
                result[p.LinkDocumentType].Add(p.Extension);
            }
            return result;
        }

        private void PopulateExtensionPathIcoMap(DocumentTypeFormViewModel vm)
        {
            if (vm == null) return;
            if (vm.ExtensionesDisponibles == null || vm.ExtensionesDisponibles.Count == 0)
            {
                vm.ExtensionPathIcoById = new Dictionary<long, string>();
                return;
            }
            vm.ExtensionPathIcoById = ExtensionPathIcoQueries.LoadPathIcoMap(db.Database,
                vm.ExtensionesDisponibles.Select(e => e.IdObject));
        }

        private string BuildExtensionBadgesWithIcons(List<TSql_Extension> extensions)
        {
            if (extensions == null || extensions.Count == 0)
            {
                return "<span class=\"text-muted\">" + HttpUtility.HtmlEncode(DocumentType.List_NoExtensions) + "</span>";
            }

            var map = ExtensionPathIcoQueries.LoadPathIcoMap(db.Database, extensions.Select(e => e.IdObject));

            return string.Join(" ", extensions.Select(e =>
            {
                var label = HttpUtility.HtmlEncode(e.TextLabel ?? "");
                var img = "";
                if (map != null && map.TryGetValue(e.IdObject, out var path) && !string.IsNullOrWhiteSpace(path))
                {
                    var src = Url.Content(path.StartsWith("~") ? path : "~" + path);
                    img = "<img src=\"" + HttpUtility.HtmlAttributeEncode(src) + "\" alt=\"\" style=\"height:14px;width:auto;vertical-align:middle;margin-right:4px;object-fit:contain\" />";
                }

                return "<span class=\"badge bg-label-info me-1 mb-1\">" + img + label + "</span>";
            }));
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
