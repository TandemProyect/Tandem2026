using DataTables.Mvc;
using Desing.Helpers;
using Desing.Resources;
using System;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Web;
using System.Web.Mvc;

namespace Desing.Controllers
{
    /// <summary>
    /// Develop › Agentes: lista de solo lectura de <c>TSql_DevelodAgentDocument</c>.
    /// Protegido por la función «DevelopAgent» (rol «Develop»): clave de controlador del filtro global.
    /// </summary>
    [Authorize]
    public class DevelopAgentController : BaseController
    {
        private const int DescriptionPreviewLength = 20;

        public ActionResult Index()
        {
            ViewBag.TableMissing = !DevelopAgentRepository.TableExists(db);
            return View();
        }

        [OutputCache(Duration = 1)]
        public JsonResult ListDocuments([ModelBinder(typeof(DataTablesBinder))] IDataTablesRequest requestModel)
        {
            try
            {
                var query = DevelopAgentRepository.ListDocuments(db).AsQueryable();
                var totalCount = query.Count();

                if (!string.IsNullOrEmpty(requestModel.Search.Value))
                {
                    var value = requestModel.Search.Value.Trim();
                    query = query.Where(p =>
                        (p.TextLabel ?? "").IndexOf(value, StringComparison.OrdinalIgnoreCase) >= 0
                        || (p.TextCommand ?? "").IndexOf(value, StringComparison.OrdinalIgnoreCase) >= 0
                        || (p.TextDescription ?? "").IndexOf(value, StringComparison.OrdinalIgnoreCase) >= 0);
                }

                var filteredCount = query.Count();

                var orderByString = string.Empty;
                foreach (var column in requestModel.Columns.GetSortedColumns())
                {
                    string orderColumn;
                    switch (column.Data)
                    {
                        case "TextLabel": orderColumn = "TextLabel"; break;
                        case "TextCommand": orderColumn = "TextCommand"; break;
                        case "TextDescription": orderColumn = "TextDescription"; break;
                        default: orderColumn = "NumberOrder"; break;
                    }
                    orderByString += orderByString != string.Empty ? "," : "";
                    orderByString += orderColumn + (column.SortDirection == Column.OrderDirection.Ascendant ? " asc" : " desc");
                }

                query = query.OrderBy(string.IsNullOrEmpty(orderByString) ? "NumberOrder asc, TextLabel asc" : orderByString);
                query = query.ApplyDataTablesPaging(requestModel.Start, requestModel.Length);

                var ttOpen = HttpUtility.HtmlAttributeEncode(DevelopAgent.List_OpenTooltip);

                var data = query.ToList().Select(p =>
                {
                    var label = p.TextLabel ?? "";
                    var command = p.TextCommand ?? "";
                    var description = p.TextDescription ?? "";
                    var preview = description.Length > DescriptionPreviewLength
                        ? description.Substring(0, DescriptionPreviewLength) + "…"
                        : description;
                    return new
                    {
                        IdObject = p.IdObject,
                        TextLabel = "<a href=\"#\" class=\"js-agent-doc-open\" title=\"" + ttOpen + "\">" + HttpUtility.HtmlEncode(label) + "</a>",
                        TextLabelPlain = label,
                        TextCommand = "<code>" + HttpUtility.HtmlEncode(command) + "</code>",
                        TextCommandPlain = command,
                        TextDescription = HttpUtility.HtmlEncode(preview),
                        TextDescriptionFull = description,
                        TextPath = p.TextPath ?? ""
                    };
                }).ToList();

                return Json(DataTablesMvcJson.Create(requestModel.Draw, data, filteredCount, totalCount), JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(ex.Message);
            }
        }
    }
}
