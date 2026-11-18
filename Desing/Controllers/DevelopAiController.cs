using DataTables.Mvc;
using Desing.Helpers;
using Desing.Resources;
using System;
using System.Globalization;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Web;
using System.Web.Mvc;

namespace Desing.Controllers
{
    /// <summary>
    /// Develop › IA: catálogo de solo lectura de <c>TSql_AiCommand</c> y sus órdenes.
    /// Protegido por la función «DevelopAi» (rol «Develop»): clave de controlador del filtro global.
    /// </summary>
    [Authorize]
    public class DevelopAiController : BaseController
    {
        private const int PreviewLength = 80;

        public ActionResult Index(string language)
        {
            var languages = DevelopAiRepository.ListLanguages(db);
            ViewBag.TableMissing = !DevelopAiRepository.TableExists(db);
            ViewBag.Languages = languages;
            ViewBag.Language = DevelopAiRepository.ResolveLanguage(languages, language, CultureInfo.CurrentUICulture);
            return View();
        }

        [OutputCache(Duration = 1)]
        public JsonResult ListCommands([ModelBinder(typeof(DataTablesBinder))] IDataTablesRequest requestModel, string language)
        {
            try
            {
                var query = DevelopAiRepository.ListCommands(db, language).AsQueryable();
                var totalCount = query.Count();

                if (!string.IsNullOrEmpty(requestModel.Search.Value))
                {
                    var value = requestModel.Search.Value.Trim();
                    query = query.Where(p =>
                        (p.TextName ?? "").IndexOf(value, StringComparison.OrdinalIgnoreCase) >= 0
                        || (p.TextCode ?? "").IndexOf(value, StringComparison.OrdinalIgnoreCase) >= 0
                        || (p.TextGroup ?? "").IndexOf(value, StringComparison.OrdinalIgnoreCase) >= 0
                        || (p.TextOrders ?? "").IndexOf(value, StringComparison.OrdinalIgnoreCase) >= 0
                        || (p.TextMeaning ?? "").IndexOf(value, StringComparison.OrdinalIgnoreCase) >= 0);
                }

                var filteredCount = query.Count();
                var orderByString = string.Empty;
                foreach (var column in requestModel.Columns.GetSortedColumns())
                {
                    string orderColumn;
                    switch (column.Data)
                    {
                        case "TextName": orderColumn = "TextName"; break;
                        case "TextOrders": orderColumn = "TextOrders"; break;
                        case "TextMeaning": orderColumn = "TextMeaning"; break;
                        case "TextCode": orderColumn = "TextCode"; break;
                        default: orderColumn = "NumberOrder"; break;
                    }

                    orderByString += orderByString != string.Empty ? "," : "";
                    orderByString += orderColumn + (column.SortDirection == Column.OrderDirection.Ascendant ? " asc" : " desc");
                }

                query = query.OrderBy(string.IsNullOrEmpty(orderByString) ? "NumberOrder asc, TextName asc" : orderByString);
                query = query.ApplyDataTablesPaging(requestModel.Start, requestModel.Length);

                var pending = HttpUtility.HtmlEncode(DevelopAi.Badge_Pending);
                var data = query.ToList().Select(p =>
                {
                    var name = p.TextName ?? "";
                    var code = p.TextCode ?? "";
                    var group = p.TextGroup ?? "";
                    var orders = p.TextOrders ?? "";
                    var meaning = p.TextMeaning ?? "";
                    var nameHtml = "<span class=\"fw-semibold\">" + HttpUtility.HtmlEncode(name) + "</span>";
                    if (!p.IsAiImplemented)
                    {
                        nameHtml += " <span class=\"badge bg-label-warning\">" + pending + "</span>";
                    }

                    nameHtml += "<div class=\"text-muted small\">" + HttpUtility.HtmlEncode(code)
                        + (group.Length == 0 ? "" : " · " + HttpUtility.HtmlEncode(group)) + "</div>";

                    return new
                    {
                        p.IdObject,
                        TextName = nameHtml,
                        TextNamePlain = name,
                        TextCode = code,
                        TextGroup = group,
                        TextOrders = HttpUtility.HtmlEncode(Preview(orders)),
                        TextOrdersFull = orders,
                        TextMeaning = HttpUtility.HtmlEncode(Preview(meaning)),
                        TextMeaningFull = meaning,
                        TextHandler = p.TextHandler ?? "",
                        IsAiImplemented = p.IsAiImplemented
                    };
                }).ToList();

                return Json(DataTablesMvcJson.Create(requestModel.Draw, data, filteredCount, totalCount), JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(ex.Message);
            }
        }

        private static string Preview(string value)
        {
            if (string.IsNullOrEmpty(value) || value.Length <= PreviewLength)
            {
                return value ?? "";
            }

            return value.Substring(0, PreviewLength) + "…";
        }
    }
}
