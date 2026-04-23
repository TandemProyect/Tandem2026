using DataTables.Mvc;
using Desing.Models;
using System;
using System.Data.Entity;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Web.Mvc;

namespace Desing.Controllers
{
    public class MasterArticlesController : BaseController
    {
        public ActionResult Index()
        {
            return View();
        }
        [OutputCache(Duration = 1)]
        public JsonResult ListMasterArticles([ModelBinder(typeof(DataTablesBinder))] IDataTablesRequest requestModel)
        {
            try
            {
                IQueryable<ListMasterArticle> query = from masterArticles in db.Tsql_Master_Articles
                                                      join system in db.TSql_System on masterArticles.LinkSystem equals system.IdObject
                                                      join company in db.TSql_Company on system.LinkCompany equals company.SysObjectID
                                                      select new ListMasterArticle
                                                      {
                                                          IdObject = masterArticles.IdObject,
                                                          CompanyTextLabel = company.TextLabel,
                                                          System_TextLabel = system.TextLabel,
                                                          TextCode = masterArticles.TextCode,
                                                          TextLabel = masterArticles.TextLabel,
                                                          NumberHigh = masterArticles.NumberHigh,
                                                          NumberWidth = masterArticles.NumberWidth,
                                                          NumberLong = masterArticles.NumberLong,
                                                          NumberWeight = masterArticles.NumberWeight,
                                                          NumberMts2 = masterArticles.NumberMts2,
                                                          NumberMts3 = masterArticles.NumberMts3,
                                                          TextBlockNumber = masterArticles.TextBlockNumber,
                                                          TextStlNumber = masterArticles.TextStlNumber,
                                                          TextColor1 = masterArticles.TextColor1,
                                                          TextColor2 = masterArticles.TextColor2,
                                                          AddChangeBy = masterArticles.AddLastDateChange,
                                                          AddIsActive = masterArticles.AddIsActive,
                                                      };

                var totalCount = query.Count();

                // Apply filters
                if (requestModel.Search.Value != String.Empty)
                {
                    var value = requestModel.Search.Value.Trim();
                    query = query.Where(p => p.CompanyTextLabel.Contains(value) ||
                                             p.System_TextLabel.Contains(value) ||
                                             p.TextCode.Contains(value) ||
                                             p.TextLabel.Contains(value) ||
                                             p.NumberHigh.ToString().Contains(value) ||
                                             p.NumberWidth.ToString().Contains(value) ||
                                             p.NumberLong.ToString().Contains(value) ||
                                             p.NumberWeight.ToString().Contains(value) ||
                                             p.NumberMts2.ToString().Contains(value) ||
                                             p.NumberMts3.ToString().Contains(value) ||
                                             p.TextBlockNumber.Contains(value) ||
                                             p.TextStlNumber.Contains(value) ||
                                             p.TextColor1.Contains(value) ||
                                             p.TextColor2.Contains(value) ||
                                             p.AddChangeBy.ToString().Contains(value) ||
                                             p.AddIsActive.ToString().Contains(value)

                    );
                }

                var filteredCount = query.Count();

                // Sort
                var sortedColumns = requestModel.Columns.GetSortedColumns();
                var orderByString = String.Empty;
                string orderColumn = "";
                foreach (var column in sortedColumns)
                {
                    switch (column.Data)
                    {
                        case "CompanyTextLabel":
                            orderColumn = "CompanyTextLabel";
                            break;
                        case "System_TextLabel":
                            orderColumn = "System_TextLabel";
                            break;
                        case "TextCode":
                            orderColumn = "TextCode";
                            break;
                        case "TextLabel":
                            orderColumn = "TextLabel";
                            break;
                        case "NumberHigh":
                            orderColumn = "NumberHigh";
                            break;
                        case "NumberWidth":
                            orderColumn = "NumberWidth";
                            break;
                        case "NumberLong":
                            orderColumn = "NumberLong";
                            break;
                        case "NumberWeight":
                            orderColumn = "NumberWeight";
                            break;
                        case "NumberMts2":
                            orderColumn = "NumberMts2";
                            break;
                        case "NumberMts3":
                            orderColumn = "NumberMts3";
                            break;
                        case "TextBlockNumber":
                            orderColumn = "TextBlockNumber";
                            break;
                        case "TextStlNumber":
                            orderColumn = "TextStlNumber";
                            break;
                        case "TextColor1":
                            orderColumn = "TextColor1";
                            break;
                        case "TextColor2":
                            orderColumn = "TextColor2";
                            break;
                        case "AddChangeBy":
                            orderColumn = "AddChangeBy";
                            break;
                        case "AddIsActive":
                            orderColumn = "AddIsActive";
                            break;
                        default:
                            orderColumn = "CompanyTextLabel";
                            break;
                    }
                    orderByString += orderByString != String.Empty ? "," : "";
                    orderByString += (column.Data == "AttName" ? "AttName" : orderColumn) + (column.SortDirection == Column.OrderDirection.Ascendant ? " asc" : " desc");
                }
                query = query.OrderBy(orderByString == String.Empty ? "name asc" : orderByString);
                // Paging
                query = query.Skip(requestModel.Start).Take(requestModel.Length);

                // Rights
                bool allowEdit = true;
                bool allowDelete = true;
                var data = query.ToList().Select(p => new
                {
                    emptyColumn = "",

                    SysObjectID = p.IdObject,
                    CompanyTextLabel = p.CompanyTextLabel,
                    System_TextLabel = p.System_TextLabel,
                    TextCode = p.TextCode,
                    TextLabel = p.TextLabel,
                    NumberHigh = p.NumberHigh,
                    NumberWidth = p.NumberWidth,
                    NumberLong = p.NumberLong,
                    NumberWeight = p.NumberWeight,
                    NumberMts2 = p.NumberMts2,
                    NumberMts3 = p.NumberMts3,
                    TextBlockNumber = p.TextBlockNumber,
                    TextStlNumber = p.TextStlNumber,
                    TextColor1 = p.TextColor1,
                    TextColor2 = p.TextColor2,
                    AddChangeBy = p.AddChangeBy,
                    AddIsActive = p.AddIsActive,
                    allowEdit = allowEdit,
                    allowDelete = allowDelete,
                    buttonEdit = "<a title='Tile' href='" + Url.Content("~/Employee/Edit/" + p.IdObject) + "' class=\"btn btn-default btn-xs\"><span class=\"fas fa-edit\" aria-hidden=\"true\"></span></a>",
                    buttonDelete = "<a title='" + "' href='" + Url.Content("~/Employee/Delete/" + p.IdObject) + "' class=\"btn btn-danger btn-xs\" data-modalpaging><span class=\"fas fa-trash-alt\" aria-hidden=\"true\"></span></a>"
                    //buttonDelete = "<a title='" + Language.Employee.DeleteEmployeeTitle + "' href='" + Url.Content("~/Employee/Delete/" + p.SysObjectID) + "' class=\"btn btn-danger btn-xs\" data-modalpaging><span class=\"glyphicon glyphicon-trash\" aria-hidden=\"true\"></span></a>"
                }).ToList();
                return Json(new DataTablesResponse(requestModel.Draw, data, filteredCount, totalCount), JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(ex.Message);
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