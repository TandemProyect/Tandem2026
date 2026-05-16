using DAL;

using DataTables.Mvc;

using Desing.Helpers;

using Desing.Models;

using Microsoft.AspNet.Identity;

using System;

using System.Collections.Generic;

using System.Linq;

using System.Linq.Dynamic.Core;

using System.Web;

using System.Web.Mvc;



namespace Desing.Controllers

{

    [Authorize]

    public class DocumentTypeController : BaseController

    {

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



        public ActionResult Create()

        {

            var vm = new DocumentTypeFormViewModel

            {

                DocumentType = new TSql_DocumentType

                {

                    Is_Active = true

                },

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

                TextLabel = vm.DocumentType.TextLabel,

                TextCode = vm.DocumentType.TextCode,

                TextDescription = vm.DocumentType.TextDescription,

                Is_Active = vm.DocumentType.Is_Active

            };

            IntranetAuditHelper.SetAuditOnCreate(entity, User);



            db.TSql_DocumentType.Add(entity);

            db.SaveChanges();



            SyncExtensions(entity, vm.IdExtensionesSeleccionadas);



            TempData["ToastType"] = "Act";

            TempData["ToastTitle"] = "Tipo de documento";

            TempData["ToastMessage"] = "Tipo de documento creado correctamente.";

            return RedirectToAction("Index");

        }



        public ActionResult Edit(long id)

        {

            var entity = db.TSql_DocumentType.FirstOrDefault(x => x.IdObject == id && !x.Is_Delete);

            if (entity == null)

            {

                return HttpNotFound();

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

                return HttpNotFound();

            }

            if (vm.IdExtensionesSeleccionadas == null)

            {

                vm.IdExtensionesSeleccionadas = new List<long>();

            }



            var entity = db.TSql_DocumentType.FirstOrDefault(x => x.IdObject == vm.DocumentType.IdObject && !x.Is_Delete);

            if (entity == null)

            {

                return HttpNotFound();

            }



            ValidateDocumentType(vm.DocumentType, vm.DocumentType.IdObject);



            if (!ModelState.IsValid)

            {

                vm.ExtensionesDisponibles = LoadAvailableExtensions();

                vm.DocumentType = entity;

                PopulateExtensionPathIcoMap(vm);

                return View(vm);

            }



            entity.TextLabel = vm.DocumentType.TextLabel;

            entity.TextCode = vm.DocumentType.TextCode;

            entity.TextDescription = vm.DocumentType.TextDescription;

            entity.Is_Active = vm.DocumentType.Is_Active;

            IntranetAuditHelper.SetAuditOnUpdate(entity, User);



            db.SaveChanges();



            SyncExtensions(entity, vm.IdExtensionesSeleccionadas);



            TempData["ToastType"] = "Act";

            TempData["ToastTitle"] = "Tipo de documento";

            TempData["ToastMessage"] = "Tipo de documento actualizado correctamente.";

            return RedirectToAction("Index");

        }



        [OutputCache(Duration = 1)]

        public JsonResult ListDocumentTypes([ModelBinder(typeof(DataTablesBinder))] IDataTablesRequest requestModel)

