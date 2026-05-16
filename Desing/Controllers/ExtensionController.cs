using DAL;
using DataTables.Mvc;
using Desing.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Web;
using System.Web.Mvc;

namespace Desing.Controllers
{
    /// <summary>
    /// CRUD de extensiones de fichero (TSql_Extension). Catálogo simple
    /// usado por TSql_DocumentType vía la tabla puente
    /// TSql_DocumentTypeExtension.
    ///
    /// Reglas aplicadas (sql-tsql-table-conventions.mdc + intranet-ui-forms.mdc):
    ///   - Filtro por defecto Is_Delete == false en todas las queries.
    ///   - Auditoría centralizada en IntranetAuditHelper (SetAuditOnCreate /
    ///     SetAuditOnUpdate / SetAuditOnDelete).
    ///   - Soft-delete; nunca DELETE físico.
    ///   - TextLabel obligatorio y único (case-insensitive) entre las
    ///     extensiones no borradas.
    /// </summary>
    [Authorize]
    public class ExtensionController : BaseController
    {
        /// <summary>Tamaño máximo por defecto: 10 MB.</summary>
        public const long DefaultMaxFileSizeBytes = 10L * 1024 * 1024;

        /// <summary>Mínimo permitido: 1 byte. Pensado para descartar 0/negativos.</summary>
        public const long MinMaxFileSizeBytes = 1L;

        /// <summary>Máximo razonable: 2 GB (límite práctico de IIS y de Int32 en upload).</summary>
        public const long MaxMaxFileSizeBytes = 2L * 1024 * 1024 * 1024;

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
            TempData["ToastTitle"] = "Extensión";
            TempData["ToastMessage"] = "Extensión creada correctamente.";
            return RedirectToAction("Index");
        }

