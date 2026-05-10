using DataTables.Mvc;
using Desing.Models;
using System;
using System.Data.Entity;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Web.Mvc;

namespace Desing.Controllers
{
    public class TSql_CompanyController : BaseController
    {
        public ActionResult Index()
        {

            return View();
        }

        public ActionResult Details()
        {

            return View();
        }


        [OutputCache(Duration = 1)]
        public JsonResult ListTSql_Company([ModelBinder(typeof(DataTablesBinder))] IDataTablesRequest requestModel)
        {
            try
            {
                IQueryable<CompanyModel> query = from company in db.TSql_Company
                                                 join country in db.TSql_Countrys on company.LinkCountry equals country.IdObject into countryGroup
                                                 from country in countryGroup.DefaultIfEmpty()
                                                 select new CompanyModel
                                                 {
                                                     SysObjectID = company.SysObjectID,
                                                     AddLeter = company.AddLeter,
                                                     TextLabel = company.TextLabel,
                                                     TextLogo = company.TextLogo,
                                                     TextDescription = company.TextDescription,
                                                     TextAddress_1 = company.TextAddress_1,
                                                     TextAddress_2 = company.TextAddress_2,
                                                     TextPostal_Code = (int)company.TextPostal_Code,
                                                     TextTown_1 = company.TextTown_1,
                                                     Country = country.TextLabel,
                                                     TextFlag = country.TextFlag
                                                 };

                var totalCount = query.Count();

                // Apply filters
                if (requestModel.Search.Value != String.Empty)
                {
                    var value = requestModel.Search.Value.Trim();
                    query = query.Where(p => p.AddLeter.Contains(value) ||
                                             p.TextLabel.Contains(value) ||
                                              p.TextDescription.Contains(value) ||
                                              p.TextAddress_1.Contains(value) ||
                                              p.TextAddress_2.Contains(value) ||
                                              p.TextPostal_Code.ToString().Contains(value) ||
                                              p.Country.Contains(value)

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
                        case "AddLeter":
                            orderColumn = "AddLeter";
                            break;
                        case "TextLabel":
                            orderColumn = "TextLabel";
                            break;
                        case "TextAddress_1":
                            orderColumn = "TextAddress_1";
                            break;
                        case "TextAddress_2":
                            orderColumn = "TextAddress_2";
                            break;
                        case "TextPostal_Code":
                            orderColumn = "TextPostal_Code";
                            break;
                        case "country":
                            orderColumn = "Country";
                            break;
                        default:
                            orderColumn = "TextLabel";
                            break;
                    }
                    orderByString += orderByString != String.Empty ? "," : "";
                    orderByString += (column.Data == "TextLabel" ? "TextLabel" : orderColumn) + (column.SortDirection == Column.OrderDirection.Ascendant ? " asc" : " desc");
                }
                query = query.OrderBy(orderByString == String.Empty ? "name asc" : orderByString);
                // Paging
                query = query.Skip(requestModel.Start).Take(requestModel.Length);

                bool allowDelete = true;
                var data = query.ToList().Select(p => new
                {
                    emptyColumn = "",
                    SysObjectID = p.SysObjectID,
                    TextLabel = "<a title='Abrir Empresa' href='" + Url.Content("~/TSql_Company/Details/" + p.SysObjectID) + "'>" + p.TextLabel + "</a>",
                    AddLeter = p.AddLeter,
                    TextLogo = p.TextLogo,
                    TextDescription = p.TextDescription,
                    TextAddress_1 = p.TextAddress_1,
                    TextAddress_2 = p.TextAddress_2,
                    TextPostal_Code = p.TextPostal_Code,
                    TextTown_1 = p.TextTown_1,
                    TextFlag = p.Country,
                    Country = p.Country,
                    allowDelete = allowDelete,
                    buttonEdit = "<a title='Tile' href='" + Url.Content("~/Employee/Edit/" + p.SysObjectID) + "' class=\"btn btn-default btn-xs\"><span class=\"fas fa-edit\" aria-hidden=\"true\"></span></a>",
                    buttonDelete = "<a title='" + "' href='" + Url.Content("~/Employee/Delete/" + p.SysObjectID) + "' class=\"btn btn-danger btn-xs\" data-modalpaging><span class=\"fas fa-trash-alt\" aria-hidden=\"true\"></span></a>"
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