        {

            try

            {

                var query = db.TSql_DocumentType

                    .Where(d => !d.Is_Delete)

                    .Select(d => new

                    {

                        d.IdObject,

                        d.TextLabel,

                        d.TextCode,

                        d.TextDescription,

                        d.Is_Active,

                        d.Is_Delete

                    });



                var totalCount = query.Count();



                if (!string.IsNullOrEmpty(requestModel.Search.Value))

                {

                    var value = requestModel.Search.Value.Trim();

                    query = query.Where(p => (p.TextLabel ?? "").Contains(value) ||

                                             (p.TextCode ?? "").Contains(value) ||

                                             (p.TextDescription ?? "").Contains(value));

                }



                var filteredCount = query.Count();



                var sortedColumns = requestModel.Columns.GetSortedColumns();

                var orderByString = string.Empty;

                foreach (var column in sortedColumns)

                {

                    var orderColumn = column.Data == "TextCode" ? "TextCode"

                        : column.Data == "TextDescription" ? "TextDescription"

                        : "TextLabel";

                    orderByString += orderByString != string.Empty ? "," : "";

                    orderByString += orderColumn + (column.SortDirection == Column.OrderDirection.Ascendant ? " asc" : " desc");

                }

                query = query.OrderBy(string.IsNullOrEmpty(orderByString) ? "TextLabel asc" : orderByString);

                query = query.ApplyDataTablesPaging(requestModel.Start, requestModel.Length);



                var rows = query.ToList();

                var ids = rows.Select(r => r.IdObject).ToList();



                // Pre-cargar las extensiones asignadas a esta página (1 query) para no hacer N+1.

                var extensionsByDocType = LoadAssignedExtensionsBatch(ids);



                var data = rows.Select(p => new

                {

                    IdObject = p.IdObject,

                    TextLabel = "<a href='" + Url.Action("Details", new { id = p.IdObject }) + "'>" + HttpUtility.HtmlEncode(p.TextLabel) + "</a>",

                    TextCode = p.TextCode ?? "",

                    TextDescription = p.TextDescription ?? "",

                    Is_Active = p.Is_Active,

                    Extensions =
                        BuildExtensionBadgesWithIcons(extensionsByDocType.ContainsKey(p.IdObject)
                            ? extensionsByDocType[p.IdObject]
                            : null),

                    activeBadge = p.Is_Active

                        ? "<span class=\"badge bg-label-success\">Activo</span>"

                        : "<span class=\"badge bg-label-secondary\">Inactivo</span>",

                    buttonEdit = "<a title='Editar' href='" + Url.Action("Edit", new { id = p.IdObject }) + "' class=\"btn btn-warning btn-xs\"><span class=\"fas fa-edit\"></span></a>",

                    buttonDelete = "<a title='Eliminar' onclick=\"deleteDocumentType(" + p.IdObject + ")\" class=\"btn btn-danger btn-xs\"><span class=\"fas fa-trash-alt\"></span></a>"

                }).ToList();



                return Json(DataTablesMvcJson.Create(requestModel.Draw, data, filteredCount, totalCount), JsonRequestBehavior.AllowGet);

            }

            catch (Exception ex)

            {

                return Json(ex.Message);

            }

        }



        [HttpPost]

        public JsonResult DeleteDocumentType(long id)

        {

            var entity = db.TSql_DocumentType.FirstOrDefault(x => x.IdObject == id && !x.Is_Delete);

            if (entity == null)

            {

                return Json(new { IsOk = false, Message = "Tipo de documento no encontrado." });

            }



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



            return Json(new { IsOk = true, Message = "Tipo de documento eliminado correctamente." });

        }



        /* ===================================================================

           Helpers privados

           =================================================================== */



        private void ValidateDocumentType(TSql_DocumentType model, long? excludeId)

        {

            if (model == null || string.IsNullOrWhiteSpace(model.TextLabel))

            {

                ModelState.AddModelError("DocumentType.TextLabel", "El nombre del tipo de documento es obligatorio.");

                return;

            }



            if (!string.IsNullOrWhiteSpace(model.TextCode) &&

                db.TSql_DocumentType.Any(x =>

                    !x.Is_Delete &&

                    x.TextCode == model.TextCode &&

                    (!excludeId.HasValue || x.IdObject != excludeId.Value)))

            {

                ModelState.AddModelError("DocumentType.TextCode", excludeId.HasValue

                    ? "Ya existe otro tipo de documento con ese código."

                    : "Ya existe un tipo de documento con ese código.");

            }

        }



        /// <summary>

        /// Sincroniza los enlaces TSql_DocumentTypeExtension del tipo dado con

        /// el conjunto de IDs marcados en el formulario:

        ///   - Soft-delete de los enlaces activos que ya no están seleccionados.

        ///   - Reactivación de enlaces previamente borrados que vuelven a estarlo.

        ///   - Inserción de los enlaces nuevos.

        /// </summary>

        private void SyncExtensions(TSql_DocumentType documentType, IList<long> selectedExtensionIds)

        {

            var seleccionados = (selectedExtensionIds ?? new List<long>())

                .Where(id => id > 0)

                .Distinct()

                .ToList();



            // Filtrar solo extensiones que existan, estén activas y no borradas.

            var validIds = db.TSql_Extension

                .Where(e => !e.Is_Delete && e.Is_Active && seleccionados.Contains(e.IdObject))

                .Select(e => e.IdObject)

                .ToList();



            // Snapshot completo (incluyendo borradas) para poder reactivar.

            var existentes = db.TSql_DocumentTypeExtension

                .Where(l => l.LinkDocumentType == documentType.IdObject)

                .ToList();



            // 1) Soft-delete de los activos que ya no estén marcados.

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



            // 3) Crear los enlaces nuevos (no existían en absoluto).

            //    Nota: TSql_DocumentTypeExtension NO tiene TextLabel (la etiqueta

            //    del enlace se compone en runtime con DocumentType.TextLabel +

            //    Extension.TextLabel) y las FK reales son LinkDocumentType /

            //    LinkExtension (no IdDocumentType / IdExtension).

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

            if (vm == null)

            {

                return;

            }

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

                return "<span class=\"text-muted\">—</span>";

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

    }

}

