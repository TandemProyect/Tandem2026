using DAL;
using DataTables.Mvc;
using Desing.Helpers;
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
    /// CRUD de extensiones de fichero (TSql_Extension). Catalogo simple usado
    /// por TSql_DocumentType via la tabla puente TSql_DocumentTypeExtension y,
    /// opcionalmente, por TSql_Document.LinkExtension.
    ///
    /// Patron Materio + DataTables estandar (rowActions, TextLabelPlain,
    /// exportOptsPlainVisible, colReorder fijo a la derecha) y delegacion de
    /// textos a Desing.Resources.Extension (.resx + DbBackedResourceManager)
    /// reutilizando Desing.Resources.Common para botones y mensajes genericos.
    ///
    /// Reglas aplicadas (sql-tsql-table-conventions.mdc + intranet-ui-forms.mdc):
    ///   - Filtro por defecto Is_Delete == false en todas las queries.
    ///   - Auditoria centralizada en IntranetAuditHelper (SetAuditOnCreate /
    ///     SetAuditOnUpdate / SetAuditOnDelete).
    ///   - Soft-delete: nunca DELETE fisico. Al borrar una extension se
    ///     hace tambien soft-delete de los enlaces TSql_DocumentTypeExtension
    ///     activos, igual que DocumentTypeController.
    ///   - TextLabel obligatorio y unico (case-insensitive) entre extensiones
    ///     no borradas.
    ///   - Borrado bloqueado si la extension esta enlazada a tipos de
    ///     documento activos (TSql_DocumentTypeExtension) o si esta en uso
    ///     por documentos (TSql_Document.LinkExtension).
    /// </summary>
    [Authorize]
    public class ExtensionController : BaseController
    {
        /// <summary>Tamano maximo por defecto: 10 MB.</summary>
        public const long DefaultMaxFileSizeBytes = 10L * 1024 * 1024;

        /// <summary>Minimo permitido: 1 byte. Pensado para descartar 0/negativos.</summary>
        public const long MinMaxFileSizeBytes = 1L;

        /// <summary>Maximo razonable: 2 GB (limite practico de IIS y de Int32 en upload).</summary>
        public const long MaxMaxFileSizeBytes = 2L * 1024 * 1024 * 1024;

        // ---------------------------------------------------------------------
        // INDEX + DataTable (patron Materio + applyListDefaults)
        // ---------------------------------------------------------------------
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult Details(long id)
        {
            var entity = LoadEntity(id);
            if (entity == null)
            {
                return HttpNotFound();
            }

            ViewBag.Audit = IntranetAuditHelper.BuildDisplay(
                db,
                entity.LinkMadeBy,
                entity.LinModifiedBy,
                entity.AddChangeBy,
                entity.AddDateMade,
                null,
                entity.Ntimeschanged);

            ViewBag.DocumentTypesAsignados = LoadAssignedDocumentTypes(entity.IdObject);
            ViewBag.MaxFileSizeDisplay = FormatBytes(entity.NumberMaxFileSizeBytes);
            ViewBag.ExtensionPathIco =
                ExtensionPathIcoQueries.GetPathIco(db.Database, entity.IdObject);

            return View(entity);
        }

        [OutputCache(Duration = 1)]
        public JsonResult ListExtensions([ModelBinder(typeof(DataTablesBinder))] IDataTablesRequest requestModel)
        {
            try
            {
                var query = db.TSql_Extension
                    .Where(e => !e.Is_Delete)
                    .Select(e => new
                    {
                        e.IdObject,
                        e.TextLabel,
                        e.Is_Active,
                        e.Is_Delete,
                        e.NumberMaxFileSizeBytes
                    });

                var totalCount = query.Count();

                if (!string.IsNullOrEmpty(requestModel.Search.Value))
                {
                    var value = requestModel.Search.Value.Trim();
                    query = query.Where(p => (p.TextLabel ?? "").Contains(value));
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
                        case "MaxFileSize":
                        case "NumberMaxFileSizeBytes":
                            orderColumn = "NumberMaxFileSizeBytes"; break;
                        case "IcoThumb":
                            orderColumn = "TextLabel"; break;
                        case "Is_Active":
                        case "activeBadge":
                            orderColumn = "Is_Active"; break;
                        default:
                            orderColumn = "TextLabel"; break;
                    }
                    orderByString += orderByString != string.Empty ? "," : "";
                    orderByString += orderColumn +
                        (column.SortDirection == Column.OrderDirection.Ascendant ? " asc" : " desc");
                }
                query = query.OrderBy(string.IsNullOrEmpty(orderByString) ? "TextLabel asc" : orderByString);
                query = query.ApplyDataTablesPaging(requestModel.Start, requestModel.Length);

                var rows = query.ToList();
                var ids = rows.ConvertAll(r => r.IdObject);

                // Iconos cacheados de una sola query.
                var pathIcoMap = ExtensionPathIcoQueries.LoadPathIcoMap(db.Database, ids);

                // Tipos de documento por extension (una sola query) para mostrar badges.
                var docTypesByExtension = LoadAssignedDocumentTypesBatch(ids);

                // Bloqueos de borrado: enlaces N:N activos o documentos que usan
                // la extension via TSql_Document.LinkExtension (no obligatoria).
                var idsWithDocTypes = db.TSql_DocumentTypeExtension
                    .Where(l => !l.Is_Delete && ids.Contains(l.LinkExtension))
                    .Select(l => l.LinkExtension)
                    .Distinct()
                    .ToList()
                    .ToHashSet();

                var idsWithDocuments = db.TSql_Document
                    .Where(d => !d.Is_Delete
                             && d.LinkExtension.HasValue
                             && ids.Contains(d.LinkExtension.Value))
                    .Select(d => d.LinkExtension.Value)
                    .Distinct()
                    .ToList()
                    .ToHashSet();

                var ttOpen = HttpUtility.HtmlAttributeEncode(Extension.List_LinkOpenTooltip);
                var ttEdit = HttpUtility.HtmlAttributeEncode(Extension.List_LinkEditTooltip);
                var ttDelete = HttpUtility.HtmlAttributeEncode(Extension.List_LinkDeleteTooltip);
                var ttDeleteDocTypes = HttpUtility.HtmlAttributeEncode(Extension.List_LinkDeleteLockedDocumentTypesTooltip);
                var ttDeleteDocuments = HttpUtility.HtmlAttributeEncode(Extension.List_LinkDeleteLockedDocumentsTooltip);
                var lblActive = HttpUtility.HtmlEncode(Extension.State_Active);
                var lblInactive = HttpUtility.HtmlEncode(Extension.State_Inactive);

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
                    else if (idsWithDocTypes.Contains(p.IdObject))
                    {
                        deleteBtn =
                            "<a title=\"" + ttDeleteDocTypes + "\" class=\"btn btn-secondary btn-xs disabled\" aria-disabled=\"true\" tabindex=\"-1\">" +
                            "<span class=\"fas fa-trash-alt\" aria-hidden=\"true\"></span></a>";
                    }
                    else
                    {
                        deleteBtn =
                            "<a title=\"" + ttDelete + "\" href=\"#\" onclick=\"DeleteExtension(" + p.IdObject +
                            "); return false;\" class=\"btn btn-danger btn-xs\"><span class=\"fas fa-trash-alt\" aria-hidden=\"true\"></span></a>";
                    }

                    var rowActions =
                        "<div class=\"d-inline-flex align-items-center gap-2\" role=\"group\">" +
                        editBtn + deleteBtn + "</div>";

                    return new
                    {
                        IdObject = p.IdObject,
                        IcoThumb = HtmlExtensionIcoThumb(pathIcoMap, p.IdObject),
                        TextLabel = nameCell,
                        TextLabelPlain = namePlain,
                        MaxFileSize = FormatBytes(p.NumberMaxFileSizeBytes),
                        DocumentTypes = BuildDocumentTypeBadges(
                            docTypesByExtension.ContainsKey(p.IdObject)
                                ? docTypesByExtension[p.IdObject]
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
            var entity = new TSql_Extension
            {
                Is_Active = true,
                NumberMaxFileSizeBytes = DefaultMaxFileSizeBytes
            };
            return View(entity);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(
            [Bind(Include =
                "IdObject,TextLabel,Is_Active,NumberMaxFileSizeBytes,IcoPath")]
            TSql_Extension entity,
            HttpPostedFileBase icoFile)
        {
            if (entity == null)
            {
                entity = new TSql_Extension { Is_Active = true, NumberMaxFileSizeBytes = DefaultMaxFileSizeBytes };
            }

            TrimIcoPathStored(entity);

            string pathIcoStored = entity.IcoPath;
            if (!TryPersistIcoUpload(ref pathIcoStored, icoFile, null, nameof(TSql_Extension.IcoPath)))
            {
                return View(entity);
            }

            ValidateExtension(entity, null);

            if (!ModelState.IsValid)
            {
                return View(entity);
            }

            var nueva = new TSql_Extension
            {
                TextLabel = (entity.TextLabel ?? string.Empty).Trim(),
                Is_Active = entity.Is_Active,
                NumberMaxFileSizeBytes = entity.NumberMaxFileSizeBytes
            };
            IntranetAuditHelper.SetAuditOnCreate(nueva, User);

            db.TSql_Extension.Add(nueva);
            db.SaveChanges();

            ExtensionPathIcoQueries.SetPathIco(db.Database, nueva.IdObject, pathIcoStored);

            TempData["ToastType"] = "Act";
            TempData["ToastTitle"] = Extension.ToastTitle_CreateExtension;
            TempData["ToastMessage"] = string.Format(Extension.ToastMessage_ExtensionCreated, nueva.TextLabel);
            return RedirectToAction("Index");
        }

        // ---------------------------------------------------------------------
        // EDIT
        // ---------------------------------------------------------------------
        public ActionResult Edit(long id)
        {
            var entity = LoadEntity(id);
            if (entity == null)
            {
                TempData["ToastType"] = "Error";
                TempData["ToastTitle"] = Extension.ToastTitle_EditExtension;
                TempData["ToastMessage"] = Extension.Err_ExtensionNotFound;
                return RedirectToAction("Index");
            }

            entity.IcoPath =
                ExtensionPathIcoQueries.GetPathIco(db.Database, entity.IdObject) ?? "";
            return View(entity);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(
            [Bind(Include =
                "IdObject,TextLabel,Is_Active,NumberMaxFileSizeBytes,IcoPath")]
            TSql_Extension entity,
            HttpPostedFileBase icoFile)
        {
            if (entity == null)
            {
                TempData["ToastType"] = "Error";
                TempData["ToastTitle"] = Extension.ToastTitle_EditExtension;
                TempData["ToastMessage"] = Extension.Err_ExtensionNotFound;
                return RedirectToAction("Index");
            }

            var actual = LoadEntity(entity.IdObject);
            if (actual == null)
            {
                TempData["ToastType"] = "Error";
                TempData["ToastTitle"] = Extension.ToastTitle_EditExtension;
                TempData["ToastMessage"] = Extension.Err_ExtensionNotFound;
                return RedirectToAction("Index");
            }

            TrimIcoPathStored(entity);

            string pathIcoStored = entity.IcoPath;
            if (!TryPersistIcoUpload(ref pathIcoStored, icoFile, entity.IdObject, nameof(TSql_Extension.IcoPath)))
            {
                actual.TextLabel = entity.TextLabel;
                actual.Is_Active = entity.Is_Active;
                actual.NumberMaxFileSizeBytes = entity.NumberMaxFileSizeBytes;
                actual.IcoPath = entity.IcoPath;
                return View(actual);
            }

            ValidateExtension(entity, entity.IdObject);

            if (!ModelState.IsValid)
            {
                // Repintamos el formulario con los valores enviados por el usuario
                // pero conservando la entidad original detras (IdObject correcto).
                actual.TextLabel = entity.TextLabel;
                actual.Is_Active = entity.Is_Active;
                actual.NumberMaxFileSizeBytes = entity.NumberMaxFileSizeBytes;
                actual.IcoPath = entity.IcoPath;
                return View(actual);
            }

            actual.TextLabel = (entity.TextLabel ?? string.Empty).Trim();
            actual.Is_Active = entity.Is_Active;
            actual.NumberMaxFileSizeBytes = entity.NumberMaxFileSizeBytes;
            IntranetAuditHelper.SetAuditOnUpdate(actual, User);

            db.SaveChanges();

            ExtensionPathIcoQueries.SetPathIco(db.Database, actual.IdObject, pathIcoStored);

            TempData["ToastType"] = "Act";
            TempData["ToastTitle"] = Extension.ToastTitle_EditExtension;
            TempData["ToastMessage"] = string.Format(Extension.ToastMessage_ExtensionUpdated, actual.TextLabel);
            return RedirectToAction("Index");
        }

        // ---------------------------------------------------------------------
        // DELETE (logico). Bloqueada si hay enlaces a DocumentType o documentos.
        // ---------------------------------------------------------------------
        [HttpPost]
        [ValidateAntiForgeryToken]
        public JsonResult DeleteExtension(long id)
        {
            var entity = LoadEntity(id);
            if (entity == null)
            {
                return Json(new { IsOk = false, Message = Extension.Err_ExtensionNotFound });
            }

            // Bloqueo por documentos que usan la extension (LinkExtension nullable).
            if (db.TSql_Document.Any(d => !d.Is_Delete
                                       && d.LinkExtension.HasValue
                                       && d.LinkExtension.Value == id))
            {
                return Json(new { IsOk = false, Message = Extension.Err_CannotDeleteHasDocuments });
            }

            // Bloqueo por enlaces N:N activos con tipos de documento (TSql_DocumentTypeExtension).
            if (db.TSql_DocumentTypeExtension.Any(l => !l.Is_Delete && l.LinkExtension == id))
            {
                return Json(new { IsOk = false, Message = Extension.Err_CannotDeleteHasDocumentTypes });
            }

            var nombre = entity.TextLabel ?? "";

            // Soft-delete de la extension.
            IntranetAuditHelper.SetAuditOnDelete(entity, User);

            // Soft-delete en cascada de los enlaces N:N "huerfanos" (deberian estar
            // ya borrados por la guarda anterior, pero por simetria con
            // DocumentTypeController limpiamos los que quedasen activos).
            var activeLinks = db.TSql_DocumentTypeExtension
                .Where(l => l.LinkExtension == id && !l.Is_Delete)
                .ToList();
            foreach (var link in activeLinks)
            {
                IntranetAuditHelper.SetAuditOnDelete(link, User);
            }

            db.SaveChanges();

            return Json(new
            {
                IsOk = true,
                Message = string.Format(Extension.ToastMessage_ExtensionDeleted, nombre)
            });
        }

        /* ===================================================================
           Validacion servidor (mensajes traducidos)
           =================================================================== */
        private void ValidateExtension(TSql_Extension model, long? excludeId)
        {
            ClearFieldErrors("TextLabel");
            if (model == null || string.IsNullOrWhiteSpace(model.TextLabel))
            {
                ModelState.AddModelError("TextLabel", Extension.Val_NameRequired);
                return;
            }

            var label = model.TextLabel.Trim();
            if (label.Length > 500)
            {
                ModelState.AddModelError("TextLabel", Extension.Val_NameTooLong);
                return;
            }

            // Comparacion case-insensitive via ToLower() para que LINQ-to-Entities
            // lo traduzca a SQL (LOWER); SQL Server suele ser case-insensitive
            // segun collation, pero garantizamos comportamiento consistente.
            var labelLower = label.ToLower();
            var existsQuery = db.TSql_Extension.Where(x =>
                !x.Is_Delete &&
                x.TextLabel.ToLower() == labelLower);

            if (excludeId.HasValue)
            {
                existsQuery = existsQuery.Where(x => x.IdObject != excludeId.Value);
            }

            if (existsQuery.Any())
            {
                ModelState.AddModelError("TextLabel", excludeId.HasValue
                    ? Extension.Val_DuplicateNameEdit
                    : Extension.Val_DuplicateNameCreate);
            }

            ClearFieldErrors("NumberMaxFileSizeBytes");
            if (model.NumberMaxFileSizeBytes < MinMaxFileSizeBytes)
            {
                ModelState.AddModelError("NumberMaxFileSizeBytes", Extension.Val_MaxFileSizeMin);
            }
            else if (model.NumberMaxFileSizeBytes > MaxMaxFileSizeBytes)
            {
                ModelState.AddModelError("NumberMaxFileSizeBytes",
                    string.Format(Extension.Val_MaxFileSizeMax, MaxMaxFileSizeBytes));
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
           Helpers privados
           =================================================================== */

        private static void TrimIcoPathStored(TSql_Extension entity)
        {
            if (entity.IcoPath == null)
            {
                entity.IcoPath = "";
            }
            else
            {
                entity.IcoPath = entity.IcoPath.Trim();
            }
        }

        private bool TryPersistIcoUpload(
            ref string pathIcoStored,
            HttpPostedFileBase icoFile,
            long? extensionIdForFileName,
            string modelStateKey)
        {
            string error;
            var prefix = extensionIdForFileName.HasValue
                ? "ext_" + extensionIdForFileName.Value.ToString(System.Globalization.CultureInfo.InvariantCulture)
                : "ext";
            var uploaded = IntranetFileHelper.TrySaveExtensionIcoFile(icoFile, prefix, out error);
            if (error != null)
            {
                // IntranetFileHelper devuelve mensajes en es duros; traducimos
                // a las claves Val_Ico* del modulo segun coincidencia textual.
                ModelState.AddModelError(modelStateKey, TranslateIcoUploadError(error));
                return false;
            }

            if (!string.IsNullOrEmpty(uploaded))
            {
                pathIcoStored = uploaded;
                ModelState.Remove(modelStateKey);
            }

            pathIcoStored = string.IsNullOrWhiteSpace(pathIcoStored) ? null : pathIcoStored.Trim();
            if (pathIcoStored != null && pathIcoStored.Length > 500)
            {
                ModelState.AddModelError(modelStateKey, Extension.Val_IcoPathTooLong);
                return false;
            }

            return true;
        }

        private static string TranslateIcoUploadError(string rawError)
        {
            if (string.IsNullOrEmpty(rawError)) return rawError;
            if (rawError.IndexOf("extens", StringComparison.OrdinalIgnoreCase) >= 0
                && rawError.IndexOf("tener", StringComparison.OrdinalIgnoreCase) >= 0)
            {
                return Extension.Val_IcoFileMissingExtension;
            }
            if (rawError.IndexOf("Formato", StringComparison.OrdinalIgnoreCase) >= 0
                || rawError.IndexOf("permit", StringComparison.OrdinalIgnoreCase) >= 0)
            {
                return Extension.Val_IcoFormatNotAllowed;
            }
            return rawError;
        }

        private string HtmlExtensionIcoThumb(Dictionary<long, string> pathIcoMap, long idObject)
        {
            if (pathIcoMap == null
                || !pathIcoMap.TryGetValue(idObject, out var path)
                || string.IsNullOrWhiteSpace(path))
            {
                return "<span class=\"text-muted\">" + HttpUtility.HtmlEncode(Extension.List_NoIcon) + "</span>";
            }

            var src = Url.Content(path.StartsWith("~") ? path : "~" + path);
            return "<img src=\"" + HttpUtility.HtmlAttributeEncode(src)
                   + "\" alt=\"\" style=\"max-height:28px;max-width:40px;object-fit:contain\" />";
        }

        private string BuildDocumentTypeBadges(List<TSql_DocumentType> docTypes)
        {
            if (docTypes == null || docTypes.Count == 0)
            {
                return "<span class=\"text-muted\">" + HttpUtility.HtmlEncode(Extension.List_NoDocumentTypes) + "</span>";
            }

            return string.Join(" ", docTypes.Select(d =>
            {
                var label = HttpUtility.HtmlEncode(d.TextLabel ?? "");
                var href = Url.Action("Details", "DocumentType", new { id = d.IdObject });
                return "<a class=\"badge bg-label-info me-1 mb-1\" href=\"" + href + "\">" + label + "</a>";
            }));
        }

        private TSql_Extension LoadEntity(long id)
        {
            return db.TSql_Extension.FirstOrDefault(x => x.IdObject == id && !x.Is_Delete);
        }

        /// <summary>
        /// Devuelve los tipos de documento que actualmente tienen un enlace
        /// activo (Is_Delete=false en TSql_DocumentTypeExtension) hacia esta
        /// extension. Solo informativo en la pantalla Details.
        /// </summary>
        private List<TSql_DocumentType> LoadAssignedDocumentTypes(long extensionId)
        {
            return (from l in db.TSql_DocumentTypeExtension
                    join d in db.TSql_DocumentType on l.LinkDocumentType equals d.IdObject
                    where !l.Is_Delete
                       && !d.Is_Delete
                       && l.LinkExtension == extensionId
                    orderby d.TextLabel
                    select d)
                   .Distinct()
                   .ToList();
        }

        /// <summary>
        /// Carga, en una sola query, los tipos de documento activos enlazados
        /// a cada extension de la pagina. Evita el N+1 en ListExtensions.
        /// </summary>
        private Dictionary<long, List<TSql_DocumentType>> LoadAssignedDocumentTypesBatch(IList<long> extensionIds)
        {
            var result = new Dictionary<long, List<TSql_DocumentType>>();
            if (extensionIds == null || extensionIds.Count == 0)
            {
                return result;
            }

            var pairs = (from l in db.TSql_DocumentTypeExtension
                         join d in db.TSql_DocumentType on l.LinkDocumentType equals d.IdObject
                         where !l.Is_Delete
                            && !d.Is_Delete
                            && extensionIds.Contains(l.LinkExtension)
                         orderby d.TextLabel
                         select new { l.LinkExtension, DocType = d }).ToList();

            foreach (var p in pairs)
            {
                if (!result.ContainsKey(p.LinkExtension))
                {
                    result[p.LinkExtension] = new List<TSql_DocumentType>();
                }
                result[p.LinkExtension].Add(p.DocType);
            }
            return result;
        }

        /// <summary>
        /// Convierte un numero de bytes a una cadena legible (KB / MB / GB).
        /// Usa potencias de 1024 (KiB/MiB/GiB), redondeo a 1 decimal cuando aporta
        /// (ej.: 1572864 -> "1.5 MB"; 10485760 -> "10 MB").
        /// </summary>
        private static string FormatBytes(long bytes)
        {
            const long KB = 1024L;
            const long MB = KB * 1024L;
            const long GB = MB * 1024L;

            if (bytes <= 0)
            {
                return "0 B";
            }

            if (bytes >= GB)
            {
                return FormatUnit(bytes, GB, "GB");
            }
            if (bytes >= MB)
            {
                return FormatUnit(bytes, MB, "MB");
            }
            if (bytes >= KB)
            {
                return FormatUnit(bytes, KB, "KB");
            }
            return bytes + " B";
        }

        private static string FormatUnit(long bytes, long unit, string suffix)
        {
            var value = bytes / (double)unit;
            // Mostrar 1 decimal solo si aporta informacion (no para enteros).
            return Math.Abs(value - Math.Round(value)) < 0.05
                ? string.Format(System.Globalization.CultureInfo.InvariantCulture, "{0:0} {1}", value, suffix)
                : string.Format(System.Globalization.CultureInfo.InvariantCulture, "{0:0.#} {1}", value, suffix);
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