        public ActionResult Edit(long id)
        {
            var entity = LoadEntity(id);
            if (entity == null)
            {
                return HttpNotFound();
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
                return HttpNotFound();
            }

            var actual = LoadEntity(entity.IdObject);
            if (actual == null)
            {
                return HttpNotFound();
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
                // pero conservando la entidad original detrás (IdObject correcto).
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
            TempData["ToastTitle"] = "Extensión";
            TempData["ToastMessage"] = "Extensión actualizada correctamente.";
            return RedirectToAction("Index");
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
                    var orderColumn = column.Data == "Is_Active" ? "Is_Active"
                        : column.Data == "MaxFileSize" ? "NumberMaxFileSizeBytes"
                        : column.Data == "IcoThumb" ? "TextLabel"
                        : "TextLabel";
                    orderByString += orderByString != string.Empty ? "," : "";
                    orderByString += orderColumn + (column.SortDirection == Column.OrderDirection.Ascendant ? " asc" : " desc");
                }
                query = query.OrderBy(string.IsNullOrEmpty(orderByString) ? "TextLabel asc" : orderByString);
                query = query.ApplyDataTablesPaging(requestModel.Start, requestModel.Length);

                var rows = query.ToList();
                var pathIcoMap = ExtensionPathIcoQueries.LoadPathIcoMap(db.Database, rows.ConvertAll(r => r.IdObject));

                var data = rows.Select(p => new
                {
                    IdObject = p.IdObject,
                    IcoThumb = HtmlExtensionIcoThumb(pathIcoMap, p.IdObject),
                    TextLabel = "<a href='" + Url.Action("Details", new { id = p.IdObject }) + "'>" + HttpUtility.HtmlEncode(p.TextLabel) + "</a>",
                    Is_Active = p.Is_Active,
                    MaxFileSize = FormatBytes(p.NumberMaxFileSizeBytes),
                    activeBadge = p.Is_Active
                        ? "<span class=\"badge bg-label-success\">Activo</span>"
                        : "<span class=\"badge bg-label-secondary\">Inactivo</span>",
                    buttonEdit = "<a title='Editar' href='" + Url.Action("Edit", new { id = p.IdObject }) + "' class=\"btn btn-warning btn-xs\"><span class=\"fas fa-edit\"></span></a>",
                    buttonDelete = "<a title='Eliminar' onclick=\"deleteExtension(" + p.IdObject + ")\" class=\"btn btn-danger btn-xs\"><span class=\"fas fa-trash-alt\"></span></a>"
                }).ToList();

                return Json(DataTablesMvcJson.Create(requestModel.Draw, data, filteredCount, totalCount), JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(ex.Message);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public JsonResult DeleteExtension(long id)
        {
            var entity = LoadEntity(id);
            if (entity == null)
            {
                return Json(new { IsOk = false, Message = "Extensión no encontrada." });
            }

            // Soft-delete de la extensión.
            //
            // NOTA: NO se cascada el borrado a TSql_DocumentTypeExtension.
            // Los enlaces N:N siguen vivos; los Index de DocumentType filtran
            // por Is_Delete del propio enlace (no por el de la extensión),
            // así que un DocumentType puede continuar mostrando esta extensión
            // hasta que el usuario edite el tipo y desmarque el enlace, o
            // hasta que se decida cascada manualmente.
            IntranetAuditHelper.SetAuditOnDelete(entity, User);
            db.SaveChanges();

            return Json(new { IsOk = true, Message = "Extensión eliminada correctamente." });
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
                ModelState.AddModelError(modelStateKey, error);
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
                ModelState.AddModelError(modelStateKey,
                    "La ruta virtual del icono no puede superar los 500 caracteres.");
                return false;
            }

            return true;
        }

        private string HtmlExtensionIcoThumb(Dictionary<long, string> pathIcoMap, long idObject)
        {
            if (pathIcoMap == null
                || !pathIcoMap.TryGetValue(idObject, out var path)
                || string.IsNullOrWhiteSpace(path))
            {
                return "<span class=\"text-muted\">—</span>";
            }

            var src = Url.Content(path.StartsWith("~") ? path : "~" + path);
            return "<img src=\"" + HttpUtility.HtmlAttributeEncode(src)
                   + "\" alt=\"\" style=\"max-height:28px;max-width:40px;object-fit:contain\" />";
        }

        private TSql_Extension LoadEntity(long id)
        {
            return db.TSql_Extension.FirstOrDefault(x => x.IdObject == id && !x.Is_Delete);
        }

        private void ValidateExtension(TSql_Extension model, long? excludeId)
        {
            if (model == null || string.IsNullOrWhiteSpace(model.TextLabel))
            {
                ModelState.AddModelError("TextLabel", "El nombre de la extensión es obligatorio.");
                return;
            }

            var label = model.TextLabel.Trim();
            if (label.Length > 500)
            {
                ModelState.AddModelError("TextLabel", "El nombre no puede superar los 500 caracteres.");
                return;
            }

            // Comparación case-insensitive vía ToLower() para que LINQ-to-Entities
            // lo traduzca a SQL (LOWER); SQL Server suele ser case-insensitive
            // según collation, pero garantizamos comportamiento consistente.
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
                    ? "Ya existe otra extensión con ese nombre."
                    : "Ya existe una extensión con ese nombre.");
            }

            if (model.NumberMaxFileSizeBytes < MinMaxFileSizeBytes)
            {
                ModelState.AddModelError("NumberMaxFileSizeBytes", "El tamaño máximo debe ser mayor que 0.");
            }
            else if (model.NumberMaxFileSizeBytes > MaxMaxFileSizeBytes)
            {
                ModelState.AddModelError("NumberMaxFileSizeBytes",
                    string.Format("El tamaño máximo no puede superar {0} bytes (2 GB).", MaxMaxFileSizeBytes));
            }
        }

        /// <summary>
        /// Convierte un número de bytes a una cadena legible (KB / MB / GB).
        /// Usa potencias de 1024 (KiB/MiB/GiB), redondeo a 1 decimal cuando aporta
        /// (ej.: 1572864 → "1.5 MB"; 10485760 → "10 MB").
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
            // Mostrar 1 decimal solo si aporta información (no para enteros).
            return Math.Abs(value - Math.Round(value)) < 0.05
                ? string.Format(System.Globalization.CultureInfo.InvariantCulture, "{0:0} {1}", value, suffix)
                : string.Format(System.Globalization.CultureInfo.InvariantCulture, "{0:0.#} {1}", value, suffix);
        }

        /// <summary>
        /// Devuelve los tipos de documento que actualmente tienen un enlace
        /// activo (Is_Delete=false en TSql_DocumentTypeExtension) hacia esta
        /// extensión. Solo informativo en la pantalla Details.
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
    }
}
